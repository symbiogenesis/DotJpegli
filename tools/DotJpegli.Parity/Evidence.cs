namespace DotJpegli.Parity;

internal sealed record SourceImage(string Id, string Category, string SourcePath, string PpmSha256, string RgbSha256, int Width, int Height);
internal sealed record EncodedSample(long Bytes, string JpegSha256, string DecodedRgbSha256, string JpegPath, QualityMetrics Metrics);
internal sealed record RawCase(string Image, string Category, int Sampling, int Progression, float Distance, EncodedSample Managed, EncodedSample Native,
    string? StructureDifference = null)
{
    public double SizeRatio => (double)Managed.Bytes / Native.Bytes;
}
internal sealed record MatchedRate(string Image, string Category, int Sampling, int Progression, double TargetBytes,
    double ManagedPsnrDb, double NativePsnrDb, double ManagedSsim, double NativeSsim, bool IsAnchor = true)
{
    public double PsnrDeltaDb => ManagedPsnrDb - NativePsnrDb;
    public double SsimDelta => ManagedSsim - NativeSsim;
}
internal sealed record GroupResult(string Category, int Sampling, int Progression, int Images, int Cases, int MatchedTargets, int AnchorTargets,
    double GeometricMeanSizeRatio, double WorstSizeRatio, double MeanPsnrDeltaDb, double WorstPsnrDeltaDb,
    double MeanSsimDelta, double WorstSsimDelta, bool Passed);
internal sealed record MatchedQuality(string Image, string Category, int Sampling, int Progression, string Metric,
    double TargetQuality, double ManagedBytes, double NativeBytes, bool IsAnchor = true)
{
    public double SizeRatio => ManagedBytes / NativeBytes;
}
internal sealed record QualityGroupResult(string Category, int Sampling, int Progression, string Metric, int MatchedTargets, int AnchorTargets,
    double GeometricMeanSizeRatio, double WorstSizeRatio, bool Passed);
internal sealed record Policy(
    double MaximumGeometricMeanSizeRatio = 1.02, double MaximumCaseSizeRatio = 1.05,
    double MinimumMeanPsnrDeltaDb = -.10, double MinimumTargetPsnrDeltaDb = -.25,
    double MinimumMeanSsimDelta = -.001, double MinimumTargetSsimDelta = -.003);
internal sealed record Summary(int SchemaVersion, string Scope, bool Passed, string UpstreamRevision,
    string NativeVersion, string NativeExecutableSha256, string ManagedAssemblySha256, string HarnessAssemblySha256,
    string? TestedRevision, string? GitHubRevision, string? GitHubRunId, float[] Distances, int[] Samplings, int[] Progressions, object Settings,
    object Metrics, Policy Policy, int NaturalImages, int ControlledImages, int Cases, int MatchedTargets, int MatchedRateAnchors,
    int MatchedQualityTargets, int MatchedQualityAnchors, IReadOnlyList<GroupResult> Groups, IReadOnlyList<QualityGroupResult> MatchedQualityGroups,
    IReadOnlyList<string> MatchedQualityExclusions, IReadOnlyList<string> Failures);

internal static class Evidence
{
    public static List<MatchedRate> Match(IReadOnlyList<RawCase> cases, List<string> failures)
    {
        var results = new List<MatchedRate>();
        foreach (var group in cases.GroupBy(item => (item.Image, item.Category, item.Sampling, item.Progression)))
        {
            try
            {
                var managedPsnr = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Managed.Bytes, item.Managed.Metrics.RgbPsnrDb)));
                var nativePsnr = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Native.Bytes, item.Native.Metrics.RgbPsnrDb)));
                var managedSsim = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Managed.Bytes, item.Managed.Metrics.LumaSsim)));
                var nativeSsim = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Native.Bytes, item.Native.Metrics.LumaSsim)));
                double[] anchors = RateDistortion.Targets(managedPsnr, nativePsnr, managedSsim, nativeSsim);
                foreach (double target in anchors.Concat(RateDistortion.WorstRateTargets(managedPsnr, nativePsnr, managedSsim, nativeSsim)).Distinct().Order())
                    results.Add(new(group.Key.Image, group.Key.Category, group.Key.Sampling, group.Key.Progression, target,
                        RateDistortion.Interpolate(managedPsnr, target), RateDistortion.Interpolate(nativePsnr, target),
                        RateDistortion.Interpolate(managedSsim, target), RateDistortion.Interpolate(nativeSsim, target), anchors.Contains(target)));
            }
            catch (InvalidDataException error)
            {
                failures.Add($"{group.Key.Image}/{group.Key.Sampling}/p{group.Key.Progression}: {error.Message}");
            }
        }
        return results;
    }

    public static List<GroupResult> Evaluate(IReadOnlyList<RawCase> cases, IReadOnlyList<MatchedRate> matches, Policy policy, List<string> failures)
    {
        var results = new List<GroupResult>();
        foreach (var group in cases.GroupBy(item => (item.Category, item.Sampling, item.Progression)))
        {
            MatchedRate[] rates = matches.Where(item => item.Category == group.Key.Category && item.Sampling == group.Key.Sampling && item.Progression == group.Key.Progression).ToArray();
            MatchedRate[] anchors = rates.Where(item => item.IsAnchor).ToArray();
            int images = group.Select(item => item.Image).Distinct().Count();
            double geometricMean = Math.Exp(group.Average(item => Math.Log(item.SizeRatio)));
            double worstRatio = group.Max(item => item.SizeRatio);
            double meanPsnr = anchors.Length == 0 ? -120 : anchors.Average(item => item.PsnrDeltaDb);
            double worstPsnr = rates.Length == 0 ? -120 : rates.Min(item => item.PsnrDeltaDb);
            double meanSsim = anchors.Length == 0 ? -2 : anchors.Average(item => item.SsimDelta);
            double worstSsim = rates.Length == 0 ? -2 : rates.Min(item => item.SsimDelta);
            bool coverage = anchors.Length == images * 5 && group.Select(item => item.Image).Distinct().All(image => anchors.Count(item => item.Image == image) == 5);
            bool passed = group.All(item => item.StructureDifference is null) && coverage && geometricMean <= policy.MaximumGeometricMeanSizeRatio &&
                worstRatio <= policy.MaximumCaseSizeRatio && meanPsnr >= policy.MinimumMeanPsnrDeltaDb &&
                worstPsnr >= policy.MinimumTargetPsnrDeltaDb && meanSsim >= policy.MinimumMeanSsimDelta && worstSsim >= policy.MinimumTargetSsimDelta;
            results.Add(new(group.Key.Category, group.Key.Sampling, group.Key.Progression, images, group.Count(), rates.Length, anchors.Length, geometricMean, worstRatio,
                meanPsnr, worstPsnr, meanSsim, worstSsim, passed));
            string prefix = $"{group.Key.Category}/{group.Key.Sampling}/p{group.Key.Progression}";
            foreach (RawCase item in group.Where(item => item.StructureDifference is not null))
                failures.Add(FormattableString.Invariant($"{item.Image}/{item.Sampling}/p{item.Progression}/d{item.Distance}: JPEG configuration mismatch: {item.StructureDifference}."));
            if (!coverage) failures.Add($"{prefix}: {anchors.Length} of {images * 5} required matched-rate anchors; every image requires five.");
            if (geometricMean > policy.MaximumGeometricMeanSizeRatio)
                failures.Add(FormattableString.Invariant($"{prefix}: geometric mean size ratio {geometricMean:F6} exceeds {policy.MaximumGeometricMeanSizeRatio}."));
            if (meanPsnr < policy.MinimumMeanPsnrDeltaDb)
                failures.Add(FormattableString.Invariant($"{prefix}: mean matched-rate PSNR delta {meanPsnr:F6} dB below {policy.MinimumMeanPsnrDeltaDb}."));
            if (meanSsim < policy.MinimumMeanSsimDelta)
                failures.Add(FormattableString.Invariant($"{prefix}: mean matched-rate SSIM delta {meanSsim:F6} below {policy.MinimumMeanSsimDelta}."));
            foreach (RawCase item in group.Where(item => item.SizeRatio > policy.MaximumCaseSizeRatio))
                failures.Add(FormattableString.Invariant($"{item.Image}/{item.Sampling}/p{item.Progression}/d{item.Distance}: size ratio {item.SizeRatio:F6} exceeds {policy.MaximumCaseSizeRatio}."));
            foreach (MatchedRate item in rates.Where(item => item.PsnrDeltaDb < policy.MinimumTargetPsnrDeltaDb || item.SsimDelta < policy.MinimumTargetSsimDelta))
                failures.Add(FormattableString.Invariant($"{item.Image}/{item.Sampling}/p{item.Progression}/{item.TargetBytes:F2} bytes: matched-rate PSNR delta {item.PsnrDeltaDb:F6} dB, SSIM delta {item.SsimDelta:F6}; required >= {policy.MinimumTargetPsnrDeltaDb}, {policy.MinimumTargetSsimDelta}."));
        }
        return results;
    }

    public static List<MatchedQuality> MatchQuality(IReadOnlyList<RawCase> cases, List<string> exclusions)
    {
        var results = new List<MatchedQuality>();
        foreach (var group in cases.GroupBy(item => (item.Image, item.Category, item.Sampling, item.Progression)))
        {
            foreach (string metric in new[] { "rgb-psnr-db", "luma-ssim" })
            {
                double Select(QualityMetrics metrics) => metric == "rgb-psnr-db" ? metrics.RgbPsnrDb : metrics.LumaSsim;
                var managed = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Managed.Bytes, Select(item.Managed.Metrics))));
                var native = RateDistortion.Frontier(group.Select(item => new CurvePoint(item.Native.Bytes, Select(item.Native.Metrics))));
                try
                {
                    double[] anchors = RateDistortion.QualityTargets(managed, native);
                    foreach (double target in anchors.Concat(RateDistortion.WorstQualityTargets(managed, native)).Distinct().Order())
                        results.Add(new(group.Key.Image, group.Key.Category, group.Key.Sampling, group.Key.Progression, metric,
                            target, RateDistortion.Invert(managed, target), RateDistortion.Invert(native, target), anchors.Contains(target)));
                }
                catch (InvalidDataException error)
                {
                    exclusions.Add($"{group.Key.Image}/{group.Key.Sampling}/p{group.Key.Progression}/{metric}: {error.Message}");
                }
            }
        }
        return results;
    }

    public static List<QualityGroupResult> EvaluateQuality(IReadOnlyList<RawCase> cases, IReadOnlyList<MatchedQuality> matches, Policy policy, List<string> failures)
    {
        var results = new List<QualityGroupResult>();
        foreach (var group in cases.GroupBy(item => (item.Category, item.Sampling, item.Progression)))
        {
            foreach (string metric in new[] { "rgb-psnr-db", "luma-ssim" })
            {
                MatchedQuality[] rates = matches.Where(item => item.Category == group.Key.Category && item.Sampling == group.Key.Sampling &&
                    item.Progression == group.Key.Progression && item.Metric == metric).ToArray();
                MatchedQuality[] anchors = rates.Where(item => item.IsAnchor).ToArray();
                int requiredTargets = group.Select(item => item.Image).Distinct().Count() * 5;
                double geometricMean = anchors.Length == 0 ? 1000 : Math.Exp(anchors.Average(item => Math.Log(item.SizeRatio)));
                double worstRatio = rates.Length == 0 ? 1000 : rates.Max(item => item.SizeRatio);
                bool coverage = anchors.Length == requiredTargets && group.Select(item => item.Image).Distinct().All(image => anchors.Count(item => item.Image == image) == 5);
                bool passed = coverage && geometricMean <= policy.MaximumGeometricMeanSizeRatio && worstRatio <= policy.MaximumCaseSizeRatio;
                results.Add(new(group.Key.Category, group.Key.Sampling, group.Key.Progression, metric, rates.Length, anchors.Length, geometricMean, worstRatio, passed));
                string prefix = $"{group.Key.Category}/{group.Key.Sampling}/p{group.Key.Progression}/{metric}";
                if (!coverage) failures.Add($"{prefix}: {anchors.Length} of {requiredTargets} required matched-quality anchors; every image requires five, missing curves cannot establish parity.");
                if (geometricMean > policy.MaximumGeometricMeanSizeRatio)
                    failures.Add(FormattableString.Invariant($"{prefix}: matched-quality geometric mean rate ratio {geometricMean:F6} exceeds {policy.MaximumGeometricMeanSizeRatio}."));
                foreach (MatchedQuality item in rates.Where(item => item.SizeRatio > policy.MaximumCaseSizeRatio))
                    failures.Add(FormattableString.Invariant($"{item.Image}/{item.Sampling}/p{item.Progression}/{metric}/q{item.TargetQuality:F6}: matched-quality rate ratio {item.SizeRatio:F6} exceeds {policy.MaximumCaseSizeRatio}."));
            }
        }
        return results;
    }
}
