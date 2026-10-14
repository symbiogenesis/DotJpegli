using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderCorpusTests
{
    public static IEnumerable<TheoryDataRow<int, int, float, ChromaSubsampling, bool, bool>> CorpusCases
    {
        get
        {
            // Discovery only enumerates scalar options. Image creation and encoding
            // stay in the test so each combination can be scheduled independently.
            foreach ((int width, int height) in new[] { (1, 1), (7, 9), (16, 16), (31, 33), (63, 1), (1, 63) })
                foreach (float quality in new[] { 0f, 1f, 50f, 85f, 99f, 100f })
                    foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                        foreach (bool optimize in new[] { false, true })
                            foreach (bool adaptive in new[] { false, true })
                                yield return new(width, height, quality, sampling, optimize, adaptive);
        }
    }

    // Broader image/quality matrix belongs to CI, not the focused local selection.
    [Theory]
    [MemberData(nameof(CorpusCases))]
    public void DimensionsQualitiesAndSamplingDecode(int width, int height, float quality,
        ChromaSubsampling sampling, bool optimize, bool adaptive)
    {
        var random = new Random(width * 1024 + height);
        byte[] pixels = new byte[width * height * 3];
        random.NextBytes(pixels);
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
