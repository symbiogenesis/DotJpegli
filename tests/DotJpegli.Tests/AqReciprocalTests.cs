namespace DotJpegli.Tests;

using System.Numerics;
using DotJpegli.Internal;
using Xunit;

public sealed class AqReciprocalTests
{
    [Fact]
    public void EverySeedBinAndExponentPreservesDivisionBits()
    {
        int accepted = 0;
        for (int exponent = 1; exponent < 255; exponent++)
        {
            for (int bin = 0; bin < 256; bin++)
            {
                foreach (int offset in new[] { 0, 1, 16383, 32766, 32767 })
                {
                    float denominator = BitConverter.Int32BitsToSingle((exponent << 23) | (bin << 15) | offset);
                    foreach (float numerator in new[] { denominator, -denominator, 1f, -1.25f })
                    {
                        float actual = AqReciprocal.Divide(numerator, denominator, out bool fallback);
                        Assert.Equal(BitConverter.SingleToInt32Bits(numerator / denominator),
                            BitConverter.SingleToInt32Bits(actual));
                        if (!fallback)
                        {
                            accepted++;
                        }
                    }
                }
            }
        }
        Assert.True(accepted > 500000);
    }

    [Fact]
    public void FullBitOperandsAndAdjacentCandidatesHaveIndependentExactCertificates()
    {
        var random = new Random(64129);
        int accepted = 0;
        for (int sample = 0; sample < 16384; sample++)
        {
            float numerator = BitConverter.Int32BitsToSingle((int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1));
            float denominator = BitConverter.Int32BitsToSingle((int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1));
            float expected = numerator / denominator;
            float actual = AqReciprocal.Divide(numerator, denominator, out bool fallback);
            Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
            if (!fallback)
            {
                Assert.True(IndependentlyCertifies(numerator, denominator, actual));
                Assert.False(AqReciprocal.Certifies(numerator, denominator, -actual));
                accepted++;
            }
            foreach (float candidate in new[] { expected, MathF.BitDecrement(expected), MathF.BitIncrement(expected) })
            {
                Assert.Equal(IndependentlyCertifies(numerator, denominator, candidate),
                    AqReciprocal.Certifies(numerator, denominator, candidate));
            }
        }
        Assert.True(accepted > 4000);
    }

    [Fact]
    public void ExceptionalSignedZeroUnderflowOverflowAndMidpointCasesUseExactFallback()
    {
        float smallestNormal = BitConverter.Int32BitsToSingle(0x00800000);
        float[] special = [0f, -0f, float.Epsilon, -float.Epsilon, MathF.BitDecrement(smallestNormal),
            smallestNormal, -smallestNormal, float.MaxValue, -float.MaxValue, float.PositiveInfinity,
            float.NegativeInfinity, BitConverter.Int32BitsToSingle(0x7fc01234),
            BitConverter.Int32BitsToSingle(unchecked((int)0xffc05678))];
        foreach (float numerator in special)
        {
            foreach (float denominator in special)
            {
                float actual = AqReciprocal.Divide(numerator, denominator, out bool fallback);
                Assert.Equal(BitConverter.SingleToInt32Bits(numerator / denominator),
                    BitConverter.SingleToInt32Bits(actual));
                if (!fallback)
                {
                    Assert.True(IndependentlyCertifies(numerator, denominator, actual));
                }
            }
        }

        // Exact ties around both even and odd subnormal significands, and the
        // normal/subnormal boundary, must retain the runtime's ties-to-even.
        foreach (int numeratorBits in new[] { 1, 3, 5, 7, 0x00ffffff })
        {
            foreach (int sign in new[] { 0, int.MinValue })
            {
                float numerator = BitConverter.Int32BitsToSingle(numeratorBits | sign);
                float actual = AqReciprocal.Divide(numerator, 2f, out bool fallback);
                Assert.True(fallback);
                Assert.False(AqReciprocal.Certifies(numerator, 2f, actual));
                Assert.Equal(BitConverter.SingleToInt32Bits(numerator / 2f), BitConverter.SingleToInt32Bits(actual));
            }
        }

        AssertFallback(float.MaxValue, smallestNormal);
        AssertFallback(smallestNormal, float.MaxValue);
        AssertFallback(1f, -1f);
        AssertFallback(0f, 1f);
        AssertFallback(-0f, 1f);
        AssertFallback(float.MaxValue, 1f);
    }

    private static void AssertFallback(float numerator, float denominator)
    {
        float actual = AqReciprocal.Divide(numerator, denominator, out bool fallback);
        Assert.True(fallback);
        Assert.Equal(BitConverter.SingleToInt32Bits(numerator / denominator), BitConverter.SingleToInt32Bits(actual));
    }

    private static bool IndependentlyCertifies(float numerator, float denominator, float candidate)
    {
        int numeratorBits = BitConverter.SingleToInt32Bits(numerator);
        int candidateBits = BitConverter.SingleToInt32Bits(candidate);
        float magnitude = MathF.Abs(candidate);
        if (!float.IsFinite(numerator) || numerator == 0 || !float.IsNormal(denominator) || denominator <= 0
            || !float.IsNormal(candidate) || !float.IsFinite(MathF.BitIncrement(magnitude))
            || (numeratorBits < 0) != (candidateBits < 0))
        {
            return false;
        }

        (BigInteger n, BigInteger nDenominator) = Decode(MathF.Abs(numerator));
        (BigInteger d, BigInteger dDenominator) = Decode(denominator);
        (BigInteger q, BigInteger qDenominator) = Decode(magnitude);
        (BigInteger previous, BigInteger previousDenominator) = Decode(MathF.BitDecrement(magnitude));
        (BigInteger next, BigInteger nextDenominator) = Decode(MathF.BitIncrement(magnitude));
        BigInteger quotientNumerator = n * dDenominator;
        BigInteger quotientDenominator = nDenominator * d;
        BigInteger lowerNumerator = (previous * qDenominator) + (q * previousDenominator);
        BigInteger lowerDenominator = 2 * previousDenominator * qDenominator;
        BigInteger upperNumerator = (q * nextDenominator) + (next * qDenominator);
        BigInteger upperDenominator = 2 * qDenominator * nextDenominator;
        return quotientNumerator * lowerDenominator > lowerNumerator * quotientDenominator
            && quotientNumerator * upperDenominator < upperNumerator * quotientDenominator;
    }

    private static (BigInteger Numerator, BigInteger Denominator) Decode(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        int exponent = (bits >> 23) & 255;
        int significand = bits & 0x007fffff;
        if (exponent != 0)
        {
            significand |= 0x00800000;
        }
        int power = exponent == 0 ? -149 : exponent - 150;
        return power >= 0 ? (new BigInteger(significand) << power, BigInteger.One)
            : (new BigInteger(significand), BigInteger.One << -power);
    }
}
