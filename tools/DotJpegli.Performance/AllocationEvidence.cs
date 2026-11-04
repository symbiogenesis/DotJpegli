namespace DotJpegli.Performance;

using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Parity;

/// <summary>Untimed allocation traffic observations; no latency or acceptance verdict.</summary>
internal static class AllocationEvidence
{
    private const string Pin = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal static async Task<int> Run(string[] args)
    {
        var options = Options.Parse(args);
        if (options.Diagnostic is not null || !OperatingSystem.IsLinux())
        {
            throw new ArgumentException("Allocation observations require the full Kodak matrix and Linux GNU linker probe.");
        }

        string probe = Path.Combine(Path.GetDirectoryName(options.Native)!, "jpegli-allocation-native");
        if (!File.Exists(probe) || (Directory.Exists(options.Output) && Directory.EnumerateFileSystemEntries(options.Output).Any()))
        {
            throw new InvalidDataException("Allocation probe must exist and evidence output must be empty.");
        }

        _ = Directory.CreateDirectory(options.Output);
        string inputs = Path.Combine(options.Output, "inputs"), jpegs = Path.Combine(options.Output, "jpegs");
        _ = Directory.CreateDirectory(inputs);
        _ = Directory.CreateDirectory(jpegs);
        if ((await Program.Native(options.Native, options.Output, options.Output, "version")).Trim() != Pin ||
            (await Program.Native(probe, options.Output, options.Output, "version")).Trim() != Pin)
        {
            throw new InvalidDataException("Allocation encoders do not report the pinned upstream revision.");
        }

        _ = await Program.Native(probe, options.Output, options.Output, "allocation-self-test");
        List<Observation> observations = [];
        DateTimeOffset started = DateTimeOffset.UtcNow;
        await Write("summary.json", Summary(false));
        foreach ((string id, string category, string sourcePath, PpmImage image) in Program.ReadCorpus(options.Corpus).Where(item => item.Category == "natural"))
        {
            string input = Path.Combine(inputs, id + ".ppm");
            image.Write(input);
            string inputHash = HashFile(input);
            foreach (int sampling in Evidence.Samplings)
            {
                foreach (int progression in Evidence.Progressions)
                {
                    foreach (float distance in Evidence.Distances)
                    {
                        string stem = FormattableString.Invariant($"{id}_{sampling}_p{progression}_d{distance:0}");
                        string nativePath = Path.Combine(jpegs, stem + "_native.jpg"), probePath = Path.Combine(jpegs, stem + "_probe.jpg");
                        string[] settings = [distance.ToString("R", CultureInfo.InvariantCulture), sampling.ToString(CultureInfo.InvariantCulture), progression.ToString(CultureInfo.InvariantCulture)];
                        _ = await Program.Native(options.Native, options.Output, options.Output, "encode", input, nativePath, settings[0], settings[1], settings[2], "1", "1");
                        string nativeHash = HashFile(nativePath);
                        string result = await Program.Native(probe, options.Output, options.Output, "allocations", input, settings[0], settings[1], settings[2], probePath);
                        long[] nativeSamples = ParseNative(result, image.Width, image.Height, sampling, progression, distance, new FileInfo(nativePath).Length);
                        if (HashFile(probePath) != nativeHash)
                        {
                            throw new InvalidDataException("Instrumented native allocation probe changed the reference JPEG.");
                        }

                        var encoderOptions = new JpegliEncoderOptions
                        {
                            ColorSpace = JpegColorSpace.YCbCr,
                            Distance = distance,
                            ProgressiveLevel = progression,
                            Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                            OptimizeHuffmanTables = true,
                            AdaptiveQuantization = true,
                            ForceBaseline = true,
                            MaxDegreeOfParallelism = 1
                        };
                        byte[] expected = Encode();
                        for (int warmup = 0; warmup < 5; warmup++)
                        {
                            Verify(Encode());
                        }

                        long[] managedSamples = new long[3];
                        for (int sample = 0; sample < managedSamples.Length; sample++)
                        {
                            long before = GC.GetTotalAllocatedBytes(precise: true);
                            byte[] encoded = Encode();
                            managedSamples[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
                            Verify(encoded);
                        }

                        byte[] nativeJpeg = await File.ReadAllBytesAsync(nativePath);
                        if (JpegStructure.Compare(expected, nativeJpeg, image, sampling, progression) is string difference)
                        {
                            throw new InvalidDataException("Allocation encoder settings differ: " + difference);
                        }

                        observations.Add(new(id, category, sourcePath, image.Width, image.Height, inputHash, sampling, progression, distance,
                            managedSamples, nativeSamples, expected.Length, nativeJpeg.Length, Hash(expected), nativeHash));
                        await Write("cases.json", observations);
                        Console.WriteLine($"Allocation observation {stem}: {observations.Count}/192 retained.");

                        byte[] Encode() => JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, encoderOptions);
                        void Verify(byte[] encoded)
                        {
                            if (!encoded.AsSpan().SequenceEqual(expected))
                            {
                                throw new InvalidDataException("Managed JPEG changed during allocation observations.");
                            }
                        }
                    }
                }
            }
        }

        if (observations.Count != 192)
        {
            throw new InvalidDataException("Incomplete allocation photograph matrix.");
        }

        await Write("summary.json", Summary(true));
        Console.WriteLine("Complete untimed managed/native allocation matrix retained; no latency verdict.");
        return 0;

        object Summary(bool complete) => new
        {
            schemaVersion = 1,
            complete,
            cases = observations.Count,
            expectedCases = 192,
            startedAtUtc = started,
            testedRevision = Environment.GetEnvironmentVariable("GITHUB_SHA"),
            githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
            upstreamPin = Pin,
            managedEncoderSha256 = HashFile(typeof(JpegliEncoder).Assembly.Location),
            managedRunnerSha256 = HashFile(typeof(AllocationEvidence).Assembly.Location),
            corpusManifestSha256 = HashFile(Path.Combine(AppContext.BaseDirectory, "corpus.json")),
            nativeExecutableSha256 = HashFile(options.Native),
            allocationProbeSha256 = HashFile(probe),
            environment = Program.EnvironmentDetails(),
            warmups = 5,
            samplesPerCase = 3,
            managedMethod = "GC.GetTotalAllocatedBytes(precise:true) delta per complete encode; includes returned JPEG; warmed pools; prepared input excluded",
            nativeMethod = "Successful requested bytes from GNU linker-wrapped malloc/calloc/realloc/aligned_alloc/posix_memalign/memalign and replaced C++ new; full realloc request counted; includes returned JPEG; prepared input excluded",
            scope = "Untimed allocation traffic on Kodak24, distances1/4, sampling444/420, progression0/2; excludes allocator overhead, retained pool capacity and peak memory; no latency or acceptance verdict"
        };
        Task Write<T>(string name, T value) => File.WriteAllTextAsync(Path.Combine(options.Output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
    }

    internal static long[] ParseNative(string text, int width, int height, int sampling, int progression, float distance, long bytes)
    {
        using var document = JsonDocument.Parse(text);
        JsonElement value = document.RootElement;
        if (value.GetProperty("upstreamPin").GetString() != Pin || value.GetProperty("encoder").GetString() != "native-jpegli" ||
            value.GetProperty("width").GetInt32() != width || value.GetProperty("height").GetInt32() != height ||
            value.GetProperty("distance").GetSingle() != distance || value.GetProperty("sampling").GetString() != sampling.ToString(CultureInfo.InvariantCulture) ||
            value.GetProperty("progressiveLevel").GetInt32() != progression || value.GetProperty("warmups").GetInt32() != 5 ||
            value.GetProperty("bytes").GetInt64() != bytes || value.GetProperty("method").GetString() != "gnu-linker-wrap-and-cxx-new")
        {
            throw new InvalidDataException("Native allocation observation identity or settings mismatch.");
        }

        JsonElement[] samples = value.GetProperty("samples").EnumerateArray().ToArray();
        long[] allocated = samples.Select(sample => sample.GetProperty("allocatedBytes").GetInt64()).ToArray();
        if (samples.Length != 3 || allocated.Any(sample => sample < bytes) || samples.Any(sample => sample.GetProperty("allocationCalls").GetInt64() <= 0))
        {
            throw new InvalidDataException("Native allocation observations are incomplete or exclude the JPEG output.");
        }

        return allocated;
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static string HashFile(string path) => Hash(File.ReadAllBytes(path));

    private sealed record Observation(string Id, string Category, string SourcePath, int Width, int Height, string PpmSha256,
        int Sampling, int ProgressiveLevel, float Distance, long[] ManagedAllocatedBytes, long[] NativeAllocatedBytes,
        long ManagedBytes, long NativeBytes, string ManagedJpegSha256, string NativeJpegSha256);
}
