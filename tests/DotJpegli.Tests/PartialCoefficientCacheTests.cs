namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class PartialCoefficientCacheTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PartialCacheRetainsExactCompleteMcuRowsWithinItsActualBudget(bool subsampled, bool zigZag)
    {
        const int width = 37, height = 67;
        byte[] pixels = TestImages.Gradient(width, height);
        var source = new PixelSource(pixels, PixelLayout.Rgb24);
        var sampling = JpegSamplingFactors.FromSubsampling(subsampled
            ? ChromaSubsampling.Quarter : ChromaSubsampling.None);
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTableForDistance(1.45f, component)).ToArray();
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables,
            subsampled ? 2 : 1, subsampled ? 2 : 1, false);
        using var fullAnalysis = new AdaptiveQuantization(width, height, sampling.MaxVertical);
        using var partialAnalysis = new AdaptiveQuantization(width, height, sampling.MaxVertical);
        using QuantizedImage full = QuantizedImage.Create(source, width, height, width * 3, sampling,
            tables, 1.45f, 0, fullAnalysis, multipliers, TestContext.Current.CancellationToken, zigZagOrdered: zigZag)!;
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(4096, diagnostics);
        using (QuantizedImage partial = QuantizedImage.Create(source, width, height, width * 3, sampling,
            tables, 1.45f, 0, partialAnalysis, multipliers, TestContext.Current.CancellationToken,
            zigZagOrdered: zigZag, allowPartial: true, cacheBudget: budget)!)
        {
            Assert.NotNull(partial);
            Assert.False(partial.IsComplete);
            Assert.InRange(partial.RentedBytes, 1, 4096);
            Assert.Equal(partial.RentedBytes, diagnostics.CurrentBytes);
            for (int component = 0; component < 3; component++)
            {
                JpegComponentSampling factor = sampling[component];
                int columns = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
                int rows = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
                bool reachedUncached = false;
                for (int row = 0; row < rows; row++)
                {
                    if (!partial.ContainsRow(component, row))
                    {
                        reachedUncached = true;
                        continue;
                    }
                    Assert.False(reachedUncached);
                    Assert.True(partial.ContainsMcuRow(row / factor.Vertical * sampling.MaxVertical * 8));
                    for (int column = 0; column < columns; column++)
                    {
                        Assert.Equal(full.GetBlock(component, column, row).ToArray(),
                            partial.GetBlock(component, column, row).ToArray());
                    }
                }
                Assert.True(reachedUncached);
            }
        }
        Assert.Equal(0, diagnostics.CurrentBytes);
        Assert.Equal(4096, budget.AvailableBytes);
    }

    [Fact]
    public void SegmentsKeepTheEntireImageWhenComponentPoolRoundingWouldRejectIt()
    {
        const int width = 257, height = 257;
        var source = new PixelSource(TestImages.Gradient(width, height), PixelLayout.Rgb24);
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Quantization.CreateTableForDistance(4, component)).ToArray();
        using QuantizedImage full = QuantizedImage.Create(source, width, height, width * 3, sampling,
            tables, 4, 0, null, null, TestContext.Current.CancellationToken)!;
        using QuantizedImage segmented = QuantizedImage.Create(source, width, height, width * 3, sampling,
            tables, 4, 0, null, null, TestContext.Current.CancellationToken,
            maximumBytes: 500000, allowPartial: true)!;
        Assert.NotNull(segmented);
        Assert.True(segmented.IsComplete);
        Assert.InRange(segmented.RentedBytes, 1, 500000);
        Assert.True(full.RentedBytes > 500000);
        for (int component = 0; component < 3; component++)
        {
            for (int row = 0; row < 33; row++)
            {
                for (int column = 0; column < 33; column++)
                {
                    Assert.Equal(full.GetBlock(component, column, row).ToArray(),
                        segmented.GetBlock(component, column, row).ToArray());
                }
            }
        }
    }

    [Fact]
    public void PlanningDoesNotDoubleStorageAtTheFormerOneRowCliff()
    {
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        int[] widths = [512, 512, 512];
        int[] before = [256, 256, 256], after = [257, 257, 257];
        Assert.Equal(48L * 1024 * 1024, QuantizedImage.PlannedBytes(widths, before, 256, sampling, segmented: false));
        Assert.Equal(96L * 1024 * 1024, QuantizedImage.PlannedBytes(widths, after, 257, sampling, segmented: false));
        Assert.Equal((48L * 1024 * 1024) + (3 * 65536),
            QuantizedImage.PlannedBytes(widths, after, 257, sampling, segmented: true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrFailedRentalsRefundAllAcceptedStorage(bool fail)
    {
        const int width = 257, height = 257;
        byte[] pixels = TestImages.Gradient(width, height);
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Quantization.CreateTableForDistance(4, component)).ToArray();
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(500000, diagnostics);
        var pool = new ObservedPool(fail);
        if (fail)
        {
            _ = Assert.Throws<IOException>(() => QuantizedImage.Create(new PixelSource(pixels, PixelLayout.Rgb24), width, height, width * 3,
                sampling, tables, 4, 0, null, null, TestContext.Current.CancellationToken,
                allowPartial: true, cacheBudget: budget, pool: pool));
        }
        else
        {
            Assert.Null(QuantizedImage.Create(new PixelSource(pixels, PixelLayout.Rgb24), width, height, width * 3, sampling, tables, 4, 0,
                null, null, TestContext.Current.CancellationToken, allowPartial: true, cacheBudget: budget, pool: pool));
        }
        Assert.Equal(pool.Rented, pool.Returned);
        Assert.Equal(0, diagnostics.CurrentBytes);
        Assert.Equal(500000, budget.AvailableBytes);
    }

    private sealed class ObservedPool(bool fail) : ArrayPool<short>
    {
        internal int Rented { get; private set; }
        internal int Returned { get; private set; }

        public override short[] Rent(int minimumLength)
        {
            if (this.Rented == 1)
            {
                if (fail)
                {
                    throw new IOException("Injected coefficient rental failure.");
                }
                this.Rented++;
                return new short[500000];
            }
            this.Rented++;
            return new short[minimumLength];
        }

        public override void Return(short[] array, bool clearArray = false) => this.Returned++;
    }
}
