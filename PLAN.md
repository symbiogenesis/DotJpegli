# Todo Plan

Completed optimizations, architectural notes, and benchmark results live in
[`docs/development.md`](docs/development.md),
[`docs/optimization-followup.md`](docs/optimization-followup.md), and
[`docs/performance-parity.md`](docs/performance-parity.md).

All changes must preserve the safe, managed-only, dependency-free runtime
contract. Default encoding changes must keep JPEG output deterministic;
output-changing compression ideas must stay opt-in until validated against
quality floors.

## 1. Control-case encoding speed (`> 1.10x` native limit)

In the latest full 256-case native comparison (`be14f89`), all four Kodak
photograph groups pass both the `1.05` group-mean limit and the `1.10`
per-case limit. All four deterministic control groups pass the `1.05` group
limit (`0.773–0.932`), but 15 individual control cases still exceed `1.10`:

- [ ] **Speed up sparse and smooth sequential (`p0`) controls:**
  Reduce per-block overhead on `smooth-gradient` (`1.177–1.294`),
  `low-contrast` (`1.137–1.272`), `low-noise` (`1.203–1.248`), `line-art`
  (`1.155–1.162`), and `color-edges` (`1.101–1.118`), where most AC
  coefficients quantize to zero and fixed AQ, DCT, and token-emission costs
  dominate.
- [ ] **Reduce fixed per-encode progressive setup on tiny images:**
  `tiny-odd-edges` (`17×19`) now passes sequential bounds (`0.816–1.090`) but
  exceeds `1.10` on progressive `p2/d1` (`1.146` for 4:4:4, `1.168` for 4:2:0).
  Pool or `stackalloc` small-image `AdaptiveQuantizationWorkspace` and
  `HuffmanTable.BuildOptimal` scratch arrays, and fast-path single-MCU-row
  conversion without intermediate plane rental.

## 2. CI validation suites

- [ ] **Run `[performance-workloads]` on CI:**
  Refresh the 26 x64 and ARM64 large-image, cache-boundary, memory, and
  parallel/concurrent workload diagnostics (last captured at `0104778`).
- [ ] **Run `[compare-release]` on CI and fix remaining 1.0.0 regressions:**
  Re-test all six lanes against the 1.0.0 baseline (last captured at `0104778`,
  where x64 default lanes on .NET 10 and .NET 11 had regressions) after the
  recent rolling-plane, contiguous hot-path, and entropy-coding improvements.

## 3. Rolling converted-plane extensions

- [ ] **Support `CacheDiagnostics` with rolling planes:**
  `CanUseRollingConvertedPlanes` currently disables the 32-row rolling window
  when `options.CacheDiagnostics` is set so existing full-plane diagnostic
  assertions keep their row counts. Add explicit rolling-window diagnostic
  fields so callers can inspect rolling cache usage without forcing full-image
  plane allocation.
- [ ] **Support 4:2:2 and custom vertical sampling factors:**
  Extend `ConvertedPlanes.TryCreateRolling` and `AdvanceConvertedWindow` beyond
  4:4:4, 4:2:0, and grayscale to support `ChromaSubsampling.HalfHorizontal`
  (`2×1`) and arbitrary vertical sampling factors (`mcuHeight + 6` halo rounded
  up to a power-of-two ring height).
- [ ] **Enable rolling planes for streaming grayscale sequential encodes:**
  Extend the streaming sequential token-budget check in
  `CanUseRollingConvertedPlanes` from 3-component interleaved scans to
  1-component grayscale sequential scans when coefficients are not cached.

## 4. Large-image block masks and progressive caches

- [ ] **Segment `QuantizedImage.blockMasks` for high-megapixel images:**
  `QuantizedImage.blockMasks` currently rents one `ulong[blockCount]` array per
  component from `ArrayPool<ulong>.Shared` (~1.5 MB per component on a 12 MP
  4:4:4 image). Segment `blockMasks` alongside `QuantizedImage.segments` when
  segmented storage is used, and charge large mask rentals to
  `EncoderCacheBudget`.
- [ ] **Evaluate single-pass progressive AC collection above L3 cache size:**
  When `QuantizedImage.IsComplete` is true on very large progressive images
  whose coefficient cache exceeds L3 (`> 16–32 MiB`), scan-major traversal
  reloads non-empty `short[64]` blocks from main memory across multiple AC
  scans. Test a block-mask-assisted single-pass collector for caches that
  exceed L3 while keeping small and medium images on scan-major traversal.

## 5. Parallel progressive and grayscale row pipeline

- [ ] **Produce quantized blocks and AC masks directly in parallel workers:**
  `ParallelDctRows` currently outputs quantized `short` rows only for
  3-component baseline 4:4:4 and 4:2:0 encodes; parallel progressive and
  grayscale encodes still use the legacy float row buffer (`1.012–1.025x`
  slower on ARM64 in run `41772797925`). Extend
  `ParallelDctRows.CanPrepareQuantizedRows` to progressive and grayscale modes
  so worker threads compute both quantized `short` blocks and `ulong` nonzero
  AC masks directly.

## 6. Opt-in compression research

- [ ] **Find recommended presets for `TailZeroRunBias` and `ChromaDownsamplingSharpness`:**
  Measure rate-distortion curves (Butteraugli, SSIMULACRA2, MS-SSIM, RGB PSNR)
  and encoding latency across all 24 Kodak photographs and 8 controls to
  identify default or preset recommendations (such as `TailZeroRunBias = 0.12f`
  and `ChromaDownsamplingSharpness = 0.15f`).
- [ ] **Prototype adaptive progressive scan-script pruning (opt-in):**
  Inspect precomputed `blockMasks` before entropy coding to merge AC bands
  (`1..2` + `3..63`) or reduce successive-approximation depth (`al = 2 -> 1`)
  when high-frequency bands are sparse (e.g., `Distance >= 2.5` or smooth
  regions), cutting scan-header, EOB, and entropy-pass overhead.
- [ ] **Validate offline compression searches on a broader corpus:**
  Test equal-length Huffman symbol ordering, saturated-edge color overshoot,
  image-specific RDOQ feedback, and cross-block DC trellis across a larger
  photographic, line-art, and chroma-edge corpus before exposing any as public
  presets.
