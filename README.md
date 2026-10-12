# DotJpegli

A fully managed C# JPEG encoder targeting .NET 10. This is an experimental incremental
port of [Google Jpegli](https://github.com/google/jpegli), with unsafe compilation
disabled and no native binaries, P/Invoke, pointer code, `Unsafe` APIs, or runtime
NuGet dependencies.

The source is pinned to
[`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20),
retrieved on October 12, 2026. This port uses Google's BSD-licensed C++
implementation directly.

## Encoding

```csharp
using DotJpegli;

// Gamma-encoded eight-bit sRGB pixels, interleaved R, G, B.
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    Subsampling = ChromaSubsampling.Quarter,
    OptimizeHuffmanTables = true,
    AdaptiveQuantization = true
});
File.WriteAllBytes("output.jpg", jpeg);
```

`PixelLayout` also supports BGR24, RGBA32, BGRA32 and Gray8. Alpha is ignored;
composite translucent pixels before encoding. Use `stride` for row padding. The
last row needs only its pixel bytes, without trailing padding. Negative strides
are not supported. The input must contain every row; extra trailing bytes are
allowed.

The stream overload takes a writable stream as its first argument. It leaves
the stream open and does not seek. Cancellation and I/O failures may leave a
partial JPEG. Pass a `CancellationToken` for cancellation between MCUs. Dimensions
must be 1..65535 and fit `MaxPixels` (100 million by default).

Spatial adaptive quantization is enabled by default. It analyzes luma texture,
brightness and neighboring regions to adjust coefficient zeroing thresholds.
Set `AdaptiveQuantization = false` to use the original uniform quantization path;
this preserves the initial managed encoder's output for the same options.

Huffman optimization makes two passes over the supplied pixels, keeping only
block buffers, bounded rows of spatial-analysis data and symbol histograms
rather than a full image of coefficients or an image-sized adaptive map.
Keep the input unchanged until encoding returns. The byte-array overload also
holds the encoded JPEG in memory; use the stream overload for larger outputs.

## Port status

Implemented: baseline sequential JPEG/JFIF; RGB and grayscale input; 4:4:4,
4:2:2 and 4:2:0 box sampling with edge replication; recursive scalar DCT; Jpegli
quality-to-distance and perceptual Y/Cb/Cr quantization tables; scalar adaptive
quantization with local differences, fuzzy erosion, masking, high-frequency and
gamma modulation, quality dampening and frequency-specific coefficient thresholds;
optimized length-limited canonical Huffman coding;
byte stuffing and marker output.

Pending: progressive scans, upstream downsampling filters and empty dummy-block
coding at MCU edges, XYB and ICC profiles, higher precision input,
metadata/restart options, SIMD/parallel paths, and decoder functionality.
The managed DCT uses standard JPEG coefficient units; upstream's DCT output is
one eighth of those units. This scaling adaptation is accounted for during
quantization.

This version produces valid JPEGs but does **not** claim byte-identical output,
compression parity, or speed parity with Jpegli. Quality 100 is still lossy.
Jpegli quality is not interchangeable with libjpeg's quality scale. Google's
[comparison study](https://arxiv.org/abs/2403.18589) supports advantages over
MozJPEG and libjpeg-turbo in its tested conditions, rather than a universal
"smallest JPEG encoder" ranking.

## Validation

```text
dotnet build tests/DotJpegli.Tests/DotJpegli.Tests.csproj -c Release
dotnet run --project tests/DotJpegli.Tests -c Release --no-build -- -class DotJpegli.Tests.EncoderAdaptiveQuantizationTests
```

The second command is a focused local selection. Full suites run only in the
GitHub Actions workflow, on Windows and Linux; benchmarks also belong on CI and
none are included yet. Do not batch local selections to approximate a full suite.
Tests use xUnit and a separate StbImageSharp decoder; these packages are test-only
and are not part of the encoder or its package dependency graph.

Numerical development probes checked the DCT against an independent cosine
definition. Compiled C++ reference probes matched 120 color and grayscale quantization tables
(7,680 entries); selected native-reference tables are retained as regression
fixtures. Adaptive-analysis and coefficient regressions also use independently compiled
C++ scalar fixtures from the pinned source, including edge padding and saturated
quantization tables. These sampled numerical checks do not establish full encoder
parity. Future parity work should encode the same corpus with this pinned C++ revision and
compare decoded quality at matched sizes. Execute performance benchmarks only
on CI.

`Directory.Build.targets` rejects `AllowUnsafeBlocks=true`, including a command
line override. Library and tests compile with warnings treated as errors.

## License

BSD-3-Clause. See [LICENSE](LICENSE), [LICENSE.jpegli](LICENSE.jpegli) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for upstream provenance and notices.
