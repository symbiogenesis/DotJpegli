// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

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
    private float[] luma = [];
    private float[] preErosion = [];
    private float[] difference = [];
    private float[] fuzzy = [];
    private float[] strengths = [];
    private bool disposed;
    private IDisposable? previousStorage;
    private int previousTop;
    private int previousStride;
    private bool previousUseVector;

    internal AdaptiveQuantization(int width, int height, int verticalSampling, ArrayPool<float>? scratchPool = null,
        KernelPreference fuzzyErosionKernel = KernelPreference.Automatic)
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
            this.difference = this.Allocate(this.paddedWidth);
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
        this.Return(ref this.difference);
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

    private void ComputeRowCore(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, bool useVector, bool forceBatchedModulation,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        int mcuHeight = this.verticalSampling * 8;
        IDisposable? storage = pixels.ConvertedStorage;
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
        for (int row = firstPreRow; row < (this.verticalSampling * 2) + 2; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int sourceY = Math.Clamp(firstPreY + row, 0, preHeight - 1) * 4;
            this.ComputePreErosion(sourceY - firstLumaY, row, useVector);
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
                    this.ModulateEightBlocks(blockX * 8, 5 + (blockY * 8), modulationRows, modulationValues);
                    for (int block = 0; block < 8; block++)
                    {
                        int x = (blockX + block) * 2;
                        float value = this.fuzzy[this.preStride + x] + this.fuzzy[this.preStride + x + 1]
                            + this.fuzzy[x] + this.fuzzy[x + 1];
                        value = ComputeMask(value);
                        value = modulationValues[block] + value;
                        value = (-0.15526878023684174f * InvLog2E * FastLog2(modulationValues[8 + block])) + value;
                        float field = (FastPow2(value * 1.442695041f) * mul) + add;
                        strengths[(blockY * this.BlocksWide) + blockX + block] = MathF.Max(0f, (0.6f / field) - 1f);
                    }
                }
            }
#endif
            for (; blockX < this.BlocksWide; blockX++)
            {
                int x = blockX * 2;
                float value = this.fuzzy[this.preStride + x] + this.fuzzy[this.preStride + x + 1]
                    + this.fuzzy[x] + this.fuzzy[x + 1];
                value = ComputeMask(value);
                if (useVector && Vector<float>.Count is 4 or 8)
                {
                    value = this.ModulateVector(blockX * 8, 5 + (blockY * 8), value);
                }
                else
                {
                    value = this.HfModulationScalar(blockX * 8, 5 + (blockY * 8), value);
                    value = this.GammaModulationScalar(blockX * 8, 5 + (blockY * 8), value);
                }
                float field = (FastPow2(value * 1.442695041f) * mul) + add;
                strengths[(blockY * this.BlocksWide) + blockX] = MathF.Max(0f, (0.6f / field) - 1f);
            }
        }
        this.previousStorage = storage;
        this.previousTop = top;
        this.previousStride = stride;
        this.previousUseVector = useVector;
    }

    private void ComputePreErosion(int firstRow, int outputRow, bool useVector)
    {
        const float gammaOffset = (float)(0.019 / InputScaling);
        float maskingScale = MathF.Sqrt((float)(211.50759899638012f * 1e8));
        int middle = (firstRow * this.lumaStride) + 1;
        int x = 0;
        if (useVector)
        {
            for (; x <= this.paddedWidth - Vector<float>.Count; x += Vector<float>.Count)
            {
                int index = middle + x;
                Vector<float> above = new(this.luma, index - this.lumaStride);
                Vector<float> current = new(this.luma, index);
                Vector<float> below = new(this.luma, index + this.lumaStride);
                Vector<float> sum = PreErosionDifference(current, new(this.luma, index - 1), new(this.luma, index + 1),
                    above, below, gammaOffset, maskingScale);
                // Each following pixel row reuses the two vertical neighbors
                // already loaded. Retain the original left-to-right dy sum,
                // but keep it in a vector instead of rereading scratch per row.
                above = current;
                current = below;
                below = new(this.luma, index + (2 * this.lumaStride));
                sum += PreErosionDifference(current, new(this.luma, index + this.lumaStride - 1),
                    new(this.luma, index + this.lumaStride + 1), above, below, gammaOffset, maskingScale);
                above = current;
                current = below;
                below = new(this.luma, index + (3 * this.lumaStride));
                sum += PreErosionDifference(current, new(this.luma, index + (2 * this.lumaStride) - 1),
                    new(this.luma, index + (2 * this.lumaStride) + 1), above, below, gammaOffset, maskingScale);
                above = current;
                current = below;
                below = new(this.luma, index + (4 * this.lumaStride));
                sum += PreErosionDifference(current, new(this.luma, index + (3 * this.lumaStride) - 1),
                    new(this.luma, index + (3 * this.lumaStride) + 1), above, below, gammaOffset, maskingScale);
                sum.CopyTo(this.difference, x);
            }
        }

        for (; x < this.paddedWidth; x++)
        {
            int index = middle + x;
            float sum = PreErosionDifference(this.luma[index], this.luma[index - 1], this.luma[index + 1],
                this.luma[index - this.lumaStride], this.luma[index + this.lumaStride], gammaOffset, maskingScale);
            for (int dy = 1; dy < 4; dy++)
            {
                int next = index + (dy * this.lumaStride);
                sum += PreErosionDifference(this.luma[next], this.luma[next - 1], this.luma[next + 1],
                    this.luma[next - this.lumaStride], this.luma[next + this.lumaStride], gammaOffset, maskingScale);
            }
            this.difference[x] = sum;
        }
        int output = outputRow * this.preStride;
        for (x = 0; x < this.preStride; x++)
        {
            int offset = x * 4;
            this.preErosion[output + x] = (this.difference[offset] + this.difference[offset + 1]
                + this.difference[offset + 2] + this.difference[offset + 3]) * 0.25f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<float> PreErosionDifference(Vector<float> value, Vector<float> left, Vector<float> right,
        Vector<float> above, Vector<float> below, float gammaOffset, float maskingScale)
    {
        Vector<float> baseValue = new Vector<float>(0.25f) * (right + left + (above + below));
        Vector<float> diff = DerivativeRatio(value + new Vector<float>(gammaOffset), invert: false) * (value - baseValue);
        diff = Vector.Min(diff * diff, new Vector<float>(0.2f));
        return new Vector<float>(0.25f) * Vector.SquareRoot((diff * new Vector<float>(maskingScale)) + new Vector<float>(28f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PreErosionDifference(float value, float left, float right, float above, float below,
        float gammaOffset, float maskingScale)
    {
        float baseValue = 0.25f * (right + left + (above + below));
        float diff = DerivativeRatio(value + gammaOffset, invert: false) * (value - baseValue);
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

    private static float ComputeMask(float value)
    {
        float v = MathF.Max(value * 0.74760422233706747f, 1e-3f);
        float v2 = 1f / (v + 305.04035728311436f);
        float v3 = 1f / ((v * v) + 2.1925739705298404f);
        float v4 = 1f / ((v * v) + (0.25f * 2.1925739705298404f));
        return -0.74174993f + ((3.2353257320940401f * v4)
            + ((12.906028311180409f * v2) + (5.0220313103171232f * v3)));
    }

#if !NETSTANDARD2_1
    private void ModulateEightBlocks(int x, int y, Span<Vector256<float>> rows, Span<float> output)
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
            for (int column = 0; column < 8; column++)
            {
                Vector256<float> samples = current[column];
                if (column != 7)
                {
                    sum += Vector256.Abs(samples - current[column + 1]);
                }

                sum += Vector256.Abs(samples - below[column]);
                ratio += DerivativeRatio(samples + gammaOffset);
            }
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
    private static Vector256<float> DerivativeRatio(Vector256<float> value)
    {
        value = Vector256.Max(value, Vector256<float>.Zero);
        Vector256<float> squared = value * value;
        Vector256<float> numerator = (Vector256.Create(NumMul) * squared) + Vector256.Create(NumOffset);
        Vector256<float> denominator = (Vector256.Create(DenMul) * value * squared) + Vector256.Create(VOffset);
        return numerator / denominator;
    }

#endif
    private float ModulateVector(int x, int y, float value)
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
                ratio = AddGammaDerivatives(ratio, DerivativeRatio(samples + gammaOffset, invert: true));
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
                ratio = AddGammaFourDerivatives(ratio, DerivativeRatio(left + gammaOffset, invert: true));
                ratio = AddGammaFourDerivatives(ratio, DerivativeRatio(right + gammaOffset, invert: true));
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
        return (-0.15526878023684174f * InvLog2E * FastLog2(ratio)) + value;
    }

    private float HfModulationScalar(int x, int y, float value)
    {
        float sum = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = ((y + dy) * this.lumaStride) + x + 1;
            int next = dy == 7 ? row : row + this.lumaStride;
            for (int dx = 0; dx < 8; dx++)
            {
                if (dx != 7)
                {
                    sum += MathF.Abs(this.luma[row + dx] - this.luma[row + dx + 1]);
                }

                sum += MathF.Abs(this.luma[row + dx] - this.luma[next + dx]);
            }
        }
        const float sumCoeff = (float)(-2.0052193233688884f * InputScaling / 112.0);
        return (sum * sumCoeff) + value;
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

    private float GammaModulationScalar(int x, int y, float value)
    {
        float ratio = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = ((y + dy) * this.lumaStride) + x + 1;
            for (int dx = 0; dx < 8; dx++)
            {
                ratio += DerivativeRatio(this.luma[row + dx] + (0.16f / InputScaling), invert: true);
            }
        }
        ratio *= InputScaling / 64f;
        return (-0.15526878023684174f * InvLog2E * FastLog2(ratio)) + value;
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

    private static float DerivativeRatio(float value, bool invert)
    {
        value = MathF.Max(value, 0);
        float squared = value * value;
        float numerator = (NumMul * squared) + NumOffset;
        float denominator = (DenMul * value * squared) + VOffset;
        return invert ? numerator / denominator : denominator / numerator;
    }

    private static Vector<float> DerivativeRatio(Vector<float> value, bool invert)
    {
        value = Vector.Max(value, Vector<float>.Zero);
        Vector<float> squared = value * value;
        Vector<float> numerator = (new Vector<float>(NumMul) * squared) + new Vector<float>(NumOffset);
        Vector<float> denominator = (new Vector<float>(DenMul) * value * squared) + new Vector<float>(VOffset);
        return invert ? numerator / denominator : denominator / numerator;
    }

    private static float FastLog2(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        int exponent = unchecked(bits - 0x3f2aaaab) >> 23;
        float mantissa = BitConverter.Int32BitsToSingle(unchecked(bits - (exponent << 23))) - 1f;
        float numerator = (((0.74245873327820566f * mantissa) + 1.4287160470083755f)
            * mantissa) - 1.8503833400518310e-6f;
        float denominator = (((0.17409343003366853f * mantissa) + 1.0096718572241148f)
            * mantissa) + 0.99032814277590719f;
        return (numerator / denominator) + exponent;
    }

    private static float FastPow2(float value)
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
        return numerator / denominator;
    }
}
