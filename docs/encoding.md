# Encoding

[Documentation](README.md)

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

## Pixel layouts and sampling

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
Ratios may be fractional. A component's
sample area is `maxHorizontal / component.Horizontal` by
`maxVertical / component.Vertical`. The sum of `Horizontal * Vertical` over all
three components must be at most ten. Invalid factors are rejected before output,
including for grayscale output; valid color settings are ignored for grayscale.
The `Subsampling` enum must still be valid when overridden. Null `SamplingFactors`
uses the preset and preserves its JPEG bytes.

For example, `SamplingFactors = new(new(3, 1), new(2, 1), new(1, 1))`
averages Cb over 1.5x1 source pixels and Cr over 3x1 pixels. Fractional ratios
use area-weighted averaging of converted samples, horizontally then vertically,
with exact rational sample boundaries and replicated image edges. Smoothing,
when requested, precedes averaging. Component dimensions and noninterleaved
scan/restart counts round up the rational dimensions without truncating ratios.
Integer ratios retain the existing kernels and JPEG bytes.

Fractional sampling extends the pinned upstream encoder, which rejects these
ratios. It requires a decoder that supports nonintegral component upsampling;
[StbImageSharp](https://github.com/StbSharp/StbImageSharp/blob/master/src/StbImage.Generated.Jpg.cs)
and [libjpeg-turbo](https://github.com/libjpeg-turbo/libjpeg-turbo/blob/main/src/jdsample.c)
reject it in their usual pixel decoding paths. Use integer ratios when targeting
those readers.

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

## Streams and limits

The stream overload takes a writable stream as its first argument. It leaves
the stream open and does not seek. Cancellation and I/O failures may leave a
partial JPEG. Pass a `CancellationToken` for cancellation between MCUs. Dimensions
must be 1..65535 and fit `MaxPixels` (100 million by default).
Use `MaxDegreeOfParallelism` to opt into
[parallel execution](compression.md#parallel-execution), with identical JPEG
bytes and additional source-copy/row-buffer memory. The stream remains on the
calling thread and all workers finish before encoding returns.

## Higher-precision input

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
opsin transform; other output modes retain gamma-encoded samples unless
[source ICC conversion](#source-icc-color-management) is enabled. For conversion to XYB supply sRGB input and
a scaled XYB output profile; other modes use the color space described by the
attached profile. Typed spans contain native numeric
values; deserialize byte-oriented source data with its correct endianness first.

The output remains a lossy eight-bit JPEG. Higher-precision input retains more
information before coefficient quantization; it does not enable 12-bit JPEG or
lossless encoding. Stream behavior, cancellation, pixel limits and source lifetime
requirements are the same as for byte input. Samples are read on demand with
bounded working storage; no full-image conversion buffer is allocated. Float
validation adds one input pass, with cancellation checks between rows.

## Out-of-range floating-point input

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
mapping or HDR JPEG output. Source ICC conversion is enabled separately and
requires normalized input. Explicit XYB output
applies its sRGB linearization and opsin transform as described below. Decoders
can clamp reconstructed samples to their output range.

## Source ICC color management

Set `SourceIccProfile` to the profile describing the input samples and
`IccProfile` to the destination profile. Conversion runs before smoothing,
sampling, adaptive analysis and quantization, preserving fractional precision:

```csharp
byte[] jpeg = JpegliEncoder.Encode(displayP3Pixels, width, height, options: new()
{
    SourceIccProfile = JpegliIccProfiles.CreateDisplayP3Profile(),
    IccProfile = JpegliIccProfiles.CreateSrgbProfile(),
    ColorRenderingIntent = IccRenderingIntent.RelativeColorimetric,
    Quality = 90
});
```

An empty destination selects and embeds RGB sRGB for RGB/YCbCr output, grayscale
sRGB for grayscale output, or the built-in scaled XYB profile for XYB output.
`Automatic` still selects the JPEG output space from the input layout. Supply
an RGB destination for RGB/YCbCr output and a grayscale destination for grayscale
output. Source profile channels must match the RGB/BGR or grayscale input layout;
alpha and padding remain ignored. Direct YCbCr and already scaled XYB input
cannot use source ICC conversion. For XYB output the source is converted to
sRGB first, then passed through the existing opsin transform; only the built-in
scaled XYB destination profile is accepted in this mode.

The managed engine supports ICC v2/v4 RGB and grayscale matrix/TRC profiles,
identity, gamma and sampled `curv` curves, `para` functions 0–4, and
`mft1`, `mft2`, `mAB` and `mBA` LUT pipelines with XYZ or Lab PCS. Eight-bit
`mft1` with XYZ PCS is rejected because its encoding is implementation-specific.
LUT interpolation is linear for one input channel and trilinear for RGB.
Device-link, abstract, named-color, CMYK/multichannel, iccMAX and unsupported
transform types are rejected explicitly. The built-in HDR and XYB profiles'
compatibility `B2A0` tags are not inverse transforms and cannot serve as ordinary
destinations. Their forward transforms can describe RGB-layout source data.
See [ICC.1:2022](https://www.color.org/specification/ICC.1-2022-05.pdf) for the
profile and PCS encodings.

`ColorRenderingIntent` defaults to relative colorimetric. Perceptual and
saturation select their corresponding LUTs; missing intent tables fall back to
the perceptual table or matrix/TRC transform. Absolute colorimetric uses the
relative transform with source/destination media-white scaling. Matrix/TRC
profiles otherwise share their transform across intents. No black-point
compensation or additional gamut mapping is applied; destination samples are
clipped to 0..1. A source LUT can itself contain a tone/gamut transform.

Consumed float samples must be finite and in 0..1, even with
`AllowOutOfRangeFloatSamples` enabled. Malformed, mismatched and unsupported
profiles and nonfinite conversion results fail before output. Profiles are
parsed into immutable owned transforms and the destination bytes are copied
before stream writes; do not mutate profile memory while encoding starts.
The source profile is not embedded. Conversion uses bounded per-pixel storage
without a full-image converted buffer and is shared safely by parallel workers.
Cancellation is checked during profile preparation and between image rows.
Leaving `SourceIccProfile` empty preserves the existing JPEG coding and opaque
profile-attachment behavior. This capability does not establish quality,
compression or performance parity with upstream.

## Direct YCbCr input

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

## Output color space

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
with the usual factor range and ten-block MCU limits. Subsampled green disables
adaptive quantization. Other sampling, smoothing, dummy blocks, restart markers,
progressive scans, refinement and coefficient preflight use the existing paths.
All options are still validated, including ignored sampling presets and JFIF densities.

RGB output omits JFIF APP0 and Adobe APP14; the RGB component identifiers signal
the color space. JFIF density options therefore have no effect. Optional metadata
begins immediately after SOI in its usual order. Some JPEG readers have limited
support for RGB or subsampled RGB JPEGs. This option does not perform color
management by itself; use `SourceIccProfile` for conversion. Any attached profile
must describe the chosen output interpretation.

## sRGB-to-XYB output

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
embedded unchanged; it describes the stored channels. `SourceIccProfile` can
convert another RGB or grayscale source into sRGB before the XYB transform;
that path requires the built-in XYB output profile. Direct YCbCr input
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

## Scaled XYB input

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
