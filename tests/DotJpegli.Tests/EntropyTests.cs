using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EntropyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryBaselineAmplitudePreservesItsCategoryAndComplementSuffix(bool ordered)
    {
        HuffmanTable dc = HuffmanTable.CreateDefault(false, false);
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        using var expected = new MemoryStream();
        using var actual = new MemoryStream();
        var expectedWriter = new BitWriter(expected, buffered: true);
        var actualWriter = new BitWriter(actual, buffered: true);
        short[] block = new short[64];
        for (int value = -2047; value <= 2047; value++)
        {
            block[0] = (short)value;
            block[1] = (short)(value / 2);
            int previous = 0;
            EntropyEncoder.WriteBlock(actualWriter, block, ref previous, dc, ac, zigZagOrdered: ordered);
            WriteAmplitude(expectedWriter, dc, value);
            if (block[1] != 0)
                WriteAmplitude(expectedWriter, ac, block[1]);
            ac.WriteSymbol(expectedWriter, 0);
        }
        expectedWriter.Flush();
        actualWriter.Flush();
        Assert.Equal(expected.ToArray(), actual.ToArray());

        static void WriteAmplitude(BitWriter writer, HuffmanTable table, int value)
        {
            int magnitude = Math.Abs(value);
            int category = 0;
            while (magnitude != 0)
            {
                category++;
                magnitude >>= 1;
            }
            uint bits = (uint)(value < 0 ? value - 1 : value) & ((1U << category) - 1);
            table.WriteSymbolWithBits(writer, category, bits, category);
        }
    }

    [Fact]
    public void BitsStuffFfAndPadWithOnes()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        writer.WriteBits(0xff, 8);
        writer.WriteBits(1, 1);
        writer.Flush();
        Assert.Equal(new byte[] { 0xff, 0, 0xff, 0 }, stream.ToArray());
    }

    [Fact]
    public void RestartMarkersPadAndStuffEntropyButRemainUnstuffed()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        writer.WriteBits(1, 1); // Padding creates FF, which must be stuffed.
        writer.WriteRestartMarker(0);
        writer.WriteBits(0, 1); // Next segment starts with a fresh bit buffer.
        writer.WriteRestartMarker(7);
        writer.WriteBits(0x42, 8);
        writer.WriteRestartMarker(0); // Aligned segment needs no padding byte.
        writer.Flush();
        Assert.Equal(new byte[] { 0xff, 0, 0xff, 0xd0, 0x7f, 0xff, 0xd7, 0x42, 0xff, 0xd0 },
            stream.ToArray());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void InvalidRestartMarkerDoesNotFlushPendingBits(int index)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        writer.WriteBits(0, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteRestartMarker(index));
        Assert.Equal(0, stream.Length);
        writer.Flush();
        Assert.Equal(new byte[] { 0x7f }, stream.ToArray());
    }

    [Fact]
    public void ZeroBlockHasKnownAnnexKEncoding()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int previous = 0;
        EntropyEncoder.WriteBlock(writer, new short[64], ref previous,
            HuffmanTable.CreateDefault(false, false), HuffmanTable.CreateDefault(true, false));
        writer.Flush();
        // DC category zero = 00; EOB = 1010; pad = 11.
        Assert.Equal(new byte[] { 0x2b }, stream.ToArray());
    }

    [Theory]
    [InlineData(-1023)]
    [InlineData(1023)]
    public void FinalAcCoefficientIncludesCategoryTenWithoutTrailingEob(short value)
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[63]] = value;
        long[] dc = new long[256];
        long[] ac = new long[256];
        int previous = 0;

        EntropyEncoder.AccumulateFrequencies(block, ref previous, dc, ac);

        Assert.Equal(1, dc[0]);
        Assert.Equal(3, ac[0xf0]);
        Assert.Equal(1, ac[0xea]);
        Assert.Equal(0, ac[0]);
        Assert.Equal(4, ac.Sum());
    }

    [Theory]
    [InlineData(1, -1024)]
    [InlineData(16, short.MinValue)]
    [InlineData(32, short.MaxValue)]
    [InlineData(63, 1024)]
    public void InvalidAcCoefficientPreservesOutputPredictorAndFrequencies(int position, short value)
    {
        short[] block = new short[64];
        block[0] = 5;
        block[1] = 1;
        block[position] = value;
        long[] dc = new long[256];
        long[] ac = new long[256];
        int previous = 3;
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EntropyEncoder.WriteBlock(writer, block, ref previous,
                HuffmanTable.CreateDefault(false, false), HuffmanTable.CreateDefault(true, false)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EntropyEncoder.AccumulateFrequencies(block, ref previous, dc, ac));

        writer.Flush();
        Assert.Equal(0, stream.Length);
        Assert.Equal(3, previous);
        Assert.Equal(0, dc.Sum());
        Assert.Equal(0, ac.Sum());
    }

    [Fact]
    public void OptimizerProducesValidBoundedCanonicalTables()
    {
        long[] frequencies = new long[256];
        var random = new Random(42);
        for (int i = 0; i < frequencies.Length; i++)
            frequencies[i] = i % 7 == 0 ? 0 : random.Next(1, 10000);
        HuffmanTable table = HuffmanTable.CreateOptimized(frequencies);
        Assert.Equal(16, table.Counts.Length);
        Assert.Equal(frequencies.Count(value => value != 0), table.Symbols.Length);
        Assert.Equal(table.Symbols.Length, table.Symbols.Distinct().Count());
        uint code = 0;
        for (int length = 1; length <= 16; length++)
        {
            Assert.True(code + table.Counts[length - 1] < (1U << length));
            code = (code + table.Counts[length - 1]) << 1;
        }
    }
}
