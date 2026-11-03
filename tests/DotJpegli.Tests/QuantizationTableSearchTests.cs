namespace DotJpegli.Tests;

using System.Security.Cryptography;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class QuantizationTableSearchTests
{
    private static readonly int[] ZigZag =
    [
        0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
    ];

    private static readonly double[] Scales = [.8, 1, 1.25];

    [Fact]
    public void CachedBandCostsMatchIndependentCosineTransformAndAmplitudeCategories()
    {
        byte[] pixels = new byte[8 * 8 * 3];
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                pixels.AsSpan(((row * 8) + column) * 3, 3).Fill(column % 2 == 0 ? (byte)32 : (byte)224);
            }
        }

        ushort[][] tables = Enumerable.Range(0, 3).Select(_ => Enumerable.Repeat((ushort)17, 64).ToArray()).ToArray();
        (IReadOnlyList<QuantizationTableSearch.BandCost> costs, long sourceBlocks) = QuantizationTableSearch.CalculateBandCosts(new(8, 8, pixels), tables,
            ChromaSubsampling.None, TestContext.Current.CancellationToken);
        Assert.Equal(3, sourceBlocks);
        Assert.Equal(18, costs.Count);
        foreach (QuantizationTableSearch.BandCost cost in costs)
        {
            Assert.Equal(Scales[cost.ScaleIndex], cost.Scale);
            int start = cost.Band switch { 0 => 1, 1 => 6, _ => 21 };
            int end = cost.Band switch { 0 => 5, 1 => 20, _ => 63 };
            double distortion = 0, rate = 0;
            for (int zig = start; zig <= end; zig++)
            {
                int frequency = ZigZag[zig], u = frequency % 8, v = frequency / 8;
                double coefficient = 0;
                if (cost.Group == 0)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            coefficient += (x % 2 == 0 ? -96 : 96)
                                * Math.Cos(((2 * x) + 1) * u * Math.PI / 16)
                                * Math.Cos(((2 * y) + 1) * v * Math.PI / 16);
                        }
                    }
                    coefficient *= .25 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1);
                }

                int quantizer = (int)Math.Round(17 * cost.Scale, MidpointRounding.AwayFromZero);
                int rounded = (int)Math.Round(coefficient / quantizer, MidpointRounding.AwayFromZero);
                double error = coefficient - (rounded * quantizer);
                distortion += error * error;
                if (rounded != 0)
                {
                    int amplitude = Math.Abs(rounded), category = 0;
                    while (amplitude != 0)
                    {
                        category++;
                        amplitude >>= 1;
                    }
                    rate += 1 + category;
                }
            }
            Assert.InRange(Math.Abs(distortion - cost.DistortionCost), 0, .05);
            Assert.Equal(rate, cost.RateCost);
        }
    }

    [Fact]
    public void JointFamilyKeepsDcAndSharesEachChromaBandChoiceWithoutMutatingBaseTables()
    {
        ushort[][] tables = Enumerable.Range(0, 3).Select(component =>
            Enumerable.Range(0, 64).Select(frequency => (ushort)(19 + (component * 13) + frequency)).ToArray()).ToArray();
        tables[1][63] = 255;
        tables[2][63] = 1;
        ushort[] original = tables.SelectMany(table => table).ToArray();
        (IReadOnlyList<QuantizationTableSearch.BandCost> costs, _) = QuantizationTableSearch.CalculateBandCosts(new(1, 1, [97, 103, 117]), tables,
            ChromaSubsampling.None, TestContext.Current.CancellationToken);
        IReadOnlyList<QuantizationTableSearch.Candidate> candidates = QuantizationTableSearch.GenerateCandidates(tables, costs);
        Assert.Equal(729, candidates.Count);
        Assert.Equal(729, candidates.Select(candidate => candidate.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("control", candidates[0].Id);
        Assert.Equal(original, candidates[0].Tables.SelectMany(table => table));
        foreach (QuantizationTableSearch.Candidate candidate in candidates)
        {
            for (int component = 0; component < 3; component++)
            {
                Assert.Equal(tables[component][0], candidate.Tables[component][0]);
                Assert.All(candidate.Tables[component], quantizer => Assert.InRange(quantizer, (ushort)1, (ushort)255));
            }
            for (int group = 0; group < 2; group++)
            {
                for (int band = 0; band < 3; band++)
                {
                    int start = band switch { 0 => 1, 1 => 6, _ => 21 };
                    int end = band switch { 0 => 5, 1 => 20, _ => 63 };
                    Assert.Contains(Scales, scale => Enumerable.Range(group == 0 ? 0 : 1, group == 0 ? 1 : 2)
                        .All(component => Enumerable.Range(start, end - start + 1).All(zig =>
                            candidate.Tables[component][ZigZag[zig]] == Math.Clamp(
                                Math.Round(tables[component][ZigZag[zig]] * scale, MidpointRounding.AwayFromZero), 1, 255))));
                }
            }
        }
        Assert.Equal(original, tables.SelectMany(table => table));
        Assert.Contains(candidates, candidate => !candidate.Tables[0].SequenceEqual(tables[0])
            && !candidate.Tables[1].SequenceEqual(tables[1]));
        IReadOnlyList<QuantizationTableSearch.Candidate> shortlist = QuantizationTableSearch.Shortlist(candidates);
        Assert.Equal("control", shortlist[0].Id);
        Assert.InRange(shortlist.Count, 2, 17);
        Assert.Equal(shortlist.Count, shortlist.Select(candidate => candidate.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(shortlist.Select(candidate => candidate.Id), QuantizationTableSearch.Shortlist(candidates).Select(candidate => candidate.Id));
    }

    [Theory]
    [InlineData(ChromaSubsampling.None, 1f)]
    [InlineData(ChromaSubsampling.None, 4f)]
    [InlineData(ChromaSubsampling.Quarter, 1f)]
    [InlineData(ChromaSubsampling.Quarter, 4f)]
    public void EveryFinalistReproducesActualBytesMetricsAndConstrainedMinimum(ChromaSubsampling sampling, float distance)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24), original = (byte[])pixels.Clone();
        var image = new PpmImage(width, height, pixels);
        int decodeCount = 0;
        QuantizationTableSearch.SearchResult result = QuantizationTableSearch.Search(image, distance, sampling, jpeg =>
        {
            decodeCount++;
            return Decode(jpeg);
        }, TestContext.Current.CancellationToken);
        Assert.Equal(original, pixels);
        Assert.Equal(result.EncodeCount, decodeCount);
        Assert.InRange(result.EncodeCount, 2, 17);
        Assert.Equal(729, result.Candidates.Count);
        Assert.Equal(18, result.BandCosts.Count);
        Assert.Equal(sampling == ChromaSubsampling.None ? 18 : 10, result.SourceBlockCount);
        byte[] control = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24, Options(distance, sampling),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("control", result.Trials[0].Id);
        Assert.Equal(control.Length, result.Trials[0].Bytes);
        Assert.Equal(Hash(control), result.Trials[0].Sha256);
        var structure = JpegDocument.Read(control, progressive: false, expectedInterval: 0);
        for (int component = 0; component < 3; component++)
        {
            Assert.Equal(result.BaseTables[component].Select((_, zig) => (byte)result.BaseTables[component][ZigZag[zig]]),
                structure.Quantizers[component]);
        }

        var controlMetrics = QualityMetrics.Compare(image, Decode(control));
        var eligible = new List<(QuantizationTableSearch.Trial Trial, byte[] Jpeg)>();
        foreach (QuantizationTableSearch.Trial trial in result.Trials)
        {
            QuantizationTableSearch.Candidate candidate = Assert.Single(result.Candidates, item => item.Id == trial.Id);
            JpegliEncoderOptions options = Options(distance, sampling) with
            {
                QuantizationTables = candidate.Id == "control" ? null : candidate.Tables.Select(table => (ReadOnlyMemory<ushort>)table).ToArray()
            };
            byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, PixelLayout.Rgb24, options,
                cancellationToken: TestContext.Current.CancellationToken);
            PpmImage decoded = Decode(jpeg);
            var metrics = QualityMetrics.Compare(image, decoded);
            Assert.Equal(jpeg.Length, trial.Bytes);
            Assert.Equal(Hash(jpeg), trial.Sha256);
            Assert.Equal(Hash(decoded.Pixels), trial.DecodedSha256);
            Assert.Equal(metrics, trial.Metrics);
            bool accepted = metrics.RgbPsnrDb >= controlMetrics.RgbPsnrDb && metrics.LumaSsim >= controlMetrics.LumaSsim;
            Assert.Equal(accepted, trial.Eligible);
            if (accepted)
            {
                eligible.Add((trial, jpeg));
            }
        }
        (QuantizationTableSearch.Trial winner, byte[] winnerJpeg) = eligible.OrderBy(item => item.Trial.Bytes).First();
        Assert.Equal(winner.Id, result.WinnerId);
        Assert.Equal(winnerJpeg, result.Jpeg);
        Assert.True(result.Jpeg.Length <= control.Length);
    }

    [Fact]
    public void ShortlistNormalizesCostsKeepsTradeoffsAndResolvesTiesInDeclaredOrder()
    {
        QuantizationTableSearch.Candidate[] candidates =
        [
            new("control", [], 100, 10),
            new("distortion-heavy", [], 1000, 0),
            new("rate-heavy", [], 0, 100),
            new("a", [], 25, 20),
            new("b", [], 50, 10),
            new("c", [], 100, 5),
            new("d", [], 200, 2.5),
            new("control-tie", [], 100, 10),
            new("dominated", [], 10000, 1000)
        ];
        Assert.Equal(["control", "a", "b", "c", "control-tie", "d"],
            QuantizationTableSearch.Shortlist(candidates).Select(candidate => candidate.Id));
        QuantizationTableSearch.Candidate[] tied = Enumerable.Range(0, 7)
            .Select(index => new QuantizationTableSearch.Candidate(index == 0 ? "control" : $"tie-{index}", [], 0, 0)).ToArray();
        Assert.Equal(["control", "tie-1", "tie-2", "tie-3"],
            QuantizationTableSearch.Shortlist(tied).Select(candidate => candidate.Id));
    }

    [Fact]
    public void ExactQualityGateRejectsEveryDegradedFinalistAndKeepsControl()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        int calls = 0;
        QuantizationTableSearch.SearchResult result = QuantizationTableSearch.Search(image, 1, ChromaSubsampling.None,
            _ => calls++ == 0 ? image : new(1, 1, [0, 0, 0]), TestContext.Current.CancellationToken);
        Assert.Equal("control", result.WinnerId);
        Assert.True(result.Trials[0].Eligible);
        Assert.All(result.Trials.Skip(1), trial => Assert.False(trial.Eligible));
        Assert.Equal(result.EncodeCount, calls);
    }

    [Fact]
    public void CancellationStopsBeforeWorkAndAfterTheFirstDecoderCall()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        int calls = 0;
        _ = Assert.Throws<OperationCanceledException>(() => QuantizationTableSearch.Search(image, 1, ChromaSubsampling.None,
            _ => { calls++; return image; }, cancellation.Token));
        Assert.Equal(0, calls);
        using var between = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        _ = Assert.Throws<OperationCanceledException>(() => QuantizationTableSearch.Search(image, 1, ChromaSubsampling.None,
            _ => { calls++; between.Cancel(); return image; }, between.Token));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void DecoderFailuresAndChangedDimensionsStopSearch()
    {
        var image = new PpmImage(1, 1, [71, 113, 197]);
        var failure = new InvalidDataException("decode failure");
        int calls = 0;
        Assert.Same(failure, Assert.Throws<InvalidDataException>(() => QuantizationTableSearch.Search(image, 1,
            ChromaSubsampling.None, _ => { calls++; throw failure; }, TestContext.Current.CancellationToken)));
        Assert.Equal(1, calls);
        _ = Assert.Throws<InvalidDataException>(() => QuantizationTableSearch.Search(image, 1,
            ChromaSubsampling.None, _ => new(2, 1, new byte[6]), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(25.01f)]
    public void InvalidDistancesFailBeforeDecoder(float distance) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => QuantizationTableSearch.Search(new(1, 1, [1, 2, 3]), distance,
            ChromaSubsampling.None, _ => throw new InvalidOperationException("Decoder must not run."), TestContext.Current.CancellationToken));

    [Fact]
    public void UnsupportedSamplingAndMalformedImagesFailBeforeDecoder()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => QuantizationTableSearch.Search(new(1, 1, [1, 2, 3]), 1,
            ChromaSubsampling.HalfHorizontal, _ => throw new InvalidOperationException("Decoder must not run."), TestContext.Current.CancellationToken));
        foreach (PpmImage image in new PpmImage[] { new(0, 1, []), new(65536, 1, []), new(1, 1, []), new(1, 1, [1, 2, 3, 4]) })
        {
            _ = Assert.ThrowsAny<ArgumentException>(() => QuantizationTableSearch.Search(image, 1,
                ChromaSubsampling.None, _ => throw new InvalidOperationException("Decoder must not run."), TestContext.Current.CancellationToken));
        }
    }

    private static JpegliEncoderOptions Options(float distance, ChromaSubsampling sampling) => new()
    {
        ColorSpace = JpegColorSpace.YCbCr,
        Distance = distance,
        Subsampling = sampling,
        ProgressiveLevel = 0,
        AdaptiveQuantization = true,
        OptimizeHuffmanTables = true,
        ForceBaseline = true,
        MaxDegreeOfParallelism = 1
    };

    private static PpmImage Decode(byte[] jpeg)
    {
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(decoded.Width, decoded.Height, decoded.Data);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
