# Targeted performance development and full acceptance evidence

`DotJpegli.Performance` runs CI-only encoding latency comparisons, paired revision regressions, workload diagnostics, and untimed allocation observations. Local execution is restricted to Release builds, synthetic `--self-test*` checks, and untimed numerical analysis commands.

```powershell
dotnet restore tools/DotJpegli.Performance --locked-mode
dotnet build tools/DotJpegli.Performance -c Release --no-restore
dotnet tools/DotJpegli.Performance/bin/Release/net11.0/DotJpegli.Performance.dll --self-test
```

## Full acceptance protocol

Include `[full-parity]` in a commit message pushed to `main` (or dispatch `ci.yml` with `mode=full`) to run the 256-case native performance matrix after the 1,280-pair quality matrix:

```sh
dotnet tools/DotJpegli.Performance/bin/Release/net11.0/DotJpegli.Performance.dll \
  --native /absolute/path/jpegli-parity-native \
  --corpus /absolute/path/kodak-ppm \
  --output /absolute/path/empty-evidence-directory
```

- **Matrix**: 24 Kodak photographs + 8 synthetic controls, distances `1` and `4`, samplings `444` and `420`, sequential (`0`) and progressive (`2`) = 256 cases. Both encoders run single-threaded with RGB8-to-YCbCr, adaptive quantization, and optimized Huffman tables.
- **Protocol**: Tiered JIT is disabled. Seven alternating native/managed blocks run per case, each with 5 untimed warmups and a common iteration count (`3..100`) calibrated to ~50 ms of native encoding. Every block must take `>= 1 ms` with relative MAD `<= 10%`.
- **Bounds**: Each of the 8 corpus/sampling/progression groups must achieve a geometric-mean managed/native median latency ratio `<= 1.05`, and every individual case must be `<= 1.10`.
- **Automatic profiling on failure**: If a latency bound fails, CI captures `dotnet-trace` CPU profiles (`performance-profile-<revision>`) on up to four worst-ratio cases (disable with workflow dispatch input `profile=false`).

## Targeted diagnostics and revision comparisons

By default, a routine push to `main` runs 4 targeted cases (`kodim01`, distance `1`, 444/420, sequential/progressive) and uploads `performance-targeted-<revision>`.

- **Custom targeted dispatch**: Run up to 64 cases on CI via workflow dispatch (`mode=targeted`) or `--diagnostic --images ... --distances ... --samplings ... --progressions ... --blocks 3..7 --warmups 5..10 [--iterations 3..100]`:

  ```sh
  gh workflow run ci.yml --ref main -f mode=targeted \
    -f images=kodim03 -f distances=4 -f samplings=420 -f progressions=2 \
    -f blocks=3 -f iterations=3 -f warmups=5
  ```

- **Baseline revision comparison (`[compare-baseline]`)**: Appending `[compare-baseline]` to a `main` commit message builds baseline revision `2ef3d5fcc12a804c7cd707446091eb74fa9c6476` in a second persistent single-CPU worker and runs 6 balanced ABBA/BAAB quartets across 32 cases (`kodim01`, `low-noise`, `low-contrast`, `tiny-odd-edges`) with default SIMD and with `DOTNET_EnableAVX512=0` (`performance-baseline-<revision>`). Local synthetic self-test: `--self-test-revision-comparison`.
- **Offline evidence diff**: Compare two existing evidence directories without running benchmarks:

  ```powershell
  dotnet run --project tools/DotJpegli.Automation -c Release -- compare-performance --baseline /absolute/baseline-evidence --current /absolute/current-evidence --baseline-revision <full-baseline-sha> --current-revision <full-current-sha> --output /absolute/comparison
  ```

## Regression comparison against released 1.0.0

Include `[compare-release]` in a `main` commit message to compare all 256 cases against the NuGet `1.0.0` source revision (`a40868bfdace7d737413085522a88d351aa16782`) across six lanes (`.NET 10` and `.NET 11` on x64 default, x64 with AVX-512 disabled, and ARM64), or use `[compare-release-wide]` to run only the `.NET 11` x64 `Vector512` lane (`DOTJPEGLI_REQUIRE_WIDE_SIMD=1`):

- **Protocol**: `--compare-revisions --release-regression` runs two persistent single-CPU workers across 6 balanced ABBA/BAAB quartets (5 warmups, 20 encodes per leg or 100 for tiny inputs). Up to 8 attempts per case are allowed when only stability guards (MAD, drift, minority timing state, or order bias) fail; the first valid attempt is final.
- **Verdict**: Fails (`exit 1`) if any valid case consistently favors `1.0.0` by `> 0.5%` across all quartets, or (`exit 2`) if any case remains invalid after 8 attempts.
- **Algorithm scenario diagnostics**: After the release gate, x64 lanes also compare `DotJpegliSparseTransposedQuantization` and (on .NET 11) `DotJpegliFullWidthTranspose` across 64 configurations (`algorithm-scenarios-*`).

## Encoder optimization batch comparisons

Include `[compare-encoder]` in a `main` commit message to compare the current encoder against pre-batch revision `4dd88ecb53c92b6f1087c0238cc8a50cbd1de166` across four lanes (`.NET 10`/`.NET 11` on x64 with `DOTNET_EnableAVX512=0` and on ARM64):

- **Cases**: 64 RGB24 configurations (`kodim01/13/19/23`, `smooth-gradient`, `low-noise`, `low-contrast`, `tiny-odd-edges`) plus 4 Display-P3 `--source-icc` configurations on `kodim01`, using 6 balanced ABBA/BAAB quartets without retries (`encoder-batch-<architecture>-<framework>-<revision>`).

## Bounded workload diagnostics

Include `[performance-workloads]` in a `main` commit message (or dispatch `ci.yml` with `performance_workloads=true`) to run `--workload-diagnostics` on x64 and ARM64 (`performance-workloads-<architecture>-<revision>`):

- **Scenarios (26 total)**: Sequential child processes (10-minute total measurement budget) covering converted-plane and coefficient-cache boundary sizes (tiled `kodim01` up to `2048x5464` and `5x` enlarged `3840x2560`), deterministic noise, RGB/XYB/Display-P3 source-ICC inputs, byte-array vs streaming output, disabled/16 MiB/default cache budgets, tiered vs untiered JIT warmups, `MaxDegreeOfParallelism = 1, 2, 4`, and `2` or `4` concurrent encodes.
- **Metrics**: Cold first-encode latency, warm batch latencies, per-encode cache peak bytes (`CacheDiagnostics`), `GC.GetTotalAllocatedBytes`, GC heap phase snapshots, and 25 ms RSS polling.
- **Local self-test**: Run `--self-test-workloads` locally to verify scenario bounds, tiling/enlargement hashes, and stream byte counting without running benchmarks.

## Interleaved kernel selection diagnostics

Include `[compare-kernels]` in a `main` commit message to run `--compare-kernels --include-automatic` on x64 and ARM64 (`.NET 10` and `.NET 11`), comparing `Original`, `Optimized`, and `Automatic` preferences for the fuzzy-erosion and narrow-zigzag kernels across `kodim01`, standard controls, and size-threshold controls (`guard-below-32k`, `guard-at-32k`, `guard-above-32k`, `narrow-tall`, `intermediate-64k`, `intermediate-128k`).

- **Outputs**: `kernel-comparison-<architecture>-<framework>-<sha>`. See the [retained assessment](../../docs/performance-evidence/2026-11-03-kernel-guard-692a052/README.md).
- **Local self-test**: `--self-test-kernel-comparison`.

## Offline numerical and microbenchmark studies

| Command | Scope | Retained report |
| --- | --- | --- |
| `--aq-reciprocal-analysis --photograph <PPM> --output <dir>` | Untimed local/CI numerical check comparing original vs certified-reciprocal AQ division across 6 images and full-bit float operands. | [`2026-11-03-aq-reciprocal`](../../docs/performance-evidence/2026-11-03-aq-reciprocal/README.md) |
| `--coefficient-cache-analysis --photograph <PPM> --output <dir>` | Untimed local/CI storage study comparing 32-block bitmap/packed-short and bit-packed tiles against dense coefficient arrays. | [`2026-11-03-compact-coefficients`](../../docs/performance-evidence/2026-11-03-compact-coefficients/README.md) |
| `--ac-reciprocal-experiment --photograph <PPM> --output <dir>` | CI-only 6-quartet timing experiment for prepared-reciprocal AC quantization intervals. | [`2026-11-03-ac-reciprocal`](../../docs/performance-evidence/2026-11-03-ac-reciprocal/README.md) |
| `--byte-stuffing-experiment --corpus <dir> --output <dir>` | CI-only 6-quartet timing experiment comparing scalar and portable SIMD `0xFF00` byte stuffing. | [`2026-11-03-byte-stuffing`](../../docs/performance-evidence/2026-11-03-byte-stuffing/README.md) |

## Untimed managed/native allocation observations

The `Managed and Google C++ jpegli allocations` CI job runs:

```text
dotnet DotJpegli.Performance.dll --allocation-evidence --native /absolute/jpegli-parity-native --corpus /absolute/corpus --output /absolute/allocation-evidence
```

- **Protocol**: Untimed observation across all 192 Kodak photograph configurations (24 images, distances `1` and `4`, 444/420, sequential/progressive), recording 3 observed encodes after 5 warmups.
- **Metrics**: Managed values record exact `GC.GetTotalAllocatedBytes` deltas (including the returned `byte[]` JPEG); native values record heap byte requests intercepted by the [GNU linker allocation probe](../parity/native/README.md#untimed-native-allocations).
- **Chart update**: Pass `--allocations` to `DotJpegli.Automation performance-charts` to regenerate the allocation chart from retained CI evidence.
