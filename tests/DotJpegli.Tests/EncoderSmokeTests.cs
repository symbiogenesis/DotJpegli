using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderSmokeTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, true)]
    [InlineData(ChromaSubsampling.HalfHorizontal, false)]
    [InlineData(ChromaSubsampling.Quarter, true)]
    public void OddSizedColorImageDecodes(ChromaSubsampling sampling, bool optimize)
    {
        const int width = 19, height = 17;
        byte[] pixels = TestImages.Gradient(width, height);
        byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, options: new()
        {
            Quality = 90, Subsampling = sampling, OptimizeHuffmanTables = optimize
        }, cancellationToken: TestContext.Current.CancellationToken);
        ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        double error = TestImages.MeanAbsoluteError(pixels, decoded.Data);
        Assert.True(error < 7, $"Mean absolute RGB error: {error}");
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        Assert.Equal(new byte[] { 0xff, 0xd9 }, jpeg[^2..]);
    }

    [Fact]
    public void GrayscalePreservesNeutralValues()
    {
        byte[] pixels = Enumerable.Repeat((byte)128, 13 * 9).ToArray();
        byte[] jpeg = JpegliEncoder.Encode(pixels, 13, 9, PixelLayout.Gray8,
            cancellationToken: TestContext.Current.CancellationToken);
        ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.Grey);
        Assert.Equal(13, decoded.Width);
        Assert.Equal(9, decoded.Height);
        Assert.All(decoded.Data, value => Assert.InRange(value, (byte)127, (byte)129));
    }

    [Fact]
    public void OptimizedTablesReduceConstantImageSize()
    {
        byte[] pixels = Enumerable.Repeat((byte)128, 64 * 64 * 3).ToArray();
        byte[] standard = JpegliEncoder.Encode(pixels, 64, 64, options: new() { OptimizeHuffmanTables = false },
            cancellationToken: TestContext.Current.CancellationToken);
        byte[] optimized = JpegliEncoder.Encode(pixels, 64, 64, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(optimized.Length < standard.Length);
        Assert.Equal(ImageResult.FromMemory(standard).Data, ImageResult.FromMemory(optimized).Data);
    }
}

internal static class TestImages
{
    public static byte[] Gradient(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = (y * width + x) * 3;
                pixels[offset] = (byte)(32 + 180 * x / Math.Max(1, width - 1));
                pixels[offset + 1] = (byte)(32 + 180 * y / Math.Max(1, height - 1));
                pixels[offset + 2] = (byte)(48 + 140 * (x + y) / Math.Max(1, width + height - 2));
            }
        }
        return pixels;
    }

    public static double MeanAbsoluteError(byte[] expected, byte[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        long sum = 0;
        for (int i = 0; i < expected.Length; i++)
            sum += Math.Abs(expected[i] - actual[i]);
        return sum / (double)expected.Length;
    }
}
