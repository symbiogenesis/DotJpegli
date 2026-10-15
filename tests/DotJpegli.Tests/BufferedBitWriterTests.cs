using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class BufferedBitWriterTests
{
    [Fact]
    public void DefaultWriterStillEmitsCompleteBytesEagerly()
    {
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        writer.WriteBits(0xff, 8);
        Assert.Equal(new byte[] { 0xff, 0 }, output.ToArray());
        writer.WriteBits(0, 1);
        Assert.Equal(2, output.Length);
    }

    [Fact]
    public void AlignedFlushDrainsBufferedBytesWithoutFlushingOrClosingTheStream()
    {
        using var output = new ObservedStream();
        var writer = new BitWriter(output, buffered: true);
        writer.WriteBits(0x42ff, 16);
        Assert.Equal(0, output.Length);
        writer.Flush();
        Assert.Equal(new byte[] { 0x42, 0xff, 0 }, output.ToArray());
        Assert.Equal(1, output.SpanWrites);
        Assert.Equal(0, output.ByteWrites);
        Assert.Equal(0, output.Flushes);
        Assert.True(output.CanWrite);
        writer.Flush();
        Assert.Equal(1, output.SpanWrites);
    }

    [Fact]
    public void RandomWritesAndRestartMarkersMatchTheEagerWriterAcrossBufferBoundaries()
    {
        using var expected = new MemoryStream();
        using var actual = new ObservedStream();
        var eager = new BitWriter(expected);
        var buffered = new BitWriter(actual, buffered: true);
        var random = new Random(713991);
        for (int index = 0; index < 10000; index++)
        {
            int count = random.Next(33);
            uint bits = (uint)random.NextInt64(0, 1L << 32);
            if (count != 32)
                bits &= (1U << count) - 1;
            eager.WriteBits(bits, count);
            buffered.WriteBits(bits, count);
            if (index % 157 == 156)
            {
                eager.WriteRestartMarker(index % 8);
                buffered.WriteRestartMarker(index % 8);
                Assert.Equal(expected.ToArray(), actual.ToArray());
            }
        }
        eager.Flush();
        buffered.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Equal(0, actual.Flushes);
    }

    [Fact]
    public void StuffedBytesAtBufferBoundaryAreCorrectAndWritesStayBounded()
    {
        using var expected = new MemoryStream();
        using var actual = new ObservedStream();
        var eager = new BitWriter(expected);
        var buffered = new BitWriter(actual, buffered: true);
        for (int index = 0; index < 12301; index++)
        {
            uint value = index % 4096 >= 4093 ? 0xffU : 0x42U;
            eager.WriteBits(value, 8);
            buffered.WriteBits(value, 8);
        }
        Assert.InRange(actual.Length, 1, expected.Length);
        eager.Flush();
        buffered.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Equal(0, actual.ByteWrites);
        Assert.InRange(actual.SpanWrites, 4, 5);
        Assert.InRange(actual.MaximumWrite, 4095, 4096);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void InvalidWritesPreservePendingBytesAndBits(int restartIndex)
    {
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        writer.WriteBits(0x42, 8);
        writer.WriteBits(0, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBits(3, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBits(0, 33));
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteRestartMarker(restartIndex));
        Assert.Equal(0, output.Length);
        writer.Flush();
        Assert.Equal(new byte[] { 0x42, 0x7f }, output.ToArray());
    }

    [Fact]
    public void RestartMarkerDrainsPaddedEntropyBeforeItsUnstuffedBytes()
    {
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        writer.WriteBits(0x42ff, 16);
        writer.WriteBits(1, 1);
        writer.WriteRestartMarker(7);
        Assert.Equal(new byte[] { 0x42, 0xff, 0, 0xff, 0, 0xff, 0xd7 }, output.ToArray());
        writer.WriteBits(0, 1);
        writer.Flush();
        Assert.Equal(0x7f, output.ToArray()[^1]);
    }

    [Fact]
    public void FullWordsAndEverySpillAlignmentMatchTheEagerBitStream()
    {
        for (int prefixBits = 0; prefixBits < 64; prefixBits++)
        {
            using var expected = new MemoryStream();
            using var actual = new MemoryStream();
            var eager = new BitWriter(expected);
            var buffered = new BitWriter(actual, buffered: true);
            for (int index = 0; index < prefixBits; index++)
            {
                eager.WriteBits((uint)(index & 1), 1);
                buffered.WriteBits((uint)(index & 1), 1);
            }
            foreach (uint word in new uint[] { 0, 0xffffffff, 0x12fffe00, 0xfeff0102, 0x89abcdef })
            {
                eager.WriteBits(word, 32);
                buffered.WriteBits(word, 32);
            }
            eager.WriteBits(0, 0);
            buffered.WriteBits(0, 0);
            eager.Flush();
            buffered.Flush();
            Assert.Equal(expected.ToArray(), actual.ToArray());
        }
    }

    [Fact]
    public void CompleteWordStuffingChecksEveryByteAndAlignedFlushEmitsNoPaddingByte()
    {
        for (int stuffedByte = -1; stuffedByte < 8; stuffedByte++)
        {
            byte[] bytes = [0xfe, 0, 0x7f, 0x80, 1, 0xfe, 0, 0x42];
            if (stuffedByte >= 0)
                bytes[stuffedByte] = 0xff;
            using var actual = new MemoryStream();
            var writer = new BitWriter(actual, buffered: true);
            foreach (byte value in bytes)
                writer.WriteBits(value, 8);
            writer.Flush();
            Assert.Equal(bytes.SelectMany(value => value == 0xff ? new byte[] { 0xff, 0 } : [value]).ToArray(), actual.ToArray());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestinationFailuresAreUnwrappedAndLeaveTheStreamOpen(bool atCapacity)
    {
        using var output = new ObservedStream { Failure = new IOException("Injected entropy write failure.") };
        var writer = new BitWriter(output, buffered: true);
        IOException error = Assert.Throws<IOException>(() =>
        {
            if (atCapacity)
                for (int index = 0; index < 4096; index++)
                    writer.WriteBits(0x42, 8);
            else
            {
                writer.WriteBits(0xff, 8);
                writer.Flush();
            }
        });
        Assert.Same(output.Failure, error);
        Assert.True(output.CanWrite);
    }

    private sealed class ObservedStream : Stream
    {
        private readonly MemoryStream destination = new();
        internal int SpanWrites { get; private set; }
        internal int ByteWrites { get; private set; }
        internal int Flushes { get; private set; }
        internal int MaximumWrite { get; private set; }
        internal IOException? Failure { get; init; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => destination.CanWrite;
        public override long Length => destination.Length;
        public override long Position { get => destination.Position; set => throw new NotSupportedException(); }
        public override void Flush() => Flushes++;
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (Failure is not null)
                throw Failure;
            SpanWrites++;
            MaximumWrite = Math.Max(MaximumWrite, buffer.Length);
            destination.Write(buffer);
        }
        public override void WriteByte(byte value)
        {
            ByteWrites++;
            destination.WriteByte(value);
        }
        internal byte[] ToArray() => destination.ToArray();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                destination.Dispose();
            base.Dispose(disposing);
        }
    }
}
