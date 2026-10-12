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
    [InlineData(false, "28DE5C4D3F40CB55A73AB7B5385BECB5DD62978355CE9020302834F90DE0F1A7")]
    [InlineData(true, "6591D26B56FD1B31CBD26930978463A09D55E89B910CDA36CDB5BDCE552CE341")]
    public void DisablingAdaptiveQuantizationPreservesBaselineOutput(bool optimize, string expectedHash)
    {
        // Captured from the original managed baseline revision df35cb7.
        byte[] jpeg = JpegliEncoder.Encode(TestImages.Gradient(19, 17), 19, 17, options: new()
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
