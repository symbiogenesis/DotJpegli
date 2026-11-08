# Optimization follow-up batches

[Documentation](README.md)

This document records the completed speed, allocation, memory, and compression
optimization batches and their CI validation results. Remaining work items are
tracked in [`PLAN.md`](../PLAN.md).

## Batch 2: Rolling planes, cached presets, block masks, and entropy tuning (`f3ffe8f`–`be14f89`)

| Area | Item | Implementation summary | Status |
| --- | --- | --- | --- |
| Speed & Memory | 32-row rolling MCU-row converted planes | `PixelSource.WithRollingConvertedPlanes` and `AdvanceConvertedWindow` use a 32-row luma (`row & 31`) and 16-row 4:2:0 chroma (`row & 15`) circular window on eligible single-worker, unsmoothed 4:4:4, 4:2:0, and grayscale encodes (`height > 32`) whenever full planes exceed the contiguous `8 MiB` pool limit or `MaxCacheBytes` budget (`PixelSource.PlanFullConvertedBytes` / `PlanRollingConvertedBytes`). Small/medium images that fit in a single contiguous `<= 8 MiB` rental keep direct contiguous row indexing. | Implemented (`f3ffe8f`, `a4fb50e`, `be14f89`) |
| Speed & Allocations | Dimension-aware `PooledOutputStream` capacity | `PooledOutputStream(int initialCapacity)` pre-sizes the first `ArrayPool<byte>` rental via `JpegliEncoder.EstimateInitialOutputCapacity(width, height)` (`clamp(width * height / 2, 4096, 1048576)`), eliminating repeated `4 KiB -> 256 KiB` buffer growth copies. | Implemented (`f3ffe8f`) |
| Speed & Allocations | Cached quantization configs, scan presets, and pooled histograms | `Quantization.GetOrCreateConfiguration` caches built-in `ushort[][]` tables, selectors, adaptive multipliers, and pre-transposed `PreparedTable[]` (up to 256 entries); `JpegScanConfiguration` caches static presets and level-2 scripts; `JpegliEncoder` pools `long[256]` histograms in `[ThreadStatic]` storage; `HuffmanCodingPlan` uses `stackalloc` for `acContextIndexes` and fast-paths single-active-histogram clustering. | Implemented (`f3ffe8f`, `b27a6ae`) |
| Speed | Sequential row hoisting and local-variable register allocation | `JpegliEncoder.PrepareSequentialChunk` hoists `GetConvertedPlaneSegment` from the inner `blockX` loop to the `blockY` loop, and hot DCT/parallel loops avoid passing loop-carried plane dimensions as `out` arguments so the JIT keeps them in registers. | Implemented (`f3ffe8f`, `a4fb50e`) |
| Progressive Speed | Per-block nonzero AC bitmasks (`ulong[][]`) and half-block SIMD | `QuantizedImage` stores the 64-bit nonzero AC mask computed in registers during `ForwardDct.TransformQuantize*WithMask`. `ProgressiveEntropyEncoder` skips 128-byte coefficient block loads when `bandNonzero == 0` and uses half-block SIMD classification (`OrderedAcMask`) for shifted point-transform bands. | Implemented (`f3ffe8f`, `a4fb50e`) |
| Compression & Speed | In-register tail-zero-run pruning | `ForwardDct.TransformQuantizeBiasedWithMask` and `Quantization.PruneTailZeroRunTransposed` prune borderline trailing `±1` coefficients directly from in-register transposed float DCT vectors `r0..r7` when `TailZeroRunBias > 0`, eliminating the second 2D DCT pass without adding register pressure to the default `TailZeroRunBias == 0` path. | Implemented (`f3ffe8f`) |

### CI validation at `be14f89`

Full 256-case native parity, 1,280-pair quality/compression parity, and 192-case
allocation measurements ran in
[CI run 42376806989](https://github.com/symbiogenesis/DotJpegli/actions/runs/42376806989)
([retained evidence](performance-evidence/2026-11-07-be14f89/README.md)):

- **Kodak photographs (all 4 groups pass `<= 1.05` group and `<= 1.10` individual bounds):**
  Sequential 4:4:4 and 4:2:0 average `0.858x` and `0.847x` native latency
  (14.2–15.3% less time); progressive level 2 averages `0.584x` and `0.629x`
  native latency (37.1–41.6% less time).
- **Deterministic controls:** All 4 control groups pass the `1.05` group-mean
  bound (`0.773–0.932x`), and `tiny-odd-edges` sequential (`p0`) now passes the
  `1.10` per-case bound (`0.816–1.090x`). Fifteen control cases still exceed
  `1.10` (`smooth-gradient`, `low-contrast`, `low-noise`, `line-art`, and
  `color-edges` in sequential `p0`, plus `tiny-odd-edges` in progressive
  `p2/d1`).
- **Allocations per encode:** DotJpegli averages `79.2–109.1 KiB` of managed GC
  allocations per Kodak encode vs. `1,924.8–4,288.3 KiB` of native heap requests
  for Google Jpegli.

---

## Batch 1: Cache planning, ICC/XYB tables, smoothing halo, and parallel baseline (`51126a7`–`1898828`)

| Area | Integration summary | Status |
| --- | --- | --- |
| Progressive AC collection across scans | Single block traversal per component with independent scan states when coefficients are uncached or partially cached; complete caches keep scan-major traversal. | Implemented |
| Joint sample/coefficient cache planning | Reserve complete coefficient capacity (`<= 64 MiB`) before admitting converted samples when available `MaxCacheBytes` is below `128 MiB`. | Implemented |
| Parallel baseline histogram/token collection | Complete-cache baseline scans collect per-worker histograms and tokens in parallel (up to 32 slots and 4 MiB staging) with exact predecessor DC lookup before ordered merge. | Implemented |
| Sampled ICC inverse index | Single-level 1,028-byte value grid index for destination sampled curves with 512–8,192 knots (`<= 32` knots per bucket), isolated in a non-inlined helper. | Implemented |
| Smoothing row reuse | Fixed 3-row halo and tile (`4,504` bytes of stack scratch) preserving the 8-neighbor addition order and cutting raw sample reads from 576 to 100 (`1×1`) or 9,216 to 1,156 (`4×4`). | Implemented |
| Byte-input XYB transfer lookup | Lazy 256-entry (`1 KiB`) float table for untransformed 8-bit RGB/grayscale XYB inputs. | Implemented |
| Quantized parallel row pipeline | Worker threads produce quantized `short` rows directly for 3-component baseline 4:4:4 and 4:2:0 encodes without a whole-image coefficient cache. | Implemented |
| Equal-length Huffman symbol ordering | Order equal-length symbols by descending frequency in a separate non-inlined helper when needed; offline search tool compares orderings. | Implemented |
| Image-specific RDOQ feedback, DC trellis, and color preprocessing | Opt-in offline search commands (`--search-huffman-feedback`, `--search-dc-trellis`, `--search-color-preprocessing`) in [`DotJpegli.Parity`](../tools/DotJpegli.Parity/README.md#follow-up-compression-experiments). | Implemented offline |

### Hosted comparison and corrective validation

The initial paired CI run
([run 41769154898](https://github.com/symbiogenesis/DotJpegli/actions/runs/41769154898))
compared `4c2b2abe` against pre-batch `b2fa25da` using six balanced ABBA/BAAB
quartets per case (ratios `< 1.0` favor the new revision):

| Scenario | x64 .NET 10 | x64 .NET 11 | ARM64 .NET 10 | ARM64 .NET 11 |
| --- | ---: | ---: | ---: | ---: |
| Ordinary | 47/64; 0.9960 | 55/64; 0.9897 | 60/64; 1.0084 | 59/64; 1.0027 |
| Source ICC | 3/4; 0.9951 | 4/4; 0.9964 | 4/4; 1.0058 | 4/4; 1.0129 |
| Smoothing | 2/8; 0.1426 | 8/8; 0.1436 | 8/8; 0.2084 | 8/8; 0.2012 |
| Constrained cache | 6/8; 0.3913 | 7/8; 0.4519 | 8/8; 0.4799 | 8/8; 0.4827 |
| Sampled ICC | 7/8; 0.8285 | 8/8; 0.8709 | 8/8; 0.7541 | 8/8; 0.7208 |

Inspection of `4c2b2abe` showed that fusing AC scans when the full coefficient
cache was already in memory regressed ARM64 progressive latency, inlining the
sampled ICC index enlarged the parametric curve stack frame, and a captured
closure in default Huffman planning added per-encode allocations. Two harness
config bugs also skipped the XYB and two-worker scenarios.

### Corrective CI paired comparison

Revision `744f7be` restored scan-major traversal for complete coefficient
caches, isolated the sampled ICC index in a `NoInlining` helper, removed the
default Huffman planning closure, and fixed the XYB and two-worker harness
configurations (followed by `1898828`, which guarded single-worker encodes from
allocating the parallel histogram collector's cancellation state).

The corrective paired run
([run 41772797925](https://github.com/symbiogenesis/DotJpegli/actions/runs/41772797925))
compared `744f7be` against `b2fa25da` across all 432 requested rows (385
timing-valid, 47 timing-invalid, 100% matching JPEG hashes):

| Scenario | x64 .NET 10 | x64 .NET 11 | ARM64 .NET 10 | ARM64 .NET 11 |
| --- | ---: | ---: | ---: | ---: |
| Ordinary | 61/64; 0.9985 | 54/64; 1.0060 | 61/64; 0.9981 | 57/64; 0.9998 |
| Source ICC | 3/4; 1.0009 | 4/4; 1.0079 | 4/4; 1.0012 | 4/4; 1.0002 |
| Smoothing | 8/8; 0.1495 | 4/8; 0.1483 | 8/8; 0.2168 | 8/8; 0.2006 |
| Byte XYB | 8/8; 0.6386 | 6/8; 0.6162 | 8/8; 0.6769 | 8/8; 0.6726 |
| Constrained cache | 8/8; 0.5024 | 6/8; 0.6176 | 8/8; 0.4808 | 8/8; 0.4828 |
| Sampled ICC | 3/8; 0.8785 | 7/8; 0.8646 | 8/8; 0.7686 | 8/8; 0.7309 |
| Two workers | 1/8; 0.9978 | 6/8; 0.9250 | 8/8; 0.9196 | 8/8; 0.9436 |

- All valid smoothing (`0.148–0.217x`), byte-XYB (`0.616–0.677x`), sampled-ICC
  (`0.731–0.879x`), constrained-cache progressive (`0.171–0.376x`, saving
  17–18 KiB/encode), and parallel sequential (`0.746–0.923x`, saving
  32–35 KiB/encode) cases favored the batch.
- Parallel progressive encodes on ARM64 still used the legacy float row buffer
  and favored the baseline (`1.012–1.025x`); extending quantized parallel rows
  to progressive and grayscale encodes is tracked in [`PLAN.md`](../PLAN.md).

### Offline compression search results (`kodim01`)

Run `41772797925` also evaluated the four bounded offline compression searches on
`kodim01` (`768×512`, 4:2:0, distance 1) against the pinned native decoder,
requiring winners to meet all three control quality floors (RGB PSNR, local luma
SSIM, and `dotjpegli-ms-ssim-v1`):

| Offline search | Candidates | Control / winner bytes | Result on `kodim01` |
| --- | ---: | ---: | --- |
| Equal-length Huffman ordering (`p2`) | 4 | 126,953 / 126,948 | Frequency-descending order saved 5 bytes (0.004%) with identical decoded pixels. |
| Cross-block DC trellis (`p0`) | 5 | 130,250 / 130,250 | Control retained; all 4 modified candidates reduced PSNR/SSIM below the control floor. |
| Image-specific AC Huffman feedback (`p0`) | 7 | 130,250 / 130,250 | Control retained; all 6 RDOQ/feedback candidates reduced quality below the control floor. |
| Color preprocessing (`p0`) | 15 | 130,524 / 130,500 | `overshoot-d1-w1` saved 24 bytes (0.018%) and improved all three quality scores slightly. |
