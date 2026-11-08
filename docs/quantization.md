# Quantization

[Documentation](README.md)

## Standard quantization tables

Set `UseStandardQuantizationTables = true` to use the conventional JPEG Annex K
luma and chroma matrices instead of Jpegli's perceptual matrices (default `false`):

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    UseStandardQuantizationTables = true
});
```

- **Scaling:** `Quality` maps to a distance via `JpegliEncoder.QualityToDistance`
  (or `Distance` overrides it directly). Standard matrices scale linearly from
  that distance using upstream's piecewise formula in
  [`quant.cc`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc),
  with no 4:2:0 chroma compensation. Values are clamped to `1..255` (or
  `1..32767` when `ForceBaseline = false`) before integer conversion.
- **Table assignments:** Grayscale uses the luma matrix in table `0`. YCbCr uses
  the luma matrix in table `0` and shares the chroma matrix in table `1` for
  both `Cb` and `Cr`. Automatic RGB output always uses one shared Annex K luma
  table in slot `0`, and automatic XYB output always uses its three perceptual
  XYB tables, regardless of `UseStandardQuantizationTables`.

## Custom quantization tables

Set `QuantizationTables` to supply final quantization values directly in
**natural row-major 8×8 frequency order** (the encoder reorders them into JPEG
zigzag order when writing `DQT` markers):

```csharp
ushort[] table = Enumerable.Repeat((ushort)16, 64).ToArray();
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    QuantizationTables = [table.AsMemory()],
    AdaptiveQuantization = false
});
```

- **Table counts and selectors:** Each table must have 64 `ushort` values in
  `1..255` (`1..32767` when `ForceBaseline = false`), with index `0` as the DC
  quantizer.
  - **Grayscale:** Requires 1 table (selector `0`).
  - **Color (`YCbCr`, `Rgb`, or `Xyb`):** Accepts 1 shared table (`0/0/0`), 2
    tables where components 1 and 2 share the second table (`0/1/1`), or 3
    separate tables (`0/1/2`).
- **Precedence:** Supplied tables are copied and used unchanged, overriding
  `Quality`, `Distance`, `UseStandardQuantizationTables`, 4:2:0 compensation,
  and CICP table scaling (`Quality` and `Distance` must still have valid values).
- **Adaptive quantization:** Remains enabled unless `AdaptiveQuantization = false`.
  For YCbCr output, adaptive zeroing thresholds infer the effective distance
  from the supplied tables and any CICP transfer hint; grayscale, RGB, and XYB
  use uniform adaptive multipliers.

## Extended quantization (`ForceBaseline = false`)

Set `ForceBaseline = false` (default `true`) to allow 16-bit quantizers in
`1..32767` and up to four sequential Huffman table slots (`0..3`):

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 10,
    ForceBaseline = false
});
```

- **Marker framing:** Any table whose values are all `<= 255` is written with
  8-bit `DQT` entries; tables containing a value `> 255` use big-endian 16-bit
  `DQT` entries. Sequential output uses extended sequential `SOF1` whenever any
  table requires 16-bit entries or sequential Huffman selectors use slots `2` or
  `3`, and otherwise keeps baseline `SOF0`. Progressive output always uses `SOF2`.
- **Adaptive thresholds:** When `ForceBaseline = false`, YCbCr adaptive distance
  inference treats `32767` (rather than `255`) as its saturation ceiling, which
  can slightly change adaptive thresholds even when table entries fit in 8 bits.
- **Compatibility:** Sample precision remains 8 bits (not 12-bit or lossless
  JPEG). Some JPEG viewers have limited `SOF1` support.

## Transfer-function quantization tuning

Set `CicpTransferFunction` (`0..255`, default `2` for unspecified) to tune
perceptual quantization for high-dynamic-range transfer curves, following
[`quant.cc`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc):

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    CicpTransferFunction = 16, // Already PQ-encoded input samples.
    IccProfile = matchingProfile
});
```

- **Supported codes:** Code `16` (PQ) multiplies Jpegli's perceptual global
  quantization scale by `0.4`, and code `18` (HLG) multiplies it by `0.5` before
  4:2:0 compensation and rounding. All other codes (including `13` for sRGB)
  keep the default `1.0` scale.
- **Scope:** Applies to perceptual YCbCr and grayscale table generation and to
  YCbCr adaptive thresholds (including when `UseStandardQuantizationTables = true`
  or `QuantizationTables` is supplied). RGB and XYB output ignore this hint.
  Setting `CicpTransferFunction` only scales quantization—it does not convert
  pixel transfer curves, tone-map, or embed an ICC profile.

### Inferring CICP from `IccProfile`

Set `InferCicpTransferFunctionFromIccProfile = true` (default `false`) to read
the transfer-function byte from the attached `IccProfile`'s `cicp` tag when
`CicpTransferFunction` is left at `2` (unspecified):

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    IccProfile = matchingProfile,
    InferCicpTransferFunctionFromIccProfile = true
});
```

Any explicit `CicpTransferFunction` value other than `2` overrides profile
lookup. Following upstream's
[`FindCICPTag`](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/extras/enc/jpegli.cc),
the lookup checks for at least 132 profile bytes, a complete tag directory, and
a final `cicp` tag entry pointing to at least 12 readable bytes starting with
the `cicp` signature, reading byte `9` as the transfer code (or falling back to
`2` if absent or invalid).
