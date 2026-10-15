namespace DotJpegli.Compatibility;

internal static class StandardFloatingPoint
{
    internal static float CopySign(float value, float sign)
    {
#if NETSTANDARD2_1
        return BitConverter.Int32BitsToSingle((BitConverter.SingleToInt32Bits(value) & 0x7fffffff)
            | (BitConverter.SingleToInt32Bits(sign) & int.MinValue));
#else
        return MathF.CopySign(value, sign);
#endif
    }

    internal static float Log2(float value)
    {
#if NETSTANDARD2_1
        return (float)Math.Log(value, 2);
#else
        return MathF.Log2(value);
#endif
    }

    internal static float BitDecrement(float value)
    {
#if NETSTANDARD2_1
        int bits = BitConverter.SingleToInt32Bits(value);
        if (float.IsNaN(value) || value == float.NegativeInfinity)
            return value;
        if (value == 0)
            return -float.Epsilon;
        return BitConverter.Int32BitsToSingle(bits < 0 ? bits + 1 : bits - 1);
#else
        return MathF.BitDecrement(value);
#endif
    }

    internal static double BitDecrement(double value)
    {
#if NETSTANDARD2_1
        long bits = BitConverter.DoubleToInt64Bits(value);
        if (double.IsNaN(value) || value == double.NegativeInfinity)
            return value;
        if (value == 0)
            return -double.Epsilon;
        return BitConverter.Int64BitsToDouble(bits < 0 ? bits + 1 : bits - 1);
#else
        return Math.BitDecrement(value);
#endif
    }
}
