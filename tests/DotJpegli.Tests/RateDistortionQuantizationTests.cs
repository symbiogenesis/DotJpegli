namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class RateDistortionQuantizationTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, .5)]
    [InlineData(false, 16)]
    [InlineData(true, 0)]
    [InlineData(true, .5)]
    [InlineData(true, 16)]
    public void DynamicProgramMatchesExhaustiveSearchIncludingLongRunsAndFinalCoefficient(bool chroma, double lambda)
    {
        int[] positions = [1, 2, 17, 34, 62, 63];
        short[] original = new short[64];
        short[] amplitudes = [3, -2, 1, -3, 2, -1];
        float[] natural = new float[64];
        ushort[] table = Enumerable.Range(0, 64).Select(index => (ushort)(1 + (index % 7))).ToArray();
        for (int index = 0; index < positions.Length; index++)
        {
            int position = positions[index], frequency = JpegConstants.ZigZag[position];
            original[position] = amplitudes[index];
            natural[frequency] = (amplitudes[index] - (.375f * Math.Sign(amplitudes[index]))) * table[frequency];
        }
        original[0] = -117;
        double best = double.PositiveInfinity;
        short[] candidate = (short[])original.Clone();
        Enumerate(0);
        short[] actual = (short[])original.Clone();
        int evaluations = RateDistortionQuantization.Optimize(natural, table, actual, lambda, chroma);
        double actualCost = Objective(natural, table, actual, lambda, chroma);
        Assert.InRange(Math.Abs(best - actualCost), 0, 1e-8);
        Assert.Equal(original[0], actual[0]);
        Assert.InRange(evaluations, 63, 6048);
        for (int position = 1; position < 64; position++)
        {
            Assert.Contains(actual[position], Choices(original[position]));
        }

        void Enumerate(int index)
        {
            if (index == positions.Length)
            {
                best = Math.Min(best, Objective(natural, table, candidate, lambda, chroma));
                return;
            }
            int position = positions[index];
            foreach (short value in Choices(original[position]))
            {
                candidate[position] = value;
                Enumerate(index + 1);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RateEstimateMatchesIndependentEntropyHistogramAcrossCategoriesAndRuns(bool chroma)
    {
        short[] block = new short[64];
        Assert.Equal(IndependentBits(block, chroma), RateDistortionQuantization.EstimateAcBits(block, chroma));
        foreach (int position in new[] { 1, 16, 17, 32, 33, 48, 49, 62, 63 })
        {
            foreach (short value in new short[] { -1023, -512, -3, -1, 1, 2, 511, 1023 })
            {
                Array.Clear(block);
                block[position] = value;
                Assert.Equal(IndependentBits(block, chroma), RateDistortionQuantization.EstimateAcBits(block, chroma));
            }
        }
        var random = new Random(5173);
        for (int iteration = 0; iteration < 32; iteration++)
        {
            for (int position = 1; position < 64; position++)
            {
                block[position] = random.Next(4) == 0 ? (short)random.Next(-1023, 1024) : (short)0;
            }
            Assert.Equal(IndependentBits(block, chroma), RateDistortionQuantization.EstimateAcBits(block, chroma));
        }
    }

    [Fact]
    public void EqualObjectiveKeepsOriginalAndOptimizationPreservesDcAndSuffix()
    {
        float[] natural = new float[72];
        natural[JpegConstants.ZigZag[1]] = .5f;
        ushort[] table = Enumerable.Repeat((ushort)1, 72).ToArray();
        short[] original = new short[72];
        original[0] = short.MinValue;
        original[1] = 1;
        Array.Fill(original, (short)-23456, 64, 8);
        short[] actual = (short[])original.Clone();
        int firstCount = RateDistortionQuantization.Optimize(natural, table, actual, 0, chroma: false);
        Assert.Equal(original, actual);
        int secondCount = RateDistortionQuantization.Optimize(natural, table, actual, 0, chroma: false);
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(original, actual);
    }

    [Fact]
    public void FiniteExtremeInputsAndLargestLambdaKeepBoundedSearchAndFiniteDecisions()
    {
        float[] natural = Enumerable.Repeat(float.MaxValue, 64).ToArray();
        ushort[] table = Enumerable.Repeat((ushort)255, 64).ToArray();
        short[] actual = Enumerable.Repeat((short)1023, 64).ToArray();
        int evaluations = RateDistortionQuantization.Optimize(natural, table, actual, double.MaxValue, chroma: false);
        Assert.Equal((short)1023, actual[0]);
        Assert.All(actual.Skip(1), value => Assert.Equal((short)0, value));
        Assert.InRange(evaluations, 63, 6048);
    }

    [Fact]
    public void AllZeroBlockHasOneCandidatePerPositionAndPreservesOutput()
    {
        float[] natural = new float[64];
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        short[] block = new short[64];
        block[0] = 125;
        Assert.Equal(63, RateDistortionQuantization.Optimize(natural, table, block, 3, chroma: true));
        Assert.Equal((short)125, block[0]);
        Assert.All(block.Skip(1), value => Assert.Equal((short)0, value));
    }

    [Fact]
    public void ValidationRejectsInvalidInputBeforeChangingAnyCoefficient()
    {
        float[] natural = new float[64];
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        short[] block = Enumerable.Repeat((short)3, 72).ToArray();
        short[] original = (short[])block.Clone();
        foreach (double lambda in new[] { -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RateDistortionQuantization.Optimize(natural, table, block, lambda, false));
            Assert.Equal(original, block);
        }
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            natural[63] = invalid;
            Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural, table, block, 1, false));
            Assert.Equal(original, block);
        }
        natural[63] = 0;
        foreach (ushort invalid in new ushort[] { 0, 256, ushort.MaxValue })
        {
            table[63] = invalid;
            Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural, table, block, 1, false));
            Assert.Equal(original, block);
        }
        table[63] = 1;
        foreach (short invalid in new short[] { -1024, 1024, short.MinValue, short.MaxValue })
        {
            block[63] = invalid;
            short[] before = (short[])block.Clone();
            Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural, table, block, 1, false));
            Assert.Throws<ArgumentException>(() => RateDistortionQuantization.EstimateAcBits(block, false));
            Assert.Equal(before, block);
        }
        block[63] = 3;
        Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural.AsSpan(0, 63), table, block, 1, false));
        Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural, table.AsSpan(0, 63), block, 1, false));
        Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(natural, table, block.AsSpan(0, 63), 1, false));
        Assert.Throws<ArgumentException>(() => RateDistortionQuantization.EstimateAcBits(block.AsSpan(0, 63), false));
        Assert.Equal(original, block);
    }

    private static short[] Choices(short original) => original == 0 ? [0]
        : Math.Abs((int)original) == 1 ? [original, 0]
        : [original, (short)(original - Math.Sign(original)), 0];

    private static double Objective(float[] natural, ushort[] table, short[] block, double lambda, bool chroma)
    {
        double distortion = 0;
        for (int position = 1; position < 64; position++)
        {
            int frequency = JpegConstants.ZigZag[position];
            double error = natural[frequency] - (block[position] * table[frequency]);
            distortion += error * error;
        }
        return distortion + (lambda * IndependentBits(block, chroma));
    }

    private static int IndependentBits(short[] zigzag, bool chroma)
    {
        short[] natural = new short[64];
        for (int position = 0; position < 64; position++)
        {
            natural[JpegConstants.ZigZag[position]] = zigzag[position];
        }
        long[] dc = new long[256], ac = new long[256];
        int previousDc = natural[0];
        EntropyEncoder.AccumulateFrequencies(natural, ref previousDc, dc, ac);
        var standard = HuffmanTable.CreateDefault(isAc: true, chroma);
        int bits = (int)(standard.EstimateCost(ac) - standard.EstimateCost(new long[256]));
        for (int symbol = 0; symbol < 256; symbol++)
        {
            bits += (int)ac[symbol] * (symbol & 15);
        }
        return bits;
    }
}
