using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderNoninterleavedDcTests
{
    public static TheoryData<ChromaSubsampling, bool> PresetModes
    {
        get
        {
            var modes = new TheoryData<ChromaSubsampling, bool>();
            foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                foreach (bool optimize in new[] { false, true })
                    modes.Add(sampling, optimize);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(PresetModes))]
    public void ComponentDcScansPreserveAllRealCoefficientsAndPixelsWithOddEdgesAndRestarts(
        ChromaSubsampling sampling, bool optimize)
    {
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, Subsampling = sampling, OptimizeHuffmanTables = optimize,
            AdaptiveQuantization = optimize
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int interval in new[] { 0, 1, 7, 65535 })
        {
            var progressive = options with
            {
                Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 2,
                ProgressiveAcScanEnds = new[] { 1, 5, 63 }, ProgressiveAcRefinementBits = new[] { 0, 3, 1 },
                RestartInterval = interval
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, progressive);
            JpegDocument actual = Read(jpeg, progressive);
            AssertRealBlockParity(baseline, reference, jpeg, actual, PixelLayout.Rgb24);
            AssertComponentCodingUnits(actual, interval);
            Assert.Equal(9, actual.Scans.Count(scan => scan.First == 0));
            if (interval == 1)
                Assert.Contains(actual.Scans.Where(scan => scan.First == 0), scan => scan.RestartMarkers.Count > 8);
            if (!optimize)
                foreach (Scan scan in actual.Scans.Where(scan => scan.First != 0))
                    Assert.All(scan.EobRuns, run => Assert.Equal(1, run));
        }
    }

    public static TheoryData<int, int, int, int, int, int, bool> CustomModes => new()
    {
        { 1, 1, 2, 1, 1, 2, false }, { 1, 1, 2, 1, 1, 2, true },
        { 1, 2, 2, 1, 1, 1, false }, { 1, 2, 2, 1, 1, 1, true },
        { 1, 1, 3, 2, 1, 1, false }, { 1, 1, 3, 2, 1, 1, true },
        { 1, 1, 1, 1, 2, 3, false }, { 1, 1, 1, 1, 2, 3, true },
        { 1, 1, 4, 1, 1, 4, false }, { 1, 1, 4, 1, 1, 4, true },
        { 2, 1, 4, 1, 2, 2, false }, { 2, 1, 4, 1, 2, 2, true },
        { 1, 2, 2, 2, 1, 4, false }, { 1, 2, 2, 2, 1, 4, true }
    };

    [Theory]
    [MemberData(nameof(CustomModes))]
    public void SubsampledLumaAndIndependentComponentMaximaUseRealBlockRasterGeometry(
        int yh, int yv, int cbh, int cbv, int crh, int crv, bool optimize)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 25), (145, 49) })
        {
            byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SamplingFactors = new(new(yh, yv), new(cbh, cbv), new(crh, crv)),
                SmoothingFactor = optimize ? 73 : 0, OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            var progressive = options with
            {
                Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 10,
                ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = new[] { 0, 2 }
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, progressive);
            JpegDocument actual = Read(jpeg, progressive);
            AssertRealBlockParity(baseline, JpegDocument.Read(baseline, false, 2), jpeg, actual, PixelLayout.Rgb24);
            AssertComponentCodingUnits(actual, 2);
            Assert.Equal(33, actual.Scans.Count(scan => scan.First == 0));
        }
    }

    public static TheoryData<int, JpegColorSpace> TypedModes
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
    [MemberData(nameof(TypedModes))]
    public void PaddedByteWordAndFractionalFloatInputsPreserveEachOutputColorSpace(int kind, JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17, channels = 4, stride = width * channels + 13;
        Array source = Source(kind, width * height * channels);
        Array padded = Pad(source, width * channels, height, stride);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, Quality = 90, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            IccProfile = colorSpace == JpegColorSpace.Xyb ? new byte[] { 1, 2, 3 } : default,
            RestartInterval = 2
        };
        byte[] baseline = EncodeSource(source, width, height, PixelLayout.Bgra32, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 2);
        foreach (bool optimize in new[] { false, true })
        {
            var progressive = options with
            {
                Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 1,
                ProgressiveAcScanEnds = new[] { 1, 5, 63 }, ProgressiveAcRefinementBits = new[] { 10, 0, 2 },
                OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = EncodeSource(source, width, height, PixelLayout.Bgra32, progressive);
            Assert.Equal(jpeg, EncodeSource(padded, width, height, PixelLayout.Bgra32, progressive, stride));
            JpegDocument actual = Read(jpeg, progressive);
            AssertRealBlockParity(baseline, reference, jpeg, actual,
                colorSpace == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
            AssertComponentCodingUnits(actual, 2);
            if (colorSpace == JpegColorSpace.Grayscale)
                Assert.Equal(jpeg, EncodeSource(source, width, height, PixelLayout.Bgra32,
                    progressive with { ProgressiveInterleaveDc = true }));
        }
    }

    [Theory]
    [InlineData(0, PixelLayout.YCbCr24)]
    [InlineData(1, PixelLayout.YCbCr24)]
    [InlineData(2, PixelLayout.YCbCr24)]
    [InlineData(0, PixelLayout.Xyb24)]
    [InlineData(1, PixelLayout.Xyb24)]
    [InlineData(2, PixelLayout.Xyb24)]
    public void DirectYCbCrAndScaledXybInputsPreserveCoefficients(int kind, PixelLayout layout)
    {
        const int width = 33, height = 19;
        Array source = Source(kind, width * height * 3);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, IccProfile = layout == PixelLayout.Xyb24 ? new byte[] { 1, 2, 3 } : default,
            RestartInterval = 1, AdaptiveQuantization = false
        };
        byte[] baseline = EncodeSource(source, width, height, layout, options);
        var progressive = options with
        {
            Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 2
        };
        byte[] jpeg = EncodeSource(source, width, height, layout, progressive);
        JpegDocument actual = Read(jpeg, progressive);
        AssertRealBlockParity(baseline, JpegDocument.Read(baseline, false, 1), jpeg, actual, PixelLayout.Rgb24);
        AssertComponentCodingUnits(actual, 1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultAndExplicitInterleavingPreserveBytesAndGrayscaleIgnoresTheSwitch(bool optimize)
    {
        Assert.True(new JpegliEncoderOptions().ProgressiveInterleaveDc);
        foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Gray8 })
            foreach (int dcBits in new[] { 0, 2, 10 })
                foreach (int interval in new[] { 0, 1, 7 })
                {
                    byte[] pixels = Pattern(19, 17, layout);
                    var options = new JpegliEncoderOptions
                    {
                        OptimizeHuffmanTables = optimize, RestartInterval = interval
                    };
                    Assert.Equal(Encode(pixels, 19, 17, layout, options),
                        Encode(pixels, 19, 17, layout, options with { ProgressiveInterleaveDc = true }));
                    options = options with
                    {
                        Progressive = true, ProgressiveRefinementBits = dcBits,
                        ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = new[] { 0, 2 }
                    };
                    byte[] expected = Encode(pixels, 19, 17, layout, options);
                    Assert.Equal(expected, Encode(pixels, 19, 17, layout,
                        options with { ProgressiveInterleaveDc = true }));
                    if (layout == PixelLayout.Gray8)
                        Assert.Equal(expected, Encode(pixels, 19, 17, layout,
                            options with { ProgressiveInterleaveDc = false }));
                }
    }

    [Fact]
    public void OnlyDcScansExpandAndAcBandOrderAndSparseDepthsStayUnchanged()
    {
        byte[] pixels = Pattern(19, 17, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveRefinementBits = 2,
            ProgressiveAcScanEnds = new[] { 1, 5, 63 }, ProgressiveAcRefinementBits = new[] { 0, 3, 1 }
        };
        JpegDocument interleaved = Read(Encode(pixels, 19, 17, PixelLayout.Rgb24, options), options);
        options = options with { ProgressiveInterleaveDc = false };
        JpegDocument actual = Read(Encode(pixels, 19, 17, PixelLayout.Rgb24, options), options);
        string[] expectedScans = interleaved.Scans.SelectMany(scan => scan.First == 0
            ? scan.Selectors.Select(selector => $"{selector.Component}:0-0:{scan.High}/{scan.Low}")
            : new[] { Signature(scan) }).ToArray();
        Assert.Equal(expectedScans, actual.Scans.Select(Signature));
        Assert.Equal(interleaved.Scans.Where(scan => scan.First != 0).Select(Signature),
            actual.Scans.Where(scan => scan.First != 0).Select(Signature));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullAndRepeatedAcDepthListsRemainEquivalentWithSeparateDcScans(bool optimize)
    {
        byte[] pixels = Pattern(17, 9, PixelLayout.Rgb24);
        foreach (int dcBits in new[] { 0, 2, 10 })
            foreach (int[]? ends in new int[]?[] { null, [2, 63] })
            {
                var options = new JpegliEncoderOptions
                {
                    Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = dcBits,
                    ProgressiveAcScanEnds = ends, OptimizeHuffmanTables = optimize, RestartInterval = 2
                };
                byte[] expected = Encode(pixels, 17, 9, PixelLayout.Rgb24, options);
                Assert.Equal(expected, Encode(pixels, 17, 9, PixelLayout.Rgb24,
                    options with { ProgressiveAcRefinementBits = null }));
                Assert.Equal(expected, Encode(pixels, 17, 9, PixelLayout.Rgb24, options with
                {
                    ProgressiveAcRefinementBits = Enumerable.Repeat(dcBits, ends?.Length ?? 1).ToArray()
                }));
                Read(expected, options);
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlatImagesKeepAcEobRunsSeparateFromComponentDcAndRestartBoundaries(bool optimize)
    {
        const int width = 145, height = 17;
        int[] ends = [1, 5, 63], bits = [0, 3, 1];
        byte[] pixels = FlatPixels(width, height, PixelLayout.Rgb24);
        var baselineOptions = new JpegliEncoderOptions { AdaptiveQuantization = false, OptimizeHuffmanTables = optimize };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, baselineOptions);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int interval in new[] { 0, 1, 7 })
        {
            var options = baselineOptions with
            {
                Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 2,
                ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = bits, RestartInterval = interval
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            JpegDocument document = Read(jpeg, options);
            AssertRealBlockParity(baseline, reference, jpeg, document, PixelLayout.Rgb24);
            foreach (Scan scan in document.Scans.Where(scan => scan.First == 0))
                Assert.Empty(scan.EobRuns);
            foreach (Scan scan in document.Scans.Where(scan => scan.First != 0))
            {
                int limit = !optimize ? 1 : interval == 0 ? scan.CodingUnits : interval;
                Assert.Equal(Enumerable.Range(0, (scan.CodingUnits - 1) / limit + 1)
                    .Select(index => Math.Min(limit, scan.CodingUnits - index * limit)), scan.EobRuns);
            }
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void ListsAreSnapshottedBeforeFirstWriteAndNonseekableStreamsMatchArrays(int kind, bool optimize)
    {
        const int width = 19, height = 17, stride = width * 3 + 13;
        Array source = Source(kind, width * height * 3);
        Array padded = Pad(source, width * 3, height, stride);
        var ends = new List<int> { 2, 63 };
        var bits = new List<int> { 0, 2 };
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 1,
            ProgressiveAcScanEnds = ends, ProgressiveAcRefinementBits = bits,
            RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] expected = EncodeSource(source, width, height, PixelLayout.Rgb24, options);
        Assert.Equal(expected, EncodeSource(source, width, height, PixelLayout.Rgb24,
            options with { ProgressiveAcRefinementBits = new[] { 0, 2 } }));
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
        EncodeSource(output, padded, width, height, PixelLayout.Rgb24, options,
            TestContext.Current.CancellationToken, stride);
        Assert.True(changed);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        Read(expected, options with { ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = new[] { 0, 2 } });
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void CancellationDuringLaterComponentInitialAndRefinementDcScansLeavesPartialOutput(int kind, bool optimize)
    {
        const int width = 65, height = 33;
        Array source = Source(kind, width * height * 3);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 2,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] complete = EncodeSource(source, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = Read(complete, options);
        foreach (int high in new[] { 0, 2, 1 })
        {
            Scan target = document.Scans.First(scan => scan.First == 0 && scan.High == high
                && Assert.Single(scan.Selectors).Component == 2);
            int cancelAfter = target.DataStart + 1;
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination, () =>
            {
                if (destination.Length >= cancelAfter)
                    cancellation.Cancel();
            });
            var error = Assert.Throws<OperationCanceledException>(() =>
                EncodeSource(output, source, width, height, PixelLayout.Rgb24, options, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken);
            Assert.False(output.Closed);
            Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
            Assert.Equal(complete[..cancelAfter], destination.ToArray()[..cancelAfter]);
        }
    }

    [Fact]
    public void DisablingDcInterleavingRequiresProgressiveBeforeOutputForEveryPrecisionAndColorMode()
    {
        foreach (int kind in new[] { 0, 1, 2 })
            foreach (PixelLayout layout in new[] { PixelLayout.Rgb24, PixelLayout.Gray8 })
            {
                var options = new JpegliEncoderOptions { ProgressiveInterleaveDc = false };
                Array source = Source(kind, layout == PixelLayout.Gray8 ? 1 : 3);
                var arrayError = Assert.Throws<ArgumentException>(() => EncodeSource(source, 1, 1, layout, options));
                Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveInterleaveDc), arrayError.ParamName);
                using var destination = new MemoryStream();
                destination.Write([0x12, 0x34]);
                using var output = new WriteOnlyStream(destination);
                var streamError = Assert.Throws<ArgumentException>(() => EncodeSource(output, source, 1, 1, layout,
                    options, TestContext.Current.CancellationToken));
                Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveInterleaveDc), streamError.ParamName);
                Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
                Assert.False(output.Closed);
            }
    }

    [Fact]
    public void ValidNoninterleavedOptionsHonorPreCancellationBeforeWriting()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveInterleaveDc = false,
            ProgressiveAcScanEnds = new[] { 2, 63 }, ProgressiveAcRefinementBits = new[] { 0, 2 }
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

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) => JpegDocument.Read(jpeg,
        options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits,
        options.ProgressiveAcScanEnds, options.ProgressiveAcRefinementBits, options.ProgressiveInterleaveDc);

    private static string Signature(Scan scan) =>
        $"{string.Join(',', scan.Selectors.Select(selector => selector.Component))}:{scan.First}-{scan.Last}:{scan.High}/{scan.Low}";

    private static void AssertComponentCodingUnits(JpegDocument document, int interval)
    {
        Assert.All(document.Blocks.Values, block => Assert.False(block.IsDummy));
        int maxH = document.Components.Values.Max(component => component.H);
        int maxV = document.Components.Values.Max(component => component.V);
        foreach (Scan scan in document.Scans)
        {
            Selector selector = Assert.Single(scan.Selectors);
            Component component = document.Components[selector.Component];
            int columns = (document.Width * component.H + 8 * maxH - 1) / (8 * maxH);
            int rows = (document.Height * component.V + 8 * maxV - 1) / (8 * maxV);
            Assert.Equal(columns * rows, scan.CodingUnits);
            Assert.Equal(columns * rows, document.Blocks.Count(pair => pair.Key.Component == selector.Component));
            int restartCount = interval == 0 ? 0 : (columns * rows - 1) / interval;
            Assert.Equal(Enumerable.Range(0, restartCount).Select(index => 0xd0 + index % 8), scan.RestartMarkers);
        }
    }

    private static void AssertRealBlockParity(byte[] baseline, JpegDocument reference,
        byte[] progressive, JpegDocument actual, PixelLayout layout)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        Assert.Equal(reference.Quantizers.Count, actual.Quantizers.Count);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count(pair => !pair.Value.IsDummy), actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(progressive, channels).Data);
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
        JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, layout, options, stride, cancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, (PixelComponentLayout)layout, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, (PixelComponentLayout)layout, options, stride, cancellationToken);
    }
}
