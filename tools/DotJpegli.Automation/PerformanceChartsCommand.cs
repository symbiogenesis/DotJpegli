namespace DotJpegli.Automation;

using System.Globalization;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static DotJpegli.Automation.PerformanceJson;

internal static class PerformanceChartsCommand
{
    private const string Ink = "var(--ink)";
    private const string Muted = "var(--muted)";
    private const string Teal = "var(--teal)";
    private const string Amber = "var(--amber)";
    private static readonly JsonSerializerOptions MetricsOptions = new(Indented) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, NewLine = "\n" };
    private static readonly (int Sampling, int Progression, string Label)[] Modes =
    [
        (444, 0, "Sequential · 4:4:4"), (420, 0, "Sequential · 4:2:0"),
        (444, 2, "Progressive · 4:4:4"), (420, 2, "Progressive · 4:2:0"),
    ];

    internal static void Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            SelfTest();
            return;
        }

        string performanceDirectory = args.Get("performance"), qualityDirectory = args.Get("quality");
        JsonNode performance = Read(Path.Combine(performanceDirectory, "summary.json"));
        JsonNode run = Read(Path.Combine(performanceDirectory, "run.json"));
        JsonNode[] cases = Items(Read(Path.Combine(performanceDirectory, "cases.json")));
        JsonNode quality = Read(Path.Combine(qualityDirectory, "summary.json"));
        JsonNode[] qualityCases = Items(Read(Path.Combine(qualityDirectory, "cases.json")));
        JsonNode[] matches = Items(Read(Path.Combine(qualityDirectory, "matched-quality.json")));
        ValidatePerformance(performance, run, cases);
        ValidateQuality(quality, qualityCases, matches);
        Require(Equal(performance["upstreamPin"], quality["upstreamRevision"]), "Chart sources must compare the same upstream revision.");

        Require(DateTimeOffset.TryParse(Text(run["startedAtUtc"]), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset started), "Invalid performance evidence timestamp.");
        var performanceIdentity = new Identity(Text(performance["testedRevision"]), Text(performance["githubRunId"]),
            started.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        string qualityRun = Text(quality["gitHubRunId"]);
        var qualityIdentity = new Identity(Text(quality["testedRevision"]), qualityRun,
            qualityRun == performanceIdentity.Run ? performanceIdentity.DateUtc : null);
        Row[] timeRows = TimeRows(performance, cases);
        Row[] sizeRows = SizeRows(matches);
        Row[] allocationRows = AllocationRows(cases);
        double worstTimeRatio = cases.Max(item => Number(item["medianRatio"]));
        double worstSizeRatio = matches.Max(item => Positive(item["sizeRatio"], "Matched-quality size ratio"));
        string output = args.Get("output");
        _ = Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "wall-time.svg"), WallTime(timeRows, performanceIdentity, worstTimeRatio, True(performance["passed"])));
        File.WriteAllText(Path.Combine(output, "compression.svg"), Compression(sizeRows, qualityIdentity, worstSizeRatio));
        File.WriteAllText(Path.Combine(output, "allocations.svg"), Allocations(allocationRows, performanceIdentity));
        string metrics = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            performance = performanceIdentity,
            quality = qualityIdentity,
            upstreamRevision = Text(performance["upstreamPin"]),
            performancePassed = True(performance["passed"]),
            qualityPassed = True(quality["passed"]),
            wallTime = timeRows,
            compression = sizeRows,
            allocations = allocationRows,
            worstCaseWallTimeRatio = worstTimeRatio,
            worstMatchedQualitySizeRatio = worstSizeRatio,
            allocationMethod = "Arithmetic mean across cases of mean block allocatedManagedBytes / case iterations / 1024; managed only; includes returned JPEG; excludes retained pool capacity and peak memory",
        }, MetricsOptions);
        File.WriteAllText(Path.Combine(output, "metrics.json"), metrics + "\n");
        Console.WriteLine(metrics);
    }

    private static void ValidatePerformance(JsonNode summary, JsonNode run, JsonNode[] cases)
    {
        Require(Number(summary["schemaVersion"]) == 1 && True(summary["valid"]) && True(summary["coverageComplete"]) &&
            Number(summary["cases"]) == 256 && Number(run["expectedCases"]) == 256 &&
            Items(summary["invalidReasons"]).Length == 0 && run["diagnostic"] is null && run["environment"]?["instrumentation"] is null,
            "Charts need a complete, valid, uninstrumented 256-case performance matrix.");
        foreach (string field in new[] { "testedRevision", "githubRunId", "upstreamPin", "managedEncoderSha256", "nativeExecutableSha256" })
        {
            Require(!string.IsNullOrWhiteSpace(Text(summary[field])) && Equal(summary[field], run[field]), "Performance run/summary identity mismatch.");
        }

        ValidateCoverage(cases, "id", "progressiveLevel", [1, 4]);
        foreach (JsonNode item in cases)
        {
            Require(Items(item["invalidReasons"]).Length == 0 && item["structureDifference"] is null,
                "Invalid or unmatched performance case.");
            double ratio = Positive(item["managed"]?["median"], "Managed median") / Positive(item["native"]?["median"], "Native median");
            Close(ratio, Positive(item["medianRatio"], "Case ratio"));
            double iterations = Positive(item["iterations"], "Iterations");
            Require(Integer(item["iterations"]) && iterations >= 3, "Invalid performance iteration count.");
            JsonNode[] blocks = Items(item["blocks"]);
            Require(blocks.Length == 7 && blocks.Select(block => Number(block["block"])).Order().SequenceEqual([0d, 1, 2, 3, 4, 5, 6]), "Expected seven independent blocks per performance case.");
            foreach (JsonNode block in blocks)
            {
                double allocated = Number(block["allocatedManagedBytes"]);
                Require(double.IsFinite(allocated) && allocated >= 0, "Invalid allocation total.");
                _ = Positive(block["managedNsPerEncode"], "Managed block latency");
                _ = Positive(block["nativeNsPerEncode"], "Native block latency");
            }
        }

        Require(Items(summary["groups"]).Length == 8, "Expected eight performance groups.");
    }

    private static void ValidateQuality(JsonNode summary, JsonNode[] cases, JsonNode[] matches)
    {
        Require(Number(summary["schemaVersion"]) == 1 && True(summary["passed"]) && Number(summary["cases"]) == 1280 &&
            Items(summary["failures"]).Length == 0 && Items(summary["matchedQualityExclusions"]).Length == 0,
            "Charts need a complete passing 1,280-pair quality/compression comparison.");
        ValidateCoverage(cases, "image", "progression", [.25, .5, .75, 1, 1.5, 2, 3, 4, 6, 8]);
        Require(Items(summary["matchedQualityGroups"]).Length == 16 && matches.Length == Number(summary["matchedQualityTargets"]), "Incomplete matched-quality evidence.");
        foreach (JsonNode group in Items(summary["matchedQualityGroups"]))
        {
            Require(True(group["passed"]), "A matched-quality group failed.");
            JsonNode[] selected = matches.Where(item => SameGroup(item, group, "progression") && Equal(item["metric"], group["metric"])).ToArray();
            JsonNode[] anchors = selected.Where(item => True(item["isAnchor"])).ToArray();
            int images = Text(group["category"]) == "natural" ? 24 : 8;
            Require(selected.Length == Number(group["matchedTargets"]) && anchors.Length == images * 5 &&
                anchors.GroupBy(item => Text(item["image"])).Count() == images &&
                anchors.GroupBy(item => Text(item["image"])).All(items => items.Count() == 5), "Incomplete matched-quality anchors.");
            foreach (JsonNode match in selected)
            {
                Close(Positive(match["managedBytes"], "Matched managed size") / Positive(match["nativeBytes"], "Matched native size"), Positive(match["sizeRatio"], "Matched size ratio"));
            }

            Close(GeometricMean(anchors.Select(item => Positive(item["sizeRatio"], "Matched-quality size ratio"))), Positive(group["geometricMeanSizeRatio"], "Matched-quality group ratio"));
        }
    }

    private static void ValidateCoverage(JsonNode[] cases, string idField, string progressionField, double[] distances)
    {
        Require(cases.Length == 32 * 4 * distances.Length, "Incomplete source case matrix.");
        HashSet<string> keys = [];
        foreach (JsonNode item in cases)
        {
            string id = Text(item[idField]), category = Text(item["category"]);
            double sampling = Number(item["sampling"]), progression = Number(item[progressionField]), distance = Number(item["distance"]);
            Require(category is "natural" or "controlled" && sampling is 444 or 420 && progression is 0 or 2 && distances.Contains(distance), "Unexpected source case settings.");
            Require(keys.Add(string.Create(CultureInfo.InvariantCulture, $"{id}/{category}/{sampling}/{progression}/{distance}")), "Duplicate source case.");
        }

        foreach ((string category, int count) in new[] { ("natural", 24), ("controlled", 8) })
        {
            IGrouping<string, JsonNode>[] images = cases.Where(item => Text(item["category"]) == category).GroupBy(item => Text(item[idField])).ToArray();
            Require(images.Length == count && images.All(image => image.Count() == 4 * distances.Length), "Incomplete corpus coverage.");
            if (category == "natural")
            {
                Require(images.Select(group => group.Key).Order(StringComparer.Ordinal).SequenceEqual(Enumerable.Range(1, 24).Select(index => string.Create(CultureInfo.InvariantCulture, $"kodim{index:00}"))), "Expected all 24 Kodak photographs.");
            }
        }
    }

    private static Row[] TimeRows(JsonNode summary, JsonNode[] cases)
    {
        List<Row> rows = [];
        foreach (string category in new[] { "natural", "controlled" })
        {
            foreach ((int sampling, int progression, string label) in Modes)
            {
                JsonNode[] selected = Select(cases, category, sampling, progression, "progressiveLevel");
                double value = GeometricMean(selected.Select(item => Number(item["medianRatio"])));
                JsonNode group = Items(summary["groups"]).Single(item => Text(item["category"]) == category && Number(item["sampling"]) == sampling && Number(item["progressiveLevel"]) == progression);
                Require(True(group["valid"]) && Number(group["cases"]) == selected.Length, "Performance group is incomplete or invalid.");
                Close(value, Positive(group["geometricMeanRatio"], "Performance group ratio"));
                rows.Add(new(category, label, "DotJpegli / native", value));
            }
        }

        return rows.ToArray();
    }

    private static Row[] SizeRows(JsonNode[] matches)
    {
        List<Row> rows = [];
        foreach ((int sampling, int progression, string label) in Modes)
        {
            foreach (string metric in new[] { "rgb-psnr-db", "luma-ssim" })
            {
                JsonNode[] selected = Select(matches, "natural", sampling, progression, "progression").Where(item => Text(item["metric"]) == metric && True(item["isAnchor"])).ToArray();
                rows.Add(new("natural", label, metric == "rgb-psnr-db" ? "Matched PSNR" : "Matched SSIM", GeometricMean(selected.Select(item => Number(item["sizeRatio"])))));
            }
        }

        return rows.ToArray();
    }

    private static Row[] AllocationRows(JsonNode[] cases) => Modes.Select(mode => new Row("natural", mode.Label, "DotJpegli managed KiB / encode",
        Select(cases, "natural", mode.Sampling, mode.Progression, "progressiveLevel").Average(item =>
            Items(item["blocks"]).Average(block => Number(block["allocatedManagedBytes"]) / Number(item["iterations"])) / 1024))).ToArray();

    private static string WallTime(Row[] rows, Identity identity, double worst, bool passed)
    {
        // A shared, symmetric scale shows both improvements and regressions against native.
        double extent = Math.Max(30, Math.Ceiling(rows.Max(row => Math.Abs(row.Value - 1)) * 10) * 10);
        const int zero = 516, halfWidth = 150;
        StringBuilder svg = Start("Encoding time", "Percentage change in elapsed encoding time versus native Google Jpegli. Left of zero means less time; right means more time. Geometric means of per-case median ratios, with photographs and deterministic controls shown separately.", 584);
        TextAt(svg, 32, 46, "Encoding time", 26, Ink, "650");
        TextAt(svg, 32, 76, "Compared with native Google Jpegli · less time is better", 16);
        TextAt(svg, 32, 120, "Encoding", 14, weight: "600");
        TextAt(svg, 244, 120, "Inputs", 14, weight: "600");
        TextAt(svg, zero - halfWidth, 120, "Less time", 14, Teal);
        TextAt(svg, zero, 120, "Native", 14, anchor: "middle");
        TextAt(svg, zero + halfWidth, 120, "More time", 14, Amber, anchor: "end");
        foreach (int step in new[] { -2, -1, 0, 1, 2 })
        {
            double x = zero + (step * halfWidth / 2d);
            Line(svg, x, 140, x, 492, step == 0 ? "var(--baseline)" : "var(--grid)");
            TextAt(svg, x, 516, (step * extent / 2).ToString("+0;-0;0", CultureInfo.InvariantCulture) + "%", 13, anchor: "middle");
        }

        for (int index = 0; index < Modes.Length; index++)
        {
            int y = 166 + (index * 92);
            TextAt(svg, 32, y + 7, Modes[index].Progression == 0 ? "Sequential" : "Progressive", 18, Ink, "600");
            TextAt(svg, 32, y + 32, Modes[index].Sampling == 444 ? "4:4:4" : "4:2:0", 16);
            for (int category = 0; category < 2; category++)
            {
                Row row = rows[index + (category * Modes.Length)];
                int baseline = y + (category * 34);
                double percent = (row.Value - 1) * 100;
                double width = halfWidth * Math.Abs(percent) / extent;
                string color = percent <= 0 ? Teal : Amber;
                TextAt(svg, 244, baseline + 5, category == 0 ? "Photos" : "Controls", 16);
                Rectangle(svg, percent < 0 ? zero - width : zero, baseline - 8, width, 16, color);
                TextAt(svg, 848, baseline + 5, TimeChange(row.Value), 18, color, "600", "end");
            }

            if (index < Modes.Length - 1)
            {
                Line(svg, 32, y + 60, 848, y + 60);
            }
        }

        TextAt(svg, 32, 546, $"Full performance parity: {(passed ? "passed" : "not established")} · worst case: {TimeChange(worst)}", 14);
        Footer(svg, identity, 568);
        return Finish(svg);
    }

    private static string Compression(Row[] rows, Identity identity, double worst)
    {
        double difference = rows.Max(row => Math.Abs(row.Value - 1)) * 100;
        // Round up so the compact headline remains an upper bound on every photo group mean.
        double bound = Math.Ceiling(difference * 1000) / 1000;
        StringBuilder svg = Start("JPEG file size at matched quality", "Maximum absolute percentage difference in JPEG file size versus native Google Jpegli among photograph group means at matched PSNR and SSIM, rounded up to 0.001 percentage points. Geometric means across 24 Kodak photographs, with five matched-quality anchors per image and metric. The worst individual matched-quality overhead, including controls, is shown separately.", 264);
        TextAt(svg, 32, 46, "JPEG file size", 26, Ink, "650");
        TextAt(svg, 32, 76, "Compared with native Google Jpegli at matched quality", 16);
        TextAt(svg, 32, 144, "≤" + bound.ToString("0.###", CultureInfo.InvariantCulture) + "%", 40, Teal, "650");
        TextAt(svg, 280, 124, "file-size difference on photographs", 19, Ink, "500");
        TextAt(svg, 280, 151, "Across all four encoding modes · matched PSNR and SSIM", 15);
        Line(svg, 32, 180, 848, 180);
        TextAt(svg, 32, 207, "24 photos · five anchors per image and metric · bound on group means", 14);
        TextAt(svg, 32, 230, FormattableString.Invariant($"Full quality matrix passed · worst size overhead: {(worst - 1) * 100:F4}% (including controls)"), 14);
        Footer(svg, identity, 250);
        return Finish(svg);
    }

    private static string Allocations(Row[] rows, Identity identity)
    {
        StringBuilder svg = Start("Managed allocations", "Average managed allocations per complete encode in KiB for 24 Kodak photographs. Bars start at zero. Includes the returned JPEG; native allocations, retained pool capacity and peak memory were not measured.", 386);
        TextAt(svg, 32, 46, "Managed allocations", 26, Ink, "650");
        TextAt(svg, 32, 76, "KiB per encode · DotJpegli only · lower is better", 16);
        double maximum = Math.Max(50, Math.Ceiling(rows.Max(row => row.Value) / 50) * 50);
        const int left = 278, width = 440;
        for (int index = 0; index <= 4; index++)
        {
            double x = left + (width * index / 4d);
            Line(svg, x, 108, x, 284);
            TextAt(svg, x, 308, (maximum * index / 4).ToString("0.#", CultureInfo.InvariantCulture), 13, anchor: "middle");
        }

        for (int index = 0; index < rows.Length; index++)
        {
            int y = 125 + (index * 46);
            TextAt(svg, 32, y + 6, rows[index].Label, 17, Ink, "500");
            Rectangle(svg, left, y - 9, width * rows[index].Value / maximum, 18, Teal);
            TextAt(svg, 848, y + 6, FormattableString.Invariant($"{rows[index].Value:F1}"), 19, Ink, "600", "end");
        }

        TextAt(svg, 32, 340, "Includes JPEG output · native allocations and peak memory were not measured", 14);
        Footer(svg, identity, 370);
        return Finish(svg);
    }

    private static StringBuilder Start(string title, string description, int height)
    {
        StringBuilder svg = new();
        _ = svg.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"880\" height=\"{height}\" viewBox=\"0 0 880 {height}\" role=\"img\" aria-labelledby=\"title desc\">\n");
        _ = svg.Append("  <title id=\"title\">").Append(Escape(title)).Append("</title>\n  <desc id=\"desc\">").Append(Escape(description)).Append("</desc>\n");
        _ = svg.Append("""
              <style>
                :root { color-scheme: light dark; --bg: #ffffff; --ink: #1f2937; --muted: #576575; --grid: #e5eaf0; --baseline: #8996a6; --teal: #0b766e; --amber: #a25212; }
                @media (prefers-color-scheme: dark) {
                  :root { --bg: #0d1117; --ink: #e6edf3; --muted: #a3b1c2; --grid: #252e3a; --baseline: #738399; --teal: #5cdbca; --amber: #f2b36f; }
                }
                text { font-variant-numeric: tabular-nums; }
              </style>
            """);
        _ = svg.Append(CultureInfo.InvariantCulture, $"\n  <rect width=\"880\" height=\"{height}\" rx=\"12\" fill=\"var(--bg)\"/>\n  <g font-family=\"-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif\">\n");
        return svg;
    }

    private static void Line(StringBuilder svg, double x1, int y1, double x2, int y2, string color = "var(--grid)") =>
        svg.Append(CultureInfo.InvariantCulture, $"    <line x1=\"{x1:F2}\" y1=\"{y1}\" x2=\"{x2:F2}\" y2=\"{y2}\" stroke=\"{color}\"/>\n");

    private static void Rectangle(StringBuilder svg, double x, int y, double width, int height, string color) =>
        svg.Append(CultureInfo.InvariantCulture, $"    <rect x=\"{x:F2}\" y=\"{y}\" width=\"{width:F2}\" height=\"{height}\" rx=\"3\" fill=\"{color}\"/>\n");

    private static void TextAt(StringBuilder svg, double x, int y, string value, int size, string color = Muted, string weight = "400", string anchor = "start") =>
        svg.Append(CultureInfo.InvariantCulture, $"    <text x=\"{x:F2}\" y=\"{y}\" font-size=\"{size}\" fill=\"{color}\" font-weight=\"{weight}\" text-anchor=\"{anchor}\">{Escape(value)}</text>\n");

    private static string TimeChange(double ratio) => ratio == 1 ? "Same time" :
        FormattableString.Invariant($"{Math.Abs(ratio - 1) * 100:F1}% {(ratio > 1 ? "more" : "less")}");

    private static void Footer(StringBuilder svg, Identity identity, int y) =>
        TextAt(svg, 32, y, $"{identity.DateUtc ?? "Date not recorded"} UTC · revision {identity.Revision[..Math.Min(7, identity.Revision.Length)]} · run {identity.Run}", 12);

    private static string Finish(StringBuilder svg) => svg.Append("  </g>\n</svg>\n").ToString();

    private static string Escape(string text) => SecurityElement.Escape(text) ?? string.Empty;

    private static JsonNode[] Items(JsonNode? node) => node is JsonArray array ? array.Select(item => item ?? throw new InvalidOperationException("Null evidence item.")).ToArray() : throw new InvalidOperationException("Expected an evidence array.");

    private static JsonNode[] Select(JsonNode[] items, string category, int sampling, int progression, string progressionField) => items.Where(item => Text(item["category"]) == category && Number(item["sampling"]) == sampling && Number(item[progressionField]) == progression).ToArray();

    private static bool SameGroup(JsonNode item, JsonNode group, string progressionField) => Equal(item["category"], group["category"]) && Equal(item["sampling"], group["sampling"]) && Equal(item[progressionField], group[progressionField]);

    private static double GeometricMean(IEnumerable<double> values)
    {
        double[] samples = values.ToArray();
        Require(samples.Length > 0 && samples.All(sample => double.IsFinite(sample) && sample > 0), "Invalid geometric-mean inputs.");
        return Math.Exp(samples.Average(Math.Log));
    }

    private static void Close(double actual, double expected) => Require(double.IsFinite(actual) && Math.Abs(actual - expected) <= 1e-10 * Math.Max(1, Math.Abs(expected)), "Source statistics disagree with raw evidence.");

    private static void SelfTest()
    {
        Close(GeometricMean([.5, 2]), 1);
        Rejected(() => GeometricMean([1, double.NaN]), "Nonfinite ratios were accepted.");
        List<JsonNode> cases = [];
        foreach ((int sampling, int progression, _) in Modes)
        {
            foreach ((int iterations, int bytes) in new[] { (2, 2048), (4, 16384) })
            {
                cases.Add(new JsonObject
                {
                    ["category"] = "natural",
                    ["sampling"] = sampling,
                    ["progressiveLevel"] = progression,
                    ["iterations"] = iterations,
                    ["blocks"] = new JsonArray(new JsonObject { ["allocatedManagedBytes"] = bytes }),
                });
            }
        }

        foreach (Row row in AllocationRows(cases.ToArray()))
        {
            Close(row.Value, 2.5);
        }

        List<JsonNode> coverage = [];
        foreach (string category in new[] { "natural", "controlled" })
        {
            for (int image = 1; image <= (category == "natural" ? 24 : 8); image++)
            {
                foreach ((int sampling, int progression, _) in Modes)
                {
                    foreach (int distance in new[] { 1, 4 })
                    {
                        coverage.Add(new JsonObject
                        {
                            ["id"] = string.Create(CultureInfo.InvariantCulture, $"{(category == "natural" ? "kodim" : "control")}{image:00}"),
                            ["category"] = category,
                            ["sampling"] = sampling,
                            ["progressiveLevel"] = progression,
                            ["distance"] = distance,
                        });
                    }
                }
            }
        }

        ValidateCoverage(coverage.ToArray(), "id", "progressiveLevel", [1, 4]);
        Rejected(() => ValidateCoverage(coverage.Skip(1).ToArray(), "id", "progressiveLevel", [1, 4]), "Incomplete coverage was accepted.");
        coverage[0] = coverage[1].DeepClone();
        Rejected(() => ValidateCoverage(coverage.ToArray(), "id", "progressiveLevel", [1, 4]), "Duplicate coverage was accepted.");
        Console.WriteLine("Performance chart aggregation and coverage self-test passed (synthetic data; no benchmarks).");
    }

    private sealed record Identity(string Revision, string Run, string? DateUtc);

    private sealed record Row(string Category, string Label, string Metric, double Value);
}
