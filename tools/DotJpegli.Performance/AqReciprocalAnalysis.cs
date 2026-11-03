namespace DotJpegli.Performance;

using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>Untimed numerical evidence, with no benchmark or acceptance verdict.</summary>
internal static class AqReciprocalAnalysis
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private sealed record SiteCounts(string Site, long Accepted, long Fallback);
    private sealed record Case(string Image, string PixelsSha256, int Width, int Height, float Distance,
        int Sampling, int Progression, string JpegSha256, int JpegBytes, SiteCounts[] Sites,
        long VectorBatches, long FallbackVectorBatches, long Vector256Batches, long FallbackVector256Batches);

    internal static async Task<int> Run(string[] args)
    {
        if (args.Length != 5 || args[0] != "--aq-reciprocal-analysis" || args[1] != "--photograph" || args[3] != "--output")
        {
            throw new ArgumentException("Expected --aq-reciprocal-analysis --photograph <PPM> --output <empty-directory>.", nameof(args));
        }
        string output = Path.GetFullPath(args[4]);
        if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
        {
            throw new InvalidDataException("Numerical evidence output directory must be empty.");
        }
        var photograph = PpmImage.Read(args[2]);
        if (photograph.Width > 1024 || photograph.Height > 1024)
        {
            throw new InvalidDataException("The bounded numerical analysis accepts photographs up to 1024 by 1024 pixels.");
        }
        var sources = ControlledCorpus.Create().Where(source => source.Name is
            "smooth-gradient" or "low-contrast" or "low-noise" or "full-noise" or "tiny-odd-edges").ToList();
        sources.Insert(0, ("photograph", photograph));
        _ = Directory.CreateDirectory(output);
        await Write(output, "run.json", new
        {
            schemaVersion = 1,
            scope = "Untimed binary32 numerical certification and actual AQ fallback counts; no benchmarks, native acceptance or speed verdict",
            startedAtUtc = DateTimeOffset.UtcNow,
            runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            operatingSystem = RuntimeInformation.OSDescription,
            vectorLanes = Vector<float>.Count,
            vectorAccelerated = Vector.IsHardwareAccelerated,
            vector256Accelerated = Vector256.IsHardwareAccelerated,
            encoderSha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
            runnerSha256 = Hash(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location)),
            policy = "One Original and one forced guarded encode per case, distance 1/4, 444/420, baseline/progressive level 2, AQ and optimized Huffman, one worker, default token budget",
            sources = sources.Select(source => new { source.Name, source.Image.Width, source.Image.Height, pixelsSha256 = Hash(source.Image.Pixels) }),
            notMeasured = "Latency, allocations, retained capacity, peak working memory, other hosts, full native acceptance"
        });
        await Write(output, "summary.json", new { status = "incomplete", numericalParity = false, performanceVerdict = "not-evaluated" });
        var cases = new List<Case>();
        foreach ((string name, PpmImage image) in sources)
        {
            foreach (float distance in new[] { 1f, 4f })
            {
                foreach (int sampling in new[] { 444, 420 })
                {
                    foreach (int progression in new[] { 0, 2 })
                    {
                        var options = new JpegliEncoderOptions
                        {
                            Distance = distance,
                            Subsampling = sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                            ProgressiveLevel = progression,
                            AdaptiveQuantization = true,
                            OptimizeHuffmanTables = true,
                            MaxDegreeOfParallelism = 1,
                            AqRationalKernel = KernelPreference.Original
                        };
                        byte[] original = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, options: options);
                        var diagnostics = new AqRationalDiagnostics();
                        byte[] guarded = JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, options: options with
                        {
                            AqRationalKernel = KernelPreference.Optimized,
                            AqRationalDiagnostics = diagnostics
                        });
                        if (!original.AsSpan().SequenceEqual(guarded) || diagnostics.Calls == 0)
                        {
                            throw new InvalidDataException($"AQ numerical parity or diagnostic coverage failed for {name}/{distance}/{sampling}/{progression}.");
                        }
                        cases.Add(new(name, Hash(image.Pixels), image.Width, image.Height, distance, sampling,
                            progression, Hash(guarded), guarded.Length, Counts(diagnostics),
                            diagnostics.VectorBatches, diagnostics.FallbackVectorBatches,
                            diagnostics.Vector256Batches, diagnostics.FallbackVector256Batches));
                    }
                }
            }
            await Write(output, "cases.json", cases);
        }
        object stress = DivisionStress();
        SiteCounts[] totals = Enum.GetValues<AqRationalSite>().Select(site => new SiteCounts(site.ToString(),
            cases.Sum(result => result.Sites.Single(count => count.Site == site.ToString()).Accepted),
            cases.Sum(result => result.Sites.Single(count => count.Site == site.ToString()).Fallback))).ToArray();
        await Write(output, "summary.json", new
        {
            schemaVersion = 1,
            status = "complete",
            numericalParity = true,
            cases = cases.Count,
            totals,
            vectorBatches = cases.Sum(result => result.VectorBatches),
            fallbackVectorBatches = cases.Sum(result => result.FallbackVectorBatches),
            vector256Batches = cases.Sum(result => result.Vector256Batches),
            fallbackVector256Batches = cases.Sum(result => result.FallbackVector256Batches),
            divisionStress = stress,
            decision = "Retain original automatic division; numerical certification and fallback counts alone do not establish a speed benefit",
            performanceVerdict = "not-evaluated"
        });
        Console.WriteLine($"AQ numerical analysis passed: {cases.Count} exact JPEG comparisons; no benchmark executed.");
        return 0;
    }

    private static SiteCounts[] Counts(AqRationalDiagnostics diagnostics)
        => Enum.GetValues<AqRationalSite>().Select(site =>
            new SiteCounts(site.ToString(), diagnostics.GetAccepted(site), diagnostics.GetFallback(site))).ToArray();

    private static object DivisionStress()
    {
        // Full-bit deterministic samples include invalid signs, zero, subnormals,
        // overflow and exceptional values. Exact midpoint/adversarial cell proofs
        // live in the independent BigInteger regressions, rather than this sample.
        const int samples = 262144;
        uint state = 0x71394b5d;
        uint Next()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
        int accepted = 0, fallback = 0;
        for (int i = 0; i < samples; i++)
        {
            float numerator = BitConverter.Int32BitsToSingle(unchecked((int)Next()));
            float denominator = BitConverter.Int32BitsToSingle(unchecked((int)Next()));
            float expected = numerator / denominator;
            float actual = AqReciprocal.Divide(numerator, denominator, out bool usedFallback);
            if (BitConverter.SingleToInt32Bits(expected) != BitConverter.SingleToInt32Bits(actual))
            {
                throw new InvalidDataException("Guarded division changed binary32 output bits in the full-bit sample.");
            }
            if (usedFallback)
            {
                fallback++;
            }
            else
            {
                accepted++;
            }
        }
        return new { samples, seed = "0x71394b5d", accepted, fallback, exactBits = true };
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static Task Write<T>(string output, string name, T value)
        => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
}
