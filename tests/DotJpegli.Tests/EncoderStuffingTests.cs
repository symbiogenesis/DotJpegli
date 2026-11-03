namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class EncoderStuffingTests
{
    [Theory]
    [InlineData(0, false, 0, 1)]
    [InlineData(0, true, 3, 3)]
    [InlineData(1, false, 0, 3)]
    [InlineData(1, true, 3, 1)]
    [InlineData(2, false, 3, 3)]
    [InlineData(2, true, 0, 1)]
    [InlineData(3, false, 0, 1)]
    [InlineData(3, true, 3, 3)]
    public void StuffingPreferencesPreserveCompleteJpegsAndEveryDestinationWrite(
        int mode, bool subsampled, int restartInterval, int workers)
    {
        const int width = 257, height = 193;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(mode) with
        {
            Subsampling = subsampled ? ChromaSubsampling.Quarter : ChromaSubsampling.None,
            RestartInterval = restartInterval,
            MaxDegreeOfParallelism = workers
        };
        long[] budgets = mode == 3 ? [0L] : [0L, 64L * 1024 * 1024];
        foreach (long budget in budgets)
        {
            options = options with { EntropyTokenBudgetBytes = budget };
            using var expected = new ObservedStream();
            Encode(expected, pixels, width, height,
                options with { EntropyByteStuffingKernel = KernelPreference.Original });
            foreach (KernelPreference preference in new[] { KernelPreference.Automatic, KernelPreference.Optimized })
            {
                using var actual = new ObservedStream();
                Encode(actual, pixels, width, height, options with { EntropyByteStuffingKernel = preference });
                AssertWritesEqual(expected, actual);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GrayscaleTinyAndOddEdgesKeepIdenticalPaddingAndMarkers(bool progressive)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 35), (17, 19) })
        {
            byte[] pixels = Enumerable.Range(0, width * height).Select(index => (byte)(index * 37)).ToArray();
            var options = new JpegliEncoderOptions
            {
                Progressive = progressive,
                ProgressiveRefinementBits = progressive ? 2 : 0,
                RestartRows = 1,
                EntropyByteStuffingKernel = KernelPreference.Original
            };
            byte[] expected = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Gray8, options,
                cancellationToken: TestContext.Current.CancellationToken);
            foreach (KernelPreference preference in new[] { KernelPreference.Automatic, KernelPreference.Optimized })
            {
                Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height, PixelLayout.Gray8,
                    options with { EntropyByteStuffingKernel = preference },
                    cancellationToken: TestContext.Current.CancellationToken));
            }
        }
    }

    [Theory]
    [InlineData(0, 0L, 1)]
    [InlineData(0, 67108864L, 3)]
    [InlineData(1, 0L, 3)]
    [InlineData(1, 67108864L, 3)]
    public void PartialDestinationFailuresAndCancellationPreserveExactWritesAndPrefixes(
        int mode, long budget, int workers)
    {
        const int width = 257, height = 193;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(mode) with
        {
            EntropyTokenBudgetBytes = budget,
            MaxDegreeOfParallelism = workers,
            RestartInterval = mode == 0 ? 97 : 0,
            EntropyByteStuffingKernel = KernelPreference.Original
        };
        using var complete = new ObservedStream();
        Encode(complete, pixels, width, height, options);
        // Interrupt a real entropy-buffer drain after headers and table writes.
        int interruptedWrite = complete.Writes.FindIndex(write => write.Bytes.Length >= 4080);
        Assert.True(interruptedWrite >= 0);
        foreach (int acceptedBytes in new[] { 0, 1, int.MaxValue })
        {
            var failure = new IOException("Injected byte-stuffing destination failure.");
            using var expected = new ObservedStream(interruptedWrite, acceptedBytes, failure);
            Assert.Same(failure, Assert.Throws<IOException>(() => Encode(expected, pixels, width, height, options)));
            using var actual = new ObservedStream(interruptedWrite, acceptedBytes, failure);
            Assert.Same(failure, Assert.Throws<IOException>(() => Encode(actual, pixels, width, height,
                options with { EntropyByteStuffingKernel = KernelPreference.Optimized })));
            AssertWritesEqual(expected, actual);
            Assert.True(expected.CanWrite);
            Assert.True(actual.CanWrite);
            Assert.Equal(complete.ToArray()[..(int)actual.Length], actual.ToArray());
        }

        using var expectedCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var actualCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var canceledExpected = new ObservedStream(interruptedWrite, cancellation: expectedCancellation);
        using var canceledActual = new ObservedStream(interruptedWrite, cancellation: actualCancellation);
        OperationCanceledException expectedError = Assert.Throws<OperationCanceledException>(() =>
            Encode(canceledExpected, pixels, width, height, options, expectedCancellation.Token));
        OperationCanceledException actualError = Assert.Throws<OperationCanceledException>(() =>
            Encode(canceledActual, pixels, width, height,
                options with { EntropyByteStuffingKernel = KernelPreference.Optimized }, actualCancellation.Token));
        Assert.Equal(expectedCancellation.Token, expectedError.CancellationToken);
        Assert.Equal(actualCancellation.Token, actualError.CancellationToken);
        AssertWritesEqual(canceledExpected, canceledActual);
        Assert.InRange(canceledActual.Length, 1, complete.Length - 1);
        Assert.Equal(complete.ToArray()[..(int)canceledActual.Length], canceledActual.ToArray());
        Assert.True(canceledExpected.CanWrite);
        Assert.True(canceledActual.CanWrite);
        using var recovered = new ObservedStream();
        Encode(recovered, pixels, width, height, options with { EntropyByteStuffingKernel = KernelPreference.Optimized });
        AssertWritesEqual(complete, recovered);
    }

    [Fact]
    public async Task ConcurrentEncodesKeepTheirOwnStuffingPreference()
    {
        const int width = 129, height = 97;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(1) with { MaxDegreeOfParallelism = 2 };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height,
            options: options with { EntropyByteStuffingKernel = KernelPreference.Original },
            cancellationToken: TestContext.Current.CancellationToken);
        Task<byte[]>[] encodes = Enum.GetValues<KernelPreference>().Select(preference => Task.Run(() =>
            JpegliEncoder.Encode(pixels, width, height,
                options: options with { EntropyByteStuffingKernel = preference },
                cancellationToken: TestContext.Current.CancellationToken), TestContext.Current.CancellationToken)).ToArray();
        foreach (byte[] actual in await Task.WhenAll(encodes))
        {
            Assert.Equal(expected, actual);
        }
    }

    private static void Encode(Stream output, byte[] pixels, int width, int height,
        JpegliEncoderOptions options, CancellationToken? cancellationToken = null)
        => JpegliEncoder.Encode(output, pixels, width, height, options: options,
            cancellationToken: cancellationToken ?? TestContext.Current.CancellationToken);

    private static JpegliEncoderOptions Options(int mode)
        => new()
        {
            Distance = 1.7f,
            Progressive = mode == 1,
            ProgressiveRefinementBits = mode == 1 ? 2 : 0,
            OptimizeHuffmanTables = mode != 3,
            ScanScript = mode == 2 ?
            [
                new([2], 0, 0, 0, 1), new([0, 1], 0, 0, 0, 1),
                new([0], 1, 5, 0, 1), new([0], 6, 63, 0, 1),
                new([1], 1, 63, 0, 1), new([2], 1, 63, 0, 1),
                new([0, 1, 2], 0, 0, 1, 0),
                new([0], 1, 63, 1, 0), new([1], 1, 63, 1, 0), new([2], 1, 63, 1, 0)
            ] : null
        };

    private static byte[] Pattern(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        new Random(741817).NextBytes(pixels);
        return pixels;
    }

    private static void AssertWritesEqual(ObservedStream expected, ObservedStream actual)
    {
        Assert.Equal(expected.Writes.Count, actual.Writes.Count);
        for (int index = 0; index < expected.Writes.Count; index++)
        {
            Assert.Equal(expected.Writes[index].ByteWrite, actual.Writes[index].ByteWrite);
            Assert.Equal(expected.Writes[index].Bytes, actual.Writes[index].Bytes);
        }
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    private sealed class ObservedStream(int interruptedWrite = -1, int acceptedBytes = int.MaxValue,
        IOException? failure = null, CancellationTokenSource? cancellation = null) : Stream
    {
        private readonly MemoryStream destination = new();
        internal List<(bool ByteWrite, byte[] Bytes)> Writes { get; } = [];
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => this.destination.CanWrite;
        public override long Length => this.destination.Length;
        public override long Position { get => this.destination.Position; set => throw new NotSupportedException(); }
        public override void Flush() => throw new InvalidOperationException("Encoding must not flush its destination.");
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer) => this.WriteCore(buffer, byteWrite: false);
        public override void WriteByte(byte value) => this.WriteCore([value], byteWrite: true);
        internal byte[] ToArray() => this.destination.ToArray();

        private void WriteCore(ReadOnlySpan<byte> bytes, bool byteWrite)
        {
            int index = this.Writes.Count;
            this.Writes.Add((byteWrite, bytes.ToArray()));
            bool interrupted = index == interruptedWrite;
            this.destination.Write(interrupted && failure is not null ? bytes[..Math.Min(acceptedBytes, bytes.Length)] : bytes);
            if (interrupted)
            {
                cancellation?.Cancel();
                if (failure is not null)
                {
                    throw failure;
                }
            }
        }

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
