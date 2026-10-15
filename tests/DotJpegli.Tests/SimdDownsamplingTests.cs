using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SimdDownsamplingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void PreparedInteriorBlocksPreserveScalarBitsAndOutputBoundaries(int kind)
    {
        const int width = 41, height = 35, stride = width * 4 + 13;
        PixelSource pixels = SimdSampleFixtures.Pixels(
            SimdSampleFixtures.Source(kind, width, height, stride, PixelComponentLayout.Bgra), PixelComponentLayout.Bgra)
            .WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? storage = pixels.ConvertedStorage;
        foreach (int ratio in new[] { 1, 2 })
            for (int component = 0; component < 3; component++)
                foreach ((int left, int top) in new[]
                    { (0, 0), (8, 8), (width - 8 * ratio, height - 8 * ratio), (width - 5, height - 3), (-1, -1) })
                    AssertBlock(pixels, width, height, stride, component, left, top, ratio, ratio, 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AllUnsmoothedRatiosPreserveScalarBitsForEverySourcePrecision(int kind)
    {
        foreach (PixelComponentLayout layout in new[] { PixelComponentLayout.Rgb, PixelComponentLayout.Bgra })
            foreach ((int width, int height) in new[] { (1, 1), (35, 33) })
            {
                int stride = width * SimdSampleFixtures.Channels(layout) + 13;
                PixelSource pixels = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(kind, width, height, stride, layout), layout);
                for (int sampleWidth = 1; sampleWidth <= 4; sampleWidth++)
                    for (int sampleHeight = 1; sampleHeight <= 4; sampleHeight++)
                        for (int component = 0; component < 3; component++)
                            foreach ((int left, int top) in new[] { (0, 0), (width - 5, height - 7), (-2, -1) })
                                AssertBlock(pixels, width, height, stride, component, left, top, sampleWidth, sampleHeight, 0);
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NullAndNonlinearTransformsAndSmoothingPreserveScalarBits(int kind)
    {
        const int width = 19, height = 17;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
        {
            int stride = width * SimdSampleFixtures.Channels(layout) + 11;
            PixelSource source = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(kind, width, height, stride, layout), layout);
            foreach (JpegColorSpace colorSpace in SimdSampleFixtures.OutputSpaces(layout))
            {
                PixelSource pixels = source.WithColorSpace(colorSpace);
                foreach ((int sampleWidth, int sampleHeight) in new[] { (1, 1), (2, 2), (3, 4), (4, 3) })
                    foreach (int smoothing in new[] { 0, 73 })
                        for (int component = 0; component < (pixels.IsGrayscale ? 1 : 3); component++)
                            AssertBlock(pixels, width, height, stride, component, width - 3, height - 2,
                                sampleWidth, sampleHeight, smoothing);
            }
        }
    }

    [Theory]
    [InlineData(1, 0xC1A03C18u)]
    [InlineData(2, 0xC21679FAu)]
    public void VectorQuarterSamplingMatchesIndependentNativePairwiseFixture(int component, uint expectedBits)
    {
        // Independently compiled pinned C++ fixture from DownsamplingTests.
        byte[] rgb = [59, 24, 93, 14, 189, 158, 12, 217, 15, 139, 140, 27];
        float[] samples = new float[64];
        Downsampling.FillBlockVector(new PixelSource(rgb, PixelLayout.Rgb24), 2, 2, 6, component,
            0, 0, 2, 2, samples);
        Assert.Equal(expectedBits, BitConverter.SingleToUInt32Bits(samples[0]));
    }

    private static void AssertBlock(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight, int smoothing)
    {
        float[] scalar = Enumerable.Repeat(-54321.125f, 71).ToArray();
        float[] vector = (float[])scalar.Clone();
        float[] dispatched = (float[])scalar.Clone();
        Downsampling.FillBlockScalar(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, scalar, smoothing);
        Downsampling.FillBlockVector(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, vector, smoothing);
        Downsampling.FillBlock(pixels, width, height, stride, component, left, top,
            sampleWidth, sampleHeight, dispatched, smoothing);
        for (int index = 0; index < scalar.Length; index++)
        {
            SimdSampleFixtures.AssertBits(scalar[index], vector[index]);
            SimdSampleFixtures.AssertBits(scalar[index], dispatched[index]);
        }
    }
}
