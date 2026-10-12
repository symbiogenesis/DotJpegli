using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderProgressiveTests
{
    public static TheoryData<PixelLayout, ChromaSubsampling, bool, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, ChromaSubsampling, bool, bool>();
            foreach (bool optimize in new[] { false, true })
                foreach (bool adaptive in new[] { false, true })
                {
                    modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, optimize, adaptive);
                    foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                        modes.Add(PixelLayout.Rgb24, sampling, optimize, adaptive);
                }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void InitialDcAndComponentAcScansPreserveBaselineCoefficientsAndPixels(
        PixelLayout layout, ChromaSubsampling sampling, bool optimize, bool adaptive)
    {
        // Odd edges create wholly padded interleaved luma blocks. Even 4:2:0
        // has 20 DC MCUs, so short restart intervals wrap RST7 to RST0.
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling,
            OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive
        };
        Assert.False(options.Progressive);
        byte[] baseline = Encode(pixels, width, height, layout, options);
        Assert.Equal(baseline, Encode(pixels, width, height, layout, options with { Progressive = false }));
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        byte[] expectedPixels = ImageResult.FromMemory(baseline, channels).Data;

        foreach (int interval in new[] { 0, 1, 2, 7, reference.McuCount, reference.McuCount + 1, 65535 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with
            {
                Progressive = true, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true, expectedInterval: interval);
            Assert.Equal(reference.Width, actual.Width);
            Assert.Equal(reference.Height, actual.Height);
            Assert.Equal(reference.Components, actual.Components);
            foreach ((int selector, byte[] quantizer) in reference.Quantizers)
                Assert.Equal(quantizer, actual.Quantizers[selector]);
            Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
            foreach ((BlockKey key, Block expected) in reference.Blocks)
            {
                Block block = actual.Blocks[key];
                Assert.Equal(expected.IsDummy, block.IsDummy);
                if (block.IsDummy)
                {
                    Assert.Equal(0, block.DcDifference);
                    Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                }
                else
                    Assert.Equal(expected.Coefficients, block.Coefficients);
            }
            ImageResult decoded = ImageResult.FromMemory(jpeg, channels);
            Assert.Equal(width, decoded.Width);
            Assert.Equal(height, decoded.Height);
            Assert.Equal(expectedPixels, decoded.Data);
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, ChromaSubsampling.Quarter, false)]
    [InlineData(PixelLayout.Gray8, ChromaSubsampling.Quarter, true)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.None, false)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.None, true)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.HalfHorizontal, false)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.HalfHorizontal, true)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.Quarter, false)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.Quarter, true)]
    public void TinyAndNarrowImagesHaveOneAcCodingUnitPerRealBlock(
        PixelLayout layout, ChromaSubsampling sampling, bool optimize)
    {
        foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 17) })
        {
            byte[] pixels = Pattern(width, height, layout);
            var options = new JpegliEncoderOptions
            {
                Subsampling = sampling, OptimizeHuffmanTables = optimize, RestartInterval = 2
            };
            byte[] baseline = Encode(pixels, width, height, layout, options);
            byte[] progressive = Encode(pixels, width, height, layout, options with { Progressive = true });
            JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
            JpegDocument actual = JpegDocument.Read(progressive, progressive: true, expectedInterval: 2);
            Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
            foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
                Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
            ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
            Assert.Equal(ImageResult.FromMemory(baseline, channels).Data,
                ImageResult.FromMemory(progressive, channels).Data);
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, false)]
    [InlineData(PixelLayout.Gray8, true)]
    [InlineData(PixelLayout.Rgb24, false)]
    [InlineData(PixelLayout.Rgb24, true)]
    public void FlatImagesAggregateEobRunsOnlyWithOptimizedTablesAndResetAtEveryBoundary(
        PixelLayout layout, bool optimize)
    {
        // Odd 4:2:0 edges leave padded DC blocks that must not extend an AC run.
        // The two chroma scans share a table but must begin with fresh run state.
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
                Progressive = true, RestartInterval = interval
            });
            JpegDocument actual = JpegDocument.Read(jpeg, progressive: true, expectedInterval: interval);
            AssertBaselineParity(baseline, reference, jpeg, actual, layout);
            Assert.Empty(actual.Scans[0].EobRuns);
            foreach (Scan scan in actual.Scans.Skip(1))
            {
                int component = Assert.Single(scan.Selectors).Component;
                int realBlocks = reference.Blocks.Count(pair =>
                    pair.Key.Component == component && !pair.Value.IsDummy);
                int runLimit = optimize ? (interval == 0 ? realBlocks : interval) : 1;
                int[] expectedRuns = Enumerable.Range(0, (realBlocks - 1) / runLimit + 1)
                    .Select(index => Math.Min(runLimit, realBlocks - index * runLimit)).ToArray();
                Assert.Equal(expectedRuns, scan.EobRuns);
                Assert.Equal(realBlocks, scan.EobRuns.Sum());
                if (optimize && interval != 1)
                    Assert.True(scan.EobRuns.Count < realBlocks);
            }
        }
    }

    [Fact]
    public void EobRunsFlushBeforeTexturedBlocksAndPreserveTheirCoefficients()
    {
        const int width = 160, height = 8;
        byte[] pixels = FlatPixels(width, height, PixelLayout.Gray8);
        byte[] texture = Pattern(width, height, PixelLayout.Gray8);
        foreach (int block in new[] { 7, 15 })
            for (int y = 0; y < height; y++)
                texture.AsSpan(y * width + block * 8, 8).CopyTo(pixels.AsSpan(y * width + block * 8, 8));
        var options = new JpegliEncoderOptions { Quality = 90, AdaptiveQuantization = false };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Gray8, options);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, options with { Progressive = true });
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        JpegDocument actual = JpegDocument.Read(jpeg, progressive: true, expectedInterval: 0);
        AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Gray8);
        foreach (int block in new[] { 7, 15 })
            Assert.Contains(reference.Blocks[new(1, block, 0)].Coefficients[1..], coefficient => coefficient != 0);
        Assert.Equal(7, actual.Scans[1].EobRuns[0]);
        Assert.Contains(actual.Scans[1].EobRuns, run => run > 1);
        Assert.True(actual.Scans[1].EobRuns.Count < 20);
    }

    [Fact]
    public void EobRunsSplitAtTheMaximumRepresentableLength()
    {
        // 256 columns by 129 real rows gives 33024 blocks, with a partial last
        // pixel row, and needs exactly two EOB symbols even without restarts.
        const int width = 2048, height = 1025;
        byte[] pixels = FlatPixels(width, height, PixelLayout.Gray8);
        var options = new JpegliEncoderOptions { AdaptiveQuantization = false };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Gray8, options);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, options with { Progressive = true });
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        JpegDocument actual = JpegDocument.Read(jpeg, progressive: true, expectedInterval: 0);
        Assert.Equal(new[] { 32767, 257 }, actual.Scans[1].EobRuns);
        AssertBaselineParity(baseline, reference, jpeg, actual, PixelLayout.Gray8);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProgressiveStreamEncodingMatchesArrayWithoutSeekingOrClosing(bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(output, pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        JpegDocument.Read(destination.ToArray(), progressive: true, expectedInterval: 2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringALaterAcScanLeavesStreamOpenWithPartialOutput(bool optimize)
    {
        const int width = 65, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Progressive = true, RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, progressive: true, expectedInterval: 2);
        // Cancel after entropy begins in the second AC scan, after DC and Y AC
        // have completed. This catches tokens checked only in the first scan.
        int cancelAfter = document.Scans[2].DataStart + 1;
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

    [Theory]
    [InlineData(PixelLayout.Rgb24, 3)]
    [InlineData(PixelLayout.Bgr24, 3)]
    [InlineData(PixelLayout.Rgba32, 4)]
    [InlineData(PixelLayout.Bgra32, 4)]
    public void ProgressivePixelLayoutsAndPaddedStrideProduceSameJpeg(PixelLayout layout, int bytesPerPixel)
    {
        const int width = 17, height = 9;
        byte[] rgb = Pattern(width, height, PixelLayout.Rgb24);
        int stride = width * bytesPerPixel + 13;
        byte[] source = new byte[(height - 1) * stride + width * bytesPerPixel];
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3;
                int output = y * stride + x * bytesPerPixel;
                source[output + (bgr ? 2 : 0)] = rgb[input];
                source[output + 1] = rgb[input + 1];
                source[output + (bgr ? 0 : 2)] = rgb[input + 2];
                if (bytesPerPixel == 4)
                    source[output + 3] = (byte)(x * 17);
            }
        var options = new JpegliEncoderOptions { Progressive = true, RestartInterval = 2 };
        Assert.Equal(Encode(rgb, width, height, PixelLayout.Rgb24, options),
            JpegliEncoder.Encode(source, width, height, layout, options, stride,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    internal static byte[] Encode(byte[] pixels, int width, int height, PixelLayout layout, JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);

    internal static void AssertBaselineParity(byte[] baseline, JpegDocument reference,
        byte[] progressive, JpegDocument actual, PixelLayout layout)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data,
            ImageResult.FromMemory(progressive, channels).Data);
    }

    internal static byte[] FlatPixels(int width, int height, PixelLayout layout)
    {
        int channels = layout == PixelLayout.Gray8 ? 1 : 3;
        byte[] pixels = new byte[width * height * channels];
        for (int pixel = 0; pixel < width * height; pixel++)
            for (int channel = 0; channel < channels; channel++)
                pixels[pixel * channels + channel] = (byte)(96 + channel * 40);
        return pixels;
    }

    internal static byte[] Pattern(int width, int height, PixelLayout layout)
    {
        int channels = layout == PixelLayout.Gray8 ? 1 : 3;
        byte[] pixels = new byte[width * height * channels];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < channels; channel++)
                    pixels[(y * width + x) * channels + channel] =
                        (byte)(24 + (11 * x + 19 * y + 37 * channel + 7 * (x / 8) * (channel + 1)) % 208);
        return pixels;
    }

    internal sealed record Component(int H, int V, int Quantizer);
    internal sealed record BlockKey(int Component, int X, int Y);
    internal sealed record Block(bool IsDummy, int DcDifference, int[] Coefficients);
    internal sealed record Selector(int Component, int Dc, int Ac);
    private sealed record EntropySegment(byte[] Bytes, int Terminator);
    internal sealed record Scan(List<Selector> Selectors, int First, int Last, int High, int Low, int DataStart)
    {
        public List<int> EobRuns { get; } = [];
    }

    // This reader reconstructs the quantized coefficients using only the emitted
    // marker definitions and canonical Huffman codes. It uses no encoder helpers,
    // including the encoder's block traversal, scan or restart implementation.
    internal sealed record JpegDocument(int Width, int Height, int McuCount,
        Dictionary<int, Component> Components, Dictionary<int, byte[]> Quantizers,
        Dictionary<BlockKey, Block> Blocks, List<Scan> Scans)
    {
        public static JpegDocument Read(byte[] jpeg, bool progressive, int expectedInterval,
            int expectedRefinementBits = 0)
        {
            Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
            int position = 2, width = 0, height = 0, frameCount = 0, driCount = 0;
            var components = new Dictionary<int, Component>();
            var quantizers = new Dictionary<int, byte[]>();
            var tables = new Dictionary<int, Dictionary<int, int>>();
            var blocks = new Dictionary<BlockKey, Block>();
            var scans = new List<Scan>();
            var acBlocks = new HashSet<BlockKey>();
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
                    case 0xc2:
                        Assert.Equal(progressive ? 0xc2 : 0xc0, marker);
                        frameCount++;
                        Assert.Equal(8, jpeg[position++]);
                        height = Word(jpeg, position);
                        width = Word(jpeg, position + 2);
                        position += 4;
                        int componentCount = jpeg[position++];
                        Assert.True(componentCount is 1 or 3);
                        for (int i = 0; i < componentCount; i++)
                        {
                            int component = jpeg[position++], factors = jpeg[position++];
                            components.Add(component, new(factors >> 4, factors & 15, jpeg[position++]));
                        }
                        Assert.Equal(end, position);
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
                                for (int i = 0; i < counts[bits - 1]; i++)
                                    codes.Add((bits << 16) | code++, jpeg[position++]);
                                code <<= 1;
                            }
                            tables[selector] = codes;
                        }
                        Assert.Equal(end, position);
                        break;
                    case 0xdd:
                        driCount++;
                        Assert.Equal(4, length);
                        Assert.Equal(expectedInterval, Word(jpeg, position));
                        Assert.Empty(scans);
                        break;
                    case 0xda:
                        var selectors = new List<Selector>();
                        int count = jpeg[position++];
                        for (int i = 0; i < count; i++)
                        {
                            int component = jpeg[position++], selector = jpeg[position++];
                            Assert.Contains(component, components.Keys);
                            selectors.Add(new(component, selector >> 4, 0x10 | (selector & 15)));
                        }
                        int first = jpeg[position++], last = jpeg[position++];
                        int approximation = jpeg[position++];
                        int high = approximation >> 4, low = approximation & 15;
                        Assert.Equal(end, position);
                        var scan = new Scan(selectors, first, last, high, low, end);
                        int cycle = progressive ? scans.Count / (components.Count + 1) : 0;
                        int band = progressive ? scans.Count % (components.Count + 1) : 0;
                        Assert.Equal(cycle == 0 ? 0 : expectedRefinementBits - cycle + 1, high);
                        Assert.Equal(progressive ? expectedRefinementBits - cycle : 0, low);
                        if (band == 0)
                        {
                            Assert.Equal(components.Keys, selectors.Select(selector => selector.Component));
                            Assert.Equal(0, first);
                            Assert.Equal(progressive ? 0 : 63, last);
                            if (progressive)
                            {
                                Assert.All(selectors, selector => Assert.Equal(0x10, selector.Ac));
                                if (high != 0)
                                    Assert.All(selectors, selector => Assert.Equal(0, selector.Dc));
                            }
                        }
                        else
                        {
                            Selector selector = Assert.Single(selectors);
                            Assert.Equal(components.Keys.ElementAt(band - 1), selector.Component);
                            Assert.Equal(1, first);
                            Assert.Equal(63, last);
                            Assert.Equal(0, selector.Dc);
                        }
                        List<EntropySegment> segments = ReadSegments(jpeg, ref position);
                        DecodeScan(scan, segments, width, height, components, tables,
                            blocks, acBlocks, progressive, expectedInterval);
                        scans.Add(scan);
                        continue;
                }
                position = end;
            }
            Assert.Equal(1, frameCount);
            Assert.Equal(expectedInterval == 0 ? 0 : 1, driCount);
            Assert.Equal(progressive ? (components.Count + 1) * (expectedRefinementBits + 1) : 1, scans.Count);
            Assert.Equal(blocks.Count(pair => !pair.Value.IsDummy), acBlocks.Count);
            Assert.Equal(components.Count, quantizers.Count);
            int maxH = components.Values.Max(component => component.H);
            int maxV = components.Values.Max(component => component.V);
            int mcuCount = Ceiling(width, 8 * maxH) * Ceiling(height, 8 * maxV);
            return new(width, height, mcuCount, components, quantizers, blocks, scans);
        }

        private static List<EntropySegment> ReadSegments(byte[] jpeg, ref int position)
        {
            var segments = new List<EntropySegment>();
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
                if (next == 0)
                {
                    bytes.Add(0xff);
                    continue;
                }
                segments.Add(new(bytes.ToArray(), next));
                bytes.Clear();
                if (next is >= 0xd0 and <= 0xd7)
                    continue;
                Assert.True(next is 0xc4 or 0xda or 0xd9, $"Unexpected scan terminator 0x{next:x2}.");
                position -= 2; // The outer marker reader consumes the next scan/table/EOI.
                return segments;
            }
        }

        private static void DecodeScan(Scan scan, List<EntropySegment> segments, int width, int height,
            Dictionary<int, Component> components, Dictionary<int, Dictionary<int, int>> tables,
            Dictionary<BlockKey, Block> blocks, HashSet<BlockKey> acBlocks, bool progressive, int interval)
        {
            int maxH = components.Values.Max(component => component.H);
            int maxV = components.Values.Max(component => component.V);
            int mcuColumns = Ceiling(width, 8 * maxH);
            bool dcScan = scan.First == 0;
            Component firstComponent = components[scan.Selectors[0].Component];
            int acColumns = Ceiling(width * firstComponent.H, 8 * maxH);
            int units = dcScan ? mcuColumns * Ceiling(height, 8 * maxV)
                : acColumns * Ceiling(height * firstComponent.V, 8 * maxV);
            Assert.Equal(interval == 0 ? 1 : (units - 1) / interval + 1, segments.Count);
            int unit = 0;
            for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                EntropySegment segment = segments[segmentIndex];
                if (segmentIndex != segments.Count - 1)
                    Assert.Equal(0xd0 + segmentIndex % 8, segment.Terminator);
                else
                    Assert.False(segment.Terminator is >= 0xd0 and <= 0xd7); // No final restart marker.
                var reader = new EntropyBits(segment.Bytes);
                var previousDc = components.Keys.ToDictionary(component => component, _ => 0);
                int remainingEobBlocks = 0;
                int endUnit = interval == 0 ? units : Math.Min(units, unit + interval);
                for (; unit < endUnit; unit++)
                    foreach (Selector selector in scan.Selectors)
                    {
                        Component component = components[selector.Component];
                        int horizontal = dcScan ? component.H : 1;
                        int vertical = dcScan ? component.V : 1;
                        for (int y = 0; y < vertical; y++)
                            for (int x = 0; x < horizontal; x++)
                            {
                                int blockX = dcScan ? unit % mcuColumns * horizontal + x : unit % acColumns;
                                int blockY = dcScan ? unit / mcuColumns * vertical + y : unit / acColumns;
                                var key = new BlockKey(selector.Component, blockX, blockY);
                                bool dummy = blockX >= Ceiling(width * component.H, 8 * maxH)
                                    || blockY >= Ceiling(height * component.V, 8 * maxV);
                                if (dcScan && scan.High == 0)
                                {
                                    int difference = reader.SignedValue(reader.Symbol(tables[selector.Dc]));
                                    previousDc[selector.Component] += difference;
                                    int[] coefficients = new int[64];
                                    coefficients[0] = previousDc[selector.Component] << scan.Low;
                                    Assert.True(blocks.TryAdd(key, new(dummy, difference, coefficients)));
                                    if (dummy)
                                        Assert.Equal(0, difference);
                                }
                                else if (dcScan)
                                {
                                    // DC approximation uses two's complement: a refinement bit
                                    // moves a negative value toward zero, unlike AC magnitudes.
                                    blocks[key].Coefficients[0] |= reader.UnsignedValue(1) << scan.Low;
                                }
                                if (!dcScan || !progressive)
                                {
                                    Block block = blocks[key];
                                    if (scan.High == 0)
                                        ReadAc(reader, tables[selector.Ac], block.Coefficients,
                                            progressive, scan.Low, scan.EobRuns, ref remainingEobBlocks);
                                    else
                                        ReadAcRefinement(reader, tables[selector.Ac], block.Coefficients,
                                            scan.Low, scan.EobRuns, ref remainingEobBlocks);
                                    if (dummy)
                                        Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                                    else if (scan.High == 0)
                                        Assert.True(acBlocks.Add(key));
                                }
                            }
                    }
                // Every segment must contain exactly its expected coding units,
                // followed by no more than seven one-bits for byte alignment.
                Assert.Equal(0, remainingEobBlocks); // Runs cannot cross a restart or the end of a scan.
                reader.AssertOnlyPaddingRemains();
            }
            Assert.Equal(units, unit);
        }

        private static void ReadAc(EntropyBits reader, Dictionary<int, int> table, int[] coefficients,
            bool progressive, int low, List<int> eobRuns, ref int remainingEobBlocks)
        {
            if (remainingEobBlocks > 0)
            {
                Assert.True(progressive);
                remainingEobBlocks--;
                return;
            }
            for (int coefficient = 1; coefficient < 64;)
            {
                int symbol = reader.Symbol(table);
                if (symbol == 0xf0)
                {
                    coefficient += 16;
                    Assert.InRange(coefficient, 1, 64);
                    continue;
                }
                if ((symbol & 15) == 0)
                {
                    if (progressive)
                    {
                        int extraBits = symbol >> 4;
                        Assert.InRange(extraBits, 0, 14);
                        int run = (1 << extraBits) + reader.UnsignedValue(extraBits);
                        eobRuns.Add(run);
                        remainingEobBlocks = run - 1; // Includes the current block's remaining coefficients.
                    }
                    else
                        Assert.Equal(0, symbol); // Baseline allows only a single-block EOB.
                    return;
                }
                Assert.InRange(symbol & 15, 1, 10);
                coefficient += symbol >> 4;
                Assert.InRange(coefficient, 1, 63);
                int value = reader.SignedValue(symbol & 15);
                Assert.NotEqual(0, value);
                coefficients[coefficient++] = value << low;
            }
        }

        // ITU-T T.81 Annex G: runs count only coefficients still equal to zero.
        // Each already significant coefficient consumes a correction bit, while
        // a newly significant coefficient consumes a sign bit before that run.
        private static void ReadAcRefinement(EntropyBits reader, Dictionary<int, int> table,
            int[] coefficients, int low, List<int> eobRuns, ref int remainingEobBlocks)
        {
            int coefficient = 1;
            if (remainingEobBlocks == 0)
                while (coefficient < 64)
                {
                    int symbol = reader.Symbol(table), run = symbol >> 4, size = symbol & 15;
                    Assert.InRange(size, 0, 1);
                    if (size == 0 && run != 15)
                    {
                        remainingEobBlocks = (1 << run) + reader.UnsignedValue(run);
                        eobRuns.Add(remainingEobBlocks);
                        break;
                    }
                    int newValue = size == 0 ? 0 : (reader.UnsignedValue(1) == 1 ? 1 : -1) << low;
                    int zeros = size == 0 ? 16 : run;
                    bool inserted = false;
                    while (coefficient < 64)
                    {
                        if (coefficients[coefficient] != 0)
                            CorrectAc(reader, coefficients, coefficient, low);
                        else if (zeros != 0)
                        {
                            zeros--;
                            if (size == 0 && zeros == 0)
                            {
                                coefficient++;
                                break;
                            }
                        }
                        else
                        {
                            Assert.Equal(1, size);
                            coefficients[coefficient++] = newValue;
                            inserted = true;
                            break;
                        }
                        coefficient++;
                    }
                    Assert.Equal(0, zeros);
                    Assert.Equal(size == 1, inserted);
                }
            if (remainingEobBlocks > 0)
            {
                for (; coefficient < 64; coefficient++)
                    if (coefficients[coefficient] != 0)
                        CorrectAc(reader, coefficients, coefficient, low);
                remainingEobBlocks--;
            }
        }

        private static void CorrectAc(EntropyBits reader, int[] coefficients, int coefficient, int low)
        {
            int bit = 1 << low;
            Assert.Equal(0, Math.Abs(coefficients[coefficient]) & bit);
            if (reader.UnsignedValue(1) != 0)
                coefficients[coefficient] += coefficients[coefficient] > 0 ? bit : -bit;
        }

        private static int Word(byte[] bytes, int position) => bytes[position] * 256 + bytes[position + 1];
        private static int Ceiling(int value, int divisor) => (value + divisor - 1) / divisor;
    }

    private sealed class EntropyBits(byte[] bytes)
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
            throw new InvalidDataException("Invalid Huffman code in encoder output.");
        }

        public int SignedValue(int count)
        {
            Assert.InRange(count, 0, 11);
            int value = UnsignedValue(count);
            return count == 0 || value >= (1 << (count - 1)) ? value : value - (1 << count) + 1;
        }

        public int UnsignedValue(int count)
        {
            Assert.InRange(count, 0, 14);
            int value = 0;
            for (int i = 0; i < count; i++)
                value = (value << 1) | Bit();
            return value;
        }

        public void AssertOnlyPaddingRemains()
        {
            Assert.InRange(bytes.Length * 8 - position, 0, 7);
            while (position < bytes.Length * 8)
                Assert.Equal(1, Bit());
        }

        private int Bit()
        {
            Assert.True(position < bytes.Length * 8, "Unexpected end of entropy data.");
            return (bytes[position / 8] >> (7 - position++ % 8)) & 1;
        }
    }

    internal sealed class WriteOnlyStream(Stream inner, Action? onWrite = null) : Stream
    {
        public bool Closed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !Closed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);
            onWrite?.Invoke();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            inner.Write(buffer);
            onWrite?.Invoke();
        }
        public override void WriteByte(byte value)
        {
            inner.WriteByte(value);
            onWrite?.Invoke();
        }
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
