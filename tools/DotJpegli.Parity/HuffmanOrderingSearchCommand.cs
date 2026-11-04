namespace DotJpegli.Parity;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class HuffmanOrderingSearchCommand
{
    private const string UpstreamRevision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record SearchOptions(string Input, string Output, string Native, float Distance,
        ChromaSubsampling Subsampling, int Progression)
    {
        internal static SearchOptions Parse(string[] args)
        {
            if (args.Length != 12)
            {
                throw new ArgumentException("Usage: --search-huffman-orderings --input RGB8_PPM --output NEW_DIRECTORY --native PINNED_DECODER --distance NUMBER --sampling 444|420 --progression 0|2.");
            }
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index += 2)
            {
                if (args[index] is not ("--input" or "--output" or "--native" or "--distance" or "--sampling" or "--progression")
                    || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate Huffman ordering search option.");
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
                _ => throw new ArgumentException("Search sampling must be 444 or 420.")
            };
            int progression = values["--progression"] switch
            {
                "0" => 0,
                "2" => 2,
                _ => throw new ArgumentException("Search progression must be 0 or 2.")
            };
            var options = new SearchOptions(Path.GetFullPath(values["--input"]), Path.GetFullPath(values["--output"]),
                Path.GetFullPath(values["--native"]), distance, sampling, progression);
            ValidatePaths(options);
            if (!File.Exists(options.Native))
            {
                throw new ArgumentException("The pinned native decoder executable must exist.");
            }
            return options;
        }
    }

    internal static int Run(string[] args) => Run(args, Native);

    internal static int Run(string[] args, Func<string, string, string[], string> native)
    {
        ArgumentNullException.ThrowIfNull(native);
        var options = SearchOptions.Parse(args);
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-huffman-decode-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string jpegPath = Path.Combine(scratch, "candidate.jpg"), decodedPath = Path.Combine(scratch, "decoded.ppm");
        try
        {
            string version = native(options.Native, scratch, ["version"]).Trim();
            if (!version.Equals(UpstreamRevision, StringComparison.Ordinal))
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
        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        HuffmanOrderingSearch.SearchResult result = HuffmanOrderingSearch.Search(image, options.Distance,
            options.Subsampling, options.Progression, decode);
        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded offline equal-length Huffman symbol ordering; complete stuffed JPEG bytes decide; no universal gain or global optimum claim",
            input = new { path = options.Input, sha256 = Hash(input), pixelSha256 = Hash(image.Pixels), image.Width, image.Height },
            encoder = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
                version = typeof(JpegliEncoder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            },
            runner = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(HuffmanOrderingSearchCommand).Assembly.Location)),
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
            },
            decoder = decoderIdentity,
            settings = new
            {
                options.Distance,
                sampling = options.Subsampling == ChromaSubsampling.None ? 444 : 420,
                progressiveLevel = options.Progression,
                adaptiveQuantization = true,
                optimizeHuffmanTables = true,
                forceBaselineQuantizers = true,
                workers = 1
            },
            winner = result.Winner.Id,
            output = "winner.jpg",
            result.EncodeCount,
            result.DecodeCount,
            result.TotalScanCount,
            totalHistogramScanCount = result.TotalScanCount,
            totalEntropyPassCount = 2 * result.TotalScanCount,
            unchangedDecodedPixels = true,
            preservedByConstruction = "Every symbol retains its code length; histogram clustering, coefficients, quantizers and scans are unchanged. Only equal-length symbol order and corresponding canonical codes change",
            ranking = "Minimum complete JPEG length including byte stuffing, padding, markers and table definitions. Control first; retain first candidate on ties",
            costScope = "Four complete encodes and independent decodes; logical histogram and emission scan counts exclude coefficient preparation. Counts do not measure wall time, allocations or peak memory",
            candidates = HuffmanOrderingSearch.Candidates.Select((candidate, index) => new
            {
                candidate.Id,
                order = candidate.Order.ToString(),
                trial = result.Trials[index]
            })
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
        var text = new StringBuilder("# Bounded Huffman symbol-ordering search\n\n");
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Selected `{result.Winner.Id}`: {result.Jpeg.Length} complete JPEG bytes; control: {result.Trials[0].Bytes} bytes. Four deterministic orders were measured by file length, with the control kept on ties.");
        _ = text.AppendLine();
        _ = text.AppendLine("Each ordering preserves every symbol's code length, all quantized coefficients and the scan configuration. All four independent decodes match the control RGB pixels. Ranking includes actual stuffing and padding; this is an offline size experiment, not a speed or global-optimality claim.");
        _ = text.AppendLine();
        _ = text.AppendLine("| Candidate | Complete bytes | Scans | DHT bytes |\n| --- | ---: | ---: | ---: |");
        foreach (HuffmanOrderingSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {trial.Id} | {trial.Bytes} | {trial.ScanCount} | {trial.HuffmanDefinitionBytes} |");
        }
        _ = text.AppendLine();
        _ = text.AppendLine("search.json retains every trial's JPEG and decoded-pixel hashes, actual sizes, source/encoder/runner/decoder identities and logical pass counts. No timings are collected.");
        ValidatePaths(options);
        _ = Directory.CreateDirectory(options.Output);
        using (var output = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            output.Write(result.Jpeg);
        }
        WriteNew(Path.Combine(options.Output, "search.json"), json);
        WriteNew(Path.Combine(options.Output, "report.md"), text.ToString());
        Console.WriteLine(FormattableString.Invariant($"Selected {result.Winner.Id}: {result.Jpeg.Length} bytes from {result.EncodeCount} complete encodes and decodes. Output: {options.Output}"));
        return 0;
    }

    private static void ValidatePaths(SearchOptions options)
    {
        if (!File.Exists(options.Input) || Directory.Exists(options.Output) || File.Exists(options.Output))
        {
            throw new ArgumentException("The input file must exist and the output directory must not exist.");
        }
    }

#pragma warning disable RS0030 // Required synchronous Func<byte[], PpmImage> decoder boundary.
    private static string Native(string executable, string workingDirectory, string[] args)
        => QuantizationTableSearchCommand.NativeAsync(executable, workingDirectory, TimeSpan.FromMinutes(3), args).GetAwaiter().GetResult();
#pragma warning restore RS0030

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static void WriteNew(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(text);
    }
}
