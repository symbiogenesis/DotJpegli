namespace DotJpegli.Tests;

using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Internal;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class ColorPreprocessingTests
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void ReconstructionMustImproveItsDeclaredModelAndKeepsOriginalLuma()
    {
        PpmImage image = Pattern(17, 13);
        byte[] original = image.Pixels.ToArray();
        ColorPreprocessing.Prepared prepared = Assert.IsType<ColorPreprocessing.Prepared>(ColorPreprocessing.Reconstruct420(image, 4, Cancellation));
        Assert.True(prepared.AfterError is { } after && prepared.BeforeError is { } before && after < before);
        Assert.Equal(4, prepared.Iterations);
        Assert.Equal(PixelComponentLayout.YCbCr, prepared.Layout);
        Assert.Equal(image.Pixels.Length, prepared.Pixels.Length);
        for (int p = 0; p < image.Pixels.Length; p += 3)
        {
            ColorConversion.RgbToYCbCr(image.Pixels[p], image.Pixels[p + 1], image.Pixels[p + 2], out float luma, out _, out _);
            Assert.Equal(BitConverter.SingleToInt32Bits(luma / 255f), BitConverter.SingleToInt32Bits(prepared.Pixels[p]));
        }
        Assert.All(prepared.Pixels, value => Assert.True(float.IsFinite(value) && value is >= 0 and <= 1));
        Assert.Equal(original, image.Pixels);
        Assert.Null(ColorPreprocessing.Reconstruct420(new(3, 3, Enumerable.Repeat((byte)128, 27).ToArray()), 4, Cancellation));
        Assert.Throws<ArgumentOutOfRangeException>(() => ColorPreprocessing.Reconstruct420(image, 5, Cancellation));
    }

    [Fact]
    public void SaturatedRunOvershootIsBoundedComponentSafeAndPreservesGrayEdges()
    {
        byte[] pixels = Enumerable.Range(0, 8).SelectMany(x => Enumerable.Repeat(x < 4 ? (byte)0 : (byte)255, 3)).ToArray();
        ColorPreprocessing.Prepared prepared = Assert.IsType<ColorPreprocessing.Prepared>(ColorPreprocessing.Dering(new(8, 1, pixels), 2, Cancellation));
        Assert.Equal(6, prepared.ChangedSamples);
        Assert.Null(prepared.BeforeError);
        for (int x = 0; x < 8; x++)
        {
            float expected = x == 3 ? -2 / 255f : x == 4 ? 257 / 255f : pixels[x * 3] / 255f;
            for (int channel = 0; channel < 3; channel++)
            {
                Assert.Equal(expected, prepared.Pixels[(x * 3) + channel]);
            }
        }
        Assert.Null(ColorPreprocessing.Dering(new(3, 1, [128, 5, 17, 128, 5, 17, 128, 5, 17]), 2, Cancellation));
        Assert.Throws<ArgumentOutOfRangeException>(() => ColorPreprocessing.Dering(new(1, 1, [0, 0, 0]), 4.01, Cancellation));
    }

    [Fact]
    public void SamplingEarlyExitUsesAnExactNonnegativeFullImageLowerBound()
    {
        Assert.False(ColorPreprocessing.AdviseSampling(Pattern(33, 65), true, Cancellation).Complete);
        foreach (PpmImage image in new[] { Pattern(33, 65), new PpmImage(1, 1, [128, 128, 128]),
            new PpmImage(33, 65, Enumerable.Range(0, 33 * 65).SelectMany(index => Enumerable.Repeat((byte)(index & 255), 3)).ToArray()) })
        {
            ColorPreprocessing.SamplingAdvice full = ColorPreprocessing.AdviseSampling(image, false, Cancellation);
            ColorPreprocessing.SamplingAdvice early = ColorPreprocessing.AdviseSampling(image, true, Cancellation);
            long expected = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int p = ((y * image.Width) + x) * 3;
                    foreach (int q in new[] { x + 1 < image.Width ? p + 3 : -1,
                        y + 1 < image.Height ? p + (image.Width * 3) : -1 })
                    {
                        if (q >= 0)
                        {
                            int r = image.Pixels[p] - image.Pixels[q], g = image.Pixels[p + 1] - image.Pixels[q + 1], b = image.Pixels[p + 2] - image.Pixels[q + 2];
                            expected += Math.Abs((2 * b) - r - g) + Math.Abs((2 * r) - g - b);
                        }
                    }
                }
            }
            Assert.True(full.Complete);
            Assert.Equal(full.TotalEdges, full.VisitedEdges);
            Assert.Equal(expected, full.ChromaEdgeSum);
            Assert.Equal(full.Recommendation, early.Recommendation);
            Assert.InRange(early.ChromaEdgeSum, 0, full.ChromaEdgeSum);
            Assert.InRange(early.VisitedEdges, 0, full.TotalEdges);
            if (!early.Complete)
            {
                Assert.Equal("444", early.Recommendation);
                Assert.True(early.ChromaEdgeSum > early.Threshold * early.TotalEdges);
            }
        }
    }

    [Fact]
    public void EqualSizedEligibleOutputsKeepTheOriginalControl()
    {
        var image = new PpmImage(3, 3, Enumerable.Repeat((byte)128, 27).ToArray());
        ColorPreprocessingSearch.SearchResult result = ColorPreprocessingSearch.Search(image, 1, ChromaSubsampling.Quarter, Decode, Cancellation);
        Assert.Equal("control", result.WinnerId);
        Assert.Equal(3, result.Trials.Count);
        Assert.All(result.Trials, trial =>
        {
            Assert.True(trial.Eligible);
            Assert.Equal(result.Trials[0].Bytes, trial.Bytes);
        });
    }

    [Theory]
    [InlineData(ChromaSubsampling.None)]
    [InlineData(ChromaSubsampling.Quarter)]
    public void EveryActualTrialReproducesBytesAndThreeFloorConstrainedMinimum(ChromaSubsampling sampling)
    {
        PpmImage image = Pattern(17, 13);
        byte[] original = image.Pixels.ToArray();
        int decodes = 0;
        ColorPreprocessingSearch.SearchResult result = ColorPreprocessingSearch.Search(image, 1, sampling, jpeg =>
        {
            decodes++;
            return Decode(jpeg);
        }, Cancellation);
        Assert.Equal("control", result.Trials[0].Candidate.Id);
        Assert.Equal(result.Trials.Count, decodes);
        Assert.Equal(result.Candidates.Count, result.Trials.Count + result.Rejections.Count);
        Assert.InRange(result.Trials.Count, 3, 15);
        ColorPreprocessingSearch.Trial control = result.Trials[0];
        foreach (ColorPreprocessingSearch.Trial trial in result.Trials)
        {
            ColorPreprocessing.Prepared? prepared = ColorPreprocessingSearch.Prepare(image, trial.Candidate, sampling, Cancellation);
            JpegliEncoderOptions options = ColorPreprocessingSearch.Options(trial.Candidate, sampling);
            byte[] jpeg = prepared is null ? JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, options, cancellationToken: Cancellation)
                : JpegliEncoder.Encode(prepared.Pixels, image.Width, image.Height, prepared.Layout, options, cancellationToken: Cancellation);
            PpmImage decoded = Decode(jpeg);
            var metrics = QualityMetrics.Compare(image, decoded);
            double structural = MultiScaleSsim.Compare(image, decoded, Cancellation);
            Assert.Equal(trial.Sha256, Hash(jpeg));
            Assert.Equal(trial.DecodedSha256, Hash(decoded.Pixels));
            Assert.Equal(trial.Bytes, jpeg.Length);
            Assert.Equal(trial.Metrics, metrics);
            Assert.Equal(trial.MultiScaleSsim, structural);
            Assert.Equal(metrics.RgbPsnrDb >= control.Metrics.RgbPsnrDb && metrics.LumaSsim >= control.Metrics.LumaSsim
                && structural >= control.MultiScaleSsim, trial.Eligible);
            Component component = JpegDocument.Read(jpeg, false, 0).Components[1];
            Assert.Equal(sampling == ChromaSubsampling.Quarter ? 2 : 1, component.H);
            Assert.Equal(sampling == ChromaSubsampling.Quarter ? 2 : 1, component.V);
        }
        ColorPreprocessingSearch.Trial winner = result.Trials.Where(trial => trial.Eligible).OrderBy(trial => trial.Bytes).First();
        Assert.Equal(winner.Candidate.Id, result.WinnerId);
        Assert.Equal(winner.Sha256, Hash(result.Jpeg));
        Assert.Equal(original, image.Pixels);
        Assert.InRange(result.ReferenceMetricValidationCount, 1, result.Trials.Count);
    }

    [Fact]
    public void BoundedSearchRejectsInvalidOrCanceledWorkBeforeDecoding()
    {
        Assert.Throws<ArgumentException>(() => ColorPreprocessingSearch.Search(new(1025, 1, new byte[3075]), 1,
            ChromaSubsampling.Quarter, _ => throw new InvalidOperationException(), Cancellation));
        Assert.Throws<OperationCanceledException>(() => ColorPreprocessingSearch.Search(Pattern(3, 3), 1,
            ChromaSubsampling.Quarter, _ => throw new InvalidOperationException(), new CancellationToken(true)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ColorPreprocessingSearch.GenerateCandidates(float.NaN, ChromaSubsampling.Quarter));
        Assert.Throws<ArgumentOutOfRangeException>(() => ColorPreprocessingSearch.GenerateCandidates(1, ChromaSubsampling.HalfHorizontal));
    }

    [Fact]
    public void OutOfRangeOvershootIsRejectedWhileControlAndDecoderFailuresRemainVisible()
    {
        byte[] pixels = Enumerable.Range(0, 16 * 8).SelectMany(index =>
            Enumerable.Repeat(index % 16 < 8 ? (byte)0 : (byte)255, 3)).ToArray();
        var image = new PpmImage(16, 8, pixels);
        var candidate = new ColorPreprocessingSearch.Candidate("overshoot", .01f,
            ColorPreprocessingSearch.Kind.SaturatedRunOvershoot, 0, 1);
        ColorPreprocessing.Prepared prepared = Assert.IsType<ColorPreprocessing.Prepared>(
            ColorPreprocessingSearch.Prepare(image, candidate, ChromaSubsampling.None, Cancellation));
        ArgumentException error = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(prepared.Pixels,
            image.Width, image.Height, prepared.Layout, ColorPreprocessingSearch.Options(candidate, ChromaSubsampling.None),
            cancellationToken: Cancellation));
        Assert.Equal("pixels", error.ParamName);
        ColorPreprocessingSearch.SearchResult result = ColorPreprocessingSearch.Search(image, .01f,
            ChromaSubsampling.None, Decode, Cancellation);
        Assert.Equal("control", result.WinnerId);
        Assert.Contains(result.Rejections, rejection => rejection.Stage == "coefficient-validation");
        Assert.All(result.Trials, trial => Assert.True(trial.Eligible));
        var decoderFailure = new ArgumentException("Decoder failed.", "pixels");
        Assert.Same(decoderFailure, Assert.Throws<ArgumentException>(() => ColorPreprocessingSearch.Search(image, .01f,
            ChromaSubsampling.None, _ => throw decoderFailure, Cancellation)));
    }

    [Fact]
    public void CommandWritesCompleteEvidenceAndKeepsExistingOutputOnFailure()
    {
        string directory = Path.Combine(Path.GetTempPath(), "DotJpegli-color-test-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(directory);
        string input = Path.Combine(directory, "input.ppm"), output = Path.Combine(directory, "result");
        try
        {
            Pattern(9, 7).Write(input);
            var options = new ColorPreprocessingSearchCommand.SearchOptions(input, output, input, 1, ChromaSubsampling.Quarter);
            Assert.Throws<InvalidDataException>(() => ColorPreprocessingSearchCommand.Run(options,
                _ => throw new InvalidDataException("test failure"), new { decoder = "test" }));
            Assert.False(Directory.Exists(output));
            Assert.Equal(0, ColorPreprocessingSearchCommand.Run(options, Decode, new { decoder = "StbImageSharp test" }));
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "search.json")));
            JsonElement report = json.RootElement;
            Assert.Equal(1, report.GetProperty("schemaVersion").GetInt32());
            Assert.Equal(420, report.GetProperty("settings").GetProperty("sampling").GetInt32());
            Assert.Equal(report.GetProperty("encodeCount").GetInt32(), report.GetProperty("trials").GetArrayLength());
            Assert.Equal(report.GetProperty("encodeCount").GetInt32(), report.GetProperty("decodeCount").GetInt32());
            byte[] winner = File.ReadAllBytes(Path.Combine(output, "winner.jpg"));
            string id = report.GetProperty("winner").GetString()!;
            JsonElement selected = Assert.Single(report.GetProperty("trials").EnumerateArray(),
                trial => trial.GetProperty("candidate").GetProperty("id").GetString() == id);
            Assert.Equal(Hash(winner), selected.GetProperty("sha256").GetString());
            Assert.True(selected.GetProperty("eligible").GetBoolean());
            Assert.Throws<ArgumentException>(() => ColorPreprocessingSearchCommand.Run(options, Decode, new { }));
            Assert.Equal(winner, File.ReadAllBytes(Path.Combine(output, "winner.jpg")));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static PpmImage Pattern(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int p = ((y * width) + x) * 3;
                pixels[p] = x / 3 % 2 == 0 ? (byte)255 : (byte)0;
                pixels[p + 1] = y / 3 % 2 == 0 ? (byte)0 : (byte)255;
                pixels[p + 2] = (byte)(((x * 17) + (y * 31)) & 255);
            }
        }
        return new(width, height, pixels);
    }

    private static PpmImage Decode(byte[] jpeg)
    {
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(decoded.Width, decoded.Height, decoded.Data);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
