using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotJpegli.Internal;

/// <summary>Bounded pooled entropy decisions retained between histogram collection and emission.</summary>
internal sealed class EntropyTokens : IDisposable
{
    private const int DefaultSegmentLength = 4096;
    private const int CountShift = 32;
    private const int SymbolShift = 38;
    private const int ContextShift = 46;
    private const int KindShift = 49;
    private const ulong RawBitsKind = 1UL << KindShift;
    private const ulong RestartKind = 2UL << KindShift;
    private readonly List<ulong[]> segments = [];
    private readonly long maximumBytes;
    private readonly int segmentLength;
    private ulong[]? current;
    private int currentCount;
    private long rentedBytes;

    internal EntropyTokens(long maximumBytes = 64L * 1024 * 1024)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumBytes);
        this.maximumBytes = Math.Min(maximumBytes, 64L * 1024 * 1024);
        uint available = (uint)Math.Min(DefaultSegmentLength, this.maximumBytes / sizeof(ulong));
        // Shared-pool buckets start at sixteen elements. Smaller budgets simply
        // abandon on the first add; no allocation is needed for empty scans.
        segmentLength = available < 16 ? 0 : 1 << BitOperations.Log2(available);
    }

    internal bool IsComplete { get; private set; } = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHuffman(int component, bool isAc, int symbol, uint suffix, int count)
    {
        Debug.Assert((uint)component < 3 && (uint)symbol < 256 && (uint)count <= 16);
        Debug.Assert(count == 32 || (suffix >> count) == 0);
        int context = (component << 1) | (isAc ? 1 : 0);
        Add(suffix | ((ulong)(uint)count << CountShift) | ((ulong)(uint)symbol << SymbolShift)
            | ((ulong)(uint)context << ContextShift));
    }

    internal void AddBits(uint bits, int count)
    {
        Debug.Assert((uint)count <= 32 && (count == 32 || (bits >> count) == 0));
        Add(RawBitsKind | bits | ((ulong)(uint)count << CountShift));
    }

    internal void AddRestart(int index)
    {
        Debug.Assert((uint)index < 8);
        Add(RestartKind | (uint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(ulong token)
    {
        if (!IsComplete)
            return;
        ulong[]? segment = current;
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
    private void AddSegment(ulong token)
    {
        if (segmentLength == 0 || rentedBytes + (long)segmentLength * sizeof(ulong) > maximumBytes)
        {
            Dispose();
            return;
        }
        ulong[] next = ArrayPool<ulong>.Shared.Rent(segmentLength);
        if (rentedBytes + (long)next.Length * sizeof(ulong) > maximumBytes)
        {
            ArrayPool<ulong>.Shared.Return(next);
            Dispose();
            return;
        }
        segments.Add(next);
        rentedBytes += (long)next.Length * sizeof(ulong);
        current = next;
        next[0] = token;
        currentCount = 1;
    }

    internal void Replay(BitWriter writer, HuffmanCodingPlan.ScanCoding coding, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(coding);
        if (!IsComplete)
            throw new InvalidOperationException("An abandoned entropy token sequence cannot be replayed.");
        cancellationToken.ThrowIfCancellationRequested();
        var lookups = new ulong[Math.Max(coding.DcTables.Length, coding.AcTables.Length) * 2][];
        for (int component = 0; component < coding.DcTables.Length; component++)
            lookups[component * 2] = coding.DcTables[component].GetPrebiasedCodes();
        for (int component = 0; component < coding.AcTables.Length; component++)
            lookups[component * 2 + 1] = coding.AcTables[component].GetPrebiasedCodes();
        for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
        {
            ulong[] segment = segments[segmentIndex];
            int length = segmentIndex == segments.Count - 1 ? currentCount : segment.Length;
            for (int index = 0; index < length; index++)
            {
                if ((index & 255) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                ulong token = segment[index];
                int kind = (int)(token >> KindShift);
                int count = (int)(token >> CountShift) & 63;
                if (kind == 0)
                {
                    int context = (int)(token >> ContextShift) & 7;
                    int symbol = (int)(token >> SymbolShift) & 255;
                    ulong prepared = lookups[context][symbol];
                    if ((int)(prepared >> 38) == count + 1)
                        writer.WriteBits((uint)prepared | (uint)token, (int)(prepared >> 32) & 63);
                    else
                    {
                        HuffmanTable table = (context & 1) == 0 ? coding.DcTables[context >> 1] : coding.AcTables[context >> 1];
                        table.WriteSymbolWithBits(writer, symbol, (uint)token, count);
                    }
                }
                else if (kind == 1)
                    writer.WriteBits((uint)token, count);
                else
                    writer.WriteRestartMarker((int)token & 7);
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
        foreach (ulong[] segment in segments)
            ArrayPool<ulong>.Shared.Return(segment);
        segments.Clear();
    }
}
