namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

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
            {
                bits &= (1U << count) - 1;
            }

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
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBits(3, 1));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBits(0, 33));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteRestartMarker(restartIndex));
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
            {
                bytes[stuffedByte] = 0xff;
            }

            using var actual = new MemoryStream();
            var writer = new BitWriter(actual, buffered: true);
            foreach (byte value in bytes)
            {
                writer.WriteBits(value, 8);
            }

            writer.Flush();
            Assert.Equal(bytes.SelectMany(value => value == 0xff ? new byte[] { 0xff, 0 } : [value]).ToArray(), actual.ToArray());
        }
    }

    [Fact]
    public void EveryStuffingMaskPreservesBytesAndDrainBoundariesWithOneThroughSeventeenBytesFree()
    {
        for (int free = 1; free <= 17; free++)
        {
            byte[] prefix = CreatePrefix(4096 - free);
            byte[] stuffedPrefix = StuffBytes(prefix);
            for (int mask = 0; mask < 256; mask++)
            {
                byte[] word = CreateWord(mask);
                byte[] expected = [.. stuffedPrefix, .. StuffBytes(word)];
                using var actual = new ObservedStream();
                var writer = new BitWriter(actual, buffered: true);
                WriteBytes(writer, prefix);
                Assert.Equal(0, actual.Length);
                WriteBytes(writer, word);
                Assert.Equal(FirstWordDrainLength(stuffedPrefix.Length, word), actual.Length);
                writer.Flush();
                Assert.Equal(expected, actual.ToArray());
                Assert.Equal(0, actual.ByteWrites);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void StuffedWordFailuresPreserveTheSameWriteAndAcceptedPrefix(int acceptedBytes)
    {
        for (int free = 1; free <= 17; free++)
        {
            byte[] prefix = CreatePrefix(4096 - free);
            byte[] stuffedPrefix = StuffBytes(prefix);
            foreach (int mask in new int[] { 0, 1, 0x80, 0x55, 0xfe, 0xff })
            {
                byte[] word = CreateWord(mask);
                byte[] expected = [.. stuffedPrefix, .. StuffBytes(word)];
                int drainLength = FirstWordDrainLength(stuffedPrefix.Length, word);
                int failedLength = drainLength == 0 ? expected.Length : drainLength;
                using var output = new ObservedStream
                {
                    Failure = new IOException("Injected partial entropy write failure."),
                    BytesAcceptedOnFailure = acceptedBytes,
                };
                var writer = new BitWriter(output, buffered: true);
                WriteBytes(writer, prefix);
                bool wordCompleted = false;
                IOException error = Assert.Throws<IOException>(() =>
                {
                    WriteBytes(writer, word);
                    wordCompleted = true;
                    writer.Flush();
                });
                Assert.Same(output.Failure, error);
                Assert.Equal(drainLength == 0, wordCompleted);
                Assert.Equal(expected.AsSpan(0, failedLength).ToArray(), output.FailedWrite);
                Assert.Equal(expected.AsSpan(0, Math.Min(acceptedBytes, failedLength)).ToArray(), output.ToArray());
                Assert.True(output.CanWrite);
            }
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
            {
                for (int index = 0; index < 4096; index++)
                {
                    writer.WriteBits(0x42, 8);
                }
            }
            else
            {
                writer.WriteBits(0xff, 8);
                writer.Flush();
            }
        });
        Assert.Same(output.Failure, error);
        Assert.True(output.CanWrite);
    }

    private static byte[] CreatePrefix(int stuffedLength)
    {
        // Complete words keep the bit accumulator empty. Stuffing in the first
        // word selects every final output-buffer offset without a flush.
        int extraBytes = stuffedLength & 7;
        byte[] prefix = new byte[stuffedLength - extraBytes];
        prefix.AsSpan().Fill(0x42);
        prefix.AsSpan(0, extraBytes).Fill(0xff);
        return prefix;
    }

    private static byte[] CreateWord(int mask)
    {
        byte[] word = [0xfe, 0, 0x7f, 0x80, 1, 0xfe, 0, 0x42];
        for (int index = 0; index < word.Length; index++)
        {
            if ((mask & (1 << index)) != 0)
            {
                word[index] = 0xff;
            }
        }

        return word;
    }

    private static byte[] StuffBytes(byte[] bytes)
        => bytes.SelectMany(value => value == 0xff ? new byte[] { 0xff, 0 } : [value]).ToArray();

    private static void WriteBytes(BitWriter writer, byte[] bytes)
    {
        foreach (byte value in bytes)
        {
            writer.WriteBits(value, 8);
        }
    }

    private static int FirstWordDrainLength(int prefixLength, byte[] word)
    {
        if (!word.Contains((byte)0xff))
        {
            // The established whole-word path reserves eight bytes at once.
            if (prefixLength > 4096 - 8)
            {
                return prefixLength;
            }

            return prefixLength + 8 == 4096 ? 4096 : 0;
        }

        // The established stuffing boundary reserves a possible byte pair
        // before each byte, and drains immediately when the buffer is full.
        int count = prefixLength;
        foreach (byte value in word)
        {
            if (count > 4096 - 2)
            {
                return count;
            }

            count += value == 0xff ? 2 : 1;
            if (count == 4096)
            {
                return count;
            }
        }

        return 0;
    }

    private sealed class ObservedStream : Stream
    {
        private readonly MemoryStream destination = new();
        internal int SpanWrites { get; private set; }
        internal int ByteWrites { get; private set; }
        internal int Flushes { get; private set; }
        internal int MaximumWrite { get; private set; }
        internal IOException? Failure { get; init; }
        internal int BytesAcceptedOnFailure { get; init; }
        internal byte[]? FailedWrite { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => this.destination.CanWrite;
        public override long Length => this.destination.Length;
        public override long Position { get => this.destination.Position; set => throw new NotSupportedException(); }
        public override void Flush() => this.Flushes++;
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (this.Failure is not null)
            {
                this.FailedWrite = buffer.ToArray();
                this.destination.Write(buffer[..Math.Min(this.BytesAcceptedOnFailure, buffer.Length)]);
                throw this.Failure;
            }

            this.SpanWrites++;
            this.MaximumWrite = Math.Max(this.MaximumWrite, buffer.Length);
            this.destination.Write(buffer);
        }
        public override void WriteByte(byte value)
        {
            this.ByteWrites++;
            this.destination.WriteByte(value);
        }
        internal byte[] ToArray() => this.destination.ToArray();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.destination.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
