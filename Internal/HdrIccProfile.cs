// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class HdrIccProfile
{
    // Fixed Rec.2020/D65/perceptual PQ and HLG branches with upstream's default
    // JPEGLI_ENABLE_3D_ICC_TONEMAPPING=1, lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create(bool pq)
    {
        byte[] profile = new byte[4344];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "RGB "u8);
        Signature(bytes, 20, "Lab "u8);
        UInt32(bytes, 128, 10);
        Tag(bytes, 0, "desc"u8, 252, 68);
        Tag(bytes, 1, "cprt"u8, 320, 36);
        Tag(bytes, 2, "wtpt"u8, 356, 20);
        Tag(bytes, 3, "chad"u8, 376, 44);
        Tag(bytes, 4, "cicp"u8, 420, 12);
        Tag(bytes, 5, "rXYZ"u8, 432, 20);
        Tag(bytes, 6, "gXYZ"u8, 452, 20);
        Tag(bytes, 7, "bXYZ"u8, 472, 20);
        Tag(bytes, 8, "A2B0"u8, 492, 3772);
        Tag(bytes, 9, "B2A0"u8, 4264, 80);
        Mluc(bytes[252..320], pq ? "RGB_D65_202_Per_PeQ" : "RGB_D65_202_Per_HLG");
        Mluc(bytes[320..356], "CC0");
        Xyz(bytes[356..376], [0.964203f, 1f, 0.824905f]);
        WriteChromaticAdaptation(bytes[376..420]);
        Signature(bytes, 420, "cicp"u8);
        bytes[428] = 9; // Rec.2020 primaries.
        bytes[429] = pq ? (byte)16 : (byte)18;
        bytes[431] = 1; // RGB matrix (zero), full range.
        ReadOnlySpan<float> colorants = RgbIccProfile.Rec2020Colorants;
        for (int channel = 0; channel < 3; channel++)
            Xyz(bytes.Slice(432 + channel * 20, 20), colorants.Slice(channel * 3, 3));
        WriteForwardTransform(bytes[492..4264], pq);
        WriteCompatibilityTransform(bytes[4264..]);
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
