using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderSamplingTests
{
    [Theory]
    [InlineData(ChromaSubsampling.HalfHorizontal, false, false)]
    [InlineData(ChromaSubsampling.HalfHorizontal, false, true)]
    [InlineData(ChromaSubsampling.HalfHorizontal, true, false)]
    [InlineData(ChromaSubsampling.HalfHorizontal, true, true)]
    [InlineData(ChromaSubsampling.Quarter, false, false)]
    [InlineData(ChromaSubsampling.Quarter, false, true)]
    [InlineData(ChromaSubsampling.Quarter, true, false)]
    [InlineData(ChromaSubsampling.Quarter, true, true)]
    public void DummyLumaBlocksEmitZeroDifferenceAndEobWithoutChangingPredictor(
        ChromaSubsampling sampling, bool optimize, bool adaptive)
    {
        foreach ((int width, int height) in new[] { (17, 17), (1, 1), (8, 8), (9, 9) })
        {
            // Vary both edges so replicated dummy blocks have nonzero AC and/or
            // a different DC. Equal RGB channels also give an independent Y DC oracle.
            byte[] pixels = NeutralPattern(width, height);
            BaselineScan scan = BaselineScan.Read(Encode(pixels, width, height, sampling, optimize, adaptive));
            int previousRealDc = 0;
            int dummyCount = 0;
            foreach (Block block in scan.Blocks.Where(block => block.Component == 1))
            {
                if (block.X >= width || block.Y >= height)
                {
                    dummyCount++;
                    Assert.Equal(0, block.DcDifference);
                    Assert.Equal(0, block.FirstAcSymbol); // Immediate EOB, without even a ZRL.
                    Assert.Equal(0, block.NonzeroAcCount);
                    Assert.Equal(previousRealDc, block.Dc);
                    continue;
                }

                double sampleSum = 0;
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        sampleSum += pixels[(Math.Min(block.Y + y, height - 1) * width
                            + Math.Min(block.X + x, width - 1)) * 3] - 128;
                // JPEG's orthonormal DC is sum / 8. AQ does not alter DC rounding.
                // Permit either neighbor at an exact half tie after float RGB conversion.
                double expectedDc = sampleSum / (8 * scan.LumaDcQuantizer);
                Assert.InRange(Math.Abs(block.Dc - expectedDc), 0, 0.501);
                Assert.Equal(block.Dc - previousRealDc, block.DcDifference);
                previousRealDc = block.Dc;
            }

            int vertical = sampling == ChromaSubsampling.Quarter ? 2 : 1;
            int expectedDummyCount = ((width + 15) / 16) * ((height + 8 * vertical - 1) / (8 * vertical))
                * 2 * vertical - ((width + 7) / 8) * ((height + 7) / 8);
            Assert.Equal(expectedDummyCount, dummyCount);
        }
    }

    [Theory]
    [InlineData(ChromaSubsampling.HalfHorizontal, false)]
    [InlineData(ChromaSubsampling.HalfHorizontal, true)]
    [InlineData(ChromaSubsampling.Quarter, false)]
    [InlineData(ChromaSubsampling.Quarter, true)]
    public void NarrowAndTallImagesDecodeIdenticallyWithBothHuffmanPasses(
        ChromaSubsampling sampling, bool adaptive)
    {
        foreach ((int width, int height) in new[] { (1, 33), (33, 1), (9, 33), (33, 9) })
        {
            byte[] pixels = TestImages.Gradient(width, height);
            ImageResult standard = ImageResult.FromMemory(
                Encode(pixels, width, height, sampling, false, adaptive), ColorComponents.RedGreenBlue);
            ImageResult optimized = ImageResult.FromMemory(
                Encode(pixels, width, height, sampling, true, adaptive), ColorComponents.RedGreenBlue);
            Assert.Equal(width, optimized.Width);
            Assert.Equal(height, optimized.Height);
            Assert.Equal(standard.Data, optimized.Data);
        }
    }

    private static byte[] Encode(byte[] pixels, int width, int height,
        ChromaSubsampling sampling, bool optimize, bool adaptive) =>
        JpegliEncoder.Encode(pixels, width, height, options: new()
        {
            Quality = 90, Subsampling = sampling,
            OptimizeHuffmanTables = optimize, AdaptiveQuantization = adaptive
        }, cancellationToken: TestContext.Current.CancellationToken);

    private static byte[] NeutralPattern(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                byte value = (byte)(32 + (11 * x + 19 * y + 7 * (x / 8) + 13 * (y / 8)) % 192);
                Array.Fill(pixels, value, (y * width + x) * 3, 3);
            }
        return pixels;
    }

    private sealed record Block(int Component, int X, int Y, int DcDifference, int Dc,
        int FirstAcSymbol, int NonzeroAcCount);

    // Test-only baseline entropy reader. It uses the emitted marker definitions,
    // rather than the encoder's Huffman, entropy, quantization or DCT helpers.
    private sealed record BaselineScan(int LumaDcQuantizer, List<Block> Blocks)
    {
        public static BaselineScan Read(byte[] jpeg)
        {
            Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
            int position = 2, width = 0, height = 0;
            var components = new Dictionary<int, (int H, int V, int Quantizer)>();
            var quantizers = new Dictionary<int, int>();
            var tables = new Dictionary<int, Dictionary<int, int>>();
            var selectors = new List<(int Component, int Dc, int Ac)>();
            while (true)
            {
                Assert.Equal(0xff, jpeg[position++]);
                int marker = jpeg[position++];
                int length = Word(jpeg, position);
                int end = position + length;
                position += 2;
                if (marker == 0xdb)
                {
                    while (position < end)
                    {
                        int selector = jpeg[position++];
                        Assert.Equal(0, selector >> 4); // Eight-bit baseline DQT.
                        quantizers[selector & 15] = jpeg[position];
                        position += 64;
                    }
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
                        components[component] = (factors >> 4, factors & 15, jpeg[position++]);
                    }
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
                }
                else if (marker == 0xda)
                {
                    int count = jpeg[position++];
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

            var entropy = new List<byte>();
            while (true)
            {
                byte value = jpeg[position++];
                if (value == 0xff)
                {
                    int next = jpeg[position++];
                    if (next != 0)
                    {
                        Assert.Equal(0xd9, next);
                        Assert.Equal(jpeg.Length, position);
                        break;
                    }
                }
                entropy.Add(value);
            }

            var reader = new EntropyBits(entropy.ToArray());
            var blocks = new List<Block>();
            var previousDc = components.Keys.ToDictionary(component => component, _ => 0);
            int maxH = components.Values.Max(component => component.H);
            int maxV = components.Values.Max(component => component.V);
            for (int top = 0; top < height; top += 8 * maxV)
                for (int left = 0; left < width; left += 8 * maxH)
                    foreach ((int component, int dcTable, int acTable) in selectors)
                    {
                        (int horizontal, int vertical, _) = components[component];
                        for (int y = 0; y < vertical; y++)
                            for (int x = 0; x < horizontal; x++)
                            {
                                int difference = reader.SignedValue(reader.Symbol(tables[dcTable]));
                                previousDc[component] += difference;
                                int firstAc = -1, nonzeroAc = 0;
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
                                    Assert.NotEqual(0, reader.SignedValue(symbol & 15));
                                    nonzeroAc++;
                                    coefficient++;
                                }
                                blocks.Add(new(component, left * horizontal / maxH + x * 8,
                                    top * vertical / maxV + y * 8, difference,
                                    previousDc[component], firstAc, nonzeroAc));
                            }
                    }
            reader.AssertOnlyPaddingRemains();
            return new(quantizers[components[1].Quantizer], blocks);
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
}
