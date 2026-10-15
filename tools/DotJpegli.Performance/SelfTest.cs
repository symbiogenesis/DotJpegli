namespace DotJpegli.Performance;

internal static class SelfTest
{
    public static void Run()
    {
        var policy = new Policy();
        Assert(policy.ManagedWorkers == 1 && policy.Blocks == 7 && policy.Warmups >= 5 && policy.MaximumGroupGeometricMeanRatio == 1.05 && policy.MaximumCaseRatio == 1.10, "Fixed policy changed.");
        Assert(Evidence.Iterations(1_000_000, policy) == 50 && Evidence.Iterations(1, policy) == 100 && Evidence.Iterations(100_000_000, policy) == 3, "Calibration/count clamps are incorrect.");
        Reject(() => Evidence.Iterations(double.NaN, policy));
        Reject(() => Evidence.Iterations(0, policy));
        TimingStatistics stats = Evidence.Statistics([7, 1, 3, 5]);
        Near(stats.Median, 4); Near(stats.P10, 1.6); Near(stats.P90, 6.4); Near(stats.RelativeMad, .5);
        Assert(Evidence.Statistics([1, 1, 1]).RelativeMad == 0, "Constant samples should have zero MAD.");
        Reject(() => Evidence.Statistics([])); Reject(() => Evidence.Statistics([1, double.PositiveInfinity]));
        Reject(() => Evidence.Statistics([0, 1]));
        const string nativeFixture = """
            {"encoder":"native-jpegli","upstreamPin":"031a0077f5799a6041004267fc12b956c1f52a20","width":768,"height":512,
            "distance":1,"sampling":"444","progressiveLevel":2,"optimizeHuffman":true,"adaptiveQuantization":true,
            "threads":1,"warmups":5,"iterations":3,"repeats":1,"bytes":12345,"deterministic":true,"samplesNsPerEncode":[1000000]}
            """;
        Program.NativeSample native = Program.ParseNativeSample(nativeFixture, 768, 512, 1, 444, 2, 3, policy);
        Assert(native.NsPerEncode == 1_000_000 && native.Bytes == 12345, "Native JSON contract parsing failed.");
        Reject(() => Program.ParseNativeSample(nativeFixture.Replace("\"deterministic\":true", "\"deterministic\":false", StringComparison.Ordinal), 768, 512, 1, 444, 2, 3, policy));
        Reject(() => Program.ParseNativeSample(nativeFixture.Replace("[1000000]", "[]", StringComparison.Ordinal), 768, 512, 1, 444, 2, 3, policy));
        Reject(() => Program.ParseNativeSample(nativeFixture.Replace("[1000000]", "[-1000000]", StringComparison.Ordinal), 768, 512, 1, 444, 2, 3, policy));
        Reject(() => Program.ParseNativeSample(nativeFixture.Replace("\"threads\":1,", "", StringComparison.Ordinal), 768, 512, 1, 444, 2, 3, policy));
        Reject(() => Program.ParseNativeSample(nativeFixture, 768, 512, 1, 420, 2, 3, policy));

        List<CaseResult> cases = Corpus(1);
        Evaluation passing = Evidence.Evaluate(cases, policy);
        Assert(passing.Valid && passing.Passed && passing.Groups.Length == 8 && cases.Count == 256, "Complete in-bound evidence must pass.");
        Assert(Evidence.Evaluate(Corpus(.5), policy).Passed, "Faster managed latency is accepted.");
        cases[0] = Case(cases[0].Id, cases[0].Category, cases[0].Sampling, cases[0].ProgressiveLevel, cases[0].Distance, 1.11);
        Evaluation isolatedSlow = Evidence.Evaluate(cases, policy);
        Assert(Evidence.IsDecisiveCaseFailure(cases[0], policy), "A stable individual counterexample must permit a failed verdict.");
        Assert(!Evidence.IsDecisiveCaseFailure(Corpus(1)[0], policy), "An in-bound observation cannot bypass complete passing coverage.");
        Assert(!Evidence.IsDecisiveCaseFailure(cases[0] with { StructureDifference = "wrong quantizers" }, policy), "Unmatched settings cannot establish a counterexample.");
        Assert(!Evidence.IsDecisiveCaseFailure(cases[0] with { MedianRatio = 100 }, policy), "A counterexample ratio must agree with raw measurements.");
        Assert(!Evidence.IsDecisiveCaseFailure(cases[0] with { Blocks = cases[0].Blocks[..6] }, policy), "Incomplete blocks cannot establish a counterexample.");
        Evaluation counterexample = Evidence.EvaluateCounterexample([cases[0]], policy);
        Assert(counterexample.Valid && !counterexample.Passed && counterexample.Groups.Length == 0, "A counterexample permits only a failed verdict without aggregate claims.");
        Evaluation invalidPrior = Evidence.EvaluateCounterexample([cases[1] with { StructureDifference = "wrong quantizers" }, cases[0]], policy);
        Assert(!invalidPrior.Valid && !invalidPrior.Passed && invalidPrior.InvalidReasons.Any(reason => reason.Contains("wrong quantizers", StringComparison.Ordinal)), "Early failure must preserve invalid earlier evidence.");
        Reject(() => Evidence.EvaluateCounterexample([Corpus(1)[0]], policy));
        Assert(isolatedSlow.Valid && !isolatedSlow.Passed && isolatedSlow.Groups[0].GeometricMeanRatio < 1.05 && isolatedSlow.Failures.Any(reason => reason.Contains("exceeds 1.10", StringComparison.Ordinal)), "A slow individual case cannot hide in the mean.");
        Evaluation meanSlow = Evidence.Evaluate(Corpus(1.051), policy);
        Assert(meanSlow.Valid && !meanSlow.Passed && meanSlow.Failures.Length == 8, "Group mean alone must fail despite acceptable individual ratios.");
        Assert(!Evidence.Evaluate(cases.Take(255).ToArray(), policy).Valid, "Missing matrix case must invalidate evidence.");
        cases = Corpus(1); cases[0] = cases[1];
        Assert(!Evidence.Evaluate(cases, policy).Valid, "Duplicate coverage must invalidate evidence even at the expected count.");
        cases = Corpus(1); cases[0] = cases[0] with { Distance = 9 };
        Assert(!Evidence.Evaluate(cases, policy).Valid, "Settings outside the predeclared matrix must be invalid.");
        cases = Corpus(1); cases[0] = cases[0] with { MedianRatio = .1 };
        Assert(!Evidence.Evaluate(cases, policy).Valid, "Summaries cannot contradict raw timings.");
        cases = Corpus(1); cases[0] = cases[0] with { StructureDifference = "wrong quantizers" };
        Evaluation wrongStructure = Evidence.Evaluate(cases, policy);
        Assert(!wrongStructure.Valid && !wrongStructure.Groups[0].Valid && !wrongStructure.Groups[0].Passed, "Different JPEG settings cannot establish speed parity or a valid group PASS.");

        BlockSample[] blocks = Corpus(1)[0].Blocks;
        Assert(Evidence.ValidateSamples(blocks, 3, policy).Length == 0, "Valid blocks rejected.");
        Assert(Evidence.ValidateSamples(blocks[..6], 3, policy).Length > 0, "Missing block accepted.");
        Assert(Evidence.ValidateSamples([.. blocks.Skip(1), blocks[1]], 3, policy).Length > 0, "Duplicate block accepted.");
        BlockSample[] invalid = [.. blocks]; invalid[0] = invalid[0] with { NativeNsPerEncode = double.NaN };
        Assert(Evidence.ValidateSamples(invalid, 3, policy).Length > 0, "Nonfinite sample accepted.");
        invalid = [.. blocks]; invalid[0] = invalid[0] with { ManagedNsPerEncode = 1 };
        Assert(Evidence.ValidateSamples(invalid, 3, policy).Length > 0, "Subminimum timer interval accepted.");
        invalid = [.. blocks]; invalid[1] = invalid[1] with { Order = invalid[0].Order };
        Assert(Evidence.ValidateSamples(invalid, 3, policy).Length > 0, "Repeated nonalternating measurement order accepted.");
        invalid = blocks.Select((block, index) => block with { ManagedNsPerEncode = index < 3 ? 800_000 : index == 3 ? 1_000_000 : 1_200_000 }).ToArray();
        Assert(Evidence.ValidateSamples(invalid, 3, policy).Any(reason => reason.Contains("MAD", StringComparison.Ordinal)), "Unstable samples must invalidate evidence.");
        Assert(Evidence.ValidateSamples(blocks, 2, policy).Length > 0 && Evidence.ValidateSamples(blocks, 101, policy).Length > 0, "Iteration limits must be enforced.");
    }

    private static List<CaseResult> Corpus(double ratio)
    {
        var cases = new List<CaseResult>();
        foreach (string category in new[] { "natural", "controlled" })
            foreach (string id in Evidence.ImageIds(category))
                foreach (int sampling in Evidence.Samplings)
                    foreach (int progression in Evidence.Progressions)
                        foreach (float distance in Evidence.Distances)
                            cases.Add(Case(id, category, sampling, progression, distance, ratio));
        return cases;
    }

    private static CaseResult Case(string id, string category, int sampling, int progression, float distance, double ratio)
    {
        BlockSample[] blocks = Enumerable.Range(0, 7).Select(index => new BlockSample(index, index % 2 == 0 ? "native-managed" : "managed-native",
            1_000_000 * ratio, 1_000_000, 0, 0, 0, 0)).ToArray();
        return new(id, category, sampling, progression, distance, 3, 1_000_000, blocks,
            Evidence.Statistics(blocks.Select(block => block.ManagedNsPerEncode).ToArray()), Evidence.Statistics(blocks.Select(block => block.NativeNsPerEncode).ToArray()),
            ratio, 100, 100, "synthetic-managed", "synthetic-native", null, []);
    }

    private static void Near(double actual, double expected) => Assert(Math.Abs(actual - expected) < 1e-12, "Percentile/MAD math differs from the specified interpolation.");
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException("Self-test: " + message); }
    private static void Reject(Action action)
    {
        try { action(); }
        catch (Exception error) when (error is InvalidDataException or KeyNotFoundException) { return; }
        throw new InvalidOperationException("Self-test: invalid timing input was accepted.");
    }
}
