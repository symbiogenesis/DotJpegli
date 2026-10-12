using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderCorpusTests
{
    // Broader image/quality matrix belongs to CI, not the focused local selection.
    [Theory]
    [InlineData(1, 1)]
    [InlineData(7, 9)]
    [InlineData(16, 16)]
    [InlineData(31, 33)]
    [InlineData(63, 1)]
    [InlineData(1, 63)]
    public void DimensionsQualitiesAndSamplingDecode(int width, int height)
    {
        var random = new Random(width * 1024 + height);
        byte[] pixels = new byte[width * height * 3];
        random.NextBytes(pixels);
        foreach (float quality in new[] { 0f, 1f, 50f, 85f, 99f, 100f })
            foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                foreach (bool optimize in new[] { false, true })
                    foreach (bool adaptive in new[] { false, true })
                    {
                        byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, options: new()
                        {
                            Quality = quality, Subsampling = sampling, OptimizeHuffmanTables = optimize,
                            AdaptiveQuantization = adaptive
                        }, cancellationToken: TestContext.Current.CancellationToken);
                        ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
                        Assert.Equal(width, decoded.Width);
                        Assert.Equal(height, decoded.Height);
                        Assert.Equal(pixels.Length, decoded.Data.Length);
                    }
    }
}
