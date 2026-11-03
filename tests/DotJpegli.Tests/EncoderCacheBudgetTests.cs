namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class EncoderCacheBudgetTests
{
    [Fact]
    public void AllCacheKindsShareOneLimitAndRefundCapacity()
    {
        var diagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(1024, diagnostics);
        Assert.True(budget.TryReserve(256, EncoderCacheKind.ConvertedSamples));
        Assert.True(budget.TryReserve(512, EncoderCacheKind.QuantizedCoefficients));
        var tokens = new EntropyTokenBudget(cacheBudget: budget);
        Assert.False(tokens.TryReserve(257));
        Assert.True(tokens.TryReserve(256));
        Assert.Equal(1024, diagnostics.PeakBytes);
        Assert.Equal(256, diagnostics.ConvertedSamplePeakBytes);
        Assert.Equal(512, diagnostics.CoefficientPeakBytes);
        Assert.Equal(256, diagnostics.EntropyTokenPeakBytes);
        Assert.Equal(0, budget.AvailableBytes);
        tokens.Release(256);
        budget.Release(512, EncoderCacheKind.QuantizedCoefficients);
        budget.Release(256, EncoderCacheKind.ConvertedSamples);
        Assert.Equal(0, diagnostics.CurrentBytes);
        Assert.Equal(1024, budget.AvailableBytes);
    }

    [Fact]
    public void NegativeLimitIsRejectedBeforeOutput()
    {
        using var output = new MemoryStream();
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(output, new byte[3], 1, 1,
                options: new JpegliEncoderOptions { MaxCacheBytes = -1 },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.MaxCacheBytes), error.ParamName);
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void RawOnlyScanCanSkipCollectionWhenNoTokenRentalFits()
    {
        var budget = new EncoderCacheBudget(64);
        var sharedTokens = new EntropyTokenBudget(cacheBudget: budget);
        using var first = new EntropyTokens(sharedTokens);
        using var refinement = new EntropyTokens(sharedTokens);
        Assert.True(first.TryBeginRecording());
        first.AddBits(1, 1);
        Assert.Equal(0, budget.AvailableBytes);
        Assert.False(refinement.TryBeginRecording());
        Assert.False(refinement.IsComplete);
        Assert.True(first.IsComplete);
        first.Dispose();
        Assert.Equal(64, budget.AvailableBytes);
    }
}
