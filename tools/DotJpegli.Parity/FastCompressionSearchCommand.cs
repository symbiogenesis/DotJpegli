namespace DotJpegli.Parity;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class FastCompressionSearchCommand
{
    private const string Revision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal sealed record SearchOptions(string Input, string Output, string Native, float Distance,
        ChromaSubsampling Sampling, int Progression)
    {
        internal static SearchOptions Parse(string[] args)
        {
            if (args.Length is not (10 or 12))
            {
                throw new ArgumentException("Usage: --search-fast-compression --input RGB8_PPM --output NEW_DIRECTORY --native PINNED_DECODER --distance NUMBER --sampling 444|420 [--progression 0|2].");
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index += 2)
            {
                if (args[index] is not ("--input" or "--output" or "--native" or "--distance" or "--sampling" or "--progression")
                    || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate fast-compression search option.");
                }
            }

            if (!float.TryParse(values["--distance"], NumberStyles.Float, CultureInfo.InvariantCulture, out float distance)
                || !float.IsFinite(distance) || distance is <= 0 or > 25)
            {
                throw new ArgumentException("Search distance must be finite and greater than zero through 25.");
            }

            ChromaSubsampling sampling = values["--sampling"] switch
            {
                "444" => ChromaSubsampling.None,
                "420" => ChromaSubsampling.Quarter,
                _ => throw new ArgumentException("Sampling must be 444 or 420.")
            };

            int progression = 0;
            if (values.TryGetValue("--progression", out string? progressionText))
            {
                progression = progressionText switch
                {
                    "0" => 0,
                    "2" => 2,
                    _ => throw new ArgumentException("Progression must be 0 or 2.")
                };
            }

            var options = new SearchOptions(Path.GetFullPath(values["--input"]), Path.GetFullPath(values["--output"]),
                Path.GetFullPath(values["--native"]), distance, sampling, progression);
            ValidatePaths(options);
            if (!File.Exists(options.Native))
            {
                throw new ArgumentException("The pinned decoder executable must exist.");
            }

            return options;
        }
    }

    internal static int Run(string[] args) => Run(args, Native);

    internal static int Run(string[] args, Func<string, string, string[], string> native)
    {
        ArgumentNullException.ThrowIfNull(native);
        var options = SearchOptions.Parse(args);
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-fast-compression-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string jpegPath = Path.Combine(scratch, "candidate.jpg"), decodedPath = Path.Combine(scratch, "decoded.ppm");
        try
        {
            string version = native(options.Native, scratch, ["version"]).Trim();
            if (!version.Equals(Revision, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Native decoder does not report the pinned upstream revision.");
            }

            PpmImage Decode(byte[] jpeg)
            {
                File.WriteAllBytes(jpegPath, jpeg);
                File.Delete(decodedPath);
                _ = native(options.Native, scratch, ["decode", jpegPath, decodedPath]);
                return PpmImage.Read(decodedPath);
            }

            return Run(options, Decode, new
            {
                implementation = "pinned jpegli RGB8, fancy upsampling, JDCT_ISLOW",
                version,
                executableSha256 = Hash(File.ReadAllBytes(options.Native))
            });
        }
        finally
        {
            File.Delete(jpegPath);
            File.Delete(decodedPath);
            Directory.Delete(scratch);
        }
    }

    internal static int Run(SearchOptions options, Func<byte[], PpmImage> decode, object decoderIdentity)
    {
        ValidatePaths(options);
        if (new FileInfo(options.Input).Length > 16L * 1024 * 1024)
        {
            throw new ArgumentException("The bounded experiment accepts PPM files up to 16 MiB including their header.", nameof(options));
        }

        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        FastCompressionSearch.SearchResult result = FastCompressionSearch.Search(
            image, options.Distance, options.Sampling, options.Progression, decode);
        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded O(1) fast compression parameter search (TailZeroRunBias, ChromaDownsamplingSharpness, ProgressiveLevel); evaluated against decoded RGB PSNR, local luma SSIM, and dotjpegli-ms-ssim-v1 floors",
            input = new { path = options.Input, sha256 = Hash(input), pixelSha256 = Hash(image.Pixels), image.Width, image.Height },
            encoder = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
                version = typeof(JpegliEncoder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            },
            runner = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(FastCompressionSearchCommand).Assembly.Location)),
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
            },
            decoder = decoderIdentity,
            settings = new
            {
                options.Distance,
                sampling = options.Sampling == ChromaSubsampling.None ? 444 : 420,
                progression = options.Progression,
                adaptiveQuantization = true,
                optimizeHuffmanTables = true,
                workers = 1,
                forceBaselineQuantizers = true
            },
            winner = result.WinnerId,
            gateWinner = result.GateWinnerId,
            output = "winner.jpg",
            encodeCount = result.Trials.Count,
            decodeCount = result.Trials.Count,
            result.ReferenceMetricValidationCount,
            perceptualObjective = new { id = MultiScaleSsim.MetricId, definition = MultiScaleSsim.Definition },
            result.Candidates,
            result.Trials
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
        string markdown = FormattableString.Invariant($"# Fast compression parameter search\n\nSelected strict winner `{result.WinnerId}` ({result.Jpeg.Length} bytes) and gate-tolerant winner `{result.GateWinnerId}` from {result.Trials.Count} complete encodes/decodes.\n");
        ValidatePaths(options);
        _ = Directory.CreateDirectory(options.Output);
        using (var output = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            output.Write(result.Jpeg);
        }

        WriteNew(Path.Combine(options.Output, "search.json"), json);
        WriteNew(Path.Combine(options.Output, "report.md"), markdown);
        Console.WriteLine(FormattableString.Invariant($"Selected {result.WinnerId} (gate: {result.GateWinnerId}): {result.Jpeg.Length} bytes. Output: {options.Output}"));
        return 0;
    }

    private static void ValidatePaths(SearchOptions options)
    {
        if (!File.Exists(options.Input) || File.Exists(options.Output) || Directory.Exists(options.Output))
        {
            throw new ArgumentException("Input must exist and the output directory must not exist.");
        }
    }

#pragma warning disable RS0030 // Required synchronous decoder callback boundary.
    private static string Native(string executable, string directory, string[] args)
        => QuantizationTableSearchCommand.NativeAsync(executable, directory, TimeSpan.FromMinutes(3), args).GetAwaiter().GetResult();
#pragma warning restore RS0030

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteNew(string path, string content)
    {
        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }
}
