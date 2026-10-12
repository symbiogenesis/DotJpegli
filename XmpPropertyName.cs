namespace DotJpegli;

/// <summary>An XMP property's namespace URI and local name, independent of its XML prefix.</summary>
/// <remarks>
/// Used by JpegliEncoderOptions.StandardXmpProperties to retain complete top-level properties
/// in standard XMP during automatic partitioning. Names are validated when encoding.
/// </remarks>
public readonly record struct XmpPropertyName(string NamespaceUri, string LocalName);
