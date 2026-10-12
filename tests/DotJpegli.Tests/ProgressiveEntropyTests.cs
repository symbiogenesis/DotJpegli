using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ProgressiveEntropyTests
{
    [Fact]
    public void InitialDcScanUsesPredictorAndAnnexKCodesWithoutAcTokens()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable dc = HuffmanTable.CreateDefault(false, false);
        short[] block = new short[64];
        int previous = 0;

        ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc);
        writer.Flush();
        block[0] = 5;
        ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc);
        writer.Flush();
        Assert.Equal(5, previous);
        block[0] = -3;
        ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous, dc);
        writer.Flush();

        // Category 0: 00. Difference +5: 100 101. Difference -8: 101 0111.
        // Each segment is padded independently with ones.
        Assert.Equal(new byte[] { 0x3f, 0x97, 0xaf }, stream.ToArray());
        Assert.Equal(-3, previous);
    }

    [Fact]
    public void InitialAcScanUsesAnnexKCodesWithoutDcTokens()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        short[] block = new short[64];

        ProgressiveEntropyEncoder.WriteAc(writer, block, ac);
        writer.Flush();
        block[1] = -3;
        ProgressiveEntropyEncoder.WriteAc(writer, block, ac);
        writer.Flush();

        // Empty AC band: EOB 1010. First AC = -3: 01 00, then EOB 1010.
        Assert.Equal(new byte[] { 0xaf, 0x4a }, stream.ToArray());
    }

    [Fact]
    public void AcScanEmitsZrlBeforeNonzeroAndStuffsFfBytes()
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        block[JpegConstants.ZigZag[17]] = 1;

        ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false));
        writer.Flush();

        // Sixteen zeros: ZRL 11111111001; +1: 00 1; EOB: 1010; pad: 111111.
        Assert.Equal(new byte[] { 0xff, 0, 0x26, 0xbf }, stream.ToArray());
    }

    [Fact]
    public void DcFrequencyPassTracksDifferencesAndIgnoresAcCoefficients()
    {
        short[] block = new short[64];
        block[1] = short.MaxValue;
        long[] frequencies = new long[256];
        int previous = 0;

        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies);
        block[0] = 5;
        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies);
        block[0] = -3;
        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies);

        Assert.Equal(-3, previous);
        Assert.Equal(1, frequencies[0]);
        Assert.Equal(1, frequencies[3]);
        Assert.Equal(1, frequencies[4]);
        Assert.Equal(3, frequencies.Sum());
    }

    [Fact]
    public void AcFrequencyPassUsesZigZagRunsAndIgnoresDcCoefficient()
    {
        short[] block = new short[64];
        block[0] = short.MinValue;
        block[JpegConstants.ZigZag[17]] = -3;
        block[JpegConstants.ZigZag[20]] = 7;
        long[] frequencies = new long[256];

        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies);

        Assert.Equal(1, frequencies[0xf0]);
        Assert.Equal(1, frequencies[0x02]);
        Assert.Equal(1, frequencies[0x23]);
        Assert.Equal(1, frequencies[0]);
        Assert.Equal(4, frequencies.Sum());
    }

    [Theory]
    [InlineData(-1023)]
    [InlineData(1023)]
    public void NonzeroFinalAcCoefficientHasThreeZrlTokensAndNoEob(short value)
    {
        short[] block = new short[64];
        block[JpegConstants.ZigZag[63]] = value;
        long[] frequencies = new long[256];

        ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies);

        Assert.Equal(3, frequencies[0xf0]);
        Assert.Equal(1, frequencies[0xea]);
        Assert.Equal(0, frequencies[0]);
        Assert.Equal(4, frequencies.Sum());
    }

    [Fact]
    public void AllZeroAcBlockUsesSingleEobInsteadOfTrailingZrl()
    {
        long[] frequencies = new long[256];
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(new short[64], frequencies);
        Assert.Equal(1, frequencies[0]);
        Assert.Equal(1, frequencies.Sum());
    }

    [Theory]
    [InlineData(1, 0x00, "7F")]
    [InlineData(2, 0x10, "3F")]
    [InlineData(3, 0x10, "7F")]
    [InlineData(4, 0x20, "1F")]
    [InlineData(7, 0x20, "7F")]
    [InlineData(8, 0x30, "0F")]
    [InlineData(16, 0x40, "07")]
    [InlineData(32, 0x50, "03")]
    [InlineData(64, 0x60, "01")]
    [InlineData(128, 0x70, "00")]
    [InlineData(255, 0x70, "7F")]
    [InlineData(256, 0x80, "007F")]
    [InlineData(511, 0x80, "7FFF00")]
    [InlineData(512, 0x90, "003F")]
    [InlineData(1024, 0xa0, "001F")]
    [InlineData(2048, 0xb0, "000F")]
    [InlineData(4096, 0xc0, "0007")]
    [InlineData(8192, 0xd0, "0003")]
    [InlineData(16384, 0xe0, "0001")]
    [InlineData(32766, 0xe0, "7FFD")]
    [InlineData(32767, 0xe0, "7FFF00")]
    public void EobRunUsesAnnexGSymbolAndAdditionalBits(int blocks, int symbol, string expectedHex)
    {
        long[] tableFrequencies = new long[256];
        tableFrequencies[symbol] = 1;
        HuffmanTable ac = HuffmanTable.CreateOptimized(tableFrequencies);
        Assert.Equal(new byte[] { (byte)symbol }, ac.Symbols);
        Assert.Equal(1, ac.Counts[0]); // The only symbol gets the one-bit code 0.
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        long[] frequencies = new long[256];
        int pending = 0;
        int frequencyPending = 0;

        for (int index = 0; index < blocks; index++)
        {
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending);
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref frequencyPending);
        }

        Assert.Equal(blocks == 32767 ? 0 : blocks, pending);
        Assert.Equal(pending, frequencyPending);
        Assert.Equal(blocks == 32767 ? 1 : 0, frequencies.Sum());
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref frequencyPending);
        writer.Flush();

        Assert.Equal(0, pending);
        Assert.Equal(0, frequencyPending);
        Assert.Equal(1, frequencies[symbol]);
        Assert.Equal(1, frequencies.Sum());
        Assert.Equal(Convert.FromHexString(expectedHex), stream.ToArray());
    }

    [Fact]
    public void PartialTailJoinsZeroBlocksAndFlushesBeforeNextBlocksZrl()
    {
        short[] partialBlock = new short[64];
        partialBlock[1] = -3;
        short[] zeroBlock = new short[64];
        short[] finalBlock = new short[64];
        finalBlock[JpegConstants.ZigZag[63]] = 1;
        long[] frequencies = new long[256];
        int frequencyPending = 0;
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(partialBlock, frequencies, ref frequencyPending);
        Assert.Equal(1, frequencyPending);
        Assert.Equal(1, frequencies[0x02]);
        Assert.Equal(1, frequencies.Sum());
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(zeroBlock, frequencies, ref frequencyPending);
        Assert.Equal(2, frequencyPending);
        Assert.Equal(1, frequencies.Sum());
        ProgressiveEntropyEncoder.AccumulateAcFrequencies(finalBlock, frequencies, ref frequencyPending);
        Assert.Equal(0, frequencyPending);
        Assert.Equal(1, frequencies[0x10]);
        Assert.Equal(3, frequencies[0xf0]);
        Assert.Equal(1, frequencies[0xe1]);
        Assert.Equal(6, frequencies.Sum());

        HuffmanTable ac = HuffmanTable.CreateOptimized(frequencies);
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int pending = 0;
        ProgressiveEntropyEncoder.WriteAc(writer, partialBlock, ac, ref pending);
        ProgressiveEntropyEncoder.WriteAc(writer, zeroBlock, ac, ref pending);
        ProgressiveEntropyEncoder.WriteAc(writer, finalBlock, ac, ref pending);
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        writer.Flush();

        // Hand-ordered Annex G tokens: -3, EOB run 2, three ZRLs, then +1.
        using var expectedStream = new MemoryStream();
        var expectedWriter = new BitWriter(expectedStream);
        ac.WriteSymbol(expectedWriter, 0x02);
        expectedWriter.WriteBits(0, 2);
        ac.WriteSymbol(expectedWriter, 0x10);
        expectedWriter.WriteBits(0, 1);
        for (int index = 0; index < 3; index++)
        {
            ac.WriteSymbol(expectedWriter, 0xf0);
        }
        ac.WriteSymbol(expectedWriter, 0xe1);
        expectedWriter.WriteBits(1, 1);
        expectedWriter.Flush();
        Assert.Equal(expectedStream.ToArray(), stream.ToArray());
        Assert.Equal(0, pending);
    }

    [Fact]
    public void EobRunsSplitAtMaximumAndResumeWithRemainingBlock()
    {
        short[] block = new short[64];
        long[] frequencies = new long[256];
        long[] tableFrequencies = new long[256];
        tableFrequencies[0xe0] = 2;
        tableFrequencies[0] = 1;
        HuffmanTable ac = HuffmanTable.CreateOptimized(tableFrequencies);
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int pending = 0;
        int frequencyPending = 0;
        for (int index = 0; index < 2 * 32767 + 1; index++)
        {
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending);
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref frequencyPending);
            if (index is 32766 or 65533)
            {
                Assert.Equal(0, pending);
                Assert.Equal(0, frequencyPending);
            }
        }
        Assert.Equal(1, pending);
        Assert.Equal(1, frequencyPending);
        Assert.Equal(2, frequencies[0xe0]);
        Assert.Equal(2, frequencies.Sum());
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref frequencyPending);
        writer.Flush();
        Assert.Equal(tableFrequencies, frequencies);

        using var expectedStream = new MemoryStream();
        var expectedWriter = new BitWriter(expectedStream);
        for (int index = 0; index < 2; index++)
        {
            ac.WriteSymbol(expectedWriter, 0xe0);
            expectedWriter.WriteBits(16383, 14);
        }
        ac.WriteSymbol(expectedWriter, 0);
        expectedWriter.Flush();
        Assert.Equal(expectedStream.ToArray(), stream.ToArray());
        Assert.Equal(0, pending);
        Assert.Equal(0, frequencyPending);
    }

    [Fact]
    public void ExplicitFlushResetsRunAndEmptyFlushDoesNotEmitTokens()
    {
        short[] block = new short[64];
        long[] frequencies = new long[256];
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int pending = 0;
        int frequencyPending = 0;
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref frequencyPending);
        for (int segment = 0; segment < 2; segment++)
        {
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending);
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref frequencyPending);
            ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
            ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref frequencyPending);
            Assert.Equal(0, pending);
            Assert.Equal(0, frequencyPending);
            if (segment == 0)
            {
                writer.WriteRestartMarker(0);
            }
        }
        ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending);
        ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref frequencyPending);
        writer.Flush();
        Assert.Equal(new byte[] { 0xaf, 0xff, 0xd0, 0xaf }, stream.ToArray());
        Assert.Equal(2, frequencies[0]);
        Assert.Equal(2, frequencies.Sum());
    }

    [Theory]
    [InlineData(-1024)]
    [InlineData(1024)]
    public void InvalidAcBlockDoesNotFlushPendingRun(short value)
    {
        short[] block = new short[64];
        block[1] = 1;
        block[JpegConstants.ZigZag[63]] = value;
        long[] frequencies = new long[256];
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int pending = 7;
        int frequencyPending = 7;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false), ref pending));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref frequencyPending));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
        Assert.Equal(7, pending);
        Assert.Equal(7, frequencyPending);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(65)]
    public void InvalidAcBlockLengthPreservesPendingRun(int length)
    {
        short[] block = new short[length];
        long[] frequencies = new long[256];
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        int pending = 7;
        int frequencyPending = 7;
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false), ref pending));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref frequencyPending));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
        Assert.Equal(7, pending);
        Assert.Equal(7, frequencyPending);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(32768)]
    [InlineData(int.MaxValue)]
    public void InvalidPendingRunIsRejectedWithoutWriting(int initialPending)
    {
        short[] block = new short[64];
        long[] frequencies = new long[256];
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        HuffmanTable ac = HuffmanTable.CreateDefault(true, false);
        int pending = initialPending;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, ac, ref pending));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref pending));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.FlushAc(writer, ac, ref pending));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref pending));
        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
        Assert.Equal(initialPending, pending);
    }

    [Fact]
    public void EobFrequencyOverflowPreservesPendingRun()
    {
        long[] frequencies = new long[256];
        frequencies[0x20] = long.MaxValue;
        int pending = 7;
        Assert.Throws<OverflowException>(() =>
            ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref pending));
        Assert.Equal(7, pending);
        Assert.Equal(long.MaxValue, frequencies[0x20]);
    }

    [Theory]
    [InlineData(-2047)]
    [InlineData(2047)]
    public void DcFrequencyPassAcceptsCategoryEleven(short value)
    {
        short[] block = new short[64];
        block[0] = value;
        long[] frequencies = new long[256];
        int previous = 0;
        ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies);
        Assert.Equal(value, previous);
        Assert.Equal(1, frequencies[11]);
    }

    [Theory]
    [InlineData(-2048)]
    [InlineData(2048)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidDcDifferenceDoesNotWriteOrChangePredictor(int previous)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        int original = previous;
        long[] frequencies = new long[256];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous,
                HuffmanTable.CreateDefault(false, false)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies));

        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(original, previous);
        Assert.Equal(0, frequencies.Sum());
    }

    [Theory]
    [InlineData(-1024)]
    [InlineData(1024)]
    public void InvalidFinalAcCoefficientIsRejectedBeforeWritingEarlierTokens(short value)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[64];
        block[1] = 1;
        block[JpegConstants.ZigZag[63]] = value;
        long[] frequencies = new long[256];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies));

        writer.Flush();
        Assert.Empty(stream.ToArray());
        Assert.Equal(0, frequencies.Sum());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(65)]
    public void BothScansRequireExactlyOneBlock(int length)
    {
        using var stream = new MemoryStream();
        var writer = new BitWriter(stream);
        short[] block = new short[length];
        int previous = 0;
        long[] frequencies = new long[256];

        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.WriteDc(writer, block, ref previous,
                HuffmanTable.CreateDefault(false, false)));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.WriteAc(writer, block, HuffmanTable.CreateDefault(true, false)));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies));
        Assert.Throws<ArgumentException>(() =>
            ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies));
    }

    [Fact]
    public void DcFrequencyOverflowKeepsPredictorUnchanged()
    {
        short[] block = new short[64];
        block[0] = 1;
        long[] frequencies = new long[256];
        frequencies[1] = long.MaxValue;
        int previous = 0;

        Assert.Throws<OverflowException>(() =>
            ProgressiveEntropyEncoder.AccumulateDcFrequencies(block, ref previous, frequencies));
        Assert.Equal(0, previous);
        Assert.Equal(long.MaxValue, frequencies[1]);
    }
}
