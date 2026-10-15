using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class HuffmanTableOptimizationTests
{
    [Fact]
    public void CombinedSymbolAndSuffixWritesPreserveEntropyBytes()
    {
        HuffmanTable table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var separate = new BitWriter(expected);
        var combined = new BitWriter(actual);
        for (int symbol = 0; symbol < 256; symbol++)
            for (int count = 0; count <= 14; count++)
            {
                uint bits = (uint)(symbol * 131 + count * 17) & ((1U << count) - 1);
                table.WriteSymbol(separate, symbol);
                separate.WriteBits(bits, count);
                table.WriteSymbolWithBits(combined, symbol, bits, count);
            }
        separate.Flush();
        combined.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Fact]
    public void EmptyAndSingletonTablesMatchIndependentReference()
    {
        AssertMatchesReference(new long[256]);
        for (int symbol = 0; symbol < 256; symbol++)
        {
            long[] frequencies = new long[256];
            frequencies[symbol] = symbol % 2 == 0 ? 1 : long.MaxValue;
            AssertMatchesReference(frequencies);
        }
    }

    [Fact]
    public void DenseAndSparseTiesPreserveExactCanonicalTables()
    {
        foreach (long frequency in new long[] { 1, 2, 65535, long.MaxValue })
        {
            long[] frequencies = new long[256];
            Array.Fill(frequencies, frequency);
            AssertMatchesReference(frequencies);

            for (int symbol = 0; symbol < 256; symbol++)
            {
                frequencies[symbol] = symbol % 3 == 0 ? 0 : frequency;
            }
            AssertMatchesReference(frequencies);
        }

        // Leaves and already-merged parents frequently have equal weights.
        AssertMatchesReference(Enumerable.Range(0, 256)
            .Select(symbol => 1L << (symbol % 12)).ToArray());
    }

    [Fact]
    public void SkewedTablesPreserveLengthLimitRetries()
    {
        long[] frequencies = new long[256];
        frequencies[0] = 1;
        frequencies[1] = 1;
        for (int symbol = 2; symbol < 89; symbol++)
        {
            frequencies[symbol] = checked(frequencies[symbol - 1] + frequencies[symbol - 2]);
        }

        ReferenceTable expected = BuildReference(frequencies);
        Assert.True(expected.Retries > 1);
        AssertMatchesReference(frequencies, expected);

        // UInt128 accumulation must remain exact when internal weights exceed long.
        Array.Fill(frequencies, long.MaxValue);
        for (int symbol = 0; symbol < 63; symbol++)
        {
            frequencies[symbol] = 1L << symbol;
        }
        AssertMatchesReference(frequencies);
    }

    [Fact]
    public void RandomTablesMatchIndependentReference()
    {
        var random = new Random(98173);
        for (int sample = 0; sample < 64; sample++)
        {
            long[] frequencies = new long[256];
            for (int symbol = 0; symbol < frequencies.Length; symbol++)
            {
                frequencies[symbol] = random.Next(4) == 0 ? 0 :
                    sample % 3 == 0 ? random.Next(1, 8) :
                    sample % 3 == 1 ? random.NextInt64(1, long.MaxValue) :
                    1L << random.Next(0, 63);
            }
            AssertMatchesReference(frequencies);
        }
    }

    private static void AssertMatchesReference(long[] frequencies, ReferenceTable? expected = null)
    {
        expected ??= BuildReference(frequencies);
        HuffmanTable actual = HuffmanTable.CreateOptimized(frequencies);
        Assert.Equal(expected.Counts, actual.Counts);
        Assert.Equal(expected.Symbols, actual.Symbols);
        Assert.Equal(BitConverter.SingleToInt32Bits(actual.EstimateCost(frequencies)),
            BitConverter.SingleToInt32Bits(HuffmanTable.EstimateOptimizedCost(frequencies)));
    }

    // Deliberately use a slow unordered priority set instead of the production
    // sorted-leaf/internal-node queues, with explicit tie ordering for both.
    private static ReferenceTable BuildReference(long[] frequencies)
    {
        UInt128 minimumCount = 0;
        int retries = 0;
        while (true)
        {
            List<ReferenceNode> available = [];
            for (int symbol = 0; symbol < 256; symbol++)
            {
                if (frequencies[symbol] != 0)
                {
                    available.Add(new ReferenceNode(UInt128.Max((UInt128)frequencies[symbol], minimumCount), symbol, 0));
                }
            }
            if (available.Count == 0)
            {
                available.Add(new ReferenceNode(UInt128.Max(1, minimumCount), 0, 0));
            }
            available.Add(new ReferenceNode(UInt128.Max(1, minimumCount), 256, 0));

            int order = 0;
            while (available.Count > 1)
            {
                ReferenceNode left = RemoveMinimum(available);
                ReferenceNode right = RemoveMinimum(available);
                available.Add(new ReferenceNode(left.Count + right.Count, -1, order++, left, right));
            }

            int[] depths = new int[257];
            SetReferenceDepths(available[0], depths, 0);
            if (depths.Max() <= 16)
            {
                byte[] counts = Enumerable.Range(1, 16)
                    .Select(length => checked((byte)depths.Take(256).Count(depth => depth == length)))
                    .ToArray();
                byte[] symbols = Enumerable.Range(0, 256)
                    .Where(symbol => depths[symbol] != 0)
                    .OrderBy(symbol => depths[symbol]).ThenBy(symbol => symbol)
                    .Select(symbol => (byte)symbol).ToArray();
                return new ReferenceTable(counts, symbols, retries);
            }

            minimumCount = minimumCount == 0 ? 1 : minimumCount * 2 + 1;
            retries++;
            Assert.True(retries < 128, "The reference tree did not reach JPEG's depth limit.");
        }
    }

    private static ReferenceNode RemoveMinimum(List<ReferenceNode> available)
    {
        int minimum = 0;
        for (int index = 1; index < available.Count; index++)
        {
            ReferenceNode candidate = available[index];
            ReferenceNode current = available[minimum];
            int comparison = candidate.Count.CompareTo(current.Count);
            if (comparison == 0)
            {
                comparison = (candidate.Symbol < 0).CompareTo(current.Symbol < 0);
            }
            if (comparison == 0)
            {
                comparison = candidate.Symbol >= 0
                    ? current.Symbol.CompareTo(candidate.Symbol)
                    : candidate.Order.CompareTo(current.Order);
            }
            if (comparison < 0)
            {
                minimum = index;
            }
        }

        ReferenceNode result = available[minimum];
        available.RemoveAt(minimum);
        return result;
    }

    private static void SetReferenceDepths(ReferenceNode node, int[] depths, int level)
    {
        if (node.Symbol >= 0)
        {
            depths[node.Symbol] = level;
            return;
        }
        SetReferenceDepths(node.Left!, depths, level + 1);
        SetReferenceDepths(node.Right!, depths, level + 1);
    }

    private sealed record ReferenceNode(UInt128 Count, int Symbol, int Order,
        ReferenceNode? Left = null, ReferenceNode? Right = null);

    private sealed record ReferenceTable(byte[] Counts, byte[] Symbols, int Retries);
}
