namespace DotJpegli.Internal;

using System.Buffers;
using System.Buffers.Binary;
#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.ExceptionServices;

internal sealed partial class EntropyTokens
{
    internal const int ParallelReplayMaximumBytes = 4 * 1024 * 1024;
    internal const int ParallelReplayMaximumWorkers = 32;

    private readonly record struct TokenPosition(int Segment, int Offset);
    private readonly record struct RestartRange(TokenPosition Start, TokenPosition End, int FollowingRestart, long Count);

    // Called only at an empty scan boundary. Restart records already separate
    // the DC predictors, progressive EOB runs and correction bits in this cache.
    // The caller retains its original serial writer when no restart is present.
    internal bool TryReplayParallel(Stream output, HuffmanCodingPlan.ScanCoding coding, int workerLimit,
        CancellationToken cancellationToken, ArrayPool<byte>? pool = null)
    {
        Guard.ThrowIfNull(output);
        Guard.ThrowIfNull(coding);
        if (!this.IsComplete)
        {
            throw new InvalidOperationException("An abandoned entropy token sequence cannot be replayed.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (workerLimit < 2 || !this.hasRestart)
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
        var batch = new ReplayWork?[workers];
        var cursor = new TokenPosition(0, 0);
        while (cursor.Segment < this.segments.Count)
        {
            int count = 0;
            int rentedBytes = 0;
            try
            {
                // The batch contains at most one range per worker. Metadata,
                // writer buffers and retained output all have fixed bounds.
                while (count < batch.Length && cursor.Segment < this.segments.Count)
                {
                    RestartRange range = this.ReadRestartRange(ref cursor, cancellationToken);
                    var work = new ReplayWork(range);
                    batch[count++] = work;
                    long payloadBound = (range.Count * 8) + 2;
                    // A word has at most 32 code/suffix bits and can double in
                    // size for FF stuffing. Reserve padding plus four bytes per
                    // recorded drain; all nonfinal drains exceed 4080 bytes.
                    long storageBound = payloadBound + (4 * ((payloadBound / 4080) + 1));
                    if (storageBound <= ParallelReplayMaximumBytes - rentedBytes)
                    {
                        byte[] rental = pool.Rent((int)storageBound);
                        if (rental.Length <= ParallelReplayMaximumBytes - rentedBytes)
                        {
                            try
                            {
                                work.Output = new RecordedOutput(rental);
                            }
                            catch
                            {
                                pool.Return(rental);
                                throw;
                            }
                            rentedBytes += rental.Length;
                        }
                        else
                        {
                            pool.Return(rental);
                        }
                    }
                }

                // Every body catches its own error, so Parallel.For cannot wrap
                // coding failures in AggregateException or select a later range.
                _ = Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = workers }, index =>
                {
                    ReplayWork work = batch[index]!;
                    if (work.Output is null)
                    {
                        return;
                    }
                    try
                    {
                        var writer = new BitWriter(work.Output, buffered: true);
                        this.ReplayRangeWithLookups(writer, coding, lookups, lookupOffset,
                            work.Range.Start, work.Range.End, cancellationToken);
                        writer.Flush();
                    }
                    catch (Exception error)
                    {
                        work.Error = ExceptionDispatchInfo.Capture(error);
                    }
                });

                for (int index = 0; index < count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReplayWork work = batch[index]!;
                    if (work.Output is { } recorded)
                    {
                        recorded.CopyToDestination(output, cancellationToken);
                        work.Error?.Throw();
                    }
                    else
                    {
                        // A large segment or unexpectedly oversized pool bucket
                        // keeps the original bounded writer on the destination.
                        var writer = new BitWriter(output, buffered: true);
                        this.ReplayRangeWithLookups(writer, coding, lookups, lookupOffset,
                            work.Range.Start, work.Range.End, cancellationToken);
                        writer.Flush();
                    }
                    if (work.Range.FollowingRestart >= 0)
                    {
                        output.WriteByte(0xff);
                        output.WriteByte((byte)(0xd0 + work.Range.FollowingRestart));
                    }
                }
            }
            finally
            {
                for (int index = 0; index < count; index++)
                {
                    if (batch[index]!.Output is { } recorded)
                    {
                        pool.Return(recorded.Buffer);
                    }
                    batch[index] = null;
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return true;
    }

    private ulong[] PrepareParallelLookups(HuffmanCodingPlan.ScanCoding coding, out int lookupOffset)
    {
        lookupOffset = 0;
        if (this.huffmanContextMask == 0)
        {
            return [];
        }
        if ((this.huffmanContextMask & (this.huffmanContextMask - 1)) == 0)
        {
            int context = BitOperations.Log2((uint)this.huffmanContextMask);
            lookupOffset = context * 256;
            HuffmanTable[] tables = (context & 1) == 0 ? coding.DcTables : coding.AcTables;
            return context / 2 < tables.Length ? tables[context / 2].GetPrebiasedCodes() : [];
        }

        ulong[] lookups = new ulong[(BitOperations.Log2((uint)this.huffmanContextMask) + 1) * 256];
        for (int component = 0; component < coding.DcTables.Length; component++)
        {
            if ((this.huffmanContextMask & (1 << (component * 2))) != 0)
            {
                coding.DcTables[component].GetPrebiasedCodes().CopyTo(lookups.AsSpan(component * 512, 256));
            }
        }
        for (int component = 0; component < coding.AcTables.Length; component++)
        {
            if ((this.huffmanContextMask & (2 << (component * 2))) != 0)
            {
                coding.AcTables[component].GetPrebiasedCodes().CopyTo(lookups.AsSpan((component * 512) + 256, 256));
            }
        }
        return lookups;
    }

    private RestartRange ReadRestartRange(ref TokenPosition cursor, CancellationToken cancellationToken)
    {
        TokenPosition start = cursor;
        long count = 0;
        while (cursor.Segment < this.segments.Count)
        {
            uint[] segment = this.segments[cursor.Segment];
            int length = cursor.Segment == this.segments.Count - 1 ? this.currentCount : segment.Length;
            for (int offset = cursor.Offset; offset < length; offset++)
            {
                if ((offset & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                uint token = segment[offset];
                if (((token >> ContextShift) & 7) == 7)
                {
                    var end = new TokenPosition(cursor.Segment, offset);
                    cursor = offset + 1 == length ? new(cursor.Segment + 1, 0) : new(cursor.Segment, offset + 1);
                    return new(start, end, (int)token & 7, count);
                }
                count++;
            }
            cursor = new(cursor.Segment + 1, 0);
        }
        return new(start, cursor, -1, count);
    }

    private sealed class ReplayWork(RestartRange range)
    {
        internal RestartRange Range { get; } = range;
        internal RecordedOutput? Output { get; set; }
        internal ExceptionDispatchInfo? Error { get; set; }
    }

    // Keep each original Stream.Write call, including its exact size. Merely
    // concatenating bytes into larger writes changes destination-failure prefixes.
    // Drain lengths live inline in the same bounded byte rental as the payload.
    private sealed class RecordedOutput(byte[] buffer) : Stream
    {
        private int count;

        internal byte[] Buffer { get; } = buffer;
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            BinaryPrimitives.WriteInt32LittleEndian(this.Buffer.AsSpan(this.count, 4), buffer.Length);
            buffer.CopyTo(this.Buffer.AsSpan(this.count + 4));
            this.count += buffer.Length + 4;
        }

        internal void CopyToDestination(Stream output, CancellationToken cancellationToken)
        {
            for (int offset = 0; offset < this.count;)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int length = BinaryPrimitives.ReadInt32LittleEndian(this.Buffer.AsSpan(offset, 4));
                output.Write(this.Buffer.AsSpan(offset + 4, length));
                offset += length + 4;
            }
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
