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
    public static byte[] CreateSrgbProfile() => SrgbIccProfile.Create();

    /// <summary>
    /// Creates Jpegli's perceptual ICC v4 grayscale display profile for
    /// gamma-encoded samples with the sRGB transfer function and D65 media white.
    /// Assign it explicitly to JpegliEncoderOptions.IccProfile for grayscale
    /// output. The profile retains upstream's D65 media-white tag and D50 PCS
    /// header, without a chromatic-adaptation or CICP tag. It does not convert
    /// source samples and is not an RGB or scaled XYB profile.
    /// Every call returns a separate mutable array with deterministic bytes.
    /// </summary>
    public static byte[] CreateSrgbGrayscaleProfile() => SrgbGrayscaleIccProfile.Create();

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
