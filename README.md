# DotJpegli

A fully managed C# JPEG encoder targeting .NET 10. This is an experimental incremental
port of [Google Jpegli](https://github.com/google/jpegli), with unsafe compilation
disabled and no native binaries, P/Invoke, pointer code, `Unsafe` APIs, or runtime
NuGet dependencies.

The source is pinned to
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20),
retrieved on October 12, 2026. This port uses Google's BSD-licensed C++
implementation directly.

## Encoding

```csharp
using DotJpegli;

// Gamma-encoded eight-bit sRGB pixels, interleaved R, G, B.
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    Subsampling = ChromaSubsampling.Quarter,
    OptimizeHuffmanTables = true,
    AdaptiveQuantization = true
});
File.WriteAllBytes("output.jpg", jpeg);
```

`PixelLayout` also supports BGR24, RGBA32, BGRA32, Gray8 and YCbCr24. Alpha is ignored;
composite translucent pixels before encoding. Use `stride` for row padding. The
last row needs only its pixel bytes, without trailing padding. Negative strides
are not supported. The input must contain every row; extra trailing bytes are
allowed.

`Subsampling` selects the color component sampling:

| Setting | Ratio | Chroma sample area | MCU size |
| --- | --- | --- | --- |
| `None` | 4:4:4 | 1x1 pixels | 8x8 pixels |
| `HalfHorizontal` | 4:2:2 | 2x1 pixels | 16x8 pixels |
| `Quarter` (default) | 4:2:0 | 2x2 pixels | 16x16 pixels |
| `HalfVertical` | 4:4:0 | 1x2 pixels | 8x16 pixels |
| `QuarterHorizontal` | 4:1:1 | 4x1 pixels | 32x8 pixels |
| `ThirdHorizontal` | — | 3x1 pixels | 24x8 pixels |
| `ThirdHorizontalHalfVertical` | — | 3x2 pixels | 24x16 pixels |
| `QuarterVertical` | — | 1x4 pixels | 8x32 pixels |
| `QuarterHorizontalHalfVertical` | 4:1:0 | 4x2 pixels | 32x16 pixels |
| `HalfHorizontalQuarterVertical` | — | 2x4 pixels | 16x32 pixels |
| `ThirdVertical` | — | 1x3 pixels | 8x24 pixels |
| `HalfHorizontalThirdVertical` | — | 2x3 pixels | 16x24 pixels |

Grayscale always uses one sample per pixel and 8x8 MCUs. All color modes use
upstream scalar box averaging after optional smoothing, with replicated image
edges. Only 4:2:0 applies upstream's specialized quantization-table compensation;
all other modes use the same perceptual tables as 4:4:4 and 4:2:2.
The less common modes are identified by their sample areas where the usual
4:a:b notation is ambiguous. Preset color MCUs contain at most ten blocks,
including one block for each chroma component.

The three-row modes use upstream's dedicated `Downsample1x3` and `Downsample2x3`
kernels: average horizontally first, then sum the three rows from top to bottom
and multiply by `1f / 3f`. The pinned upstream selector routes vertical factor
three to its two-row kernels instead. DotJpegli uses all three rows as the sample
area specifies; these modes therefore differ from that pinned selector's output.

Set `SamplingFactors` to override the `Subsampling` preset with separate Y, Cb
and Cr factors. Each factor is the number of eight-by-eight component blocks in
one color MCU, rather than a sample area. For example, this keeps luma at full
resolution, averages Cb over 1x2 pixels and Cr over 2x2 pixels:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    SamplingFactors = new(
        Luma: new(2, 2),
        Cb: new(2, 1),
        Cr: new(1, 1))
});
```

Factors must be 1..4. The largest horizontal and vertical factors across Y, Cb
and Cr determine the MCU size; the maxima may belong to different components.
Each component factor must divide its corresponding maximum. A component's
sample area is `maxHorizontal / component.Horizontal` by
`maxVertical / component.Vertical`. The sum of `Horizontal * Vertical` over all
three components must be at most ten. Invalid factors are rejected before output,
including for grayscale input; valid color settings are ignored for grayscale.
The `Subsampling` enum must still be valid when overridden. Null `SamplingFactors`
uses the preset and preserves its JPEG bytes.

Luma may have smaller factors than Cb or Cr. For example, `SamplingFactors =
new(new(1, 1), new(2, 2), new(1, 1))` averages Y and Cr over 2x2 pixels while
keeping Cb at full resolution in a 16x16 MCU. Subsampled luma automatically
disables adaptive quantization, even when `AdaptiveQuantization` is true,
following the pinned upstream encoder. It reduces brightness detail and may
have limited reader support; the existing presets keep full-resolution luma.

Custom sampling uses the same scalar kernels, smoothing, replicated edges,
dummy-block coding, restart markers and progressive scans as the presets.
Only the exact Y=2x2, Cb=1x1, Cr=1x1 scheme uses upstream's 4:2:0 quantization
compensation. Other schemes, including asymmetric ones with Y=2x2, use the regular
tables. When enabled, adaptive quantization follows the pinned streaming encoder: it indexes
the luma field by the component's block row within the MCU and the block's
full-resolution horizontal position. This also applies during progressive passes.
Combined sample areas use horizontal averaging followed by vertical averaging,
except the specialized 2x2 kernel. Three-row sample areas always include all three
rows, including for subsampled luma; they retain the selector deviation described above.
Unusual sampling schemes may have limited support in some JPEG readers.

The stream overload takes a writable stream as its first argument. It leaves
the stream open and does not seek. Cancellation and I/O failures may leave a
partial JPEG. Pass a `CancellationToken` for cancellation between MCUs. Dimensions
must be 1..65535 and fit `MaxPixels` (100 million by default).

### Higher-precision input

The `ReadOnlySpan<ushort>` and `ReadOnlySpan<float>` overloads accept interleaved
gamma-encoded RGB/grayscale samples, using `PixelComponentLayout.Rgb`, `Bgr`, `Rgba`,
`Bgra` or `Gray`. They also accept full-range JPEG YCbCr samples using `YCbCr`, as
described [below](#direct-ycbcr-input). The default is `Rgb`. For these overloads, `stride` counts source elements,
not bytes; zero means tightly packed rows. The final row needs only its pixel
elements. Alpha, row padding and extra trailing elements are ignored.

```csharp
ushort[] rgb16 = ReadRgb16Samples(); // 0..65535 per component.
byte[] jpeg16 = JpegliEncoder.Encode(rgb16, width, height);

float[] gray = ReadNormalizedGraySamples(); // Finite 0..1 samples.
byte[] jpegFloat = JpegliEncoder.Encode(gray, width, height,
    PixelComponentLayout.Gray, options: new() { Quality = 90 });
```

Sixteen-bit samples are scaled by `1.0 / 257.0` and float samples by 255, following
the pinned Jpegli scalar input reader. Fractional values pass through color conversion,
adaptive analysis, smoothing, sampling and the DCT until quantization, without
first rounding to bytes. Expanding byte samples to `ushort` with `value * 257`
preserves the byte encoder's JPEG output. Float arithmetic can differ near coefficient
rounding boundaries, so `value / 255f` does not promise byte-identical encoding.

By default, consumed float color or grayscale samples must be finite and between 0 and 1;
invalid values are rejected before output is written. Ignored alpha and padding
need not satisfy that range. This is narrower than upstream Jpegli's float input,
which also permits out-of-gamut values. These overloads do not linearize samples,
tone-map HDR, or apply color profiles. Supply gamma-encoded values in the color
space described by any attached ICC profile. Typed spans contain native numeric
values; deserialize byte-oriented source data with its correct endianness first.

The output remains a lossy eight-bit JPEG. Higher-precision input retains more
information before coefficient quantization; it does not enable 12-bit JPEG or
lossless encoding. Stream behavior, cancellation, pixel limits and source lifetime
requirements are the same as for byte input. Samples are read on demand with
bounded working storage; no full-image conversion buffer is allocated. Float
validation adds one input pass, with cancellation checks between rows.

### Out-of-range floating-point input

Set `AllowOutOfRangeFloatSamples = true` to retain gamma-encoded RGB, grayscale
or direct YCbCr samples outside the nominal 0..1 range until quantization:

```csharp
byte[] jpeg = JpegliEncoder.Encode(floatPixels, width, height, options: new()
{
    Quality = 85,
    AllowOutOfRangeFloatSamples = true
});
```

The encoder multiplies samples by 255 without clipping them to 0..255, following
the pinned upstream input reader. NaN, infinities and samples whose scaling
overflows are rejected. Alpha, padding and trailing storage remain ignored.
The option has no effect on byte or 16-bit input, and normalized float input
retains its existing JPEG bytes.

If any consumed float is outside 0..1, an additional pass checks the complete
image before writing output. Analysis and DCT computations must remain finite;
rounded quantized DC coefficients must be -1024..1023 and AC coefficients
-1023..1023. These bounds follow eight-bit JPEG's signed coefficient precision
and Huffman categories in [ITU T.81, F.1.1.4 and F.1.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).
They are deliberately stricter than upstream's coefficient storage. The check
uses full coefficients, including when progressive scans initially defer bits,
and bounded row/block storage instead of a full-image coefficient buffer.
Cancellation is checked between rows and MCUs during validation.

Acceptance depends on quality, smoothing and sampling. For example, a constant
slightly negative grayscale image may fit after coarse quantization yet exceed
the DC limit at quality 100. Unencodable input throws an `ArgumentException`
with `ParamName` equal to `pixels` before any output, with the same acceptance
across baseline/progressive and Huffman settings. This option adds no tone
mapping, linear-light conversion, HDR JPEG output or color management. Decoders
can clamp reconstructed samples to their output range.

### Direct YCbCr input

Use `PixelLayout.YCbCr24` for eight-bit samples or `PixelComponentLayout.YCbCr`
for 16-bit and float samples already in JPEG's full-range, BT.601-derived YCbCr
space. Supply three interleaved values per pixel in Y, Cb, Cr order, with all
components at the full image resolution. The encoder scales the source values
as above and passes them directly to smoothing, sampling and the DCT, without
an RGB conversion, following the pinned upstream's `JCS_YCbCr` input path.

```csharp
byte[] jpeg = JpegliEncoder.Encode(ycbcrPixels, width, height,
    PixelLayout.YCbCr24, options: new() { Subsampling = ChromaSubsampling.None });
```

Byte samples span 0..255, with neutral chroma at 128. Sixteen-bit samples span
0..65535, with neutral chroma at 32896 (`128 * 257`). Float samples must be
finite and, by default, in 0..1, with neutral chroma at `128f / 255f`; normalized 0.5 is not
the exact neutral value. Fractional precision is retained until quantization.
These inputs use JPEG's full-range component values, not limited-range video
levels; convert video ranges and other YCbCr matrices before encoding.

This input is neither planar nor already subsampled (for example, NV12, YUV420
or YUY2). `Subsampling` and `SamplingFactors` still select the output sampling;
the default averages chroma over 2x2 pixels. Select `None`, as above, to retain
full-resolution chroma. Adaptive quantization analyzes the supplied Y values
before smoothing and disables itself for subsampled luma as usual. Stride,
pixel limits, float validation, metadata, scan modes and stream behavior have
the same contracts as the other interleaved layouts. An attached ICC profile
should describe the RGB interpretation of the output; the encoder neither
applies a profile transform nor embeds a YCbCr ICC profile automatically.

### Profiles and metadata

Set `ExifData` to raw TIFF bytes for Exif metadata, beginning with the TIFF header.
The encoder prepends `Exif\0\0` and embeds the bytes unchanged in a single APP1
marker. Do not include the Exif signature, APP1 marker or segment length in the
input. An empty value, the default, writes no Exif. The limit is 65,527 bytes.
TIFF structure, tags and offsets are not validated or rewritten; supply data whose
dimensions, orientation, color space, densities and optional thumbnail describe
the encoded image. Orientation tags do not rotate source pixels. Do not change
the backing memory or dispose its owner until encoding returns.

The output retains its leading JFIF APP0 marker, followed by any Exif APP1 marker,
as in [Adobe's JPEG handler](https://github.com/adobe/XMP-Toolkit-SDK/blob/main/XMPFiles/source/FileHandlers/JPEG_Handler.cpp).
This embeds Exif metadata in a JFIF JPEG; it does not claim conformance to the
complete Exif file standard, which requires APP1 immediately after SOI.

Set `XmpPacket` to a serialized XMP packet to embed UTF-8 bytes in a single
standard APP1 marker with the `http://ns.adobe.com/xap/1.0/\0` identifier.
Without extended XMP, null or empty writes no XMP. The limit is 65,504 UTF-8 bytes,
including any packet wrapper or padding supplied by the caller, unless
`AutoPartitionXmp` is enabled as described below. The encoder adds
no BOM, terminator or XML wrapper. Unpaired UTF-16 surrogates are rejected before
output. Standard-only packets remain opaque unless automatic partitioning applies;
supply a complete valid packet. For larger or explicitly partitioned metadata, use `ExtendedXmpPacket`
as described below. The framing follows
[Adobe's XMP storage specification](https://developer.adobe.com/xmp/docs/xmp-specifications/).
The encoder does not reconcile XMP values with Exif, ICC or JFIF metadata.

`ExtendedXmpPacket` accepts a serialized extended XMP tree **without an xpacket
wrapper**. Null or empty writes no extension and preserves standard-only behavior.
The encoder writes the exact UTF-8 bytes, with no added BOM or terminator, in
consecutive APP1 chunks of up to 65,458 bytes. Each chunk includes the uppercase
MD5 identifier of the entire serialization, its unsigned 32-bit byte length, and
its byte offset in network byte order, following Adobe's
[extended XMP storage format](https://github.com/adobe/XMP-Toolkit-SDK/blob/main/docs/XMPSpecificationPart3.pdf).
MD5 identifies the metadata for this format; it does not authenticate it.
UTF-8 characters may span chunks. The total encoded length must fit an unsigned
32-bit integer; there is no 255-chunk limit as there is for ICC profiles.

When `XmpPacket` is null or empty, a minimal standard packet is generated with
the required `xmpNote:HasExtendedXMP` digest link. This keeps all caller-supplied
properties in the extension; readers supporting only standard XMP will see the
link alone. For example:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ExtendedXmpPacket = File.ReadAllText("extended-xmp.xml")
});
```

To keep important properties in the standard packet, partition metadata yourself
and also supply `XmpPacket`, limited to 65,504 UTF-8 bytes. It is preserved unchanged
and must contain exactly one top-level `HasExtendedXMP` property in the
`http://ns.adobe.com/xmp/note/` namespace, equal to the uppercase MD5 of the extended
packet's exact UTF-8 bytes. Supported forms are an attribute or a plain-text child
of a top-level `rdf:Description`; namespace prefixes may vary. The standard root
must be `x:xmpmeta` in `adobe:ns:meta/` or `rdf:RDF` in the RDF namespace. Only this
combination parses standard XML to verify the link; malformed XML, DTDs, missing,
duplicate, structured or mismatched links are rejected before output. Standard-only
packets retain their opaque behavior unless automatic partitioning is enabled for
an oversized packet. Manually supplied packets are not partitioned or rewritten,
and extended XML, RDF and schemas are not parsed or validated. Supply
valid XMP partitions with consistent properties. Unpaired UTF-16 surrogates in
either serialization are rejected before output.

Extension encoding and digest preparation use bounded working storage, with a
cancellation check between chunks, before any output is written. Output also checks
cancellation between APP1 chunks. Neither phase allocates a complete UTF-8 copy
of the extension. Cancellation or an I/O failure during output can leave a partial
JPEG and leaves the destination stream open.

Set `AutoPartitionXmp = true` to accept an `XmpPacket` larger than 65,504 UTF-8
bytes without manually constructing its extended-XMP link:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    XmpPacket = File.ReadAllText("metadata.xmp"),
    AutoPartitionXmp = true
});
```

The default is false. Packets that fit the standard marker retain their exact
bytes, including opaque packets; the limit counts UTF-8 bytes rather than characters.
Oversized packets move all properties together into the extension. The generated
standard packet contains only the digest link, so readers supporting only standard
XMP will not see the properties. Use `StandardXmpProperties` or explicit standard
and extended packets to keep selected properties visible to those readers. A nonempty `ExtendedXmpPacket`
cannot be combined with automatic partitioning.

Automatic partitioning parses oversized XML with DTDs prohibited. The root must
be `x:xmpmeta` containing exactly one `rdf:RDF`, or `rdf:RDF` itself, with one or
more direct `rdf:Description` children. Their `rdf:about` must be absent or empty;
`rdf:ID` and `rdf:nodeID` are not supported. Existing `xmpNote:HasExtendedXMP`
attributes or elements and nested `xpacket` instructions are rejected. The XML
root is reserialized without the document declaration, outer packet wrapper or
document padding. Namespaces, property values, arrays, structures and inherited
XML attributes remain together; XML spelling such as quotes and entity references
may change. This checks the envelope, not complete RDF or schema semantics;
supply valid XMP. Parsing and reserialization use storage proportional to the
packet size. Subsequent hashing and APP1 output retain bounded chunk buffers,
with cancellation checks during parsing and between chunks. Validation finishes
before JPEG output begins.

Set `StandardXmpProperties` alongside `AutoPartitionXmp` to keep chosen top-level
properties visible to readers that only support standard XMP:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    XmpPacket = File.ReadAllText("metadata.xmp"),
    AutoPartitionXmp = true,
    StandardXmpProperties = new XmpPropertyName[]
    {
        new("http://purl.org/dc/elements/1.1/", "title"),
        new("http://ns.adobe.com/xap/1.0/", "Rating")
    }
});
```

Names match by namespace URI and local name, independently of XML prefixes.
Every matching top-level attribute or child element of each `rdf:Description`
moves into the standard packet; remaining properties stay in the extension.
Complete property subtrees, including arrays, structures and qualifiers, move
together. Namespace declarations and inherited `xml:lang`, `xml:base` and
`xml:space` remain on both envelopes. The generated standard envelope omits
formatting whitespace, comments and processing instructions outside properties;
the extension retains those nodes. The standard packet also gets exactly one
digest link identifying the resulting extension. This follows Adobe's
[top-level property partition model](https://github.com/adobe/XMP-Toolkit-SDK/blob/main/XMPCore/source/XMPUtils.cpp).

Null or empty selections preserve the existing automatic behavior. Duplicate
names have no effect and absent names are ignored. Nonempty selections require
`AutoPartitionXmp = true`, even with a small or absent packet. Smaller packets
still retain their exact original bytes. Names require a nonempty namespace URI
and an XML NCName local name; RDF, XML, XMLNS and `adobe:ns:meta/` envelope
namespaces and the `xmpNote:HasExtendedXMP` control property cannot be selected.
Do not mutate the selection list until encoding returns.

The selected properties, their inherited envelope attributes and the added
digest link must fit in 65,504 UTF-8 bytes. Larger selections throw an
`ArgumentOutOfRangeException` for `StandardXmpProperties` before any JPEG output;
properties are never silently dropped or split. An empty-property extension
envelope may be written if every property is selected. The existing XML envelope
validation applies; this does not validate complete RDF graphs or XMP schemas.

Set `IccProfile` to raw ICC profile bytes to embed them unchanged in numbered
APP2 markers. An empty profile, the default, writes no ICC markers. The encoder
does not parse the profile, apply a color transform, or provide a built-in profile;
use a profile describing the source RGB or grayscale values. Do not change its
backing memory or dispose its owner until encoding returns. Profiles can contain
at most 16,707,345 bytes (255 chunks of up to 65,519 bytes), following
[ICC.1 Annex B.5](https://www.color.org/ICC1V42old.pdf).

`Comment` writes one JPEG COM marker containing UTF-8 bytes without a terminator.
Null or empty writes no comment. Its UTF-8 length can be at most 65,533 bytes;
non-ASCII characters can consume several bytes each. JPEG does not define a COM
character encoding, so readers may display non-ASCII comments differently.

`DensityUnit`, `HorizontalDensity`, and `VerticalDensity` configure the JFIF
header. Each density must be 1..65535. `Unspecified`, the default, expresses
the pixel aspect ratio; both values default to 1. `DotsPerInch` and
`DotsPerCentimeter` express physical pixel densities, as defined by
[JFIF](https://www.w3.org/Graphics/JPEG/jfif3.pdf). These fields do not resize pixels.

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ExifData = File.ReadAllBytes("exif-tiff.bin"),
    XmpPacket = File.ReadAllText("metadata.xmp"),
    IccProfile = File.ReadAllBytes("srgb.icc"),
    Comment = "Photo export",
    DensityUnit = JpegDensityUnit.DotsPerInch,
    HorizontalDensity = 300,
    VerticalDensity = 300
});
```

Metadata is written after the mandatory JFIF marker in this order: Exif APP1,
standard XMP APP1, extended XMP APP1 chunks, ICC APP2 chunks and COM, followed by
the coding tables and scans.
Defaults preserve the previous JPEG bytes. Metadata does not change coefficients
or decoded sample values. Viewers may interpret orientation, profiles and other
metadata when displaying the image.
Invalid metadata options are rejected before writing output. Cancellation is
checked between metadata segments as well as between MCUs; cancellation or an
I/O failure during metadata output can leave a partial JPEG and leaves the stream open.

### Compression options

Set `RestartInterval` to 1..65535 to insert restart markers after that many
minimum coded units (MCUs); zero, the default, disables them. MCUs span 8x8 pixels
for grayscale and 4:4:4; for other color modes, multiply each chroma sample area
dimension in the table above by eight. For example, 4:1:0 uses 32x16 pixels.
With custom factors, MCUs span `8 * maxHorizontal` by `8 * maxVertical` pixels,
using the largest corresponding factors across all three components.
Partial edge MCUs
count toward the interval, which can cross row boundaries. Restart markers
reset DC prediction and align the entropy stream between intervals, adding a
small amount of output overhead without changing the decoded image. For example,
`new JpegliEncoderOptions { RestartInterval = 64 }` restarts every 64 MCUs.

Set `Progressive = true` to write a progressive JPEG: an initial DC scan provides
a coarse preview, followed by one full-precision AC scan per component. This
uses two scans for grayscale and four for color, with the same final quantized
coefficients as baseline encoding. The default remains baseline sequential JPEG.
Set `ProgressiveRefinementBits` to 1..10 with `Progressive = true` to deliver
that many low coefficient bits in later refinement scans. For example,
`new JpegliEncoderOptions { Progressive = true, ProgressiveRefinementBits = 2 }`
writes initial scans with two bits deferred, then two rounds of DC and component
AC refinement. With `N` refinement bits this produces `2 * (N + 1)` grayscale scans
or `4 * (N + 1)` color scans, preserving the final quantized coefficients and pixels.
The default, zero, keeps the original full-precision initial scans. More refinement
rounds require more encoding passes and CPU time. This scan script does not reproduce
upstream Jpegli's default script or promise smaller files.

With `OptimizeHuffmanTables = true` (the default), progressive initial and refinement
AC scans combine consecutive block endings into end-of-block (EOB) runs,
up to 32,767 blocks per run. Initial scans finish runs before nonzero AC
coefficients; refinement scans finish them before newly significant coefficients.
Both finish runs at restart markers and scan boundaries. With optimization disabled,
the standard Huffman tables use only single-block EOB symbols. Aggregation changes
the entropy coding, preserving the quantized coefficients and decoded pixels. AC refinement scans
buffer up to 255 correction bits in fixed storage, matching the pinned upstream
token limit and flushing a run before another block would exceed that limit.
Buffered correction bits follow the run symbol and its length bits in block order.
DC refinement uses raw bits.

In progressive AC scans, a single component block counts as one MCU, so
`RestartInterval` can produce more markers than in the DC scan. Restart counting
and the RST0..RST7 sequence begin again in each scan.

Spatial adaptive quantization is enabled by default. It analyzes luma texture,
brightness and neighboring regions to adjust coefficient zeroing thresholds.
Set `AdaptiveQuantization = false` to use uniform quantization. Subsampled luma
also disables adaptive analysis automatically, matching upstream. Upstream sampling
arithmetic and dummy-block coding apply to both modes; partially filled MCUs can
produce different JPEG bytes from earlier port versions.

Set `SmoothingFactor` to 1..100 to apply upstream's optional 3x3 input filter.
The default, zero, disables smoothing. For example,
`new JpegliEncoderOptions { SmoothingFactor = 25 }` mildly softens the input;
higher values suppress more noise and fine detail. The filter operates on
converted Y/Cb/Cr (or grayscale) samples before chroma subsampling. Each of the
eight neighbors receives weight `SmoothingFactor / 1024`, with the remaining
weight on the center sample. Original image edges are replicated before
filtering, including partially filled blocks. Adaptive quantization analyzes
the original, unsmoothed luma, matching upstream. Both baseline and progressive
encoding recompute filtered samples on demand with fixed block storage.

Baseline Huffman optimization makes two passes over the supplied pixels, keeping only
block buffers, bounded rows of spatial-analysis data and symbol histograms
rather than a full image of coefficients or an image-sized adaptive map.
Keep the input unchanged until encoding returns. The byte-array overload also
holds the encoded JPEG in memory; use the stream overload for larger outputs.
Progressive encoding recomputes blocks in each scan's traversal order, using
additional passes and CPU time to retain bounded working memory. Optimized
progressive output collects frequencies from the initial DC and all AC scans before
writing them. Neither mode retains an image-sized coefficient buffer.

## Port status

Implemented: baseline sequential and optional spectral-selection and successive-approximation progressive
JPEG/JFIF; RGB, grayscale and direct interleaved full-range YCbCr input; 4:4:4,
4:2:2, 4:2:0, 4:4:0, 4:1:1 and 4:1:0, plus 3x1, 3x2, 1x4, 2x4, 1x3 and 2x3 chroma
sample areas, using upstream scalar box sampling with edge replication and empty
dummy-block coding that preserves DC predictors at MCU edges; optional upstream
3x3 input smoothing before sampling; recursive scalar DCT; Jpegli
quality-to-distance and perceptual Y/Cb/Cr quantization tables; scalar adaptive
quantization with local differences, fuzzy erosion, masking, high-frequency and
gamma modulation, quality dampening and frequency-specific coefficient thresholds;
optimized length-limited canonical Huffman coding;
byte stuffing and marker output; optional MCU restart intervals with DRI and
cycling RST markers, including predictor resets during Huffman optimization;
progressive initial and refinement EOB-run aggregation with optimized Huffman tables;
optional DC and AC refinement scans with bounded correction-bit storage.
Raw Exif TIFF embedding, standard and extended UTF-8 XMP packets in APP1,
opt-in automatic partitioning of oversized XMP with selected standard properties, raw ICC profiles
with APP2 chunking, UTF-8 COM comments, and configurable JFIF density fields are
also implemented.
Gamma-encoded 16-bit and normalized floating-point RGB and grayscale input are
supported with fractional samples preserved through quantization.
Direct YCbCr input supports the same three source precisions and downstream options.
Opt-in out-of-range floating-point input retains samples through quantization,
with finite-computation and eight-bit coefficient validation before output.
Custom per-component sampling factors support different Cb and Cr resolutions,
integer sample areas including subsampled luma, and at most ten blocks per MCU.

Pending: fractional sampling ratios,
XYB color encoding and built-in profiles or color management,
structured metadata editing, other metadata, SIMD/parallel paths,
and decoder functionality.
The managed DCT uses standard JPEG coefficient units; upstream's DCT output is
one eighth of those units. This scaling adaptation is accounted for during
quantization.

This version produces valid JPEGs but does **not** claim byte-identical output,
compression parity, or speed parity with Jpegli. Quality 100 is still lossy.
Jpegli quality is not interchangeable with libjpeg's quality scale. Google's
[comparison study](https://arxiv.org/abs/2403.18589) supports advantages over
MozJPEG and libjpeg-turbo in its tested conditions, rather than a universal
"smallest JPEG encoder" ranking.

## Validation

```text
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release
dotnet run --project tests/DotJpegli.Tests -c Release --no-build -- -class DotJpegli.Tests.EncoderSamplingTests
```

The second command is a focused local selection. Full suites run only in the
GitHub Actions workflow, on Windows and Linux; benchmarks also belong on CI and
none are included yet. Do not batch local selections to approximate a full suite.
Tests use xUnit and a separate StbImageSharp decoder; these packages are test-only
and are not part of the encoder or its package dependency graph.

Numerical development probes checked the DCT against an independent cosine
definition. Compiled C++ reference probes matched 120 color and grayscale quantization tables
(7,680 entries); selected native-reference tables are retained as regression
fixtures. Adaptive-analysis and coefficient regressions also use independently compiled
C++ scalar fixtures from the pinned source, including edge padding and saturated
quantization tables. These sampled numerical checks do not establish full encoder
parity. Sampling regressions retain independent scalar C++ fixtures for pairwise
averaging and odd edge padding, and entropy parsing checks dummy-block tokens and
DC predictor continuity. Future parity work should encode the same corpus with this pinned C++ revision and
compare decoded quality at matched sizes. Execute performance benchmarks only
on CI.

Additional 4:4:0 and 4:1:1 regressions use independent scalar C++ fixtures for
vertical and pairwise horizontal averaging, odd edges, block seams and smoothing.
They also check emitted sampling factors, dummy-block DC continuity, restart counts,
coefficient and decoded-pixel parity across scan modes, padded input layouts and
grayscale's independence from color sampling settings.

The 3x1, 3x2, 1x4, 4x2 and 2x4 regressions use independently compiled pinned
scalar C++ fixtures for horizontal-first averaging, intermediate rounding,
four-row pairwise sums, smoothing, block seams and replicated edges. Independent
JPEG parsing checks sampling factors, ten-block MCUs, dummy DC continuity,
restarts and coefficient parity across baseline and progressive refinement;
an independent decoder checks final pixel parity. They also cover padded byte,
16-bit and fractional float inputs. Four-block adaptive-analysis rows preserve
the established two-block spatial field when both use the same padded height.

Three-row sampling regressions use independently compiled scalar C++ fixtures
for the dedicated 1x3 and 2x3 kernels, including intermediate rounding, smoothing,
block seams and replicated edges. They check all three rows contribute, emitted
sampling factors, dummy-block DC continuity, restart markers, coefficient and
decoded-pixel parity across baseline and progressive refinement, and padded byte,
16-bit and fractional float inputs. Adaptive-analysis checks compare three-block
rows with single-block traversal of the same padded image across 24-pixel seams.
These checks cover the dedicated kernels, not the pinned upstream selector.

Custom-factor regressions independently parse component factors, real and dummy
chroma blocks, per-scan restart counts and refinement data. They compare baseline
and progressive coefficients and decoded pixels, verify every preset's byte
compatibility with equivalent explicit factors, and check asymmetric chroma DC
values against an independent color-conversion and box-average oracle. They also
cover typed inputs, padded layouts, grayscale settings and validation before output.
Independent full-image scalar C++ adaptive-field fixtures verify bounded analysis
across custom MCU rows. Emitted chroma AC checks use those fixture strengths to
verify component-local vertical indexing and scaled horizontal field columns,
with cases that distinguish incorrect indexing in baseline and progressive output.

Subsampled-luma regressions check component maxima on different components,
3x3 and 4x4 sample areas, ten-block MCUs, replicated edges, dummy DC continuity,
restart cycling and coefficient parity through ten progressive refinement bits.
Independent double-precision box-average, smoothing and cosine-DCT oracles check
luma DC and AC coefficients. An independent decoder verifies final pixel parity
across scan modes for thirteen sampling schemes. Typed and padded input checks,
grayscale's retained adaptive analysis, automatic disabling of adaptive analysis
for subsampled luma, and fractional-ratio rejection before output are also covered.

Input smoothing regressions compare converted, filtered and subsampled block
bits with independently compiled scalar C++ fixtures, including block seams,
odd edges and padding. An independent grayscale filter and cosine DCT oracle
checks the emitted coefficients. Encoding checks cover scan and Huffman modes,
refinement, restart intervals, padded layouts, alpha handling and cancellation.

Restart regressions independently parse marker definitions and entropy segments
to check MCU boundaries, all-component DC resets, one-bit padding and marker
cycling, and compare decoded pixels with restart markers disabled. The marker
and reset rules follow [ITU T.81, sections B.2.4.4 and E.1.3/E.1.4](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).

Progressive regressions reconstruct every real block from independently parsed
DC and AC scans and compare it with baseline coefficients and decoded pixels.
They check noninterleaved block order, odd edges, per-scan restart counting and
padding, as well as cancellation during later scans and padded input layouts.
EOB-run regressions check run-length extension bits, the 32,767-block limit,
flat-image aggregation and run termination at restart and scan boundaries.
The scan rules follow [ITU T.81, Annex G](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).

Refinement regressions check initial point transforms and later bit planes,
including negative DC coefficients, AC sign bits, correction-bit order around
ZRL and EOB symbols, and optimized frequency collection. An independent scan
parser reconstructs coefficients across refinement rounds and compares them
with baseline output, alongside decoded-pixel, restart, edge, layout and
cancellation checks.
Refinement EOB-run regressions also check correction-bit ordering across blocks,
fixed-buffer flushes, the 32,767-block limit, and restart and scan boundaries.

Metadata regressions independently parse APP0, APP2 and COM segments to check
network-byte-order density fields, ICC sequence/count bytes, chunk boundaries,
the 255-chunk profile limit, raw marker payload bytes and UTF-8 comment lengths.
They also check default byte compatibility, unchanged decoded samples across
baseline and progressive modes, validation before output, nonseekable streams,
cancellation between metadata segments and I/O failures.

APP1 regressions independently check Exif and standard XMP identifiers, raw TIFF
byte preservation, UTF-8 without an added BOM or terminator, maximum segment
lengths, multibyte limits and malformed UTF-16 rejection before output. They also
check combined metadata ordering, unchanged coding data and decoded pixels across
baseline, progressive and refinement modes, grayscale and both Huffman settings,
along with nonseekable streams, cancellation boundaries and I/O failures.

Extended XMP regressions independently reconstruct APP1 chunks and verify exact
UTF-8 bytes, uppercase MD5 links, unsigned network-byte-order lengths and offsets,
full-chunk boundaries, and multibyte characters split across segments. They check
generated standard packets and unchanged caller-supplied packets with namespace
aliases and attribute or element links, reject invalid linkage and malformed input
before output, and cover metadata ordering, unchanged coding data and decoded
pixels, nonseekable streams, per-chunk cancellation and I/O failures.

Automatic XMP regressions check the exact UTF-8 threshold, preserved small packets,
wrapper removal and XML property/namespace preservation. Independent APP1 parsing
reconstructs chunks and verifies their lengths, offsets and MD5 link. They also
cover rejection before output, unchanged coding data and decoded pixels across
scan modes, nonseekable streams, cancellation and I/O failures.

Selective XMP regressions independently reconstruct extension chunks, verify
their MD5 link, and compare top-level property contents across both packets.
They cover selected attributes and structured elements across descriptions,
namespace aliases and inherited XML attributes, duplicate and absent selections,
all-selected metadata, compact envelopes and final UTF-8 size limits. They also
check validation before output, default compatibility, unchanged coding bytes
and decoded pixels across scan modes, nonseekable streams and cancellation.

Higher-precision regressions use independently parsed JPEG coefficients to check
fractional grayscale DC values, compare expanded 16-bit byte inputs with the byte
encoder, and compare baseline and progressive coefficients and decoded pixels.
They cover color and grayscale layouts, element strides, alpha and padding,
smoothing, sampling, adaptive and Huffman modes, restarts, stream behavior, and
float validation before output.

Out-of-range float regressions compare emitted coefficients against independent
constant-block and smoothing/box-average/cosine-DCT oracles, including values
that distinguish retaining source samples from clipping them. They check odd
edges, custom sampling, baseline/progressive/refinement and Huffman parity,
ignored alpha and padding, default compatibility, and rejection before output
for nonfinite samples, scaling overflow and unrepresentable DC/AC coefficients.

Direct YCbCr regressions use independent all-component constant-block DC and
smoothed box-average/cosine-DCT oracles, including custom sampling, subsampled
luma and odd edges. They check 16-bit byte compatibility, fractional samples,
padded strides, validation before output, and coefficient and decoded-pixel
parity across scan, Huffman and restart modes.

`Directory.Build.targets` rejects `AllowUnsafeBlocks=true`, including a command
line override. Library and tests compile with warnings treated as errors.

## License

BSD-3-Clause. See [LICENSE](LICENSE), [LICENSE.jpegli](LICENSE.jpegli) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for upstream provenance and notices.
