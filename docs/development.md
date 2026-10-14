# Development and validation

[Documentation](README.md)

## Build

The library project and its source are in `src/DotJpegli/`; tests are in
`tests/DotJpegli.Tests/`. Build the library from the repository root with:

```text
dotnet build src/DotJpegli/DotJpegli.csproj -c Release
```

## Focused tests and CI

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

See the [GitHub Actions workflow](../.github/workflows/ci.yml) for the hosted checks.

## Regression coverage

Raw-metadata regressions independently parse every supported APP/COM code,
network-byte-order lengths, zero and maximum payloads, sliced storage and
marker-looking payload bytes. They check duplicate and combined metadata order,
descriptor snapshots, default compatibility and rejection before output across
source precisions. Coding-byte and independent decoded-pixel comparisons use
unrecognized application payloads and raw comments across scan modes. Nonseekable
streams, cancellation between markers and I/O failures are also covered.

Custom-chromaticity validation compares 252 complete RGB/grayscale profiles
with independently compiled unchanged pinned C++ fixtures across six named
transfers, gamma 2.2 and all four intents. Cases include Adobe RGB, ACES AP0
imaginary primaries, custom D50/D60/D65 whites, named-valued custom coordinates
and scientific-notation descriptions. Independent primary-matrix and Bradford
oracles check adapted colorants; parsing checks tag alignment, shared curves,
source media white, omitted CICP/HDR LUTs and ICC identification. Encoding
checks verify exact APP2 embedding and unchanged coding across representative
source precisions, output color spaces, scan modes and nonseekable streams.
These checks validate profiles and sample preservation without establishing
a color-managed lossy JPEG quality comparison.

Built-in combination regressions compare all 288 named RGB/grayscale profiles
and 48 gamma-2.2 profiles across four intents with independently compiled pinned
C++ fixtures. Independent parsing checks media white, Bradford adaptation,
adapted colorants, decoding curves, CICP eligibility, HDR transform selection and ICC
identification. They cover existing factory bytes, fresh storage, invalid
parameters and exact APP2 embedding with unchanged coding across representative
source precisions, scan modes and streams. These checks validate profile
serialization and sample preservation without establishing color-managed lossy
JPEG quality.

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

SIMD color-conversion, downsampling and quantization regressions compare managed
vector and automatic dispatch results with the scalar implementations, including fractional
source normalization, chroma FMA rounding, adaptive threshold and coefficient
rounding boundaries, padded edges and scalar fallbacks. Complete JPEG SHA-256
fixtures captured before these paths were introduced check unchanged encoding
across byte, 16-bit and float input, color spaces, sampling, smoothing, custom
extended tables, scan modes, refinement and restarts. CI repeats focused SIMD
and encoder fixture checks with hardware intrinsics disabled. Performance
measurements remain CI-only and speed parity is not established.

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
