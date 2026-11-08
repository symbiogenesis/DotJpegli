# Licensing and third-party notices

## DotJpegli

The managed port and original DotJpegli code are copyright (c) 2026 Edward Miller,
licensed under [BSD-3-Clause](LICENSE). The main license also retains the upstream
copyright notice for adapted portions.

## README photograph and derived comparison images

The photograph files in `docs/images/comparison/` are separately licensed under
[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/),
as stated in the [ESA/Webb image-use policy](https://esawebb.org/copyright/).
They are not covered by the BSD license for DotJpegli's code.

- Title: NIRCam Image of the “Cosmic Cliffs” in Carina (`weic2205a`).
- Credit: **NASA, ESA, CSA, and STScI**.
- Source and caption: https://esawebb.org/images/weic2205a/
- Official lossless 4000 × 2317 TIFF: https://esawebb.org/media/archives/images/publicationtiff/weic2205a.tif
- Changes: resized to 2400 × 1390; metadata removed; encoded in PNG, JPEG, WebP,
  AVIF and JPEG XL; corresponding details cropped and labelled in a lossless PNG
  comparison sheet. No endorsement is implied.

The [comparison record](docs/photo-comparison.md) and
[machine-readable provenance](docs/images/comparison/comparison.json) retain the
source hash, settings, output hashes and exact credit.

## Jpegli

DotJpegli adapts selected encoder routines and supporting data directly from Google's Jpegli source,
distributed under BSD-3-Clause:

- Repository: https://github.com/google/jpegli
- Revision: `031a0077f5799a6041004267fc12b956c1f52a20`
- Retrieved: October 12, 2026
- License: https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/LICENSE

> Copyright (c) the JPEG XL Project Authors.
> All rights reserved.
>
> Redistribution and use in source and binary forms, with or without
> modification, are permitted provided that the following conditions are met:
>
> 1. Redistributions of source code must retain the above copyright notice, this
>    list of conditions and the following disclaimer.
>
> 2. Redistributions in binary form must reproduce the above copyright notice,
>    this list of conditions and the following disclaimer in the documentation
>    and/or other materials provided with the distribution.
>
> 3. Neither the name of the copyright holder nor the names of its
>    contributors may be used to endorse or promote products derived from
>    this software without specific prior written permission.
>
> THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
> AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
> IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
> DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
> FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
> DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
> SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
> CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
> OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
> OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The license text above is reproduced unchanged from the
[upstream license at the pinned revision](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/LICENSE).
The upstream
[author list](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/AUTHORS)
includes Google, Cloudinary and individual contributors. Its
[contribution guide](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/CONTRIBUTING.md)
uses "the Jpegli Project Authors" for new file headers and describes Google's role
in receiving contributor license agreements. The repository-wide license at this
revision retains "the JPEG XL Project Authors"; we preserve that supplied notice.

| Managed file | Authoritative upstream source |
| --- | --- |
| `src/DotJpegli/Internal/ForwardDct.cs` | `lib/jpegli/dct-inl.h`: recursive DCT and transform ordering |
| `src/DotJpegli/Internal/ColorConversion.cs` | `lib/jpegli/color_transform.cc`: RGB to YCbCr conversion |
| `src/DotJpegli/Internal/XybColorConversion.cs` | `lib/extras/xyb_transform.cc`, `lib/cms/opsin_params.h`, `lib/cms/transfer_functions-inl.h`: sRGB transfer, opsin absorbance and scaled XYB |
| `src/DotJpegli/Internal/PixelSource.cs` | `lib/jpegli/input.cc`, `color_transform.cc`: 16-bit and float input scaling, direct YCbCr input |
| `src/DotJpegli/Internal/IccProfile.cs`, `IccColorTransform.cs` | `lib/extras/enc/jpegli.cc`: `FindCICPTag`, raw-profile CICP lookup and source ICC color transforms |
| `src/DotJpegli/Internal/XybIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/opsin_params.h`: fixed perceptual scaled XYB ICC profile (`CC0` upstream profile copyright tag) |
| `src/DotJpegli/Internal/BuiltInIccColorEncoding.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`: named white points, primaries, adapted colorants, luminance coefficients, descriptions and CICP eligibility |
| `src/DotJpegli/Internal/CustomIccColorEncoding.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`, `lib/base/matrix_ops.h`: custom CIE xy descriptions, primary matrix construction/inversion and Bradford adaptation |
| `src/DotJpegli/Internal/RgbIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`: named and custom RGB ICC profiles with parametric or 64-entry PQ/HLG curves (`CC0` upstream profile copyright tag) |
| `src/DotJpegli/Internal/HdrIccProfile.cs`, `HdrToneMapping.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/transfer_functions.h`, `lib/cms/tone_mapping.h`: PQ/HLG transfer functions, BT.2408 tone mapping, HLG OOTF, gamut mapping and 9x9x9 Lab CLUT ICC profiles (`CC0` upstream profile copyright tag) |
| `src/DotJpegli/Internal/GrayscaleIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`: grayscale ICC profiles across named/custom white points, transfers and rendering intents (`CC0` upstream profile copyright tag) |
| `src/DotJpegli/Internal/GammaTransferFunction.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/base/common.h`: generic gamma encoding exponent, six-significant-digit description formatting and float decoding parameter |
| `src/DotJpegli/Internal/IccProfileWriter.cs` | `lib/cms/jpegli_cms_internal.h`: shared deterministic ICC header, Bradford adaptation, tag serialization and profile ID |
| `src/DotJpegli/Internal/Downsampling.cs`, `SmoothedDownsampling.cs` | `lib/jpegli/downsample.cc`, `encode.cc`: 2x1, 2x2, 1x2 and 4x1 box filters, input smoothing and edge padding |
| `src/DotJpegli/JpegliEncoder.cs` | `lib/jpegli/encode_streaming.cc`, `encode.cc`: empty MCU block tokens, DC predictor continuity and default progressive scan presets |
| `src/DotJpegli/Internal/JpegScanScript.cs` | `lib/jpegli/encode.cc`: `ValidateScanScript`, spectral/refinement progression rules and AC context limits |
| `src/DotJpegli/Internal/Quantization.cs`, `XybQuantization.cs` | `lib/jpegli/quant.cc`, `encode.cc`, `dct-inl.h`: quantization matrices, distance scaling and zero-bias thresholding |
| `src/DotJpegli/Internal/AdaptiveQuantization.cs` | `lib/jpegli/adaptive_quantization.cc`: spatial analysis, fuzzy erosion and perceptual modulation |
| `src/DotJpegli/Internal/HuffmanTable.cs`, `HuffmanCodingPlan.cs` | `lib/jpegli/huffman.cc`, `entropy_coding.cc`: standard tables, tree construction, canonical coding and cost-based histogram clustering |
| `src/DotJpegli/Internal/EntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`: baseline coefficient tokenization |
| `src/DotJpegli/Internal/ProgressiveEntropyEncoder.cs`, `ProgressiveAcCollector.cs`, `ProgressiveAcRefinementState.cs` | `lib/jpegli/entropy_coding-inl.h`, `entropy_coding.cc`: progressive DC/AC point transforms, spectral-range tokenization, EOB-run aggregation and refinement tokens |
| `src/DotJpegli/Internal/BitWriter.cs` | `lib/jpegli/bit_writer.h`, `bit_writer.cc`: bit packing, byte stuffing and padding |
| `src/DotJpegli/Internal/JpegConstants.cs` | `lib/jpegli/common_internal.h`: natural/zigzag order tables |

The public managed API, image traversal and marker writer
are implemented for this port. No upstream C++ binary is linked.

## SorterHunter selection-network topology

The nine-input selection network in `AdaptiveQuantization.SelectFourMinima`
prunes Bert Dobbelaere's seven-layer sorting-network topology to its first four
outputs. The C# implementation and pruning are maintained in this repository.

- Topology: https://bertdobbelaere.github.io/sorting_networks.html#N9L25D7
- Repository: https://github.com/bertdobbelaere/SorterHunter
- License: https://github.com/bertdobbelaere/SorterHunter/blob/master/LICENSE

> MIT License
>
> Copyright (c) 2017 bertdobbelaere
>
> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all
> copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.

## CLI dependencies

The standalone CLI includes [System.CommandLine](https://github.com/dotnet/command-line-api)
2.0.14 (MIT), [StbImageSharp](https://github.com/StbSharp/StbImageSharp)
2.30.16 (Unlicense or MIT, distributed here under the Unlicense), and portions of
the .NET runtime (MIT). These are not dependencies of the DotJpegli library or
NuGet package. Native distributions also include `LICENSE.dotnet.txt` and
`THIRD-PARTY-NOTICES.dotnet.txt` from the exact runtime packs used to build them.

### System.CommandLine and .NET runtime: MIT

Copyright (c) .NET Foundation and Contributors. All rights reserved.
Copyright (c) Microsoft Corporation. All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

### StbImageSharp: Unlicense

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <https://unlicense.org/>.

## Test-only dependencies

xUnit (Apache-2.0) is used only for testing. It is not a dependency of the
DotJpegli library, NuGet package or standalone CLI.
