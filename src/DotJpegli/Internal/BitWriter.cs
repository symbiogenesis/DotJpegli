// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// Managed scalar adaptation of google/jpegli lib/jpegli/bit_writer.h
// (WriteBits, DischargeBitBuffer, HasZeroByte and EmitByte) and bit_writer.cc
// (JumpToByteBoundary), revision 031a0077f5799a6041004267fc12b956c1f52a20.
// Buffered output accumulates complete 64-bit words and stores eight bytes at
// once unless JPEG byte stuffing is needed. The default remains byte-eager.
internal sealed class BitWriter : IDisposable
{
    private const int BufferCapacity = 4096;
    private readonly Stream output;
    private readonly bool vectorStuffing;
    private byte[]? outputBuffer;
    private int outputCount;
    private ulong buffer;
    private int bufferedBits;

    public BitWriter(Stream output, bool buffered = false,
        KernelPreference stuffingKernel = KernelPreference.Automatic)
    {
        Guard.ThrowIfNull(output);
        if (!output.CanWrite)
        {
            throw new ArgumentException("The output stream must be writable.", nameof(output));
        }

        this.output = output;
        this.outputBuffer = buffered ? ArrayPool<byte>.Shared.Rent(BufferCapacity) : null;
        // Automatic retains the measured control until a portable deployment
        // rule is established. Explicit choices belong to this writer only.
        this.vectorStuffing = stuffingKernel switch
        {
            KernelPreference.Automatic or KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(stuffingKernel))
        };
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.outputBuffer, null) is { } rented)
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
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

        this.WriteBitsPrevalidated(bits, count);
    }

    // Entropy replay has already checked each code/suffix when constructing
    // the token and table. Keep its common accumulator update small enough to
    // inline; output discharge and eager stream writes stay out of that loop.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBitsPrevalidated(uint bits, int count)
    {
        Debug.Assert((uint)count <= 32 && (count == 32 || (bits >> count) == 0));
        if (this.outputBuffer is not null)
        {
            int totalBits = this.bufferedBits + count;
            if (totalBits >= 64)
            {
                this.WriteSpilledBits(bits, count, totalBits - 64);
            }
            else
            {
                this.buffer = (this.buffer << count) | bits;
                this.bufferedBits = totalBits;
            }
            return;
        }

        this.WriteEagerBits(bits, count);
    }

    // Token replay normally emits a long run of prebiased Huffman codes. Keep
    // its accumulator in locals until a word is discharged instead of loading
    // and storing the writer fields, and testing its mode, for every symbol.
    // Raw correction bits share that accumulator. Restart records and
    // noncanonical suffix widths stop at an exact prefix so the caller can
    // preserve the strict generic write behavior.
    internal int WriteEntropyTokens(ReadOnlySpan<uint> tokens, ReadOnlySpan<ulong> lookups, int lookupOffset = 0)
    {
        if (this.outputBuffer is null)
        {
            return 0;
        }

        if (lookups.Length > 6 * 256 || (lookups.Length & 255) != 0)
        {
            throw new ArgumentException("Lookup storage must contain zero to six complete Huffman contexts.", nameof(lookups));
        }
        if ((uint)lookupOffset > (uint)((6 * 256) - lookups.Length) || (lookupOffset & 255) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookupOffset));
        }

        // Sequential scans use all six contexts. Keep their original constant
        // lookup bound and unbiased index instead of paying for progressive
        // scan offsets on every token. Specialize once per replay batch.
        return lookups.Length == 6 * 256
            ? this.WriteEntropyTokens<FullContextLookup>(tokens, lookups, 0)
            : this.WriteEntropyTokens<PartialContextLookup>(tokens, lookups, lookupOffset);
    }

    private readonly struct FullContextLookup
    {
    }

    private readonly struct PartialContextLookup
    {
    }

    private int WriteEntropyTokens<TLookup>(ReadOnlySpan<uint> tokens, ReadOnlySpan<ulong> lookups, int lookupOffset)
        where TLookup : struct
    {
        if (typeof(TLookup) == typeof(FullContextLookup))
        {
            lookups = lookups[..(6 * 256)];
        }

        ulong pending = this.buffer;
        int pendingBits = this.bufferedBits;
        int index = 0;
        for (; index < tokens.Length; index++)
        {
            uint token = tokens[index];
            int contextAndSymbol = (int)(token >> 16) & 2047;
            int lookupIndex = typeof(TLookup) == typeof(FullContextLookup)
                ? contextAndSymbol : contextAndSymbol - lookupOffset;
            uint bits;
            int count;
            if ((uint)lookupIndex < (uint)lookups.Length)
            {
                ulong prepared = lookups[lookupIndex];
                if ((int)(prepared >> 38) != (int)(token >> 27) + 1)
                {
                    break;
                }

                bits = (uint)prepared | (token & 0xffffU);
                count = (int)(prepared >> 32) & 63;
            }
            else if (contextAndSymbol is >= (6 * 256) and < (7 * 256))
            {
                // AddBits already split wider payloads into at most 24 bits.
                // Progressive corrections no longer terminate every batch.
                bits = token & 0xffffffU;
                count = (int)(token >> 27);
            }
            else
            {
                break;
            }

            int totalBits = pendingBits + count;
            if (totalBits >= 64)
            {
                int spillBits = totalBits - 64;
                ulong word = (pending << (count - spillBits)) | (bits >> spillBits);
                if (this.outputCount <= BufferCapacity - 16
                    && (unchecked(~word - 0x0101010101010101UL) & word & 0x8080808080808080UL) == 0)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(this.outputBuffer.AsSpan(this.outputCount, sizeof(ulong)), word);
                    this.outputCount += 8;
                }
                else
                {
                    // Preserve the same pending prefix if the destination throws.
                    this.buffer = pending;
                    this.bufferedBits = pendingBits;
                    this.WriteWord(word);
                }

                pending = bits & ((1UL << spillBits) - 1);
                pendingBits = spillBits;
            }
            else
            {
                pending = (pending << count) | bits;
                pendingBits = totalBits;
            }
        }
        this.buffer = pending;
        this.bufferedBits = pendingBits;
        return index;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteSpilledBits(uint bits, int count, int spillBits)
    {
        // Complete the oldest word before retaining the low spill bits.
        // The previous word has at most 63 bits, so the spill is 0..31;
        // no C# shift can wrap a count of 64 back to zero.
        this.WriteWord((this.buffer << (count - spillBits)) | (bits >> spillBits));
        this.buffer = bits & ((1UL << spillBits) - 1);
        this.bufferedBits = spillBits;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteEagerBits(uint bits, int count)
    {
        // Eager mode preserves the observable complete-byte writes used by
        // callers that do not opt in to batching.
        this.buffer = (this.buffer << count) | bits;
        this.bufferedBits += count;
        while (this.bufferedBits >= 8)
        {
            this.bufferedBits -= 8;
            this.WriteByte((byte)(this.buffer >> this.bufferedBits));
        }

        this.buffer &= (1UL << this.bufferedBits) - 1;
    }

    public void Flush()
    {
        if (this.bufferedBits != 0)
        {
            int padding = (8 - (this.bufferedBits & 7)) & 7;
            if (padding != 0)
            {
                this.WriteBits((1U << padding) - 1, padding);
            }

            while (this.bufferedBits >= 8)
            {
                this.bufferedBits -= 8;
                this.WriteByte((byte)(this.buffer >> this.bufferedBits));
            }
        }

        this.buffer = 0;
        this.bufferedBits = 0;
        this.DrainOutput();
    }

    public void WriteRestartMarker(int index)
    {
        if ((uint)index > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        // Finish the entropy segment with one-bits (including byte stuffing),
        // then emit the marker itself without entropy byte stuffing.
        this.Flush();
        this.output.WriteByte(0xff);
        this.output.WriteByte((byte)(0xd0 + index));
    }

    private void WriteByte(byte value)
    {
        if (this.outputBuffer is null)
        {
            this.output.WriteByte(value);
            if (value == 0xff)
            {
                this.output.WriteByte(0);
            }

            return;
        }

        // Reserve room for both a sample byte and its possible stuffing byte.
        // Keeping the pair together also leaves any failed write at a complete
        // entropy-byte boundary.
        if (this.outputCount > BufferCapacity - 2)
        {
            this.DrainOutput();
        }

        this.outputBuffer[this.outputCount++] = value;
        if (value == 0xff)
        {
            this.outputBuffer[this.outputCount++] = 0;
        }

        if (this.outputCount == BufferCapacity)
        {
            this.DrainOutput();
        }
    }

    private void WriteWord(ulong word)
    {
        // Upstream DischargeBitBuffer tests for FF once per word. Most words
        // need no stuffing and can use a single big-endian store.
        ulong inverted = ~word;
        if ((unchecked(inverted - 0x0101010101010101UL) & word & 0x8080808080808080UL) == 0)
        {
            if (this.outputCount > BufferCapacity - 8)
            {
                this.DrainOutput();
            }

            BinaryPrimitives.WriteUInt64BigEndian(this.outputBuffer.AsSpan(this.outputCount, sizeof(ulong)), word);
            this.outputCount += 8;
            if (this.outputCount == BufferCapacity)
            {
                this.DrainOutput();
            }

            return;
        }

        // A stuffed word needs at most sixteen bytes. When all of them fit,
        // keep the index local and check capacity once for the complete word.
        // The boundary path below retains the byte/pair drain rules.
        byte[] destination = this.outputBuffer!;
        int offset = this.outputCount;
        if (offset <= BufferCapacity - 16)
        {
            if (this.vectorStuffing)
            {
                offset += EntropyByteStuffing.StuffWord(word, destination.AsSpan(offset, 16));
            }
            else
            {
                for (int shift = 56; shift >= 0; shift -= 8)
                {
                    byte value = (byte)(word >> shift);
                    destination[offset++] = value;
                    if (value == 0xff)
                    {
                        destination[offset++] = 0;
                    }
                }
            }

            this.outputCount = offset;
            if (offset == BufferCapacity)
            {
                this.DrainOutput();
            }

            return;
        }

        this.WriteByte((byte)(word >> 56));
        this.WriteByte((byte)(word >> 48));
        this.WriteByte((byte)(word >> 40));
        this.WriteByte((byte)(word >> 32));
        this.WriteByte((byte)(word >> 24));
        this.WriteByte((byte)(word >> 16));
        this.WriteByte((byte)(word >> 8));
        this.WriteByte((byte)word);
    }

    private void DrainOutput()
    {
        if (this.outputCount == 0)
        {
            return;
        }

        this.output.Write(this.outputBuffer.AsSpan(0, this.outputCount));
        this.outputCount = 0;
    }
}
