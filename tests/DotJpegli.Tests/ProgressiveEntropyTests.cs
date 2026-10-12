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
