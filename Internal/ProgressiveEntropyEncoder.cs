// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Numerics;

namespace DotJpegli.Internal;

// Progressive scans use the difference, run/category and refinement tokens from
// TokenizeProgressiveDC, TokenizeACProgressiveScan and TokenizeACRefinementScan
// in google/jpegli lib/jpegli/entropy_coding.cc, revision
// 031a0077f5799a6041004267fc12b956c1f52a20, and ITU T.81 Annex G.
// DC and AC are emitted separately; refinement EOBs are limited to one block.
internal static class ProgressiveEntropyEncoder
{
    private const int MaximumEobRun = 0x7fff;

    public static void WriteDc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        HuffmanTable dc,
        int successiveLow = 0)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(dc);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateDc(naturalCoefficients, previousDc, successiveLow);

        int currentDc = naturalCoefficients[0] >> successiveLow;
        int difference = currentDc - previousDc;
        int category = Category(difference);
        dc.WriteSymbol(writer, category);
        writer.WriteBits(AdditionalBits(difference, category), category);
        previousDc = currentDc;
    }

    public static void AccumulateDcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        long[] dc,
        int successiveLow = 0)
    {
        ValidateFrequencies(dc);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateDc(naturalCoefficients, previousDc, successiveLow);
        int currentDc = naturalCoefficients[0] >> successiveLow;
        checked { dc[Category(currentDc - previousDc)]++; }
        previousDc = currentDc;
    }

    public static void WriteDcRefinement(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        int successiveLow)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ValidateSuccessiveLow(successiveLow, refinement: true);
        ValidateLength(naturalCoefficients);
        // Unlike AC magnitudes, DC is refined in its signed representation.
        writer.WriteBits((uint)((naturalCoefficients[0] >> successiveLow) & 1), 1);
    }

    public static void WriteAc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        int successiveLow = 0)
    {
        // The standard AC tables only define the one-block EOB symbol.
        int pendingEobRun = 0;
        WriteAc(writer, naturalCoefficients, ac, ref pendingEobRun, successiveLow);
        FlushAc(writer, ac, ref pendingEobRun);
    }

    public static void WriteAc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        ref int pendingEobRun,
        int successiveLow = 0)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateAc(naturalCoefficients);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == MaximumEobRun)
        {
            FlushAc(writer, ac, ref pendingEobRun);
        }

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            int value = ShiftAc(naturalCoefficients[order[position]], successiveLow);
            if (value == 0)
            {
                run++;
                continue;
            }

            if (pendingEobRun != 0)
            {
                FlushAc(writer, ac, ref pendingEobRun);
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
            pendingEobRun++;
            if (pendingEobRun == MaximumEobRun)
            {
                FlushAc(writer, ac, ref pendingEobRun);
            }
        }
    }

    public static void AccumulateAcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        int successiveLow = 0)
    {
        int pendingEobRun = 0;
        AccumulateAcFrequencies(naturalCoefficients, ac, ref pendingEobRun, successiveLow);
        FlushAcFrequencies(ac, ref pendingEobRun);
    }

    public static void AccumulateAcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        ref int pendingEobRun,
        int successiveLow = 0)
    {
        ValidateFrequencies(ac);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateAc(naturalCoefficients);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == MaximumEobRun)
        {
            FlushAcFrequencies(ac, ref pendingEobRun);
        }

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            int value = ShiftAc(naturalCoefficients[order[position]], successiveLow);
            if (value == 0)
            {
                run++;
                continue;
            }

            if (pendingEobRun != 0)
            {
                FlushAcFrequencies(ac, ref pendingEobRun);
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
            pendingEobRun++;
            if (pendingEobRun == MaximumEobRun)
            {
                FlushAcFrequencies(ac, ref pendingEobRun);
            }
        }
    }

    public static void WriteAcRefinement(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        int successiveLow)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateSuccessiveLow(successiveLow, refinement: true);
        ValidateAc(naturalCoefficients);

        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        int lastNew = 0;
        for (int position = 1; position < 64; position++)
        {
            if ((Math.Abs((int)naturalCoefficients[order[position]]) >> successiveLow) == 1)
            {
                lastNew = position;
            }
        }

        // Annex G.1.2.3 appends correction bits after a symbol and its sign.
        // A one-block EOB keeps buffering bounded, including with optimized tables.
        Span<byte> corrections = stackalloc byte[63];
        int correctionCount = 0;
        int run = 0;
        for (int position = 1; position < 64; position++)
        {
            int value = naturalCoefficients[order[position]];
            int magnitude = Math.Abs(value) >> successiveLow;
            if (magnitude == 0)
            {
                run++;
                if (run == 16 && position < lastNew)
                {
                    ac.WriteSymbol(writer, 0xf0);
                    WriteCorrectionBits(writer, corrections[..correctionCount]);
                    correctionCount = 0;
                    run = 0;
                }
            }
            else if (magnitude > 1)
            {
                // Previously significant coefficients do not count toward the run.
                corrections[correctionCount++] = (byte)(magnitude & 1);
            }
            else
            {
                ac.WriteSymbol(writer, (run << 4) | 1);
                writer.WriteBits(value > 0 ? 1U : 0U, 1);
                WriteCorrectionBits(writer, corrections[..correctionCount]);
                correctionCount = 0;
                run = 0;
            }
        }

        if (lastNew < 63)
        {
            ac.WriteSymbol(writer, 0);
            WriteCorrectionBits(writer, corrections[..correctionCount]);
        }
    }

    public static void AccumulateAcRefinementFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        int successiveLow)
    {
        ValidateFrequencies(ac);
        ValidateSuccessiveLow(successiveLow, refinement: true);
        ValidateAc(naturalCoefficients);

        int run = 0;
        int lastNew = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = 1; position < 64; position++)
        {
            int magnitude = Math.Abs((int)naturalCoefficients[order[position]]) >> successiveLow;
            if (magnitude == 0)
            {
                run++;
            }
            else if (magnitude == 1)
            {
                while (run >= 16)
                {
                    checked { ac[0xf0]++; }
                    run -= 16;
                }

                checked { ac[(run << 4) | 1]++; }
                run = 0;
                lastNew = position;
            }
        }

        if (lastNew < 63)
        {
            checked { ac[0]++; }
        }
    }

    private static void WriteCorrectionBits(BitWriter writer, ReadOnlySpan<byte> corrections)
    {
        foreach (byte correction in corrections)
        {
            writer.WriteBits(correction, 1);
        }
    }

    // Flush before a restart marker or the end of an initial AC scan. Annex G encodes
    // a run as floor(log2(run)) in the symbol's high nibble followed by the
    // remaining low bits. 0xf0 is reserved for ZRL, limiting runs to 32767.
    public static void FlushAc(BitWriter writer, HuffmanTable ac, ref int pendingEobRun)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == 0)
        {
            return;
        }

        int bits = BitOperations.Log2((uint)pendingEobRun);
        ac.WriteSymbol(writer, bits << 4);
        writer.WriteBits((uint)(pendingEobRun - (1 << bits)), bits);
        pendingEobRun = 0;
    }

    public static void FlushAcFrequencies(long[] ac, ref int pendingEobRun)
    {
        ValidateFrequencies(ac);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == 0)
        {
            return;
        }

        int bits = BitOperations.Log2((uint)pendingEobRun);
        checked { ac[bits << 4]++; }
        pendingEobRun = 0;
    }

    private static void ValidateEobRun(int pendingEobRun)
    {
        if ((uint)pendingEobRun > MaximumEobRun)
        {
            throw new ArgumentOutOfRangeException(nameof(pendingEobRun), "An EOB run must contain 0 to 32767 blocks.");
        }
    }

    private static int Category(int value) =>
        value == 0 ? 0 : BitOperations.Log2((uint)Math.Abs(value)) + 1;

    private static uint AdditionalBits(int value, int category) =>
        unchecked((uint)(value < 0 ? value - 1 : value)) & ((1U << category) - 1);

    private static int ShiftAc(int value, int successiveLow) =>
        value < 0 ? -(-value >> successiveLow) : value >> successiveLow;

    private static void ValidateSuccessiveLow(int successiveLow, bool refinement)
    {
        int maximum = refinement ? 9 : 10;
        if ((uint)successiveLow > maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(successiveLow),
                $"The successive approximation bit must be between 0 and {maximum}.");
        }
    }

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

    private static void ValidateDc(ReadOnlySpan<short> coefficients, int previousDc, int successiveLow)
    {
        ValidateLength(coefficients);
        long difference = (long)(coefficients[0] >> successiveLow) - previousDc;
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
