// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotJpegli.Internal;

// Managed scalar adaptation of google/jpegli lib/jpegli/bit_writer.h
// (WriteBits, DischargeBitBuffer, HasZeroByte and EmitByte) and bit_writer.cc
// (JumpToByteBoundary), revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Buffered output accumulates complete 64-bit words and stores eight bytes at
// once unless JPEG byte stuffing is needed. The default remains byte-eager.
internal sealed class BitWriter
{
    private readonly Stream output;
    private readonly byte[]? outputBuffer;
    private int outputCount;
    private ulong buffer;
    private int bufferedBits;

    public BitWriter(Stream output, bool buffered = false)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite)
        {
            throw new ArgumentException("The output stream must be writable.", nameof(output));
        }

        this.output = output;
        outputBuffer = buffered ? new byte[4096] : null;
    }

    public void WriteBits(uint bits, int count)
    {
        if ((uint)count > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "A write must contain 0 to 32 bits.");
        }

        if (count < 32 && (bits >> count) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bits), "The value must fit in the requested bit count.");
        }

        WriteBitsPrevalidated(bits, count);
    }

    // Entropy replay has already checked each code/suffix when constructing
    // the token and table. Keep its common accumulator update small enough to
    // inline; output discharge and eager stream writes stay out of that loop.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBitsPrevalidated(uint bits, int count)
    {
        Debug.Assert((uint)count <= 32 && (count == 32 || (bits >> count) == 0));
        if (outputBuffer is not null)
        {
            int totalBits = bufferedBits + count;
            if (totalBits >= 64)
            {
                WriteSpilledBits(bits, count, totalBits - 64);
            }
            else
            {
                buffer = (buffer << count) | bits;
                bufferedBits = totalBits;
            }
            return;
        }

        WriteEagerBits(bits, count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteSpilledBits(uint bits, int count, int spillBits)
    {
        // Complete the oldest word before retaining the low spill bits.
        // The previous word has at most 63 bits, so the spill is 0..31;
        // no C# shift can wrap a count of 64 back to zero.
        WriteWord((buffer << (count - spillBits)) | (bits >> spillBits));
        buffer = bits & ((1UL << spillBits) - 1);
        bufferedBits = spillBits;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteEagerBits(uint bits, int count)
    {
        // Eager mode preserves the observable complete-byte writes used by
        // callers that do not opt in to batching.
        buffer = (buffer << count) | bits;
        bufferedBits += count;
        while (bufferedBits >= 8)
        {
            bufferedBits -= 8;
            WriteByte((byte)(buffer >> bufferedBits));
        }

        buffer &= (1UL << bufferedBits) - 1;
    }

    public void Flush()
    {
        if (bufferedBits != 0)
        {
            int padding = (8 - (bufferedBits & 7)) & 7;
            if (padding != 0)
                WriteBits((1U << padding) - 1, padding);
            while (bufferedBits >= 8)
            {
                bufferedBits -= 8;
                WriteByte((byte)(buffer >> bufferedBits));
            }
        }

        buffer = 0;
        bufferedBits = 0;
        DrainOutput();
    }

    public void WriteRestartMarker(int index)
    {
        if ((uint)index > 7)
            throw new ArgumentOutOfRangeException(nameof(index));

        // Finish the entropy segment with one-bits (including byte stuffing),
        // then emit the marker itself without entropy byte stuffing.
        Flush();
        output.WriteByte(0xff);
        output.WriteByte((byte)(0xd0 + index));
    }

    private void WriteByte(byte value)
    {
        if (outputBuffer is null)
        {
            output.WriteByte(value);
            if (value == 0xff)
                output.WriteByte(0);
            return;
        }

        // Reserve room for both a sample byte and its possible stuffing byte.
        // Keeping the pair together also leaves any failed write at a complete
        // entropy-byte boundary.
        if (outputCount > outputBuffer.Length - 2)
            DrainOutput();
        outputBuffer[outputCount++] = value;
        if (value == 0xff)
            outputBuffer[outputCount++] = 0;
        if (outputCount == outputBuffer.Length)
            DrainOutput();
    }

    private void WriteWord(ulong word)
    {
        // Upstream DischargeBitBuffer tests for FF once per word. Most words
        // need no stuffing and can use a single big-endian store.
        ulong inverted = ~word;
        if ((unchecked(inverted - 0x0101010101010101UL) & word & 0x8080808080808080UL) == 0)
        {
            if (outputCount > outputBuffer!.Length - 8)
                DrainOutput();
            BinaryPrimitives.WriteUInt64BigEndian(outputBuffer.AsSpan(outputCount), word);
            outputCount += 8;
            if (outputCount == outputBuffer.Length)
                DrainOutput();
            return;
        }

        // The rare stuffing path retains the existing capacity and pair rules.
        WriteByte((byte)(word >> 56));
        WriteByte((byte)(word >> 48));
        WriteByte((byte)(word >> 40));
        WriteByte((byte)(word >> 32));
        WriteByte((byte)(word >> 24));
        WriteByte((byte)(word >> 16));
        WriteByte((byte)(word >> 8));
        WriteByte((byte)word);
    }

    private void DrainOutput()
    {
        if (outputCount == 0)
            return;
        output.Write(outputBuffer.AsSpan(0, outputCount));
        outputCount = 0;
    }
}
