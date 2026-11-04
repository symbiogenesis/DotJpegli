namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class XybByteLinearizationTests
{
    [Fact]
    public void ByteLookupMatchesDirectTransferBitsForEveryPairOfChannelValues()
    {
        for (int red = 0; red < 256; red++)
        {
            for (int green = 0; green < 256; green++)
            {
                byte blue = (byte)((red * 73) + (green * 151));
                XybColorConversion.SrgbToScaledXyb(Normalize((byte)red), Normalize((byte)green), Normalize(blue),
                    out float expectedX, out float expectedY, out float expectedB);
                XybColorConversion.SrgbBytesToScaledXyb((byte)red, (byte)green, blue,
                    out float actualX, out float actualY, out float actualB);
                AssertBits(expectedX, actualX);
                AssertBits(expectedY, actualY);
                AssertBits(expectedB, actualB);
            }
        }
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24)]
    [InlineData(PixelLayout.Bgr24)]
    [InlineData(PixelLayout.Rgba32)]
    [InlineData(PixelLayout.Bgra32)]
    [InlineData(PixelLayout.Gray8)]
    public void RawAndCachedByteReadersAndCompleteJpegsMatchNormalizedFloatInput(PixelLayout layout)
    {
        const int width = 19, height = 17;
        int channels = layout == PixelLayout.Gray8 ? 1 : layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
        int stride = (width * channels) + 7;
        byte[] bytes = new byte[(stride * height) + 11];
        float[] floats = new float[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)((index * 73) + (index / 13));
            floats[index] = Normalize(bytes[index]);
        }
        PixelSource source = new PixelSource(bytes, layout).WithColorSpace(JpegColorSpace.Xyb);
        PixelSource reference = new PixelSource(floats, (PixelComponentLayout)(int)layout).WithColorSpace(JpegColorSpace.Xyb);
        PixelSource cached = source.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? storage = cached.ConvertedStorage;
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                int offset = (row * stride) + (column * channels);
                for (int component = 0; component < 3; component++)
                {
                    float expected = reference.ReadSample(offset, component);
                    AssertBits(expected, source.ReadSample(offset, component));
                    AssertBits(expected, cached.ReadSample(offset, component));
                }
            }
        }

        var options = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.Xyb,
            IccProfile = new byte[] { 1, 2, 3 },
            Progressive = true,
            ProgressiveRefinementBits = 2,
            RestartInterval = 2,
            SmoothingFactor = 37
        };
        foreach (long budget in new[] { 0L, 64L * 1024 * 1024 })
        {
            JpegliEncoderOptions selected = options with { MaxCacheBytes = budget };
            byte[] expected = JpegliEncoder.Encode(floats, width, height, (PixelComponentLayout)(int)layout,
                selected, stride, TestContext.Current.CancellationToken);
            Assert.Equal(expected, JpegliEncoder.Encode(bytes, width, height, layout,
                selected, stride, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public void AlreadyScaledByteXybBypassesSrgbTransfer()
    {
        byte[] samples = [11, 128, 255];
        var source = new PixelSource(samples, PixelLayout.Xyb24);
        for (int component = 0; component < 3; component++)
        {
            AssertBits(samples[component], source.ReadSample(0, component));
        }
    }

    private static float Normalize(byte value) => (float)(value * (1.0 / 255.0));

    private static void AssertBits(float expected, float actual)
        => Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
