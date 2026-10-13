using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Built-in output profiles for the managed encoder.</summary>
public static class JpegliIccProfiles
{
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
}
