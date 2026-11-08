namespace DotJpegli.Automation;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args is ["--help"])
            {
                Console.WriteLine("DotJpegli automation commands: self-test, infer-sharp, corpus, release, cli-release, native-reference, compare-performance, performance-evidence, performance-charts.");
                return 0;
            }

            var options = new Arguments(args.AsSpan(1));
            switch (args[0])
            {
                case "self-test":
                    options.Validate([], []);
                    var selfTest = new Arguments(["--self-test"]);
                    InferSharpCommand.Run(selfTest);
                    await CorpusCommand.Run(selfTest);
                    await ReleaseCommand.Run(selfTest);
                    await CliReleaseCommand.Run(selfTest);
                    NativeReferenceCommand.Run(selfTest);
                    PerformanceComparisonCommand.Run(selfTest);
                    PerformanceEvidenceCommand.Run(selfTest);
                    PerformanceChartsCommand.Run(selfTest);
                    break;
                case "infer-sharp":
                    options.Validate(["report", "translation-log", "baseline", "framework", "source-root"], ["self-test"]);
                    InferSharpCommand.Run(options);
                    break;
                case "corpus":
                    options.Validate(["output", "source", "manifest"], ["self-test"]);
                    await CorpusCommand.Run(options);
                    break;
                case "release":
                    options.Validate(["package", "version", "revision", "first-artifacts", "second-artifacts", "hashes", "output-root", "release-directory", "summary"], ["self-test", "reproducibility"]);
                    await ReleaseCommand.Run(options);
                    break;
                case "cli-release":
                    options.Validate(["artifact-root", "rid", "smoke", "output-root", "version", "portable-binary", "installed-binary", "doc-dir", "extra-file", "original", "current", "installed-dir"], ["self-test", "assert-path-restored"], "extra-file");
                    await CliReleaseCommand.Run(options);
                    break;
                case "native-reference":
                    options.Validate(["build-directory"], ["self-test"]);
                    NativeReferenceCommand.Run(options);
                    break;
                case "compare-performance":
                    options.Validate(["baseline", "current", "baseline-revision", "current-revision", "output", "summary"], ["self-test"]);
                    PerformanceComparisonCommand.Run(options);
                    break;
                case "performance-evidence":
                    options.Validate(["evidence", "architecture", "directory", "summary"],
                        ["self-test", "assert-simd", "select-profiles", "compare-runtimes", "narrow", "full"], "evidence");
                    PerformanceEvidenceCommand.Run(options);
                    break;
                case "performance-charts":
                    options.Validate(["performance", "quality", "allocations", "output"], ["self-test"]);
                    PerformanceChartsCommand.Run(options);
                    break;
                default:
                    throw new ArgumentException($"Unknown command: {args[0]}");
            }

            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidOperationException or System.Text.Json.JsonException or System.Xml.XmlException or HttpRequestException or OperationCanceledException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
