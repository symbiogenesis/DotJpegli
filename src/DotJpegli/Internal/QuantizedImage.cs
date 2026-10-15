using System.Buffers;
using System.Numerics;

namespace DotJpegli.Internal;

/// <summary>Reuses full coefficients across histogram, spectral and refinement passes.</summary>
internal sealed class QuantizedImage : IDisposable
{
    // Keep the streaming path for very large images rather than requiring an
    // unbounded image-sized allocation. The rented arrays are returned on every exit.
    internal const long MaximumBytes = 64L * 1024 * 1024;
    private readonly short[][] components;
    private readonly int[] widths;

    private QuantizedImage(short[][] components, int[] widths)
    {
        this.components = components;
        this.widths = widths;
    }

    internal static QuantizedImage? Create(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, CancellationToken cancellationToken)
    {
        var widths = new int[tables.Length];
        var heights = new int[tables.Length];
        long rentedBytes = 0;
        for (int component = 0; component < tables.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = (width * factor.Horizontal + 8 * sampling.MaxHorizontal - 1)
                / (8 * sampling.MaxHorizontal);
            heights[component] = (height * factor.Vertical + 8 * sampling.MaxVertical - 1)
                / (8 * sampling.MaxVertical);
            long length = (long)widths[component] * heights[component] * 64;
            // Account for the shared pool's power-of-two buckets as well as
            // useful coefficients when enforcing the working-storage budget.
            rentedBytes += (long)BitOperations.RoundUpToPowerOf2((ulong)length) * sizeof(short);
        }
        if (rentedBytes > MaximumBytes)
            return null;

        var image = new QuantizedImage(new short[tables.Length][], widths);
        try
        {
            for (int component = 0; component < tables.Length; component++)
                image.components[component] = ArrayPool<short>.Shared.Rent(widths[component] * heights[component] * 64);
            Span<float> samples = stackalloc float[64];
            Span<float> transformed = stackalloc float[64];
            int horizontal = sampling.MaxHorizontal;
            int vertical = sampling.MaxVertical;
            float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
            for (int top = 0; top < height; top += vertical * 8)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pixels.ParallelRows?.PrepareRow(top, cancellationToken);
                if (adaptive is not null)
                    adaptive.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
                for (int component = 0; component < tables.Length; component++)
                {
                    JpegComponentSampling factor = sampling[component];
                    int firstRow = top / (vertical * 8) * factor.Vertical;
                    for (int row = 0; row < factor.Vertical && firstRow + row < heights[component]; row++)
                        for (int x = 0; x < widths[component]; x++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            int y = firstRow + row;
                            if (pixels.ParallelRows is { } parallelRows)
                                parallelRows.CopyBlock(component, x, y, transformed);
                            else
                            {
                                Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                                ForwardDct.Transform(samples, transformed);
                            }
                            float strength = adaptive is null ? 0f : strengths![row * adaptive.BlocksWide
                                + x * horizontal / factor.Horizontal];
                            // Built-in DC zero-bias multipliers are zero, so the full DC
                            // coefficient does not depend on scan order or restart predictors.
                            Span<short> quantized = image.Block(component, x, y);
                            Quantization.Quantize(transformed, tables[component], quantized,
                                component, distance, grayscale: pixels.IsGrayscale,
                                adaptiveQuantization: adaptive is not null, adaptiveStrength: strength,
                                adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                                rgb: pixels.IsRgb);
                            EntropyEncoder.ValidateAc(quantized);
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
        => components[component].AsSpan((y * widths[component] + x) * 64, 64);

    internal ReadOnlySpan<short> GetBlock(int component, int x, int y)
        => Block(component, x, y);

    public void Dispose()
    {
        for (int component = 0; component < components.Length; component++)
        {
            if (components[component] is { } buffer)
            {
                components[component] = null!;
                ArrayPool<short>.Shared.Return(buffer);
            }
        }
    }
}
