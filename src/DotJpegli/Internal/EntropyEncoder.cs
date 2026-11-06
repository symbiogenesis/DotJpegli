// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

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
        Guard.ThrowIfNull(writer);
        Guard.ThrowIfNull(dc);
        Guard.ThrowIfNull(ac);
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
            {
                ac.WriteSymbol(writer, 0);
            }

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
        Guard.ThrowIfNull(dc);
        Guard.ThrowIfNull(ac);
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
        checked
        { dc[category]++; }
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
                    checked
                    { ac[0xf0]++; }
                    tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                    run -= 16;
                }
                int value = naturalCoefficients[position];
                category = Category(value);
                int symbol = (run << 4) | category;
                checked
                { ac[symbol]++; }
                tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
                previousPosition = position;
                nonzero &= nonzero - 1;
            }
            if (previousPosition != 63)
            {
                checked
                { ac[0]++; }
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
                checked
                { ac[0]++; }
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
                checked
                { ac[0xf0]++; }
                tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                run -= 16;
            }

            category = Category(value);
            int symbol = (run << 4) | category;
            checked
            { ac[symbol]++; }
            tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
        }
    }

    private static void AccumulateOrderedBlock(ReadOnlySpan<short> coefficients, ref int previousDc,
        long[] dc, long[] ac, EntropyTokens tokens, int component, Span<uint> destination)
        => AccumulateOrderedBlockCore(coefficients, NonzeroAcMask(coefficients),
            coefficients[0] - previousDc, ref previousDc, dc, ac, tokens, component, destination);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AccumulateOrderedBlockCore(ReadOnlySpan<short> coefficients, ulong nonzero,
        int difference, ref int previousDc, long[] dc, long[] ac, EntropyTokens tokens, int component,
        Span<uint> destination)
    {
        int count = 0;
        int dcContext = component << 1;
        int acContext = dcContext | 1;
        ReadOnlySpan<uint> amplitudes = PreparedAmplitudes.Values.AsSpan(0, 4096);
        uint amplitude = PrepareAmplitude(difference, amplitudes);
        int category = (int)(amplitude >> 27);
        IncrementReservedFrequency(dc, category, tokens, count);
        destination[count++] = amplitude | ((uint)dcContext << 24);
        previousDc = coefficients[0];
        int previousPosition = 0;
        while (nonzero != 0)
        {
            int position = BitOperations.TrailingZeroCount(nonzero);
            int run = position - previousPosition - 1;
            while (run >= 16)
            {
                IncrementReservedFrequency(ac, 0xf0, tokens, count);
                destination[count++] = EntropyTokens.PackHuffman(acContext, 0xf0, 0, 0);
                run -= 16;
            }
            int value = coefficients[position];
            amplitude = PrepareAmplitude(value, amplitudes);
            category = (int)(amplitude >> 27);
            int symbol = (run << 4) | category;
            IncrementReservedFrequency(ac, symbol, tokens, count);
            destination[count++] = amplitude | ((uint)run << 20) | ((uint)acContext << 24);
            previousPosition = position;
            nonzero &= nonzero - 1;
        }
        if (previousPosition != 63)
        {
            IncrementReservedFrequency(ac, 0, tokens, count);
            destination[count++] = EntropyTokens.PackHuffman(acContext, 0, 0, 0);
        }
        tokens.CommitBlock(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AccumulateDcOnlyBlock(
        short dcValue,
        ref int previousDc,
        long[] dc,
        long[] ac,
        EntropyTokens? tokens = null,
        int component = 0)
    {
        int difference = dcValue - previousDc;
        if ((uint)(difference + 2047) > 4094)
        {
            ThrowDcDifferenceOutOfRange();
        }

        previousDc = dcValue;
        if (tokens is not null && tokens.TryReserveDcOnlyBlock(out Span<uint> destination))
        {
            int dcContext = component << 1;
            int acContext = dcContext | 1;
            uint amplitude = PreparedAmplitudes.Values[difference + 2048];
            int category = (int)(amplitude >> 27);
            IncrementReservedFrequency(dc, category, tokens, 0);
            destination[0] = amplitude | ((uint)dcContext << 24);
            IncrementReservedFrequency(ac, 0, tokens, 1);
            destination[1] = EntropyTokens.PackHuffman(acContext, 0, 0, 0);
            tokens.CommitBlock(2);
            return;
        }

        int slowCategory = Category(difference);
        checked
        { dc[slowCategory]++; }
        tokens?.AddHuffman(component, false, slowCategory, AdditionalBits(difference, slowCategory), slowCategory);
        checked
        { ac[0]++; }
        tokens?.AddHuffman(component, true, 0, 0, 0);
    }

    internal static void AccumulateOrderedBlockWithMask(
        ReadOnlySpan<short> zigZagCoefficients,
        ulong nonzeroAcMask,
        ref int previousDc,
        long[] dc,
        long[] ac,
        EntropyTokens? tokens = null,
        int component = 0)
    {
        int difference = zigZagCoefficients[0] - previousDc;
        if ((uint)(difference + 2047) > 4094)
        {
            ThrowDcDifferenceOutOfRange();
        }

        if (tokens is not null && tokens.TryReserveBlock(out Span<uint> destination))
        {
            AccumulateOrderedBlockCore(zigZagCoefficients, nonzeroAcMask, difference, ref previousDc,
                dc, ac, tokens, component, destination);
            return;
        }

        int category = Category(difference);
        checked
        { dc[category]++; }
        tokens?.AddHuffman(component, false, category, AdditionalBits(difference, category), category);
        previousDc = zigZagCoefficients[0];

        ulong nonzero = nonzeroAcMask;
        int previousPosition = 0;
        while (nonzero != 0)
        {
            int position = BitOperations.TrailingZeroCount(nonzero);
            int run = position - previousPosition - 1;
            while (run >= 16)
            {
                checked
                { ac[0xf0]++; }
                tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                run -= 16;
            }
            int value = zigZagCoefficients[position];
            category = Category(value);
            int symbol = (run << 4) | category;
            checked
            { ac[symbol]++; }
            tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
            previousPosition = position;
            nonzero &= nonzero - 1;
        }
        if (previousPosition != 63)
        {
            checked
            { ac[0]++; }
            tokens?.AddHuffman(component, true, 0, 0, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteDcOnlyBlock(
        BitWriter writer,
        short dcValue,
        ref int previousDc,
        HuffmanTable dc,
        HuffmanTable ac)
    {
        int difference = dcValue - previousDc;
        if ((uint)(difference + 2047) > 4094)
        {
            ThrowDcDifferenceOutOfRange();
        }

        int category = Category(difference);
        dc.WriteSymbolWithBits(writer, category, AdditionalBits(difference, category), category);
        previousDc = dcValue;
        ac.WriteSymbol(writer, 0);
    }

    internal static void WriteOrderedBlockWithMask(
        BitWriter writer,
        ReadOnlySpan<short> zigZagCoefficients,
        ulong nonzeroAcMask,
        ref int previousDc,
        HuffmanTable dc,
        HuffmanTable ac)
    {
        int difference = zigZagCoefficients[0] - previousDc;
        if ((uint)(difference + 2047) > 4094)
        {
            ThrowDcDifferenceOutOfRange();
        }

        int category = Category(difference);
        dc.WriteSymbolWithBits(writer, category, AdditionalBits(difference, category), category);
        previousDc = zigZagCoefficients[0];

        ulong nonzero = nonzeroAcMask;
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
            int value = zigZagCoefficients[position];
            category = Category(value);
            ac.WriteSymbolWithBits(writer, (run << 4) | category, AdditionalBits(value, category), category);
            previousPosition = position;
            nonzero &= nonzero - 1;
        }
        if (previousPosition != 63)
        {
            ac.WriteSymbol(writer, 0);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDcDifferenceOutOfRange()
        => throw new ArgumentOutOfRangeException("coefficients", "The DC difference exceeds baseline JPEG's 11-bit category.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PrepareAmplitude(int value, ReadOnlySpan<uint> amplitudes)
    {
        int index = value + 2048;
        if ((uint)index < 4096)
        {
            return amplitudes[index];
        }

        int category = Category(value);
        return EntropyTokens.PackHuffman(0, category, AdditionalBits(value, category), category);
    }

    private static class PreparedAmplitudes
    {
        // A 16 KiB table covers every validated baseline DC difference and AC
        // coefficient. Generic already-validated callers retain the arithmetic
        // fallback for wider short amplitudes. Allocate only on token recording.
        internal static readonly uint[] Values = Create();

        private static uint[] Create()
        {
            uint[] values = new uint[4096];
            for (int index = 0; index < values.Length; index++)
            {
                int value = index - 2048;
                int category = Category(value);
                values[index] = EntropyTokens.PackHuffman(0, category, AdditionalBits(value, category), category);
            }
            return values;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void IncrementReservedFrequency(long[] frequencies, int symbol, EntropyTokens tokens, int count)
    {
        ref long frequency = ref frequencies[symbol];
        // The reservation guarantees room for every block token, and the
        // caller validated coefficient/table lengths before entering this path.
        // Histogram overflow is its only exceptional exit. Commit the same
        // earlier prefix without an EH region spilling count for every token.
        long next = unchecked(frequency + 1);
        if (next < frequency)
        {
            tokens.CommitBlock(count);
            throw new OverflowException();
        }
        frequency = next;
    }

    // Signed amplitudes are unpredictable in image data. Derive magnitude and
    // the negative-value suffix adjustment without a branch for each symbol;
    // leading-zero count also handles the zero DC difference directly.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Category(int value)
    {
        int sign = value >> 31;
        uint magnitude = unchecked((uint)((value ^ sign) - sign));
        return 32 - BitOperations.LeadingZeroCount(magnitude);
    }

    private static uint AdditionalBits(int value, int category) =>
        unchecked((uint)(value + (value >> 31))) & ((1U << category) - 1);

    private static int CountNonzeroAc(ReadOnlySpan<short> coefficients)
    {
        const short zero = 0;
        return 63 - coefficients[1..].Count(zero);
    }

    // One bit per zigzag position. Masking DC lets its wider signed range use
    // the same full-vector loads as the AC coefficients without a scalar tail.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong NonzeroAcMask(ReadOnlySpan<short> zigZagCoefficients)
    {
#if !NETSTANDARD2_1
        if (Vector512.IsHardwareAccelerated)
        {
            return NonzeroAcMaskVector512(zigZagCoefficients);
        }

        if (Vector128.IsHardwareAccelerated)
        {
            return Quantization.NonzeroAcMask128(
                Vector128.Create(zigZagCoefficients[..8]),
                Vector128.Create(zigZagCoefficients.Slice(8, 8)),
                Vector128.Create(zigZagCoefficients.Slice(16, 8)),
                Vector128.Create(zigZagCoefficients.Slice(24, 8)),
                Vector128.Create(zigZagCoefficients.Slice(32, 8)),
                Vector128.Create(zigZagCoefficients.Slice(40, 8)),
                Vector128.Create(zigZagCoefficients.Slice(48, 8)),
                Vector128.Create(zigZagCoefficients.Slice(56, 8)));
        }
#endif
        ulong mask = 0;
        for (int position = 1; position < 64; position++)
        {
            if (zigZagCoefficients[position] != 0)
            {
                mask |= 1UL << position;
            }
        }
        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !NETSTANDARD2_1
    internal static ulong NonzeroAcMaskVector512(ReadOnlySpan<short> zigZagCoefficients)
    {
        var lower = Vector512.Create(zigZagCoefficients[..32]);
        var upper = Vector512.Create(zigZagCoefficients.Slice(32, 32));
        uint lowerZeros = (uint)Vector512.ExtractMostSignificantBits(Vector512.Equals(lower, Vector512<short>.Zero));
        uint upperZeros = (uint)Vector512.ExtractMostSignificantBits(Vector512.Equals(upper, Vector512<short>.Zero));
        return (~lowerZeros | ((ulong)~upperZeros << 32)) & ~1UL;
    }

#else
    internal static ulong NonzeroAcMaskVector512(ReadOnlySpan<short> zigZagCoefficients)
        => NonzeroAcMask(zigZagCoefficients);

#endif
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
        {
            ValidateAc(coefficients);
        }
    }

    // Cached blocks call this once before their histogram and emission passes.
    // DC depends on scan order and predictors and is always checked separately.
    internal static void ValidateAc(ReadOnlySpan<short> coefficients)
    {
        if (!AcCoefficientsInRange(coefficients))
        {
            throw new ArgumentOutOfRangeException(nameof(coefficients), "An AC coefficient exceeds baseline JPEG's 10-bit category.");
        }
    }

    internal static bool AcCoefficientsInRange(ReadOnlySpan<short> coefficients)
    {
        if (coefficients.Length != 64)
        {
            throw new ArgumentException("A DCT block must contain 64 coefficients.", nameof(coefficients));
        }

        bool invalid = false;
#if !NETSTANDARD2_1
        int lanes = Vector<short>.Count;
        if (Vector.IsHardwareAccelerated && lanes <= 64 && 64 % lanes == 0)
        {
            var minimum = new Vector<short>(-1023);
            var maximum = new Vector<short>(1023);
            // Load full vectors without a scalar tail. The first lane is DC,
            // whose wider/difference-dependent range must not affect this test.
            var values = Vector.WithElement(new Vector<short>(coefficients), 0, (short)0);
            Vector<short> outside = Vector.LessThan(values, minimum) | Vector.GreaterThan(values, maximum);
            for (int offset = lanes; offset < 64; offset += lanes)
            {
                values = new Vector<short>(coefficients.Slice(offset, lanes));
                outside |= Vector.LessThan(values, minimum) | Vector.GreaterThan(values, maximum);
            }
            invalid = !Vector.EqualsAll(outside, Vector<short>.Zero);
        }
        else
#endif
        {
            for (int offset = 1; offset < 64; offset++)
            {
                invalid |= coefficients[offset] is < -1023 or > 1023;
            }
        }
        return !invalid;
    }
}
