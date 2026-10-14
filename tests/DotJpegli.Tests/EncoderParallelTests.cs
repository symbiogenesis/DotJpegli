using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderParallelTests
{
    public static TheoryData<int, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<int, int>();
            foreach (int precision in new[] { 0, 1, 2 })
                for (int mode = 0; mode < 12; mode++)
                    modes.Add(precision, mode);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void ParallelPreparationPreservesCompleteJpegBytes(int precision, int mode)
    {
        // Several MCU rows plus odd edges exercise real, replicated and wholly
        // padded blocks, including predictors crossing row/restart boundaries.
        const int width = 37, height = 35;
        PixelComponentLayout layout = Layout(mode);
        int stride = width * Components(layout) + 11;
        Array pixels = Pattern(precision, width, height, layout, stride, trailing: 7);
        var options = Options(mode);
        byte[] expected = Encode(pixels, width, height, layout, options, stride);
        Assert.Equal(1, options.MaxDegreeOfParallelism);
        foreach (int degree in new[] { -1, 2, 5 })
            Assert.Equal(expected, Encode(pixels, width, height, layout,
                options with { MaxDegreeOfParallelism = degree }, stride));
    }

    [Theory]
    [InlineData(0, 1, 33)]
    [InlineData(0, 35, 1)]
    [InlineData(0, 19, 33)]
    [InlineData(1, 1, 33)]
    [InlineData(1, 35, 1)]
    [InlineData(1, 19, 33)]
    [InlineData(2, 1, 33)]
    [InlineData(2, 35, 1)]
    [InlineData(2, 19, 33)]
    public void NarrowImagesPaddedRowsAndIgnoredAlphaPreserveBytes(int precision, int width, int height)
    {
        Array compact = Pattern(precision, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, SmoothingFactor = 37, RestartInterval = 2
        };
        byte[] expected = Encode(compact, width, height, PixelComponentLayout.Rgb, options);
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba, PixelComponentLayout.Bgra })
        {
            int stride = width * Components(layout) + 13;
            // The shortest array has no final-row padding. Float alpha, padding
            // and trailing slots contain NaN and must never become consumed data.
            foreach (int trailing in new[] { 0, 17 })
            {
                Array padded = Pattern(precision, width, height, layout, stride, trailing);
                Array original = (Array)padded.Clone();
                Assert.Equal(expected, Encode(padded, width, height, layout,
                    options with { MaxDegreeOfParallelism = 3 }, stride));
                PixelAssert.Equal(original, padded);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(int.MinValue)]
    public void InvalidDegreeIsRejectedBeforeOutputForEveryPrecision(int degree)
    {
        foreach (int precision in new[] { 0, 1, 2 })
        {
            using var output = new MemoryStream();
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output,
                Pattern(precision, 1, 1, PixelComponentLayout.Rgb), 1, 1, PixelComponentLayout.Rgb,
                new() { MaxDegreeOfParallelism = degree }));
            Assert.Equal(nameof(JpegliEncoderOptions.MaxDegreeOfParallelism), error.ParamName);
            Assert.Equal(0, output.Length);
            Assert.True(output.CanWrite);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void PreCancellationPreservesTokenAndWritesNothing(int precision)
    {
        using var output = new MemoryStream();
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output,
            Pattern(precision, 37, 35, PixelComponentLayout.Rgb), 37, 35, PixelComponentLayout.Rgb,
            new() { MaxDegreeOfParallelism = 3 }, cancellationToken: token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NonSeekableDestinationsReceiveOrderedWritesOnTheCallingThread(int precision)
    {
        const int width = 37, height = 35;
        Array pixels = Pattern(precision, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions { RestartRows = 1 };
        byte[] expected = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        int callerThread = Environment.CurrentManagedThreadId;
        using var destination = new MemoryStream();
        using var output = new ObservedStream(destination, () =>
            Assert.Equal(callerThread, Environment.CurrentManagedThreadId));
        Encode(output, pixels, width, height, PixelComponentLayout.Rgb,
            options with { MaxDegreeOfParallelism = int.MaxValue });
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CancellationDuringOutputPreservesTokenAndOrderedPrefix(int precision)
    {
        const int width = 37, height = 35;
        Array pixels = Pattern(precision, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            MaxDegreeOfParallelism = 3, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] complete = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new ObservedStream(destination, () =>
        {
            if (destination.Length >= complete.Length / 2)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() =>
            Encode(output, pixels, width, height, PixelComponentLayout.Rgb, options,
                cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, complete.Length / 2, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DestinationFailuresRemainUnwrappedAndLeaveTheStreamOpen(int precision)
    {
        const int width = 37, height = 35;
        Array pixels = Pattern(precision, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions { MaxDegreeOfParallelism = 3, RestartRows = 1 };
        byte[] complete = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        var failure = new IOException("Injected destination failure.");
        using var destination = new MemoryStream();
        using var output = new ObservedStream(destination, () =>
        {
            if (destination.Length >= complete.Length / 2)
                throw failure;
        });
        Assert.Same(failure, Assert.Throws<IOException>(() =>
            Encode(output, pixels, width, height, PixelComponentLayout.Rgb, options)));
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, complete.Length / 2, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidFloatSamplesAndUnencodableCoefficientsWriteNoOutput(bool optimize)
    {
        const int width = 37, height = 35;
        // A failure in the final source row catches validation/preflight paths
        // that process only the first row or leak an AggregateException.
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.MaxValue, 1000f })
        {
            float[] pixels = (float[])Pattern(2, width, height, PixelComponentLayout.Gray);
            pixels[^1] = invalid;
            using var output = new MemoryStream();
            var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels,
                width, height, PixelComponentLayout.Gray, new()
                {
                    MaxDegreeOfParallelism = 3, AllowOutOfRangeFloatSamples = true,
                    AdaptiveQuantization = false, Quality = 100, OptimizeHuffmanTables = optimize
                }));
            Assert.Equal("pixels", error.ParamName);
            Assert.Equal(0, output.Length);
            Assert.True(output.CanWrite);
        }
        byte[] valid = JpegliEncoder.Encode(new float[width * height], width, height, PixelComponentLayout.Gray,
            new() { MaxDegreeOfParallelism = 3 }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(0xff, valid[^2]);
        Assert.Equal(0xd9, valid[^1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PermittedOutOfRangeFloatSamplesPreserveBytesAfterCoefficientPreflight(bool progressive)
    {
        const int width = 37, height = 35;
        float[] pixels = Enumerable.Range(0, width * height)
            .Select(index => index % 3 == 0 ? -0.1f : index % 3 == 1 ? 1.1f : 0.4f).ToArray();
        var options = new JpegliEncoderOptions
        {
            AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
            Progressive = progressive, RestartRows = 1, Quality = 30
        };
        byte[] expected = Encode(pixels, width, height, PixelComponentLayout.Gray, options);
        Assert.Equal(expected, Encode(pixels, width, height, PixelComponentLayout.Gray,
            options with { MaxDegreeOfParallelism = 3 }));
    }

    [Fact]
    public void WorkerComputationFailuresPreserveTheOriginalExceptionType()
    {
        // Public float preflight rejects these samples before scheduling. Use
        // the internal row processor to exercise its worker-error boundary.
        const int width = 37, height = 35;
        float[] samples = Enumerable.Repeat(float.MaxValue, width * height * 3).ToArray();
        var pixels = new PixelSource(samples, PixelComponentLayout.Rgb).WithColorSpace(JpegColorSpace.Xyb);
        var rows = new ParallelDctRows(pixels, width, height, width * 3,
            new(new(2, 2), new(2, 2), new(1, 1)), 0, 3, TestContext.Current.CancellationToken);
        var error = Assert.Throws<ArgumentException>(() => rows.PrepareRow(0, TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
    }

    [Fact]
    public async Task IndependentConcurrentEncodesPreserveBytesWithReusableOptions()
    {
        const int width = 37, height = 35;
        var options = new JpegliEncoderOptions
        {
            MaxDegreeOfParallelism = 3, Progressive = true, ProgressiveRefinementBits = 2,
            SmoothingFactor = 37, RestartRows = 1
        };
        Array[] pixels = Enumerable.Range(0, 3)
            .Select(precision => Pattern(precision, width, height, PixelComponentLayout.Rgb)).ToArray();
        byte[][] expected = pixels.Select(source => Encode(source, width, height, PixelComponentLayout.Rgb,
            options with { MaxDegreeOfParallelism = 1 })).ToArray();
        Task<byte[]>[] encodes = Enumerable.Range(0, 6).Select(index => Task.Run(() =>
            Encode(pixels[index % 3], width, height, PixelComponentLayout.Rgb, options),
            TestContext.Current.CancellationToken)).ToArray();
        byte[][] actual = await Task.WhenAll(encodes);
        for (int index = 0; index < actual.Length; index++)
            Assert.Equal(expected[index % 3], actual[index]);
    }

    private static PixelComponentLayout Layout(int mode) => mode switch
    {
        6 => PixelComponentLayout.Gray,
        7 => PixelComponentLayout.YCbCr,
        8 => PixelComponentLayout.Xyb,
        _ => mode % 2 == 0 ? PixelComponentLayout.Rgba : PixelComponentLayout.Bgra
    };

    private static JpegliEncoderOptions Options(int mode) => mode switch
    {
        0 => new(),
        1 => new() { Subsampling = ChromaSubsampling.None, ProgressiveLevel = 2, RestartRows = 1, SmoothingFactor = 37 },
        2 => new() { Subsampling = ChromaSubsampling.HalfHorizontalThirdVertical, AdaptiveQuantization = false,
            OptimizeHuffmanTables = false, SequentialInterleaveComponents = false, RestartInterval = 3 },
        3 => new() { SamplingFactors = new(new(3, 1), new(2, 1), new(1, 1)),
            ScanScript = ProgressiveScript(), OptimizeHuffmanTables = false, RestartRows = 1 },
        4 => new() { ColorSpace = JpegColorSpace.Rgb, Progressive = true, ProgressiveRefinementBits = 2,
            AdaptiveQuantization = false, RestartInterval = 2, SmoothingFactor = 37 },
        5 => new() { ColorSpace = JpegColorSpace.Xyb, IccProfile = JpegliIccProfiles.CreateXybProfile(),
            ProgressiveLevel = 2, RestartRows = 1 },
        6 => new() { Progressive = true, ProgressiveRefinementBits = 2,
            OptimizeHuffmanTables = false, SmoothingFactor = 100, RestartInterval = 2 },
        7 => new() { SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            ScanScript = ProgressiveScript(), RestartInterval = 2 },
        8 => new() { IccProfile = JpegliIccProfiles.CreateXybProfile(), ProgressiveLevel = 2, RestartInterval = 3 },
        9 => new() { ForceBaseline = false, SequentialInterleaveComponents = false,
            QuantizationTables = new ReadOnlyMemory<ushort>[]
            {
                Enumerable.Range(0, 64).Select(index => (ushort)(index == 0 ? 7 : 1 + index * 11)).ToArray(),
                Enumerable.Range(0, 64).Select(index => (ushort)(index == 0 ? 13 : 1 + index * 7)).ToArray()
            } },
        10 => new() { ScanScript = [new([2]), new([0, 1])], SmoothingFactor = 37, RestartRows = 1 },
        _ => new() { ColorSpace = JpegColorSpace.Grayscale, Quality = 97,
            OptimizeHuffmanTables = false, RestartInterval = 2 }
    };

    private static JpegScan[] ProgressiveScript() =>
    [
        new([2], 0, 0, 0, 1), new([0, 1], 0, 0, 0, 1),
        new([0], 1, 63, 0, 1), new([1], 1, 63, 0, 1), new([2], 1, 63, 0, 1),
        new([0, 1, 2], 0, 0, 1, 0),
        new([0], 1, 63, 1, 0), new([1], 1, 63, 1, 0), new([2], 1, 63, 1, 0)
    ];

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Pattern(int precision, int width, int height, PixelComponentLayout layout,
        int stride = 0, int trailing = 0)
    {
        int components = Components(layout);
        if (stride == 0)
            stride = width * components;
        int length = (height - 1) * stride + width * components + trailing;
        Array pixels = precision switch
        {
            0 => Enumerable.Repeat((byte)231, length).ToArray(),
            1 => Enumerable.Repeat((ushort)54321, length).ToArray(),
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        bool bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < (layout == PixelComponentLayout.Gray ? 1 : 3); channel++)
                {
                    int offset = y * stride + x * components + (bgr ? 2 - channel : channel);
                    int value = (x * 29 + y * 53 + channel * 97 + x * y * 3) & 255;
                    if (pixels is byte[] bytes)
                        bytes[offset] = (byte)value;
                    else if (pixels is ushort[] words)
                        words[offset] = (ushort)Math.Min(65535, value * 257 + (x * 17 + y * 31 + channel * 11) % 257);
                    else
                        ((float[])pixels)[offset] = Math.Min(1f,
                            (value + (x * 17 + y * 31 + channel * 11) % 257 / 257f) / 255f);
                }
        return pixels;
    }

    private static PixelLayout ByteLayout(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Bgr => PixelLayout.Bgr24,
        PixelComponentLayout.Rgba => PixelLayout.Rgba32,
        PixelComponentLayout.Bgra => PixelLayout.Bgra32,
        PixelComponentLayout.Gray => PixelLayout.Gray8,
        PixelComponentLayout.YCbCr => PixelLayout.YCbCr24,
        PixelComponentLayout.Xyb => PixelLayout.Xyb24,
        _ => PixelLayout.Rgb24
    };

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, layout, options, stride);
        return output.ToArray();
    }

    private static void Encode(Stream output, Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0, CancellationToken? cancellationToken = null)
    {
        CancellationToken token = cancellationToken ?? TestContext.Current.CancellationToken;
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, ByteLayout(layout), options, stride, token);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, token);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, token);
    }

    private sealed class ObservedStream(Stream inner, Action onWrite) : Stream
    {
        public bool Closed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !Closed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
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
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
