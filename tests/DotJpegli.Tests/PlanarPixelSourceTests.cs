using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class PlanarPixelSourceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ConvertedPlanesPreserveEverySampleAndReplicatedRowForEveryLayout(int precision)
    {
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
            foreach ((int width, int height) in new[] { (1, 1), (19, 17) })
            {
                int stride = width * SimdSampleFixtures.Channels(layout) + 11;
                Array source = SimdSampleFixtures.Source(precision, width, height, stride, layout);
                PixelSource pixels = SimdSampleFixtures.Pixels(source, layout);
                foreach (JpegColorSpace output in SimdSampleFixtures.OutputSpaces(layout))
                    AssertConverted(pixels.WithColorSpace(output), width, height, stride);
                Assert.Equal(source, SimdSampleFixtures.Source(precision, width, height, stride, layout));
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void IccManagedPlanesPreserveNormalizedSourcePrecisionAndOutputTransform(int precision)
    {
        const int width = 19, height = 3;
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba,
                PixelComponentLayout.Bgra, PixelComponentLayout.Gray })
            foreach (JpegColorSpace output in new[]
                { JpegColorSpace.YCbCr, JpegColorSpace.Rgb, JpegColorSpace.Xyb, JpegColorSpace.Grayscale })
            {
                bool sourceGray = layout == PixelComponentLayout.Gray;
                bool destinationGray = output == JpegColorSpace.Grayscale;
                IccColorTransform transform = IccColorTransform.Create(
                    sourceGray ? JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile() : JpegliIccProfiles.CreateLinearSrgbProfile(),
                    destinationGray ? JpegliIccProfiles.CreateSrgbGrayscaleProfile() : JpegliIccProfiles.CreateSrgbProfile(),
                    sourceGray, destinationGray, IccRenderingIntent.RelativeColorimetric,
                    TestContext.Current.CancellationToken);
                int stride = width * SimdSampleFixtures.Channels(layout) + 11;
                PixelSource pixels = SimdSampleFixtures.Pixels(
                    SimdSampleFixtures.Source(precision, width, height, stride, layout), layout)
                    .WithColorSpace(output).WithColorTransform(transform);
                AssertConverted(pixels, width, height, stride);
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CachedScalarAndVectorDownsamplingPreserveSmoothingAndFractionalSampling(int precision)
    {
        const int width = 19, height = 17, stride = width * 4 + 11;
        PixelSource pixels = SimdSampleFixtures.Pixels(
            SimdSampleFixtures.Source(precision, width, height, stride, PixelComponentLayout.Bgra), PixelComponentLayout.Bgra);
        PixelSource cached = pixels.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        float[] expected = new float[64], scalar = new float[64], vector = new float[64];
        for (int component = 0; component < 3; component++)
            foreach ((int sx, int sy) in new[] { (1, 1), (2, 2), (3, 4), (4, 3) })
                foreach (int smoothing in new[] { 0, 73 })
                {
                    Downsampling.FillBlockScalar(pixels, width, height, stride, component, width - 3, height - 2,
                        sx, sy, expected, smoothing);
                    Downsampling.FillBlockScalar(cached, width, height, stride, component, width - 3, height - 2,
                        sx, sy, scalar, smoothing);
                    Downsampling.FillBlockVector(cached, width, height, stride, component, width - 3, height - 2,
                        sx, sy, vector, smoothing);
                    AssertBits(expected, scalar);
                    AssertBits(expected, vector);

                    Downsampling.FillBlock(pixels, width, height, stride, component, 1, 1, 3, 4, 2, 3, expected, smoothing);
                    Downsampling.FillBlock(cached, width, height, stride, component, 1, 1, 3, 4, 2, 3, vector, smoothing);
                    AssertBits(expected, vector);
                }
    }

    [Fact]
    public void ConvertedSnapshotOwnsSamplesAndPreservesOriginalLengthAndMetadata()
    {
        const int width = 19, height = 3, stride = width * 4 + 11;
        float[] samples = (float[])SimdSampleFixtures.Source(2, width, height, stride, PixelComponentLayout.Bgra);
        PixelSource pixels = new PixelSource(samples, PixelComponentLayout.Bgra).WithColorSpace(JpegColorSpace.Xyb);
        PixelSource cached = pixels.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        var snapshot = new PixelSource.Snapshot(cached, samples.Length, TestContext.Current.CancellationToken);
        float[] expected = new float[32], actual = new float[32];
        cached.ReadRowVector(width, height, stride, 2, -2, 1, expected);
        Array.Fill(samples, float.NaN);
        PixelSource restored = snapshot.GetPixels();
        Assert.Equal(samples.Length, restored.Length);
        Assert.Equal(4, restored.ComponentsPerPixel);
        Assert.True(restored.IsXyb);
        restored.ReadRowVector(width, height, stride, 2, -2, 1, actual);
        AssertBits(expected, actual);
    }

    [Fact]
    public void WiderFiniteFloatValuesAndSignedZeroPreserveBits()
    {
        float[] source = [-0f, 0f, 1f, -0.1f, 1.1f, 0.25f, -0f, -0f, -0f];
        foreach (JpegColorSpace output in new[] { JpegColorSpace.Rgb, JpegColorSpace.YCbCr, JpegColorSpace.Grayscale })
            AssertConverted(new PixelSource(source, PixelComponentLayout.Rgb).WithColorSpace(output), 3, 1, 9);
    }

    [Fact]
    public void ConversionHonorsPreCancellation()
    {
        var token = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() => ConvertCanceled(token));
        Assert.Equal(token, error.CancellationToken);
    }

    [Fact]
    public void PlanesLargerThanMemoryLimitRetainTheStreamingReader()
    {
        // The size guard must run before allocating or reading any source samples.
        PixelSource pixels = new PixelSource(ReadOnlySpan<byte>.Empty, PixelLayout.Rgb24);
        PixelSource streaming = pixels.WithConvertedPlanes(4096, 4096, 4096 * 3,
            TestContext.Current.CancellationToken);
        Assert.Equal(0, streaming.Length);
        Assert.False(streaming.IsGrayscale);
    }

    private static void ConvertCanceled(CancellationToken token)
        => new PixelSource(new byte[3], PixelLayout.Rgb24).WithConvertedPlanes(1, 1, 3, token);

    private static void AssertConverted(PixelSource pixels, int width, int height, int stride)
    {
        PixelSource cached = pixels.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        PixelSource restored = new PixelSource.Snapshot(cached, pixels.Length, TestContext.Current.CancellationToken).GetPixels();
        Assert.Equal(pixels.Length, cached.Length);
        int components = pixels.IsGrayscale ? 1 : 3;
        float[] expected = new float[32], scalar = new float[32], vector = new float[32];
        for (int component = 0; component < components; component++)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * pixels.ComponentsPerPixel;
                    float sample = pixels.ReadSample(offset, component);
                    SimdSampleFixtures.AssertBits(sample, cached.ReadSample(offset, component));
                    SimdSampleFixtures.AssertBits(sample, cached.ReadSampleAt(x, y, stride, component));
                    SimdSampleFixtures.AssertBits(sample, restored.ReadSample(offset, component));
                }
            foreach (int x in new[] { -2, 0, width - 2, width + 2 })
                foreach (int y in new[] { -1, height - 1, height + 1 })
                {
                    pixels.ReadRowScalar(width, height, stride, component, x, y, expected);
                    cached.ReadRowScalar(width, height, stride, component, x, y, scalar);
                    cached.ReadRowVector(width, height, stride, component, x, y, vector);
                    AssertBits(expected, scalar);
                    AssertBits(expected, vector);
                }
        }
    }

    private static void AssertBits(float[] expected, float[] actual)
    {
        for (int index = 0; index < expected.Length; index++)
            SimdSampleFixtures.AssertBits(expected[index], actual[index]);
    }
}
