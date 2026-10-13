# DotJpegli

A fully managed C# JPEG encoder targeting .NET 10. This is an experimental incremental
port of the encoder in [Google Jpegli](https://github.com/google/jpegli), with unsafe compilation
disabled and no native binaries, P/Invoke, pointer code, `Unsafe` APIs, or runtime
NuGet dependencies.

DotJpegli's scope is **encoder only**. Implementing or porting a JPEG decoder is
outside the project's scope and is not planned. Generated JPEGs are intended for
existing JPEG decoders; compatibility requirements for optional output modes are
documented below. Port completion is judged on encoder functionality, output
compatibility, quality/compression validation, and performance.

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

`PixelLayout` also supports BGR24, RGBA32, BGRA32, Gray8, YCbCr24 and already scaled
Xyb24 (see [scaled XYB input](#scaled-xyb-input)). Alpha is ignored;
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
including for grayscale output; valid color settings are ignored for grayscale.
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
first rounding to bytes. Explicit [sRGB-to-XYB output](#srgb-to-xyb-output) instead
normalizes directly to 0..1 before its nonlinear transform. Expanding byte samples to `ushort` with `value * 257`
preserves the byte encoder's JPEG output. Float arithmetic can differ near coefficient
rounding boundaries, so `value / 255f` does not promise byte-identical encoding.

By default, consumed float color or grayscale samples must be finite and between 0 and 1;
invalid values are rejected before output is written. Ignored alpha and padding
need not satisfy that range. This is narrower than upstream Jpegli's float input,
which also permits out-of-gamut values. XYB output linearizes sRGB before its
opsin transform; other output modes retain gamma-encoded samples. No HDR tone
mapping or source ICC transform is applied. For conversion to XYB supply sRGB input and
a scaled XYB output profile; other modes use the color space described by the
attached profile. Typed spans contain native numeric
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
mapping, HDR JPEG output or source ICC color management. Explicit XYB output
applies its sRGB linearization and opsin transform as described below. Decoders
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

### Output color space

Set `ColorSpace` to choose the components stored in the JPEG independently of
the source layout. `JpegColorSpace.Automatic`, the default, preserves existing
output: grayscale for grayscale input, XYB for scaled XYB input, and YCbCr for
other color input.

```csharp
byte[] grayJpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ColorSpace = JpegColorSpace.Grayscale
});

byte[] rgbJpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ColorSpace = JpegColorSpace.Rgb,
    Quality = 90
});
```

`Grayscale` uses the existing fractional RGB-to-luminance conversion, or only
the supplied Y component of direct YCbCr input. Unused float Cb/Cr samples are
ignored, including during range validation. Grayscale output always uses 1x1
sampling. `YCbCr` also accepts grayscale input, adding neutral chroma at 128
JPEG sample units. This neutral fill deliberately differs from the pinned
upstream `GrayscaleToYCbCr` helper, which fills chroma with zero.

`Rgb` preserves R/G/B values without a YCbCr conversion; BGR layouts are
reordered and alpha is ignored. Grayscale input replicates its value across
all three components. Direct YCbCr-to-RGB conversion is unsupported and is
rejected before output. All three source precisions retain their existing scaling.

RGB JPEGs use `R/G/B` frame and scan identifiers, by default one shared Annex K luma
quantization table, and one shared pair of Huffman tables, following upstream's
non-XYB RGB path. Quality and distance use the standard-table linear scale;
`UseStandardQuantizationTables` and CICP transfer-function hints have no effect
on RGB output. Adaptive quantization analyzes unsmoothed green, using uniform
0.5 AC bias multipliers and offsets; disabling it uses zero bias.

RGB defaults to full-resolution 1x1 factors and ignores the `Subsampling` preset.
Custom `SamplingFactors` map `Luma`, `Cb` and `Cr` to R, G and B respectively,
with the usual divisibility and ten-block MCU limits. Subsampled green disables
adaptive quantization. Other sampling, smoothing, dummy blocks, restart markers,
progressive scans, refinement and coefficient preflight use the existing paths.
All options are still validated, including ignored sampling presets and JFIF densities.

RGB output omits JFIF APP0 and Adobe APP14; the RGB component identifiers signal
the color space. JFIF density options therefore have no effect. Optional metadata
begins immediately after SOI in its usual order. Some JPEG readers have limited
support for RGB or subsampled RGB JPEGs. This option does not perform color
management or generate an ICC profile; any attached profile
must describe the chosen output interpretation.

### sRGB-to-XYB output

Set `ColorSpace = JpegColorSpace.Xyb` to convert gamma-encoded sRGB or grayscale
input to Jpegli's scaled XYB representation:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ColorSpace = JpegColorSpace.Xyb,
    IccProfile = JpegliIccProfiles.CreateXybProfile(),
    Quality = 90
});
```

This supports byte, 16-bit and float RGB/BGR layouts, including ignored alpha,
and grayscale replicated across the three source channels. The source must use
sRGB primaries, D65 white and the sRGB transfer function. Samples are normalized
directly to 0..1, linearized, transformed through upstream's biased opsin matrix
at its default intensity target of 255, then scaled to X, Y and B-minus-Y.
Fractional values pass to the existing XYB quantization and sampling paths;
conversion is performed on demand without a full-image conversion buffer.

A nonempty matching **scaled XYB output** ICC profile is required. Use
`JpegliIccProfiles.CreateXybProfile()` for the built-in Jpegli profile at the
default intensity target of 255, or supply a matching external profile. It is
embedded unchanged; it describes the stored channels. Arbitrary source ICC
conversion and tone mapping are not provided. Direct YCbCr input
cannot be converted to XYB. `Automatic` continues to select YCbCr for RGB input
and grayscale for grayscale input.

The managed transform uses `MathF.Pow` for the piecewise sRGB transfer and
`MathF.Cbrt` for the opsin roots, retaining upstream's matrix and scale constants.
These differ slightly from the pinned upstream's rational and fast cube-root
approximations and can change coefficient rounding near boundaries. Encoded
channels are not clipped to 0..255. With `AllowOutOfRangeFloatSamples`, the sRGB
transfer preserves each sample's sign and biased absorbance is clamped to zero,
as upstream does. Nonfinite nonlinear computations and unrepresentable JPEG
coefficients are rejected before output. Existing metadata, smoothing, custom
sampling, adaptive analysis, progressive scans, restarts and stream behavior apply.
Readers must apply the matching ICC profile for correct display.

### Scaled XYB input

Use `PixelLayout.Xyb24` or `PixelComponentLayout.Xyb` for samples already
transformed and scaled using Jpegli's XYB representation. Components are
interleaved **scaled X, scaled Y, scaled B-minus-Y**, matching the pinned
upstream `ScaleXYBRow` output. Raw opsin-space X/Y/B values and ordinary RGB
values are not valid inputs for these layouts. These layouts bypass RGB
linearization, the opsin transform and XYB scaling.

```csharp
byte[] jpeg = JpegliEncoder.Encode(scaledXybSamples, width, height,
    PixelComponentLayout.Xyb, options: new()
    {
        IccProfile = JpegliIccProfiles.CreateXybProfile(),
        Quality = 90
    });
```

Here `scaledXybSamples` is a `float[]` containing normalized scaled samples.
Byte and 16-bit layouts represent the same nominal range with 0..255 and
0..65535. Existing element strides, fractional precision, float validation and
optional out-of-range coefficient preflight apply. `Automatic` selects XYB
output; explicit `JpegColorSpace.Xyb` is equivalent. Scaled XYB input cannot
be converted to any other output color space.

A nonempty `IccProfile` is required to describe the stored scaled channels.
Supply the built-in profile from `JpegliIccProfiles.CreateXybProfile()` or a
matching Jpegli XYB profile; an ordinary sRGB profile is unsuitable.
The profile is embedded unchanged and an external profile's transform is not
validated. Correct display requires a reader that applies the profile;
readers ignoring ICC will interpret the scaled channels as RGB.

`CreateXybProfile()` builds the pinned upstream's 720-byte perceptual ICC v4.4
profile in managed code. Every call returns a separate mutable array with
deterministic bytes, including upstream's fixed date and ICC profile ID.
Its forward transform maps scaled X/Y/B-minus-Y to D50 XYZ using a small linear
CLUT, cubic curves and a matrix. The compatibility `B2A0` tag is an identity,
not an inverse transform; this profile is intended for displaying XYB output
and must not be used to convert RGB input to XYB. Profiles remain explicitly
assigned through `IccProfile`; the encoder does not attach one automatically.

XYB JPEGs use `R/G/B` component identifiers, shared Huffman table 0, and by default
three separate perceptual quantization tables with selectors 0/1/2. The matrices and
nonlinear distance scaling come from the pinned upstream XYB mode.
`UseStandardQuantizationTables` and transfer-function hints do not change
XYB quantization. Quality and explicit distance retain their usual precedence
and tables are clamped to the `ForceBaseline` range, 1..255 by default or
1..32767 when false. `QuantizationTables` can override these generated tables
with final values in the selected range.

Default factors are X=2x2, Y=2x2, B-minus-Y=1x1: a 16x16 MCU with nine blocks,
retaining full-resolution X/Y and averaging B-minus-Y over 2x2 pixels. The
`Subsampling` preset is ignored. Custom `SamplingFactors` map `Luma`, `Cb`
and `Cr` to the three scaled channels and retain the existing factor limits.
Adaptive quantization analyzes unsmoothed scaled Y, disables itself when Y is
subsampled, and uses uniform 0.5 AC bias multipliers and offsets when enabled;
disabled analysis uses zero bias. Smoothing, replicated edges, dummy blocks,
progressive refinement, restart markers and metadata use the existing paths.
XYB omits JFIF and Adobe markers, so JFIF densities have no effect. Ignored
options are still validated. This direct-input path does not establish
color-managed visual quality, full encoder parity or HDR conformance.

### Profiles and metadata

Set `ExifData` to raw TIFF bytes for Exif metadata, beginning with the TIFF header.
The encoder prepends `Exif\0\0` and embeds the bytes unchanged in a single APP1
marker. Do not include the Exif signature, APP1 marker or segment length in the
input. An empty value, the default, writes no Exif. The limit is 65,527 bytes.
TIFF structure, tags and offsets are not validated or rewritten; supply data whose
dimensions, orientation, color space, densities and optional thumbnail describe
the encoded image. Orientation tags do not rotate source pixels. Do not change
the backing memory or dispose its owner until encoding returns.

Grayscale and YCbCr output retain their leading JFIF APP0 marker, followed by any Exif APP1 marker,
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
APP2 markers. An empty profile, the default, writes no ICC markers except that
XYB output requires a nonempty matching scaled XYB profile. The profile
is opaque unless the opt-in CICP lookup described below is enabled. Supply a
profile describing the output's RGB, grayscale or scaled XYB interpretation.
Use `JpegliIccProfiles.CreateSrgbProfile()` for a built-in RGB sRGB profile,
`JpegliIccProfiles.CreateDisplayP3Profile()` for RGB Display P3,
`JpegliIccProfiles.CreateSrgbGrayscaleProfile()` for grayscale with the sRGB
transfer function, or `JpegliIccProfiles.CreateXybProfile()` for scaled XYB. These factories return a
fresh mutable array with deterministic bytes and require explicit assignment to
`IccProfile`. The encoder does not apply an attached profile's color transform.
Do not change its backing memory or dispose its owner until encoding returns.
Profiles can contain at most 16,707,345 bytes (255 chunks of up to 65,519 bytes), following
[ICC.1 Annex B.5](https://www.color.org/ICC1V42old.pdf).

Every RGB and grayscale profile factory also has an overload accepting
`IccRenderingIntent.Perceptual`, `RelativeColorimetric`, `Saturation` or
`AbsoluteColorimetric`. Gamma factories take the intent after `gamma`:

```csharp
byte[] profile = JpegliIccProfiles.CreateSrgbProfile(IccRenderingIntent.RelativeColorimetric);
byte[] gammaProfile = JpegliIccProfiles.CreateGammaGrayscaleProfile(2.2, IccRenderingIntent.AbsoluteColorimetric);
```

Existing overloads retain their perceptual intent and exact bytes. The new
overloads use upstream's intent-dependent descriptions, header intent and ICC
MD5 identification. Invalid enum values throw `ArgumentOutOfRangeException`
for `renderingIntent`. Intent is a preference for the reader's color-management
system: the encoder preserves samples, and the profile's colorants, curves,
adaptation and HDR tone/gamut transforms are the same across intents. HDR
profiles retain `A2B0` and the identity compatibility `B2A0`; reader behavior can
vary. `CreateXybProfile()` remains perceptual-only, matching the pinned builder.
Other white-point and transfer-function combinations remain pending.

`CreateSrgbProfile()` provides the pinned Jpegli builder's 504-byte perceptual
ICC v4.4 display profile, with sRGB transfer curves, D65-to-D50 Bradford
adaptation and RGB-to-D50-XYZ colorants. Its `cicp` tag describes sRGB/BT.709
primaries, transfer code 13, an RGB matrix and full range. Opt-in CICP lookup
therefore retains the default non-HDR quantization tuning. Use this RGB profile
for RGB output or YCbCr output whose RGB interpretation is gamma-encoded sRGB.
It is unsuitable for grayscale, scaled XYB or samples in another RGB space.
Attaching it does not convert samples or validate their color space.

`CreateDisplayP3Profile()` provides the pinned builder's 516-byte perceptual
ICC v4.4 RGB display profile for **already Display P3** samples. It uses P3
primaries, the sRGB transfer function, D65-to-D50 Bradford adaptation and
RGB-to-D50-XYZ colorants. Its `cicp` tag contains `[12, 13, 0, 1]`, identifying
P3 primaries with D65 white, the sRGB transfer function, an RGB matrix and full
range. Opt-in CICP lookup retains the default non-HDR quantization tuning:

```csharp
byte[] jpeg = JpegliEncoder.Encode(displayP3Pixels, width, height, options: new()
{
    ColorSpace = JpegColorSpace.Rgb,
    IccProfile = JpegliIccProfiles.CreateDisplayP3Profile()
});
```

The factory returns fresh deterministic storage and requires explicit assignment.
Use it for RGB output or YCbCr output whose RGB interpretation is Display P3.
Attaching it preserves JPEG coding and does not convert sRGB samples to Display P3
or apply gamut mapping. It is unsuitable for grayscale, scaled XYB, linear-light
P3 or DCI-P3 samples. Explicit sRGB-to-XYB conversion continues to require sRGB
input and the scaled XYB output profile. The encoder does not validate that
attached profiles match source or output color spaces.

`CreateSrgbGrayscaleProfile()` provides the pinned builder's 328-byte perceptual
ICC v4.4 grayscale display profile. Its `kTRC` tag describes the sRGB decoding
curve. Following upstream, its media-white tag is D65 while its header uses the
ICC D50 PCS illuminant; it has no chromatic-adaptation or CICP tag. Opt-in CICP
lookup therefore returns unspecified (2) and retains default quantization tuning.
Use it for grayscale output whose samples follow the sRGB transfer function:

```csharp
byte[] jpeg = JpegliEncoder.Encode(grayPixels, width, height, PixelLayout.Gray8,
    options: new() { IccProfile = JpegliIccProfiles.CreateSrgbGrayscaleProfile() });
```

The factory returns fresh deterministic storage and requires explicit assignment.
Attaching the profile leaves sample conversion and JPEG coding unchanged. It does
not make linear, PQ or HLG grayscale samples sRGB, and is unsuitable for color output.

`CreateLinearSrgbProfile()` and `CreateLinearSrgbGrayscaleProfile()` provide
the pinned builder's perceptual ICC v4.4 profiles for **already linear-light**
samples. The 536-byte RGB profile retains sRGB primaries, Bradford adaptation
from D65 to D50, and shared identity transfer curves; its CICP transfer code is 8.
The 328-byte grayscale profile retains D65 media white, a D50 PCS header and an
identity `kTRC`, with no chromatic-adaptation or CICP tag. Both factories return
fresh deterministic arrays for explicit assignment:

```csharp
byte[] jpeg = JpegliEncoder.Encode(linearRgbSamples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        ColorSpace = JpegColorSpace.Rgb,
        IccProfile = JpegliIccProfiles.CreateLinearSrgbProfile()
    });
```

Supply normalized linear-light float samples, or integer samples scaled to the
input type's full range. RGB output stores them directly; YCbCr output applies
the usual matrix to those same values. Use the grayscale profile for grayscale
output. Attaching either profile preserves the samples and JPEG coding; it does
not decode the sRGB transfer function. Explicit XYB conversion still requires
gamma-encoded sRGB input and the scaled XYB profile. Quantization and adaptive
analysis retain their existing tuning; these factories do not establish a
perceptual-quality guarantee for linear-light JPEGs. Opt-in CICP lookup returns
8 for the RGB profile and unspecified (2) for grayscale, both retaining default
quantization tuning.

`CreateGammaSrgbProfile(gamma)`, `CreateGammaDisplayP3Profile(gamma)`,
`CreateGammaRec2020Profile(gamma)` and `CreateGammaGrayscaleProfile(gamma)` provide
perceptual ICC v4.4 display profiles for samples already encoded with a pure
power law. `gamma` is the **decoding exponent**, so `linear = encoded^gamma`;
it must be finite and from 1 to 8192, inclusive. The RGB profiles use sRGB/Rec.709,
Display P3 or Rec.2020 primaries with D65-to-D50 adaptation; the grayscale
profile retains D65 media white and the D50 XYZ PCS header:

```csharp
byte[] jpeg = JpegliEncoder.Encode(gamma22Pixels, width, height, options: new()
{
    IccProfile = JpegliIccProfiles.CreateGammaSrgbProfile(2.2)
});
```

All four factories return fresh deterministic arrays for explicit assignment. RGB
output or corresponding YCbCr output can use the matching RGB profile;
grayscale output uses the grayscale profile. Their 16-byte type-0 parametric curves decode the
power law, with one curve shared by all RGB channels. Curve exponents are rounded
through upstream's float and signed 15.16 fixed-point serialization. A pure gamma
2.2 curve differs from the piecewise sRGB transfer function; use `CreateSrgbProfile()`
for sRGB samples. Attaching a profile preserves JPEG coding and applies no
source conversion. The wide-gamut factories describe samples already using the
selected primaries and power law; they do not convert sRGB to P3 or Rec.2020.
For ordinary Display P3's piecewise sRGB transfer function, use
`CreateDisplayP3Profile()`; for BT.709-transfer Rec.2020, use `CreateRec2020Profile()`.
The pure-gamma Rec.2020 profile does not describe PQ or HLG samples.

The factories port the pinned builder's direct `GAMMA` branch with encoding
exponent `1 / gamma`, without snapping to named transfer functions or rounding
that reciprocal to a decimal grid. Descriptions use upstream's six-significant-digit
reciprocal formatting, independent of the current culture. All four omit the
CICP tag, including at gamma 1 or 2.6; grayscale also omits chromatic adaptation.
Opt-in CICP lookup returns unspecified (2) and preserves default quantization.
Profile lengths vary with the description; gamma 2.2 produces 508-byte RGB and
324-byte grayscale profiles. These factories describe samples with the supplied
power law and do not validate that the source uses it.

`CreateLinearDisplayP3Profile()` and `CreateLinearRec2020Profile()` provide
536-byte perceptual ICC v4.4 RGB display profiles for **already linear-light**
samples with P3 or Rec.2020 primaries and D65 white. Both use Bradford adaptation
to D50, adapted RGB colorants and shared identity transfer curves. Their CICP
tags are `[12, 8, 0, 1]` and `[9, 8, 0, 1]`, respectively. Opt-in CICP lookup
returns 8 and retains default quantization tuning. For example:

```csharp
byte[] jpeg = JpegliEncoder.Encode(linearRec2020Samples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        ColorSpace = JpegColorSpace.Rgb,
        IccProfile = JpegliIccProfiles.CreateLinearRec2020Profile()
    });
```

Each factory returns a fresh deterministic array for explicit attachment to
RGB output or YCbCr output whose RGB interpretation uses the corresponding
linear primaries. Supply normalized float samples or integers scaled to their
full range. Attaching a profile preserves JPEG coding and does not convert
primaries, linearize samples or apply tone mapping. These profiles do not
describe gamma-encoded Display P3, PQ, HLG, grayscale or scaled XYB samples.
The linear P3 description retains upstream's `RGB_D65_DCI_Per_Lin` spelling;
its white point remains D65. Existing sRGB-to-XYB conversion still requires
gamma-encoded sRGB input and the scaled XYB profile.

`CreateRec709Profile()` and `CreateRec2020Profile()` provide 536-byte
perceptual ICC v4.4 RGB display profiles for samples already encoded with
the **BT.709 transfer function** and Rec.709 or Rec.2020 primaries. Both use
D65-to-D50 Bradford adaptation, adapted RGB colorants and shared piecewise
parametric decoding curves. Their CICP tags are `[1, 1, 0, 1]` and `[9, 1, 0, 1]`:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rec2020Samples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        ColorSpace = JpegColorSpace.Rgb,
        IccProfile = JpegliIccProfiles.CreateRec2020Profile()
    });
```

Each factory returns a fresh deterministic array for explicit attachment to
RGB output or YCbCr output with the corresponding RGB interpretation. Supply
full-range samples; expand limited-range video before encoding. The Rec.2020
factory preserves the pinned builder's BT.709 curve and CICP transfer code 1,
with coefficients 1.099, 0.099 and 0.45 and an encoded knee at 0.081. It does
not select CICP transfer codes 14 or 15 or their precision-specific curves.
Opt-in CICP lookup returns 1 and retains default quantization tuning.
Attaching these profiles leaves JPEG coding unchanged and does not convert
primaries, decode the source transfer function or apply tone mapping. Use the
existing profiles for sRGB, linear-light, grayscale or scaled XYB output.

`CreateRec709DisplayP3Profile()` and `CreateSrgbRec2020Profile()` complete two
additional D65/perceptual RGB profile combinations from the pinned builder:

| Factory | RGB primaries | Transfer function | CICP fields |
| --- | --- | --- | --- |
| `CreateRec709DisplayP3Profile()` | Display P3 | BT.709 | `[12, 1, 0, 1]` |
| `CreateSrgbRec2020Profile()` | Rec.2020 | sRGB | `[9, 13, 0, 1]` |

Both produce fresh deterministic 536-byte ICC v4.4 profiles with D65-to-D50
Bradford adaptation, adapted colorants and shared piecewise parametric decoding
curves. Assign them explicitly to `IccProfile` for RGB output or YCbCr output
with the corresponding RGB interpretation. For example:

```csharp
byte[] jpeg = JpegliEncoder.Encode(p3Bt709Samples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        IccProfile = JpegliIccProfiles.CreateRec709DisplayP3Profile()
    });
```

Supply full-range samples already using the selected primaries and transfer
function. Ordinary Display P3 uses the sRGB curve and `CreateDisplayP3Profile()`;
BT.709-transfer Rec.2020 uses `CreateRec2020Profile()`. These combinations preserve
the upstream descriptions `RGB_D65_DCI_Per_709` and `RGB_D65_202_Per_SRG`, respectively.
Opt-in CICP lookup returns 1 or 13 and retains default quantization tuning.
Profile attachment preserves JPEG coding without converting the source,
expanding limited-range video or applying tone mapping.

`CreateDciP3Profile()` provides the pinned builder's perceptual ICC v4.4 RGB
display profile for samples already encoded as **DCI-P3**. It uses P3 primaries,
DCI white at xy `(0.314, 0.351)`, Bradford adaptation to D50 and shared gamma
2.6 decoding curves. Its CICP tag is `[11, 17, 0, 1]`. For example:

```csharp
byte[] jpeg = JpegliEncoder.Encode(dciP3Samples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        ColorSpace = JpegColorSpace.Rgb,
        IccProfile = JpegliIccProfiles.CreateDciP3Profile()
    });
```

The factory returns a fresh deterministic array for explicit attachment to RGB
output or YCbCr output with that RGB interpretation. Supply full-range DCI-P3
samples. Attaching it preserves JPEG coding and does not convert primaries,
white point or transfer function. Display P3 uses D65 white and the sRGB transfer
function; use `CreateDisplayP3Profile()` for those samples. This RGB profile does
not describe cinema XYZ, grayscale or scaled XYB data. Opt-in CICP lookup returns
17 and retains default quantization tuning. The JPEG remains lossy eight-bit
output and does not provide digital-cinema packaging or conformance.

`CreateRec2100PqProfile()` and `CreateRec2100HlgProfile()` provide the pinned
builder's 4,344-byte perceptual ICC v4.4 RGB display profiles for **already
Rec.2100 PQ or HLG** samples with Rec.2020 primaries and D65 white. Their CICP
tags contain `[9, 16, 0, 1]` and `[9, 18, 0, 1]`. For example:

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqSamples, width, height,
    PixelComponentLayout.Rgb, options: new()
    {
        IccProfile = JpegliIccProfiles.CreateRec2100PqProfile(),
        InferCicpTransferFunctionFromIccProfile = true
    });
```

Each factory returns a fresh deterministic array for explicit attachment to RGB
output or YCbCr output with the corresponding RGB interpretation. Supply
full-range samples; expand limited-range video before encoding. Attaching the
profile preserves the source samples. Opt-in CICP lookup selects the existing
PQ or HLG perceptual quantization tuning for YCbCr output; RGB output continues
to use its standard quantization tables. Explicit transfer hints override lookup.

These profiles retain upstream's default three-dimensional ICC tone mapping.
The `A2B0` tag contains a 9x9x9 eight-bit RGB-to-D50-Lab lookup table, with
identity input/output curves. PQ uses upstream's BT.2408 tone mapper from
10,000 to 250 nits; HLG applies its 300-to-80-nit OOTF adjustment. Both apply
upstream's gamut mapping with saturation-preservation weight 0.3. These display
transforms live in the profile and are applied by compatible color-managed
readers, rather than by the JPEG encoder. Readers may instead interpret CICP
directly; rendering varies by reader. The `B2A0` tag is an identity for
reader compatibility, not an inverse transform for source conversion.

The parameterless factories retain perceptual rendering intent and upstream's generic
`RGB_D65_202_Per_PeQ` / `RGB_D65_202_Per_HLG` descriptions. Their fixed display
assumptions and coarse lookup grid do not provide configurable HDR rendering.
The intent overloads select the reader preference described above; relative
colorimetric profiles use upstream's `Rec2100PQ` / `Rec2100HLG` descriptions.
The JPEG remains lossy eight-bit output; embedding an ICC profile does not
establish HDR display or Rec.2100 conformance. These RGB profiles are unsuitable
for grayscale, scaled XYB, or samples using other primaries or transfers.

Four additional factories provide profiles for **already encoded grayscale**
samples, using the pinned builder's grayscale branches:

| Factory | Media white | Decoding curve | Bytes |
| --- | --- | --- | --- |
| `CreateRec709GrayscaleProfile()` | D65 | BT.709 parametric curve | 328 |
| `CreateDciGrayscaleProfile()` | DCI, xy `(0.314, 0.351)` | Gamma 2.6 | 328 |
| `CreateRec2100PqGrayscaleProfile()` | D65 | 64-entry PQ table, normalized to 10,000 nits | 436 |
| `CreateRec2100HlgGrayscaleProfile()` | D65 | 64-entry HLG table | 436 |

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqGraySamples, width, height,
    PixelComponentLayout.Gray, options: new()
    {
        IccProfile = JpegliIccProfiles.CreateRec2100PqGrayscaleProfile(),
        CicpTransferFunction = 16
    });
```

Each call returns fresh deterministic bytes for explicit attachment to grayscale
output. Supply full-range samples using the profile's transfer function and white
point. The profiles retain upstream's source media-white tag and D50 XYZ PCS
header, with no chromatic-adaptation or CICP tag. Opt-in CICP lookup therefore
returns unspecified (2); set `CicpTransferFunction` explicitly, as above, to request
existing PQ/HLG quantization tuning. Attaching the profile alone preserves JPEG
coding and applies no sample conversion or limited-range expansion.

The grayscale PQ/HLG profiles contain transfer tables without tone mapping or
HLG OOTF adjustment. Compatible readers interpolate between the 64 entries;
these grayscale curves differ from the RGB profiles' tone/gamut-mapped Lab LUTs.
DCI grayscale has no RGB primaries and does not describe cinema XYZ. Output
remains lossy eight-bit JPEG; these factories do not establish HDR display,
Rec.2100 or digital-cinema conformance.

Set `IptcData` to raw IPTC-IIM bytes to embed them unchanged in one Photoshop
APP13 marker. Supply only the IIM datasets, without Photoshop or JPEG framing.
Empty (the default) writes no IPTC. The encoder adds `Photoshop 3.0\0` and one
`8BIM` resource with ID `0x0404`, an empty resource name, a network-byte-order
unpadded data length and zero padding when the data length is odd, following
[Adobe's image resource format](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/).
The maximum is 65,506 input bytes, accounting for the framing and even-byte padding;
larger inputs are rejected before output. Data is neither parsed nor transcoded;
supply valid IIM datasets with their intended text encoding. No IPTC digest is
added, and values are not reconciled with XMP or Exif. Do not change the backing
memory or dispose its owner until encoding returns.

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
    IccProfile = JpegliIccProfiles.CreateSrgbProfile(),
    IptcData = File.ReadAllBytes("metadata.iim"),
    Comment = "Photo export",
    DensityUnit = JpegDensityUnit.DotsPerInch,
    HorizontalDensity = 300,
    VerticalDensity = 300
});
```

Metadata is written after JFIF for grayscale/YCbCr, or directly after SOI for RGB,
in this order: Exif APP1,
standard XMP APP1, extended XMP APP1 chunks, ICC APP2 chunks, IPTC APP13 and COM,
followed by the coding tables and scans.
Defaults preserve the previous JPEG bytes. Metadata does not change coefficients
or decoded sample values. Viewers may interpret orientation, profiles and other
metadata when displaying the image.
Invalid metadata options are rejected before writing output. Cancellation is
checked between metadata segments as well as between MCUs; cancellation or an
I/O failure during metadata output can leave a partial JPEG and leaves the stream open.

### Compression options

`Quality` accepts finite values from 0 to 100, including fractional values, and
defaults to 85. Set `Distance` to a finite value greater than zero to control
Jpegli's perceptual quantization distance directly, following upstream's
[`jpegli_set_distance`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/encode.cc).
Lower distances request finer quantization:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Distance = 1.0f,
    Subsampling = ChromaSubsampling.Quarter
});

float distance = JpegliEncoder.QualityToDistance(85); // 1.45f.
```

Null `Distance`, the default, uses the quality mapping and preserves existing
JPEG bytes. A supplied distance overrides `Quality`, which must still be valid.
Using `Distance = JpegliEncoder.QualityToDistance(quality)` produces the same
JPEG as that quality setting, including with adaptive quantization and
progressive scans. The helper applies the pinned upstream polynomial to
fractional quality as well as integers: quality 0 maps to 25, 30 to 6.4,
and 100 to 0.01. The special value at 100 is distinct from the approximately
0.1 distance approached by qualities just below 100.

Direct distance also accepts values outside that quality range, such as 0.001
or 30. Tables are clamped to 1..255 by default, or 1..32767 with `ForceBaseline = false`; tiny distances
can share all-one tables and large distances can share saturated tables.
Distance is a quantization setting, not a measured or guaranteed perceptual
error. Very small distances and quality 100 remain lossy. This option applies
to every input precision and layout, with the same sampling, smoothing, metadata,
scan, stream and cancellation behavior as quality-based encoding.

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

Set `RestartRows` to a positive number to restart after that many MCU rows in
each scan, following the pinned upstream's `restart_in_rows` setting:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ProgressiveLevel = 2,
    RestartRows = 1
});
```

Zero, the default, keeps `RestartInterval` and its existing JPEG bytes. Positive
values override that fixed interval, which must still be valid; negative row
counts are rejected. The effective interval is the scan's MCU columns multiplied
by `RestartRows`, capped at 65,535 with overflow-safe arithmetic. Rows refer to
the current scan rather than source pixel rows: interleaved scans use frame MCUs,
while single-component sequential, DC and AC scans use the component's real block columns
and rows. For a 33-pixel-wide 4:2:0 image with `RestartRows = 1`, interleaved DC
restarts every three MCUs, luma AC every five blocks, and chroma AC every three
blocks. Separate luma DC scans also use five blocks per row. DRI markers update
the interval only when it changes, and RST numbering starts at RST0 in each scan.
Intervals that reach the cap can cease to align with complete rows. The option
applies to all output color spaces, sampling factors, input precisions and scan
scripts, retaining final coefficients, decoded samples and stream behavior.

Set `SequentialInterleaveComponents = false` to write one complete sequential
scan for each output component, in frame order:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    SequentialInterleaveComponents = false,
    RestartRows = 1
});
```

This selects a custom sequential scan script supported by the pinned upstream's
[`ValidateScanScript`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/encode.cc).
Each scan sends DC and all AC coefficients at full precision, visits real
component blocks in raster order, and omits wholly padded MCU blocks. Each block
counts as one MCU for restarts; row-based intervals use that component's real
block columns. DC predictors reset at each scan and restart; RST numbering starts
at RST0 in each scan and advances modulo eight.
The output retains its sequential frame (SOF0 by default, SOF1 for wide quantizers), quantization tables, final
coefficients and decoded samples. Color encoding needs additional passes but
retains bounded working storage. JPEG reader support can vary.

The default `true` preserves existing interleaved JPEG bytes. Grayscale bytes
are unchanged with either value. False requires sequential output and rejects
`Progressive = true` before output, except when `ProgressiveLevel = 0` selects
sequential encoding. Positive progression levels are also rejected. Sampling,
source precision, smoothing, adaptive quantization, custom quantization tables,
metadata and stream behavior retain their usual contracts. This setting selects
a fixed script; `ScanScript` below provides explicit control. Neither promises
upstream byte, size or speed parity.

Set `ScanScript` to supply complete sequential or progressive scans explicitly.
For example, this sends Y separately, then Cb and Cr together:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ScanScript = [new JpegScan([0]), new JpegScan([1, 2])],
    RestartRows = 1
});
```

`JpegScan` contains `Components`, `SpectralStart`, `SpectralEnd`,
`SuccessiveHigh` and `SuccessiveLow`, corresponding to upstream `jpeg_scan_info`.
Component indices are zero-based in output frame order: Y/Cb/Cr, R/G/B or
scaled X/Y/B-minus-Y, or just zero for grayscale. Selectors must be strictly
increasing within a scan; scans may send component groups in any order.
The defaults `0, 63, 0, 0` send a complete sequential scan. Each component
must occur exactly once in a sequential script. The first scan's spectral
range determines sequential SOF0/SOF1 or progressive SOF2 output, overriding
`Progressive`; the two modes cannot be mixed.

Progressive DC scans use spectral range `0..0` and may select several components.
AC scans select one component and an inclusive range of JPEG zigzag positions
`1..63`, after that component's initial DC scan. For example, grayscale can
send two initial AC bands and refine their combined range later:

```csharp
byte[] jpeg = JpegliEncoder.Encode(grayPixels, width, height, PixelLayout.Gray8,
    options: new()
    {
        ScanScript =
        [
            new JpegScan([0], 0, 0),
            new JpegScan([0], 1, 10, 0, 1),
            new JpegScan([0], 11, 63, 0, 1),
            new JpegScan([0], 1, 63, 1, 0)
        ]
    });
```

Each coefficient starts with `SuccessiveHigh = 0` and a deferred bit count
`SuccessiveLow` in `0..10`. Refinement must use the coefficient's previous
low bit as its high bit and deliver exactly the next bit (`low = high - 1`).
Every coefficient of every component must finish at low bit zero. Missing
ranges, repeated initial scans, skipped bits, AC before DC and incomplete
scripts are rejected with `ArgumentException` for `ScanScript` before output.
This follows the pinned upstream's `ValidateScanScript` and its limit of 252
AC component scans. That limit plus at most 33 component DC scans bounds
color scripts to 285 entries. Existing preset/band controls retain their limits.
Bands and DC component groups may change between valid refinement scans.

`ScanScript` cannot be combined with `ProgressiveLevel`, explicit
`ProgressiveAcScanEnds` or `ProgressiveAcRefinementBits`, nonzero
`ProgressiveRefinementBits`, `ProgressiveInterleaveDc = false` or
`SequentialInterleaveComponents = false`. Null preserves existing bytes.
The encoder copies the script and every component list before output; do not
modify them while encoding starts. It recomputes full coefficients per scan,
retaining bounded working storage and the existing input, quantization,
metadata and stream contracts. Single-component scans visit real blocks in
raster order; scans selecting two or three components use frame MCUs with
dummy blocks. Histograms and output share that traversal and restart interval.
Predictors, EOB runs and RST numbering reset per scan; DRI changes when the
interval changes. Final real coefficients are preserved. Extra scans increase
CPU work and do not guarantee smaller files or upstream byte parity.

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

Set `ProgressiveLevel` to select the pinned upstream's scan presets directly:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ProgressiveLevel = 2,
    RestartInterval = 64
});
```

| Level | DC | AC in JPEG zigzag order |
| --- | --- | --- |
| `0` | Sequential scan with AC | All coefficients at full precision |
| `1` | Full precision | Positions 1..63, then one refinement round |
| `2` or greater | Full precision | Positions 1..2 at full precision; 3..63, then two refinement rounds |

The level overrides `Progressive`, so setting it alone selects the corresponding
encoding mode. Null, the default, keeps the existing progressive controls and
JPEG bytes. Negative levels are rejected; all values at least two use the same
script, as in upstream's setter.

Progressive presets interleave DC only when both maximum **output** sampling
factors are one. Otherwise they send separate component DC scans in raster order,
with one real block per restart MCU. Grayscale always has 1x1 output factors;
RGB's default full resolution interleaves DC, while XYB's default 2x2 factors
select separate DC scans. Explicit `SamplingFactors` determine this choice for
color output, including schemes where all components have identical factors.
Every initial spectral range and each refinement round is sent for all components
in frame order before the next range or round, following the pinned script.

An explicit level cannot be combined with `ProgressiveAcScanEnds`,
`ProgressiveAcRefinementBits`, nonzero `ProgressiveRefinementBits`, or
`ProgressiveInterleaveDc = false`. Such combinations are rejected before output.
Quality, adaptive quantization, smoothing, sampling, metadata and Huffman settings
retain their usual behavior. The final quantized coefficients are preserved;
these presets do not establish upstream JPEG byte parity or guarantee smaller files.

Set `ProgressiveAcScanEnds` to send AC coefficients in several spectral bands:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Progressive = true,
    ProgressiveAcScanEnds = [2, 63],
    ProgressiveRefinementBits = 2
});
```

The inclusive end positions use JPEG zigzag order. This example sends positions
1..2 for each component before positions 3..63. Ends must be strictly increasing
in 1..63, and the final end must be 63, so every AC coefficient is delivered.
An empty list is invalid; null, the default, uses `[63]` and retains existing JPEG
bytes. A non-null list requires `Progressive = true`. The encoder copies and
validates the list before writing output; do not modify it while encoding starts.

Each round sends DC first, then every component in the first band, then every
component in each subsequent band. By default all bands defer the same number of
bits from `ProgressiveRefinementBits` and recur in each refinement round.
With `B` bands, `C` components and `N` refinement bits, the JPEG contains
`(1 + B * C) * (N + 1)` scans. Coefficients, quantization and the final decoded
pixels are preserved. More bands require more image passes and marker overhead;
they do not guarantee smaller files or faster decoding.

Set `ProgressiveAcRefinementBits` to defer a different number of bits in each band:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Progressive = true,
    ProgressiveAcScanEnds = [2, 63],
    ProgressiveAcRefinementBits = [0, 2]
});
```

This sends DC and positions 1..2 at full precision, followed by positions 3..63
with two bits deferred and then two refinement rounds of only that higher band.
The spectral ranges and bit schedule match the pinned upstream's level 2.
By default DotJpegli interleaves color DC in subsampled MCUs. Upstream sends
separate component DC scans when its maximum sampling factors exceed one;
use `ProgressiveInterleaveDc = false` to select that ordering, as described below.
These settings do not establish JPEG byte parity.

The new list requires `Progressive = true`, exactly one value per resolved AC
band, and values in 0..10. When `ProgressiveAcScanEnds` is null there is one
band, so a supplied depth list must have one value. Null depths use
`ProgressiveRefinementBits` for all bands and preserve existing bytes.
The scalar setting always controls DC; the list overrides only AC.
Both lists are copied and validated before output, with the same source-lifetime
requirements as other progressive settings.

All initial scans are sent first. Refinement rounds descend from the largest
DC or AC depth minus one to zero; each round sends DC if it has a bit left,
then each unfinished band for every component. Completed ranges are skipped.
With DC depth `D` and band depths `A[b]`, the scan count is
`D + 1 + C * sum(A[b] + 1)` with the default interleaved DC. Every coefficient still reaches full precision,
including when AC depth exceeds DC depth or all AC bands finish before DC.
Restart counting and EOB runs reset per scan, and storage remains bounded.

Set `ProgressiveInterleaveDc = false` to send separate component DC scans:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Progressive = true,
    ProgressiveInterleaveDc = false,
    ProgressiveAcScanEnds = [2, 63],
    ProgressiveAcRefinementBits = [0, 2]
});
```

For subsampled color, this combination follows the pinned upstream level-2
component, spectral and refinement schedule: three full-precision DC scans,
three scans for positions 1..2, then three initial scans for positions 3..63
and two rounds of their refinement. It has fifteen color scans. For full-resolution
1x1 sampling, upstream interleaves DC; keep the default `true` to follow that rule.
This option selects DC ordering explicitly and does not change sampling or AC bands.

Separate DC scans visit only real component blocks in raster order, including
partially filled edge blocks; wholly padded MCU blocks are omitted. Initial
DC and each later DC refinement round send components in frame order before
their AC scans. Each real component block counts as one MCU for restarts,
and DC prediction and RST0..RST7 numbering restart at every scan. Optimized
Huffman frequency collection uses the same traversal and predictor resets.

False requires `Progressive = true` and is rejected before output otherwise.
The default `true` preserves existing JPEG bytes, and grayscale bytes are
unchanged with either setting. For `C` components, DC depth `D` and band
depths `A[b]`, separate DC produces `C * (D + 1 + sum(A[b] + 1))` scans.
Final coefficients and decoded pixels are preserved. Color output needs
additional scans and passes; it does not promise smaller files or encoder-byte
parity with upstream. Working storage remains bounded, and existing typed-input,
stream, cancellation, smoothing and adaptive-quantization behavior applies.

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

In separate sequential scans, progressive AC scans and separate component DC scans, a single component
block counts as one MCU, so `RestartInterval` can produce more markers than in
an interleaved DC scan. Restart counting
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

### Standard quantization tables

Set `UseStandardQuantizationTables = true` to use the conventional JPEG Annex K
luma and chroma matrices instead of Jpegli's perceptual matrices:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    UseStandardQuantizationTables = true
});
```

The default is false and preserves existing JPEG bytes. `Quality` still maps
through `QualityToDistance`; a positive finite `Distance` still overrides it.
The standard matrices use the pinned upstream's piecewise linear-quality scale
from that distance, with rounding and clamping to the `ForceBaseline` range
(1..255 by default, or 1..32767 when false). Both chroma components
share table 1; grayscale uses only the luma matrix in table 0. Sampling factors
still select the output resolution, but standard tables have no 4:2:0 compensation.
Automatic RGB tables use the shared luma matrix in table 0, independently of this option.
The managed encoder clamps before converting to integers, including immediately
below distance 25 where upstream's scale can overflow its integer conversion.
Those cases retain legal saturated tables; they do not establish native parity.

Adaptive quantization, Jpegli's coefficient thresholds, smoothing, higher-precision
and direct YCbCr input, restart markers and progressive refinement remain available.
Adaptive thresholds follow upstream's shared chroma table selectors when inferring
distance from emitted tables. This option changes the matrices and their scaling;
it does not promise libjpeg byte parity, equivalent quality, size or speed.
In particular, fractional scaling can differ from libjpeg's integer quality scale.
The scaling and shared-table rules are ported from
[the pinned upstream quantizer](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc).

### Custom quantization tables

Set `QuantizationTables` to supply final quantizers in natural row-major
8x8 frequency order. Each table must contain exactly 64 `ushort` values in 1..255
(or 1..32767 with `ForceBaseline = false`);
the first value controls DC. The encoder writes them in JPEG zigzag order:

```csharp
ushort[] table = Enumerable.Repeat((ushort)16, 64).ToArray();
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    QuantizationTables = [table.AsMemory()],
    AdaptiveQuantization = false
});
```

Grayscale output requires one table. Color output accepts one shared table
(selectors 0/0/0), two tables with the second shared by components 1 and 2
(0/1/1), or three separate tables (0/1/2). Component order follows the output:
Y/Cb/Cr, R/G/B, or scaled X/Y/B-minus-Y. Equal-valued tables retain separate
selectors when supplied separately. Null, the default, preserves automatic tables
and existing bytes. Empty lists and values outside the selected range are rejected
with an `ArgumentException` for `QuantizationTables` before output.

Supplied values are used unchanged. They override table construction from
`Quality`, `Distance` and `UseStandardQuantizationTables`, including automatic
sampling and transfer-function scaling. Those options remain validated.
Adaptive quantization still applies unless disabled; YCbCr adaptive thresholds
use the actual table selectors and CICP transfer hint, while grayscale/RGB/XYB
retain their uniform adaptive multipliers. Thus a transfer hint can affect
YCbCr coefficients with adaptive quantization enabled without changing the tables.

The list and all table data are copied before writing output. Do not mutate them
while encoding starts; the encoder leaves caller storage unchanged. The same
tables serve coefficient preflight, Huffman optimization, sequential and progressive
scans, restarts, all input precisions and custom sampling. XYB still requires its
output ICC profile. Custom tables do not enable lossless JPEG.
This final-value API corresponds to table installation with scale 100 in
[the pinned upstream encoder](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/encode.cc);
DotJpegli rejects invalid quantizers instead of scaling or clamping them.

### Extended quantization

Set `ForceBaseline = false` to use the pinned upstream's nonbaseline quantizer
range of 1..32767 for automatic and caller-supplied tables:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 10,
    ForceBaseline = false
});
```

Each table uses eight-bit DQT entries when all its values are at most 255;
otherwise its 64 entries are unsigned 16-bit values in network byte order.
Tables with different precisions can share one DQT marker. Sequential output
uses extended sequential SOF1 when any table needs 16 bits, and otherwise
retains baseline SOF0. Progressive output always retains SOF2. Separate
component scans and caller-supplied scan scripts follow the same frame rule.

`ForceBaseline` defaults to `true` and preserves existing JPEG bytes. The opt-in
range applies to perceptual YCbCr/grayscale and XYB matrices and standard
matrices, including RGB's shared standard luma table. Adaptive YCbCr distance
inference treats 32767 as its saturated upper endpoint when the option is false;
255 then becomes an ordinary rounded value, which can change adaptive thresholds
even for eight-bit tables. Grayscale, RGB and XYB retain their uniform adaptive
multipliers. Custom tables are snapshotted and checked against the selected range
before output; zero and values above 32767 are rejected in extended mode.

Sampling, smoothing, source precision, metadata, restarts, bounded storage,
coefficient validation and stream behavior retain their existing contracts.
JPEG sample precision remains eight bits. Sixteen-bit quantizers do not enable
12-bit samples or lossless encoding. Some readers have limited SOF1 support.
This ports the quantizer range and marker rules from the pinned upstream
`quant.cc`, `encode.cc` and `bitstream.cc`; it does not establish encoder parity.

### Transfer-function quantization tuning

Set `CicpTransferFunction` to describe the transfer function of the supplied samples:

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    CicpTransferFunction = 16, // Already PQ-encoded input samples.
    IccProfile = matchingProfile
});
```

The option accepts one byte's code range, 0..255, and defaults to 2 (unspecified).
Codes 16 (PQ) and 18 (HLG) multiply upstream's perceptual global quantization
scale by 0.4 and 0.5 respectively, before any 4:2:0 compensation and coefficient
rounding. Other codes preserve default tuning, including 13 (sRGB); registration
of the supplied code is not validated. Quality and direct-distance controls retain
their existing meaning. This applies to RGB, grayscale and direct YCbCr input at
all three source precisions, with unchanged sampling and scan options.

Adaptive quantization infers its coefficient thresholds using the same
transfer-function scale. With `UseStandardQuantizationTables`, the emitted Annex K
tables ignore the hint, but adaptive thresholds still use it, following upstream.
Grayscale retains its constant adaptive coefficient multipliers.
RGB output ignores transfer-function hints for both tables and adaptive thresholds.

The hint follows [the pinned upstream quantizer](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc).
It does not convert transfer functions, apply a profile, tone-map or write CICP
metadata. Supply appropriately encoded samples and any matching ICC profile
yourself. Output remains a lossy eight-bit JPEG; this tuning alone does not
establish HDR conformance or a measured perceptual quality guarantee.

Set `InferCicpTransferFunctionFromIccProfile = true` to obtain the hint from the
profile's `cicp` tag instead of specifying it separately:

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    IccProfile = matchingProfile,
    InferCicpTransferFunctionFromIccProfile = true
});
```

The default is false and preserves the existing output for every attached
profile. Lookup applies only when `CicpTransferFunction` is 2 (unspecified);
every other explicit code takes precedence, including codes that retain default
tuning. To request unspecified tuning independently of the profile, leave lookup
disabled. Inferred codes feed both table generation and adaptive thresholds,
including standard tables and out-of-range float coefficient validation.

Lookup follows [the pinned upstream raw-profile helper](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/extras/enc/jpegli.cc).
It requires at least 132 profile bytes and a complete directory of 12-byte tag
entries. The last `cicp` entry wins, must declare at least 12 payload bytes, and
must point to 12 readable bytes starting with the `cicp` type signature. Byte 9
of that payload supplies the transfer code. Missing, truncated or mismatched
data returns 2; an invalid last entry does not fall back to an earlier one.
Unsigned counts and offsets are bounded before indexing. Directory scanning uses
constant working storage and checks cancellation periodically, before JPEG output.
The profile may span APP2 markers; lookup uses the original complete profile.

This narrow lookup does not validate ICC conformance, the header signature,
version or declared profile size, the rest of a larger declared tag, reserved
bytes, or the primaries, matrix and range fields. Unreadable lookup data remains
embedded unchanged. No other profile tags are interpreted, and no profile is
generated, reconciled with an explicit hint, or applied to the samples.

## Encoder port status

This status covers the encoder only. Decoder implementation is outside the
project's scope and does not count as unfinished port work.

Implemented: baseline or extended sequential with interleaved or separate component scans,
and optional spectral-selection and successive-approximation progressive
JPEG; grayscale/YCbCr JFIF and explicit RGB output; RGB, grayscale and direct
interleaved full-range YCbCr input with explicit output color-space selection; 4:4:4,
4:2:2, 4:2:0, 4:4:0, 4:1:1 and 4:1:0, plus 3x1, 3x2, 1x4, 2x4, 1x3 and 2x3 chroma
sample areas, using upstream scalar box sampling with edge replication and empty
dummy-block coding that preserves DC predictors at MCU edges; optional upstream
3x3 input smoothing before sampling; recursive DCT with automatic managed SIMD
and a scalar fallback; Jpegli
quality-to-distance and perceptual Y/Cb/Cr quantization tables; scalar adaptive
quantization with local differences, fuzzy erosion, masking, high-frequency and
gamma modulation, quality dampening and frequency-specific coefficient thresholds;
optimized length-limited canonical Huffman coding;
byte stuffing and marker output; optional MCU restart intervals with DRI and
cycling RST markers, including predictor resets during Huffman optimization;
optional row-based restart intervals with per-scan geometry and DRI updates;
progressive initial and refinement EOB-run aggregation with optimized Huffman tables;
optional DC and AC refinement scans with bounded correction-bit storage;
configurable complete AC spectral bands with optional separate refinement depths,
and optional separate component DC scans with real-block raster traversal.
Optional upstream progression-level presets select sequential output or the pinned
level-1/level-2 spectral and refinement scripts with sampling-based DC interleaving.
Complete caller-supplied scan scripts support component subsets, arbitrary
spectral schedules and changing refinement bands or DC component groups,
with upstream progression validation and per-scan restart geometry.
Raw Exif TIFF embedding, standard and extended UTF-8 XMP packets in APP1,
opt-in automatic partitioning of oversized XMP with selected standard properties, raw ICC profiles
with APP2 chunking, raw IPTC-IIM in Photoshop APP13, UTF-8 COM comments, and
configurable JFIF density fields are also implemented.
Direct perceptual-distance control and a public quality-to-distance mapping
are available, retaining baseline quantization tables by default and existing quality bytes.
Optional conventional Annex K quantization matrices use upstream distance scaling,
with one luma table and a shared chroma table, retaining Jpegli adaptive quantization.
Caller-supplied quantization tables support shared or separate output
component selectors, with strict validation and snapshots before output.
Opt-in nonbaseline quantization permits 1..32767 values, mixed eight/sixteen-bit
DQT tables and extended sequential SOF1 frames while keeping eight-bit samples.
Optional CICP transfer-function hints provide upstream PQ/HLG perceptual table
scaling and adaptive threshold inference without changing or signaling the input color space.
Opt-in lookup obtains those hints from attached ICC profiles using upstream's
raw-profile CICP tag rules while preserving the profile bytes.
Gamma-encoded 16-bit and normalized floating-point RGB and grayscale input are
supported with fractional samples preserved through quantization.
Direct YCbCr input supports the same three source precisions and downstream options.
Opt-in out-of-range floating-point input retains samples through quantization,
with finite-computation and eight-bit coefficient validation before output.
Custom per-component sampling factors support different Cb and Cr resolutions,
integer sample areas including subsampled luma, and at most ten blocks per MCU.

Direct already scaled XYB input and explicit sRGB/grayscale-to-XYB output use
the upstream XYB quantizers, sampling defaults and Y-based adaptive analysis,
with a required explicitly assigned output ICC profile. A managed factory
provides the pinned upstream's perceptual scaled XYB profile.
The same API provides a fixed perceptual sRGB RGB display profile for explicit
attachment to RGB or YCbCr output, with shared transfer curves and a CICP tag.
It also provides a fixed D65 grayscale profile with the sRGB transfer curve for
explicit attachment to grayscale output.
Linear-light variants provide identity transfer curves for RGB with sRGB
primaries and D65 grayscale, using the same explicit output-profile assignment.
An RGB Display P3 profile provides P3 colorants with the sRGB transfer curve,
D65-to-D50 adaptation and CICP signaling for explicit output-profile assignment.
Linear Display P3 and Rec.2020 RGB variants use the corresponding primaries,
D65-to-D50 adaptation, identity transfer curves and linear CICP signaling.
Rec.709 and Rec.2020 RGB profiles use the pinned BT.709 parametric transfer
curve, D65-to-D50 adaptation and CICP transfer code 1.
Additional RGB combinations provide Display P3 primaries with BT.709 transfer
and Rec.2020 primaries with sRGB transfer, including adapted colorants and CICP.
The DCI-P3 RGB profile uses DCI-to-D50 adaptation, gamma 2.6 decoding curves
and CICP primaries/transfer codes 11/17.
Rec.2100 PQ and HLG RGB profiles use Rec.2020 colorants and CICP signaling,
with upstream's default tone/gamut-mapped 9x9x9 RGB-to-Lab display transforms.
Additional grayscale profiles provide BT.709 at D65, gamma 2.6 at DCI white,
and untone-mapped 64-entry PQ/HLG curves at D65, without CICP signaling.
Configurable pure-gamma profiles provide sRGB/Rec.709, Display P3 and Rec.2020
RGB primaries with D65-to-D50 adaptation, plus D65 grayscale, with type-0
decoding curves and no CICP tag.
All built-in RGB and grayscale profiles also support selectable perceptual,
relative colorimetric, saturation and absolute colorimetric rendering intents,
with upstream descriptions and ICC profile identification. Existing overloads
retain their default bytes; scaled XYB remains perceptual-only as upstream requires.

The DCT SIMD path transforms independent columns and rows with runtime-sized
`System.Numerics.Vector<float>` lanes, preserving the scalar evaluation order
and fused multiply-add rounding. Hardware acceleration is selected automatically
when the vector width divides an eight-sample row; otherwise the scalar path
is used. Both paths retain bounded stack storage, overlapping-span support and
standard JPEG coefficient units. This completes the DCT SIMD path; broader SIMD
coverage and parallel encoding remain pending. No speed improvement is claimed
without CI performance measurements.

Pending encoder work: fractional sampling ratios,
other built-in profile transfer/white-point combinations and arbitrary source ICC color management,
structured metadata editing, other metadata, SIMD in other encoder stages and parallel encoding.
Fractional sampling ratios are also rejected by the pinned upstream encoder.
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
The external decoder validates encoder output and is not a port target.

Rendering-intent regressions compare complete profiles with independently compiled
pinned C++ fixtures across every RGB and grayscale factory and all four intents.
Independent parsing checks header intents, descriptions, aligned tag extents and
the ICC MD5 ID, including the fields excluded from identification. They check
preserved transform payloads, existing overload bytes, fresh storage, invalid
intent rejection and exact APP2 embedding with unchanged JPEG coding across
representative source precisions and scan modes. These checks validate profile
serialization and sample preservation without establishing color-managed lossy
JPEG quality or reader-specific rendering.

Custom-script regressions independently parse explicit scan descriptors and
per-coefficient progression, reconstruct final real coefficients and check
decoded-pixel parity. They cover component subsets, changing DC groups,
split and joined AC refinement bands, per-scan restart geometry, source
precisions, compatible preset bytes, caller-list snapshots and rejection
before output.

Separate-sequential regressions independently parse frame/scan descriptors,
Huffman symbols and coefficients, checking real-block raster traversal, predictor
resets, entropy padding, changing DRI intervals and RST cycling. They compare
final coefficients and decoded pixels with interleaved output across sampling
schemes, output color spaces, source precisions, custom tables and Huffman modes,
and cover default/grayscale byte compatibility, metadata, stream behavior,
cancellation and validation before output.

Custom-quantization regressions independently parse DQT zigzag values and frame
selectors, compare constant-block DC and complete cosine-DCT coefficients, and
check table-sharing effects on adaptive thresholds. They cover default-table byte
compatibility, coefficient and decoded-pixel parity across source precisions and
scan/Huffman/restart modes, custom sampling, validation before output, floating-point
coefficient preflight, and mutation of caller tables after the first stream write.

Extended-quantization regressions independently parse mixed DQT precisions,
network-byte-order words, zigzag values and SOF0/SOF1/SOF2 selection. Pinned scalar
C++ fixtures check complete coarse YCbCr tables and adaptive distance inference
at both saturation endpoints. Independent DC/cosine-DCT and entropy checks cover
coefficient parity across scan scripts, refinement and restarts; an independent
decoder compares sequential and progressive pixels. They also cover default bytes,
automatic table modes, extreme-distance clamping, typed and padded input,
out-of-range float preflight, validation before output, snapshots and cancellation.

Progression-level regressions independently parse the emitted scan descriptors,
coefficients and restart boundaries, compare final coefficients and decoded pixels
with baseline output, and verify sampling-based DC interleaving across output
color spaces. They also cover input precisions, strides, preset aliases, custom
setting rejection before output, stream behavior and cancellation.

Spectral-band regressions independently parse each scan's spectral range and
initial/refinement entropy, verify complete per-block coefficient coverage,
EOB runs, correction bits, restart cycling and scan padding, and compare final
coefficients with baseline encoding. Decoder checks verify final pixels where
the independent decoder supports the output mode. Focused cases also cover
single-position bands, typed and padded input, default-byte compatibility,
nonseekable streams, cancellation and invalid partitions before output.

Per-band refinement regressions check mixed AC depths independently of DC depth,
including upstream's level-2 spectral/bit descriptors and the maximum ten-bit
depth. Independent scan parsing checks range coverage, refinement order, EOB
runs, correction bits, restart boundaries and padding, then compares complete
coefficients and decoded pixels with baseline output. They also cover typed
inputs, default-byte compatibility, list snapshots and validation before output.

Separate-DC regressions independently parse component selectors, spectral and
refinement schedules, real-block traversal, restart counts, cycling and padding.
They compare coefficients with baseline output across odd edges, custom sampling,
mixed refinement depths and Huffman settings, with decoder checks for supported
color modes. They also cover typed input, grayscale and default compatibility,
nonseekable streams, cancellation and validation before output.

SIMD DCT regressions compare exact coefficient bits with the scalar implementation
for fractional/random blocks, all impulse positions, constants, checkerboards,
signed zero, subnormal and large finite samples. An independent cosine definition
checks coefficient units and frequency order. Both paths are checked for overlapping
spans, untouched output tails and validation before writes. CI also runs the
focused DCT checks with hardware intrinsics disabled to exercise the scalar fallback
and software vector operations.

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

Direct-distance regressions check the public quality mapping, precedence and
validation before output, plus JPEG byte compatibility at equivalent quality
across source precisions and layouts. Independently compiled pinned C++ table
fixtures cover direct distances and nonlinear scaling. Independent marker and
coefficient parsing checks table clamping and constant-block DC values, including
positive finite distance extremes; these extremes do not establish upstream
encoder parity.

Standard-table regressions independently parse table IDs and frame selectors,
check Annex K table scaling and constant-block coefficients, and retain native
scalar fixtures for table values and shared-selector adaptive distance inference.
They also cover default byte compatibility, direct-distance precedence, source
precision, padded layouts, custom sampling, restart markers, baseline and
progressive coefficient/decoded-pixel parity, and validation before output.

Transfer-function regressions retain complete native scalar PQ/HLG quantization
tables and adaptive-inference fixtures for 4:4:4, 4:2:0 and grayscale. Independent
coefficient parsing checks shared-chroma adaptive thresholds using analytical
cosine inputs that distinguish missing hints and incorrect table selectors. They
also cover default and other-code byte compatibility, typed and padded inputs,
custom sampling, scan/refinement parity, metadata and stream behavior, finite
distance extremes, and out-of-range float coefficient validation before output.

ICC transfer lookup regressions compare inferred and explicit hints across
input precisions, layouts, sampling, adaptive and standard tables, scan modes
and restarts. They independently reconstruct APP2 chunks to check unchanged
profile bytes and cover sliced storage, marker boundaries, missing and malformed
directories, extreme unsigned fields and last-entry selection. Native helper
fixtures check the pinned lookup's permissive field handling. Cancellation,
nonseekable streams and out-of-range coefficient rejection before output are covered.

RGB-to-XYB regressions check transformed values against independently compiled
pinned scalar C++ fixtures and a double-precision transfer/opsin oracle. They
cover source normalization, ignored alpha and padding, grayscale replication,
sampling and smoothing, adaptive analysis, progressive/restart coefficient
parity, unchanged default behavior and rejection before output. Native transform
fixtures allow the documented difference from upstream's fast approximations;
these checks do not establish an ICC color-managed RGB roundtrip.

Built-in XYB profile regressions check the complete profile against a SHA-256
fixture generated by compiling the pinned C++ builder unchanged. Independent
parsing checks header fields, aligned tag extents, curve/CLUT/matrix structure
and the ICC MD5 profile ID. An independent forward-profile evaluator compares
scaled XYB samples with analytical sRGB-to-D50-XYZ values, including black,
white and saturated colors. Encoding checks verify exact APP2 embedding,
unchanged coding bytes, typed source input, progressive output and nonseekable
streams. These checks validate the profile transform, without establishing a
color-managed lossy JPEG quality comparison.

Built-in sRGB profile regressions check the complete profile against an
independently compiled pinned C++ SHA-256 fixture, the ICC MD5 ID and fresh
storage on every factory call. Independent parsing checks the header, aligned
tags, shared transfer curves and CICP fields. A double-precision sRGB-to-D50-XYZ
oracle checks the parsed matrix and curves over a color cube, including transfer
boundaries. Encoding checks cover exact APP2 embedding, unchanged coding data,
CICP lookup, source precisions, RGB/YCbCr output, progressive refinement and
nonseekable streams. These checks do not establish a color-managed lossy JPEG
quality comparison.

Built-in sRGB grayscale profile regressions compare complete bytes with an
independently compiled pinned C++ fixture, verify the ICC MD5 ID and fresh
storage, and independently parse the header, aligned tags, D65 media white and
sRGB decoding curve. Encoding checks cover exact APP2 embedding, unchanged
coding data, typed and padded inputs, explicit grayscale conversion, scan modes,
restart markers, CICP lookup and nonseekable streams. These checks do not establish
a color-managed lossy JPEG quality comparison.

Linear-sRGB profile regressions compare both complete profiles with independently
compiled pinned C++ fixtures, verify their ICC MD5 IDs and fresh storage, and
parse aligned tags, shared identity curves, colorants, white points and CICP
fields. Independent curve and matrix evaluation checks linear samples against
D50 XYZ values. Encoding checks verify exact APP2 embedding and unchanged coding
data with representative precisions, scan modes and CICP lookup settings. These
checks validate profile serialization and sample preservation, without
establishing a color-managed lossy JPEG quality comparison.

Display P3 profile regressions compare complete bytes with an independently
compiled pinned C++ fixture, verify the ICC MD5 ID and fresh storage, and parse
aligned tags, shared sRGB curves, P3 colorants, Bradford adaptation and CICP
fields. An independent double-precision P3-to-D50-XYZ oracle checks the parsed
matrix and curves across a color cube and transfer boundaries. Encoding checks
verify exact APP2 embedding and unchanged coding data across source precisions,
RGB/YCbCr output, progressive refinement, restarts and nonseekable streams.
These checks validate profile serialization and sample preservation without
establishing a color-managed lossy JPEG quality comparison.

Linear P3 and Rec.2020 profile regressions compare complete bytes with
independently compiled pinned C++ fixtures, verify ICC MD5 IDs and fresh storage,
and parse aligned tags, identity curves, colorants, adaptation and CICP fields.
Independent primary and Bradford matrix oracles check linear samples against
D50 XYZ. Encoding checks verify exact APP2 embedding and unchanged coding across
input precisions, RGB/YCbCr output, scan modes and CICP lookup settings. These
checks validate serialization and sample preservation without establishing a
color-managed lossy JPEG quality comparison.

Rec.709 and BT.709-transfer Rec.2020 profile regressions compare complete bytes
with independently compiled pinned C++ fixtures and verify ICC MD5 IDs, fresh
storage, aligned tags and CICP fields. Independent transfer-curve and
primary/Bradford matrix oracles check parsed profiles, including the original
and fixed-point-rounded transfer knee. Encoding checks verify exact APP2
embedding and unchanged coding across input precisions, RGB/YCbCr output,
progressive refinement, restarts and CICP lookup settings.

Additional RGB profile regressions compare complete bytes with independently
compiled pinned C++ fixtures and verify ICC MD5 IDs, fresh storage, aligned tags
and CICP fields. Independent transfer-curve and primary/Bradford matrix oracles
check P3 with BT.709 transfer and Rec.2020 with sRGB transfer. Encoding checks
verify exact APP2 embedding and unchanged coding across representative source
precisions, RGB/YCbCr output, scan modes, restarts and nonseekable streams.
These checks validate serialization and sample preservation without establishing
a color-managed lossy JPEG quality comparison.

DCI-P3 profile regressions compare complete bytes with an independently compiled
pinned C++ fixture and verify the ICC MD5 ID, fresh storage, aligned tags, shared
gamma 2.6 curves and CICP fields. Independent primary and Bradford matrix oracles
check colorants and DCI-to-D50 adaptation. Encoding checks verify exact APP2
embedding and unchanged coding across input precisions, RGB/YCbCr output, scan
modes, refinement, restarts and nonseekable streams. These checks validate
serialization and sample preservation without establishing a color-managed
lossy JPEG quality comparison.

Additional grayscale regressions compare all four complete profiles with
independently compiled pinned C++ fixtures and verify their MD5 IDs, fresh
storage, aligned tags and source media white. Independent BT.709/DCI formulas
and PQ/HLG transfer-table oracles check decoding and interpolation. Encoding
checks verify exact APP2 embedding, preserved coding across input precisions,
scan modes and restarts, nonseekable streams, cancellation and explicit transfer
hints in the absence of a CICP tag. These checks validate serialization and
sample preservation without establishing color-managed lossy JPEG quality.

Pure-gamma profile regressions compare complete profile hashes and IDs with
independently compiled pinned C++ fixtures, including both exponent endpoints
and fractional values. They independently parse dynamic tag offsets, aligned
descriptions, shared type-0 curves, MD5 IDs and the absence of CICP, and compare
decoded curve samples with the specified power law. Encoding checks verify exact
APP2 embedding and preserved coding across input precisions, output color spaces,
scan modes and restarts, including nonseekable streams and cancellation.

Wide-gamut pure-gamma regressions compare complete P3 and Rec.2020 profiles
with independently compiled pinned C++ fixtures, including the exponent endpoints.
Independent parsing checks aligned tags, shared type-0 curves, MD5 IDs and the
absence of CICP; primary and Bradford matrix oracles check the adapted colorants
and power-law decoding. Encoding checks verify exact APP2 embedding and unchanged
coding across source precisions, RGB/YCbCr output, progressive refinement and
nonseekable streams. These checks validate serialization and sample preservation
without establishing a color-managed lossy JPEG quality comparison.

Scaled XYB regressions compare complete emitted quantization tables with
independently compiled pinned C++ scalar fixtures. Independent JPEG parsing
checks three quantizer selectors, shared Huffman selectors, sampling factors,
constant and smoothed sampled cosine-DCT coefficients, and baseline/progressive
refinement parity. They cover Y-based adaptive thresholds, typed and padded
inputs, ICC byte preservation, stream behavior, cancellation and rejection
before output. Decoder checks compare reconstructed scaled channels across scan
modes; they do not test an ICC color-managed RGB roundtrip.

Output color-space regressions independently parse RGB frame/scan identifiers,
shared table selectors and marker framing, and check unequal constant-channel DC
coefficients against analytical values. They cover grayscale conversion and neutral
chroma, source precision/layout/stride compatibility, green-channel adaptive analysis,
custom sampling and progressive/restart coefficient parity. Default bytes and
validation before output remain covered by focused input regressions.

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

Row-restart regressions independently derive scan widths from frame and scan
markers, verify changing DRI intervals, RST cycling, predictor resets, EOB-run
boundaries and entropy padding, and reconstruct final coefficients. They check
fixed-interval byte equivalence where scan widths match, unchanged decoded
samples, odd edges, custom sampling, progressive refinement and level presets,
overflow-safe clamping, typed inputs, validation before output and stream behavior.

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

IPTC regressions independently parse Photoshop APP13 and the image resource
header, checking sliced raw data, network-byte-order lengths, odd-byte padding
and the single-marker limit. They also cover validation before output, combined
metadata ordering, default compatibility, unchanged coding bytes and decoded
pixels, typed and padded inputs, nonseekable streams, cancellation and I/O failures.

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
