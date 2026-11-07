# Algorithm, speed, memory, and compression plan

## Scope and constraints

Improve encoding speed, allocations, peak working memory, and compression
efficiency while preserving the safe, managed-only, portable, dependency-free
runtime contract. Default encoding changes must preserve deterministic JPEG
bytes, scalar/vector evaluation order, threshold comparisons, rounding, error
handling, cancellation, and destination-output prefixes. Output-changing
compression improvements must remain explicit opt-ins (`JpegliEncoderOptions`
or offline search tools) until validated against quality floors.

This document tracks the current implementation state after `f3ffe8f` and all
remaining validation, architectural, and compression work. Detailed
implementation notes live in [development documentation](docs/development.md),
follow-up batch measurements live in
[optimization follow-up](docs/optimization-followup.md), and native parity
protocols live in [performance parity](docs/performance-parity.md).

## Completed in `f3ffe8f` (Priority 1–3 batch)

| Priority | Item | Implementation summary | Status |
| --- | --- | --- | --- |
| 1 (Speed & Memory) | 32-row rolling MCU-row converted planes | `PixelSource.WithRollingConvertedPlanes` and `AdvanceConvertedWindow` replace upfront full-image float plane allocation with a 32-row luma (`row & 31`) and 16-row 4:2:0 chroma (`row & 15`) circular window on eligible single-worker, single-pass 4:4:4, 4:2:0, and grayscale encodes (`height > 32`) | Implemented (`f3ffe8f`) |
| 1 (Speed & Allocations) | Dimension-aware `PooledOutputStream` initial capacity | `PooledOutputStream(int initialCapacity)` pre-sizes the first `ArrayPool<byte>` rental via `JpegliEncoder.EstimateInitialOutputCapacity(width, height)` (`clamp(width * height / 2, 4096, 1048576)`), eliminating repeated `4K -> 256K` growth copies | Implemented (`f3ffe8f`) |
| 1 (Speed & Allocations) | Cached quantization configuration & scan presets | `Quantization.GetOrCreateConfiguration` caches built-in `ushort[][]` tables, selectors, adaptive multipliers, and pre-transposed `PreparedTable[]` (up to 256 entries); `JpegScanConfiguration` caches static presets and level-2 scripts; `JpegliEncoder` pools `long[256]` histograms in `[ThreadStatic]` storage; `HuffmanCodingPlan` uses `stackalloc` for `acContextIndexes` and fast-paths single-active-histogram clustering | Implemented (`f3ffe8f`) |
| 1 (Speed) | Sequential converted-segment row hoisting | `JpegliEncoder.PrepareSequentialChunk` hoists `GetConvertedPlaneSegment` from the inner `blockX` loop to the `blockY` loop | Implemented (`f3ffe8f`) |
| 2 (Progressive Speed) | Per-block nonzero AC bitmasks (`ulong[][]`) | `QuantizedImage` stores the 64-bit nonzero AC mask already computed in registers during `ForwardDct.TransformQuantize*WithMask`; `ProgressiveEntropyEncoder` (`AccumulateEmptyAcBlock`, `AccumulateOrderedAcFrequenciesWithMask`, `WriteOrderedAcWithMask`, `AccumulateEmptyAcRefinementBlock`, `ProcessOrderedAcRefinementWithMask`) skips 128-byte coefficient block loads when `bandNonzero == 0` and fast-paths sparse blocks (`PopCount <= 8`) | Implemented (`f3ffe8f`) |
| 3 (Compression + Speed) | In-register tail-zero-run pruning | `ForwardDct.TransformQuantizeBiasedWithMask` (`NoInlining`) and `Quantization.PruneTailZeroRunTransposed` prune borderline trailing unit coefficients directly from in-register transposed float DCT vectors `r0..r7`, eliminating the second 2D DCT pass when `TailZeroRunBias > 0` without increasing register pressure on the default `TailZeroRunBias == 0` path | Implemented (`f3ffe8f`) |

Targeted CI validation on `f3ffe8f` ([run 42360668980](https://github.com/symbiogenesis/DotJpegli/actions/runs/42360668980))
passed all build, test, formatting, NativeAOT/trimming, allocation, and package
gates, with `kodim01` at distance 1 reaching `managed/native` ratios of
`0.703` (`444/p0`), `0.600` (`444/p2`), `0.654` (`420/p0`), and `0.645` (`420/p2`).

## Remaining work

### 1. Immediate CI validation and evidence refresh

- **Full 256-case native parity, workload, and release-regression run:**
  Run the complete 256-case quality/compression and latency comparison
  (`[full-parity]`), bounded large-image/memory/parallel workloads
  (`[performance-workloads]`), and 1.0.0 release regression comparison
  (`[compare-release]`) on CI.
- **Assess deterministic control bounds:**
  In the previous full reference (`615b373`, run `42196948996`), all four Kodak
  photograph groups passed both group (`<= 1.05`) and individual (`<= 1.10`)
  bounds, while deterministic controls failed individual bounds on small inputs
  (worst case `tiny-odd-edges` 4:2:0 sequential at `1.367`). Verify how much of
  the control gap is closed by cached quantization configurations, cached scan
  presets, pooled histograms, and pre-sized output buffers, and identify any
  remaining cases above `1.10`.
- **Refresh retained evidence and charts:**
  Once a complete valid `[full-parity]` and allocation run is collected, update
  `docs/performance-evidence/`, `docs/performance-parity.md`, and the README SVG
  charts via `./tools/update-performance-charts.ps1`.

### 2. Tiny-image and fixed per-encode overhead (if control cases still exceed `1.10`)

- **Tiny-image setup audit (`<= 64x64`, `tiny-odd-edges`):**
  `CanUseRollingConvertedPlanes` currently requires `height > 32`, which is
  appropriate because tiny images already fit in L1 cache. However, on tiny
  controls (such as `17x19`), fixed per-encode setup dominates wall time:
  - `AdaptiveQuantizationWorkspace` array rentals and row clearing for small
    padded dimensions.
  - `BitWriter` 4 KiB buffer rental/return and marker writer allocations.
  - `HuffmanTable.BuildOptimal` workspace arrays when building 4–10 tables for
    a handful of blocks.
- **Action:** If any tiny control case remains above the `1.10` individual bound
  in `[full-parity]` profiles, pool or `stackalloc` small-image AQ/Huffman
  scratch buffers and fast-path single-MCU-row conversion without intermediate
  plane rental.

### 3. Rolling converted planes extensions

- **`CacheDiagnostics` compatibility:**
  `CanUseRollingConvertedPlanes` currently disables rolling planes when
  `options.CacheDiagnostics is not null` so existing tests asserting full-image
  `ConvertedSampleCachedRows == height` keep their exact numbers. Add explicit
  rolling-window diagnostic fields (or distinguish rolling vs. prefix-cached
  plane modes) so diagnostic callers can observe rolling cache behavior without
  forcing full-image plane allocation.
- **4:2:2 (`ChromaSubsampling.HalfHorizontal`) and arbitrary sampling factors:**
  Extend `ConvertedPlanes.TryCreateRolling` and `AdvanceConvertedWindow` beyond
  4:4:4, 4:2:0, and grayscale to support 4:2:2 (`2x1`) and custom vertical
  sampling factors (`mcuHeight + 6` halo rounded up to a power-of-two ring
  height).
- **Streaming sequential single-component and custom-scan eligibility:**
  Currently `CanUseRollingConvertedPlanes` admits streaming sequential encodes
  for 3-component interleaved scans (`componentCount == 3`) and full-coefficient-cache
  encodes for grayscale/progressive. Extend the streaming sequential token-budget
  check to single-component grayscale sequential encodes (`componentCount == 1`)
  when coefficients are not cached.

### 4. Block-mask lifecycle and large-image segmentation

- **Segmented `blockMasks` for high-megapixel images:**
  `QuantizedImage.blockMasks` currently rents one `ulong[blockCount]` array per
  component from `ArrayPool<ulong>.Shared`. For a 12 MP 4:4:4 image
  (`187,500` blocks per component), each `ulong[]` is `1.5 MB` (allocated on the
  Large Object Heap if it exceeds `ArrayPool<ulong>.Shared` bucket limits).
  Store `blockMasks` in segments matching `QuantizedImage.segments` (or bounded
  power-of-two chunks) when `segmented` is true, and account large mask rentals
  under `EncoderCacheBudget`.
- **Single-pass multi-scan progressive AC collection on L3-exceeding caches:**
  When `QuantizedImage.IsComplete` is true on very large progressive images
  whose `short` coefficient cache exceeds L3 cache (`> 16–32 MiB`), scan-major
  traversal still loads non-empty `short[64]` blocks from DRAM across multiple
  AC scans (`1..2`, `3..63 al=2`, `3..63 ah=2..1`, `3..63 ah=1..0`). Evaluate a
  block-mask-assisted single-pass collector for large complete caches that
  exceed L3 size while keeping small/medium images on scan-major traversal.

### 5. Parallel progressive row pipeline (`docs/optimization-followup.md` carryover)

- **Quantized + block-mask parallel rows for progressive and grayscale:**
  `ParallelDctRows` currently produces quantized `short` rows directly only for
  3-component baseline 4:4:4 and 4:2:0 encodes; parallel progressive and
  grayscale encodes still use the legacy floating-point parallel row workspace,
  which showed `1.012–1.025x` ARM64 regressions in paired run `41772797925`.
- **Action:** Extend `ParallelDctRows.CanPrepareQuantizedRows` to progressive
  and grayscale configurations, computing both quantized `short` blocks and
  their `ulong` nonzero AC masks in worker threads so `QuantizedImage.Create`
  copies pre-quantized rows and masks without allocating float row buffers or
  recomputing `EntropyEncoder.NonzeroAcMask` on the coordinator thread.

### 6. Win-win compression + speed research (opt-in and offline)

- **Corpus-wide Pareto curve for `TailZeroRunBias` and `ChromaDownsamplingSharpness`:**
  With `PruneTailZeroRunTransposed` now fused in-register at zero extra DCT pass
  cost, measure BD-rate (bits per pixel vs. Butteraugli, SSIMULACRA2, MS-SSIM,
  and RGB PSNR) and encoding latency across all 24 Kodak photographs and 8
  controls to identify optimal recommended presets (e.g., `TailZeroRunBias = 0.12f`,
  `ChromaDownsamplingSharpness = 0.15f`).
- **Adaptive progressive scan-script pruning (opt-in):**
  Investigate an opt-in progressive optimization that inspects the precomputed
  `blockMasks` / coefficient magnitude distribution before entropy coding to
  coalesce `1..2` + `3..63` or reduce successive-approximation depth (`al = 2 -> 1`)
  when high-frequency bands are sparse (such as `Distance >= 2.5` or smooth
  regions), saving both scan-header/EOB overhead and entropy pass time.
- **Broader corpus validation for offline searches:**
  Evaluate equal-length Huffman symbol ordering, saturated-edge color overshoot,
  image-specific RDOQ feedback, and cross-block DC trellis across a broader
  photographic, line-art, and chroma-edge corpus before promoting any
  output-changing search to a public preset.
