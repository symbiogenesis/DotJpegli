// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

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
    private bool IsGrayscaleInput => ComponentsPerPixel == 1;
    internal JpegColorSpace ColorSpace => colorSpace == JpegColorSpace.Automatic
        ? (xyb ? JpegColorSpace.Xyb : IsGrayscaleInput ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr) : colorSpace;
    internal bool IsGrayscale => ColorSpace == JpegColorSpace.Grayscale;
    // XYB is carried in an RGB JPEG: identifiers, entropy selectors and AQ rules
    // are shared, while sampling defaults and quantization matrices differ.
    internal bool IsRgb => ColorSpace is JpegColorSpace.Rgb or JpegColorSpace.Xyb;
    internal bool IsXyb => ColorSpace == JpegColorSpace.Xyb;
    internal int AnalysisComponent => IsRgb ? 1 : 0;

    private PixelSource(PixelSource source, JpegColorSpace colorSpace)
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
