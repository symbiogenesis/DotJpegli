using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class XybIccProfileTests
{
    // An independent MSVC C++17 /fp:strict probe included the unchanged
    // lib/cms/jpegli_cms_internal.h from google/jpegli revision
    // 031a0077f5799a6041004267fc12b956c1f52a20 and called MaybeCreateProfile
    // with XYB/perceptual encoding. The full native profile is 720 bytes.
    private const string NativeSha256 = "1F0B56AD33BD18343B89F0F3338306EF9B80F1860053A7F6B8C98B569C04064F";
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];

    [Fact]
    public void MatchesTheCompletePinnedNativeProfile()
    {
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        Assert.Equal(720, profile.Length);
        Assert.Equal(NativeSha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal("558EDD940F32040699C68A17B40D3F7B", Convert.ToHexString(profile.AsSpan(84, 16)));
    }

    [Fact]
    public void EachCallOwnsItsMutableStorage()
    {
        byte[] first = JpegliIccProfiles.CreateXybProfile();
        byte[] second = JpegliIccProfiles.CreateXybProfile();
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
        Array.Fill(first, (byte)0xdd);
        Assert.Equal(NativeSha256, Convert.ToHexString(SHA256.HashData(second)));
        Assert.Equal(second, JpegliIccProfiles.CreateXybProfile());
    }

    [Fact]
    public void HeaderAndAlignedTagsDescribeAnXybInputProfileWithXyzPcs()
    {
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        Assert.Equal(profile.Length, Int32(profile, 0));
        Assert.Equal(0x04400000, Int32(profile, 8));
        Assert.Equal("scnr", Signature(profile, 12));
        Assert.Equal("RGB ", Signature(profile, 16));
        Assert.Equal("XYZ ", Signature(profile, 20));
        Assert.Equal("acsp", Signature(profile, 36));
        Assert.Equal(0, Int32(profile, 64)); // Perceptual rendering intent.
        Assert.Equal(0x0000f6d6, Int32(profile, 68));
        Assert.Equal(0x00010000, Int32(profile, 72));
        Assert.Equal(0x0000d32d, Int32(profile, 76));
        Assert.All(profile[100..128], value => Assert.Equal(0, value));

        Dictionary<string, Tag> tags = Tags(profile);
        Assert.Equal(new[] { "A2B0", "B2A0", "chad", "cprt", "desc", "wtpt" }, tags.Keys.Order());
        Assert.Equal("XYB_Per", Text(profile, tags["desc"]));
        Assert.Equal("CC0", Text(profile, tags["cprt"]));
        Tag white = tags["wtpt"];
        Assert.Equal("XYZ ", Signature(profile, white.Offset));
        AssertClose(0.964203, Fixed(profile, white.Offset + 8), 1.0 / 65536, "media white X");
        AssertClose(1, Fixed(profile, white.Offset + 12), 1.0 / 65536, "media white Y");
        AssertClose(0.824905, Fixed(profile, white.Offset + 16), 1.0 / 65536, "media white Z");
        Assert.Equal("sf32", Signature(profile, tags["chad"].Offset));
        Assert.Equal(44, tags["chad"].Length);
    }

    [Fact]
    public void ProfileIdIsTheIndependentIccMd5Digest()
    {
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        byte[] id = profile[84..100];
        // ICC.1 profile identification omits flags, intent and the ID itself.
        Array.Clear(profile, 44, 4);
        Array.Clear(profile, 64, 4);
        Array.Clear(profile, 84, 16);
        Assert.Equal(id, MD5.HashData(profile));
    }

    [Fact]
    public void ParsedA2BTransformReturnsD50XyzForAnIndependentSrgbColorCube()
    {
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        Tag transform = Tags(profile)["A2B0"];
        Assert.Equal("mAB ", Signature(profile, transform.Offset));
        ValidateTransformStages(profile, transform);

        double[] values = [0, 0.001, 0.01, 0.04044999, 0.04045, 0.04045001, 0.0625, 0.25, 0.5, 0.75, 1];
        foreach (double r in values)
            foreach (double g in values)
                foreach (double b in values)
                {
                    double[] expected = SrgbToD50Xyz(r, g, b);
                    double[] actual = EvaluateA2B(profile, transform, SrgbToScaledXyb(r, g, b));
                    for (int channel = 0; channel < 3; channel++)
                        AssertClose(expected[channel], actual[channel], 0.0003,
                            $"PCS channel {channel}, sRGB ({r:R}, {g:R}, {b:R})");
                }
    }

    [Fact]
    public void B2ATagIsTheUpstreamIdentityReaderCompatibilityTransform()
    {
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        Tag tag = Tags(profile)["B2A0"];
        Assert.Equal("mBA ", Signature(profile, tag.Offset));
        Assert.Equal(3, profile[tag.Offset + 8]);
        Assert.Equal(3, profile[tag.Offset + 9]);
        Assert.Equal(32, Int32(profile, tag.Offset + 12));
        for (int offset = 16; offset <= 28; offset += 4)
            Assert.Equal(0, Int32(profile, tag.Offset + offset));
        Assert.Equal(80, tag.Length);
        foreach (double value in new[] { 0, 0.125, 0.5, 0.875, 1 })
            for (int channel = 0; channel < 3; channel++)
                Assert.Equal(value, Curve(profile, tag.Offset + 32 + channel * 16, value));
    }

    public static TheoryData<int, bool, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<int, bool, bool>();
            for (int kind = 0; kind < 3; kind++)
                foreach (bool converted in new[] { false, true })
                    foreach (bool progressive in new[] { false, true })
                        modes.Add(kind, converted, progressive);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void ExplicitProfileEmbedsExactBytesWithoutChangingDirectOrConvertedXybCoding(
        int kind, bool converted, bool progressive)
    {
        const int width = 19, height = 17;
        byte[] bytes = Enumerable.Range(0, width * height * 3)
            .Select(index => (byte)((index * 73 + index / 17 * 29) % 256)).ToArray();
        Array pixels = kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + index % 47 - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
        PixelComponentLayout layout = converted ? PixelComponentLayout.Rgb : PixelComponentLayout.Xyb;
        byte[] profile = JpegliIccProfiles.CreateXybProfile();
        byte[] original = profile.ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.Xyb, IccProfile = profile,
            InferCicpTransferFunctionFromIccProfile = true, Distance = 1.375f,
            SmoothingFactor = 37, Progressive = progressive,
            ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 2
        };
        byte[] actual = Encode(pixels, width, height, layout, options);
        byte[] opaque = Encode(pixels, width, height, layout, options with { IccProfile = OpaqueProfile });
        (byte[] embedded, byte[] coding) = ReadIccAndCoding(actual);
        Assert.Equal(original, embedded);
        Assert.Equal(ReadIccAndCoding(opaque).Coding, coding);
        Assert.Equal(original, profile);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, layout, options);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(PixelLayout.Xyb24)]
    [InlineData(PixelLayout.Rgb24)]
    public void FactoryAvailabilityDoesNotMakeAnEmptyXybProfileImplicit(PixelLayout layout)
    {
        using var output = new MemoryStream();
        output.Write([0x12, 0x34]);
        var options = new JpegliEncoderOptions { ColorSpace = JpegColorSpace.Xyb };
        ArgumentException error = Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output,
            new byte[8 * 8 * 3], 8, 8, layout, options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("IccProfile", error.ParamName);
        Assert.Equal(new byte[] { 0x12, 0x34 }, output.ToArray());
    }

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        Assert.InRange(count, 1, (profile.Length - 132) / 12);
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
        for (int index = 1; index < ordered.Length; index++)
            Assert.True(ordered[index].Offset >= ordered[index - 1].Offset + ordered[index - 1].Length);
        return result;
    }

    private static string Text(byte[] profile, Tag tag)
    {
        Assert.Equal("mluc", Signature(profile, tag.Offset));
        Assert.Equal(1, Int32(profile, tag.Offset + 8));
        Assert.Equal(12, Int32(profile, tag.Offset + 12));
        Assert.Equal("enUS", Signature(profile, tag.Offset + 16));
        int length = Int32(profile, tag.Offset + 20), offset = Int32(profile, tag.Offset + 24);
        Assert.Equal(0, length % 2);
        Assert.InRange(offset, 28, tag.Length);
        Assert.InRange(length, 0, tag.Length - offset);
        return Encoding.BigEndianUnicode.GetString(profile, tag.Offset + offset, length);
    }

    private static void ValidateTransformStages(byte[] profile, Tag tag)
    {
        Assert.Equal(3, profile[tag.Offset + 8]);
        Assert.Equal(3, profile[tag.Offset + 9]);
        Assert.Equal(0, UInt16(profile, tag.Offset + 10));
        foreach ((int field, int bytes) in new[] { (12, 48), (16, 48), (20, 96), (24, 68), (28, 48) })
        {
            int offset = Int32(profile, tag.Offset + field);
            Assert.Equal(0, offset % 4);
            Assert.InRange(offset, 32, tag.Length - bytes);
        }
        int clut = tag.Offset + Int32(profile, tag.Offset + 24);
        Assert.Equal(new byte[] { 2, 2, 2 }, profile[clut..(clut + 3)]);
        Assert.All(profile[(clut + 3)..(clut + 16)], value => Assert.Equal(0, value));
        Assert.Equal(2, profile[clut + 16]);
        Assert.All(profile[(clut + 17)..(clut + 20)], value => Assert.Equal(0, value));
    }

    // Decode the ICC stages from the profile, without production serialization
    // or color-conversion helpers: A curves -> 16-bit CLUT -> M curves ->
    // affine matrix -> B curves. XYZ PCS 1.0 encodes as 32768, not 65535.
    private static double[] EvaluateA2B(byte[] profile, Tag tag, double[] input)
    {
        int a = tag.Offset + Int32(profile, tag.Offset + 28);
        for (int channel = 0; channel < 3; channel++)
            input[channel] = Curve(profile, a + channel * 16, Math.Clamp(input[channel], 0, 1));
        int clut = tag.Offset + Int32(profile, tag.Offset + 24) + 20;
        var mixed = new double[3];
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int b = 0; b < 2; b++)
                {
                    double weight = (x == 0 ? 1 - input[0] : input[0])
                        * (y == 0 ? 1 - input[1] : input[1]) * (b == 0 ? 1 - input[2] : input[2]);
                    int corner = clut + ((x * 2 + y) * 2 + b) * 6;
                    for (int channel = 0; channel < 3; channel++)
                        mixed[channel] += weight * UInt16(profile, corner + channel * 2) / 65535.0;
                }
        int m = tag.Offset + Int32(profile, tag.Offset + 20);
        for (int channel = 0; channel < 3; channel++)
            mixed[channel] = Curve(profile, m + channel * 32, mixed[channel]);
        int matrix = tag.Offset + Int32(profile, tag.Offset + 16);
        int curves = tag.Offset + Int32(profile, tag.Offset + 12);
        var output = new double[3];
        for (int row = 0; row < 3; row++)
        {
            double value = Fixed(profile, matrix + 36 + row * 4);
            for (int column = 0; column < 3; column++)
                value += Fixed(profile, matrix + (row * 3 + column) * 4) * mixed[column];
            output[row] = Curve(profile, curves + row * 16, value) * (65535.0 / 32768);
        }
        return output;
    }

    private static double Curve(byte[] profile, int offset, double value)
    {
        Assert.Equal("para", Signature(profile, offset));
        Assert.Equal(0, Int32(profile, offset + 4));
        Assert.Equal(0, UInt16(profile, offset + 10));
        int kind = UInt16(profile, offset + 8);
        double gamma = Fixed(profile, offset + 12);
        if (kind == 0)
            return Math.Pow(Math.Max(0, value), gamma);
        Assert.Equal(3, kind);
        double a = Fixed(profile, offset + 16), b = Fixed(profile, offset + 20);
        double c = Fixed(profile, offset + 24), d = Fixed(profile, offset + 28);
        return value >= d ? Math.Pow(a * value + b, gamma) : c * value;
    }

    private static double[] SrgbToScaledXyb(double r, double g, double b)
    {
        double[] rgb = [Linearize(r), Linearize(g), Linearize(b)];
        double[,] absorbance =
        {
            { 0.30, 0.622, 0.078 },
            { 0.23, 0.692, 0.078 },
            { 0.24342268924547819, 0.20476744424496821, 0.5518098665095536 }
        };
        const double bias = 0.0037930732552754493;
        var cones = new double[3];
        for (int row = 0; row < 3; row++)
        {
            double value = bias;
            for (int column = 0; column < 3; column++)
                value += absorbance[row, column] * rgb[column];
            cones[row] = Math.Cbrt(value) - Math.Cbrt(bias);
        }
        double x = (cones[0] - cones[1]) / 2, y = (cones[0] + cones[1]) / 2;
        return [(x + 0.015386134) * 22.995788804, y * 1.183000077,
            (cones[2] - y + 0.27770459) * 1.502141333];
    }

    private static double[] SrgbToD50Xyz(double r, double g, double b)
    {
        // Independent analytic encoded-sRGB -> linear RGB -> Bradford-adapted
        // D50 XYZ reference. This does not use the profile's opsin inverse.
        r = Linearize(r);
        g = Linearize(g);
        b = Linearize(b);
        return [0.4360747 * r + 0.3850649 * g + 0.1430804 * b,
            0.2225045 * r + 0.7168786 * g + 0.0606169 * b,
            0.0139322 * r + 0.0971045 * g + 0.7141733 * b];
    }

    private static double Linearize(double value) => value <= 0.04045
        ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);

    private static (byte[] Icc, byte[] Coding) ReadIccAndCoding(byte[] jpeg)
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
        Assert.NotNull(profile);
        return (profile, coding.ToArray());
    }

    private static byte[] Encode(Array source, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)(int)layout,
            options, cancellationToken: TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout,
            options, cancellationToken: TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, layout,
            options, cancellationToken: TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array source, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout,
                options, cancellationToken: TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout,
                options, cancellationToken: TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, layout,
                options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static int Int32(byte[] profile, int offset) => BinaryPrimitives.ReadInt32BigEndian(profile.AsSpan(offset, 4));
    private static int UInt16(byte[] profile, int offset) => BinaryPrimitives.ReadUInt16BigEndian(profile.AsSpan(offset, 2));
    private static double Fixed(byte[] profile, int offset) => Int32(profile, offset) / 65536.0;
    private static string Signature(byte[] profile, int offset) => Encoding.ASCII.GetString(profile, offset, 4);
    private sealed record Tag(int Offset, int Length);

    private static void AssertClose(double expected, double actual, double tolerance, string label) =>
        Assert.True(double.IsFinite(actual) && Math.Abs(expected - actual) <= tolerance,
            $"{label}: expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
}
