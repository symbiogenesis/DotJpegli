namespace DotJpegli;

/// <summary>The byte order of gamma-encoded, eight-bit source pixels.</summary>
public enum PixelLayout
{
    Rgb24,
    Bgr24,
    /// <summary>RGBA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Rgba32,
    /// <summary>BGRA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Bgra32,
    Gray8
}
