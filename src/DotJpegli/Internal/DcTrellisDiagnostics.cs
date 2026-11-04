namespace DotJpegli.Internal;

// Per-encode, single-worker research counts; no timing or allocation claims.
internal sealed class DcTrellisDiagnostics
{
    internal long Chains { get; private set; }
    internal long SourceTransformBlocks { get; private set; }
    internal long CandidateEvaluations { get; private set; }
    internal long ChangedCoefficients { get; private set; }
    internal long OriginalBits { get; private set; }
    internal long SelectedBits { get; private set; }
    internal double OriginalSquaredError { get; private set; }
    internal double SelectedSquaredError { get; private set; }

    internal void RecordChain(int blocks, DcRateDistortionQuantization.Diagnostics result)
    {
        this.Chains++;
        this.SourceTransformBlocks += blocks;
        this.CandidateEvaluations += result.Evaluations;
        this.ChangedCoefficients += result.ChangedCoefficients;
        this.OriginalBits += result.OriginalBits;
        this.SelectedBits += result.SelectedBits;
        this.OriginalSquaredError += result.OriginalSquaredError;
        this.SelectedSquaredError += result.SelectedSquaredError;
    }
}
