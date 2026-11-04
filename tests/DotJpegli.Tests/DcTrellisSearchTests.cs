namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;

public sealed class DcTrellisSearchTests
{
    [Fact]
    public void CompleteSearchRetainsEveryMeasuredTrialAndReproducesTheEligibleWinner()
    {
        string root = Path.Combine(Path.GetTempPath(), "DotJpegli-dc-command-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.ppm"), output = Path.Combine(root, "result");
        try
        {
            var image = new PpmImage(9, 5, Enumerable.Range(0, 135).Select(index => (byte)(index * 31 % 256)).ToArray());
            image.Write(input);
            int decodes = 0;
            var options = new RateDistortionSearchCommand.SearchOptions(input, output, "unused", ChromaSubsampling.Quarter);
            Assert.Equal(0, DcTrellisSearchCommand.Run(options, jpeg =>
            {
                decodes++;
                var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
                return new(decoded.Width, decoded.Height, decoded.Data);
            }, new { name = "StbImageSharp" }));
            Assert.Equal(5, decodes);
            byte[] winner = File.ReadAllBytes(Path.Combine(output, "winner.jpg"));
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "search.json")));
            JsonElement report = json.RootElement;
            Assert.Equal(5, report.GetProperty("encodeCount").GetInt32());
            Assert.Equal(5, report.GetProperty("decodeCount").GetInt32());
            Assert.Equal(15, report.GetProperty("histogramScanCount").GetInt32());
            Assert.Equal(15, report.GetProperty("outputScanCount").GetInt32());
            Assert.Equal(30, report.GetProperty("totalEntropyScanPassCount").GetInt32());
            Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))),
                report.GetProperty("input").GetProperty("sha256").GetString());
            JsonElement[] trials = report.GetProperty("trials").EnumerateArray().ToArray();
            Assert.Equal(5, trials.Length);
            Assert.Equal(0, trials[0].GetProperty("candidateEvaluations").GetInt64());
            Assert.All(trials.Skip(1), trial => Assert.True(trial.GetProperty("sourceTransformBlocks").GetInt64() > 0));
            string id = report.GetProperty("winner").GetString()!;
            JsonElement selected = Assert.Single(trials, trial => trial.GetProperty("id").GetString() == id);
            Assert.True(selected.GetProperty("eligible").GetBoolean());
            Assert.Equal(winner.Length, selected.GetProperty("bytes").GetInt32());
            Assert.Equal(trials.Where(trial => trial.GetProperty("eligible").GetBoolean())
                .Min(trial => trial.GetProperty("bytes").GetInt32()), winner.Length);
            DcTrellisSearch.Candidate candidate = Assert.Single(DcTrellisSearch.Candidates, item => item.Id == id);
            Assert.Equal(winner, JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
                candidate.CreateOptions(options.Subsampling), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Contains(id, File.ReadAllText(Path.Combine(output, "report.md")), StringComparison.Ordinal);
            _ = Assert.Throws<ArgumentException>(() => DcTrellisSearchCommand.Run(options,
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
