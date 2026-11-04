namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;

public sealed class HuffmanFeedbackSearchTests
{
    [Fact]
    public void PairedControlsAndAllCostModelsAreReportedAndWinnerIsReproducible()
    {
        string root = Path.Combine(Path.GetTempPath(), "DotJpegli-feedback-command-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.ppm"), output = Path.Combine(root, "result");
        try
        {
            var image = new PpmImage(9, 5, Enumerable.Range(0, 135).Select(index => (byte)(index * 31 % 256)).ToArray());
            image.Write(input);
            var options = new RateDistortionSearchCommand.SearchOptions(input, output, "unused", ChromaSubsampling.Quarter);
            int decodes = 0;
            Assert.Equal(0, HuffmanFeedbackSearchCommand.Run(options, jpeg =>
            {
                decodes++;
                var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
                return new(decoded.Width, decoded.Height, decoded.Data);
            }, new { name = "StbImageSharp" }));
            Assert.Equal(7, decodes);
            byte[] winner = File.ReadAllBytes(Path.Combine(output, "winner.jpg"));
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "search.json")));
            JsonElement report = json.RootElement;
            Assert.Equal(7, report.GetProperty("encodeCount").GetInt32());
            Assert.Equal(7, report.GetProperty("decodeCount").GetInt32());
            Assert.Equal(30, report.GetProperty("histogramScanCount").GetInt32());
            Assert.Equal(21, report.GetProperty("outputScanCount").GetInt32());
            Assert.Equal(51, report.GetProperty("totalEntropyScanPassCount").GetInt32());
            Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))),
                report.GetProperty("input").GetProperty("sha256").GetString());
            JsonElement[] trials = report.GetProperty("trials").EnumerateArray().ToArray();
            Assert.Equal(7, trials.Length);
            Assert.Equal(3, trials.Count(trial => trial.GetProperty("feedback").GetBoolean()));
            Assert.Equal(0, trials[0].GetProperty("standardEvaluations").GetInt64());
            foreach (JsonElement trial in trials)
            {
                bool feedback = trial.GetProperty("feedback").GetBoolean();
                if (feedback)
                {
                    Assert.True(trial.GetProperty("feedbackBlocks").GetInt64() > 0);
                    Assert.Equal(trial.GetProperty("feedbackBlocks").GetInt64(), trial.GetProperty("feedbackHistogramBlocks").GetInt64());
                    JsonElement[] lengths = trial.GetProperty("modelAcCodeLengths").EnumerateArray().ToArray();
                    Assert.Equal(3, lengths.Length);
                    Assert.All(lengths, component => Assert.Equal(256, component.GetArrayLength()));
                    double lambda = trial.GetProperty("lambda").GetDouble();
                    Assert.Contains(trials, other => !other.GetProperty("feedback").GetBoolean()
                        && other.GetProperty("lambda").GetDouble() == lambda);
                }
                else
                {
                    Assert.Equal(0, trial.GetProperty("feedbackEvaluations").GetInt64());
                    Assert.Equal(JsonValueKind.Null, trial.GetProperty("modelAcCodeLengths").ValueKind);
                }
            }
            string id = report.GetProperty("winner").GetString()!;
            JsonElement selected = Assert.Single(trials, trial => trial.GetProperty("id").GetString() == id);
            Assert.True(selected.GetProperty("eligible").GetBoolean());
            Assert.Equal(Convert.ToHexString(SHA256.HashData(winner)), selected.GetProperty("sha256").GetString());
            Assert.Equal(trials.Where(trial => trial.GetProperty("eligible").GetBoolean())
                .Min(trial => trial.GetProperty("bytes").GetInt32()), winner.Length);
            HuffmanFeedbackSearch.Candidate candidate = Assert.Single(HuffmanFeedbackSearch.Candidates, item => item.Id == id);
            Assert.Equal(winner, JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                candidate.CreateOptions(options.Subsampling), cancellationToken: TestContext.Current.CancellationToken));
            _ = Assert.Throws<ArgumentException>(() => HuffmanFeedbackSearchCommand.Run(options,
                _ => throw new InvalidOperationException("Reject output first."), new { }));
        }
        finally
        {
            if (Directory.Exists(output))
            {
                foreach (string file in Directory.EnumerateFiles(output))
                {
                    File.Delete(file);
                }
                Directory.Delete(output);
            }
            File.Delete(input);
            Directory.Delete(root);
        }
    }
}
