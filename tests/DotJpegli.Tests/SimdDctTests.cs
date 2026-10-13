using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SimdDctTests
{
    private delegate void DctTransform(ReadOnlySpan<float> samples, Span<float> coefficients);

    private static readonly DctTransform[] Transforms =
    [
        ForwardDct.TransformScalar,
        ForwardDct.TransformVector,
        ForwardDct.Transform
    ];

    [Fact]
    public void VectorAndDispatchMatchScalarBitsForFiniteBlocks()
    {
        foreach (float[] samples in SampleBlocks())
        {
            float[] original = (float[])samples.Clone();
            float[] expected = new float[64];
            float[] vector = new float[64];
            float[] dispatched = new float[64];
            ForwardDct.TransformScalar(samples, expected);
            ForwardDct.TransformVector(samples, vector);
            ForwardDct.Transform(samples, dispatched);
            AssertSameBits(expected, vector);
            AssertSameBits(expected, dispatched);
            AssertSameBits(original, samples);
        }
    }

    [Fact]
    public void VectorMatchesIndependentCosineDefinition()
    {
        foreach (float[] samples in SampleBlocks())
        {
            float[] actual = new float[64];
            ForwardDct.TransformVector(samples, actual);
            double tolerance = Math.Max(0.0002, samples.Max(value => Math.Abs(value)) * 0.000004);
            for (int v = 0; v < 8; v++)
                for (int u = 0; u < 8; u++)
                {
                    double sum = 0;
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            sum += samples[y * 8 + x]
                                * Math.Cos((2 * x + 1) * u * Math.PI / 16)
                                * Math.Cos((2 * y + 1) * v * Math.PI / 16);
                    double expected = sum * 0.25
                        * (u == 0 ? 1 / Math.Sqrt(2) : 1)
                        * (v == 0 ? 1 / Math.Sqrt(2) : 1);
                    Assert.InRange(Math.Abs(expected - actual[v * 8 + u]), 0, tolerance);
                }
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 7)]
    [InlineData(7, 0)]
    [InlineData(0, 32)]
    [InlineData(32, 0)]
    [InlineData(0, 63)]
    [InlineData(63, 0)]
    public void AllPathsSupportOverlappingSpansAndPreserveEverythingOutsideOutput(int inputStart, int outputStart)
    {
        float[] samples = Enumerable.Range(0, 64).Select(i => (i - 31.5f) * ((i % 3) + 0.125f)).ToArray();
        float[] expected = new float[64];
        ForwardDct.TransformScalar(samples, expected);
        foreach (DctTransform transform in Transforms)
        {
            float[] buffer = Enumerable.Repeat(-12345.625f, Math.Max(inputStart, outputStart) + 72).ToArray();
            samples.CopyTo(buffer, inputStart);
            float[] original = (float[])buffer.Clone();
            transform(buffer.AsSpan(inputStart, 64), buffer.AsSpan(outputStart));
            AssertSameBits(expected, buffer.AsSpan(outputStart, 64));
            for (int i = 0; i < buffer.Length; i++)
                if (i < outputStart || i >= outputStart + 64)
                    Assert.Equal(BitConverter.SingleToInt32Bits(original[i]), BitConverter.SingleToInt32Bits(buffer[i]));
        }
    }

    [Fact]
    public void AllPathsValidateLengthsBeforeWritingOutput()
    {
        foreach (DctTransform transform in Transforms)
            foreach (int inputLength in new[] { 0, 63, 64, 65 })
                foreach (int outputLength in new[] { 0, 63, 72 })
                {
                    if (inputLength == 64 && outputLength >= 64)
                        continue;
                    float[] samples = Enumerable.Repeat(1.25f, inputLength).ToArray();
                    float[] coefficients = Enumerable.Repeat(-54321.125f, outputLength).ToArray();
                    float[] original = (float[])coefficients.Clone();
                    ArgumentException error = Assert.Throws<ArgumentException>(() => transform(samples, coefficients));
                    Assert.Equal(inputLength != 64 ? "samples" : "coefficients", error.ParamName);
                    AssertSameBits(original, coefficients);
                }
    }

    private static IEnumerable<float[]> SampleBlocks()
    {
        yield return new float[64];
        yield return Enumerable.Range(0, 64).Select(i => i % 2 == 0 ? -0f : 0f).ToArray();
        yield return Enumerable.Repeat(40.125f, 64).ToArray();
        yield return Enumerable.Repeat(-127.875f, 64).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => (i / 8 + i % 8) % 2 == 0 ? 127.625f : -128.375f).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => (i - 31.5f) * 0.125f).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => (i % 5 - 2) * 16384.125f).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => BitConverter.Int32BitsToSingle(
            (i % 2 == 0 ? 0 : int.MinValue) | (i + 1))).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => MathF.ScaleB(i - 31.5f, -126)).ToArray();
        yield return Enumerable.Range(0, 64).Select(i => MathF.ScaleB(i - 31.5f, 110)).ToArray();
        for (int position = 0; position < 64; position++)
        {
            float[] impulse = new float[64];
            impulse[position] = position % 2 == 0 ? 103.375f : -89.125f;
            yield return impulse;
        }

        var random = new Random(2917);
        for (int block = 0; block < 48; block++)
        {
            float[] samples = new float[64];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = block < 24
                    ? (float)(random.NextDouble() * 256 - 128)
                    : MathF.ScaleB((float)(random.NextDouble() * 2 - 1), random.Next(-16, 17));
            yield return samples;
        }
    }

    private static void AssertSameBits(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.True(BitConverter.SingleToInt32Bits(expected[i]) == BitConverter.SingleToInt32Bits(actual[i]),
                $"Coefficient {i}: expected {expected[i]:R}, actual {actual[i]:R}.");
    }
}
