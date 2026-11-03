namespace DotJpegli.Internal;

#if !NETSTANDARD2_1
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.Intrinsics;
#endif

/// <summary>Expands one big-endian entropy word into its JPEG-stuffed bytes.</summary>
internal static class EntropyByteStuffing
{
    // The caller reserves the worst-case sixteen bytes. The vector schedule
    // writes all sixteen, including unused trailing bytes; only the returned
    // prefix belongs to the entropy stream.
    internal static int StuffWord(ulong word, Span<byte> destination)
    {
#if !NETSTANDARD2_1
        if (Vector128.IsHardwareAccelerated)
        {
            return StuffWordVector(word, destination);
        }
#endif
        return StuffWordOriginal(word, destination);
    }

    internal static int StuffWordOriginal(ulong word, Span<byte> destination)
    {
        if (destination.Length < 16)
        {
            throw new ArgumentException("A stuffed word requires sixteen writable bytes.", nameof(destination));
        }

        int offset = 0;
        for (int shift = 56; shift >= 0; shift -= 8)
        {
            byte value = (byte)(word >> shift);
            destination[offset++] = value;
            if (value == 0xff)
            {
                destination[offset++] = 0;
            }
        }

        return offset;
    }

#if !NETSTANDARD2_1
    // This explicit entry also permits software-vector correctness checks.
    // No CPU-specific instruction set or machine byte order is assumed.
    internal static int StuffWordVector(ulong word, Span<byte> destination)
    {
        if (destination.Length < 16)
        {
            throw new ArgumentException("A stuffed word requires sixteen writable bytes.", nameof(destination));
        }

        ulong ordered = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;
        Vector128<byte> bytes = Vector128.CreateScalar(ordered).AsByte();
        uint mask = Vector128.ExtractMostSignificantBits(Vector128.Equals(bytes, Vector128.Create((byte)0xff)));
        Vector128.Shuffle(bytes, ShuffleMasks.Values[mask]).CopyTo(destination);
        return 8 + BitOperations.PopCount(mask);
    }

    // One immutable array owns 4,096 bytes of vector payload plus its managed
    // array header. Only the vector schedule references this nested table.
    // Invalid shuffle indices emit zero for inserted and unused output bytes.
    private static class ShuffleMasks
    {
        internal static readonly Vector128<byte>[] Values = Create();

        private static Vector128<byte>[] Create()
        {
            var values = new Vector128<byte>[256];
            Span<byte> indices = stackalloc byte[16];
            for (int mask = 0; mask < values.Length; mask++)
            {
                indices.Fill(byte.MaxValue);
                int offset = 0;
                for (int lane = 0; lane < 8; lane++)
                {
                    indices[offset++] = (byte)lane;
                    if ((mask & (1 << lane)) != 0)
                    {
                        offset++;
                    }
                }

                values[mask] = Vector128.Create(indices);
            }

            return values;
        }
    }
#endif
}
