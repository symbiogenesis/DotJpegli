namespace DotJpegli.Parity;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class DcTrellisSearchCommand
{
    private const string Revision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal static int Run(string[] args)
    {
        var options = RateDistortionSearchCommand.SearchOptions.Parse(args);
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-dc-trellis-decode-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string jpegPath = Path.Combine(scratch, "candidate.jpg"), decodedPath = Path.Combine(scratch, "decoded.ppm");
        try
        {
            string revision = Native(options.Native, scratch, ["version"]).Trim();
            if (!revision.Equals(Revision, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Native decoder does not report the pinned upstream revision.");
            }
            PpmImage Decode(byte[] jpeg)
            {
                File.WriteAllBytes(jpegPath, jpeg);
                File.Delete(decodedPath);
                _ = Native(options.Native, scratch, ["decode", jpegPath, decodedPath]);
                return PpmImage.Read(decodedPath);
            }
            return Run(options, Decode, new
            {
                implementation = "pinned jpegli RGB8, fancy upsampling, JDCT_ISLOW",
                revision,
                executableSha256 = Hash(File.ReadAllBytes(options.Native))
            });
        }
        finally
        {
            File.Delete(jpegPath);
            File.Delete(decodedPath);
            Directory.Delete(scratch);
        }
    }

    internal static int Run(RateDistortionSearchCommand.SearchOptions options, Func<byte[], PpmImage> decode, object decoderIdentity)
    {
        ValidatePaths(options);
        if (new FileInfo(options.Input).Length > 16L * 1024 * 1024)
        {
            throw new ArgumentException("The bounded experiment accepts PPM files up to 16 MiB including their header.", nameof(options));
        }
        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        DcTrellisSearch.SearchResult result = DcTrellisSearch.Search(image, options.Subsampling, decode);
        int scans = result.Trials.Count * 3;
        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded offline DC chain trellis; five lambdas at distance one; no global or perceptual optimum claim",
            input = new { path = options.Input, sha256 = Hash(input), pixelSha256 = Hash(image.Pixels), image.Width, image.Height },
            encoderAssemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
            runnerAssemblySha256 = Hash(File.ReadAllBytes(typeof(DcTrellisSearchCommand).Assembly.Location)),
            decoder = decoderIdentity,
            settings = new
            {
                sampling = options.Subsampling == ChromaSubsampling.None ? 444 : 420,
                distance = 1,
                progressiveLevel = 0,
                interleavedComponents = false,
                adaptiveQuantization = true,
                optimizeHuffmanTables = true,
                forceBaselineQuantizers = true,
                workers = 1,
                candidates = "Each original quantized DC and its two adjacent baseline-range integers; AC unchanged",
                model = "Actual floating DCT targets; squared DC error plus lambda times standard-table DC category and amplitude bits; predictor resets respected",
                modelExcludes = "AC bits, byte stuffing, padding, table headers and perceptual error",
                preparation = "Complete coefficient cache plus a second source-transform pass for DC targets; temporary targets, resets and backpointers included in the unchanged 64 MiB cache limit"
            },
            ranking = "Minimum complete JPEG size among candidates with RGB PSNR, local luma SSIM and multiscale structural score at least the unchanged control. Control first, keep first on ties",
            winner = result.Winner.Id,
            output = "winner.jpg",
            encodeCount = result.Trials.Count,
            decodeCount = result.Trials.Count,
            histogramScanCount = scans,
            outputScanCount = scans,
            totalEntropyScanPassCount = 2 * scans,
            sourceDcTransformBlockCount = result.Trials.Sum(trial => trial.SourceTransformBlocks),
            candidateEvaluations = result.Trials.Sum(trial => trial.CandidateEvaluations),
            result.ReferenceMetricValidationCount,
            metricEvaluation = "Prepared metrics per candidate; independent reference metrics recheck every incumbent",
            perceptualObjective = new { id = MultiScaleSsim.MetricId, definition = MultiScaleSsim.Definition },
            candidates = DcTrellisSearch.Candidates,
            trials = result.Trials
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
        var text = new StringBuilder("# Bounded DC chain trellis search\n\n");
        _ = text.AppendLine(FormattableString.Invariant($"Selected `{result.Winner.Id}`: {result.Jpeg.Length} complete JPEG bytes; control: {result.Trials[0].Bytes}. Control is retained on size ties and candidates must meet all three decoded quality floors."));
        _ = text.AppendLine("\nThe model uses floating DCT DC targets and standard DC lengths over each component's real raster scan order. AC coefficients stay unchanged. Complete output relearns Huffman tables and actual JPEG sizes decide. This is an offline size/quality experiment; no speed or universal gain is claimed.\n");
        _ = text.AppendLine("| Candidate | Bytes | Eligible | Changed DCs | Evaluations |\n| --- | ---: | --- | ---: | ---: |");
        foreach (DcTrellisSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(FormattableString.Invariant($"| {trial.Id} | {trial.Bytes} | {trial.Eligible} | {trial.ChangedCoefficients} | {trial.CandidateEvaluations} |"));
        }
        _ = text.AppendLine("\nsearch.json retains every trial, hashes, decoded metrics, source-transform work, modeled rate/distortion, quality eligibility, logical entropy scan counts and reference-metric checks. Logical counts do not measure wall time, allocations or peak process memory.");
        ValidatePaths(options);
        _ = Directory.CreateDirectory(options.Output);
        using (var stream = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            stream.Write(result.Jpeg);
        }
        WriteNew(Path.Combine(options.Output, "search.json"), json);
        WriteNew(Path.Combine(options.Output, "report.md"), text.ToString());
        Console.WriteLine(FormattableString.Invariant($"Selected {result.Winner.Id}: {result.Jpeg.Length} bytes from {result.Trials.Count} complete encodes and decodes. Output: {options.Output}"));
        return 0;
    }

    private static void ValidatePaths(RateDistortionSearchCommand.SearchOptions options)
    {
        if (!File.Exists(options.Input) || Directory.Exists(options.Output) || File.Exists(options.Output))
        {
            throw new ArgumentException("The input file must exist and the output directory must not exist.");
        }
    }

#pragma warning disable RS0030 // Synchronous callback at the independent-decoder boundary.
    private static string Native(string executable, string directory, string[] args)
        => QuantizationTableSearchCommand.NativeAsync(executable, directory, TimeSpan.FromMinutes(3), args).GetAwaiter().GetResult();
#pragma warning restore RS0030

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static void WriteNew(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(text);
    }
}
