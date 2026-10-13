// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class HdrIccProfile
{
    // Fixed Rec.2020/D65 PQ and HLG branches with upstream's default
    // JPEGLI_ENABLE_3D_ICC_TONEMAPPING=1, lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create(bool pq,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
    {
        string intentName = RenderingIntentName(renderingIntent);
        string description = renderingIntent == IccRenderingIntent.RelativeColorimetric
            ? (pq ? "Rec2100PQ" : "Rec2100HLG")
            : "RGB_D65_202_" + intentName + (pq ? "_PeQ" : "_HLG");
        int descriptionSize = (28 + description.Length * 2 + 3) & ~3;
        int copyrightOffset = 252 + descriptionSize;
        int whiteOffset = copyrightOffset + 36;
        int adaptationOffset = whiteOffset + 20;
        int cicpOffset = adaptationOffset + 44;
        int colorantsOffset = cicpOffset + 12;
        int forwardOffset = colorantsOffset + 60;
        int compatibilityOffset = forwardOffset + 3772;
        byte[] profile = new byte[compatibilityOffset + 80];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "RGB "u8, renderingIntent);
        Signature(bytes, 20, "Lab "u8);
        UInt32(bytes, 128, 10);
        Tag(bytes, 0, "desc"u8, 252, (uint)descriptionSize);
        Tag(bytes, 1, "cprt"u8, (uint)copyrightOffset, 36);
        Tag(bytes, 2, "wtpt"u8, (uint)whiteOffset, 20);
        Tag(bytes, 3, "chad"u8, (uint)adaptationOffset, 44);
        Tag(bytes, 4, "cicp"u8, (uint)cicpOffset, 12);
        Tag(bytes, 5, "rXYZ"u8, (uint)colorantsOffset, 20);
        Tag(bytes, 6, "gXYZ"u8, (uint)(colorantsOffset + 20), 20);
        Tag(bytes, 7, "bXYZ"u8, (uint)(colorantsOffset + 40), 20);
        Tag(bytes, 8, "A2B0"u8, (uint)forwardOffset, 3772);
        Tag(bytes, 9, "B2A0"u8, (uint)compatibilityOffset, 80);
        Mluc(bytes[252..copyrightOffset], description);
        Mluc(bytes[copyrightOffset..whiteOffset], "CC0");
        Xyz(bytes[whiteOffset..adaptationOffset], [0.964203f, 1f, 0.824905f]);
        WriteChromaticAdaptation(bytes[adaptationOffset..cicpOffset]);
        Signature(bytes, cicpOffset, "cicp"u8);
        bytes[cicpOffset + 8] = 9; // Rec.2020 primaries.
        bytes[cicpOffset + 9] = pq ? (byte)16 : (byte)18;
        bytes[cicpOffset + 11] = 1; // RGB matrix (zero), full range.
        ReadOnlySpan<float> colorants = RgbIccProfile.Rec2020Colorants;
        for (int channel = 0; channel < 3; channel++)
            Xyz(bytes.Slice(colorantsOffset + channel * 20, 20), colorants.Slice(channel * 3, 3));
        WriteForwardTransform(bytes[forwardOffset..compatibilityOffset], pq);
        WriteCompatibilityTransform(bytes[compatibilityOffset..]);
        ProfileId(bytes);
        return profile;
    }

    private static void WriteForwardTransform(Span<byte> tag, bool pq)
    {
        Signature(tag, 0, "mft1"u8);
        tag[8] = tag[9] = 3;
        tag[10] = 9;
        for (int diagonal = 0; diagonal < 3; diagonal++)
            Fixed(tag, 12 + diagonal * 16, 1f);
        // mft1 has 256-entry eight-bit input and output curves for each channel.
        for (int channel = 0; channel < 3; channel++)
            for (int value = 0; value < 256; value++)
            {
                tag[48 + channel * 256 + value] = (byte)value;
                tag[3003 + channel * 256 + value] = (byte)value;
            }
        HdrToneMapping.WriteClut(tag.Slice(816, 2187), pq);
        // The last byte is zero alignment padding, included in upstream's tag size.
    }

    private static void WriteCompatibilityTransform(Span<byte> tag)
    {
        // Upstream adds this identity for reader compatibility. It cannot undo
        // the forward tone/gamut mapping and is not a source conversion profile.
        Signature(tag, 0, "mBA "u8);
        tag[8] = tag[9] = 3;
        UInt32(tag, 12, 32);
        for (int channel = 0; channel < 3; channel++)
            Curve(tag.Slice(32 + channel * 16, 16), [1f], 0);
    }

    private static void Xyz(Span<byte> tag, ReadOnlySpan<float> xyz)
    {
        Signature(tag, 0, "XYZ "u8);
        for (int channel = 0; channel < 3; channel++)
            Fixed(tag, 8 + channel * 4, xyz[channel]);
    }
}
