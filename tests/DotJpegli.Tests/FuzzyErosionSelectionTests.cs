namespace DotJpegli.Tests;

using System.Numerics;
using DotJpegli.Internal;
using Xunit;

public sealed class FuzzyErosionSelectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScalarAndVectorSelectFirstFourForEveryDistinctPermutation(bool useNetwork)
    {
        float[] values = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        float[][] columns = CreateColumns();
        int lane = 0, permutations = 0;
        do
        {
            SelectScalar(values, useNetwork, out float v0, out float v1, out float v2, out float v3);
            Assert.Equal(1f, v0);
            Assert.Equal(2f, v1);
            Assert.Equal(3f, v2);
            Assert.Equal(4f, v3);

            // Each lane receives a different permutation and distinct values,
            // exposing lane mixing as well as an incorrect selection network.
            for (int input = 0; input < 9; input++)
            {
                columns[input][lane] = values[input] + (lane * 16f);
            }

            if (++lane == Vector<float>.Count)
            {
                (Vector<float> vector0, Vector<float> vector1, Vector<float> vector2, Vector<float> vector3) = SelectVector(columns, useNetwork);
                for (int outputLane = 0; outputLane < Vector<float>.Count; outputLane++)
                {
                    float offset = outputLane * 16f;
                    Assert.Equal(1f + offset, vector0[outputLane]);
                    Assert.Equal(2f + offset, vector1[outputLane]);
                    Assert.Equal(3f + offset, vector2[outputLane]);
                    Assert.Equal(4f + offset, vector3[outputLane]);
                }
                lane = 0;
            }
            permutations++;
        }
        while (NextPermutation(values));

        Assert.Equal(362880, permutations);
        Assert.Equal(0, lane);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScalarAndVectorSelectFirstFourForEveryBinaryNeighborhood(bool useNetwork)
    {
        float[][] columns = CreateColumns();
        for (int mask = 0; mask < 512; mask += Vector<float>.Count)
        {
            for (int lane = 0; lane < Vector<float>.Count; lane++)
            {
                for (int input = 0; input < 9; input++)
                {
                    columns[input][lane] = (((mask + lane) & (1 << input)) == 0) ? 1.25f : 17.5f;
                }
            }

            AssertMatchesSortedNeighborhoods(columns, useNetwork);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TiedAndExtremePositiveValuesPreserveSelectedFloatBits(bool useNetwork)
    {
        float[][] columns = CreateColumns();
        float[] boundaries =
        [
            float.Epsilon, BitConverter.Int32BitsToSingle(0x007fffff),
            BitConverter.Int32BitsToSingle(0x00800000), 1f,
            BitConverter.Int32BitsToSingle(0x3f800001), 128f, float.MaxValue
        ];
        var random = new Random(9317);
        for (int batch = 0; batch < 2048; batch++)
        {
            for (int lane = 0; lane < Vector<float>.Count; lane++)
            {
                for (int input = 0; input < 9; input++)
                {
                    columns[input][lane] = batch < boundaries.Length
                        ? boundaries[(batch + input + lane) % boundaries.Length]
                        : BitConverter.Int32BitsToSingle(random.Next(1, 0x7f800000));
                }
                // Include repeated positive values across several ranks.
                columns[4][lane] = columns[1][lane];
                columns[8][lane] = columns[1][lane];
            }
            AssertMatchesSortedNeighborhoods(columns, useNetwork);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ExplicitKernelsPreserveStrengthBitsAcrossEdgesAndReusedNeighborhoods(int sampling)
    {
        int mcuHeight = sampling * 8, height = (mcuHeight * 3) + 3;
        foreach (int width in new[] { 1, 3, 9, 19, 67, 129 })
        {
            int stride = width + 7;
            byte[] samples = new byte[stride * height];
            new Random(7139 + width + sampling).NextBytes(samples);
            PixelSource pixels = new PixelSource(samples, PixelLayout.Gray8)
                .WithConvertedPlanes(width, height, stride, TestContext.Current.CancellationToken);
            using IDisposable? storage = pixels.ConvertedStorage;
            using var insertion = new AdaptiveQuantization(width, height, sampling,
                fuzzyErosionKernel: KernelPreference.Original);
            using var network = new AdaptiveQuantization(width, height, sampling,
                fuzzyErosionKernel: KernelPreference.Optimized);
            float[] expected = new float[insertion.BlocksWide * sampling];
            float[] actual = new float[expected.Length];
            foreach (bool vector in new[] { false, true })
            {
                foreach (int row in new[] { 0, 1, 2, 3, 3, 0, 2, 1, 2, 3 })
                {
                    if (vector)
                    {
                        insertion.ComputeRowVector(pixels, stride, row * mcuHeight, 4, expected,
                            TestContext.Current.CancellationToken);
                        network.ComputeRowVector(pixels, stride, row * mcuHeight, 4, actual,
                            TestContext.Current.CancellationToken);
                    }
                    else
                    {
                        insertion.ComputeRowScalar(pixels, stride, row * mcuHeight, 4, expected,
                            TestContext.Current.CancellationToken);
                        network.ComputeRowScalar(pixels, stride, row * mcuHeight, 4, actual,
                            TestContext.Current.CancellationToken);
                    }
                    for (int block = 0; block < expected.Length; block++)
                    {
                        AssertSameBits(expected[block], actual[block]);
                    }
                }
            }
        }
    }

    private static void AssertMatchesSortedNeighborhoods(float[][] columns, bool useNetwork)
    {
        (Vector<float> vector0, Vector<float> vector1, Vector<float> vector2, Vector<float> vector3) = SelectVector(columns, useNetwork);
        Span<float> expected = stackalloc float[9];
        Span<float> inputs = stackalloc float[9];
        for (int lane = 0; lane < Vector<float>.Count; lane++)
        {
            for (int input = 0; input < 9; input++)
            {
                inputs[input] = expected[input] = columns[input][lane];
            }

            expected.Sort();
            SelectScalar(inputs, useNetwork, out float v0, out float v1, out float v2, out float v3);
            AssertSameBits(expected[0], v0);
            AssertSameBits(expected[1], v1);
            AssertSameBits(expected[2], v2);
            AssertSameBits(expected[3], v3);
            AssertSameBits(expected[0], vector0[lane]);
            AssertSameBits(expected[1], vector1[lane]);
            AssertSameBits(expected[2], vector2[lane]);
            AssertSameBits(expected[3], vector3[lane]);
        }
    }

    private static (Vector<float> v0, Vector<float> v1, Vector<float> v2, Vector<float> v3)
        SelectVector(float[][] columns, bool useNetwork)
    {
        Vector<float> v0 = new(columns[0]), v1 = new(columns[1]);
        Vector<float> v2 = new(columns[2]), v3 = new(columns[3]);
        if (useNetwork)
        {
            AdaptiveQuantization.SelectFourMinimaNetwork(ref v0, ref v1, ref v2, ref v3,
                new(columns[4]), new(columns[5]), new(columns[6]), new(columns[7]), new(columns[8]));
        }
        else
        {
            AdaptiveQuantization.SelectFourMinimaInsertion(ref v0, ref v1, ref v2, ref v3,
                new(columns[4]), new(columns[5]), new(columns[6]), new(columns[7]), new(columns[8]));
        }

        return (v0, v1, v2, v3);
    }

    private static void SelectScalar(ReadOnlySpan<float> values, bool useNetwork,
        out float v0, out float v1, out float v2, out float v3)
    {
        v0 = values[0];
        v1 = values[1];
        v2 = values[2];
        v3 = values[3];
        if (useNetwork)
        {
            AdaptiveQuantization.SelectFourMinimaNetwork(ref v0, ref v1, ref v2, ref v3,
                values[4], values[5], values[6], values[7], values[8]);
        }
        else
        {
            AdaptiveQuantization.SelectFourMinimaInsertion(ref v0, ref v1, ref v2, ref v3,
                values[4], values[5], values[6], values[7], values[8]);
        }
    }

    private static float[][] CreateColumns()
    {
        float[][] columns = new float[9][];
        for (int input = 0; input < columns.Length; input++)
        {
            columns[input] = new float[Vector<float>.Count];
        }

        return columns;
    }

    private static bool NextPermutation(Span<float> values)
    {
        int pivot = values.Length - 2;
        while (pivot >= 0 && values[pivot] >= values[pivot + 1])
        {
            pivot--;
        }

        if (pivot < 0)
        {
            return false;
        }

        int successor = values.Length - 1;
        while (values[successor] <= values[pivot])
        {
            successor--;
        }

        (values[pivot], values[successor]) = (values[successor], values[pivot]);
        values[(pivot + 1)..].Reverse();
        return true;
    }

    private static void AssertSameBits(float expected, float actual)
        => Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
