using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotJpegli.Parity;

namespace DotJpegli.Performance;

internal static class Program
{
    private const string UpstreamPin = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<int> Main(string[] args)
    {
        string? output = null;
        try
        {
            if (args.SequenceEqual(new[] { "--self-test" }))
            {
                SelfTest.Run();
                Console.WriteLine("Performance evidence self-test passed (synthetic timings only; no benchmark executed).");
                return 0;
            }
            RequireCi();
            Options options = Options.Parse(args);
            if (Directory.Exists(options.Output) && Directory.EnumerateFileSystemEntries(options.Output).Any())
                throw new InvalidDataException("Evidence output directory must be empty to prevent stale evidence.");
            Directory.CreateDirectory(options.Output);
            output = options.Output;
            string inputs = Path.Combine(output, "inputs"), jpegs = Path.Combine(output, "jpegs");
            Directory.CreateDirectory(inputs);
            Directory.CreateDirectory(jpegs);
            string worker = Path.Combine(Path.GetTempPath(), "DotJpegli-performance-native-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(worker);
            string nativeVersion = (await Native(options.Native, worker, output, "version")).Trim();
            if (nativeVersion != UpstreamPin) throw new InvalidDataException("Native encoder does not report the pinned upstream revision.");
            var images = ReadCorpus(options.Corpus);
            var sources = new List<SourceImage>();
            var cases = new List<CaseResult>();
            var policy = new Policy();
            foreach (var (id, category, sourcePath, image) in images)
            {
                string input = Path.Combine(inputs, id + ".ppm");
                image.Write(input);
                sources.Add(new(id, category, sourcePath, HashFile(input), Hash(image.Pixels), image.Width, image.Height));
                await WriteJson(output, "sources.json", sources);
                foreach (int sampling in Evidence.Samplings)
                    foreach (int progression in Evidence.Progressions)
                        foreach (float distance in Evidence.Distances)
                        {
                            int caseIndex = cases.Count;
                            var settings = new JpegliEncoderOptions
                            {
                                ColorSpace = JpegColorSpace.YCbCr, Distance = distance, ProgressiveLevel = progression,
                                Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                                OptimizeHuffmanTables = true, AdaptiveQuantization = true, ForceBaseline = true,
                                MaxDegreeOfParallelism = policy.ManagedWorkers
                            };
                            string stem = $"{id}_{sampling}_p{progression}_d{distance.ToString("0", CultureInfo.InvariantCulture)}";
                            string managedPath = Path.Combine(jpegs, stem + "_managed.jpg"), nativePath = Path.Combine(jpegs, stem + "_native.jpg");
                            byte[] managedJpeg = Encode(image, settings);
                            await File.WriteAllBytesAsync(managedPath, managedJpeg);
                            var calibration = await BenchmarkNative(options, worker, image, input, distance, sampling, progression, policy.MinimumIterations, policy);
                            int iterations = Evidence.Iterations(calibration.NsPerEncode, policy);
                            var blocks = new List<BlockSample>();
                            long nativeBytes = 0;
                            string? expectedNativeHash = null;
                            for (int block = 0; block < policy.Blocks; block++)
                            {
                                NativeSample nativeSample;
                                ManagedSample managedSample;
                                bool nativeFirst = (caseIndex + block) % 2 == 0;
                                if (nativeFirst)
                                {
                                    nativeSample = await BenchmarkNative(options, worker, image, input, distance, sampling, progression, iterations, policy, nativePath);
                                    managedSample = BenchmarkManaged(image, settings, managedJpeg, iterations, policy.Warmups);
                                }
                                else
                                {
                                    managedSample = BenchmarkManaged(image, settings, managedJpeg, iterations, policy.Warmups);
                                    nativeSample = await BenchmarkNative(options, worker, image, input, distance, sampling, progression, iterations, policy, nativePath);
                                }
                                if (block != 0 && nativeSample.Bytes != nativeBytes) throw new InvalidDataException("Native byte count changed across independent blocks.");
                                string nativeHash = HashFile(nativePath);
                                if (expectedNativeHash is not null && nativeHash != expectedNativeHash)
                                    throw new InvalidDataException("Native JPEG bytes changed across independent blocks.");
                                expectedNativeHash = nativeHash;
                                nativeBytes = nativeSample.Bytes;
                                blocks.Add(new(block, nativeFirst ? "native-managed" : "managed-native", managedSample.NsPerEncode, nativeSample.NsPerEncode,
                                    managedSample.Gen0Collections, managedSample.Gen1Collections, managedSample.Gen2Collections, managedSample.AllocatedBytes));
                            }
                            byte[] nativeJpeg = await File.ReadAllBytesAsync(nativePath);
                            if (nativeJpeg.LongLength != nativeBytes) throw new InvalidDataException("Retained native JPEG does not match measured output size.");
                            string? structure = JpegStructure.Compare(managedJpeg, nativeJpeg, image, sampling, progression);
                            TimingStatistics managedStats = Evidence.Statistics(blocks.Select(block => block.ManagedNsPerEncode).ToArray());
                            TimingStatistics nativeStats = Evidence.Statistics(blocks.Select(block => block.NativeNsPerEncode).ToArray());
                            cases.Add(new(id, category, sampling, progression, distance, iterations, calibration.NsPerEncode, blocks.ToArray(), managedStats, nativeStats,
                                managedStats.Median / nativeStats.Median, managedJpeg.LongLength, nativeBytes, Hash(managedJpeg), Hash(nativeJpeg), structure,
                                Evidence.ValidateSamples(blocks.ToArray(), iterations, policy)));
                            await WriteJson(output, "cases.json", cases);
                        }
                Console.WriteLine($"Measured {id} ({category}); {cases.Count}/256 cases retained.");
            }
            Evaluation evaluation = Evidence.Evaluate(cases, policy);
            string status = !evaluation.Valid ? "invalid" : evaluation.Passed ? "passed" : "failed";
            var summary = new
            {
                schemaVersion = 1, status, evaluation.Valid, evaluation.Passed,
                scope = "Encode latency noninferiority for RGB8 to YCbCr, Kodak24 and eight fixed controls, distances1/4, 444/420, progression0/2, one managed/native worker on this CI CPU; no universal speed claim",
                upstreamPin = UpstreamPin, nativeVersion, testedRevision = Environment.GetEnvironmentVariable("DOTJPEGLI_PERFORMANCE_REVISION") ?? Environment.GetEnvironmentVariable("GITHUB_SHA"),
                githubRevision = Environment.GetEnvironmentVariable("GITHUB_SHA"), githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
                nativeExecutableSha256 = HashFile(options.Native), managedEncoderSha256 = HashFile(typeof(JpegliEncoder).Assembly.Location),
                managedRunnerSha256 = HashFile(Assembly.GetExecutingAssembly().Location), corpusManifestSha256 = HashFile(Path.Combine(AppContext.BaseDirectory, "corpus.json")),
                naturalImages = sources.Count(source => source.Category == "natural"), controlledImages = sources.Count(source => source.Category == "controlled"), cases = cases.Count,
                distances = Evidence.Distances, samplings = Evidence.Samplings, progressiveLevels = Evidence.Progressions, policy,
                settings = new { sourcePrecision = "RGB8", outputColorSpace = "YCbCr", adaptiveQuantization = true, optimizeHuffman = true,
                    forceBaselineQuantizers = true, managedWorkers = 1, nativeWorkers = 1, smoothing = 0, restartInterval = 0,
                    jfif = "1.01; unit0; densities1; no ICC or other metadata" },
                method = new { latency = "Sum per-encode monotonic elapsed times / iterations, then median of seven independent blocks for each encoder; ratio managed median/native median",
                    scope = "Complete in-memory encoder creation/configuration, RGB8 input, JPEG buffer creation and encode/finalization; no file I/O, subprocess startup, warmup, buffer verification or native free in timed region",
                    calibration = "Native-only unreported trial with three iterations after five warmups; ceil(50ms/calibration latency), clamped3..100; same count for both encoders",
                    order = "Alternate native-managed and managed-native by case index plus block index; native process fresh per block; five untimed encode warmups per block for both",
                    jit = "TieredCompilation=false in runner runtimeconfig; method first JIT compilation fully optimized; no tier transition intentionally included",
                    gc = "Natural managed collection inside timed encode is included; no forced collections or no-GC region; collection/allocation diagnostics retained",
                    percentiles = "Linear interpolation between sorted block samples, p10/p50/p90; MAD=median absolute deviation/median; any case over10%MAD invalidates evidence",
                    minimumInterval = "At least1ms summed encode interval per block; calibration50ms is a target, capped tiny-image loops may be shorter",
                    structures = "Untimed one JPEG per encoder: frame/component/sampling/precision, all DQT values, scan script and JFIF compared" },
                environment = EnvironmentDetails(), groups = evaluation.Groups, invalidReasons = evaluation.InvalidReasons, failures = evaluation.Failures
            };
            await WriteJson(output, "summary.json", summary);
            await File.WriteAllTextAsync(Path.Combine(output, "report.md"), Report(status, cases.Count, evaluation));
            Console.WriteLine($"Performance parity {status.ToUpperInvariant()}; evidence: {output}");
            return !evaluation.Valid ? 2 : evaluation.Passed ? 0 : 1;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or
            System.ComponentModel.Win32Exception or TimeoutException or JsonException or OverflowException or NotSupportedException or KeyNotFoundException or FormatException)
        {
            Console.Error.WriteLine($"Performance evidence invalid: {error.Message}");
            if (output is not null)
            {
                await WriteJson(output, "summary.json", new { schemaVersion = 1, status = "invalid", valid = false, passed = false, error = error.Message });
                await File.WriteAllTextAsync(Path.Combine(output, "report.md"), $"# Performance parity: INVALID\n\n{error.Message}\n\nPartial raw evidence is retained; no parity conclusion is valid.\n");
            }
            return 2;
        }
    }

    private static void RequireCi()
    {
#if DEBUG
        throw new InvalidOperationException("Actual measurements require a Release build.");
#else
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != "true" ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_SHA")))
            throw new InvalidOperationException("Benchmarks may execute only in GitHub Actions. Use --self-test locally for synthetic evidence checks.");
        if (AppContext.TryGetSwitch("System.Runtime.TieredCompilation", out bool tiering) && tiering)
            throw new InvalidOperationException("Tiered compilation must be disabled for this fixed protocol.");
#endif
    }

    private static List<(string Id, string Category, string SourcePath, PpmImage Image)> ReadCorpus(string corpus)
    {
        string[] files = Directory.GetFiles(corpus, "*.ppm", SearchOption.AllDirectories);
        if (files.Length != 24) throw new InvalidDataException("The natural corpus requires exactly24 Kodak P6 PPM inputs.");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "corpus.json")));
        var images = new List<(string Id, string Category, string SourcePath, PpmImage Image)>();
        foreach (JsonElement item in manifest.RootElement.GetProperty("images").EnumerateArray())
        {
            string id = Path.GetFileNameWithoutExtension(item.GetProperty("name").GetString() ?? throw new InvalidDataException("Missing manifest image name."));
            string[] candidates = files.Where(path => Path.GetFileNameWithoutExtension(path) == id).ToArray();
            if (candidates.Length != 1) throw new InvalidDataException($"Missing or duplicate Kodak image {id}.");
            string path = candidates[0];
            if (HashFile(path) != item.GetProperty("ppmSha256").GetString()) throw new InvalidDataException($"Kodak PPM hash mismatch: {id}.");
            PpmImage image = PpmImage.Read(path);
            if (image.Width != item.GetProperty("width").GetInt32() || image.Height != item.GetProperty("height").GetInt32())
                throw new InvalidDataException($"Kodak dimensions mismatch: {id}.");
            images.Add((id, "natural", Path.GetRelativePath(corpus, path).Replace('\\', '/'), image));
        }
        if (images.Count != 24 || !images.Select(item => item.Id).Order().SequenceEqual(Evidence.ImageIds("natural").Order()))
            throw new InvalidDataException("The pinned manifest does not cover Kodak24 exactly.");
        foreach (var (name, image) in ControlledCorpus.Create()) images.Add((name, "controlled", "generated:" + name, image));
        if (!images.Where(item => item.Category == "controlled").Select(item => item.Id).Order().SequenceEqual(Evidence.ControlledIds.Order()))
            throw new InvalidDataException("Controlled generator does not cover all eight fixed inputs exactly.");
        return images;
    }

    private static byte[] Encode(PpmImage image, JpegliEncoderOptions settings) => JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, settings);

    private static ManagedSample BenchmarkManaged(PpmImage image, JpegliEncoderOptions settings, byte[] expected, int iterations, int warmups)
    {
        for (int warmup = 0; warmup < warmups; warmup++)
            if (!Encode(image, settings).AsSpan().SequenceEqual(expected)) throw new InvalidDataException("Managed warmup output is not deterministic.");
        long ticks = 0, allocatedStart = GC.GetTotalAllocatedBytes(precise: true);
        int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            long start = Stopwatch.GetTimestamp();
            byte[] bytes = Encode(image, settings);
            ticks += Stopwatch.GetTimestamp() - start;
            // Verify/use the result after stopping the timer, as the native runner does.
            if (!bytes.AsSpan().SequenceEqual(expected)) throw new InvalidDataException("Managed timed output is not deterministic.");
            GC.KeepAlive(bytes);
        }
        return new(ticks * (1_000_000_000.0 / Stopwatch.Frequency) / iterations, GC.CollectionCount(0) - gen0,
            GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2, GC.GetTotalAllocatedBytes(precise: true) - allocatedStart);
    }

    private static async Task<NativeSample> BenchmarkNative(Options options, string worker, PpmImage image, string input, float distance,
        int sampling, int progression, int iterations, Policy policy, string? jpegOutput = null)
    {
        var args = new List<string> { "benchmark", input, distance.ToString("R", CultureInfo.InvariantCulture), sampling.ToString(CultureInfo.InvariantCulture),
            progression.ToString(CultureInfo.InvariantCulture), policy.Warmups.ToString(CultureInfo.InvariantCulture), iterations.ToString(CultureInfo.InvariantCulture), "1" };
        if (jpegOutput is not null) args.Add(jpegOutput);
        return ParseNativeSample(await Native(options.Native, worker, options.Output, args.ToArray()), image.Width, image.Height, distance, sampling, progression, iterations, policy);
    }

    internal static NativeSample ParseNativeSample(string text, int width, int height, float distance, int sampling, int progression, int iterations, Policy policy)
    {
        using JsonDocument json = JsonDocument.Parse(text);
        JsonElement result = json.RootElement;
        if (result.GetProperty("upstreamPin").GetString() != UpstreamPin || result.GetProperty("width").GetInt32() != width ||
            result.GetProperty("height").GetInt32() != height || result.GetProperty("distance").GetSingle() != distance ||
            result.GetProperty("sampling").GetString() != sampling.ToString(CultureInfo.InvariantCulture) || result.GetProperty("progressiveLevel").GetInt32() != progression ||
            result.GetProperty("warmups").GetInt32() != policy.Warmups || result.GetProperty("iterations").GetInt32() != iterations ||
            result.GetProperty("repeats").GetInt32() != 1 || !result.GetProperty("deterministic").GetBoolean() ||
            !result.GetProperty("adaptiveQuantization").GetBoolean() || !result.GetProperty("optimizeHuffman").GetBoolean() || result.GetProperty("threads").GetInt32() != 1)
            throw new InvalidDataException("Native measurement contradicts the fixed settings or identity.");
        JsonElement[] samples = result.GetProperty("samplesNsPerEncode").EnumerateArray().ToArray();
        long bytes = result.GetProperty("bytes").GetInt64();
        if (samples.Length != 1 || !double.IsFinite(samples[0].GetDouble()) || samples[0].GetDouble() <= 0 || bytes <= 0)
            throw new InvalidDataException("Native benchmark returned invalid or missing timing samples/output bytes.");
        return new(samples[0].GetDouble(), bytes);
    }

    private static async Task<string> Native(string executable, string workingDirectory, string output, params string[] args)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true };
        foreach (string argument in args) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Native process could not start.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(), stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException("Native benchmark command exceeded the ten-minute limit.");
        }
        string standardOutput = await stdout, standardError = await stderr;
        await File.AppendAllTextAsync(Path.Combine(output, "native.log"), $"{JsonSerializer.Serialize(args)} exit={process.ExitCode}\n{standardOutput}{standardError}\n");
        if (process.ExitCode != 0) throw new InvalidDataException($"Native {args[0]} failed with exit{process.ExitCode}: {standardError}");
        return standardOutput;
    }

    private static object EnvironmentDetails()
    {
        string cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "not supplied";
        string? affinity = null;
        if (OperatingSystem.IsLinux())
        {
            cpu = File.ReadLines("/proc/cpuinfo").FirstOrDefault(line => line.StartsWith("model name", StringComparison.Ordinal))?.Split(':', 2)[1].Trim() ?? cpu;
            affinity = File.ReadLines("/proc/self/status").FirstOrDefault(line => line.StartsWith("Cpus_allowed_list:", StringComparison.Ordinal))?.Split(':', 2)[1].Trim();
        }
        return new { cpu, affinity, os = RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture, RuntimeInformation.ProcessArchitecture,
            runtime = RuntimeInformation.FrameworkDescription, runtimeVersion = Environment.Version.ToString(), logicalProcessors = Environment.ProcessorCount,
            Stopwatch.Frequency, Stopwatch.IsHighResolution, Vector.IsHardwareAccelerated, vectorFloatWidth = Vector<float>.Count,
            vector128 = Vector128.IsHardwareAccelerated, vector256 = Vector256.IsHardwareAccelerated, vector512 = Vector512.IsHardwareAccelerated,
            serverGc = System.Runtime.GCSettings.IsServerGC, gcLatencyMode = System.Runtime.GCSettings.LatencyMode.ToString(),
            dotnetTieredCompilation = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation"),
            complusTieredCompilation = Environment.GetEnvironmentVariable("COMPlus_TieredCompilation"),
            githubRunnerOs = Environment.GetEnvironmentVariable("RUNNER_OS"), githubRunnerArchitecture = Environment.GetEnvironmentVariable("RUNNER_ARCH") };
    }

    private static string Report(string status, int count, Evaluation evaluation)
    {
        var text = new StringBuilder();
        text.AppendLine($"# Performance parity: {status.ToUpperInvariant()}");
        text.AppendLine();
        text.AppendLine($"{count}/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `{UpstreamPin}`.");
        text.AppendLine($"Tested revision: `{Environment.GetEnvironmentVariable("DOTJPEGLI_PERFORMANCE_REVISION") ?? Environment.GetEnvironmentVariable("GITHUB_SHA")}`.");
        text.AppendLine();
        text.AppendLine("Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.");
        text.AppendLine();
        text.AppendLine("Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.");
        text.AppendLine();
        text.AppendLine("Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.");
        text.AppendLine();
        text.AppendLine("| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |");
        text.AppendLine("| --- | --- | --- | ---: | ---: | ---: | --- |");
        foreach (GroupResult group in evaluation.Groups)
            text.AppendLine(FormattableString.Invariant($"| {group.Category} | {group.Sampling} | {group.ProgressiveLevel} | {group.Cases} | {group.GeometricMeanRatio:F6} | {group.WorstRatio:F6} | {(!group.Valid ? "INVALID" : group.Passed ? "PASS" : "FAIL")} |"));
        text.AppendLine();
        text.AppendLine("Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.");
        foreach (var (label, reasons) in new[] { ("Invalid evidence", evaluation.InvalidReasons), ("Failed bounds", evaluation.Failures) })
            if (reasons.Length > 0)
            {
                text.AppendLine(); text.AppendLine(label + ":"); text.AppendLine();
                foreach (string reason in reasons) text.AppendLine("- " + reason);
            }
        return text.ToString();
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string HashFile(string path) => Hash(File.ReadAllBytes(path));
    private static Task WriteJson<T>(string output, string name, T value) => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
    internal sealed record NativeSample(double NsPerEncode, long Bytes);
    private sealed record ManagedSample(double NsPerEncode, int Gen0Collections, int Gen1Collections, int Gen2Collections, long AllocatedBytes);
}

internal sealed record Options(string Native, string Corpus, string Output)
{
    public static Options Parse(string[] args)
    {
        if (args.Length != 6) throw new ArgumentException("Usage: DotJpegli.Performance --native PATH --corpus DIRECTORY --output EMPTY_DIRECTORY; or --self-test.");
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < args.Length; index += 2)
        {
            if (args[index] is not ("--native" or "--corpus" or "--output") || !Path.IsPathFullyQualified(args[index + 1]) ||
                !values.TryAdd(args[index], Path.GetFullPath(args[index + 1])))
                throw new ArgumentException("Unknown/duplicate option or non-absolute path.");
        }
        if (values.Count != 3 || !File.Exists(values["--native"]) || !Directory.Exists(values["--corpus"]))
            throw new ArgumentException("Native executable and corpus directory must exist.");
        return new(values["--native"], values["--corpus"], values["--output"]);
    }
}
