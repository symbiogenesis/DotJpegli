# Quantization

[Documentation](README.md)

## Standard quantization tables

Set `UseStandardQuantizationTables = true` to use the conventional JPEG Annex K
luma and chroma matrices instead of Jpegli's perceptual matrices:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 85,
    UseStandardQuantizationTables = true
});
```

The default is false, selecting Jpegli's perceptual matrices. `Quality` maps
through `QualityToDistance`; a positive finite `Distance` still overrides it.
The standard matrices use the pinned upstream's piecewise linear-quality scale
from that distance, with rounding and clamping to the `ForceBaseline` range
(1..255 by default, or 1..32767 when false). Both chroma components
share table 1; grayscale uses only the luma matrix in table 0. Sampling factors
still select the output resolution, but standard tables have no 4:2:0 compensation.
Automatic RGB tables use the shared luma matrix in table 0, independently of this option.
The managed encoder clamps before converting to integers, including immediately
below distance 25 where upstream's scale can overflow its integer conversion.
Those cases retain legal saturated tables; they do not establish native parity.

Adaptive quantization, Jpegli's coefficient thresholds, smoothing, higher-precision
and direct YCbCr input, restart markers and progressive refinement remain available.
Adaptive thresholds follow upstream's shared chroma table selectors when inferring
distance from emitted tables. This option changes the matrices and their scaling;
it does not promise libjpeg byte parity, equivalent quality, size or speed.
In particular, fractional scaling can differ from libjpeg's integer quality scale.
The scaling and shared-table rules are ported from
[the pinned upstream quantizer](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc).

## Custom quantization tables

Set `QuantizationTables` to supply final quantizers in natural row-major
8x8 frequency order. Each table must contain exactly 64 `ushort` values in 1..255
(or 1..32767 with `ForceBaseline = false`);
the first value controls DC. The encoder writes them in JPEG zigzag order:

```csharp
ushort[] table = Enumerable.Repeat((ushort)16, 64).ToArray();
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    QuantizationTables = [table.AsMemory()],
    AdaptiveQuantization = false
});
```

Grayscale output requires one table. Color output accepts one shared table
(selectors 0/0/0), two tables with the second shared by components 1 and 2
(0/1/1), or three separate tables (0/1/2). Component order follows the output:
Y/Cb/Cr, R/G/B, or scaled X/Y/B-minus-Y. Equal-valued tables retain separate
selectors when supplied separately. Null, the default, preserves automatic tables
and existing bytes. Empty lists and values outside the selected range are rejected
with an `ArgumentException` for `QuantizationTables` before output.

Supplied values are used unchanged. They override table construction from
`Quality`, `Distance` and `UseStandardQuantizationTables`, including automatic
sampling and transfer-function scaling. Those options remain validated.
Adaptive quantization still applies unless disabled; YCbCr adaptive thresholds
use the actual table selectors and CICP transfer hint, while grayscale/RGB/XYB
retain their uniform adaptive multipliers. Thus a transfer hint can affect
YCbCr coefficients with adaptive quantization enabled without changing the tables.

The list and all table data are copied before writing output. Do not mutate them
while encoding starts; the encoder leaves caller storage unchanged. The same
tables serve coefficient preflight, Huffman optimization, sequential and progressive
scans, restarts, all input precisions and custom sampling. XYB still requires its
output ICC profile. Custom tables do not enable lossless JPEG.
This final-value API corresponds to table installation with scale 100 in
[the pinned upstream encoder](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/encode.cc);
DotJpegli rejects invalid quantizers instead of scaling or clamping them.

## Extended quantization

Set `ForceBaseline = false` to use the pinned upstream's nonbaseline quantizer
range of 1..32767 for automatic and caller-supplied tables:

```csharp
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width, height, options: new()
{
    Quality = 10,
    ForceBaseline = false
});
```

Each table uses eight-bit DQT entries when all its values are at most 255;
otherwise its 64 entries are unsigned 16-bit values in network byte order.
Tables with different precisions can share one DQT marker. Sequential output
uses extended sequential SOF1 when any table needs 16 bits or optimized Huffman
selectors use slots 2 or 3, and otherwise retains baseline SOF0. Progressive
output always retains SOF2. Separate
component scans and caller-supplied scan scripts follow the same frame rule.

`ForceBaseline` defaults to `true`, limiting quantizers to eight bits and
sequential Huffman selectors to slots 0 and 1. The opt-in
range applies to perceptual YCbCr/grayscale and XYB matrices and standard
matrices, including RGB's shared standard luma table. Adaptive YCbCr distance
inference treats 32767 as its saturated upper endpoint when the option is false;
255 then becomes an ordinary rounded value, which can change adaptive thresholds
even for eight-bit tables. Grayscale, RGB and XYB retain their uniform adaptive
multipliers. Custom tables are snapshotted and checked against the selected range
before output; zero and values above 32767 are rejected in extended mode.

Sampling, smoothing, source precision, metadata, restarts, bounded storage,
coefficient validation and stream behavior retain their existing contracts.
JPEG sample precision remains eight bits. Sixteen-bit quantizers do not enable
12-bit samples or lossless encoding. Some readers have limited SOF1 support.
This ports the quantizer range and marker rules from the pinned upstream
`quant.cc`, `encode.cc` and `bitstream.cc`; it does not establish encoder parity.

## Transfer-function quantization tuning

Set `CicpTransferFunction` to describe the transfer function of the supplied samples:

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    CicpTransferFunction = 16, // Already PQ-encoded input samples.
    IccProfile = matchingProfile
});
```

The option accepts one byte's code range, 0..255, and defaults to 2 (unspecified).
Codes 16 (PQ) and 18 (HLG) multiply upstream's perceptual global quantization
scale by 0.4 and 0.5 respectively, before any 4:2:0 compensation and coefficient
rounding. Other codes preserve default tuning, including 13 (sRGB); registration
of the supplied code is not validated. Quality and direct-distance controls retain
their existing meaning. This applies to RGB, grayscale and direct YCbCr input at
all three source precisions, with unchanged sampling and scan options.

Adaptive quantization infers its coefficient thresholds using the same
transfer-function scale. With `UseStandardQuantizationTables`, the emitted Annex K
tables ignore the hint, but adaptive thresholds still use it, following upstream.
Grayscale retains its constant adaptive coefficient multipliers.
RGB output ignores transfer-function hints for both tables and adaptive thresholds.

The hint follows [the pinned upstream quantizer](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/jpegli/quant.cc).
It does not convert transfer functions, apply a profile, tone-map or write CICP
metadata. Supply appropriately encoded samples and any matching ICC profile
yourself. Output remains a lossy eight-bit JPEG; this tuning alone does not
establish HDR conformance or a measured perceptual quality guarantee.

Set `InferCicpTransferFunctionFromIccProfile = true` to obtain the hint from the
profile's `cicp` tag instead of specifying it separately:

```csharp
byte[] jpeg = JpegliEncoder.Encode(pqPixels, width, height, options: new()
{
    IccProfile = matchingProfile,
    InferCicpTransferFunctionFromIccProfile = true
});
```

The default is false, leaving transfer-function tuning independent of the attached
profile. Lookup applies only when `CicpTransferFunction` is 2 (unspecified);
every other explicit code takes precedence, including codes that retain default
tuning. To request unspecified tuning independently of the profile, leave lookup
disabled. Inferred codes feed both table generation and adaptive thresholds,
including standard tables and out-of-range float coefficient validation.

Lookup follows [the pinned upstream raw-profile helper](https://github.com/google/jpegli/blob/031a0077f5799a6041004267fc12b956c1f52a20/lib/extras/enc/jpegli.cc).
It requires at least 132 profile bytes and a complete directory of 12-byte tag
entries. The last `cicp` entry wins, must declare at least 12 payload bytes, and
must point to 12 readable bytes starting with the `cicp` type signature. Byte 9
of that payload supplies the transfer code. Missing, truncated or mismatched
data returns 2; an invalid last entry does not fall back to an earlier one.
Unsigned counts and offsets are bounded before indexing. Directory scanning uses
constant working storage and checks cancellation periodically, before JPEG output.
The profile may span APP2 markers; lookup uses the original complete profile.

This narrow lookup does not validate ICC conformance, the header signature,
version or declared profile size, the rest of a larger declared tag, reserved
bytes, or the primaries, matrix and range fields. Unreadable lookup data remains
embedded unchanged. No other profile tags are interpreted, and no profile is
generated, reconciled with an explicit hint, or applied to the samples.
