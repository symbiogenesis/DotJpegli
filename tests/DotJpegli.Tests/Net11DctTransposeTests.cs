namespace DotJpegli.Tests;

using System.Runtime.Intrinsics;
using DotJpegli.Internal;
using Xunit;

public sealed class Net11DctTransposeTests
{
    [Fact]
    public void PortableTransposeMovesEveryIndependentLaneWithoutArithmetic()
    {
        foreach (int payload in new[] { int.MinValue, 1, unchecked((int)0xff801234) })
        {
            for (int source = 0; source < 64; source++)
            {
                var rows = new Vector256<float>[9];
                rows[source / 8] = Vector256.WithElement(Vector256<int>.Zero, source % 8, payload).AsSingle();
                rows[8] = Vector256.Create(0x7f805678).AsSingle();
                ForwardDct.TransposePortable(rows);
                for (int row = 0; row < 8; row++)
                {
                    for (int column = 0; column < 8; column++)
                    {
                        int expected = row == source % 8 && column == source / 8 ? payload : 0;
                        Assert.Equal(expected, BitConverter.SingleToInt32Bits(rows[row][column]));
                    }
                }

                Assert.Equal(Vector256.Create(0x7f805678), rows[8].AsInt32());
            }
        }
    }

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
        bits[8] = 0x7f801234;
        bits[9] = unchecked((int)0xff805678);
        bits[10] = 0x7f7fffff;
        bits[11] = unchecked((int)0xff7fffff);
        float[] values = bits.Select(BitConverter.Int32BitsToSingle).ToArray();
        Vector256<float>[] rows = Enumerable.Range(0, 9)
            .Select(row => Vector256.Create(values.AsSpan(row * 8, 8))).ToArray();
        ForwardDct.TransposePortable(rows);
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                Assert.Equal(bits[(column * 8) + row], BitConverter.SingleToInt32Bits(rows[row][column]));
            }
        }

        ForwardDct.TransposePortable(rows);
        for (int row = 0; row < rows.Length; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                Assert.Equal(bits[(row * 8) + column], BitConverter.SingleToInt32Bits(rows[row][column]));
            }
        }
    }

    [Fact]
    public void PortableTransposeRejectsIncompleteRowsBeforeWriting()
    {
        Vector256<float>[] rows = Enumerable.Range(0, 7).Select(row => Vector256.Create((float)row)).ToArray();
        var original = (Vector256<float>[])rows.Clone();
        Assert.Equal("rows", Assert.Throws<ArgumentException>(() => ForwardDct.TransposePortable(rows)).ParamName);
        Assert.Equal(original, rows);
    }
}
