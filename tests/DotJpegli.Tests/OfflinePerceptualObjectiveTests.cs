namespace DotJpegli.Tests;

using DotJpegli.Parity;
using Xunit;

public sealed class OfflinePerceptualObjectiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MultiscaleFloorRejectsCandidatesThatPassBothOriginalFloors(bool rateDistortion)
    {
        const int width = 16, height = 16;
        byte[] pixels = new byte[width * height * 3];
        pixels.AsSpan().Fill(128);
        var source = new PpmImage(width, height, pixels);
        byte[] checkerboard = new byte[pixels.Length], step = new byte[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = ((y * width) + x) * 3;
                checkerboard.AsSpan(offset, 3).Fill((byte)(((x + y) & 1) == 0 ? 120 : 136));
                step.AsSpan(offset, 3).Fill((byte)(x < width / 2 ? 124 : 132));
            }
        }
        var control = new PpmImage(width, height, checkerboard);
        var candidate = new PpmImage(width, height, step);
        var controlMetrics = QualityMetrics.Compare(source, control);
        var candidateMetrics = QualityMetrics.Compare(source, candidate);
        Assert.True(candidateMetrics.RgbPsnrDb > controlMetrics.RgbPsnrDb);
        Assert.True(candidateMetrics.LumaSsim > controlMetrics.LumaSsim);
        double controlScore = MultiScaleSsim.Compare(source, control, TestContext.Current.CancellationToken);
        double candidateScore = MultiScaleSsim.Compare(source, candidate, TestContext.Current.CancellationToken);
        Assert.True(candidateScore < controlScore);

        int decodes = 0;
        PpmImage Decode(byte[] _) => decodes++ == 0 ? control : candidate;
        (QualityMetrics Metrics, double Score, bool Eligible)[] trials;
        string winner;
        int validations;
        if (rateDistortion)
        {
            RateDistortionSearch.SearchResult result = RateDistortionSearch.Search(source, ChromaSubsampling.None,
                Decode, TestContext.Current.CancellationToken);
            trials = result.Trials.Select(trial => (trial.Metrics, trial.MultiScaleSsim, trial.Eligible)).ToArray();
            winner = result.WinnerId;
            validations = result.ReferenceMetricValidationCount;
        }
        else
        {
            QuantizationTableSearch.SearchResult result = QuantizationTableSearch.Search(source, 1, ChromaSubsampling.None,
                Decode, TestContext.Current.CancellationToken);
            trials = result.Trials.Select(trial => (trial.Metrics, trial.MultiScaleSsim, trial.Eligible)).ToArray();
            winner = result.WinnerId;
            validations = result.ReferenceMetricValidationCount;
        }
        Assert.Equal("control", winner);
        Assert.Equal(1, validations);
        Assert.Equal(decodes, trials.Length);
        Assert.True(trials[0].Eligible);
        Assert.Equal(controlScore, trials[0].Score);
        Assert.All(trials.Skip(1), trial =>
        {
            Assert.Equal(candidateMetrics, trial.Metrics);
            Assert.Equal(candidateScore, trial.Score);
            Assert.False(trial.Eligible);
        });
    }
}
