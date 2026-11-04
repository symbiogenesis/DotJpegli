namespace DotJpegli.Parity;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using DotJpegli.Internal;

internal static class DcTrellisSearch
{
    internal sealed record Candidate(string Id, double Lambda)
    {
        internal JpegliEncoderOptions CreateOptions(ChromaSubsampling sampling, DcTrellisDiagnostics? diagnostics = null) => new()
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = 1,
            Subsampling = sampling,
            ProgressiveLevel = 0,
            SequentialInterleaveComponents = false,
            OptimizeHuffmanTables = true,
            AdaptiveQuantization = true,
            ForceBaseline = true,
            MaxDegreeOfParallelism = 1,
            DcTrellisLambda = this.Lambda,
            DcTrellisDiagnostics = diagnostics
        };
    }

    internal sealed record Trial(string Id, double Lambda, int Bytes, string Sha256, string DecodedSha256,
        QualityMetrics Metrics, double MultiScaleSsim, bool Eligible, long Chains, long SourceTransformBlocks,
        long CandidateEvaluations, long ChangedCoefficients, long OriginalBits, long SelectedBits,
        double OriginalSquaredError, double SelectedSquaredError);

    internal sealed record SearchResult(byte[] Jpeg, Candidate Winner, IReadOnlyList<Trial> Trials,
        int ReferenceMetricValidationCount);

    internal static ReadOnlyCollection<Candidate> Candidates { get; } = Array.AsReadOnly<Candidate>(
    [new("control", 0), new("dc-lambda-1", 1), new("dc-lambda-16", 16),
        new("dc-lambda-256", 256), new("dc-lambda-4096", 4096)]);

    internal static SearchResult Search(PpmImage image, ChromaSubsampling sampling, Func<byte[], PpmImage> decode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(decode);
        if (image.Width is < 1 or > 1024 || image.Height is < 1 or > 1024
            || image.Pixels.Length != image.Width * image.Height * 3)
        {
            throw new ArgumentException("DC search accepts complete RGB8 images up to 1024 by 1024 pixels.", nameof(image));
        }
        if (sampling is not (ChromaSubsampling.None or ChromaSubsampling.Quarter))
        {
            throw new ArgumentOutOfRangeException(nameof(sampling));
        }
        PreparedQualityMetrics reference = QualityMetrics.Prepare(image);
        PreparedMultiScaleSsim structural = MultiScaleSsim.Prepare(image, cancellationToken);
        var trials = new List<Trial>(Candidates.Count);
        byte[]? best = null;
        Candidate winner = Candidates[0];
        QualityMetrics? control = null;
        double controlStructural = 0;
        int referenceValidations = 0;
        foreach (Candidate candidate in Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var diagnostics = new DcTrellisDiagnostics();
            byte[] jpeg = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                candidate.CreateOptions(sampling, diagnostics), cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            PpmImage decoded = decode(jpeg);
            QualityMetrics metrics = reference.Compare(decoded);
            double score = structural.Compare(decoded, cancellationToken);
            if (control is null)
            {
                control = metrics;
                controlStructural = score;
            }
            bool eligible = metrics.RgbPsnrDb >= control.RgbPsnrDb && metrics.LumaSsim >= control.LumaSsim
                && score >= controlStructural;
            trials.Add(new(candidate.Id, candidate.Lambda, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels), metrics,
                score, eligible, diagnostics.Chains, diagnostics.SourceTransformBlocks,
                diagnostics.CandidateEvaluations, diagnostics.ChangedCoefficients,
                diagnostics.OriginalBits, diagnostics.SelectedBits,
                diagnostics.OriginalSquaredError, diagnostics.SelectedSquaredError));
            if (eligible && (best is null || jpeg.Length < best.Length))
            {
                if (QualityMetrics.Compare(image, decoded) != metrics
                    || MultiScaleSsim.Compare(image, decoded, cancellationToken) != score)
                {
                    throw new InvalidDataException("Prepared quality metrics disagree with the reference scorer.");
                }
                referenceValidations++;
                best = jpeg;
                winner = candidate;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, trials.AsReadOnly(), referenceValidations);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
