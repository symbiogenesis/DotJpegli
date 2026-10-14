using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class HuffmanCodingPlanTests
{
    [Fact]
    public void IdenticalComponentHistogramsMergeAndShareOneDcAndAcDefinition()
    {
        JpegScan[] scans = [new([0, 1, 2])];
        long[][] dc = [Histogram(0, 10000), Histogram(0, 10000), Histogram(0, 10000)];
        long[][][] ac = [[Histogram(0, 10000), Histogram(0, 10000), Histogram(0, 10000)]];
        HuffmanCodingPlan plan = HuffmanCodingPlan.Create(scans, dc, ac, false, true);
        Assert.False(plan.RequiresExtendedSequential);
        Assert.Equal(new byte[] { 0, 0, 0 }, plan.Scans[0].Selectors);
        Assert.Equal(new[] { 0, 16 }, plan.Scans[0].Definitions.Select(definition => definition.Selector));
        Assert.Same(plan.Scans[0].DcTables[0], plan.Scans[0].DcTables[2]);
        Assert.Same(plan.Scans[0].AcTables[0], plan.Scans[0].AcTables[2]);
        using var stream = new MemoryStream();
        JpegMarkerWriter.WriteHuffmanTables(stream, plan.Scans[0].Definitions);
        // Two tables, each one selector, sixteen counts and a single symbol,
        // share one marker and one two-byte length field.
        Assert.Equal(40, stream.Length);
        Assert.Equal(new byte[] { 255, 196, 0, 38 }, stream.ToArray()[..4]);
    }

    [Fact]
    public void TableHeaderCostMergesSmallDisjointHistograms()
    {
        HuffmanCodingPlan plan = HuffmanCodingPlan.Create([new([0, 1, 2])],
            [Histogram(0, 1), Histogram(1, 1), Histogram(2, 1)],
            [[Histogram(0, 1), Histogram(1, 1), Histogram(2, 1)]], false, true);
        Assert.Equal(2, plan.Scans[0].Definitions.Length);
        Assert.Equal(new byte[] { 0, 0, 0 }, plan.Scans[0].Selectors);
    }

    [Theory]
    [InlineData(true, 4, false)]
    [InlineData(false, 6, true)]
    public void SequentialClustersRespectBaselineSlotsOrRequireExtendedFrame(
        bool forceBaseline, int definitions, bool extended)
    {
        HuffmanCodingPlan plan = HuffmanCodingPlan.Create([new([0, 1, 2])],
            [Histogram(0, 10000), Histogram(1, 10000), Histogram(2, 10000)],
            [[Histogram(0, 10000), Histogram(1, 10000), Histogram(2, 10000)]], false, forceBaseline);
        Assert.Equal(definitions, plan.Scans[0].Definitions.Length);
        Assert.Equal(extended, plan.RequiresExtendedSequential);
        Assert.All(plan.Scans[0].Selectors, selector =>
        {
            Assert.InRange(selector >> 4, 0, forceBaseline ? 1 : 2);
            Assert.InRange(selector & 15, 0, forceBaseline ? 1 : 2);
        });
    }

    [Fact]
    public void FifthDistinctProgressiveAcClusterRedefinesSlotZeroAtItsFirstUse()
    {
        JpegScan[] scans =
        [
            new([0], 0, 0), new([0], 1, 2), new([0], 3, 4),
            new([0], 5, 6), new([0], 7, 8), new([0], 9, 63)
        ];
        long[][][] ac = [[new long[256]], [Histogram(0, 10000)], [Histogram(1, 10000)],
            [Histogram(2, 10000)], [Histogram(3, 10000)], [Histogram(4, 10000)]];
        HuffmanCodingPlan plan = HuffmanCodingPlan.Create(scans, [Histogram(0, 10000)], ac, true, true);
        Assert.Equal(new[] { 0, 16, 17, 18, 19 }, plan.Scans[0].Definitions.Select(definition => definition.Selector));
        Assert.Equal(new byte[] { 0, 0, 1, 2, 3, 0 }, plan.Scans.Select(scan => scan.Selectors[0]));
        Assert.All(plan.Scans[1..5], scan => Assert.Empty(scan.Definitions));
        HuffmanCodingPlan.Definition replacement = Assert.Single(plan.Scans[5].Definitions);
        Assert.Equal(16, replacement.Selector);
        Assert.Equal(new byte[] { 4 }, replacement.Table.Symbols);
        Assert.Same(replacement.Table, plan.Scans[5].AcTables[0]);
    }

    [Theory]
    [InlineData(0, ChromaSubsampling.None)]
    [InlineData(1, ChromaSubsampling.None)]
    [InlineData(2, ChromaSubsampling.Quarter)]
    public void ClusteredEntropyPreservesAllRealCoefficientsAndDecodedPixels(int level, ChromaSubsampling sampling)
    {
        const int width = 65, height = 49;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { ProgressiveLevel = level, Subsampling = sampling, RestartRows = 1 };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24,
            options with { ProgressiveLevel = 0, OptimizeHuffmanTables = false });
        byte[] actual = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0, expectedRestartRows: 1);
        JpegDocument document = JpegDocument.Read(actual, level != 0, 0,
            expectedRestartRows: 1, expectedInterleaveDc: sampling == ChromaSubsampling.None,
            expectedAcScanEnds: level == 2 ? [2, 63] : [63],
            expectedAcRefinementBits: level == 2 ? [0, 2] : level == 1 ? [1] : [0]);
        foreach ((BlockKey key, Block block) in reference.Blocks)
            if (!block.IsDummy)
                Assert.Equal(block.Coefficients, document.Blocks[key].Coefficients);
        Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(actual, ColorComponents.RedGreenBlue).Data);
    }

    private static long[] Histogram(int symbol, long count)
    {
        var frequencies = new long[256];
        frequencies[symbol] = count;
        return frequencies;
    }
}
