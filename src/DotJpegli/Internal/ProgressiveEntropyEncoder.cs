// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace DotJpegli.Internal;

// Progressive scans use the difference, run/category and refinement tokens from
// TokenizeProgressiveDC, TokenizeACProgressiveScan and TokenizeACRefinementScan
// in google/jpegli lib/jpegli/entropy_coding.cc, revision
// 031a0077f5799a6041004267fc12b956c1f52a20, and ITU T.81 Annex G.
// DC and AC are emitted separately. Optimized tables can aggregate AC EOB runs,
// including refinement runs with a bounded buffer of correction bits.
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
        dc.WriteSymbolWithBits(writer, category, AdditionalBits(difference, category), category);
        previousDc = currentDc;
    }

    public static void AccumulateDcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        ref int previousDc,
        long[] dc,
        int successiveLow = 0,
        EntropyTokens? tokens = null,
        int component = 0)
    {
        ValidateFrequencies(dc);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateDc(naturalCoefficients, previousDc, successiveLow);
        int currentDc = naturalCoefficients[0] >> successiveLow;
        int difference = currentDc - previousDc;
        int category = Category(difference);
        checked { dc[category]++; }
        tokens?.AddHuffman(component, false, category, AdditionalBits(difference, category), category);
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
        int successiveLow = 0,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        bool zigZagOrdered = false)
    {
        // The standard AC tables only define the one-block EOB symbol.
        int pendingEobRun = 0;
        WriteAc(writer, naturalCoefficients, ac, ref pendingEobRun, successiveLow, spectralStart, spectralEnd,
            coefficientsValidated, zigZagOrdered);
        FlushAc(writer, ac, ref pendingEobRun);
    }

    public static void WriteAc(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        ref int pendingEobRun,
        int successiveLow = 0,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        bool zigZagOrdered = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateSpectralRange(spectralStart, spectralEnd);
        ValidateAc(naturalCoefficients, coefficientsValidated);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == MaximumEobRun)
        {
            FlushAc(writer, ac, ref pendingEobRun);
        }

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = spectralStart; position <= spectralEnd; position++)
        {
            int value = ShiftAc(naturalCoefficients[zigZagOrdered ? position : order[position]], successiveLow);
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
            ac.WriteSymbolWithBits(writer, (run << 4) | category, AdditionalBits(value, category), category);
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
        int successiveLow = 0,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        int pendingEobRun = 0;
        AccumulateAcFrequencies(naturalCoefficients, ac, ref pendingEobRun, successiveLow, spectralStart, spectralEnd,
            coefficientsValidated, tokens, component, zigZagOrdered);
        FlushAcFrequencies(ac, ref pendingEobRun, tokens, component);
    }

    public static void AccumulateAcFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        ref int pendingEobRun,
        int successiveLow = 0,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        ValidateFrequencies(ac);
        ValidateSuccessiveLow(successiveLow, refinement: false);
        ValidateSpectralRange(spectralStart, spectralEnd);
        ValidateAc(naturalCoefficients, coefficientsValidated);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == MaximumEobRun)
        {
            FlushAcFrequencies(ac, ref pendingEobRun, tokens, component);
        }

        int run = 0;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        for (int position = spectralStart; position <= spectralEnd; position++)
        {
            int value = ShiftAc(naturalCoefficients[zigZagOrdered ? position : order[position]], successiveLow);
            if (value == 0)
            {
                run++;
                continue;
            }

            if (pendingEobRun != 0)
            {
                FlushAcFrequencies(ac, ref pendingEobRun, tokens, component);
            }

            while (run >= 16)
            {
                checked { ac[0xf0]++; }
                tokens?.AddHuffman(component, true, 0xf0, 0, 0);
                run -= 16;
            }

            int category = Category(value);
            int symbol = (run << 4) | category;
            checked { ac[symbol]++; }
            tokens?.AddHuffman(component, true, symbol, AdditionalBits(value, category), category);
            run = 0;
        }

        if (run != 0)
        {
            pendingEobRun++;
            if (pendingEobRun == MaximumEobRun)
            {
                FlushAcFrequencies(ac, ref pendingEobRun, tokens, component);
            }
        }
    }

    public static void WriteAcRefinement(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        int successiveLow,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        bool zigZagOrdered = false)
    {
        // Standard tables only define the one-block EOB symbol.
        var state = new ProgressiveAcRefinementState();
        WriteAcRefinement(writer, naturalCoefficients, ac, state, successiveLow, spectralStart, spectralEnd,
            coefficientsValidated, zigZagOrdered);
        FlushAcRefinement(writer, ac, state);
    }

    public static void WriteAcRefinement(
        BitWriter writer,
        ReadOnlySpan<short> naturalCoefficients,
        HuffmanTable ac,
        ProgressiveAcRefinementState state,
        int successiveLow,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        bool zigZagOrdered = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ArgumentNullException.ThrowIfNull(state);
        ValidateSuccessiveLow(successiveLow, refinement: true);
        ValidateSpectralRange(spectralStart, spectralEnd);
        ValidateAc(naturalCoefficients, coefficientsValidated);
        ProcessAcRefinement(naturalCoefficients, state, successiveLow, spectralStart, spectralEnd, writer, ac, null,
            zigZagOrdered: zigZagOrdered);
    }

    public static void AccumulateAcRefinementFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        int successiveLow,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        var state = new ProgressiveAcRefinementState();
        AccumulateAcRefinementFrequencies(naturalCoefficients, ac, state, successiveLow, spectralStart, spectralEnd,
            coefficientsValidated, tokens, component, zigZagOrdered);
        FlushAcRefinementFrequencies(ac, state, tokens, component);
    }

    public static void AccumulateAcRefinementFrequencies(
        ReadOnlySpan<short> naturalCoefficients,
        long[] ac,
        ProgressiveAcRefinementState state,
        int successiveLow,
        int spectralStart = 1,
        int spectralEnd = 63,
        bool coefficientsValidated = false,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        ValidateFrequencies(ac);
        ArgumentNullException.ThrowIfNull(state);
        ValidateSuccessiveLow(successiveLow, refinement: true);
        ValidateSpectralRange(spectralStart, spectralEnd);
        ValidateAc(naturalCoefficients, coefficientsValidated);
        ProcessAcRefinement(naturalCoefficients, state, successiveLow, spectralStart, spectralEnd, null, null, ac,
            tokens, component, zigZagOrdered);
    }

    // Keep frequency decisions and emitted tokens on the same traversal, since
    // both must split EOB runs at exactly the same buffer and block boundaries.
    private static void ProcessAcRefinement(
        ReadOnlySpan<short> naturalCoefficients,
        ProgressiveAcRefinementState state,
        int successiveLow,
        int spectralStart,
        int spectralEnd,
        BitWriter? writer,
        HuffmanTable? ac,
        long[]? frequencies,
        EntropyTokens? tokens = null,
        int component = 0,
        bool zigZagOrdered = false)
    {
        if (zigZagOrdered)
        {
            ProcessOrderedAcRefinement(naturalCoefficients, state, successiveLow, spectralStart, spectralEnd,
                writer, ac, frequencies, tokens, component);
            return;
        }
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        int lastNew = spectralStart - 1;
        int oldCount = 0;
        for (int position = spectralStart; position <= spectralEnd; position++)
        {
            int magnitude = Math.Abs((int)naturalCoefficients[order[position]]) >> successiveLow;
            if (magnitude == 1)
            {
                lastNew = position;
            }
            else if (magnitude > 1)
            {
                oldCount++;
            }
        }

        // A new coefficient terminates any preceding EOB run. A block without
        // new coefficients contributes all its old-coefficient correction bits,
        // and the run must split before that block if they would exceed capacity.
        if (lastNew >= spectralStart || state.CorrectionBitCount + oldCount > ProgressiveAcRefinementState.MaximumCorrectionBits)
        {
            FlushAcRefinementCore(state, writer, ac, frequencies, tokens, component);
        }

        // Annex G.1.2.3 appends correction bits after a symbol and its sign.
        Span<byte> corrections = stackalloc byte[63];
        int correctionCount = 0;
        int run = 0;
        for (int position = spectralStart; position <= spectralEnd; position++)
        {
            int value = naturalCoefficients[order[position]];
            int magnitude = Math.Abs(value) >> successiveLow;
            if (magnitude == 0)
            {
                run++;
                if (run == 16 && position < lastNew)
                {
                    WriteRefinementSymbol(0xf0, writer, ac, frequencies, tokens: tokens, component: component);
                    WriteCorrectionBits(writer, corrections[..correctionCount], tokens);
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
                WriteRefinementSymbol((run << 4) | 1, writer, ac, frequencies, value > 0 ? 1U : 0U, 1,
                    tokens, component);
                WriteCorrectionBits(writer, corrections[..correctionCount], tokens);
                correctionCount = 0;
                run = 0;
            }
        }

        if (lastNew < spectralEnd)
        {
            state.AppendBlock(corrections[..correctionCount]);
            if (state.PendingEobRun == MaximumEobRun)
            {
                FlushAcRefinementCore(state, writer, ac, frequencies, tokens, component);
            }
        }
    }

    private static void ProcessOrderedAcRefinement(
        ReadOnlySpan<short> coefficients, ProgressiveAcRefinementState state,
        int successiveLow, int spectralStart, int spectralEnd,
        BitWriter? writer, HuffmanTable? ac, long[]? frequencies,
        EntropyTokens? tokens, int component)
    {
        ClassifyOrderedCoefficients(coefficients, successiveLow, out ulong newlySignificant, out ulong previouslySignificant);
        ulong band = (ulong.MaxValue << spectralStart) & (ulong.MaxValue >> (63 - spectralEnd));
        newlySignificant &= band;
        previouslySignificant &= band;
        int lastNew = newlySignificant == 0 ? spectralStart - 1 : BitOperations.Log2(newlySignificant);
        int oldCount = BitOperations.PopCount(previouslySignificant);
        if (lastNew >= spectralStart || state.CorrectionBitCount + oldCount > ProgressiveAcRefinementState.MaximumCorrectionBits)
            FlushAcRefinementCore(state, writer, ac, frequencies, tokens, component);

        Span<byte> corrections = stackalloc byte[63];
        int correctionCount = 0;
        int run = 0;
        int previousPosition = spectralStart - 1;
        ulong occupied = newlySignificant | previouslySignificant;
        while (occupied != 0)
        {
            int position = BitOperations.TrailingZeroCount(occupied);
            occupied &= occupied - 1;
            run += position - previousPosition - 1;
            // Old coefficients do not count as zeros. Discharge a ZRL before
            // the next occupied position, so a correction after the sixteenth
            // zero belongs to the following symbol's correction field.
            while (run >= 16 && position <= lastNew)
            {
                WriteRefinementSymbol(0xf0, writer, ac, frequencies, tokens: tokens, component: component);
                WriteCorrectionBits(writer, corrections[..correctionCount], tokens);
                correctionCount = 0;
                run -= 16;
            }
            int value = coefficients[position];
            if ((previouslySignificant & (1UL << position)) != 0)
                corrections[correctionCount++] = (byte)((Math.Abs(value) >> successiveLow) & 1);
            else
            {
                WriteRefinementSymbol((run << 4) | 1, writer, ac, frequencies, value > 0 ? 1U : 0U, 1,
                    tokens, component);
                WriteCorrectionBits(writer, corrections[..correctionCount], tokens);
                correctionCount = 0;
                run = 0;
            }
            previousPosition = position;
        }
        if (lastNew < spectralEnd)
        {
            state.AppendBlock(corrections[..correctionCount]);
            if (state.PendingEobRun == MaximumEobRun)
                FlushAcRefinementCore(state, writer, ac, frequencies, tokens, component);
        }
    }

    private static void ClassifyOrderedCoefficients(ReadOnlySpan<short> coefficients, int successiveLow,
        out ulong newlySignificant, out ulong previouslySignificant)
    {
        newlySignificant = previouslySignificant = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            var one = Vector256.Create((short)1);
            for (int offset = 0; offset < 64; offset += Vector256<short>.Count)
            {
                var magnitude = Vector256.ShiftRightLogical(Vector256.Abs(Vector256.Create(coefficients[offset..])), successiveLow);
                newlySignificant |= (ulong)Vector256.Equals(magnitude, one).ExtractMostSignificantBits() << offset;
                previouslySignificant |= (ulong)Vector256.GreaterThan(magnitude, one).ExtractMostSignificantBits() << offset;
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            var one = Vector128.Create((short)1);
            for (int offset = 0; offset < 64; offset += Vector128<short>.Count)
            {
                var magnitude = Vector128.ShiftRightLogical(Vector128.Abs(Vector128.Create(coefficients[offset..])), successiveLow);
                newlySignificant |= (ulong)Vector128.Equals(magnitude, one).ExtractMostSignificantBits() << offset;
                previouslySignificant |= (ulong)Vector128.GreaterThan(magnitude, one).ExtractMostSignificantBits() << offset;
            }
        }
        else
        {
            for (int position = 1; position < 64; position++)
            {
                int magnitude = Math.Abs((int)coefficients[position]) >> successiveLow;
                if (magnitude == 1)
                    newlySignificant |= 1UL << position;
                else if (magnitude > 1)
                    previouslySignificant |= 1UL << position;
            }
        }
    }

    public static void FlushAcRefinement(BitWriter writer, HuffmanTable ac, ProgressiveAcRefinementState state)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(ac);
        ArgumentNullException.ThrowIfNull(state);
        FlushAcRefinementCore(state, writer, ac, null);
    }

    public static void FlushAcRefinementFrequencies(long[] ac, ProgressiveAcRefinementState state,
        EntropyTokens? tokens = null, int component = 0)
    {
        ValidateFrequencies(ac);
        ArgumentNullException.ThrowIfNull(state);
        FlushAcRefinementCore(state, null, null, ac, tokens, component);
    }

    private static void FlushAcRefinementCore(
        ProgressiveAcRefinementState state,
        BitWriter? writer,
        HuffmanTable? ac,
        long[]? frequencies,
        EntropyTokens? tokens = null,
        int component = 0)
    {
        if (state.PendingEobRun == 0)
        {
            return;
        }

        int bits = BitOperations.Log2((uint)state.PendingEobRun);
        WriteRefinementSymbol(bits << 4, writer, ac, frequencies,
            (uint)(state.PendingEobRun - (1 << bits)), bits, tokens, component);
        WriteCorrectionBits(writer, state.Corrections, tokens);
        state.Clear();
    }

    private static void WriteRefinementSymbol(int symbol, BitWriter? writer, HuffmanTable? ac, long[]? frequencies,
        uint additionalBits = 0, int bitCount = 0, EntropyTokens? tokens = null, int component = 0)
    {
        if (frequencies is not null)
        {
            checked { frequencies[symbol]++; }
            tokens?.AddHuffman(component, true, symbol, additionalBits, bitCount);
        }
        else
        {
            ac!.WriteSymbolWithBits(writer!, symbol, additionalBits, bitCount);
        }
    }

    private static void WriteCorrectionBits(BitWriter? writer, ReadOnlySpan<byte> corrections, EntropyTokens? tokens = null)
    {
        if (writer is null && tokens?.IsComplete != true)
            return;
        // Eight bytes containing one bit each can be gathered with a multiply.
        // Big-endian loading places the first correction in the highest bit;
        // four such groups supply one complete 32-bit write.
        while (corrections.Length >= 32)
        {
            uint packed = PackCorrectionByte(corrections) << 24
                | PackCorrectionByte(corrections[8..]) << 16
                | PackCorrectionByte(corrections[16..]) << 8
                | PackCorrectionByte(corrections[24..]);
            WriteRawBits(writer, tokens, packed, 32);
            corrections = corrections[32..];
        }
        while (corrections.Length >= 8)
        {
            WriteRawBits(writer, tokens, PackCorrectionByte(corrections), 8);
            corrections = corrections[8..];
        }
        if (corrections.IsEmpty)
            return;
        uint remainder = 0;
        foreach (byte correction in corrections)
        {
            remainder = (remainder << 1) | correction;
        }
        WriteRawBits(writer, tokens, remainder, corrections.Length);
    }

    private static void WriteRawBits(BitWriter? writer, EntropyTokens? tokens, uint bits, int count)
    {
        if (writer is not null)
            writer.WriteBits(bits, count);
        else
            tokens!.AddBits(bits, count);
    }

    private static uint PackCorrectionByte(ReadOnlySpan<byte> corrections)
        => (uint)(unchecked(BinaryPrimitives.ReadUInt64BigEndian(corrections) * 0x0102040810204080UL) >> 56);

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
        ac.WriteSymbolWithBits(writer, bits << 4, (uint)(pendingEobRun - (1 << bits)), bits);
        pendingEobRun = 0;
    }

    public static void FlushAcFrequencies(long[] ac, ref int pendingEobRun, EntropyTokens? tokens = null, int component = 0)
    {
        ValidateFrequencies(ac);
        ValidateEobRun(pendingEobRun);
        if (pendingEobRun == 0)
        {
            return;
        }

        int bits = BitOperations.Log2((uint)pendingEobRun);
        checked { ac[bits << 4]++; }
        tokens?.AddHuffman(component, true, bits << 4, (uint)(pendingEobRun - (1 << bits)), bits);
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

    private static void ValidateSpectralRange(int spectralStart, int spectralEnd)
    {
        if (spectralStart is < 1 or > 63)
            throw new ArgumentOutOfRangeException(nameof(spectralStart), "An AC scan must start at a zigzag position from 1 to 63.");
        if (spectralEnd < spectralStart || spectralEnd > 63)
            throw new ArgumentOutOfRangeException(nameof(spectralEnd), "An AC scan must end at or after its start, at most at position 63.");
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

    private static void ValidateAc(ReadOnlySpan<short> coefficients, bool coefficientsValidated)
    {
        ValidateLength(coefficients);
        if (!coefficientsValidated && !EntropyEncoder.AcCoefficientsInRange(coefficients))
            throw new ArgumentOutOfRangeException(nameof(coefficients), "An AC coefficient exceeds eight-bit JPEG's 10-bit category.");
    }
}
