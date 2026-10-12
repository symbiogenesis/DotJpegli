namespace DotJpegli;

/// <summary>Sampling of Cb and Cr relative to the luminance component.</summary>
public enum ChromaSubsampling
{
    /// <summary>4:4:4: one chroma sample per pixel.</summary>
    None,
    /// <summary>4:2:2: one chroma sample per two horizontal pixels.</summary>
    HalfHorizontal,
    /// <summary>4:2:0: one chroma sample per two-by-two pixel area.</summary>
    Quarter,
    /// <summary>4:4:0: one chroma sample per two vertical pixels.</summary>
    HalfVertical,
    /// <summary>4:1:1: one chroma sample per four horizontal pixels.</summary>
    QuarterHorizontal
}
