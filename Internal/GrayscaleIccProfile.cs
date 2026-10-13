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

    // Fixed D65 and DCI grayscale branches of lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] Create(TransferFunction transfer,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
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
        return Create(transfer, description, renderingIntent: renderingIntent);
    }

    internal static byte[] CreateGamma(double gamma,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
    {
        GammaTransferFunction curve = GammaTransferFunction.Create(gamma);
        return Create(TransferFunction.Linear, "Gra_D65_Per_" + curve.Description, curve.Exponent, renderingIntent);
    }

    private static byte[] Create(TransferFunction transfer, string description, float? gamma = null,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
    {
        description = description.Replace("_Per_", "_" + RenderingIntentName(renderingIntent) + "_", StringComparison.Ordinal);
        bool tableCurve = transfer is TransferFunction.Pq or TransferFunction.Hlg;
        int curveSize = gamma is not null ? 16 : tableCurve ? 12 + 64 * 2 : 32;
        int descriptionSize = (28 + description.Length * 2 + 3) & ~3;
        int copyrightOffset = 180 + descriptionSize;
        int whiteOffset = copyrightOffset + 36;
        int curveOffset = whiteOffset + 20;
        byte[] profile = new byte[curveOffset + curveSize];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "GRAY"u8, renderingIntent);
        UInt32(bytes, 128, 4);
        Tag(bytes, 0, "desc"u8, 180, (uint)descriptionSize);
        Tag(bytes, 1, "cprt"u8, (uint)copyrightOffset, 36);
        Tag(bytes, 2, "wtpt"u8, (uint)whiteOffset, 20);
        Tag(bytes, 3, "kTRC"u8, (uint)curveOffset, (uint)curveSize);
        Mluc(bytes[180..copyrightOffset], description);
        Mluc(bytes[copyrightOffset..whiteOffset], "CC0");

        // Upstream's grayscale branch keeps the source media white, omits
        // chad/CICP, and uses a D50 PCS illuminant in the common ICC header.
        // CIEXYZFromWhiteCIExy rounds the reciprocal to float before multiplying.
        double whiteX = transfer == TransferFunction.Dci ? 0.314 : 0.3127;
        double whiteY = transfer == TransferFunction.Dci ? 0.351 : 0.3290;
        float factor = (float)(1.0 / whiteY);
        Signature(bytes, whiteOffset, "XYZ "u8);
        Fixed(bytes, whiteOffset + 8, (float)(whiteX * factor));
        Fixed(bytes, whiteOffset + 12, 1f);
        Fixed(bytes, whiteOffset + 16, (float)((1.0 - whiteX - whiteY) * factor));
        if (gamma is float exponent)
            Curve(bytes[curveOffset..], [exponent], 0);
        else if (tableCurve)
            WriteTableCurve(bytes[curveOffset..], transfer == TransferFunction.Pq);
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
            Curve(bytes[curveOffset..], parameters, 3);
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
