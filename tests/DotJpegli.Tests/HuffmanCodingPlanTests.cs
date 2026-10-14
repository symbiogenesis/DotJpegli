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

    [Fact]
    public void ExtraSequentialHuffmanSlotsRequireSof1EvenWhenEveryQuantizerFitsEightBits()
    {
        const int width = 256, height = 256;
        var pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = (y * width + x) * 3;
                pixels[offset] = 128;
                pixels[offset + 1] = (byte)(((x / 8 + y / 8) & 1) * 255);
                pixels[offset + 2] = (byte)(x / 8 * 4);
            }
        var options = new JpegliEncoderOptions
        {
            Subsampling = ChromaSubsampling.None, AdaptiveQuantization = false,
            QuantizationTables = [(ReadOnlyMemory<ushort>)Enumerable.Repeat((ushort)1, 64).ToArray()]
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.YCbCr24, options);
        byte[] extended = Encode(pixels, width, height, PixelLayout.YCbCr24, options with { ForceBaseline = false });
        AssertFrameAndSelectors(baseline, 0xc0, 1);
        AssertFrameAndSelectors(extended, 0xc1, 2);
        Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(extended, ColorComponents.RedGreenBlue).Data);
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

    private static void AssertFrameAndSelectors(byte[] jpeg, int expectedFrame, int maximumSlot)
    {
        int position = 2, frame = 0;
        while (position < jpeg.Length)
        {
            Assert.Equal(255, jpeg[position]);
            int marker = jpeg[position + 1];
            int length = (jpeg[position + 2] << 8) | jpeg[position + 3];
            int start = position + 4, end = position + 2 + length;
            if (marker is 0xc0 or 0xc1)
                frame = marker;
            if (marker == 0xdb)
                Assert.Equal(0, jpeg[start] >> 4); // All-one, eight-bit table.
            if (marker == 0xda)
            {
                Assert.Equal(expectedFrame, frame);
                int components = jpeg[start];
                for (int component = 0; component < components; component++)
                {
                    int selector = jpeg[start + 2 + component * 2];
                    Assert.InRange(selector >> 4, 0, maximumSlot);
                    Assert.InRange(selector & 15, 0, maximumSlot);
                }
                if (maximumSlot == 2)
                    Assert.Contains(Enumerable.Range(0, components),
                        component => jpeg[start + 2 + component * 2] >> 4 == 2);
                return;
            }
            position = end;
        }
        Assert.Fail("JPEG has no sequential scan.");
    }
}
