// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// Port of HistogramCost, ClusterJpegHistograms and OptimizeHuffmanCodes in
// google/jpegli lib/jpegli/entropy_coding.cc at revision
// 031a0077f5799a6041004267fc12b956c1f52a20. Histograms, rather than image-sized
// coefficient/token storage, are retained between the encoder's two passes.
internal sealed class HuffmanCodingPlan
{
    internal readonly record struct Definition(HuffmanTable Table, int Selector);
    internal sealed record ScanCoding(HuffmanTable[] DcTables, HuffmanTable[] AcTables,
        byte[] Selectors, Definition[] Definitions);

    private HuffmanCodingPlan(ScanCoding[] scans, bool extendedSequential)
    {
        Scans = scans;
        RequiresExtendedSequential = extendedSequential;
    }

    internal ScanCoding[] Scans { get; }
    internal bool RequiresExtendedSequential { get; }

    internal static HuffmanCodingPlan Create(IReadOnlyList<JpegScan> scans,
        long[][] dcFrequencies, long[][][] acFrequencies, bool progressive, bool forceBaseline)
    {
        int componentCount = dcFrequencies.Length;
        if (componentCount is not (1 or 3) || scans.Count != acFrequencies.Length)
            throw new ArgumentException("Supply component DC and per-scan AC histograms.");
        bool limitToBaseline = !progressive && forceBaseline;
        Clusters dc = Cluster(dcFrequencies, limitToBaseline);
        var acContexts = new List<long[]>();
        var acContextIndexes = new int[scans.Count, componentCount];
        for (int scanIndex = 0; scanIndex < scans.Count; scanIndex++)
        {
            if (acFrequencies[scanIndex].Length != componentCount)
                throw new ArgumentException("Each scan requires one histogram per component.", nameof(acFrequencies));
            if (scans[scanIndex].SpectralEnd == 0)
                continue;
            foreach (int component in scans[scanIndex].Components)
            {
                acContextIndexes[scanIndex, component] = acContexts.Count;
                acContexts.Add(acFrequencies[scanIndex][component]);
            }
        }
        Clusters ac = Cluster(acContexts, limitToBaseline);
        HuffmanTable[] dcTables = dc.Histograms.Select(histogram => HuffmanTable.CreateOptimized(histogram)).ToArray();
        HuffmanTable[] acTables = ac.Histograms.Select(histogram => HuffmanTable.CreateOptimized(histogram)).ToArray();
        HuffmanTable unused = HuffmanTable.CreateOptimized(new long[256]);
        int nextAcDefinition = Math.Min(4, acTables.Length);
        var firstDefinitions = new List<Definition>();
        for (int index = 0; index < dcTables.Length; index++)
            firstDefinitions.Add(new(dcTables[index], dc.Slots[index]));
        for (int index = 0; index < nextAcDefinition; index++)
            firstDefinitions.Add(new(acTables[index], 0x10 | ac.Slots[index]));

        bool extendedSequential = false;
        var coding = new ScanCoding[scans.Count];
        for (int scanIndex = 0; scanIndex < scans.Count; scanIndex++)
        {
            JpegScan scan = scans[scanIndex];
            var scanDc = Enumerable.Repeat(unused, componentCount).ToArray();
            var scanAc = Enumerable.Repeat(unused, componentCount).ToArray();
            var selectors = new byte[componentCount];
            var definitions = scanIndex == 0 ? firstDefinitions : new List<Definition>();
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
                            definitions.Add(new(acTables[nextAcDefinition], 0x10 | ac.Slots[nextAcDefinition]));
                            nextAcDefinition++;
                        }
                    }
                }
                selectors[component] = (byte)((dcSlot << 4) | acSlot);
                extendedSequential |= !progressive && (dcSlot > 1 || acSlot > 1);
            }
            coding[scanIndex] = new(scanDc, scanAc, selectors, definitions.ToArray());
        }
        return new(coding, extendedSequential);
    }

    private sealed record Clusters(List<long[]> Histograms, int[] Indexes, List<int> Slots);

    private static Clusters Cluster(IReadOnlyList<long[]> histograms, bool forceBaseline)
    {
        var clusters = new List<long[]>();
        var indexes = new int[histograms.Count];
        var slots = new List<int>();
        var slotHistograms = new List<int>();
        var slotCosts = new List<float>();
        for (int index = 0; index < histograms.Count; index++)
        {
            long[] current = histograms[index];
            if (current.Length != 256 || current.Any(count => count < 0))
                throw new ArgumentException("Each histogram requires 256 nonnegative frequencies.", nameof(histograms));
            if (current.All(count => count == 0))
                continue;
            int bestSlot = slotHistograms.Count;
            float bestCost = forceBaseline && bestSlot > 1 ? float.MaxValue : Cost(current);
            for (int slot = 0; slot < slotHistograms.Count; slot++)
            {
                long[] combined = Add(clusters[slotHistograms[slot]], current);
                float cost = Cost(combined) - slotCosts[slot];
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestSlot = slot;
                }
            }
            if (bestSlot == slotHistograms.Count)
            {
                int histogramIndex = clusters.Count;
                clusters.Add((long[])current.Clone());
                indexes[index] = histogramIndex;
                if (bestSlot < 4)
                {
                    slotHistograms.Add(histogramIndex);
                    slotCosts.Add(bestCost);
                }
                else
                    bestSlot = (slots[^1] + 1) % 4;
                slotHistograms[bestSlot] = histogramIndex;
                slotCosts[bestSlot] = bestCost;
                slots.Add(bestSlot);
            }
            else
            {
                int histogramIndex = slotHistograms[bestSlot];
                clusters[histogramIndex] = Add(clusters[histogramIndex], current);
                indexes[index] = histogramIndex;
                slotCosts[bestSlot] += bestCost;
            }
        }
        return new(clusters, indexes, slots);
    }

    private static long[] Add(long[] left, long[] right)
    {
        var sum = new long[256];
        for (int symbol = 0; symbol < sum.Length; symbol++)
            sum[symbol] = checked(left[symbol] + right[symbol]);
        return sum;
    }

    private static float Cost(long[] frequencies)
        => HuffmanTable.CreateOptimized(frequencies).EstimateCost(frequencies);
}
