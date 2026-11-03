using System.Runtime.InteropServices;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class FusedBlockPreparationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenteredBlocksPreserveSeparateDctQuantizationAndOrdering(bool ordered)
    {
        var random = new Random(49137);
        foreach (bool adaptive in new[] { false, true })
            foreach (int component in new[] { 0, 1, 2 })
            {
                var table = Table(component, adaptive);
                for (int block = 0; block < 80; block++)
                {
                    float[] samples = Enumerable.Range(0, 64).Select(index => block < 64
                        ? index == block ? 103.375f : 0f : (float)(random.NextDouble() * 256 - 128)).ToArray();
                    float strength = block % 3 == 0 ? 0f : block % 3 == 1 ? 0.625f : float.MaxValue;
                    short[] expected = Original(samples, table, strength, ordered);
                    short[] actual = Enumerable.Repeat((short)-23456, 72).ToArray();
                    ForwardDct.TransformQuantizeBlock(samples, table, actual, strength, ordered);
                    Assert.Equal(expected, actual[..64]);
                    Assert.All(actual[64..], value => Assert.Equal((short)-23456, value));
                }
            }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 9)]
    [InlineData(8, 8)]
    [InlineData(19, 17)]
    [InlineData(257, 193)]
    public void PartialPlaneBlocksReplicateEveryRightAndBottomEdgeBeforeTransforming(int width, int height)
    {
        float[] plane = Enumerable.Range(0, width * height)
            .Select(index => ((index * 31 + index / width * 47) & 255) + (index % 7) * 0.0625f).ToArray();
        foreach (bool adaptive in new[] { false, true })
            foreach (int component in new[] { 0, 1, 2 })
            {
                var table = Table(component, adaptive);
                for (int top = 0; top < height; top += 8)
                    for (int left = 0; left < width; left += 8)
                    {
                        float[] samples = Centered(plane, width, height, left, top);
                        short[] expected = Original(samples, table, 0.625f, ordered: true);
                        short[] actual = Enumerable.Repeat((short)-23456, 72).ToArray();
                        ForwardDct.TransformQuantizePaddedPlane(plane, width, height, left, top,
                            table, actual, 0.625f);
                        Assert.Equal(expected, actual[..64]);
                        Assert.All(actual[64..], value => Assert.Equal((short)-23456, value));
                    }
            }
    }

    [Fact]
    public void PaddedPlaneAndCenteredBlockConsumeAliasedSamplesBeforeWriting()
    {
        var table = Table(0, adaptive: false);
        foreach (int offset in new[] { 0, 7, 63, 128 })
        {
            float[] plane = Enumerable.Range(0, 13 * 11).Select(index => (index * 31 & 255) + 0.125f).ToArray();
            short[] expected = Original(Centered(plane, 13, 11, 8, 8), table, 0f, ordered: true);
            Span<short> output = MemoryMarshal.Cast<float, short>(plane.AsSpan()).Slice(offset, 64);
            ForwardDct.TransformQuantizePaddedPlane(plane, 13, 11, 8, 8, table, output);
            Assert.Equal(expected, output.ToArray());
        }
        foreach (bool ordered in new[] { false, true })
        {
            float[] samples = Enumerable.Range(0, 100).Select(index => (index * 31 & 255) - 128.125f).ToArray();
            short[] expected = Original(samples[..64], table, 0f, ordered);
            Span<short> output = MemoryMarshal.Cast<float, short>(samples.AsSpan()).Slice(7, 64);
            ForwardDct.TransformQuantizeBlock(samples.AsSpan(0, 64), table, output, zigZagOrdered: ordered);
            Assert.Equal(expected, output.ToArray());
        }
    }

    private static Quantization.PreparedTable Table(int component, bool adaptive)
        => Quantization.PreparedTable.Create(Enumerable.Range(0, 64).Select(index => (ushort)(1 + index % 19)).ToArray(),
            component, 4f, adaptiveQuantization: adaptive,
            adaptiveMultipliers: Enumerable.Range(0, 64).Select(index => index % 7 * 0.1875f).ToArray());

    private static float[] Centered(float[] plane, int width, int height, int left, int top)
        => Enumerable.Range(0, 64).Select(index =>
            plane[Math.Min(top + index / 8, height - 1) * width + Math.Min(left + index % 8, width - 1)] - 128f).ToArray();

    private static short[] Original(float[] samples, Quantization.PreparedTable table, float strength, bool ordered)
    {
        float[] coefficients = new float[64];
        short[] natural = new short[64], result = new short[64];
        ForwardDct.TransformScalar(samples, coefficients);
        Quantization.QuantizePrepared(coefficients, table, natural, strength);
        EntropyEncoder.ValidateAc(natural);
        if (ordered)
            ZigZagOrdering.Reorder(natural, result);
        return ordered ? result : natural;
    }
}
