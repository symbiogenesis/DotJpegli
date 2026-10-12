using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderTransferFunctionTests
{
    [Fact]
    public void DefaultAndEveryNonHdrCodePreserveExistingBytes()
    {
        Assert.Equal(2, new JpegliEncoderOptions().CicpTransferFunction);
        const int width = 9, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        foreach (bool progressive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
                RestartInterval = 1, UseStandardQuantizationTables = progressive
            };
            byte[] expected = EncodeSource(pixels, width, height, PixelComponentLayout.Rgb, options);
            for (int code = 0; code <= 255; code++)
                if (code is not (16 or 18))
                    Assert.Equal(expected, EncodeSource(pixels, width, height, PixelComponentLayout.Rgb,
                        options with { CicpTransferFunction = code }));
        }
    }

    [Theory]
    [InlineData(0, PixelComponentLayout.Rgb, 16, ChromaSubsampling.Quarter)]
    [InlineData(0, PixelComponentLayout.Gray, 18, ChromaSubsampling.Quarter)]
    [InlineData(0, PixelComponentLayout.YCbCr, 16, ChromaSubsampling.None)]
    [InlineData(1, PixelComponentLayout.Rgb, 18, ChromaSubsampling.HalfVertical)]
    [InlineData(1, PixelComponentLayout.Gray, 16, ChromaSubsampling.None)]
    [InlineData(1, PixelComponentLayout.YCbCr, 18, ChromaSubsampling.Quarter)]
    [InlineData(2, PixelComponentLayout.Rgb, 16, ChromaSubsampling.ThirdHorizontalHalfVertical)]
    [InlineData(2, PixelComponentLayout.Gray, 18, ChromaSubsampling.None)]
    [InlineData(2, PixelComponentLayout.YCbCr, 18, ChromaSubsampling.HalfHorizontal)]
    public void PerceptualDqtUsesTheIndependentTransferScaleAcrossInputTypes(
        int kind, PixelComponentLayout layout, int transfer, ChromaSubsampling sampling)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width, height, layout);
        Array before = (Array)pixels.Clone();
        var options = new JpegliEncoderOptions
        {
            CicpTransferFunction = transfer, Distance = 2.375f, Subsampling = sampling,
            AdaptiveQuantization = false, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] jpeg = EncodeSource(pixels, width, height, layout, options);
        JpegDocument document = Read(jpeg, options);
        bool is420 = layout != PixelComponentLayout.Gray && sampling == ChromaSubsampling.Quarter;
        for (int component = 0; component < (layout == PixelComponentLayout.Gray ? 1 : 3); component++)
            AssertPerceptualTableSamples(document.Quantizers[component], options.Distance.Value,
                component, transfer, is420);
        Assert.False(jpeg.SequenceEqual(EncodeSource(pixels, width, height, layout,
            options with { CicpTransferFunction = 2 })));
        Assert.Equal(before, pixels);
        ColorComponents channels = layout == PixelComponentLayout.Gray
            ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult decoded = ImageResult.FromMemory(jpeg, channels);
        Assert.Equal((width, height), (decoded.Width, decoded.Height));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void ExplicitSamplingUsesCompensationOnlyForTheExact420Scheme(int transfer)
    {
        byte[] pixels = Pattern(17, 19, PixelLayout.YCbCr24);
        JpegSamplingFactors[] factors =
        [
            new(new(2, 2), new(1, 1), new(1, 1)),
            new(new(3, 2), new(1, 2), new(1, 2)),
            new(new(1, 1), new(2, 1), new(2, 1)),
            new(new(1, 2), new(2, 1), new(1, 1))
        ];
        for (int mode = 0; mode < factors.Length; mode++)
        {
            var options = new JpegliEncoderOptions
            {
                CicpTransferFunction = transfer, Distance = 2.375f, SamplingFactors = factors[mode],
                Subsampling = ChromaSubsampling.Quarter, AdaptiveQuantization = false
            };
            JpegDocument document = Read(EncodeSource(pixels, 17, 19, PixelComponentLayout.YCbCr, options), options);
            for (int component = 0; component < 3; component++)
                AssertPerceptualTableSamples(document.Quantizers[component], options.Distance.Value,
                    component, transfer, mode == 0);
        }
    }

    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void DirectYCbCrAndGrayscaleConstantsAreNotTransferConverted(int transfer)
    {
        foreach (PixelComponentLayout layout in new[] { PixelComponentLayout.Gray, PixelComponentLayout.YCbCr })
            foreach (int kind in new[] { 0, 1, 2 })
            {
                int channels = layout == PixelComponentLayout.Gray ? 1 : 3;
                byte[] bytes = Enumerable.Range(0, 8 * 8 * channels)
                    .Select(index => (byte)(channels == 1 || index % 3 == 0 ? 64 : index % 3 == 1 ? 96 : 160)).ToArray();
                Array pixels = ConvertSource(bytes, kind);
                var options = new JpegliEncoderOptions
                {
                    CicpTransferFunction = transfer, Distance = 2.375f,
                    AdaptiveQuantization = false, Subsampling = ChromaSubsampling.None
                };
                JpegDocument document = Read(EncodeSource(pixels, 8, 8, layout, options), options);
                foreach ((BlockKey key, Block block) in document.Blocks)
                {
                    double sample = key.Component == 1 ? 64 : key.Component == 2 ? 96 : 160;
                    int quantizer = document.Quantizers[document.Components[key.Component].Quantizer][0];
                    Assert.Equal((int)Math.Round(8d * (sample - 128d) / quantizer,
                        MidpointRounding.AwayFromZero), block.Coefficients[0]);
                    Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                }
            }
    }

    [Theory]
    [InlineData(0, 16)]
    [InlineData(1, 18)]
    [InlineData(2, 16)]
    public void PaddedLayoutsIgnoredAlphaAndElementStridesPreserveTheCompactEncoding(int kind, int transfer)
    {
        const int width = 19, height = 17;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>().Where(layout => layout != PixelComponentLayout.Xyb))
        {
            PixelComponentLayout compactLayout = layout is PixelComponentLayout.Gray or PixelComponentLayout.YCbCr
                ? layout : PixelComponentLayout.Rgb;
            int stride = width * Components(layout) + 13;
            Array compact = Source(kind, width, height, compactLayout);
            Array padded = Source(kind, width, height, layout, stride, 17);
            Array before = (Array)padded.Clone();
            var options = new JpegliEncoderOptions
            {
                CicpTransferFunction = transfer, Progressive = true, ProgressiveRefinementBits = 2,
                SmoothingFactor = 37, RestartInterval = 2
            };
            Assert.Equal(EncodeSource(compact, width, height, compactLayout, options),
                EncodeSource(padded, width, height, layout, options, stride));
            Assert.Equal(before, padded);
        }
    }

    [Theory]
    [InlineData(0, PixelComponentLayout.Rgb, 16, false, false)]
    [InlineData(1, PixelComponentLayout.Gray, 18, true, false)]
    [InlineData(2, PixelComponentLayout.YCbCr, 16, true, true)]
    [InlineData(0, PixelComponentLayout.YCbCr, 18, false, true)]
    public void BaselineProgressiveRefinementAndHuffmanModesPreserveCoefficientsAndPixels(
        int kind, PixelComponentLayout layout, int transfer, bool adaptive, bool standard)
    {
        const int width = 65, height = 17;
        Array pixels = Source(kind, width, height, layout);
        var options = new JpegliEncoderOptions
        {
            CicpTransferFunction = transfer, UseStandardQuantizationTables = standard,
            AdaptiveQuantization = adaptive, OptimizeHuffmanTables = false,
            Subsampling = layout == PixelComponentLayout.YCbCr ? ChromaSubsampling.HalfVertical : ChromaSubsampling.Quarter,
            SmoothingFactor = 37
        };
        byte[] baseline = EncodeSource(pixels, width, height, layout, options);
        JpegDocument reference = Read(baseline, options);
        ColorComponents channels = layout == PixelComponentLayout.Gray
            ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        byte[] expectedPixels = ImageResult.FromMemory(baseline, channels).Data;
        foreach ((bool progressive, int bits, bool optimize, int interval) in new[]
        {
            (false, 0, true, 2), (true, 0, false, 1), (true, 2, true, 2)
        })
        {
            JpegliEncoderOptions variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits,
                OptimizeHuffmanTables = optimize, RestartInterval = interval
            };
            byte[] jpeg = EncodeSource(pixels, width, height, layout, variant);
            AssertCoefficients(reference, Read(jpeg, variant));
            Assert.Equal(expectedPixels, ImageResult.FromMemory(jpeg, channels).Data);
        }
    }

    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void StandardTablesIgnoreTransferHintAndUniformQuantizationKeepsExactBytes(int transfer)
    {
        foreach (PixelComponentLayout layout in new[] { PixelComponentLayout.Gray, PixelComponentLayout.YCbCr })
        {
            Array pixels = Source(2, 19, 17, layout);
            var options = new JpegliEncoderOptions
            {
                UseStandardQuantizationTables = true, Distance = 1.45f,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
            };
            byte[] expected = EncodeSource(pixels, 19, 17, layout, options);
            JpegDocument reference = Read(expected, options);
            byte[] hinted = EncodeSource(pixels, 19, 17, layout, options with { CicpTransferFunction = transfer });
            JpegDocument actual = Read(hinted, options);
            Assert.Equal(reference.Components, actual.Components);
            Assert.Equal(reference.Quantizers.Keys, actual.Quantizers.Keys);
            foreach ((int selector, byte[] quantizer) in reference.Quantizers)
                Assert.Equal(quantizer, actual.Quantizers[selector]);
            if (layout == PixelComponentLayout.YCbCr)
                Assert.Equal(new[] { 0, 1, 1 }, actual.Components.Values.Select(component => component.Quantizer));
            JpegliEncoderOptions uniform = options with { AdaptiveQuantization = false };
            Assert.Equal(EncodeSource(pixels, 19, 17, layout, uniform),
                EncodeSource(pixels, 19, 17, layout, uniform with { CicpTransferFunction = transfer }));
            // Grayscale AQ multipliers are fixed at 0.5 and ignore distance inference.
            if (layout == PixelComponentLayout.Gray)
                Assert.Equal(expected, hinted);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidTransferCodeIsRejectedBeforeAnyOutputForEveryInputOverload(int transfer)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        {
            Array pixels = Source(kind, 1, 1, PixelComponentLayout.Rgb);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            var options = new JpegliEncoderOptions
            {
                CicpTransferFunction = transfer, UseStandardQuantizationTables = true,
                IccProfile = new byte[] { 1, 2, 3 }, Comment = "must not be written"
            };
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => EncodeSource(output, pixels,
                1, 1, PixelComponentLayout.Rgb, options, TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.CicpTransferFunction), error.ParamName);
            Assert.Equal(0, destination.Length);
            Assert.False(output.Closed);
        }
    }

    [Theory]
    [InlineData(0, 16)]
    [InlineData(1, 18)]
    [InlineData(2, 16)]
    public void TransferHintPreservesMetadataNonSeekableStreamsAndCancellation(int kind, int transfer)
    {
        const int width = 33, height = 19;
        Array pixels = Source(kind, width, height, PixelComponentLayout.YCbCr);
        var options = new JpegliEncoderOptions
        {
            CicpTransferFunction = transfer, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, IccProfile = new byte[] { 0, 16, 18, 255 },
            ExifData = new byte[] { 73, 73, 42, 0, 8, 0, 0, 0 },
            XmpPacket = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\"/>", Comment = "unchanged transfer metadata",
            DensityUnit = JpegDensityUnit.DotsPerInch, HorizontalDensity = 144, VerticalDensity = 96
        };
        byte[] complete = EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, options);
        Assert.Equal(MetadataHeaders(EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr,
            options with { CicpTransferFunction = 2 })), MetadataHeaders(complete));
        var plain = options with { IccProfile = default, ExifData = default, XmpPacket = null, Comment = null };
        Assert.Equal(MetadataHeaders(EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr,
            plain with { CicpTransferFunction = 2 })), MetadataHeaders(
            EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, plain)));

        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        EncodeSource(output, pixels, width, height, PixelComponentLayout.YCbCr, options,
            TestContext.Current.CancellationToken);
        Assert.Equal(complete, destination.ToArray());
        Assert.False(output.Closed);
        using var empty = new MemoryStream();
        var canceled = new CancellationToken(true);
        var preError = Assert.Throws<OperationCanceledException>(() => EncodeSource(empty, pixels, width, height,
            PixelComponentLayout.YCbCr, options, canceled));
        Assert.Equal(canceled, preError.CancellationToken);
        Assert.Equal(0, empty.Length);

        int stopAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var partial = new MemoryStream();
        using var partialOutput = new WriteOnlyStream(partial, () =>
        {
            if (partial.Length >= stopAfter)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => EncodeSource(partialOutput, pixels, width, height,
            PixelComponentLayout.YCbCr, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(partialOutput.Closed);
        Assert.InRange(partial.Length, stopAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)partial.Length], partial.ToArray());
    }

    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void FinerTransferQuantizersParticipateInOutOfRangeFloatPreflight(int transfer)
    {
        float[] pixels = Enumerable.Repeat(-0.1f, 64).ToArray();
        var options = new JpegliEncoderOptions
        {
            Distance = 1f, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
        };
        // Original JPEG units are -25.5, giving unquantized DC -1228. The
        // default DC quantizer 2 makes -614 representable; either HDR hint
        // gives quantizer 1 and must reject the same untouched source.
        JpegDocument reference = Read(EncodeSource(pixels, 8, 8, PixelComponentLayout.Gray, options), options);
        Assert.Equal((byte)2, reference.Quantizers[0][0]);
        Assert.Equal(-614, Assert.Single(reference.Blocks).Value.Coefficients[0]);
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => EncodeSource(output, pixels, 8, 8,
            PixelComponentLayout.Gray, options with { CicpTransferFunction = transfer },
            TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, output.Length);
        Assert.All(pixels, sample => Assert.Equal(-0.1f, sample));
    }

    [Theory]
    [InlineData(16, float.Epsilon, 1)]
    [InlineData(18, float.Epsilon, 1)]
    [InlineData(16, float.MaxValue, 255)]
    [InlineData(18, float.MaxValue, 255)]
    public void TransferHintsKeepExtremeDistancesWithinBaselineQuantizerBounds(
        int transfer, float distance, int expectedQuantizer)
    {
        byte[] pixels = Enumerable.Repeat((byte)64, 64 * 3).ToArray();
        var options = new JpegliEncoderOptions
        {
            CicpTransferFunction = transfer, Distance = distance, AdaptiveQuantization = false,
            Progressive = true, ProgressiveRefinementBits = 2
        };
        byte[] jpeg = EncodeSource(pixels, 8, 8, PixelComponentLayout.YCbCr, options);
        JpegDocument document = Read(jpeg, options);
        foreach (byte[] table in document.Quantizers.Values)
            Assert.All(table, entry => Assert.Equal((byte)expectedQuantizer, entry));
        Assert.Equal((8, 8), (ImageResult.FromMemory(jpeg).Width, ImageResult.FromMemory(jpeg).Height));
    }

    [Theory]
    [InlineData(16, 2.94042110f, 1.16154540f, 1.29577017f, 1.29881847f)]
    [InlineData(18, 2.35233688f, 1.13731623f, 1.06647623f, 1.06891489f)]
    public void StandardSharedChromaAdaptiveInferenceAndEmittedThresholdsUseTransferScale(
        int transfer, float expectedDistance, float cbMultiplier, float crMultiplier, float wrongCrMultiplier)
    {
        // Independently extracted quant.cc SetQuantMatrices, QuantValsToDistance
        // and InitQuantizer, MSVC x64 /O2 /fp:precise, pinned upstream revision
        // 031a0077f5799a6041004267fc12b956c1f52a20. Standard tables at distance .9
        // remain unchanged; their inferred distance still uses the transfer scale.
        ushort[] y =
        [
            3,2,2,3,4,7,9,11,2,2,2,3,5,10,11,10,
            2,2,3,4,7,10,12,10,2,3,4,5,9,15,14,11,
            3,4,7,10,12,19,18,14,4,6,10,11,14,18,20,16,
            9,11,14,15,18,22,21,18,13,16,17,17,20,18,18,18
        ];
        ushort[] chroma =
        [
            3,3,4,8,18,18,18,18,3,4,5,12,18,18,18,18,
            4,5,10,18,18,18,18,18,8,12,18,18,18,18,18,18,
            18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,
            18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18
        ];
        ushort[][] tables = [y, chroma, chroma];
        Assert.InRange(MathF.Abs(expectedDistance - Quantization.QuantValuesToDistance(tables,
            sharedChromaTable: true, cicpTransferFunction: transfer)), 0f, 0.000001f);
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, false,
            sharedChromaTable: true, cicpTransferFunction: transfer);
        Assert.InRange(MathF.Abs(cbMultiplier - multipliers[1][1]), 0f, 0.000001f);
        Assert.InRange(MathF.Abs(crMultiplier - multipliers[2][1]), 0f, 0.000001f);

        // A constant Y=128 plane has independently established scalar AQ
        // strength .0467278957 while the first AC luma quantizer is below 9.
        // Two analytical Cr cosines distinguish using the unadjusted global
        // scale and using a Cr basis for the shared Cb quantization selector.
        const float strength = 0.0467278957f, defaultCrMultiplier = 0.607888103f;
        foreach (bool sharedSelectorProbe in new[] { false, true })
        {
            double correctThreshold = 0.57988f + strength * crMultiplier;
            double wrongThreshold = 0.57988f + strength *
                (sharedSelectorProbe ? wrongCrMultiplier : defaultCrMultiplier);
            double target = (correctThreshold + wrongThreshold) / 2d;
            Assert.True(Math.Abs(correctThreshold - wrongThreshold) > 0.0001d);
            double amplitude = target * chroma[1] / (4d * Math.Sqrt(2d));
            float[] pixels = new float[8 * 8 * 3];
            for (int row = 0; row < 8; row++)
                for (int column = 0; column < 8; column++)
                {
                    int offset = (row * 8 + column) * 3;
                    pixels[offset] = pixels[offset + 1] = 128f / 255f;
                    pixels[offset + 2] = (float)((128d + amplitude *
                        Math.Cos(Math.PI * (2 * column + 1) / 16d)) / 255d);
                }
            foreach (bool progressive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions
                {
                    CicpTransferFunction = transfer, UseStandardQuantizationTables = true,
                    Distance = 0.9f, Subsampling = ChromaSubsampling.None,
                    Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
                    OptimizeHuffmanTables = progressive, RestartInterval = 1
                };
                JpegDocument document = Read(EncodeSource(pixels, 8, 8, PixelComponentLayout.YCbCr, options), options);
                Assert.Equal(ToWireOrder(y), document.Quantizers[0]);
                Assert.Equal(ToWireOrder(chroma), document.Quantizers[1]);
                Assert.Equal(new[] { 0, 1, 1 }, document.Components.Values.Select(component => component.Quantizer));
                Assert.Equal(sharedSelectorProbe ? 1 : 0, document.Blocks[new(3, 0, 0)].Coefficients[1]);
                Assert.All(document.Blocks[new(3, 0, 0)].Coefficients[2..], coefficient => Assert.Equal(0, coefficient));
                if (!sharedSelectorProbe)
                {
                    JpegliEncoderOptions unadjusted = options with { CicpTransferFunction = 2 };
                    JpegDocument noHint = Read(EncodeSource(pixels, 8, 8, PixelComponentLayout.YCbCr, unadjusted), unadjusted);
                    Assert.Equal(1, noHint.Blocks[new(3, 0, 0)].Coefficients[1]);
                }
            }
        }
    }

    private static byte[] ToWireOrder(ushort[] natural)
    {
        int[] zigzag =
        [
            0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
            12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
            35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
            58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
        ];
        return zigzag.Select(index => (byte)natural[index]).ToArray();
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(16, true)]
    [InlineData(18, false)]
    [InlineData(18, true)]
    public void CompletePerceptualTablesAndAdaptiveInferenceMatchPinnedNativeFixtures(int transfer, bool is420)
    {
        // Full natural-order baseline tables and inferred distance from the
        // verbatim pinned quant.cc functions, MSVC x64 /O2 /fp:precise, revision
        // 031a0077f5799a6041004267fc12b956c1f52a20. PQ fixtures are below the
        // distance-1.5 branch; HLG fixtures also exercise nonlinear AC scaling.
        float distance = transfer == 16 ? 1.45f : 2.5f;
        (ushort[][] Tables, float InferredDistance, float GrayDistance, float[][] Multipliers) fixture =
            (transfer, is420) switch
            {
                (16, false) =>
                (
                    [
                        [1,2,3,3,3,3,4,4,2,2,3,3,3,3,4,4,3,3,3,3,4,4,4,4,3,3,3,3,4,5,5,5,
                            3,3,4,4,5,5,6,6,3,3,4,5,5,5,5,6,4,4,4,5,6,5,5,5,4,4,4,5,6,6,5,4],
                        [3,7,9,11,11,17,19,29,7,9,9,14,17,17,27,22,9,9,11,18,20,24,104,25,11,14,18,19,16,25,25,36,
                            11,17,20,16,20,15,20,52,17,17,24,25,15,32,47,68,19,27,104,25,20,47,62,89,29,22,25,36,52,68,89,113],
                        [3,5,7,7,9,9,9,10,5,6,7,7,8,7,7,8,7,7,7,7,9,7,8,9,7,7,7,7,8,8,8,8,
                            9,8,9,8,7,10,9,8,9,7,7,8,10,9,10,65,9,7,8,8,9,10,64,84,10,8,9,8,8,65,84,116]
                    ],
                    1.45059276f, 1.44653392f,
                    [
                        [0.0162055306f, 0.660525262f, 0.684143841f],
                        [1.10016441f, 1.10077786f, 1.03085136f],
                        [0.714886129f, 0.596512318f, 0.583653748f]
                    ]
                ),
                (16, true) =>
                (
                    [
                        [2,2,4,3,4,4,5,5,2,3,4,3,4,4,5,5,4,4,4,4,4,5,5,5,3,3,4,4,5,6,6,6,
                            4,4,4,5,6,6,7,7,4,4,5,6,6,7,6,7,5,5,5,6,7,6,7,7,5,5,5,6,7,7,7,5],
                        [1,3,4,4,4,6,7,12,3,3,4,5,6,6,14,9,4,4,5,7,7,11,43,10,4,5,7,7,7,11,12,17,
                            4,6,7,7,7,6,7,24,6,6,11,11,6,12,18,6,7,14,43,12,7,18,6,6,12,9,10,17,24,6,6,47],
                        [1,2,3,3,3,3,4,4,2,2,3,2,3,3,3,4,3,3,3,3,3,3,3,4,3,2,3,3,3,3,4,4,
                            3,3,3,3,3,4,3,4,3,3,3,3,4,3,4,6,4,3,3,4,3,4,6,6,4,4,4,4,4,6,6,48]
                    ],
                    1.85401618f, 0f,
                    [
                        [0.0267752241f, 0.640313745f, 0.596621156f],
                        [1.11678541f, 1.05339575f, 1.00797725f],
                        [0.872180939f, 0.52659899f, 0.511360288f]
                    ]
                ),
                (18, false) =>
                (
                    [
                        [3,3,5,5,7,8,8,8,3,4,5,6,7,7,9,9,5,5,6,7,8,8,8,9,5,6,7,7,9,10,10,10,
                            7,7,8,9,10,11,12,12,8,7,8,10,11,12,11,13,8,9,8,10,12,11,12,12,8,9,9,10,12,13,12,10],
                        [6,11,17,20,24,37,40,63,11,16,17,28,36,36,58,46,17,17,22,36,42,52,223,53,20,28,36,41,35,52,52,78,
                            24,36,42,35,42,32,43,111,37,36,52,52,32,68,101,147,40,58,223,52,43,101,133,192,63,46,53,78,111,147,192,244],
                        [6,8,14,13,19,19,20,21,8,12,13,15,18,15,15,18,14,13,14,14,19,15,17,19,13,15,14,15,17,17,16,18,
                            19,18,19,17,15,21,20,18,19,15,15,17,21,19,22,140,20,15,17,16,20,22,137,181,21,18,19,18,18,140,181,250]
                    ],
                    2.50067163f, 2.50048041f,
                    [
                        [0.0437175967f, 0.607916355f, 0.456329286f],
                        [1.14342773f, 0.977446079f, 0.971311927f],
                        [1.12431192f, 0.414533615f, 0.39547962f]
                    ]
                ),
                _ =>
                (
                    [
                        [3,4,7,7,9,9,10,10,4,5,6,7,9,8,11,11,7,6,7,8,9,10,10,11,7,7,8,9,11,12,12,12,
                            9,9,9,11,12,13,15,15,9,8,10,12,13,14,14,16,10,11,10,12,15,14,14,14,10,11,11,12,15,16,14,12],
                        [3,4,7,8,9,13,16,26,4,6,7,10,13,14,29,19,7,7,10,14,14,24,92,23,8,10,14,15,15,22,25,36,
                            9,13,14,15,16,13,14,52,13,14,24,22,13,27,39,13,16,29,92,25,14,39,13,13,26,19,23,36,52,13,13,100],
                        [3,3,6,5,7,7,8,8,3,4,6,5,7,6,7,8,6,6,6,5,6,7,7,8,5,5,5,6,7,7,8,9,
                            7,7,6,7,6,9,7,8,7,6,7,7,9,7,8,12,8,7,7,8,7,8,13,12,8,8,8,9,8,12,12,103]
                    ],
                    3.05576086f, 0f,
                    [
                        [0.0568000004f, 0.582899988f, 0.34799999f],
                        [1.16400003f, 0.918799996f, 0.943000019f],
                        [1.31900001f, 0.328000009f, 0.305999994f]
                    ]
                )
            };
        var (tables, inferredDistance, grayDistance, multipliers) = fixture;
        var options = new JpegliEncoderOptions
        {
            CicpTransferFunction = transfer, Distance = distance,
            Subsampling = is420 ? ChromaSubsampling.Quarter : ChromaSubsampling.None,
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
        };
        byte[] pixels = Pattern(19, 17, PixelLayout.YCbCr24);
        JpegDocument document = Read(EncodeSource(pixels, 19, 17, PixelComponentLayout.YCbCr, options), options);
        for (int component = 0; component < tables.Length; component++)
            Assert.Equal(ToWireOrder(tables[component]), document.Quantizers[component]);
        Assert.InRange(MathF.Abs(inferredDistance - Quantization.QuantValuesToDistance(tables,
            cicpTransferFunction: transfer)), 0f, 0.000001f);
        float[][] actualMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables,
            is420 ? 2 : 1, is420 ? 2 : 1, false, cicpTransferFunction: transfer);
        int[] frequencies = [1, 9, 63];
        for (int component = 0; component < tables.Length; component++)
            for (int index = 0; index < frequencies.Length; index++)
                Assert.InRange(MathF.Abs(multipliers[component][index] -
                    actualMultipliers[component][frequencies[index]]), 0f, 0.000001f);
        if (!is420)
        {
            Array gray = Source(2, 19, 17, PixelComponentLayout.Gray);
            JpegDocument grayDocument = Read(EncodeSource(gray, 19, 17, PixelComponentLayout.Gray, options), options);
            Assert.Equal(ToWireOrder(tables[0]), grayDocument.Quantizers[0]);
            Assert.InRange(MathF.Abs(grayDistance - Quantization.QuantValuesToDistance([tables[0]],
                cicpTransferFunction: transfer)), 0f, 0.000001f);
            float[][] grayMultipliers = Quantization.CreateAdaptiveZeroBiasMultipliers([tables[0]],
                1, 1, true, cicpTransferFunction: transfer);
            Assert.All(grayMultipliers[0].Skip(1), multiplier => Assert.Equal(0.5f, multiplier));
        }
    }

    private static void AssertPerceptualTableSamples(byte[] table, float distance, int component,
        int transfer, bool is420)
    {
        // Independent pinned quant.cc constants at natural frequencies 0, 1, 63,
        // which also occupy the same wire DQT positions. No encoder quantizer helper is used.
        float[][] matrices =
        [
            [1.2397409345866273f, 1.7227115097630963f, 4.37787101190424f],
            [2.8236197786377537f, 6.495639358561486f, 112.16099098350989f],
            [2.9217254961255255f, 4.497681013199305f, 114.89202448569779f]
        ];
        float[] exponents = [1f, 0.51f, 1f];
        float[] chroma420Rescale = [0.4093f, 0.3209f, 0.3368f];
        int[] positions = [0, 1, 63];
        float globalScale = 1.73966010f;
        globalScale *= transfer == 16 ? 0.4f : 0.5f;
        if (is420)
            globalScale *= 1.22f;
        for (int index = 0; index < positions.Length; index++)
        {
            float distanceScale = distance < 1.5f ? distance : MathF.Max(0.5f * distance,
                (float)Math.Pow(1.5f, 1d - exponents[index]) * MathF.Pow(distance, exponents[index]));
            float scale = globalScale * distanceScale;
            if (is420 && component > 0)
                scale *= chroma420Rescale[index];
            Assert.Equal((byte)Math.Clamp(MathF.Round(scale * matrices[component][index],
                MidpointRounding.AwayFromZero), 1f, 255f), table[positions[index]]);
        }
    }

    private static byte[] MetadataHeaders(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        using var metadata = new MemoryStream();
        for (int offset = 2; offset < jpeg.Length;)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int marker = jpeg[offset + 1];
            if (marker == 0xda)
                break;
            int length = (jpeg[offset + 2] << 8) | jpeg[offset + 3];
            Assert.InRange(length, 2, jpeg.Length - offset - 2);
            if (marker is >= 0xe0 and <= 0xef or 0xfe)
                metadata.Write(jpeg.AsSpan(offset, length + 2));
            offset += length + 2;
        }
        return metadata.ToArray();
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

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array ConvertSource(byte[] bytes, int kind) => kind switch
    {
        0 => bytes,
        1 => bytes.Select(value => (ushort)(value * 257)).ToArray(),
        _ => bytes.Select(value => value / 255f).ToArray()
    };

    private static Array Source(int kind, int width, int height, PixelComponentLayout layout,
        int stride = 0, int trailing = 0)
    {
        int components = Components(layout);
        if (stride == 0)
            stride = width * components;
        int length = (height - 1) * stride + width * components + trailing;
        Array pixels = kind switch
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
                    int value = 24 + (11 * x + 19 * y + 37 * channel + 7 * (x / 8) * (channel + 1)) % 208;
                    if (pixels is byte[] bytes)
                        bytes[offset] = (byte)value;
                    else if (pixels is ushort[] words)
                        words[offset] = (ushort)(value * 257 + (11 * x + 17 * y + 29 * channel) % 113);
                    else
                        ((float[])pixels)[offset] = (value + (11 * x + 17 * y + 29 * channel) % 113 / 257f) / 255f;
                }
        return pixels;
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
        using var output = new MemoryStream();
        EncodeSource(output, pixels, width, height, layout, options, TestContext.Current.CancellationToken, stride);
        return output.ToArray();
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
