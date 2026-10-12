using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderRgbToXybTests
{
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];
    private static readonly int[] ComponentIds = ['R', 'G', 'B'];
    private static readonly JpegSamplingFactors FullResolution = new(new(1, 1), new(1, 1), new(1, 1));

    public static TheoryData<int, PixelComponentLayout> SourceModes
    {
        get
        {
            var modes = new TheoryData<int, PixelComponentLayout>();
            for (int kind = 0; kind < 3; kind++)
                foreach (PixelComponentLayout layout in new[]
                    { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba, PixelComponentLayout.Bgra, PixelComponentLayout.Gray })
                    modes.Add(kind, layout);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(SourceModes))]
    public void ExplicitXybTransformsAllRgbLayoutsAndGrayscaleBeforeSmoothingAndSampling(int kind, PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        Array rgb = RgbSource(kind, width, height);
        Array source = Arrange(rgb, kind, width, height, layout);
        var options = Options() with
        {
            Distance = 0.375f, AdaptiveQuantization = false, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] actual = Encode(source, width, height, layout, options);
        // Transform rounding can put a channel just outside 0..1 (for example,
        // white Y on Linux). Preserve it in the direct-XYB comparison, including
        // coefficient preflight; the original sRGB source remains strictly validated.
        float[] scaled = ScaledXyb(rgb, width, height, layout == PixelComponentLayout.Gray);
        AssertNormalizedTransformRange(scaled);
        byte[] expected = Encode(scaled,
            width, height, PixelComponentLayout.Xyb, options with { AllowOutOfRangeFloatSamples = true });
        AssertCoefficients(Read(expected, options), Read(actual, options), tolerance: 1);

        // Element strides, trailing storage and alpha must never enter the transform.
        int stride = width * Components(layout) + 13;
        Array padded = Arrange(rgb, kind, width, height, layout, stride);
        Assert.Equal(actual, Encode(padded, width, height, layout, options, stride));
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, padded, width, height, layout, options, stride);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(PixelComponentLayout.Rgb)]
    [InlineData(PixelComponentLayout.Bgr)]
    [InlineData(PixelComponentLayout.Rgba)]
    [InlineData(PixelComponentLayout.Bgra)]
    [InlineData(PixelComponentLayout.Gray)]
    public void ByteExpansionPreservesConvertedEncoding(PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        byte[] source = (byte[])Arrange(RgbSource(0, width, height), 0, width, height, layout);
        ushort[] expanded = source.Select(value => (ushort)(value * 257)).ToArray();
        var options = Options() with { Progressive = true, ProgressiveRefinementBits = 2, SmoothingFactor = 37, RestartInterval = 2 };
        Assert.Equal(Encode(source, width, height, layout, options), Encode(expanded, width, height, layout, options));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CustomSamplingSmoothingAndAdaptiveAnalysisMatchDirectScaledXyb(bool adaptive)
    {
        const int width = 33, height = 19;
        Array rgb = RgbSource(2, width, height);
        float[] scaled = ScaledXyb(rgb, width, height, grayscale: false);
        AssertNormalizedTransformRange(scaled);
        foreach (JpegSamplingFactors? factors in new JpegSamplingFactors?[]
            { null, FullResolution, new(new(1, 2), new(2, 1), new(1, 1)), new(new(2, 1), new(1, 1), new(1, 1)) })
            foreach (int smoothing in new[] { 0, 37 })
            {
                var options = Options() with
                {
                    Distance = 0.375f, AdaptiveQuantization = adaptive, SamplingFactors = factors,
                    SmoothingFactor = smoothing, OptimizeHuffmanTables = false, RestartInterval = 2
                };
                AssertCoefficients(Read(Encode(scaled, width, height, PixelComponentLayout.Xyb,
                    options with { AllowOutOfRangeFloatSamples = true }), options),
                    Read(Encode(rgb, width, height, PixelComponentLayout.Rgb, options), options), tolerance: 1);
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProgressiveRefinementHuffmanAndRestartModesPreserveConvertedCoefficients(bool adaptive)
    {
        const int width = 65, height = 17;
        Array pixels = RgbSource(2, width, height);
        foreach (JpegSamplingFactors? factors in new JpegSamplingFactors?[] { null, new(new(1, 2), new(2, 1), new(1, 1)) })
        {
            var options = Options() with
            {
                AdaptiveQuantization = adaptive, SamplingFactors = factors,
                SmoothingFactor = 37, OptimizeHuffmanTables = false
            };
            JpegDocument baseline = Read(Encode(pixels, width, height, PixelComponentLayout.Rgb, options), options);
            foreach ((bool progressive, int bits, bool optimize, int interval) in new[]
                { (false, 0, true, 2), (false, 0, false, 1), (true, 0, false, 2), (true, 2, true, 1), (true, 10, false, 2) })
            {
                var variant = options with
                {
                    Progressive = progressive, ProgressiveRefinementBits = bits,
                    OptimizeHuffmanTables = optimize, RestartInterval = interval
                };
                AssertCoefficients(baseline, Read(Encode(pixels, width, height, PixelComponentLayout.Rgb, variant), variant));
            }
        }
    }

    [Theory]
    [MemberData(nameof(SourceModes))]
    public void AutomaticDefaultsRemainUnchangedAndExplicitXybUsesItsSamplingTablesAndOpaqueProfile(int kind, PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        Array source = Arrange(RgbSource(kind, width, height), kind, width, height, layout);
        var options = Options();
        JpegColorSpace automatic = layout == PixelComponentLayout.Gray ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr;
        Assert.Equal(Encode(source, width, height, layout, options with { ColorSpace = automatic }),
            Encode(source, width, height, layout, options with { ColorSpace = JpegColorSpace.Automatic }));

        byte[] jpeg = Encode(source, width, height, layout, options);
        foreach (ChromaSubsampling preset in Enum.GetValues<ChromaSubsampling>())
            Assert.Equal(jpeg, Encode(source, width, height, layout, options with { Subsampling = preset }));
        JpegDocument document = Read(jpeg, options);
        Assert.Equal(ComponentIds, document.Components.Keys);
        Assert.Equal(new Component(2, 2, 0), document.Components['R']);
        Assert.Equal(new Component(2, 2, 1), document.Components['G']);
        Assert.Equal(new Component(1, 1, 2), document.Components['B']);
        Assert.Equal(new[] { 0, 1, 2 }, document.Quantizers.Keys);
        Dictionary<int, List<byte[]>> headers = HeaderPayloads(jpeg);
        Assert.False(headers.ContainsKey(0xe0));
        Assert.False(headers.ContainsKey(0xee));
        byte[] profile = Assert.Single(headers[0xe2]);
        Assert.True(profile.AsSpan(0, 12).SequenceEqual("ICC_PROFILE\0"u8));
        Assert.Equal(new byte[] { 1, 1 }, profile[12..14]);
        Assert.Equal(OpaqueProfile, profile[14..]);
    }

    [Theory]
    [MemberData(nameof(SourceModes))]
    public void MissingXybProfileRejectsBeforeWritingForEveryConvertedSource(int kind, PixelComponentLayout layout)
    {
        Array source = Arrange(RgbSource(kind, 8, 8), kind, 8, 8, layout);
        AssertRejected(source, layout, Options() with { IccProfile = ReadOnlyMemory<byte>.Empty }, "IccProfile");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DirectYcbcrToXybAndScaledXybToOtherSpacesStillRejectBeforeWriting(int kind)
    {
        Array source = RgbSource(kind, 8, 8);
        AssertRejected(source, PixelComponentLayout.YCbCr, Options(), "ColorSpace");
        foreach (JpegColorSpace output in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb })
            AssertRejected(source, PixelComponentLayout.Xyb, Options() with { ColorSpace = output }, "ColorSpace");
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MaxValue / 512f)]
    [InlineData(-float.MaxValue / 512f)]
    [InlineData(-0.01f)]
    [InlineData(1.01f)]
    [InlineData(1.00000011920928955078125f)] // Next float above 1: still invalid source input.
    public void InvalidConsumedFloatsRejectBeforeHeadersEvenWhenLastPixelIsInvalid(float invalid)
    {
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba, PixelComponentLayout.Bgra, PixelComponentLayout.Gray })
        {
            int components = Components(layout);
            float[] source = Enumerable.Repeat(0.5f, 64 * components).ToArray();
            for (int channel = 0; channel < (layout == PixelComponentLayout.Gray ? 1 : 3); channel++)
            {
                Array.Fill(source, 0.5f);
                source[63 * components + channel] = invalid;
                AssertRejected(source, layout, Options(), "pixels");
                // The smaller finite maximum scales to JPEG units without overflow,
                // but nonlinear sRGB conversion must still reject it before output.
                if (!float.IsFinite(invalid) || Math.Abs(invalid) > 1e10f)
                    AssertRejected(source, layout, Options() with { AllowOutOfRangeFloatSamples = true }, "pixels");
            }
        }
    }

    [Theory]
    [InlineData(1e20f)]
    [InlineData(-1e20f)]
    public void NonlinearOverflowRejectsAllChannelInputsBeforeAbsorbanceClamping(float value)
    {
        float[] source = Enumerable.Repeat(value, 64 * 3).ToArray();
        foreach ((bool progressive, int bits, bool optimize) in new[] { (false, 0, false), (true, 10, true) })
            AssertRejected(source, PixelComponentLayout.Rgb, Options() with
            {
                AllowOutOfRangeFloatSamples = true, Progressive = progressive,
                ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
            }, "pixels");
    }

    [Fact]
    public void PermittedOutsideSrgbSamplesTransformWithoutClippingAndPreflightUnencodableCoefficients()
    {
        const int width = 8, height = 8;
        var source = new float[width * height * 3];
        for (int pixel = 0; pixel < width * height; pixel++)
        {
            source[pixel * 3] = -0.002f;
            source[pixel * 3 + 1] = 1.002f;
            source[pixel * 3 + 2] = 0.50037f;
        }
        var options = Options() with
        {
            Distance = 0.375f, SamplingFactors = FullResolution, AdaptiveQuantization = false,
            AllowOutOfRangeFloatSamples = true, Progressive = true, ProgressiveRefinementBits = 2
        };
        JpegDocument actual = Read(Encode(source, width, height, PixelComponentLayout.Rgb, options), options);
        float[] scaled = ScaledXyb(source, width, height, grayscale: false);
        AssertCoefficients(Read(Encode(scaled, width, height, PixelComponentLayout.Xyb, options), options), actual, tolerance: 1);
        float[] clipped = source.Select(value => Math.Clamp(value, 0f, 1f)).ToArray();
        JpegDocument clippedDocument = Read(Encode(clipped, width, height, PixelComponentLayout.Rgb, options), options);
        Assert.Contains(actual.Blocks, pair => !pair.Value.Coefficients.SequenceEqual(clippedDocument.Blocks[pair.Key].Coefficients));

        Array.Fill(source, 20f);
        foreach ((bool progressive, int bits, bool optimize) in new[] { (false, 0, false), (false, 0, true), (true, 2, false), (true, 10, true) })
            AssertRejected(source, PixelComponentLayout.Rgb,
                options with { Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize }, "pixels");
    }

    private static JpegliEncoderOptions Options() => new() { ColorSpace = JpegColorSpace.Xyb, IccProfile = OpaqueProfile };

    private static Array RgbSource(int kind, int width, int height)
    {
        var bytes = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < 3; channel++)
                    bytes[(y * width + x) * 3 + channel] = (byte)((19 * x + 37 * y + 71 * channel + 11 * x * y) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + index % 47 - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
    }

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Arrange(Array rgb, int kind, int width, int height, PixelComponentLayout layout, int stride = 0)
    {
        int components = Components(layout);
        if (stride == 0)
            stride = width * components;
        int length = (height - 1) * stride + width * components + 7;
        Array result = kind switch
        {
            0 => Enumerable.Repeat((byte)0xdd, length).ToArray(),
            1 => Enumerable.Repeat((ushort)0xdddd, length).ToArray(),
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < Math.Min(components, 3); channel++)
                {
                    int rgbChannel = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra ? 2 - channel : channel;
                    result.SetValue(rgb.GetValue((y * width + x) * 3 + rgbChannel), y * stride + x * components + channel);
                }
        return result;
    }

    private static float SrgbSample(Array source, int offset) => source switch
    {
        byte[] bytes => (float)(bytes[offset] * (1.0 / 255.0)),
        ushort[] words => (float)(words[offset] * (1.0 / 65535.0)),
        _ => ((float[])source)[offset]
    };

    private static float[] ScaledXyb(Array rgb, int width, int height, bool grayscale)
    {
        var result = new float[width * height * 3];
        for (int offset = 0; offset < result.Length; offset += 3)
        {
            float r = SrgbSample(rgb, offset), g = grayscale ? r : SrgbSample(rgb, offset + 1), b = grayscale ? r : SrgbSample(rgb, offset + 2);
            XybColorConversion.SrgbToScaledXyb(r, g, b, out float x, out float y, out float bMinusY);
            result[offset] = x / 255f;
            result[offset + 1] = y / 255f;
            result[offset + 2] = bMinusY / 255f;
        }
        return result;
    }

    private static void AssertNormalizedTransformRange(float[] scaled)
    {
        // Only rounding-scale excursions are expected from normalized sRGB.
        // Keep the same sub-sample tolerance as the independent native fixtures.
        const float tolerance = 0.0025f / 255f;
        Assert.All(scaled, value => Assert.InRange(value, -tolerance, 1f + tolerance));
    }

    private static byte[] Encode(Array source, int width, int height, PixelComponentLayout layout, JpegliEncoderOptions options, int stride = 0) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)(int)layout, options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, layout, options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array source, int width, int height, PixelComponentLayout layout, JpegliEncoderOptions options, int stride = 0)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout, options, stride, TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, layout, options, stride, TestContext.Current.CancellationToken);
    }

    private static void AssertRejected(Array source, PixelComponentLayout layout, JpegliEncoderOptions options, string parameter)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        destination.Write([0x12, 0x34]);
        ArgumentException error = Assert.ThrowsAny<ArgumentException>(() => Encode(output, source, 8, 8, layout, options));
        Assert.Equal(parameter, error.ParamName);
        Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual, int tolerance = 0)
    {
        // Direct normalized XYB input divides and re-multiplies the converted
        // samples by 255; allow one coefficient only for that float round-trip.
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((int selector, byte[] quantizer) in expected.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        foreach ((BlockKey key, Block block) in expected.Blocks)
        {
            Block other = actual.Blocks[key];
            Assert.Equal(block.IsDummy, other.IsDummy);
            if (block.IsDummy)
                Assert.All(other.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
            else
                for (int frequency = 0; frequency < 64; frequency++)
                    Assert.InRange(Math.Abs(block.Coefficients[frequency] - other.Coefficients[frequency]), 0, tolerance);
        }
    }

    private static Dictionary<int, List<byte[]>> HeaderPayloads(byte[] jpeg)
    {
        var result = new Dictionary<int, List<byte[]>>();
        int offset = 2;
        while (jpeg[offset + 1] != 0xda)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int marker = jpeg[offset + 1], length = jpeg[offset + 2] * 256 + jpeg[offset + 3];
            if (!result.TryGetValue(marker, out List<byte[]>? payloads))
                result.Add(marker, payloads = []);
            payloads.Add(jpeg[(offset + 4)..(offset + 2 + length)]);
            offset += 2 + length;
        }
        return result;
    }
}
