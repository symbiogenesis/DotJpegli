using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class AdaptiveQuantizationReuseTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ReplicatedImageEdgesMatchUnpreparedAnalysis(int sampling)
    {
        foreach (int width in new[] { 1, 9, 32 })
            foreach (int height in new[] { 1, 2, sampling * 8 + 1 })
            {
                int stride = width + 7;
                PixelSource original = new(Pattern(width, height, stride, 31), PixelLayout.Gray8);
                PixelSource prepared = original.WithConvertedPlanes(width, height, stride,
                    TestContext.Current.CancellationToken);
                using IDisposable? storage = prepared.ConvertedStorage;
                foreach (bool vector in new[] { false, true })
                {
                    var expectedAnalysis = new AdaptiveQuantization(width, height, sampling);
                    var actualAnalysis = new AdaptiveQuantization(width, height, sampling);
                    float[] expected = new float[actualAnalysis.BlocksWide * sampling];
                    float[] actual = new float[expected.Length];
                    for (int top = 0; top < height; top += sampling * 8)
                    {
                        if (vector)
                        {
                            expectedAnalysis.ComputeRowVector(original, stride, top, 4, expected,
                                TestContext.Current.CancellationToken);
                            actualAnalysis.ComputeRowVector(prepared, stride, top, 4, actual,
                                TestContext.Current.CancellationToken);
                        }
                        else
                        {
                            expectedAnalysis.ComputeRowScalar(original, stride, top, 4, expected,
                                TestContext.Current.CancellationToken);
                            actualAnalysis.ComputeRowScalar(prepared, stride, top, 4, actual,
                                TestContext.Current.CancellationToken);
                        }
                        Assert.Equal(expected.Select(BitConverter.SingleToInt32Bits),
                            actual.Select(BitConverter.SingleToInt32Bits));
                    }
                }
            }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PreparedConsecutiveAndReorderedRowsMatchFreshAnalysisBits(int sampling)
    {
        int mcuHeight = sampling * 8;
        int height = mcuHeight * 3 + 3;
        foreach (int width in new[] { 19, 67 })
        {
            int stride = width + 7;
            PixelSource pixels = new PixelSource(Pattern(width, height, stride, 0), PixelLayout.Gray8)
                .WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
            using IDisposable? storage = pixels.ConvertedStorage;
            foreach (bool vector in new[] { false, true })
            {
                var analysis = new AdaptiveQuantization(width, height, sampling);
                foreach (int row in new[] { 0, 1, 2, 3, 3, 0, 2, 1, 2, 3 })
                    AssertFresh(analysis, pixels, width, height, stride, sampling, row * mcuHeight, vector);
            }
            // Explicit scalar/vector mode changes also invalidate reuse.
            var mixed = new AdaptiveQuantization(width, height, sampling);
            for (int row = 0; row < 4; row++)
                AssertFresh(mixed, pixels, width, height, stride, sampling, row * mcuHeight, row % 2 == 0);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ChangedStorageAndStrideAndCancellationMatchFreshAnalysis(int sampling)
    {
        const int width = 19;
        int mcuHeight = sampling * 8;
        int height = 3 * mcuHeight + 3;
        int stride = width + 7;
        PixelSource first = new PixelSource(Pattern(width, height, stride, 0), PixelLayout.Gray8)
            .WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? firstStorage = first.ConvertedStorage;
        PixelSource second = new PixelSource(Pattern(width, height, stride + 3, 71), PixelLayout.Gray8)
            .WithConvertedPlanes(width, height, stride + 3, TestContext.Current.CancellationToken);
        using IDisposable? secondStorage = second.ConvertedStorage;
        var analysis = new AdaptiveQuantization(width, height, sampling);
        AssertFresh(analysis, first, width, height, stride, sampling, 0, true);
        AssertFresh(analysis, second, width, height, stride + 3, sampling, mcuHeight, true);
        // Cached reads use plane coordinates, allowing this test to change the
        // supplied stride independently of the immutable storage identity.
        AssertFresh(analysis, second, width, height, stride + 5, sampling, 2 * mcuHeight, true);
        float[] strengths = Enumerable.Repeat(-123f, analysis.BlocksWide * sampling).ToArray();
        var canceled = new CancellationToken(true);
        OperationCanceledException? error = null;
        try
        {
            analysis.ComputeRowVector(second, stride + 5, 3 * mcuHeight, 4, strengths, canceled);
        }
        catch (OperationCanceledException exception)
        {
            error = exception;
        }
        Assert.NotNull(error);
        Assert.Equal(canceled, error.CancellationToken);
        Assert.All(strengths, value => Assert.Equal(-123f, value));
        AssertFresh(analysis, second, width, height, stride + 5, sampling, 3 * mcuHeight, true);
    }

    [Fact]
    public void MutableUnpreparedSourcesAreRecomputed()
    {
        const int width = 19, height = 35, stride = 26;
        byte[] samples = Pattern(width, height, stride, 0);
        PixelSource pixels = new(samples, PixelLayout.Gray8);
        var analysis = new AdaptiveQuantization(width, height, 1);
        AssertFresh(analysis, pixels, width, height, stride, 1, 0, true);
        Pattern(width, height, stride, 91).CopyTo(samples, 0);
        AssertFresh(analysis, pixels, width, height, stride, 1, 8, true);
    }

    private static void AssertFresh(AdaptiveQuantization analysis, PixelSource pixels, int width, int height,
        int stride, int sampling, int top, bool vector)
    {
        var fresh = new AdaptiveQuantization(width, height, sampling);
        float[] expected = new float[analysis.BlocksWide * sampling];
        float[] actual = new float[expected.Length];
        if (vector)
        {
            fresh.ComputeRowVector(pixels, stride, top, 4, expected, TestContext.Current.CancellationToken);
            analysis.ComputeRowVector(pixels, stride, top, 4, actual, TestContext.Current.CancellationToken);
        }
        else
        {
            fresh.ComputeRowScalar(pixels, stride, top, 4, expected, TestContext.Current.CancellationToken);
            analysis.ComputeRowScalar(pixels, stride, top, 4, actual, TestContext.Current.CancellationToken);
        }
        for (int index = 0; index < expected.Length; index++)
            Assert.Equal(BitConverter.SingleToInt32Bits(expected[index]), BitConverter.SingleToInt32Bits(actual[index]));
    }

    private static byte[] Pattern(int width, int height, int stride, int bias)
    {
        byte[] samples = new byte[height * stride];
        for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
                samples[row * stride + column] = (byte)((row * 29 + column * 13 + row * column * 7 + bias) & 255);
        return samples;
    }
}
