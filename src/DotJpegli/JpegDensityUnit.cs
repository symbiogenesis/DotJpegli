namespace DotJpegli;

/// <summary>Units for the horizontal and vertical densities in the JFIF header.</summary>
public enum JpegDensityUnit
{
    /// <summary>No physical units; the densities specify the pixel aspect ratio.</summary>
    Unspecified,
    /// <summary>Pixels per inch.</summary>
    DotsPerInch,
    /// <summary>Pixels per centimeter.</summary>
    DotsPerCentimeter
}
