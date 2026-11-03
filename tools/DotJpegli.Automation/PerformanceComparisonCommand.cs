namespace DotJpegli.Automation;

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static DotJpegli.Automation.PerformanceJson;

internal static partial class PerformanceComparisonCommand
{
    private static readonly string[] HostFields =
    [
        "cpu", "affinity", "os", "osArchitecture", "processArchitecture", "runtimeProductVersion",
        "encoderTargetFramework", "explicitNet11Simd", "logicalProcessors", "frequency", "isHighResolution",
        "isHardwareAccelerated", "vectorFloatWidth", "vector128", "vector256", "vector512", "serverGc",
        "gcLatencyMode", "dotnetTieredCompilation", "complusTieredCompilation", "instrumentation",
        "githubRunnerOs", "githubRunnerArchitecture",
    ];

    internal static void Run(Arguments args)
    {
        try
        {
            if (args.Has("self-test"))
            {
                SelfTest();
                return;
            }

            string beforeRevision = args.Get("baseline-revision");
            string afterRevision = args.Get("current-revision");
            Require(RevisionPattern().IsMatch(beforeRevision) && RevisionPattern().IsMatch(afterRevision), "Revisions must be full lowercase Git commit IDs");
            JsonObject result = Compare(ReadEvidence(args.Get("baseline")), ReadEvidence(args.Get("current")), beforeRevision, afterRevision);
            Console.Write(Save(result, args.Get("output"), args.Optional("summary")));
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or ArgumentException or System.Text.Json.JsonException)
        {
            throw new InvalidOperationException($"Performance revision comparison invalid: {exception.Message}", exception);
        }
    }

    private static string CaseKey(JsonNode? value)
    {
        Fields(value, "id", "sampling", "progressiveLevel", "distance");
        string id = Text(value!["id"]);
        Require(id.Length > 0, "Missing case image ID");
        Require(Integer(value["sampling"]) && Integer(value["progressiveLevel"]), "Invalid case sampling/progression");
        double distance = Positive(value["distance"], "Case distance");
        return string.Create(CultureInfo.InvariantCulture, $"{id.Length}:{id}/{Number(value["sampling"])}/{Number(value["progressiveLevel"])}/{distance:R}");
    }

    private static Dictionary<string, JsonNode> Index(JsonNode? items, Func<JsonNode, string> key, string label)
    {
        Require(items is JsonArray { Count: > 0 }, $"Missing {label}");
        Dictionary<string, JsonNode> result = [];
        foreach (JsonNode? item in (JsonArray)items!)
        {
            Require(item is not null, $"Invalid {label}");
            Require(result.TryAdd(key(item!), item!), $"Duplicate {label}");
        }

        return result;
    }

    private static void Sha256(JsonNode? value, string label) => Require(
        value?.GetValueKind() == System.Text.Json.JsonValueKind.String && Sha256Pattern().IsMatch(Text(value)), $"Invalid {label} SHA-256");

    private static double Allocated(JsonNode value, double blockCount)
    {
        Fields(value, "iterations", "blocks");
        Require(Integer(value["iterations"]) && Number(value["iterations"]) > 0, "Invalid encode iteration count");
        Require(value["blocks"] is JsonArray blocks && blocks.Count == blockCount, "Allocation blocks do not match the evidence policy");
        double total = 0, correction = 0;
        foreach (JsonNode? block in (JsonArray)value["blocks"]!)
        {
            Fields(block, "allocatedManagedBytes");
            Require(Integer(block!["allocatedManagedBytes"]) && Number(block["allocatedManagedBytes"]) >= 0, "Allocated bytes must be nonnegative integers");
            double adjusted = (Number(block["allocatedManagedBytes"]) / Number(value["iterations"])) - correction;
            double next = total + adjusted;
            correction = next - total - adjusted;
            total = next;
        }

        return total / blockCount;
    }

    private static JsonObject Compare(JsonNode before, JsonNode after, string beforeRevision, string afterRevision)
    {
        foreach ((JsonNode evidence, string revision) in new[] { (before, beforeRevision), (after, afterRevision) })
        {
            Fields(evidence, "run", "summary", "cases", "sources");
            JsonNode run = evidence["run"]!, status = evidence["summary"]!;
            Fields(status, "valid", "mode", "testedRevision", "selectedCoverageComplete", "cases");
            Fields(run, "testedRevision", "expectedCases", "environment", "policy", "githubRunId", "upstreamPin", "managedEncoderSha256", "managedRunnerSha256", "nativeExecutableSha256", "corpusManifestSha256");
            Fields(run["environment"], HostFields);
            Require(True(status["valid"]), "Both comparisons require stable matched evidence");
            Require(Equal(status["mode"], JsonValue.Create("diagnostic")), "Expected selected-case diagnostic evidence");
            Require(Equal(run["testedRevision"], JsonValue.Create(revision)) && Equal(status["testedRevision"], JsonValue.Create(revision)), "Tested revision mismatch");
            Require(Integer(run["expectedCases"]) && Number(run["expectedCases"]) > 0, "Invalid expected case count");
            Require(True(status["selectedCoverageComplete"]) && Equal(status["cases"], run["expectedCases"]), "Incomplete selected-case evidence");
            Require(run["environment"]!["instrumentation"] is null, "Instrumented timings cannot be used for this comparison");
            foreach (string field in new[] { "managedEncoderSha256", "managedRunnerSha256", "nativeExecutableSha256", "corpusManifestSha256" })
            {
                Sha256(run[field], field);
            }
        }

        JsonNode left = before["run"]!, right = after["run"]!;
        Require(Equal(left["policy"], right["policy"]), "Evidence policy mismatch");
        Fields(left["policy"], "blocks");
        JsonNode? count = left["policy"]!["blocks"];
        Require(Integer(count) && Number(count) > 0, "Invalid policy block count");
        double blockCount = Number(count);
        Require(left["githubRunId"] is not null && left["githubRunId"]!.ToJsonString() is not "\"\"" and not "false" and not "0" && Equal(left["githubRunId"], right["githubRunId"]), "Comparisons must belong to the same CI run");
        foreach (string field in new[] { "upstreamPin", "nativeExecutableSha256", "corpusManifestSha256" })
        {
            Require(Equal(left[field], right[field]), $"Reference/input mismatch: {field}");
        }

        foreach (string field in HostFields)
        {
            Require(Equal(left["environment"]![field], right["environment"]![field]), $"Host/runtime mismatch: {field}");
        }

        JsonNode? affinity = left["environment"]!["affinity"];
        string affinityText = affinity?.GetValueKind() == System.Text.Json.JsonValueKind.String ? Text(affinity) : affinity?.ToJsonString() ?? string.Empty;
        Require(AffinityPattern().IsMatch(affinityText), "Comparisons must be pinned to one CPU");
        Dictionary<string, JsonNode> old = Index(before["cases"], CaseKey, "baseline cases");
        Dictionary<string, JsonNode> current = Index(after["cases"], CaseKey, "current cases");
        Require(old.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(current.Keys) && old.Count == Number(left["expectedCases"]) && current.Count == Number(right["expectedCases"]), "Selected-case mismatch");
        static string SourceKey(JsonNode item)
        {
            Fields(item, "id");
            return Text(item["id"]);
        }

        Dictionary<string, JsonNode> oldSources = Index(before["sources"], SourceKey, "baseline sources");
        Dictionary<string, JsonNode> newSources = Index(after["sources"], SourceKey, "current sources");
        var selected = old.Values.Select(item => Text(item["id"])).ToHashSet(StringComparer.Ordinal);
        Require(oldSources.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(newSources.Keys) && selected.SetEquals(oldSources.Keys), "Selected source-image mismatch");
        foreach ((string id, JsonNode source) in oldSources)
        {
            Fields(source, "category", "width", "height", "ppmSha256", "pixelsSha256");
            Fields(newSources[id], "category", "width", "height", "ppmSha256", "pixelsSha256");
            foreach (string field in new[] { "category", "width", "height", "ppmSha256", "pixelsSha256" })
            {
                Require(Equal(source[field], newSources[id][field]), $"Source-image mismatch: {id}/{field}");
            }

            Sha256(source["ppmSha256"], $"{id}/ppmSha256");
            Sha256(source["pixelsSha256"], $"{id}/pixelsSha256");
        }

        JsonArray comparisons = [];
        foreach (JsonNode item in current.Values.OrderBy(value => Text(value["id"]), StringComparer.Ordinal).ThenBy(value => Number(value["sampling"])).ThenBy(value => Number(value["progressiveLevel"])).ThenBy(value => Number(value["distance"])))
        {
            string key = CaseKey(item);
            JsonNode previous = old[key];
            foreach (JsonNode evidenceCase in new[] { previous, item })
            {
                Fields(evidenceCase, "managedJpegSha256", "nativeJpegSha256", "managed", "native");
                Fields(evidenceCase["managed"], "median");
                Fields(evidenceCase["native"], "median");
            }

            foreach (string field in new[] { "managedJpegSha256", "nativeJpegSha256" })
            {
                Sha256(previous[field], field);
                Require(Equal(previous[field], item[field]), $"JPEG bytes changed: {key}/{field}");
            }

            double bm = Positive(previous["managed"]!["median"], "Baseline managed median");
            double am = Positive(item["managed"]!["median"], "Current managed median");
            double bn = Positive(previous["native"]!["median"], "Baseline native median");
            double an = Positive(item["native"]!["median"], "Current native median");
            comparisons.Add(new JsonObject
            {
                ["case"] = new JsonArray(item["id"]!.DeepClone(), item["sampling"]!.DeepClone(), item["progressiveLevel"]!.DeepClone(), item["distance"]!.DeepClone()),
                ["baselineManagedNs"] = bm,
                ["currentManagedNs"] = am,
                ["managedRatio"] = am / bm,
                ["nativeRatio"] = an / bn,
                ["baselineAllocatedBytesPerEncode"] = Allocated(previous, blockCount),
                ["currentAllocatedBytesPerEncode"] = Allocated(item, blockCount),
            });
        }

        return new JsonObject
        {
            ["baselineRevision"] = beforeRevision,
            ["currentRevision"] = afterRevision,
            ["githubRunId"] = left["githubRunId"]!.DeepClone(),
            ["environment"] = right["environment"]!.DeepClone(),
            ["policy"] = right["policy"]!.DeepClone(),
            ["comparisons"] = comparisons,
        };
    }

    private static string Markdown(JsonObject result)
    {
        JsonNode environment = result["environment"]!;
        StringBuilder report = new();
        report.Append("# Same-host encoder development comparison\n\n");
        report.Append(CultureInfo.InvariantCulture, $"Baseline `{Text(result["baselineRevision"])}`, current `{Text(result["currentRevision"])}`; CPU: {Text(environment["cpu"])}; affinity: {Text(environment["affinity"])}; Vector128/256/512: {True(environment["vector128"])}/{True(environment["vector256"])}/{True(environment["vector512"])}.\n\n");
        report.Append("Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.\n\n");
        report.Append("Allocated bytes are the mean per-block managed allocation total divided by encodes per block; pool capacity and peak working set are separate quantities.\n\n");
        report.Append("| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |\n| --- | ---: | ---: | ---: | ---: | ---: | ---: |\n");
        foreach (JsonNode? item in (JsonArray)result["comparisons"]!)
        {
            JsonNode identity = item!["case"]!;
            report.Append(CultureInfo.InvariantCulture, $"| {Text(identity[0])}/{Number(identity[1])}/p{Number(identity[2])}/d{Number(identity[3]):G} | {Number(item["baselineManagedNs"]) / 1e6:F3} | {Number(item["currentManagedNs"]) / 1e6:F3} | {Number(item["managedRatio"]):F6} | {Number(item["nativeRatio"]):F6} | {Number(item["baselineAllocatedBytesPerEncode"]):F1} | {Number(item["currentAllocatedBytesPerEncode"]):F1} |\n");
        }

        return report.ToString();
    }

    private static JsonObject ReadEvidence(string directory)
    {
        JsonObject evidence = [];
        foreach (string name in new[] { "run", "summary", "cases", "sources" })
        {
            evidence[name] = Read(Path.Combine(directory, name + ".json"));
        }

        return evidence;
    }

    private static string Save(JsonObject result, string prefix, string? summary)
    {
        string report = Markdown(result);
        string fullPrefix = Path.GetFullPath(prefix);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPrefix)!);
        File.WriteAllText(fullPrefix + ".md", report);
        Write(fullPrefix + ".json", result);
        if (!string.IsNullOrEmpty(summary))
        {
            File.AppendAllText(summary, report);
        }

        return report;
    }

    [GeneratedRegex("\\A[0-9a-f]{40}\\z", RegexOptions.CultureInvariant)]
    private static partial Regex RevisionPattern();

    [GeneratedRegex("\\A[0-9a-f]{64}\\z", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Pattern();

    [GeneratedRegex("\\A[0-9]+\\z", RegexOptions.CultureInvariant)]
    private static partial Regex AffinityPattern();
}
