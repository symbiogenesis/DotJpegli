// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

#if NETSTANDARD2_1
using UInt128 = System.Numerics.BigInteger;
#endif
using System.Runtime.InteropServices;

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
        if (!TryReadSparseHistogram(frequencies, out int firstSymbol, out long first,
                out int secondSymbol, out long second))
            return CreateOptimizedTree(frequencies);
        byte[] histogram = new byte[16];
        histogram[0] = 1;
        if (secondSymbol < 0)
            return new HuffmanTable(histogram, [(byte)Math.Max(0, firstSymbol)]);
        // The unit-weight padding symbol pairs with the smaller real weight.
        // Equal real weights give the lower symbol the shorter code, matching
        // the tree's descending-symbol ordering before its first merge.
        histogram[1] = 1;
        if (first < second)
            (firstSymbol, secondSymbol) = (secondSymbol, firstSymbol);
        return new HuffmanTable(histogram, [(byte)firstSymbol, (byte)secondSymbol]);
    }

    private static HuffmanTable CreateOptimizedTree(ReadOnlySpan<long> frequencies)
    {
        Span<int> depth = stackalloc int[257];
        BuildCodeLengths(frequencies, depth);
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
        if (!TryReadSparseHistogram(frequencies, out _, out long first, out int secondSymbol, out long second))
            return EstimateTreeCost(frequencies);
        // The padding pseudo-symbol has weight one. Zero/singleton inputs use
        // one real length-one code; two real symbols use lengths one and two,
        // assigning the shorter code to the larger frequency. Include the
        // selector, sixteen counts and one byte per emitted real symbol.
        return secondSymbol < 0
            ? HistogramCost((UInt128)first + 144)
            : HistogramCost((UInt128)Math.Max(first, second) + (UInt128)Math.Min(first, second) * 2 + 152);
    }

    private static bool TryReadSparseHistogram(ReadOnlySpan<long> frequencies,
        out int firstSymbol, out long first, out int secondSymbol, out long second)
    {
        if (frequencies.Length != 256)
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));
        firstSymbol = secondSymbol = -1;
        first = second = 0;
        for (int symbol = 0; symbol < frequencies.Length; symbol++)
        {
            long frequency = frequencies[symbol];
            if (frequency < 0)
                throw new ArgumentOutOfRangeException(nameof(frequencies), "Symbol frequencies cannot be negative.");
            if (frequency == 0)
                continue;
            if (firstSymbol < 0)
                (firstSymbol, first) = (symbol, frequency);
            else if (secondSymbol < 0)
                (secondSymbol, second) = (symbol, frequency);
            else
                return false;
        }
        return true;
    }

    private static float EstimateTreeCost(ReadOnlySpan<long> frequencies)
    {
        Span<int> depth = stackalloc int[257];
        bool narrow = BuildCodeLengths(frequencies, depth);
        int symbolCount = 0;
        if (narrow)
        {
            ulong narrowBits = 0;
            for (int symbol = 0; symbol < 256; symbol++)
            {
                symbolCount += depth[symbol] == 0 ? 0 : 1;
                narrowBits += (ulong)frequencies[symbol] * (uint)depth[symbol];
            }
            // Keep the same final conversion as the wide path, including the
            // portable target's double intermediate.
            return HistogramCost((UInt128)(narrowBits + (uint)((17 + symbolCount) * 8)));
        }
        UInt128 bits = 0;
        for (int symbol = 0; symbol < 256; symbol++)
        {
            symbolCount += depth[symbol] == 0 ? 0 : 1;
            bits += (UInt128)frequencies[symbol] * (uint)depth[symbol];
        }
        return HistogramCost(bits + (uint)((17 + symbolCount) * 8));
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
        return HistogramCost(bits);
    }

    private static float HistogramCost(UInt128 bits) =>
#if NETSTANDARD2_1
        // Match UInt128's float conversion, including its double intermediate.
        (float)(double)bits;
#else
        (float)bits;
#endif

    private static bool BuildCodeLengths(ReadOnlySpan<long> frequencies, Span<int> depth)
    {
        if (frequencies.Length != 256)
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));

#if NETSTANDARD2_1
        bool narrow = true;
        int symbols = 0;
        for (int symbol = 0; symbol < frequencies.Length; symbol++)
        {
            long frequency = frequencies[symbol];
            if (frequency < 0)
                throw new ArgumentOutOfRangeException(nameof(frequencies), "Symbol frequencies cannot be negative.");
            narrow &= frequency <= uint.MaxValue;
            symbols += frequency == 0 ? 0 : 1;
        }
#else
        bool narrow = !frequencies.ContainsAnyExceptInRange(0L, (long)uint.MaxValue);
        if (!narrow && frequencies.ContainsAnyInRange(long.MinValue, -1L))
            throw new ArgumentOutOfRangeException(nameof(frequencies), "Symbol frequencies cannot be negative.");
        int symbols = frequencies.Length - frequencies.Count(0L);
#endif
        int leaves = Math.Max(1, symbols) + 1;
        if (narrow)
        {
            // At the uint bound, every retry reaches uniform weights by the
            // time minimumCount reaches uint.MaxValue. All 257 leaf weights
            // and their merged sum remain below 2^41; a 16-bit code cost is
            // below 2^45. Neither can collide with the ulong sentinel.
            // Positive long and ulong weights have identical representations;
            // read the validated histogram directly instead of copying it.
            Span<Node<ulong>> narrowTree = stackalloc Node<ulong>[2 * leaves + 1];
            BuildCodeLengths<ulong, NarrowArithmetic>(MemoryMarshal.Cast<long, ulong>(frequencies), depth, narrowTree);
            return true;
        }

        Span<UInt128> counts =
#if NETSTANDARD2_1
            new UInt128[256];
#else
            stackalloc UInt128[256];
#endif
        for (int symbol = 0; symbol < frequencies.Length; symbol++)
            counts[symbol] = (UInt128)frequencies[symbol];
        Span<Node<UInt128>> tree =
#if NETSTANDARD2_1
            new Node<UInt128>[2 * leaves + 1];
#else
            stackalloc Node<UInt128>[2 * leaves + 1];
#endif
        BuildCodeLengths<UInt128, WideArithmetic>(counts, depth, tree);
        return false;
    }

    private static void BuildCodeLengths<T, TArithmetic>(ReadOnlySpan<T> frequencies,
        Span<int> depth, Span<Node<T>> tree)
        where T : struct, IComparable<T>
        where TArithmetic : struct, ITreeArithmetic<T>
    {
        // Every retry has at most one leaf per frequency, one internal node per
        // merge, and two sentinels. Reuse this bounded storage across retries.
        // Value-type arithmetic lets the JIT specialize this one algorithm for
        // bounded ulong counts and for the exact UInt128/BigInteger fallback.
        TArithmetic arithmetic = default;
        T minimumCount = default;
        // Every retry writes the same active symbols. Inactive depths need
        // clearing only once, rather than zeroing the full array per retry.
        depth.Clear();
        while (true)
        {
            int leafCount = 0;
            for (int symbol = frequencies.Length - 1; symbol >= 0; symbol--)
            {
                T count = frequencies[symbol];
                if (count.CompareTo(default) != 0)
                {
                    tree[leafCount++] = new Node<T>(count.CompareTo(minimumCount) >= 0 ? count : minimumCount, -1, symbol);
                }
            }
            T pseudoCount = arithmetic.One.CompareTo(minimumCount) >= 0 ? arithmetic.One : minimumCount;
            if (leafCount == 0)
                tree[leafCount++] = new Node<T>(pseudoCount, -1, 0);
            // Removing this padding symbol leaves unused code space, keeping
            // all-ones codes unavailable to real symbols, as in the wide tree.
            tree[leafCount++] = new Node<T>(pseudoCount, -1, frequencies.Length);

            tree[..leafCount].Sort(static (left, right) =>
            {
                int comparison = left.Count.CompareTo(right.Count);
                return comparison != 0 ? comparison : right.RightOrSymbol.CompareTo(left.RightOrSymbol);
            });
            Node<T> sentinel = new(arithmetic.Sentinel, -1, -1);
            tree[leafCount] = sentinel;
            tree[leafCount + 1] = sentinel;
            int leafIndex = 0;
            int parentIndex = leafCount + 1;
            for (int merge = 1; merge < leafCount; merge++)
            {
                int left = tree[leafIndex].Count.CompareTo(tree[parentIndex].Count) <= 0 ? leafIndex++ : parentIndex++;
                int right = tree[leafIndex].Count.CompareTo(tree[parentIndex].Count) <= 0 ? leafIndex++ : parentIndex++;
                // UInt128 keeps even 256 long.MaxValue frequencies exact;
                // no saturated internal node can collide with the sentinel.
                tree[leafCount + merge] = new Node<T>(arithmetic.Add(tree[left].Count, tree[right].Count), left, right);
                tree[leafCount + merge + 1] = sentinel;
            }

            SetDepth<T>(tree, 2 * leafCount - 1, depth, 0);
            if (!depth.ContainsAnyInRange(17, int.MaxValue))
            {
                return;
            }

            // CreateHuffmanTree uses limits 1, 2, 4... and clamps at limit-1.
            // This retry eventually makes leaf weights nearly uniform.
            minimumCount = arithmetic.NextMinimum(minimumCount);
        }
    }

    private static void SetDepth<T>(ReadOnlySpan<Node<T>> tree, int nodeIndex, Span<int> depth, int level)
    {
        Node<T> node = tree[nodeIndex];
        if (node.Left < 0)
        {
            depth[node.RightOrSymbol] = level;
            return;
        }

        SetDepth(tree, node.Left, depth, level + 1);
        SetDepth(tree, node.RightOrSymbol, depth, level + 1);
    }

    private interface ITreeArithmetic<T>
    {
        T One { get; }
        T Sentinel { get; }
        T Add(T left, T right);
        T NextMinimum(T minimum);
    }

    private readonly struct NarrowArithmetic : ITreeArithmetic<ulong>
    {
        public ulong One => 1;
        public ulong Sentinel => ulong.MaxValue;
        public ulong Add(ulong left, ulong right) => left + right;
        public ulong NextMinimum(ulong minimum) => minimum == 0 ? 1 : minimum * 2 + 1;
    }

    private readonly struct WideArithmetic : ITreeArithmetic<UInt128>
    {
        public UInt128 One => 1;
        public UInt128 Sentinel =>
#if NETSTANDARD2_1
            (UInt128.One << 128) - 1;
#else
            UInt128.MaxValue;
#endif
        public UInt128 Add(UInt128 left, UInt128 right) => left + right;
        public UInt128 NextMinimum(UInt128 minimum) => minimum == 0 ? 1 : minimum * 2 + 1;
    }

    private readonly record struct Node<T>(T Count, int Left, int RightOrSymbol);
}
