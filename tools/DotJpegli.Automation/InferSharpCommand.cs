namespace DotJpegli.Automation;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static partial class InferSharpCommand
{
    private static readonly string[] CoverageLabels = ["Method successfully translated", "Method partially translated", "Instructions translated", "Instructions skipped"];

    public static void Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            InferSharpSelfTest.Run();
            return;
        }

        Validate(args.Get("report"), args.Get("translation-log"), args.Optional("baseline"), args.Optional("framework"),
            args.Optional("source-root") ?? Directory.GetCurrentDirectory(), Console.Out);
    }

    internal static void Validate(string reportPath, string logPath, string? baselinePath, string? framework, string sourceRoot, TextWriter output)
    {
        if (!File.Exists(reportPath))
        {
            throw new InvalidDataException($"Missing InferSharp SARIF report: {reportPath}");
        }

        using JsonDocument reportDocument = ReadJson(reportPath, "Invalid InferSharp SARIF");
        JsonElement report = reportDocument.RootElement;
        JsonElement runs = Property(report, "runs");
        if (String(Property(report, "version")) != "2.1.0" || runs.ValueKind != JsonValueKind.Array || runs.GetArrayLength() == 0)
        {
            throw new InvalidDataException("Invalid InferSharp SARIF: expected SARIF 2.1.0 with at least one run.");
        }

        ValidateCoverage(logPath, output);
        List<JsonElement> findings = [];
        foreach (JsonElement run in runs.EnumerateArray())
        {
            JsonElement results = Property(run, "results");
            if (!string.Equals(String(Property(Property(Property(run, "tool"), "driver"), "name")), "Infer", StringComparison.OrdinalIgnoreCase) ||
                results.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("Invalid InferSharp SARIF: each Infer run must contain a results array.");
            }

            foreach (JsonElement result in results.EnumerateArray())
            {
                if (result.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("Invalid InferSharp SARIF: each result must be an object.");
                }

                findings.Add(result);
            }
        }

        Dictionary<FindingIdentity, bool> reviewed = ReadBaseline(baselinePath, framework, sourceRoot);
        int unreviewed = 0;
        foreach (JsonElement finding in findings)
        {
            JsonElement locations = Property(finding, "locations");
            if (locations.ValueKind != JsonValueKind.Array || locations.GetArrayLength() == 0 ||
                Property(locations[0], "physicalLocation").ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("Invalid InferSharp SARIF: each finding requires a physical location.");
            }

            JsonElement location = Property(locations[0], "physicalLocation");
            string? rule = String(Property(finding, "ruleId"));
            string? message = String(Property(Property(finding, "message"), "text"));
            string? uri = String(Property(Property(location, "artifactLocation"), "uri"));
            if (string.IsNullOrWhiteSpace(rule) || message is null || uri is null ||
                !TryPositiveInteger(Property(Property(location, "region"), "startLine"), out long line))
            {
                throw new InvalidDataException("Invalid InferSharp SARIF: each finding requires a rule, message, source URI and positive line number.");
            }

            string path = uri.StartsWith("file:/_/", StringComparison.OrdinalIgnoreCase) ? uri[8..] : uri;
            string fingerprint = String(Property(Property(finding, "fingerprints"), "hash/v1")) ?? string.Empty;
            var identity = new FindingIdentity(rule, path, line, fingerprint);
            if (reviewed.TryGetValue(identity, out bool used) && !used)
            {
                reviewed[identity] = true;
            }
            else
            {
                unreviewed++;
                output.WriteLine($"UNREVIEWED: {rule} {path} {line} {message}");
            }
        }

        output.WriteLine($"InferSharp: {findings.Count} findings; unreviewed findings: {unreviewed}.");
        if (unreviewed > 0)
        {
            throw new InvalidDataException($"InferSharp reported {unreviewed} unreviewed finding(s). Review the uploaded text and SARIF reports.");
        }

        if (reviewed.ContainsValue(false))
        {
            throw new InvalidDataException("Stale InferSharp baseline: remove exceptions for findings no longer reported.");
        }
    }

    private static void ValidateCoverage(string logPath, TextWriter output)
    {
        if (!File.Exists(logPath))
        {
            throw new InvalidDataException($"Missing InferSharp translation log: {logPath}");
        }

        string log = File.ReadAllText(logPath);
        foreach (string label in CoverageLabels)
        {
            MatchCollection statistics = Regex.Matches(log, "^" + Regex.Escape(label) + @": (?<count>[0-9]+) \((?<percent>[0-9]+)%\)\r?$", RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (statistics.Count != 1 || !long.TryParse(statistics[0].Groups["count"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out long count))
            {
                throw new InvalidDataException($"Invalid InferSharp coverage: missing or ambiguous '{label}' statistic.");
            }

            if (label is "Method successfully translated" or "Instructions translated" && count == 0)
            {
                throw new InvalidDataException($"Empty InferSharp translation: {label} must be positive.");
            }

            output.WriteLine(statistics[0].Value.TrimEnd());
        }
    }

    private static Dictionary<FindingIdentity, bool> ReadBaseline(string? path, string? framework, string sourceRoot)
    {
        Dictionary<FindingIdentity, bool> reviewed = [];
        if (string.IsNullOrEmpty(path))
        {
            if (!string.IsNullOrEmpty(framework))
            {
                throw new InvalidDataException("Invalid InferSharp baseline: TargetFramework requires BaselinePath.");
            }

            return reviewed;
        }

        using JsonDocument baselineDocument = ReadJson(path, "Invalid InferSharp baseline");
        JsonElement baseline = baselineDocument.RootElement;
        JsonElement sources = Property(baseline, "sources");
        JsonElement entries = framework is null ? default : Property(Property(baseline, "targets"), framework);
        if (!Property(baseline, "version").TryGetInt32Value(out int version) || version != 1 ||
            !string.Equals(String(Property(baseline, "analyzer")), "InferSharp v1.5", StringComparison.OrdinalIgnoreCase) ||
            !Matches(Property(baseline, "reviewedRevision"), "^[0-9a-f]{40}$") || sources.ValueKind != JsonValueKind.Object ||
            !sources.EnumerateObject().Any() || string.IsNullOrEmpty(framework) || entries.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Invalid InferSharp baseline: expected a reviewed v1.5 baseline for this framework.");
        }

        foreach (JsonProperty source in sources.EnumerateObject())
        {
            if (!SourceIdentityPattern().IsMatch(source.Name) ||
                source.Name.Split('/').Contains("..", StringComparer.Ordinal) || !Matches(source.Value, "^[0-9a-f]{64}$"))
            {
                throw new InvalidDataException("Invalid InferSharp baseline: invalid source identity.");
            }

            string sourcePath = Path.Combine(sourceRoot, source.Name);
            if (!File.Exists(sourcePath))
            {
                throw new InvalidDataException($"InferSharp reviewed source changed or is missing: {source.Name}");
            }

            string normalized = File.ReadAllText(sourcePath).Replace("\r\n", "\n", StringComparison.Ordinal);
            string hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
            if (!string.Equals(hash, String(source.Value), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"InferSharp reviewed source changed: {source.Name}. Re-review the affected exceptions.");
            }
        }

        foreach (JsonElement entry in entries.EnumerateArray())
        {
            string? rule = String(Property(entry, "ruleId"));
            string? sourcePath = String(Property(entry, "path"));
            string? fingerprint = String(Property(entry, "fingerprint"));
            if (string.IsNullOrEmpty(rule) || string.IsNullOrEmpty(sourcePath) || !sources.TryGetProperty(sourcePath, out _) ||
                !TryPositiveInteger(Property(entry, "line"), out long line) || !Matches(Property(entry, "fingerprint"), "^[0-9a-f]{1,32}$") ||
                string.IsNullOrWhiteSpace(String(Property(entry, "reason"))))
            {
                throw new InvalidDataException("Invalid InferSharp baseline: every exception needs an exact identity, reviewed source and reason.");
            }

            if (!reviewed.TryAdd(new(rule, sourcePath, line, fingerprint!), false))
            {
                throw new InvalidDataException("Invalid InferSharp baseline: duplicate exception.");
            }
        }

        return reviewed;
    }

    private static JsonDocument ReadJson(string path, string error)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException($"{error}: {exception.Message}", exception);
        }
    }

    private static JsonElement Property(JsonElement value, string name) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out JsonElement property) ? property : default;

    private static string? String(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static bool Matches(JsonElement value, string pattern) => String(value) is string text &&
        Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static bool TryPositiveInteger(JsonElement value, out long result)
    {
        result = 0;
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out result) && result > 0;
    }

    private static bool TryGetInt32Value(this JsonElement value, out int result)
    {
        result = 0;
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out result);
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+(?:/[A-Za-z0-9_.-]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SourceIdentityPattern();

    private readonly record struct FindingIdentity(string Rule, string Path, long Line, string Fingerprint);
}
