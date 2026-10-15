// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace DotJpegli.Internal;

// Managed adaptation of the horizontal 2/3/4 and vertical 2/3/4 downsampling
// methods and ApplyInputSmoothing in
// google/jpegli lib/jpegli/downsample.cc, and PadInputBuffer in encode.cc,
// revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Reading and converting edge pixels on demand is equivalent to padding the
// upstream planar input, and keeps temporary storage bounded to fixed row/block scratch.
// Custom sample areas compose the horizontal and vertical ordered averages.
// Three-row sampling follows Downsample1x3/Downsample2x3's summation order;
// the pinned upstream selector instead routes vertical factor 3 to two-row kernels.
internal static class Downsampling
{
    // Block coordinates are in the component's output grid. Integer ratios
    // retain the existing ordered kernels and automatic SIMD dispatch.
    internal static void FillBlock(PixelSource pixels, int width, int height, int stride,
        int component, int blockX, int blockY, int maxHorizontal, int maxVertical,
        int componentHorizontal, int componentVertical, Span<float> samples, int smoothingFactor = 0)
    {
        if (maxHorizontal % componentHorizontal == 0 && maxVertical % componentVertical == 0)
        {
            int sampleWidth = maxHorizontal / componentHorizontal;
            int sampleHeight = maxVertical / componentVertical;
            FillBlock(pixels, width, height, stride, component, blockX * 8 * sampleWidth,
                blockY * 8 * sampleHeight, sampleWidth, sampleHeight, samples, smoothingFactor);
            return;
        }

        float neighborWeight = smoothingFactor / 1024f;
        float centerWeight = 1f - 8f * neighborWeight;
        // Express pixel and sample boundaries in integer units of 1/factor,
        // avoiding drift at block/MCU boundaries for ratios such as 3/2 and 4/3.
        for (int row = 0; row < 8; row++)
        {
            int yStart = (blockY * 8 + row) * maxVertical;
            int yEnd = yStart + maxVertical;
            for (int column = 0; column < 8; column++)
            {
                int xStart = (blockX * 8 + column) * maxHorizontal;
                int xEnd = xStart + maxHorizontal;
                float total = 0f;
                for (int y = yStart / componentVertical; y * componentVertical < yEnd; y++)
                {
                    int yWeight = Math.Min(yEnd, (y + 1) * componentVertical)
                        - Math.Max(yStart, y * componentVertical);
                    float horizontal = 0f;
                    for (int x = xStart / componentHorizontal; x * componentHorizontal < xEnd; x++)
                    {
                        int xWeight = Math.Min(xEnd, (x + 1) * componentHorizontal)
                            - Math.Max(xStart, x * componentHorizontal);
                        horizontal += ReadSample(pixels, width, height, stride, component, x, y,
                            neighborWeight, centerWeight) * xWeight;
                    }
                    total += (horizontal / maxHorizontal) * yWeight;
                }
                samples[row * 8 + column] = total / maxVertical - 128f;
            }
        }
    }

    internal static void FillBlock(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int left, int top,
        int sampleWidth, int sampleHeight, Span<float> samples, int smoothingFactor = 0)
        => FillBlock(new PixelSource(pixels, layout), width, height, stride, component, left, top,
            sampleWidth, sampleHeight, samples, smoothingFactor);

    internal static void FillBlock(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight,
        Span<float> samples, int smoothingFactor = 0)
    {
        if (Vector.IsHardwareAccelerated && CanUseVector && smoothingFactor == 0)
            FillBlockVector(pixels, width, height, stride, component, left, top, sampleWidth, sampleHeight, samples);
        else
            FillBlockScalar(pixels, width, height, stride, component, left, top, sampleWidth, sampleHeight, samples, smoothingFactor);
    }

    // Explicit paths make operation ordering and fallback behavior testable.
    internal static void FillBlockVector(PixelSource pixels, int width, int height, int stride,
        int component, int left, int top, int sampleWidth, int sampleHeight,
        Span<float> samples, int smoothingFactor = 0)
    {
        if (!CanUseVector || smoothingFactor != 0 || sampleWidth is < 1 or > 4 || sampleHeight is < 1 or > 4)
        {
            FillBlockScalar(pixels, width, height, stride, component, left, top, sampleWidth, sampleHeight, samples, smoothingFactor);
            return;
        }

        bool direct = sampleWidth == 1 && sampleHeight == 1;
        bool directQuarter = sampleWidth == 2 && sampleHeight == 2
            && (Vector256.IsHardwareAccelerated || Vector128.IsHardwareAccelerated);
        if ((direct || directQuarter) && left >= 0 && top >= 0
            && left <= width - 8 * sampleWidth && top <= height - 8 * sampleHeight)
        {
            ReadOnlySpan<float> plane = pixels.GetConvertedPlane(component);
            if (!plane.IsEmpty)
            {
                if (direct)
                    FillConvertedBlock(plane, width, left, top, samples);
                else
                    FillConvertedQuarterBlock(plane, width, left, top, samples);
                return;
            }
        }

        Span<float> rawRow = stackalloc float[32];
        // Each consecutive eight-sample group supplies one horizontal term
        // for all output columns. At most four terms in each of four rows.
        Span<float> terms = stackalloc float[128];
        int lanes = Vector<float>.Count;
        bool quarter = sampleWidth == 2 && sampleHeight == 2;
        var center = new Vector<float>(128f);
        if (sampleWidth == 1 && sampleHeight == 1)
        {
            for (int row = 0; row < 8; row++)
            {
                Span<float> destination = samples.Slice(row * 8, 8);
                pixels.ReadRowVector(width, height, stride, component, left, top + row, destination);
                for (int column = 0; column < 8; column += lanes)
                    (new Vector<float>(destination.Slice(column, lanes)) - center).CopyTo(destination.Slice(column, lanes));
            }
            return;
        }
        for (int row = 0; row < 8; row++)
        {
            for (int dy = 0; dy < sampleHeight; dy++)
            {
                pixels.ReadRowVector(width, height, stride, component, left, top + row * sampleHeight + dy,
                    rawRow[..(8 * sampleWidth)]);
                for (int dx = 0; dx < sampleWidth; dx++)
                    for (int column = 0; column < 8; column++)
                        terms[(dy * sampleWidth + dx) * 8 + column] = rawRow[column * sampleWidth + dx];
            }

            for (int column = 0; column < 8; column += lanes)
            {
                Vector<float> first = AverageRowVector(terms, 0, column, sampleWidth, quarter);
                Vector<float> sample = first;
                if (sampleHeight != 1)
                {
                    Vector<float> second = AverageRowVector(terms, 1, column, sampleWidth, quarter);
                    if (sampleHeight == 2)
                        sample = (first + second) * new Vector<float>(quarter ? 0.25f : 0.5f);
                    else
                    {
                        Vector<float> third = AverageRowVector(terms, 2, column, sampleWidth, quarter);
                        sample = sampleHeight == 3
                            ? ((first + second) + third) * new Vector<float>(1f / 3f)
                            : ((first + second) + (third + AverageRowVector(terms, 3, column, sampleWidth, quarter)))
                                * new Vector<float>(0.25f);
                    }
                }
                (sample - center).CopyTo(samples.Slice(row * 8 + column, lanes));
            }
        }
    }

    private static bool CanUseVector => Vector<float>.Count <= 8 && 8 % Vector<float>.Count == 0;

    private static void FillConvertedBlock(ReadOnlySpan<float> plane, int width, int left, int top, Span<float> samples)
    {
        int lanes = Vector<float>.Count;
        var center = new Vector<float>(128f);
        for (int row = 0; row < 8; row++)
        {
            int source = (top + row) * width + left;
            for (int column = 0; column < 8; column += lanes)
                (new Vector<float>(plane.Slice(source + column, lanes)) - center)
                    .CopyTo(samples.Slice(row * 8 + column, lanes));
        }
    }

    private static void FillConvertedQuarterBlock(ReadOnlySpan<float> plane, int width, int left, int top,
        Span<float> samples)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            var scale = Vector256.Create(0.25f);
            var center = Vector256.Create(128f);
            for (int row = 0; row < 8; row++)
            {
                int source = (top + row * 2) * width + left;
                Vector256<float> first = PairwiseSums256(plane.Slice(source, 16));
                Vector256<float> second = PairwiseSums256(plane.Slice(source + width, 16));
                // Downsample2x2 adds each row's pairs before the single scale.
                ((first + second) * scale - center).CopyTo(samples.Slice(row * 8, 8));
            }
        }
        else
        {
            var scale = Vector128.Create(0.25f);
            var center = Vector128.Create(128f);
            for (int row = 0; row < 8; row++)
                for (int column = 0; column < 8; column += 4)
                {
                    int source = (top + row * 2) * width + left + column * 2;
                    Vector128<float> first = PairwiseSums128(plane.Slice(source, 8));
                    Vector128<float> second = PairwiseSums128(plane.Slice(source + width, 8));
                    ((first + second) * scale - center).CopyTo(samples.Slice(row * 8 + column, 4));
                }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<float> PairwiseSums256(ReadOnlySpan<float> samples)
    {
        Vector256<float> first = Vector256.Create(samples[..8]);
        Vector256<float> second = Vector256.Create(samples[8..]);
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        return PairwiseSumsNet11(first, second);
#else
        return PairwiseSumsPortable(first, second);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<float> PairwiseSums128(ReadOnlySpan<float> samples)
    {
        Vector128<float> first = Vector128.Create(samples[..4]);
        Vector128<float> second = Vector128.Create(samples[4..]);
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        return PairwiseSumsNet11(first, second);
#else
        return PairwiseSumsPortable(first, second);
#endif
    }

    // Keep both lane-composition variants portable. Only data movement changes;
    // each even sample is still added to its adjacent odd sample exactly once.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<float> PairwiseSumsPortable(Vector256<float> first, Vector256<float> second)
    {
        var even = Vector256.Create(0, 2, 4, 6, 0, 2, 4, 6);
        var odd = Vector256.Create(1, 3, 5, 7, 1, 3, 5, 7);
        Vector256<float> evenSamples = Vector256.Create(Vector256.Shuffle(first, even).GetLower(), Vector256.Shuffle(second, even).GetLower());
        Vector256<float> oddSamples = Vector256.Create(Vector256.Shuffle(first, odd).GetLower(), Vector256.Shuffle(second, odd).GetLower());
        return evenSamples + oddSamples;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<float> PairwiseSumsPortable(Vector128<float> first, Vector128<float> second)
    {
        var even = Vector128.Create(0, 2, 0, 2);
        var odd = Vector128.Create(1, 3, 1, 3);
        Vector128<float> evenSamples = Vector128.Create(Vector128.Shuffle(first, even).GetLower(), Vector128.Shuffle(second, even).GetLower());
        Vector128<float> oddSamples = Vector128.Create(Vector128.Shuffle(first, odd).GetLower(), Vector128.Shuffle(second, odd).GetLower());
        return evenSamples + oddSamples;
    }

#if NET11_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<float> PairwiseSumsNet11(Vector256<float> first, Vector256<float> second)
        => Vector256.UnzipEven(first, second) + Vector256.UnzipOdd(first, second);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<float> PairwiseSumsNet11(Vector128<float> first, Vector128<float> second)
        => Vector128.UnzipEven(first, second) + Vector128.UnzipOdd(first, second);
#endif

    private static Vector<float> AverageRowVector(ReadOnlySpan<float> terms, int row, int column,
        int sampleWidth, bool quarter)
    {
        int start = row * sampleWidth * 8 + column;
        int lanes = Vector<float>.Count;
        Vector<float> first = new(terms.Slice(start, lanes));
        if (sampleWidth == 1)
            return first;
        Vector<float> second = new(terms.Slice(start + 8, lanes));
        if (sampleWidth == 2)
            return quarter ? first + second : (first + second) * new Vector<float>(0.5f);
        Vector<float> third = new(terms.Slice(start + 16, lanes));
        if (sampleWidth == 3)
            return ((first + second) + third) * new Vector<float>(1f / 3f);
        Vector<float> fourth = new(terms.Slice(start + 24, lanes));
        return ((first + second) + (third + fourth)) * new Vector<float>(0.25f);
    }

    internal static void FillBlockScalar(PixelSource pixels, int width, int height, int stride,
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
        return pixels.ReadSampleAt(Math.Clamp(x, 0, width - 1), Math.Clamp(y, 0, height - 1), stride, component);
    }
}
