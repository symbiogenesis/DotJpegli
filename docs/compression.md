# Compression and scan options

[Documentation](README.md)

## Quality and distance

- **`Quality`** accepts finite values from `0` to `100` (including fractional
  values) and defaults to `85`.
- **`Distance`** accepts any finite value greater than `0` to set Jpegli's
  perceptual quantization distance directly (matching upstream
  [`jpegli_set_distance`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/encode.cc)).
  Lower distances use finer quantization.

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Distance = 1.0f,
    Subsampling = ChromaSubsampling.Quarter
});

float distance = JpegliEncoder.QualityToDistance(85); // 1.45f
```

When `Distance` is `null` (the default), the encoder maps `Quality` using
`JpegliEncoder.QualityToDistance(quality)`: quality `0` maps to `25.0`, `30` to
`6.4`, and `100` to `0.01` (a special step down from the `~0.1` approached just
below 100). When `Distance` is set, it overrides `Quality` (`Quality` must still
be in `0..100`).

Direct `Distance` also accepts values outside the `0..100` quality range (such
as `0.001` or `30`). Quantization table entries are clamped to `1..255` by
default, or `1..32767` when `ForceBaseline = false`. Even very small distances
and quality 100 remain lossy 8-bit JPEG.

## Restart markers

- **`RestartInterval`** (`0..65535`, default `0`): Inserts a restart marker
  (`RST0`..`RST7`) after every `RestartInterval` minimum coded units (MCUs).
  Zero disables restart markers.
- **`RestartRows`** (`>= 0`, default `0`): When positive, overrides
  `RestartInterval` (`RestartInterval` must still be valid) and inserts a
  restart marker after that many MCU rows in the current scan, capped at
  `65,535` MCUs (`min(RestartRows * mcuColumnsInScan, 65535)`).

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ProgressiveLevel = 2,
    RestartRows = 1
});
```

Restart markers reset DC prediction, terminate progressive EOB runs, pad the
bitstream to the next byte, and cycle `RST0`..`RST7` from `RST0` at the start of
each scan, without changing quantized coefficients or decoded pixels.

In **interleaved** scans, an MCU spans `8 * maxHorizontal` by `8 * maxVertical`
pixels (8×8 for grayscale and 4:4:4; 16×16 for 4:2:0; see the
[sampling table](encoding.md#pixel-layouts-and-sampling)). In
**single-component** scans (non-interleaved sequential scans, separate DC scans,
and progressive AC scans), each real 8×8 component block counts as one MCU, and
`RestartRows` uses that component's real block columns. DRI markers are emitted
before a scan whenever the effective restart interval changes.

## Scan modes and scripts

All scan configurations preserve the same quantized coefficients and final
decoded pixels. Option lists (`ScanScript`, `ProgressiveAcScanEnds`,
`ProgressiveAcRefinementBits`) are copied and validated before output is
written.

### Sequential scans

By default (`Progressive = false`, `SequentialInterleaveComponents = true`),
the encoder writes a single interleaved sequential scan containing all
components.

Set `SequentialInterleaveComponents = false` to write one sequential scan per
component in frame order (`Y`, `Cb`, `Cr`), visiting only real component blocks
in raster order and omitting padded edge MCU blocks:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    SequentialInterleaveComponents = false,
    RestartRows = 1
});
```

`SequentialInterleaveComponents = false` requires sequential output (`Progressive`
must be `false` and `ProgressiveLevel` must be `null` or `0`). Grayscale output
has one component and is unchanged.

### Progressive scans and presets

Set `Progressive = true` to write a progressive JPEG (`SOF2`), or set
`ProgressiveLevel` (`>= 0`) to select one of Jpegli's built-in scan presets
directly (`ProgressiveLevel` overrides `Progressive`):

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    ProgressiveLevel = 2,
    RestartInterval = 64
});
```

| `ProgressiveLevel` | DC scan | AC scans (JPEG zigzag positions `1..63`) |
| --- | --- | --- |
| `0` | Interleaved with AC (sequential `SOF0`/`SOF1`) | All coefficients (`1..63`) at full precision |
| `1` | Full precision | `1..63` with 1 bit deferred, then 1 refinement scan |
| `2` | Full precision | `1..2` at full precision; `3..63` with 2 bits deferred, then 2 refinement scans |
| `>= 3` | Full precision | Luma: `1..2` full, `3..63` with 2 refinement bits; Chroma: `1..5` full, `6..63` with 1 refinement bit (falls back to level 2 for grayscale) |

When `ProgressiveLevel` is set, DC is interleaved only when both maximum output
sampling factors are `1` (such as 4:4:4, grayscale, or default RGB); subsampled
modes (such as 4:2:0 or default XYB) emit separate per-component DC scans.
`ProgressiveLevel` cannot be combined with `ScanScript`,
`ProgressiveAcScanEnds`, `ProgressiveAcRefinementBits`, non-zero
`ProgressiveRefinementBits`, or `ProgressiveInterleaveDc = false`.

### Customizing progressive bands, refinement, and DC ordering

When `Progressive = true` and `ProgressiveLevel` is `null`, you can configure
the progressive schedule with four options:

- **`ProgressiveRefinementBits`** (`0..10`, default `0`): Number of low-order
  bits deferred from the initial DC and AC scans and delivered one bit plane at
  a time in subsequent refinement scans.
- **`ProgressiveAcScanEnds`** (strictly increasing ends in `1..63` ending at
  `63`, default `null` meaning `[63]`): Splits AC coefficients into multiple
  spectral bands (for example, `[2, 63]` sends `1..2` then `3..63`).
- **`ProgressiveAcRefinementBits`** (one value in `0..10` per AC band, default
  `null`): Overrides `ProgressiveRefinementBits` for each AC band independently
  while `ProgressiveRefinementBits` continues to control DC refinement depth.
- **`ProgressiveInterleaveDc`** (default `true`): Set to `false` to emit
  separate per-component DC scans in raster order instead of an interleaved DC
  scan.

For example, this matches the `ProgressiveLevel = 2` schedule on subsampled
color output using explicit progressive options:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Progressive = true,
    ProgressiveInterleaveDc = false,
    ProgressiveAcScanEnds = [2, 63],
    ProgressiveAcRefinementBits = [0, 2]
});
```

### Custom scan scripts

Set `ScanScript` (`IReadOnlyList<JpegScan>`) for full manual control over
sequential or progressive scans (`ScanScript` overrides `Progressive` and cannot
be combined with other scan-schedule options):

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

Each `JpegScan(Components, SpectralStart = 0, SpectralEnd = 63, SuccessiveHigh = 0, SuccessiveLow = 0)`
follows upstream's `ValidateScanScript` rules:

- Component indices (`0..2`, or `0` for grayscale) must be strictly increasing
  within each scan.
- The first scan's spectral range selects sequential (`0..63`) or progressive
  output; sequential and progressive scans cannot be mixed.
- In a sequential script, every component must appear in exactly one scan with
  `SpectralStart = 0, SpectralEnd = 63, SuccessiveHigh = 0, SuccessiveLow = 0`.
- In a progressive script, DC scans use `0..0` (1 to 3 components) and AC scans
  use a range within `1..63` for a single component after its initial DC scan.
  Each coefficient must start with `SuccessiveHigh = 0` and `SuccessiveLow` in
  `0..10`, step down one bit at a time (`SuccessiveLow = SuccessiveHigh - 1`),
  and finish at `SuccessiveLow = 0`. At most 252 AC component scans are allowed.

## Huffman optimization

With `OptimizeHuffmanTables = true` (the default):

- **Histogram clustering:** Grayscale and YCbCr encodes collect symbol
  frequencies across scans and greedily merge DC and AC histograms when the
  combined bit cost (including `DHT` marker overhead) is lower, followed by
  slot eviction and post-pass consolidation. Baseline output uses at most 2
  tables per class (`0..1`); extended sequential (`SOF1`) and progressive
  (`SOF2`) output can use up to 4 slots (`0..3`) and redefine AC slots between
  scans.
- **Exact 16-bit code lengths and symbol ordering:** When a Huffman tree exceeds
  JPEG's 16-bit codeword limit, the Larmore-Hirschberg Package-Merge algorithm
  computes optimal length-limited code lengths. Within equal-length code groups,
  symbols are ordered by descending frequency so more frequent symbols receive
  lower canonical codewords (with more zero bits), reducing `0xFF00` byte
  stuffing.
- **Progressive EOB runs:** Progressive initial and refinement AC scans combine
  consecutive block endings into end-of-block (EOB) runs up to `32,767` blocks
  (flushing at restart markers, scan ends, or when a refinement scan buffers
  `2,047` correction bits).

Setting `OptimizeHuffmanTables = false` uses standard fixed Huffman tables and
single-block EOB symbols. Explicit RGB and XYB output spaces use shared Huffman
tables.

## Adaptive quantization and smoothing

- **`AdaptiveQuantization`** (default `true`): Analyzes local spatial texture
  and brightness to modulate per-block quantization zeroing thresholds (on `Y`
  for YCbCr/grayscale/XYB and `G` for RGB). Set to `false` for uniform
  quantization. Automatically disabled if custom sampling subsamples the
  analyzed component.
- **`SmoothingFactor`** (`0..100`, default `0`): Applies upstream's optional 3×3
  box-weighted smoothing filter to converted samples before chroma subsampling
  (each of the 8 neighbors gets weight `SmoothingFactor / 1024`). Adaptive
  quantization still analyzes the unsmoothed luma.
- **`TailZeroRunBias`** (`0..0.5`, default `0`): Raises the dead-zone threshold
  for trailing `±1` AC coefficients in zigzag order when preceded by a run of at
  least 16 zeros (`ZRL`), pruning borderline high-frequency isolated
  coefficients directly in registers during the forward DCT without a second DCT
  pass.
- **`ChromaDownsamplingSharpness`** (`0..0.5`, default `0`): Blends the 2×2
  chroma box average toward the quad's local midpoint
  `0.5 * (min + max)` during 4:2:0 (`Quarter`) downsampling to preserve
  saturated color edges.

## Parallel execution

Set `MaxDegreeOfParallelism` to a positive worker count, or `-1` to use
`Environment.ProcessorCount` (default `1` for serial execution; `0` and `< -1`
are invalid):

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    MaxDegreeOfParallelism = 4
});
```

Parallel execution produces **exact byte-for-byte identical JPEGs** to serial
execution:

- **Parallel DCT and row conversion:** For images with at least `16,384` pixels
  and `64` blocks per MCU row (at least `32` blocks per worker), worker threads
  convert pixels, smooth/downsample components, and compute DCT blocks across
  each MCU row. Eligible 3-component baseline 4:4:4 and 4:2:0 encodes produce
  quantized `short` rows directly in workers.
- **Parallel baseline histogram/token collection:** Complete-cache baseline
  scans can collect per-worker histograms and tokens in parallel (up to 32 slots
  and 4 MiB of pooled staging) before deterministic ordered merge.
- **Parallel entropy replay:** Complete cached-token scans can replay entropy
  in parallel across restart intervals, or across 8,192-token bit chunks (when a
  scan without restarts has at least `16,384` tokens), merging unpadded bit
  streams on the calling thread within a 4 MiB pooled staging cap.
- **Thread safety and streams:** All workers complete before `Encode` returns,
  and only the calling thread writes to the destination `Stream`.

## Memory and source lifetime

Keep input pixel buffers and option memory unchanged until `Encode` returns.

`MaxCacheBytes` (default `192 MiB`; `0` disables image caches) limits the
combined pooled array capacity owned by a single encode across three internal
caches, each of which also has its own `64 MiB` ceiling:

1. **Converted sample planes (`<= 64 MiB`):**
   - **Contiguous planes (`<= 8 MiB`):** Small and medium images rent a single
     contiguous `ArrayPool<float>` buffer (with half-resolution chroma planes
     for unsmoothed 4:2:0) for fast direct row indexing.
   - **32-row rolling window:** Single-worker, unsmoothed 4:4:4, 4:2:0, and
     grayscale encodes (`height > 32`) automatically use a 32-row luma / 16-row
     4:2:0 chroma circular buffer when full planes would exceed the `8 MiB`
     contiguous pool limit or when `MaxCacheBytes` is constrained, avoiding
     full-image float plane allocations.
   - **Segmented or prefix caches:** Other multi-pass or parallel encodes use
     bounded row segments up to `64 MiB`, or cache a 16-row-aligned prefix of
     the image and recompute remaining rows on demand.
2. **Quantized coefficients (`<= 64 MiB`):**
   - Single-worker interleaved baseline YCbCr encodes normally stream blocks
     directly into entropy tokens without allocating a full-image coefficient
     cache (except on 256-bit SIMD hosts without 512-bit SIMD when the budget
     admits separate component rows).
   - Multi-scan and progressive encodes cache `short[64]` quantized blocks in
     contiguous or segmented arrays (plus a `ulong` nonzero AC bitmask per block
     so progressive AC scans skip empty blocks without loading coefficient
     arrays). If an image exceeds the budget, the planner reserves what fits and
     recomputes uncached MCU rows on demand.
3. **Entropy tokens (`<= 64 MiB` shared across scans):**
   - Optimized Huffman encoding records compact 32-bit entropy tokens (packing
     up to 24 progressive refinement correction bits per token) so the emission
     pass can write the bitstream without re-running DCT or quantization. If a
     scan exhausts the token budget, that scan falls back to on-demand emission
     and refunds its token rentals for subsequent scans.

**Output buffering and per-encode pooling:**

- The `byte[]` `Encode` overload pre-sizes its initial `ArrayPool<byte>` buffer
  from the image dimensions (`clamp(width * height / 2, 4 KiB, 1 MiB)`) to avoid
  repeated growth copies, and chains 1 MiB segments if the JPEG exceeds 1 MiB.
  Use the `Stream` overload to write output directly without allocating a final
  returned `byte[]`.
- Built-in quantization table configurations (up to 256 entries), scan-script
  presets, and thread-local Huffman frequency histograms are cached and reused
  across encodes.
