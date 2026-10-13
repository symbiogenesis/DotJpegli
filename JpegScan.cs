namespace DotJpegli;

/// <summary>Describes one sequential or progressive JPEG scan in an explicit scan script.</summary>
/// <param name="Components">
/// Zero-based output component indices in strictly increasing order. Grayscale has
/// component 0; YCbCr uses 0/1/2 for Y/Cb/Cr, RGB uses R/G/B, and scaled XYB uses
/// X/Y/B-minus-Y. These are frame indices, not the component identifiers written
/// into JPEG markers. Sequential and progressive DC scans may select several
/// components; progressive AC scans select exactly one.
/// </param>
/// <param name="SpectralStart">
/// First coefficient in JPEG zigzag order, from 0 to 63. Position 0 is DC.
/// Sequential scans use 0..63; progressive DC scans use 0..0, and progressive
/// AC scans start at a position from 1 to 63.
/// </param>
/// <param name="SpectralEnd">Inclusive last coefficient in JPEG zigzag order, from SpectralStart to 63.</param>
/// <param name="SuccessiveHigh">
/// Previous approximation bit for a refinement scan, from 1 to 10, or 0 for an
/// initial scan. Refinement requires all selected coefficients to have reached
/// this bit in earlier scans and delivers exactly the next lower bit.
/// </param>
/// <param name="SuccessiveLow">
/// Number of low coefficient bits deferred by this scan, from 0 to 10.
/// Refinement requires SuccessiveLow = SuccessiveHigh - 1. Sequential scans
/// use zero for both successive approximation values.
/// </param>
/// <remarks>
/// The first scan determines sequential or progressive mode from its spectral
/// range. The script must deliver every coefficient of every output component
/// through bit zero, and a component's initial DC scan must precede its AC scans.
/// The encoder copies this descriptor and its component list before output;
/// callers must keep their supplied lists unchanged while encoding starts.
/// </remarks>
public sealed record JpegScan(
    IReadOnlyList<int> Components,
    int SpectralStart = 0,
    int SpectralEnd = 63,
    int SuccessiveHigh = 0,
    int SuccessiveLow = 0);
