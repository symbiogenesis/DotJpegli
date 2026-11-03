# Algorithm and allocation improvement plan

## Scope and constraints

Improve encoding speed, allocations and bounded working memory while preserving
JPEG bytes and the safe, managed-only, portable, dependency-free runtime contract.
Quality-changing experiments must be explicit opt-ins with the default path
unchanged. Public API changes require a separate design.

This document describes the current state and remaining work. Implementation
details belong in [development documentation](docs/development.md); measurement
protocols and retained evidence belong in
[performance documentation](docs/performance-parity.md). Git history records
completed work.

## Current state

- AQ retains insertion and selection-network fuzzy erosion. Automatic uses
  insertion because measurements do not justify a portable selection guard.
- Narrow transposed zigzag ordering retains original and tiled schedules.
  Automatic uses tiled ordering on x64 with accelerated 256-bit SIMD, eight
  float lanes, disabled 512-bit SIMD, both source dimensions at least eight and
  at least 32,768 source pixels. The size floor is provisional. Other narrow
  paths use Original; normal wide-SIMD dispatch remains available.
- Kernel preferences are immutable per encode. There is no startup benchmark,
  shared mutable selector or timing-dependent JPEG output.
- Scalar DCT uses fixed eight- and four-point schedules, preserving the original
  recursive arithmetic without recursive calls or per-stage scratch spans.
- AQ scratch has encode-owned bounded pooling, oversized-array fallback and
  strengths-row reuse across passes. Identical clamped pre-erosion rows reuse
  their rounded results at image boundaries.
- Scalar AQ modulation shares one block traversal for high-frequency and gamma
  analysis, retaining independent raster-order sums and the original rounding.
- Eight-block SIMD AQ modulation carries each horizontal neighbor into the next
  column and handles the final column separately, preserving each lane's
  high-frequency and gamma accumulation order without additional scratch.
- Prepared ICC destination transforms retain the exact curve endpoints already
  computed during inverse validation. Pixel conversion reuses those values;
  shared curves remain immutable and inverse arithmetic is unchanged.
- Entropy-token scans share one encode-owned 64 MiB budget, accounting actual
  accepted rental lengths. Earlier scans acquire capacity first. Abandoning a
  scan refunds its rentals without invalidating siblings; successful replay
  releases storage early. Which scans remain cached can change with this policy.
- Entropy words use a single capacity check for byte stuffing when sixteen
  output bytes remain; the boundary path retains per-byte drain behavior.
- Progressive refinement correction bits fill up to 24 bits per cached raw token.
  Direct emission retains its existing grouping; fewer token records can keep
  more corrections within the unchanged shared rental budget.
- Kernel diagnostics include deterministic controls below, at and above the
  32,768-pixel guard, an 8-by-4,096 tall image and 64K/128K intermediate images.
  They run in bounded sequential selections within the optional CI kernel
  comparison and do not expand the fixed native acceptance corpus.
- Converted planes, fused DCT/quantization, prepared quantizers, coefficient and
  token caches, histogram planning and buffered entropy emission already have
  optimized paths. New proposals must be compared against the current encoder.

These mechanisms have focused numerical, byte-parity, ownership and failure-path
regressions. They do not establish a latency or peak-working-memory benefit for
every workload. Full CI results and performance evidence must be assessed for
the revision being delivered.

## Immediate validation and measurement work

1. Measure shared token budgeting on large progressive and custom-script images
   whose scans compete for capacity. Compare cached/fallback scan behavior,
   latency, allocated bytes, retained rental capacity and peak working memory.
   Include sparse and noisy controls; check whether early-scan priority harms
   later scans. The 64 MiB token cap must remain fixed.
2. Measure Automatic narrow ordering around the 32K-pixel guard, at intermediate
   sizes and on unusually narrow/tall images. Compare the combined Automatic
   policy as well as isolated forced kernels on .NET 10/11, x64 and ARM64.
   The CI kernel comparison now supplies threshold, tall-image and 64K/128K
   intermediate controls; assessment of retained results remains open.
   Retain the original fallback where evidence remains inconclusive.
3. Use interleaved same-host revision comparisons with identical inputs,
   runtime, CPU affinity, encoding policy and JPEG hashes. Detect minority
   timing states and order bias as well as MAD and drift; native drift alone
   cannot correct mixed timing regimes. Report invalid and inconclusive rows.

Do not infer speed from operation counts, reduced allocations from pool reuse,
or lower peak memory from allocated-byte totals. Include photograph, low-noise,
low-contrast and tiny odd-edge controls, both distances and sampling modes,
baseline/progressive output and dedicated cache-pressure workloads as relevant.

## Bounded speed and memory candidates

### Exact SIMD scheduling

Search DCT transpose and AQ stencil/modulation schedules for fewer shuffles,
shorter dependencies, lower register pressure and fewer spills. Preserve each
floating-point operation's evaluation order. Count packing and staging costs,
retain portable fallbacks and validate independent lanes, guards and whole
encodes before measuring.

[Minotaur](https://arxiv.org/abs/2306.00229),
[Isaria](https://doi.org/10.1145/3617232.3624873) and
[AlphaEvolve](https://arxiv.org/abs/2506.13131) offer search methods, not direct
.NET compiler extensions or established gains for these kernels.

### Rounding-aware numerical fast paths

Research AC quantization reciprocal intervals before changing production
division. Derive a bound containing the existing rounded quotient, accept a
fast result only when both bounds preserve bias comparisons and final integer
rounding, and use exact division for ambiguous boundaries and exceptional cases.
Measure fallback frequency and the cost of interval checks before deployment.

AQ rational functions, `FastLog2` and `FastPow2` require equivalent care:
correctly rounded mathematical functions need not match the existing
approximations. Preserve numerical ordering, saturation, validation and output
prefixes. The interval proof and fallback-rate evidence remain open.

[RLIBM's fast evaluation work](https://people.cs.rutgers.edu/~sn349/papers/cgo-2023-rlibm.pdf)
and [RLIBM-PROG](https://arxiv.org/abs/2111.12852) motivate rounding-aware search;
they do not prove parity for these adaptations.

### SIMD byte stuffing and token packing

Profile `BitWriter.WriteEntropyTokens` and `WriteWord` before adding larger
staging batches. The current no-`FF` path already handles eight-byte words.
Stuffed words also share a capacity check when their maximum sixteen bytes fit.
Compare saved branches against staging, copying and insertion costs. Preserve
bit order, padding, restart markers, cancellation and destination-failure
prefixes. [PJRD](https://github.com/jstankowski/PJRD) supplies a reference approach
to parallel encoding and SIMD stuffing, not measured DotJpegli gains.

## Architectural candidates

### Parallel entropy coding

Start with restart boundaries already requested by the caller: build global
Huffman tables, encode independent segments into bounded pooled buffers and
concatenate in order. Preserve bytes for identical restart settings and account
for additional buffers. Adding restarts solely for threading can increase size.

A later baseline-only design could emit unstuffed bit chunks, merge at bit
boundaries and stuff bytes without new restarts. Correct preceding DC predictors
are required. Progressive EOB runs and correction-bit dependencies need a
separate design.

### Compact coefficient cache

Compare nonzero bitmaps plus packed shorts with bit-packed tiles in
`QuantizedImage`. Preserve inexpensive DC access and progressive lookup, retain
dense fallback, and account for packing, unpacking, rank/indexing and actual
pooled bucket sizes. Sparse and noisy images must both be measured. Determine
whether keeping more images below the 64 MiB cache limit offsets representation
costs. [FastLanes](https://ir.cwi.nl/pub/33289/) is an internal-layout inspiration,
not a replacement JPEG bitstream.

### Larger-image and ICC workspaces

Profile dedicated large-image, source-ICC, smoothing and parallel workloads.
Evaluate banded converted planes with the AQ halo and a coordinated memory
policy across the independent converted-plane, coefficient and token caches.
For ICC workloads, investigate specialized immutable curve/CLUT evaluation and
certified approximations only after identifying their cost. RGB8 measurements
without source ICC do not establish a benefit.

## Optional compression and tuning experiments

- **Rate-distortion optimized quantization:** Begin baseline-only with bounded
  AC coefficient searches over distortion plus estimated run/category cost.
  Use current AQ and optimized Huffman coding as the control. Compare matched
  quality and matched size with multiple metrics; account for added CPU work.
- **Offline quantization-table optimization:** Cache per-band candidate costs,
  search luma/chroma tables jointly and exact-encode finalists. Run lengths,
  Huffman tables, AQ, stuffing and headers are not separable. A target-bytes API
  would require a separate opt-in design and actual-size verification.
  [RDOEA](https://www.sciencedirect.com/science/article/pii/S0950705122002143) and
  [RDOEA_FR](https://www.sciencedirect.com/science/article/pii/S1568494626001456)
  provide candidate search techniques.
- **Content-dependent progressive scans and table assignment:** Use the custom
  scan API to search spectral boundaries, refinement depths and Huffman table
  assignments. Preserve final quantized coefficients when changing only the
  schedule. Compare complete JPEG sizes, including definitions, padding, EOB
  aggregation and stuffing, and charge the additional candidate passes.
- **Faster offline perceptual objectives:** Screen candidates with reusable
  reference features or faster metrics, then validate finalists with the
  reference perceptual metric and existing PSNR/SSIM checks. Faster metric
  evaluation is not encoder acceleration or identical-score evidence.
- **Domain-specific learned presets:** Investigate only with an explicit
  workload and task metric. Matched recognition accuracy differs from matched
  visual quality. Keep training and neural dependencies outside the runtime.

## Acceptance and delivery

- Preserve scalar/vector evaluation order, threshold comparisons, rounding,
  JPEG bytes, errors, cancellation and destination-output prefixes for default
  encoding changes. Keep exclusive encode ownership and bounded cleanup on
  success, cancellation and exceptions.
- Use local Release builds and focused regressions for changed kernels,
  workspace lifetimes, complete-encode parity and portable/software fallbacks.
  Full test suites and every benchmark run only on CI; filtered runs must not
  approximate a full suite.
- Make self-contained logical commits as soon as each change is ready. Finish
  a batch locally, then push its settled revision once to `main`.
- Reuse the automatic push CI run. Inspect existing runs before any manual
  dispatch, reuse queued/running/successful runs and prefer rerunning a failed
  run when a retry is justified. Dispatch only when no automatic run exists for
  the revision or the user requests an additional run. Collect planned
  diagnostics in that run.
- Keep fixed native acceptance bounds. Separate baseline improvement, native
  parity, allocations, rental capacity and peak memory. Report regressions and
  unstable measurements without relaxing thresholds or claiming universal gains.
- Format/decoder changes, ANS or neural entropy replacements, and runtime
  training dependencies are outside scope. Asymptotic matrix/DFT results and
  approximate transforms do not justify replacing exact small floating-point
  kernels without concrete numerical and performance evidence.
