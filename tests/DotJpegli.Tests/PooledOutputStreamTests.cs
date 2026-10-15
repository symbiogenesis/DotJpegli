using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class PooledOutputStreamTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(4097)]
    [InlineData(131073)]
    [InlineData(1048577)]
    public void MixedWritesAndGrowthPreserveEveryByteAndReturnIndependentCopies(int length)
    {
        byte[] input = new byte[length];
        new Random(1984).NextBytes(input);
        using var stream = new PooledOutputStream();
        int offset = 0;
        while (offset < input.Length)
        {
            stream.WriteByte(input[offset++]);
            int chunk = Math.Min(997, input.Length - offset);
            stream.Write(input, offset, chunk);
            offset += chunk;
            chunk = Math.Min(65537, input.Length - offset);
            stream.Write(input.AsSpan(offset, chunk));
            offset += chunk;
        }
        Assert.Equal(length, stream.Length);
        byte[] first = stream.ToArray();
        Assert.Equal(input, first);
        if (first.Length != 0)
            first[0] ^= 0xff;
        Assert.Equal(input, stream.ToArray());
        stream.Dispose();
        // Later renters and disposal must never change the returned JPEG.
        using var later = new PooledOutputStream();
        later.Write(new byte[length]);
        Assert.Equal(input.Skip(1), first.Skip(1));
    }

    [Fact]
    public void DisposalAndRejectedWritesLeaveNoPartialOutput()
    {
        var stream = new PooledOutputStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(new byte[3], 1, 3));
        Assert.Empty(stream.ToArray());
        stream.WriteByte(0xff);
        stream.Dispose();
        stream.Dispose();
        Assert.False(stream.CanWrite);
        Assert.Throws<ObjectDisposedException>(() => stream.WriteByte(1));
        Assert.Throws<ObjectDisposedException>(() => stream.ToArray());
    }
}
