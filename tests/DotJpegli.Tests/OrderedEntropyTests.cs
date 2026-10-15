using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class OrderedEntropyTests
{
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

    private static short[] Order(short[] natural)
        => JpegConstants.ZigZag.ToArray().Select(index => natural[index]).ToArray();
}
