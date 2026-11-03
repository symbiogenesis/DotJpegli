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
  at least 32,768 source pixels. The
  [retained threshold, tall-image and intermediate-size assessment](docs/performance-evidence/2026-11-03-kernel-guard-692a052/README.md)
  supports keeping this conservative deployment floor on the measured x64
  hosts; it is not a measured crossover. Other narrow paths use Original;
  normal wide-SIMD dispatch remains available.
- Kernel preferences are immutable per encode. There is no startup benchmark,
  shared mutable selector or timing-dependent JPEG output.
- Scalar DCT uses fixed eight- and four-point schedules, preserving the original
  recursive arithmetic without recursive calls or per-stage scratch spans.
- The .NET 11 eight-row transpose shared by DCT and batched AQ modulation
  interleaves full-width rows with sixteen Zip operations instead of thirty-two
  half-width Zip operations, retaining the same half reads and joins. Other
  targets retain the four-panel schedule. Independent lanes and exact payload
  bits are covered; fewer managed operations do not establish a machine-shuffle
  or latency improvement.
- AQ scratch has encode-owned bounded pooling, oversized-array fallback and
  strengths-row reuse across passes. Identical clamped pre-erosion rows reuse
  their rounded results at image boundaries.
- AQ pre-erosion reduces completed four-pixel groups directly into its output,
  removing one padded-width float workspace and its rental/return. Scalar and
  vector paths preserve the original vertical and horizontal addition order.
  An independent staged-stencil oracle checks intermediate bits and pool tails;
  workspace failure and cleanup checks cover the reduced rental count. Latency
  and peak-working-memory effects remain unmeasured.
- Scalar AQ modulation shares one block traversal for high-frequency and gamma
  analysis, carrying horizontal neighbors through the first seven columns and
  handling the final column separately. Independent raster-order sums and the
  original rounding are retained without additional scratch.
- Eight-block SIMD AQ modulation carries each horizontal neighbor into the next
  column and handles the final column separately, preserving each lane's
  high-frequency and gamma accumulation order without additional scratch.
- The bounded [AQ rounding-cell investigation](docs/performance-evidence/2026-11-03-aq-reciprocal/README.md)
  certifies reciprocal candidates against the existing binary32 rational
  intermediates with exact midpoint-product comparisons and original division
  fallback. Independent integer proofs, scalar/vector field regressions and
  96 untimed complete-JPEG comparisons preserve exact bits and bytes. Retained
  evidence includes actual per-site and affected-vector-batch fallback counts.
  Automatic retains original division; this completes the numerical proof and
  fallback study without a latency, allocation or peak-memory claim.
- Prepared ICC destination transforms retain the exact curve endpoints already
  computed during inverse validation. Pixel conversion reuses those values;
  shared curves remain immutable and inverse arithmetic is unchanged.
- Parametric ICC curves prepare the exact reciprocal exponent and type 1/2
  branch threshold once. Per-pixel evaluation reuses these immutable values
  without changing power evaluation, clipping or rounding.
- The bounded [AC reciprocal-interval investigation](docs/performance-evidence/2026-11-03-ac-reciprocal/README.md)
  derives adjacent reciprocal bounds for every supported divisor, certifies
  suppression and ties-to-even agreement, and retains exact scalar/SIMD division
  for ambiguous intervals. Focused regressions verify boundaries, exceptional
  values and output prefixes. All fourteen valid prepared-kernel comparisons on
  the measured ARM64 host favor division despite rare fallback on image-derived
  samples. This prototype is rejected for deployment; production division is
  unchanged. The result does not reject every possible reciprocal algorithm.
- Entropy-token scans share one encode-owned 64 MiB budget, accounting actual
  accepted rental lengths. Earlier scans acquire capacity first. Abandoning a
  scan refunds its rentals without invalidating siblings; successful replay
  releases storage early. Which scans remain cached can change with this policy.
- Entropy words use a single capacity check for byte stuffing when sixteen
  output bytes remain; the boundary path retains per-byte drain behavior.
- Portable SIMD byte stuffing expands eight-byte words into the existing
  sixteen-byte reservation using an immutable 4 KiB shuffle table. Internal
  per-encode preferences cover serial, restarted and unaligned parallel replay;
  software and portable-library paths retain scalar expansion. The
  [completed bounded comparison](docs/performance-evidence/2026-11-03-byte-stuffing/README.md)
  favors SIMD in one valid direct-helper case but establishes no complete-encode
  or portable deployment benefit. Automatic retains Original; no larger staging
  batches are introduced. Exact bytes, padding, destination writes, partial
  failures and cancellation have focused coverage.
- Single-context entropy-token replay reuses its immutable prepared Huffman
  lookup directly, avoiding stack clearing and copying for that scan. Multiple
  contexts retain contiguous staging and strict fallback behavior.
- Progressive refinement correction bits fill up to 24 bits per cached raw token.
  Direct emission retains its existing grouping; fewer token records can keep
  more corrections within the unchanged shared rental budget.
- Opt-in parallel encoding replays complete cached-token scans at existing
  restart boundaries using the scan's global Huffman tables. At most 32 segments
  use 4 MiB of accepted pooled staging plus bounded worker scratch. Segments
  exceeding that budget and uncached scans retain serial emission. Destination
  writes remain on the caller's thread in their original order and sizes;
  no restart markers are added. Latency and peak memory remain unmeasured.
- Complete cached scans without restart markers also support opt-in parallel
  entropy replay. Ordered token collection resolves DC prediction, progressive
  EOB runs and correction bits before workers pack unstuffed chunks. One caller
  writer merges their exact bits, preserving stuffing, padding and destination
  writes. Batches retain at most 32 chunks under the same 4 MiB accepted-rental
  cap; unsupported chunks and uncached scans retain serial emission. Latency
  and peak memory remain unmeasured.
- Kernel diagnostics include deterministic controls below, at and above the
  32,768-pixel guard, an 8-by-4,096 tall image and 64K/128K intermediate images.
  They run in bounded sequential selections within the optional CI kernel
  comparison and do not expand the fixed native acceptance corpus.
- Same-host revision diagnostics use persistent workers and balanced ABBA/BAAB
  quartets with identical runner bytes, inputs, runtime, CPU affinity, encoding
  policy and iteration counts. Encoder commit metadata and JPEG hashes must
  match the declared comparison. MAD, minority timing states, repeated-leg drift
  and order bias invalidate affected rows; reports retain invalid and
  inconclusive results. The automatic `[compare-baseline]` run collects normal
  and narrow-SIMD selections sequentially. See the
  [comparison protocol](tools/DotJpegli.Performance/README.md).
- Converted planes, fused DCT/quantization, prepared quantizers, coefficient and
  token caches, histogram planning and buffered entropy emission already have
  optimized paths. New proposals must be compared against the current encoder.
- The bounded [compact coefficient-cache comparison](docs/performance-evidence/2026-11-03-compact-coefficients/README.md)
  compares bitmap/packed-short and per-band bit-packed tiles with dense storage.
  Exact block, direct-DC and individual progressive lookups preserve all values.
  Real sparse/noisy coefficients, adversarial full-width values, actual pooled
  payload/index lengths and shared 64 MiB admission trials expose both savings
  and metadata regressions. The prototypes remain offline; production retains
  dense storage. Work counters describe these codecs, without a latency,
  allocation, construction-overlap or peak-memory claim.
- The opt-in [offline progressive scan search](tools/DotJpegli.Parity/README.md#bounded-progressive-scan-search)
  compares 181 complete JPEG candidates across spectral boundaries, refinement
  depths, component orders and standard/optimized Huffman assignments. It keeps
  final real coefficients fixed, includes the existing level-2 control, retains
  every candidate's size, script and table selectors, and charges all encode and
  logical entropy passes. It makes no global optimum or latency claim.
- The opt-in [offline quantization-table search](tools/DotJpegli.Parity/README.md#bounded-quantization-table-search)
  caches 18 band-option costs in one DCT traversal and screens 729 joint
  luma/chroma table configurations. At most 17 finalists are completely encoded
  with AQ and optimized Huffman coding and decoded by the pinned native wrapper.
  The smallest complete JPEG meeting the control PSNR, local SSIM and offline
  multiscale structural scores wins, with the unchanged default control included
  first. Reports retain all tables,
  surrogate costs, exact trials, rejections, hashes and operation counts. This is
  a bounded offline experiment, without a global optimum, visual-quality or
  corpus-wide compression claim.
- The opt-in [offline rate-distortion quantization search](tools/DotJpegli.Parity/README.md#bounded-rate-distortion-quantization-search)
  compares 25 complete baseline JPEGs across five distances and five AC search
  weights, retaining the unchanged AQ/optimized-Huffman control. A bounded
  dynamic program chooses each original AC value, one step toward zero or zero
  using squared DCT error and standard-table run/category costs, including ZRL
  and EOB; DC stays fixed. Complete encodes relearn optimized Huffman tables.
  Reports retain decoded PSNR, local SSIM and the multiscale structural score,
  PSNR/SSIM matched-size and matched-quality curves, unsupported ranges, hashes
  and search-work counts. Default bytes and public
  APIs are unchanged; corpus compression and latency remain unmeasured.
- Offline quantization searches reuse immutable source features for PSNR/local
  SSIM and a specified five-scale structural objective. Both independent
  reference scorers validate the control and every provisional winner before
  acceptance; all three control floors apply. Schema 2 reports charge feature
  preparation, comparisons and reference validation and retain every score.
  Focused regressions check exact score bits and reproduce eligibility and
  winner bytes. This adds image-sized offline storage; latency, allocations and
  peak memory remain unmeasured. See the [objective implementation](tools/DotJpegli.Parity/README.md#reusable-offline-quality-objectives).

These mechanisms have focused numerical, byte-parity, ownership and failure-path
regressions. They do not establish a latency or peak-working-memory benefit for
every workload. Full CI results and performance evidence must be assessed for
the revision being delivered.

## Immediate validation and measurement work

Measure shared token budgeting on large progressive and custom-script images
whose scans compete for capacity. Compare cached/fallback scan behavior,
latency, allocated bytes, retained rental capacity and peak working memory.
Include sparse and noisy controls; check whether early-scan priority harms
later scans. The 64 MiB token cap must remain fixed.

Do not infer speed from operation counts, reduced allocations from pool reuse,
or lower peak memory from allocated-byte totals. Include photograph, low-noise,
low-contrast and tiny odd-edge controls, both distances and sampling modes,
baseline/progressive output and dedicated cache-pressure workloads as relevant.

## Architectural candidates

### Larger-image and ICC workspaces

Profile dedicated large-image, source-ICC, smoothing and parallel workloads.
Evaluate banded converted planes with the AQ halo and a coordinated memory
policy across the independent converted-plane, coefficient and token caches.
For ICC workloads, investigate specialized immutable curve/CLUT evaluation and
certified approximations only after identifying their cost. RGB8 measurements
without source ICC do not establish a benefit.

## Optional compression and tuning experiments

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
