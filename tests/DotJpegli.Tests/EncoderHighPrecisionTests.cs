using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderHighPrecisionTests
{
    public static TheoryData<bool, PixelComponentLayout, ChromaSubsampling, bool, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<bool, PixelComponentLayout, ChromaSubsampling, bool, int>();
            foreach (bool floatingPoint in new[] { false, true })
                foreach (bool adaptive in new[] { false, true })
                    foreach (int smoothing in new[] { 0, 37 })
                    {
                        modes.Add(floatingPoint, PixelComponentLayout.Gray, ChromaSubsampling.Quarter, adaptive, smoothing);
                        foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                            modes.Add(floatingPoint, PixelComponentLayout.Rgb, sampling, adaptive, smoothing);
                    }
            return modes;
        }
    }

    [Theory]
    [InlineData(false, 32845)]
    [InlineData(false, 32947)]
    [InlineData(true, 32845)]
    [InlineData(true, 32947)]
    public void FractionalGrayscaleSamplesSurviveConversionAndHaveIndependentDcOracle(bool floatingPoint, int value)
    {
        const int width = 8, height = 8;
        float normalized = value / 65535f;
        Array pixels = floatingPoint
            ? Enumerable.Repeat(normalized, width * height).ToArray()
            : Enumerable.Repeat((ushort)value, width * height).ToArray();
        // A constant 8x8 block has orthonormal JPEG DC = 8 * (sample - 128).
        // Both samples round to byte 128, but their fractional DC rounds to -2 or +2.
        // The formula uses source samples and the emitted quantizer, no encoder helpers.
        double sample = floatingPoint ? normalized * 255d : value * (255d / 65535);
        Assert.Equal(128d, Math.Round(sample, MidpointRounding.AwayFromZero));
        foreach (bool adaptive in new[] { false, true })
            foreach (int smoothing in new[] { 0, 37, 100 })
                foreach (bool optimize in new[] { false, true })
                    foreach (bool progressive in new[] { false, true })
                    {
                        var options = new JpegliEncoderOptions
                        {
                            Quality = 100, AdaptiveQuantization = adaptive, SmoothingFactor = smoothing,
                            OptimizeHuffmanTables = optimize, Progressive = progressive,
                            ProgressiveRefinementBits = progressive ? 2 : 0
                        };
                        byte[] jpeg = Encode(pixels, width, height, PixelComponentLayout.Gray, options);
                        JpegDocument document = Read(jpeg, options);
                        Block block = Assert.Single(document.Blocks).Value;
                        byte quantizer = document.Quantizers[document.Components[1].Quantizer][0];
                        Assert.Equal((byte)1, quantizer);
                        int expectedDc = (int)Math.Round(8 * (sample - 128) / quantizer, MidpointRounding.AwayFromZero);
                        Assert.Equal(value < 32896 ? -2 : 2, expectedDc);
                        Assert.Equal(expectedDc, block.Coefficients[0]);
                        Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                        Assert.Equal((byte)8, FramePrecision(jpeg));
                    }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void FractionalSamplesPreserveCoefficientsAndPixelsAcrossScanAndHuffmanModes(
        bool floatingPoint, PixelComponentLayout layout, ChromaSubsampling sampling, bool adaptive, int smoothing)
    {
        // Odd dimensions exercise replicated edges and wholly padded luma DC blocks.
        const int width = 33, height = 19;
        Array pixels = Pattern(floatingPoint, width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling, AdaptiveQuantization = adaptive,
            SmoothingFactor = smoothing, OptimizeHuffmanTables = false, RestartInterval = 2
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = Read(baseline, options);
        ColorComponents channels = layout == PixelComponentLayout.Gray ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult decoded = ImageResult.FromMemory(baseline, channels);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        foreach (bool optimize in new[] { false, true })
            foreach (int refinementBits in new[] { 0, 2 })
            {
                var progressiveOptions = options with
                {
                    OptimizeHuffmanTables = optimize, Progressive = true, ProgressiveRefinementBits = refinementBits
                };
                byte[] progressive = Encode(pixels, width, height, layout, progressiveOptions);
                JpegDocument actual = Read(progressive, progressiveOptions);
                AssertCoefficients(reference, actual);
                Assert.Equal(decoded.Data, ImageResult.FromMemory(progressive, channels).Data);
            }
        byte[] optimized = Encode(pixels, width, height, layout, options with { OptimizeHuffmanTables = true });
        AssertCoefficients(reference, Read(optimized, options));
        Assert.Equal(decoded.Data, ImageResult.FromMemory(optimized, channels).Data);
    }

    [Theory]
    [InlineData(false, PixelComponentLayout.Rgb)]
    [InlineData(false, PixelComponentLayout.Bgr)]
    [InlineData(false, PixelComponentLayout.Rgba)]
    [InlineData(false, PixelComponentLayout.Bgra)]
    [InlineData(false, PixelComponentLayout.Gray)]
    [InlineData(true, PixelComponentLayout.Rgb)]
    [InlineData(true, PixelComponentLayout.Bgr)]
    [InlineData(true, PixelComponentLayout.Rgba)]
    [InlineData(true, PixelComponentLayout.Bgra)]
    [InlineData(true, PixelComponentLayout.Gray)]
    public void LayoutsAndElementStridesIgnoreAlphaPaddingAndTrailingElements(bool floatingPoint, PixelComponentLayout layout)
    {
        foreach ((int width, int height) in new[] { (1, 17), (17, 1), (19, 33) })
        {
            PixelComponentLayout compactLayout = layout == PixelComponentLayout.Gray ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
            Array compact = Pattern(floatingPoint, width, height, compactLayout);
            int stride = width * Components(layout) + 13;
            var options = new JpegliEncoderOptions
            {
                SmoothingFactor = 73, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, compactLayout, options);
            // The shortest source has no final-row padding. The longer source has
            // invalid float sentinels in every ignored alpha, padding and trailing slot.
            foreach (int trailing in new[] { 0, 17 })
            {
                Array padded = Pattern(floatingPoint, width, height, layout, stride, trailing);
                Assert.Equal(expected, Encode(padded, width, height, layout, options, stride));
            }
        }
    }

    [Theory]
    [InlineData(false, PixelComponentLayout.Rgb)]
    [InlineData(false, PixelComponentLayout.Gray)]
    [InlineData(true, PixelComponentLayout.Rgb)]
    [InlineData(true, PixelComponentLayout.Gray)]
    public void StreamOverloadsMatchArrayAndLeaveNonSeekableDestinationOpen(bool floatingPoint, PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        Array pixels = Pattern(floatingPoint, width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] expected = Encode(pixels, width, height, layout, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, width, height, layout, options);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        Read(destination.ToArray(), options);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringProgressiveScansLeavesPartialOutputAndStreamOpen(bool floatingPoint)
    {
        const int width = 65, height = 33;
        Array pixels = Pattern(floatingPoint, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] complete = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        int cancelAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            EncodeWithCancellation(output, pixels, width, height, cancellation.Token, options: options));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IoFailurePropagatesAndLeavesPartialOutputAndStreamOpen(bool floatingPoint)
    {
        const int width = 33, height = 19;
        Array pixels = Pattern(floatingPoint, width, height, PixelComponentLayout.Rgb);
        var options = new JpegliEncoderOptions { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2 };
        byte[] complete = Encode(pixels, width, height, PixelComponentLayout.Rgb, options);
        int failAfter = Read(complete, options).Scans[1].DataStart + 1;
        var failure = new IOException("Injected destination failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= failAfter)
                throw failure;
        });
        IOException error = Assert.Throws<IOException>(() => Encode(output, pixels, width, height, options: options));
        Assert.Same(failure, error);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, failAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultLayoutAndOptionsAreRgb(bool floatingPoint)
    {
        Array pixels = Pattern(floatingPoint, 9, 11, PixelComponentLayout.Rgb);
        byte[] expected = Encode(pixels, 9, 11, PixelComponentLayout.Rgb, new());
        byte[] actual = pixels is float[] floats
            ? JpegliEncoder.Encode(floats, 9, 11, cancellationToken: TestContext.Current.CancellationToken)
            : JpegliEncoder.Encode((ushort[])pixels, 9, 11, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, PixelComponentLayout.Gray, ChromaSubsampling.Quarter)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb, ChromaSubsampling.None)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb, ChromaSubsampling.HalfHorizontal)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb, ChromaSubsampling.Quarter)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb, ChromaSubsampling.HalfVertical)]
    [InlineData(PixelLayout.Rgb24, PixelComponentLayout.Rgb, ChromaSubsampling.QuarterHorizontal)]
    public void ExpandedBytesProduceExactlyTheExistingByteEncoding(
        PixelLayout byteLayout, PixelComponentLayout layout, ChromaSubsampling sampling)
    {
        const int width = 19, height = 17;
        byte[] bytes = EncoderProgressiveTests.Pattern(width, height, byteLayout);
        ushort[] pixels = bytes.Select(value => (ushort)(value * 257)).ToArray();
        foreach (bool adaptive in new[] { false, true })
            foreach (int smoothing in new[] { 0, 37 })
                foreach (bool optimize in new[] { false, true })
                    foreach (bool progressive in new[] { false, true })
                    {
                        var options = new JpegliEncoderOptions
                        {
                            Subsampling = sampling, AdaptiveQuantization = adaptive, SmoothingFactor = smoothing,
                            OptimizeHuffmanTables = optimize, Progressive = progressive,
                            ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 2
                        };
                        byte[] expected = JpegliEncoder.Encode(bytes, width, height, byteLayout, options,
                            cancellationToken: TestContext.Current.CancellationToken);
                        Assert.Equal(expected, Encode(pixels, width, height, layout, options));
                    }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StreamValidationRejectsNullOrReadOnlyDestination(bool floatingPoint)
    {
        Array pixels = Pattern(floatingPoint, 1, 1, PixelComponentLayout.Rgb);
        Assert.Throws<ArgumentNullException>(() => Encode(null!, pixels, 1, 1));
        using var output = new MemoryStream(new byte[16], writable: false);
        var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels, 1, 1));
        Assert.Equal("output", error.ParamName);
        Assert.Equal(16, output.Length);
        Assert.Equal(new byte[16], output.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidationAndPreCancellationWriteNothing(bool floatingPoint)
    {
        Array pixels = Pattern(floatingPoint, 2, 2, PixelComponentLayout.Rgb);
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 65536, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 65536));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 2, 2, options: new() { MaxPixels = 3 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, options: new() { MaxPixels = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, options: new() { Quality = float.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, options: new() { SmoothingFactor = 101 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, options: new() { Subsampling = (ChromaSubsampling)99 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, layout: (PixelComponentLayout)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 1, 1, stride: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Encode(output, pixels, 2, 1, stride: 5));
        Assert.Throws<ArgumentException>(() => Encode(output, pixels, 3, 2));
        Assert.Throws<ArgumentException>(() => Encode(output, pixels, 2, 2, stride: 7));
        Assert.Throws<ArgumentException>(() => Encode(output, pixels, 1, 2, stride: int.MaxValue));
        var token = new CancellationToken(true);
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            EncodeWithCancellation(output, pixels, 2, 2, token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(-0.000001f)]
    [InlineData(1.000001f)]
    public void InvalidConsumedFloatSamplesAreRejectedBeforeOutput(float invalid)
    {
        const int width = 17, height = 9;
        foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>().Where(layout => layout != PixelComponentLayout.Xyb))
            foreach (bool optimize in new[] { false, true })
            {
                float[] pixels = (float[])Pattern(true, width, height, layout);
                // The invalid value is in a color channel of the last source pixel,
                // so rejecting only the initial block or validating after headers fails.
                pixels[(width * height - 1) * Components(layout) + (layout == PixelComponentLayout.Gray ? 0 : 2)] = invalid;
                using var output = new MemoryStream();
                var error = Assert.ThrowsAny<ArgumentException>(() => Encode(output, pixels, width, height, layout,
                    new() { OptimizeHuffmanTables = optimize, Progressive = true, ProgressiveRefinementBits = 2 }));
                Assert.Equal("pixels", error.ParamName);
                Assert.Equal(0, output.Length);
                Assert.True(output.CanWrite);
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullRangeEndpointsEncodeAsBlackAndWhite(bool floatingPoint)
    {
        foreach (bool white in new[] { false, true })
        {
            Array pixels = floatingPoint
                ? Enumerable.Repeat(white ? 1f : 0f, 64).ToArray()
                : Enumerable.Repeat(white ? ushort.MaxValue : ushort.MinValue, 64).ToArray();
            byte[] jpeg = Encode(pixels, 8, 8, PixelComponentLayout.Gray, new() { Quality = 100 });
            ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.Grey);
            Assert.All(decoded.Data, sample => Assert.Equal(white ? (byte)255 : (byte)0, sample));
        }
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in expected.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in expected.Blocks)
        {
            Assert.Equal(block.IsDummy, actual.Blocks[key].IsDummy);
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        }
    }

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Pattern(bool floatingPoint, int width, int height, PixelComponentLayout layout,
        int stride = 0, int trailing = 0)
    {
        int components = Components(layout);
        if (stride == 0)
            stride = width * components;
        int length = (height - 1) * stride + width * components + trailing;
        Array result = floatingPoint ? Enumerable.Repeat(float.NaN, length).ToArray()
            : Enumerable.Repeat(ushort.MaxValue, length).ToArray();
        bool bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < (layout == PixelComponentLayout.Gray ? 1 : 3); channel++)
                {
                    int offset = y * stride + x * components + (bgr ? 2 - channel : channel);
                    ushort value = (ushort)(4000 + (977 * x + 1703 * y + 11007 * channel + 59 * x * y) % 55000);
                    if (result is float[] floats)
                        floats[offset] = value / 65535f;
                    else
                        ((ushort[])result)[offset] = value;
                }
        if (result is float[] alpha && components == 4)
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    alpha[y * stride + x * components + 3] = ((x + y) % 4) switch
                    {
                        0 => float.NaN,
                        1 => float.PositiveInfinity,
                        2 => -3f,
                        _ => 4f
                    };
        return result;
    }

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0) => pixels is float[] floats
        ? JpegliEncoder.Encode(floats, width, height, layout, options, stride, TestContext.Current.CancellationToken)
        : JpegliEncoder.Encode((ushort[])pixels, width, height, layout, options, stride, TestContext.Current.CancellationToken);

    private static void Encode(Stream output, Array pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0) =>
        EncodeWithCancellation(output, pixels, width, height, TestContext.Current.CancellationToken, layout, options, stride);

    private static void EncodeWithCancellation(Stream output, Array pixels, int width, int height,
        CancellationToken cancellationToken, PixelComponentLayout layout = PixelComponentLayout.Rgb,
        JpegliEncoderOptions? options = null, int stride = 0)
    {
        if (pixels is float[] floats)
            JpegliEncoder.Encode(output, floats, width, height, layout, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (ushort[])pixels, width, height, layout, options, stride, cancellationToken);
    }

    private static byte FramePrecision(byte[] jpeg)
    {
        for (int position = 2; position < jpeg.Length;)
        {
            Assert.Equal(0xff, jpeg[position]);
            int marker = jpeg[position + 1];
            if (marker is 0xc0 or 0xc2)
                return jpeg[position + 4];
            int length = jpeg[position + 2] * 256 + jpeg[position + 3];
            position += length + 2;
        }
        throw new InvalidDataException("No JPEG frame marker was emitted.");
    }
}
