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
        int tableLimit = 0, HuffmanSymbolOrder symbolOrder = HuffmanSymbolOrder.Canonical)
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
        Clusters dc = Cluster(dcFrequencies, limitToBaseline, tableLimit);
        var acContexts = new List<long[]>();
        int[,] acContextIndexes = new int[scans.Count, componentCount];
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
                acContextIndexes[scanIndex, component] = acContexts.Count;
                acContexts.Add(acFrequencies[scanIndex][component]);
            }
        }
        Clusters ac = Cluster(acContexts, limitToBaseline, tableLimit);
        HuffmanTable[] dcTables = dc.Histograms.Select(histogram => CreateTable(histogram, symbolOrder)).ToArray();
        HuffmanTable[] acTables = ac.Histograms.Select(histogram => CreateTable(histogram, symbolOrder)).ToArray();
        HuffmanTable unused = UnusedTable.Value;
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
            var scanDc = new HuffmanTable[componentCount];
            var scanAc = new HuffmanTable[componentCount];
            Array.Fill(scanDc, unused);
            Array.Fill(scanAc, unused);
            byte[] selectors = new byte[componentCount];
            // Most progressive scans reuse previously defined tables. Avoid
            // allocating an empty list until a later scan actually evicts one.
            List<Definition>? definitions = scanIndex == 0 ? firstDefinitions : null;
            foreach (int component in scan.Components)
            {
                int dcSlot = 0, acSlot = 0;
                if (!progressive || (scan.SpectralStart == 0 && scan.SuccessiveHigh == 0))
                {
                    int cluster = dc.Indexes[component];
                    if (dcTables.Length != 0)
                    {
                        scanDc[component] = dcTables[cluster];
                        dcSlot = dc.Slots[cluster];
                    }
                }
                if (scan.SpectralEnd > 0)
                {
                    int cluster = ac.Indexes[acContextIndexes[scanIndex, component]];
                    if (acTables.Length != 0)
                    {
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
                }
                selectors[component] = (byte)((dcSlot << 4) | acSlot);
                extendedSequential |= !progressive && (dcSlot > 1 || acSlot > 1);
            }
            coding[scanIndex] = new(scanDc, scanAc, selectors, definitions?.ToArray() ?? []);
        }
        return new(coding, extendedSequential);
    }

    private sealed record Clusters(List<long[]> Histograms, int[] Indexes, List<int> Slots);

    private static Clusters Cluster(IReadOnlyList<long[]> histograms, bool forceBaseline, int tableLimit)
    {
        var clusters = new List<long[]>();
        int[] indexes = new int[histograms.Count];
        var slots = new List<int>();
        // JPEG has only four table slots, even when later clusters evict them.
        Span<int> slotHistograms = stackalloc int[4];
        Span<float> slotCosts = stackalloc float[4];
        int slotCount = 0;
        Span<long> combined = stackalloc long[256];
        for (int index = 0; index < histograms.Count; index++)
        {
            long[] current = histograms[index];
            if (current.Length != 256 || current.AsSpan().ContainsAnyInRange(long.MinValue, -1L))
            {
                throw new ArgumentException("Each histogram requires 256 nonnegative frequencies.", nameof(histograms));
            }

            if (!current.AsSpan().ContainsAnyExcept(0L))
            {
                continue;
            }

            int bestSlot = slotCount;
            // Explicit offline limits retain a bounded set of clusters instead
            // of evicting table slots. Zero keeps the existing default policy.
            bool forbidNewCluster = (forceBaseline && bestSlot > 1)
                || (tableLimit != 0 && bestSlot >= tableLimit);
            float bestCost = forbidNewCluster ? float.MaxValue : Cost(current);
            for (int slot = 0; slot < slotCount; slot++)
            {
                Add(clusters[slotHistograms[slot]], current, combined);
                float cost = Cost(combined) - slotCosts[slot];
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestSlot = slot;
                }
            }
            if (bestSlot == slotCount)
            {
                int histogramIndex = clusters.Count;
                clusters.Add((long[])current.Clone());
                indexes[index] = histogramIndex;
                if (bestSlot < slotHistograms.Length)
                {
                    slotCount++;
                }
                else
                {
                    bestSlot = (slots[^1] + 1) % 4;
                }

                slotHistograms[bestSlot] = histogramIndex;
                slotCosts[bestSlot] = bestCost;
                slots.Add(bestSlot);
            }
            else
            {
                int histogramIndex = slotHistograms[bestSlot];
                Add(clusters[histogramIndex], current, clusters[histogramIndex]);
                indexes[index] = histogramIndex;
                slotCosts[bestSlot] += bestCost;
            }
        }
        return new(clusters, indexes, slots);
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

    private static HuffmanTable CreateTable(long[] histogram, HuffmanSymbolOrder order)
    {
        var table = HuffmanTable.CreateOptimized(histogram);
        return order == HuffmanSymbolOrder.Canonical ? table : HuffmanSymbolOrdering.Reorder(table, histogram, order);
    }
}
