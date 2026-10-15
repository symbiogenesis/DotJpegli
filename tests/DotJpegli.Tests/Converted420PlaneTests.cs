using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class Converted420PlaneTests
{
    [Theory]
    [InlineData(PixelComponentLayout.Rgb)]
    [InlineData(PixelComponentLayout.Bgr)]
    [InlineData(PixelComponentLayout.Rgba)]
    [InlineData(PixelComponentLayout.Bgra)]
    public void CompactPlanesPreserveSourcePaddedAveragesAndFullResolutionFallbacks(PixelComponentLayout layout)
    {
        foreach ((int width, int height) in Sizes())
        {
            int stride = width * SimdSampleFixtures.Channels(layout) + 7;
            byte[] source = (byte[])SimdSampleFixtures.Source(0, width, height, stride, layout);
            PixelSource original = SimdSampleFixtures.Pixels(source, layout);
            PixelSource compact = original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken);
            using IDisposable? storage = compact.ConvertedStorage;
            Assert.NotNull(storage);
            Assert.True(compact.GetConvertedPlane(1).IsEmpty);
            float[] expected = new float[64];
            for (int component = 0; component < 3; component++)
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        float sample = original.ReadSampleAt(x, y, stride, component);
                        SimdSampleFixtures.AssertBits(sample, compact.ReadSampleAt(x, y, stride, component));
                        SimdSampleFixtures.AssertBits(sample, compact.ReadSample(y * stride + x * original.ComponentsPerPixel, component));
                    }
                if (component == 0)
                    continue;
                ReadOnlySpan<float> plane = compact.GetConverted420ChromaPlane(component, out int planeWidth, out int planeHeight);
                Assert.Equal((width + 15) / 16 * 8, planeWidth);
                Assert.Equal((height + 15) / 16 * 8, planeHeight);
                for (int top = 0; top < planeHeight; top += 8)
                    for (int left = 0; left < planeWidth; left += 8)
                    {
                        Downsampling.FillBlockScalar(original, width, height, stride, component, left * 2, top * 2,
                            2, 2, expected);
                        for (int y = 0; y < 8; y++)
                            for (int x = 0; x < 8; x++)
                                SimdSampleFixtures.AssertBits(expected[y * 8 + x], plane[(top + y) * planeWidth + left + x] - 128f);
                    }
            }
            // Compact snapshots retain a source copy for reads that cannot be
            // reconstructed from already averaged chroma values.
            PixelSource restored = new PixelSource.Snapshot(compact, compact.Length, TestContext.Current.CancellationToken).GetPixels();
            float[][] rows = [new float[33], new float[33], new float[33]];
            for (int component = 0; component < 3; component++)
                original.ReadRowScalar(width, height, stride, component, -2, height, rows[component]);
            Array.Fill(source, byte.MaxValue);
            float[] vector = new float[33];
            for (int component = 0; component < 3; component++)
            {
                restored.ReadRowVector(width, height, stride, component, -2, height, vector);
                AssertBits(rows[component], vector);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompactQuantizedBlocksMatchFullPlanesWithAndWithoutAdaptiveAnalysis(bool adaptive)
    {
        JpegSamplingFactors sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.Quarter);
        ushort[][] tables = Enumerable.Range(0, 3).Select(component => Quantization.CreateTableForDistance(1.45f, component, 2, 2)).ToArray();
        float[][]? multipliers = adaptive ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 2, 2, false) : null;
        foreach ((int width, int height) in Sizes())
        {
            int stride = width * 3 + 7;
            PixelSource original = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(0, width, height, stride, PixelComponentLayout.Rgb), PixelComponentLayout.Rgb);
            PixelSource full = original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
            using IDisposable? fullStorage = full.ConvertedStorage;
            PixelSource compact = original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken);
            using IDisposable? compactStorage = compact.ConvertedStorage;
            using QuantizedImage expected = QuantizedImage.Create(full, width, height, stride, sampling, tables, 1.45f, 0,
                adaptive ? new AdaptiveQuantization(width, height, 2) : null, multipliers, TestContext.Current.CancellationToken)!;
            using QuantizedImage actual = QuantizedImage.Create(compact, width, height, stride, sampling, tables, 1.45f, 0,
                adaptive ? new AdaptiveQuantization(width, height, 2) : null, multipliers, TestContext.Current.CancellationToken)!;
            for (int component = 0; component < 3; component++)
            {
                int scale = component == 0 ? 8 : 16;
                for (int y = 0; y < (height + scale - 1) / scale; y++)
                    for (int x = 0; x < (width + scale - 1) / scale; x++)
                        Assert.Equal(expected.GetBlock(component, x, y).ToArray(), actual.GetBlock(component, x, y).ToArray());
            }
        }
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24)]
    [InlineData(PixelLayout.Bgr24)]
    [InlineData(PixelLayout.Rgba32)]
    [InlineData(PixelLayout.Bgra32)]
    public void EncodedBytesMatchFullPlaneParallelPathAcrossScanModes(PixelLayout layout)
    {
        PixelComponentLayout components = layout switch
        {
            PixelLayout.Rgb24 => PixelComponentLayout.Rgb,
            PixelLayout.Bgr24 => PixelComponentLayout.Bgr,
            PixelLayout.Rgba32 => PixelComponentLayout.Rgba,
            _ => PixelComponentLayout.Bgra
        };
        foreach ((int width, int height) in new[] { (1, 1), (2, 2), (10, 10), (17, 19), (32, 33) })
        {
            int stride = width * SimdSampleFixtures.Channels(components) + 7;
            byte[] source = (byte[])SimdSampleFixtures.Source(0, width, height, stride, components);
            foreach (int mode in new[] { 0, 1, 2, 3 })
            {
                var options = new JpegliEncoderOptions
                {
                    Subsampling = ChromaSubsampling.Quarter, ProgressiveLevel = mode == 1 ? 2 : null,
                    ScanScript = mode == 2 ? [new([2]), new([0, 1])] : null,
                    AdaptiveQuantization = mode != 3, RestartInterval = 2
                };
                byte[] compact = JpegliEncoder.Encode(source, width, height, layout, options, stride, TestContext.Current.CancellationToken);
                byte[] full = JpegliEncoder.Encode(source, width, height, layout,
                    options with { MaxDegreeOfParallelism = 2 }, stride, TestContext.Current.CancellationToken);
                Assert.Equal(full, compact);
            }
        }
    }

    [Fact]
    public void OtherPrecisionsAndColorTransformsRetainFullPlanes()
    {
        const int width = 19, height = 3, stride = width * 3 + 7;
        foreach (int precision in new[] { 0, 1, 2 })
            foreach (PixelComponentLayout layout in Enum.GetValues<PixelComponentLayout>())
            {
                int sourceStride = width * SimdSampleFixtures.Channels(layout) + 7;
                PixelSource source = SimdSampleFixtures.Pixels(SimdSampleFixtures.Source(precision, width, height, sourceStride, layout), layout);
                foreach (JpegColorSpace output in SimdSampleFixtures.OutputSpaces(layout))
                {
                    PixelSource original = source.WithColorSpace(output);
                    PixelSource prepared = original.WithConverted420Planes(width, height, sourceStride, TestContext.Current.CancellationToken);
                    using IDisposable? storage = prepared.ConvertedStorage;
                    bool compact = precision == 0 && (layout is PixelComponentLayout.Rgb or PixelComponentLayout.Bgr
                        or PixelComponentLayout.Rgba or PixelComponentLayout.Bgra) && original.ColorSpace == JpegColorSpace.YCbCr;
                    Assert.Equal(compact, !prepared.GetConverted420ChromaPlane(1, out _, out _).IsEmpty);
                    for (int component = 0; component < (prepared.IsGrayscale ? 1 : 3); component++)
                        SimdSampleFixtures.AssertBits(original.ReadSampleAt(0, 0, sourceStride, component), prepared.ReadSampleAt(0, 0, sourceStride, component));
                }
            }
        IccColorTransform transform = IccColorTransform.Create(JpegliIccProfiles.CreateLinearSrgbProfile(), JpegliIccProfiles.CreateSrgbProfile(),
            false, false, IccRenderingIntent.RelativeColorimetric, TestContext.Current.CancellationToken);
        PixelSource managed = new PixelSource(new byte[stride * height], PixelLayout.Rgb24).WithColorTransform(transform)
            .WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? managedStorage = managed.ConvertedStorage;
        Assert.True(managed.GetConverted420ChromaPlane(1, out _, out _).IsEmpty);
    }

    [Fact]
    public void BudgetFallbackAndPreCancellationDoNotReadTheSource()
    {
        PixelSource empty = new(ReadOnlySpan<byte>.Empty, PixelLayout.Rgb24);
        PixelSource oversized = empty.WithConverted420Planes(65535, 65535, 65535 * 3, TestContext.Current.CancellationToken);
        Assert.Null(oversized.ConvertedStorage);
        // The compact plane alone would occupy the full 64 MiB bucket;
        // including its transient row scratch must reject that allocation.
        PixelSource scratchExceedsBudget = empty.WithConverted420Planes(4096, 2048, 4096 * 3, TestContext.Current.CancellationToken);
        Assert.Null(scratchExceedsBudget.ConvertedStorage);
        var cancellation = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => ConvertCanceled(cancellation));
        Assert.Equal(cancellation, error.CancellationToken);
    }

    private static void ConvertCanceled(CancellationToken token)
        => new PixelSource(ReadOnlySpan<byte>.Empty, PixelLayout.Rgb24).WithConverted420Planes(1, 1, 3, token);

    private static (int Width, int Height)[] Sizes() =>
        [(1, 1), (1, 17), (2, 2), (7, 9), (8, 8), (9, 7), (10, 10), (15, 16), (16, 15), (17, 19), (31, 32), (32, 31), (33, 35)];

    private static void AssertBits(float[] expected, float[] actual)
    {
        for (int index = 0; index < expected.Length; index++)
            SimdSampleFixtures.AssertBits(expected[index], actual[index]);
    }
}
