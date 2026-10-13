// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class GrayscaleIccProfile
{
    internal enum TransferFunction
    {
        Srgb,
        Linear,
        Rec709,
        Dci,
        Pq,
        Hlg
    }

    // Fixed D65 and DCI/perceptual grayscale branches of lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create(TransferFunction transfer)
    {
        string description = transfer switch
        {
            TransferFunction.Srgb => "Gra_D65_Per_SRG",
            TransferFunction.Linear => "Gra_D65_Per_Lin",
            TransferFunction.Rec709 => "Gra_D65_Per_709",
            TransferFunction.Dci => "Gra_DCI_Per_DCI",
            TransferFunction.Pq => "Gra_D65_Per_PeQ",
            TransferFunction.Hlg => "Gra_D65_Per_HLG",
            _ => throw new ArgumentOutOfRangeException(nameof(transfer))
        };
        bool tableCurve = transfer is TransferFunction.Pq or TransferFunction.Hlg;
        int curveSize = tableCurve ? 12 + 64 * 2 : 32;
        byte[] profile = new byte[296 + curveSize];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "GRAY"u8);
        UInt32(bytes, 128, 4);
        Tag(bytes, 0, "desc"u8, 180, 60);
        Tag(bytes, 1, "cprt"u8, 240, 36);
        Tag(bytes, 2, "wtpt"u8, 276, 20);
        Tag(bytes, 3, "kTRC"u8, 296, (uint)curveSize);
        Mluc(bytes[180..240], description);
        Mluc(bytes[240..276], "CC0");

        // Upstream's grayscale branch keeps the source media white, omits
        // chad/CICP, and uses a D50 PCS illuminant in the common ICC header.
        // CIEXYZFromWhiteCIExy rounds the reciprocal to float before multiplying.
        double whiteX = transfer == TransferFunction.Dci ? 0.314 : 0.3127;
        double whiteY = transfer == TransferFunction.Dci ? 0.351 : 0.3290;
        float factor = (float)(1.0 / whiteY);
        Signature(bytes, 276, "XYZ "u8);
        Fixed(bytes, 284, (float)(whiteX * factor));
        Fixed(bytes, 288, 1f);
        Fixed(bytes, 292, (float)((1.0 - whiteX - whiteY) * factor));
        if (tableCurve)
            WriteTableCurve(bytes[296..], transfer == TransferFunction.Pq);
        else
        {
            ReadOnlySpan<float> parameters = transfer switch
            {
                TransferFunction.Linear => [1f, 1f, 0f, 1f, 0f],
                TransferFunction.Rec709 => [(float)(1.0 / 0.45), (float)(1.0 / 1.099),
                    (float)(0.099 / 1.099), (float)(1.0 / 4.5), 0.081f],
                TransferFunction.Dci => [2.6f, 1f, 0f, 1f, 0f],
                _ => [2.4f, (float)(1.0 / 1.055), (float)(0.055 / 1.055),
                    (float)(1.0 / 12.92), 0.04045f]
            };
            Curve(bytes[296..], parameters, 3);
        }
        ProfileId(bytes);
        return profile;
    }

    private static void WriteTableCurve(Span<byte> tag, bool pq)
    {
        // CreateTableCurve<64> uses a float grid and double intermediates, but
        // the transfer functions and roundf return through float. Grayscale
        // cannot use upstream's tone mapping and has no HLG OOTF adjustment.
        Signature(tag, 0, "curv"u8);
        UInt32(tag, 8, 64);
        for (int index = 0; index < 64; index++)
        {
            float encoded = (float)index / 63;
            double decoded = pq
                ? HdrToneMapping.PqDisplayFromEncoded(10000f, encoded)
                : HdrToneMapping.HlgDisplayFromEncoded(encoded);
            float scaled = (float)(Math.Clamp(decoded, 0.0, 1.0) * 65535.0);
            UInt16(tag, 12 + index * 2,
                (ushort)MathF.Round(scaled, MidpointRounding.AwayFromZero));
        }
    }
}
