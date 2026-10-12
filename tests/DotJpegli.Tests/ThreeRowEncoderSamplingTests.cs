using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class ThreeRowEncoderSamplingTests
{
    public static TheoryData<ChromaSubsampling, int> SamplingModes => new()
    {
        { ChromaSubsampling.ThirdVertical, 1 },
        { ChromaSubsampling.HalfHorizontalThirdVertical, 2 }
    };

    public static TheoryData<ChromaSubsampling, int, bool, bool, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<ChromaSubsampling, int, bool, bool, int>();
            foreach ((ChromaSubsampling sampling, int horizontal) in new[]
            {
                (ChromaSubsampling.ThirdVertical, 1),
                (ChromaSubsampling.HalfHorizontalThirdVertical, 2)
            })
                foreach ((bool optimize, bool adaptive, int smoothing) in new[]
                {
                    (false, false, 0), (true, false, 37),
                    (false, true, 73), (true, true, 100)
                })
                    modes.Add(sampling, horizontal, optimize, adaptive, smoothing);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void ThreeRowMcuEdgesPreserveCoefficientsAcrossEntropyAndScanModes(
        ChromaSubsampling sampling, int horizontal, bool optimize, bool adaptive, int smoothing)
    {
        // Check both sides of the 24-row MCU boundary, wholly dummy luma
        // rows/columns, and enough MCUs for RST7 to wrap back to RST0.
        foreach ((int width, int height) in new[]
        {
            (1, 1), (1, 25), (17, 1), (17, 23), (17, 24), (17, 25), (145, 49)
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
            AssertSamplingAndDummyDc(reference, width, height, horizontal, interval: 0, refinement: 0);

            byte[] alternate = Encode(pixels, width, height, PixelLayout.Rgb24,
                options with { OptimizeHuffmanTables = !optimize });
            JpegDocument other = JpegDocument.Read(alternate, progressive: false, expectedInterval: 0);
            AssertBaselineParity(baseline, reference, alternate, other, PixelLayout.Rgb24);
            AssertSamplingAndDummyDc(other, width, height, horizontal, interval: 0, refinement: 0);

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
                // The independent reader checks scan unit counts, restart
                // count/cycle, reset predictors and trailing entropy padding.
                JpegDocument actual = JpegDocument.Read(jpeg, progressive, interval, refinement);
                AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
                AssertSamplingAndDummyDc(actual, width, height, horizontal, interval, refinement);
            }
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void IndependentBoxAverageDcOracleCoversThreeRowChromaAndReplicatedEdges(
        ChromaSubsampling sampling, int horizontal)
    {
        const int width = 17, height = 25;
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
            AssertSamplingAndDummyDc(document, width, height, horizontal, interval: 2, refinement: 2);
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                int sampleWidth = key.Component == 1 ? 1 : horizontal;
                int sampleHeight = key.Component == 1 ? 1 : 3;
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
                double expected = sum / (8 * sampleWidth * sampleHeight * quantizer);
                // Permit an exact half tie and float rounding in the encoder.
                Assert.InRange(Math.Abs(block.Coefficients[0] - expected), 0, 0.501);
            }
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void MaximumRefinementPreservesRealBlocksAndDummyDc(
        ChromaSubsampling sampling, int horizontal)
    {
        const int width = 17, height = 25;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        foreach (bool optimize in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 100, Subsampling = sampling, AdaptiveQuantization = false,
                OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            byte[] progressive = Encode(pixels, width, height, PixelLayout.Rgb24,
                options with { Progressive = true, ProgressiveRefinementBits = 10 });
            JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
            JpegDocument actual = JpegDocument.Read(progressive, progressive: true,
                expectedInterval: 2, expectedRefinementBits: 10);
            AssertBaselineParity(baseline, reference, progressive, actual, PixelLayout.Rgb24);
            AssertSamplingAndDummyDc(actual, width, height, horizontal, interval: 2, refinement: 10);
        }
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void PaddedBgraByteAndHighPrecisionInputsMatchCompactRgb(
        ChromaSubsampling sampling, int horizontal)
    {
        const int width = 17, height = 25, stride = width * 4 + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * 4).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * stride + x * 4;
                padded[output] = compact[input + 2];
                padded[output + 1] = compact[input + 1];
                padded[output + 2] = compact[input];
                padded[output + 3] = (byte)(17 * x + 29 * y);
            }
        ushort[] words = padded.Select(value => (ushort)(value * 257)).ToArray();
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Subsampling = sampling, SmoothingFactor = 73, AdaptiveQuantization = true,
                Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, PixelLayout.Rgb24, options);
            Assert.Equal(expected, JpegliEncoder.Encode(padded, width, height, PixelLayout.Bgra32,
                options, stride, TestContext.Current.CancellationToken));
            Assert.Equal(expected, JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken));
            JpegDocument document = JpegDocument.Read(expected, progressive, expectedInterval: 2, refinement);
            AssertSamplingAndDummyDc(document, width, height, horizontal, interval: 2, refinement);
        }

        // Non-byte-aligned samples exercise the float reader; NaN padding and
        // alpha must stay unconsumed. The final row has no trailing padding.
        float[] floats = compact.Select((value, index) => (value + (index % 7) / 8f) / 255f).ToArray();
        float[] paddedFloats = Enumerable.Repeat(float.NaN, (height - 1) * stride + width * 4).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3, output = y * stride + x * 4;
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
            floatOptions, stride, TestContext.Current.CancellationToken));
        byte[] floatProgressive = JpegliEncoder.Encode(paddedFloats, width, height, PixelComponentLayout.Bgra,
            floatOptions with { Progressive = true, ProgressiveRefinementBits = 2 },
            stride, TestContext.Current.CancellationToken);
        JpegDocument floatReference = JpegDocument.Read(floatBaseline, progressive: false, expectedInterval: 2);
        JpegDocument floatActual = JpegDocument.Read(floatProgressive, progressive: true,
            expectedInterval: 2, expectedRefinementBits: 2);
        AssertBaselineParity(floatBaseline, floatReference, floatProgressive, floatActual, PixelLayout.Rgb24);
        AssertSamplingAndDummyDc(floatActual, width, height, horizontal, interval: 2, refinement: 2);
    }

    [Theory]
    [MemberData(nameof(SamplingModes))]
    public void GrayscaleIgnoresThreeRowColorSamplingAndRowPadding(
        ChromaSubsampling sampling, int horizontal)
    {
        Assert.InRange(horizontal, 1, 2);
        const int width = 17, height = 25, stride = width + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Gray8);
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width).ToArray();
        for (int y = 0; y < height; y++)
            compact.AsSpan(y * width, width).CopyTo(padded.AsSpan(y * stride, width));
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            var options = new JpegliEncoderOptions
            {
                Subsampling = ChromaSubsampling.None, SmoothingFactor = 73,
                Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
            };
            byte[] expected = Encode(compact, width, height, PixelLayout.Gray8, options);
            byte[] actual = JpegliEncoder.Encode(padded, width, height, PixelLayout.Gray8,
                options with { Subsampling = sampling }, stride, TestContext.Current.CancellationToken);
            Assert.Equal(expected, actual);
            JpegDocument document = JpegDocument.Read(actual, progressive, expectedInterval: 2, refinement);
            Component component = Assert.Single(document.Components).Value;
            Assert.Equal((1, 1), (component.H, component.V));
            Assert.DoesNotContain(document.Blocks.Values, block => block.IsDummy);
        }
    }

    private static void AssertSamplingAndDummyDc(JpegDocument document, int width, int height,
        int horizontal, int interval, int refinement)
    {
        const int vertical = 3;
        Assert.Equal(width, document.Width);
        Assert.Equal(height, document.Height);
        Assert.Equal(3, document.Components.Count);
        Assert.Equal((horizontal, vertical), (document.Components[1].H, document.Components[1].V));
        Assert.Equal((1, 1), (document.Components[2].H, document.Components[2].V));
        Assert.Equal((1, 1), (document.Components[3].H, document.Components[3].V));
        int columns = Ceiling(width, 8 * horizontal);
        int mcuCount = columns * Ceiling(height, 8 * vertical);
        Assert.Equal(mcuCount, document.McuCount);
        Assert.Equal(mcuCount * (horizontal * vertical + 2), document.Blocks.Count);
        Assert.Equal(mcuCount * horizontal * vertical - Ceiling(width, 8) * Ceiling(height, 8),
            document.Blocks.Count(pair => pair.Key.Component == 1 && pair.Value.IsDummy));
        Assert.DoesNotContain(document.Blocks, pair => pair.Key.Component != 1 && pair.Value.IsDummy);
        int previousDc = 0;
        for (int mcu = 0; mcu < mcuCount; mcu++)
        {
            if (interval != 0 && mcu % interval == 0)
                previousDc = 0;
            for (int y = 0; y < vertical; y++)
                for (int x = 0; x < horizontal; x++)
                {
                    Block block = document.Blocks[new(1, mcu % columns * horizontal + x, mcu / columns * vertical + y)];
                    Assert.Equal((block.Coefficients[0] >> refinement) - (previousDc >> refinement),
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

    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;
}
