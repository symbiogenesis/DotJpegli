namespace DotJpegli;

/// <summary>Reusable settings for the managed baseline encoder.</summary>
public sealed record JpegliEncoderOptions
{
    /// <summary>Jpegli quality, from 0 to 100. This is not the libjpeg quality scale.</summary>
    public float Quality { get; init; } = 85;

    public ChromaSubsampling Subsampling { get; init; } = ChromaSubsampling.Quarter;

    /// <summary>Use Jpegli's spatial analysis to suppress perceptually masked coefficients.</summary>
    public bool AdaptiveQuantization { get; init; } = true;

    /// <summary>Collect symbol frequencies in a first pass to reduce entropy-coded size.</summary>
    public bool OptimizeHuffmanTables { get; init; } = true;

    /// <summary>
    /// Number of minimum coded units (MCUs) between restart markers, from 0 to 65535.
    /// Zero disables restart markers. An MCU is 8x8 pixels for grayscale or 4:4:4,
    /// 16x8 for 4:2:2, and 16x16 for 4:2:0, including partial edge units.
    /// </summary>
    public int RestartInterval { get; init; }

    /// <summary>Maximum accepted pixel count. Override deliberately for larger images.</summary>
    public long MaxPixels { get; init; } = 100_000_000;

    internal void Validate()
    {
        if (!float.IsFinite(Quality) || Quality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be finite and between 0 and 100.");
        if (!Enum.IsDefined(Subsampling))
            throw new ArgumentOutOfRangeException(nameof(Subsampling));
        if (RestartInterval is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(RestartInterval), "Restart interval must be between 0 and 65535 MCUs.");
        if (MaxPixels <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxPixels));
    }
}
