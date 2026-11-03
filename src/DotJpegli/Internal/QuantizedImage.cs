namespace DotJpegli.Internal;

using System.Buffers;
#if !NETSTANDARD2_1
using System.Numerics;
#endif

/// <summary>Reuses full coefficients across histogram, spectral and refinement passes.</summary>
internal sealed class QuantizedImage : IDisposable
{
    // Keep the streaming path for very large images rather than requiring an
    // unbounded image-sized allocation. The rented arrays are returned on every exit.
    internal const long MaximumBytes = 64L * 1024 * 1024;
    private readonly short[][] components;
    private readonly int[] widths;

    private QuantizedImage(short[][] components, int[] widths, bool zigZagOrdered)
    {
        this.components = components;
        this.widths = widths;
        this.IsZigZagOrdered = zigZagOrdered;
    }

    internal bool IsZigZagOrdered { get; }

    internal static QuantizedImage? Create(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, CancellationToken cancellationToken,
        bool zigZagOrdered = false, KernelPreference narrowZigZagKernel = KernelPreference.Automatic,
        double rateDistortionLambda = 0, RateDistortionDiagnostics? rateDistortionDiagnostics = null)
    {
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
        long rentedBytes = 0;
        for (int component = 0; component < tables.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal);
            heights[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical);
            long length = (long)widths[component] * heights[component] * 64;
            // Account for the shared pool's power-of-two buckets as well as
            // useful coefficients when enforcing the working-storage budget.
            rentedBytes += (long)BitOperations.RoundUpToPowerOf2((ulong)length) * sizeof(short);
        }
        if (rentedBytes > MaximumBytes)
        {
            return null;
        }

        var image = new QuantizedImage(new short[tables.Length][], widths, zigZagOrdered);
        try
        {
            var quantizers = new Quantization.PreparedTable[tables.Length];
            for (int component = 0; component < tables.Length; component++)
            {
                image.components[component] = ArrayPool<short>.Shared.Rent(widths[component] * heights[component] * 64);
                quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: pixels.IsRgb, narrowZigZagKernel: narrowZigZagKernel,
                    imageWidth: width, imageHeight: height);
            }
            Span<float> samples = stackalloc float[64];
            Span<float> searchCoefficients = rateDistortionLambda > 0 ? stackalloc float[64] : default;
            Span<short> naturalOrder = stackalloc short[64];
            Span<short> searchOriginal = rateDistortionLambda > 0 ? stackalloc short[64] : default;
            int horizontal = sampling.MaxHorizontal;
            int vertical = sampling.MaxVertical;
            Span<float> strengths = adaptive is null ? default : adaptive.Strengths;
            for (int top = 0; top < height; top += vertical * 8)
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
                    for (int row = 0; row < factor.Vertical && firstRow + row < heights[component]; row++)
                    {
                        for (int x = 0; x < widths[component]; x++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            int y = firstRow + row;
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
                                ForwardDct.Transform(samples, searchCoefficients);
                                Quantization.QuantizePrepared(searchCoefficients, quantizers[component], naturalOrder,
                                    adaptiveStrength: strength);
                                EntropyEncoder.ValidateAc(naturalOrder);
                                ZigZagOrdering.Reorder(naturalOrder, quantized);
                                quantized.CopyTo(searchOriginal);
                                int evaluations = RateDistortionQuantization.Optimize(searchCoefficients, tables[component],
                                    quantized, rateDistortionLambda, chroma: component != 0);
                                int changes = 0;
                                for (int coefficient = 1; coefficient < 64; coefficient++)
                                {
                                    if (quantized[coefficient] != searchOriginal[coefficient])
                                    {
                                        changes++;
                                    }
                                }
                                rateDistortionDiagnostics?.RecordBlock(evaluations, changes);
                                continue;
                            }
                            if (pixels.ParallelRows is { } parallelRows)
                            {
                                parallelRows.CopyBlock(component, x, y, samples);
                            }
                            else if (!plane.IsEmpty && (x * 8) + 8 <= planeWidth && (y * 8) + 8 <= planeHeight)
                            {
                                ForwardDct.TransformQuantizePlane(plane, planeWidth, x * 8, y * 8,
                                    quantizers[component], quantized, strength, zigZagOrdered);
                                continue;
                            }
                            else if (!plane.IsEmpty && zigZagOrdered)
                            {
                                ForwardDct.TransformQuantizePaddedPlane(plane, planeWidth, planeHeight,
                                    x * 8, y * 8, quantizers[component], quantized, strength);
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

    private Span<short> Block(int component, int x, int y)
        => this.components[component].AsSpan(((y * this.widths[component]) + x) * 64, 64);

    internal ReadOnlySpan<short> GetBlock(int component, int x, int y)
        => this.Block(component, x, y);

    public void Dispose()
    {
        for (int component = 0; component < this.components.Length; component++)
        {
            if (this.components[component] is { } buffer)
            {
                this.components[component] = null!;
                ArrayPool<short>.Shared.Return(buffer);
            }
        }
    }
}
