using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class OrderedProgressiveEntropyTests
{
    [Theory]
    [InlineData(false, 1, 63)]
    [InlineData(false, 1, 1)]
    [InlineData(false, 63, 63)]
    [InlineData(false, 7, 32)]
    [InlineData(false, 33, 63)]
    [InlineData(true, 1, 63)]
    [InlineData(true, 1, 1)]
    [InlineData(true, 63, 63)]
    [InlineData(true, 7, 32)]
    [InlineData(true, 33, 63)]
    public void OrderedScansMatchNaturalHistogramsStateOutputAndRecordedReplay(bool refine, int start, int end)
    {
        for (int low = 0; low <= (refine ? 9 : 10); low++)
        {
            short[][] natural = Blocks(low);
            short[][] ordered = natural.Select(ToOrdered).ToArray();
            long[] expectedFrequencies = new long[256], actualFrequencies = new long[256];
            var expectedState = new ProgressiveAcRefinementState();
            var actualState = new ProgressiveAcRefinementState();
            int expectedPending = 0, actualPending = 0;
            using var tokens = new EntropyTokens();
            for (int index = 0; index < natural.Length; index++)
            {
                if (refine)
                {
                    ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(natural[index], expectedFrequencies, expectedState, low, start, end);
                    ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(ordered[index], actualFrequencies, actualState, low, start, end,
                        coefficientsValidated: true, tokens: tokens, component: 2, zigZagOrdered: true);
                }
                else
                {
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(natural[index], expectedFrequencies, ref expectedPending, low, start, end);
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(ordered[index], actualFrequencies, ref actualPending, low, start, end,
                        coefficientsValidated: true, tokens: tokens, component: 2, zigZagOrdered: true);
                }
                Assert.Equal(expectedState.PendingEobRun, actualState.PendingEobRun);
                Assert.Equal(expectedState.Corrections.ToArray(), actualState.Corrections.ToArray());
                Assert.Equal(expectedPending, actualPending);
                if ((index + 1) % 31 == 0 || index + 1 == natural.Length)
                {
                    if (refine)
                    {
                        ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(expectedFrequencies, expectedState);
                        ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(actualFrequencies, actualState, tokens, 2);
                    }
                    else
                    {
                        ProgressiveEntropyEncoder.FlushAcFrequencies(expectedFrequencies, ref expectedPending);
                        ProgressiveEntropyEncoder.FlushAcFrequencies(actualFrequencies, ref actualPending, tokens, 2);
                    }
                    if (index + 1 != natural.Length)
                        tokens.AddRestart(index / 31 % 8);
                }
            }
            Assert.Equal(expectedFrequencies, actualFrequencies);
            HuffmanTable table = HuffmanTable.CreateOptimized(expectedFrequencies);
            byte[] expected = Write(natural, table, refine, low, start, end, ordered: false);
            Assert.Equal(expected, Write(ordered, table, refine, low, start, end, ordered: true));
            using var replayOutput = new MemoryStream();
            var replayWriter = new BitWriter(replayOutput, buffered: true);
            tokens.Replay(replayWriter, new HuffmanCodingPlan.ScanCoding([], [table, table, table], [], []),
                TestContext.Current.CancellationToken);
            replayWriter.Flush();
            Assert.Equal(expected, replayOutput.ToArray());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrderedConvenienceOverloadsPreserveTheirImplicitFlush(bool refine)
    {
        short[] natural = Blocks(1)[5], ordered = ToOrdered(natural);
        long[] frequencies = new long[256];
        using var tokens = new EntropyTokens();
        if (refine)
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(ordered, frequencies, 1, tokens: tokens, component: 2, zigZagOrdered: true);
        else
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(ordered, frequencies, 1, tokens: tokens, component: 2, zigZagOrdered: true);
        HuffmanTable table = HuffmanTable.CreateOptimized(frequencies);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var naturalWriter = new BitWriter(expected);
        var orderedWriter = new BitWriter(actual);
        if (refine)
        {
            ProgressiveEntropyEncoder.WriteAcRefinement(naturalWriter, natural, table, 1);
            ProgressiveEntropyEncoder.WriteAcRefinement(orderedWriter, ordered, table, 1, zigZagOrdered: true);
        }
        else
        {
            ProgressiveEntropyEncoder.WriteAc(naturalWriter, natural, table, 1);
            ProgressiveEntropyEncoder.WriteAc(orderedWriter, ordered, table, 1, zigZagOrdered: true);
        }
        naturalWriter.Flush();
        orderedWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Fact]
    public void InvalidOrderedCoefficientsPreservePriorRefinementStateAndOutput()
    {
        short[] ordered = Enumerable.Repeat((short)3, 64).ToArray();
        ordered[0] = short.MinValue;
        var state = new ProgressiveAcRefinementState();
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(ordered, frequencies, state, 0, zigZagOrdered: true);
        ordered[63] = 1024;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(ordered, frequencies, state, 0, zigZagOrdered: true));
        Assert.Equal(1, state.PendingEobRun);
        Assert.Equal(63, state.CorrectionBitCount);
        Assert.Equal(0, frequencies.Sum());
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(new BitWriter(output), ordered, HuffmanTable.CreateOptimized(frequencies), state, 0, zigZagOrdered: true));
        Assert.Empty(output.ToArray());
    }

    private static byte[] Write(short[][] blocks, HuffmanTable table, bool refine, int low, int start, int end, bool ordered)
    {
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        var state = new ProgressiveAcRefinementState();
        int pending = 0;
        for (int index = 0; index < blocks.Length; index++)
        {
            if (refine)
                ProgressiveEntropyEncoder.WriteAcRefinement(writer, blocks[index], table, state, low, start, end,
                    zigZagOrdered: ordered);
            else
                ProgressiveEntropyEncoder.WriteAc(writer, blocks[index], table, ref pending, low, start, end,
                    zigZagOrdered: ordered);
            if ((index + 1) % 31 == 0 || index + 1 == blocks.Length)
            {
                if (refine)
                    ProgressiveEntropyEncoder.FlushAcRefinement(writer, table, state);
                else
                    ProgressiveEntropyEncoder.FlushAc(writer, table, ref pending);
                if (index + 1 != blocks.Length)
                    writer.WriteRestartMarker(index / 31 % 8);
            }
        }
        writer.Flush();
        return output.ToArray();
    }

    private static short[][] Blocks(int low)
    {
        var random = new Random(71573 + low);
        short[][] result = Enumerable.Range(0, 192).Select(index => Enumerable.Range(0, 64)
            .Select(position => (short)(position == 0 ? short.MinValue : index % 16 == 0 ? 0 : index % 16 < 5 ? 3
                : random.Next(4) == 0 ? random.Next(-1023, 1024) : 0)).ToArray()).ToArray();
        // Interspersed old coefficients straddle the sixteenth zero, before a
        // new coefficient; the final new coefficient also exercises bit 63.
        Array.Clear(result[5]);
        foreach (int position in new[] { 1, 8, 16, 18, 31, 45 })
            result[5][JpegConstants.ZigZag[position]] = (short)(low < 9 ? 3 << low : 0);
        result[5][JpegConstants.ZigZag[20]] = (short)-(low < 10 ? 1 << low : 0);
        result[5][JpegConstants.ZigZag[63]] = (short)(low < 10 ? 1 << low : 0);
        return result;
    }

    private static short[] ToOrdered(short[] natural) => JpegConstants.ZigZag.ToArray().Select(position => natural[position]).ToArray();
}
