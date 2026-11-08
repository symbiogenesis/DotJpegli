# Cosmic Cliffs encoder comparison

[Documentation](README.md)

This page documents the single-image visual comparison featured in the root [README](../README.md). All eight full-resolution images, the lossless detail crop sheet, exact byte counts, SSIMULACRA2 scores, and SHA-256 hashes are stored in [`images/comparison/`](images/comparison/) and [`comparison.json`](images/comparison/comparison.json).

## Photograph and license

**Credit: NASA, ESA, CSA, and STScI.**
[NIRCam Image of the "Cosmic Cliffs" in Carina](https://esawebb.org/images/weic2205a/) (released July 12, 2022).

ESA/Webb distributes this image under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/); see the [ESA/Webb copyright policy](https://esawebb.org/copyright/). This license applies to the photograph assets in `docs/images/comparison/` separately from DotJpegli's BSD-3-Clause code license.

- **Source file**: [Official publication TIFF](https://esawebb.org/media/archives/images/publicationtiff/weic2205a.tif) (`4000 x 2317`, `11,715,498` bytes, SHA-256 `145becf8e9ca2c35473d102da19a34786185bc5da947367db75dc78a0c323e71`).
- **Preparation**: Resized once in sRGB RGB8 to `2400 x 1390` using Pillow 13.0.0 Lanczos resampling, with metadata and the redundant sRGB ICC profile stripped. Every encoder receives the exact same `2400 x 1390` RGB8 samples.

## Outputs and settings

| Encoder | Complete bytes | KiB | SSIMULACRA2 | Settings |
| --- | ---: | ---: | ---: | --- |
| [Source PNG](images/comparison/source.png) | 4,437,354 | 4333.4 | 100.00 | Lossless `2400 x 1390` RGB8 input |
| [DotJpegli 5.0.0](images/comparison/dotjpegli.jpg) | 723,724 | 706.8 | 80.49 | Distance 1, 4:2:0, progressive preset 2, AQ, optimized Huffman, 1 worker ([`encode-readme-photo.cs`](../tools/encode-readme-photo.cs)) |
| [Google jpegli](images/comparison/google-jpegli.jpg) | 723,938 | 707.0 | 80.49 | Distance 1, 4:2:0, progressive preset 2, AQ, optimized Huffman |
| [mozjpeg 4.1.1](images/comparison/mozjpeg.jpg) | 726,330 | 709.3 | 78.92 | Quality 90, `-sample 2x2 -progressive -optimize`, default trellis |
| [libjpeg-turbo 3.2.0](images/comparison/libjpeg-turbo.jpg) | 822,647 | 803.4 | 80.41 | Quality 90, 4:2:0 (`subsampling=2`), progressive, optimized Huffman |
| [WebP 1.6.0](images/comparison/webp.webp) | 640,770 | 625.8 | 82.28 | Quality 90, method 6 |
| [libavif 1.4.2 / libaom 3.14.1](images/comparison/avif.avif) | 837,683 | 818.0 | 86.85 | Quality 90, speed 6, 1 worker, 8-bit 4:2:0, full range, CICP `1/13/6` |
| [libjxl 0.12.0](images/comparison/jpeg-xl.jxl) | 759,353 | 741.6 | 88.02 | Distance 1, effort 7, 1 worker |

### Interpretation notes

- **File sizes**: Complete file lengths in bytes (`1 KiB = 1024 bytes`).
- **Quality scales**: Nominal settings (`Quality 90` vs `Distance 1`) are not matched for equal distortion across codecs. SSIMULACRA2 scores are computed on the full `2400 x 1390` decoded images using libjxl 0.12.0's [`ssimulacra2`](https://github.com/libjxl/libjxl/blob/v0.12.0/tools/ssimulacra2.cc) tool (higher is closer to the source).
- **Detail crop sheet**: [`detail-comparison.png`](images/comparison/detail-comparison.png) shows the exact `480 x 300` pixel crop `(850, 420)..(1330, 720)` from each decoded image at 1:1 pixel scale.
- **External study**: For a multi-image perceptual study of Jpegli, see Google's [2024 human-rater paper](https://arxiv.org/abs/2403.18589) across 49 CID22 images.

## Reproduce the assets

[`tools/generate-readme-comparison.py`](../tools/generate-readme-comparison.py) verifies the source TIFF hash, runs each encoder (untimed), computes SSIMULACRA2, and writes the crop sheet and `comparison.json`. The committed assets were generated at revision `40a3074c61fbb052e452a82faf6d74616d060060` using:

- **Google jpegli**: Unmodified commit [`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20) built via `tools/parity/build-upstream.ps1`.
- **mozjpeg**: [`v4.1.1`](https://github.com/mozilla/mozjpeg/archive/refs/tags/v4.1.1.zip) static `cjpeg` built in Release (`WITH_SIMD=OFF`, `ENABLE_SHARED=OFF`, `PNG_SUPPORTED=OFF`, `-DCMAKE_POLICY_VERSION_MINIMUM=3.5`).
- **libjpeg-turbo 3.2.0 & libwebp 1.6.0**: Bundled in the Pillow 13.0.0 wheel.
- **JPEG XL & SSIMULACRA2**: [`v0.12.0` Windows x64 static tools](https://github.com/libjxl/libjxl/releases/download/v0.12.0/jxl-x64-windows-static.zip) (`cjxl`, `djxl`, `ssimulacra2`).
- **AVIF**: [`v1.4.2` Windows tools](https://github.com/AOMediaCodec/libavif/releases/download/v1.4.2/windows-artifacts.zip) (`avifenc`, `avifdec`).

Example invocation:

```powershell
python -m pip install --target .artifacts/photo-python Pillow==13.0.0
$env:PYTHONPATH = (Resolve-Path .artifacts/photo-python).Path
$dotnetPath = (Get-Command dotnet.exe).Source
Invoke-WebRequest https://esawebb.org/media/archives/images/publicationtiff/weic2205a.tif -OutFile .artifacts/cosmic-cliffs.tif
python tools/generate-readme-comparison.py `
  --source-tiff .artifacts/cosmic-cliffs.tif `
  --output .artifacts/reproduced-comparison --scratch .artifacts/photo-scratch `
  --dotnet $dotnetPath --dotjpegli tools/encode-readme-photo.cs `
  --jpegli /path/to/jpegli-parity-native.exe --mozjpeg /path/to/cjpeg-static.exe `
  --cjxl /path/to/cjxl.exe --djxl /path/to/djxl.exe `
  --avifenc /path/to/avifenc.exe --avifdec /path/to/avifdec.exe `
  --ssimulacra2 /path/to/ssimulacra2.exe --revision YOUR_SOURCE_COMMIT
```
