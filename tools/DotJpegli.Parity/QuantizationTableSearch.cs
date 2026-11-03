namespace DotJpegli.Parity;

using System.Globalization;
using System.Security.Cryptography;
using DotJpegli.Internal;

// Offline screening only: independent rounded-coefficient costs are a surrogate,
// so complete AQ-enabled JPEGs and independently decoded pixels decide the winner.
internal static class QuantizationTableSearch
{
    private static ReadOnlySpan<double> Scales => [.8, 1, 1.25];
    private static ReadOnlySpan<double> Lambdas => [.25, .5, 1, 2];
    private static ReadOnlySpan<int> BandEnds => [5, 20, 63];

    internal sealed record BandCost(int Group, int Band, int ScaleIndex, double Scale,
        double DistortionCost, double RateCost);

    internal sealed record Candidate(string Id, ushort[][] Tables, double DistortionCost, double RateCost)
    {
        internal JpegliEncoderOptions CreateOptions(float distance, ChromaSubsampling subsampling) => new()
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = distance,
            Subsampling = subsampling,
            AdaptiveQuantization = true,
            OptimizeHuffmanTables = true,
            ForceBaseline = true,
            ProgressiveLevel = 0,
            MaxDegreeOfParallelism = 1,
            // The control must exercise the original default-table path,
            // including its distance and AQ preparation policy.
            QuantizationTables = this.Id == "control" ? null
                : this.Tables.Select(table => (ReadOnlyMemory<ushort>)table).ToArray()
        };
    }

    internal sealed record Trial(string Id, int Bytes, string Sha256, string DecodedSha256,
        QualityMetrics Metrics, bool Eligible);

    internal sealed record SearchResult(byte[] Jpeg, string WinnerId, ushort[][] BaseTables,
        IReadOnlyList<BandCost> BandCosts, IReadOnlyList<Candidate> Candidates,
        IReadOnlyList<Trial> Trials, long SourceBlockCount)
    {
        internal int EncodeCount => this.Trials.Count;
    }

    internal static SearchResult Search(PpmImage image, float distance, ChromaSubsampling subsampling,
        Func<byte[], PpmImage> decode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decode);
        ValidateImage(image);
        cancellationToken.ThrowIfCancellationRequested();
        ushort[][] baseTables = CreateBaseTables(distance, subsampling);
        (IReadOnlyList<BandCost> bandCosts, long sourceBlockCount) =
            CalculateBandCosts(image, baseTables, subsampling, cancellationToken);
        IReadOnlyList<Candidate> candidates = GenerateCandidates(baseTables, bandCosts);
        IReadOnlyList<Candidate> finalists = Shortlist(candidates);
        var trials = new List<Trial>(finalists.Count);
        byte[]? best = null;
        string winner = "control";
        QualityMetrics? control = null;
        foreach (Candidate candidate in finalists)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] jpeg = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                candidate.CreateOptions(distance, subsampling), cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            PpmImage decoded = decode(jpeg);
            cancellationToken.ThrowIfCancellationRequested();
            ValidateImage(decoded);
            var metrics = QualityMetrics.Compare(image, decoded);
            control ??= metrics;
            bool eligible = metrics.RgbPsnrDb >= control.RgbPsnrDb && metrics.LumaSsim >= control.LumaSsim;
            trials.Add(new(candidate.Id, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels), metrics, eligible));
            // The control is first and is always eligible. Keep the earlier
            // candidate on equal complete sizes and retain only current/best JPEGs.
            if (eligible && (best is null || jpeg.Length < best.Length))
            {
                best = jpeg;
                winner = candidate.Id;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, baseTables, bandCosts, candidates, trials.AsReadOnly(), sourceBlockCount);
    }

    internal static ushort[][] CreateBaseTables(float distance, ChromaSubsampling subsampling)
    {
        if (!float.IsFinite(distance) || distance is <= 0f or > 25f)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Search distance must be finite and greater than zero through 25.");
        }
        ValidateSampling(subsampling);
        int factor = subsampling == ChromaSubsampling.Quarter ? 2 : 1;
        return Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTableForDistance(distance, component, factor, factor))
            .ToArray();
    }

    internal static (IReadOnlyList<BandCost> BandCosts, long SourceBlockCount) CalculateBandCosts(
        PpmImage image, ushort[][] baseTables, ChromaSubsampling subsampling,
        CancellationToken cancellationToken = default)
    {
        ValidateImage(image);
        ValidateTables(baseTables);
        ValidateSampling(subsampling);
        cancellationToken.ThrowIfCancellationRequested();
        var pixels = new PixelSource(image.Pixels, PixelLayout.Rgb24);
        double[] distortion = new double[18], rate = new double[18];
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        long blocks = 0;
        for (int component = 0; component < 3; component++)
        {
            int sampleSize = component > 0 && subsampling == ChromaSubsampling.Quarter ? 2 : 1;
            int columns = (image.Width + (8 * sampleSize) - 1) / (8 * sampleSize);
            int rows = (image.Height + (8 * sampleSize) - 1) / (8 * sampleSize);
            int group = component == 0 ? 0 : 1;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Downsampling.FillBlock(pixels, image.Width, image.Height, image.Width * 3, component,
                        x * 8 * sampleSize, y * 8 * sampleSize, sampleSize, sampleSize, samples);
                    ForwardDct.Transform(samples, coefficients);
                    blocks++;
                    int first = 1;
                    for (int band = 0; band < 3; band++)
                    {
                        for (int position = first; position <= BandEnds[band]; position++)
                        {
                            int natural = JpegConstants.ZigZag[position];
                            double coefficient = coefficients[natural];
                            for (int scale = 0; scale < 3; scale++)
                            {
                                int index = CostIndex(group, band, scale);
                                ushort quantizer = ScaleQuantizer(baseTables[component][natural], Scales[scale]);
                                double rounded = Math.Round(coefficient / quantizer, MidpointRounding.AwayFromZero);
                                double error = coefficient - (rounded * quantizer);
                                distortion[index] += error * error;
                                if (rounded != 0)
                                {
                                    // One nonzero marker plus the signed-amplitude
                                    // category. No run, EOB, table, header or stuffing model.
                                    rate[index] += 2 + Math.ILogB(Math.Abs(rounded));
                                }
                            }
                        }
                        first = BandEnds[band] + 1;
                    }
                }
            }
        }

        var costs = new List<BandCost>(18);
        for (int group = 0; group < 2; group++)
        {
            for (int band = 0; band < 3; band++)
            {
                for (int scale = 0; scale < 3; scale++)
                {
                    int index = CostIndex(group, band, scale);
                    costs.Add(new(group, band, scale, Scales[scale], distortion[index], rate[index]));
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return (costs.AsReadOnly(), blocks);
    }

    internal static IReadOnlyList<Candidate> GenerateCandidates(ushort[][] baseTables, IReadOnlyList<BandCost> bandCosts)
    {
        ValidateTables(baseTables);
        ArgumentNullException.ThrowIfNull(bandCosts);
        if (bandCosts.Count != 18)
        {
            throw new ArgumentException("Exactly eighteen group/band/scale costs are required.", nameof(bandCosts));
        }
        var costs =
            bandCosts.ToDictionary(cost => (cost.Group, cost.Band, cost.ScaleIndex));
        var candidates = new List<Candidate>(729);
        void Add(int code)
        {
            int[] choices = new int[6];
            int remaining = code;
            for (int index = 5; index >= 0; index--)
            {
                choices[index] = remaining % 3;
                remaining /= 3;
            }
            ushort[][] tables = baseTables.Select(table => table.ToArray()).ToArray();
            double distortion = 0, rate = 0;
            for (int group = 0; group < 2; group++)
            {
                int first = 1;
                for (int band = 0; band < 3; band++)
                {
                    int scale = choices[(group * 3) + band];
                    BandCost cost = costs[(group, band, scale)];
                    distortion += cost.DistortionCost;
                    rate += cost.RateCost;
                    for (int component = group == 0 ? 0 : 1; component < (group == 0 ? 1 : 3); component++)
                    {
                        for (int position = first; position <= BandEnds[band]; position++)
                        {
                            int natural = JpegConstants.ZigZag[position];
                            tables[component][natural] = ScaleQuantizer(baseTables[component][natural], Scales[scale]);
                        }
                    }
                    first = BandEnds[band] + 1;
                }
            }
            string id = code == 364 ? "control" : string.Create(CultureInfo.InvariantCulture,
                $"y-{choices[0]}{choices[1]}{choices[2]}-c-{choices[3]}{choices[4]}{choices[5]}");
            candidates.Add(new(id, tables, distortion, rate));
        }

        // Six ternary digits all equal to one select the unchanged base tables.
        Add(364);
        for (int code = 0; code < 729; code++)
        {
            if (code != 364)
            {
                Add(code);
            }
        }
        return candidates.AsReadOnly();
    }

    internal static IReadOnlyList<Candidate> Shortlist(IReadOnlyList<Candidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 0 || candidates[0].Id != "control")
        {
            throw new ArgumentException("The first candidate must be the unchanged control.", nameof(candidates));
        }
        Candidate control = candidates[0];
        double distortionScale = Math.Max(control.DistortionCost, 1);
        double rateScale = Math.Max(control.RateCost, 1);
        var result = new List<Candidate> { control };
        var seen = new HashSet<string>(StringComparer.Ordinal) { control.Id };
        foreach (double lambda in Lambdas)
        {
            // OrderBy is stable: the declared enumeration order resolves ties.
            foreach (Candidate candidate in candidates.OrderBy(candidate =>
                (candidate.DistortionCost / distortionScale) + (lambda * candidate.RateCost / rateScale)).Take(4))
            {
                if (seen.Add(candidate.Id))
                {
                    result.Add(candidate);
                }
            }
        }
        return result.AsReadOnly();
    }

    private static int CostIndex(int group, int band, int scale) => (((group * 3) + band) * 3) + scale;

    private static ushort ScaleQuantizer(ushort value, double scale)
        => (ushort)Math.Clamp(Math.Round(value * scale, MidpointRounding.AwayFromZero), 1, 255);

    private static void ValidateSampling(ChromaSubsampling subsampling)
    {
        if (subsampling is not (ChromaSubsampling.None or ChromaSubsampling.Quarter))
        {
            throw new ArgumentOutOfRangeException(nameof(subsampling), "Search sampling must be 444 or 420.");
        }
    }

    private static void ValidateImage(PpmImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Pixels is null || image.Width is <= 0 or > 65535 || image.Height is <= 0 or > 65535
            || (long)image.Width * image.Height * 3 != image.Pixels.Length)
        {
            throw new ArgumentException("Search requires a complete RGB8 raster with JPEG dimensions.", nameof(image));
        }
    }

    private static void ValidateTables(ushort[][] tables)
    {
        ArgumentNullException.ThrowIfNull(tables);
        if (tables.Length != 3 || tables.Any(table => table is null || table.Length != 64
            || table.Any(value => value is < 1 or > 255)))
        {
            throw new ArgumentException("Search requires three baseline quantization tables of 64 values.", nameof(tables));
        }
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
