# Quality and compression parity validation

[Documentation](README.md)

The `quality-compression-parity` job in [CI](../.github/workflows/ci.yml) compares DotJpegli against unmodified Google Jpegli (`031a0077f5799a6041004267fc12b956c1f52a20`). Passing this job establishes quality and compression parity within the tested scope and tolerances below. It does not guarantee byte-identical JPEGs, performance parity, or parity on untested options.

## Established result

The full quality and compression comparison passes for revision `be14f89` in [CI run 42376806989](https://github.com/symbiogenesis/DotJpegli/actions/runs/42376806989) (November 7, 2026). Its [retained summary](performance-evidence/2026-11-07-be14f89/quality/summary.json) evaluates 1,280 encoding pairs (2,850 matched-rate targets and 4,396 matched-quality targets) after the Huffman entropy coding, progressive refinement, and rolling/contiguous converted-plane updates:

- Every matched-quality group geometric mean is slightly smaller than native (up to 0.031342% smaller on photographs).
- The worst individual matched-quality or same-distance file-size overhead is 0.363636% (within the 2% group and 5% individual bounds).

### Retained full baseline (`b07eb5d`)

The complete raw baseline snapshot from [CI run 41690962313](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313) (November 3, 2026, revision `b07eb5d240c2be1f6b9aebac13d081fb50d740cd`) is also committed in full. All **1,280 encoding pairs** passed across 24 photographs and 8 synthetic controls, two sampling modes, two scan modes, and ten distances (640 matched-rate anchors and 1,280 matched-quality anchors, expanded with endpoint/knot checks to 2,057 matched-rate and 4,396 matched-quality targets):

| Measured bound (`b07eb5d`) | Worst observed | Declared limit |
| --- | ---: | ---: |
| File-size overhead at the same distance | 0.087051% | 5% |
| File-size overhead at matched PSNR or SSIM | 0.087051% | 5% |
| PSNR loss at matched file size | 0.013825 dB | 0.25 dB |
| SSIM loss at matched file size | 0.000136338 | 0.003 |
| Group geometric-mean overhead at the same distance | 0.001296% | 2% |
| Group geometric-mean overhead at matched quality | 0.001783% | 2% |

In that baseline, decoded RGB bytes were identical in 872 of 1,280 pairs and complete JPEG bytes were identical in 496; all remaining cases met every quality and size bound. Optimized grayscale/YCbCr output uses upstream cost-based [Huffman histogram clustering](compression.md#huffman-optimization).

Committed baseline files:
- [Full result table](parity-evidence/2026-11-03-b07eb5d/report.md) and [summary](parity-evidence/2026-11-03-b07eb5d/summary.json)
- [Raw cases](parity-evidence/2026-11-03-b07eb5d/cases.json), [matched-rate points](parity-evidence/2026-11-03-b07eb5d/matched-rates.json), and [matched-quality points](parity-evidence/2026-11-03-b07eb5d/matched-quality.json)
- [Source hashes](parity-evidence/2026-11-03-b07eb5d/sources.json), [native provenance](parity-evidence/2026-11-03-b07eb5d/provenance.json), [artifact identity](parity-evidence/2026-11-03-b07eb5d/snapshot.json), and [file hashes](parity-evidence/2026-11-03-b07eb5d/hashes.json)

## Corpus and matched settings

- **Corpus**: All 24 photographs from the [Kodak Lossless True Color Image Suite](https://r0k.us/graphics/kodak/) plus 8 generated control images (gradients, hard edges, fine detail, noise, and odd dimensions). [The corpus manifest](../tools/parity/corpus.json) pins both the source PNG SHA-256 and the decoded P6 RGB8 SHA-256. Photographs and controls are evaluated in separate aggregate groups.
- **Encoder settings**: Both encoders receive identical RGB8 bytes and use YCbCr output, 4:4:4 or 4:2:0 sampling, adaptive quantization, optimized Huffman tables, baseline (level 0) or progressive level-2 scans, baseline quantizer bounds (`1..255`), no smoothing, no restarts, and JFIF 1.01 (`1x1` aspect density) with no extra metadata.
- **Distances**: `0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8`. Complete JPEG byte lengths (including headers and bit padding) are compared.
- **Decoding**: Both JPEGs are decoded to RGB8 using the same pinned upstream Jpegli decoder (`JDCT_ISLOW` and fancy upsampling) before comparing against the source image.

## Metrics and acceptance policy

Thresholds are fixed before measurement. Passing requires meeting every aggregate and per-case bound:

| Comparison | Aggregate bound | Individual bound |
| --- | --- | --- |
| Managed/native bytes at the same distance | Geometric mean <= 1.02 | <= 1.05 |
| Managed PSNR loss at matched bytes | Mean <= 0.10 dB | <= 0.25 dB |
| Managed SSIM loss at matched bytes | Mean <= 0.001 | <= 0.003 |
| Managed/native bytes at matched PSNR or SSIM | Geometric mean <= 1.02 | <= 1.05 |

- **PSNR**: Equal-weight RGB sample MSE with peak 255 (capped at 120 dB for exact reconstruction).
- **SSIM**: BT.601 luma, disjoint 8x8 windows (clipped at image edges), population moments, `C1 = (0.01 * 255)^2`, `C2 = (0.03 * 255)^2`, and pixel-weighted averaging.
- **Frontier interpolation**: Each image's measured rate/distortion points form a nondominated frontier per metric. Five interior anchors (`0.1, 0.3, 0.5, 0.7, 0.9` of the shared log-byte or quality interval) determine aggregate means, while every shared-range endpoint and frontier knot is checked against the individual bound. Missing any required anchor fails the gate.
- **Structural validation**: Frame geometry, component sampling, quantization tables, JFIF fields, and scan scripts must match between managed and native outputs (entropy table selectors may differ).

## Reproduction and evidence

Full corpus comparisons run only on CI (triggered via `[full-parity]` in the commit message on `main` or `mode=full` workflow dispatch). The CI job uploads `quality-compression-parity-<commit>` containing all reports, JSON evidence, and encoded JPEGs.

Locally, you can build the runner and execute its synthetic `--self-test` metric and interpolation check:

```powershell
dotnet build tools/DotJpegli.Parity/DotJpegli.Parity.csproj -c Release
dotnet run --project tools/DotJpegli.Parity -c Release -f net11.0 --no-build -- --self-test
```

See the [parity runner guide](../tools/DotJpegli.Parity/README.md) for offline compression research commands.
