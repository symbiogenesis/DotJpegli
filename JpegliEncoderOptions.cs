using System.Text;
using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Reusable settings for the managed JPEG encoder.</summary>
public sealed record JpegliEncoderOptions
{
    /// <summary>
    /// Color space stored in the JPEG. Automatic (the default) writes grayscale
    /// for grayscale input, XYB for scaled XYB input, and YCbCr for other color input,
    /// preserving existing bytes.
    /// Grayscale converts RGB to luminance or uses Y from direct YCbCr input.
    /// YCbCr adds neutral chroma to grayscale input. Rgb keeps RGB components or
    /// replicates grayscale; direct YCbCr-to-RGB conversion is not supported.
    /// RGB output uses R/G/B component identifiers, omits JFIF, and defaults to a shared
    /// Annex K luma quantization table regardless of UseStandardQuantizationTables
    /// and transfer-function hints. It defaults to full resolution, ignoring the
    /// Subsampling preset; custom SamplingFactors map Luma/Cb/Cr to R/G/B.
    /// Adaptive RGB quantization analyzes green and disables itself if green is
    /// subsampled. Xyb converts sRGB or grayscale input using sRGB linearization,
    /// the opsin transform and Jpegli scaling, or accepts already scaled XYB input.
    /// It requires a nonempty IccProfile describing the scaled output channels;
    /// the source is assumed to be sRGB and no source ICC transform is applied.
    /// JpegliIccProfiles.CreateXybProfile provides the built-in output profile.
    /// Direct YCbCr-to-XYB and XYB-to-other conversions are unsupported.
    /// It uses RGB identifiers and defaults to three perceptual XYB tables with
    /// X/Y factors 2x2 and B-minus-Y factors 1x1. Its adaptive analysis uses Y.
    /// RGB/XYB JPEG reader support varies. This does not apply an ICC transform.
    /// </summary>
    public JpegColorSpace ColorSpace { get; init; }

    /// <summary>Jpegli quality, from 0 to 100. This is not the libjpeg quality scale.</summary>
    public float Quality { get; init; } = 85;

    /// <summary>
    /// Optional Jpegli perceptual distance, overriding Quality when set.
    /// Must be finite and greater than zero; lower values request finer quantization.
    /// Null (the default) maps Quality through JpegliEncoder.QualityToDistance.
    /// Quality must remain valid even when overridden. Quantization tables stay
    /// clamped to 1..255 with ForceBaseline, or 1..32767 without it, so extreme distances can produce
    /// the same tables. This is a quantization setting, not a measured or guaranteed
    /// perceptual error; even very small distances remain lossy.
    /// QuantizationTables takes precedence over generated tables when supplied.
    /// </summary>
    public float? Distance { get; init; }

    /// <summary>
    /// Use the conventional JPEG Annex K luma and shared chroma quantization matrices.
    /// False (the default) uses Jpegli's perceptual matrices for grayscale and YCbCr.
    /// Automatic RGB tables use one shared Annex K luma matrix; automatic XYB tables
    /// use its three perceptual matrices. QuantizationTables overrides all generated
    /// matrices. Quality still maps to Jpegli
    /// distance, and an explicit Distance still overrides it; the standard matrices use
    /// the pinned upstream's linear scaling from that distance without 4:2:0 compensation.
    /// Values are clamped to the ForceBaseline range before integer conversion,
    /// including near distance 25 where upstream's conversion can overflow.
    /// Adaptive quantization and
    /// Jpegli's coefficient thresholds remain available; this is not libjpeg byte parity
    /// or a promise of equivalent quality, size or speed.
    /// </summary>
    public bool UseStandardQuantizationTables { get; init; }

    /// <summary>
    /// Limit generated and caller-supplied quantizers to baseline JPEG's 1..255 range.
    /// True (the default) preserves existing bytes. False permits values from 1 to
    /// 32767, following Jpegli's nonbaseline mode. Each table containing a value above
    /// 255 uses 16-bit DQT entries; sequential output then uses extended sequential
    /// SOF1 instead of baseline SOF0. Progressive output retains SOF2. Tables whose
    /// values all fit in eight bits still use eight-bit DQT entries and allow SOF0.
    /// Adaptive YCbCr thresholds infer distance using the selected saturation limit.
    /// Output sample precision remains eight bits; this does not enable lossless or
    /// 12-bit JPEG. Reader support for extended sequential JPEG varies.
    /// </summary>
    public bool ForceBaseline { get; init; } = true;

    /// <summary>
    /// Optional final quantization tables, each with exactly 64 values from 1 to
    /// 255 (or 32767 with ForceBaseline false) in natural row-major frequency order
    /// (not JPEG zigzag order).
    /// Null preserves the automatically generated tables. Grayscale output requires
    /// one table; color output accepts one shared table, two tables with the second
    /// shared by components 1 and 2, or three separate component tables. Components
    /// are Y/Cb/Cr, R/G/B or scaled X/Y/B-minus-Y for the selected output color space.
    /// Values are used unchanged, overriding Quality, Distance and
    /// UseStandardQuantizationTables for table construction, including sampling and
    /// transfer-function rescaling. Those options must still be valid. Adaptive
    /// quantization remains enabled by default and can discard small coefficients;
    /// its YCbCr thresholds are inferred from these tables and the transfer hint.
    /// The list and each table are copied and validated before output. Do not modify
    /// them while encoding starts. The encoder never changes the supplied storage.
    /// </summary>
    public IReadOnlyList<ReadOnlyMemory<ushort>>? QuantizationTables { get; init; }

    /// <summary>
    /// CICP transfer-function code describing the supplied samples, from 0 to 255.
    /// The default, 2 (unspecified), preserves existing output. Codes 16 (PQ) and
    /// 18 (HLG) tune Jpegli's perceptual quantization and adaptive coefficient
    /// thresholds; other codes retain the default tuning. Standard quantization
    /// matrices ignore this hint when constructing tables, but adaptive thresholds
    /// still use it for YCbCr. RGB and XYB output ignore the hint. Caller-supplied
    /// QuantizationTables are used unchanged. This does not transform samples,
    /// tone-map, validate a registered code, or embed color metadata.
    /// Supply any required ICC profile separately.
    /// With InferCicpTransferFunctionFromIccProfile enabled, code 2 permits lookup
    /// from that profile; every other code overrides it.
    /// Output remains a lossy eight-bit JPEG; this hint does not establish HDR conformance.
    /// </summary>
    public int CicpTransferFunction { get; init; } = 2;

    /// <summary>
    /// Look up the CICP transfer-function byte in IccProfile when CicpTransferFunction is 2.
    /// False (the default) keeps the profile independent of quantization. Every explicit
    /// code other than 2 takes precedence. Missing or unreadable CICP data falls back to 2.
    /// Follows the pinned upstream raw-profile helper: requires a complete tag directory,
    /// uses its last cicp entry, and reads a 12-byte cicp payload with declared size at least 12.
    /// Other ICC header fields, tag extent beyond those bytes, reserved bytes and CICP
    /// fields are not validated. This is not ICC conformance validation or color management.
    /// The profile is embedded unchanged; only codes 16/18 affect quantization tuning.
    /// </summary>
    public bool InferCicpTransferFunctionFromIccProfile { get; init; }

    /// <summary>YCbCr chroma sample area relative to luma. Defaults to 4:2:0 (2x2); SamplingFactors overrides it. Ignored for RGB/XYB.</summary>
    public ChromaSubsampling Subsampling { get; init; } = ChromaSubsampling.Quarter;

    /// <summary>
    /// Optional per-component Y, Cb and Cr sampling factors, overriding Subsampling.
    /// Null (the default) uses the preset for YCbCr, full resolution for RGB,
    /// and factors 2x2/2x2/1x1 for scaled XYB.
    /// Factors must be 1..4, with integer sample
    /// areas relative to the largest factors and at most ten blocks per color MCU.
    /// For RGB output, Luma/Cb/Cr map to R/G/B and subsampled green disables adaptive quantization.
    /// For XYB output they map to scaled X/Y/B-minus-Y and subsampled Y disables adaptive quantization.
    /// Subsampled luma disables adaptive quantization for YCbCr, following upstream.
    /// Valid color sampling settings are ignored for grayscale output.
    /// </summary>
    public JpegSamplingFactors? SamplingFactors { get; init; }

    /// <summary>
    /// Allow consumed floating-point samples outside 0..1 without clipping them before quantization.
    /// False (the default) requires normalized samples. Ignored for byte and 16-bit input.
    /// Samples and their scaling to JPEG units must be finite. If any consumed sample is outside
    /// 0..1, a bounded coefficient pass validates the image before output: computations must remain
    /// finite, quantized DC must be -1024..1023 and AC -1023..1023. Unencodable input is rejected
    /// with an ArgumentException for pixels, independently of Huffman or scan settings.
    /// RGB-to-XYB output extends the sRGB transfer with the source sign, clamps biased
    /// opsin absorbance to zero, and requires finite nonlinear computations before output.
    /// Acceptance depends on quality, smoothing and sampling; this does not enable HDR output,
    /// tone mapping or source ICC color management.
    /// </summary>
    public bool AllowOutOfRangeFloatSamples { get; init; }

    /// <summary>
    /// Raw Exif TIFF bytes, starting with the TIFF header, to embed unchanged in one APP1 marker.
    /// Do not include the Exif signature or JPEG marker/length bytes; the encoder adds them.
    /// Empty (the default) omits Exif. At most 65,527 bytes are supported; TIFF data is not validated.
    /// Tags, offsets, thumbnails, orientation and dimensions are neither interpreted nor updated.
    /// For grayscale/YCbCr output the JFIF APP0 marker remains first; RGB omits it.
    /// Do not change the underlying memory or dispose its
    /// owner until encoding returns.
    /// </summary>
    public ReadOnlyMemory<byte> ExifData { get; init; }

    /// <summary>
    /// Serialized XMP packet to embed as UTF-8 in one standard APP1 marker.
    /// Null or empty omits standard-only XMP. At most 65,504 encoded bytes are supported
    /// unless AutoPartitionXmp is enabled.
    /// Supply the complete packet, including any desired packet wrapper. No BOM or terminator is added.
    /// With nonempty ExtendedXmpPacket, a supplied standard packet is parsed only to validate its
    /// digest link. Oversized automatic packets are parsed for partitioning; other standard-only
    /// packets are not parsed. Metadata is not reconciled with Exif, ICC or JFIF fields.
    /// Malformed UTF-16 (unpaired surrogates) is rejected before output is written.
    /// </summary>
    public string? XmpPacket { get; init; }

    /// <summary>
    /// Automatically store an XmpPacket exceeding 65,504 UTF-8 bytes in extended XMP.
    /// False (the default) retains the single-marker limit. Smaller packets are unchanged.
    /// Oversized packets must have an x:xmpmeta or rdf:RDF root with one or more top-level
    /// rdf:Description elements whose rdf:about is empty or absent, without rdf:ID/nodeID.
    /// DTDs, existing HasExtendedXMP links and nested xpacket instructions are rejected.
    /// By default all properties move to the extension; StandardXmpProperties can retain
    /// selected properties alongside the digest link. The XML root is reserialized without its
    /// document declaration, outer packet wrapper or padding. Property values and namespaces are retained; XML spelling
    /// may change. RDF/schema semantics are not validated. Parsing uses storage proportional
    /// to the packet. Cannot be combined with a nonempty ExtendedXmpPacket.
    /// </summary>
    public bool AutoPartitionXmp { get; init; }

    /// <summary>
    /// Top-level properties to retain in standard XMP when an oversized packet is automatically partitioned.
    /// Null or empty (the default) moves all properties into the extension. A nonempty selection requires
    /// AutoPartitionXmp. Names match namespace URI and local name, independently of XML prefixes;
    /// every matching attribute or element on a top-level rdf:Description is retained as a whole.
    /// Absent names are ignored and duplicate selections have no effect. Arrays, structures and qualifiers
    /// are never split. The RDF/XML/envelope namespaces and HasExtendedXMP control property cannot be selected.
    /// Names must have a nonempty namespace URI and an XML NCName local name. The generated standard packet,
    /// including its envelope and digest link, must fit 65,504 UTF-8 bytes or encoding fails before output.
    /// Smaller input packets remain unchanged. An empty-property extension may be written when all
    /// properties are selected. Do not mutate the list until encoding returns.
    /// </summary>
    public IReadOnlyList<XmpPropertyName>? StandardXmpProperties { get; init; }

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
    /// At most 16,707,345 bytes are supported. The profile is not applied to the pixels;
    /// supply a profile describing the output's RGB or grayscale interpretation.
    /// JpegliIccProfiles.CreateSrgbProfile provides an RGB sRGB profile for explicit
    /// assignment to RGB or YCbCr output with an sRGB interpretation, not grayscale.
    /// JpegliIccProfiles.CreateSrgbGrayscaleProfile provides the corresponding
    /// built-in profile for grayscale output with the sRGB transfer function.
    /// CreateDisplayP3Profile describes already Display P3 samples for RGB or
    /// YCbCr output; assigning it does not convert sRGB samples to Display P3.
    /// CreateLinearSrgbProfile and CreateLinearSrgbGrayscaleProfile describe
    /// already linear-light RGB and grayscale samples respectively; assigning
    /// them does not linearize the input.
    /// CreateLinearDisplayP3Profile and CreateLinearRec2020Profile describe
    /// already linear-light RGB samples with the corresponding primaries;
    /// assigning them does not convert the source color space or tone-map it.
    /// XYB output requires a nonempty profile describing Jpegli's scaled XYB channels, rather than an
    /// ordinary RGB profile. JpegliIccProfiles.CreateXybProfile provides a matching
    /// built-in profile; it must be assigned explicitly. The encoder does not validate
    /// an external profile's transform. It remains opaque
    /// unless InferCicpTransferFunctionFromIccProfile enables the narrow CICP lookup.
    /// Do not change the underlying memory or dispose its owner until encoding returns.
    /// </summary>
    public ReadOnlyMemory<byte> IccProfile { get; init; }

    /// <summary>
    /// Raw IPTC-IIM bytes to embed unchanged in one Photoshop APP13 marker, as image resource 0x0404.
    /// Supply only the IIM data, without a Photoshop signature, resource header or JPEG marker bytes.
    /// Empty (the default) omits IPTC. At most 65,506 bytes are supported, including room for odd-length padding.
    /// IIM datasets and their text encoding are not validated, interpreted or rewritten. No IPTC digest is added,
    /// and values are not reconciled with Exif or XMP. Do not change the underlying memory or dispose its
    /// owner until encoding returns.
    /// </summary>
    public ReadOnlyMemory<byte> IptcData { get; init; }

    /// <summary>
    /// Optional JPEG comment, encoded as UTF-8 in a single COM marker without a terminator.
    /// Null or empty omits the comment. The encoded length must not exceed 65,533 bytes.
    /// JPEG does not specify a comment character encoding, so readers may display it differently.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>Units for JFIF densities. The default specifies a pixel aspect ratio. Ignored for RGB output.</summary>
    public JpegDensityUnit DensityUnit { get; init; }

    /// <summary>Horizontal JFIF density, from 1 to 65535. Defaults to 1. Ignored for RGB output.</summary>
    public int HorizontalDensity { get; init; } = 1;

    /// <summary>Vertical JFIF density, from 1 to 65535. Defaults to 1. Ignored for RGB output.</summary>
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
    /// Uses green for RGB output and scaled Y for XYB output. Automatically disabled when custom sampling subsamples
    /// the analyzed component in either direction.
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
    /// per component and optional refinement scans. False (the default) writes sequential JPEG,
    /// using SOF0 or SOF1 according to the emitted quantizers.
    /// Coefficients are recomputed for each scan to keep working memory bounded.
    /// </summary>
    public bool Progressive { get; init; }

    /// <summary>
    /// Optional complete JPEG scan script, overriding Progressive and determining
    /// sequential or progressive output from its first spectral range. Null retains
    /// the existing scan settings and JPEG bytes. Components are zero-based output
    /// indices and must be strictly increasing within each scan. Sequential scans
    /// send 0..63 at full precision, selecting each component exactly once.
    /// Progressive DC scans send position 0; AC scans select one component and a
    /// range of zigzag positions 1..63, after that component's first DC scan.
    /// Each coefficient must be introduced with SuccessiveHigh=0, refined one bit
    /// at a time, and finish at SuccessiveLow=0. Bands and component groups may
    /// change between scans. The script permits at most 252 AC component scans,
    /// following the pinned upstream encoder's context limit.
    /// Cannot be combined with ProgressiveLevel, explicit AC ends/depths, nonzero
    /// ProgressiveRefinementBits, ProgressiveInterleaveDc=false or
    /// SequentialInterleaveComponents=false. Progressive itself is ignored.
    /// The script and all component lists are copied and validated before output;
    /// do not modify them while encoding starts. Each scan recomputes coefficients
    /// using bounded storage, resets entropy/restart state, and uses its own MCU
    /// geometry for RestartRows. Final real coefficients are unchanged.
    /// </summary>
    public IReadOnlyList<JpegScan>? ScanScript { get; init; }

    /// <summary>
    /// Interleave all components in the sequential scan. True (the default)
    /// preserves existing bytes. False writes one complete DC/AC scan per component,
    /// in frame component order, visiting only real blocks in raster order.
    /// Each block is one MCU for restart counting; RestartRows uses the selected
    /// component's real block columns. Grayscale output is unchanged.
    /// Requires sequential output: Progressive must be false unless ProgressiveLevel
    /// is zero. Positive ProgressiveLevel presets cannot be combined with false.
    /// Final coefficients are unchanged; color output requires more encoding passes.
    /// No full-image coefficient buffer is allocated. Reader support can vary.
    /// </summary>
    public bool SequentialInterleaveComponents { get; init; } = true;

    /// <summary>
    /// Optional pinned Jpegli scan preset, overriding Progressive. Null (the default)
    /// preserves the existing scan settings. Zero selects sequential output; one sends DC
    /// at full precision and all AC coefficients with one refinement bit; two or
    /// greater sends AC positions 1..2 at full precision and 3..63 with two refinement
    /// bits. Positions use JPEG zigzag order. Negative values are invalid.
    /// Progressive presets interleave DC only when both maximum output sampling
    /// factors are one; otherwise they send separate component DC scans.
    /// Cannot be combined with explicit ProgressiveAcScanEnds or
    /// ProgressiveAcRefinementBits, nonzero ProgressiveRefinementBits, or
    /// ProgressiveInterleaveDc=false. Other encoding settings still apply.
    /// Final coefficients are unchanged; this does not promise upstream byte parity,
    /// smaller files or faster encoding. Presets recompute coefficients per scan.
    /// </summary>
    public int? ProgressiveLevel { get; init; }

    /// <summary>
    /// Interleave all components in each progressive DC scan. True (the default)
    /// preserves existing output. False requires Progressive and sends separate
    /// component DC scans, visiting only real blocks in raster order, both initially
    /// and during refinement. Each block is one MCU for restart counting.
    /// Grayscale output is unchanged. Final coefficients are unchanged; color
    /// output requires more scans and encoding passes. Combined with AC ends
    /// [2, 63], AC depths [0, 2] and DC depth zero, false matches the pinned
    /// upstream level-2 scan descriptors for subsampled color output.
    /// </summary>
    public bool ProgressiveInterleaveDc { get; init; } = true;

    /// <summary>
    /// Number of coefficient bits delivered in successive refinement scans, from 0 to 10.
    /// Zero (the default) uses full-precision initial scans. A positive value requires
    /// Progressive and adds that many rounds of DC and component AC refinement scans.
    /// ProgressiveAcRefinementBits can override the depth of each AC band without
    /// changing this setting's DC depth.
    /// Final coefficients are unchanged; more rounds require more encoding passes.
    /// </summary>
    public int ProgressiveRefinementBits { get; init; }

    /// <summary>
    /// Optional inclusive ends of progressive AC scans in JPEG zigzag order.
    /// For example, [2, 63] sends positions 1..2 before positions 3..63.
    /// Requires Progressive; ends must be strictly increasing from 1 to 63,
    /// with the final end equal to 63. Null uses one AC scan per component,
    /// preserving existing bytes. Each band is sent for every component before
    /// the next band. Bands use ProgressiveRefinementBits unless overridden by
    /// ProgressiveAcRefinementBits. Restart counting and EOB runs start afresh per scan.
    /// The list is copied and validated before output; do not modify it while
    /// encoding starts. Additional bands require additional encoding passes.
    /// </summary>
    public IReadOnlyList<int>? ProgressiveAcScanEnds { get; init; }

    /// <summary>
    /// Optional number of deferred bits, from 0 to 10, for each progressive AC band.
    /// Requires Progressive and exactly one value per ProgressiveAcScanEnds entry,
    /// or one value when the ends are null. Null (the default) uses
    /// ProgressiveRefinementBits for every band and preserves existing bytes.
    /// This overrides AC depths only; ProgressiveRefinementBits still sets the DC depth.
    /// Initial scans send DC, then each band for every component. Refinement rounds
    /// descend from the largest depth toward zero, sending DC then each band only
    /// when that coefficient range has a bit left to deliver. Each scan resets
    /// restart counting and EOB runs. Final coefficients are unchanged.
    /// For ends [2, 63], depths [0, 2] and DC depth zero, the spectral ranges and
    /// bit schedule match upstream's level 2. Set ProgressiveInterleaveDc to false
    /// when maximum sampling factors exceed one to match its separate DC scans.
    /// This does not promise encoder-byte parity.
    /// The list is copied and validated before output; do not modify it while
    /// encoding starts. Larger depths require additional encoding passes.
    /// </summary>
    public IReadOnlyList<int>? ProgressiveAcRefinementBits { get; init; }

    /// <summary>
    /// Number of minimum coded units (MCUs) between restart markers, from 0 to 65535.
    /// Zero disables restart markers. An MCU is 8x8 pixels for grayscale or 4:4:4,
    /// otherwise eight times the horizontal and vertical chroma sample area dimensions,
    /// including partial edge units. For example, 4:2:0 uses 16x16 and 4:1:0 uses 32x16.
    /// Custom factors use eight times the largest horizontal and vertical component factors.
    /// In separate sequential scans, progressive AC scans and noninterleaved DC scans,
    /// each component block is one MCU. The interval
    /// and restart marker sequence start afresh in every scan.
    /// Positive RestartRows overrides this interval; this value must still be valid.
    /// </summary>
    public int RestartInterval { get; init; }

    /// <summary>
    /// Restart after this many MCU rows in each scan. Zero (the default) uses
    /// RestartInterval and preserves existing bytes. Positive values override
    /// RestartInterval, which must still be valid. Negative values are rejected.
    /// Each scan's interval is its MCU columns times this value, capped at 65535.
    /// Interleaved scans use the frame MCU width; single-component scans use
    /// that component's real block columns. Rows are scan rows, rather
    /// than source pixel rows, so their height depends on sampling and scan type.
    /// DRI markers update the interval when it changes between scans, and the
    /// restart marker sequence starts afresh in every scan. Very large values
    /// can reach the cap and therefore stop aligning with complete rows.
    /// </summary>
    public int RestartRows { get; init; }

    /// <summary>Maximum accepted pixel count. Override deliberately for larger images.</summary>
    public long MaxPixels { get; init; } = 100_000_000;

    internal int ResolveRestartInterval(int mcusPerRow) => RestartRows == 0
        ? RestartInterval : (int)Math.Min((long)RestartRows * mcusPerRow, 65535);

    internal ushort[][]? PrepareQuantizationTables(int componentCount)
    {
        if (QuantizationTables is not { } tables)
            return null;
        int count = tables.Count;
        if (count is < 1 or > 3 || (componentCount == 1 && count != 1))
            throw new ArgumentException("Supply one quantization table for grayscale, or one to three for color output.",
                nameof(QuantizationTables));
        var snapshot = new ushort[count][];
        for (int index = 0; index < count; index++)
        {
            ReadOnlyMemory<ushort> table = tables[index];
            if (table.Length != 64)
                throw new ArgumentException("Each quantization table must contain exactly 64 values in natural frequency order.",
                    nameof(QuantizationTables));
            snapshot[index] = table.ToArray();
            int maximum = ForceBaseline ? 255 : 32767;
            foreach (ushort value in snapshot[index])
                if (value < 1 || value > maximum)
                    throw new ArgumentException($"Quantization values must be between 1 and {maximum}.",
                        nameof(QuantizationTables));
        }
        return snapshot;
    }

    internal JpegliEncoderOptions ResolveProgressiveLevel(JpegSamplingFactors sampling)
    {
        if (ProgressiveLevel is not int level)
            return this;
        if (level == 0)
            return this with { Progressive = false };
        // SetDefaultScanScript, lib/jpegli/encode.cc, pinned revision
        // 031a0077f5799a6041004267fc12b956c1f52a20. All levels >= 2 share a script.
        return this with
        {
            Progressive = true,
            ProgressiveInterleaveDc = sampling.MaxHorizontal == 1 && sampling.MaxVertical == 1,
            ProgressiveAcScanEnds = level == 1 ? [63] : [2, 63],
            ProgressiveAcRefinementBits = level == 1 ? [1] : [0, 2]
        };
    }

    internal int[] PrepareProgressiveAcScanEnds()
    {
        if (ProgressiveAcScanEnds is not { } ends)
            return [63];
        if (!Progressive)
            throw new ArgumentException("AC scan bands require progressive encoding.", nameof(ProgressiveAcScanEnds));
        int count = ends.Count;
        if (count is < 1 or > 63)
            throw new ArgumentException("AC scan ends must contain between 1 and 63 positions.", nameof(ProgressiveAcScanEnds));
        var snapshot = new int[count];
        int previous = 0;
        for (int index = 0; index < count; index++)
        {
            int end = ends[index];
            if (end <= previous || end > 63)
                throw new ArgumentException("AC scan ends must be strictly increasing positions from 1 to 63.", nameof(ProgressiveAcScanEnds));
            snapshot[index] = end;
            previous = end;
        }
        if (previous != 63)
            throw new ArgumentException("The final AC scan end must be 63.", nameof(ProgressiveAcScanEnds));
        return snapshot;
    }

    internal int[] PrepareProgressiveAcRefinementBits(int bandCount)
    {
        var snapshot = new int[bandCount];
        if (ProgressiveAcRefinementBits is not { } depths)
        {
            Array.Fill(snapshot, ProgressiveRefinementBits);
            return snapshot;
        }
        if (!Progressive)
            throw new ArgumentException("AC refinement depths require progressive encoding.", nameof(ProgressiveAcRefinementBits));
        if (depths.Count != bandCount)
            throw new ArgumentException("AC refinement depths must contain one value per AC scan band.", nameof(ProgressiveAcRefinementBits));
        for (int index = 0; index < bandCount; index++)
        {
            int depth = depths[index];
            if (depth is < 0 or > 10)
                throw new ArgumentOutOfRangeException(nameof(ProgressiveAcRefinementBits), "AC refinement depths must be between 0 and 10.");
            snapshot[index] = depth;
        }
        return snapshot;
    }

    internal void Validate()
    {
        if (ScanScript is not null && (ProgressiveLevel is not null
            || ProgressiveAcScanEnds is not null || ProgressiveAcRefinementBits is not null
            || ProgressiveRefinementBits != 0 || !ProgressiveInterleaveDc || !SequentialInterleaveComponents))
            throw new ArgumentException("A scan script cannot be combined with other custom scan settings.",
                nameof(ScanScript));
        bool progressiveOutput = ProgressiveLevel is int requestedLevel ? requestedLevel > 0 : Progressive;
        if (!SequentialInterleaveComponents && progressiveOutput)
            throw new ArgumentException("Separate sequential component scans require sequential encoding.",
                nameof(SequentialInterleaveComponents));
        if (ProgressiveLevel is int level)
        {
            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(ProgressiveLevel), "The progressive level must be nonnegative.");
            if (ProgressiveAcScanEnds is not null || ProgressiveAcRefinementBits is not null
                || ProgressiveRefinementBits != 0 || !ProgressiveInterleaveDc)
                throw new ArgumentException("A progressive level cannot be combined with custom progressive scan settings.",
                    nameof(ProgressiveLevel));
        }
        if (!Enum.IsDefined(ColorSpace))
            throw new ArgumentOutOfRangeException(nameof(ColorSpace));
        if (!float.IsFinite(Quality) || Quality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be finite and between 0 and 100.");
        if (Distance is float distance && (!float.IsFinite(distance) || distance <= 0f))
            throw new ArgumentOutOfRangeException(nameof(Distance), "Distance must be finite and greater than zero.");
        if (CicpTransferFunction is < 0 or > 255)
            throw new ArgumentOutOfRangeException(nameof(CicpTransferFunction), "The CICP transfer-function code must be between 0 and 255.");
        if (!Enum.IsDefined(Subsampling))
            throw new ArgumentOutOfRangeException(nameof(Subsampling));
        SamplingFactors?.Validate();
        if (ExifData.Length > JpegMarkerWriter.MaxExifDataBytes)
            throw new ArgumentOutOfRangeException(nameof(ExifData), "The Exif TIFF data must not exceed 65,527 bytes.");
        if (AutoPartitionXmp && !string.IsNullOrEmpty(ExtendedXmpPacket))
            throw new ArgumentException("Automatic XMP partitioning cannot be combined with an explicit extended packet.",
                nameof(AutoPartitionXmp));
        if (XmpPacket is not null && !AutoPartitionXmp)
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
        if (IptcData.Length > JpegMarkerWriter.MaxIptcDataBytes)
            throw new ArgumentOutOfRangeException(nameof(IptcData), "The IPTC-IIM data must not exceed 65,506 bytes in one Photoshop APP13 marker.");
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
        if (!ProgressiveInterleaveDc && !Progressive)
            throw new ArgumentException("Separate DC scans require progressive encoding.", nameof(ProgressiveInterleaveDc));
        if (RestartInterval is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(RestartInterval), "Restart interval must be between 0 and 65535 MCUs.");
        if (RestartRows < 0)
            throw new ArgumentOutOfRangeException(nameof(RestartRows), "Restart rows must be nonnegative.");
        if (MaxPixels <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxPixels));
    }
}
