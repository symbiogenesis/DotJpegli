// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

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
        UInt32(bytes, 0, (uint)profile.Length);
        Signature(bytes, 4, "jxl "u8);
        UInt32(bytes, 8, 0x04400000);
        Signature(bytes, 12, "scnr"u8);
        Signature(bytes, 16, "RGB "u8);
        Signature(bytes, 20, "XYZ "u8);
        // Keep upstream's fixed timestamp and platform for reproducible output.
        UInt16(bytes, 24, 2019);
        UInt16(bytes, 26, 12);
        UInt16(bytes, 28, 1);
        Signature(bytes, 36, "acsp"u8);
        Signature(bytes, 40, "APPL"u8);
        UInt32(bytes, 68, 0x0000f6d6);
        UInt32(bytes, 72, 0x00010000);
        UInt32(bytes, 76, 0x0000d32d);
        Signature(bytes, 80, "jxl "u8);

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

        // ICC profile ID hashes the complete profile with flags, intent and
        // ID zeroed. They are already zero in this new perceptual profile.
        Span<byte> digest = stackalloc byte[16];
        MD5.HashData(bytes, digest);
        digest.CopyTo(bytes[84..100]);
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

    private static void WriteChromaticAdaptation(Span<byte> tag)
    {
        Signature(tag, 0, "sf32"u8);
        // AdaptToXYZD50 uses float matrices and double intermediate dot products.
        ReadOnlySpan<float> bradford = [0.8951f, 0.2664f, -0.1614f,
            -0.7502f, 1.7135f, 0.0367f, 0.0389f, -0.0685f, 1.0296f];
        ReadOnlySpan<float> inverse = [0.9869929f, -0.1470543f, 0.1599627f,
            0.4323053f, 0.5183603f, 0.0492912f, -0.0085287f, 0.0400428f, 0.9684867f];
        ReadOnlySpan<float> white = [0.3127f / 0.3290f, 1f, (1f - 0.3127f - 0.3290f) / 0.3290f];
        ReadOnlySpan<float> d50 = [0.96422f, 1f, 0.82521f];
        Span<float> adapted = stackalloc float[9];
        for (int row = 0; row < 3; row++)
        {
            double source = 0, destination = 0;
            for (int column = 0; column < 3; column++)
            {
                source += (double)bradford[row * 3 + column] * white[column];
                destination += (double)bradford[row * 3 + column] * d50[column];
            }
            float scale = (float)destination / (float)source;
            for (int column = 0; column < 3; column++)
                adapted[row * 3 + column] = (float)((double)scale * bradford[row * 3 + column]);
        }
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
            {
                double value = 0;
                for (int index = 0; index < 3; index++)
                    value += (double)inverse[row * 3 + index] * adapted[index * 3 + column];
                Fixed(tag, 8 + (row * 3 + column) * 4, (float)value);
            }
    }

    private static void Mluc(Span<byte> tag, string text)
    {
        Signature(tag, 0, "mluc"u8);
        UInt32(tag, 8, 1);
        UInt32(tag, 12, 12);
        Signature(tag, 16, "enUS"u8);
        UInt32(tag, 20, (uint)(text.Length * 2));
        UInt32(tag, 24, 28);
        Encoding.BigEndianUnicode.GetBytes(text, tag[28..]);
    }

    private static void Curve(Span<byte> tag, ReadOnlySpan<float> parameters, ushort type)
    {
        Signature(tag, 0, "para"u8);
        UInt16(tag, 8, type);
        for (int index = 0; index < parameters.Length; index++)
            Fixed(tag, 12 + index * 4, parameters[index]);
    }

    private static void Tag(Span<byte> profile, int index, ReadOnlySpan<byte> signature, uint offset, uint size)
    {
        int position = 132 + index * 12;
        Signature(profile, position, signature);
        UInt32(profile, position + 4, offset);
        UInt32(profile, position + 8, size);
    }

    private static void Fixed(Span<byte> bytes, int offset, float value) =>
        BinaryPrimitives.WriteInt32BigEndian(bytes.Slice(offset, 4),
            (int)MathF.Round(value * 65536f, MidpointRounding.AwayFromZero));

    private static void UInt32(Span<byte> bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(bytes.Slice(offset, 4), value);

    private static void UInt16(Span<byte> bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(offset, 2), value);

    private static void Signature(Span<byte> bytes, int offset, ReadOnlySpan<byte> value) =>
        value.CopyTo(bytes.Slice(offset, 4));
}
