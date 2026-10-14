namespace DotJpegli;

/// <summary>The CIE 1931 xy chromaticities of three RGB primaries.</summary>
/// <remarks>The source white point and transfer function are selected separately.
/// Finite imaginary primaries are supported when their matrix is representable.</remarks>
/// <param name="Red">The red primary's chromaticity.</param>
/// <param name="Green">The green primary's chromaticity.</param>
/// <param name="Blue">The blue primary's chromaticity.</param>
public readonly record struct IccRgbPrimaries(IccChromaticity Red, IccChromaticity Green, IccChromaticity Blue);
