using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

// StbImageSharp stores its failure reason in a process-wide static field.
// Exact rejection assertions must not overlap decoder calls in other tests.
[CollectionDefinition(nameof(StbFailureReasonCollection), DisableParallelization = true)]
public sealed class StbFailureReasonCollection;

[Collection(nameof(StbFailureReasonCollection))]
public sealed class FractionalEncoderSamplingTests
{
    public static TheoryData<int, int, int, int, int, int> SamplingModes => new()
    {
        { 3, 1, 2, 1, 1, 1 },
        { 4, 1, 3, 1, 1, 1 },
        { 1, 3, 1, 2, 1, 1 },
        { 1, 4, 1, 3, 1, 1 },
        { 3, 2, 2, 1, 1, 1 },
        { 2, 3, 1, 2, 1, 1 },
        { 3, 1, 2, 2, 1, 3 }, // Both ratios are fractional; exactly ten MCU blocks.
        { 2, 1, 3, 1, 1, 2 }  // The maximum factors belong to chroma components.
    };

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void FractionalGeometryAndRealCoefficientsSurviveScanModesAndRestartBoundaries(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        var sampling = new JpegSamplingFactors(new(yh, yv), new(cbh, cbv), new(crh, crv));
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (11, 13), (17, 19), (145, 49) })
            foreach (bool adaptive in new[] { false, true })
            {
                byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
                var options = new JpegliEncoderOptions
                {
                    Quality = 90, SamplingFactors = sampling, AdaptiveQuantization = adaptive,
                    SmoothingFactor = adaptive ? 73 : 0, OptimizeHuffmanTables = adaptive
                };
                JpegDocument baseline = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, options), false, 0);
                AssertGeometry(baseline, sampling, interleaved: true);

                foreach ((bool progressive, int refinement, bool separateDc) in new[]
                {
                    (false, 0, false), (true, 0, false), (true, 2, false), (true, 2, true)
                })
                {
                    var scanOptions = options with
                    {
                        Progressive = progressive, ProgressiveRefinementBits = refinement,
                        ProgressiveInterleaveDc = !separateDc, RestartInterval = 1,
                        OptimizeHuffmanTables = !options.OptimizeHuffmanTables
                    };
                    JpegDocument actual = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, scanOptions),
                        progressive, 1, refinement, expectedInterleaveDc: !separateDc);
                    AssertRealCoefficients(baseline, actual);
                    AssertGeometry(actual, sampling, interleaved: !separateDc);
                    if (width == 145)
                        Assert.Contains(actual.Scans, scan => scan.RestartMarkers.Count > 8);
                }

                JpegScan[] script = [new([2]), new([0, 1])];
                var grouped = options with { ScanScript = script, RestartRows = 1 };
                JpegDocument groupedDocument = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, grouped),
                    false, 0, expectedRestartRows: 1, expectedScanScript: script);
                AssertRealCoefficients(baseline, groupedDocument);
                AssertGeometry(groupedDocument, sampling, interleaved: false);
                foreach (Scan scan in groupedDocument.Scans)
                {
                    Component first = groupedDocument.Components[scan.Selectors[0].Component];
                    int columns = scan.Selectors.Count > 1 ? Ceiling(width, 8 * MaxHorizontal(sampling))
                        : Ceiling(width * first.H, 8 * MaxHorizontal(sampling));
                    Assert.Equal(columns, scan.RestartInterval);
                }
            }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentAreaAndCosineDctOracleChecksEveryRealCoefficient(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        var sampling = new JpegSamplingFactors(new(yh, yv), new(cbh, cbv), new(crh, crv));
        // Define the standard zigzag locally so this oracle has no production
        // sampling, transform, quantizer, or coefficient-order dependencies.
        int[] zigzag =
        [
            0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
            12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
        ];
        double[] zeroBias = [0.59082, 0.58146, 0.57988];
        foreach ((int width, int height) in new[] { (11, 13), (37, 29) })
        {
            byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
            var options = new JpegliEncoderOptions { Quality = 90, SamplingFactors = sampling, AdaptiveQuantization = false };
            JpegDocument document = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.YCbCr24, options), false, 0);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                Component component = document.Components[key.Component];
                double[] samples = FractionalDownsamplingTests.AreaBlock(
                    (x, y) => pixels[(y * width + x) * 3 + key.Component - 1],
                    width, height, key.X, key.Y, MaxHorizontal(sampling), MaxVertical(sampling), component.H, component.V);
                for (int index = 0; index < 64; index++)
                {
                    int u = zigzag[index] % 8, v = zigzag[index] / 8;
                    double coefficient = IndependentDct.Coefficient(samples, u, v);
                    double quantized = coefficient / document.Quantizers[component.Quantizer][index];
                    if (index != 0 && Math.Abs(quantized) < zeroBias[key.Component - 1] - 0.001)
                        Assert.Equal(0, block.Coefficients[index]);
                    else if (index == 0 || Math.Abs(quantized) > zeroBias[key.Component - 1] + 0.001)
                        // The tolerance admits float DCT rounding at exact half
                        // ties; it still detects a one-step error away from ties.
                        Assert.InRange(Math.Abs(block.Coefficients[index] - quantized), 0, 0.502);
                    else
                        Assert.InRange(Math.Abs(block.Coefficients[index]), 0, 1);
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentAreaDcOracleIncludesFractionalPhaseAndPaddedImageEdges(
        int yh, int yv, int cbh, int cbv, int crh, int crv)
    {
        const int width = 37, height = 29;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var sampling = new JpegSamplingFactors(new(yh, yv), new(cbh, cbv), new(crh, crv));
        foreach (bool adaptive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SamplingFactors = sampling, AdaptiveQuantization = adaptive,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            // Direct YCbCr lets the independent oracle measure area coverage
            // without depending on the encoder's RGB conversion arithmetic.
            JpegDocument document = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.YCbCr24, options), true, 2, 2);
            AssertGeometry(document, sampling, interleaved: true);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                Component component = document.Components[key.Component];
                double[] samples = FractionalDownsamplingTests.AreaBlock(
                    (x, y) => pixels[(y * width + x) * 3 + key.Component - 1],
                    width, height, key.X, key.Y, MaxHorizontal(sampling), MaxVertical(sampling), component.H, component.V);
                double dc = samples.Sum() / (8 * document.Quantizers[component.Quantizer][0]);
                Assert.InRange(Math.Abs(block.Coefficients[0] - dc), 0, 0.501);
            }
        }
    }

    [Theory]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    public void AdaptiveChromaColumnsUseFloorOfTheFractionalFullResolutionPosition(int maximum, int chroma)
    {
        const int width = 145, height = 33, quality = 85;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var sampling = new JpegSamplingFactors(new(maximum, 1), new(chroma, 1), new(1, 1));
        var options = new JpegliEncoderOptions { Quality = quality, SamplingFactors = sampling, RestartInterval = 2 };
        JpegDocument document = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, options), false, 2);
        var analysis = new AdaptiveQuantization(width, height, 1);
        float[] strengths = new float[analysis.BlocksWide];
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Quantization.CreateTable(quality, component)).ToArray();
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, false);
        float[] samples = new float[64], coefficients = new float[64];
        short[] expected = new short[64], alternative = new short[64];
        int sensitive = 0;
        for (int blockY = 0; blockY < Ceiling(height, 8); blockY++)
        {
            analysis.ComputeRow(pixels, width * 3, 3, PixelLayout.Rgb24, blockY * 8, tables[0][1],
                strengths, TestContext.Current.CancellationToken);
            for (int blockX = 0; blockX < Ceiling(width * chroma, 8 * maximum); blockX++)
            {
                Downsampling.FillBlock(new PixelSource(pixels, PixelLayout.Rgb24), width, height, width * 3,
                    1, blockX, blockY, maximum, 1, chroma, 1, samples);
                ForwardDct.Transform(samples, coefficients);
                int column = blockX * maximum / chroma;
                Quantization.Quantize(coefficients, tables[1], expected, 1, Quantization.QualityToDistance(quality),
                    adaptiveQuantization: true, adaptiveStrength: strengths[column], adaptiveMultipliers: multipliers[1]);
                Block actual = document.Blocks[new(2, blockX, blockY)];
                for (int index = 1; index < 64; index++)
                    Assert.Equal(expected[JpegConstants.ZigZag[index]], actual.Coefficients[index]);

                // Truncating the ratio before multiplying loses within-MCU
                // phase and eventually points at a different analysis column.
                int alternativeColumn = blockX * (maximum / chroma);
                Quantization.Quantize(coefficients, tables[1], alternative, 1, Quantization.QualityToDistance(quality),
                    adaptiveQuantization: true, adaptiveStrength: strengths[alternativeColumn], adaptiveMultipliers: multipliers[1]);
                if (!expected.AsSpan(1).SequenceEqual(alternative.AsSpan(1))) sensitive++;
            }
        }
        Assert.True(sensitive > 0, "The fixture must distinguish truncation before scaling.");
    }

    [Fact]
    public void PaddedByteWordFloatAndNonseekableStreamInputsKeepFractionalSampling()
    {
        const int width = 17, height = 25, stride = width * 4 + 7;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * 4).ToArray();
        float[] floats = compact.Select((value, index) => (value + index % 7 / 8f) / 255f).ToArray();
        float[] paddedFloats = Enumerable.Repeat(float.NaN, padded.Length).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * stride + x * 4;
                for (int component = 0; component < 3; component++)
                {
                    padded[output + 2 - component] = compact[input + component];
                    paddedFloats[output + 2 - component] = floats[input + component];
                }
                padded[output + 3] = (byte)(17 * x + 29 * y);
            }
        ushort[] words = padded.Select(value => (ushort)(value * 257)).ToArray();
        var sampling = new JpegSamplingFactors(new(3, 1), new(2, 2), new(1, 3));
        foreach (bool progressive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                SamplingFactors = sampling, SmoothingFactor = 73, Progressive = progressive,
                ProgressiveRefinementBits = progressive ? 2 : 0, RestartRows = 1
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
            JpegDocument document = JpegDocument.Read(floatExpected, progressive, 0, progressive ? 2 : 0, expectedRestartRows: 1);
            AssertGeometry(document, sampling, interleaved: true);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void IndependentEntropyReaderVerifiesFractionalFramesRejectedByStbResampling(bool vertical, bool progressive)
    {
        const int width = 17, height = 19;
        var sampling = vertical ? new JpegSamplingFactors(new(1, 3), new(1, 2), new(1, 1))
            : new JpegSamplingFactors(new(3, 1), new(2, 1), new(1, 1));
        var options = new JpegliEncoderOptions { SamplingFactors = sampling, Progressive = progressive, RestartInterval = 2 };
        byte[] jpeg = Encode(Pattern(width, height, PixelLayout.Rgb24), width, height, PixelLayout.Rgb24, options);
        AssertGeometry(JpegDocument.Read(jpeg, progressive, 2), sampling, interleaved: true);
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue));
        Assert.Contains(vertical ? "bad V" : "bad H", exception.Message);
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Xyb)]
    public void RgbAndXybFramesPreserveFractionalChannelCoefficientsAcrossScans(JpegColorSpace colorSpace)
    {
        const int width = 37, height = 29;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        // Green/scaled Y stays at full resolution for adaptive analysis while
        // the first component has a fractional 3:2 sampling ratio.
        var sampling = new JpegSamplingFactors(new(2, 1), new(3, 1), new(1, 1));
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, SamplingFactors = sampling, Quality = 90,
            IccProfile = colorSpace == JpegColorSpace.Xyb ? JpegliIccProfiles.CreateXybProfile() : default
        };
        JpegDocument baseline = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, options), false, 0);
        Assert.Equal(new[] { 82, 71, 66 }, baseline.Components.Keys);
        AssertGeometry(baseline, sampling, interleaved: true);
        var progressive = options with { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 };
        JpegDocument actual = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, progressive), true, 2, 2);
        AssertRealCoefficients(baseline, actual);
        AssertGeometry(actual, sampling, interleaved: true);
        JpegScan[] script = [new([2]), new([0, 1])];
        var grouped = options with { ScanScript = script, RestartRows = 1 };
        JpegDocument separate = JpegDocument.Read(Encode(pixels, width, height, PixelLayout.Rgb24, grouped), false, 0,
            expectedRestartRows: 1, expectedScanScript: script);
        AssertRealCoefficients(baseline, separate);
        AssertGeometry(separate, sampling, interleaved: false);
    }

    private static void AssertRealCoefficients(JpegDocument reference, JpegDocument actual)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] table) in reference.Quantizers) Assert.Equal(table, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count(pair => !pair.Value.IsDummy), actual.Blocks.Count(pair => !pair.Value.IsDummy));
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
    }

    private static void AssertGeometry(JpegDocument document, JpegSamplingFactors sampling, bool interleaved)
    {
        JpegComponentSampling[] factors = [sampling.Luma, sampling.Cb, sampling.Cr];
        int maxH = MaxHorizontal(sampling), maxV = MaxVertical(sampling);
        int mcuColumns = Ceiling(document.Width, 8 * maxH), mcuRows = Ceiling(document.Height, 8 * maxV);
        Assert.Equal(mcuColumns * mcuRows, document.McuCount);
        if (interleaved)
            Assert.Equal(document.McuCount * factors.Sum(factor => factor.Horizontal * factor.Vertical), document.Blocks.Count);
        for (int index = 0; index < 3; index++)
        {
            int id = document.Components.Keys.ElementAt(index);
            JpegComponentSampling factor = factors[index];
            Assert.Equal((factor.Horizontal, factor.Vertical), (document.Components[id].H, document.Components[id].V));
            int columns = Ceiling(document.Width * factor.Horizontal, 8 * maxH);
            int rows = Ceiling(document.Height * factor.Vertical, 8 * maxV);
            Assert.Equal(columns * rows, document.Blocks.Count(pair => pair.Key.Component == id && !pair.Value.IsDummy));
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => pair.Key.Component == id))
            {
                Assert.Equal(key.X >= columns || key.Y >= rows, block.IsDummy);
                if (block.IsDummy)
                {
                    Assert.Equal(0, block.DcDifference);
                    Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
                }
            }
        }
    }

    private static int MaxHorizontal(JpegSamplingFactors sampling) =>
        Math.Max(sampling.Luma.Horizontal, Math.Max(sampling.Cb.Horizontal, sampling.Cr.Horizontal));

    private static int MaxVertical(JpegSamplingFactors sampling) =>
        Math.Max(sampling.Luma.Vertical, Math.Max(sampling.Cb.Vertical, sampling.Cr.Vertical));

    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;
}
