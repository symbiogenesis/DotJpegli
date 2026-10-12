namespace DotJpegli;

/// <summary>Reusable settings for the initial baseline encoder port.</summary>
public sealed record JpegliEncoderOptions
{
    /// <summary>Jpegli quality, from 0 to 100. This is not the libjpeg quality scale.</summary>
    public float Quality { get; init; } = 85;

    public ChromaSubsampling Subsampling { get; init; } = ChromaSubsampling.Quarter;

    /// <summary>Collect symbol frequencies in a first pass to reduce entropy-coded size.</summary>
    public bool OptimizeHuffmanTables { get; init; } = true;

    /// <summary>Maximum accepted pixel count. Override deliberately for larger images.</summary>
    public long MaxPixels { get; init; } = 100_000_000;

    internal void Validate()
    {
        if (!float.IsFinite(Quality) || Quality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be finite and between 0 and 100.");
        if (!Enum.IsDefined(Subsampling))
            throw new ArgumentOutOfRangeException(nameof(Subsampling));
        if (MaxPixels <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxPixels));
    }
}
