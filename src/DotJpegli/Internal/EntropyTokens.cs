namespace DotJpegli.Internal;

using System.Buffers;
using System.Diagnostics;
#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

/// <summary>Bounded pooled entropy decisions retained between histogram collection and emission.</summary>
internal sealed partial class EntropyTokens : IDisposable
{
    // Preserve the original 32KB initial and 512KB maximum rental sizes.
    private const int DefaultSegmentLength = 8192;
    private const int MaximumSegmentLength = 131072;
    // Pinned jpegli's Token stores a 16-bit suffix and 8-bit symbol in four
    // bytes (encode_internal.h). Three context bits leave five explicit count
    // bits for our noncanonical-suffix fallback. Contexts 6/7 encode raw/restart.
    private const int CountShift = 27;
    private const int SymbolShift = 16;
    private const int ContextShift = 24;
    private const uint RawBitsContext = 6U << ContextShift;
    private const uint RestartContext = 7U << ContextShift;
    private readonly List<uint[]> segments = [];
    private readonly EntropyTokenBudget budget;
    private readonly ArrayPool<uint> pool;
    private readonly int huffmanContextMask;
    private int nextSegmentLength;
    private uint[]? current;
    private int currentCount;
    private long rentedBytes;
    private bool hasRestart;

    internal EntropyTokens(long maximumBytes = EntropyTokenBudget.DefaultMaximumBytes, int huffmanContextMask = 63)
        : this(new EntropyTokenBudget(maximumBytes), huffmanContextMask)
    {
    }

    internal EntropyTokens(EntropyTokenBudget budget, int huffmanContextMask = 63, ArrayPool<uint>? pool = null)
    {
        Guard.ThrowIfNull(budget);
        if ((uint)huffmanContextMask > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(huffmanContextMask));
        }

        this.huffmanContextMask = huffmanContextMask;
        this.budget = budget;
        this.pool = pool ?? ArrayPool<uint>.Shared;
        uint available = (uint)Math.Min(DefaultSegmentLength, budget.MaximumBytes / sizeof(uint));
        // Shared-pool buckets start at sixteen elements. Smaller budgets simply
        // abandon on the first add; no allocation is needed for empty scans.
        this.nextSegmentLength = available < 16 ? 0 : 1 << BitOperations.Log2(available);
    }

    internal bool IsComplete { get; private set; } = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHuffman(int component, bool isAc, int symbol, uint suffix, int count)
    {
        Debug.Assert((uint)component < 3 && (uint)symbol < 256 && (uint)count <= 16);
        Debug.Assert(count == 32 || (suffix >> count) == 0);
        int context = (component << 1) | (isAc ? 1 : 0);
        this.Add(PackHuffman(context, symbol, suffix, count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint PackHuffman(int context, int symbol, uint suffix, int count)
        => suffix | ((uint)count << CountShift) | ((uint)symbol << SymbolShift)
            | ((uint)context << ContextShift);

    // A sequential block emits at most 64 tokens. Borrow only existing space;
    // ordinary Add handles allocation, segment boundaries and small budgets.
    internal bool TryReserveBlock(out Span<uint> destination)
    {
        uint[]? segment = this.current;
        if (this.IsComplete && segment is not null && segment.Length - this.currentCount >= 64)
        {
            destination = segment.AsSpan(this.currentCount, 64);
            return true;
        }
        destination = default;
        return false;
    }

    internal void CommitBlock(int count)
    {
        Debug.Assert(this.IsComplete && this.current is not null && (uint)count <= 64 && this.currentCount + count <= this.current.Length);
        this.currentCount += count;
    }

    internal void AddBits(uint bits, int count)
    {
        Debug.Assert((uint)count <= 32 && (count == 32 || (bits >> count) == 0));
        if (count > 24)
        {
            // Raw payloads have 24 available bits. Keep stream order, including
            // when these two records cross a segment or abandon the budget.
            int remaining = count - 24;
            this.Add(RawBitsContext | (24U << CountShift) | (bits >> remaining));
            this.Add(RawBitsContext | ((uint)remaining << CountShift) | (bits & ((1U << remaining) - 1)));
        }
        else
        {
            this.Add(RawBitsContext | bits | ((uint)count << CountShift));
        }
    }

    internal void AddRestart(int index)
    {
        Debug.Assert((uint)index < 8);
        this.hasRestart = true;
        this.Add(RestartContext | (uint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(uint token)
    {
        if (!this.IsComplete)
        {
            return;
        }

        uint[]? segment = this.current;
        int index = this.currentCount;
        if (segment is not null && (uint)index < (uint)segment.Length)
        {
            segment[index] = token;
            this.currentCount = index + 1;
            return;
        }
        this.AddSegment(token);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddSegment(uint token)
    {
        uint available = (uint)Math.Min(MaximumSegmentLength, this.budget.AvailableBytes / sizeof(uint));
        if (this.nextSegmentLength == 0 || available < 16)
        {
            this.Dispose();
            return;
        }
        // A small first segment retains the existing tiny-image behavior.
        // Growing segments use fewer rentals for larger scans, while the final
        // power-of-two request still fits the remaining actual-storage budget.
        int segmentLength = Math.Min(this.nextSegmentLength, 1 << BitOperations.Log2(available));
        uint[] next = this.pool.Rent(segmentLength);
        long bytes = (long)next.Length * sizeof(uint);
        if (!this.budget.TryReserve(bytes))
        {
            this.pool.Return(next);
            this.Dispose();
            return;
        }
        try
        {
            this.segments.Add(next);
        }
        catch
        {
            this.budget.Release(bytes);
            this.pool.Return(next);
            throw;
        }
        this.rentedBytes += bytes;
        this.nextSegmentLength = Math.Min(MaximumSegmentLength, next.Length * 2);
        this.current = next;
        next[0] = token;
        this.currentCount = 1;
    }

    internal void Replay(BitWriter writer, HuffmanCodingPlan.ScanCoding coding, CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(writer);
        Guard.ThrowIfNull(coding);
        if (!this.IsComplete)
        {
            throw new InvalidOperationException("An abandoned entropy token sequence cannot be replayed.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (this.segments.Count == 0)
        {
            return;
        }
        // Tiny scans do not amortize preparing and copying 256 lookup entries
        // per Huffman context. Their complete bounded stream can use the strict
        // symbol writer directly, including noncanonical suffix widths.
        if (this.segments.Count == 1 && this.currentCount <= 32)
        {
            this.ReplaySmall(writer, coding, cancellationToken);
            return;
        }
        // Progressive scans usually emit one Huffman context. Reuse that
        // immutable table's prepared array without staging a copy on the stack.
        if (this.huffmanContextMask != 0 && (this.huffmanContextMask & (this.huffmanContextMask - 1)) == 0)
        {
            int context = BitOperations.Log2((uint)this.huffmanContextMask);
            HuffmanTable[] tables = (context & 1) == 0 ? coding.DcTables : coding.AcTables;
            ReadOnlySpan<ulong> selected = context / 2 < tables.Length ? tables[context / 2].GetPrebiasedCodes() : [];
            this.ReplayWithLookups(writer, coding, selected, context * 256, cancellationToken);
            return;
        }

        // Multiple contexts retain contiguous lookup storage. Raw-only scans
        // need no Huffman lookup storage.
        int contextCount = this.huffmanContextMask == 0 ? 0 : BitOperations.Log2((uint)this.huffmanContextMask) + 1;
        Span<ulong> lookups = stackalloc ulong[contextCount * 256];
        lookups.Clear();
        for (int component = 0; component < coding.DcTables.Length; component++)
        {
            if ((this.huffmanContextMask & (1 << (component * 2))) != 0)
            {
                coding.DcTables[component].GetPrebiasedCodes().CopyTo(lookups.Slice(component * 512, 256));
            }
        }

        for (int component = 0; component < coding.AcTables.Length; component++)
        {
            if ((this.huffmanContextMask & (2 << (component * 2))) != 0)
            {
                coding.AcTables[component].GetPrebiasedCodes().CopyTo(lookups.Slice((component * 512) + 256, 256));
            }
        }

        this.ReplayWithLookups(writer, coding, lookups, 0, cancellationToken);
    }

    private void ReplayWithLookups(BitWriter writer, HuffmanCodingPlan.ScanCoding coding,
        ReadOnlySpan<ulong> lookups, int lookupOffset, CancellationToken cancellationToken)
        => this.ReplayRangeWithLookups(writer, coding, lookups, lookupOffset,
            default, new TokenPosition(this.segments.Count, 0), cancellationToken);

    private void ReplayRangeWithLookups(BitWriter writer, HuffmanCodingPlan.ScanCoding coding,
        ReadOnlySpan<ulong> lookups, int lookupOffset, TokenPosition start, TokenPosition endPosition,
        CancellationToken cancellationToken)
    {
        for (int segmentIndex = start.Segment; segmentIndex < this.segments.Count && segmentIndex <= endPosition.Segment; segmentIndex++)
        {
            uint[] segment = this.segments[segmentIndex];
            int length = segmentIndex == endPosition.Segment ? endPosition.Offset
                : segmentIndex == this.segments.Count - 1 ? this.currentCount : segment.Length;
            int first = segmentIndex == start.Segment ? start.Offset : 0;
            for (int chunk = first; chunk < length; chunk += 256)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int end = Math.Min(chunk + 256, length);
                int index = chunk;
                while (index < end)
                {
                    index += writer.WriteEntropyTokens(segment.AsSpan(index, end - index), lookups, lookupOffset);
                    if (index == end)
                    {
                        break;
                    }

                    uint token = segment[index++];
                    int context = (int)(token >> ContextShift) & 7;
                    int count = (int)(token >> CountShift);
                    if (context < 6)
                    {
                        // Context and symbol occupy adjacent bits in the token.
                        // One index avoids the jagged table lookup per symbol.
                        int lookupIndex = (int)(token >> SymbolShift) & 2047;
                        int relativeIndex = lookupIndex - lookupOffset;
                        ulong prepared = (uint)relativeIndex < (uint)lookups.Length ? lookups[relativeIndex] : 0;
                        if ((int)(prepared >> 38) == count + 1)
                        {
                            writer.WriteBitsPrevalidated((uint)prepared | (token & 0xffffU), (int)(prepared >> 32) & 63);
                        }
                        else
                        {
                            HuffmanTable table = (context & 1) == 0 ? coding.DcTables[context >> 1] : coding.AcTables[context >> 1];
                            table.WriteSymbolWithBits(writer, lookupIndex & 255, token & 0xffffU, count);
                        }
                    }
                    else if (context == 6)
                    {
                        writer.WriteBitsPrevalidated(token & 0xffffffU, count);
                    }
                    else
                    {
                        writer.WriteRestartMarker((int)token & 7);
                    }
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReplaySmall(BitWriter writer, HuffmanCodingPlan.ScanCoding coding, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        uint[] segment = this.segments[0];
        int length = this.currentCount;
        for (int index = 0; index < length; index++)
        {
            uint token = segment[index];
            int context = (int)(token >> ContextShift) & 7;
            int count = (int)(token >> CountShift);
            if (context < 6)
            {
                HuffmanTable table = (context & 1) == 0 ? coding.DcTables[context >> 1] : coding.AcTables[context >> 1];
                table.WriteSymbolWithBits(writer, (int)(token >> SymbolShift) & 255, token & 0xffffU, count);
            }
            else if (context == 6)
            {
                writer.WriteBitsPrevalidated(token & 0xffffffU, count);
            }
            else
            {
                writer.WriteRestartMarker((int)token & 7);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void Dispose()
    {
        this.IsComplete = false;
        this.current = null;
        this.currentCount = 0;
        foreach (uint[] segment in this.segments)
        {
            this.pool.Return(segment);
        }

        this.segments.Clear();
        this.budget.Release(this.rentedBytes);
        this.rentedBytes = 0;
    }
}
