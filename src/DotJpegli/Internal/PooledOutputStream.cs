namespace DotJpegli.Internal;

using System.Buffers;

/// <summary>Reusable staging storage for the byte-array encode API's owned result.</summary>
internal sealed class PooledOutputStream(int initialCapacity = 4096) : Stream
{
    // Pool the initial buffer. Larger JPEGs append bounded ordinary segments,
    // avoiding repeated copies of all preceding output or giant pool rentals.
    private const int MaximumPooledCapacity = 1024 * 1024;
    private readonly int initialCapacity = Math.Clamp(initialCapacity, 4096, MaximumPooledCapacity);
    private byte[] buffer = [];
    private int count;
    private int bufferCount;
    private List<byte[]>? completed;
    private bool pooled;
    private bool disposed;

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => !this.disposed;
    public override long Length
    {
        get
        {
            Guard.ThrowIfDisposed(this.disposed, this);
            return this.count;
        }
    }
    public override long Position { get => this.Length; set => throw new NotSupportedException(); }

    internal byte[] ToArray()
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        if (this.completed is null)
        {
            return this.buffer.AsSpan(0, this.count).ToArray();
        }

        // Every byte is copied below before this owned result can escape.
#if NETSTANDARD2_1
        byte[] result = new byte[this.count];
#else
        byte[] result = GC.AllocateUninitializedArray<byte>(this.count);
#endif
        int offset = 0;
        foreach (byte[] segment in this.completed)
        {
            segment.CopyTo(result, offset);
            offset += segment.Length;
        }
        this.buffer.AsSpan(0, this.bufferCount).CopyTo(result.AsSpan(offset));
        return result;
    }

    public override void Write(ReadOnlySpan<byte> source)
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        this.CheckLength(source.Length);
        while (!source.IsEmpty)
        {
            this.EnsureCapacity(source.Length);
            int length = Math.Min(source.Length, this.buffer.Length - this.bufferCount);
            source[..length].CopyTo(this.buffer.AsSpan(this.bufferCount));
            this.bufferCount += length;
            this.count += length;
            source = source[length..];
        }
    }

    public override void Write(byte[] source, int offset, int length)
    {
        Guard.ThrowIfNull(source);
        this.Write(source.AsSpan(offset, length));
    }

    public override void WriteByte(byte value)
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        this.CheckLength(1);
        this.EnsureCapacity(1);
        this.buffer[this.bufferCount++] = value;
        this.count++;
    }

    private void EnsureCapacity(int additional)
    {
        if (this.bufferCount < this.buffer.Length)
        {
            return;
        }

        if (this.buffer.Length == MaximumPooledCapacity)
        {
            byte[] nextSegment = new byte[MaximumPooledCapacity];
            this.completed ??= [];
            this.completed.Add(this.buffer);
            this.buffer = nextSegment;
            this.bufferCount = 0;
            this.pooled = false;
            return;
        }

        int capacity = (int)Math.Min(MaximumPooledCapacity,
            Math.Max((long)this.bufferCount + additional, Math.Max(this.initialCapacity, (long)this.buffer.Length * 2)));
        byte[] next = ArrayPool<byte>.Shared.Rent(capacity);
        this.buffer.AsSpan(0, this.bufferCount).CopyTo(next);
        byte[] previous = this.buffer;
        bool previousPooled = this.pooled;
        this.buffer = next;
        this.pooled = true;
        if (previousPooled)
        {
            ArrayPool<byte>.Shared.Return(previous);
        }
    }

    private void CheckLength(int additional)
    {
        if (additional > RuntimeLimits.ArrayMaxLength - this.count)
        {
            throw new IOException("The encoded JPEG exceeds the maximum array length.");
        }
    }

    public override void Flush() => Guard.ThrowIfDisposed(this.disposed, this);
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.disposed = true;
            if (this.pooled)
            {
                ArrayPool<byte>.Shared.Return(this.buffer);
            }

            // Only the first completed segment came from the pool. Subsequent
            // segments are ordinary bounded arrays and are released together.
            if (this.completed is { Count: > 0 } segments)
            {
                ArrayPool<byte>.Shared.Return(segments[0]);
                segments.Clear();
            }

            this.buffer = [];
            this.count = 0;
            this.bufferCount = 0;
            this.pooled = false;
        }
        base.Dispose(disposing);
    }
}
