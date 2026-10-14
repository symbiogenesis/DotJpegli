namespace DotJpegli.Tests;

// Keep the test oracle as the direct orthonormal cosine transform, independent
// of the encoder's fast DCT. Only its invariant trigonometric terms are cached;
// each coefficient retains the original multiplication and accumulation order.
internal static class IndependentDct
{
    private static readonly double[] Cosines = CreateCosines();
    private static readonly double DcScale = 1 / Math.Sqrt(2);

    public static double Coefficient(ReadOnlySpan<double> samples, int u, int v)
    {
        ReadOnlySpan<double> horizontal = Cosines.AsSpan(u * 8, 8);
        ReadOnlySpan<double> vertical = Cosines.AsSpan(v * 8, 8);
        double sum = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                sum += samples[y * 8 + x] * horizontal[x] * vertical[y];
        return sum * 0.25 * (u == 0 ? DcScale : 1) * (v == 0 ? DcScale : 1);
    }

    private static double[] CreateCosines()
    {
        var cosines = new double[64];
        for (int frequency = 0; frequency < 8; frequency++)
            for (int position = 0; position < 8; position++)
                cosines[frequency * 8 + position] = Math.Cos((2 * position + 1) * frequency * Math.PI / 16);
        return cosines;
    }
}
