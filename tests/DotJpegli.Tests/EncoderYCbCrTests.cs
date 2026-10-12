using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderYCbCrTests
{
    public static TheoryData<int, int, int> OracleModes
    {
        get
        {
            var modes = new TheoryData<int, int, int>();
            for (int kind = 0; kind < 3; kind++)
                for (int sampling = 0; sampling < 3; sampling++)
                    foreach (int smoothing in new[] { 0, 37 })
                        modes.Add(kind, sampling, smoothing);
            return modes;
        }
    }

    public static TheoryData<int, ChromaSubsampling, bool> PipelineModes
    {
        get
        {
            var modes = new TheoryData<int, ChromaSubsampling, bool>();
            for (int kind = 0; kind < 3; kind++)
                foreach (ChromaSubsampling sampling in new[] { ChromaSubsampling.None, ChromaSubsampling.Quarter })
                    foreach (bool adaptive in new[] { false, true })
                        modes.Add(kind, sampling, adaptive);
            return modes;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ConstantComponentsHaveIndependentDcAndRetainFractionalSamples(int kind)
    {
        Array pixels = Allocate(kind, 8 * 8 * 3);
        int[] values = kind == 0 ? [32, 74, 217] : [32845, 32947, 41005];
        for (int pixel = 0; pixel < 64; pixel++)
            for (int component = 0; component < 3; component++)
                Set(pixels, pixel * 3 + component, values[component]);
        foreach (int smoothing in new[] { 0, 37, 100 })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 100, Subsampling = ChromaSubsampling.None,
                AdaptiveQuantization = false, SmoothingFactor = smoothing,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
            };
            JpegDocument document = Read(Encode(pixels, 8, 8, options), options);
            Assert.Equal(3, document.Components.Count);
            for (int component = 1; component <= 3; component++)
            {
                Block block = document.Blocks[new(component, 0, 0)];
                int quantizer = document.Quantizers[document.Components[component].Quantizer][0];
                Assert.Equal(1, quantizer);
                // The DC of a constant orthonormal 8x8 block is eight times its
                // centered sample. No RGB conversion belongs in this calculation.
                int dc = (int)Math.Round(8 * (Sample(pixels, component - 1) - 128) / quantizer,
                    MidpointRounding.AwayFromZero);
                Assert.Equal(dc, block.Coefficients[0]);
                Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                if (kind != 0 && component <= 2)
                {
                    Assert.Equal(128d, Math.Round(Sample(pixels, component - 1)));
                    Assert.Equal(component == 1 ? -2 : 2, dc);
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(OracleModes))]
    public void AsymmetricAndSubsampledLumaCoefficientsMatchIndependentFilterAndDctOracle(
        int kind, int sampling, int smoothing)
    {
        const int width = 19, height = 17;
        Array pixels = Pattern(kind, width, height);
        var options = new JpegliEncoderOptions
        {
            Quality = 92, AdaptiveQuantization = false, SamplingFactors = Factors(sampling),
            SmoothingFactor = smoothing, OptimizeHuffmanTables = false, RestartInterval = 2
        };
        JpegDocument document = Read(Encode(pixels, width, height, options), options);
        JpegComponentSampling[] factors = [options.SamplingFactors.Luma, options.SamplingFactors.Cb,
            options.SamplingFactors.Cr];
        for (int component = 1; component <= 3; component++)
            Assert.Equal((factors[component - 1].Horizontal, factors[component - 1].Vertical),
                (document.Components[component].H, document.Components[component].V));
        int maxH = factors.Max(factor => factor.Horizontal), maxV = factors.Max(factor => factor.Vertical);
        var substantialAc = new int[3];
        foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            Component component = document.Components[key.Component];
            int areaWidth = maxH / component.H, areaHeight = maxV / component.V;
            double[] samples = OracleSamples(pixels, width, height, key, areaWidth, areaHeight, smoothing);
            byte[] quantizer = document.Quantizers[component.Quantizer];
            for (int index = 0; index < 64; index++)
            {
                int natural = Zigzag[index];
                double coefficient = OracleDct(samples, natural % 8, natural / 8) / quantizer[index];
                // The color quantizer suppresses small AC values within its
                // component dead zones (all below 0.591); the remaining values
                // must round to the nearest integer, allowing float half ties.
                double tolerance = index != 0 && block.Coefficients[index] == 0 ? 0.591 : 0.501;
                Assert.True(Math.Abs(block.Coefficients[index] - coefficient) <= tolerance,
                    $"Component {key.Component}, block ({key.X},{key.Y}), coefficient {index}: " +
                    $"expected nearest integer to {coefficient}, got {block.Coefficients[index]}.");
                if (index != 0 && Math.Abs(block.Coefficients[index]) >= 2)
                    substantialAc[key.Component - 1]++;
            }
        }
        Assert.All(substantialAc, count => Assert.True(count >= 8,
            "The independent oracle must check substantial nonzero AC data in every component."));
    }

    [Theory]
    [MemberData(nameof(PipelineModes))]
    public void ScanHuffmanAndRestartModesPreserveCoefficientsAndDecodedPixels(
        int kind, ChromaSubsampling sampling, bool adaptive)
    {
        // Ten or more MCUs exercise restart marker wraparound. Odd image edges
        // also leave dummy luma blocks in interleaved 4:2:0 scans.
        const int width = 65, height = 17;
        Array pixels = Pattern(kind, width, height);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling, AdaptiveQuantization = adaptive,
            SmoothingFactor = 37, OptimizeHuffmanTables = false
        };
        byte[] baseline = Encode(pixels, width, height, options);
        JpegDocument reference = Read(baseline, options);
        ImageResult decoded = ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        foreach ((bool progressive, int refinement, bool optimize, int interval) in new[]
        {
            (false, 0, true, 2), (false, 0, false, 1),
            (true, 0, false, 2), (true, 2, true, 1)
        })
        {
            var variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = refinement,
                OptimizeHuffmanTables = optimize, RestartInterval = interval
            };
            byte[] jpeg = Encode(pixels, width, height, variant);
            AssertCoefficients(reference, Read(jpeg, variant));
            Assert.Equal(decoded.Data, ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
        }
    }

    [Fact]
    public void ExpandedBytesHaveExactlyTheSameEncodingAsSixteenBitYCbCr()
    {
        const int width = 19, height = 17;
        byte[] pixels = (byte[])Pattern(0, width, height);
        ushort[] words = pixels.Select(value => (ushort)(value * 257)).ToArray();
        JpegSamplingFactors?[] sampling = [null, new(new(1, 1), new(1, 1), new(1, 1)), Factors(0), Factors(2)];
        foreach (JpegSamplingFactors? factors in sampling)
            foreach (bool progressive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions
                {
                    SamplingFactors = factors, SmoothingFactor = progressive ? 37 : 0,
                    OptimizeHuffmanTables = progressive, Progressive = progressive,
                    ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 2
                };
                Assert.Equal(Encode(pixels, width, height, options), Encode(words, width, height, options));
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ElementStridesLastRowPaddingAndNonSeekableStreamsPreserveEncoding(int kind)
    {
        foreach ((int width, int height) in new[] { (1, 17), (17, 1), (19, 17) })
        {
            Array compact = Pattern(kind, width, height);
            int stride = width * 3 + 13;
            var options = new JpegliEncoderOptions
            {
                SmoothingFactor = 73, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, options);
            foreach (int trailing in new[] { 0, 17 })
            {
                // No final-row padding is required. Longer float storage contains
                // invalid sentinels everywhere outside consumed Y/Cb/Cr elements.
                Array padded = Allocate(kind, (height - 1) * stride + width * 3 + trailing);
                for (int y = 0; y < height; y++)
                    Array.Copy(compact, y * width * 3, padded, y * stride, width * 3);
                Assert.Equal(expected, Encode(padded, width, height, options, stride));
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                Encode(output, padded, width, height, options, stride);
                Assert.False(output.Closed);
                Assert.Equal(expected, destination.ToArray());
            }
        }
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(-0.000001f)]
    [InlineData(1.000001f)]
    public void EveryConsumedFloatComponentIsValidatedBeforeOutput(float invalid)
    {
        const int width = 17, height = 9;
        foreach (int component in new[] { 0, 1, 2 })
            foreach (bool optimize in new[] { false, true })
            {
                float[] pixels = (float[])Pattern(2, width, height);
                pixels[(width * height - 1) * 3 + component] = invalid;
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels, width, height,
                    new() { OptimizeHuffmanTables = optimize, Progressive = true, ProgressiveRefinementBits = 2 }));
                Assert.Equal("pixels", error.ParamName);
                Assert.Equal(0, destination.Length);
                Assert.False(output.Closed);
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ShapeOptionAndPreCancellationValidationWriteNothing(int kind)
    {
        Array pixels = Pattern(kind, 2, 2);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Assert.Throws<ArgumentNullException>(() => Encode(null!, pixels, 2, 2, new()));
        using var readOnly = new MemoryStream(new byte[16], writable: false);
        Assert.Equal("output", Assert.Throws<ArgumentException>(() => Encode(readOnly, pixels, 2, 2, new())).ParamName);
        Assert.Equal(new byte[16], readOnly.ToArray());
        Assert.Equal("width", Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 0, 2, new())).ParamName);
        Assert.Equal("stride", Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 2, 2, new(), 5)).ParamName);
        Assert.Equal("pixels", Assert.Throws<ArgumentException>(() => Encode(output, pixels, 2, 2, new(), 7)).ParamName);
        Assert.Equal("pixels", Assert.Throws<ArgumentException>(() => Encode(output, pixels, 3, 2, new())).ParamName);
        Assert.Equal(nameof(JpegliEncoderOptions.SmoothingFactor), Assert.Throws<ArgumentOutOfRangeException>(() =>
            Encode(output, pixels, 2, 2, new() { SmoothingFactor = 101 })).ParamName);
        var cancellation = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            EncodeWithCancellation(output, pixels, 2, 2, new(), cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void StreamCancellationAndIoFailuresPreservePartialOutputAndLeaveDestinationOpen(int kind)
    {
        const int width = 65, height = 17;
        Array pixels = Pattern(kind, width, height);
        var options = new JpegliEncoderOptions
        {
            SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] complete = Encode(pixels, width, height, options);
        int stopAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var cancelledDestination = new MemoryStream();
        using var cancelledOutput = new WriteOnlyStream(cancelledDestination, () =>
        {
            if (cancelledDestination.Length >= stopAfter)
                cancellation.Cancel();
        });
        OperationCanceledException cancelled = Assert.Throws<OperationCanceledException>(() =>
            EncodeWithCancellation(cancelledOutput, pixels, width, height, options, cancellation.Token));
        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.False(cancelledOutput.Closed);
        Assert.InRange(cancelledDestination.Length, stopAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)cancelledDestination.Length], cancelledDestination.ToArray());

        var failure = new IOException("Injected destination failure.");
        using var failedDestination = new MemoryStream();
        using var failedOutput = new WriteOnlyStream(failedDestination, () =>
        {
            if (failedDestination.Length >= stopAfter)
                throw failure;
        });
        Assert.Same(failure, Assert.Throws<IOException>(() => Encode(failedOutput, pixels, width, height, options)));
        Assert.False(failedOutput.Closed);
        Assert.InRange(failedDestination.Length, stopAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)failedDestination.Length], failedDestination.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NeutralChromaDecodesToGrayscaleIncludingFullRangeEndpoints(int kind)
    {
        foreach (byte value in new byte[] { 0, 37, 128, 255 })
        {
            Array pixels = Allocate(kind, 64 * 3);
            for (int pixel = 0; pixel < 64; pixel++)
            {
                Set(pixels, pixel * 3, kind == 0 ? value : value * 257);
                Set(pixels, pixel * 3 + 1, kind == 0 ? 128 : 32896);
                Set(pixels, pixel * 3 + 2, kind == 0 ? 128 : 32896);
            }
            byte[] jpeg = Encode(pixels, 8, 8, new() { Quality = 100, AdaptiveQuantization = false });
            Assert.All(ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data,
                sample => Assert.Equal(value, sample));
        }
    }

    [Fact]
    public void IndependentlyPreconvertedRgbFloatsProduceTheSameYCbCrPipelineOutput()
    {
        const int width = 19, height = 17;
        var rgb = new float[width * height * 3];
        var ycbcr = new float[rgb.Length];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = (y * width + x) * 3;
                for (int component = 0; component < 3; component++)
                    rgb[offset + component] = (7000 + (977 * x + 1703 * y + 5003 * component + 59 * x * y) % 45000) / 65535f;
                double red = rgb[offset] * 255d, green = rgb[offset + 1] * 255d, blue = rgb[offset + 2] * 255d;
                double luma = 0.299 * red + 0.587 * green + 0.114 * blue;
                ycbcr[offset] = (float)(luma / 255);
                ycbcr[offset + 1] = (float)(((blue - luma) / 1.772 + 128) / 255);
                ycbcr[offset + 2] = (float)(((red - luma) / 1.402 + 128) / 255);
            }
        foreach (JpegSamplingFactors? sampling in new JpegSamplingFactors?[] { null, Factors(0) })
            foreach (bool adaptive in new[] { false, true })
                foreach (bool progressive in new[] { false, true })
                {
                    var options = new JpegliEncoderOptions
                    {
                        Quality = 90, SamplingFactors = sampling, AdaptiveQuantization = adaptive,
                        SmoothingFactor = 37, Progressive = progressive,
                        ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 2
                    };
                    byte[] expected = JpegliEncoder.Encode(rgb, width, height, PixelComponentLayout.Rgb,
                        options, cancellationToken: TestContext.Current.CancellationToken);
                    Assert.Equal(expected, Encode(ycbcr, width, height, options));
                }
    }

    private static JpegSamplingFactors Factors(int sampling) => sampling switch
    {
        0 => new(new(1, 2), new(2, 1), new(1, 1)),
        1 => new(new(3, 1), new(1, 1), new(1, 1)),
        _ => new(new(1, 1), new(1, 3), new(1, 1))
    };

    private static Array Allocate(int kind, int length) => kind switch
    {
        0 => Enumerable.Repeat(byte.MaxValue, length).ToArray(),
        1 => Enumerable.Repeat(ushort.MaxValue, length).ToArray(),
        _ => Enumerable.Range(0, length).Select(index => (index % 5) switch
        {
            0 => float.NaN, 1 => float.PositiveInfinity, 2 => float.NegativeInfinity, 3 => -1f, _ => 2f
        }).ToArray()
    };

    private static Array Pattern(int kind, int width, int height)
    {
        Array pixels = Allocate(kind, width * height * 3);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int component = 0; component < 3; component++)
                {
                    int value = kind == 0
                        ? 7 + (19 * x + 37 * y + 53 * component + 11 * x * y) % 241
                        : 4000 + (977 * x + 1703 * y + 11007 * component + 59 * x * y) % 55000;
                    Set(pixels, (y * width + x) * 3 + component, value);
                }
        return pixels;
    }

    private static void Set(Array pixels, int offset, int value)
    {
        if (pixels is byte[] bytes)
            bytes[offset] = (byte)value;
        else if (pixels is ushort[] words)
            words[offset] = (ushort)value;
        else
            ((float[])pixels)[offset] = value / 65535f;
    }

    private static double Sample(Array pixels, int offset) => pixels switch
    {
        byte[] bytes => bytes[offset],
        ushort[] words => words[offset] / 257d,
        _ => ((float[])pixels)[offset] * 255d
    };

    // Mathematical oracle: edge-extend each original channel, smooth before a
    // rectangular box average, and apply the direct orthonormal cosine DCT.
    // No encoder conversion, filtering, DCT or quantization helper is called.
    private static double[] OracleSamples(Array pixels, int width, int height, BlockKey key,
        int areaWidth, int areaHeight, int smoothing)
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
                        double center = Raw(column, row), neighbors = 0;
                        for (int ny = -1; ny <= 1; ny++)
                            for (int nx = -1; nx <= 1; nx++)
                                if (nx != 0 || ny != 0)
                                    neighbors += Raw(column + nx, row + ny);
                        sum += center * (1 - smoothing / 128d) + neighbors * smoothing / 1024d;
                    }
                result[y * 8 + x] = sum / (areaWidth * areaHeight) - 128;
            }
        return result;

        double Raw(int column, int row) => Sample(pixels,
            (Math.Clamp(row, 0, height - 1) * width + Math.Clamp(column, 0, width - 1)) * 3 + key.Component - 1);
    }

    private static double OracleDct(double[] samples, int u, int v)
    {
        double sum = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                sum += samples[y * 8 + x] * Math.Cos((2 * x + 1) * u * Math.PI / 16)
                    * Math.Cos((2 * y + 1) * v * Math.PI / 16);
        return sum / 4 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1);
    }

    private static byte[] Encode(Array pixels, int width, int height, JpegliEncoderOptions options, int stride = 0) =>
        pixels switch
        {
            byte[] bytes => JpegliEncoder.Encode(bytes, width, height, PixelLayout.YCbCr24, options, stride,
                TestContext.Current.CancellationToken),
            ushort[] words => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.YCbCr, options, stride,
                TestContext.Current.CancellationToken),
            _ => JpegliEncoder.Encode((float[])pixels, width, height, PixelComponentLayout.YCbCr, options, stride,
                TestContext.Current.CancellationToken)
        };

    private static void Encode(Stream output, Array pixels, int width, int height, JpegliEncoderOptions options,
        int stride = 0) =>
        EncodeWithCancellation(output, pixels, width, height, options, TestContext.Current.CancellationToken, stride);

    private static void EncodeWithCancellation(Stream output, Array pixels, int width, int height,
        JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0)
    {
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, PixelLayout.YCbCr24, options, stride, cancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, PixelComponentLayout.YCbCr, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, PixelComponentLayout.YCbCr, options, stride, cancellationToken);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        Assert.Equal(expected.Quantizers.Count, actual.Quantizers.Count);
        foreach ((int selector, byte[] quantizer) in expected.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
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

    private static ReadOnlySpan<int> Zigzag =>
    [
        0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
    ];
}
