using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class FractionalDownsamplingTests
{
    [Theory]
    [InlineData(3, 1, 2, 1)]
    [InlineData(4, 1, 3, 1)]
    [InlineData(1, 3, 1, 2)]
    [InlineData(1, 4, 1, 3)]
    [InlineData(3, 3, 2, 2)]
    [InlineData(4, 3, 3, 2)]
    public void WeightedPixelAreasPreserveGlobalPhaseAndReplicateEdges(
        int maxH, int maxV, int componentH, int componentV)
    {
        foreach ((int width, int height) in new[] { (1, 1), (17, 19), (49, 43) })
        {
            int stride = width * 3 + 7;
            byte[] pixels = Enumerable.Repeat((byte)0xe5, stride * height).ToArray();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    for (int component = 0; component < 3; component++)
                        pixels[y * stride + x * 3 + component] = (byte)((17 * x + 29 * y + 43 * component) % 256);

            foreach (int smoothing in new[] { 0, 73 })
                foreach ((int blockX, int blockY) in new[] { (0, 0), (1, 1), (2, 2) })
                    for (int component = 0; component < 3; component++)
                    {
                        float[] actual = new float[64];
                        Downsampling.FillBlock(new PixelSource(pixels, PixelLayout.YCbCr24), width, height, stride,
                            component, blockX, blockY, maxH, maxV, componentH, componentV, actual, smoothing);
                        double[] expected = AreaBlock((x, y) => pixels[y * stride + x * 3 + component],
                            width, height, blockX, blockY, maxH, maxV, componentH, componentV, smoothing);
                        for (int index = 0; index < actual.Length; index++)
                            Assert.InRange(Math.Abs(actual[index] - expected[index]), 0, 0.0001);
                    }
        }
    }

    [Fact]
    public void ThreeToTwoRatioSplitsAlternatingPixelsAtTheirExactAreaBoundaries()
    {
        byte[] pixels = [0, 30, 90, 210, 45, 15];
        float[] actual = new float[64];
        Downsampling.FillBlock(new PixelSource(pixels, PixelLayout.Gray8), 6, 1, 6,
            0, 0, 0, 3, 1, 2, 1, actual);
        Assert.Equal(new float[] { 10 - 128, 70 - 128, 155 - 128, 25 - 128 }, actual[..4]);
        Assert.All(actual[4..8], sample => Assert.Equal(15 - 128, sample));
    }

    [Theory]
    [InlineData(3, 3, 1, 1)]
    [InlineData(4, 4, 2, 2)]
    [InlineData(4, 2, 1, 1)]
    [InlineData(2, 4, 2, 1)]
    [InlineData(2, 2, 1, 1)]
    public void IntegerRatiosRetainEveryLegacySampleBit(int maxH, int maxV, int componentH, int componentV)
    {
        const int width = 31, height = 29;
        byte[] pixels = EncoderProgressiveTests.Pattern(width, height, PixelLayout.Rgb24);
        int sampleWidth = maxH / componentH, sampleHeight = maxV / componentV;
        foreach (int smoothing in new[] { 0, 73 })
            for (int component = 0; component < 3; component++)
            {
                float[] expected = new float[64], actual = new float[64];
                Downsampling.FillBlock(pixels, width, height, width * 3, 3, PixelLayout.Rgb24,
                    component, 8 * sampleWidth, 8 * sampleHeight, sampleWidth, sampleHeight, expected, smoothing);
                Downsampling.FillBlock(new PixelSource(pixels, PixelLayout.Rgb24), width, height, width * 3,
                    component, 1, 1, maxH, maxV, componentH, componentV, actual, smoothing);
                Assert.Equal(expected.Select(BitConverter.SingleToUInt32Bits), actual.Select(BitConverter.SingleToUInt32Bits));
            }
    }

    [Fact]
    public void WordAndFractionalFloatSourcesKeepTheirPrecisionBeforeAreaAveraging()
    {
        const int width = 17, height = 19, stride = width * 3 + 5;
        ushort[] words = Enumerable.Range(0, stride * height).Select(index => (ushort)(index * 379 % 65536)).ToArray();
        float[] floats = words.Select((value, index) => (value + index % 7 / 8f) / 65535f).ToArray();
        float[] actual = new float[64];
        for (int component = 0; component < 3; component++)
        {
            Downsampling.FillBlock(new PixelSource(words, PixelComponentLayout.YCbCr), width, height, stride,
                component, 1, 1, 3, 3, 2, 2, actual);
            double[] expected = AreaBlock((x, y) => words[y * stride + x * 3 + component] / 257d,
                width, height, 1, 1, 3, 3, 2, 2);
            for (int index = 0; index < actual.Length; index++)
                Assert.InRange(Math.Abs(actual[index] - expected[index]), 0, 0.0001);

            Downsampling.FillBlock(new PixelSource(floats, PixelComponentLayout.YCbCr), width, height, stride,
                component, 1, 1, 3, 3, 2, 2, actual);
            expected = AreaBlock((x, y) => floats[y * stride + x * 3 + component] * 255d,
                width, height, 1, 1, 3, 3, 2, 2);
            for (int index = 0; index < actual.Length; index++)
                Assert.InRange(Math.Abs(actual[index] - expected[index]), 0, 0.0001);
        }
    }

    // Independent continuous-area oracle: use double rectangle coordinates and
    // pixel intersections, rather than the encoder's integer overlap grid.
    internal static double[] AreaBlock(Func<int, int, double> raw, int width, int height,
        int blockX, int blockY, int maxH, int maxV, int componentH, int componentV, int smoothing = 0)
    {
        double Raw(int x, int y) => raw(Math.Clamp(x, 0, width - 1), Math.Clamp(y, 0, height - 1));
        double Sample(int x, int y)
        {
            if (smoothing == 0) return Raw(x, y);
            double neighbors = 0;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (dx != 0 || dy != 0) neighbors += Raw(x + dx, y + dy);
            return Raw(x, y) * (1 - smoothing / 128d) + neighbors * (smoothing / 1024d);
        }

        double[] result = new double[64];
        double stepX = maxH / (double)componentH, stepY = maxV / (double)componentV;
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
            {
                double left = (blockX * 8 + column) * stepX, right = left + stepX;
                double top = (blockY * 8 + row) * stepY, bottom = top + stepY;
                double sum = 0;
                for (int y = (int)Math.Floor(top); y < Math.Ceiling(bottom); y++)
                    for (int x = (int)Math.Floor(left); x < Math.Ceiling(right); x++)
                    {
                        double overlapX = Math.Max(0, Math.Min(right, x + 1) - Math.Max(left, x));
                        double overlapY = Math.Max(0, Math.Min(bottom, y + 1) - Math.Max(top, y));
                        sum += Sample(x, y) * overlapX * overlapY;
                    }
                result[row * 8 + column] = sum / (stepX * stepY) - 128;
            }
        return result;
    }
}
