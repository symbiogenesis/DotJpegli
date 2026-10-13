namespace DotJpegli;

/// <summary>A built-in source white point described by an ICC output profile.</summary>
/// <remarks>Selecting a white point does not adapt or alter samples during encoding.</remarks>
public enum IccWhitePoint
{
    /// <summary>CIE Standard Illuminant D65, at xy (0.3127, 0.3290).</summary>
    D65 = 1,
    /// <summary>CIE Standard Illuminant E, at xy (1/3, 1/3).</summary>
    EqualEnergy = 10,
    /// <summary>The DCI white point, at xy (0.314, 0.351).</summary>
    Dci = 11
}
