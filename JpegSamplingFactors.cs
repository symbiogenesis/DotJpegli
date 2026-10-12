namespace DotJpegli;

/// <summary>Custom Y, Cb and Cr sampling factors, validated when encoding.</summary>
/// <remarks>
/// Each factor must be 1..4. Luma must have the largest horizontal and vertical
/// factors, and each chroma factor must divide the corresponding luma factor.
/// The sum of Horizontal * Vertical over all three components must not exceed ten.
/// Grayscale encoding ignores valid color sampling factors and always uses 1x1.
/// </remarks>
public sealed record JpegSamplingFactors(
    JpegComponentSampling Luma, JpegComponentSampling Cb, JpegComponentSampling Cr)
{
    internal JpegComponentSampling this[int component] => component switch
    {
        0 => Luma,
        1 => Cb,
        2 => Cr,
        _ => throw new ArgumentOutOfRangeException(nameof(component))
    };

    // The upstream quantizer compensates only this exact sampling scheme.
    internal bool Is420 => Luma == new JpegComponentSampling(2, 2)
        && Cb == new JpegComponentSampling(1, 1) && Cr == new JpegComponentSampling(1, 1);

    internal void Validate()
    {
        int blocks = 0;
        for (int component = 0; component < 3; component++)
        {
            JpegComponentSampling factor = this[component];
            if (factor.Horizontal is < 1 or > 4 || factor.Vertical is < 1 or > 4)
                throw new ArgumentOutOfRangeException(nameof(JpegliEncoderOptions.SamplingFactors),
                    "Each component sampling factor must be between 1 and 4.");
            blocks += factor.Horizontal * factor.Vertical;
        }
        if (blocks > 10)
            throw new ArgumentException("A color MCU must contain at most ten blocks.",
                nameof(JpegliEncoderOptions.SamplingFactors));
        for (int component = 1; component < 3; component++)
        {
            JpegComponentSampling factor = this[component];
            if (Luma.Horizontal % factor.Horizontal != 0 || Luma.Vertical % factor.Vertical != 0)
                throw new ArgumentException(
                    "Luma must have the largest sampling factors, with integer ratios to each chroma component.",
                    nameof(JpegliEncoderOptions.SamplingFactors));
        }
    }

    internal static JpegSamplingFactors FromSubsampling(ChromaSubsampling subsampling)
    {
        (int horizontal, int vertical) = subsampling switch
        {
            ChromaSubsampling.None => (1, 1),
            ChromaSubsampling.HalfHorizontal => (2, 1),
            ChromaSubsampling.Quarter => (2, 2),
            ChromaSubsampling.HalfVertical => (1, 2),
            ChromaSubsampling.QuarterHorizontal => (4, 1),
            ChromaSubsampling.ThirdHorizontal => (3, 1),
            ChromaSubsampling.ThirdHorizontalHalfVertical => (3, 2),
            ChromaSubsampling.QuarterVertical => (1, 4),
            ChromaSubsampling.QuarterHorizontalHalfVertical => (4, 2),
            ChromaSubsampling.HalfHorizontalQuarterVertical => (2, 4),
            ChromaSubsampling.ThirdVertical => (1, 3),
            ChromaSubsampling.HalfHorizontalThirdVertical => (2, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(subsampling))
        };
        return new(new(horizontal, vertical), new(1, 1), new(1, 1));
    }
}
