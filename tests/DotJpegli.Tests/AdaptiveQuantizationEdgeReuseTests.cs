namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class AdaptiveQuantizationEdgeReuseTests
{
    // Scalar strength bits retained from d02e9c9, before clamped pre-erosion
    // rows were reused. The odd final row exercises bottom padding and MCU reuse.
    public static TheoryData<int, int, int[]> BoundaryFixtures => new()
    {
        { 1, 1, [0x3e154f20, 0x3d2bf300] },
        { 1, 9, [0x3fbd0cc8, 0x3f99f24a, 0x3f7fd0e6, 0x3ecd49a4] },
        { 2, 1, [0x3e154f20, 0x3d2bf300, 0x3e154f20, 0x3d2bf300] },
        { 2, 17, [0x3fbd6b8c, 0x3f9ab4d0, 0x4006fda6, 0x3f9cc802,
            0x3fae74f4, 0x3e5e32a0, 0x3fae3fb6, 0x3db3a910] },
        { 3, 1, [0x3e154f20, 0x3d2bf300, 0x3e154f20, 0x3d2bf300, 0x3e154f20, 0x3d2bf300] },
        { 3, 25, [0x3fbd6b8c, 0x3f9ab4d0, 0x40072b32, 0x3f9de248,
            0x40112ee8, 0x3fa943ea, 0x3f931778, 0x3e4a1fe0,
            0x3f92c9d8, 0x3db72070, 0x3f92c9d8, 0x3db72070] },
        { 4, 1, [0x3e154f20, 0x3d2bf300, 0x3e154f20, 0x3d2bf300,
            0x3e154f20, 0x3d2bf300, 0x3e154f20, 0x3d2bf300] },
        { 4, 33, [0x3fbd6b8c, 0x3f9ab4d0, 0x40072b32, 0x3f9de248,
            0x40116f20, 0x3faa695c, 0x3fdeed2e, 0x3f9a7334,
            0x3e5972e8, 0x3e528648, 0x3e1a33c8, 0x3d900d20,
            0x3e1a33c8, 0x3d900d20, 0x3e1a33c8, 0x3d900d20] }
    };

    [Theory]
    [MemberData(nameof(BoundaryFixtures))]
    public void ClampedRowsPreserveRetainedStrengthBits(int sampling, int height, int[] expected)
    {
        const int width = 9, stride = 16;
        byte[] samples = new byte[height * stride];
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                samples[(row * stride) + column] = (byte)(((row * 29) + (column * 13) + (row * column * 7) + 31) & 255);
            }
        }

        PixelSource original = new(samples, PixelLayout.Gray8);
        PixelSource converted = original.WithConvertedPlanes(width, height, stride,
            TestContext.Current.CancellationToken);
        using IDisposable? storage = converted.ConvertedStorage;
        foreach (bool prepared in new[] { false, true })
        {
            PixelSource pixels = prepared ? converted : original;
            foreach (bool vector in new[] { false, true })
            {
                using var analysis = new AdaptiveQuantization(width, height, sampling);
                int rowLength = analysis.BlocksWide * sampling;
                int mcuHeight = sampling * 8;
                int rowCount = ((height - 1) / mcuHeight) + 1;
                Assert.Equal(rowLength * rowCount, expected.Length);
                float[] actual = new float[rowLength + 1];
                // Ascending rows exercise immutable neighborhood reuse, and the
                // reverse pass must also work without a consecutive predecessor.
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int row = 0; row < rowCount; row++)
                    {
                        int index = pass == 0 ? row : rowCount - row - 1;
                        actual.AsSpan().Fill(-123.25f);
                        if (vector)
                        {
                            analysis.ComputeRowVector(pixels, stride, index * mcuHeight, 4, actual,
                                TestContext.Current.CancellationToken);
                        }
                        else
                        {
                            analysis.ComputeRowScalar(pixels, stride, index * mcuHeight, 4, actual,
                                TestContext.Current.CancellationToken);
                        }

                        for (int column = 0; column < rowLength; column++)
                        {
                            Assert.Equal(expected[(index * rowLength) + column], BitConverter.SingleToInt32Bits(actual[column]));
                        }
                        Assert.Equal(-123.25f, actual[^1]);
                    }
                }
            }
        }
    }
}
