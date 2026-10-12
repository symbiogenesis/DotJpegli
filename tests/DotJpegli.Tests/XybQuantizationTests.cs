using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class XybQuantizationTests
{
    // Independently compiled scalar C++ captures from quant.cc at google/jpegli
    // 031a0077f5799a6041004267fc12b956c1f52a20, kBaseQuantMatrixXYB,
    // kGlobalScaleXYB and DistanceToScale. Stored in JPEG zigzag order.
    // No production constants or helper functions construct these expectations.
    public static TheoryData<float, string, string, string> QuantizationFixtures => new()
    {
        { 0.1f,
            "010303030303030303030303030303030303030303040303030303040504040404040405050404040404050504060604050506070605090505090905090A0A06",
            "01010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101",
            "01010102020202020202020202020203020202020304020203020204080404040404040806040404040406040604040604050505050506060606070707080809" },
        { 0.375f,
            "040B0B0C0C0C0B0B0B0B0C0B0C0B0C0D0C0A0A0C0D0E0D0C0B0C0D0E140F0D0E0E0D0F14120E0E0E0E0E1212111616111211171A171122121222211321242416",
            "01020202020202020202030202020303030202030303030302030303030303030303030303030303030303030303030303040403040404040404040404040404",
            "0205050706070806060809080908090A090808090A0F08090B09080F1E0E0F0E0E0F0E1E160E0E0E0E0E1610160E0E16101213131312181616181A1A1A1E1E22" },
        { 1f,
            "0B1D1D201F201E1D1D1E211E201E21221F1C1C1F2225221F1D1F222534292425252429342F25252625252F2F2D3A3A2D2F2D3E463E2D5B31315B59325960603A",
            "020505050505060505060706060607070706060707080707070707080908080707080809090808080808090909090909090A0B090B0A0B0A0A0B0B0A0B0A0A0A",
            "050E0E1210121510101517161916171C181414181C2815171E17152850262926262926503B26252725263B2B3B26263B2B31333233313F3B3B3F46454650505C" },
        { 1.5f,
            "102B2B312F312D2B2B2D312C2F2C31332F29292F3338342F2C2F34384E3D353737353D4E47373839383747474358584347445D685D44894A4A89864A86919156",
            "040707080708080808080A0908090A0B0A09090A0B0B0A0A0A0A0A0B0D0C0C0B0B0C0C0D0E0C0C0C0C0C0E0D0D0D0D0D0D0F110E110F100F0F10100F100F0F0F",
            "0716161B191B2019192023212521232A241E1E242A3C20232E23203C78393E38383E39785839373A373958415838385841494D4B4D495E59595E696769787889" },
        { 3f,
            "213D3D4E4A4E4B45454B6251555162665E49495E666F675B575B676F9D7A6B6E6E6B7A9D8D6E6971696E8D8D7EAFAF7E8D87BBD1BB87FF9393FFFF95FFFFFFAD",
            "070A0A0C0C0C0E0D0D0E15110F1115151511111515171514141415171B1817161617181B1B18161716181B1A191B1B191A1D211B211D201F1F20211F211E1E1E",
            "0E1F1F2A272A35282835463D423D465347363647537940435B434079F0727B71717B72F0B07168746871B082A57171A582929A959A92BDB2B2BDD3CED3EFEFFF" },
        { 10f,
            "6D8F8FAEA3AEB69F9FB6FFE7E9E7FFFFFFC7C7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "1717171B1A1B221D1D22463029304647462D2D46474C45414241454C59514E4A4A4E51595B51424D42515B584958584958626E5B6E626C66666C6D666D656564",
            "3048485F565F805B5B80E9AEB6AEE9FFEE9191EEFFFFD7D5FFD5D7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" },
    };

    [Theory]
    [MemberData(nameof(QuantizationFixtures))]
    public void ThreeEmittedTablesMatchPinnedUpstreamCapture(float distance, string x, string y, string b)
    {
        var options = new JpegliEncoderOptions { Distance = distance, IccProfile = new byte[] { 0x23 }, AdaptiveQuantization = false };
        JpegDocument document = Read(Encode(options), options);
        Assert.Equal(new[] { 0, 1, 2 }, document.Quantizers.Keys);
        Assert.Equal(Convert.FromHexString(x), document.Quantizers[0]);
        Assert.Equal(Convert.FromHexString(y), document.Quantizers[1]);
        Assert.Equal(Convert.FromHexString(b), document.Quantizers[2]);
        Assert.Equal(0, document.Components['R'].Quantizer);
        Assert.Equal(1, document.Components['G'].Quantizer);
        Assert.Equal(2, document.Components['B'].Quantizer);
        foreach (bool standard in new[] { false, true })
            foreach (int transfer in new[] { 0, 2, 16, 18, 255 })
            {
                var variant = options with { UseStandardQuantizationTables = standard, CicpTransferFunction = transfer };
                Assert.Equal(Encode(options), Encode(variant));
            }
        var custom = options with { SamplingFactors = new(new(2, 2), new(1, 1), new(1, 1)) };
        JpegDocument sampled = Read(Encode(custom), custom);
        for (int channel = 0; channel < 3; channel++)
            Assert.Equal(document.Quantizers[channel], sampled.Quantizers[channel]);
    }

    [Theory]
    [InlineData(float.Epsilon, 1)]
    [InlineData(float.MaxValue, 255)]
    public void ExtremeDistancesClampEveryTableWithoutIntegerOverflow(float distance, byte expected)
    {
        var options = new JpegliEncoderOptions { Distance = distance, IccProfile = new byte[] { 0x23 } };
        JpegDocument document = Read(Encode(options), options);
        Assert.Equal(3, document.Quantizers.Count);
        foreach (byte[] table in document.Quantizers.Values)
            Assert.All(table, value => Assert.Equal(expected, value));
    }

    [Fact]
    public void DistanceOverridesQualityAndCicpHintsDoNotAlterAdaptiveCoefficients()
    {
        const int width = 33, height = 19;
        var pixels = new byte[width * height * 3];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = (byte)((index * 73 + index / 7 * 31) % 256);
        var options = new JpegliEncoderOptions
        {
            Distance = 1.45f, Quality = 1, IccProfile = new byte[] { 0x23 }, SmoothingFactor = 37,
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Xyb24, options,
            cancellationToken: TestContext.Current.CancellationToken);
        foreach (bool standard in new[] { false, true })
            foreach (int transfer in new[] { 2, 16, 18 })
            {
                var variant = options with { Quality = 100, UseStandardQuantizationTables = standard, CicpTransferFunction = transfer };
                Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height, PixelLayout.Xyb24, variant,
                    cancellationToken: TestContext.Current.CancellationToken));
            }
    }

    private static byte[] Encode(JpegliEncoderOptions options) => JpegliEncoder.Encode(new byte[] { 32, 128, 224 }, 1, 1,
        PixelLayout.Xyb24, options, cancellationToken: TestContext.Current.CancellationToken);

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);
}

