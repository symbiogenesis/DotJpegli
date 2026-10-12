// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

// Scalar adaptation of DownsampleRow2x1, Downsample1x2, DownsampleRow4x1,
// Downsample2x2 and ApplyInputSmoothing in
// google/jpegli lib/jpegli/downsample.cc, and PadInputBuffer in encode.cc,
// revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Reading and converting edge pixels on demand is equivalent to padding the
// upstream planar input, and keeps temporary storage bounded to one block.
internal static class Downsampling
{
    internal static void FillBlock(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int left, int top,
        int sampleWidth, int sampleHeight, Span<float> samples, int smoothingFactor = 0)
    {
        float neighborWeight = smoothingFactor / 1024f;
        float centerWeight = 1f - 8f * neighborWeight;
        for (int row = 0; row < 8; row++)
        {
            int y = top + row * sampleHeight;
            for (int column = 0; column < 8; column++)
            {
                int x = left + column * sampleWidth;
                float sample = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y,
                    neighborWeight, centerWeight);
                if (sampleWidth == 2)
                {
                    float right = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y,
                        neighborWeight, centerWeight);
                    if (sampleHeight == 2)
                    {
                        float bottom = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y + 1,
                            neighborWeight, centerWeight);
                        float bottomRight = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y + 1,
                            neighborWeight, centerWeight);
                        // Preserve upstream's pairwise addition before scaling.
                        sample = ((sample + right) + (bottom + bottomRight)) * 0.25f;
                    }
                    else
                    {
                        sample = (sample + right) * 0.5f;
                    }
                }
                else if (sampleWidth == 4)
                {
                    float second = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y,
                        neighborWeight, centerWeight);
                    float third = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 2, y,
                        neighborWeight, centerWeight);
                    float fourth = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 3, y,
                        neighborWeight, centerWeight);
                    // DownsampleRow4x1 also adds pairs before scaling.
                    sample = ((sample + second) + (third + fourth)) * 0.25f;
                }
                else if (sampleHeight == 2)
                {
                    float bottom = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y + 1,
                        neighborWeight, centerWeight);
                    sample = (sample + bottom) * 0.5f;
                }
                samples[row * 8 + column] = sample - 128f;
            }
        }
    }

    private static float ReadSample(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int x, int y, float neighborWeight, float centerWeight)
    {
        float center = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y);
        if (neighborWeight == 0f)
            return center;

        // Smooth converted, uncentered samples before downsampling. Keep x and
        // y in the padded image so each raw neighbor clamps independently.
        float topLeft = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x - 1, y - 1);
        float top = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y - 1);
        float topRight = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y - 1);
        float left = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x - 1, y);
        float right = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y);
        float bottomLeft = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x - 1, y + 1);
        float bottom = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y + 1);
        float bottomRight = ReadRawSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y + 1);
        // Preserve upstream's left-to-right sum of the eight neighbors.
        float neighbors = topLeft + top + topRight + left + right + bottomLeft + bottom + bottomRight;
        return center * centerWeight + neighbors * neighborWeight;
    }

    private static float ReadRawSample(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int x, int y)
    {
        int offset = Math.Clamp(y, 0, height - 1) * stride + Math.Clamp(x, 0, width - 1) * bytesPerPixel;
        if (layout == PixelLayout.Gray8)
            return pixels[offset];

        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        byte r = pixels[offset + (bgr ? 2 : 0)];
        byte g = pixels[offset + 1];
        byte b = pixels[offset + (bgr ? 0 : 2)];
        ColorConversion.RgbToYCbCr(r, g, b, out float luma, out float cb, out float cr);
        return component switch { 0 => luma, 1 => cb, _ => cr };
    }
}
