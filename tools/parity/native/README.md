# Pinned native reference

`jpegli-parity-native` is a CI validation tool that links the unmodified encoder and decoder from `google/jpegli` revision `031a0077f5799a6041004267fc12b956c1f52a20` (`jpegli-static` CMake target, with pinned Highway, skcms, and libjpeg-turbo submodules). It is never packaged with the managed library.

## Build and verify

Building requires Git, CMake, Ninja, a C/C++17 compiler, and PowerShell 7.6 or later:

```powershell
$build = @{
    SourceDirectory = '/absolute/temp/path/upstream'
    BuildDirectory = '/absolute/temp/path/native-build'
    Generator = 'Ninja'
}
./tools/parity/build-upstream.ps1 @build
```

To verify a cached build's pinned source identity and executable hash:

```powershell
./tools/parity/build-upstream.ps1 -VerifyOnly -BuildDirectory /absolute/temp/path/native-build
```

`provenance.json` records the upstream and submodule commits, compiler/CMake flags, executable SHA-256, and encode/decode settings.

## Commands

```text
jpegli-parity-native version
jpegli-parity-native encode INPUT.ppm OUTPUT.jpg DISTANCE 444|420 LEVEL OPTIMIZE_0|1 AQ_0|1
jpegli-parity-native decode INPUT.jpg OUTPUT.ppm
jpegli-parity-native benchmark INPUT.ppm DISTANCE 444|420 LEVEL WARMUPS ITERATIONS REPEATS [OUTPUT.jpg]
```

- **`encode`**: Reads binary P6 RGB8 (`maxval 255`) and encodes YCbCr JPEG with the specified Butteraugli distance, sampling (`444` or `420`), progression level (`0` or `2`), Huffman optimization (`0` or `1`), and adaptive quantization (`0` or `1`), using baseline quantizer bounds, no smoothing or restarts, and a standard JFIF 1.01 (`1x1` density) header.
- **`decode`**: Decodes any JPEG to binary P6 RGB8 using `JDCT_ISLOW` and fancy upsampling with no ICC conversion. Any warning or suspended decode fails the command.
- **`benchmark` (CI-only)**: Measures single-threaded in-memory encoding (`jpegli_mem_dest`) with AQ and optimized Huffman enabled. Requires `WARMUPS >= 5`. Each timed sample averages `ITERATIONS` individual `steady_clock` encode lifecycles (create, configure, encode, finish, destroy), excluding file I/O and byte-for-byte output verification. Emits a single JSON summary line with `samplesNsPerEncode`.

## Untimed native allocations

On Linux, `build-upstream.ps1` also builds `jpegli-allocation-native`, a separate executable that wraps `malloc`, `calloc`, `realloc`, `aligned_alloc`, `posix_memalign`, `memalign`, and C++ `new` via GNU linker `--wrap` flags (leaving `jpegli-parity-native` uninstrumented):

```text
jpegli-allocation-native allocation-self-test
jpegli-allocation-native allocations INPUT.ppm DISTANCE 444|420 LEVEL OUTPUT.jpg
```

- **`allocation-self-test`**: Verifies allocator wrapper routing on synthetic allocations without encoding or timing (safe to run locally).
- **`allocations` (CI-only)**: Runs 5 untimed warmups followed by 3 observed in-memory encodes, reporting total requested heap bytes per encode (including `realloc` requests and output buffer growth, excluding allocator metadata/slack).
