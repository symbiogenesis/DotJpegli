using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class NumericalTests
{
    [Fact]
    public void DctMatchesIndependentCosineDefinition()
    {
        var random = new Random(17);
        float[] input = new float[64];
        float[] actual = new float[64];
        for (int block = 0; block < 32; block++)
        {
            for (int i = 0; i < 64; i++)
                input[i] = random.Next(256) - 128;
            ForwardDct.Transform(input, actual);
            for (int v = 0; v < 8; v++)
                for (int u = 0; u < 8; u++)
                {
                    double sum = 0;
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            sum += input[y * 8 + x] * Math.Cos((2 * x + 1) * u * Math.PI / 16)
                                * Math.Cos((2 * y + 1) * v * Math.PI / 16);
                    double expected = sum * 0.25 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1);
                    Assert.InRange(Math.Abs(expected - actual[v * 8 + u]), 0, 0.0002);
                }
        }
    }

    [Fact]
    public void DctConstantBlockHasOnlyDcAndSupportsAliasing()
    {
        float[] block = Enumerable.Repeat(40f, 64).ToArray();
        ForwardDct.Transform(block, block);
        Assert.InRange(block[0], 319.999f, 320.001f);
        Assert.All(block.Skip(1), value => Assert.InRange(Math.Abs(value), 0, 0.0002f));
    }

    [Fact]
    public void QuantTablesAreBaselineAndDistinct()
    {
        var y = Quantization.CreateTable(85, 0);
        var cb = Quantization.CreateTable(85, 1);
        var cr = Quantization.CreateTable(85, 2);
        Assert.False(y.SequenceEqual(cb));
        Assert.False(cb.SequenceEqual(cr));
        foreach (float quality in new[] { 0f, 25f, 85f, 100f })
            for (int component = 0; component < 3; component++)
                Assert.All(Quantization.CreateTable(quality, component), value => Assert.InRange(value, (ushort)1, (ushort)255));
    }

    [Fact]
    public void QuantizationMatchesPinnedCppFixtures()
    {
        // Values emitted by compiling the pinned C++ quant.cc constants,
        // DistanceToScale and jpegli_quality_to_distance with MSVC.
        // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20, Q85.
        ushort[] y444 = [3,4,7,7,8,9,10,10,4,5,7,7,9,8,10,10,7,7,7,9,9,10,9,11,7,7,9,9,11,12,12,12,8,9,9,11,12,12,14,14,9,8,10,12,12,14,13,15,10,10,9,12,14,13,14,14,10,10,11,12,14,15,14,11];
        ushort[] cb420 = [4,6,10,11,11,15,18,30,6,9,10,12,16,16,34,23,10,10,12,18,17,28,106,26,11,12,18,17,17,26,31,42,11,16,17,17,19,15,17,60,15,16,28,26,15,31,46,15,18,34,106,31,17,46,15,15,30,23,26,42,60,15,15,116];
        ushort[] cr420 = [4,4,8,7,8,8,9,10,4,6,8,6,8,7,9,9,8,8,8,7,7,8,8,9,7,6,7,7,8,8,9,10,8,8,7,8,6,10,8,10,8,7,8,8,10,9,10,14,9,9,8,9,8,10,16,14,10,9,9,10,10,14,14,119];
        Assert.Equal(y444, Quantization.CreateTable(85, 0));
        Assert.Equal(y444, Quantization.CreateGrayscaleTable(85));
        Assert.Equal(cb420, Quantization.CreateTable(85, 1, 2, 2));
        Assert.Equal(cr420, Quantization.CreateTable(85, 2, 2, 2));
    }
}
