using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderOutOfRangeFloatTests
{
    public static TheoryData<PixelComponentLayout, int, int> OracleModes
    {
        get
        {
            var modes = new TheoryData<PixelComponentLayout, int, int>();
            foreach (PixelComponentLayout layout in new[]
                { PixelComponentLayout.Gray, PixelComponentLayout.Rgb, PixelComponentLayout.YCbCr })
                foreach (int sampling in new[] { 0, 1, 2 })
                    foreach (int smoothing in new[] { 0, 37 })
                        modes.Add(layout, sampling, smoothing);
            return modes;
        }
    }

    [Theory]
    [InlineData(PixelComponentLayout.Gray, -0.01f)]
    [InlineData(PixelComponentLayout.Gray, 1.01f)]
    [InlineData(PixelComponentLayout.Rgb, -0.01f)]
    [InlineData(PixelComponentLayout.Rgb, 1.01f)]
    [InlineData(PixelComponentLayout.YCbCr, -0.01f)]
    [InlineData(PixelComponentLayout.YCbCr, 1.01f)]
    public void ConstantBlocksHaveIndependentDcOracleAndAreNotClipped(PixelComponentLayout layout, float value)
    {
        int components = Components(layout);
        float[] pixels = Enumerable.Range(0, 64 * components)
            .Select(index => layout == PixelComponentLayout.YCbCr && index % components != 0
                ? index % components == 1 ? 1.01f : -0.01f : value).ToArray();
        foreach (int smoothing in new[] { 0, 37, 100 })
            foreach ((bool progressive, int bits, bool optimize) in ScanModes())
            {
                var options = new JpegliEncoderOptions
                {
                    Quality = 90, Subsampling = ChromaSubsampling.None, AdaptiveQuantization = false,
                    AllowOutOfRangeFloatSamples = true, SmoothingFactor = smoothing,
                    Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
                };
                JpegDocument document = Read(Encode(pixels, 8, 8, layout, options), options);
                float[] clipped = pixels.Select(sample => Math.Clamp(sample, 0f, 1f)).ToArray();
                bool differsFromClipping = false;
                for (int component = 1; component <= components; component++)
                {
                    Block block = document.Blocks[new(component, 0, 0)];
                    int quantizer = document.Quantizers[document.Components[component].Quantizer][0];
                    // Constant orthonormal JPEG DC = 8 * (converted sample - 128).
                    // Source samples, JFIF conversion and the emitted quantizer are independent of encoder helpers.
                    int expected = (int)Math.Round(8 * (Converted(pixels, 0, layout, component - 1) - 128) / quantizer,
                        MidpointRounding.AwayFromZero);
                    int clippedDc = (int)Math.Round(8 * (Converted(clipped, 0, layout, component - 1) - 128) / quantizer,
                        MidpointRounding.AwayFromZero);
                    Assert.Equal(expected, block.Coefficients[0]);
                    Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                    differsFromClipping |= expected != clippedDc;
                }
                Assert.True(differsFromClipping, "The oracle must distinguish preserved samples from clipped input.");
            }
    }

    [Theory]
    [MemberData(nameof(OracleModes))]
    public void OddEdgesConversionSmoothingAndSamplingMatchIndependentCosineOracle(
        PixelComponentLayout layout, int sampling, int smoothing)
    {
        const int width = 19, height = 17;
        float[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 92, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
            SamplingFactors = Factors(sampling), SmoothingFactor = smoothing,
            OptimizeHuffmanTables = false, RestartInterval = 2
        };
        JpegDocument document = Read(Encode(pixels, width, height, layout, options), options);
        int maxH = document.Components.Values.Max(component => component.H);
        int maxV = document.Components.Values.Max(component => component.V);
        var substantialAc = new int[document.Components.Count];
        foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            Component component = document.Components[key.Component];
            double[] samples = OracleSamples(pixels, width, height, layout, key,
                maxH / component.H, maxV / component.V, smoothing);
            byte[] quantizer = document.Quantizers[component.Quantizer];
            for (int index = 0; index < 64; index++)
            {
                int natural = Zigzag[index];
                double expected = OracleDct(samples, natural % 8, natural / 8) / quantizer[index];
                double tolerance = index != 0 && block.Coefficients[index] == 0 ? 0.591 : 0.501;
                Assert.True(Math.Abs(block.Coefficients[index] - expected) <= tolerance,
                    $"Component {key.Component}, block ({key.X},{key.Y}), coefficient {index}: " +
                    $"expected nearest integer to {expected}, got {block.Coefficients[index]}.");
                if (index != 0 && Math.Abs(block.Coefficients[index]) >= 2)
                    substantialAc[key.Component - 1]++;
            }
        }
        Assert.All(substantialAc, count => Assert.True(count >= 8));
        JpegDocument clipped = Read(Encode(pixels.Select(value => Math.Clamp(value, 0f, 1f)).ToArray(),
            width, height, layout, options), options);
        Assert.Contains(document.Blocks, pair => !pair.Value.IsDummy &&
            !pair.Value.Coefficients.SequenceEqual(clipped.Blocks[pair.Key].Coefficients));
    }

    [Theory]
    [InlineData(PixelComponentLayout.Gray, ChromaSubsampling.Quarter, false)]
    [InlineData(PixelComponentLayout.Gray, ChromaSubsampling.Quarter, true)]
    [InlineData(PixelComponentLayout.Rgb, ChromaSubsampling.None, false)]
    [InlineData(PixelComponentLayout.Rgb, ChromaSubsampling.Quarter, true)]
    [InlineData(PixelComponentLayout.YCbCr, ChromaSubsampling.None, true)]
    [InlineData(PixelComponentLayout.YCbCr, ChromaSubsampling.Quarter, false)]
    public void BaselineProgressiveAndHuffmanModesPreserveCoefficientsAndDecodedPixels(
        PixelComponentLayout layout, ChromaSubsampling sampling, bool adaptive)
    {
        const int width = 33, height = 19;
        float[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, AllowOutOfRangeFloatSamples = true, Subsampling = sampling,
            AdaptiveQuantization = adaptive, SmoothingFactor = 37, OptimizeHuffmanTables = false, RestartInterval = 2
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = Read(baseline, options);
        ColorComponents channels = layout == PixelComponentLayout.Gray ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult decoded = ImageResult.FromMemory(baseline, channels);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
        {
            var variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = Encode(pixels, width, height, layout, variant);
            AssertCoefficients(reference, Read(jpeg, variant));
            Assert.Equal(decoded.Data, ImageResult.FromMemory(jpeg, channels).Data);
        }
    }

    [Fact]
    public void Quality100CanPreserveOutOfRangeSamplesWhenQuantizedCoefficientsFit()
    {
        const int width = 19, height = 17;
        float[] pixels = Pattern(width, height, PixelComponentLayout.Gray);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false, RestartInterval = 2
        };
        JpegDocument baseline = Read(Encode(pixels, width, height, PixelComponentLayout.Gray, options), options);
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
        {
            var variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
            };
            AssertCoefficients(baseline, Read(Encode(pixels, width, height, PixelComponentLayout.Gray, variant), variant));
        }
    }

    [Theory]
    [InlineData(-0.0001f, -1024)]
    [InlineData(1.0034f, 1023)]
    public void SignedElevenBitDcEndpointsRemainEncodable(float value, int expectedDc)
    {
        float[] pixels = Enumerable.Repeat(value, 64).ToArray();
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 100, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
                Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
            };
            Block block = Assert.Single(Read(Encode(pixels, 8, 8, PixelComponentLayout.Gray, options), options).Blocks).Value;
            Assert.Equal(expectedDc, block.Coefficients[0]);
            Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void AcCategoryTenEndpointsAreAllowedButTheNextMagnitudeIsRejected(int sign)
    {
        // A horizontal cosine with amplitude A has orthonormal u=1,v=0 coefficient 4*sqrt(2)*A.
        // Values are deliberately inside the rounding bins to avoid float half ties.
        foreach (double magnitude in new[] { 1022.8, 1023.8 })
        {
            var pixels = new float[64];
            double amplitude = sign * magnitude / (4 * Math.Sqrt(2));
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    pixels[y * 8 + x] = (float)((128 + amplitude * Math.Cos((2 * x + 1) * Math.PI / 16)) / 255);
            foreach ((bool progressive, int bits, bool optimize) in ScanModes())
            {
                var options = new JpegliEncoderOptions
                {
                    Quality = 100, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
                    Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
                };
                if (magnitude > 1023)
                    AssertRejectedWithoutOutput(pixels, 8, 8, PixelComponentLayout.Gray, options);
                else
                {
                    Block block = Assert.Single(Read(Encode(pixels, 8, 8, PixelComponentLayout.Gray, options), options).Blocks).Value;
                    Assert.Equal(0, block.Coefficients[0]);
                    Assert.Equal(sign * 1023, block.Coefficients[1]);
                    Assert.All(block.Coefficients[2..], coefficient => Assert.Equal(0, coefficient));
                }
            }
        }
    }

    [Theory]
    [InlineData(-0.000001f)]
    [InlineData(1.000001f)]
    public void DefaultStillRejectsOutOfRangeSamplesBeforeOutput(float invalid)
    {
        Assert.False(new JpegliEncoderOptions().AllowOutOfRangeFloatSamples);
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.Gray, PixelComponentLayout.Rgb, PixelComponentLayout.YCbCr })
        {
            float[] pixels = Pattern(17, 9, layout);
            pixels[^1] = invalid;
            AssertRejectedWithoutOutput(pixels, 17, 9, layout, new());
            Assert.Equal("pixels", Assert.Throws<ArgumentException>(() => Encode(pixels, 17, 9, layout, new())).ParamName);
        }
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.MaxValue)]
    [InlineData(-float.MaxValue)]
    [InlineData(float.MaxValue / 512f)]
    [InlineData(-float.MaxValue / 512f)]
    [InlineData(100000f)]
    public void InvalidLateConsumedSampleIsRejectedBeforeOutputInEveryEntropyMode(float invalid)
    {
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.Gray, PixelComponentLayout.Rgb, PixelComponentLayout.YCbCr })
            for (int component = 0; component < Components(layout); component++)
                foreach ((bool progressive, int bits, bool optimize) in ScanModes())
                {
                    float[] pixels = Enumerable.Repeat(0.5f, 24 * 8 * Components(layout)).ToArray();
                    pixels[(24 * 8 - 1) * Components(layout) + component] = invalid;
                    AssertRejectedWithoutOutput(pixels, 24, 8, layout, new()
                    {
                        Quality = 100, AdaptiveQuantization = false, AllowOutOfRangeFloatSamples = true,
                        Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
                    });
                }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FullPrecisionDcAndAcLimitsArePreflightedBeforeAnyHeaders(int kind)
    {
        const int width = 24, height = 8;
        float[] pixels = Enumerable.Repeat(0.5f, width * height).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 16; x < width; x++)
                pixels[y * width + x] = kind switch
                {
                    0 => -0.01f, 1 => 1.01f, _ => x < 20 ? -0.6f : 1.6f
                };
        // At quality 100 the final constant DC is about -1044 or +1036.
        // Those fit category 11 but violate the signed eleven-bit coefficient range.
        // The split block has a central DC but an AC magnitude greater than 1023.
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
            foreach (bool adaptive in new[] { false, true })
                AssertRejectedWithoutOutput(pixels, width, height, PixelComponentLayout.Gray, new()
                {
                    Quality = 100, AdaptiveQuantization = adaptive, AllowOutOfRangeFloatSamples = true,
                    Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize
                });
    }

    [Fact]
    public void AlternatingOutsideConstantsAreRejectedInsteadOfEncodingOversizedDcDifferences()
    {
        const int width = 24, height = 8;
        var pixels = new float[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = x / 8 % 2 == 0 ? -0.01f : 1.01f;
        // Adjacent full-precision DCs differ by about 2080, beyond category 11.
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
            AssertRejectedWithoutOutput(pixels, width, height, PixelComponentLayout.Gray, new()
            {
                Quality = 100, AdaptiveQuantization = false, AllowOutOfRangeFloatSamples = true,
                Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize,
                RestartInterval = 2
            });
    }

    [Theory]
    [InlineData(PixelComponentLayout.Rgb)]
    [InlineData(PixelComponentLayout.Bgr)]
    [InlineData(PixelComponentLayout.Rgba)]
    [InlineData(PixelComponentLayout.Bgra)]
    [InlineData(PixelComponentLayout.Gray)]
    [InlineData(PixelComponentLayout.YCbCr)]
    public void IgnoredAlphaPaddingAndTrailingNonfiniteSamplesDoNotAffectOutput(PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        PixelComponentLayout compactLayout = layout == PixelComponentLayout.Gray ? layout
            : layout == PixelComponentLayout.YCbCr ? layout : PixelComponentLayout.Rgb;
        float[] compact = Pattern(width, height, compactLayout);
        int components = layout is PixelComponentLayout.Rgba or PixelComponentLayout.Bgra ? 4 : Components(layout);
        int stride = width * components + 11;
        var options = new JpegliEncoderOptions
        {
            AllowOutOfRangeFloatSamples = true, SmoothingFactor = 37, Progressive = true,
            ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = Encode(compact, width, height, compactLayout, options);
        foreach (int trailing in new[] { 0, 17 })
        {
            float[] padded = Enumerable.Repeat(float.NaN, (height - 1) * stride + width * components + trailing).ToArray();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    for (int component = 0; component < Components(compactLayout); component++)
                    {
                        int target = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra ? 2 - component : component;
                        padded[y * stride + x * components + target] = compact[(y * width + x) * Components(compactLayout) + component];
                    }
            Assert.Equal(expected, Encode(padded, width, height, layout, options, stride));
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            Encode(output, padded, width, height, layout, options, TestContext.Current.CancellationToken, stride);
            Assert.False(output.Closed);
            Assert.Equal(expected, destination.ToArray());
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, PixelComponentLayout.Gray)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb)]
    [InlineData(PixelLayout.YCbCr24, PixelComponentLayout.YCbCr)]
    public void OptionDoesNotChangeBytesWordsOrInRangeFloats(PixelLayout byteLayout, PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        byte[] bytes = EncoderProgressiveTests.Pattern(width, height, byteLayout);
        ushort[] words = bytes.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = bytes.Select(value => value / 255f).ToArray();
        foreach ((bool progressive, int bits, bool optimize) in ScanModes())
        {
            var options = new JpegliEncoderOptions
            {
                Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize,
                SmoothingFactor = 37, RestartInterval = 2
            };
            var allowed = options with { AllowOutOfRangeFloatSamples = true };
            Assert.Equal(JpegliEncoder.Encode(bytes, width, height, byteLayout, options,
                    cancellationToken: TestContext.Current.CancellationToken),
                JpegliEncoder.Encode(bytes, width, height, byteLayout, allowed,
                    cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(JpegliEncoder.Encode(words, width, height, layout, options,
                    cancellationToken: TestContext.Current.CancellationToken),
                JpegliEncoder.Encode(words, width, height, layout, allowed,
                    cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(Encode(floats, width, height, layout, options), Encode(floats, width, height, layout, allowed));
        }
    }

    [Fact]
    public void PreCancellationWritesNothingAndPreservesTokenAndDestination()
    {
        float[] pixels = Pattern(19, 17, PixelComponentLayout.Rgb);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var cancellation = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            Encode(output, pixels, 19, 17, PixelComponentLayout.Rgb,
                new() { AllowOutOfRangeFloatSamples = true }, cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationAndIoFailurePreservePartialOutputAndLeaveDestinationOpen(bool optimize)
    {
        const int width = 33, height = 19;
        float[] pixels = Pattern(width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            AllowOutOfRangeFloatSamples = true, Progressive = true, ProgressiveRefinementBits = 2,
            OptimizeHuffmanTables = optimize, RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] complete = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        int stopAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var cancelledDestination = new MemoryStream();
        using var cancelledOutput = new WriteOnlyStream(cancelledDestination, () =>
        {
            if (cancelledDestination.Length >= stopAfter)
                cancellation.Cancel();
        });
        OperationCanceledException cancelled = Assert.Throws<OperationCanceledException>(() =>
            Encode(cancelledOutput, pixels, width, height, PixelComponentLayout.Rgb, options, cancellation.Token));
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
        Assert.Same(failure, Assert.Throws<IOException>(() =>
            Encode(failedOutput, pixels, width, height, PixelComponentLayout.Rgb, options, TestContext.Current.CancellationToken)));
        Assert.False(failedOutput.Closed);
        Assert.InRange(failedDestination.Length, stopAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)failedDestination.Length], failedDestination.ToArray());
    }

    private static IEnumerable<(bool Progressive, int Bits, bool Optimize)> ScanModes()
    {
        foreach (bool optimize in new[] { false, true })
        {
            yield return (false, 0, optimize);
            foreach (int bits in new[] { 0, 2, 10 })
                yield return (true, bits, optimize);
        }
    }

    private static int Components(PixelComponentLayout layout) => layout == PixelComponentLayout.Gray ? 1 : 3;

    private static JpegSamplingFactors Factors(int sampling) => sampling switch
    {
        0 => new(new(1, 1), new(1, 1), new(1, 1)),
        1 => new(new(2, 2), new(1, 1), new(1, 1)),
        _ => new(new(1, 2), new(2, 1), new(1, 1))
    };

    private static float[] Pattern(int width, int height, PixelComponentLayout layout)
    {
        int components = Components(layout);
        var pixels = new float[width * height * components];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int component = 0; component < components; component++)
                    pixels[(y * width + x) * components + component] =
                        -0.01f + ((19 * x + 37 * y + 53 * component + 11 * x * y) % 103) * 0.01f;
        Assert.Contains(pixels, value => value < 0);
        Assert.Contains(pixels, value => value > 1);
        return pixels;
    }

    private static double Converted(float[] pixels, int offset, PixelComponentLayout layout, int component)
    {
        if (layout is PixelComponentLayout.Gray or PixelComponentLayout.YCbCr)
            return pixels[offset + component] * 255d;
        double red = pixels[offset] * 255d, green = pixels[offset + 1] * 255d, blue = pixels[offset + 2] * 255d;
        double luma = 0.299 * red + 0.587 * green + 0.114 * blue;
        return component switch { 0 => luma, 1 => (blue - luma) / 1.772 + 128, _ => (red - luma) / 1.402 + 128 };
    }

    // Independent edge extension, 3x3 smoothing, rectangular box average and orthonormal cosine DCT.
    private static double[] OracleSamples(float[] pixels, int width, int height, PixelComponentLayout layout,
        BlockKey key, int areaWidth, int areaHeight, int smoothing)
    {
        var result = new double[64];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                double sum = 0;
                for (int dy = 0; dy < areaHeight; dy++)
                    for (int dx = 0; dx < areaWidth; dx++)
                    {
                        int column = (key.X * 8 + x) * areaWidth + dx, row = (key.Y * 8 + y) * areaHeight + dy;
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

        double Raw(int x, int y) => Converted(pixels,
            (Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)) * Components(layout), layout, key.Component - 1);
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

    private static byte[] Encode(float[] pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0) =>
        JpegliEncoder.Encode(pixels, width, height, layout, options, stride, TestContext.Current.CancellationToken);

    private static void Encode(Stream output, float[] pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0) =>
        JpegliEncoder.Encode(output, pixels, width, height, layout, options, stride, cancellationToken);

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertRejectedWithoutOutput(float[] pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options)
    {
        using var destination = new MemoryStream();
        destination.Write([0x12, 0x34]);
        using var output = new WriteOnlyStream(destination);
        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            Encode(output, pixels, width, height, layout, options, TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
        Assert.False(output.Closed);
    }

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
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
