// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Numerics;

namespace DotJpegli.Internal;

// Managed scalar adaptation of ComputeTokensForBlock<T, false> in
// google/jpegli lib/jpegli/entropy_coding-inl.h, revision
// 031a0077f5799a6041004267fc12b956c1f52a20. Writes baseline tokens directly
// instead of storing them in the native encoder's token arrays.
internal static class EntropyEncoder
{
    public static void WriteBlock(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        HuffmanTable dc,
        HuffmanTable ac)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateBlock(naturalCoefficients, previousDc);

        int difference = naturalCoefficients[0] - previousDc;
        int category = Category(difference);
        dc.WriteSymbol(writer, category);
        writer.WriteBits(AdditionalBits(difference, category), category);
        previousDc = naturalCoefficients[0];

        int nonzeros = CountNonzeroAc(naturalCoefficients);
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            if (nonzeros == 0)
            {
                ac.WriteSymbol(writer, 0);
                break;
            }

            int run = 0;
            int value;
            while ((value = naturalCoefficients[order[position]]) == 0)
            {
                run++;
                position++;
            }

            nonzeros--;
            while (run >= 16)
            {
                ac.WriteSymbol(writer, 0xf0);
                run -= 16;
            }

            category = Category(value);
            ac.WriteSymbol(writer, (run << 4) | category);
            writer.WriteBits(AdditionalBits(value, category), category);
        }
    }

    public static void AccumulateFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        long[] dc,
        long[] ac)
    {
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(ac);
        if (dc.Length != 256 || ac.Length != 256)
        {
            throw new ArgumentException("Frequency tables must each have 256 entries.");
        }

        ValidateBlock(naturalCoefficients, previousDc);
        int difference = naturalCoefficients[0] - previousDc;
        checked { dc[Category(difference)]++; }
        previousDc = naturalCoefficients[0];

        int nonzeros = CountNonzeroAc(naturalCoefficients);
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            if (nonzeros == 0)
            {
                checked { ac[0]++; }
                break;
            }

            int run = 0;
            int value;
            while ((value = naturalCoefficients[order[position]]) == 0)
            {
                run++;
                position++;
            }

            nonzeros--;
            while (run >= 16)
            {
                checked { ac[0xf0]++; }
                run -= 16;
            }

            checked { ac[(run << 4) | Category(value)]++; }
        }
    }

    private static int Category(int value) =>
        value == 0 ? 0 : BitOperations.Log2((uint)Math.Abs(value)) + 1;

    private static uint AdditionalBits(int value, int category) =>
        unchecked((uint)(value < 0 ? value - 1 : value)) & ((1U << category) - 1);

    private static int CountNonzeroAc(ReadOnlySpan<short> coefficients) =>
        63 - coefficients[1..].Count((short)0);

    private static void ValidateBlock(ReadOnlySpan<short> coefficients, int previousDc)
    {
        if (coefficients.Length != 64)
        {
            throw new ArgumentException("A DCT block must contain 64 coefficients.", nameof(coefficients));
        }

        // Baseline eight-bit JPEG permits DC categories through 11 and
        // AC categories through 10. Validate before producing any bytes.
        long difference = (long)coefficients[0] - previousDc;
        if (difference is < -2047 or > 2047)
        {
            throw new ArgumentOutOfRangeException(nameof(coefficients), "The DC difference exceeds baseline JPEG's 11-bit category.");
        }

        if (coefficients[1..].ContainsAnyExceptInRange((short)-1023, (short)1023))
        {
            throw new ArgumentOutOfRangeException(nameof(coefficients), "An AC coefficient exceeds baseline JPEG's 10-bit category.");
        }
    }
}
