namespace DotJpegli.Automation;

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using static DotJpegli.Automation.PerformanceJson;

internal static partial class PerformanceEvidenceCommand
{
    private static readonly string[] Versions = ["10.0.14", "11.0.0-rc.3.26522.148", "net11-baseline", "net11-simd"];
    private static readonly string[] OperationNames = ["self-test", "assert-simd", "select-profiles", "compare-runtimes"];

    internal static void Run(Arguments args)
    {
        Require(OperationNames.Count(args.Has) == 1, "Select exactly one performance evidence operation.");
        if (args.Has("self-test"))
        {
            SelfTest();
        }
        else if (args.Has("assert-simd"))
        {
            IReadOnlyList<string> evidence = args.Values("evidence");
            Require(evidence.Count > 0, "SIMD checks need evidence directories.");
            bool narrow = args.Has("narrow");
            string? architecture = args.Optional("architecture");
            Require(narrow || architecture is "X64" or "ARM64", "Architecture must be X64 or ARM64.");
            foreach (string directory in evidence)
            {
                AssertSimd(Read(Path.Combine(directory, "run.json")), architecture, narrow);
            }
        }
        else if (args.Has("select-profiles"))
        {
            IReadOnlyList<string> evidence = args.Values("evidence");
            Require(evidence.Count == 1, "Profile selection needs exactly one evidence directory.");
            foreach (string profile in Profiles(evidence[0], args.Has("full")))
            {
                Console.WriteLine(profile);
            }
        }
        else if (args.Has("compare-runtimes"))
        {
            Console.Write(CompareRuntimes(
                args.Get("directory"),
                args.Optional("architecture") ?? Environment.GetEnvironmentVariable("RUNNER_ARCH"),
                args.Optional("summary") ?? Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")));
        }
        else
        {
            throw new InvalidOperationException("Select a performance evidence operation.");
        }
    }

    private static void AssertSimd(JsonNode run, string? architecture, bool narrow)
    {
        Fields(run, "environment");
        JsonNode environment = run["environment"]!;
        Fields(environment, "vector128", "vector256", "vector512", "vectorFloatWidth");
        if (narrow || architecture == "X64")
        {
            Require(True(environment["vector256"]) && False(environment["vector512"]) && Number(environment["vectorFloatWidth"]) == 8, "Expected accelerated narrow SIMD.");
        }

        if (!narrow)
        {
            Require(True(environment["vector128"]), "Expected accelerated SIMD.");
            if (architecture == "ARM64")
            {
                Require(Number(environment["vectorFloatWidth"]) == 4 && False(run["narrowApplicable"]), "Expected ARM64 four-lane analysis.");
            }
        }
    }

    private static string[] Profiles(string directory, bool full)
    {
        JsonNode cases = Read(Path.Combine(directory, "cases.json"));
        Require(cases is JsonArray { Count: > 0 }, "No performance cases to profile.");
        JsonNode[] ordered = ((JsonArray)cases).Select(item => item ?? throw new InvalidOperationException("Invalid performance case.")).OrderByDescending(item => Number(item["medianRatio"])).ToArray();
        List<JsonNode> profiles = [];
        if (full)
        {
            HashSet<string> selected = [];
            foreach (JsonNode item in ordered)
            {
                if (!selected.Add(Text(item["id"])))
                {
                    continue;
                }

                profiles.Add(item);
                if (selected.Count == 3)
                {
                    break;
                }
            }

            JsonNode sources = Read(Path.Combine(directory, "sources.json"));
            Require(sources is JsonArray { Count: > 0 }, "No source images to profile.");
            string smallest = Text(((JsonArray)sources).OrderBy(source => Number(source!["width"]) * Number(source["height"])).First()!["id"]);
            if (!selected.Contains(smallest))
            {
                JsonNode? item = Array.Find(ordered, value => Text(value["id"]) == smallest);
                Require(item is not null, "Smallest source image has no performance cases.");
                if (Number(item!["medianRatio"]) > 1.10)
                {
                    profiles.Add(item);
                }
            }
        }
        else
        {
            profiles.Add(ordered[0]);
        }

        return profiles.Select(item => string.Create(CultureInfo.InvariantCulture, $"{Text(item["id"])} {Number(item["sampling"]):G} {Number(item["progressiveLevel"]):G} {Number(item["distance"]):G}")).ToArray();
    }

    private static string CompareRuntimes(string directory, string? architecture, string? summary)
    {
        List<(JsonNode Run, JsonNode Case)> data = [];
        foreach (string version in Versions)
        {
            string path = Path.Combine(directory, "runtime-" + version);
            JsonNode run = Read(Path.Combine(path, "run.json"));
            JsonNode status = Read(Path.Combine(path, "summary.json"));
            JsonNode cases = Read(Path.Combine(path, "cases.json"));
            Require(True(status["valid"]) && cases is JsonArray { Count: 1 }, "Runtime comparison needs stable matched evidence.");
            string expected = version == "10.0.14" ? version : "11.0.0-rc.3.26522.148";
            JsonNode environment = run["environment"]!;
            ReadOnlySpan<char> expectedPrefix = expected.IndexOf('-') is int dash and >= 0 ? expected.AsSpan(0, dash) : expected;
            Require(expectedPrefix.SequenceEqual(Text(environment["runtimeVersion"])), "Unexpected runtime version.");
            string product = Text(environment["runtimeProductVersion"]);
            Require(product == expected || (expected == "10.0.14" && product.StartsWith(expected + "-servicing.", StringComparison.Ordinal)), "Unexpected runtime build.");
            Require(version == "net11-simd" ? True(environment["explicitNet11Simd"]) : False(environment["explicitNet11Simd"]), "Unexpected SIMD experiment build.");
            data.Add((run, cases[0]!));
        }

        (JsonNode left, JsonNode firstCase) = data[0];
        Require(!Equal(data[2].Run["managedEncoderSha256"], data[3].Run["managedEncoderSha256"]), "SIMD variant must change the encoder binary.");
        foreach (string field in new[] { "testedRevision", "managedEncoderSha256", "managedRunnerSha256", "nativeExecutableSha256" })
        {
            Fields(left, field);
            Fields(data[1].Run, field);
            Require(Equal(left[field], data[1].Run[field]), "Comparison binary/revision mismatch.");
        }

        foreach ((JsonNode run, JsonNode item) in data)
        {
            foreach (string field in new[] { "cpu", "affinity", "vectorFloatWidth" })
            {
                Fields(left["environment"], field);
                Fields(run["environment"], field);
                Require(Equal(left["environment"]![field], run["environment"]![field]), "Comparison host mismatch.");
            }

            Fields(run, "testedRevision", "nativeExecutableSha256");
            Fields(item, "managedJpegSha256");
            Require(Equal(left["testedRevision"], run["testedRevision"]), "Comparison source revision mismatch.");
            Require(Equal(left["nativeExecutableSha256"], run["nativeExecutableSha256"]), "Comparison native mismatch.");
            Require(Equal(firstCase["managedJpegSha256"], item["managedJpegSha256"]), "Experiment changed JPEG bytes.");
        }

        double ratio = Positive(data[1].Case["managed"]!["median"], "Managed median") / Positive(firstCase["managed"]!["median"], "Managed median");
        double simdRatio = Positive(data[3].Case["managed"]!["median"], "Managed median") / Positive(data[2].Case["managed"]!["median"], "Managed median");
        StringBuilder report = new();
        _ = report.Append("# One-case .NET runtime and portable SIMD API comparison\n\nKodak01 4:2:0 sequential distance 1, same CPU, 7 blocks/20 iterations/5 warmups. The first pair uses identical net10.0 binaries to isolate runtime changes. The second pair uses the pinned .NET 11 SDK and runtime, comparing portable Shuffle/Create lane assembly with Zip/Unzip/Concat in the DCT transpose and downsampling. Both implementations use portable APIs, with no CPU-specific intrinsic paths. All four JPEG hashes must match. No whole-corpus or migration verdict.\n\n");
        _ = report.Append(CultureInfo.InvariantCulture, $"Host: {Text(left["environment"]!["cpu"])}; architecture: {architecture}.\n\n");
        _ = report.Append("| Build/runtime | Managed median (ms) | Native median (ms) |\n| --- | ---: | ---: |\n");
        for (int index = 0; index < Versions.Length; index++)
        {
            _ = report.Append(CultureInfo.InvariantCulture, $"| {Versions[index]} | {Positive(data[index].Case["managed"]!["median"], "Managed median") / 1e6:F3} | {Positive(data[index].Case["native"]!["median"], "Native median") / 1e6:F3} |\n");
        }

        _ = report.Append(CultureInfo.InvariantCulture, $"\n.NET 11/.NET 10 managed latency ratio: **{ratio:F6}** (lower is better). JPEG hashes match.\n");
        _ = report.Append(CultureInfo.InvariantCulture, $"\nExplicit .NET 11 SIMD/baseline latency ratio on .NET 11: **{simdRatio:F6}** (lower is better). This measures this rewrite only, not all possible new API uses.\n");
        string text = report.ToString();
        File.WriteAllText(Path.Combine(directory, "runtime-comparison.md"), text);
        JsonArray versions = [];
        foreach (string version in Versions)
        {
            versions.Add(version);
        }

        Write(Path.Combine(directory, "runtime-comparison.json"), new JsonObject
        {
            ["versions"] = versions,
            ["net11OverNet10"] = ratio,
            ["explicitSimdOverNet11Baseline"] = simdRatio,
            ["scope"] = "one case, same CPU; separate runtime-only and explicit SIMD diagnostics",
        });
        if (!string.IsNullOrEmpty(summary))
        {
            File.AppendAllText(summary, text);
        }

        return text;
    }
}
