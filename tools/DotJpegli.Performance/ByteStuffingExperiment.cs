namespace DotJpegli.Performance;

using System.Buffers.Binary;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>Bounded, paired diagnostics for scalar and portable SIMD byte stuffing.</summary>
internal static class ByteStuffingExperiment
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private sealed record Workload(string Id, string Scope, int Iterations, long UnitsPerIteration,
        object Input, Action<bool> Execute, Func<byte[]> Snapshot);
    private sealed record Result(string Id, string Scope, int Iterations, long UnitsPerIteration,
        object Input, string OutputSha256, long OutputBytes, KernelComparison.Quartet[] Quartets,
        KernelComparison.Analysis Analysis);

    internal static Task<int> Run(string[] args)
    {
        Program.RequireCi();
        string? corpus = null, output = null;
        for (int index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--byte-stuffing-experiment":
                    break;
                case "--corpus" when index + 1 < args.Length:
                    corpus = args[++index];
                    break;
                case "--output" when index + 1 < args.Length:
                    output = args[++index];
                    break;
                default:
                    throw new ArgumentException($"Unknown or incomplete byte-stuffing argument: {args[index]}.");
            }
        }
        return RunCore(corpus ?? throw new ArgumentException("--corpus is required."),
            output ?? throw new ArgumentException("--output is required."));
    }

    // Kept separate so an explicitly authorized local research harness can use
    // the identical protocol. The normal command entry always requires CI.
    internal static async Task<int> RunCore(string corpus, string output)
    {
        if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
        {
            throw new InvalidDataException("Byte-stuffing evidence output directory must be empty.");
        }

        _ = Directory.CreateDirectory(output);
        _ = Directory.CreateDirectory(Path.Combine(output, "raw-comparisons"));
        _ = Directory.CreateDirectory(Path.Combine(output, "inputs"));
        _ = Directory.CreateDirectory(Path.Combine(output, "outputs"));
        await WriteJson(output, "run.json", new
        {
            schemaVersion = 1,
            mode = "bounded-byte-stuffing-experiment",
            startedAtUtc = DateTimeOffset.UtcNow,
            revision = Environment.GetEnvironmentVariable("DOTJPEGLI_PERFORMANCE_REVISION")
                ?? Environment.GetEnvironmentVariable("GITHUB_SHA"),
            encoderSha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
            runnerSha256 = Hash(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location)),
            environment = Program.EnvironmentDetails(),
            processAffinityMask = OperatingSystem.IsWindows() ? Process.GetCurrentProcess().ProcessorAffinity.ToInt64() : (long?)null,
            method = new
            {
                quartets = 6,
                warmupsPerLeg = 3,
                rampOperationsPerVariantBeforeComparison = 32,
                fixedIterationsPerLeg = new { directWordExpansion = 256, tokenReplay = 200, completeJpeg = 100, tinyCompleteJpeg = 2000 },
                payloadGenerator = "xorshift32 seed0x731997, shifts13/17/5, low byte per step; deterministic replacements enforce FF distributions.",
                order = "Balanced ABBA/BAAB; A=original, B=optimized; alternating by comparison index and block.",
                timing = "Fixed work counts; parity and snapshots before/after each timed leg; no hashing or output copying inside direct-kernel timing.",
                allocation = "Total allocated bytes and GC collections per timed leg; no forced collections; retained capacity and peak memory are not inferred.",
                shuffleTable = "One lazy immutable 256x16-byte vector array:4096 payload bytes plus managed header; initialized before timing; no per-call table allocation.",
                scope = "Four word distributions, two token replay workloads and eight complete JPEG controls; no native or full-corpus verdict. Automatic policy is unchanged."
            }
        });
        await WriteJson(output, "summary.json", new { status = "incomplete", expectedComparisons = 14 });
        var results = new List<Result>();
        IEnumerable<Workload> workloads = CreateWordWorkloads().Concat(CreateReplayWorkloads()).Concat(CreateEncodeWorkloads(corpus, output));
        foreach (Workload workload in workloads)
        {
            // Both variants receive identical untimed work before retaining the
            // control or measuring a leg, allowing the CPU clocks to settle.
            for (int ramp = 0; ramp < 32; ramp++)
            {
                workload.Execute(false);
                workload.Execute(true);
            }
            workload.Execute(false);
            byte[] expected = workload.Snapshot();
            workload.Execute(true);
            RequireSame(expected, workload.Snapshot());
            await File.WriteAllBytesAsync(Path.Combine(output, "outputs", workload.Id + (workload.Scope == "complete-jpeg" ? ".jpg" : ".bin")), expected);
            int comparison = results.Count;
            var samples = new List<KernelComparison.Quartet>();
            for (int block = 0; block < 6; block++)
            {
                string order = (comparison + block) % 2 == 0 ? "ABBA" : "BAAB";
                var legs = new KernelComparison.Leg[4];
                for (int position = 0; position < 4; position++)
                {
                    legs[position] = Measure(workload, order[position] == 'B', position, expected);
                }
                samples.Add(new(block, order, legs));
                await WriteJson(Path.Combine(output, "raw-comparisons"), workload.Id + ".json", new
                {
                    schemaVersion = 1,
                    workload.Id,
                    workload.Scope,
                    workload.Iterations,
                    workload.UnitsPerIteration,
                    workload.Input,
                    outputSha256 = Hash(expected),
                    outputBytes = expected.LongLength,
                    comparisonIndex = comparison,
                    expectedQuartets = 6,
                    completedQuartets = samples.Count,
                    updatedAtUtc = DateTimeOffset.UtcNow,
                    quartets = samples
                });
            }
            var policy = new Policy { Blocks = 6, Warmups = 3, MinimumIterations = workload.Iterations, MaximumIterations = workload.Iterations };
            KernelComparison.Quartet[] quartets = samples.ToArray();
            KernelComparison.Analysis analysis = KernelComparison.Analyze(quartets, workload.Iterations, policy, comparison);
            results.Add(new(workload.Id, workload.Scope, workload.Iterations, workload.UnitsPerIteration,
                workload.Input, Hash(expected), expected.LongLength, quartets, analysis));
            await WriteJson(output, "cases.json", results);
            Console.WriteLine(FormattableString.Invariant($"{workload.Id}: optimized/original={analysis.PairedRatios?.Median:F6}; preference={analysis.EvidencePreference}; valid={analysis.InvalidReasons.Length == 0}; {results.Count}/14"));
        }
        string[] invalid = results.SelectMany(item => item.Analysis.InvalidReasons.Select(reason => item.Id + ": " + reason)).ToArray();
        if (results.Count != 14)
        {
            invalid = [.. invalid, "Comparison coverage is incomplete."];
        }
        await WriteJson(output, "summary.json", new
        {
            status = invalid.Length == 0 ? "completed" : "invalid",
            valid = invalid.Length == 0,
            comparisons = results.Count,
            expectedComparisons = 14,
            invalidReasons = invalid,
            parity = "Every completed comparison produced identical bytes before and after every timed leg.",
            defaultPolicy = "Unchanged; measured evidence is diagnostic and host-specific."
        });
        return invalid.Length == 0 ? 0 : 2;
    }

    private static KernelComparison.Leg Measure(Workload workload, bool optimized, int position, byte[] expected)
    {
        for (int warmup = 0; warmup < 3; warmup++)
        {
            workload.Execute(optimized);
        }
        RequireSame(expected, workload.Snapshot());
        int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
        long allocated = GC.GetTotalAllocatedBytes(precise: true);
        long start = Stopwatch.GetTimestamp();
        for (int iteration = 0; iteration < workload.Iterations; iteration++)
        {
            workload.Execute(optimized);
        }
        long ticks = Stopwatch.GetTimestamp() - start;
        long bytes = GC.GetTotalAllocatedBytes(precise: true) - allocated;
        var leg = new KernelComparison.Leg(position, optimized ? "optimized" : "original",
            ticks * (1_000_000_000.0 / Stopwatch.Frequency) / workload.Iterations, bytes,
            GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2);
        RequireSame(expected, workload.Snapshot());
        return leg;
    }

    private static IEnumerable<Workload> CreateWordWorkloads()
    {
        foreach (string distribution in new[] { "no-ff", "uniform", "ff-one-in-eight", "all-ff" })
        {
            byte[] input = Payload(distribution, 32768 * 8);
            ulong[] words = Enumerable.Range(0, 32768).Select(index => BinaryPrimitives.ReadUInt64BigEndian(input.AsSpan(index * 8, 8))).ToArray();
            byte[] destination = new byte[(words.Length * 16) + 16];
            int length = 0;
            void Execute(bool optimized)
            {
                int offset = 0;
                if (optimized)
                {
                    foreach (ulong word in words)
                    {
                        offset += EntropyByteStuffing.StuffWord(word, destination.AsSpan(offset, 16));
                    }
                }
                else
                {
                    foreach (ulong word in words)
                    {
                        offset += EntropyByteStuffing.StuffWordOriginal(word, destination.AsSpan(offset, 16));
                    }
                }
                length = offset;
            }
            yield return new("word-" + distribution, "direct-word-expansion", 256, words.LongLength,
                new { distribution, inputSha256 = Hash(input), inputBytes = input.LongLength, ffBytes = input.LongCount(value => value == 0xff) },
                Execute, () => destination.AsSpan(0, length).ToArray());
        }
    }

    private static IEnumerable<Workload> CreateReplayWorkloads()
    {
        var table = HuffmanTable.CreateDefault(isAc: true, chroma: false);
        ulong[] lookups = table.GetPrebiasedCodes();
        foreach (bool rawOnly in new[] { false, true })
        {
            byte[] input = Payload(rawOnly ? "all-ff" : "uniform", 32768 * 3);
            uint[] tokens = new uint[32768];
            for (int index = 0; index < tokens.Length; index++)
            {
                uint payload = ((uint)input[index * 3] << 16) | ((uint)input[(index * 3) + 1] << 8) | input[(index * 3) + 2];
                int symbol = table.Symbols[index % table.Symbols.Length];
                int count = symbol == 0xf0 ? 0 : symbol & 15;
                tokens[index] = rawOnly || index % 11 == 10 ? (6U << 24) | (24U << 27) | payload
                    : EntropyTokens.PackHuffman(1, symbol, payload & ((1U << count) - 1), count);
            }
            var destination = new MemoryStream(tokens.Length * 8);
            void Execute(bool optimized)
            {
                destination.SetLength(0);
                destination.Position = 0;
                var writer = new BitWriter(destination, buffered: true,
                    stuffingKernel: optimized ? KernelPreference.Optimized : KernelPreference.Original);
                if (writer.WriteEntropyTokens(tokens, lookups, 256) != tokens.Length)
                {
                    throw new InvalidDataException("The diagnostic token workload did not use complete canonical replay.");
                }
                writer.Flush();
            }
            yield return new(rawOnly ? "replay-raw-all-ff" : "replay-canonical-mixed", "bit-writer-token-replay", 200, tokens.LongLength,
                new { rawOnly, inputSha256 = Hash(input), tokens = tokens.Length, tableCounts = table.Counts, tableSymbols = table.Symbols },
                Execute, destination.ToArray);
        }
    }

    private static IEnumerable<Workload> CreateEncodeWorkloads(string corpus, string output)
    {
        var images = Program.ReadCorpus(corpus).Where(item => item.Id is "kodim01" or "low-noise" or "low-contrast" or "tiny-odd-edges")
            .ToDictionary(item => item.Id, item => item.Image, StringComparer.Ordinal);
        (string Id, int Sampling, int Progression, float Distance)[] cases =
        [
            ("kodim01", 444, 0, 1), ("kodim01", 420, 2, 4),
            ("low-noise", 420, 0, 1), ("low-noise", 444, 2, 4),
            ("low-contrast", 444, 2, 1), ("low-contrast", 420, 0, 4),
            ("tiny-odd-edges", 420, 2, 1), ("tiny-odd-edges", 444, 0, 4)
        ];
        foreach ((string id, int sampling, int progression, float distance) in cases)
        {
            PpmImage image = images[id];
            image.Write(Path.Combine(output, "inputs", id + ".ppm"));
            var original = new JpegliEncoderOptions
            {
                Distance = distance,
                ProgressiveLevel = progression,
                Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                OptimizeHuffmanTables = true,
                AdaptiveQuantization = true,
                ForceBaseline = true,
                MaxDegreeOfParallelism = 1,
                EntropyByteStuffingKernel = KernelPreference.Original
            };
            JpegliEncoderOptions optimized = original with { EntropyByteStuffingKernel = KernelPreference.Optimized };
            byte[] current = [];
            void Execute(bool vector) => current = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, vector ? optimized : original);
            yield return new(FormattableString.Invariant($"encode-{id}-{sampling}-p{progression}-d{distance}"), "complete-jpeg",
                id == "tiny-odd-edges" ? 2000 : 100, 1,
                new { id, image.Width, image.Height, sampling, progression, distance, pixelsSha256 = Hash(image.Pixels) }, Execute, () => current);
        }
    }

    private static byte[] Payload(string distribution, int length)
    {
        uint random = 0x731997;
        byte[] bytes = new byte[length];
        for (int index = 0; index < bytes.Length; index++)
        {
            random ^= random << 13;
            random ^= random >> 17;
            random ^= random << 5;
            bytes[index] = (byte)random;
            if (distribution == "all-ff" || (distribution == "ff-one-in-eight" && index % 8 == 0))
            {
                bytes[index] = 0xff;
            }
            else if (distribution != "uniform" && bytes[index] == 0xff)
            {
                bytes[index] = 0xfe;
            }
        }
        return bytes;
    }

    private static void RequireSame(byte[] expected, byte[] actual)
    {
        if (!expected.AsSpan().SequenceEqual(actual))
        {
            throw new InvalidDataException("Original and optimized byte stuffing produced different output.");
        }
    }
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static Task WriteJson<T>(string output, string name, T value)
        => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
}
