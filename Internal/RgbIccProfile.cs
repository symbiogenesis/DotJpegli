// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using static DotJpegli.Internal.IccProfileWriter;

namespace DotJpegli.Internal;

internal static class RgbIccProfile
{
    private enum TransferFunction : byte
    {
        Rec709 = 1,
        Linear = 8,
        Srgb = 13,
        Dci = 17
    }

    // Fixed D65 and DCI RGB branches of lib/cms/jpegli_cms_internal.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    internal static byte[] CreateSrgb(bool linear = false,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        linear ? "RGB_D65_SRG_Per_Lin" : "sRGB", 1,
        linear ? TransferFunction.Linear : TransferFunction.Srgb, SrgbColorants, renderingIntent: renderingIntent);

    internal static byte[] CreateRec709(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_D65_SRG_Per_709", 1, TransferFunction.Rec709, SrgbColorants, renderingIntent: renderingIntent);

    internal static byte[] CreateRec709DisplayP3(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_D65_DCI_Per_709", 12, TransferFunction.Rec709, DisplayP3Colorants, renderingIntent: renderingIntent);

    internal static byte[] CreateGamma(double gamma, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        CreateGamma(gamma, "SRG", 1, SrgbColorants, renderingIntent);

    internal static byte[] CreateGammaDisplayP3(double gamma, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        CreateGamma(gamma, "DCI", 12, DisplayP3Colorants, renderingIntent);

    internal static byte[] CreateGammaRec2020(double gamma, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        CreateGamma(gamma, "202", 9, Rec2020Colorants, renderingIntent);

    private static byte[] CreateGamma(double gamma, string primariesDescription, byte primaries,
        ReadOnlySpan<float> colorants, IccRenderingIntent renderingIntent)
    {
        GammaTransferFunction curve = GammaTransferFunction.Create(gamma);
        return Create("RGB_D65_" + primariesDescription + "_Per_" + curve.Description, primaries,
            TransferFunction.Linear, colorants, gamma: curve.Exponent, renderingIntent: renderingIntent);
    }

    private static ReadOnlySpan<float> SrgbColorants =>
        // Precomputed columns of upstream's PrimariesToXYZD50 with its fixed
        // sRGB primaries from color_encoding_cms.h: R=(0.639998686,0.330010138),
        // G=(0.300003784,0.600003357), B=(0.150002046,0.059997204).
        // These are the float results before ICC s15Fixed16 serialization.
        [0.43602091074f, 0.222481667995f, 0.0139219872653f,
         0.385103434324f, 0.71689671278f, 0.097085095942f,
         0.143095657229f, 0.0606217607856f, 0.714202940464f];

    internal static byte[] CreateDisplayP3(bool linear = false,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        // Upstream's generic description calls the P3 primaries DCI even
        // though this branch retains D65 white.
        linear ? "RGB_D65_DCI_Per_Lin" : "DisplayP3", 12,
        linear ? TransferFunction.Linear : TransferFunction.Srgb, DisplayP3Colorants, renderingIntent: renderingIntent);

    private static ReadOnlySpan<float> DisplayP3Colorants =>
        // P3 primaries: R=(0.680,0.320), G=(0.265,0.690), B=(0.150,0.060).
        // Canonical upstream float results before ICC s15Fixed16 serialization.
        [0.51510214805603027f, 0.2411818653345108f, -0.0010494081070646644f,
         0.29196470975875854f, 0.69223642349243164f, 0.041881758719682693f,
         0.15715308487415314f, 0.066581852734088898f, 0.78437769412994385f];

    internal static byte[] CreateDciP3(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_DCI_DCI_Per_DCI", 11, TransferFunction.Dci,
        // The same P3 primaries with DCI white xy=(0.314,0.351), adapted to D50.
        // Canonical upstream float columns before ICC s15Fixed16 serialization.
        [0.48614510893821716f, 0.22667698562145233f, -0.00080051715485751629f,
         0.32383733987808228f, 0.71032541990280151f, 0.04323844239115715f,
         0.15423749387264252f, 0.062997661530971527f, 0.7827720046043396f],
        dciWhite: true, renderingIntent: renderingIntent);

    internal static byte[] CreateLinearRec2020(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_D65_202_Per_Lin", 9, TransferFunction.Linear, Rec2020Colorants, renderingIntent: renderingIntent);

    internal static byte[] CreateRec2020(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_D65_202_Per_709", 9, TransferFunction.Rec709, Rec2020Colorants, renderingIntent: renderingIntent);

    internal static byte[] CreateSrgbRec2020(IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) => Create(
        "RGB_D65_202_Per_SRG", 9, TransferFunction.Srgb, Rec2020Colorants, renderingIntent: renderingIntent);

    internal static ReadOnlySpan<float> Rec2020Colorants =>
        // Rec.2020 (upstream PRIMARIES_2100) primaries:
        // R=(0.708,0.292), G=(0.170,0.797), B=(0.131,0.046).
        // Canonical upstream float results before ICC s15Fixed16 serialization.
        [0.67345893383026123f, 0.27903327345848083f, -0.0019313860684633255f,
         0.16566081345081329f, 0.67533797025680542f, 0.029979368671774864f,
         0.12510021030902863f, 0.045628782361745834f, 0.79716199636459351f];

    private static byte[] Create(string description, byte primaries, TransferFunction transfer,
        ReadOnlySpan<float> colorants, bool dciWhite = false, float? gamma = null,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual)
    {
        string intentName = RenderingIntentName(renderingIntent);
        if (renderingIntent != IccRenderingIntent.Perceptual)
        {
            description = description switch
            {
                "sRGB" => "RGB_D65_SRG_Per_SRG",
                "DisplayP3" => "RGB_D65_DCI_Per_SRG",
                _ => description
            };
            description = description.Replace("_Per_", "_" + intentName + "_", StringComparison.Ordinal);
        }
        bool includeCicp = gamma is null;
        int tagCount = includeCicp ? 11 : 10;
        int descriptionOffset = 132 + tagCount * 12;
        int descriptionSize = (28 + description.Length * 2 + 3) & ~3;
        int copyrightOffset = descriptionOffset + descriptionSize;
        int whiteOffset = copyrightOffset + 36;
        int adaptationOffset = whiteOffset + 20;
        int cicpOffset = adaptationOffset + 44;
        int redOffset = cicpOffset + (includeCicp ? 12 : 0);
        int greenOffset = redOffset + 20;
        int blueOffset = greenOffset + 20;
        int curveOffset = blueOffset + 20;
        int curveSize = includeCicp ? 32 : 16;
        byte[] profile = new byte[curveOffset + curveSize];
        Span<byte> bytes = profile;
        Header(bytes, "mntr"u8, "RGB "u8, renderingIntent);
        UInt32(bytes, 128, (uint)tagCount);
        Tag(bytes, 0, "desc"u8, (uint)descriptionOffset, (uint)descriptionSize);
        Tag(bytes, 1, "cprt"u8, (uint)copyrightOffset, 36);
        Tag(bytes, 2, "wtpt"u8, (uint)whiteOffset, 20);
        Tag(bytes, 3, "chad"u8, (uint)adaptationOffset, 44);
        int nextTag = 4;
        if (includeCicp)
            Tag(bytes, nextTag++, "cicp"u8, (uint)cicpOffset, 12);
        Tag(bytes, nextTag++, "rXYZ"u8, (uint)redOffset, 20);
        Tag(bytes, nextTag++, "gXYZ"u8, (uint)greenOffset, 20);
        Tag(bytes, nextTag++, "bXYZ"u8, (uint)blueOffset, 20);
        // Upstream shares one decoding curve among all three channels.
        Tag(bytes, nextTag++, "rTRC"u8, (uint)curveOffset, (uint)curveSize);
        Tag(bytes, nextTag++, "gTRC"u8, (uint)curveOffset, (uint)curveSize);
        Tag(bytes, nextTag, "bTRC"u8, (uint)curveOffset, (uint)curveSize);
        Mluc(bytes[descriptionOffset..copyrightOffset], description);
        Mluc(bytes[copyrightOffset..whiteOffset], "CC0");
        Xyz(bytes[whiteOffset..adaptationOffset], [0.964203f, 1f, 0.824905f]);
        if (dciWhite)
            WriteChromaticAdaptation(bytes[adaptationOffset..cicpOffset], 0.314f, 0.351f);
        else
            WriteChromaticAdaptation(bytes[adaptationOffset..cicpOffset]);
        if (includeCicp)
        {
            Signature(bytes, cicpOffset, "cicp"u8);
            bytes[cicpOffset + 8] = primaries;
            bytes[cicpOffset + 9] = (byte)transfer;
            bytes[cicpOffset + 11] = 1; // RGB matrix (zero), full range.
        }

        Xyz(bytes[redOffset..greenOffset], colorants[..3]);
        Xyz(bytes[greenOffset..blueOffset], colorants[3..6]);
        Xyz(bytes[blueOffset..curveOffset], colorants[6..9]);
        if (gamma is float exponent)
            Curve(bytes[curveOffset..], [exponent], 0);
        else
        {
            ReadOnlySpan<float> parameters = transfer switch
            {
                TransferFunction.Linear => [1f, 1f, 0f, 1f, 0f],
                TransferFunction.Dci => [2.6f, 1f, 0f, 1f, 0f],
                TransferFunction.Rec709 => [(float)(1.0 / 0.45), (float)(1.0 / 1.099),
                    (float)(0.099 / 1.099), (float)(1.0 / 4.5), 0.081f],
                TransferFunction.Srgb => [2.4f, (float)(1.0 / 1.055), (float)(0.055 / 1.055),
                    (float)(1.0 / 12.92), 0.04045f],
                _ => throw new ArgumentOutOfRangeException(nameof(transfer))
            };
            Curve(bytes[curveOffset..], parameters, 3);
        }
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
