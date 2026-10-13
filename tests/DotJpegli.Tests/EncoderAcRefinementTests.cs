using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderAcRefinementTests
{
    public static TheoryData<PixelLayout, bool, int> MixedModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, bool, int>();
            foreach (PixelLayout layout in new[] { PixelLayout.Gray8, PixelLayout.Rgb24 })
                foreach (bool optimize in new[] { false, true })
                    foreach (int mode in Enumerable.Range(0, 5))
                        modes.Add(layout, optimize, mode);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(MixedModes))]
    public void MixedBandDepthsPreserveEveryCoefficientAndPixelThroughSparseRefinementRounds(
        PixelLayout layout, bool optimize, int mode)
    {
        // Odd edges need dummy interleaved DC blocks. Restart interval one
        // wraps RST7 even when only a subset of bands remains in a round.
        const int width = 145, height = 17;
        (int dcBits, int[] ends, int[] acBits) = mode switch
        {
            0 => (0, new[] { 2, 63 }, new[] { 0, 2 }),
            1 => (10, new[] { 1, 5, 63 }, new[] { 0, 1, 5 }),
            2 => (1, new[] { 1, 5, 63 }, new[] { 10, 0, 2 }),
            3 => (10, new[] { 2, 63 }, new[] { 0, 0 }),
            _ => (2, new[] { 1, 5, 63 }, new[] { 10, 0, 1 })
        };
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, AdaptiveQuantization = mode % 2 == 1,
            OptimizeHuffmanTables = optimize, Subsampling = ChromaSubsampling.Quarter
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int position in ends.SelectMany(end => new[] { end, end + 1 }).Where(position => position <= 63))
            Assert.Contains(reference.Blocks.Values, block => block.Coefficients[position] != 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveRefinementBits = dcBits,
                ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = acBits,
                RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, interval, dcBits, ends, acBits);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            Assert.Equal(dcBits + 1, actual.Scans.Count(scan => scan.First == 0));
            if (!optimize)
                foreach (Scan scan in actual.Scans.Where(scan => scan.First != 0))
                    Assert.All(scan.EobRuns, run => Assert.Equal(1, run));
        }
    }

    [Fact]
    public void LevelTwoStyleBandsHaveTheExpectedSpectralAndBitOrder()
    {
        int[] ends = [2, 63], bits = [0, 2];
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = bits
        };
        JpegDocument document = JpegDocument.Read(Encode(Pattern(17, 9, PixelLayout.Rgb24),
            17, 9, PixelLayout.Rgb24, options), true, 0, 0, ends, bits);
        Assert.Equal(new[]
        {
            "1,2,3:0-0:0/0",
            "1:1-2:0/0", "2:1-2:0/0", "3:1-2:0/0",
            "1:3-63:0/2", "2:3-63:0/2", "3:3-63:0/2",
            "1:3-63:2/1", "2:3-63:2/1", "3:3-63:2/1",
            "1:3-63:1/0", "2:3-63:1/0", "3:3-63:1/0"
        }, document.Scans.Select(scan =>
            $"{string.Join(',', scan.Selectors.Select(selector => selector.Component))}:{scan.First}-{scan.Last}:{scan.High}/{scan.Low}"));
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false)]
    [InlineData(PixelLayout.Gray8, true)]
    [InlineData(PixelLayout.Rgb24, false)]
    [InlineData(PixelLayout.Rgb24, true)]
    public void SingleCoefficientBandsAndOddTinyImagesHandleEveryIndependentDepth(PixelLayout layout, bool optimize)
    {
        int[] ends = Enumerable.Range(1, 63).ToArray();
        int[] bits = Enumerable.Range(1, 63).Select(position => position % 11).ToArray();
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 17) })
        {
            byte[] pixels = Pattern(width, height, layout);
            var options = new JpegliEncoderOptions
            {
                Quality = 100, AdaptiveQuantization = false, OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] baseline = Encode(pixels, width, height, layout, options);
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, ProgressiveRefinementBits = 1,
                ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = bits
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, 2, 1, ends, bits);
            AssertBaselineParity(baseline, JpegDocument.Read(baseline, false, 2), jpeg, actual, layout);
            Assert.All(actual.Scans.Where(scan => scan.First != 0), scan => Assert.Equal(scan.First, scan.Last));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlatBandsResetEobAndRestartStateWhenBandsLeaveTheSchedule(bool optimize)
    {
        const int width = 145, height = 17;
        int[] ends = [1, 5, 63], bits = [0, 3, 1];
        byte[] pixels = FlatPixels(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { AdaptiveQuantization = false, OptimizeHuffmanTables = optimize };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with
            {
                Progressive = true, ProgressiveRefinementBits = 2, ProgressiveAcScanEnds = ends,
                ProgressiveAcRefinementBits = bits, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, true, interval, 2, ends, bits);
            AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
            foreach (Scan scan in actual.Scans.Where(scan => scan.First != 0))
            {
                int component = Assert.Single(scan.Selectors).Component;
                int count = reference.Blocks.Count(pair => pair.Key.Component == component && !pair.Value.IsDummy);
                int limit = !optimize ? 1 : interval == 0 ? count : interval;
                Assert.Equal(Enumerable.Range(0, (count - 1) / limit + 1)
                    .Select(index => Math.Min(limit, count - index * limit)), scan.EobRuns);
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
    public void TypedPaddedInputsAndCustomSamplingPreserveCoefficientsAndPixels(int kind, JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17, channels = 4, stride = width * channels + 13;
        int[] ends = [1, 5, 63], bits = [10, 0, 2];
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
                Progressive = true, ProgressiveRefinementBits = 1, ProgressiveAcScanEnds = ends,
                ProgressiveAcRefinementBits = bits, OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = EncodeSource(compact, width, height, PixelLayout.Bgra32, progressive);
            Assert.Equal(jpeg, EncodeSource(padded, width, height, PixelLayout.Bgra32, progressive, stride));
            AssertBaselineParity(baseline, reference, jpeg,
                JpegDocument.Read(jpeg, true, 2, 1, ends, bits), PixelLayout.Rgb24);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullAndRepeatedDepthsPreserveExistingJpegBytes(bool optimize)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, RestartInterval = 2 };
        Assert.Null(options.ProgressiveAcRefinementBits);
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with { ProgressiveAcRefinementBits = null }));
        foreach (int dcBits in new[] { 0, 2, 10 })
            foreach (int[]? ends in new int[]?[] { null, [2, 63] })
            {
                var progressive = options with
                {
                    Progressive = true, ProgressiveRefinementBits = dcBits, ProgressiveAcScanEnds = ends
                };
                byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, progressive);
                Assert.Equal(expected, Encode(pixels, width, height, PixelLayout.Rgb24,
                    progressive with { ProgressiveAcRefinementBits = null }));
                Assert.Equal(expected, Encode(pixels, width, height, PixelLayout.Rgb24, progressive with
                {
                    ProgressiveAcRefinementBits = Enumerable.Repeat(dcBits, ends?.Length ?? 1).ToArray()
                }));
            }
    }

    [Fact]
    public void InvalidDepthListsAreRejectedBeforeOutputForEveryInputType()
    {
        foreach (int[] bits in new int[][] { [], [0, 0], [-1], [11], [int.MinValue], [int.MaxValue] })
            AssertInvalid(new JpegliEncoderOptions { Progressive = true, ProgressiveAcRefinementBits = bits });
        foreach (int[] bits in new int[][] { [], [0], [0, 1, 2], [-1, 0], [0, 11] })
            AssertInvalid(new JpegliEncoderOptions
            {
                Progressive = true, ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = bits
            });
        AssertInvalid(new JpegliEncoderOptions { ProgressiveAcRefinementBits = new[] { 0 } });
        AssertInvalid(new JpegliEncoderOptions { ProgressiveAcRefinementBits = Array.Empty<int>() });
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void BothListsAreSnapshottedBeforeTheFirstWriteToANonseekableStream(int kind, bool optimize)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width * height * 3);
        var ends = new List<int> { 2, 63 };
        var bits = new List<int> { 0, 2 };
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 1, ProgressiveAcScanEnds = ends,
            ProgressiveAcRefinementBits = bits, RestartInterval = 2, OptimizeHuffmanTables = optimize
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
            bits.Clear();
            bits.Add(10);
        });
        EncodeSource(output, pixels, width, height, PixelLayout.Rgb24, options, TestContext.Current.CancellationToken);
        Assert.True(changed);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        JpegDocument.Read(destination.ToArray(), true, 2, 1, new[] { 2, 63 }, new[] { 0, 2 });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringFinalSparseAcRefinementLeavesPartialOutputAndStreamOpen(bool optimize)
    {
        const int width = 65, height = 33;
        int[] ends = [1, 5, 63], bits = [2, 0, 1];
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = bits,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, true, 2, 0, ends, bits);
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
    public void PreCancellationWithValidMixedDepthsWritesNoOutputForEveryInputType()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = new[] { 0, 2 }
        };
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
            ArgumentException error = Assert.ThrowsAny<ArgumentException>(() =>
                EncodeSource(output, Source(kind, 3), 1, 1, PixelLayout.Rgb24, options, TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveAcRefinementBits), error.ParamName);
            Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
            Assert.False(output.Closed);
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
            Array.Fill(floats, float.NaN);
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
