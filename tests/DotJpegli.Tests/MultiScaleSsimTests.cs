namespace DotJpegli.Tests;

using DotJpegli.Parity;
using Xunit;

public sealed class MultiScaleSsimTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 17)]
    [InlineData(19, 1)]
    [InlineData(3, 5)]
    [InlineData(8, 8)]
    [InlineData(17, 19)]
    [InlineData(32, 24)]
    public void PreparedScoresMatchDirectReferenceBitsForEveryScaleAndOddEdges(int width, int height)
    {
        var random = new Random((width * 100) + height);
        for (int pattern = 0; pattern < 5; pattern++)
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
                    _ => pixels[index]
                };
            }
            var source = new PpmImage(width, height, pixels);
            PreparedMultiScaleSsim prepared = MultiScaleSsim.Prepare(source, TestContext.Current.CancellationToken);
            for (int distortion = 0; distortion < 4; distortion++)
            {
                byte[] decodedPixels = pixels.ToArray();
                for (int index = 0; index < decodedPixels.Length; index++)
                {
                    decodedPixels[index] = distortion switch
                    {
                        0 => decodedPixels[index],
                        1 => (byte)Math.Min(255, decodedPixels[index] + 10),
                        2 => (byte)(255 - decodedPixels[index]),
                        _ => (byte)random.Next(256)
                    };
                }
                var decoded = new PpmImage(width, height, decodedPixels);
                double expected = MultiScaleSsim.Compare(source, decoded, TestContext.Current.CancellationToken),
                    actual = prepared.Compare(decoded, TestContext.Current.CancellationToken);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));
                Assert.InRange(actual, 0, 1);
                if (distortion == 0)
                {
                    Assert.InRange(Math.Abs(actual - 1), 0, 1e-11);
                }
                Assert.InRange(Math.Abs(expected - MultiScaleSsim.Compare(decoded, source, TestContext.Current.CancellationToken)), 0, 1e-12);
            }
        }
    }

    [Theory]
    [InlineData(0, 255)]
    [InlineData(17, 191)]
    [InlineData(117, 118)]
    public void ConstantImagesMatchAnalyticFinalScaleLuminance(int sourceValue, int decodedValue)
    {
        var source = new PpmImage(7, 9, Enumerable.Repeat((byte)sourceValue, 7 * 9 * 3).ToArray());
        var decoded = new PpmImage(7, 9, Enumerable.Repeat((byte)decodedValue, 7 * 9 * 3).ToArray());
        const double c1 = .01 * .01 * 255 * 255;
        double luminance = ((2.0 * sourceValue * decodedValue) + c1) /
            ((sourceValue * sourceValue) + (decodedValue * decodedValue) + c1);
        double expected = Math.Pow(luminance, .1333);
        double actual = MultiScaleSsim.Compare(source, decoded, TestContext.Current.CancellationToken);
        Assert.InRange(Math.Abs(expected - actual), 0, 1e-11);
        Assert.Equal(BitConverter.DoubleToInt64Bits(actual),
            BitConverter.DoubleToInt64Bits(MultiScaleSsim.Prepare(source, TestContext.Current.CancellationToken)
                .Compare(decoded, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public void PreparedPyramidOwnsItsSourceAndSupportsConcurrentReuse()
    {
        const int width = 13, height = 9;
        var random = new Random(93);
        byte[] pixels = new byte[width * height * 3];
        random.NextBytes(pixels);
        var source = new PpmImage(width, height, pixels);
        var snapshot = new PpmImage(width, height, pixels.ToArray());
        PreparedMultiScaleSsim prepared = MultiScaleSsim.Prepare(source, TestContext.Current.CancellationToken);
        PpmImage[] decoded = Enumerable.Range(0, 4).Select(_ =>
        {
            byte[] raster = new byte[pixels.Length];
            random.NextBytes(raster);
            return new PpmImage(width, height, raster);
        }).ToArray();
        double[] expected = decoded.Select(image => MultiScaleSsim.Compare(snapshot, image, TestContext.Current.CancellationToken)).ToArray();
        pixels.AsSpan().Clear();
        _ = Parallel.For(0, decoded.Length, index =>
        {
            double actual = prepared.Compare(decoded[index], TestContext.Current.CancellationToken);
            Assert.Equal(BitConverter.DoubleToInt64Bits(expected[index]), BitConverter.DoubleToInt64Bits(actual));
        });
        decoded[0].Pixels.AsSpan().Fill(255);
        Assert.Equal(BitConverter.DoubleToInt64Bits(MultiScaleSsim.Compare(snapshot, decoded[0], TestContext.Current.CancellationToken)),
            BitConverter.DoubleToInt64Bits(prepared.Compare(decoded[0], TestContext.Current.CancellationToken)));
        Assert.All(pixels, value => Assert.Equal((byte)0, value));
    }

    [Fact]
    public void OddEdgesReplicateLastRowsAndColumnsInEveryPyramidScale()
    {
        byte[] pixels = Enumerable.Range(1, 9).SelectMany(value => Enumerable.Repeat((byte)value, 3)).ToArray();
        MultiScaleSsim.Scale[] pyramid = MultiScaleSsim.CreatePyramid(new(3, 3, pixels), TestContext.Current.CancellationToken);
        Assert.Equal(5, pyramid.Length);
        Assert.Equal((3, 3), (pyramid[0].Width, pyramid[0].Height));
        Assert.Equal((2, 2), (pyramid[1].Width, pyramid[1].Height));
        double[] expected = [3, 4.5, 7.5, 9];
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.InRange(Math.Abs(expected[index] - pyramid[1].Luma[index]), 0, 1e-12);
        }
        for (int scaleIndex = 2; scaleIndex < pyramid.Length; scaleIndex++)
        {
            Assert.Equal((1, 1), (pyramid[scaleIndex].Width, pyramid[scaleIndex].Height));
            Assert.InRange(Math.Abs(6 - Assert.Single(pyramid[scaleIndex].Luma)), 0, 1e-12);
        }
    }

    [Fact]
    public void CancelledMetricsStopAtEntryAndKeepPreparedReferenceReusable()
    {
        var image = new PpmImage(1, 1, [17, 29, 43]);
        PreparedMultiScaleSsim prepared = MultiScaleSsim.Prepare(image, TestContext.Current.CancellationToken);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        CancellationToken token = cancellation.Token;
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => MultiScaleSsim.Prepare(image, token)).CancellationToken);
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => new PreparedMultiScaleSsim(image, token)).CancellationToken);
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => MultiScaleSsim.Compare(image, image, token)).CancellationToken);
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => prepared.Compare(image, token)).CancellationToken);
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => MultiScaleSsim.CreatePyramid(image, token)).CancellationToken);
        MultiScaleSsim.Scale[] pyramid = MultiScaleSsim.CreatePyramid(image, TestContext.Current.CancellationToken);
        Assert.Equal(token, Assert.Throws<OperationCanceledException>(() => MultiScaleSsim.PrepareMoments(pyramid[0], token)).CancellationToken);
        Assert.InRange(Math.Abs(prepared.Compare(image, TestContext.Current.CancellationToken) - 1), 0, 1e-11);
    }

    [Fact]
    public void InvalidRastersAndDifferentDimensionsAreRejected()
    {
        var source = new PpmImage(1, 1, [17, 29, 43]);
        PreparedMultiScaleSsim prepared = MultiScaleSsim.Prepare(source, TestContext.Current.CancellationToken);
        _ = Assert.Throws<ArgumentNullException>(() => MultiScaleSsim.Prepare(null!, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentNullException>(() => MultiScaleSsim.Compare(null!, source, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentNullException>(() => MultiScaleSsim.Compare(source, null!, TestContext.Current.CancellationToken));
        _ = Assert.Throws<ArgumentNullException>(() => prepared.Compare(null!, TestContext.Current.CancellationToken));
        PpmImage[] invalid =
        [
            new(0, 1, []), new(1, 0, []), new(-1, 1, []), new(1, -1, []),
            new(1, 1, null!), new(1, 1, []), new(1, 1, [0, 0]),
            new(1, 1, [0, 0, 0, 0]), new(int.MaxValue, int.MaxValue, [0, 0, 0])
        ];
        foreach (PpmImage image in invalid)
        {
            _ = Assert.Throws<InvalidDataException>(() => MultiScaleSsim.Prepare(image, TestContext.Current.CancellationToken));
            _ = Assert.Throws<InvalidDataException>(() => MultiScaleSsim.Compare(image, source, TestContext.Current.CancellationToken));
            _ = Assert.Throws<InvalidDataException>(() => MultiScaleSsim.Compare(source, image, TestContext.Current.CancellationToken));
            _ = Assert.Throws<InvalidDataException>(() => prepared.Compare(image, TestContext.Current.CancellationToken));
        }
        var different = new PpmImage(2, 1, [0, 0, 0, 0, 0, 0]);
        _ = Assert.Throws<InvalidDataException>(() => MultiScaleSsim.Compare(source, different, TestContext.Current.CancellationToken));
        _ = Assert.Throws<InvalidDataException>(() => prepared.Compare(different, TestContext.Current.CancellationToken));
    }
}
