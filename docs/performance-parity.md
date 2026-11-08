# Performance parity validation

[Documentation](README.md)

This guide documents the performance comparison between DotJpegli and unchanged
native Google Jpegli (`031a0077f5799a6041004267fc12b956c1f52a20`), the fixed
acceptance bounds, the measurement methodology, and the CI diagnostic modes.

## Current native comparison (`be14f89`)

The latest complete 256-case comparison ran on revision
`be14f89161d77736adc09a64536646e16d7bd1c4` in
[CI run 42376806989](https://github.com/symbiogenesis/DotJpegli/actions/runs/42376806989)
on November 7, 2026 ([retained evidence](performance-evidence/2026-11-07-be14f89/README.md)):

| Corpus | Sampling | Sequential (`p0`) mean / worst | Progressive (`p2`) mean / worst | Verdict |
| --- | --- | ---: | ---: | --- |
| Kodak photographs (24 images) | 4:4:4 | 0.858 / 1.042 | 0.584 / 0.641 | **PASS** |
| Kodak photographs (24 images) | 4:2:0 | 0.847 / 1.083 | 0.629 / 0.681 | **PASS** |
| Deterministic controls (8 scenes) | 4:4:4 | 0.932 / 1.294 | 0.773 / 1.146 | FAIL (per-case `> 1.10`) |
| Deterministic controls (8 scenes) | 4:2:0 | 0.863 / 1.209 | 0.814 / 1.168 | FAIL (per-case `> 1.10`) |

- **Ratios** are `managed median latency / native median latency` across
  distances `1` and `4` (lower is faster).
- **Kodak photographs:** All four photograph groups pass both the `<= 1.05`
  group-mean bound and the `<= 1.10` per-case bound, using **14.2–15.3% less
  time** than native for sequential encoding and **37.1–41.6% less time** for
  progressive encoding.
- **Deterministic controls:** All four control groups pass the `<= 1.05`
  group-mean bound (using 6.8–22.7% less time on average), but 15 individual
  control cases exceed the `1.10` per-case limit (worst case: `1.294` on
  sequential 4:4:4 `smooth-gradient` at distance 1). Because every individual
  case must be `<= 1.10`, the overall 256-case parity verdict remains **FAILED**.
- **Host environment:** AMD EPYC 9V45 x64, .NET `11.0.0-rc.3.26522.148`,
  8 float SIMD lanes (`Vector512` enabled), 1 worker pinned to 1 CPU, tiered
  compilation disabled.

### Allocations per encode (`be14f89`)

The same CI run measured untimed per-encode allocations across all 192 Kodak
photograph configurations (`24` images × distances `1, 4` × `4:4:4, 4:2:0` ×
levels `0, 2`), including the returned JPEG output buffer:

| Encoding mode | DotJpegli managed GC (`KiB/encode`) | Google C++ jpegli heap requests (`KiB/encode`) |
| --- | ---: | ---: |
| Sequential, 4:4:4 | 90.3 | 1,924.8 |
| Sequential, 4:2:0 | 79.2 | 2,190.1 |
| Progressive level 2, 4:4:4 | 109.1 | 4,288.3 |
| Progressive level 2, 4:2:0 | 97.6 | 3,209.3 |

---

## Acceptance scope and bounds

The full comparison covers **256 matched cases**:

- **Inputs:** 24 hash-pinned Kodak photographs plus 8 deterministic synthetic
  controls (`smooth-gradient`, `low-contrast`, `color-edges`, `line-art`,
  `structured-texture`, `low-noise`, `full-noise`, `tiny-odd-edges`).
- **Settings:** RGB8 input to YCbCr output, distances `1` and `4`, chroma
  sampling `4:4:4` and `4:2:0`, progression levels `0` (sequential) and `2`
  (progressive), adaptive quantization enabled, optimized Huffman tables,
  baseline quantizers (`1..255`), JFIF 1.01 (`1×1` aspect ratio), 1 worker.

A passing verdict requires all 256 cases to complete with valid timing samples,
matching JPEG structure, and both latency bounds satisfied:

| Bound | Limit |
| --- | ---: |
| Group geometric mean of `managed / native` median latency ratios (per corpus category, sampling, and progression) | `<= 1.05` |
| Every individual case `managed / native` median latency ratio | `<= 1.10` |
| Minimum total timed duration per block | `>= 1 ms` |
| Maximum relative median absolute deviation (MAD) per encoder in every case | `<= 10%` |

All 256 cases are measured even when an earlier case exceeds `1.10`, so a single
run captures all remaining optimization targets.

---

## Timing and allocation measurement methods

### Encoding latency

1. Input images are loaded into memory before timing.
2. A 3-iteration native calibration encode selects a shared iteration count
   targeting `50 ms` per block (clamped to `3..100` encodes).
3. Seven paired blocks alternate which encoder runs first (`native/managed` vs.
   `managed/native`). Before each block, both encoders run 5 untimed warmup
   encodes.
4. Each individual encode is timed (`Stopwatch` for managed, `steady_clock` for
   native) from encoder creation through final JPEG output in newly allocated
   memory. Managed GC during encoding is included; file I/O, process startup,
   output verification, and freeing the native output buffer happen outside
   timed intervals.
5. Each case compares the median of the 7 block samples for each encoder without
   discarding outliers.

### Allocations per encode

The untimed allocation job (`--allocation-evidence`) runs 5 warmups followed by
3 observed complete encodes for each of the 192 Kodak photograph cases:

- **DotJpegli:** Reports exact `GC.GetTotalAllocatedBytes` deltas (including
  object headers, alignment, and the returned `byte[]` JPEG).
- **Native Google Jpegli:** Uses `jpegli-allocation-native`, which wraps
  `malloc`, `calloc`, `realloc`, `aligned_alloc`, `posix_memalign`, `memalign`,
  and C++ `operator new` via GNU linker `--wrap` flags to sum requested heap
  bytes (including output buffer growth, excluding allocator metadata and
  internal `libc` allocations). See the
  [allocation protocol](../tools/DotJpegli.Performance/README.md#untimed-managednative-allocation-observations).

---

## CI workflow markers and targeted runs

All benchmarks run on CI ([`AGENTS.md`](../AGENTS.md)). An ordinary push to
`main` runs 4 targeted Ubuntu `.NET 11` benchmark cases (`kodim01` at distance
`1`, `4:4:4`/`4:2:0`, levels `0`/`2`). Include markers in the commit message on
`main` to run additional suites in the same automatic workflow:

| Commit marker | CI suite |
| --- | --- |
| `[full-parity]` | Runs the full 1,280-pair [quality/compression parity](quality-compression-parity.md) matrix, the full 256-case native latency matrix, and the 192-case allocation job. |
| `[performance-workloads]` | Runs the 26 bounded large-image, cache-boundary, memory/RSS, and parallel/concurrent workload scenarios on x64 and ARM64 ([workload method](../tools/DotJpegli.Performance/README.md#bounded-workload-diagnostics)). |
| `[compare-release]` | Runs the 256-case paired ABBA/BAAB regression comparison against NuGet `1.0.0` (`a40868b`) across 6 lanes (`.NET 10`/`.NET 11` on x64 default, x64 narrow AVX2, and ARM64), plus `algorithm-scenarios-*` A/B checks ([release comparison protocol](../tools/DotJpegli.Performance/README.md#regression-comparison-against-released-100)). |
| `[compare-release-wide]` | Runs only the x64 `.NET 11` default lane of `[compare-release]` with `DOTJPEGLI_REQUIRE_WIDE_SIMD=1` requiring hardware `Vector512`. |
| `[compare-baseline]` | Interleaves 32 cases against baseline `2ef3d5f` (with and without AVX-512) using 6 balanced ABBA/BAAB quartets. |
| `[compare-encoder]` | Interleaves 64 ordinary cases plus 4 Display-P3 `SourceIccProfile` cases against `4dd88ec` on x64 and ARM64 ([batch comparison guide](../tools/DotJpegli.Performance/README.md#encoder-optimization-batch-comparisons)). |
| `[compare-kernels]` | Compares `Original`, `Optimized`, and `Automatic` fuzzy-erosion and narrow-zigzag kernels across threshold-bracketing textures on x64 and ARM64. |
| `[compare-net11]` | Compares `kodim01` 4:2:0 sequential across `.NET 10` and `.NET 11` runtimes and `.NET 11` `Zip`/`Unzip` SIMD vs. `.NET 10` lane assembly. |

To run a custom selection of images or counts via manual workflow dispatch (when
no automatic run already covers the commit):

```sh
gh workflow run ci.yml --ref main -f mode=targeted \
  -f framework=net11.0 -f images=kodim03 -f distances=4 -f samplings=420 -f progressions=2 \
  -f blocks=3 -f iterations=3 -f warmups=5
```

When a targeted or full latency run fails a bound, CI automatically captures
`dotnet-trace` CPU profiles (`.nettrace` and Speedscope JSON) for the highest-ratio
cases into `performance-profile-<revision>` (pass `-f profile=false` on manual
dispatch to skip profiling).

Local validation is limited to building the runner and running its synthetic
self-test (no encoding or timing):

```powershell
dotnet build tools/DotJpegli.Performance/DotJpegli.Performance.csproj -c Release
dotnet run --project tools/DotJpegli.Performance -c Release -f net11.0 --no-build -- --self-test
```

---

## Additional retained snapshots

- **Release regression against 1.0.0 (`0104778`):**
  The [release regression snapshot at `0104778`](performance-evidence/2026-11-03-0104778/release/README.md)
  compared 1,536 cases (256 × 6 lanes) against `1.0.0`: 1,310 favored `0104778`,
  116 were inconclusive, and 110 favored `1.0.0` (99 on `.NET 10` x64 default
  and 11 on `.NET 11` x64 default; the other 4 lanes passed with 0 regressions).
- **Large-image, cache, and parallel workloads (`0104778`):**
  The [workload snapshot at `0104778`](performance-evidence/2026-11-03-0104778-workloads/README.md)
  retains all 26 scenarios on x64 and ARM64, including 64 MiB converted-plane
  and coefficient-cache boundaries, 9.8 MP photo enlargements, cold vs. warm
  default-tiering batches, 1/2/4 workers, and 2/4 concurrent encodes.
- **Kernel guard assessment (`692a052`):**
  The [kernel guard study](performance-evidence/2026-11-03-kernel-guard-692a052/README.md)
  evaluated the `32,768`-pixel floor for tiled narrow zigzag ordering across
  x64 and ARM64 on `.NET 10` and `.NET 11`.
- **Compact coefficient storage study (`2026-11-03`):**
  The [untimed compact coefficient study](performance-evidence/2026-11-03-compact-coefficients/README.md)
  evaluated 32-block bitmap and bit-packed coefficient tiles across 360 AQ
  configurations, 75 adversarial inputs, and 30 shared-budget capacity trials.
