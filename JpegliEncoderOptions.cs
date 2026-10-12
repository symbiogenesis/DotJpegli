using System.Text;
using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Reusable settings for the managed JPEG encoder.</summary>
public sealed record JpegliEncoderOptions
{
    /// <summary>Jpegli quality, from 0 to 100. This is not the libjpeg quality scale.</summary>
    public float Quality { get; init; } = 85;

    /// <summary>Chroma sample area relative to luma. Defaults to 4:2:0 (2x2); SamplingFactors overrides it.</summary>
    public ChromaSubsampling Subsampling { get; init; } = ChromaSubsampling.Quarter;

    /// <summary>
    /// Optional per-component Y, Cb and Cr sampling factors, overriding Subsampling.
    /// Null (the default) uses the preset. Factors must be 1..4, with integer sample
    /// areas relative to the largest factors and at most ten blocks per color MCU.
    /// Subsampled luma automatically disables adaptive quantization, following upstream.
    /// Valid color sampling settings are ignored for grayscale input.
    /// </summary>
    public JpegSamplingFactors? SamplingFactors { get; init; }

    /// <summary>
    /// Allow consumed floating-point samples outside 0..1 without clipping them before quantization.
    /// False (the default) requires normalized samples. Ignored for byte and 16-bit input.
    /// Samples and their scaling to JPEG units must be finite. If any consumed sample is outside
    /// 0..1, a bounded coefficient pass validates the image before output: computations must remain
    /// finite, quantized DC must be -1024..1023 and AC -1023..1023. Unencodable input is rejected
    /// with an ArgumentException for pixels, independently of Huffman or scan settings.
    /// Acceptance depends on quality, smoothing and sampling; this does not enable HDR output,
    /// tone mapping, linear-light conversion or color management.
    /// </summary>
    public bool AllowOutOfRangeFloatSamples { get; init; }

    /// <summary>
    /// Raw Exif TIFF bytes, starting with the TIFF header, to embed unchanged in one APP1 marker.
    /// Do not include the Exif signature or JPEG marker/length bytes; the encoder adds them.
    /// Empty (the default) omits Exif. At most 65,527 bytes are supported; TIFF data is not validated.
    /// Tags, offsets, thumbnails, orientation and dimensions are neither interpreted nor updated.
    /// The JFIF APP0 marker remains first. Do not change the underlying memory or dispose its
    /// owner until encoding returns.
    /// </summary>
    public ReadOnlyMemory<byte> ExifData { get; init; }

    /// <summary>
    /// Serialized XMP packet to embed as UTF-8 in one standard APP1 marker.
    /// Null or empty omits standard-only XMP. At most 65,504 encoded bytes are supported.
    /// Supply the complete packet, including any desired packet wrapper. No BOM or terminator is added.
    /// With nonempty ExtendedXmpPacket, a supplied standard packet is parsed only to validate its
    /// digest link; otherwise XML is not parsed. Metadata is not reconciled with Exif, ICC or JFIF fields.
    /// Malformed UTF-16 (unpaired surrogates) is rejected before output is written.
    /// </summary>
    public string? XmpPacket { get; init; }

    /// <summary>
    /// Serialized extended XMP, without an xpacket wrapper, to embed as UTF-8 in chunked APP1 markers.
    /// Null or empty (the default) omits extended XMP. The encoded length must fit an unsigned 32-bit integer.
    /// The encoder hashes the exact UTF-8 bytes with the format's MD5 identifier and generates a minimal
    /// standard packet containing xmpNote:HasExtendedXMP when XmpPacket is null or empty.
    /// A supplied XmpPacket is preserved unchanged and must contain exactly one matching uppercase digest
    /// in a top-level RDF Description attribute or plain-text child in http://ns.adobe.com/xmp/note/.
    /// Its root must be x:xmpmeta or rdf:RDF. Invalid links, malformed standard XML, DTDs and unpaired
    /// UTF-16 surrogates are rejected before output. Extended XML/RDF and schemas are not parsed or validated;
    /// callers must partition metadata and supply a valid wrapper-free serialization. No BOM is added.
    /// Encoding and hashing use bounded working storage; cancellation is checked between chunks.
    /// </summary>
    public string? ExtendedXmpPacket { get; init; }

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

    /// <summary>
    /// Use Jpegli's spatial analysis to suppress perceptually masked coefficients.
    /// Automatically disabled when custom sampling subsamples luma in either direction.
    /// </summary>
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
    /// otherwise eight times the horizontal and vertical chroma sample area dimensions,
    /// including partial edge units. For example, 4:2:0 uses 16x16 and 4:1:0 uses 32x16.
    /// Custom factors use eight times the largest horizontal and vertical component factors.
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
        SamplingFactors?.Validate();
        if (ExifData.Length > JpegMarkerWriter.MaxExifDataBytes)
            throw new ArgumentOutOfRangeException(nameof(ExifData), "The Exif TIFF data must not exceed 65,527 bytes.");
        if (XmpPacket is not null)
        {
            if (XmpPacket.Length > JpegMarkerWriter.MaxXmpPacketBytes)
                throw new ArgumentOutOfRangeException(nameof(XmpPacket), "The UTF-8 XMP packet must not exceed 65,504 bytes.");
            try
            {
                if (JpegMarkerWriter.XmpEncoding.GetByteCount(XmpPacket) > JpegMarkerWriter.MaxXmpPacketBytes)
                    throw new ArgumentOutOfRangeException(nameof(XmpPacket), "The UTF-8 XMP packet must not exceed 65,504 bytes.");
            }
            catch (EncoderFallbackException error)
            {
                throw new ArgumentException("The XMP packet must contain well-formed UTF-16.", nameof(XmpPacket), error);
            }
        }
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
