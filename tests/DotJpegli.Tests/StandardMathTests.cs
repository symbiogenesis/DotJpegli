#if DOTJPEGLI_NETSTANDARD_TESTS
using System.Numerics;
using DotJpegli.Compatibility;
using Xunit;

namespace DotJpegli.Tests;

public sealed class StandardMathTests
{
    [Fact]
    public void FusedMultiplyAddCorrectsDoubleRoundingAtFloatMidpoints()
    {
        float first = 1f + MathF.ScaleB(1f, -12);
        float third = 1f + MathF.ScaleB(3f, -12);
        AssertSame(first, first, float.Epsilon);
        AssertSame(first, first, -float.Epsilon);
        AssertSame(first, third, float.Epsilon);
        AssertSame(first, third, -float.Epsilon);
        AssertSame(-first, first, float.Epsilon);
        AssertSame(-first, first, -float.Epsilon);

        // 31 * 601 * 1801 == 2^25 - 1: this product is the
        // overflow midpoint, with an arbitrarily small residual.
        float overflowX = MathF.ScaleB(601f * 1801f, 64);
        float overflowY = MathF.ScaleB(31f, 39);
        AssertSame(overflowX, overflowY, -float.Epsilon);
        AssertSame(overflowX, overflowY, float.Epsilon);
        AssertSame(-overflowX, overflowY, float.Epsilon);
        AssertSame(-overflowX, overflowY, -float.Epsilon);
    }

    [Fact]
    public void FusedMultiplyAddPreservesExceptionalValuesAndSignedZero()
    {
        float[] values = [0f, -0f, float.Epsilon, -float.Epsilon, float.MaxValue,
            -float.MaxValue, 1f, -1f, float.PositiveInfinity, float.NegativeInfinity, float.NaN];
        foreach (float x in values)
        foreach (float y in values)
        foreach (float z in values)
            AssertSame(x, y, z);
    }

    [Fact]
    public void FusedMultiplyAddMatchesRuntimeAcrossFiniteBitsAndCancellation()
    {
        uint state = 0x91e10da5;
        for (int index = 0; index < 200_000; index++)
        {
            float x = NextFinite(ref state);
            float y = NextFinite(ref state);
            AssertSame(x, y, NextFinite(ref state));

            float product = x * y;
            if (float.IsFinite(product))
            {
                AssertSame(x, y, -product);
                AssertSame(x, y, -MathF.BitIncrement(product));
                AssertSame(x, y, -MathF.BitDecrement(product));
            }
        }
    }

    [Fact]
    public void VectorFusedMultiplyAddMatchesRuntimeInEveryLane()
    {
        uint state = 0x31415926;
        float[] x = new float[Vector<float>.Count];
        float[] y = new float[x.Length];
        float[] z = new float[x.Length];
        for (int iteration = 0; iteration < 4096; iteration++)
        {
            for (int lane = 0; lane < x.Length; lane++)
            {
                x[lane] = NextFinite(ref state);
                y[lane] = NextFinite(ref state);
                z[lane] = NextFinite(ref state);
            }

            Vector<float> actual = StandardMath.FusedMultiplyAdd(new Vector<float>(x), new Vector<float>(y), new Vector<float>(z));
            for (int lane = 0; lane < x.Length; lane++)
                Assert.Equal(BitConverter.SingleToInt32Bits(MathF.FusedMultiplyAdd(x[lane], y[lane], z[lane])),
                    BitConverter.SingleToInt32Bits(actual[lane]));
        }
    }

    private static float NextFinite(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        uint bits = state;
        if ((bits & 0x7f800000) == 0x7f800000)
            bits ^= 0x00800000;
        return BitConverter.Int32BitsToSingle(unchecked((int)bits));
    }

    private static void AssertSame(float x, float y, float z)
    {
        float expected = MathF.FusedMultiplyAdd(x, y, z);
        float actual = StandardMath.FusedMultiplyAdd(x, y, z);
        if (float.IsNaN(expected))
            Assert.True(float.IsNaN(actual));
        else
            Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
    }
}
#endif
