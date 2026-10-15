// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Numerics;

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
        // streaming reader remains available when full planes exceed 64 MiB.
        const int maximumSamples = 64 * 1024 * 1024 / sizeof(float);
        int components = IsGrayscale ? 1 : 3;
        if ((long)width * height * components > maximumSamples)
            return this;
        var planes = new ConvertedPlanes(width, height, stride, Length, components);
        int planeLength = planes.PlaneLength;
        for (int y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rowStart = y * stride;
            int destination = y * width;
            for (int x = 0; x < width; x++)
            {
                ReadConvertedComponents(rowStart + x * ComponentsPerPixel,
                    out float first, out float second, out float third);
                planes.Samples[destination + x] = first;
                if (components > 1)
                {
                    planes.Samples[planeLength + destination + x] = second;
                    planes.Samples[2 * planeLength + destination + x] = third;
                }
            }
        }
        return new PixelSource(this, colorSpace, convertedPlanes: planes);
    }

    private sealed class ConvertedPlanes(int width, int height, int sourceStride, int sourceLength, int components)
    {
        internal readonly int Width = width;
        internal readonly int SourceStride = sourceStride;
        internal readonly int SourceLength = sourceLength;
        internal readonly int PlaneLength = checked(width * height);
        internal readonly float[] Samples = new float[checked(width * height * components)];
    }

    /// <summary>Owned managed storage allows workers to recreate this span-based reader safely.</summary>
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
            bytes = convertedPlanes is null && source.sourceType == SourceType.Byte ? Copy(source.bytes, length, cancellationToken) : [];
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
        if (convertedPlanes is not null)
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
        => convertedPlanes is null ? ReadSample(y * stride + x * ComponentsPerPixel, component)
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
        if (convertedPlanes is not null)
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
        if (convertedPlanes is not null)
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
