namespace DotJpegli.Parity;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;

// Offline, bounded complete-JPEG search. The encoder's normal defaults and
// runtime do not perform this search, and candidates share no coefficient state.
internal static class ProgressiveScanSearch
{
    private static IReadOnlyList<int> AllComponents { get; } = Array.AsReadOnly<int>([0, 1, 2]);

    internal sealed record ScanCandidate(string Id, IReadOnlyList<JpegScan>? Script, bool OptimizeHuffmanTables,
        int HuffmanTableLimit = 0)
    {
        internal JpegliEncoderOptions CreateOptions(float distance, ChromaSubsampling subsampling) => new()
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = distance,
            Subsampling = subsampling,
            AdaptiveQuantization = true,
            OptimizeHuffmanTables = this.OptimizeHuffmanTables,
            HuffmanTableLimit = this.HuffmanTableLimit,
            ForceBaseline = true,
            MaxDegreeOfParallelism = 1,
            ProgressiveLevel = this.Script is null ? 2 : null,
            ScanScript = this.Script
        };
    }

    internal sealed record Trial(string Id, int Bytes, string Sha256, int ScanCount, bool OptimizeHuffmanTables,
        int[][] HuffmanSelectors, int HuffmanDefinitionBytes, int HuffmanDefinitionCount, int HuffmanTableLimit = 0);

    internal sealed record SearchResult(byte[] Jpeg, ScanCandidate Winner, IReadOnlyList<Trial> Trials)
    {
        internal int EncodeCount => this.Trials.Count;
        internal int TotalScanCount => this.Trials.Sum(trial => trial.ScanCount);
        internal int TotalHistogramScanCount => this.Trials.Where(trial => trial.OptimizeHuffmanTables).Sum(trial => trial.ScanCount);
        // These count logical histogram collection and scan emission/replay.
        // They are not counts of DCT evaluations or source-image traversals.
        internal int TotalEntropyPassCount => this.TotalScanCount + this.TotalHistogramScanCount;
    }

    internal static IReadOnlyList<ScanCandidate> Candidates { get; } = CreateCandidates();

    internal static SearchResult Search(byte[] pixels, int width, int height, float distance,
        ChromaSubsampling subsampling, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        return Run(candidate => JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24,
            candidate.CreateOptions(distance, subsampling), cancellationToken: cancellationToken), cancellationToken);
    }

    internal static SearchResult Run(Func<ScanCandidate, byte[]> encode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encode);
        var trials = new List<Trial>(Candidates.Count);
        byte[]? best = null;
        ScanCandidate winner = Candidates[0];
        foreach (ScanCandidate candidate in Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] jpeg = encode(candidate);
            cancellationToken.ThrowIfCancellationRequested();
            var structure = JpegStructure.Read(jpeg);
            trials.Add(new(candidate.Id, jpeg.Length, Convert.ToHexString(SHA256.HashData(jpeg)),
                structure.Scans.Length, candidate.OptimizeHuffmanTables, structure.HuffmanSelectors,
                structure.HuffmanDefinitionBytes, structure.HuffmanDefinitionCount, candidate.HuffmanTableLimit));
            // Keep the control on equal sizes; otherwise keep the first minimum
            // in the declared candidate order. Only winner and current JPEGs live.
            if (best is null || jpeg.Length < best.Length)
            {
                best = jpeg;
                winner = candidate;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, trials.AsReadOnly());
    }

    private static ReadOnlyCollection<ScanCandidate> CreateCandidates()
    {
        var candidates = new List<ScanCandidate>
        {
            new("default-level2-optimized", null, true)
        };
        int[][] ends = [[63], [2, 63], [5, 63], [15, 63]];
        int[][] orders = [[0, 1, 2], [1, 2, 0], [2, 1, 0]];
        foreach (int[] bandEnds in ends)
        {
            for (int depth = 0; depth <= 2; depth++)
            {
                foreach (int[] order in orders)
                {
                    ReadOnlyCollection<JpegScan> script = CreateScript(bandEnds, depth, order);
                    string stem = string.Create(CultureInfo.InvariantCulture,
                        $"ends-{string.Join('-', bandEnds)}-depth-{depth}-order-{string.Join(string.Empty, order)}");
                    candidates.Add(new(stem + "-optimized", script, true));
                    candidates.Add(new(stem + "-optimized-limit1", script, true, 1));
                    candidates.Add(new(stem + "-optimized-limit2", script, true, 2));
                    candidates.Add(new(stem + "-optimized-limit4", script, true, 4));
                    candidates.Add(new(stem + "-standard", script, false));
                }
            }
        }

        return candidates.AsReadOnly();
    }

    private static ReadOnlyCollection<JpegScan> CreateScript(int[] ends, int depth, int[] order)
    {
        var script = new List<JpegScan>
        {
            // DC is full precision in every searched script. Only AC band
            // boundaries, refinement depth and component ordering vary.
            new(AllComponents, 0, 0)
        };
        void AddBands(int high, int low)
        {
            int start = 1;
            foreach (int end in ends)
            {
                foreach (int component in order)
                {
                    script.Add(new(Array.AsReadOnly<int>([component]), start, end, high, low));
                }
                start = end + 1;
            }
        }

        AddBands(0, depth);
        for (int low = depth - 1; low >= 0; low--)
        {
            AddBands(low + 1, low);
        }
        return script.AsReadOnly();
    }
}
