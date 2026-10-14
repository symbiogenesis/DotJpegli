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
        if (frequencies.Length != 256)
        {
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));
        }

        UInt128[] counts = new UInt128[257];
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
        int[] depth = BuildCodeLengths(counts);
        byte[] histogram = new byte[16];
        List<byte> symbols = new(256);
        for (int length = 1; length <= 16; length++)
        {
            for (int symbol = 0; symbol < 256; symbol++)
            {
                if (depth[symbol] == length)
                {
                    histogram[length - 1] = checked((byte)(histogram[length - 1] + 1));
                    symbols.Add((byte)symbol);
                }
            }
        }

        return new HuffmanTable(histogram, symbols.ToArray());
    }

    public void WriteSymbol(BitWriter writer, int symbol)
    {
        if ((uint)symbol >= 256 || lengths[symbol] == 0)
        {
            throw new InvalidOperationException($"The Huffman table has no code for symbol {symbol}.");
        }

        writer.WriteBits(codes[symbol], lengths[symbol]);
    }

    private static int[] BuildCodeLengths(UInt128[] frequencies)
    {
        int[] depth = new int[frequencies.Length];
        UInt128 minimumCount = 0;
        while (true)
        {
            List<Node> tree = new(2 * frequencies.Length + 1);
            for (int symbol = frequencies.Length - 1; symbol >= 0; symbol--)
            {
                if (frequencies[symbol] != 0)
                {
                    tree.Add(new Node(UInt128.Max(frequencies[symbol], minimumCount), -1, symbol));
                }
            }

            tree.Sort(static (left, right) =>
            {
                int comparison = left.Count.CompareTo(right.Count);
                return comparison != 0 ? comparison : right.RightOrSymbol.CompareTo(left.RightOrSymbol);
            });
            int leafCount = tree.Count;
            Node sentinel = new(UInt128.MaxValue, -1, -1);
            tree.Add(sentinel);
            tree.Add(sentinel);
            int leafIndex = 0;
            int parentIndex = leafCount + 1;
            for (int merge = 1; merge < leafCount; merge++)
            {
                int left = tree[leafIndex].Count <= tree[parentIndex].Count ? leafIndex++ : parentIndex++;
                int right = tree[leafIndex].Count <= tree[parentIndex].Count ? leafIndex++ : parentIndex++;
                // UInt128 keeps even 256 long.MaxValue frequencies exact;
                // no saturated internal node can collide with the sentinel.
                tree[^1] = new Node(tree[left].Count + tree[right].Count, left, right);
                tree.Add(sentinel);
            }

            Array.Clear(depth);
            SetDepth(tree, 2 * leafCount - 1, depth, 0);
            if (depth.Max() <= 16)
            {
                return depth;
            }

            // CreateHuffmanTree uses limits 1, 2, 4... and clamps at limit-1.
            // This retry eventually makes leaf weights nearly uniform.
            minimumCount = minimumCount == 0 ? 1 : minimumCount * 2 + 1;
        }
    }

    private static void SetDepth(List<Node> tree, int nodeIndex, int[] depth, int level)
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
