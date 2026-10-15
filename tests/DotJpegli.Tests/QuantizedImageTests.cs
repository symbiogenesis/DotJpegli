using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class QuantizedImageTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void CachedBlocksMatchStreamingQuantizationWithIrregularSampling(int mode, bool adaptive)
    {
        const int width = 37, height = 35;
        JpegSamplingFactors sampling = mode switch
        {
            0 => new(new(2, 2), new(1, 1), new(1, 1)),
            1 => new(new(3, 2), new(2, 1), new(1, 1)),
            _ => new(new(2, 1), new(1, 2), new(1, 1))
        };
        byte[] input = TestImages.Gradient(width, height);
        var pixels = new PixelSource(input, PixelLayout.Rgb24);
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTableForDistance(1.45f, component)).ToArray();
        float[][]? multipliers = adaptive ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, false) : null;
        var analysis = adaptive ? new AdaptiveQuantization(width, height, sampling.MaxVertical) : null;
        using QuantizedImage image = QuantizedImage.Create(pixels, width, height, width * 3, sampling, tables,
            1.45f, 37, analysis, multipliers, TestContext.Current.CancellationToken)!;
        Assert.NotNull(image);
        var strengths = new float[((width + 7) / 8) * sampling.MaxVertical];
        var samples = new float[64];
        var transformed = new float[64];
        var expected = new short[64];
        var actual = new short[64];
        for (int component = 0; component < 3; component++)
        {
            JpegComponentSampling factor = sampling[component];
            int columns = (width * factor.Horizontal + sampling.MaxHorizontal * 8 - 1) / (sampling.MaxHorizontal * 8);
            int rows = (height * factor.Vertical + sampling.MaxVertical * 8 - 1) / (sampling.MaxVertical * 8);
            for (int y = 0; y < rows; y++)
            {
                int top = y / factor.Vertical * sampling.MaxVertical * 8;
                analysis?.ComputeRow(pixels, width * 3, top, tables[0][1], strengths, TestContext.Current.CancellationToken);
                for (int x = 0; x < columns; x++)
                {
                    Downsampling.FillBlock(pixels, width, height, width * 3, component, x, y,
                        sampling.MaxHorizontal, sampling.MaxVertical, factor.Horizontal, factor.Vertical, samples, 37);
                    ForwardDct.Transform(samples, transformed);
                    float strength = !adaptive ? 0f : strengths[(y % factor.Vertical) * analysis!.BlocksWide
                        + x * sampling.MaxHorizontal / factor.Horizontal];
                    Quantization.Quantize(transformed, tables[component], expected, component, 1.45f,
                        adaptiveQuantization: adaptive, adaptiveStrength: strength,
                        adaptiveMultipliers: multipliers is null ? default : multipliers[component]);
                    image.GetBlock(component, x, y).CopyTo(actual);
                    Assert.Equal(expected, actual);
                }
            }
        }
    }

    [Fact]
    public void OversizedCacheFallsBackBeforeReadingOrAllocatingImageStorage()
    {
        ushort[][] tables = [new ushort[64], new ushort[64], new ushort[64]];
        Assert.Null(QuantizedImage.Create(new PixelSource([], PixelLayout.Rgb24), 65535, 65535, 65535 * 3,
            JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None), tables, 1f, 0,
            null, null, TestContext.Current.CancellationToken));
    }
}
