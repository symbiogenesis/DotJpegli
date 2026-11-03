# Algorithm and allocation improvement plan

Research window: November 2021 through November 2, 2026.
Implementation baseline: `2ef3d5fcc12a804c7cd707446091eb74fa9c6476`.

This plan records candidates from the research survey, their fit to DotJpegli,
and the work required to evaluate them. A paper's improvement over its own
baseline is not evidence of an improvement over this encoder. Operation counts
are useful screening criteria; wall time, allocations and working memory must
be measured separately.

## Approved first batch

The first batch preserves JPEG bytes and the managed-only, safe, portable,
dependency-free runtime contract. It covers three bounded changes:

1. Improve the four-of-nine minimum-selection network in adaptive quantization.
2. Improve an exact SIMD permutation schedule, initially narrow transposed-to-
   zigzag coefficient ordering.
3. Reuse bounded adaptive-quantization workspace and row strengths, with clear
   ownership and cleanup on success, cancellation and exceptions.

These three implementation tasks and their validation are complete. Focused local
checks and both automatic runs' full tests pass. Selected-case measurements show
lower allocations and a modest overall latency improvement, with individual
regressions and timing limitations recorded below. Further research stays pending.
Larger architectural changes and quality-changing experiments remain pending.

## Evidence and constraints

The retained [EPYC 9V45 evidence](docs/performance-evidence/2026-11-02-eb86172/README.md)
tests `eb86172`, which precedes several optimizations in the implementation
baseline. Its instrumented samples are target-selection hints, not current
timings or acceptance evidence. Restricting event aggregation to the managed
benchmark stack gives these approximate inclusive shares:

| Case | Adaptive quantization | Entropy-token replay | Other observation |
| --- | ---: | ---: | --- |
| low-noise/444/p0/d4 | 31% | 5% | Pre-erosion about 10%; eight-block modulation about 10%. |
| low-contrast/444/p0/d4 | 26% | 11% | Pre-erosion about 9%; eight-block modulation about 6%. |
| tiny-odd-edges/444/p2/d1 | 14% | 13% | Huffman planning about 42%; later code already improves its cost estimator. |

Four-of-nine selection accounts for about 2.4-3% in the low-detail traces. Even
halving that kernel would save only about 1.2-1.5% of total encoding time in
those cases. Halving a stage that occupies 31% saves about 15.5% overall.
Inlining obscures some individual DCT costs; do not infer a DCT percentage from
the coordinator's apparent self time.

Retained managed allocation totals average about 39 KB per tiny baseline
encode, 70 KB per tiny progressive encode, and 72-74 KB for selected low-detail
cases. These include setup and returned output. Pool rentals, retained pool
capacity and peak working set are different measurements from allocated bytes.

Existing optimizations must not be counted as new work: converted-plane reuse,
compact 4:2:0 planes, fused DCT/quantization including padded blocks, prepared
quantizers and transposed layouts, overlapping AQ rows, eight-block modulation,
SIMD nonzero masks, sparse Huffman special cases, packed histogram cost
estimation, conservative AC suppression before division, tiny-token replay
without lookup preparation, pooled coefficient/token storage and buffered
64-bit entropy emission.

## Speed and memory candidates

### 1. Four-of-nine selection network — first batch

Location: `AdaptiveQuantization.ComputeFuzzyErosion` and its scalar helper.
The baseline sorts four values and inserts five more, using 45 min/max
operations. Search or prune a nine-input comparison network to retain only the
four smallest sorted outputs, minimizing both operation count and dependency
depth. Keep the subsequent weighted sum's evaluation order unchanged.

[AlphaDev (June 2023)](https://deepmind.google/blog/alphadev-discovers-faster-sorting-algorithms/)
motivates searching small kernels with strict correctness oracles. This does
not mean importing AlphaDev's sort-three routine or claiming a new optimal
network. Verify all 512 binary inputs, all 9! distinct permutations, ties,
supported scalar/vector paths and integration with existing AQ regressions.

### 2. Exact SIMD scheduling — first batch, then further search

Locations: `Quantization` transposed zigzag conversion, `ForwardDct` transposes,
and AQ stencil/modulation scheduling. Optimize shuffles, live ranges, spills and
instruction dependencies without reassociating floating-point arithmetic.

[Minotaur (OOPSLA 2024)](https://arxiv.org/abs/2306.00229),
[Isaria (ASPLOS 2024)](https://doi.org/10.1145/3617232.3624873) and
[AlphaEvolve (2025)](https://arxiv.org/abs/2506.13131) provide transferable
synthesis and evaluator-driven search techniques. They are not direct .NET
compiler extensions. Isaria's 2026 CACM article revisits the earlier work.

The initial experiment packs adjacent transposed columns into 2x4 tiles so
each zigzag output vector draws from fewer inputs. Count packing costs as well
as the shuffles eliminated. Check every input bit independently, signed-short
patterns, output guards, DC-only blocks and .NET 10/11 portable fallbacks.
Further DCT and AQ schedule searches remain pending after this first change.

### 3. Bounded workspace reuse — first batch

Locations: AQ scratch arrays and row strengths used by sequential, component,
progressive and coefficient-cache passes. Reuse scratch through an encode-owned
lease, with bounded pooling and a nonpooled fallback for oversized buffers.
Reuse strengths between passes instead of allocating another row array.

This is conventional engineering, not a recent mathematical breakthrough.
Retain exclusive ownership during an encode; avoid shared mutable workspaces or
unbounded thread-local caches. Verify constructor rollback, deterministic
cleanup, cancellation, I/O exceptions, repeated disposal, concurrent independent
encodes and numerical parity with unpooled/dirty buffers. Public API changes
are outside this batch.

### 4. Rounding-aware numerical fast paths — pending research

Locations: AQ rational functions, `FastLog2`, `FastPow2`, and quantization
division. [RLIBM's fast evaluation work (2023)](https://people.cs.rutgers.edu/~sn349/papers/cgo-2023-rlibm.pdf)
uses output rounding intervals when generating efficient polynomials.
[RLIBM-PROG](https://arxiv.org/abs/2111.12852), first submitted November 25,
2021, is also within this survey's window.

Possible adaptations include specialized evaluation schedules or a reciprocal
estimate with a proven uncertainty interval and exact-division fallback near
zero-bias and half-integer boundaries. These are proposed adaptations, not
guarantees supplied by the papers. Replacing division with multiplication alone
is known to change outputs. Correctly rounded real log/exp functions also need
not match the existing upstream approximations. Require a proof of the final
quantized result, or evaluate as an explicit quality-changing option.

The interval proof and fallback-rate measurements remain pending. This candidate
is outside the shared token-budget batch below.

### 5. Parallel entropy coding — pending architecture work

[PJRD (SoftwareX, September 2026)](https://www.sciencedirect.com/science/article/pii/S2352711026002669)
and its [author implementation](https://github.com/jstankowski/PJRD) describe
parallel baseline JPEG encoding, SIMD stuffing and RDOQ. Current DotJpegli
parallelism leaves quantization, histogram collection and entropy writing ordered.

Start with restart boundaries already requested by the caller: build global
Huffman tables, encode independent segments into bounded pooled buffers, then
concatenate in order. Preserve bytes for identical restart settings and account
for additional buffers. Adding restarts solely for threading can increase size.

A later baseline-only experiment can emit unstuffed bit chunks without new
restarts, then merge at bit boundaries and stuff bytes. Correct preceding DC
predictors are required. Progressive EOB and correction-bit dependencies need
separate designs; PJRD does not establish their solution.

### 6. SIMD byte stuffing and token packing — pending

Location: `BitWriter.WriteEntropyTokens` and `WriteWord`. PJRD motivates larger
byte batches, vector detection of `FF`, and bulk copying/insertion. Current code
already handles eight-byte words without `FF` efficiently. Measure whether
saved branches exceed staging/copy costs. Preserve bit order, padding, restart
markers, cancellation and destination-failure behavior.

### 7. Compact coefficient cache — pending

Location: `QuantizedImage`, currently 128 bytes per dense 8x8 block. Compare
nonzero bitmaps plus packed shorts with bit-packed tiles, retaining inexpensive
DC access and progressive coefficient lookup. The [FastLanes layout (2023)](https://ir.cwi.nl/pub/33289/)
is an inspiration for internal representations, not a JPEG bitstream replacement.

Potential benefits are lower bandwidth and keeping larger images below the
64 MiB coefficient-cache limit, avoiding recomputation. Costs include packing,
unpacking, rank/indexing and noisy-image expansion. Keep dense fallback and
account for actual pooled bucket sizes. Benchmark both sparse and dense images.

### 8. Dynamic token budget across scans — implemented, CI pending

Location: `EntropyTokenScans`. The former equal division of 64 MiB by scan count
could leave capacity unused by a small DC scan while a large AC scan abandoned
caching. One encode-owned budget now accounts actual accepted rental lengths
across scans on demand, retaining the cap and complete-sequence fallback.
Implementation and bounded local validation are complete; automatic CI validation
is pending. This is an engineering change, not a literature-derived breakthrough.

### 9. Larger-image and ICC workspaces — pending, lower priority

Evaluate banded converted planes with the AQ halo and a coordinated memory
policy across the three independent 64 MiB caches. For ICC workloads, consider
specialized immutable curve/CLUT evaluation and certified approximations.
Current no-ICC RGB8 evidence cannot establish their benefit. Profile dedicated
large-image, source-ICC, smoothing and parallel workloads before implementation.

## Compression and tuning candidates

### 10. Optional rate-distortion optimized quantization — pending

PJRD supplies a recent implementation of a technique whose mathematical roots
are older. Minimize `D + lambda * R` over nearby integer coefficients, including
run/category costs. Start baseline-only with a bounded search over AC decisions.
Use current AQ plus optimized Huffman coding as the control. Expect extra CPU
work and changed reconstruction; evaluate matched quality and matched size with
more than a single objective metric. Keep the default encoding path unchanged.

### 11. Offline joint quantization-table optimization — pending

Custom tables are already supported. [RDOEA (2022)](https://www.sciencedirect.com/science/article/pii/S0950705122002143)
uses cached per-band costs to accelerate table search.
[RDOEA_FR (April 2026)](https://www.sciencedirect.com/science/article/pii/S1568494626001456)
addresses fixed-rate constraints and joint luma/chroma optimization.

Transform once, cache candidate distortion/rate estimates, search offline and
ship only selected constants. Exact-encode finalists because run lengths,
Huffman tables, AQ, stuffing and header costs are not separable. A future target-
bytes API would be an opt-in slower feature with final actual-size verification.

### 12. Content-dependent progressive scans and table assignment — pending

Use the existing custom scan API to search spectral boundaries, refinement
depths and Huffman table assignments. Evaluate complete JPEG sizes, including
definitions, padding, EOB aggregation and stuffing. Preserve final quantized
coefficients if only the scan schedule changes. Extra candidate passes trade
encoding time for size. This combines established scan optimization with recent
evaluator-driven search, rather than applying a new scan-selection theorem.

### 13. Faster perceptual objectives for offline tuning — pending

[Fast SSIMULACRA2 (IWAIT 2026)](https://pure.nitech.ac.jp/ja/publications/fast-implementation-of-ssimulacra2-for-image-quality-assessment/)
uses faster filtering, reusable reference features and feature selection.
Use it to screen table/AQ candidates, then validate finalists with the reference
metric and existing PSNR/SSIM checks. Its reported speedup is metric evaluation,
not encoder speed; correlation preservation does not mean identical scores.

### 14. Domain-specific learned presets — pending, specialized

[Differentiable JPEG (WACV 2024)](https://arxiv.org/abs/2309.06978) can support
offline parameter training. Improvements in its differentiable codec model's
fidelity are not production compression gains.
[J4D (June 2026)](https://arxiv.org/abs/2606.16185) targets downstream DNN
accuracy. Investigate only for machine-vision workloads with an explicit task
metric. Matched recognition accuracy is different from matched visual quality.
Keep training and neural dependencies outside the managed runtime library.

## Theoretical watchlist and exclusions

OpenAI's [October 2026 release](https://openai.com/index/sharing-ai-progress-in-mathematics/)
contains research at different verification stages. Use each result's precise
formalization scope rather than assigning one verification status to the corpus.

| Result | Why it does not currently justify an implementation |
| --- | --- |
| [Exact DFT below n log n, September 2026](https://github.com/openai/math/blob/main/lean/docs/130.md) | The uniform claimed bound is O(n(log n)^(1-10^-13)) with exact complex arithmetic and unrestricted coefficients. No demonstrated stable floating-point 8-point DCT follows. The subsequential scope also explicitly excludes conditioning and bit-complexity claims. |
| [Matrix exponent at most 9/4, 2026](https://github.com/openai/math/blob/main/lean/docs/107.md) | An asymptotic arithmetic result without a practical crossover guarantee; our kernels are small constant-coefficient transforms. |
| [AlphaTensor, 2022](https://deepmind.google/blog/discovering-novel-algorithms-with-alphatensor/) | Its 47-multiplication 4x4 headline is over F2. Transfer the search method, not the arithmetic circuit, to floating-point DCT work. |
| [OpenAI's ten advances, August 2026](https://openai.com/index/ten-advances-in-mathematics/) | Packing, error-correcting code bounds, hardness and structural results do not directly shorten JPEG Huffman streams or accelerate these quantizers. |
| [Directed shortest paths, 2025](https://arxiv.org/abs/2504.17033) and [2026](https://arxiv.org/abs/2602.07868) | No corresponding large weighted-graph workload exists here. A coefficient trellis is a small layered DAG suited to ordinary dynamic programming. |
| [Multiplierless approximate DCTs, 2022](https://arxiv.org/abs/2206.00122) | Fewer mathematical multiplies need not beat current SIMD; actual JPEG decoders retain their fixed inverse transform. Treat as an optional rate-distortion experiment. |

A scaled AAN DCT is a useful older experimental control, not a breakthrough
within this window. Likewise, arXiv `2207.11638` was uploaded in 2022 but
originally published in 2017. ANS, learned entropy streams and neural codec
replacements require format/decoder changes and are outside this JPEG scope.

## Validation and delivery

- Preserve scalar/vector numerical ordering, threshold comparisons, rounding,
  errors, cancellation and output-prefix behavior. Do not accept fewer
  operations as proof of faster encoding.
- Run local Release builds and focused regressions for the changed kernels,
  workspace lifetime, encoder parity and portable fallbacks. Full suites and
  every benchmark run only on CI, as required by `AGENTS.md`.
- Finish the batch locally, make self-contained logical commits, and push its
  settled revision once to `main`. Reuse that automatic CI run; do not dispatch
  another run for the same revision.
- In that planned run collect current/native timings and a serial same-host
  comparison against the implementation baseline, using identical input cases,
  CPU affinity, runtime, policy and JPEG hashes. Include allocated bytes per
  encode. Record noninterleaved revision ordering as a limitation.
- Repeat the baseline/current selections serially with managed AVX-512 disabled
  so the narrow zigzag path is measured even on an AVX-512-capable runner. Keep
  this diagnostic separate from normal hardware-dispatch acceptance.
- Include a photograph, low-noise and low-contrast controls, tiny odd edges,
  both distances, sampling modes and baseline/progressive output. Keep fixed
  native acceptance bounds; distinguish baseline improvement from native parity.
- Report regressions and invalid/noisy measurements explicitly. No universal
  speed or quality claim follows from selected-case success.
- Future measurement work: compare revisions in interleaved blocks and detect
  timing-state transitions. A tight majority cluster can pass the MAD check
  despite slow/fast regimes in the remaining blocks; native drift alone cannot
  reliably correct such data.

## Runtime selection follow-up

The original and optimized fuzzy-erosion and narrow-zigzag kernels are retained
side by side. Internal immutable options carry each encode's preferences into
its AQ instance and prepared quantizers. Explicit preferences support tests and
CI; ordinary callers use Automatic. There is no process-global mutable selector,
per-image race, startup benchmark or timing-dependent JPEG output.

The isolated comparisons measured one kernel at a time with the other set to
Automatic. The final per-encode policy uses tiled narrow ordering only on x64
with accelerated 256-bit SIMD, eight float lanes, disabled 512-bit SIMD, source
dimensions at least eight and at least 32,768 source pixels. Small images,
software, four-lane paths, .NET Standard and other architectures keep Original.
The 32K-pixel floor is a provisional minimum-work deployment guard, not a
measured crossover: sampled areas were 323, 49,601 and 393,216 pixels.
Intermediate sizes and unusually narrow/tall images need further measurements.

The choice is captured once in each prepared quantizer, not recomputed for every
block. Wide-SIMD and shared DC-only paths remain available. Explicit narrow
preferences reach the 128-bit implementation even on an AVX-512 host; default
dispatch retains the wide path. AQ Automatic keeps insertion on every platform:
its results do not justify a portable guard for the selection network.

The `[compare-kernels]` push runs one interleaved comparison job per
framework and architecture: .NET 10/11 on x64 and ARM64. Six balanced ABBA/BAAB
quartets compare identical encodes in the same process and on one CPU, using
20 iterations per leg (100 for inputs below 1,024 pixels) and five warmups.
Both variants and Automatic must produce identical JPEG bytes. x64 uses actual
256-bit acceleration with AVX-512 disabled; four-lane ARM64 records narrow
ordering as unreachable instead of claiming to measure it.

Raw leg timings, allocations, collections and quartet ratios are retained.
Guards check minimum timed work, MAD, repeated-leg drift, minority timing states
and order bias. Small or inconsistent differences stay inconclusive. Rules keep
the original fallback and do not use image IDs, distance or progression lookups.

[Run 41581024412](https://github.com/symbiogenesis/DotJpegli/actions/runs/41581024412)
measured the alternatives in four architecture/framework lanes. All JPEG hashes
match. All 42 valid non-tiny narrow-ordering comparisons favor Tiled by case
median; 17 satisfy the stronger every-quartet preference criterion. Tiny results
are inconclusive and contain every valid narrow-ordering regression. AQ is
inconclusive on ARM64 and x64/.NET 10; x64/.NET 11 has three strong individual
wins but no defensible hardware/input partition. Each complete timing summary
is invalid because some rows fail stability guards; those rows are retained and
excluded from dispatch justification. This is a provisional deployment policy,
not a guarantee that every selected encode is fastest.

Focused tests cover both kernels exhaustively, per-instance preference capture,
forced prepared-table output, all nine kernel combinations in complete encodes,
concurrent encodes, padding and software fallback. Boundary cases cover images
below, at and above the 32K-pixel guard. The next automatic comparison also uses
`--include-automatic`: both Original versus the actual Automatic policy, so the
combined decision is measured as well as isolated forced kernels.

[Retained selection evidence](docs/performance-evidence/2026-11-02-kernel-selection-9af9a87/README.md)
records each lane's raw samples, invalid rows, preference decisions and limits.

## First-batch implementation and validation

### Implemented

- `d577986`: selection network reduced from 45 to 39 min/max operations, with
  nominal output dependency depth reduced from 10 to 7. This prunes Bert
  Dobbelaere's published nine-input topology; it is not a claim of a newly
  discovered optimum. Scalar and vector implementations retain weighted-sum
  ordering. The packaged third-party notices include the source's MIT notice.
- `63022a8`: adjacent-column tiles reduce narrow ordering from 40 byte shuffles
  and 32 ORs to 26 shuffles and 18 ORs, plus eight half-vector packs. .NET 11 uses
  Zip operations; compatible targets use shuffle/select packing. Wide ordering
  and the DC-only shortcut are unchanged. Counts are not measured speedups.
- `cfb0723`: production AQ scratch uses an encode-owned `ArrayPool<float>` lease
  when its largest request is at most 1 MiB, with ordinary-array fallback above
  that threshold. One lazy strengths row is reused across passes. Tests retain
  an injectable pool to verify dirty rentals, rollback and return ownership.

### Local validation

The pinned SDK `11.0.100-rc.3.26522.148` was temporarily installed under ignored
`.artifacts/tooling`, after checking its SHA512 against official release metadata.
Local Release builds passed without warnings for all three library targets.
Focused regressions passed on Windows ARM64:

- 84 selected kernel/workspace/encoder-parity tests on .NET 10 and on .NET 11.
- 108 parallel/cancellation/destination-failure tests on each runtime.
- 26 changed-kernel/workspace tests on .NET 11 with hardware intrinsics disabled.
- 16 permutation/quantization tests with `DotJpegliNet11Simd=false`.
- 10 selection/workspace tests against the .NET Standard library on .NET 10.

The selection regressions cover all 362,880 distinct permutations and all 512
binary order patterns, ties, positive float extremes and independent vector
lanes. Narrow ordering checks all 1,024 independent input bits and output guards.
Workspace regressions use oversized NaN-poisoned rentals and injected allocation
failures. Existing parallel tests cover independent concurrent encodes,
cancellation and I/O errors. No local benchmark or full suite was run.

### CI measurement

[Automatic run 41578440598](https://github.com/symbiogenesis/DotJpegli/actions/runs/41578440598)
tests `2fa9afe`. All six Windows/Linux full-suite lanes, fallback checks,
deployment and reproducibility checks pass. Normal-dispatch evidence on AMD
EPYC 9V45 / .NET 11 RC3 covers all 32 selected cases, with matching baseline and
current JPEG hashes. Allocated bytes per encode fall in every case, by about
4.1-47.7% relative to `2ef3d5f`.

Latency is mixed. Low-contrast, low-noise and tiny-image groups use roughly
1.1-1.4% less time by geometric mean. The Kodak01 group is about 8.5% slower:
two progressive 4:2:0 cases are 42-47% slower while their native comparisons
also drift 27-33%. This noninterleaved diagnostic does not establish a uniform
speed improvement or isolate which change affects an individual case.
Existing managed/native latency bounds still fail.

The run's intended narrow-SIMD pair actually retained AVX-512. The guard rejected
that report correctly: `DOTNET_EnableAVX512F` is an obsolete switch name for
this runtime. The correction uses .NET 11's documented `DOTNET_EnableAVX512=0`
and still requires actual accelerated 256-bit SIMD, disabled 512-bit SIMD, and
eight float lanes in both evidence sets before a narrow-path claim. The corrected
automatic `[compare-baseline]` push run supplies that validation; no manual
duplicate workflow is dispatched.

That correction is validated in
[automatic run 41579107572](https://github.com/symbiogenesis/DotJpegli/actions/runs/41579107572)
at `67d5724`. Actual vector capabilities confirm accelerated 256-bit SIMD,
disabled 512-bit SIMD and eight float lanes for both compared revisions.
All 32 narrow-path cases have valid measurements and identical JPEG hashes.
On AMD EPYC 7763 / .NET 11 RC3, one worker, affinity 0 and tiered compilation
disabled, the managed latency geometric-mean ratio is **0.99003** (about 1% less
time), and native drift is **0.99977**. Twenty-four cases are faster; eight are
slower, with the largest slowdown about 3.64%. Allocated bytes fall by
**4.07-47.71%**. This measures the combined batch, not isolated kernel gains.

The first run's second pair, which also had AVX-512 enabled, is useful repeat
evidence for normal dispatch. Its two Kodak01 anomalies do not recur: managed
ratios are 0.99594 and 0.99922. Across its 32 cases the geometric-mean managed
ratio is 0.98664, with native drift 1.00030. Raw blocks in the original pair
show mixed slow/fast timing states without corresponding managed GC, so the
initial large slowdowns do not establish a reproducible code regression.

The corrected run's normal-dispatch report is deliberately rejected because
Kodak01/444/p0/d1 exceeds the fixed 10% managed relative-MAD limit. All full test,
fallback, deployment, reproducibility and package checks pass; the overall run
still fails because of that noisy normal measurement and the existing native
latency bounds. No thresholds were relaxed and no additional retry was launched.

[Retained first-batch evidence](docs/performance-evidence/2026-11-02-first-batch-67d5724/README.md)
includes the verified narrow raw samples and the earlier normal/repeat comparison
reports. The first batch makes no compression-size change: its compared JPEG
outputs are byte-identical.

The `performance-baseline-2fa9afe53325be785bd638c7c4448598d308cb9f` artifact
retains raw baseline evidence, both attempted narrow evidence sets and the normal
comparison report. The normal current evidence is in the corresponding
`performance-targeted` artifact. Allocation totals exclude retained pool capacity
and do not establish lower peak working memory.

## Shared token-budget implementation and validation

`418c556` replaces equal per-scan shares with one encode-owned 64 MiB budget.
Segment requests retain the existing geometric growth and use smaller final
rentals when necessary. Accounting uses actual accepted array lengths, including
oversized rentals. Exhaustion abandons and releases only the affected scan;
its capacity becomes available to later scans while siblings remain replayable.
Allocation follows scan order, so early scans have priority. The shared budget
adds one small object allocation per token-cache owner.

`a114953` releases each successful scan's rentals immediately after replay and
adds whole-encoder comparisons against uncached emission. Focused regressions
cover dirty and oversized rentals, rent failures, partial raw writes, repeated
disposal, sibling and independent-owner lifetimes, and cached/fallback byte parity
through sequential component scans, progressive refinement and restart markers.
They also cover concurrent encodes, cancellation and destination-failure prefixes.
Public encoder settings and the managed-only runtime contract are unchanged.

Local Release library and test builds pass with zero warnings for all three
library targets. Touched-file `dotnet format` checks pass at severity info.
On Windows ARM64, 74 focused token, replay and encoder tests pass on each of
.NET 10 and .NET 11. The 20 new tests also pass on .NET 11 with hardware intrinsics
disabled and against the .NET Standard library on .NET 10. No local benchmark or
full test suite was run.

The settled revision's automatic push run will supply full CI validation and
normal selected-case performance measurements; those results are pending.
Large progressive workloads still need measurements before claiming a latency
or peak-working-memory benefit. Tests establish bounded rental ownership and
byte parity, not such a benefit. The other pending candidates remain open.
