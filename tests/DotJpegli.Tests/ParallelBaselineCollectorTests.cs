namespace DotJpegli.Tests;

using System.Buffers;
using System.Collections.Concurrent;
using DotJpegli.Internal;
using Xunit;

public sealed class ParallelBaselineCollectorTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CompleteBaselineJpegsMatchSerialCollectionWithCustomGroupsAndBudgets(bool customScript, bool grayscale)
    {
        const int width = 265, height = 137;
        int components = grayscale ? 1 : 3;
        byte[] pixels = new byte[width * height * components];
        new Random(27183).NextBytes(pixels);
        var options = new JpegliEncoderOptions
        {
            Quality = 90,
            RestartInterval = 7,
            ScanScript = customScript ? grayscale ? [new([0])] : [new([0, 2]), new([1])] : null
        };
        PixelLayout layout = grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24;
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);
        foreach (int tokenBudget in new[] { 0, 64, 65536 })
        {
            Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height, layout,
                options with { MaxDegreeOfParallelism = 4, EntropyTokenBudgetBytes = tokenBudget },
                cancellationToken: TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PrivateHistogramsAndChunkPredictorsMatchSerialMcuOrder(int configuration)
    {
        const int width = 521, height = 273;
        (JpegSamplingFactors sampling, int components, JpegScan scan) = Configuration(configuration);
        using QuantizedImage image = Image(width, height, sampling, components);
        foreach (int restart in new[] { 0, 1, 7 })
        {
            long[][] expectedDc = Histograms(components), expectedAc = Histograms(components);
            using var expectedTokens = new EntropyTokens();
            CollectSerial(image, width, height, sampling, scan, restart, expectedDc, expectedAc, expectedTokens);
            foreach (int tokenBytes in new[] { 0, 64, 65536 })
            {
                long[][] actualDc = Histograms(components), actualAc = Histograms(components);
                var diagnostics = new EncoderCacheDiagnostics();
                var cacheBudget = new EncoderCacheBudget(tokenBytes, diagnostics);
                var pool = new TrackingPool();
                using (var actualTokens = new EntropyTokens(new EntropyTokenBudget(tokenBytes, cacheBudget)))
                {
                    Assert.True(ParallelBaselineCollector.TryCollect(image, width, height, sampling, scan,
                        restart, 4, actualDc, actualAc, actualTokens, TestContext.Current.CancellationToken, pool));
                    for (int component = 0; component < components; component++)
                    {
                        Assert.Equal(expectedDc[component], actualDc[component]);
                        Assert.Equal(expectedAc[component], actualAc[component]);
                    }
                    if (actualTokens.IsComplete)
                    {
                        Assert.Equal(Replay(expectedTokens, expectedDc, expectedAc), Replay(actualTokens, actualDc, actualAc));
                    }
                    Assert.InRange(diagnostics.PeakBytes, 0, tokenBytes);
                }
                Assert.Equal(0, diagnostics.CurrentBytes);
                Assert.Equal(pool.Rents, pool.Returns);
                Assert.Equal(0, pool.OutstandingBytes);
                Assert.InRange(pool.PeakBytes, 0, ParallelBaselineCollector.MaximumStagingBytes);
            }
            // Exercise complete shared token streams as well as abandoned ones.
            long[][] completeDc = Histograms(components), completeAc = Histograms(components);
            using var complete = new EntropyTokens();
            Assert.True(ParallelBaselineCollector.TryCollect(image, width, height, sampling, scan,
                restart, 3, completeDc, completeAc, complete, TestContext.Current.CancellationToken));
            Assert.True(complete.IsComplete);
            Assert.Equal(Replay(expectedTokens, expectedDc, expectedAc), Replay(complete, completeDc, completeAc));
        }
    }

    [Fact]
    public void OversizedStagingRentalsRecollectEachChunkWithoutDoublingHistograms()
    {
        const int width = 265, height = 137;
        (JpegSamplingFactors sampling, int components, JpegScan scan) = Configuration(2);
        using QuantizedImage image = Image(width, height, sampling, components);
        long[][] expectedDc = Histograms(components), expectedAc = Histograms(components);
        long[][] actualDc = Histograms(components), actualAc = Histograms(components);
        using var expected = new EntropyTokens();
        using var actual = new EntropyTokens();
        CollectSerial(image, width, height, sampling, scan, 7, expectedDc, expectedAc, expected);
        var pool = new TrackingPool(oversized: true);
        Assert.True(ParallelBaselineCollector.TryCollect(image, width, height, sampling, scan, 7, 4,
            actualDc, actualAc, actual, TestContext.Current.CancellationToken, pool));
        for (int component = 0; component < components; component++)
        {
            Assert.Equal(expectedDc[component], actualDc[component]);
            Assert.Equal(expectedAc[component], actualAc[component]);
        }
        Assert.True(actual.IsComplete);
        Assert.Equal(Replay(expected, expectedDc, expectedAc), Replay(actual, actualDc, actualAc));
        Assert.True(pool.Rents > 0);
        Assert.Equal(pool.Rents, pool.Returns);
        Assert.Equal(0, pool.OutstandingBytes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WorkerCancellationAndRentalFailureJoinAndReturnEveryPrivateLease(bool fail)
    {
        const int width = 521, height = 273;
        (JpegSamplingFactors sampling, int components, JpegScan scan) = Configuration(1);
        using QuantizedImage image = Image(width, height, sampling, components);
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var pool = new TrackingPool(fail: fail, canceled: fail ? null : canceled);
        using var tokens = new EntropyTokens();
        if (fail)
        {
            _ = Assert.Throws<InvalidOperationException>(() => ParallelBaselineCollector.TryCollect(image,
                width, height, sampling, scan, 0, 4, Histograms(components), Histograms(components),
                tokens, canceled.Token, pool));
        }
        else
        {
            OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
                ParallelBaselineCollector.TryCollect(image, width, height, sampling, scan, 0, 4,
                    Histograms(components), Histograms(components), tokens, canceled.Token, pool));
            Assert.Equal(canceled.Token, error.CancellationToken);
        }
        Assert.Equal(pool.Rents, pool.Returns);
        Assert.Equal(0, pool.OutstandingBytes);
    }

    [Fact]
    public void SmallOrNaturalOrderCachesLeaveOriginalCollectionUntouched()
    {
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        JpegScan scan = new([0], 0, 63);
        using QuantizedImage tiny = Image(1, 1, sampling, 1);
        using QuantizedImage natural = Image(137, 73, sampling, 1, ordered: false);
        using var tokens = new EntropyTokens();
        long[][] dc = Histograms(1), ac = Histograms(1);
        Assert.False(ParallelBaselineCollector.TryCollect(tiny, 1, 1, sampling, scan, 0, 4,
            dc, ac, tokens, TestContext.Current.CancellationToken));
        Assert.False(ParallelBaselineCollector.TryCollect(natural, 137, 73, sampling, scan, 0, 4,
            dc, ac, tokens, TestContext.Current.CancellationToken));
        Assert.All(dc[0], count => Assert.Equal(0, count));
        Assert.All(ac[0], count => Assert.Equal(0, count));
        Assert.True(tokens.IsComplete);
    }

    [Fact]
    public void AppendingPackedTokensPreservesExistingBitsSuffixesAndRestartRecords()
    {
        using var source = new EntropyTokens();
        for (int index = 0; index < 10003; index++)
        {
            source.AddHuffman(0, false, 1, (uint)(index & 1), 1);
        }
        source.AddBits(0x12345, 17);
        source.AddRestart(6);
        source.AddHuffman(0, true, 0, 0, 0);
        var coding = new HuffmanCodingPlan.ScanCoding([HuffmanTable.CreateDefault(false, false)],
            [HuffmanTable.CreateDefault(true, false)], [], []);
        using var expected = new MemoryStream();
        var expectedWriter = new BitWriter(expected, buffered: true);
        expectedWriter.WriteBits(3, 2);
        source.Replay(expectedWriter, coding, TestContext.Current.CancellationToken);
        expectedWriter.Flush();
        using var destination = new EntropyTokens();
        destination.AddBits(3, 2);
        source.AppendTo(destination, TestContext.Current.CancellationToken);
        using var actual = new MemoryStream();
        var actualWriter = new BitWriter(actual, buffered: true);
        destination.Replay(actualWriter, coding, TestContext.Current.CancellationToken);
        actualWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());
        _ = Assert.Throws<ArgumentException>(() => source.AppendTo(source, TestContext.Current.CancellationToken));
    }

    private static (JpegSamplingFactors Sampling, int Components, JpegScan Scan) Configuration(int configuration)
    {
        JpegSamplingFactors sampling = configuration == 2 ? new(new(1, 2), new(2, 1), new(1, 1))
            : JpegSamplingFactors.FromSubsampling(configuration == 0 ? ChromaSubsampling.None : ChromaSubsampling.Quarter);
        int components = configuration == 0 ? 1 : 3;
        JpegScan scan = new(configuration switch { 0 => [0], 3 => [1], 4 => [0, 2], _ => [0, 1, 2] });
        return (sampling, components, scan);
    }

    private static QuantizedImage Image(int width, int height, JpegSamplingFactors sampling, int components, bool ordered = true)
    {
        byte[] pixels = new byte[width * height * components];
        new Random(47731).NextBytes(pixels);
        PixelSource source = new(pixels, components == 1 ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        ushort[][] tables = Enumerable.Range(0, components).Select(_ => Enumerable.Repeat((ushort)3, 64).ToArray()).ToArray();
        return QuantizedImage.Create(source, width, height, width * components, sampling, tables, 1f, 0,
            null, null, TestContext.Current.CancellationToken, zigZagOrdered: ordered)!;
    }

    private static long[][] Histograms(int components) => Enumerable.Range(0, components).Select(_ => new long[256]).ToArray();

    private static void CollectSerial(QuantizedImage image, int width, int height, JpegSamplingFactors sampling,
        JpegScan scan, int restart, long[][] dc, long[][] ac, EntropyTokens tokens)
    {
        int[] widths = Enumerable.Range(0, dc.Length).Select(component =>
            ((width * sampling[component].Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal)).ToArray();
        int[] heights = Enumerable.Range(0, dc.Length).Select(component =>
            ((height * sampling[component].Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical)).ToArray();
        bool single = scan.Components.Count == 1;
        int columns = single ? widths[scan.Components[0]] : (width + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
        int rows = single ? heights[scan.Components[0]] : (height + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
        int[] previous = new int[dc.Length];
        short[] dummy = new short[64];
        int mcu = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++, mcu++)
            {
                if (mcu != 0 && restart != 0 && mcu % restart == 0)
                {
                    Array.Clear(previous);
                    tokens.AddRestart(((mcu / restart) - 1) & 7);
                }
                foreach (int component in scan.Components)
                {
                    int horizontal = single ? 1 : sampling[component].Horizontal;
                    int vertical = single ? 1 : sampling[component].Vertical;
                    for (int blockY = 0; blockY < vertical; blockY++)
                    {
                        for (int blockX = 0; blockX < horizontal; blockX++)
                        {
                            int bx = (x * horizontal) + blockX, by = (y * vertical) + blockY;
                            ReadOnlySpan<short> block;
                            if (bx >= widths[component] || by >= heights[component])
                            {
                                dummy[0] = checked((short)previous[component]);
                                block = dummy;
                            }
                            else
                            {
                                block = image.GetBlock(component, bx, by);
                            }
                            EntropyEncoder.AccumulateFrequencies(block, ref previous[component], dc[component], ac[component],
                                coefficientsValidated: true, tokens: tokens, component: component, zigZagOrdered: true);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Replay(EntropyTokens tokens, long[][] dc, long[][] ac)
    {
        HuffmanTable[] dcTables = dc.Select(frequencies => HuffmanTable.CreateOptimized(frequencies)).ToArray();
        HuffmanTable[] acTables = ac.Select(frequencies => HuffmanTable.CreateOptimized(frequencies)).ToArray();
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, new(dcTables, acTables, [], []), TestContext.Current.CancellationToken);
        writer.Flush();
        return output.ToArray();
    }

    private sealed class TrackingPool(bool oversized = false, bool fail = false,
        CancellationTokenSource? canceled = null) : ArrayPool<uint>
    {
        private readonly ConcurrentDictionary<uint[], byte> outstanding = new();
        private readonly Lock sync = new();

        internal int Rents { get; private set; }
        internal int Returns { get; private set; }
        internal long OutstandingBytes { get; private set; }
        internal long PeakBytes { get; private set; }

        public override uint[] Rent(int minimumLength)
        {
            if (fail)
            {
                throw new InvalidOperationException("Injected staging rental failure.");
            }
            uint[] rental = new uint[oversized ? (ParallelBaselineCollector.MaximumStagingBytes / sizeof(uint)) + 1 : minimumLength];
            Assert.True(this.outstanding.TryAdd(rental, 0));
            lock (this.sync)
            {
                this.Rents++;
                this.OutstandingBytes += (long)rental.Length * sizeof(uint);
                this.PeakBytes = Math.Max(this.PeakBytes, this.OutstandingBytes);
            }
            canceled?.Cancel();
            return rental;
        }

        public override void Return(uint[] array, bool clearArray = false)
        {
            Assert.True(this.outstanding.TryRemove(array, out _));
            lock (this.sync)
            {
                this.Returns++;
                this.OutstandingBytes -= (long)array.Length * sizeof(uint);
            }
        }
    }
}
