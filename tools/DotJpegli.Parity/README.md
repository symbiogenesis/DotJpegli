# Quality and compression evidence runner

`DotJpegli.Parity` validates quality and compression parity against the pinned native Google Jpegli wrapper (`tools/parity/native`) and provides offline compression-research commands. Full corpus runs execute only on CI.

```text
dotnet DotJpegli.Parity.dll --native /absolute/path/jpegli-parity-native --corpus /absolute/path/corpus --output /absolute/path/empty-evidence-directory
```

## Corpus comparison protocol

- **Inputs**: 24 Kodak photographs (`kodim01.ppm`..`kodim24.ppm` in binary P6 RGB8, verified against `tools/parity/corpus.json`) plus 8 generated deterministic control scenes.
- **Settings**: 4:4:4 and 4:2:0 sampling, sequential (level 0) and progressive (level 2), adaptive quantization, optimized Huffman coding, baseline quantizer bounds (`1..255`), JFIF 1.01 (`1x1` density), and distances `0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8` (1,280 pairs total). Both JPEGs are decoded to RGB8 with the same pinned native decoder.
- **Acceptance bounds** (applied separately per corpus category, sampling mode, and progression mode):
  - Same-distance and matched-quality file-size ratio (managed / native): geometric mean `<= 1.02`, individual `<= 1.05`.
  - Matched-rate PSNR loss: mean `<= 0.10 dB`, individual `<= 0.25 dB` (equal-weight RGB MSE, peak 255, capped at 120 dB for exact matches).
  - Matched-rate SSIM loss: mean `<= 0.001`, individual `<= 0.003` (BT.601 luma, disjoint 8x8 windows clipped at edges, population moments, `C1 = (0.01 * 255)^2`, `C2 = (0.03 * 255)^2`, pixel-weighted mean).
- **Frontier interpolation**: Nondominated rate-distortion points per metric are interpolated linearly in quality and logarithmically in complete file bytes. Five interior anchors (`0.1, 0.3, 0.5, 0.7, 0.9` of the shared interval) determine aggregate means, while all shared-range endpoints and frontier knots are checked against the individual bounds.
- **Outputs**: `summary.json` (schema 1), `sources.json`, `cases.json`, `matched-rates.json`, `matched-quality.json`, `report.md`, native logs, and all paired JPEGs. Exit codes: `0` (passed), `1` (bound or coverage failure), `2` (invalid input or tool error).

### Local self-test

Run the fast synthetic self-test locally (verifies metrics, PPM parsing, frontier interpolation, and marker validation without running the corpus):

```text
dotnet DotJpegli.Parity.dll --self-test
```

## Reusable offline quality objectives

Offline quantization and rate-distortion searches evaluate decoded candidates against three quality floors—RGB PSNR, local luma SSIM, and a 5-scale multiscale structural objective (`dotjpegli-ms-ssim-v1`)—caching immutable source features so repeated candidate evaluations avoid redundant source work:

- **Multiscale SSIM (`dotjpegli-ms-ssim-v1`)**: Follows [Wang, Simoncelli, and Bovik (2003)](https://ece.uwaterloo.ca/~z70wang/publications/msssim.pdf) using double-precision BT.601 luma, a normalized `11x11` Gaussian window (`sigma = 1.5`) with replicated edges, `C1 = (0.01 * 255)^2`, `C2 = (0.03 * 255)^2`, scale weights `0.0448, 0.2856, 0.3001, 0.2363, 0.1333`, and `2x2` box downsampling across five scales.
- **Verification**: Both the control and every provisional winning candidate are re-verified with independent unprepared reference scorers before acceptance (`PreparedQualityMetricsTests`, `MultiScaleSsimTests`).

## Bounded progressive scan search

Searches a 181-candidate family of progressive scan scripts and Huffman table limits for a single binary P6 RGB8 image:

```text
dotnet DotJpegli.Parity.dll --search-progressive-scans --input /absolute/image.ppm --output /absolute/new-search-directory --distance 1 --sampling 420
```

- **Candidates**: The default level-2 progressive preset (control) plus 180 combinations of AC band splits (`[63]`, `[2,63]`, `[5,63]`, `[15,63]`), uniform AC refinement depths (`0, 1, 2` with full-precision interleaved DC), component AC orders (`Y/Cb/Cr`, `Cb/Cr/Y`, `Cr/Cb/Y`), and Huffman table policies (standard tables, automatic clustering, or clustering capped at `1, 2, 4` tables per class).
- **Selection**: All candidates share identical quantized coefficients; the candidate with the smallest complete JPEG byte length wins (ties keep the earliest candidate, so the winner never exceeds the control size).
- **Outputs**: `winner.jpg`, `search.json` (schema 1), and `report.md`. Focused tests: `ProgressiveScanSearchTests`, `ProgressiveScanSearchCommandTests`, `HuffmanTableLimitTests`.

## Bounded quantization-table search

Screens 729 AC quantization band-scaling configurations and encodes/decodes up to 17 finalists for a single P6 RGB8 image:

```text
dotnet DotJpegli.Parity.dll --search-quantization-tables --input /absolute/image.ppm --output /absolute/new-search-directory --native /absolute/jpegli-parity-native --distance 1 --sampling 420
```

- **Candidates**: Starting from the distance-derived Y, Cb, and Cr tables (with DC fixed), three zigzag AC bands (`1..5`, `6..20`, `21..63`) are scaled by `0.8, 1.0, 1.25` independently for luma and chroma (`3^6 = 729` combinations).
- **Screening and selection**: A single DCT pass caches band distortion and bit-cost surrogates to shortlist up to 16 candidates across weights `0.25, 0.5, 1, 2` plus the unchanged control. Finalists are encoded and decoded through the native wrapper; the smallest JPEG meeting all three control quality floors (`PSNR`, `SSIM`, `dotjpegli-ms-ssim-v1`) wins.
- **Outputs**: `winner.jpg`, `search.json` (schema 2), and `report.md`. Focused tests: `QuantizationTableSearchTests`, `QuantizationTableSearchCommandTests`.

## Bounded rate-distortion quantization search

Evaluates per-block dynamic-programming AC quantization across 25 distance/weight pairs (`distances 0.5, 1, 2, 4, 8` x `weights 0, 1, 16, 256, 4096`):

```text
dotnet DotJpegli.Parity.dll --search-rate-distortion --input /absolute/image.ppm --output /absolute/new-search-directory --native /absolute/jpegli-parity-native --sampling 420
```

- **Algorithm**: Starting from normal AQ-quantized coefficients, each block evaluates keeping each AC value, stepping one quantization unit toward zero, or zeroing it (keeping DC fixed), minimizing squared DCT error plus `weight * estimated_AC_bits` before rebuilding optimized Huffman tables.
- **Selection**: Compares the 5-distance rate-distortion curves against the weight-0 control curve and writes the smallest JPEG that meets all three distance-1 control quality scores to `winner.jpg` (`search.json` schema 2, `report.md`). Focused tests: `RateDistortionQuantizationTests`, `EncoderRateDistortionTests`, `RateDistortionSearchTests`, `RateDistortionSearchCommandTests`.

## Follow-up compression experiments

Four additional offline research commands evaluate candidate compression techniques on P6 RGB8 images up to `1024x1024` without altering public encoder defaults:

```text
dotnet DotJpegli.Parity.dll --search-huffman-orderings --input IMAGE.ppm --output NEW_DIRECTORY --native PINNED_DECODER --distance 1 --sampling 420 --progression 2
dotnet DotJpegli.Parity.dll --search-dc-trellis --input IMAGE.ppm --output NEW_DIRECTORY --native PINNED_DECODER --sampling 420
dotnet DotJpegli.Parity.dll --search-huffman-feedback --input IMAGE.ppm --output NEW_DIRECTORY --native PINNED_DECODER --sampling 420
dotnet DotJpegli.Parity.dll --search-color-preprocessing --input IMAGE.ppm --output NEW_DIRECTORY --native PINNED_DECODER --distance 1 --sampling 420
```

| Command | Description |
| --- | --- |
| `--search-huffman-orderings` | Compares canonical, descending-frequency, ascending-frequency, and reversed symbol orders within each Huffman code length (lossless to quantized coefficients; reduces `0xFF00` byte stuffing). |
| `--search-dc-trellis` | Evaluates restart-aware dynamic programming across adjacent DC values at distance 1 (`weights 0, 1, 16, 256, 4096`, up to 65,536 blocks/component). |
| `--search-huffman-feedback` | Compares two-pass AC quantization using the image's clustered Huffman code lengths against single-pass standard-table estimates (`weights 16, 256, 4096`). |
| `--search-color-preprocessing` | Evaluates up to 15 candidates combining damped 4:2:0 chroma reconstruction corrections and saturated-edge overshoot around the target distance. |

Each command writes `winner.jpg`, `search.json`, and `report.md`, requiring any winning candidate to match or beat the control's `PSNR`, `SSIM`, and `dotjpegli-ms-ssim-v1` scores while reducing complete JPEG byte size.
