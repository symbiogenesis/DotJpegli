namespace DotJpegli.Tests;

using System.Text;
using DotJpegli.Parity;
using Xunit;

public sealed class PpmImageTests
{
    [Theory]
    [InlineData("P6\n1 1\n255\n")]
    [InlineData("P6\r\n1 1\r\n255\r\n")]
    [InlineData("\t\v\f P6\n# dimensions\n01# width\n1\t255\t")]
    [InlineData("P6\n1\0 1\0\n255\n")]
    public void HeaderParsingPreservesCommentsSeparatorsAndNumericGrammar(string header)
    {
        byte[] pixels = [10, 32, 35];
        var image = PpmImage.Read([.. Encoding.ASCII.GetBytes(header), .. pixels]);

        Assert.Equal(1, image.Width);
        Assert.Equal(1, image.Height);
        Assert.Equal(pixels, image.Pixels);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(32)]
    public void OnlyTheRasterSeparatorIsConsumed(byte separator)
    {
        byte[] pixels = [35, 10, 32];
        var image = PpmImage.Read([.. "P6\n1 1\n255"u8, separator, .. pixels]);

        Assert.Equal(pixels, image.Pixels);
    }

    [Theory]
    [InlineData("")]
    [InlineData("P6\n")]
    [InlineData("P3\n1 1\n255\n")]
    [InlineData("P6\n+1 1\n255\n")]
    [InlineData("P6\n-1 1\n255\n")]
    [InlineData("P6\n0 1\n255\n")]
    [InlineData("P6\n1 0\n255\n")]
    [InlineData("P6\n1.0 1\n255\n")]
    [InlineData("P6\n1e0 1\n255\n")]
    [InlineData("P6\n1\0x 1\n255\n")]
    [InlineData("P6\n2147483648 1\n255\n")]
    [InlineData("P6\n1 2147483648\n255\n")]
    [InlineData("P6\n2147483647 2147483647\n255\n")]
    [InlineData("P6\n1 1\n65535\n")]
    [InlineData("P6\n1 1\n255")]
    public void InvalidHeadersAreRejected(string header)
        => Assert.Throws<InvalidDataException>(() => PpmImage.Read([.. Encoding.ASCII.GetBytes(header), 35, 10, 32]));

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void NonAsciiHeaderTokensAreRejected(int position)
    {
        byte[] data = [.. "P6\n1 1\n255\n"u8, 35, 10, 32];
        data[position] = 0xff;

        _ = Assert.Throws<InvalidDataException>(() => PpmImage.Read(data));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void RasterLengthMustMatchDimensions(int length)
        => Assert.Throws<InvalidDataException>(() => PpmImage.Read([.. "P6\n1 1\n255\n"u8, .. new byte[length]]));
}
