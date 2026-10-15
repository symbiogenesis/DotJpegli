using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class FusedPlaneQuantizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FusedPlaneBlocksMatchTheOriginalPipelineForEveryFrequencyAndQuantizerMode(bool ordered)
    {
        const int stride = 17, left = 3, top = 2;
        var random = new Random(31729);
        foreach (bool adaptive in new[] { false, true })
            foreach (int component in new[] { 0, 1, 2 })
                for (int block = 0; block < 80; block++)
                {
                    float[] plane = Enumerable.Repeat(float.NaN, (top + 7) * stride + left + 8).ToArray();
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            plane[(top + y) * stride + left + x] = block < 64
                                ? y * 8 + x == block ? 231.375f : 128f : (float)(random.NextDouble() * 255.5);
                    ushort[] divisors = Enumerable.Range(0, 64).Select(index => (ushort)(1 + (index * 7 + block) % 19)).ToArray();
                    float[] multipliers = Enumerable.Range(0, 64).Select(index => (index % 7) * 0.1875f).ToArray();
                    var table = Quantization.PreparedTable.Create(divisors, component, 1f,
                        adaptiveQuantization: adaptive, adaptiveMultipliers: multipliers);
                    float strength = block % 3 == 0 ? 0f : block % 3 == 1 ? 0.625f : float.MaxValue;
                    short[] expected = Enumerable.Repeat((short)-23456, 72).ToArray(), actual = (short[])expected.Clone();
                    Original(plane, stride, left, top, table, expected, strength, ordered);
                    ForwardDct.TransformQuantizePlane(plane, stride, left, top, table, actual, strength, ordered);
                    Assert.Equal(expected, actual);
                }
    }

    [Fact]
    public void TransposedQuantizationPreservesHalfTiesAndStrictAdaptiveBiasBoundaries()
    {
        foreach (ushort divisor in new ushort[] { 1, 3, 7, 19, 255, 32767 })
            foreach (float strength in new[] { 0f, 0.625f, 2f })
                foreach (int component in new[] { 0, 1, 2 })
                {
                    ushort[] divisors = Enumerable.Repeat(divisor, 64).ToArray();
                    float[] multipliers = Enumerable.Range(0, 64).Select(index => index == 0 ? 0.5f : index % 3 * 0.25f).ToArray();
                    var table = Quantization.PreparedTable.Create(divisors, component, 1f,
                        adaptiveQuantization: true, adaptiveMultipliers: multipliers);
                    foreach (int integer in new[] { 0, 1, 2, 7, 63, 1022 })
                    {
                        float center = (integer + 0.5f) * divisor;
                        float[] boundary = [MathF.BitDecrement(center), center, MathF.BitIncrement(center)];
                        float[] coefficients = Enumerable.Range(0, 64)
                            .Select(index => (index % 2 == 0 ? 1 : -1) * boundary[index / 2 % 3]).ToArray();
                        AssertTransposed(coefficients, table, strength);
                    }
                    float offset = component switch { 0 => 0.59082f, 1 => 0.58146f, _ => 0.57988f };
                    float[] biased = Enumerable.Range(0, 64).Select(index =>
                    {
                        float center = (offset + strength * multipliers[index]) * divisor;
                        return (index % 2 == 0 ? 1 : -1) * ((index % 3) switch
                        {
                            0 => MathF.BitDecrement(center),
                            1 => center,
                            _ => MathF.BitIncrement(center)
                        });
                    }).ToArray();
                    AssertTransposed(biased, table, strength);
                }
    }

    [Fact]
    public void BothPortableOrderingNetworksPreserveEverySignedShortBitPattern()
    {
        for (int shift = 0; shift < 1024; shift++)
        {
            short[] transposed = Enumerable.Range(0, 64).Select(index => unchecked((short)(index + shift * 64))).ToArray();
            short[] expected = JpegConstants.ZigZag.ToArray().Select(index => transposed[(index % 8) * 8 + index / 8]).ToArray();
            short[] narrow = new short[64], wide = new short[64];
            Quantization.StoreTransposedZigZag128(Vector128.Create(transposed.AsSpan(0, 8)), Vector128.Create(transposed.AsSpan(8, 8)),
                Vector128.Create(transposed.AsSpan(16, 8)), Vector128.Create(transposed.AsSpan(24, 8)),
                Vector128.Create(transposed.AsSpan(32, 8)), Vector128.Create(transposed.AsSpan(40, 8)),
                Vector128.Create(transposed.AsSpan(48, 8)), Vector128.Create(transposed.AsSpan(56, 8)), narrow);
            Quantization.StoreTransposedZigZag512(Vector128.Create(transposed.AsSpan(0, 8)), Vector128.Create(transposed.AsSpan(8, 8)),
                Vector128.Create(transposed.AsSpan(16, 8)), Vector128.Create(transposed.AsSpan(24, 8)),
                Vector128.Create(transposed.AsSpan(32, 8)), Vector128.Create(transposed.AsSpan(40, 8)),
                Vector128.Create(transposed.AsSpan(48, 8)), Vector128.Create(transposed.AsSpan(56, 8)), wide);
            Assert.Equal(expected, narrow);
            Assert.Equal(expected, wide);
        }
    }

    [Fact]
    public void InvalidTransposedCoefficientsReturnWithoutWritingAnyOutput()
    {
        var table = Quantization.PreparedTable.Create(Enumerable.Repeat((ushort)1, 64).ToArray(), 0, 1f);
        foreach (int index in new[] { 0, 1, 7, 8, 16, 63 })
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity,
                float.MaxValue, -float.MaxValue, 1024f, -1024f })
            {
                float[] coefficients = new float[64];
                coefficients[index] = value;
                if (index == 0 && float.IsFinite(value))
                {
                    AssertTransposed(coefficients, table, 0);
                    continue;
                }
                short[] output = Enumerable.Repeat((short)-23456, 72).ToArray();
                Assert.False(TryTransposed(coefficients, table, 0, output));
                Assert.All(output, item => Assert.Equal((short)-23456, item));
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidValuesAndAcLimitsPreserveTheOriginalExceptionAndWrittenPrefix(bool ordered)
    {
        var table = Quantization.PreparedTable.Create(Enumerable.Repeat((ushort)1, 64).ToArray(), 0, 1f,
            adaptiveQuantization: true, adaptiveMultipliers: Enumerable.Repeat(0.5f, 64).ToArray());
        foreach (float sample in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.MaxValue, 8192f })
            foreach (float strength in new[] { 0f, float.NaN, float.PositiveInfinity, -1f })
            {
                float[] plane = Enumerable.Range(0, 64).Select(index => index % 2 == 0 ? sample : -sample).ToArray();
                short[] expected = Enumerable.Repeat((short)-23456, 72).ToArray(), actual = (short[])expected.Clone();
                ArgumentException expectedError = Assert.ThrowsAny<ArgumentException>(() => Original(plane, 8, 0, 0, table, expected, strength, ordered));
                ArgumentException actualError = Assert.ThrowsAny<ArgumentException>(() =>
                    ForwardDct.TransformQuantizePlane(plane, 8, 0, 0, table, actual, strength, ordered));
                Assert.Equal(expectedError.GetType(), actualError.GetType());
                Assert.Equal(expectedError.Message, actualError.Message);
                Assert.Equal(expected, actual);
            }
    }

    [Fact]
    public void OverlappingPlaneAndOrderedOutputConsumeAllSamplesBeforeWriting()
    {
        const int stride = 13, left = 3, top = 2, length = (top + 7) * stride + left + 8;
        var table = Quantization.PreparedTable.Create(Enumerable.Repeat((ushort)3, 64).ToArray(), 0, 1f);
        foreach (int offset in new[] { 0, 1, 7, 19, 63, 128 })
        {
            float[] storage = Enumerable.Range(0, length + 64).Select(index => (index * 31 % 256) + 0.125f).ToArray();
            short[] expected = new short[64];
            Original(storage, stride, left, top, table, expected, 0, true);
            Span<short> aliased = MemoryMarshal.Cast<float, short>(storage.AsSpan()).Slice(offset, 64);
            ForwardDct.TransformQuantizePlane(storage, stride, left, top, table, aliased);
            Assert.Equal(expected, aliased.ToArray());
        }
    }

    private static void AssertTransposed(float[] coefficients, Quantization.PreparedTable table, float strength)
    {
        short[] natural = new short[64], expected = new short[64], actual = Enumerable.Repeat((short)-23456, 72).ToArray();
        Quantization.QuantizePrepared(coefficients, table, natural, strength);
        EntropyEncoder.ValidateAc(natural);
        ZigZagOrdering.Reorder(natural, expected);
        Assert.True(TryTransposed(coefficients, table, strength, actual));
        Assert.Equal(expected, actual[..64]);
        Assert.All(actual[64..], value => Assert.Equal((short)-23456, value));
    }

    private static bool TryTransposed(float[] coefficients, Quantization.PreparedTable table, float strength, Span<short> output)
    {
        float[] transposed = Enumerable.Range(0, 64).Select(index => coefficients[(index % 8) * 8 + index / 8]).ToArray();
        return Quantization.TryQuantizeTransposed(Vector256.Create(transposed.AsSpan(0, 8)), Vector256.Create(transposed.AsSpan(8, 8)),
            Vector256.Create(transposed.AsSpan(16, 8)), Vector256.Create(transposed.AsSpan(24, 8)),
            Vector256.Create(transposed.AsSpan(32, 8)), Vector256.Create(transposed.AsSpan(40, 8)),
            Vector256.Create(transposed.AsSpan(48, 8)), Vector256.Create(transposed.AsSpan(56, 8)), table, table.GetTransposed(), strength, output);
    }

    private static void Original(float[] plane, int stride, int left, int top, Quantization.PreparedTable table,
        Span<short> output, float strength, bool ordered)
    {
        float[] coefficients = new float[64];
        Span<short> temporary = stackalloc short[64];
        ForwardDct.TransformPlane(plane, stride, left, top, coefficients);
        Span<short> natural = ordered ? temporary : output[..64];
        Quantization.QuantizePrepared(coefficients, table, natural, strength);
        EntropyEncoder.ValidateAc(natural);
        if (ordered)
            ZigZagOrdering.Reorder(natural, output);
    }
}
