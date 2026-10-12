namespace DotJpegli;

/// <summary>The component order of 16-bit or normalized floating-point source pixels.</summary>
public enum PixelComponentLayout
{
    Rgb,
    Bgr,
    /// <summary>RGBA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Rgba,
    /// <summary>BGRA input. Alpha is ignored; composite translucent pixels before encoding.</summary>
    Bgra,
    Gray,
    /// <summary>
    /// Full-range JPEG Y, Cb, Cr input, with three interleaved samples per pixel.
    /// Chroma is centered at 32896 for 16-bit samples or 128f / 255f for normalized floats.
    /// No RGB conversion or video-range expansion is applied.
    /// </summary>
    YCbCr
}
