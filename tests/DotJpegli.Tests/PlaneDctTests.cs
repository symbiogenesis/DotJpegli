using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class PlaneDctTests
{
    [Theory]
    [InlineData(8, 0, 0)]
    [InlineData(13, 3, 2)]
    [InlineData(41, 17, 3)]
    public void StridedPlaneMatchesSeparateCenteringAndScalarDctBits(int stride, int left, int top)
    {
        var random = new Random(4917);
        for (int block = 0; block < 100; block++)
        {
            float[] plane = Enumerable.Repeat(float.NaN, (top + 7) * stride + left + 8).ToArray();
            float[] centered = new float[64];
            for (int row = 0; row < 8; row++)
                for (int column = 0; column < 8; column++)
                {
                    int index = row * 8 + column;
                    float sample = block < 64 ? index == block ? 231.375f : 128f
                        : block < 80 ? (float)(random.NextDouble() * 256)
                        : MathF.ScaleB((float)(random.NextDouble() * 2 - 1), random.Next(-149, 116));
                    plane[(top + row) * stride + left + column] = sample;
                    centered[index] = sample - 128f;
                }
            float[] expected = new float[64];
            float[] actual = Enumerable.Repeat(-12345.625f, 72).ToArray();
            ForwardDct.TransformScalar(centered, expected);
            ForwardDct.TransformPlane(plane, stride, left, top, actual);
            AssertBits(expected, actual.AsSpan(0, 64));
            Assert.All(actual[64..], sample => Assert.Equal(-12345.625f, sample));
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 13)]
    [InlineData(13, 0)]
    [InlineData(0, 64)]
    [InlineData(64, 0)]
    [InlineData(0, 120)]
    public void PlaneAndOutputMayOverlapWithoutChangingAnyOtherStorage(int planeStart, int outputStart)
    {
        const int stride = 13, left = 3, top = 2;
        const int length = (top + 7) * stride + left + 8;
        float[] buffer = Enumerable.Range(0, Math.Max(planeStart + length, outputStart + 72))
            .Select(index => (index - 31.5f) * ((index % 7) + 0.125f)).ToArray();
        float[] original = (float[])buffer.Clone();
        float[] centered = new float[64];
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                centered[row * 8 + column] = buffer[planeStart + (top + row) * stride + left + column] - 128f;
        float[] expected = new float[64];
        ForwardDct.TransformScalar(centered, expected);
        ForwardDct.TransformPlane(buffer.AsSpan(planeStart, length), stride, left, top, buffer.AsSpan(outputStart));
        AssertBits(expected, buffer.AsSpan(outputStart, 64));
        for (int index = 0; index < buffer.Length; index++)
            if (index < outputStart || index >= outputStart + 64)
                Assert.Equal(BitConverter.SingleToInt32Bits(original[index]), BitConverter.SingleToInt32Bits(buffer[index]));
    }

    [Theory]
    [InlineData(7, 0, 0, 64, 64, "stride")]
    [InlineData(8, -1, 0, 64, 64, "left")]
    [InlineData(8, 0, -1, 64, 64, "top")]
    [InlineData(8, 1, 0, 64, 64, "left")]
    [InlineData(13, 3, 2, 127, 64, "plane")]
    [InlineData(8, 0, 0, 64, 63, "coefficients")]
    [InlineData(int.MaxValue, 0, int.MaxValue, 64, 64, "plane")]
    public void InvalidPlaneArgumentsAreRejectedBeforeWriting(int stride, int left, int top,
        int planeLength, int outputLength, string parameter)
    {
        float[] plane = new float[planeLength];
        float[] output = Enumerable.Repeat(-12345.625f, outputLength).ToArray();
        ArgumentException error = Assert.ThrowsAny<ArgumentException>(() =>
            ForwardDct.TransformPlane(plane, stride, left, top, output));
        Assert.Equal(parameter, error.ParamName);
        Assert.All(output, sample => Assert.Equal(-12345.625f, sample));
    }

    private static void AssertBits(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual)
    {
        for (int index = 0; index < expected.Length; index++)
            Assert.True(BitConverter.SingleToInt32Bits(expected[index]) == BitConverter.SingleToInt32Bits(actual[index]),
                $"Coefficient {index}: expected {expected[index]:R}, actual {actual[index]:R}.");
    }
}
