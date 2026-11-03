namespace DotJpegli.Tests;

using Xunit;

public sealed class EncoderTokenBudgetTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CompleteJpegBytesSurviveSharedBudgetExhaustionAcrossScanKinds(int mode)
    {
        const int width = 145, height = 97;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(mode);
        byte[] expected = Encode(pixels, width, height, options);
        // The irregular limits include unusable trailing bytes. A single
        // initial rental can survive while later scans fall back, or an
        // abandoned large scan can release capacity for smaller later scans.
        foreach (long budget in new long[] { 0, 64, 32768 + 63, 65536 + 63 })
        {
            Assert.Equal(expected, Encode(pixels, width, height,
                options with { EntropyTokenBudgetBytes = budget }));
        }
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(32768L)]
    [InlineData(65599L)]
    public void CancellationAndDestinationFailuresPreserveTheJpegPrefixAndAllowAnotherEncode(long budget)
    {
        const int width = 67, height = 49;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(4) with { EntropyTokenBudgetBytes = budget };
        byte[] expected = Encode(pixels, width, height, options);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var canceledDestination = new MemoryStream();
        using var canceledOutput = new ObservedStream(canceledDestination, () =>
        {
            if (canceledDestination.Length >= expected.Length / 2)
            {
                cancellation.Cancel();
            }
        });
        OperationCanceledException canceled = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(canceledOutput, pixels, width, height, options: options,
                cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken);
        Assert.True(canceledOutput.CanWrite);
        AssertPrefix(expected, canceledDestination);

        var failure = new IOException("Injected shared-token-budget destination failure.");
        using var failedDestination = new MemoryStream();
        using var failedOutput = new ObservedStream(failedDestination, () =>
        {
            if (failedDestination.Length >= expected.Length / 2)
            {
                throw failure;
            }
        });
        Assert.Same(failure, Assert.Throws<IOException>(() => JpegliEncoder.Encode(failedOutput, pixels,
            width, height, options: options, cancellationToken: TestContext.Current.CancellationToken)));
        Assert.True(failedOutput.CanWrite);
        AssertPrefix(expected, failedDestination);
        Assert.Equal(expected, Encode(pixels, width, height, options));
    }

    [Fact]
    public async Task ConcurrentEncodesKeepIndependentBudgetsAndFallbackDecisions()
    {
        const int width = 67, height = 35;
        byte[] pixels = Pattern(width, height);
        JpegliEncoderOptions options = Options(4) with { MaxDegreeOfParallelism = 2 };
        byte[] expected = Encode(pixels, width, height, options);
        long[] budgets = [0, 64, 32768 + 63, 65536 + 63, 64L * 1024 * 1024];
        Task<byte[]>[] encodes = budgets.Select(budget => Task.Run(() => Encode(pixels, width, height,
            options with { EntropyTokenBudgetBytes = budget }), TestContext.Current.CancellationToken)).ToArray();
        foreach (byte[] actual in await Task.WhenAll(encodes))
        {
            Assert.Equal(expected, actual);
        }
    }

    private static byte[] Encode(byte[] pixels, int width, int height, JpegliEncoderOptions options)
        => JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static void AssertPrefix(byte[] expected, MemoryStream destination)
    {
        Assert.InRange(destination.Length, expected.Length / 2, expected.Length - 1);
        Assert.Equal(expected[..(int)destination.Length], destination.ToArray());
    }

    private static JpegliEncoderOptions Options(int mode)
    {
        var options = new JpegliEncoderOptions
        {
            Quality = 95,
            OptimizeHuffmanTables = true,
            Subsampling = ChromaSubsampling.Quarter,
            RestartInterval = 3
        };
        return mode switch
        {
            0 => options,
            1 => options with { SequentialInterleaveComponents = false },
            2 => options with
            {
                ScanScript = [new([2]), new([0, 1])],
                SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
                RestartInterval = 0,
                RestartRows = 1
            },
            3 => options with
            {
                Progressive = true,
                ProgressiveRefinementBits = 2,
                ProgressiveAcScanEnds = [2, 17, 63],
                ProgressiveAcRefinementBits = [0, 1, 2]
            },
            4 => options with { ScanScript = ProgressiveScript(), SmoothingFactor = 37 },
            _ => options with
            {
                ColorSpace = JpegColorSpace.Grayscale,
                ScanScript =
                [
                    new([0], 0, 0, 0, 1), new([0], 32, 63, 0, 1), new([0], 1, 31, 0, 1),
                    new([0], 0, 0, 1, 0), new([0], 1, 63, 1, 0)
                ]
            }
        };
    }

    private static JpegScan[] ProgressiveScript() =>
    [
        new([2], 0, 0, 0, 1), new([0, 1], 0, 0, 0, 1),
        new([0], 1, 31, 0, 1), new([0], 32, 63, 0, 1),
        new([1], 1, 63, 0, 1), new([2], 1, 63, 0, 1),
        new([0, 1, 2], 0, 0, 1, 0),
        new([0], 1, 63, 1, 0), new([1], 1, 63, 1, 0), new([2], 1, 63, 1, 0)
    ];

    private static byte[] Pattern(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        var random = new Random(741813);
        random.NextBytes(pixels);
        return pixels;
    }

    private sealed class ObservedStream(Stream inner, Action onWrite) : Stream
    {
        private bool closed;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !this.closed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            inner.Write(buffer);
            onWrite();
        }
        public override void WriteByte(byte value)
        {
            inner.WriteByte(value);
            onWrite();
        }
        protected override void Dispose(bool disposing)
        {
            this.closed = true;
            base.Dispose(disposing);
        }
    }
}
