// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in THIRD-PARTY-NOTICES.md or at https://developers.google.com/open-source/licenses/bsd.

namespace DotJpegli.Internal;

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

/// <summary>Spatial adaptive quantization, with storage bounded to one MCU neighborhood.</summary>
internal sealed class AdaptiveQuantization : IDisposable
{
    // Derived from google/jpegli lib/jpegli/adaptive_quantization.cc at
    // 031a0077f5799a6041004267fc12b956c1f52a20. Unlike upstream's rolling
    // image buffers, each invocation uses a bounded luma neighborhood.
    // Consecutive immutable plane rows retain their shared luma and pre-erosion cells.
    private const float InputScaling = 1f / 255f;
    private const float InvLog2E = 0.6931471805599453f;
    private const float SgMul = 226.0480446705883f;
    private const float SgRetMul = 1f / 73.377132366608819f * 18.6580932135f * InvLog2E;
    private const float Epsilon = 1e-2f;
    private const float NumOffset = Epsilon / InputScaling / InputScaling;
    private const float NumMul = SgRetMul * 3 * SgMul;
    private const float VOffset = ((7.14672470003f * InvLog2E) + Epsilon) / InputScaling;
    private const float DenMul = InvLog2E * SgMul * InputScaling * InputScaling;
    private const int MaximumPooledSamples = 1024 * 1024 / sizeof(float);

    private readonly int width;
    private readonly int height;
    private readonly int verticalSampling;
    private readonly int paddedWidth;
    private readonly int paddedHeight;
    private readonly int lumaStride;
    private readonly int preStride;
    private readonly ArrayPool<float>? scratchPool;
    private readonly bool useSelectionNetwork;
    private readonly bool useAqReciprocal;
    private readonly AqRationalDiagnostics? aqRationalDiagnostics;
    private float[] luma = [];
    private float[] preErosion = [];
    private float[] fuzzy = [];
    private float[] strengths = [];
    private bool disposed;
    private IDisposable? previousStorage;
    private int previousTop;
    private int previousStride;
    private bool previousUseVector;

    internal AdaptiveQuantization(int width, int height, int verticalSampling, ArrayPool<float>? scratchPool = null,
        KernelPreference fuzzyErosionKernel = KernelPreference.Automatic,
        KernelPreference aqRationalKernel = KernelPreference.Automatic,
        AqRationalDiagnostics? aqRationalDiagnostics = null)
    {
        Guard.ThrowIfNegativeOrZero(width);
        Guard.ThrowIfNegativeOrZero(height);
        if (verticalSampling is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(verticalSampling));
        }

        this.width = width;
        this.height = height;
        this.verticalSampling = verticalSampling;
        this.useSelectionNetwork = KernelPolicy.UseSelectionNetwork(fuzzyErosionKernel, width);
        this.useAqReciprocal = aqRationalKernel switch
        {
            KernelPreference.Automatic or KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(aqRationalKernel))
        };
        this.aqRationalDiagnostics = aqRationalDiagnostics;
        this.BlocksWide = checked((width + 7) / 8);
        this.paddedWidth = checked(this.BlocksWide * 8);
        int mcuHeight = verticalSampling * 8;
        this.paddedHeight = checked((height + mcuHeight - 1) / mcuHeight * mcuHeight);
        this.lumaStride = checked(this.paddedWidth + 2);
        this.preStride = checked(this.BlocksWide * 2);
        int lumaLength = checked(this.lumaStride * (mcuHeight + 10));
        int preErosionLength = checked(this.preStride * ((verticalSampling * 2) + 2));
        int fuzzyLength = checked(this.preStride * 2);
        // Typical widths reuse bounded shared-pool buckets. Exceptionally wide
        // images retain the original ordinary arrays instead of growing the pool.
        this.scratchPool = lumaLength <= MaximumPooledSamples ? scratchPool : null;
        try
        {
            this.luma = this.Allocate(lumaLength);
            this.preErosion = this.Allocate(preErosionLength);
            this.fuzzy = this.Allocate(fuzzyLength);
        }
        catch
        {
            this.Dispose();
            throw;
        }
    }

    internal int BlocksWide { get; }

    // Analysis and its ordered consumers run synchronously within one encode.
    // Reuse the same MCU output storage across histogram and emission passes.
    internal Span<float> Strengths
    {
        get
        {
            Guard.ThrowIfDisposed(this.disposed, this);
            int length = this.BlocksWide * this.verticalSampling;
            if (this.strengths.Length == 0)
            {
                this.strengths = this.Allocate(length);
            }

            return this.strengths.AsSpan(0, length);
        }
    }

    private float[] Allocate(int length) => this.scratchPool?.Rent(length) ?? new float[length];

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.previousStorage = null;
        this.Return(ref this.luma);
        this.Return(ref this.preErosion);
        this.Return(ref this.fuzzy);
        this.Return(ref this.strengths);
    }

    private void Return(ref float[] storage)
    {
        float[] owned = storage;
        storage = [];
        if (owned.Length != 0)
        {
            this.scratchPool?.Return(owned);
        }
    }

    /// <summary>Writes strengths in raster order for the luma blocks in this MCU row.</summary>
    internal void ComputeRow(ReadOnlySpan<byte> pixels, int stride, int _,
        PixelLayout layout, int top, int lumaAcQuant, Span<float> strengths,
        CancellationToken cancellationToken)
        => this.ComputeRow(new PixelSource(pixels, layout), stride, top, lumaAcQuant, strengths, cancellationToken);

    internal void ComputeRow(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => this.ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths,
            Vector.IsHardwareAccelerated, false, cancellationToken);

    internal void ComputeRowScalar(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => this.ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths, false, false, cancellationToken);

    internal void ComputeRowVector(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => this.ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths, true, false, cancellationToken);

    // The explicit batch entry also verifies eight-block reductions on runtimes
    // that implement Vector256 in software. Dispatch retains its four-lane path.
    internal void ComputeRowBatched(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => this.ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths,
            useVector: true, forceBatchedModulation: true, cancellationToken);

    // Value-type specialization keeps the opt-in certificate and diagnostic
    // branches out of the normal per-pixel loops, including non-tiered and AOT
    // builds. Dispatch once for the whole MCU row; both paths retain the same
    // rounded expressions, pixel ordering and neighborhood storage.
    private readonly struct OriginalDivision
    {
    }

    private readonly struct CertifiedDivision
    {
    }

    private void ComputeRowCore(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, bool useVector, bool forceBatchedModulation,
        CancellationToken cancellationToken)
    {
        if (this.useAqReciprocal)
        {
            this.ComputeRowCore<CertifiedDivision>(pixels, stride, top, lumaAcQuant,
                strengths, useVector, forceBatchedModulation, cancellationToken);
            // Specializing the arithmetic can change which NaN operand the JIT
            // propagates through commutative expressions. Exceptional internal
            // inputs therefore retain Original's exact payloads; public pixel
            // validation already excludes them. Only the research path scans
            // its output and its initial certificate counts remain available.
            foreach (float strength in strengths[..(this.BlocksWide * this.verticalSampling)])
            {
                if (!float.IsFinite(strength))
                {
                    this.ComputeRowCore<OriginalDivision>(pixels, stride, top, lumaAcQuant,
                        strengths, useVector, forceBatchedModulation, cancellationToken);
                    break;
                }
            }
        }
        else
        {
            this.ComputeRowCore<OriginalDivision>(pixels, stride, top, lumaAcQuant,
                strengths, useVector, forceBatchedModulation, cancellationToken);
        }
    }

    private void ComputeRowCore<TDivision>(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, bool useVector, bool forceBatchedModulation,
        CancellationToken cancellationToken)
        where TDivision : struct
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        int mcuHeight = this.verticalSampling * 8;
        bool immutableNeighborhood = pixels.HasCompleteConvertedPlanes
            || Math.Min(this.height, (long)top + mcuHeight + 5) <= pixels.ConvertedRows;
        IDisposable? storage = immutableNeighborhood ? pixels.ConvertedStorage : null;
        bool reuseNeighborhood = storage is not null && ReferenceEquals(storage, this.previousStorage)
            && top == this.previousTop + mcuHeight && stride == this.previousStride && useVector == this.previousUseVector;
        // A failed or canceled invocation must never expose partially updated
        // neighborhoods to the next call. Only immutable converted planes may
        // reuse rows; caller-owned source spans can change between invocations.
        this.previousStorage = null;
        cancellationToken.ThrowIfCancellationRequested();
        if (top < 0 || top >= this.height || top % mcuHeight != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(top));
        }

        if (strengths.Length < this.BlocksWide * this.verticalSampling)
        {
            throw new ArgumentException("The strength buffer must hold every luma block in the MCU row.", nameof(strengths));
        }

        // The outer four-row pre-erosion cells also need their immediate
        // pixel neighbors. Replicate the original image edges into all padding.
        int firstLumaY = top - 5;
        int firstLumaRow = 0;
        if (reuseNeighborhood)
        {
            // The next neighborhood shares ten complete padded rows. Copy
            // them together instead of rereading and padding each source row.
            this.luma.AsSpan(mcuHeight * this.lumaStride, 10 * this.lumaStride).CopyTo(this.luma);
            firstLumaRow = 10;
        }
        int previousSourceY = firstLumaRow == 0 ? -1
            : Math.Clamp(firstLumaY + firstLumaRow - 1, 0, this.height - 1);
        for (int row = firstLumaRow; row < mcuHeight + 10; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int sourceY = Math.Clamp(firstLumaY + row, 0, this.height - 1);
            int destination = (row * this.lumaStride) + 1;
            if (sourceY == previousSourceY)
            {
                // Top/bottom padding often repeats several full rows. Keep
                // their exact values without reentering the pixel reader.
                this.luma.AsSpan((row - 1) * this.lumaStride, this.lumaStride)
                    .CopyTo(this.luma.AsSpan(row * this.lumaStride, this.lumaStride));
                previousSourceY = sourceY;
                continue;
            }
            if (useVector)
            {
                pixels.ReadRowVector(this.width, this.height, stride, pixels.AnalysisComponent, 0, sourceY,
                    this.luma.AsSpan(destination, this.width));
            }
            else
            {
                pixels.ReadRowScalar(this.width, this.height, stride, pixels.AnalysisComponent, 0, sourceY,
                    this.luma.AsSpan(destination, this.width));
            }

            this.luma[destination - 1] = this.luma[destination];
            this.luma.AsSpan(destination + this.width, this.paddedWidth - this.width + 1).Fill(this.luma[destination + this.width - 1]);
            previousSourceY = sourceY;
        }

        int firstPreY = (top / 4) - 1;
        int preHeight = this.paddedHeight / 4;
        int firstPreRow = 0;
        if (reuseNeighborhood)
        {
            // The two final four-pixel rows are the next MCU's upper
            // neighborhood. Retain their exact already-rounded values.
            this.preErosion.AsSpan(this.verticalSampling * 2 * this.preStride, 2 * this.preStride).CopyTo(this.preErosion);
            firstPreRow = 2;
        }
        int previousPreY = firstPreRow == 0 ? -1
            : Math.Clamp(firstPreY + firstPreRow - 1, 0, preHeight - 1) * 4;
        for (int row = firstPreRow; row < (this.verticalSampling * 2) + 2; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int sourceY = Math.Clamp(firstPreY + row, 0, preHeight - 1) * 4;
            if (sourceY == previousPreY)
            {
                // Clamped top/bottom cells share the complete pixel stencil.
                // Retain its already-rounded result instead of analyzing it again.
                this.preErosion.AsSpan((row - 1) * this.preStride, this.preStride)
                    .CopyTo(this.preErosion.AsSpan(row * this.preStride, this.preStride));
            }
            else
            {
                this.ComputePreErosion<TDivision>(sourceY - firstLumaY, row, useVector);
            }
            previousPreY = sourceY;
        }

        const float acQuant = 0.841f;
        float dampen = lumaAcQuant < 9 ? 1f : MathF.Max(0f, 1f - ((lumaAcQuant - 9f) / 56f));
        float mul = acQuant * dampen;
        float add = (1f - dampen) * (0.48f * acQuant);
#if !NETSTANDARD2_1
        bool batchModulation = useVector && this.BlocksWide >= 8
            && ((Vector<float>.Count == 8 && Vector256.IsHardwareAccelerated) || forceBatchedModulation);
        // Allocate once per MCU row and reuse across every eight-block group.
        Span<Vector256<float>> modulationRows = batchModulation ? stackalloc Vector256<float>[16] : default;
        Span<float> modulationValues = batchModulation ? stackalloc float[16] : default;
#endif
        for (int blockY = 0; blockY < this.verticalSampling; blockY++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ComputeFuzzyErosion((blockY * 2) + 1, 0, useVector);
            this.ComputeFuzzyErosion((blockY * 2) + 2, this.preStride, useVector);
            int blockX = 0;
#if !NETSTANDARD2_1
            if (batchModulation)
            {
                for (; blockX <= this.BlocksWide - 8; blockX += 8)
                {
                    this.ModulateEightBlocks<TDivision>(blockX * 8, 5 + (blockY * 8), modulationRows, modulationValues);
                    FinalizeEightBlocks<TDivision>(this.fuzzy.AsSpan(this.preStride + (blockX * 2), 16),
                        this.fuzzy.AsSpan(blockX * 2, 16), modulationValues, mul, add,
                        strengths.Slice((blockY * this.BlocksWide) + blockX, 8), useVector: true,
                        this.aqRationalDiagnostics);
                }
            }
#endif
            for (; blockX < this.BlocksWide; blockX++)
            {
                int x = blockX * 2;
                float value = this.fuzzy[this.preStride + x] + this.fuzzy[this.preStride + x + 1]
                    + this.fuzzy[x] + this.fuzzy[x + 1];
                value = ComputeMask<TDivision>(value, this.aqRationalDiagnostics);
                if (useVector && Vector<float>.Count is 4 or 8)
                {
                    value = this.ModulateVector<TDivision>(blockX * 8, 5 + (blockY * 8), value);
                }
                else
                {
                    int offset = ((5 + (blockY * 8)) * this.lumaStride) + (blockX * 8) + 1;
                    value = ModulateScalar<TDivision>(this.luma.AsSpan(offset), this.lumaStride, value,
                        this.aqRationalDiagnostics);
                }
                float field = (FastPow2<TDivision>(value * 1.442695041f, this.aqRationalDiagnostics) * mul) + add;
                strengths[(blockY * this.BlocksWide) + blockX] = MathF.Max(0f,
                    Divide<TDivision>(0.6f, field, AqRationalSite.Strength, this.aqRationalDiagnostics) - 1f);
            }
        }
        this.previousStorage = storage;
        this.previousTop = top;
        this.previousStride = stride;
        this.previousUseVector = useVector;
    }

    private void ComputePreErosion<TDivision>(int firstRow, int outputRow, bool useVector)
        where TDivision : struct
    {
        const float gammaOffset = (float)(0.019 / InputScaling);
        float maskingScale = MathF.Sqrt((float)(211.50759899638012f * 1e8));
        int middle = (firstRow * this.lumaStride) + 1;
        int output = outputRow * this.preStride;
        int x = 0;
        if (useVector && Vector<float>.Count % 4 == 0)
        {
            for (; x <= this.paddedWidth - Vector<float>.Count; x += Vector<float>.Count)
            {
                int index = middle + x;
                Vector<float> above = new(this.luma, index - this.lumaStride);
                Vector<float> current = new(this.luma, index);
                Vector<float> below = new(this.luma, index + this.lumaStride);
                Vector<float> sum = this.PreErosionDifference<TDivision>(current, new(this.luma, index - 1), new(this.luma, index + 1),
                    above, below, gammaOffset, maskingScale);
                // Each following pixel row reuses the two vertical neighbors
                // already loaded. Retain the original left-to-right dy sum,
                // but keep it in a vector instead of rereading scratch per row.
                above = current;
                current = below;
                below = new(this.luma, index + (2 * this.lumaStride));
                sum += this.PreErosionDifference<TDivision>(current, new(this.luma, index + this.lumaStride - 1),
                    new(this.luma, index + this.lumaStride + 1), above, below, gammaOffset, maskingScale);
                above = current;
                current = below;
                below = new(this.luma, index + (3 * this.lumaStride));
                sum += this.PreErosionDifference<TDivision>(current, new(this.luma, index + (2 * this.lumaStride) - 1),
                    new(this.luma, index + (2 * this.lumaStride) + 1), above, below, gammaOffset, maskingScale);
                above = current;
                current = below;
                below = new(this.luma, index + (4 * this.lumaStride));
                sum += this.PreErosionDifference<TDivision>(current, new(this.luma, index + (3 * this.lumaStride) - 1),
                    new(this.luma, index + (3 * this.lumaStride) + 1), above, below, gammaOffset, maskingScale);
                // Reduce each horizontal group while its four-row sums are
                // still available. Constant-index lane reads preserve the original
                // left-to-right scalar reduction without width-sized staging.
#if !NETSTANDARD2_1
                if (Vector<float>.Count == 8)
                {
                    Vector256<float> sum256 = sum.AsVector256();
                    Vector128<float> lower = sum256.GetLower();
                    Vector128<float> upper = sum256.GetUpper();
                    this.preErosion[output + (x / 4)] =
                        (lower.GetElement(0) + lower.GetElement(1) + lower.GetElement(2) + lower.GetElement(3)) * 0.25f;
                    this.preErosion[output + (x / 4) + 1] =
                        (upper.GetElement(0) + upper.GetElement(1) + upper.GetElement(2) + upper.GetElement(3)) * 0.25f;
                }
                else if (Vector<float>.Count == 4)
                {
                    Vector128<float> sum128 = sum.AsVector128();
                    this.preErosion[output + (x / 4)] =
                        (sum128.GetElement(0) + sum128.GetElement(1) + sum128.GetElement(2) + sum128.GetElement(3)) * 0.25f;
                }
                else
#endif
                {
                    for (int lane = 0; lane < Vector<float>.Count; lane += 4)
                    {
                        this.preErosion[output + ((x + lane) / 4)] =
                            (sum[lane] + sum[lane + 1] + sum[lane + 2] + sum[lane + 3]) * 0.25f;
                    }
                }
            }
        }

        for (; x < this.paddedWidth; x += 4)
        {
            int index = middle + x;
            float first = this.PreErosionColumn<TDivision>(index, gammaOffset, maskingScale);
            float second = this.PreErosionColumn<TDivision>(index + 1, gammaOffset, maskingScale);
            float third = this.PreErosionColumn<TDivision>(index + 2, gammaOffset, maskingScale);
            float fourth = this.PreErosionColumn<TDivision>(index + 3, gammaOffset, maskingScale);
            this.preErosion[output + (x / 4)] = (first + second + third + fourth) * 0.25f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float PreErosionColumn<TDivision>(int index, float gammaOffset, float maskingScale)
        where TDivision : struct
    {
        float sum = this.PreErosionDifference<TDivision>(this.luma[index], this.luma[index - 1], this.luma[index + 1],
            this.luma[index - this.lumaStride], this.luma[index + this.lumaStride], gammaOffset, maskingScale);
        for (int dy = 1; dy < 4; dy++)
        {
            int next = index + (dy * this.lumaStride);
            sum += this.PreErosionDifference<TDivision>(this.luma[next], this.luma[next - 1], this.luma[next + 1],
                this.luma[next - this.lumaStride], this.luma[next + this.lumaStride], gammaOffset, maskingScale);
        }
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector<float> PreErosionDifference<TDivision>(Vector<float> value, Vector<float> left, Vector<float> right,
        Vector<float> above, Vector<float> below, float gammaOffset, float maskingScale)
        where TDivision : struct
    {
        Vector<float> baseValue = new Vector<float>(0.25f) * (right + left + (above + below));
        Vector<float> diff = DerivativeRatio<TDivision>(value + new Vector<float>(gammaOffset), invert: false,
            this.aqRationalDiagnostics) * (value - baseValue);
        diff = Vector.Min(diff * diff, new Vector<float>(0.2f));
        return new Vector<float>(0.25f) * Vector.SquareRoot((diff * new Vector<float>(maskingScale)) + new Vector<float>(28f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float PreErosionDifference<TDivision>(float value, float left, float right, float above, float below,
        float gammaOffset, float maskingScale)
        where TDivision : struct
    {
        float baseValue = 0.25f * (right + left + (above + below));
        float diff = DerivativeRatio<TDivision>(value + gammaOffset, invert: false,
            this.aqRationalDiagnostics) * (value - baseValue);
        diff = MathF.Min(diff * diff, 0.2f);
        return 0.25f * MathF.Sqrt((diff * maskingScale) + 28f);
    }

    private void ComputeFuzzyErosion(int row, int output, bool useVector)
    {
        int x = 0;
        // The boundary cells replicate their nearest neighbor; interior cells
        // can load all nine neighbors without gathering or clamping lanes.
        if (useVector && this.preStride > Vector<float>.Count + 1)
        {
            this.ComputeFuzzyErosionScalar(row, output, x++);
            for (; x < this.preStride - Vector<float>.Count; x += Vector<float>.Count)
            {
                int upper = ((row - 1) * this.preStride) + x;
                int middle = (row * this.preStride) + x;
                int lower = ((row + 1) * this.preStride) + x;
                Vector<float> min0 = new(this.preErosion, upper - 1);
                Vector<float> min1 = new(this.preErosion, upper);
                Vector<float> min2 = new(this.preErosion, upper + 1);
                Vector<float> min3 = new(this.preErosion, middle - 1);
                this.SelectFourMinima(ref min0, ref min1, ref min2, ref min3,
                    new(this.preErosion, middle), new(this.preErosion, middle + 1),
                    new(this.preErosion, lower - 1), new(this.preErosion, lower), new(this.preErosion, lower + 1));
                Vector<float> valueSum = (new Vector<float>(0.125f) * min0) + (new Vector<float>(0.075f) * min1)
                    + ((new Vector<float>(0.06f) * min2) + (new Vector<float>(0.05f) * min3));
                valueSum.CopyTo(this.fuzzy, output + x);
            }
        }
        for (; x < this.preStride; x++)
        {
            this.ComputeFuzzyErosionScalar(row, output, x);
        }
    }

    private void ComputeFuzzyErosionScalar(int row, int output, int x)
    {
        // Keep the four lowest values from the replicated 3x3 neighborhood.
        int left = Math.Max(x - 1, 0), right = Math.Min(x + 1, this.preStride - 1);
        int upper = (row - 1) * this.preStride, middle = row * this.preStride, lower = (row + 1) * this.preStride;
        float min0 = this.preErosion[upper + left], min1 = this.preErosion[upper + x];
        float min2 = this.preErosion[upper + right], min3 = this.preErosion[middle + left];
        this.SelectFourMinima(ref min0, ref min1, ref min2, ref min3,
            this.preErosion[middle + x], this.preErosion[middle + right],
            this.preErosion[lower + left], this.preErosion[lower + x], this.preErosion[lower + right]);
        this.fuzzy[output + x] = (0.125f * min0) + (0.075f * min1) + ((0.06f * min2) + (0.05f * min3));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SelectFourMinima(ref float v0, ref float v1, ref float v2, ref float v3,
        float v4, float v5, float v6, float v7, float v8)
    {
        if (this.useSelectionNetwork)
        {
            SelectFourMinimaNetwork(ref v0, ref v1, ref v2, ref v3, v4, v5, v6, v7, v8);
        }
        else
        {
            SelectFourMinimaInsertion(ref v0, ref v1, ref v2, ref v3, v4, v5, v6, v7, v8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SelectFourMinima(ref Vector<float> v0, ref Vector<float> v1,
        ref Vector<float> v2, ref Vector<float> v3, Vector<float> v4, Vector<float> v5,
        Vector<float> v6, Vector<float> v7, Vector<float> v8)
    {
        if (this.useSelectionNetwork)
        {
            SelectFourMinimaNetwork(ref v0, ref v1, ref v2, ref v3, v4, v5, v6, v7, v8);
        }
        else
        {
            SelectFourMinimaInsertion(ref v0, ref v1, ref v2, ref v3, v4, v5, v6, v7, v8);
        }
    }

    // Retain the original sorted-four insertion algorithm as an independent
    // comparison kernel: five compare/exchanges followed by five insertions,
    // each using three maxima and four minima (45 min/max operations total).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SelectFourMinimaInsertion(ref float v0, ref float v1, ref float v2, ref float v3,
        float v4, float v5, float v6, float v7, float v8)
    {
        SortPair(ref v0, ref v1);
        SortPair(ref v2, ref v3);
        SortPair(ref v0, ref v2);
        SortPair(ref v1, ref v3);
        SortPair(ref v1, ref v2);
        InsertMinimum(v4, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v5, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v6, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v7, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v8, ref v0, ref v1, ref v2, ref v3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SelectFourMinimaInsertion(ref Vector<float> v0, ref Vector<float> v1,
        ref Vector<float> v2, ref Vector<float> v3, Vector<float> v4, Vector<float> v5,
        Vector<float> v6, Vector<float> v7, Vector<float> v8)
    {
        SortPair(ref v0, ref v1);
        SortPair(ref v2, ref v3);
        SortPair(ref v0, ref v2);
        SortPair(ref v1, ref v3);
        SortPair(ref v1, ref v2);
        InsertMinimum(v4, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v5, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v6, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v7, ref v0, ref v1, ref v2, ref v3);
        InsertMinimum(v8, ref v0, ref v1, ref v2, ref v3);
    }

    // Seven-layer nine-input network, pruned backwards from its first four
    // sorted outputs: 17 compare/exchanges and five minimum-only operations.
    // This uses 39 min/max operations instead of 45 for four-value insertion.
    // Topology from Bert Dobbelaere's SorterHunter network catalog:
    // https://bertdobbelaere.github.io/sorting_networks.html#N9L25D7
    // For finite positive pre-erosion values, equal inputs have identical bits,
    // so reordering comparisons preserves the weighted erosion exactly.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SelectFourMinimaNetwork(ref float v0, ref float v1, ref float v2, ref float v3,
        float v4, float v5, float v6, float v7, float v8)
    {
        SortPair(ref v0, ref v3);
        SortPair(ref v1, ref v7);
        SortPair(ref v2, ref v5);
        SortPair(ref v4, ref v8);

        SortPair(ref v0, ref v7);
        SortPair(ref v2, ref v4);
        SortPair(ref v3, ref v8);
        SortPair(ref v5, ref v6);

        SortPair(ref v0, ref v2);
        SortPair(ref v1, ref v3);
        SortPair(ref v4, ref v5);
        v7 = MathF.Min(v7, v8);

        SortPair(ref v1, ref v4);
        v3 = MathF.Min(v3, v6);
        v5 = MathF.Min(v5, v7);

        SortPair(ref v0, ref v1);
        SortPair(ref v2, ref v4);
        SortPair(ref v3, ref v5);

        SortPair(ref v2, ref v3);
        v4 = MathF.Min(v4, v5);

        SortPair(ref v1, ref v2);
        v3 = MathF.Min(v3, v4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SelectFourMinimaNetwork(ref Vector<float> v0, ref Vector<float> v1,
        ref Vector<float> v2, ref Vector<float> v3, Vector<float> v4, Vector<float> v5,
        Vector<float> v6, Vector<float> v7, Vector<float> v8)
    {
        SortPair(ref v0, ref v3);
        SortPair(ref v1, ref v7);
        SortPair(ref v2, ref v5);
        SortPair(ref v4, ref v8);

        SortPair(ref v0, ref v7);
        SortPair(ref v2, ref v4);
        SortPair(ref v3, ref v8);
        SortPair(ref v5, ref v6);

        SortPair(ref v0, ref v2);
        SortPair(ref v1, ref v3);
        SortPair(ref v4, ref v5);
        v7 = Vector.Min(v7, v8);

        SortPair(ref v1, ref v4);
        v3 = Vector.Min(v3, v6);
        v5 = Vector.Min(v5, v7);

        SortPair(ref v0, ref v1);
        SortPair(ref v2, ref v4);
        SortPair(ref v3, ref v5);

        SortPair(ref v2, ref v3);
        v4 = Vector.Min(v4, v5);

        SortPair(ref v1, ref v2);
        v3 = Vector.Min(v3, v4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortPair(ref float first, ref float second)
    {
        float maximum = MathF.Max(first, second);
        first = MathF.Min(first, second);
        second = maximum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortPair(ref Vector<float> first, ref Vector<float> second)
    {
        var maximum = Vector.Max(first, second);
        first = Vector.Min(first, second);
        second = maximum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InsertMinimum(float value, ref float min0, ref float min1, ref float min2, ref float min3)
    {
        float next0 = MathF.Max(min0, value);
        float next1 = MathF.Max(min1, next0);
        float next2 = MathF.Max(min2, next1);
        min0 = MathF.Min(min0, value);
        min1 = MathF.Min(min1, next0);
        min2 = MathF.Min(min2, next1);
        min3 = MathF.Min(min3, next2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InsertMinimum(Vector<float> value, ref Vector<float> min0, ref Vector<float> min1,
        ref Vector<float> min2, ref Vector<float> min3)
    {
        var next0 = Vector.Max(min0, value);
        var next1 = Vector.Max(min1, next0);
        var next2 = Vector.Max(min2, next1);
        min0 = Vector.Min(min0, value);
        min1 = Vector.Min(min1, next0);
        min2 = Vector.Min(min2, next1);
        min3 = Vector.Min(min3, next2);
    }

    private static float ComputeMask<TDivision>(float value, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        float v = MathF.Max(value * 0.74760422233706747f, 1e-3f);
        float v2 = Divide<TDivision>(1f, v + 305.04035728311436f, AqRationalSite.Mask, diagnostics);
        float v3 = Divide<TDivision>(1f, (v * v) + 2.1925739705298404f, AqRationalSite.Mask, diagnostics);
        float v4 = Divide<TDivision>(1f, (v * v) + (0.25f * 2.1925739705298404f), AqRationalSite.Mask, diagnostics);
        return -0.74174993f + ((3.2353257320940401f * v4)
            + ((12.906028311180409f * v2) + (5.0220313103171232f * v3)));
    }

#if !NETSTANDARD2_1
    // The scalar alternative retains the original eight independent block
    // expressions for exact comparisons and exceptional-value propagation.
    internal static void FinalizeEightBlocks(ReadOnlySpan<float> lowerFuzzy, ReadOnlySpan<float> upperFuzzy,
        ReadOnlySpan<float> modulation, float mul, float add, Span<float> strengths, bool useVector)
        => FinalizeEightBlocks<OriginalDivision>(lowerFuzzy, upperFuzzy, modulation, mul, add, strengths,
            useVector, null);

    private static void FinalizeEightBlocks<TDivision>(ReadOnlySpan<float> lowerFuzzy, ReadOnlySpan<float> upperFuzzy,
        ReadOnlySpan<float> modulation, float mul, float add, Span<float> strengths, bool useVector,
        AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        // CertifiedDivision retains its original scalar calls and diagnostic
        // accounting. The ordinary path uses exact vector division, without
        // reciprocal approximations or reassociating any block's expression.
        if (useVector && typeof(TDivision) == typeof(OriginalDivision)
            && TryFinalizeEightBlocks(lowerFuzzy, upperFuzzy, modulation, mul, add, strengths))
        {
            return;
        }
        for (int block = 0; block < 8; block++)
        {
            int x = block * 2;
            float value = lowerFuzzy[x] + lowerFuzzy[x + 1] + upperFuzzy[x] + upperFuzzy[x + 1];
            value = ComputeMask<TDivision>(value, diagnostics);
            value = modulation[block] + value;
            value = (-0.15526878023684174f * InvLog2E * FastLog2<TDivision>(modulation[8 + block], diagnostics)) + value;
            float field = (FastPow2<TDivision>(value * 1.442695041f, diagnostics) * mul) + add;
            strengths[block] = MathF.Max(0f,
                Divide<TDivision>(0.6f, field, AqRationalSite.Strength, diagnostics) - 1f);
        }
    }

    private static bool TryFinalizeEightBlocks(ReadOnlySpan<float> lowerFuzzy, ReadOnlySpan<float> upperFuzzy,
        ReadOnlySpan<float> modulation, float mul, float add, Span<float> strengths)
    {
        DeinterleaveFuzzy(lowerFuzzy, out Vector256<float> lowerEven, out Vector256<float> lowerOdd);
        DeinterleaveFuzzy(upperFuzzy, out Vector256<float> upperEven, out Vector256<float> upperOdd);
        Vector256<float> value = lowerEven + lowerOdd + upperEven + upperOdd;
        var ratio = Vector256.Create(modulation.Slice(8, 8));
        var hf = Vector256.Create(modulation);
        var infinity = Vector256.Create(float.PositiveInfinity);
        Vector256<float> valid = Vector256.LessThan(Vector256.Abs(value), infinity)
            & Vector256.LessThan(Vector256.Abs(hf), infinity)
            & Vector256.GreaterThan(ratio, Vector256<float>.Zero)
            & Vector256.LessThan(ratio, infinity);
        if (valid.ExtractMostSignificantBits() != 255 || !float.IsFinite(mul) || !float.IsFinite(add))
        {
            return false;
        }

        var v = Vector256.Max(value * Vector256.Create(0.74760422233706747f), Vector256.Create(1e-3f));
        Vector256<float> v2 = Vector256.Create(1f) / (v + Vector256.Create(305.04035728311436f));
        Vector256<float> v3 = Vector256.Create(1f) / ((v * v) + Vector256.Create(2.1925739705298404f));
        Vector256<float> v4 = Vector256.Create(1f) / ((v * v) + Vector256.Create(0.25f * 2.1925739705298404f));
        value = Vector256.Create(-0.74174993f) + ((Vector256.Create(3.2353257320940401f) * v4)
            + ((Vector256.Create(12.906028311180409f) * v2) + (Vector256.Create(5.0220313103171232f) * v3)));
        value = hf + value;
        value = (Vector256.Create(-0.15526878023684174f * InvLog2E) * FastLog2Vector(ratio)) + value;
        value *= Vector256.Create(1.442695041f);
        // Keep exceptional float-to-int conversions and exponent wraparound on
        // the original path, as well as any NaN payload or signed-zero corner.
        if (!Vector256.GreaterThanOrEqualAll(value, Vector256.Create(-126f))
            || !Vector256.LessThanAll(value, Vector256.Create(127f)))
        {
            return false;
        }
        Vector256<float> field = (FastPow2Vector(value) * Vector256.Create(mul)) + Vector256.Create(add);
        if (!Vector256.LessThanAll(Vector256.Abs(field), infinity))
        {
            return false;
        }
        Vector256<float> strength = (Vector256.Create(0.6f) / field) - Vector256.Create(1f);
        // MathF.Max(0, -0) returns positive zero. Selecting strictly positive
        // strengths preserves that rule on every portable vector backend.
        Vector256.ConditionalSelect(Vector256.GreaterThan(strength, Vector256<float>.Zero),
            strength, Vector256<float>.Zero).CopyTo(strengths);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DeinterleaveFuzzy(ReadOnlySpan<float> values,
        out Vector256<float> even, out Vector256<float> odd)
    {
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        // Deinterleave 128-bit halves before joining so 256-bit x64 hosts emit
        // in-lane shuffles instead of extra cross-lane half extractions.
        var fourth = Vector128.Create(values.Slice(12, 4));
        var first = Vector128.Create(values);
        var second = Vector128.Create(values.Slice(4, 4));
        var third = Vector128.Create(values.Slice(8, 4));
        even = Vector256.Create(Vector128.UnzipEven(first, second), Vector128.UnzipEven(third, fourth));
        odd = Vector256.Create(Vector128.UnzipOdd(first, second), Vector128.UnzipOdd(third, fourth));
#else
        var second = Vector256.Create(values.Slice(8, 8));
        var first = Vector256.Create(values);
        var evens = Vector256.Create(0, 2, 4, 6, 0, 2, 4, 6);
        var odds = Vector256.Create(1, 3, 5, 7, 1, 3, 5, 7);
        even = Vector256.Create(Vector256.Shuffle(first, evens).GetLower(), Vector256.Shuffle(second, evens).GetLower());
        odd = Vector256.Create(Vector256.Shuffle(first, odds).GetLower(), Vector256.Shuffle(second, odds).GetLower());
#endif
    }

    private static Vector256<float> FastLog2Vector(Vector256<float> value)
    {
        Vector256<int> bits = value.AsInt32();
        var exponent = Vector256.ShiftRightArithmetic(bits - Vector256.Create(0x3f2aaaab), 23);
        Vector256<float> mantissa = (bits - Vector256.ShiftLeft(exponent, 23)).AsSingle() - Vector256.Create(1f);
        Vector256<float> numerator = (((Vector256.Create(0.74245873327820566f) * mantissa) + Vector256.Create(1.4287160470083755f))
            * mantissa) - Vector256.Create(1.8503833400518310e-6f);
        Vector256<float> denominator = (((Vector256.Create(0.17409343003366853f) * mantissa) + Vector256.Create(1.0096718572241148f))
            * mantissa) + Vector256.Create(0.99032814277590719f);
        return (numerator / denominator) + Vector256.ConvertToSingle(exponent);
    }

    private static Vector256<float> FastPow2Vector(Vector256<float> value)
    {
        var floor = Vector256.Floor(value);
        Vector256<float> exponent = Vector256.ShiftLeft(Vector256.ConvertToInt32(floor) + Vector256.Create(127), 23).AsSingle();
        Vector256<float> fraction = value - floor;
        Vector256<float> numerator = fraction + Vector256.Create(1.01749063e1f);
        numerator = (numerator * fraction) + Vector256.Create(4.88687798e1f);
        numerator = (numerator * fraction) + Vector256.Create(9.85506591e1f);
        numerator *= exponent;
        Vector256<float> denominator = (fraction * Vector256.Create(2.10242958e-1f)) - Vector256.Create(2.22328856e-2f);
        denominator = (denominator * fraction) - Vector256.Create(1.94414990e1f);
        denominator = (denominator * fraction) + Vector256.Create(9.85506633e1f);
        return numerator / denominator;
    }

    private void ModulateEightBlocks<TDivision>(int x, int y, Span<Vector256<float>> rows, Span<float> output)
        where TDivision : struct
    {
        Span<Vector256<float>> current = rows[..8], next = rows.Slice(8, 8);
        int row = (y * this.lumaStride) + x + 1;
        this.LoadModulationColumns(row, current);
        Vector256<float> sum = Vector256<float>.Zero, ratio = Vector256<float>.Zero;
        var gammaOffset = Vector256.Create(0.16f / InputScaling);
        for (int dy = 0; dy < 8; dy++)
        {
            if (dy != 7)
            {
                this.LoadModulationColumns(row + this.lumaStride, next);
            }

            ReadOnlySpan<Vector256<float>> below = dy == 7 ? current : next;
            // A lane now represents one block. Each lane sees the original
            // horizontal/vertical interleaving and gamma raster-order sum,
            // without extracting or horizontally reducing individual lanes.
            // Carry each horizontal neighbor into the next column instead of
            // reading it again; the last column has no horizontal difference.
            Vector256<float> samples = current[0];
            for (int column = 0; column < 7; column++)
            {
                Vector256<float> nextSamples = current[column + 1];
                sum += Vector256.Abs(samples - nextSamples);
                sum += Vector256.Abs(samples - below[column]);
                ratio += DerivativeRatio<TDivision>(samples + gammaOffset, this.aqRationalDiagnostics);
                samples = nextSamples;
            }
            sum += Vector256.Abs(samples - below[7]);
            ratio += DerivativeRatio<TDivision>(samples + gammaOffset, this.aqRationalDiagnostics);
            Span<Vector256<float>> previous = current;
            current = next;
            next = previous;
            row += this.lumaStride;
        }
        const float sumCoeff = (float)(-2.0052193233688884f * InputScaling / 112.0);
        (sum * Vector256.Create(sumCoeff)).CopyTo(output[..8]);
        (ratio * Vector256.Create(InputScaling / 64f)).CopyTo(output.Slice(8, 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadModulationColumns(int row, Span<Vector256<float>> columns)
    {
        for (int block = 0; block < 8; block++)
        {
            columns[block] = Vector256.Create(this.luma.AsSpan(row + (block * 8), 8));
        }
        // Eight consecutive blocks form a row panel. Transpose its samples so
        // every vector contains the same column from eight independent blocks.
        ForwardDct.TransposePortable(columns);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> DerivativeRatio<TDivision>(Vector256<float> value, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        value = Vector256.Max(value, Vector256<float>.Zero);
        Vector256<float> squared = value * value;
        Vector256<float> numerator = (Vector256.Create(NumMul) * squared) + Vector256.Create(NumOffset);
        Vector256<float> denominator = (Vector256.Create(DenMul) * value * squared) + Vector256.Create(VOffset);
        return Divide<TDivision>(numerator, denominator, AqRationalSite.Derivative, diagnostics);
    }

#endif
    private float ModulateVector<TDivision>(int x, int y, float value)
        where TDivision : struct
    {
        float sum = 0;
        float ratio = 0;
        int row = (y * this.lumaStride) + x + 1;
        var gammaOffset = new Vector<float>(0.16f / InputScaling);
        if (Vector<float>.Count == 8)
        {
            Vector<float> samples = new(this.luma, row);
            for (int dy = 0; dy < 8; dy++)
            {
                // Carry the next row forward and share the current samples
                // between both modulations, halving their luma vector loads.
                Vector<float> next = dy == 7 ? samples : new(this.luma, row + this.lumaStride);
                var horizontal = Vector.Abs(samples - new Vector<float>(this.luma, row + 1));
                var vertical = Vector.Abs(samples - next);
                sum = AddHfDifferences(sum, horizontal, vertical);
                ratio = AddGammaDerivatives(ratio, DerivativeRatio<TDivision>(samples + gammaOffset, invert: true,
                    this.aqRationalDiagnostics));
                samples = next;
                row += this.lumaStride;
            }
        }
        else
        {
            Vector<float> left = new(this.luma, row);
            Vector<float> right = new(this.luma, row + 4);
            for (int dy = 0; dy < 8; dy++)
            {
                Vector<float> nextLeft = dy == 7 ? left : new(this.luma, row + this.lumaStride);
                Vector<float> nextRight = dy == 7 ? right : new(this.luma, row + this.lumaStride + 4);
                var horizontalLeft = Vector.Abs(left - new Vector<float>(this.luma, row + 1));
                var horizontalRight = Vector.Abs(right - new Vector<float>(this.luma, row + 5));
                sum = AddHfFourDifferences(sum, horizontalLeft, Vector.Abs(left - nextLeft), includeLastHorizontal: true);
                sum = AddHfFourDifferences(sum, horizontalRight, Vector.Abs(right - nextRight), includeLastHorizontal: false);
                ratio = AddGammaFourDerivatives(ratio, DerivativeRatio<TDivision>(left + gammaOffset, invert: true,
                    this.aqRationalDiagnostics));
                ratio = AddGammaFourDerivatives(ratio, DerivativeRatio<TDivision>(right + gammaOffset, invert: true,
                    this.aqRationalDiagnostics));
                left = nextLeft;
                right = nextRight;
                row += this.lumaStride;
            }
        }

        // The reductions remain independent and strictly left to right.
        // Apply HF first, then gamma, matching the original two traversals.
        const float sumCoeff = (float)(-2.0052193233688884f * InputScaling / 112.0);
        value = (sum * sumCoeff) + value;
        ratio *= InputScaling / 64f;
        return (-0.15526878023684174f * InvLog2E * FastLog2<TDivision>(ratio, this.aqRationalDiagnostics)) + value;
    }

    internal static float ModulateScalar(ReadOnlySpan<float> samples, int stride, float value,
        bool useReciprocal = false, AqRationalDiagnostics? diagnostics = null)
        => useReciprocal ? ModulateScalar<CertifiedDivision>(samples, stride, value, diagnostics)
            : ModulateScalar<OriginalDivision>(samples, stride, value, diagnostics);

    private static float ModulateScalar<TDivision>(ReadOnlySpan<float> samples, int stride, float value,
        AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        float sum = 0;
        float ratio = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = dy * stride;
            int next = dy == 7 ? row : row + stride;
            float sample = samples[row];
            for (int dx = 0; dx < 7; dx++)
            {
                float nextSample = samples[row + dx + 1];
                sum += MathF.Abs(sample - nextSample);
                sum += MathF.Abs(sample - samples[next + dx]);
                ratio += DerivativeRatio<TDivision>(sample + (0.16f / InputScaling), invert: true, diagnostics);
                sample = nextSample;
            }
            // The final column has no horizontal difference. Carrying each
            // neighbor above preserves both raster-order sums without rereads.
            sum += MathF.Abs(sample - samples[next + 7]);
            ratio += DerivativeRatio<TDivision>(sample + (0.16f / InputScaling), invert: true, diagnostics);
        }
        // Share sample loads while retaining two independent raster-order sums.
        // Apply HF before gamma, exactly as the previous two scalar traversals did.
        const float sumCoeff = (float)(-2.0052193233688884f * InputScaling / 112.0);
        value = (sum * sumCoeff) + value;
        ratio *= InputScaling / 64f;
        return (-0.15526878023684174f * InvLog2E * FastLog2<TDivision>(ratio, diagnostics)) + value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AddHfFourDifferences(float sum, Vector<float> horizontal, Vector<float> vertical,
        bool includeLastHorizontal)
    {
        sum += horizontal[0];
        sum += vertical[0];
        sum += horizontal[1];
        sum += vertical[1];
        sum += horizontal[2];
        sum += vertical[2];
        if (includeLastHorizontal)
        {
            sum += horizontal[3];
        }

        return sum + vertical[3];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AddHfDifferences(float sum, Vector<float> horizontal, Vector<float> vertical)
    {
        // Constant lanes avoid spilling two vectors to stack scratch just to
        // reload them in a scalar loop. Preserve every original addition.
        sum += horizontal[0];
        sum += vertical[0];
        sum += horizontal[1];
        sum += vertical[1];
        sum += horizontal[2];
        sum += vertical[2];
        sum += horizontal[3];
        sum += vertical[3];
        sum += horizontal[4];
        sum += vertical[4];
        sum += horizontal[5];
        sum += vertical[5];
        sum += horizontal[6];
        sum += vertical[6];
        return sum + vertical[7];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AddGammaFourDerivatives(float ratio, Vector<float> values)
    {
        ratio += values[0];
        ratio += values[1];
        ratio += values[2];
        return ratio + values[3];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AddGammaDerivatives(float ratio, Vector<float> values)
    {
        ratio += values[0];
        ratio += values[1];
        ratio += values[2];
        ratio += values[3];
        ratio += values[4];
        ratio += values[5];
        ratio += values[6];
        return ratio + values[7];
    }

    private static float DerivativeRatio<TDivision>(float value, bool invert, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        value = MathF.Max(value, 0);
        float squared = value * value;
        float numerator = (NumMul * squared) + NumOffset;
        float denominator = (DenMul * value * squared) + VOffset;
        return invert ? Divide<TDivision>(numerator, denominator, AqRationalSite.Derivative, diagnostics)
            : Divide<TDivision>(denominator, numerator, AqRationalSite.Derivative, diagnostics);
    }

    private static Vector<float> DerivativeRatio<TDivision>(Vector<float> value, bool invert, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        value = Vector.Max(value, Vector<float>.Zero);
        Vector<float> squared = value * value;
        Vector<float> numerator = (new Vector<float>(NumMul) * squared) + new Vector<float>(NumOffset);
        Vector<float> denominator = (new Vector<float>(DenMul) * value * squared) + new Vector<float>(VOffset);
        return invert ? Divide<TDivision>(numerator, denominator, AqRationalSite.Derivative, diagnostics)
            : Divide<TDivision>(denominator, numerator, AqRationalSite.Derivative, diagnostics);
    }

    private static float FastLog2<TDivision>(float value, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        int exponent = unchecked(bits - 0x3f2aaaab) >> 23;
        float mantissa = BitConverter.Int32BitsToSingle(unchecked(bits - (exponent << 23))) - 1f;
        float numerator = (((0.74245873327820566f * mantissa) + 1.4287160470083755f)
            * mantissa) - 1.8503833400518310e-6f;
        float denominator = (((0.17409343003366853f * mantissa) + 1.0096718572241148f)
            * mantissa) + 0.99032814277590719f;
        return Divide<TDivision>(numerator, denominator, AqRationalSite.Log2, diagnostics) + exponent;
    }

    private static float FastPow2<TDivision>(float value, AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        float floor = MathF.Floor(value);
        float exponent = BitConverter.Int32BitsToSingle(((int)floor + 127) << 23);
        float fraction = value - floor;
        float numerator = fraction + 1.01749063e1f;
        numerator = (numerator * fraction) + 4.88687798e1f;
        numerator = (numerator * fraction) + 9.85506591e1f;
        numerator *= exponent;
        float denominator = (fraction * 2.10242958e-1f) - 2.22328856e-2f;
        denominator = (denominator * fraction) - 1.94414990e1f;
        denominator = (denominator * fraction) + 9.85506633e1f;
        return Divide<TDivision>(numerator, denominator, AqRationalSite.Pow2, diagnostics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Divide<TDivision>(float numerator, float denominator, AqRationalSite site,
        AqRationalDiagnostics? diagnostics)
        where TDivision : struct
    {
        if (typeof(TDivision) == typeof(OriginalDivision))
        {
            return numerator / denominator;
        }
        float result = AqReciprocal.Divide(numerator, denominator, out bool fallback);
        diagnostics?.Record(site, fallback);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<float> Divide<TDivision>(Vector<float> numerator, Vector<float> denominator, AqRationalSite site,
        AqRationalDiagnostics? diagnostics)
        where TDivision : struct
        => typeof(TDivision) == typeof(CertifiedDivision) ? DivideReciprocal(numerator, denominator, site, diagnostics) : numerator / denominator;

    private static Vector<float> DivideReciprocal(Vector<float> numerator, Vector<float> denominator,
        AqRationalSite site, AqRationalDiagnostics? diagnostics)
    {
        // Explicit research only: certify each lane and preserve the original
        // vector division's exceptional-value bits on every fallback lane.
        Span<float> results = stackalloc float[Vector<float>.Count];
        Span<bool> fallbacks = stackalloc bool[Vector<float>.Count];
        bool anyFallback = false;
        for (int lane = 0; lane < results.Length; lane++)
        {
            results[lane] = AqReciprocal.Divide(numerator[lane], denominator[lane], out bool fallback);
            fallbacks[lane] = fallback;
            anyFallback |= fallback;
            diagnostics?.Record(site, fallback);
        }
        if (anyFallback)
        {
            Vector<float> original = numerator / denominator;
            for (int lane = 0; lane < results.Length; lane++)
            {
                if (fallbacks[lane])
                {
                    results[lane] = original[lane];
                }
            }
        }
        diagnostics?.RecordVector(anyFallback, vector256: false);
        return new Vector<float>(results);
    }

#if !NETSTANDARD2_1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> Divide<TDivision>(Vector256<float> numerator, Vector256<float> denominator, AqRationalSite site,
        AqRationalDiagnostics? diagnostics)
        where TDivision : struct
        => typeof(TDivision) == typeof(CertifiedDivision) ? DivideReciprocal(numerator, denominator, site, diagnostics) : numerator / denominator;

    private static Vector256<float> DivideReciprocal(Vector256<float> numerator, Vector256<float> denominator,
        AqRationalSite site, AqRationalDiagnostics? diagnostics)
    {
        Span<float> results = stackalloc float[8];
        Span<bool> fallbacks = stackalloc bool[8];
        bool anyFallback = false;
        for (int lane = 0; lane < results.Length; lane++)
        {
            results[lane] = AqReciprocal.Divide(numerator[lane], denominator[lane], out bool fallback);
            fallbacks[lane] = fallback;
            anyFallback |= fallback;
            diagnostics?.Record(site, fallback);
        }
        if (anyFallback)
        {
            Vector256<float> original = numerator / denominator;
            for (int lane = 0; lane < results.Length; lane++)
            {
                if (fallbacks[lane])
                {
                    results[lane] = original[lane];
                }
            }
        }
        diagnostics?.RecordVector(anyFallback, vector256: true);
        return Vector256.Create(results);
    }
#endif
}
