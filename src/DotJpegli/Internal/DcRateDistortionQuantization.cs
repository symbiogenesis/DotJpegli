namespace DotJpegli.Internal;

#if !NETSTANDARD2_1
using System.Numerics;
#endif

/// <summary>Bounded research-only DC chain search in actual entropy traversal order.</summary>
internal static class DcRateDistortionQuantization
{
    internal const int MaximumBlocks = 65536;
    private static readonly byte[] LumaLengths = CodeLengths(HuffmanTable.CreateDefault(false, false));
    private static readonly byte[] ChromaLengths = CodeLengths(HuffmanTable.CreateDefault(false, true));

    internal readonly record struct Diagnostics(int Evaluations, int ChangedCoefficients,
        int OriginalBits, int SelectedBits, double OriginalSquaredError, double SelectedSquaredError);

    internal static ReadOnlySpan<byte> StandardCodeLengths(bool chroma) => chroma ? ChromaLengths : LumaLengths;

    internal static byte[] CodeLengths(HuffmanTable table)
    {
        Guard.ThrowIfNull(table);
        byte[] lengths = new byte[12];
        int offset = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int index = 0; index < table.Counts[length - 1]; index++)
            {
                int symbol = table.Symbols[offset++];
                if (symbol < lengths.Length)
                {
                    lengths[symbol] = (byte)length;
                }
            }
        }
        return lengths;
    }

    /// <summary>
    /// Minimizes coefficient squared error plus lambda times DC symbol/amplitude
    /// bits over each original quantized DC and its two adjacent integers.
    /// </summary>
    /// <remarks>
    /// The caller supplies entropy order and predictor resets before blocks;
    /// the first predictor is always zero. AC, stuffing, padding, DHT cost and
    /// perceptual error are excluded. Zero-length symbols are unavailable.
    /// Ties prefer fewer coefficient changes, then deterministic candidate order.
    /// Scratch is exactly three backpointer bytes per block, at most 192 KiB.
    /// </remarks>
    internal static Diagnostics Optimize(ReadOnlySpan<float> sourceDc, ushort quantizer,
        Span<short> quantizedDc, ReadOnlySpan<byte> codeLengths, ReadOnlySpan<bool> resetBeforeBlock,
        double lambda, CancellationToken cancellationToken = default)
    {
        if (sourceDc.Length != quantizedDc.Length)
        {
            throw new ArgumentException("Each DC value requires a source coefficient.", nameof(sourceDc));
        }
        if (quantizer is < 1 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(quantizer));
        }
        if (!double.IsFinite(lambda) || lambda < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lambda));
        }
        Validate(quantizedDc, codeLengths, resetBeforeBlock);
        foreach (float source in sourceDc)
        {
            if (!float.IsFinite(source))
            {
                throw new ArgumentException("DC source coefficients must be finite.", nameof(sourceDc));
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        int originalBits = EstimateBitsValidated(quantizedDc, codeLengths, resetBeforeBlock);
        double originalError = SquaredError(sourceDc, quantizer, quantizedDc);
        if (quantizedDc.IsEmpty)
        {
            return default;
        }
        byte[] previous = new byte[quantizedDc.Length * 3];
        Span<double> costs = stackalloc double[3], nextCosts = stackalloc double[3];
        Span<int> changes = stackalloc int[3], nextChanges = stackalloc int[3];
        costs.Fill(double.PositiveInfinity);
        costs[0] = 0;
        changes.Clear();
        double distortionScale = lambda > 1 ? 1 / lambda : 1;
        double rateScale = lambda > 1 ? 1 : lambda;
        int evaluations = 0;
        for (int block = 0; block < quantizedDc.Length; block++)
        {
            if ((block & 1023) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            nextCosts.Fill(double.PositiveInfinity);
            nextChanges.Fill(int.MaxValue);
            short original = quantizedDc[block];
            int candidates = CandidateCount(original);
            int predecessors = block == 0 ? 1 : CandidateCount(quantizedDc[block - 1]);
            for (int candidate = 0; candidate < candidates; candidate++)
            {
                short value = Candidate(original, candidate);
                double error = sourceDc[block] - ((double)value * quantizer);
                double distortion = error * error * distortionScale;
                for (int predecessor = 0; predecessor < predecessors; predecessor++)
                {
                    if (double.IsPositiveInfinity(costs[predecessor]))
                    {
                        continue;
                    }
                    int predictor = block == 0 || resetBeforeBlock[block] ? 0
                        : Candidate(quantizedDc[block - 1], predecessor);
                    int category = Category(value - predictor);
                    if (codeLengths[category] == 0)
                    {
                        continue;
                    }
                    double cost = costs[predecessor] + distortion + ((category + codeLengths[category]) * rateScale);
                    int changed = changes[predecessor] + (value == original ? 0 : 1);
                    evaluations++;
                    if (cost < nextCosts[candidate]
                        || (cost == nextCosts[candidate] && changed < nextChanges[candidate]))
                    {
                        nextCosts[candidate] = cost;
                        nextChanges[candidate] = changed;
                        previous[(block * 3) + candidate] = (byte)predecessor;
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
        int best = 0;
        for (int candidate = 1; candidate < CandidateCount(quantizedDc[^1]); candidate++)
        {
            if (costs[candidate] < costs[best]
                || (costs[candidate] == costs[best] && changes[candidate] < changes[best]))
            {
                best = candidate;
            }
        }
        int changedCoefficients = changes[best];
        cancellationToken.ThrowIfCancellationRequested();
        for (int block = quantizedDc.Length - 1; block >= 0; block--)
        {
            quantizedDc[block] = Candidate(quantizedDc[block], best);
            best = previous[(block * 3) + best];
        }
        return new(evaluations, changedCoefficients, originalBits,
            EstimateBitsValidated(quantizedDc, codeLengths, resetBeforeBlock), originalError,
            SquaredError(sourceDc, quantizer, quantizedDc));
    }

    internal static int EstimateBits(ReadOnlySpan<short> quantizedDc, ReadOnlySpan<byte> codeLengths,
        ReadOnlySpan<bool> resetBeforeBlock)
    {
        Validate(quantizedDc, codeLengths, resetBeforeBlock);
        return EstimateBitsValidated(quantizedDc, codeLengths, resetBeforeBlock);
    }

    private static void Validate(ReadOnlySpan<short> quantizedDc, ReadOnlySpan<byte> codeLengths,
        ReadOnlySpan<bool> resetBeforeBlock)
    {
        if (quantizedDc.Length > MaximumBlocks)
        {
            throw new ArgumentException("The DC chain exceeds the bounded research block limit.", nameof(quantizedDc));
        }
        if (resetBeforeBlock.Length != quantizedDc.Length)
        {
            throw new ArgumentException("Each DC block requires a predictor-reset flag.", nameof(resetBeforeBlock));
        }
        if (codeLengths.Length != 12)
        {
            throw new ArgumentException("DC code lengths require categories zero through eleven.", nameof(codeLengths));
        }
        if (codeLengths.ContainsAnyExceptInRange((byte)0, (byte)16))
        {
            throw new ArgumentException("Huffman code lengths cannot exceed sixteen bits.", nameof(codeLengths));
        }
        int predictor = 0;
        for (int block = 0; block < quantizedDc.Length; block++)
        {
            short value = quantizedDc[block];
            if (value is < -1024 or > 1023)
            {
                throw new ArgumentException("Baseline DC values must fit the signed eleven-bit range.", nameof(quantizedDc));
            }
            if (resetBeforeBlock[block])
            {
                predictor = 0;
            }
            if (codeLengths[Category(value - predictor)] == 0)
            {
                throw new ArgumentException("The supplied table cannot encode the original DC chain.", nameof(codeLengths));
            }
            predictor = value;
        }
    }

    private static int EstimateBitsValidated(ReadOnlySpan<short> quantizedDc, ReadOnlySpan<byte> codeLengths,
        ReadOnlySpan<bool> resetBeforeBlock)
    {
        int bits = 0, predictor = 0;
        for (int block = 0; block < quantizedDc.Length; block++)
        {
            if (resetBeforeBlock[block])
            {
                predictor = 0;
            }
            int category = Category(quantizedDc[block] - predictor);
            bits += category + codeLengths[category];
            predictor = quantizedDc[block];
        }
        return bits;
    }

    private static double SquaredError(ReadOnlySpan<float> sourceDc, ushort quantizer, ReadOnlySpan<short> quantizedDc)
    {
        double total = 0;
        for (int block = 0; block < quantizedDc.Length; block++)
        {
            double error = sourceDc[block] - ((double)quantizedDc[block] * quantizer);
            total += error * error;
        }
        return total;
    }

    private static int CandidateCount(short original) => original is -1024 or 1023 ? 2 : 3;
    private static short Candidate(short original, int index)
        => index == 0 ? original : (short)(original + (index == 1 && original != -1024 ? -1 : 1));
    private static int Category(int difference)
        => 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(difference));
}
