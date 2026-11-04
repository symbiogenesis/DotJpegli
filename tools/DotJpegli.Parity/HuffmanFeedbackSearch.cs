namespace DotJpegli.Parity;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using DotJpegli.Internal;

internal static class HuffmanFeedbackSearch
{
    internal sealed record Candidate(string Id, double Lambda, bool Feedback)
    {
        internal JpegliEncoderOptions CreateOptions(ChromaSubsampling sampling, RateDistortionDiagnostics? diagnostics = null) => new()
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
            RateDistortionLambda = this.Lambda,
            RateDistortionHuffmanFeedback = this.Feedback,
            RateDistortionDiagnostics = diagnostics
        };
    }

    internal sealed record Trial(string Id, double Lambda, bool Feedback, int Bytes, string Sha256,
        string DecodedSha256, QualityMetrics Metrics, double MultiScaleSsim, bool Eligible,
        long StandardSearchedBlocks, long StandardEvaluations, long StandardChanges,
        long FeedbackHistogramBlocks, long FeedbackBlocks, long FeedbackEvaluations, long FeedbackChanges,
        long FeedbackOriginalBits, long FeedbackSelectedBits, double FeedbackOriginalSquaredError,
        double FeedbackSelectedSquaredError, int[][]? ModelAcCodeLengths);

    internal sealed record SearchResult(byte[] Jpeg, Candidate Winner, IReadOnlyList<Trial> Trials,
        int ReferenceMetricValidationCount);

    internal static ReadOnlyCollection<Candidate> Candidates { get; } = Array.AsReadOnly<Candidate>(
    [new("control", 0, false),
        new("ac-lambda-16", 16, false), new("ac-lambda-16-feedback", 16, true),
        new("ac-lambda-256", 256, false), new("ac-lambda-256-feedback", 256, true),
        new("ac-lambda-4096", 4096, false), new("ac-lambda-4096-feedback", 4096, true)]);

    internal static SearchResult Search(PpmImage image, ChromaSubsampling sampling, Func<byte[], PpmImage> decode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(decode);
        if (image.Width is < 1 or > 1024 || image.Height is < 1 or > 1024
            || image.Pixels.Length != image.Width * image.Height * 3)
        {
            throw new ArgumentException("Feedback search accepts complete RGB8 images up to 1024 by 1024 pixels.", nameof(image));
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
            var diagnostics = new RateDistortionDiagnostics();
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
            trials.Add(new(candidate.Id, candidate.Lambda, candidate.Feedback, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels),
                metrics, score, eligible, diagnostics.Blocks, diagnostics.CandidateEvaluations, diagnostics.ChangedCoefficients,
                diagnostics.FeedbackHistogramBlocks, diagnostics.FeedbackBlocks, diagnostics.FeedbackEvaluations,
                diagnostics.FeedbackChanges, diagnostics.FeedbackOriginalBits, diagnostics.FeedbackSelectedBits,
                diagnostics.FeedbackOriginalSquaredError, diagnostics.FeedbackSelectedSquaredError,
                diagnostics.FeedbackAcCodeLengths?.Select(lengths => lengths.Select(length => (int)length).ToArray()).ToArray()));
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
