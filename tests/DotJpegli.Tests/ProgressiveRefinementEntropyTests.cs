using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ProgressiveRefinementEntropyTests
{
    [Fact]
    public void InitialDcUsesArithmeticShiftAndRetainsShiftedPredictor()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable dc = HuffmanTable.CreateDefault(false, false);
        short[] block = new short[64];
        long[] frequencies = new long[256];
        int previous = 0;
        int frequencyPrevious = 0;
        block[0] = 5;
        ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc, 1);
        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref frequencyPrevious, frequencies, 1);
        writer.Flush();
        Assert.Equal(2, previous);
        Assert.Equal(previous, frequencyPrevious);

        block[0] = -3;
        ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc, 1);
        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref frequencyPrevious, frequencies, 1);
        writer.Flush();

        // +2: category 2 (011) and 10. Difference -4: category 3 (100) and 011.
        Assert.Equal(new byte[] { 0x77, 0x8f }, stream.ToArray());
        Assert.Equal(-2, previous);
        Assert.Equal(previous, frequencyPrevious);
        Assert.Equal(1, frequencies[2]);
        Assert.Equal(1, frequencies[3]);
        Assert.Equal(2, frequencies.Sum());
    }

    [Theory]
    [InlineData(0, "AABF")]
    [InlineData(1, "999F")]
    public void DcRefinementWritesSignedBitsWithoutHuffmanTokens(int successiveLow, string expectedHex)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        block[1] = short.MaxValue; // DC refinement does not inspect AC coefficients.
        for (short value = -5; value <= 5; value++)
        {
            block[0] = value;
            ProgressiveEntropyEncoder.WriteDcRefinement(writer, block, successiveLow);
        }

        writer.Flush();
        Assert.Equal(Convert.FromHexString(expectedHex), stream.ToArray());
    }

    [Theory]
    [InlineData(-1024)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1023)]
    public void AllTenDcRefinementBitsReconstructSignedCoefficient(short coefficient)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        block[0] = coefficient;
        for (int successiveLow = 9; successiveLow >= 0; successiveLow--)
        {
            ProgressiveEntropyEncoder.WriteDcRefinement(writer, block, successiveLow);
        }

        writer.Flush();
        byte[] entropy = Unstuff(stream.ToArray());
        int lowerBits = (entropy[0] << 2) | (entropy[1] >> 6);
        int reconstructed = ((coefficient >> 10) << 10) | lowerBits;
        Assert.Equal(coefficient, reconstructed);
    }

    [Fact]
    public void InitialAcTruncatesNegativeMagnitudeTowardZero()
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[1]] = -1;
        block[JpegConstants.ZigZag[2]] = -7;
        block[JpegConstants.ZigZag[3]] = 7;
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, 2);
        Assert.Equal(1, frequencies[0x11]);
        Assert.Equal(1, frequencies[0x01]);
        Assert.Equal(1, frequencies[0]);
        Assert.Equal(3, frequencies.Sum());

        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false), 2);
        writer.Flush();
        // Run 1, -1: 1100 0. Run 0, +1: 00 1. EOB: 1010. Pad: 1111.
        Assert.Equal(Convert.FromHexString("C1AF"), stream.ToArray());
    }

    [Fact]
    public void InitialAcShiftedZeroBlocksStillAggregateEobRuns()
    {
        short[] block = new short[64];
        Array.Fill(block, (short)-1023);
        long[] frequencies = new long[256];
        int pending = 0;
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending, 10);
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending, 10);
        Assert.Equal(2, pending);
        Assert.Equal(0, frequencies.Sum());
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref pending);
        Assert.Equal(1, frequencies[0x10]);
        Assert.Equal(1, frequencies.Sum());

        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable ac = HuffmanTable.CreateOptimized(frequencies);
        ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending, 10);
        ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending, 10);
        Assert.Equal(2, pending);
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        writer.Flush();
        Assert.Equal(new byte[] { 0x3f }, stream.ToArray());
    }

    [Fact]
    public void AcRefinementWritesSignBeforeCorrectionsAndSkipsOldCoefficientsInRuns()
    {
        short[] block = new short[64];
        block[0] = short.MinValue; // AC refinement does not inspect DC.
        block[JpegConstants.ZigZag[1]] = -6;
        block[JpegConstants.ZigZag[2]] = -1;
        block[JpegConstants.ZigZag[3]] = 4;
        block[JpegConstants.ZigZag[4]] = -2;
        block[JpegConstants.ZigZag[5]] = 6;
        block[JpegConstants.ZigZag[7]] = 2;
        block[JpegConstants.ZigZag[63]] = -4;

        // The insignificant -1 contributes a zero, while -6 and +4 contribute 10.
        CheckAcRefinement(block, 1, (0x11, "010"), (0x11, "11"), (0x00, "0"));
    }

    [Fact]
    public void ZrlCorrectionBitsStopAtSixteenthStillZeroCoefficient()
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[1]] = 6;
        // Positions 2..17 supply exactly sixteen zeros.
        block[JpegConstants.ZigZag[18]] = -4;
        block[JpegConstants.ZigZag[19]] = -2;
        block[JpegConstants.ZigZag[63]] = 6;

        // Position 18 belongs to the newly significant coefficient's correction
        // field, after its sign, rather than to the preceding ZRL.
        CheckAcRefinement(block, 1, (0xf0, "1"), (0x01, "00"), (0x00, "1"));
    }

    [Fact]
    public void ZrlSkipsInterspersedOldCoefficientsWhenCountingSixteenZeros()
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[1]] = -6;
        block[JpegConstants.ZigZag[8]] = 4;
        block[JpegConstants.ZigZag[16]] = 6;
        block[JpegConstants.ZigZag[20]] = -2;

        // Sixteen zeros plus three old values precede the new negative value.
        CheckAcRefinement(block, 1, (0xf0, "101"), (0x01, "0"), (0x00, ""));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    public void NewlySignificantFinalCoefficientNeedsNoEob(int successiveLow)
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[63]] = (short)-(1 << successiveLow);
        CheckAcRefinement(block, successiveLow,
            (0xf0, ""), (0xf0, ""), (0xf0, ""), (0xe1, "0"));
    }

    [Fact]
    public void TrailingOldCoefficientNeedsEobEvenWithoutTrailingZeros()
    {
        short[] block = new short[64];
        for (int position = 1; position <= 62; position++)
        {
            block[JpegConstants.ZigZag[position]] = 1;
        }
        block[JpegConstants.ZigZag[63]] = -3;
        var tokens = Enumerable.Repeat((0x01, "1"), 62).Append((0x00, "1")).ToArray();
        CheckAcRefinement(block, 0, tokens);
    }

    [Fact]
    public void AllOldCoefficientsUseOneEobWithSixtyThreeCorrectionBits()
    {
        short[] block = new short[64];
        Array.Fill(block, (short)-3);
        CheckAcRefinement(block, 0, (0x00, new string('1', 63)));
    }

    [Fact]
    public void NoNewCoefficientsAvoidTrailingZrlAndUseOneEobPerBlock()
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[63]] = -6;
        CheckAcRefinement(block, 1, (0x00, "1"));
        Array.Clear(block);
        CheckAcRefinement(block, 9, (0x00, ""));

        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 9);
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 9);
        Assert.Equal(2, frequencies[0]);
        Assert.Equal(2, frequencies.Sum());
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable ac = HuffmanTable.CreateOptimized(frequencies);
        ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, ac, 9);
        ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, ac, 9);
        writer.Flush();
        Assert.Equal(new byte[] { 0x3f }, stream.ToArray());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidInitialBitPositionPreservesOutputPredictorAndPendingEob(int successiveLow)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        long[] frequencies = new long[256];
        int previous = 7;
        int pending = 7;
        HuffmanTable dc = HuffmanTable.CreateDefault(false, false);
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, successiveLow));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(7, previous);
        Assert.Equal(7, pending);
        Assert.Equal(0, frequencies.Sum());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidRefinementBitPositionDoesNotWrite(int successiveLow)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        long[] frequencies = new long[256];
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteDcRefinement(writer, block, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, ac, successiveLow));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, successiveLow));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
    }

    [Theory]
    [InlineData(-1024)]
    [InlineData(1024)]
    public void InvalidFinalAcCoefficientIsRejectedBeforeAnyRefinementTokens(short value)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        block[1] = 1;
        block[JpegConstants.ZigZag[63]] = value;
        long[] frequencies = new long[256];
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, HuffmanTable.CreateDefault(true, false), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 0));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(65)]
    public void RefinementRequiresExactlyOneBlock(int length)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[length];
        long[] frequencies = new long[256];
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.WriteDcRefinement(writer, block, 0));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.WriteAcRefinement(writer, block, HuffmanTable.CreateDefault(true, false), 0));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, 0));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
    }

    private static void CheckAcRefinement(short[] block, int successiveLow, params (int Symbol, string Bits)[] tokens)
    {
        long[] expectedFrequencies = new long[256];
        foreach ((int symbol, _) in tokens)
        {
            expectedFrequencies[symbol]++;
        }
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, successiveLow);
        Assert.Equal(expectedFrequencies, frequencies);

        HuffmanTable[] tables = [HuffmanTable.CreateDefault(true, false),
            HuffmanTable.CreateDefault(true, true), HuffmanTable.CreateOptimized(expectedFrequencies)];
        foreach (HuffmanTable ac in tables)
        {
            using var expected = new MemoryStream();
            var expectedWriter = new BitWriter(expected);
            foreach ((int symbol, string bits) in tokens)
            {
                ac.WriteSymbol(expectedWriter, symbol);
                foreach (char bit in bits)
                {
                    expectedWriter.WriteBits((uint)(bit - '0'), 1);
                }
            }
            expectedWriter.Flush();

            using var actual = new MemoryStream();
            var actualWriter = new BitWriter(actual);
            ProgressiveEntropyEncoder.WriteAcRefinement(actualWriter, block, ac, successiveLow);
            actualWriter.Flush();
            Assert.Equal(expected.ToArray(), actual.ToArray());
        }
    }

    private static byte[] Unstuff(byte[] bytes)
    {
        var result = new List<byte>();
        for (int index = 0; index < bytes.Length; index++)
        {
            result.Add(bytes[index]);
            if (bytes[index] == 0xff)
            {
                Assert.Equal(0, bytes[++index]);
            }
        }
        return result.ToArray();
    }
}
