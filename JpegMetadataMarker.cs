namespace DotJpegli;

/// <summary>Describes one raw JPEG application or comment metadata marker.</summary>
/// <param name="MarkerCode">
/// Marker code without the leading 0xFF: 0xE0..0xEF for APP0..APP15, or 0xFE for COM.
/// Structural JPEG markers are not supported.
/// </param>
/// <param name="Data">
/// Complete payload, including any format-specific identifier, without JPEG marker or length bytes.
/// At most 65,533 bytes are supported. Empty data emits a marker with a two-byte length field.
/// The encoder preserves these bytes without interpreting, splitting or escaping them.
/// Do not change the underlying memory or dispose its owner until encoding returns.
/// </param>
/// <remarks>
/// Use JpegliEncoderOptions.MetadataMarkers to emit these descriptors in order after built-in
/// metadata and before coding tables. Duplicate markers are permitted. The caller is responsible
/// for payload validity and consistency with the encoded image and other metadata. Recognized
/// signatures, such as JFIF, Adobe or ICC, can change how readers interpret the image.
/// </remarks>
public sealed record JpegMetadataMarker(int MarkerCode, ReadOnlyMemory<byte> Data);
