using System.Text;
using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Reusable settings for the managed JPEG encoder.</summary>
public sealed record JpegliEncoderOptions
{
    /// <summary>Jpegli quality, from 0 to 100. This is not the libjpeg quality scale.</summary>
    public float Quality { get; init; } = 85;

    public ChromaSubsampling Subsampling { get; init; } = ChromaSubsampling.Quarter;

    /// <summary>
    /// Raw ICC profile bytes to embed in APP2 markers. Empty (the default) omits the profile.
    /// At most 16,707,345 bytes are supported. The profile is neither interpreted nor applied
    /// to the pixels; supply a profile describing the input RGB or grayscale values.
    /// Do not change the underlying memory or dispose its owner until encoding returns.
    /// </summary>
    public ReadOnlyMemory<byte> IccProfile { get; init; }

    /// <summary>
    /// Optional JPEG comment, encoded as UTF-8 in a single COM marker without a terminator.
    /// Null or empty omits the comment. The encoded length must not exceed 65,533 bytes.
    /// JPEG does not specify a comment character encoding, so readers may display it differently.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>Units for JFIF densities. The default specifies a pixel aspect ratio.</summary>
    public JpegDensityUnit DensityUnit { get; init; }

    /// <summary>Horizontal JFIF density, from 1 to 65535. Defaults to 1.</summary>
    public int HorizontalDensity { get; init; } = 1;

    /// <summary>Vertical JFIF density, from 1 to 65535. Defaults to 1.</summary>
    public int VerticalDensity { get; init; } = 1;

    /// <summary>
    /// Strength of the upstream 3x3 input smoothing filter, from 0 to 100.
    /// Zero (the default) disables smoothing. Applied to converted components
    /// before chroma subsampling; adaptive analysis still uses the original luma.
    /// Increasing the value softens fine detail as well as noise.
    /// </summary>
    public int SmoothingFactor { get; init; }

    /// <summary>Use Jpegli's spatial analysis to suppress perceptually masked coefficients.</summary>
    public bool AdaptiveQuantization { get; init; } = true;

    /// <summary>
    /// Collect symbol frequencies before writing scans to reduce entropy-coded size.
    /// Progressive initial and refinement AC scans also aggregate end-of-block runs
    /// when enabled, using bounded correction-bit storage for refinement scans.
    /// </summary>
    public bool OptimizeHuffmanTables { get; init; } = true;

    /// <summary>
    /// Write a progressive JPEG with a DC preview followed by an AC scan
    /// per component and optional refinement scans. False (the default) writes baseline JPEG.
    /// Coefficients are recomputed for each scan to keep working memory bounded.
    /// </summary>
    public bool Progressive { get; init; }

    /// <summary>
    /// Number of coefficient bits delivered in successive refinement scans, from 0 to 10.
    /// Zero (the default) uses full-precision initial scans. A positive value requires
    /// Progressive and adds that many rounds of DC and component AC refinement scans.
    /// Final coefficients are unchanged; more rounds require more encoding passes.
    /// </summary>
    public int ProgressiveRefinementBits { get; init; }

    /// <summary>
    /// Number of minimum coded units (MCUs) between restart markers, from 0 to 65535.
    /// Zero disables restart markers. An MCU is 8x8 pixels for grayscale or 4:4:4,
    /// 16x8 for 4:2:2, 16x16 for 4:2:0, 8x16 for 4:4:0, and 32x8 for 4:1:1,
    /// including partial edge units.
    /// In progressive AC scans, each component block is one MCU. The interval
    /// and restart marker sequence start afresh in every scan.
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
        if (IccProfile.Length > JpegMarkerWriter.MaxIccProfileBytes)
            throw new ArgumentOutOfRangeException(nameof(IccProfile), "The ICC profile must fit in at most 255 APP2 markers (16,707,345 bytes).");
        if (Comment is not null && (Comment.Length > JpegMarkerWriter.MaxMarkerPayloadBytes ||
            Encoding.UTF8.GetByteCount(Comment) > JpegMarkerWriter.MaxMarkerPayloadBytes))
            throw new ArgumentOutOfRangeException(nameof(Comment), "The UTF-8 comment must not exceed 65,533 bytes.");
        if (!Enum.IsDefined(DensityUnit))
            throw new ArgumentOutOfRangeException(nameof(DensityUnit));
        if (HorizontalDensity is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(HorizontalDensity), "Horizontal density must be between 1 and 65535.");
        if (VerticalDensity is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(VerticalDensity), "Vertical density must be between 1 and 65535.");
        if (SmoothingFactor is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(SmoothingFactor), "Smoothing factor must be between 0 and 100.");
        if (ProgressiveRefinementBits is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(ProgressiveRefinementBits), "Progressive refinement bits must be between 0 and 10.");
        if (ProgressiveRefinementBits != 0 && !Progressive)
            throw new ArgumentException("Progressive refinement requires progressive encoding.", nameof(ProgressiveRefinementBits));
        if (RestartInterval is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(RestartInterval), "Restart interval must be between 0 and 65535 MCUs.");
        if (MaxPixels <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxPixels));
    }
}
