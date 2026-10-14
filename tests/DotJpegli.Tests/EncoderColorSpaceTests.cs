using System.Security.Cryptography;
using System.Text;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderColorSpaceTests
{
    private static readonly int[] RgbIds = ['R', 'G', 'B'];

    // ITU-T T.81 Annex K luminance matrix, independent of encoder constants.
    private static ReadOnlySpan<byte> Luminance =>
    [
        16,11,10,16,24,40,51,61,12,12,14,19,26,58,60,55,
        14,13,16,24,40,57,69,56,14,17,22,29,51,87,80,62,
        18,22,37,56,68,109,103,77,24,35,55,64,81,104,113,92,
        49,64,78,87,103,121,120,101,72,92,95,98,112,100,103,99
    ];

    private static ReadOnlySpan<int> Zigzag =>
    [
        0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
        12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
        35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
        58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
    ];

    [Fact]
    public void AutomaticDefaultsAndExistingBytesRemainStable()
    {
        Assert.Equal(0, (int)JpegColorSpace.Automatic);
        Assert.Equal(1, (int)JpegColorSpace.Grayscale);
        Assert.Equal(2, (int)JpegColorSpace.YCbCr);
        Assert.Equal(3, (int)JpegColorSpace.Rgb);
        Assert.Equal(JpegColorSpace.Automatic, new JpegliEncoderOptions().ColorSpace);
        byte[] jpeg = JpegliEncoder.Encode(TestImages.Gradient(19, 17), 19, 17, options: new()
        {
            Quality = 90, Subsampling = ChromaSubsampling.Quarter,
            AdaptiveQuantization = false, OptimizeHuffmanTables = true
        }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("145458B6A9AAF6FD5EBF4A4AACF66C7EC8F9E528E2E2A8B97D733E715DE4812F",
            Convert.ToHexString(SHA256.HashData(jpeg)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AutomaticMatchesExplicitOutputForEveryNonXybSourceLayout(int kind)
    {
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>().Where(layout => layout != PixelComponentLayout.Xyb))
        {
            Array pixels = Source(kind, 19, 17, layout);
            var automatic = new JpegliEncoderOptions
            {
                SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            JpegColorSpace selected = layout == PixelComponentLayout.Gray ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr;
            byte[] expected = Encode(pixels, 19, 17, layout, automatic);
            Assert.Equal(expected, Encode(pixels, 19, 17, layout, automatic with { ColorSpace = selected }));
            Assert.Equal(layout == PixelComponentLayout.Gray ? new[] { 1 } : new[] { 1, 2, 3 }, Read(expected, automatic).Components.Keys);
            Assert.Contains(Markers(expected), marker => marker.Code == 0xe0);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RgbLayoutsPrecisionAndElementStridesPreserveEachOutput(int kind)
    {
        const int width = 19, height = 17;
        Array rgb = Source(kind, width, height, PixelComponentLayout.Rgb);
        foreach (JpegColorSpace colorSpace in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb })
        {
            var options = new JpegliEncoderOptions
            {
                ColorSpace = colorSpace, SmoothingFactor = 73, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] expected = Encode(rgb, width, height, PixelComponentLayout.Rgb, options);
            foreach (PixelComponentLayout layout in new[]
                { PixelComponentLayout.Rgb, PixelComponentLayout.Bgr, PixelComponentLayout.Rgba, PixelComponentLayout.Bgra })
            {
                Array packed = Repack(rgb, width, height, layout);
                int rowElements = width * Channels(layout), stride = rowElements + 13;
                // No final-row padding is required; float padding and alpha are NaN.
                Array padded = Allocate(kind, (height - 1) * stride + rowElements);
                for (int row = 0; row < height; row++)
                    Array.Copy(packed, row * rowElements, padded, row * stride, rowElements);
                Assert.Equal(expected, Encode(packed, width, height, layout, options));
                Assert.Equal(expected, Encode(padded, width, height, layout, options, stride));
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                Encode(output, padded, width, height, layout, options, stride);
                Assert.False(output.Closed);
                Assert.Equal(expected, destination.ToArray());
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GraySourcesReplicateForRgbAndSynthesizeNeutralYCbCr(int kind)
    {
        const int width = 19, height = 17;
        Array gray = Source(kind, width, height, PixelComponentLayout.Gray);
        foreach (JpegColorSpace colorSpace in new[] { JpegColorSpace.Rgb, JpegColorSpace.YCbCr })
        {
            PixelComponentLayout layout = colorSpace == JpegColorSpace.Rgb ? PixelComponentLayout.Rgb : PixelComponentLayout.YCbCr;
            Array expanded = Allocate(kind, width * height * 3);
            for (int pixel = 0; pixel < width * height; pixel++)
                for (int component = 0; component < 3; component++)
                    if (component == 0 || colorSpace == JpegColorSpace.Rgb)
                        Array.Copy(gray, pixel, expanded, pixel * 3 + component, 1);
                    else
                        Set(expanded, pixel * 3 + component, kind == 0 ? 128 : 32896);
            var options = new JpegliEncoderOptions
            {
                ColorSpace = colorSpace, SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] expected = Encode(expanded, width, height, layout, options);
            Assert.Equal(expected, Encode(gray, width, height, PixelComponentLayout.Gray, options));
            ImageResult decoded = ImageResult.FromMemory(expected, ColorComponents.RedGreenBlue);
            Assert.Equal((width, height), (decoded.Width, decoded.Height));
            for (int pixel = 0; pixel < width * height; pixel++)
            {
                Assert.Equal(decoded.Data[pixel * 3], decoded.Data[pixel * 3 + 1]);
                Assert.Equal(decoded.Data[pixel * 3], decoded.Data[pixel * 3 + 2]);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void YCbCrToGrayscaleConsumesOnlyY(int kind)
    {
        const int width = 19, height = 17;
        Array ycbcr = Source(kind, width, height, PixelComponentLayout.YCbCr);
        Array gray = Allocate(kind, width * height);
        for (int pixel = 0; pixel < width * height; pixel++)
            Array.Copy(ycbcr, pixel * 3, gray, pixel, 1);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.Grayscale, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 1), new(2, 1), new(1, 1)),
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = Encode(gray, width, height, PixelComponentLayout.Gray, options);
        if (ycbcr is float[] floats)
            for (int pixel = 0; pixel < width * height; pixel++)
            {
                floats[pixel * 3 + 1] = float.NaN;
                floats[pixel * 3 + 2] = pixel % 2 == 0 ? float.PositiveInfinity : -float.MaxValue;
            }
        Assert.Equal(expected, Encode(ycbcr, width, height, PixelComponentLayout.YCbCr, options));
        Assert.Equal(expected, Encode(ycbcr, width, height, PixelComponentLayout.YCbCr,
            options with { AllowOutOfRangeFloatSamples = true }));
        Component component = Assert.Single(Read(expected, options).Components).Value;
        Assert.Equal((1, 1), (component.H, component.V));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RgbMarkersAndSharedAnnexKQuantizerDescribeDirectChannels(int kind)
    {
        Array pixels = Source(kind, 19, 17, PixelComponentLayout.Rgb);
        foreach (float distance in new[] { 0.01f, 0.375f, 1.45f, 4.6f, 12.5f, 30f })
            foreach (bool progressive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions
                {
                    ColorSpace = JpegColorSpace.Rgb, Distance = distance, AdaptiveQuantization = false,
                    Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
                    OptimizeHuffmanTables = progressive, RestartInterval = 2
                };
                byte[] jpeg = Encode(pixels, 19, 17, PixelComponentLayout.Rgb, options);
                JpegDocument document = Read(jpeg, options);
                Assert.Equal(RgbIds, document.Components.Keys);
                Assert.Equal(new[] { 0 }, document.Quantizers.Keys);
                Assert.Equal(ExpectedRgbTable(distance), document.Quantizers[0]);
                Assert.All(document.Components.Values, component => Assert.Equal(new Component(1, 1, 0), component));
                Assert.All(document.Scans.SelectMany(scan => scan.Selectors), selector =>
                {
                    Assert.Equal(0, selector.Dc);
                    Assert.Equal(0x10, selector.Ac);
                });
                List<Marker> markers = Markers(jpeg);
                Assert.DoesNotContain(markers, marker => marker.Code is 0xe0 or 0xee);
                Assert.Equal(new[] { 0, 0x10 }, HuffmanSelectors(markers).Distinct().Order());
                ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
                Assert.Equal((19, 17), (decoded.Width, decoded.Height));
                Assert.Equal(jpeg, Encode(pixels, 19, 17, PixelComponentLayout.Rgb,
                    options with { Subsampling = ChromaSubsampling.None, UseStandardQuantizationTables = true }));
                foreach (int transferFunction in new[] { 16, 18 })
                    Assert.Equal(document.Quantizers[0], Read(Encode(pixels, 19, 17,
                        PixelComponentLayout.Rgb, options with { CicpTransferFunction = transferFunction }), options).Quantizers[0]);
            }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 37)]
    [InlineData(1, 0)]
    [InlineData(1, 37)]
    [InlineData(2, 0)]
    [InlineData(2, 37)]
    public void DirectRgbCoefficientsMatchIndependentSmoothingSamplingAndDctOracle(int kind, int smoothing)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width, height, PixelComponentLayout.Rgb);
        foreach (JpegSamplingFactors? factors in new JpegSamplingFactors?[]
            { null, new(new(1, 2), new(2, 1), new(1, 1)), new(new(1, 1), new(1, 3), new(1, 1)) })
        {
            var options = new JpegliEncoderOptions
            {
                ColorSpace = JpegColorSpace.Rgb, Distance = 0.375f, SamplingFactors = factors,
                AdaptiveQuantization = false, SmoothingFactor = smoothing, RestartInterval = 2
            };
            JpegDocument document = Read(Encode(pixels, width, height, PixelComponentLayout.Rgb, options), options);
            JpegComponentSampling[] expectedFactors = factors is null
                ? [new(1, 1), new(1, 1), new(1, 1)] : [factors.Luma, factors.Cb, factors.Cr];
            for (int component = 0; component < 3; component++)
                Assert.Equal((expectedFactors[component].Horizontal, expectedFactors[component].Vertical),
                    (document.Components[RgbIds[component]].H, document.Components[RgbIds[component]].V));
            int maxH = document.Components.Values.Max(component => component.H);
            int maxV = document.Components.Values.Max(component => component.V);
            int substantialAc = 0;
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                Component component = document.Components[key.Component];
                int channel = Array.IndexOf(RgbIds, key.Component);
                double[] samples = OracleSamples(pixels, width, height, key, channel,
                    maxH / component.H, maxV / component.V, smoothing, grayscale: false);
                byte[] quantizer = document.Quantizers[0];
                for (int frequency = 0; frequency < 64; frequency++)
                {
                    int natural = Zigzag[frequency];
                    double coefficient = OracleDct(samples, natural % 8, natural / 8) / quantizer[frequency];
                    Assert.True(Math.Abs(block.Coefficients[frequency] - coefficient) <= 0.501,
                        $"{(char)key.Component} ({key.X},{key.Y}) frequency {frequency}: {coefficient} -> {block.Coefficients[frequency]}.");
                    if (frequency != 0 && Math.Abs(block.Coefficients[frequency]) >= 2)
                        substantialAc++;
                }
            }
            Assert.True(substantialAc >= 24, "Check substantial AC detail, not only DC or zero coefficients.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RgbToGrayscaleMatchesIndependentLuminanceAndDctOracle(int kind)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.Grayscale, Quality = 92, AdaptiveQuantization = false,
            SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] jpeg = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        JpegDocument document = Read(jpeg, options);
        Assert.Equal(new[] { 1 }, document.Components.Keys);
        Assert.Contains(Markers(jpeg), marker => marker.Code == 0xe0);
        foreach ((BlockKey key, Block block) in document.Blocks)
        {
            double[] samples = OracleSamples(pixels, width, height, key, 0, 1, 1, 37, grayscale: true);
            for (int frequency = 0; frequency < 64; frequency++)
            {
                int natural = Zigzag[frequency];
                double coefficient = OracleDct(samples, natural % 8, natural / 8) / document.Quantizers[0][frequency];
                Assert.InRange(Math.Abs(block.Coefficients[frequency] - coefficient), 0, 0.501);
            }
        }
    }

    [Theory]
    [InlineData(0, JpegColorSpace.Rgb)]
    [InlineData(1, JpegColorSpace.Rgb)]
    [InlineData(2, JpegColorSpace.Rgb)]
    [InlineData(0, JpegColorSpace.Grayscale)]
    [InlineData(1, JpegColorSpace.Grayscale)]
    [InlineData(2, JpegColorSpace.Grayscale)]
    [InlineData(0, JpegColorSpace.YCbCr)]
    [InlineData(1, JpegColorSpace.YCbCr)]
    [InlineData(2, JpegColorSpace.YCbCr)]
    public void ScansHuffmanOptimizationAndRestartsPreserveCoefficientsAndDecodedPixels(int kind, JpegColorSpace colorSpace)
    {
        const int width = 65, height = 17;
        Array pixels = Source(kind, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, Quality = 90, SmoothingFactor = 37, OptimizeHuffmanTables = false,
            SamplingFactors = colorSpace == JpegColorSpace.Rgb ? new(new(2, 1), new(1, 1), new(1, 1)) : null
        };
        byte[] baseline = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        JpegDocument expected = Read(baseline, options);
        byte[] decoded = ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data;
        foreach ((bool progressive, int refinement, bool optimize, int interval) in new[]
            { (false, 0, true, 2), (false, 0, false, 1), (true, 0, false, 2), (true, 2, true, 1) })
        {
            var variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = refinement,
                OptimizeHuffmanTables = optimize, RestartInterval = interval
            };
            byte[] jpeg = Encode(pixels, width, height, PixelComponentLayout.Rgb, variant);
            AssertCoefficients(expected, Read(jpeg, variant));
            Assert.Equal(decoded, ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
        }
    }

    [Fact]
    public void ExpandedBytesRetainExactlyTheSameEncodingForEveryOutput()
    {
        byte[] bytes = (byte[])Source(0, 19, 17, PixelComponentLayout.Rgb);
        ushort[] words = bytes.Select(value => (ushort)(value * 257)).ToArray();
        foreach (JpegColorSpace colorSpace in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb })
        {
            var options = new JpegliEncoderOptions
            {
                ColorSpace = colorSpace, SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            Assert.Equal(Encode(bytes, 19, 17, PixelComponentLayout.Rgb, options), Encode(words, 19, 17, PixelComponentLayout.Rgb, options));
        }
    }

    [Fact]
    public void RgbAdaptiveAnalysisUsesGreenResolution()
    {
        byte[] pixels = (byte[])Source(0, 48, 35, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions { ColorSpace = JpegColorSpace.Rgb, Quality = 70 };
        foreach (JpegSamplingFactors factors in new[]
            { new JpegSamplingFactors(new(2, 1), new(1, 1), new(1, 1)), new(new(1, 2), new(1, 1), new(1, 1)) })
        {
            var sampled = options with { SamplingFactors = factors };
            Assert.Equal(Encode(pixels, 48, 35, PixelComponentLayout.Rgb, sampled),
                Encode(pixels, 48, 35, PixelComponentLayout.Rgb, sampled with { AdaptiveQuantization = false }));
        }
        var greenFull = options with { SamplingFactors = new(new(1, 1), new(2, 1), new(1, 1)) };
        Assert.False(Encode(pixels, 48, 35, PixelComponentLayout.Rgb, greenFull).SequenceEqual(
            Encode(pixels, 48, 35, PixelComponentLayout.Rgb, greenFull with { AdaptiveQuantization = false })));
    }

    [Theory]
    [InlineData(false, 0.499, 0)]
    [InlineData(false, 0.501, 1)]
    [InlineData(false, 0.55, 1)]
    [InlineData(true, 0.51, 0)]
    [InlineData(true, 0.55, 1)]
    public void RgbUsesUniformHalfThresholdsAcrossRedAndBlue(bool adaptive, double target, int expectedAc)
    {
        // At distance 1.45 Annex K's u=1 quantizer is 3. A constant G=128
        // field has independent pinned scalar AQ strength 0.0467278957.
        // RGB's offset/multiplier are 0.5/0.5, giving threshold 0.52336394785.
        // YCbCr's component offsets would erase target 0.55.
        const int quantizer = 3;
        double amplitude = target * quantizer / (4 * Math.Sqrt(2));
        float[] pixels = new float[64 * 3];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int offset = (y * 8 + x) * 3;
                pixels[offset] = pixels[offset + 2] = (float)((128 + amplitude * Math.Cos((2 * x + 1) * Math.PI / 16)) / 255);
                pixels[offset + 1] = 128f / 255f;
            }
        var options = new JpegliEncoderOptions { ColorSpace = JpegColorSpace.Rgb, Distance = 1.45f, AdaptiveQuantization = adaptive };
        JpegDocument document = Read(Encode(pixels, 8, 8, PixelComponentLayout.Rgb, options), options);
        Assert.Equal((byte)quantizer, document.Quantizers[0][1]);
        foreach (int component in new[] { (int)'R', (int)'B' })
        {
            Block block = document.Blocks[new(component, 0, 0)];
            Assert.Equal(0, block.Coefficients[0]);
            Assert.Equal(expectedAc, block.Coefficients[1]);
            Assert.All(block.Coefficients[2..], coefficient => Assert.Equal(0, coefficient));
        }
        Assert.All(document.Blocks[new('G', 0, 0)].Coefficients, coefficient => Assert.Equal(0, coefficient));
    }

    [Theory]
    [InlineData(0.5231, 0)]
    [InlineData(0.5236, 1)]
    public void RgbAdaptiveFieldUsesGreenSamplesDespiteRedTexture(double target, int expectedAc)
    {
        // The independently pinned constant G=128 field gives strength
        // 0.0467278957, hence threshold 0.52336394785. The B cosine brackets
        // that threshold while the unrelated R field changes dramatically.
        // Reading R or converted Y for analysis would move this threshold.
        double amplitude = target * 3 / (4 * Math.Sqrt(2));
        var options = new JpegliEncoderOptions { ColorSpace = JpegColorSpace.Rgb, Distance = 1.45f };
        foreach (int redPattern in new[] { 0, 1, 2 })
        {
            var pixels = new float[64 * 3];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int offset = (y * 8 + x) * 3;
                    pixels[offset] = (redPattern == 0 ? 32f : redPattern == 1 ? 224f : ((x + y) % 2 == 0 ? 16f : 240f)) / 255f;
                    pixels[offset + 1] = 128f / 255f;
                    pixels[offset + 2] = (float)((128 + amplitude * Math.Cos((2 * x + 1) * Math.PI / 16)) / 255);
                }
            JpegDocument document = Read(Encode(pixels, 8, 8, PixelComponentLayout.Rgb, options), options);
            Assert.Equal(expectedAc, document.Blocks[new('B', 0, 0)].Coefficients[1]);
            Assert.All(document.Blocks[new('B', 0, 0)].Coefficients[2..], value => Assert.Equal(0, value));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InvalidColorSpaceAndUnsupportedConversionsAreRejectedBeforeOutput(int kind)
    {
        Array rgb = Source(kind, 8, 8, PixelComponentLayout.Rgb);
        Array ycbcr = Source(kind, 8, 8, PixelComponentLayout.YCbCr);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, rgb, 8, 8,
                PixelComponentLayout.Rgb, new() { ColorSpace = (JpegColorSpace)invalid }));
            Assert.Equal(nameof(JpegliEncoderOptions.ColorSpace), error.ParamName);
        }
        var unsupported = Assert.Throws<ArgumentException>(() => Encode(output, ycbcr, 8, 8,
            PixelComponentLayout.YCbCr, new() { ColorSpace = JpegColorSpace.Rgb }));
        Assert.Equal(nameof(JpegliEncoderOptions.ColorSpace), unsupported.ParamName);
        var sampling = Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, rgb, 8, 8,
            PixelComponentLayout.Rgb, new() { ColorSpace = JpegColorSpace.Rgb, Subsampling = (ChromaSubsampling)999 }));
        Assert.Equal(nameof(JpegliEncoderOptions.Subsampling), sampling.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.YCbCr)]
    public void ConsumedFloatsAndOutOfRangeCoefficientsAreValidatedBeforeOutput(JpegColorSpace colorSpace)
    {
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -0.0001f, 1.0001f })
            foreach (int channel in new[] { 0, 1, 2 })
            {
                float[] pixels = (float[])Source(2, 17, 9, PixelComponentLayout.Rgb);
                pixels[pixels.Length - 3 + channel] = invalid;
                using var output = new MemoryStream();
                var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels, 17, 9,
                    PixelComponentLayout.Rgb, new() { ColorSpace = colorSpace }));
                Assert.Equal("pixels", error.ParamName);
                Assert.Equal(0, output.Length);
            }
        foreach ((bool progressive, bool optimize) in new[] { (false, false), (false, true), (true, false), (true, true) })
        {
            var options = new JpegliEncoderOptions
            {
                ColorSpace = colorSpace, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
                Distance = 0.1f, Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
                OptimizeHuffmanTables = optimize
            };
            float[] pixels = Enumerable.Repeat(-0.05f, 64).ToArray();
            using var output = new MemoryStream();
            var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels, 8, 8, PixelComponentLayout.Gray, options));
            Assert.Equal("pixels", error.ParamName);
            Assert.Equal(0, output.Length);
            JpegliEncoderOptions coarse = options with { Distance = 30f };
            JpegDocument document = Read(Encode(pixels, 8, 8, PixelComponentLayout.Gray, coarse), coarse);
            foreach ((BlockKey key, Block block) in document.Blocks)
            {
                int quantizer = document.Quantizers[document.Components[key.Component].Quantizer][0];
                int dc = colorSpace == JpegColorSpace.YCbCr && key.Component != 1 ? 0
                    : (int)Math.Round(8 * (-0.05 * 255 - 128) / quantizer, MidpointRounding.AwayFromZero);
                Assert.Equal(dc, block.Coefficients[0]);
                Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
            }
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.YCbCr)]
    public void MetadataPreservesCodingAndRespectsColorSpaceHeader(JpegColorSpace colorSpace)
    {
        byte[] pixels = (byte[])Source(0, 19, 17, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] original = Encode(pixels, 19, 17, PixelComponentLayout.Rgb, options);
        var enriched = options with
        {
            ExifData = new byte[] { 0x49, 0x49, 0x2a, 0, 8, 0, 0, 0 },
            XmpPacket = "<x:xmpmeta xmlns:x='adobe:ns:meta/'/>", IccProfile = new byte[] { 1, 3, 5, 7 },
            IptcData = new byte[] { 0x1c, 2, 5, 0, 1, 65 }, Comment = "RGB and grayscale metadata ✓"
        };
        byte[] jpeg = Encode(pixels, 19, 17, PixelComponentLayout.Rgb, enriched);
        List<Marker> markers = Markers(jpeg);
        Assert.Equal(colorSpace == JpegColorSpace.Rgb ? 0 : 1, markers.Count(marker => marker.Code == 0xe0));
        Assert.DoesNotContain(markers, marker => marker.Code == 0xee);
        Assert.Equal(2, markers.Count(marker => marker.Code == 0xe1));
        Assert.Equal(1, markers.Count(marker => marker.Code == 0xe2));
        Assert.Equal(1, markers.Count(marker => marker.Code == 0xed));
        Assert.Equal(Encoding.UTF8.GetBytes(enriched.Comment!), Assert.Single(markers, marker => marker.Code == 0xfe).Payload);
        AssertCoefficients(Read(original, options), Read(jpeg, enriched));
        Assert.Equal(ImageResult.FromMemory(original, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
    }

    private static Array Source(int kind, int width, int height, PixelComponentLayout layout)
    {
        int channels = Channels(layout);
        Array result = Allocate(kind, width * height * channels);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int component = 0; component < Math.Min(3, channels); component++)
                {
                    int value = kind == 0 ? 7 + (19 * x + 37 * y + 53 * component + 11 * x * y) % 241
                        : 4000 + (977 * x + 1703 * y + 11007 * component + 59 * x * y) % 55000;
                    Set(result, (y * width + x) * channels + component, value);
                }
        return result;
    }

    private static Array Allocate(int kind, int length) => kind switch
    {
        0 => Enumerable.Repeat(byte.MaxValue, length).ToArray(),
        1 => Enumerable.Repeat(ushort.MaxValue, length).ToArray(),
        _ => Enumerable.Repeat(float.NaN, length).ToArray()
    };

    private static Array Repack(Array rgb, int width, int height, PixelComponentLayout layout)
    {
        int kind = rgb is byte[] ? 0 : rgb is ushort[] ? 1 : 2;
        int channels = Channels(layout);
        bool bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        Array result = Allocate(kind, width * height * channels);
        for (int pixel = 0; pixel < width * height; pixel++)
            for (int component = 0; component < 3; component++)
                Array.Copy(rgb, pixel * 3 + component, result,
                    pixel * channels + (bgr ? 2 - component : component), 1);
        return result;
    }

    private static int Channels(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static void Set(Array array, int offset, int value)
    {
        if (array is byte[] bytes)
            bytes[offset] = (byte)value;
        else if (array is ushort[] words)
            words[offset] = (ushort)value;
        else
            ((float[])array)[offset] = value / 65535f;
    }

    private static double Sample(Array array, int offset) => array switch
    {
        byte[] bytes => bytes[offset],
        ushort[] words => words[offset] / 257d,
        _ => ((float[])array)[offset] * 255d
    };

    // Mathematical oracle: edge extension, 3x3 smoothing, box filtering,
    // and direct orthonormal DCT. No production helper is called.
    private static double[] OracleSamples(Array pixels, int width, int height, BlockKey key,
        int component, int areaWidth, int areaHeight, int smoothing, bool grayscale)
    {
        var result = new double[64];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                double sum = 0;
                for (int dy = 0; dy < areaHeight; dy++)
                    for (int dx = 0; dx < areaWidth; dx++)
                    {
                        int column = (key.X * 8 + x) * areaWidth + dx;
                        int row = (key.Y * 8 + y) * areaHeight + dy;
                        double neighbors = 0;
                        for (int ny = -1; ny <= 1; ny++)
                            for (int nx = -1; nx <= 1; nx++)
                                if (nx != 0 || ny != 0)
                                    neighbors += Raw(column + nx, row + ny);
                        sum += Raw(column, row) * (1 - smoothing / 128d) + neighbors * smoothing / 1024d;
                    }
                result[y * 8 + x] = sum / (areaWidth * areaHeight) - 128;
            }
        return result;

        double Raw(int column, int row)
        {
            int offset = (Math.Clamp(row, 0, height - 1) * width + Math.Clamp(column, 0, width - 1)) * 3;
            return grayscale ? 0.299 * Sample(pixels, offset) + 0.587 * Sample(pixels, offset + 1)
                + 0.114 * Sample(pixels, offset + 2) : Sample(pixels, offset + component);
        }
    }

    private static double OracleDct(double[] samples, int u, int v) =>
        IndependentDct.Coefficient(samples, u, v);

    private static byte[] ExpectedRgbTable(float distance)
    {
        float linearQuality = distance <= 0.1f ? 1f
            : distance <= 4.6f ? (200f / 9f) * (distance - 0.1f)
            : distance <= 6.4f ? 5000f / (100f - (distance - 0.1f) / 0.09f)
            : distance < 25f ? 530000f / (3450f - 300f * MathF.Sqrt((848f * distance - 5330f) / 120f))
            : 5000f;
        float scale = 0.01f * linearQuality;
        byte[] result = new byte[64];
        for (int index = 0; index < result.Length; index++)
            result[index] = (byte)Math.Clamp(MathF.Round(scale * Luminance[Zigzag[index]], MidpointRounding.AwayFromZero), 1, 255);
        return result;
    }

    // Independent marker traversal, including stuffed entropy bytes and RSTs.
    private sealed record Marker(int Code, byte[] Payload);

    private static List<Marker> Markers(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        var result = new List<Marker>();
        int position = 2;
        while (position < jpeg.Length)
        {
            Assert.Equal(0xff, jpeg[position++]);
            int marker = jpeg[position++];
            if (marker == 0xd9)
            {
                Assert.Equal(jpeg.Length, position);
                return result;
            }
            int length = jpeg[position] * 256 + jpeg[position + 1];
            int end = position + length;
            Assert.InRange(length, 2, jpeg.Length - position);
            result.Add(new(marker, jpeg[(position + 2)..end]));
            position = end;
            if (marker != 0xda)
                continue;
            while (true)
            {
                if (jpeg[position++] != 0xff)
                    continue;
                int next = jpeg[position++];
                if (next == 0 || next is >= 0xd0 and <= 0xd7)
                    continue;
                position -= 2;
                break;
            }
        }
        throw new InvalidDataException("Missing end-of-image marker.");
    }

    private static IEnumerable<int> HuffmanSelectors(List<Marker> markers)
    {
        foreach (Marker marker in markers.Where(marker => marker.Code == 0xc4))
        {
            int position = 0;
            while (position < marker.Payload.Length)
            {
                int selector = marker.Payload[position++];
                int symbols = 0;
                for (int bits = 0; bits < 16; bits++)
                    symbols += marker.Payload[position++];
                position += symbols;
                Assert.InRange(position, 0, marker.Payload.Length);
                yield return selector;
            }
        }
    }

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0) => pixels switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, ByteLayout(layout), options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])pixels, width, height, layout, options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0)
    {
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, ByteLayout(layout), options, stride, TestContext.Current.CancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, TestContext.Current.CancellationToken);
    }

    private static PixelLayout ByteLayout(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Rgb => PixelLayout.Rgb24,
        PixelComponentLayout.Bgr => PixelLayout.Bgr24,
        PixelComponentLayout.Rgba => PixelLayout.Rgba32,
        PixelComponentLayout.Bgra => PixelLayout.Bgra32,
        PixelComponentLayout.Gray => PixelLayout.Gray8,
        _ => PixelLayout.YCbCr24
    };

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Quantizers.Keys, actual.Quantizers.Keys);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach (int id in expected.Quantizers.Keys)
            Assert.Equal(expected.Quantizers[id], actual.Quantizers[id]);
        foreach ((BlockKey key, Block block) in expected.Blocks)
        {
            Block other = actual.Blocks[key];
            Assert.Equal(block.IsDummy, other.IsDummy);
            if (!block.IsDummy)
                Assert.Equal(block.Coefficients, other.Coefficients);
            else
                Assert.All(other.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
        }
    }

    private sealed class WriteOnlyStream(Stream destination) : Stream
    {
        public bool Closed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => destination.Flush();
        public override void Write(byte[] buffer, int offset, int count) => destination.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => destination.Write(buffer);
        public override void WriteByte(byte value) => destination.WriteByte(value);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { Closed = true; base.Dispose(disposing); }
    }
}
