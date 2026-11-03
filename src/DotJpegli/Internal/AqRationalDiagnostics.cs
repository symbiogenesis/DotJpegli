namespace DotJpegli.Internal;

internal enum AqRationalSite
{
    Mask,
    Derivative,
    Log2,
    Pow2,
    Strength
}

// One exclusively owned sink per synchronous analysis/encode. No timing,
// selection, shared state or diagnostic allocation is added to Automatic.
internal sealed class AqRationalDiagnostics
{
    private readonly long[] accepted = new long[5];
    private readonly long[] fallback = new long[5];

    internal long Calls { get; private set; }
    internal long VectorBatches { get; private set; }
    internal long FallbackVectorBatches { get; private set; }
    internal long Vector256Batches { get; private set; }
    internal long FallbackVector256Batches { get; private set; }

    internal long GetAccepted(AqRationalSite site) => this.accepted[(int)site];

    internal long GetFallback(AqRationalSite site) => this.fallback[(int)site];

    internal void Record(AqRationalSite site, bool usedFallback)
    {
        this.Calls++;
        if (usedFallback)
        {
            this.fallback[(int)site]++;
        }
        else
        {
            this.accepted[(int)site]++;
        }
    }

    internal void RecordVector(bool usedFallback, bool vector256)
    {
        if (vector256)
        {
            this.Vector256Batches++;
            if (usedFallback)
            {
                this.FallbackVector256Batches++;
            }
        }
        else
        {
            this.VectorBatches++;
            if (usedFallback)
            {
                this.FallbackVectorBatches++;
            }
        }
    }
}
