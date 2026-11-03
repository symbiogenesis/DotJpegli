namespace DotJpegli.Performance;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>Offline AC quantization research; never selected by the encoder.</summary>
internal static class AcReciprocalExperiment
{
    internal readonly record struct Interval(float Lower, float Upper)
    {
        internal static Interval Create(int divisor)
        {
            if (divisor is < 1 or > 32767)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor));
            }

            float reciprocal = 1f / divisor;
            // A binary32 significand times this <=15-bit integer fits exactly
            // in binary64. This comparison therefore directs the bounds without
            // assuming that binary64 division would round in the same direction.
            double product = (double)reciprocal * divisor;
            return product == 1 ? new(reciprocal, reciprocal)
                : product < 1 ? new(reciprocal, MathF.BitIncrement(reciprocal))
                : new(MathF.BitDecrement(reciprocal), reciprocal);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryQuantize(float coefficient, Interval reciprocal, float threshold, out float value)
    {
        value = 0;
        if (!float.IsFinite(coefficient) || !(threshold >= 0))
        {
            return false;
        }

        return TryQuantizeFinite(coefficient, reciprocal, threshold, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryQuantizeFinite(float coefficient, Interval reciprocal, float threshold, out float value)
    {
        value = 0;

        // Rounded multiplication is monotone. Since Lower <= 1/divisor <=
        // Upper, these products enclose RN(coefficient/divisor), with their
        // order reversed for negative coefficients. This includes underflow.
        float lower = coefficient * reciprocal.Lower;
        float upper = coefficient * reciprocal.Upper;
        bool suppressLower = MathF.Abs(lower) < threshold;
        bool suppressUpper = MathF.Abs(upper) < threshold;
        if (suppressLower != suppressUpper)
        {
            return false;
        }
        if (suppressLower)
        {
            return true;
        }

        lower = MathF.Round(lower, MidpointRounding.ToEven);
        upper = MathF.Round(upper, MidpointRounding.ToEven);
        if (lower != upper)
        {
            return false;
        }

        value = lower;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static short Exact(float coefficient, float divisor, float threshold, bool requireEightBit = true)
    {
        ValidateCoefficient(coefficient);
        float quotient = coefficient / divisor;
        float value = MathF.Abs(quotient) < threshold ? 0 : MathF.Round(quotient, MidpointRounding.ToEven);
        return Store(value, requireEightBit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static short Guarded(float coefficient, float divisor, Interval reciprocal, float threshold,
        bool requireEightBit = true)
    {
        ValidateCoefficient(coefficient);
        if (TryQuantizeFinite(coefficient, reciprocal, threshold, out float value))
        {
            return Store(value, requireEightBit);
        }
        float quotient = coefficient / divisor;
        return Store(MathF.Abs(quotient) < threshold ? 0 : MathF.Round(quotient, MidpointRounding.ToEven), requireEightBit);
    }

    internal static void ScalarExact(float[] coefficients, float[] divisors, float[] thresholds, short[] output)
    {
        for (int i = 0; i < coefficients.Length; i++)
        {
            output[i] = Exact(coefficients[i], divisors[i], thresholds[i]);
        }
    }

    internal static void ScalarGuarded(float[] coefficients, float[] divisors, Interval[] reciprocals,
        float[] thresholds, short[] output)
    {
        for (int i = 0; i < coefficients.Length; i++)
        {
            output[i] = Guarded(coefficients[i], divisors[i], reciprocals[i], thresholds[i]);
        }
    }

    internal static void VectorExact(float[] coefficients, float[] divisors, float[] thresholds, float[] output)
    {
        for (int i = 0; i < coefficients.Length; i += Vector<float>.Count)
        {
            Vector<float> source = new(coefficients, i);
            ValidateVector(source);
            Vector<float> quotient = source / new Vector<float>(divisors, i);
            Vector<float> value = Round(quotient, new Vector<float>(thresholds, i));
            StoreVector(value, output, i);
        }
    }

    internal static void VectorGuarded(float[] coefficients, float[] divisors, float[] reciprocalLower,
        float[] reciprocalUpper, float[] thresholds, float[] output)
    {
        for (int i = 0; i < coefficients.Length; i += Vector<float>.Count)
        {
            Vector<float> source = new(coefficients, i);
            ValidateVector(source);
            Vector<float> threshold = new(thresholds, i);
            Vector<float> lower = source * new Vector<float>(reciprocalLower, i);
            Vector<float> upper = source * new Vector<float>(reciprocalUpper, i);
            var lowerSuppressed = Vector.LessThan(Vector.Abs(lower), threshold);
            var upperSuppressed = Vector.LessThan(Vector.Abs(upper), threshold);
            lower = Vector.ConditionalSelect(lowerSuppressed, Vector<float>.Zero, Vector.Round(lower));
            upper = Vector.ConditionalSelect(upperSuppressed, Vector<float>.Zero, Vector.Round(upper));
            Vector<int> accepted = Vector.Equals(lowerSuppressed, upperSuppressed) & Vector.Equals(lower, upper);
            if (!Vector.EqualsAll(accepted, new Vector<int>(-1)))
            {
                // A single ambiguous lane pays for the original vector divide.
                // Keep all lanes from that exact fallback rather than add a blend.
                lower = Round(source / new Vector<float>(divisors, i), threshold);
            }
            StoreVector(lower, output, i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<float> Round(Vector<float> quotient, Vector<float> threshold)
        => Vector.ConditionalSelect(Vector.LessThan(Vector.Abs(quotient), threshold), Vector<float>.Zero, Vector.Round(quotient));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "The offline experiment preserves the production quantizer's reported coefficients parameter.")]
    private static void ValidateCoefficient(float coefficient)
    {
        if (!float.IsFinite(coefficient))
        {
            throw new ArgumentException("DCT coefficients must be finite.", "coefficients");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "The offline experiment preserves the production quantizer's reported coefficients parameter.")]
    private static short Store(float value, bool requireEightBit)
    {
        if (requireEightBit && (value < -1023f || value > 1023f))
        {
            throw new ArgumentOutOfRangeException("coefficients", "A quantized coefficient exceeds eight-bit JPEG's supported range.");
        }
        return (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "The offline experiment preserves the production quantizer's reported coefficients parameter.")]
    private static void ValidateVector(Vector<float> source)
    {
        if (!Vector.LessThanOrEqualAll(Vector.Abs(source), new Vector<float>(float.MaxValue)))
        {
            throw new ArgumentException("DCT coefficients must be finite.", "coefficients");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "The offline experiment preserves the production quantizer's reported coefficients parameter.")]
    private static void StoreVector(Vector<float> value, float[] output, int index)
    {
        if (!Vector.LessThanOrEqualAll(Vector.Abs(value), new Vector<float>(1023f)))
        {
            throw new ArgumentOutOfRangeException("coefficients", "A quantized coefficient exceeds eight-bit JPEG's supported range.");
        }
        value.CopyTo(output, index);
    }
}
