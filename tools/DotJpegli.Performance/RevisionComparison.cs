namespace DotJpegli.Performance;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>CI-only, steady-state comparisons of two statically built encoder revisions.</summary>
internal static class RevisionComparison
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
    private static readonly string[] HostFields =
    [
        "cpu", "affinity", "os", "osArchitecture", "processArchitecture", "runtime", "runtimeVersion",
        "corelibInformationalVersion", "runtimeProductVersion", "encoderTargetFramework", "explicitNet11Simd",
        "logicalProcessors", "frequency", "isHighResolution", "isHardwareAccelerated", "vectorFloatWidth",
        "vector128", "vector256", "vector512", "serverGc", "gcLatencyMode", "dotnetTieredCompilation",
        "complusTieredCompilation", "instrumentation", "githubRunnerOs", "githubRunnerArchitecture"
    ];

    private sealed record Settings(string BaselineRunner, string BaselineRevision, string CurrentRevision,
        string Corpus, string Output, DiagnosticSelection Selection);
    internal sealed record Leg(int Position, string Variant, RevisionWorker.Measurement Measurement);
    internal sealed record Quartet(int Block, string Order, Leg[] Legs);
    private sealed record Result(string Id, string Category, int Width, int Height, int Sampling,
        int ProgressiveLevel, float Distance, int Iterations, string BaselineJpegSha256,
        string CurrentJpegSha256, Quartet[] Quartets, KernelComparison.Analysis Analysis);

    internal static async Task<int> Run(string[] args)
    {
        Program.RequireCi();
        string? output = null;
        try
        {
            Settings options = Parse(args);
            if (Directory.Exists(options.Output) && Directory.EnumerateFileSystemEntries(options.Output).Any())
            {
                throw new InvalidDataException("Revision evidence output directory must be empty.");
            }

            output = options.Output;
            _ = Directory.CreateDirectory(output);
            _ = Directory.CreateDirectory(Path.Combine(output, "inputs"));
            _ = Directory.CreateDirectory(Path.Combine(output, "raw-comparisons"));
            DiagnosticSelection selection = options.Selection;
            var results = new List<Result>();
            var sources = new List<SourceImage>();
            await WriteJson(output, "summary.json", new
            {
                schemaVersion = 1,
                mode = "interleaved-revision-comparison",
                status = "incomplete",
                valid = false,
                expectedComparisons = selection.ExpectedCases,
                parityVerdict = "not-evaluated"
            });

            await using RevisionWorkerClient baseline = await RevisionWorkerClient.Start(options.BaselineRunner, options.BaselineRevision);
            await using RevisionWorkerClient current = await RevisionWorkerClient.Start(Assembly.GetExecutingAssembly().Location, options.CurrentRevision);
            await WriteJson(output, "run.json", new
            {
                schemaVersion = 1,
                mode = "interleaved-revision-comparison",
                startedAtUtc = DateTimeOffset.UtcNow,
                options.BaselineRevision,
                options.CurrentRevision,
                githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
                baseline = baseline.Identity,
                current = current.Identity,
                corpusManifestSha256 = HashFile(Path.Combine(AppContext.BaseDirectory, "corpus.json")),
                selection,
                expectedComparisons = selection.ExpectedCases,
                method = new
                {
                    scope = "Selected managed revision diagnostics; no native or full performance parity verdict",
                    order = "Balanced alternating ABBA/BAAB quartets by case and block index; A=baseline, B=current",
                    workers = "Two persistent processes using identical runner bytes and statically built encoder libraries; only one worker encodes at a time",
                    latency = "Complete public byte-array Encode calls; process startup, input loading, warmups and JPEG hash checks excluded",
                    iterations = "Identical fixed iterations and warmups for both revisions; inputs below1024 pixels use max(requested,100)",
                    ratio = "Median of per-quartet geometric-mean current/baseline ratios",
                    validation = "Relative MAD <=10%, repeated legs within each quartet <=10% drift, p90/p10 <=1.20 and ABBA/BAAB order bias <=10%",
                    preference = "Valid rows require every quartet beyond1+/-0.005 in the same direction; other valid rows are inconclusive",
                    gc = "Natural collections included; no forced GC or no-GC region; allocations and collections retained per leg",
                    memory = "Allocated bytes do not measure retained pool capacity or peak working memory",
                    rawMeasurements = "raw-comparisons/*.json updated after every completed quartet, before analysis"
                }
            });
            ValidateIdentities(baseline.Identity, current.Identity, options.BaselineRevision, options.CurrentRevision,
                Environment.GetEnvironmentVariable("DOTNET_EnableAVX512") == "0");

            foreach ((string id, string category, string sourcePath, Parity.PpmImage image) in Program.ReadCorpus(options.Corpus)
                .Where(source => selection.Images.Contains(source.Id, StringComparer.Ordinal)))
            {
                string input = Path.Combine(output, "inputs", id + ".ppm");
                image.Write(input);
                sources.Add(new(id, category, sourcePath, HashFile(input), Hash(image.Pixels), image.Width, image.Height));
                await WriteJson(output, "sources.json", sources);
                foreach (int sampling in selection.Samplings)
                {
                    foreach (int progression in selection.Progressions)
                    {
                        foreach (float distance in selection.Distances)
                        {
                            int index = results.Count;
                            int iterations = Iterations(image.Width, image.Height, selection.Policy.MinimumIterations);
                            Policy casePolicy = selection.Policy with { MinimumIterations = iterations, MaximumIterations = iterations };
                            string stem = FormattableString.Invariant($"{id}_{sampling}_p{progression}_d{distance:0}");
                            var quartets = new List<Quartet>();
                            for (int block = 0; block < selection.Policy.Blocks; block++)
                            {
                                string order = Order(index, block);
                                var legs = new Leg[4];
                                for (int position = 0; position < order.Length; position++)
                                {
                                    bool isBaseline = order[position] == 'A';
                                    RevisionWorker.Measurement measured = await (isBaseline ? baseline : current).Measure(
                                        input, sampling, progression, distance, iterations, selection.Policy.Warmups);
                                    legs[position] = new(position, isBaseline ? "baseline" : "current", measured);
                                }

                                quartets.Add(new(block, order, legs));
                                await WriteJson(Path.Combine(output, "raw-comparisons"), stem + ".json", new
                                {
                                    schemaVersion = 1,
                                    id,
                                    category,
                                    width = image.Width,
                                    height = image.Height,
                                    sampling,
                                    progressiveLevel = progression,
                                    distance,
                                    comparisonIndex = index,
                                    iterations,
                                    warmups = selection.Policy.Warmups,
                                    completedQuartets = quartets.Count,
                                    expectedQuartets = selection.Policy.Blocks,
                                    updatedAtUtc = DateTimeOffset.UtcNow,
                                    quartets
                                });
                            }

                            Quartet[] raw = quartets.ToArray();
                            KernelComparison.Analysis analysis = Analyze(raw, iterations, casePolicy, index);
                            string baselineHash = raw[0].Legs.First(leg => leg.Variant == "baseline").Measurement.JpegSha256;
                            string currentHash = raw[0].Legs.First(leg => leg.Variant == "current").Measurement.JpegSha256;
                            results.Add(new(id, category, image.Width, image.Height, sampling, progression, distance,
                                iterations, baselineHash, currentHash, raw, analysis));
                            await WriteJson(output, "cases.json", results);
                            await WriteJson(output, "progress.json", new
                            {
                                completedComparisons = results.Count,
                                expectedComparisons = selection.ExpectedCases,
                                updatedAtUtc = DateTimeOffset.UtcNow
                            });
                            Console.WriteLine(FormattableString.Invariant($"{id}/{sampling}/p{progression}/d{distance}: current/baseline={Metric(analysis.PairedRatios?.Median)}; preference={analysis.EvidencePreference}; valid={analysis.InvalidReasons.Length == 0}; {results.Count}/{selection.ExpectedCases}"));
                        }
                    }
                }
            }

            string[] invalid = results.SelectMany(result => result.Analysis.InvalidReasons.Select(reason => $"{Label(result)}: {reason}")).ToArray();
            if (results.Count != selection.ExpectedCases)
            {
                invalid = [.. invalid, "Selected comparison coverage is incomplete."];
            }

            bool valid = invalid.Length == 0;
            await WriteJson(output, "summary.json", new
            {
                schemaVersion = 1,
                mode = "interleaved-revision-comparison",
                status = valid ? "completed" : "invalid",
                valid,
                parityVerdict = "not-evaluated",
                fullCoverageComplete = false,
                comparisons = results.Count,
                expectedComparisons = selection.ExpectedCases,
                inconclusiveComparisons = results.Count(result => result.Analysis.InvalidReasons.Length == 0 && result.Analysis.EvidencePreference == "inconclusive"),
                invalidReasons = invalid
            });
            await File.WriteAllTextAsync(Path.Combine(output, "report.md"), Report(results, invalid));
            return valid ? 0 : 2;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException
            or UnauthorizedAccessException or InvalidOperationException or OverflowException or JsonException or FormatException
            or TimeoutException or System.ComponentModel.Win32Exception or NotSupportedException)
        {
            Console.Error.WriteLine($"Revision comparison invalid: {error.Message}");
            if (output is not null)
            {
                await WriteJson(output, "summary.json", new
                {
                    schemaVersion = 1,
                    mode = "interleaved-revision-comparison",
                    status = "invalid",
                    valid = false,
                    parityVerdict = "not-evaluated",
                    invalidReasons = new[] { error.Message }
                });
                await File.WriteAllTextAsync(Path.Combine(output, "report.md"), "# Interleaved revision comparison: INVALID\n\n" + error.Message + "\n");
            }

            return 2;
        }
    }

    private static Settings Parse(string[] args)
    {
        if (args.Count(argument => argument == "--compare-revisions") != 1)
        {
            throw new ArgumentException("Supply --compare-revisions exactly once.");
        }

        string[] pairs = args.Where(argument => argument != "--compare-revisions").ToArray();
        if (pairs.Length % 2 != 0)
        {
            throw new ArgumentException("Revision comparison options require name/value pairs.");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < pairs.Length; index += 2)
        {
            if (!values.TryAdd(pairs[index], pairs[index + 1]))
            {
                throw new ArgumentException("Duplicate revision comparison option.");
            }
        }

        string Required(string name)
        {
            if (!values.Remove(name, out string? value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Missing {name}.");
            }

            return value;
        }

        string runner = Path.GetFullPath(Required("--baseline-runner"));
        string baseline = Required("--baseline-revision"), current = Required("--current-revision");
        RequireRevision(baseline);
        RequireRevision(current);
        string corpus = Path.GetFullPath(Required("--corpus")), output = Path.GetFullPath(Required("--output"));
        if (!File.Exists(runner) || !Directory.Exists(corpus))
        {
            throw new ArgumentException("Baseline runner and corpus must exist.");
        }

        _ = values.TryAdd("--blocks", "6");
        _ = values.TryAdd("--iterations", "20");
        var selection = DiagnosticSelection.Parse(values);
        if (selection.Policy.Blocks is not (4 or 6))
        {
            throw new ArgumentException("Revision comparisons require four or six balanced quartets.");
        }

        return new(runner, baseline, current, corpus, output, selection);
    }

    private static void RequireRevision(string? value)
    {
        if (value is null || value.Length != 40 || value.Any(character => character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException("Revisions must be full lowercase Git commit IDs.");
        }
    }

    internal static void ValidateIdentities(RevisionWorker.Identity baseline, RevisionWorker.Identity current,
        string baselineRevision, string currentRevision, bool requireNarrow = false)
    {
        foreach ((RevisionWorker.Identity identity, string expected) in new[] { (baseline, baselineRevision), (current, currentRevision) })
        {
            RequireRevision(identity.TestedRevision);
            if (identity.TestedRevision != expected || !IsHash(identity.EncoderSha256) || !IsHash(identity.RunnerSha256)
                || identity.AssemblyInformationalVersion?.EndsWith("+" + expected, StringComparison.Ordinal) != true)
            {
                throw new InvalidDataException("Worker revision or assembly identity is invalid.");
            }

            JsonElement environment = identity.Environment;
            if (environment.ValueKind != JsonValueKind.Object || HostFields.Any(field => !environment.TryGetProperty(field, out _))
                || !environment.TryGetProperty("affinity", out JsonElement affinity)
                || affinity.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(affinity.GetString())
                || affinity.GetString()!.Any(character => character is < '0' or > '9')
                || !environment.TryGetProperty("instrumentation", out JsonElement instrumentation) || instrumentation.ValueKind != JsonValueKind.Null)
            {
                throw new InvalidDataException("Workers must be uninstrumented and pinned to one CPU.");
            }

            if (requireNarrow && (environment.GetProperty("vector256").ValueKind != JsonValueKind.True
                || environment.GetProperty("vector512").ValueKind != JsonValueKind.False
                || !environment.GetProperty("vectorFloatWidth").TryGetInt32(out int width) || width != 8))
            {
                throw new InvalidDataException("The narrow comparison requires accelerated eight-lane256-bit SIMD without512-bit acceleration.");
            }
        }

        if (baseline.RunnerSha256 != current.RunnerSha256 || !JsonElement.DeepEquals(baseline.Environment, current.Environment))
        {
            throw new InvalidDataException("Workers must use identical runner bytes, CPU, runtime, SIMD capabilities and encoding environment.");
        }
    }

    internal static KernelComparison.Analysis Analyze(Quartet[] quartets, int iterations, Policy policy, int comparisonIndex)
    {
        KernelComparison.Quartet[] samples = quartets.Select(quartet => new KernelComparison.Quartet(quartet.Block, quartet.Order,
            quartet.Legs.Select(leg => new KernelComparison.Leg(leg.Position,
                leg.Variant == "baseline" ? "original" : leg.Variant == "current" ? "optimized" : leg.Variant,
                leg.Measurement.NsPerEncode, leg.Measurement.AllocatedBytes, leg.Measurement.Gen0Collections,
                leg.Measurement.Gen1Collections, leg.Measurement.Gen2Collections)).ToArray())).ToArray();
        KernelComparison.Analysis analyzed = KernelComparison.Analyze(samples, iterations, policy, comparisonIndex);
        var invalid = analyzed.InvalidReasons.Select(reason => reason.Replace("original", "baseline", StringComparison.Ordinal)
            .Replace("optimized", "current", StringComparison.Ordinal)).ToList();
        RevisionWorker.Measurement[] measurements = quartets.SelectMany(quartet => quartet.Legs).Select(leg => leg.Measurement).ToArray();
        if (measurements.Length == 0 || measurements.Any(measurement => !IsHash(measurement.JpegSha256) || measurement.JpegBytes <= 0)
            || measurements.Select(measurement => (measurement.JpegSha256, measurement.JpegBytes)).Distinct().Count() != 1)
        {
            invalid.Add("Baseline and current must produce identical deterministic JPEG bytes in every leg.");
        }

        string preference = invalid.Count != 0 ? "inconclusive" : analyzed.EvidencePreference switch
        {
            "original" => "baseline",
            "optimized" => "current",
            _ => "inconclusive"
        };
        return analyzed with { EvidencePreference = preference, InvalidReasons = invalid.ToArray() };
    }

    private static bool IsHash(string? value) => value is { Length: 64 } && value.All(character => character is (>= '0' and <= '9') or (>= 'a' and <= 'f'));
    private static string Order(int comparison, int block) => (comparison + block) % 2 == 0 ? "ABBA" : "BAAB";
    private static int Iterations(int width, int height, int requested) => (long)width * height < 1024 ? Math.Max(requested, 100) : requested;
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static string HashFile(string path) => Hash(File.ReadAllBytes(path));
    private static async Task WriteJson<T>(string output, string name, T value)
        => await File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions));
    private static string Metric(double? value) => value?.ToString("F6", CultureInfo.InvariantCulture) ?? "n/a";
    private static string Label(Result result) => FormattableString.Invariant($"{result.Id}/{result.Sampling}/p{result.ProgressiveLevel}/d{result.Distance}");

    private static string Report(IReadOnlyList<Result> results, string[] invalid)
    {
        var report = new StringBuilder("# Interleaved revision comparison\n\n");
        _ = report.AppendLine("Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.");
        _ = report.AppendLine("\nMAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the selected timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than0.5% in the same direction. This diagnostic has no native or whole-corpus parity verdict.");
        _ = report.AppendLine("\n| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference |\n| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |");
        foreach (Result result in results)
        {
            KernelComparison.Analysis analysis = result.Analysis;
            _ = report.AppendLine(FormattableString.Invariant($"| {Label(result)} | {Metric(analysis.PairedRatios?.Median)} | {Metric(analysis.OrderBias)} | {Metric(analysis.MaximumWithinQuartetDrift)} | {Metric(analysis.OriginalPercentileDrift)} | {Metric(analysis.OptimizedPercentileDrift)} | {Metric(analysis.OriginalAllocatedBytesPerEncode)} | {Metric(analysis.OptimizedAllocatedBytesPerEncode)} | {analysis.InvalidReasons.Length == 0} | {analysis.EvidencePreference} |"));
        }

        if (invalid.Length != 0)
        {
            _ = report.AppendLine("\nInvalid evidence:\n");
            foreach (string reason in invalid)
            {
                _ = report.AppendLine("- " + reason);
            }
        }

        return report.ToString();
    }

    internal static void SelfTest()
    {
        string baselineRevision = new('1', 40), currentRevision = new('2', 40), hash = new('a', 64);
        Dictionary<string, object?> environmentFields = HostFields.ToDictionary(field => field, _ => (object?)null, StringComparer.Ordinal);
        environmentFields["cpu"] = "Synthetic CPU";
        environmentFields["affinity"] = "0";
        environmentFields["vector256"] = true;
        environmentFields["vector512"] = false;
        environmentFields["vectorFloatWidth"] = 8;
        JsonElement environment = JsonSerializer.SerializeToElement(environmentFields);
        var baseline = new RevisionWorker.Identity(hash, hash, baselineRevision, environment, "1.0.0+" + baselineRevision);
        var current = new RevisionWorker.Identity(new string('b', 64), hash, currentRevision, environment, "1.0.0+" + currentRevision);
        ValidateIdentities(baseline, current, baselineRevision, currentRevision, requireNarrow: true);
        Reject(() => ValidateIdentities(baseline, current with { TestedRevision = baselineRevision }, baselineRevision, currentRevision));
        Reject(() => ValidateIdentities(baseline, current with { RunnerSha256 = new string('c', 64) }, baselineRevision, currentRevision));
        Reject(() => ValidateIdentities(baseline, current with { AssemblyInformationalVersion = "1.0.0+" + baselineRevision }, baselineRevision, currentRevision));
        foreach ((string name, object? changed) in new (string, object?)[]
        {
            ("affinity", "0-1"), ("instrumentation", "profile"), ("runtime", "different runtime"),
            ("vector512", true), ("vector256", false), ("vectorFloatWidth", 4)
        })
        {
            var mutated = new Dictionary<string, object?>(environmentFields, StringComparer.Ordinal) { [name] = changed };
            JsonElement altered = JsonSerializer.SerializeToElement(mutated);
            Reject(() => ValidateIdentities(baseline, current with { Environment = altered }, baselineRevision, currentRevision, requireNarrow: true));
            if (name.StartsWith("vector", StringComparison.Ordinal))
            {
                Reject(() => ValidateIdentities(baseline with { Environment = altered }, current with { Environment = altered }, baselineRevision, currentRevision, requireNarrow: true));
            }
        }

        var missing = new Dictionary<string, object?>(environmentFields, StringComparer.Ordinal);
        _ = missing.Remove("cpu");
        JsonElement incomplete = JsonSerializer.SerializeToElement(missing);
        Reject(() => ValidateIdentities(baseline with { Environment = incomplete }, current with { Environment = incomplete }, baselineRevision, currentRevision));

        var policy = new Policy { Blocks = 6, MinimumIterations = 20, MaximumIterations = 20 };
        Quartet[] samples = Enumerable.Range(0, policy.Blocks).Select(block =>
        {
            string order = Order(0, block);
            return new Quartet(block, order, order.Select((variant, position) => new Leg(position,
                variant == 'A' ? "baseline" : "current", new RevisionWorker.Measurement(
                    variant == 'A' ? 1_000_000 : 800_000, 2000, 0, 0, 0, hash, 1024))).ToArray());
        }).ToArray();
        KernelComparison.Analysis stable = Analyze(samples, 20, policy, 0);
        Check(stable.InvalidReasons.Length == 0 && stable.EvidencePreference == "current"
            && Math.Abs(stable.PairedRatios!.Median - .8) < 1e-12, "Stable revision quartet analysis failed.");
        Check(samples.Count(sample => sample.Order == "ABBA") == 3 && samples.Count(sample => sample.Order == "BAAB") == 3,
            "Revision quartet orders must balance.");
        Check(Iterations(17, 13, 20) == 100 && Iterations(32, 32, 20) == 20, "Small input iteration policy changed.");
        Quartet[] jpegMismatch = Mutate(samples, (block, leg) => block == 0 && leg.Position == 0
            ? leg with { Measurement = leg.Measurement with { JpegSha256 = new string('b', 64) } } : leg);
        Check(Analyze(jpegMismatch, 20, policy, 0).InvalidReasons.Any(reason => reason.Contains("JPEG", StringComparison.Ordinal)), "JPEG mismatch accepted.");
        Quartet[] minority = Mutate(samples, (block, leg) => block < 2
            ? leg with { Measurement = leg.Measurement with { NsPerEncode = leg.Measurement.NsPerEncode * 1.5 } } : leg);
        KernelComparison.Analysis minorityAnalysis = Analyze(minority, 20, policy, 0);
        Check(minorityAnalysis.Original!.RelativeMad == 0 && minorityAnalysis.InvalidReasons.Any(reason => reason.Contains("p90/p10", StringComparison.Ordinal))
            && minorityAnalysis.EvidencePreference == "inconclusive", "Zero-MAD minority timing states accepted.");
        Quartet[] spike = Mutate(samples, (block, leg) => block == 0 && leg.Position == 0
            ? leg with { Measurement = leg.Measurement with { NsPerEncode = 1_500_000 } } : leg);
        Check(Analyze(spike, 20, policy, 0).InvalidReasons.Any(reason => reason.Contains("repeated legs", StringComparison.Ordinal)), "Single-leg drift accepted.");
        Quartet[] orderBias = Mutate(samples, (block, leg) => Order(0, block) == "BAAB" && leg.Variant == "current"
            ? leg with { Measurement = leg.Measurement with { NsPerEncode = 1_000_000 } } : leg);
        Check(Analyze(orderBias, 20, policy, 0).InvalidReasons.Any(reason => reason.Contains("order bias", StringComparison.Ordinal)), "Revision order bias accepted.");
        Quartet[] tinyEffect = Mutate(samples, (_, leg) => leg.Variant == "current"
            ? leg with { Measurement = leg.Measurement with { NsPerEncode = 997_000 } } : leg);
        Check(Analyze(tinyEffect, 20, policy, 0).EvidencePreference == "inconclusive", "Small revision effect must remain inconclusive.");
        Check(Analyze(samples[..5], 20, policy, 0).InvalidReasons.Length != 0, "Incomplete quartets accepted.");
        Check(Analyze(samples, 21, policy, 0).InvalidReasons.Length != 0, "Mismatched iterations accepted.");
        string[] arguments = ["--compare-revisions", "--baseline-runner", Assembly.GetExecutingAssembly().Location,
            "--baseline-revision", baselineRevision, "--current-revision", currentRevision,
            "--corpus", AppContext.BaseDirectory, "--output", Path.Combine(AppContext.BaseDirectory, "synthetic-output")];
        Settings options = Parse(arguments);
        Check(options.Selection.Policy.Blocks == 6 && options.Selection.Policy.MinimumIterations == 20
            && options.Selection.Policy.MaximumIterations == 20, "Revision comparison defaults changed.");
        Reject(() => Parse([.. arguments, "--blocks", "5"]));
        Reject(() => Parse([.. arguments, "--iterations", "0"]));
        Reject(() => Parse([.. arguments, "--unknown", "value"]));
        Reject(() => Parse([.. arguments, "--current-revision", currentRevision]));
        Console.WriteLine("Revision comparison self-test passed (synthetic identities and timings only).");
    }

    private static Quartet[] Mutate(Quartet[] samples, Func<int, Leg, Leg> mutation)
        => samples.Select(sample => sample with { Legs = sample.Legs.Select(leg => mutation(sample.Block, leg)).ToArray() }).ToArray();

    private static void Reject(Action action)
    {
        bool rejected = false;
        try
        {
            action();
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException)
        {
            rejected = true;
        }

        Check(rejected, "Invalid revision comparison evidence or options accepted.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
