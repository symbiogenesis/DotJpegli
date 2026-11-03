namespace DotJpegli.Automation;

using System.Globalization;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static DotJpegli.Automation.PerformanceJson;

internal static class PerformanceChartsCommand
{
    private const string Indigo = "#4f46e5";
    private const string Teal = "#0f766e";
    private const string Slate = "#cbd5e1";
    private const int PlotLeft = 258;
    private const int PlotWidth = 490;
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
        File.WriteAllText(Path.Combine(output, "wall-time.svg"), WallTime(timeRows, performanceIdentity, run, worstTimeRatio, True(performance["passed"])));
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

    private static string WallTime(Row[] rows, Identity identity, JsonNode run, double worst, bool passed)
    {
        double maximum = Math.Max(1.25, Math.Ceiling(rows.Max(row => row.Value) * 4) / 4);
        StringBuilder svg = Start("Encoding wall time", "Geometric mean of DotJpegli/native median wall-time ratios. Native Google Jpegli is one. All bars start at zero. Photographs and controls appear separately.", 820);
        TextAt(svg, 32, 40, "Encoding wall time", 25, "#0f172a", "700");
        TextAt(svg, 32, 68, "Less time is better · geometric mean of per-case median ratios", 15);
        Legend(svg, 32, 100, Indigo, "DotJpegli");
        Legend(svg, 170, 100, Slate, "Native Google Jpegli = 1×");
        Axis(svg, 150, 676, maximum, Enumerable.Range(0, 6).Select(index => maximum * index / 5).ToArray(), "×");
        TextAt(svg, 32, 143, "KODAK PHOTOGRAPHS", 12, "#475569", "700");
        TextAt(svg, 915, 143, "24 images · 48 cases per row", 12, anchor: "end");
        TextAt(svg, 32, 405, "DETERMINISTIC CONTROLS", 12, "#475569", "700");
        TextAt(svg, 915, 405, "8 images · 16 cases per row", 12, anchor: "end");
        for (int index = 0; index < rows.Length; index++)
        {
            Row row = rows[index];
            int y = index < 4 ? 165 + (index * 57) : 427 + ((index - 4) * 57);
            TextAt(svg, 32, y + 20, row.Label, 15, "#334155", "500");
            Bar(svg, y, 1, maximum, 10, Slate);
            Bar(svg, y + 15, row.Value, maximum, 14, Indigo);
            TextAt(svg, 772, y + 25, FormattableString.Invariant($"{row.Value:F3}×"), 16, "#312e81", "700");
            double percent = (row.Value - 1) * 100;
            TextAt(svg, 915, y + 25, FormattableString.Invariant($"{Math.Abs(percent):F1}% {(percent > 0 ? "more" : "less")}"), 13, anchor: "end");
        }

        TextAt(svg, 32, 727, FormattableString.Invariant($"Full performance parity: {(passed ? "passed" : "not established")} · worst individual case: {worst:F3}× native time"), 14, passed ? Teal : "#92400e", "600");
        TextAt(svg, 32, 752, "Distances 1 and 4 · one worker · tiered compilation disabled · 7 blocks / case", 12);
        TextAt(svg, 32, 775, Text(run["environment"]?["cpu"]) + " · " + Text(run["environment"]?["runtime"]), 12);
        Footer(svg, identity, 800);
        return Finish(svg);
    }

    private static string Compression(Row[] rows, Identity identity, double worst)
    {
        StringBuilder svg = Start("Compression at matched quality", "Relative JPEG file sizes at matched PSNR and matched SSIM, geometric means over Kodak photographs. Native Google Jpegli is one. The axis starts at zero, so near-identical compression produces near-identical bars.", 575);
        TextAt(svg, 32, 40, "Compression at matched quality", 25, "#0f172a", "700");
        TextAt(svg, 32, 68, "Smaller files are better · native Google Jpegli = 1.000000×", 15);
        Legend(svg, 32, 100, Indigo, "Matched PSNR");
        Legend(svg, 210, 100, Teal, "Matched SSIM");
        TextAt(svg, 915, 100, "24 photographs · 5 anchors / image / metric", 12, anchor: "end");
        Axis(svg, 123, 425, 1.1, [0, .25, .5, .75, 1], "×");
        Reference(svg, 123, 425, 1, 1.1);
        for (int index = 0; index < 4; index++)
        {
            int y = 145 + (index * 68);
            TextAt(svg, 32, y + 22, rows[index * 2].Label, 15, "#334155", "500");
            for (int metric = 0; metric < 2; metric++)
            {
                Row row = rows[(index * 2) + metric];
                string color = metric == 0 ? Indigo : Teal;
                Bar(svg, y + (metric * 21), row.Value, 1.1, 14, color);
                TextAt(svg, 915, y + 12 + (metric * 21), FormattableString.Invariant($"{row.Value:F6}×"), 15, color, "600", "end");
            }
        }

        TextAt(svg, 32, 477, "Nearly identical compression: the zero-based bars preserve the scale of the measured differences.", 13, "#334155");
        TextAt(svg, 32, 502, FormattableString.Invariant($"Full 1,280-pair quality matrix passed · worst matched-quality overhead, including controls: {(worst - 1) * 100:F5}%"), 13, Teal, "600");
        TextAt(svg, 32, 527, "Ten distances from 0.25 to 8 · sizes interpolated on PSNR / SSIM quality envelopes", 12);
        Footer(svg, identity, 554);
        return Finish(svg);
    }

    private static string Allocations(Row[] rows, Identity identity)
    {
        StringBuilder svg = Start("Managed allocations per encode", "Arithmetic mean allocated managed bytes per encode, shown in KiB for Kodak photographs. Native allocations were not measured. These values include the returned JPEG and do not measure retained pool capacity or peak memory.", 470);
        TextAt(svg, 32, 40, "Managed allocations per encode", 25, "#0f172a", "700");
        TextAt(svg, 32, 68, "Lower is better · DotJpegli only · native allocations were not measured", 15);
        Legend(svg, 32, 100, Indigo, "Managed KiB / encode");
        TextAt(svg, 915, 100, "24 photographs · 48 cases per row", 12, anchor: "end");
        double maximum = Math.Ceiling(rows.Max(row => row.Value) / 50) * 50;
        Axis(svg, 120, 327, maximum, Enumerable.Range(0, 6).Select(index => maximum * index / 5).ToArray(), string.Empty);
        for (int index = 0; index < rows.Length; index++)
        {
            int y = 141 + (index * 45);
            TextAt(svg, 32, y + 17, rows[index].Label, 15, "#334155", "500");
            Bar(svg, y, rows[index].Value, maximum, 22, Indigo);
            TextAt(svg, 915, y + 17, FormattableString.Invariant($"{rows[index].Value:F1} KiB"), 16, "#312e81", "700", "end");
        }

        TextAt(svg, 32, 380, "Arithmetic mean across cases of mean block allocations ÷ encodes per block. 1 KiB = 1,024 bytes.", 13);
        TextAt(svg, 32, 405, "Includes the returned JPEG; excludes warmup. Retained pool capacity and peak memory are not measured.", 13);
        Footer(svg, identity, 447);
        return Finish(svg);
    }

    private static StringBuilder Start(string title, string description, int height)
    {
        StringBuilder svg = new();
        _ = svg.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"960\" height=\"{height}\" viewBox=\"0 0 960 {height}\" role=\"img\" aria-labelledby=\"title desc\">\n");
        _ = svg.Append("  <title id=\"title\">").Append(Escape(title)).Append("</title>\n  <desc id=\"desc\">").Append(Escape(description)).Append("</desc>\n");
        _ = svg.Append(CultureInfo.InvariantCulture, $"  <rect x=\"1\" y=\"1\" width=\"958\" height=\"{height - 2}\" rx=\"16\" fill=\"#ffffff\" stroke=\"#e2e8f0\"/>\n  <g font-family=\"system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif\">\n");
        return svg;
    }

    private static void Axis(StringBuilder svg, int top, int bottom, double maximum, double[] ticks, string suffix)
    {
        foreach (double tick in ticks)
        {
            double x = PlotLeft + (PlotWidth * tick / maximum);
            _ = svg.Append(CultureInfo.InvariantCulture, $"    <line x1=\"{x:F2}\" x2=\"{x:F2}\" y1=\"{top}\" y2=\"{bottom}\" stroke=\"#e2e8f0\"/>\n");
            TextAt(svg, x, bottom + 21, tick.ToString("0.##", CultureInfo.InvariantCulture) + suffix, 12, anchor: "middle");
        }
    }

    private static void Reference(StringBuilder svg, int top, int bottom, double value, double maximum)
    {
        double x = PlotLeft + (PlotWidth * value / maximum);
        _ = svg.Append(CultureInfo.InvariantCulture, $"    <line x1=\"{x:F2}\" x2=\"{x:F2}\" y1=\"{top}\" y2=\"{bottom}\" stroke=\"#64748b\" stroke-dasharray=\"4 4\"/>\n");
    }

    private static void Bar(StringBuilder svg, int y, double value, double maximum, int height, string color) =>
        svg.Append(CultureInfo.InvariantCulture, $"    <rect x=\"{PlotLeft}\" y=\"{y}\" width=\"{PlotWidth * value / maximum:F2}\" height=\"{height}\" rx=\"3\" fill=\"{color}\"/>\n");

    private static void Legend(StringBuilder svg, int x, int y, string color, string label)
    {
        _ = svg.Append(CultureInfo.InvariantCulture, $"    <rect x=\"{x}\" y=\"{y - 10}\" width=\"14\" height=\"11\" rx=\"3\" fill=\"{color}\"/>\n");
        TextAt(svg, x + 23, y, label, 13, "#334155");
    }

    private static void TextAt(StringBuilder svg, double x, int y, string value, int size, string color = "#64748b", string weight = "400", string anchor = "start") =>
        svg.Append(CultureInfo.InvariantCulture, $"    <text x=\"{x:F2}\" y=\"{y}\" font-size=\"{size}\" fill=\"{color}\" font-weight=\"{weight}\" text-anchor=\"{anchor}\">{Escape(value)}</text>\n");

    private static void Footer(StringBuilder svg, Identity identity, int y) =>
        TextAt(svg, 32, y, $"{identity.DateUtc ?? "Date not recorded"} UTC · revision {identity.Revision[..Math.Min(7, identity.Revision.Length)]} · hosted run {identity.Run}", 12);

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
