namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;

public sealed class RateDistortionSearchCommandTests
{
    [Fact]
    public void ReportRetainsGridComparisonsSearchWorkAndReproducibleWinner()
    {
        using var fixture = new SearchFixture();
        int decodes = 0;
        Assert.Equal(0, RateDistortionSearchCommand.Run(fixture.Options, jpeg =>
        {
            decodes++;
            return Decode(jpeg);
        }, new { name = "StbImageSharp test decoder" }));

        byte[] winner = File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture.Output, "search.json")));
        JsonElement root = json.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(25, decodes);
        Assert.Equal(decodes, root.GetProperty("encodeCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("decodeCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("totalHistogramScanCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("totalOutputScanCount").GetInt32());
        Assert.Equal(2 * decodes, root.GetProperty("totalEntropyPassCount").GetInt32());
        Assert.Equal(25, root.GetProperty("candidates").GetArrayLength());
        Assert.Equal(25, root.GetProperty("trials").GetArrayLength());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture.Input))),
            root.GetProperty("input").GetProperty("sha256").GetString());
        Assert.Equal("StbImageSharp test decoder", root.GetProperty("decoder").GetProperty("name").GetString());
        Assert.Equal(420, root.GetProperty("settings").GetProperty("sampling").GetInt32());
        Assert.Equal(5, root.GetProperty("settings").GetProperty("distances").GetArrayLength());
        Assert.Equal(5, root.GetProperty("settings").GetProperty("lambdas").GetArrayLength());

        long blocks = 0, transitions = 0, changes = 0;
        foreach (JsonElement trial in root.GetProperty("trials").EnumerateArray())
        {
            long searched = trial.GetProperty("searchedBlocks").GetInt64();
            long evaluations = trial.GetProperty("candidateEvaluations").GetInt64();
            long changed = trial.GetProperty("changedCoefficients").GetInt64();
            if (trial.GetProperty("lambda").GetDouble() == 0)
            {
                Assert.Equal(0, searched);
                Assert.Equal(0, evaluations);
                Assert.Equal(0, changed);
            }
            else
            {
                Assert.Equal(4, searched);
                Assert.True(evaluations > 0);
            }
            blocks += searched;
            transitions += evaluations;
            changes += changed;
            Assert.Equal(64, trial.GetProperty("sha256").GetString()!.Length);
            Assert.Equal(64, trial.GetProperty("decodedSha256").GetString()!.Length);
        }
        Assert.Equal(80, blocks);
        Assert.Equal(blocks, root.GetProperty("searchedBlocks").GetInt64());
        Assert.Equal(transitions, root.GetProperty("candidateEvaluations").GetInt64());
        Assert.Equal(changes, root.GetProperty("changedCoefficients").GetInt64());
        Assert.True(changes > 0);

        JsonElement comparisons = root.GetProperty("comparisons");
        Assert.Equal(8, comparisons.GetArrayLength());
        foreach (JsonElement comparison in comparisons.EnumerateArray())
        {
            Assert.Contains(comparison.GetProperty("metric").GetString(), new[] { "rgb-psnr-db", "luma-ssim" });
            AssertComparison(comparison, "matchedRateStatus", "matchedRates");
            AssertComparison(comparison, "matchedQualityStatus", "matchedQualities");
        }

        string winnerId = root.GetProperty("winner").GetString()!;
        RateDistortionSearch.Candidate selected = Assert.Single(RateDistortionSearch.GenerateCandidates(),
            candidate => candidate.Id == winnerId);
        var image = PpmImage.Read(fixture.Input);
        Assert.Equal(winner, JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
            selected.CreateOptions(ChromaSubsampling.Quarter), cancellationToken: TestContext.Current.CancellationToken));
        JsonElement winnerTrial = Assert.Single(root.GetProperty("trials").EnumerateArray(),
            trial => trial.GetProperty("id").GetString() == winnerId);
        Assert.True(winnerTrial.GetProperty("eligible").GetBoolean());
        Assert.Equal(winner.Length, winnerTrial.GetProperty("bytes").GetInt32());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(winner)), winnerTrial.GetProperty("sha256").GetString());
        string markdown = File.ReadAllText(Path.Combine(fixture.Output, "report.md"));
        Assert.Contains(winnerId, markdown, StringComparison.Ordinal);
        Assert.Contains("Matched quality:", markdown, StringComparison.Ordinal);
        Assert.Contains("Matched size:", markdown, StringComparison.Ordinal);

        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.Run(fixture.Options,
            _ => throw new InvalidOperationException("Existing output must be rejected before decoding."), new { }));
        Assert.Equal(winner, File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg")));
    }

    [Fact]
    public void MissingUnknownDuplicateAndUnsupportedOptionsAreRejected()
    {
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse([]));
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse(
            ["--input", "unused", "--output", "unused", "--native", "unused", "--native", "unused"]));
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse(
            ["--input", "unused", "--output", "unused", "--native", "unused", "--distance", "1"]));
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse(
            ["--input", "unused", "--output", "unused", "--native", "unused", "--sampling", "422"]));
    }

    [Fact]
    public void ParseAcceptsReorderedArgumentsAndRejectsExistingOutputs()
    {
        using var fixture = new SearchFixture();
        string[] args = ["--sampling", "420", "--native", fixture.Input,
            "--output", fixture.Output, "--input", fixture.Input];
        Assert.Equal(fixture.Options with { Native = fixture.Input }, RateDistortionSearchCommand.SearchOptions.Parse(args));
        File.WriteAllText(fixture.Output, "retained", new UTF8Encoding(false));
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse(args));
        Assert.Equal("retained", File.ReadAllText(fixture.Output));
        File.Delete(fixture.Output);
        _ = Directory.CreateDirectory(fixture.Output);
        _ = Assert.Throws<ArgumentException>(() => RateDistortionSearchCommand.SearchOptions.Parse(args));
    }

    [Fact]
    public void MalformedInputAndDecoderFailureLeaveOutputUncreated()
    {
        using var fixture = new SearchFixture();
        _ = Assert.Throws<InvalidDataException>(() => RateDistortionSearchCommand.Run(fixture.Options,
            _ => throw new InvalidDataException("Decoder failed."), new { }));
        Assert.False(Directory.Exists(fixture.Output));
        _ = Assert.Throws<InvalidDataException>(() => RateDistortionSearchCommand.Run(fixture.Options,
            _ => new PpmImage(1, 1, [0, 0, 0]), new { }));
        Assert.False(Directory.Exists(fixture.Output));
        File.WriteAllText(fixture.Input, "P6\n1 1\n255\n", new UTF8Encoding(false));
        _ = Assert.Throws<InvalidDataException>(() => RateDistortionSearchCommand.Run(fixture.Options,
            _ => throw new InvalidOperationException("Malformed input must fail before decoding."), new { }));
        Assert.False(Directory.Exists(fixture.Output));
    }

    [Fact]
    public void WrongNativeRevisionLeavesOutputUncreatedAndRemovesScratch()
    {
        using var fixture = new SearchFixture();
        string? decoderScratch = null;
        _ = Assert.Throws<InvalidDataException>(() => RateDistortionSearchCommand.Run(
            Arguments(fixture), (executable, workingDirectory, args) =>
            {
                Assert.Equal(typeof(RateDistortionSearchCommand).Assembly.Location, executable);
                Assert.Equal(["version"], args);
                Assert.True(Directory.Exists(workingDirectory));
                decoderScratch = workingDirectory;
                return "unexpected-upstream-revision\n";
            }));
        Assert.False(Directory.Exists(fixture.Output));
        Assert.NotNull(decoderScratch);
        Assert.False(Directory.Exists(decoderScratch));
    }

    [Fact]
    public void NativeDecodeFailureRemovesCandidateScratchAndLeavesOutputUncreated()
    {
        using var fixture = new SearchFixture();
        string? decoderScratch = null;
        _ = Assert.Throws<InvalidDataException>(() => RateDistortionSearchCommand.Run(
            Arguments(fixture), (_, workingDirectory, args) =>
            {
                decoderScratch = workingDirectory;
                if (args.SequenceEqual(["version"]))
                {
                    return "031a0077f5799a6041004267fc12b956c1f52a20\n";
                }
                Assert.Equal("decode", args[0]);
                Assert.True(File.Exists(args[1]));
                File.WriteAllText(args[2], "incomplete decoded output", new UTF8Encoding(false));
                throw new InvalidDataException("Decoder failed.");
            }));
        Assert.False(Directory.Exists(fixture.Output));
        Assert.NotNull(decoderScratch);
        Assert.False(Directory.Exists(decoderScratch));
    }

    private static string[] Arguments(SearchFixture fixture) =>
        ["--input", fixture.Input, "--output", fixture.Output,
            "--native", typeof(RateDistortionSearchCommand).Assembly.Location, "--sampling", "420"];

    private static void AssertComparison(JsonElement comparison, string statusProperty, string pointsProperty)
    {
        string status = comparison.GetProperty(statusProperty).GetString()!;
        if (status == "supported")
        {
            Assert.Equal(5, comparison.GetProperty(pointsProperty).GetArrayLength());
        }
        else
        {
            Assert.StartsWith("unsupported: ", status, StringComparison.Ordinal);
            Assert.Equal(0, comparison.GetProperty(pointsProperty).GetArrayLength());
        }
    }

    private static PpmImage Decode(byte[] jpeg)
    {
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(decoded.Width, decoded.Height, decoded.Data);
    }

    private sealed class SearchFixture : IDisposable
    {
        private readonly string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-rate-distortion-command-" + Guid.NewGuid().ToString("N"));

        internal SearchFixture()
        {
            _ = Directory.CreateDirectory(this.scratch);
            this.Input = Path.Combine(this.scratch, "source.ppm");
            this.Output = Path.Combine(this.scratch, "result");
            byte[] pixels = Enumerable.Range(0, 9 * 5 * 3).Select(index => (byte)(index * 31 % 256)).ToArray();
            new PpmImage(9, 5, pixels).Write(this.Input);
        }

        internal string Input { get; }
        internal string Output { get; }
        internal RateDistortionSearchCommand.SearchOptions Options => new(this.Input, this.Output, "unused-native", ChromaSubsampling.Quarter);

        public void Dispose()
        {
            if (Directory.Exists(this.Output))
            {
                foreach (string file in Directory.EnumerateFiles(this.Output))
                {
                    File.Delete(file);
                }
                Directory.Delete(this.Output);
            }
            else
            {
                File.Delete(this.Output);
            }
            File.Delete(this.Input);
            Directory.Delete(this.scratch);
        }
    }
}
