namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;

public sealed class HuffmanOrderingSearchCommandTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ReportRetainsEveryTrialAndReproducesTheSelectedOrdering(int progression)
    {
        using var fixture = new Fixture();
        HuffmanOrderingSearchCommand.SearchOptions options = fixture.Options with { Progression = progression };
        Assert.Equal(0, HuffmanOrderingSearchCommand.Run(options, Decode, new { name = "StbImageSharp" }));
        byte[] jpeg = File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture.Output, "search.json")));
        JsonElement report = json.RootElement;
        Assert.Equal(4, report.GetProperty("encodeCount").GetInt32());
        Assert.Equal(4, report.GetProperty("decodeCount").GetInt32());
        Assert.True(report.GetProperty("unchangedDecodedPixels").GetBoolean());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture.Input))),
            report.GetProperty("input").GetProperty("sha256").GetString());
        JsonElement[] trials = report.GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(4, trials.Length);
        string winner = report.GetProperty("winner").GetString()!;
        JsonElement selected = Assert.Single(trials, trial => trial.GetProperty("id").GetString() == winner);
        Assert.Equal(jpeg.Length, selected.GetProperty("trial").GetProperty("bytes").GetInt32());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(jpeg)), selected.GetProperty("trial").GetProperty("sha256").GetString());
        _ = Assert.Single(trials.Select(trial => trial.GetProperty("trial").GetProperty("decodedPixelSha256").GetString()).Distinct());
        var image = PpmImage.Read(fixture.Input);
        HuffmanOrderingSearch.Candidate candidate = Assert.Single(HuffmanOrderingSearch.Candidates, item => item.Id == winner);
        Assert.Equal(jpeg, JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
            candidate.CreateOptions(1, ChromaSubsampling.Quarter, progression), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains(winner, File.ReadAllText(Path.Combine(fixture.Output, "report.md")), StringComparison.Ordinal);
        _ = Assert.Throws<ArgumentException>(() => HuffmanOrderingSearchCommand.Run(options,
            _ => throw new InvalidOperationException("Existing output must be rejected first."), new { }));
        Assert.Equal(jpeg, File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg")));
    }

    [Fact]
    public void DecodeFailureAndWrongPinnedVersionLeaveOutputUncreated()
    {
        using var fixture = new Fixture();
        _ = Assert.Throws<InvalidDataException>(() => HuffmanOrderingSearchCommand.Run(fixture.Options,
            _ => throw new InvalidDataException("Decode failed."), new { }));
        Assert.False(Directory.Exists(fixture.Output));
        string[] args = ["--input", fixture.Input, "--output", fixture.Output, "--native", fixture.Input,
            "--distance", "1", "--sampling", "420", "--progression", "0"];
        Assert.Equal(fixture.Options with { Native = fixture.Input }, HuffmanOrderingSearchCommand.SearchOptions.Parse(args));
        _ = Assert.Throws<InvalidDataException>(() => HuffmanOrderingSearchCommand.Run(args, (_, _, _) => "wrong revision"));
        Assert.False(Directory.Exists(fixture.Output));
        args[^1] = "1";
        _ = Assert.Throws<ArgumentException>(() => HuffmanOrderingSearchCommand.SearchOptions.Parse(args));
    }

    private static PpmImage Decode(byte[] jpeg)
    {
        var image = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(image.Width, image.Height, image.Data);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "DotJpegli-huffman-command-" + Guid.NewGuid().ToString("N"));
        internal Fixture()
        {
            _ = Directory.CreateDirectory(this.root);
            this.Input = Path.Combine(this.root, "source.ppm");
            this.Output = Path.Combine(this.root, "result");
            new PpmImage(9, 5, Enumerable.Range(0, 135).Select(index => (byte)(index * 31 % 256)).ToArray()).Write(this.Input);
        }
        internal string Input { get; }
        internal string Output { get; }
        internal HuffmanOrderingSearchCommand.SearchOptions Options => new(this.Input, this.Output, "unused", 1, ChromaSubsampling.Quarter, 0);
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
            File.Delete(this.Input);
            Directory.Delete(this.root);
        }
    }
}
