using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SpectralEntropyTests
{
    [Theory]
    [InlineData(1, 1, 0, 0x01, "1")]
    [InlineData(17, -3, 0, 0x02, "00")]
    [InlineData(63, 7, 2, 0x01, "1")]
    [InlineData(28, -7, 2, 0x01, "0")]
    public void InitialSingletonBandEndsWithoutEob(
        int position, short value, int successiveLow, int symbol, string bits)
    {
        short[] block = FilledBlock(9);
        Set(block, position, value);
        CheckInitialBlock(block, successiveLow, position, position, (symbol, bits));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(32)]
    public void InitialZeroRunsBeginAtSpectralStartAndEndExactlyAtSpectralEnd(int zeroCount)
    {
        const int start = 7;
        int end = start + zeroCount;
        short[] block = FilledBlock(-7);
        ClearBand(block, start, end);
        Set(block, end, -1);
        var tokens = Enumerable.Repeat((0xf0, ""), zeroCount / 16)
            .Append(((zeroCount % 16 << 4) | 1, "0")).ToArray();
        CheckInitialBlock(block, 0, start, end, tokens);
    }

    [Fact]
    public void InitialZeroBandsAggregateBeforeTheNextNonzeroBand()
    {
        const int start = 11;
        const int end = 15;
        short[] empty = FilledBlock(5);
        ClearBand(empty, start, end);
        short[] final = FilledBlock(-5);
        ClearBand(final, start, end);
        Set(final, end, -3);
        short[][] blocks = [empty, empty, empty, empty, empty, empty, empty, final];
        var tokens = new[] { (0x20, "11"), (0x42, "00") };
        long[] expected = Histogram(tokens);
        long[] frequencies = new long[256];
        int pending = 0;
        foreach (short[] block in blocks[..7])
        {
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending, 0, start, end);
        }
        Assert.Equal(7, pending);
        Assert.Equal(0, frequencies.Sum());
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(final, frequencies, ref pending, 0, start, end);
        Assert.Equal(0, pending);
        Assert.Equal(expected, frequencies);
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref pending);
        Assert.Equal(expected, frequencies);

        HuffmanTable table = HuffmanTable.CreateOptimized(expected);
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        foreach (short[] block in blocks[..7])
        {
            ProgressiveEntropyEncoder.WriteAc(writer, block, table, ref pending, 0, start, end);
        }
        Assert.Equal(7, pending);
        Assert.Empty(output.ToArray());
        ProgressiveEntropyEncoder.WriteAc(writer, final, table, ref pending, 0, start, end);
        Assert.Equal(0, pending);
        ProgressiveEntropyEncoder.FlushAc(writer, table, ref pending);
        writer.Flush();
        CheckTokens(output.ToArray(), table, expected, tokens);
    }

    [Theory]
    [InlineData(1, 1, 0, "1")]
    [InlineData(17, -2, 1, "0")]
    [InlineData(63, -512, 9, "0")]
    public void NewlySignificantSingletonBandEndsWithoutEob(
        int position, short value, int successiveLow, string sign)
    {
        short[] block = FilledBlock(1023);
        Set(block, position, value);
        CheckRefinementBlock(block, successiveLow, position, position, (0x01, sign));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(32)]
    public void RefinementZeroRunsBeginAtSpectralStartAndEndExactlyAtSpectralEnd(int zeroCount)
    {
        const int start = 7;
        int end = start + zeroCount;
        short[] block = FilledBlock(-6);
        ClearBand(block, start, end);
        Set(block, end, 2);
        var tokens = Enumerable.Repeat((0xf0, ""), zeroCount / 16)
            .Append(((zeroCount % 16 << 4) | 1, "1")).ToArray();
        CheckRefinementBlock(block, 1, start, end, tokens);
    }

    [Theory]
    [InlineData(-2, 2, "0", "110")]
    [InlineData(2, -2, "1", "010")]
    public void NewCoefficientsAtBothBandEdgesWriteSignsBeforeOnlyInBandCorrections(
        short first, short last, string firstBits, string lastBits)
    {
        short[] block = FilledBlock(6);
        ClearBand(block, 17, 23);
        Set(block, 17, first);
        Set(block, 18, 6);
        Set(block, 22, -4);
        Set(block, 23, last);
        // Three still-zero coefficients separate the new values. The old
        // coefficients contribute 10 after the final sign, and no final EOB.
        CheckRefinementBlock(block, 1, 17, 23, (0x01, firstBits), (0x31, lastBits));
    }

    [Fact]
    public void RefinementZeroBandsAggregateWithoutOutsideCorrectionBits()
    {
        short[] empty = FilledBlock(-3);
        ClearBand(empty, 21, 27);
        CheckRefinementSequence([empty, empty, empty], 0, 21, 27, (0x10, "1"));
    }

    [Fact]
    public void OutsideOldCoefficientsDoNotSplitAFullSingletonCorrectionBuffer()
    {
        short[] old = FilledBlock(3);
        short[] empty = FilledBlock(3);
        Set(empty, 17, 0);
        short[] next = FilledBlock(3);
        Set(next, 17, -2);
        short[][] blocks = Enumerable.Repeat(old, 255).Concat([empty, next]).ToArray();
        long[] frequencies = new long[256];
        var state = new ProgressiveAcRefinementState();
        foreach (short[] block in blocks[..256])
        {
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, state, 0, 17, 17);
        }
        Assert.Equal(256, state.PendingEobRun);
        Assert.Equal(255, state.CorrectionBitCount);
        Assert.Equal(0, frequencies.Sum());
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(next, frequencies, state, 0, 17, 17);
        Assert.Equal(1, state.PendingEobRun);
        Assert.Equal(new byte[] { 0 }, state.Corrections.ToArray());
        Assert.Equal(1, frequencies[0x80]);
        Assert.Equal(1, frequencies.Sum());

        // Run 256 has eight extension zeros, then the selected bit from each
        // of the first 255 blocks. The empty block joins the full buffer.
        CheckRefinementSequence(blocks, 0, 17, 17,
            (0x80, new string('0', 8) + new string('1', 255)), (0x00, "0"));
    }

    [Fact]
    public void FiveCoefficientBandSplitsCorrectionRunsOnlyAfterExactly255Bits()
    {
        short[] old = FilledBlock(3);
        Set(old, 18, 2);
        Set(old, 19, -3);
        Set(old, 20, -2);
        short[] empty = FilledBlock(-3);
        ClearBand(empty, 17, 21);
        short[] next = FilledBlock(3);
        ClearBand(next, 17, 21);
        Set(next, 21, 2);
        short[][] blocks = Enumerable.Repeat(old, 51).Concat([empty, next]).ToArray();

        // 52 blocks: floor(log2(52)) = 5, with extension 52 - 32 = 20.
        CheckRefinementSequence(blocks, 0, 17, 21,
            (0x50, "10100" + string.Concat(Enumerable.Repeat("10101", 51))), (0x00, "0"));
    }

    [Fact]
    public void NewBandCoefficientFlushesPriorCorrectionsBeforeZrlAndKeepsTrailingCorrections()
    {
        short[] first = FilledBlock(3);
        ClearBand(first, 8, 28);
        Set(first, 8, -3);
        Set(first, 28, 2);
        short[] second = FilledBlock(3);
        ClearBand(second, 8, 28);
        Set(second, 8, 3);
        Set(second, 25, -2);
        Set(second, 26, -1);
        Set(second, 28, 3);
        short[] final = FilledBlock(3);
        ClearBand(final, 8, 28);
        Set(final, 28, 1);

        CheckRefinementSequence([first, second, final], 0, 8, 28,
            (0x00, "10"), (0xf0, "1"), (0x01, "00"),
            (0x00, "1"), (0xf0, ""), (0x41, "1"));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 63)]
    [InlineData(64, 64)]
    [InlineData(int.MinValue, 63)]
    [InlineData(int.MaxValue, 63)]
    [InlineData(1, 0)]
    [InlineData(20, 19)]
    [InlineData(1, 64)]
    [InlineData(1, int.MaxValue)]
    [InlineData(20, int.MinValue)]
    public void InvalidSpectralRangesPreserveBufferedOutputPendingRunsAndFrequencies(int start, int end)
    {
        short[] block = FilledBlock(3);
        HuffmanTable table = HuffmanTable.CreateDefault(true, false);
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        writer.WriteBits(0b101, 3);
        long[] frequencies = new long[256];
        frequencies[0] = 9;
        frequencies[0xf0] = 5;
        long[] before = frequencies.ToArray();
        int pending = 7;
        var writeState = new ProgressiveAcRefinementState();
        var frequencyState = new ProgressiveAcRefinementState();
        ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, writeState, 0, 17, 21);
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, frequencyState, 0, 17, 21);
        byte[] corrections = writeState.Corrections.ToArray();
        string parameter = start is < 1 or > 63 ? "spectralStart" : "spectralEnd";

        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, table, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, table, ref pending, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, writeState, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 0, start, end)).ParamName);
        Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, frequencyState, 0, start, end)).ParamName);

        Assert.Equal(7, pending);
        Assert.Equal(before, frequencies);
        Assert.Equal(1, writeState.PendingEobRun);
        Assert.Equal(1, frequencyState.PendingEobRun);
        Assert.Equal(5, writeState.CorrectionBitCount);
        Assert.Equal(5, frequencyState.CorrectionBitCount);
        Assert.Equal(corrections, writeState.Corrections.ToArray());
        Assert.Equal(corrections, frequencyState.Corrections.ToArray());
        writer.Flush();
        Assert.Equal(new byte[] { 0xbf }, output.ToArray());
    }

    private static void CheckInitialBlock(
        short[] block, int successiveLow, int start, int end, params (int Symbol, string Bits)[] tokens)
    {
        long[] expected = Histogram(tokens);
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, successiveLow, start, end);
        Assert.Equal(expected, frequencies);
        foreach (HuffmanTable table in Tables(expected))
        {
            using var output = new MemoryStream();
            var writer = new BitWriter(output);
            ProgressiveEntropyEncoder.WriteAc(writer, block, table, successiveLow, start, end);
            writer.Flush();
            CheckTokens(output.ToArray(), table, expected, tokens);
        }
    }

    private static void CheckRefinementBlock(
        short[] block, int successiveLow, int start, int end, params (int Symbol, string Bits)[] tokens)
    {
        long[] expected = Histogram(tokens);
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, successiveLow, start, end);
        Assert.Equal(expected, frequencies);
        foreach (HuffmanTable table in Tables(expected))
        {
            using var output = new MemoryStream();
            var writer = new BitWriter(output);
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, successiveLow, start, end);
            writer.Flush();
            CheckTokens(output.ToArray(), table, expected, tokens);
        }
    }

    private static void CheckRefinementSequence(
        short[][] blocks, int successiveLow, int start, int end, params (int Symbol, string Bits)[] tokens)
    {
        long[] expected = Histogram(tokens);
        long[] frequencies = new long[256];
        var frequencyState = new ProgressiveAcRefinementState();
        foreach (short[] block in blocks)
        {
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, frequencyState, successiveLow, start, end);
        }
        ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(frequencies, frequencyState);
        Assert.Equal(expected, frequencies);
        Assert.Equal(0, frequencyState.PendingEobRun);
        Assert.Equal(0, frequencyState.CorrectionBitCount);

        HuffmanTable table = HuffmanTable.CreateOptimized(expected);
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        var writeState = new ProgressiveAcRefinementState();
        foreach (short[] block in blocks)
        {
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, table, writeState, successiveLow, start, end);
        }
        ProgressiveEntropyEncoder.FlushAcRefinement(writer, table, writeState);
        writer.Flush();
        Assert.Equal(0, writeState.PendingEobRun);
        Assert.Equal(0, writeState.CorrectionBitCount);
        CheckTokens(output.ToArray(), table, frequencies, tokens);
    }

    private static HuffmanTable[] Tables(long[] frequencies) =>
        [HuffmanTable.CreateDefault(true, false), HuffmanTable.CreateDefault(true, true), HuffmanTable.CreateOptimized(frequencies)];

    private static long[] Histogram((int Symbol, string Bits)[] tokens)
    {
        long[] frequencies = new long[256];
        foreach ((int symbol, _) in tokens)
        {
            frequencies[symbol]++;
        }
        return frequencies;
    }

    // Decode canonical prefix codes from the table's public DHT representation;
    // expected bytes never pass through the production Huffman or bit writer.
    private static void CheckTokens(
        byte[] output, HuffmanTable table, long[] frequencies, (int Symbol, string Bits)[] tokens)
    {
        var codes = new Dictionary<(int Length, int Code), int>();
        int code = 0;
        int index = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int count = 0; count < table.Counts[length - 1]; count++)
            {
                codes.Add((length, code++), table.Symbols[index++]);
            }
            code <<= 1;
        }

        var reader = new EntropyBitReader(output);
        long[] decodedFrequencies = new long[256];
        foreach ((int expectedSymbol, string bits) in tokens)
        {
            code = 0;
            int symbol = -1;
            for (int length = 1; length <= 16; length++)
            {
                code = (code << 1) | reader.ReadBit();
                if (codes.TryGetValue((length, code), out symbol))
                {
                    break;
                }
                symbol = -1;
            }
            Assert.Equal(expectedSymbol, symbol);
            decodedFrequencies[symbol]++;
            foreach (char bit in bits)
            {
                Assert.Equal(bit - '0', reader.ReadBit());
            }
        }
        Assert.Equal(frequencies, decodedFrequencies);
        Assert.InRange(reader.RemainingBits, 0, 7);
        while (reader.RemainingBits > 0)
        {
            Assert.Equal(1, reader.ReadBit());
        }
    }

    private static short[] FilledBlock(short value)
    {
        short[] block = new short[64];
        Array.Fill(block, value);
        return block;
    }

    private static void Set(short[] block, int position, short value) =>
        block[JpegConstants.ZigZag[position]] = value;

    private static void ClearBand(short[] block, int start, int end)
    {
        for (int position = start; position <= end; position++)
        {
            Set(block, position, 0);
        }
    }

    private sealed class EntropyBitReader
    {
        private readonly byte[] bytes;
        private int position;

        public EntropyBitReader(byte[] stuffed)
        {
            var unstuffed = new List<byte>();
            for (int index = 0; index < stuffed.Length; index++)
            {
                byte value = stuffed[index];
                unstuffed.Add(value);
                if (value == 0xff)
                {
                    Assert.True(index + 1 < stuffed.Length, "An entropy FF byte needs a stuffing byte.");
                    Assert.Equal(0, stuffed[++index]);
                }
            }
            bytes = unstuffed.ToArray();
        }

        public int RemainingBits => bytes.Length * 8 - position;

        public int ReadBit()
        {
            Assert.True(RemainingBits > 0, "The entropy stream ended before the expected token bits.");
            int bit = (bytes[position / 8] >> (7 - position % 8)) & 1;
            position++;
            return bit;
        }
    }
}
