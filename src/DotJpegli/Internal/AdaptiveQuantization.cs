// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

using System.Numerics;

namespace DotJpegli.Internal;

/// <summary>Spatial adaptive quantization, with storage bounded to one MCU neighborhood.</summary>
internal sealed class AdaptiveQuantization
{
    // Derived from google/jpegli lib/jpegli/adaptive_quantization.cc at
    // 031a0077f5799a6041004267fc12b956c1f52a20. Unlike upstream's rolling
    // image buffers, each invocation independently loads its bounded neighborhood.
    private const float InputScaling = 1f / 255f;
    private const float InvLog2E = 0.6931471805599453f;
    private const float SgMul = 226.0480446705883f;
    private const float SgRetMul = (1f / 73.377132366608819f) * 18.6580932135f * InvLog2E;
    private const float Epsilon = 1e-2f;
    private const float NumOffset = Epsilon / InputScaling / InputScaling;
    private const float NumMul = SgRetMul * 3 * SgMul;
    private const float VOffset = (7.14672470003f * InvLog2E + Epsilon) / InputScaling;
    private const float DenMul = InvLog2E * SgMul * InputScaling * InputScaling;

    private readonly int width;
    private readonly int height;
    private readonly int verticalSampling;
    private readonly int paddedWidth;
    private readonly int paddedHeight;
    private readonly int lumaStride;
    private readonly int preStride;
    private readonly float[] luma;
    private readonly float[] preErosion;
    private readonly float[] difference;
    private readonly float[] fuzzy;

    internal AdaptiveQuantization(int width, int height, int verticalSampling)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (verticalSampling is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(verticalSampling));
        this.width = width;
        this.height = height;
        this.verticalSampling = verticalSampling;
        BlocksWide = checked((width + 7) / 8);
        paddedWidth = checked(BlocksWide * 8);
        int mcuHeight = verticalSampling * 8;
        paddedHeight = checked(((height + mcuHeight - 1) / mcuHeight) * mcuHeight);
        lumaStride = checked(paddedWidth + 2);
        preStride = checked(BlocksWide * 2);
        luma = new float[checked(lumaStride * (mcuHeight + 10))];
        preErosion = new float[checked(preStride * (verticalSampling * 2 + 2))];
        difference = new float[paddedWidth];
        fuzzy = new float[checked(preStride * 2)];
    }

    internal int BlocksWide { get; }

    /// <summary>Writes strengths in raster order for the luma blocks in this MCU row.</summary>
    internal void ComputeRow(ReadOnlySpan<byte> pixels, int stride, int bytesPerPixel,
        PixelLayout layout, int top, int lumaAcQuant, Span<float> strengths,
        CancellationToken cancellationToken)
        => ComputeRow(new PixelSource(pixels, layout), stride, top, lumaAcQuant, strengths, cancellationToken);

    internal void ComputeRow(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths, cancellationToken,
            Vector.IsHardwareAccelerated);

    internal void ComputeRowScalar(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths, cancellationToken, false);

    internal void ComputeRowVector(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken)
        => ComputeRowCore(pixels, stride, top, lumaAcQuant, strengths, cancellationToken, true);

    private void ComputeRowCore(PixelSource pixels, int stride, int top, int lumaAcQuant,
        Span<float> strengths, CancellationToken cancellationToken, bool useVector)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int mcuHeight = verticalSampling * 8;
        if (top < 0 || top >= height || top % mcuHeight != 0)
            throw new ArgumentOutOfRangeException(nameof(top));
        if (strengths.Length < BlocksWide * verticalSampling)
            throw new ArgumentException("The strength buffer must hold every luma block in the MCU row.", nameof(strengths));

        // The outer four-row pre-erosion cells also need their immediate
        // pixel neighbors. Replicate the original image edges into all padding.
        int firstLumaY = top - 5;
        for (int row = 0; row < mcuHeight + 10; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int sourceY = Math.Clamp(firstLumaY + row, 0, height - 1);
            int destination = row * lumaStride + 1;
            if (useVector)
                pixels.ReadRowVector(width, height, stride, pixels.AnalysisComponent, 0, sourceY,
                    luma.AsSpan(destination, width));
            else
                pixels.ReadRowScalar(width, height, stride, pixels.AnalysisComponent, 0, sourceY,
                    luma.AsSpan(destination, width));
            luma[destination - 1] = luma[destination];
            luma.AsSpan(destination + width, paddedWidth - width + 1).Fill(luma[destination + width - 1]);
        }

        int firstPreY = top / 4 - 1;
        int preHeight = paddedHeight / 4;
        for (int row = 0; row < verticalSampling * 2 + 2; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int sourceY = Math.Clamp(firstPreY + row, 0, preHeight - 1) * 4;
            ComputePreErosion(sourceY - firstLumaY, row, useVector);
        }

        const float acQuant = 0.841f;
        float dampen = lumaAcQuant < 9 ? 1f : MathF.Max(0f, 1f - (lumaAcQuant - 9f) / 56f);
        float mul = acQuant * dampen;
        float add = (1f - dampen) * (0.48f * acQuant);
        for (int blockY = 0; blockY < verticalSampling; blockY++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ComputeFuzzyErosion(blockY * 2 + 1, 0, useVector);
            ComputeFuzzyErosion(blockY * 2 + 2, preStride, useVector);
            for (int blockX = 0; blockX < BlocksWide; blockX++)
            {
                int x = blockX * 2;
                float value = fuzzy[preStride + x] + fuzzy[preStride + x + 1]
                    + fuzzy[x] + fuzzy[x + 1];
                value = ComputeMask(value);
                value = HfModulation(blockX * 8, 5 + blockY * 8, value);
                value = GammaModulation(blockX * 8, 5 + blockY * 8, value, useVector);
                float field = FastPow2(value * 1.442695041f) * mul + add;
                strengths[blockY * BlocksWide + blockX] = MathF.Max(0f, 0.6f / field - 1f);
            }
        }
    }

    private void ComputePreErosion(int firstRow, int outputRow, bool useVector)
    {
        const float gammaOffset = (float)(0.019 / InputScaling);
        float maskingScale = MathF.Sqrt((float)(211.50759899638012f * 1e8));
        for (int dy = 0; dy < 4; dy++)
        {
            int middle = (firstRow + dy) * lumaStride + 1;
            int x = 0;
            if (useVector)
                for (; x <= paddedWidth - Vector<float>.Count; x += Vector<float>.Count)
                {
                    int index = middle + x;
                    Vector<float> value = new(luma, index);
                    Vector<float> baseValue = new Vector<float>(0.25f)
                        * ((new Vector<float>(luma, index + 1) + new Vector<float>(luma, index - 1))
                        + (new Vector<float>(luma, index - lumaStride) + new Vector<float>(luma, index + lumaStride)));
                    Vector<float> diff = DerivativeRatio(value + new Vector<float>(gammaOffset), invert: false)
                        * (value - baseValue);
                    diff = Vector.Min(diff * diff, new Vector<float>(0.2f));
                    diff = new Vector<float>(0.25f) * Vector.SquareRoot(diff * new Vector<float>(maskingScale) + new Vector<float>(28f));
                    if (dy != 0)
                        diff = new Vector<float>(difference, x) + diff;
                    diff.CopyTo(difference, x);
                }
            for (; x < paddedWidth; x++)
            {
                int index = middle + x;
                float value = luma[index];
                float baseValue = 0.25f * ((luma[index + 1] + luma[index - 1])
                    + (luma[index - lumaStride] + luma[index + lumaStride]));
                float diff = DerivativeRatio(value + gammaOffset, invert: false) * (value - baseValue);
                diff = MathF.Min(diff * diff, 0.2f);
                diff = 0.25f * MathF.Sqrt(diff * maskingScale + 28f);
                difference[x] = dy == 0 ? diff : difference[x] + diff;
            }
        }
        int output = outputRow * preStride;
        for (int x = 0; x < preStride; x++)
        {
            int offset = x * 4;
            preErosion[output + x] = (difference[offset] + difference[offset + 1]
                + difference[offset + 2] + difference[offset + 3]) * 0.25f;
        }
    }

    private void ComputeFuzzyErosion(int row, int output, bool useVector)
    {
        int x = 0;
        // The boundary cells replicate their nearest neighbor; interior cells
        // can load all nine neighbors without gathering or clamping lanes.
        if (useVector && preStride > Vector<float>.Count + 1)
        {
            ComputeFuzzyErosionScalar(row, output, x++);
            for (; x < preStride - Vector<float>.Count; x += Vector<float>.Count)
            {
                Vector<float> min0 = new(float.PositiveInfinity), min1 = min0, min2 = min0, min3 = min0;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        Vector<float> value = new(preErosion, (row + dy) * preStride + x + dx);
                        Vector<float> next0 = Vector.Max(min0, value);
                        Vector<float> next1 = Vector.Max(min1, next0);
                        Vector<float> next2 = Vector.Max(min2, next1);
                        min0 = Vector.Min(min0, value);
                        min1 = Vector.Min(min1, next0);
                        min2 = Vector.Min(min2, next1);
                        min3 = Vector.Min(min3, next2);
                    }
                Vector<float> valueSum = (new Vector<float>(0.125f) * min0 + new Vector<float>(0.075f) * min1)
                    + (new Vector<float>(0.06f) * min2 + new Vector<float>(0.05f) * min3);
                valueSum.CopyTo(fuzzy, output + x);
            }
        }
        for (; x < preStride; x++)
            ComputeFuzzyErosionScalar(row, output, x);
    }

    private void ComputeFuzzyErosionScalar(int row, int output, int x)
    {
        // Keep the four lowest values from the replicated 3x3 neighborhood.
        float min0 = float.PositiveInfinity, min1 = min0, min2 = min0, min3 = min0;
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                float value = preErosion[(row + dy) * preStride + Math.Clamp(x + dx, 0, preStride - 1)];
                float next0 = MathF.Max(min0, value);
                float next1 = MathF.Max(min1, next0);
                float next2 = MathF.Max(min2, next1);
                min0 = MathF.Min(min0, value);
                min1 = MathF.Min(min1, next0);
                min2 = MathF.Min(min2, next1);
                min3 = MathF.Min(min3, next2);
            }
        fuzzy[output + x] = (0.125f * min0 + 0.075f * min1) + (0.06f * min2 + 0.05f * min3);
    }

    private static float ComputeMask(float value)
    {
        float v = MathF.Max(value * 0.74760422233706747f, 1e-3f);
        float v2 = 1f / (v + 305.04035728311436f);
        float v3 = 1f / (v * v + 2.1925739705298404f);
        float v4 = 1f / (v * v + 0.25f * 2.1925739705298404f);
        return -0.74174993f + (3.2353257320940401f * v4
            + (12.906028311180409f * v2 + 5.0220313103171232f * v3));
    }

    private float HfModulation(int x, int y, float value)
    {
        float sum = 0;
        for (int dy = 0; dy < 8; dy++)
        {
            int row = (y + dy) * lumaStride + x + 1;
            int next = dy == 7 ? row : row + lumaStride;
            for (int dx = 0; dx < 8; dx++)
            {
                if (dx != 7) sum += MathF.Abs(luma[row + dx] - luma[row + dx + 1]);
                sum += MathF.Abs(luma[row + dx] - luma[next + dx]);
            }
        }
        const float sumCoeff = (float)(-2.0052193233688884f * InputScaling / 112.0);
        return sum * sumCoeff + value;
    }

    private float GammaModulation(int x, int y, float value, bool useVector)
    {
        float ratio = 0;
        Span<float> derivatives = stackalloc float[8];
        for (int dy = 0; dy < 8; dy++)
        {
            int row = (y + dy) * lumaStride + x + 1;
            int dx = 0;
            if (useVector && Vector<float>.Count <= 8)
                for (; dx <= 8 - Vector<float>.Count; dx += Vector<float>.Count)
                {
                    Vector<float> samples = new Vector<float>(luma, row + dx)
                        + new Vector<float>(0.16f / InputScaling);
                    DerivativeRatio(samples, invert: true).CopyTo(derivatives[dx..]);
                }
            for (int column = 0; column < dx; column++)
                ratio += derivatives[column];
            // Retain the scalar reduction order: a parallel sum can change
            // the final quantized coefficients at a rounding boundary.
            for (; dx < 8; dx++)
                ratio += DerivativeRatio(luma[row + dx] + 0.16f / InputScaling, invert: true);
        }
        ratio *= InputScaling / 64f;
        return (-0.15526878023684174f * InvLog2E) * FastLog2(ratio) + value;
    }

    private static float DerivativeRatio(float value, bool invert)
    {
        value = MathF.Max(value, 0);
        float squared = value * value;
        float numerator = NumMul * squared + NumOffset;
        float denominator = (DenMul * value) * squared + VOffset;
        return invert ? numerator / denominator : denominator / numerator;
    }

    private static Vector<float> DerivativeRatio(Vector<float> value, bool invert)
    {
        value = Vector.Max(value, Vector<float>.Zero);
        Vector<float> squared = value * value;
        Vector<float> numerator = new Vector<float>(NumMul) * squared + new Vector<float>(NumOffset);
        Vector<float> denominator = (new Vector<float>(DenMul) * value) * squared + new Vector<float>(VOffset);
        return invert ? numerator / denominator : denominator / numerator;
    }

    private static float FastLog2(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        int exponent = unchecked(bits - 0x3f2aaaab) >> 23;
        float mantissa = BitConverter.Int32BitsToSingle(unchecked(bits - (exponent << 23))) - 1f;
        float numerator = (0.74245873327820566f * mantissa + 1.4287160470083755f)
            * mantissa - 1.8503833400518310e-6f;
        float denominator = (0.17409343003366853f * mantissa + 1.0096718572241148f)
            * mantissa + 0.99032814277590719f;
        return numerator / denominator + exponent;
    }

    private static float FastPow2(float value)
    {
        float floor = MathF.Floor(value);
        float exponent = BitConverter.Int32BitsToSingle(((int)floor + 127) << 23);
        float fraction = value - floor;
        float numerator = fraction + 1.01749063e1f;
        numerator = numerator * fraction + 4.88687798e1f;
        numerator = numerator * fraction + 9.85506591e1f;
        numerator *= exponent;
        float denominator = fraction * 2.10242958e-1f - 2.22328856e-2f;
        denominator = denominator * fraction - 1.94414990e1f;
        denominator = denominator * fraction + 9.85506633e1f;
        return numerator / denominator;
    }
}
