using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class AdditionalDownsamplingTests
{
    // Independently compiled scalar C++ extraction of ExtRGBToYCbCr,
    // ApplyInputSmoothing, Downsample1x2 and DownsampleRow4x1 from
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    // MSVC 14.51.36231 /O2 /fp:strict; chroma uses std::fma as upstream does.
    [Theory]
    [InlineData(1, 2, 1, 0x41AA7DF8u)]
    [InlineData(1, 2, 2, 0xC2122E04u)]
    [InlineData(4, 1, 1, 0xC1A03C18u)]
    [InlineData(4, 1, 2, 0xC21679FAu)]
    public void AdditionalSamplingMatchesNativeArithmetic(int sampleWidth,
        int sampleHeight, int component, uint expectedBits)
    {
        byte[] rgb = sampleWidth == 4
            ? [59, 24, 93, 14, 189, 158, 12, 217, 15, 139, 140, 27]
            : [59, 24, 93, 14, 189, 158];
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, sampleWidth, sampleHeight, sampleWidth * 3,
            3, PixelLayout.Rgb24, component, 0, 0, sampleWidth, sampleHeight, samples);
        // Sequential four-pixel accumulation gives C1A03C1C / C21679F8,
        // rather than upstream's pairwise four-pixel sum.
        Assert.Equal(expectedBits, BitConverter.SingleToUInt32Bits(samples[0]));
    }

    [Theory]
    [InlineData(1, 2, 1)]
    [InlineData(1, 2, 2)]
    [InlineData(4, 1, 1)]
    [InlineData(4, 1, 2)]
    public void OddDimensionsReplicateNativeSamples(int sampleWidth,
        int sampleHeight, int component)
    {
        byte[] rgb =
        [
            255, 0, 0, 0, 255, 0, 0, 0, 255,
            255, 255, 0, 0, 255, 255, 255, 0, 255,
            13, 91, 247, 203, 47, 117, 37, 191, 59
        ];
        uint[] expected = (sampleWidth, component) switch
        {
            (1, 1) => [0xC2AA8713u, 0xC1A5C75Cu, 0x42D3F8EAu, 0x42B652A2u, 0x410AD5D0u, 0xC2200F06u],
            (1, 2) => [0x42943C14u, 0xC2EA43EEu, 0x422C0FB4u, 0xC24EBD2Cu, 0x42909DC4u, 0xC2848896u],
            (4, 1) => [0x41FF0000u, 0x42FEFFFEu, 0x41A8F1D8u, 0x42A8F1D6u, 0x409E7960u, 0xC2200F06u],
            _ => [0xC0A5E090u, 0xC1A5E098u, 0x41D587D8u, 0x42D587DAu, 0xC1DFD1FCu, 0xC2848896u]
        };
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, 3, 3, 9, 3, PixelLayout.Rgb24, component,
            0, 0, sampleWidth, sampleHeight, samples);
        // In 4x1, the first group averages three real pixels and one padded
        // pixel; later groups contain only repetitions of the final raw pixel.
        int fixtureColumns = sampleWidth == 4 ? 2 : 3;
        int fixtureRows = (3 + sampleHeight - 1) / sampleHeight;
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                Assert.Equal(expected[Math.Min(row, fixtureRows - 1) * fixtureColumns
                    + Math.Min(column, fixtureColumns - 1)],
                    BitConverter.SingleToUInt32Bits(samples[row * 8 + column]));
    }

    // Each fixture hashes all 64 centered float bit patterns in three blocks.
    [Theory]
    [InlineData(1, 2, 0, 0, 0xACFFABC0u, 0x3CA4D5A9u, 0x32EF4AEDu)]
    [InlineData(1, 2, 0, 1, 0x6328C3A1u, 0xDD6FFFEBu, 0x1C3E3E5Du)]
    [InlineData(1, 2, 0, 37, 0x1DE94335u, 0xD942E3C7u, 0x010A9565u)]
    [InlineData(1, 2, 0, 100, 0x50DF75E1u, 0x68E639D9u, 0x8B043945u)]
    [InlineData(1, 2, 1, 0, 0x5724F2C5u, 0x657FA28Du, 0x074AC1F5u)]
    [InlineData(1, 2, 1, 1, 0x31F9323Eu, 0xE9BAAE1Du, 0x513E947Du)]
    [InlineData(1, 2, 1, 37, 0x20B13357u, 0x22D4F1D5u, 0xFEED21FDu)]
    [InlineData(1, 2, 1, 100, 0x787DFC9Fu, 0x4F95188Du, 0x213FCB6Du)]
    [InlineData(1, 2, 2, 0, 0x78557293u, 0x29668BB0u, 0xA4463869u)]
    [InlineData(1, 2, 2, 1, 0xCDA32969u, 0xCA0B97FFu, 0x028FBBE7u)]
    [InlineData(1, 2, 2, 37, 0x948ADC23u, 0xFDDEF875u, 0x38064891u)]
    [InlineData(1, 2, 2, 100, 0xDDCDD379u, 0xFD85F18Du, 0x465AFE6Du)]
    [InlineData(4, 1, 0, 0, 0xA62F8C9Bu, 0xBFCFC875u, 0x36D2FB57u)]
    [InlineData(4, 1, 0, 1, 0xFA67AF79u, 0x29B92209u, 0xABD39FE7u)]
    [InlineData(4, 1, 0, 37, 0xA758D883u, 0x1EE8559Bu, 0x0CE824D9u)]
    [InlineData(4, 1, 0, 100, 0xCCBB7D8Du, 0xD89F322Du, 0xAC7CDBA9u)]
    [InlineData(4, 1, 1, 0, 0xE6E49FF5u, 0x98CC0205u, 0x9B84C2E5u)]
    [InlineData(4, 1, 1, 1, 0xEB104F79u, 0xE58E608Du, 0xF84B1195u)]
    [InlineData(4, 1, 1, 37, 0x591ACAA3u, 0x1AB7AB05u, 0x664C0125u)]
    [InlineData(4, 1, 1, 100, 0xB291549Du, 0x66F34769u, 0xE267E94Du)]
    [InlineData(4, 1, 2, 0, 0xD4E104B3u, 0x962DB28Fu, 0xBA15A415u)]
    [InlineData(4, 1, 2, 1, 0xD03F3959u, 0x98BF3151u, 0x45AC36E3u)]
    [InlineData(4, 1, 2, 37, 0x1EDA4343u, 0xD7F65F03u, 0x384BAA09u)]
    [InlineData(4, 1, 2, 100, 0xB2CF7F11u, 0x24A69E8Du, 0x2092717Du)]
    public void SmoothedPaddedLayoutsAndBlockSeamsMatchNativeSamples(int sampleWidth,
        int sampleHeight, int component, int smoothingFactor, uint firstBlock,
        uint seamBlock, uint edgeBlock)
    {
        const int width = 67, height = 35;
        foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Bgr24, PixelLayout.Rgba32 })
        {
            int bytesPerPixel = layout == PixelLayout.Rgba32 ? 4 : 3;
            int stride = width * bytesPerPixel + 11;
            byte[] pixels = PaddedPattern(width, height, stride, bytesPerPixel, layout);
            AssertBlocks(pixels, layout, width, height, stride, bytesPerPixel,
                sampleWidth, sampleHeight, component, smoothingFactor, firstBlock, seamBlock, edgeBlock);
            // Change every padding/alpha byte and retain the same native samples.
            // The final row deliberately has no trailing padding at all.
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
                sampleWidth, sampleHeight, component, smoothingFactor, firstBlock, seamBlock, edgeBlock);
        }
    }

    [Theory]
    [InlineData(1, 2, 0f)]
    [InlineData(1, 2, 50f)]
    [InlineData(1, 2, 85f)]
    [InlineData(1, 2, 100f)]
    [InlineData(4, 1, 0f)]
    [InlineData(4, 1, 50f)]
    [InlineData(4, 1, 85f)]
    [InlineData(4, 1, 100f)]
    public void AdditionalSamplingKeepsNon420Quantization(int sampleWidth, int sampleHeight, float quality)
    {
        // Upstream SetQuantMatrices applies sampling compensation only to 2x2;
        // 1x2 and 4x1 keep the 4:4:4 matrices and adaptive frequency weights.
        ushort[][] expected = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(quality, component)).ToArray();
        ushort[][] actual = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(quality, component, sampleWidth, sampleHeight)).ToArray();
        float[][] expectedMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(expected, 1, 1, false);
        float[][] actualMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(actual, sampleWidth, sampleHeight, false);
        for (int component = 0; component < 3; component++)
        {
            Assert.Equal(expected[component], actual[component]);
            Assert.Equal(expectedMultipliers[component], actualMultipliers[component]);
        }
    }

    private static void AssertBlocks(byte[] pixels, PixelLayout layout, int width, int height,
        int stride, int bytesPerPixel, int sampleWidth, int sampleHeight, int component,
        int smoothingFactor, uint firstBlock, uint seamBlock, uint edgeBlock)
    {
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 0, 0, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(firstBlock, HashBits(samples));
        // All seam pixels are real input. Filtering needs neighbors outside
        // each source block, before horizontal or vertical averaging.
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 8 * sampleWidth, 8 * sampleHeight, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(seamBlock, HashBits(samples));
        // This block includes the final real pixels, first padded pixels and
        // further padded pixels, which differ when smoothing is enabled.
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 64, 32, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(edgeBlock, HashBits(samples));
    }

    private static byte[] PaddedPattern(int width, int height, int stride, int bytesPerPixel, PixelLayout layout)
    {
        byte[] pixels = Enumerable.Repeat((byte)0xed, (height - 1) * stride + width * bytesPerPixel).ToArray();
        bool bgr = layout == PixelLayout.Bgr24;
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
