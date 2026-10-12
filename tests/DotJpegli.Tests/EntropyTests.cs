using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EntropyTests
{
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
