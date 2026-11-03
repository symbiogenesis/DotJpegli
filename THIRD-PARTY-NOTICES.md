# Licensing and third-party notices

## DotJpegli

The managed port and original DotJpegli code are copyright (c) 2026 Edward Miller,
licensed under [BSD-3-Clause](LICENSE). The main license also retains the upstream
copyright notice for adapted portions.

## Jpegli

DotJpegli adapts selected encoder routines and supporting data directly from Google's Jpegli source,
distributed under BSD-3-Clause:

- Repository: https://github.com/google/jpegli
- Revision: `031a0077f5799a6041004267fc12b956c1f52a20`
- Retrieved: October 12, 2026
- Copyright (c) the JPEG XL Project Authors. All rights reserved.
- Complete license: [LICENSE.jpegli](LICENSE.jpegli), also included in packages.

The copyright notice above is the wording in the
[upstream license at the pinned revision](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/LICENSE),
reproduced unchanged in `LICENSE.jpegli`. The upstream
[author list](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/AUTHORS)
includes Google, Cloudinary and individual contributors. Its
[contribution guide](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/CONTRIBUTING.md)
uses "the Jpegli Project Authors" for new file headers and describes Google's role
in receiving contributor license agreements. The repository-wide license at this
revision retains "the JPEG XL Project Authors"; we preserve that supplied notice.

| Managed file | Authoritative upstream source |
| --- | --- |
| `src/DotJpegli/Internal/ForwardDct.cs` | `lib/jpegli/dct-inl.h`: recursive DCT and transform ordering |
| `src/DotJpegli/Internal/ColorConversion.cs` | `lib/jpegli/color_transform.cc`: scalar RGB to YCbCr |
| `src/DotJpegli/Internal/XybColorConversion.cs` | `lib/extras/xyb_transform.cc`, `lib/cms/opsin_params.h`, `lib/cms/transfer_functions-inl.h`: sRGB transfer, opsin absorbance and scaled XYB; managed `MathF` power/cube root replace upstream approximations |
| `src/DotJpegli/Internal/PixelSource.cs` | `lib/jpegli/input.cc`: scalar 16-bit and float input scaling; `color_transform.cc`: direct YCbCr input with no color transform |
| `src/DotJpegli/Internal/IccProfile.cs` | `lib/extras/enc/jpegli.cc`: `FindCICPTag` and raw-profile CICP transfer-function lookup |
| `src/DotJpegli/Internal/XybIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/opsin_params.h`: fixed perceptual scaled XYB ICC header, tags, CLUT, cubic curves, D50 matrix and profile ID; the generated profile's upstream copyright tag is `CC0` |
| `src/DotJpegli/Internal/BuiltInIccColorEncoding.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`: named D65, E and DCI white points; sRGB/Rec.709, P3 and Rec.2020 primaries; canonical adapted colorants, source luminance coefficients, descriptions and CICP eligibility |
| `src/DotJpegli/Internal/CustomIccColorEncoding.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`, `lib/base/matrix_ops.h`: custom CIE xy coordinate descriptions, primary matrix construction and inversion, Bradford adaptation and ICC fixed-point range checks |
| `src/DotJpegli/Internal/RgbIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`: named and custom RGB primary/white-point/transfer combinations and four rendering intents, with shared parametric or untone-mapped 64-entry PQ/HLG curves, adapted colorants and eligible CICP fields; custom coordinates and direct generic gamma curves omit CICP; the generated profiles' upstream copyright tag is `CC0` |
| `src/DotJpegli/Internal/HdrIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`: CICP-representable D65 RGB and P3/DCI PQ and HLG ICC profiles with Lab PCS, 9x9x9 eight-bit tone-mapping LUTs and identity compatibility reverse tags across all rendering intents; the generated profiles' upstream copyright tag is `CC0` |
| `src/DotJpegli/Internal/HdrToneMapping.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/transfer_functions.h`, `lib/cms/tone_mapping.h`: scalar PQ/HLG transfer functions, BT.2408 tone mapping, HLG OOTF, gamut mapping and D50 Lab CLUT generation |
| `src/DotJpegli/Internal/GrayscaleIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`: grayscale profiles across all named transfers, named or custom source white points and four rendering intents, with parametric or direct gamma curves and untone-mapped 64-entry PQ/HLG tables; source media white and no chromatic-adaptation or CICP tag; the generated profiles' upstream copyright tag is `CC0` |
| `src/DotJpegli/Internal/GammaTransferFunction.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/base/common.h`: reciprocal generic gamma encoding exponent, six-significant-digit description formatting and float decoding parameter; managed API uses decoding gamma 1..8192, corresponding to `CustomTransferFunction::SetGamma`'s encoding range without its canonicalization |
| `src/DotJpegli/Internal/IccProfileWriter.cs` | `lib/cms/jpegli_cms_internal.h`: shared deterministic ICC header with selected rendering intent, Bradford adaptation, tag serialization and profile ID |
| `src/DotJpegli/Internal/Downsampling.cs` | `lib/jpegli/downsample.cc`, `encode.cc`: scalar 2x1, 2x2, 1x2 and 4x1 box filters, input smoothing and edge padding |
| `src/DotJpegli/JpegliEncoder.cs` | `lib/jpegli/encode_streaming.cc`: empty MCU block tokens and DC predictor continuity; `lib/jpegli/encode.cc`: default progressive spectral ranges and per-band refinement depths (color DC traversal differs for subsampling) |
| `src/DotJpegli/Internal/JpegScanScript.cs` | `lib/jpegli/encode.cc`: `ValidateScanScript`, per-coefficient refinement progression, complete coefficient coverage and the AC context limit in `ProcessCompressionParams` |
| `src/DotJpegli/Internal/Quantization.cs` | `lib/jpegli/quant.cc`, `encode.cc`, `dct-inl.h`: constants, distance scaling and quantization |
| `src/DotJpegli/Internal/XybQuantization.cs` | `lib/jpegli/quant.cc`: XYB matrices, global scale and nonlinear table scaling |
| `src/DotJpegli/Internal/AdaptiveQuantization.cs` | `lib/jpegli/adaptive_quantization.cc`: spatial analysis, fuzzy erosion and perceptual modulation |
| `src/DotJpegli/Internal/HuffmanTable.cs` | `lib/jpegli/huffman.cc`, `entropy_coding.cc`: standard tables, tree construction and canonical coding |
| `src/DotJpegli/Internal/EntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`: baseline coefficient tokens |
| `src/DotJpegli/Internal/ProgressiveEntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`, `entropy_coding.cc`: progressive DC/AC point transforms, spectral-range tokenization, initial EOB-run aggregation and refinement tokens/correction bits |
| `src/DotJpegli/Internal/BitWriter.cs` | `lib/jpegli/bit_writer.h`, `bit_writer.cc`: bit packing, stuffing and padding |
| `src/DotJpegli/Internal/JpegConstants.cs` | `lib/jpegli/common_internal.h`: natural/zigzag order |

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
