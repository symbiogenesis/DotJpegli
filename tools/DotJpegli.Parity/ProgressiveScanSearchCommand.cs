namespace DotJpegli.Parity;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class ProgressiveScanSearchCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record SearchOptions(string Input, string Output, float Distance, ChromaSubsampling Subsampling)
    {
        internal static SearchOptions Parse(string[] args)
        {
            if (args.Length != 8)
            {
                throw new ArgumentException("Usage: --search-progressive-scans --input RGB8_PPM --output NEW_DIRECTORY --distance NUMBER --sampling 444|420.");
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index += 2)
            {
                if (args[index] is not ("--input" or "--output" or "--distance" or "--sampling")
                    || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate progressive search option.");
                }
            }

            if (!float.TryParse(values["--distance"], NumberStyles.Float, CultureInfo.InvariantCulture, out float distance)
                || !float.IsFinite(distance) || distance is <= 0f or > 25f)
            {
                throw new ArgumentException("Search distance must be finite and greater than zero through 25.");
            }

            ChromaSubsampling sampling = values["--sampling"] switch
            {
                "444" => ChromaSubsampling.None,
                "420" => ChromaSubsampling.Quarter,
                _ => throw new ArgumentException("Search sampling must be 444 or 420.")
            };
            string input = Path.GetFullPath(values["--input"]), output = Path.GetFullPath(values["--output"]);
            if (!File.Exists(input) || Directory.Exists(output) || File.Exists(output))
            {
                throw new ArgumentException("The input file must exist and the output directory must not exist.");
            }

            return new(input, output, distance, sampling);
        }
    }

    internal static int Run(string[] args)
    {
        var options = SearchOptions.Parse(args);
        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        ProgressiveScanSearch.SearchResult result = ProgressiveScanSearch.Search(image.Pixels, image.Width,
            image.Height, options.Distance, options.Subsampling);
        // Search completes before publishing any output files. Existing paths are
        // refused by Parse, and CreateNew also protects files from a racing writer.
        _ = Directory.CreateDirectory(options.Output);
        using (var output = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            output.Write(result.Jpeg);
        }

        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded RGB8 to YCbCr progressive scan and Huffman-assignment search; exact complete-JPEG ranking, no global optimum claim",
            input = new
            {
                path = options.Input,
                sha256 = Hash(input),
                pixelSha256 = Hash(image.Pixels),
                image.Width,
                image.Height
            },
            encoder = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(JpegliEncoder).Assembly.Location)),
                version = typeof(JpegliEncoder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            },
            settings = new
            {
                options.Distance,
                sampling = options.Subsampling == ChromaSubsampling.None ? 444 : 420,
                adaptiveQuantization = true,
                forceBaselineQuantizers = true,
                workers = 1
            },
            winner = result.Winner.Id,
            output = "winner.jpg",
            result.EncodeCount,
            result.TotalScanCount,
            result.TotalHistogramScanCount,
            result.TotalEntropyPassCount,
            costScope = "Logical histogram collection and emission/replay scan passes across all candidates; excludes coefficient preparation and is not a CPU-time or source-traversal measurement",
            candidates = ProgressiveScanSearch.Candidates.Select((candidate, index) => new
            {
                candidate.Id,
                candidate.Script,
                candidate.HuffmanTableLimit,
                progressiveLevel = candidate.Script is null ? (int?)2 : null,
                trial = result.Trials[index]
            })
        };
        WriteNew(Path.Combine(options.Output, "search.json"), JsonSerializer.Serialize(report, JsonOptions) + "\n");
        WriteNew(Path.Combine(options.Output, "report.md"), Report(result));
        Console.WriteLine(FormattableString.Invariant($"Selected {result.Winner.Id}: {result.Jpeg.Length} bytes from {result.EncodeCount} complete encodes. Output: {options.Output}"));
        return 0;
    }

    private static string Report(ProgressiveScanSearch.SearchResult result)
    {
        var text = new StringBuilder();
        _ = text.AppendLine("# Bounded progressive scan search");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Selected `{result.Winner.Id}`: {result.Jpeg.Length} complete JPEG bytes; level-2 optimized control: {result.Trials[0].Bytes} bytes.");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Search cost: {result.EncodeCount} complete encodes, {result.TotalScanCount} output scans and {result.TotalHistogramScanCount} logical histogram scans ({result.TotalEntropyPassCount} combined collection/emission/replay passes). Coefficient preparation is additional; these counts do not measure CPU time or source traversals.");
        _ = text.AppendLine();
        _ = text.AppendLine("All candidates keep distance, sampling, AQ and quantizers fixed. Ranking includes DHT/SOS definitions, entropy padding, EOB runs and byte stuffing. Equal sizes keep the first candidate, with the existing level-2 optimized control first. Search covers a bounded family of scan scripts, standard tables and optimized table assignments with automatic or constrained clustering; it does not exhaust arbitrary assignments or claim universal compression gains.");
        _ = text.AppendLine();
        _ = text.AppendLine("| Candidate | Complete bytes | Scans | DHT bytes | Table definitions |");
        _ = text.AppendLine("| --- | ---: | ---: | ---: | ---: |");
        foreach (ProgressiveScanSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {trial.Id} | {trial.Bytes} | {trial.ScanCount} | {trial.HuffmanDefinitionBytes} | {trial.HuffmanDefinitionCount} |");
        }

        _ = text.AppendLine();
        _ = text.AppendLine("`search.json` retains source/encoder/JPEG hashes, every script, table limits, actual per-scan Huffman selectors and all candidate costs. `winner.jpg` is the selected complete output. Repeat the command with matching source and encoder hashes to reproduce the search and output.");
        return text.ToString();
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static void WriteNew(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(text);
    }
}
