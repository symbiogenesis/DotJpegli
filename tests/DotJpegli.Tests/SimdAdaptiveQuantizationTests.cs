using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SimdAdaptiveQuantizationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void VectorAndDispatchPreserveScalarStrengthBits(int kind)
    {
        const int height = 35;
        foreach (int width in new[] { 1, 3, 8, 9, 19, 32, 67 })
        {
            int components = kind == 0 ? 1 : kind == 2 ? 4 : 3;
            int stride = width * components + 7;
            byte[] bytes = new byte[stride * height];
            ushort[] words = new ushort[bytes.Length];
            float[] floats = new float[bytes.Length];
            var random = new Random(2917 + width);
            random.NextBytes(bytes);
            for (int i = 0; i < bytes.Length; i++)
            {
                words[i] = (ushort)random.Next(65536);
                floats[i] = kind switch
                {
                    5 => (float)(random.NextDouble() * 16 - 8),
                    6 => MathF.ScaleB((float)(random.NextDouble() * 2 - 1), 115),
                    7 => BitConverter.Int32BitsToSingle((i % 2 == 0 ? int.MinValue : 0) | i),
                    _ => (float)random.NextDouble()
                };
            }
            PixelSource source = kind switch
            {
                0 => new PixelSource(bytes, PixelLayout.Gray8),
                1 => new PixelSource(bytes, PixelLayout.Rgb24),
                2 => new PixelSource(bytes, PixelLayout.Bgra32),
                3 => new PixelSource(words, PixelComponentLayout.Rgb),
                _ => new PixelSource(floats, PixelComponentLayout.Rgb)
            };
            foreach (JpegColorSpace colorSpace in new[] { JpegColorSpace.YCbCr, JpegColorSpace.Rgb })
            {
                PixelSource pixels = source.WithColorSpace(colorSpace);
                for (int sampling = 1; sampling <= 4; sampling++)
                {
                    var analysis = new AdaptiveQuantization(width, height, sampling);
                    float[] expected = new float[analysis.BlocksWide * sampling];
                    float[] actual = new float[expected.Length + 1];
                    // Reverse order also checks that the SIMD buffers never
                    // depend on a previously analyzed MCU neighborhood.
                    for (int top = (height - 1) / (sampling * 8) * sampling * 8; top >= 0; top -= sampling * 8)
                        foreach (int quant in new[] { 4, 18, 65 })
                        {
                            analysis.ComputeRowScalar(pixels, stride, top, quant, expected,
                                TestContext.Current.CancellationToken);
                            actual[^1] = -123.25f;
                            analysis.ComputeRowVector(pixels, stride, top, quant, actual,
                                TestContext.Current.CancellationToken);
                            AssertSameBits(expected, actual.AsSpan(0, expected.Length));
                            Assert.Equal(-123.25f, actual[^1]);
                            analysis.ComputeRow(pixels, stride, top, quant, actual,
                                TestContext.Current.CancellationToken);
                            AssertSameBits(expected, actual.AsSpan(0, expected.Length));
                        }
                }
            }
        }
    }

    private static void AssertSameBits(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual)
    {
        for (int i = 0; i < expected.Length; i++)
            Assert.True(BitConverter.SingleToInt32Bits(expected[i]) == BitConverter.SingleToInt32Bits(actual[i]),
                $"Strength {i}: expected {expected[i]:R}, actual {actual[i]:R}.");
    }
}
