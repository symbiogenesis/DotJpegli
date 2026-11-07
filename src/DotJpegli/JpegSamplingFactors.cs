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

    internal static readonly JpegSamplingFactors Xyb = new(new(2, 2), new(2, 2), new(1, 1));

    private static readonly JpegSamplingFactors[] Presets =
    [
        new(new(1, 1), new(1, 1), new(1, 1)), // None
        new(new(2, 1), new(1, 1), new(1, 1)), // HalfHorizontal
        new(new(2, 2), new(1, 1), new(1, 1)), // Quarter
        new(new(1, 2), new(1, 1), new(1, 1)), // HalfVertical
        new(new(4, 1), new(1, 1), new(1, 1)), // QuarterHorizontal
        new(new(3, 1), new(1, 1), new(1, 1)), // ThirdHorizontal
        new(new(3, 2), new(1, 1), new(1, 1)), // ThirdHorizontalHalfVertical
        new(new(1, 4), new(1, 1), new(1, 1)), // QuarterVertical
        new(new(4, 2), new(1, 1), new(1, 1)), // QuarterHorizontalHalfVertical
        new(new(2, 4), new(1, 1), new(1, 1)), // HalfHorizontalQuarterVertical
        new(new(1, 3), new(1, 1), new(1, 1)), // ThirdVertical
        new(new(2, 3), new(1, 1), new(1, 1)), // HalfHorizontalThirdVertical
    ];

    internal static JpegSamplingFactors FromSubsampling(ChromaSubsampling subsampling)
    {
        if ((uint)subsampling >= (uint)Presets.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(subsampling));
        }

        return Presets[(int)subsampling];
    }
}
