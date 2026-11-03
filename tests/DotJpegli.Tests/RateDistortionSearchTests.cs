namespace DotJpegli.Tests;

using System.Security.Cryptography;
using DotJpegli.Internal;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class RateDistortionSearchTests
{
    [Fact]
    public void GridIsBoundedDeterministicAndPreservesDefaultControlOptions()
    {
        IReadOnlyList<RateDistortionSearch.Candidate> candidates = RateDistortionSearch.GenerateCandidates();
        Assert.Equal(25, candidates.Count);
        Assert.Equal(25, candidates.Select(candidate => candidate.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(new RateDistortionSearch.Candidate("control", 1, 0), candidates[0]);
        Assert.Equal((float[])[.5f, 1f, 2f, 4f, 8f], candidates.Select(candidate => candidate.Distance).Distinct().Order());
        Assert.Equal((double[])[0, 1, 16, 256, 4096], candidates.Select(candidate => candidate.Lambda).Distinct().Order());
        Assert.Equal(candidates, RateDistortionSearch.GenerateCandidates());
        foreach (RateDistortionSearch.Candidate candidate in candidates)
        {
            var diagnostics = new RateDistortionDiagnostics();
            JpegliEncoderOptions options = candidate.CreateOptions(ChromaSubsampling.Quarter, diagnostics);
            Assert.Equal(candidate.Distance, options.Distance);
            Assert.Equal(candidate.Lambda, options.RateDistortionLambda);
            Assert.Same(diagnostics, options.RateDistortionDiagnostics);
            Assert.Equal(ChromaSubsampling.Quarter, options.Subsampling);
            Assert.Equal(JpegColorSpace.YCbCr, options.ColorSpace);
            Assert.True(options.AdaptiveQuantization);
            Assert.True(options.OptimizeHuffmanTables);
            Assert.True(options.ForceBaseline);
            Assert.Equal(0, options.ProgressiveLevel);
            Assert.Equal(1, options.MaxDegreeOfParallelism);
            Assert.Null(options.QuantizationTables);
        }
    }

    [Theory]
    [InlineData(ChromaSubsampling.None, 18)]
    [InlineData(ChromaSubsampling.Quarter, 10)]
    public void EveryTrialRetainsActualBytesDecodedMetricsSearchWorkAndConstrainedWinner(
        ChromaSubsampling sampling, int blocks)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24), original = (byte[])pixels.Clone();
        var image = new PpmImage(width, height, pixels);
        int decodeCount = 0;
        RateDistortionSearch.SearchResult result = RateDistortionSearch.Search(image, sampling, jpeg =>
        {
            decodeCount++;
            return Decode(jpeg);
        }, TestContext.Current.CancellationToken);
        Assert.Equal(original, pixels);
        Assert.Equal(25, result.EncodeCount);
        Assert.Equal(result.EncodeCount, result.DecodeCount);
        Assert.Equal(result.DecodeCount, decodeCount);
        Assert.Equal(25, result.HistogramPassCount);
        Assert.Equal(25, result.EntropyEmissionPassCount);
        Assert.Equal(blocks * 20, result.SearchedBlocks);
        Assert.Equal(result.Trials.Sum(trial => trial.CandidateEvaluations), result.CandidateEvaluations);
        Assert.Equal(result.Trials.Sum(trial => trial.ChangedCoefficients), result.ChangedCoefficients);
        Assert.True(result.CandidateEvaluations > result.SearchedBlocks);

        // Construct the control independently, leaving the internal experiment
        // properties untouched, to verify the original public encoding policy.
        byte[] control = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24,
            new JpegliEncoderOptions
            {
                ColorSpace = JpegColorSpace.YCbCr,
                Distance = 1,
                Subsampling = sampling,
                AdaptiveQuantization = true,
                OptimizeHuffmanTables = true,
                ForceBaseline = true,
                ProgressiveLevel = 0,
                MaxDegreeOfParallelism = 1
            }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(Hash(control), result.Trials[0].Sha256);
        var controlMetrics = QualityMetrics.Compare(image, Decode(control));
        var eligible = new List<(RateDistortionSearch.Trial Trial, byte[] Jpeg)>();
        foreach (RateDistortionSearch.Trial trial in result.Trials)
        {
            RateDistortionSearch.Candidate candidate = Assert.Single(result.Candidates, item => item.Id == trial.Id);
            var diagnostics = new RateDistortionDiagnostics();
            byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24,
                candidate.CreateOptions(sampling, diagnostics), cancellationToken: TestContext.Current.CancellationToken);
            PpmImage decoded = Decode(jpeg);
            var metrics = QualityMetrics.Compare(image, decoded);
            Assert.Equal(candidate.Distance, trial.Distance);
            Assert.Equal(candidate.Lambda, trial.Lambda);
            Assert.Equal(jpeg.Length, trial.Bytes);
            Assert.Equal(Hash(jpeg), trial.Sha256);
            Assert.Equal(Hash(decoded.Pixels), trial.DecodedSha256);
            Assert.Equal(metrics, trial.Metrics);
            Assert.Equal(diagnostics.Blocks, trial.SearchedBlocks);
            Assert.Equal(diagnostics.CandidateEvaluations, trial.CandidateEvaluations);
            Assert.Equal(diagnostics.ChangedCoefficients, trial.ChangedCoefficients);
            if (trial.Lambda == 0)
            {
                Assert.Equal(0, trial.SearchedBlocks);
                Assert.Equal(0, trial.CandidateEvaluations);
                Assert.Equal(0, trial.ChangedCoefficients);
            }
            bool accepted = metrics.RgbPsnrDb >= controlMetrics.RgbPsnrDb && metrics.LumaSsim >= controlMetrics.LumaSsim;
            Assert.Equal(accepted, trial.Eligible);
            if (accepted)
            {
                eligible.Add((trial, jpeg));
            }
        }
        (RateDistortionSearch.Trial winner, byte[] winnerJpeg) = eligible.OrderBy(item => item.Trial.Bytes).First();
        Assert.Equal(winner.Id, result.WinnerId);
        Assert.Equal(winnerJpeg, result.Jpeg);
        Assert.True(result.Jpeg.Length <= control.Length);
        Assert.Equal(8, result.Comparisons.Count);
    }

    [Fact]
    public void MatchedCurvesRetainBothMetricsAndSeparateSizeAndQualityComparisons()
    {
        var trials = new List<RateDistortionSearch.Trial>();
        for (int index = 0; index < 3; index++)
        {
            int bytes = 100 << index;
            double psnr = 30 + (index * 5), ssim = .9 + (index * .04);
            trials.Add(Trial($"control-{index}", 0, bytes, psnr, ssim));
            trials.Add(Trial($"candidate-{index}", 1, bytes * 4 / 5, psnr, ssim));
        }
        IReadOnlyList<RateDistortionSearch.Comparison> comparisons = RateDistortionSearch.CompareTrials(trials);
        Assert.Equal(8, comparisons.Count);
        foreach (RateDistortionSearch.Comparison comparison in comparisons.Where(item => item.Lambda == 1))
        {
            Assert.Equal("supported", comparison.MatchedRateStatus);
            Assert.Equal("supported", comparison.MatchedQualityStatus);
            Assert.Equal(3, comparison.ControlFrontier.Count);
            Assert.Equal(3, comparison.CandidateFrontier.Count);
            Assert.Equal(5, comparison.MatchedRates.Count);
            Assert.Equal(5, comparison.MatchedQualities.Count);
            Assert.All(comparison.MatchedRates, match =>
            {
                Assert.InRange(match.Bytes, 100d, 320d);
                Assert.True(match.QualityDelta > 0);
                Assert.Equal(match.CandidateQuality - match.ControlQuality, match.QualityDelta);
            });
            Assert.All(comparison.MatchedQualities, match =>
            {
                Assert.InRange(match.SizeRatio, .8 - 1e-12, .8 + 1e-12);
                Assert.Equal(match.CandidateBytes / match.ControlBytes, match.SizeRatio);
            });
        }
        Assert.All(comparisons.Where(item => item.Lambda != 1), comparison =>
        {
            Assert.StartsWith("unsupported: ", comparison.MatchedRateStatus);
            Assert.StartsWith("unsupported: ", comparison.MatchedQualityStatus);
            Assert.Empty(comparison.MatchedRates);
            Assert.Empty(comparison.MatchedQualities);
        });
    }

    [Fact]
    public void ConstantCurvesAndDisjointRangesAreExplicitlyUnsupported()
    {
        RateDistortionSearch.Trial[] trials =
        [
            Trial("c0", 0, 100, 30, .9), Trial("c1", 0, 200, 40, .95),
            Trial("flat0", 1, 100, 35, .92), Trial("flat1", 1, 200, 35, .92),
            Trial("disjoint0", 16, 300, 50, .97), Trial("disjoint1", 16, 400, 60, .99)
        ];
        IReadOnlyList<RateDistortionSearch.Comparison> comparisons = RateDistortionSearch.CompareTrials(trials);
        Assert.All(comparisons, comparison =>
        {
            Assert.StartsWith("unsupported: ", comparison.MatchedRateStatus);
            Assert.StartsWith("unsupported: ", comparison.MatchedQualityStatus);
            Assert.Empty(comparison.MatchedRates);
            Assert.Empty(comparison.MatchedQualities);
        });
    }

    [Fact]
    public void IndependentDecodedQualityFloorsRejectEveryDegradedTrial()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        int calls = 0;
        RateDistortionSearch.SearchResult result = RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => calls++ == 0 ? image : new(1, 1, [0, 0, 0]), TestContext.Current.CancellationToken);
        Assert.Equal("control", result.WinnerId);
        Assert.True(result.Trials[0].Eligible);
        Assert.All(result.Trials.Skip(1), trial => Assert.False(trial.Eligible));
        Assert.Equal(25, calls);
    }

    [Fact]
    public void CancellationAndInvalidDecodingStopWithoutCompletingSearch()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        int calls = 0;
        Assert.Throws<OperationCanceledException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => { calls++; return image; }, cancelled.Token));
        Assert.Equal(0, calls);
        using var duringDecode = new CancellationTokenSource();
        Assert.Throws<OperationCanceledException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => { calls++; duringDecode.Cancel(); return image; }, duringDecode.Token));
        Assert.Equal(1, calls);
        Assert.Throws<InvalidDataException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => new(2, 1, [0, 0, 0, 0, 0, 0]), TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => new(1, 1, []), TestContext.Current.CancellationToken));
        Assert.Throws<InvalidDataException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            _ => throw new InvalidDataException("decoder failure"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void IncompleteInputsAndUnsupportedSamplingAreRejectedBeforeDecoding()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        Assert.Throws<ArgumentNullException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.None,
            null!, TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentNullException>(() => RateDistortionSearch.Search(null!, ChromaSubsampling.None,
            _ => image, TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentException>(() => RateDistortionSearch.Search(new(1, 1, []), ChromaSubsampling.None,
            _ => image, TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentException>(() => RateDistortionSearch.Search(new(0, 1, []), ChromaSubsampling.None,
            _ => image, TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => RateDistortionSearch.Search(image, ChromaSubsampling.HalfHorizontal,
            _ => image, TestContext.Current.CancellationToken));
    }

    private static RateDistortionSearch.Trial Trial(string id, double lambda, int bytes, double psnr, double ssim)
        => new(id, 1, lambda, bytes, "jpeg", "decoded", new(0, psnr, ssim), false, 0, 0, 0);

    private static PpmImage Decode(byte[] jpeg)
    {
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(decoded.Width, decoded.Height, decoded.Data);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
