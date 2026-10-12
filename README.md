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

`PixelLayout` also supports BGR24, RGBA32, BGRA32 and Gray8. Alpha is ignored;
composite translucent pixels before encoding. Use `stride` for row padding. The
last row needs only its pixel bytes, without trailing padding. Negative strides
are not supported. The input must contain every row; extra trailing bytes are
allowed.

The stream overload takes a writable stream as its first argument. It leaves
the stream open and does not seek. Cancellation and I/O failures may leave a
partial JPEG. Pass a `CancellationToken` for cancellation between MCUs. Dimensions
must be 1..65535 and fit `MaxPixels` (100 million by default).

Set `RestartInterval` to 1..65535 to insert restart markers after that many
minimum coded units (MCUs); zero, the default, disables them. MCUs span 8x8 pixels
for grayscale and 4:4:4, 16x8 for 4:2:2, and 16x16 for 4:2:0. Partial edge MCUs
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

With `OptimizeHuffmanTables = true` (the default), initial progressive AC scans combine
trailing zero coefficients across consecutive blocks into end-of-block (EOB)
runs, up to 32,767 blocks per run. Runs finish before nonzero AC coefficients,
restart markers and scan boundaries. With optimization disabled, the standard
Huffman tables use only single-block EOB symbols. Aggregation changes the entropy coding,
preserving the quantized coefficients and decoded pixels. AC refinement scans
emit one-block EOB symbols and their correction bits, even with optimized tables;
they retain at most one block's correction bits. DC refinement uses raw bits.

In progressive AC scans, a single component block counts as one MCU, so
`RestartInterval` can produce more markers than in the DC scan. Restart counting
and the RST0..RST7 sequence begin again in each scan.

Spatial adaptive quantization is enabled by default. It analyzes luma texture,
brightness and neighboring regions to adjust coefficient zeroing thresholds.
Set `AdaptiveQuantization = false` to use uniform quantization. Upstream sampling
arithmetic and dummy-block coding apply to both modes; partially filled MCUs can
produce different JPEG bytes from earlier port versions.

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
JPEG/JFIF; RGB and grayscale input; 4:4:4,
4:2:2 and 4:2:0 upstream scalar box sampling with edge replication and empty
dummy-block coding that preserves DC predictors at MCU edges; recursive scalar DCT; Jpegli
quality-to-distance and perceptual Y/Cb/Cr quantization tables; scalar adaptive
quantization with local differences, fuzzy erosion, masking, high-frequency and
gamma modulation, quality dampening and frequency-specific coefficient thresholds;
optimized length-limited canonical Huffman coding;
byte stuffing and marker output; optional MCU restart intervals with DRI and
cycling RST markers, including predictor resets during Huffman optimization;
initial progressive EOB-run aggregation with optimized Huffman tables;
optional DC and AC refinement scans with bounded correction-bit storage.

Pending: refinement EOB-run aggregation, optional input smoothing and other sampling ratios,
XYB and ICC profiles, higher precision input,
metadata options, SIMD/parallel paths, and decoder functionality.
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

`Directory.Build.targets` rejects `AllowUnsafeBlocks=true`, including a command
line override. Library and tests compile with warnings treated as errors.

## License

BSD-3-Clause. See [LICENSE](LICENSE), [LICENSE.jpegli](LICENSE.jpegli) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for upstream provenance and notices.
