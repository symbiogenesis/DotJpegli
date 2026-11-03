namespace DotJpegli.Performance;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>Bounded CI workload observations, separate from native acceptance.</summary>
internal static class WorkloadDiagnostics
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const int Samples = 7;
    private const int Warmups = 3;
    internal sealed record Scenario(string Id, int Width, int Height, int Sampling = 444, int Progression = 0,
        string Color = "ycbcr", bool Stream = true, int Workers = 1, int Concurrent = 1, bool Tiered = false, bool Noise = false,
        long CacheBytes = 192L * 1024 * 1024, bool ScalePhotograph = false);
    internal sealed record Request(Scenario Scenario, string Photograph, string PhotographSha256, string Output);
    internal sealed record Memory(long WorkingSetBytes, long ProcessPeakWorkingSetBytes, long ManagedBytesWithoutCollection,
        long LastGcHeapBytes, long LastGcFragmentedBytes, long LastGcCommittedBytes);
    internal sealed record Sample(int Index, double BatchMilliseconds, double[] EncodeMilliseconds,
        long AllocatedBytes, int Gen0Collections, int Gen1Collections, int Gen2Collections, Memory Memory, CacheObservation[] Caches);
    internal sealed record CacheObservation(long PeakBytes, long ConvertedSamplePeakBytes, long CoefficientPeakBytes,
        long EntropyTokenPeakBytes, long CurrentBytesAfterEncode);
    private sealed record Encoded(byte[]? Bytes, long Length, double Milliseconds, CacheObservation Cache);

    internal static Scenario[] Scenarios() =>
    [
        new("photo-array", 768, 512, Stream: false),
        new("photo-stream", 768, 512),
        new("photo-default-tiering", 768, 512, Tiered: true),
        new("tiny", 19, 17),
        new("photograph-9m-444", 3840, 2560, Progression: 2, ScalePhotograph: true),
        new("photograph-9m-420", 3840, 2560, Sampling: 420, ScalePhotograph: true),
        new("planes-444-below", 2048, 2728, Progression: 2),
        new("planes-444-above", 2048, 2736, Progression: 2),
        new("coefficients-444-below", 2048, 4096, Progression: 2),
        new("coefficients-444-above", 2048, 4104, Progression: 2),
        new("coefficients-64m-below", 2048, 5456, Progression: 2),
        new("coefficients-64m-above", 2048, 5464, Progression: 2),
        new("planes-420-below", 4096, 2048, Sampling: 420, Progression: 2),
        new("planes-420-above", 4096, 2064, Sampling: 420, Progression: 2),
        new("noise", 2048, 2056, Noise: true),
        new("rgb", 1024, 1024, Color: "rgb"),
        new("xyb", 1024, 1024, Color: "xyb"),
        new("icc", 1024, 1024, Color: "icc"),
        new("scaling-workers-1", 2048, 1024),
        new("cache-disabled", 2048, 1024, CacheBytes: 0),
        new("cache-16m", 2048, 1024, CacheBytes: 16L * 1024 * 1024),
        new("scaling-workers-2", 2048, 1024, Workers: 2),
        new("scaling-workers-4", 2048, 1024, Workers: 4),
        new("scaling-concurrent-2", 2048, 1024, Concurrent: 2),
        new("scaling-concurrent-4", 2048, 1024, Concurrent: 4)
    ];

    internal static async Task<int> Run(string[] args)
    {
        Program.RequireCi(requireUntiered: false);
        if (args.Length == 3 && args[0] == "--workload-worker" && args[1] == "--request")
        {
            Request request = JsonSerializer.Deserialize<Request>(await File.ReadAllTextAsync(args[2]), Json)
                ?? throw new InvalidDataException("Missing workload request.");
            await Worker(request);
            return 0;
        }
        if (args.Length != 5 || args[0] != "--workload-diagnostics" || args[1] != "--corpus" || args[3] != "--output")
        {
            throw new ArgumentException("Use --workload-diagnostics --corpus <absolute-directory> --output <empty-absolute-directory>.");
        }
        string output = args[4];
        if (!Path.IsPathFullyQualified(output) || (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any()))
        {
            throw new ArgumentException("Workload evidence requires an empty absolute output directory.");
        }
        Directory.CreateDirectory(output);
        (_, _, string sourcePath, PpmImage sourceImage) = Program.ReadCorpus(args[2]).Single(item => item.Id == "kodim01");
        string photographPath = Path.Combine(args[2], sourcePath);
        // ReadCorpus has already verified the pinned manifest and pixel identities.
        string photoHash = Hash(sourceImage.Pixels);
        Scenario[] scenarios = Scenarios();
        await Write(output, "run.json", new
        {
            schemaVersion = 1,
            mode = "workload-diagnostics",
            startedAtUtc = DateTimeOffset.UtcNow,
            revision = Environment.GetEnvironmentVariable("GITHUB_SHA"),
            run = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
            environment = Program.EnvironmentDetails(),
            encoderSha256 = HashFile(typeof(JpegliEncoder).Assembly.Location),
            runnerSha256 = HashFile(typeof(WorkloadDiagnostics).Assembly.Location),
            photographSha256 = photoHash,
            photographWidth = sourceImage.Width,
            photographHeight = sourceImage.Height,
            scenarios,
            samples = Samples,
            warmups = Warmups,
            maximumElapsedMinutes = 10,
            scope = "Observations only: fresh process per scenario; cold first encode then warmed samples. No native, regression or universal speed verdict."
        });
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        var completed = new List<object>();
        var errors = new List<string>();
        foreach (Scenario scenario in scenarios)
        {
            if (deadline.IsCancellationRequested)
            {
                errors.Add("Workload deadline reached before all scenarios completed.");
                break;
            }
            string directory = Path.Combine(output, scenario.Id);
            Directory.CreateDirectory(directory);
            var request = new Request(scenario, photographPath, photoHash, directory);
            string requestPath = Path.Combine(directory, "request.json");
            await File.WriteAllTextAsync(requestPath, JsonSerializer.Serialize(request, Json));
            try
            {
                object observation = await Launch(requestPath, directory, scenario.Tiered, deadline.Token);
                completed.Add(new { scenario.Id, observation });
                Console.WriteLine($"Workload {scenario.Id}: {completed.Count}/{scenarios.Length} completed.");
            }
            catch (Exception error) when (error is InvalidOperationException or IOException or OperationCanceledException or System.ComponentModel.Win32Exception)
            {
                errors.Add($"{scenario.Id}: {error.Message}");
                if (deadline.IsCancellationRequested)
                {
                    break;
                }
            }
            await Write(output, "progress.json", new { completed, errors });
        }
        await ValidateEquivalentScenarios(output, errors);
        await Write(output, "summary.json", new
        {
            schemaVersion = 1,
            mode = "workload-diagnostics",
            expected = scenarios.Length,
            completed = completed.Count,
            valid = errors.Count == 0 && completed.Count == scenarios.Length,
            parityVerdict = "not-evaluated",
            observations = completed,
            errors
        });
        await Report(output, scenarios, errors);
        return errors.Count == 0 && completed.Count == scenarios.Length ? 0 : 2;
    }

    private static async Task<object> Launch(string request, string directory, bool tiered, CancellationToken cancellationToken)
    {
        string assembly = typeof(WorkloadDiagnostics).Assembly.Location;
        string configSource = Path.ChangeExtension(assembly, ".runtimeconfig.json");
        JsonNode config = JsonNode.Parse(await File.ReadAllTextAsync(configSource, cancellationToken))!;
        config["runtimeOptions"]!["configProperties"]!["System.Runtime.TieredCompilation"] = tiered;
        string runtimeConfig = Path.Combine(directory, "worker.runtimeconfig.json");
        await File.WriteAllTextAsync(runtimeConfig, config.ToJsonString(Json), cancellationToken);
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException("Missing dotnet host."))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string argument in new[] { "exec", "--runtimeconfig", runtimeConfig, assembly, "--workload-worker", "--request", request })
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment.Remove("DOTNET_TieredCompilation");
        start.Environment.Remove("COMPlus_TieredCompilation");
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Unable to start workload worker.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        Task<string> stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
        long sampledPeak = 0;
        var rss = new List<long>();
        try
        {
            while (!process.HasExited)
            {
                try
                {
                    process.Refresh();
                    long current = process.WorkingSet64;
                    sampledPeak = Math.Max(sampledPeak, current);
                    rss.Add(current);
                }
                catch (Exception error) when ((error is InvalidOperationException or System.ComponentModel.Win32Exception) && process.HasExited)
                {
                    break;
                }
                await Task.Delay(25, cancellationToken);
            }
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0 || !File.Exists(Path.Combine(directory, "result.json")))
            {
                throw new InvalidOperationException($"Worker exit {process.ExitCode}; inspect retained worker logs.");
            }

            return new { sampledPeakWorkingSetBytes = sampledPeak, samples = rss.Count };
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            await process.WaitForExitAsync(CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(directory, "stdout.txt"), await stdout, CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(directory, "stderr.txt"), await stderr, CancellationToken.None);
            await Write(directory, "rss-samples.json", new { intervalMilliseconds = 25, samplesBytes = rss, sampledPeakWorkingSetBytes = sampledPeak });
        }
    }

    private static async Task Worker(Request request)
    {
        Scenario scenario = request.Scenario;
        if (!Scenarios().Contains(scenario))
        {
            throw new InvalidDataException("Unknown workload scenario.");
        }
        if (!AppContext.TryGetSwitch("System.Runtime.TieredCompilation", out bool actualTiered) || actualTiered != scenario.Tiered)
        {
            throw new InvalidDataException("Worker tiering differs from the requested runtime configuration.");
        }

        var photograph = PpmImage.Read(request.Photograph);
        if (Hash(photograph.Pixels) != request.PhotographSha256)
        {
            throw new InvalidDataException("Photograph hash mismatch.");
        }

        byte[] pixels = Pixels(photograph, scenario);
        byte[][] concurrentInputs = Enumerable.Range(0, scenario.Concurrent).Select(index => index == 0 ? pixels : pixels.ToArray()).ToArray();
        string inputHash = Hash(pixels);
        JpegliEncoderOptions settings = Settings(scenario);
        Memory before = Snapshot();
        long coldAllocatedStart = GC.GetTotalAllocatedBytes(precise: true);
        Encoded first = Encode(pixels, scenario, settings);
        ValidateCacheBoundary(scenario, first.Cache);
        long coldAllocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - coldAllocatedStart;
        Memory afterCold = Snapshot();
        string? expectedHash = first.Bytes is null ? null : Hash(first.Bytes);
        long expectedLength = first.Length;
        // First timed call above precedes every validation encode and warmup.
        // Streaming timings discard output; full byte equality is checked separately below.
        for (int index = 0; index < Warmups; index++)
        {
            foreach (Encoded warm in (await EncodeBatch(concurrentInputs, scenario, settings, 1)).SelectMany(batch => batch))
            {
                if (warm.Length != expectedLength || (expectedHash is not null && Hash(warm.Bytes!) != expectedHash))
                {
                    throw new InvalidDataException("Warmup output mismatch.");
                }
            }
        }
        Memory afterWarmup = Snapshot();
        var samples = new List<Sample>();
        int iterations = scenario.Width * scenario.Height < 1024 ? 32 : 1;
        for (int sample = 0; sample < Samples; sample++)
        {
            long allocated = GC.GetTotalAllocatedBytes(precise: true);
            int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
            long start = Stopwatch.GetTimestamp();
            Encoded[][] encoded = await EncodeBatch(concurrentInputs, scenario, settings, iterations);
            double elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            long allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocated;
            int g0 = GC.CollectionCount(0) - gen0, g1 = GC.CollectionCount(1) - gen1, g2 = GC.CollectionCount(2) - gen2;
            double[] timings = encoded.SelectMany(batch => batch.Select(item => item.Milliseconds)).ToArray();
            foreach (Encoded item in encoded.SelectMany(batch => batch))
            {
                if (item.Length != expectedLength || (expectedHash is not null && Hash(item.Bytes!) != expectedHash))
                {
                    throw new InvalidDataException("Measured output mismatch.");
                }
            }

            samples.Add(new(sample, elapsed, timings, allocatedBytes, g0, g1, g2, Snapshot(),
                encoded.SelectMany(batch => batch.Select(item => item.Cache)).ToArray()));
            await Write(request.Output, "samples.json", samples);
        }
        Memory afterMeasurement = Snapshot();
        // These validation allocations are outside cold/warm latency and recorded phase snapshots.
        byte[] array = JpegliEncoder.Encode(pixels, scenario.Width, scenario.Height, options: settings);
        byte[] serial = scenario.Workers == 1 ? array : JpegliEncoder.Encode(pixels, scenario.Width, scenario.Height,
            options: settings with { MaxDegreeOfParallelism = 1 });
        using var stream = new MemoryStream();
        JpegliEncoder.Encode(stream, pixels, scenario.Width, scenario.Height, options: settings);
        if (array.LongLength != expectedLength || !array.AsSpan().SequenceEqual(serial)
            || !array.AsSpan().SequenceEqual(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))))
        {
            throw new InvalidDataException("Array/stream JPEG bytes differ.");
        }
        if (scenario.Concurrent > 1)
        {
            Task<byte[]>[] validation = concurrentInputs.Select(input => Task.Run(() =>
            {
                using var destination = new MemoryStream();
                JpegliEncoder.Encode(destination, input, scenario.Width, scenario.Height, options: settings);
                return destination.ToArray();
            })).ToArray();
            foreach (byte[] concurrentJpeg in await Task.WhenAll(validation))
            {
                if (!array.AsSpan().SequenceEqual(concurrentJpeg))
                {
                    throw new InvalidDataException("Concurrent stream output differs from serial output.");
                }
            }
        }

        string jpegHash = Hash(array);
        if (expectedHash is not null && expectedHash != jpegHash)
        {
            throw new InvalidDataException("Final output hash mismatch.");
        }

        double[] latencies = samples.SelectMany(sample => sample.EncodeMilliseconds).ToArray();
        TimingStatistics latency = Evidence.Statistics(latencies);
        double[] throughput = samples.Select(sample => scenario.Concurrent * iterations * 1000.0 / sample.BatchMilliseconds).ToArray();
        await Write(request.Output, "result.json", new
        {
            schemaVersion = 1,
            scenario,
            actualTieredCompilation = actualTiered,
            environment = Program.EnvironmentDetails(),
            inputSha256 = inputHash,
            jpegSha256 = jpegHash,
            jpegBytes = expectedLength,
            coldFirstEncodeMilliseconds = first.Milliseconds,
            coldAllocatedBytes,
            coldCaches = first.Cache,
            before,
            afterCold,
            afterWarmup,
            afterMeasurement,
            samples,
            warmedLatencyMilliseconds = latency,
            medianEncodesPerSecond = Evidence.Statistics(throughput).Median,
            timingStable = latency.RelativeMad <= 0.1,
            meanWorkloadAllocatedBytesPerEncode = samples.Average(sample => (double)sample.AllocatedBytes / (scenario.Concurrent * iterations)),
            scope = "Cold is first encode after input/settings preparation, not process startup. Memory snapshots do not force GC. RSS includes runtime, caller input and validation; sampled peak is a lower bound. Stream timing discards bytes; byte equality is checked separately. Observational samples do not establish regression, scaling or latency-SLA acceptance."
        });
        GC.KeepAlive(concurrentInputs);
    }

    private static async Task<Encoded[][]> EncodeBatch(byte[][] inputs, Scenario scenario, JpegliEncoderOptions settings, int iterations)
    {
        if (inputs.Length == 1)
        {
            return [EncodeMany(inputs[0], scenario, settings, iterations)];
        }
        Task<Encoded[]>[] tasks = inputs.Select(input => Task.Run(() => EncodeMany(input, scenario, settings, iterations))).ToArray();
        return await Task.WhenAll(tasks);
    }

    private static Encoded[] EncodeMany(byte[] pixels, Scenario scenario, JpegliEncoderOptions settings, int iterations)
    {
        var result = new Encoded[iterations];
        for (int index = 0; index < iterations; index++)
        {
            result[index] = Encode(pixels, scenario, settings);
        }

        return result;
    }

    private static Encoded Encode(byte[] pixels, Scenario scenario, JpegliEncoderOptions settings)
    {
        var cache = new EncoderCacheDiagnostics();
        settings = settings with { CacheDiagnostics = cache };
        long start = Stopwatch.GetTimestamp();
        if (!scenario.Stream)
        {
            byte[] bytes = JpegliEncoder.Encode(pixels, scenario.Width, scenario.Height, options: settings);
            double elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            return new(bytes, bytes.LongLength, elapsed, Observe(cache));
        }
        using var stream = new CountingStream();
        JpegliEncoder.Encode(stream, pixels, scenario.Width, scenario.Height, options: settings);
        double milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        return new(null, stream.Length, milliseconds, Observe(cache));
    }

    private static CacheObservation Observe(EncoderCacheDiagnostics cache)
    {
        if (cache.CurrentBytes != 0)
        {
            throw new InvalidDataException("Image cache ownership remains after encoding.");
        }
        return new(cache.PeakBytes, cache.ConvertedSamplePeakBytes, cache.CoefficientPeakBytes,
            cache.EntropyTokenPeakBytes, cache.CurrentBytes);
    }

    private static void ValidateCacheBoundary(Scenario scenario, CacheObservation cache)
    {
        if (scenario.Id.StartsWith("planes-", StringComparison.Ordinal))
        {
            bool expectedAdmission = scenario.Id.EndsWith("-below", StringComparison.Ordinal);
            if ((cache.ConvertedSamplePeakBytes > 0) != expectedAdmission)
            {
                throw new InvalidDataException("Converted-plane boundary did not exercise the expected admission/fallback.");
            }
        }
        if (scenario.Id.StartsWith("coefficients-", StringComparison.Ordinal) && cache.CoefficientPeakBytes <= 0)
        {
            throw new InvalidDataException("Coefficient boundary did not exercise retained dense coefficient storage.");
        }
    }

    private static JpegliEncoderOptions Settings(Scenario scenario) => new()
    {
        Distance = 1,
        ProgressiveLevel = scenario.Progression,
        OptimizeHuffmanTables = true,
        AdaptiveQuantization = true,
        Subsampling = scenario.Sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
        MaxDegreeOfParallelism = scenario.Workers,
        MaxCacheBytes = scenario.CacheBytes,
        ColorSpace = scenario.Color switch { "rgb" => JpegColorSpace.Rgb, "xyb" => JpegColorSpace.Xyb, _ => JpegColorSpace.YCbCr },
        IccProfile = scenario.Color == "xyb" ? JpegliIccProfiles.CreateXybProfile() : ReadOnlyMemory<byte>.Empty,
        SourceIccProfile = scenario.Color == "icc" ? JpegliIccProfiles.CreateDisplayP3Profile() : ReadOnlyMemory<byte>.Empty
    };

    private static byte[] Pixels(PpmImage photograph, Scenario scenario)
    {
        byte[] pixels = new byte[checked(scenario.Width * scenario.Height * 3)];
        uint state = 0x6d2b79f5;
        for (int y = 0; y < scenario.Height; y++)
        {
            for (int x = 0; x < scenario.Width; x++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (scenario.Noise)
                    { state ^= state << 13; state ^= state >> 17; state ^= state << 5; }
                    int sourceX = scenario.ScalePhotograph ? x * photograph.Width / scenario.Width : x % photograph.Width;
                    int sourceY = scenario.ScalePhotograph ? y * photograph.Height / scenario.Height : y % photograph.Height;
                    pixels[(((y * scenario.Width) + x) * 3) + c] = scenario.Noise ? (byte)state
                        : photograph.Pixels[(((sourceY * photograph.Width) + sourceX) * 3) + c];
                }
            }
        }

        return pixels;
    }

    private static Memory Snapshot()
    {
        using var process = Process.GetCurrentProcess();
        GCMemoryInfo gc = GC.GetGCMemoryInfo();
        return new(process.WorkingSet64, process.PeakWorkingSet64, GC.GetTotalMemory(forceFullCollection: false),
            gc.HeapSizeBytes, gc.FragmentedBytes, gc.TotalCommittedBytes);
    }

    private static async Task Report(string output, Scenario[] scenarios, List<string> errors)
    {
        var report = new StringBuilder("# Workload observations\n\nSeparate from the fixed native parity protocol. Each row uses a fresh process, three warmups and seven warmed batches. Cold latency excludes process/input preparation. No speed or tail-latency acceptance verdict is inferred.\n\n| Scenario | Cold ms | Warm median ms | Encodes/s | Workload alloc KiB/encode | Cache peak MiB | Warm phase RSS MiB | MAD |\n| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\n");
        foreach (Scenario scenario in scenarios)
        {
            string file = Path.Combine(output, scenario.Id, "result.json");
            if (!File.Exists(file))
            {
                continue;
            }

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(file));
            JsonElement row = document.RootElement;
            report.AppendLine(CultureInfo.InvariantCulture, $"| {scenario.Id} | {row.GetProperty("coldFirstEncodeMilliseconds").GetDouble():F3} | {row.GetProperty("warmedLatencyMilliseconds").GetProperty("median").GetDouble():F3} | {row.GetProperty("medianEncodesPerSecond").GetDouble():F2} | {row.GetProperty("meanWorkloadAllocatedBytesPerEncode").GetDouble() / 1024:F1} | {row.GetProperty("coldCaches").GetProperty("peakBytes").GetInt64() / 1048576.0:F1} | {row.GetProperty("afterMeasurement").GetProperty("workingSetBytes").GetInt64() / 1048576.0:F1} | {row.GetProperty("warmedLatencyMilliseconds").GetProperty("relativeMad").GetDouble():P1} |");
        }
        report.AppendLine("\nRSS and managed-heap snapshots include caller input and the runtime; neither is exact retained pool capacity. Parent RSS sampling includes setup and final byte-validation allocations and is only a lower bound on peak RSS. Raw samples, cold/warm phase snapshots, hashes, runtime identities and runtime configurations are retained. Rows over 10% MAD are noisy observations, not discarded or retried. The seven-batch sample does not estimate service p95/p99. Concurrent throughput includes task scheduling, while encode latency is recorded separately. Stream timing uses a counting discard sink, without I/O or retained JPEG storage.");
        report.AppendLine("Workload allocation traffic includes encoder, result wrappers, diagnostics, options copies and task scheduling; it is not an encoder-only allocation measurement. Cache peak is accepted storage owned by one encode, not the process or concurrent sum. Instrumentation is present in all timed calls; this report does not replace uninstrumented native acceptance.");
        foreach (string error in errors)
        {
            report.AppendLine(CultureInfo.InvariantCulture, $"\nIncomplete: {error}");
        }

        await File.WriteAllTextAsync(Path.Combine(output, "report.md"), report.ToString());
    }

    private static async Task ValidateEquivalentScenarios(string output, List<string> errors)
    {
        string[][] families =
        [
            ["photo-array", "photo-stream", "photo-default-tiering"],
            ["scaling-workers-1", "scaling-workers-2", "scaling-workers-4", "scaling-concurrent-2", "scaling-concurrent-4", "cache-disabled", "cache-16m"]
        ];
        foreach (string[] family in families)
        {
            string? expected = null;
            foreach (string id in family)
            {
                string file = Path.Combine(output, id, "result.json");
                if (!File.Exists(file))
                {
                    continue;
                }
                using var result = JsonDocument.Parse(await File.ReadAllTextAsync(file));
                string hash = result.RootElement.GetProperty("jpegSha256").GetString()!;
                expected ??= hash;
                if (hash != expected)
                {
                    errors.Add($"Equivalent output differs: {id}.");
                }
            }
        }
    }

    internal static void SelfTest()
    {
        Scenario[] scenarios = Scenarios();
        if (scenarios.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != scenarios.Length
            || scenarios.Any(item => item.Width < 1 || item.Height < 1 || (long)item.Width * item.Height > 12_000_000)
            || scenarios.Max(item => item.Workers) != 4 || scenarios.Max(item => item.Concurrent) != 4
            || !scenarios.Any(item => item.Tiered) || !scenarios.Any(item => !item.Stream))
        {
            throw new InvalidDataException("Workload bounds or coverage changed.");
        }

        var photograph = new PpmImage(2, 1, [1, 2, 3, 4, 5, 6]);
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        foreach (Scenario scenario in scenarios.Where(item => item.Id.StartsWith("coefficients-64m-", StringComparison.Ordinal)))
        {
            int rows = scenario.Height / 8;
            long planned = QuantizedImage.PlannedBytes([256, 256, 256], [rows, rows, rows], rows, sampling, segmented: true);
            if ((planned <= QuantizedImage.MaximumBytes) != scenario.Id.EndsWith("-below", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Coefficient scenarios no longer bracket actual segmented capacity.");
            }
        }
        if (!Pixels(photograph, new("fixture", 3, 2)).AsSpan().SequenceEqual(new byte[] { 1, 2, 3, 4, 5, 6, 1, 2, 3, 1, 2, 3, 4, 5, 6, 1, 2, 3 }))
        {
            throw new InvalidDataException("Tiled input generator mismatch.");
        }
        if (!Pixels(photograph, new("fixture-scaled", 4, 1, ScalePhotograph: true)).AsSpan()
            .SequenceEqual(new byte[] { 1, 2, 3, 1, 2, 3, 4, 5, 6, 4, 5, 6 }))
        {
            throw new InvalidDataException("Photograph enlargement mismatch.");
        }

        using var sink = new CountingStream();
        sink.Write([1, 2, 3]);
        sink.WriteByte(4);
        if (sink.Length != 4)
        {
            throw new InvalidDataException("Stream byte accounting mismatch.");
        }
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static string HashFile(string file) { using FileStream stream = File.OpenRead(file); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
    private static Task Write(string output, string name, object value) => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, Json));

    private sealed class CountingStream : Stream
    {
        private long length;
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => this.length;
        public override long Position { get => this.length; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer) => this.length += buffer.Length;
        public override void WriteByte(byte value) => this.length++;
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
