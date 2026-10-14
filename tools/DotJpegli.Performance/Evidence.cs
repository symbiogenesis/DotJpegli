namespace DotJpegli.Performance;

// Policy is checked into source before any measurement; command-line overrides are deliberately absent.
internal sealed record Policy
{
    public int Warmups => 5;
    public int Blocks => 7;
    public int MinimumIterations => 3;
    public int MaximumIterations => 100;
    public double TargetBlockMilliseconds => 50;
    public double MinimumBlockMilliseconds => 1;
    public double MaximumRelativeMad => 0.10;
    public double MaximumGroupGeometricMeanRatio => 1.05;
    public double MaximumCaseRatio => 1.10;
    public int ManagedWorkers => 1;
}

internal sealed record SourceImage(string Id, string Category, string SourcePath, string PpmSha256, string PixelsSha256, int Width, int Height);
internal sealed record TimingStatistics(double Minimum, double P10, double Median, double P90, double Maximum, double RelativeMad);
internal sealed record BlockSample(int Block, string Order, double ManagedNsPerEncode, double NativeNsPerEncode, int Gen0Collections, int Gen1Collections, int Gen2Collections, long AllocatedManagedBytes)
{
    public double PairedRatio => ManagedNsPerEncode / NativeNsPerEncode;
}
internal sealed record CaseResult(string Id, string Category, int Sampling, int ProgressiveLevel, float Distance, int Iterations,
    double CalibrationNativeNsPerEncode, BlockSample[] Blocks, TimingStatistics Managed, TimingStatistics Native, double MedianRatio,
    long ManagedBytes, long NativeBytes, string ManagedJpegSha256, string NativeJpegSha256, string? StructureDifference, string[] InvalidReasons);
internal sealed record GroupResult(string Category, int Sampling, int ProgressiveLevel, int Cases, double GeometricMeanRatio, double WorstRatio, bool Valid, bool Passed);
internal sealed record Evaluation(bool Valid, bool Passed, GroupResult[] Groups, string[] InvalidReasons, string[] Failures);

internal static class Evidence
{
    internal static readonly float[] Distances = [1, 4];
    internal static readonly int[] Samplings = [444, 420];
    internal static readonly int[] Progressions = [0, 2];
    internal static readonly string[] ControlledIds = ["smooth-gradient", "low-contrast", "color-edges", "line-art", "structured-texture", "low-noise", "full-noise", "tiny-odd-edges"];

    public static string[] ImageIds(string category) => category == "natural"
        ? Enumerable.Range(1, 24).Select(index => $"kodim{index:00}").ToArray() : ControlledIds;

    public static double Percentile(double[] sorted, double fraction)
    {
        if (sorted.Length == 0 || sorted.Any(value => !double.IsFinite(value) || value <= 0) || fraction is < 0 or > 1)
            throw new InvalidDataException("Timing percentiles require finite positive samples and a valid probability.");
        double index = fraction * (sorted.Length - 1);
        int lower = (int)Math.Floor(index), upper = (int)Math.Ceiling(index);
        return sorted[lower] + (sorted[upper] - sorted[lower]) * (index - lower);
    }

    public static TimingStatistics Statistics(double[] values)
    {
        double[] sorted = values.Order().ToArray();
        double median = Percentile(sorted, .5);
        double[] deviations = sorted.Select(value => Math.Abs(value - median)).Order().ToArray();
        // MAD can be zero, unlike latency samples.
        double offset = .5 * (deviations.Length - 1);
        double mad = deviations[(int)Math.Floor(offset)] +
            (deviations[(int)Math.Ceiling(offset)] - deviations[(int)Math.Floor(offset)]) * (offset - Math.Floor(offset));
        return new(sorted[0], Percentile(sorted, .1), median, Percentile(sorted, .9), sorted[^1], mad / median);
    }

    public static int Iterations(double nativeNsPerEncode, Policy policy)
    {
        if (!double.IsFinite(nativeNsPerEncode) || nativeNsPerEncode <= 0)
            throw new InvalidDataException("Calibration must return a finite positive latency.");
        double required = Math.Ceiling(policy.TargetBlockMilliseconds * 1_000_000 / nativeNsPerEncode);
        return (int)Math.Clamp(required, policy.MinimumIterations, policy.MaximumIterations);
    }

    public static string[] ValidateSamples(BlockSample[] blocks, int iterations, Policy policy)
    {
        var invalid = new List<string>();
        if (iterations < policy.MinimumIterations || iterations > policy.MaximumIterations) invalid.Add("Iteration count violates fixed bounds.");
        if (blocks.Length != policy.Blocks || !blocks.Select(block => block.Block).Order().SequenceEqual(Enumerable.Range(0, policy.Blocks)))
            invalid.Add("Missing, duplicate or extra timing blocks.");
        foreach (BlockSample block in blocks)
        {
            if (block.Order is not ("native-managed" or "managed-native")) invalid.Add("Unknown measurement order.");
            foreach (var (name, value) in new[] { ("managed", block.ManagedNsPerEncode), ("native", block.NativeNsPerEncode) })
            {
                if (!double.IsFinite(value) || value <= 0) invalid.Add($"Block {block.Block}: {name} sample is nonfinite or nonpositive.");
                else if (value * iterations < policy.MinimumBlockMilliseconds * 1_000_000)
                    invalid.Add($"Block {block.Block}: {name} timer interval is shorter than the fixed 1ms minimum.");
            }
        }
        for (int block = 1; block < blocks.Length; block++)
            if (blocks[block - 1].Order == blocks[block].Order) invalid.Add("Measurement order must alternate between blocks.");
        if (invalid.Count == 0)
        {
            foreach (var (name, values) in new[] { ("managed", blocks.Select(block => block.ManagedNsPerEncode).ToArray()), ("native", blocks.Select(block => block.NativeNsPerEncode).ToArray()) })
                if (Statistics(values).RelativeMad > policy.MaximumRelativeMad)
                    invalid.Add($"{name} relative MAD exceeds the fixed 10% noise bound.");
        }
        return invalid.ToArray();
    }

    public static Evaluation Evaluate(IReadOnlyList<CaseResult> cases, Policy policy)
    {
        var invalid = new List<string>();
        var failures = new List<string>();
        var groups = new List<GroupResult>();
        if (cases.Count != 32 * Samplings.Length * Progressions.Length * Distances.Length)
            invalid.Add("Expected exactly 256 cases across 32 images and the fixed settings matrix.");
        foreach (CaseResult item in cases)
        {
            invalid.AddRange(item.InvalidReasons.Select(reason => $"{Label(item)}: {reason}"));
            invalid.AddRange(ValidateSamples(item.Blocks, item.Iterations, policy).Select(reason => $"{Label(item)}: {reason}"));
            if (item.Blocks.Length > 0 && item.Blocks.All(block => double.IsFinite(block.ManagedNsPerEncode) && block.ManagedNsPerEncode > 0 && double.IsFinite(block.NativeNsPerEncode) && block.NativeNsPerEncode > 0))
            {
                double computedRatio = Statistics(item.Blocks.Select(block => block.ManagedNsPerEncode).ToArray()).Median /
                    Statistics(item.Blocks.Select(block => block.NativeNsPerEncode).ToArray()).Median;
                if (Math.Abs(computedRatio - item.MedianRatio) > 1e-12) invalid.Add($"{Label(item)}: stored ratio contradicts raw samples.");
            }
            if (!double.IsFinite(item.MedianRatio) || item.MedianRatio <= 0) invalid.Add($"{Label(item)}: invalid ratio.");
            if (item.StructureDifference is not null) invalid.Add($"{Label(item)}: JPEG structure differs: {item.StructureDifference}");
            if (item.Category is not ("natural" or "controlled") || !ImageIds(item.Category).Contains(item.Id, StringComparer.Ordinal) ||
                !Samplings.Contains(item.Sampling) || !Progressions.Contains(item.ProgressiveLevel) || !Distances.Contains(item.Distance))
                invalid.Add($"{Label(item)}: case outside the declared matrix.");
            if (item.MedianRatio > policy.MaximumCaseRatio) failures.Add($"{Label(item)}: median latency ratio {item.MedianRatio:F6} exceeds 1.10.");
        }
        foreach (string category in new[] { "natural", "controlled" })
            foreach (int sampling in Samplings)
                foreach (int progression in Progressions)
                {
                    CaseResult[] group = cases.Where(item => item.Category == category && item.Sampling == sampling && item.ProgressiveLevel == progression).ToArray();
                    bool groupValid = group.Length == ImageIds(category).Length * Distances.Length && group.All(item =>
                        item.StructureDifference is null && item.InvalidReasons.Length == 0 && ValidateSamples(item.Blocks, item.Iterations, policy).Length == 0 &&
                        ImageIds(category).Contains(item.Id, StringComparer.Ordinal) && Distances.Contains(item.Distance) &&
                        double.IsFinite(item.MedianRatio) && item.MedianRatio > 0 &&
                        Math.Abs(Statistics(item.Blocks.Select(block => block.ManagedNsPerEncode).ToArray()).Median /
                            Statistics(item.Blocks.Select(block => block.NativeNsPerEncode).ToArray()).Median - item.MedianRatio) <= 1e-12);
                    foreach (string id in ImageIds(category))
                        foreach (float distance in Distances)
                            if (group.Count(item => item.Id == id && item.Distance == distance) != 1)
                            {
                                invalid.Add($"{category}/{sampling}/p{progression}/{id}/d{distance}: missing or duplicate case.");
                                groupValid = false;
                            }
                    if (group.Length == 0 || group.Any(item => !double.IsFinite(item.MedianRatio) || item.MedianRatio <= 0)) continue;
                    double mean = Math.Exp(group.Average(item => Math.Log(item.MedianRatio))), worst = group.Max(item => item.MedianRatio);
                    bool passed = groupValid && mean <= policy.MaximumGroupGeometricMeanRatio && worst <= policy.MaximumCaseRatio;
                    groups.Add(new(category, sampling, progression, group.Length, mean, worst, groupValid, passed));
                    if (mean > policy.MaximumGroupGeometricMeanRatio) failures.Add($"{category}/{sampling}/p{progression}: geometric mean latency ratio {mean:F6} exceeds 1.05.");
                }
        return new(invalid.Count == 0, invalid.Count == 0 && failures.Count == 0, groups.ToArray(), invalid.Distinct().ToArray(), failures.ToArray());
    }

    internal static string Label(CaseResult item) => $"{item.Id}/{item.Sampling}/p{item.ProgressiveLevel}/d{item.Distance}";
}
