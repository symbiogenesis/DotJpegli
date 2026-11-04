namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class SmoothedDownsamplingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(37)]
    [InlineData(100)]
    public void EveryIntegerFactorPreservesScalarBitsAtInteriorAndPaddedEdges(int smoothing)
    {
        const int width = 37, height = 35, stride = (width * 3) + 11;
        var pixels = new PixelSource((byte[])SimdSampleFixtures.Source(0, width, height, stride,
            PixelComponentLayout.Rgb), PixelLayout.Rgb24);
        for (int sampleWidth = 1; sampleWidth <= 4; sampleWidth++)
        {
            for (int sampleHeight = 1; sampleHeight <= 4; sampleHeight++)
            {
                for (int component = 0; component < 3; component++)
                {
                    foreach ((int left, int top) in new[] { (1, 1), (-2, -1), (width - 3, height - 2) })
                    {
                        AssertBlock(pixels, width, height, stride, component, left, top,
                            sampleWidth, sampleHeight, smoothing);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EveryLayoutPrecisionAndColorConversionPreservesScalarBits(int kind)
    {
        const int width = 11, height = 9;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
        {
            int stride = (width * SimdSampleFixtures.Channels(layout)) + 7;
            PixelSource source = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(kind, width, height, stride, layout), layout);
            foreach (JpegColorSpace output in SimdSampleFixtures.OutputSpaces(layout))
            {
                PixelSource pixels = source.WithColorSpace(output);
                foreach ((int sampleWidth, int sampleHeight) in new[] { (1, 1), (2, 2), (3, 4), (4, 3) })
                {
                    for (int component = 0; component < (pixels.IsGrayscale ? 1 : 3); component++)
                    {
                        AssertBlock(pixels, width, height, stride, component, width - 3, height - 2,
                            sampleWidth, sampleHeight, 73);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Xyb)]
    public void PreparedRowsPreserveSmoothingAcrossActualAndPaddedPixels(JpegColorSpace output)
    {
        const int width = 19, height = 17, stride = (width * 4) + 11;
        PixelSource source = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(1, width, height, stride,
            PixelComponentLayout.Bgra), PixelComponentLayout.Bgra).WithColorSpace(output);
        PixelSource cached = source.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? storage = cached.ConvertedStorage;
        foreach ((int sampleWidth, int sampleHeight) in new[] { (1, 1), (2, 2), (3, 4), (4, 3) })
        {
            for (int component = 0; component < 3; component++)
            {
                AssertBlock(cached, width, height, stride, component, width - 3, height - 2,
                    sampleWidth, sampleHeight, 37);
            }
        }
    }

    [Fact]
    public void TinyImageAndExtendedFloatCancellationKeepOriginalSummationOrder()
    {
        float[] values = [-0f, 1e20f, -1e20f, float.Epsilon, -float.Epsilon, 0f, 0.125f, -0.25f, 1.25f,
            1e-20f, -1e-20f, -0f, -1e20f, 1e20f, 0f, 0.5f, 0.75f, 1f];
        PixelSource pixels = new PixelSource(values, PixelComponentLayout.Rgb).WithColorSpace(JpegColorSpace.Rgb);
        for (int component = 0; component < 3; component++)
        {
            for (int factor = 1; factor <= 4; factor++)
            {
                AssertBlock(pixels, 3, 2, 9, component, -1, -1, factor, factor, 37);
            }
        }
        var single = new PixelSource([17], PixelLayout.Gray8);
        AssertBlock(single, 1, 1, 1, 0, -1, -1, 4, 4, 100);
    }

    private static void AssertBlock(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight, int smoothing)
    {
        float[] expected = Enumerable.Repeat(-54321.125f, 71).ToArray();
        float[] actual = (float[])expected.Clone(), dispatched = (float[])expected.Clone();
        Downsampling.FillBlockScalar(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, expected, smoothing);
        SmoothedDownsampling.FillBlock(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, actual, smoothing);
        Downsampling.FillBlock(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, dispatched, smoothing);
        for (int index = 0; index < expected.Length; index++)
        {
            SimdSampleFixtures.AssertBits(expected[index], actual[index]);
            SimdSampleFixtures.AssertBits(expected[index], dispatched[index]);
        }
    }
}
