using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class Rec2100IccProfileTests
{
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];

    // Independent native fixtures from unchanged google/jpegli
    // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20,
    // with D65, Rec.2020 primaries, PQ/HLG and perceptual intent, using the
    // pinned builder's default 3D tone mapping. Compiled with MSVC ARM64
    // C++17 /O2 /fp:strict; the fixtures contain the complete ICC profiles.
    [Theory]
    [InlineData(false, "B2F97E195A8C64DD55EFF71FC1DF480A12E59D71105D7D9DABFCC23F9252C48E", "22BC30F3E3B03923D34FE5CF368CCFA6")]
    [InlineData(true, "CB3A3012517E21922D895462F66841F6EDEDDE8031CC3CD3041A12AF8536A556", "D2BA1D3F609E15C6612C1CDE2CC98D15")]
    public void CompleteProfilesMatchThePinnedNativeFixtures(bool hlg, string sha256, string id)
    {
        byte[] profile = Create(hlg);
        Assert.Equal(4344, profile.Length);
        Assert.Equal(sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal(id, Convert.ToHexString(profile.AsSpan(84, 16)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FactoriesReturnIndependentDeterministicStorageWithTheIccMd5Id(bool hlg)
    {
        byte[] first = Create(hlg), second = Create(hlg);
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
        Array.Fill(first, (byte)0xdd);
        Assert.Equal(second, Create(hlg));

        byte[] id = second[84..100];
        Array.Clear(second, 44, 4);
        Array.Clear(second, 64, 4);
        Array.Clear(second, 84, 16);
        Assert.Equal(id, MD5.HashData(second));
    }

    [Theory]
    [InlineData(false, "RGB_D65_202_Per_PeQ", 16)]
    [InlineData(true, "RGB_D65_202_Per_HLG", 18)]
    public void HeaderAndAlignedTagsDescribeRec2020RgbWithLabPcs(bool hlg, string description, int transfer)
    {
        byte[] profile = Create(hlg);
        Assert.Equal(profile.Length, Int32(profile, 0));
        Assert.Equal("jxl ", Signature(profile, 4));
        Assert.Equal(0x04400000, Int32(profile, 8));
        Assert.Equal("mntr", Signature(profile, 12));
        Assert.Equal("RGB ", Signature(profile, 16));
        Assert.Equal("Lab ", Signature(profile, 20));
        Assert.Equal(new[] { 2019, 12, 1, 0, 0, 0 },
            Enumerable.Range(0, 6).Select(index => UInt16(profile, 24 + index * 2)));
        Assert.Equal("acsp", Signature(profile, 36));
        Assert.Equal("APPL", Signature(profile, 40));
        Assert.All(profile[44..68], value => Assert.Equal(0, value));
        Assert.Equal(new[] { 0x0000f6d6, 0x00010000, 0x0000d32d },
            Enumerable.Range(0, 3).Select(index => Int32(profile, 68 + index * 4)));
        Assert.Equal("jxl ", Signature(profile, 80));
        Assert.All(profile[100..128], value => Assert.Equal(0, value));

        Dictionary<string, Tag> tags = Tags(profile);
        Assert.Equal(new[] { "desc", "cprt", "wtpt", "chad", "cicp", "rXYZ", "gXYZ", "bXYZ", "A2B0", "B2A0" },
            tags.Keys);
        Assert.Equal(description, Text(profile, tags["desc"]));
        Assert.Equal("CC0", Text(profile, tags["cprt"]));
        Assert.Equal("XYZ ", Signature(profile, tags["wtpt"].Offset));
        Assert.Equal("sf32", Signature(profile, tags["chad"].Offset));
        Assert.Equal(44, tags["chad"].Length);
        Tag cicp = tags["cicp"];
        Assert.Equal("cicp", Signature(profile, cicp.Offset));
        Assert.Equal(12, cicp.Length);
        Assert.Equal(new byte[] { 9, (byte)transfer, 0, 1 },
            profile[(cicp.Offset + 8)..(cicp.Offset + 12)]);
        Assert.Equal(transfer, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));

        // These fallback XYZ columns still describe Bradford-adapted Rec.2020.
        // The HDR decoding/tone mapping is represented by A2B0, without r/g/bTRC.
        double[][] colorants =
        [
            [0.673459, 0.279033, -0.001931],
            [0.165661, 0.675338, 0.029979],
            [0.125100, 0.045629, 0.797162]
        ];
        for (int channel = 0; channel < 3; channel++)
        {
            Tag colorant = tags[new[] { "rXYZ", "gXYZ", "bXYZ" }[channel]];
            Assert.Equal("XYZ ", Signature(profile, colorant.Offset));
            Assert.Equal(20, colorant.Length);
            for (int coordinate = 0; coordinate < 3; coordinate++)
                AssertClose(colorants[channel][coordinate], Fixed(profile, colorant.Offset + 8 + coordinate * 4),
                    0.5 / 65536, $"Rec.2020 colorant {channel}, coordinate {coordinate}");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForwardLutHasNineGridPointsAndIdentityInputOutputStages(bool hlg)
    {
        byte[] profile = Create(hlg);
        Tag tag = Tags(profile)["A2B0"];
        Assert.Equal("mft1", Signature(profile, tag.Offset));
        Assert.Equal(3772, tag.Length);
        Assert.Equal(new byte[] { 3, 3, 9, 0 }, profile[(tag.Offset + 8)..(tag.Offset + 12)]);
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                Assert.Equal(row == column ? 1 : 0, Fixed(profile, tag.Offset + 12 + (row * 3 + column) * 4));
        for (int channel = 0; channel < 3; channel++)
            for (int value = 0; value < 256; value++)
            {
                Assert.Equal(value, profile[tag.Offset + 48 + channel * 256 + value]);
                Assert.Equal(value, profile[tag.Offset + 48 + 768 + 9 * 9 * 9 * 3 + channel * 256 + value]);
            }
        Assert.Equal(0, profile[tag.Offset + tag.Length - 1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedToneMappingNodesMatchIndependentNativeNeutralsAndSaturatedColors(bool hlg)
    {
        byte[] profile = Create(hlg);
        Tag tag = Tags(profile)["A2B0"];
        int[] lightness = hlg ? [0, 30, 62, 88, 109, 133, 165, 205, 255]
            : [0, 5, 40, 96, 161, 212, 242, 253, 255];
        for (int index = 0; index < 9; index++)
            Assert.Equal(new byte[] { (byte)lightness[index], 128, 128 }, Node(index, index, index));

        // Blue varies fastest in the native RGB grid. These asymmetric nodes
        // also cover the PQ desaturation absent from the HLG path.
        Assert.Equal(hlg ? new byte[] { 81, 189, 12 } : [118, 165, 38], Node(0, 0, 8));
        Assert.Equal(hlg ? new byte[] { 223, 1, 203 } : [242, 89, 148], Node(0, 8, 0));
        Assert.Equal(hlg ? new byte[] { 163, 232, 187 } : [201, 185, 148], Node(8, 0, 0));
        Assert.Equal(hlg ? new byte[] { 151, 109, 59 } : [159, 138, 64], Node(1, 6, 8));
        Assert.Equal(hlg ? new byte[] { 174, 216, 190 } : [202, 184, 148], Node(8, 4, 0));

        byte[] Node(int r, int g, int b)
        {
            int offset = tag.Offset + 48 + 768 + ((r * 9 + g) * 9 + b) * 3;
            return profile[offset..(offset + 3)];
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void B2ATagIsAnIdentityReaderCompatibilityTransform(bool hlg)
    {
        byte[] profile = Create(hlg);
        Tag tag = Tags(profile)["B2A0"];
        Assert.Equal("mBA ", Signature(profile, tag.Offset));
        Assert.Equal(80, tag.Length);
        Assert.Equal(new byte[] { 3, 3, 0, 0 }, profile[(tag.Offset + 8)..(tag.Offset + 12)]);
        Assert.Equal(32, Int32(profile, tag.Offset + 12));
        Assert.All(profile[(tag.Offset + 16)..(tag.Offset + 32)], value => Assert.Equal(0, value));
        for (int channel = 0; channel < 3; channel++)
        {
            int curve = tag.Offset + 32 + channel * 16;
            Assert.Equal("para", Signature(profile, curve));
            Assert.Equal(0, Int32(profile, curve + 4));
            Assert.Equal(0, UInt16(profile, curve + 8));
            Assert.Equal(0, UInt16(profile, curve + 10));
            Assert.Equal(1, Fixed(profile, curve + 12));
        }
    }

    [Fact]
    public void HlgNeutralAxisMatchesIndependentInverseOetfAndDisplayGammaEquations()
    {
        byte[] profile = JpegliIccProfiles.CreateRec2100HlgProfile();
        int clut = Tags(profile)["A2B0"].Offset + 48 + 768;
        double gamma = Math.Pow(1.111, Math.Log2(80.0 / 300));
        for (int index = 0; index < 9; index++)
        {
            double encoded = index / 8.0;
            double scene = encoded <= 0.5 ? encoded * encoded / 3
                : (Math.Exp((encoded - 0.5599107295) / 0.17883277) + 1 - 4 * 0.17883277) / 12;
            double luminance = Math.Pow(scene, gamma);
            double lightness = luminance > 216.0 / 24389
                ? 116 * Math.Cbrt(luminance) - 16 : 24389.0 / 27 * luminance;
            int expected = (int)Math.Round(255 * lightness / 100, MidpointRounding.AwayFromZero);
            int offset = clut + ((index * 9 + index) * 9 + index) * 3;
            // Allow one byte for float matrix adaptation and ICC quantization.
            Assert.InRange((int)profile[offset], expected - 1, expected + 1);
            Assert.Equal(128, profile[offset + 1]);
            Assert.Equal(128, profile[offset + 2]);
        }
    }

    public static TheoryData<bool, int, JpegColorSpace, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<bool, int, JpegColorSpace, bool>();
            foreach (bool hlg in new[] { false, true })
                for (int kind = 0; kind < 3; kind++)
                {
                    modes.Add(hlg, kind, JpegColorSpace.YCbCr, kind == 1);
                    modes.Add(hlg, kind, JpegColorSpace.Rgb, kind != 1);
                }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void ExplicitEmbeddingPreservesCodingAndPaddedSamplesAcrossPrecisionsAndScans(
        bool hlg, int kind, JpegColorSpace colorSpace, bool progressive)
    {
        const int width = 19, height = 17, stride = width * 4 + 5;
        Array pixels = Source(kind, width, height, stride);
        Array originalPixels = (Array)pixels.Clone();
        byte[] profile = Create(hlg);
        byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        byte[] originalBacking = backing.ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, IccProfile = backing.AsMemory(3, profile.Length),
            Distance = 1.375f, SmoothingFactor = 37, Progressive = progressive,
            ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 2,
            OptimizeHuffmanTables = progressive
        };
        byte[] actual = Encode(pixels, width, height, stride, options);
        (byte[]? embedded, byte[] coding) = ReadIccAndCoding(actual);
        Assert.Equal(profile, embedded);
        Assert.Equal(coding, ReadIccAndCoding(Encode(pixels, width, height, stride,
            options with { IccProfile = OpaqueProfile })).Coding);
        var withoutProfile = ReadIccAndCoding(Encode(pixels, width, height, stride,
            options with { IccProfile = default }));
        Assert.Null(withoutProfile.Icc);
        Assert.Equal(coding, withoutProfile.Coding);
        byte[] inferred = Encode(pixels, width, height, stride,
            options with { InferCicpTransferFunctionFromIccProfile = true });
        Assert.Equal(Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = hlg ? 18 : 16 }), inferred);
        var inferredParts = ReadIccAndCoding(inferred);
        Assert.Equal(profile, inferredParts.Icc);
        if (colorSpace == JpegColorSpace.YCbCr)
            Assert.False(coding.SequenceEqual(inferredParts.Coding));
        else
            Assert.Equal(coding, inferredParts.Coding);
        JpegDocument document = JpegDocument.Read(actual, progressive, 2, options.ProgressiveRefinementBits);
        Assert.Equal((width, height), (document.Width, document.Height));
        Assert.Equal(3, document.Components.Count);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, stride, options);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
        Assert.Equal(originalBacking, backing);
        PixelAssert.Equal(originalPixels, pixels);
    }

    [Theory]
    [InlineData(false, 16)]
    [InlineData(true, 18)]
    public void TransferInferenceIsOptInAndExplicitHintsOverrideMetadataWithoutRewritingIt(bool hlg, int transfer)
    {
        Assert.False(new JpegliEncoderOptions().InferCicpTransferFunctionFromIccProfile);
        const int width = 19, height = 17, stride = width * 4 + 5;
        Array pixels = Source(0, width, height, stride);
        byte[] profile = Create(hlg);
        var options = new JpegliEncoderOptions { IccProfile = profile, Distance = 2.375f };
        byte[] disabled = Encode(pixels, width, height, stride, options);
        byte[] inferred = Encode(pixels, width, height, stride,
            options with { InferCicpTransferFunctionFromIccProfile = true });
        Assert.Equal(Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = transfer }), inferred);
        Assert.False(ReadIccAndCoding(disabled).Coding.SequenceEqual(ReadIccAndCoding(inferred).Coding));
        Assert.Equal(profile, ReadIccAndCoding(disabled).Icc);
        Assert.Equal(profile, ReadIccAndCoding(inferred).Icc);
        Assert.Equal(disabled, Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = 1, InferCicpTransferFunctionFromIccProfile = true }));
        int otherHdrTransfer = hlg ? 16 : 18;
        byte[] overridden = Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = otherHdrTransfer });
        Assert.Equal(overridden, Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = otherHdrTransfer, InferCicpTransferFunctionFromIccProfile = true }));
        Assert.Equal(profile, ReadIccAndCoding(overridden).Icc);
        Assert.Equal(profile, Create(hlg));
    }

    private static byte[] Create(bool hlg) => hlg
        ? JpegliIccProfiles.CreateRec2100HlgProfile() : JpegliIccProfiles.CreateRec2100PqProfile();

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        Assert.Equal(10, count);
        var result = new Dictionary<string, Tag>();
        var extents = new List<Tag>();
        for (int index = 0; index < count; index++)
        {
            int entry = 132 + index * 12;
            var tag = new Tag(Int32(profile, entry + 4), Int32(profile, entry + 8));
            Assert.Equal(0, tag.Offset % 4);
            Assert.InRange(tag.Offset, end, profile.Length - 8);
            Assert.InRange(tag.Length, 8, profile.Length - tag.Offset);
            Assert.Equal(0, Int32(profile, tag.Offset + 4));
            Assert.True(result.TryAdd(Signature(profile, entry), tag));
            extents.Add(tag);
        }
        Tag[] ordered = extents.OrderBy(tag => tag.Offset).ToArray();
        Assert.Equal(end, ordered[0].Offset);
        for (int index = 1; index < ordered.Length; index++)
        {
            int previousEnd = ordered[index - 1].Offset + ordered[index - 1].Length;
            Assert.Equal((previousEnd + 3) & ~3, ordered[index].Offset);
            Assert.All(profile[previousEnd..ordered[index].Offset], value => Assert.Equal(0, value));
        }
        Assert.Equal(profile.Length, ordered[^1].Offset + ordered[^1].Length);
        return result;
    }

    private static string Text(byte[] profile, Tag tag)
    {
        Assert.Equal("mluc", Signature(profile, tag.Offset));
        Assert.Equal(1, Int32(profile, tag.Offset + 8));
        Assert.Equal(12, Int32(profile, tag.Offset + 12));
        Assert.Equal("enUS", Signature(profile, tag.Offset + 16));
        int length = Int32(profile, tag.Offset + 20), offset = Int32(profile, tag.Offset + 24);
        Assert.Equal(28, offset);
        Assert.Equal(0, length % 2);
        Assert.InRange(length, 0, tag.Length - offset);
        Assert.All(profile[(tag.Offset + offset + length)..(tag.Offset + tag.Length)], value => Assert.Equal(0, value));
        return Encoding.BigEndianUnicode.GetString(profile, tag.Offset + offset, length);
    }

    private static (byte[]? Icc, byte[] Coding) ReadIccAndCoding(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        using var coding = new MemoryStream();
        coding.Write(jpeg.AsSpan(0, 2));
        byte[]? profile = null;
        int offset = 2;
        while (jpeg[offset + 1] != 0xda)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int length = UInt16(jpeg, offset + 2), end = offset + 2 + length;
            Assert.InRange(length, 2, jpeg.Length - offset - 2);
            if (jpeg[offset + 1] == 0xe2)
            {
                Assert.Null(profile);
                Assert.True(jpeg.AsSpan(offset + 4, 12).SequenceEqual("ICC_PROFILE\0"u8));
                Assert.Equal(1, jpeg[offset + 16]);
                Assert.Equal(1, jpeg[offset + 17]);
                profile = jpeg[(offset + 18)..end];
            }
            else
                coding.Write(jpeg.AsSpan(offset, end - offset));
            offset = end;
        }
        coding.Write(jpeg.AsSpan(offset));
        return (profile, coding.ToArray());
    }

    private static Array Source(int kind, int width, int height, int stride)
    {
        int rowLength = width * 4;
        byte[] bytes = Enumerable.Repeat((byte)0xad, (height - 1) * stride + rowLength).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < 3; channel++)
                    bytes[y * stride + x * 4 + channel] = (byte)((x * 73 + y * 29 + channel * 61) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + index % 47 - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => index % stride >= rowLength || index % stride % 4 == 3
                ? float.NaN : Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
    }

    private static byte[] Encode(Array source, int width, int height, int stride,
        JpegliEncoderOptions options) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, PixelLayout.Bgra32,
            options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Bgra,
            options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, PixelComponentLayout.Bgra,
            options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array source, int width, int height, int stride,
        JpegliEncoderOptions options)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, PixelLayout.Bgra32,
                options, stride, TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken);
    }

    private static int Int32(byte[] bytes, int offset) => BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
    private static int UInt16(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
    private static double Fixed(byte[] bytes, int offset) => Int32(bytes, offset) / 65536.0;
    private static string Signature(byte[] bytes, int offset) => Encoding.ASCII.GetString(bytes, offset, 4);
    private sealed record Tag(int Offset, int Length);
    private static void AssertClose(double expected, double actual, double tolerance, string label) =>
        Assert.True(double.IsFinite(actual) && Math.Abs(expected - actual) <= tolerance,
            $"{label}: expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
}
