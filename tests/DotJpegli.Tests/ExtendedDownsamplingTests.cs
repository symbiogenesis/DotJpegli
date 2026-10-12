using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ExtendedDownsamplingTests
{
    // Independently compiled scalar C++ extraction of ExtRGBToYCbCr,
    // ApplyInputSmoothing, DownsampleRow3x1, Downsample3x2,
    // Downsample1x4, Downsample4x2 and Downsample2x4 from
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    // MSVC 14.51.36231 /O2 /fp:strict; chroma uses std::fma as upstream does.
    // The reference converts and pads separate planes before filtering and
    // downsampling, independently of the managed on-demand pixel reads.
    [Theory]
    [InlineData(3, 1, 0, 0xC1CA44F0u)]
    [InlineData(3, 1, 1, 0xC0FDB800u)]
    [InlineData(3, 1, 2, 0xC254383Cu)]
    [InlineData(3, 2, 0, 0xC0CABB00u)]
    [InlineData(3, 2, 1, 0xC16ABEB0u)]
    [InlineData(3, 2, 2, 0xC1AEFA04u)]
    [InlineData(1, 4, 0, 0xC19A1060u)]
    [InlineData(1, 4, 1, 0xC1A03C18u)]
    [InlineData(1, 4, 2, 0xC21679FAu)]
    [InlineData(4, 2, 0, 0xC114B230u)]
    [InlineData(4, 2, 1, 0xC098B690u)]
    [InlineData(4, 2, 2, 0xC152A170u)]
    [InlineData(2, 4, 0, 0xC114B230u)]
    [InlineData(2, 4, 1, 0xC098B690u)]
    [InlineData(2, 4, 2, 0xC152A170u)]
    public void ExtendedSamplingMatchesNativeArithmetic(int sampleWidth,
        int sampleHeight, int component, uint expectedBits)
    {
        byte[] rgb =
        [
            59, 24, 93, 14, 189, 158, 12, 217, 15, 139, 140, 27,
            209, 37, 242, 113, 247, 39, 223, 21, 97, 33, 163, 211
        ];
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb.AsSpan(0, sampleWidth * sampleHeight * 3),
            sampleWidth, sampleHeight, sampleWidth * 3, 3, PixelLayout.Rgb24,
            component, 0, 0, sampleWidth, sampleHeight, samples);
        // These chroma inputs distinguish reciprocal multiplication, paired
        // four-sample sums, and horizontal scaling before vertical averaging
        // from one flat box sum. For example, flat 3x2 Cb is C16ABEC0.
        Assert.Equal(expectedBits, BitConverter.SingleToUInt32Bits(samples[0]));
    }

    // Hash every centered float bit pattern, including partial sample groups
    // and all repeated right/bottom pixels of a small odd-sized image.
    [Theory]
    [InlineData(3, 1, 0, 0xA0F385B7u)]
    [InlineData(3, 1, 1, 0xEBF9DF0Fu)]
    [InlineData(3, 1, 2, 0x1D391D62u)]
    [InlineData(3, 2, 0, 0xE1F974DDu)]
    [InlineData(3, 2, 1, 0xE32F65B5u)]
    [InlineData(3, 2, 2, 0xE62E8DC9u)]
    [InlineData(1, 4, 0, 0x8CEEAF84u)]
    [InlineData(1, 4, 1, 0x3CA7DA07u)]
    [InlineData(1, 4, 2, 0xC1E6718Bu)]
    [InlineData(4, 2, 0, 0xDCA23B29u)]
    [InlineData(4, 2, 1, 0x0A17E5A5u)]
    [InlineData(4, 2, 2, 0x62795159u)]
    [InlineData(2, 4, 0, 0xE912F56Du)]
    [InlineData(2, 4, 1, 0xC59C3BC7u)]
    [InlineData(2, 4, 2, 0x82417CD3u)]
    public void OddDimensionsReplicateNativeSamples(int sampleWidth,
        int sampleHeight, int component, uint expectedHash)
    {
        const int width = 5, height = 5, stride = width * 3 + 11;
        byte[] rgb = PaddedPattern(width, height, stride, 3, PixelLayout.Rgb24);
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, width, height, stride, 3, PixelLayout.Rgb24,
            component, 0, 0, sampleWidth, sampleHeight, samples);
        Assert.Equal(expectedHash, HashBits(samples));
    }

    // Each fixture hashes all 64 centered float bit patterns in three blocks.
    [Theory]
    [InlineData(3, 1, 0, 0, 0x7AD773F7u, 0x938D0C77u, 0x8BC1EA4Bu)]
    [InlineData(3, 1, 0, 1, 0x8168A1B4u, 0xF3126FEDu, 0x36B16AB1u)]
    [InlineData(3, 1, 0, 37, 0xA5717871u, 0x1701A9B3u, 0x0434486Du)]
    [InlineData(3, 1, 0, 100, 0x0A019705u, 0x900EEEA9u, 0xBEEFED5Bu)]
    [InlineData(3, 1, 1, 0, 0x5983465Fu, 0x69111FDFu, 0x3C9EF8FFu)]
    [InlineData(3, 1, 1, 1, 0x44EB84ADu, 0xA83D965Bu, 0x5F5D9975u)]
    [InlineData(3, 1, 1, 37, 0x186A3579u, 0x721298F7u, 0xE3D6C8CBu)]
    [InlineData(3, 1, 1, 100, 0x2D56C4ADu, 0x20F41479u, 0xC3DD5255u)]
    [InlineData(3, 1, 2, 0, 0xFEC24627u, 0xA4952EECu, 0x17067C17u)]
    [InlineData(3, 1, 2, 1, 0x459F4CB5u, 0x92A0EFFFu, 0xC24AADFBu)]
    [InlineData(3, 1, 2, 37, 0x6316DD81u, 0xB1EE28CBu, 0x73265F66u)]
    [InlineData(3, 1, 2, 100, 0xD26D2C51u, 0x86BBCDEDu, 0x6C80DBAFu)]
    [InlineData(3, 2, 0, 0, 0x7872AF0Fu, 0x4D406B9Du, 0xCD1621B1u)]
    [InlineData(3, 2, 0, 1, 0x9ED05193u, 0x15DCF003u, 0xEC2AE4F1u)]
    [InlineData(3, 2, 0, 37, 0x5C36011Du, 0x415269E5u, 0x804AAB77u)]
    [InlineData(3, 2, 0, 100, 0xBD4DDE81u, 0xA344D9FDu, 0x232DCDE1u)]
    [InlineData(3, 2, 1, 0, 0xB2F88347u, 0xB7053727u, 0x0E7059F1u)]
    [InlineData(3, 2, 1, 1, 0x7DDFB3CDu, 0x4C1087CBu, 0x2824240Bu)]
    [InlineData(3, 2, 1, 37, 0x3EBDF749u, 0xF4969A2Fu, 0xF31EF945u)]
    [InlineData(3, 2, 1, 100, 0xE5B84947u, 0x8E416FF1u, 0x2E5948D9u)]
    [InlineData(3, 2, 2, 0, 0x57C067EDu, 0x57756E9Du, 0x023DBE99u)]
    [InlineData(3, 2, 2, 1, 0xA3176A51u, 0xC0113DD9u, 0xCEC56165u)]
    [InlineData(3, 2, 2, 37, 0xAA561BE1u, 0x591FE43Du, 0xF13162EDu)]
    [InlineData(3, 2, 2, 100, 0x34447DA5u, 0x4C200DA5u, 0x2710C183u)]
    [InlineData(1, 4, 0, 0, 0xC20205D3u, 0x0BA27BCDu, 0xDC1102C5u)]
    [InlineData(1, 4, 0, 1, 0xC9D0DC5Du, 0xDB079875u, 0xF4BE9E65u)]
    [InlineData(1, 4, 0, 37, 0x359E69A1u, 0x3511E55Du, 0xD76A150Du)]
    [InlineData(1, 4, 0, 100, 0x310CF0DDu, 0x7FAAE9FDu, 0x4102BD0Du)]
    [InlineData(1, 4, 1, 0, 0x92ECCC27u, 0x235E9E8Du, 0xD84EE12Bu)]
    [InlineData(1, 4, 1, 1, 0xBC8691DFu, 0x4A81802Du, 0x6667861Du)]
    [InlineData(1, 4, 1, 37, 0x62A69E95u, 0x73340727u, 0x58E65283u)]
    [InlineData(1, 4, 1, 100, 0x4E240F07u, 0xEB83EB0Bu, 0x724DCD15u)]
    [InlineData(1, 4, 2, 0, 0x935906D9u, 0x0100554Du, 0xBEF4815Bu)]
    [InlineData(1, 4, 2, 1, 0x9D51F3BDu, 0x066E65BDu, 0xB8392473u)]
    [InlineData(1, 4, 2, 37, 0x666B1FC9u, 0x2E4BD671u, 0x9638A661u)]
    [InlineData(1, 4, 2, 100, 0x02E9AF85u, 0xC82E0CCDu, 0x6E11EDABu)]
    [InlineData(4, 2, 0, 0, 0x772EC541u, 0x2338D385u, 0x2C8932ADu)]
    [InlineData(4, 2, 0, 1, 0xD64C1B45u, 0x480EC991u, 0x68DF80F5u)]
    [InlineData(4, 2, 0, 37, 0xCF283519u, 0xE013A3E9u, 0x5CB22465u)]
    [InlineData(4, 2, 0, 100, 0xEE610C1Du, 0x086C83DDu, 0x4D1EA005u)]
    [InlineData(4, 2, 1, 0, 0xEC52B2A9u, 0xB526BD31u, 0xC6AA7535u)]
    [InlineData(4, 2, 1, 1, 0xF165D0D9u, 0xDCFB8F55u, 0x5B97E935u)]
    [InlineData(4, 2, 1, 37, 0x5A62D349u, 0x4E531179u, 0xE2FDAD95u)]
    [InlineData(4, 2, 1, 100, 0xE87B5BA5u, 0xF6ABA6F5u, 0xDE319645u)]
    [InlineData(4, 2, 2, 0, 0xEDC27AA1u, 0x239AE501u, 0x0CA296C9u)]
    [InlineData(4, 2, 2, 1, 0xD9FDDE49u, 0x55E144D5u, 0x706E2E7Bu)]
    [InlineData(4, 2, 2, 37, 0x9D1AC07Du, 0xE266338Du, 0xE1DF38D5u)]
    [InlineData(4, 2, 2, 100, 0xB467AAADu, 0xFAB7DFF5u, 0x2175C6A1u)]
    [InlineData(2, 4, 0, 0, 0x522B9131u, 0x5C01A9B1u, 0x203E3C4Du)]
    [InlineData(2, 4, 0, 1, 0xC0A782B9u, 0x70326959u, 0x2917760Du)]
    [InlineData(2, 4, 0, 37, 0x4184E08Bu, 0x7C810339u, 0x5FCBFC75u)]
    [InlineData(2, 4, 0, 100, 0x4DADAE65u, 0xCFD1C09Du, 0xA8C4EFA5u)]
    [InlineData(2, 4, 1, 0, 0x082BB34Du, 0x35C38969u, 0x8C42F3A5u)]
    [InlineData(2, 4, 1, 1, 0x7883A4A1u, 0x96AB74CDu, 0x530B5049u)]
    [InlineData(2, 4, 1, 37, 0xCCBF2571u, 0xAE3CEF6Du, 0xB39AFC81u)]
    [InlineData(2, 4, 1, 100, 0x0E337EE7u, 0xF111AB19u, 0xD2379AFDu)]
    [InlineData(2, 4, 2, 0, 0x911193C1u, 0x887ADE97u, 0xF57B9885u)]
    [InlineData(2, 4, 2, 1, 0x2ADECDF1u, 0x98004769u, 0x0CBE69CFu)]
    [InlineData(2, 4, 2, 37, 0xC0333A4Du, 0x6BFE70ADu, 0x05857799u)]
    [InlineData(2, 4, 2, 100, 0x3BDF2575u, 0x3BBD2D45u, 0x53BA907Du)]
    public void SmoothedPaddedLayoutsAndBlockSeamsMatchNativeSamples(int sampleWidth,
        int sampleHeight, int component, int smoothingFactor, uint firstBlock,
        uint seamBlock, uint edgeBlock)
    {
        const int width = 67, height = 67;
        foreach (PixelLayout layout in new[]
            { PixelLayout.Rgb24, PixelLayout.Bgr24, PixelLayout.Rgba32, PixelLayout.Bgra32 })
        {
            int bytesPerPixel = layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
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

    [Fact]
    public void ExtendedSamplingKeepsNon420PerceptualQuantization()
    {
        // The quality-85 1x1 luma table has a separate native fixture in NumericalTests.
        // Upstream applies sampling compensation exclusively to 2x2.
        ushort[][] expected = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(85f, component, 1, 1)).ToArray();
        float[][] expectedMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(expected, 1, 1, false);
        foreach ((int horizontal, int vertical) in new[] { (3, 1), (3, 2), (1, 4), (4, 2), (2, 4) })
        {
            ushort[][] actual = Enumerable.Range(0, 3)
                .Select(component => Quantization.CreateTable(85f, component, horizontal, vertical)).ToArray();
            float[][] actualMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(actual,
                horizontal, vertical, false);
            for (int component = 0; component < 3; component++)
            {
                Assert.Equal(expected[component], actual[component]);
                Assert.Equal(expectedMultipliers[component], actualMultipliers[component]);
            }
        }
        ushort[][] compensated = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(85f, component, 2, 2)).ToArray();
        float[][] compensatedMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(compensated, 2, 2, false);
        for (int component = 0; component < 3; component++)
        {
            Assert.False(expected[component].SequenceEqual(compensated[component]));
            Assert.False(expectedMultipliers[component].SequenceEqual(compensatedMultipliers[component]));
        }
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    public void QuantizationRejectsUnsupportedVerticalFactorsAndOversizedColorMcus(int horizontal, int vertical)
    {
        Assert.Equal("verticalSampling", Assert.Throws<ArgumentOutOfRangeException>(
            () => Quantization.CreateTable(85f, 0, horizontal, vertical)).ParamName);
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(85f, component)).ToArray();
        Assert.Equal("verticalSampling", Assert.Throws<ArgumentOutOfRangeException>(
            () => Quantization.CreateAdaptiveZeroBiasMultipliers(tables, horizontal, vertical, false)).ParamName);
    }

    private static void AssertBlocks(byte[] pixels, PixelLayout layout, int width, int height,
        int stride, int bytesPerPixel, int sampleWidth, int sampleHeight, int component,
        int smoothingFactor, uint firstBlock, uint seamBlock, uint edgeBlock)
    {
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 0, 0, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(firstBlock, HashBits(samples));
        // Every source pixel in this block is real, including the neighbors
        // required by smoothing across its left/top and right/bottom seams.
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, 8 * sampleWidth, 8 * sampleHeight,
            sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(seamBlock, HashBits(samples));
        // Keep the edge block aligned to the component's sampling grid.
        // It spans final real, first padded, and further padded pixels.
        int edgeLeft = width / (8 * sampleWidth) * (8 * sampleWidth);
        int edgeTop = height / (8 * sampleHeight) * (8 * sampleHeight);
        Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout,
            component, edgeLeft, edgeTop, sampleWidth, sampleHeight, samples, smoothingFactor);
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
