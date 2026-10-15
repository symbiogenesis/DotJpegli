// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Numerics;
using System.Runtime.Intrinsics;

namespace DotJpegli.Internal;

// Managed scalar adaptation of ComputeTokensForBlock<T, false> in
// google/jpegli lib/jpegli/entropy_coding-inl.h, revision
// 031a0077f5799a6041004267fc12b956c1f52a20. Histogram collection can retain
// bounded tokens for emission without a second coefficient traversal.
internal static class EntropyEncoder
{
    public static void WriteBlock(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        HuffmanTable dc,
        HuffmanTable ac,
        bool coefficientsValidated = false,
        bool zigZagOrdered = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateBlock(naturalCoefficients, previousDc, coefficientsValidated);

        int difference = naturalCoefficients[0] - previousDc;
        int category = Category(difference);
        dc.WriteSymbolWithBits(writer, category, AdditionalBits(difference, category), category);
        previousDc = naturalCoefficients[0];

        if (zigZagOrdered)
        {
            ulong nonzero = NonzeroAcMask(naturalCoefficients);
            int previousPosition = 0;
            while (nonzero != 0)
            {
                int position = BitOperations.TrailingZeroCount(nonzero);
                int run = position - previousPosition - 1;
                while (run >= 16)
                {
                    ac.WriteSymbol(writer, 0xf0);
                    run -= 16;
                }
                int value = naturalCoefficients[position];
                category = Category(value);
                ac.WriteSymbolWithBits(writer, (run << 4) | category, AdditionalBits(value, category), category);
                previousPosition = position;
                nonzero &= nonzero - 1;
            }
            if (previousPosition != 63)
                ac.WriteSymbol(writer, 0);
            return;
        }

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
            ac.WriteSymbolWithBits(writer, (run << 4) | category, AdditionalBits(value, category), category);
        }
    }

    public static void AccumulateFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        long[] dc,
        long[] ac,
        bool coefficientsValidated = false,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(ac);
        if (dc.Length != 256 || ac.Length != 256)
        {
            throw new ArgumentException("Frequency tables must each have 256 entries.");
        }

        ValidateBlock(naturalCoefficients, previousDc, coefficientsValidated);
        if (zigZagOrdered && tokens is not null && tokens.TryReserveBlock(out Span<uint> destination))
        {
            AccumulateOrderedBlock(naturalCoefficients, ref previousDc, dc, ac, tokens, component, destination);
            return;
        }
        int difference = naturalCoefficients[0] - previousDc;
        int category = Category(difference);
        checked { dc[category]++; }
        tokens?.AddHuffman(component, false, category, AdditionalBits(difference, category), category);
        previousDc = naturalCoefficients[0];

        if (zigZagOrdered)
        {
            ulong nonzero = NonzeroAcMask(naturalCoefficients);
            int previousPosition = 0;
            while (nonzero != 0)
            {
                int position = BitOperations.TrailingZeroCount(nonzero);
                int run = position - previousPosition - 1;
                while (run >= 16)
                {
                    checked { ac[0xf0]++; }
                    tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                    run -= 16;
                }
                int value = naturalCoefficients[position];
                category = Category(value);
                int symbol = (run << 4) | category;
                checked { ac[symbol]++; }
                tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
                previousPosition = position;
                nonzero &= nonzero - 1;
            }
            if (previousPosition != 63)
            {
                checked { ac[0]++; }
                tokens?.AddHuffman(component, true, 0, 0, 0);
            }
            return;
        }

        int nonzeros = CountNonzeroAc(naturalCoefficients);
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            if (nonzeros == 0)
            {
                checked { ac[0]++; }
                tokens?.AddHuffman(component, true, 0, 0, 0);
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
                tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                run -= 16;
            }

            category = Category(value);
            int symbol = (run << 4) | category;
            checked { ac[symbol]++; }
            tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
        }
    }

    private static void AccumulateOrderedBlock(ReadOnlySpan<short> coefficients, ref int previousDc,
        long[] dc, long[] ac, EntropyTokens tokens, int component, Span<uint> destination)
    {
        int count = 0;
        int dcContext = component << 1;
        int acContext = dcContext | 1;
        try
        {
            int difference = coefficients[0] - previousDc;
            int category = Category(difference);
            checked { dc[category]++; }
            destination[count++] = EntropyTokens.PackHuffman(dcContext, category, AdditionalBits(difference, category), category);
            previousDc = coefficients[0];
            ulong nonzero = NonzeroAcMask(coefficients);
            int previousPosition = 0;
            while (nonzero != 0)
            {
                int position = BitOperations.TrailingZeroCount(nonzero);
                int run = position - previousPosition - 1;
                while (run >= 16)
                {
                    checked { ac[0xf0]++; }
                    destination[count++] = EntropyTokens.PackHuffman(acContext, 0xf0, 0, 0);
                    run -= 16;
                }
                int value = coefficients[position];
                category = Category(value);
                int symbol = (run << 4) | category;
                checked { ac[symbol]++; }
                destination[count++] = EntropyTokens.PackHuffman(acContext, symbol, AdditionalBits(value, category), category);
                previousPosition = position;
                nonzero &= nonzero - 1;
            }
            if (previousPosition != 63)
            {
                checked { ac[0]++; }
                destination[count++] = EntropyTokens.PackHuffman(acContext, 0, 0, 0);
            }
        }
        finally
        {
            // A checked histogram overflow retains exactly the earlier tokens,
            // just as the individual AddHuffman calls do on the strict path.
            tokens.CommitBlock(count);
        }
    }

    private static int Category(int value) =>
        value == 0 ? 0 : BitOperations.Log2((uint)Math.Abs(value)) + 1;

    private static uint AdditionalBits(int value, int category) =>
        unchecked((uint)(value < 0 ? value - 1 : value)) & ((1U << category) - 1);

    private static int CountNonzeroAc(ReadOnlySpan<short> coefficients) =>
        63 - coefficients[1..].Count((short)0);

    // One bit per zigzag position. Masking DC lets its wider signed range use
    // the same full-vector loads as the AC coefficients without a scalar tail.
    internal static ulong NonzeroAcMask(ReadOnlySpan<short> zigZagCoefficients)
    {
        ulong mask = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            for (int position = 0; position < 64; position += Vector256<short>.Count)
            {
                Vector256<short> values = Vector256.Create(zigZagCoefficients.Slice(position, Vector256<short>.Count));
                uint zeros = Vector256.ExtractMostSignificantBits(Vector256.Equals(values, Vector256<short>.Zero));
                mask |= (ulong)(~zeros & 0xffffU) << position;
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (int position = 0; position < 64; position += Vector128<short>.Count)
            {
                Vector128<short> values = Vector128.Create(zigZagCoefficients.Slice(position, Vector128<short>.Count));
                uint zeros = Vector128.ExtractMostSignificantBits(Vector128.Equals(values, Vector128<short>.Zero));
                mask |= (ulong)(~zeros & 0xffU) << position;
            }
        }
        else
        {
            for (int position = 1; position < 64; position++)
                if (zigZagCoefficients[position] != 0)
                    mask |= 1UL << position;
        }
        return mask & ~1UL;
    }

    private static void ValidateBlock(ReadOnlySpan<short> coefficients, int previousDc, bool coefficientsValidated)
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

        if (!coefficientsValidated)
            ValidateAc(coefficients);
    }

    // Cached blocks call this once before their histogram and emission passes.
    // DC depends on scan order and predictors and is always checked separately.
    internal static void ValidateAc(ReadOnlySpan<short> coefficients)
    {
        if (!AcCoefficientsInRange(coefficients))
            throw new ArgumentOutOfRangeException(nameof(coefficients), "An AC coefficient exceeds baseline JPEG's 10-bit category.");
    }

    internal static bool AcCoefficientsInRange(ReadOnlySpan<short> coefficients)
    {
        if (coefficients.Length != 64)
            throw new ArgumentException("A DCT block must contain 64 coefficients.", nameof(coefficients));

        bool invalid = false;
        int lanes = Vector<short>.Count;
        if (Vector.IsHardwareAccelerated && lanes <= 64 && 64 % lanes == 0)
        {
            var minimum = new Vector<short>(-1023);
            var maximum = new Vector<short>(1023);
            // Load full vectors without a scalar tail. The first lane is DC,
            // whose wider/difference-dependent range must not affect this test.
            Vector<short> values = Vector.WithElement(new Vector<short>(coefficients), 0, (short)0);
            Vector<short> outside = Vector.LessThan(values, minimum) | Vector.GreaterThan(values, maximum);
            for (int offset = lanes; offset < 64; offset += lanes)
            {
                values = new Vector<short>(coefficients.Slice(offset, lanes));
                outside |= Vector.LessThan(values, minimum) | Vector.GreaterThan(values, maximum);
            }
            invalid = !Vector.EqualsAll(outside, Vector<short>.Zero);
        }
        else
        {
            for (int offset = 1; offset < 64; offset++)
                invalid |= coefficients[offset] is < -1023 or > 1023;
        }
        return !invalid;
    }
}
