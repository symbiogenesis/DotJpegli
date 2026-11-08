<div align="center">

# DotJpegli

**Pure managed JPEG encoder for .NET.**

[![CI](https://github.com/symbiogenesis/DotJpegli/actions/workflows/ci.yml/badge.svg?branch=main&event=push)](https://github.com/symbiogenesis/DotJpegli/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/DotJpegli.svg?logo=nuget)](https://www.nuget.org/packages/DotJpegli)
[![.NET Standard 2.1 · .NET 10 · .NET 11](https://img.shields.io/badge/.NET-Standard_2.1_%7C_10_%7C_11-512BD4)](https://github.com/symbiogenesis/DotJpegli/blob/main/src/DotJpegli/DotJpegli.csproj)
[![License: BSD-3-Clause](https://img.shields.io/badge/License-BSD--3--Clause-blue.svg)](https://github.com/symbiogenesis/DotJpegli/blob/main/LICENSE)

[Photograph comparison](#photograph-comparison) · [Quick start](#quick-start) · [Features](#features) · [Performance](#performance) · [Documentation](#documentation)

</div>

DotJpegli is a JPEG encoder for .NET based on
[Google Jpegli](https://github.com/google/jpegli). It saves color or grayscale
images as standard or progressive JPEGs, with options for image quality, color
profiles, and metadata.

It is written entirely in C# with no native libraries, unsafe code, or extra
packages required.

## Photograph comparison

Fine clouds of dust, rich color, and thousands of stars make the James Webb
Space Telescope's **Cosmic Cliffs** a good test for image compression. Click the
**2400 × 1390** source photograph to view it at full size:

[![James Webb Space Telescope photograph of the Cosmic Cliffs: glowing amber clouds, blue starlight and intricate filaments in the Carina Nebula.](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/source.png)](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/source.png)

**Photograph: NASA, ESA, CSA, and STScI — [ESA/Webb source](https://esawebb.org/images/weic2205a/),
[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).** Resized from the
original image; the comparison files are re-encoded and the detail panels are
cropped.

Each encoder compressed the same source image using typical high-quality
settings. Higher quality scores mean the result looks closer to the original:

| Encoder | File size | Quality score |
| --- | ---: | ---: |
| [Original PNG](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/source.png) | 4,333.4 KiB | 100.00 |
| **[DotJpegli 5.0.0](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/dotjpegli.jpg)** | **706.8 KiB** | **80.49** |
| [Google Jpegli (C++)](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/google-jpegli.jpg) | 707.0 KiB | 80.49 |
| [mozjpeg 4.1.1](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/mozjpeg.jpg) | 709.3 KiB | 78.92 |
| [libjpeg-turbo 3.2.0](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/libjpeg-turbo.jpg) | 803.4 KiB | 80.41 |
| [WebP 1.6.0](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/webp.webp) | 625.8 KiB | 82.28 |
| [AVIF (libavif 1.4.2)](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/avif.avif) | 818.0 KiB | 86.85 |
| [JPEG XL (libjxl 0.12.0)](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/jpeg-xl.jxl) | 741.6 KiB | 88.02 |

Because each format uses its own quality scale, these files are not tuned to
match exact visual quality or file size. The image below shows the same
**480 × 300** section from each version at full size so you can compare stars,
dust clouds, and color gradients:

[![Pixel-for-pixel source and encoded detail crops for DotJpegli, Google jpegli, mozjpeg, libjpeg-turbo, WebP, AVIF and JPEG XL, each labelled with its complete file size. Credit: NASA, ESA, CSA, and STScI; CC BY 4.0.](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/detail-comparison.png)](https://raw.githubusercontent.com/symbiogenesis/DotJpegli/main/docs/images/comparison/detail-comparison.png)

See [photograph comparison details](docs/photo-comparison.md) for the exact
settings, byte counts, and steps to reproduce these files. Google also published
a [study comparing Jpegli, mozjpeg, and libjpeg-turbo](https://arxiv.org/abs/2403.18589).

## Features

| Feature | What it does |
| --- | --- |
| **Standard and progressive JPEG** | Save standard JPEGs or progressive JPEGs that load in stages, with simple quality controls. |
| **Flexible pixel input** | Encode 8-bit, 16-bit, or floating-point pixel data in RGB, BGR, grayscale, and other color formats. |
| **Color profiles and metadata** | Embed or convert ICC color profiles, and read, edit, or write Exif, XMP, IPTC, and comment metadata. |
| **Streams and multi-threading** | Write to byte arrays or streams, cancel long operations, and optionally use multiple CPU cores. |
| **Self-contained** | Runs everywhere .NET runs (.NET Standard 2.1, .NET 10, and .NET 11) with no native dependencies. |

## Command-line tool

To convert images from the terminal without writing code, download `dotjpegli`
from [Releases](https://github.com/symbiogenesis/DotJpegli/releases). Installers
and standalone downloads are available for Windows, Linux, and macOS, and do not
require .NET to be installed.

```powershell
dotjpegli encode input.png output.jpg --quality 90 --progressive
```

See the [CLI guide](docs/cli.md) for installation and command options.

## Quick start

### Install the package

```sh
dotnet add package DotJpegli --version 5.0.0
```

### Encode an image

Pass an RGB pixel array and its width and height to the encoder:

```csharp
using DotJpegli;

byte[] rgbPixels = [255, 0, 0]; // One red pixel (R, G, B).

byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width: 1, height: 1, options: new()
{
    Quality = 85
});

File.WriteAllBytes("output.jpg", jpeg);
```

By default, `Encode` writes a standard JPEG at quality 85. Transparency is
ignored, so blend transparent images onto a background before encoding. See the
[encoding guide](docs/encoding.md) for other pixel formats and input options.

### Write a progressive JPEG to a stream

You can also write directly to a file or stream and turn on progressive encoding:

```csharp
using FileStream output = File.Create("progressive.jpg");

JpegliEncoder.Encode(output, rgbPixels, width: 1, height: 1, options: new()
{
    Quality = 85,
    Progressive = true
});
```

Writing to a stream avoids creating a separate byte array in memory for the
finished file. See [compression options](docs/compression.md) for more ways to
adjust quality and file size.

## Performance

These charts compare DotJpegli with **Google's C++ Jpegli encoder** across 24
standard test photographs.

### Encoding speed

Across the 24 photographs, DotJpegli took **37–42% less time** than Google's C++
encoder for progressive JPEGs, and **14–15% less time** for standard JPEGs.

![Chart comparing encoding speed between DotJpegli and Google's C++ Jpegli encoder across photographs and test patterns.](docs/images/performance/wall-time.svg)

### Memory allocated per image

DotJpegli reuses internal buffers, allocating **79–109 KiB** of memory per image
(including the finished JPEG), compared with **1.9–4.3 MiB** requested per image
by Google's C++ encoder.

![Chart comparing memory allocated per image by DotJpegli and Google's C++ Jpegli encoder.](docs/images/performance/allocations.svg)

On the synthetic test patterns shown alongside the photographs, DotJpegli is
also faster on average, though the C++ encoder is still faster on some simple
patterns like smooth gradients. See [performance details](docs/performance-parity.md)
for the full benchmark results and test setup.

## Documentation

| Guide | Topics |
| --- | --- |
| [Encoding](docs/encoding.md) | Pixel formats, color spaces, streams, and input limits. |
| [Profiles and metadata](docs/metadata.md) | Color profiles (ICC), Exif, XMP, IPTC, and metadata editing. |
| [Compression and scans](docs/compression.md) | Quality settings, progressive JPEGs, multi-threading, and memory limits. |
| [Command-line encoder](docs/cli.md) | Command-line options, standalone downloads, and installers. |
| [Quantization](docs/quantization.md) | Custom compression tables and HDR tuning. |
| [Photograph comparison](docs/photo-comparison.md) | Source photograph, encoder settings, file sizes, and reproduction steps. |
| [Development and validation](docs/development.md) | Building from source, project design, and running tests. |
| [Publishing releases](docs/publishing.md) | How NuGet packages and GitHub releases are published. |
| [Quality and compression parity](docs/quality-compression-parity.md) | How image quality and file sizes compare with Google's C++ Jpegli. |
| [Performance parity](docs/performance-parity.md) | How encoding speed and memory use are measured. |

## Good to know

- **Encoding only:** DotJpegli creates JPEG files; it does not decode (read)
  existing JPEGs.
- **Standard 8-bit JPEG:** Like all standard JPEGs, the output is 8-bit and uses
  lossy compression—even at quality 100 or when starting from 16-bit or
  floating-point pixels.
- **Quality scale:** Jpegli's quality numbers do not match `libjpeg`
  one-for-one, so the same number (such as 85 or 90) may produce a different
  file size or look than other JPEG encoders.
- **Comparison with Google's C++ encoder:** DotJpegli is tested to match
  Google's C++ Jpegli closely in
  [image quality and file size](docs/quality-compression-parity.md), though the
  two encoders do not always produce identical files byte-for-byte.

## License

Copyright (c) 2026 Edward Miller. Licensed under the
[BSD 3-Clause License](https://github.com/symbiogenesis/DotJpegli/blob/main/LICENSE).
See [Third-Party Notices](https://github.com/symbiogenesis/DotJpegli/blob/main/THIRD-PARTY-NOTICES.md)
for Google Jpegli license and attribution details.

The Cosmic Cliffs photograph and comparison crops are licensed separately under
[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/), with full credit and
source links in the comparison above.
