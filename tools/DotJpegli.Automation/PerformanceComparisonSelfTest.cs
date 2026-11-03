namespace DotJpegli.Automation;

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using static DotJpegli.Automation.PerformanceJson;

internal static partial class PerformanceComparisonCommand
{
    private static void SelfTest()
    {
        foreach (bool boolean in new[] { true, false })
        {
            Require(Equal(JsonValue.Create(boolean), JsonValue.Create(boolean)) &&
                !Equal(JsonValue.Create(boolean), JsonValue.Create(boolean ? 1 : 0)) &&
                !Equal(JsonValue.Create(boolean ? 1 : 0), JsonValue.Create(boolean)), "Boolean/numeric JSON equality self-test failed");
        }

        string beforeRevision = new('1', 40), afterRevision = new('2', 40);
        JsonObject environment = [];
        foreach (string field in HostFields)
        {
            environment[field] = null;
        }

        environment["cpu"] = "Synthetic CPU";
        environment["affinity"] = "0";
        environment["vector128"] = true;
        environment["vector256"] = true;
        environment["vector512"] = false;
        var before = (JsonObject)JsonNode.Parse("""
            {
              "run": { "expectedCases": 1, "policy": { "blocks": 3 }, "githubRunId": "synthetic", "recordedAt": "2026-11-02T00:00:00Z" },
              "summary": { "valid": true, "mode": "diagnostic", "selectedCoverageComplete": true, "cases": 1 },
              "sources": [{ "id": "synthetic", "category": "controlled", "width": 8, "height": 8 }],
              "cases": [{ "id": "synthetic", "sampling": 420, "progressiveLevel": 0, "distance": 1, "iterations": 5,
                "blocks": [{ "allocatedManagedBytes": 2000 }, { "allocatedManagedBytes": 2200 }, { "allocatedManagedBytes": 2400 }],
                "managed": { "median": 100 }, "native": { "median": 125 } }]
            }
            """)!;
        before["run"]!["testedRevision"] = beforeRevision;
        before["summary"]!["testedRevision"] = beforeRevision;
        before["run"]!["upstreamPin"] = new string('3', 40);
        before["run"]!["environment"] = environment;
        before["sources"]![0]!["ppmSha256"] = new string('b', 64);
        before["sources"]![0]!["pixelsSha256"] = new string('c', 64);
        before["cases"]![0]!["managedJpegSha256"] = new string('d', 64);
        before["cases"]![0]!["nativeJpegSha256"] = new string('e', 64);
        foreach (string field in new[] { "managedEncoderSha256", "managedRunnerSha256", "nativeExecutableSha256", "corpusManifestSha256" })
        {
            before["run"]![field] = new string('a', 64);
        }

        var after = (JsonObject)before.DeepClone();
        after["run"]!["testedRevision"] = afterRevision;
        after["summary"]!["testedRevision"] = afterRevision;
        after["cases"]![0]!["iterations"] = 10;
        after["cases"]![0]!["managed"]!["median"] = 80;
        after["cases"]![0]!["blocks"] = JsonNode.Parse("""[{"allocatedManagedBytes":3000},{"allocatedManagedBytes":3300},{"allocatedManagedBytes":3600}]""");
        JsonObject result = Compare(before, after, beforeRevision, afterRevision);
        JsonNode item = result["comparisons"]![0]!;
        Require(Number(item["managedRatio"]) == 0.8 && Number(item["nativeRatio"]) == 1 &&
            Number(item["baselineAllocatedBytesPerEncode"]) == 440 && Number(item["currentAllocatedBytesPerEncode"]) == 330, "Allocation/ratio normalization self-test failed");
        Action<JsonObject>[] mutations =
        [
            e => e["summary"]!["valid"] = false,
            e => e["summary"]!["selectedCoverageComplete"] = false,
            e => e["run"]!["policy"]!["blocks"] = 4,
            e => e["run"]!["testedRevision"] = new string('0', 40),
            e => e["run"]!["nativeExecutableSha256"] = new string('f', 64),
            e => e["run"]!["corpusManifestSha256"] = new string('f', 64),
            e => e["run"]!["githubRunId"] = "different",
            e => e["run"]!["environment"]!["instrumentation"] = "profiler",
            e => e["sources"]![0]!["pixelsSha256"] = new string('f', 64),
            e => e["cases"]![0]!["managedJpegSha256"] = new string('f', 64),
            e => e["cases"]![0]!["nativeJpegSha256"] = new string('f', 64),
            e => ((JsonArray)e["cases"]!).Add(e["cases"]![0]!.DeepClone()),
            e => e["cases"]![0]!["iterations"] = 0,
            e => e["cases"]![0]!["blocks"]![0]!["allocatedManagedBytes"] = -1,
            e => e["run"]!["environment"]!["vector128"] = false,
            e => e["run"]!["environment"]!["vector128"] = 1,
            e => e["run"]!["environment"]!["vector256"] = false,
            e => e["run"]!["environment"]!["vector512"] = true,
            e => e["run"]!["environment"]!["affinity"] = "0-1",
            e => e["run"]!["managedEncoderSha256"] = new string('A', 64),
            e => e["cases"]![0]!["distance"] = true,
            e => e["cases"]![0]!["sampling"] = JsonNode.Parse("420.0"),
            e => e["cases"]![0]!["managed"]!["median"] = "80",
            // JSON cannot encode NaN/Infinity. Reject their string representation as well as numeric overflow.
            e => e["cases"]![0]!["managed"]!["median"] = "NaN",
            e => e["cases"]![0]!["managed"]!["median"] = JsonNode.Parse("1e999"),
            e => e["cases"]![0]!["blocks"]![0]!["allocatedManagedBytes"] = false,
            e => e["cases"]![0]!["blocks"] = new JsonArray(e["cases"]![0]!["blocks"]![0]!.DeepClone()),
            e => e["summary"]!["valid"] = 1,
            e => e["sources"]![0]!["id"] = "Synthetic",
            e => ((JsonArray)e["sources"]!).Add(e["sources"]![0]!.DeepClone()),
            e => ((JsonObject)e["run"]!["environment"]!).Remove("cpu"),
        ];
        foreach (Action<JsonObject> mutate in mutations)
        {
            var invalid = (JsonObject)after.DeepClone();
            mutate(invalid);
            Rejected(() => Compare(before, invalid, beforeRevision, afterRevision), "Mismatch rejection self-test failed");
        }

        var equivalent = (JsonObject)after.DeepClone();
        equivalent["cases"]![0]!["distance"] = JsonNode.Parse("1.0");
        before["run"]!["policy"]!["warmups"] = 5;
        equivalent["run"]!["policy"] = JsonNode.Parse("""{"warmups":5,"blocks":3}""");
        _ = Compare(before, equivalent, beforeRevision, afterRevision);

        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string testRoot = Path.Combine(temporaryRoot, "jpegli-comparator-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + "[synthetic]");
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        try
        {
            _ = Directory.CreateDirectory(testRoot);
            foreach (string name in new[] { "run", "summary", "cases", "sources" })
            {
                File.WriteAllText(Path.Combine(testRoot, name + ".json"), after[name]!.ToJsonString(Indented), new UTF8Encoding(true));
            }

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            string summary = Path.Combine(testRoot, "summary.md");
            File.WriteAllText(summary, "Existing summary\n");
            string relativeRoot = Path.GetRelativePath(Directory.GetCurrentDirectory(), testRoot);
            Require(Equal(ReadEvidence(relativeRoot), after), "BOM/array/timestamp evidence reading self-test failed");
            string report = Save(result, Path.Combine(relativeRoot, "reports", "comparison"), Path.Combine(relativeRoot, "summary.md"));
            Require(report.Contains("0.800000", StringComparison.Ordinal) && report.Contains("440.0", StringComparison.Ordinal), "Invariant report number formatting self-test failed");
            Require(File.ReadAllText(summary) == "Existing summary\n" + report, "Summary append self-test failed");
            Require(Equal(result, Read(Path.Combine(relativeRoot, "reports", "comparison.json"))), "Report JSON schema self-test failed");
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            Require(Path.GetDirectoryName(Path.GetFullPath(testRoot))!.Equals(Path.TrimEndingDirectorySeparator(temporaryRoot), StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(testRoot).StartsWith("jpegli-comparator-", StringComparison.Ordinal), "Refusing to remove an unexpected self-test directory");
            if (Directory.Exists(testRoot))
            {
                Directory.Delete(testRoot, true);
            }
        }

        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Performance revision comparator self-test passed ({mutations.Length} rejected mismatches; synthetic data only)."));
    }
}
