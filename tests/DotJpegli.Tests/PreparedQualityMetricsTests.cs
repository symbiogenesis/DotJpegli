namespace DotJpegli.Tests;

using DotJpegli.Parity;
using Xunit;

public sealed class PreparedQualityMetricsTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 19)]
    [InlineData(23, 1)]
    [InlineData(3, 5)]
    [InlineData(8, 8)]
    [InlineData(9, 7)]
    [InlineData(17, 19)]
    [InlineData(32, 24)]
    public void PreparedScoresMatchIndependentReferenceBitsAcrossRastersAndDistortions(int width, int height)
    {
        var random = new Random((width * 100) + height);
        for (int pattern = 0; pattern < 6; pattern++)
        {
            byte[] pixels = new byte[width * height * 3];
            random.NextBytes(pixels);
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = pattern switch
                {
                    0 => 0,
                    1 => 255,
                    2 => 117,
                    3 => (byte)((index & 1) == 0 ? 0 : 255),
                    4 => (byte)(127 + (pixels[index] % 3)),
                    _ => pixels[index]
                };
            }
            var source = new PpmImage(width, height, pixels);
            PreparedQualityMetrics prepared = QualityMetrics.Prepare(source);
            for (int distortion = 0; distortion < 5; distortion++)
            {
                byte[] decodedPixels = pixels.ToArray();
                for (int index = 0; index < decodedPixels.Length; index++)
                {
                    decodedPixels[index] = distortion switch
                    {
                        0 => decodedPixels[index],
                        1 => (byte)Math.Min(255, decodedPixels[index] + 1),
                        2 => (byte)Math.Max(0, decodedPixels[index] - 10),
                        3 => (byte)(255 - decodedPixels[index]),
                        _ => (byte)random.Next(256)
                    };
                }
                var decoded = new PpmImage(width, height, decodedPixels);
                AssertBits(QualityMetrics.Compare(source, decoded), prepared.Compare(decoded));
            }
        }
    }

    [Fact]
    public void PreparationOwnsItsSourceSnapshotAndComparisonsObserveCurrentDecodedPixels()
    {
        byte[] pixels = [5, 11, 19, 231, 117, 31];
        var source = new PpmImage(2, 1, pixels);
        var snapshot = new PpmImage(2, 1, pixels.ToArray());
        var decoded = new PpmImage(2, 1, pixels.ToArray());
        PreparedQualityMetrics prepared = QualityMetrics.Prepare(source);
        pixels.AsSpan().Fill(255);
        AssertBits(QualityMetrics.Compare(snapshot, decoded), prepared.Compare(decoded));
        decoded.Pixels.AsSpan().Clear();
        AssertBits(QualityMetrics.Compare(snapshot, decoded), prepared.Compare(decoded));
        Assert.Equal(new byte[] { 5, 11, 19, 231, 117, 31 }, snapshot.Pixels);
        Assert.All(pixels, value => Assert.Equal((byte)255, value));
    }

    [Fact]
    public async Task PreparedReferenceSupportsConcurrentIndependentComparisons()
    {
        const int width = 17, height = 11;
        var random = new Random(817);
        byte[] pixels = new byte[width * height * 3];
        random.NextBytes(pixels);
        var source = new PpmImage(width, height, pixels);
        PreparedQualityMetrics prepared = QualityMetrics.Prepare(source);
        PpmImage[] decoded = Enumerable.Range(0, 8).Select(_ =>
        {
            byte[] raster = new byte[pixels.Length];
            random.NextBytes(raster);
            return new PpmImage(width, height, raster);
        }).ToArray();
        QualityMetrics[] expected = decoded.Select(image => QualityMetrics.Compare(source, image)).ToArray();
        QualityMetrics[] actual = await Task.WhenAll(decoded.Select(image => Task.Run(() => prepared.Compare(image),
            TestContext.Current.CancellationToken)));
        for (int index = 0; index < decoded.Length; index++)
        {
            AssertBits(expected[index], actual[index]);
        }
    }

    [Fact]
    public void InvalidRastersAndChangedDimensionsAreRejectedBeforeScoring()
    {
        _ = Assert.Throws<ArgumentNullException>(() => QualityMetrics.Prepare(null!));
        PpmImage[] invalid =
        [
            new(0, 1, []), new(1, 0, []), new(-1, 1, []), new(1, -1, []),
            new(1, 1, null!), new(1, 1, []), new(1, 1, [0, 0]),
            new(1, 1, [0, 0, 0, 0]), new(int.MaxValue, int.MaxValue, [0, 0, 0])
        ];
        foreach (PpmImage source in invalid)
        {
            _ = Assert.Throws<InvalidDataException>(() => QualityMetrics.Prepare(source));
        }
        PreparedQualityMetrics prepared = QualityMetrics.Prepare(new(1, 1, [17, 29, 43]));
        _ = Assert.Throws<ArgumentNullException>(() => prepared.Compare(null!));
        foreach (PpmImage decoded in invalid)
        {
            _ = Assert.Throws<InvalidDataException>(() => prepared.Compare(decoded));
        }
        _ = Assert.Throws<InvalidDataException>(() => prepared.Compare(new(2, 1, [0, 0, 0, 0, 0, 0])));
    }

    private static void AssertBits(QualityMetrics expected, QualityMetrics actual)
    {
        Assert.Equal(BitConverter.DoubleToInt64Bits(expected.RgbMse), BitConverter.DoubleToInt64Bits(actual.RgbMse));
        Assert.Equal(BitConverter.DoubleToInt64Bits(expected.RgbPsnrDb), BitConverter.DoubleToInt64Bits(actual.RgbPsnrDb));
        Assert.Equal(BitConverter.DoubleToInt64Bits(expected.LumaSsim), BitConverter.DoubleToInt64Bits(actual.LumaSsim));
    }
}
