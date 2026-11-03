namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ParallelDctRowsTests
{
    [Theory]
    [InlineData(37, 35, 0)]
    [InlineData(37, 35, 1)]
    [InlineData(37, 35, 2)]
    [InlineData(257, 129, 0)]
    [InlineData(257, 129, 1)]
    [InlineData(257, 129, 2)]
    [InlineData(257, 129, 3)]
    [InlineData(257, 129, 4)]
    public void ContiguousRangesAndSerialRowsMatchIndependentBlocks(int width, int height, int cache)
    {
        int stride = (width * 3) + 7;
        byte[] bytes = new byte[stride * height];
        new Random(761).NextBytes(bytes);
        PixelSource original = new(bytes, PixelLayout.Rgb24);
        PixelSource prepared = cache switch
        {
            1 => original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken),
            2 => original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken),
            3 => original.WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken,
                new EncoderCacheBudget(128 * 1024), allowPartial: true),
            4 => original.WithConverted420Planes(width, height, stride, TestContext.Current.CancellationToken,
                new EncoderCacheBudget(64 * 1024), allowPartial: true),
            _ => original
        };
        using IDisposable? storage = prepared.ConvertedStorage;
        if (cache is 3 or 4)
        {
            Assert.NotNull(storage);
            Assert.False(prepared.HasCompleteConvertedPlanes);
            Assert.InRange(prepared.ConvertedRows, 16, height - 1);
        }
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.Quarter);
        var rows = new ParallelDctRows(prepared, width, height, stride, sampling,
            smoothingFactor: 0, maxDegreeOfParallelism: 3, TestContext.Current.CancellationToken);
        PixelSource oracle = original;
        if (cache is 3 or 4)
        {
            // A partial converted prefix cannot replace the raw tail. Workers
            // must own that tail even when compact-plane borrowing is enabled.
            oracle = new PixelSource(bytes.ToArray(), PixelLayout.Rgb24);
            Array.Clear(bytes);
        }
        Span<float> samples = stackalloc float[64];
        Span<float> expected = stackalloc float[64];
        Span<float> actual = stackalloc float[64];
        // Complete-row requests and individual component scans use different
        // range sizes; the latter must not overwrite other prepared components.
        foreach (bool separate in new[] { false, true })
        {
            for (int top = 0; top < height; top += 16)
            {
                if (!separate)
                {
                    rows.PrepareRow(top, TestContext.Current.CancellationToken);
                }
                for (int component = 0; component < 3; component++)
                {
                    if (separate)
                    {
                        rows.PrepareRow(top, TestContext.Current.CancellationToken, component);
                    }
                    JpegComponentSampling factor = sampling[component];
                    int blocksWide = ((width * factor.Horizontal) + 15) / 16;
                    int blocksHigh = ((height * factor.Vertical) + 15) / 16;
                    int firstRow = top / 16 * factor.Vertical;
                    for (int y = firstRow; y < Math.Min(firstRow + factor.Vertical, blocksHigh); y++)
                    {
                        for (int x = 0; x < blocksWide; x++)
                        {
                            Downsampling.FillBlock(oracle, width, height, stride, component,
                                x, y, 2, 2, factor.Horizontal, factor.Vertical, samples);
                            ForwardDct.Transform(samples, expected);
                            rows.CopyBlock(component, x, y, actual);
                            for (int coefficient = 0; coefficient < 64; coefficient++)
                            {
                                Assert.Equal(BitConverter.SingleToInt32Bits(expected[coefficient]),
                                    BitConverter.SingleToInt32Bits(actual[coefficient]));
                            }
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void TinyAndNarrowImagesAvoidSourceSnapshotsForDctWorkers()
    {
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.Quarter);
        Assert.False(ParallelDctRows.ShouldCreate(37, 35, sampling, grayscale: false));
        Assert.False(ParallelDctRows.ShouldCreate(1, 65535, sampling, grayscale: false));
        Assert.True(ParallelDctRows.ShouldCreate(257, 129, sampling, grayscale: false));
    }
}
