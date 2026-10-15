# One-case .NET runtime and portable SIMD API comparison

Kodak01 4:2:0 sequential distance 1, same CPU, 7 blocks/20 iterations/5 warmups. The first pair uses identical net10.0 binaries to isolate runtime changes. The second pair uses the pinned .NET 11 SDK and runtime, comparing portable Shuffle/Create lane assembly with Zip/Unzip/Concat in the DCT transpose and downsampling. Both implementations use portable APIs, with no CPU-specific intrinsic paths. All four JPEG hashes must match. No whole-corpus or migration verdict.

Host: INTEL(R) XEON(R) PLATINUM 8573C; architecture: X64.

| Build/runtime | Managed median (ms) | Native median (ms) |
| --- | ---: | ---: |
| 10.0.13 | 10.593 | 5.886 |
| 11.0.0-rc.2.26504.105 | 10.508 | 5.835 |
| net11-baseline | 10.515 | 5.853 |
| net11-simd | 10.501 | 5.824 |

.NET 11/.NET 10 managed latency ratio: **0.991983** (lower is better). JPEG hashes match.

Explicit .NET 11 SIMD/baseline latency ratio on .NET 11: **0.998677** (lower is better). This measures this rewrite only, not all possible new API uses.
