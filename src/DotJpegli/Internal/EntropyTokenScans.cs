namespace DotJpegli.Internal;

/// <summary>Divides one fixed token budget across every scan and owns their rentals.</summary>
internal sealed class EntropyTokenScans : IDisposable
{
    private readonly EntropyTokens[] scans;

    internal EntropyTokenScans(int count)
    {
        Guard.ThrowIfNegativeOrZero(count);
        long budgetPerScan = 64L * 1024 * 1024 / count;
        scans = new EntropyTokens[count];
        for (int index = 0; index < count; index++)
            scans[index] = new EntropyTokens(budgetPerScan);
    }

    internal EntropyTokenScans(IReadOnlyList<JpegScan> script, bool progressive)
    {
        Guard.ThrowIfNull(script);
        Guard.ThrowIfNegativeOrZero(script.Count);
        long budgetPerScan = 64L * 1024 * 1024 / script.Count;
        scans = new EntropyTokens[script.Count];
        for (int index = 0; index < scans.Length; index++)
        {
            JpegScan scan = script[index];
            int selectedContexts = (scan.SpectralStart == 0 && (!progressive || scan.SuccessiveHigh == 0) ? 1 : 0)
                | (scan.SpectralEnd > 0 ? 2 : 0);
            int mask = 0;
            foreach (int component in scan.Components)
                mask |= selectedContexts << (component * 2);
            scans[index] = new EntropyTokens(budgetPerScan, mask);
        }
    }

    internal EntropyTokens this[int index] => scans[index];

    public void Dispose()
    {
        foreach (EntropyTokens scan in scans)
            scan.Dispose();
    }
}
