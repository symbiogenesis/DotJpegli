# Encoding

[Documentation](README.md)

```csharp
using DotJpegli;

// Gamma-encoded 8-bit sRGB pixels, interleaved R, G, B.
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    Subsampling = ChromaSubsampling.Quarter,
    OptimizeHuffmanTables = true,
    AdaptiveQuantization = true
});
File.WriteAllBytes("output.jpg", jpeg);
```

## Pixel layouts and sampling

`PixelLayout` supports 8-bit `Rgb24` (default), `Bgr24`, `Rgba32`, `Bgra32`,
`Gray8`, `YCbCr24`, and already-scaled `Xyb24` (see
[scaled XYB input](#scaled-xyb-input)). Alpha channels are ignored; composite
translucent pixels before encoding.

Use `stride` for row padding (0 means tightly packed rows). The final row needs
only its pixel bytes, without trailing row padding. Negative strides are not
supported. The buffer must contain all rows; extra trailing bytes are allowed.

`Subsampling` selects the chroma component sampling for YCbCr output:

| Setting | Ratio | Chroma sample area | MCU size |
| --- | --- | --- | --- |
| `None` | 4:4:4 | 1×1 pixels | 8×8 pixels |
| `HalfHorizontal` | 4:2:2 | 2×1 pixels | 16×8 pixels |
| `Quarter` (default) | 4:2:0 | 2×2 pixels | 16×16 pixels |
| `HalfVertical` | 4:4:0 | 1×2 pixels | 8×16 pixels |
| `QuarterHorizontal` | 4:1:1 | 4×1 pixels | 32×8 pixels |
| `ThirdHorizontal` | — | 3×1 pixels | 24×8 pixels |
| `ThirdHorizontalHalfVertical` | — | 3×2 pixels | 24×16 pixels |
| `QuarterVertical` | — | 1×4 pixels | 8×32 pixels |
| `QuarterHorizontalHalfVertical` | 4:1:0 | 4×2 pixels | 32×16 pixels |
| `HalfHorizontalQuarterVertical` | — | 2×4 pixels | 16×32 pixels |
| `ThirdVertical` | — | 1×3 pixels | 8×24 pixels |
| `HalfHorizontalThirdVertical` | — | 2×3 pixels | 16×24 pixels |

Grayscale output always uses 1×1 sampling and 8×8 MCUs. Color subsampling modes
average converted samples using box filters after optional smoothing, with
replicated image edges. Only 4:2:0 applies Jpegli's specialized quantization-table
compensation; all other modes use the same perceptual tables as 4:4:4 and 4:2:2.
Preset color MCUs contain at most 10 blocks (including one block per chroma
component).

The three-row modes (`ThirdVertical` and `HalfHorizontalThirdVertical`) use
dedicated `1×3` and `2×3` kernels that average horizontally first, then sum all
three rows from top to bottom and multiply by `1f / 3f`. (The pinned upstream
selector routes vertical factor 3 to its two-row kernels; DotJpegli includes all
three rows as the sample area specifies.)

### Custom sampling factors

Set `SamplingFactors` to override `Subsampling` with explicit per-component
block counts `(Horizontal, Vertical)` per color MCU. For example, this keeps
luma at full resolution, averages Cb over 1×2 pixels, and averages Cr over 2×2
pixels:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    SamplingFactors = new(
        Luma: new(2, 2),
        Cb: new(2, 1),
        Cr: new(1, 1))
});
```

- Each factor must be in `1..4`, and the sum of `Horizontal * Vertical` across
  all three components must be at most 10.
- The largest horizontal and vertical factors across the three components set
  the MCU size (`8 * maxHorizontal` by `8 * maxVertical` pixels).
- A component's sample area is `maxHorizontal / component.Horizontal` by
  `maxVertical / component.Vertical` pixels.
- Invalid factors are rejected before output, including for grayscale; valid
  color factors are ignored when encoding grayscale. `Subsampling` must still be
  a valid enum value when overridden.
- Only the exact `Y=2×2, Cb=1×1, Cr=1×1` scheme uses Jpegli's 4:2:0 quantization
  compensation.
- **Fractional ratios:** Ratios such as `new(new(3, 1), new(2, 1), new(1, 1))`
  (averaging Cb over 1.5×1 pixels) use area-weighted averaging with exact
  rational boundaries. Fractional sampling is a managed extension: decoders like
  [StbImageSharp](https://github.com/StbSharp/StbImageSharp/blob/master/src/StbImage.Generated.Jpg.cs)
  and [libjpeg-turbo](https://github.com/libjpeg-turbo/libjpeg-turbo/blob/main/src/jdsample.c)
  reject non-integer ratios, so use integer ratios when targeting standard JPEG
  viewers.
- **Subsampled luma:** Luma can use smaller factors than Cb or Cr (for example,
  `new(new(1, 1), new(2, 2), new(1, 1))`). Subsampling the analyzed component
  automatically disables adaptive quantization, matching upstream.

## Streams and limits

The stream overload takes a writable `Stream` as its first argument. It writes
sequentially, does not seek, and leaves the stream open. Cancellation (checked
between metadata segments and MCUs via `CancellationToken`) or I/O failures may
leave a partial JPEG in the stream.

Image dimensions must be in `1..65535` and `width * height` must not exceed
`MaxPixels` (100,000,000 by default). Keep input buffers and option memory
unchanged until `Encode` returns. See
[parallel execution](compression.md#parallel-execution) for multi-threaded
encoding and [memory and source lifetime](compression.md#memory-and-source-lifetime)
for cache budgets.

## Higher-precision input

The `ReadOnlySpan<ushort>` and `ReadOnlySpan<float>` overloads accept
interleaved samples using `PixelComponentLayout` (`Rgb` by default, `Bgr`,
`Rgba`, `Bgra`, `Gray`, `YCbCr`, or `Xyb`). For these overloads, `stride` counts
**elements** (`ushort` or `float`), not bytes. Typed spans use native endianness.

```csharp
ushort[] rgb16 = ReadRgb16Samples(); // 0..65535 per component.
byte[] jpeg16 = JpegliEncoder.Encode(rgb16, width, height);

float[] gray = ReadNormalizedGraySamples(); // Finite 0..1 samples.
byte[] jpegFloat = JpegliEncoder.Encode(gray, width, height,
    PixelComponentLayout.Gray, options: new() { Quality = 90 });
```

- **16-bit (`ushort`) input** is scaled by `1.0 / 257.0` (so expanding `byte`
  values as `value * 257` produces identical JPEG output).
- **Float input** must be finite and in `0..1` by default (scaled by `255.0`),
  retaining fractional precision through color conversion, adaptive analysis,
  smoothing, downsampling, and the DCT until quantization.
- Output is always a lossy 8-bit JPEG (not 12-bit or lossless).

## Out-of-range floating-point input

Set `AllowOutOfRangeFloatSamples = true` to pass float RGB, grayscale, or direct
YCbCr samples outside `0..1` through to quantization without clamping them to
`0..255` first:

```csharp
byte[] jpeg = JpegliEncoder.Encode(floatPixels, width, height, options: new()
{
    Quality = 85,
    AllowOutOfRangeFloatSamples = true
});
```

NaN, infinities, and values that overflow when scaled are rejected. If any
consumed sample is outside `0..1`, a preflight pass verifies that all analysis
and DCT values remain finite and that every quantized coefficient fits within
8-bit JPEG's signed Huffman limits (`-1024..1023` for DC and `-1023..1023` for
AC in [ITU T.81, F.1.1.4 and F.1.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf)).
Whether an out-of-range image fits depends on quality, smoothing, and sampling;
unencodable inputs throw `ArgumentException` (`ParamName: "pixels"`) before any
output is written.

`SourceIccProfile` always requires normalized `0..1` float samples even when
`AllowOutOfRangeFloatSamples` is true.

## Source ICC color management

Set `SourceIccProfile` to convert input pixels from their source ICC profile
into `IccProfile` before smoothing, downsampling, adaptive analysis, and
quantization:

```csharp
byte[] jpeg = JpegliEncoder.Encode(displayP3Pixels, width, height, options: new()
{
    SourceIccProfile = JpegliIccProfiles.CreateDisplayP3Profile(),
    IccProfile = JpegliIccProfiles.CreateSrgbProfile(),
    ColorRenderingIntent = IccRenderingIntent.RelativeColorimetric,
    Quality = 90
});
```

- **Default destination profile:** If `IccProfile` is empty, the encoder
  automatically selects and embeds sRGB for RGB/YCbCr output, sRGB grayscale for
  grayscale output, or the built-in scaled XYB profile for XYB output.
- **Supported layouts:** RGB, BGR, and grayscale inputs (with or without alpha)
  at any precision (`byte`, `ushort`, or normalized `0..1` `float`). Direct
  YCbCr and already-scaled XYB inputs cannot use `SourceIccProfile`. For XYB
  output, pixels convert to sRGB first and then pass through the opsin transform
  (requiring the built-in XYB output profile).
- **Supported ICC profiles:** ICC v2 and v4 RGB and grayscale matrix/TRC
  profiles, identity/gamma/sampled `curv` curves, `para` functions 0–4, and
  `mft1`, `mft2`, `mAB`, and `mBA` LUT pipelines with XYZ or Lab PCS (8-bit
  `mft1` with XYZ PCS is rejected because its encoding is ambiguous). DeviceLink,
  abstract, named-color, CMYK, and iccMAX profiles are rejected. Built-in HDR
  and XYB profiles can serve as forward source profiles for RGB input, but their
  identity `B2A0` tags cannot serve as general inverse destination profiles.
- **Rendering intents:** `ColorRenderingIntent` defaults to
  `RelativeColorimetric`. `Perceptual` and `Saturation` select the matching LUT
  tables (falling back to perceptual or matrix/TRC when absent);
  `AbsoluteColorimetric` scales by source/destination media white. Converted
  destination samples are clamped to `0..1` without black-point compensation.
- **Caching:** Converted planes are cached within the
  [converted-sample budget](compression.md#memory-and-source-lifetime) so
  multi-pass encodes do not repeat nonlinear transforms.

## Direct YCbCr input

Use `PixelLayout.YCbCr24` (`byte`) or `PixelComponentLayout.YCbCr` (`ushort` /
`float`) for interleaved full-resolution samples already in JPEG's full-range
BT.601 YCbCr space. The encoder skips RGB-to-YCbCr conversion and passes the
samples directly to smoothing, chroma subsampling, and the DCT:

```csharp
byte[] jpeg = JpegliEncoder.Encode(ycbcrPixels, width, height,
    PixelLayout.YCbCr24, options: new() { Subsampling = ChromaSubsampling.None });
```

- **Neutral chroma:** `128` for `byte` (`0..255`), `32896` (`128 * 257`) for
  `ushort` (`0..65535`), and `128f / 255f` for `float` (`0..1`). Convert
  limited-range video levels (`16..235`) or non-BT.601 matrices before encoding.
- **Subsampling:** Input must be interleaved Y, Cb, Cr at full resolution (not
  planar or pre-subsampled NV12/YUV420). `Subsampling` defaults to `Quarter`
  (4:2:0); set `ChromaSubsampling.None` to keep 4:4:4 chroma.

## Output color space

Set `ColorSpace` (`JpegColorSpace`) to control the components stored in the JPEG:

| `ColorSpace` | Stored components | Behavior |
| --- | --- | --- |
| `Automatic` (default) | Grayscale, XYB, or YCbCr | Grayscale for grayscale input, XYB for scaled XYB input, and YCbCr for RGB/BGR/YCbCr input. |
| `Grayscale` | 1 component (`Y`, 1×1) | Converts RGB/BGR to luminance or takes `Y` from direct YCbCr input (ignoring Cb/Cr). |
| `YCbCr` | 3 components (`Y`, `Cb`, `Cr`) | Converts RGB/BGR to YCbCr, keeps direct YCbCr, or pads grayscale input with neutral chroma (`128`). |
| `Rgb` | 3 components (`R`, `G`, `B`) | Stores untransformed RGB (or replicates grayscale across R/G/B). Rejects direct YCbCr input. |
| `Xyb` | 3 components (`R`, `G`, `B` IDs storing scaled `X`, `Y`, `B-Y`) | Converts sRGB/grayscale to scaled XYB or accepts already-scaled XYB input. Requires a matching XYB ICC profile. |

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

### RGB output details

- Uses `R`, `G`, `B` component identifiers, omits JFIF APP0 and Adobe APP14
  markers (so JFIF density settings have no effect), and defaults to 1×1
  sampling (ignoring the `Subsampling` preset).
- Custom `SamplingFactors` map `Luma`, `Cb`, `Cr` to `R`, `G`, `B`. Adaptive
  quantization analyzes the green channel (with uniform `0.5` AC bias) and
  disables itself if green is subsampled.
- Uses one shared Annex K luma quantization table and one shared pair of Huffman
  tables by default, ignoring `UseStandardQuantizationTables` and CICP transfer
  hints.

## sRGB-to-XYB output

Set `ColorSpace = JpegColorSpace.Xyb` to convert gamma-encoded sRGB (or
grayscale) input into Jpegli's perceptual scaled XYB color space:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ColorSpace = JpegColorSpace.Xyb,
    IccProfile = JpegliIccProfiles.CreateXybProfile(),
    Quality = 90
});
```

- **Required ICC profile:** XYB output stores scaled `X`, `Y`, and `B-minus-Y`
  channels under `R/G/B` JPEG component IDs and omits JFIF/Adobe markers. A
  non-empty XYB ICC profile is required so viewers can decode the channels back
  to RGB (unless `SourceIccProfile` is set, which embeds the built-in XYB
  profile automatically when `IccProfile` is empty). Use
  `JpegliIccProfiles.CreateXybProfile()` to create the built-in 720-byte ICC
  v4.4 profile.
- **Transform:** Input samples are normalized to `0..1`, linearized from sRGB
  (`MathF.Pow`), transformed via Jpegli's biased opsin absorbance matrix and
  cube roots (`MathF.Cbrt`) at intensity target 255, and scaled into `X`, `Y`,
  and `B-minus-Y`.
- **Sampling and quantization:** Defaults to `X=2×2, Y=2×2, B-minus-Y=1×1` (a
  16×16 MCU with 9 blocks that subsamples only `B-minus-Y`), three perceptual
  XYB quantization tables, and shared Huffman table 0. Adaptive quantization
  analyzes `Y` and disables itself if `Y` is subsampled.

## Scaled XYB input

Use `PixelLayout.Xyb24` (`byte`) or `PixelComponentLayout.Xyb` (`ushort` /
`float`) when input samples are **already** transformed and scaled into
interleaved `scaled X, scaled Y, scaled B-minus-Y` (matching upstream
`ScaleXYBRow`):

```csharp
byte[] jpeg = JpegliEncoder.Encode(scaledXybSamples, width, height,
    PixelComponentLayout.Xyb, options: new()
    {
        IccProfile = JpegliIccProfiles.CreateXybProfile(),
        Quality = 90
    });
```

This path skips sRGB linearization, the opsin transform, and XYB scaling, and
feeds the samples directly into the XYB sampling and quantization pipeline
above. A non-empty `IccProfile` (such as `JpegliIccProfiles.CreateXybProfile()`)
is required, and scaled XYB input cannot be converted to other output color
spaces.
