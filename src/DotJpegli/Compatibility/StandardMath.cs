#if NETSTANDARD2_1
using System.Numerics;

namespace DotJpegli.Compatibility;

/// <summary>Single-rounding arithmetic for runtimes without fused multiply-add.</summary>
internal static class StandardMath
{
    // Halfway between float.MaxValue and the next value in an unbounded
    // single-precision format: 2^128 - 2^103. Rounding a tie overflows.
    private const double OverflowMidpoint = 340282356779733661637539395458142568448d;

    internal static float FusedMultiplyAdd(float x, float y, float z)
    {
        // Every product of two finite floats is exact in a double. TwoSum
        // then recovers the error in the double addition. That error matters
        // only when the double result lands on a float rounding midpoint.
        double product = (double)x * y;
        double sum = product + z;
        float rounded = (float)sum;
        if (double.IsNaN(sum) || double.IsInfinity(sum))
            return rounded;

        double virtualAddend = sum - product;
        double error = (product - (sum - virtualAddend)) + ((double)z - virtualAddend);
        if (error == 0)
            return rounded;

        if (float.IsInfinity(rounded))
        {
            if (Math.Abs(sum) == OverflowMidpoint && (sum > 0 ? error < 0 : error > 0))
                return sum > 0 ? float.MaxValue : -float.MaxValue;
            return rounded;
        }

        double roundedValue = rounded;
        if (sum == roundedValue)
            return rounded;

        bool aboveRounded = sum > roundedValue;
        float adjacent = Adjacent(rounded, aboveRounded);
        double midpoint = (roundedValue + adjacent) * 0.5;
        if (sum != midpoint)
            return rounded;

        return (error > 0) == aboveRounded ? adjacent : rounded;
    }

    internal static Vector<float> FusedMultiplyAdd(Vector<float> x, Vector<float> y, Vector<float> z)
    {
        Span<float> result = stackalloc float[Vector<float>.Count];
        for (int lane = 0; lane < result.Length; lane++)
            result[lane] = FusedMultiplyAdd(x[lane], y[lane], z[lane]);
        return new Vector<float>(result);
    }

    private static float Adjacent(float value, bool upward)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        if ((bits & int.MaxValue) == 0)
            return BitConverter.Int32BitsToSingle(upward ? 1 : unchecked((int)0x80000001));
        bits += (value > 0) == upward ? 1 : -1;
        return BitConverter.Int32BitsToSingle(bits);
    }
}
#endif
