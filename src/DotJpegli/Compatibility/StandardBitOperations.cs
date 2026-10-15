#if NETSTANDARD2_1
namespace DotJpegli.Compatibility;

internal static class StandardBitOperations
{
    internal static int LeadingZeroCount(uint value)
    {
        if (value == 0)
            return 32;
        int count = 0;
        if (value < 0x10000) { count += 16; value <<= 16; }
        if (value < 0x1000000) { count += 8; value <<= 8; }
        if (value < 0x10000000) { count += 4; value <<= 4; }
        if (value < 0x40000000) { count += 2; value <<= 2; }
        if (value < 0x80000000) count++;
        return count;
    }

    internal static int LeadingZeroCount(ulong value) => (uint)(value >> 32) is uint high && high != 0
        ? LeadingZeroCount(high) : 32 + LeadingZeroCount((uint)value);

    internal static int TrailingZeroCount(uint value)
    {
        if (value == 0)
            return 32;
        int count = 0;
        if ((value & 0xffff) == 0) { count += 16; value >>= 16; }
        if ((value & 0xff) == 0) { count += 8; value >>= 8; }
        if ((value & 0xf) == 0) { count += 4; value >>= 4; }
        if ((value & 3) == 0) { count += 2; value >>= 2; }
        if ((value & 1) == 0) count++;
        return count;
    }

    internal static int TrailingZeroCount(ulong value) => (uint)value is uint low && low != 0
        ? TrailingZeroCount(low) : 32 + TrailingZeroCount((uint)(value >> 32));

    internal static int Log2(uint value) => value == 0 ? 0 : 31 - LeadingZeroCount(value);
    internal static int Log2(ulong value) => value == 0 ? 0 : 63 - LeadingZeroCount(value);

    internal static int PopCount(uint value)
    {
        value -= (value >> 1) & 0x55555555;
        value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
        value = (value + (value >> 4)) & 0x0f0f0f0f;
        return (int)((value * 0x01010101) >> 24);
    }

    internal static int PopCount(ulong value) => PopCount((uint)value) + PopCount((uint)(value >> 32));

    internal static uint RoundUpToPowerOf2(uint value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    internal static ulong RoundUpToPowerOf2(ulong value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value |= value >> 32;
        return value + 1;
    }
}
#endif
