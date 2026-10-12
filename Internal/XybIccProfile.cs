// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class XybIccProfile
{
    // Ported from lib/cms/jpegli_cms_internal.h and lib/cms/opsin_params.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20. Only the fixed
    // scaled XYB, D65, perceptual profile is supported. The A2B transform uses
    // a linear 2x2x2 CLUT, cubic curves and an opsin-to-D50-XYZ matrix.
    private const float Bias = 0.0037930732552754493f;
    private const float ScaleX = 22.995788804f;
    private const float ScaleY = 1.183000077f;
    private const float ScaleB = 1.502141333f;
    private const float OffsetX = 0.015386134f;
    private const float OffsetB = 0.27770459f;

    internal static byte[] Create()
    {
        byte[] profile = new byte[720];
        Span<byte> bytes = profile;
        Header(bytes, "scnr"u8, "RGB "u8);

        UInt32(bytes, 128, 6);
        Tag(bytes, 0, "desc"u8, 204, 44);
        Tag(bytes, 1, "cprt"u8, 248, 36);
        Tag(bytes, 2, "wtpt"u8, 284, 20);
        Tag(bytes, 3, "chad"u8, 304, 44);
        Tag(bytes, 4, "A2B0"u8, 348, 292);
        Tag(bytes, 5, "B2A0"u8, 640, 80);
        Mluc(bytes[204..248], "XYB_Per");
        Mluc(bytes[248..284], "CC0");
        Signature(bytes, 284, "XYZ "u8);
        Fixed(bytes, 292, 0.964203f);
        Fixed(bytes, 296, 1f);
        Fixed(bytes, 300, 0.824905f);
        WriteChromaticAdaptation(bytes[304..348]);
        WriteForwardTransform(bytes[348..640]);
        WriteCompatibilityTransform(bytes[640..]);

        ProfileId(bytes);
        return profile;
    }

    private static void WriteForwardTransform(Span<byte> tag)
    {
        Signature(tag, 0, "mAB "u8);
        tag[8] = tag[9] = 3;
        UInt32(tag, 12, 32);  // B curves, also reused as A curves.
        UInt32(tag, 16, 244); // Matrix.
        UInt32(tag, 20, 148); // M curves.
        UInt32(tag, 24, 80);  // CLUT.
        UInt32(tag, 28, 32);
        for (int component = 0; component < 3; component++)
            Curve(tag.Slice(32 + component * 16, 16), [1f], 0);

        tag[80] = tag[81] = tag[82] = 2;
        tag[96] = 2; // 16-bit CLUT entries.
        float scaleXY = ScaleX * ScaleY / (ScaleX + ScaleY);
        float scaleBY = ScaleY * ScaleB / (ScaleY + ScaleB);
        ReadOnlySpan<float> offsets = [OffsetX, -OffsetX + 1f / ScaleX, OffsetB];
        ReadOnlySpan<float> scales = [scaleXY, scaleXY, scaleBY];
        int position = 100;
        for (int ix = 0; ix < 2; ix++)
            for (int iy = 0; iy < 2; iy++)
                for (int ib = 0; ib < 2; ib++)
                {
                    float x = ix / ScaleX - OffsetX;
                    float y = iy / ScaleY;
                    float b = ib / ScaleB - OffsetB;
                    ReadOnlySpan<float> mixed = [y + x, y - x, b + y];
                    for (int component = 0; component < 3; component++, position += 2)
                    {
                        float value = (mixed[component] + offsets[component]) * scales[component];
                        UInt16(tag, position, (ushort)MathF.Round(65535f * value, MidpointRounding.AwayFromZero));
                    }
                }

        for (int component = 0; component < 3; component++)
        {
            float b = -offsets[component] - MathF.Cbrt(-Bias);
            Curve(tag.Slice(148 + component * 32, 32),
                [3f, 1f / scales[component], b, 0f, MathF.Max(0f, -b * scales[component])], 3);
        }

        ReadOnlySpan<double> matrix = [1.5170095, -1.1065225, 0.071623,
            -0.050022, 0.5683655, -0.018344, -1.387676, 1.1145555, 0.6857255];
        for (int index = 0; index < matrix.Length; index++)
            Fixed(tag, 244 + index * 4, (float)matrix[index]);
        for (int row = 0; row < 3; row++)
        {
            float intercept = 0;
            for (int column = 0; column < 3; column++)
                intercept = (float)(intercept + matrix[row * 3 + column] * -Bias);
            Fixed(tag, 280 + row * 4, intercept);
        }
    }

    private static void WriteCompatibilityTransform(Span<byte> tag)
    {
        // Pinned upstream supplies this identity solely for reader compatibility.
        // It is not the inverse of A2B0 and must not be used for RGB-to-XYB conversion.
        Signature(tag, 0, "mBA "u8);
        tag[8] = tag[9] = 3;
        UInt32(tag, 12, 32);
        for (int component = 0; component < 3; component++)
            Curve(tag.Slice(32 + component * 16, 16), [1f], 0);
    }
}
