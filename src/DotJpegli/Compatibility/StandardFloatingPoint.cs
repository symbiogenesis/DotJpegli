namespace DotJpegli.Compatibility;

internal static class StandardFloatingPoint
{
#if NETSTANDARD2_1
    internal static float CopySign(float value, float sign) =>
        BitConverter.Int32BitsToSingle((BitConverter.SingleToInt32Bits(value) & 0x7fffffff)
            | (BitConverter.SingleToInt32Bits(sign) & int.MinValue));
#else
    internal static float CopySign(float value, float sign) => MathF.CopySign(value, sign);
#endif


#if NETSTANDARD2_1
    internal static float Log2(float value) => (float)Math.Log(value, 2);
#else
    internal static float Log2(float value) => MathF.Log2(value);
#endif


#if NETSTANDARD2_1
    internal static float BitDecrement(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        if (float.IsNaN(value) || value == float.NegativeInfinity)
        {
            return value;
        }

        if (value == 0)
        {
            return -float.Epsilon;
        }

        return BitConverter.Int32BitsToSingle(bits < 0 ? bits + 1 : bits - 1);
    }
#else
    internal static float BitDecrement(float value) => MathF.BitDecrement(value);
#endif

#if NETSTANDARD2_1
    internal static double BitDecrement(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        if (double.IsNaN(value) || value == double.NegativeInfinity)
        {
            return value;
        }

        if (value == 0)
        {
            return -double.Epsilon;
        }

        return BitConverter.Int64BitsToDouble(bits < 0 ? bits + 1 : bits - 1);
    }
#else
    internal static double BitDecrement(double value) => Math.BitDecrement(value);
#endif
}
