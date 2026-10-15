# Documentation

Start with the [project overview](../README.md) for setup and a minimal encoding example.

| Guide | Contents |
| --- | --- |
| [Encoding](encoding.md) | Pixel layouts, source precision, source ICC conversion, sampling, streams, limits and output color spaces. |
| [Profiles and metadata](metadata.md) | ICC profile factories, structured editing and reconciliation, Exif, standard and extended XMP, IPTC, comments and raw markers. |
| [Compression and scan options](compression.md) | Quality, distance, restart intervals, scan scripts, progression, smoothing, parallel execution and memory use. |
| [Quantization](quantization.md) | Standard, custom and extended tables, plus transfer-function tuning and ICC lookup. |
| [Development and validation](development.md) | Upstream implementation, managed SIMD, local builds, focused tests, CI policy and regression coverage. |
| [Quality and compression parity](quality-compression-parity.md) | Pinned matched-corpus comparison, metrics, acceptance bounds and hosted evidence. |
| [Performance parity](performance-parity.md) | Current measurement scope, matched encoding latency, fixed acceptance bounds and CI diagnostics. |

[upstream.json](upstream.json) records source provenance, managed extensions and validation scope.
