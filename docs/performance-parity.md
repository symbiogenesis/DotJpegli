# Performance parity validation

[Documentation](README.md)

Performance parity requires a passing hosted comparison against unchanged Google
Jpegli at `031a0077f5799a6041004267fc12b956c1f52a20`. The presence of the runner
does not establish parity. Results must name the tested revision, hosted run,
machine and measured bounds. This protocol was fixed before its first measurement.

## Scope and acceptance policy

The comparison covers the 24 hash-pinned Kodak photographs and eight deterministic
controls used by the [quality/compression comparison](quality-compression-parity.md).
Distances are **1 and 4**, sampling is **4:4:4 and 4:2:0**, and progression is
**sequential level 0 and progressive level 2**, giving **256 matched cases**.
Both encoders use RGB8 input, YCbCr output, adaptive quantization, optimized
Huffman coding, baseline-range quantizers and equivalent JFIF metadata.
Smoothing, restart intervals and optional metadata are absent. The primary
comparison uses one managed worker and the single-threaded native encoder.

Acceptance requires complete coverage, matching JPEG frame/sampling/quantization/
scan settings, deterministic outputs, valid timing samples and both bounds below.
The ratio is managed latency divided by native latency; lower is better.

| Bound | Fixed limit |
| --- | ---: |
| Geometric mean of case latency ratios, separately for each corpus category, sampling and progression | 1.05 |
| Every case latency ratio | 1.10 |

These bounds express latency noninferiority within 5% per group and 10% per case.
They are never relaxed after measurements. An aggregate speedup cannot conceal
a slower individual case or a failed group. Each image and distance has equal
weight within its group. A failed or unstable measurement cannot support a parity
claim. Raw timings expose variation so reviewers can assess the evidence.
Every block must contain at least 1 ms of summed timed work. The relative median
absolute deviation must be at most 10% separately for each encoder in every case;
exceeding this fixed variability limit makes the evidence invalid.

## Timing method

Inputs are loaded before timing. Each timed operation creates a fresh encoder,
configures the matched options, encodes all pixels and finishes a complete JPEG
in newly allocated memory. Managed timing covers the public byte-array encode
API, including its output copy and allocations. Native timing covers compressor
creation, configuration, memory destination, scanline submission, finish and
destruction. Native destination freeing and output checks occur outside timing;
managed garbage collection during encoding remains included. No forced collections
are used. These are the normal APIs' allocation and output costs.

Reading input files, process startup, JIT warmup, output hashing, JPEG validation,
file writes and report generation are excluded. The native wrapper uses a
monotonic steady clock; the managed runner uses `Stopwatch`. A calibration encode
chooses a common iteration count targeting a 50 ms native block, bounded to
3–100 encodes. Calibration never contributes to reported samples. Each encoder
receives five warmup encodes before every block. Seven paired blocks alternate
which encoder runs first. Each encode has its own timed interval; the intervals
are summed and divided by iterations to form a block sample. Output comparisons
and native buffer release between iterations remain outside these intervals.
The case ratio compares the two encoders' median block samples, retaining all
seven samples without discarding outliers.

CI pins the comparison process and its native children to the same available CPU.
Encoders run serially, with no concurrent benchmark in this runner. The Linux
host remains shared infrastructure; CPU model, OS, runtime, SIMD width, timer
frequency and relevant environment settings are retained. Tiered compilation is
disabled for this comparison so optimized JIT code does not change tiers during
measurement. This measured runtime configuration must accompany any speed claim.

## Reproduction and retained evidence

All benchmarks run only on CI under [agent instructions](../AGENTS.md). Publishing
`main` starts the existing workflow; reuse its automatic run. The comparison
follows the quality/compression measurement on the same Ubuntu runner, using the
same pinned native build and prepared corpus. It uploads
`performance-parity-<revision>`, including unsuccessful comparisons.

Evidence includes a versioned summary, group verdicts and failures, raw repeated
timings, per-case output and input hashes, native build provenance, runtime and
machine details and a readable report. Exit 0 means every bound passed; exit 1
means valid measurements exceeded the latency bounds; exit 2 means invalid
input, settings, coverage or timing evidence, or a runner error.
Retain the report and machine-readable evidence durably when recording a
hosted result.

Local builds and the focused synthetic-statistics check are allowed:

```text
dotnet build tools/DotJpegli.Performance/DotJpegli.Performance.csproj -c Release
dotnet run --project tools/DotJpegli.Performance -c Release --no-build -- --self-test
```

This scope measures single-image steady-state encoding latency. It excludes
cold startup, decoding, parallel scaling, batch throughput, allocation/memory
parity, other architectures and untested encoder options. A successful result
would establish parity only for the measured corpus, settings, host and bounds.
