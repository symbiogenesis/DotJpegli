namespace DotJpegli.Internal;

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
#if !NETSTANDARD2_1
using System.Numerics;
#endif

/// <summary>Parallel sample conversion and DCT with a single bounded MCU row of results.</summary>
internal sealed class ParallelDctRows
{
    private const int MinimumBlocksPerWorker = 32;
    private const int MinimumImagePixels = 128 * 128;
    private readonly PixelSource.Snapshot source;
    private readonly int width;
    private readonly int height;
    private readonly int stride;
    private readonly JpegSamplingFactors sampling;
    private readonly int smoothingFactor;
    private readonly int maxDegreeOfParallelism;
    private readonly int[] blockOffsets;
    private readonly int[] blocksWide;
    private readonly int[] blocksHigh;
    private float[] coefficients = [];
    private short[]? quantizedCoefficients;
    private ulong[]? quantizedNonzeroAcMasks;
    private float[] quantizedStrengths = [];
    private Quantization.PreparedTable[]? quantizers;
    private Action<int>? prepareQuantizedRange;
    private bool hasQuantizedStrengths;
    private int quantizedTop = -1;
    private readonly ExceptionDispatchInfo?[] errors;
    private readonly ParallelOptions parallelOptions;
    private readonly Action<int> prepareRange;
    private int firstPreparedBlock;
    private int preparedBlockCount;
    private int rangeCount;
    private int mcuRow;
    private int preparedTop = -1;
    private int preparedComponents;

    internal ParallelDctRows(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, int smoothingFactor, int maxDegreeOfParallelism,
        CancellationToken cancellationToken)
    {
        this.width = width;
        this.height = height;
        this.stride = stride;
        this.sampling = sampling;
        this.smoothingFactor = smoothingFactor;
        this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        this.parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        this.prepareRange = this.PrepareRange;
        int componentCount = pixels.IsGrayscale ? 1 : 3;
        this.blockOffsets = new int[componentCount + 1];
        this.blocksWide = new int[componentCount];
        this.blocksHigh = new int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            JpegComponentSampling factor = sampling[component];
            this.blocksWide[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal);
            this.blocksHigh[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical);
            this.blockOffsets[component + 1] = checked(this.blockOffsets[component] + (this.blocksWide[component] * factor.Vertical));
        }
        this.errors = new ExceptionDispatchInfo?[this.blockOffsets[^1]];
        int length = checked(((height - 1) * stride) + (width * pixels.ComponentsPerPixel));
        this.source = new PixelSource.Snapshot(pixels, length, cancellationToken,
            borrowCompactPlanes: sampling.Is420 && smoothingFactor == 0);
    }

    // Avoid a source snapshot and row scheduling when the image cannot supply
    // two useful ranges. Requested workers still apply to entropy replay.
    internal static bool ShouldCreate(int width, int height, JpegSamplingFactors sampling, bool grayscale)
    {
        if ((long)width * height < MinimumImagePixels)
        {
            return false;
        }
        int blocks = 0;
        int mcuWidth = 8 * sampling.MaxHorizontal;
        for (int component = 0; component < (grayscale ? 1 : 3); component++)
        {
            JpegComponentSampling factor = sampling[component];
            int componentWidth = ((width * factor.Horizontal) + mcuWidth - 1) / mcuWidth;
            blocks += componentWidth * factor.Vertical;
        }
        return blocks >= 2 * MinimumBlocksPerWorker;
    }

    internal void PrepareRow(int top, CancellationToken cancellationToken, int singleComponent = -1)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int componentMask = singleComponent < 0 ? (1 << this.blocksWide.Length) - 1 : 1 << singleComponent;
        if (this.preparedTop == top && (this.preparedComponents & componentMask) == componentMask)
        {
            return;
        }

        if (this.coefficients.Length == 0)
        {
            this.coefficients = new float[checked(this.blockOffsets[^1] * 64)];
        }

        if (this.preparedTop != top)
        {
            this.preparedComponents = 0;
        }
        // Separate-component scans only need that component's row. Computing
        // the other components would multiply progressive/refinement DCT work.
        int firstBlock = singleComponent < 0 ? 0 : this.blockOffsets[singleComponent];
        int endBlock = singleComponent < 0 ? this.blockOffsets[^1] : this.blockOffsets[singleComponent + 1];
        Array.Clear(this.errors, firstBlock, endBlock - firstBlock);
        this.firstPreparedBlock = firstBlock;
        this.preparedBlockCount = endBlock - firstBlock;
        this.rangeCount = Math.Max(1, Math.Min(this.maxDegreeOfParallelism,
            this.preparedBlockCount / MinimumBlocksPerWorker));
        this.mcuRow = top / (8 * this.sampling.MaxVertical);
        this.parallelOptions.CancellationToken = cancellationToken;
        // Each invocation joins all workers before exposing the row to the
        // ordered consumer. Coarse contiguous ranges amortize scheduling and
        // reconstruct the source only once per range. Short component rows
        // execute directly without a Parallel.For allocation or worker join.
        if (this.rangeCount == 1)
        {
            this.PrepareRange(0);
        }
        else
        {
            _ = Parallel.For(0, this.rangeCount, this.parallelOptions, this.prepareRange);
        }
        cancellationToken.ThrowIfCancellationRequested();
        for (int block = firstBlock; block < endBlock; block++)
        {
            this.errors[block]?.Throw();
        }

        this.preparedTop = top;
        this.preparedComponents |= componentMask;
    }

    internal bool CanPrepareQuantizedRows => this.blocksWide.Length == 3 && (this.sampling.Is420
        || (this.sampling.MaxHorizontal == 1 && this.sampling.MaxVertical == 1));
    internal bool HasFloatRowStorage => this.coefficients.Length != 0;
    internal bool HasQuantizedRowStorage => this.quantizedCoefficients is not null;
    internal PixelSource SourceForAnalysis => this.source.GetPixels();

    // The caller computes AQ once, then snapshots that row's strengths before
    // starting workers. Quantizers are immutable and their three references are
    // copied, so no worker depends on a caller-mutated array or span.
    internal void PrepareQuantizedRow(int top, Quantization.PreparedTable[] quantizers,
        ReadOnlySpan<float> strengths, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.ThrowIfNull(quantizers);
        if (!this.CanPrepareQuantizedRows)
        {
            throw new NotSupportedException("Quantized row preparation requires three-component 4:4:4 or 4:2:0 sampling.");
        }
        int mcuHeight = this.sampling.MaxVertical * 8;
        if (top < 0 || top >= this.height || top % mcuHeight != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(top));
        }
        if (quantizers.Length != 3)
        {
            throw new ArgumentException("Supply one immutable quantizer per component.", nameof(quantizers));
        }
        int strengthCount = (this.width + 7) / 8 * this.sampling.MaxVertical;
        if (!strengths.IsEmpty && strengths.Length < strengthCount)
        {
            throw new ArgumentException("Strengths must cover the luma MCU row.", nameof(strengths));
        }

        this.quantizedTop = -1;
        this.quantizedCoefficients ??= new short[checked(this.blockOffsets[^1] * 64)];
        this.quantizedNonzeroAcMasks ??= new ulong[this.blockOffsets[^1]];
#if !NETSTANDARD2_1
        bool changedQuantizers = this.quantizers is null;
        if (this.quantizers is not null)
        {
            for (int component = 0; component < 3; component++)
            {
                changedQuantizers |= !ReferenceEquals(this.quantizers[component], quantizers[component]);
            }
        }
#endif
        this.quantizers ??= new Quantization.PreparedTable[3];
        quantizers.CopyTo(this.quantizers, 0);
#if !NETSTANDARD2_1
        if (changedQuantizers && Vector.IsHardwareAccelerated && Vector<float>.Count is 4 or 8
            && this.source.GetPixels().HasCompleteConvertedPlanes)
        {
            // The fused plane kernels will need these immutable layouts. Warm
            // them on the coordinator so workers do not allocate losing CAS
            // candidates; scalar and compatibility paths never request them.
            foreach (Quantization.PreparedTable quantizer in this.quantizers)
            {
                _ = quantizer.GetTransposed();
            }
        }
#endif
        this.hasQuantizedStrengths = !strengths.IsEmpty;
        if (this.hasQuantizedStrengths)
        {
            if (this.quantizedStrengths.Length == 0)
            {
                this.quantizedStrengths = new float[strengthCount];
            }
            strengths[..strengthCount].CopyTo(this.quantizedStrengths);
        }
        Array.Clear(this.errors, 0, this.errors.Length);
        this.firstPreparedBlock = 0;
        this.preparedBlockCount = this.blockOffsets[^1];
        this.rangeCount = Math.Max(1, Math.Min(this.maxDegreeOfParallelism,
            this.preparedBlockCount / MinimumBlocksPerWorker));
        this.mcuRow = top / mcuHeight;
        this.parallelOptions.CancellationToken = cancellationToken;
        this.prepareQuantizedRange ??= this.PrepareQuantizedRange;
        if (this.rangeCount == 1)
        {
            this.PrepareQuantizedRange(0);
        }
        else
        {
            _ = Parallel.For(0, this.rangeCount, this.parallelOptions, this.prepareQuantizedRange);
        }
        cancellationToken.ThrowIfCancellationRequested();
        for (int block = 0; block < this.errors.Length; block++)
        {
            this.errors[block]?.Throw();
        }
        this.quantizedTop = top;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PrepareQuantizedRange(int range)
    {
        int firstBlock = (int)((long)this.preparedBlockCount * range / this.rangeCount);
        int endBlock = (int)((long)this.preparedBlockCount * (range + 1) / this.rangeCount);
        PixelSource pixels = this.source.GetPixels();
        Span<float> samples = stackalloc float[64];
        int component = 0;
        int strengthWidth = (this.width + 7) / 8;
        for (int block = firstBlock; block < endBlock; block++)
        {
            this.parallelOptions.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                while (block >= this.blockOffsets[component + 1])
                {
                    component++;
                }
                JpegComponentSampling factor = this.sampling[component];
                int localBlock = block - this.blockOffsets[component];
                int blockX = localBlock % this.blocksWide[component];
                int localRow = localBlock / this.blocksWide[component];
                int blockY = (this.mcuRow * factor.Vertical) + localRow;
                if (blockY >= this.blocksHigh[component])
                {
                    continue;
                }
                float strength = this.hasQuantizedStrengths ? this.quantizedStrengths[(localRow * strengthWidth)
                    + (blockX * this.sampling.MaxHorizontal / factor.Horizontal)] : 0f;
                Span<short> destination = this.quantizedCoefficients!.AsSpan(block * 64, 64);
                Quantization.PreparedTable quantizer = this.quantizers![component];
                bool fullResolution = this.smoothingFactor == 0
                    && factor.Horizontal == this.sampling.MaxHorizontal && factor.Vertical == this.sampling.MaxVertical;
                bool compactChroma = this.smoothingFactor == 0 && this.sampling.Is420 && component != 0;
                ReadOnlySpan<float> plane = fullResolution ? pixels.GetConvertedPlane(component) : default;
                int planeWidth = this.width, planeHeight = this.height, planeTop = blockY * 8;
                if (compactChroma)
                {
                    plane = pixels.GetConverted420ChromaPlane(component, out int chromaWidth, out int chromaHeight);
                    planeWidth = chromaWidth;
                    planeHeight = chromaHeight;
                }
                if (plane.IsEmpty && (fullResolution || compactChroma))
                {
                    plane = pixels.GetConvertedPlaneSegment(component, blockY * 8, compactChroma,
                        out int segWidth, out int segHeight, out int segTop);
                    planeWidth = segWidth;
                    planeHeight = segHeight;
                    planeTop = segTop;
                }
                if (!plane.IsEmpty)
                {
                    if ((blockX * 8) + 8 <= planeWidth && planeTop + 8 <= planeHeight)
                    {
                        this.quantizedNonzeroAcMasks![block] = ForwardDct.TransformQuantizePlaneWithMask(
                            plane, planeWidth, blockX * 8, planeTop, quantizer, destination, strength);
                    }
                    else
                    {
                        this.quantizedNonzeroAcMasks![block] = ForwardDct.TransformQuantizePaddedPlaneWithMask(
                            plane, planeWidth, planeHeight, blockX * 8, planeTop, quantizer, destination, strength);
                    }
                    continue;
                }
                Downsampling.FillBlock(pixels, this.width, this.height, this.stride, component,
                    blockX, blockY, this.sampling.MaxHorizontal, this.sampling.MaxVertical,
                    factor.Horizontal, factor.Vertical, samples, this.smoothingFactor);
                this.quantizedNonzeroAcMasks![block] = ForwardDct.TransformQuantizeBlockWithMask(
                    samples, quantizer, destination, strength);
            }
            catch (Exception error)
            {
                this.errors[block] = ExceptionDispatchInfo.Capture(error);
            }
        }
    }

    internal ReadOnlySpan<short> GetQuantizedBlock(int component, int blockX, int blockY)
        => this.GetQuantizedBlock(component, blockX, blockY, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlySpan<short> GetQuantizedBlock(int component, int blockX, int blockY, out ulong nonzeroAcMask)
    {
        if (this.quantizedTop < 0 || blockY / this.sampling[component].Vertical
            != this.quantizedTop / (8 * this.sampling.MaxVertical))
        {
            throw new InvalidOperationException("Prepare the complete quantized MCU row before reading its blocks.");
        }
        int localRow = blockY % this.sampling[component].Vertical;
        int block = this.blockOffsets[component] + (localRow * this.blocksWide[component]) + blockX;
        nonzeroAcMask = this.quantizedNonzeroAcMasks![block];
        return this.quantizedCoefficients.AsSpan(block * 64, 64);
    }

    private void PrepareRange(int range)
    {
        int firstBlock = this.firstPreparedBlock + (int)((long)this.preparedBlockCount * range / this.rangeCount);
        int endBlock = this.firstPreparedBlock + (int)((long)this.preparedBlockCount * (range + 1) / this.rangeCount);
        PixelSource pixels = this.source.GetPixels();
        Span<float> samples = stackalloc float[64];
        int component = 0;
        for (int block = firstBlock; block < endBlock; block++)
        {
            this.parallelOptions.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                while (block >= this.blockOffsets[component + 1])
                {
                    component++;
                }

                JpegComponentSampling factor = this.sampling[component];
                int localBlock = block - this.blockOffsets[component];
                int blockX = localBlock % this.blocksWide[component];
                int blockY = (this.mcuRow * factor.Vertical) + (localBlock / this.blocksWide[component]);
                if (blockY >= this.blocksHigh[component])
                {
                    continue; // Dummy blocks retain the ordered consumer's DC predictor.
                }

                bool fullResolution = this.smoothingFactor == 0
                    && factor.Horizontal == this.sampling.MaxHorizontal && factor.Vertical == this.sampling.MaxVertical;
                bool compactChroma = this.smoothingFactor == 0 && this.sampling.Is420 && component != 0;
                ReadOnlySpan<float> plane = fullResolution ? pixels.GetConvertedPlane(component) : default;
                int planeWidth = this.width, planeHeight = this.height;
                int planeTop = blockY * 8;
                if (compactChroma)
                {
                    plane = pixels.GetConverted420ChromaPlane(component, out int chromaWidth, out int chromaHeight);
                    planeWidth = chromaWidth;
                    planeHeight = chromaHeight;
                }
                if (plane.IsEmpty && (fullResolution || compactChroma))
                {
                    plane = pixels.GetConvertedPlaneSegment(component, blockY * 8, compactChroma,
                        out int segWidth, out int segHeight, out int segTop);
                    planeWidth = segWidth;
                    planeHeight = segHeight;
                    planeTop = segTop;
                }
                Span<float> destination = this.coefficients.AsSpan(block * 64, 64);
                if (!plane.IsEmpty)
                {
                    int left = blockX * 8, top = planeTop;
                    if (left + 8 <= planeWidth && top + 8 <= planeHeight)
                    {
                        ForwardDct.TransformPlane(plane, planeWidth, left, top, destination);
                        continue;
                    }
                    for (int row = 0; row < 8; row++)
                    {
                        int sourceRow = Math.Min(top + row, planeHeight - 1) * planeWidth;
                        for (int column = 0; column < 8; column++)
                        {
                            samples[(row * 8) + column] = plane[sourceRow + Math.Min(left + column, planeWidth - 1)] - 128f;
                        }
                    }
                }
                else
                {
                    Downsampling.FillBlock(pixels, this.width, this.height, this.stride, component,
                        blockX, blockY, this.sampling.MaxHorizontal, this.sampling.MaxVertical,
                        factor.Horizontal, factor.Vertical, samples, this.smoothingFactor);
                }
                ForwardDct.Transform(samples, destination);
            }
            catch (Exception error)
            {
                // Preserve the public exception type rather than exposing a
                // Parallel.For AggregateException. Slots retain deterministic order.
                this.errors[block] = ExceptionDispatchInfo.Capture(error);
            }
        }
    }

    internal void CopyBlock(int component, int blockX, int blockY, Span<float> destination)
    {
        int localRow = blockY % this.sampling[component].Vertical;
        int block = this.blockOffsets[component] + (localRow * this.blocksWide[component]) + blockX;
        this.coefficients.AsSpan(block * 64, 64).CopyTo(destination);
    }
}
