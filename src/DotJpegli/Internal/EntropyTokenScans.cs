namespace DotJpegli.Internal;

using System.Buffers;

/// <summary>Shares one fixed token budget across every scan and owns their rentals.</summary>
internal sealed class EntropyTokenScans : IDisposable
{
    // One encode collects tokens synchronously: DC scans in script order, then
    // progressive AC scans in component/block order with script-ordered states.
    // Abandoning a scan refunds its rentals for subsequent collection. The
    // shared actual-rental budget has one owner and needs no synchronization.
    private readonly EntropyTokens[] scans;

    internal EntropyTokenScans(int count, long maximumBytes = EntropyTokenBudget.DefaultMaximumBytes, ArrayPool<uint>? pool = null,
        EncoderCacheBudget? cacheBudget = null)
    {
        Guard.ThrowIfNegativeOrZero(count);
        var budget = new EntropyTokenBudget(maximumBytes, cacheBudget);
        this.scans = new EntropyTokens[count];
        for (int index = 0; index < count; index++)
        {
            this.scans[index] = new EntropyTokens(budget, pool: pool);
        }
    }

    internal EntropyTokenScans(IReadOnlyList<JpegScan> script, bool progressive,
        long maximumBytes = EntropyTokenBudget.DefaultMaximumBytes, ArrayPool<uint>? pool = null,
        EncoderCacheBudget? cacheBudget = null)
    {
        Guard.ThrowIfNull(script);
        Guard.ThrowIfNegativeOrZero(script.Count);
        var budget = new EntropyTokenBudget(maximumBytes, cacheBudget);
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
