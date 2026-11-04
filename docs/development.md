# Development and validation

[Documentation](README.md)

## Upstream implementation

DotJpegli adapts the encoder at Google Jpegli revision
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20).
The [upstream manifest](upstream.json) records provenance and managed extensions;
[third-party notices](../THIRD-PARTY-NOTICES.md) map managed files to upstream routines.
The library has no native binaries, P/Invoke, pointer code, `Unsafe` APIs or runtime
NuGet dependencies. Decoder dependencies are validation-only.

The managed DCT uses standard JPEG coefficient units; upstream's transform emits
one eighth of those units, accounted for during quantization. Portable
`System.Numerics.Vector<float>` paths in the .NET 10 and .NET 11 targets accelerate the DCT, RGB-to-YCbCr conversion,
unsmoothed integer-ratio downsampling and AC quantization. DCT dispatch requires
runtime hardware acceleration and four or eight float lanes; unsupported cases
use scalar fallbacks. Paths preserve scalar evaluation order,
fused multiply-add rounding, sample normalization, averaging order and
quantization thresholds and rounding.

The offline AC reciprocal study prepares adjacent binary32 reciprocal endpoints
using an exact binary64 product with each integer divisor. Scalar and portable
SIMD prototypes accept only matching suppression and ties-to-even results at
both endpoints, with original division for ambiguity. The
[retained proof and measurements](performance-evidence/2026-11-03-ac-reciprocal/README.md)
reject this prototype for deployment: its guard costs more than division on the
measured host even when fallback is absent. Production quantization is unchanged;
the research kernels live only in the performance tool and focused regressions.

The [bounded AQ reciprocal investigation](performance-evidence/2026-11-03-aq-reciprocal/README.md)
certifies the existing rational expressions' binary32 quotients, including those
inside `FastLog2` and `FastPow2`. A 256-entry immutable binary64 reciprocal-seed
table and two Newton steps propose a quotient. Exact binary64 midpoint products
certify strict membership in its binary32 rounding cell; ambiguous, exceptional
and unsupported cases use the original division. Polynomial coefficients,
intermediate rounding, clamps, exponent reconstruction and reduction order stay
unchanged. This certifies the existing approximations, rather than substituting
mathematical logarithm or exponentiation functions.

Only the internal per-encode `AqRationalKernel.Optimized` diagnostic choice uses
this prototype. Automatic and Original retain division and do not access the
reciprocal table. An optional exclusively owned sink
records counts without clocks or global selection state. Research vector paths
certify lanes individually, using the original vector division for fallback
lanes whenever any lane is ambiguous. Their scalarized certification and bounded
stack scratch are confined to the explicit research branch. Counts retain both
ambiguous lanes and affected vector batches. Untimed analysis and focused
regressions establish numerical parity and fallback distributions; latency and
memory effects are unmeasured.

Eight-lane SIMD batches adaptive modulation across neighboring blocks while
preserving each block's addition order. Complete converted-plane blocks combine
DCT, prepared quantization, AC validation and zigzag ordering without a final
float transpose or intermediate coefficient buffer. Four-lane SIMD also
quantizes the two transposed DCT panels directly, staging only 64 shorts before
zigzag ordering. Invalid coefficients use the original separate operations
before writing caller output. The .NET 11 target uses portable `Zip`/`Unzip`/`Concat`
for DCT transposes and adjacent-pair downsampling; .NET 10 uses portable lane
assembly. No CPU-specific intrinsics are used. Smoothing and nonlinear
RGB-to-XYB conversion use scalar paths. These implementation choices do not
establish speed parity; see the [performance measurements and protocol](performance-parity.md).

Scalar AQ modulation computes high-frequency and gamma sums in one traversal
of each 8-by-8 block, carrying each horizontal neighbor into the next column.
The final column is handled separately without a horizontal difference. The
accumulators keep their original raster order, and
high-frequency modulation is applied before gamma modulation. An independent
two-pass regression checks result bits across strided blocks, low-contrast
inputs, impulses and exceptional values, including minimally sized slices that
end at the final sample. Reusing sample loads is not evidence
of an end-to-end latency improvement.

Eight-block SIMD AQ modulation carries each horizontal neighbor into the next
column, then handles the final column without a horizontal difference. Each
lane retains its original horizontal/vertical interleaving and gamma raster
order, including the last row's vertical subtraction. The schedule adds no
scratch storage; focused scalar/vector bit-parity and complete-encode checks
cover the change. Latency remains a CI measurement question.

ICC destination transforms retain the exact lower and upper curve endpoints
computed during inverse validation, avoiding repeated endpoint evaluation for
every converted pixel. The values belong to each prepared transform; shared
identity curves remain immutable. Validation still evaluates the upper endpoint
before the lower endpoint, and inverse interpolation and parametric arithmetic
are unchanged. Focused regressions check output bits at clipping boundaries and
sampled-curve plateaus. Source-ICC workload measurements are still needed to
establish the latency effect.

Parametric ICC curves also retain the exact reciprocal gamma and, for types 1
and 2, the power-branch threshold computed from their stored coefficients.
These immutable values replace the same divisions in per-pixel evaluation;
power calls, clipping, validation and all other arithmetic retain their order.
Independent bit regressions cover nonintegral gamma across all five types,
clipped endpoints and adjacent floats at a nonrepresentable branch threshold.
Source-ICC latency remains unmeasured.

The scalar DCT specializes the same fixed eight- and four-point stages as the
vector path. It keeps the recursive port's arithmetic and fused multiply-adds
while avoiding recursive calls and per-stage scratch spans. A retained recursive
test oracle checks coefficient bits, including signed zero, subnormals and
exceptional values; complete-encode fixtures check JPEG output. This scheduling
change still needs CI measurements before claiming a latency benefit.

The eight-row transpose uses the released four-panel schedule for DCT and
eight-block AQ modulation. The replacement using full-width `Vector256.Zip`
added cross-half instructions on x64, so .NET 11 again uses portable
`Vector128.Zip` inside each panel. The transform only permutes bits, including
signed zeros and NaN payloads; independent single-lane regressions also check
untouched tails. Complete-encode timing remains a CI measurement.

Two opt-in library build properties retain alternative schedules for isolated
CI comparisons: `DotJpegliFullWidthTranspose=true` enables full-width Zip on
.NET 11 when `DotJpegliNet11Simd` is enabled, and
`DotJpegliSparseTransposedQuantization=true` enables zero-row, conservative
suppression and fully-suppressed-row shortcuts in the fused SIMD quantizer.
Both default to `false`; compiling each alternative separately keeps unused
experimental dispatch out of ordinary encodes. Existing transpose and fused
quantization tests check the same exact outputs in either build. Runtime,
vector capabilities, image geometry and quantization settings can inform a
future deterministic selection policy only after same-host complete-encode
measurements establish the relevant tradeoffs.

The release repair retains scenario-specific choices throughout the encoder.
Selection uses encoding settings and available capabilities, never image IDs
or timing probes during encoding.

| Operation | Deterministic choice |
| --- | --- |
| Entropy replay | Six-context lookups use direct indexes; partial lookups subtract their validated context offset. Select the specialization once per token batch. |
| Rate-distortion search | Enter the isolated search helper only when its explicit lambda is positive. Ordinary coefficient preparation retains its compact path. |
| AQ arithmetic | Select Original or certified reciprocal arithmetic once per MCU row from the immutable per-encode preference. Automatic retains Original. |
| Sequential coefficient storage | Retain the existing bounded cache on accelerated 256-bit hosts without accelerated 512-bit vectors; other eligible sequential cases stream. Existing admission limits and fallbacks still apply. |
| Sparse quantization and full-width transpose | Retain both schedules through the build properties above. Automatic gains a scenario rule only after CI establishes which settings favor each alternative. |

AQ copies the preceding pre-erosion row when edge clamping selects the same
four-pixel stencil again. This reuses the rounded result without new scratch
storage and retains cancellation checks for every requested row. Retained
strength-bit fixtures cover one-row and odd-edge images, sampling factors one
through four, scalar/vector paths and immutable converted-plane reuse. The
avoided stencil work does not establish an end-to-end speed improvement.

Pre-erosion also reduces each completed four-pixel group directly into its output.
The vector path reads four lanes in their original order; the scalar path keeps
four column sums. Each column retains its four-row addition order, followed by
the original left-to-right horizontal sum and multiplication. This eliminates
the padded-width `difference` array, its full-row stores and subsequent reads:
three scratch arrays are prepared, with a fourth rented lazily for strengths.
The removed request was four bytes times the padded source width; its pooled
capacity could be larger. An independent original staged-stencil oracle compares
intermediate result bits across boundaries, sampling factors and neighborhood
reuse. Pool regressions cover dirty tails, failed rentals and cleanup. This
storage reduction does not establish latency or peak-working-memory savings.

Fuzzy erosion retains both the four-value insertion algorithm (45 min/max
operations) and the pruned seven-layer network (39 operations). Narrow
transposed zigzag ordering retains the original shuffle network and adjacent-
column 2x4 tiles; .NET 11 packs tiles with portable 64-bit Zip operations and
.NET 10 uses compatible shuffle/select operations. Internal immutable settings
let CI compare one kernel at a time without changing concurrent encodes.
Automatic uses tiled ordering for x64 eight-lane, accelerated 256-bit execution
without 512-bit acceleration when both source dimensions are at least eight and
the image has at least 32,768 pixels. The
[threshold, tall-image and intermediate-size assessment](performance-evidence/2026-11-03-kernel-guard-692a052/README.md)
supports retaining this conservative deployment floor on the measured x64
hosts, without establishing a crossover. Other configurations retain original
narrow ordering; default wide-SIMD dispatch remains available. AQ keeps the
original insertion kernel because no portable selection guard was established. Both
algorithms preserve the existing arithmetic; no runtime benchmarking is used.

The [bounded compact coefficient-cache study](performance-evidence/2026-11-03-compact-coefficients/README.md)
compares immutable offline layouts in 32-block tiles. Bitmap tiles retain direct
DC shorts, 63-bit AC masks, per-block packed-value offsets and nonzero shorts;
individual AC lookup uses a mask test and population-count rank. Bit-packed tiles
retain direct DC shorts and per-band widths and bit offsets for folded signed
AC values. Both layouts support exact full-block expansion and individual
coefficient access, with optional dense tiles when compact payloads would grow.
Accepted payload and tile-index rentals share a fixed 64 MiB cap, charged at
their actual pooled lengths; dense fallback still carries the compact index.

The untimed analysis checks 24 actual AQ image configurations across distances
1/4 and 444/420, plus adversarial signed coefficients and partial tiles. It
retains useful bytes, actual rental capacity and explicitly defined packing,
read, indexing, rank and bit-operation counters. Capacity trials repeat all
source components in their original proportions around the then-current summed
rental boundary, preserving one shared cap across components. These replicas
establish storage admissions only. Counters compare the executable offline
codecs: production dense storage receives DCT output directly and returns block
spans without the prototype's packing and expansion passes. The study did not
deploy either compact representation or measure latency, allocations or peak
memory. Current production still stores dense coefficients, using bounded
segments and a complete prefix of MCU rows when whole-component rentals exceed
the available capacity; this later policy is separate from the historical study.

Optimized Huffman encoding shares one 64 MiB entropy-token rental budget across
all scans in an encode, additionally constrained by the shared `MaxCacheBytes`
capacity across converted samples, coefficients and tokens. Scans acquire
storage in script order, accounting for
actual pooled array lengths, so small DC scans can leave capacity for larger AC
scans. Exhaustion abandons only the affected scan, returns its rentals and lets
later scans use the refunded capacity. Emission replays complete sequences and
recomputes abandoned ones; successful replay releases that scan's storage early.
Independent encodes own independent budgets. This policy preserves JPEG bytes
but can change which scans are cached; it does not establish lower latency or
peak working memory for every image.

With `MaxDegreeOfParallelism` greater than one, complete cached-token scans can
replay independent segments at caller-requested restart boundaries in parallel.
The histogram pass still builds global Huffman tables before emission, and
progressive EOB runs and correction bits stop at each restart. Workers use
private buffered bit writers, preserving the serial padding and byte stuffing.
The caller concatenates their output and emits the original restart markers.
Workers record each bit-writer drain, so destination writes retain their original
sizes, order and calling thread, including destination-failure prefixes.

A batch has at most 32 segment slots and 4 MiB of accepted pooled staging,
accounting actual rental lengths, in addition to at most 32 private 4 KiB writer
buffers, at most 12 KiB of additional lookup staging and fixed batch metadata.
Oversized rentals are immediately returned; oversized segments use serial
replay. The range cursor crosses token rentals
without allocating an image-sized restart index. Uncached scans retain serial
replay; complete scans without restarts use the bit-chunk path below. All workers
finish before emission or cleanup, and rentals are returned on success,
cancellation and failure. This resolves parallel coding at existing restart
boundaries; it does not establish
a latency or peak-memory improvement.

Complete token caches without restart markers also support parallel entropy
replay. The ordered histogram/token pass already resolves DC predictors,
progressive EOB runs, correction-bit order and the final pending EOB run. Workers
therefore need only the scan's immutable global Huffman tables and explicit
code/suffix or raw-bit records; neither baseline nor progressive replay needs
new predictor or run state at a chunk boundary.

Scans with at least 16,384 records split into 8,192-token chunks, with at most
32 chunks per batch and a shared 4 MiB cap on accepted actual rental lengths.
Each chunk requests at most 32 KiB for unstuffed 32-bit words, retaining its
exact final partial word separately. One caller-owned buffered bit writer merges
those words and tails without padding at chunk or batch boundaries. Its global
64-bit discharge sees the same words as serial replay, so byte stuffing,
destination write sizes and destination-failure prefixes remain identical.
Only the original scan boundary receives padding. The fixed chunk threshold
bounds scheduling overhead; it is not a measured performance crossover.

Missing Huffman codes and noncanonical suffix widths discard the entire staged
chunk and replay its original records through the same caller writer. This
retains strict validation and the original error prefix even when earlier
chunks left pending bits. Oversized rentals also use serial chunk replay.
Workers finish before merging or cleanup; all accepted rentals return on
success, cancellation, destination failure and rental failure. Staging adds
one 4 KiB merge-writer buffer, at most 12 KiB of lookup staging and fixed batch
metadata beyond the accepted rental cap. Tiny scans, single-worker execution
and uncached scans retain serial replay. Focused regressions cover every
64-bit seam alignment, all-FF stuffing, token rental boundaries, strict fallback,
exact destination writes, partial failures, cancellation and full baseline,
progressive and custom-refinement JPEG parity. Latency, allocations and peak
memory remain CI measurement questions.

Single-context entropy-token replay reads the table's immutable prepared lookup
directly, avoiding a stack clear and copy for that scan. Bulk and strict replay
use the same context-relative index; raw bits and restart markers retain their
absolute contexts. Multiple contexts retain contiguous stack staging. Focused
regressions compare distinct tables across all six contexts, generic suffix
fallbacks, raw bits, restart markers and missing-symbol output prefixes. This
removes stack staging, not table allocation; the extra index subtraction and
avoided copying still require CI latency measurements.

Progressive refinement fills each cached raw correction token with up to 24 bits,
matching the token payload width. Both reserved and ordinary token construction
use the same packing; direct bit-writer emission retains its existing grouping.
This reduces correction-token records without adding scratch storage or changing
the shared rental cap. Actual rented capacity depends on pool bucket sizes;
fewer records alone do not establish lower allocations or peak working memory.

Buffered entropy output checks capacity once for a stuffed eight-byte word when
at least sixteen bytes remain. It writes directly into the existing buffer with
a local index. Near the boundary it retains the per-byte reservation and drain
rules, including complete stuffing pairs and destination-failure prefixes.
Focused tests cover every stuffing mask, one through seventeen bytes of free
space, exact drains and partial destination failures. No staging buffer is added;
the reduced checks still need CI latency measurements.

An internal immutable per-encode stuffing preference also permits a portable
128-bit SIMD expansion within that same sixteen-byte reservation. Equality
extracts an eight-bit `FF` mask; one of 256 shuffle masks emits payload bytes
and inserted zeros. Only the returned prefix is committed, although the store
can touch all sixteen reserved bytes. The immutable table has 4 KiB of payload
plus its array header. No per-word scratch or larger output batch is introduced.
Every serial and parallel replay writer captures the preference, including
restart overflow fallback and unaligned merging. .NET Standard and software
execution retain scalar expansion. Original no-`FF` stores and boundary drains
remain shared by both choices.

The [bounded stuffing experiment](performance-evidence/2026-11-03-byte-stuffing/README.md)
compares helper expansion, actual token replay and eight complete JPEG controls,
retaining invalid timing rows. One valid helper case favors SIMD, but the
complete-encode results do not justify a deployment guard. Automatic therefore
keeps Original; the explicit choice exists for diagnostics, with no public API
change or startup measurement.

The optional [offline progressive scan search](../tools/DotJpegli.Parity/README.md#bounded-progressive-scan-search)
uses explicit scan scripts and exact complete-JPEG sizes to choose among a bounded
family of spectral splits, refinement depths, component orders and Huffman table
assignments. An internal per-encode table limit can constrain histogram clustering
to one, two or four tables per class. Zero retains the existing clustering and
slot-redefinition behavior; the normal public options and bytes are unchanged.
Limits are experimental controls for the offline tool, not new public API.
Focused tests reconstruct every real quantized coefficient using an independent
JPEG entropy reader and compare decoded pixels for every candidate at both
distances and sampling modes. Synthetic marker fixtures check reported DHT costs
and selectors. All candidate encodes and logical histogram/emission/replay scan
passes are counted, including discarded candidates. These counts exclude
coefficient preparation and do not establish CPU time or peak working memory.

The optional [offline quantization-table search](../tools/DotJpegli.Parity/README.md#bounded-quantization-table-search)
caches independent per-band coefficient costs in one bounded DCT traversal and
screens 729 joint Y/chroma table configurations. DC is fixed; three AC bands
vary independently for Y and paired Cb/Cr, retaining distinct chroma base tables.
The surrogate omits AQ and coupled entropy costs. At most 17 finalists use
complete default-policy baseline AQ/Huffman encodes and a common pinned native
decoder. Only outputs meeting the default control's RGB PSNR and local luma SSIM
scores and the offline multiscale structural score can replace it, and actual
complete JPEG size decides the winner.
Reports retain all costs, tables, exact trials and hashes. Focused tests check
cached costs against an independent cosine-DCT oracle and reproduce every exact
finalist with an independent managed decoder. This tool adds no public API or
runtime dependency; corpus measurements and all benchmarks remain CI-only.

The optional [offline rate-distortion quantization search](../tools/DotJpegli.Parity/README.md#bounded-rate-distortion-quantization-search)
starts with the original AQ coefficients and changes only AC values through an
internal baseline-only control. A bounded dynamic program considers each
original coefficient, one step toward zero and zero; its objective combines
squared DCT reconstruction error with standard-table run/category, amplitude,
ZRL and EOB bit costs. Complete candidate encodes rebuild optimized Huffman
tables from the searched coefficients. Histogram collection and emission share
the same bounded coefficient cache, including token-cache fallback. The
experiment rejects images exceeding the unchanged 64 MiB coefficient-cache
limit before writing output. The default fused path and public API are unchanged.

The offline grid contains five distances and five weights, including five
unchanged controls. Every one of the 25 complete JPEGs is decoded and scored
with RGB PSNR and local luma SSIM. Reports retain per-metric nondominated curves,
matched-size and matched-quality interpolation without extrapolation, and
unsupported ranges. The distance-1 control sets all three measured quality floors
for choosing a winning complete JPEG. Search counts include every real block,
candidate/state transition and changed AC coefficient, as well as all complete
encodes, decodes and logical histogram/emission passes. These counts do not
measure CPU time, allocations or peak working memory. Exhaustive small-block
oracles, independent entropy histograms, coefficient parsing, decoder checks and
failure-path fixtures validate the experiment; corpus measurements remain CI-only.

Both offline quantization searches use a single immutable
[prepared quality reference](../tools/DotJpegli.Parity/README.md#reusable-offline-quality-objectives)
to reuse source RGB, exact luma and clipped-window moments across decoded trials.
They also prepare source pyramids and Gaussian moments for the explicitly
specified five-scale `dotjpegli-ms-ssim-v1` objective. All three control scores
are eligibility floors. Both independent reference scorers validate the control
and each provisional winner before acceptance. Schema 2 reports record the
metric definition, all scores and feature/comparison/validation counts without
extra encodes or decodes. Focused regressions check exact score bits, analytic
constants, snapshot ownership and concurrent reuse; complete-search fixtures
reproduce all scores, eligibility and selected bytes. Additional image-sized
offline reference storage and reduced arithmetic do not establish a latency,
allocation or peak-memory benefit. Encoder paths and the fixed native acceptance
runner are unchanged.

## Build

The library targets `netstandard2.1`, `net10.0` and `net11.0`; tests and tools run
on `net10.0` and `net11.0`. The .NET Standard 2.1 assembly preserves the public
encoding and metadata APIs without runtime NuGet dependencies, using compatibility
implementations for APIs introduced in newer runtimes. The .NET 11 build uses newer
portable SIMD APIs, while the .NET 10 build retains compatible implementations.
The .NET Standard target uses scalar DCT and quantization paths and preserves
single-round fused multiply-add behavior in software.
`global.json` pins .NET SDK `11.0.100-rc.3.26522.148` with
roll-forward disabled and `allowPrerelease: true`. Install that exact SDK;
CI reads the same file. Running the .NET 10 test and tool targets also requires
the .NET 10 runtime. Update the SDK pin deliberately to receive compiler,
runtime and analyzer improvements, then validate all three library targets.
All projects use C# 15 explicitly, including when compiling older framework
targets, so the pinned compiler provides a consistent language version.
Internal unions model scan modes, metadata edit intent and snapshot sample storage.
The .NET 11 target uses the framework's union contracts; older library targets
declare internal `UnionAttribute` and `IUnion` compatibility contracts. Keep union
cases as reference types where allocations or boxing matter, and handle default
null contents explicitly. Span-based pixel readers and packed entropy tokens
retain their existing storage representations.

All projects explicitly enable the SDK's .NET analyzers, including the
`netstandard2.1` library, and use `AnalysisLevel=latest-recommended`.
`TreatWarningsAsErrors` makes analyzer findings build failures; code-style
analysis also runs during builds via `EnforceCodeStyleInBuild`. The pinned SDK
keeps the rule set consistent locally and on CI. Fix findings or justify
exceptions at the smallest scope: format-mandated ICC/XMP MD5, standard Exif
type names, compatibility adapters and validation helpers that report public
argument or option names have local suppressions. Only test sources exempt
`CA1861`, so expected vectors can stay beside their assertions and each test
can own fresh input arrays. Production code still enforces that allocation rule.

`CA5390` (hard-coded encryption keys) and `CA5403` (hard-coded certificates) are
explicitly disabled. These two opt-in rules have
[known performance defects](https://github.com/dotnet/roslyn-analyzers/issues/6737),
including [quadratic work across helper call sites](https://github.com/dotnet/sdk/issues/56280).
They consumed 96.34 seconds of a 97.21-second Release rebuild of
`DotJpegli.Automation` on the local Windows ARM64 host with the pinned SDK.
The matched rebuild with only those two rules disabled took 0.89 seconds;
total reported analyzer time fell from 96.730 to 0.297 seconds. Both builds used
`--no-restore --no-incremental -p:ReportAnalyzer=true -v:diag`, so the comparison
includes compilation rather than an incremental no-op. This is build timing,
not an encoder benchmark or an end-to-end CI speedup measurement.

The cost repeats in CI's formatting passes and in jobs that compile automation
for native provenance, release checks and synthetic fixtures. Before this change,
[CI run 41592528894](https://github.com/symbiogenesis/DotJpegli/actions/runs/41592528894)
spent 415 seconds verifying EditorConfig rules, 109 seconds verifying the cached
native reference and 195 seconds verifying package contents/provenance; its Linux
.NET 11 full test suite took 14 seconds. The fix removes the pathological rule
execution at its source without adding jobs, sharding tests, caching compiled
outputs or changing workflow coverage. End-to-end timings still need a CI run.

The two hard-coded-key/certificate checks will no longer run if those APIs are
introduced later. Current library, test and tool sources use neither API; the
remaining security rules, banned API analyzer, trimming/AOT analysis and strict
warning policy stay enabled. Reassess the two rules when updating the pinned SDK.
Use EditorConfig severity `none`, rather than `NoWarn`, because suppressing the
diagnostic alone still incurs the expensive analysis.

Prefer framework span APIs for buffer searches, validation and counting, and
span splitting when substrings do not need to be retained. Use collection
expressions, records and primary constructors where they simplify the code.
Keep numerical operation ordering and caller-owned buffer snapshots intact:
modernization must preserve JPEG bytes, validation and the managed-only contract.
Modern targets use bulk `BinaryPrimitives.ReverseEndianness` for Exif numeric
arrays and span range/value searches for Huffman histogram validation. The
.NET Standard target keeps compatible scalar implementations of these operations.

The library project and its source are in `src/DotJpegli/`; tests are in
`tests/DotJpegli.Tests/`. Build the library from the repository root with:

```text
dotnet build src/DotJpegli/DotJpegli.csproj -c Release
```

Use `-f netstandard2.1` to build only the portable library target. The pinned
compiler is still required when building that target from source.

Repository automation requires PowerShell 7.6 LTS or newer (`pwsh`) and .NET.
Scripts declare their minimum version, use strict mode 3.0 and stop on errors.
Scripts invoking native tools enable `$PSNativeCommandUseErrorActionPreference`
or check exit codes explicitly, so a failing Git, CMake or .NET command stops the operation immediately.
Strict mode uses a fixed version so a future PowerShell update cannot silently
change script behavior. Filesystem inputs use literal paths, JSON evidence keeps
timestamp strings and array shapes, and text artifacts use UTF-8 without a BOM.

The CI PowerShell job runs on Linux and Windows. It checks every repository
PowerShell file and workflow PowerShell command with all default PSScriptAnalyzer
rules, treating every diagnostic severity as a failure. Analysis tools are pinned
to PSScriptAnalyzer 1.25.0 and powershell-yaml 0.4.12; the latter reads workflow
YAML for inline command analysis. Build artifacts and ignored files are excluded.
Install these development-only modules with the PSResourceGet included in
PowerShell, then run the same check locally:

```powershell
Install-PSResource -Name PSScriptAnalyzer -Version 1.25.0 -Scope CurrentUser -TrustRepository
Install-PSResource -Name powershell-yaml -Version 0.4.12 -Scope CurrentUser -TrustRepository
./tools/ci/analyze-powershell.ps1
```

The compiled [`DotJpegli.Automation`](../tools/DotJpegli.Automation/) tool owns
corpus preparation, InferSharp report validation, performance evidence processing,
native provenance checks and release package inspection. Its PowerShell entry
points retain their command-line parameters and handle process orchestration.
The shared launcher builds the tool incrementally in Release with the pinned SDK
and locked dependencies, then runs it using .NET 10 or a newer installed runtime.
Build output is captured so profile selection emits only the selected cases.

The corpus tool uses its lossless PNG decoder and verifies both the downloaded
PNG hashes and canonical PPM hashes against the committed manifest:

```powershell
./tools/parity/prepare-corpus.ps1 -Output /absolute/path/new-corpus
```

Use `-Source /absolute/path/png-directory` to read previously downloaded inputs.
The output directory must not already exist. Check corpus conversion and the
revision comparator locally with synthetic fixtures, without running benchmarks:

```powershell
./tools/parity/prepare-corpus.ps1 -SelfTest
./tools/compare-performance-revisions.ps1 -SelfTest
./tools/ci/performance-evidence.ps1 -SelfTest
./tools/assert-infersharp.ps1 -SelfTest
./tools/verify-release.ps1 -SelfTest
./tools/Invoke-Automation.ps1 -CommandArguments @('native-reference', '--self-test')
```

`tools/ci/performance-evidence.ps1` groups SIMD validation (`-AssertSimd`),
profile selection (`-SelectProfiles`) and runtime reports (`-CompareRuntimes`).
The native reference command has separate fixtures for cached provenance checks.

The CI-only revision comparison uses two persistent processes running identical
measurement-worker binaries against separately built encoder assemblies. The
baseline lane copies the current runner output and replaces only DotJpegli.dll;
the library has no runtime dependencies. Handshakes verify assembly commit
metadata, runner hashes and identical measurement environments before encoding.
Only one worker receives work at a time. Balanced ABBA/BAAB quartets share the
kernel comparison's timing validation; raw samples survive invalid or
inconclusive results. The [runner guide](../tools/DotJpegli.Performance/README.md)
describes timing scope and CI selection. Run its
`--self-test-revision-comparison` locally for synthetic protocol and statistical
checks; timed measurements remain CI-only.

## Banned APIs

All projects reference `Microsoft.CodeAnalysis.BannedApiAnalyzers` as a private
build dependency. A single [`BannedSymbols.txt`](../BannedSymbols.txt) applies the
same policy to the library, tests and tools. It
rejects unsafe/native interop, CPU-specific intrinsics, runtime code generation,
blocking task waits, dangerous serialization, weak cryptography, implicit byte
order/encoding and process-wide mutation. Each entry explains the alternative;
`RS0030` diagnostics fail builds through `TreatWarningsAsErrors`. The analyzer
does not become a runtime or transitive package dependency for consumers.

The encoder has no uses of filesystem, process, console, network, clock or random
APIs. These are not blanket bans: tests and validation tools legitimately use files,
native process comparisons, evidence timestamps and explicitly seeded test data.
Those projects follow the same banned API list without project-wide exclusions.

Portable SIMD, bounded `MemoryMarshal.Cast`/`AsBytes`, integer/floating-point bit
conversions and explicitly seeded test randomness remain available. MD5 is allowed
for the ICC and extended XMP identifiers mandated by those formats; use SHA-256 or
stronger for security-sensitive hashes. Add bans using documentation comment IDs
and a diagnostic explaining the supported alternative. Any necessary exception
should suppress `RS0030` at the smallest possible scope with its format or test
justification, rather than disabling the analyzer for a project.
The test project's `.editorconfig` exempts only xUnit's generated synchronous
entry point, which blocks while bridging to its asynchronous runners.

## Reproducible builds

All projects use [DotNet.ReproducibleBuilds](https://github.com/dotnet/reproducible-builds)
and its Isolated SDK, pinned to 2.0.5. The build package embeds debugging symbols,
publishes repository metadata and enables source-path normalization on CI.
The Isolated SDK limits machine-specific assembly lookup and implicit package
fallbacks. These are build-time tools; the encoder package has no runtime NuGet
dependencies. Source Link support comes from the .NET SDK.

`NuGet.Config` selects nuget.org, committed `packages.lock.json` files fix the
resolved package graph, and `.gitattributes` keeps text checkouts in LF format.
`DeterministicTimestamp` defaults to the ZIP epoch (1980-01-01 UTC), so package
entry timestamps do not depend on checkout or build times. The pinned SDK's
NuGet pack task supports this setting. An explicit override is allowed, but it
must match between builds when comparing package bytes.
Restore the recorded graph before building:

```text
dotnet restore DotJpegli.slnx --locked-mode
dotnet build DotJpegli.slnx -c Release --no-restore
```

When updating a package intentionally, update `Directory.Packages.props`, run
`dotnet restore DotJpegli.slnx --force-evaluate`, and commit the lock-file changes.
The Isolated SDK version is declared separately in `Directory.Build.props`.

With Git and PowerShell 7.6 or newer installed, verify the Release library and unsigned
NuGet package containing all three target-framework assemblies from a committed revision:

```powershell
./tools/verify-release.ps1 -Reproducibility
```

The script builds the same commit in two fresh Git worktrees at different absolute
paths, using the pinned SDK, locked restore, `ContinuousIntegrationBuild=true` and
`RepositoryBranch=refs/heads/main`. It retains `DotJpegli.netstandard2.1.dll`,
`DotJpegli.net10.0.dll`, `DotJpegli.net11.0.dll` and one `.nupkg` from each checkout,
and compares all four complete-file SHA-256 hashes. It removes its worktrees and retains these
artifacts and `hashes.json` in a unique temporary directory. It checks committed
`HEAD`, so commit changes before running
it. CI runs this comparison independently on Windows and Linux in the existing
validation workflow; it does not compare the two operating systems' outputs.
This is a build check and does not run tests or benchmarks.

## Trimming and NativeAOT

The library declares `IsAotCompatible=true` for its `net10.0` and `net11.0` targets. This
also marks its assemblies as trimmable and enables the SDK's trimming,
single-file and AOT compatibility analyzers. Analyzer warnings fail the build.
The SDK-selected ILLink package is a build-time dependency; it adds no runtime
NuGet dependency to the encoder package.

`tests/DotJpegli.AotSmoke` is a dependency-free consumer apart from the library
and build tooling. It roots the entire DotJpegli assembly with
`TrimmerRootAssembly`, so publishing analyzes methods that its runtime checks
do not call. This follows Microsoft's
[library trimming guidance](https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming).
Both linker and native compiler warnings fail publishing, without suppressions.
Its focused runtime checks inspect JPEG framing, dimensions and scan markers,
compare array and stream output, and exercise progressive/parallel encoding,
high-precision pixels, source ICC conversion and Exif/XMP/IPTC roundtrips.

To run the same deployment checks locally, select a supported host RID (for
example, `win-arm64`, `win-x64` or `linux-x64`) and an absolute temporary output
directory:

```text
dotnet publish tests/DotJpegli.AotSmoke/DotJpegli.AotSmoke.csproj -c Release -f net10.0 -r <RID> --self-contained true -p:PublishAot=false -o <TEMP>/trimmed-smoke
<TEMP>/trimmed-smoke/DotJpegli.AotSmoke
dotnet publish tests/DotJpegli.AotSmoke/DotJpegli.AotSmoke.csproj -c Release -f net10.0 -r <RID> --self-contained true -p:PublishAot=true -o <TEMP>/aot-smoke
<TEMP>/aot-smoke/DotJpegli.AotSmoke
```

On Windows, run the published `.exe`. Repeat with `-f net11.0` to check its
separate SIMD implementation. NativeAOT needs the platform's
[native toolchain](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites).
These are focused correctness checks, not benchmarks or the full test suite.
The deployment CI job publishes and runs both modes on Windows and Linux for
both frameworks. It first verifies the library's committed dependency lock,
then lets each publish restore the SDK-selected runtime and compiler packs for
its RID and mode. The smoke project opts out of lock-file generation because
its SDK-only deployment tooling varies by RID and publish mode; the library,
unit tests and other tools retain their portable locks. After local publishing,
restore the solution without RID or publish overrides before committing
lock-file changes.

## Focused tests and CI

```text
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release
dotnet run --project tests/DotJpegli.Tests -c Release -f net10.0 --no-build -- -class DotJpegli.Tests.EncoderSamplingTests
dotnet run --project tests/DotJpegli.Tests -c Release -f net11.0 --no-build -- -class DotJpegli.Tests.EncoderSamplingTests
```

The run commands select the same focused tests for each target framework;
`dotnet run` requires an explicit `-f net10.0` or `-f net11.0` for these projects.
To run focused regressions against the .NET Standard 2.1 library assembly,
select it explicitly while keeping the test host on .NET 10:

```text
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release -f net10.0 -p:DotJpegliLibraryTargetFramework=netstandard2.1
dotnet run --project tests/DotJpegli.Tests -c Release -f net10.0 --no-build -p:DotJpegliLibraryTargetFramework=netstandard2.1 -- -class DotJpegli.Tests.ManagedArchitectureTests -class DotJpegli.Tests.EncoderSmokeTests
```

The property forces the project reference to select `netstandard2.1` and keeps
test binaries and compiler outputs separate from regular builds. Architecture
regressions verify the loaded library's target framework and dependency contract.
The compatibility lane excludes three test classes that directly exercise
intrinsic-only internal helpers absent from that assembly; the remaining encoder,
numerical, metadata and scalar/vector fallback regressions run against it.
Software fused multiply-add regressions compare compatibility results with the
host runtime over finite adversarial and deterministic random inputs.

Full suites run only in the GitHub Actions workflow, on Windows and Linux for
the .NET 10 and .NET 11 libraries, plus the .NET Standard 2.1 library on a .NET 10
test host; benchmarks also belong on CI.
A normal push to `main` runs four targeted Ubuntu encoding benchmarks on .NET 11:
`kodim01` at distance 1, sampling 444/420 and progression 0/2. Workflow dispatch
can select the .NET 10 runner with `framework=net10.0`; the default is `net11.0`.
These run directly, without the full quality/compression or performance matrices. The native reference
and photographic corpus are cached with pinned identities and retained provenance.
The [performance guide](performance-parity.md) explains selecting individual images,
settings and bounded measurement counts through workflow dispatch. A targeted
success applies only to its selected cases and does not establish full parity.
The full [quality/compression comparison](quality-compression-parity.md), followed
by the fixed 256-case performance protocol, remains available through manual
dispatch with `mode=full`. Both paths retain their evidence. The performance
runner's local `--self-test` checks synthetic statistics and policy without
measuring encoding time.
Do not batch local selections to approximate a full suite.
Tests use xUnit and a separate StbImageSharp decoder; these packages are test-only
and are not part of the encoder or its package dependency graph.
The external decoder validates encoder output and is not a port target.

`xunit.runner.json` enables parallel execution of individual tests and discovered
theory rows, using the conservative scheduler with one worker per logical CPU.
The corpus matrix exposes each dimension, quality, sampling and option combination
as a separate theory row so it can use those workers. Inputs and random seeds
remain local to each test; shared oracle data is read-only. Keep new tests free of
shared mutable state, or explicitly disable parallel execution for tests that need it.
For a serial diagnostic run, add `-parallelMode none` to a focused command.

Independent cosine-DCT oracles cache their invariant cosine basis while retaining
the direct double-precision sum and existing tolerances. The numerical and SIMD
DCT tests also retain the literal cosine definition as a separate reference.

Exact coefficient comparisons and typed source-buffer comparisons use xUnit's
optimized array equality, avoiding one scalar assertion or boxed object per value.
Comparisons with numerical tolerances retain their per-coefficient checks.
Sampling regressions decode each baseline once and reuse those independent pixels
across scan variants within the same test.

CI actions are pinned to full commit hashes with release-version comments.
Dependabot checks those pins weekly; review the referenced releases when updating
them. Both hosted operating systems run even if the other lane fails.
CI prints the twenty slowest full-suite test cases from xUnit's XML timing report
to make future test overhead visible without running the suite locally.
Publishing `main` starts validation automatically; reuse that run instead of
dispatching a duplicate for the same revision. See [agent instructions](../AGENTS.md).
Workflow concurrency allows one active run per branch and cancels superseded runs.

The independent JPEG reader extracts entropy prefixes and magnitudes in byte
groups. Canonical Huffman decoding, segment bounds, restart markers and padding
checks remain part of the oracle.

See the [GitHub Actions workflow](../.github/workflows/ci.yml) for the hosted checks.

## InferSharp analysis

The separate [InferSharp workflow](../.github/workflows/infersharp.yml) follows
the [JetDatabaseWriter pipeline](https://github.com/symbiogenesis/JetDatabaseWriter/blob/main/.github/workflows/infersharp.yml).
Run it manually from GitHub Actions against `main`. It builds and analyzes
`netstandard2.1`, `net10.0` and `net11.0` independently, using the pinned SDK,
locked dependencies and a digest-pinned InferSharp v1.5 container. Only the
DotJpegli library and its portable PDB are staged for analysis. This build
disables Roslyn analyzers; normal CI retains the strict compiler, analyzer,
trimming and NativeAOT checks.

Each target retains text and SARIF reports and `analysis.log` for seven days,
including on analysis failure. The job summary records translation coverage
and finding counts. InferSharp can exit successfully when it finds bugs or
skips unsupported IL, so `tools/assert-infersharp.ps1` rejects findings, invalid
reports, missing coverage and empty translations. Partial translation remains
visible in the log and summary; a successful run covers only translated code.

No findings are suppressed by default. If a finding requires an exception,
review its trace and source first. The validator accepts an optional
`-BaselinePath` and `-TargetFramework` for a reviewed v1 baseline in the same
format as JetDatabaseWriter: analyzer version, reviewed commit, normalized
source SHA-256 hashes and per-target rule/path/line/fingerprint identities with
reasons. It rejects changed sources, new findings and stale exceptions. Add
the baseline and those arguments to the workflow only after that review.

Run the focused report-validator regressions with PowerShell 7.6 or newer:

```powershell
./tools/assert-infersharp.ps1 -SelfTest
```

Before dispatching, check for a queued, running or successful InferSharp run
for the intended commit and reuse it. This workflow does not start the regular
test or benchmark workflow.

## Regression coverage

Source-ICC regressions compare transformed samples with independent transfer,
matrix and PCS oracles and synthetic LUT fixtures. Encoder checks cover source
precision, RGB/BGR/alpha/padding, grayscale and XYB destinations, metadata,
serial/parallel coding, stream behavior and profile snapshots. Malformed bounds,
unsupported profiles, channel mismatches, normalized-input requirements and
cancellation are checked before output. These tests validate supported color
conversion formats without establishing corpus quality, compression or
performance parity.

Structured-metadata regressions independently inspect TIFF directories and values,
XMP XML/RDF properties and IIM dataset bytes. They cover typed and generic edits,
unknown-value preservation, byte orders, thumbnail offsets, language alternatives,
repeated datasets, UTF-8 declarations, malformed input and explicit unsupported
structures. Reconciliation checks dimensions, orientation, physical densities,
descriptive mappings, alias removal, owned output storage, raw-marker conflicts,
automatic XMP partitioning and cancellation. Complete coding-byte comparisons
across source precisions and scan modes, plus independent decoded-pixel checks,
verify that metadata editing does not change image encoding.

Parallel-encoding regressions compare complete JPEG bytes with serial execution
across source precisions, padded layouts, color spaces, odd edges, custom and
fractional sampling, smoothing, adaptive quantization, Huffman modes, sequential
and progressive scan scripts, refinement and restarts. They cover worker-limit
validation, ignored alpha/padding, out-of-range float preflight, concurrent
independent encodes, nonseekable streams, calling-thread writes, cancellation
and I/O failures. CI also exercises parallel parity with hardware intrinsics
disabled. These checks validate deterministic execution, without establishing
a measured speed improvement or upstream performance parity.

Raw-metadata regressions independently parse every supported APP/COM code,
network-byte-order lengths, zero and maximum payloads, sliced storage and
marker-looking payload bytes. They check duplicate and combined metadata order,
descriptor snapshots, default compatibility and rejection before output across
source precisions. Coding-byte and independent decoded-pixel comparisons use
unrecognized application payloads and raw comments across scan modes. Nonseekable
streams, cancellation between markers and I/O failures are also covered.

Custom-chromaticity validation compares 252 complete RGB/grayscale profiles
with independently compiled unchanged pinned C++ fixtures across six named
transfers, gamma 2.2 and all four intents. Cases include Adobe RGB, ACES AP0
imaginary primaries, custom D50/D60/D65 whites, named-valued custom coordinates
and scientific-notation descriptions. Independent primary-matrix and Bradford
oracles check adapted colorants; parsing checks tag alignment, shared curves,
source media white, omitted CICP/HDR LUTs and ICC identification. Encoding
checks verify exact APP2 embedding and unchanged coding across representative
source precisions, output color spaces, scan modes and nonseekable streams.
These checks validate profiles and sample preservation without establishing
a color-managed lossy JPEG quality comparison.

Built-in combination regressions compare all 288 named RGB/grayscale profiles
and 48 gamma-2.2 profiles across four intents with independently compiled pinned
C++ fixtures. Independent parsing checks media white, Bradford adaptation,
adapted colorants, decoding curves, CICP eligibility, HDR transform selection and ICC
identification. They cover existing factory bytes, fresh storage, invalid
parameters and exact APP2 embedding with unchanged coding across representative
source precisions, scan modes and streams. These checks validate profile
serialization and sample preservation without establishing color-managed lossy
JPEG quality.

Rendering-intent regressions compare complete profiles with independently compiled
pinned C++ fixtures across every RGB and grayscale factory and all four intents.
Independent parsing checks header intents, descriptions, aligned tag extents and
the ICC MD5 ID, including the fields excluded from identification. They check
preserved transform payloads, existing overload bytes, fresh storage, invalid
intent rejection and exact APP2 embedding with unchanged JPEG coding across
representative source precisions and scan modes. These checks validate profile
serialization and sample preservation without establishing color-managed lossy
JPEG quality or reader-specific rendering.

Custom-script regressions independently parse explicit scan descriptors and
per-coefficient progression, reconstruct final real coefficients and check
decoded-pixel parity. They cover component subsets, changing DC groups,
split and joined AC refinement bands, per-scan restart geometry, source
precisions, compatible preset bytes, caller-list snapshots and rejection
before output.

Separate-sequential regressions independently parse frame/scan descriptors,
Huffman symbols and coefficients, checking real-block raster traversal, predictor
resets, entropy padding, changing DRI intervals and RST cycling. They compare
final coefficients and decoded pixels with interleaved output across sampling
schemes, output color spaces, source precisions, custom tables and Huffman modes,
and cover default/grayscale byte compatibility, metadata, stream behavior,
cancellation and validation before output.

Custom-quantization regressions independently parse DQT zigzag values and frame
selectors, compare constant-block DC and complete cosine-DCT coefficients, and
check table-sharing effects on adaptive thresholds. They cover default-table byte
compatibility, coefficient and decoded-pixel parity across source precisions and
scan/Huffman/restart modes, custom sampling, validation before output, floating-point
coefficient preflight, and mutation of caller tables after the first stream write.

Extended-quantization regressions independently parse mixed DQT precisions,
network-byte-order words, zigzag values and SOF0/SOF1/SOF2 selection. Pinned scalar
C++ fixtures check complete coarse YCbCr tables and adaptive distance inference
at both saturation endpoints. Independent DC/cosine-DCT and entropy checks cover
coefficient parity across scan scripts, refinement and restarts; an independent
decoder compares sequential and progressive pixels. They also cover default bytes,
automatic table modes, extreme-distance clamping, typed and padded input,
out-of-range float preflight, validation before output, snapshots and cancellation.

Progression-level regressions independently parse the emitted scan descriptors,
coefficients and restart boundaries, compare final coefficients and decoded pixels
with baseline output, and verify sampling-based DC interleaving across output
color spaces. They also cover input precisions, strides, preset aliases, custom
setting rejection before output, stream behavior and cancellation.

Spectral-band regressions independently parse each scan's spectral range and
initial/refinement entropy, verify complete per-block coefficient coverage,
EOB runs, correction bits, restart cycling and scan padding, and compare final
coefficients with baseline encoding. Decoder checks verify final pixels where
the independent decoder supports the output mode. Focused cases also cover
single-position bands, typed and padded input, default-byte compatibility,
nonseekable streams, cancellation and invalid partitions before output.

Per-band refinement regressions check mixed AC depths independently of DC depth,
including upstream's level-2 spectral/bit descriptors and the maximum ten-bit
depth. Independent scan parsing checks range coverage, refinement order, EOB
runs, correction bits, restart boundaries and padding, then compares complete
coefficients and decoded pixels with baseline output. They also cover typed
inputs, default-byte compatibility, list snapshots and validation before output.

Separate-DC regressions independently parse component selectors, spectral and
refinement schedules, real-block traversal, restart counts, cycling and padding.
They compare coefficients with baseline output across odd edges, custom sampling,
mixed refinement depths and Huffman settings, with decoder checks for supported
color modes. They also cover typed input, grayscale and default compatibility,
nonseekable streams, cancellation and validation before output.

SIMD DCT regressions compare exact coefficient bits with the scalar implementation
for fractional/random blocks, all impulse positions, constants, checkerboards,
signed zero, subnormal and large finite samples. An independent cosine definition
checks coefficient units and frequency order. Both paths are checked for overlapping
spans, untouched output tails and validation before writes. CI also runs the
focused DCT checks with hardware intrinsics disabled to exercise the scalar fallback
and software vector operations.

SIMD color-conversion, downsampling and quantization regressions compare managed
vector and automatic dispatch results with the scalar implementations, including fractional
source normalization, chroma FMA rounding, adaptive threshold and coefficient
rounding boundaries, padded edges and scalar fallbacks. Complete JPEG SHA-256
fixtures check complete encoding bytes
across byte, 16-bit and float input, color spaces, sampling, smoothing, custom
extended tables, scan modes, refinement and restarts. CI repeats focused SIMD
and encoder fixture checks with hardware intrinsics disabled. Performance
measurements remain CI-only and speed parity is not established.

Numerical regressions check the DCT against an independent cosine definition.
Quantization regressions use selected independently compiled native-reference
tables. Adaptive-analysis and coefficient regressions also use independently compiled
C++ scalar fixtures from the pinned source, including edge padding and saturated
quantization tables. These sampled numerical checks do not establish full encoder
parity. Sampling regressions retain independent scalar C++ fixtures for pairwise
averaging and odd edge padding, and entropy parsing checks dummy-block tokens and
DC predictor continuity. The [quality/compression gate](quality-compression-parity.md)
encodes the same hash-pinned corpus with this pinned C++ revision and compares
decoded quality at matched sizes and bytes at matched quality. Execute corpus
measurements and performance benchmarks only on CI.

Direct-distance regressions check the public quality mapping, precedence and
validation before output, plus JPEG byte compatibility at equivalent quality
across source precisions and layouts. Independently compiled pinned C++ table
fixtures cover direct distances and nonlinear scaling. Independent marker and
coefficient parsing checks table clamping and constant-block DC values, including
positive finite distance extremes; these extremes do not establish upstream
encoder parity.

Standard-table regressions independently parse table IDs and frame selectors,
check Annex K table scaling and constant-block coefficients, and retain native
scalar fixtures for table values and shared-selector adaptive distance inference.
They also cover default byte compatibility, direct-distance precedence, source
precision, padded layouts, custom sampling, restart markers, baseline and
progressive coefficient/decoded-pixel parity, and validation before output.

Transfer-function regressions retain complete native scalar PQ/HLG quantization
tables and adaptive-inference fixtures for 4:4:4, 4:2:0 and grayscale. Independent
coefficient parsing checks shared-chroma adaptive thresholds using analytical
cosine inputs that distinguish missing hints and incorrect table selectors. They
also cover default and other-code byte compatibility, typed and padded inputs,
custom sampling, scan/refinement parity, metadata and stream behavior, finite
distance extremes, and out-of-range float coefficient validation before output.

ICC transfer lookup regressions compare inferred and explicit hints across
input precisions, layouts, sampling, adaptive and standard tables, scan modes
and restarts. They independently reconstruct APP2 chunks to check unchanged
profile bytes and cover sliced storage, marker boundaries, missing and malformed
directories, extreme unsigned fields and last-entry selection. Native helper
fixtures check the pinned lookup's permissive field handling. Cancellation,
nonseekable streams and out-of-range coefficient rejection before output are covered.

RGB-to-XYB regressions check transformed values against independently compiled
pinned scalar C++ fixtures and a double-precision transfer/opsin oracle. They
cover source normalization, ignored alpha and padding, grayscale replication,
sampling and smoothing, adaptive analysis, progressive/restart coefficient
parity, unchanged default behavior and rejection before output. Native transform
fixtures allow the documented difference from upstream's fast approximations;
these checks do not establish an ICC color-managed RGB roundtrip.

Built-in XYB profile regressions check the complete profile against a SHA-256
fixture generated by compiling the pinned C++ builder unchanged. Independent
parsing checks header fields, aligned tag extents, curve/CLUT/matrix structure
and the ICC MD5 profile ID. An independent forward-profile evaluator compares
scaled XYB samples with analytical sRGB-to-D50-XYZ values, including black,
white and saturated colors. Encoding checks verify exact APP2 embedding,
unchanged coding bytes, typed source input, progressive output and nonseekable
streams. These checks validate the profile transform, without establishing a
color-managed lossy JPEG quality comparison.

Built-in sRGB profile regressions check the complete profile against an
independently compiled pinned C++ SHA-256 fixture, the ICC MD5 ID and fresh
storage on every factory call. Independent parsing checks the header, aligned
tags, shared transfer curves and CICP fields. A double-precision sRGB-to-D50-XYZ
oracle checks the parsed matrix and curves over a color cube, including transfer
boundaries. Encoding checks cover exact APP2 embedding, unchanged coding data,
CICP lookup, source precisions, RGB/YCbCr output, progressive refinement and
nonseekable streams. These checks do not establish a color-managed lossy JPEG
quality comparison.

Built-in sRGB grayscale profile regressions compare complete bytes with an
independently compiled pinned C++ fixture, verify the ICC MD5 ID and fresh
storage, and independently parse the header, aligned tags, D65 media white and
sRGB decoding curve. Encoding checks cover exact APP2 embedding, unchanged
coding data, typed and padded inputs, explicit grayscale conversion, scan modes,
restart markers, CICP lookup and nonseekable streams. These checks do not establish
a color-managed lossy JPEG quality comparison.

Linear-sRGB profile regressions compare both complete profiles with independently
compiled pinned C++ fixtures, verify their ICC MD5 IDs and fresh storage, and
parse aligned tags, shared identity curves, colorants, white points and CICP
fields. Independent curve and matrix evaluation checks linear samples against
D50 XYZ values. Encoding checks verify exact APP2 embedding and unchanged coding
data with representative precisions, scan modes and CICP lookup settings. These
checks validate profile serialization and sample preservation, without
establishing a color-managed lossy JPEG quality comparison.

Display P3 profile regressions compare complete bytes with an independently
compiled pinned C++ fixture, verify the ICC MD5 ID and fresh storage, and parse
aligned tags, shared sRGB curves, P3 colorants, Bradford adaptation and CICP
fields. An independent double-precision P3-to-D50-XYZ oracle checks the parsed
matrix and curves across a color cube and transfer boundaries. Encoding checks
verify exact APP2 embedding and unchanged coding data across source precisions,
RGB/YCbCr output, progressive refinement, restarts and nonseekable streams.
These checks validate profile serialization and sample preservation without
establishing a color-managed lossy JPEG quality comparison.

Linear P3 and Rec.2020 profile regressions compare complete bytes with
independently compiled pinned C++ fixtures, verify ICC MD5 IDs and fresh storage,
and parse aligned tags, identity curves, colorants, adaptation and CICP fields.
Independent primary and Bradford matrix oracles check linear samples against
D50 XYZ. Encoding checks verify exact APP2 embedding and unchanged coding across
input precisions, RGB/YCbCr output, scan modes and CICP lookup settings. These
checks validate serialization and sample preservation without establishing a
color-managed lossy JPEG quality comparison.

Rec.709 and BT.709-transfer Rec.2020 profile regressions compare complete bytes
with independently compiled pinned C++ fixtures and verify ICC MD5 IDs, fresh
storage, aligned tags and CICP fields. Independent transfer-curve and
primary/Bradford matrix oracles check parsed profiles, including the original
and fixed-point-rounded transfer knee. Encoding checks verify exact APP2
embedding and unchanged coding across input precisions, RGB/YCbCr output,
progressive refinement, restarts and CICP lookup settings.

Additional RGB profile regressions compare complete bytes with independently
compiled pinned C++ fixtures and verify ICC MD5 IDs, fresh storage, aligned tags
and CICP fields. Independent transfer-curve and primary/Bradford matrix oracles
check P3 with BT.709 transfer and Rec.2020 with sRGB transfer. Encoding checks
verify exact APP2 embedding and unchanged coding across representative source
precisions, RGB/YCbCr output, scan modes, restarts and nonseekable streams.
These checks validate serialization and sample preservation without establishing
a color-managed lossy JPEG quality comparison.

DCI-P3 profile regressions compare complete bytes with an independently compiled
pinned C++ fixture and verify the ICC MD5 ID, fresh storage, aligned tags, shared
gamma 2.6 curves and CICP fields. Independent primary and Bradford matrix oracles
check colorants and DCI-to-D50 adaptation. Encoding checks verify exact APP2
embedding and unchanged coding across input precisions, RGB/YCbCr output, scan
modes, refinement, restarts and nonseekable streams. These checks validate
serialization and sample preservation without establishing a color-managed
lossy JPEG quality comparison.

Additional grayscale regressions compare all four complete profiles with
independently compiled pinned C++ fixtures and verify their MD5 IDs, fresh
storage, aligned tags and source media white. Independent BT.709/DCI formulas
and PQ/HLG transfer-table oracles check decoding and interpolation. Encoding
checks verify exact APP2 embedding, preserved coding across input precisions,
scan modes and restarts, nonseekable streams, cancellation and explicit transfer
hints in the absence of a CICP tag. These checks validate serialization and
sample preservation without establishing color-managed lossy JPEG quality.

Pure-gamma profile regressions compare complete profile hashes and IDs with
independently compiled pinned C++ fixtures, including both exponent endpoints
and fractional values. They independently parse dynamic tag offsets, aligned
descriptions, shared type-0 curves, MD5 IDs and the absence of CICP, and compare
decoded curve samples with the specified power law. Encoding checks verify exact
APP2 embedding and preserved coding across input precisions, output color spaces,
scan modes and restarts, including nonseekable streams and cancellation.

Wide-gamut pure-gamma regressions compare complete P3 and Rec.2020 profiles
with independently compiled pinned C++ fixtures, including the exponent endpoints.
Independent parsing checks aligned tags, shared type-0 curves, MD5 IDs and the
absence of CICP; primary and Bradford matrix oracles check the adapted colorants
and power-law decoding. Encoding checks verify exact APP2 embedding and unchanged
coding across source precisions, RGB/YCbCr output, progressive refinement and
nonseekable streams. These checks validate serialization and sample preservation
without establishing a color-managed lossy JPEG quality comparison.

Scaled XYB regressions compare complete emitted quantization tables with
independently compiled pinned C++ scalar fixtures. Independent JPEG parsing
checks three quantizer selectors, shared Huffman selectors, sampling factors,
constant and smoothed sampled cosine-DCT coefficients, and baseline/progressive
refinement parity. They cover Y-based adaptive thresholds, typed and padded
inputs, ICC byte preservation, stream behavior, cancellation and rejection
before output. Decoder checks compare reconstructed scaled channels across scan
modes; they do not test an ICC color-managed RGB roundtrip.

Output color-space regressions independently parse RGB frame/scan identifiers,
shared table selectors and marker framing, and check unequal constant-channel DC
coefficients against analytical values. They cover grayscale conversion and neutral
chroma, source precision/layout/stride compatibility, green-channel adaptive analysis,
custom sampling and progressive/restart coefficient parity. Default bytes and
validation before output remain covered by focused input regressions.

Additional 4:4:0 and 4:1:1 regressions use independent scalar C++ fixtures for
vertical and pairwise horizontal averaging, odd edges, block seams and smoothing.
They also check emitted sampling factors, dummy-block DC continuity, restart counts,
coefficient and decoded-pixel parity across scan modes, padded input layouts and
grayscale's independence from color sampling settings.

The 3x1, 3x2, 1x4, 4x2 and 2x4 regressions use independently compiled pinned
scalar C++ fixtures for horizontal-first averaging, intermediate rounding,
four-row pairwise sums, smoothing, block seams and replicated edges. Independent
JPEG parsing checks sampling factors, ten-block MCUs, dummy DC continuity,
restarts and coefficient parity across baseline and progressive refinement;
an independent decoder checks final pixel parity. They also cover padded byte,
16-bit and fractional float inputs. Four-block adaptive-analysis rows preserve
the established two-block spatial field when both use the same padded height.

Three-row sampling regressions use independently compiled scalar C++ fixtures
for the dedicated 1x3 and 2x3 kernels, including intermediate rounding, smoothing,
block seams and replicated edges. They check all three rows contribute, emitted
sampling factors, dummy-block DC continuity, restart markers, coefficient and
decoded-pixel parity across baseline and progressive refinement, and padded byte,
16-bit and fractional float inputs. Adaptive-analysis checks compare three-block
rows with single-block traversal of the same padded image across 24-pixel seams.
These checks cover the dedicated kernels, not the pinned upstream selector.

Custom-factor regressions independently parse component factors, real and dummy
chroma blocks, per-scan restart counts and refinement data. They compare baseline
and progressive coefficients and decoded pixels, verify every preset's byte
compatibility with equivalent explicit factors, and check asymmetric chroma DC
values against an independent color-conversion and box-average oracle. They also
cover typed inputs, padded layouts, grayscale settings and validation before output.
Independent full-image scalar C++ adaptive-field fixtures verify bounded analysis
across custom MCU rows. Emitted chroma AC checks use those fixture strengths to
verify component-local vertical indexing and scaled horizontal field columns,
with cases that distinguish incorrect indexing in baseline and progressive output.

Fractional-ratio regressions check 3:2 and 4:3 ratios, mixed axes, rational block
boundaries, smoothing and replicated edges against independent double-precision
pixel-area oracles. Independent entropy parsing checks real/dummy geometry,
scan and restart counts, coefficient parity across sequential and progressive
scans, and typed/padded input and nonseekable streams. Existing integer sample
bits and complete JPEG fixtures remain unchanged. StbImageSharp's rejection of
fractional upsampling is tested explicitly; these checks do not establish
decoded-pixel parity with a fractional-capable reader or the pinned upstream.

Subsampled-luma regressions check component maxima on different components,
3x3 and 4x4 sample areas, ten-block MCUs, replicated edges, dummy DC continuity,
restart cycling and coefficient parity through ten progressive refinement bits.
Independent double-precision box-average, smoothing and cosine-DCT oracles check
luma DC and AC coefficients. An independent decoder verifies final pixel parity
across scan modes for thirteen sampling schemes. Typed and padded input checks,
grayscale's retained adaptive analysis, automatic disabling of adaptive analysis
for subsampled luma, and acceptance of fractional factors across source precisions
are also covered.

Input smoothing regressions compare converted, filtered and subsampled block
bits with independently compiled scalar C++ fixtures, including block seams,
odd edges and padding. An independent grayscale filter and cosine DCT oracle
checks the emitted coefficients. Encoding checks cover scan and Huffman modes,
refinement, restart intervals, padded layouts, alpha handling and cancellation.

Restart regressions independently parse marker definitions and entropy segments
to check MCU boundaries, all-component DC resets, one-bit padding and marker
cycling, and compare decoded pixels with restart markers disabled. The marker
and reset rules follow [ITU T.81, sections B.2.4.4 and E.1.3/E.1.4](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).

Row-restart regressions independently derive scan widths from frame and scan
markers, verify changing DRI intervals, RST cycling, predictor resets, EOB-run
boundaries and entropy padding, and reconstruct final coefficients. They check
fixed-interval byte equivalence where scan widths match, unchanged decoded
samples, odd edges, custom sampling, progressive refinement and level presets,
overflow-safe clamping, typed inputs, validation before output and stream behavior.

Progressive regressions reconstruct every real block from independently parsed
DC and AC scans and compare it with baseline coefficients and decoded pixels.
They check noninterleaved block order, odd edges, per-scan restart counting and
padding, as well as cancellation during later scans and padded input layouts.
EOB-run regressions check run-length extension bits, the 32,767-block limit,
flat-image aggregation and run termination at restart and scan boundaries.
The scan rules follow [ITU T.81, Annex G](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).

Refinement regressions check initial point transforms and later bit planes,
including negative DC coefficients, AC sign bits, correction-bit order around
ZRL and EOB symbols, and optimized frequency collection. An independent scan
parser reconstructs coefficients across refinement rounds and compares them
with baseline output, alongside decoded-pixel, restart, edge, layout and
cancellation checks.
Refinement EOB-run regressions also check correction-bit ordering across blocks,
fixed-buffer flushes, the 32,767-block limit, and restart and scan boundaries.

Metadata regressions independently parse APP0, APP2 and COM segments to check
network-byte-order density fields, ICC sequence/count bytes, chunk boundaries,
the 255-chunk profile limit, raw marker payload bytes and UTF-8 comment lengths.
They also check default byte compatibility, unchanged decoded samples across
baseline and progressive modes, validation before output, nonseekable streams,
cancellation between metadata segments and I/O failures.

APP1 regressions independently check Exif and standard XMP identifiers, raw TIFF
byte preservation, UTF-8 without an added BOM or terminator, maximum segment
lengths, multibyte limits and malformed UTF-16 rejection before output. They also
check combined metadata ordering, unchanged coding data and decoded pixels across
baseline, progressive and refinement modes, grayscale and both Huffman settings,
along with nonseekable streams, cancellation boundaries and I/O failures.

IPTC regressions independently parse Photoshop APP13 and the image resource
header, checking sliced raw data, network-byte-order lengths, odd-byte padding
and the single-marker limit. They also cover validation before output, combined
metadata ordering, default compatibility, unchanged coding bytes and decoded
pixels, typed and padded inputs, nonseekable streams, cancellation and I/O failures.

Extended XMP regressions independently reconstruct APP1 chunks and verify exact
UTF-8 bytes, uppercase MD5 links, unsigned network-byte-order lengths and offsets,
full-chunk boundaries, and multibyte characters split across segments. They check
generated standard packets and unchanged caller-supplied packets with namespace
aliases and attribute or element links, reject invalid linkage and malformed input
before output, and cover metadata ordering, unchanged coding data and decoded
pixels, nonseekable streams, per-chunk cancellation and I/O failures.

Automatic XMP regressions check the exact UTF-8 threshold, preserved small packets,
wrapper removal and XML property/namespace preservation. Independent APP1 parsing
reconstructs chunks and verifies their lengths, offsets and MD5 link. They also
cover rejection before output, unchanged coding data and decoded pixels across
scan modes, nonseekable streams, cancellation and I/O failures.

Selective XMP regressions independently reconstruct extension chunks, verify
their MD5 link, and compare top-level property contents across both packets.
They cover selected attributes and structured elements across descriptions,
namespace aliases and inherited XML attributes, duplicate and absent selections,
all-selected metadata, compact envelopes and final UTF-8 size limits. They also
check validation before output, default compatibility, unchanged coding bytes
and decoded pixels across scan modes, nonseekable streams and cancellation.

Higher-precision regressions use independently parsed JPEG coefficients to check
fractional grayscale DC values, compare expanded 16-bit byte inputs with the byte
encoder, and compare baseline and progressive coefficients and decoded pixels.
They cover color and grayscale layouts, element strides, alpha and padding,
smoothing, sampling, adaptive and Huffman modes, restarts, stream behavior, and
float validation before output.

Out-of-range float regressions compare emitted coefficients against independent
constant-block and smoothing/box-average/cosine-DCT oracles, including values
that distinguish retaining source samples from clipping them. They check odd
edges, custom sampling, baseline/progressive/refinement and Huffman parity,
ignored alpha and padding, default compatibility, and rejection before output
for nonfinite samples, scaling overflow and unrepresentable DC/AC coefficients.

Direct YCbCr regressions use independent all-component constant-block DC and
smoothed box-average/cosine-DCT oracles, including custom sampling, subsampled
luma and odd edges. They check 16-bit byte compatibility, fractional samples,
padded strides, validation before output, and coefficient and decoded-pixel
parity across scan, Huffman and restart modes.

`Directory.Build.targets` rejects `AllowUnsafeBlocks=true`, including a command
line override. Library and tests compile with warnings treated as errors.
