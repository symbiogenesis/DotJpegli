using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderRefinementTests
{
    public static TheoryData<PixelLayout, ChromaSubsampling, bool, bool, int> EncodingModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, ChromaSubsampling, bool, bool, int>();
            foreach (bool optimize in new[] { false, true })
                foreach (bool adaptive in new[] { false, true })
                    foreach (int bits in new[] { 1, 2 })
                    {
                        modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, optimize, adaptive, bits);
                        foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                            modes.Add(PixelLayout.Rgb24, sampling, optimize, adaptive, bits);
                    }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void SuccessiveApproximationPreservesEveryRealCoefficientAndDecodedPixel(
        PixelLayout layout, ChromaSubsampling sampling, bool optimize, bool adaptive, int bits)
    {
        // Odd edges need dummy interleaved DC blocks, but no dummy AC blocks.
        // Even 4:2:0 has 20 DC MCUs, enough to wrap the restart sequence.
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling,
            OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        foreach (int interval in new[] { 0, 1, 7, 65535 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveRefinementBits = bits, RestartInterval = interval
            });
            // The independent reader validates every SOS's component order,
            // Ss/Se/Ah/Al, MCU counts, restart cycling/reset and one-bit padding.
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: interval, expectedRefinementBits: bits);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            foreach (Scan scan in actual.Scans.Where(scan => scan.High != 0 && scan.First != 0))
                Assert.All(scan.EobRuns, run => Assert.Equal(1, run));
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false)]
    [InlineData(PixelLayout.Gray8, true)]
    [InlineData(PixelLayout.Rgb24, false)]
    [InlineData(PixelLayout.Rgb24, true)]
    public void MaximumRefinementDepthHandlesTinyNarrowAndOddImages(PixelLayout layout, bool optimize)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 17) })
            foreach (int quality in new[] { 0, 100 })
            {
                byte[] pixels = Pattern(width, height, layout);
                var options = new JpegliEncoderOptions
                {
                    Quality = quality, Subsampling = ChromaSubsampling.Quarter,
                    OptimizeHuffmanTables = optimize, AdaptiveQuantization = false, RestartInterval = 2
                };
                byte[] baseline = Encode(pixels, width, height, layout, options);
                JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
                byte[] jpeg = Encode(pixels, width, height, layout, options with
                {
                    Progressive = true, ProgressiveRefinementBits = 10
                });
                JpegDocument actual = JpegDocument.Read(jpeg, progressive: true,
                    expectedInterval: 2, expectedRefinementBits: 10);
                AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeOddDcAndBothAcSignsSurviveDifferentPointTransforms(bool optimize)
    {
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        // The texture's whole-block sums happen to produce even DC values.
        // These two blocks have level-shifted sums of -72 and +72, giving
        // quantized DC values of -9 and +9 with quality 100's unit quantizer.
        for (int y = 0; y < 8; y++)
        {
            pixels.AsSpan(y * width, 8).Fill(127);
            pixels.AsSpan(y * width + 8, 8).Fill(129);
        }
        pixels[0] = 119;
        pixels[8] = 137;
        var options = new JpegliEncoderOptions
        {
            Quality = 100, AdaptiveQuantization = false, OptimizeHuffmanTables = optimize
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Gray8, options);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        Assert.Equal(-9, reference.Blocks[new(1, 0, 0)].Coefficients[0]);
        Assert.Equal(9, reference.Blocks[new(1, 1, 0)].Coefficients[0]);
        int[] dc = reference.Blocks.Values.Select(block => block.Coefficients[0]).ToArray();
        int[] ac = reference.Blocks.Values.SelectMany(block => block.Coefficients[1..]).ToArray();
        Assert.Contains(dc, value => value < 0 && (value & 1) != 0);
        Assert.Contains(dc, value => value > 0 && (value & 1) != 0);
        Assert.Contains(ac, value => value < -2 && (value & 1) != 0);
        Assert.Contains(ac, value => value > 2 && (value & 1) != 0);
        Assert.Contains(ac, value => value == -1);
        Assert.Contains(ac, value => value == 1);
        foreach (int bits in new[] { 1, 2, 10 })
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, options with
            {
                Progressive = true, ProgressiveRefinementBits = bits, RestartInterval = 1
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: 1, expectedRefinementBits: bits);
            AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Gray8);
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false)]
    [InlineData(PixelLayout.Gray8, true)]
    [InlineData(PixelLayout.Rgb24, false)]
    [InlineData(PixelLayout.Rgb24, true)]
    public void FlatRefinementScansUseOneEobPerRealBlock(PixelLayout layout, bool optimize)
    {
        const int width = 145, height = 17;
        byte[] pixels = FlatPixels(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Subsampling = ChromaSubsampling.Quarter,
            AdaptiveQuantization = false, OptimizeHuffmanTables = optimize
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true,
                expectedInterval: interval, expectedRefinementBits: 2);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            foreach (Scan scan in actual.Scans)
            {
                if (scan.First == 0)
                {
                    Assert.Empty(scan.EobRuns);
                    continue;
                }
                int component = Assert.Single(scan.Selectors).Component;
                int realBlocks = reference.Blocks.Count(pair =>
                    pair.Key.Component == component && !pair.Value.IsDummy);
                int runLimit = scan.High != 0 || !optimize ? 1 : interval == 0 ? realBlocks : interval;
                int[] expectedRuns = Enumerable.Range(0, (realBlocks - 1) / runLimit + 1)
                    .Select(index => Math.Min(runLimit, realBlocks - index * runLimit)).ToArray();
                Assert.Equal(expectedRuns, scan.EobRuns);
            }
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, 1)]
    [InlineData(PixelLayout.Rgb24, 3)]
    [InlineData(PixelLayout.Bgr24, 3)]
    [InlineData(PixelLayout.Rgba32, 4)]
    [InlineData(PixelLayout.Bgra32, 4)]
    public void RefinementPixelLayoutsAndPaddedStrideProduceSameJpeg(PixelLayout layout, int bytesPerPixel)
    {
        const int width = 17, height = 9;
        PixelLayout referenceLayout = layout == PixelLayout.Gray8 ? layout : PixelLayout.Rgb24;
        byte[] pixels = Pattern(width, height, referenceLayout);
        int channels = referenceLayout == PixelLayout.Gray8 ? 1 : 3;
        int stride = width * bytesPerPixel + 13;
        byte[] source = new byte[(height - 1) * stride + width * bytesPerPixel];
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * channels;
                int output = y * stride + x * bytesPerPixel;
                for (int channel = 0; channel < channels; channel++)
                    source[output + (bgr ? 2 - channel : channel)] = pixels[input + channel];
                if (bytesPerPixel == 4)
                    source[output + 3] = (byte)(x * 17);
            }
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = Encode(pixels, width, height, referenceLayout, options);
        Assert.Equal(expected, JpegliEncoder.Encode(source, width, height, layout, options, stride,
            cancellationToken: TestContext.Current.CancellationToken));
        using var destination = new MemoryStream();
        using var outputStream = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(outputStream, source, width, height, layout, options, stride,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(outputStream.Closed);
        Assert.Equal(expected, destination.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringFinalRefinementScanLeavesStreamOpenWithPartialOutput(bool optimize)
    {
        const int width = 65, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, progressive: true,
            expectedInterval: 2, expectedRefinementBits: 2);
        int cancelAfter = document.Scans[^1].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        OperationCanceledException exception = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, pixels, width, height, options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..cancelAfter], destination.ToArray()[..cancelAfter]);
    }
}
