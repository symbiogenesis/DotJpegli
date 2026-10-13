using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderRestartRowsTests
{
    public static TheoryData<PixelLayout, ChromaSubsampling, bool> BaselineModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, ChromaSubsampling, bool>();
            foreach (bool optimize in new[] { false, true })
            {
                modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, optimize);
                foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                    modes.Add(PixelLayout.Rgb24, sampling, optimize);
            }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(BaselineModes))]
    public void BaselineRowsUseTheEmittedSamplingGeometryAndPreserveCoefficients(
        PixelLayout layout, ChromaSubsampling sampling, bool optimize)
    {
        const int width = 145, height = 81;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, Subsampling = sampling, OptimizeHuffmanTables = optimize,
            AdaptiveQuantization = optimize
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        Assert.Equal(baseline, Encode(pixels, width, height, layout, options with { RestartRows = 0 }));
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (int rows in new[] { 1, 2 })
        {
            var restarted = options with { RestartRows = rows, RestartInterval = 7 };
            byte[] jpeg = Encode(pixels, width, height, layout, restarted);
            JpegDocument actual = Read(jpeg, restarted);
            AssertRealParity(baseline, reference, jpeg, actual, layout);
            AssertScanIntervals(actual, rows);
        }
    }

    public static TheoryData<int, int, int, int, int, int, bool, bool> CustomModes
    {
        get
        {
            var modes = new TheoryData<int, int, int, int, int, int, bool, bool>();
            foreach ((int yh, int yv, int cbh, int cbv, int crh, int crv) in new[]
            {
                (2, 2, 1, 1, 1, 1), (1, 1, 2, 1, 1, 2), (1, 1, 3, 2, 1, 1),
                (1, 1, 1, 1, 2, 3), (1, 1, 4, 1, 1, 4), (2, 1, 4, 1, 2, 2), (4, 1, 2, 1, 1, 1)
            })
                foreach (bool interleave in new[] { false, true })
                    foreach (bool optimize in new[] { false, true })
                        modes.Add(yh, yv, cbh, cbv, crh, crv, interleave, optimize);
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(CustomModes))]
    public void ChangingComponentWidthsUpdateDriForInitialAndRefinementScans(
        int yh, int yv, int cbh, int cbv, int crh, int crv, bool interleave, bool optimize)
    {
        const int width = 145, height = 81;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Quality = 100, SamplingFactors = new(new(yh, yv), new(cbh, cbv), new(crh, crv)),
            OptimizeHuffmanTables = optimize, SmoothingFactor = optimize ? 37 : 0
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        var progressive = options with
        {
            Progressive = true, ProgressiveInterleaveDc = interleave, ProgressiveRefinementBits = 2,
            ProgressiveAcScanEnds = [1, 5, 63], ProgressiveAcRefinementBits = [0, 3, 1],
            RestartRows = 1, RestartInterval = 7
        };
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, progressive);
        JpegDocument actual = Read(jpeg, progressive);
        AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg, actual, PixelLayout.Rgb24);
        AssertScanIntervals(actual, 1);
        Assert.Contains(actual.Scans, scan => scan.High > 0 && scan.First == 0);
        Assert.Contains(actual.Scans, scan => scan.High > 0 && scan.First > 0);
        Assert.Contains(actual.Scans, scan => scan.RestartMarkers.Count > 8);
    }

    [Theory]
    [InlineData(0, ChromaSubsampling.None, false)]
    [InlineData(0, ChromaSubsampling.Quarter, true)]
    [InlineData(1, ChromaSubsampling.None, false)]
    [InlineData(1, ChromaSubsampling.Quarter, true)]
    [InlineData(2, ChromaSubsampling.None, true)]
    [InlineData(2, ChromaSubsampling.Quarter, false)]
    [InlineData(int.MaxValue, ChromaSubsampling.Quarter, true)]
    public void ProgressiveLevelPresetsUseTheResolvedScanWidths(int level, ChromaSubsampling sampling, bool optimize)
    {
        const int width = 65, height = 49;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            ProgressiveLevel = level, Subsampling = sampling, OptimizeHuffmanTables = optimize
        };
        byte[] referenceJpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument reference = Read(referenceJpeg, options);
        options = options with { RestartRows = 2, RestartInterval = 1 };
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument actual = Read(jpeg, options);
        AssertRealParity(referenceJpeg, reference, jpeg, actual, PixelLayout.Rgb24);
        AssertScanIntervals(actual, 2);
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false, false)]
    [InlineData(PixelLayout.Gray8, true, true)]
    [InlineData(PixelLayout.Rgb24, false, true)]
    [InlineData(PixelLayout.Rgb24, true, false)]
    public void ConstantScanWidthsAreByteEquivalentToFixedIntervalsAndZeroRowsPreserveBytes(
        PixelLayout layout, bool progressive, bool optimize)
    {
        const int width = 17, height = 81;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Subsampling = ChromaSubsampling.None, OptimizeHuffmanTables = optimize,
            Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
            RestartInterval = 6
        };
        Assert.Equal(0, options.RestartRows);
        byte[] fixedJpeg = Encode(pixels, width, height, layout, options);
        Assert.Equal(fixedJpeg, Encode(pixels, width, height, layout, options with { RestartRows = 0 }));
        var rowOptions = options with { RestartRows = 2, RestartInterval = 1 };
        byte[] rowJpeg = Encode(pixels, width, height, layout, rowOptions);
        Assert.Equal(fixedJpeg, rowJpeg);
        Read(rowJpeg, rowOptions);
    }

    [Fact]
    public void RestartNumbersWrapAndNeverEmitATrailingMarkerAtAnExactRowBoundary()
    {
        var options = new JpegliEncoderOptions { RestartRows = 1, Progressive = true };
        JpegDocument document = Read(Encode(Pattern(17, 88, PixelLayout.Gray8), 17, 88, PixelLayout.Gray8, options), options);
        foreach (Scan scan in document.Scans)
            Assert.Equal(new[] { 0xd0, 0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd0, 0xd1 }, scan.RestartMarkers);
    }

    [Theory]
    [InlineData(21845)]
    [InlineData(21846)]
    [InlineData(65535)]
    [InlineData(65536)]
    [InlineData(int.MaxValue)]
    public void RowsClampToTheDriLimitWithoutIntegerOverflow(int rows)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        foreach (bool progressive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                RestartRows = rows, Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, options);
            Assert.Equal(Encode(pixels, width, height, PixelLayout.Gray8,
                options with { RestartRows = 0, RestartInterval = 65535 }), jpeg);
            JpegDocument actual = Read(jpeg, options);
            Assert.All(actual.Scans, scan => Assert.Empty(scan.RestartMarkers));
        }
    }

    [Fact]
    public void ClampingIsAppliedSeparatelyWhenOnlySomeComponentWidthsExceedTheLimit()
    {
        const int width = 17, height = 17;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            RestartRows = 30000, Progressive = true, ProgressiveRefinementBits = 1,
            ProgressiveAcScanEnds = [2, 63], ProgressiveAcRefinementBits = [0, 2]
        };
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument actual = Read(jpeg, options);
        Assert.Equal(new[] { 60000, 65535 }, actual.Scans.Select(scan => scan.RestartInterval).Distinct().Order());
        AssertScanIntervals(actual, 30000);
        byte[] uninterrupted = Encode(pixels, width, height, PixelLayout.Rgb24, options with { RestartRows = 0 });
        AssertRealParity(uninterrupted, Read(uninterrupted, options with { RestartRows = 0 }),
            jpeg, actual, PixelLayout.Rgb24);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlatAcEobRunsEndAtEachComponentRowRestartBoundary(bool optimize)
    {
        const int width = 65, height = 49;
        var options = new JpegliEncoderOptions
        {
            Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 2,
            ProgressiveAcScanEnds = [1, 5, 63], ProgressiveAcRefinementBits = [0, 3, 1],
            RestartRows = 2, OptimizeHuffmanTables = optimize, AdaptiveQuantization = false
        };
        byte[] jpeg = Encode(FlatPixels(width, height, PixelLayout.Rgb24), width, height, PixelLayout.Rgb24, options);
        JpegDocument document = Read(jpeg, options);
        foreach (Scan scan in document.Scans.Where(scan => scan.First > 0))
        {
            int limit = optimize ? Interval(document, scan, 2) : 1;
            Assert.Equal(Enumerable.Range(0, (scan.CodingUnits - 1) / limit + 1)
                .Select(index => Math.Min(limit, scan.CodingUnits - index * limit)), scan.EobRuns);
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
    public void PaddedTypedInputsAndNonseekableStreamsMatchArrays(int kind, JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17, channels = 4, stride = width * channels + 13;
        Array source = Source(kind, width * height * channels);
        Array padded = Pad(source, width * channels, height, stride);
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, Quality = 90, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            IccProfile = colorSpace == JpegColorSpace.Xyb ? new byte[] { 1, 2, 3 } : default
        };
        byte[] baseline = EncodeSource(source, width, height, PixelLayout.Bgra32, options);
        JpegDocument reference = JpegDocument.Read(baseline, false, 0);
        foreach (bool optimize in new[] { false, true })
        {
            var progressive = options with
            {
                Progressive = true, ProgressiveInterleaveDc = false, ProgressiveRefinementBits = 1,
                ProgressiveAcScanEnds = [1, 5, 63], ProgressiveAcRefinementBits = [0, 3, 1],
                RestartRows = 2, RestartInterval = 7, OptimizeHuffmanTables = optimize
            };
            byte[] jpeg = EncodeSource(source, width, height, PixelLayout.Bgra32, progressive);
            Assert.Equal(jpeg, EncodeSource(padded, width, height, PixelLayout.Bgra32, progressive, stride));
            AssertRealParity(baseline, reference, jpeg, Read(jpeg, progressive),
                colorSpace == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            EncodeSource(output, padded, width, height, PixelLayout.Bgra32, progressive,
                TestContext.Current.CancellationToken, stride);
            Assert.False(output.Closed);
            Assert.Equal(jpeg, destination.ToArray());
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NegativeRowsAreRejectedBeforeWritingForEveryInputType(int rows) =>
        AssertInvalid(new() { RestartRows = rows }, nameof(JpegliEncoderOptions.RestartRows));

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(int.MaxValue)]
    public void PositiveRowsStillValidateTheFixedIntervalBeforeOutput(int interval) =>
        AssertInvalid(new() { RestartRows = 1, RestartInterval = interval }, nameof(JpegliEncoderOptions.RestartInterval));

    [Fact]
    public void PositiveRowsStillValidateOtherOptionsBeforeOutput()
    {
        AssertInvalid(new() { RestartRows = 1, Quality = 101 }, nameof(JpegliEncoderOptions.Quality));
        AssertInvalid(new() { RestartRows = 1, ProgressiveRefinementBits = 1 },
            nameof(JpegliEncoderOptions.ProgressiveRefinementBits));
    }

    [Fact]
    public void PreCancellationWritesNoOutputForEveryInputType()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions { RestartRows = int.MaxValue, ProgressiveLevel = 2 };
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringRefinementWithChangingDriLeavesTheStreamOpen(bool optimize)
    {
        const int width = 65, height = 33;
        var options = new JpegliEncoderOptions
        {
            RestartRows = 1, Progressive = true, ProgressiveInterleaveDc = false,
            ProgressiveRefinementBits = 1, ProgressiveAcScanEnds = [1, 5, 63],
            ProgressiveAcRefinementBits = [2, 0, 1], OptimizeHuffmanTables = optimize
        };
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        int cancelAfter = Read(complete, options).Scans[^1].DataStart + 1;
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

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options)
    {
        int maxH = 1, maxV = 1;
        // SOF is read independently because level presets choose interleaving
        // from the resolved output factors, including grayscale and custom factors.
        for (int position = 2; ;)
        {
            Assert.Equal(0xff, jpeg[position++]);
            int marker = jpeg[position++];
            int length = jpeg[position] * 256 + jpeg[position + 1];
            if (marker is 0xc0 or 0xc2)
            {
                int count = jpeg[position + 7];
                for (int component = 0; component < count; component++)
                {
                    int sampling = jpeg[position + 9 + component * 3];
                    maxH = Math.Max(maxH, sampling >> 4);
                    maxV = Math.Max(maxV, sampling & 15);
                }
                break;
            }
            position += length;
        }
        int? level = options.ProgressiveLevel;
        return JpegDocument.Read(jpeg, level is int value ? value > 0 : options.Progressive,
            options.RestartInterval, level is null ? options.ProgressiveRefinementBits : 0,
            level is null ? options.ProgressiveAcScanEnds : level == 1 ? [63] : [2, 63],
            level is null ? options.ProgressiveAcRefinementBits : level == 1 ? [1] : [0, 2],
            level is null ? options.ProgressiveInterleaveDc : maxH == 1 && maxV == 1,
            expectedRestartRows: options.RestartRows);
    }

    private static int Interval(JpegDocument document, Scan scan, int rows)
    {
        int maxH = document.Components.Values.Max(component => component.H);
        int horizontal = scan.Selectors.Count > 1 ? 1 : document.Components[scan.Selectors[0].Component].H;
        int columns = (document.Width * horizontal + 8 * maxH - 1) / (8 * maxH);
        return (int)Math.Min((long)rows * columns, 65535);
    }

    private static void AssertScanIntervals(JpegDocument document, int rows)
    {
        foreach (Scan scan in document.Scans)
        {
            int interval = Interval(document, scan, rows);
            Assert.Equal(interval, scan.RestartInterval);
            Assert.Equal((scan.CodingUnits - 1) / interval, scan.RestartMarkers.Count);
            Assert.Equal(Enumerable.Range(0, scan.RestartMarkers.Count).Select(index => 0xd0 + index % 8),
                scan.RestartMarkers);
        }
    }

    private static void AssertRealParity(byte[] baseline, JpegDocument reference, byte[] jpeg,
        JpegDocument actual, PixelLayout layout)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count(pair => !pair.Value.IsDummy), actual.Blocks.Count(pair => !pair.Value.IsDummy));
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(jpeg, channels).Data);
    }

    private static void AssertInvalid(JpegliEncoderOptions options, string parameter)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        {
            using var destination = new MemoryStream();
            destination.Write([0x12, 0x34]);
            using var output = new WriteOnlyStream(destination);
            ArgumentException error = Assert.ThrowsAny<ArgumentException>(() =>
                EncodeSource(output, Source(kind, 3), 1, 1, PixelLayout.Rgb24, options,
                    TestContext.Current.CancellationToken));
            Assert.Equal(parameter, error.ParamName);
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
