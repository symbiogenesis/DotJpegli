using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Output profiles for the managed encoder.</summary>
public static class JpegliIccProfiles
{
    /// <summary>Creates an ICC v4 RGB display profile from custom CIE xy
    /// primaries and a source white point.</summary>
    /// <remarks>
    /// RGB colorants are adapted to D50 with the pinned builder's Bradford transform.
    /// Assign the returned profile explicitly to JpegliEncoderOptions.IccProfile for
    /// RGB or corresponding YCbCr output already using these chromaticities and
    /// the selected transfer function. Attaching it preserves the source samples.
    /// Custom profiles omit CICP. PQ and HLG use shared 64-entry decoding curves
    /// with no tone mapping or HLG OOTF adjustment. Rendering intent is a display
    /// preference. Every call returns a separate mutable array with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates, transfer function
    /// or rendering intent are invalid, or the derived ICC values are unrepresentable.</exception>
    /// <exception cref="ArgumentException">The primaries do not define an invertible RGB matrix.</exception>
    public static byte[] CreateRgbProfile(IccRgbPrimaries primaries, IccChromaticity whitePoint,
        IccTransferFunction transferFunction, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        RgbIccProfile.Create(primaries, whitePoint, transferFunction, renderingIntent);

    /// <summary>Creates an ICC v4 grayscale display profile with a custom CIE xy
    /// source white point and the selected transfer function.</summary>
    /// <remarks>
    /// The profile retains the source media white and a D50 XYZ PCS header, without
    /// chromatic-adaptation or CICP tags. PQ and HLG use 64-entry decoding curves
    /// with no tone mapping or HLG OOTF adjustment. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for samples already using the selected encoding.
    /// Attaching it preserves samples. Every call returns a separate mutable array.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates, transfer function
    /// or rendering intent are invalid, or the source white is unrepresentable in ICC.</exception>
    public static byte[] CreateGrayscaleProfile(IccChromaticity whitePoint, IccTransferFunction transferFunction,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        GrayscaleIccProfile.Create(whitePoint, transferFunction, renderingIntent);

    /// <summary>Creates an ICC v4 RGB display profile for power-law samples
    /// with custom CIE xy primaries and a source white point.</summary>
    /// <remarks>
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and within
    /// 1..8192. The profile uses a shared type-0 curve, D50-adapted colorants and no
    /// CICP tag. Assign it explicitly to JpegliEncoderOptions.IccProfile for RGB or
    /// corresponding YCbCr samples already using that encoding. Attaching it preserves
    /// samples. Every call returns a separate mutable array with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates or rendering intent
    /// are invalid, gamma is outside its range, or the derived ICC values are unrepresentable.</exception>
    /// <exception cref="ArgumentException">The primaries do not define an invertible RGB matrix.</exception>
    public static byte[] CreateGammaRgbProfile(IccRgbPrimaries primaries, IccChromaticity whitePoint,
        double gamma, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        RgbIccProfile.CreateGamma(primaries, whitePoint, gamma, renderingIntent);

    /// <summary>Creates an ICC v4 grayscale display profile for power-law samples
    /// with a custom CIE xy source white point.</summary>
    /// <remarks>
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and within
    /// 1..8192. The profile retains the source media white and a D50 XYZ PCS header
    /// with a type-0 curve, without chromatic-adaptation or CICP tags. Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for samples already using that
    /// encoding. Attaching it preserves samples. Every call returns a separate mutable array.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates or rendering intent
    /// are invalid, gamma is outside its range, or the source white is unrepresentable in ICC.</exception>
    public static byte[] CreateGammaGrayscaleProfile(IccChromaticity whitePoint, double gamma,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        GrayscaleIccProfile.CreateGamma(whitePoint, gamma, renderingIntent);

    /// <summary>Creates an ICC v4 RGB display profile from the pinned builder's
    /// built-in primaries, white point and transfer function.</summary>
    /// <remarks>
    /// Assign the profile to JpegliEncoderOptions.IccProfile for RGB or corresponding
    /// YCbCr output whose samples already use the selected color encoding and full range.
    /// Attaching it preserves samples without color conversion or limited-range expansion.
    /// RGB colorants and the source white point are adapted to a D50 PCS.
    /// PQ and HLG profiles with D65 white, or P3 primaries and DCI white, use the pinned
    /// builder's forward tone/gamut transform to D50 Lab and an identity compatibility
    /// B2A tag that is not an inverse transform. Other PQ and HLG combinations use a
    /// 64-entry decoding curve with no tone mapping or HLG OOTF adjustment.
    /// A CICP tag is included only for D65 white or P3 primaries with DCI white.
    /// Selecting an intent changes the display preference, not the transform.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">A primaries, white point,
    /// transfer function or rendering intent value is invalid.</exception>
    public static byte[] CreateRgbProfile(IccPrimaries primaries, IccWhitePoint whitePoint,
        IccTransferFunction transferFunction, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        RgbIccProfile.Create(primaries, whitePoint, transferFunction, renderingIntent);

    /// <summary>Creates an ICC v4 grayscale display profile from the pinned builder's
    /// built-in white point and transfer function.</summary>
    /// <remarks>
    /// Assign the profile to JpegliEncoderOptions.IccProfile for full-range grayscale
    /// samples already using the selected transfer function. Attaching it preserves
    /// samples without color conversion or limited-range expansion.
    /// The profile keeps the source media white and a D50 XYZ PCS header without
    /// chromatic-adaptation or CICP tags. PQ and HLG use a 64-entry decoding curve
    /// with no tone mapping or HLG OOTF adjustment. Selecting an intent changes the
    /// display preference, not the decoding curve.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">A white point, transfer function
    /// or rendering intent value is invalid.</exception>
    public static byte[] CreateGrayscaleProfile(IccWhitePoint whitePoint, IccTransferFunction transferFunction,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        GrayscaleIccProfile.Create(whitePoint, transferFunction, renderingIntent);

    /// <summary>Creates an ICC v4 RGB display profile for power-law samples with
    /// the pinned builder's built-in primaries and white point.</summary>
    /// <remarks>
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and from
    /// 1 to 8192. The profile uses a shared type-0 parametric curve, D50-adapted
    /// RGB colorants and source white, and no CICP tag. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output whose
    /// full-range samples already use those primaries, white and power law.
    /// Attaching it preserves samples without color conversion, limited-range
    /// expansion or tone mapping. Every call returns a separate mutable array
    /// with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">A primaries, white point or
    /// rendering intent value is invalid, or gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaRgbProfile(IccPrimaries primaries, IccWhitePoint whitePoint,
        double gamma, IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        RgbIccProfile.CreateGamma(primaries, whitePoint, gamma, renderingIntent);

    /// <summary>Creates an ICC v4 grayscale display profile for power-law samples
    /// with the pinned builder's built-in white point.</summary>
    /// <remarks>
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and from
    /// 1 to 8192. The profile uses a type-0 parametric curve and keeps the source
    /// media white with a D50 XYZ PCS header, without chromatic-adaptation or CICP
    /// tags. Assign it explicitly to JpegliEncoderOptions.IccProfile for full-range
    /// grayscale output already using that white and power law. Attaching it
    /// preserves samples without color conversion or tone mapping.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">A white point or rendering
    /// intent value is invalid, or gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaGrayscaleProfile(IccWhitePoint whitePoint, double gamma,
        IccRenderingIntent renderingIntent = IccRenderingIntent.Perceptual) =>
        GrayscaleIccProfile.CreateGamma(whitePoint, gamma, renderingIntent);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB profile for gamma-encoded sRGB
    /// samples, with D65 white adapted to D50 XYZ. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or YCbCr output whose RGB
    /// interpretation is sRGB. It does not convert source samples and is not
    /// a grayscale or scaled XYB profile.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateSrgbProfile() => RgbIccProfile.CreateSrgb();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// power-law samples with sRGB/Rec.709 primaries and D65 white adapted to D50.
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and from
    /// 1 to 8192; for example, 2.2 specifies a pure gamma 2.2 curve. The profile
    /// uses a shared type-0 parametric curve and has no CICP tag. This is a pure
    /// power law rather than the piecewise sRGB transfer function. Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for RGB or corresponding
    /// YCbCr output. Attaching it preserves samples without color conversion.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaSrgbProfile(double gamma) => RgbIccProfile.CreateGamma(gamma);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// power-law samples with Display P3 primaries and D65 white adapted to D50.
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and from
    /// 1 to 8192. A shared type-0 parametric curve describes the power law;
    /// this profile has no CICP tag. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output
    /// already using those primaries and that power law. Attaching it preserves
    /// samples without color conversion. For ordinary Display P3 samples with
    /// the piecewise sRGB transfer function, use CreateDisplayP3Profile instead.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaDisplayP3Profile(double gamma) => RgbIccProfile.CreateGammaDisplayP3(gamma);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// power-law samples with Rec.2020 primaries and D65 white adapted to D50.
    /// Gamma is the decoding exponent (linear = encoded^gamma), finite and from
    /// 1 to 8192. A shared type-0 parametric curve describes the power law;
    /// this profile has no CICP tag. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output
    /// already using those primaries and that power law. Attaching it preserves
    /// samples without color conversion, limited-range expansion or tone mapping.
    /// This profile does not describe BT.709-transfer, PQ or HLG samples.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaRec2020Profile(double gamma) => RgbIccProfile.CreateGammaRec2020(gamma);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for full-range
    /// power-law samples with D65 media white and a D50 XYZ PCS header. Gamma is
    /// the decoding exponent (linear = encoded^gamma), finite and from 1 to 8192.
    /// The profile uses a type-0 parametric curve without chad or CICP tags.
    /// Assign it explicitly to JpegliEncoderOptions.IccProfile for grayscale output
    /// already using that power law. Attaching it does not convert source samples.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Gamma is nonfinite or outside 1..8192.</exception>
    public static byte[] CreateGammaGrayscaleProfile(double gamma) => GrayscaleIccProfile.CreateGamma(gamma);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for samples with
    /// Rec.709 primaries, the BT.709 transfer function and D65 white adapted
    /// to D50 XYZ. Assign it explicitly to JpegliEncoderOptions.IccProfile for
    /// RGB or YCbCr output whose RGB interpretation is Rec.709. Samples must
    /// already use those primaries and transfer function; attaching this
    /// profile does not convert source samples or expand limited-range video.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec709Profile() => RgbIccProfile.CreateRec709();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// samples with Display P3 primaries, the BT.709 transfer function and D65
    /// white adapted to D50 XYZ. Its CICP fields are [12, 1, 0, 1]. Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for RGB or corresponding
    /// YCbCr output already using those primaries and that transfer function.
    /// Attaching it preserves samples without color conversion, limited-range
    /// expansion or tone mapping. Ordinary Display P3 samples use the sRGB
    /// transfer function and should use CreateDisplayP3Profile instead.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec709DisplayP3Profile() => RgbIccProfile.CreateRec709DisplayP3();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for samples with
    /// Rec.2020 primaries, the pinned builder's BT.709 transfer function and
    /// D65 white adapted to D50 XYZ. Its CICP transfer code is 1. Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for RGB or YCbCr output
    /// with that RGB interpretation. Samples must already use those primaries
    /// and transfer function; attaching this profile does not convert source
    /// samples, expand limited-range video or apply tone mapping.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec2020Profile() => RgbIccProfile.CreateRec2020();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// samples with Rec.2020 primaries, the sRGB transfer function and D65 white
    /// adapted to D50 XYZ. Its CICP fields are [9, 13, 0, 1]. Assign it explicitly
    /// to JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output
    /// already using those primaries and that transfer function. Attaching it
    /// preserves samples without color conversion, limited-range expansion or
    /// tone mapping. For BT.709-transfer Rec.2020 use CreateRec2020Profile;
    /// for PQ or HLG use the corresponding Rec.2100 factory instead.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateSrgbRec2020Profile() => RgbIccProfile.CreateSrgbRec2020();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// Rec.2100 PQ samples with Rec.2020 primaries and D65 white. Its forward
    /// transform contains the pinned builder's 10,000-to-250-nit tone mapping
    /// and gamut mapping to D50 Lab. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output.
    /// Attaching it does not convert or tone-map the encoded source samples.
    /// Its compatibility B2A tag is an identity, not an inverse transform.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec2100PqProfile() => HdrIccProfile.Create(pq: true);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for full-range
    /// Rec.2100 HLG samples with Rec.2020 primaries and D65 white. Its forward
    /// transform contains the pinned builder's 300-to-80-nit HLG OOTF
    /// adjustment and gamut mapping to D50 Lab. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for RGB or corresponding YCbCr output.
    /// Attaching it does not convert or tone-map the encoded source samples.
    /// Its compatibility B2A tag is an identity, not an inverse transform.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec2100HlgProfile() => HdrIccProfile.Create(pq: false);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for Display P3
    /// samples, with P3 primaries, the sRGB transfer function and D65 white
    /// adapted to D50 XYZ. Assign it explicitly to JpegliEncoderOptions.IccProfile
    /// for RGB or YCbCr output whose RGB interpretation is Display P3. Samples
    /// must already use Display P3; attaching the profile does not convert source
    /// samples. It is not a grayscale or scaled XYB profile.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateDisplayP3Profile() => RgbIccProfile.CreateDisplayP3();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for DCI-P3
    /// samples, with P3 primaries, DCI white adapted to D50 XYZ and a gamma
    /// 2.6 decoding curve. Assign it explicitly to JpegliEncoderOptions.IccProfile
    /// for RGB or YCbCr output whose RGB interpretation is DCI-P3. Samples
    /// must already use that color space; attaching this profile does not
    /// convert source samples. It does not describe Display P3 or cinema XYZ.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateDciP3Profile() => RgbIccProfile.CreateDciP3();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB profile for linear-light samples
    /// with sRGB primaries and D65 white adapted to D50 XYZ. Assign it explicitly
    /// to JpegliEncoderOptions.IccProfile for RGB or YCbCr output whose RGB
    /// interpretation is linear sRGB. Samples must already use a linear transfer
    /// function; attaching this profile does not linearize gamma-encoded input.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateLinearSrgbProfile() => RgbIccProfile.CreateSrgb(linear: true);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for linear-light
    /// samples with Display P3 primaries and D65 white adapted to D50 XYZ.
    /// Assign it explicitly to JpegliEncoderOptions.IccProfile for RGB or YCbCr
    /// output whose RGB interpretation is linear Display P3. Samples must
    /// already use those primaries and a linear transfer function; attaching
    /// this profile does not convert or linearize source samples.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateLinearDisplayP3Profile() => RgbIccProfile.CreateDisplayP3(linear: true);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 RGB display profile for linear-light
    /// samples with Rec.2020 primaries and D65 white adapted to D50 XYZ.
    /// Assign it explicitly to JpegliEncoderOptions.IccProfile for RGB or YCbCr
    /// output whose RGB interpretation is linear Rec.2020. Samples must
    /// already use those primaries and a linear transfer function; attaching
    /// this profile does not convert, linearize or tone-map source samples.
    /// It is not a PQ or HLG profile.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateLinearRec2020Profile() => RgbIccProfile.CreateLinearRec2020();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// gamma-encoded samples with the sRGB transfer function and D65 media white.
    /// Assign it explicitly to JpegliEncoderOptions.IccProfile for grayscale
    /// output. The profile retains upstream's D65 media-white tag and D50 PCS
    /// header, without a chromatic-adaptation or CICP tag. It does not convert
    /// source samples and is not an RGB or scaled XYB profile.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateSrgbGrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Srgb);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// linear-light samples with D65 media white. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for grayscale output. The profile retains
    /// upstream's D65 media-white tag and D50 PCS header, without a
    /// chromatic-adaptation or CICP tag. Samples must already be linear;
    /// attaching this profile does not linearize gamma-encoded input.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateLinearSrgbGrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Linear);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// full-range BT.709-transfer samples with D65 media white. Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for grayscale output.
    /// Samples must already use the BT.709 transfer function; attaching the
    /// profile does not convert samples or expand limited-range video.
    /// The profile retains upstream's D50 PCS header without chad or CICP.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec709GrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Rec709);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// gamma 2.6 samples with DCI media white at xy (0.314, 0.351). Assign it
    /// explicitly to JpegliEncoderOptions.IccProfile for grayscale output.
    /// Attaching it does not convert source samples. It retains upstream's
    /// D50 PCS header without chad or CICP and does not describe cinema XYZ.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateDciGrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Dci);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// full-range PQ samples with D65 media white and a 64-entry decoding
    /// curve normalized to 10,000 nits. Assign it explicitly to
    /// JpegliEncoderOptions.IccProfile for grayscale output. This profile
    /// has no tone mapping or CICP tag; attaching it preserves source samples
    /// and does not establish HDR display or Rec.2100 conformance.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec2100PqGrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Pq);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// full-range HLG samples with D65 media white and a 64-entry decoding
    /// curve. Assign it explicitly to JpegliEncoderOptions.IccProfile for
    /// grayscale output. This profile has no tone mapping, HLG OOTF adjustment
    /// or CICP tag; attaching it preserves source samples and does not
    /// establish HDR display or Rec.2100 conformance.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateRec2100HlgGrayscaleProfile() =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Hlg);

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 profile for scaled X, Y and B-minus-Y
    /// output at the default intensity target of 255. Assign it to
    /// JpegliEncoderOptions.IccProfile when encoding XYB output.
    /// The forward transform describes scaled XYB to D50 XYZ for color-managed
    /// display. Its compatibility B2A tag is an identity, not an inverse transform;
    /// do not use this profile to convert RGB input to XYB.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateXybProfile() => XybIccProfile.Create();

    /// <summary>Creates the output profile described by <see cref="CreateSrgbProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateSrgbProfile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateSrgb(renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateGammaSrgbProfile(double)"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid. Gamma must be finite and within 1..8192.</exception>
    public static byte[] CreateGammaSrgbProfile(double gamma, IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateGamma(gamma, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateGammaDisplayP3Profile(double)"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid. Gamma must be finite and within 1..8192.</exception>
    public static byte[] CreateGammaDisplayP3Profile(double gamma, IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateGammaDisplayP3(gamma, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateGammaRec2020Profile(double)"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid. Gamma must be finite and within 1..8192.</exception>
    public static byte[] CreateGammaRec2020Profile(double gamma, IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateGammaRec2020(gamma, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateGammaGrayscaleProfile(double)"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid. Gamma must be finite and within 1..8192.</exception>
    public static byte[] CreateGammaGrayscaleProfile(double gamma, IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.CreateGamma(gamma, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec709Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec709Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateRec709(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec709DisplayP3Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec709DisplayP3Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateRec709DisplayP3(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec2020Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec2020Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateRec2020(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateSrgbRec2020Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateSrgbRec2020Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateSrgbRec2020(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec2100PqProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec2100PqProfile(IccRenderingIntent renderingIntent) =>
        HdrIccProfile.Create(pq: true, renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec2100HlgProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec2100HlgProfile(IccRenderingIntent renderingIntent) =>
        HdrIccProfile.Create(pq: false, renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateDisplayP3Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateDisplayP3Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateDisplayP3(renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateDciP3Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateDciP3Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateDciP3(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateLinearSrgbProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateLinearSrgbProfile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateSrgb(linear: true, renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateLinearDisplayP3Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateLinearDisplayP3Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateDisplayP3(linear: true, renderingIntent: renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateLinearRec2020Profile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateLinearRec2020Profile(IccRenderingIntent renderingIntent) =>
        RgbIccProfile.CreateLinearRec2020(renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateSrgbGrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateSrgbGrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Srgb, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateLinearSrgbGrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateLinearSrgbGrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Linear, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec709GrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec709GrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Rec709, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateDciGrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateDciGrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Dci, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec2100PqGrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec2100PqGrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Pq, renderingIntent);

    /// <summary>Creates the output profile described by <see cref="CreateRec2100HlgGrayscaleProfile()"/>
    /// with the specified rendering intent.</summary>
    /// <remarks>The intent is a display preference; attaching the profile preserves source samples.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The rendering intent is invalid.</exception>
    public static byte[] CreateRec2100HlgGrayscaleProfile(IccRenderingIntent renderingIntent) =>
        GrayscaleIccProfile.Create(GrayscaleIccProfile.TransferFunction.Hlg, renderingIntent);
}
