namespace DotJpegli.Internal;

using System.Buffers;
#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

/// <summary>Reuses full coefficients across histogram, spectral and refinement passes.</summary>
internal sealed class QuantizedImage : IDisposable
{
    // Keep the streaming path for very large images rather than requiring an
    // unbounded image-sized allocation. The rented arrays are returned on every exit.
    internal const long MaximumBytes = 64L * 1024 * 1024;
    private const int SegmentLength = 65536;
    private readonly short[][] components;
    private readonly short[][][]? segments;
    private readonly int[] widths;
    private readonly int[] cachedHeights;
    private readonly int cachedMcuRows;
    private readonly int mcuHeight;
    private readonly ArrayPool<short> pool;
    private readonly EncoderCacheBudget? cacheBudget;

    private QuantizedImage(int[] widths, int[] cachedHeights, int cachedMcuRows, int mcuHeight,
        bool complete, bool segmented, bool zigZagOrdered, ArrayPool<short> pool, EncoderCacheBudget? cacheBudget)
    {
        this.components = new short[widths.Length][];
        this.segments = segmented ? new short[widths.Length][][] : null;
        this.widths = widths;
        this.cachedHeights = cachedHeights;
        this.cachedMcuRows = cachedMcuRows;
        this.mcuHeight = mcuHeight;
        this.pool = pool;
        this.cacheBudget = cacheBudget;
        this.IsComplete = complete;
        this.IsZigZagOrdered = zigZagOrdered;
    }

    internal bool IsZigZagOrdered { get; }
    internal bool IsComplete { get; }
    internal long RentedBytes { get; private set; }

    internal bool ContainsRow(int component, int y) => (uint)y < (uint)this.cachedHeights[component];
    internal bool ContainsMcuRow(int top) => top >= 0 && top / this.mcuHeight < this.cachedMcuRows;

    internal static QuantizedImage? Create(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, CancellationToken cancellationToken,
        bool zigZagOrdered = false, KernelPreference narrowZigZagKernel = KernelPreference.Automatic,
        double rateDistortionLambda = 0, RateDistortionDiagnostics? rateDistortionDiagnostics = null,
        long maximumBytes = MaximumBytes, bool allowPartial = false, EncoderCacheBudget? cacheBudget = null,
        ArrayPool<short>? pool = null)
    {
        Guard.ThrowIfNegative(maximumBytes);
        cancellationToken.ThrowIfCancellationRequested();
        if (!double.IsFinite(rateDistortionLambda) || rateDistortionLambda < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rateDistortionLambda));
        }
        if (rateDistortionLambda > 0 && !zigZagOrdered)
        {
            throw new ArgumentException("The offline rate-distortion experiment requires zigzag-ordered cache blocks.", nameof(zigZagOrdered));
        }
        int[] widths = new int[tables.Length];
        int[] heights = new int[tables.Length];
        for (int component = 0; component < tables.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal);
            heights[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical);
        }

        long available = Math.Min(Math.Min(maximumBytes, MaximumBytes), cacheBudget?.AvailableBytes ?? long.MaxValue);
        int mcuHeight = sampling.MaxVertical * 8;
        int totalMcuRows = (height + mcuHeight - 1) / mcuHeight;
        int cachedMcuRows = totalMcuRows;
        bool segmented = PlannedBytes(widths, heights, totalMcuRows, sampling, segmented: false) > available;
        if (segmented)
        {
            if (!allowPartial || rateDistortionLambda > 0)
            {
                return null;
            }

            // Fixed-size coefficient segments avoid whole-component pool bucket
            // jumps. Above the cap, retain a complete prefix of MCU rows across
            // every component so the remaining scans can still reuse that work.
            int lower = 0, upper = totalMcuRows;
            while (lower < upper)
            {
                int middle = lower + ((upper - lower + 1) / 2);
                if (PlannedBytes(widths, heights, middle, sampling, segmented: true) <= available)
                {
                    lower = middle;
                }
                else
                {
                    upper = middle - 1;
                }
            }
            cachedMcuRows = lower;
            if (cachedMcuRows == 0)
            {
                return null;
            }
        }

        int[] cachedHeights = new int[heights.Length];
        for (int component = 0; component < heights.Length; component++)
        {
            cachedHeights[component] = Math.Min(heights[component], cachedMcuRows * sampling[component].Vertical);
        }

        var image = new QuantizedImage(widths, cachedHeights, cachedMcuRows, mcuHeight,
            cachedMcuRows == totalMcuRows, segmented, zigZagOrdered, pool ?? ArrayPool<short>.Shared, cacheBudget);
        try
        {
            var quantizers = new Quantization.PreparedTable[tables.Length];
            for (int component = 0; component < tables.Length; component++)
            {
                int length = checked(widths[component] * cachedHeights[component] * 64);
                if (!image.AllocateComponent(component, length, available, cancellationToken))
                {
                    image.Dispose();
                    return null;
                }
                quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: pixels.IsRgb, narrowZigZagKernel: narrowZigZagKernel,
                    imageWidth: width, imageHeight: height);
            }
            Span<float> samples = stackalloc float[64];
            Span<short> naturalOrder = stackalloc short[64];
            int horizontal = sampling.MaxHorizontal;
            int vertical = sampling.MaxVertical;
            Span<float> strengths = adaptive is null ? default : adaptive.Strengths;
            for (int top = 0; top < Math.Min(height, cachedMcuRows * mcuHeight); top += vertical * 8)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pixels.ParallelRows?.PrepareRow(top, cancellationToken);
                adaptive?.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths, cancellationToken);

                for (int component = 0; component < tables.Length; component++)
                {
                    JpegComponentSampling factor = sampling[component];
                    ReadOnlySpan<float> plane = smoothingFactor == 0 && factor.Horizontal == horizontal
                        && factor.Vertical == vertical ? pixels.GetConvertedPlane(component) : default;
                    int planeWidth = width, planeHeight = height;
                    if (smoothingFactor == 0 && sampling.Is420 && component != 0)
                    {
                        plane = pixels.GetConverted420ChromaPlane(component, out planeWidth, out planeHeight);
                    }

                    int firstRow = top / (vertical * 8) * factor.Vertical;
                    for (int row = 0; row < factor.Vertical && firstRow + row < cachedHeights[component]; row++)
                    {
                        int y = firstRow + row;
                        ReadOnlySpan<float> rowPlane = plane;
                        int rowPlaneWidth = planeWidth, rowPlaneHeight = planeHeight, planeY = y * 8;
                        bool compactChroma = sampling.Is420 && component != 0;
                        if (rowPlane.IsEmpty && smoothingFactor == 0
                            && ((factor.Horizontal == horizontal && factor.Vertical == vertical) || compactChroma))
                        {
                            rowPlane = pixels.GetConvertedPlaneSegment(component, y * 8, compactChroma,
                                out rowPlaneWidth, out rowPlaneHeight, out planeY);
                        }
                        for (int x = 0; x < widths[component]; x++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            float strength = adaptive is null ? 0f : strengths[(row * adaptive.BlocksWide)
                                + (x * horizontal / factor.Horizontal)];
                            Span<short> quantized = image.Block(component, x, y);
                            if (rateDistortionLambda > 0)
                            {
                                // Retain the original AQ decisions as candidate values, then
                                // search AC only. Histogram collection and emission both read
                                // this same completed cache; the default fused path is unchanged.
                                Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                                PrepareRateDistortionBlock(samples, quantizers[component], tables[component],
                                    quantized, strength, rateDistortionLambda, component != 0, rateDistortionDiagnostics);
                                continue;
                            }
                            if (pixels.ParallelRows is { } parallelRows)
                            {
                                parallelRows.CopyBlock(component, x, y, samples);
                            }
                            else if (!rowPlane.IsEmpty && (x * 8) + 8 <= rowPlaneWidth && planeY + 8 <= rowPlaneHeight)
                            {
                                ForwardDct.TransformQuantizePlane(rowPlane, rowPlaneWidth, x * 8, planeY,
                                    quantizers[component], quantized, strength, zigZagOrdered);
                                continue;
                            }
                            else if (!rowPlane.IsEmpty && zigZagOrdered)
                            {
                                ForwardDct.TransformQuantizePaddedPlane(rowPlane, rowPlaneWidth, rowPlaneHeight,
                                    x * 8, planeY, quantizers[component], quantized, strength);
                                continue;
                            }
                            else
                            {
                                Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                                ForwardDct.TransformQuantizeBlock(samples, quantizers[component], quantized,
                                    strength, zigZagOrdered);
                                continue;
                            }
                            // Built-in DC zero-bias multipliers are zero, so the full DC
                            // coefficient does not depend on scan order or restart predictors.
                            Span<short> natural = zigZagOrdered ? naturalOrder : quantized;
                            Quantization.QuantizePrepared(samples, quantizers[component], natural,
                                adaptiveStrength: strength);
                            EntropyEncoder.ValidateAc(natural);
                            if (zigZagOrdered)
                            {
                                ZigZagOrdering.Reorder(natural, quantized);
                            }
                        }
                    }
                }
            }
            return image;
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    // Keep the opt-in search's scratch and live state out of the ordinary cache
    // loop. Conditional stack allocations otherwise change that method's frame
    // even when the experiment is disabled for every block.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PrepareRateDistortionBlock(ReadOnlySpan<float> samples,
        Quantization.PreparedTable quantizer, ReadOnlySpan<ushort> table, Span<short> quantized,
        float strength, double lambda, bool chroma, RateDistortionDiagnostics? diagnostics)
    {
        Span<float> coefficients = stackalloc float[64];
        Span<short> natural = stackalloc short[64];
        Span<short> original = stackalloc short[64];
        ForwardDct.Transform(samples, coefficients);
        Quantization.QuantizePrepared(coefficients, quantizer, natural, adaptiveStrength: strength);
        EntropyEncoder.ValidateAc(natural);
        ZigZagOrdering.Reorder(natural, quantized);
        quantized.CopyTo(original);
        int evaluations = RateDistortionQuantization.Optimize(coefficients, table, quantized, lambda, chroma);
        int changes = 0;
        for (int coefficient = 1; coefficient < 64; coefficient++)
        {
            if (quantized[coefficient] != original[coefficient])
            {
                changes++;
            }
        }
        diagnostics?.RecordBlock(evaluations, changes);
    }

    // Plans only capacity; no pixels, coefficients or image-sized arrays are read.
    internal static long PlannedBytes(int[] widths, int[] heights, int mcuRows,
        JpegSamplingFactors sampling, bool segmented)
    {
        long bytes = 0;
        for (int component = 0; component < widths.Length; component++)
        {
            long rows = Math.Min(heights[component], (long)mcuRows * sampling[component].Vertical);
            ulong length = (ulong)(widths[component] * rows * 64);
            if (segmented)
            {
                bytes += (long)(length / SegmentLength) * SegmentLength * sizeof(short);
                length %= SegmentLength;
            }
            if (length != 0)
            {
                bytes += (long)Math.Max(16UL, BitOperations.RoundUpToPowerOf2(length)) * sizeof(short);
            }
        }
        return bytes;
    }

    private bool AllocateComponent(int component, int length, long maximumBytes, CancellationToken cancellationToken)
    {
        if (this.segments is null)
        {
            short[]? rental = this.Rent(length, maximumBytes);
            if (rental is null)
            {
                return false;
            }
            this.components[component] = rental;
            return true;
        }

        int count = ((length - 1) / SegmentLength) + 1;
        short[][] chunks = new short[count][];
        this.segments[component] = chunks;
        for (int index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            short[]? rental = this.Rent(Math.Min(SegmentLength, length - (index * SegmentLength)), maximumBytes);
            if (rental is null)
            {
                return false;
            }
            chunks[index] = rental;
        }
        return true;
    }

    private short[]? Rent(int length, long maximumBytes)
    {
        short[] rental = this.pool.Rent(length);
        long bytes = (long)rental.Length * sizeof(short);
        if (bytes > maximumBytes - this.RentedBytes
            || (this.cacheBudget is not null && !this.cacheBudget.TryReserve(bytes, EncoderCacheKind.QuantizedCoefficients)))
        {
            this.pool.Return(rental);
            return null;
        }
        this.RentedBytes += bytes;
        return rental;
    }

    private Span<short> Block(int component, int x, int y)
    {
        int offset = checked(((y * this.widths[component]) + x) * 64);
        return this.segments is null ? this.components[component].AsSpan(offset, 64)
            : this.segments[component][offset / SegmentLength].AsSpan(offset % SegmentLength, 64);
    }

    internal ReadOnlySpan<short> GetBlock(int component, int x, int y)
        => this.Block(component, x, y);

    public void Dispose()
    {
        for (int component = 0; component < this.components.Length; component++)
        {
            if (this.components[component] is { } buffer)
            {
                this.components[component] = null!;
                this.pool.Return(buffer);
            }
            if (this.segments?[component] is { } chunks)
            {
                this.segments[component] = null!;
                foreach (short[]? chunk in chunks)
                {
                    if (chunk is not null)
                    {
                        this.pool.Return(chunk);
                    }
                }
            }
        }
        this.cacheBudget?.Release(this.RentedBytes, EncoderCacheKind.QuantizedCoefficients);
        this.RentedBytes = 0;
    }
}
