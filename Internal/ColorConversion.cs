// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

namespace DotJpegli.Internal;

internal static class ColorConversion
{
    // Directly derived from google/jpegli lib/jpegli/color_transform.cc,
    // ExtRGBToYCbCr<0, 1, 2>, revision
    // 031a0077f5799a6041004267fc12b956c1f52a20.
    // Full-range BT.601 as defined by JFIF Clause 7.
    private const float RedWeight = 0.299f;
    private const float GreenWeight = 0.587f;
    private const float BlueWeight = 0.114f;
    private const float RedAmplitude = 0.701f;
    private const float BlueAmplitude = 0.886f;
    private const float RedDifference = RedAmplitude + RedWeight;
    private const float BlueDifference = BlueAmplitude + BlueWeight;
    private const float RedNormalization = 1f / (RedAmplitude + (GreenWeight + BlueWeight));
    private const float BlueNormalization = 1f / (RedWeight + (GreenWeight + BlueAmplitude));

    /// <summary>
    /// Converts a byte RGB pixel to fractional JPEG YCbCr sample units.
    /// Chroma is centered at 128 and can reach 255.5 at saturated primaries.
    /// </summary>
    internal static void RgbToYCbCr(byte r, byte g, byte b, out float y, out float cb, out float cr)
    {
        // Keep the C++ multiply and addition ordering: y = r_base +
        // (g_base + b_base). Only the final chroma operations use MulAdd.
        float redBase = r * RedWeight;
        float redDifference = r * RedDifference;
        float greenBase = g * GreenWeight;
        float blueBase = b * BlueWeight;
        float blueDifference = b * BlueDifference;
        y = redBase + (greenBase + blueBase);
        cb = MathF.FusedMultiplyAdd(blueDifference - y, BlueNormalization, 128f);
        cr = MathF.FusedMultiplyAdd(redDifference - y, RedNormalization, 128f);
    }
}
