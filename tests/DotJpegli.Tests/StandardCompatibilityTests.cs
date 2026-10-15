#if DOTJPEGLI_NETSTANDARD_TESTS
using System.Numerics;
using DotJpegli.Compatibility;
using Xunit;

namespace DotJpegli.Tests;

public sealed class StandardCompatibilityTests
{
    [Fact]
    public void BitOperationsMatchRuntimeForBoundariesAndEverySingleBit()
    {
        uint[] narrow = [0, 1, 2, 3, uint.MaxValue, uint.MaxValue - 1, 0x7fffffff, 0x80000000];
        ulong[] wide = [0, 1, 2, 3, ulong.MaxValue, ulong.MaxValue - 1, 0x7fffffffffffffff, 0x8000000000000000];
        foreach (uint value in narrow)
            AssertBitOperations(value);
        foreach (ulong value in wide)
            AssertBitOperations(value);
        for (int bit = 0; bit < 32; bit++)
        {
            uint value = 1u << bit;
            AssertBitOperations(value);
            AssertBitOperations(value - 1);
            AssertBitOperations(value + 1);
            AssertBitOperations(~value);
        }
        for (int bit = 0; bit < 64; bit++)
        {
            ulong value = 1ul << bit;
            AssertBitOperations(value);
            AssertBitOperations(value - 1);
            AssertBitOperations(value + 1);
            AssertBitOperations(~value);
        }
    }

    [Fact]
    public void BitOperationsMatchRuntimeForDeterministicRandomValues()
    {
        uint state = 0xbadc0ffe;
        for (int iteration = 0; iteration < 10_000; iteration++)
        {
            AssertBitOperations(Next(ref state));
            AssertBitOperations(((ulong)Next(ref state) << 32) | Next(ref state));
        }
    }

    [Fact]
    public void CopySignAndBitDecrementPreserveSpecialValueBits()
    {
        uint[] bits = [0, 0x80000000, 1, 0x80000001, 0x007fffff, 0x807fffff,
            0x00800000, 0x80800000, 0x3f800000, 0xbf800000, 0x7f7fffff, 0xff7fffff,
            0x7f800000, 0xff800000, 0x7fc01234, 0xffc01234, 0x7f801234, 0xff801234];
        foreach (uint valueBits in bits)
        {
            float value = FromBits(valueBits);
            AssertBitsEqual(MathF.BitDecrement(value), StandardFloatingPoint.BitDecrement(value));
            foreach (uint signBits in bits)
            {
                float sign = FromBits(signBits);
                AssertBitsEqual(MathF.CopySign(value, sign), StandardFloatingPoint.CopySign(value, sign));
            }
        }
    }

    [Fact]
    public void FloatingPointHelpersMatchRuntimeAcrossExponentBoundariesAndRandomBits()
    {
        for (uint exponent = 1; exponent < 255; exponent++)
        {
            uint boundary = exponent << 23;
            for (uint offset = boundary - 1; offset <= boundary + 1; offset++)
            {
                AssertBitDecrement(FromBits(offset));
                AssertBitDecrement(FromBits(offset | 0x80000000));
            }
        }

        uint state = 0xcafef00d;
        for (int iteration = 0; iteration < 10_000; iteration++)
        {
            float value = FromBits(Next(ref state));
            float sign = FromBits(Next(ref state));
            AssertBitDecrement(value);
            AssertBitsEqual(MathF.CopySign(value, sign), StandardFloatingPoint.CopySign(value, sign));
        }
    }

    [Fact]
    public void AsciiValidationMatchesRuntimeForEveryByteAndUtf16CodeUnit()
    {
        Span<byte> bytes = stackalloc byte[3];
        Span<char> characters = stackalloc char[3];
        bytes.Clear();
        characters.Clear();
        Assert.True(StandardAscii.IsValid(ReadOnlySpan<byte>.Empty));
        Assert.True(StandardAscii.IsValid(ReadOnlySpan<char>.Empty));
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            bytes[1] = (byte)value;
            Assert.Equal(System.Text.Ascii.IsValid(bytes), StandardAscii.IsValid(bytes));
        }
        for (int value = 0; value <= char.MaxValue; value++)
        {
            characters[1] = (char)value;
            Assert.Equal(System.Text.Ascii.IsValid(characters), StandardAscii.IsValid(characters));
        }
    }

    private static void AssertBitOperations(uint value)
    {
        Assert.Equal(BitOperations.LeadingZeroCount(value), StandardBitOperations.LeadingZeroCount(value));
        Assert.Equal(BitOperations.TrailingZeroCount(value), StandardBitOperations.TrailingZeroCount(value));
        Assert.Equal(BitOperations.Log2(value), StandardBitOperations.Log2(value));
        Assert.Equal(BitOperations.PopCount(value), StandardBitOperations.PopCount(value));
        Assert.Equal(BitOperations.RoundUpToPowerOf2(value), StandardBitOperations.RoundUpToPowerOf2(value));
    }

    private static void AssertBitOperations(ulong value)
    {
        Assert.Equal(BitOperations.LeadingZeroCount(value), StandardBitOperations.LeadingZeroCount(value));
        Assert.Equal(BitOperations.TrailingZeroCount(value), StandardBitOperations.TrailingZeroCount(value));
        Assert.Equal(BitOperations.Log2(value), StandardBitOperations.Log2(value));
        Assert.Equal(BitOperations.PopCount(value), StandardBitOperations.PopCount(value));
        Assert.Equal(BitOperations.RoundUpToPowerOf2(value), StandardBitOperations.RoundUpToPowerOf2(value));
    }

    private static void AssertBitDecrement(float value) =>
        AssertBitsEqual(MathF.BitDecrement(value), StandardFloatingPoint.BitDecrement(value));

    private static void AssertBitsEqual(float expected, float actual) =>
        Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));

    private static float FromBits(uint value) => BitConverter.Int32BitsToSingle(unchecked((int)value));

    private static uint Next(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state;
    }
}
#endif
