using System.Buffers.Binary;
using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class SourceIccColorManagementTests
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public static TheoryData<int, PixelComponentLayout> SourceModes
    {
        get
        {
            var modes = new TheoryData<int, PixelComponentLayout>();
            for (int precision = 0; precision < 3; precision++)
                foreach (PixelComponentLayout layout in new[]
                    { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba, PixelComponentLayout.Bgra, PixelComponentLayout.Gray })
                    modes.Add(precision, layout);
            return modes;
        }
    }

    [Theory]
    [InlineData(0.0f, 0.003f, 0.0031308f)]
    [InlineData(0.01f, 0.18f, 0.5f)]
    [InlineData(0.6f, 0.8f, 1.0f)]
    public void LinearSrgbUsesIndependentPiecewiseTransferOracle(float r, float g, float b)
    {
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbProfile(), JpegliIccProfiles.CreateSrgbProfile());
        AssertConversion(transform, r, g, b, Srgb(r), Srgb(g), Srgb(b), 0.0001);

        var reverse = Transform(JpegliIccProfiles.CreateSrgbProfile(), JpegliIccProfiles.CreateLinearSrgbProfile());
        AssertConversion(reverse, r, g, b, Linear(r), Linear(g), Linear(b), 0.0001);
    }

    [Theory]
    [InlineData(0.15f, 0.3f, 0.5f)]
    [InlineData(0.6f, 0.25f, 0.4f)]
    [InlineData(0.95f, 0.05f, 0.2f)]
    public void DisplayP3UsesIndependentD65PrimaryMatrixOracle(float r, float g, float b)
    {
        // Standard D65 linear Display P3 -> linear sRGB matrix. Both profiles
        // Bradford-adapt to the same PCS, so that adaptation cancels. ICC fixed-
        // point colorants account for the small tolerance in this double oracle.
        double lr = Linear(r), lg = Linear(g), lb = Linear(b);
        double expectedR = Srgb(Math.Clamp(1.2249401763 * lr - 0.2249401763 * lg, 0, 1));
        double expectedG = Srgb(Math.Clamp(-0.0420569547 * lr + 1.0420569547 * lg, 0, 1));
        double expectedB = Srgb(Math.Clamp(-0.0196375546 * lr - 0.0786360456 * lg + 1.0982736002 * lb, 0, 1));
        var transform = Transform(JpegliIccProfiles.CreateDisplayP3Profile(), JpegliIccProfiles.CreateSrgbProfile());
        AssertConversion(transform, r, g, b, expectedR, expectedG, expectedB, 0.0004);
    }

    [Theory]
    [InlineData(0.003f)]
    [InlineData(0.18f)]
    [InlineData(0.75f)]
    public void GrayscaleProfilesDecodeAndEncodeWithoutRgbLumaConversion(float gray)
    {
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(),
            JpegliIccProfiles.CreateSrgbGrayscaleProfile(), sourceGray: true, destinationGray: true);
        AssertConversion(transform, gray, gray, gray, Srgb(gray), Srgb(gray), Srgb(gray), 0.0001);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AllFiveParametricCurveTypesFollowIndependentPiecewiseEquations(int function)
    {
        double[] parameters = function switch
        {
            0 => [2],
            1 => [2, 1, -0.25],
            2 => [2, 1, -0.25, 0.125],
            3 => [2, 1, 0, 0.25, 0.25],
            _ => [2, 1, 0, 0.25, 0.25, 0.0625, 0.0625]
        };
        byte[] source = ParametricGrayProfile(function, parameters);
        var transform = Transform(source, JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), true, true);
        var inverse = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), source, true, true);
        foreach (float x in new[] { 0.1f, 0.25f, 0.6f })
        {
            double expected = function switch
            {
                0 => x * (double)x,
                1 => Math.Pow(Math.Max(x - 0.25, 0), 2),
                2 => Math.Pow(Math.Max(x - 0.25, 0), 2) + 0.125,
                3 => x < 0.25 ? 0.25 * x : x * (double)x,
                _ => x < 0.25 ? 0.25 * x + 0.0625 : x * (double)x + 0.0625
            };
            AssertConversion(transform, x, x, x, expected, expected, expected, 0.000001);
            if (function is not (1 or 2) || x >= 0.25f)
                AssertConversion(inverse, (float)expected, (float)expected, (float)expected, x, x, x, 0.000001);
        }
    }

    [Fact]
    public void SampledCurvesInterpolateAndInvertTheirStoredIntervals()
    {
        byte[] sampled = SampledGrayProfile(0, 16384, 65535);
        byte[] linear = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        var forward = Transform(sampled, linear, true, true);
        double low = 16384 / 65535d;
        AssertConversion(forward, 0.25f, 0.25f, 0.25f, low / 2, low / 2, low / 2, 0.000001);
        AssertConversion(forward, 0.75f, 0.75f, 0.75f, (1 + low) / 2, (1 + low) / 2, (1 + low) / 2, 0.000001);
        var reverse = Transform(linear, sampled, true, true);
        float sample = (float)((1 + low) / 2);
        AssertConversion(reverse, sample, sample, sample, 0.75, 0.75, 0.75, 0.000001);
        var error = Assert.Throws<ArgumentException>(() => Transform(linear, SampledGrayProfile(0, 65535, 16384), true, true));
        Assert.Equal("IccProfile", error.ParamName);
    }

    [Fact]
    public void LabGrayscaleCurveRepresentsLightnessRatherThanXyzLuminance()
    {
        byte[] lab = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        "Lab "u8.CopyTo(lab.AsSpan(20));
        byte[] xyz = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        var forward = Transform(lab, xyz, true, true);
        foreach (float gray in new[] { 0f, 0.05f, 0.5f, 1f })
        {
            double l = gray * 100d;
            double expected = l <= 8 ? l * 27 / 24389 : Math.Pow((l + 16) / 116, 3);
            AssertConversion(forward, gray, gray, gray, expected, expected, expected, 0.000001);
        }
        var reverse = Transform(xyz, lab, true, true);
        double lightness = (116 * Math.Cbrt(0.18f) - 16) / 100;
        AssertConversion(reverse, 0.18f, 0.18f, 0.18f, lightness, lightness, lightness, 0.000001);
    }

    [Fact]
    public void AbsoluteIntentScalesMediaWhiteWhileRelativeIntentKeepsPcsLuminance()
    {
        byte[] source = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        byte[] destination = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        int sourceWhite = Tag(source, "wtpt"u8), destinationWhite = Tag(destination, "wtpt"u8);
        Write32(source, sourceWhite + 12, 32768);
        Write32(destination, destinationWhite + 12, 16384);
        // Source gray XYZ is D50*curve irrespective of media white. Absolute
        // intent then applies source/destination wtpt component ratios.
        AssertConversion(Transform(source, destination, true, true), 0.2f, 0.2f, 0.2f, 0.2, 0.2, 0.2, 0.000001);
        AssertConversion(Transform(source, destination, true, true, IccRenderingIntent.AbsoluteColorimetric),
            0.2f, 0.2f, 0.2f, 0.4, 0.4, 0.4, 0.000001);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void DestinationParametricCurveWhollyLinearBranchInvertsAndClipsCorrectly(int function)
    {
        byte[] destination = function == 3 ? ParametricGrayProfile(3, 2, 1, 0, 0.5, 2) :
            ParametricGrayProfile(4, 2, 1, 0, 0.5, 2, 0, 0.125);
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), destination, true, true);
        double offset = function == 4 ? 0.125 : 0;
        foreach (float sample in new[] { 0.2f, 0.5f, 0.8f })
        {
            double expected = Math.Clamp((sample - offset) / 0.5, 0, 1);
            AssertConversion(transform, sample, sample, sample, expected, expected, expected, 0.000001);
        }
    }

    [Fact]
    public void DestinationParametricTerminalPlateauUsesFirstWhiteSample()
    {
        byte[] destination = ParametricGrayProfile(3, 1, 1, 1, 0, 0.5);
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), destination, true, true);
        AssertConversion(transform, 1, 1, 1, 0.5, 0.5, 0.5, 0.000001);
    }

    [Theory]
    [InlineData(3, 1, 1, 4, 0.5, 0.25)]
    [InlineData(4, 1, 1, 4, 0.5, 0.25)]
    [InlineData(3, 2, 0, 2, 0.75, 0.5)]
    [InlineData(4, 2, 0, 2, 0.75, 0.5)]
    public void DestinationParametricWhitePlateauCanBeginOnTheLinearBranch(int function, double a, double b, double c, double d, double expected)
    {
        byte[] destination = function == 3 ? ParametricGrayProfile(3, 1, a, b, c, d) :
            ParametricGrayProfile(4, 1, a, b, c, d, 0, 0);
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), destination, true, true);
        AssertConversion(transform, 1, 1, 1, expected, expected, expected, 0.000001);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void DestinationParametricInverseGapSelectsNearestRepresentableBranch(int function)
    {
        byte[] destination = function == 3 ? ParametricGrayProfile(3, 1, 1, 0, 0.1, 0.5) :
            ParametricGrayProfile(4, 1, 1, 0, 0.1, 0.5, 0.125, 0.0625);
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), destination, true, true);
        transform.Convert(0.2f, 0.2f, 0.2f, out float r, out float g, out float b);
        // The nearest low-branch output must remain below d after conversion
        // to float. A double predecessor of d rounds back onto the high branch.
        Assert.Equal(float.BitDecrement(0.5f), r);
        Assert.Equal(r, g);
        Assert.Equal(r, b);
        float high = function == 3 ? 0.4f : 0.5f;
        transform.Convert(high, high, high, out r, out g, out b);
        Assert.Equal(0.5f, r);
        Assert.Equal(r, g);
        Assert.Equal(r, b);
    }

    [Fact]
    public void SampledDestinationInitialInteriorAndTerminalFlatsHaveDefinedInverse()
    {
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(),
            SampledGrayProfile(0, 0, 32768, 32768, 65535, 65535), true, true);
        AssertConversion(transform, 0, 0, 0, 0.2, 0.2, 0.2, 0.000001);
        float middle = 32768 / 65535f;
        // Float storage is just below the exact midpoint, selecting the first
        // edge; an exact plateau value cannot be represented as an input float.
        AssertConversion(transform, middle, middle, middle, 0.4, 0.4, 0.4, 0.000001);
        AssertConversion(transform, 1, 1, 1, 0.8, 0.8, 0.8, 0.000001);
    }

    private static byte[] ParametricGrayProfile(int function, params double[] parameters)
    {
        byte[] data = new byte[12 + parameters.Length * 4];
        "para"u8.CopyTo(data);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(8, 2), (ushort)function);
        for (int index = 0; index < parameters.Length; index++)
            Write32(data, 12 + index * 4, unchecked((uint)(int)(parameters[index] * 65536)));
        byte[] profile = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        int offset = (profile.Length + 3) & ~3;
        Array.Resize(ref profile, offset + data.Length);
        data.CopyTo(profile, offset);
        Write32(profile, 0, (uint)profile.Length);
        int count = (int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(128, 4));
        for (int index = 0; index < count; index++)
        {
            int entry = 132 + index * 12;
            if (profile.AsSpan(entry, 4).SequenceEqual("kTRC"u8))
            {
                Write32(profile, entry + 4, (uint)offset);
                Write32(profile, entry + 8, (uint)data.Length);
                return profile;
            }
        }
        throw new InvalidOperationException("Missing fixture curve.");
    }

    private static byte[] SampledGrayProfile(params ushort[] values)
    {
        byte[] profile = JpegliIccProfiles.CreateSrgbGrayscaleProfile();
        int curve = Tag(profile, "kTRC"u8);
        "curv"u8.CopyTo(profile.AsSpan(curve));
        Write32(profile, curve + 8, (uint)values.Length);
        for (int index = 0; index < values.Length; index++)
            BinaryPrimitives.WriteUInt16BigEndian(profile.AsSpan(curve + 12 + index * 2, 2), values[index]);
        return profile;
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.YCbCr)]
    public void ConstantLinearSourceHasIndependentQuantizedDcOracle(JpegColorSpace colorSpace)
    {
        const float r = 0.18f, g = 0.36f, b = 0.60f;
        float[] pixels = Enumerable.Range(0, 64).SelectMany(_ => new[] { r, g, b }).ToArray();
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(), ColorSpace = colorSpace,
            Quality = 100, AdaptiveQuantization = false, Subsampling = ChromaSubsampling.None
        };
        byte[] jpeg = Encode(pixels, 8, 8, PixelComponentLayout.Rgb, options);
        JpegDocument document = Read(jpeg, options);
        double er = Srgb(r) * 255, eg = Srgb(g) * 255, eb = Srgb(b) * 255;
        double[] samples = colorSpace == JpegColorSpace.Rgb ? [er, eg, eb] :
            [0.299 * er + 0.587 * eg + 0.114 * eb,
             128 - 0.168736 * er - 0.331264 * eg + 0.5 * eb,
             128 + 0.5 * er - 0.418688 * eg - 0.081312 * eb];
        int[] ids = colorSpace == JpegColorSpace.Rgb ? ['R', 'G', 'B'] : [1, 2, 3];
        for (int channel = 0; channel < 3; channel++)
        {
            Block block = document.Blocks[new(ids[channel], 0, 0)];
            byte quantizer = document.Quantizers[document.Components[ids[channel]].Quantizer][0];
            int expected = (int)Math.Round(8 * (samples[channel] - 128) / quantizer, MidpointRounding.AwayFromZero);
            Assert.InRange(block.Coefficients[0], expected - 1, expected + 1);
            Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
        }
        Assert.Equal(JpegliIccProfiles.CreateSrgbProfile(), ReassembleIcc(jpeg));
    }

    [Fact]
    public void GrayscaleDestinationUsesProfileLuminanceBeforeItsEncodingCurve()
    {
        byte[] source = JpegliIccProfiles.CreateLinearSrgbProfile();
        const float r = 0.18f, g = 0.36f, b = 0.60f;
        float[] pixels = Enumerable.Range(0, 64).SelectMany(_ => new[] { r, g, b }).ToArray();
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = source, ColorSpace = JpegColorSpace.Grayscale,
            Quality = 100, AdaptiveQuantization = false
        };
        byte[] jpeg = Encode(pixels, 8, 8, PixelComponentLayout.Rgb, options);
        JpegDocument document = Read(jpeg, options);
        // Independent ICC parsing extracts the PCS luminance row. Applying the
        // grayscale curve before conventional RGB luma would give another DC.
        double linearY = r * ColorantY(source, "rXYZ"u8) +
            g * ColorantY(source, "gXYZ"u8) + b * ColorantY(source, "bXYZ"u8);
        int expected = (int)Math.Round(8 * (255 * Srgb(linearY) - 128), MidpointRounding.AwayFromZero);
        Block block = Assert.Single(document.Blocks).Value;
        Assert.Equal((byte)1, document.Quantizers[document.Components[1].Quantizer][0]);
        Assert.InRange(block.Coefficients[0], expected - 1, expected + 1);
        Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
        Assert.Equal(JpegliIccProfiles.CreateSrgbGrayscaleProfile(), ReassembleIcc(jpeg));
    }

    [Theory]
    [MemberData(nameof(SourceModes))]
    public void AllSourcePrecisionsAndLayoutsTransformBeforeSamplingSmoothingAndAdaptiveAnalysis(int precision, PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        bool gray = layout == PixelComponentLayout.Gray;
        int stride = width * Components(layout) + 11;
        Array pixels = Source(precision, width, height, layout, stride);
        Array before = (Array)pixels.Clone();
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = Sliced(gray ? JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile() :
                JpegliIccProfiles.CreateLinearSrgbProfile()),
            SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, SamplingFactors = gray ? null : new(new(2, 2), new(2, 1), new(1, 1))
        };
        byte[] actual = Encode(pixels, width, height, layout, options, stride);
        float[] converted = IndependentlyConvert(pixels, width, height, layout, stride);
        var expectedOptions = options with
        {
            SourceIccProfile = default,
            IccProfile = gray ? JpegliIccProfiles.CreateSrgbGrayscaleProfile() : JpegliIccProfiles.CreateSrgbProfile()
        };
        byte[] expected = Encode(converted, width, height, gray ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb, expectedOptions);
        AssertCoefficients(Read(expected, options), Read(actual, options), 1);
        Assert.False(actual.SequenceEqual(Encode(pixels, width, height, layout,
            options with { SourceIccProfile = default, IccProfile = expectedOptions.IccProfile }, stride)));
        Assert.Equal(expectedOptions.IccProfile.ToArray(), ReassembleIcc(actual));
        Assert.Equal(before, pixels);
        ImageResult decoded = ImageResult.FromMemory(actual, gray ? ColorComponents.Grey : ColorComponents.RedGreenBlue);
        Assert.Equal((width, height), (decoded.Width, decoded.Height));
        Assert.Equal(actual, Encode(pixels, width, height, layout, options with { MaxDegreeOfParallelism = 3 }, stride));
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, layout, options, stride, Cancellation);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.YCbCr)]
    public void ExplicitDestinationProfileDescribesTheConvertedSamplesAndIsPreservedExactly(JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17;
        float[] pixels = (float[])Source(2, width, height, PixelComponentLayout.Rgb, width * 3);
        byte[] destinationProfile = JpegliIccProfiles.CreateGammaSrgbProfile(2.2);
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(),
            IccProfile = Sliced(destinationProfile), ColorSpace = colorSpace,
            Subsampling = ChromaSubsampling.None, AdaptiveQuantization = false
        };
        float[] converted = pixels.Select(value => (float)Math.Pow(value, 1 / 2.2)).ToArray();
        byte[] actual = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        byte[] expected = Encode(converted, width, height, PixelComponentLayout.Rgb, options with { SourceIccProfile = default });
        AssertCoefficients(Read(expected, options), Read(actual, options), 1);
        Assert.Equal(destinationProfile, ReassembleIcc(actual));
    }

    [Fact]
    public void XybColorManagementMatchesIndependentSrgbConversionAndAttachesMatchingProfile()
    {
        const int width = 19, height = 17;
        float[] pixels = (float[])Source(2, width, height, PixelComponentLayout.Rgb, width * 3);
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(), ColorSpace = JpegColorSpace.Xyb,
            SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        float[] converted = pixels.Select(value => (float)Srgb(value)).ToArray();
        byte[] actual = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        byte[] expected = Encode(converted, width, height, PixelComponentLayout.Rgb,
            options with { SourceIccProfile = default, IccProfile = JpegliIccProfiles.CreateXybProfile() });
        AssertCoefficients(Read(expected, options), Read(actual, options), 1);
        Assert.Equal(JpegliIccProfiles.CreateXybProfile(), ReassembleIcc(actual));
        Assert.Equal(actual, Encode(pixels, width, height, PixelComponentLayout.Rgb, options with { MaxDegreeOfParallelism = 3 }));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("signature")]
    [InlineData("declared size")]
    [InlineData("tag count")]
    [InlineData("tag offset")]
    [InlineData("tag size")]
    [InlineData("CMYK")]
    [InlineData("wrong channels")]
    public void InvalidSourceProfilesFailBeforeStreamOutput(string corruption)
    {
        byte[] profile = JpegliIccProfiles.CreateLinearSrgbProfile();
        switch (corruption)
        {
            case "short": profile = profile[..127]; break;
            case "signature": profile[36] = (byte)'X'; break;
            case "declared size": Write32(profile, 0, uint.MaxValue); break;
            case "tag count": Write32(profile, 128, uint.MaxValue); break;
            case "tag offset": Write32(profile, 136, uint.MaxValue); break;
            case "tag size": Write32(profile, 140, uint.MaxValue); break;
            case "CMYK": "CMYK"u8.CopyTo(profile.AsSpan(16)); break;
            case "wrong channels": profile = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(); break;
        }
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 1, 2, 3 },
            1, 1, PixelLayout.Rgb24, new() { SourceIccProfile = profile }, cancellationToken: Cancellation));
        Assert.Equal("SourceIccProfile", error.ParamName);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(PixelComponentLayout.YCbCr)]
    [InlineData(PixelComponentLayout.Xyb)]
    public void DirectComponentInputsRejectSourceProfileBeforeOutput(PixelComponentLayout layout)
    {
        using var output = new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, new float[] { 0.2f, 0.4f, 0.6f },
            1, 1, layout, new() { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                IccProfile = layout == PixelComponentLayout.Xyb ? JpegliIccProfiles.CreateXybProfile() : default },
            cancellationToken: Cancellation));
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(-0.001f)]
    [InlineData(1.001f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void ColorManagedConsumedFloatSamplesRemainNormalizedEvenWhenExtendedRangeIsAllowed(float invalid)
    {
        float[] pixels = Enumerable.Repeat(0.5f, 19 * 17 * 3).ToArray();
        pixels[^1] = invalid;
        using var output = new MemoryStream();
        var error = Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, pixels, 19, 17,
            PixelComponentLayout.Rgb, new() { SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(),
                AllowOutOfRangeFloatSamples = true }, cancellationToken: Cancellation));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void DestinationMismatchAndInvalidIntentFailBeforeOutput()
    {
        foreach (JpegliEncoderOptions options in new[]
        {
            new JpegliEncoderOptions { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                IccProfile = JpegliIccProfiles.CreateSrgbGrayscaleProfile(), ColorSpace = JpegColorSpace.Rgb },
            new JpegliEncoderOptions { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                IccProfile = JpegliIccProfiles.CreateSrgbProfile(), ColorSpace = JpegColorSpace.Grayscale },
            new JpegliEncoderOptions { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                IccProfile = JpegliIccProfiles.CreateSrgbProfile(), ColorSpace = JpegColorSpace.Xyb },
            new JpegliEncoderOptions { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                ColorRenderingIntent = (IccRenderingIntent)42 }
        })
        {
            using var output = new MemoryStream();
            Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 1, 2, 3 },
                1, 1, PixelLayout.Rgb24, options, cancellationToken: Cancellation));
            Assert.Equal(0, output.Length);
            Assert.True(output.CanWrite);
        }
    }

    [Fact]
    public void TransformConstructionAndEncodingHonorPreCancellation()
    {
        byte[] source = JpegliIccProfiles.CreateLinearSrgbProfile(), destination = JpegliIccProfiles.CreateSrgbProfile();
        var canceled = new CancellationToken(true);
        var transformError = Assert.Throws<OperationCanceledException>(() => IccColorTransform.Create(source, destination,
            false, false, IccRenderingIntent.RelativeColorimetric, canceled));
        Assert.Equal(canceled, transformError.CancellationToken);
        using var output = new MemoryStream();
        var encodeError = Assert.Throws<OperationCanceledException>(() => JpegliEncoder.Encode(output,
            new byte[] { 1, 2, 3 }, 1, 1, PixelLayout.Rgb24,
            new() { SourceIccProfile = source }, cancellationToken: canceled));
        Assert.Equal(canceled, encodeError.CancellationToken);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void EmptySourceProfilePreservesExistingBytesAndMetadataSemantics()
    {
        byte[] pixels = Pattern(19, 17, PixelLayout.Rgb24);
        byte[] opaque = [1, 2, 3, 4];
        var options = new JpegliEncoderOptions { IccProfile = opaque, Progressive = true, ProgressiveRefinementBits = 2 };
        byte[] reference = JpegliEncoder.Encode(pixels, 19, 17, PixelLayout.Rgb24, options, cancellationToken: Cancellation);
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
            Assert.Equal(reference, JpegliEncoder.Encode(pixels, 19, 17, PixelLayout.Rgb24,
                options with { SourceIccProfile = ReadOnlyMemory<byte>.Empty, ColorRenderingIntent = intent }, cancellationToken: Cancellation));
        Assert.Equal(opaque, ReassembleIcc(reference));
    }

    [Fact]
    public void ProfileMutationOnFirstWriteCannotChangeConvertedSamplesOrMetadata()
    {
        byte[] source = JpegliIccProfiles.CreateLinearSrgbProfile(), destinationProfile = JpegliIccProfiles.CreateGammaSrgbProfile(2.2);
        byte[] originalDestination = destinationProfile.ToArray();
        byte[] pixels = Pattern(19, 17, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = source, IccProfile = destinationProfile, Progressive = true,
            ProgressiveRefinementBits = 2, MaxDegreeOfParallelism = 3
        };
        byte[] expected = Encode(pixels, 19, 17, PixelComponentLayout.Rgb, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            Array.Fill(source, (byte)0x61);
            Array.Fill(destinationProfile, (byte)0x92);
        });
        Encode(output, pixels, 19, 17, PixelComponentLayout.Rgb, options, 0, Cancellation);
        Assert.Equal(expected, destination.ToArray());
        Assert.Equal(originalDestination, ReassembleIcc(destination.ToArray()));
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData("mft1", 2)]
    [InlineData("mft2", 2)]
    [InlineData("mft2", 4)]
    [InlineData("mAB ", 4)]
    public void SourceLabLutsUseTheirSpecifiedPcsEncoding(string kind, int version)
    {
        byte[] profile = LutProfile(kind, false, version);
        var transform = Transform(profile, JpegliIccProfiles.CreateSrgbProfile());
        // Lab neutral L*=50, D50 -> XYZ Y=((50+16)/116)^3. lut8
        // quantizes L to 128/255; lut16 remains legacy even in v4.
        double l = kind == "mft1" ? 128 / 255d * 100 : kind == "mAB " ? 32768 / 65535d * 100 : 50;
        double expected = Srgb(Math.Pow((l + 16) / 116, 3));
        foreach ((float r, float g, float b) in new[] { (0f, 0f, 0f), (0.2f, 0.4f, 0.7f), (1f, 1f, 1f) })
            AssertConversion(transform, r, g, b, expected, expected, expected, 0.0003);
    }

    [Theory]
    [InlineData("mft1", 2)]
    [InlineData("mft2", 2)]
    [InlineData("mft2", 4)]
    [InlineData("mBA ", 4)]
    public void DestinationLutsActuallyApplyTheBToADeviceTransform(string kind, int version)
    {
        byte[] profile = LutProfile(kind, true, version);
        var transform = Transform(JpegliIccProfiles.CreateSrgbProfile(), profile);
        foreach ((float r, float g, float b) in new[] { (0f, 0f, 0f), (0.2f, 0.4f, 0.7f), (1f, 1f, 1f) })
            AssertConversion(transform, r, g, b, 51 / 255d, 102 / 255d, 179 / 255d, 0.00003);
    }

    [Fact]
    public void MAbFullPipelineAppliesAToClutToMToMatrixToBWithXyzScaling()
    {
        byte[] profile = MakeProfile(4, new[] { ("A2B0", OrderedLut(false)) });
        "XYZ "u8.CopyTo(profile.AsSpan(20));
        var transform = Transform(profile, JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), destinationGray: true);
        const float r = 0.6f, g = 0.3f, b = 0.2f;
        // A squares all device channels, CLUT swaps X/Y, M squares, matrix
        // halves and adds 1/8, B squares, then PCSXYZ words map to 0..1.99997.
        double expected = Math.Pow(0.5 * Math.Pow(r, 4) + 0.125, 2) * (65535d / 32768);
        AssertConversion(transform, r, g, b, expected, expected, expected, 0.00001);
    }

    [Fact]
    public void MBaFullPipelineAppliesBToMatrixToMToClutToAWithXyzScaling()
    {
        byte[] profile = MakeProfile(4, new[] { ("B2A0", OrderedLut(true)) });
        "XYZ "u8.CopyTo(profile.AsSpan(20));
        var transform = Transform(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(), profile, sourceGray: true);
        const float gray = 0.4f;
        double scale = 65535d / 32768;
        double x = 0.9642 * gray / scale, y = gray / scale, z = 0.8249 * gray / scale;
        // B squares PCS coordinates, matrix halves and adds 1/8, M squares,
        // CLUT swaps X/Y, A squares the device channels.
        AssertConversion(transform, gray, gray, gray,
            Math.Pow(0.5 * y * y + 0.125, 4), Math.Pow(0.5 * x * x + 0.125, 4),
            Math.Pow(0.5 * z * z + 0.125, 4), 0.000002);
    }

    [Theory]
    [InlineData(IccRenderingIntent.Perceptual, 25)]
    [InlineData(IccRenderingIntent.RelativeColorimetric, 50)]
    [InlineData(IccRenderingIntent.Saturation, 75)]
    [InlineData(IccRenderingIntent.AbsoluteColorimetric, 50)]
    public void RenderingIntentSelectsTheCorrespondingLutAndColorimetricFallback(IccRenderingIntent intent, int l)
    {
        byte[] profile = MakeProfile(4, new[]
        {
            ("A2B0", Lut("mft2", false, 25)),
            ("A2B1", Lut("mft2", false, 50)),
            ("A2B2", Lut("mft2", false, 75))
        });
        double expected = Srgb(Math.Pow((l + 16) / 116d, 3));
        AssertConversion(Transform(profile, JpegliIccProfiles.CreateSrgbProfile(), intent: intent),
            0.2f, 0.4f, 0.7f, expected, expected, expected, 0.0003);
        byte[] fallback = LutProfile("mft2", false, 4);
        expected = Srgb(Math.Pow(66 / 116d, 3));
        AssertConversion(Transform(fallback, JpegliIccProfiles.CreateSrgbProfile(), intent: intent),
            0.2f, 0.4f, 0.7f, expected, expected, expected, 0.0003);
    }

    [Theory]
    [InlineData("mft1", "grid")]
    [InlineData("mft2", "grid")]
    [InlineData("mft2", "table")]
    [InlineData("mAB ", "offset")]
    [InlineData("mAB ", "precision")]
    [InlineData("mAB ", "curve")]
    [InlineData("mAB ", "grid")]
    public void MalformedLutExtentsAndStagesFailBeforeStreamOutput(string kind, string corruption)
    {
        byte[] profile = LutProfile(kind, false, kind == "mft1" ? 2 : 4);
        int offset = Tag(profile, "A2B0"u8);
        switch (corruption)
        {
            case "grid": profile[offset + (kind == "mAB " ? 104 : 10)] = 255; break;
            case "table": BinaryPrimitives.WriteUInt16BigEndian(profile.AsSpan(offset + 48, 2), ushort.MaxValue); break;
            case "offset": Write32(profile, offset + 24, uint.MaxValue); break;
            case "precision": profile[offset + 104 + 16] = 3; break;
            case "curve": Write32(profile, offset + 32 + 8, uint.MaxValue); break;
        }
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 1, 2, 3 },
            1, 1, PixelLayout.Rgb24, new() { SourceIccProfile = profile }, cancellationToken: Cancellation));
        Assert.Equal("SourceIccProfile", error.ParamName);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void UndefinedLut8XyzEncodingIsRejectedBeforeOutput()
    {
        byte[] profile = LutProfile("mft1", false, 2);
        "XYZ "u8.CopyTo(profile.AsSpan(20));
        using var output = new MemoryStream();
        Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 1, 2, 3 }, 1, 1,
            PixelLayout.Rgb24, new() { SourceIccProfile = profile }, cancellationToken: Cancellation));
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BuiltInHdrCompatibilityLutCannotServeAsDestinationInverse(bool pq)
    {
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 1, 2, 3 }, 1, 1,
            PixelLayout.Rgb24, new() { SourceIccProfile = JpegliIccProfiles.CreateSrgbProfile(),
                IccProfile = pq ? JpegliIccProfiles.CreateRec2100PqProfile() : JpegliIccProfiles.CreateRec2100HlgProfile() },
            cancellationToken: Cancellation));
        Assert.Equal("IccProfile", error.ParamName);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void FiniteCurveParametersCannotHideNonfiniteIntermediateSamples()
    {
        byte[] profile = JpegliIccProfiles.CreateSrgbProfile();
        int curve = Tag(profile, "rTRC"u8);
        // Finite s15Fixed16 parameters in a type-3 curve can overflow pow.
        // Clamping infinity to one would silently turn invalid color into white.
        Assert.True(profile.AsSpan(curve, 4).SequenceEqual("para"u8));
        BinaryPrimitives.WriteUInt16BigEndian(profile.AsSpan(curve + 8, 2), 3);
        Write32(profile, curve + 12, 0x7fff0000); // g
        Write32(profile, curve + 16, 0x7fff0000); // a
        Write32(profile, curve + 20, 0); // b
        Write32(profile, curve + 24, 0); // c
        Write32(profile, curve + 28, 0); // d
        using var output = new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[] { 128, 128, 128 }, 1, 1,
            PixelLayout.Rgb24, new() { SourceIccProfile = profile }, cancellationToken: Cancellation));
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    private static byte[] LutProfile(string kind, bool reverse, int version) =>
        MakeProfile(version, new[] { (reverse ? "B2A0" : "A2B0", Lut(kind, reverse, 50)) });

    private static byte[] MakeProfile(int version, (string Signature, byte[] Data)[] tags)
    {
        int tagCount = tags.Length + 1, start = 132 + tagCount * 12;
        byte[] profile = new byte[start + 20 + tags.Sum(tag => (tag.Data.Length + 3) & ~3)];
        Write32(profile, 0, (uint)profile.Length);
        Write32(profile, 8, (uint)version << 24);
        "mntr"u8.CopyTo(profile.AsSpan(12));
        "RGB "u8.CopyTo(profile.AsSpan(16));
        "Lab "u8.CopyTo(profile.AsSpan(20));
        "acsp"u8.CopyTo(profile.AsSpan(36));
        Write32(profile, 68, 0x0000f6d6);
        Write32(profile, 72, 0x00010000);
        Write32(profile, 76, 0x0000d32d);
        Write32(profile, 128, (uint)tagCount);
        "wtpt"u8.CopyTo(profile.AsSpan(132));
        Write32(profile, 136, (uint)start);
        Write32(profile, 140, 20);
        "XYZ "u8.CopyTo(profile.AsSpan(start));
        profile.AsSpan(68, 12).CopyTo(profile.AsSpan(start + 8));
        start += 20;
        for (int index = 0; index < tags.Length; index++)
        {
            int entry = 144 + index * 12;
            System.Text.Encoding.ASCII.GetBytes(tags[index].Signature).CopyTo(profile, entry);
            Write32(profile, entry + 4, (uint)start);
            Write32(profile, entry + 8, (uint)tags[index].Data.Length);
            tags[index].Data.CopyTo(profile, start);
            start += (tags[index].Data.Length + 3) & ~3;
        }
        return profile;
    }

    private static byte[] Lut(string kind, bool reverse, int l)
    {
        bool bytePrecision = kind == "mft1", multiStage = kind is "mAB " or "mBA ";
        int precision = bytePrecision ? 1 : 2;
        int[] constant = reverse ? [51 * 257, 102 * 257, 179 * 257] :
            multiStage ? [(int)Math.Round(l / 100d * 65535), 128 * 257, 128 * 257] :
            bytePrecision ? [(int)Math.Round(l / 100d * 255) * 257, 128 * 257, 128 * 257] :
            [l * 65280 / 100, 32768, 32768];
        int clut, outputTables = 0;
        byte[] data;
        if (multiStage)
        {
            data = new byte[104 + 20 + 8 * 3 * 2];
            data[8] = data[9] = 3;
            Write32(data, 12, 32); // B curves.
            Write32(data, 24, 104); // CLUT.
            Write32(data, 28, 68); // A curves.
            for (int index = 0; index < 6; index++)
                "curv"u8.CopyTo(data.AsSpan(32 + index * 12)); // count=0 identity.
            data[104] = data[105] = data[106] = 2;
            data[120] = 2;
            clut = 124;
        }
        else
        {
            int tableEntries = bytePrecision ? 256 : 2;
            int header = bytePrecision ? 48 : 52;
            clut = header + 3 * tableEntries * precision;
            outputTables = clut + 8 * 3 * precision;
            data = new byte[outputTables + 3 * tableEntries * precision];
            data[8] = data[9] = 3;
            data[10] = 2;
            for (int index = 0; index < 3; index++)
                Write32(data, 12 + index * 16, 65536);
            if (!bytePrecision)
            {
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(48, 2), 2);
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(50, 2), 2);
            }
            for (int channel = 0; channel < 3; channel++)
                for (int index = 0; index < tableEntries; index++)
                {
                    int value = bytePrecision ? index * 257 : index * 65535;
                    WriteLutValue(data, header + (channel * tableEntries + index) * precision, value, precision);
                    WriteLutValue(data, outputTables + (channel * tableEntries + index) * precision, value, precision);
                }
        }
        System.Text.Encoding.ASCII.GetBytes(kind).CopyTo(data, 0);
        for (int index = 0; index < 8 * 3; index++)
            WriteLutValue(data, clut + index * precision, constant[index % 3], precision);
        return data;
    }

    private static void WriteLutValue(byte[] data, int offset, int value, int precision)
    {
        if (precision == 1)
            data[offset] = (byte)(value / 257);
        else
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset, 2), (ushort)value);
    }

    private static byte[] OrderedLut(bool reverse)
    {
        const int bCurves = 32, matrix = 80, mCurves = 128, clut = 176, aCurves = 244;
        byte[] data = new byte[292];
        (reverse ? "mBA "u8 : "mAB "u8).CopyTo(data);
        data[8] = data[9] = 3;
        Write32(data, 12, bCurves);
        Write32(data, 16, matrix);
        Write32(data, 20, mCurves);
        Write32(data, 24, clut);
        Write32(data, 28, aCurves);
        foreach (int curves in new[] { bCurves, mCurves, aCurves })
            for (int channel = 0; channel < 3; channel++)
            {
                "para"u8.CopyTo(data.AsSpan(curves + channel * 16));
                Write32(data, curves + channel * 16 + 12, 2 * 65536);
            }
        for (int channel = 0; channel < 3; channel++)
        {
            Write32(data, matrix + channel * 16, 32768);
            Write32(data, matrix + 36 + channel * 4, 8192);
        }
        data[clut] = data[clut + 1] = data[clut + 2] = 2;
        data[clut + 16] = 2;
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    int offset = clut + 20 + (x * 4 + y * 2 + z) * 6;
                    WriteLutValue(data, offset, y * 65535, 2);
                    WriteLutValue(data, offset + 2, x * 65535, 2);
                    WriteLutValue(data, offset + 4, z * 65535, 2);
                }
        return data;
    }

    private static IccColorTransform Transform(byte[] source, byte[] destination, bool sourceGray = false, bool destinationGray = false,
        IccRenderingIntent intent = IccRenderingIntent.RelativeColorimetric) =>
        IccColorTransform.Create(source, destination, sourceGray, destinationGray, intent, Cancellation);

    private static void AssertConversion(IccColorTransform transform, float r, float g, float b,
        double expectedR, double expectedG, double expectedB, double tolerance)
    {
        transform.Convert(r, g, b, out float actualR, out float actualG, out float actualB);
        Assert.InRange(actualR, expectedR - tolerance, expectedR + tolerance);
        Assert.InRange(actualG, expectedG - tolerance, expectedG + tolerance);
        Assert.InRange(actualB, expectedB - tolerance, expectedB + tolerance);
    }

    private static double Srgb(double linear) => linear <= 0.0031308 ? 12.92 * linear : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;
    private static double Linear(double encoded) => encoded <= 0.04045 ? encoded / 12.92 : Math.Pow((encoded + 0.055) / 1.055, 2.4);

    private static double ColorantY(byte[] profile, ReadOnlySpan<byte> signature)
    {
        int offset = Tag(profile, signature);
        Assert.True(profile.AsSpan(offset, 4).SequenceEqual("XYZ "u8));
        return BinaryPrimitives.ReadInt32BigEndian(profile.AsSpan(offset + 12, 4)) / 65536d;
    }

    private static int Tag(byte[] profile, ReadOnlySpan<byte> signature)
    {
        int count = checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(128, 4)));
        for (int index = 0; index < count; index++)
        {
            int entry = 132 + index * 12;
            if (profile.AsSpan(entry, 4).SequenceEqual(signature))
                return checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(entry + 4, 4)));
        }
        throw new InvalidOperationException("Missing fixture tag.");
    }

    private static void Write32(byte[] buffer, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(offset, 4), value);

    private static ReadOnlyMemory<byte> Sliced(byte[] profile)
    {
        byte[] backing = Enumerable.Repeat((byte)0x51, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        return backing.AsMemory(3, profile.Length);
    }

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Source(int precision, int width, int height, PixelComponentLayout layout, int stride)
    {
        int components = Components(layout);
        int length = (height - 1) * stride + width * components + 13;
        Array pixels = precision switch
        {
            0 => Enumerable.Repeat((byte)239, length).ToArray(),
            1 => Enumerable.Repeat((ushort)61453, length).ToArray(),
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
                for (int channel = 0; channel < (components == 1 ? 1 : 3); channel++)
                {
                    int targetChannel = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra ? 2 - channel : channel;
                    int offset = row * stride + column * components + targetChannel;
                    int value = 15 + (11 * column + 19 * row + 37 * channel) % 225;
                    if (pixels is byte[] bytes)
                        bytes[offset] = (byte)value;
                    else if (pixels is ushort[] words)
                        words[offset] = (ushort)(value * 257 + (column + row + channel) % 113);
                    else
                        ((float[])pixels)[offset] = (value + (column + row + channel) % 113 / 257f) / 255f;
                }
        return pixels;
    }

    private static float[] IndependentlyConvert(Array pixels, int width, int height, PixelComponentLayout layout, int stride)
    {
        int components = Components(layout), outputComponents = components == 1 ? 1 : 3;
        float[] converted = new float[width * height * outputComponents];
        for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
                for (int channel = 0; channel < outputComponents; channel++)
                {
                    int sourceChannel = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra ? 2 - channel : channel;
                    int offset = row * stride + column * components + sourceChannel;
                    double sample = pixels switch
                    {
                        byte[] bytes => bytes[offset] / 255d,
                        ushort[] words => words[offset] / 65535d,
                        _ => ((float[])pixels)[offset]
                    };
                    converted[(row * width + column) * outputComponents + channel] = (float)Srgb(sample);
                }
        return converted;
    }

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout, JpegliEncoderOptions options, int stride = 0)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, layout, options, stride, Cancellation);
        return output.ToArray();
    }

    private static void Encode(Stream output, Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride, CancellationToken cancellation)
    {
        if (pixels is byte[] bytes)
        {
            PixelLayout byteLayout = layout switch
            {
                PixelComponentLayout.Gray => PixelLayout.Gray8,
                PixelComponentLayout.Bgr => PixelLayout.Bgr24,
                PixelComponentLayout.Rgba => PixelLayout.Rgba32,
                PixelComponentLayout.Bgra => PixelLayout.Bgra32,
                _ => PixelLayout.Rgb24
            };
            JpegliEncoder.Encode(output, bytes, width, height, byteLayout, options, stride, cancellation);
        }
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, cancellation);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, cancellation);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual, int tolerance)
    {
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((int selector, byte[] quantizer) in expected.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        foreach ((BlockKey key, Block block) in expected.Blocks)
        {
            Assert.Equal(block.IsDummy, actual.Blocks[key].IsDummy);
            for (int index = 0; index < 64; index++)
                Assert.InRange(actual.Blocks[key].Coefficients[index], block.Coefficients[index] - tolerance, block.Coefficients[index] + tolerance);
        }
    }

    private static byte[] ReassembleIcc(byte[] jpeg)
    {
        using var profile = new MemoryStream();
        int expectedSequence = 1, declaredCount = 0;
        for (int position = 2; position < jpeg.Length;)
        {
            Assert.Equal(0xff, jpeg[position]);
            if (jpeg[position + 1] == 0xda)
                break;
            int length = (jpeg[position + 2] << 8) | jpeg[position + 3];
            if (jpeg[position + 1] == 0xe2)
            {
                ReadOnlySpan<byte> payload = jpeg.AsSpan(position + 4, length - 2);
                Assert.True(payload[..12].SequenceEqual("ICC_PROFILE\0"u8));
                Assert.Equal(expectedSequence++, payload[12]);
                if (declaredCount == 0)
                    declaredCount = payload[13];
                Assert.Equal(declaredCount, payload[13]);
                profile.Write(payload[14..]);
            }
            position += length + 2;
        }
        Assert.Equal(declaredCount, expectedSequence - 1);
        return profile.ToArray();
    }
}
