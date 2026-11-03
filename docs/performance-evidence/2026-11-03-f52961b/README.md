# Fresh performance measurements and README charts

Revision `f52961be35384adfe99b078a47ec0f16b21633e4` was measured on November 3,
2026 (America/Chicago), in automatic [CI run 41603913358](https://github.com/symbiogenesis/DotJpegli/actions/runs/41603913358).
The complete quality/compression comparison, full test suites, deployment,
reproducibility and script checks passed. All 256 performance cases are valid,
stable and matched, but the fixed latency bounds **failed**.

On this AMD EPYC 9V74 x64 host, photograph group means show 19.8–20.2% lower
latency for progressive encoding and 13.5–16.1% higher latency for sequential
encoding. The worst individual ratio, including controls, is 1.502862.
The runtime is .NET `11.0.0-rc.3.26522.148`, using one worker and disabled
tiered compilation. Different CPU models and revisions in earlier snapshots
prevent interpreting their differences as optimization gains or regressions.

- [Performance report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Fresh quality/compression report](../../parity-evidence/2026-11-03-f52961b/report.md), [summary](../../parity-evidence/2026-11-03-f52961b/summary.json), [matched-quality points](../../parity-evidence/2026-11-03-f52961b/matched-quality.json) and [artifact identity](../../parity-evidence/2026-11-03-f52961b/snapshot.json).
- [Native provenance](native-provenance.json) and [artifact identities and retained-file hashes](provenance.json).
- [README chart values](../../images/performance/metrics.json) and SVGs for [compression](../../images/performance/compression.svg), [wall time](../../images/performance/wall-time.svg) and [allocations](../../images/performance/allocations.svg).
- Original CI SVGs: [compression](charts/compression.svg), [wall time](charts/wall-time.svg) and [allocations](charts/allocations.svg).

The measurements and numeric chart values were retained from that CI run.
The original CI SVGs are archived in `charts/`, with their original hashes in
`provenance.json`.
The current README SVGs were regenerated locally with the chart generator to simplify
the figures and support light and dark themes; the measured values are unchanged.
The compression summary shows an upper bound on the largest absolute percentage
file-size difference among the eight photograph mode/metric groups. For each
group, the difference is `(geometric mean ratio − 1) × 100`; the maximum absolute
difference is rounded up to the next 0.001%, yielding **≤0.002%**. Exact per-mode
PSNR and SSIM ratios remain in `metrics.json`. Each ratio uses five matched PSNR
or SSIM anchors per photograph and mode, using quality curves measured at ten
distances from 0.25 to 8. This bound summarizes group means. Separately, the
complete 1,280-pair comparison includes eight controls and checks endpoints and
curve knots as well as the summarized anchors. The largest matched-quality size
overhead across all targets is 0.087051%, displayed as 0.0871% in the summary.

Wall-time bars show percentage differences from native, computed as
`(geometric mean ratio − 1) × 100` from per-case managed/native median ratios.
Photographs and controls are paired under each encoding mode on the same scale:
left of zero means less time and right means more. Measurements use distances
1 and 4, and every median uses all seven measurement blocks. These group averages
cannot override failed individual latency bounds.

Allocation bars show the arithmetic mean across photograph cases of each case's
mean block allocations divided by encodes per block, converted to KiB. These
managed-only measurements include the returned JPEG and exclude warmup;
native allocations, retained pool capacity and peak memory were not measured.
They do not establish memory parity.

Regenerate the same charts from the retained measurements, from the repository
root with the pinned SDK and PowerShell 7.6 or newer:

```powershell
./tools/update-performance-charts.ps1 -Performance docs/performance-evidence/2026-11-03-f52961b/full -Quality docs/parity-evidence/2026-11-03-f52961b -Output docs/images/performance
```

This reads evidence without encoding images or running benchmarks. The generator
checks completeness, validity, source identities and agreement between raw
values and summary statistics. Its focused `-SelfTest` uses synthetic data only.
Future full CI runs retain the three charts in this style and their exact values in the
`performance-charts-<revision>` artifact.

Exact inputs, JPEGs and instrumented diagnostic traces remain in the hosted
artifacts. Paths inside the raw JSON refer to that artifact layout. Instrumented
profile timings do not contribute to these charts or the latency verdict.
