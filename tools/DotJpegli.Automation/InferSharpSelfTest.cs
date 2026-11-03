namespace DotJpegli.Automation;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

internal static partial class InferSharpSelfTest
{
    private const string SourceText = "class Example { }\n";
    private const string Coverage = "Method successfully translated: 20 (80%)\nMethod partially translated: 5 (20%)\nInstructions translated: 100 (90%)\nInstructions skipped: 10 (10%)";

    public static void Run()
    {
        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string fixtureRoot = Path.Combine(temporaryRoot, "DotJpegli-infersharp-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(Path.Combine(fixtureRoot, "src"));
        string reportPath = Path.Combine(fixtureRoot, "report[fixture].sarif");
        string logPath = Path.Combine(fixtureRoot, "translation[fixture].txt");
        string baselinePath = Path.Combine(fixtureRoot, "baseline[fixture].json");
        string sourcePath = Path.Combine(fixtureRoot, "src/Example.cs");
        string sourceHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(SourceText)));
        int passed = 0;

        void WriteReport(JsonArray? results = null, JsonArray? runs = null) => WriteJson(reportPath, new JsonObject
        {
            ["version"] = "2.1.0",
            ["runs"] = runs ?? [RunFixture(results)],
        });

        JsonObject Baseline() => new()
        {
            ["version"] = 1,
            ["analyzer"] = "InferSharp v1.5",
            ["reviewedRevision"] = new string('a', 40),
            ["sources"] = new JsonObject { ["src/Example.cs"] = sourceHash },
            ["targets"] = new JsonObject
            {
                ["net11.0"] = new JsonArray(new JsonObject
                {
                    ["ruleId"] = "NULL_DEREFERENCE",
                    ["path"] = "src/Example.cs",
                    ["line"] = 1,
                    ["fingerprint"] = "abcdef0123456789",
                    ["reason"] = "Test fixture for an explicitly reviewed finding.",
                }),
            },
        };

        void WriteFixture()
        {
            WriteReport();
            File.WriteAllText(logPath, Coverage);
            File.WriteAllText(sourcePath, SourceText);
            WriteJson(baselinePath, Baseline());
        }

        void Check(string name, string? failure = null, bool baseline = false, bool relative = false)
        {
            using var output = new StringWriter(CultureInfo.InvariantCulture) { NewLine = "\n" };
            string? caught = null;
            try
            {
                string workingDirectory = Directory.GetCurrentDirectory();
                InferSharpCommand.Validate(relative ? Path.GetRelativePath(workingDirectory, reportPath) : reportPath,
                    relative ? Path.GetRelativePath(workingDirectory, logPath) : logPath,
                    baseline ? (relative ? Path.GetRelativePath(workingDirectory, baselinePath) : baselinePath) : null,
                    baseline ? "net11.0" : null, relative ? Path.GetRelativePath(workingDirectory, fixtureRoot) : fixtureRoot, output);
            }
            catch (InvalidDataException exception)
            {
                caught = exception.Message;
            }

            if (failure is not null)
            {
                if (caught is null || !caught.Contains(failure, StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"{name} expected failure containing '{failure}'; actual: '{caught}'.");
                }
            }
            else if (caught is not null || !SuccessOutputPattern().IsMatch(output.ToString()))
            {
                throw new InvalidDataException($"{name} unexpectedly failed: {caught ?? "Missing findings validation output."}");
            }

            passed++;
            Console.WriteLine($"PASS: {name}");
        }

        try
        {
            WriteFixture();
            Check("Clean report with nonzero translation");
            Check("Relative report and log paths resolve from the working directory", relative: true);
            WriteReport(runs: [RunFixture(), RunFixture()]);
            Check("Clean report with multiple runs");
            WriteReport([Finding()]);
            Check("Findings fail regardless of analyzer exit status", "reported 1 unreviewed finding");
            WriteReport(runs: [RunFixture(), RunFixture([Finding()])]);
            Check("Findings in later SARIF runs fail", "reported 1 unreviewed finding");

            WriteFixture();
            File.Delete(reportPath);
            Check("Missing report fails", "Missing InferSharp SARIF report");
            File.WriteAllText(reportPath, "{ invalid JSON");
            Check("Malformed JSON fails", "Invalid InferSharp SARIF");
            foreach (string invalid in new[]
            {
                /*lang=json,strict*/ "{\"version\":\"1.0.0\",\"runs\":[{\"tool\":{\"driver\":{\"name\":\"Infer\"}},\"results\":[]}]}",
                /*lang=json,strict*/ "{\"version\":\"2.1.0\",\"runs\":[]}",
                /*lang=json,strict*/ "{\"version\":\"2.1.0\",\"runs\":{}}",
                /*lang=json,strict*/ "{\"version\":\"2.1.0\"}",
            })
            {
                File.WriteAllText(reportPath, invalid);
                Check("Invalid report shape fails", "Invalid InferSharp SARIF");
            }

            foreach (string invalidRun in new[]
            {
                /*lang=json,strict*/ "{\"tool\":{\"driver\":{\"name\":\"Other\"}},\"results\":[]}",
                /*lang=json,strict*/ "{\"tool\":{\"driver\":{\"name\":\"Infer\"}}}",
                /*lang=json,strict*/ "{\"tool\":{\"driver\":{\"name\":\"Infer\"}},\"results\":null}",
                /*lang=json,strict*/ "{\"tool\":{\"driver\":{\"name\":\"Infer\"}},\"results\":{}}",
                /*lang=json,strict*/ "{\"results\":[]}",
                /*lang=json,strict*/ "{\"tool\":[{\"driver\":{\"name\":\"Infer\"}}],\"results\":[]}",
                /*lang=json,strict*/ "{\"tool\":{\"driver\":{\"name\":\"Infer\"}},\"results\":[\"invalid result\"]}",
            })
            {
                WriteReport(runs: [JsonNode.Parse(invalidRun)]);
                Check("Invalid run, tool or results fails", "Invalid InferSharp SARIF");
            }

            foreach (string field in new[] { "ruleId", "message", "locations" })
            {
                JsonObject finding = Finding();
                _ = finding.Remove(field);
                WriteReport([finding]);
                Check($"Finding without {field} fails", "Invalid InferSharp SARIF");
            }

            foreach (string invalidLocation in new[]
            {
                "{}",
                /*lang=json,strict*/ "{\"physicalLocation\":{}}",
                /*lang=json,strict*/ "{\"physicalLocation\":{\"artifactLocation\":{\"uri\":\"file:/_/src/Example.cs\"},\"region\":{\"startLine\":0}}}",
                /*lang=json,strict*/ "{\"physicalLocation\":{\"artifactLocation\":{\"uri\":\"file:/_/src/Example.cs\"},\"region\":{\"startLine\":\"1\"}}}",
            })
            {
                JsonObject finding = Finding();
                finding["locations"] = new JsonArray(JsonNode.Parse(invalidLocation));
                WriteReport([finding]);
                Check("Malformed finding location fails", "Invalid InferSharp SARIF");
            }

            WriteFixture();
            File.Delete(logPath);
            Check("Missing translation log fails", "Missing InferSharp translation log");
            File.WriteAllText(logPath, string.Empty);
            Check("Empty translation log fails", "Invalid InferSharp coverage");
            foreach (string label in new[] { "Method successfully translated", "Method partially translated", "Instructions translated", "Instructions skipped" })
            {
                string line = Regex.Match(Coverage, "^" + Regex.Escape(label) + ":.*$", RegexOptions.Multiline | RegexOptions.CultureInvariant).Value;
                File.WriteAllText(logPath, Coverage.Replace(line, string.Empty, StringComparison.Ordinal));
                Check($"Missing {label} statistic fails", "Invalid InferSharp coverage");
                File.WriteAllText(logPath, Coverage + "\n" + line);
                Check($"Duplicate {label} statistic fails", "Invalid InferSharp coverage");
            }

            foreach (string label in new[] { "Method successfully translated", "Instructions translated" })
            {
                File.WriteAllText(logPath, Regex.Replace(Coverage, "^" + Regex.Escape(label) + ":.*$", label + ": 0 (0%)", RegexOptions.Multiline | RegexOptions.CultureInvariant));
                Check($"Zero {label} fails", "Empty InferSharp translation");
            }

            File.WriteAllText(logPath, Coverage.Replace("\n", "\r\n", StringComparison.Ordinal));
            Check("CRLF translation log succeeds");

            WriteFixture();
            WriteReport([Finding()]);
            Check("Exact reviewed baseline identity succeeds", baseline: true);
            Check("Relative baseline and source root resolve from the working directory", baseline: true, relative: true);
            File.WriteAllText(sourcePath, SourceText.Replace("\n", "\r\n", StringComparison.Ordinal));
            Check("Source hashes normalize line endings", baseline: true);
            JsonObject newFinding = Finding();
            newFinding["ruleId"] = "RESOURCE_LEAK";
            WriteReport([Finding(), newFinding]);
            Check("New finding alongside a reviewed finding fails", "reported 1 unreviewed finding", baseline: true);
            WriteReport([Finding()]);

            foreach (string field in new[] { "ruleId", "path", "line", "fingerprint" })
            {
                JsonObject baseline = Baseline();
                JsonNode entry = baseline["targets"]!["net11.0"]![0]!;
                if (field == "line")
                {
                    entry[field] = 2;
                }
                else
                {
                    entry[field] = field switch { "ruleId" => "RESOURCE_LEAK", "path" => "src/Other.cs", _ => "1234" };
                }

                if (field == "path")
                {
                    baseline["sources"]!["src/Other.cs"] = sourceHash;
                    File.WriteAllText(Path.Combine(fixtureRoot, "src/Other.cs"), SourceText);
                }

                WriteJson(baselinePath, baseline);
                Check($"Changed finding {field} is unreviewed", "reported 1 unreviewed finding", baseline: true);
            }

            WriteFixture();
            Check("Stale baseline fails", "Stale InferSharp baseline", baseline: true);
            WriteReport([Finding()]);
            File.WriteAllText(sourcePath, "class Changed { }\n");
            Check("Changed reviewed source fails", "reviewed source changed", baseline: true);
            File.Delete(sourcePath);
            Check("Missing reviewed source fails", "reviewed source changed or is missing", baseline: true);

            WriteFixture();
            JsonObject duplicateBaseline = Baseline();
            var exceptions = (JsonArray)duplicateBaseline["targets"]!["net11.0"]!;
            exceptions.Add(exceptions[0]!.DeepClone());
            WriteJson(baselinePath, duplicateBaseline);
            Check("Duplicate baseline exception fails", "duplicate exception", baseline: true);
            WriteFixture();
            WriteReport([Finding(), Finding()]);
            Check("One baseline exception cannot hide duplicate findings", "reported 1 unreviewed finding", baseline: true);

            WriteFixture();
            JsonObject unexplained = Baseline();
            unexplained["targets"]!["net11.0"]![0]!["reason"] = string.Empty;
            WriteJson(baselinePath, unexplained);
            Check("Unexplained baseline exception fails", "every exception needs an exact identity", baseline: true);
            File.Delete(baselinePath);
            Check("Missing baseline fails", "Invalid InferSharp baseline", baseline: true);
            File.WriteAllText(baselinePath, "{ invalid JSON");
            Check("Malformed baseline fails", "Invalid InferSharp baseline", baseline: true);
            JsonObject missingFramework = Baseline();
            missingFramework["targets"]!["net11.0"] = null;
            WriteJson(baselinePath, missingFramework);
            Check("Baseline without selected framework fails", "Invalid InferSharp baseline", baseline: true);

            foreach (string path in new[] { "../outside.cs", "src/../../outside.cs", "/absolute.cs", "C:/absolute.cs", "src\\Example.cs" })
            {
                JsonObject baseline = Baseline();
                baseline["sources"] = new JsonObject { [path] = sourceHash };
                WriteJson(baselinePath, baseline);
                Check("Invalid source path fails", "invalid source identity", baseline: true);
            }

            Console.WriteLine($"Passed {passed} InferSharp gate checks.");
        }
        finally
        {
            RemoveFixtureDirectory(fixtureRoot, temporaryRoot);
        }
    }

    private static void RemoveFixtureDirectory(string fixtureRoot, string temporaryRoot)
    {
        // Delete only this invocation's unique directory immediately below the temporary root.
        string resolved = Path.GetFullPath(fixtureRoot);
        if (!string.Equals(Path.GetDirectoryName(resolved), Path.TrimEndingDirectorySeparator(temporaryRoot), StringComparison.Ordinal) ||
            !FixtureDirectoryPattern().IsMatch(Path.GetFileName(resolved)))
        {
            throw new InvalidDataException("Refusing to remove an unexpected fixture directory.");
        }

        Directory.Delete(resolved, recursive: true);
    }

    private static void WriteJson(string path, JsonNode value) => File.WriteAllText(path, value.ToJsonString());

    private static JsonObject RunFixture(JsonArray? results = null) => new()
    {
        ["tool"] = new JsonObject { ["driver"] = new JsonObject { ["name"] = "Infer" } },
        ["results"] = results ?? [],
    };

    private static JsonObject Finding() => new()
    {
        ["ruleId"] = "NULL_DEREFERENCE",
        ["message"] = new JsonObject { ["text"] = "Fixture finding" },
        ["fingerprints"] = new JsonObject { ["hash/v1"] = "abcdef0123456789" },
        ["locations"] = new JsonArray(new JsonObject
        {
            ["physicalLocation"] = new JsonObject
            {
                ["artifactLocation"] = new JsonObject { ["uri"] = "file:/_/src/Example.cs" },
                ["region"] = new JsonObject { ["startLine"] = 1 },
            },
        }),
    };

    [GeneratedRegex(@"^InferSharp: .*unreviewed findings: 0\.$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex SuccessOutputPattern();

    [GeneratedRegex("^DotJpegli-infersharp-[0-9a-f]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex FixtureDirectoryPattern();
}
