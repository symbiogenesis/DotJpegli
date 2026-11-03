namespace DotJpegli.Internal;

using System.Diagnostics;

/// <summary>Accounts accepted token rentals across the serialized scans of one encode.</summary>
internal sealed class EntropyTokenBudget
{
    internal const long DefaultMaximumBytes = 64L * 1024 * 1024;
    private long rentedBytes;
    private readonly EncoderCacheBudget? cacheBudget;

    internal EntropyTokenBudget(long maximumBytes = DefaultMaximumBytes, EncoderCacheBudget? cacheBudget = null)
    {
        Guard.ThrowIfNegative(maximumBytes);
        this.MaximumBytes = Math.Min(maximumBytes, DefaultMaximumBytes);
        this.cacheBudget = cacheBudget;
    }

    internal long MaximumBytes { get; }
    internal long AvailableBytes => Math.Min(this.MaximumBytes - this.rentedBytes, this.cacheBudget?.AvailableBytes ?? long.MaxValue);

    internal bool TryReserve(long bytes)
    {
        Debug.Assert(bytes >= 0);
        if (bytes > this.AvailableBytes)
        {
            return false;
        }

        if (this.cacheBudget is not null && !this.cacheBudget.TryReserve(bytes, EncoderCacheKind.EntropyTokens))
        {
            return false;
        }

        this.rentedBytes += bytes;
        return true;
    }

    internal void Release(long bytes)
    {
        Debug.Assert(bytes >= 0 && bytes <= this.rentedBytes);
        this.rentedBytes -= bytes;
        this.cacheBudget?.Release(bytes, EncoderCacheKind.EntropyTokens);
    }
}
