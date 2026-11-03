namespace DotJpegli.Internal;

using System.Diagnostics;

internal enum EncoderCacheKind
{
    ConvertedSamples,
    QuantizedCoefficients,
    EntropyTokens
}

/// <summary>Accounts image-cache storage owned by one synchronous encode.</summary>
internal sealed class EncoderCacheBudget
{
    internal const long DefaultMaximumBytes = 192L * 1024 * 1024;
    private readonly EncoderCacheDiagnostics? diagnostics;
    private long rentedBytes;

    internal EncoderCacheBudget(long maximumBytes, EncoderCacheDiagnostics? diagnostics = null)
    {
        Guard.ThrowIfNegative(maximumBytes);
        this.MaximumBytes = maximumBytes;
        this.diagnostics = diagnostics;
    }

    internal long MaximumBytes { get; }
    internal long AvailableBytes => this.MaximumBytes - this.rentedBytes;

    internal bool TryReserve(long bytes, EncoderCacheKind kind)
    {
        Debug.Assert(bytes >= 0);
        if (bytes > this.AvailableBytes)
        {
            return false;
        }

        this.rentedBytes += bytes;
        this.diagnostics?.Reserve(bytes, kind);
        return true;
    }

    internal void Release(long bytes, EncoderCacheKind kind)
    {
        Debug.Assert(bytes >= 0 && bytes <= this.rentedBytes);
        this.rentedBytes -= bytes;
        this.diagnostics?.Release(bytes, kind);
    }
}

/// <summary>Optional per-encode observation; allocation traffic and process memory are separate metrics.</summary>
internal sealed class EncoderCacheDiagnostics
{
    private long convertedSampleBytes;
    private long coefficientBytes;
    private long entropyTokenBytes;

    internal long CurrentBytes { get; private set; }
    internal long PeakBytes { get; private set; }
    internal long ConvertedSamplePeakBytes { get; private set; }
    internal long CoefficientPeakBytes { get; private set; }
    internal long EntropyTokenPeakBytes { get; private set; }

    internal void Reserve(long bytes, EncoderCacheKind kind)
    {
        this.CurrentBytes += bytes;
        this.PeakBytes = Math.Max(this.PeakBytes, this.CurrentBytes);
        switch (kind)
        {
            case EncoderCacheKind.ConvertedSamples:
                this.convertedSampleBytes += bytes;
                this.ConvertedSamplePeakBytes = Math.Max(this.ConvertedSamplePeakBytes, this.convertedSampleBytes);
                break;
            case EncoderCacheKind.QuantizedCoefficients:
                this.coefficientBytes += bytes;
                this.CoefficientPeakBytes = Math.Max(this.CoefficientPeakBytes, this.coefficientBytes);
                break;
            case EncoderCacheKind.EntropyTokens:
                this.entropyTokenBytes += bytes;
                this.EntropyTokenPeakBytes = Math.Max(this.EntropyTokenPeakBytes, this.entropyTokenBytes);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    internal void Release(long bytes, EncoderCacheKind kind)
    {
        this.CurrentBytes -= bytes;
        switch (kind)
        {
            case EncoderCacheKind.ConvertedSamples:
                this.convertedSampleBytes -= bytes;
                break;
            case EncoderCacheKind.QuantizedCoefficients:
                this.coefficientBytes -= bytes;
                break;
            case EncoderCacheKind.EntropyTokens:
                this.entropyTokenBytes -= bytes;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
