namespace DotJpegli.Internal;

#if !NETSTANDARD2_1
using System.Numerics;
#endif

/// <summary>Bounded offline AC search; never selected by default encoding.</summary>
internal static class RateDistortionQuantization
{
    private static readonly byte[] LumaLengths = CreateLengths(chroma: false);
    private static readonly byte[] ChromaLengths = CreateLengths(chroma: true);

    /// <summary>
    /// Minimizes squared coefficient error plus lambda times standard-table AC bits
    /// over each original value, one step toward zero, and zero. Keeps DC unchanged.
    /// </summary>
    /// <remarks>
    /// The model includes run/category symbols, amplitudes, ZRL and EOB, but excludes
    /// DC, stuffing, padding and table headers. Equal costs prefer fewer changes.
    /// Returns the number of reachable-state candidate evaluations.
    /// </remarks>
    internal static int Optimize(ReadOnlySpan<float> naturalCoefficients, ReadOnlySpan<ushort> table,
        Span<short> zigZagQuantized, double lambda, bool chroma)
        => Optimize(naturalCoefficients, table, zigZagQuantized, lambda, chroma ? ChromaLengths : LumaLengths);

    // The image-specific model can omit symbols; unavailable transitions are
    // excluded while the original, encodable block remains a feasible control.
    internal static int Optimize(ReadOnlySpan<float> naturalCoefficients, ReadOnlySpan<ushort> table,
        Span<short> zigZagQuantized, double lambda, ReadOnlySpan<byte> codeLengths)
    {
        if (naturalCoefficients.Length < 64)
        {
            throw new ArgumentException("A block requires 64 natural coefficients.", nameof(naturalCoefficients));
        }
        if (table.Length < 64)
        {
            throw new ArgumentException("A block requires 64 quantizers.", nameof(table));
        }
        ValidateQuantized(zigZagQuantized);
        ValidateLengths(codeLengths);
        _ = EstimateAcBitsValidated(zigZagQuantized, codeLengths);
        if (double.IsNaN(lambda) || double.IsInfinity(lambda) || lambda < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lambda));
        }
        for (int index = 0; index < 64; index++)
        {
            if (float.IsNaN(naturalCoefficients[index]) || float.IsInfinity(naturalCoefficients[index]))
            {
                throw new ArgumentException("Coefficients must be finite.", nameof(naturalCoefficients));
            }
            if (table[index] is < 1 or > 255)
            {
                throw new ArgumentException("Baseline quantizers must be between 1 and 255.", nameof(table));
            }
        }

        ReadOnlySpan<byte> lengths = codeLengths;
        ReadOnlySpan<int> order = JpegConstants.ZigZag;
        Span<double> costs = stackalloc double[64], nextCosts = stackalloc double[64];
        Span<int> changes = stackalloc int[64], nextChanges = stackalloc int[64];
        Span<byte> previousRuns = stackalloc byte[64 * 64];
        Span<short> selected = stackalloc short[64 * 64];
        Span<short> candidates = stackalloc short[3];
        Span<double> distortions = stackalloc double[3];
        costs.Fill(double.PositiveInfinity);
        changes.Fill(int.MaxValue);
        costs[0] = 0;
        changes[0] = 0;
        int evaluations = 0;
        // Dividing the whole objective by lambda when lambda > 1 keeps finite
        // inputs finite even for the largest accepted double lambda.
        double distortionScale = lambda > 1 ? 1 / lambda : 1;
        double rateScale = lambda > 1 ? 1 : lambda;
        for (int position = 1; position < 64; position++)
        {
            nextCosts.Fill(double.PositiveInfinity);
            nextChanges.Fill(int.MaxValue);
            short original = zigZagQuantized[position];
            int candidateCount = 1;
            candidates[0] = original;
            if (original != 0)
            {
                candidates[candidateCount++] = (short)(original - Math.Sign(original));
                if (Math.Abs((int)original) > 1)
                {
                    candidates[candidateCount++] = 0;
                }
            }
            int natural = order[position];
            for (int candidate = 0; candidate < candidateCount; candidate++)
            {
                double error = naturalCoefficients[natural] - ((double)candidates[candidate] * table[natural]);
                distortions[candidate] = error * error * distortionScale;
            }
            for (int run = 0; run < position; run++)
            {
                if (double.IsPositiveInfinity(costs[run]))
                {
                    continue;
                }
                for (int candidate = 0; candidate < candidateCount; candidate++)
                {
                    short value = candidates[candidate];
                    int nextRun = value == 0 ? run + 1 : 0;
                    int bits = value == 0 ? 0 : NonzeroBits(value, run, lengths);
                    if (bits == int.MaxValue)
                    {
                        continue;
                    }
                    double cost = costs[run] + distortions[candidate] + (bits * rateScale);
                    int changed = changes[run] + (value == original ? 0 : 1);
                    evaluations++;
                    if (cost < nextCosts[nextRun] || (cost == nextCosts[nextRun] && changed < nextChanges[nextRun]))
                    {
                        nextCosts[nextRun] = cost;
                        nextChanges[nextRun] = changed;
                        previousRuns[(position * 64) + nextRun] = (byte)run;
                        selected[(position * 64) + nextRun] = value;
                    }
                }
            }
            Span<double> oldCosts = costs;
            costs = nextCosts;
            nextCosts = oldCosts;
            Span<int> oldChanges = changes;
            changes = nextChanges;
            nextChanges = oldChanges;
        }

        double bestCost = double.PositiveInfinity;
        int bestChanges = int.MaxValue, bestRun = 0;
        for (int run = 0; run < 64; run++)
        {
            if (run != 0 && lengths[0] == 0)
            {
                continue;
            }
            double cost = costs[run] + (run == 0 ? 0 : lengths[0] * rateScale);
            if (cost < bestCost || (cost == bestCost && changes[run] < bestChanges))
            {
                bestCost = cost;
                bestChanges = changes[run];
                bestRun = run;
            }
        }
        for (int position = 63; position >= 1; position--)
        {
            int index = (position * 64) + bestRun;
            zigZagQuantized[position] = selected[index];
            bestRun = previousRuns[index];
        }
        return evaluations;
    }

    /// <summary>Returns standard-table AC symbol and amplitude bits before stuffing and padding.</summary>
    internal static int EstimateAcBits(ReadOnlySpan<short> zigZagQuantized, bool chroma)
        => EstimateAcBits(zigZagQuantized, chroma ? ChromaLengths : LumaLengths);

    internal static int EstimateAcBits(ReadOnlySpan<short> zigZagQuantized, ReadOnlySpan<byte> codeLengths)
    {
        ValidateQuantized(zigZagQuantized);
        ValidateLengths(codeLengths);
        return EstimateAcBitsValidated(zigZagQuantized, codeLengths);
    }

    private static int EstimateAcBitsValidated(ReadOnlySpan<short> zigZagQuantized, ReadOnlySpan<byte> lengths)
    {
        int bits = 0, run = 0;
        for (int position = 1; position < 64; position++)
        {
            short value = zigZagQuantized[position];
            if (value == 0)
            {
                run++;
            }
            else
            {
                int nonzeroBits = NonzeroBits(value, run, lengths);
                if (nonzeroBits == int.MaxValue)
                {
                    throw new ArgumentException("The supplied table cannot encode the original AC block.", nameof(lengths));
                }
                bits += nonzeroBits;
                run = 0;
            }
        }
        if (run != 0 && lengths[0] == 0)
        {
            throw new ArgumentException("The supplied table cannot encode the original block's end-of-block symbol.", nameof(lengths));
        }
        return bits + (run == 0 ? 0 : lengths[0]);
    }

    private static int NonzeroBits(short value, int run, ReadOnlySpan<byte> lengths)
    {
        int category = 32 - BitOperations.LeadingZeroCount((uint)Math.Abs((int)value));
        int symbol = ((run % 16) << 4) | category;
        return lengths[symbol] == 0 || (run >= 16 && lengths[0xf0] == 0) ? int.MaxValue
            : (run / 16 * lengths[0xf0]) + lengths[symbol] + category;
    }

    private static byte[] CreateLengths(bool chroma)
        => CodeLengths(HuffmanTable.CreateDefault(isAc: true, chroma));

    internal static byte[] CodeLengths(HuffmanTable table)
    {
        Guard.ThrowIfNull(table);
        byte[] lengths = new byte[256];
        int symbol = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int count = 0; count < table.Counts[length - 1]; count++)
            {
                lengths[table.Symbols[symbol++]] = (byte)length;
            }
        }
        return lengths;
    }

    private static void ValidateLengths(ReadOnlySpan<byte> codeLengths)
    {
        if (codeLengths.Length != 256)
        {
            throw new ArgumentException("AC code lengths require 256 symbols.", nameof(codeLengths));
        }
        foreach (byte length in codeLengths)
        {
            if (length > 16)
            {
                throw new ArgumentException("Huffman code lengths cannot exceed sixteen bits.", nameof(codeLengths));
            }
        }
    }

    private static void ValidateQuantized(ReadOnlySpan<short> zigZagQuantized)
    {
        if (zigZagQuantized.Length < 64)
        {
            throw new ArgumentException("A block requires 64 zigzag coefficients.", nameof(zigZagQuantized));
        }
        for (int position = 1; position < 64; position++)
        {
            if (zigZagQuantized[position] is < -1023 or > 1023)
            {
                throw new ArgumentException("Baseline AC coefficients must be between -1023 and 1023.", nameof(zigZagQuantized));
            }
        }
    }
}
