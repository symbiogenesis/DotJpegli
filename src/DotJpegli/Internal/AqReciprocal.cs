namespace DotJpegli.Internal;

/// <summary>Offline AQ reciprocal experiment; default kernels retain division.</summary>
internal static class AqReciprocal
{
    private static readonly double[] ReciprocalSeeds = CreateReciprocalSeeds();

    internal static float Divide(float numerator, float denominator, out bool usedFallback)
    {
        uint denominatorBits = (uint)BitConverter.SingleToInt32Bits(denominator);
        uint exponent = denominatorBits >> 23;
        if (exponent is > 0 and < 255)
        {
            uint fraction = denominatorBits & 0x007fffff;
            double mantissa = 1d + (fraction * (1d / 8388608d));
            double reciprocal = ReciprocalSeeds[fraction >> 15];
            reciprocal *= 2d - (mantissa * reciprocal);
            reciprocal *= 2d - (mantissa * reciprocal);
            double scale = BitConverter.Int64BitsToDouble((long)(1150 - exponent) << 52);
            float candidate = (float)((double)numerator * (reciprocal * scale));
            if (Certifies(numerator, denominator, candidate))
            {
                usedFallback = false;
                return candidate;
            }
        }

        usedFallback = true;
        return numerator / denominator;
    }

    internal static bool Certifies(float numerator, float denominator, float candidate)
    {
        uint numeratorBits = (uint)BitConverter.SingleToInt32Bits(numerator);
        uint denominatorBits = (uint)BitConverter.SingleToInt32Bits(denominator);
        uint candidateBits = (uint)BitConverter.SingleToInt32Bits(candidate);
        uint numeratorMagnitude = numeratorBits & 0x7fffffff;
        uint candidateMagnitude = candidateBits & 0x7fffffff;
        if (numeratorMagnitude is 0 or >= 0x7f800000
            || (denominatorBits >> 23) is 0 or >= 255
            || candidateMagnitude is < 0x00800000 or >= 0x7f7fffff
            || ((numeratorBits ^ candidateBits) & 0x80000000) != 0)
        {
            return false;
        }

        double magnitude = BitConverter.Int32BitsToSingle((int)numeratorMagnitude);
        double center = BitConverter.Int32BitsToSingle((int)candidateMagnitude);
        double previous = BitConverter.Int32BitsToSingle((int)candidateMagnitude - 1);
        double next = BitConverter.Int32BitsToSingle((int)candidateMagnitude + 1);
        double lowerMidpoint = (previous + center) * 0.5d;
        double upperMidpoint = (center + next) * 0.5d;

        // An adjacent binary32 midpoint has at most 25 significant bits.
        // Multiplication by a positive normal binary32 denominator needs at
        // most 49 bits, and its exponent lies within binary64's normal range.
        // Both midpoint products and the converted numerator are therefore
        // exact. Strict interior membership proves nearest binary32 rounding
        // independently of the reciprocal approximation; ties use division.
        return magnitude > lowerMidpoint * denominator
            && magnitude < upperMidpoint * denominator;
    }

    private static double[] CreateReciprocalSeeds()
    {
        double[] seeds = new double[256];
        for (int index = 0; index < seeds.Length; index++)
        {
            seeds[index] = 1d / (1d + ((index + 0.5d) / 256d));
        }
        return seeds;
    }
}
