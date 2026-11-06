// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

using System.Buffers;

// One state belongs to one refinement scan. EOB runs must be flushed at scan
// ends and restart boundaries. A byte stores each correction bit so the backing
// buffer contains exactly MaximumCorrectionBits bytes, independent of scan size.
internal sealed class ProgressiveAcRefinementState(bool pooled = false) : IDisposable
{
    public const int MaximumCorrectionBits = 2047;
    private const int InitialCorrectionBits = 255;

    private byte[] corrections = [];
    private bool rented;

    public int PendingEobRun { get; private set; }

    public int CorrectionBitCount { get; private set; }

    internal ReadOnlySpan<byte> Corrections => this.corrections.AsSpan(0, this.CorrectionBitCount);

    internal void AppendBlock(ReadOnlySpan<byte> blockCorrections)
    {
        int required = this.CorrectionBitCount + blockCorrections.Length;
        if (required > this.corrections.Length)
        {
            if (pooled && !this.rented)
            {
                this.corrections = ArrayPool<byte>.Shared.Rent(MaximumCorrectionBits);
                this.rented = true;
            }
            else
            {
                Array.Resize(ref this.corrections, Math.Min(MaximumCorrectionBits,
                    Math.Max(required, Math.Max(InitialCorrectionBits, this.corrections.Length * 2))));
            }
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

    public void Dispose()
    {
        if (this.rented)
        {
            byte[] toReturn = this.corrections;
            this.corrections = [];
            this.rented = false;
            ArrayPool<byte>.Shared.Return(toReturn);
        }
    }
}
