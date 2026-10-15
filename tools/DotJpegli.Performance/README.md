# Performance noninferiority evidence

This CI-only tool compares complete in-memory encode latency with the pinned
native jpegli encoder. It uses the shared Kodak24 hash manifest, eight shared
deterministic controls, distances 1/4, sampling 444/420 and progression 0/2:
256 cases. Both encoders use one worker, RGB8 to YCbCr, optimized Huffman tables,
adaptive quantization and baseline-compatible quantizers. Its results apply to
that corpus, configuration and CI host; they do not prove universal speed parity.

The checked-in policy requires each corpus/sampling/progression group to have
geometric mean managed/native median latency ratio <=1.05, and every case <=1.10.
Parallel managed results cannot satisfy this single-worker gate.
One stable matched case above the individual limit stops measurement with a
failed verdict and explicit incomplete coverage. Such a counterexample cannot
be reversed by later cases. A passing verdict always requires all 256 cases;
unmatched or unstable measurements cannot trigger this early failure.

Seven independent blocks alternate native/managed measurement order. Each block
warms both encoders with five untimed encodes. Tiered JIT compilation is disabled
in the runner runtime configuration. Native three-iteration calibration targets
50ms, clamped to 3..100 encodes; the same count applies to both encoders. Each
encode is timed individually and intervals are summed. The scope includes
encoder construction, configuration, memory output and finalization; it excludes
process startup, files, warmups, byte verification and native output-buffer free.
Managed collections during an encode remain in its measured latency; none are
forced or suppressed. Medians include every block; no outliers are removed.

Any block below 1ms total timed interval, a relative MAD above 10% in either
encoder, missing coverage, nonfinite timings, hash mismatch or unmatched JPEG
structure invalidates the result. Exit 0 means valid passing evidence, 1 means
valid failed latency bounds, and 2 means invalid evidence. Raw blocks, percentiles,
MAD, GC diagnostics, hashes, exact inputs/JPEGs, settings, native command metadata,
revision and CPU/OS/runtime/vector/affinity information are retained in the output.
Run identity and an incomplete summary are written before measurement; progress
is saved after every completed case so interrupted runs retain useful evidence.

The measured run requires a Release build and GitHub Actions:

```sh
dotnet tools/DotJpegli.Performance/bin/Release/net10.0/DotJpegli.Performance.dll \
  --native /absolute/path/jpegli-parity \
  --corpus /absolute/path/kodak-ppm \
  --output /absolute/path/empty-evidence-directory
```

Local validation is deliberately limited to build and synthetic self-test:

```sh
dotnet restore tools/DotJpegli.Performance --locked-mode
dotnet build tools/DotJpegli.Performance -c Release --no-restore
dotnet tools/DotJpegli.Performance/bin/Release/net10.0/DotJpegli.Performance.dll --self-test
```

The self-test exercises only policy, coverage and statistics using fabricated
samples. It does not encode images or read clocks and is not a benchmark.
