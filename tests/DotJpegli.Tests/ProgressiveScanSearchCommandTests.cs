namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotJpegli.Parity;
using Xunit;

public sealed class ProgressiveScanSearchCommandTests
{
    [Fact]
    public void CommandRetainsReproducibleWinnerAndEveryCandidateCostAndRefusesOverwrite()
    {
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-scan-search-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string input = Path.Combine(scratch, "source.ppm"), output = Path.Combine(scratch, "result");
        try
        {
            File.WriteAllBytes(input, "P6\n1 1\n255\n"u8.ToArray().Concat(new byte[] { 31, 113, 207 }).ToArray());
            string[] args = ["--input", input, "--output", output, "--distance", "1", "--sampling", "420"];
            Assert.Equal(0, ProgressiveScanSearchCommand.Run(args));
            string jpegPath = Path.Combine(output, "winner.jpg");
            byte[] jpeg = File.ReadAllBytes(jpegPath);
            using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "search.json")));
            JsonElement root = report.RootElement, candidates = root.GetProperty("candidates");
            Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal(ProgressiveScanSearch.Candidates.Count, root.GetProperty("encodeCount").GetInt32());
            Assert.Equal(ProgressiveScanSearch.Candidates.Count, candidates.GetArrayLength());
            string winner = root.GetProperty("winner").GetString()!;
            ProgressiveScanSearch.ScanCandidate selected = Assert.Single(ProgressiveScanSearch.Candidates, candidate => candidate.Id == winner);
            byte[] reproduced = JpegliEncoder.Encode([31, 113, 207], 1, 1, PixelLayout.Rgb24,
                selected.CreateOptions(1, ChromaSubsampling.Quarter), cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(reproduced, jpeg);
            int totalScans = 0, histogramScans = 0, minimumBytes = int.MaxValue;
            foreach (JsonElement candidate in candidates.EnumerateArray())
            {
                JsonElement trial = candidate.GetProperty("trial");
                int scans = trial.GetProperty("scanCount").GetInt32();
                totalScans += scans;
                if (trial.GetProperty("optimizeHuffmanTables").GetBoolean())
                {
                    histogramScans += scans;
                }

                minimumBytes = Math.Min(minimumBytes, trial.GetProperty("bytes").GetInt32());
                Assert.Equal(scans, trial.GetProperty("huffmanSelectors").GetArrayLength());
                if (candidate.GetProperty("id").GetString() == winner)
                {
                    Assert.Equal(Convert.ToHexString(SHA256.HashData(jpeg)), trial.GetProperty("sha256").GetString());
                }
            }

            Assert.Equal(minimumBytes, jpeg.Length);
            Assert.Equal(totalScans, root.GetProperty("totalScanCount").GetInt32());
            Assert.Equal(histogramScans, root.GetProperty("totalHistogramScanCount").GetInt32());
            Assert.Equal(totalScans + histogramScans, root.GetProperty("totalEntropyPassCount").GetInt32());
            Assert.Contains(winner, File.ReadAllText(Path.Combine(output, "report.md")), StringComparison.Ordinal);
            _ = Assert.Throws<ArgumentException>(() => ProgressiveScanSearchCommand.Run(args));
            Assert.Equal(jpeg, File.ReadAllBytes(jpegPath));
        }
        finally
        {
            if (Directory.Exists(output))
            {
                foreach (string name in new[] { "winner.jpg", "search.json", "report.md" })
                {
                    File.Delete(Path.Combine(output, name));
                }

                Directory.Delete(output);
            }

            File.Delete(input);
            Directory.Delete(scratch);
        }
    }

    [Theory]
    [InlineData("0", "444")]
    [InlineData("-1", "444")]
    [InlineData("25.01", "420")]
    [InlineData("NaN", "420")]
    [InlineData("Infinity", "420")]
    [InlineData("1,5", "420")]
    [InlineData("1", "422")]
    public void InvalidSearchSettingsAreRejected(string distance, string sampling) =>
        Assert.Throws<ArgumentException>(() => ProgressiveScanSearchCommand.SearchOptions.Parse(
            ["--input", "unused.ppm", "--output", "unused-result", "--distance", distance, "--sampling", sampling]));

    [Fact]
    public void MissingUnknownAndDuplicateOptionsAreRejected()
    {
        _ = Assert.Throws<ArgumentException>(() => ProgressiveScanSearchCommand.SearchOptions.Parse([]));
        _ = Assert.Throws<ArgumentException>(() => ProgressiveScanSearchCommand.SearchOptions.Parse(
            ["--input", "unused.ppm", "--output", "unused-result", "--distance", "1", "--distance", "4"]));
        _ = Assert.Throws<ArgumentException>(() => ProgressiveScanSearchCommand.SearchOptions.Parse(
            ["--input", "unused.ppm", "--output", "unused-result", "--distance", "1", "--unknown", "420"]));
    }

    [Fact]
    public void MalformedInputLeavesOutputUncreated()
    {
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-invalid-search-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string input = Path.Combine(scratch, "source.ppm"), output = Path.Combine(scratch, "result");
        try
        {
            File.WriteAllText(input, "P6\n1 1\n255\n", new UTF8Encoding(false));
            _ = Assert.Throws<InvalidDataException>(() => ProgressiveScanSearchCommand.Run(
                ["--input", input, "--output", output, "--distance", "1", "--sampling", "444"]));
            Assert.False(Directory.Exists(output));
        }
        finally
        {
            File.Delete(input);
            Directory.Delete(scratch);
        }
    }
}
