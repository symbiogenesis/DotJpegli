using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ThreeRowDownsamplingTests
{
    // Independent scalar C++ extraction of ExtRGBToYCbCr, ApplyInputSmoothing,
    // DownsampleRow2x1, Downsample1x3 and Downsample2x3 from google/jpegli
    // 031a0077f5799a6041004267fc12b956c1f52a20. MSVC 14.51.36231 /O2 /fp:strict,
    // with std::fma for chroma. It converts real pixels into three planes,
    // pads those planes, filters them, then invokes the explicit kernels.
    // The pinned upstream method selector dispatches its vertical-factor-three
    // case to two-row kernels; these fixtures describe the explicit three-row
    // kernels and deliberately do not claim parity with that selector.
    [Theory]
    [InlineData(1, 0, 0xC1CA44F0u)]
    [InlineData(1, 1, 0xC0FDB800u)]
    [InlineData(1, 2, 0xC254383Cu)]
    [InlineData(2, 0, 0xC0CABB00u)]
    [InlineData(2, 1, 0xC16ABEA8u)]
    [InlineData(2, 2, 0xC1AEFA08u)]
    public void ThreeRowSamplingMatchesExplicitNativeKernel(int sampleWidth,
        int component, uint expectedBits)
    {
        byte[] rgb =
        [
            59, 24, 93, 14, 189, 158, 12, 217, 15,
            139, 140, 27, 209, 37, 242, 113, 247, 39
        ];
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb.AsSpan(0, sampleWidth * 3 * 3),
            sampleWidth, 3, sampleWidth * 3, 3, PixelLayout.Rgb24,
            component, 0, 0, sampleWidth, 3, samples);
        // 2x3 rounds each horizontal pair before scaling the three-row sum.
        // A flat six-pixel average, or ignoring row three, changes these bits.
        Assert.Equal(expectedBits, BitConverter.SingleToUInt32Bits(samples[0]));
    }

    [Theory]
    [InlineData(1, 0, 0x0E811177u)]
    [InlineData(1, 1, 0xF20CE8B8u)]
    [InlineData(1, 2, 0x18836FD3u)]
    [InlineData(2, 0, 0xF61592C9u)]
    [InlineData(2, 1, 0xE3762293u)]
    [InlineData(2, 2, 0xC1E47665u)]
    public void OddDimensionsReplicateSamplesBeforeThreeRowAveraging(int sampleWidth,
        int component, uint expectedHash)
    {
        const int width = 5, height = 5, stride = width * 3 + 11;
        byte[] pixels = PaddedPattern(width, height, stride, 3, PixelLayout.Rgb24);
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(pixels, width, height, stride, 3, PixelLayout.Rgb24,
            component, 0, 0, sampleWidth, 3, samples);
        Assert.Equal(expectedHash, HashBits(samples));
    }

    // Hash all 64 float bit patterns in the first, interior and edge blocks.
    [Theory]
    [InlineData(1, 0, 0, 0x2E2DAA0Bu, 0x0667A783u, 0xE3818B7Bu)]
    [InlineData(1, 0, 1, 0xC43D8ED3u, 0x6D24A0A9u, 0x41A847E1u)]
    [InlineData(1, 0, 37, 0xA9D66C9Fu, 0xB85D4885u, 0xCF1B9331u)]
    [InlineData(1, 0, 100, 0xBFA49885u, 0xCC45A7B5u, 0x34E358C5u)]
    [InlineData(1, 1, 0, 0xBABF678Fu, 0x5CB59EC4u, 0x4BECC810u)]
    [InlineData(1, 1, 1, 0xC098073Cu, 0xE0A44F8Bu, 0xF45E2634u)]
    [InlineData(1, 1, 37, 0xF291C399u, 0x203ECA67u, 0xACF0C08Bu)]
    [InlineData(1, 1, 100, 0x891B0A99u, 0x998EB003u, 0xF68201C3u)]
    [InlineData(1, 2, 0, 0xE00087FDu, 0xAC4286E2u, 0x874DE583u)]
    [InlineData(1, 2, 1, 0x901B9C4Fu, 0x186EDD9Eu, 0x26FB61ADu)]
    [InlineData(1, 2, 37, 0xE6FD87B3u, 0xB675CADBu, 0xB15FBE5Du)]
    [InlineData(1, 2, 100, 0xC89A55A5u, 0xCF94FDF5u, 0xD996038Fu)]
    [InlineData(2, 0, 0, 0x0668442Fu, 0x1B971737u, 0x511F3F37u)]
    [InlineData(2, 0, 1, 0x779BBEDBu, 0xE71DC891u, 0xA8ABC879u)]
    [InlineData(2, 0, 37, 0x782ECFFBu, 0xAF7DD861u, 0xE556228Du)]
    [InlineData(2, 0, 100, 0xC73E926Du, 0xDA4DB4B5u, 0x702101CDu)]
    [InlineData(2, 1, 0, 0x2CB4BC19u, 0x74A02DB6u, 0x355F3F6Du)]
    [InlineData(2, 1, 1, 0xFDF56BBEu, 0xDA1F03D7u, 0x4A6E2CA9u)]
    [InlineData(2, 1, 37, 0x51B4401Fu, 0xA6ED9E45u, 0x2978CDA7u)]
    [InlineData(2, 1, 100, 0xD17F5435u, 0x09DA2341u, 0x690547ABu)]
    [InlineData(2, 2, 0, 0x31AD1BC5u, 0x3B9717BDu, 0x9F5F1847u)]
    [InlineData(2, 2, 1, 0x1F7375A3u, 0x003FA019u, 0xFE5E8DA2u)]
    [InlineData(2, 2, 37, 0xBECBB939u, 0xB5737045u, 0xD0F454D9u)]
    [InlineData(2, 2, 100, 0x520D96CDu, 0x4D8BA875u, 0x119D8B4Fu)]
    public void SmoothedPaddedLayoutsAndBlockSeamsMatchExplicitNativeKernels(int sampleWidth,
        int component, int smoothingFactor, uint firstBlock, uint seamBlock, uint edgeBlock)
    {
        const int width = 67, height = 67;
        foreach (PixelLayout layout in new[]
            { PixelLayout.Rgb24, PixelLayout.Bgr24, PixelLayout.Rgba32, PixelLayout.Bgra32 })
        {
            int bytesPerPixel = layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
            int stride = width * bytesPerPixel + 11;
            byte[] pixels = PaddedPattern(width, height, stride, bytesPerPixel, layout);
            AssertBlocks(pixels, layout, width, height, stride, bytesPerPixel,
                sampleWidth, component, smoothingFactor, firstBlock, seamBlock, edgeBlock);
            // Ignore alpha and row padding, including an unpadded final row.
            for (int y = 0; y < height; y++)
            {
                for (int offset = y * stride + width * bytesPerPixel;
                    offset < Math.Min((y + 1) * stride, pixels.Length); offset++)
                    pixels[offset] ^= 0xff;
                if (bytesPerPixel == 4)
                    for (int x = 0; x < width; x++)
                        pixels[y * stride + x * bytesPerPixel + 3] ^= 0xff;
            }
            AssertBlocks(pixels, layout, width, height, stride, bytesPerPixel,
                sampleWidth, component, smoothingFactor, firstBlock, seamBlock, edgeBlock);
        }
    }

    [Theory]
    [InlineData(1, 0f)]
    [InlineData(1, 50f)]
    [InlineData(1, 85f)]
    [InlineData(1, 100f)]
    [InlineData(2, 0f)]
    [InlineData(2, 50f)]
    [InlineData(2, 85f)]
    [InlineData(2, 100f)]
    public void ThreeRowSamplingKeepsNon420PerceptualQuantization(int sampleWidth, float quality)
    {
        // SetQuantMatrices compensates exclusively for 2x2 sampling.
        ushort[][] expected = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(quality, component, 1, 1)).ToArray();
        ushort[][] actual = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(quality, component, sampleWidth, 3)).ToArray();
        float[][] expectedMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(expected, 1, 1, false);
        float[][] actualMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(actual, sampleWidth, 3, false);
        for (int component = 0; component < 3; component++)
        {
            Assert.Equal(expected[component], actual[component]);
            Assert.Equal(expectedMultipliers[component], actualMultipliers[component]);
        }
    }

    private static void AssertBlocks(byte[] pixels, PixelLayout layout, int width, int height,
        int stride, int bytesPerPixel, int sampleWidth, int component,
        int smoothingFactor, uint firstBlock, uint seamBlock, uint edgeBlock)
    {
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 0, 0, sampleWidth, 3, samples, smoothingFactor);
        Assert.Equal(firstBlock, HashBits(samples));
        // Filtering uses real pixels beyond every side of the interior block.
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 8 * sampleWidth, 24, sampleWidth, 3, samples, smoothingFactor);
        Assert.Equal(seamBlock, HashBits(samples));
        // Align the partial final block to the component sampling grid.
        int edgeLeft = width / (8 * sampleWidth) * (8 * sampleWidth);
        int edgeTop = height / 24 * 24;
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, edgeLeft, edgeTop, sampleWidth, 3, samples, smoothingFactor);
        Assert.Equal(edgeBlock, HashBits(samples));
    }

    private static byte[] PaddedPattern(int width, int height, int stride, int bytesPerPixel, PixelLayout layout)
    {
        byte[] pixels = Enumerable.Repeat((byte)0xed, (height - 1) * stride + width * bytesPerPixel).ToArray();
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = y * stride + x * bytesPerPixel;
                pixels[offset + (bgr ? 2 : 0)] = (byte)((x * 73 + y * 29 + 11) & 255);
                pixels[offset + 1] = (byte)((x * 17 + y * 97 + 83) & 255);
                pixels[offset + (bgr ? 0 : 2)] = (byte)((x * 43 + y * 7 + 191) & 255);
                if (bytesPerPixel == 4)
                    pixels[offset + 3] = (byte)((x * 17 + y * 29) & 255);
            }
        return pixels;
    }

    private static uint HashBits(ReadOnlySpan<float> samples)
    {
        uint hash = 2166136261u;
        foreach (float sample in samples)
            hash = unchecked((hash ^ BitConverter.SingleToUInt32Bits(sample)) * 16777619u);
        return hash;
    }
}