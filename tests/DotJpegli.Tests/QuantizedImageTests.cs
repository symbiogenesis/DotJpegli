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
    public void OrderedTinyAndOddPlaneEdgesMatchUnfusedStreamingBlocks()
    {
        foreach ((int width, int height) in new[] { (1, 1), (19, 17) })
            foreach (ChromaSubsampling subsampling in new[] { ChromaSubsampling.None, ChromaSubsampling.Quarter })
                foreach (bool adaptive in new[] { false, true })
                {
                    JpegSamplingFactors sampling = JpegSamplingFactors.FromSubsampling(subsampling);
                    int horizontal = sampling.MaxHorizontal, vertical = sampling.MaxVertical;
                    int stride = width * 3;
                    PixelSource original = new(TestImages.Gradient(width, height), PixelLayout.Rgb24);
                    PixelSource converted = sampling.Is420
                        ? original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken)
                        : original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
                    using IDisposable? storage = converted.ConvertedStorage;
                    ushort[][] tables = Enumerable.Range(0, 3)
                        .Select(component => Quantization.CreateTableForDistance(4f, component, horizontal, vertical)).ToArray();
                    float[][]? multipliers = adaptive
                        ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, horizontal, vertical, false) : null;
                    using QuantizedImage raw = QuantizedImage.Create(original, width, height, stride, sampling,
                        tables, 4f, 0, adaptive ? new(width, height, vertical) : null, multipliers,
                        TestContext.Current.CancellationToken, zigZagOrdered: true)!;
                    using QuantizedImage planar = QuantizedImage.Create(converted, width, height, stride, sampling,
                        tables, 4f, 0, adaptive ? new(width, height, vertical) : null, multipliers,
                        TestContext.Current.CancellationToken, zigZagOrdered: true)!;
                    var analysis = adaptive ? new AdaptiveQuantization(width, height, vertical) : null;
                    float[] strengths = new float[((width + 7) / 8) * vertical];
                    float[] samples = new float[64], transformed = new float[64];
                    short[] natural = new short[64], expected = new short[64];
                    for (int component = 0; component < 3; component++)
                    {
                        JpegComponentSampling factor = sampling[component];
                        int columns = (width * factor.Horizontal + horizontal * 8 - 1) / (horizontal * 8);
                        int rows = (height * factor.Vertical + vertical * 8 - 1) / (vertical * 8);
                        for (int y = 0; y < rows; y++)
                        {
                            int top = y / factor.Vertical * vertical * 8;
                            analysis?.ComputeRow(original, stride, top, tables[0][1], strengths,
                                TestContext.Current.CancellationToken);
                            for (int x = 0; x < columns; x++)
                            {
                                Downsampling.FillBlock(original, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples);
                                ForwardDct.Transform(samples, transformed);
                                float strength = !adaptive ? 0f : strengths[(y % factor.Vertical) * analysis!.BlocksWide
                                    + x * horizontal / factor.Horizontal];
                                Quantization.Quantize(transformed, tables[component], natural, component, 4f,
                                    adaptiveQuantization: adaptive, adaptiveStrength: strength,
                                    adaptiveMultipliers: multipliers is null ? default : multipliers[component]);
                                EntropyEncoder.ValidateAc(natural);
                                ZigZagOrdering.Reorder(natural, expected);
                                Assert.Equal(expected, raw.GetBlock(component, x, y).ToArray());
                                Assert.Equal(expected, planar.GetBlock(component, x, y).ToArray());
                            }
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
