using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderSequentialScansTests
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
    public void CompleteComponentScansPreserveRealCoefficientsAndPixelsWithOddEdgesAndRestarts(
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
            var sequential = options with { SequentialInterleaveComponents = false, RestartInterval = interval };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, sequential);
            JpegDocument actual = ReadSequential(jpeg, sequential);
            AssertRealParity(baseline, reference, jpeg, actual, false);
            AssertComponentGeometry(actual, sequential);
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
    public void CustomSamplingUsesRealBlockRasterOrderAndPerComponentRestartRows(
        int yh, int yv, int cbh, int cbv, int crh, int crv, bool optimize)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 25), (145, 97) })
        {
            byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
            var options = new JpegliEncoderOptions
            {
                Quality = 90, SamplingFactors = new(new(yh, yv), new(cbh, cbv), new(crh, crv)),
                SmoothingFactor = optimize ? 73 : 0, OptimizeHuffmanTables = optimize
            };
            byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
            var sequential = options with
            {
                SequentialInterleaveComponents = false, RestartRows = 1, RestartInterval = 7
            };
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, sequential);
            JpegDocument actual = ReadSequential(jpeg, sequential);
            AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg, actual, false);
            AssertComponentGeometry(actual, sequential);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestartNumbersWrapAndResetAtEachScanWithoutTrailingMarkers(bool optimize)
    {
        const int width = 17, height = 88;
        var options = new JpegliEncoderOptions
        {
            SequentialInterleaveComponents = false, Subsampling = ChromaSubsampling.None,
            RestartRows = 1, OptimizeHuffmanTables = optimize
        };
        JpegDocument document = ReadSequential(Encode(Pattern(width, height, PixelLayout.Rgb24),
            width, height, PixelLayout.Rgb24, options), options);
        foreach (Scan scan in document.Scans)
            Assert.Equal(new[] { 0xd0, 0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd0, 0xd1 },
                scan.RestartMarkers);
    }

    [Theory]
    [InlineData(30000)]
    [InlineData(int.MaxValue)]
    public void RestartRowsClampSeparatelyForEachComponentWithoutOverflow(int rows)
    {
        const int width = 17, height = 17;
        var options = new JpegliEncoderOptions { SequentialInterleaveComponents = false, RestartRows = rows };
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = ReadSequential(jpeg, options);
        AssertComponentGeometry(document, options);
        Assert.All(document.Scans, scan => Assert.Empty(scan.RestartMarkers));
        Assert.Equal(rows == int.MaxValue ? new[] { 65535, 65535, 65535 } : new[] { 65535, 60000, 60000 },
            document.Scans.Select(scan => scan.RestartInterval));
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options with
        {
            SequentialInterleaveComponents = true, RestartRows = 0
        });
        AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg, document, false);
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
    public void TypedPaddedInputsAndNonseekableStreamsPreserveCoefficientsAndMetadata(
        int kind, JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17, channels = 4, stride = width * channels + 13;
        Array source = Source(kind, width * height * channels);
        Array padded = Pad(source, width * channels, height, stride);
        foreach (bool optimize in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                ColorSpace = colorSpace, Quality = 90, SmoothingFactor = 37,
                SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)), OptimizeHuffmanTables = optimize,
                IccProfile = new byte[] { 1, 2, 3 }, ExifData = new byte[] { 4, 5, 6 },
                XmpPacket = "<metadata>sequential scans</metadata>", IptcData = new byte[] { 7, 8, 9 },
                Comment = "component scans \u03c0", DensityUnit = JpegDensityUnit.DotsPerInch,
                HorizontalDensity = 96, VerticalDensity = 120
            };
            byte[] baseline = EncodeSource(source, width, height, options);
            var sequential = options with { SequentialInterleaveComponents = false, RestartRows = 2 };
            byte[] jpeg = EncodeSource(source, width, height, sequential);
            Assert.Equal(jpeg, EncodeSource(padded, width, height, sequential, stride));
            if (colorSpace == JpegColorSpace.Grayscale)
                Assert.Equal(jpeg, EncodeSource(source, width, height,
                    sequential with { SequentialInterleaveComponents = true }));
            AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg,
                ReadSequential(jpeg, sequential), colorSpace == JpegColorSpace.Grayscale);
            Assert.Equal(Metadata(baseline).Count, Metadata(jpeg).Count);
            foreach ((byte[] expected, byte[] actual) in Metadata(baseline).Zip(Metadata(jpeg)))
                Assert.Equal(expected, actual);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            EncodeSource(output, padded, width, height, sequential, TestContext.Current.CancellationToken, stride);
            Assert.False(output.Closed);
            Assert.Equal(jpeg, destination.ToArray());
        }
    }

    [Theory]
    [InlineData(PixelComponentLayout.YCbCr, false)]
    [InlineData(PixelComponentLayout.YCbCr, true)]
    [InlineData(PixelComponentLayout.Xyb, false)]
    [InlineData(PixelComponentLayout.Xyb, true)]
    public void DirectComponentInputsPreserveSharedAndSeparateCustomQuantizationTables(
        PixelComponentLayout layout, bool optimize)
    {
        const int width = 19, height = 17, stride = width * 3 + 7;
        foreach (int kind in new[] { 0, 1, 2 })
            foreach (int tableCount in new[] { 1, 2, 3 })
            {
                Array source = Source(kind, width * height * 3);
                Array padded = Pad(source, width * 3, height, stride);
                ReadOnlyMemory<ushort>[] tables = Enumerable.Range(0, tableCount)
                    .Select(table => (ReadOnlyMemory<ushort>)Enumerable.Range(0, 64)
                        .Select(index => (ushort)(7 + table * 11 + index % 13)).ToArray()).ToArray();
                var options = new JpegliEncoderOptions
                {
                    SamplingFactors = new(new(2, 1), new(1, 1), new(1, 2)),
                    QuantizationTables = tables, OptimizeHuffmanTables = optimize,
                    IccProfile = layout == PixelComponentLayout.Xyb ? new byte[] { 1, 2, 3 } : default
                };
                byte[] baseline = EncodeComponents(source, width, height, layout, options);
                var sequential = options with { SequentialInterleaveComponents = false, RestartInterval = 2 };
                byte[] jpeg = EncodeComponents(source, width, height, layout, sequential);
                Assert.Equal(jpeg, EncodeComponents(padded, width, height, layout, sequential, stride));
                JpegDocument actual = ReadSequential(jpeg, sequential);
                AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg, actual, false);
                Assert.Equal(tableCount, actual.Quantizers.Count);
                Assert.Equal(tableCount == 1 ? new[] { 0, 0, 0 } : tableCount == 2 ? new[] { 0, 1, 1 } : new[] { 0, 1, 2 },
                    actual.Components.Values.Select(component => component.Quantizer));
            }
    }

    [Theory]
    [InlineData(PixelComponentLayout.Gray)]
    [InlineData(PixelComponentLayout.Rgb)]
    [InlineData(PixelComponentLayout.YCbCr)]
    public void AcceptedOutOfRangeFloatsPreserveEveryCoefficient(PixelComponentLayout layout)
    {
        const int width = 19, height = 17;
        int channels = layout == PixelComponentLayout.Gray ? 1 : 3;
        float[] source = (float[])Source(2, width * height * channels);
        for (int index = 0; index < source.Length; index += 17)
            source[index] = index % 34 == 0 ? -0.01f : 1.01f;
        foreach (bool optimize in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Quality = 90, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
                SmoothingFactor = 37, OptimizeHuffmanTables = optimize
            };
            byte[] baseline = EncodeComponents(source, width, height, layout, options);
            var sequential = options with { SequentialInterleaveComponents = false, RestartRows = 1 };
            byte[] jpeg = EncodeComponents(source, width, height, layout, sequential);
            AssertRealParity(baseline, JpegDocument.Read(baseline, false, 0), jpeg,
                ReadSequential(jpeg, sequential), layout == PixelComponentLayout.Gray);
        }
    }

    [Theory]
    [InlineData(false, 1.01f)]
    [InlineData(true, float.NaN)]
    [InlineData(true, float.PositiveInfinity)]
    [InlineData(true, -0.01f)]
    public void InvalidLateFloatComponentsArePreflightedBeforeAnyOutput(bool allow, float invalid)
    {
        const int width = 24, height = 8;
        float[] source = Enumerable.Repeat(0.5f, width * height * 3).ToArray();
        for (int row = 0; row < height; row++)
            for (int column = 16; column < width; column++)
                source[(row * width + column) * 3 + 2] = invalid;
        // With unit quantization, the final blue block at -0.01 has DC about
        // -1044, outside the baseline signed eleven-bit coefficient range.
        foreach (bool optimize in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                SequentialInterleaveComponents = false, ColorSpace = JpegColorSpace.Rgb,
                QuantizationTables = new ReadOnlyMemory<ushort>[] { Enumerable.Repeat((ushort)1, 64).ToArray() },
                AllowOutOfRangeFloatSamples = allow, AdaptiveQuantization = false, OptimizeHuffmanTables = optimize
            };
            var arrayError = Assert.Throws<ArgumentException>(() =>
                EncodeComponents(source, width, height, PixelComponentLayout.Rgb, options));
            Assert.Equal("pixels", arrayError.ParamName);
            using var destination = new MemoryStream();
            destination.Write([0x12, 0x34]);
            using var output = new WriteOnlyStream(destination);
            var streamError = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, source,
                width, height, PixelComponentLayout.Rgb, options, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal("pixels", streamError.ParamName);
            Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
            Assert.False(output.Closed);
        }
    }

    [Fact]
    public void DefaultTrueAndZeroProgressiveLevelPreserveBytes()
    {
        Assert.True(new JpegliEncoderOptions().SequentialInterleaveComponents);
        foreach (bool optimize in new[] { false, true })
            foreach (PixelLayout layout in new[] { PixelLayout.Gray8, PixelLayout.Rgb24 })
            {
                const int width = 19, height = 17;
                byte[] pixels = Pattern(width, height, layout);
                var options = new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, RestartRows = 2 };
                byte[] baseline = Encode(pixels, width, height, layout, options);
                Assert.Equal(baseline, Encode(pixels, width, height, layout,
                    options with { SequentialInterleaveComponents = true }));
                byte[] componentScans = Encode(pixels, width, height, layout,
                    options with { SequentialInterleaveComponents = false });
                Assert.Equal(componentScans, Encode(pixels, width, height, layout, options with
                {
                    SequentialInterleaveComponents = false, Progressive = true, ProgressiveLevel = 0
                }));
                if (layout == PixelLayout.Gray8)
                    Assert.Equal(baseline, componentScans);
            }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void EffectiveProgressiveEncodingRejectsComponentSequentialScansBeforeOutput(int? level)
    {
        foreach (int kind in new[] { 0, 1, 2 })
            foreach (JpegColorSpace colorSpace in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr })
            {
                var options = new JpegliEncoderOptions
                {
                    SequentialInterleaveComponents = false, Progressive = level is null,
                    ProgressiveLevel = level, ColorSpace = colorSpace
                };
                Array source = Source(kind, 4);
                var arrayError = Assert.Throws<ArgumentException>(() => EncodeSource(source, 1, 1, options));
                Assert.Equal(nameof(JpegliEncoderOptions.SequentialInterleaveComponents), arrayError.ParamName);
                using var destination = new MemoryStream();
                destination.Write([0x12, 0x34]);
                using var output = new WriteOnlyStream(destination);
                var streamError = Assert.Throws<ArgumentException>(() =>
                    EncodeSource(output, source, 1, 1, options, TestContext.Current.CancellationToken));
                Assert.Equal(nameof(JpegliEncoderOptions.SequentialInterleaveComponents), streamError.ParamName);
                Assert.Equal(new byte[] { 0x12, 0x34 }, destination.ToArray());
                Assert.False(output.Closed);
            }
    }

    [Fact]
    public void PreCancellationWritesNoOutputForEveryInputType()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var options = new JpegliEncoderOptions { SequentialInterleaveComponents = false, RestartRows = 1 };
        foreach (int kind in new[] { 0, 1, 2 })
        {
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            var error = Assert.Throws<OperationCanceledException>(() =>
                EncodeSource(output, Source(kind, 4), 1, 1, options, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken);
            Assert.Empty(destination.ToArray());
            Assert.False(output.Closed);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringEachLaterComponentScanLeavesTheStreamOpen(bool optimize)
    {
        const int width = 65, height = 49;
        var options = new JpegliEncoderOptions
        {
            SequentialInterleaveComponents = false, RestartRows = 1,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)), OptimizeHuffmanTables = optimize
        };
        foreach (int kind in new[] { 0, 1, 2 })
        {
            Array source = Source(kind, width * height * 4);
            byte[] complete = EncodeSource(source, width, height, options);
            JpegDocument document = ReadSequential(complete, options);
            foreach (Scan scan in document.Scans.Skip(1))
            {
                int cancelAfter = scan.DataStart + 1;
                using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination, () =>
                {
                    if (destination.Length >= cancelAfter)
                        cancellation.Cancel();
                });
                var error = Assert.Throws<OperationCanceledException>(() =>
                    EncodeSource(output, source, width, height, options, cancellation.Token));
                Assert.Equal(cancellation.Token, error.CancellationToken);
                Assert.False(output.Closed);
                Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
                Assert.Equal(complete[..cancelAfter], destination.ToArray()[..cancelAfter]);
            }
        }
    }

    private static void AssertRealParity(byte[] baseline, JpegDocument reference, byte[] jpeg,
        JpegDocument actual, bool grayscale)
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
        ColorComponents channels = grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(jpeg, channels).Data);
    }

    private static void AssertComponentGeometry(JpegDocument document, JpegliEncoderOptions options)
    {
        Assert.Equal(document.Components.Count, document.Scans.Count);
        Assert.Equal(document.Components.Keys, document.Scans.Select(scan => Assert.Single(scan.Selectors).Component));
        Assert.All(document.Blocks.Values, block => Assert.False(block.IsDummy));
        int maxH = document.Components.Values.Max(component => component.H);
        int maxV = document.Components.Values.Max(component => component.V);
        foreach (Scan scan in document.Scans)
        {
            Selector selector = Assert.Single(scan.Selectors);
            Component component = document.Components[selector.Component];
            int columns = Ceiling(document.Width * component.H, 8 * maxH);
            int rows = Ceiling(document.Height * component.V, 8 * maxV);
            int interval = options.RestartRows == 0 ? options.RestartInterval
                : (int)Math.Min((long)columns * options.RestartRows, 65535);
            Assert.Equal(columns * rows, scan.CodingUnits);
            Assert.Equal(columns * rows, document.Blocks.Count(pair => pair.Key.Component == selector.Component));
            Assert.Equal(interval, scan.RestartInterval);
            int count = interval == 0 ? 0 : (columns * rows - 1) / interval;
            Assert.Equal(Enumerable.Range(0, count).Select(index => 0xd0 + index % 8), scan.RestartMarkers);
        }
    }

    // Independent single-component sequential reader. The existing progressive
    // reader checks a single interleaved baseline scan, so this reader derives
    // traversal and restart boundaries from the emitted SOF/SOS/DRI only.
    private static JpegDocument ReadSequential(byte[] jpeg, JpegliEncoderOptions options)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        int position = 2, width = 0, height = 0, frames = 0, interval = 0, priorInterval = 0;
        bool pendingDri = false;
        var components = new Dictionary<int, Component>();
        var quantizers = new Dictionary<int, byte[]>();
        var tables = new Dictionary<int, Dictionary<int, int>>();
        var blocks = new Dictionary<BlockKey, Block>();
        var scans = new List<Scan>();
        while (true)
        {
            Assert.Equal(0xff, jpeg[position++]);
            int marker = jpeg[position++];
            if (marker == 0xd9)
            {
                Assert.Equal(jpeg.Length, position);
                break;
            }
            int length = Word(jpeg, position), end = position + length;
            Assert.InRange(length, 2, jpeg.Length - position);
            position += 2;
            switch (marker)
            {
                case 0xc0:
                    frames++;
                    Assert.Equal(8, jpeg[position++]);
                    height = Word(jpeg, position);
                    width = Word(jpeg, position + 2);
                    position += 4;
                    int componentCount = jpeg[position++];
                    Assert.True(componentCount is 1 or 3);
                    for (int component = 0; component < componentCount; component++)
                    {
                        int id = jpeg[position++], factors = jpeg[position++];
                        components.Add(id, new(factors >> 4, factors & 15, jpeg[position++]));
                    }
                    Assert.Equal(end, position);
                    break;
                case 0xc2:
                    Assert.Fail("Component sequential scans must use SOF0.");
                    break;
                case 0xdb:
                    while (position < end)
                    {
                        int selector = jpeg[position++];
                        Assert.Equal(0, selector >> 4);
                        quantizers.Add(selector & 15, jpeg[position..(position + 64)]);
                        position += 64;
                    }
                    Assert.Equal(end, position);
                    break;
                case 0xc4:
                    while (position < end)
                    {
                        int selector = jpeg[position++];
                        byte[] counts = jpeg[position..(position + 16)];
                        position += 16;
                        var codes = new Dictionary<int, int>();
                        int code = 0;
                        for (int bits = 1; bits <= 16; bits++)
                        {
                            for (int symbol = 0; symbol < counts[bits - 1]; symbol++)
                                codes.Add((bits << 16) | code++, jpeg[position++]);
                            code <<= 1;
                        }
                        tables[selector] = codes;
                    }
                    Assert.Equal(end, position);
                    break;
                case 0xdd:
                    Assert.Equal(4, length);
                    Assert.False(pendingDri);
                    interval = Word(jpeg, position);
                    pendingDri = true;
                    break;
                case 0xda:
                    Assert.Equal(1, frames);
                    Assert.Equal(1, jpeg[position++]);
                    int selected = jpeg[position++], selectors = jpeg[position++];
                    Assert.Equal(components.Keys.ElementAt(scans.Count), selected);
                    Assert.Equal(0, jpeg[position++]);
                    Assert.Equal(63, jpeg[position++]);
                    Assert.Equal(0, jpeg[position++]);
                    Assert.Equal(end, position);
                    var scan = new Scan([new(selected, selectors >> 4, 0x10 | (selectors & 15))], 0, 63, 0, 0, end);
                    int maxH = components.Values.Max(component => component.H);
                    int maxV = components.Values.Max(component => component.V);
                    Component componentFactors = components[selected];
                    int columns = Ceiling(width * componentFactors.H, 8 * maxH);
                    int rows = Ceiling(height * componentFactors.V, 8 * maxV);
                    int expectedInterval = options.RestartRows == 0 ? options.RestartInterval
                        : (int)Math.Min((long)options.RestartRows * columns, 65535);
                    Assert.Equal(expectedInterval, interval);
                    Assert.Equal(interval != priorInterval, pendingDri);
                    priorInterval = interval;
                    pendingDri = false;
                    scan.RestartInterval = interval;
                    scan.CodingUnits = columns * rows;
                    int unit = 0, restart = 0;
                    while (unit < scan.CodingUnits)
                    {
                        (byte[] entropy, int terminator) = ReadEntropy(jpeg, ref position);
                        var bits = new SequentialBits(entropy);
                        int previousDc = 0;
                        int stop = interval == 0 ? scan.CodingUnits : Math.Min(unit + interval, scan.CodingUnits);
                        for (; unit < stop; unit++)
                        {
                            int difference = bits.SignedValue(bits.Symbol(tables[selectors >> 4]));
                            previousDc += difference;
                            int[] coefficients = new int[64];
                            coefficients[0] = previousDc;
                            for (int coefficient = 1; coefficient < 64;)
                            {
                                int symbol = bits.Symbol(tables[0x10 | (selectors & 15)]);
                                if (symbol == 0)
                                    break;
                                if (symbol == 0xf0)
                                {
                                    coefficient += 16;
                                    Assert.InRange(coefficient, 1, 64);
                                    continue;
                                }
                                Assert.InRange(symbol & 15, 1, 10);
                                coefficient += symbol >> 4;
                                Assert.InRange(coefficient, 1, 63);
                                coefficients[coefficient++] = bits.SignedValue(symbol & 15);
                            }
                            Assert.True(blocks.TryAdd(new(selected, unit % columns, unit / columns),
                                new(false, difference, coefficients)));
                        }
                        bits.AssertOnlyPaddingRemains();
                        if (unit < scan.CodingUnits)
                        {
                            Assert.Equal(0xd0 + restart++ % 8, terminator);
                            scan.RestartMarkers.Add(terminator);
                        }
                        else
                        {
                            Assert.True(terminator is 0xc4 or 0xda or 0xdd or 0xd9);
                            position -= 2;
                        }
                    }
                    scans.Add(scan);
                    continue;
            }
            position = end;
        }
        Assert.Equal(1, frames);
        Assert.False(pendingDri);
        Assert.Equal(components.Count, scans.Count);
        Assert.Equal(components.Values.Select(component => component.Quantizer).Distinct().Count(), quantizers.Count);
        int mcuCount = Ceiling(width, 8 * components.Values.Max(component => component.H))
            * Ceiling(height, 8 * components.Values.Max(component => component.V));
        return new(width, height, mcuCount, components, quantizers, blocks, scans);
    }

    private static (byte[] Bytes, int Terminator) ReadEntropy(byte[] jpeg, ref int position)
    {
        var bytes = new List<byte>();
        while (true)
        {
            byte value = jpeg[position++];
            if (value != 0xff)
            {
                bytes.Add(value);
                continue;
            }
            int next = jpeg[position++];
            if (next != 0)
                return (bytes.ToArray(), next);
            bytes.Add(0xff);
        }
    }

    private static List<byte[]> Metadata(byte[] jpeg)
    {
        var metadata = new List<byte[]>();
        for (int position = 2; ;)
        {
            int start = position;
            Assert.Equal(0xff, jpeg[position++]);
            int marker = jpeg[position++];
            if (marker == 0xda)
                return metadata;
            int end = position + Word(jpeg, position);
            if (marker is >= 0xe0 and <= 0xef or 0xfe)
                metadata.Add(jpeg[start..end]);
            position = end;
        }
    }

    private sealed class SequentialBits(byte[] bytes)
    {
        private int position;

        public int Symbol(Dictionary<int, int> table)
        {
            int code = 0;
            for (int length = 1; length <= 16; length++)
            {
                code = (code << 1) | Bit();
                if (table.TryGetValue((length << 16) | code, out int symbol))
                    return symbol;
            }
            throw new InvalidDataException("Invalid sequential Huffman code.");
        }

        public int SignedValue(int count)
        {
            Assert.InRange(count, 0, 11);
            int value = 0;
            for (int bit = 0; bit < count; bit++)
                value = (value << 1) | Bit();
            return count == 0 || value >= (1 << (count - 1)) ? value : value - (1 << count) + 1;
        }

        public void AssertOnlyPaddingRemains()
        {
            Assert.InRange(bytes.Length * 8 - position, 0, 7);
            while (position < bytes.Length * 8)
                Assert.Equal(1, Bit());
        }

        private int Bit()
        {
            Assert.True(position < bytes.Length * 8, "Unexpected end of sequential entropy data.");
            return (bytes[position / 8] >> (7 - position++ % 8)) & 1;
        }
    }

    private static int Word(byte[] bytes, int position) => bytes[position] * 256 + bytes[position + 1];
    private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;

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

    private static byte[] EncodeSource(Array source, int width, int height,
        JpegliEncoderOptions options, int stride = 0) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, PixelLayout.Bgra32, options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Bgra, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, PixelComponentLayout.Bgra, options, stride, TestContext.Current.CancellationToken)
    };

    private static byte[] EncodeComponents(Array source, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)layout, options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout, options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, layout, options, stride, TestContext.Current.CancellationToken)
    };

    private static void EncodeSource(Stream output, Array source, int width, int height,
        JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, PixelLayout.Bgra32, options, stride, cancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, PixelComponentLayout.Bgra, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, PixelComponentLayout.Bgra, options, stride, cancellationToken);
    }
}
