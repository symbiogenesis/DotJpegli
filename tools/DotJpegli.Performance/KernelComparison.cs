namespace DotJpegli.Performance;

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>CI-only paired comparisons of immutable per-encode kernel choices.</summary>
internal static class KernelComparison
{
    private const double MaximumWithinQuartetDrift = 1.10;
    private const double MaximumPercentileDrift = 1.20;
    private const double MaximumOrderBias = 1.10;
    private const double PreferenceMargin = 0.005;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    internal sealed record Leg(int Position, string Variant, double NsPerEncode, long AllocatedBytes,
        int Gen0Collections, int Gen1Collections, int Gen2Collections);
    internal sealed record Quartet(int Block, string Order, Leg[] Legs);
    internal sealed record Analysis(
        [property: JsonPropertyName("baseline")] TimingStatistics? Original,
        [property: JsonPropertyName("candidate")] TimingStatistics? Optimized,
        TimingStatistics? PairedRatios, double? GeometricMeanPairedRatio, double? OrderBias,
        double? MaximumWithinQuartetDrift,
        [property: JsonPropertyName("baselinePercentileDrift")] double? OriginalPercentileDrift,
        [property: JsonPropertyName("candidatePercentileDrift")] double? OptimizedPercentileDrift,
        [property: JsonPropertyName("baselineAllocatedBytesPerEncode")] double? OriginalAllocatedBytesPerEncode,
        [property: JsonPropertyName("candidateAllocatedBytesPerEncode")] double? OptimizedAllocatedBytesPerEncode,
        string EvidencePreference, string[] InvalidReasons);
    internal sealed record Result(string Id, string Category, int Width, int Height, int Sampling,
        int ProgressiveLevel, float Distance, string Kernel, string BaselinePreference, string CandidatePreference,
        string BaselineVariant, string CandidateVariant,
        [property: JsonPropertyName("candidateKernelReachable")] bool OptimizedKernelReachable,
        int Iterations,
        [property: JsonPropertyName("baselineJpegSha256")] string OriginalJpegSha256,
        [property: JsonPropertyName("candidateJpegSha256")] string OptimizedJpegSha256, string AutomaticJpegSha256,
        long JpegBytes, Quartet[] Quartets, Analysis Analysis);
    private sealed record Skipped(string Id, int Width, int Height, int Sampling,
        int ProgressiveLevel, float Distance, string Kernel, string Reason);
    private sealed record Settings(string Corpus, string Output, DiagnosticSelection Selection, bool IncludeAutomatic);

    internal static async Task<int> Run(string[] args)
    {
        Program.RequireCi();
        string? output = null;
        try
        {
            Settings options = Parse(args);
            if (Directory.Exists(options.Output) && Directory.EnumerateFileSystemEntries(options.Output).Any())
            {
                throw new InvalidDataException("Kernel evidence output directory must be empty.");
            }

            _ = Directory.CreateDirectory(options.Output);
            output = options.Output;
            _ = Directory.CreateDirectory(Path.Combine(output, "inputs"));
            _ = Directory.CreateDirectory(Path.Combine(output, "jpegs"));
            _ = Directory.CreateDirectory(Path.Combine(output, "raw-comparisons"));
            DiagnosticSelection selection = options.Selection;
            Policy policy = selection.Policy;
            int requestedIterations = policy.MinimumIterations;
            bool narrowApplicable = Vector256.IsHardwareAccelerated && Vector<float>.Count == 8;
            int expectedComparisons = ExpectedComparisons(selection.ExpectedCases, narrowApplicable, options.IncludeAutomatic);
            var results = new List<Result>();
            var skipped = new List<Skipped>();
            var sources = new List<SourceImage>();
            await WriteJson(output, "run.json", new
            {
                schemaVersion = 2,
                mode = "interleaved-kernel-comparison",
                startedAtUtc = DateTimeOffset.UtcNow,
                testedRevision = Environment.GetEnvironmentVariable("DOTJPEGLI_PERFORMANCE_REVISION")
                    ?? Environment.GetEnvironmentVariable("GITHUB_SHA"),
                githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
                managedEncoderSha256 = HashFile(typeof(JpegliEncoder).Assembly.Location),
                managedRunnerSha256 = HashFile(Assembly.GetExecutingAssembly().Location),
                corpusManifestSha256 = HashFile(Path.Combine(AppContext.BaseDirectory, "corpus.json")),
                selection,
                expectedComparisons,
                requestedComparisons = ExpectedComparisons(selection.ExpectedCases, true, options.IncludeAutomatic),
                options.IncludeAutomatic,
                narrowApplicable,
                environment = Program.EnvironmentDetails(),
                method = new
                {
                    scope = "Selected managed kernels only; no native comparison or full performance parity verdict",
                    variants = "Forced pairs change one kernel Original/Optimized and hold the other Automatic; optional automatic-policy pair compares both Original against both Automatic; exact preferences retained per comparison",
                    order = "Six quartets by default; require an even count, balanced ABBA/BAAB by comparison and block index; A=baseline, B=candidate",
                    warmup = "Each leg receives the same untimed warmups; options are immutable and prepared outside the timer",
                    iterations = "Fixed requested count, except inputs below1024 pixels use max(requested,100); actual iterations retained per comparison",
                    calibration = "None; both variants use identical recorded iteration counts",
                    latency = "Only complete public byte-array Encode calls timed; output equality checked after stopping each timer",
                    ratio = "Each quartet uses geometric mean candidate / geometric mean baseline; primary statistic is median of those paired ratios",
                    gc = "Natural GC included; no forced collections or no-GC region; per-leg allocations and collection counts retained",
                    minimumLegMilliseconds = policy.MinimumBlockMilliseconds,
                    maximumRelativeMad = policy.MaximumRelativeMad,
                    maximumWithinQuartetDrift = MaximumWithinQuartetDrift,
                    maximumP90ToP10Drift = MaximumPercentileDrift,
                    maximumOrderBias = MaximumOrderBias,
                    evidencePreference = "Baseline or candidate variant name only when valid and every quartet ratio lies beyond1+/-0.005 in the same direction; otherwise inconclusive",
                    rawMeasurements = "raw-comparisons/*.json is updated after each completed quartet, before analysis; nonfinite raw values use named JSON strings"
                }
            });
            await WriteJson(output, "summary.json", new
            { schemaVersion = 2, mode = "interleaved-kernel-comparison", status = "incomplete", valid = false, expectedComparisons });
            foreach ((string? id, string? category, string? sourcePath, PpmImage? image) in Program.ReadCorpus(options.Corpus)
                .Concat(KernelDiagnosticInputs.Create(selection.Images))
                .Where(image => selection.Images.Contains(image.Id, StringComparer.Ordinal)))
            {
                string inputPath = Path.Combine(output, "inputs", id + ".ppm");
                image.Write(inputPath);
                sources.Add(new(id, category, sourcePath, HashFile(inputPath), Hash(image.Pixels), image.Width, image.Height));
                await WriteJson(output, "sources.json", sources);
                foreach (int sampling in selection.Samplings)
                {
                    foreach (int progression in selection.Progressions)
                    {
                        foreach (float distance in selection.Distances)
                        {
                            var automaticSettings = new JpegliEncoderOptions
                            {
                                ColorSpace = JpegColorSpace.YCbCr,
                                Distance = distance,
                                ProgressiveLevel = progression,
                                Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                                OptimizeHuffmanTables = true,
                                AdaptiveQuantization = true,
                                ForceBaseline = true,
                                MaxDegreeOfParallelism = 1,
                                FuzzyErosionKernel = KernelPreference.Automatic,
                                NarrowZigZagKernel = KernelPreference.Automatic
                            };
                            string stem = $"{id}_{sampling}_p{progression}_d{distance.ToString("0", CultureInfo.InvariantCulture)}";
                            byte[] automatic = Encode(image, automaticSettings);
                            byte[] expected = automatic;
                            await File.WriteAllBytesAsync(Path.Combine(output, "jpegs", stem + "_automatic.jpg"), automatic);
                            int iterations = Iterations(image.Width, image.Height, requestedIterations);
                            Policy casePolicy = policy with { MinimumIterations = iterations, MaximumIterations = iterations };
                            string[] comparisons = options.IncludeAutomatic
                                ? ["fuzzy-erosion", "narrow-zigzag", "automatic-policy"] : ["fuzzy-erosion", "narrow-zigzag"];
                            foreach (string kernel in comparisons)
                            {
                                if (kernel == "narrow-zigzag" && !narrowApplicable)
                                {
                                    skipped.Add(new(id, image.Width, image.Height, sampling, progression, distance,
                                        kernel, "The production pipeline does not reach the eight-lane narrow zigzag kernel on this vector configuration."));
                                    await WriteJson(output, "skipped.json", skipped);
                                    continue;
                                }
                                JpegliEncoderOptions original = kernel switch
                                {
                                    "fuzzy-erosion" => automaticSettings with { FuzzyErosionKernel = KernelPreference.Original },
                                    "narrow-zigzag" => automaticSettings with { NarrowZigZagKernel = KernelPreference.Original },
                                    _ => automaticSettings with
                                    {
                                        FuzzyErosionKernel = KernelPreference.Original,
                                        NarrowZigZagKernel = KernelPreference.Original
                                    }
                                };
                                JpegliEncoderOptions optimized = kernel switch
                                {
                                    "fuzzy-erosion" => automaticSettings with { FuzzyErosionKernel = KernelPreference.Optimized },
                                    "narrow-zigzag" => automaticSettings with { NarrowZigZagKernel = KernelPreference.Optimized },
                                    _ => automaticSettings
                                };
                                string baselinePreference = PreferenceName(original), candidatePreference = PreferenceName(optimized);
                                string candidateVariant = kernel == "automatic-policy" ? "automatic" : "optimized";
                                bool candidateReachable = kernel != "automatic-policy"
                                    || KernelPolicy.UseSelectionNetwork(KernelPreference.Automatic, image.Width)
                                    || (!Vector512.IsHardwareAccelerated && narrowApplicable
                                        && KernelPolicy.UseTiledZigZag(KernelPreference.Automatic, image.Width, image.Height));
                                byte[] originalJpeg = Encode(image, original);
                                byte[] optimizedJpeg = Encode(image, optimized);
                                RequireSameBytes(expected, originalJpeg);
                                RequireSameBytes(expected, optimizedJpeg);
                                await File.WriteAllBytesAsync(Path.Combine(output, "jpegs", stem + "_" + kernel + "_original.jpg"), originalJpeg);
                                await File.WriteAllBytesAsync(Path.Combine(output, "jpegs", stem + "_" + kernel + ".jpg"), optimizedJpeg);
                                int comparisonIndex = results.Count;
                                var quartets = new List<Quartet>();
                                for (int block = 0; block < policy.Blocks; block++)
                                {
                                    string order = Order(comparisonIndex, block);
                                    var legs = new Leg[4];
                                    for (int position = 0; position < order.Length; position++)
                                    {
                                        bool baseline = order[position] == 'A';
                                        legs[position] = Measure(image, baseline ? original : optimized, expected,
                                            iterations, policy.Warmups, position, baseline ? "original" : candidateVariant);
                                    }
                                    quartets.Add(new(block, order, legs));
                                    await WriteJson(Path.Combine(output, "raw-comparisons"), stem + "_" + kernel + ".json", new
                                    {
                                        schemaVersion = 2,
                                        id,
                                        category,
                                        width = image.Width,
                                        height = image.Height,
                                        sampling,
                                        progressiveLevel = progression,
                                        distance,
                                        kernel,
                                        comparisonIndex,
                                        baselinePreference,
                                        candidatePreference,
                                        baselineVariant = "original",
                                        candidateVariant,
                                        candidateKernelReachable = candidateReachable,
                                        baselineJpegSha256 = Hash(originalJpeg),
                                        candidateJpegSha256 = Hash(optimizedJpeg),
                                        automaticJpegSha256 = Hash(automatic),
                                        iterations,
                                        completedQuartets = quartets.Count,
                                        expectedQuartets = policy.Blocks,
                                        updatedAtUtc = DateTimeOffset.UtcNow,
                                        quartets
                                    });
                                }
                                Quartet[] samples = quartets.ToArray();
                                Analysis analysis = Analyze(samples, iterations, casePolicy, comparisonIndex, candidateVariant);
                                results.Add(new(id, category, image.Width, image.Height, sampling, progression, distance,
                                    kernel, baselinePreference, candidatePreference, "original", candidateVariant,
                                    candidateReachable, iterations, Hash(originalJpeg), Hash(optimizedJpeg), Hash(automatic),
                                    expected.LongLength, samples, analysis));
                                await WriteJson(output, "cases.json", results);
                                await WriteJson(output, "progress.json", new
                                { completedComparisons = results.Count, expectedComparisons, updatedAtUtc = DateTimeOffset.UtcNow });
                                string ratio = Metric(analysis.PairedRatios?.Median, "F4");
                                Console.WriteLine(FormattableString.Invariant($"{id}/{sampling}/p{progression}/d{distance}/{kernel}: {candidateVariant}/original={ratio}; preference={analysis.EvidencePreference}; valid={analysis.InvalidReasons.Length == 0}; {results.Count}/{expectedComparisons}"));
                            }
                        }
                    }
                }
            }
            string[] invalid = results.SelectMany(item => item.Analysis.InvalidReasons.Select(reason =>
                $"{Label(item)}: {reason}")).ToArray();
            if (results.Count != expectedComparisons)
            {
                invalid = [.. invalid, "Selected comparison coverage is incomplete."];
            }

            bool valid = invalid.Length == 0;
            await WriteJson(output, "summary.json", new
            {
                schemaVersion = 2,
                mode = "interleaved-kernel-comparison",
                status = valid ? "completed" : "invalid",
                valid,
                parityVerdict = "not-evaluated",
                fullCoverageComplete = false,
                comparisons = results.Count,
                expectedComparisons,
                skippedComparisons = skipped.Count,
                invalidReasons = invalid
            });
            await WriteJson(output, "skipped.json", skipped);
            await File.WriteAllTextAsync(Path.Combine(output, "report.md"), Report(results, skipped, invalid));
            return valid ? 0 : 2;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException
            or UnauthorizedAccessException or InvalidOperationException or OverflowException or JsonException or FormatException)
        {
            Console.Error.WriteLine($"Kernel comparison invalid: {error.Message}");
            if (output is not null)
            {
                await WriteJson(output, "summary.json", new
                { schemaVersion = 2, mode = "interleaved-kernel-comparison", status = "invalid", valid = false, error = error.Message });
                await File.WriteAllTextAsync(Path.Combine(output, "report.md"),
                    $"# Kernel comparison: INVALID\n\n{error.Message}\n\nPartial raw evidence is retained; no timing conclusion is valid.\n");
            }
            return 2;
        }
    }

    private static Settings Parse(string[] args)
    {
        (bool includeAutomatic, Dictionary<string, string> values) = ParseValues(args);
        foreach (string name in new[] { "--corpus", "--output" })
        {
            if (!values.TryGetValue(name, out string? path) || !Path.IsPathFullyQualified(path))
            {
                throw new ArgumentException("Corpus and output must be absolute paths.");
            }
        }

        string corpus = Path.GetFullPath(values["--corpus"]), output = Path.GetFullPath(values["--output"]);
        if (!Directory.Exists(corpus))
        {
            throw new ArgumentException("Corpus directory must exist.");
        }

        _ = values.Remove("--corpus");
        _ = values.Remove("--output");
        _ = values.TryAdd("--iterations", "20");
        _ = values.TryAdd("--blocks", "6");
        var selection = DiagnosticSelection.Parse(values, KernelDiagnosticInputs.Ids);
        RequireEvenBlocks(selection.Policy.Blocks);
        return new(corpus, output, selection, includeAutomatic);
    }

    private static (bool IncludeAutomatic, Dictionary<string, string> Values) ParseValues(string[] args)
    {
        if (args.Count(value => value == "--compare-kernels") != 1)
        {
            throw new ArgumentException("Supply --compare-kernels exactly once.");
        }

        int includeAutomatic = args.Count(value => value == "--include-automatic");
        if (includeAutomatic > 1)
        {
            throw new ArgumentException("Supply --include-automatic at most once.");
        }

        string[] pairs = args.Where(value => value is not ("--compare-kernels" or "--include-automatic")).ToArray();
        if (pairs.Length % 2 != 0)
        {
            throw new ArgumentException("Kernel comparison options require name/value pairs.");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < pairs.Length; index += 2)
        {
            if (!values.TryAdd(pairs[index], pairs[index + 1]))
            {
                throw new ArgumentException("Duplicate kernel comparison option.");
            }
        }

        return (includeAutomatic == 1, values);
    }

    private static int ExpectedComparisons(int cases, bool narrowApplicable, bool includeAutomatic)
        => cases * ((narrowApplicable ? 2 : 1) + (includeAutomatic ? 1 : 0));

    private static string PreferenceName(JpegliEncoderOptions settings)
        => $"fuzzy={settings.FuzzyErosionKernel.ToString().ToLowerInvariant()},narrow={settings.NarrowZigZagKernel.ToString().ToLowerInvariant()}";

    private static void RequireEvenBlocks(int blocks)
    {
        if (blocks is < 4 or > 6 || (blocks & 1) != 0)
        {
            throw new ArgumentException("Kernel comparisons require four or six quartets for balanced ABBA/BAAB orders.");
        }
    }

    private static int Iterations(int width, int height, int requested)
        => (long)width * height < 1024 ? Math.Max(requested, 100) : requested;
    private static string Order(int comparison, int block) => (comparison + block) % 2 == 0 ? "ABBA" : "BAAB";
    private static byte[] Encode(PpmImage image, JpegliEncoderOptions settings)
        => JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, settings);
    private static void RequireSameBytes(byte[] expected, byte[] actual)
    {
        if (!actual.AsSpan().SequenceEqual(expected))
        {
            throw new InvalidDataException("Original, optimized and automatic kernels must produce identical JPEG bytes.");
        }
    }

    private static Leg Measure(PpmImage image, JpegliEncoderOptions settings, byte[] expected,
        int iterations, int warmups, int position, string variant)
    {
        for (int warmup = 0; warmup < warmups; warmup++)
        {
            RequireSameBytes(expected, Encode(image, settings));
        }

        int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
        long allocated = GC.GetTotalAllocatedBytes(precise: true), ticks = 0;
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            long start = Stopwatch.GetTimestamp();
            byte[] jpeg = Encode(image, settings);
            ticks += Stopwatch.GetTimestamp() - start;
            RequireSameBytes(expected, jpeg);
            GC.KeepAlive(jpeg);
        }
        long allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocated;
        return new(position, variant, ticks * (1_000_000_000.0 / Stopwatch.Frequency) / iterations,
            allocatedBytes, GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2);
    }

    private static double Ratio(Quartet quartet, string candidateVariant)
        => Math.Exp(quartet.Legs.Where(leg => leg.Variant == candidateVariant).Average(leg => Math.Log(leg.NsPerEncode))
            - quartet.Legs.Where(leg => leg.Variant == "original").Average(leg => Math.Log(leg.NsPerEncode)));
    private static double Drift(IEnumerable<double> values)
    {
        double[] samples = values.ToArray();
        return samples.Max() / samples.Min();
    }

    internal static string[] Validate(Quartet[] quartets, int iterations, Policy policy, int comparisonIndex,
        string candidateVariant = "optimized")
    {
        var invalid = new List<string>();
        if (candidateVariant is not ("optimized" or "automatic"))
        {
            invalid.Add("Unknown candidate variant.");
        }

        if (policy.Blocks is < 4 or > 6 || (policy.Blocks & 1) != 0)
        {
            invalid.Add("Quartet count must be four or six for balanced ABBA/BAAB orders.");
        }

        if (iterations < policy.MinimumIterations || iterations > policy.MaximumIterations)
        {
            invalid.Add("Iteration count violates its declared bounds.");
        }

        if (quartets.Length != policy.Blocks || !quartets.Select(block => block.Block).Order().SequenceEqual(Enumerable.Range(0, policy.Blocks)))
        {
            invalid.Add("Missing, duplicate or extra quartets.");
        }

        foreach (Quartet block in quartets)
        {
            if (block.Order != Order(comparisonIndex, block.Block) || block.Legs.Length != 4)
            {
                invalid.Add($"Quartet {block.Block}: invalid ABBA/BAAB layout.");
                continue;
            }
            for (int position = 0; position < block.Legs.Length; position++)
            {
                Leg leg = block.Legs[position];
                string expected = block.Order[position] == 'A' ? "original" : candidateVariant;
                if (leg.Position != position || leg.Variant != expected)
                {
                    invalid.Add($"Quartet {block.Block}: mismatched leg identity.");
                }

                if (!double.IsFinite(leg.NsPerEncode) || leg.NsPerEncode <= 0)
                {
                    invalid.Add($"Quartet {block.Block}: nonfinite or nonpositive latency.");
                }
                else if (leg.NsPerEncode * iterations < policy.MinimumBlockMilliseconds * 1_000_000)
                {
                    invalid.Add($"Quartet {block.Block}: leg interval is below the fixed 1ms minimum.");
                }

                if (leg.AllocatedBytes < 0 || leg.Gen0Collections < 0 || leg.Gen1Collections < 0 || leg.Gen2Collections < 0)
                {
                    invalid.Add($"Quartet {block.Block}: invalid allocation or collection count.");
                }
            }
        }
        if (invalid.Count != 0)
        {
            return invalid.Distinct().ToArray();
        }

        if (quartets.GroupBy(block => block.Order).Any(group => group.Count() != policy.Blocks / 2))
        {
            invalid.Add("ABBA/BAAB orders are not balanced.");
        }

        foreach (string variant in new[] { "original", candidateVariant })
        {
            double[] values = quartets.SelectMany(block => block.Legs).Where(leg => leg.Variant == variant)
                .Select(leg => leg.NsPerEncode).ToArray();
            TimingStatistics stats = Evidence.Statistics(values);
            if (stats.RelativeMad > policy.MaximumRelativeMad)
            {
                invalid.Add($"{variant}: relative MAD exceeds10%.");
            }

            if (stats.P90 / stats.P10 > MaximumPercentileDrift)
            {
                invalid.Add($"{variant}: p90/p10 drift exceeds20%, including minority timing states.");
            }

            if (quartets.Any(block => Drift(block.Legs.Where(leg => leg.Variant == variant).Select(leg => leg.NsPerEncode)) > MaximumWithinQuartetDrift))
            {
                invalid.Add($"{variant}: repeated legs within a quartet differ by over10%.");
            }
        }
        double[] ratios = quartets.Select(block => Ratio(block, candidateVariant)).ToArray();
        if (Evidence.Statistics(ratios).RelativeMad > policy.MaximumRelativeMad)
        {
            invalid.Add("Paired-ratio relative MAD exceeds10%.");
        }

        double[] orderMedians = quartets.GroupBy(block => block.Order)
            .Select(group => Evidence.Statistics(group.Select(block => Ratio(block, candidateVariant)).ToArray()).Median).ToArray();
        if (orderMedians.Length != 2 || Drift(orderMedians) > MaximumOrderBias)
        {
            invalid.Add("ABBA/BAAB order bias exceeds10% or one order is absent.");
        }

        return invalid.Distinct().ToArray();
    }

    private static Analysis Analyze(Quartet[] quartets, int iterations, Policy policy, int comparisonIndex,
        string candidateVariant = "optimized")
    {
        string[] invalid = Validate(quartets, iterations, policy, comparisonIndex, candidateVariant);
        if (iterations <= 0 || quartets.Length == 0 || quartets.Any(block => block.Legs.Length != 4
            || block.Legs.Count(leg => leg.Variant == "original") != 2
            || block.Legs.Count(leg => leg.Variant == candidateVariant) != 2
            || block.Legs.Any(leg => !double.IsFinite(leg.NsPerEncode) || leg.NsPerEncode <= 0
                || leg.NsPerEncode * iterations < policy.MinimumBlockMilliseconds * 1_000_000)))
        {
            return new(null, null, null, null, null, null, null, null, null, null, "inconclusive",
                invalid.Length != 0 ? invalid : ["Raw samples cannot support numeric statistics."]);
        }

        Leg[] original = quartets.SelectMany(block => block.Legs).Where(leg => leg.Variant == "original").ToArray();
        Leg[] optimized = quartets.SelectMany(block => block.Legs).Where(leg => leg.Variant == candidateVariant).ToArray();
        double[] ratios = quartets.Select(block => Ratio(block, candidateVariant)).ToArray();
        TimingStatistics a = Evidence.Statistics(original.Select(leg => leg.NsPerEncode).ToArray());
        TimingStatistics b = Evidence.Statistics(optimized.Select(leg => leg.NsPerEncode).ToArray());
        double orderBias = Drift(quartets.GroupBy(block => block.Order)
            .Select(group => Evidence.Statistics(group.Select(block => Ratio(block, candidateVariant)).ToArray()).Median));
        double maximumDrift = quartets.SelectMany(block => block.Legs.GroupBy(leg => leg.Variant)
            .Select(group => Drift(group.Select(leg => leg.NsPerEncode)))).Max();
        string preference = invalid.Length != 0 ? "inconclusive"
            : ratios.All(ratio => ratio < 1 - PreferenceMargin) ? candidateVariant
            : ratios.All(ratio => ratio > 1 + PreferenceMargin) ? "original" : "inconclusive";
        return new(a, b, Evidence.Statistics(ratios), Math.Exp(ratios.Average(Math.Log)), orderBias,
            maximumDrift, a.P90 / a.P10, b.P90 / b.P10,
            original.Average(leg => (double)leg.AllocatedBytes / iterations),
            optimized.Average(leg => (double)leg.AllocatedBytes / iterations),
            preference, invalid);
    }

    private static string Report(IReadOnlyList<Result> results, IReadOnlyList<Skipped> skipped, string[] invalid)
    {
        var text = new StringBuilder("# Interleaved kernel comparison\n\n");
        _ = text.AppendLine("Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.");
        _ = text.AppendLine("\nRatios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.");
        _ = text.AppendLine("\nEvidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.");
        _ = text.AppendLine("\n| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |\n| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |");
        foreach (Result item in results)
        {
            _ = text.AppendLine(FormattableString.Invariant($"| {item.Id}/{item.Sampling}/p{item.ProgressiveLevel}/d{item.Distance:g} | {item.Kernel} | {item.BaselinePreference} | {item.CandidatePreference} | {item.OptimizedKernelReachable} | {item.Iterations} | {Metric(item.Analysis.Original?.Median / 1e6, "F4")} | {Metric(item.Analysis.Optimized?.Median / 1e6, "F4")} | {Metric(item.Analysis.PairedRatios?.Median, "F6")} | {Metric(item.Analysis.OriginalAllocatedBytesPerEncode, "F1")} | {Metric(item.Analysis.OptimizedAllocatedBytesPerEncode, "F1")} | {item.Analysis.EvidencePreference} | {item.Analysis.InvalidReasons.Length == 0} |"));
        }

        if (skipped.Count != 0)
        {
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"\n{skipped.Count} narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.");
        }

        if (invalid.Length != 0)
        {
            _ = text.AppendLine("\nInvalid evidence:");
            foreach (string reason in invalid)
            {
                _ = text.AppendLine("- " + reason);
            }
        }
        return text.ToString();
    }

    private static string Label(Result item)
        => $"{item.Id}/{item.Sampling}/p{item.ProgressiveLevel}/d{item.Distance.ToString("g", CultureInfo.InvariantCulture)}/{item.Kernel}";
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string HashFile(string path) => Hash(File.ReadAllBytes(path));
    private static string Metric(double? value, string format) => value?.ToString(format, CultureInfo.InvariantCulture) ?? "n/a";
    private static Task WriteJson<T>(string output, string name, T value)
        => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");

    private static void CheckDiagnosticInputs()
    {
        (string Id, string Category, string SourcePath, PpmImage Image)[] inputs = KernelDiagnosticInputs.Create(KernelDiagnosticInputs.Ids).ToArray();
        (string Id, string Category, string SourcePath, PpmImage Image)[] repeated = KernelDiagnosticInputs.Create(KernelDiagnosticInputs.Ids).ToArray();
        (int Width, int Height)[] dimensions = [(255, 128), (256, 128), (257, 128), (8, 4096), (256, 256), (512, 256)];
        Check(inputs.Length == dimensions.Length && inputs.Select(input => input.Id).Distinct(StringComparer.Ordinal).Count() == inputs.Length,
            "Kernel controls must have six unique identities.");
        for (int index = 0; index < inputs.Length; index++)
        {
            PpmImage image = inputs[index].Image;
            Check((image.Width, image.Height) == dimensions[index] && image.Pixels.Length == image.Width * image.Height * 3,
                "Kernel control dimensions or pixel lengths changed.");
            Check(Hash(image.Pixels) == Hash(repeated[index].Image.Pixels), "Kernel inputs must be deterministic.");
            Check(KernelPolicy.UseTiledZigZagForConfiguration(image.Width, image.Height, true, 8, true, false, true) == (index != 0),
                "Kernel controls no longer straddle the Automatic size guard.");
            Check(inputs[index].Category == "kernel-diagnostic" && inputs[index].SourcePath == "generated:" + inputs[index].Id,
                "Kernel control provenance is ambiguous.");
        }
        for (int y = 0; y < dimensions[0].Height; y++)
        {
            ReadOnlySpan<byte> below = inputs[0].Image.Pixels.AsSpan(y * dimensions[0].Width * 3, dimensions[0].Width * 3);
            Check(below.SequenceEqual(inputs[1].Image.Pixels.AsSpan(y * dimensions[1].Width * 3, below.Length))
                && below.SequenceEqual(inputs[2].Image.Pixels.AsSpan(y * dimensions[2].Width * 3, below.Length)),
                "Neighboring guard inputs must preserve their common pixels.");
        }
        for (int y = 0; y < dimensions[4].Height; y++)
        {
            ReadOnlySpan<byte> intermediate = inputs[4].Image.Pixels.AsSpan(y * dimensions[4].Width * 3, dimensions[4].Width * 3);
            Check(intermediate.SequenceEqual(inputs[5].Image.Pixels.AsSpan(y * dimensions[5].Width * 3, intermediate.Length)),
                "Intermediate controls must preserve their common pixels across different row strides.");
            if (y < dimensions[1].Height)
            {
                Check(intermediate.SequenceEqual(inputs[1].Image.Pixels.AsSpan(y * dimensions[1].Width * 3, intermediate.Length)),
                    "Intermediate controls must preserve common pixels with the guard control.");
            }
        }
        Check(!KernelDiagnosticInputs.Create(["kodim01"]).Any(), "Unselected kernel controls must not be generated.");
        Check(KernelDiagnosticInputs.Create(["intermediate-128k", "guard-at-32k"]).Select(input => input.Id)
            .SequenceEqual(["intermediate-128k", "guard-at-32k"]),
            "Kernel generation must honor the selected image list.");
        string[] ids = ["kodim01", "low-noise", "low-contrast", "tiny-odd-edges", "guard-below-32k", "guard-at-32k", "guard-above-32k", "narrow-tall"];
        var selectionValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["--images"] = string.Join(',', ids),
            ["--distances"] = "1,4"
        };
        var selection = DiagnosticSelection.Parse(selectionValues, KernelDiagnosticInputs.Ids);
        Check(selection.ExpectedCases == 64, "Kernel controls must fit the existing bounded diagnostic selection.");
        RejectArguments(() => DiagnosticSelection.Parse(selectionValues));
        selectionValues["--images"] = "intermediate-64k,intermediate-128k";
        var intermediateSelection = DiagnosticSelection.Parse(selectionValues, KernelDiagnosticInputs.Ids);
        Check(intermediateSelection.ExpectedCases == 16,
            "Intermediate controls must use a separate bounded selection with both distances, samplings and progressions.");
        RejectArguments(() => DiagnosticSelection.Parse(selectionValues));
        selectionValues["--images"] = string.Join(',', ids.Concat(intermediateSelection.Images));
        RejectArguments(() => DiagnosticSelection.Parse(selectionValues, KernelDiagnosticInputs.Ids));
        Check(Evidence.ControlledIds.Length == 8 && !Evidence.ControlledIds.Intersect(KernelDiagnosticInputs.Ids).Any(),
            "Kernel controls must not change the fixed acceptance corpus.");
    }

    internal static void SelfTest()
    {
        CheckDiagnosticInputs();
        (bool includeAutomatic, Dictionary<string, string> values) = ParseValues(["--compare-kernels", "--iterations", "20"]);
        Check(!includeAutomatic && values["--iterations"] == "20",
            "Automatic policy comparisons must be disabled by default.");
        (bool enabledIncludeAutomatic, Dictionary<string, string> enabledValues) = ParseValues(["--compare-kernels", "--include-automatic", "--iterations", "20"]);
        Check(enabledIncludeAutomatic && enabledValues.Count == 1 && enabledValues["--iterations"] == "20",
            "Optional automatic flag must not become a diagnostic name/value option.");
        RejectArguments(() => ParseValues(["--compare-kernels", "--include-automatic", "--include-automatic"]));
        RejectArguments(() => ParseValues(["--compare-kernels", "--include-automatic", "false"]));
        RejectArguments(() => ParseValues(["--include-automatic"]));
        Check(ExpectedComparisons(64, true, false) == 128 && ExpectedComparisons(64, false, false) == 64,
            "Default forced-pair comparison counts changed.");
        Check(ExpectedComparisons(64, true, true) == 192 && ExpectedComparisons(64, false, true) == 128,
            "Automatic comparisons must add one measured pair per case even without a reachable new kernel.");
        Check(PreferenceName(new JpegliEncoderOptions
        {
            FuzzyErosionKernel = KernelPreference.Original,
            NarrowZigZagKernel = KernelPreference.Original
        }) == "fuzzy=original,narrow=original" && PreferenceName(new JpegliEncoderOptions()) == "fuzzy=automatic,narrow=automatic",
            "Baseline and automatic candidate preference names are ambiguous.");
        var policy = new Policy { Blocks = 6, MinimumIterations = 20, MaximumIterations = 20 };
        Quartet[] samples = Enumerable.Range(0, policy.Blocks).Select(block =>
        {
            string order = Order(0, block);
            return new Quartet(block, order, order.Select((variant, position) =>
                new Leg(position, variant == 'A' ? "original" : "optimized", variant == 'A' ? 1_000_000 : 800_000,
                    2000, 0, 0, 0)).ToArray());
        }).ToArray();
        Check(Validate(samples, 20, policy, 0).Length == 0, "Stable ABBA/BAAB samples rejected.");
        Analysis result = Analyze(samples, 20, policy, 0);
        Check(result.PairedRatios is not null && Math.Abs(result.PairedRatios.Median - .8) < 1e-12, "Paired ratio calculation is incorrect.");
        Check(result.OriginalAllocatedBytesPerEncode == 100 && result.OrderBias == 1,
            "Allocation or order-bias calculation is incorrect.");
        Check(Iterations(17, 13, 20) == 100 && Iterations(32, 32, 20) == 20,
            "Small-input iteration policy changed.");
        Check(result.EvidencePreference == "optimized", "Consistent large improvement must favor Optimized.");
        Quartet[] automaticSamples = samples.Select(block => block with
        {
            Legs = block.Legs.Select(leg => leg.Variant == "optimized" ? leg with { Variant = "automatic" } : leg).ToArray()
        }).ToArray();
        Analysis automaticAnalysis = Analyze(automaticSamples, 20, policy, 0, "automatic");
        Check(automaticAnalysis.InvalidReasons.Length == 0 && automaticAnalysis.EvidencePreference == "automatic"
            && automaticAnalysis.PairedRatios is not null && Math.Abs(automaticAnalysis.PairedRatios.Median - .8) < 1e-12,
            "Automatic candidate legs must retain the same paired statistics without being labeled optimized.");
        string automaticJson = JsonSerializer.Serialize(automaticAnalysis, JsonOptions);
        Check(automaticJson.Contains("\"candidate\"", StringComparison.Ordinal)
            && !automaticJson.Contains("\"optimized\"", StringComparison.Ordinal),
            "Automatic analysis must use neutral baseline/candidate JSON labels.");
        Check(samples.Count(block => block.Order == "ABBA") == 3 && samples.Count(block => block.Order == "BAAB") == 3,
            "Six quartets must balance both orders.");
        RequireEvenBlocks(6);
        bool oddRejected = false;
        try
        { RequireEvenBlocks(7); }
        catch (ArgumentException) { oddRejected = true; }
        Check(oddRejected, "Odd kernel quartet count accepted.");
        Check(Validate(samples, 20, policy with { Blocks = 7 }, 0).Length != 0,
            "Odd analysis quartet policy accepted.");
        Check(Validate(samples[..5], 20, policy, 0).Length != 0, "Missing quartet accepted.");
        Check(Validate(samples, 20, policy, 1).Length != 0, "Wrong comparison order accepted.");
        Quartet[] shortLegs = samples.Select(block => block with
        { Legs = block.Legs.Select(leg => leg with { NsPerEncode = 1 }).ToArray() }).ToArray();
        Check(Validate(shortLegs, 20, policy, 0).Any(reason => reason.Contains("1ms", StringComparison.Ordinal)),
            "Subminimum timer intervals accepted.");
        Quartet[] drift = samples.Select(block => block with
        { Legs = block.Legs.Select(leg => leg with { NsPerEncode = leg.NsPerEncode * (block.Block < 2 ? 1.5 : 1) }).ToArray() }).ToArray();
        Check(Evidence.Statistics(drift.SelectMany(block => block.Legs).Where(leg => leg.Variant == "original")
            .Select(leg => leg.NsPerEncode).ToArray()).RelativeMad == 0, "Minority-state fixture must evade MAD.");
        Check(Validate(drift, 20, policy, 0).Any(reason => reason.Contains("p90/p10", StringComparison.Ordinal)),
            "Minority timing-state transition accepted despite zero MAD.");
        Quartet[] spike = samples.Select(block => block with { Legs = block.Legs.ToArray() }).ToArray();
        spike[0].Legs[0] = spike[0].Legs[0] with { NsPerEncode = 1_500_000 };
        Check(Validate(spike, 20, policy, 0).Any(reason => reason.Contains("repeated legs", StringComparison.Ordinal)),
            "Within-quartet timing transition accepted.");
        Quartet[] orderBias = samples.Select(block => block with
        {
            Legs = block.Legs.Select(leg => leg.Variant == "optimized" && block.Order == "BAAB"
                ? leg with { NsPerEncode = 1_000_000 } : leg).ToArray()
        }).ToArray();
        Check(Validate(orderBias, 20, policy, 0).Any(reason => reason.Contains("order bias", StringComparison.Ordinal)),
            "Measurement-order bias accepted.");
        Check(Analyze(drift, 20, policy, 0).EvidencePreference == "inconclusive",
            "Invalid drift samples must never favor a kernel.");
        Quartet[] slow = samples.Select(block => block with
        {
            Legs = block.Legs.Select(leg => leg.Variant == "optimized"
            ? leg with { NsPerEncode = 1_200_000 } : leg).ToArray()
        }).ToArray();
        Check(Analyze(slow, 20, policy, 0).EvidencePreference == "original", "Consistent slowdown must favor Original.");
        Quartet[] small = samples.Select(block => block with
        {
            Legs = block.Legs.Select(leg => leg.Variant == "optimized"
            ? leg with { NsPerEncode = 997_000 } : leg).ToArray()
        }).ToArray();
        Check(Analyze(small, 20, policy, 0).EvidencePreference == "inconclusive", "Small effects must remain inconclusive.");
        Quartet[] mixed = samples.Select(block => block with
        {
            Legs = block.Legs.Select(leg => leg.Variant == "optimized"
            ? leg with { NsPerEncode = block.Block == 0 ? 1_001_000 : 990_000 } : leg).ToArray()
        }).ToArray();
        Check(Analyze(mixed, 20, policy, 0).EvidencePreference == "inconclusive", "Ratios crossing1 must remain inconclusive.");
        Quartet[] nonfinite = samples.Select(block => block with { Legs = block.Legs.ToArray() }).ToArray();
        nonfinite[0].Legs[0] = nonfinite[0].Legs[0] with { NsPerEncode = double.NaN };
        Analysis nonfiniteAnalysis = Analyze(nonfinite, 20, policy, 0);
        Check(nonfiniteAnalysis.Original is null && nonfiniteAnalysis.EvidencePreference == "inconclusive"
            && nonfiniteAnalysis.InvalidReasons.Length != 0, "Nonfinite samples must fail before numeric statistics.");
        Check(JsonSerializer.Serialize(new { quartets = nonfinite, analysis = nonfiniteAnalysis }, JsonOptions)
            .Contains("\"NaN\"", StringComparison.Ordinal), "Nonfinite raw samples must remain serializable.");
        Quartet[] malformed = samples.Select(block => block with { Legs = block.Legs.ToArray() }).ToArray();
        malformed[0] = malformed[0] with { Legs = malformed[0].Legs[..3] };
        Analysis malformedAnalysis = Analyze(malformed, 20, policy, 0);
        Check(malformedAnalysis.PairedRatios is null && malformedAnalysis.InvalidReasons.Length != 0,
            "Malformed quartets must fail before numeric statistics.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }

    private static void RejectArguments(Action action)
    {
        bool rejected = false;
        try
        { action(); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid kernel comparison flags were accepted.");
    }
}
