namespace DotJpegli.Internal;

using System.Diagnostics;

/// <summary>Accounts accepted token rentals across the serialized scans of one encode.</summary>
internal sealed class EntropyTokenBudget
{
    internal const long DefaultMaximumBytes = 64L * 1024 * 1024;
    private long rentedBytes;

    internal EntropyTokenBudget(long maximumBytes = DefaultMaximumBytes)
    {
        Guard.ThrowIfNegative(maximumBytes);
        this.MaximumBytes = Math.Min(maximumBytes, DefaultMaximumBytes);
    }

    internal long MaximumBytes { get; }
    internal long AvailableBytes => this.MaximumBytes - this.rentedBytes;

    internal bool TryReserve(long bytes)
    {
        Debug.Assert(bytes >= 0);
        if (bytes > this.AvailableBytes)
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
    }
}
