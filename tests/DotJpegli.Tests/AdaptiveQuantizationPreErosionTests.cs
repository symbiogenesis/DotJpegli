namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class AdaptiveQuantizationPreErosionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void DirectGroupsMatchOriginalStagedStencilBits(int sampling)
    {
        int mcuHeight = sampling * 8;
        foreach (int width in new[] { 1, 3, 8, 9, 19, 32, 57, 64, 65, 129 })
        {
            foreach (int height in new[] { 1, (mcuHeight * 2) + 3 })
            {
                int stride = width + 7;
                byte[] samples = new byte[height * stride];
                new Random(9127 + width + height).NextBytes(samples);
                // Mix flat cells, low contrasts and noise at four-pixel seams.
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < Math.Min(width, 12); x++)
                    {
                        samples[(y * stride) + x] = x < 4 ? (byte)128 : (byte)(127 + ((x + y) % 3));
                    }
                }
                PixelSource original = new(samples, PixelLayout.Gray8);
                PixelSource prepared = original.WithConvertedPlanes(width, height, stride,
                    TestContext.Current.CancellationToken);
                using IDisposable? storage = prepared.ConvertedStorage;
                foreach (bool converted in new[] { false, true })
                {
                    PixelSource pixels = converted ? prepared : original;
                    foreach (bool vector in new[] { false, true })
                    {
                        var pool = new CapturingPool();
                        using var analysis = new AdaptiveQuantization(width, height, sampling, pool);
                        int paddedWidth = analysis.BlocksWide * 8;
                        int lumaStride = paddedWidth + 2;
                        int preStride = paddedWidth / 4;
                        int preLength = preStride * ((sampling * 2) + 2);
                        Assert.Equal(new[] { lumaStride * (mcuHeight + 10), preLength, preStride * 2 },
                            pool.Lengths);
                        float[] strengths = new float[analysis.BlocksWide * sampling];
                        int[] rows = height == 1 ? [0, 0] : [0, 1, 2, 2, 0];
                        foreach (int row in rows)
                        {
                            int top = row * mcuHeight;
                            if (vector)
                            {
                                analysis.ComputeRowVector(pixels, stride, top, 4, strengths,
                                    TestContext.Current.CancellationToken);
                            }
                            else
                            {
                                analysis.ComputeRowScalar(pixels, stride, top, 4, strengths,
                                    TestContext.Current.CancellationToken);
                            }
                            float[] expected = OriginalStagedStencil(pool.Rentals[0], paddedWidth, height, sampling, top);
                            float[] actual = pool.Rentals[1];
                            for (int cell = 0; cell < preLength; cell++)
                            {
                                Assert.Equal(BitConverter.SingleToInt32Bits(expected[cell]),
                                    BitConverter.SingleToInt32Bits(actual[cell]));
                            }
                            Assert.All(actual.AsSpan(preLength).ToArray(), value => Assert.True(float.IsNaN(value)));
                        }
                    }
                }
            }
        }
    }

    // Retain the original full-width intermediate and a separate horizontal
    // pass. The oracle visits dy before x, independently of the fused groups.
    private static float[] OriginalStagedStencil(float[] luma, int paddedWidth, int height, int sampling, int top)
    {
        int lumaStride = paddedWidth + 2;
        int preStride = paddedWidth / 4;
        int preRows = (sampling * 2) + 2;
        int paddedHeight = (height + (sampling * 8) - 1) / (sampling * 8) * sampling * 8;
        float[] expected = new float[preRows * preStride];
        float[] differences = new float[paddedWidth];
        for (int row = 0; row < preRows; row++)
        {
            int sourceY = Math.Clamp((top / 4) - 1 + row, 0, (paddedHeight / 4) - 1) * 4;
            int firstRow = sourceY - (top - 5);
            for (int dy = 0; dy < 4; dy++)
            {
                for (int x = 0; x < paddedWidth; x++)
                {
                    int index = ((firstRow + dy) * lumaStride) + x + 1;
                    float value = StencilValue(luma[index], luma[index - 1], luma[index + 1],
                        luma[index - lumaStride], luma[index + lumaStride]);
                    differences[x] = dy == 0 ? value : differences[x] + value;
                }
            }
            for (int x = 0; x < preStride; x++)
            {
                int offset = x * 4;
                expected[(row * preStride) + x] = (differences[offset] + differences[offset + 1]
                    + differences[offset + 2] + differences[offset + 3]) * 0.25f;
            }
        }
        return expected;
    }

    private static float StencilValue(float value, float left, float right, float above, float below)
    {
        const float inputScaling = 1f / 255f;
        const float invLog2E = 0.6931471805599453f;
        const float sgMul = 226.0480446705883f;
        const float sgRetMul = 1f / 73.377132366608819f * 18.6580932135f * invLog2E;
        const float epsilon = 1e-2f;
        const float numOffset = epsilon / inputScaling / inputScaling;
        const float numMul = sgRetMul * 3 * sgMul;
        const float vOffset = ((7.14672470003f * invLog2E) + epsilon) / inputScaling;
        const float denMul = invLog2E * sgMul * inputScaling * inputScaling;
        const float gammaOffset = (float)(0.019 / inputScaling);
        float maskingScale = MathF.Sqrt((float)(211.50759899638012f * 1e8));
        float baseValue = 0.25f * (right + left + (above + below));
        float derivativeInput = MathF.Max(value + gammaOffset, 0f);
        float squared = derivativeInput * derivativeInput;
        float numerator = (numMul * squared) + numOffset;
        float denominator = (denMul * derivativeInput * squared) + vOffset;
        float diff = denominator / numerator * (value - baseValue);
        diff = MathF.Min(diff * diff, 0.2f);
        return 0.25f * MathF.Sqrt((diff * maskingScale) + 28f);
    }

    private sealed class CapturingPool : ArrayPool<float>
    {
        internal List<int> Lengths { get; } = [];
        internal List<float[]> Rentals { get; } = [];

        public override float[] Rent(int minimumLength)
        {
            this.Lengths.Add(minimumLength);
            float[] rental = new float[minimumLength + 7];
            Array.Fill(rental, float.NaN);
            this.Rentals.Add(rental);
            return rental;
        }

        public override void Return(float[] array, bool clearArray = false)
        {
            Assert.Contains(array, this.Rentals);
            Array.Fill(array, float.NaN);
        }
    }
}
