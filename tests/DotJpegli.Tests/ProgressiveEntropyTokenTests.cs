using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ProgressiveEntropyTokenTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void RecordedProgressiveScansPreserveHistogramsCorrectionsAndRestartBytes(int mode)
    {
        short[][] blocks = Blocks();
        using var tokens = new EntropyTokens();
        long[] expectedFrequencies = Collect(blocks, mode, null, 31);
        long[] actualFrequencies = Collect(blocks, mode, tokens, 31);
        Assert.Equal(expectedFrequencies, actualFrequencies);
        HuffmanTable table = HuffmanTable.CreateOptimized(expectedFrequencies);
        HuffmanTable unused = HuffmanTable.CreateOptimized(new long[256]);
        var coding = new HuffmanCodingPlan.ScanCoding(
            mode == 0 ? [unused, unused, table] : [unused, unused, unused],
            mode == 0 ? [unused, unused, unused] : [unused, unused, table], new byte[3], []);
        using var expected = new MemoryStream();
        var writer = new BitWriter(expected, buffered: true);
        Write(blocks, mode, writer, table, 31);
        writer.Flush();
        using var actual = new MemoryStream();
        var replay = new BitWriter(actual, buffered: true);
        tokens.Replay(replay, coding, TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.True(tokens.IsComplete);
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void MaximumEobRunsRecordTheirExtensionAndSplitAtTheSameBlock(int mode)
    {
        short[][] blocks = Enumerable.Repeat(new short[64], 32769).ToArray();
        using var tokens = new EntropyTokens();
        long[] frequencies = Collect(blocks, mode, tokens, 0, narrowBand: true);
        Assert.Equal(Collect(blocks, mode, null, 0, narrowBand: true), frequencies);
        Assert.Equal(2, frequencies.Sum());
        HuffmanTable table = HuffmanTable.CreateOptimized(frequencies);
        var coding = new HuffmanCodingPlan.ScanCoding([], [table, table, table], [], []);
        using var expected = new MemoryStream();
        var writer = new BitWriter(expected);
        Write(blocks, mode, writer, table, 0, narrowBand: true);
        writer.Flush();
        using var actual = new MemoryStream();
        var replay = new BitWriter(actual);
        tokens.Replay(replay, coding, TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void AbandoningRecordedScansPreservesAllHistograms(int mode)
    {
        short[][] blocks = Blocks();
        using var tokens = new EntropyTokens(128);
        Assert.Equal(Collect(blocks, mode, null, 31), Collect(blocks, mode, tokens, 31));
        Assert.False(tokens.IsComplete);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConvenienceFrequencyOverloadsRecordTheirImplicitEobFlush(bool refine)
    {
        short[] block = Blocks()[0];
        long[] frequencies = new long[256];
        using var tokens = new EntropyTokens();
        if (refine)
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 0, tokens: tokens, component: 2);
        else
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, 2, tokens: tokens, component: 2);
        HuffmanTable table = HuffmanTable.CreateOptimized(frequencies);
        using var expected = new MemoryStream();
        var writer = new BitWriter(expected);
        if (refine)
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, 0);
        else
            ProgressiveEntropyEncoder.WriteAc(writer, block, table, 2);
        writer.Flush();
        using var actual = new MemoryStream();
        var replay = new BitWriter(actual);
        tokens.Replay(replay, new HuffmanCodingPlan.ScanCoding([], [table, table, table], [], []),
            TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    private static short[][] Blocks()
    {
        var random = new Random(49173);
        return Enumerable.Range(0, 288).Select(block => Enumerable.Range(0, 64).Select(position =>
            (short)(position == 0 ? random.Next(-1000, 1001) : block % 17 < 6 ? 3 : block % 17 < 8 ? 0
                : random.Next(3) == 0 ? random.Next(-7, 8) : 0)).ToArray()).ToArray();
    }

    private static long[] Collect(short[][] blocks, int mode, EntropyTokens? tokens, int restartEvery, bool narrowBand = false)
    {
        long[] frequencies = new long[256];
        int previous = 0, pending = 0;
        var state = new ProgressiveAcRefinementState();
        int start = mode is 2 or 4 ? 7 : 1;
        int end = narrowBand ? 1 : mode is 2 or 4 ? 32 : 63;
        for (int index = 0; index < blocks.Length; index++)
        {
            if (mode == 0)
                ProgressiveEntropyEncoder.AccumulateDcFrequencies(blocks[index], ref previous, frequencies, 2, tokens, 2);
            else if (mode < 3)
                ProgressiveEntropyEncoder.AccumulateAcFrequencies(blocks[index], frequencies, ref pending, 2, start, end,
                    tokens: tokens, component: 2);
            else
                ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(blocks[index], frequencies, state, 0, start, end,
                    tokens: tokens, component: 2);
            if (restartEvery > 0 && (index + 1) % restartEvery == 0)
            {
                Flush();
                tokens?.AddRestart(index / restartEvery % 8);
                previous = 0;
            }
        }
        Flush();
        return frequencies;

        void Flush()
        {
            if (mode is 1 or 2)
                ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref pending, tokens, 2);
            else if (mode >= 3)
                ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(frequencies, state, tokens, 2);
        }
    }

    private static void Write(short[][] blocks, int mode, BitWriter writer, HuffmanTable table,
        int restartEvery, bool narrowBand = false)
    {
        int previous = 0, pending = 0;
        var state = new ProgressiveAcRefinementState();
        int start = mode is 2 or 4 ? 7 : 1;
        int end = narrowBand ? 1 : mode is 2 or 4 ? 32 : 63;
        for (int index = 0; index < blocks.Length; index++)
        {
            if (mode == 0)
                ProgressiveEntropyEncoder.WriteDc(writer, blocks[index], ref previous, table, 2);
            else if (mode < 3)
                ProgressiveEntropyEncoder.WriteAc(writer, blocks[index], table, ref pending, 2, start, end);
            else
                ProgressiveEntropyEncoder.WriteAcRefinement(writer, blocks[index], table, state, 0, start, end);
            if (restartEvery > 0 && (index + 1) % restartEvery == 0)
            {
                Flush();
                writer.WriteRestartMarker(index / restartEvery % 8);
                previous = 0;
            }
        }
        Flush();

        void Flush()
        {
            if (mode is 1 or 2)
                ProgressiveEntropyEncoder.FlushAc(writer, table, ref pending);
            else if (mode >= 3)
                ProgressiveEntropyEncoder.FlushAcRefinement(writer, table, state);
        }
    }
}
