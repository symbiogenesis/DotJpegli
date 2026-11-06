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
        Span<byte> symbols = stackalloc byte[table.Symbols.Length];
        table.Symbols.AsSpan().CopyTo(symbols);
        OrderByLengthGroups(symbols, table.Counts, frequencies, order);
        return table.ReorderWithinLengths(symbols);
    }

    internal static void OrderByLengthGroups(Span<byte> symbols, ReadOnlySpan<byte> counts,
        ReadOnlySpan<long> frequencies, HuffmanSymbolOrder order)
    {
        int start = 0;
        if (order == HuffmanSymbolOrder.FrequencyDescending)
        {
            foreach (byte count in counts)
            {
                Span<byte> group = symbols.Slice(start, count);
                for (int index = 1; index < group.Length; index++)
                {
                    byte symbol = group[index];
                    long symbolFrequency = frequencies[symbol];
                    int previous = index - 1;
                    while (previous >= 0)
                    {
                        byte previousSymbol = group[previous];
                        long previousFrequency = frequencies[previousSymbol];
                        if (symbolFrequency < previousFrequency
                            || (symbolFrequency == previousFrequency && symbol >= previousSymbol))
                        {
                            break;
                        }

                        group[previous + 1] = previousSymbol;
                        previous--;
                    }
                    group[previous + 1] = symbol;
                }
                start += count;
            }
            return;
        }

        foreach (byte count in counts)
        {
            Span<byte> group = symbols.Slice(start, count);
            if (group.Length > 1)
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
    }

    private static bool ComesBefore(byte left, byte right, ReadOnlySpan<long> frequencies, HuffmanSymbolOrder order)
    {
        if (order == HuffmanSymbolOrder.Canonical)
        {
            return left < right;
        }
        if (order == HuffmanSymbolOrder.ReverseWithinLength)
        {
            return left > right;
        }
        int comparison = frequencies[left].CompareTo(frequencies[right]);
        return comparison == 0 ? left < right
            : order == HuffmanSymbolOrder.FrequencyDescending ? comparison > 0 : comparison < 0;
    }
}
