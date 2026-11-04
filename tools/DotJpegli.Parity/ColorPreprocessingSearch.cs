namespace DotJpegli.Parity;

using System.Globalization;
using System.Security.Cryptography;
using DotJpegli.Internal;

internal static class ColorPreprocessingSearch
{
    internal enum Kind { Original, Reconstruction420, SaturatedRunOvershoot }
    internal sealed record Candidate(string Id, float Distance, Kind Kind, int Iterations, double Weight);
    internal sealed record ModelEvidence(string Definition, double? BeforeError, double? AfterError, int Iterations, int ChangedSamples);
    internal sealed record Trial(Candidate Candidate, int Bytes, string Sha256, string DecodedSha256,
        QualityMetrics Metrics, double MultiScaleSsim, bool Eligible, ModelEvidence? Model);
    internal sealed record Rejection(Candidate Candidate, string Stage, string Reason);
    internal sealed record SearchResult(byte[] Jpeg, string WinnerId, IReadOnlyList<Candidate> Candidates,
        IReadOnlyList<Trial> Trials, IReadOnlyList<Rejection> Rejections, ColorPreprocessing.SamplingAdvice SamplingAdvice,
        int ReferenceMetricValidationCount);

    internal static SearchResult Search(PpmImage source, float distance, ChromaSubsampling sampling,
        Func<byte[], PpmImage> decode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decode);
        ColorPreprocessing.Validate(source);
        IReadOnlyList<Candidate> candidates = GenerateCandidates(distance, sampling);
        cancellationToken.ThrowIfCancellationRequested();
        var image = new PpmImage(source.Width, source.Height, source.Pixels.ToArray());
        PreparedQualityMetrics reference = QualityMetrics.Prepare(image);
        PreparedMultiScaleSsim perceptual = MultiScaleSsim.Prepare(image, cancellationToken);
        ColorPreprocessing.SamplingAdvice advice = ColorPreprocessing.AdviseSampling(image, true, cancellationToken);
        var trials = new List<Trial>();
        var rejected = new List<Rejection>();
        byte[]? best = null;
        string winner = "control";
        QualityMetrics? control = null;
        double controlStructural = 0;
        int validations = 0;
        foreach (Candidate candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ColorPreprocessing.Prepared? prepared = Prepare(image, candidate, sampling, cancellationToken);
            if (candidate.Kind != Kind.Original && prepared is null)
            {
                rejected.Add(new(candidate, "preparation", candidate.Kind == Kind.Reconstruction420
                    ? "The unquantized reconstruction model did not improve on ordinary 2x2 chroma averaging."
                    : "No qualifying saturated component run edge exists."));
                continue;
            }
            JpegliEncoderOptions options = Options(candidate, sampling);
            byte[] jpeg;
            try
            {
                jpeg = prepared is null
                    ? JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, options, cancellationToken: cancellationToken)
                    : JpegliEncoder.Encode(prepared.Pixels, image.Width, image.Height, prepared.Layout, options, cancellationToken: cancellationToken);
            }
            catch (ArgumentException error) when (prepared is not null && (error.ParamName == "coefficients"
                || (error.ParamName == "pixels" && error.InnerException is ArgumentException { ParamName: "coefficients" or "adaptiveStrength" })))
            {
                rejected.Add(new(candidate, "coefficient-validation", "Candidate exceeds supported finite eight-bit JPEG coefficient computations."));
                continue;
            }
            cancellationToken.ThrowIfCancellationRequested();
            PpmImage decoded = decode(jpeg);
            cancellationToken.ThrowIfCancellationRequested();
            ColorPreprocessing.Validate(decoded);
            QualityMetrics metrics = reference.Compare(decoded);
            double structural = perceptual.Compare(decoded, cancellationToken);
            if (control is null)
            {
                control = metrics;
                controlStructural = structural;
            }
            bool eligible = metrics.RgbPsnrDb >= control.RgbPsnrDb && metrics.LumaSsim >= control.LumaSsim
                && structural >= controlStructural;
            ModelEvidence? evidence = prepared is null ? null : new(prepared.Model, prepared.BeforeError,
                prepared.AfterError, prepared.Iterations, prepared.ChangedSamples);
            trials.Add(new(candidate, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels), metrics, structural, eligible, evidence));
            if (eligible && (best is null || jpeg.Length < best.Length))
            {
                if (QualityMetrics.Compare(image, decoded) != metrics || MultiScaleSsim.Compare(image, decoded, cancellationToken) != structural)
                {
                    throw new InvalidDataException("Prepared color-search scores disagree with the independent reference scorers.");
                }
                validations++;
                best = jpeg;
                winner = candidate.Id;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, candidates, trials.AsReadOnly(), rejected.AsReadOnly(), advice, validations);
    }

    internal static IReadOnlyList<Candidate> GenerateCandidates(float distance, ChromaSubsampling sampling)
    {
        if (!float.IsFinite(distance) || distance is <= 0 or > 25)
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }
        if (sampling is not (ChromaSubsampling.None or ChromaSubsampling.Quarter))
        {
            throw new ArgumentOutOfRangeException(nameof(sampling));
        }
        var result = new List<Candidate> { new("control", distance, Kind.Original, 0, 0) };
        foreach (float current in new[] { distance, Math.Clamp(distance * .8f, float.Epsilon, 25), Math.Clamp(distance * 1.25f, float.Epsilon, 25) }.Distinct())
        {
            string label = current.ToString("R", CultureInfo.InvariantCulture);
            if (current != distance)
            {
                result.Add(new("original-d" + label, current, Kind.Original, 0, 0));
            }
            if (sampling == ChromaSubsampling.Quarter)
            {
                foreach (int iterations in new[] { 2, 4 })
                {
                    result.Add(new($"reconstruct-d{label}-i{iterations}", current, Kind.Reconstruction420, iterations, 0));
                }
            }
            foreach (double weight in new[] { .5, 1 })
            {
                result.Add(new(FormattableString.Invariant($"overshoot-d{label}-w{weight}"), current, Kind.SaturatedRunOvershoot, 0, weight));
            }
        }
        return result.AsReadOnly();
    }

    internal static ColorPreprocessing.Prepared? Prepare(PpmImage image, Candidate candidate,
        ChromaSubsampling sampling, CancellationToken cancellationToken)
    {
        if (candidate.Kind == Kind.Reconstruction420)
        {
            if (sampling != ChromaSubsampling.Quarter)
            {
                throw new ArgumentException("The reconstruction candidate requires explicit 4:2:0 sampling.", nameof(sampling));
            }
            return ColorPreprocessing.Reconstruct420(image, candidate.Iterations, cancellationToken);
        }
        if (candidate.Kind == Kind.SaturatedRunOvershoot)
        {
            int factor = sampling == ChromaSubsampling.Quarter ? 2 : 1;
            ushort minimumAc = Enumerable.Range(0, 3)
                .SelectMany(component => Quantization.CreateTableForDistance(candidate.Distance, component, factor, factor).Skip(1)).Min();
            return ColorPreprocessing.Dering(image, Math.Min(4, minimumAc * candidate.Weight), cancellationToken);
        }
        return null;
    }

    internal static JpegliEncoderOptions Options(Candidate candidate, ChromaSubsampling sampling) => new()
    {
        ColorSpace = JpegColorSpace.YCbCr,
        Distance = candidate.Distance,
        Subsampling = sampling,
        AdaptiveQuantization = true,
        OptimizeHuffmanTables = true,
        ForceBaseline = true,
        ProgressiveLevel = 0,
        MaxDegreeOfParallelism = 1,
        AllowOutOfRangeFloatSamples = candidate.Kind == Kind.SaturatedRunOvershoot
    };

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
