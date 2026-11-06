namespace DotJpegli.Internal;

using System.Buffers;
using System.Buffers.Binary;

internal sealed partial class EntropyTokens
{
    internal const int UnalignedReplayChunkTokens = 8192;

    private readonly record struct UnalignedRange(TokenPosition Start, TokenPosition End, int Count);

    // Ordered token collection has already resolved preceding DC predictors,
    // progressive EOB runs and correction-bit order. Workers only replace the
    // resulting codes and suffixes with unstuffed bits. Their chunks do not
    // acquire padding, stuffing or independent byte alignment.
    private bool TryReplayUnaligned(Stream output, HuffmanCodingPlan.ScanCoding coding, int workerLimit,
        ArrayPool<byte>? pool, KernelPreference stuffingKernel, CancellationToken cancellationToken)
    {
        long tokenCount = this.currentCount;
        for (int index = 0; index < this.segments.Count - 1; index++)
        {
            tokenCount += this.segments[index].Length;
        }
        if (tokenCount < 2 * UnalignedReplayChunkTokens)
        {
            return false;
        }
        if (!output.CanWrite)
        {
            throw new ArgumentException("The output stream must be writable.", nameof(output));
        }

        pool ??= ArrayPool<byte>.Shared;
        int workers = Math.Min(workerLimit, ParallelReplayMaximumWorkers);
        ulong[] lookups = this.PrepareParallelLookups(coding, out int lookupOffset);
        var batch = new UnalignedWork?[workers];
        var cursor = new TokenPosition(0, 0);
        using var writer = new BitWriter(output, buffered: true, stuffingKernel);
        while (cursor.Segment < this.segments.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int count = 0;
            int rentedBytes = 0;
            try
            {
                while (count < batch.Length && cursor.Segment < this.segments.Count)
                {
                    UnalignedRange range = this.ReadUnalignedRange(ref cursor);
                    var work = new UnalignedWork(range);
                    batch[count++] = work;
                    // Every canonical token contributes at most 32 bits. The
                    // final partial word also fits this four-byte/token bound.
                    int storageBound = range.Count * sizeof(uint);
                    if (storageBound <= ParallelReplayMaximumBytes - rentedBytes)
                    {
                        byte[] rental = pool.Rent(storageBound);
                        if (rental.Length <= ParallelReplayMaximumBytes - rentedBytes)
                        {
                            work.Buffer = rental;
                            rentedBytes += rental.Length;
                        }
                        else
                        {
                            pool.Return(rental);
                        }
                    }
                }

                _ = Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = workers }, index =>
                {
                    UnalignedWork work = batch[index]!;
                    if (work.Buffer is null)
                    {
                        return;
                    }
                    try
                    {
                        this.PrepareUnalignedRange(work, lookups, lookupOffset, cancellationToken);
                    }
                    catch (Exception)
                    {
                        // Replay the original tokens on the caller if staging
                        // fails. That strict path preserves the serial error,
                        // exception identity and already emitted prefix without
                        // Parallel.For wrapping it in AggregateException.
                        work.Prepared = false;
                    }
                });

                for (int index = 0; index < count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    UnalignedWork work = batch[index]!;
                    if (work.Prepared)
                    {
                        // One writer owns every chunk. Its existing 64-bit
                        // discharge consequently sees exactly the serial words,
                        // including FF stuffing and destination drain lengths.
                        for (int offset = 0; offset < work.WordBytes; offset += sizeof(uint))
                        {
                            if ((offset & 1023) == 0)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            writer.WriteBitsPrevalidated(BinaryPrimitives.ReadUInt32LittleEndian(
                                work.Buffer!.AsSpan(offset, sizeof(uint))), 32);
                        }
                        if (work.TailBits != 0)
                        {
                            writer.WriteBitsPrevalidated(work.Tail, work.TailBits);
                        }
                    }
                    else
                    {
                        // Missing codes, noncanonical suffix widths and actual
                        // rentals exceeding the shared cap retain strict replay.
                        this.ReplayRangeWithLookups(writer, coding, lookups, lookupOffset,
                            work.Range.Start, work.Range.End, cancellationToken);
                    }
                }
            }
            finally
            {
                for (int index = 0; index < count; index++)
                {
                    if (batch[index]!.Buffer is { } buffer)
                    {
                        pool.Return(buffer);
                    }
                    batch[index] = null;
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        writer.Flush();
        return true;
    }

    private UnalignedRange ReadUnalignedRange(ref TokenPosition cursor)
    {
        TokenPosition start = cursor;
        int remaining = UnalignedReplayChunkTokens;
        while (remaining != 0 && cursor.Segment < this.segments.Count)
        {
            int length = cursor.Segment == this.segments.Count - 1
                ? this.currentCount : this.segments[cursor.Segment].Length;
            int take = Math.Min(remaining, length - cursor.Offset);
            remaining -= take;
            cursor = cursor.Offset + take == length
                ? new(cursor.Segment + 1, 0) : new(cursor.Segment, cursor.Offset + take);
        }
        return new(start, cursor, UnalignedReplayChunkTokens - remaining);
    }

    private void PrepareUnalignedRange(UnalignedWork work, ReadOnlySpan<ulong> lookups, int lookupOffset,
        CancellationToken cancellationToken)
    {
        ulong pending = 0;
        int pendingBits = 0;
        int wordBytes = 0;
        for (int segmentIndex = work.Range.Start.Segment;
            segmentIndex < this.segments.Count && segmentIndex <= work.Range.End.Segment; segmentIndex++)
        {
            uint[] segment = this.segments[segmentIndex];
            int length = segmentIndex == work.Range.End.Segment ? work.Range.End.Offset
                : segmentIndex == this.segments.Count - 1 ? this.currentCount : segment.Length;
            int first = segmentIndex == work.Range.Start.Segment ? work.Range.Start.Offset : 0;
            for (int index = first; index < length; index++)
            {
                if ((index & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                uint token = segment[index];
                int contextAndSymbol = (int)(token >> SymbolShift) & 2047;
                int lookupIndex = contextAndSymbol - lookupOffset;
                uint bits;
                int bitCount;
                if ((uint)lookupIndex < (uint)lookups.Length)
                {
                    ulong prepared = lookups[lookupIndex];
                    if ((int)(prepared >> 38) != (int)(token >> CountShift) + 1)
                    {
                        return;
                    }
                    bits = (uint)prepared | (token & 0xffffU);
                    bitCount = (int)(prepared >> 32) & 63;
                }
                else if (contextAndSymbol is >= (6 * 256) and < (7 * 256))
                {
                    bits = token & 0xffffffU;
                    bitCount = (int)(token >> CountShift);
                }
                else
                {
                    return;
                }

                pending = (pending << bitCount) | bits;
                pendingBits += bitCount;
                if (pendingBits >= 32)
                {
                    pendingBits -= 32;
                    BinaryPrimitives.WriteUInt32LittleEndian(work.Buffer.AsSpan(wordBytes, sizeof(uint)),
                        (uint)(pending >> pendingBits));
                    wordBytes += sizeof(uint);
                    pending &= (1UL << pendingBits) - 1;
                }
            }
        }
        work.WordBytes = wordBytes;
        work.Tail = (uint)pending;
        work.TailBits = pendingBits;
        work.Prepared = true;
    }

    private sealed class UnalignedWork(UnalignedRange range)
    {
        internal UnalignedRange Range { get; } = range;
        internal byte[]? Buffer { get; set; }
        internal bool Prepared { get; set; }
        internal int WordBytes { get; set; }
        internal uint Tail { get; set; }
        internal int TailBits { get; set; }
    }
}
