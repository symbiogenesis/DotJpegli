namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class AdaptiveQuantizationWorkspaceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void DirtyRentalsAndReusedStrengthsMatchUnpooledAnalysisBits(int sampling)
    {
        const int width = 67, stride = width + 7;
        int mcuHeight = sampling * 8;
        int height = (mcuHeight * 3) + 3;
        byte[] samples = new byte[height * stride];
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                samples[(row * stride) + column] = (byte)(((row * 29) + (column * 13) + (row * column * 7)) & 255);
            }
        }

        PixelSource pixels = new PixelSource(samples, PixelLayout.Gray8)
            .WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
        using IDisposable? storage = pixels.ConvertedStorage;
        var pool = new DirtyPool();
        using (var analysis = new AdaptiveQuantization(width, height, sampling, pool))
        {
            foreach (bool vector in new[] { false, true })
            {
                foreach (int row in new[] { 0, 1, 2, 3, 3, 0, 2, 1, 2, 3 })
                {
                    using var fresh = new AdaptiveQuantization(width, height, sampling);
                    float[] expected = new float[analysis.BlocksWide * sampling];
                    Span<float> actual = analysis.Strengths;
                    // No output cell may depend on an earlier row or rental.
                    actual.Fill(float.NaN);
                    if (vector)
                    {
                        fresh.ComputeRowVector(pixels, stride, row * mcuHeight, 4, expected,
                            TestContext.Current.CancellationToken);
                        analysis.ComputeRowVector(pixels, stride, row * mcuHeight, 4, actual,
                            TestContext.Current.CancellationToken);
                    }
                    else
                    {
                        fresh.ComputeRowScalar(pixels, stride, row * mcuHeight, 4, expected,
                            TestContext.Current.CancellationToken);
                        analysis.ComputeRowScalar(pixels, stride, row * mcuHeight, 4, actual,
                            TestContext.Current.CancellationToken);
                    }
                    for (int index = 0; index < expected.Length; index++)
                    {
                        Assert.Equal(BitConverter.SingleToInt32Bits(expected[index]),
                            BitConverter.SingleToInt32Bits(actual[index]));
                    }
                }
            }

            Assert.Equal(4, pool.Rents);
            Assert.Equal(4, pool.Outstanding.Count);
        }
        Assert.Equal(4, pool.Returns);
        Assert.Empty(pool.Outstanding);
    }

    [Fact]
    public void PartialConstructionReturnsEverySuccessfulRental()
    {
        var pool = new DirtyPool(throwOnRent: 3);
        _ = Assert.Throws<InvalidOperationException>(() => new AdaptiveQuantization(67, 19, 2, pool));
        Assert.Equal(2, pool.Returns);
        Assert.Empty(pool.Outstanding);
    }

    [Fact]
    public void FailedStrengthRentalCanBeRetriedAndDisposalReturnsStorageOnce()
    {
        var pool = new DirtyPool(throwOnRent: 4);
        var analysis = new AdaptiveQuantization(19, 23, 2, pool);
        try
        {
            _ = Assert.Throws<InvalidOperationException>(() => { _ = analysis.Strengths.Length; });
            Assert.Equal(3, pool.Outstanding.Count);
            Assert.Equal(6, analysis.Strengths.Length);
            Assert.Equal(4, pool.Outstanding.Count);
        }
        finally
        {
            analysis.Dispose();
        }
        analysis.Dispose();
        Assert.Equal(4, pool.Returns);
        Assert.Empty(pool.Outstanding);
        _ = Assert.Throws<ObjectDisposedException>(() => { _ = analysis.Strengths.Length; });
        _ = Assert.Throws<ObjectDisposedException>(() => analysis.ComputeRow(new byte[19 * 23], 19, 1,
            PixelLayout.Gray8, 0, 4, new float[6], TestContext.Current.CancellationToken));
    }

    [Fact]
    public void VeryWideImagesKeepScratchOutOfThePool()
    {
        var pool = new DirtyPool();
        using var analysis = new AdaptiveQuantization(16384, 1, 1, pool);
        Assert.Equal(2048, analysis.Strengths.Length);
        Assert.Equal(0, pool.Rents);
        Assert.Empty(pool.Outstanding);
    }

    private sealed class DirtyPool(int throwOnRent = 0) : ArrayPool<float>
    {
        internal int Rents { get; private set; }
        internal int Returns { get; private set; }
        internal List<float[]> Outstanding { get; } = [];

        public override float[] Rent(int minimumLength)
        {
            this.Rents++;
            if (this.Rents == throwOnRent)
            {
                throw new InvalidOperationException("Injected rental failure.");
            }
            // Oversized, poisoned arrays expose accidental reads of uninitialized
            // scratch or of padding belonging to the pool rather than the image.
            float[] buffer = new float[minimumLength + 17];
            Array.Fill(buffer, float.NaN);
            this.Outstanding.Add(buffer);
            return buffer;
        }

        public override void Return(float[] array, bool clearArray = false)
        {
            Assert.True(this.Outstanding.Remove(array), "A rental was returned more than once.");
            this.Returns++;
            Array.Fill(array, float.NaN);
        }
    }
}
