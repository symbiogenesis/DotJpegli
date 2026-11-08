# Documentation

Start with the [project overview](../README.md) for setup and a minimal encoding example.

| Guide | Contents |
| --- | --- |
| [Encoding](encoding.md) | Pixel layouts, source precision, source ICC conversion, sampling, streams, limits, and output color spaces. |
| [Profiles and metadata](metadata.md) | ICC profile factories, structured editing and reconciliation, Exif, standard and extended XMP, IPTC, comments, and raw markers. |
| [Compression and scan options](compression.md) | Quality, distance, restart intervals, scan scripts, progression, smoothing, parallel execution, and memory use. |
| [Command-line encoder](cli.md) | NativeAOT CLI usage, portable downloads, and Windows, Linux, and macOS packages. |
| [Quantization](quantization.md) | Standard, custom, and extended tables, plus transfer-function tuning and ICC CICP lookup. |
| [Development and validation](development.md) | Architecture, managed SIMD, local builds, focused tests, CI policy, and regression coverage. |
| [Optimization follow-up](optimization-followup.md) | Optimization batches, implementation details, paired CI benchmarks, and offline compression studies. |
| [Publishing releases](publishing.md) | NuGet trusted publishing, GitHub release downloads, CI artifact previews, and retries. |
| [Quality and compression parity](quality-compression-parity.md) | Pinned matched-corpus comparison, metrics, acceptance bounds, and hosted evidence. |
| [Performance parity](performance-parity.md) | Current native benchmark results, fixed acceptance bounds, timing method, and CI diagnostics. |
| [Photograph comparison](photo-comparison.md) | High-resolution source photograph, seven encoder outputs, file sizes, quality scores, and reproduction instructions. |

[`upstream.json`](upstream.json) records upstream provenance, managed extensions, and validation status.
