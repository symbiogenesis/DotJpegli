// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

using System.Numerics;

// Reuse the three converted halo rows needed by the original eight-neighbor
// smoother. Each lane retains its left-to-right additions and separate products.
// The fixed tile also retains the original horizontal/vertical averaging order.
internal static class SmoothedDownsampling
{
    internal static void FillBlock(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight,
        Span<float> samples, int smoothingFactor)
    {
        if (smoothingFactor == 0 || sampleWidth is < 1 or > 4 || sampleHeight is < 1 or > 4)
        {
            Downsampling.FillBlockScalar(pixels, width, height, stride, component, left, top,
                sampleWidth, sampleHeight, samples, smoothingFactor);
            return;
        }
        int columns = 8 * sampleWidth, rows = 8 * sampleHeight, haloWidth = columns + 2;
        Span<float> halo = stackalloc float[3 * 34];
        Span<float> previous = halo[..haloWidth];
        Span<float> current = halo.Slice(34, haloWidth);
        Span<float> next = halo.Slice(68, haloWidth);
        Span<float> tile = stackalloc float[32 * 32];
        pixels.ReadRowScalar(width, height, stride, component, left - 1, top - 1, previous);
        pixels.ReadRowScalar(width, height, stride, component, left - 1, top, current);
        float neighborWeight = smoothingFactor / 1024f;
        float centerWeight = 1f - (8f * neighborWeight);
        for (int row = 0; row < rows; row++)
        {
            pixels.ReadRowScalar(width, height, stride, component, left - 1, top + row + 1, next);
            SmoothRow(previous, current, next, tile.Slice(row * columns, columns), neighborWeight, centerWeight);
            Span<float> recycled = previous;
            previous = current;
            current = next;
            next = recycled;
        }
        bool quarter = sampleWidth == 2 && sampleHeight == 2;
        float sharpness = quarter ? pixels.GetChromaDownsamplingSharpness(component) : 0f;
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                int offset = (row * sampleHeight * columns) + (column * sampleWidth);
                float sample;
                if (quarter && sharpness > 0f)
                {
                    sample = Downsampling.Downsample2x2Quad(tile[offset], tile[offset + 1],
                        tile[offset + columns], tile[offset + columns + 1], sharpness);
                }
                else
                {
                    float first = AverageRow(tile, offset, sampleWidth, quarter);
                    sample = first;
                    if (sampleHeight != 1)
                    {
                        float second = AverageRow(tile, offset + columns, sampleWidth, quarter);
                        if (sampleHeight == 2)
                        {
                            sample = (first + second) * (quarter ? 0.25f : 0.5f);
                        }
                        else
                        {
                            float third = AverageRow(tile, offset + (2 * columns), sampleWidth, quarter);
                            sample = sampleHeight == 3
                                ? (first + second + third) * (1f / 3f)
                                : (first + second + (third + AverageRow(tile, offset + (3 * columns), sampleWidth, quarter))) * 0.25f;
                        }
                    }
                }
                samples[(row * 8) + column] = sample - 128f;
            }
        }
    }

    private static void SmoothRow(ReadOnlySpan<float> previous, ReadOnlySpan<float> current,
        ReadOnlySpan<float> next, Span<float> destination, float neighborWeight, float centerWeight)
    {
        int column = 0, lanes = Vector<float>.Count;
        if (Vector.IsHardwareAccelerated && lanes <= 8)
        {
            var neighborScale = new Vector<float>(neighborWeight);
            var centerScale = new Vector<float>(centerWeight);
            for (; column <= destination.Length - lanes; column += lanes)
            {
                Vector<float> neighbors = PortableVector.Load(previous.Slice(column, lanes))
                    + PortableVector.Load(previous.Slice(column + 1, lanes))
                    + PortableVector.Load(previous.Slice(column + 2, lanes))
                    + PortableVector.Load(current.Slice(column, lanes))
                    + PortableVector.Load(current.Slice(column + 2, lanes))
                    + PortableVector.Load(next.Slice(column, lanes))
                    + PortableVector.Load(next.Slice(column + 1, lanes))
                    + PortableVector.Load(next.Slice(column + 2, lanes));
                ((PortableVector.Load(current.Slice(column + 1, lanes)) * centerScale)
                    + (neighbors * neighborScale)).CopyTo(destination.Slice(column, lanes));
            }
        }
        for (; column < destination.Length; column++)
        {
            float neighbors = previous[column] + previous[column + 1] + previous[column + 2]
                + current[column] + current[column + 2] + next[column] + next[column + 1] + next[column + 2];
            destination[column] = (current[column + 1] * centerWeight) + (neighbors * neighborWeight);
        }
    }

    private static float AverageRow(ReadOnlySpan<float> tile, int offset, int width, bool quarter)
    {
        float first = tile[offset];
        if (width == 1)
        {
            return first;
        }
        float second = tile[offset + 1];
        if (width == 2)
        {
            return quarter ? first + second : (first + second) * 0.5f;
        }
        float third = tile[offset + 2];
        return width == 3 ? (first + second + third) * (1f / 3f)
            : (first + second + (third + tile[offset + 3])) * 0.25f;
    }
}
