# Compression and scan options

[Documentation](README.md)

## Quality and distance

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

Null `Distance`, the default, uses the quality mapping.
A supplied distance overrides `Quality`, which must still be valid.
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

## Restart markers

Set `RestartInterval` to 1..65535 to insert restart markers after that many
minimum coded units (MCUs); zero, the default, disables them. MCUs span 8x8 pixels
for grayscale and 4:4:4; for other color modes, multiply each chroma sample area
dimension in the [sampling table](encoding.md#pixel-layouts-and-sampling) by eight. For example, 4:1:0 uses 32x16 pixels.
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

Zero, the default, uses `RestartInterval`. Positive
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

## Sequential scans

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
The output retains its sequential frame (SOF0 by default, SOF1 for wide quantizers or extended Huffman selectors), quantization tables, final
coefficients and decoded samples. Color encoding needs additional passes but
retains bounded working storage. JPEG reader support can vary.

The default `true` interleaves color components. Grayscale bytes
are unchanged with either value. False requires sequential output and rejects
`Progressive = true` before output, except when `ProgressiveLevel = 0` selects
sequential encoding. Positive progression levels are also rejected. Sampling,
source precision, smoothing, adaptive quantization, custom quantization tables,
metadata and stream behavior retain their usual contracts. This setting selects
a fixed script; `ScanScript` below provides explicit control. Neither promises
upstream byte, size or speed parity.

## Custom scan scripts

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
`SequentialInterleaveComponents = false`. Null uses the other scan controls.
The encoder copies the script and every component list before output; do not
modify them while encoding starts. Optimized grayscale/YCbCr encoding reuses
quantized coefficients across scans within a 64 MiB pooled-buffer budget, falling
back to row/block recomputation for larger images. It retains the existing input, quantization,
metadata and stream contracts. Single-component scans visit real blocks in
raster order; scans selecting two or three components use frame MCUs with
dummy blocks. Histograms and output share that traversal and restart interval.
Predictors, EOB runs and RST numbering reset per scan; DRI changes when the
interval changes. Final real coefficients are preserved. Extra scans increase
CPU work and do not guarantee smaller files or upstream byte parity.

## Progressive scans

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
The default, zero, uses full-precision initial scans. More refinement
rounds require more encoding passes and CPU time. This scan script does not reproduce
upstream Jpegli's default script or promise smaller files.

### Progression presets

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
encoding mode. Null, the default, uses the other progressive controls.
Negative levels are rejected; all values at least two use the same
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

### AC spectral bands and refinement

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
An empty list is invalid; null, the default, uses `[63]`.
A non-null list requires `Progressive = true`. The encoder copies and
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

The list requires `Progressive = true`, exactly one value per resolved AC
band, and values in 0..10. When `ProgressiveAcScanEnds` is null there is one
band, so a supplied depth list must have one value. Null depths use
`ProgressiveRefinementBits` for all bands.
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

### DC scan ordering

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
The default `true` interleaves color DC; grayscale bytes are
unchanged with either setting. For `C` components, DC depth `D` and band
depths `A[b]`, separate DC produces `C * (D + 1 + sum(A[b] + 1))` scans.
Final coefficients and decoded pixels are preserved. Color output needs
additional scans and passes; it does not promise smaller files or encoder-byte
parity with upstream. Working storage remains bounded, and existing typed-input,
stream, cancellation, smoothing and adaptive-quantization behavior applies.

## Huffman optimization

Grayscale and YCbCr output use the pinned upstream's greedy histogram clustering:
component DC histograms and per-scan/component AC histograms merge when combined
entropy and table-definition cost is lower. Baseline output uses at most two
tables of each class; progressive coding can replace four AC slots between scans.
Definitions share DHT markers, and each scan selects its actual active tables.
Table choices affect entropy bytes while preserving quantized coefficients and
decoded pixels. Explicit RGB/XYB output uses shared tables; disabling
optimization retains the standard tables.

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

## Adaptive quantization and smoothing

Spatial adaptive quantization is enabled by default. It analyzes luma texture,
brightness and neighboring regions to adjust coefficient zeroing thresholds.
Set `AdaptiveQuantization = false` to use uniform quantization. Subsampled luma
also disables adaptive analysis automatically, matching upstream. Upstream sampling
arithmetic and dummy-block coding apply to both modes, including partially filled MCUs.

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

## Parallel execution

Set `MaxDegreeOfParallelism` to a positive worker limit, or `-1` to use the
runtime's processor count:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    MaxDegreeOfParallelism = 4
});
```

The default, `1`, uses serial execution. Zero and
values below `-1` are rejected before output. When the effective limit exceeds
one, workers convert source samples, smooth/downsample components and compute
the DCT independently across each MCU row. Adaptive analysis, quantization,
Huffman frequency collection and entropy output remain ordered, producing the
same JPEG bytes as serial execution with the same settings. The option applies
to every source precision, output color space, sampling scheme and scan script.

Parallel execution shares cached converted samples when available; otherwise
it copies the consumed source storage once, including row padding and ignored
alpha but excluding trailing storage. It also buffers one MCU row of DCT
coefficients. Its additional memory is proportional to image width and, when
needed, the consumed source storage.
Keep the supplied pixels unchanged until encoding returns; adaptive analysis
and float preflight still read them. All workers finish synchronously before
encoding returns or throws. The destination stream is written only by the
calling thread. Cancellation and I/O failures retain the usual partial-output
contract. Thread scheduling and the extra copy can outweigh parallel work,
especially for small images; speed claims require CI performance measurements.

## Memory and source lifetime

Keep the input unchanged until encoding returns. The encoder retains converted
samples, optimized grayscale/YCbCr quantized coefficients and entropy tokens
within three independent 64 MiB budgets. Pooled allocation sizes count toward
the limits; token storage shares its budget across scans. Larger images and
exhausted token budgets use coefficient replay or row/block recomputation.
These budgets exclude caller pixels, output, metadata, histograms and ordinary
row/block scratch.

A single-worker optimized interleaved sequential YCbCr scan records entropy
tokens while transforming and quantizing blocks, avoiding the image-wide
coefficient cache and its subsequent traversal. If tokens exceed their budget,
emission recomputes blocks with the same prepared quantizers. Other optimized
grayscale/YCbCr scan layouts reuse quantized coefficients when that cache fits,
and otherwise recompute blocks in scan order. Optimized progressive output
collects frequencies from the initial DC and all AC scans before writing them. Histogram
storage grows with the bounded scan-context count, independently of image area;
spatial analysis uses bounded neighboring rows.

Single-worker optimized, unsmoothed RGB8-to-YCbCr 4:2:0 encoding without source
ICC conversion retains full-resolution Y and compact chroma planes. Two rows of
full-resolution chroma scratch share the converted-sample budget. Source edges
are replicated before the ordered 2×2 averages. Other input/mode combinations
use full converted planes or streaming recomputation.

The byte-array overload also holds the encoded JPEG in memory; use the stream
overload for larger outputs. Opt-in parallel execution additionally keeps the
row of DCT coefficients and, when needed, source copy described above.
