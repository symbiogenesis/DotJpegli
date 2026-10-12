// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// Managed scalar adaptation of google/jpegli lib/jpegli/bit_writer.h
// (WriteBits and EmitByte) and bit_writer.cc (JumpToByteBoundary), revision
// 031a0077f5799a6041004267fc12b956c1f52a20. Emits complete bytes eagerly.
internal sealed class BitWriter
{
    private readonly Stream output;
    private ulong buffer;
    private int bufferedBits;

    public BitWriter(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite)
        {
            throw new ArgumentException("The output stream must be writable.", nameof(output));
        }

        this.output = output;
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

        // At most seven bits remain after each write, so 32 added bits fit
        // comfortably in this managed 64-bit accumulator.
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
            int padding = 8 - bufferedBits;
            WriteBits((1U << padding) - 1, padding);
        }

        buffer = 0;
        bufferedBits = 0;
    }

    private void WriteByte(byte value)
    {
        output.WriteByte(value);
        if (value == 0xff)
        {
            output.WriteByte(0);
        }
    }
}
