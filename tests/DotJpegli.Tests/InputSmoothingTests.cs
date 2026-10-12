using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class InputSmoothingTests
{
    // Independently compiled scalar C++ extraction of ExtRGBToYCbCr,
    // ApplyInputSmoothing, DownsampleRow2x1 and Downsample2x2 from
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    // MSVC 14.51.36231 /O2 /fp:strict; chroma uses std::fma as upstream does.
    // Each fixture hashes all 64 centered float bit patterns of a block.
    [Theory]
    [InlineData(1, 1, 0, 0, 0x3A63BDB7u, 0xC61F2D15u, 0xA4C6C085u)]
    [InlineData(1, 1, 0, 1, 0x879273AAu, 0x191965FCu, 0x5EFA0EA5u)]
    [InlineData(1, 1, 0, 100, 0x87F1A1F1u, 0xEE585651u, 0x4C555261u)]
    [InlineData(1, 1, 1, 0, 0x308C6F96u, 0x9AD11587u, 0x8F7DBB85u)]
    [InlineData(1, 1, 1, 1, 0x6B950B97u, 0x663C39D2u, 0x2790C42Du)]
    [InlineData(1, 1, 1, 100, 0xF886C801u, 0xAF67B1A3u, 0xA267C161u)]
    [InlineData(1, 1, 2, 0, 0x8336C42Cu, 0x8A5E4487u, 0x08337125u)]
    [InlineData(1, 1, 2, 1, 0xACC207A8u, 0x37B98C9Au, 0xBA2F86D5u)]
    [InlineData(1, 1, 2, 100, 0xCDC823EFu, 0x56EB2821u, 0x3FED4609u)]
    [InlineData(2, 1, 0, 0, 0x5947812Du, 0xE025E659u, 0xA4C6C085u)]
    [InlineData(2, 1, 0, 1, 0xD72A8069u, 0x26E3195Cu, 0xA6A984D5u)]
    [InlineData(2, 1, 0, 100, 0x47535CEDu, 0xD5E12F71u, 0xED22E5D1u)]
    [InlineData(2, 1, 1, 0, 0xEAFC700Bu, 0xFDF23C6Au, 0x8F7DBB85u)]
    [InlineData(2, 1, 1, 1, 0x7B833F5Fu, 0xA3D0184Fu, 0x7E04241Du)]
    [InlineData(2, 1, 1, 100, 0x5543D619u, 0x724254CBu, 0xC6E174D5u)]
    [InlineData(2, 1, 2, 0, 0xF8C6BF2Eu, 0x1B6372DCu, 0x08337125u)]
    [InlineData(2, 1, 2, 1, 0x00ED707Eu, 0xA642B82Bu, 0x45F4EAF1u)]
    [InlineData(2, 1, 2, 100, 0x12C1E6F1u, 0xD31F9509u, 0x36F48195u)]
    [InlineData(2, 2, 0, 0, 0x8CD84AE7u, 0x04E2F81Au, 0x48704FE5u)]
    [InlineData(2, 2, 0, 1, 0xD6768D09u, 0x53CC74D7u, 0x273D4B25u)]
    [InlineData(2, 2, 0, 100, 0x8A0BD371u, 0x03D73D4Du, 0xDE83AF85u)]
    [InlineData(2, 2, 1, 0, 0x73BDF995u, 0x5E5C9C15u, 0xDD3B3965u)]
    [InlineData(2, 2, 1, 1, 0xB063FE0Fu, 0x3E2301A1u, 0x9E4E0E5Du)]
    [InlineData(2, 2, 1, 100, 0x51D9DA51u, 0x02273F65u, 0x2E15AB35u)]
    [InlineData(2, 2, 2, 0, 0x509F118Fu, 0xF33E4945u, 0xA8F97C45u)]
    [InlineData(2, 2, 2, 1, 0x04C45013u, 0xDCC51791u, 0x0319EBE5u)]
    [InlineData(2, 2, 2, 100, 0xAAA72BF1u, 0xF154D3B9u, 0xBA17D8C5u)]
    public void ConvertedSmoothedAndSubsampledBlocksMatchNativeBits(int sampleWidth,
        int sampleHeight, int component, int smoothingFactor, uint firstBlock,
        uint seamBlock, uint edgeBlock)
    {
        const int width = 17, height = 19, stride = width * 3 + 7;
        byte[] rgb = Enumerable.Repeat((byte)0xED, height * stride).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = y * stride + x * 3;
                rgb[offset] = (byte)((x * 73 + y * 29 + 11) & 255);
                rgb[offset + 1] = (byte)((x * 17 + y * 97 + 83) & 255);
                rgb[offset + 2] = (byte)((x * 43 + y * 7 + 191) & 255);
            }

        // These blocks exercise the top/left border, real neighbors across
        // block seams, and distinct actual/first-padded/further-padded pixels.
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, width, height, stride, 3, PixelLayout.Rgb24,
            component, 0, 0, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(firstBlock, HashBits(samples));
        Downsampling.FillBlock(rgb, width, height, stride, 3, PixelLayout.Rgb24,
            component, 8, 8, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(seamBlock, HashBits(samples));
        Downsampling.FillBlock(rgb, width, height, stride, 3, PixelLayout.Rgb24,
            component, 16, 16, sampleWidth, sampleHeight, samples, smoothingFactor);
        Assert.Equal(edgeBlock, HashBits(samples));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void GrayscaleSmoothingReplicatesOriginalOddImageEdges(int smoothingFactor)
    {
        byte[] gray = [0, 255, 7, 0xED, 0xED, 130, 23, 201, 0xED, 0xED, 19, 87, 241];
        uint[] expected = smoothingFactor switch
        {
            0 =>
            [
                0xC3000000u, 0x42FE0000u, 0xC2F20000u, 0xC2F20000u,
                0x40000000u, 0xC2D20000u, 0x42920000u, 0x42920000u,
                0xC2DA0000u, 0xC2240000u, 0x42E20000u, 0x42E20000u,
                0xC2DA0000u, 0xC2240000u, 0x42E20000u, 0x42E20000u
            ],
            1 =>
            [
                0xC2FE7380u, 0x42FB3B80u, 0xC2F03E00u, 0xC2F0DD00u,
                0x3FC0A000u, 0xC2D08600u, 0x4290EF00u, 0x42911900u,
                0xC2D94B00u, 0xC222F700u, 0x42E0D100u, 0x42E1C400u,
                0xC2D99A00u, 0xC222FE00u, 0x42E11900u, 0x42E20000u
            ],
            _ =>
            [
                0xC24A3C00u, 0xC1361000u, 0xC2047000u, 0xC2805400u,
                0xC23E0C00u, 0xC1F96000u, 0x419D7000u, 0x41DF1000u,
                0xC2934C00u, 0xC171F000u, 0x42574800u, 0x42CA9000u,
                0xC2B22800u, 0xC17CE000u, 0x4287C400u, 0x42E20000u
            ]
        };
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(gray, 3, 3, 5, 1, PixelLayout.Gray8, 0,
            0, 0, 1, 1, samples, smoothingFactor);
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                Assert.Equal(expected[Math.Min(row, 3) * 4 + Math.Min(column, 3)],
                    BitConverter.SingleToUInt32Bits(samples[row * 8 + column]));
    }

    private static uint HashBits(ReadOnlySpan<float> samples)
    {
        uint hash = 2166136261u;
        foreach (float sample in samples)
            hash = unchecked((hash ^ BitConverter.SingleToUInt32Bits(sample)) * 16777619u);
        return hash;
    }
}
