# Upstream source and notices

DotJpegli adapts selected routines and data directly from Google's Jpegli source,
distributed under BSD-3-Clause:

- Repository: https://github.com/google/jpegli
- Revision: `031a0077f5799a6041004267fc12b956c1f52a20`
- Retrieved: October 12, 2026
- Copyright (c) the JPEG XL Project Authors. All rights reserved.
- Complete license: [LICENSE.jpegli](LICENSE.jpegli), also included in packages.

| Managed file | Authoritative upstream source |
| --- | --- |
| `Internal/ForwardDct.cs` | `lib/jpegli/dct-inl.h`: recursive DCT and transform ordering |
| `Internal/ColorConversion.cs` | `lib/jpegli/color_transform.cc`: scalar RGB to YCbCr |
| `Internal/XybColorConversion.cs` | `lib/extras/xyb_transform.cc`, `lib/cms/opsin_params.h`, `lib/cms/transfer_functions-inl.h`: sRGB transfer, opsin absorbance and scaled XYB; managed `MathF` power/cube root replace upstream approximations |
| `Internal/PixelSource.cs` | `lib/jpegli/input.cc`: scalar 16-bit and float input scaling; `color_transform.cc`: direct YCbCr input with no color transform |
| `Internal/IccProfile.cs` | `lib/extras/enc/jpegli.cc`: `FindCICPTag` and raw-profile CICP transfer-function lookup |
| `Internal/XybIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/opsin_params.h`: fixed perceptual scaled XYB ICC header, tags, CLUT, cubic curves, D50 matrix and profile ID; the generated profile's upstream copyright tag is `CC0` |
| `Internal/RgbIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/color_encoding_cms.h`: fixed D65/perceptual RGB profiles with sRGB/Rec.709, P3 or Rec.2020 primaries, shared sRGB, BT.709 or identity parametric transfer curves, adapted colorants and CICP fields; the generated profiles' upstream copyright tag is `CC0` |
| `Internal/HdrIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`: fixed Rec.2020/D65/perceptual PQ and HLG ICC profiles with CICP signaling, Lab PCS, 9x9x9 eight-bit tone-mapping LUTs and identity compatibility reverse tags; the generated profiles' upstream copyright tag is `CC0` |
| `Internal/HdrToneMapping.cs` | `lib/cms/jpegli_cms_internal.h`, `lib/cms/transfer_functions.h`, `lib/cms/tone_mapping.h`: scalar PQ/HLG transfer functions, BT.2408 tone mapping, HLG OOTF, gamut mapping and D50 Lab CLUT generation |
| `Internal/GrayscaleIccProfile.cs` | `lib/cms/jpegli_cms_internal.h`: fixed perceptual grayscale profiles with sRGB, identity, BT.709 or DCI parametric curves and 64-entry PQ/HLG decoding tables; source D65/DCI media white and no chromatic-adaptation or CICP tag; the generated profiles' upstream copyright tag is `CC0` |
| `Internal/IccProfileWriter.cs` | `lib/cms/jpegli_cms_internal.h`: shared fixed perceptual ICC header, Bradford adaptation, tag serialization and profile ID |
| `Internal/Downsampling.cs` | `lib/jpegli/downsample.cc`, `encode.cc`: scalar 2x1, 2x2, 1x2 and 4x1 box filters, input smoothing and edge padding |
| `JpegliEncoder.cs` | `lib/jpegli/encode_streaming.cc`: empty MCU block tokens and DC predictor continuity; `lib/jpegli/encode.cc`: default progressive spectral ranges and per-band refinement depths (color DC traversal differs for subsampling) |
| `Internal/Quantization.cs` | `lib/jpegli/quant.cc`, `encode.cc`, `dct-inl.h`: constants, distance scaling and quantization |
| `Internal/XybQuantization.cs` | `lib/jpegli/quant.cc`: XYB matrices, global scale and nonlinear table scaling |
| `Internal/AdaptiveQuantization.cs` | `lib/jpegli/adaptive_quantization.cc`: spatial analysis, fuzzy erosion and perceptual modulation |
| `Internal/HuffmanTable.cs` | `lib/jpegli/huffman.cc`, `entropy_coding.cc`: standard tables, tree construction and canonical coding |
| `Internal/EntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`: baseline coefficient tokens |
| `Internal/ProgressiveEntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`, `entropy_coding.cc`: progressive DC/AC point transforms, spectral-range tokenization, initial EOB-run aggregation and refinement tokens/correction bits |
| `Internal/BitWriter.cs` | `lib/jpegli/bit_writer.h`, `bit_writer.cc`: bit packing, stuffing and padding |
| `Internal/JpegConstants.cs` | `lib/jpegli/common_internal.h`: natural/zigzag order |

The public managed API, image traversal and marker writer
are implemented for this port. No upstream C++ binary is linked.

Test-only dependencies have their own licenses: xUnit (Apache-2.0) and
StbImageSharp (MIT or Unlicense). They are not dependencies of the DotJpegli
library or NuGet package.
