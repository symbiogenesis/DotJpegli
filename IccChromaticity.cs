namespace DotJpegli;

/// <summary>A CIE 1931 xy chromaticity used by an ICC output profile.</summary>
/// <remarks>Profile factories validate the coordinates. Specifying a chromaticity
/// does not convert or adapt samples during encoding.</remarks>
/// <param name="X">The CIE x coordinate.</param>
/// <param name="Y">The CIE y coordinate.</param>
public readonly record struct IccChromaticity(double X, double Y);
