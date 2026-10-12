// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Numerics;

namespace DotJpegli.Internal;

// Initial progressive scans (Ah = Al = 0) use the same difference and
// run/category tokens as ComputeTokensForBlock in google/jpegli
// lib/jpegli/entropy_coding-inl.h, revision
// 031a0077f5799a6041004267fc12b956c1f52a20. DC and AC are emitted separately.
// Each AC end-of-block token describes one block, without EOB run aggregation.
internal static class ProgressiveEntropyEncoder
{
    public static void WriteDc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        HuffmanTable dc)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(dc);
        ValidateDc(naturalCoefficients, previousDc);

        int difference = naturalCoefficients[0] - previousDc;
        int category = Category(difference);
        dc.WriteSymbol(writer, category);
        writer.WriteBits(AdditionalBits(difference, category), category);
        previousDc = naturalCoefficients[0];
    }

    public static void AccumulateDcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        long[] dc)
    {
        ValidateFrequencies(dc);
        ValidateDc(naturalCoefficients, previousDc);
        checked { dc[Category(naturalCoefficients[0] - previousDc)]++; }
        previousDc = naturalCoefficients[0];
    }

    public static void WriteAc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateAc(naturalCoefficients);

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            int value = naturalCoefficients[order[position]];
            if (value == 0)
            {
                run++;
                continue;
            }

            while (run >= 16)
            {
                ac.WriteSymbol(writer, 0xf0);
                run -= 16;
            }

            int category = Category(value);
            ac.WriteSymbol(writer, (run << 4) | category);
            writer.WriteBits(AdditionalBits(value, category), category);
            run = 0;
        }

        if (run != 0)
        {
            ac.WriteSymbol(writer, 0);
        }
    }

    public static void AccumulateAcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac)
    {
        ValidateFrequencies(ac);
        ValidateAc(naturalCoefficients);

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            int value = naturalCoefficients[order[position]];
            if (value == 0)
            {
                run++;
                continue;
            }

            while (run >= 16)
            {
                checked { ac[0xf0]++; }
                run -= 16;
            }

            checked { ac[(run << 4) | Category(value)]++; }
            run = 0;
        }

        if (run != 0)
        {
            checked { ac[0]++; }
        }
    }

    private static int Category(int value) =>
        value == 0 ? 0 : BitOperations.Log2((uint)Math.Abs(value)) + 1;

    private static uint AdditionalBits(int value, int category) =>
        unchecked((uint)(value < 0 ? value - 1 : value)) & ((1U << category) - 1);

    private static void ValidateFrequencies(long[] frequencies)
    {
        ArgumentNullException.ThrowIfNull(frequencies);
        if (frequencies.Length != 256)
        {
            throw new ArgumentException("A frequency table must have 256 entries.", nameof(frequencies));
        }
    }

    private static void ValidateLength(ReadOnlySpan<short> coefficients)
    {
        if (coefficients.Length != 64)
        {
            throw new ArgumentException("A DCT block must contain 64 coefficients.", nameof(coefficients));
        }
    }

    private static void ValidateDc(ReadOnlySpan<short> coefficients, int previousDc)
    {
        ValidateLength(coefficients);
        long difference = (long)coefficients[0] - previousDc;
        if (difference is < -2047 or > 2047)
        {
            throw new ArgumentOutOfRangeException(nameof(coefficients), "The DC difference exceeds eight-bit JPEG's 11-bit category.");
        }
    }

    private static void ValidateAc(ReadOnlySpan<short> coefficients)
    {
        ValidateLength(coefficients);
        for (int index = 1; index < 64; index++)
        {
            if (coefficients[index] is < -1023 or > 1023)
            {
                throw new ArgumentOutOfRangeException(nameof(coefficients), "An AC coefficient exceeds eight-bit JPEG's 10-bit category.");
            }
        }
    }
}
