using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class OrderedEntropyTests
{
    [Fact]
    public void ReservedAmplitudePreparationPreservesEveryBaselineDcAndShortAcValue()
    {
        HuffmanTable table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        using var expected = new MemoryStream();
        var expectedWriter = new BitWriter(expected, buffered: true);
        using var tokens = new EntropyTokens();
        tokens.AddBits(0, 0);
        long[] dc = new long[256], ac = new long[256];
        short[] block = new short[64];
        for (int value = -2047; value <= 2047; value++)
            Record(value, value / 2);
        // Callers supplying already-validated blocks previously allowed wider
        // short amplitudes. Exercise the arithmetic fallback as well as every
        // cached value without changing that internal caller behavior.
        for (int value = short.MinValue; value <= short.MaxValue; value++)
            Record(0, value);
        expectedWriter.Flush();
        using var actual = new MemoryStream();
        var actualWriter = new BitWriter(actual, buffered: true);
        tokens.Replay(actualWriter, new HuffmanCodingPlan.ScanCoding([table], [table], [], []),
            TestContext.Current.CancellationToken);
        actualWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());

        void Record(int dcValue, int acValue)
        {
            block[0] = (short)dcValue;
            block[1] = (short)acValue;
            int expectedPrevious = 0, actualPrevious = 0;
            EntropyEncoder.WriteBlock(expectedWriter, block, ref expectedPrevious, table, table,
                coefficientsValidated: true);
            EntropyEncoder.AccumulateFrequencies(block, ref actualPrevious, dc, ac,
                coefficientsValidated: true, tokens: tokens, zigZagOrdered: true);
        }
    }

    [Fact]
    public void NonzeroMasksPreserveEveryPositionAndIgnoreDc()
    {
        short[] block = new short[64];
        for (int position = 0; position < 64; position++)
            foreach (short value in new[] { short.MinValue, (short)-1, (short)1, short.MaxValue })
            {
                block[position] = value;
                Assert.Equal(position == 0 ? 0UL : 1UL << position, EntropyEncoder.NonzeroAcMask(block));
                block[position] = 0;
            }
        Array.Fill(block, (short)1);
        Assert.Equal(ulong.MaxValue - 1, EntropyEncoder.NonzeroAcMask(block));
        Array.Clear(block);
        Assert.Equal(0UL, EntropyEncoder.NonzeroAcMask(block));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrderedBlocksPreserveHistogramsDirectBytesAndRecordedTokens(bool validated)
    {
        var random = new Random(841371);
        short[][] natural = Enumerable.Range(0, 512).Select(block => Enumerable.Range(0, 64)
            .Select(position => (short)(position == 0 ? random.Next(-1000, 1001)
                : block % 11 == 0 ? 0 : random.Next(block % 7 + 1) == 0 ? random.Next(-1023, 1024) : 0)).ToArray()).ToArray();
        short[][] ordered = natural.Select(Order).ToArray();
        long[] dcNatural = new long[256], acNatural = new long[256], dcOrdered = new long[256], acOrdered = new long[256];
        using var tokens = new EntropyTokens();
        int previousNatural = 0, previousOrdered = 0;
        for (int index = 0; index < natural.Length; index++)
        {
            EntropyEncoder.AccumulateFrequencies(natural[index], ref previousNatural, dcNatural, acNatural, validated);
            EntropyEncoder.AccumulateFrequencies(ordered[index], ref previousOrdered, dcOrdered, acOrdered, validated,
                tokens: tokens, zigZagOrdered: true);
        }
        Assert.Equal(dcNatural, dcOrdered);
        Assert.Equal(acNatural, acOrdered);
        Assert.Equal(previousNatural, previousOrdered);
        HuffmanTable dc = HuffmanTable.CreateOptimized(dcNatural), ac = HuffmanTable.CreateOptimized(acNatural);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var naturalWriter = new BitWriter(expected, buffered: true);
        var orderedWriter = new BitWriter(actual, buffered: true);
        previousNatural = previousOrdered = 0;
        for (int index = 0; index < natural.Length; index++)
        {
            EntropyEncoder.WriteBlock(naturalWriter, natural[index], ref previousNatural, dc, ac, validated);
            EntropyEncoder.WriteBlock(orderedWriter, ordered[index], ref previousOrdered, dc, ac, validated, zigZagOrdered: true);
        }
        naturalWriter.Flush();
        orderedWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
        using var recorded = new MemoryStream();
        var replay = new BitWriter(recorded, buffered: true);
        tokens.Replay(replay, new HuffmanCodingPlan.ScanCoding([dc], [ac], [], []), TestContext.Current.CancellationToken);
        replay.Flush();
        Assert.Equal(expected.ToArray(), recorded.ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(32)]
    [InlineData(63)]
    public void HistogramOverflowPreservesTheSamePartialFrequenciesPredictorAndTokens(int position)
    {
        short[] natural = new short[64];
        natural[0] = 5;
        natural[JpegConstants.ZigZag[position]] = 1023;
        short[] ordered = Order(natural);
        long[] dcNatural = new long[256], acNatural = new long[256], dcOrdered = new long[256], acOrdered = new long[256];
        int symbol = (((position - 1) % 16) << 4) | 10;
        acNatural[symbol] = acOrdered[symbol] = long.MaxValue;
        int previousNatural = 0, previousOrdered = 0;
        using var naturalTokens = new EntropyTokens();
        using var orderedTokens = new EntropyTokens();
        // Prime an existing segment so ordered collection exercises its
        // whole-block reservation, including partial commits on overflow.
        naturalTokens.AddBits(0, 0);
        orderedTokens.AddBits(0, 0);
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(natural, ref previousNatural,
            dcNatural, acNatural, tokens: naturalTokens));
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(ordered, ref previousOrdered,
            dcOrdered, acOrdered, tokens: orderedTokens, zigZagOrdered: true));
        Assert.Equal(previousNatural, previousOrdered);
        Assert.Equal(dcNatural, dcOrdered);
        Assert.Equal(acNatural, acOrdered);
        HuffmanTable table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        var coding = new HuffmanCodingPlan.ScanCoding([table], [table], [], []);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var naturalWriter = new BitWriter(expected);
        var orderedWriter = new BitWriter(actual);
        naturalTokens.Replay(naturalWriter, coding, TestContext.Current.CancellationToken);
        orderedTokens.Replay(orderedWriter, coding, TestContext.Current.CancellationToken);
        naturalWriter.Flush();
        orderedWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReservedBlockOverflowAtDcZrlOrEobRetainsOnlyEarlierTokens(int failure)
    {
        short[] natural = new short[64];
        natural[0] = 5;
        if (failure == 1)
            natural[JpegConstants.ZigZag[63]] = 1;
        short[] ordered = Order(natural);
        long[] dcNatural = new long[256], dcOrdered = new long[256], acNatural = new long[256], acOrdered = new long[256];
        if (failure == 0)
            dcNatural[3] = dcOrdered[3] = long.MaxValue;
        else
            acNatural[failure == 1 ? 0xf0 : 0] = acOrdered[failure == 1 ? 0xf0 : 0] = long.MaxValue;
        using var naturalTokens = new EntropyTokens();
        using var orderedTokens = new EntropyTokens();
        naturalTokens.AddBits(0x42, 8);
        orderedTokens.AddBits(0x42, 8);
        int previousNatural = 0, previousOrdered = 0;
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(natural, ref previousNatural,
            dcNatural, acNatural, tokens: naturalTokens));
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(ordered, ref previousOrdered,
            dcOrdered, acOrdered, tokens: orderedTokens, zigZagOrdered: true));
        Assert.Equal(dcNatural, dcOrdered);
        Assert.Equal(acNatural, acOrdered);
        Assert.Equal(previousNatural, previousOrdered);
        HuffmanTable table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        var coding = new HuffmanCodingPlan.ScanCoding([table], [table], [], []);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var naturalWriter = new BitWriter(expected);
        var orderedWriter = new BitWriter(actual);
        naturalTokens.Replay(naturalWriter, coding, TestContext.Current.CancellationToken);
        orderedTokens.Replay(orderedWriter, coding, TestContext.Current.CancellationToken);
        naturalWriter.Flush();
        orderedWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(62)]
    public void RepeatedSymbolsCommitExactlyThePrefixBeforeHistogramOverflow(int headroom)
    {
        short[] block = Enumerable.Repeat((short)1, 64).ToArray();
        block[0] = 5;
        long[] expectedDc = new long[256], actualDc = new long[256];
        long[] expectedAc = new long[256], actualAc = new long[256];
        expectedAc[1] = actualAc[1] = long.MaxValue - headroom;
        using var expectedTokens = new EntropyTokens();
        using var actualTokens = new EntropyTokens();
        expectedTokens.AddBits(0x42, 8);
        actualTokens.AddBits(0x42, 8);
        int expectedPrevious = 0, actualPrevious = 0;
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(block, ref expectedPrevious,
            expectedDc, expectedAc, tokens: expectedTokens));
        Assert.Throws<OverflowException>(() => EntropyEncoder.AccumulateFrequencies(block, ref actualPrevious,
            actualDc, actualAc, tokens: actualTokens, zigZagOrdered: true));
        Assert.Equal(expectedDc, actualDc);
        Assert.Equal(expectedAc, actualAc);
        Assert.Equal(expectedPrevious, actualPrevious);
        HuffmanTable table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        var coding = new HuffmanCodingPlan.ScanCoding([table], [table], [], []);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var expectedWriter = new BitWriter(expected, buffered: true);
        var actualWriter = new BitWriter(actual, buffered: true);
        expectedTokens.Replay(expectedWriter, coding, TestContext.Current.CancellationToken);
        actualTokens.Replay(actualWriter, coding, TestContext.Current.CancellationToken);
        expectedWriter.Flush();
        actualWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    private static short[] Order(short[] natural)
        => JpegConstants.ZigZag.ToArray().Select(index => natural[index]).ToArray();
}
