# Encoder port status

[Documentation](README.md)

## Progress summary

**26 of 27 tracked capability areas implemented (96.3%).**
**1 capability area pending.**

Each row in the implemented and pending tables counts once, with equal weight.
Progress is calculated as `implemented / total × 100`, rounded to one decimal
place. These counts cover the documented encoder implementation roadmap;
quality, compression and performance validation are tracked separately below.

## Scope and upstream source

DotJpegli is an experimental, fully managed C# JPEG encoder for .NET 10,
based on [Google Jpegli](https://github.com/google/jpegli). Unsafe compilation
is disabled, with no native binaries, P/Invoke, pointer code, `Unsafe` APIs
or runtime NuGet dependencies.

The scope is **encoder only**. Decoder implementation is outside the project
and excluded from the progress count. Generated JPEGs use existing decoders;
see [encoding](encoding.md) for compatibility requirements of optional modes.
Port completion also requires output compatibility, quality/compression
validation and performance validation.

The upstream source is pinned to
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20),
retrieved on October 12, 2026. The port adapts Google's BSD-licensed C++
implementation directly. See [upstream.json](upstream.json) for machine-readable
provenance and implementation notes, and [third-party notices](../THIRD-PARTY-NOTICES.md)
for source attribution.

## Implemented capabilities

| Capability area | Status | Coverage |
| --- | --- | --- |
| Sequential coding | Implemented | Baseline and extended sequential JPEG, with interleaved or separate component scans. |
| Progressive coding | Implemented | Spectral selection and successive approximation, including DC/AC refinement with bounded correction-bit storage. |
| Progression presets | Implemented | Upstream level-0/1/2 scan presets with sampling-based DC interleaving. |
| Caller-supplied scan scripts | Implemented | Component subsets, spectral schedules, changing refinement bands and DC groups, with progression validation and per-scan restart geometry. |
| Huffman coding | Implemented | Optimized, length-limited canonical tables; progressive initial/refinement EOB-run aggregation. |
| Bitstream and marker output | Implemented | Bit packing, byte stuffing, entropy padding and JPEG marker framing. |
| Restart markers | Implemented | Fixed MCU and row-based intervals, per-scan DRI updates, cycling RST markers and DC predictor resets during coding and Huffman optimization. |
| Input and output color spaces | Implemented | RGB/BGR, alpha-bearing, grayscale and interleaved full-range YCbCr layouts; grayscale, YCbCr and explicit RGB output. |
| Input precision and float range | Implemented | Byte, 16-bit and float samples; fractional precision through quantization; optional out-of-range floats with finite-computation and eight-bit coefficient preflight. |
| Integer component sampling | Implemented | Twelve chroma presets, custom asymmetric factors and subsampled luma, with at most ten blocks per MCU, replicated edges and dummy blocks preserving DC predictors. |
| Fractional sampling ratios | Implemented | Custom ratios such as 3:2 and 4:3 use area-weighted averaging, rational block geometry, replicated edges and per-scan restarts. Requires a compatible decoder; extends the pinned upstream. |
| Input smoothing | Implemented | Upstream 3×3 filter before sampling, with adaptive analysis of unsmoothed samples. |
| DCT and managed SIMD | Implemented | Recursive scalar DCT, automatic managed vector acceleration and scalar fallback, retaining bounded stack storage and overlapping-span support. |
| SIMD beyond the DCT | Implemented | Managed vector RGB-to-YCbCr conversion, unsmoothed integer-ratio downsampling and AC quantization, with automatic hardware dispatch, bounded storage and scalar fallbacks. |
| Parallel encoding | Implemented | Opt-in worker limits for pixel conversion, downsampling and DCT across each MCU row; ordered adaptive analysis, quantization and entropy coding preserve serial JPEG bytes. |
| Quality and distance control | Implemented | Public quality-to-distance mapping and direct perceptual-distance control. |
| Perceptual and adaptive quantization | Implemented | Jpegli Y/Cb/Cr tables; local differences, fuzzy erosion, masking, high-frequency/gamma modulation, quality dampening and frequency-specific thresholds. |
| Standard quantization tables | Implemented | Conventional Annex K luma/shared-chroma matrices with upstream distance scaling and retained adaptive quantization. |
| Custom and extended quantization | Implemented | Shared/separate selectors, snapshots and validation; optional quantizers up to 32767, mixed eight/sixteen-bit DQT tables and eight-bit SOF1 output. |
| Transfer-function tuning | Implemented | PQ/HLG CICP hints and opt-in raw ICC transfer lookup, preserving profile bytes and source samples. |
| Metadata embedding | Implemented | Exif TIFF, standard/extended XMP, chunked ICC, Photoshop IPTC-IIM, UTF-8 comments, JFIF densities and raw APP0–APP15/COM markers with caller order preserved. |
| Structured metadata editing | Implemented | Opt-in Exif directory/tag, XMP property/array/language and IPTC dataset editors; explicit reconciliation of dimensions, orientation, physical density and supplied descriptive fields. Unsupported offset-sensitive Exif structures fail explicitly; see the metadata guide for supported formats and restrictions. |
| Automatic XMP partitioning | Implemented | Oversized packets move to extended XMP; selected top-level properties can remain in the standard packet. |
| XYB encoding and profile | Implemented | Already scaled XYB input and sRGB/grayscale conversion, upstream XYB quantizers/sampling/Y-based analysis, and a managed scaled XYB output profile. |
| Named and generic ICC profiles | Implemented | RGB/grayscale sRGB, linear, P3, Rec.709/2020, DCI, PQ/HLG and pure-gamma profiles; every named combination of three whites, three RGB primary sets and six transfers, plus gamma. |
| Custom ICC chromaticities and intents | Implemented | Caller-supplied CIE xy primaries/whites, primary-matrix and Bradford adaptation, storage validation and upstream descriptions; four selectable RGB/grayscale rendering intents. |

See [encoding](encoding.md), [compression](compression.md),
[quantization](quantization.md) and [profiles and metadata](metadata.md) for API
contracts, examples and detailed restrictions. [Development and validation](development.md)
describes the regression coverage behind these implementation statuses.

## Pending capabilities

| Capability area | Status | Remaining work |
| --- | --- | --- |
| Source ICC color management | Pending | Convert arbitrary source profiles into the chosen output color space. Current profile attachment describes output samples. |

## Validation and compatibility

| Validation area | Current status |
| --- | --- |
| Encoder functionality and output compatibility | Focused regressions check coefficients, marker framing, independent decoding, stream behavior and validation. Optional modes retain the reader requirements in the API guides. |
| Quality and compression parity | Not established. A matched-corpus comparison with the pinned upstream encoder remains necessary. |
| Performance parity | Not established. Parallel execution is available; speed claims require CI performance measurements. |

Output remains **lossy eight-bit JPEG**, including at quality 100. Jpegli's quality
scale is not interchangeable with libjpeg's. Byte-identical upstream output,
compression parity and speed parity are not claimed. Google's
[comparison study](https://arxiv.org/abs/2403.18589) describes results in its tested
conditions and does not establish a universal smallest-encoder ranking.

The managed DCT uses standard JPEG coefficient units; upstream's output is one
eighth of those units, with the adaptation accounted for during quantization.
Its `System.Numerics.Vector<float>` path preserves scalar evaluation order and
fused multiply-add rounding. Hardware acceleration requires runtime support and
a vector width that divides an eight-sample row; other cases use the scalar fallback.

Managed vectors also accelerate RGB-to-YCbCr conversion, unsmoothed integer-ratio
downsampling and AC quantization. Conversion preserves source normalization,
scalar evaluation order and chroma FMA rounding; downsampling preserves pairwise
and composed averaging; quantization preserves coefficient division, adaptive
thresholds and rounding rules. Smoothing and nonlinear RGB-to-XYB conversion
retain scalar paths. These changes do not establish a measured speed improvement
or upstream performance parity.

`MaxDegreeOfParallelism` defaults to one worker; positive limits or automatic
selection (`-1`) enable parallel conversion, sampling and DCT when the effective
limit exceeds one. Parallel execution copies the consumed source storage once
and retains one MCU row of DCT coefficients, with no image-sized coefficient
buffer. All workers finish before encoding returns. Adaptive analysis,
quantization and stream writes remain ordered. This execution option extends
the managed port and does not establish a measured speed improvement.

ICC profiles require explicit assignment and describe the stored samples.
XYB display requires a matching profile and a reader that applies it; XYB profiles
remain perceptual-only. Generic PQ/HLG profiles use Lab tone-mapping LUTs only
for CICP-representable RGB combinations; other RGB and grayscale variants use
untone-mapped decoding curves. Custom-coordinate profiles omit CICP, including
coordinates matching named encodings. Profile generation and embedding do not
establish color-managed JPEG quality or HDR conformance.
