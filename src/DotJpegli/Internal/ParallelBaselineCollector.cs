namespace DotJpegli.Internal;

using System.Buffers;
#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.ExceptionServices;

/// <summary>Counts baseline symbols and records ordered tokens with private bounded worker state.</summary>
internal static class ParallelBaselineCollector
{
    internal const int MaximumStagingBytes = 4 * 1024 * 1024;
    private const int MaximumWorkers = 32;
    private const int MinimumBlocksPerWorker = 128;

    internal static bool TryCollect(QuantizedImage image, int width, int height,
        JpegSamplingFactors sampling, JpegScan scan, int restartInterval, int workerLimit,
        long[][] dcFrequencies, long[][] acFrequencies, EntropyTokens tokens,
        CancellationToken cancellationToken, ArrayPool<uint>? stagingPool = null)
    {
        if (workerLimit < 2 || !image.IsComplete || !image.IsZigZagOrdered
            || scan.SpectralStart != 0 || scan.SpectralEnd != 63
            || scan.SuccessiveHigh != 0 || scan.SuccessiveLow != 0)
        {
            return false;
        }
        cancellationToken.ThrowIfCancellationRequested();
        var layout = new Layout(image, width, height, sampling, scan, restartInterval, dcFrequencies.Length);
        int workers = (int)Math.Min(Math.Min(workerLimit, MaximumWorkers),
            (long)layout.McuCount * layout.BlocksPerMcu / MinimumBlocksPerWorker);
        if (workers < 2)
        {
            return false;
        }

        // A baseline block emits at most 64 packed records, and every MCU
        // can contribute one restart. Each accepted private budget is a power
        // of two, and their sum never exceeds the fixed staging allowance.
        long workerBudget = 1L << BitOperations.Log2((ulong)(MaximumStagingBytes / workers));
        int chunkMcus = (int)Math.Min((layout.McuCount + workers - 1) / workers,
            workerBudget / (((layout.BlocksPerMcu * 64L) + 1) * sizeof(uint)));
        if (chunkMcus == 0)
        {
            return false;
        }
        var batch = new Work[workers];
        for (int index = 0; index < batch.Length; index++)
        {
            batch[index] = new(dcFrequencies.Length, layout.Components);
        }
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = workers };
        void Collect(int index)
        {
            Work work = batch[index];
            try
            {
                layout.CollectRange(work.FirstMcu, work.EndMcu, work.Dc, work.Ac, work.Tokens, cancellationToken);
            }
            catch (Exception error)
            {
                work.Error = ExceptionDispatchInfo.Capture(error);
            }
        }
        Action<int> collect = Collect;
        int cursor = 0;
        while (cursor < layout.McuCount)
        {
            int count = 0;
            try
            {
                while (count < batch.Length && cursor < layout.McuCount)
                {
                    Work work = batch[count++];
                    work.Reset(cursor, Math.Min(layout.McuCount, cursor + chunkMcus));
                    if (tokens.IsComplete)
                    {
                        work.Tokens = new(new EntropyTokenBudget(workerBudget), pool: stagingPool);
                    }
                    cursor = work.EndMcu;
                }
                // Bodies capture coding/cancellation failures independently.
                // No shared histogram, token stream or budget is touched until
                // every worker has joined and the caller visits chunk order.
                _ = Parallel.For(0, count, parallelOptions, collect);
                for (int index = 0; index < count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Work work = batch[index];
                    work.Error?.Throw();
                    if (tokens.IsComplete && work.Tokens is not { IsComplete: true })
                    {
                        // An unusual pool bucket may exceed a private budget.
                        // Recollect this chunk directly before merging its
                        // counters, so neither symbols nor tokens are doubled.
                        layout.CollectRange(work.FirstMcu, work.EndMcu, dcFrequencies, acFrequencies,
                            tokens, cancellationToken);
                        continue;
                    }
                    foreach (int component in layout.Components)
                    {
                        for (int symbol = 0; symbol < 256; symbol++)
                        {
                            dcFrequencies[component][symbol] = checked(dcFrequencies[component][symbol] + work.Dc[component][symbol]);
                            acFrequencies[component][symbol] = checked(acFrequencies[component][symbol] + work.Ac[component][symbol]);
                        }
                    }
                    if (tokens.IsComplete)
                    {
                        work.Tokens!.AppendTo(tokens, cancellationToken);
                    }
                }
            }
            finally
            {
                for (int index = 0; index < count; index++)
                {
                    batch[index].Tokens?.Dispose();
                    batch[index].Tokens = null;
                }
            }
        }
        return true;
    }

    private sealed class Work
    {
        internal Work(int componentCount, int[] components)
        {
            this.Dc = new long[componentCount][];
            this.Ac = new long[componentCount][];
            foreach (int component in components)
            {
                this.Dc[component] = new long[256];
                this.Ac[component] = new long[256];
            }
        }

        internal long[][] Dc { get; }
        internal long[][] Ac { get; }
        internal int FirstMcu { get; private set; }
        internal int EndMcu { get; private set; }
        internal EntropyTokens? Tokens { get; set; }
        internal ExceptionDispatchInfo? Error { get; set; }

        internal void Reset(int firstMcu, int endMcu)
        {
            this.FirstMcu = firstMcu;
            this.EndMcu = endMcu;
            this.Error = null;
            for (int component = 0; component < this.Dc.Length; component++)
            {
                if (this.Dc[component] is { } dc)
                {
                    Array.Clear(dc, 0, dc.Length);
                    Array.Clear(this.Ac[component], 0, this.Ac[component].Length);
                }
            }
        }
    }

    private sealed class Layout
    {
        private readonly QuantizedImage image;
        private readonly int restartInterval;
        private readonly int mcuColumns;
        private readonly int[] widths;
        private readonly int[] heights;
        private readonly int[] horizontal;
        private readonly int[] vertical;

        internal Layout(QuantizedImage image, int width, int height, JpegSamplingFactors sampling,
            JpegScan scan, int restartInterval, int componentCount)
        {
            this.image = image;
            this.restartInterval = restartInterval;
            this.Components = scan.Components.ToArray();
            this.widths = new int[componentCount];
            this.heights = new int[componentCount];
            this.horizontal = new int[componentCount];
            this.vertical = new int[componentCount];
            foreach (int component in this.Components)
            {
                JpegComponentSampling factor = sampling[component];
                this.widths[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
                this.heights[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
                this.horizontal[component] = this.Components.Length == 1 ? 1 : factor.Horizontal;
                this.vertical[component] = this.Components.Length == 1 ? 1 : factor.Vertical;
                this.BlocksPerMcu += this.horizontal[component] * this.vertical[component];
            }
            this.mcuColumns = this.Components.Length == 1 ? this.widths[this.Components[0]]
                : (width + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
            int mcuRows = this.Components.Length == 1 ? this.heights[this.Components[0]]
                : (height + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
            this.McuCount = checked(this.mcuColumns * mcuRows);
        }

        internal int[] Components { get; }
        internal int BlocksPerMcu { get; }
        internal int McuCount { get; }

        internal void CollectRange(int firstMcu, int endMcu, long[][] dc, long[][] ac,
            EntropyTokens? tokens, CancellationToken cancellationToken)
        {
            Span<int> previousDc = stackalloc int[3];
            previousDc.Clear();
            if (firstMcu != 0 && (this.restartInterval == 0 || firstMcu % this.restartInterval != 0))
            {
                int previous = firstMcu - 1;
                foreach (int component in this.Components)
                {
                    int x = Math.Min((((previous % this.mcuColumns) + 1) * this.horizontal[component]) - 1, this.widths[component] - 1);
                    int y = Math.Min((((previous / this.mcuColumns) + 1) * this.vertical[component]) - 1, this.heights[component] - 1);
                    previousDc[component] = this.image.GetBlock(component, x, y)[0];
                }
            }
            bool hasBlockMasks = this.image.HasBlockMasks;
            for (int mcu = firstMcu; mcu < endMcu; mcu++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (mcu != 0 && this.restartInterval != 0 && mcu % this.restartInterval == 0)
                {
                    previousDc.Clear();
                    tokens?.AddRestart(((mcu / this.restartInterval) - 1) & 7);
                }
                int mcuX = mcu % this.mcuColumns, mcuY = mcu / this.mcuColumns;
                foreach (int component in this.Components)
                {
                    for (int row = 0; row < this.vertical[component]; row++)
                    {
                        int y = (mcuY * this.vertical[component]) + row;
                        for (int column = 0; column < this.horizontal[component]; column++)
                        {
                            int x = (mcuX * this.horizontal[component]) + column;
                            if (x >= this.widths[component] || y >= this.heights[component])
                            {
                                EntropyEncoder.AccumulateDcOnlyBlock(checked((short)previousDc[component]),
                                    ref previousDc[component], dc[component], ac[component],
                                    tokens: tokens, component: component);
                            }
                            else
                            {
                                ReadOnlySpan<short> block = this.image.GetBlock(component, x, y);
                                if (hasBlockMasks)
                                {
                                    ulong mask = this.image.GetBlockMask(component, x, y);
                                    if (mask == 0)
                                    {
                                        EntropyEncoder.AccumulateDcOnlyBlock(block[0], ref previousDc[component],
                                            dc[component], ac[component], tokens: tokens, component: component);
                                    }
                                    else
                                    {
                                        EntropyEncoder.AccumulateOrderedBlockWithMask(block, mask, ref previousDc[component],
                                            dc[component], ac[component], tokens: tokens, component: component);
                                    }
                                }
                                else
                                {
                                    EntropyEncoder.AccumulateFrequencies(block, ref previousDc[component], dc[component], ac[component],
                                        coefficientsValidated: true, tokens: tokens, component: component, zigZagOrdered: true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
