using System.Security.Cryptography;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderSimdTests
{
    // Complete JPEG hashes originally captured before SIMD beyond the DCT at
    // 1ead1c0505730c59d6380fce5324d6aaa904cdff. Grayscale/YCbCr optimized
    // hashes were intentionally recaptured after Huffman clustering at 28184c1;
    // independent coefficient/decoder tests cover the unchanged image content.
    // Odd edges and padded RGBA/BGRA sources cover automatic/adaptive tables,
    // extended custom tables, sequential/progressive scans, refinement,
    // restarts, smoothing, custom sampling and grayscale/RGB/YCbCr/XYB output.
    [Theory]
    [InlineData(0, 0, "8BD7DDB27DB10E12509FA0FD0E1D36C18FD884D1A124992AE750F52FD2A55B5A")]
    [InlineData(0, 1, "BC7CAD28DC99B798F092728C583F33C9AB686110B215C6E901537D4AE68B6D2C")]
    [InlineData(0, 2, "2E06C3101C1F098015AE74753779992B3DAD847126DAFE8F2DF17BD149D38829")]
    [InlineData(0, 3, "BBF594310BF698B3AE8CC5751783BA030F88A5B86FE866D5F4C06A54B27F440D")]
    [InlineData(0, 4, "ED99D55DD82456E932CDFDC20F07F14F8CA0EF15FA47812CA01B4B5E1A6F54AD")]
    [InlineData(0, 5, "090A152D82C626084629EB9D9C9375736940FB9913C7DD27C2098BDA4D9F5D94")]
    [InlineData(0, 6, "081FDEB43840BDEA3463C5224DD8E879C79214CD378BB152E92BE6DF4C73AEDC")]
    [InlineData(0, 7, "DF2B21D46A589AE1D7B4F45EDF632195E03925DB7F0B982C01217A9C4EAAC111")]
    [InlineData(0, 8, "BAA8FD80953E3A685326FDE626C4D5FDC58F968C06FC343A50BFE8C8985EB341")]
    [InlineData(0, 9, "0E218E6CF3931FAD7D0824BD54C8308A06F8E07A372D426D5BBF484A15212CA9")]
    [InlineData(0, 10, "63E15087E7E8E38C2AF3419F70F0B3CA5A0CE626A32D35AE8341895B76A4B730")]
    [InlineData(1, 0, "6BC74181BB319C5A69ECF4799F85F85D4A6B13FA3E67C34C1CD57791239A05F9")]
    [InlineData(1, 1, "9F070C92A166568BF790FB90E7119E59DA151C4F6D3217FCA0E353F2EAF2F222")]
    [InlineData(1, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(1, 3, "C6E4806F3000A078C2772D2260C5F26690F576356ED8521215BD3852FDBC9FB1")]
    [InlineData(1, 4, "27F945AB8203440DCD8BA56197709E264AA11BD91B5817D7D809400D72C1AE50")]
    [InlineData(1, 5, "1E74FDFE9DF9D7E4BCEBE51EBC04ADD9CB45D77979F54E5FC0A7DF3470C04A9F")]
    [InlineData(1, 6, "B7B416C32C106F3FC11CC4CE2D20A38ADEAA07FA3BEE490838D8B036C0A1032C")]
    [InlineData(1, 7, "C6FEC76064DD88704A0512749B525C713B20329A33DAE2C5C8C12D647245FE41")]
    [InlineData(1, 8, "0A760EE4B7B61D74208C0D47CD81C6ABCC6FAAA646529A005B2C452410C12BBC")]
    [InlineData(1, 9, "E191465D9DD4496AC7F2FC7E0DC05CC13F43068A550826B319CF9D2D3683DAB5")]
    [InlineData(1, 10, "17AD33B7B3FF528AC7814FF6A1E9BC2A6B5D20DCBFBF61ACE2591982E48B6A23")]
    [InlineData(2, 0, "6BC74181BB319C5A69ECF4799F85F85D4A6B13FA3E67C34C1CD57791239A05F9")]
    [InlineData(2, 1, "9F070C92A166568BF790FB90E7119E59DA151C4F6D3217FCA0E353F2EAF2F222")]
    [InlineData(2, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(2, 3, "C6E4806F3000A078C2772D2260C5F26690F576356ED8521215BD3852FDBC9FB1")]
    [InlineData(2, 4, "27F945AB8203440DCD8BA56197709E264AA11BD91B5817D7D809400D72C1AE50")]
    [InlineData(2, 5, "1E74FDFE9DF9D7E4BCEBE51EBC04ADD9CB45D77979F54E5FC0A7DF3470C04A9F")]
    [InlineData(2, 6, "B7B416C32C106F3FC11CC4CE2D20A38ADEAA07FA3BEE490838D8B036C0A1032C")]
    [InlineData(2, 7, "C6FEC76064DD88704A0512749B525C713B20329A33DAE2C5C8C12D647245FE41")]
    [InlineData(2, 8, "0A760EE4B7B61D74208C0D47CD81C6ABCC6FAAA646529A005B2C452410C12BBC")]
    [InlineData(2, 9, "E191465D9DD4496AC7F2FC7E0DC05CC13F43068A550826B319CF9D2D3683DAB5")]
    [InlineData(2, 10, "17AD33B7B3FF528AC7814FF6A1E9BC2A6B5D20DCBFBF61ACE2591982E48B6A23")]
    public void ManagedSimdPreservesPreexistingJpegBytes(int precision, int mode, string expectedHash)
    {
        byte[] jpeg = EncodeFixture(precision, mode);
        Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(jpeg)));
    }

    private static byte[] EncodeFixture(int precision, int mode)
    {
        const int width = 19, height = 13;
        bool gray = mode == 10;
        bool direct = mode == 8;
        bool bgr = mode % 2 != 0;
        int components = gray ? 1 : direct ? 3 : 4;
        int stride = width * components + 5;
        byte[] bytes = Enumerable.Repeat((byte)231, height * stride + 7).ToArray();
        ushort[] words = Enumerable.Repeat((ushort)54321, bytes.Length).ToArray();
        float[] floats = Enumerable.Repeat(float.NaN, bytes.Length).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int c = 0; c < (gray ? 1 : 3); c++)
                {
                    int offset = y * stride + x * components + c;
                    int value = (x * 29 + y * 53 + c * 97 + x * y * 3) & 255;
                    bytes[offset] = (byte)value;
                    words[offset] = (ushort)Math.Min(65535, value * 257 + (x * 17 + y * 31 + c * 11) % 257);
                    floats[offset] = Math.Min(1f, (value + (x * 17 + y * 31 + c * 11) % 257 / 257f) / 255f);
                }
        var options = mode switch
        {
            0 => new JpegliEncoderOptions(),
            1 => new() { Subsampling = ChromaSubsampling.None, ProgressiveLevel = 2, RestartRows = 1 },
            2 => new() { Subsampling = ChromaSubsampling.HalfHorizontalThirdVertical, AdaptiveQuantization = false,
                OptimizeHuffmanTables = false, SequentialInterleaveComponents = false, RestartInterval = 3 },
            3 => new() { ColorSpace = JpegColorSpace.Grayscale, Quality = 97, RestartInterval = 2 },
            4 => new() { ColorSpace = JpegColorSpace.Rgb, Progressive = true, ProgressiveRefinementBits = 2,
                RestartInterval = 3 },
            5 => new() { ForceBaseline = false, Progressive = true, ProgressiveRefinementBits = 1,
                QuantizationTables = new ReadOnlyMemory<ushort>[] {
                    Enumerable.Range(0, 64).Select(i => (ushort)(i == 0 ? 7 : 1 + i * 11)).ToArray(),
                    Enumerable.Range(0, 64).Select(i => (ushort)(i == 0 ? 13 : 1 + i * 7)).ToArray() } },
            6 => new() { SamplingFactors = new(new(3, 1), new(1, 1), new(1, 1)),
                UseStandardQuantizationTables = true, RestartRows = 1 },
            7 => new() { Subsampling = ChromaSubsampling.None, SmoothingFactor = 37, ProgressiveLevel = 1 },
            8 => new() { Subsampling = ChromaSubsampling.None, CicpTransferFunction = 16, ProgressiveLevel = 2 },
            9 => new() { ColorSpace = JpegColorSpace.Xyb, IccProfile = JpegliIccProfiles.CreateXybProfile(),
                ProgressiveLevel = 2, RestartRows = 1 },
            _ => new() { ColorSpace = JpegColorSpace.YCbCr, Subsampling = ChromaSubsampling.None, Progressive = true }
        };
        PixelLayout byteLayout = gray ? PixelLayout.Gray8 : direct ? PixelLayout.YCbCr24
            : bgr ? PixelLayout.Bgra32 : PixelLayout.Rgba32;
        PixelComponentLayout typedLayout = gray ? PixelComponentLayout.Gray : direct ? PixelComponentLayout.YCbCr
            : bgr ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgba;
        return precision switch
        {
            0 => JpegliEncoder.Encode(bytes, width, height, byteLayout, options, stride),
            1 => JpegliEncoder.Encode(words, width, height, typedLayout, options, stride),
            _ => JpegliEncoder.Encode(floats, width, height, typedLayout, options, stride)
        };
    }
}
