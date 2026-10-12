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
| `Internal/Downsampling.cs` | `lib/jpegli/downsample.cc`, `encode.cc`: scalar box filters and input edge padding |
| `JpegliEncoder.cs` | `lib/jpegli/encode_streaming.cc`: empty MCU block tokens and DC predictor continuity |
| `Internal/Quantization.cs` | `lib/jpegli/quant.cc`, `encode.cc`, `dct-inl.h`: constants, distance scaling and quantization |
| `Internal/AdaptiveQuantization.cs` | `lib/jpegli/adaptive_quantization.cc`: spatial analysis, fuzzy erosion and perceptual modulation |
| `Internal/HuffmanTable.cs` | `lib/jpegli/huffman.cc`, `entropy_coding.cc`: standard tables, tree construction and canonical coding |
| `Internal/EntropyEncoder.cs` | `lib/jpegli/entropy_coding-inl.h`: baseline coefficient tokens |
| `Internal/BitWriter.cs` | `lib/jpegli/bit_writer.h`, `bit_writer.cc`: bit packing, stuffing and padding |
| `Internal/JpegConstants.cs` | `lib/jpegli/common_internal.h`: natural/zigzag order |

The public managed API, image traversal and marker writer
are implemented for this port. No upstream C++ binary is linked.

Test-only dependencies have their own licenses: xUnit (Apache-2.0) and
StbImageSharp (MIT or Unlicense). They are not dependencies of the DotJpegli
library or NuGet package.
