# Profiles and metadata

[Documentation](README.md)

## Structured editing and reconciliation

Raw encoder options (`ExifData`, `XmpPacket`, `IptcData`) embed caller-supplied
byte payloads directly. When you need to inspect, modify, or synchronize
metadata before encoding, use `ExifMetadata`, `XmpMetadata`, `IptcMetadata`, and
`JpegliMetadata.Reconcile`.

### Structured editors

- **`ExifMetadata.Parse`:** Parses classic TIFF (`1..12` field types) in
  little-endian or big-endian byte order across Image, Photo, GPS,
  Interoperability, and Thumbnail directories. `GetEntry` / `SetEntry` and typed
  methods edit unsigned integers, ASCII strings, and unsigned rationals (raw
  primitive numerical entry bytes in the API use little-endian order).
  `Serialize` preserves the original TIFF byte order (new instances default to
  little-endian), keeps unknown standard tags, and rebases JPEG thumbnail
  offsets. `RemoveThumbnail` removes the preview directory and thumbnail bytes.
  BigTIFF, extra next-IFD chains, and non-relocatable offset structures (such as
  MakerNotes) are rejected unless removed first.
- **`XmpMetadata.Parse`:** Parses RDF descriptions (`rdf:about=""` or omitted)
  inside an `x:xmpmeta` or `rdf:RDF` root (DTDs and external entities are
  prohibited). Reads, sets, or removes simple properties, `XElement` subtrees,
  ordered/unordered string arrays, and language alternatives by namespace URI
  and local name, preserving unedited properties and qualifiers. Standard
  packets that already contain `xmpNote:HasExtendedXMP` links can be inspected
  but require merging the extended tree and removing the old link before editing.
- **`IptcMetadata.Parse`:** Parses IPTC-IIM datasets with standard or extended
  lengths. `GetValues` / `SetValues` edit raw datasets; `GetTextValues` /
  `SetTextValues` edit Application Record 2 text in UTF-8 (adding the `ESC % G`
  coded-character-set marker and RecordVersion `2:0 = 4` when needed, while
  requiring existing undesignated text to be ASCII and enforcing By-line,
  CopyrightNotice, and Caption-Abstract limits).

### Reconciling metadata across formats

Use `JpegliMetadata.Reconcile` to produce an updated `JpegliEncoderOptions` copy
with consistent dimensions, orientation, physical densities, and descriptive
fields across JFIF, Exif, XMP, and IPTC:

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
    ExifColorSpace = 1 // 1 = sRGB, 65535 = uncalibrated.
});
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: options);
```

`Reconcile` does not mutate the original buffers. It:

- Updates TIFF/Exif and XMP image dimensions, sets `Orientation` (default `1`;
  does not rotate pixels), and strips any stale Exif thumbnail.
- Copies physical JFIF densities (`DotsPerInch` or `DotsPerCentimeter`) to Exif
  and XMP, or removes stale physical resolution tags when `DensityUnit` is
  `Unspecified`.
- Synchronizes `Artist`, `Copyright`, and `Description` across Exif ASCII, XMP
  Dublin Core (`x-default`), and IPTC Record 2 (`null` leaves a field unchanged;
  `""` removes it from all three formats). Because shared edits must fit both
  Exif ASCII and IPTC length limits (`Artist <= 32` bytes, `Copyright <= 128`
  bytes, `Description <= 2000` bytes), use `XmpMetadata` or `IptcMetadata`
  directly for non-ASCII Unicode text.
- Synchronizes `ExifColorSpace` (`1` or `65535`) across Exif and XMP when set.
- Enables `AutoPartitionXmp` and validates final segment sizes before returning.
   Schemas follow the
  [Adobe XMP namespaces](https://developer.adobe.com/xmp/docs/xmp-namespaces/)
  and [IPTC IIM specification](https://www.iptc.org/std/IIM/4.2/specification/IIMV4.2.pdf).

## Exif

Set `ExifData` to raw TIFF bytes starting with the TIFF header (`II` or `MM`), up
to `65,527` bytes. The encoder prepends `Exif\0\0` and writes a single `APP1`
segment (do not include `Exif\0\0` or `APP1` marker bytes in `ExifData`). Empty
omits Exif. For grayscale and YCbCr output, the JFIF `APP0` marker remains first,
followed by Exif `APP1`, matching
[Adobe's JPEG handler](https://github.com/adobe/XMP-Toolkit-SDK/blob/main/XMPFiles/source/FileHandlers/JPEG_Handler.cpp).

## XMP

Set `XmpPacket` to a serialized XMP string (up to `65,504` UTF-8 bytes unless
`AutoPartitionXmp` is enabled). The encoder writes the UTF-8 bytes in a single
`APP1` marker with the `http://ns.adobe.com/xap/1.0/\0` header, without adding a
BOM, terminator, or `xpacket` wrapper. Unpaired UTF-16 surrogates are rejected.
Framing follows
[Adobe's XMP storage specification](https://developer.adobe.com/xmp/docs/xmp-specifications/).

### Extended XMP

Set `ExtendedXmpPacket` to a serialized extended XMP XML tree **without an
`xpacket` wrapper** (up to `uint.MaxValue` UTF-8 bytes):

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ExtendedXmpPacket = File.ReadAllText("extended-xmp.xml")
});
```

The encoder splits the UTF-8 payload into `APP1` chunks of up to `65,458` bytes,
each prefixed with the uppercase 32-character MD5 hex digest of the full
extended payload, its 32-bit total byte length, and its 32-bit chunk offset in
big-endian order ([XMP Specification Part 3](https://github.com/adobe/XMP-Toolkit-SDK/blob/main/docs/XMPSpecificationPart3.pdf)).

- If `XmpPacket` is `null` or empty, the encoder generates a minimal standard
  XMP packet containing the required `xmpNote:HasExtendedXMP` digest link.
- If you supply both `XmpPacket` (`<= 65,504` UTF-8 bytes) and
  `ExtendedXmpPacket`, `XmpPacket` is kept unchanged after verifying that its
  `x:xmpmeta` or `rdf:RDF` root contains a single top-level `HasExtendedXMP`
  attribute or text element matching the uppercase MD5 of `ExtendedXmpPacket`.

### Automatic XMP partitioning

Set `AutoPartitionXmp = true` (default `false`) to automatically split an
`XmpPacket` larger than `65,504` UTF-8 bytes into standard and extended XMP
(`ExtendedXmpPacket` must be empty):

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

- Packets `<= 65,504` UTF-8 bytes are written unchanged in a single standard
  `APP1` marker.
- Oversized packets are parsed (`x:xmpmeta` or `rdf:RDF` root with empty-subject
  `rdf:Description` children; no DTDs, `rdf:ID`, `rdf:nodeID`, nested `xpacket`
  instructions, or pre-existing `HasExtendedXMP` links) and reserialized without
  the outer `xpacket` wrapper.
- By default, all properties move into the extended XMP chunks and the generated
  standard packet holds only `xmpNote:HasExtendedXMP`.
- Set `StandardXmpProperties` (requires `AutoPartitionXmp = true`) to keep
  specified top-level `(NamespaceUri, LocalName)` properties in the standard
  `APP1` packet so basic XMP readers can see them. The selected properties plus
  envelope and digest link must fit within `65,504` UTF-8 bytes.

## ICC profiles

Set `IccProfile` to embed raw ICC profile bytes in numbered `APP2` chunks (up to
`16,707,345` bytes across 255 chunks of `65,519` bytes, per
[ICC.1 Annex B.5](https://www.color.org/ICC1V42old.pdf)).

- When `SourceIccProfile` is empty, `IccProfile` is embedded unchanged without
  transforming pixel values (and is opaque unless
  [CICP transfer lookup](quantization.md#transfer-function-quantization-tuning)
  is enabled).
- When `SourceIccProfile` is set, the encoder converts input pixels from
  `SourceIccProfile` into `IccProfile` (or into a default sRGB / grayscale /
  scaled XYB profile when `IccProfile` is empty); see
  [source ICC color management](encoding.md#source-icc-color-management).

### Built-in ICC profile factories (`JpegliIccProfiles`)

All `JpegliIccProfiles` factory methods generate deterministic ICC v4.4 profiles
in managed code (matching upstream `jpegli_cms_internal.h`) and return a fresh
`byte[]` on each call. Attach the returned array to `IccProfile` (or
`SourceIccProfile`) explicitly.

Every RGB and grayscale factory has an overload accepting `IccRenderingIntent`
(`Perceptual` by default, `RelativeColorimetric`, `Saturation`, or
`AbsoluteColorimetric`), which sets the profile header intent, upstream
description string, and ICC MD5 profile ID. (`CreateXybProfile()` is
perceptual-only.)

#### Named RGB and XYB profile factories

| Factory method | Primaries & white point | Transfer curve | CICP tag | Size (perceptual) | Notes |
| --- | --- | --- | --- | ---: | --- |
| `CreateSrgbProfile([intent])` | sRGB / BT.709, D65 | Piecewise sRGB | `[1, 13, 0, 1]` | 504 B | Standard gamma-encoded sRGB for RGB or YCbCr output. |
| `CreateDisplayP3Profile([intent])` | P3, D65 | Piecewise sRGB | `[12, 13, 0, 1]` | 516 B | Display P3 for RGB or YCbCr output. |
| `CreateLinearSrgbProfile([intent])` | sRGB / BT.709, D65 | Linear (identity) | `[1, 8, 0, 1]` | 536 B | Linear-light sRGB samples. |
| `CreateLinearDisplayP3Profile([intent])` | P3, D65 | Linear (identity) | `[12, 8, 0, 1]` | 536 B | Linear-light P3 samples (`RGB_D65_DCI_Per_Lin` description). |
| `CreateLinearRec2020Profile([intent])` | Rec.2020, D65 | Linear (identity) | `[9, 8, 0, 1]` | 536 B | Linear-light Rec.2020 samples. |
| `CreateRec709Profile([intent])` | Rec.709, D65 | BT.709 parametric | `[1, 1, 0, 1]` | 536 B | Full-range BT.709 samples. |
| `CreateRec2020Profile([intent])` | Rec.2020, D65 | BT.709 parametric | `[9, 1, 0, 1]` | 536 B | Full-range Rec.2020 with BT.709 curve. |
| `CreateRec709DisplayP3Profile([intent])` | P3, D65 | BT.709 parametric | `[12, 1, 0, 1]` | 536 B | Display P3 primaries with BT.709 curve. |
| `CreateSrgbRec2020Profile([intent])` | Rec.2020, D65 | Piecewise sRGB | `[9, 13, 0, 1]` | 536 B | Rec.2020 primaries with sRGB curve. |
| `CreateDciP3Profile([intent])` | P3, DCI `(0.314, 0.351)` | Pure gamma 2.6 | `[11, 17, 0, 1]` | 520 B | Theatrical DCI-P3 RGB samples. |
| `CreateRec2100PqProfile([intent])` | Rec.2020, D65 | PQ + 9×9×9 Lab LUT | `[9, 16, 0, 1]` | 4,344 B | Rec.2100 PQ with BT.2408 (10,000 -> 250 nit) tone/gamut LUT in `A2B0` and identity `B2A0`. CICP code `16`. |
| `CreateRec2100HlgProfile([intent])` | Rec.2020, D65 | HLG + 9×9×9 Lab LUT | `[9, 18, 0, 1]` | 4,344 B | Rec.2100 HLG with 300 -> 80 nit OOTF and gamut LUT in `A2B0` and identity `B2A0`. CICP code `18`. |
| `CreateGammaSrgbProfile(gamma, [intent])` | sRGB / BT.709, D65 | Pure power `x^gamma` | None | ~508 B | Decoding exponent `gamma` in `1..8192` (`1 / gamma` encoding exponent). |
| `CreateGammaDisplayP3Profile(gamma, [intent])` | P3, D65 | Pure power `x^gamma` | None | ~508 B | Decoding exponent `gamma` in `1..8192`. |
| `CreateGammaRec2020Profile(gamma, [intent])` | Rec.2020, D65 | Pure power `x^gamma` | None | ~508 B | Decoding exponent `gamma` in `1..8192`. |
| `CreateXybProfile()` | Scaled XYB -> D50 XYZ | 3D CLUT + cubic + matrix | None | 720 B | Required display profile for `JpegColorSpace.Xyb` (intensity target 255; identity `B2A0`). |

#### Named grayscale profile factories

Grayscale profiles retain their source media white tag, use a D50 XYZ PCS header,
and omit chromatic-adaptation (`chad`) and `cicp` tags (so CICP lookup returns
`2`; set `CicpTransferFunction` explicitly when encoding PQ or HLG grayscale).

| Factory method | Media white | Decoding curve (`kTRC`) | Size (perceptual) |
| --- | --- | --- | ---: |
| `CreateSrgbGrayscaleProfile([intent])` | D65 | Piecewise sRGB parametric curve | 328 B |
| `CreateLinearSrgbGrayscaleProfile([intent])` | D65 | Linear identity curve | 328 B |
| `CreateRec709GrayscaleProfile([intent])` | D65 | BT.709 parametric curve | 328 B |
| `CreateDciGrayscaleProfile([intent])` | DCI `(0.314, 0.351)` | Pure gamma 2.6 | 328 B |
| `CreateRec2100PqGrayscaleProfile([intent])` | D65 | 64-entry untone-mapped PQ table (10,000 nits) | 436 B |
| `CreateRec2100HlgGrayscaleProfile([intent])` | D65 | 64-entry untone-mapped HLG table | 436 B |
| `CreateGammaGrayscaleProfile(gamma, [intent])` | D65 | Pure power `x^gamma` (`gamma` in `1..8192`) | ~324 B |

#### Generic and custom-chromaticity factories

To combine any primaries, white point, transfer function, and rendering intent:

- `CreateRgbProfile(primaries, whitePoint, transferFunction, [renderingIntent])`
- `CreateGrayscaleProfile(whitePoint, transferFunction, [renderingIntent])`
- `CreateGammaRgbProfile(primaries, whitePoint, gamma, [renderingIntent])`
- `CreateGammaGrayscaleProfile(whitePoint, gamma, [renderingIntent])`

Parameters accept either the built-in enums (`IccPrimaries`: `Srgb`,
`DisplayP3`, `Rec2020`; `IccWhitePoint`: `D65`, `EqualEnergy`, `Dci`;
`IccTransferFunction`: `Rec709`, `Linear`, `Srgb`, `Pq`, `Dci`, `Hlg`) or custom
CIE `xy` coordinates via `IccRgbPrimaries` and `IccChromaticity` (white points
require finite `0 <= X <= 1` and `0 < Y <= 1`; singular matrices or values
outside ICC fixed-point range are rejected):

```csharp
byte[] profile = JpegliIccProfiles.CreateRgbProfile(
    IccPrimaries.DisplayP3, IccWhitePoint.EqualEnergy, IccTransferFunction.Linear);

var adobeRgbPrimaries = new IccRgbPrimaries(
    new(0.640, 0.330), new(0.210, 0.710), new(0.150, 0.060));
byte[] customProfile = JpegliIccProfiles.CreateGammaRgbProfile(
    adobeRgbPrimaries, new IccChromaticity(0.3127, 0.3290), 563.0 / 256.0);
```

Named D65 RGB combinations and P3 with DCI white include `cicp` tags and use the
9×9×9 tone/gamut-mapped Lab LUT for PQ and HLG; other white-point/primary
combinations and custom-coordinate overloads omit `cicp` and use 64-entry
untone-mapped PQ/HLG decoding curves with XYZ PCS.

## IPTC, comments, and raw markers

- **`IptcData`:** Raw IPTC-IIM dataset bytes (up to `65,506` bytes; empty omits
  IPTC). The encoder wraps them in a single Photoshop `APP13` marker (`Photoshop 3.0\0`,
  `8BIM` resource `0x0404`, big-endian length, and even-byte padding) per
  [Adobe's image resource format](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/).
- **`Comment`:** Optional string written as UTF-8 bytes without a terminator in
  a single `COM` (`0xFE`) marker (up to `65,533` UTF-8 bytes).
- **`MetadataMarkers`:** Optional list of raw `JpegMetadataMarker(byte MarkerCode, ReadOnlyMemory<byte> Data)`
  segments for `APP0`..`APP15` (`0xE0`..`0xEF`) and `COM` (`0xFE`), up to
  `65,533` bytes per marker, matching upstream `jpegli_write_marker`:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    MetadataMarkers =
    [
        new(0xE3, File.ReadAllBytes("application-metadata.bin")), // APP3
        new(0xFE, new byte[] { 0x63, 0x61, 0x66, 0xE9 }) // Raw COM
    ]
});
```

## JFIF density

`DensityUnit` (`Unspecified` by default for aspect ratio, `DotsPerInch`, or
`DotsPerCentimeter`), `HorizontalDensity` (`1..65535`, default `1`), and
`VerticalDensity` (`1..65535`, default `1`) configure the JFIF `APP0` header for
grayscale and YCbCr output ([JFIF specification](https://www.w3.org/Graphics/JPEG/jfif3.pdf)).
They do not resize pixels and are ignored for RGB and XYB output (which omit
JFIF `APP0`).

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

## Metadata order

Metadata segments are written immediately after `SOI` (and after JFIF `APP0` for
grayscale and YCbCr) in this order before quantization/Huffman tables and scans:

1. Exif `APP1`
2. Standard XMP `APP1`
3. Extended XMP `APP1` chunks
4. ICC `APP2` chunks
5. IPTC `APP13`
6. `Comment` `COM`
7. `MetadataMarkers` in caller list order
