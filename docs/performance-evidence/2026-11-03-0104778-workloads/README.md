# Segmented converted-plane workload observations

[Automatic CI run 41695349570](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570)
measured revision `0104778b2691cf3e61d7dbac280457f383d9bc0c` on November 3,
2026. Both workload jobs completed all 26 scenarios and their output checks.
This snapshot preserves the instrumented workload observations after segmented
and partial converted-plane caching. It does not establish native parity,
released-version regression acceptance or service latency bounds. The overall
workflow failed its separate native latency and chart-generation steps and two
of six release-regression lanes (x64 .NET 10/11 default). Successful workload
collection does not imply that the workflow passed.

| Lane | Host identity | Available CPUs | Runtime | Retained report |
| --- | --- | ---: | --- | --- |
| x64 | AMD EPYC 9V74 80-Core Processor | 4, affinity 0-3 | .NET 11 RC3, `11.0.0-rc.3.26522.148` | [x64 report](X64/report.md) |
| ARM64 | Implementer `0x41`, part `0xd49`, revision 0 | 4, affinity 0-3 | Same runtime product build | [ARM64 report](ARM64/report.md) |

Each scenario starts a fresh process, prepares its inputs and options, records
the first encode, then runs three warmup batches and seven measured batches.
The separate extended-tiering photograph scenario uses 256 warmup batches.
Tiny inputs use 32 encodes per caller per batch; other scenarios use one.
Tiered compilation is disabled except in the two explicit tiering scenarios.
Scenarios run sequentially rather than as balanced interleaved comparisons.

## Converted-plane boundaries

The boundary cases use tiled Kodak01 content, distance 1, progressive level 2,
one worker and a discard stream.

| Boundary | Below / above dimensions | x64 below / above median ms | ARM64 below / above median ms | x64 / ARM64 above-to-below ratio |
| --- | --- | ---: | ---: | ---: |
| 4:4:4 converted planes | 2048x2728 / 2048x2736 | 118.472 / 130.053 | 175.074 / 186.945 | 1.098 / 1.068 |
| 4:2:0 converted planes | 4096x2048 / 4096x2064 | 116.697 / 119.091 | 181.802 / 186.790 | 1.021 / 1.027 |
| Former whole-component coefficient bucket | 2048x4096 / 2048x4104 | 303.609 / 311.617 | 418.801 / 424.517 | 1.026 / 1.014 |
| Segmented coefficient 64 MiB capacity | 2048x5456 / 2048x5464 | 489.043 / 500.513 | 671.138 / 680.093 | 1.023 / 1.013 |

The 4:4:4 above-boundary case retains
2,720 of 2,736 converted rows; the 4:2:0 case retains all 2,064 rows. Every
boundary row passes this diagnostic's dispersion checks. These within-run
observations do not establish revision speedups across separate hosts and runs.

## Concurrency and tiering

For the 2048x1024 sequential 4:4:4 scaling workload, one/two/four requested
encoder workers have medians of 20.427/28.378/26.861 ms on x64 and
40.979/40.423/40.219 ms on ARM64. All three x64 worker rows and the two/four
worker ARM64 rows fail the diagnostic's dispersion bounds. Their median
differences should not be treated as reliable scaling gains.

| Independent concurrent calls, one worker each | x64 encodes/s | ARM64 encodes/s |
| --- | ---: | ---: |
| 1 | 48.95 | 24.40 |
| 2 | 96.17 | 48.56 |
| 4 | 113.35 | 94.62 |

Each concurrent caller owns separate input pixels. The two/four concurrent
rows pass the dispersion checks. On x64, four concurrent calls increase
median per-call latency to 35.034 ms from 20.427 ms for one call. These are
observations on four available CPUs, with task scheduling included in batch
throughput. They do not establish a general worker threshold or predict other
hosts, workloads or service latency distributions.

The extended-tiering photograph case has medians of 3.324 ms on x64 and
7.106 ms on ARM64, compared with 36.795 and 73.562 ms after three warmups.
The extended x64 row and the three-warmup ARM64 row fail dispersion checks.
A fixed count of 256 warmups does not prove compilation has stabilized, and
no JIT trace establishes the reason for the different observed timing states.

## Memory and measurement limits

Each lane records 453 cache observations, including cold encodes and measured
calls. Every recorded ownership peak stays within that scenario's configured
cache budget and returns to zero after encoding. The usual 192 MiB budget
applies to accepted image-cache ownership, not total process memory. For
example, the 64 MiB coefficient-boundary cases have warm-phase RSS around
774 MiB on x64 and 794-799 MiB on ARM64.

Allocation traffic includes diagnostic sinks, result wrappers, options copies
and task scheduling. It is not encoder-only allocation or retained pool
capacity. RSS and managed-heap snapshots include caller pixels and the runtime
and do not force GC. Parent RSS sampling every 25 ms gives a lower bound on
the process peak and includes setup and final untimed output verification.
Cold latency excludes process startup and input/settings preparation. Stream
timing discards bytes; byte equality is checked separately.

The diagnostic marks relative MAD above 10% or p90/p10 above 1.20 as noisy;
all samples are retained. Noisy ARM64 rows are photo array, three-warmup
default tiering, tiny input and two/four workers. Noisy x64 rows are photo
array, photo stream, extended default tiering, tiny input and one/two/four
workers. These checks describe dispersion rather than timing acceptance.
Seven measured batches cannot establish service p95/p99. The 9.8-megapixel
photograph scenarios enlarge one pinned Kodak image rather than sampling a
diverse camera-native corpus. Cross-host timings are not before/after evidence.

## Provenance

Retained artifacts:

- [ARM64 artifact 13260362920](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570/artifacts/13260362920).
- [x64 artifact 13260692531](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570/artifacts/13260692531).

Both artifacts are copied in full: 372 files totaling 1,283,908 bytes,
including requests, results, raw samples, runtime configurations, RSS samples
and worker logs. [Provenance](provenance.json) records artifact metadata reported
by GitHub. [Retained file hashes](retained-files.json) identify every copied
file. Archive digests are GitHub-reported values; retained-file hashes were
calculated locally. No benchmark ran locally to produce this snapshot.
