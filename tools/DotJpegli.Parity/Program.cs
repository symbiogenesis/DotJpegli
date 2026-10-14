using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotJpegli;

namespace DotJpegli.Parity;

internal static class Program
{
    private const string UpstreamRevision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly float[] Distances = [.25f, .5f, .75f, 1, 1.5f, 2, 3, 4, 6, 8];
    private static readonly int[] Samplings = [444, 420];
    private static readonly int[] Progressions = [0, 2];
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.SequenceEqual(new[] { "--self-test" })) { SelfTest.Run(); Console.WriteLine("Parity harness self-test passed."); return 0; }
            var options = Options.Parse(args);
            string native = options.Native, corpus = options.Corpus, output = options.Output;
            string[] naturalFiles = ValidateCorpus(corpus);
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
                throw new InvalidDataException("Evidence output directory must be empty to prevent stale evidence.");
            Directory.CreateDirectory(output);
            string inputDirectory = Path.Combine(output, "inputs"), jpegDirectory = Path.Combine(output, "jpegs");
            Directory.CreateDirectory(inputDirectory); Directory.CreateDirectory(jpegDirectory);
            string worker = Path.Combine(Path.GetTempPath(), "DotJpegli-parity-native-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(worker);
            string nativeVersion = (await Native(native, worker, output, "version")).Trim();
            if (!nativeVersion.Equals(UpstreamRevision, StringComparison.Ordinal))
                throw new InvalidDataException("Native encoder does not report the pinned upstream revision.");
            var sources = new List<SourceImage>();
            var cases = new List<RawCase>();
            var images = new List<(string Id, string Category, string SourcePath, PpmImage Image)>();
            foreach (string path in naturalFiles)
                images.Add((Path.GetFileNameWithoutExtension(path), "natural", Path.GetRelativePath(corpus, path).Replace('\\', '/'), PpmImage.Read(path)));
            foreach (var (name, image) in ControlledCorpus.Create()) images.Add((name, "controlled", "generated:" + name, image));
            if (images.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != images.Count)
                throw new InvalidDataException("Corpus image IDs must be unique.");
            foreach (var (id, category, sourcePath, image) in images)
            {
                string input = Path.Combine(inputDirectory, id + ".ppm");
                image.Write(input);
                sources.Add(new(id, category, sourcePath, HashFile(input), Hash(image.Pixels), image.Width, image.Height));
                foreach (int sampling in Samplings)
                {
                    foreach (int progression in Progressions)
                    {
                        foreach (float distance in Distances)
                        {
                            string stem = $"{id}_{sampling}_p{progression}_d{distance.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', '_')}";
                            string managedPath = Path.Combine(jpegDirectory, stem + "_managed.jpg"), nativePath = Path.Combine(jpegDirectory, stem + "_native.jpg");
                            byte[] jpeg = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, new()
                            {
                                ColorSpace = JpegColorSpace.YCbCr, Distance = distance, ProgressiveLevel = progression,
                                Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                                OptimizeHuffmanTables = true, AdaptiveQuantization = true, ForceBaseline = true,
                                MaxDegreeOfParallelism = 1
                            });
                            await File.WriteAllBytesAsync(managedPath, jpeg);
                            await Native(native, worker, output, "encode", input, nativePath, distance.ToString("R", CultureInfo.InvariantCulture),
                                sampling.ToString(CultureInfo.InvariantCulture), progression.ToString(CultureInfo.InvariantCulture), "1", "1");
                            string? structureDifference = JpegStructure.Compare(jpeg, await File.ReadAllBytesAsync(nativePath), image, sampling, progression);
                            async Task<EncodedSample> Decode(string path)
                            {
                                string decodedPath = Path.Combine(worker, "decoded.ppm");
                                await Native(native, worker, output, "decode", path, decodedPath);
                                PpmImage decoded = PpmImage.Read(decodedPath);
                                File.Delete(decodedPath);
                                return new(new FileInfo(path).Length, HashFile(path), Hash(decoded.Pixels),
                                    Path.GetRelativePath(output, path).Replace('\\', '/'), QualityMetrics.Compare(image, decoded));
                            }
                            EncodedSample managedSample = await Decode(managedPath), nativeSample = await Decode(nativePath);
                            cases.Add(new(id, category, sampling, progression, distance, managedSample, nativeSample, structureDifference));
                        }
                    }
                }
                Console.WriteLine($"Compared {id} ({category}); {cases.Count} cases complete.");
            }
            var failures = new List<string>();
            var exclusions = new List<string>();
            var policy = new Policy();
            List<MatchedRate> matches = Evidence.Match(cases, failures);
            List<GroupResult> groups = Evidence.Evaluate(cases, matches, policy, failures);
            List<MatchedQuality> qualityMatches = Evidence.MatchQuality(cases, exclusions);
            List<QualityGroupResult> qualityGroups = Evidence.EvaluateQuality(cases, qualityMatches, policy, failures);
            var summary = new Summary(1, "RGB8 to YCbCr, Kodak24 and controlled corpus; noninferiority within declared tolerances; no universal or visual-quality proof",
                failures.Count == 0 && groups.All(group => group.Passed) && qualityGroups.All(group => group.Passed), UpstreamRevision,
                nativeVersion, HashFile(native), HashFile(typeof(JpegliEncoder).Assembly.Location), HashFile(Assembly.GetExecutingAssembly().Location),
                Environment.GetEnvironmentVariable("DOTJPEGLI_PARITY_REVISION") ?? Environment.GetEnvironmentVariable("GITHUB_SHA"),
                Environment.GetEnvironmentVariable("GITHUB_SHA"), Environment.GetEnvironmentVariable("GITHUB_RUN_ID"), Distances, Samplings, Progressions,
                new { adaptiveQuantization = true, optimizeHuffman = true, forceBaselineQuantizers = true, sourcePrecision = "RGB8", outputColorSpace = "YCbCr",
                    managedWorkers = 1, smoothing = 0, restartInterval = 0, jfif = "1.01; density unit0, X1, Y1; no ICC or other metadata",
                    commonDecoder = "pinned jpegli; RGB8, fancy upsampling, JDCT_ISLOW",
                    jpegStructureGate = "Dimensions, precision, SOF, YCbCr ids/factors/quantizers, complete DQT values, SOS component/spectral/refinement scripts and JFIF matched; Huffman selectors/bytes may differ" },
                new { rgbPsnr = "10*log10(255^2 / mean squared RGB sample error); exact identity capped120dB",
                    lumaSsim = "BT.601 luma .299R+.587G+.114B; disjoint8x8 windows, clipped edges; population moments; pixel-weighted mean; C1=(.01*255)^2 C2=(.03*255)^2",
                    matchedRate = "Metric-specific nondominated envelope; max quality at duplicate rates; linear metric interpolation in log(bytes); five interior anchors at .1,.3,.5,.7,.9 of common log-rate range for means; overlap endpoints and union of all four frontier knots for full interpolated-range worst bounds; no extrapolation",
                    matchedQuality = "Inverse metric-specific envelope; five anchors at .1,.3,.5,.7,.9 of common quality range for geometric means; overlap endpoints and union of both frontier quality knots for full interpolated-range worst bounds; linear metric to log(bytes); no extrapolation" },
                policy, sources.Count(source => source.Category == "natural"), sources.Count(source => source.Category == "controlled"), cases.Count, matches.Count,
                matches.Count(item => item.IsAnchor), qualityMatches.Count, qualityMatches.Count(item => item.IsAnchor), groups, qualityGroups, exclusions, failures);
            await WriteJson(output, "sources.json", sources);
            await WriteJson(output, "cases.json", cases);
            await WriteJson(output, "matched-rates.json", matches);
            await WriteJson(output, "matched-quality.json", qualityMatches);
            await WriteJson(output, "summary.json", summary);
            await File.WriteAllTextAsync(Path.Combine(output, "report.md"), Report(summary));
            Console.WriteLine($"Parity gate {(summary.Passed ? "PASSED" : "FAILED")}: {cases.Count} cases, {matches.Count} matched-rate targets, {qualityMatches.Count} matched-quality targets; {failures.Count} failures. Evidence: {output}");
            return summary.Passed ? 0 : 1;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
        {
            Console.Error.WriteLine($"Parity harness error: {error.Message}");
            return 2;
        }
    }

    internal static string[] ValidateCorpus(string directory)
    {
        string[] files = Directory.GetFiles(directory, "*.ppm", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        if (files.Length != 24) throw new InvalidDataException("Natural corpus requires all 24 Kodak photographs as RGB8 P6 PPM files.");
        for (int index = 1; index <= 24; index++)
            if (!files.Any(path => Path.GetFileNameWithoutExtension(path).Equals($"kodim{index:00}", StringComparison.Ordinal)))
                throw new InvalidDataException($"Natural corpus is missing kodim{index:00}.ppm.");
        return files;
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string HashFile(string path) => Hash(File.ReadAllBytes(path));
    private static Task WriteJson<T>(string output, string name, T value) => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");

    private static async Task<string> Native(string executable, string workingDirectory, string output, params string[] args)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Native process could not start.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(), stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException("Native encoder/decoder exceeded the three-minute per-command limit.");
        }
        string standardOutput = await stdout, standardError = await stderr;
        await File.AppendAllTextAsync(Path.Combine(output, "native.log"), $"{JsonSerializer.Serialize(args)} exit={process.ExitCode}\n{standardOutput}{standardError}\n");
        if (process.ExitCode != 0) throw new InvalidDataException($"Native {args[0]} failed with exit {process.ExitCode}: {standardError}");
        return standardOutput;
    }

    private static string Report(Summary summary)
    {
        var text = new StringBuilder();
        text.AppendLine($"# Quality and compression parity: {(summary.Passed ? "PASS" : "FAIL")}");
        text.AppendLine(); text.AppendLine(summary.Scope); text.AppendLine();
        text.AppendLine($"Pinned upstream: `{summary.UpstreamRevision}`. Cases: {summary.Cases}; matched-rate targets: {summary.MatchedTargets} ({summary.MatchedRateAnchors} anchors); matched-quality targets: {summary.MatchedQualityTargets} ({summary.MatchedQualityAnchors} anchors).");
        text.AppendLine($"Tested revision: `{summary.TestedRevision ?? "not supplied"}`.");
        text.AppendLine(); text.AppendLine("Every group and every image/target must satisfy the fixed bounds. Aggregate results never excuse an individual failure. The gate measures PSNR and this explicitly defined local luma SSIM, without a human study or Butteraugli/MS-SSIM claim.");
        text.AppendLine(); text.AppendLine("Size limits: <=1.02 geometric mean and <=1.05 each case/target. Matched-rate PSNR delta: >=-0.10dB mean, >=-0.25dB each target. SSIM delta: >=-0.001 mean, >=-0.003 each target. Lower managed bytes and higher managed quality are accepted.");
        text.AppendLine(); text.AppendLine("Means use five interior anchors per image. Worst bounds use those anchors, overlap endpoints and the union of both encoders' frontier knots, covering the full common piecewise-linear interpolated range. No extrapolation or unmeasured real encoder behavior between samples is claimed.");
        text.AppendLine(); text.AppendLine("| Corpus | Sampling | Progression | Mean size ratio | Worst ratio | Mean PSNR delta dB | Worst PSNR delta dB | Mean SSIM delta | Worst SSIM delta | Verdict |");
        text.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |");
        foreach (GroupResult group in summary.Groups)
            text.AppendLine(FormattableString.Invariant($"| {group.Category} | {group.Sampling} | {group.Progression} | {group.GeometricMeanSizeRatio:F6} | {group.WorstSizeRatio:F6} | {group.MeanPsnrDeltaDb:F6} | {group.WorstPsnrDeltaDb:F6} | {group.MeanSsimDelta:F6} | {group.WorstSsimDelta:F6} | {(group.Passed ? "PASS" : "FAIL")} |"));
        text.AppendLine(); text.AppendLine("| Corpus | Sampling | Progression | Matched quality metric | Targets | Mean rate ratio | Worst ratio | Verdict |");
        text.AppendLine("| --- | --- | --- | --- | ---: | ---: | ---: | --- |");
        foreach (QualityGroupResult group in summary.MatchedQualityGroups)
            text.AppendLine(FormattableString.Invariant($"| {group.Category} | {group.Sampling} | {group.Progression} | {group.Metric} | {group.MatchedTargets} ({group.AnchorTargets} anchors) | {group.GeometricMeanSizeRatio:F6} | {group.WorstSizeRatio:F6} | {(group.Passed ? "PASS" : "FAIL")} |"));
        text.AppendLine(); text.AppendLine("Raw samples, source hashes, decoded hashes, settings, bounds and failures are in the JSON files; JPEGs and exact RGB inputs are retained. Decoder PPM scratch is discarded after its hash and metrics are recorded.");
        if (summary.MatchedQualityExclusions.Count > 0)
        {
            text.AppendLine(); text.AppendLine("Matched-quality exclusions (no extrapolation):"); text.AppendLine();
            foreach (string exclusion in summary.MatchedQualityExclusions) text.AppendLine("- " + exclusion);
        }
        if (summary.Failures.Count > 0)
        {
            text.AppendLine(); text.AppendLine("Gate failures:"); text.AppendLine();
            foreach (string failure in summary.Failures) text.AppendLine("- " + failure);
        }
        return text.ToString();
    }
}

internal sealed record Options(string Native, string Corpus, string Output)
{
    public static Options Parse(string[] args)
    {
        if (args.Length != 6) throw new ArgumentException("Usage: DotJpegli.Parity --native PATH --corpus DIRECTORY --output EMPTY_DIRECTORY; or --self-test.");
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < args.Length; index += 2)
            if (args[index] is not ("--native" or "--corpus" or "--output") || !values.TryAdd(args[index], Path.GetFullPath(args[index + 1])))
                throw new ArgumentException("Unknown or duplicate parity option.");
        if (values.Count != 3 || !File.Exists(values["--native"]) || !Directory.Exists(values["--corpus"]))
            throw new ArgumentException("Native executable and corpus directory must exist.");
        return new(values["--native"], values["--corpus"], values["--output"]);
    }
}
