namespace DotJpegli.Automation;

using System.Globalization;
using System.Text.Json.Nodes;
using static DotJpegli.Automation.PerformanceJson;

internal static partial class PerformanceEvidenceCommand
{
    private static void SelfTest()
    {
        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string directory = Path.Combine(temporaryRoot, "dotjpegli-ci-helpers-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        _ = Directory.CreateDirectory(directory);
        try
        {
            JsonNode run = JsonNode.Parse("""{"environment":{"vector128":true,"vector256":true,"vector512":false,"vectorFloatWidth":8},"narrowApplicable":true}""")!;
            AssertSimd(run, "X64", false);
            AssertSimd(run, null, true);
            run["environment"]!["vector512"] = true;
            Rejected(() => AssertSimd(run, null, true), "Wide SIMD was accepted for narrow evidence.");
            run["environment"]!["vector256"] = "true";
            run["environment"]!["vector512"] = false;
            Rejected(() => AssertSimd(run, null, true), "String SIMD capability was accepted as a Boolean.");
            run["environment"] = JsonNode.Parse("""{"vector128":true,"vector256":false,"vector512":false,"vectorFloatWidth":4}""");
            run["narrowApplicable"] = false;
            AssertSimd(run, "ARM64", false);
            Rejected(() => AssertSimd(run, "X64", false), "ARM64 capabilities were accepted as X64.");
            run["narrowApplicable"] = true;
            Rejected(() => AssertSimd(run, "ARM64", false), "Applicable narrow SIMD was accepted for ARM64.");

            var cases = (JsonArray)JsonNode.Parse("""
                [
                  {"id":"a","medianRatio":5,"sampling":444,"progressiveLevel":0,"distance":1},
                  {"id":"a","medianRatio":4,"sampling":420,"progressiveLevel":2,"distance":4},
                  {"id":"b","medianRatio":3,"sampling":420,"progressiveLevel":2,"distance":4},
                  {"id":"c","medianRatio":2,"sampling":444,"progressiveLevel":0,"distance":1},
                  {"id":"tiny","medianRatio":1.11,"sampling":420,"progressiveLevel":0,"distance":1}
                ]
                """)!;
            string casePath = Path.Combine(directory, "cases.json");
            Write(casePath, cases);
            Write(Path.Combine(directory, "sources.json"), JsonNode.Parse("""[{"id":"a","width":512,"height":768},{"id":"tiny","width":1,"height":1}]""")!);
            Require(Profiles(directory, false).SequenceEqual(["a 444 0 1"], StringComparer.Ordinal), "Wrong selected-case profile.");
            Require(Profiles(directory, true).SequenceEqual(["a 444 0 1", "b 420 2 4", "c 444 0 1", "tiny 420 0 1"], StringComparer.Ordinal), "Full profiles must select distinct worst images and the failing smallest input.");
            cases[4]!["medianRatio"] = 1.10;
            Write(casePath, cases);
            Require(Profiles(directory, true).Length == 3, "Smallest passing input must not add a profile.");
            Write(casePath, new JsonArray());
            Rejected(() => Profiles(directory, false), "Empty case arrays were accepted.");
            Write(casePath, cases[0]!);
            Rejected(() => Profiles(directory, false), "A case object was accepted instead of an array.");

            for (int index = 0; index < Versions.Length; index++)
            {
                string path = Path.Combine(directory, "runtime-" + Versions[index]);
                _ = Directory.CreateDirectory(path);
                JsonObject runtime = new()
                {
                    ["testedRevision"] = "revision",
                    ["managedEncoderSha256"] = index == 3 ? "simd" : index == 2 ? "baseline" : "shared",
                    ["managedRunnerSha256"] = "runner",
                    ["nativeExecutableSha256"] = "native",
                    ["environment"] = new JsonObject
                    {
                        ["cpu"] = "synthetic CPU",
                        ["affinity"] = "2",
                        ["vectorFloatWidth"] = 8,
                        ["runtimeVersion"] = index == 0 ? "10.0.14" : "11.0.0",
                        ["runtimeProductVersion"] = index == 0 ? "10.0.14-servicing.test" : "11.0.0-rc.3.26522.148",
                        ["explicitNet11Simd"] = index == 3,
                    },
                };
                Write(Path.Combine(path, "run.json"), runtime);
                Write(Path.Combine(path, "summary.json"), new JsonObject { ["valid"] = true });
                Write(Path.Combine(path, "cases.json"), new JsonArray(new JsonObject
                {
                    ["managedJpegSha256"] = "jpeg",
                    ["managed"] = new JsonObject { ["median"] = 1000000 + (100000 * index) },
                    ["native"] = new JsonObject { ["median"] = 500000 },
                }));
            }

            string summary = Path.Combine(directory, "job-summary.md");
            _ = CompareRuntimes(directory, "X64", summary);
            JsonNode result = Read(Path.Combine(directory, "runtime-comparison.json"));
            Require(Number(result["net11OverNet10"]) == 1.1 && Math.Abs(Number(result["explicitSimdOverNet11Baseline"]) - (13.0 / 12)) < 1e-12, "Wrong runtime comparison ratios.");
            Require(File.ReadAllText(summary).Contains("| 10.0.14 | 1.000 | 0.500 |", StringComparison.Ordinal), "Missing invariant Markdown timing output.");
            string runtimeCasePath = Path.Combine(directory, "runtime-net11-simd", "cases.json");
            JsonNode runtimeCase = Read(runtimeCasePath)[0]!.DeepClone();
            Write(runtimeCasePath, runtimeCase);
            Rejected(() => CompareRuntimes(directory, "X64", null), "Runtime comparison accepted a case object instead of an array.");
            Write(runtimeCasePath, new JsonArray(runtimeCase));
            string verdictPath = Path.Combine(directory, "runtime-net11-simd", "summary.json");
            Write(verdictPath, new JsonObject { ["valid"] = "false" });
            Rejected(() => CompareRuntimes(directory, "X64", null), "Runtime comparison accepted a string evidence verdict.");
            Write(verdictPath, new JsonObject { ["valid"] = true });
            string simdPath = Path.Combine(directory, "runtime-net11-simd", "run.json");
            JsonNode simdRun = Read(simdPath);
            simdRun["environment"]!["affinity"] = "3";
            Write(simdPath, simdRun);
            Rejected(() => CompareRuntimes(directory, "X64", null), "Runtime comparison accepted a different host.");
            simdRun["environment"]!["affinity"] = "2";
            Write(simdPath, simdRun);
            runtimeCase["managedJpegSha256"] = "changed-jpeg";
            Write(runtimeCasePath, new JsonArray(runtimeCase.DeepClone()));
            Rejected(() => CompareRuntimes(directory, "X64", null), "Runtime comparison accepted changed JPEG bytes.");
            Console.WriteLine("Performance evidence synthetic checks passed.");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            Require(Path.GetDirectoryName(resolved)!.Equals(Path.TrimEndingDirectorySeparator(temporaryRoot), StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(resolved).StartsWith("dotjpegli-ci-helpers-", StringComparison.Ordinal), "Unexpected synthetic fixture cleanup path.");
            Directory.Delete(resolved, true);
        }
    }
}
