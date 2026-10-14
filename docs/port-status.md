# Encoder port status

[Documentation](README.md)

## Scope and upstream source

A fully managed C# JPEG encoder targeting .NET 10. This is an experimental incremental
port of the encoder in [Google Jpegli](https://github.com/google/jpegli), with unsafe compilation
disabled and no native binaries, P/Invoke, pointer code, `Unsafe` APIs, or runtime
NuGet dependencies.

DotJpegli's scope is **encoder only**. Implementing or porting a JPEG decoder is
outside the project's scope and is not planned. Generated JPEGs are intended for
existing JPEG decoders; compatibility requirements for optional output modes are
documented in the [encoding guide](encoding.md). Port completion is judged on encoder functionality, output
compatibility, quality/compression validation, and performance.

The source is pinned to
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20),
retrieved on October 12, 2026. This port uses Google's BSD-licensed C++
implementation directly.

Machine-readable provenance and implementation notes are in [upstream.json](upstream.json).
See [third-party notices](../THIRD-PARTY-NOTICES.md) for source attribution.

## Implemented and pending work

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
Raw APP0..APP15 and COM markers complete other metadata embedding, matching
the pinned upstream's generic marker API. Caller-supplied payloads and order
are preserved, with descriptor snapshots and validation before output;
interpreting or editing metadata structures remains pending.
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

Generic RGB and grayscale factories complete every combination of the pinned
builder's three named white points (D65, equal-energy E and DCI), three RGB
primary sets and six named transfer functions, plus pure gamma and all four
rendering intents. PQ/HLG select the upstream Lab tone-mapping LUT only for
CICP-representable RGB combinations; other RGB and grayscale variants use
untone-mapped 64-entry decoding curves. Existing named factory bytes are preserved.

Custom RGB primaries and source white points complete the profile chromaticity
port. Generic RGB and grayscale factories accept caller-supplied CIE xy
coordinates with the same named transfers, pure gamma and four intents.
The managed path computes the pinned builder's primary matrix and Bradford
adaptation, checks singular matrices and ICC storage limits, and retains
upstream's custom-coordinate descriptions. Custom PQ/HLG profiles use
untone-mapped decoding curves without CICP, including coordinates matching
named encodings. Existing named profile bytes are preserved.

The DCT SIMD path transforms independent columns and rows with runtime-sized
`System.Numerics.Vector<float>` lanes, preserving the scalar evaluation order
and fused multiply-add rounding. Hardware acceleration is selected automatically
when the vector width divides an eight-sample row; otherwise the scalar path
is used. Both paths retain bounded stack storage, overlapping-span support and
standard JPEG coefficient units. This completes the DCT SIMD path; broader SIMD
coverage and parallel encoding remain pending. No speed improvement is claimed
without CI performance measurements.

Pending encoder work: fractional sampling ratios,
arbitrary source ICC color management,
structured metadata editing, SIMD in other encoder stages and parallel encoding.
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
