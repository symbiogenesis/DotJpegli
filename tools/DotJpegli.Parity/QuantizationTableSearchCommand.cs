namespace DotJpegli.Parity;

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class QuantizationTableSearchCommand
{
    private const string UpstreamRevision = "031a0077f5799a6041004267fc12b956c1f52a20";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record SearchOptions(string Input, string Output, string Native, float Distance, ChromaSubsampling Subsampling)
    {
        internal static SearchOptions Parse(string[] args)
        {
            if (args.Length != 10)
            {
                throw new ArgumentException("Usage: --search-quantization-tables --input RGB8_PPM --output NEW_DIRECTORY --native PINNED_DECODER --distance NUMBER --sampling 444|420.");
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index += 2)
            {
                if (args[index] is not ("--input" or "--output" or "--native" or "--distance" or "--sampling")
                    || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate quantization search option.");
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
            var options = new SearchOptions(Path.GetFullPath(values["--input"]), Path.GetFullPath(values["--output"]),
                Path.GetFullPath(values["--native"]), distance, sampling);
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
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-quantization-decode-" + Guid.NewGuid().ToString("N"));
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

    // Decoder injection keeps focused correctness fixtures independent of the
    // native executable. Published CLI evidence always uses the pinned decoder.
    internal static int Run(SearchOptions options, Func<byte[], PpmImage> decode, object decoderIdentity)
    {
        ValidatePaths(options);
        byte[] input = File.ReadAllBytes(options.Input);
        var image = PpmImage.Read(input);
        QuantizationTableSearch.SearchResult result = QuantizationTableSearch.Search(image, options.Distance, options.Subsampling, decode);
        var report = new
        {
            schemaVersion = 1,
            scope = "Bounded offline RGB8 to YCbCr quantization-table search; complete-JPEG ranking at decoded control PSNR and SSIM floors; no global optimum claim",
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
            runner = new
            {
                assemblySha256 = Hash(File.ReadAllBytes(typeof(QuantizationTableSearchCommand).Assembly.Location)),
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
            },
            decoder = decoderIdentity,
            settings = new
            {
                options.Distance,
                sampling = options.Subsampling == ChromaSubsampling.None ? 444 : 420,
                adaptiveQuantization = true,
                optimizeHuffmanTables = true,
                progressiveLevel = 0,
                forceBaselineQuantizers = true,
                workers = 1,
                acZigZagBands = (int[][])[[1, 5], [6, 20], [21, 63]],
                scales = new[] { .8, 1, 1.25 },
                groups = "Independent Y and shared Cb/Cr AC scales; distinct base Cb/Cr tables; DC fixed",
                shortlistWeights = new[] { .25, .5, 1, 2 },
                finalistsPerWeight = 4,
                quantizerOrder = "natural row-major frequency order"
            },
            winner = result.WinnerId,
            output = "winner.jpg",
            result.BaseTables,
            result.SourceBlockCount,
            cachedBandCostCount = result.BandCosts.Count,
            screenedCandidateCount = result.Candidates.Count,
            result.EncodeCount,
            decodeCount = result.EncodeCount,
            totalHistogramScanCount = result.EncodeCount,
            totalOutputScanCount = result.EncodeCount,
            totalEntropyPassCount = 2 * result.EncodeCount,
            costScope = "One source block DCT traversal for cached screening costs, then one complete encode and decode per finalist; logical one-scan histogram collection and emission/replay passes exclude coefficient preparation; counts are not CPU time, allocations or peak memory",
            screeningScope = "Per-band quantization squared error and estimated AC amplitude/nonzero costs are only surrogates; they omit AQ, runs, optimized Huffman assignment, stuffing and headers. Actual complete encodes and decoded metrics decide the winner",
            qualityScope = "RGB PSNR and disjoint 8x8 pixel-weighted BT.601 luma SSIM as defined by QualityMetrics; no perceptual or human visual-quality guarantee",
            result.BandCosts,
            result.Candidates,
            result.Trials
        };
        string json = JsonSerializer.Serialize(report, JsonOptions) + "\n", markdown = Report(result);
        // Finish all search, decoding and serialization before creating output.
        ValidatePaths(options);
        _ = Directory.CreateDirectory(options.Output);
        using (var output = new FileStream(Path.Combine(options.Output, "winner.jpg"), FileMode.CreateNew))
        {
            output.Write(result.Jpeg);
        }

        WriteNew(Path.Combine(options.Output, "search.json"), json);
        WriteNew(Path.Combine(options.Output, "report.md"), markdown);
        Console.WriteLine(FormattableString.Invariant($"Selected {result.WinnerId}: {result.Jpeg.Length} bytes from {result.EncodeCount} complete encodes and decodes. Output: {options.Output}"));
        return 0;
    }

    private static void ValidatePaths(SearchOptions options)
    {
        if (!File.Exists(options.Input) || Directory.Exists(options.Output) || File.Exists(options.Output))
        {
            throw new ArgumentException("The input file must exist and the output directory must not exist.");
        }
    }

    // Search's independent-decoder callback is synchronous. This one offline
    // boundary bridges the asynchronous process/pipe deadline to that callback.
#pragma warning disable RS0030 // Required synchronous Func<byte[], PpmImage> decoder boundary.
    private static string Native(string executable, string workingDirectory, string[] args)
        => NativeAsync(executable, workingDirectory, TimeSpan.FromMinutes(3), args).GetAwaiter().GetResult();
#pragma warning restore RS0030

    internal static async Task<string> NativeAsync(string executable, string workingDirectory, TimeSpan timeout, params string[] args)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        // One deadline covers process exit and both streams: a descendant can
        // retain inherited pipe handles after its parent has already exited.
        using var process = new Process { StartInfo = start };
        using var deadline = new CancellationTokenSource(timeout);
        if (!process.Start())
        {
            throw new InvalidOperationException("Native decoder process could not start.");
        }
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(deadline.Token),
            stderr = process.StandardError.ReadToEndAsync(deadline.Token);
        var completion = Task.WhenAll(process.WaitForExitAsync(deadline.Token), stdout, stderr);
        try
        {
            await completion.WaitAsync(deadline.Token);
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            Exception? terminationError = null;
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception error) when (error is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                // A process can exit between the state check and the kill call.
                // Preserve other termination failures as timeout diagnostics.
                terminationError = error;
            }
            finally
            {
                // Closing the read handles also interrupts descendants that
                // inherited them; timeout cleanup never waits for their EOF.
                process.StandardOutput.Dispose();
                process.StandardError.Dispose();
            }
            try
            {
                // Kill is asynchronous. Give the decoder a separate bounded
                // cleanup grace to release scratch files before Run removes them.
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (Exception error) when (error is TimeoutException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                terminationError ??= error;
            }
            // Closing a pipe can fault an in-flight read after the deadline
            // task has returned. Observe that cleanup failure without waiting.
            _ = completion.ContinueWith(static task => { _ = task.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            throw new TimeoutException("Native decoder exceeded its per-command process/pipe deadline.", terminationError);
        }

        string standardOutput = await stdout, standardError = await stderr;
        if (process.ExitCode != 0)
        {
            throw new InvalidDataException($"Native command failed with exit {process.ExitCode}: {standardError}");
        }

        return standardOutput;
    }

    private static string Report(QuantizationTableSearch.SearchResult result)
    {
        var text = new StringBuilder();
        _ = text.AppendLine("# Bounded offline quantization-table search");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Selected `{result.WinnerId}`: {result.Jpeg.Length} complete JPEG bytes; unchanged default-table control: {result.Trials[0].Bytes} bytes.");
        _ = text.AppendLine();
        _ = text.AppendLine("Joint Y/Cb/Cr table candidates combine independently scaled Y and paired Cb/Cr AC bands, retaining distinct Cb/Cr base tables and fixed DC. Cached per-band costs only screen candidates: AQ, run lengths, optimized Huffman tables, stuffing and headers are coupled and require complete finalist encodes. Each finalist is decoded by the same decoder and measured against the source. Only candidates reaching both control RGB PSNR and local luma SSIM floors are eligible. Minimum complete bytes wins; equal bytes keep the earlier candidate, with control first.");
        _ = text.AppendLine();
        _ = text.AppendLine(CultureInfo.InvariantCulture, $"Search cost: {result.SourceBlockCount} source block DCTs, {result.BandCosts.Count} cached band costs, {result.Candidates.Count} screened joint candidates, {result.EncodeCount} complete encodes and decodes. Each encode has one logical histogram scan and one emission/replay scan; coefficient preparation is additional. These counts do not measure CPU time, allocations or peak working memory.");
        _ = text.AppendLine();
        _ = text.AppendLine("This is a finite offline experiment with no global optimum, latency, perceptual-quality or universal compression claim. Encoder defaults and public APIs are unchanged. Run corpus or performance measurements on CI.");
        _ = text.AppendLine();
        _ = text.AppendLine("| Finalist | Complete bytes | RGB PSNR dB | Luma SSIM | Eligible |");
        _ = text.AppendLine("| --- | ---: | ---: | ---: | --- |");
        foreach (QuantizationTableSearch.Trial trial in result.Trials)
        {
            _ = text.AppendLine(CultureInfo.InvariantCulture, $"| {trial.Id} | {trial.Bytes} | {trial.Metrics.RgbPsnrDb:R} | {trial.Metrics.LumaSsim:R} | {trial.Eligible} |");
        }

        _ = text.AppendLine();
        _ = text.AppendLine("`search.json` retains all natural-order tables, cached costs, screened candidates, finalist JPEG and decoded hashes, exact sizes, quality metrics, rejections and source/encoder/decoder identities. Reproduce `winner.jpg` using its candidate tables and the recorded encoder settings; control uses the unchanged default tables. Search never requests a target byte count.");
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
