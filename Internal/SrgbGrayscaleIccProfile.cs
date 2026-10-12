// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class SrgbGrayscaleIccProfile
{
    // Fixed D65/sRGB/perceptual grayscale branch of lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create()
    {
        byte[] profile = new byte[328];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "GRAY"u8);
        UInt32(bytes, 128, 4);
        Tag(bytes, 0, "desc"u8, 180, 60);
        Tag(bytes, 1, "cprt"u8, 240, 36);
        Tag(bytes, 2, "wtpt"u8, 276, 20);
        Tag(bytes, 3, "kTRC"u8, 296, 32);
        Mluc(bytes[180..240], "Gra_D65_Per_SRG");
        Mluc(bytes[240..276], "CC0");

        // Upstream's grayscale branch keeps the source D65 media white, omits
        // chad/CICP, and uses a D50 PCS illuminant in the common ICC header.
        // CIEXYZFromWhiteCIExy rounds the reciprocal to float before multiplying.
        float factor = (float)(1.0 / 0.3290);
        Signature(bytes, 276, "XYZ "u8);
        Fixed(bytes, 284, (float)(0.3127 * factor));
        Fixed(bytes, 288, 1f);
        Fixed(bytes, 292, (float)((1.0 - 0.3127 - 0.3290) * factor));
        Curve(bytes[296..], [2.4f, (float)(1.0 / 1.055), (float)(0.055 / 1.055),
            (float)(1.0 / 12.92), 0.04045f], 3);
        ProfileId(bytes);
        return profile;
    }
}
