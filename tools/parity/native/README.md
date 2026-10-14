# Pinned native reference

This executable is a validation tool, outside the managed runtime library. It
links the unchanged encoder and decoder from google/jpegli revision
`031a0077f5799a6041004267fc12b956c1f52a20` through the upstream `jpegli-static`
CMake target. Both the build script and CMake reject a different or modified
tracked source checkout. The script initializes and verifies Highway, skcms and
libjpeg-turbo at the Git submodule revisions in that upstream commit.

Ubuntu CI requires Git, CMake, Ninja, a C/C++17 toolchain and PowerShell:

```powershell
pwsh tools/parity/build-upstream.ps1 `
    -SourceDirectory /absolute/temp/path/upstream `
    -BuildDirectory /absolute/temp/path/native-build `
    -Generator Ninja
```

The resulting `/absolute/temp/path/native-build/jpegli-parity-native` supports:

```text
jpegli-parity-native version
jpegli-parity-native encode INPUT.ppm OUTPUT.jpg DISTANCE 444|420 LEVEL OPTIMIZE_0|1 AQ_0|1
jpegli-parity-native decode INPUT.jpg OUTPUT.ppm
jpegli-parity-native benchmark INPUT.ppm DISTANCE 444|420 LEVEL WARMUPS ITERATIONS REPEATS [OUTPUT.jpg]
```

Input and decoded output are binary RGB P6 PPM with maxval 255. Encode uses
eight-bit RGB input and YCbCr output, explicit distance, baseline quantizer
limits, explicit sampling factors, upstream progression level and the requested
Huffman/adaptive settings. Smoothing and restart intervals are zero. JFIF 1.01
with aspect densities 1×1 matches the managed encoder's mandatory marker, so
full JPEG sizes include equivalent metadata. No ICC or other optional metadata
is added.

Decode always uses the same pinned decoder for both encoders, full-size RGB
eight-bit output, `JDCT_ISLOW` and fancy upsampling. It performs no ICC conversion.
Warnings or suspended decoding fail the tool. These explicit decoder settings
are recorded for reproducibility; they do not assert parity with another reader.

`provenance.json` records source and dependency revisions, compiler/CMake
settings, executable SHA-256 and the encode/decode contract. Full matched-corpus
quality/compression runs belong on CI; this build script runs no corpus,
performance test or benchmark.

The `benchmark` command is a CI-only, single-threaded in-process measurement.
It uses the same encoder configuration as `encode`, with optimized Huffman
coding and adaptive quantization enabled. It reads RGB input before timing and
uses `jpegli_mem_dest`, so process startup, input/file/console I/O and output
validation are excluded. Each timed interval covers a fresh encoder's complete
create/configure/encode/finish/destroy lifecycle, including destination
allocation and growth. The returned JPEG buffer is compared byte for byte with
the first warmup and freed after the interval. Timer call overhead is included;
one reported sample is the sum of per-encode `steady_clock` intervals divided
by `ITERATIONS`, rather than wall time around validation between iterations.

At least five warmups and positive iteration/repeat counts are required. The
matched performance runner calibrates a common iteration count from the native
encoder, then collects seven independent blocks in alternating encoder order.
The command prints one JSON object with the upstream pin, settings, dimensions,
counts, JPEG size, compiler/platform description and `samplesNsPerEncode`.
`deterministic` is emitted only after every warmup and timed output matches.
Optional `OUTPUT.jpg` retains the validated stream for structure/size checks;
writing it happens after all timing. Any encoder warning, suspended scanline,
changed output or output write failure makes the command fail. No benchmark
should be executed locally, including calibration or a smoke invocation.
