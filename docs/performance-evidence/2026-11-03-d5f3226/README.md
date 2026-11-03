# README performance refresh

Revision `d5f3226d9501a1b636f13eb15ff9b7113a767920` was measured on November 3,
2026 (America/Chicago), in automatic [CI run 41619016995](https://github.com/symbiogenesis/DotJpegli/actions/runs/41619016995).
The complete 1,280-pair quality/compression comparison passed. All 256
performance cases are valid, stable and matched, but the fixed latency bounds
**failed**.

On this AMD EPYC 7763 x64 host, photograph group means show 3.7–4.3% lower
encoding time for progressive encoding and 25.0–25.5% higher encoding time for
sequential encoding. The worst individual managed/native ratio, including
controls, is 1.859315. The runtime is .NET `11.0.0-rc.3.26522.148`, using one
worker, eight-lane portable SIMD and disabled tiered compilation. The previous
README snapshot used an AMD EPYC 9V74 host and a different encoder revision;
these separate measurements do not establish revision gains or regressions.

- [Performance report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Quality/compression report](../../parity-evidence/2026-11-03-d5f3226/report.md), [summary](../../parity-evidence/2026-11-03-d5f3226/summary.json), [matched-quality points](../../parity-evidence/2026-11-03-d5f3226/matched-quality.json), [artifact identity](../../parity-evidence/2026-11-03-d5f3226/snapshot.json) and [retained-file hashes](../../parity-evidence/2026-11-03-d5f3226/hashes.json).
- [Native provenance](native-provenance.json) and [artifact identities and retained-file hashes](provenance.json).
- Archived CI chart values and SVGs: [metrics](charts/metrics.json), [compression](charts/compression.svg), [wall time](charts/wall-time.svg) and [allocations](charts/allocations.svg).
- Current README [chart values](../../images/performance/metrics.json), [compression](../../images/performance/compression.svg), [wall time](../../images/performance/wall-time.svg) and [allocations](../../images/performance/allocations.svg).

The README charts were regenerated locally from these retained measurements
and match the archived CI charts exactly. No benchmarks ran locally.

Compression summarizes the eight photograph mode/metric group means at matched
PSNR and SSIM. The largest absolute percentage difference, computed as
`abs(geometric mean ratio − 1) × 100`, is rounded up to the next 0.001%, yielding
**≤0.002%**. Each group uses five matched-quality anchors per photograph and mode,
from curves measured at ten distances from 0.25 to 8. This bound summarizes group
means; the complete comparison also includes eight controls, endpoints and curve
knots. The largest individual matched-quality size overhead across all targets
is 0.087051%, displayed as 0.0871% in the summary.

Wall-time bars show `(geometric mean ratio − 1) × 100` from per-case
managed/native median ratios, with photographs and controls paired under each
encoding mode. Measurements use distances 1 and 4, sampling 444/420 and
progression 0/2. Every median includes all seven alternating-order blocks.
Group averages do not override failed individual latency bounds.

Allocation bars show the arithmetic mean across photograph cases of each
case's mean block allocations divided by encodes per block, converted to KiB.
These managed-only measurements include the returned JPEG and exclude warmup.
Native allocations, retained pool capacity and peak memory were not measured.

Regenerate the same charts from the repository root with the pinned SDK and
PowerShell 7.6 or newer:

```powershell
./tools/update-performance-charts.ps1 -Performance docs/performance-evidence/2026-11-03-d5f3226/full -Quality docs/parity-evidence/2026-11-03-d5f3226 -Output docs/images/performance
```

The generator checks complete coverage, validity, source identities and agreement
between raw values and summary statistics. Its focused `-SelfTest` uses synthetic
data only. Exact inputs, JPEGs and instrumented diagnostic traces remain in the
hosted artifacts; paths inside the raw JSON refer to that artifact layout.
Instrumented profile timings do not contribute to these charts or the latency
verdict. See the [performance protocol](../../performance-parity.md) for scope and
acceptance bounds.
