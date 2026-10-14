namespace DotJpegli;

/// <summary>Built-in RGB primaries described by an ICC output profile.</summary>
/// <remarks>The white point and transfer function are selected separately.</remarks>
public enum IccPrimaries
{
    /// <summary>The pinned builder's sRGB/Rec.709 primaries.</summary>
    Srgb = 1,
    /// <summary>P3 primaries, shared by Display P3 and DCI-P3.</summary>
    DisplayP3 = 11,
    /// <summary>Rec.2020 primaries, also used by Rec.2100.</summary>
    Rec2020 = 9
}
