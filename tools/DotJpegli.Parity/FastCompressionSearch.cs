namespace DotJpegli.Parity;

using System.Globalization;
using System.Security.Cryptography;

internal static class FastCompressionSearch
{
    internal sealed record Candidate(string Id, float Distance, int ProgressiveLevel,
        float TailZeroRunBias, float ChromaDownsamplingSharpness);

    internal sealed record Trial(Candidate Candidate, int Bytes, string Sha256, string DecodedSha256,
        QualityMetrics Metrics, double MultiScaleSsim, bool StrictEligible, bool GateEligible);

    internal sealed record SearchResult(byte[] Jpeg, string WinnerId, string GateWinnerId,
        IReadOnlyList<Candidate> Candidates, IReadOnlyList<Trial> Trials, int ReferenceMetricValidationCount);

    private static readonly float[] TailZeroRunBiases = [0f, 0.05f, 0.1f, 0.15f, 0.25f];
    private static readonly float[] ChromaSharpnessValues = [0f, 0.15f, 0.25f, 0.35f];

    internal static SearchResult Search(PpmImage source, float distance, ChromaSubsampling sampling,
        int progression, Func<byte[], PpmImage> decode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decode);
        ColorPreprocessing.Validate(source);
        IReadOnlyList<Candidate> candidates = GenerateCandidates(distance, sampling, progression);
        cancellationToken.ThrowIfCancellationRequested();
        var image = new PpmImage(source.Width, source.Height, source.Pixels.ToArray());
        PreparedQualityMetrics reference = QualityMetrics.Prepare(image);
        PreparedMultiScaleSsim perceptual = MultiScaleSsim.Prepare(image, cancellationToken);
        var trials = new List<Trial>(candidates.Count);
        byte[]? bestStrict = null;
        byte[]? bestGate = null;
        string strictWinner = "control";
        string gateWinner = "control";
        QualityMetrics? control = null;
        double controlStructural = 0;
        int validations = 0;

        foreach (Candidate candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            JpegliEncoderOptions options = Options(candidate, sampling);
            byte[] jpeg = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                options, cancellationToken: cancellationToken);
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

            bool strictEligible = metrics.RgbPsnrDb >= control.RgbPsnrDb
                && metrics.LumaSsim >= control.LumaSsim
                && structural >= controlStructural;
            bool gateEligible = metrics.RgbPsnrDb >= control.RgbPsnrDb - 0.05
                && metrics.LumaSsim >= control.LumaSsim - 0.0005
                && structural >= controlStructural - 0.0005;

            trials.Add(new(candidate, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels),
                metrics, structural, strictEligible, gateEligible));

            bool selectedStrict = strictEligible && (bestStrict is null || jpeg.Length < bestStrict.Length);
            bool selectedGate = gateEligible && (bestGate is null || jpeg.Length < bestGate.Length);
            if (selectedStrict || selectedGate)
            {
                if (QualityMetrics.Compare(image, decoded) != metrics
                    || MultiScaleSsim.Compare(image, decoded, cancellationToken) != structural)
                {
                    throw new InvalidDataException("Prepared fast-compression search scores disagree with the independent reference scorers.");
                }

                validations++;
                if (selectedStrict)
                {
                    bestStrict = jpeg;
                    strictWinner = candidate.Id;
                }

                if (selectedGate)
                {
                    bestGate = jpeg;
                    gateWinner = candidate.Id;
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new(bestStrict!, strictWinner, gateWinner, candidates, trials.AsReadOnly(), validations);
    }

    internal static IReadOnlyList<Candidate> GenerateCandidates(float distance, ChromaSubsampling sampling, int progression)
    {
        if (!float.IsFinite(distance) || distance is <= 0 or > 25)
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }

        if (sampling is not (ChromaSubsampling.None or ChromaSubsampling.Quarter))
        {
            throw new ArgumentOutOfRangeException(nameof(sampling));
        }

        if (progression is not (0 or 2))
        {
            throw new ArgumentOutOfRangeException(nameof(progression));
        }

        var result = new List<Candidate>
        {
            new("control", distance, progression, 0f, 0f)
        };

        int[] levels = progression == 2 ? [2, 3] : [0];
        float[] sharpnessCandidates = sampling == ChromaSubsampling.Quarter ? ChromaSharpnessValues : [0f];

        foreach (int level in levels)
        {
            foreach (float bias in TailZeroRunBiases)
            {
                foreach (float sharpness in sharpnessCandidates)
                {
                    if (level == progression && bias == 0f && sharpness == 0f)
                    {
                        continue;
                    }

                    string biasLabel = bias.ToString("0.##", CultureInfo.InvariantCulture);
                    string sharpLabel = sharpness.ToString("0.##", CultureInfo.InvariantCulture);
                    result.Add(new($"p{level}-zrl{biasLabel}-sharp{sharpLabel}", distance, level, bias, sharpness));
                }
            }
        }

        return result.AsReadOnly();
    }

    internal static JpegliEncoderOptions Options(Candidate candidate, ChromaSubsampling sampling) => new()
    {
        ColorSpace = JpegColorSpace.YCbCr,
        Distance = candidate.Distance,
        Subsampling = sampling,
        AdaptiveQuantization = true,
        OptimizeHuffmanTables = true,
        ForceBaseline = true,
        ProgressiveLevel = candidate.ProgressiveLevel,
        TailZeroRunBias = candidate.TailZeroRunBias,
        ChromaDownsamplingSharpness = candidate.ChromaDownsamplingSharpness,
        MaxDegreeOfParallelism = 1
    };

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
