using System.Security.Cryptography;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderAdaptiveQuantizationTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, PixelLayout.Rgb24)]
    [InlineData(ChromaSubsampling.HalfHorizontal, PixelLayout.Rgb24)]
    [InlineData(ChromaSubsampling.Quarter, PixelLayout.Rgb24)]
    [InlineData(ChromaSubsampling.Quarter, PixelLayout.Gray8)]
    public void BothHuffmanPassesProduceTheSameDecodedPixels(ChromaSubsampling sampling, PixelLayout layout)
    {
        // Cross several MCU rows, including bottom padding, and a partial right MCU.
        const int width = 19, height = 33;
        byte[] rgb = TestImages.Gradient(width, height);
        byte[] pixels = layout == PixelLayout.Gray8
            ? Enumerable.Range(0, width * height).Select(index => rgb[index * 3]).ToArray() : rgb;
        var options = new JpegliEncoderOptions
        {
            Quality = 85, Subsampling = sampling, AdaptiveQuantization = true, OptimizeHuffmanTables = false
        };
        byte[] standard = JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);
        byte[] optimized = JpegliEncoder.Encode(pixels, width, height, layout,
            options with { OptimizeHuffmanTables = true }, cancellationToken: TestContext.Current.CancellationToken);
        ImageResult decoded = ImageResult.FromMemory(optimized, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        Assert.Equal(ImageResult.FromMemory(standard, ColorComponents.RedGreenBlue).Data, decoded.Data);
    }

    [Theory]
    [InlineData(16, 16, false, "67E1DBDFF268E67A5F6D0655C3B909011B7FE1200D0B8B33EB2BD6107D0746C7")]
    [InlineData(16, 16, true, "2BC187175F20DF01F49200BA32515280973B796D372641749C464B050790F7F2")]
    [InlineData(19, 17, false, "202FD903163EC382260EDF412DA6A385522E7A18C84009B5ACF851FEF024B34A")]
    [InlineData(19, 17, true, "145458B6A9AAF6FD5EBF4A4AACF66C7EC8F9E528E2E2A8B97D733E715DE4812F")]
    public void UniformQuantizationOutputRemainsStable(int width, int height, bool optimize, string expectedHash)
    {
        // Standard-table fixtures retain d6554af; optimized entropy hashes were
        // recaptured after clustering at 28184c1. Streaming dummy-block coding
        // is checked independently in EncoderSamplingTests.
        byte[] jpeg = JpegliEncoder.Encode(TestImages.Gradient(width, height), width, height, options: new()
        {
            Quality = 90, Subsampling = ChromaSubsampling.Quarter,
            AdaptiveQuantization = false, OptimizeHuffmanTables = optimize
        }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(jpeg)));
    }

    [Fact]
    public void AdaptiveQuantizationIsEnabledByDefault()
    {
        byte[] pixels = TestImages.Gradient(23, 29);
        Assert.True(new JpegliEncoderOptions().AdaptiveQuantization);
        Assert.Equal(JpegliEncoder.Encode(pixels, 23, 29, cancellationToken: TestContext.Current.CancellationToken),
            JpegliEncoder.Encode(pixels, 23, 29, options: new() { AdaptiveQuantization = true },
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void SpatialAnalysisChangesMaskedDetailAndKeepsImageDecodable()
    {
        const int width = 48, height = 35;
        byte[] pixels = TestImages.Gradient(width, height);
        // Small checkerboard detail beside a smooth region exercises spatial masking.
        for (int y = 0; y < height; y++)
            for (int x = width / 2; x < width; x++)
                for (int channel = 0; channel < 3; channel++)
                {
                    int offset = (y * width + x) * 3 + channel;
                    pixels[offset] = (byte)(pixels[offset] + ((x + y) % 2 == 0 ? 4 : -4));
                }
        var options = new JpegliEncoderOptions { Quality = 70, Subsampling = ChromaSubsampling.None };
        byte[] adaptive = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        byte[] uniform = JpegliEncoder.Encode(pixels, width, height,
            options: options with { AdaptiveQuantization = false }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(adaptive.SequenceEqual(uniform));
        ImageResult decoded = ImageResult.FromMemory(adaptive, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        Assert.True(TestImages.MeanAbsoluteError(pixels, decoded.Data) < 8);
    }
}
