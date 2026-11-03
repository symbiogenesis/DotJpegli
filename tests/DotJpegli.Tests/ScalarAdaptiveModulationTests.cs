namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ScalarAdaptiveModulationTests
{
    [Fact]
    public void FusedTraversalPreservesOriginalTwoPassBits()
    {
        foreach (float[] block in SampleBlocks())
        {
            foreach (int stride in new[] { 8, 11, 127 })
            {
                float[] samples = Enumerable.Repeat(float.NaN, stride * 8).ToArray();
                for (int row = 0; row < 8; row++)
                {
                    block.AsSpan(row * 8, 8).CopyTo(samples.AsSpan(row * stride));
                }

                foreach (float value in new[] { -2.125f, 0f, 3.75f })
                {
                    float expected = ModulateOriginal(samples, stride, value);
                    float actual = AdaptiveQuantization.ModulateScalar(samples, stride, value);
                    Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
                }
            }
        }
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(31)]
    [InlineData(128)]
    public void MinimalStridedSlicePreservesOriginalTwoPassBits(int stride)
    {
        // End the slice at the final sample, so a neighbor read past the last
        // column or row cannot silently consume padding outside the block.
        float[] storage = Enumerable.Repeat(float.NaN, (stride * 7) + 13).ToArray();
        Span<float> samples = storage.AsSpan(5);
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                samples[(row * stride) + column] = ((row * 17) + (column * 31)) % 256;
            }
        }

        const float value = -0.125f;
        float expected = ModulateOriginal(samples, stride, value);
        float actual = AdaptiveQuantization.ModulateScalar(samples, stride, value);
        Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
    }

    private static IEnumerable<float[]> SampleBlocks()
    {
        yield return new float[64];
        yield return Enumerable.Range(0, 64).Select(i => i % 2 == 0 ? -0f : 0f).ToArray();
        yield return Enumerable.Repeat(128.125f, 64).ToArray();
        foreach (float impulse in new[]
        {
            255f, -89.125f, float.Epsilon, -float.Epsilon, float.MaxValue,
            float.PositiveInfinity, float.NegativeInfinity,
            BitConverter.Int32BitsToSingle(0x7FC12345)
        })
        {
            for (int position = 0; position < 64; position++)
            {
                float[] samples = new float[64];
                samples[position] = impulse;
                yield return samples;
            }
        }

        var random = new Random(4157);
        for (int block = 0; block < 64; block++)
        {
            float[] samples = new float[64];
            for (int index = 0; index < samples.Length; index++)
            {
                samples[index] = (block % 3) switch
                {
                    0 => (float)(random.NextDouble() * 255),
                    1 => 128f + (float)(random.NextDouble() * 0.001),
                    _ => MathF.ScaleB((float)((random.NextDouble() * 2) - 1), random.Next(-149, 121))
                };
            }

            yield return samples;
        }
    }

    // Keep the previous separate traversals as a bitwise oracle, including the
    // existing approximations. A mathematical logarithm would change rounding.
    private static float ModulateOriginal(ReadOnlySpan<float> samples, int stride, float value)
    {
        const float inputScaling = 1f / 255f;
        const float invLog2E = 0.6931471805599453f;
        float sum = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = dy * stride;
            int next = dy == 7 ? row : row + stride;
            for (int dx = 0; dx < 8; dx++)
            {
                if (dx != 7)
                {
                    sum += MathF.Abs(samples[row + dx] - samples[row + dx + 1]);
                }

                sum += MathF.Abs(samples[row + dx] - samples[next + dx]);
            }
        }

        const float sumCoeff = (float)(-2.0052193233688884f * inputScaling / 112.0);
        value = (sum * sumCoeff) + value;
        float ratio = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = dy * stride;
            for (int dx = 0; dx < 8; dx++)
            {
                ratio += DerivativeRatio(samples[row + dx] + (0.16f / inputScaling));
            }
        }

        ratio *= inputScaling / 64f;
        return (-0.15526878023684174f * invLog2E * FastLog2(ratio)) + value;
    }

    private static float DerivativeRatio(float value)
    {
        const float inputScaling = 1f / 255f;
        const float invLog2E = 0.6931471805599453f;
        const float sgMul = 226.0480446705883f;
        const float sgRetMul = 1f / 73.377132366608819f * 18.6580932135f * invLog2E;
        const float epsilon = 1e-2f;
        const float numOffset = epsilon / inputScaling / inputScaling;
        const float numMul = sgRetMul * 3 * sgMul;
        const float vOffset = ((7.14672470003f * invLog2E) + epsilon) / inputScaling;
        const float denMul = invLog2E * sgMul * inputScaling * inputScaling;
        value = MathF.Max(value, 0);
        float squared = value * value;
        float numerator = (numMul * squared) + numOffset;
        float denominator = (denMul * value * squared) + vOffset;
        return numerator / denominator;
    }

    private static float FastLog2(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        int exponent = unchecked(bits - 0x3f2aaaab) >> 23;
        float mantissa = BitConverter.Int32BitsToSingle(unchecked(bits - (exponent << 23))) - 1f;
        float numerator = (((0.74245873327820566f * mantissa) + 1.4287160470083755f)
            * mantissa) - 1.8503833400518310e-6f;
        float denominator = (((0.17409343003366853f * mantissa) + 1.0096718572241148f)
            * mantissa) + 0.99032814277590719f;
        return (numerator / denominator) + exponent;
    }
}
