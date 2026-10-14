namespace DotJpegli.Parity;

internal readonly record struct CurvePoint(long Bytes, double Quality);

internal static class RateDistortion
{
    public static List<CurvePoint> Frontier(IEnumerable<CurvePoint> points)
    {
        var frontier = new List<CurvePoint>();
        foreach (CurvePoint point in points.GroupBy(point => point.Bytes)
            .Select(group => new CurvePoint(group.Key, group.Max(point => point.Quality))).OrderBy(point => point.Bytes))
        {
            if (point.Bytes <= 0 || !double.IsFinite(point.Quality))
                throw new InvalidDataException("Rate-distortion samples must have positive rates and finite metrics.");
            if (frontier.Count == 0 || point.Quality > frontier[^1].Quality) frontier.Add(point);
        }
        return frontier;
    }

    public static double Interpolate(IReadOnlyList<CurvePoint> curve, double bytes)
    {
        if (curve.Count < 2 || !double.IsFinite(bytes) || bytes < curve[0].Bytes || bytes > curve[^1].Bytes)
            throw new InvalidDataException("No supported rate-distortion interval; extrapolation is prohibited.");
        for (int index = 1; index < curve.Count; index++)
        {
            if (bytes <= curve[index].Bytes)
            {
                CurvePoint a = curve[index - 1], b = curve[index];
                double fraction = (Math.Log(bytes) - Math.Log(a.Bytes)) / (Math.Log(b.Bytes) - Math.Log(a.Bytes));
                return a.Quality + fraction * (b.Quality - a.Quality);
            }
        }
        throw new InvalidDataException("Unreachable rate-distortion interpolation point.");
    }

    public static double[] Targets(params IReadOnlyList<CurvePoint>[] curves)
    {
        if (curves.Length == 0 || curves.Any(curve => curve.Count < 2))
            throw new InvalidDataException("At least two nondominated points are required in every metric curve.");
        double minimum = curves.Max(curve => (double)curve[0].Bytes), maximum = curves.Min(curve => (double)curve[^1].Bytes);
        if (minimum >= maximum) throw new InvalidDataException("No common rate-distortion range.");
        return new[] { .1, .3, .5, .7, .9 }
            .Select(fraction => Math.Exp(Math.Log(minimum) + fraction * (Math.Log(maximum) - Math.Log(minimum)))).ToArray();
    }

    public static double Invert(IReadOnlyList<CurvePoint> curve, double quality)
    {
        if (curve.Count < 2 || !double.IsFinite(quality) || quality < curve[0].Quality || quality > curve[^1].Quality)
            throw new InvalidDataException("No supported quality interval; extrapolation is prohibited.");
        for (int index = 1; index < curve.Count; index++)
        {
            if (quality <= curve[index].Quality)
            {
                CurvePoint a = curve[index - 1], b = curve[index];
                double fraction = (quality - a.Quality) / (b.Quality - a.Quality);
                return Math.Exp(Math.Log(a.Bytes) + fraction * (Math.Log(b.Bytes) - Math.Log(a.Bytes)));
            }
        }
        throw new InvalidDataException("Unreachable quality interpolation point.");
    }

    public static double[] QualityTargets(IReadOnlyList<CurvePoint> managed, IReadOnlyList<CurvePoint> native)
    {
        if (managed.Count < 2 || native.Count < 2) throw new InvalidDataException("Constant or insufficient quality curve.");
        double minimum = Math.Max(managed[0].Quality, native[0].Quality), maximum = Math.Min(managed[^1].Quality, native[^1].Quality);
        if (minimum >= maximum) throw new InvalidDataException("No common quality range.");
        return new[] { .1, .3, .5, .7, .9 }.Select(fraction => minimum + fraction * (maximum - minimum)).ToArray();
    }

    public static double[] WorstRateTargets(params IReadOnlyList<CurvePoint>[] curves)
    {
        _ = Targets(curves);
        double minimum = curves.Max(curve => (double)curve[0].Bytes), maximum = curves.Min(curve => (double)curve[^1].Bytes);
        return curves.SelectMany(curve => curve.Select(point => (double)point.Bytes)).Where(bytes => bytes >= minimum && bytes <= maximum)
            .Append(minimum).Append(maximum).Distinct().Order().ToArray();
    }

    public static double[] WorstQualityTargets(IReadOnlyList<CurvePoint> managed, IReadOnlyList<CurvePoint> native)
    {
        _ = QualityTargets(managed, native);
        double minimum = Math.Max(managed[0].Quality, native[0].Quality), maximum = Math.Min(managed[^1].Quality, native[^1].Quality);
        return managed.Concat(native).Select(point => point.Quality).Where(quality => quality >= minimum && quality <= maximum)
            .Append(minimum).Append(maximum).Distinct().Order().ToArray();
    }
}
