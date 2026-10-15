using System.Buffers;

namespace DotJpegli.Internal;

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
    public override bool CanWrite => !disposed;
    public override long Length
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return count;
        }
    }
    public override long Position { get => Length; set => throw new NotSupportedException(); }

    internal byte[] ToArray()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return buffer.AsSpan(0, count).ToArray();
    }

    public override void Write(ReadOnlySpan<byte> source)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        EnsureCapacity(source.Length);
        source.CopyTo(buffer.AsSpan(count));
        count += source.Length;
    }

    public override void Write(byte[] source, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(source);
        Write(source.AsSpan(offset, length));
    }

    public override void WriteByte(byte value)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        EnsureCapacity(1);
        buffer[count++] = value;
    }

    private void EnsureCapacity(int additional)
    {
        if (additional <= buffer.Length - count)
            return;
        if (additional > Array.MaxLength - count)
            throw new IOException("The encoded JPEG exceeds the maximum array length.");
        int capacity = (int)Math.Max(count + additional,
            Math.Min(Array.MaxLength, Math.Max(4096L, (long)buffer.Length * 2)));
        bool nextPooled = capacity <= MaximumPooledCapacity;
        byte[] next = nextPooled ? ArrayPool<byte>.Shared.Rent(capacity) : new byte[capacity];
        buffer.AsSpan(0, count).CopyTo(next);
        byte[] previous = buffer;
        bool previousPooled = pooled;
        buffer = next;
        pooled = nextPooled;
        if (previousPooled)
            ArrayPool<byte>.Shared.Return(previous);
    }

    public override void Flush() => ObjectDisposedException.ThrowIf(disposed, this);
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            disposed = true;
            if (pooled)
                ArrayPool<byte>.Shared.Return(buffer);
            buffer = [];
            count = 0;
            pooled = false;
        }
        base.Dispose(disposing);
    }
}
