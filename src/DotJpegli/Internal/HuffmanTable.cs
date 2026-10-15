// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// Managed adaptation of google/jpegli lib/jpegli/huffman.cc
// (CreateHuffmanTree and AddStandardHuffmanTables) and entropy_coding.cc
// (BuildJpegHuffmanTable and BuildHuffmanCodeTable), revision
// 031a0077f5799a6041004267fc12b956c1f52a20.
internal sealed class HuffmanTable
{
    private readonly uint[] codes = new uint[256];
    private readonly byte[] lengths = new byte[256];
    private ulong[]? prebiasedCodes;

    public byte[] Counts { get; }

    public byte[] Symbols { get; }

    private HuffmanTable(byte[] counts, byte[] symbols)
    {
        Counts = counts;
        Symbols = symbols;
        uint code = 0;
        int symbolIndex = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int index = 0; index < counts[length - 1]; index++)
            {
                int symbol = symbols[symbolIndex++];
                // JPEG reserves the all-ones code for padding. Also reject
                // over-subscribed prefix trees and duplicate symbols.
                if (code >= (1U << length) - 1 || lengths[symbol] != 0)
                {
                    throw new InvalidOperationException("The Huffman table is not a valid JPEG prefix code.");
                }

                codes[symbol] = code++;
                lengths[symbol] = (byte)length;
            }

            code <<= 1;
        }

        if (symbolIndex != symbols.Length)
        {
            throw new InvalidOperationException("The Huffman table symbol count is invalid.");
        }
    }

    public static HuffmanTable CreateDefault(bool isAc, bool chroma)
    {
        if (!isAc)
        {
            return new HuffmanTable(
                chroma
                    ? [0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0]
                    : [0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0],
                [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        }

        return chroma
            ? new HuffmanTable(
                [0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 119],
                [
                    0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71,
                    0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91, 0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0,
                    0x15, 0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34, 0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
                    0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                    0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
                    0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
                    0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5,
                    0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
                    0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
                    0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                    0xf9, 0xfa,
                ])
            : new HuffmanTable(
                [0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 125],
                [
                    0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07,
                    0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08, 0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0,
                    0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
                    0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
                    0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
                    0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
                    0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7,
                    0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5,
                    0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
                    0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                    0xf9, 0xfa,
                ]);
    }

    public static HuffmanTable CreateOptimized(ReadOnlySpan<long> frequencies)
    {
        Span<UInt128> counts = stackalloc UInt128[257];
        InitializeCounts(frequencies, counts);
        Span<int> depth = stackalloc int[257];
        BuildCodeLengths(counts, depth);
        byte[] histogram = new byte[16];
        int symbolCount = 0;
        for (int symbol = 0; symbol < 256; symbol++)
        {
            int length = depth[symbol];
            if (length != 0)
            {
                histogram[length - 1] = checked((byte)(histogram[length - 1] + 1));
                symbolCount++;
            }
        }

        // Counting offsets group symbols by length while the ascending traversal
        // preserves JPEG's canonical order within each length.
        Span<int> offsets = stackalloc int[16];
        int offset = 0;
        for (int length = 0; length < histogram.Length; length++)
        {
            offsets[length] = offset;
            offset += histogram[length];
        }

        byte[] symbols = new byte[symbolCount];
        for (int symbol = 0; symbol < 256; symbol++)
        {
            int length = depth[symbol];
            if (length != 0)
            {
                symbols[offsets[length - 1]++] = (byte)symbol;
            }
        }

        return new HuffmanTable(histogram, symbols);
    }

    // Clustering needs only code lengths and the symbol count. Constructing
    // canonical codes and allocating table arrays for each trial is unnecessary.
    internal static float EstimateOptimizedCost(ReadOnlySpan<long> frequencies)
    {
        Span<UInt128> counts = stackalloc UInt128[257];
        InitializeCounts(frequencies, counts);
        Span<int> depth = stackalloc int[257];
        BuildCodeLengths(counts, depth);
        int symbolCount = 0;
        UInt128 bits = 0;
        for (int symbol = 0; symbol < 256; symbol++)
        {
            symbolCount += depth[symbol] == 0 ? 0 : 1;
            bits += (UInt128)frequencies[symbol] * (uint)depth[symbol];
        }
        return (float)(bits + (uint)((17 + symbolCount) * 8));
    }

    private static void InitializeCounts(ReadOnlySpan<long> frequencies, Span<UInt128> counts)
    {
        if (frequencies.Length != 256)
        {
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));
        }

        bool hasSymbol = false;
        for (int symbol = 0; symbol < 256; symbol++)
        {
            if (frequencies[symbol] < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequencies), "Symbol frequencies cannot be negative.");
            }

            counts[symbol] = (UInt128)frequencies[symbol];
            hasSymbol |= frequencies[symbol] != 0;
        }

        if (!hasSymbol)
        {
            counts[0] = 1;
        }

        // Removing the pseudo-symbol leaves unused code space, preventing
        // an all-ones code from being assigned to any real symbol.
        counts[256] = 1;
    }

    public void WriteSymbol(BitWriter writer, int symbol)
    {
        if ((uint)symbol >= 256 || lengths[symbol] == 0)
        {
            throw new InvalidOperationException($"The Huffman table has no code for symbol {symbol}.");
        }

        writer.WriteBits(codes[symbol], lengths[symbol]);
    }

    // Entropy callers supply a bounded, validated amplitude or EOB-run suffix.
    // The Huffman code and suffix together fit a single 32-bit write.
    public void WriteSymbolWithBits(BitWriter writer, int symbol, uint bits, int count)
    {
        if ((uint)symbol >= 256 || lengths[symbol] == 0)
            throw new InvalidOperationException($"The Huffman table has no code for symbol {symbol}.");
        writer.WriteBits((codes[symbol] << count) | bits, lengths[symbol] + count);
    }

    // WriteTokens in upstream entropy_coding.cc pre-shifts each code by the
    // symbol's JPEG suffix width. Preserve a count tag so generic/internal
    // callers with another suffix width still use the strict write method.
    internal ulong[] GetPrebiasedCodes()
    {
        if (Volatile.Read(ref prebiasedCodes) is { } existing)
            return existing;
        var prepared = new ulong[256];
        for (int symbol = 0; symbol < prepared.Length; symbol++)
        {
            int length = lengths[symbol];
            if (length == 0)
                continue;
            int count = symbol == 0xf0 ? 0 : (symbol & 15) == 0 ? symbol >> 4 : symbol & 15;
            // Low 32 bits are the shifted prefix; the next six are the total
            // length. Count+1 distinguishes valid zero-suffix symbols from
            // missing table entries, whose entire prepared value remains zero.
            prepared[symbol] = (codes[symbol] << count) | ((ulong)(uint)(length + count) << 32)
                | ((ulong)(uint)(count + 1) << 38);
        }
        return Interlocked.CompareExchange(ref prebiasedCodes, prepared, null) ?? prepared;
    }

    // HistogramCost in upstream includes the selector, 16 length counts and
    // one byte per symbol, excluding the surrounding DHT marker and length.
    internal float EstimateCost(ReadOnlySpan<long> frequencies)
    {
        if (frequencies.Length != 256)
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));
        UInt128 bits = (UInt128)((17 + Symbols.Length) * 8);
        for (int symbol = 0; symbol < frequencies.Length; symbol++)
        {
            if (frequencies[symbol] < 0 || (frequencies[symbol] != 0 && lengths[symbol] == 0))
                throw new ArgumentException("The histogram must use the table's symbols.", nameof(frequencies));
            bits += (UInt128)frequencies[symbol] * lengths[symbol];
        }
        return (float)bits;
    }

    private static void BuildCodeLengths(ReadOnlySpan<UInt128> frequencies, Span<int> depth)
    {
        // Every retry has at most one leaf per frequency, one internal node per
        // merge, and two sentinels. Reuse this bounded storage across retries.
        Span<Node> tree = stackalloc Node[2 * 257 + 1];
        UInt128 minimumCount = 0;
        while (true)
        {
            int leafCount = 0;
            for (int symbol = frequencies.Length - 1; symbol >= 0; symbol--)
            {
                if (frequencies[symbol] != 0)
                {
                    tree[leafCount++] = new Node(UInt128.Max(frequencies[symbol], minimumCount), -1, symbol);
                }
            }

            tree[..leafCount].Sort(static (left, right) =>
            {
                int comparison = left.Count.CompareTo(right.Count);
                return comparison != 0 ? comparison : right.RightOrSymbol.CompareTo(left.RightOrSymbol);
            });
            Node sentinel = new(UInt128.MaxValue, -1, -1);
            tree[leafCount] = sentinel;
            tree[leafCount + 1] = sentinel;
            int leafIndex = 0;
            int parentIndex = leafCount + 1;
            for (int merge = 1; merge < leafCount; merge++)
            {
                int left = tree[leafIndex].Count <= tree[parentIndex].Count ? leafIndex++ : parentIndex++;
                int right = tree[leafIndex].Count <= tree[parentIndex].Count ? leafIndex++ : parentIndex++;
                // UInt128 keeps even 256 long.MaxValue frequencies exact;
                // no saturated internal node can collide with the sentinel.
                tree[leafCount + merge] = new Node(tree[left].Count + tree[right].Count, left, right);
                tree[leafCount + merge + 1] = sentinel;
            }

            depth.Clear();
            SetDepth(tree, 2 * leafCount - 1, depth, 0);
            if (!depth.ContainsAnyInRange(17, int.MaxValue))
            {
                return;
            }

            // CreateHuffmanTree uses limits 1, 2, 4... and clamps at limit-1.
            // This retry eventually makes leaf weights nearly uniform.
            minimumCount = minimumCount == 0 ? 1 : minimumCount * 2 + 1;
        }
    }

    private static void SetDepth(ReadOnlySpan<Node> tree, int nodeIndex, Span<int> depth, int level)
    {
        Node node = tree[nodeIndex];
        if (node.Left < 0)
        {
            depth[node.RightOrSymbol] = level;
            return;
        }

        SetDepth(tree, node.Left, depth, level + 1);
        SetDepth(tree, node.RightOrSymbol, depth, level + 1);
    }

    private readonly record struct Node(UInt128 Count, int Left, int RightOrSymbol);
}
