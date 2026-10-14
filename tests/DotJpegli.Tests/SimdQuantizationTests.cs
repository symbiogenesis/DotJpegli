using System.Runtime.InteropServices;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SimdQuantizationTests
{
    private delegate void Quantizer(ReadOnlySpan<float> coefficients, ReadOnlySpan<ushort> table,
        Span<short> output, int component, float distance, bool grayscale, bool adaptiveQuantization,
        float adaptiveStrength, ReadOnlySpan<float> adaptiveMultipliers, int previousDc,
        bool requireEightBitCoefficients, bool rgb);

    private static readonly Quantizer[] Paths =
    [Quantization.QuantizeScalar, Quantization.QuantizeVector, Quantization.Quantize];

    [Theory]
    [InlineData(0, false, false, false)]
    [InlineData(1, false, false, false)]
    [InlineData(2, false, false, false)]
    [InlineData(0, false, false, true)]
    [InlineData(1, false, false, true)]
    [InlineData(2, false, false, true)]
    [InlineData(0, true, false, false)]
    [InlineData(0, true, false, true)]
    [InlineData(0, false, true, false)]
    [InlineData(0, false, true, true)]
    [InlineData(2, false, true, true)]
    public void AllPathsMatchIndependentQuantizationAcrossModes(int component, bool grayscale,
        bool rgb, bool adaptive)
    {
        var random = new Random(7319);
        for (int block = 0; block < 48; block++)
        {
            float[] coefficients = new float[72];
            ushort[] table = new ushort[72];
            float[] multipliers = new float[72];
            for (int i = 0; i < 64; i++)
            {
                table[i] = (ushort)random.Next(1, block < 24 ? 256 : 32768);
                coefficients[i] = block < 24
                    ? (float)(random.NextDouble() * 8192 - 4096)
                    : MathF.ScaleB((float)(random.NextDouble() * 2 - 1), random.Next(-149, 129));
                multipliers[i] = (float)random.NextDouble() * 2.5f;
            }
            // Extra source storage is intentionally invalid: only 64 values
            // belong to the block, and the output tail must stay untouched.
            Array.Fill(coefficients, float.NaN, 64, 8);
            Array.Fill(multipliers, float.NaN, 64, 8);
            float strength = block % 3 == 0 ? 0f : block % 3 == 1 ? 0.625f : float.MaxValue;
            byte[] originalCoefficients = MemoryMarshal.AsBytes(coefficients.AsSpan()).ToArray();
            ushort[] originalTable = (ushort[])table.Clone();
            byte[] originalMultipliers = MemoryMarshal.AsBytes(multipliers.AsSpan()).ToArray();
            short[] expected = Reference(coefficients, table, component, grayscale, rgb,
                adaptive, strength, multipliers, previousDc: 37);
            foreach (Quantizer path in Paths)
            {
                short[] output = Enumerable.Repeat((short)-23456, 80).ToArray();
                Invoke(path, coefficients, table, output.AsSpan(3), component, grayscale, rgb,
                    adaptive, strength, multipliers, previousDc: 37);
                Assert.Equal(expected, output.AsSpan(3, 64).ToArray());
                Assert.All(output[..3], value => Assert.Equal((short)-23456, value));
                Assert.All(output[67..], value => Assert.Equal((short)-23456, value));
                Assert.Equal(originalCoefficients, MemoryMarshal.AsBytes(coefficients.AsSpan()).ToArray());
                Assert.Equal(originalTable, table);
                Assert.Equal(originalMultipliers, MemoryMarshal.AsBytes(multipliers.AsSpan()).ToArray());
            }
        }
    }

    [Fact]
    public void AdaptiveThresholdKeepsSeparateMultiplyAndAddRounding()
    {
        var random = new Random(5137);
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        int found = 0;
        for (int trial = 0; trial < 1024 && found < 24; trial++)
        {
            float strength = (float)random.NextDouble() * 3;
            float multiplier = (float)random.NextDouble() * 3;
            float threshold = 0.59082f + strength * multiplier;
            if (threshold == MathF.FusedMultiplyAdd(strength, multiplier, 0.59082f))
                continue;
            found++;
            float[] multipliers = Enumerable.Repeat(multiplier, 64).ToArray();
            multipliers[0] = 0;
            float[] boundaries = [MathF.BitDecrement(threshold), threshold, MathF.BitIncrement(threshold)];
            float[] coefficients = Enumerable.Range(0, 64).Select(i =>
                (i % 2 == 0 ? 1 : -1) * boundaries[(i / 2) % 3]).ToArray();
            foreach (Quantizer path in Paths)
            {
                short[] actual = new short[64];
                Invoke(path, coefficients, table, actual, adaptive: true,
                    strength: strength, multipliers: multipliers);
                Assert.Equal(Reference(coefficients, table, 0, false, false, true,
                    strength, multipliers, 0), actual);
            }
        }
        Assert.Equal(24, found);
    }

    [Theory]
    [InlineData(0, false, false, false)]
    [InlineData(1, false, false, false)]
    [InlineData(2, false, false, false)]
    [InlineData(0, true, false, true)]
    [InlineData(0, false, true, true)]
    [InlineData(2, false, false, true)]
    public void ThresholdComparisonIsStrictAndAcHalfTiesAreEven(int component, bool grayscale,
        bool rgb, bool adaptive)
    {
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        float[] multipliers = Enumerable.Repeat(1.25f, 64).ToArray();
        multipliers[0] = 0;
        const float strength = 0.625f;
        float offset = grayscale || rgb ? 0.5f : component == 0 ? 0.59082f
            : component == 1 ? 0.58146f : 0.57988f;
        float threshold = offset + (adaptive ? strength * 1.25f : 0);
        float[] boundaries =
        [MathF.BitDecrement(threshold), threshold, MathF.BitIncrement(threshold),
            0.5f, 1.5f, 2.5f, 3.5f, MathF.BitDecrement(2.5f), MathF.BitIncrement(2.5f)];
        float[] coefficients = Enumerable.Range(0, 64).Select(i =>
            (i % 2 == 0 ? 1 : -1) * boundaries[(i / 2) % boundaries.Length]).ToArray();
        foreach (Quantizer path in Paths)
        {
            short[] actual = new short[64];
            Invoke(path, coefficients, table, actual, component, grayscale, rgb, adaptive,
                strength, multipliers);
            Assert.Equal(Reference(coefficients, table, component, grayscale, rgb, adaptive,
                strength, multipliers, 0), actual);
            Assert.Equal((short)0, actual[1]);
            Assert.NotEqual((short)0, actual[2]);
            Assert.NotEqual((short)0, actual[3]);
        }
    }

    [Fact]
    public void DivisionAndRoundingRemainExactNearHalfIntegersForNonPowerOfTwoTables()
    {
        foreach (ushort quantizer in new ushort[] { 3, 7, 11, 19, 31, 255, 256, 32767 })
            foreach (int integer in new[] { 0, 1, 2, 7, 63, 1023, 32766 })
            {
                float center = (integer + 0.5f) * quantizer;
                float[] values = [MathF.BitDecrement(center), center, MathF.BitIncrement(center)];
                float[] coefficients = Enumerable.Range(0, 64).Select(i =>
                    (i % 2 == 0 ? 1 : -1) * values[(i / 2) % 3]).ToArray();
                ushort[] table = Enumerable.Repeat(quantizer, 64).ToArray();
                short[] expected = Reference(coefficients, table, 0, true, false, false, 0, [], 0);
                foreach (Quantizer path in Paths)
                {
                    short[] actual = new short[64];
                    Invoke(path, coefficients, table, actual, grayscale: true);
                    Assert.Equal(expected, actual);
                }
            }
    }

    [Fact]
    public void DcUsesPreviousBlockThresholdAndAwayFromZeroRounding()
    {
        float[] coefficients = new float[64];
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        float[] multipliers = new float[64];
        multipliers[0] = 1.5f;
        foreach (float dc in new[] { -1.5f, -0.5f, 0.5f, 1.5f, 6.5f, 7.5f, 9f, 10.5f })
            foreach (Quantizer path in Paths)
            {
                coefficients[0] = dc;
                short[] actual = new short[64];
                Invoke(path, coefficients, table, actual, adaptive: true, strength: 1,
                    multipliers: multipliers, previousDc: 9);
                short expected = MathF.Abs(dc - 9) < 1.5f ? (short)9
                    : (short)MathF.Round(dc, MidpointRounding.AwayFromZero);
                Assert.Equal(expected, actual[0]);
            }
    }

    [Fact]
    public void ExtremeFiniteValuesSaturateAndAdaptiveThresholdOverflowMatchesScalar()
    {
        float[] values = [float.MaxValue, float.MinValue, short.MaxValue, short.MinValue,
            32767.5f, -32768.5f, float.Epsilon, -float.Epsilon, 0f, -0f];
        float[] coefficients = Enumerable.Range(0, 64).Select(i => values[i % values.Length]).ToArray();
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        float[] multipliers = Enumerable.Repeat(float.MaxValue, 64).ToArray();
        foreach (bool adaptive in new[] { false, true })
            foreach (Quantizer path in Paths)
            {
                short[] actual = new short[64];
                Invoke(path, coefficients, table, actual, adaptive: adaptive,
                    strength: float.MaxValue, multipliers: multipliers);
                Assert.Equal(Reference(coefficients, table, 0, false, false, adaptive,
                    float.MaxValue, multipliers, 0), actual);
            }
    }

    [Fact]
    public void InvalidValuesPreserveFirstExceptionAndPrecedingWrites()
    {
        foreach (int position in Enumerable.Range(0, 64))
            foreach (int invalidKind in Enumerable.Range(0, 8))
            {
                float[] coefficients = Enumerable.Repeat(17.25f, 64).ToArray();
                ushort[] table = Enumerable.Repeat((ushort)3, 64).ToArray();
                float[] multipliers = Enumerable.Repeat(0.5f, 64).ToArray();
                switch (invalidKind)
                {
                    case 0: table[position] = 0; break;
                    case 1: table[position] = 32768; break;
                    case 2: coefficients[position] = float.NaN; break;
                    case 3: coefficients[position] = float.PositiveInfinity; break;
                    case 4: coefficients[position] = float.NegativeInfinity; break;
                    case 5: multipliers[position] = float.NaN; break;
                    case 6: multipliers[position] = float.PositiveInfinity; break;
                    case 7: multipliers[position] = -float.Epsilon; break;
                }
                short[] expected = Enumerable.Repeat((short)-23456, 72).ToArray();
                ArgumentException expectedError = Assert.ThrowsAny<ArgumentException>(() =>
                    Invoke(Paths[0], coefficients, table, expected, adaptive: true,
                        strength: 1, multipliers: multipliers));
                foreach (Quantizer path in Paths[1..])
                {
                    short[] actual = Enumerable.Repeat((short)-23456, 72).ToArray();
                    ArgumentException actualError = Assert.ThrowsAny<ArgumentException>(() =>
                        Invoke(path, coefficients, table, actual, adaptive: true,
                            strength: 1, multipliers: multipliers));
                    Assert.Equal(expectedError.GetType(), actualError.GetType());
                    Assert.Equal(expectedError.ParamName, actualError.ParamName);
                    Assert.Equal(expectedError.Message, actualError.Message);
                    Assert.Equal(expected, actual);
                }
            }
    }

    [Fact]
    public void EightBitRangeChecksPrecedeLaterInvalidInputsAndPreserveWrittenPrefix()
    {
        foreach (int position in Enumerable.Range(0, 64))
            foreach (float value in new[] { -1024.5f, -1024f, -1023f, 1023f, 1023.5f, 1024f })
            {
                float[] coefficients = Enumerable.Repeat(1f, 64).ToArray();
                coefficients[position] = value;
                ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
                if (position < 63)
                    table[position + 1] = 0;
                short[] expected = Enumerable.Repeat((short)-23456, 64).ToArray();
                Exception? expectedError = Record.Exception(() =>
                    Invoke(Paths[0], coefficients, table, expected, requireEightBit: true));
                foreach (Quantizer path in Paths[1..])
                {
                    short[] actual = Enumerable.Repeat((short)-23456, 64).ToArray();
                    Exception? actualError = Record.Exception(() =>
                        Invoke(path, coefficients, table, actual, requireEightBit: true));
                    Assert.Equal(expectedError?.GetType(), actualError?.GetType());
                    Assert.Equal(expectedError?.Message, actualError?.Message);
                    Assert.Equal(expected, actual);
                }
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(63)]
    public void AliasedTableStoragePreservesScalarReadAndWriteOrder(int outputStart)
    {
        float[] coefficients = Enumerable.Repeat(91f, 64).ToArray();
        short[] initial = Enumerable.Repeat((short)3, 144).ToArray();
        short[] expected = (short[])initial.Clone();
        Paths[0](coefficients, MemoryMarshal.Cast<short, ushort>(expected.AsSpan(0, 64)),
            expected.AsSpan(outputStart, 64), 0, 1, false, false, 0, [], 0, false, false);
        foreach (Quantizer path in Paths[1..])
        {
            short[] actual = (short[])initial.Clone();
            path(coefficients, MemoryMarshal.Cast<short, ushort>(actual.AsSpan(0, 64)),
                actual.AsSpan(outputStart, 64), 0, 1, false, false, 0, [], 0, false, false);
            Assert.Equal(expected, actual);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AliasedFloatStoragePreservesScalarReadAndWriteOrder(bool aliasMultipliers)
    {
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        float[] initial = Enumerable.Repeat(17.25f, 72).ToArray();
        float[] coefficients = Enumerable.Repeat(10f, 64).ToArray();
        float[] expected = (float[])initial.Clone();
        Invoke(Paths[0], aliasMultipliers ? coefficients : expected, table,
            MemoryMarshal.Cast<float, short>(expected.AsSpan()).Slice(7, 64), adaptive: aliasMultipliers,
            strength: 0.1f, multipliers: expected);
        foreach (Quantizer path in Paths[1..])
        {
            float[] actual = (float[])initial.Clone();
            Invoke(path, aliasMultipliers ? coefficients : actual, table,
                MemoryMarshal.Cast<float, short>(actual.AsSpan()).Slice(7, 64), adaptive: aliasMultipliers,
                strength: 0.1f, multipliers: actual);
            Assert.Equal(MemoryMarshal.AsBytes(expected.AsSpan()).ToArray(),
                MemoryMarshal.AsBytes(actual.AsSpan()).ToArray());
        }
    }

    [Fact]
    public void AllPathsValidateArgumentsBeforeWriting()
    {
        foreach (Quantizer path in Paths)
        {
            float[] coefficients = new float[64];
            ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
            short[] output = Enumerable.Repeat((short)-23456, 64).ToArray();
            float[] multipliers = new float[64];
            Assert.Equal("component", Assert.Throws<ArgumentOutOfRangeException>(() =>
                Invoke(path, coefficients, table, output, component: -1)).ParamName);
            Assert.Equal("component", Assert.Throws<ArgumentOutOfRangeException>(() =>
                Invoke(path, coefficients, table, output, component: 1, grayscale: true)).ParamName);
            foreach (float distance in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
                Assert.Equal("distance", Assert.Throws<ArgumentOutOfRangeException>(() =>
                    path(coefficients, table, output, 0, distance, false, false, 0, [], 0, false, false)).ParamName);
            Assert.Equal("coefficients", Assert.Throws<ArgumentException>(() =>
                Invoke(path, coefficients[..63], table, output)).ParamName);
            Assert.Equal("table", Assert.Throws<ArgumentException>(() =>
                Invoke(path, coefficients, table[..63], output)).ParamName);
            Assert.Equal("output", Assert.Throws<ArgumentException>(() =>
                Invoke(path, coefficients, table, output.AsSpan(0, 63))).ParamName);
            foreach (float strength in new[] { -float.Epsilon, float.NaN, float.PositiveInfinity })
                Assert.Equal("adaptiveStrength", Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Invoke(path, coefficients, table, output, adaptive: true,
                        strength: strength, multipliers: multipliers)).ParamName);
            Assert.Equal("adaptiveMultipliers", Assert.Throws<ArgumentException>(() =>
                Invoke(path, coefficients, table, output, adaptive: true, multipliers: multipliers[..63])).ParamName);
            Assert.All(output, value => Assert.Equal((short)-23456, value));
            // Disabled AQ deliberately ignores its optional inputs.
            Invoke(path, coefficients, table, output, strength: float.NaN, multipliers: [float.NaN]);
            Assert.All(output, value => Assert.Equal((short)0, value));
        }
    }

    private static void Invoke(Quantizer path, ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, int component = 0, bool grayscale = false,
        bool rgb = false, bool adaptive = false, float strength = 0,
        ReadOnlySpan<float> multipliers = default, int previousDc = 0, bool requireEightBit = false)
        => path(coefficients, table, output, component, 1, grayscale, adaptive, strength,
            multipliers, previousDc, requireEightBit, rgb);

    private static short[] Reference(float[] coefficients, ushort[] table, int component,
        bool grayscale, bool rgb, bool adaptive, float strength, float[] multipliers, int previousDc)
    {
        short[] output = new short[64];
        float offset = grayscale || rgb ? adaptive ? 0.5f : 0f : component == 0 ? 0.59082f
            : component == 1 ? 0.58146f : 0.57988f;
        for (int i = 0; i < 64; i++)
        {
            float quotient = coefficients[i] / table[i];
            float threshold = (i == 0 ? 0 : offset) + (adaptive ? strength * multipliers[i] : 0);
            float rounded = i == 0
                ? MathF.Abs(quotient - previousDc) < threshold ? previousDc
                    : MathF.Round(quotient, MidpointRounding.AwayFromZero)
                : MathF.Abs(quotient) < threshold ? 0 : MathF.Round(quotient);
            output[i] = (short)Math.Clamp(rounded, short.MinValue, short.MaxValue);
        }
        return output;
    }
}
