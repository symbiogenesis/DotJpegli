// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

// Scalar adaptation of the horizontal 2/3/4 and vertical 2/3/4 downsampling
// methods and ApplyInputSmoothing in
// google/jpegli lib/jpegli/downsample.cc, and PadInputBuffer in encode.cc,
// revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Reading and converting edge pixels on demand is equivalent to padding the
// upstream planar input, and keeps temporary storage bounded to one block.
// Three-row sampling uses the dedicated Downsample1x3/Downsample2x3 kernels;
// the pinned upstream selector instead routes vertical factor 3 to two-row kernels.
internal static class Downsampling
{
    internal static void FillBlock(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int left, int top,
        int sampleWidth, int sampleHeight, Span<float> samples, int smoothingFactor = 0)
        => FillBlock(new PixelSource(pixels, layout), width, height, stride, component, left, top,
            sampleWidth, sampleHeight, samples, smoothingFactor);

    internal static void FillBlock(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight,
        Span<float> samples, int smoothingFactor = 0)
    {
        float neighborWeight = smoothingFactor / 1024f;
        float centerWeight = 1f - 8f * neighborWeight;
        for (int row = 0; row < 8; row++)
        {
            int y = top + row * sampleHeight;
            for (int column = 0; column < 8; column++)
            {
                int x = left + column * sampleWidth;
                float sample;
                if (sampleWidth == 2 && sampleHeight == 2)
                {
                    float first = ReadSample(pixels, width, height, stride, component, x, y,
                        neighborWeight, centerWeight);
                    float right = ReadSample(pixels, width, height, stride, component, x + 1, y,
                        neighborWeight, centerWeight);
                    float bottom = ReadSample(pixels, width, height, stride, component, x, y + 1,
                        neighborWeight, centerWeight);
                    float bottomRight = ReadSample(pixels, width, height, stride, component, x + 1, y + 1,
                        neighborWeight, centerWeight);
                    // Downsample2x2 scales only once, after summing both row pairs.
                    sample = ((first + right) + (bottom + bottomRight)) * 0.25f;
                }
                else
                {
                    // Other combined methods first downsample each row, then
                    // average vertically. Preserve those intermediate roundings.
                    sample = ReadRowSample(pixels, width, height, stride, component, x, y,
                        sampleWidth, neighborWeight, centerWeight);
                    if (sampleHeight != 1)
                    {
                        float second = ReadRowSample(pixels, width, height, stride, component, x, y + 1,
                            sampleWidth, neighborWeight, centerWeight);
                        if (sampleHeight == 2)
                            sample = (sample + second) * 0.5f;
                        else
                        {
                            float third = ReadRowSample(pixels, width, height, stride, component, x, y + 2,
                                sampleWidth, neighborWeight, centerWeight);
                            if (sampleHeight == 3)
                                sample = ((sample + second) + third) * (1f / 3f);
                            else
                            {
                                float fourth = ReadRowSample(pixels, width, height, stride, component, x, y + 3,
                                    sampleWidth, neighborWeight, centerWeight);
                                sample = ((sample + second) + (third + fourth)) * 0.25f;
                            }
                        }
                    }
                }
                samples[row * 8 + column] = sample - 128f;
            }
        }
    }

    private static float ReadRowSample(PixelSource pixels, int width, int height, int stride,
        int component, int x, int y, int sampleWidth, float neighborWeight, float centerWeight)
    {
        float first = ReadSample(pixels, width, height, stride, component, x, y,
            neighborWeight, centerWeight);
        if (sampleWidth == 1)
            return first;
        float second = ReadSample(pixels, width, height, stride, component, x + 1, y,
            neighborWeight, centerWeight);
        if (sampleWidth == 2)
            return (first + second) * 0.5f;
        float third = ReadSample(pixels, width, height, stride, component, x + 2, y,
            neighborWeight, centerWeight);
        if (sampleWidth == 3)
            return ((first + second) + third) * (1f / 3f);
        float fourth = ReadSample(pixels, width, height, stride, component, x + 3, y,
            neighborWeight, centerWeight);
        return ((first + second) + (third + fourth)) * 0.25f;
    }

    private static float ReadSample(PixelSource pixels, int width, int height, int stride,
        int component, int x, int y, float neighborWeight, float centerWeight)
    {
        float center = ReadRawSample(pixels, width, height, stride, component, x, y);
        if (neighborWeight == 0f)
            return center;

        // Smooth converted, uncentered samples before downsampling. Keep x and
        // y in the padded image so each raw neighbor clamps independently.
        float topLeft = ReadRawSample(pixels, width, height, stride, component, x - 1, y - 1);
        float top = ReadRawSample(pixels, width, height, stride, component, x, y - 1);
        float topRight = ReadRawSample(pixels, width, height, stride, component, x + 1, y - 1);
        float left = ReadRawSample(pixels, width, height, stride, component, x - 1, y);
        float right = ReadRawSample(pixels, width, height, stride, component, x + 1, y);
        float bottomLeft = ReadRawSample(pixels, width, height, stride, component, x - 1, y + 1);
        float bottom = ReadRawSample(pixels, width, height, stride, component, x, y + 1);
        float bottomRight = ReadRawSample(pixels, width, height, stride, component, x + 1, y + 1);
        // Preserve upstream's left-to-right sum of the eight neighbors.
        float neighbors = topLeft + top + topRight + left + right + bottomLeft + bottom + bottomRight;
        return center * centerWeight + neighbors * neighborWeight;
    }

    private static float ReadRawSample(PixelSource pixels, int width, int height, int stride,
        int component, int x, int y)
    {
        int offset = Math.Clamp(y, 0, height - 1) * stride
            + Math.Clamp(x, 0, width - 1) * pixels.ComponentsPerPixel;
        return pixels.ReadSample(offset, component);
    }
}
