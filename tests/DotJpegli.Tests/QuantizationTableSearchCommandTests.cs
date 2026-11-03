namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;

public sealed class QuantizationTableSearchCommandTests
{
    [Fact]
    public void ReportRetainsReproducibleTablesQualityAndAllSearchCosts()
    {
        using var fixture = new SearchFixture();
        int decodes = 0;
        Assert.Equal(0, QuantizationTableSearchCommand.Run(fixture.Options, jpeg =>
        {
            decodes++;
            var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
            return new(decoded.Width, decoded.Height, decoded.Data);
        }, new { name = "StbImageSharp test decoder" }));

        byte[] winner = File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture.Output, "search.json")));
        JsonElement root = json.RootElement;
        Assert.Equal(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(18, root.GetProperty("cachedBandCostCount").GetInt32());
        Assert.Equal(729, root.GetProperty("screenedCandidateCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("encodeCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("decodeCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("preparedMetricCount").GetInt32());
        Assert.Equal(2, root.GetProperty("referenceFeaturePreparationCount").GetInt32());
        Assert.Equal(2 * decodes, root.GetProperty("preparedScorerCallCount").GetInt32());
        Assert.InRange(root.GetProperty("referenceMetricValidationCount").GetInt32(), 1, decodes);
        Assert.Equal(2 * root.GetProperty("referenceMetricValidationCount").GetInt32(),
            root.GetProperty("referenceScorerCallCount").GetInt32());
        Assert.Contains("reference scorer", root.GetProperty("metricEvaluation").GetString(), StringComparison.Ordinal);
        Assert.Equal(MultiScaleSsim.MetricId, root.GetProperty("perceptualObjective").GetProperty("id").GetString());
        Assert.Equal(MultiScaleSsim.Definition, root.GetProperty("perceptualObjective").GetProperty("definition").GetString());
        Assert.InRange(decodes, 1, 17);
        Assert.Equal(2 * decodes, root.GetProperty("totalEntropyPassCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("totalHistogramScanCount").GetInt32());
        Assert.Equal(decodes, root.GetProperty("totalOutputScanCount").GetInt32());
        Assert.Equal(18, root.GetProperty("bandCosts").GetArrayLength());
        Assert.Equal(729, root.GetProperty("candidates").GetArrayLength());
        Assert.Equal(decodes, root.GetProperty("trials").GetArrayLength());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture.Input))),
            root.GetProperty("input").GetProperty("sha256").GetString());
        Assert.Equal("StbImageSharp test decoder", root.GetProperty("decoder").GetProperty("name").GetString());

        string winnerId = root.GetProperty("winner").GetString()!;
        JsonElement selected = Assert.Single(root.GetProperty("candidates").EnumerateArray(),
            candidate => candidate.GetProperty("id").GetString() == winnerId);
        ReadOnlyMemory<ushort>[] tables = selected.GetProperty("tables").EnumerateArray()
            .Select(table => new ReadOnlyMemory<ushort>(table.EnumerateArray().Select(value => value.GetUInt16()).ToArray())).ToArray();
        var image = PpmImage.Read(fixture.Input);
        var settings = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = 1,
            Subsampling = ChromaSubsampling.Quarter,
            ProgressiveLevel = 0,
            AdaptiveQuantization = true,
            OptimizeHuffmanTables = true,
            ForceBaseline = true,
            MaxDegreeOfParallelism = 1,
            QuantizationTables = winnerId == "control" ? null : tables
        };
        Assert.Equal(winner, JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
            settings, cancellationToken: TestContext.Current.CancellationToken));
        JsonElement winnerTrial = Assert.Single(root.GetProperty("trials").EnumerateArray(),
            trial => trial.GetProperty("id").GetString() == winnerId);
        Assert.True(winnerTrial.GetProperty("eligible").GetBoolean());
        var decodedWinner = ImageResult.FromMemory(winner, ColorComponents.RedGreenBlue);
        Assert.Equal(MultiScaleSsim.Compare(image, new(decodedWinner.Width, decodedWinner.Height, decodedWinner.Data),
            TestContext.Current.CancellationToken),
            winnerTrial.GetProperty("multiScaleSsim").GetDouble());
        Assert.Equal(winner.Length, winnerTrial.GetProperty("bytes").GetInt32());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(winner)), winnerTrial.GetProperty("sha256").GetString());
        Assert.Contains(winnerId, File.ReadAllText(Path.Combine(fixture.Output, "report.md")), StringComparison.Ordinal);

        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.Run(fixture.Options,
            _ => throw new InvalidOperationException("Existing output must be rejected before decoding."), new { }));
        Assert.Equal(winner, File.ReadAllBytes(Path.Combine(fixture.Output, "winner.jpg")));
    }

    [Theory]
    [InlineData("0", "444")]
    [InlineData("-1", "444")]
    [InlineData("25.01", "420")]
    [InlineData("NaN", "420")]
    [InlineData("Infinity", "420")]
    [InlineData("1,5", "420")]
    [InlineData("1", "422")]
    public void InvalidSearchSettingsAreRejectedBeforeFilesAreRead(string distance, string sampling) =>
        Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse(
            ["--input", "unused.ppm", "--output", "unused-result", "--native", "unused-native", "--distance", distance, "--sampling", sampling]));

    [Fact]
    public void MissingUnknownAndDuplicateOptionsAreRejected()
    {
        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse([]));
        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse(
            ["--input", "unused", "--output", "unused", "--native", "unused", "--distance", "1", "--distance", "1"]));
        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse(
            ["--input", "unused", "--output", "unused", "--native", "unused", "--distance", "1", "--unknown", "420"]));
    }

    [Fact]
    public void ParseAcceptsReorderedOptionsAndRejectsExistingFileAndDirectoryOutputs()
    {
        using var fixture = new SearchFixture();
        string[] args = ["--sampling", "420", "--distance", "1", "--native", fixture.Input,
            "--output", fixture.Output, "--input", fixture.Input];
        Assert.Equal(fixture.Options with { Native = fixture.Input }, QuantizationTableSearchCommand.SearchOptions.Parse(args));
        File.WriteAllText(fixture.Output, "retained", new UTF8Encoding(false));
        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse(args));
        Assert.Equal("retained", File.ReadAllText(fixture.Output));
        File.Delete(fixture.Output);
        _ = Directory.CreateDirectory(fixture.Output);
        _ = Assert.Throws<ArgumentException>(() => QuantizationTableSearchCommand.SearchOptions.Parse(args));
    }

    [Fact]
    public void MalformedInputAndDecoderFailureLeaveOutputUncreated()
    {
        using var fixture = new SearchFixture();
        _ = Assert.Throws<InvalidDataException>(() => QuantizationTableSearchCommand.Run(fixture.Options,
            _ => throw new InvalidDataException("Decoder failed."), new { }));
        Assert.False(Directory.Exists(fixture.Output));
        _ = Assert.Throws<InvalidDataException>(() => QuantizationTableSearchCommand.Run(fixture.Options,
            _ => new PpmImage(1, 1, [0, 0, 0]), new { }));
        Assert.False(Directory.Exists(fixture.Output));
        File.WriteAllText(fixture.Input, "P6\n1 1\n255\n", new UTF8Encoding(false));
        _ = Assert.Throws<InvalidDataException>(() => QuantizationTableSearchCommand.Run(fixture.Options,
            _ => throw new InvalidOperationException("Malformed input must fail before decoding."), new { }));
        Assert.False(Directory.Exists(fixture.Output));
    }

    private sealed class SearchFixture : IDisposable
    {
        private readonly string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-quantization-command-" + Guid.NewGuid().ToString("N"));

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
        internal QuantizationTableSearchCommand.SearchOptions Options => new(this.Input, this.Output, "unused-native", 1, ChromaSubsampling.Quarter);

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
