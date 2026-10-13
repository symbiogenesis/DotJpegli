namespace DotJpegli;

/// <summary>A built-in transfer function described by an ICC output profile.</summary>
/// <remarks>Samples must already use the selected transfer function; attaching a profile does not convert them.</remarks>
public enum IccTransferFunction
{
    /// <summary>The pinned builder's BT.709 transfer function.</summary>
    Rec709 = 1,
    /// <summary>Linear-light samples.</summary>
    Linear = 8,
    /// <summary>The piecewise sRGB transfer function.</summary>
    Srgb = 13,
    /// <summary>The SMPTE ST 2084 perceptual quantizer (PQ) transfer function.</summary>
    Pq = 16,
    /// <summary>The gamma 2.6 decoding curve from SMPTE ST 428-1.</summary>
    Dci = 17,
    /// <summary>The Rec.2100 hybrid log-gamma (HLG) transfer function.</summary>
    Hlg = 18
}
