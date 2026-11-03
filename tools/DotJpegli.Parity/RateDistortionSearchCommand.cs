namespace DotJpegli.Parity;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class RateDistortionSearchCommand
{
    private const string UpstreamRevision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record SearchOptions(string Input, string Output, string Native, ChromaSubsampling Subsampling)
    {
        internal static SearchOptions Parse(string[] args)
        {
            if (args.Length != 8)
            {
                throw new ArgumentException("Usage: --search-rate-distortion --input RGB8_PPM --output NEW_DIRECTORY --native PINNED_DECODER --sampling 444|420.");
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index += 2)
            {
                if (args[index] is not ("--input" or "--output" or "--native" or "--sampling")
                    || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate rate-distortion search option.");
                }
            }

            ChromaSubsampling sampling = values["--sampling"] switch
            {
                "444" => ChromaSubsampling.None,
                "420" => ChromaSubsampling.Quarter,
                _ => throw new ArgumentException("Search sampling must be 444 or 420.")
            };
            var options = new SearchOptions(Path.GetFullPath(values["--input"]), Path.GetFullPath(values["--output"]),
                Path.GetFullPath(values["--native"]), sampling);
            ValidatePaths(options);
            if (!File.Exists(options.Native))
            {
                throw new ArgumentException("The pinned native decoder executable must exist.");
            }

            return options;
        }
    }

    internal static int Run(string[] args) => Run(args, Native);

    internal static int Run(string[] args, Func<string, string, string[], string> native)
    {
        ArgumentNullException.ThrowIfNull(native);
        var options = SearchOptions.Parse(args);
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-rate-distortion-decode-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string jpegPath = Path.Combine(scratch, "candidate.jpg"), decodedPath = Path.Combine(scratch, "decoded.ppm");
        try
        {
            string version = native(options.Native, scratch, ["version"]).Trim();
            if (!version.Equals(UpstreamRevision, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Native decoder does not report the pinned upstream revision.");
            }

            PpmImage Decode(byte[] jpeg)
            {
                File.WriteAllBytes(jpegPath, jpeg);
                File.Delete(decodedPath);
                _ = native(options.Native, scratch, ["decode", jpegPath, decodedPath]);
                return PpmImage.Read(decodedPath);
            }

            return Run(options, Decode, new
            {
                implementation = "pinned jpegli RGB8, fancy upsampling, JDCT_ISLOW",
                version,
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

    // Tests inject an independent managed decoder; CLI reports always identify
    // the pinned native executable used for all candidates and controls.
    internal static int Run(SearchOptions options, Func<byte[], PpmImage> decode, object decoderIdentity)
    {
        ValidatePaths(options);
        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        RateDistortionSearch.SearchResult result = RateDistortionSearch.Search(image, options.Subsampling, decode);
        IReadOnlyList<RateDistortionSearch.Candidate> candidates = result.Candidates;
        var report = new
        {
            schemaVersion = 2,
            scope = "Bounded offline baseline RGB8 to YCbCr AC rate-distortion quantization; complete JPEGs with PSNR, local SSIM and multiscale structural floors; PSNR/SSIM curves; no global optimum claim",
            input = new
            {
                path = options.Input,
                sha256 = Hash(input),
                pixelSha256 = Hash(image.Pixels),
                image.Width,
                image.Height
            },
            encoder = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
                version = typeof(JpegliEncoder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            },
            runner = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(RateDistortionSearchCommand).Assembly.Location)),
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
            },
            decoder = decoderIdentity,
            settings = new
            {
                sampling = options.Subsampling == ChromaSubsampling.None ? 444 : 420,
                distances = candidates.Select(candidate => candidate.Distance).Distinct().ToArray(),
                lambdas = candidates.Select(candidate => candidate.Lambda).Distinct().ToArray(),
                adaptiveQuantization = true,
                optimizeHuffmanTables = true,
                progressiveLevel = 0,
                forceBaselineQuantizers = true,
                workers = 1,
                candidatesPerAc = "Original AQ coefficient, one step toward zero, zero; duplicate choices removed; DC fixed",
                objective = "Unweighted squared DCT reconstruction error plus lambda times standard AC Huffman run/category and amplitude lengths, including ZRL and EOB",
                entropyModel = "Fixed standard luma/chroma tables estimate search costs; complete output relearns optimized Huffman tables",
                comparison = "Nondominated curves; five common interior targets; log-byte interpolation; no extrapolation"
            },
            winner = result.WinnerId,
            output = "winner.jpg",
            encodeCount = result.Trials.Count,
            decodeCount = result.Trials.Count,
            referenceFeaturePreparationCount = 2,
            preparedMetricCount = result.Trials.Count,
            preparedScorerCallCount = 2 * result.Trials.Count,
            result.ReferenceMetricValidationCount,
            referenceScorerCallCount = 2 * result.ReferenceMetricValidationCount,
            totalHistogramScanCount = result.Trials.Count,
            totalOutputScanCount = result.Trials.Count,
            totalEntropyPassCount = 2 * result.Trials.Count,
            result.SearchedBlocks,
            result.CandidateEvaluations,
            result.ChangedCoefficients,
            costScope = "Every candidate performs a complete encode and decode; positive-lambda encodes add one bounded AC dynamic-programming search per real block. Retained block, transition and changed-coefficient counts account algorithmic work, not CPU time, allocations or peak memory",
            qualityScope = "RGB PSNR and disjoint 8x8 pixel-weighted BT.601 luma SSIM as defined by QualityMetrics, plus the explicitly defined offline multiscale structural objective; all three control floors apply. Estimated DCT distortion is not perceptual quality. Unsupported PSNR/SSIM curves are retained with their reasons",
            perceptualObjective = new { id = MultiScaleSsim.MetricId, definition = MultiScaleSsim.Definition },
            metricEvaluation = "One immutable source-feature preparation for PSNR/local SSIM and one for the multiscale objective; prepared score sets for every decoded trial; each independent reference scorer checks the control and every provisional winner without another encode or decode. Reused arithmetic is not measured latency or encoder acceleration",
            candidates,
            result.Trials,
            result.Comparisons
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n", markdown = Report(result);
        ValidatePaths(options);
        _ = Directory.CreateDirectory(options.Output);
        using (var output = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            output.Write(result.Jpeg);
        }

        WriteNew(Path.Combine(options.Output, "search.json"), json);
        WriteNew(Path.Combine(options.Output, "report.md"), markdown);
        Console.WriteLine(FormattableString.Invariant($"Selected {result.WinnerId}: {result.Jpeg.Length} bytes from {result.Trials.Count} complete encodes and decodes. Output: {options.Output}"));
        return 0;
    }

    private static void ValidatePaths(SearchOptions options)
    {
        if (!File.Exists(options.Input) || Directory.Exists(options.Output) || File.Exists(options.Output))
        {
            throw new ArgumentException("The input file must exist and the output directory must not exist.");
        }
    }

#pragma warning disable RS0030 // Required synchronous Func<byte[], PpmImage> decoder boundary.
    private static string Native(string executable, string workingDirectory, string[] args)
        => QuantizationTableSearchCommand.NativeAsync(executable, workingDirectory, TimeSpan.FromMinutes(3), args).GetAwaiter().GetResult();
#pragma warning restore RS0030

    private static string Report(RateDistortionSearch.SearchResult result)
    {
        var text = new StringBuilder();
        _ = text.AppendLine("# Bounded offline rate-distortion quantization search");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Selected `{result.WinnerId}`: {result.Jpeg.Length} complete JPEG bytes from {result.Trials.Count} complete encodes and decodes.");
        _ = text.AppendLine();
        _ = text.AppendLine("Each positive-lambda trial begins with the normal AQ coefficients and retains DC. A finite dynamic program chooses original AC values, one step toward zero or zero using squared DCT error plus estimated run/category bit cost, including EOB and ZRL. Every candidate is completely encoded with optimized Huffman tables and independently decoded. Complete JPEG sizes and decoded RGB PSNR, local luma SSIM and the offline multiscale structural score decide eligibility; the surrogate cannot establish actual entropy size or visual quality.");
        _ = text.AppendLine();
        _ = text.AppendLine("Matched-size and matched-quality PSNR/local SSIM comparisons use nondominated curves and five supported interior targets, interpolating in log bytes without extrapolation. Missing or constant common ranges remain explicitly unsupported in search.json. Winner selection uses all three quality floors of the distance-1 unchanged AQ/Huffman control; equal complete sizes retain the earlier candidate.");
        _ = text.AppendLine();
        _ = text.AppendLine("| Trial | Distance | Lambda | Complete bytes | RGB PSNR dB | Luma SSIM | Multiscale SSIM | Eligible |");
        _ = text.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |");
        foreach (RateDistortionSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {trial.Id} | {trial.Distance:R} | {trial.Lambda:R} | {trial.Bytes} | {trial.Metrics.RgbPsnrDb:R} | {trial.Metrics.LumaSsim:R} | {trial.MultiScaleSsim:R} | {trial.Eligible} |");
        }

        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Added search work: {result.SearchedBlocks} real blocks, {result.CandidateEvaluations} candidate/state transitions, {result.ChangedCoefficients} changed AC coefficients across all trials.");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Quality evaluation: one immutable preparation for each objective family, {result.Trials.Count} prepared score sets and {result.ReferenceMetricValidationCount} independent reference validations per family of the control and provisional winners. No additional encode or decode is needed. Source RGB, luma, window moments and multiscale features add image-sized offline storage; saved repeated arithmetic is not a measured speedup.");
        _ = text.AppendLine();
        _ = text.AppendLine(MultiScaleSsim.MetricId + ": " + MultiScaleSsim.Definition);
        foreach (RateDistortionSearch.Comparison comparison in result.Comparisons)
        {
            _ = text.AppendLine();
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"## Lambda {comparison.Lambda:R}: {comparison.Metric}");
            _ = text.AppendLine();
            _ = text.AppendLine("Matched size: " + comparison.MatchedRateStatus);
            _ = text.AppendLine();
            _ = text.AppendLine("| Bytes | Control quality | Candidate quality | Quality delta |");
            _ = text.AppendLine("| ---: | ---: | ---: | ---: |");
            foreach (RateDistortionSearch.MatchedRate point in comparison.MatchedRates)
            {
                _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {point.Bytes:R} | {point.ControlQuality:R} | {point.CandidateQuality:R} | {point.QualityDelta:R} |");
            }
            _ = text.AppendLine();
            _ = text.AppendLine("Matched quality: " + comparison.MatchedQualityStatus);
            _ = text.AppendLine();
            _ = text.AppendLine("| Quality | Control bytes | Candidate bytes | Candidate/control size ratio |");
            _ = text.AppendLine("| ---: | ---: | ---: | ---: |");
            foreach (RateDistortionSearch.MatchedQuality point in comparison.MatchedQualities)
            {
                _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {point.Quality:R} | {point.ControlBytes:R} | {point.CandidateBytes:R} | {point.SizeRatio:R} |");
            }
        }
        _ = text.AppendLine();
        _ = text.AppendLine("`search.json` retains all trial settings, complete JPEG and decoded hashes, quality metrics, eligibility, matched comparisons and algorithmic work counts. One histogram and one emission/replay pass are charged per complete encode; coefficient preparation and positive-lambda searches are additional. These counts do not measure CPU time, allocations or peak working memory. Reproduce winner.jpg with the recorded distance, lambda and sampling settings using the same encoder revision.");
        _ = text.AppendLine();
        _ = text.AppendLine("This finite offline experiment makes no global optimum, latency, perceptual-quality or corpus-wide compression claim. Encoder defaults and public APIs are unchanged. Corpus and performance measurements run on CI.");
        return text.ToString();
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static void WriteNew(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(text);
    }
}
