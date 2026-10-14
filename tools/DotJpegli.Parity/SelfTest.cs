using System.Text;

namespace DotJpegli.Parity;

internal static class SelfTest
{
    public static void Run()
    {
        var image = new PpmImage(11, 9, Enumerable.Range(0, 11 * 9 * 3).Select(index => (byte)(index % 180)).ToArray());
        QualityMetrics identity = QualityMetrics.Compare(image, image);
        Near(identity.RgbMse, 0); Near(identity.RgbPsnrDb, 120); Near(identity.LumaSsim, 1);
        var shifted = image with { Pixels = image.Pixels.Select(value => (byte)(value + 10)).ToArray() };
        QualityMetrics distortion = QualityMetrics.Compare(image, shifted);
        Near(distortion.RgbMse, 100); Near(distortion.RgbPsnrDb, 10 * Math.Log10(650.25));
        Check(distortion.LumaSsim < 1 && distortion.LumaSsim > .8, "SSIM must detect a nontrivial luminance distortion.");
        Throws<InvalidDataException>(() => QualityMetrics.Compare(image, new(1, 1, [0, 0, 0])));

        var points = RateDistortion.Frontier(new[] { new CurvePoint(100, 30), new CurvePoint(150, 25), new CurvePoint(200, 35),
            new CurvePoint(200, 40), new CurvePoint(400, 38), new CurvePoint(800, 50) });
        Check(points.Count == 3 && points[1].Bytes == 200 && points[1].Quality == 40, "Duplicate rates and dominated points must be removed.");
        Near(RateDistortion.Interpolate(points, Math.Sqrt(100 * 200)), 35);
        Near(RateDistortion.Invert(points, 35), Math.Sqrt(100 * 200));
        double[] targets = RateDistortion.Targets(points, points);
        Check(targets.Length == 5 && targets[0] > 100 && targets[^1] < 800, "Rate targets must be interior.");
        double[] qualityTargets = RateDistortion.QualityTargets(points, points);
        Near(qualityTargets[0], 32); Near(qualityTargets[^1], 48);
        Check(RateDistortion.WorstRateTargets(points, points).SequenceEqual(new double[] { 100, 200, 800 }), "Rate worst-case checks must include overlap endpoints and frontier knots.");
        Check(RateDistortion.WorstQualityTargets(points, points).SequenceEqual(new double[] { 30, 40, 50 }), "Inverse worst-case checks must include overlap endpoints and frontier knots.");
        Throws<InvalidDataException>(() => RateDistortion.Interpolate(points, 99));
        Throws<InvalidDataException>(() => RateDistortion.Invert(points, 29));
        Throws<InvalidDataException>(() => RateDistortion.Targets(new[] { new CurvePoint(100, 30) }));
        Throws<InvalidDataException>(() => RateDistortion.QualityTargets(points, new[] { new CurvePoint(900, 60), new CurvePoint(1000, 70) }));
        Throws<InvalidDataException>(() => RateDistortion.Frontier(new[] { new CurvePoint(0, 30) }));

        byte[] header = Encoding.ASCII.GetBytes("P6\n# header comment\n1 1\n255\n");
        PpmImage whitespacePixels = PpmImage.Read(header.Concat(new byte[] { 10, 32, 35 }).ToArray());
        Check(whitespacePixels.Pixels.SequenceEqual(new byte[] { 10, 32, 35 }), "Raster bytes must not be treated as header whitespace/comments.");
        Throws<InvalidDataException>(() => PpmImage.Read(header.Concat(new byte[] { 0, 0 }).ToArray()));
        Throws<InvalidDataException>(() => PpmImage.Read(header.Concat(new byte[] { 0, 0, 0, 0 }).ToArray()));
        Throws<InvalidDataException>(() => PpmImage.Read(Encoding.ASCII.GetBytes("P3\n1 1\n255\n0 0 0")));
        Throws<InvalidDataException>(() => PpmImage.Read(Encoding.ASCII.GetBytes("P6\n1 1\n65535\n")));
        Throws<ArgumentException>(() => Options.Parse([]));
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-parity-self-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        Throws<InvalidDataException>(() => Program.ValidateCorpus(scratch));
        // This directory is uniquely owned and verified empty; no recursive cleanup.
        Directory.Delete(scratch);

        var sample = new EncodedSample(100, "hash", "hash", "image.jpg", new(1, 48, .99));
        RawCase[] cases = [new("image", "controlled", 444, 2, 1, sample, sample)];
        MatchedRate[] matches = Enumerable.Range(0, 5).Select(index => new MatchedRate("image", "controlled", 444, 2, 100 + index, 48, 48, .99, .99)).ToArray();
        var failures = new List<string>();
        List<GroupResult> passed = Evidence.Evaluate(cases, matches, new(), failures);
        Check(passed.Single().Passed && failures.Count == 0, "Equal encoders must pass the gate.");
        failures.Clear();
        RawCase[] excessiveSize = [cases[0] with { Managed = sample with { Bytes = 106 } }];
        Check(!Evidence.Evaluate(excessiveSize, matches, new(), failures).Single().Passed && failures.Count >= 2, "Aggregate and individual size violations must fail.");
        failures.Clear();
        MatchedRate[] badQuality = matches.Select(item => item with { ManagedPsnrDb = 47.7, ManagedSsim = .986 }).ToArray();
        Check(!Evidence.Evaluate(cases, badQuality, new(), failures).Single().Passed && failures.Count >= 2, "Matched-rate quality violations must fail.");
        failures.Clear();
        Check(!Evidence.Evaluate(cases, matches.Take(4).ToArray(), new(), failures).Single().Passed, "Missing rate coverage must fail.");
        failures.Clear();
        var qualityMatches = new[] { new MatchedQuality("image", "controlled", 444, 2, "rgb-psnr-db", 48, 106, 100),
            new MatchedQuality("image", "controlled", 444, 2, "luma-ssim", .99, 100, 100) };
        Check(!Evidence.EvaluateQuality(cases, qualityMatches, new(), failures)[0].Passed && failures.Count >= 2,
            "Matched-quality rate violations must fail.");
        failures.Clear();
        MatchedQuality[] completeQuality = new[] { "rgb-psnr-db", "luma-ssim" }.SelectMany(metric => Enumerable.Range(0, 5)
            .Select(index => new MatchedQuality("image", "controlled", 444, 2, metric, index, 100, 100))).ToArray();
        Check(Evidence.EvaluateQuality(cases, completeQuality, new(), failures).All(item => item.Passed) && failures.Count == 0,
            "Complete equal matched-quality evidence must pass.");
        failures.Clear();
        Check(Evidence.EvaluateQuality(cases, completeQuality.Take(9).ToArray(), new(), failures).Any(item => !item.Passed) && failures.Count > 0,
            "Omitted matched-quality target must fail coverage.");
        failures.Clear();
        var betweenAnchorViolation = new MatchedRate("image", "controlled", 444, 2, 150, 47, 48, .99, .99, false);
        GroupResult knotFailed = Evidence.Evaluate(cases, matches.Append(betweenAnchorViolation).ToArray(), new(), failures).Single();
        Check(!knotFailed.Passed && knotFailed.MeanPsnrDeltaDb == 0 && knotFailed.WorstPsnrDeltaDb == -1,
            "Non-anchor knot failure must affect worst bounds without changing anchor means.");
        failures.Clear();
        var betweenQualityAnchorViolation = new MatchedQuality("image", "controlled", 444, 2, "rgb-psnr-db", 48, 106, 100, false);
        QualityGroupResult qualityKnotFailed = Evidence.EvaluateQuality(cases, completeQuality.Append(betweenQualityAnchorViolation).ToArray(), new(), failures)[0];
        Check(!qualityKnotFailed.Passed && qualityKnotFailed.GeometricMeanSizeRatio == 1 && qualityKnotFailed.WorstSizeRatio == 1.06,
            "Non-anchor inverse knot failure must affect worst bounds without changing anchor geometric means.");
        Check(ControlledCorpus.Create().Count() == 8 && ControlledCorpus.Create().All(item => item.Image.Pixels.Length == item.Image.Width * item.Image.Height * 3),
            "Controlled corpus dimensions must be valid.");
        PpmImage fixture = ControlledCorpus.Create().Single(item => item.Name == "tiny-odd-edges").Image;
        foreach (int sampling in new[] { 444, 420 })
        {
            foreach (int progression in new[] { 0, 2 })
            {
                byte[] encoded = DotJpegli.JpegliEncoder.Encode(fixture.Pixels, fixture.Width, fixture.Height,
                    options: new() { ProgressiveLevel = progression, Subsampling = sampling == 444 ? DotJpegli.ChromaSubsampling.None : DotJpegli.ChromaSubsampling.Quarter });
                Check(JpegStructure.Compare(encoded, encoded, fixture, sampling, progression) is null, "JPEG structural gate must accept a correctly configured encoding.");
                Check(JpegStructure.Compare(encoded, encoded, fixture, sampling == 444 ? 420 : 444, progression) is not null, "JPEG structural gate must reject wrong sampling.");
                Check(JpegStructure.Compare(encoded, encoded, fixture, sampling, progression == 0 ? 2 : 0) is not null, "JPEG structural gate must reject wrong progression.");
                byte[] changed = encoded.ToArray();
                int dqt = FindMarker(changed, 0xdb);
                changed[dqt + 5] = changed[dqt + 5] == 255 ? (byte)254 : (byte)(changed[dqt + 5] + 1);
                Check(JpegStructure.Compare(encoded, changed, fixture, sampling, progression)?.Contains("quantization table values differ", StringComparison.Ordinal) == true,
                    "JPEG structural gate must detect changed quantization values.");
                Throws<InvalidDataException>(() => JpegStructure.Read(encoded[..^1]));
            }
        }
        failures.Clear();
        Check(!Evidence.Evaluate([cases[0] with { StructureDifference = "test mismatch" }], matches, new(), failures).Single().Passed,
            "JPEG configuration mismatch must fail the evidence gate.");
    }

    private static void Near(double actual, double expected)
    {
        Check(double.IsFinite(actual) && Math.Abs(actual - expected) <= 1e-9 * Math.Max(1, Math.Abs(expected)), $"Expected {expected:R}, got {actual:R}.");
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Self-test failed: " + message);
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Self-test failed: expected " + typeof(T).Name);
    }
    private static int FindMarker(byte[] data, byte marker)
    {
        for (int index = 2; index < data.Length - 1; index++)
            if (data[index] == 0xff && data[index + 1] == marker) return index;
        throw new InvalidOperationException("Self-test fixture does not contain expected JPEG marker.");
    }
}
