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
    QuarterHorizontal,
    /// <summary>One chroma sample per three horizontal pixels (3x1).</summary>
    ThirdHorizontal,
    /// <summary>One chroma sample per three-by-two pixel area (3x2).</summary>
    ThirdHorizontalHalfVertical,
    /// <summary>One chroma sample per four vertical pixels (1x4).</summary>
    QuarterVertical,
    /// <summary>4:1:0: one chroma sample per four-by-two pixel area (4x2).</summary>
    QuarterHorizontalHalfVertical,
    /// <summary>One chroma sample per two-by-four pixel area (2x4).</summary>
    HalfHorizontalQuarterVertical,
    /// <summary>One chroma sample per three vertical pixels (1x3).</summary>
    ThirdVertical,
    /// <summary>One chroma sample per two-by-three pixel area (2x3).</summary>
    HalfHorizontalThirdVertical
}
