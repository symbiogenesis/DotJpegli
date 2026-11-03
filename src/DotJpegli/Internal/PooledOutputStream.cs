namespace DotJpegli.Internal;

using System.Buffers;

/// <summary>Reusable staging storage for the byte-array encode API's owned result.</summary>
internal sealed class PooledOutputStream : Stream
{
    // Avoid retaining unusually large JPEG buffers in the shared pool. Larger
    // encodes continue with ordinary arrays, like MemoryStream's growth path.
    private const int MaximumPooledCapacity = 1024 * 1024;
    private byte[] buffer = [];
    private int count;
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
        return this.buffer.AsSpan(0, this.count).ToArray();
    }

    public override void Write(ReadOnlySpan<byte> source)
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        this.EnsureCapacity(source.Length);
        source.CopyTo(this.buffer.AsSpan(this.count));
        this.count += source.Length;
    }

    public override void Write(byte[] source, int offset, int length)
    {
        Guard.ThrowIfNull(source);
        this.Write(source.AsSpan(offset, length));
    }

    public override void WriteByte(byte value)
    {
        Guard.ThrowIfDisposed(this.disposed, this);
        this.EnsureCapacity(1);
        this.buffer[this.count++] = value;
    }

    private void EnsureCapacity(int additional)
    {
        if (additional <= this.buffer.Length - this.count)
        {
            return;
        }

        if (additional > RuntimeLimits.ArrayMaxLength - this.count)
        {
            throw new IOException("The encoded JPEG exceeds the maximum array length.");
        }

        int capacity = (int)Math.Max(this.count + additional,
            Math.Min(RuntimeLimits.ArrayMaxLength, Math.Max(4096L, (long)this.buffer.Length * 2)));
        bool nextPooled = capacity <= MaximumPooledCapacity;
        byte[] next = nextPooled ? ArrayPool<byte>.Shared.Rent(capacity) : new byte[capacity];
        this.buffer.AsSpan(0, this.count).CopyTo(next);
        byte[] previous = this.buffer;
        bool previousPooled = this.pooled;
        this.buffer = next;
        this.pooled = nextPooled;
        if (previousPooled)
        {
            ArrayPool<byte>.Shared.Return(previous);
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

            this.buffer = [];
            this.count = 0;
            this.pooled = false;
        }
        base.Dispose(disposing);
    }
}
