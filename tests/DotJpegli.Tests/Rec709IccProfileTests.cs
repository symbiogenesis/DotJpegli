using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class Rec709IccProfileTests
{
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];

    // Independent native fixtures from unchanged google/jpegli
    // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20,
    // with D65, sRGB/Rec.2020 primaries, BT709 transfer and perceptual intent,
    // compiled by VS2026 ARM64 C++17 /O2 /fp:strict.
    [Theory]
    [InlineData(false, "045567821C2B986EA4218C62D67068C26415CB5C7655FE7AE0F6C9CE0F5F6DAF", "CFBA11E85E5933B6BA8D908E88DFFB58")]
    [InlineData(true, "B69A102609B438F3D4CE0EEE90C85E77DEAA289257599EB81FB6CCA96EF43DA7", "5E074A7982BF282A685AF94289F51A07")]
    public void CompleteProfilesMatchThePinnedNativeFixtures(bool rec2020, string sha256, string id)
    {
        byte[] profile = Create(rec2020);
        Assert.Equal(536, profile.Length);
        Assert.Equal(sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal(id, Convert.ToHexString(profile.AsSpan(84, 16)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FactoriesReturnIndependentDeterministicStorageWithTheIccMd5Id(bool rec2020)
    {
        byte[] first = Create(rec2020), second = Create(rec2020);
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
        Array.Fill(first, (byte)0xdd);
        Assert.Equal(second, Create(rec2020));

        byte[] id = second[84..100];
        Array.Clear(second, 44, 4);
        Array.Clear(second, 64, 4);
        Array.Clear(second, 84, 16);
        Assert.Equal(id, MD5.HashData(second));
    }

    [Theory]
    [InlineData(false, "RGB_D65_SRG_Per_709", 1)]
    [InlineData(true, "RGB_D65_202_Per_709", 9)]
    public void HeaderAndAlignedTagsDescribeTheRequestedPrimariesAndSharedBt709Curves(
        bool rec2020, string description, int primaries)
    {
        byte[] profile = Create(rec2020);
        Assert.Equal(536, profile.Length);
        Assert.Equal(profile.Length, Int32(profile, 0));
        Assert.Equal("jxl ", Signature(profile, 4));
        Assert.Equal(0x04400000, Int32(profile, 8));
        Assert.Equal("mntr", Signature(profile, 12));
        Assert.Equal("RGB ", Signature(profile, 16));
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
        Assert.Equal(new[] { "desc", "cprt", "wtpt", "chad", "cicp", "rXYZ", "gXYZ", "bXYZ", "rTRC", "gTRC", "bTRC" },
            tags.Keys);
        Assert.Equal(68, tags["desc"].Length);
        Assert.Equal(description, Text(profile, tags["desc"]));
        Assert.Equal("CC0", Text(profile, tags["cprt"]));
        Tag white = tags["wtpt"];
        Assert.Equal("XYZ ", Signature(profile, white.Offset));
        Assert.Equal(20, white.Length);
        double[] expectedWhite = [0.964203, 1, 0.824905];
        for (int channel = 0; channel < 3; channel++)
            AssertClose(expectedWhite[channel], Fixed(profile, white.Offset + 8 + channel * 4),
                0.5 / 65536, $"media white channel {channel}");
        Assert.Equal("sf32", Signature(profile, tags["chad"].Offset));
        Assert.Equal(44, tags["chad"].Length);
        Tag cicp = tags["cicp"];
        Assert.Equal("cicp", Signature(profile, cicp.Offset));
        Assert.Equal(12, cicp.Length);
        Assert.Equal(new byte[] { (byte)primaries, 1, 0, 1 },
            profile[(cicp.Offset + 8)..(cicp.Offset + 12)]);
        Assert.Equal(1, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));

        Tag curve = tags["rTRC"];
        Assert.Equal("para", Signature(profile, curve.Offset));
        Assert.Equal(32, curve.Length);
        Assert.Equal(3, UInt16(profile, curve.Offset + 8));
        Assert.Equal(0, UInt16(profile, curve.Offset + 10));
        double[] parameters = [1 / 0.45, 1 / 1.099, 0.099 / 1.099, 1 / 4.5, 0.081];
        for (int index = 0; index < parameters.Length; index++)
            AssertClose(parameters[index], Fixed(profile, curve.Offset + 12 + index * 4),
                0.5 / 65536, $"BT709 curve parameter {index}");
        foreach (string color in new[] { "r", "g", "b" })
        {
            Assert.Equal(curve, tags[color + "TRC"]);
            Assert.Equal("XYZ ", Signature(profile, tags[color + "XYZ"].Offset));
            Assert.Equal(20, tags[color + "XYZ"].Length);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedCurvesMatchIndependentBt709EquationsAcrossNominalAndRoundedKnees(bool rec2020)
    {
        byte[] profile = Create(rec2020);
        Tag curve = Tags(profile)["rTRC"];
        double threshold = Fixed(profile, curve.Offset + 28);
        double[] values = [-0.125, -1e-8, 0, 1e-8, 0.001, 0.018, 0.04045,
            threshold - 1e-8, threshold, threshold + 1e-8,
            0.08099999, 0.081, 0.08100001, 0.125, 0.25, 0.5, 0.75, 1];
        foreach (double value in values.Concat(Enumerable.Range(0, 256).Select(value => value / 255.0)))
        {
            // BT709's rounded coefficients have a small discontinuity at the
            // knee; ICC quantizes the knee too. Preserve the selected branch
            // while comparing with independent double-precision equations.
            double expected = DecodeBt709(value, threshold);
            AssertClose(expected, EvaluateCurve(profile, curve, value), 0.00004,
                $"BT709 decode at {value:R}");
        }
        AssertClose((threshold - 1e-8) / 4.5, EvaluateCurve(profile, curve, threshold - 1e-8),
            0.000001, "linear branch immediately below the rounded knee");
        AssertClose(Math.Pow((threshold + 0.099) / 1.099, 1 / 0.45),
            EvaluateCurve(profile, curve, threshold), 0.00001, "power branch at the rounded knee");
        Assert.True(Math.Abs(EvaluateCurve(profile, curve, 0.5) - 0.5) > 0.2);
        double srgb = Math.Pow((0.5 + 0.055) / 1.055, 2.4);
        Assert.True(Math.Abs(EvaluateCurve(profile, curve, 0.5) - srgb) > 0.04);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedColorantsAndAdaptationMatchIndependentPrimariesAndBradfordOracles(bool rec2020)
    {
        byte[] profile = Create(rec2020);
        Dictionary<string, Tag> tags = Tags(profile);
        double[,] matrix = PrimariesToD65(rec2020), adaptation = BradfordD65ToD50();
        double threshold = Fixed(profile, tags["rTRC"].Offset + 28);
        double[] values = [-0.125, 0, 0.001, 0.081, 0.25, 0.5, 1];
        foreach (double r in values)
            foreach (double g in values)
                foreach (double b in values)
                    CheckColor(r, g, b);
        foreach (double value in new[] { threshold - 1e-8, threshold, threshold + 1e-8,
            0.08099999, 0.08100001 })
        {
            CheckColor(value, 0.25, 0.75);
            CheckColor(0.25, value, 0.75);
            CheckColor(0.25, 0.75, value);
        }

        Tag chad = tags["chad"];
        double[,] actualAdaptation = new double[3, 3];
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
            {
                actualAdaptation[row, column] = Fixed(profile, chad.Offset + 8 + (row * 3 + column) * 4);
                AssertClose(adaptation[row, column], actualAdaptation[row, column], 0.5 / 65536,
                    $"Bradford adaptation entry ({row}, {column})");
            }
        double[] adaptedWhite = Multiply(actualAdaptation, D65White());
        for (int channel = 0; channel < 3; channel++)
            AssertClose(new[] { 0.96422, 1, 0.82521 }[channel], adaptedWhite[channel],
                0.00003, $"adapted D65 white channel {channel}");

        void CheckColor(double r, double g, double b)
        {
            double[] input = [r, g, b];
            double[] expected = Multiply(adaptation,
                Multiply(matrix, input.Select(value => DecodeBt709(value, threshold)).ToArray()));
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
                    $"D50 XYZ channel {row}, encoded RGB ({r:R}, {g:R}, {b:R})");
            }
        }
    }

    public static TheoryData<bool, int, JpegColorSpace, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<bool, int, JpegColorSpace, bool>();
            foreach (bool rec2020 in new[] { false, true })
                for (int kind = 0; kind < 3; kind++)
                {
                    modes.Add(rec2020, kind, JpegColorSpace.YCbCr, kind == 1);
                    modes.Add(rec2020, kind, JpegColorSpace.Rgb, kind != 1);
                }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void ExplicitEmbeddingPreservesCodingAndPaddedSamplesAcrossPrecisionsAndScans(
        bool rec2020, int kind, JpegColorSpace colorSpace, bool progressive)
    {
        const int width = 19, height = 17, stride = width * 4 + 5;
        int refinement = progressive ? 2 : 0;
        Array pixels = Source(kind, width, height, stride);
        Array originalPixels = (Array)pixels.Clone();
        byte[] profile = Create(rec2020);
        byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        byte[] originalBacking = backing.ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, IccProfile = backing.AsMemory(3, profile.Length),
            Distance = 1.375f, SmoothingFactor = 37, Progressive = progressive,
            ProgressiveRefinementBits = refinement, RestartInterval = 2,
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
        Assert.Equal(actual, Encode(pixels, width, height, stride,
            options with { InferCicpTransferFunctionFromIccProfile = true }));
        Assert.Equal(actual, Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = 1 }));

        JpegDocument document = JpegDocument.Read(actual, progressive, 2, refinement);
        Assert.Equal((width, height), (document.Width, document.Height));
        Assert.Equal(3, document.Components.Count);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, stride, options);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
        Assert.Equal(originalBacking, backing);
        Assert.Equal(originalPixels.Cast<object>(), pixels.Cast<object>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TransferInferenceIsOptInAndExplicitHintsOverrideMetadataWithoutRewritingIt(bool rec2020)
    {
        Assert.False(new JpegliEncoderOptions().InferCicpTransferFunctionFromIccProfile);
        const int width = 19, height = 17, stride = width * 4 + 5;
        Array pixels = Source(0, width, height, stride);
        byte[] profile = Create(rec2020);
        var options = new JpegliEncoderOptions { IccProfile = profile, Distance = 2.375f };
        byte[] explicitHdr = Encode(pixels, width, height, stride, options with { CicpTransferFunction = 16 });
        Assert.Equal(explicitHdr, Encode(pixels, width, height, stride,
            options with { CicpTransferFunction = 16, InferCicpTransferFunctionFromIccProfile = true }));
        Assert.Equal(profile, ReadIccAndCoding(explicitHdr).Icc);
        byte[] unchangedCoding = ReadIccAndCoding(Encode(pixels, width, height, stride, options)).Coding;
        Assert.False(unchangedCoding.SequenceEqual(ReadIccAndCoding(explicitHdr).Coding));

        // CICP code 1 currently shares ordinary coding with unspecified transfer.
        // Change only that byte in a copied profile to make opt-in observable.
        byte[] modified = profile.ToArray();
        modified[Tags(modified)["cicp"].Offset + 9] = 16;
        var modifiedOptions = options with { IccProfile = modified };
        byte[] disabled = Encode(pixels, width, height, stride, modifiedOptions);
        Assert.Equal(unchangedCoding, ReadIccAndCoding(disabled).Coding);
        byte[] inferred = Encode(pixels, width, height, stride,
            modifiedOptions with { InferCicpTransferFunctionFromIccProfile = true });
        Assert.Equal(Encode(pixels, width, height, stride,
            modifiedOptions with { CicpTransferFunction = 16 }), inferred);
        Assert.Equal(ReadIccAndCoding(explicitHdr).Coding, ReadIccAndCoding(inferred).Coding);
        Assert.Equal(modified, ReadIccAndCoding(inferred).Icc);
        Assert.Equal(disabled, Encode(pixels, width, height, stride,
            modifiedOptions with { CicpTransferFunction = 1, InferCicpTransferFunctionFromIccProfile = true }));
        Assert.Equal(profile, Create(rec2020));
    }

    private static byte[] Create(bool rec2020) => rec2020
        ? JpegliIccProfiles.CreateRec2020Profile() : JpegliIccProfiles.CreateRec709Profile();

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        Assert.Equal(11, count);
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
        Assert.Equal(9, extents.Count);
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

    private static double DecodeBt709(double value, double threshold) => value < threshold
        ? value / 4.5 : Math.Pow((value + 0.099) / 1.099, 1 / 0.45);

    private static double[,] PrimariesToD65(bool rec2020)
    {
        // Derive from independent xy coordinates, including upstream's more
        // precise built-in sRGB chromaticities rather than rounded nominal ones.
        double[,] xy = rec2020
            ? new double[,] { { 0.708, 0.292 }, { 0.170, 0.797 }, { 0.131, 0.046 } }
            : new double[,] { { 0.639998686, 0.330010138 }, { 0.300003784, 0.600003357 },
                { 0.150002046, 0.059997204 } };
        double[,] primaries = new double[3, 3];
        for (int channel = 0; channel < 3; channel++)
        {
            double x = xy[channel, 0], y = xy[channel, 1];
            primaries[0, channel] = x / y;
            primaries[1, channel] = 1;
            primaries[2, channel] = (1 - x - y) / y;
        }
        double[] scale = Solve(primaries, D65White());
        for (int row = 0; row < 3; row++)
            for (int channel = 0; channel < 3; channel++)
                primaries[row, channel] *= scale[channel];
        return primaries;
    }

    private static double[,] BradfordD65ToD50()
    {
        double[,] bradford = { { 0.8951, 0.2664, -0.1614 },
            { -0.7502, 1.7135, 0.0367 }, { 0.0389, -0.0685, 1.0296 } };
        double[] source = Multiply(bradford, D65White());
        // Upstream adapts to analytic D50; wtpt and the header round separately.
        double[] destination = Multiply(bradford, [0.96422, 1, 0.82521]);
        double[,] result = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            double[] adapted = new double[3];
            for (int row = 0; row < 3; row++)
                adapted[row] = bradford[row, column] * destination[row] / source[row];
            double[] xyz = Solve(bradford, adapted);
            for (int row = 0; row < 3; row++)
                result[row, column] = xyz[row];
        }
        return result;
    }

    private static double[] Solve(double[,] matrix, double[] vector)
    {
        double[,] augmented = new double[3, 4];
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
                augmented[row, column] = matrix[row, column];
            augmented[row, 3] = vector[row];
        }
        for (int column = 0; column < 3; column++)
        {
            int pivot = column;
            for (int row = column + 1; row < 3; row++)
                if (Math.Abs(augmented[row, column]) > Math.Abs(augmented[pivot, column]))
                    pivot = row;
            for (int index = column; index < 4; index++)
                (augmented[column, index], augmented[pivot, index]) =
                    (augmented[pivot, index], augmented[column, index]);
            double divisor = augmented[column, column];
            for (int index = column; index < 4; index++)
                augmented[column, index] /= divisor;
            for (int row = 0; row < 3; row++)
            {
                if (row == column)
                    continue;
                double factor = augmented[row, column];
                for (int index = column; index < 4; index++)
                    augmented[row, index] -= factor * augmented[column, index];
            }
        }
        return [augmented[0, 3], augmented[1, 3], augmented[2, 3]];
    }

    private static double[] Multiply(double[,] matrix, double[] vector)
    {
        double[] result = new double[3];
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                result[row] += matrix[row, column] * vector[column];
        return result;
    }

    private static double[] D65White() => [0.3127 / 0.3290, 1, (1 - 0.3127 - 0.3290) / 0.3290];

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
