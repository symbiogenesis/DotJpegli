using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class AdditionalEncoderSamplingTests
{
    public static TheoryData<ChromaSubsampling, int, int, bool, bool, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<ChromaSubsampling, int, int, bool, bool, int>();
            foreach ((ChromaSubsampling sampling, int horizontal, int vertical) in new[]
            {
                (ChromaSubsampling.HalfVertical, 1, 2),
                (ChromaSubsampling.QuarterHorizontal, 4, 1)
            })
                foreach (bool optimize in new[] { false, true })
                    foreach (bool adaptive in new[] { false, true })
                        foreach (int smoothing in new[] { 0, 73 })
                            modes.Add(sampling, horizontal, vertical, optimize, adaptive, smoothing);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void MarkerFactorsAndRealCoefficientsSurviveAllScanKindsAndDummyPadding(
        ChromaSubsampling sampling, int horizontal, int vertical,
        bool optimize, bool adaptive, int smoothing)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 17), (145, 17) })
        {
            byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
            var options = new JpegliEncoderOptions
            {
                Quality = 90, Subsampling = sampling,
                OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive,
                SmoothingFactor = smoothing
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
            AssertSampling(reference, width, height, horizontal, vertical);
            AssertDummyDcContinuity(reference, horizontal, vertical, interval: 0, refinementBits: 0);

            // Huffman optimization must change only entropy coding, including the
            // wholly padded blocks at the right or bottom edge of an MCU.
            byte[] otherHuffman = Encode(pixels, width, height, PixelLayout.Rgb24,
                options with { OptimizeHuffmanTables = !optimize });
            JpegDocument other = JpegDocument.Read(otherHuffman, progressive: false, expectedInterval: 0);
            AssertBaselineParity(baseline, reference, otherHuffman, other, PixelLayout.Rgb24);
            AssertDummyDcContinuity(other, horizontal, vertical, interval: 0, refinementBits: 0);

            foreach ((bool progressive, int refinement, int interval) in new[]
            {
                (false, 0, 2), (true, 0, 1), (true, 2, 7), (true, 10, 2)
            })
            {
                byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
                {
                    Progressive = progressive, ProgressiveRefinementBits = refinement,
                    RestartInterval = interval
                });
                JpegDocument actual = JpegDocument.Read(jpeg, progressive, interval, refinement);
                AssertSampling(actual, width, height, horizontal, vertical);
                AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
                AssertDummyDcContinuity(actual, horizontal, vertical, interval, refinement);
            }
        }
    }

    [Theory]
    [InlineData(ChromaSubsampling.HalfVertical, 1, 2, false, false)]
    [InlineData(ChromaSubsampling.HalfVertical, 1, 2, false, true)]
    [InlineData(ChromaSubsampling.HalfVertical, 1, 2, true, false)]
    [InlineData(ChromaSubsampling.HalfVertical, 1, 2, true, true)]
    [InlineData(ChromaSubsampling.QuarterHorizontal, 4, 1, false, false)]
    [InlineData(ChromaSubsampling.QuarterHorizontal, 4, 1, false, true)]
    [InlineData(ChromaSubsampling.QuarterHorizontal, 4, 1, true, false)]
    [InlineData(ChromaSubsampling.QuarterHorizontal, 4, 1, true, true)]
    public void NeutralInputHasIndependentLumaDcOracleAcrossDummyBlocksAndRestartResets(
        ChromaSubsampling sampling, int horizontal, int vertical, bool optimize, bool adaptive)
    {
        const int width = 33, height = 17;
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Array.Fill(pixels, (byte)(32 + (11 * x + 19 * y + 7 * (x / 8) + 13 * (y / 8)) % 192),
                    (y * width + x) * 3, 3);
        foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, new()
            {
                Quality = 90, Subsampling = sampling,
                OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive,
                Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
            });
            JpegDocument document = JpegDocument.Read(jpeg, progressive, expectedInterval: 2, refinement);
            AssertSampling(document, width, height, horizontal, vertical);
            AssertDummyDcContinuity(document, horizontal, vertical, interval: 2, refinement);
            int quantizer = document.Quantizers[document.Components[1].Quantizer][0];
            foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => pair.Key.Component == 1
                && !pair.Value.IsDummy))
            {
                double sum = 0;
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        sum += pixels[(Math.Min(key.Y * 8 + y, height - 1) * width
                            + Math.Min(key.X * 8 + x, width - 1)) * 3] - 128;
                // DC is the sum / 8 before quantization. AQ leaves DC rounding
                // unchanged; permit either neighbor at an exact half tie.
                Assert.InRange(Math.Abs(block.Coefficients[0] - sum / (8 * quantizer)), 0, 0.501);
            }
        }
    }

    [Theory]
    [InlineData(ChromaSubsampling.HalfVertical)]
    [InlineData(ChromaSubsampling.QuarterHorizontal)]
    public void PaddedColorLayoutsIgnoreRowPaddingAndAlpha(ChromaSubsampling sampling)
    {
        foreach ((int width, int height) in new[] { (1, 17), (33, 1), (35, 19) })
        {
            byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
            foreach ((bool progressive, int refinement, bool optimize) in new[]
            {
                (false, 0, false), (true, 0, true), (true, 2, false), (true, 2, true)
            })
            {
                var options = new JpegliEncoderOptions
                {
                    Quality = 90, Subsampling = sampling, SmoothingFactor = 100,
                    Progressive = progressive, ProgressiveRefinementBits = refinement,
                    OptimizeHuffmanTables = optimize, RestartInterval = 2
                };
                byte[] expected = Encode(compact, width, height, PixelLayout.Rgb24, options);
                foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Bgr24,
                    PixelLayout.Rgba32, PixelLayout.Bgra32 })
                {
                    int channels = layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
                    int stride = width * channels + 13;
                    byte[] padded = PaddedPixels(compact, width, height, layout, stride);
                    byte[] actual = JpegliEncoder.Encode(padded, width, height, layout, options, stride,
                        cancellationToken: TestContext.Current.CancellationToken);
                    Assert.Equal(expected, actual);
                    JpegDocument.Read(actual, progressive, expectedInterval: 2, refinement);
                    if (channels == 4)
                    {
                        for (int y = 0; y < height; y++)
                            for (int x = 0; x < width; x++)
                                padded[y * stride + x * channels + 3] ^= 0xff;
                        Assert.Equal(expected, JpegliEncoder.Encode(padded, width, height, layout, options, stride,
                            cancellationToken: TestContext.Current.CancellationToken));
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(ChromaSubsampling.HalfVertical)]
    [InlineData(ChromaSubsampling.QuarterHorizontal)]
    public void Gray8IgnoresColorSamplingForEveryScanKindAndPaddedRows(ChromaSubsampling sampling)
    {
        const int width = 17, height = 33, stride = width + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Gray8);
        byte[] padded = PaddedPixels(compact, width, height, PixelLayout.Gray8, stride);
        foreach (bool optimize in new[] { false, true })
            foreach (bool adaptive in new[] { false, true })
                foreach ((bool progressive, int refinement) in new[] { (false, 0), (true, 0), (true, 2) })
                {
                    var options = new JpegliEncoderOptions
                    {
                        Subsampling = ChromaSubsampling.None, SmoothingFactor = 73,
                        OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive,
                        Progressive = progressive, ProgressiveRefinementBits = refinement, RestartInterval = 2
                    };
                    byte[] expected = Encode(compact, width, height, PixelLayout.Gray8, options);
                    byte[] actual = JpegliEncoder.Encode(padded, width, height, PixelLayout.Gray8,
                        options with { Subsampling = sampling }, stride,
                        cancellationToken: TestContext.Current.CancellationToken);
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
        int mcuCount = Ceiling(width, 8 * horizontal) * Ceiling(height, 8 * vertical);
        Assert.Equal(mcuCount, document.McuCount);
        Assert.Equal(mcuCount * (horizontal * vertical + 2), document.Blocks.Count);
        int lumaRealBlocks = Ceiling(width, 8) * Ceiling(height, 8);
        Assert.Equal(mcuCount * horizontal * vertical - lumaRealBlocks,
            document.Blocks.Count(pair => pair.Key.Component == 1 && pair.Value.IsDummy));
        Assert.DoesNotContain(document.Blocks, pair => pair.Key.Component != 1 && pair.Value.IsDummy);
    }

    private static void AssertDummyDcContinuity(JpegDocument document, int horizontal, int vertical,
        int interval, int refinementBits)
    {
        int columns = Ceiling(document.Width, 8 * horizontal);
        int previousDc = 0;
        for (int mcu = 0; mcu < document.McuCount; mcu++)
        {
            if (interval != 0 && mcu % interval == 0)
                previousDc = 0;
            for (int y = 0; y < vertical; y++)
                for (int x = 0; x < horizontal; x++)
                {
                    var key = new BlockKey(1, mcu % columns * horizontal + x, mcu / columns * vertical + y);
                    Block block = document.Blocks[key];
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

    private static byte[] PaddedPixels(byte[] compact, int width, int height, PixelLayout layout, int stride)
    {
        int channels = layout == PixelLayout.Gray8 ? 1
            : layout is PixelLayout.Rgba32 or PixelLayout.Bgra32 ? 4 : 3;
        // Supply exactly the bytes the API requires, with no padding after the final row.
        byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * channels).ToArray();
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int output = y * stride + x * channels;
                if (layout == PixelLayout.Gray8)
                {
                    padded[output] = compact[y * width + x];
                    continue;
                }
                int input = (y * width + x) * 3;
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
