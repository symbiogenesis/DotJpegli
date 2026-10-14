namespace DotJpegli;

/// <summary>Explicit values to synchronize through JpegliMetadata.Reconcile.</summary>
public sealed record JpegliMetadataEdits
{
    /// <summary>
    /// Exif/TIFF orientation from 1 to 8. Defaults to 1 (normal). Describes the supplied
    /// pixels; changing this value does not rotate or mirror them.
    /// </summary>
    public int Orientation { get; init; } = 1;

    /// <summary>
    /// One artist, synchronized to Exif Artist, XMP dc:creator and IPTC By-line.
    /// Null preserves existing values; empty removes them. Must be ASCII without NUL
    /// and fit IPTC's 32-byte limit. Use the individual editors for Unicode metadata.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Copyright notice, synchronized to Exif Copyright, XMP dc:rights x-default and
    /// IPTC CopyrightNotice. Null preserves existing values; empty removes them.
    /// Must be ASCII without NUL and fit IPTC's 128-byte limit.
    /// </summary>
    public string? Copyright { get; init; }

    /// <summary>
    /// Description, synchronized to Exif ImageDescription, XMP dc:description x-default
    /// and IPTC Caption-Abstract. Null preserves existing values; empty removes them.
    /// Must be ASCII without NUL and fit IPTC's 2,000-byte limit.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional Exif/XMP ColorSpace: 1 for sRGB or 65535 for uncalibrated.
    /// Null preserves existing values. Choose a value describing the stored samples;
    /// the helper does not infer it from ICC bytes or transform pixels.
    /// </summary>
    public int? ExifColorSpace { get; init; }
}
