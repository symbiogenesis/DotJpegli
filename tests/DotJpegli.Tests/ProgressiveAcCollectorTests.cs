namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class ProgressiveAcCollectorTests
{
    [Fact]
    public void FusionRequiresRepeatedAcScansForOneComponent()
    {
        JpegScan[] scans = [new([0, 1, 2], 0, 0), new([0], 1, 63), new([1], 1, 63), new([2], 1, 63)];
        Assert.False(ProgressiveAcCollector.ShouldFuse([]));
        Assert.False(ProgressiveAcCollector.ShouldFuse([new([0], 0, 0), new([0], 1, 63)]));
        Assert.False(ProgressiveAcCollector.ShouldFuse(scans));
        Assert.False(ProgressiveAcCollector.ShouldFuse([.. scans, new([0], 0, 0, 1, 0)]));
        Assert.True(ProgressiveAcCollector.ShouldFuse([.. scans, new([2], 1, 63, 1, 0)]));
        Assert.True(ProgressiveAcCollector.ShouldFuse(Script(1)));
        Assert.True(ProgressiveAcCollector.ShouldFuse(Script(3)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    public void BlockMajorCollectionMatchesIndependentScanMajorHistogramsAndReplay(int cacheMode, int configuration)
    {
        const int width = 67, height = 51;
        bool grayscale = configuration == 0;
        JpegSamplingFactors sampling = configuration == 2 ? new(new(1, 2), new(2, 1), new(1, 1))
            : JpegSamplingFactors.FromSubsampling(configuration == 1 ? ChromaSubsampling.Quarter : ChromaSubsampling.None);
        if (grayscale)
        {
            sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        }
        int components = grayscale ? 1 : 3, stride = width * components;
        byte[] pixels = Pixels(width, height, components);
        PixelSource source = new(pixels, grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        ushort[][] tables = Enumerable.Range(0, components).Select(component =>
            Enumerable.Range(0, 64).Select(index => (ushort)(1 + (((index * 3) + component) % 17))).ToArray()).ToArray();
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, grayscale);
        using var analysis = new AdaptiveQuantization(width, height, sampling.MaxVertical);
        using QuantizedImage full = QuantizedImage.Create(source, width, height, stride, sampling,
            tables, 1f, 37, analysis, multipliers, TestContext.Current.CancellationToken, zigZagOrdered: true)!;
        int[] widths = Enumerable.Range(0, components).Select(component =>
            ((width * sampling[component].Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal)).ToArray();
        int[] heights = Enumerable.Range(0, components).Select(component =>
            ((height * sampling[component].Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical)).ToArray();
        long partialBytes = QuantizedImage.PlannedBytes(widths, heights, 1, sampling, segmented: true);
        using QuantizedImage? partial = cacheMode is 1 or 3 ? QuantizedImage.Create(source, width, height, stride,
            sampling, tables, 1f, 37, analysis, multipliers, TestContext.Current.CancellationToken,
            zigZagOrdered: cacheMode != 3, maximumBytes: partialBytes, allowPartial: true) : null;
        if (cacheMode is 1 or 3)
        {
            Assert.NotNull(partial);
            Assert.False(partial.IsComplete);
            Assert.True(partial.ContainsMcuRow(0));
            Assert.False(partial.ContainsMcuRow(8 * sampling.MaxVertical));
        }
        QuantizedImage? actualCache = cacheMode == 2 ? full : partial;
        JpegScan[] scans = Script(components);
        foreach (bool restarts in new[] { false, true })
        {
            int[] intervals = Enumerable.Range(0, components).Select(component => restarts ? 2 + component : 0).ToArray();
            long[][][] expected = Frequencies(scans, components);
            using var referenceTokens = new EntropyTokenScans(scans, true);
            CollectScanMajor(full, widths, heights, scans, expected, referenceTokens, intervals);
            foreach (int tokenBudget in new[] { 0, 64, 65536 })
            {
                long[][][] actual = Frequencies(scans, components);
                var diagnostics = new EncoderCacheDiagnostics();
                var budget = new EncoderCacheBudget(tokenBudget, diagnostics);
                using (var tokens = new EntropyTokenScans(scans, true, tokenBudget, cacheBudget: budget))
                {
                    ProgressiveAcCollector.Collect(source, width, height, stride, sampling, tables, 1f, 37,
                        analysis, multipliers, scans, actual, tokens, intervals, actualCache,
                        TestContext.Current.CancellationToken);
                    Assert.InRange(diagnostics.PeakBytes, 0, tokenBudget);
                    for (int scan = 0; scan < scans.Length; scan++)
                    {
                        for (int component = 0; component < components; component++)
                        {
                            Assert.Equal(expected[scan][component], actual[scan][component]);
                        }
                        if (scans[scan].SpectralStart > 0 && tokens[scan].IsComplete)
                        {
                            int component = scans[scan].Components[0];
                            Assert.Equal(Replay(referenceTokens[scan], expected[scan][component], component, components),
                                Replay(tokens[scan], actual[scan][component], component, components));
                        }
                    }
                }
                Assert.Equal(0, diagnostics.CurrentBytes);
            }
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void CompleteProgressiveJpegsMatchAcrossBudgetsScriptsAndWorkers(int scriptMode, bool parallel)
    {
        int width = parallel ? 257 : 67, height = parallel ? 65 : 51;
        byte[] pixels = Pixels(width, height, 3);
        var options = new JpegliEncoderOptions
        {
            Quality = 90,
            Progressive = true,
            ProgressiveLevel = scriptMode == 1 ? 2 : null,
            ScanScript = scriptMode == 2 ? Script(3) : null,
            RestartRows = 1,
            MaxDegreeOfParallelism = parallel ? 4 : 1,
            SmoothingFactor = 37
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        foreach (int cacheBytes in new[] { 0, 6144, 65536 })
        {
            foreach (int tokenBytes in new[] { 0, 64, 65536 })
            {
                Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height,
                    options: options with { MaxCacheBytes = cacheBytes, EntropyTokenBudgetBytes = tokenBytes },
                    cancellationToken: TestContext.Current.CancellationToken));
            }
        }
    }

    [Fact]
    public void CancellationDuringTokenRentalReturnsEveryLease()
    {
        const int width = 19, height = 23;
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var pool = new CancelingPool(canceled);
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(1024, diagnostics);
        JpegScan[] scans = Script(1);
        long[][][] frequencies = Frequencies(scans, 1);
        PixelSource source = new(Pixels(width, height, 1), PixelLayout.Gray8);
        ushort[][] tables = [Enumerable.Repeat((ushort)3, 64).ToArray()];
        using (var tokens = new EntropyTokenScans(scans, true, 1024, pool, budget))
        {
            OperationCanceledException? error = null;
            try
            {
                ProgressiveAcCollector.Collect(source, width, height, width,
                    JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None), tables, 1f, 0,
                    null, null, scans, frequencies, tokens, [1], null, canceled.Token);
            }
            catch (OperationCanceledException exception)
            {
                error = exception;
            }
            Assert.NotNull(error);
            Assert.Equal(canceled.Token, error.CancellationToken);
        }
        Assert.Equal(pool.Rents, pool.Returns);
        Assert.True(pool.Rents > 0);
        Assert.Equal(0, diagnostics.CurrentBytes);
        Assert.InRange(diagnostics.PeakBytes, 0, 1024);
    }

    private static JpegScan[] Script(int components)
    {
        int[] all = Enumerable.Range(0, components).ToArray();
        var scans = new List<JpegScan> { new(all, 0, 0, 0, 2) };
        for (int component = components - 1; component >= 0; component--)
        {
            scans.Add(new([component], 32, 63, 0, 2));
            scans.Add(new([component], 1, 31, 0, 2));
        }
        scans.Add(new(all, 0, 0, 2, 1));
        for (int component = 0; component < components; component++)
        {
            scans.Add(new([component], 1, 63, 2, 1));
        }
        scans.Add(new(all, 0, 0, 1, 0));
        for (int component = components - 1; component >= 0; component--)
        {
            scans.Add(new([component], 1, 7, 1, 0));
            scans.Add(new([component], 8, 63, 1, 0));
        }
        return scans.ToArray();
    }

    private static long[][][] Frequencies(JpegScan[] scans, int components)
        => scans.Select(scan => Enumerable.Range(0, components).Select(component =>
            scan.SpectralStart > 0 && scan.Components[0] == component ? new long[256] : []).ToArray()).ToArray();

    // Deliberately retains the old scan-major traversal and per-scan state.
    private static void CollectScanMajor(QuantizedImage image, int[] widths, int[] heights,
        JpegScan[] scans, long[][][] frequencies, EntropyTokenScans tokens, int[] intervals)
    {
        for (int index = 0; index < scans.Length; index++)
        {
            JpegScan scan = scans[index];
            if (scan.SpectralStart == 0)
            {
                continue;
            }
            int component = scan.Components[0], remaining = intervals[component], restart = 0, pending = 0;
            var refinement = new ProgressiveAcRefinementState();
            for (int y = 0; y < heights[component]; y++)
            {
                for (int x = 0; x < widths[component]; x++)
                {
                    if (intervals[component] != 0 && remaining == 0)
                    {
                        Flush();
                        tokens[index].AddRestart(restart);
                        restart = (restart + 1) & 7;
                        remaining = intervals[component];
                    }
                    if (scan.SuccessiveHigh == 0)
                    {
                        ProgressiveEntropyEncoder.AccumulateAcFrequencies(image.GetBlock(component, x, y),
                            frequencies[index][component], ref pending, scan.SuccessiveLow, scan.SpectralStart,
                            scan.SpectralEnd, coefficientsValidated: true, tokens: tokens[index], component: component,
                            zigZagOrdered: true);
                    }
                    else
                    {
                        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(image.GetBlock(component, x, y),
                            frequencies[index][component], refinement, scan.SuccessiveLow, scan.SpectralStart,
                            scan.SpectralEnd, coefficientsValidated: true, tokens: tokens[index], component: component,
                            zigZagOrdered: true);
                    }
                    if (intervals[component] != 0)
                    {
                        remaining--;
                    }
                }
            }
            Flush();

            void Flush()
            {
                if (scan.SuccessiveHigh == 0)
                {
                    ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies[index][component], ref pending, tokens[index], component);
                }
                else
                {
                    ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(frequencies[index][component], refinement, tokens[index], component);
                }
            }
        }
    }

    private static byte[] Replay(EntropyTokens tokens, long[] frequencies, int component, int components)
    {
        HuffmanTable[] dc = Enumerable.Repeat(HuffmanTable.CreateDefault(false, false), components).ToArray();
        HuffmanTable[] ac = Enumerable.Repeat(HuffmanTable.CreateDefault(true, false), components).ToArray();
        ac[component] = HuffmanTable.CreateOptimized(frequencies);
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, new(dc, ac, new byte[components], []), TestContext.Current.CancellationToken);
        writer.Flush();
        return output.ToArray();
    }

    private static byte[] Pixels(int width, int height, int components)
    {
        byte[] pixels = new byte[width * height * components];
        new Random(98764).NextBytes(pixels);
        return pixels;
    }

    private sealed class CancelingPool(CancellationTokenSource canceled) : ArrayPool<uint>
    {
        internal int Rents { get; private set; }
        internal int Returns { get; private set; }

        public override uint[] Rent(int minimumLength)
        {
            this.Rents++;
            canceled.Cancel();
            return new uint[minimumLength];
        }

        public override void Return(uint[] array, bool clearArray = false) => this.Returns++;
    }
}
