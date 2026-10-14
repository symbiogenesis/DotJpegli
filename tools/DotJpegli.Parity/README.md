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
