namespace DotJpegli.Tests;

using Xunit;

public sealed class EncoderParallelEntropyTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void CachedMixedAndUncachedRestartScansPreserveCompleteJpegs(int mode, bool flat)
    {
        const int width = 65, height = 73;
        byte[] pixels = new byte[width * height * 3];
        new Random(81149).NextBytes(pixels);
        if (flat)
        {
            Array.Fill(pixels, (byte)127);
        }
        var options = new JpegliEncoderOptions
        {
            Distance = 1.7f,
            Subsampling = mode == 1 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
            Progressive = mode == 1,
            ProgressiveRefinementBits = mode == 1 ? 2 : 0,
            ScanScript = mode == 2 ? ProgressiveScript() : null,
            RestartInterval = mode == 1 ? 3 : 0,
            RestartRows = mode == 1 ? 0 : 1,
            OptimizeHuffmanTables = true
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24, options,
            cancellationToken: TestContext.Current.CancellationToken);
        foreach (int workers in new[] { 2, 5 })
        {
            foreach (long budget in new[] { 0L, 128L, 64L * 1024 * 1024 })
            {
                Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24,
                    options with { MaxDegreeOfParallelism = workers, EntropyTokenBudgetBytes = budget },
                    cancellationToken: TestContext.Current.CancellationToken));
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StandardHuffmanAndTinyOddEdgesKeepTheSequentialFallbackBytes(bool progressive)
    {
        const int width = 1, height = 35;
        byte[] pixels = Enumerable.Range(0, width * height).Select(index => (byte)(index * 37)).ToArray();
        var options = new JpegliEncoderOptions
        {
            Distance = 3.5f,
            Progressive = progressive,
            ProgressiveRefinementBits = progressive ? 2 : 0,
            RestartRows = 1,
            OptimizeHuffmanTables = false
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Gray8, options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height, PixelLayout.Gray8,
            options with { MaxDegreeOfParallelism = 5 }, cancellationToken: TestContext.Current.CancellationToken));
    }

    private static JpegScan[] ProgressiveScript() =>
    [
        new([2], 0, 0, 0, 1), new([0, 1], 0, 0, 0, 1),
        new([0], 1, 5, 0, 1), new([0], 6, 63, 0, 1),
        new([1], 1, 63, 0, 1), new([2], 1, 63, 0, 1),
        new([0, 1, 2], 0, 0, 1, 0),
        new([0], 1, 63, 1, 0), new([1], 1, 63, 1, 0), new([2], 1, 63, 1, 0)
    ];
}
