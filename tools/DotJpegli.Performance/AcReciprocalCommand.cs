namespace DotJpegli.Performance;

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>Bounded prepared-kernel experiment, separate from encoder acceptance.</summary>
internal static class AcReciprocalCommand
{
    private const int Count = 16384;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private sealed record Input(string Id, string PixelsSha256, float[] Coefficients, float[] Divisors,
        AcReciprocalExperiment.Interval[] Reciprocals, float[] Lower, float[] Upper, float[] Thresholds);
    private sealed record Result(string Id, string Kernel, string InputSha256, int Coefficients,
        int AmbiguousCoefficients, int AmbiguousVectorBatches, int VectorBatches, int Iterations,
        KernelComparison.Quartet[] Quartets, KernelComparison.Analysis Analysis);

    // Program.Main enforces its existing CI guard before dispatch. A one-time
    // explicitly authorized local harness may invoke this entry directly.
    internal static async Task<int> Run(string[] args)
    {
        if (args.Length != 5 || args[0] != "--ac-reciprocal-experiment" || args[1] != "--photograph" || args[3] != "--output")
        {
            throw new ArgumentException("Expected --ac-reciprocal-experiment --photograph <PPM> --output <empty-directory>.", nameof(args));
        }
        string output = Path.GetFullPath(args[4]);
        if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
        {
            throw new InvalidDataException("Experiment output directory must be empty.");
        }
        _ = Directory.CreateDirectory(output);
        var policy = new Policy { Blocks = 6, MaximumIterations = 16384 };
        var sources = ControlledCorpus.Create().Where(source => source.Name is "smooth-gradient" or "low-contrast" or "low-noise" or "full-noise" or "tiny-odd-edges").ToList();
        sources.Insert(0, ("photograph", PpmImage.Read(args[2])));
        Input[] inputs = sources.Select(source => Prepare(source.Name, source.Image)).Append(Boundaries()).ToArray();
        var results = new List<Result>();
        using var process = Process.GetCurrentProcess();
        nint? originalAffinity = null;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            originalAffinity = process.ProcessorAffinity;
            long available = originalAffinity.Value;
            process.ProcessorAffinity = (nint)(available & -available);
        }
        try
        {
            await WriteJson(output, "run.json", new
            {
                schemaVersion = 1,
                startedAtUtc = DateTimeOffset.UtcNow,
                scope = "Prepared scalar and portable SIMD AC kernels only; no JPEG/latency/parity acceptance verdict",
                runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                operatingSystem = RuntimeInformation.OSDescription,
                processorCount = Environment.ProcessorCount,
                processorAffinity = OperatingSystem.IsWindows() || OperatingSystem.IsLinux() ? process.ProcessorAffinity : (long?)null,
                vectorLanes = Vector<float>.Count,
                vectorAccelerated = Vector.IsHardwareAccelerated,
                stopwatchFrequency = Stopwatch.Frequency,
                processPriority = process.PriorityClass.ToString(),
                encoderSha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
                runnerSha256 = Hash(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location)),
                policy,
                preCalibrationWarmupsPerVariant = 64,
                calibrationTargetMultiplier = 4,
                calibrationTargetMilliseconds = policy.TargetBlockMilliseconds * 4,
                preparation = "16384 deterministic AC samples per source, RGB-to-Y luminance DCT, edge replication; both distances and luma 444/420 tables; representative nonadaptive and AQ thresholds, not measured AQ strengths",
                timed = "finite/range validation, two endpoint products, strict suppression agreement, ties-to-even agreement and exact-division fallback; SIMD falls back a whole ambiguous batch; preparation, diagnostics and parity checks excluded",
                notMeasured = "DC, coefficient demotion/stores into zigzag, complete encoding, actual AQ-derived fallback distributions, allocations/peak memory, other CPUs/runtimes",
                inputs = inputs.Select(input => new { input.Id, input.PixelsSha256, kernelInputSha256 = InputHash(input) })
            });
            foreach (Input input in inputs)
            {
                bool[] ambiguous = Enumerable.Range(0, Count).Select(i =>
                    !AcReciprocalExperiment.TryQuantize(input.Coefficients[i], input.Reciprocals[i], input.Thresholds[i], out _)).ToArray();
                int ambiguousCount = ambiguous.Count(value => value);
                int ambiguousBatches = Enumerable.Range(0, Count / Vector<float>.Count)
                    .Count(batch => ambiguous.AsSpan(batch * Vector<float>.Count, Vector<float>.Count).Contains(true));
                foreach (bool vector in new[] { false, true })
                {
                    if (vector && !Vector.IsHardwareAccelerated)
                    {
                        continue;
                    }
                    short[] originalShort = new short[Count], candidateShort = new short[Count];
                    float[] originalFloat = new float[Count], candidateFloat = new float[Count];
                    Action baseline = vector
                        ? () => AcReciprocalExperiment.VectorExact(input.Coefficients, input.Divisors, input.Thresholds, originalFloat)
                        : () => AcReciprocalExperiment.ScalarExact(input.Coefficients, input.Divisors, input.Thresholds, originalShort);
                    Action candidate = vector
                        ? () => AcReciprocalExperiment.VectorGuarded(input.Coefficients, input.Divisors, input.Lower, input.Upper, input.Thresholds, candidateFloat)
                        : () => AcReciprocalExperiment.ScalarGuarded(input.Coefficients, input.Divisors, input.Reciprocals, input.Thresholds, candidateShort);
                    void Verify()
                    {
                        if (!originalShort.AsSpan().SequenceEqual(candidateShort) || !originalFloat.AsSpan().SequenceEqual(candidateFloat))
                        {
                            throw new InvalidDataException("Exact and guarded kernel results differ.");
                        }
                    }
                    for (int warmup = 0; warmup < 64; warmup++)
                    {
                        baseline();
                        candidate();
                        Verify();
                    }
                    long start = Stopwatch.GetTimestamp();
                    for (int iteration = 0; iteration < 32; iteration++)
                    {
                        baseline();
                    }
                    double calibration = Stopwatch.GetElapsedTime(start).TotalNanoseconds / 32;
                    int iterations = Evidence.Iterations(calibration / 4, policy);
                    int comparisonIndex = results.Count;
                    var quartets = new List<KernelComparison.Quartet>();
                    for (int block = 0; block < policy.Blocks; block++)
                    {
                        string order = (comparisonIndex + block) % 2 == 0 ? "ABBA" : "BAAB";
                        var legs = new KernelComparison.Leg[4];
                        for (int position = 0; position < order.Length; position++)
                        {
                            bool original = order[position] == 'A';
                            Action action = original ? baseline : candidate;
                            // Symmetric warmups keep timing order out of JIT and
                            // initial cache population. No GC is forced.
                            for (int warmup = 0; warmup < policy.Warmups; warmup++)
                            {
                                action();
                            }
                            long allocated = GC.GetAllocatedBytesForCurrentThread();
                            int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
                            start = Stopwatch.GetTimestamp();
                            for (int iteration = 0; iteration < iterations; iteration++)
                            {
                                action();
                            }
                            double elapsed = Stopwatch.GetElapsedTime(start).TotalNanoseconds;
                            legs[position] = new(position, original ? "original" : "optimized", elapsed / iterations,
                                GC.GetAllocatedBytesForCurrentThread() - allocated, GC.CollectionCount(0) - gen0,
                                GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2);
                            Verify();
                        }
                        quartets.Add(new(block, order, legs));
                    }
                    KernelComparison.Quartet[] samples = quartets.ToArray();
                    KernelComparison.Analysis analysis = KernelComparison.Analyze(samples, iterations, policy, comparisonIndex);
                    results.Add(new(input.Id, vector ? "portable-vector" : "scalar", InputHash(input), Count,
                        ambiguousCount, ambiguousBatches, Count / Vector<float>.Count, iterations, samples, analysis));
                    await WriteJson(output, "results.json", results);
                    Console.WriteLine($"{input.Id}/{(vector ? "vector" : "scalar")}: ratio={analysis.GeometricMeanPairedRatio:F3}, preference={analysis.EvidencePreference}, invalid={analysis.InvalidReasons.Length}, fallback={ambiguousCount}/{Count}");
                }
            }
            await WriteJson(output, "summary.json", new
            {
                schemaVersion = 1,
                completed = true,
                rows = results.Count,
                validRows = results.Count(result => result.Analysis.InvalidReasons.Length == 0),
                originalPreferred = results.Count(result => result.Analysis.EvidencePreference == "original"),
                candidatePreferred = results.Count(result => result.Analysis.EvidencePreference == "optimized"),
                decision = "Retain production division. Assess only valid rows; invalid/inconclusive rows establish no speed result. This bounded prototype does not reject every possible reciprocal algorithm.",
                acceptanceVerdict = "not-evaluated"
            });
            return 0;
        }
        finally
        {
            if (originalAffinity is { } affinity && (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()))
            {
                process.ProcessorAffinity = affinity;
            }
        }
    }

    private static Input Prepare(string id, PpmImage image)
    {
        float[] coefficients = new float[Count], divisors = new float[Count], thresholds = new float[Count];
        ushort[][] tables = [Quantization.CreateTableForDistance(1, 0), Quantization.CreateTableForDistance(4, 0),
            Quantization.CreateTableForDistance(1, 0, 2, 2), Quantization.CreateTableForDistance(4, 0, 2, 2)];
        float[] strengths = [0, 0.25f, 1, 4];
        Span<float> samples = stackalloc float[64], transformed = stackalloc float[64];
        int blocksWide = (image.Width + 7) / 8, blocksHigh = (image.Height + 7) / 8;
        int index = 0;
        for (int block = 0; index < Count; block++)
        {
            // An odd stride spreads this bounded selection across a photograph;
            // repeat tiny inputs so every timed row has identical work counts.
            int sourceBlock = block * 37 % (blocksWide * blocksHigh);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int px = Math.Min((sourceBlock % blocksWide * 8) + x, image.Width - 1);
                    int py = Math.Min((sourceBlock / blocksWide * 8) + y, image.Height - 1);
                    int position = ((py * image.Width) + px) * 3;
                    ColorConversion.RgbToYCbCr(image.Pixels[position], image.Pixels[position + 1], image.Pixels[position + 2], out float luma, out _, out _);
                    samples[(y * 8) + x] = luma - 128;
                }
            }
            ForwardDct.Transform(samples, transformed);
            for (int frequency = 1; frequency < 64 && index < Count; frequency++, index++)
            {
                coefficients[index] = transformed[frequency];
                divisors[index] = tables[block % tables.Length][frequency];
                thresholds[index] = 0.59082f + (strengths[block / tables.Length % strengths.Length] * 0.5f);
            }
        }
        return Create(id, Hash(image.Pixels), coefficients, divisors, thresholds);
    }

    private static Input Boundaries()
    {
        float[] coefficients = new float[Count], divisors = new float[Count], thresholds = new float[Count];
        int[] values = [1, 3, 7, 11, 19, 31, 255, 256, 32767];
        for (int i = 0; i < Count; i++)
        {
            int divisor = values[i / 3 % values.Length];
            float center = i / 27 % 2 == 0 ? ((i / 54 % 32) + 0.5f) * divisor : 0.59082f * divisor;
            float coefficient = i % 3 == 0 ? MathF.BitDecrement(center) : i % 3 == 1 ? center : MathF.BitIncrement(center);
            coefficients[i] = i / 81 % 2 == 0 ? coefficient : -coefficient;
            divisors[i] = divisor;
            thresholds[i] = 0.59082f;
        }
        return Create("adversarial-boundaries", "generated", coefficients, divisors, thresholds);
    }

    private static Input Create(string id, string pixelsHash, float[] coefficients, float[] divisors, float[] thresholds)
    {
        AcReciprocalExperiment.Interval[] reciprocal = divisors.Select(divisor => AcReciprocalExperiment.Interval.Create((int)divisor)).ToArray();
        return new(id, pixelsHash, coefficients, divisors, reciprocal, reciprocal.Select(value => value.Lower).ToArray(),
            reciprocal.Select(value => value.Upper).ToArray(), thresholds);
    }

    private static string InputHash(Input input)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(MemoryMarshal.AsBytes(input.Coefficients.AsSpan()));
        hash.AppendData(MemoryMarshal.AsBytes(input.Divisors.AsSpan()));
        hash.AppendData(MemoryMarshal.AsBytes(input.Thresholds.AsSpan()));
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
    private static Task WriteJson(string output, string name, object value)
        => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
}
