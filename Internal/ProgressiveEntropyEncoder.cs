// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Numerics;

namespace DotJpegli.Internal;

// Initial progressive scans (Ah = Al = 0) use the difference and
// run/category tokens from TokenizeProgressiveDC and
// TokenizeACProgressiveScan in google/jpegli lib/jpegli/entropy_coding.cc, revision
// 031a0077f5799a6041004267fc12b956c1f52a20. DC and AC are emitted separately.
internal static class ProgressiveEntropyEncoder
{
    private const int MaximumEobRun = 0x7fff;

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
        // The standard AC tables only define the one-block EOB symbol.
        int pendingEobRun = 0;
        WriteAc(writer, naturalCoefficients, ac, ref pendingEobRun);
        FlushAc(writer, ac, ref pendingEobRun);
    }

    public static void WriteAc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        ref int pendingEobRun)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
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
            int value = naturalCoefficients[order[position]];
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
        long[] ac)
    {
        int pendingEobRun = 0;
        AccumulateAcFrequencies(naturalCoefficients, ac, ref pendingEobRun);
        FlushAcFrequencies(ac, ref pendingEobRun);
    }

    public static void AccumulateAcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        ref int pendingEobRun)
    {
        ValidateFrequencies(ac);
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
            int value = naturalCoefficients[order[position]];
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

    // Flush before a restart marker or the end of an AC scan. Annex G encodes
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
