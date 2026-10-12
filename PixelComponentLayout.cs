namespace DotJpegli;

/// <summary>The component order of gamma-encoded 16-bit or normalized floating-point pixels.</summary>
public enum PixelComponentLayout
{
    Rgb,
    Bgr,
    /// <summary>RGBA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Rgba,
    /// <summary>BGRA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Bgra,
    Gray
}
