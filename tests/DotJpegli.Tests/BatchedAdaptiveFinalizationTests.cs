#if !DOTJPEGLI_NETSTANDARD_TESTS
namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class BatchedAdaptiveFinalizationTests
{
    [Fact]
    public void VectorFinalizationPreservesOriginalScalarBitsAcrossPolynomialIntervals()
    {
        var random = new Random(431705);
        float[] lower = new float[16], upper = new float[16], modulation = new float[16];
        foreach (int quant in new[] { 1, 8, 9, 10, 18, 64, 65, 255, 32767 })
        {
            float dampen = quant < 9 ? 1f : MathF.Max(0f, 1f - ((quant - 9f) / 56f));
            float mul = 0.841f * dampen;
            float add = (1f - dampen) * (0.48f * 0.841f);
            for (int batch = 0; batch < 128; batch++)
            {
                for (int index = 0; index < lower.Length; index++)
                {
                    lower[index] = (float)(random.NextDouble() * 32);
                    upper[index] = (float)(random.NextDouble() * 32);
                }
                for (int block = 0; block < 8; block++)
                {
                    modulation[block] = (float)((random.NextDouble() * 12) - 8);
                    // Neighbors of the mantissa-reduction boundary exercise
                    // both exponent branches of the original FastLog2.
                    modulation[8 + block] = MathF.ScaleB(BitConverter.Int32BitsToSingle(
                        0x3f2aaaab + ((batch + block) % 3) - 1), (batch % 25) - 12);
                }
                AssertOriginalBits(lower, upper, modulation, mul, add);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ExceptionalLanesAndExponentWraparoundPreserveOriginalScalarBits(int input)
    {
        float[] special =
        [
            0f, -0f, float.Epsilon, -float.Epsilon,
            BitConverter.Int32BitsToSingle(0x007fffff),
            float.MaxValue, -float.MaxValue,
            float.PositiveInfinity, float.NegativeInfinity,
            BitConverter.Int32BitsToSingle(0x7fc12345),
            BitConverter.Int32BitsToSingle(unchecked((int)0xffc54321)),
            -1024f, -128f, -126f, 127f, 128f, 1024f
        ];
        foreach (float value in special)
        {
            for (int lane = 0; lane < 8; lane++)
            {
                float[] lower = Enumerable.Repeat(0.5f, 16).ToArray();
                float[] upper = Enumerable.Repeat(0.25f, 16).ToArray();
                float[] modulation = new float[16];
                modulation.AsSpan(8).Fill(1f);
                switch (input)
                {
                    case 0:
                        lower[lane * 2] = value;
                        break;
                    case 1:
                        upper[(lane * 2) + 1] = value;
                        break;
                    case 2:
                        modulation[lane] = value;
                        break;
                    default:
                        modulation[8 + lane] = value;
                        break;
                }
                AssertOriginalBits(lower, upper, modulation, 0.841f, 0f);
            }
        }
    }

    [Fact]
    public void DampingAndStrengthClampingPreserveOriginalBits()
    {
        float[] lower = new float[16], upper = new float[16], modulation = new float[16];
        modulation.AsSpan(8).Fill(1f);
        foreach (float mul in new[] { 0f, -0f, 0.841f, float.Epsilon, float.MaxValue,
            float.PositiveInfinity, BitConverter.Int32BitsToSingle(0x7fc12345) })
        {
            foreach (float add in new[] { 0f, -0f, 0.6f, -0.6f, float.Epsilon, float.MaxValue,
                float.NegativeInfinity, BitConverter.Int32BitsToSingle(0x7fc54321) })
            {
                AssertOriginalBits(lower, upper, modulation, mul, add);
            }
        }
    }

    private static void AssertOriginalBits(float[] lower, float[] upper, float[] modulation, float mul, float add)
    {
        float[] expected = new float[9], actual = new float[9];
        expected[^1] = actual[^1] = -123.25f;
        AdaptiveQuantization.FinalizeEightBlocks(lower, upper, modulation, mul, add, expected, useVector: false);
        AdaptiveQuantization.FinalizeEightBlocks(lower, upper, modulation, mul, add, actual, useVector: true);
        Assert.Equal(expected.Select(BitConverter.SingleToInt32Bits), actual.Select(BitConverter.SingleToInt32Bits));
        Assert.Equal(-123.25f, actual[^1]);
    }
}
#endif
