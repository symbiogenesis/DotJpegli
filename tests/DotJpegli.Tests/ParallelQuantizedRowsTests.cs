namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ParallelQuantizedRowsTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    public void QuantizedRowsMatchCompleteCacheAndOwnPartialSourceAndStrengths(bool subsampled, int cache)
    {
        const int width = 257, height = 129, stride = (width * 3) + 7;
        byte[] bytes = new byte[stride * height];
        new Random(92187).NextBytes(bytes);
        byte[] original = bytes.ToArray();
        PixelSource source = new(bytes, PixelLayout.Rgb24);
        var sampling = JpegSamplingFactors.FromSubsampling(subsampled ? ChromaSubsampling.Quarter : ChromaSubsampling.None);
        PixelSource prepared = cache switch
        {
            1 => source.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken),
            2 when subsampled => source.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken),
            2 => source.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken),
            3 when subsampled => source.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken,
                new EncoderCacheBudget(96 * 1024), allowPartial: true),
            3 => source.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken,
                new EncoderCacheBudget(96 * 1024), allowPartial: true),
            _ => source
        };
        using IDisposable? storage = prepared.ConvertedStorage;
        if (cache == 3)
        {
            Assert.NotNull(storage);
            Assert.False(prepared.HasCompleteConvertedPlanes);
        }
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Enumerable.Range(0, 64)
            .Select(index => (ushort)(1 + (((index * 3) + component) % 17))).ToArray()).ToArray();
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables,
            subsampled ? 2 : 1, subsampled ? 2 : 1, false);
        Quantization.PreparedTable[] quantizers = Enumerable.Range(0, 3).Select(component => Quantization.PreparedTable.Create(tables[component],
            component, 1f, adaptiveQuantization: true, adaptiveMultipliers: multipliers[component],
            imageWidth: width, imageHeight: height)).ToArray();
        PixelSource oracle = new(original, PixelLayout.Rgb24);
        using var oracleAnalysis = new AdaptiveQuantization(width, height, sampling.MaxVertical);
        using QuantizedImage expected = QuantizedImage.Create(oracle, width, height, stride, sampling, tables,
            1f, 0, oracleAnalysis, multipliers, TestContext.Current.CancellationToken, zigZagOrdered: true)!;
        var rows = new ParallelDctRows(prepared, width, height, stride, sampling, 0, 3, TestContext.Current.CancellationToken);
        Assert.True(rows.CanPrepareQuantizedRows);
        Assert.False(rows.HasFloatRowStorage);
        Assert.False(rows.HasQuantizedRowStorage);
        // Complete planes may be borrowed; missing/partial planes must retain
        // the exact source precision in the worker-owned snapshot.
        Array.Clear(bytes);
        using var analysis = new AdaptiveQuantization(width, height, sampling.MaxVertical);
        PixelSource analysisSource = rows.SourceForAnalysis;
        Span<float> strengths = analysis.Strengths;
        int mcuHeight = 8 * sampling.MaxVertical;
        for (int top = 0; top < height; top += mcuHeight)
        {
            analysis.ComputeRow(analysisSource, stride, top, tables[0][1], strengths, TestContext.Current.CancellationToken);
            rows.PrepareQuantizedRow(top, quantizers, strengths, TestContext.Current.CancellationToken);
            strengths.Fill(float.NaN);
            for (int component = 0; component < 3; component++)
            {
                JpegComponentSampling factor = sampling[component];
                int columns = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
                int totalRows = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
                int firstRow = top / mcuHeight * factor.Vertical;
                for (int y = firstRow; y < Math.Min(firstRow + factor.Vertical, totalRows); y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        Assert.Equal(expected.GetBlock(component, x, y).ToArray(), rows.GetQuantizedBlock(component, x, y).ToArray());
                    }
                }
            }
        }
        Assert.False(rows.HasFloatRowStorage);
        Assert.True(rows.HasQuantizedRowStorage);
        rows.PrepareRow(0, TestContext.Current.CancellationToken);
        Assert.True(rows.HasFloatRowStorage);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void StreamingWorkerRowsPreserveJpegsWithoutAnImageCoefficientCache(int precision, bool subsampled)
    {
        const int width = 257, height = 129, stride = (width * 3) + 7;
        Array pixels = Input(precision, height * stride);
        var options = new JpegliEncoderOptions
        {
            Quality = 90,
            Subsampling = subsampled ? ChromaSubsampling.Quarter : ChromaSubsampling.None,
            RestartInterval = 7
        };
        byte[] expected = Encode(pixels, width, height, stride, options, TestContext.Current.CancellationToken);
        foreach (int cacheBytes in new[] { 0, 96 * 1024, 192 * 1024 * 1024 })
        {
            foreach (int tokenBytes in new[] { 0, 64 * 1024 * 1024 })
            {
                var diagnostics = new EncoderCacheDiagnostics();
                byte[] actual = Encode(pixels, width, height, stride, options with
                {
                    MaxDegreeOfParallelism = 4,
                    MaxCacheBytes = cacheBytes,
                    EntropyTokenBudgetBytes = tokenBytes,
                    CacheDiagnostics = diagnostics
                }, TestContext.Current.CancellationToken);
                Assert.Equal(expected, actual);
                Assert.Equal(0, diagnostics.CoefficientPeakBytes);
                Assert.Equal(0, diagnostics.CurrentBytes);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UncachedEmissionReadsOwnedSourceAndKeepsDestinationOnCallingThread(bool subsampled)
    {
        const int width = 257, height = 129;
        byte[] pixels = (byte[])Input(0, width * height * 3);
        var options = new JpegliEncoderOptions
        {
            Quality = 90,
            MaxCacheBytes = 0,
            EntropyTokenBudgetBytes = 0,
            MaxDegreeOfParallelism = 4,
            Subsampling = subsampled ? ChromaSubsampling.Quarter : ChromaSubsampling.None,
            SmoothingFactor = 37,
            RestartRows = 1
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        bool changed = false;
        int callingThread = Environment.CurrentManagedThreadId;
        using var observed = new ObservedStream(destination, () =>
        {
            Assert.Equal(callingThread, Environment.CurrentManagedThreadId);
            if (!changed)
            {
                Array.Clear(pixels);
                changed = true;
            }
        });
        JpegliEncoder.Encode(observed, pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(changed);
        Assert.Equal(expected, destination.ToArray());
    }

    [Fact]
    public void DestinationFailureAndCancellationPreserveExactPrefixes()
    {
        const int width = 257, height = 129;
        byte[] pixels = (byte[])Input(0, width * height * 3);
        var options = new JpegliEncoderOptions
        {
            Quality = 90,
            MaxCacheBytes = 0,
            EntropyTokenBudgetBytes = 0,
            MaxDegreeOfParallelism = 4,
            RestartInterval = 7
        };
        byte[] complete = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        var failure = new IOException("Injected quantized row output failure.");
        using var failed = new MemoryStream();
        using var throwing = new ObservedStream(failed, () =>
        {
            if (failed.Length >= complete.Length / 2)
            {
                throw failure;
            }
        });
        Assert.Same(failure, Assert.Throws<IOException>(() => JpegliEncoder.Encode(throwing, pixels,
            width, height, options: options, cancellationToken: TestContext.Current.CancellationToken)));
        Assert.Equal(complete[..(int)failed.Length], failed.ToArray());
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var partial = new MemoryStream();
        using var canceling = new ObservedStream(partial, () =>
        {
            if (partial.Length >= complete.Length / 2)
            {
                canceled.Cancel();
            }
        });
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() => JpegliEncoder.Encode(canceling,
            pixels, width, height, options: options, cancellationToken: canceled.Token));
        Assert.Equal(canceled.Token, error.CancellationToken);
        Assert.InRange(partial.Length, complete.Length / 2, complete.Length - 1);
        Assert.Equal(complete[..(int)partial.Length], partial.ToArray());
    }

    [Fact]
    public void WorkerFailuresDoNotExposePartialRowsOrAggregateExceptions()
    {
        const int width = 257, height = 129;
        float[] pixels = Enumerable.Repeat(.5f, width * height * 3).ToArray();
        pixels[0] = float.NaN;
        PixelSource source = new(pixels, PixelComponentLayout.Rgb);
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.Quarter);
        var rows = new ParallelDctRows(source, width, height, width * 3, sampling, 0, 4, TestContext.Current.CancellationToken);
        Quantization.PreparedTable[] quantizers = Enumerable.Range(0, 3).Select(component => Quantization.PreparedTable.Create(
            Enumerable.Repeat((ushort)3, 64).ToArray(), component, 1f)).ToArray();
        _ = Assert.Throws<ArgumentException>(() => rows.PrepareQuantizedRow(0, quantizers, [], TestContext.Current.CancellationToken));
        _ = Assert.Throws<InvalidOperationException>(() => { _ = rows.GetQuantizedBlock(0, 0, 0).Length; });
        Assert.False(rows.HasFloatRowStorage);
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        canceled.Cancel();
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            rows.PrepareQuantizedRow(0, quantizers, [], canceled.Token));
        Assert.Equal(canceled.Token, error.CancellationToken);
    }

    private static Array Input(int precision, int length)
    {
        byte[] bytes = new byte[length];
        new Random(97133).NextBytes(bytes);
        return precision switch
        {
            1 => bytes.Select(value => (ushort)(value * 257)).ToArray(),
            2 => bytes.Select(value => value / 255f).ToArray(),
            _ => bytes
        };
    }

    private static byte[] Encode(Array pixels, int width, int height, int stride, JpegliEncoderOptions options,
        CancellationToken cancellationToken) => pixels switch
        {
            byte[] bytes => JpegliEncoder.Encode(bytes, width, height, PixelLayout.Rgb24, options, stride, cancellationToken),
            ushort[] words => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Rgb, options, stride, cancellationToken),
            float[] floats => JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgb, options, stride, cancellationToken),
            _ => throw new ArgumentException("Unexpected source precision.", nameof(pixels))
        };

    private sealed class ObservedStream(MemoryStream destination, Action afterWrite) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => destination.Length;
        public override long Position { get => destination.Position; set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            destination.Write(buffer);
            afterWrite();
        }
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void WriteByte(byte value)
        {
            destination.WriteByte(value);
            afterWrite();
        }
        public override void Flush() => destination.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
