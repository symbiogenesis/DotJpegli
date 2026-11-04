namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class JointCachePlanningTests
{
    [Fact]
    public void PlanningWithholdsCapacityWithoutChargingAnAllocation()
    {
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(1024, diagnostics);
        budget.Withhold(512);
        Assert.Equal(512, budget.AvailableBytes);
        Assert.False(budget.TryReserve(513, EncoderCacheKind.ConvertedSamples));
        Assert.True(budget.TryReserve(256, EncoderCacheKind.ConvertedSamples));
        Assert.Equal(256, diagnostics.PeakBytes);
        budget.Withhold(0);
        Assert.Equal(768, budget.AvailableBytes);
        budget.Release(256, EncoderCacheKind.ConvertedSamples);
        Assert.Equal(0, diagnostics.CurrentBytes);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => budget.Withhold(1025));
    }

    [Fact]
    public void ConstrainedProgressiveEncodingRetainsCompleteCoefficientsAndExactOutput()
    {
        const int width = 256, height = 256;
        const long limit = 1024 * 1024;
        byte[] pixels = TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions
        {
            ProgressiveLevel = 2,
            Subsampling = ChromaSubsampling.None,
            MaxCacheBytes = 0
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        var diagnostics = new EncoderCacheDiagnostics();
        byte[] actual = JpegliEncoder.Encode(pixels, width, height,
            options: options with { MaxCacheBytes = limit, CacheDiagnostics = diagnostics },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
        Assert.Equal(3 * width * height * sizeof(short), diagnostics.CoefficientPeakBytes);
        Assert.InRange(diagnostics.PeakBytes, 1, limit);
        Assert.Equal(0, diagnostics.CurrentBytes);
    }

    [Fact]
    public void NormalBudgetAndInsufficientCapacityKeepExistingAdmission()
    {
        JpegSamplingFactors sampling = new(new(1, 1), new(1, 1), new(1, 1));
        Assert.Equal(0, QuantizedImage.PlanFullCacheReservation(256, 256, sampling, 3,
            EncoderCacheBudget.DefaultMaximumBytes));
        Assert.Equal(0, QuantizedImage.PlanFullCacheReservation(256, 256, sampling, 3, 1024));
        Assert.Equal(393216, QuantizedImage.PlanFullCacheReservation(256, 256, sampling, 3, 1048576));
    }
}
