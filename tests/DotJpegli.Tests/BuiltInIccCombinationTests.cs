using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class BuiltInIccCombinationTests
{
    private static readonly IccWhitePoint[] Whites =
        [IccWhitePoint.D65, IccWhitePoint.EqualEnergy, IccWhitePoint.Dci];
    private static readonly IccPrimaries[] Primaries =
        [IccPrimaries.Srgb, IccPrimaries.DisplayP3, IccPrimaries.Rec2020];
    private static readonly IccTransferFunction[] Transfers =
        [IccTransferFunction.Rec709, IccTransferFunction.Linear, IccTransferFunction.Srgb,
         IccTransferFunction.Pq, IccTransferFunction.Dci, IccTransferFunction.Hlg];

    public static TheoryData<string> NativeCases => new(NativeFixtures.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Trim()));

    [Fact]
    public void PublicEnumsRetainTheNativeNamedValuesAndFactoriesDefaultToPerceptual()
    {
        string[] keys = NativeFixtures.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(row => row.Trim().Split(' ')[0]).ToArray();
        Assert.Equal(336, keys.Length);
        Assert.Equal(336, keys.Distinct().Count());
        Assert.Equal(288, keys.Count(key => key.Split('-')[3] != "6"));
        Assert.Equal(48, keys.Count(key => key.Split('-')[3] == "6"));
        Assert.Equal(new[] { "D65", "EqualEnergy", "Dci" }, Enum.GetNames<IccWhitePoint>());
        Assert.Equal(new[] { 1, 10, 11 }, Enum.GetValues<IccWhitePoint>().Select(value => (int)value));
        Assert.Equal(new[] { "Srgb", "Rec2020", "DisplayP3" }, Enum.GetNames<IccPrimaries>());
        Assert.Equal(new[] { 1, 9, 11 }, Enum.GetValues<IccPrimaries>().Select(value => (int)value));
        Assert.Equal(new[] { "Rec709", "Linear", "Srgb", "Pq", "Dci", "Hlg" }, Enum.GetNames<IccTransferFunction>());
        Assert.Equal(new[] { 1, 8, 13, 16, 17, 18 }, Enum.GetValues<IccTransferFunction>().Select(value => (int)value));
        foreach (IccWhitePoint white in Whites)
        {
            foreach (IccTransferFunction transfer in Transfers)
            {
                Assert.Equal(JpegliIccProfiles.CreateGrayscaleProfile(white, transfer),
                    JpegliIccProfiles.CreateGrayscaleProfile(white, transfer, IccRenderingIntent.Perceptual));
                foreach (IccPrimaries primaries in Primaries)
                    Assert.Equal(JpegliIccProfiles.CreateRgbProfile(primaries, white, transfer),
                        JpegliIccProfiles.CreateRgbProfile(primaries, white, transfer, IccRenderingIntent.Perceptual));
            }
            Assert.Equal(JpegliIccProfiles.CreateGammaGrayscaleProfile(white, 2.2),
                JpegliIccProfiles.CreateGammaGrayscaleProfile(white, 2.2, IccRenderingIntent.Perceptual));
            foreach (IccPrimaries primaries in Primaries)
                Assert.Equal(JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, 2.2),
                    JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, 2.2, IccRenderingIntent.Perceptual));
        }
    }

    [Theory]
    [MemberData(nameof(NativeCases))]
    public void EveryNamedCombinationAndGammaMatchesAnIndependentCompleteNativeProfile(string row)
    {
        Case item = Parse(row);
        byte[] profile = Create(item), second = Create(item);
        Assert.Equal(item.Size, profile.Length);
        Assert.Equal(item.Description, Description(profile));
        Assert.Equal(item.Sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal(item.Id, Convert.ToHexString(profile.AsSpan(84, 16)));
        Assert.NotSame(profile, second);
        Assert.Equal(profile, second);
        ValidateStructure(profile, item);
        Array.Fill(profile, (byte)0xd7);
        Assert.Equal(second, Create(item));
    }

    [Fact]
    public void EveryExistingFactoryIsByteCompatibleForEveryRenderingIntent()
    {
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
        {
            Assert.Equal(JpegliIccProfiles.CreateSrgbProfile(intent), Rgb(IccPrimaries.Srgb, IccTransferFunction.Srgb));
            Assert.Equal(JpegliIccProfiles.CreateDisplayP3Profile(intent), Rgb(IccPrimaries.DisplayP3, IccTransferFunction.Srgb));
            Assert.Equal(JpegliIccProfiles.CreateSrgbRec2020Profile(intent), Rgb(IccPrimaries.Rec2020, IccTransferFunction.Srgb));
            Assert.Equal(JpegliIccProfiles.CreateRec709Profile(intent), Rgb(IccPrimaries.Srgb, IccTransferFunction.Rec709));
            Assert.Equal(JpegliIccProfiles.CreateRec709DisplayP3Profile(intent), Rgb(IccPrimaries.DisplayP3, IccTransferFunction.Rec709));
            Assert.Equal(JpegliIccProfiles.CreateRec2020Profile(intent), Rgb(IccPrimaries.Rec2020, IccTransferFunction.Rec709));
            Assert.Equal(JpegliIccProfiles.CreateLinearSrgbProfile(intent), Rgb(IccPrimaries.Srgb, IccTransferFunction.Linear));
            Assert.Equal(JpegliIccProfiles.CreateLinearDisplayP3Profile(intent), Rgb(IccPrimaries.DisplayP3, IccTransferFunction.Linear));
            Assert.Equal(JpegliIccProfiles.CreateLinearRec2020Profile(intent), Rgb(IccPrimaries.Rec2020, IccTransferFunction.Linear));
            Assert.Equal(JpegliIccProfiles.CreateRec2100PqProfile(intent), Rgb(IccPrimaries.Rec2020, IccTransferFunction.Pq));
            Assert.Equal(JpegliIccProfiles.CreateRec2100HlgProfile(intent), Rgb(IccPrimaries.Rec2020, IccTransferFunction.Hlg));
            Assert.Equal(JpegliIccProfiles.CreateDciP3Profile(intent),
                JpegliIccProfiles.CreateRgbProfile(IccPrimaries.DisplayP3, IccWhitePoint.Dci, IccTransferFunction.Dci, intent));
            Assert.Equal(JpegliIccProfiles.CreateSrgbGrayscaleProfile(intent), Gray(IccTransferFunction.Srgb));
            Assert.Equal(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(intent), Gray(IccTransferFunction.Linear));
            Assert.Equal(JpegliIccProfiles.CreateRec709GrayscaleProfile(intent), Gray(IccTransferFunction.Rec709));
            Assert.Equal(JpegliIccProfiles.CreateRec2100PqGrayscaleProfile(intent), Gray(IccTransferFunction.Pq));
            Assert.Equal(JpegliIccProfiles.CreateRec2100HlgGrayscaleProfile(intent), Gray(IccTransferFunction.Hlg));
            Assert.Equal(JpegliIccProfiles.CreateDciGrayscaleProfile(intent),
                JpegliIccProfiles.CreateGrayscaleProfile(IccWhitePoint.Dci, IccTransferFunction.Dci, intent));
            foreach (double gamma in new[] { 1.0, 2.2, 2.6, 8192.0 })
            {
                Assert.Equal(JpegliIccProfiles.CreateGammaSrgbProfile(gamma, intent),
                    JpegliIccProfiles.CreateGammaRgbProfile(IccPrimaries.Srgb, IccWhitePoint.D65, gamma, intent));
                Assert.Equal(JpegliIccProfiles.CreateGammaDisplayP3Profile(gamma, intent),
                    JpegliIccProfiles.CreateGammaRgbProfile(IccPrimaries.DisplayP3, IccWhitePoint.D65, gamma, intent));
                Assert.Equal(JpegliIccProfiles.CreateGammaRec2020Profile(gamma, intent),
                    JpegliIccProfiles.CreateGammaRgbProfile(IccPrimaries.Rec2020, IccWhitePoint.D65, gamma, intent));
                Assert.Equal(JpegliIccProfiles.CreateGammaGrayscaleProfile(gamma, intent),
                    JpegliIccProfiles.CreateGammaGrayscaleProfile(IccWhitePoint.D65, gamma, intent));
            }
            byte[] Rgb(IccPrimaries primaries, IccTransferFunction transfer) =>
                JpegliIccProfiles.CreateRgbProfile(primaries, IccWhitePoint.D65, transfer, intent);
            byte[] Gray(IccTransferFunction transfer) => JpegliIccProfiles.CreateGrayscaleProfile(IccWhitePoint.D65, transfer, intent);
        }
    }

    [Fact]
    public void InvalidEnumsAndGammaAreRejectedWithThePublicParameterNames()
    {
        foreach (int value in new[] { -1, 0, 2, 256, 65535, int.MinValue, int.MaxValue })
        {
            Check("primaries", () => JpegliIccProfiles.CreateRgbProfile((IccPrimaries)value, IccWhitePoint.D65, IccTransferFunction.Srgb));
            Check("primaries", () => JpegliIccProfiles.CreateGammaRgbProfile((IccPrimaries)value, IccWhitePoint.D65, 2.2));
            Check("whitePoint", () => JpegliIccProfiles.CreateRgbProfile(IccPrimaries.Srgb, (IccWhitePoint)value, IccTransferFunction.Srgb));
            Check("whitePoint", () => JpegliIccProfiles.CreateGrayscaleProfile((IccWhitePoint)value, IccTransferFunction.Srgb));
            Check("whitePoint", () => JpegliIccProfiles.CreateGammaRgbProfile(IccPrimaries.Srgb, (IccWhitePoint)value, 2.2));
            Check("whitePoint", () => JpegliIccProfiles.CreateGammaGrayscaleProfile((IccWhitePoint)value, 2.2));
            Check("transferFunction", () => JpegliIccProfiles.CreateRgbProfile(IccPrimaries.Srgb, IccWhitePoint.D65, (IccTransferFunction)value));
            Check("transferFunction", () => JpegliIccProfiles.CreateGrayscaleProfile(IccWhitePoint.D65, (IccTransferFunction)value));
        }
        foreach (int value in new[] { -1, 4, 256, int.MinValue, int.MaxValue })
        {
            Check("renderingIntent", () => JpegliIccProfiles.CreateRgbProfile(IccPrimaries.Srgb, IccWhitePoint.D65, IccTransferFunction.Srgb, (IccRenderingIntent)value));
            Check("renderingIntent", () => JpegliIccProfiles.CreateGrayscaleProfile(IccWhitePoint.D65, IccTransferFunction.Srgb, (IccRenderingIntent)value));
            Check("renderingIntent", () => JpegliIccProfiles.CreateGammaRgbProfile(IccPrimaries.Srgb, IccWhitePoint.D65, 2.2, (IccRenderingIntent)value));
            Check("renderingIntent", () => JpegliIccProfiles.CreateGammaGrayscaleProfile(IccWhitePoint.D65, 2.2, (IccRenderingIntent)value));
        }
        foreach (double gamma in new[] { double.NaN, double.NegativeInfinity, double.PositiveInfinity, -1, 0, 0.999, 8192.001 })
            foreach (IccWhitePoint white in Whites)
                foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
                {
                    Check("gamma", () => JpegliIccProfiles.CreateGammaGrayscaleProfile(white, gamma, intent));
                    foreach (IccPrimaries primaries in Primaries)
                        Check("gamma", () => JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, gamma, intent));
                }
        foreach (double gamma in new[] { 1.0, 8192.0 })
            foreach (IccWhitePoint white in Whites)
                foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
                {
                    ValidateGamma(JpegliIccProfiles.CreateGammaGrayscaleProfile(white, gamma, intent), "kTRC", gamma);
                    foreach (IccPrimaries primaries in Primaries)
                        ValidateGamma(JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, gamma, intent), "rTRC", gamma);
                }
        static void Check(string parameter, Action create) =>
            Assert.Equal(parameter, Assert.Throws<ArgumentOutOfRangeException>(create).ParamName);
    }

    [Theory]
    [InlineData(false, IccPrimaries.Srgb, IccWhitePoint.EqualEnergy, IccTransferFunction.Pq, 0, false)]
    [InlineData(false, IccPrimaries.Rec2020, IccWhitePoint.Dci, IccTransferFunction.Hlg, 1, true)]
    [InlineData(false, IccPrimaries.DisplayP3, IccWhitePoint.Dci, IccTransferFunction.Pq, 2, false)]
    [InlineData(false, IccPrimaries.Srgb, IccWhitePoint.D65, IccTransferFunction.Hlg, 1, true)]
    [InlineData(true, IccPrimaries.Srgb, IccWhitePoint.EqualEnergy, IccTransferFunction.Srgb, 0, true)]
    [InlineData(true, IccPrimaries.Srgb, IccWhitePoint.EqualEnergy, IccTransferFunction.Pq, 1, false)]
    [InlineData(true, IccPrimaries.Srgb, IccWhitePoint.Dci, IccTransferFunction.Rec709, 2, true)]
    public void NewCombinationProfilesEmbedUnchangedAndPreserveCodingAcrossPrecisionsScansAndNonseekStreams(
        bool grayscale, IccPrimaries primaries, IccWhitePoint white, IccTransferFunction transfer, int precision, bool progressive)
    {
        const int width = 19, height = 17;
        PixelComponentLayout layout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgba;
        int components = grayscale ? 1 : 4, stride = width * components + 5;
        Array source = Source(precision, width, height, stride, components), original = (Array)source.Clone();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = grayscale ? JpegColorSpace.Grayscale : precision == 1 ? JpegColorSpace.Rgb : JpegColorSpace.YCbCr,
            Distance = 1.375f, Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
            OptimizeHuffmanTables = progressive, RestartInterval = 2
        };
        byte[] coding = ReadIccAndCoding(Encode(source, width, height, stride, layout, options)).Coding;
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
        {
            byte[][] profiles = grayscale
                ? [JpegliIccProfiles.CreateGrayscaleProfile(white, transfer, intent), JpegliIccProfiles.CreateGammaGrayscaleProfile(white, 2.2, intent)]
                : [JpegliIccProfiles.CreateRgbProfile(primaries, white, transfer, intent), JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, 2.2, intent)];
            foreach (byte[] profile in profiles)
            {
                byte[] backing = Enumerable.Repeat((byte)0xa9, profile.Length + 13).ToArray();
                profile.CopyTo(backing, 5);
                byte[] originalBacking = backing.ToArray();
                JpegliEncoderOptions selected = options with { IccProfile = backing.AsMemory(5, profile.Length) };
                byte[] jpeg = Encode(source, width, height, stride, layout, selected);
                (byte[]? embedded, byte[] actualCoding) = ReadIccAndCoding(jpeg);
                Assert.Equal(profile, embedded);
                Assert.Equal(coding, actualCoding);
                JpegDocument document = JpegDocument.Read(jpeg, progressive, 2, selected.ProgressiveRefinementBits);
                Assert.Equal((width, height), (document.Width, document.Height));
                Assert.Equal(grayscale ? 1 : 3, document.Components.Count);
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                Encode(output, source, width, height, stride, layout, selected);
                Assert.Equal(jpeg, destination.ToArray());
                Assert.False(output.Closed);
                Assert.Equal(originalBacking, backing);
            }
        }
        Assert.Equal(original.Cast<object>(), source.Cast<object>());
    }

    private static void ValidateStructure(byte[] profile, Case item)
    {
        Assert.Equal(profile.Length, Int32(profile, 0));
        Assert.Equal("mntr", Signature(profile, 12));
        Assert.Equal(item.Grayscale ? "GRAY" : "RGB ", Signature(profile, 16));
        Assert.Equal("acsp", Signature(profile, 36));
        Assert.Equal((int)item.Intent, Int32(profile, 64));
        byte[] normalized = profile.ToArray();
        Array.Clear(normalized, 44, 4);
        Array.Clear(normalized, 64, 4);
        Array.Clear(normalized, 84, 16);
        Assert.Equal(profile[84..100], MD5.HashData(normalized));
        Dictionary<string, Tag> tags = Tags(profile);
        bool cicp = !item.Grayscale && item.Transfer != 6 &&
            (item.White == IccWhitePoint.D65 || item.White == IccWhitePoint.Dci && item.Primaries == IccPrimaries.DisplayP3);
        bool toneMap = cicp && item.Transfer is 3 or 5;
        Assert.Equal(toneMap ? "Lab " : "XYZ ", Signature(profile, 20));
        Assert.Equal(cicp, tags.ContainsKey("cicp"));
        Assert.Equal(toneMap, tags.ContainsKey("A2B0"));
        Assert.Equal(toneMap, tags.ContainsKey("B2A0"));
        Assert.Equal(!item.Grayscale, tags.ContainsKey("chad"));
        Assert.Equal(item.Grayscale, tags.ContainsKey("kTRC"));
        Assert.Equal(!item.Grayscale && !toneMap, tags.ContainsKey("rTRC"));
        if (cicp)
        {
            Tag tag = tags["cicp"];
            Assert.Equal("cicp", Signature(profile, tag.Offset));
            int primary = item.Primaries == IccPrimaries.DisplayP3 ? item.White == IccWhitePoint.D65 ? 12 : 11 : (int)item.Primaries;
            Assert.Equal(new byte[] { (byte)primary, (byte)Transfers[item.Transfer], 0, 1 }, profile[(tag.Offset + 8)..(tag.Offset + 12)]);
        }
        double[] white = item.Grayscale ? WhiteXyz(item.White) : [0.964203, 1, 0.824905];
        ValidateXyz(profile, tags["wtpt"], white);
        if (!item.Grayscale)
        {
            double[,] adapted = Bradford(item.White), matrix = PrimaryMatrix(item.Primaries, item.White);
            Tag chad = tags["chad"];
            Assert.Equal("sf32", Signature(profile, chad.Offset));
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                    Assert.InRange(Math.Abs(Fixed(profile, chad.Offset + 8 + (row * 3 + column) * 4) - adapted[row, column]), 0, 2.0 / 65536);
            for (int channel = 0; channel < 3; channel++)
            {
                double[] xyz = Multiply(adapted, [matrix[0, channel], matrix[1, channel], matrix[2, channel]]);
                ValidateXyz(profile, tags[new[] { "rXYZ", "gXYZ", "bXYZ" }[channel]], xyz);
            }
            if (!toneMap)
            {
                Assert.Equal(tags["rTRC"], tags["gTRC"]);
                Assert.Equal(tags["rTRC"], tags["bTRC"]);
            }
        }
        if (!toneMap)
        {
            string signature = item.Grayscale ? "kTRC" : "rTRC";
            if (item.Transfer == 6)
                ValidateGamma(profile, signature, 2.2);
            else
                ValidateTransfer(profile, tags[signature], Transfers[item.Transfer]);
        }
    }

    private static void ValidateTransfer(byte[] profile, Tag curve, IccTransferFunction transfer)
    {
        if (transfer is IccTransferFunction.Pq or IccTransferFunction.Hlg)
        {
            Assert.Equal("curv", Signature(profile, curve.Offset));
            Assert.Equal(140, curve.Length);
            Assert.Equal(64, Int32(profile, curve.Offset + 8));
            int previous = -1;
            for (int index = 0; index < 64; index++)
            {
                double encoded = index / 63.0;
                double expected = Decode(transfer, encoded);
                int value = BinaryPrimitives.ReadUInt16BigEndian(profile.AsSpan(curve.Offset + 12 + index * 2, 2));
                Assert.InRange(Math.Abs(value / 65535.0 - expected), 0, 2.0 / 65535);
                Assert.InRange(value, previous, 65535);
                previous = value;
            }
        }
        else
        {
            Assert.Equal("para", Signature(profile, curve.Offset));
            Assert.Equal(32, curve.Length);
            Assert.Equal(3, BinaryPrimitives.ReadUInt16BigEndian(profile.AsSpan(curve.Offset + 8, 2)));
            foreach (double encoded in new[] { 0.0, 0.01, 0.04, 0.08, 0.081, 0.1, 0.5, 1.0 })
            {
                double gamma = Fixed(profile, curve.Offset + 12), a = Fixed(profile, curve.Offset + 16),
                    b = Fixed(profile, curve.Offset + 20), c = Fixed(profile, curve.Offset + 24), threshold = Fixed(profile, curve.Offset + 28);
                double actual = encoded >= threshold ? Math.Pow(a * encoded + b, gamma) : c * encoded;
                Assert.InRange(Math.Abs(actual - Decode(transfer, encoded)), 0, 5e-5);
            }
        }
    }

    private static double Decode(IccTransferFunction transfer, double value) => transfer switch
    {
        IccTransferFunction.Linear => value,
        IccTransferFunction.Dci => Math.Pow(value, 2.6),
        IccTransferFunction.Srgb => value < 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4),
        IccTransferFunction.Rec709 => value < 0.081 ? value / 4.5 : Math.Pow((value + 0.099) / 1.099, 1 / 0.45),
        IccTransferFunction.Pq => Math.Pow(Math.Max(Math.Pow(value, 1 / (2523.0 / 32)) - 3424.0 / 4096, 0)
            / (2413.0 / 128 - 2392.0 / 128 * Math.Pow(value, 1 / (2523.0 / 32))), 1 / (2610.0 / 16384)),
        IccTransferFunction.Hlg => value <= 0.5 ? value * value / 3
            : (Math.Exp((value - 0.559910729529562) / 0.17883277) + 0.28466892) / 12,
        _ => throw new ArgumentOutOfRangeException(nameof(transfer))
    };

    private static void ValidateGamma(byte[] profile, string signature, double gamma)
    {
        Dictionary<string, Tag> tags = Tags(profile);
        Assert.False(tags.ContainsKey("cicp"));
        Assert.False(tags.ContainsKey("A2B0"));
        Assert.False(tags.ContainsKey("B2A0"));
        Tag curve = tags[signature];
        Assert.Equal("para", Signature(profile, curve.Offset));
        Assert.Equal(16, curve.Length);
        Assert.Equal(0, Int32(profile, curve.Offset + 8));
        Assert.InRange(Math.Abs(Fixed(profile, curve.Offset + 12) - gamma), 0, 1.0 / 65536);
    }

    private static double[] WhiteXyz(IccWhitePoint white)
    {
        (double x, double y) = white switch
        {
            IccWhitePoint.D65 => (0.3127, 0.3290), IccWhitePoint.EqualEnergy => (1.0 / 3, 1.0 / 3),
            IccWhitePoint.Dci => (0.314, 0.351), _ => throw new ArgumentOutOfRangeException(nameof(white))
        };
        return [x / y, 1, (1 - x - y) / y];
    }

    private static double[,] PrimaryMatrix(IccPrimaries primaries, IccWhitePoint white)
    {
        double[,] xy = primaries switch
        {
            IccPrimaries.Srgb => new double[,] { { 0.639998686, 0.330010138 }, { 0.300003784, 0.600003357 }, { 0.150002046, 0.059997204 } },
            IccPrimaries.DisplayP3 => new double[,] { { 0.680, 0.320 }, { 0.265, 0.690 }, { 0.150, 0.060 } },
            IccPrimaries.Rec2020 => new double[,] { { 0.708, 0.292 }, { 0.170, 0.797 }, { 0.131, 0.046 } },
            _ => throw new ArgumentOutOfRangeException(nameof(primaries))
        };
        double[,] matrix = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            matrix[0, column] = xy[column, 0] / xy[column, 1];
            matrix[1, column] = 1;
            matrix[2, column] = (1 - xy[column, 0] - xy[column, 1]) / xy[column, 1];
        }
        double[] scale = Solve(matrix, WhiteXyz(white));
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                matrix[row, column] *= scale[column];
        return matrix;
    }

    private static double[,] Bradford(IccWhitePoint white)
    {
        double[,] basis = { { 0.8951, 0.2664, -0.1614 }, { -0.7502, 1.7135, 0.0367 }, { 0.0389, -0.0685, 1.0296 } };
        double[] source = Multiply(basis, WhiteXyz(white)), destination = Multiply(basis, [0.96422, 1, 0.82521]);
        double[,] matrix = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            double[] mapped = Solve(basis, Enumerable.Range(0, 3).Select(row => basis[row, column] * destination[row] / source[row]).ToArray());
            for (int row = 0; row < 3; row++)
                matrix[row, column] = mapped[row];
        }
        return matrix;
    }

    private static double[] Multiply(double[,] matrix, double[] vector) => Enumerable.Range(0, 3)
        .Select(row => Enumerable.Range(0, 3).Sum(column => matrix[row, column] * vector[column])).ToArray();

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
            int pivot = Enumerable.Range(column, 3 - column).MaxBy(row => Math.Abs(augmented[row, column]));
            for (int index = column; index < 4; index++)
                (augmented[column, index], augmented[pivot, index]) = (augmented[pivot, index], augmented[column, index]);
            double divisor = augmented[column, column];
            for (int index = column; index < 4; index++)
                augmented[column, index] /= divisor;
            for (int row = 0; row < 3; row++)
                if (row != column)
                {
                    double scale = augmented[row, column];
                    for (int index = column; index < 4; index++)
                        augmented[row, index] -= scale * augmented[column, index];
                }
        }
        return [augmented[0, 3], augmented[1, 3], augmented[2, 3]];
    }

    private static void ValidateXyz(byte[] profile, Tag tag, double[] expected)
    {
        Assert.Equal("XYZ ", Signature(profile, tag.Offset));
        Assert.Equal(20, tag.Length);
        for (int component = 0; component < 3; component++)
            Assert.InRange(Math.Abs(Fixed(profile, tag.Offset + 8 + component * 4) - expected[component]), 0, 2.0 / 65536);
    }

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        var tags = new Dictionary<string, Tag>();
        for (int index = 0; index < count; index++)
        {
            int entry = 132 + index * 12;
            var tag = new Tag(Int32(profile, entry + 4), Int32(profile, entry + 8));
            Assert.Equal(0, tag.Offset % 4);
            Assert.InRange(tag.Offset, end, profile.Length - 8);
            Assert.InRange(tag.Length, 8, profile.Length - tag.Offset);
            Assert.Equal(0, Int32(profile, tag.Offset + 4));
            Assert.True(tags.TryAdd(Signature(profile, entry), tag));
        }
        Tag[] ordered = tags.Values.Distinct().OrderBy(tag => tag.Offset).ToArray();
        Assert.Equal(end, ordered[0].Offset);
        for (int index = 1; index < ordered.Length; index++)
        {
            int previousEnd = ordered[index - 1].Offset + ordered[index - 1].Length;
            Assert.Equal((previousEnd + 3) & ~3, ordered[index].Offset);
            Assert.All(profile[previousEnd..ordered[index].Offset], value => Assert.Equal(0, value));
        }
        Assert.Equal(profile.Length, ordered[^1].Offset + ordered[^1].Length);
        return tags;
    }

    private static string Description(byte[] profile)
    {
        Tag tag = Tags(profile)["desc"];
        Assert.Equal("mluc", Signature(profile, tag.Offset));
        Assert.Equal(1, Int32(profile, tag.Offset + 8));
        Assert.Equal(12, Int32(profile, tag.Offset + 12));
        Assert.Equal("enUS", Signature(profile, tag.Offset + 16));
        int length = Int32(profile, tag.Offset + 20), offset = Int32(profile, tag.Offset + 24);
        Assert.Equal(28, offset);
        Assert.Equal(0, length % 2);
        return Encoding.BigEndianUnicode.GetString(profile, tag.Offset + offset, length);
    }

    private static Case Parse(string row)
    {
        string[] parts = row.Split(' ');
        int[] key = parts[0].Split('-').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        return new(key[0] == 1, Whites[key[1]], Primaries[key[2]], key[3], (IccRenderingIntent)key[4],
            int.Parse(parts[1], CultureInfo.InvariantCulture), parts[2], parts[3], parts[4]);
    }

    private static byte[] Create(Case item) => item.Grayscale
        ? item.Transfer == 6 ? JpegliIccProfiles.CreateGammaGrayscaleProfile(item.White, 2.2, item.Intent)
            : JpegliIccProfiles.CreateGrayscaleProfile(item.White, Transfers[item.Transfer], item.Intent)
        : item.Transfer == 6 ? JpegliIccProfiles.CreateGammaRgbProfile(item.Primaries, item.White, 2.2, item.Intent)
            : JpegliIccProfiles.CreateRgbProfile(item.Primaries, item.White, Transfers[item.Transfer], item.Intent);

    private static (byte[]? Icc, byte[] Coding) ReadIccAndCoding(byte[] jpeg)
    {
        using var coding = new MemoryStream();
        coding.Write(jpeg.AsSpan(0, 2));
        byte[]? profile = null;
        int offset = 2;
        while (jpeg[offset + 1] != 0xda)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int length = BinaryPrimitives.ReadUInt16BigEndian(jpeg.AsSpan(offset + 2, 2)), end = offset + 2 + length;
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

    private static Array Source(int kind, int width, int height, int stride, int components)
    {
        byte[] bytes = Enumerable.Repeat((byte)0xad, (height - 1) * stride + width * components).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < components; channel++)
                    bytes[y * stride + x * components + channel] = (byte)((x * 73 + y * 29 + channel * 61) % 256);
        return kind switch { 0 => bytes, 1 => bytes.Select(value => (ushort)(value * 257)).ToArray(),
            _ => bytes.Select(value => value / 255f).ToArray() };
    }

    private static byte[] Encode(Array source, int width, int height, int stride, PixelComponentLayout layout, JpegliEncoderOptions options)
        => source switch
        {
            byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)(int)layout, options, stride, TestContext.Current.CancellationToken),
            ushort[] words => JpegliEncoder.Encode(words, width, height, layout, options, stride, TestContext.Current.CancellationToken),
            _ => JpegliEncoder.Encode((float[])source, width, height, layout, options, stride, TestContext.Current.CancellationToken)
        };

    private static void Encode(Stream output, Array source, int width, int height, int stride, PixelComponentLayout layout, JpegliEncoderOptions options)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout, options, stride, TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, layout, options, stride, TestContext.Current.CancellationToken);
    }

    private static int Int32(byte[] profile, int offset) => BinaryPrimitives.ReadInt32BigEndian(profile.AsSpan(offset, 4));
    private static double Fixed(byte[] profile, int offset) => Int32(profile, offset) / 65536.0;
    private static string Signature(byte[] profile, int offset) => Encoding.ASCII.GetString(profile, offset, 4);
    private sealed record Tag(int Offset, int Length);
    private sealed record Case(bool Grayscale, IccWhitePoint White, IccPrimaries Primaries, int Transfer,
        IccRenderingIntent Intent, int Size, string Description, string Sha256, string Id);

    // Complete fixtures generated from unchanged google/jpegli at
    // 031a0077f5799a6041004267fc12b956c1f52a20, lib/cms/jpegli_cms_internal.h.
    // MSVC ARM64 C++17 /O2 /fp:strict; default 3D tone mapping; gamma exponent 2.2.
    // Keys are grayscale-white-primary-transfer-intent, indexing the arrays
    // above, with transfer 6 denoting gamma. All 288 named combinations and
    // all 48 gamma combinations are covered; no managed output produced these.
    private const string NativeFixtures = """
        0-0-0-0-0 536 RGB_D65_SRG_Per_709 045567821C2B986EA4218C62D67068C26415CB5C7655FE7AE0F6C9CE0F5F6DAF CFBA11E85E5933B6BA8D908E88DFFB58
        0-0-0-0-1 536 RGB_D65_SRG_Rel_709 F708D24C35DED631D1A400FA9EE2ED2641661791162E3D4568380719A53162AF B067A93E125ACB3772B3F56933649780
        0-0-0-0-2 536 RGB_D65_SRG_Sat_709 250DBE984C5E2E4B6901104164D55F5FCED3C3CB6A4F836EA0313EFFE4E0714C A0D549605F5C2FF746E9F6BCED1F05A4
        0-0-0-0-3 536 RGB_D65_SRG_Abs_709 8332E8D70665A59F604993A9A92EE5CE96A939EC4F39295FBCCED5BD3DBDC869 3900160773DB40220E6A414263CBB29F
        0-0-0-1-0 536 RGB_D65_SRG_Per_Lin 138747C4D990CAFE065BB407D55219D3B7FB7F22CF803FCD07D85915514028D7 AC25B029F881260EB2154A2D8B1CAC5B
        0-0-0-1-1 536 RGB_D65_SRG_Rel_Lin 68155F2BD3F362CA3A62BDD1620BF3A7BFCA7C0554062BB2A5FB5DFDDF0D73F4 8EA3E5F0307CF8C739BCC35EF6A24336
        0-0-0-1-2 536 RGB_D65_SRG_Sat_Lin D303497AD2C178EF88769C405681C4DA10FCBF5867FE02FE6EC5742F0148F5F7 72F3DAD14D99F38EDCE0D1F53A428B69
        0-0-0-1-3 536 RGB_D65_SRG_Abs_Lin 399EA7DAFFB9F0200F75A3FED4187DCF3C6F8B40027D5F7BBF0320F9155AFB63 F54F7F6D457B7141276728FB23A59FBA
        0-0-0-2-0 504 sRGB 1406EFC5429250FEBE778C182EB5E5168AA14B2CAEFCF772211A2EC2ECF31659 A3379298068DF11976289D87A33230AA
        0-0-0-2-1 536 RGB_D65_SRG_Rel_SRG D7F9E32AC2A06A7E3865A828E865DAFB7CF1947584D3C06EC97A660C59786B51 425B410C0A8285639EB234EF7C6DA647
        0-0-0-2-2 536 RGB_D65_SRG_Sat_SRG CA2731A3499AD3130D0F4EB29BC7C7AC12223CF20C5105D20A6C957865E4E6B4 FA50180B9BAF603FB4E616219CC52198
        0-0-0-2-3 536 RGB_D65_SRG_Abs_SRG 6848B5773C2E9F90D8AA2A62F517F79C9E7953E106EDB01C9B0B58B5257A6389 0C50DEFBC368D066B1C8B421BA571BF0
        0-0-0-3-0 4344 RGB_D65_SRG_Per_PeQ CC7A23F92D369FC884EF7C836D041E10B10E8104ED393DE1488C67D66E7CAC10 F463500B39004F89623F245F4C47D1F6
        0-0-0-3-1 4344 RGB_D65_SRG_Rel_PeQ C89A75AA6599FDFDA76B55215563B8D3189CC2DADABB1F6F8FC4450E8CA877F3 55188A189E786B42E1756FA9A2D23E53
        0-0-0-3-2 4344 RGB_D65_SRG_Sat_PeQ D4CDB7365B98DABF93E39F7741202D15815F7B1B51E78E3A96DB17BCC4AAFC64 CE7756B0F2FC36BC2527362F77267D43
        0-0-0-3-3 4344 RGB_D65_SRG_Abs_PeQ 037CB54810E597F0D662918C7F1BECBFA6669743EC174B0F0525F37A6210EDBE 98F0954C90970DFC9FF809FF233E3930
        0-0-0-4-0 536 RGB_D65_SRG_Per_DCI 6577027F4C8509916D6830BBB2EC6BCDBC921DBF9D0C9EA1CD0EF8CF5B2BD9DC 1C4BE12C4F56FD412A310DCF935B3C19
        0-0-0-4-1 536 RGB_D65_SRG_Rel_DCI AB895064FB595FF5FAB79578B0AEBDEE992B43733C83745930B4785BC19F737E 0AB269D5B7CC83B67D2DC64324053E9E
        0-0-0-4-2 536 RGB_D65_SRG_Sat_DCI 184AE1A2D0A0F7ABAFFFD01DD1252BDB3EB3D814A075B4F21040F1CCB2A4D0A2 E35430C5840F17C753CB93EF2737902E
        0-0-0-4-3 536 RGB_D65_SRG_Abs_DCI E71C4FEACB504ED1AD2AA5F5F2D7E81909100505418A08B777ABA34AF2A01157 C7A70CB23B32B0D7B5ECEA589CCBAEC1
        0-0-0-5-0 4344 RGB_D65_SRG_Per_HLG D7CA64399F0F18F5D05ED6EB406E7C589A6EACEB60773E82D934195E05066B5B CA2AA0949AE331AF9AEA99CF5869CF1B
        0-0-0-5-1 4344 RGB_D65_SRG_Rel_HLG 51C6851F488FC2BAF76F669729EB072028882C8C5B3A86EB960DFDDC44C5577B 242852ABFD035E936981926285220D03
        0-0-0-5-2 4344 RGB_D65_SRG_Sat_HLG 009C220A9CD0B0E5724E2F1B2F6CC5E1A4A54FE98D87C6C607B7023A17E63AC1 4BBE9F1BEB15D8C76BA9524EBD4F2FF4
        0-0-0-5-3 4344 RGB_D65_SRG_Abs_HLG 552A3425ABB6FDEBE5B6276BDE470F0DA3859133D8FBD7300E02D42CA0CC7211 A03EFF22ABB32D15CD76D8BE7557C705
        0-0-0-6-0 508 RGB_D65_SRG_Per_g0.454545 AC2E845DC09136B1E0BA9BDA284CB6A0A88977BA939666D2DDA1A0A965632325 2C27024414F660F61D61B369ED9E7E1E
        0-0-0-6-1 508 RGB_D65_SRG_Rel_g0.454545 8FD66285480C22C54EFAC8EC97544B8DA95ECA4A6E5FB6BA7657444632B1768F C49B0B327FDF7103AF69E3EB182A2ABA
        0-0-0-6-2 508 RGB_D65_SRG_Sat_g0.454545 5764625ABCBC884622373F2A657ED32D7141C13D9852BE2E43D46C70244DFFFF C8A8E6F7D123A6A2CC4EE9C94D9D794F
        0-0-0-6-3 508 RGB_D65_SRG_Abs_g0.454545 455457052091EAC8454C082C37954BE937559069008CD9086D8DC312DD432A49 644263D28A56D9717A4C092CF6F1F3D2
        0-0-1-0-0 536 RGB_D65_DCI_Per_709 277501682AE976513DA5E633BC98612B99D4B467E00070E29E5DE94F4A2ACD6E D6EE73D61B2F87577400D00BD925EA65
        0-0-1-0-1 536 RGB_D65_DCI_Rel_709 6520C17F58DC78186CA13C84F6595108A60CEB776C4ADD80BD0A08B77314B822 F07653C8ED5704F0E01565D4D4F07309
        0-0-1-0-2 536 RGB_D65_DCI_Sat_709 F880B561F9F0035CD25AB6EAA5573B24B1DECEA2B20ED01A67DED8DE520947CF 66AE784284DE248E4396E50D60A33FA0
        0-0-1-0-3 536 RGB_D65_DCI_Abs_709 3F6406315893DE67E03122762A467B19ECC704617F5C59247AA8D346A654BF4F 5A19123031DC26654DC0552F86E8FEB8
        0-0-1-1-0 536 RGB_D65_DCI_Per_Lin 49201BF4652A43DFE19017C07E2FAF75EA2D43BD710F90195F6164E5E0C69A15 C52340FED739BD5472CF28CAA75A250C
        0-0-1-1-1 536 RGB_D65_DCI_Rel_Lin 54FB70EE405AEC537F21E8C4D92AF975AEA399A3F98E59EB6AA74554CE53A35A 55C217930B65C3C814472A91FA2503D1
        0-0-1-1-2 536 RGB_D65_DCI_Sat_Lin 936AEFB5CBE71B0EC4BD0BA9CACB373B680C93FF0D67897C20AA9C4BD7EA57C0 AA26E5FDA07B859E65F3B527B7DF3382
        0-0-1-1-3 536 RGB_D65_DCI_Abs_Lin 2E8BCA8E9D6BD2C8A63603A51244AF7BBEFAEE5C717FAB63EE89D39EB0EE3E77 413F51234FF477BA33AD398401F66B98
        0-0-1-2-0 516 DisplayP3 A35258ED146F3F0A6A24C25B102AAF2A02C9721CBB647422FA5A3F1CFD132C96 DF9F5B76A9627A080C7EDDA1399115DC
        0-0-1-2-1 536 RGB_D65_DCI_Rel_SRG 128399778F07C073EA45C68C7962128DE6583DC47A4500AE8F5B636D3EDECD6D FDDDAD1AE16B2FD710915B7A1AAF3630
        0-0-1-2-2 536 RGB_D65_DCI_Sat_SRG 5C799686D43633C124F3FE9CE7ECC57ECF910B4CF008DC9F64473D5647D78201 B11A0F745A979D824814E2689CE50489
        0-0-1-2-3 536 RGB_D65_DCI_Abs_SRG A44781DF1AE1EEAAEB4F70280C653D81DDA7638157650C16B38357EB6C6E3CD7 767931510EB3D26883961C91EB931E8B
        0-0-1-3-0 4344 RGB_D65_DCI_Per_PeQ 812982F5923541EAC1A5CFC558A0F8E47333652F35866BCF9369BB31683AD554 AA027A64EABE351403C2B3FEBD6494CE
        0-0-1-3-1 4344 RGB_D65_DCI_Rel_PeQ E83EFA275580BFB77C28E2A2B976A4C795395EAD13E9CC16F6DCAD871A6C415D 14824A017BB6B395B86236CEB7A4EF66
        0-0-1-3-2 4344 RGB_D65_DCI_Sat_PeQ BE547EC6F23178E0822B625F21C3CDA6A787F45631E96793D4B3080BBBEA2C77 E8A73A6D863BB21C77884A3C78D9ABC9
        0-0-1-3-3 4344 RGB_D65_DCI_Abs_PeQ 7903B992825DBDAB3BD2ACD2436EFAE298438FD9B3D0CA14666326979573DC19 E5B17B501AA3E07D546948251C38643B
        0-0-1-4-0 536 RGB_D65_DCI_Per_DCI CDBFF4E78C6F5401B7970BC6DB691A7F75EC2313F78ACD68AF288526F382112D 7E554D5645CF6CE199198B9A682319C1
        0-0-1-4-1 536 RGB_D65_DCI_Rel_DCI 90654A2755A03F10102F172FA0805BE48A30B82115207CBBDD4A17DB78A2AED8 DE9F9B2ADC9C15BB60FFC38CDF460351
        0-0-1-4-2 536 RGB_D65_DCI_Sat_DCI 8B2786BF059CEFEFA757725684C8B099AF066F67FBD6E089AEB771DAD9BD92C1 4253496C7C11A342DD027C00E1E2DAF5
        0-0-1-4-3 536 RGB_D65_DCI_Abs_DCI A3AD34526BD20D8CE60BDF3B309B4298309C8106578F4EBE0B1BBC4361B55D93 B1621D7B8075F0307458730BD8A6E86C
        0-0-1-5-0 4344 RGB_D65_DCI_Per_HLG 722D2E49CA95FD9DFB403D497BF6489B84D04EE8C8B1F7A6553890CBE65AC7CA CEA4C0B322A5C9D552B53B08E96D3015
        0-0-1-5-1 4344 RGB_D65_DCI_Rel_HLG 9BDDDC779023BBC7B265A9EB633AD74E4FBF0E5A4FDEA6C93B99720728664194 DD75657D2AEEC3FEFAA1A09854353C1A
        0-0-1-5-2 4344 RGB_D65_DCI_Sat_HLG 0706C68EF026EC3F3C06CE10C83592DAE3137F235676CF41541A22FC1E3F47E8 88F94DFF090C8AB15A5FB1DFCF585288
        0-0-1-5-3 4344 RGB_D65_DCI_Abs_HLG 2585E0F2E08EBB9718E5D890BA10B83D38442E2630B5841B3C1BD31CDA7FB9F1 06A1722F3289D5261A8CB31BE1C4F57C
        0-0-1-6-0 508 RGB_D65_DCI_Per_g0.454545 91229100CD4881E0B532F59C4903366EF0319B22A2186BC5F3A0A6FFBB420ABE 1C788F9A5E5A76A22F2975B38FDC9D7E
        0-0-1-6-1 508 RGB_D65_DCI_Rel_g0.454545 E56380004FEC813ED0BD3CDBECA9B33F120B33DA2A4D1A14899B7F5BE8BECBC5 50948FA52122172E8DDECB48E2179372
        0-0-1-6-2 508 RGB_D65_DCI_Sat_g0.454545 28B4EA5A5D831C08FD07F61CFD58160F4A2B089FFA3C1F19D1613D2ECC745A60 C832EB220A90CBB90DB792C89E7754EB
        0-0-1-6-3 508 RGB_D65_DCI_Abs_g0.454545 8ECEFE6041B07C2455DED172A2F76FAC054BEA81E97B5B4C1C846808C944D1F8 913FD225ED3C7ABCB414601A8E471311
        0-0-2-0-0 536 RGB_D65_202_Per_709 B69A102609B438F3D4CE0EEE90C85E77DEAA289257599EB81FB6CCA96EF43DA7 5E074A7982BF282A685AF94289F51A07
        0-0-2-0-1 536 RGB_D65_202_Rel_709 877C5B36808954F322AF187B56C3B3EBFD1CC8BAD569F27F67F1A0C450D11B9C 06E12BFEE4A6D89B65B6C030517DB75D
        0-0-2-0-2 536 RGB_D65_202_Sat_709 031BF590C83D7B9E170A0480630127569719F6E3FB54529F362AC17DF7A4297F B98117A4097B3FE22CDB523AA06B7398
        0-0-2-0-3 536 RGB_D65_202_Abs_709 5B7B8F622C2128E807CFF5F4498F9B4102BCD113F80CBFB96B65711DD51C523F A99D14C6B26335A1995C72CEE242DA26
        0-0-2-1-0 536 RGB_D65_202_Per_Lin E403CC09CAF20E276AC86B96B1D0A009B7B6ACFBC654775F311D2BCEDCB984D9 899A91EB98AABBA5AF5036683720388C
        0-0-2-1-1 536 RGB_D65_202_Rel_Lin 8210903E196C88BFC368C602D1B413D7CAF5339AA4FF58E25BEB7A4F50E37CA8 BB7E83AECA02D7DEFAC82890C44622DC
        0-0-2-1-2 536 RGB_D65_202_Sat_Lin 24B913DC471C0BDB23EE12B88488C51BBA81DCEF8402583336EB01C72B821A20 677278A0DC24DA0B94C9CE7C5A63B4CB
        0-0-2-1-3 536 RGB_D65_202_Abs_Lin D3C89C804692B16D420AF1689349C974C941EAA8D7F0B9E861E1E757DF7F90DA 0763CA1BBDB7532F407034FF742BBDE4
        0-0-2-2-0 536 RGB_D65_202_Per_SRG DD8321748965BBCFBF7868C7FA478079C091EB38D7ABD3696319992434E21F23 F2E724B7691C1EBB156A779F3F845AFF
        0-0-2-2-1 536 RGB_D65_202_Rel_SRG 7B905EF6C193DA187D1D7AE805336BCCC43B2893632C9BDDC36F5CAD39FFAFB8 33CE3AB3FACE7865C059ADED09B6BAA8
        0-0-2-2-2 536 RGB_D65_202_Sat_SRG 25383923BA08D36533C2B8D9803A549725CC762A583D1E4B3075BB181F6FA80A 4FD233911E13E05B6689F04CE7489825
        0-0-2-2-3 536 RGB_D65_202_Abs_SRG F51CDCA109CAE950BC1EEE2654157A2500143D0171AE1F105A3C2194FC53B1AF BCA46A1576751CC30E77863800E71A2C
        0-0-2-3-0 4344 RGB_D65_202_Per_PeQ B2F97E195A8C64DD55EFF71FC1DF480A12E59D71105D7D9DABFCC23F9252C48E 22BC30F3E3B03923D34FE5CF368CCFA6
        0-0-2-3-1 4324 Rec2100PQ 745265D126B3BBFDB6A0E481936D5E51752D4BFED11387B83536BCBA9CE1884A 5AD24C1614A4EABC83D43904A8600E8F
        0-0-2-3-2 4344 RGB_D65_202_Sat_PeQ 19258CF5A23226CCDF4A2FC7570D140109A2474237FA8699427A0F6DB77BDB18 EACCD1C5137E163C5E87B73322E3BE52
        0-0-2-3-3 4344 RGB_D65_202_Abs_PeQ 63E020914913198E14CB5F551B8E0C7F55C05AEE6CB14F77C9E98F343E43A0DD BC638EC8A4DB1CFD16B099BEB167CE35
        0-0-2-4-0 536 RGB_D65_202_Per_DCI 131614497AA965139977DB10B234A52CAB9D8BE6D94C30A7D432455092F13889 83D28BDFFE5DE798C22A8C06EC74DFA0
        0-0-2-4-1 536 RGB_D65_202_Rel_DCI 525323150D572C8432F7640CCD44570C5068327C5BDC3A9C0D33D97D3EB2EDA2 1BC335A68B22897285C0708D0B8D6E91
        0-0-2-4-2 536 RGB_D65_202_Sat_DCI 5C7025C6729759E57C3915E387B297A72BF636661C11BCBEDC8076C56FFF33E5 2F20768EE6CFB94E2A29B7E05C7AD5A1
        0-0-2-4-3 536 RGB_D65_202_Abs_DCI BD6F89FF3E4CD7F3E67188F87E28D170F04DC81012C7FF17472E3D726B06ED27 BAFC09208E54F32FEB334CA586DC1AD9
        0-0-2-5-0 4344 RGB_D65_202_Per_HLG CB3A3012517E21922D895462F66841F6EDEDDE8031CC3CD3041A12AF8536A556 D2BA1D3F609E15C6612C1CDE2CC98D15
        0-0-2-5-1 4324 Rec2100HLG 8C50CF64E267FD7DE589200757166A63CB8445E857FE7796DA727D41BD8DCD41 B252BC24B541E49061B981815D91AA01
        0-0-2-5-2 4344 RGB_D65_202_Sat_HLG 5DD75F6056508FC31AD97840FE140E102F5DC7C0018003FA71D05437A4E18370 749767A7DFB4104198C5E21AF8D20BDB
        0-0-2-5-3 4344 RGB_D65_202_Abs_HLG 30C52909B67BFE7994FE022F54550B5BDE5DA3D73ABE5AFDA65479459C7CB5D3 2E1554B0CB15F545C500C4E8F58CB7FB
        0-0-2-6-0 508 RGB_D65_202_Per_g0.454545 C8CBFD51C18758493286F90838EAA5EBE376C81F0C4AC6143767F91881636CC2 15AE98FBE0D14BACEF0F4C6CF3840102
        0-0-2-6-1 508 RGB_D65_202_Rel_g0.454545 988C2BC0C86BEBCFB60927CC0EB62265BC7661E6A1CCD909A01F1971971B3C63 D784C40258FAA533352CBA935C9C423E
        0-0-2-6-2 508 RGB_D65_202_Sat_g0.454545 D96630C0AFA23226D72C4A7BF16529B18C5BB63FE33B1BA660604776308DAB6C B616A03F909EEDF0DD68A88E4D438532
        0-0-2-6-3 508 RGB_D65_202_Abs_g0.454545 6793761ED0324987893C923B1DC7D7B6B3A5CC3AE417172B8B3B301EBFE6494D 470F61AC297F00499E551A422849CACB
        0-1-0-0-0 512 RGB_EER_SRG_Per_709 DFB187F6BBEE0802AAA7A130239E6D1CA8D6B76D320CD033B2C51339A9181B03 2EB9479B762B82C4142D785FC5586AF5
        0-1-0-0-1 512 RGB_EER_SRG_Rel_709 47654492AA63C06AD2B9DB81B03A3ED86915589D10B0A1F773881A33A2E398ED 53A800C4F76340F0B89EBCD7A1E2B9FB
        0-1-0-0-2 512 RGB_EER_SRG_Sat_709 B586AA3BF0B3FB178E1490FF155A5309E5591BD2CCD09E7C9B23D9BCB95DBEA2 C1F2A7F4014AE832F91059AC639C364B
        0-1-0-0-3 512 RGB_EER_SRG_Abs_709 0C1D22390611998AF1ECDC1100C7AC7982AECF739888A8AC9B1DAA196B1D2800 391D27031F3E263A6DFC9BF629E6DA35
        0-1-0-1-0 512 RGB_EER_SRG_Per_Lin C75BE959BD21C05386EEB360CF9F94E31AFAF1340C93DC64800995402754E93F C6DB33638ECECC14FB45835643B92BAF
        0-1-0-1-1 512 RGB_EER_SRG_Rel_Lin C216E3DFD8349ED911D7CA185072262444BA8C0F97BCA50A27329FCB7239B482 08DF1D89B6EF4BB79DFBC54DDF6F9EED
        0-1-0-1-2 512 RGB_EER_SRG_Sat_Lin A208C9B96A21CA7AAAC24BEC16DB73098705E22CA61E0B0D4B396D7DFB4656E8 36193DE19A2D666207BE630CFFB21FE7
        0-1-0-1-3 512 RGB_EER_SRG_Abs_Lin BAE9DDD3E18F1FF5F9D39A8BB5D17FF0C2A0EAD272B45F1101F3BAE4DD59E122 F1CA6D6BF0C851219F2DF4D8CD576C76
        0-1-0-2-0 512 RGB_EER_SRG_Per_SRG 4B6BE9C62EE670FD0E2731E8866172144C21C296DD9110EB38BABE94960A91F5 101B2B7D48A3E9145A67856599A6A4B7
        0-1-0-2-1 512 RGB_EER_SRG_Rel_SRG 2E04BB1013287F003FD3BC57E6114BE0B909AA3266841D56DB730393195693F6 920D4D91542A5A06A5FFF87618D2E26B
        0-1-0-2-2 512 RGB_EER_SRG_Sat_SRG F98B944AF98756E6422398FB771F663C85454448DF974A831C3723BE9A80DBA7 EA547AE490A6E852242D1AA2A258A8D8
        0-1-0-2-3 512 RGB_EER_SRG_Abs_SRG 202A2434B207561EEFDA17C68A27C7B29780AFFCD1D218AE3533C5184A53DA84 D87D7DDA84B2F145D9A7EACDC94CC7B3
        0-1-0-3-0 620 RGB_EER_SRG_Per_PeQ 7BA18FD1F827E1F40655A3D1127E0124BE634C014CBC17B29FD2373C2A08A5F8 95129A75AF27411959C6A1A174EF2761
        0-1-0-3-1 620 RGB_EER_SRG_Rel_PeQ 7682DFE941CE0E9A2162C7E1A45EAAF48C56EB6E38638C712A81AEC204C71B5A B703FFD24CE3FCF3059EE3A235986156
        0-1-0-3-2 620 RGB_EER_SRG_Sat_PeQ 1C8BE51C2C3E79BA54236ED3C90FA8E996BB1B99657016B2AFE5AC8EDC3B5A17 FEEEAB4EA1DBD366BBD13DCB10C16BCA
        0-1-0-3-3 620 RGB_EER_SRG_Abs_PeQ CAD73501B836DB5031195043B379ADFB33562957C4A7D6D6B306DFE802FD7B11 7FEF290FB74164D4D3F52F992CCCEBCA
        0-1-0-4-0 512 RGB_EER_SRG_Per_DCI 9D5D3880181F58A68A121D0936F4DD0B20BF584D3D6C5F8BB742336D7E89D641 C427C900118285609B32538CF57232E0
        0-1-0-4-1 512 RGB_EER_SRG_Rel_DCI 4380A37B9F578D38B990AF8F463CDF1A5D50445BB4FB238084DA09C745E9281B 3B34D57D73A008CB596458C3619E02E0
        0-1-0-4-2 512 RGB_EER_SRG_Sat_DCI 3DEC05B83E021A81ADE52399A8A957C25441DDD741AC35844961F2C3191B6A8B 91F05C9509D28B1CDD360B6EF7916533
        0-1-0-4-3 512 RGB_EER_SRG_Abs_DCI DF12AE05FD4B41F535B4A30A6850929BEF0BA79067801240CAA20E939ACD5496 9400F65EB41EEB61A07F1AB074E77FE3
        0-1-0-5-0 620 RGB_EER_SRG_Per_HLG C76F0C26EC4DA9A52DB319668799917DA8F16F82C46DF1DD3F8C7BB1BE4C4D97 23CF622A1536F53F44139D166ABBEAE9
        0-1-0-5-1 620 RGB_EER_SRG_Rel_HLG 1ACADE1E2DAD072E86637F88D16AFE6CC80D4DB73C98425D6282633A58625E8E 2AEFE14C2E5871086B896F78503D628E
        0-1-0-5-2 620 RGB_EER_SRG_Sat_HLG D3C55747125F8DD23AE82149ADD081668A4215E522ADC665C23905BBA70C9D22 D6D160DDD6DDC25B338E483262BCEBEC
        0-1-0-5-3 620 RGB_EER_SRG_Abs_HLG FBCDA7C8D09B032B11330BAA63D0B53F113BDD59D1A1E348919AA5E0304C73ED 4ADF62FC7AE849732E3D510D55C4AA9A
        0-1-0-6-0 508 RGB_EER_SRG_Per_g0.454545 2EAF062D17E402A319698FBF65C2FF9803A03B2BF4FCD08BCC12055A29A85381 A249D75F4AD9CF0E898B5F2BC3F19A86
        0-1-0-6-1 508 RGB_EER_SRG_Rel_g0.454545 3FE7B3B4BF6A7B73316588634F161934C4FEF2AC449262ED1D757AB055C51121 7D0697983CEA2ADA64D746B5B9140DA8
        0-1-0-6-2 508 RGB_EER_SRG_Sat_g0.454545 F283361A8400F4EC8C4A79F8B6B3E87B0C56CBFC5522C68B8DBEBB347C3F3F2D B06E31192D073356F92C153D7AA463A6
        0-1-0-6-3 508 RGB_EER_SRG_Abs_g0.454545 20F7DB6479C4FC0289044D4FD462347F43D94210E45AF8D4EFF4EBEC61521BBA FD94E169E1A19000D05D6020A546FF4B
        0-1-1-0-0 512 RGB_EER_DCI_Per_709 F14E23A5305C4D2DA84D3B334E8599A729E035EA503458DD0F134D5AE98FA747 AFCB514D3A462F495DDCD1FD9ED67C4D
        0-1-1-0-1 512 RGB_EER_DCI_Rel_709 6D6FD50BFADF8DDAA0B56B6F3A5AE038EE70567D6DB1DC99B91E6D66905D1B20 DF9F863A44B5322EC67780C8C2D0FAB8
        0-1-1-0-2 512 RGB_EER_DCI_Sat_709 80C1F58A823B0F05DE8E9631178FEF8E049D3FA739E0D212D0976C6A743015E0 C68A1D3202979A55AD270C84CEBC21E4
        0-1-1-0-3 512 RGB_EER_DCI_Abs_709 C84C7B3551F7EE3DE629DCA76AC146AB2278645F0D082724308A6707CD9A2300 21B6DC3AD169C6B08A9C2D31677174EA
        0-1-1-1-0 512 RGB_EER_DCI_Per_Lin 39088CF61147EFEAEE6B8F24B7CCDE40B13E3BF07E8FBB2EA163175DC50F7318 4409847698D6FA418E4F9CCFA836D826
        0-1-1-1-1 512 RGB_EER_DCI_Rel_Lin FFB9A00756AE532BC35E9ABF0E88B3A22EADABE31C4990B473B969F7EAD16F75 98B9858AB02E8A9D2852EE3A8114E710
        0-1-1-1-2 512 RGB_EER_DCI_Sat_Lin 2064994F827E96E3F9FA1A920FB8808A8C02AFF87FB668294A0656A070501F7A 75882970FF21073C5C9BD17D1A306D3E
        0-1-1-1-3 512 RGB_EER_DCI_Abs_Lin FF251C9CB068C620287D8700FFA00B09E602B0AB6DD0679023487047F6D97CF7 A31FBE0146537B331F62FA9BE9FACD63
        0-1-1-2-0 512 RGB_EER_DCI_Per_SRG 49F0EBEF749D3787B42BB88C57B36D1B8DE5883D8B40A3EEF0439D993364B2AA 3E8A967D2AB9D96D61A5EE7541C3E7C1
        0-1-1-2-1 512 RGB_EER_DCI_Rel_SRG 4D9E4C2838F202DADCB6CC620A2512627E8C3C9B184D69FC7898B3B98F0BD230 7B9E3B771D1B3660435DF6B78FC9606E
        0-1-1-2-2 512 RGB_EER_DCI_Sat_SRG 787104A285429704A6CFA2AD8762EFCF54F48B4DAF86578CBB0791A2FCA1A400 0D5F7CA6FA9ED60E040256A5F1E544BA
        0-1-1-2-3 512 RGB_EER_DCI_Abs_SRG 25EA10D2A9D99C31907970DC8DEF10CA413CC91780287FF7A742167D0F7BD6AE DAA627895F7F93BD88D79A378AFBAA08
        0-1-1-3-0 620 RGB_EER_DCI_Per_PeQ A95BF9F39956DD4A33C070F59AA5C3D59769EB5949A24944816834C7B1B2E032 375851D5119A46E4F7398EF1C506D0FD
        0-1-1-3-1 620 RGB_EER_DCI_Rel_PeQ B08FD1988D434542B5B9982CA8169BCBE31B086BDD05383362B62CFFF81E130A B7944A4FC5DDDEE6829C52FE032FE922
        0-1-1-3-2 620 RGB_EER_DCI_Sat_PeQ F7FD083B79DF2CB56E58999B4231917ADEEDE603D022DC8A853599FCA0D88703 A659658DCDC071E5CAA43B2E3F52841F
        0-1-1-3-3 620 RGB_EER_DCI_Abs_PeQ 642D8A48B75D10D15664310B87FA14B3F1410FAAB44AA1A6BB20C3AC4B7C6CEA 94D3A93D68328227F62EFE1467D7ECCD
        0-1-1-4-0 512 RGB_EER_DCI_Per_DCI 84CA20EE564A502BA4D50705F2846C7F2D51517CA5F8A9A1BC8F4751947B3F15 505697BCAADB56EADDD9336154E25188
        0-1-1-4-1 512 RGB_EER_DCI_Rel_DCI 61222997595B401E4F19B492A9707A43CEB2A20AE27A7DF5D39664A4DE47205A 83BDDE99DD0422C6B77C076473D93C2E
        0-1-1-4-2 512 RGB_EER_DCI_Sat_DCI EB5881C6467F993EAC85AE7C452EBA8F9F4DBB2038706DB33C8F265E028CE673 DEF21AA79967D5CC656DE742CD63A992
        0-1-1-4-3 512 RGB_EER_DCI_Abs_DCI CB4C114C9EF6E018670E930685C899897D3D9868038F516EE84E3D3CBD11CD12 C7C0F57C438DD0058652D92066FB88CE
        0-1-1-5-0 620 RGB_EER_DCI_Per_HLG CC9DDCAA8097786A8AD8D2434BEC29333CA7D7E10314BBFFBB1C25C6B525D927 1AC9489D0A52A7B161900D3ADC8A68A3
        0-1-1-5-1 620 RGB_EER_DCI_Rel_HLG 0AD61C022138CA9C8FD8BA26936D55B0E7DF0E7C86D724985A424BD1580A71D8 EDA1F1126CF523ABCABEB93ECB0D1913
        0-1-1-5-2 620 RGB_EER_DCI_Sat_HLG AB0019604188A21A2DD35011CB9EC7B00D2AFAB2D25B6B73D0A4B9BFE2F95147 27B5DDB5ACB6F4DB181921AFE934B3E0
        0-1-1-5-3 620 RGB_EER_DCI_Abs_HLG D59438A68BC4AB931A812E04BA0D7058DC715E5A6187C092B038A005D2341FCB EB4753A98752AD88DCE4DD847F0F21B9
        0-1-1-6-0 508 RGB_EER_DCI_Per_g0.454545 09B7BE8AB497016EB9F96A8537E31295B50D3B91A89432860CA513AFFDB350CD C6CCC56902DCC12905A1BEC5F29FC84C
        0-1-1-6-1 508 RGB_EER_DCI_Rel_g0.454545 A385D720C0D9C64617D8ECE21FAA29CF887C3A0CFF48DA51C4FE75ECB642AB56 AD287D8797D12333D33EA30637E105CA
        0-1-1-6-2 508 RGB_EER_DCI_Sat_g0.454545 0F089DDBBD5E2729DA0F0F3383EA03DAC6A56D160691525EEC8F6109DA202ADA 103D72FA683EDF9117EF80D9B5685783
        0-1-1-6-3 508 RGB_EER_DCI_Abs_g0.454545 395952CD184FFCEDD98715BBF6B9D6CEEB013EE8AB7135CDFB56CC2391E2AD5E 0FF58177B9CCEA94A264DD3B02E2C590
        0-1-2-0-0 512 RGB_EER_202_Per_709 F5C3509E77F939D00797D9E3143A8D94FA716341B906FDAE1D4C9EF4CFCDE2B2 2A0705FA05BC750E6B467CC72A3B004D
        0-1-2-0-1 512 RGB_EER_202_Rel_709 0DA44F1F5704F9D8A8ED42DC143D7D5CE1F997177AA648F265AAF8B3BB475E58 61F19AC68C20DD665D5C09FAEAED323C
        0-1-2-0-2 512 RGB_EER_202_Sat_709 77A8AAB5F18C13EBB3A9CB1B9D0A19FE0737B2EA0B8BA313381EF0037B7ABB45 C7232A9C6D3A63437D28A540F04DF18C
        0-1-2-0-3 512 RGB_EER_202_Abs_709 0F16B69D24ABE21CA5DDA7AFCF2AE9ECD2184CE2BB0EEB926FEC6D96F5A9D323 F7C14D02D964D15A8C8F64555B3AFA96
        0-1-2-1-0 512 RGB_EER_202_Per_Lin BD5ECD6B6D7EA9C6686F8010E3198DE3B20002497C8628B844F650F3B59C6C47 77174E9ADF13AAE3346118B734C328E0
        0-1-2-1-1 512 RGB_EER_202_Rel_Lin 061BC2F2AAD9BB413DA68281D0F56394D3B1DAB05235BD59163F58C1D248BE66 A9491967C616D5B6E484B67DC53D7F1C
        0-1-2-1-2 512 RGB_EER_202_Sat_Lin 503FA4BF79CFEE4A459AD9B27256E2A160CCCAB573F07839EEEE36F2EE36E3D5 77D05E0DC01CC8948E612FEFE91935A3
        0-1-2-1-3 512 RGB_EER_202_Abs_Lin A5AADB336D0CC34F2C98BC29B6EF04C28C52F5DE752D90EC34B63D50BFDADD10 E3CE036F82E5C02F056E0302D8D049DD
        0-1-2-2-0 512 RGB_EER_202_Per_SRG 50BA6561076B8366809CD6A9295311D840DD947E3396553816BE03BFC279CD6A 2EE33C4B01B832D032F905DC0880C2EE
        0-1-2-2-1 512 RGB_EER_202_Rel_SRG D7DB678C8D45E8DCBD7F55A6F471BA6FF4A48B5E7B72779EBFB4CDAA2B9070D2 0DC793D944A8F444E7A36E706D74B83E
        0-1-2-2-2 512 RGB_EER_202_Sat_SRG 5B3280071B47B287C6FC34099F0AFEBE08A7A8BF1A228BAD8F012E3B5C6FF36E EB10FBEDA75CBFD9F04D3D4ED8B7F141
        0-1-2-2-3 512 RGB_EER_202_Abs_SRG 4F4806EF318CC7C59FB46A11869D5AD404996143DB95DB093F60838DDAEDEBE7 A40B8B699C90A6AD21FF5A506C06E0B1
        0-1-2-3-0 620 RGB_EER_202_Per_PeQ 7F06383B50DD84093ECA8A663486C88C3474F551D910CCA79CD5BC072DEDA704 FDBB0356B60CFF28919EDB8D300038FE
        0-1-2-3-1 620 RGB_EER_202_Rel_PeQ 30C915EFE0F04F43105DA8CA6F2038C9D039430F1374994A1483197C9B0BF063 ECD374CD4F2E13A4698AB6801DE55928
        0-1-2-3-2 620 RGB_EER_202_Sat_PeQ F20C969281D5CD8FAE34CEC04A313A44CF9D762C55A217220AAC3F63BA8FF4A0 6B71E42849E5DD917569EE784D9BBAEE
        0-1-2-3-3 620 RGB_EER_202_Abs_PeQ 522B348C2828EC1D8E06D759BCCD3EE6E940C9FD9ACD8680F761A9021DE5A777 35D1B1CAFBE16BF5383F7019BD03B37F
        0-1-2-4-0 512 RGB_EER_202_Per_DCI 40319D92CCAB7798C44723C982412083CE932594C98C1CACBCE168C12AC5D267 66A72C4FDD638FB227DF1579B89B19D6
        0-1-2-4-1 512 RGB_EER_202_Rel_DCI 087C6789B4E3958DEA86F95146029B451A384505AF598A211067F3169090F004 7CD9374077EFE93D2D0B88BBA40DA891
        0-1-2-4-2 512 RGB_EER_202_Sat_DCI 9FA3572430A3CED155180CDDBFF2A33D44F70BFCFF9596DB5A97858FD863B3EF E2BEC6460317A7B2E9FEFB1307F81B1E
        0-1-2-4-3 512 RGB_EER_202_Abs_DCI 3A3E86C5450117EC598DBB364FC5A30251FD222B262F89A6D2742E8271942A1D 70671EFA6B5035AB4DA41AF05BF5910D
        0-1-2-5-0 620 RGB_EER_202_Per_HLG 5830402DB8DAAEB0F17DBE223FB9947311E79F9CB7198EAB64DC3C2F825B0CB4 F56E9C58D14044D4F48E5AB31541A203
        0-1-2-5-1 620 RGB_EER_202_Rel_HLG A65085CA183E17CE5C00A5464F2BF7DF206F6B39408C10FA8B3FD78A7337463F 424C37502D0A8C378F3020634DB96AEB
        0-1-2-5-2 620 RGB_EER_202_Sat_HLG 478F85E1BA02CAAEA8CD7BCFDC01177F5EAD4507C975EC4CAEA2B1A2CC7D1FB9 A3C196E574BFD0D9C54C4F5CF95A4B2B
        0-1-2-5-3 620 RGB_EER_202_Abs_HLG C6E5A8ABC718C51D04CB7C569A2A8C9EA300CB87901D4FAF7E0375FCB5A2074B 140A49662F95A467E437135836627CC5
        0-1-2-6-0 508 RGB_EER_202_Per_g0.454545 6BB3EF796EB316767516C177D8CF64B4D013718C04D4896A9BEA76875C01C0E5 034BB60FC5DDB876EB2C2E109EFF4638
        0-1-2-6-1 508 RGB_EER_202_Rel_g0.454545 E7DC597DA27AF6E085AB0B1D8787D87266160272DC72107F27F37B81F3BD0C9D 7A8B5DCB61779DA6C8FBCFA504F5F78E
        0-1-2-6-2 508 RGB_EER_202_Sat_g0.454545 A9E102AA248BD7BABD4BBCBA6B512308484D72315CD51B61B340771670EBC242 0693804F88825F16D6ADD9C23E3555B9
        0-1-2-6-3 508 RGB_EER_202_Abs_g0.454545 B047F9BC134A94673DE100ACF8718E3330D7DBBF8C24D3D06A647F00E72B5A43 D940B876658DEE1FE9153B5EA79F6AAC
        0-2-0-0-0 512 RGB_DCI_SRG_Per_709 D0D17FF690B100664B20EEE57469438E3A34D236CC6362CC39263CBFB13CE732 3979E292BEC31A8DD82BE5F7AB3103FF
        0-2-0-0-1 512 RGB_DCI_SRG_Rel_709 D8B257D88548F0E8BB5C9606C69D80E945905FB6D9DFD052DA974760B0E5D3B6 8D3729E77F14E04544CF7529F3CD3297
        0-2-0-0-2 512 RGB_DCI_SRG_Sat_709 21721EA03E2FF15105F91A695CFE7DF133F7314C7658AA4B548EB472469B661B C7708DA4EC215F447AD9C3430E9426DB
        0-2-0-0-3 512 RGB_DCI_SRG_Abs_709 FCC7273EC523EB19B6E51CD041314F1E2EB03BF4D3A3939F7C64ED757C4010E1 ADBD79FE9A8E89C4E8CF810AA3DF62C5
        0-2-0-1-0 512 RGB_DCI_SRG_Per_Lin 931DF4AF6F2FB3CAA57B51F798179DB7C91ACB2A18A0030228BA2DDE81124050 CD77D1C8F0BAD899BEAB797F9298578D
        0-2-0-1-1 512 RGB_DCI_SRG_Rel_Lin 8AC87254C5A49ED84A51200C5BC10139BBB0F39CA34575AB64B8B406D2FC1064 5A25E673DB0787A8ACAF7D06127DB9F8
        0-2-0-1-2 512 RGB_DCI_SRG_Sat_Lin 13C910D21104D58CEA0005D68CA15B064F15005A454814B8E5CDF12A511CC1A3 F844BC98D1AF36FC3FA2E3B70E412446
        0-2-0-1-3 512 RGB_DCI_SRG_Abs_Lin 051FD4AA41EFA53D4B198FDB95C54A2477898BBAA78900465E511865226526A2 34166EC995097B5DA50555FC621E258C
        0-2-0-2-0 512 RGB_DCI_SRG_Per_SRG 2211E98ADC3225B220F5DAAC31ED0879E48EE330A17418EBE3F1EA15CBF988EF 94C2A88CDFFC5181A714DF6EBB5DC155
        0-2-0-2-1 512 RGB_DCI_SRG_Rel_SRG 426CE177B22EDF16C9778EF2354935CEB5FAAA65A1DA781368FEAE15588493DF 8A80973307F45BB9CA92B1EB9F6A537C
        0-2-0-2-2 512 RGB_DCI_SRG_Sat_SRG B6AC9E7CA045F89817E9CFC9F437C62488E17398CA8071BF2EE9D93E7CDD8CEB 00298F2784A84700447363CC0DB6F259
        0-2-0-2-3 512 RGB_DCI_SRG_Abs_SRG EEDB3E45ED5F8458AFE3E62FF204C74E5F93D9A35029A391CD5A66EACA99205B 21B5DC9DB1F0FB41865582AC8458ED34
        0-2-0-3-0 620 RGB_DCI_SRG_Per_PeQ D710FDECE7960D08762B4725D1CA7D27D5703F70FBBFAD8F2BCA2390E7DB7C4E AEF5CBF8978F7DA9BBC0989A53B67A07
        0-2-0-3-1 620 RGB_DCI_SRG_Rel_PeQ FB32BEF26B1CB3EF8F86142A04D516221B16B9713C558DAFC497852A313FB6B4 13B13DDE26C78383CCABB9DD4BF26BBC
        0-2-0-3-2 620 RGB_DCI_SRG_Sat_PeQ 4487F20A5F95EE122D72A37AFBB9667B419978D507E2E04404A7F9D622F5C7E0 444FEE0032098D966D4547BC5A33DBB5
        0-2-0-3-3 620 RGB_DCI_SRG_Abs_PeQ 6A381494DCECBC81BEC3819903764282F8D9BC3AAE82ED3A91034E4A372F786F 4CDF1F11EBE3F0B26E13E80CD454B964
        0-2-0-4-0 512 RGB_DCI_SRG_Per_DCI EC88FC431D4C0C5F3F0A9328671B634FC4574F913625B2596D665F9C97432628 F81188F3E4E30430EBC547DF072D0F8D
        0-2-0-4-1 512 RGB_DCI_SRG_Rel_DCI 88B299A6CBE1D5561666AB28CF5DD2212B879BCCB01FE9348774FE24136F80C0 8A9C701AF40FAD8C551230E1F9DC4C52
        0-2-0-4-2 512 RGB_DCI_SRG_Sat_DCI B827D353F2754AD950DB18383E7B9374384E1836D7D4566366EB4A52715386C4 805A09A593EB5DDF0B5BB845C1060391
        0-2-0-4-3 512 RGB_DCI_SRG_Abs_DCI FB43EB3CC6D30DF1E3CEA66F422814901C4FC85432DA6A80D2458AAF443445DB 8A1E66CF374FF0D66FE946C83D5D956A
        0-2-0-5-0 620 RGB_DCI_SRG_Per_HLG D71EFD4B474F23752AFF86426775682900FAC0D9AFCB71BDC46241EA3CBF644B 821DCDC00E7D7B5B67E616BAF91F0352
        0-2-0-5-1 620 RGB_DCI_SRG_Rel_HLG 7E70ACDF0182EA2B7F081C5CF0ED593A8BB57E9E78D054DAF7417DA6FE6CFAC8 FBE5358A7E0A01AE4D088657D44A4999
        0-2-0-5-2 620 RGB_DCI_SRG_Sat_HLG 8A12DCE40F528F7027A99A96FDD500F69A15C90AA21BD8EDBCB4BF092907020F 3118E693903F18C73818CC95B33E3677
        0-2-0-5-3 620 RGB_DCI_SRG_Abs_HLG 2F36BE1BBA0D9C2752D330C0B059E3FAE66E44CA264C9767F175EAC00E12DA6D 18A426749916E1ABB95D23DDA7E2C319
        0-2-0-6-0 508 RGB_DCI_SRG_Per_g0.454545 A88BA5C739D7E5A39E0CDC0B56FFD25D3E01E1454B47BF23705337D25FF6EE23 05100C43DAFFAF64E0015509BBBBFAF1
        0-2-0-6-1 508 RGB_DCI_SRG_Rel_g0.454545 3E6E024780B6DC0A3B4169FCC1332A2996B4877A3263106B9566F63319B53AE6 6AFCA8EC47AB21F5D93CEC443441C09E
        0-2-0-6-2 508 RGB_DCI_SRG_Sat_g0.454545 24CEBE5C64A480537D2DEC41E84D6C4E558A6AEE40487C877B2127515921BE66 ADC0125FFFF3ED4878ED50832AF41EB3
        0-2-0-6-3 508 RGB_DCI_SRG_Abs_g0.454545 0604598D7713DCF0B4ED43653F445E5CC631D7BA11CF783E5BD4C849C21FFF9D 09EC6E8A2483E452894AB8D34BD2D4BB
        0-2-1-0-0 536 RGB_DCI_DCI_Per_709 D71CEBA9DD3A4302468273C5F02777C6593C382143976119FC1E92262D731341 EBEEA9C9B673524CEB2717334617110F
        0-2-1-0-1 536 RGB_DCI_DCI_Rel_709 2AB71CFA58BC5148EBE1C2211F9A2FB6670BB1A8A526C051302A2D3D643A744B E2378D0840DF05470546B496C0A45373
        0-2-1-0-2 536 RGB_DCI_DCI_Sat_709 D337FA2088DDC2D49D1D0F7620F2372BBF3CDEC86C0D2AC1E033EDEF6F206209 67D2D7B2C6A2E0A9A5D42E3F6A4B94FC
        0-2-1-0-3 536 RGB_DCI_DCI_Abs_709 C2324BBA5D535A2917E8D1275D7CC306EA751EEDAF21B4BBFC9A8AEF590EEE89 F2DE9A19ADE04AC147DA803E74827B5B
        0-2-1-1-0 536 RGB_DCI_DCI_Per_Lin 48A00F31C6243657A012BE6E2693347EFAF6E067332EA0E696EAD4B03DBB8780 F176427A61882C2C9F7FA7D4DF491D61
        0-2-1-1-1 536 RGB_DCI_DCI_Rel_Lin 4814806705CBF95E4BC31BBED39CEF1F21F37029B83F1E5AB5939E6490D6C3A3 C5D75243B37AD23EB31868628B4A7363
        0-2-1-1-2 536 RGB_DCI_DCI_Sat_Lin D34B140C93C75FAEA5E095A97F66C7B921FC8E6C270BE67883EEFED233E8E87B D7DB46162CEC322CBA3543E28632BD27
        0-2-1-1-3 536 RGB_DCI_DCI_Abs_Lin 4C600D3ABB572DB8583AC07E97F2132E32661DEB7B74DFD954414162A360EEF9 C9B2B0E3B3C89FE3BF56ABBD7A0FF87C
        0-2-1-2-0 536 RGB_DCI_DCI_Per_SRG 1A9974D034410AAB04BE9767EA02FDE92BD96EB1ED7BD280BF899AFD30822E7C A27D6F55EA89858BC35AA2273042F46F
        0-2-1-2-1 536 RGB_DCI_DCI_Rel_SRG 33264C9234E14608309DE024893FCAA3B1B282B83E9392F063BB2AE56EDD2A4B 73D67E700161C23BD409C936E36FB8CD
        0-2-1-2-2 536 RGB_DCI_DCI_Sat_SRG 39D1064B210EB96F96CBA23B50D1B626E362278F1A6209DC98D93B323A742926 99472D7E858D04164B8116575C912EAC
        0-2-1-2-3 536 RGB_DCI_DCI_Abs_SRG 5C583317AD01F41DC99F3B1E778A897571DA9C2B3DE9BA9C060A5D659EAFF65F D44DA6C77252D6BF1CB4EE8F7E255E0E
        0-2-1-3-0 4344 RGB_DCI_DCI_Per_PeQ EE622BD33089199FF83268118365F6368AA40FCB09ED31ADC8F44F54DB6A6B44 513BB739042508F2A1A9BBE3BFE1A7F8
        0-2-1-3-1 4344 RGB_DCI_DCI_Rel_PeQ 853CA426943DAFD76DD72B0FC083D2D858554F063A669C8E04A31184C69D1A26 DB07FF204C26FF9F771BEF1AD2C846E5
        0-2-1-3-2 4344 RGB_DCI_DCI_Sat_PeQ CB0DC66BB088902D03928F92B44AAD64B1BCB8B427167FA8F2599DAC76F9C534 3D34DD8387E42B84519BB2F30AC77B6D
        0-2-1-3-3 4344 RGB_DCI_DCI_Abs_PeQ 67BE11347C4055D6A0B138CDA41C77BA63F838EBA288DE17DABAE831D33F660E 6315C91BAA11A1A1A12A8BBBEF41D9BC
        0-2-1-4-0 536 RGB_DCI_DCI_Per_DCI D39C3009847419929E2746189C3EBC10B6DF93A7519A83C78D64A26778D94578 CFF4D77412057A38BA87E9CC1ED826FD
        0-2-1-4-1 536 RGB_DCI_DCI_Rel_DCI 53413E194F813165BA5FA7714360BF81E39C59EDE6080CF8B49CDB833A22B9D6 A67CFF6759145C4C77C6F117C351D3ED
        0-2-1-4-2 536 RGB_DCI_DCI_Sat_DCI 82C3FE168D7C1DF54BCF5E056F25AAFB4F30301C05A881E4EB045A3985EA670D CB1DC19A9ED6B718693C4A4FE6847A16
        0-2-1-4-3 536 RGB_DCI_DCI_Abs_DCI DA1BED85D6D48904633F2FE916ACE1D7A59565FDB2946C888BDCBF253A263505 06D292C79637270B32C9A6B3165E071E
        0-2-1-5-0 4344 RGB_DCI_DCI_Per_HLG 4F20AD000A7459B16190DC2485E286FE9B6970DA7F47B8D56ECAE2C8813F5CCD 86743A4DD500CA3A3146D33B89883666
        0-2-1-5-1 4344 RGB_DCI_DCI_Rel_HLG BC255C0343C95E9FD602EB4E6EDA8FD93E894F72526ED787875D4D723AC36760 7578E9C86DCF5078E7CE977752380D55
        0-2-1-5-2 4344 RGB_DCI_DCI_Sat_HLG 846962D0F0A32F5D85FC95785AEA556A076973F3D553B93DB6D129E159348C22 7BDC6418D0C4408C344E7CFDF8D40DCE
        0-2-1-5-3 4344 RGB_DCI_DCI_Abs_HLG FB15643C29AA31A0579033B76E000218BE76BFF4F12850463CDC4EBF070779EC 8F96E9EF71D80E330914C22F9C5396A1
        0-2-1-6-0 508 RGB_DCI_DCI_Per_g0.454545 C45463D62172DE2C28CD0C8AFA41272BA740F0ED4F27BA2EFFA08F44D80C1C67 B931BAA48A0726430D85564449AB507D
        0-2-1-6-1 508 RGB_DCI_DCI_Rel_g0.454545 449BF017C0B829019E8550A789AA6A58A8C5A10377D238D25A268C89F5EB7694 F4A6C99F1816CD88D5AB5F394F17D685
        0-2-1-6-2 508 RGB_DCI_DCI_Sat_g0.454545 26353692A51D7C22B09330C4F65172F9BF3EA796476FD9D779D3C5FAE3B7AF6C 7046505B40633538178F719A98A1A7A4
        0-2-1-6-3 508 RGB_DCI_DCI_Abs_g0.454545 31BA64BC398323846134F49E1BC3E6A2081220269F2F94BF82AC592D2F22A26A 9910403F3236D1C836AA6CE1815DDA06
        0-2-2-0-0 512 RGB_DCI_202_Per_709 7716528006C24C22CF3E396E60E42C73CD84B26B9BBAA853443938FB1263F32C B46FD0DCB935F892505A37BFF93D9141
        0-2-2-0-1 512 RGB_DCI_202_Rel_709 FAA7BED34FF1C86B91309089173421B4D5087BB227C76FCD42ED790A07811C19 8A0088377E767188A378FE5DCDBF6C7C
        0-2-2-0-2 512 RGB_DCI_202_Sat_709 C33F72B8CEC1F61AC2FFC5CF35D63CF70FA018129DE55DC013F9599FD804CC03 809783AF9DA0C5FB3D8A9F465D2082E9
        0-2-2-0-3 512 RGB_DCI_202_Abs_709 CBD4EF5236C072250F82663AF92827CB97495FBF0E11E0BC34C3217D4AB0E04C D6AD95273444964C3BD1CDFA05D42D90
        0-2-2-1-0 512 RGB_DCI_202_Per_Lin DAC65169A0FDAFE543C5658A93E34C02300C396EF23E8912BA96FA00C8241CE5 4C6CAF9B3342BC9C6CFFDD01277DD5B6
        0-2-2-1-1 512 RGB_DCI_202_Rel_Lin A0689F7E567FC994CAA5872C27620DBF3847A183BBA8B95D11ADBC5229A2B6C1 94CF16A29496CAC2FC5E0448CD7F2172
        0-2-2-1-2 512 RGB_DCI_202_Sat_Lin 829CFF0193DBFB7F99455552E0A5BFD584EACA9CFB99F65D9CD812DB4B5218C8 B0EE986A805BEC5CE34BA8718142132D
        0-2-2-1-3 512 RGB_DCI_202_Abs_Lin B2AEA6493AAC84C7F8EB0E2B1A900904E5A70730E1300964C9F97F1F574A98A5 FD2E7FDD566AA34976989005F7C8CA45
        0-2-2-2-0 512 RGB_DCI_202_Per_SRG CBE8C9180780FEB2FB5E4C99659B057F9280838F1ED0866F5CA28598726D0DA1 FA81FFBA8CAF83A229AFFD4953EEA72D
        0-2-2-2-1 512 RGB_DCI_202_Rel_SRG DBFB754BBF230141CEC13A3BA96A4A237CD6F24A4AA12DDAC5CC311AC6689272 785EB2FC52FE63B29D3748EF92B652EB
        0-2-2-2-2 512 RGB_DCI_202_Sat_SRG 35A5FFAB84B95FCAA7476AB66E468E9A6B89996087D3600078354D6B6A219AF2 28B3E6B81C03A83321D3BA1C603C7EED
        0-2-2-2-3 512 RGB_DCI_202_Abs_SRG 0D22D5519C61516374B01BFED60C5AE7404EE40982FAEE183200D88338F6C4D7 BC5E44CE713F218FAAB2FA7E64C065D7
        0-2-2-3-0 620 RGB_DCI_202_Per_PeQ 729E5C7A0D0E0702862F1D50B6FDF0317AB3EA784F7516006CAFBE308A2C8955 EF3794D6E5CC813BE6180DF250C1E280
        0-2-2-3-1 620 RGB_DCI_202_Rel_PeQ E82C669BC692E87828E305A742603D73E69B8C4C51A86F234007EC93C28E8A7C 3FEC661B7E57A7D2E85E39219FCF96D7
        0-2-2-3-2 620 RGB_DCI_202_Sat_PeQ B97CF1B7497E1CBC3564610D65B9CFF265EBE01D14BC98C1F98C8889B7DE24F0 8D16E79ABB12000385DFE578D61C5196
        0-2-2-3-3 620 RGB_DCI_202_Abs_PeQ 786E24EA894E52F375E801D7273A7C70D49F0525C6DD5B5F81D5B3A6C878D190 EC61D577D74C12D1CD1E19EA424627F7
        0-2-2-4-0 512 RGB_DCI_202_Per_DCI B1E07DE523528C8BCD8F41F797D6D4D487DE25AC8D8DEB636A9C6B32458DEF25 FC2656F99F6BF66D33FD44CD80A4F103
        0-2-2-4-1 512 RGB_DCI_202_Rel_DCI 3909157BD7A1B9C3190D1398F1B32DEEDB2306FB8B228DF70171846CE8DC182B 8C08717B5CA4AB52E32FE6DF03EC7837
        0-2-2-4-2 512 RGB_DCI_202_Sat_DCI B5F910FAA2B3E49B102EBF61E4AD10F50D71A9C99BA6D12861F8121621936648 440AF7083BFFD7D20F7338824607F674
        0-2-2-4-3 512 RGB_DCI_202_Abs_DCI DCFAD2AADC5B19CD064D657CC1B4E7663F51FD35C120AC6A4AEB1E6AFEED097E FC0CAAA08284E85481D16DB9821E3B4A
        0-2-2-5-0 620 RGB_DCI_202_Per_HLG FD58363492EB754A02BB00654ADA69A92CA7371AD30FD9D46F5D882890389169 3CF0422BBAD5649178ED78481C84C056
        0-2-2-5-1 620 RGB_DCI_202_Rel_HLG 84C5F064CDD0A36D2FE51C0494E0AD9BA481F410834B50A13BF202BE7AD3DC12 60C00BE4344EDB2F7D638CC6CDE2D240
        0-2-2-5-2 620 RGB_DCI_202_Sat_HLG ACF7E54D0A9DBFA46EDBEEED190C737316B6FB5136AE946D3245DB0AFD190FCC 78E86A65D269A2E5DA1DD49DB1D01A1C
        0-2-2-5-3 620 RGB_DCI_202_Abs_HLG D173AB59E969664D63D4EDEE671637F1EC668FA5C9EE18E3C8C8101A0CD5660F DBB40D824D971215E7BFE4E287220639
        0-2-2-6-0 508 RGB_DCI_202_Per_g0.454545 590D38E2060675D42133CAEC8534333B94B8BE5F26AF796454BB8B320E676904 DC27F565686E39C4B1793BE6472434C5
        0-2-2-6-1 508 RGB_DCI_202_Rel_g0.454545 014D13506D3116B8DE3B32B53B093BAF787DF03EA388DBF46DD5D473751EBF98 EB8123B16342AA1D362755333B2BD4E3
        0-2-2-6-2 508 RGB_DCI_202_Sat_g0.454545 8E3D2E9C2182AFAD3A6E1B190824BCE973410D48E5AB857B33FA6E868F10881C 9D7D49AEB0362C03682FBEB99DBD3054
        0-2-2-6-3 508 RGB_DCI_202_Abs_g0.454545 D548E3B24DA2485875F3DAB32312832F92C510DE6D158BEDAA87D19B25E210B1 EF09EAAB42D18674398093E017145CC7
        1-0-0-0-0 328 Gra_D65_Per_709 2955B00FF8629C6C518509DC1F31279828D00508B096F4EB9565D271849CBB81 40AB32E3684939CF0CB29EB2C82798F5
        1-0-0-0-1 328 Gra_D65_Rel_709 21BB7D309296C4A133DBC959A823C6D5B6A0ABEE8F8BCEC755B478340899B6BC 63987949042F061A2CFC614A7BA28C0E
        1-0-0-0-2 328 Gra_D65_Sat_709 1B98C059FB18E1BEDF098B296D149E743FC47D406708623FBEF2C1341E3B1679 35E56C2E71C32703342363AB803B09A1
        1-0-0-0-3 328 Gra_D65_Abs_709 E0AEBE49BF9C414FAC99D63D4CFC88A003F78BE216A8E79E1FCAC38139AB062A 640C47F9A27B0C3E4E7408C8EF7A7D26
        1-0-0-1-0 328 Gra_D65_Per_Lin D05AF72D689C7497399BFE7BEB87AE7C3687AAB4AB9347C16DCAAFA2325C0404 83DF0E7B027D00AC32774D1BB8D17F1D
        1-0-0-1-1 328 Gra_D65_Rel_Lin 57363D9EC00043FE6E3F40B1C3E0CC4676773011FD0818710FB26545002AC27D 1948C0D0B76231404F39210BE0ECA278
        1-0-0-1-2 328 Gra_D65_Sat_Lin F9685F0624CFA55E8F82414DD0517FE422FE3BBC84CD1A495E811E84EB81D1AB FD3BA7D2DC0CDCEE7B2451481D717ED6
        1-0-0-1-3 328 Gra_D65_Abs_Lin C7FA974EF57AB2C0A65E1D4BAFFCCAF561EE51CDD330D0C4D3477E9102F8B3BD DDA520E614E743A182392A741F5D209B
        1-0-0-2-0 328 Gra_D65_Per_SRG BD99A693D2BA41BA6C0E818B2D7EE2B2A313289AD5B9C66FEFD54D09FFCA6406 57A702AE8C0F59013AAA9E549C8C5CDA
        1-0-0-2-1 328 Gra_D65_Rel_SRG 48D006762D583F6E354A3223C0A5AEAFF7F45A324E229D237D69630BCC170241 299AB5112C852FC36DE6088516DB10CD
        1-0-0-2-2 328 Gra_D65_Sat_SRG A740CEC0E4F43FBA3762371FEDDD1EC850C29273CA7CB5008055A46E17CBA49C 7242AE4265A4745546C264F22A676FD4
        1-0-0-2-3 328 Gra_D65_Abs_SRG 1859F0F5516436B7B58425C56D9F3A0803F00793157783B6EC7CBD5836DF1DED 81859C899F65AE311171E765EC794CC0
        1-0-0-3-0 436 Gra_D65_Per_PeQ 1F25816054A452DD533EF8F56203506387A9521474225B59DEB3D8D91F51BF58 5713614D083B44B7CEA46698FE2DAF5F
        1-0-0-3-1 436 Gra_D65_Rel_PeQ EBF0E2F4AF24AD9886D35FFEA5687432BA56CA230D630335DF41F2B2606CD2B5 31CCE3447985195058E1B75C929E6BAF
        1-0-0-3-2 436 Gra_D65_Sat_PeQ F1D0FB3E66D46BEC57AEF10B528134345CCDD0D229FE7E5540E2BB9B750DE903 DDFA106B4724EDD077A4274B78E3FFE5
        1-0-0-3-3 436 Gra_D65_Abs_PeQ E1619972BE205B2B57F5784549CD16247FD6ADBAEBB1975C74D6ABD35A7379C7 3B21F2AF9ABE9BC829E2F5EA08081C66
        1-0-0-4-0 328 Gra_D65_Per_DCI E3BE549BAB3FEFF815E9CCFAEAC3416D2717B62E03E578156610C382949B3608 48B51FFC644EB84D29C9F7CF8DA93180
        1-0-0-4-1 328 Gra_D65_Rel_DCI FBD865816E1E663EE4F4797787BAEE630C3B3F502889ABFBD56A7B5F803C6DDF A394A0527BDFEEBBE7410BF80F513DB4
        1-0-0-4-2 328 Gra_D65_Sat_DCI 16E3973529B35E8E93ABE2E86E44096C059B388702DFC5210E81FD88D1A00C94 7F81A25E1637FB63AC7C6D16EC7984A2
        1-0-0-4-3 328 Gra_D65_Abs_DCI A286731694ABD667FBC85B762B9DD0954BB446BBAEE72971279D0CAA679B628B C7D689BACE784F1B7740E6FFAD5E653B
        1-0-0-5-0 436 Gra_D65_Per_HLG 0EA7002A458B45560E4FA1B9FA8B99115C2CFB5468DCA8FE494480E6D093E013 6B40A764E4E988F46E9F399DE140CA8C
        1-0-0-5-1 436 Gra_D65_Rel_HLG E19D615BAD5F967D4ECDA1168CD8D2973CA66C7CF793775E0CD39B9F8B462E52 ED355BB4E23679124AD235F4C62D0CE8
        1-0-0-5-2 436 Gra_D65_Sat_HLG F693115C894D2AF52365BD0F5F4869F983A4827E938F127DC6BBE4B5941C1459 BC9057232EF0CAA9F0127B5DCF0B534F
        1-0-0-5-3 436 Gra_D65_Abs_HLG AB372C25531C15A58F5915D2F41B0548303ED999BEAAB6BB8A70A364A9741E20 68BC6A510C3262DA214E7B5AB2B63C99
        1-0-0-6-0 324 Gra_D65_Per_g0.454545 3B9ABDA49BB8C3869C7D4B09595F8BD1AD1B95C8E2F956D4EFD4FACCA808381E 165030E6E575D21BBB727F0344B1A59B
        1-0-0-6-1 324 Gra_D65_Rel_g0.454545 CF8F4950A44C20C14C20F80DE4C4EB9A0AE65221738B3E8C4A9ED7C7C49026EE FF46DC08572C9E71FBE1657CE8963825
        1-0-0-6-2 324 Gra_D65_Sat_g0.454545 4E9E94A6C1B04742D3EF3AC80820E30677E7C61B79BAB33B615887E9F9550A1E E5D26E286423C44A5D3598E0D75D2807
        1-0-0-6-3 324 Gra_D65_Abs_g0.454545 9439C3A802BDC258C474791C3470B27AADDE091096DFF91FD6B88899DF05DE64 A8D686BDC13929E3FD59AAAD1B956805
        1-1-0-0-0 328 Gra_EER_Per_709 92E06A518FD04049CADCA0395C175C936A94D5789604D55B5877C03DECC78DA7 AF3CBEB01E7DAE00D7D74BFD5178FBE9
        1-1-0-0-1 328 Gra_EER_Rel_709 1C66206FDAD829B8E67951F29BA96910240F739100D97C9983FA91A8DE212E83 5478C41575E852097C50C84E70096B79
        1-1-0-0-2 328 Gra_EER_Sat_709 2670D164EC24B5E247381A79942799B89D634C97F8346644DA0A2B5CBD6E48B3 E5007CF1DEBC6850BECC6E105E78B4DE
        1-1-0-0-3 328 Gra_EER_Abs_709 94B02E6984A258E4E9FB2919F3830F2BC511D7E0A3B72A7D74DE33FCADDE2E42 BEBDFE8D3437F03029EA3E5D74BA777A
        1-1-0-1-0 328 Gra_EER_Per_Lin 6EE164A12274CE1E137B4BAC5C42F39821985416FE8329E671D4A569DBB4AB89 0019F22A853A8E2F185476AAF1370A32
        1-1-0-1-1 328 Gra_EER_Rel_Lin 7C4D390ECC680686B532FF83CD3CE2873DD1469C3CFD593F9C4340356F33529C A203CCBE3E27C555173CAEF628F51963
        1-1-0-1-2 328 Gra_EER_Sat_Lin 6664CC83E50C76E6AF74EA659645D6424B30607D5F438EA02B92038B79F43C0F ADD45C2B915E057D8087641CC4723288
        1-1-0-1-3 328 Gra_EER_Abs_Lin C75CE3931FCDCE45B1FBAA21D1351F474A9E807ECD820398AC4F8E05600CA102 0A43678B2C69681ECC58B26F0E23121A
        1-1-0-2-0 328 Gra_EER_Per_SRG DC0004D2709371DAF7EBD4E79E2A8FE810943DDBB2257073B902F34951EA2BAE 5FF230A826D98E2F1E645AF5BBEBCE23
        1-1-0-2-1 328 Gra_EER_Rel_SRG 710B784C529A8A7B4C9EEC0FC92D58E7DBCBF1BCDA020050E3FF07F62745F210 9AA714E729A9D2B585622DB4364FB49D
        1-1-0-2-2 328 Gra_EER_Sat_SRG A90E0D411498215760374FEA12FCF0197231D1C0957B7CF873F031DF28CF7357 B7AF9F7F54FC297CF29E26060DD43DC2
        1-1-0-2-3 328 Gra_EER_Abs_SRG 95B539DC142D73B30424CE4A4915C29445288845F82E45F679869E2D4977D76C B1126AA7A1CDC3D3C93AB28147C50208
        1-1-0-3-0 436 Gra_EER_Per_PeQ 87D348E84C5191C1E5658E6A03141713401AB581A2CA4ED863D143ECD07A2227 4D454650144C7657B32776213C8A1D72
        1-1-0-3-1 436 Gra_EER_Rel_PeQ A739928A5A4EB72BE160DBD5CC6DA860A8076E4CE49205820684C81BD866F57D 84FBF4C2CC91C38E4465FA812513FC71
        1-1-0-3-2 436 Gra_EER_Sat_PeQ A177409EDE9295C974F24688EB95DF7057B8E71C4B3374285F517A489281FF12 100684598097B4CF7D5F75BC00E169B0
        1-1-0-3-3 436 Gra_EER_Abs_PeQ 83D68DDD9E1EC8E6FCC815555FD348724FFC48D7F357114FE3CF12A4D488C572 9027C4A5F8503820C0D3AEC0BA88ABAC
        1-1-0-4-0 328 Gra_EER_Per_DCI 662B0AE8736C680B088F2D651E7536C1D28664B41C159A45A3FBB4F364276FD8 74B4190E5DD4D4CE0BCE04A912C76ABA
        1-1-0-4-1 328 Gra_EER_Rel_DCI 9381A568DE1F688CC9A158C6B40617A8E9ED269C4711208D95CFFA86A6088255 DE9D2CE35DA645FBB7A33A9C9FDC2190
        1-1-0-4-2 328 Gra_EER_Sat_DCI 8AE792224E2838072239BF0884C917B8CAFAAA31DAE587CA376BBD56B35543A0 8F9DFB5C9985FD9764AE307BFA3E9CE8
        1-1-0-4-3 328 Gra_EER_Abs_DCI 4714603CE68E6E113765FEAFF3D735C03C2247D7B0D1B51BD81CDD9F0DFD0A0B 0AA00A686B29F3B695394A097AD15D64
        1-1-0-5-0 436 Gra_EER_Per_HLG 9B7EE9B950BECFB7D96E5364F31B5A1B48886B99EA146DC5D265D998398695C5 526711E3ABD8A4E19405E72F4C13D336
        1-1-0-5-1 436 Gra_EER_Rel_HLG 311EE33A3CA55FBD3D494BD4E41EF65D9E00AA709CE3CDF56B419B6E111824F8 9137EEBF39A0D695EEF8319F3AB1D86A
        1-1-0-5-2 436 Gra_EER_Sat_HLG 4DB1DF1522F6B4BBC9629E5E7B92D539A50E2D5A13B2195556939CDC1157E4F7 2A875621F27192BC4AB8082A6444EB25
        1-1-0-5-3 436 Gra_EER_Abs_HLG EEF2BFA28CA47BD60BFAB5D2143311CBE78A1E8848E626E2031F3F3726CC5C92 BFA8C1E3DFBA26478A22570F5CF71A40
        1-1-0-6-0 324 Gra_EER_Per_g0.454545 3D33AC3DE17974A35C5AD42B5FF17BD75408C96C836ECBF661C895DED48B861E 36A1AFAE7EB87A42EF35D305343CF832
        1-1-0-6-1 324 Gra_EER_Rel_g0.454545 A0732FFADA192A898D0A0AC8B0E1E9E8833755379BBB42EB99D5B039CA23F11E 6961025214D9135145D487FF970D7675
        1-1-0-6-2 324 Gra_EER_Sat_g0.454545 9CC2F460787EFF3CAE9492103FCC1444AD8105EB016FFC454EA3078E93CE7D53 1F80FC07C550C1192539286A579850F5
        1-1-0-6-3 324 Gra_EER_Abs_g0.454545 C6A346F771A33FE333B9B589A2BE39F46B2E629446B3499951DEC24AD5CCA629 7DD90FB988C2856218284D5ECDC4537F
        1-2-0-0-0 328 Gra_DCI_Per_709 3D42D09D8472453B1540FBDF53DC7C00D79C8EE2FEBC21E13F325E2E573BB85A FB8B4EAEC494FA9BA9239C3B6DCC3165
        1-2-0-0-1 328 Gra_DCI_Rel_709 7E9DF4E650835709AD965ACE581A9824DDB196C6B759962CF8BFF46F14D558F2 FE90263782AEFED0BA1E5FD059996374
        1-2-0-0-2 328 Gra_DCI_Sat_709 20C399541DC2B981AA94A49F4A081EFD6F65E507685F8964E4DA2E48302A838B 45D562B2E63220E805E72859D7B800EF
        1-2-0-0-3 328 Gra_DCI_Abs_709 7E1F7491B69F15742C31940A789B7BE4B8838458CE44F39E47B4B71E9752E604 82EF6DEADEF5E1DADAC8C32B266D8CA6
        1-2-0-1-0 328 Gra_DCI_Per_Lin E7F6EF0D550EA9F39E9FC7856F2AEF3C0959409E4EA491CF40EA12F4A5C38900 FD92836D025F3D0DA72AFBFE3B09C648
        1-2-0-1-1 328 Gra_DCI_Rel_Lin 7E74B791F3B030F36353489303D3495CDC9F73D89DA327F98973778779106111 D468AD02780143896C902B76CDF21838
        1-2-0-1-2 328 Gra_DCI_Sat_Lin B0020B281A208CC265EA7A06B5755E0DB6FF6AA7DDFAA72288FE957F1B1DFEA3 6CC00A5E7353EF3F119B11E5B442438B
        1-2-0-1-3 328 Gra_DCI_Abs_Lin 826A3D696EA13F9882D63BD9AC600A3D690C62280C9A785DB298E669D116828A FC4997DC95E49D997D0AB2A05FF2CF7F
        1-2-0-2-0 328 Gra_DCI_Per_SRG E77C1CA0B54CFE8AA391CE6ACA417C15E90E49C721AE014C6399739834190F9F FF9354CAEE082C7E3600F2CE4B1DCDD3
        1-2-0-2-1 328 Gra_DCI_Rel_SRG F86B769BCF360DC7220E5FEF3CD65B162E56395736E3560D5F37D83EFEEF5048 740827C98A6717E10708EF0051477AAD
        1-2-0-2-2 328 Gra_DCI_Sat_SRG 609F5BC7C768AF8480A18A6A51DAE429266D9D9E09E5EE5AEE93CF40E0D03EC6 EEC0EBB270540DF6D35DEDC9707821D3
        1-2-0-2-3 328 Gra_DCI_Abs_SRG F9F97174166740593E9CC50B6F8AB3F2D0A6EB708E0E57EB5D94163D07FF6887 71B7B32002171D380E5D19F585208A6D
        1-2-0-3-0 436 Gra_DCI_Per_PeQ 40FC10E97A888A580BB9C83456107BD941E1807ABB0424156364DC94FB8D934E 58884A1F2B0CC2CE30958D6C0DB2A241
        1-2-0-3-1 436 Gra_DCI_Rel_PeQ C773B9C41B0DA16E359FF0520DDBAC92A87156274159AE3F93C5CD0DCB819999 8C7A4FDF6643382AAFD33CB9D60EE044
        1-2-0-3-2 436 Gra_DCI_Sat_PeQ F482ED912B57392969DCEE2F05E6B59577C46C59FEB989DFEF4C3829F9FE1023 1EDCF5A6B14D93EBCB4A1061A7BE992D
        1-2-0-3-3 436 Gra_DCI_Abs_PeQ 700775DEE735058F0873FB72C52B2C18E770372B6468D057AB487DD3D1182F19 49EBEE3FFA142680B6974694B089CD41
        1-2-0-4-0 328 Gra_DCI_Per_DCI E3D49DE1DEE11786DBA14DE3F273B1CBBC86FD5501F3DC42CD393F737ACE82FB C20C5B4FA3DD24109E6DA3D52288B148
        1-2-0-4-1 328 Gra_DCI_Rel_DCI D970DAA032BCF85DE92BB5630608B4026525DAB1998AC4D1D9307C4F9FF51A26 095D1221B485EE866B0281D711A65AB6
        1-2-0-4-2 328 Gra_DCI_Sat_DCI ABEF3264B4BD5AAD69F89C5A541D38F1E56C18ED1ED754286EEE2A7BB30EDB49 04FAB36B81635E85CB60A6D7E5CE523B
        1-2-0-4-3 328 Gra_DCI_Abs_DCI 6A2F45DB780DD6A996BCE2825C5D40E3D9F17C783EBA50146B9DCF35802AF454 3A3B4512F7B234CF634BAF3D7D4C1137
        1-2-0-5-0 436 Gra_DCI_Per_HLG B62BAC320C12FEBC82AABE9AABF89B5C5D7246FA53154DB3329118E1095C3408 6E6E1A5EF9E60826DFBA0CF47719E720
        1-2-0-5-1 436 Gra_DCI_Rel_HLG 6EE6E46DE3C058CAF47886007052FC714CD74F2D5652442CE9AF6049A49B82E0 66D238C7E44EEB4ADEFB89C06DD35B0F
        1-2-0-5-2 436 Gra_DCI_Sat_HLG 8382AD399F252DBA9C2C73292AE6344A51B3AEF24351789571FA34FCB410FB07 9CF9B0A04D9AF9DAA9641E44110571AC
        1-2-0-5-3 436 Gra_DCI_Abs_HLG 2AAACC65460B8883EC484AF324F7ED5FFA5CB1DE3CC94642911FB8B73E380348 E60FAF52EABFE21D9438C2911DF3D7BE
        1-2-0-6-0 324 Gra_DCI_Per_g0.454545 08960982194E269FA51796AA52B68CD12992E7EF97318FF592F2E67D309D6ACA E43498D0E17FE3527695FB3094E8C420
        1-2-0-6-1 324 Gra_DCI_Rel_g0.454545 7E7C3395E38A57AAEB65147E7DB9544815FD5156B20B5651081FF08F83E54F16 F86BE0751605A55FD6E724C3B77F5BE9
        1-2-0-6-2 324 Gra_DCI_Sat_g0.454545 D49A31CCA609C0A409EA0610BF9442A8580A5D0F80C08426C0FC15E3FD65A202 E1B777C74B8411C3082F7005C7B8919E
        1-2-0-6-3 324 Gra_DCI_Abs_g0.454545 B4F85EFF47228B7EC6DDD65C8FF5944E3AFE69541528E187713659A7CE728C62 92026AF7D02085E10012BE28983D39A2
        """;
}
