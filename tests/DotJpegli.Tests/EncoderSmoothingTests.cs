using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderSmoothingTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(true, 2)]
    public void DefaultSmoothingAndExplicitZeroProduceIdenticalBytes(bool progressive, int refinementBits)
    {
        Assert.Equal(0, new JpegliEncoderOptions().SmoothingFactor);
        const int width = 19, height = 33;
        foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Gray8 })
            foreach (bool optimize in new[] { false, true })
                foreach (bool adaptive in new[] { false, true })
                {
                    byte[] pixels = Pattern(width, height, layout);
                    var options = new JpegliEncoderOptions
                    {
                        Progressive = progressive, ProgressiveRefinementBits = refinementBits,
                        OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive, RestartInterval = 2
                    };
                    Assert.Equal(Encode(pixels, width, height, layout, options),
                        Encode(pixels, width, height, layout, options with { SmoothingFactor = 0 }));
                }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidSmoothingFactorWritesNoOutput(int factor)
    {
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(destination, new byte[3], 1, 1,
                options: new() { SmoothingFactor = factor },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.SmoothingFactor), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    public static TheoryData<PixelLayout, ChromaSubsampling, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, ChromaSubsampling, bool>();
            foreach (bool adaptive in new[] { false, true })
            {
                modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, adaptive);
                foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                    modes.Add(PixelLayout.Rgb24, sampling, adaptive);
            }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void SmoothingPreservesCoefficientsAcrossEntropyAndScanModes(
        PixelLayout layout, ChromaSubsampling sampling, bool adaptive)
    {
        // Partial edges cross iMCU rows and enough restart boundaries to cycle RST0..7.
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling, SmoothingFactor = 37,
            AdaptiveQuantization = adaptive, OptimizeHuffmanTables = false
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        foreach ((bool progressive, int bits, bool optimize, int interval) in new[]
        {
            (false, 0, true, 3), (true, 0, false, 1),
            (true, 2, true, 7), (true, 10, false, 2)
        })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits,
                OptimizeHuffmanTables = optimize, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive, interval, bits);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
        }
    }

    [Theory]
    [InlineData(1, 17)]
    [InlineData(17, 1)]
    [InlineData(9, 11)]
    [InlineData(17, 17)]
    public void GrayscaleCoefficientsMatchIndependentSmoothingAndDctOracle(int width, int height)
    {
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        foreach (int factor in new[] { 1, 37, 100 })
            foreach (bool optimize in new[] { false, true })
            {
                byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, new()
                {
                    Quality = 95, SmoothingFactor = factor, AdaptiveQuantization = false,
                    OptimizeHuffmanTables = optimize, RestartInterval = 2
                });
                JpegDocument document = JpegDocument.Read(jpeg, progressive: false, expectedInterval: 2);
                byte[] quantizer = document.Quantizers[document.Components[1].Quantizer];
                foreach ((BlockKey key, Block block) in document.Blocks)
                {
                    Assert.False(block.IsDummy); // Grayscale has one block per MCU.
                    double[] samples = OracleSamples(pixels, width, height, key.X * 8, key.Y * 8, factor);
                    for (int zigzag = 0; zigzag < 64; zigzag++)
                    {
                        int natural = Zigzag[zigzag];
                        double value = OracleDct(samples, natural % 8, natural / 8) / quantizer[zigzag];
                        // Float DCT arithmetic can straddle an exact rounding tie;
                        // every coefficient must still be the closest integer.
                        Assert.InRange(Math.Abs(block.Coefficients[zigzag] - value), 0, 0.501);
                    }
                }
            }
    }

    [Theory]
    [InlineData(PixelLayout.Bgr24, 3)]
    [InlineData(PixelLayout.Rgba32, 4)]
    [InlineData(PixelLayout.Bgra32, 4)]
    [InlineData(PixelLayout.Gray8, 1)]
    public void NarrowAndOddPaddedLayoutsIgnorePaddingAndAlpha(PixelLayout layout, int bytesPerPixel)
    {
        foreach ((int width, int height) in new[] { (1, 17), (17, 1), (19, 33) })
        {
            PixelLayout compactLayout = layout == PixelLayout.Gray8 ? PixelLayout.Gray8 : PixelLayout.Rgb24;
            byte[] compact = Pattern(width, height, compactLayout);
            int stride = width * bytesPerPixel + 13;
            // There is deliberately no trailing padding after the last row.
            byte[] padded = Enumerable.Repeat((byte)0xe5, (height - 1) * stride + width * bytesPerPixel).ToArray();
            bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int output = y * stride + x * bytesPerPixel;
                    if (layout == PixelLayout.Gray8)
                    {
                        padded[output] = compact[y * width + x];
                        continue;
                    }
                    int input = (y * width + x) * 3;
                    padded[output + (bgr ? 2 : 0)] = compact[input];
                    padded[output + 1] = compact[input + 1];
                    padded[output + (bgr ? 0 : 2)] = compact[input + 2];
                    if (bytesPerPixel == 4)
                        padded[output + 3] = (byte)((x * 17 + y * 29) % 256);
                }
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SmoothingFactor = 100, Progressive = true, ProgressiveRefinementBits = 2,
                RestartInterval = 2, Subsampling = ChromaSubsampling.Quarter
            };
            byte[] expected = Encode(compact, width, height, compactLayout, options);
            byte[] actual = JpegliEncoder.Encode(padded, width, height, layout, options, stride,
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(expected, actual);
            JpegDocument.Read(actual, progressive: true, expectedInterval: 2, expectedRefinementBits: 2);
            if (bytesPerPixel == 4)
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        padded[y * stride + x * bytesPerPixel + 3] ^= 0xff;
                Assert.Equal(expected, JpegliEncoder.Encode(padded, width, height, layout, options, stride,
                    cancellationToken: TestContext.Current.CancellationToken));
            }
        }
    }

    [Fact]
    public void SmoothingAttenuatesDecodedCheckerboardNoise()
    {
        const int width = 33, height = 17;
        byte[] pixels = new byte[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = (byte)(((x + y) & 1) == 0 ? 72 : 184);
        var options = new JpegliEncoderOptions { Quality = 100, AdaptiveQuantization = false };
        byte[] original = Encode(pixels, width, height, PixelLayout.Gray8, options);
        byte[] smoothed = Encode(pixels, width, height, PixelLayout.Gray8, options with { SmoothingFactor = 100 });
        Assert.False(original.SequenceEqual(smoothed));
        ImageResult decoded = ImageResult.FromMemory(smoothed, ColorComponents.Grey);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        byte[] reference = ImageResult.FromMemory(original, ColorComponents.Grey).Data;
        Assert.True(NeighborVariation(decoded.Data, width, height) < 0.45 * NeighborVariation(reference, width, height));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringSmoothedRefinementLeavesPartialOutputAndStreamOpen(bool optimize)
    {
        const int width = 65, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            SmoothingFactor = 100, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, progressive: true,
            expectedInterval: 2, expectedRefinementBits: 2);
        Scan refinement = document.Scans.First(scan => scan.High != 0 && scan.First != 0);
        int cancelAfter = refinement.DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, pixels, width, height, options: options,
                cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..cancelAfter], destination.ToArray()[..cancelAfter]);
    }

    // Independent oracle: apply the mathematical kernel to edge-extended input
    // at every padded block position, then use the direct orthonormal JPEG DCT.
    // It calls no encoder color, filtering, DCT or quantization helpers.
    private static double[] OracleSamples(byte[] pixels, int width, int height, int left, int top, int factor)
    {
        var samples = new double[64];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                double center = pixels[Math.Clamp(top + y, 0, height - 1) * width + Math.Clamp(left + x, 0, width - 1)];
                double neighbors = 0;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (dx != 0 || dy != 0)
                            neighbors += pixels[Math.Clamp(top + y + dy, 0, height - 1) * width
                                + Math.Clamp(left + x + dx, 0, width - 1)];
                double weight = factor / 1024.0;
                samples[y * 8 + x] = center * (1 - 8 * weight) + neighbors * weight - 128;
            }
        return samples;
    }

    private static double OracleDct(double[] samples, int u, int v)
    {
        double sum = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                sum += samples[y * 8 + x] * Math.Cos((2 * x + 1) * u * Math.PI / 16)
                    * Math.Cos((2 * y + 1) * v * Math.PI / 16);
        return sum * 0.25 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1);
    }

    private static double NeighborVariation(byte[] pixels, int width, int height)
    {
        double sum = 0;
        for (int y = 2; y < height - 2; y++)
            for (int x = 2; x < width - 3; x++)
                sum += Math.Abs(pixels[y * width + x + 1] - pixels[y * width + x]);
        return sum;
    }

    private static ReadOnlySpan<int> Zigzag =>
    [
        0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
    ];
}
