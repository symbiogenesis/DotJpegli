using System.Security.Cryptography;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderSimdTests
{
    // Complete JPEG hashes captured from the pre-SIMD-beyond-DCT encoder at
    // 1ead1c0505730c59d6380fce5324d6aaa904cdff, before changing the pipeline.
    // Odd edges and padded RGBA/BGRA sources cover automatic/adaptive tables,
    // extended custom tables, sequential/progressive scans, refinement,
    // restarts, smoothing, custom sampling and grayscale/RGB/YCbCr/XYB output.
    [Theory]
    [InlineData(0, 0, "65DA81E398CCABB293AF70C1F6863068F7841053621D2E112D0AFB9358683E98")]
    [InlineData(0, 1, "A6E5D0CAF1FA63105CA64E621B0ACE9FAEDFA407B8E469233732092F12BE16B4")]
    [InlineData(0, 2, "2E06C3101C1F098015AE74753779992B3DAD847126DAFE8F2DF17BD149D38829")]
    [InlineData(0, 3, "644FB9EB48AB88DD5C3682D0C0C61BDC582057AFA06842EDF88DC901A2575364")]
    [InlineData(0, 4, "ED99D55DD82456E932CDFDC20F07F14F8CA0EF15FA47812CA01B4B5E1A6F54AD")]
    [InlineData(0, 5, "6F6E983D56BBE4246E94093F0CCC868DB98267BD9BC4F6B99D7B5C738E26CD50")]
    [InlineData(0, 6, "8979F043BB01D03C5E10AB6743B4001FA9E1A70522C8613CC7A729FDB27A169C")]
    [InlineData(0, 7, "20739C6873BDA636D22507D2824B25A5584E83842C20892E2AC85EE0BDBB5DF4")]
    [InlineData(0, 8, "7115B18A38EE3F7D17456D05E32BAB331FADECA0C3AFC74F9CE42E4778E00D40")]
    [InlineData(0, 9, "0E218E6CF3931FAD7D0824BD54C8308A06F8E07A372D426D5BBF484A15212CA9")]
    [InlineData(0, 10, "D60A4F0E89EE3CD5D7838C4CA8ADAECAF4BF2F034164D0A410CDDEEA467A034D")]
    [InlineData(1, 0, "09B87451F554660D74E0ADD4630E15E28E2D0FC29F48EADF24E624230AE5A76B")]
    [InlineData(1, 1, "D0794DCF45A3167C6A1C600C831697A769341B39A4FC6C9023D596624AF2B9BF")]
    [InlineData(1, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(1, 3, "51DB378AB956644448730B43763367B4456345D049DD4593E25722E47FDBD97D")]
    [InlineData(1, 4, "27F945AB8203440DCD8BA56197709E264AA11BD91B5817D7D809400D72C1AE50")]
    [InlineData(1, 5, "88FA28486201BED5EF77F26AC848A0E96BC88FC6CFD848E2B4956E0A1053EA10")]
    [InlineData(1, 6, "9F59AED41CB4FAF6CF8AFB7AE0226D3B8C28ABF6AF662CF54B13BB5FDF7D3374")]
    [InlineData(1, 7, "2DB94D5D0FD6A0A4A9241299ECBDF47999EE854489ACF8BE6D71CEE9E4C80DA3")]
    [InlineData(1, 8, "F6A534F6B678C4A95CE4F339D3B7A5E7CC164D2A007B118898B82A009B63572E")]
    [InlineData(1, 9, "E191465D9DD4496AC7F2FC7E0DC05CC13F43068A550826B319CF9D2D3683DAB5")]
    [InlineData(1, 10, "2838C8570B7C3E94B75129AB590337FC5B80BA44C821A13B1DDC87D104D9EC8D")]
    [InlineData(2, 0, "09B87451F554660D74E0ADD4630E15E28E2D0FC29F48EADF24E624230AE5A76B")]
    [InlineData(2, 1, "D0794DCF45A3167C6A1C600C831697A769341B39A4FC6C9023D596624AF2B9BF")]
    [InlineData(2, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(2, 3, "51DB378AB956644448730B43763367B4456345D049DD4593E25722E47FDBD97D")]
    [InlineData(2, 4, "27F945AB8203440DCD8BA56197709E264AA11BD91B5817D7D809400D72C1AE50")]
    [InlineData(2, 5, "88FA28486201BED5EF77F26AC848A0E96BC88FC6CFD848E2B4956E0A1053EA10")]
    [InlineData(2, 6, "9F59AED41CB4FAF6CF8AFB7AE0226D3B8C28ABF6AF662CF54B13BB5FDF7D3374")]
    [InlineData(2, 7, "2DB94D5D0FD6A0A4A9241299ECBDF47999EE854489ACF8BE6D71CEE9E4C80DA3")]
    [InlineData(2, 8, "F6A534F6B678C4A95CE4F339D3B7A5E7CC164D2A007B118898B82A009B63572E")]
    [InlineData(2, 9, "E191465D9DD4496AC7F2FC7E0DC05CC13F43068A550826B319CF9D2D3683DAB5")]
    [InlineData(2, 10, "2838C8570B7C3E94B75129AB590337FC5B80BA44C821A13B1DDC87D104D9EC8D")]
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
