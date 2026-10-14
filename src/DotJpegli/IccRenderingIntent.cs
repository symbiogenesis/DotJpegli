namespace DotJpegli;

/// <summary>An ICC profile's preferred display intent or the intent for source color conversion.</summary>
/// <remarks>
/// A color-management system interprets this preference when displaying the JPEG.
/// Profile factory overloads store the preference without converting samples.
/// JpegliEncoderOptions.ColorRenderingIntent selects the SourceIccProfile conversion intent.
/// </remarks>
public enum IccRenderingIntent
{
    /// <summary>Prefer preserving the overall appearance.</summary>
    Perceptual = 0,
    /// <summary>Match in-gamut colors relative to the destination media white.</summary>
    RelativeColorimetric = 1,
    /// <summary>Prefer preserving saturation.</summary>
    Saturation = 2,
    /// <summary>Match colors including the source media white.</summary>
    AbsoluteColorimetric = 3
}
