using System.Runtime.Intrinsics;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class Net11DctTransposeTests
{
    [Fact]
    public void PortableTransposePreservesEveryBitAndItsInverseRestoresRows()
    {
        int[] bits = Enumerable.Range(0, 72).Select(index => unchecked(index * 1932735281)).ToArray();
        bits[0] = 0;
        bits[1] = int.MinValue;
        bits[2] = 0x7f800000;
        bits[3] = unchecked((int)0xff800000);
        bits[4] = 0x7fc01234;
        bits[5] = unchecked((int)0xffc05678);
        bits[6] = 1;
        bits[7] = unchecked((int)0x80000001);
        float[] values = bits.Select(BitConverter.Int32BitsToSingle).ToArray();
        Vector256<float>[] rows = Enumerable.Range(0, 9)
            .Select(row => Vector256.Create(values.AsSpan(row * 8, 8))).ToArray();
        ForwardDct.TransposePortable(rows);
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                Assert.Equal(bits[column * 8 + row], BitConverter.SingleToInt32Bits(rows[row][column]));
        ForwardDct.TransposePortable(rows);
        for (int row = 0; row < rows.Length; row++)
            for (int column = 0; column < 8; column++)
                Assert.Equal(bits[row * 8 + column], BitConverter.SingleToInt32Bits(rows[row][column]));
    }

    [Fact]
    public void PortableTransposeRejectsIncompleteRowsBeforeWriting()
    {
        Vector256<float>[] rows = Enumerable.Range(0, 7).Select(row => Vector256.Create((float)row)).ToArray();
        Vector256<float>[] original = (Vector256<float>[])rows.Clone();
        Assert.Equal("rows", Assert.Throws<ArgumentException>(() => ForwardDct.TransposePortable(rows)).ParamName);
        Assert.Equal(original, rows);
    }
}
