// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

namespace DotJpegli.Internal;

/// <summary>Converts normalized encoded sRGB to scaled XYB in fractional JPEG sample units.</summary>
internal static class XybColorConversion
{
    // Derived from lib/extras/xyb_transform.cc and lib/cms/opsin_params.h,
    // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20, at the upstream
    // default intensity target of 255. The managed transfer and cube root use
    // MathF instead of upstream's rational/fast-math approximations.
    private const float Bias = 0.0037930732552754493f;
    private static readonly float BiasCubeRoot = MathF.Cbrt(Bias);
    private const float M00 = 0.30f;
    private const float M02 = 0.078f;
    private const float M01 = 1f - M02 - M00;
    private const float M10 = 0.23f;
    private const float M12 = 0.078f;
    private const float M11 = 1f - M12 - M10;
    private const float M20 = 0.24342268924547819f;
    private const float M21 = 0.20476744424496821f;
    private const float M22 = 1f - M20 - M21;

    internal static void SrgbToScaledXyb(float r, float g, float b,
        out float x, out float y, out float bMinusY)
    {
        r = Linearize(r);
        g = Linearize(g);
        b = Linearize(b);
        float mixed0 = Absorb(r, g, b, M00, M01, M02);
        float mixed1 = Absorb(r, g, b, M10, M11, M12);
        float mixed2 = Absorb(r, g, b, M20, M21, M22);
        float opsin0 = MathF.Cbrt(MathF.Max(0f, mixed0)) - BiasCubeRoot;
        float opsin1 = MathF.Cbrt(MathF.Max(0f, mixed1)) - BiasCubeRoot;
        float opsin2 = MathF.Cbrt(MathF.Max(0f, mixed2)) - BiasCubeRoot;
        float unscaledX = 0.5f * (opsin0 - opsin1);
        float unscaledY = 0.5f * (opsin0 + opsin1);
        x = ((unscaledX + 0.015386134f) * 22.995788804f) * 255f;
        y = (unscaledY * 1.183000077f) * 255f;
        bMinusY = ((opsin2 - unscaledY + 0.27770459f) * 1.502141333f) * 255f;
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(bMinusY))
            throw NonfinitePixels();
    }

    private static float Linearize(float value)
    {
        // TF_SRGB::DisplayFromEncoded preserves the sign for out-of-gamut
        // samples. Do not clip source values; only absorbance is clamped below.
        float magnitude = MathF.Abs(value);
        float linear = magnitude <= 0.04045f ? magnitude / 12.92f
            : MathF.Pow((magnitude + 0.055f) / 1.055f, 2.4f);
        if (!float.IsFinite(linear))
            throw NonfinitePixels();
        return MathF.CopySign(linear, value);
    }

    private static float Absorb(float r, float g, float b, float mr, float mg, float mb)
    {
        float mixed = MathF.FusedMultiplyAdd(mr, r,
            MathF.FusedMultiplyAdd(mg, g, MathF.FusedMultiplyAdd(mb, b, Bias)));
        if (!float.IsFinite(mixed))
            throw NonfinitePixels();
        return mixed;
    }

    private static ArgumentException NonfinitePixels() => new(
        "The floating-point pixels must produce finite sRGB-to-XYB computations.", "pixels");
}
