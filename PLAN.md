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

These tasks are in progress. Implementation and validation results will be
recorded below before the batch is published. Larger architectural changes and
quality-changing experiments remain pending.

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

### 8. Dynamic token budget across scans — pending

Location: `EntropyTokenScans`. The total 64 MiB budget is divided equally by
scan count. A small DC scan may leave capacity unused while a large AC scan
abandons caching. Consider shared, demand-driven accounting without increasing
the cap. Preserve rental accounting and complete-sequence fallback semantics.
This is an engineering observation, not a literature-derived breakthrough.

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
- Include a photograph, low-noise and low-contrast controls, tiny odd edges,
  both distances, sampling modes and baseline/progressive output. Keep fixed
  native acceptance bounds; distinguish baseline improvement from native parity.
- Report regressions and invalid/noisy measurements explicitly. No universal
  speed or quality claim follows from selected-case success.

## First-batch results

Pending implementation, focused validation and the single planned CI run.
