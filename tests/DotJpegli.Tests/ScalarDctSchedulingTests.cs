namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class ScalarDctSchedulingTests
{
    [Fact]
    public void FixedScalarScheduleMatchesOriginalRecursivePortBits()
    {
        foreach (float[] samples in SampleBlocks())
        {
            float[] expected = new float[64];
            float[] actual = new float[72];
            Array.Fill(actual, -12345.625f);
            TransformOriginal(samples, expected);
            ForwardDct.TransformScalar(samples, actual);
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.Equal(BitConverter.SingleToInt32Bits(expected[index]),
                    BitConverter.SingleToInt32Bits(actual[index]));
            }

            Assert.All(actual.Skip(64), value => Assert.Equal(-12345.625f, value));
        }
    }

    private static IEnumerable<float[]> SampleBlocks()
    {
        yield return new float[64];
        yield return Enumerable.Range(0, 64).Select(i => i % 2 == 0 ? -0f : 0f).ToArray();
        yield return Enumerable.Repeat(40.125f, 64).ToArray();
        foreach (float impulse in new[]
        {
            103.375f, -89.125f, float.Epsilon, -float.Epsilon, float.MaxValue,
            float.PositiveInfinity, float.NegativeInfinity,
            BitConverter.Int32BitsToSingle(0x7FC12345)
        })
        {
            for (int position = 0; position < 64; position++)
            {
                float[] samples = new float[64];
                samples[position] = impulse;
                yield return samples;
            }
        }

        var random = new Random(7819);
        for (int block = 0; block < 128; block++)
        {
            float[] samples = new float[64];
            for (int index = 0; index < samples.Length; index++)
            {
                samples[index] = block < 64
                    ? (float)((random.NextDouble() * 256) - 128)
                    : MathF.ScaleB((float)((random.NextDouble() * 2) - 1), random.Next(-149, 121));
            }

            yield return samples;
        }
    }

    // Retain the pre-specialization recursive port as a bitwise oracle. A cosine
    // definition permits rounding differences and cannot establish this contract.
    private static void TransformOriginal(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        Span<float> intermediate = stackalloc float[64];
        Span<float> lane = stackalloc float[8];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                lane[y] = samples[(y * 8) + x];
            }

            TransformRecursive(lane);
            for (int v = 0; v < 8; v++)
            {
                intermediate[(v * 8) + x] = lane[v] * 0.125f;
            }
        }

        for (int v = 0; v < 8; v++)
        {
            Span<float> row = intermediate.Slice(v * 8, 8);
            TransformRecursive(row);
            row.CopyTo(coefficients.Slice(v * 8, 8));
        }
    }

    private static void TransformRecursive(Span<float> values)
    {
        int size = values.Length;
        if (size == 2)
        {
            float first = values[0];
            float second = values[1];
            values[0] = first + second;
            values[1] = first - second;
            return;
        }

        int half = size / 2;
        Span<float> temporary = stackalloc float[8];
        Span<float> even = temporary[..half];
        Span<float> odd = temporary.Slice(half, half);
        for (int i = 0; i < half; i++)
        {
            even[i] = values[i] + values[size - 1 - i];
        }

        TransformRecursive(even);
        for (int i = 0; i < half; i++)
        {
            odd[i] = values[i] - values[size - 1 - i];
        }

        ReadOnlySpan<float> multipliers = size == 4
            ? [0.541196100146197f, 1.3065629648763764f]
            : [0.5097955791041592f, 0.6013448869350453f, 0.8999762231364156f, 2.5629154477415055f];
        for (int i = 0; i < half; i++)
        {
            odd[i] *= multipliers[i];
        }

        TransformRecursive(odd);
        odd[0] = FloatingPointMath.FusedMultiplyAdd(odd[0], 1.41421356237f, odd[1]);
        for (int i = 1; i + 1 < half; i++)
        {
            odd[i] += odd[i + 1];
        }

        for (int i = 0; i < half; i++)
        {
            values[2 * i] = even[i];
            values[(2 * i) + 1] = odd[i];
        }
    }
}
