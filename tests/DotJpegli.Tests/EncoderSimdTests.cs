namespace DotJpegli.Tests;

using System.Security.Cryptography;
using Xunit;

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
    [InlineData(0, 0, "AC40347DD143B04E65E18A8675D2834F56084D8660A8BCE5B55FECC0A81321EC")]
    [InlineData(0, 1, "B2FF3EF4E9EA6B7B6742B5C47A8522EAFBD94A68E0DC66D2E8781B741F3EAEC3")]
    [InlineData(0, 2, "2E06C3101C1F098015AE74753779992B3DAD847126DAFE8F2DF17BD149D38829")]
    [InlineData(0, 3, "71E6C8CD051C4D33A47E48BCFF3EFE3DF79D36FA381E53E91E99D0A6F45FD211")]
    [InlineData(0, 4, "424D5036BDB0F2098139F05038F5DD08497C66262ECD254779FB9FE2D89918BA")]
    [InlineData(0, 5, "D053A24D62E08BAEDEFBA3E9F53FB434C480970F34A08D0559305126E8F2568B")]
    [InlineData(0, 6, "39E76A7E8E6E2ADB4719527854038E75E4BFC13D9082CDEEAD42DBCEDB249501")]
    [InlineData(0, 7, "99FE191394BF827F73B1D9A83BC6877E38BB1F13DE4DC3A4ACFA82FA7186A54D")]
    [InlineData(0, 8, "76ADE9FFA5657E5E5253518C4F41DCD1B3A70E32173B6F74055E82E43C7874E1")]
    [InlineData(0, 9, "F8E585DB73C83B1BD6111CD06E1A080ED6AA918134A3CF492B3BAC7074834611")]
    [InlineData(0, 10, "E84F0FE6D1810D74D659158CAAB453AD07CD972800E576A568A44D0D56A1C1C5")]
    [InlineData(1, 0, "0B0E8AD65745D1C06E46E43E9F0A875A271720080DA2FD974C8745533E74A57B")]
    [InlineData(1, 1, "F7A776FCC1478F62AEAE6314EA649CDE56A181E3E63D9F65096371A79F0CBA87")]
    [InlineData(1, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(1, 3, "23B61903D177BC8528738ABB0244543DA5C159440395D8E00141A19344C9A31F")]
    [InlineData(1, 4, "907EFFAB47BE3E515CC84354622FB2DCA8EBB6BC0C515C46763D42343B4F3E1B")]
    [InlineData(1, 5, "520850A3A242118F457C6E0CC4EC633FFDEBD6E08617CD79F9374907638CAB47")]
    [InlineData(1, 6, "011F2F426C9C9B4A8E707A32C72D8049DEA56A7945075F0D7687FD3D15622063")]
    [InlineData(1, 7, "B49BE48AD20935F44458245A0532081D268F23D99C2D297722E8AC71AF677E0B")]
    [InlineData(1, 8, "A5E14FF6CB7B983A44F450D37EE7CB2CA75677A51E5E26506BBD849ED5C3B4FD")]
    [InlineData(1, 9, "0AE73FF3579F00474CF5F9CC0D21600D13C76D43862ABE4E562795C5A3BA9E6F")]
    [InlineData(1, 10, "2E95D9259ECD58ABEF6470D7E834EE1184A17ECB05BC1F49346AC09D046B6D8B")]
    [InlineData(2, 0, "0B0E8AD65745D1C06E46E43E9F0A875A271720080DA2FD974C8745533E74A57B")]
    [InlineData(2, 1, "F7A776FCC1478F62AEAE6314EA649CDE56A181E3E63D9F65096371A79F0CBA87")]
    [InlineData(2, 2, "73AF18DD6CC7B54936BE0E161E6C9B0DCF4DE5783C9673097FDF48A2F9F9E4B9")]
    [InlineData(2, 3, "23B61903D177BC8528738ABB0244543DA5C159440395D8E00141A19344C9A31F")]
    [InlineData(2, 4, "907EFFAB47BE3E515CC84354622FB2DCA8EBB6BC0C515C46763D42343B4F3E1B")]
    [InlineData(2, 5, "520850A3A242118F457C6E0CC4EC633FFDEBD6E08617CD79F9374907638CAB47")]
    [InlineData(2, 6, "011F2F426C9C9B4A8E707A32C72D8049DEA56A7945075F0D7687FD3D15622063")]
    [InlineData(2, 7, "B49BE48AD20935F44458245A0532081D268F23D99C2D297722E8AC71AF677E0B")]
    [InlineData(2, 8, "A5E14FF6CB7B983A44F450D37EE7CB2CA75677A51E5E26506BBD849ED5C3B4FD")]
    [InlineData(2, 9, "0AE73FF3579F00474CF5F9CC0D21600D13C76D43862ABE4E562795C5A3BA9E6F")]
    [InlineData(2, 10, "2E95D9259ECD58ABEF6470D7E834EE1184A17ECB05BC1F49346AC09D046B6D8B")]
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
        int stride = (width * components) + 5;
        byte[] bytes = Enumerable.Repeat((byte)231, (height * stride) + 7).ToArray();
        ushort[] words = Enumerable.Repeat((ushort)54321, bytes.Length).ToArray();
        float[] floats = Enumerable.Repeat(float.NaN, bytes.Length).ToArray();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int c = 0; c < (gray ? 1 : 3); c++)
                {
                    int offset = (y * stride) + (x * components) + c;
                    int value = ((x * 29) + (y * 53) + (c * 97) + (x * y * 3)) & 255;
                    bytes[offset] = (byte)value;
                    words[offset] = (ushort)Math.Min(65535, (value * 257) + (((x * 17) + (y * 31) + (c * 11)) % 257));
                    floats[offset] = Math.Min(1f, (value + (((x * 17) + (y * 31) + (c * 11)) % 257 / 257f)) / 255f);
                }
            }
        }

        JpegliEncoderOptions options = mode switch
        {
            0 => new JpegliEncoderOptions(),
            1 => new() { Subsampling = ChromaSubsampling.None, ProgressiveLevel = 2, RestartRows = 1 },
            2 => new()
            {
                Subsampling = ChromaSubsampling.HalfHorizontalThirdVertical,
                AdaptiveQuantization = false,
                OptimizeHuffmanTables = false,
                SequentialInterleaveComponents = false,
                RestartInterval = 3
            },
            3 => new() { ColorSpace = JpegColorSpace.Grayscale, Quality = 97, RestartInterval = 2 },
            4 => new()
            {
                ColorSpace = JpegColorSpace.Rgb,
                Progressive = true,
                ProgressiveRefinementBits = 2,
                RestartInterval = 3
            },
            5 => new()
            {
                ForceBaseline = false,
                Progressive = true,
                ProgressiveRefinementBits = 1,
                QuantizationTables = [
                    Enumerable.Range(0, 64).Select(i => (ushort)(i == 0 ? 7 : 1 + (i * 11))).ToArray(),
                    Enumerable.Range(0, 64).Select(i => (ushort)(i == 0 ? 13 : 1 + (i * 7))).ToArray() ]
            },
            6 => new()
            {
                SamplingFactors = new(new(3, 1), new(1, 1), new(1, 1)),
                UseStandardQuantizationTables = true,
                RestartRows = 1
            },
            7 => new() { Subsampling = ChromaSubsampling.None, SmoothingFactor = 37, ProgressiveLevel = 1 },
            8 => new() { Subsampling = ChromaSubsampling.None, CicpTransferFunction = 16, ProgressiveLevel = 2 },
            9 => new()
            {
                ColorSpace = JpegColorSpace.Xyb,
                IccProfile = JpegliIccProfiles.CreateXybProfile(),
                ProgressiveLevel = 2,
                RestartRows = 1
            },
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
