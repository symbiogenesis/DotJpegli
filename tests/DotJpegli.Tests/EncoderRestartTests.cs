using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderRestartTests
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
    public void RestartSegmentsPreserveRealCoefficientsAndDecodedPixels(
        PixelLayout layout, ChromaSubsampling sampling, bool optimize, bool adaptive)
    {
        // At least 10 MCUs in every mode: interval 1 wraps RST7 to RST0.
        // Both odd edges also exercise wholly padded luma blocks in sampled MCUs.
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = sampling,
            OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive
        };
        Assert.Equal(0, options.RestartInterval);
        byte[] uninterrupted = Encode(pixels, width, height, layout, options);
        Assert.Equal(uninterrupted, Encode(pixels, width, height, layout, options with { RestartInterval = 0 }));
        BaselineScan reference = BaselineScan.Read(uninterrupted, 0);
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        byte[] expectedPixels = ImageResult.FromMemory(uninterrupted, channels).Data;

        foreach (int interval in new[] { 1, 2, 7, reference.McuCount, reference.McuCount + 1, 65535 })
        {
            byte[] restarted = Encode(pixels, width, height, layout, options with { RestartInterval = interval });
            BaselineScan actual = BaselineScan.Read(restarted, interval);
            Assert.Equal(reference.McuCount, actual.McuCount);
            Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
            for (int i = 0; i < reference.Blocks.Count; i++)
            {
                Block expected = reference.Blocks[i], block = actual.Blocks[i];
                Assert.Equal((expected.Component, expected.X, expected.Y), (block.Component, block.X, block.Y));
                if (block.IsDummy)
                {
                    Assert.Equal(0, block.DcDifference);
                    Assert.Equal(0, block.FirstAcSymbol); // Immediate EOB, including after a reset.
                    Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
                }
                else
                {
                    // Reconstructing DC with a fresh predictor at every segment independently
                    // catches failure to reset any of Y, Cb or Cr in the encoder.
                    Assert.Equal(expected.Coefficients, block.Coefficients);
                }
            }
            ImageResult decoded = ImageResult.FromMemory(restarted, channels);
            Assert.Equal(width, decoded.Width);
            Assert.Equal(height, decoded.Height);
            Assert.Equal(expectedPixels, decoded.Data);
        }
    }

    [Theory]
    [InlineData(PixelLayout.Gray8, ChromaSubsampling.None)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.None)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.HalfHorizontal)]
    [InlineData(PixelLayout.Rgb24, ChromaSubsampling.Quarter)]
    public void OptimizedStatisticsIncludeDcCategoriesIntroducedByPredictorResets(
        PixelLayout layout, ChromaSubsampling sampling)
    {
        int mcuWidth = layout == PixelLayout.Gray8 || sampling == ChromaSubsampling.None ? 8 : 16;
        int mcuHeight = layout != PixelLayout.Gray8 && sampling == ChromaSubsampling.Quarter ? 16 : 8;
        int channels = layout == PixelLayout.Gray8 ? 1 : 3;
        byte[] pixels = new byte[4 * mcuWidth * mcuHeight * channels];
        for (int y = 0; y < mcuHeight; y++)
            for (int x = 0; x < 4 * mcuWidth; x++)
                for (int channel = 0; channel < channels; channel++)
                    pixels[(y * 4 * mcuWidth + x) * channels + channel] = (byte)(128 + x / mcuWidth * (channel + 1));

        foreach (bool adaptive in new[] { false, true })
        {
            // Small ordinary deltas all share a category; absolute DC values after
            // each reset require categories absent from an uninterrupted first pass.
            var options = new JpegliEncoderOptions
            {
                Quality = 100, Subsampling = sampling, AdaptiveQuantization = adaptive,
                OptimizeHuffmanTables = false, RestartInterval = 1
            };
            BaselineScan standard = BaselineScan.Read(Encode(pixels, 4 * mcuWidth, mcuHeight, layout, options), 1);
            BaselineScan optimized = BaselineScan.Read(Encode(pixels, 4 * mcuWidth, mcuHeight, layout,
                options with { OptimizeHuffmanTables = true }), 1);
            Assert.Equal(standard.Blocks.Count, optimized.Blocks.Count);
            for (int i = 0; i < standard.Blocks.Count; i++)
                Assert.Equal(standard.Blocks[i].Coefficients, optimized.Blocks[i].Coefficients);
            foreach (int component in optimized.Blocks.Select(block => block.Component).Distinct())
                Assert.True(optimized.Blocks.Where(block => block.Component == component)
                    .Select(block => block.Coefficients[0]).Distinct().Count() > 2);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StreamEncodingMatchesArrayEncodingWithoutSeekingOrClosing(bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { RestartInterval = 2, OptimizeHuffmanTables = optimize };
        byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(output, pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        BaselineScan.Read(destination.ToArray(), 2);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidRestartIntervalWritesNoOutput(int interval)
    {
        using var output = new MemoryStream();
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(output, new byte[3], 1, 1,
                options: new() { RestartInterval = interval }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.RestartInterval), exception.ParamName);
        Assert.Equal(0, output.Length);
    }

    private static byte[] Encode(byte[] pixels, int width, int height, PixelLayout layout, JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static byte[] Pattern(int width, int height, PixelLayout layout)
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

    private sealed record Block(int Component, int X, int Y, bool IsDummy, int DcDifference,
        int FirstAcSymbol, int[] Coefficients);

    private sealed record EntropySegment(byte[] Bytes, int Terminator);

    // Independent baseline parser: emitted DHTs define the decoder and raw marker
    // boundaries define the segments. No encoder entropy or sampling helpers are used.
    private sealed record BaselineScan(int McuCount, List<Block> Blocks)
    {
        public static BaselineScan Read(byte[] jpeg, int expectedInterval)
        {
            Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
            int position = 2, width = 0, height = 0, driCount = 0;
            var components = new Dictionary<int, (int H, int V)>();
            var tables = new Dictionary<int, Dictionary<int, int>>();
            var selectors = new List<(int Component, int Dc, int Ac)>();
            while (true)
            {
                Assert.Equal(0xff, jpeg[position++]);
                int marker = jpeg[position++];
                int length = Word(jpeg, position), end = position + length;
                Assert.InRange(length, 2, jpeg.Length - position);
                position += 2;
                if (marker == 0xdd)
                {
                    driCount++;
                    Assert.Equal(4, length);
                    Assert.Equal(expectedInterval, Word(jpeg, position));
                }
                else if (marker == 0xc0)
                {
                    Assert.Equal(8, jpeg[position++]);
                    height = Word(jpeg, position);
                    width = Word(jpeg, position + 2);
                    position += 4;
                    int count = jpeg[position++];
                    for (int i = 0; i < count; i++)
                    {
                        int component = jpeg[position++], factors = jpeg[position++];
                        components.Add(component, (factors >> 4, factors & 15));
                        position++; // Quantizer selector.
                    }
                    Assert.Equal(end, position);
                }
                else if (marker == 0xc4)
                {
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
                        tables.Add(selector, codes);
                    }
                    Assert.Equal(end, position);
                }
                else if (marker == 0xda)
                {
                    int count = jpeg[position++];
                    Assert.Equal(components.Count, count);
                    for (int i = 0; i < count; i++)
                    {
                        int component = jpeg[position++], selector = jpeg[position++];
                        selectors.Add((component, selector >> 4, 0x10 | (selector & 15)));
                    }
                    Assert.Equal(new byte[] { 0, 63, 0 }, jpeg[position..end]);
                    position = end;
                    break;
                }
                position = end;
            }
            Assert.Equal(expectedInterval == 0 ? 0 : 1, driCount);

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
                    bytes.Add(0xff); // Stuffed data, never a marker.
                    continue;
                }
                Assert.True(next == 0xd9 || next is >= 0xd0 and <= 0xd7, $"Unexpected scan marker 0x{next:x2}.");
                segments.Add(new(bytes.ToArray(), next));
                bytes.Clear();
                if (next == 0xd9)
                {
                    Assert.Equal(jpeg.Length, position);
                    break;
                }
            }

            int maxH = components.Values.Max(component => component.H);
            int maxV = components.Values.Max(component => component.V);
            int columns = (width + 8 * maxH - 1) / (8 * maxH);
            int rows = (height + 8 * maxV - 1) / (8 * maxV);
            int mcuCount = columns * rows;
            int expectedSegments = expectedInterval == 0 ? 1 : (mcuCount - 1) / expectedInterval + 1;
            Assert.Equal(expectedSegments, segments.Count); // No trailing RST, even at an exact interval.
            var blocks = new List<Block>();
            int mcu = 0;
            for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                EntropySegment segment = segments[segmentIndex];
                Assert.Equal(segmentIndex == segments.Count - 1 ? 0xd9 : 0xd0 + segmentIndex % 8, segment.Terminator);
                var reader = new EntropyBits(segment.Bytes);
                var previousDc = components.Keys.ToDictionary(component => component, _ => 0);
                int endMcu = expectedInterval == 0 ? mcuCount : Math.Min(mcuCount, mcu + expectedInterval);
                for (; mcu < endMcu; mcu++)
                    foreach ((int component, int dcTable, int acTable) in selectors)
                    {
                        (int horizontal, int vertical) = components[component];
                        for (int y = 0; y < vertical; y++)
                            for (int x = 0; x < horizontal; x++)
                            {
                                int difference = reader.SignedValue(reader.Symbol(tables[dcTable]));
                                previousDc[component] += difference;
                                int[] coefficients = new int[64];
                                coefficients[0] = previousDc[component];
                                int firstAc = -1;
                                for (int coefficient = 1; coefficient < 64;)
                                {
                                    int symbol = reader.Symbol(tables[acTable]);
                                    if (firstAc < 0)
                                        firstAc = symbol;
                                    if (symbol == 0)
                                        break;
                                    if (symbol == 0xf0)
                                    {
                                        coefficient += 16;
                                        Assert.InRange(coefficient, 1, 64);
                                        continue;
                                    }
                                    coefficient += symbol >> 4;
                                    Assert.InRange(coefficient, 1, 63);
                                    Assert.InRange(symbol & 15, 1, 10);
                                    int value = reader.SignedValue(symbol & 15);
                                    Assert.NotEqual(0, value);
                                    coefficients[coefficient++] = value;
                                }
                                int left = mcu % columns * 8 * horizontal + 8 * x;
                                int top = mcu / columns * 8 * vertical + 8 * y;
                                bool dummy = left >= (width * horizontal + maxH - 1) / maxH
                                    || top >= (height * vertical + maxV - 1) / maxV;
                                blocks.Add(new(component, left, top, dummy, difference, firstAc, coefficients));
                            }
                    }
                // A decoder that merely skips to markers could overlook extra MCUs,
                // partial blocks or invalid byte-alignment padding at a boundary.
                reader.AssertOnlyPaddingRemains();
            }
            Assert.Equal(mcuCount, mcu);
            return new(mcuCount, blocks);
        }

        private static int Word(byte[] bytes, int position) => bytes[position] * 256 + bytes[position + 1];
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
            int value = 0;
            for (int i = 0; i < count; i++)
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
            Assert.True(position < bytes.Length * 8, "Unexpected end of entropy data.");
            return (bytes[position / 8] >> (7 - position++ % 8)) & 1;
        }
    }

    private sealed class WriteOnlyStream(Stream inner) : Stream
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
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
        public override void WriteByte(byte value) => inner.WriteByte(value);
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
