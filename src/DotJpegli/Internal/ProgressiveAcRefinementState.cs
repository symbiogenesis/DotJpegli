// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// One state belongs to one refinement scan. EOB runs must be flushed at scan
// ends and restart boundaries. A byte stores each correction bit so the backing
// buffer contains exactly MaximumCorrectionBits bytes, independent of scan size.
internal sealed class ProgressiveAcRefinementState
{
    // Matches the uint8_t refinement token bit count in pinned Jpegli.
    public const int MaximumCorrectionBits = 255;

    private readonly byte[] corrections = new byte[MaximumCorrectionBits];

    public int PendingEobRun { get; private set; }

    public int CorrectionBitCount { get; private set; }

    internal ReadOnlySpan<byte> Corrections => corrections.AsSpan(0, CorrectionBitCount);

    internal void AppendBlock(ReadOnlySpan<byte> blockCorrections)
    {
        blockCorrections.CopyTo(corrections.AsSpan(CorrectionBitCount));
        CorrectionBitCount += blockCorrections.Length;
        PendingEobRun++;
    }

    internal void Clear()
    {
        PendingEobRun = 0;
        CorrectionBitCount = 0;
    }
}
