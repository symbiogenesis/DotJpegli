namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class PixelSourceSnapshotTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RawSnapshotOwnsSamplesAndPreservesPrecisionAndLayout(int precision)
    {
        const int width = 19, height = 3;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
        {
            int stride = (width * SimdSampleFixtures.Channels(layout)) + 7;
            foreach (JpegColorSpace output in SimdSampleFixtures.OutputSpaces(layout))
            {
                Array samples = SimdSampleFixtures.Source(precision, width, height, stride, layout);
                var original = (Array)samples.Clone();
                PixelSource source = SimdSampleFixtures.Pixels(samples, layout).WithColorSpace(output);
                var snapshot = new PixelSource.Snapshot(source, samples.Length, TestContext.Current.CancellationToken);
                Array.Clear(samples);

                PixelSource restored = snapshot.GetPixels();
                PixelSource expected = SimdSampleFixtures.Pixels(original, layout).WithColorSpace(output);
                Assert.Equal(original.Length, restored.Length);
                Assert.Equal(expected.ComponentsPerPixel, restored.ComponentsPerPixel);
                Assert.Equal(expected.ColorSpace, restored.ColorSpace);
                Assert.Equal(expected.CanManageSourceColor, restored.CanManageSourceColor);
                Assert.Null(restored.ConvertedStorage);
                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        int offset = (row * stride) + (column * expected.ComponentsPerPixel);
                        for (int component = 0; component < (expected.IsGrayscale ? 1 : 3); component++)
                        {
                            SimdSampleFixtures.AssertBits(expected.ReadSample(offset, component), restored.ReadSample(offset, component));
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EmptySamplesRemainValidSnapshotCases(int precision)
    {
        Array samples = precision switch
        {
            0 => (byte[])[],
            1 => (ushort[])[],
            _ => (float[])[]
        };
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
        {
            PixelSource source = SimdSampleFixtures.Pixels(samples, layout);
            PixelSource restored = new PixelSource.Snapshot(source, 0, TestContext.Current.CancellationToken).GetPixels();
            Assert.Equal(0, restored.Length);
            Assert.Equal(source.ComponentsPerPixel, restored.ComponentsPerPixel);
            Assert.Equal(source.ColorSpace, restored.ColorSpace);
            Assert.Equal(source.CanManageSourceColor, restored.CanManageSourceColor);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SnapshotHonorsPreCancellation(int precision)
    {
        Array samples = SimdSampleFixtures.Source(precision, 1, 1, 3, PixelComponentLayout.Rgb);
        var token = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() => CreateSnapshot(samples, token));
        Assert.Equal(token, error.CancellationToken);
    }

    private static void CreateSnapshot(Array samples, CancellationToken token)
        => _ = new PixelSource.Snapshot(SimdSampleFixtures.Pixels(samples, PixelComponentLayout.Rgb), samples.Length, token);
}
