namespace DotJpegli.Parity;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class HuffmanFeedbackSearchCommand
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
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-huffman-feedback-decode-" + Guid.NewGuid().ToString("N"));
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
        HuffmanFeedbackSearch.SearchResult result = HuffmanFeedbackSearch.Search(image, options.Subsampling, decode);
        int outputScans = result.Trials.Count * 3;
        int histogramScans = outputScans + (3 * result.Trials.Count(trial => trial.Feedback));
        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded offline image-specific Huffman feedback for AC RDOQ; one feedback pass; seven complete candidates; no convergence, global optimum or universal gain claim",
            input = new { path = options.Input, sha256 = Hash(input), pixelSha256 = Hash(image.Pixels), image.Width, image.Height },
            encoderAssemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
            runnerAssemblySha256 = Hash(File.ReadAllBytes(typeof(HuffmanFeedbackSearchCommand).Assembly.Location)),
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
                standardStage = "Original AQ AC, one step toward zero, zero; standard-table AC symbol and amplitude costs",
                feedbackStage = "Build exact image-specific clustered Huffman lengths from standard-RDOQ coefficients; one extra AC refinement over current AC, one step toward zero, zero; missing symbols forbidden",
                objective = "Actual floating DCT targets; squared AC reconstruction error plus lambda times run/category, ZRL, EOB and amplitude bits under the fixed feedback model",
                modelExcludes = "DC changes, byte stuffing, padding, table headers and perceptual error",
                finalOutput = "Relearn output Huffman tables after refinement; evaluate complete JPEG bytes and decoded metrics",
                storage = "One complete coefficient cache within the unchanged 64 MiB cap; fixed-size block search scratch and bounded Huffman histograms; no image-sized floating coefficient cache"
            },
            ranking = "Minimum complete JPEG size among candidates with RGB PSNR, local luma SSIM and multiscale structural score at least the unchanged control. Standard-RDOQ paired controls retained. Control first; keep first on ties",
            winner = result.Winner.Id,
            output = "winner.jpg",
            encodeCount = result.Trials.Count,
            decodeCount = result.Trials.Count,
            histogramScanCount = histogramScans,
            outputScanCount = outputScans,
            totalEntropyScanPassCount = histogramScans + outputScans,
            standardCandidateEvaluations = result.Trials.Sum(trial => trial.StandardEvaluations),
            feedbackCandidateEvaluations = result.Trials.Sum(trial => trial.FeedbackEvaluations),
            feedbackSourceTransformBlocks = result.Trials.Sum(trial => trial.FeedbackBlocks),
            result.ReferenceMetricValidationCount,
            metricEvaluation = "Prepared metrics per candidate; independent reference metrics recheck every incumbent",
            perceptualObjective = new { id = MultiScaleSsim.MetricId, definition = MultiScaleSsim.Definition },
            candidates = HuffmanFeedbackSearch.Candidates,
            trials = result.Trials
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
        var text = new StringBuilder("# Bounded image-specific Huffman feedback\n\n");
        _ = text.AppendLine(FormattableString.Invariant($"Selected `{result.Winner.Id}`: {result.Jpeg.Length} complete JPEG bytes; unchanged control: {result.Trials[0].Bytes}. Candidates must meet all three decoded quality floors; keep the first on size ties."));
        _ = text.AppendLine("\nEach feedback candidate starts with standard-table AC RDOQ, learns the image's actual clustered AC Huffman lengths, then runs one bounded refinement pass with real floating DCT targets. DC stays fixed. The final output relearns its tables. Paired standard-RDOQ outputs remain candidates, so the feedback pass has no assumed size or quality benefit.\n");
        _ = text.AppendLine("| Candidate | Bytes | Eligible | Standard changes | Feedback changes |\n| --- | ---: | --- | ---: | ---: |");
        foreach (HuffmanFeedbackSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(FormattableString.Invariant($"| {trial.Id} | {trial.Bytes} | {trial.Eligible} | {trial.StandardChanges} | {trial.FeedbackChanges} |"));
        }
        _ = text.AppendLine("\nsearch.json retains every trial, source/JPEG/pixel/assembly/decoder hashes, all decoded metrics, eligibility, every feedback model's 256 AC code lengths per component, extra histogram/transform work and modeled rate/distortion. Logical pass counts do not measure time, allocations or peak process memory. No convergence or universal improvement is claimed.");
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
