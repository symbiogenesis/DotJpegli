using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class SubsampledLumaTests
{
    public static TheoryData<int, int, int, int, int, int> SamplingModes => new()
    {
        { 1, 1, 2, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 2 },
        { 1, 1, 2, 1, 1, 2 }, // Horizontal and vertical maxima belong to different components.
        { 1, 1, 2, 2, 1, 1 },
        { 2, 1, 1, 2, 1, 1 },
        { 1, 2, 2, 1, 1, 1 },
        { 1, 1, 3, 2, 1, 1 },
        { 1, 1, 1, 1, 2, 3 },
        { 1, 1, 3, 1, 1, 3 },
        { 1, 1, 4, 1, 1, 4 }, // A 32x32 MCU with nine actual blocks, rather than sixteen luma blocks.
        { 2, 1, 4, 1, 2, 2 }, // Exactly ten blocks; luma averages a 2x2 sample area.
        { 1, 2, 2, 2, 1, 4 },
        { 1, 1, 1, 1, 4, 2 }
    };

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void ComponentMaximaDetermineMcuGeometryAndPreserveCoefficientsAcrossScans(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 25), (145, 49) })
            foreach ((bool optimize, int smoothing) in new[] { (false, 0), (true, 73) })
            {
                byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
                var options = new JpegliEncoderOptions
                {
                    Quality = 90, SamplingFactors = sampling,
                    OptimizeHuffmanTables = optimize, SmoothingFactor = smoothing
                };
                byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
                JpegDocument reference = Read(baseline, options);
                AssertSamplingAndDummyDc(reference, sampling, interval: 0, refinement: 0);
                foreach ((bool progressive, int refinement, int interval) in new[]
                {
                    (false, 0, 2), (true, 0, 1), (true, 2, 7), (true, 10, 2)
                })
                {
                    var actualOptions = options with
                    {
                        Progressive = progressive, ProgressiveRefinementBits = refinement,
                        RestartInterval = interval, OptimizeHuffmanTables = !optimize
                    };
                    byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, actualOptions);
                    JpegDocument actual = Read(jpeg, actualOptions);
                    // This reader independently checks scan unit counts, every
                    // restart marker (including RST7 wraparound), and entropy padding.
                    AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
                    AssertSamplingAndDummyDc(actual, sampling, interval, refinement);
                }
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void SubsampledLumaAcMatchesIndependentSmoothedBoxAverageAndCosineDct(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 35, height = 27;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        int maxH = Math.Max(yh, Math.Max(cbh, crh)), maxV = Math.Max(yv, Math.Max(cbv, crv));
        int sampleWidth = maxH / yh, sampleHeight = maxV / yv;
        foreach (int smoothing in new[] { 0, 37, 100 })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 100, SamplingFactors = sampling, SmoothingFactor = smoothing,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            JpegDocument actual = Read(Encode(pixels, width, height, PixelLayout.Rgb24, options), options);
            byte[] quantizer = actual.Quantizers[actual.Components[1].Quantizer];
            int substantialAc = 0;
            foreach ((BlockKey key, Block block) in actual.Blocks.Where(pair => pair.Key.Component == 1 && !pair.Value.IsDummy))
            {
                var samples = new double[64];
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                    {
                        double sum = 0;
                        for (int dy = 0; dy < sampleHeight; dy++)
                            for (int dx = 0; dx < sampleWidth; dx++)
                                sum += SmoothedComponent(actual, index => pixels[index], 1,
                                    (key.X * 8 + x) * sampleWidth + dx,
                                    (key.Y * 8 + y) * sampleHeight + dy, smoothing);
                        samples[y * 8 + x] = sum / (sampleWidth * sampleHeight);
                    }
                for (int zigzag = 1; zigzag < 64; zigzag++)
                {
                    int natural = Zigzag[zigzag];
                    int u = natural % 8, v = natural / 8;
                    double sum = 0;
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            sum += samples[y * 8 + x] * Math.Cos((2 * x + 1) * u * Math.PI / 16)
                                * Math.Cos((2 * y + 1) * v * Math.PI / 16);
                    double expected = sum * 0.25 * (u == 0 ? 1 / Math.Sqrt(2) : 1)
                        * (v == 0 ? 1 / Math.Sqrt(2) : 1) / quantizer[zigzag];
                    // Nonadaptive color AC may zero values below its 0.59082
                    // dead zone; other values round to the nearest integer.
                    Assert.InRange(Math.Abs(block.Coefficients[zigzag] - expected), 0, 0.601);
                    if (Math.Abs(block.Coefficients[zigzag]) >= 2)
                        substantialAc++;
                }
            }
            Assert.True(substantialAc >= 8, "The oracle must cover substantial nonzero luma AC coefficients.");
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentBoxAverageAndSmoothingDcOracleIncludesSubsampledLumaAndEdges(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        JpegSamplingFactors sampling = Factors(yh, yv, cbh, cbv, crh, crv);
        foreach ((int width, int height) in new[] { (1, 33), (33, 1), (35, 27) })
            foreach (int smoothing in new[] { 0, 37, 100 })
            {
                byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
                var options = new JpegliEncoderOptions
                {
                    Quality = 95, SamplingFactors = sampling, SmoothingFactor = smoothing,
                    Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
                };
                JpegDocument actual = Read(Encode(pixels, width, height, PixelLayout.Rgb24, options), options);
                AssertIndependentDc(actual, index => pixels[index], smoothing);
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void SubsampledLumaAutomaticallyDisablesAdaptiveQuantizationInEveryScanMode(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 35, height = 27;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        foreach (bool optimize in new[] { false, true })
            foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
            {
                var options = new JpegliEncoderOptions
                {
                    Quality = 85, SamplingFactors = Factors(yh, yv, cbh, cbv, crh, crv),
                    AdaptiveQuantization = false, SmoothingFactor = 37, OptimizeHuffmanTables = optimize,
                    Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
                };
                Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options),
                    Encode(pixels, width, height, PixelLayout.Rgb24, options with { AdaptiveQuantization = true }));
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void PaddedByteWordAndFractionalFloatInputsPreserveSamplingAndScanParity(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 17, height = 25, stride = width * 4 + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * 4).ToArray();
        float[] floats = compact.Select((value, index) => (value + index % 7 / 8f) / 255f).ToArray();
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
        JpegDocument? floatReference = null;
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SamplingFactors = sampling, SmoothingFactor = 73,
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
            Assert.Equal(floatExpected, JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgb,
                options with { AdaptiveQuantization = false }, cancellationToken: TestContext.Current.CancellationToken));
            JpegDocument actual = Read(floatExpected, options);
            AssertSamplingAndDummyDc(actual, sampling, interval: 2, refinement);
            AssertIndependentDc(actual, index => floats[index] * 255d, options.SmoothingFactor);
            if (floatReference is null)
                floatReference = actual;
            else
                AssertRealCoefficients(floatReference, actual);
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void GrayscaleIgnoresValidColorFactorsAndRetainsAdaptiveQuantization(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 35, height = 27;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        ushort[] words = pixels.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = pixels.Select((value, index) => (value + index % 7 / 8f) / 255f).ToArray();
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 85, Progressive = progressive, ProgressiveRefinementBits = refinement,
                SmoothingFactor = 37, RestartInterval = 2
            };
            var custom = options with { SamplingFactors = Factors(yh, yv, cbh, cbv, crh, crv) };
            byte[] expected = Encode(pixels, width, height, PixelLayout.Gray8, options);
            Assert.NotEqual(expected, Encode(pixels, width, height, PixelLayout.Gray8,
                options with { AdaptiveQuantization = false }));
            Assert.Equal(expected, Encode(pixels, width, height, PixelLayout.Gray8, custom));
            Assert.Equal(expected, JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Gray,
                custom, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Gray,
                options, cancellationToken: TestContext.Current.CancellationToken),
                JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Gray,
                    custom, cancellationToken: TestContext.Current.CancellationToken));
            JpegDocument actual = Read(expected, options);
            Component component = Assert.Single(actual.Components).Value;
            Assert.Equal((1, 1), (component.H, component.V));
            Assert.DoesNotContain(actual.Blocks.Values, block => block.IsDummy);
        }
    }

    [Fact]
    public void FractionalRatiosToComponentMaximaEncodeAndAreIgnoredForGrayscale()
    {
        foreach (JpegSamplingFactors sampling in new[]
        {
            Factors(2, 1, 3, 1, 1, 1), Factors(1, 2, 1, 1, 1, 3),
            Factors(1, 1, 2, 1, 3, 1), Factors(1, 1, 1, 3, 1, 2),
            Factors(1, 1, 4, 1, 3, 1), Factors(1, 1, 1, 4, 1, 3)
        })
            foreach (bool grayscale in new[] { false, true })
            {
                int channels = grayscale ? 1 : 3;
                PixelLayout byteLayout = grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24;
                PixelComponentLayout typedLayout = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
                var options = new JpegliEncoderOptions { SamplingFactors = sampling };
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                Action[] encoders =
                [
                    () => JpegliEncoder.Encode(output, new byte[channels], 1, 1, byteLayout, options,
                        cancellationToken: TestContext.Current.CancellationToken),
                    () => JpegliEncoder.Encode(output, new ushort[channels], 1, 1, typedLayout, options,
                        cancellationToken: TestContext.Current.CancellationToken),
                    () => JpegliEncoder.Encode(output, new float[channels], 1, 1, typedLayout, options,
                        cancellationToken: TestContext.Current.CancellationToken)
                ];
                foreach (Action encode in encoders)
                {
                    destination.SetLength(0);
                    destination.Position = 0;
                    encode();
                    byte[] jpeg = destination.ToArray();
                    JpegDocument document = Read(jpeg, options);
                    Assert.Equal(grayscale ? 1 : 3, document.Components.Count);
                    if (grayscale)
                        Assert.Equal(Encode(new byte[1], 1, 1, PixelLayout.Gray8, new()), jpeg);
                    else
                        for (int id = 1; id <= 3; id++)
                            Assert.Equal((sampling[id - 1].Horizontal, sampling[id - 1].Vertical),
                                (document.Components[id].H, document.Components[id].V));
                    Assert.False(output.Closed);
                }
            }
    }

    private static void AssertIndependentDc(JpegDocument document, Func<int, double> sample, int smoothing)
    {
        int maxH = document.Components.Values.Max(component => component.H);
        int maxV = document.Components.Values.Max(component => component.V);
        foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            Component component = document.Components[key.Component];
            int sampleWidth = maxH / component.H, sampleHeight = maxV / component.V;
            double sum = 0;
            for (int y = key.Y * 8 * sampleHeight; y < (key.Y + 1) * 8 * sampleHeight; y++)
                for (int x = key.X * 8 * sampleWidth; x < (key.X + 1) * 8 * sampleWidth; x++)
                    sum += SmoothedComponent(document, sample, key.Component, x, y, smoothing);
            int quantizer = document.Quantizers[component.Quantizer][0];
            double expected = sum / (8 * sampleWidth * sampleHeight * quantizer);
            // The DC coefficient is eight times the centered block mean.
            // Permit half ties and the encoder's float arithmetic.
            Assert.InRange(Math.Abs(block.Coefficients[0] - expected), 0, 0.501);
        }
    }

    private static double SmoothedComponent(JpegDocument document, Func<int, double> sample,
        int component, int x, int y, int smoothing)
    {
        double center = Converted(x, y);
        if (smoothing == 0)
            return center;
        double neighbors = 0;
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                if (dx != 0 || dy != 0)
                    neighbors += Converted(x + dx, y + dy);
        return center * (1 - 8 * smoothing / 1024d) + neighbors * smoothing / 1024d;

        double Converted(int column, int row)
        {
            int offset = (Math.Clamp(row, 0, document.Height - 1) * document.Width
                + Math.Clamp(column, 0, document.Width - 1)) * 3;
            double red = sample(offset), green = sample(offset + 1), blue = sample(offset + 2);
            double luma = 0.299 * red + 0.587 * green + 0.114 * blue;
            return component switch
            {
                1 => luma - 128,
                2 => (blue - luma) / 1.772,
                _ => (red - luma) / 1.402
            };
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
            Assert.Equal(realColumns * realRows,
                document.Blocks.Count(pair => pair.Key.Component == id && !pair.Value.IsDummy));
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

    private static void AssertRealCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Quantizers.Count, actual.Quantizers.Count);
        foreach ((int selector, byte[] quantizer) in expected.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in expected.Blocks)
        {
            Assert.Equal(block.IsDummy, actual.Blocks[key].IsDummy);
            if (!block.IsDummy)
                Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        }
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static JpegSamplingFactors Factors(int yh, int yv, int cbh, int cbv, int crh, int crv) =>
        new(new(yh, yv), new(cbh, cbv), new(crh, crv));

    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;

    private static ReadOnlySpan<int> Zigzag =>
    [
        0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
    ];
}
