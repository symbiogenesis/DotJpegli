// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class SrgbIccProfile
{
    // Fixed D65/sRGB-primaries/perceptual branches of lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create(bool linear = false)
    {
        string description = linear ? "RGB_D65_SRG_Per_Lin" : "sRGB";
        int descriptionSize = (28 + description.Length * 2 + 3) & ~3;
        int copyrightOffset = 264 + descriptionSize;
        int whiteOffset = copyrightOffset + 36;
        int adaptationOffset = whiteOffset + 20;
        int cicpOffset = adaptationOffset + 44;
        int redOffset = cicpOffset + 12;
        int greenOffset = redOffset + 20;
        int blueOffset = greenOffset + 20;
        int curveOffset = blueOffset + 20;
        byte[] profile = new byte[curveOffset + 32];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "RGB "u8);
        UInt32(bytes, 128, 11);
        Tag(bytes, 0, "desc"u8, 264, (uint)descriptionSize);
        Tag(bytes, 1, "cprt"u8, (uint)copyrightOffset, 36);
        Tag(bytes, 2, "wtpt"u8, (uint)whiteOffset, 20);
        Tag(bytes, 3, "chad"u8, (uint)adaptationOffset, 44);
        Tag(bytes, 4, "cicp"u8, (uint)cicpOffset, 12);
        Tag(bytes, 5, "rXYZ"u8, (uint)redOffset, 20);
        Tag(bytes, 6, "gXYZ"u8, (uint)greenOffset, 20);
        Tag(bytes, 7, "bXYZ"u8, (uint)blueOffset, 20);
        // Upstream shares one decoding curve among all three channels.
        Tag(bytes, 8, "rTRC"u8, (uint)curveOffset, 32);
        Tag(bytes, 9, "gTRC"u8, (uint)curveOffset, 32);
        Tag(bytes, 10, "bTRC"u8, (uint)curveOffset, 32);
        Mluc(bytes[264..copyrightOffset], description);
        Mluc(bytes[copyrightOffset..whiteOffset], "CC0");
        Xyz(bytes[whiteOffset..adaptationOffset], [0.964203f, 1f, 0.824905f]);
        WriteChromaticAdaptation(bytes[adaptationOffset..cicpOffset]);
        Signature(bytes, cicpOffset, "cicp"u8);
        bytes[cicpOffset + 8] = 1;  // BT.709/sRGB primaries.
        bytes[cicpOffset + 9] = linear ? (byte)8 : (byte)13;
        bytes[cicpOffset + 11] = 1; // RGB matrix (zero), full range.

        // Precomputed columns of upstream's PrimariesToXYZD50 with its fixed
        // sRGB primaries from color_encoding_cms.h: R=(0.639998686,0.330010138),
        // G=(0.300003784,0.600003357), B=(0.150002046,0.059997204).
        // These are the float results before ICC s15Fixed16 serialization.
        Xyz(bytes[redOffset..greenOffset], [0.43602091074f, 0.222481667995f, 0.0139219872653f]);
        Xyz(bytes[greenOffset..blueOffset], [0.385103434324f, 0.71689671278f, 0.097085095942f]);
        Xyz(bytes[blueOffset..curveOffset], [0.143095657229f, 0.0606217607856f, 0.714202940464f]);
        Curve(bytes[curveOffset..], linear
            ? [1f, 1f, 0f, 1f, 0f]
            : [2.4f, (float)(1.0 / 1.055), (float)(0.055 / 1.055),
                (float)(1.0 / 12.92), 0.04045f], 3);
        ProfileId(bytes);
        return profile;
    }

    private static void Xyz(Span<byte> tag, ReadOnlySpan<float> xyz)
    {
        Signature(tag, 0, "XYZ "u8);
        for (int component = 0; component < 3; component++)
            Fixed(tag, 8 + component * 4, xyz[component]);
    }
}
