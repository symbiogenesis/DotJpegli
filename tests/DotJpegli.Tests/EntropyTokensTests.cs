using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EntropyTokensTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RecordedFrequencyTokensPreserveComponentTablesStuffingAndRestarts(bool buffered)
    {
        var random = new Random(43931);
        short[][] blocks = Enumerable.Range(0, 729).Select(block => Enumerable.Range(0, 64)
            .Select(position => (short)(position == 0 ? random.Next(-1000, 1001)
                : block % 5 == 0 ? 0 : random.Next(4) == 0 ? random.Next(-1023, 1024) : 0)).ToArray()).ToArray();
        long[][] dc = [new long[256], new long[256], new long[256]];
        long[][] ac = [new long[256], new long[256], new long[256]];
        int[] predictors = new int[3];
        using var tokens = new EntropyTokens();
        for (int block = 0; block < blocks.Length; block++)
        {
            int component = block % 3;
            EntropyEncoder.AccumulateFrequencies(blocks[block], ref predictors[component], dc[component], ac[component],
                tokens: tokens, component: component);
            if (block % 21 == 20)
            {
                tokens.AddRestart(block / 21 % 8);
                Array.Clear(predictors);
            }
        }
        HuffmanCodingPlan.ScanCoding coding = HuffmanCodingPlan.Create([new([0, 1, 2])], dc, [ac], false, true).Scans[0];
        using var expected = new MemoryStream();
        var writer = new BitWriter(expected, buffered);
        Array.Clear(predictors);
        for (int block = 0; block < blocks.Length; block++)
        {
            int component = block % 3;
            EntropyEncoder.WriteBlock(writer, blocks[block], ref predictors[component], coding.DcTables[component],
                coding.AcTables[component]);
            if (block % 21 == 20)
            {
                writer.WriteRestartMarker(block / 21 % 8);
                Array.Clear(predictors);
            }
        }
        writer.Flush();
        using var actual = new MemoryStream();
        var replay = new BitWriter(actual, buffered);
        tokens.Replay(replay, coding, TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.True(tokens.IsComplete);
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Fact]
    public void RawBitsAndHuffmanTokensShareCorrectByteBoundaries()
    {
        HuffmanTable dc = HuffmanTable.CreateDefault(false, false), ac = HuffmanTable.CreateDefault(true, false);
        var coding = new HuffmanCodingPlan.ScanCoding([dc, dc, dc], [ac, ac, ac], new byte[3], []);
        using var tokens = new EntropyTokens();
        tokens.AddBits(uint.MaxValue, 32);
        tokens.AddHuffman(2, true, 0, 0, 0);
        tokens.AddRestart(7);
        tokens.AddBits(0, 1);
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, coding, TestContext.Current.CancellationToken);
        writer.Flush();
        Assert.Equal(new byte[] { 0xff, 0, 0xff, 0, 0xff, 0, 0xff, 0, 0xaf, 0xff, 0xd7, 0x7f }, output.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(128)]
    public void ExhaustedBudgetAbandonsAllTokensAndKeepsFrequencyCollectionCorrect(int maximumBytes)
    {
        using var tokens = new EntropyTokens(maximumBytes);
        long[] expectedDc = new long[256], expectedAc = new long[256], actualDc = new long[256], actualAc = new long[256];
        short[] block = new short[64];
        int expectedPrevious = 0, actualPrevious = 0;
        for (int index = 0; index < 40; index++)
        {
            EntropyEncoder.AccumulateFrequencies(block, ref expectedPrevious, expectedDc, expectedAc);
            EntropyEncoder.AccumulateFrequencies(block, ref actualPrevious, actualDc, actualAc, tokens: tokens);
        }
        Assert.False(tokens.IsComplete);
        tokens.AddBits(uint.MaxValue, 32);
        tokens.AddRestart(0);
        Assert.Equal(expectedDc, actualDc);
        Assert.Equal(expectedAc, actualAc);
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        var coding = new HuffmanCodingPlan.ScanCoding([], [], [], []);
        Assert.Throws<InvalidOperationException>(() => tokens.Replay(writer, coding, TestContext.Current.CancellationToken));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void ReplayHonorsPreCancellationWithoutEmittingBytes()
    {
        using var tokens = new EntropyTokens();
        tokens.AddBits(0x42, 8);
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        var cancellation = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => tokens.Replay(writer,
            new HuffmanCodingPlan.ScanCoding([], [], [], []), cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void ValidationAndHistogramOverflowStillHappenBeforeRecordingTheFailingSymbol()
    {
        using var tokens = new EntropyTokens();
        short[] block = new short[64];
        block[63] = 1024;
        long[] dc = new long[256], ac = new long[256];
        int previous = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => EntropyEncoder.AccumulateFrequencies(block, ref previous, dc, ac,
            tokens: tokens));
        block[63] = 0;
        dc[0] = long.MaxValue;
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(block, ref previous, dc, ac, tokens: tokens));
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        tokens.Replay(writer, new HuffmanCodingPlan.ScanCoding([], [], [], []), TestContext.Current.CancellationToken);
        writer.Flush();
        Assert.Equal(0, output.Length);
    }
}
