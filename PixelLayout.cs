namespace DotJpegli;

/// <summary>The component order of eight-bit source pixels.</summary>
public enum PixelLayout
{
    Rgb24,
    Bgr24,
    /// <summary>RGBA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Rgba32,
    /// <summary>BGRA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Bgra32,
    Gray8,
    /// <summary>
    /// Full-range JPEG Y, Cb, Cr input, with three interleaved samples per pixel.
    /// Chroma is centered at 128. No RGB conversion or video-range expansion is applied.
    /// </summary>
    YCbCr24
}
