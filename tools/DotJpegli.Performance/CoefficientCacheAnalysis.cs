namespace DotJpegli.Performance;

using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Internal;
using DotJpegli.Parity;

/// <summary>Untimed storage and access-work comparison of immutable coefficient layouts.</summary>
internal static class CoefficientCacheAnalysis
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private sealed record LayoutResult(string Layout, bool DenseFallback, long UsefulBytes, long RentedBytes,
        long PayloadBytes, long RentedPayloadBytes, long IndexBytes, long RentedIndexBytes,
        int CompactTiles, int DenseTiles, CoefficientCacheWork Packing,
        CoefficientCacheWork FullBlockPass, CoefficientCacheWork DcPass, CoefficientCacheWork IndividualCoefficientPass);
    private sealed record ComponentResult(int Component, int Blocks, long NonzeroAc, string CoefficientsSha256,
        long ProductionDenseRentalBytes, LayoutResult[] Layouts);
    private sealed record CaseResult(string Image, int Width, int Height, string PixelsSha256, float Distance,
        int Sampling, ComponentResult[] Components);
    private sealed record CapacityComponent(int Component, string SourceCoefficientsSha256, int SourceBlocks,
        int ReplicatedBlocks, string ReplicatedCoefficientsSha256, long ProductionDenseRentalBytes, int CheckedBlocks);
    private sealed record CapacityResult(string Image, float Distance, int Sampling, int Repetitions,
        long DenseUsefulBytes, long ProductionDenseRentalBytes, bool ProductionDenseAdmitted, CapacityComponent[] Components,
        CapacityLayout[] Layouts);
    private sealed record CapacityLayout(string Layout, bool Admitted, string? Rejection, long AcceptedRentalBytes,
        long ObservedPayloadRentalBytes, long ObservedIndexRentalBytes, long AcceptedUsefulBytes,
        int AcceptedCompactTiles, int AcceptedDenseTiles, CoefficientCacheWork[] AcceptedPacking);

    internal static async Task<int> Run(string[] args)
    {
        if (args.Length != 5 || args[0] != "--coefficient-cache-analysis" || args[1] != "--photograph" || args[3] != "--output")
        {
            throw new ArgumentException("Expected --coefficient-cache-analysis --photograph <PPM> --output <empty-directory>.", nameof(args));
        }
        string output = Path.GetFullPath(args[4]);
        if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
        {
            throw new InvalidDataException("Coefficient storage evidence output directory must be empty.");
        }
        var photograph = PpmImage.Read(args[2]);
        if (photograph.Width > 1024 || photograph.Height > 1024)
        {
            throw new InvalidDataException("The bounded storage analysis accepts photographs up to 1024 by 1024 pixels.");
        }
        var sources = ControlledCorpus.Create().Where(source => source.Name is
            "smooth-gradient" or "low-contrast" or "low-noise" or "full-noise" or "tiny-odd-edges").ToList();
        sources.Insert(0, ("photograph", photograph));
        _ = Directory.CreateDirectory(output);
        await Write(output, "run.json", new
        {
            schemaVersion = 1,
            scope = "Untimed exact coefficient storage, rental-cap admission and abstract access work; offline prototypes, no encoder dispatch change",
            runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            operatingSystem = RuntimeInformation.OSDescription,
            encoderSha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
            runnerSha256 = Hash(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location)),
            maximumAcceptedRentalBytes = QuantizedImage.MaximumBytes,
            blocksPerTile = CoefficientCacheRepresentation.BlocksPerTile,
            coefficientPolicy = "Actual complete QuantizedImage coefficients, zigzag order, AQ, distance 1/4, 444/420, smoothing 0, original narrow zigzag preference",
            accessPolicy = "One full-block pass, one direct-DC pass and one individual-coefficient pass over positions 0..63; every value compared exactly with the original cache",
            workScope = "Counters compare executable offline codecs. Production dense storage is filled by DCT directly and returns spans without the prototype's pack/copy passes; these counters do not measure production work or speed. Rejected capacity rows retain accepted-prefix counters and all attempted rental lengths",
            capacityPolicy = "Repeat all three completed components of four declared real cases, plus an adversarial full-width grayscale set, by the largest integral repetition fitting the production 64 MiB summed bucket budget, then by one more repetition; one shared prototype cap across components, actual accepted/rejected rentals and 129 sampled blocks per admitted component",
            sources = sources.Select(source => new { source.Name, source.Image.Width, source.Image.Height, pixelsSha256 = Hash(source.Image.Pixels) }),
            notMeasured = "Latency, allocated bytes, managed object/array headers, source and construction overlap, peak memory, native acceptance, end-to-end compact JPEG encoding"
        });
        await Write(output, "summary.json", new { status = "incomplete", exactCoefficients = false, performanceVerdict = "not-evaluated" });
        var cases = new List<CaseResult>();
        var capacity = new List<CapacityResult>();
        foreach ((string name, PpmImage image) in sources)
        {
            foreach (float distance in new[] { 1f, 4f })
            {
                foreach (int sampling in new[] { 444, 420 })
                {
                    short[][] coefficients = Prepare(image, distance, sampling);
                    ComponentResult[] components = coefficients.Select((values, component) => Analyze(component, values)).ToArray();
                    cases.Add(new(name, image.Width, image.Height, Hash(image.Pixels), distance, sampling, components));
                    // These are storage-cap replicas, not claims about unseen large-image coefficients.
                    if (((name is "photograph" or "smooth-gradient" or "full-noise") && distance == 1 && sampling == 444)
                        || (name == "full-noise" && distance == 4 && sampling == 420))
                    {
                        capacity.AddRange(AnalyzeCapacity(name, distance, sampling, coefficients));
                        await Write(output, "capacity.json", capacity);
                    }
                    await Write(output, "cases.json", cases);
                    Console.WriteLine($"Coefficient storage: {name}, distance {distance}, {sampling}; exact block/DC/coefficient checks passed.");
                }
            }
        }
        var adversarial = new List<object>();
        foreach (int blocks in new[] { 1, 31, 32, 33, 67 })
        {
            foreach (string pattern in new[] { "zero-ac", "coefficient-63-only", "full-width" })
            {
                short[] values = Adversarial(blocks, pattern);
                adversarial.Add(new { pattern, blocks, component = Analyze(0, values) });
            }
        }
        await Write(output, "adversarial.json", adversarial);
        capacity.AddRange(AnalyzeCapacity("synthetic-full-width-grayscale", 0, 400, [Adversarial(32, "full-width")]));
        await Write(output, "capacity.json", capacity);
        int layoutComparisons = cases.Sum(result => result.Components.Sum(component => component.Layouts.Length));
        await Write(output, "summary.json", new
        {
            schemaVersion = 1,
            status = "complete",
            exactCoefficients = true,
            imageConfigurations = cases.Count,
            components = cases.Sum(result => result.Components.Length),
            layoutComparisons,
            adversarialLayoutComparisons = adversarial.Count * 5,
            capacityComparisons = capacity.Sum(result => result.Layouts.Length),
            maximumAcceptedRentalBytes = QuantizedImage.MaximumBytes,
            decision = "Retain production dense storage: rental/index overhead prevents uniform storage savings, and added packing/access work leaves latency unestablished; any additional admitted replicas establish storage capacity only",
            performanceVerdict = "not-evaluated"
        });
        Console.WriteLine($"Coefficient storage analysis passed: {layoutComparisons} exact layout comparisons; no benchmark executed.");
        return 0;
    }

    private static short[] Adversarial(int blocks, string pattern)
    {
        short[] result = new short[blocks * 64];
        for (int block = 0; block < blocks; block++)
        {
            result[block * 64] = (block & 1) == 0 ? short.MinValue : short.MaxValue;
            for (int coefficient = 1; coefficient < 64; coefficient++)
            {
                result[(block * 64) + coefficient] = pattern switch
                {
                    "zero-ac" => 0,
                    "coefficient-63-only" => coefficient == 63 ? short.MinValue : (short)0,
                    "full-width" => ((block + coefficient) & 1) == 0 ? short.MinValue : short.MaxValue,
                    _ => throw new ArgumentException("Unknown adversarial coefficient pattern.", nameof(pattern))
                };
            }
        }
        return result;
    }

    private static short[][] Prepare(PpmImage image, float distance, int samplingMode)
    {
        var sampling = JpegSamplingFactors.FromSubsampling(samplingMode == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter);
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Quantization.CreateTableForDistance(distance,
            component, sampling.MaxHorizontal, sampling.MaxVertical)).ToArray();
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables,
            sampling.MaxHorizontal, sampling.MaxVertical, grayscale: false);
        using var adaptive = new AdaptiveQuantization(image.Width, image.Height, sampling.MaxVertical);
        using QuantizedImage cache = QuantizedImage.Create(new PixelSource(image.Pixels, PixelLayout.Rgb24),
            image.Width, image.Height, image.Width * 3, sampling, tables, distance, 0, adaptive, multipliers,
            CancellationToken.None, zigZagOrdered: true, narrowZigZagKernel: KernelPreference.Original)
            ?? throw new InvalidDataException("Bounded source exceeded the existing coefficient cache limit.");
        short[][] result = new short[3][];
        for (int component = 0; component < result.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            int columns = ((image.Width * factor.Horizontal) + (sampling.MaxHorizontal * 8) - 1) / (sampling.MaxHorizontal * 8);
            int rows = ((image.Height * factor.Vertical) + (sampling.MaxVertical * 8) - 1) / (sampling.MaxVertical * 8);
            result[component] = new short[columns * rows * 64];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    cache.GetBlock(component, x, y).CopyTo(result[component].AsSpan(((y * columns) + x) * 64, 64));
                }
            }
        }
        return result;
    }

    private static ComponentResult Analyze(int component, short[] coefficients)
    {
        int blocks = coefficients.Length / 64;
        long nonzeroAc = 0;
        for (int index = 0; index < coefficients.Length; index++)
        {
            if (index % 64 != 0 && coefficients[index] != 0)
            {
                nonzeroAc++;
            }
        }
        var layouts = new List<LayoutResult>();
        foreach (CoefficientCacheLayout layout in Enum.GetValues<CoefficientCacheLayout>())
        {
            bool[] fallbackOptions = layout == CoefficientCacheLayout.Dense ? [true] : [false, true];
            foreach (bool fallback in fallbackOptions)
            {
                using CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create(coefficients, layout, fallback)
                    ?? throw new InvalidDataException("Bounded component exceeded the prototype storage limit.");
                var full = new CoefficientCacheWork();
                var dc = new CoefficientCacheWork();
                var individual = new CoefficientCacheWork();
                Check(coefficients, cache, Enumerable.Range(0, blocks), full, dc, individual);
                layouts.Add(new(layout.ToString(), fallback, cache.UsefulBytes, cache.RentedBytes,
                    cache.PayloadBytes, cache.RentedPayloadBytes, cache.IndexBytes, cache.RentedIndexBytes,
                    cache.CompactTiles, cache.DenseTiles, cache.PackWork, full, dc, individual));
            }
        }
        return new(component, blocks, nonzeroAc, HashCoefficients(coefficients), DenseRentalBytes(coefficients.Length), layouts.ToArray());
    }

    private static IEnumerable<CapacityResult> AnalyzeCapacity(string name, float distance, int sampling, short[][] source)
    {
        long PlannedDenseBytes(int count) => source.Sum(component =>
            (long)BitOperations.RoundUpToPowerOf2((ulong)component.Length * (uint)count) * sizeof(short));
        int lower = 1, upper = 2;
        while (PlannedDenseBytes(upper) <= QuantizedImage.MaximumBytes)
        {
            lower = upper;
            upper *= 2;
        }
        while (upper - lower > 1)
        {
            int middle = lower + ((upper - lower) / 2);
            if (PlannedDenseBytes(middle) <= QuantizedImage.MaximumBytes)
            {
                lower = middle;
            }
            else
            {
                upper = middle;
            }
        }
        foreach (int repetitions in new[] { lower, upper })
        {
            short[][] coefficients = source.Select(component => Repeat(component, repetitions)).ToArray();
            var components = new CapacityComponent[coefficients.Length];
            int[][] checks = new int[coefficients.Length][];
            for (int component = 0; component < coefficients.Length; component++)
            {
                int blocks = coefficients[component].Length / 64;
                checks[component] = Enumerable.Range(0, 129).Select(index => (int)((long)index * (blocks - 1) / 128)).ToArray();
                components[component] = new(component, HashCoefficients(source[component]), source[component].Length / 64,
                    blocks, HashCoefficients(coefficients[component]), DenseRentalBytes(coefficients[component].Length), checks[component].Length);
            }
            var layouts = new List<CapacityLayout>();
            foreach (CoefficientCacheLayout layout in Enum.GetValues<CoefficientCacheLayout>())
            {
                var caches = new List<CoefficientCacheRepresentation>();
                long acceptedBytes = 0, observedPayload = 0, observedIndexes = 0;
                string? rejection = null;
                try
                {
                    for (int component = 0; component < coefficients.Length; component++)
                    {
                        var payloadPool = new ObservedPool<byte>();
                        var indexPool = new ObservedPool<int>();
                        var cache = CoefficientCacheRepresentation.Create(coefficients[component], layout,
                            maximumBytes: QuantizedImage.MaximumBytes - acceptedBytes, pool: payloadPool, indexPool: indexPool);
                        observedPayload += payloadPool.RentedElements;
                        observedIndexes += indexPool.RentedElements * sizeof(int);
                        if (cache is null)
                        {
                            rejection = $"Component {component}: " + (payloadPool.RentedElements + indexPool.RentedElements == 0
                                ? "useful payload/index exceeds remaining shared cap before rental"
                                : "actual rental buckets exceed remaining shared cap; rejected rentals returned");
                            break;
                        }
                        caches.Add(cache);
                        acceptedBytes += cache.RentedBytes;
                        Check(coefficients[component], cache, checks[component], new(), new(), new());
                    }
                    layouts.Add(new(layout.ToString(), rejection is null, rejection, acceptedBytes,
                        observedPayload, observedIndexes, caches.Sum(cache => cache.UsefulBytes),
                        caches.Sum(cache => cache.CompactTiles), caches.Sum(cache => cache.DenseTiles),
                        caches.Select(cache => cache.PackWork).ToArray()));
                }
                finally
                {
                    foreach (CoefficientCacheRepresentation cache in caches)
                    {
                        cache.Dispose();
                    }
                }
            }
            long denseRental = components.Sum(component => component.ProductionDenseRentalBytes);
            if (denseRental != PlannedDenseBytes(repetitions))
            {
                throw new InvalidDataException("Observed production pool buckets differ from the planned boundary.");
            }
            yield return new(name, distance, sampling, repetitions, coefficients.Sum(component => component.LongLength * sizeof(short)),
                denseRental, denseRental <= QuantizedImage.MaximumBytes, components, layouts.ToArray());
        }
    }

    private static short[] Repeat(short[] source, int repetitions)
    {
        short[] result = new short[checked(source.Length * repetitions)];
        for (int repetition = 0; repetition < repetitions; repetition++)
        {
            source.CopyTo(result, repetition * source.Length);
        }
        return result;
    }

    private sealed class ObservedPool<T> : ArrayPool<T>
    {
        internal long RentedElements { get; private set; }

        public override T[] Rent(int minimumLength)
        {
            T[] result = Shared.Rent(minimumLength);
            this.RentedElements += result.LongLength;
            return result;
        }

        public override void Return(T[] array, bool clearArray = false) => Shared.Return(array, clearArray);
    }

    private static void Check(short[] expected, CoefficientCacheRepresentation cache, IEnumerable<int> blocks,
        CoefficientCacheWork full, CoefficientCacheWork dc, CoefficientCacheWork individual)
    {
        Span<short> scratch = stackalloc short[64];
        foreach (int block in blocks)
        {
            ReadOnlySpan<short> original = expected.AsSpan(block * 64, 64);
            cache.CopyBlock(block, scratch, full);
            if (!original.SequenceEqual(scratch) || original[0] != cache.GetDc(block, dc))
            {
                throw new InvalidDataException("A coefficient representation changed full-block or direct DC values.");
            }
            for (int coefficient = 0; coefficient < 64; coefficient++)
            {
                if (original[coefficient] != cache.GetCoefficient(block, coefficient, individual))
                {
                    throw new InvalidDataException("A coefficient representation changed an individual progressive lookup.");
                }
            }
        }
    }

    private static long DenseRentalBytes(int coefficients)
    {
        short[] rental = ArrayPool<short>.Shared.Rent(coefficients);
        try
        {
            return rental.LongLength * sizeof(short);
        }
        finally
        {
            ArrayPool<short>.Shared.Return(rental);
        }
    }

    private static string HashCoefficients(short[] coefficients)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(coefficients.AsSpan())));
        }

        byte[] bytes = new byte[coefficients.Length * sizeof(short)];
        for (int index = 0; index < coefficients.Length; index++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(index * sizeof(short)), coefficients[index]);
        }
        return Hash(bytes);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static Task Write<T>(string output, string name, T value)
        => File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions) + "\n");
}
