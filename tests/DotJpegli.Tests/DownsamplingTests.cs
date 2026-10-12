using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class DownsamplingTests
{
    // Fixtures from an independently compiled scalar C++ probe of the pinned
    // color_transform.cc and downsample.cc formulas (MSVC /O2 /fp:strict).
    [Theory]
    [InlineData(1, 0xC1A03C18u)]
    [InlineData(2, 0xC21679FAu)]
    public void QuarterSamplingMatchesNativePairwiseArithmetic(int component, uint expectedBits)
    {
        byte[] rgb = [59, 24, 93, 14, 189, 158, 12, 217, 15, 139, 140, 27];
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, 2, 2, 6, 3, PixelLayout.Rgb24, component,
            0, 0, 2, 2, samples);
        // Sequential accumulation instead gives C1A03C1C / C21679F8.
        Assert.Equal(expectedBits, BitConverter.SingleToUInt32Bits(samples[0]));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void OddDimensionsReplicateNativePaddedSamples(int component, int vertical)
    {
        byte[] rgb =
        [
            255, 0, 0, 0, 255, 0, 0, 0, 255,
            255, 255, 0, 0, 255, 255, 255, 0, 255,
            13, 91, 247, 203, 47, 117, 37, 191, 59
        ];
        uint[] expected = (component, vertical) switch
        {
            (1, 1) => [0xC27EFFFEu, 0x42FEFFFEu, 0xC228F1D6u, 0x42A8F1D6u, 0x4247AD5Cu, 0xC2200F06u],
            (1, 2) => [0xC253F8EAu, 0x42D3F8EAu, 0x4247AD5Cu, 0xC2200F06u],
            (2, 1) => [0x4125E0A0u, 0xC1A5E098u, 0xC25587DCu, 0x42D587DAu, 0x4124FCC0u, 0xC2848896u],
            _ => [0xC1AC0FB4u, 0x422C0FB4u, 0x4124FCC0u, 0xC2848896u]
        };
        Span<float> samples = stackalloc float[64];
        Downsampling.FillBlock(rgb, 3, 3, 9, 3, PixelLayout.Rgb24, component,
            0, 0, 2, vertical, samples);
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
            {
                int fixtureRow = Math.Min(row, expected.Length / 2 - 1);
                Assert.Equal(expected[fixtureRow * 2 + Math.Min(column, 1)],
                    BitConverter.SingleToUInt32Bits(samples[row * 8 + column]));
            }
    }
}
