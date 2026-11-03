namespace DotJpegli.Internal;

// Offline search operation counts only; these do not measure CPU time or memory.
// Each search candidate supplies its own sink and uses a single encoder worker.
internal sealed class RateDistortionDiagnostics
{
    internal long Blocks { get; private set; }
    internal long CandidateEvaluations { get; private set; }
    internal long ChangedCoefficients { get; private set; }

    internal void RecordBlock(int evaluations, int changes)
    {
        this.Blocks++;
        this.CandidateEvaluations += evaluations;
        this.ChangedCoefficients += changes;
    }
}
