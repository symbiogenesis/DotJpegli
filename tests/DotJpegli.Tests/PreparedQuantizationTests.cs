using System.Runtime.InteropServices;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class PreparedQuantizationTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, false, false)]
    [InlineData(0, true, false)]
    [InlineData(0, false, true)]
    [InlineData(2, false, true)]
    public void PreparedTablesPreserveScalarResultsAcrossQuantizerModes(int component, bool grayscale, bool rgb)
    {
        var random = new Random(5941);
        foreach (bool adaptive in new[] { false, true })
            for (int block = 0; block < 48; block++)
            {
                ushort[] table = new ushort[72];
                float[] coefficients = new float[72];
                float[] multipliers = new float[72];
                for (int i = 0; i < 64; i++)
                {
                    table[i] = (ushort)random.Next(1, block < 24 ? 256 : 32768);
                    coefficients[i] = block < 24 ? (float)(random.NextDouble() * 8192 - 4096)
                        : MathF.ScaleB((float)(random.NextDouble() * 2 - 1), random.Next(-149, 128));
                    multipliers[i] = (float)random.NextDouble() * 2.5f;
                }
                Array.Fill(coefficients, float.NaN, 64, 8);
                Array.Fill(multipliers, float.NaN, 64, 8);
                var prepared = Quantization.PreparedTable.Create(table, component, 1f, grayscale, adaptive, multipliers, rgb);
                float strength = block % 3 == 0 ? 0f : block % 3 == 1 ? 0.625f : float.MaxValue;
                short[] expected = Enumerable.Repeat((short)-23456, 80).ToArray();
                short[] actual = (short[])expected.Clone();
                Quantization.QuantizeScalar(coefficients, table, expected.AsSpan(3), component, 1f,
                    grayscale, adaptive, strength, multipliers, previousDc: 37, rgb: rgb);
                Quantization.QuantizePrepared(coefficients, prepared, actual.AsSpan(3), strength, previousDc: 37);
                Assert.Equal(expected, actual);
            }
    }

    [Fact]
    public void HalfIntegerBoundariesKeepExactDivisionAndRounding()
    {
        foreach (ushort divisor in new ushort[] { 3, 7, 11, 19, 31, 255, 256, 32767 })
        {
            ushort[] table = Enumerable.Repeat(divisor, 64).ToArray();
            var prepared = Quantization.PreparedTable.Create(table, 0, 1f, grayscale: true);
            foreach (int integer in new[] { 0, 1, 2, 7, 63, 1023, 32766 })
            {
                float center = (integer + 0.5f) * divisor;
                float[] values = [MathF.BitDecrement(center), center, MathF.BitIncrement(center)];
                float[] coefficients = Enumerable.Range(0, 64)
                    .Select(i => (i % 2 == 0 ? 1 : -1) * values[(i / 2) % 3]).ToArray();
                short[] expected = new short[64], actual = new short[64];
                Quantization.QuantizeScalar(coefficients, table, expected, 0, 1f, grayscale: true);
                Quantization.QuantizePrepared(coefficients, prepared, actual);
                Assert.Equal(expected, actual);
            }
        }
    }

    [Fact]
    public void InvalidCoefficientsPreserveFirstExceptionAndWrittenPrefix()
    {
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        var prepared = Quantization.PreparedTable.Create(table, 0, 1f);
        for (int position = 0; position < 64; position++)
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, -1024.5f, 1023.5f })
            {
                float[] coefficients = Enumerable.Repeat(17.25f, 64).ToArray();
                coefficients[position] = value;
                if (position < 63)
                    coefficients[position + 1] = float.NaN;
                short[] expected = Enumerable.Repeat((short)-23456, 72).ToArray();
                short[] actual = (short[])expected.Clone();
                ArgumentException expectedError = Assert.ThrowsAny<ArgumentException>(() =>
                    Quantization.QuantizeScalar(coefficients, table, expected, 0, 1f, requireEightBitCoefficients: true));
                ArgumentException actualError = Assert.ThrowsAny<ArgumentException>(() =>
                    Quantization.QuantizePrepared(coefficients, prepared, actual, requireEightBitCoefficients: true));
                Assert.Equal(expectedError.GetType(), actualError.GetType());
                Assert.Equal(expectedError.Message, actualError.Message);
                Assert.Equal(expected, actual);
            }
    }

    [Fact]
    public void SourceMutationCannotChangePreparedDivisorsOrMultipliers()
    {
        ushort[] table = Enumerable.Repeat((ushort)3, 64).ToArray();
        float[] multipliers = Enumerable.Repeat(0.5f, 64).ToArray();
        float[] coefficients = Enumerable.Repeat(17.25f, 64).ToArray();
        short[] expected = new short[64], actual = new short[64];
        Quantization.QuantizeScalar(coefficients, table, expected, 0, 1f,
            adaptiveQuantization: true, adaptiveStrength: 1f, adaptiveMultipliers: multipliers);
        var prepared = Quantization.PreparedTable.Create(table, 0, 1f,
            adaptiveQuantization: true, adaptiveMultipliers: multipliers);
        Array.Fill(table, (ushort)0);
        Array.Fill(multipliers, float.NaN);
        Quantization.QuantizePrepared(coefficients, prepared, actual, adaptiveStrength: 1f);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AliasedCoefficientStoragePreservesScalarReadAndWriteOrder()
    {
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        var prepared = Quantization.PreparedTable.Create(table, 0, 1f);
        float[] expected = Enumerable.Repeat(17.25f, 72).ToArray();
        float[] actual = (float[])expected.Clone();
        Quantization.QuantizeScalar(expected, table, MemoryMarshal.Cast<float, short>(expected.AsSpan()).Slice(7, 64), 0, 1f);
        Quantization.QuantizePrepared(actual, prepared, MemoryMarshal.Cast<float, short>(actual.AsSpan()).Slice(7, 64));
        Assert.Equal(MemoryMarshal.AsBytes(expected.AsSpan()).ToArray(), MemoryMarshal.AsBytes(actual.AsSpan()).ToArray());
    }

    [Fact]
    public void PreparedTablesValidateStaticInputsAndDynamicStrength()
    {
        ushort[] table = Enumerable.Repeat((ushort)1, 64).ToArray();
        float[] multipliers = new float[64];
        foreach (int position in new[] { 0, 17, 63 })
        {
            table[position] = 0;
            Assert.Equal("table", Assert.Throws<ArgumentException>(() =>
                Quantization.PreparedTable.Create(table, 0, 1f)).ParamName);
            table[position] = 1;
            multipliers[position] = float.NaN;
            Assert.Equal("adaptiveMultipliers", Assert.Throws<ArgumentException>(() =>
                Quantization.PreparedTable.Create(table, 0, 1f, adaptiveQuantization: true, adaptiveMultipliers: multipliers)).ParamName);
            multipliers[position] = 0;
        }
        var adaptive = Quantization.PreparedTable.Create(table, 0, 1f,
            adaptiveQuantization: true, adaptiveMultipliers: multipliers);
        float[] coefficients = new float[64];
        short[] output = Enumerable.Repeat((short)-23456, 64).ToArray();
        foreach (float strength in new[] { float.NaN, float.PositiveInfinity, -float.Epsilon })
            Assert.Equal("adaptiveStrength", Assert.Throws<ArgumentOutOfRangeException>(() =>
                Quantization.QuantizePrepared(coefficients, adaptive, output, strength)).ParamName);
        Assert.All(output, value => Assert.Equal((short)-23456, value));
        var uniform = Quantization.PreparedTable.Create(table, 0, 1f, grayscale: true, adaptiveMultipliers: [float.NaN]);
        Quantization.QuantizePrepared(coefficients, uniform, output, float.NaN);
        Assert.All(output, value => Assert.Equal((short)0, value));
    }
}
