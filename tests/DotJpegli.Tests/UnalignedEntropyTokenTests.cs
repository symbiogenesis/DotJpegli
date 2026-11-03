namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class UnalignedEntropyTokenTests
{
    private const int ChunkTokens = 8192;

    [Fact]
    public void EveryBitAlignmentAcrossChunksPreservesStuffingPaddingAndDestinationWrites()
    {
        for (int alignment = 0; alignment < 64; alignment++)
        {
            using var tokens = new EntropyTokens(huffmanContextMask: 0);
            for (int index = 0; index < (ChunkTokens * 2) + 3; index++)
            {
                // Three final records vary the first seam across every possible
                // position in the merge writer's 64-bit accumulator. Ones
                // force stuffing on both sides of every unpadded seam.
                int count = index == ChunkTokens - 3 ? alignment / 3
                    : index == ChunkTokens - 2 ? alignment / 3
                    : index == ChunkTokens - 1 ? alignment - (2 * (alignment / 3)) : 24;
                tokens.AddBits((1U << count) - 1, count);
            }
            AssertParity(tokens, EmptyCoding(), workers: 3);
        }
    }

    [Theory]
    [InlineData(1, 2, false)]
    [InlineData(2, 5, false)]
    [InlineData(63, 64, false)]
    [InlineData(63, 3, true)]
    public void HuffmanContextsAndStrictSuffixFallbackCrossTokenRentals(int mask, int workers, bool noncanonical)
    {
        var table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        var coding = new HuffmanCodingPlan.ScanCoding([table, table, table], [table, table, table], [], []);
        using var tokens = new EntropyTokens(new EntropyTokenBudget(), huffmanContextMask: mask, new OddTokenPool());
        for (int index = 0; index < (ChunkTokens * 4) + 29; index++)
        {
            int context = mask == 63 ? index % 6 : mask == 1 ? 0 : 1;
            int symbol = index & 255;
            int width = symbol == 0xf0 ? 0 : (symbol & 15) == 0 ? symbol >> 4 : symbol & 15;
            if (noncanonical && index % 11 == 10)
            {
                width = (width + 3) % 17;
            }
            uint suffix = unchecked((uint)index * 0x9e3779b9U) & ((1U << width) - 1);
            tokens.AddHuffman(context >> 1, (context & 1) != 0, symbol, suffix, width);
            if (index % 17 == 0)
            {
                tokens.AddBits(0xffffff, 24);
            }
        }
        AssertParity(tokens, coding, workers);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 7)]
    [InlineData(3, 0)]
    [InlineData(5, 11)]
    public void DestinationFailuresKeepExceptionIdentityAndExactSerialPrefix(int failOnWrite, int acceptedBytes)
    {
        using EntropyTokens tokens = RawTokens(ChunkTokens * 5);
        var failure = new IOException("Injected unaligned entropy destination failure.");
        using var expected = new ObservedStream(failOnWrite, acceptedBytes, failure);
        Assert.Same(failure, Assert.Throws<IOException>(() => ReplaySerial(tokens, EmptyCoding(), expected)));
        using var actual = new ObservedStream(failOnWrite, acceptedBytes, failure);
        var pool = new TrackingPool();
        Assert.Same(failure, Assert.Throws<IOException>(() => ReplayParallel(tokens, EmptyCoding(), actual, 3, pool)));
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        AssertReturned(pool);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void MissingSymbolsUseStrictReplayAtTheOriginalPendingBitPrefix(int badChunk)
    {
        long[] frequencies = new long[256];
        frequencies[5] = 1;
        var table = HuffmanTable.CreateOptimized(frequencies);
        var coding = new HuffmanCodingPlan.ScanCoding([table], [table], [], []);
        using var tokens = new EntropyTokens();
        for (int index = 0; index < ChunkTokens * 5; index++)
        {
            if (index == (badChunk * ChunkTokens) + 719)
            {
                tokens.AddHuffman(0, true, 0, 0, 0);
            }
            else
            {
                tokens.AddBits((uint)index & 0x7fffff, 23);
            }
        }
        using var expected = new ObservedStream();
        InvalidOperationException serial = Assert.Throws<InvalidOperationException>(() => ReplaySerial(tokens, coding, expected));
        using var actual = new ObservedStream();
        var pool = new TrackingPool();
        InvalidOperationException parallel = Assert.Throws<InvalidOperationException>(() => ReplayParallel(tokens, coding, actual, 3, pool));
        Assert.Equal(serial.Message, parallel.Message);
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        AssertReturned(pool);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OversizedOrInflatedRentalsRetainSerialOutputAndTheSharedCapacityBound(bool oversized)
    {
        using EntropyTokens tokens = RawTokens(ChunkTokens * 5);
        var pool = new TrackingPool(fixedLength: oversized
            ? EntropyTokens.ParallelReplayMaximumBytes + 1 : EntropyTokens.ParallelReplayMaximumBytes / 2);
        AssertParity(tokens, EmptyCoding(), workers: 5, pool);
        Assert.InRange(pool.MaximumAcceptedBytes, 0, EntropyTokens.ParallelReplayMaximumBytes);
    }

    [Fact]
    public void RentalFailureReturnsEarlierBuffersWithoutTouchingTheDestination()
    {
        using EntropyTokens tokens = RawTokens(ChunkTokens * 5);
        var failure = new IOException("Injected unaligned worker rental failure.");
        var pool = new TrackingPool(throwOnRent: 2, failure: failure);
        using var output = new ObservedStream();
        Assert.Same(failure, Assert.Throws<IOException>(() => ReplayParallel(tokens, EmptyCoding(), output, 3, pool)));
        Assert.Empty(output.Bytes);
        AssertReturned(pool);
    }

    [Fact]
    public void CancellationAfterAWriteReturnsRentalsAndLeavesReusableTokens()
    {
        using EntropyTokens tokens = RawTokens(ChunkTokens * 5);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var output = new ObservedStream(onWrite: cancellation.Cancel);
        var pool = new TrackingPool();
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            tokens.TryReplayParallel(output, EmptyCoding(), 3, cancellation.Token, pool));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.NotEmpty(output.Bytes);
        AssertReturned(pool);
        using var complete = new ObservedStream();
        ReplaySerial(tokens, EmptyCoding(), complete);
        Assert.Equal(complete.Bytes[..output.Bytes.Length], output.Bytes);
        AssertParity(tokens, EmptyCoding(), workers: 2);
    }

    [Fact]
    public void TinySingleWorkerAndPreCanceledScansDoNotRentOrWrite()
    {
        using EntropyTokens tiny = RawTokens(7);
        using EntropyTokens large = RawTokens(ChunkTokens * 3);
        using var output = new ObservedStream();
        var pool = new TrackingPool();
        Assert.False(tiny.TryReplayParallel(output, EmptyCoding(), 5, TestContext.Current.CancellationToken, pool));
        Assert.False(large.TryReplayParallel(output, EmptyCoding(), 1, TestContext.Current.CancellationToken, pool));
        var canceled = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            large.TryReplayParallel(output, EmptyCoding(), 3, canceled, pool));
        Assert.Equal(canceled, error.CancellationToken);
        Assert.Empty(output.Bytes);
        Assert.Equal(0, pool.Rents);
    }

    [Fact]
    public void EligibilityBoundaryAndZeroBitRecordsRetainAnEmptyScan()
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int index = 0; index < (ChunkTokens * 2) - 1; index++)
        {
            tokens.AddBits(0, 0);
        }
        using var output = new ObservedStream();
        var pool = new TrackingPool();
        Assert.False(tokens.TryReplayParallel(output, EmptyCoding(), 2, TestContext.Current.CancellationToken, pool));
        Assert.Equal(0, pool.Rents);
        tokens.AddBits(0, 0);
        ReplayParallel(tokens, EmptyCoding(), output, 2, pool);
        Assert.Empty(output.Bytes);
        Assert.Empty(output.Writes);
        Assert.Equal(2, pool.Rents);
        AssertReturned(pool);
    }

    [Fact]
    public void SplitThirtyTwoBitRawRecordsAndMoreThanThirtyTwoChunksPreserveBatchSeams()
    {
        using var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int index = 0; index < (ChunkTokens * 17) + 9; index++)
        {
            tokens.AddBits(unchecked((uint)index * 0x9e3779b9U), 32);
        }
        AssertParity(tokens, EmptyCoding(), workers: int.MaxValue);
    }

    private static EntropyTokens RawTokens(int count)
    {
        var tokens = new EntropyTokens(huffmanContextMask: 0);
        for (int index = 0; index < count; index++)
        {
            tokens.AddBits(unchecked((uint)index * 0x9e3779b9U) & 0xffffff, 24);
        }
        return tokens;
    }

    private static HuffmanCodingPlan.ScanCoding EmptyCoding() => new([], [], [], []);

    private static void AssertParity(EntropyTokens tokens, HuffmanCodingPlan.ScanCoding coding, int workers, TrackingPool? pool = null)
    {
        using var expected = new ObservedStream();
        ReplaySerial(tokens, coding, expected);
        using var actual = new ObservedStream();
        pool ??= new TrackingPool();
        ReplayParallel(tokens, coding, actual, workers, pool);
        Assert.Equal(expected.Bytes, actual.Bytes);
        Assert.Equal(expected.Writes, actual.Writes);
        Assert.True(pool.Rents > 0);
        AssertReturned(pool);
    }

    private static void AssertReturned(TrackingPool pool)
    {
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.Rents, pool.Returns);
    }

    private static void ReplaySerial(EntropyTokens tokens, HuffmanCodingPlan.ScanCoding coding, Stream output)
    {
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, coding, TestContext.Current.CancellationToken);
        writer.Flush();
    }

    private static void ReplayParallel(EntropyTokens tokens, HuffmanCodingPlan.ScanCoding coding, Stream output, int workers, TrackingPool pool)
        => Assert.True(tokens.TryReplayParallel(output, coding, workers, TestContext.Current.CancellationToken, pool));

    private sealed class OddTokenPool : ArrayPool<uint>
    {
        public override uint[] Rent(int minimumLength) => new uint[minimumLength + 17];
        public override void Return(uint[] array, bool clearArray = false) => Array.Fill(array, uint.MaxValue);
    }

    private sealed class TrackingPool(int fixedLength = 0, int throwOnRent = 0, Exception? failure = null) : ArrayPool<byte>
    {
        private int attempts;
        private long acceptedBytes;

        internal int Rents { get; private set; }
        internal int Returns { get; private set; }
        internal long MaximumAcceptedBytes { get; private set; }
        internal HashSet<byte[]> Outstanding { get; } = [];

        public override byte[] Rent(int minimumLength)
        {
            if (++this.attempts == throwOnRent)
            {
                throw failure ?? new IOException("Injected rental failure.");
            }
            byte[] buffer = new byte[Math.Max(minimumLength, fixedLength)];
            Array.Fill(buffer, (byte)0xcd);
            Assert.True(this.Outstanding.Add(buffer));
            this.Rents++;
            // Oversized buckets are transiently rented then rejected; only
            // accepted capacity belongs to the staging budget.
            if (buffer.Length <= EntropyTokens.ParallelReplayMaximumBytes)
            {
                this.acceptedBytes += buffer.Length;
                this.MaximumAcceptedBytes = Math.Max(this.MaximumAcceptedBytes, this.acceptedBytes);
            }
            return buffer;
        }

        public override void Return(byte[] array, bool clearArray = false)
        {
            Assert.True(this.Outstanding.Remove(array));
            this.Returns++;
            if (array.Length <= EntropyTokens.ParallelReplayMaximumBytes)
            {
                this.acceptedBytes -= array.Length;
            }
            Array.Fill(array, (byte)0xcd);
        }
    }

    private sealed class ObservedStream(int failOnWrite = 0, int acceptedBytes = 0, Exception? failure = null,
        Action? onWrite = null) : Stream
    {
        private readonly MemoryStream inner = new();
        private readonly int callerThread = Environment.CurrentManagedThreadId;

        internal byte[] Bytes => this.inner.ToArray();
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
            Assert.Equal(this.callerThread, Environment.CurrentManagedThreadId);
            this.Writes.Add(buffer.Length);
            if (this.Writes.Count == failOnWrite)
            {
                this.inner.Write(buffer[..Math.Min(acceptedBytes, buffer.Length)]);
                throw failure ?? new IOException("Injected destination failure.");
            }
            this.inner.Write(buffer);
            onWrite?.Invoke();
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
