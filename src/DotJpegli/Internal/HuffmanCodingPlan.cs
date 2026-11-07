// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// Port of HistogramCost, ClusterJpegHistograms and OptimizeHuffmanCodes in
// google/jpegli lib/jpegli/entropy_coding.cc at revision
// 031a0077f5799a6041004267fc12b956c1f52a20. Histograms, rather than image-sized
// token storage, determine coding tables. The encoder may separately retain a
// budget-limited quantized image to avoid rebuilding coefficients for every scan.
internal sealed class HuffmanCodingPlan
{
    private static class UnusedTable
    {
        // Missing contexts share one immutable fallback, including its lazily
        // prepared replay codes, instead of rebuilding the same tree per image.
        internal static readonly HuffmanTable Value = HuffmanTable.CreateOptimized(new long[256]);
        internal static readonly HuffmanTable[] Single = [Value];
        internal static readonly HuffmanTable[] Triple = [Value, Value, Value];
    }

    internal readonly record struct Definition(HuffmanTable Table, int Selector);
    internal sealed record ScanCoding(HuffmanTable[] DcTables, HuffmanTable[] AcTables,
        byte[] Selectors, Definition[] Definitions);

    private HuffmanCodingPlan(ScanCoding[] scans, bool extendedSequential)
    {
        this.Scans = scans;
        this.RequiresExtendedSequential = extendedSequential;
    }

    internal ScanCoding[] Scans { get; }
    internal bool RequiresExtendedSequential { get; }

    internal static HuffmanCodingPlan Create(IReadOnlyList<JpegScan> scans,
        long[][] dcFrequencies, long[][][] acFrequencies, bool progressive, bool forceBaseline,
        int tableLimit = 0, HuffmanSymbolOrder symbolOrder = HuffmanSymbolOrder.FrequencyDescending,
        bool reuseHistograms = false)
    {
        if (tableLimit is not (0 or 1 or 2 or 4))
        {
            throw new ArgumentOutOfRangeException(nameof(tableLimit));
        }
        int componentCount = dcFrequencies.Length;
        if (componentCount is not (1 or 3) || scans.Count != acFrequencies.Length)
        {
            throw new ArgumentException("Supply component DC and per-scan AC histograms.");
        }

        bool limitToBaseline = !progressive && forceBaseline;
        Clusters dc = Cluster(dcFrequencies, limitToBaseline, tableLimit, reuseHistograms);
        int contextCapacity = scans.Count * componentCount;
        var acContexts = new List<long[]>(contextCapacity);
        Span<int> acContextIndexes = contextCapacity <= 64
            ? stackalloc int[contextCapacity] : new int[contextCapacity];
        for (int scanIndex = 0; scanIndex < scans.Count; scanIndex++)
        {
            if (acFrequencies[scanIndex].Length != componentCount)
            {
                throw new ArgumentException("Each scan requires one histogram per component.", nameof(acFrequencies));
            }

            if (scans[scanIndex].SpectralEnd == 0)
            {
                continue;
            }

            foreach (int component in scans[scanIndex].Components)
            {
                acContextIndexes[(scanIndex * componentCount) + component] = acContexts.Count;
                acContexts.Add(acFrequencies[scanIndex][component]);
            }
        }
        Clusters ac = Cluster(acContexts, limitToBaseline, tableLimit, reuseHistograms);
        HuffmanTable[] dcTables = CreateTables(dc.Histograms, symbolOrder);
        HuffmanTable[] acTables = CreateTables(ac.Histograms, symbolOrder);
        HuffmanTable unused = UnusedTable.Value;
        HuffmanTable[] unusedComponents = componentCount == 1 ? UnusedTable.Single : UnusedTable.Triple;
        int nextAcDefinition = Math.Min(4, acTables.Length);
        var firstDefinitions = new List<Definition>(dcTables.Length + nextAcDefinition);
        for (int index = 0; index < dcTables.Length; index++)
        {
            firstDefinitions.Add(new(dcTables[index], dc.Slots[index]));
        }

        for (int index = 0; index < nextAcDefinition; index++)
        {
            firstDefinitions.Add(new(acTables[index], 0x10 | ac.Slots[index]));
        }

        bool extendedSequential = false;
        var coding = new ScanCoding[scans.Count];
        for (int scanIndex = 0; scanIndex < scans.Count; scanIndex++)
        {
            JpegScan scan = scans[scanIndex];
            bool usesDc = (!progressive || (scan.SpectralStart == 0 && scan.SuccessiveHigh == 0)) && dcTables.Length != 0;
            bool usesAc = scan.SpectralEnd > 0 && acTables.Length != 0;
            HuffmanTable[] scanDc = usesDc ? new HuffmanTable[componentCount] : unusedComponents;
            HuffmanTable[] scanAc = usesAc ? new HuffmanTable[componentCount] : unusedComponents;
            if (usesDc)
            {
                Array.Fill(scanDc, unused);
            }
            if (usesAc)
            {
                Array.Fill(scanAc, unused);
            }
            byte[] selectors = new byte[componentCount];
            // Most progressive scans reuse previously defined tables. Avoid
            // allocating an empty list until a later scan actually evicts one.
            List<Definition>? definitions = scanIndex == 0 ? firstDefinitions : null;
            foreach (int component in scan.Components)
            {
                int dcSlot = 0, acSlot = 0;
                if (usesDc)
                {
                    int cluster = dc.Indexes[component];
                    scanDc[component] = dcTables[cluster];
                    dcSlot = dc.Slots[cluster];
                }
                if (usesAc)
                {
                    int cluster = ac.Indexes[acContextIndexes[(scanIndex * componentCount) + component]];
                    scanAc[component] = acTables[cluster];
                    acSlot = ac.Slots[cluster];
                    // First use of a new cluster replaces its assigned AC
                    // slot. Clustering never reuses an evicted histogram.
                    while (nextAcDefinition <= cluster)
                    {
                        (definitions ??= []).Add(new(acTables[nextAcDefinition], 0x10 | ac.Slots[nextAcDefinition]));
                        nextAcDefinition++;
                    }
                }
                selectors[component] = (byte)((dcSlot << 4) | acSlot);
                extendedSequential |= !progressive && (dcSlot > 1 || acSlot > 1);
            }
            coding[scanIndex] = new(scanDc, scanAc, selectors, definitions?.ToArray() ?? []);
        }
        return new(coding, extendedSequential);
    }

    private sealed record Clusters(List<long[]> Histograms, int[] Indexes, List<int> Slots);

    private static Clusters Cluster(IReadOnlyList<long[]> histograms, bool forceBaseline, int tableLimit, bool reuseHistograms)
    {
        var clusters = new List<long[]>();
        int[] indexes = new int[histograms.Count];
        var slots = new List<int>();
        Span<bool> nonEmpty = histograms.Count <= 64 ? stackalloc bool[histograms.Count] : new bool[histograms.Count];
        Span<float> standaloneCosts = histograms.Count <= 64 ? stackalloc float[histograms.Count] : new float[histograms.Count];
        int nonEmptyCount = 0;
        for (int index = 0; index < histograms.Count; index++)
        {
            ReadOnlySpan<long> current = histograms[index];
            if (current.Length != 256)
            {
                throw new ArgumentException("Each histogram requires 256 nonnegative frequencies.", nameof(histograms));
            }

            bool hasNonZero = current.ContainsAnyExcept(0L);
            if (hasNonZero && current.ContainsAnyInRange(long.MinValue, -1L))
            {
                throw new ArgumentException("Each histogram requires 256 nonnegative frequencies.", nameof(histograms));
            }

            nonEmpty[index] = hasNonZero;
            if (hasNonZero)
            {
                nonEmptyCount++;
            }
            standaloneCosts[index] = -1f;
        }

        if (nonEmptyCount <= 1 || tableLimit == 1)
        {
            long[]? singleCluster = null;
            for (int index = 0; index < histograms.Count; index++)
            {
                if (!nonEmpty[index])
                {
                    continue;
                }

                long[] current = histograms[index];
                if (singleCluster is null)
                {
                    singleCluster = reuseHistograms ? current : (long[])current.Clone();
                    clusters.Add(singleCluster);
                    slots.Add(0);
                }
                else
                {
                    Add(singleCluster, current, singleCluster);
                }
                indexes[index] = 0;
            }
            return new(clusters, indexes, slots);
        }

        // JPEG has only four table slots, even when later clusters evict them.
        Span<int> slotHistograms = stackalloc int[4];
        Span<float> slotCosts = stackalloc float[4];
        Span<int> createdStep = stackalloc int[4];
        Span<int> modifiedStep = stackalloc int[4];
        int slotCount = 0;
        Span<long> combined = stackalloc long[256];
        Span<long> bestCombined = stackalloc long[256];
        for (int index = 0; index < histograms.Count; index++)
        {
            if (!nonEmpty[index])
            {
                continue;
            }

            long[] current = histograms[index];
            int bestSlot = slotCount;
            // Explicit offline limits retain a bounded set of clusters instead
            // of evicting table slots. Zero keeps the existing default policy.
            bool forbidNewCluster = (forceBaseline && bestSlot > 1)
                || (tableLimit != 0 && bestSlot >= tableLimit);
            float bestCost = float.MaxValue;
            if (!forbidNewCluster)
            {
                bestCost = standaloneCosts[index];
                if (bestCost < 0f)
                {
                    standaloneCosts[index] = bestCost = Cost(current);
                }
            }

            float bestMergedCost = 0f;
            for (int slot = 0; slot < slotCount; slot++)
            {
                Add(clusters[slotHistograms[slot]], current, combined);
                float mergedCost = Cost(combined);
                float cost = mergedCost - slotCosts[slot];
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestMergedCost = mergedCost;
                    bestSlot = slot;
                    Span<long> previous = bestCombined;
                    bestCombined = combined;
                    combined = previous;
                }
            }
            if (bestSlot == slotCount)
            {
                int histogramIndex = clusters.Count;
                clusters.Add(reuseHistograms ? current : (long[])current.Clone());
                indexes[index] = histogramIndex;
                if (bestSlot < slotHistograms.Length)
                {
                    createdStep[bestSlot] = index;
                    modifiedStep[bestSlot] = index;
                    slotCount++;
                }
                else
                {
                    bestSlot = SelectEvictionSlot(histograms, index, nonEmpty, standaloneCosts,
                        clusters, slotHistograms, slotCosts, (slots[^1] + 1) % 4, combined);
                }

                slotHistograms[bestSlot] = histogramIndex;
                slotCosts[bestSlot] = bestCost;
                slots.Add(bestSlot);
            }
            else
            {
                int histogramIndex = slotHistograms[bestSlot];
                bestCombined.CopyTo(clusters[histogramIndex]);
                indexes[index] = histogramIndex;
                slotCosts[bestSlot] = bestMergedCost;
                modifiedStep[bestSlot] = index;
            }
        }
        if (clusters.Count is > 1 and <= 4)
        {
            ConsolidateClusters(clusters, indexes, slots, slotCosts[..clusters.Count],
                createdStep[..clusters.Count], modifiedStep[..clusters.Count], combined, bestCombined);
        }
        return new(clusters, indexes, slots);
    }

    private static int SelectEvictionSlot(IReadOnlyList<long[]> histograms, int currentIndex,
        ReadOnlySpan<bool> nonEmpty, Span<float> standaloneCosts,
        List<long[]> clusters, ReadOnlySpan<int> slotHistograms, ReadOnlySpan<float> slotCosts,
        int defaultSlot, Span<long> scratch)
    {
        int bestSlot = defaultSlot;
        float lowestFutureSavings = float.MaxValue;
        for (int offset = 0; offset < 4; offset++)
        {
            int slot = (defaultSlot + offset) & 3;
            long[] cluster = clusters[slotHistograms[slot]];
            float slotCost = slotCosts[slot];
            float futureSavings = 0f;
            for (int futureIndex = currentIndex + 1; futureIndex < histograms.Count; futureIndex++)
            {
                if (!nonEmpty[futureIndex])
                {
                    continue;
                }

                long[] future = histograms[futureIndex];
                float standaloneCost = standaloneCosts[futureIndex];
                if (standaloneCost < 0f)
                {
                    standaloneCosts[futureIndex] = standaloneCost = Cost(future);
                }

                Add(cluster, future, scratch);
                float mergeDelta = Cost(scratch) - slotCost;
                if (mergeDelta < standaloneCost)
                {
                    futureSavings += standaloneCost - mergeDelta;
                    if (futureSavings >= lowestFutureSavings)
                    {
                        break;
                    }
                }
            }
            if (futureSavings < lowestFutureSavings)
            {
                lowestFutureSavings = futureSavings;
                bestSlot = slot;
                if (futureSavings == 0f)
                {
                    break;
                }
            }
        }
        return bestSlot;
    }

    private static void ConsolidateClusters(List<long[]> clusters, int[] indexes, List<int> slots,
        Span<float> clusterCosts, Span<int> createdStep, Span<int> modifiedStep,
        Span<long> combined, Span<long> bestCombined)
    {
        while (clusters.Count > 1)
        {
            int bestFirst = -1, bestSecond = -1;
            float bestCost = 0f;
            float bestSavings = 0f;
            for (int first = 0; first < clusters.Count - 1; first++)
            {
                for (int second = first + 1; second < clusters.Count; second++)
                {
                    // When neither cluster changed after the later cluster was
                    // created, the main pass already tested this exact pair and
                    // found no savings.
                    if (modifiedStep[first] <= createdStep[second]
                        && modifiedStep[second] <= createdStep[second])
                    {
                        continue;
                    }

                    Add(clusters[first], clusters[second], combined);
                    float mergedCost = Cost(combined);
                    float savings = clusterCosts[first] + clusterCosts[second] - mergedCost;
                    if (savings > bestSavings)
                    {
                        bestSavings = savings;
                        bestCost = mergedCost;
                        bestFirst = first;
                        bestSecond = second;
                        Span<long> swap = bestCombined;
                        bestCombined = combined;
                        combined = swap;
                    }
                }
            }
            if (bestFirst < 0)
            {
                break;
            }

            bestCombined.CopyTo(clusters[bestFirst]);
            clusterCosts[bestFirst] = bestCost;
            modifiedStep[bestFirst] = int.MaxValue;
            for (int index = bestSecond + 1; index < clusters.Count; index++)
            {
                clusterCosts[index - 1] = clusterCosts[index];
                createdStep[index - 1] = createdStep[index];
                modifiedStep[index - 1] = modifiedStep[index];
            }

            clusters.RemoveAt(bestSecond);
            slots.RemoveAt(slots.Count - 1);
            for (int index = 0; index < indexes.Length; index++)
            {
                if (indexes[index] == bestSecond)
                {
                    indexes[index] = bestFirst;
                }
                else if (indexes[index] > bestSecond)
                {
                    indexes[index]--;
                }
            }
        }
    }

    private static void Add(ReadOnlySpan<long> left, ReadOnlySpan<long> right, Span<long> sum)
    {
        for (int symbol = 0; symbol < sum.Length; symbol++)
        {
            sum[symbol] = checked(left[symbol] + right[symbol]);
        }
    }

    private static float Cost(ReadOnlySpan<long> frequencies)
        => HuffmanTable.EstimateOptimizedCost(frequencies);

    private static HuffmanTable[] CreateTables(List<long[]> histograms, HuffmanSymbolOrder order)
    {
        var tables = new HuffmanTable[histograms.Count];
        for (int index = 0; index < tables.Length; index++)
        {
            tables[index] = HuffmanTable.CreateOptimized(histograms[index], order);
        }

        return tables;
    }
}
