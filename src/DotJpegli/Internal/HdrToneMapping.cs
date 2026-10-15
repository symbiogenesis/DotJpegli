// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

internal static class HdrToneMapping
{
    // Named-primary/white-point branches of ToneMapPixel, tone_mapping.h and
    // transfer_functions.h, google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
    // Keep float evaluation order: the upstream transfer functions calculate in
    // double but return through copysignf before subsequent float arithmetic.
    internal static void WriteClut(Span<byte> destination, bool pq,
        ReadOnlySpan<float> luminances, ReadOnlySpan<float> colorants)
    {
        const int gridSize = 9;
        const int byteCount = gridSize * gridSize * gridSize * 3;
        if (destination.Length != byteCount)
            throw new ArgumentException("The HDR CLUT requires 2187 bytes.", nameof(destination));

        Span<float> decoded = stackalloc float[gridSize];
        for (int node = 0; node < gridSize; node++)
        {
            float encoded = node * (1f / (gridSize - 1));
            decoded[node] = pq ? PqDisplayFromEncoded(10000f, encoded) : HlgDisplayFromEncoded(encoded);
        }

        var toneMapper = new Rec2408ToneMapper();
        float hlgGamma = MathF.Pow(1.111f, StandardFloatingPoint.Log2(80f / 300f));
        float hlgExponent = hlgGamma - 1f;
        Span<float> linear = stackalloc float[3];
        int offset = 0;
        for (int red = 0; red < gridSize; red++)
        {
            for (int green = 0; green < gridSize; green++)
            {
                for (int blue = 0; blue < gridSize; blue++)
                {
                    linear[0] = decoded[red];
                    linear[1] = decoded[green];
                    linear[2] = decoded[blue];
                    if (pq)
                        toneMapper.Apply(linear, luminances);
                    else
                        ApplyHlgOotf(linear, hlgExponent, luminances);
                    GamutMap(linear, luminances);
                    WriteLab(linear, destination.Slice(offset, 3), colorants);
                    offset += 3;
                }
            }
        }
    }

    private static float Luminance(ReadOnlySpan<float> rgb, ReadOnlySpan<float> luminances) =>
        luminances[0] * rgb[0] + luminances[1] * rgb[1] + luminances[2] * rgb[2];

    private static void ApplyHlgOotf(Span<float> rgb, float exponent, ReadOnlySpan<float> luminances)
    {
        if (exponent >= -0.01f && exponent <= 0.01f)
            return;
        float ratio = MathF.Min(MathF.Pow(Luminance(rgb, luminances), exponent), 1e9f);
        // Black has pow(0, negative) == infinity, capped before multiplication.
        rgb[0] *= ratio;
        rgb[1] *= ratio;
        rgb[2] *= ratio;
    }

    private static void GamutMap(Span<float> rgb, ReadOnlySpan<float> luminances)
    {
        float luminance = Luminance(rgb, luminances);
        float grayMixSaturation = 0f;
        float grayMixLuminance = 0f;
        for (int channel = 0; channel < 3; channel++)
        {
            float value = rgb[channel];
            float valueMinusGray = value - luminance;
            float inverse = 1f / (valueMinusGray == 0f ? 1f : valueMinusGray);
            float valueOverDifference = value * inverse;
            if (valueMinusGray < 0f)
                grayMixSaturation = MathF.Max(grayMixSaturation, valueOverDifference);
            grayMixLuminance = MathF.Max(grayMixLuminance,
                valueMinusGray <= 0f ? grayMixSaturation : valueOverDifference - inverse);
        }
        float grayMix = Math.Clamp(
            0.3f * (grayMixSaturation - grayMixLuminance) + grayMixLuminance, 0f, 1f);
        for (int channel = 0; channel < 3; channel++)
            rgb[channel] = grayMix * (luminance - rgb[channel]) + rgb[channel];
        float maximum = MathF.Max(MathF.Max(MathF.Max(1f, rgb[0]), rgb[1]), rgb[2]);
        float normalizer = 1f / maximum;
        for (int channel = 0; channel < 3; channel++)
            rgb[channel] *= normalizer;
    }

    private static void WriteLab(ReadOnlySpan<float> rgb, Span<byte> destination, ReadOnlySpan<float> colorants)
    {
        // Colorants are columns of AdaptToXYZD50 * PrimariesToXYZ. Unlike
        // matrix construction, ToneMapPixel accumulates products in float.
        Span<float> xyz = stackalloc float[3];
        for (int row = 0; row < 3; row++)
        {
            float value = 0f;
            for (int channel = 0; channel < 3; channel++)
                value += rgb[channel] * colorants[channel * 3 + row];
            xyz[row] = value;
        }
        float fx = LabFunction(xyz[0] / 0.964212f);
        float fy = LabFunction(xyz[1]);
        float fz = LabFunction(xyz[2] / 0.825188f);
        destination[0] = RoundByte(255f * Math.Clamp(1.16f * fy - 0.16f, 0f, 1f));
        destination[1] = RoundByte(128f + Math.Clamp(500f * (fx - fy), -128f, 127f));
        destination[2] = RoundByte(128f + Math.Clamp(200f * (fy - fz), -128f, 127f));
    }

    private static float LabFunction(float value)
    {
        const float delta = (float)(6.0 / 29);
        return value <= delta * delta * delta
            ? value * (1f / (3f * delta * delta)) + 4f / 29
            : MathF.Cbrt(value);
    }

    private static byte RoundByte(float value) =>
        (byte)MathF.Round(value, MidpointRounding.AwayFromZero);

    internal static float HlgDisplayFromEncoded(double encoded)
    {
        if (encoded == 0.0)
            return 0f;
        const double a = 0.17883277;
        const double inverseA = 1.0 / a;
        const double b = 1 - 4 * a;
        const double c = 0.5599107295;
        double magnitude = Math.Abs(encoded);
        double scene = magnitude <= 0.5
            ? magnitude * magnitude * (1.0 / 3)
            : (Math.Exp((magnitude - c) * inverseA) + b) * (1.0 / 12);
        return StandardFloatingPoint.CopySign((float)scene, (float)encoded);
    }

    private const double PqM1 = 2610.0 / 16384;
    private const double PqM2 = (2523.0 / 4096) * 128;
    private const double PqC1 = 3424.0 / 4096;
    private const double PqC2 = (2413.0 / 4096) * 32;
    private const double PqC3 = (2392.0 / 4096) * 32;

    internal static float PqDisplayFromEncoded(float intensityTarget, double encoded)
    {
        if (encoded == 0.0)
            return 0f;
        double power = Math.Pow(Math.Abs(encoded), 1.0 / PqM2);
        double numerator = Math.Max(power - PqC1, 0.0);
        double denominator = PqC2 - PqC3 * power;
        double display = Math.Pow(numerator / denominator, 1.0 / PqM1);
        return StandardFloatingPoint.CopySign((float)(display * (10000f / intensityTarget)), (float)encoded);
    }

    private static float PqEncodedFromDisplay(float intensityTarget, double display)
    {
        if (display == 0.0)
            return 0f;
        double power = Math.Pow(Math.Abs(display) * (intensityTarget * (1f / 10000f)), PqM1);
        double numerator = PqC1 + power * PqC2;
        double denominator = 1.0 + power * PqC3;
        return StandardFloatingPoint.CopySign((float)Math.Pow(numerator / denominator, PqM2), (float)display);
    }

    private readonly struct Rec2408ToneMapper
    {
        private readonly float masteringMinimum;
        private readonly float masteringRange;
        private readonly float inverseMasteringRange;
        private readonly float minimumLuminance;
        private readonly float maximumLuminance;
        private readonly float knee;
        private readonly float inverseOneMinusKnee;

        public Rec2408ToneMapper()
        {
            masteringMinimum = PqEncodedFromDisplay(1f, 0f);
            float masteringMaximum = PqEncodedFromDisplay(1f, 10000f);
            masteringRange = masteringMaximum - masteringMinimum;
            inverseMasteringRange = 1f / masteringRange;
            minimumLuminance = (PqEncodedFromDisplay(1f, 0f) - masteringMinimum) * inverseMasteringRange;
            maximumLuminance = (PqEncodedFromDisplay(1f, 250f) - masteringMinimum) * inverseMasteringRange;
            knee = 1.5f * maximumLuminance - 0.5f;
            inverseOneMinusKnee = 1f / MathF.Max(1e-6f, 1f - knee);
        }

        internal void Apply(Span<float> rgb, ReadOnlySpan<float> luminances)
        {
            float luminance = 10000f * Luminance(rgb, luminances);
            float normalizedPq = MathF.Min(1f,
                (PqEncodedFromDisplay(1f, luminance) - masteringMinimum) * inverseMasteringRange);
            float e2 = normalizedPq < knee ? normalizedPq : Polynomial(normalizedPq);
            float oneMinusE2 = 1f - e2;
            float oneMinusE2Squared = oneMinusE2 * oneMinusE2;
            float oneMinusE2Fourth = oneMinusE2Squared * oneMinusE2Squared;
            float e3 = minimumLuminance * oneMinusE2Fourth + e2;
            float e4 = e3 * masteringRange + masteringMinimum;
            float d4 = PqDisplayFromEncoded(1f, e4);
            float newLuminance = Math.Clamp(d4, 0f, 250f);
            const float minimum = 1e-6f;
            bool useCap = luminance <= minimum;
            float ratio = newLuminance / MathF.Max(luminance, minimum);
            float cap = newLuminance * (1f / 250f);
            float multiplier = ratio * (10000f / 250f);
            for (int channel = 0; channel < 3; channel++)
                rgb[channel] = useCap ? cap : rgb[channel] * multiplier;
        }

        private float Polynomial(float value)
        {
            float t = (value - knee) * inverseOneMinusKnee;
            float tSquared = t * t;
            float tCubed = tSquared * t;
            return (2f * tCubed - 3f * tSquared + 1f) * knee +
                (tCubed - 2f * tSquared + t) * (1f - knee) +
                (-2f * tCubed + 3f * tSquared) * maximumLuminance;
        }
    }
}
