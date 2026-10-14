// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace DotJpegli.Internal;

// ICC v4 serialization, ported from lib/cms/jpegli_cms_internal.h,
// google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
internal static class IccProfileWriter
{
    internal static string RenderingIntentName(IccRenderingIntent renderingIntent) => renderingIntent switch
    {
        IccRenderingIntent.Perceptual => "Per",
        IccRenderingIntent.RelativeColorimetric => "Rel",
        IccRenderingIntent.Saturation => "Sat",
        IccRenderingIntent.AbsoluteColorimetric => "Abs",
        _ => throw new ArgumentOutOfRangeException(nameof(renderingIntent))
    };

    internal static void Header(Span<byte> bytes, ReadOnlySpan<byte> deviceClass, ReadOnlySpan<byte> colorSpace,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
    {
        _ = RenderingIntentName(renderingIntent);
        UInt32(bytes, 0, (uint)bytes.Length);
        Signature(bytes, 4, "jxl "u8);
        UInt32(bytes, 8, 0x04400000);
        Signature(bytes, 12, deviceClass);
        Signature(bytes, 16, colorSpace);
        Signature(bytes, 20, "XYZ "u8);
        // Keep upstream's fixed timestamp and platform for reproducible output.
        UInt16(bytes, 24, 2019);
        UInt16(bytes, 26, 12);
        UInt16(bytes, 28, 1);
        Signature(bytes, 36, "acsp"u8);
        Signature(bytes, 40, "APPL"u8);
        UInt32(bytes, 64, (uint)renderingIntent);
        UInt32(bytes, 68, 0x0000f6d6);
        UInt32(bytes, 72, 0x00010000);
        UInt32(bytes, 76, 0x0000d32d);
        Signature(bytes, 80, "jxl "u8);
    }

    internal static void ProfileId(Span<byte> bytes)
    {
        // ICC profile identification excludes flags, rendering intent and the ID.
        uint flags = BinaryPrimitives.ReadUInt32BigEndian(bytes[44..48]);
        uint intent = BinaryPrimitives.ReadUInt32BigEndian(bytes[64..68]);
        bytes[44..48].Clear();
        bytes[64..68].Clear();
        bytes[84..100].Clear();
        Span<byte> digest = stackalloc byte[16];
        MD5.HashData(bytes, digest);
        UInt32(bytes, 44, flags);
        UInt32(bytes, 64, intent);
        digest.CopyTo(bytes[84..100]);
    }

    internal static void WriteChromaticAdaptation(Span<byte> tag,
        float whitePointX = 0.3127f, float whitePointY = 0.3290f)
    {
        Signature(tag, 0, "sf32"u8);
        // AdaptToXYZD50 uses float matrices and double intermediate dot products.
        ReadOnlySpan<float> bradford = [0.8951f, 0.2664f, -0.1614f,
            -0.7502f, 1.7135f, 0.0367f, 0.0389f, -0.0685f, 1.0296f];
        ReadOnlySpan<float> inverse = [0.9869929f, -0.1470543f, 0.1599627f,
            0.4323053f, 0.5183603f, 0.0492912f, -0.0085287f, 0.0400428f, 0.9684867f];
        ReadOnlySpan<float> white = [whitePointX / whitePointY, 1f,
            (1f - whitePointX - whitePointY) / whitePointY];
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

    internal static void WriteChromaticAdaptation(Span<byte> tag, ReadOnlySpan<float> matrix)
    {
        Signature(tag, 0, "sf32"u8);
        for (int index = 0; index < 9; index++)
            Fixed(tag, 8 + index * 4, matrix[index]);
    }

    internal static void Mluc(Span<byte> tag, string text)
    {
        Signature(tag, 0, "mluc"u8);
        UInt32(tag, 8, 1);
        UInt32(tag, 12, 12);
        Signature(tag, 16, "enUS"u8);
        UInt32(tag, 20, (uint)(text.Length * 2));
        UInt32(tag, 24, 28);
        Encoding.BigEndianUnicode.GetBytes(text, tag[28..]);
    }

    internal static void Curve(Span<byte> tag, ReadOnlySpan<float> parameters, ushort type)
    {
        Signature(tag, 0, "para"u8);
        UInt16(tag, 8, type);
        for (int index = 0; index < parameters.Length; index++)
            Fixed(tag, 12 + index * 4, parameters[index]);
    }

    internal static void Tag(Span<byte> profile, int index, ReadOnlySpan<byte> signature, uint offset, uint size)
    {
        int position = 132 + index * 12;
        Signature(profile, position, signature);
        UInt32(profile, position + 4, offset);
        UInt32(profile, position + 8, size);
    }

    internal static void Fixed(Span<byte> bytes, int offset, float value) =>
        BinaryPrimitives.WriteInt32BigEndian(bytes.Slice(offset, 4),
            (int)MathF.Round(value * 65536f, MidpointRounding.AwayFromZero));

    internal static void UInt32(Span<byte> bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(bytes.Slice(offset, 4), value);

    internal static void UInt16(Span<byte> bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(offset, 2), value);

    internal static void Signature(Span<byte> bytes, int offset, ReadOnlySpan<byte> value) =>
        value.CopyTo(bytes.Slice(offset, 4));
}
