using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class CustomEncoderSamplingTests
{
    public static TheoryData<int, int, int, int, int, int> SamplingModes => new()
    {
        { 2, 2, 2, 1, 1, 1 },
        { 4, 1, 2, 1, 1, 1 },
        { 1, 3, 1, 3, 1, 1 },
        { 2, 1, 2, 1, 2, 1 },
        { 2, 2, 2, 2, 1, 2 }, // Exactly ten blocks in each interleaved MCU.
        { 2, 2, 1, 2, 2, 1 }
    };

    public static TheoryData<ChromaSubsampling, int, int> PresetModes => new()
    {
        { ChromaSubsampling.None, 1, 1 },
        { ChromaSubsampling.HalfHorizontal, 2, 1 },
        { ChromaSubsampling.Quarter, 2, 2 },
        { ChromaSubsampling.HalfVertical, 1, 2 },
        { ChromaSubsampling.QuarterHorizontal, 4, 1 },
        { ChromaSubsampling.ThirdHorizontal, 3, 1 },
        { ChromaSubsampling.ThirdHorizontalHalfVertical, 3, 2 },
        { ChromaSubsampling.QuarterVertical, 1, 4 },
        { ChromaSubsampling.QuarterHorizontalHalfVertical, 4, 2 },
        { ChromaSubsampling.HalfHorizontalQuarterVertical, 2, 4 },
        { ChromaSubsampling.ThirdVertical, 1, 3 },
        { ChromaSubsampling.HalfHorizontalThirdVertical, 2, 3 }
    };

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void AsymmetricAndMultipleChromaBlocksPreserveCoefficientsAcrossScans(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        // Narrow and odd edges create wholly padded blocks in chroma as well
        // as luma. The largest case wraps RST7 for every sampling scheme.
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 25), (145, 49) })
            foreach ((bool optimize, bool adaptive, int smoothing) in new[]
            {
                (false, false, 0), (true, true, 73)
            })
            {
                byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
                var options = new JpegliEncoderOptions
                {
                    Quality = 90, SamplingFactors = sampling, Subsampling = ChromaSubsampling.Quarter,
                    OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive, SmoothingFactor = smoothing
                };
                byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
                JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
                AssertSamplingAndDummyDc(reference, sampling, interval: 0, refinement: 0);

                foreach ((bool progressive, int refinement, int interval) in new[]
                {
                    (false, 0, 2), (true, 0, 1), (true, 2, 7), (true, 10, 2)
                })
                {
                    byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
                    {
                        Progressive = progressive, ProgressiveRefinementBits = refinement,
                        RestartInterval = interval, OptimizeHuffmanTables = !optimize
                    });
                    // The independent reader also checks scan block counts,
                    // restart cycling, predictor resets and entropy padding.
                    JpegDocument actual = JpegDocument.Read(jpeg, progressive, interval, refinement);
                    AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
                    AssertSamplingAndDummyDc(actual, sampling, interval, refinement);
                }
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentComponentBoxAverageDcOracleIncludesReplicatedEdges(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 35, height = 27;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach (bool adaptive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SamplingFactors = sampling, AdaptiveQuantization = adaptive,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            JpegDocument document = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: 2, expectedRefinementBits: 2);
            AssertSamplingAndDummyDc(document, sampling, interval: 2, refinement: 2);
            AssertIndependentDc(document, index => pixels[index]);
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void AsymmetricFactorsUseTheSameQuantizationTablesAsFullResolutionColor(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 17, height = 25;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach (int quality in new[] { 10, 90, 100 })
            foreach (bool adaptive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions
                {
                    Quality = quality, Subsampling = ChromaSubsampling.None, AdaptiveQuantization = adaptive
                };
                JpegDocument reference = JpegDocument.Read(
                    Encode(pixels, width, height, PixelLayout.Rgb24, options),
                    progressive: false, expectedInterval: 0);
                JpegDocument actual = JpegDocument.Read(
                    Encode(pixels, width, height, PixelLayout.Rgb24, options with
                    {
                        SamplingFactors = sampling, Subsampling = ChromaSubsampling.Quarter
                    }), progressive: false, expectedInterval: 0);
                // 4:2:0 table compensation applies to the complete sampling
                // scheme, even when one chroma component has a 2x2 ratio.
                Assert.Equal(reference.Quantizers.Count, actual.Quantizers.Count);
                foreach ((int selector, byte[] quantizer) in reference.Quantizers)
                    Assert.Equal(quantizer, actual.Quantizers[selector]);
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void PaddedByteWordAndFractionalFloatInputsPreserveCustomSampling(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 17, height = 25, stride = width * 4 + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * 4).ToArray();
        float[] floats = compact.Select((value, index) => (value + (index % 7) / 8f) / 255f).ToArray();
        float[] paddedFloats = Enumerable.Repeat(float.NaN, padded.Length).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * stride + x * 4;
                padded[output] = compact[input + 2];
                padded[output + 1] = compact[input + 1];
                padded[output + 2] = compact[input];
                padded[output + 3] = (byte)(17 * x + 29 * y);
                paddedFloats[output] = floats[input + 2];
                paddedFloats[output + 1] = floats[input + 1];
                paddedFloats[output + 2] = floats[input];
            }
        ushort[] words = padded.Select(value => (ushort)(value * 257)).ToArray();
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                SamplingFactors = sampling, SmoothingFactor = 73,
                Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, PixelLayout.Rgb24, options);
            Assert.Equal(expected, JpegliEncoder.Encode(padded, width, height, PixelLayout.Bgra32,
                options, stride, TestContext.Current.CancellationToken));
            Assert.Equal(expected, JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken));
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            JpegliEncoder.Encode(output, words, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken);
            Assert.False(output.Closed);
            Assert.Equal(expected, destination.ToArray());

            byte[] floatExpected = JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgb,
                options, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(floatExpected, JpegliEncoder.Encode(paddedFloats, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken));
            JpegDocument floatDocument = JpegDocument.Read(floatExpected, progressive, expectedInterval: 2, refinement);
            AssertSamplingAndDummyDc(floatDocument, sampling, interval: 2, refinement);
        }

        var floatOptions = new JpegliEncoderOptions { Quality = 90, SamplingFactors = sampling };
        byte[] floatBaseline = JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgb,
            floatOptions, cancellationToken: TestContext.Current.CancellationToken);
        byte[] floatProgressive = JpegliEncoder.Encode(paddedFloats, width, height, PixelComponentLayout.Bgra,
            floatOptions with { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 },
            stride, TestContext.Current.CancellationToken);
        JpegDocument floatReference = JpegDocument.Read(floatBaseline, progressive: false, expectedInterval: 0);
        JpegDocument floatActual = JpegDocument.Read(floatProgressive, progressive: true,
            expectedInterval: 2, expectedRefinementBits: 2);
        AssertBaselineParity(floatBaseline, floatReference, floatProgressive, floatActual, PixelLayout.Rgb24);
        AssertIndependentDc(floatActual, index => floats[index] * 255d);
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void FlatMultiBlockChromaEobRunsRestartAtEveryScanBoundary(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 145, height = 33;
        byte[] pixels = FlatPixels(width, height, PixelLayout.Rgb24);
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach (bool optimize in new[] { false, true })
            foreach (int interval in new[] { 0, 1, 7 })
            {
                byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, new()
                {
                    SamplingFactors = sampling, AdaptiveQuantization = false,
                    OptimizeHuffmanTables = optimize, Progressive = true,
                    ProgressiveRefinementBits = 2, RestartInterval = interval
                });
                JpegDocument document = JpegDocument.Read(jpeg, progressive: true,
                    expectedInterval: interval, expectedRefinementBits: 2);
                AssertSamplingAndDummyDc(document, sampling, interval, refinement: 2);
                foreach (Scan scan in document.Scans.Where(scan => scan.First != 0))
                {
                    int component = Assert.Single(scan.Selectors).Component;
                    int realBlocks = document.Blocks.Count(pair => pair.Key.Component == component && !pair.Value.IsDummy);
                    int limit = optimize ? (interval == 0 ? realBlocks : interval) : 1;
                    int[] expectedRuns = Enumerable.Range(0, Ceiling(realBlocks, limit))
                        .Select(index => Math.Min(limit, realBlocks - index * limit)).ToArray();
                    Assert.Equal(expectedRuns, scan.EobRuns);
                }
            }
    }

    [Theory]
    [MemberData(nameof(PresetModes))]
    public void ExplicitFactorsMatchExistingPresetBytesAndOverrideSubsampling(
        ChromaSubsampling preset, int horizontal, int vertical)
    {
        const int width = 17, height = 25;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegSamplingFactors sampling = Factors(horizontal, vertical, 1, 1, 1, 1);
        foreach (bool optimize in new[] { false, true })
            foreach ((bool progressive, int refinement, bool adaptive, int smoothing) in new[]
            {
                (false, 0, false, 0), (true, 0, true, 37), (true, 2, true, 100)
            })
            {
                var options = new JpegliEncoderOptions
                {
                    Subsampling = preset, OptimizeHuffmanTables = optimize,
                    Progressive = progressive, ProgressiveRefinementBits = refinement,
                    AdaptiveQuantization = adaptive, SmoothingFactor = smoothing, RestartInterval = 2
                };
                byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, options);
                byte[] actual = Encode(pixels, width, height, PixelLayout.Rgb24, options with
                {
                    SamplingFactors = sampling,
                    Subsampling = preset == ChromaSubsampling.None ? ChromaSubsampling.Quarter : ChromaSubsampling.None
                });
                Assert.Equal(expected, actual);
                JpegDocument document = JpegDocument.Read(actual, progressive, expectedInterval: 2, refinement);
                AssertSamplingAndDummyDc(document, sampling, interval: 2, refinement);
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void GrayscaleIgnoresValidCustomColorFactorsForEveryInputType(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 17, height = 25;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        ushort[] words = pixels.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = pixels.Select((value, index) => (value + index % 7 / 8f) / 255f).ToArray();
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Subsampling = ChromaSubsampling.None, Progressive = progressive,
                ProgressiveRefinementBits = refinement, SmoothingFactor = 73, RestartInterval = 2
            };
            var custom = options with { SamplingFactors = Factors(yh, yv, cbh, cbv, crh, crv) };
            byte[] expected = Encode(pixels, width, height, PixelLayout.Gray8, options);
            byte[] actual = Encode(pixels, width, height, PixelLayout.Gray8, custom);
            Assert.Equal(expected, actual);
            Assert.Equal(expected, JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Gray,
                custom, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Gray,
                options, cancellationToken: TestContext.Current.CancellationToken),
                JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Gray,
                    custom, cancellationToken: TestContext.Current.CancellationToken));
            JpegDocument document = JpegDocument.Read(actual, progressive, expectedInterval: 2, refinement);
            Component component = Assert.Single(document.Components).Value;
            Assert.Equal((1, 1), (component.H, component.V));
            Assert.DoesNotContain(document.Blocks.Values, block => block.IsDummy);
        }
    }

    [Fact]
    public void InvalidFactorsAreRejectedBeforeOutputForRgbAndGrayscaleInputs()
    {
        foreach (JpegSamplingFactors sampling in new[]
        {
            Factors(0, 1, 1, 1, 1, 1), Factors(1, 5, 1, 1, 1, 1),
            Factors(1, 1, -1, 1, 1, 1), Factors(1, 1, 1, 0, 1, 1),
            Factors(1, 1, 1, 1, 5, 1), Factors(1, 1, 1, 1, 1, -1),
            Factors(3, 3, 1, 1, 1, 1), Factors(2, 2, 2, 2, 2, 2)
        })
            foreach (bool grayscale in new[] { false, true })
            {
                var options = new JpegliEncoderOptions { SamplingFactors = sampling };
                PixelLayout byteLayout = grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24;
                PixelComponentLayout typedLayout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
                int channels = grayscale ? 1 : 3;
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                AssertInvalid(() => JpegliEncoder.Encode(output, new byte[channels], 1, 1,
                    byteLayout, options, cancellationToken: TestContext.Current.CancellationToken), destination);
                AssertInvalid(() => JpegliEncoder.Encode(output, new ushort[channels], 1, 1,
                    typedLayout, options, cancellationToken: TestContext.Current.CancellationToken), destination);
                AssertInvalid(() => JpegliEncoder.Encode(output, new float[channels], 1, 1,
                    typedLayout, options, cancellationToken: TestContext.Current.CancellationToken), destination);
                Assert.False(output.Closed);
            }
    }

    [Fact]
    public void InvalidPresetStillFailsValidationWhenCustomFactorsAreSupplied()
    {
        using var output = new MemoryStream();
        var options = new JpegliEncoderOptions
        {
            SamplingFactors = Factors(2, 2, 2, 1, 1, 1), Subsampling = (ChromaSubsampling)999
        };
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(output,
            new byte[3], 1, 1, options: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.Subsampling), error.ParamName);
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void PreCancellationWithValidCustomSamplingWritesNoOutput()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions { SamplingFactors = Factors(2, 2, 2, 1, 1, 1) };
        foreach (bool grayscale in new[] { false, true })
        {
            PixelLayout byteLayout = grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24;
            PixelComponentLayout typedLayout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
            int channels = grayscale ? 1 : 3;
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            Action[] encoders =
            [
                () => JpegliEncoder.Encode(output, new byte[channels], 1, 1, byteLayout, options,
                    cancellationToken: cancellation.Token),
                () => JpegliEncoder.Encode(output, new ushort[channels], 1, 1, typedLayout, options,
                    cancellationToken: cancellation.Token),
                () => JpegliEncoder.Encode(output, new float[channels], 1, 1, typedLayout, options,
                    cancellationToken: cancellation.Token)
            ];
            foreach (Action encode in encoders)
            {
                var error = Assert.Throws<OperationCanceledException>(encode);
                Assert.Equal(cancellation.Token, error.CancellationToken);
                Assert.Equal(0, destination.Length);
                Assert.False(output.Closed);
            }
        }
    }

    private static void AssertInvalid(Action encode, MemoryStream destination)
    {
        var error = Assert.ThrowsAny<ArgumentException>(encode);
        Assert.Equal(nameof(JpegliEncoderOptions.SamplingFactors), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    private static void AssertIndependentDc(JpegDocument document, Func<int, double> sample)
    {
        int maxH = document.Components.Values.Max(component => component.H);
        int maxV = document.Components.Values.Max(component => component.V);
        foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            Component component = document.Components[key.Component];
            int sampleWidth = maxH / component.H, sampleHeight = maxV / component.V;
            double sum = 0;
            for (int y = 0; y < 8 * sampleHeight; y++)
                for (int x = 0; x < 8 * sampleWidth; x++)
                {
                    int offset = (Math.Min(key.Y * 8 * sampleHeight + y, document.Height - 1) * document.Width
                        + Math.Min(key.X * 8 * sampleWidth + x, document.Width - 1)) * 3;
                    double red = sample(offset), green = sample(offset + 1), blue = sample(offset + 2);
                    double luma = 0.299 * red + 0.587 * green + 0.114 * blue;
                    sum += key.Component switch
                    {
                        1 => luma - 128,
                        2 => (blue - luma) / 1.772,
                        _ => (red - luma) / 1.402
                    };
                }
            int quantizer = document.Quantizers[component.Quantizer][0];
            double expected = sum / (8 * sampleWidth * sampleHeight * quantizer);
            // Permit exact half ties and float rounding in the encoder.
            Assert.InRange(Math.Abs(block.Coefficients[0] - expected), 0, 0.501);
        }
    }

    private static void AssertSamplingAndDummyDc(JpegDocument document, JpegSamplingFactors sampling,
        int interval, int refinement)
    {
        JpegComponentSampling[] factors = [sampling.Luma, sampling.Cb, sampling.Cr];
        Assert.Equal(3, document.Components.Count);
        int maxH = factors.Max(component => component.Horizontal);
        int maxV = factors.Max(component => component.Vertical);
        int columns = Ceiling(document.Width, 8 * maxH), rows = Ceiling(document.Height, 8 * maxV);
        Assert.Equal(columns * rows, document.McuCount);
        Assert.Equal(document.McuCount * factors.Sum(component => component.Horizontal * component.Vertical),
            document.Blocks.Count);
        for (int id = 1; id <= factors.Length; id++)
        {
            JpegComponentSampling factor = factors[id - 1];
            Assert.Equal((factor.Horizontal, factor.Vertical), (document.Components[id].H, document.Components[id].V));
            int realColumns = Ceiling(document.Width * factor.Horizontal, 8 * maxH);
            int realRows = Ceiling(document.Height * factor.Vertical, 8 * maxV);
            Assert.Equal(realColumns * realRows, document.Blocks.Count(pair => pair.Key.Component == id && !pair.Value.IsDummy));
            int previousDc = 0;
            for (int mcu = 0; mcu < document.McuCount; mcu++)
            {
                if (interval != 0 && mcu % interval == 0)
                    previousDc = 0;
                for (int y = 0; y < factor.Vertical; y++)
                    for (int x = 0; x < factor.Horizontal; x++)
                    {
                        int blockX = mcu % columns * factor.Horizontal + x;
                        int blockY = mcu / columns * factor.Vertical + y;
                        Block block = document.Blocks[new(id, blockX, blockY)];
                        Assert.Equal(blockX >= realColumns || blockY >= realRows, block.IsDummy);
                        Assert.Equal((block.Coefficients[0] >> refinement) - (previousDc >> refinement), block.DcDifference);
                        if (block.IsDummy)
                        {
                            Assert.Equal(0, block.DcDifference);
                            Assert.Equal(previousDc, block.Coefficients[0]);
                            Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                        }
                        previousDc = block.Coefficients[0];
                    }
            }
        }
    }

    private static JpegSamplingFactors Factors(int yh, int yv, int cbh, int cbv, int crh, int crv) =>
        new(new(yh, yv), new(cbh, cbv), new(crh, crv));

    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;
}
