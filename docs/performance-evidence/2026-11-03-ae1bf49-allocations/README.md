# Managed and official Google C++ jpegli allocations

The automatic [CI run 41781968920](https://github.com/symbiogenesis/DotJpegli/actions/runs/41781968920)
collected all **192 photograph configurations** at revision
`ae1bf49b847507aa7912ad98156aca057745ac16` on November 3, 2026
(America/Chicago; November 4 UTC). The native build, allocation wrapper tests,
provenance checks and complete allocation collection passed. No latency was
measured by this collection and no additional workflow was dispatched.

| Encoding | DotJpegli managed GC bytes, KiB/encode | Google C++ jpegli requested heap bytes, KiB/encode |
| --- | ---: | ---: |
| Sequential, 4:4:4 | 120.7 | 1924.8 |
| Sequential, 4:2:0 | 109.5 | 2190.1 |
| Progressive level 2, 4:4:4 | 168.5 | 4288.3 |
| Progressive level 2, 4:2:0 | 155.9 | 3209.3 |

Each value is an arithmetic mean across all 24 Kodak photographs and distances
1/4, with three complete encode samples per case following five warmups. Both
encoders use RGB8 to YCbCr, one worker, optimized Huffman coding, adaptive
quantization and baseline-range quantizers. Prepared input pixels and file I/O
are outside the measured scope; the returned JPEG is included. The host is an
AMD EPYC 9V45 x64 runner on Ubuntu 26.04.1 LTS, with .NET
11.0.0-rc.3.26522.148, eight float SIMD lanes and tiered compilation disabled.

DotJpegli uses precise `GC.GetTotalAllocatedBytes` deltas, including managed
object headers and alignment. Pools are warmed and their retained capacity is
excluded. Native observations count successful requests to GNU linker-wrapped
`malloc`, `calloc`, `realloc`, `aligned_alloc`, `posix_memalign` and `memalign`,
with replacement C++ allocation operators covering `new` and `new[]`. A
reallocation counts its complete new request, even if the allocator can extend
an existing block. Native allocator metadata, alignment slack and private libc
internal allocations are outside those entry points. These different accounting
domains describe allocation traffic, not peak or retained memory.

The upstream encoder source is the unmodified official
[`google/jpegli` revision `031a0077`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20).
An independent executable contains the allocation instrumentation; the ordinary
reference used for latency remains uninstrumented. Every probe JPEG must match
the ordinary native output byte for byte. Managed output must be deterministic
and its JPEG structure must match the native settings. All 192 checks passed.

## Retained evidence

- [Summary and machine identities](summary.json) and [all raw allocation samples and JPEG/input hashes](cases.json).
- [Pinned corpus manifest](corpus.json), [unmodified native build and probe provenance](native-provenance.json), and [successful job steps](jobs.json).
- [GitHub artifact identity](artifact.json), [workflow identity](run.json), and [locally calculated retained-file hashes](provenance.json).
- Regenerated [allocation chart](charts/allocations.svg) and [full-precision chart values](charts/metrics.json).

The hosted artifact retains native command logs, the exact prepared inputs and native/probe JPEGs.
Only compact evidence is copied here. GitHub's reported archive digest and
locally calculated hashes of individual retained files are recorded separately.
The generator also verifies that every allocation input hash and dimension
matches the earlier complete latency corpus.

Compression and wall-time chart data still come from the separate complete
[`744f7be` comparison](../2026-11-03-744f7be/README.md). Those SVGs regenerate
byte for byte unchanged. The allocation chart has its own run, revision and date;
combining these sources does not establish latency improvements between runs or
change the earlier performance acceptance result.

Regenerate all charts from the retained evidence without running a benchmark:

```powershell
./tools/update-performance-charts.ps1 `
  -Performance docs/performance-evidence/2026-11-03-744f7be/full `
  -Quality docs/performance-evidence/2026-11-03-744f7be/quality `
  -Allocations docs/performance-evidence/2026-11-03-ae1bf49-allocations `
  -Output .artifacts/regenerated-readme-charts
```

The [measurement protocol](../../../tools/DotJpegli.Performance/README.md#untimed-managednative-allocation-observations)
documents scope and accounting. Local validation regenerated the chart from CI
data, checked both series and all corpus hashes, and ran synthetic aggregation
checks. All allocation collection and benchmarks ran on CI.
