# Profiles and metadata

[Documentation](README.md)

## Structured editing and reconciliation

Raw encoder options embed supplied payloads. The separate `ExifMetadata`,
`XmpMetadata` and `IptcMetadata` editors interpret supported structures and
serialize owned results for those options. Editing is explicit; it never changes
source pixels. These managed APIs extend the encoder port.

`ExifMetadata.Parse` reads a TIFF body in either byte order. `GetEntry`/`SetEntry`
edit standard TIFF values, and typed methods edit unsigned integers, ASCII and
unsigned rationals. Image, Photo, GPS, Interoperability and Thumbnail directories
are supported. Serialization retains the input byte order (new editors use
little endian), preserving endian-dependent opaque structures such as Unicode
UserComment. Raw primitive numerical entry values use little-endian order in the
editor API. Serialization retains unknown standard entries and rebases JPEG
thumbnail offsets. Malformed directories, offsets,
counts and cycles are rejected. Classic TIFF field types 1–12 are supported;
BigTIFF, additional next-IFD chains and nonstandard field types are unsupported.
Parsing conservatively limits aggregate declared value bytes to the input length,
so heavily aliased value storage may be rejected. MakerNotes and known unsupported
offset-bearing structures cannot safely be relocated: serialization rejects them
until removed. Private tags with embedded offsets require the caller to remove
them when their format depends on fixed positions.
`RemoveThumbnail` removes the complete preview directory and JPEG data.

`XmpMetadata.Parse` accepts ordinary empty-subject RDF descriptions inside an
`x:xmpmeta` or `rdf:RDF` envelope. Simple properties, complete `XElement` property
trees, ordered/unordered string arrays and language alternatives can be read,
replaced or removed by namespace URI and local name. Unedited properties and
qualifiers are retained; XML values returned or supplied are copied. DTDs and
external entities are prohibited. Standard packets containing `HasExtendedXMP`
links can be inspected but cannot be edited: combine the standard and extended
trees and remove the old link first, then let the encoder partition the result.

`IptcMetadata.Parse` reads IIM datasets with standard or extended lengths.
`GetValues`/`SetValues` edit opaque repeated datasets; `GetTextValues`/
`SetTextValues` edit Application Record 2 text. Text editing uses strict UTF-8
with the `ESC % G` coded-character-set designation. Undesignated existing text
must be ASCII, and incompatible encodings are rejected rather than relabeled.
Known By-line, CopyrightNotice and Caption-Abstract byte limits, repeatability and
character restrictions are enforced. Nonempty text edits add missing RecordVersion
2:0 as version 4, retaining existing version bytes. Removing a text dataset does
not decode or relabel retained data. Raw dataset editing remains available for
other values and schemas.

Use `JpegliMetadata.Reconcile` to prepare a consistent set of metadata before
encoding:

```csharp
var options = JpegliMetadata.Reconcile(new JpegliEncoderOptions
{
    ExifData = existingTiff,
    XmpPacket = existingXmp,
    IptcData = existingIim,
    DensityUnit = JpegDensityUnit.DotsPerInch,
    HorizontalDensity = 300,
    VerticalDensity = 300
}, width, height, new JpegliMetadataEdits
{
    Orientation = 1,
    Artist = "Ada",
    Copyright = "Copyright 2026 Ada",
    Description = "Portrait",
    ExifColorSpace = 1 // Only when the stored samples have an sRGB interpretation.
});
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: options);
```

The helper returns an options copy with owned Exif/IPTC bytes and serialized XMP;
it does not mutate the original storage. It updates TIFF/Exif and XMP dimensions,
sets the requested orientation (default 1), and removes the old Exif thumbnail.
Orientation describes the supplied pixels and does not rotate them. Physical
JFIF densities are copied to Exif and XMP; unspecified units remove stale physical
resolution fields while preserving the JFIF pixel aspect ratio. RGB/XYB output
omits JFIF as usual, but retains explicitly supplied physical resolution in Exif/XMP.

Supplied artist, copyright and description values replace the corresponding
Exif ASCII, XMP Dublin Core and IPTC text fields. Stale XMP TIFF aliases are removed;
copyright and description update `x-default` while retaining other languages.
Null leaves a descriptive field untouched, including existing differences between
formats; empty removes all representations of that field. These shared edits
must be ASCII without NUL, to fit Exif ASCII, and meet IPTC limits (artist 32 bytes,
copyright 128, description 2,000). Use the individual XMP/IPTC editors for Unicode.
Artist and copyright permit graphic characters and spaces; description also
permits CR/LF line breaks. Other control characters are rejected.
Optional Exif color space accepts 1 (sRGB) or 65535 (uncalibrated), synchronizing
Exif and XMP without inferring ICC transforms. Unrelated fields are retained.

Reconciliation enables automatic XMP partitioning and validates final payload
sizes before returning. Existing manual extended packets, stale digest links and
recognized raw Exif/XMP/JFIF/Photoshop markers must be resolved first; unrelated
raw markers and ICC bytes are retained. Use the returned options with the same
dimensions supplied to the helper. Schema mappings follow the
[Adobe XMP namespaces](https://developer.adobe.com/xmp/docs/xmp-namespaces/)
and [IPTC IIM specification](https://www.iptc.org/std/IIM/4.2/specification/IIMV4.2.pdf).

## Exif

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

## XMP

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

### Extended XMP

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

### Automatic XMP partitioning

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

## ICC profiles

Set `IccProfile` to raw ICC profile bytes to embed them unchanged in numbered
APP2 markers. An empty profile, the default, writes no ICC markers except that
XYB output requires a nonempty matching scaled XYB profile. The profile
is opaque unless the opt-in [CICP lookup](quantization.md#transfer-function-quantization-tuning) is enabled. Supply a
profile describing the output's RGB, grayscale or scaled XYB interpretation.
Use `JpegliIccProfiles.CreateSrgbProfile()` for a built-in RGB sRGB profile,
`JpegliIccProfiles.CreateDisplayP3Profile()` for RGB Display P3,
`JpegliIccProfiles.CreateSrgbGrayscaleProfile()` for grayscale with the sRGB
transfer function, or `JpegliIccProfiles.CreateXybProfile()` for scaled XYB. These factories return a
fresh mutable array with deterministic bytes and require explicit assignment to
`IccProfile`. With `SourceIccProfile` empty, the encoder attaches the profile
without converting samples. Supplying a source profile enables
[source ICC color conversion](encoding.md#source-icc-color-management) into the
destination profile, or an automatically attached sRGB/grayscale/XYB destination.
Do not change its backing memory or dispose its owner until encoding returns.
Profiles can contain at most 16,707,345 bytes (255 chunks of up to 65,519 bytes), following
[ICC.1 Annex B.5](https://www.color.org/ICC1V42old.pdf).

### Rendering intents

Every RGB and grayscale profile factory also has an overload accepting
`IccRenderingIntent.Perceptual`, `RelativeColorimetric`, `Saturation` or
`AbsoluteColorimetric`. Gamma factories take the intent after `gamma`:

```csharp
byte[] profile = JpegliIccProfiles.CreateSrgbProfile(IccRenderingIntent.RelativeColorimetric);
byte[] gammaProfile = JpegliIccProfiles.CreateGammaGrayscaleProfile(2.2, IccRenderingIntent.AbsoluteColorimetric);
```

Overloads without an intent argument use perceptual intent. Explicit-intent
overloads use upstream's intent-dependent descriptions, header intent and ICC
MD5 identification. Invalid enum values throw `ArgumentOutOfRangeException`
for `renderingIntent`. Intent is a preference for the reader's color-management
system when attaching profiles without source conversion. The profile's colorants, curves,
adaptation and HDR tone/gamut transforms are the same across intents. HDR
profiles retain `A2B0` and the identity compatibility `B2A0`; reader behavior can
vary. `CreateXybProfile()` remains perceptual-only, matching the pinned builder.
The generic factories below cover every combination of the pinned builder's
named white points, primaries and transfer functions.

### Generic profile factories

`CreateRgbProfile(primaries, whitePoint, transferFunction, renderingIntent)`
and `CreateGrayscaleProfile(whitePoint, transferFunction, renderingIntent)`
select the color encoding directly. `IccPrimaries` provides `Srgb`, `DisplayP3`
and `Rec2020`; `IccWhitePoint` provides `D65`, `EqualEnergy` (illuminant E) and
`Dci`; `IccTransferFunction` provides `Rec709`, `Linear`, `Srgb`, `Pq`, `Dci`
(gamma 2.6) and `Hlg`. Rendering intent defaults to perceptual:

```csharp
byte[] profile = JpegliIccProfiles.CreateRgbProfile(
    IccPrimaries.DisplayP3, IccWhitePoint.EqualEnergy, IccTransferFunction.Linear);
byte[] grayProfile = JpegliIccProfiles.CreateGrayscaleProfile(
    IccWhitePoint.D65, IccTransferFunction.Dci, IccRenderingIntent.RelativeColorimetric);
```

`CreateGammaRgbProfile(primaries, whitePoint, gamma, renderingIntent)` and
`CreateGammaGrayscaleProfile(whitePoint, gamma, renderingIntent)` cover the same
white points with pure decoding exponents in 1..8192. Existing named factories
retain their exact bytes. The selection describes samples already using those
primaries, white point and transfer; attach the returned profile explicitly.
P3 names its primaries independently of its white point. Selecting DCI white
does not convert Display P3 samples to DCI-P3, and selecting a linear curve
does not linearize samples.

RGB profiles adapt their colorants and source white to D50. CICP signaling is
available for D65 RGB and P3 with DCI white, following upstream. In those
combinations PQ/HLG use the upstream 9x9x9 tone/gamut-mapped Lab transform.
Other RGB PQ/HLG combinations use shared 64-entry decoding curves with XYZ PCS,
without tone mapping, HLG OOTF or CICP. Grayscale retains the selected source
media white with no adaptation or CICP; its PQ/HLG curves are also untone-mapped.
Pure-gamma profiles always omit CICP. Source ICC conversion is available through
`SourceIccProfile`, subject to the supported transform formats in the encoding
guide. Profile embedding does not establish color-managed
JPEG quality or HDR display conformance.

### Custom chromaticities

The same four generic factories accept custom CIE xy coordinates through
`IccChromaticity` and `IccRgbPrimaries`:

```csharp
var adobeRgbPrimaries = new IccRgbPrimaries(
    new(0.640, 0.330), new(0.210, 0.710), new(0.150, 0.060));
byte[] profile = JpegliIccProfiles.CreateGammaRgbProfile(
    adobeRgbPrimaries, new IccChromaticity(0.3127, 0.3290), 563.0 / 256.0);
byte[] grayProfile = JpegliIccProfiles.CreateGrayscaleProfile(
    new IccChromaticity(0.34567, 0.35850), IccTransferFunction.Linear);
```

These overloads support all six named transfer functions, pure gamma and all
four rendering intents. RGB profiles compute the primary matrix and Bradford
adaptation to D50; grayscale profiles retain the custom source media white.
Custom profiles omit CICP even if their coordinates match a named encoding.
PQ/HLG use untone-mapped 64-entry decoding curves with XYZ PCS and no HLG OOTF.
Select the named overloads for their existing CICP and HDR tone/gamut transforms.

Coordinates must be finite. White points require `0 <= X <= 1` and
`0 < Y <= 1`; primary coordinates may lie outside that range, including imaginary
primaries. Singular primary matrices and computations that cannot be stored
in ICC signed fixed-point fields are rejected by the factory. Profiles have
deterministic coordinate-based descriptions and return fresh storage.
Attach the profile explicitly to `IccProfile` for samples already using that
encoding; creating or embedding it does not transform the input samples.

### Built-in profiles

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

## IPTC

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

## Comments

`Comment` writes one JPEG COM marker containing UTF-8 bytes without a terminator.
Null or empty writes no comment. Its UTF-8 length can be at most 65,533 bytes;
non-ASCII characters can consume several bytes each. JPEG does not define a COM
character encoding, so readers may display non-ASCII comments differently.

## Raw metadata markers

Use `MetadataMarkers` for other metadata formats or additional raw comments.
Each `JpegMetadataMarker` supplies a marker code without the leading `0xFF` and
its complete payload, including any format-specific identifier:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    MetadataMarkers =
    [
        new(0xE3, File.ReadAllBytes("application-metadata.bin")), // APP3
        new(0xFE, new byte[] { 0x63, 0x61, 0x66, 0xE9 }) // Raw COM in the caller's encoding.
    ]
});
```

This ports the pinned upstream's `jpegli_write_marker` support for all
APP0..APP15 (`0xE0`..`0xEF`) and COM (`0xFE`) markers. Structural markers are
rejected. Each payload must fit 65,533 bytes; larger payloads must be divided
by the caller according to their format. The encoder adds the marker and
big-endian length only, preserving payload bytes without signatures, padding,
terminators or entropy byte stuffing. Empty payloads emit a length of two.
Duplicates and caller order are preserved; null or an empty list writes nothing.
All descriptors are copied and validated before output. Keep the list unchanged
while encoding starts, and keep payload memory unchanged and alive until encoding
returns. Invalid codes, oversized payloads and null descriptors are rejected
with `ParamName` equal to `MetadataMarkers` before any output.

Payloads are not interpreted or reconciled with built-in metadata. The caller
must ensure valid structures and consistent values. In particular, recognized
JFIF, Adobe or ICC signatures can change a reader's interpretation of the JPEG;
adding them does not change the encoder's color-space or sampling settings.
Use the [structured editors and reconciliation helper](#structured-editing-and-reconciliation)
to interpret and update supported metadata before assigning the built-in fields.

## JFIF density

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

## Metadata order and stream behavior

Metadata is written after JFIF for grayscale/YCbCr, or directly after SOI for RGB,
in this order: Exif APP1,
standard XMP APP1, extended XMP APP1 chunks, ICC APP2 chunks, IPTC APP13, COM
and then `MetadataMarkers` in caller order,
followed by the coding tables and scans.
Metadata does not change coefficients or coding bytes. Viewers may interpret
orientation, profiles and other metadata when displaying the image.
Invalid metadata options are rejected before writing output. Cancellation is
checked between metadata segments as well as between MCUs; cancellation or an
I/O failure during metadata output can leave a partial JPEG and leaves the stream open.
