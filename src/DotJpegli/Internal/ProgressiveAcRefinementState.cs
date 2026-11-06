// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// One state belongs to one refinement scan. EOB runs must be flushed at scan
// ends and restart boundaries. A byte stores each correction bit so the backing
// buffer contains exactly MaximumCorrectionBits bytes, independent of scan size.
internal sealed class ProgressiveAcRefinementState
{
    public const int MaximumCorrectionBits = 2047;
    private const int InitialCorrectionBits = 255;

    private byte[] corrections = new byte[InitialCorrectionBits];

    public int PendingEobRun { get; private set; }

    public int CorrectionBitCount { get; private set; }

    internal ReadOnlySpan<byte> Corrections => this.corrections.AsSpan(0, this.CorrectionBitCount);

    internal void AppendBlock(ReadOnlySpan<byte> blockCorrections)
    {
        int required = this.CorrectionBitCount + blockCorrections.Length;
        if (required > this.corrections.Length)
        {
            Array.Resize(ref this.corrections, Math.Min(MaximumCorrectionBits,
                Math.Max(required, this.corrections.Length * 2)));
        }

        blockCorrections.CopyTo(this.corrections.AsSpan(this.CorrectionBitCount));
        this.CorrectionBitCount = required;
        this.PendingEobRun++;
    }

    internal void Clear()
    {
        this.PendingEobRun = 0;
        this.CorrectionBitCount = 0;
    }
}
