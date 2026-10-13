using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderSpectralSelectionTests
{
    public static TheoryData<PixelLayout, bool, int, int> SpectralModes => new()
    {
        { PixelLayout.Gray8, false, 0, 0 }, { PixelLayout.Gray8, true, 0, 1 },
        { PixelLayout.Gray8, false, 2, 1 }, { PixelLayout.Gray8, true, 2, 0 },
        { PixelLayout.Gray8, false, 10, 0 }, { PixelLayout.Gray8, true, 10, 1 },
        { PixelLayout.Rgb24, false, 0, 1 }, { PixelLayout.Rgb24, true, 0, 0 },
        { PixelLayout.Rgb24, false, 2, 0 }, { PixelLayout.Rgb24, true, 2, 1 },
        { PixelLayout.Rgb24, false, 10, 1 }, { PixelLayout.Rgb24, true, 10, 0 }
    };

    [Theory]
    [MemberData(nameof(SpectralModes))]
    public void BandsAndRefinementRoundsPreserveAllCoefficientsAndPixels(
        PixelLayout layout, bool optimize, int bits, int bandMode)
    {
        // Odd 4:2:0 edges create dummy DC blocks; interval 1 also wraps RST7.
        const int width = 145, height = 17;
        int[] ends = bandMode == 0 ? [2, 63] : [1, 5, 17, 63];
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, AdaptiveQuantization = false,
            OptimizeHuffmanTables = optimize, Subsampling = ChromaSubsampling.Quarter
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        // Both signs and nonzero coefficients around every selected boundary
        // make truncating a band or assigning correction bits to its neighbor visible.
        int[] ac = reference.Blocks.Values.SelectMany(block => block.Coefficients[1..]).ToArray();
        Assert.Contains(ac, value => value < 0);
        Assert.Contains(ac, value => value > 0);
        foreach (int position in ends.SelectMany(end => new[] { end, end + 1 }).Where(position => position <= 63))
            Assert.Contains(reference.Blocks.Values, block => block.Coefficients[position] != 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveAcScanEnds = ends,
                ProgressiveRefinementBits = bits, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, interval, bits, ends);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            if (!optimize)
                foreach (Scan scan in actual.Scans.Where(scan => scan.First != 0))
                    Assert.All(scan.EobRuns, run => Assert.Equal(1, run));
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false)]
    [InlineData(PixelLayout.Gray8, true)]
    [InlineData(PixelLayout.Rgb24, false)]
    [InlineData(PixelLayout.Rgb24, true)]
    public void EveryAcPositionCanBeItsOwnBandAtInitialAndMaximumRefinementDepth(
        PixelLayout layout, bool optimize)
    {
        const int width = 17, height = 9;
        int[] ends = Enumerable.Range(1, 63).ToArray();
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, AdaptiveQuantization = false,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 2);
        foreach (int bits in new[] { 0, 10 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveRefinementBits = bits, ProgressiveAcScanEnds = ends
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, 2, bits, ends);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            Assert.All(actual.Scans.Where(scan => scan.First != 0), scan => Assert.Equal(scan.First, scan.Last));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlatBandsResetEobRunsAndRestartSequencesForEveryComponentAndRound(bool optimize)
    {
        const int width = 145, height = 17;
        int[] ends = [1, 5, 17, 63];
        byte[] pixels = FlatPixels(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { AdaptiveQuantization = false, OptimizeHuffmanTables = optimize };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
            {
                Progressive = true, ProgressiveRefinementBits = 2,
                ProgressiveAcScanEnds = ends, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, interval, 2, ends);
            AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
            foreach (Scan scan in actual.Scans.Where(scan => scan.First != 0))
            {
                int component = Assert.Single(scan.Selectors).Component;
                int count = reference.Blocks.Count(pair => pair.Key.Component == component && !pair.Value.IsDummy);
                int limit = !optimize ? 1 : interval == 0 ? count : interval;
                int[] expected = Enumerable.Range(0, (count - 1) / limit + 1)
                    .Select(index => Math.Min(limit, count - index * limit)).ToArray();
                Assert.Equal(expected, scan.EobRuns);
            }
        }
    }

    public static TheoryData<int, JpegColorSpace> TypedColorModes
    {
        get
        {
            var modes = new TheoryData<int, JpegColorSpace>();
            foreach (int kind in new[] { 0, 1, 2 })
                foreach (JpegColorSpace colorSpace in new[]
                    { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb, JpegColorSpace.Xyb })
                    modes.Add(kind, colorSpace);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(TypedColorModes))]
    public void TypedPaddedInputsAndCustomSamplingPreserveAllColorSpaceCoefficients(
        int kind, JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17, channels = 4, stride = width * channels + 13;
        int[] ends = [1, 5, 17, 63];
        Array compact = Source(kind, width * height * channels);
        Array padded = Pad(compact, width * channels, height, stride);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, Quality = 90, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            IccProfile = colorSpace == JpegColorSpace.Xyb ? new byte[] { 1, 2, 3 } : default,
            RestartInterval = 2
        };
        byte[] baseline = EncodeSource(compact, width, height, PixelLayout.Bgra32, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 2);
        foreach (bool optimize in new[] { false, true })
        {
            var progressive = options with
            {
                Progressive = true, ProgressiveRefinementBits = 2,
                ProgressiveAcScanEnds = ends, OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = EncodeSource(compact, width, height, PixelLayout.Bgra32, progressive);
            Assert.Equal(jpeg, EncodeSource(padded, width, height, PixelLayout.Bgra32, progressive, stride));
            JpegDocument actual = JpegDocument.Read(jpeg, true, 2, 2, ends);
            AssertCoefficients(reference, actual);
            // Stb decodes stored RGB/XYB channels without applying the ICC profile.
            Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
                ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
        }
    }

    [Theory]
    [InlineData(0, PixelLayout.YCbCr24)]
    [InlineData(1, PixelLayout.YCbCr24)]
    [InlineData(2, PixelLayout.YCbCr24)]
    [InlineData(0, PixelLayout.Xyb24)]
    [InlineData(1, PixelLayout.Xyb24)]
    [InlineData(2, PixelLayout.Xyb24)]
    public void DirectColorInputsUseTheSameBandsAndFinalCoefficients(int kind, PixelLayout layout)
    {
        const int width = 19, height = 17;
        int[] ends = [2, 63];
        Array pixels = Source(kind, width * height * 3);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, IccProfile = layout == PixelLayout.Xyb24 ? new byte[] { 1, 2, 3 } : default,
            RestartInterval = 1
        };
        byte[] baseline = EncodeSource(pixels, width, height, layout, options);
        byte[] jpeg = EncodeSource(pixels, width, height, layout, options with
        {
            Progressive = true, ProgressiveRefinementBits = 2, ProgressiveAcScanEnds = ends
        });
        AssertCoefficients(JpegDocument.Read(baseline, false, 1), JpegDocument.Read(jpeg, true, 1, 2, ends));
        Assert.Equal(ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue).Data);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullAndSingleFullBandPreserveDefaultJpegBytes(bool optimize)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, RestartInterval = 2 };
        Assert.Null(options.ProgressiveAcScanEnds);
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with { ProgressiveAcScanEnds = null }));
        foreach (int bits in new[] { 0, 2, 10 })
        {
            var progressive = options with { Progressive = true, ProgressiveRefinementBits = bits };
            byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, progressive);
            Assert.Equal(expected, Encode(pixels, width, height, PixelLayout.Rgb24,
                progressive with { ProgressiveAcScanEnds = null }));
            Assert.Equal(expected, Encode(pixels, width, height, PixelLayout.Rgb24,
                progressive with { ProgressiveAcScanEnds = new[] { 63 } }));
        }
    }

    [Fact]
    public void InvalidBandListsAreRejectedBeforeOutputForEveryInputType()
    {
        int[][] invalid =
        [
            [], [0, 63], [-1, 63], [64], [1, 64], [1, 62], [63, 63],
            [5, 2, 63], [2, 2, 63], Enumerable.Range(1, 63).Append(63).ToArray()
        ];
        foreach (int[] ends in invalid)
            AssertInvalid(new JpegliEncoderOptions { Progressive = true, ProgressiveAcScanEnds = ends });
        AssertInvalid(new JpegliEncoderOptions { ProgressiveAcScanEnds = new[] { 63 } });
        AssertInvalid(new JpegliEncoderOptions { ProgressiveAcScanEnds = Array.Empty<int>() });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void BandListIsSnapshottedBeforeWritingToANonseekableStream(int kind)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width * height * 3);
        var ends = new List<int> { 2, 63 };
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, ProgressiveAcScanEnds = ends, RestartInterval = 2
        };
        byte[] expected = EncodeSource(pixels, width, height, PixelLayout.Rgb24, options);
        using var destination = new MemoryStream();
        bool changed = false;
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (changed)
                return;
            changed = true;
            ends.Clear();
            ends.Add(63);
        });
        EncodeSource(output, pixels, width, height, PixelLayout.Rgb24, options, TestContext.Current.CancellationToken);
        Assert.True(changed);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        JpegDocument.Read(destination.ToArray(), true, 2, 2, new[] { 2, 63 });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringTheLastBandOfTheLastRoundLeavesPartialOutputAndStreamOpen(bool optimize)
    {
        const int width = 65, height = 33;
        int[] ends = [1, 5, 17, 63];
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2, ProgressiveAcScanEnds = ends,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, true, 2, 2, ends);
        int cancelAfter = document.Scans[^1].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, pixels, width, height, options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..cancelAfter], destination.ToArray()[..cancelAfter]);
    }

    [Fact]
    public void PreCancellationWithValidBandsWritesNoOutputForEveryInputType()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions { Progressive = true, ProgressiveAcScanEnds = new[] { 2, 63 } };
        foreach (int kind in new[] { 0, 1, 2 })
        {
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            var error = Assert.Throws<OperationCanceledException>(() =>
                EncodeSource(output, Source(kind, 3), 1, 1, PixelLayout.Rgb24, options, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken);
            Assert.Empty(destination.ToArray());
            Assert.False(output.Closed);
        }
    }

    private static void AssertInvalid(JpegliEncoderOptions options)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        {
            using var destination = new MemoryStream();
            destination.Write([0x12, 0x34]);
            using var output = new WriteOnlyStream(destination);
            ArgumentException error = Assert.Throws<ArgumentException>(() =>
                EncodeSource(output, Source(kind, 3), 1, 1, PixelLayout.Rgb24, options, TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveAcScanEnds), error.ParamName);
            Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
            Assert.False(output.Closed);
        }
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
            if (block.IsDummy)
                Assert.All(other.Coefficients[1..], value => Assert.Equal(0, value));
            else
                Assert.Equal(block.Coefficients, other.Coefficients);
        }
    }

    private static Array Source(int kind, int elements) => kind switch
    {
        0 => Enumerable.Range(0, elements).Select(index => (byte)(24 + (index * 37 + index / 7 * 19) % 208)).ToArray(),
        1 => Enumerable.Range(0, elements).Select(index => (ushort)((24 + (index * 37 + index / 7 * 19) % 208) * 257 + index % 7)).ToArray(),
        _ => Enumerable.Range(0, elements).Select(index => (24 + (index * 37 + index / 7 * 19) % 208 + index % 7 / 8f) / 255f).ToArray()
    };

    private static Array Pad(Array source, int rowElements, int height, int stride)
    {
        Array padded = Array.CreateInstance(source.GetType().GetElementType()!, (height - 1) * stride + rowElements);
        if (padded is float[] floats)
            Array.Fill(floats, float.NaN); // Padding is deliberately invalid as consumed input.
        for (int row = 0; row < height; row++)
            Array.Copy(source, row * rowElements, padded, row * stride, rowElements);
        return padded;
    }

    private static byte[] EncodeSource(Array source, int width, int height, PixelLayout layout,
        JpegliEncoderOptions options, int stride = 0) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, layout, options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, (PixelComponentLayout)layout, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, (PixelComponentLayout)layout, options, stride, TestContext.Current.CancellationToken)
    };

    private static void EncodeSource(Stream output, Array source, int width, int height, PixelLayout layout,
        JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, layout, options, cancellationToken: cancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, (PixelComponentLayout)layout, options, cancellationToken: cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, (PixelComponentLayout)layout, options, cancellationToken: cancellationToken);
    }
}
