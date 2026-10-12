using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderXybTests
{
    private static readonly int[] ComponentIds = ['R', 'G', 'B'];
    private static readonly byte[] OpaqueProfile = [0x01, 0x23, 0x45, 0x67];
    private static readonly JpegSamplingFactors FullResolution = new(new(1, 1), new(1, 1), new(1, 1));

    [Fact]
    public void XybEnumsAppendWithoutChangingExistingValues()
    {
        Assert.Equal(6, (int)PixelLayout.Xyb24);
        Assert.Equal(6, (int)PixelComponentLayout.Xyb);
        Assert.Equal(4, (int)JpegColorSpace.Xyb);
        Assert.Equal(5, (int)PixelLayout.YCbCr24);
        Assert.Equal(5, (int)PixelComponentLayout.YCbCr);
        Assert.Equal(3, (int)JpegColorSpace.Rgb);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AutomaticXybUsesThreeTablesRgbIdsDefaultSamplingAndUnmodifiedProfile(int kind)
    {
        const int width = 19, height = 17, length = 65_520;
        byte[] backing = Enumerable.Range(0, length + 8).Select(index => (byte)(index * 73 + 11)).ToArray();
        ReadOnlyMemory<byte> profile = backing.AsMemory(3, length);
        Array pixels = Source(kind, width, height);
        var options = Options() with { IccProfile = profile, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 };
        byte[] jpeg = Encode(pixels, width, height, options);
        Assert.Equal(jpeg, Encode(pixels, width, height, options with { ColorSpace = JpegColorSpace.Xyb }));
        foreach (ChromaSubsampling preset in Enum.GetValues<ChromaSubsampling>())
            Assert.Equal(jpeg, Encode(pixels, width, height, options with { Subsampling = preset }));

        JpegDocument document = Read(jpeg, options);
        Assert.Equal(ComponentIds, document.Components.Keys);
        Assert.Equal(new Component(2, 2, 0), document.Components['R']);
        Assert.Equal(new Component(2, 2, 1), document.Components['G']);
        Assert.Equal(new Component(1, 1, 2), document.Components['B']);
        Assert.Equal(9, document.Components.Values.Sum(component => component.H * component.V));
        Assert.Equal(new[] { 0, 1, 2 }, document.Quantizers.Keys);
        Assert.All(document.Scans.SelectMany(scan => scan.Selectors), selector =>
        {
            Assert.Equal(0, selector.Dc);
            Assert.Equal(0x10, selector.Ac);
        });
        List<Marker> markers = Markers(jpeg);
        Assert.DoesNotContain(markers, marker => marker.Code is 0xe0 or 0xee);
        Assert.Equal(new[] { 0, 0x10 }, HuffmanSelectors(markers).Distinct().Order());
        Marker[] chunks = markers.Where(marker => marker.Code == 0xe2).ToArray();
        Assert.Equal(2, chunks.Length);
        for (int index = 0; index < chunks.Length; index++)
        {
            Assert.True(chunks[index].Payload.AsSpan(0, 12).SequenceEqual("ICC_PROFILE\0"u8));
            Assert.Equal(index + 1, chunks[index].Payload[12]);
            Assert.Equal(chunks.Length, chunks[index].Payload[13]);
        }
        Assert.Equal(profile.ToArray(), chunks.SelectMany(chunk => chunk.Payload.Skip(14)).ToArray());
        Assert.Equal(65_519 + 14, chunks[0].Payload.Length);
        Assert.Equal(15, chunks[1].Payload.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ElementStridesNonseekableStreamsAndExpandedBytesPreserveEncoding(int kind)
    {
        const int width = 19, height = 17, stride = width * 3 + 13;
        Array pixels = Source(kind, width, height);
        Array padded = kind switch
        {
            0 => new byte[(height - 1) * stride + width * 3],
            1 => new ushort[(height - 1) * stride + width * 3],
            _ => Enumerable.Repeat(float.NaN, (height - 1) * stride + width * 3 + 7).ToArray()
        };
        for (int y = 0; y < height; y++)
            Array.Copy(pixels, y * width * 3, padded, y * stride, width * 3);
        var options = Options() with { SmoothingFactor = 37, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 };
        byte[] expected = Encode(pixels, width, height, options);
        Assert.Equal(expected, Encode(padded, width, height, options, stride));
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, padded, width, height, options, stride, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());

        byte[] bytes = (byte[])Source(0, width, height);
        ushort[] words = bytes.Select(value => (ushort)(value * 257)).ToArray();
        Assert.Equal(Encode(bytes, width, height, options), Encode(words, width, height, options));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 37)]
    [InlineData(1, 0)]
    [InlineData(1, 37)]
    [InlineData(2, 0)]
    [InlineData(2, 37)]
    public void DirectScaledSamplesMatchIndependentSmoothingSamplingAndDctOracle(int kind, int smoothing)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width, height);
        foreach (JpegSamplingFactors? factors in new JpegSamplingFactors?[]
            { null, FullResolution, new(new(1, 2), new(2, 1), new(1, 1)), new(new(1, 1), new(1, 3), new(1, 1)) })
        {
            var options = Options() with
            {
                Distance = 0.375f, AdaptiveQuantization = false, SmoothingFactor = smoothing,
                SamplingFactors = factors, RestartInterval = 2
            };
            JpegDocument document = Read(Encode(pixels, width, height, options), options);
            JpegComponentSampling[] expectedFactors = factors is null
                ? [new(2, 2), new(2, 2), new(1, 1)] : [factors.Luma, factors.Cb, factors.Cr];
            int maxH = expectedFactors.Max(factor => factor.Horizontal), maxV = expectedFactors.Max(factor => factor.Vertical);
            int substantialAc = 0;
            for (int channel = 0; channel < 3; channel++)
                Assert.Equal(new Component(expectedFactors[channel].Horizontal, expectedFactors[channel].Vertical, channel),
                    document.Components[ComponentIds[channel]]);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                int channel = Array.IndexOf(ComponentIds, key.Component);
                Component component = document.Components[key.Component];
                double[] samples = OracleSamples(pixels, width, height, key, channel,
                    maxH / component.H, maxV / component.V, smoothing);
                byte[] quantizer = document.Quantizers[channel];
                for (int frequency = 0; frequency < 64; frequency++)
                {
                    int natural = Zigzag[frequency];
                    double coefficient = OracleDct(samples, natural % 8, natural / 8) / quantizer[frequency];
                    Assert.True(Math.Abs(block.Coefficients[frequency] - coefficient) <= 0.501,
                        $"XYB channel {channel} block ({key.X},{key.Y}) frequency {frequency}: {coefficient} -> {block.Coefficients[frequency]}.");
                    if (frequency != 0 && Math.Abs(block.Coefficients[frequency]) >= 2)
                        substantialAc++;
                }
            }
            Assert.True(substantialAc >= 24, "Exercise substantial AC detail and channel-specific tables.");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScansRefinementHuffmanOptimizationAndRestartsPreserveCoefficients(bool adaptive)
    {
        const int width = 65, height = 17;
        Array pixels = Source(2, width, height);
        foreach (JpegSamplingFactors? factors in new JpegSamplingFactors?[] { null, new(new(1, 2), new(2, 1), new(1, 1)) })
        {
            var options = Options() with
            {
                AdaptiveQuantization = adaptive, SmoothingFactor = 37, OptimizeHuffmanTables = false, SamplingFactors = factors
            };
            byte[] baseline = Encode(pixels, width, height, options);
            JpegDocument expected = Read(baseline, options);
            ImageResult decoded = ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue);
            Assert.Equal((width, height), (decoded.Width, decoded.Height));
            foreach ((bool progressive, int refinement, bool optimize, int interval) in new[]
                { (false, 0, true, 2), (false, 0, false, 1), (true, 0, false, 2), (true, 2, true, 1), (true, 10, false, 2) })
            {
                var variant = options with
                {
                    Progressive = progressive, ProgressiveRefinementBits = refinement,
                    OptimizeHuffmanTables = optimize, RestartInterval = interval
                };
                byte[] jpeg = Encode(pixels, width, height, variant);
                AssertCoefficients(expected, Read(jpeg, variant));
                Assert.Equal(decoded.Data, ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
            }
        }
    }

    [Theory]
    [InlineData(false, 0.499, 0)]
    [InlineData(false, 0.501, 1)]
    [InlineData(false, 0.55, 1)]
    [InlineData(true, 0.5231, 0)]
    [InlineData(true, 0.5236, 1)]
    public void AdaptiveThresholdsAreUniformAndAnalyzeYDespiteXTexture(bool adaptive, double target, int expectedAc)
    {
        // Pinned upstream tables give X/B u=1 quantizers 41/21 at distance 1.45.
        // An independently captured scalar field for constant Y=128 has strength
        // 0.0467278957, hence the common XYB threshold 0.5 + 0.5*strength.
        var options = Options() with { Distance = 1.45f, SamplingFactors = FullResolution, AdaptiveQuantization = adaptive };
        foreach (int xPattern in new[] { 0, 1, 2 })
        {
            var pixels = new float[64 * 3];
            double amplitude = target * 21 / (4 * Math.Sqrt(2));
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int offset = (y * 8 + x) * 3;
                    pixels[offset] = (xPattern == 0 ? 32f : xPattern == 1 ? 224f : ((x + y) % 2 == 0 ? 16f : 240f)) / 255f;
                    pixels[offset + 1] = 128f / 255f;
                    pixels[offset + 2] = (float)((128 + amplitude * Math.Cos((2 * x + 1) * Math.PI / 16)) / 255);
                }
            JpegDocument document = Read(Encode(pixels, 8, 8, options), options);
            Assert.Equal((byte)41, document.Quantizers[0][1]);
            Assert.Equal((byte)21, document.Quantizers[2][1]);
            Assert.Equal(expectedAc, document.Blocks[new('B', 0, 0)].Coefficients[1]);
            Assert.All(document.Blocks[new('B', 0, 0)].Coefficients[2..], value => Assert.Equal(0, value));
            Assert.All(document.Blocks[new('G', 0, 0)].Coefficients, value => Assert.Equal(0, value));

            // Move the same normalized coefficient to X, retaining constant Y.
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    pixels[(y * 8 + x) * 3] = (float)((128 + amplitude * 41 / 21 * Math.Cos((2 * x + 1) * Math.PI / 16)) / 255);
            document = Read(Encode(pixels, 8, 8, options), options);
            Assert.Equal(expectedAc, document.Blocks[new('R', 0, 0)].Coefficients[1]);
        }
    }

    [Fact]
    public void SubsampledYDisablesAdaptiveAnalysisInEitherDirection()
    {
        Array pixels = Source(0, 48, 35);
        var options = Options() with { Quality = 70 };
        foreach (JpegSamplingFactors factors in new[]
            { new JpegSamplingFactors(new(2, 1), new(1, 1), new(1, 1)), new(new(1, 2), new(1, 1), new(1, 1)) })
        {
            var sampled = options with { SamplingFactors = factors };
            Assert.Equal(Encode(pixels, 48, 35, sampled), Encode(pixels, 48, 35, sampled with { AdaptiveQuantization = false }));
        }
        var yFull = options with { SamplingFactors = new(new(1, 1), new(2, 1), new(1, 1)) };
        Assert.False(Encode(pixels, 48, 35, yFull).SequenceEqual(Encode(pixels, 48, 35, yFull with { AdaptiveQuantization = false })));
    }

    [Fact]
    public void SmoothedCoefficientsUseIndependentUnsmoothedYAdaptiveField()
    {
        const int width = 19, height = 23;
        // Independent full-image scalar C++ capture in AdaptiveQuantizationTests:
        // sampling 1 and AC quantizer 4. Every quantizer below 9 uses that same
        // undampened field, including XYB's Y u=1 quantizer 7 at distance 1.45.
        double[] strengths = [0.252179861, 2.069556, 1.5491066, 0.262044787, 2.70128822,
            1.68419814, 0.168017507, 2.51836777, 1.48105407];
        byte[] pixels = (byte[])Source(0, width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[(y * width + x) * 3 + 1] = (byte)(x < 8 ? (y < 12 ? 16 : x * 5 + y * 3)
                    : (x * 17 + y * 29 + x * y * 7) % 256);
        var options = Options() with { Distance = 1.45f, SmoothingFactor = 37, SamplingFactors = FullResolution };
        JpegDocument document = Read(Encode(pixels, width, height, options), options);
        Assert.Equal((byte)7, document.Quantizers[1][1]);
        int erasedDetail = 0;
        foreach ((BlockKey key, Block block) in document.Blocks)
        {
            Assert.False(block.IsDummy);
            int channel = Array.IndexOf(ComponentIds, key.Component);
            double[] samples = OracleSamples(pixels, width, height, key, channel, 1, 1, 37);
            double threshold = 0.5 + 0.5 * strengths[key.Y * 3 + key.X];
            for (int frequency = 0; frequency < 64; frequency++)
            {
                int natural = Zigzag[frequency];
                double coefficient = OracleDct(samples, natural % 8, natural / 8) / document.Quantizers[channel][frequency];
                if (frequency != 0 && Math.Abs(coefficient) < threshold - 0.001)
                {
                    Assert.Equal(0, block.Coefficients[frequency]);
                    if (Math.Abs(coefficient) > 0.501)
                        erasedDetail++;
                }
                else if (frequency == 0 || Math.Abs(coefficient) > threshold + 0.001)
                    Assert.InRange(Math.Abs(block.Coefficients[frequency] - coefficient), 0, 0.501);
            }
        }
        Assert.True(erasedDetail >= 6, "Check adaptive erasure of detail that ordinary rounding would retain.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MissingProfilesAndUnsupportedTransformsFailBeforeOutput(int kind)
    {
        Array pixels = Source(kind, 8, 8);
        AssertRejected(pixels, Options() with { IccProfile = ReadOnlyMemory<byte>.Empty }, "IccProfile");
        foreach (JpegColorSpace output in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb })
            AssertRejected(pixels, Options() with { ColorSpace = output }, "ColorSpace");
        foreach (PixelComponentLayout layout in new[]
            { PixelComponentLayout.YCbCr })
        {
            Array source = kind switch { 0 => new byte[256], 1 => new ushort[256], _ => new float[256] };
            using var destination = new MemoryStream();
            using var stream = new WriteOnlyStream(destination);
            destination.Write([0x12, 0x34]);
            var exception = Assert.Throws<ArgumentException>(() => Encode(stream, source, 8, 8,
                Options() with { ColorSpace = JpegColorSpace.Xyb }, layout: layout, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal("ColorSpace", exception.ParamName);
            Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
            Assert.False(stream.Closed);
        }
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.MaxValue)]
    [InlineData(-0.01f)]
    [InlineData(1.01f)]
    public void InvalidConsumedFloatsFailBeforeHeaders(float invalid)
    {
        var pixels = Enumerable.Repeat(0.5f, 8 * 8 * 3).ToArray();
        for (int channel = 0; channel < 3; channel++)
        {
            pixels[^3] = pixels[^2] = pixels[^1] = 0.5f;
            pixels[pixels.Length - 3 + channel] = invalid;
            AssertRejected(pixels, Options(), "pixels");
            if (!float.IsFinite(invalid) || Math.Abs(invalid) == float.MaxValue)
                AssertRejected(pixels, Options() with { AllowOutOfRangeFloatSamples = true }, "pixels");
        }
    }

    [Fact]
    public void PermittedOutsideFloatsRetainScaledSamplesAndPreflightUnencodableCoefficients()
    {
        float[] constants = [-0.002f, 1.002f, 0.50037f];
        var pixels = new float[64 * 3];
        for (int pixel = 0; pixel < 64; pixel++)
            constants.CopyTo(pixels, pixel * 3);
        var options = Options() with
        {
            Distance = 0.375f, SamplingFactors = FullResolution, AdaptiveQuantization = false,
            AllowOutOfRangeFloatSamples = true, Progressive = true, ProgressiveRefinementBits = 2
        };
        JpegDocument document = Read(Encode(pixels, 8, 8, options), options);
        for (int channel = 0; channel < 3; channel++)
        {
            Block block = document.Blocks[new(ComponentIds[channel], 0, 0)];
            int expectedDc = (int)Math.Round(8 * (constants[channel] * 255d - 128) / document.Quantizers[channel][0], MidpointRounding.AwayFromZero);
            Assert.Equal(expectedDc, block.Coefficients[0]);
            Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
        }
        for (int pixel = 0; pixel < 64; pixel++)
            pixels[pixel * 3 + 1] = 4f;
        foreach ((bool progressive, int bits, bool optimize) in new[] { (false, 0, false), (false, 0, true), (true, 2, false), (true, 10, true) })
            AssertRejected(pixels, options with { Progressive = progressive, ProgressiveRefinementBits = bits, OptimizeHuffmanTables = optimize }, "pixels");
    }

    [Fact]
    public void PreCancellationWritesNothingAndPreservesDestinationAndToken()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var cancellation = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, Source(2, 8, 8), 8, 8, Options(), cancellationToken: cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationAndIoFailureDuringRefinementPreserveWrittenPrefix(bool optimize)
    {
        const int width = 33, height = 19;
        Array pixels = Source(2, width, height);
        var options = Options() with
        {
            Progressive = true, ProgressiveRefinementBits = 2, OptimizeHuffmanTables = optimize, RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] complete = Encode(pixels, width, height, options);
        int stopAfter = Read(complete, options).Scans[5].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var cancelledDestination = new MemoryStream();
        using var cancelledOutput = new WriteOnlyStream(cancelledDestination, () =>
        {
            if (cancelledDestination.Length >= stopAfter)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => Encode(cancelledOutput, pixels, width, height, options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
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
        Assert.Same(failure, Assert.Throws<IOException>(() => Encode(failedOutput, pixels, width, height, options, cancellationToken: TestContext.Current.CancellationToken)));
        Assert.False(failedOutput.Closed);
        Assert.InRange(failedDestination.Length, stopAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)failedDestination.Length], failedDestination.ToArray());
    }

    private static JpegliEncoderOptions Options() => new() { IccProfile = OpaqueProfile };

    private static Array Source(int kind, int width, int height)
    {
        var bytes = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < 3; channel++)
                    bytes[(y * width + x) * 3 + channel] = (byte)((19 * x + 37 * y + 71 * channel + 11 * x * y) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + (index % 47) - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
    }

    private static double Sample(Array pixels, int offset) => pixels switch
    {
        byte[] bytes => bytes[offset],
        ushort[] words => words[offset] / 257d,
        _ => ((float[])pixels)[offset] * 255d
    };

    // Independent replicated edges, 3x3 smoothing, box averaging and cosine DCT.
    // Source samples are already scaled XYB: no RGB or opsin transformation occurs.
    private static double[] OracleSamples(Array pixels, int width, int height, BlockKey key, int channel,
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

        double Raw(int x, int y) => Sample(pixels, (Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)) * 3 + channel);
    }

    private static double OracleDct(double[] samples, int u, int v)
    {
        double sum = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                sum += samples[y * 8 + x] * Math.Cos((2 * x + 1) * u * Math.PI / 16) * Math.Cos((2 * y + 1) * v * Math.PI / 16);
        return sum / 4 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1);
    }

    private static byte[] Encode(Array pixels, int width, int height, JpegliEncoderOptions options, int stride = 0) => pixels switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, PixelLayout.Xyb24, options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Xyb, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])pixels, width, height, PixelComponentLayout.Xyb, options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array pixels, int width, int height, JpegliEncoderOptions options, int stride = 0,
        PixelComponentLayout layout = PixelComponentLayout.Xyb, CancellationToken cancellationToken = default)
    {
        if (cancellationToken == default)
            cancellationToken = TestContext.Current.CancellationToken;
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout, options, stride, cancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, cancellationToken);
    }

    private static void AssertRejected(Array pixels, JpegliEncoderOptions options, string parameter)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        destination.Write([0x12, 0x34]);
        ArgumentException error = Assert.ThrowsAny<ArgumentException>(() => Encode(output, pixels, 8, 8, options));
        Assert.Equal(parameter, error.ParamName);
        Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
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
            int length = jpeg[position] * 256 + jpeg[position + 1], end = position + length;
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
                int selector = marker.Payload[position++], symbols = 0;
                for (int bits = 0; bits < 16; bits++)
                    symbols += marker.Payload[position++];
                position += symbols;
                Assert.InRange(position, 0, marker.Payload.Length);
                yield return selector;
            }
        }
    }

    private static ReadOnlySpan<int> Zigzag =>
    [
        0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
        12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
        35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
        58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
    ];
}
