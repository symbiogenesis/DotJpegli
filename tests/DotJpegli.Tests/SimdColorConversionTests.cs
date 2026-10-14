using System.Numerics;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class SimdColorConversionTests
{
    [Fact]
    public void VectorTransformPreservesScalarBitsForFractionalAndExtremeFiniteInputs()
    {
        int lanes = Vector<float>.Count;
        float[] red = new float[lanes], green = new float[lanes], blue = new float[lanes];
        var random = new Random(4927);
        for (int group = 0; group < 96; group++)
        {
            for (int lane = 0; lane < lanes; lane++)
            {
                red[lane] = Sample();
                green[lane] = Sample();
                blue[lane] = Sample();
            }
            ColorConversion.RgbToYCbCr(new Vector<float>(red), new Vector<float>(green), new Vector<float>(blue),
                out Vector<float> y, out Vector<float> cb, out Vector<float> cr);
            for (int lane = 0; lane < lanes; lane++)
            {
                ColorConversion.RgbToYCbCr(red[lane], green[lane], blue[lane],
                    out float expectedY, out float expectedCb, out float expectedCr);
                SimdSampleFixtures.AssertBits(expectedY, y[lane]);
                SimdSampleFixtures.AssertBits(expectedCb, cb[lane]);
                SimdSampleFixtures.AssertBits(expectedCr, cr[lane]);
            }

            float Sample() => group switch
            {
                0 => random.Next(2) == 0 ? -0f : 0f,
                1 => BitConverter.Int32BitsToSingle(random.Next(1, 100) | (random.Next(2) == 0 ? 0 : int.MinValue)),
                < 48 => (float)(random.NextDouble() * 512 - 128),
                _ => MathF.ScaleB((float)(random.NextDouble() * 2 - 1), random.Next(-125, 111))
            };
        }
    }

    [Fact]
    public void VectorTransformMatchesIndependentFullRangeBt601Definition()
    {
        int lanes = Vector<float>.Count;
        float[] red = new float[lanes], green = new float[lanes], blue = new float[lanes];
        for (int group = 0; group < 8; group++)
        {
            for (int lane = 0; lane < lanes; lane++)
            {
                int index = group * lanes + lane;
                red[lane] = index % 7 == 0 ? 255f : (index * 79 % 256) + 0.125f;
                green[lane] = index % 7 == 0 ? 0f : (index * 43 % 256) + 0.25f;
                blue[lane] = index % 7 == 0 ? 0f : (index * 137 % 256) + 0.375f;
            }
            ColorConversion.RgbToYCbCr(new Vector<float>(red), new Vector<float>(green), new Vector<float>(blue),
                out Vector<float> y, out Vector<float> cb, out Vector<float> cr);
            for (int lane = 0; lane < lanes; lane++)
            {
                double luma = 0.299 * red[lane] + 0.587 * green[lane] + 0.114 * blue[lane];
                Assert.InRange(Math.Abs(y[lane] - luma), 0, 0.00006);
                Assert.InRange(Math.Abs(cb[lane] - (128 + (blue[lane] - luma) / 1.772)), 0, 0.00006);
                Assert.InRange(Math.Abs(cr[lane] - (128 + (red[lane] - luma) / 1.402)), 0, 0.00006);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void VectorRowsMatchIndividualScalarReadsForEverySourceAndOutput(int kind)
    {
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
            foreach ((int width, int height) in new[] { (1, 1), (19, 3) })
            {
                int stride = width * SimdSampleFixtures.Channels(layout) + 11;
                Array source = SimdSampleFixtures.Source(kind, width, height, stride, layout);
                PixelSource pixels = SimdSampleFixtures.Pixels(source, layout);
                foreach (JpegColorSpace colorSpace in SimdSampleFixtures.OutputSpaces(layout))
                    AssertRows(pixels.WithColorSpace(colorSpace), width, height, stride);
                Assert.Equal(source, SimdSampleFixtures.Source(kind, width, height, stride, layout));
            }
    }

    private static void AssertRows(PixelSource pixels, int width, int height, int stride)
    {
        foreach (int length in new[] { 0, Vector<float>.Count - 1, Vector<float>.Count, Vector<float>.Count + 1, 32 })
            foreach (int x in new[] { -2, 0, width - 2 })
                foreach (int y in new[] { -1, height - 1, height + 1 })
                    for (int component = 0; component < (pixels.IsGrayscale ? 1 : 3); component++)
                    {
                        float[] scalar = Enumerable.Repeat(-54321.125f, length + 3).ToArray();
                        float[] vector = (float[])scalar.Clone();
                        pixels.ReadRowScalar(width, height, stride, component, x, y, scalar.AsSpan(0, length));
                        pixels.ReadRowVector(width, height, stride, component, x, y, vector.AsSpan(0, length));
                        for (int column = 0; column < length; column++)
                        {
                            int offset = Math.Clamp(y, 0, height - 1) * stride
                                + Math.Clamp(x + column, 0, width - 1) * pixels.ComponentsPerPixel;
                            SimdSampleFixtures.AssertBits(pixels.ReadSample(offset, component), scalar[column]);
                            SimdSampleFixtures.AssertBits(scalar[column], vector[column]);
                        }
                        for (int column = length; column < vector.Length; column++)
                            SimdSampleFixtures.AssertBits(-54321.125f, vector[column]);
                    }
    }
}

internal static class SimdSampleFixtures
{
    internal static Array Source(int kind, int width, int height, int stride, PixelComponentLayout layout)
    {
        int channels = Channels(layout);
        int length = (height - 1) * stride + width * channels;
        Array source = kind switch
        {
            0 => Enumerable.Repeat(byte.MaxValue, length).ToArray(),
            1 => Enumerable.Repeat(ushort.MaxValue, length).ToArray(),
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int component = 0; component < Math.Min(channels, 3); component++)
                {
                    int offset = y * stride + x * channels + component;
                    int value = (x * 1877 + y * 10307 + component * 23131 + 17) % 65536;
                    if (source is byte[] bytes)
                        bytes[offset] = (byte)(value % 256);
                    else if (source is ushort[] words)
                        words[offset] = (ushort)value;
                    else
                        ((float[])source)[offset] = value / 65535f;
                }
        return source;
    }

    internal static PixelSource Pixels(Array source, PixelComponentLayout layout) => source switch
    {
        byte[] bytes => new PixelSource(bytes, layout switch
        {
            PixelComponentLayout.Rgb => PixelLayout.Rgb24,
            PixelComponentLayout.Bgr => PixelLayout.Bgr24,
            PixelComponentLayout.Rgba => PixelLayout.Rgba32,
            PixelComponentLayout.Bgra => PixelLayout.Bgra32,
            PixelComponentLayout.Gray => PixelLayout.Gray8,
            PixelComponentLayout.Xyb => PixelLayout.Xyb24,
            _ => PixelLayout.YCbCr24
        }),
        ushort[] words => new PixelSource(words, layout),
        _ => new PixelSource((float[])source, layout)
    };

    internal static IEnumerable<JpegColorSpace> OutputSpaces(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Xyb => [JpegColorSpace.Automatic, JpegColorSpace.Xyb],
        PixelComponentLayout.YCbCr => [JpegColorSpace.Automatic, JpegColorSpace.YCbCr, JpegColorSpace.Grayscale],
        _ => Enum.GetValues<JpegColorSpace>()
    };

    internal static int Channels(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    internal static void AssertBits(float expected, float actual) => Assert.True(
        BitConverter.SingleToInt32Bits(expected) == BitConverter.SingleToInt32Bits(actual),
        $"Expected {expected:R} ({BitConverter.SingleToUInt32Bits(expected):X8}), actual {actual:R} ({BitConverter.SingleToUInt32Bits(actual):X8}).");
}
