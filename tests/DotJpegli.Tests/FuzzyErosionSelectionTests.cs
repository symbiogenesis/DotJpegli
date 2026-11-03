using System.Numerics;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class FuzzyErosionSelectionTests
{
    [Fact]
    public void ScalarAndVectorSelectFirstFourForEveryDistinctPermutation()
    {
        float[] values = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        float[][] columns = CreateColumns();
        int lane = 0, permutations = 0;
        do
        {
            float v0 = values[0], v1 = values[1], v2 = values[2], v3 = values[3];
            AdaptiveQuantization.SelectFourMinima(ref v0, ref v1, ref v2, ref v3,
                values[4], values[5], values[6], values[7], values[8]);
            Assert.Equal(1f, v0);
            Assert.Equal(2f, v1);
            Assert.Equal(3f, v2);
            Assert.Equal(4f, v3);

            // Each lane receives a different permutation and distinct values,
            // exposing lane mixing as well as an incorrect selection network.
            for (int input = 0; input < 9; input++)
                columns[input][lane] = values[input] + lane * 16f;
            if (++lane == Vector<float>.Count)
            {
                var selected = SelectVector(columns);
                for (int outputLane = 0; outputLane < Vector<float>.Count; outputLane++)
                {
                    float offset = outputLane * 16f;
                    Assert.Equal(1f + offset, selected.V0[outputLane]);
                    Assert.Equal(2f + offset, selected.V1[outputLane]);
                    Assert.Equal(3f + offset, selected.V2[outputLane]);
                    Assert.Equal(4f + offset, selected.V3[outputLane]);
                }
                lane = 0;
            }
            permutations++;
        }
        while (NextPermutation(values));

        Assert.Equal(362880, permutations);
        Assert.Equal(0, lane);
    }

    [Fact]
    public void ScalarAndVectorSelectFirstFourForEveryBinaryNeighborhood()
    {
        float[][] columns = CreateColumns();
        for (int mask = 0; mask < 512; mask += Vector<float>.Count)
        {
            for (int lane = 0; lane < Vector<float>.Count; lane++)
                for (int input = 0; input < 9; input++)
                    columns[input][lane] = (((mask + lane) & (1 << input)) == 0) ? 1.25f : 17.5f;
            AssertMatchesSortedNeighborhoods(columns);
        }
    }

    [Fact]
    public void TiedAndExtremePositiveValuesPreserveSelectedFloatBits()
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
                    columns[input][lane] = batch < boundaries.Length
                        ? boundaries[(batch + input + lane) % boundaries.Length]
                        : BitConverter.Int32BitsToSingle(random.Next(1, 0x7f800000));
                // Include repeated positive values across several ranks.
                columns[4][lane] = columns[1][lane];
                columns[8][lane] = columns[1][lane];
            }
            AssertMatchesSortedNeighborhoods(columns);
        }
    }

    private static void AssertMatchesSortedNeighborhoods(float[][] columns)
    {
        var selected = SelectVector(columns);
        Span<float> expected = stackalloc float[9];
        for (int lane = 0; lane < Vector<float>.Count; lane++)
        {
            for (int input = 0; input < 9; input++)
                expected[input] = columns[input][lane];
            expected.Sort();
            float v0 = columns[0][lane], v1 = columns[1][lane];
            float v2 = columns[2][lane], v3 = columns[3][lane];
            AdaptiveQuantization.SelectFourMinima(ref v0, ref v1, ref v2, ref v3,
                columns[4][lane], columns[5][lane], columns[6][lane], columns[7][lane], columns[8][lane]);
            AssertSameBits(expected[0], v0);
            AssertSameBits(expected[1], v1);
            AssertSameBits(expected[2], v2);
            AssertSameBits(expected[3], v3);
            AssertSameBits(expected[0], selected.V0[lane]);
            AssertSameBits(expected[1], selected.V1[lane]);
            AssertSameBits(expected[2], selected.V2[lane]);
            AssertSameBits(expected[3], selected.V3[lane]);
        }
    }

    private static (Vector<float> V0, Vector<float> V1, Vector<float> V2, Vector<float> V3)
        SelectVector(float[][] columns)
    {
        Vector<float> v0 = new(columns[0]), v1 = new(columns[1]);
        Vector<float> v2 = new(columns[2]), v3 = new(columns[3]);
        AdaptiveQuantization.SelectFourMinima(ref v0, ref v1, ref v2, ref v3,
            new(columns[4]), new(columns[5]), new(columns[6]), new(columns[7]), new(columns[8]));
        return (v0, v1, v2, v3);
    }

    private static float[][] CreateColumns()
    {
        var columns = new float[9][];
        for (int input = 0; input < columns.Length; input++)
            columns[input] = new float[Vector<float>.Count];
        return columns;
    }

    private static bool NextPermutation(Span<float> values)
    {
        int pivot = values.Length - 2;
        while (pivot >= 0 && values[pivot] >= values[pivot + 1])
            pivot--;
        if (pivot < 0)
            return false;
        int successor = values.Length - 1;
        while (values[successor] <= values[pivot])
            successor--;
        (values[pivot], values[successor]) = (values[successor], values[pivot]);
        values[(pivot + 1)..].Reverse();
        return true;
    }

    private static void AssertSameBits(float expected, float actual)
        => Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
