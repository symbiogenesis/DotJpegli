namespace DotJpegli.Internal;

// Offline search operation counts only; these do not measure CPU time or memory.
// Each search candidate supplies its own sink and uses a single encoder worker.
internal sealed class RateDistortionDiagnostics
{
    internal long Blocks { get; private set; }
    internal long CandidateEvaluations { get; private set; }
    internal long ChangedCoefficients { get; private set; }
    internal long FeedbackHistogramBlocks { get; private set; }
    internal long FeedbackBlocks { get; private set; }
    internal long FeedbackEvaluations { get; private set; }
    internal long FeedbackChanges { get; private set; }
    internal long FeedbackOriginalBits { get; private set; }
    internal long FeedbackSelectedBits { get; private set; }
    internal double FeedbackOriginalSquaredError { get; private set; }
    internal double FeedbackSelectedSquaredError { get; private set; }
    internal byte[][]? FeedbackAcCodeLengths { get; private set; }

    internal void RecordBlock(int evaluations, int changes)
    {
        this.Blocks++;
        this.CandidateEvaluations += evaluations;
        this.ChangedCoefficients += changes;
    }

    internal void RecordFeedbackModel(byte[][] lengths, long histogramBlocks)
    {
        this.FeedbackAcCodeLengths = lengths.Select(component => (byte[])component.Clone()).ToArray();
        this.FeedbackHistogramBlocks += histogramBlocks;
    }

    internal void RecordFeedbackBlock(int evaluations, int changes, int originalBits, int selectedBits,
        double originalError, double selectedError)
    {
        this.FeedbackBlocks++;
        this.FeedbackEvaluations += evaluations;
        this.FeedbackChanges += changes;
        this.FeedbackOriginalBits += originalBits;
        this.FeedbackSelectedBits += selectedBits;
        this.FeedbackOriginalSquaredError += originalError;
        this.FeedbackSelectedSquaredError += selectedError;
    }
}
