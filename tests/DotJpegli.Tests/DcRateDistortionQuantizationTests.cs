namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class DcRateDistortionQuantizationTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, .5)]
    [InlineData(false, 16)]
    [InlineData(true, 0)]
    [InlineData(true, .5)]
    [InlineData(true, 16)]
    public void ChainOptimumMatchesIndependentExhaustiveOracleAcrossRestarts(bool chroma, double lambda)
    {
        short[] original = [-1024, -3, 0, 1, 7, 1023];
        float[] source = original.Select((value, index) => (value * 3) + (index % 2 == 0 ? 1.125f : -1.125f)).ToArray();
        bool[] resets = [false, false, true, false, false, true];
        byte[] lengths = IndependentLengths(chroma);
        Assert.Equal(lengths, DcRateDistortionQuantization.StandardCodeLengths(chroma).ToArray());
        short[] candidate = (short[])original.Clone(), selected = (short[])original.Clone();
        double best = double.PositiveInfinity;
        int bestChanges = int.MaxValue;
        Enumerate(0);
        DcRateDistortionQuantization.Diagnostics diagnostics = DcRateDistortionQuantization.Optimize(
            source, 3, selected, lengths, resets, lambda, TestContext.Current.CancellationToken);
        double actual = Objective(selected);
        Assert.InRange(Math.Abs(actual - best), 0, 1e-8);
        Assert.Equal(bestChanges, diagnostics.ChangedCoefficients);
        Assert.Equal(IndependentBits(original, lengths, resets), diagnostics.OriginalBits);
        Assert.Equal(IndependentBits(selected, lengths, resets), diagnostics.SelectedBits);
        Assert.Equal(Error(original), diagnostics.OriginalSquaredError);
        Assert.Equal(Error(selected), diagnostics.SelectedSquaredError);
        Assert.InRange(diagnostics.Evaluations, original.Length, (9 * original.Length) - 6);
        for (int block = 0; block < original.Length; block++)
        {
            Assert.Contains(selected[block], Choices(original[block]));
        }

        void Enumerate(int block)
        {
            if (block == original.Length)
            {
                double cost = Objective(candidate);
                int changes = candidate.Where((value, index) => value != original[index]).Count();
                if (cost < best || (cost == best && changes < bestChanges))
                {
                    best = cost;
                    bestChanges = changes;
                }
                return;
            }
            foreach (short value in Choices(original[block]))
            {
                candidate[block] = value;
                Enumerate(block + 1);
            }
        }
        double Error(short[] values) => values.Select((value, index) => Math.Pow(source[index] - ((double)value * 3), 2)).Sum();
        double Objective(short[] values) => Error(values) + (lambda * IndependentBits(values, lengths, resets));
    }

    [Fact]
    public void EqualObjectiveRetainsOriginalAndMissingCodesStayUnavailable()
    {
        float[] source = [.5f, .5f, .5f];
        short[] original = [1, 1, 1];
        bool[] resets = [false, true, false];
        short[] actual = (short[])original.Clone();
        DcRateDistortionQuantization.Diagnostics result = DcRateDistortionQuantization.Optimize(
            source, 1, actual, IndependentLengths(false), resets, 0, TestContext.Current.CancellationToken);
        Assert.Equal(original, actual);
        Assert.Equal(0, result.ChangedCoefficients);
        byte[] onlyZero = new byte[12];
        onlyZero[0] = 2;
        actual = [0, 0, 0];
        result = DcRateDistortionQuantization.Optimize(source, 1, actual, onlyZero, resets, 1,
            TestContext.Current.CancellationToken);
        Assert.Equal(new short[3], actual);
        Assert.Equal(3, result.Evaluations);
    }

    [Fact]
    public void RateEstimateMatchesIndependentCategoryArithmeticIncludingPredictorResets()
    {
        short[] dc = [-1024, 1023, -512, -3, 0, 1, 2, 7, 511, 1023];
        foreach (bool chroma in new[] { false, true })
        {
            byte[] lengths = IndependentLengths(chroma);
            foreach (int interval in new[] { 0, 1, 2, 7 })
            {
                bool[] resets = Enumerable.Range(0, dc.Length).Select(index => interval != 0 && index % interval == 0).ToArray();
                Assert.Equal(IndependentBits(dc, lengths, resets), DcRateDistortionQuantization.EstimateBits(dc, lengths, resets));
            }
        }
    }

    [Fact]
    public void InvalidInputsAndCancellationAreRejectedBeforeChangingTheOutput()
    {
        float[] source = [0, 1];
        short[] dc = [0, 1];
        bool[] resets = [false, false];
        byte[] lengths = IndependentLengths(false);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => DcRateDistortionQuantization.Optimize(source, 0, dc, lengths, resets, 1, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => DcRateDistortionQuantization.Optimize(source, 1, dc, lengths, resets, double.NaN, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentException>(() => DcRateDistortionQuantization.Optimize([0, float.PositiveInfinity], 1, dc, lengths, resets, 1, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentException>(() => DcRateDistortionQuantization.Optimize([0], 1, dc, lengths, resets, 1, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentException>(() => DcRateDistortionQuantization.Optimize(source, 1, dc, lengths, [], 1, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentException>(() => DcRateDistortionQuantization.Optimize(source, 1, dc, new byte[12], resets, 1, TestContext.Current.CancellationToken));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        _ = Assert.Throws<OperationCanceledException>(() => DcRateDistortionQuantization.Optimize(source, 1, dc, lengths, resets, 1, cancelled.Token));
        Assert.Equal(new short[] { 0, 1 }, dc);
        short[] tooLong = new short[DcRateDistortionQuantization.MaximumBlocks + 1];
        _ = Assert.Throws<ArgumentException>(() => DcRateDistortionQuantization.Optimize(new float[tooLong.Length], 1,
            tooLong, lengths, new bool[tooLong.Length], 1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void LargestFiniteValuesAndLambdaKeepTheMaximumSearchBoundedAndFinite()
    {
        float[] source = Enumerable.Repeat(float.MaxValue, DcRateDistortionQuantization.MaximumBlocks).ToArray();
        short[] dc = new short[source.Length];
        DcRateDistortionQuantization.Diagnostics result = DcRateDistortionQuantization.Optimize(source, 255, dc,
            IndependentLengths(false), new bool[source.Length], double.MaxValue, TestContext.Current.CancellationToken);
        Assert.InRange(result.Evaluations, source.Length, (9 * source.Length) - 6);
        Assert.Equal(0, result.ChangedCoefficients);
        Assert.True(double.IsFinite(result.SelectedSquaredError));
        Assert.Equal(result.OriginalSquaredError, result.SelectedSquaredError);
    }

    private static short[] Choices(short original)
        => Enumerable.Range(original - 1, 3).Where(value => value is >= -1024 and <= 1023)
            .Select(value => (short)value).ToArray();

    // Annex K DC lengths transcribed independently of the encoder's table builder.
    private static byte[] IndependentLengths(bool chroma)
        => chroma ? [2, 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] : [2, 3, 3, 3, 3, 3, 4, 5, 6, 7, 8, 9];

    private static int IndependentBits(short[] dc, byte[] lengths, bool[] resets)
    {
        int bits = 0, predictor = 0;
        for (int block = 0; block < dc.Length; block++)
        {
            if (resets[block])
            {
                predictor = 0;
            }
            int category = 0;
            for (int magnitude = Math.Abs(dc[block] - predictor); magnitude != 0; magnitude >>= 1)
            {
                category++;
            }
            bits += lengths[category] + category;
            predictor = dc[block];
        }
        return bits;
    }
}
