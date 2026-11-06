namespace DotJpegli.Tests;

using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class HuffmanCodingPlanTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(255)]
    public void NegativeDcAndAcFrequenciesAreRejectedAtHistogramBoundaries(int symbol)
    {
        long[] invalid = Histogram(symbol, symbol == 0 ? long.MinValue : -1L);
        _ = Assert.Throws<ArgumentException>(() => HuffmanCodingPlan.Create([new([0])],
            [invalid], [[Histogram(0, 1)]], false, true));
        _ = Assert.Throws<ArgumentException>(() => HuffmanCodingPlan.Create([new([0])],
            [Histogram(0, 1)], [[invalid]], false, true));
    }

    [Fact]
    public void EmptyHistogramsUseFallbackTablesWithoutDefinitions()
    {
        var plan = HuffmanCodingPlan.Create([new([0])],
            [new long[256]], [[new long[256]]], false, true);
        HuffmanCodingPlan.ScanCoding scan = Assert.Single(plan.Scans);
        Assert.Empty(scan.Definitions);
        Assert.Equal(new byte[] { 0 }, scan.Selectors);
        Assert.Equal(new byte[] { 0 }, Assert.Single(scan.DcTables).Symbols);
        Assert.Same(scan.DcTables[0], Assert.Single(scan.AcTables));
    }

    [Fact]
    public void LargestPositiveFrequencyAtLastSymbolProducesTableDefinitions()
    {
        var plan = HuffmanCodingPlan.Create([new([0])],
            [Histogram(255, long.MaxValue)], [[Histogram(255, long.MaxValue)]], false, true);
        Assert.Equal([0, 16], plan.Scans[0].Definitions.Select(definition => definition.Selector));
        Assert.All(plan.Scans[0].Definitions, definition => Assert.Equal(new byte[] { 255 }, definition.Table.Symbols));
    }

    [Fact]
    public void IdenticalComponentHistogramsMergeAndShareOneDcAndAcDefinition()
    {
        JpegScan[] scans = [new([0, 1, 2])];
        long[][] dc = [Histogram(0, 10000), Histogram(0, 10000), Histogram(0, 10000)];
        long[][][] ac = [[Histogram(0, 10000), Histogram(0, 10000), Histogram(0, 10000)]];
        var plan = HuffmanCodingPlan.Create(scans, dc, ac, false, true);
        Assert.False(plan.RequiresExtendedSequential);
        Assert.Equal(new byte[] { 0, 0, 0 }, plan.Scans[0].Selectors);
        Assert.Equal([0, 16], plan.Scans[0].Definitions.Select(definition => definition.Selector));
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
        var plan = HuffmanCodingPlan.Create([new([0, 1, 2])],
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
        var plan = HuffmanCodingPlan.Create([new([0, 1, 2])],
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
        var plan = HuffmanCodingPlan.Create(scans, [Histogram(0, 10000)], ac, true, true);
        Assert.Equal([0, 16, 17, 18, 19], plan.Scans[0].Definitions.Select(definition => definition.Selector));
        Assert.Equal(new byte[] { 0, 0, 1, 2, 3, 0 }, plan.Scans.Select(scan => scan.Selectors[0]));
        Assert.All(plan.Scans[1..5], scan => Assert.Empty(scan.Definitions));
        HuffmanCodingPlan.Definition replacement = Assert.Single(plan.Scans[5].Definitions);
        Assert.Equal(16, replacement.Selector);
        Assert.Equal(new byte[] { 4 }, replacement.Table.Symbols);
        Assert.Same(replacement.Table, plan.Scans[5].AcTables[0]);
    }

    [Fact]
    public void FutureAwareEvictionRetainsActiveSlotNeededByUpcomingScan()
    {
        // Scans 1..4 fill AC slots 0..3 with distinct histograms for symbols 0..3.
        // Scan 5 introduces a new histogram (symbol 4), while Scan 6 reuses symbol 0's histogram!
        // Blind round-robin eviction would evict slot 0 at Scan 5 and force another redefinition at Scan 6;
        // future-aware eviction preserves slot 0 and evicts slot 1 instead.
        JpegScan[] scans =
        [
            new([0], 0, 0), new([0], 1, 2), new([0], 3, 4),
            new([0], 5, 6), new([0], 7, 8), new([0], 9, 10), new([0], 11, 63)
        ];
        long[][][] ac =
        [
            [new long[256]],
            [Histogram(0, 10000)],
            [Histogram(1, 10000)],
            [Histogram(2, 10000)],
            [Histogram(3, 10000)],
            [Histogram(4, 10000)],
            [Histogram(0, 10000)]
        ];
        var plan = HuffmanCodingPlan.Create(scans, [Histogram(0, 10000)], ac, true, true);
        Assert.Equal(new byte[] { 0, 0, 1, 2, 3, 1, 0 }, plan.Scans.Select(scan => scan.Selectors[0]));
        _ = Assert.Single(plan.Scans[5].Definitions);
        Assert.Empty(plan.Scans[6].Definitions);
    }

    [Fact]
    public void ExtraSequentialHuffmanSlotsRequireSof1EvenWhenEveryQuantizerFitsEightBits()
    {
        const int width = 256, height = 256;
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = ((y * width) + x) * 3;
                pixels[offset] = 128;
                pixels[offset + 1] = (byte)((((x / 8) + (y / 8)) & 1) * 255);
                pixels[offset + 2] = (byte)(x / 8 * 4);
            }
        }

        var options = new JpegliEncoderOptions
        {
            Subsampling = ChromaSubsampling.None,
            AdaptiveQuantization = false,
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
        var reference = JpegDocument.Read(baseline, false, 0, expectedRestartRows: 1);
        var document = JpegDocument.Read(actual, level != 0, 0,
            expectedRestartRows: 1, expectedInterleaveDc: sampling == ChromaSubsampling.None,
            expectedAcScanEnds: level == 2 ? [2, 63] : [63],
            expectedAcRefinementBits: level == 2 ? [0, 2] : level == 1 ? [1] : [0]);
        foreach ((BlockKey key, Block block) in reference.Blocks)
        {
            if (!block.IsDummy)
            {
                Assert.Equal(block.Coefficients, document.Blocks[key].Coefficients);
            }
        }

        Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(actual, ColorComponents.RedGreenBlue).Data);
    }

    private static long[] Histogram(int symbol, long count)
    {
        long[] frequencies = new long[256];
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
            {
                frame = marker;
            }

            if (marker == 0xdb)
            {
                Assert.Equal(0, jpeg[start] >> 4); // All-one, eight-bit table.
            }

            if (marker == 0xda)
            {
                Assert.Equal(expectedFrame, frame);
                int components = jpeg[start];
                for (int component = 0; component < components; component++)
                {
                    int selector = jpeg[start + 2 + (component * 2)];
                    Assert.InRange(selector >> 4, 0, maximumSlot);
                    Assert.InRange(selector & 15, 0, maximumSlot);
                }
                if (maximumSlot == 2)
                {
                    Assert.Contains(Enumerable.Range(0, components),
                        component => jpeg[start + 2 + (component * 2)] >> 4 == 2);
                }

                return;
            }
            position = end;
        }
        Assert.Fail("JPEG has no sequential scan.");
    }
}
