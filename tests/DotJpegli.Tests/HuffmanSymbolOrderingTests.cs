namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class HuffmanSymbolOrderingTests
{
    [Fact]
    public void OrdersPreserveEveryLengthAndCostWithoutMutatingTheControl()
    {
        long[] frequencies = Enumerable.Range(0, 256).Select(index => (long)((index * 13 % 29) + 1)).ToArray();
        var control = HuffmanTable.CreateOptimized(frequencies, HuffmanSymbolOrder.Canonical);
        var defaultOptimized = HuffmanTable.CreateOptimized(frequencies);
        Assert.Equal(HuffmanSymbolOrdering.Reorder(control, frequencies, HuffmanSymbolOrder.FrequencyDescending).Symbols,
            defaultOptimized.Symbols);
        byte[] counts = (byte[])control.Counts.Clone(), symbols = (byte[])control.Symbols.Clone();
        ulong[] replay = control.GetPrebiasedCodes(), originalReplay = (ulong[])replay.Clone();
        byte[] lengths = Lengths(control);
        foreach (HuffmanSymbolOrder order in Enum.GetValues<HuffmanSymbolOrder>())
        {
            HuffmanTable reordered = HuffmanSymbolOrdering.Reorder(control, frequencies, order);
            Assert.Equal(counts, reordered.Counts);
            Assert.Equal(lengths, Lengths(reordered));
            Assert.Equal(control.EstimateCost(frequencies), reordered.EstimateCost(frequencies));
            Assert.Equal(reordered.Symbols, HuffmanSymbolOrdering.Reorder(control, frequencies, order).Symbols);
            if (order == HuffmanSymbolOrder.Canonical)
            {
                Assert.Same(control, reordered);
            }
            else
            {
                Assert.NotEqual(symbols, reordered.Symbols);
                Assert.NotSame(replay, reordered.GetPrebiasedCodes());
            }
            int start = 0;
            foreach (byte count in counts)
            {
                byte[] group = symbols.AsSpan(start, count).ToArray();
                IEnumerable<byte> expected = order switch
                {
                    HuffmanSymbolOrder.Canonical => group,
                    HuffmanSymbolOrder.ReverseWithinLength => group.Reverse(),
                    HuffmanSymbolOrder.FrequencyDescending => group.OrderByDescending(symbol => frequencies[symbol]).ThenBy(symbol => symbol),
                    HuffmanSymbolOrder.FrequencyAscending => group.OrderBy(symbol => frequencies[symbol]).ThenBy(symbol => symbol),
                    _ => throw new InvalidOperationException()
                };
                Assert.Equal(expected, reordered.Symbols.AsSpan(start, count).ToArray());
                start += count;
            }
        }
        Assert.Equal(counts, control.Counts);
        Assert.Equal(symbols, control.Symbols);
        Assert.Equal(originalReplay, replay);
    }

    [Fact]
    public void InvalidPermutationsAndSettingsAreRejected()
    {
        var table = HuffmanTable.CreateDefault(false, false);
        _ = Assert.Throws<ArgumentException>(() => table.ReorderWithinLengths([]));
        byte[] symbols = (byte[])table.Symbols.Clone();
        symbols[0] = symbols[1];
        _ = Assert.Throws<ArgumentException>(() => table.ReorderWithinLengths(symbols));
        symbols = (byte[])table.Symbols.Clone();
        (symbols[0], symbols[1]) = (symbols[1], symbols[0]);
        _ = Assert.Throws<ArgumentException>(() => table.ReorderWithinLengths(symbols));
        _ = Assert.Throws<ArgumentException>(() => HuffmanSymbolOrdering.Reorder(table, [], HuffmanSymbolOrder.Canonical));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => HuffmanSymbolOrdering.Reorder(table, new long[256], (HuffmanSymbolOrder)100));
        long[] negative = new long[256];
        negative[0] = -1;
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => HuffmanSymbolOrdering.Reorder(table, negative, HuffmanSymbolOrder.FrequencyDescending));
    }

    private static byte[] Lengths(HuffmanTable table)
    {
        byte[] lengths = new byte[256];
        int offset = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int index = 0; index < table.Counts[length - 1]; index++)
            {
                lengths[table.Symbols[offset++]] = (byte)length;
            }
        }
        return lengths;
    }
}
