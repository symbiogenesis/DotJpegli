namespace DotJpegli.Internal;

using System.Buffers;

/// <summary>Shares one fixed token budget across every scan and owns their rentals.</summary>
internal sealed class EntropyTokenScans : IDisposable
{
    // Histogram collection visits scans in script order. Earlier scans retain
    // their complete sequences first; abandoning one refunds its rentals for
    // later scans. The budget has one encode owner and needs no synchronization.
    private readonly EntropyTokens[] scans;

    internal EntropyTokenScans(int count, long maximumBytes = EntropyTokenBudget.DefaultMaximumBytes, ArrayPool<uint>? pool = null)
    {
        Guard.ThrowIfNegativeOrZero(count);
        var budget = new EntropyTokenBudget(maximumBytes);
        this.scans = new EntropyTokens[count];
        for (int index = 0; index < count; index++)
        {
            this.scans[index] = new EntropyTokens(budget, pool: pool);
        }
    }

    internal EntropyTokenScans(IReadOnlyList<JpegScan> script, bool progressive,
        long maximumBytes = EntropyTokenBudget.DefaultMaximumBytes, ArrayPool<uint>? pool = null)
    {
        Guard.ThrowIfNull(script);
        Guard.ThrowIfNegativeOrZero(script.Count);
        var budget = new EntropyTokenBudget(maximumBytes);
        this.scans = new EntropyTokens[script.Count];
        for (int index = 0; index < this.scans.Length; index++)
        {
            JpegScan scan = script[index];
            int selectedContexts = (scan.SpectralStart == 0 && (!progressive || scan.SuccessiveHigh == 0) ? 1 : 0)
                | (scan.SpectralEnd > 0 ? 2 : 0);
            int mask = 0;
            foreach (int component in scan.Components)
            {
                mask |= selectedContexts << (component * 2);
            }

            this.scans[index] = new EntropyTokens(budget, mask, pool);
        }
    }

    internal EntropyTokens this[int index] => this.scans[index];

    public void Dispose()
    {
        foreach (EntropyTokens scan in this.scans)
        {
            scan.Dispose();
        }
    }
}
