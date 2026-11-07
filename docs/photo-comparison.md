# Cosmic Cliffs encoder comparison

This is a single, untimed photographic illustration for the README. All eight
complete images and the lossless crop sheet are retained in
[`images/comparison/`](images/comparison/). The authoritative byte counts, full
precision SSIMULACRA2 scores and SHA256 hashes are in
[`comparison.json`](images/comparison/comparison.json).

## Photograph and permission

**NASA, ESA, CSA, and STScI.**
[NIRCam Image of the “Cosmic Cliffs” in Carina](https://esawebb.org/images/weic2205a/),
released July 12, 2022. This is an infrared observation by the James Webb Space
Telescope, displayed in assigned visible colours, rather than a natural-light
landscape or a generated illustration.

ESA/Webb distributes its images under
[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/), with the full credit
required beside each image; see its [image-use policy](https://esawebb.org/copyright/).
This license applies to the source and derived photograph assets separately from
the repository's BSD code license. The attribution is printed next to the README
photograph and in the crop sheet itself.

The [official publication TIFF](https://esawebb.org/media/archives/images/publicationtiff/weic2205a.tif)
is **4000 × 2317**, **11,715,498 bytes**, SHA256
`145becf8e9ca2c35473d102da19a34786185bc5da947367db75dc78a0c323e71`.
Its embedded profile identifies sRGB. Pillow 13.0.0 resizes its RGB8 samples
once with Lanczos to **2400 × 1390**. The PNG and temporary packed RGB/P6 PPM
inputs contain exactly the same samples. Metadata and the redundant sRGB ICC
profile are omitted. No sharpening, denoising or per-encoder resizing is applied.
The source TIFF is downloadable upstream; the common input PNG is committed.

## Outputs and interpretation

| Encoder | Complete bytes | KiB | SSIMULACRA2 |
| --- | ---: | ---: | ---: |
| [Source PNG](images/comparison/source.png) | 4,437,354 | 4333.4 | 100.00 |
| [DotJpegli 5.0.0](images/comparison/dotjpegli.jpg) | 723,724 | 706.8 | 80.49 |
| [Google jpegli](images/comparison/google-jpegli.jpg) | 723,938 | 707.0 | 80.49 |
| [mozjpeg 4.1.1](images/comparison/mozjpeg.jpg) | 726,330 | 709.3 | 78.92 |
| [libjpeg-turbo 3.2.0](images/comparison/libjpeg-turbo.jpg) | 822,647 | 803.4 | 80.41 |
| [WebP 1.6.0](images/comparison/webp.webp) | 640,770 | 625.8 | 82.28 |
| [libavif 1.4.2 / libaom 3.14.1](images/comparison/avif.avif) | 837,683 | 818.0 | 86.85 |
| [libjxl 0.12.0](images/comparison/jpeg-xl.jxl) | 759,353 | 741.6 | 88.02 |

Sizes are actual file lengths, including format overhead; **1 KiB = 1024 bytes**.
All JPEGs use optimized Huffman coding, progressive encoding and 4:2:0 chroma
subsampling. DotJpegli and Google jpegli share distance 1, progressive preset 2,
adaptive quantization and baseline-range quantizers. DotJpegli uses one worker
and YCbCr output. Its exact settings are in
[`encode-readme-photo.cs`](../tools/encode-readme-photo.cs); this helper uses the
library directly because the public CLI's default progressive script differs
from preset 2. DotJpegli is 214 bytes smaller than Google jpegli here; their
hashes and decoded pixels are not identical.

mozjpeg uses quality 90, its default trellis, `-sample 2x2 -progressive -optimize`.
Pillow invokes libjpeg-turbo with quality 90, subsampling 2, progressive and
optimized Huffman coding, and WebP with quality 90 and method 6. AVIF uses
libaom quality 90, speed 6, one worker, 8-bit 4:2:0, full range and CICP
`1/13/6` (BT.709/sRGB primaries, sRGB transfer, BT.601 matrix). JPEG XL uses
distance 1, effort 7 and one worker.

These selected settings do not match quality between different encoders.
Quality 90 and distance 1 are not universal visual-quality units. The scores
help readers see that the sizes represent different amounts of distortion;
they are not a human preference study or a ranking across other photographs.
AVIF and JPEG XL have higher scores at these settings. This illustration makes
no claim about speed, native/managed allocation parity or general compression
parity. Those have separate CI protocols.

The official libjxl 0.12.0 `ssimulacra2` executable compares the full common
source PNG with each full decoded RGB PNG. JPEGs use the same Pillow/libjpeg-turbo
decoder; WebP uses Pillow/libwebp, AVIF uses avifdec/dav1d, and JPEG XL uses djxl.
The [libjxl implementation](https://github.com/libjxl/libjxl/blob/v0.12.0/tools/ssimulacra2.cc)
defines the metric. Scores are independent of the presentation crop. The
lossless contact sheet uses rectangle `(850, 420)..(1330, 720)` without scaling
the cropped pixels. A viewer may scale the sheet to fit its window.

Google's separate [2024 human-rater study](https://arxiv.org/abs/2403.18589)
compared jpegli, mozjpeg and libjpeg-turbo across 49 CID22 images. Its results
and settings are distinct from this repository's illustration.

## Reproduce the assets

The generator is [`tools/generate-readme-comparison.py`](../tools/generate-readme-comparison.py).
It verifies the source TIFF hash, uses Pillow **13.0.0**, executes each encode
without timing it, checks decoded dimensions, computes the metric and produces
the contact sheet and JSON manifest. The input source, helper and output
hashes, actual managed assembly hash and external executable hashes are
retained in the manifest. The retained encode used managed source revision
`40a3074c61fbb052e452a82faf6d74616d060060`; the helper source is additionally
identified by hash. No encoder source changes accompany this example.

Use the repository's pinned .NET SDK and these upstream tools:

- **Google jpegli:** unmodified revision
  [`031a0077f5799a6041004267fc12b956c1f52a20`](https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20),
  built with the repository's `tools/parity/build-upstream.ps1`. The `encode`
  subcommand of `jpegli-parity-native` calls the official C++ library directly.
- **mozjpeg:** official
  [`v4.1.1` source archive](https://github.com/mozilla/mozjpeg/archive/refs/tags/v4.1.1.zip),
  built as the static `cjpeg` executable, Release, with `WITH_SIMD=OFF`,
  `ENABLE_SHARED=OFF` and `PNG_SUPPORTED=OFF`. The retained output used MSVC
  19.51.36260.0 on ARM64. Recent CMake needs
  `-DCMAKE_POLICY_VERSION_MINIMUM=3.5` for this source release.
- **libjpeg-turbo 3.2.0 and libwebp 1.6.0:** bundled in the official Pillow
  13.0.0 Windows ARM64 wheel. Their actual versions are queried by the generator.
- **JPEG XL and SSIMULACRA2:** official
  [`v0.12.0` Windows x64 static tools](https://github.com/libjxl/libjxl/releases/download/v0.12.0/jxl-x64-windows-static.zip),
  whose version banner identifies commit `4128790`. `cjxl`, `djxl` and
  `ssimulacra2` come from that one archive.
- **AVIF:** official
  [`v1.4.2` Windows tools](https://github.com/AOMediaCodec/libavif/releases/download/v1.4.2/windows-artifacts.zip),
  with libaom 3.14.1, dav1d 1.5.3 and libyuv 1924 reported by the release binary.

For example, with tools installed at your chosen paths:

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

All executable arguments are filesystem paths. The scratch directory retains
full command stdout/stderr, PPM/RGB inputs and decoded PNGs for inspection;
those large temporary files are not committed. Exact reproduction requires
the recorded input, tool and library versions; compilers and architectures
can change rounding and compressed bytes. Build the helper and encoder only
in Release. Full benchmark suites remain CI-only.
