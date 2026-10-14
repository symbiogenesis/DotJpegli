using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class ExtendedEncoderSamplingTests
{
    private static readonly (ChromaSubsampling Sampling, int Horizontal, int Vertical)[] SamplingFactors =
    [
        (ChromaSubsampling.ThirdHorizontal, 3, 1),
        (ChromaSubsampling.ThirdHorizontalHalfVertical, 3, 2),
        (ChromaSubsampling.QuarterVertical, 1, 4),
        (ChromaSubsampling.QuarterHorizontalHalfVertical, 4, 2),
        (ChromaSubsampling.HalfHorizontalQuarterVertical, 2, 4)
    ];

    public static TheoryData<ChromaSubsampling, int, int> SamplingModes
    {
        get
        {
            var modes = new TheoryData<ChromaSubsampling, int, int>();
            foreach ((ChromaSubsampling sampling, int horizontal, int vertical) in SamplingFactors)
                modes.Add(sampling, horizontal, vertical);
            return modes;
        }
    }

    public static TheoryData<ChromaSubsampling, int, int, bool, bool, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<ChromaSubsampling, int, int, bool, bool, int>();
            foreach ((ChromaSubsampling sampling, int horizontal, int vertical) in SamplingFactors)
                foreach ((bool optimize, bool adaptive, int smoothing) in new[]
                {
                    (false, false, 0), (true, false, 37),
                    (false, true, 73), (true, true, 100)
                })
                    modes.Add(sampling, horizontal, vertical, optimize, adaptive, smoothing);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void OddEdgesPreserveSamplingAndRealCoefficientsAcrossEntropyModes(
        ChromaSubsampling sampling, int horizontal, int vertical,
        bool optimize, bool adaptive, int smoothing)
    {
        // Narrow inputs produce entirely dummy rows or columns in an MCU.
        // The larger input has enough MCUs to wrap the restart sequence even
        // with the ten-block 4x2 and 2x4 modes.
        foreach ((int width, int height) in new[]
        {
            (1, 1), (1, 33), (33, 1), (23, 31), (24, 32), (25, 33), (35, 37), (145, 65)
        })
        {
            byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
            var options = new JpegliEncoderOptions
            {
                Quality = 90, Subsampling = sampling, OptimizeHuffmanTables = optimize,
                AdaptiveQuantization = adaptive, SmoothingFactor = smoothing
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
            AssertSampling(reference, width, height, horizontal, vertical);
            AssertDummyDcContinuity(reference, horizontal, vertical, interval: 0, refinementBits: 0);
            byte[] baselinePixels = ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data;

            byte[] alternate = Encode(pixels, width, height, PixelLayout.Rgb24,
                options with { OptimizeHuffmanTables = !optimize });
            JpegDocument alternateDocument = JpegDocument.Read(alternate, progressive: false, expectedInterval: 0);
            AssertParity(baseline, reference, alternate, alternateDocument, baselinePixels);
            AssertDummyDcContinuity(alternateDocument, horizontal, vertical, interval: 0, refinementBits: 0);

            foreach ((bool progressive, int refinement, int interval) in new[]
            {
                (false, 0, 2), (true, 0, 1), (true, 2, 7)
            })
            {
                byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
                {
                    Progressive = progressive, ProgressiveRefinementBits = refinement,
                    RestartInterval = interval
                });
                // The independent reader verifies every scan's unit count,
                // restart count/cycle, reset predictors and entropy padding.
                JpegDocument actual = JpegDocument.Read(jpeg, progressive, interval, refinement);
                AssertSampling(actual, width, height, horizontal, vertical);
                AssertParity(baseline, reference, jpeg, actual, baselinePixels);
                AssertDummyDcContinuity(actual, horizontal, vertical, interval, refinement);
            }
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void MaximumRefinementDepthPreservesRealCoefficientsAndDummyDc(
        ChromaSubsampling sampling, int horizontal, int vertical)
    {
        const int width = 35, height = 37;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        foreach (bool optimize in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 100, Subsampling = sampling, AdaptiveQuantization = false,
                OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
            {
                Progressive = true, ProgressiveRefinementBits = 10
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: 2, expectedRefinementBits: 10);
            AssertSampling(actual, width, height, horizontal, vertical);
            AssertParity(baseline, reference, jpeg, actual);
            AssertDummyDcContinuity(actual, horizontal, vertical, interval: 2, refinementBits: 10);
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentBoxAverageDcOracleCoversColorAndReplicatedEdges(
        ChromaSubsampling sampling, int horizontal, int vertical)
    {
        const int width = 35, height = 37;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        foreach (bool adaptive in new[] { false, true })
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, new()
            {
                Quality = 90, Subsampling = sampling, AdaptiveQuantization = adaptive,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            });
            JpegDocument document = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: 2, expectedRefinementBits: 2);
            AssertSampling(document, width, height, horizontal, vertical);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                int sampleWidth = key.Component == 1 ? 1 : horizontal;
                int sampleHeight = key.Component == 1 ? 1 : vertical;
                double sum = 0;
                for (int y = 0; y < 8 * sampleHeight; y++)
                    for (int x = 0; x < 8 * sampleWidth; x++)
                    {
                        int offset = (Math.Min(key.Y * 8 * sampleHeight + y, height - 1) * width
                            + Math.Min(key.X * 8 * sampleWidth + x, width - 1)) * 3;
                        double red = pixels[offset], green = pixels[offset + 1], blue = pixels[offset + 2];
                        double luma = 0.299 * red + 0.587 * green + 0.114 * blue;
                        sum += key.Component switch
                        {
                            1 => luma - 128,
                            2 => (blue - luma) / 1.772,
                            _ => (red - luma) / 1.402
                        };
                    }
                int quantizer = document.Quantizers[document.Components[key.Component].Quantizer][0];
                double expectedDc = sum / (8 * sampleWidth * sampleHeight * quantizer);
                // Independent double BT.601 and box averaging allow a half tie
                // plus float rounding, without using encoder conversion helpers.
                Assert.InRange(Math.Abs(block.Coefficients[0] - expectedDc), 0, 0.501);
            }
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void PaddedByteAndHighPrecisionLayoutsPreserveTheCompactEncoding(
        ChromaSubsampling sampling, int horizontal, int vertical)
    {
        const int width = 35, height = 37;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        foreach ((bool progressive, int refinement, bool optimize) in new[]
        {
            (false, 0, false), (true, 0, true), (true, 2, false), (true, 2, true)
        })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 90, Subsampling = sampling, SmoothingFactor = 73,
                Progressive = progressive, ProgressiveRefinementBits = refinement,
                OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, PixelLayout.Rgb24, options);
            foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Bgr24,
                PixelLayout.Rgba32, PixelLayout.Bgra32 })
            {
                int channels = layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
                int stride = width * channels + 13;
                byte[] padded = PaddedBytes(compact, width, height, layout, stride);
                byte[] actual = JpegliEncoder.Encode(padded, width, height, layout, options, stride,
                    cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal(expected, actual);

                // Multiples of 257 map to exactly the same byte-scale samples.
                ushort[] words = padded.Select(value => (ushort)(value * 257)).ToArray();
                PixelComponentLayout componentLayout = layout switch
                {
                    PixelLayout.Rgb24 => PixelComponentLayout.Rgb,
                    PixelLayout.Bgr24 => PixelComponentLayout.Bgr,
                    PixelLayout.Rgba32 => PixelComponentLayout.Rgba,
                    _ => PixelComponentLayout.Bgra
                };
                Assert.Equal(expected, JpegliEncoder.Encode(words, width, height, componentLayout,
                    options, stride, TestContext.Current.CancellationToken));
            }
            JpegDocument document = JpegDocument.Read(expected, progressive, expectedInterval: 2, refinement);
            AssertSampling(document, width, height, horizontal, vertical);
        }

        // Fractional float samples exercise a different source reader. NaN
        // alpha and row padding must remain unconsumed with smoothing enabled.
        float[] floats = compact.Select((value, index) => (value + (index % 7) / 8f) / 255f).ToArray();
        const int floatStride = width * 4 + 13;
        float[] paddedFloats = Enumerable.Repeat(float.NaN, (height - 1) * floatStride + width * 4).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * floatStride + x * 4;
                paddedFloats[output] = floats[input + 2];
                paddedFloats[output + 1] = floats[input + 1];
                paddedFloats[output + 2] = floats[input];
            }
        var floatOptions = new JpegliEncoderOptions
        {
            Subsampling = sampling, SmoothingFactor = 100, RestartInterval = 2
        };
        byte[] floatBaseline = JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgb,
            floatOptions, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(floatBaseline, JpegliEncoder.Encode(paddedFloats, width, height, PixelComponentLayout.Bgra,
            floatOptions, floatStride, TestContext.Current.CancellationToken));
        byte[] floatProgressive = JpegliEncoder.Encode(paddedFloats, width, height, PixelComponentLayout.Bgra,
            floatOptions with { Progressive = true, ProgressiveRefinementBits = 2 },
            floatStride, TestContext.Current.CancellationToken);
        JpegDocument floatReference = JpegDocument.Read(floatBaseline, progressive: false, expectedInterval: 2);
        JpegDocument floatActual = JpegDocument.Read(floatProgressive, progressive: true,
            expectedInterval: 2, expectedRefinementBits: 2);
        AssertSampling(floatActual, width, height, horizontal, vertical);
        AssertParity(floatBaseline, floatReference, floatProgressive, floatActual);
        AssertDummyDcContinuity(floatActual, horizontal, vertical, interval: 2, refinementBits: 2);
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void GrayscaleIgnoresTheColorSamplingSelection(
        ChromaSubsampling sampling, int horizontal, int vertical)
    {
        Assert.InRange(horizontal * vertical + 2, 3, 10);
        const int width = 17, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Subsampling = ChromaSubsampling.None, SmoothingFactor = 73,
                Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
            };
            byte[] expected = Encode(pixels, width, height, PixelLayout.Gray8, options);
            byte[] actual = Encode(pixels, width, height, PixelLayout.Gray8, options with { Subsampling = sampling });
            Assert.Equal(expected, actual);
            JpegDocument document = JpegDocument.Read(actual, progressive, expectedInterval: 2, refinement);
            Component component = Assert.Single(document.Components).Value;
            Assert.Equal((1, 1), (component.H, component.V));
            Assert.DoesNotContain(document.Blocks.Values, block => block.IsDummy);
        }
    }

    private static void AssertSampling(JpegDocument document, int width, int height, int horizontal, int vertical)
    {
        Assert.Equal(width, document.Width);
        Assert.Equal(height, document.Height);
        Assert.Equal(3, document.Components.Count);
        Assert.Equal((horizontal, vertical), (document.Components[1].H, document.Components[1].V));
        Assert.Equal((1, 1), (document.Components[2].H, document.Components[2].V));
        Assert.Equal((1, 1), (document.Components[3].H, document.Components[3].V));
        int blocksPerMcu = document.Components.Values.Sum(component => component.H * component.V);
        Assert.Equal(horizontal * vertical + 2, blocksPerMcu);
        Assert.InRange(blocksPerMcu, 1, 10);
        int mcuCount = Ceiling(width, 8 * horizontal) * Ceiling(height, 8 * vertical);
        Assert.Equal(mcuCount, document.McuCount);
        Assert.Equal(mcuCount * blocksPerMcu, document.Blocks.Count);
        Assert.Equal(mcuCount * horizontal * vertical - Ceiling(width, 8) * Ceiling(height, 8),
            document.Blocks.Count(pair => pair.Key.Component == 1 && pair.Value.IsDummy));
        Assert.DoesNotContain(document.Blocks, pair => pair.Key.Component != 1 && pair.Value.IsDummy);
    }

    private static void AssertParity(byte[] baseline, JpegDocument reference, byte[] jpeg, JpegDocument actual,
        byte[]? baselinePixels = null)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in reference.Blocks)
        {
            Assert.Equal(block.IsDummy, actual.Blocks[key].IsDummy);
            if (!block.IsDummy)
                Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        }
        ImageResult decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        Assert.Equal(reference.Width, decoded.Width);
        Assert.Equal(reference.Height, decoded.Height);
        Assert.Equal(baselinePixels ?? ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data, decoded.Data);
    }

    private static void AssertDummyDcContinuity(JpegDocument document, int horizontal, int vertical,
        int interval, int refinementBits)
    {
        int columns = Ceiling(document.Width, 8 * horizontal), previousDc = 0;
        for (int mcu = 0; mcu < document.McuCount; mcu++)
        {
            if (interval != 0 && mcu % interval == 0)
                previousDc = 0;
            for (int y = 0; y < vertical; y++)
                for (int x = 0; x < horizontal; x++)
                {
                    Block block = document.Blocks[new(1, mcu % columns * horizontal + x, mcu / columns * vertical + y)];
                    Assert.Equal((block.Coefficients[0] >> refinementBits) - (previousDc >> refinementBits),
                        block.DcDifference);
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

    private static byte[] PaddedBytes(byte[] compact, int width, int height, PixelLayout layout, int stride)
    {
        int channels = layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * channels).ToArray();
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * stride + x * channels;
                padded[output + (bgr ? 2 : 0)] = compact[input];
                padded[output + 1] = compact[input + 1];
                padded[output + (bgr ? 0 : 2)] = compact[input + 2];
                if (channels == 4)
                    padded[output + 3] = (byte)((17 * x + 29 * y) % 256);
            }
        return padded;
    }

    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;
}
