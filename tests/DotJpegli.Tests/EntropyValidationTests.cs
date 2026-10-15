using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EntropyValidationTests
{
    [Fact]
    public void CachedAcValidationIgnoresDcAndChecksEveryAcPositionAtBothBounds()
    {
        short[] block = new short[64];
        foreach (short dc in new[] { short.MinValue, short.MaxValue })
        {
            block[0] = dc;
            for (int position = 1; position < 64; position++)
            {
                foreach (short valid in new short[] { -1023, 1023 })
                {
                    block[position] = valid;
                    EntropyEncoder.ValidateAc(block);
                }
                foreach (short invalid in new short[] { short.MinValue, -1024, 1024, short.MaxValue })
                {
                    block[position] = invalid;
                    var error = Assert.Throws<ArgumentOutOfRangeException>(() => EntropyEncoder.ValidateAc(block));
                    Assert.Equal("coefficients", error.ParamName);
                }
                block[position] = 0;
            }
        }
    }

    [Fact]
    public void CachedValidationStillChecksSequentialDcDifferenceBeforeOutputOrHistogramChanges()
    {
        short[] block = new short[64];
        block[0] = 2048;
        using var output = new MemoryStream();
        var writer = new BitWriter(output);
        long[] dc = new long[256], ac = new long[256];
        int previous = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => EntropyEncoder.WriteBlock(writer, block, ref previous,
            HuffmanTable.CreateDefault(false, false), HuffmanTable.CreateDefault(true, false), coefficientsValidated: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => EntropyEncoder.AccumulateFrequencies(block, ref previous,
            dc, ac, coefficientsValidated: true));
        Assert.Equal(0, output.Length);
        Assert.Equal(0, previous);
        Assert.Equal(0, dc.Sum());
        Assert.Equal(0, ac.Sum());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidatedCachedBlocksPreserveSequentialAndProgressiveOutput(bool progressive)
    {
        var random = new Random(77891);
        short[][] blocks = Enumerable.Range(0, 64).Select(_ => Enumerable.Range(0, 64)
            .Select(index => (short)(index % 7 == 0 ? random.Next(-1023, 1024) : 0)).ToArray()).ToArray();
        using var strictOutput = new MemoryStream();
        using var cachedOutput = new MemoryStream();
        Write(strictOutput, validated: false);
        Write(cachedOutput, validated: true);
        Assert.Equal(strictOutput.ToArray(), cachedOutput.ToArray());

        void Write(Stream output, bool validated)
        {
            var writer = new BitWriter(output);
            HuffmanTable dc = HuffmanTable.CreateDefault(false, false);
            HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
            int previous = 0;
            foreach (short[] block in blocks)
            {
                if (progressive)
                {
                    ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc);
                    ProgressiveEntropyEncoder.WriteAc(writer, block, ac, successiveLow: 1, coefficientsValidated: validated);
                    ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, ac, successiveLow: 0,
                        coefficientsValidated: validated);
                }
                else
                    EntropyEncoder.WriteBlock(writer, block, ref previous, dc, ac, coefficientsValidated: validated);
            }
            writer.Flush();
        }
    }
}
