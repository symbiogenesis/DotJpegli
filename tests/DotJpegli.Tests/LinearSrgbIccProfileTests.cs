using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class LinearSrgbIccProfileTests
{
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];

    // Independent VS2026 ARM64 C++17 /O2 /fp:strict fixtures from unchanged
    // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20.
    // MaybeCreateProfile uses D65, linear transfer, perceptual intent and RGB's
    // precise built-in sRGB primaries from color_encoding_cms.h GetPrimaries.
    [Theory]
    [InlineData(false, "138747C4D990CAFE065BB407D55219D3B7FB7F22CF803FCD07D85915514028D7", "AC25B029F881260EB2154A2D8B1CAC5B")]
    [InlineData(true, "D05AF72D689C7497399BFE7BEB87AE7C3687AAB4AB9347C16DCAAFA2325C0404", "83DF0E7B027D00AC32774D1BB8D17F1D")]
    public void CompleteProfilesMatchThePinnedNativeFixtures(bool grayscale, string sha256, string id)
    {
        byte[] profile = Create(grayscale);
        Assert.Equal(sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal(id, Convert.ToHexString(profile.AsSpan(84, 16)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EachFactoryReturnsIndependentDeterministicStorageWithTheIccMd5Id(bool grayscale)
    {
        byte[] first = Create(grayscale), second = Create(grayscale);
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
        Array.Fill(first, (byte)0xdd);
        Assert.Equal(second, Create(grayscale));

        byte[] id = second[84..100];
        Array.Clear(second, 44, 4);
        Array.Clear(second, 64, 4);
        Array.Clear(second, 84, 16);
        Assert.Equal(id, MD5.HashData(second));
    }

    [Theory]
    [InlineData(false, 536, "RGB ", "RGB_D65_SRG_Per_Lin", 68, 8)]
    [InlineData(true, 328, "GRAY", "Gra_D65_Per_Lin", 60, 2)]
    public void HeaderTagsAndWhiteMatchThePinnedUpstreamLinearBranch(bool grayscale, int size,
        string colorSpace, string description, int descriptionSize, int transfer)
    {
        // These descriptions and tag layouts follow ColorEncodingDescriptionImpl
        // and MaybeCreateProfileImpl in google/jpegli's unchanged
        // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20.
        byte[] profile = Create(grayscale);
        Assert.Equal(size, profile.Length);
        Assert.Equal(size, Int32(profile, 0));
        Assert.Equal("jxl ", Signature(profile, 4));
        Assert.Equal(0x04400000, Int32(profile, 8));
        Assert.Equal("mntr", Signature(profile, 12));
        Assert.Equal(colorSpace, Signature(profile, 16));
        Assert.Equal("XYZ ", Signature(profile, 20));
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
        Assert.Equal(grayscale
            ? new[] { "desc", "cprt", "wtpt", "kTRC" }
            : new[] { "desc", "cprt", "wtpt", "chad", "cicp", "rXYZ", "gXYZ", "bXYZ", "rTRC", "gTRC", "bTRC" },
            tags.Keys);
        Assert.Equal(descriptionSize, tags["desc"].Length);
        Assert.Equal(description, Text(profile, tags["desc"]));
        Assert.Equal("CC0", Text(profile, tags["cprt"]));
        Assert.Equal(transfer, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));

        Tag white = tags["wtpt"];
        Assert.Equal("XYZ ", Signature(profile, white.Offset));
        Assert.Equal(20, white.Length);
        // Upstream retains D65 for GRAY, without a chad tag, whereas RGB has
        // D50 media white. Both headers use the separately rounded D50 PCS white.
        double[] expectedWhite = grayscale
            ? [0.3127 / 0.3290, 1, (1 - 0.3127 - 0.3290) / 0.3290]
            : [0.964203, 1, 0.824905];
        for (int channel = 0; channel < 3; channel++)
            AssertClose(expectedWhite[channel], Fixed(profile, white.Offset + 8 + channel * 4),
                0.5 / 65536, $"media white channel {channel}");

        Tag curve = tags[grayscale ? "kTRC" : "rTRC"];
        Assert.Equal(32, curve.Length);
        Assert.Equal("para", Signature(profile, curve.Offset));
        Assert.Equal(3, UInt16(profile, curve.Offset + 8));
        Assert.Equal(0, UInt16(profile, curve.Offset + 10));
        Assert.Equal(new[] { 1d, 1d, 0d, 1d, 0d },
            Enumerable.Range(0, 5).Select(index => Fixed(profile, curve.Offset + 12 + index * 4)));
        if (!grayscale)
        {
            Assert.Equal(curve, tags["gTRC"]);
            Assert.Equal(curve, tags["bTRC"]);
            Assert.Equal("sf32", Signature(profile, tags["chad"].Offset));
            Assert.Equal(44, tags["chad"].Length);
            Tag cicp = tags["cicp"];
            Assert.Equal("cicp", Signature(profile, cicp.Offset));
            Assert.Equal(12, cicp.Length);
            Assert.Equal(new byte[] { 1, 8, 0, 1 }, profile[(cicp.Offset + 8)..(cicp.Offset + 12)]);
            foreach (string color in new[] { "r", "g", "b" })
            {
                Assert.Equal("XYZ ", Signature(profile, tags[color + "XYZ"].Offset));
                Assert.Equal(20, tags[color + "XYZ"].Length);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedCurveIsIdentityForSampleValuesAndBothSidesOfItsZeroThreshold(bool grayscale)
    {
        byte[] profile = Create(grayscale);
        Tag curve = Tags(profile)[grayscale ? "kTRC" : "rTRC"];
        foreach (double value in Enumerable.Range(0, 256).Select(value => value / 255.0)
            .Concat(new[] { -0.25, -1e-8, 0, 1e-8, 0.04045, 0.5, 1 }))
            Assert.Equal(value, EvaluateCurve(profile, curve, value));
    }

    [Fact]
    public void ParsedRgbMatrixMapsLinearSamplesDirectlyToD50Xyz()
    {
        byte[] profile = Create(false);
        Dictionary<string, Tag> tags = Tags(profile);
        double[] values = [0, 0.001, 0.04045, 0.25, 0.5, 0.75, 1];
        foreach (double r in values)
            foreach (double g in values)
                foreach (double b in values)
                {
                    // Independent analytic linear sRGB -> Bradford-adapted D50
                    // reference. No encoded-sRGB decoding is applied here.
                    double[] expected = [0.4360747 * r + 0.3850649 * g + 0.1430804 * b,
                        0.2225045 * r + 0.7168786 * g + 0.0606169 * b,
                        0.0139322 * r + 0.0971045 * g + 0.7141733 * b];
                    double[] input = [r, g, b];
                    for (int row = 0; row < 3; row++)
                    {
                        double actual = 0;
                        for (int channel = 0; channel < 3; channel++)
                        {
                            string color = new[] { "r", "g", "b" }[channel];
                            actual += Fixed(profile, tags[color + "XYZ"].Offset + 8 + row * 4)
                                * EvaluateCurve(profile, tags[color + "TRC"], input[channel]);
                        }
                        AssertClose(expected[row], actual, 0.0001,
                            $"D50 XYZ channel {row} for linear RGB ({r:R}, {g:R}, {b:R})");
                    }
                }

        Tag chad = tags["chad"];
        double[] d65 = [0.3127 / 0.3290, 1, (1 - 0.3127 - 0.3290) / 0.3290];
        double[] d50 = [0.96422, 1, 0.82521];
        for (int row = 0; row < 3; row++)
        {
            double actual = 0;
            for (int column = 0; column < 3; column++)
                actual += Fixed(profile, chad.Offset + 8 + (row * 3 + column) * 4) * d65[column];
            AssertClose(d50[row], actual, 0.00003, $"adapted D65 white channel {row}");
        }
    }

    [Theory]
    [InlineData(0, JpegColorSpace.YCbCr, false, 0)]
    [InlineData(1, JpegColorSpace.Rgb, true, 0)]
    [InlineData(2, JpegColorSpace.YCbCr, true, 2)]
    [InlineData(0, JpegColorSpace.Grayscale, true, 2)]
    [InlineData(1, JpegColorSpace.Grayscale, false, 0)]
    [InlineData(2, JpegColorSpace.Grayscale, true, 0)]
    public void ExplicitEmbeddingPreservesCodingAndPaddedSamplesAcrossTypedInputsAndScans(
        int kind, JpegColorSpace colorSpace, bool progressive, int refinement)
    {
        const int width = 19, height = 17;
        bool grayscale = colorSpace == JpegColorSpace.Grayscale;
        PixelComponentLayout layout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Bgra;
        int components = grayscale ? 1 : 4, stride = width * components + 5;
        Array pixels = Source(kind, width, height, components, stride);
        Array original = (Array)pixels.Clone();
        byte[] profile = Create(grayscale);
        byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        byte[] originalBacking = backing.ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, IccProfile = backing.AsMemory(3, profile.Length),
            InferCicpTransferFunctionFromIccProfile = true, Distance = 1.375f,
            SmoothingFactor = 37, Progressive = progressive,
            ProgressiveRefinementBits = refinement, RestartInterval = 2
        };
        byte[] actual = Encode(pixels, width, height, layout, options, stride);
        (byte[]? embedded, byte[] coding) = ReadIccAndCoding(actual);
        Assert.Equal(profile, embedded);
        Assert.Equal(coding, ReadIccAndCoding(Encode(pixels, width, height, layout,
            options with { IccProfile = OpaqueProfile, InferCicpTransferFunctionFromIccProfile = false }, stride)).Coding);
        var withoutProfile = ReadIccAndCoding(Encode(pixels, width, height, layout,
            options with { IccProfile = default }, stride));
        Assert.Null(withoutProfile.Icc);
        Assert.Equal(coding, withoutProfile.Coding);
        Assert.Equal(actual, Encode(pixels, width, height, layout,
            options with { InferCicpTransferFunctionFromIccProfile = false }, stride));
        Assert.Equal(actual, Encode(pixels, width, height, layout,
            options with { CicpTransferFunction = 8, InferCicpTransferFunctionFromIccProfile = false }, stride));

        JpegDocument document = JpegDocument.Read(actual, progressive, 2, refinement);
        Assert.Equal((width, height), (document.Width, document.Height));
        Assert.Equal(grayscale ? 1 : 3, document.Components.Count);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, layout, options, stride);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
        Assert.Equal(originalBacking, backing);
        Assert.Equal(original.Cast<object>(), pixels.Cast<object>());
    }

    [Fact]
    public void ExplicitTransferHintOverridesTheBuiltInLinearProfileMetadata()
    {
        byte[] profile = Create(false);
        Array pixels = Source(0, 19, 17, 4, 19 * 4 + 5);
        var options = new JpegliEncoderOptions
        {
            IccProfile = profile, InferCicpTransferFunctionFromIccProfile = true,
            CicpTransferFunction = 16, Distance = 2.375f
        };
        byte[] actual = Encode(pixels, 19, 17, PixelComponentLayout.Bgra, options, 19 * 4 + 5);
        Assert.Equal(actual, Encode(pixels, 19, 17, PixelComponentLayout.Bgra,
            options with { InferCicpTransferFunctionFromIccProfile = false }, 19 * 4 + 5));
        Assert.Equal(profile, ReadIccAndCoding(actual).Icc);
        Assert.False(actual.SequenceEqual(Encode(pixels, 19, 17, PixelComponentLayout.Bgra,
            options with { CicpTransferFunction = 2 }, 19 * 4 + 5)));
        Assert.Equal(8, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ExistingBuiltinProfilesRemainUnchanged()
    {
        Assert.Equal("1406EFC5429250FEBE778C182EB5E5168AA14B2CAEFCF772211A2EC2ECF31659",
            Convert.ToHexString(SHA256.HashData(JpegliIccProfiles.CreateSrgbProfile())));
        Assert.Equal("BD99A693D2BA41BA6C0E818B2D7EE2B2A313289AD5B9C66FEFD54D09FFCA6406",
            Convert.ToHexString(SHA256.HashData(JpegliIccProfiles.CreateSrgbGrayscaleProfile())));
        byte[] xyb = JpegliIccProfiles.CreateXybProfile();
        Assert.Equal("1F0B56AD33BD18343B89F0F3338306EF9B80F1860053A7F6B8C98B569C04064F",
            Convert.ToHexString(SHA256.HashData(xyb)));
    }

    private static byte[] Create(bool grayscale) => grayscale
        ? JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile() : JpegliIccProfiles.CreateLinearSrgbProfile();

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        var result = new Dictionary<string, Tag>();
        var extents = new HashSet<Tag>();
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
        Assert.Equal(count == 4 ? 4 : 9, extents.Count);
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

    private static double EvaluateCurve(byte[] profile, Tag curve, double value)
    {
        double gamma = Fixed(profile, curve.Offset + 12), a = Fixed(profile, curve.Offset + 16),
            b = Fixed(profile, curve.Offset + 20), c = Fixed(profile, curve.Offset + 24),
            threshold = Fixed(profile, curve.Offset + 28);
        return value >= threshold ? Math.Pow(a * value + b, gamma) : c * value;
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
            int length = UInt16(jpeg, offset + 2);
            Assert.InRange(length, 2, jpeg.Length - offset - 2);
            int end = offset + 2 + length;
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

    private static Array Source(int kind, int width, int height, int components, int stride)
    {
        int rowLength = width * components;
        byte[] bytes = Enumerable.Repeat((byte)0xad, (height - 1) * stride + rowLength).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < (components == 1 ? 1 : 3); channel++)
                    bytes[y * stride + x * components + channel] = (byte)((x * 73 + y * 29 + channel * 61) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + index % 47 - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => index % stride >= rowLength || components == 4 && index % stride % 4 == 3
                ? float.NaN : Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
    }

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride) => pixels switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)(int)layout,
            options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout,
            options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])pixels, width, height, layout,
            options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride)
    {
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout,
                options, stride, TestContext.Current.CancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout,
                options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout,
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
