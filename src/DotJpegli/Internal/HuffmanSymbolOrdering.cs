namespace DotJpegli.Internal;

// Offline choices only. Canonical preserves the encoder's established bytes.
internal enum HuffmanSymbolOrder
{
    Canonical,
    FrequencyDescending,
    FrequencyAscending,
    ReverseWithinLength
}

internal static class HuffmanSymbolOrdering
{
    internal static HuffmanTable Reorder(HuffmanTable table, ReadOnlySpan<long> frequencies,
        HuffmanSymbolOrder order)
    {
        Guard.ThrowIfNull(table);
        if (frequencies.Length != 256)
        {
            throw new ArgumentException("A frequency table requires 256 entries.", nameof(frequencies));
        }
        foreach (long frequency in frequencies)
        {
            if (frequency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequencies));
            }
        }
        if (order is not (HuffmanSymbolOrder.Canonical or HuffmanSymbolOrder.FrequencyDescending
            or HuffmanSymbolOrder.FrequencyAscending or HuffmanSymbolOrder.ReverseWithinLength))
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }
        if (order == HuffmanSymbolOrder.Canonical)
        {
            return table;
        }
        Span<byte> symbols = stackalloc byte[table.Symbols.Length];
        table.Symbols.AsSpan().CopyTo(symbols);
        int start = 0;
        foreach (byte count in table.Counts)
        {
            Span<byte> group = symbols.Slice(start, count);
            if (order == HuffmanSymbolOrder.ReverseWithinLength)
            {
                group.Reverse();
            }
            else
            {
                // A bounded alphabet makes allocation-free insertion practical;
                // numeric ties keep results reproducible across runtimes.
                for (int index = 1; index < group.Length; index++)
                {
                    byte symbol = group[index];
                    int previous = index - 1;
                    while (previous >= 0 && ComesBefore(symbol, group[previous], frequencies, order))
                    {
                        group[previous + 1] = group[previous];
                        previous--;
                    }
                    group[previous + 1] = symbol;
                }
            }
            start += count;
        }
        return table.ReorderWithinLengths(symbols);
    }

    private static bool ComesBefore(byte left, byte right, ReadOnlySpan<long> frequencies, HuffmanSymbolOrder order)
    {
        int comparison = frequencies[left].CompareTo(frequencies[right]);
        return comparison == 0 ? left < right
            : order == HuffmanSymbolOrder.FrequencyDescending ? comparison > 0 : comparison < 0;
    }
}
