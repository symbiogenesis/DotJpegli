namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class PartialConvertedPlaneTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SegmentsRetainExactRowsAndUseRawSamplesAfterTheCachedPrefix(bool compact, bool complete)
    {
        const int width = 257, stride = width * 3;
        int height = compact || !complete ? 529 : 301;
        long limit = compact ? complete ? 928 * 1024 : 640 * 1024 : complete ? 995000 : 1024 * 1024;
        PixelSource original = new(TestImages.Gradient(width, height), PixelLayout.Rgb24);
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(limit, diagnostics);
        PixelSource cached = compact
            ? original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken, budget, allowPartial: true)
            : original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken, budget, allowPartial: true);
        using (cached.ConvertedStorage)
        {
            Assert.NotNull(cached.ConvertedStorage);
            Assert.Equal(complete, cached.HasCompleteConvertedPlanes);
            Assert.InRange(cached.ConvertedRows, 16, height);
            Assert.Equal(cached.ConvertedRows, diagnostics.ConvertedSampleCachedRows);
            Assert.Equal(height, diagnostics.ConvertedSampleTotalRows);
            Assert.True(cached.GetConvertedPlane(0).IsEmpty);
            Assert.True(cached.GetConverted420ChromaPlane(1, out _, out _).IsEmpty);
            if (!complete)
            {
                Assert.True(cached.ConvertedRows < height);
                Assert.Equal(0, cached.ConvertedRows % 16);
            }
            float[] expected = new float[width + 4], actual = new float[width + 4];
            foreach (int y in new[] { -1, 0, 239, 240, cached.ConvertedRows - 1, cached.ConvertedRows, height - 1, height })
            {
                for (int component = 0; component < 3; component++)
                {
                    original.ReadRowScalar(width, height, stride, component, -2, y, expected);
                    cached.ReadRowScalar(width, height, stride, component, -2, y, actual);
                    AssertBits(expected, actual);
                    cached.ReadRowVector(width, height, stride, component, -2, y, actual);
                    AssertBits(expected, actual);
                }
            }
            for (int component = 0; component < 3; component++)
            {
                for (int y = 0; y < height; y++)
                {
                    foreach (int x in new[] { 0, width / 2, width - 1 })
                    {
                        float expectedSample = original.ReadSampleAt(x, y, stride, component);
                        SimdSampleFixtures.AssertBits(expectedSample, cached.ReadSampleAt(x, y, stride, component));
                        SimdSampleFixtures.AssertBits(expectedSample, cached.ReadSample((y * stride) + (x * 3), component));
                    }
                }
            }
            Assert.InRange(diagnostics.PeakBytes, 1, limit);
        }
        Assert.Equal(0, diagnostics.CurrentBytes);
        Assert.Equal(limit, budget.AvailableBytes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SegmentWindowsKeepBlockBoundariesAndRealImagePadding(bool compact)
    {
        const int width = 257, height = 529, stride = width * 3;
        PixelSource original = new(TestImages.Gradient(width, height), PixelLayout.Rgb24);
        var budget = new EncoderCacheBudget(compact ? 928 * 1024 : 1024 * 1024);
        PixelSource cached = compact
            ? original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken, budget, allowPartial: true)
            : original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken, budget, allowPartial: true);
        using IDisposable? storage = cached.ConvertedStorage;
        float[] expected = new float[64];
        for (int component = 0; component < 3; component++)
        {
            int scale = compact && component != 0 ? 2 : 1;
            int rows = scale == 1 ? height : (height + 15) / 16 * 8;
            for (int top = 0; top < rows; top += 8)
            {
                ReadOnlySpan<float> plane = cached.GetConvertedPlaneSegment(component, top, compact && component != 0,
                    out int planeWidth, out int planeHeight, out int localRow);
                if (plane.IsEmpty)
                {
                    Assert.True(top * scale >= cached.ConvertedRows);
                    continue;
                }
                Assert.Equal(0, localRow % 8);
                Assert.True(localRow + 8 <= planeHeight || (cached.HasCompleteConvertedPlanes && top + 8 > rows));
                foreach (int left in new[] { 0, (planeWidth - 1) / 8 * 8 })
                {
                    Downsampling.FillBlock(original, width, height, stride, component, left / 8, top / 8,
                        scale, scale, 1, 1, expected);
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            float sample = plane[(Math.Min(localRow + y, planeHeight - 1) * planeWidth) + Math.Min(left + x, planeWidth - 1)] - 128f;
                            SimdSampleFixtures.AssertBits(expected[(y * 8) + x], sample);
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
    public void PartialFullResolutionPlanesPreserveSourcePrecision(int precision)
    {
        const int width = 65, height = 67, stride = (width * 4) + 7;
        PixelSource original = SimdSampleFixtures.Pixels(
            SimdSampleFixtures.Source(precision, width, height, stride, PixelComponentLayout.Bgra), PixelComponentLayout.Bgra);
        var budget = new EncoderCacheBudget(32768);
        PixelSource cached = original.WithConvertedPlanes(width, height, stride,
            TestContext.Current.CancellationToken, budget, allowPartial: true);
        using IDisposable? storage = cached.ConvertedStorage;
        Assert.NotNull(storage);
        Assert.False(cached.HasCompleteConvertedPlanes);
        for (int y = 0; y < height; y++)
        {
            for (int component = 0; component < 3; component++)
            {
                SimdSampleFixtures.AssertBits(original.ReadSampleAt(width - 1, y, stride, component),
                    cached.ReadSampleAt(width - 1, y, stride, component));
            }
        }
    }

    private static void AssertBits(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            SimdSampleFixtures.AssertBits(expected[index], actual[index]);
        }
    }
}
