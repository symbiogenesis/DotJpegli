// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Numerics;

namespace DotJpegli.Internal;

/// <summary>Reads interleaved source samples in fractional JPEG units without a converted image buffer.</summary>
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
    private bool IsGrayscaleInput => ComponentsPerPixel == 1;
    internal JpegColorSpace ColorSpace => colorSpace == JpegColorSpace.Automatic
        ? (xyb ? JpegColorSpace.Xyb : IsGrayscaleInput ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr) : colorSpace;
    internal bool IsGrayscale => ColorSpace == JpegColorSpace.Grayscale;
    // XYB is carried in an RGB JPEG: identifiers, entropy selectors and AQ rules
    // are shared, while sampling defaults and quantization matrices differ.
    internal bool IsRgb => ColorSpace is JpegColorSpace.Rgb or JpegColorSpace.Xyb;
    internal bool IsXyb => ColorSpace == JpegColorSpace.Xyb;
    internal int AnalysisComponent => IsRgb ? 1 : 0;

    private PixelSource(PixelSource source, JpegColorSpace colorSpace, ParallelDctRows? parallelRows = null)
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
    }

    internal PixelSource WithParallelRows(ParallelDctRows rows) => new(this, colorSpace, rows);

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

        internal Snapshot(PixelSource source, int length, CancellationToken cancellationToken)
        {
            bytes = source.sourceType == SourceType.Byte ? Copy(source.bytes, length, cancellationToken) : [];
            words = source.sourceType == SourceType.UInt16 ? Copy(source.words, length, cancellationToken) : [];
            floats = source.sourceType == SourceType.Float ? Copy(source.floats, length, cancellationToken) : [];
            sourceType = source.sourceType;
            bgr = source.bgr;
            ycbcr = source.ycbcr;
            xyb = source.xyb;
            colorSpace = source.colorSpace;
            componentsPerPixel = source.ComponentsPerPixel;
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
            return pixels.WithColorSpace(colorSpace);
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
        return new PixelSource(this, outputColorSpace);
    }
    internal int Length => sourceType switch
    {
        SourceType.Byte => bytes.Length,
        SourceType.UInt16 => words.Length,
        _ => floats.Length
    };

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
                            throw new ArgumentException("Floating-point color and grayscale samples must be between 0 and 1 unless AllowOutOfRangeFloatSamples is enabled.", "pixels");
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

    // Read only a bounded source row, including replicated edge pixels.
    // Gathering preserves ushort's double normalization, float scaling,
    // BGR component order and the omission of alpha/padding components.
    internal void ReadRowScalar(int width, int height, int stride, int component,
        int x, int y, Span<float> samples)
    {
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
        int lanes = Vector<float>.Count;
        // Nonlinear XYB and null transforms retain their existing scalar
        // semantics. Bound scratch space even if a future runtime has wider vectors.
        if (IsXyb || IsGrayscaleInput || ycbcr || IsRgb || lanes > 8 || samples.Length < lanes)
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
