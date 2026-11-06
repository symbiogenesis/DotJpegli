namespace DotJpegli.Performance;

using System.Globalization;

internal sealed record DiagnosticSelection(string[] Images, float[] Distances, int[] Samplings, int[] Progressions, Policy Policy)
{
    public int ExpectedCases => this.Images.Length * this.Distances.Length * this.Samplings.Length * this.Progressions.Length;

    public static DiagnosticSelection Parse(IReadOnlyDictionary<string, string> values, IEnumerable<string>? additionalImageIds = null)
    {
        string[] allowed = ["--images", "--distances", "--samplings", "--progressions", "--blocks", "--iterations", "--warmups"];
        if (values.Keys.Any(key => !allowed.Contains(key, StringComparer.Ordinal)))
        {
            throw new ArgumentException("Unknown diagnostic option.");
        }

        string[] images = List(values.GetValueOrDefault("--images", "kodim01"));
        var supportedImages = new HashSet<string>([.. Evidence.ImageIds("natural"), .. Evidence.ControlledIds, .. additionalImageIds ?? []], StringComparer.Ordinal);
        if (images.Any(id => !supportedImages.Contains(id)))
        {
            throw new ArgumentException("Diagnostic images must name pinned Kodak images (kodim01..kodim24) or generated inputs supported by this mode.");
        }

        float[] distances = List(values.GetValueOrDefault("--distances", "1")).Select(value => float.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        int[] samplings = List(values.GetValueOrDefault("--samplings", "444,420")).Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        int[] progressions = List(values.GetValueOrDefault("--progressions", "0,2")).Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        if (distances.Any(value => !Evidence.Distances.Contains(value)) || samplings.Any(value => !Evidence.Samplings.Contains(value)) ||
            progressions.Any(value => !Evidence.Progressions.Contains(value)) || distances.Distinct().Count() != distances.Length ||
            samplings.Distinct().Count() != samplings.Length || progressions.Distinct().Count() != progressions.Length)
        {
            throw new ArgumentException("Diagnostic settings select from distances 1/4, samplings 444/420 and progressions 0/2, without duplicates.");
        }

        int blocks = Count(values, "--blocks", 7, 3, 7), warmups = Count(values, "--warmups", 5, 5, 10);
        int? iterations = values.ContainsKey("--iterations") ? Count(values, "--iterations", 3, 3, 100) : null;
        var result = new DiagnosticSelection(images, distances, samplings, progressions, new Policy
        {
            Blocks = blocks,
            Warmups = warmups,
            MinimumIterations = iterations ?? 3,
            MaximumIterations = iterations ?? 100
        });
        if (result.ExpectedCases > 64)
        {
            throw new ArgumentException("A diagnostic run is limited to 64 selected cases. Use the unchanged full protocol for acceptance.");
        }

        return result;
    }

    private static string[] List(string text)
    {
        string[] values = text.Split(',', StringSplitOptions.TrimEntries);
        if (values.Length == 0 || values.Any(string.IsNullOrWhiteSpace) || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException("Diagnostic lists must be nonempty comma-separated unique values.");
        }

        return values;
    }

    private static int Count(IReadOnlyDictionary<string, string> values, string name, int fallback, int minimum, int maximum)
    {
        int count = int.Parse(values.GetValueOrDefault(name, fallback.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
        if (count < minimum || count > maximum)
        {
            throw new ArgumentException($"{name} must be between {minimum} and {maximum}.");
        }

        return count;
    }
}
