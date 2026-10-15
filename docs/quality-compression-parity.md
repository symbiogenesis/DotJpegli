# Quality and compression parity validation

[Documentation](README.md)

The `quality-compression-parity` job in [Managed encoder CI](../.github/workflows/ci.yml)
compares DotJpegli with unchanged Google Jpegli source at
`031a0077f5799a6041004267fc12b956c1f52a20`. A passing result establishes practical
quality/compression parity within the tolerances and scope below. It does not
prove equality on every possible image, byte-identical JPEGs, subjective visual
equivalence, performance parity or parity for untested options.

## Established result

[Hosted run 38517910693](https://github.com/symbiogenesis/DotJpegli/actions/runs/38517910693)
passed on October 14, 2026, testing revision
`b30697e833865d1e90af406d6206921e4884c624`. Both Windows and Ubuntu full-suite,
SIMD fallback and reproducible-build lanes also passed.

All **1,280 encoding pairs** passed: 24 photographs plus eight controls,
two sampling modes, two scan modes and ten distances. Coverage was complete,
with 640 matched-rate anchors and 1,280 matched-quality anchors. Endpoint/knot
checks expanded these to 2,057 matched-rate and 4,396 matched-quality targets;
no image or comparison was excluded.

| Measured bound | Worst observed | Declared limit |
| --- | ---: | ---: |
| File-size overhead at the same distance | 0.087051% | 5% |
| File-size overhead at matched PSNR or SSIM | 0.087051% | 5% |
| PSNR loss at matched file size | 0.013825 dB | 0.25 dB |
| SSIM loss at matched file size | 0.000136338 | 0.003 |
| Group geometric-mean overhead at the same distance | 0.001296% | 2% |
| Group geometric-mean overhead at matched quality | 0.001783% | 2% |

Decoded RGB bytes were identical in 872 of 1,280 pairs; complete JPEG bytes
were identical in 496. Remaining cases met every measured quality and size
bound. Optimized grayscale/YCbCr output uses upstream cost-based
[Huffman histogram clustering](compression.md#huffman-optimization).

The [full result table](parity-evidence/2026-10-14-b30697e/report.md),
[summary](parity-evidence/2026-10-14-b30697e/summary.json),
[raw cases](parity-evidence/2026-10-14-b30697e/cases.json),
[matched-rate points](parity-evidence/2026-10-14-b30697e/matched-rates.json),
[matched-quality points](parity-evidence/2026-10-14-b30697e/matched-quality.json),
[source hashes](parity-evidence/2026-10-14-b30697e/sources.json) and
[native provenance](parity-evidence/2026-10-14-b30697e/provenance.json)
are committed for durable review. JPEGs and exact RGB inputs remain in the
hosted artifact and the retained session evidence; their JSON paths refer to
that artifact layout. [File hashes](parity-evidence/2026-10-14-b30697e/hashes.json)
identify the saved evidence. The original `summary.json` SHA-256 is
`96daff390bcbfdd7e630203b07539c302f13cbbebfaacc89231ef5a180584869`.

## Corpus and matched settings

All 24 photographs in the [Kodak Lossless True Color Image Suite](https://r0k.us/graphics/kodak/)
are acquired at validation time. [The manifest](../tools/parity/corpus.json)
pins both the source PNG SHA-256 and the exact decoded P6 RGB8 SHA-256. The
standard-library PNG reader performs lossless filter reconstruction, without
resizing, color conversion or gamma correction. Generated controlled images
add gradients, hard edges, fine detail, noise and odd image boundaries. The
photographic and controlled groups have separate aggregate acceptance gates.

Both encoders consume the same RGB8 bytes and explicitly use YCbCr output,
4:4:4 or 4:2:0 sampling, adaptive quantization, optimized Huffman tables,
baseline (level 0) or the upstream progressive level-2 scan preset, baseline
quantizer bounds, no smoothing, no restarts and no descriptive metadata.
Both write the same JFIF version and density. Distances are
`0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8`; direct distance avoids differing
interpretations of a nominal quality number. Complete JPEG file lengths,
including headers and entropy padding, are counted.

The small native wrapper links the pinned upstream encoder and decoder without
changing either. Both JPEGs are decoded through that same decoder and settings
to RGB8, then compared with the original samples. Native build provenance
records the upstream/dependency commits, compiler and executable hash. Native
code and its dependencies are validation tools, excluded from the managed
encoder package.

## Metrics and acceptance policy

The thresholds are fixed before measurement; failed results remain failures.
Acceptance requires every available per-case bound and every aggregate bound:

| Comparison | Aggregate bound | Individual bound |
| --- | --- | --- |
| Managed/native bytes at the same distance | Geometric mean at most 1.02 | At most 1.05 |
| Managed PSNR loss at matched bytes | Mean at most 0.10 dB | At most 0.25 dB |
| Managed SSIM loss at matched bytes | Mean at most 0.001 | At most 0.003 |
| Managed/native bytes at matched PSNR or SSIM | Geometric mean at most 1.02 | At most 1.05 |

Aggregate groups keep corpus category, sampling and progression separate;
matched-quality rate comparisons also keep metrics separate. Better managed
results are allowed. PSNR uses equal-weight RGB sample MSE and a peak value of
255. Exact reconstruction has a documented finite 120 dB ceiling for report
serialization and interpolation. SSIM uses BT.601 luma, 8 by 8 nonoverlapping
local windows, clipped edge windows, population moments, constants
`C1 = (0.01 * 255)^2` and `C2 = (0.03 * 255)^2`, and pixel-weighted averaging.
This precisely specified SSIM is not MS-SSIM or Butteraugli; these metrics do
not constitute a subjective perceptual study.

Each image's measured rate/distortion points form a nondominated frontier,
independently for each metric. Duplicate rates retain the best measured
quality. Five interior anchors span the curves' shared range at fractions
`0.1, 0.3, 0.5, 0.7, 0.9` and determine aggregate means. Every shared-range
endpoint and frontier knot is also checked against the individual bound,
covering the worst case across the piecewise linear curves. Interpolation is
linear in quality and logarithmic in complete file bytes; matched-quality
comparisons invert the same curves.
There is no extrapolation. Constant or nonoverlapping ranges are reported
with their reason and coverage counts. Missing any of the five required
matched-rate or matched-quality anchors for an image fails the gate. JPEG
configuration checks also require matching frame geometry, component sampling,
quantization tables, JFIF fields and spectral/refinement scan scripts; entropy
table selectors may differ.

## Reproduction and evidence

Corpus measurements run only on CI, as required by [agent instructions](../AGENTS.md).
Dispatch the workflow on `main` with `mode=full` to run this comparison and the
full performance acceptance matrix. Routine pushes run four targeted performance
cases alongside full-suite and reproducible-build jobs. Reuse an existing run
for the intended mode and revision instead of dispatching a duplicate.
The workflow uploads `quality-compression-parity-<commit>` even when a comparison
fails. Its report includes source and JPEG hashes, settings, native provenance,
raw measurements, matched-rate and matched-quality results, exclusions and
the aggregate/worst-case verdict. The artifact retains JPEGs for inspection.
The comparison process exits unsuccessfully on a failed gate.

Local Release builds and the focused `--self-test` metric/interpolation check
are allowed; they do not establish corpus parity:

```text
dotnet build tools/DotJpegli.Parity/DotJpegli.Parity.csproj -c Release
dotnet run --project tools/DotJpegli.Parity -c Release -f net11.0 --no-build -- --self-test
```

The tested scope excludes grayscale and alternate source precisions, additional
sampling ratios, XYB, source ICC conversion, HDR tuning, custom quantizers,
other scan scripts, smoothing, restarts and extreme distances. Performance
parity remains a separate validation area. Any report must name its tested
revision and hosted run rather than infer success from the presence of this
comparison tool.
