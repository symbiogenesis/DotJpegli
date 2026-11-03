# Large-image and application workload observations

[Automatic CI run 41690962313](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313)
measured revision `b07eb5d240c2be1f6b9aebac13d081fb50d740cd` on November 3,
2026. Both workload jobs completed all 25 scenarios and their output checks.
These are instrumented observations, not native parity, revision regression or
service-latency acceptance. The overall workflow had an unrelated formatting
failure, and the separate native latency comparison failed its fixed bounds
while quality/compression passed. Three of six
[released-version regression lanes](../2026-11-03-b07eb5d/release/README.md)
also failed. Successful workload collection does not imply that the workflow
passed.

This snapshot precedes the later segmented and partial converted-plane change.
It must not be presented as the performance of that later implementation.

| Lane | Host identity | Available CPUs | Runtime | Retained report |
| --- | --- | ---: | --- | --- |
| x64 | AMD EPYC 7763 | 4, affinity 0–3 | .NET 11 RC3, `11.0.0-rc.3.26522.148` | [x64 report](X64/report.md) |
| ARM64 | Implementer `0x41`, part `0xd49`, revision 0 | 4, affinity 0–3 | Same runtime product build | [ARM64 report](ARM64/report.md) |

The x64 worker reports accelerated Vector256, no Vector512 and eight float
lanes; ARM64 reports accelerated Vector128 and four float lanes. Other than the
explicit tiering scenario, tiered compilation is disabled. Each scenario starts
a fresh process, records its first encode, then three warmup batches and seven
measured batches. Inputs and settings are prepared before the first encode.
Tiny inputs use 32 encodes per caller per batch; others use one. Different
scenarios are sequential, not balanced interleaved comparisons.

## Findings within this run

Converted-plane cache admission has the largest observed discontinuity.
Increasing the image height by eight rows for 4:4:4, or sixteen rows for 4:2:0,
drops converted-plane ownership to zero in this revision and substantially
increases observed encoding time. These cases use tiled Kodak01 content,
distance 1, progressive level 2, one worker and a discard stream.

| Boundary | Below / above dimensions | x64 below / above median ms | ARM64 below / above median ms | x64 / ARM64 above-to-below ratio |
| --- | --- | ---: | ---: | ---: |
| 4:4:4 converted planes | 2048×2728 / 2048×2736 | 150.479 / 450.618 | 178.495 / 472.644 | 2.995 / 2.648 |
| 4:2:0 converted planes | 4096×2048 / 4096×2064 | 149.812 / 544.822 | 182.365 / 579.281 | 3.637 / 3.176 |
| Former whole-component coefficient bucket | 2048×4096 / 2048×4104 | 670.728 / 689.789 | 707.555 / 722.408 | 1.028 / 1.021 |
| Segmented coefficient 64 MiB capacity | 2048×5456 / 2048×5464 | 911.833 / 920.046 | 953.918 / 965.071 | 1.009 / 1.012 |

The coefficient boundaries do not show a similarly large jump in these samples.
They do not prove equivalence, performance at every image size or a speedup
against the preceding revision. The converted-plane findings motivate keeping
bounded converted rows beyond the admission boundary and measuring that change
in a later planned run.

Request concurrency is more useful than extra workers inside this particular
2048×1024 sequential 4:4:4 workload. On x64, one/two/four requested encoder
workers have medians of 27.854/38.917/38.363 ms; two and four workers are slower
in these observations. ARM64 medians are 40.550/40.367/39.354 ms, but the two-
and four-worker samples contain distinct slower timing states, so their small
median differences should not be treated as reliable gains.

| Independent concurrent calls, one worker each | x64 encodes/s | ARM64 encodes/s |
| --- | ---: | ---: |
| 1 | 35.90 | 24.66 |
| 2 | 69.43 | 49.01 |
| 4 | 79.82 | 95.63 |

Each concurrent caller owns separate input pixels. Four concurrent calls on
x64 increase median per-call latency to 49.405 ms from 27.854 ms for one call.
These results support retaining one worker as the default and evaluating
request concurrency for batch services. They do not establish a universal
worker threshold or predict throughput on different processors or images.

The three-warmup default-tiering photograph case has medians of 33.767 ms on
x64 and 73.640 ms on ARM64, compared with 5.316 and 7.570 ms for the untiered
stream case. It has made only eleven encode calls in total. ARM64's seven
measured latencies are 73.529, 76.040, 73.640, 73.717, 75.684, 72.582 and
22.844 ms. This is consistent with a changing execution state, but no JIT trace
establishes its cause. It is not a measurement of mature tiered performance.
The later diagnostic adds a separate fixed 256-warmup scenario; that count
also does not by itself prove compilation has stabilized.

The whole-photograph enlargements reach 3840×2560, or 9.8 megapixels. They use
nearest-neighbor enlargement of the pinned 768×512 Kodak01 image, preserving
its composition. Their raw results establish complete large encodes, not a
diverse camera-native high-resolution corpus. RGB, XYB and source-ICC cases
also complete; their different output settings do not provide a common quality
or native-performance comparison.

## Memory and timing limits

Every recorded image-cache ownership peak is within the configured 192 MiB
combined budget and returns to zero after encoding. This is not a process-memory
limit. For example, the x64 64 MiB coefficient-boundary cases report warm-phase
RSS around 436 MiB. Caller pixels, runtime storage, scratch, ordinary garbage
and retained pools are outside accepted cache ownership.

Allocation figures are process-wide workload traffic, including diagnostic
sinks, options copies, result wrappers and task scheduling. They are not
encoder-only allocations or retained pool capacity. Memory phase snapshots do
not force GC. Parent RSS sampling every 25 ms is a lower bound on the observed
process peak and includes setup and final untimed byte verification. The warm
phase snapshot precedes that final verification. Cold latency excludes process
startup and input/settings preparation.

The original `timingStable` field checks only relative MAD ≤10%; it is retained
exactly as produced. Low MAD can hide minority timing states. A retrospective
p90/p10 check exceeds 1.20 for ARM64 default tiering, two/four workers and tiny
input, and for x64 photo array, photo stream, noise and tiny input. ARM64 tiny
input also exceeds 10% MAD. None of these samples is removed or retried. The
later diagnostic flags either dispersion failure; that is a diagnostic
improvement, not a change to native acceptance or a relabeling of these raw files.

Seven batches cannot establish service p95/p99 or an unconditional latency
distribution. Cross-host timings are not before/after evidence. The full native
and released-version protocols remain separate.

## Provenance

Retained artifacts:

- [ARM64 artifact 13257073100](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13257073100).
- [x64 artifact 13256479783](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13256479783).

Both artifacts are copied in full: 358 files totaling 1,135,794 bytes, including
all requests, results, raw samples, runtime configurations, RSS samples and
worker logs. [Provenance](provenance.json) records artifact metadata reported
by GitHub. [Retained file hashes](retained-files.json) identify every copied
file. The archive digests are GitHub-reported values; the retained-file hashes
were calculated locally. No benchmark ran locally to produce this snapshot.
