namespace DotJpegli.Parity;

using System.Globalization;
using System.Security.Cryptography;
using DotJpegli.Internal;

// A bounded offline experiment. DCT-domain distortion and estimated AC symbol
// costs guide coefficient choices; complete JPEG bytes and decoded metrics are
// retained independently. Neither the surrogate nor the finite grid is optimal.
internal static class RateDistortionSearch
{
    private static ReadOnlySpan<float> Distances => [.5f, 1f, 2f, 4f, 8f];
    private static ReadOnlySpan<double> Lambdas => [0, 1, 16, 256, 4096];

    internal sealed record Candidate(string Id, float Distance, double Lambda)
    {
        internal JpegliEncoderOptions CreateOptions(ChromaSubsampling subsampling,
            RateDistortionDiagnostics? diagnostics = null) => new()
            {
                ColorSpace = JpegColorSpace.YCbCr,
                Distance = this.Distance,
                Subsampling = subsampling,
                AdaptiveQuantization = true,
                OptimizeHuffmanTables = true,
                ForceBaseline = true,
                ProgressiveLevel = 0,
                MaxDegreeOfParallelism = 1,
                RateDistortionLambda = this.Lambda,
                RateDistortionDiagnostics = diagnostics
            };
    }

    internal sealed record Trial(string Id, float Distance, double Lambda, int Bytes,
        string Sha256, string DecodedSha256, QualityMetrics Metrics, bool Eligible,
        long SearchedBlocks, long CandidateEvaluations, long ChangedCoefficients, double MultiScaleSsim);

    internal sealed record MatchedRate(double Bytes, double ControlQuality,
        double CandidateQuality, double QualityDelta);

    internal sealed record MatchedQuality(double Quality, double ControlBytes,
        double CandidateBytes, double SizeRatio);

    internal sealed record Comparison(double Lambda, string Metric,
        IReadOnlyList<CurvePoint> ControlFrontier, IReadOnlyList<CurvePoint> CandidateFrontier,
        string MatchedRateStatus, IReadOnlyList<MatchedRate> MatchedRates,
        string MatchedQualityStatus, IReadOnlyList<MatchedQuality> MatchedQualities);

    internal sealed record SearchResult(byte[] Jpeg, string WinnerId,
        IReadOnlyList<Candidate> Candidates, IReadOnlyList<Trial> Trials,
        IReadOnlyList<Comparison> Comparisons, int ReferenceMetricValidationCount)
    {
        internal int EncodeCount => this.Trials.Count;
        internal int DecodeCount => this.Trials.Count;
        // Every trial uses one sequential optimized-Huffman histogram pass and
        // one emission pass; token replay does not eliminate either logical pass.
        internal int HistogramPassCount => this.Trials.Count;
        internal int EntropyEmissionPassCount => this.Trials.Count;
        internal long SearchedBlocks => this.Trials.Sum(trial => trial.SearchedBlocks);
        internal long CandidateEvaluations => this.Trials.Sum(trial => trial.CandidateEvaluations);
        internal long ChangedCoefficients => this.Trials.Sum(trial => trial.ChangedCoefficients);
    }

    internal static SearchResult Search(PpmImage image, ChromaSubsampling subsampling,
        Func<byte[], PpmImage> decode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decode);
        ValidateImage(image);
        ValidateSampling(subsampling);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Candidate> candidates = GenerateCandidates();
        PreparedQualityMetrics reference = QualityMetrics.Prepare(image);
        PreparedMultiScaleSsim perceptualReference = MultiScaleSsim.Prepare(image, cancellationToken);
        var trials = new List<Trial>(candidates.Count);
        byte[]? best = null;
        string winner = "control";
        QualityMetrics? control = null;
        double? controlPerceptualScore = null;
        int referenceValidations = 0;
        foreach (Candidate candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var diagnostics = new RateDistortionDiagnostics();
            byte[] jpeg = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                candidate.CreateOptions(subsampling, diagnostics), cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            PpmImage decoded = decode(jpeg);
            cancellationToken.ThrowIfCancellationRequested();
            ValidateImage(decoded);
            QualityMetrics metrics = reference.Compare(decoded);
            double perceptualScore = perceptualReference.Compare(decoded, cancellationToken);
            control ??= metrics;
            controlPerceptualScore ??= perceptualScore;
            bool eligible = metrics.RgbPsnrDb >= control.RgbPsnrDb && metrics.LumaSsim >= control.LumaSsim
                && perceptualScore >= controlPerceptualScore.Value;
            trials.Add(new(candidate.Id, candidate.Distance, candidate.Lambda, jpeg.Length,
                Hash(jpeg), Hash(decoded.Pixels), metrics, eligible,
                diagnostics.Blocks, diagnostics.CandidateEvaluations, diagnostics.ChangedCoefficients, perceptualScore));
            // The distance-1 unchanged control is first and always eligible.
            // Equal complete sizes retain the earlier candidate and its bytes.
            if (eligible && (best is null || jpeg.Length < best.Length))
            {
                if (QualityMetrics.Compare(image, decoded) != metrics
                    || MultiScaleSsim.Compare(image, decoded, cancellationToken) != perceptualScore)
                {
                    throw new InvalidDataException("Prepared quality metrics disagree with the reference scorer.");
                }
                referenceValidations++;
                best = jpeg;
                winner = candidate.Id;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, candidates, trials.AsReadOnly(), CompareTrials(trials), referenceValidations);
    }

    internal static IReadOnlyList<Candidate> GenerateCandidates()
    {
        var candidates = new List<Candidate>(25) { new("control", 1f, 0) };
        foreach (float distance in Distances)
        {
            foreach (double lambda in Lambdas)
            {
                if (distance == 1f && lambda == 0)
                {
                    continue;
                }
                string id = string.Create(CultureInfo.InvariantCulture, $"d-{distance:R}-lambda-{lambda:R}");
                candidates.Add(new(id, distance, lambda));
            }
        }
        return candidates.AsReadOnly();
    }

    internal static IReadOnlyList<Comparison> CompareTrials(IReadOnlyList<Trial> trials)
    {
        ArgumentNullException.ThrowIfNull(trials);
        var comparisons = new List<Comparison>();
        foreach (double lambda in Lambdas[1..])
        {
            foreach (string metric in new[] { "rgb-psnr-db", "luma-ssim" })
            {
                double Select(Trial trial) => metric == "rgb-psnr-db"
                    ? trial.Metrics.RgbPsnrDb : trial.Metrics.LumaSsim;
                List<CurvePoint> control = RateDistortion.Frontier(trials.Where(trial => trial.Lambda == 0)
                    .Select(trial => new CurvePoint(trial.Bytes, Select(trial))));
                List<CurvePoint> candidate = RateDistortion.Frontier(trials.Where(trial => trial.Lambda == lambda)
                    .Select(trial => new CurvePoint(trial.Bytes, Select(trial))));
                var rates = new List<MatchedRate>();
                string rateStatus = "supported";
                try
                {
                    foreach (double bytes in RateDistortion.Targets(control, candidate))
                    {
                        double baselineQuality = RateDistortion.Interpolate(control, bytes);
                        double candidateQuality = RateDistortion.Interpolate(candidate, bytes);
                        rates.Add(new(bytes, baselineQuality, candidateQuality, candidateQuality - baselineQuality));
                    }
                }
                catch (InvalidDataException error)
                {
                    rateStatus = "unsupported: " + error.Message;
                }

                var qualities = new List<MatchedQuality>();
                string qualityStatus = "supported";
                try
                {
                    foreach (double quality in RateDistortion.QualityTargets(control, candidate))
                    {
                        double baselineBytes = RateDistortion.Invert(control, quality);
                        double candidateBytes = RateDistortion.Invert(candidate, quality);
                        qualities.Add(new(quality, baselineBytes, candidateBytes, candidateBytes / baselineBytes));
                    }
                }
                catch (InvalidDataException error)
                {
                    qualityStatus = "unsupported: " + error.Message;
                }
                comparisons.Add(new(lambda, metric, control.AsReadOnly(), candidate.AsReadOnly(),
                    rateStatus, rates.AsReadOnly(), qualityStatus, qualities.AsReadOnly()));
            }
        }
        return comparisons.AsReadOnly();
    }

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

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
