# Development and validation

[Documentation](README.md)

## Upstream implementation

DotJpegli ports and extends the encoder from Google Jpegli revision
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20).
The [upstream manifest](upstream.json) records provenance and managed extensions,
and [third-party notices](../THIRD-PARTY-NOTICES.md) map each managed file to
its upstream C++ source. The library contains no native binaries, P/Invoke,
pointer code, `Unsafe` APIs, or runtime NuGet dependencies.

### DCT, quantization, and portable SIMD

- **Standard coefficient units:** The managed DCT outputs standard JPEG
  coefficient units (upstream's DCT outputs `1/8` of those units and compensates
  during quantization).
- **Portable SIMD:** On `.NET 10` and `.NET 11`, portable vector paths
  (`Vector<float>`, `Vector128`, `Vector256`, `Vector512`) accelerate the forward
  DCT, RGB-to-YCbCr conversion, unsmoothed integer-ratio downsampling, 3×3
  smoothing, adaptive quantization, and AC quantization while preserving exact
  scalar evaluation order, fused multiply-add (FMA) rounding, and threshold
  comparisons. `.NET 11` uses portable `Zip`/`Unzip`/`Concat` operations for DCT
  transposes and 2×2 downsampling; `.NET 10` uses compatible lane assembly;
  `.NET Standard 2.1` uses scalar paths with software single-rounding FMA.
- **Fused DCT and quantization:** On complete converted planes, `ForwardDct`
  fuses the 2D DCT, prepared quantization, AC range validation, 64-bit nonzero
  AC bitmask generation (`ulong`), and zigzag reordering without an intermediate
  float buffer. When `TailZeroRunBias > 0`, `PruneTailZeroRunTransposed` prunes
  borderline trailing `±1` coefficients directly from in-register transposed
  DCT vectors without a second 2D DCT pass.
- **Cached configurations:** Built-in quantization tables, selectors, adaptive
  multipliers, and pre-transposed `PreparedTable[]` instances are cached in
  `Quantization` (up to 256 entries), and scan-script presets are cached in
  `JpegScanConfiguration`.

### Adaptive quantization and input smoothing

- **Single-pass block modulation:** Scalar and 8-block SIMD adaptive
  quantization compute high-frequency and gamma modulation in a single pass per
  8×8 block, carrying each horizontal neighbor into the next column while
  preserving raster addition order.
- **Pre-erosion row reuse and direct reduction:** When edge clamping repeats the
  same four-pixel stencil row, AQ copies the previous pre-erosion row instead of
  recomputing it. Each four-pixel column reduces directly into its output buffer
  without a full-row intermediate `difference` array.
- **3-row smoothing halo:** Input smoothing (`SmoothingFactor > 0`) reuses a
  fixed 3-row halo and tile (`4,504` bytes of stack scratch) to preserve the
  8-neighbor addition order while cutting redundant sample reads.

### Color conversion and ICC profiles

- **Prepared ICC curves:** Validated destination ICC curves precompute their
  exact lower/upper clipping endpoints, reciprocal gamma, power-branch
  thresholds, and piecewise inverse joins once per encode instead of per pixel.
- **Fast lookup tables and grid indexing:** For 8-bit inputs with at least 256
  pixels, matrix/XYZ and grayscale source profiles precompute 256-entry float
  lookup tables per curve. Sampled destination curves with 512–8,192 knots use a
  1,028-byte single-level value grid index (in a non-inlined helper) to Narrow
  binary search ranges.
- **Byte-XYB table:** Raw 8-bit RGB/grayscale XYB encodes share a lazily
  initialized 256-entry (`1 KiB`) sRGB linearization lookup table.

### Memory, caching, and entropy coding

- **Converted sample planes (`<= 64 MiB`):**
  - Small and medium images (`<= 8 MiB` total plane size) rent a single
    contiguous `ArrayPool<float>` buffer for direct row indexing (using
    half-resolution chroma planes for unsmoothed 4:2:0).
  - Larger single-worker, unsmoothed 4:4:4, 4:2:0, and grayscale encodes
    (`height > 32`) use a **32-row luma / 16-row 4:2:0 chroma circular window**
    (`PixelSource.WithRollingConvertedPlanes`) when full planes exceed the
    `8 MiB` contiguous rental limit or when `MaxCacheBytes` is constrained.
  - Other multi-pass encodes use bounded row segments or a 16-row-aligned cached
    prefix, recomputing uncached rows on demand.
- **Coefficient cache (`<= 64 MiB`) and nonzero AC masks:**
  - `QuantizedImage` stores `short[64]` quantized blocks (contiguously or in
    bounded segments) alongside a `ulong` nonzero AC bitmask per block.
  - During progressive AC scans, `ProgressiveEntropyEncoder` uses the bitmask to
    skip empty blocks (`bandNonzero == 0`) without loading the 128-byte
    coefficient block and uses half-block SIMD (`OrderedAcMask`) when a point
    transform (`SuccessiveLow > 0`) is active.
  - When coefficients are uncached or partially cached, `ProgressiveAcCollector`
    fuses multiple AC scans for a component into a single block traversal;
    complete caches keep scan-major traversal.
- **Entropy token cache (`<= 64 MiB` shared) and buffered output:**
  - Optimized Huffman scans share a 64 MiB token budget (packing up to 24
    refinement correction bits per 32-bit token) and `[ThreadStatic]` pooled
    `long[256]` histograms.
  - `BitWriter` emits stuffed 8-byte words in a single capacity check whenever
    at least 16 bytes remain in its buffer.
  - `PooledOutputStream` pre-sizes its initial buffer from image dimensions
    (`clamp(width * height / 2, 4 KiB, 1 MiB)`) to avoid repeated growth copies.

### Parallel execution

When `MaxDegreeOfParallelism > 1` (or `-1`) and the image is large enough:

- **Parallel DCT rows (`ParallelDctRows`):** Workers convert, downsample, and
  transform block ranges across each MCU row (`>= 16,384` pixels and `>= 64`
  blocks/row, `>= 32` blocks/worker). Eligible 3-component baseline 4:4:4 and
  4:2:0 encodes produce quantized `short` rows directly in worker threads.
- **Parallel baseline histogram/token collection:** Complete-cache baseline
  scans collect per-worker histograms and token chunks in parallel (`<= 32`
  slots, `<= 4 MiB` pooled staging) before deterministic ordered merge.
- **Parallel entropy replay:** Complete cached-token scans replay bitstreams in
  parallel either at restart marker boundaries or across 8,192-token bit chunks
  (for scans without restarts that have `>= 16,384` tokens, merging unpadded
  32-bit word streams on the calling thread within a `4 MiB` staging cap).

### Deterministic dispatch and evaluated prototypes

See [optimization follow-up](optimization-followup.md) for paired CI benchmark
results across optimization batches.

| Mechanism / study | Production status and evidence |
| --- | --- |
| Narrow transposed zigzag ordering | Automatic uses tiled 2×4 ordering on 8-lane 256-bit x64 (without 512-bit SIMD) when `width >= 8`, `height >= 8`, and `pixels >= 32,768`; other configurations keep the shuffle network ([kernel guard evidence](performance-evidence/2026-11-03-kernel-guard-692a052/README.md)). |
| Sequential coefficient caching | Single-worker interleaved baseline YCbCr streams tokens directly, except on 256-bit SIMD hosts without 512-bit SIMD where component rows are prepared separately within the 64 MiB budget. |
| Single-context entropy replay | Scans using a single Huffman context read the table's immutable lookup directly without copying a 6-context lookup array onto the stack. |
| AC reciprocal intervals (offline study) | Certified binary32 reciprocal intervals cost more than hardware division on the measured host; production retains division ([AC reciprocal evidence](performance-evidence/2026-11-03-ac-reciprocal/README.md)). |
| AQ certified reciprocals (diagnostic) | `AqRationalKernel.Optimized` certifies rational quotients via a 256-entry binary64 seed table and Newton steps; `Automatic` keeps exact division ([AQ reciprocal evidence](performance-evidence/2026-11-03-aq-reciprocal/README.md)). |
| Compact 32-block coefficient tiles (offline study) | Bitmap and bit-packed tile codecs were evaluated offline for storage and operation counts; production uses dense/segmented `short` blocks with `ulong` masks ([compact coefficient evidence](performance-evidence/2026-11-03-compact-coefficients/README.md)). |
| SIMD `0xFF00` byte stuffing (diagnostic) | `EntropyByteStuffingKernel.Optimized` uses a 256-entry 128-bit shuffle table; `Automatic` keeps scalar stuffing based on complete-encode benchmarks ([byte stuffing evidence](performance-evidence/2026-11-03-byte-stuffing/README.md)). |
| `DotJpegliFullWidthTranspose` & `DotJpegliSparseTransposedQuantization` | MSBuild properties (default `false`) kept for isolated CI A/B comparisons without adding runtime branches to normal builds. |
| Offline compression searches | Opt-in progressive scan, quantization-table, RDOQ, Huffman symbol-ordering, DC trellis, and color-preprocessing searches live in [`DotJpegli.Parity`](../tools/DotJpegli.Parity/README.md). |

---

## Build

The library targets `netstandard2.1`, `net10.0`, and `net11.0`; tests and tools
target `net10.0` and `net11.0`. All projects compile with C# 15 using the exact
.NET SDK pinned in [`global.json`](../global.json) (`11.0.100-rc.3.26522.148`,
with `allowPrerelease: true` and roll-forward disabled). Running `.NET 10` test
and tool targets also requires the .NET 10 runtime.

Build the library in Release from the repository root:

```powershell
dotnet restore DotJpegli.slnx --locked-mode
dotnet build src/DotJpegli/DotJpegli.csproj -c Release --no-restore
```

Pass `-f netstandard2.1` to build only the portable target.

### Analyzers and code style

All projects enable the .NET SDK analyzers (`AnalysisLevel=latest-recommended`),
`EnforceCodeStyleInBuild`, and `TreatWarningsAsErrors`, and forbid
`AllowUnsafeBlocks=true` in `Directory.Build.targets`.

- **Local suppressions:** Narrowly scoped suppressions are used only where
  required by file formats (such as MD5 for ICC profile IDs and Extended XMP
  digests), standard Exif type names, or `CA1861` in test methods.
- **Disabled pathological rules (`CA5390`, `CA5403`):** These two opt-in rules
  (hard-coded encryption keys and certificates) are set to `none` in
  `.editorconfig` due to
  [known quadratic analyzer performance bugs](https://github.com/dotnet/roslyn-analyzers/issues/6737)
  ([dotnet/sdk#56280](https://github.com/dotnet/sdk/issues/56280)) that added ~96
  seconds per tool rebuild.

### PowerShell and automation tools

Repository scripts require PowerShell 7.6 LTS or newer (`pwsh`), use strict mode
3.0, and fail on any `PSScriptAnalyzer` diagnostic. To run the PowerShell
analyzer locally:

```powershell
Install-PSResource -Name PSScriptAnalyzer -Version 1.25.0 -Scope CurrentUser -TrustRepository
Install-PSResource -Name powershell-yaml -Version 0.4.12 -Scope CurrentUser -TrustRepository
./tools/ci/analyze-powershell.ps1
```

The [`DotJpegli.Automation`](../tools/DotJpegli.Automation/) project handles
corpus preparation, InferSharp validation, performance evidence processing,
native provenance verification, and release inspection. Run its local self-tests
(which use synthetic fixtures and do not run benchmarks):

```powershell
dotnet run --project tools/DotJpegli.Automation -c Release -- self-test
```

---

## Banned APIs

All projects reference `Microsoft.CodeAnalysis.BannedApiAnalyzers` as a private
build-only analyzer using [`BannedSymbols.txt`](../BannedSymbols.txt). It bans:

- Unsafe/pointer/native interop (`Unsafe`, `Marshal`, `GCHandle`, function pointers)
- Architecture-specific intrinsics (`System.Runtime.Intrinsics.X86`, `Arm`) in
  favor of portable `System.Numerics.Vector` and `Vector128`/`256`/`512`
- Dynamic code generation (`Reflection.Emit`), blocking task waits, insecure
  serializers, weak hashes (except format-mandated MD5), and process-wide state
  mutation

Any `RS0030` violation fails the build. The encoder itself uses no filesystem,
process, console, network, clock, or random APIs.

---

## Reproducible builds

All projects use [DotNet.ReproducibleBuilds](https://github.com/dotnet/reproducible-builds)
and its Isolated SDK (`2.0.5`), locked NuGet dependency graphs
(`packages.lock.json`), LF checkouts (`.gitattributes`), and a deterministic
package ZIP timestamp (`1980-01-01 UTC`).

To verify locally that two clean Git worktrees at different paths produce
byte-identical `netstandard2.1`, `net10.0`, and `net11.0` DLLs and `.nupkg`
archives from `HEAD`:

```powershell
dotnet run --project tools/DotJpegli.Automation -c Release -- release --reproducibility
```

When intentionally updating a NuGet dependency in `Directory.Packages.props`,
run `dotnet restore DotJpegli.slnx --force-evaluate` and commit the updated
`packages.lock.json` files.

---

## Trimming and NativeAOT

The `net10.0` and `net11.0` library targets set `IsAotCompatible=true`, enabling
trimming, single-file, and AOT analyzers during every build.

`tests/DotJpegli.AotSmoke` roots the entire `DotJpegli` assembly with
`TrimmerRootAssembly` and runs smoke tests covering progressive and parallel
encoding, high-precision inputs, source ICC conversion, and Exif/XMP/IPTC
roundtrips. To publish and run the trimmed and NativeAOT smoke checks locally
(replace `<RID>` with e.g. `win-x64`, `win-arm64`, or `linux-x64`):

```powershell
dotnet publish tests/DotJpegli.AotSmoke/DotJpegli.AotSmoke.csproj -c Release -f net10.0 -r <RID> --self-contained true -p:PublishAot=false -o <TEMP>/trimmed-smoke
<TEMP>/trimmed-smoke/DotJpegli.AotSmoke

dotnet publish tests/DotJpegli.AotSmoke/DotJpegli.AotSmoke.csproj -c Release -f net10.0 -r <RID> --self-contained true -p:PublishAot=true -o <TEMP>/aot-smoke
<TEMP>/aot-smoke/DotJpegli.AotSmoke
```

Repeat with `-f net11.0` to test the .NET 11 SIMD paths.

---

## Focused tests and CI

Local validation uses Release builds and focused test classes (full test suites
and all benchmarks run only on CI, per [`AGENTS.md`](../AGENTS.md)):

```powershell
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release
dotnet run --project tests/DotJpegli.Tests -c Release -f net10.0 --no-build -- -class DotJpegli.Tests.EncoderSamplingTests
dotnet run --project tests/DotJpegli.Tests -c Release -f net11.0 --no-build -- -class DotJpegli.Tests.EncoderSamplingTests
```

To run focused tests against the `.NET Standard 2.1` library assembly on the
`.NET 10` test host:

```powershell
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release -f net10.0 -p:DotJpegliLibraryTargetFramework=netstandard2.1
dotnet run --project tests/DotJpegli.Tests -c Release -f net10.0 --no-build -p:DotJpegliLibraryTargetFramework=netstandard2.1 -- -class DotJpegli.Tests.ManagedArchitectureTests -class DotJpegli.Tests.EncoderSmokeTests
```

In [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), every push to
`main` runs:

- Full unit and regression suites on Windows and Linux across `net10.0`,
  `net11.0`, and `netstandard2.1` (including scalar-fallback runs with hardware
  intrinsics disabled)
- EditorConfig formatting, PowerShell script analysis, reproducible-build
  verification, trimmed/NativeAOT smoke tests, Release NuGet packaging, and
  NativeAOT CLI installer builds/tests across all five RIDs
- Four targeted Ubuntu `.NET 11` encoding benchmarks on `kodim01` (or the full
  quality/performance matrices when `[full-parity]` is in the commit message;
  see [performance parity](performance-parity.md))

---

## InferSharp analysis

The manual [`.github/workflows/infersharp.yml`](../.github/workflows/infersharp.yml)
workflow runs static resource-leak and null-dereference analysis across the
`netstandard2.1`, `net10.0`, and `net11.0` builds using a digest-pinned
InferSharp v1.5 container.

`DotJpegli.Automation infer-sharp` validates translation coverage and fails on any
unreviewed finding against [`tools/infersharp-baseline.json`](../tools/infersharp-baseline.json)
(which records one reviewed `netstandard2.1` false positive on
`EntropyTokenScans`'s `using` disposal in `EncodeOptimizedScans`; the `net10.0`
and `net11.0` exception lists are empty).

---

## Regression coverage

`tests/DotJpegli.Tests/` validates the encoder using independent mathematical
oracles, custom bitstream/marker parsers, an independent managed decoder
(StbImageSharp, test-only), and fixtures compiled directly from the pinned C++
Jpegli source:

| Area | Verification and reference oracles |
| --- | --- |
| **DCT, SIMD, and numerical kernels** | Bit-exact parity between scalar and 4/8-lane SIMD paths (`Vector<float>`, `Vector128`/`256`/`512`, plus software fallback on `netstandard2.1` and `DOTNET_EnableHWIntrinsics=0`) across impulses, checkerboards, subnormals, signed zeros, and random blocks; double-precision cosine-DCT oracle; SHA-256 fixtures for complete JPEGs. |
| **Quantization and CICP tuning** | Pinned C++ table and adaptive-threshold fixtures for perceptual, standard Annex K (`UseStandardQuantizationTables`), extended 16-bit (`ForceBaseline = false`, `SOF1`), custom `QuantizationTables`, direct `Distance`, PQ/HLG `CicpTransferFunction` (`16`/`18`), and raw-profile `cicp` tag lookup. |
| **Sampling and smoothing** | Pinned C++ scalar fixtures and double-precision box-average oracles for 4:4:4, 4:2:2, 4:2:0, 4:4:0, 4:1:1, 4:1:0, 3×1, 3×2, 1×4, 2×4, dedicated 3-row (`1×3`, `2×3`) kernels, custom `SamplingFactors`, fractional ratios, subsampled luma, and 3×3 `SmoothingFactor` across block seams and odd edges. |
| **Input precision and color spaces** | `byte`, `ushort` (`* 257` byte-equivalence), and `float` (including `AllowOutOfRangeFloatSamples` coefficient preflight); `Grayscale`, `YCbCr`, `Rgb`, and `Xyb` output spaces; direct `YCbCr24`/`YCbCr` and `Xyb24`/`Xyb` inputs; pinned C++ fixtures and double-precision opsin/transfer oracles for sRGB-to-XYB. |
| **ICC profile generation and conversion** | Byte-for-byte SHA-256 parity against pinned C++ profile builders across all 288 named RGB/grayscale combinations, 48 gamma-2.2 profiles, 252 custom-chromaticity profiles, 4 rendering intents, Rec.2100 PQ/HLG Lab LUTs, and the 720-byte XYB profile; independent matrix/TRC/LUT oracles for `SourceIccProfile` conversion. |
| **Scan scripts, restarts, and entropy** | Independent JPEG marker and Huffman bitstream parser verifying coefficient reconstruction across baseline, `SequentialInterleaveComponents = false`, `ProgressiveLevel` (`0..3+`), spectral bands, per-band refinement (`0..10` bits), separate DC scans, custom `ScanScript`, `RestartInterval`, `RestartRows`, and 32,767-block EOB runs. |
| **Caching, rolling planes, and parallelism** | Byte-exact JPEG parity across contiguous planes, 32-row rolling planes, segmented/prefix caches, `MaxCacheBytes = 0`, token-budget exhaustion, and `MaxDegreeOfParallelism > 1` (parallel DCT rows, parallel baseline collection, and parallel entropy replay with and without restarts). |
| **Metadata and reconciliation** | Independent parsers for JFIF `APP0`, Exif TIFF (`II`/`MM`, thumbnails, offsets), standard/extended/auto-partitioned/selective XMP `APP1` (with MD5 links), ICC `APP2` (up to 255 chunks), IPTC `APP13`, `COM`, raw `MetadataMarkers`, and `JpegliMetadata.Reconcile`. |
| **Offline compression searches** | Exhaustive small-block dynamic-programming oracles, prepared MS-SSIM/PSNR/SSIM bit-parity checks, and end-to-end command tests for progressive scan, quantization-table, RDOQ, Huffman ordering, DC trellis, and color-preprocessing searches. |
