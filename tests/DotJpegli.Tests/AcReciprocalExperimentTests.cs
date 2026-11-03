namespace DotJpegli.Tests;

using System.Numerics;
using DotJpegli.Internal;
using DotJpegli.Performance;
using Xunit;

public sealed class AcReciprocalExperimentTests
{
    [Fact]
    public void EveryDivisorHasCertifiedAdjacentReciprocalEndpoints()
    {
        for (int divisor = 1; divisor <= 32767; divisor++)
        {
            var interval = AcReciprocalExperiment.Interval.Create(divisor);
            Assert.True((double)interval.Lower * divisor <= 1);
            Assert.True((double)interval.Upper * divisor >= 1);
            Assert.True(interval.Lower == interval.Upper || MathF.BitIncrement(interval.Lower) == interval.Upper);
            if (BitOperations.IsPow2(divisor))
            {
                Assert.Equal(interval.Lower, interval.Upper);
            }
        }
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => AcReciprocalExperiment.Interval.Create(0));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => AcReciprocalExperiment.Interval.Create(32768));
    }

    [Fact]
    public void EveryDivisorEnclosesRoundedDivisionAndPreservesAdversarialAcResults()
    {
        foreach (int divisor in Enumerable.Range(1, 32767))
        {
            var interval = AcReciprocalExperiment.Interval.Create(divisor);
            foreach (float center in new[] { 0.59082f * divisor, 0.5f * divisor, 1.5f * divisor, 1023.5f * divisor })
            {
                foreach (float magnitude in new[] { MathF.BitDecrement(center), center, MathF.BitIncrement(center) })
                {
                    foreach (float coefficient in new[] { magnitude, -magnitude })
                    {
                        float quotient = coefficient / divisor;
                        float first = coefficient * interval.Lower, second = coefficient * interval.Upper;
                        Assert.InRange(quotient, Math.Min(first, second), Math.Max(first, second));
                        foreach (float threshold in new[] { 0f, 0.59082f, MathF.Abs(quotient), float.PositiveInfinity })
                        {
                            Assert.Equal(AcReciprocalExperiment.Exact(coefficient, divisor, threshold, requireEightBit: false),
                                AcReciprocalExperiment.Guarded(coefficient, divisor, interval, threshold, requireEightBit: false));
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void BitPatternSamplesPreserveSubnormalExceptionalAndSaturatedResults()
    {
        var random = new Random(7139);
        float minNormal = BitConverter.Int32BitsToSingle(0x00800000);
        float[] special = [0, -0f, float.Epsilon, -float.Epsilon, MathF.BitDecrement(minNormal),
            minNormal, MathF.BitIncrement(minNormal), float.MaxValue, -float.MaxValue,
            float.NaN, float.PositiveInfinity, float.NegativeInfinity];
        for (int sample = 0; sample < 65536; sample++)
        {
            int divisor = random.Next(1, 32768);
            var interval = AcReciprocalExperiment.Interval.Create(divisor);
            float coefficient = sample < special.Length ? special[sample]
                : BitConverter.Int32BitsToSingle((int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1));
            float threshold = sample % 4 == 0 ? 0 : sample % 4 == 1 ? 0.59082f
                : sample % 4 == 2 ? MathF.Abs(coefficient / divisor) : float.PositiveInfinity;
            if (!float.IsFinite(coefficient))
            {
                ArgumentException expected = Assert.Throws<ArgumentException>(() => AcReciprocalExperiment.Exact(coefficient, divisor, threshold));
                ArgumentException actual = Assert.Throws<ArgumentException>(() => AcReciprocalExperiment.Guarded(coefficient, divisor, interval, threshold));
                Assert.Equal(expected.Message, actual.Message);
            }
            else
            {
                Assert.Equal(AcReciprocalExperiment.Exact(coefficient, divisor, threshold, requireEightBit: false),
                    AcReciprocalExperiment.Guarded(coefficient, divisor, interval, threshold, requireEightBit: false));
            }
        }
    }

    [Fact]
    public void GuardedScalarPreservesExistingQuantizerValidationAndWrittenPrefixes()
    {
        foreach (int divisor in new[] { 1, 3, 7, 255, 32767 })
        {
            ushort[] table = Enumerable.Repeat((ushort)divisor, 64).ToArray();
            var reciprocal = AcReciprocalExperiment.Interval.Create(divisor);
            foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 1023.5f * divisor, -1023.5f * divisor })
            {
                float[] coefficients = Enumerable.Repeat(1.25f * divisor, 64).ToArray();
                coefficients[0] = 0;
                coefficients[19] = bad;
                short[] expected = Enumerable.Repeat((short)-12345, 64).ToArray();
                short[] actual = (short[])expected.Clone();
                Exception? expectedError = Record.Exception(() => Quantization.QuantizeScalar(coefficients, table, expected, 0, 1,
                    requireEightBitCoefficients: true));
                Exception? actualError = Record.Exception(() =>
                {
                    actual[0] = 0;
                    for (int i = 1; i < coefficients.Length; i++)
                    {
                        actual[i] = AcReciprocalExperiment.Guarded(coefficients[i], divisor, reciprocal, 0.59082f);
                    }
                });
                Assert.Equal(expectedError?.GetType(), actualError?.GetType());
                Assert.Equal(expectedError?.Message, actualError?.Message);
                Assert.Equal(expected, actual);
            }
        }
    }

    [Fact]
    public void PortableVectorGuardAgreesWithIndependentScalarAcrossAmbiguousBatches()
    {
        int count = Vector<float>.Count * 1024;
        var random = new Random(9123);
        float[] coefficients = new float[count], divisors = new float[count], lower = new float[count], upper = new float[count], thresholds = new float[count];
        short[] expected = new short[count];
        float[] direct = new float[count], guarded = new float[count];
        for (int i = 0; i < count; i++)
        {
            int divisor = random.Next(1, 32768);
            var reciprocal = AcReciprocalExperiment.Interval.Create(divisor);
            float coefficient = i % 3 == 0 ? ((i / 3 % 31) + 0.5f) * divisor
                : (float)((random.NextDouble() * 2046) - 1023);
            coefficient = i % 2 == 0 ? coefficient : -coefficient;
            coefficients[i] = coefficient;
            divisors[i] = divisor;
            lower[i] = reciprocal.Lower;
            upper[i] = reciprocal.Upper;
            thresholds[i] = i % 5 == 0 ? MathF.Abs(coefficient / divisor) : 0.59082f;
            expected[i] = AcReciprocalExperiment.Exact(coefficient, divisor, thresholds[i]);
        }
        AcReciprocalExperiment.VectorExact(coefficients, divisors, thresholds, direct);
        AcReciprocalExperiment.VectorGuarded(coefficients, divisors, lower, upper, thresholds, guarded);
        Assert.Equal(expected.Select(value => (float)value), direct);
        Assert.Equal(direct, guarded);
    }
}
