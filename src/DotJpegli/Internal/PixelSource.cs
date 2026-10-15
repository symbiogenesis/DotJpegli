// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Buffers;
using System.Numerics;
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

namespace DotJpegli.Internal;

/// <summary>Reads source samples in fractional JPEG units, optionally reusing converted planes.</summary>
internal readonly ref struct PixelSource
{
    private readonly ReadOnlySpan<byte> bytes;
    private readonly ReadOnlySpan<ushort> words;
    private readonly ReadOnlySpan<float> floats;
    private readonly SourceType sourceType;
    private readonly bool bgr;
    private readonly bool ycbcr;
    private readonly bool xyb;
    private readonly JpegColorSpace colorSpace;
    private readonly IccColorTransform? colorTransform;
    private readonly ConvertedPlanes? convertedPlanes;

    internal PixelSource(ReadOnlySpan<byte> pixels, PixelLayout layout)
    {
        bytes = pixels;
        sourceType = SourceType.Byte;
        ComponentsPerPixel = layout switch
        {
            PixelLayout.Rgb24 or PixelLayout.Bgr24 or PixelLayout.YCbCr24 or PixelLayout.Xyb24 => 3,
            PixelLayout.Rgba32 or PixelLayout.Bgra32 => 4,
            PixelLayout.Gray8 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };
        bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        ycbcr = layout == PixelLayout.YCbCr24;
        xyb = layout == PixelLayout.Xyb24;
    }

    internal PixelSource(ReadOnlySpan<ushort> pixels, PixelComponentLayout layout)
    {
        words = pixels;
        sourceType = SourceType.UInt16;
        ComponentsPerPixel = GetComponentCount(layout);
        bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        ycbcr = layout == PixelComponentLayout.YCbCr;
        xyb = layout == PixelComponentLayout.Xyb;
    }

    internal PixelSource(ReadOnlySpan<float> pixels, PixelComponentLayout layout)
    {
        floats = pixels;
        sourceType = SourceType.Float;
        ComponentsPerPixel = GetComponentCount(layout);
        bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        ycbcr = layout == PixelComponentLayout.YCbCr;
        xyb = layout == PixelComponentLayout.Xyb;
    }

    internal int ComponentsPerPixel { get; }
    internal ParallelDctRows? ParallelRows { get; }
    // The encode call owns this lease; worker snapshots only borrow its storage.
    internal IDisposable? ConvertedStorage => convertedPlanes;
    internal ReadOnlySpan<float> GetConvertedPlane(int component)
        => convertedPlanes is null || (convertedPlanes.Is420 && component != 0) ? default : convertedPlanes.Samples.AsSpan(
            component * convertedPlanes.PlaneLength, convertedPlanes.PlaneLength);
    internal ReadOnlySpan<float> GetConverted420ChromaPlane(int component, out int width, out int height)
    {
        width = convertedPlanes?.ChromaWidth ?? 0;
        height = convertedPlanes?.ChromaHeight ?? 0;
        return convertedPlanes is not { Is420: true } || component is < 1 or > 2 ? default
            : convertedPlanes.Samples.AsSpan(convertedPlanes.PlaneLength
                + (component - 1) * convertedPlanes.ChromaLength, convertedPlanes.ChromaLength);
    }
    internal bool IsGrayscaleInput => ComponentsPerPixel == 1;
    internal bool CanManageSourceColor => !ycbcr && !xyb;
    internal JpegColorSpace ColorSpace => colorSpace == JpegColorSpace.Automatic
        ? (xyb ? JpegColorSpace.Xyb : IsGrayscaleInput ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr) : colorSpace;
    internal bool IsGrayscale => ColorSpace == JpegColorSpace.Grayscale;
    // XYB is carried in an RGB JPEG: identifiers, entropy selectors and AQ rules
    // are shared, while sampling defaults and quantization matrices differ.
    internal bool IsRgb => ColorSpace is JpegColorSpace.Rgb or JpegColorSpace.Xyb;
    internal bool IsXyb => ColorSpace == JpegColorSpace.Xyb;
    internal int AnalysisComponent => IsRgb ? 1 : 0;

    private PixelSource(PixelSource source, JpegColorSpace colorSpace, ParallelDctRows? parallelRows = null,
        IccColorTransform? colorTransform = null, ConvertedPlanes? convertedPlanes = null,
        bool discardConvertedPlanes = false)
    {
        bytes = source.bytes;
        words = source.words;
        floats = source.floats;
        sourceType = source.sourceType;
        bgr = source.bgr;
        ycbcr = source.ycbcr;
        xyb = source.xyb;
        ComponentsPerPixel = source.ComponentsPerPixel;
        this.colorSpace = colorSpace;
        ParallelRows = parallelRows;
        this.colorTransform = colorTransform ?? source.colorTransform;
        this.convertedPlanes = discardConvertedPlanes ? null : convertedPlanes ?? source.convertedPlanes;
    }

    internal PixelSource WithParallelRows(ParallelDctRows rows) => new(this, colorSpace, rows);

    internal PixelSource WithColorTransform(IccColorTransform transform) => new(this, colorSpace,
        colorTransform: transform, discardConvertedPlanes: true);

    /// <summary>Converts each source pixel once for reuse by analysis and every encoding scan.</summary>
    internal PixelSource WithConvertedPlanes(int width, int height, int stride,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (convertedPlanes is not null)
            return this;
        // Preserve bounded memory use for very large images. The existing
        // streaming reader remains available when pooled planes exceed 64 MiB.
        const int maximumSamples = 64 * 1024 * 1024 / sizeof(float);
        int components = IsGrayscale ? 1 : 3;
        ulong samples = (ulong)((long)width * height * components);
        if (BitOperations.RoundUpToPowerOf2(samples) > maximumSamples)
            return this;
        var planes = new ConvertedPlanes(width, height, stride, Length, components);
        try
        {
            int planeLength = planes.PlaneLength;
            bool vectorRgb = Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian
                && Vector<float>.Count is 4 or 8 && sourceType == SourceType.Byte
                && !IsGrayscaleInput && !ycbcr && !xyb && colorTransform is null
                && ColorSpace is JpegColorSpace.YCbCr or JpegColorSpace.Grayscale;
            for (int y = 0; y < height; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int destination = y * width;
                ConvertRow(y * stride, planes.Samples.AsSpan(destination, width),
                    components == 1 ? default : planes.Samples.AsSpan(planeLength + destination, width),
                    components == 1 ? default : planes.Samples.AsSpan(2 * planeLength + destination, width), vectorRgb);
            }
            return new PixelSource(this, colorSpace, convertedPlanes: planes);
        }
        catch
        {
            planes.Dispose();
            throw;
        }
    }

    // Retain full-resolution Y for adaptive analysis, but feed DCT with the
    // exact ordered 2x2 chroma averages. Only two source rows of chroma scratch
    // are needed; no full-resolution Cb/Cr image is written or reread.
    internal PixelSource WithConverted420Planes(int width, int height, int stride,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (convertedPlanes is not null)
            return this;
        if (sourceType != SourceType.Byte || IsGrayscaleInput || ycbcr || xyb
            || colorTransform is not null || ColorSpace != JpegColorSpace.YCbCr)
            return WithConvertedPlanes(width, height, stride, cancellationToken);

        int chromaWidth = checked((width + 15) / 16 * 8);
        int chromaHeight = checked((height + 15) / 16 * 8);
        ulong sampleCount = (ulong)((long)width * height + 2L * chromaWidth * chromaHeight);
        ulong scratchCount = (ulong)width * 4;
        // Include both actual shared-pool bucket sizes in the existing 64 MiB
        // budget, including the transient row scratch (minimum bucket: 16).
        const ulong maximumSamples = 64 * 1024 * 1024 / sizeof(float);
        if (Math.Max(16UL, BitOperations.RoundUpToPowerOf2(sampleCount))
            + Math.Max(16UL, BitOperations.RoundUpToPowerOf2(scratchCount)) > maximumSamples)
            return WithConvertedPlanes(width, height, stride, cancellationToken);

        var planes = new ConvertedPlanes(width, height, stride, Length, 3, is420: true);
        float[]? scratch = null;
        try
        {
            scratch = ArrayPool<float>.Shared.Rent(checked(width * 4));
            Span<float> firstCb = scratch.AsSpan(0, width), firstCr = scratch.AsSpan(width, width);
            Span<float> secondCb = scratch.AsSpan(2 * width, width), secondCr = scratch.AsSpan(3 * width, width);
            bool vectorRgb = Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian && Vector<float>.Count is 4 or 8;
            int chromaRow = 0;
            for (int y = 0; y < height; y += 2, chromaRow++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ConvertRow(y * stride, planes.Samples.AsSpan(y * width, width), firstCb, firstCr, vectorRgb);
                if (y + 1 < height)
                    ConvertRow((y + 1) * stride, planes.Samples.AsSpan((y + 1) * width, width),
                        secondCb, secondCr, vectorRgb);
                else
                {
                    firstCb.CopyTo(secondCb);
                    firstCr.CopyTo(secondCr);
                }
                DownsampleConvertedRow(firstCb, secondCb,
                    planes.Samples.AsSpan(planes.PlaneLength + chromaRow * chromaWidth, chromaWidth));
                DownsampleConvertedRow(firstCr, secondCr,
                    planes.Samples.AsSpan(planes.PlaneLength + planes.ChromaLength + chromaRow * chromaWidth, chromaWidth));
            }
            if (chromaRow < chromaHeight)
            {
                // Pad source samples before averaging. For an even source
                // height the last real average includes two different rows,
                // whereas every padded row uses the final source row twice.
                cancellationToken.ThrowIfCancellationRequested();
                int cbStart = planes.PlaneLength + chromaRow * chromaWidth;
                int crStart = cbStart + planes.ChromaLength;
                DownsampleConvertedRow(secondCb, secondCb, planes.Samples.AsSpan(cbStart, chromaWidth));
                DownsampleConvertedRow(secondCr, secondCr, planes.Samples.AsSpan(crStart, chromaWidth));
                for (int row = chromaRow + 1; row < chromaHeight; row++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    planes.Samples.AsSpan(cbStart, chromaWidth)
                        .CopyTo(planes.Samples.AsSpan(planes.PlaneLength + row * chromaWidth, chromaWidth));
                    planes.Samples.AsSpan(crStart, chromaWidth)
                        .CopyTo(planes.Samples.AsSpan(planes.PlaneLength + planes.ChromaLength + row * chromaWidth, chromaWidth));
                }
            }
            return new PixelSource(this, colorSpace, convertedPlanes: planes);
        }
        catch
        {
            planes.Dispose();
            throw;
        }
        finally
        {
            if (scratch is not null)
                ArrayPool<float>.Shared.Return(scratch);
        }
    }

    private static void DownsampleConvertedRow(ReadOnlySpan<float> first, ReadOnlySpan<float> second, Span<float> output)
    {
        int width = first.Length;
        int x = 0;
#if !NETSTANDARD2_1
        if (Vector256.IsHardwareAccelerated)
            for (; x * 2 <= width - 16; x += 8)
                ((Downsampling.PairwiseSums256(first.Slice(x * 2, 16))
                    + Downsampling.PairwiseSums256(second.Slice(x * 2, 16))) * Vector256.Create(0.25f)).CopyTo(output[x..]);
        else if (Vector128.IsHardwareAccelerated)
            for (; x * 2 <= width - 8; x += 4)
                ((Downsampling.PairwiseSums128(first.Slice(x * 2, 8))
                    + Downsampling.PairwiseSums128(second.Slice(x * 2, 8))) * Vector128.Create(0.25f)).CopyTo(output[x..]);
#endif
        for (; x < (width + 1) / 2; x++)
        {
            int left = x * 2, right = Math.Min(left + 1, width - 1);
            output[x] = ((first[left] + first[right]) + (second[left] + second[right])) * 0.25f;
        }
        float padding = ((first[^1] + first[^1]) + (second[^1] + second[^1])) * 0.25f;
        output[x..].Fill(padding);
    }

    private void ConvertRow(int rowStart, Span<float> luma, Span<float> cb, Span<float> cr, bool vectorRgb)
    {
        int converted = vectorRgb ? ConvertByteRow(rowStart, luma, cb, cr) : 0;
        for (int x = converted; x < luma.Length; x++)
        {
            ReadConvertedComponents(rowStart + x * ComponentsPerPixel, out float first, out float second, out float third);
            luma[x] = first;
            if (!cb.IsEmpty)
            {
                cb[x] = second;
                cr[x] = third;
            }
        }
    }

#if !NETSTANDARD2_1
    private int ConvertByteRow(int rowStart, Span<float> lumaSamples, Span<float> cbSamples, Span<float> crSamples)
    {
        int lanes = Vector<float>.Count;
        int step = ComponentsPerPixel;
        Vector128<byte> redIndices = ComponentIndices(bgr ? 2 : 0, step);
        Vector128<byte> greenIndices = ComponentIndices(1, step);
        Vector128<byte> blueIndices = ComponentIndices(bgr ? 0 : 2, step);
        int upperBackstep = step == 3 ? 4 : 0;
        Vector128<byte> upperRedIndices = ComponentIndices((bgr ? 2 : 0) + upperBackstep, step);
        Vector128<byte> upperGreenIndices = ComponentIndices(1 + upperBackstep, step);
        Vector128<byte> upperBlueIndices = ComponentIndices((bgr ? 0 : 2) + upperBackstep, step);
        int width = lumaSamples.Length;
        int x = 0;
        // Overlap the two RGB loads so an eight-pixel group consumes exactly
        // 24 bytes. Bound every load by the active row, including the final
        // unpadded row; leave remaining pixels to the scalar path.
        int bytesNeeded = lanes == 8 ? 8 * step : 16;
        for (; x <= width - lanes && x * step <= width * step - bytesNeeded; x += lanes)
        {
            Vector128<byte> lower = Vector128.Create(bytes.Slice(rowStart + x * step, 16));
            // Each mask selects one byte and zeros the other three bytes in
            // each lane. Values are 0..255, so signed conversion is exact too.
            Vector128<int> r0 = Vector128.Shuffle(lower, redIndices).AsInt32();
            Vector128<int> g0 = Vector128.Shuffle(lower, greenIndices).AsInt32();
            Vector128<int> b0 = Vector128.Shuffle(lower, blueIndices).AsInt32();
            Vector<float> red, green, blue;
            if (lanes == 8)
            {
                Vector128<byte> upper = Vector128.Create(bytes.Slice(rowStart + (x + 4) * step - upperBackstep, 16));
                // Assemble integer lanes before conversion, so each channel
                // needs one full-width conversion rather than two half-width ones.
                red = Vector256.ConvertToSingle(Vector256.Create(r0, Vector128.Shuffle(upper, upperRedIndices).AsInt32())).AsVector();
                green = Vector256.ConvertToSingle(Vector256.Create(g0, Vector128.Shuffle(upper, upperGreenIndices).AsInt32())).AsVector();
                blue = Vector256.ConvertToSingle(Vector256.Create(b0, Vector128.Shuffle(upper, upperBlueIndices).AsInt32())).AsVector();
            }
            else
            {
                red = Vector128.ConvertToSingle(r0).AsVector();
                green = Vector128.ConvertToSingle(g0).AsVector();
                blue = Vector128.ConvertToSingle(b0).AsVector();
            }
            ColorConversion.RgbToYCbCr(red, green, blue,
                out Vector<float> luma, out Vector<float> cb, out Vector<float> cr);
            luma.CopyTo(lumaSamples.Slice(x, lanes));
            if (!cbSamples.IsEmpty)
            {
                cb.CopyTo(cbSamples.Slice(x, lanes));
                cr.CopyTo(crSamples.Slice(x, lanes));
            }
        }
        return x;
    }

    private static Vector128<byte> ComponentIndices(int component, int step)
        => Vector128.Create(0xffffff00U | (uint)component, 0xffffff00U | (uint)(component + step),
            0xffffff00U | (uint)(component + 2 * step), 0xffffff00U | (uint)(component + 3 * step)).AsByte();

#else
    private int ConvertByteRow(int rowStart, Span<float> lumaSamples, Span<float> cbSamples, Span<float> crSamples) => 0;

#endif
    private sealed class ConvertedPlanes(int width, int height, int sourceStride, int sourceLength, int components,
        bool is420 = false) : IDisposable
    {
        internal readonly int Width = width;
        internal readonly int SourceStride = sourceStride;
        internal readonly int SourceLength = sourceLength;
        internal readonly int PlaneLength = checked(width * height);
        internal readonly bool Is420 = is420;
        internal readonly int ChromaWidth = is420 ? checked((width + 15) / 16 * 8) : width;
        internal readonly int ChromaHeight = is420 ? checked((height + 15) / 16 * 8) : height;
        internal readonly int ChromaLength = is420 ? checked((width + 15) / 16 * 8 * ((height + 15) / 16 * 8)) : checked(width * height);
        internal float[] Samples { get; private set; } = ArrayPool<float>.Shared.Rent(is420
            ? checked(width * height + 2 * ((width + 15) / 16 * 8) * ((height + 15) / 16 * 8)) : checked(width * height * components));

        public void Dispose()
        {
            float[] samples = Samples;
            Samples = [];
            if (samples.Length != 0)
                ArrayPool<float>.Shared.Return(samples);
        }
    }

    /// <summary>Workers own source copies or borrow immutable planes for the encode lease's lifetime.</summary>
    internal sealed class Snapshot
    {
        private readonly byte[] bytes;
        private readonly ushort[] words;
        private readonly float[] floats;
        private readonly SourceType sourceType;
        private readonly bool bgr;
        private readonly bool ycbcr;
        private readonly bool xyb;
        private readonly JpegColorSpace colorSpace;
        private readonly int componentsPerPixel;
        private readonly IccColorTransform? colorTransform;
        private readonly ConvertedPlanes? convertedPlanes;

        internal Snapshot(PixelSource source, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            convertedPlanes = source.convertedPlanes;
            // Compact chroma planes do not retain arbitrary full-resolution
            // chroma samples. Preserve raw source bytes for fallback readers.
            bytes = (convertedPlanes is null || convertedPlanes.Is420) && source.sourceType == SourceType.Byte
                ? Copy(source.bytes, length, cancellationToken) : [];
            words = convertedPlanes is null && source.sourceType == SourceType.UInt16 ? Copy(source.words, length, cancellationToken) : [];
            floats = convertedPlanes is null && source.sourceType == SourceType.Float ? Copy(source.floats, length, cancellationToken) : [];
            sourceType = source.sourceType;
            bgr = source.bgr;
            ycbcr = source.ycbcr;
            xyb = source.xyb;
            colorSpace = source.colorSpace;
            componentsPerPixel = source.ComponentsPerPixel;
            colorTransform = source.colorTransform;
        }

        internal PixelSource GetPixels()
        {
            // Reconstruct with the original precision and layout. In particular,
            // normalizing ushort input into floats would change rounding and XYB.
            PixelSource pixels = sourceType switch
            {
                SourceType.Byte => new PixelSource(bytes, xyb ? PixelLayout.Xyb24
                    : ycbcr ? PixelLayout.YCbCr24 : componentsPerPixel == 1 ? PixelLayout.Gray8
                    : componentsPerPixel == 4 ? (bgr ? PixelLayout.Bgra32 : PixelLayout.Rgba32)
                    : bgr ? PixelLayout.Bgr24 : PixelLayout.Rgb24),
                SourceType.UInt16 => new PixelSource(words, GetLayout()),
                _ => new PixelSource(floats, GetLayout())
            };
            pixels = pixels.WithColorSpace(colorSpace);
            if (colorTransform is not null)
                pixels = pixels.WithColorTransform(colorTransform);
            return convertedPlanes is null ? pixels : new PixelSource(pixels, colorSpace, convertedPlanes: convertedPlanes);
        }

        private PixelComponentLayout GetLayout() => xyb ? PixelComponentLayout.Xyb
            : ycbcr ? PixelComponentLayout.YCbCr : componentsPerPixel == 1 ? PixelComponentLayout.Gray
            : componentsPerPixel == 4 ? (bgr ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgba)
            : bgr ? PixelComponentLayout.Bgr : PixelComponentLayout.Rgb;

        private static T[] Copy<T>(ReadOnlySpan<T> source, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var copy = new T[length];
            for (int offset = 0; offset < length; offset += Math.Min(65536, length - offset))
            {
                cancellationToken.ThrowIfCancellationRequested();
                int count = Math.Min(65536, length - offset);
                source.Slice(offset, count).CopyTo(copy.AsSpan(offset, count));
            }
            return copy;
        }
    }

    internal PixelSource WithColorSpace(JpegColorSpace outputColorSpace)
    {
        if (xyb && outputColorSpace is not (JpegColorSpace.Automatic or JpegColorSpace.Xyb))
            throw new ArgumentException("Already scaled XYB input cannot be converted to another output color space.",
                nameof(JpegliEncoderOptions.ColorSpace));
        if (outputColorSpace == JpegColorSpace.Xyb && ycbcr)
            throw new ArgumentException("Direct YCbCr input cannot be converted to XYB output.",
                nameof(JpegliEncoderOptions.ColorSpace));
        if (outputColorSpace == JpegColorSpace.Rgb && ycbcr)
            throw new ArgumentException("Direct YCbCr input cannot be converted to RGB output.",
                nameof(JpegliEncoderOptions.ColorSpace));
        return new PixelSource(this, outputColorSpace, discardConvertedPlanes: true);
    }
    internal int Length => convertedPlanes?.SourceLength ?? (sourceType switch
    {
        SourceType.Byte => bytes.Length,
        SourceType.UInt16 => words.Length,
        _ => floats.Length
    });

    internal bool ValidateSamples(int width, int height, int stride, CancellationToken cancellationToken,
        bool allowOutOfRangeFloatSamples = false)
    {
        if (sourceType != SourceType.Float)
            return false;

        // Check only consumed components. Padding, trailing storage and alpha
        // are intentionally ignored, just as in the byte input path.
        int activeComponents = IsGrayscaleInput || (ycbcr && IsGrayscale) ? 1 : 3;
        bool hasOutOfRangeSamples = false;
        for (int row = 0; row < height; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rowStart = row * stride;
            for (int x = 0; x < width; x++)
            {
                int offset = rowStart + x * ComponentsPerPixel;
                for (int component = 0; component < activeComponents; component++)
                {
                    float value = floats[offset + component];
                    if (!float.IsFinite(value) || !float.IsFinite(value * 255f))
                        throw new ArgumentException("Floating-point color and grayscale samples and their scaling to JPEG units must be finite.", "pixels");
                    if (value is < 0f or > 1f)
                    {
                        if (!allowOutOfRangeFloatSamples)
                            throw new ArgumentException("Floating-point color and grayscale samples must be between 0 and 1. AllowOutOfRangeFloatSamples permits wider values only without SourceIccProfile conversion.", "pixels");
                        hasOutOfRangeSamples = true;
                    }
                }
                // Validate the nonlinear transform before any output, including
                // when a finite source sample would overflow sRGB linearization.
                if (IsXyb && !xyb)
                    ReadSample(offset, 0);
            }
        }
        return hasOutOfRangeSamples;
    }

    internal float ReadSample(int offset, int component)
    {
        if (convertedPlanes is not null && (!convertedPlanes.Is420 || component == 0))
        {
            int row = offset / convertedPlanes.SourceStride;
            int column = (offset - row * convertedPlanes.SourceStride) / ComponentsPerPixel;
            return convertedPlanes.Samples[component * convertedPlanes.PlaneLength + row * convertedPlanes.Width + column];
        }
        if (colorTransform is not null)
        {
            float red = ReadNormalizedComponent(offset + (IsGrayscaleInput ? 0 : bgr ? 2 : 0));
            float green = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + 1);
            float blue = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + (bgr ? 0 : 2));
            colorTransform.Convert(red, green, blue, out red, out green, out blue);
            if (IsXyb)
            {
                XybColorConversion.SrgbToScaledXyb(red, green, blue, out float scaledX, out float scaledY, out float scaledB);
                return component switch { 0 => scaledX, 1 => scaledY, _ => scaledB };
            }
            if (IsGrayscale)
                return red * 255f;
            if (IsRgb)
                return (component switch { 0 => red, 1 => green, _ => blue }) * 255f;
            ColorConversion.RgbToYCbCr(red * 255f, green * 255f, blue * 255f,
                out float luma, out float managedCb, out float managedCr);
            return component switch { 0 => luma, 1 => managedCb, _ => managedCr };
        }
        if (IsXyb && !xyb)
        {
            float red = ReadNormalizedComponent(offset + (IsGrayscaleInput ? 0 : bgr ? 2 : 0));
            float green = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + 1);
            float blue = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + (bgr ? 0 : 2));
            XybColorConversion.SrgbToScaledXyb(red, green, blue, out float scaledX, out float scaledY, out float scaledB);
            return component switch { 0 => scaledX, 1 => scaledY, _ => scaledB };
        }
        if (IsGrayscaleInput)
            return ColorSpace == JpegColorSpace.YCbCr && component > 0 ? 128f : ReadComponent(offset);
        // ChooseColorTransform in the pinned upstream uses NullTransform when
        // both input and output are JCS_YCbCr. Read the supplied component after
        // the same input scaling; smoothing and subsampling still follow below.
        if (ycbcr)
            return ReadComponent(offset + component);

        // RGB JPEGs preserve the source values, normalizing BGR order and ignoring alpha.
        if (IsRgb)
            return ReadComponent(offset + (bgr ? 2 - component : component));

        float r = ReadComponent(offset + (bgr ? 2 : 0));
        float g = ReadComponent(offset + 1);
        float b = ReadComponent(offset + (bgr ? 0 : 2));
        ColorConversion.RgbToYCbCr(r, g, b, out float y, out float cb, out float cr);
        return component switch { 0 => y, 1 => cb, _ => cr };
    }

    // The coordinate path avoids dividing an interleaved source offset for cached samples.
    internal float ReadSampleAt(int x, int y, int stride, int component)
        => convertedPlanes is null || (convertedPlanes.Is420 && component != 0)
            ? ReadSample(y * stride + x * ComponentsPerPixel, component)
            : convertedPlanes.Samples[component * convertedPlanes.PlaneLength + y * convertedPlanes.Width + x];

    private void ReadConvertedComponents(int offset, out float first, out float second, out float third)
    {
        if (colorTransform is not null || (IsXyb && !xyb))
        {
            float red = ReadNormalizedComponent(offset + (IsGrayscaleInput ? 0 : bgr ? 2 : 0));
            float green = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + 1);
            float blue = IsGrayscaleInput ? red : ReadNormalizedComponent(offset + (bgr ? 0 : 2));
            if (colorTransform is not null)
                colorTransform.Convert(red, green, blue, out red, out green, out blue);
            if (IsXyb)
            {
                XybColorConversion.SrgbToScaledXyb(red, green, blue, out first, out second, out third);
                return;
            }
            first = red * 255f;
            second = green * 255f;
            third = blue * 255f;
            if (!IsGrayscale && !IsRgb)
                ColorConversion.RgbToYCbCr(first, second, third, out first, out second, out third);
            return;
        }
        if (IsGrayscaleInput)
        {
            first = ReadComponent(offset);
            second = third = ColorSpace == JpegColorSpace.YCbCr ? 128f : first;
            return;
        }
        if (ycbcr)
        {
            first = ReadComponent(offset);
            second = ReadComponent(offset + 1);
            third = ReadComponent(offset + 2);
            return;
        }
        float r = ReadComponent(offset + (bgr ? 2 : 0));
        float g = ReadComponent(offset + 1);
        float b = ReadComponent(offset + (bgr ? 0 : 2));
        if (IsRgb)
        {
            first = r;
            second = g;
            third = b;
        }
        else
            ColorConversion.RgbToYCbCr(r, g, b, out first, out second, out third);
    }

    // Read only a bounded source row, including replicated edge pixels.
    // Gathering preserves ushort's double normalization, float scaling,
    // BGR component order and the omission of alpha/padding components.
    internal void ReadRowScalar(int width, int height, int stride, int component,
        int x, int y, Span<float> samples)
    {
        if (convertedPlanes is not null && (!convertedPlanes.Is420 || component == 0))
        {
            ReadConvertedRow(width, height, component, x, y, samples);
            return;
        }
        int rowStart = Math.Clamp(y, 0, height - 1) * stride;
        for (int column = 0; column < samples.Length; column++)
        {
            int offset = rowStart + Math.Clamp(x + column, 0, width - 1) * ComponentsPerPixel;
            samples[column] = ReadSample(offset, component);
        }
    }

    internal void ReadRowVector(int width, int height, int stride, int component,
        int x, int y, Span<float> samples)
    {
        if (convertedPlanes is not null && (!convertedPlanes.Is420 || component == 0))
        {
            ReadConvertedRow(width, height, component, x, y, samples);
            return;
        }
        int lanes = Vector<float>.Count;
        // Nonlinear XYB and null transforms retain their existing scalar
        // semantics. Bound scratch space even if a future runtime has wider vectors.
        if (colorTransform is not null || IsXyb || IsGrayscaleInput || ycbcr || IsRgb || lanes > 8 || samples.Length < lanes)
        {
            ReadRowScalar(width, height, stride, component, x, y, samples);
            return;
        }

        Span<float> gathered = stackalloc float[24];
        Span<float> red = gathered[..lanes];
        Span<float> green = gathered.Slice(lanes, lanes);
        Span<float> blue = gathered.Slice(2 * lanes, lanes);
        int rowStart = Math.Clamp(y, 0, height - 1) * stride;
        int column = 0;
        for (; column <= samples.Length - lanes; column += lanes)
        {
            for (int lane = 0; lane < lanes; lane++)
            {
                int offset = rowStart + Math.Clamp(x + column + lane, 0, width - 1) * ComponentsPerPixel;
                red[lane] = ReadComponent(offset + (bgr ? 2 : 0));
                green[lane] = ReadComponent(offset + 1);
                blue[lane] = ReadComponent(offset + (bgr ? 0 : 2));
            }
            ColorConversion.RgbToYCbCr(new Vector<float>(red), new Vector<float>(green), new Vector<float>(blue),
                out Vector<float> luma, out Vector<float> cb, out Vector<float> cr);
            Vector<float> converted = component switch { 0 => luma, 1 => cb, _ => cr };
            converted.CopyTo(samples.Slice(column, lanes));
        }

        // A row can end between vector boundaries, without reading source padding.
        if (column != samples.Length)
            ReadRowScalar(width, height, stride, component, x + column, y, samples[column..]);
    }

    private void ReadConvertedRow(int width, int height, int component, int x, int y, Span<float> samples)
    {
        ReadOnlySpan<float> row = convertedPlanes!.Samples.AsSpan(
            component * convertedPlanes.PlaneLength + Math.Clamp(y, 0, height - 1) * width, width);
        if (x < 0)
        {
            int prefix = (int)Math.Min(samples.Length, -(long)x);
            samples[..prefix].Fill(row[0]);
            samples = samples[prefix..];
            x = 0;
        }
        if (x < width)
        {
            int count = Math.Min(samples.Length, width - x);
            row.Slice(x, count).CopyTo(samples);
            samples = samples[count..];
        }
        samples.Fill(row[^1]);
    }

    private float ReadComponent(int offset) => sourceType switch
    {
        SourceType.Byte => bytes[offset],
        // Scalar ReadUint16Row and ReadFloatRow in lib/jpegli/input.cc,
        // revision 031a0077f5799a6041004267fc12b956c1f52a20.
        // Upstream's double reciprocal matters before conversion to float.
        SourceType.UInt16 => (float)(words[offset] * (1.0 / 257.0)),
        _ => floats[offset] * 255f
    };

    // ToFloatRow in upstream's extras encoder normalizes directly to 0..1
    // before its color transform. Avoid a rounded JPEG-unit intermediate.
    private float ReadNormalizedComponent(int offset) => sourceType switch
    {
        SourceType.Byte => (float)(bytes[offset] * (1.0 / 255.0)),
        SourceType.UInt16 => (float)(words[offset] * (1.0 / 65535.0)),
        _ => floats[offset]
    };

    private static int GetComponentCount(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Rgb or PixelComponentLayout.Bgr or PixelComponentLayout.YCbCr or PixelComponentLayout.Xyb => 3,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        PixelComponentLayout.Gray => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(layout))
    };

    private enum SourceType { Byte, UInt16, Float }
}
