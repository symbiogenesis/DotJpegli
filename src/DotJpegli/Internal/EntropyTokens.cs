using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotJpegli.Internal;

/// <summary>Bounded pooled entropy decisions retained between histogram collection and emission.</summary>
internal sealed class EntropyTokens : IDisposable
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
    private readonly long maximumBytes;
    private int nextSegmentLength;
    private uint[]? current;
    private int currentCount;
    private long rentedBytes;

    internal EntropyTokens(long maximumBytes = 64L * 1024 * 1024)
    {
        Guard.ThrowIfNegative(maximumBytes);
        this.maximumBytes = Math.Min(maximumBytes, 64L * 1024 * 1024);
        uint available = (uint)Math.Min(DefaultSegmentLength, this.maximumBytes / sizeof(uint));
        // Shared-pool buckets start at sixteen elements. Smaller budgets simply
        // abandon on the first add; no allocation is needed for empty scans.
        nextSegmentLength = available < 16 ? 0 : 1 << BitOperations.Log2(available);
    }

    internal bool IsComplete { get; private set; } = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHuffman(int component, bool isAc, int symbol, uint suffix, int count)
    {
        Debug.Assert((uint)component < 3 && (uint)symbol < 256 && (uint)count <= 16);
        Debug.Assert(count == 32 || (suffix >> count) == 0);
        int context = (component << 1) | (isAc ? 1 : 0);
        Add(PackHuffman(context, symbol, suffix, count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint PackHuffman(int context, int symbol, uint suffix, int count)
        => suffix | ((uint)count << CountShift) | ((uint)symbol << SymbolShift)
            | ((uint)context << ContextShift);

    // A sequential block emits at most 64 tokens. Borrow only existing space;
    // ordinary Add handles allocation, segment boundaries and small budgets.
    internal bool TryReserveBlock(out Span<uint> destination)
    {
        uint[]? segment = current;
        if (IsComplete && segment is not null && segment.Length - currentCount >= 64)
        {
            destination = segment.AsSpan(currentCount, 64);
            return true;
        }
        destination = default;
        return false;
    }

    internal void CommitBlock(int count)
    {
        Debug.Assert(IsComplete && current is not null && (uint)count <= 64 && currentCount + count <= current.Length);
        currentCount += count;
    }

    internal void AddBits(uint bits, int count)
    {
        Debug.Assert((uint)count <= 32 && (count == 32 || (bits >> count) == 0));
        if (count > 24)
        {
            // Raw payloads have 24 available bits. Keep stream order, including
            // when these two records cross a segment or abandon the budget.
            int remaining = count - 24;
            Add(RawBitsContext | (24U << CountShift) | (bits >> remaining));
            Add(RawBitsContext | ((uint)remaining << CountShift) | (bits & ((1U << remaining) - 1)));
        }
        else
            Add(RawBitsContext | bits | ((uint)count << CountShift));
    }

    internal void AddRestart(int index)
    {
        Debug.Assert((uint)index < 8);
        Add(RestartContext | (uint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(uint token)
    {
        if (!IsComplete)
            return;
        uint[]? segment = current;
        int index = currentCount;
        if (segment is not null && (uint)index < (uint)segment.Length)
        {
            segment[index] = token;
            currentCount = index + 1;
            return;
        }
        AddSegment(token);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddSegment(uint token)
    {
        uint available = (uint)Math.Min(MaximumSegmentLength, (maximumBytes - rentedBytes) / sizeof(uint));
        if (nextSegmentLength == 0 || available < 16)
        {
            Dispose();
            return;
        }
        // A small first segment retains the existing tiny-image behavior.
        // Growing segments use fewer rentals for larger scans, while the final
        // power-of-two request still fits the remaining actual-storage budget.
        int segmentLength = Math.Min(nextSegmentLength, 1 << BitOperations.Log2(available));
        uint[] next = ArrayPool<uint>.Shared.Rent(segmentLength);
        if (rentedBytes + (long)next.Length * sizeof(uint) > maximumBytes)
        {
            ArrayPool<uint>.Shared.Return(next);
            Dispose();
            return;
        }
        segments.Add(next);
        rentedBytes += (long)next.Length * sizeof(uint);
        nextSegmentLength = Math.Min(MaximumSegmentLength, next.Length * 2);
        current = next;
        next[0] = token;
        currentCount = 1;
    }

    internal void Replay(BitWriter writer, HuffmanCodingPlan.ScanCoding coding, CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(writer);
        Guard.ThrowIfNull(coding);
        if (!IsComplete)
            throw new InvalidOperationException("An abandoned entropy token sequence cannot be replayed.");
        cancellationToken.ThrowIfCancellationRequested();
        Span<ulong> lookups = stackalloc ulong[6 * 256];
        lookups.Clear();
        for (int component = 0; component < coding.DcTables.Length; component++)
            coding.DcTables[component].GetPrebiasedCodes().CopyTo(lookups.Slice(component * 512, 256));
        for (int component = 0; component < coding.AcTables.Length; component++)
            coding.AcTables[component].GetPrebiasedCodes().CopyTo(lookups.Slice(component * 512 + 256, 256));
        for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
        {
            uint[] segment = segments[segmentIndex];
            int length = segmentIndex == segments.Count - 1 ? currentCount : segment.Length;
            for (int chunk = 0; chunk < length; chunk += 256)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int end = Math.Min(chunk + 256, length);
                int index = chunk;
                while (index < end)
                {
                    index += writer.WriteEntropyTokens(segment.AsSpan(index, end - index), lookups);
                    if (index == end)
                        break;
                    uint token = segment[index++];
                    int context = (int)(token >> ContextShift) & 7;
                    int count = (int)(token >> CountShift);
                    if (context < 6)
                    {
                        // Context and symbol occupy adjacent bits in the token.
                        // One index avoids the jagged table lookup per symbol.
                        int lookupIndex = (int)(token >> SymbolShift) & 2047;
                        ulong prepared = lookups[lookupIndex];
                        if ((int)(prepared >> 38) == count + 1)
                            writer.WriteBitsPrevalidated((uint)prepared | (token & 0xffffU), (int)(prepared >> 32) & 63);
                        else
                        {
                            HuffmanTable table = (context & 1) == 0 ? coding.DcTables[context >> 1] : coding.AcTables[context >> 1];
                            table.WriteSymbolWithBits(writer, lookupIndex & 255, token & 0xffffU, count);
                        }
                    }
                    else if (context == 6)
                        writer.WriteBitsPrevalidated(token & 0xffffffU, count);
                    else
                        writer.WriteRestartMarker((int)token & 7);
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void Dispose()
    {
        IsComplete = false;
        current = null;
        currentCount = 0;
        rentedBytes = 0;
        foreach (uint[] segment in segments)
            ArrayPool<uint>.Shared.Return(segment);
        segments.Clear();
    }
}
