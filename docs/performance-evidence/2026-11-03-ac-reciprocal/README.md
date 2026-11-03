# AC reciprocal interval investigation

This bounded investigation leaves production division unchanged. It researches
one certified adjacent-reciprocal guard, including its exact fallback, rather
than every possible reciprocal algorithm. The runtime library and its public
API do not contain or select the experiment.

The settled result is to decline deployment of this prototype. All fourteen
rows in the stabilized comparison are valid and favor the original division:
the guard costs 11.7% to 27.6% more in the scalar kernel and 45.7% to 70.9% more
in the portable SIMD kernel on this host. Its very low fallback frequency on
the image-derived controls does not offset the interval checks. This is a
decision about the measured implementation and inputs, without a universal
claim about reciprocal algorithms or complete encoding.

## Interval proof

For each validated integer divisor `d` from 1 through 32767, let `r` be the
binary32 result of `1f / d`. Its significand has at most 24 bits; `d` has at most
15. Their product is exactly representable in binary64, so `(double)r * d`
determines which side of the exact reciprocal contains `r` without relying on
the direction of a binary64 division.

If the product equals one, both endpoints are `r`. If it is below one, the
endpoints are `r` and its next binary32 neighbor. If it is above one, they are
the preceding neighbor and `r`. Correctly rounded binary32 division places the
exact reciprocal between those adjacent endpoints. All endpoints are finite,
positive and normal for the supported divisors.

Rounded multiplication is monotone. For nonnegative finite coefficient `x`,
`RN(x * lower) <= RN(x / d) <= RN(x * upper)`; a negative coefficient reverses
the endpoint order. This argument includes subnormal results and signed zero;
it does not use a relative-error estimate that excludes underflow. Power-of-two
divisors have identical exact endpoints.

The guard accepts only when both products agree on the original strict
`abs(quotient) < threshold` predicate, and, when unsuppressed, on ties-to-even
integer rounding. Suppression and integer rounding partition each signed side
into contiguous intervals, so agreement certifies the enclosed quotient's
branch and result. An ambiguous interval uses the original division. Finite
coefficient validation precedes the guard; original AC range validation and
int16 saturation follow it. The scalar regression also compares the existing
quantizer's exception type, message and written prefix.

The SIMD experiment applies the same agreement checks to every lane and falls
back to the original vector division when any lane is ambiguous. Its separate
fallback counts report both coefficients and affected vector batches.

## Measurement scope

The user explicitly authorized focused local benchmarks for this session. The
normal performance runner's CI guard remains intact; a temporary local harness
called the experiment entry directly. These results carry no CI identity and
make no native or complete-encode acceptance claim.

The host was a Snapdragon X2 Elite Extreme X2E94100 (Qualcomm Oryon ARMv8),
Windows ARM64, .NET 11.0.0-rc.3.26522.148, with four accelerated float lanes.
The [initial attempt](initial-invalid/summary.json) pinned processor 0 and used
five pre-calibration warmups and 50 ms target legs. All fourteen rows were
invalid because of repeated-leg drift and, in several rows, other dispersion
or order checks. They provide no speed verdict; their
[complete raw measurements](initial-invalid/results.json) are retained.

That observed instability justified one stabilized follow-up. It pinned
processor 17 (affinity mask 131072), used AboveNormal process priority, disabled
tiered compilation, increased pre-calibration warmups to 64 per variant and
targeted 200 ms legs. Five additional warmups preceded every measured leg.
Counts remained identical between variants within each row. There were no
concurrent builds, tests or other project benchmarks during measurement. The
[stabilized metadata](stabilized/run.json) records these protocol differences,
both executable hashes, source hashes and exact counts. The evidence compares
variants within each attempt, not timings between attempts.

Each of the photograph, smooth-gradient, low-contrast, low-noise, full-noise and
tiny-odd-edge inputs contributes 16384 deterministic luma AC samples. The
preparation converts RGB to luminance, replicates block edges and uses the
current DCT. An odd block stride spreads the bounded sample across each image;
small inputs repeat to retain equal kernel work counts. Distances 1 and 4 and
444/420 luma tables are interleaved. Thresholds use the existing luma offset
plus representative nonnegative AQ contributions. They are not a measured
image's actual AQ map. A seventh input targets strict bias and half-integer
boundaries using both signs and adjacent coefficient floats.

Both variants receive identical prepared inputs and output buffers. Scalar
timing includes finite validation, quantization, AC range validation and short
storage. SIMD timing includes batch validation, quantization, AC range
validation and float storage; both SIMD variants use the same storage. It
excludes demotion and zigzag scheduling. Endpoint preparation, fallback
diagnostics, JIT warmups and output comparisons are outside the timers. Raw
reports retain all six ABBA/BAAB quartets and apply the existing MAD, percentile,
within-quartet drift and order-bias guards. Invalid and inconclusive rows remain
in the evidence.

The experiment does not measure DC, complete encoding, actual AQ-derived
fallback distributions, other processors or runtimes, retained rental capacity
or peak working memory. Production's fused transposed quantizer also has
zero-row and certified suppression shortcuts; the generic SIMD comparison does
not measure replacing those paths.

## Results and decision

The [stabilized summary](stabilized/summary.json) and
[all raw quartets and validity analyses](stabilized/results.json) retain every
row. Ratios below one favor the reciprocal guard; all fourteen valid rows favor
original division with the existing 0.5% preference margin in every quartet.
Each scalar/vector pair uses the same 16384 coefficients. Fallback counts below
are coefficients and whole four-lane vector batches, respectively.

| Input | Scalar guard/division | SIMD guard/division | Ambiguous coefficients | Ambiguous SIMD batches |
| --- | ---: | ---: | ---: | ---: |
| Photograph (`kodim01`) | 1.136 | 1.457 | 0 / 16384 | 0 / 4096 |
| Smooth gradient | 1.276 | 1.468 | 0 / 16384 | 0 / 4096 |
| Low contrast | 1.211 | 1.470 | 0 / 16384 | 0 / 4096 |
| Low noise | 1.193 | 1.492 | 0 / 16384 | 0 / 4096 |
| Full noise | 1.117 | 1.477 | 1 / 16384 | 1 / 4096 |
| Tiny odd edges | 1.245 | 1.466 | 0 / 16384 | 0 / 4096 |
| Adversarial boundaries | 1.272 | 1.709 | 4898 / 16384 | 3553 / 4096 |

The image-derived controls have at most one ambiguous coefficient per row
(0.0061%). The boundary stress control has 29.90% ambiguous coefficients and
86.74% affected SIMD batches. The guard remains slower on rows with no fallback,
so these measurements identify check/preparation-access overhead in this kernel
even before division fallback becomes frequent. They do not isolate an
individual machine instruction's cost or measure complete-encode impact.

Keep exact production division. Reopening this specific candidate requires a
materially different guard or a defined workload and new evidence, rather than
deploying this prototype on the strength of its proof or fallback percentage.
Approximation research for AQ rational functions, `FastLog2` and `FastPow2`
remains a separate question.

## Reproduction

The [local reproducer source](harness/Program.cs) and
[project](harness/Research.csproj) retain the AC invocation from the session
harness. The measured session entry point also dispatched an unrelated stuffing
experiment; that unused branch is omitted here, so the reproducer's executable
hash will differ from the recorded session hash.
Their paths assume they are copied to `.artifacts/plan-research/` under the
repository, after a Release net11.0 library build. The harness compiles the
performance sources with the `DotJpegli.Performance` assembly name for the
existing internal access, selects its own entry point, disables tiering and
sets the recorded affinity/priority. It invokes the research entry directly;
it does not set or impersonate a CI environment. The measured working tree was
based on `59f0da0` and included both research experiments; the recorded encoder
hash also includes the independent stuffing diagnostic control. Production
quantization arithmetic was unchanged.

With a fresh output directory, the measured command was equivalent to:

```powershell
& ./.artifacts/tooling/dotnet/dotnet.exe ./.artifacts/plan-research/bin/Release/net11.0/DotJpegli.Performance.dll ac --ac-reciprocal-experiment --photograph ./.artifacts/plan-research-corpus/kodim01.ppm --output ./.artifacts/ac-reciprocal-stabilized
```

The committed performance CLI exposes the same experiment behind its original
CI guard as `--ac-reciprocal-experiment --photograph <PPM> --output <directory>`.
The photograph's decoded pixel hash is recorded in metadata; runtime, compiler
and runner changes can produce different executable or numerical input hashes.

## Focused validation

Five focused regressions certify reciprocal endpoints for every divisor,
enclose rounded division and compare guarded results at adversarial bias and
half-integer boundaries for every divisor, exercise deterministic raw float bit
patterns including subnormals and exceptional values, compare production AC
validation and written prefixes, and compare SIMD batches with an independent
scalar reference. Local Release builds and the focused tests passed on .NET 10
and .NET 11.
