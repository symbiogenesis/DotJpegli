# Bounded AQ rounding-cell investigation

The interval proof and fallback-rate investigation is complete for a guarded
reciprocal implementation covering AQ's rational expressions, `FastLog2`,
`FastPow2` and the final strength division. The guard certifies the existing
binary32 approximation results. Automatic retains original division. This is
untimed numerical evidence: it establishes neither a faster deployment nor a
latency, allocation, retained-capacity or peak-memory benefit.

## Certificate

Let `n` and `d` be the already-rounded binary32 numerator and denominator from
an existing AQ expression. The prototype accepts only finite, nonzero `n`, a
positive normal `d`, and a finite normal candidate `q` with the same sign as
`n` and finite adjacent magnitudes. The largest finite binary32 candidate is
excluded. Everything else uses the original `n / d`.

A 256-entry immutable binary64 table seeds the reciprocal of the denominator's
normalized mantissa. Two binary64 Newton iterations refine that reciprocal;
an exact power-of-two exponent scale and multiplication by `n` propose `q`.
The table payload is 2,048 bytes, in addition to its managed array header.
Correctness does not depend on a reciprocal error estimate: even a poor seed
can only increase fallback.

Write `a = abs(q)`, and let `p` and `s` be its preceding and succeeding
binary32 magnitudes. The certificate checks the strict inequalities

```text
((p + a) / 2) * d < abs(n) < ((a + s) / 2) * d
```

Both midpoints have at most 25 significant bits. Multiplication by the
24-significant-bit binary32 denominator needs at most 49 significant bits.
The midpoint sums, halves, products and converted numerator are exactly
representable in binary64: their exponents remain well within its normal
range, including the smallest-normal binary32 boundary. The comparisons
therefore use exact boundaries, without a rounded-interval error estimate.
Since `d` is positive, these inequalities are precisely membership of the
exact quotient in `q`'s open rounding cell. They certify the same binary32
result as nearest, ties-to-even division. Negative numerators follow by sign
symmetry. Strict inequalities conservatively reject midpoint ties.

Original division handles rejected cells, subnormal and zero results, signed
zero, unsupported denominators, overflow, infinities and NaNs. The SIMD research
schedule certifies each lane and uses the original vector quotient for every
fallback lane. When any lane falls back, it executes the original whole-vector
division, retaining that path's exceptional-value payload behavior. The helper
can also have executed scalar division for rejected lanes. Lane and affected
batch counts describe numerical decisions; they are not counts of eliminated
division instructions.

All numerator and denominator construction retains the current binary32
operations and ordering. The implementation keeps the original polynomial
coefficients, clamps, exponent bit reconstruction, sum ordering, saturation,
validation and cancellation points. It does not replace `FastLog2` or `FastPow2`
with correctly rounded mathematical functions, whose answers could differ from
these established approximations.

## Retained analysis

The [accelerated metadata](accelerated/run.json), [all 48 cases](accelerated/cases.json)
and [summary](accelerated/summary.json) retain exact source/encoder/runner hashes,
settings, output hashes and counts. A second [software-only analysis](software/run.json)
uses the same binaries with `DOTNET_EnableHWIntrinsic=0`; its [48 cases](software/cases.json)
and [summary](software/summary.json) retain the scalar AQ route. Each case performs
one Original and one forced research encode, with exact whole-JPEG comparison.
No timer, repeated measurement, calibration or performance statistic is involved.

Both selections use Kodak `kodim01` (768 by 512), deterministic smooth-gradient,
low-contrast, low-noise and full-noise controls (257 by 193), and tiny odd edges
(19 by 17). Each source uses distances 1 and 4, 4:4:4 and 4:2:0, baseline and
progressive level 2, AQ and optimized Huffman tables, one worker, and the default
64 MiB token budget. Thus the 96 comparisons include real AQ intermediates,
padding, the complete encoder and the actual unchanged quantization decisions.
They are a bounded diagnostic selection, outside native acceptance.

The host was Windows ARM64, .NET `11.0.0-rc.3.26522.148`. The accelerated
selection had four float lanes and no accelerated Vector256; the software
selection used scalar AQ automatically. Explicit eight-lane research paths
are covered by the focused regressions, with software Vector256 on this host.
Counts below match between accelerated and scalar selections; each column
describes one selection, rather than their combined total.

| Rational site | Certified quotients | Original-division fallbacks |
| --- | ---: | ---: |
| Mask (three divisions per block) | 228,484 | 8 |
| Derivative (pre-erosion and gamma) | 9,748,920 | 72 |
| FastLog2 | 76,164 | 0 |
| FastPow2 | 76,164 | 0 |
| Strength | 76,156 | 8 |

There are 88 fallbacks among 10,205,976 quotient decisions per selection.
The accelerated route has 72 affected four-lane batches among 2,437,248;
the scalar route has no vector batches. These small fractions certify the
tested inputs' numerical coverage; they do not establish speed.

| Source, all eight configurations | Quotient decisions | Fallbacks | Affected accelerated four-lane batches |
| --- | ---: | ---: | ---: |
| Photograph | 6,586,368 | 48 | 48 |
| Smooth gradient | 902,088 | 16 | 16 |
| Low contrast | 902,088 | 0 | 0 |
| Low noise | 902,088 | 16 | 8 |
| Full noise | 902,088 | 8 | 0 |
| Tiny odd edges | 11,256 | 0 | 0 |

Each selection additionally checks 262,144 deterministic full-bit operand pairs
against original division, preserving exact bits in every result. Of these,
97,231 are certified and 164,913 fall back. This stress sample deliberately
includes unsupported signs and exceptional/exponent ranges; its distribution
is not representative of image AQ. The independent proofs below supply the
adversarial boundary coverage beyond that sample.

## Focused validation

Release builds passed for the .NET 10, .NET 11 and .NET Standard 2.1 library
targets, and the numerical runner built in Release for .NET 11. Changed-file
format verification passed. The retained focused xUnit results cover:

- [24 .NET 11 cases](tests/net11.xml): the certificate and integrated research
  kernels, exact scalar/vector/batched strength bits and untouched output tails.
- [22 initial .NET 10 cases](tests/net10-core.xml) plus [two added ICC cases](tests/net10-icc.xml).
- [24 portable-library cases](tests/portable.xml) on .NET 10.
- [21 software-SIMD integration cases](tests/software.xml) on .NET 11 with hardware
  intrinsics disabled.
- [Eight original stencil/numerical cases alongside the first 19 integration cases](tests/stencil-and-numerical.xml),
  checking the existing default arithmetic against its independent oracles.

The helper regression covers all 254 positive normal denominator exponents,
all 256 seed bins at five positions, and four numerator choices (1,300,480
comparisons). Separate random full-bit pairs and adjacent candidate results
are checked against an independent `BigInteger` rational decoder and exact
integer midpoint cross-products. Tests include wrong-sign candidates, signed
zeros, NaN payloads, infinities, over/underflow and exact ties around even and
odd subnormals and the smallest-normal boundary.

Integrated tests cover tiny and odd edges, six image patterns, sampling factors
1/2/4, multiple luma quantizers, scalar/portable-vector/eight-block paths,
exceptional direct float inputs, complete baseline/progressive/custom scripts,
zero/partial/default token budgets, parallel replay, smoothing, public float
validation, cancellation and destination-failure prefixes and write sizes.
Linear-sRGB and Display-P3 source-ICC cases compare byte and fractional-float
inputs across concurrent encodes with separately owned preferences and sinks.
Automatic and Original record no research calls. Full suites and all timed
benchmarks remain CI-only; none ran for this local-only batch.

## Reproduction and decision

After a Release .NET 11 build, run:

```powershell
dotnet tools/DotJpegli.Performance/bin/Release/net11.0/DotJpegli.Performance.dll --aq-reciprocal-analysis --photograph /path/to/kodim01.ppm --output /path/to/empty-output
```

For scalar AQ, repeat with `DOTNET_EnableHWIntrinsic=0` in a new process and a
different empty output directory. The command accepts photographs up to 1024
by 1024 pixels and preserves incomplete status if a numerical comparison fails.
Source hashes identify the exact pixels; the fixed controls come from
`ControlledCorpus`. Output directories must be empty so stale evidence cannot
be reused. This untimed command runs locally; all timing commands retain their
existing CI guard.

These artifacts were collected from working source based on local commit
`88d5b7a`, including the integration later committed as `222223c` and the analysis
command committed with this report. The recorded executable hashes identify
the actual builds; later builds can differ through commit metadata alone.
No source images or binary outputs are duplicated in this evidence snapshot.

Keep Automatic on Original. The bounded investigation supplies the requested
rounding-aware certificate, exact fallback, regression coverage and actual AQ
fallback evidence, completing that PLAN item. It does not settle every possible
AQ approximation or supply a deployment guard. Deploying this scalarized
prototype would require separate CI performance evidence on a defined workload;
low fallback frequency alone gives no such justification.
