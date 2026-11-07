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
        int contextCapacity = scans.Count * componentCount;
        long[][] scratchHistograms = new long[componentCount + contextCapacity][];
        Span<long[]> dcHistograms = scratchHistograms.AsSpan(0, componentCount);
        Span<int> dcIndexes = stackalloc int[componentCount];
        Span<int> dcSlots = stackalloc int[componentCount];
        int dcClusterCount = Cluster(dcFrequencies, dcHistograms, dcIndexes, dcSlots,
            limitToBaseline, tableLimit, reuseHistograms);
        dcHistograms = dcHistograms[..dcClusterCount];

        Span<long[]> acHistograms = scratchHistograms.AsSpan(componentCount, contextCapacity);
        Span<int> acContextIndexes = contextCapacity <= 64
            ? stackalloc int[contextCapacity] : new int[contextCapacity];
        int acContextCount = 0;
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
                acContextIndexes[(scanIndex * componentCount) + component] = acContextCount;
                acHistograms[acContextCount++] = acFrequencies[scanIndex][component];
            }
        }
        acHistograms = acHistograms[..acContextCount];
        Span<int> acIndexes = acContextCount <= 64
            ? stackalloc int[acContextCount] : new int[acContextCount];
        Span<int> acSlots = acContextCount <= 64
            ? stackalloc int[acContextCount] : new int[acContextCount];
        int acClusterCount = Cluster(acHistograms, acHistograms, acIndexes, acSlots,
            limitToBaseline, tableLimit, reuseHistograms);
        acHistograms = acHistograms[..acClusterCount];

        HuffmanTable[] dcTables = CreateTables(dcHistograms, symbolOrder);
        HuffmanTable[] acTables = CreateTables(acHistograms, symbolOrder);
        HuffmanTable unused = UnusedTable.Value;
        HuffmanTable[] unusedComponents = componentCount == 1 ? UnusedTable.Single : UnusedTable.Triple;
        int nextAcDefinition = Math.Min(4, acTables.Length);
        int firstDefinitionCount = dcTables.Length + nextAcDefinition;
        Definition[] firstDefinitions = firstDefinitionCount == 0 ? [] : new Definition[firstDefinitionCount];
        for (int index = 0; index < dcTables.Length; index++)
        {
            firstDefinitions[index] = new(dcTables[index], dcSlots[index]);
        }

        for (int index = 0; index < nextAcDefinition; index++)
        {
            firstDefinitions[dcTables.Length + index] = new(acTables[index], 0x10 | acSlots[index]);
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
            List<Definition>? definitions = null;
            foreach (int component in scan.Components)
            {
                int dcSlot = 0, acSlot = 0;
                if (usesDc)
                {
                    int cluster = dcIndexes[component];
                    scanDc[component] = dcTables[cluster];
                    dcSlot = dcSlots[cluster];
                }
                if (usesAc)
                {
                    int cluster = acIndexes[acContextIndexes[(scanIndex * componentCount) + component]];
                    scanAc[component] = acTables[cluster];
                    acSlot = acSlots[cluster];
                    // First use of a new cluster replaces its assigned AC
                    // slot. Clustering never reuses an evicted histogram.
                    while (nextAcDefinition <= cluster)
                    {
                        (definitions ??= []).Add(new(acTables[nextAcDefinition], 0x10 | acSlots[nextAcDefinition]));
                        nextAcDefinition++;
                    }
                }
                selectors[component] = (byte)((dcSlot << 4) | acSlot);
                extendedSequential |= !progressive && (dcSlot > 1 || acSlot > 1);
            }
            coding[scanIndex] = new(scanDc, scanAc, selectors,
                scanIndex == 0 ? firstDefinitions : definitions?.ToArray() ?? []);
        }
        return new(coding, extendedSequential);
    }

    private static int Cluster(ReadOnlySpan<long[]> histograms, Span<long[]> clusters,
        Span<int> indexes, Span<int> slots, bool forceBaseline, int tableLimit, bool reuseHistograms)
    {
        Span<bool> nonEmpty = histograms.Length <= 64 ? stackalloc bool[histograms.Length] : new bool[histograms.Length];
        Span<float> standaloneCosts = histograms.Length <= 64 ? stackalloc float[histograms.Length] : new float[histograms.Length];
        int nonEmptyCount = 0;
        for (int index = 0; index < histograms.Length; index++)
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
            for (int index = 0; index < histograms.Length; index++)
            {
                if (!nonEmpty[index])
                {
                    continue;
                }

                long[] current = histograms[index];
                if (singleCluster is null)
                {
                    singleCluster = reuseHistograms ? current : (long[])current.Clone();
                    clusters[0] = singleCluster;
                    slots[0] = 0;
                }
                else
                {
                    Add(singleCluster, current, singleCluster);
                }
                indexes[index] = 0;
            }
            return singleCluster is null ? 0 : 1;
        }

        // JPEG has only four table slots, even when later clusters evict them.
        Span<int> slotHistograms = stackalloc int[4];
        Span<float> slotCosts = stackalloc float[4];
        Span<int> createdStep = stackalloc int[4];
        Span<int> modifiedStep = stackalloc int[4];
        int slotCount = 0;
        int clusterCount = 0;
        Span<long> combined = stackalloc long[256];
        Span<long> bestCombined = stackalloc long[256];
        for (int index = 0; index < histograms.Length; index++)
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
                int histogramIndex = clusterCount;
                if (bestSlot < slotHistograms.Length)
                {
                    createdStep[bestSlot] = index;
                    modifiedStep[bestSlot] = index;
                    slotCount++;
                }
                else
                {
                    bestSlot = SelectEvictionSlot(histograms, index, nonEmpty, standaloneCosts,
                        clusters, slotHistograms, slotCosts, (slots[clusterCount - 1] + 1) % 4, combined);
                }

                clusters[histogramIndex] = reuseHistograms ? current : (long[])current.Clone();
                indexes[index] = histogramIndex;
                slotHistograms[bestSlot] = histogramIndex;
                slotCosts[bestSlot] = bestCost;
                slots[histogramIndex] = bestSlot;
                clusterCount++;
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
        if (clusterCount is > 1 and <= 4)
        {
            clusterCount = ConsolidateClusters(clusters[..clusterCount], indexes, slotCosts[..clusterCount],
                createdStep[..clusterCount], modifiedStep[..clusterCount], combined, bestCombined);
        }
        return clusterCount;
    }

    private static int SelectEvictionSlot(ReadOnlySpan<long[]> histograms, int currentIndex,
        ReadOnlySpan<bool> nonEmpty, Span<float> standaloneCosts,
        ReadOnlySpan<long[]> clusters, ReadOnlySpan<int> slotHistograms, ReadOnlySpan<float> slotCosts,
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
            for (int futureIndex = currentIndex + 1; futureIndex < histograms.Length; futureIndex++)
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

    private static int ConsolidateClusters(Span<long[]> clusters, Span<int> indexes,
        Span<float> clusterCosts, Span<int> createdStep, Span<int> modifiedStep,
        Span<long> combined, Span<long> bestCombined)
    {
        int clusterCount = clusters.Length;
        while (clusterCount > 1)
        {
            int bestFirst = -1, bestSecond = -1;
            float bestCost = 0f;
            float bestSavings = 0f;
            for (int first = 0; first < clusterCount - 1; first++)
            {
                for (int second = first + 1; second < clusterCount; second++)
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
            for (int index = bestSecond + 1; index < clusterCount; index++)
            {
                clusters[index - 1] = clusters[index];
                clusterCosts[index - 1] = clusterCosts[index];
                createdStep[index - 1] = createdStep[index];
                modifiedStep[index - 1] = modifiedStep[index];
            }
            clusterCount--;
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
        return clusterCount;
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

    private static HuffmanTable[] CreateTables(ReadOnlySpan<long[]> histograms, HuffmanSymbolOrder order)
    {
        if (histograms.IsEmpty)
        {
            return [];
        }
        var tables = new HuffmanTable[histograms.Length];
        for (int index = 0; index < tables.Length; index++)
        {
            tables[index] = HuffmanTable.CreateOptimized(histograms[index], order);
        }

        return tables;
    }
}
