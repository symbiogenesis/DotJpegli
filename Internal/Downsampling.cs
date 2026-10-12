// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

// Scalar adaptation of DownsampleRow2x1 and Downsample2x2 in
// google/jpegli lib/jpegli/downsample.cc, and PadInputBuffer in encode.cc,
// revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Reading and converting edge pixels on demand is equivalent to padding the
// upstream planar input, and keeps temporary storage bounded to one block.
internal static class Downsampling
{
    internal static void FillBlock(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int left, int top,
        int sampleWidth, int sampleHeight, Span<float> samples)
    {
        for (int row = 0; row < 8; row++)
        {
            int y = top + row * sampleHeight;
            for (int column = 0; column < 8; column++)
            {
                int x = left + column * sampleWidth;
                float sample = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y);
                if (sampleWidth == 2)
                {
                    float right = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y);
                    if (sampleHeight == 2)
                    {
                        float bottom = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x, y + 1);
                        float bottomRight = ReadSample(pixels, width, height, stride, bytesPerPixel, layout, component, x + 1, y + 1);
                        // Preserve upstream's pairwise addition before scaling.
                        sample = ((sample + right) + (bottom + bottomRight)) * 0.25f;
                    }
                    else
                    {
                        sample = (sample + right) * 0.5f;
                    }
                }
                samples[row * 8 + column] = sample - 128f;
            }
        }
    }

    private static float ReadSample(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int x, int y)
    {
        int offset = Math.Min(y, height - 1) * stride + Math.Min(x, width - 1) * bytesPerPixel;
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
