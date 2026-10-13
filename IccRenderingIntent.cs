namespace DotJpegli;

/// <summary>The preferred rendering intent stored in a built-in ICC output profile.</summary>
/// <remarks>
/// A color-management system interprets this preference when displaying the JPEG.
/// Selecting an intent does not convert or alter samples during encoding.
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
