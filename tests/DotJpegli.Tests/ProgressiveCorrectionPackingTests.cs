namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ProgressiveCorrectionPackingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryCorrectionLengthPreservesBitsAndFitsTheMinimumRental(bool buffered)
    {
        HuffmanTable table = Table();
        for (int length = 0; length <= ProgressiveAcRefinementState.MaximumCorrectionBits; length++)
        {
            byte[] corrections = Corrections(length);
            using var tokens = new EntropyTokens(length <= 255 ? 64 : 512);
            Record(tokens, corrections);
            Assert.True(tokens.IsComplete);
            foreach (int prefixBits in new[] { 0, 1, 7, 63 })
            {
                using var expected = new MemoryStream();
                var direct = new BitWriter(expected, buffered);
                WritePrefix(direct, prefixBits);
                table.WriteSymbol(direct, 0);
                foreach (byte correction in corrections)
                {
                    direct.WriteBits(correction, 1);
                }
                direct.WriteRestartMarker(3);

                using var actual = new MemoryStream();
                var replay = new BitWriter(actual, buffered);
                WritePrefix(replay, prefixBits);
                tokens.Replay(replay, Coding(table), TestContext.Current.CancellationToken);
                replay.WriteRestartMarker(3);
                Assert.Equal(expected.ToArray(), actual.ToArray());
            }
        }
    }

    [Fact]
    public void MaximumCorrectionGroupUsesElevenRawRecordsWithinTheFixedBudget()
    {
        using var tokens = new EntropyTokens(64);
        Record(tokens, Corrections(255));
        // One EOB plus eleven raw records leaves four of the sixteen slots.
        // A further record must abandon the cache without affecting the cap.
        for (int index = 0; index < 4; index++)
        {
            tokens.AddBits(0, 0);
        }
        Assert.True(tokens.IsComplete);
        tokens.AddBits(0, 0);
        Assert.False(tokens.IsComplete);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReservedAndOrdinaryCorrectionGroupsMatchDirectEncoding(bool ordered)
    {
        for (int length = 1; length < 63; length++)
        {
            short[] coefficients = new short[64];
            byte[] corrections = Corrections(length);
            for (int position = 1; position <= length; position++)
            {
                coefficients[ordered ? position : JpegConstants.ZigZag[position]] = (short)(2 + corrections[position - 1]);
            }
            coefficients[ordered ? length + 1 : JpegConstants.ZigZag[length + 1]] = -1;
            long[] frequencies = new long[256];
            using var tokens = new EntropyTokens();
            tokens.AddBits(0, 0);
            Assert.True(tokens.TryReserveBlock(out _));
            var state = new ProgressiveAcRefinementState();
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(coefficients, frequencies, state, 0,
                tokens: tokens, zigZagOrdered: ordered);
            ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(frequencies, state, tokens);
            var table = HuffmanTable.CreateOptimized(frequencies);

            using var expected = new MemoryStream();
            var direct = new BitWriter(expected, buffered: true);
            state = new ProgressiveAcRefinementState();
            ProgressiveEntropyEncoder.WriteAcRefinement(direct, coefficients, table, state, 0, zigZagOrdered: ordered);
            ProgressiveEntropyEncoder.FlushAcRefinement(direct, table, state);
            direct.Flush();
            using var actual = new MemoryStream();
            var replay = new BitWriter(actual, buffered: true);
            tokens.Replay(replay, Coding(table), TestContext.Current.CancellationToken);
            replay.Flush();
            Assert.Equal(expected.ToArray(), actual.ToArray());
        }
    }

    [Theory]
    [InlineData(8190)]
    [InlineData(8191)]
    [InlineData(8192)]
    public void CorrectionGroupsCrossRentalBoundariesWithoutChangingTheStream(int prefixRecords)
    {
        using var tokens = new EntropyTokens((8192 + 16) * sizeof(uint));
        for (int index = 0; index < prefixRecords; index++)
        {
            tokens.AddBits(0, 0);
        }
        byte[] corrections = Corrections(255);
        Record(tokens, corrections);
        tokens.AddRestart(7);
        Assert.True(tokens.IsComplete);
        HuffmanTable table = Table();
        using var expected = new MemoryStream();
        var direct = new BitWriter(expected, buffered: true);
        table.WriteSymbol(direct, 0);
        foreach (byte correction in corrections)
        {
            direct.WriteBits(correction, 1);
        }
        direct.WriteRestartMarker(7);
        using var actual = new MemoryStream();
        var replay = new BitWriter(actual, buffered: true);
        tokens.Replay(replay, Coding(table), TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void RepackedCorrectionsPreserveLegacyDestinationFailurePrefixes(int acceptedBytes)
    {
        using var legacy = new EntropyTokens();
        using var packed = new EntropyTokens();
        for (int group = 0; group < 160; group++)
        {
            byte[] corrections = Corrections(group % 2 == 0 ? 255 : 63);
            Record(packed, corrections);
            RecordLegacy(legacy, corrections);
            if (group % 37 == 36)
            {
                packed.AddRestart(group & 7);
                legacy.AddRestart(group & 7);
            }
        }
        foreach (int failedWrite in new[] { 1, 2, 4 })
        {
            using var expected = new FailingStream(failedWrite, acceptedBytes);
            using var actual = new FailingStream(failedWrite, acceptedBytes);
            Assert.Same(expected.Failure, Assert.Throws<IOException>(() => Replay(legacy, expected)));
            Assert.Same(actual.Failure, Assert.Throws<IOException>(() => Replay(packed, actual)));
            Assert.Equal(expected.AttemptedWrite, actual.AttemptedWrite);
            Assert.Equal(expected.ToArray(), actual.ToArray());
            Assert.True(actual.CanWrite);
        }
    }

    [Fact]
    public void CancellationBeforeReplayPreservesTheEmptyDestination()
    {
        using var tokens = new EntropyTokens(64);
        Record(tokens, Corrections(255));
        using var output = new MemoryStream();
        var cancellation = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            tokens.Replay(new BitWriter(output, buffered: true), Coding(Table()), cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
        Assert.Empty(output.ToArray());
        Assert.True(tokens.IsComplete);
    }

    private static void Record(EntropyTokens tokens, byte[] corrections)
    {
        var state = new ProgressiveAcRefinementState();
        state.AppendBlock(corrections);
        ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(new long[256], state, tokens);
        Assert.Equal(0, state.PendingEobRun);
        Assert.Equal(0, state.CorrectionBitCount);
    }

    private static void RecordLegacy(EntropyTokens tokens, byte[] corrections)
    {
        tokens.AddHuffman(0, true, 0, 0, 0);
        int offset = 0;
        while (offset < corrections.Length)
        {
            int remaining = corrections.Length - offset;
            int count = remaining >= 32 ? 32 : remaining >= 8 ? 8 : remaining;
            uint bits = 0;
            for (int index = 0; index < count; index++)
            {
                bits = (bits << 1) | corrections[offset++];
            }
            tokens.AddBits(bits, count);
        }
    }

    private static byte[] Corrections(int length)
        => Enumerable.Range(0, length).Select(index => (byte)(index % 47 < 24 ? 1 : ((index * 7) + (index / 3)) & 1)).ToArray();

    private static HuffmanTable Table()
    {
        long[] frequencies = new long[256];
        frequencies[0] = 1;
        return HuffmanTable.CreateOptimized(frequencies);
    }

    private static HuffmanCodingPlan.ScanCoding Coding(HuffmanTable table) => new([], [table], [], []);

    private static void WritePrefix(BitWriter writer, int count)
    {
        for (int index = 0; index < count; index++)
        {
            writer.WriteBits((uint)(index & 1), 1);
        }
    }

    private static void Replay(EntropyTokens tokens, Stream output)
    {
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, Coding(Table()), TestContext.Current.CancellationToken);
        writer.Flush();
    }

    private sealed class FailingStream(int failedWrite, int acceptedBytes) : MemoryStream
    {
        private int writes;
        internal IOException Failure { get; } = new("Injected correction replay failure.");
        internal byte[]? AttemptedWrite { get; private set; }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (++this.writes == failedWrite)
            {
                this.AttemptedWrite = buffer.ToArray();
                base.Write(buffer[..Math.Min(acceptedBytes, buffer.Length)]);
                throw this.Failure;
            }
            base.Write(buffer);
        }
    }
}
