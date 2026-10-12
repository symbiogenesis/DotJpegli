using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderStandardQuantizationTests
{
    // ITU-T T.81 Annex K, Tables K.1 and K.2, in natural frequency order.
    // These are independent of the encoder's constants and table construction.
    private static ReadOnlySpan<byte> Luminance =>
    [
        16,11,10,16,24,40,51,61,12,12,14,19,26,58,60,55,
        14,13,16,24,40,57,69,56,14,17,22,29,51,87,80,62,
        18,22,37,56,68,109,103,77,24,35,55,64,81,104,113,92,
        49,64,78,87,103,121,120,101,72,92,95,98,112,100,103,99
    ];

    private static ReadOnlySpan<byte> Chrominance =>
    [
        17,18,24,47,99,99,99,99,18,21,26,66,99,99,99,99,
        24,26,56,99,99,99,99,99,47,66,99,99,99,99,99,99,
        99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,
        99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99
    ];

    // Positions in the natural-order matrix, in wire DQT order.
    private static ReadOnlySpan<int> Zigzag =>
    [
        0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
        12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
        35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
        58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
    ];

    public static TheoryData<float> DirectDistances => new()
    {
        float.Epsilon, 0.001f, MathF.BitDecrement(0.1f), 0.1f,
        MathF.BitIncrement(0.1f), 0.125f, 0.375f, 1.45f,
        MathF.BitDecrement(4.6f), 4.6f, MathF.BitIncrement(4.6f),
        MathF.BitDecrement(6.4f), 6.4f, MathF.BitIncrement(6.4f),
        12.5f, MathF.BitDecrement(25f), 25f, MathF.BitIncrement(25f),
        30f, float.MaxValue
    };

    public static TheoryData<int, bool> SamplingModes
    {
        get
        {
            var result = new TheoryData<int, bool>();
            for (int mode = 0; mode < Enum.GetValues<ChromaSubsampling>().Length + 3; mode++)
                foreach (bool adaptive in new[] { false, true })
                    result.Add(mode, adaptive);
            return result;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void StandardTablesAreOptInAndPreserveExistingPerceptualOutput(int kind)
    {
        Assert.False(new JpegliEncoderOptions().UseStandardQuantizationTables);
        foreach (PixelComponentLayout layout in new[] { PixelComponentLayout.Rgb, PixelComponentLayout.Gray, PixelComponentLayout.YCbCr })
        {
            Array pixels = Source(kind, 19, 17, layout);
            var options = new JpegliEncoderOptions { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 };
            byte[] expected = EncodeSource(pixels, 19, 17, layout, options);
            Assert.Equal(expected, EncodeSource(pixels, 19, 17, layout,
                options with { UseStandardQuantizationTables = false }));
            JpegDocument document = Read(expected, options);
            Assert.Equal(layout == PixelComponentLayout.Gray ? new[] { 0 } : new[] { 0, 1, 2 },
                document.Components.Values.Select(component => component.Quantizer));
            Assert.False(expected.SequenceEqual(EncodeSource(pixels, 19, 17, layout,
                options with { UseStandardQuantizationTables = true })));
        }
    }

    [Fact]
    public void IntegerAndFractionalQualitiesEmitIndependentlyScaledAnnexKTables()
    {
        float[] qualities = Enumerable.Range(0, 101).Select(value => (float)value)
            .Concat([12.5f, 29.75f, MathF.BitDecrement(30f), MathF.BitIncrement(30f),
                50.125f, 85.25f, 99.5f, MathF.BitDecrement(100f)]).ToArray();
        byte[] pixels = Pattern(8, 8, PixelLayout.Rgb24);
        foreach (float quality in qualities)
        {
            var options = new JpegliEncoderOptions
            {
                UseStandardQuantizationTables = true, Quality = quality, AdaptiveQuantization = false,
                Subsampling = ChromaSubsampling.None
            };
            float distance = QualityDistance(quality);
            byte[] jpeg = EncodeSource(pixels, 8, 8, PixelComponentLayout.Rgb, options);
            AssertStandardTables(Read(jpeg, options), distance, grayscale: false);
            // The public distance override still wins over quality in this mode.
            Assert.Equal(jpeg, EncodeSource(pixels, 8, 8, PixelComponentLayout.Rgb,
                options with { Distance = distance, Quality = quality == 0f ? 100f : 0f }));
        }
    }

    [Fact]
    public void StandardTablesAndAdaptiveMultipliersMatchPinnedCppFixtures()
    {
        // Natural-order fixtures from the pinned quant.cc SetQuantMatrices,
        // QuantValsToDistance and InitQuantizer, compiled independently with
        // MSVC x64 /O2 /fp:precise at revision 031a0077f5799a6041004267fc12b956c1f52a20.
        ushort[] y =
        [
            5,3,3,5,7,12,15,18,4,4,4,6,8,17,18,16,
            4,4,5,7,12,17,21,17,4,5,7,9,15,26,24,19,
            5,7,11,17,20,33,31,23,7,10,16,19,24,31,34,28,
            15,19,23,26,31,36,36,30,22,28,28,29,34,30,31,30
        ];
        ushort[] chroma =
        [
            5,5,7,14,30,30,30,30,5,6,8,20,30,30,30,30,
            7,8,17,30,30,30,30,30,14,20,30,30,30,30,30,30,
            30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,
            30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30
        ];
        Assert.Equal(y, Quantization.CreateStandardTableForDistance(1.45f, 0));
        Assert.Equal(chroma, Quantization.CreateStandardTableForDistance(1.45f, 1));
        Assert.Equal(chroma, Quantization.CreateStandardTableForDistance(1.45f, 2));
        ushort[][] tables = [y, chroma, chroma];
        Assert.InRange(MathF.Abs(2.31426072f - Quantization.QuantValuesToDistance(tables, sharedChromaTable: true)), 0f, 0.00001f);
        float[][] expected =
        [
            [0.0388336331f, 0.617255569f, 0.496771127f],
            [1.13574755f, 0.999340057f, 0.98188144f],
            [1.05163026f, 0.446838617f, 0.428884447f]
        ];
        float[][] actual = Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 2, 2, false, sharedChromaTable: true);
        int[] frequencies = [1, 9, 63];
        for (int component = 0; component < actual.Length; component++)
        {
            Assert.Equal(0f, actual[component][0]);
            for (int index = 0; index < frequencies.Length; index++)
                Assert.InRange(MathF.Abs(expected[component][index] - actual[component][frequencies[index]]), 0f, 0.000001f);
        }
        ushort[][] unitTables = Enumerable.Range(0, 3).Select(_ => Enumerable.Repeat((ushort)1, 64).ToArray()).ToArray();
        Assert.InRange(MathF.Abs(0.00768749788f - Quantization.QuantValuesToDistance(unitTables, sharedChromaTable: true)), 0f, 0.000001f);
        Assert.InRange(MathF.Abs(0.141647041f - Quantization.QuantValuesToDistance([unitTables[0]], sharedChromaTable: true)), 0f, 0.000001f);
        float[][] gray = Quantization.CreateAdaptiveZeroBiasMultipliers([y], 1, 1, true, sharedChromaTable: true);
        Assert.All(Assert.Single(gray).Skip(1), multiplier => Assert.Equal(0.5f, multiplier));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void EmittedAdaptiveAcUsesSharedTableInferenceAndComponentSpecificBias(bool progressive, bool optimize)
    {
        // Independent native quant.cc fixtures at distance 1.351: using table
        // selectors [0,1,1] infers 1.79198956, while accidentally treating the
        // shared table as a Cr perceptual table infers 1.83893085. Their Cr
        // frequency-1 multipliers straddle the chosen analytical coefficient.
        const float distance = 1.351f, strength = 0.0467278957f;
        const float correctCrMultiplier = 0.847996712f, wrongCrMultiplier = 0.866299152f;
        const float cbMultiplier = 1.11422992f;
        double correctThreshold = 0.57988f + strength * correctCrMultiplier;
        double wrongThreshold = 0.57988f + strength * wrongCrMultiplier;
        double target = 0.5d * (correctThreshold + wrongThreshold);
        Assert.True(target > correctThreshold && target < wrongThreshold);
        Assert.True(target < 0.58146f + strength * cbMultiplier);

        // A constant Y=128 spatial field has the pinned independent scalar AQ
        // strength above whenever its first AC quantizer is below 9. A cosine
        // in each chroma plane has exact unquantized frequency-1 coefficient
        // 4*sqrt(2)*amplitude, and zero remaining AC coefficients.
        const int chromaQuantizer = 5;
        double amplitude = target * chromaQuantizer / (4d * Math.Sqrt(2d));
        float[] pixels = new float[8 * 8 * 3];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int offset = (y * 8 + x) * 3;
                pixels[offset] = 128f / 255f;
                pixels[offset + 1] = pixels[offset + 2] =
                    (float)((128d + amplitude * Math.Cos(Math.PI * (2 * x + 1) / 16d)) / 255d);
            }
        var options = new JpegliEncoderOptions
        {
            UseStandardQuantizationTables = true, Distance = distance, Subsampling = ChromaSubsampling.None,
            Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
            OptimizeHuffmanTables = optimize, RestartInterval = 1
        };
        JpegDocument document = Read(EncodeSource(pixels, 8, 8, PixelComponentLayout.YCbCr, options), options);
        AssertStandardTables(document, distance, grayscale: false);
        Assert.Equal((byte)chromaQuantizer, document.Quantizers[1][1]);
        Assert.Equal(0, document.Blocks[new(2, 0, 0)].Coefficients[1]);
        Assert.Equal(1, document.Blocks[new(3, 0, 0)].Coefficients[1]);
        Assert.All(document.Blocks[new(3, 0, 0)].Coefficients[2..], coefficient => Assert.Equal(0, coefficient));

        JpegliEncoderOptions uniformOptions = options with { AdaptiveQuantization = false };
        JpegDocument uniform = Read(EncodeSource(pixels, 8, 8, PixelComponentLayout.YCbCr, uniformOptions), uniformOptions);
        Assert.Equal(1, uniform.Blocks[new(2, 0, 0)].Coefficients[1]);
        Assert.Equal(1, uniform.Blocks[new(3, 0, 0)].Coefficients[1]);
    }

    [Theory]
    [MemberData(nameof(DirectDistances))]
    public void DistanceBranchesAndExtremesKeepBaselineQuantizersAndConstantDc(float distance)
    {
        foreach (bool grayscale in new[] { false, true })
            foreach (bool progressive in new[] { false, true })
            {
                PixelComponentLayout layout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.YCbCr;
                byte[] pixels = ConstantPlanes(8, 8, grayscale);
                var options = new JpegliEncoderOptions
                {
                    UseStandardQuantizationTables = true, Distance = distance, Quality = 0f,
                    AdaptiveQuantization = false, Progressive = progressive,
                    ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 1
                };
                byte[] jpeg = EncodeSource(pixels, 8, 8, layout, options);
                JpegDocument document = Read(jpeg, options);
                AssertStandardTables(document, distance, grayscale);
                foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
                {
                    int sample = key.Component == 1 ? 64 : key.Component == 2 ? 96 : 160;
                    int quantizer = document.Quantizers[document.Components[key.Component].Quantizer][0];
                    int dc = (int)Math.Round(8d * (sample - 128) / quantizer, MidpointRounding.AwayFromZero);
                    Assert.Equal(dc, block.Coefficients[0]);
                    Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
                }
                if (distance <= 0.1f || distance >= 25f)
                    foreach (byte[] table in document.Quantizers.Values)
                        Assert.All(table, value => Assert.Equal(distance <= 0.1f ? (byte)1 : (byte)255, value));
                if (distance == MathF.BitDecrement(25f))
                    foreach (byte[] table in document.Quantizers.Values)
                        Assert.All(table, value => Assert.Equal((byte)255, value));
                ImageResult decoded = ImageResult.FromMemory(jpeg,
                    grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue);
                Assert.Equal((8, 8), (decoded.Width, decoded.Height));
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void SamplingAdaptiveScansHuffmanAndRestartsPreserveTheSameCoefficients(int mode, bool adaptive)
    {
        const int width = 41, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.YCbCr24);
        JpegliEncoderOptions options = SamplingOptions(mode) with
        {
            UseStandardQuantizationTables = true, Quality = 92f,
            AdaptiveQuantization = adaptive, OptimizeHuffmanTables = false, SmoothingFactor = 37
        };
        byte[] baseline = EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, options);
        JpegDocument reference = Read(baseline, options);
        AssertStandardTables(reference, QualityDistance(92f), grayscale: false);
        foreach ((bool progressive, int bits, int interval, bool optimize) in new[]
        {
            (false, 0, 1, true), (true, 0, 1, false), (true, 0, 7, true),
            (true, 2, 1, false), (true, 2, 7, true)
        })
        {
            JpegliEncoderOptions variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits,
                RestartInterval = interval, OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, variant);
            JpegDocument actual = Read(jpeg, variant);
            AssertCoefficients(reference, actual);
            Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
                ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
        }
        // Upstream disables AQ when either axis of the luma is subsampled.
        if (mode is 12 or 13)
            Assert.Equal(baseline, EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr,
                options with { AdaptiveQuantization = !adaptive }));
    }

    [Fact]
    public void ConstantColorPlanesUseTheSameTablesInEverySamplingMode()
    {
        const int width = 17, height = 19;
        byte[] pixels = ConstantPlanes(width, height, grayscale: false);
        for (int mode = 0; mode < Enum.GetValues<ChromaSubsampling>().Length + 3; mode++)
        {
            JpegliEncoderOptions options = SamplingOptions(mode) with
            {
                UseStandardQuantizationTables = true, Quality = 50f, AdaptiveQuantization = false,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
            };
            JpegDocument document = Read(EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, options), options);
            AssertStandardTables(document, QualityDistance(50f), grayscale: false);
            Assert.Equal(ToWireOrder(Luminance), document.Quantizers[0]);
            Assert.Equal(ToWireOrder(Chrominance), document.Quantizers[1]);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                int sample = key.Component == 1 ? 64 : key.Component == 2 ? 96 : 160;
                int quantizer = key.Component == 1 ? 16 : 17;
                Assert.Equal((int)Math.Round(8d * (sample - 128) / quantizer, MidpointRounding.AwayFromZero), block.Coefficients[0]);
                Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TypedPaddedLayoutsAndIgnoredAlphaMatchTheirCompactSource(int kind)
    {
        const int width = 19, height = 17;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>().Where(layout => layout != PixelComponentLayout.Xyb))
        {
            PixelComponentLayout compactLayout = layout is PixelComponentLayout.Gray or PixelComponentLayout.YCbCr
                ? layout : PixelComponentLayout.Rgb;
            Array compact = Source(kind, width, height, compactLayout);
            int stride = width * Components(layout) + 13;
            var options = new JpegliEncoderOptions
            {
                UseStandardQuantizationTables = true, Distance = 0.375f, Progressive = true,
                ProgressiveRefinementBits = 2, RestartInterval = 2, SmoothingFactor = 37
            };
            byte[] expected = EncodeSource(compact, width, height, compactLayout, options);
            foreach (int trailing in new[] { 0, 17 })
            {
                Array padded = Source(kind, width, height, layout, stride, trailing);
                Assert.Equal(expected, EncodeSource(padded, width, height, layout, options, stride));
            }
            AssertStandardTables(Read(expected, options), options.Distance.Value,
                grayscale: layout == PixelComponentLayout.Gray);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InvalidSettingsAndConsumedSamplesAreRejectedBeforeOutput(int kind)
    {
        Array pixels = Source(kind, 1, 1, PixelComponentLayout.Rgb);
        foreach (float invalid in new[] { 0f, -1f, float.NaN, float.NegativeInfinity, float.PositiveInfinity })
        {
            using var destination = new MemoryStream();
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => EncodeSource(destination, pixels, 1, 1,
                PixelComponentLayout.Rgb, new() { UseStandardQuantizationTables = true, Distance = invalid },
                TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.Distance), error.ParamName);
            Assert.Equal(0, destination.Length);
        }
        foreach (float invalid in new[] { -1f, 101f, float.NaN, float.NegativeInfinity, float.PositiveInfinity })
        {
            using var destination = new MemoryStream();
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => EncodeSource(destination, pixels, 1, 1,
                PixelComponentLayout.Rgb, new() { UseStandardQuantizationTables = true, Distance = 1f, Quality = invalid },
                TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.Quality), error.ParamName);
            Assert.Equal(0, destination.Length);
        }
        if (pixels is float[] floats)
            foreach (float invalid in new[] { -0.01f, 1.01f, float.NaN, float.NegativeInfinity, float.PositiveInfinity })
            {
                floats[^1] = invalid;
                using var destination = new MemoryStream();
                var error = Assert.ThrowsAny<ArgumentException>(() => EncodeSource(destination, floats, 1, 1,
                    PixelComponentLayout.Rgb, new() { UseStandardQuantizationTables = true }, TestContext.Current.CancellationToken));
                Assert.Equal("pixels", error.ParamName);
                Assert.Equal(0, destination.Length);
            }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void OutOfRangeFloatPreflightUsesSelectedStandardQuantizers(bool progressive, bool optimize)
    {
        float[] pixels = Enumerable.Repeat(-0.1f, 64).ToArray();
        var options = new JpegliEncoderOptions
        {
            UseStandardQuantizationTables = true, AllowOutOfRangeFloatSamples = true,
            Quality = 100f, Distance = 25f, AdaptiveQuantization = false, Progressive = progressive,
            ProgressiveRefinementBits = progressive ? 2 : 0, OptimizeHuffmanTables = optimize
        };
        byte[] jpeg = EncodeSource(pixels, 8, 8, PixelComponentLayout.Gray, options);
        JpegDocument document = Read(jpeg, options);
        Assert.All(Assert.Single(document.Quantizers).Value, value => Assert.Equal((byte)255, value));
        Assert.Equal(-5, Assert.Single(document.Blocks).Value.Coefficients[0]);
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => EncodeSource(destination, pixels, 8, 8,
            PixelComponentLayout.Gray, options with { Distance = 0.1f, Quality = 0f }, TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void StreamsRemainOpenAndCancellationRetainsAValidOutputPrefix(int kind)
    {
        const int width = 65, height = 33;
        Array pixels = Source(kind, width, height, PixelComponentLayout.YCbCr);
        var options = new JpegliEncoderOptions
        {
            UseStandardQuantizationTables = true, Distance = 0.375f, Progressive = true,
            ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] complete = EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        EncodeSource(output, pixels, width, height, PixelComponentLayout.YCbCr, options, TestContext.Current.CancellationToken);
        Assert.Equal(complete, destination.ToArray());
        Assert.False(output.Closed);

        using var empty = new MemoryStream();
        var canceled = new CancellationToken(true);
        var preError = Assert.Throws<OperationCanceledException>(() => EncodeSource(empty, pixels, width, height,
            PixelComponentLayout.YCbCr, options, canceled));
        Assert.Equal(canceled, preError.CancellationToken);
        Assert.Equal(0, empty.Length);

        int cancelAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var partial = new MemoryStream();
        using var partialOutput = new WriteOnlyStream(partial, () =>
        {
            if (partial.Length >= cancelAfter)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => EncodeSource(partialOutput, pixels, width, height,
            PixelComponentLayout.YCbCr, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(partialOutput.Closed);
        Assert.InRange(partial.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)partial.Length], partial.ToArray());
    }

    private static float QualityDistance(float quality) => quality == 100f ? 0.01f
        : quality >= 30f ? 0.1f + (100f - quality) * 0.09f
        : (53f / 3000f) * quality * quality - (23f / 20f) * quality + 25f;

    private static byte[] ExpectedTable(float distance, bool chroma)
    {
        // Upstream standard-table mode converts distance back to a linear
        // libjpeg scale; the perceptual frequency exponents and 420 corrections
        // do not apply. Arithmetic preserves C++ single-precision operations.
        float linearQuality = distance <= 0.1f ? 1f
            : distance <= 4.6f ? (200f / 9f) * (distance - 0.1f)
            : distance <= 6.4f ? 5000f / (100f - (distance - 0.1f) / 0.09f)
            : distance < 25f ? 530000f / (3450f - 300f * MathF.Sqrt((848f * distance - 5330f) / 120f))
            : 5000f;
        float scale = 0.01f * linearQuality;
        ReadOnlySpan<byte> basis = chroma ? Chrominance : Luminance;
        byte[] result = new byte[64];
        for (int index = 0; index < result.Length; index++)
            result[index] = (byte)Math.Clamp(MathF.Round(scale * basis[index], MidpointRounding.AwayFromZero), 1f, 255f);
        return result;
    }

    private static byte[] ToWireOrder(ReadOnlySpan<byte> natural)
    {
        byte[] result = new byte[64];
        for (int index = 0; index < result.Length; index++)
            result[index] = natural[Zigzag[index]];
        return result;
    }

    private static void AssertStandardTables(JpegDocument document, float distance, bool grayscale)
    {
        Assert.Equal(grayscale ? new[] { 0 } : new[] { 0, 1 }, document.Quantizers.Keys);
        Assert.Equal(grayscale ? new[] { 0 } : new[] { 0, 1, 1 },
            document.Components.Values.Select(component => component.Quantizer));
        Assert.Equal(ToWireOrder(ExpectedTable(distance, chroma: false)), document.Quantizers[0]);
        if (!grayscale)
            Assert.Equal(ToWireOrder(ExpectedTable(distance, chroma: true)), document.Quantizers[1]);
    }

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Quantizers.Keys, actual.Quantizers.Keys);
        foreach ((int selector, byte[] table) in expected.Quantizers)
            Assert.Equal(table, actual.Quantizers[selector]);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in expected.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            Assert.False(actual.Blocks[key].IsDummy);
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        }
    }

    private static JpegliEncoderOptions SamplingOptions(int mode) => mode < 12
        ? new() { Subsampling = (ChromaSubsampling)mode }
        : new()
        {
            SamplingFactors = mode switch
            {
                12 => new(new(2, 1), new(1, 2), new(2, 1)),
                13 => new(new(1, 1), new(2, 1), new(2, 1)),
                _ => new(new(3, 2), new(1, 2), new(1, 2))
            }
        };

    private static byte[] ConstantPlanes(int width, int height, bool grayscale) =>
        Enumerable.Range(0, width * height * (grayscale ? 1 : 3))
            .Select(index => grayscale || index % 3 == 0 ? (byte)64 : index % 3 == 1 ? (byte)96 : (byte)160).ToArray();

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Source(int kind, int width, int height, PixelComponentLayout layout,
        int stride = 0, int trailing = 0)
    {
        int components = Components(layout);
        if (stride == 0)
            stride = width * components;
        int length = (height - 1) * stride + width * components + trailing;
        Array result = kind switch
        {
            0 => Enumerable.Repeat(byte.MaxValue, length).ToArray(),
            1 => Enumerable.Repeat(ushort.MaxValue, length).ToArray(),
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        bool bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < (layout == PixelComponentLayout.Gray ? 1 : 3); channel++)
                {
                    int offset = y * stride + x * components + (bgr ? 2 - channel : channel);
                    byte value = (byte)(24 + (11 * x + 19 * y + 37 * channel + 7 * (x / 8) * (channel + 1)) % 208);
                    if (result is byte[] bytes)
                        bytes[offset] = value;
                    else if (result is ushort[] words)
                        words[offset] = (ushort)(value * 257);
                    else
                        ((float[])result)[offset] = value / 255f;
                }
        return result;
    }

    private static PixelLayout ByteLayout(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Bgr => PixelLayout.Bgr24,
        PixelComponentLayout.Rgba => PixelLayout.Rgba32,
        PixelComponentLayout.Bgra => PixelLayout.Bgra32,
        PixelComponentLayout.Gray => PixelLayout.Gray8,
        PixelComponentLayout.YCbCr => PixelLayout.YCbCr24,
        _ => PixelLayout.Rgb24
    };

    private static byte[] EncodeSource(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0)
    {
        using var destination = new MemoryStream();
        EncodeSource(destination, pixels, width, height, layout, options, TestContext.Current.CancellationToken, stride);
        return destination.ToArray();
    }

    private static void EncodeSource(Stream output, Array pixels, int width, int height,
        PixelComponentLayout layout, JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0)
    {
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, ByteLayout(layout), options, stride, cancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, cancellationToken);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);
}
