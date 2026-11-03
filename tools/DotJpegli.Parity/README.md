# Quality and compression evidence runner

Run corpus comparisons only in CI, following the repository's validation rules.
The executable has no runtime package dependencies beyond the managed encoder.
It accepts the pinned native wrapper from `tools/parity/native`:

```text
dotnet DotJpegli.Parity.dll --native /absolute/path/jpegli-parity-native --corpus /absolute/path/corpus --output /absolute/path/empty-evidence-directory
```

The corpus must contain all 24 `kodim01.ppm` through `kodim24.ppm` Kodak photographs,
losslessly converted to binary P6 RGB8. The CI corpus preparation script validates
their pinned source hashes. The runner adds eight deterministic controlled scenes.
It compares both 4:4:4 and 4:2:0, sequential level 0 and progressive level 2,
with AQ and optimized Huffman coding, at distances
0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6 and 8. Both encoders receive identical RGB
bytes, JFIF metadata and baseline-range quantizer settings. A structural gate
checks frame/component/sampling/quantizer/scan settings before drawing conclusions.
The same pinned Jpegli RGB8 decoder decodes both JPEGs.

The fixed noninferiority bounds apply separately to each corpus category, sampling
mode and progression mode. Same-distance sizes and inverse PSNR/SSIM curves at
matched quality require a managed/native geometric mean rate ratio <=1.02 and
every case or target <=1.05. At matched rates, PSNR loss is at most 0.10 dB mean
and 0.25 dB each target; SSIM loss is at most 0.001 mean and 0.003 each target.
Each image must supply five matched-rate targets and five matched-quality targets
for each metric. A constant curve or absent shared interval is reported and fails
the coverage gate. Bounds are never relaxed after results are observed.

RGB PSNR uses mean squared sample error with range 255. SSIM uses BT.601 luma,
disjoint 8x8 windows (clipped at edges), population moments, C1=(0.01*255)^2,
C2=(0.03*255)^2, and a pixel-weighted mean. These metrics do not establish a human
visual-quality judgment, Butteraugli/MS-SSIM parity, or a universal compression
ranking. Exact identity PSNR is capped at 120 dB for finite interpolation.

Rate-distortion interpolation keeps metric-specific nondominated samples, chooses
maximum quality at duplicate sizes, and interpolates quality linearly in log size.
The inverse uses the same curve. Five anchors lie at 0.1, 0.3, 0.5, 0.7 and 0.9 of
the common log-rate or quality interval, and determine aggregate means. Worst
individual bounds additionally check overlap endpoints and all frontier knots
from both encoders. These points bound the full common piecewise-linear
interpolated range; real encoder behavior between measured distance settings is
not inferred. Extrapolation is prohibited. Original
samples remain in `cases.json`, so curve inversions and discarded dominated points
can be audited.

`summary.json` has versioned schema 1, fixed settings and bounds, executable hashes,
tested revision, group verdicts, coverage and all failures. `sources.json` records
exact input dimensions and hashes; `matched-rates.json` and `matched-quality.json`
contain interpolated evidence. `report.md`, native logs, canonical PPM inputs and
all paired JPEGs are retained. Decoded PPM scratch is discarded after its RGB hash
and quality metrics are recorded. Exit codes are 0 for all gates passed, 1 for
measured or coverage failures, and 2 for invalid input or runner/native errors.

The focused local check is allowed; it does not run the comparison corpus:

```text
dotnet DotJpegli.Parity.dll --self-test
```

It verifies metric identities and analytic distortion, strict PPM parsing,
frontiers/interpolation/inverse interpolation, missing coverage and failure bounds,
and JPEG marker validation using four small encoding fixtures.

## Bounded progressive scan search

Use the opt-in offline search for a single binary P6 RGB8 image:

```text
dotnet DotJpegli.Parity.dll --search-progressive-scans --input /absolute/image.ppm --output /absolute/new-search-directory --distance 1 --sampling 420
```

Input distance must be greater than zero through 25; sampling is 444 or 420.
The output directory must not exist. The command searches serially with AQ,
baseline-range quantizers, YCbCr output and one encoder worker. The encoder's
normal entry points retain their existing behavior; search is confined to this
explicit tool command. Run corpus compression or performance experiments on CI.

The finite family has 181 candidates, with the existing optimized level-2
progressive encoding first as the control. The other 180 combine:

- AC band ends `[63]`, `[2,63]`, `[5,63]` or `[15,63]`;
- uniform AC refinement depth zero, one or two, with full-precision interleaved DC;
- component AC orders Y/Cb/Cr, Cb/Cr/Y or Cr/Cb/Y within each band and refinement;
- standard Huffman tables, the existing automatic optimized assignment, or
  optimized histogram clustering constrained to one, two or four tables per class.

Constrained clustering merges into its existing slots once the limit is reached,
with the existing coding-plus-definition cost estimate choosing the merge.
Each candidate builds its own histograms and assignments. Candidate order also
affects automatic clustering and table redefinitions. Standard versus optimized
coding changes EOB aggregation as well as table construction; it is a complete
coding-policy comparison. The final choice uses the complete encoded byte length,
including DHT/SOS headers, padding, EOB aggregation and stuffing. The first
minimum wins ties, so the selected output is never larger than the included
level-2 control. This is a bounded search, without a global optimum claim.

All candidates keep source pixels, distance, sampling, AQ and quantizers fixed.
Focused regressions independently decode every real quantized coefficient and
pixel across the entire family, distances 1/4 and sampling 444/420 on an odd-edge
fixture. The search does not change the quantization or distortion objective.
The internal table limit is an offline experimental control and adds no public
encoder API.

`winner.jpg` contains the selected output. `search.json` (schema 1) retains the
input, pixel, encoder and candidate JPEG SHA256 hashes; fixed settings; every
script and table limit; actual per-scan Huffman selectors; complete candidate
sizes, scan counts and DHT definition counts/bytes. DHT bytes include marker and
length framing as well as each table's descriptor, counts and symbols. The
human-readable `report.md` lists every size and DHT cost. Repeat the same command
with matching source and encoder hashes to reproduce the result.

Reports charge all 181 complete encodes, output scans and optimized histogram
scans, including rejected candidates. Collection and emission/replay count as
separate logical entropy passes; they are not counts of coefficient transforms,
source traversals or measured CPU time. Pool-bucket capacity and peak working
memory are not inferred from these counts. The search keeps candidate JPEGs
serially, retaining only the winner and current output plus bounded trial metadata.

Focused local validation is available through the `ProgressiveScanSearchTests`,
`ProgressiveScanSearchCommandTests` and `HuffmanTableLimitTests` test classes.
These use small correctness fixtures without timing measurements; full suites
and corpus measurements remain CI-only.
