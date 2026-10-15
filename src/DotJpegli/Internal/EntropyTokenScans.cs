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

    internal EntropyTokens this[int index] => scans[index];

    public void Dispose()
    {
        foreach (EntropyTokens scan in scans)
            scan.Dispose();
    }
}
