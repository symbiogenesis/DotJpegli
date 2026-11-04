namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ImageSpecificRateDistortionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(256)]
    public void ActualTableCostDynamicProgramMatchesIndependentExhaustiveOracle(double lambda)
    {
        long[] histogram = Enumerable.Range(0, 256).Select(index => (long)((index * 17 % 31) + 1)).ToArray();
        var coding = HuffmanTable.CreateOptimized(histogram);
        byte[] lengths = RateDistortionQuantization.CodeLengths(coding);
        int[] positions = [1, 2, 17, 34, 62, 63];
        short[] original = new short[64];
        short[] values = [3, -2, 1, -3, 2, -1];
        float[] source = new float[64];
        ushort[] table = Enumerable.Repeat((ushort)3, 64).ToArray();
        for (int index = 0; index < positions.Length; index++)
        {
            original[positions[index]] = values[index];
            source[JpegConstants.ZigZag[positions[index]]] = (values[index] - (.375f * Math.Sign(values[index]))) * 3;
        }
        original[0] = -117;
        short[] candidate = (short[])original.Clone(), actual = (short[])original.Clone();
        double best = double.PositiveInfinity;
        Enumerate(0);
        int evaluations = RateDistortionQuantization.Optimize(source, table, actual, lambda, lengths);
        Assert.InRange(Math.Abs(best - Objective(actual)), 0, 1e-8);
        Assert.Equal(original[0], actual[0]);
        Assert.InRange(evaluations, 63, 6048);
        Assert.Equal(IndependentBits(actual, lengths), RateDistortionQuantization.EstimateAcBits(actual, lengths));
        void Enumerate(int index)
        {
            if (index == positions.Length)
            {
                best = Math.Min(best, Objective(candidate));
                return;
            }
            int position = positions[index];
            short value = original[position];
            short[] choices = Math.Abs((int)value) == 1 ? [value, 0] : [value, (short)(value - Math.Sign(value)), 0];
            foreach (short choice in choices)
            {
                candidate[position] = choice;
                Enumerate(index + 1);
            }
        }
        double Objective(short[] block)
        {
            double error = 0;
            for (int position = 1; position < 64; position++)
            {
                double difference = source[JpegConstants.ZigZag[position]] - (block[position] * 3);
                error += difference * difference;
            }
            return error + (lambda * IndependentBits(block, lengths));
        }
    }

    [Fact]
    public void MissingEndOfBlockAndRunSymbolsAreForbiddenRatherThanFree()
    {
        long[] histogram = new long[256];
        histogram[0xf0] = 3;
        histogram[0xe1] = 1;
        byte[] lengths = RateDistortionQuantization.CodeLengths(HuffmanTable.CreateOptimized(histogram));
        float[] source = new float[64];
        short[] actual = new short[64];
        actual[0] = -117;
        actual[63] = 1;
        short[] original = (short[])actual.Clone();
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        _ = RateDistortionQuantization.Optimize(source, table, actual, 1000, lengths);
        Assert.Equal(original, actual);
        Assert.Equal(IndependentBits(actual, lengths), RateDistortionQuantization.EstimateAcBits(actual, lengths));
        lengths[0xf0] = 0;
        _ = Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(source, table, actual, 1, lengths));
        Assert.Equal(original, actual);
        _ = Assert.Throws<ArgumentException>(() => RateDistortionQuantization.Optimize(source, table, actual, 1, new byte[255]));
        Assert.Equal(original, actual);
    }

    private static int IndependentBits(short[] block, byte[] lengths)
    {
        int bits = 0, run = 0;
        for (int position = 1; position < 64; position++)
        {
            if (block[position] == 0)
            {
                run++;
                continue;
            }
            while (run >= 16)
            {
                bits += lengths[0xf0];
                run -= 16;
            }
            int category = 0;
            for (int magnitude = Math.Abs((int)block[position]); magnitude != 0; magnitude >>= 1)
            {
                category++;
            }
            bits += lengths[(run << 4) | category] + category;
            run = 0;
        }
        return bits + (run == 0 ? 0 : lengths[0]);
    }
}
