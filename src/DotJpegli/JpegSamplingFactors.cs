namespace DotJpegli;

/// <summary>Custom component sampling factors, validated when encoding.</summary>
/// <remarks>
/// Each factor must be 1..4. Ratios to the largest corresponding factor across
/// all three components may be fractional. Subsampled YCbCr luma disables adaptive quantization.
/// The sum of Horizontal * Vertical over all three components must not exceed ten.
/// Grayscale encoding ignores valid color sampling factors and always uses 1x1.
/// RGB output maps Luma, Cb and Cr to R, G and B; subsampled green disables adaptive quantization.
/// XYB output maps them to scaled X, Y and B-minus-Y; subsampled Y disables adaptive quantization.
/// </remarks>
public sealed record JpegSamplingFactors(
    JpegComponentSampling Luma, JpegComponentSampling Cb, JpegComponentSampling Cr)
{
    internal JpegComponentSampling this[int component] => component switch
    {
        0 => this.Luma,
        1 => this.Cb,
        2 => this.Cr,
        _ => throw new ArgumentOutOfRangeException(nameof(component))
    };

    // The upstream quantizer compensates only this exact sampling scheme.
    internal bool Is420 => this.Luma == new JpegComponentSampling(2, 2)
        && this.Cb == new JpegComponentSampling(1, 1) && this.Cr == new JpegComponentSampling(1, 1);

    internal int MaxHorizontal => Math.Max(this.Luma.Horizontal, Math.Max(this.Cb.Horizontal, this.Cr.Horizontal));
    internal int MaxVertical => Math.Max(this.Luma.Vertical, Math.Max(this.Cb.Vertical, this.Cr.Vertical));
    internal bool HasFullResolutionLuma => this.Luma.Horizontal == this.MaxHorizontal && this.Luma.Vertical == this.MaxVertical;

    internal void Validate()
    {
        int blocks = 0;
        for (int component = 0; component < 3; component++)
        {
            JpegComponentSampling factor = this[component];
            if (factor.Horizontal is < 1 or > 4 || factor.Vertical is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(JpegliEncoderOptions.SamplingFactors),
                    "Each component sampling factor must be between 1 and 4.");
            }

            blocks += factor.Horizontal * factor.Vertical;
        }
        if (blocks > 10)
        {
            throw new ArgumentException("A color MCU must contain at most ten blocks.",
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
