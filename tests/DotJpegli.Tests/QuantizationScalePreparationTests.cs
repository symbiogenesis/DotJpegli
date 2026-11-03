using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class QuantizationScalePreparationTests
{
    // Original quant.cc exponents, kept independent of the prepared data.
    private static ReadOnlySpan<float> Exponents =>
    [
        1.00f, 0.51f, 0.67f, 0.74f, 1.00f, 1.00f, 1.00f, 1.00f,
        0.51f, 0.66f, 0.69f, 0.87f, 1.00f, 1.00f, 1.00f, 1.00f,
        0.67f, 0.69f, 0.84f, 0.83f, 0.96f, 1.00f, 1.00f, 1.00f,
        0.74f, 0.87f, 0.83f, 1.00f, 1.00f, 0.91f, 0.91f, 1.00f,
        1.00f, 1.00f, 0.96f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 0.91f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 0.91f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
    ];

    private static float[] Distances =>
    [
        float.Epsilon, BitConverter.Int32BitsToSingle(0x00800000), 0.001f, 0.01f, 0.1f, 0.5f, 1f,
        MathF.BitDecrement(1.5f), 1.5f, MathF.BitIncrement(1.5f),
        2f, 2.375f, 4f, 10f, 25f, 30f, 100f, 10000f, float.MaxValue
    ];

    [Fact]
    public void PreparedFrequencyFactorsPreserveOriginalScaleBits()
    {
        foreach (float distance in Distances)
            for (int frequency = 0; frequency < 64; frequency++)
            {
                float exponent = Exponents[frequency];
                float expected = distance < 1.5f ? distance
                    : MathF.Max(0.5f * distance,
                        (float)Math.Pow(1.5f, 1.0 - exponent) * MathF.Pow(distance, exponent));
                Assert.Equal(BitConverter.SingleToInt32Bits(expected),
                    BitConverter.SingleToInt32Bits(Quantization.DistanceToScale(distance, frequency)));
            }
    }

    [Fact]
    public void PreparedInverseBaseMatricesPreserveOriginalDivisionBitsForEveryTransferCode()
    {
        ReadOnlySpan<float> matrices = Quantization.BaseMatrices;
        for (int transfer = 0; transfer <= 255; transfer++)
        {
            float scale = transfer switch
            {
                16 => 1.73966010f * 0.4f,
                18 => 1.73966010f * 0.5f,
                _ => 1.73966010f
            };
            ReadOnlySpan<float> prepared = Quantization.GetInverseBaseQuantizers(transfer);
            Assert.Equal(192, prepared.Length);
            for (int index = 0; index < matrices.Length; index++)
            {
                float expected = 1f / matrices[index] / scale;
                Assert.Equal(BitConverter.SingleToInt32Bits(expected),
                    BitConverter.SingleToInt32Bits(prepared[index]));
            }
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.GetInverseBaseQuantizers(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.GetInverseBaseQuantizers(256));
    }

    [Fact]
    public void PreparedInverseFactorsPreserveOriginalScaleBits()
    {
        foreach (float scale in Distances)
            for (int frequency = 0; frequency < 64; frequency++)
            {
                float exponent = (float)(1.0 / Exponents[frequency]);
                float expected = scale < 1.5f ? scale
                    : MathF.Min(2f * scale,
                        (float)Math.Pow(1.5f, 1.0 - exponent) * MathF.Pow(scale, exponent));
                Assert.Equal(BitConverter.SingleToInt32Bits(expected),
                    BitConverter.SingleToInt32Bits(Quantization.ScaleToDistance(scale, frequency)));
            }
    }
}
