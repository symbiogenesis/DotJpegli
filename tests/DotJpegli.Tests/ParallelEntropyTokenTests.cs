namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class ParallelEntropyTokenTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 5)]
    [InlineData(2, 2)]
    [InlineData(63, 5)]
    public void RestartSegmentsPreserveContextsRawBitsRentalBoundariesAndDestinationWrites(int mask, int workers)
    {
        var table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        var coding = new HuffmanCodingPlan.ScanCoding([table, table, table], [table, table, table], [], []);
        using var tokens = new EntropyTokens(huffmanContextMask: mask);
        for (int segment = 0; segment < 18; segment++)
        {
            // Long segments cross both BitWriter drains and the token cache's
            // first rental; shorter ones exercise independent bit alignments.
            int length = segment % 4 == 0 ? 8193 : 257 + segment;
            for (int index = 0; index < length; index++)
            {
                int context = mask == 63 ? index % 6 : mask == 0 ? -1
                    : System.Numerics.BitOperations.Log2((uint)mask);
                if (context < 0 || index % 7 == 6)
                {
                    tokens.AddBits(unchecked((uint)index * 0x9e3779b9U), 32);
                }
                else
                {
                    int symbol = index & 255;
                    int width = symbol == 0xf0 ? 0 : (symbol & 15) == 0 ? symbol >> 4 : symbol & 15;
                    if (index % 11 == 10)
                    {
                        width = (width + 3) % 17;
                    }
                    uint suffix = unchecked((uint)index * 0x9e3779b9U) & ((1U << width) - 1);
                    tokens.AddHuffman(context >> 1, (context & 1) != 0, symbol, suffix, width);
                }
            }
            if (segment < 17)
            {
                tokens.AddRestart(segment & 7);
            }
        }

        using var expected = new ObservedStream();
        ReplaySerial(tokens, coding, expected);
        var pool = new TrackingPool();
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, coding, workers, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.True(pool.Rents > 0);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Theory]
    [InlineData(false, 5)]
    [InlineData(true, 1)]
    public void IneligibleScansReturnWithoutTouchingTheDestinationOrPool(bool restarts, int workers)
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        tokens.AddBits(0xff, 8);
        if (restarts)
        {
            tokens.AddRestart(0);
            tokens.AddBits(0x42, 8);
        }
        using var actual = new ObservedStream();
        var pool = new TrackingPool();
        Assert.False(tokens.TryReplayParallel(actual, EmptyCoding(), workers, TestContext.Current.CancellationToken, pool));
        Assert.Empty(actual.Bytes);
        Assert.Empty(actual.Writes);
        Assert.Equal(0, pool.Rents);
    }

    [Fact]
    public void EmptySegmentsAndATrailingRestartPreserveTheSmallReplayMarkerSequence()
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int marker = 0; marker < 18; marker++)
        {
            tokens.AddRestart(marker & 7);
        }
        using var expected = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), expected);
        var pool = new TrackingPool();
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, EmptyCoding(), 5, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Equal(18, pool.Rents);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void FullyStuffedWordsAndPaddingKeepExactDrainsAroundConsecutiveAndTrailingRestarts()
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        tokens.AddRestart(0);
        tokens.AddRestart(1);
        for (int segment = 2; segment < 6; segment++)
        {
            for (int index = 0; index < 4097 + segment; index++)
            {
                tokens.AddBits(0xffffff, 24);
            }
            tokens.AddBits(7, 3);
            tokens.AddRestart(segment);
        }
        tokens.AddRestart(6);
        using var expected = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), expected);
        var pool = new TrackingPool();
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, EmptyCoding(), 5, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void PreCanceledReplayPreservesTheTokenAndWritesNothing()
    {
        using EntropyTokens tokens = RawSegments(3, 4097);
        using var output = new ObservedStream();
        var pool = new TrackingPool();
        var canceled = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            tokens.TryReplayParallel(output, EmptyCoding(), 3, canceled, pool));
        Assert.Equal(canceled, error.CancellationToken);
        Assert.Empty(output.Bytes);
        Assert.Empty(pool.Outstanding);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(11)]
    public void DestinationFailuresKeepTheOriginalExceptionWriteBoundariesAndSerialPrefix(int failOnWrite)
    {
        using EntropyTokens tokens = RawSegments(6, 4097);
        var failure = new IOException("Injected parallel entropy destination failure.");
        using var expected = new ObservedStream(failOnWrite, failure);
        Assert.Same(failure, Assert.Throws<IOException>(() => ReplaySerial(tokens, EmptyCoding(), expected)));
        var pool = new TrackingPool();
        using var actual = new ObservedStream(failOnWrite, failure);
        Assert.Same(failure, Assert.Throws<IOException>(() =>
            tokens.TryReplayParallel(actual, EmptyCoding(), 3, TestContext.Current.CancellationToken, pool)));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void MissingHuffmanSymbolsFailAtTheSerialPrefixAndReturnAllWorkerBuffers()
    {
        long[] frequencies = new long[256];
        frequencies[5] = 1;
        var table = HuffmanTable.CreateOptimized(frequencies);
        var coding = new HuffmanCodingPlan.ScanCoding([table], [table], [], []);
        using var tokens = new EntropyTokens();
        for (int segment = 0; segment < 5; segment++)
        {
            for (int index = 0; index < 4097; index++)
            {
                tokens.AddBits(unchecked((uint)index * 0x9e3779b9U), 32);
            }
            if (segment is 2 or 4)
            {
                tokens.AddHuffman(0, true, 0, 0, 0);
            }
            tokens.AddRestart(segment & 7);
        }

        using var expected = new ObservedStream();
        InvalidOperationException serial = Assert.Throws<InvalidOperationException>(() => ReplaySerial(tokens, coding, expected));
        using var actual = new ObservedStream();
        var pool = new TrackingPool();
        InvalidOperationException parallel = Assert.Throws<InvalidOperationException>(() =>
            tokens.TryReplayParallel(actual, coding, 5, TestContext.Current.CancellationToken, pool));
        Assert.Equal(serial.Message, parallel.Message);
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void CancellationDuringDestinationWritesPreservesTheOriginalTokenAndACompleteOrderedPrefix()
    {
        using EntropyTokens tokens = RawSegments(9, 4097);
        using var complete = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), complete);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var actual = new ObservedStream(onWrite: () => cancellation.Cancel());
        var pool = new TrackingPool();
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            tokens.TryReplayParallel(actual, EmptyCoding(), 3, cancellation.Token, pool));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.InRange(actual.Bytes.Length, 1, complete.Bytes.Length - 1);
        Assert.Equal(complete.Bytes[..actual.Bytes.Length], actual.Bytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void ActualRentalsAboveTheSharedLimitAreReturnedAndEachRangeFallsBackInOrder()
    {
        using EntropyTokens tokens = RawSegments(5, 4097);
        using var expected = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), expected);
        var pool = new TrackingPool(extraLength: EntropyTokens.ParallelReplayMaximumBytes);
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, EmptyCoding(), 3, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.True(pool.Rents > 0);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void IndividuallyAcceptableInflatedRentalsShareOneCapacityLimitAndLeaveLaterRangesSerial()
    {
        using EntropyTokens tokens = RawSegments(3, 4097);
        using var expected = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), expected);
        // Each request needs much less than a bucket. Two actual buckets fill
        // the shared cap, so the third range must retain its serial writer.
        var pool = new TrackingPool(fixedLength: EntropyTokens.ParallelReplayMaximumBytes / 2);
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, EmptyCoding(), 3, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Equal(2, pool.Rents);
        Assert.InRange(pool.MaximumOutstandingBytes, 1, EntropyTokens.ParallelReplayMaximumBytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void FailedBatchRentalReturnsEarlierBuffersAndPreservesTheOriginalException()
    {
        using EntropyTokens tokens = RawSegments(5, 4097);
        var failure = new IOException("Injected entropy worker rental failure.");
        var pool = new TrackingPool(throwOnRent: 2, failure: failure);
        using var actual = new ObservedStream();
        Assert.Same(failure, Assert.Throws<IOException>(() =>
            tokens.TryReplayParallel(actual, EmptyCoding(), 3, TestContext.Current.CancellationToken, pool)));
        Assert.Empty(actual.Bytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(1, pool.Rents);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    [Fact]
    public void AnOversizedRangeKeepsTheSerialWriterBetweenBoundedParallelNeighbors()
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int segment = 0; segment < 3; segment++)
        {
            int length = segment == 1 ? (EntropyTokens.ParallelReplayMaximumBytes / 16) + 1 : 4097;
            for (int index = 0; index < length; index++)
            {
                tokens.AddBits(unchecked((uint)index * 0x9e3779b9U), 32);
            }
            if (segment < 2)
            {
                tokens.AddRestart(segment);
            }
        }
        using var expected = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), expected);
        var pool = new TrackingPool();
        using var actual = new ObservedStream();
        Assert.True(tokens.TryReplayParallel(actual, EmptyCoding(), 3, TestContext.Current.CancellationToken, pool));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.Equal(2, pool.Rents);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    private static EntropyTokens RawSegments(int count, int length)
    {
        var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int segment = 0; segment < count; segment++)
        {
            for (int index = 0; index < length; index++)
            {
                tokens.AddBits(unchecked((uint)index * 0x9e3779b9U), 32);
            }
            if (segment + 1 < count)
            {
                tokens.AddRestart(segment & 7);
            }
        }
        return tokens;
    }

    private static HuffmanCodingPlan.ScanCoding EmptyCoding() => new([], [], [], []);

    private static void ReplaySerial(EntropyTokens tokens, HuffmanCodingPlan.ScanCoding coding, Stream output)
    {
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, coding, TestContext.Current.CancellationToken);
        writer.Flush();
    }

    private sealed class TrackingPool(int extraLength = 0, int throwOnRent = 0, Exception? failure = null,
        int fixedLength = 0) : ArrayPool<byte>
    {
        private readonly Lock gate = new();
        private int rentAttempts;
        private long outstandingBytes;

        internal int Rents { get; private set; }
        internal int Returns { get; private set; }
        internal long MaximumOutstandingBytes { get; private set; }
        internal HashSet<byte[]> Outstanding { get; } = [];

        public override byte[] Rent(int minimumLength)
        {
            this.rentAttempts++;
            if (this.rentAttempts == throwOnRent)
            {
                throw failure ?? new IOException("Injected worker rental failure.");
            }
            byte[] buffer = new byte[Math.Max(minimumLength + extraLength, fixedLength)];
            Array.Fill(buffer, (byte)0xcd);
            lock (this.gate)
            {
                this.Rents++;
                Assert.True(this.Outstanding.Add(buffer));
                this.outstandingBytes += buffer.Length;
                this.MaximumOutstandingBytes = Math.Max(this.MaximumOutstandingBytes, this.outstandingBytes);
            }
            return buffer;
        }

        public override void Return(byte[] array, bool clearArray = false)
        {
            lock (this.gate)
            {
                Assert.True(this.Outstanding.Remove(array), "A worker buffer was returned more than once.");
                this.Returns++;
                this.outstandingBytes -= array.Length;
            }
            Array.Fill(array, (byte)0xcd);
        }
    }

    private sealed class ObservedStream(int failOnWrite = 0, Exception? failure = null, Action? onWrite = null) : Stream
    {
        private readonly MemoryStream inner = new();
        private readonly int ownerThread = Environment.CurrentManagedThreadId;

        internal byte[] Bytes => this.inner.ToArray();
        // A negative entry distinguishes WriteByte from a one-byte span write.
        internal List<int> Writes { get; } = [];
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.BeforeWrite(buffer.Length);
            this.inner.Write(buffer);
            onWrite?.Invoke();
        }

        public override void WriteByte(byte value)
        {
            this.BeforeWrite(-1);
            this.inner.WriteByte(value);
            onWrite?.Invoke();
        }

        private void BeforeWrite(int length)
        {
            Assert.Equal(this.ownerThread, Environment.CurrentManagedThreadId);
            this.Writes.Add(length);
            if (this.Writes.Count == failOnWrite)
            {
                throw failure ?? new IOException("Injected destination failure.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
