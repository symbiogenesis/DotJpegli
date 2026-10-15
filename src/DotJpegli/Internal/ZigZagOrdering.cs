#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

namespace DotJpegli.Internal;

/// <summary>Reorders one complete DCT block using portable vector shuffles.</summary>
internal static class ZigZagOrdering
{
#if !NETSTANDARD2_1
    private static readonly Vector512<ushort> W00 = CreateWideMask(0, 0);
    private static readonly Vector512<ushort> W01 = CreateWideMask(0, 1);
    private static readonly Vector512<ushort> W10 = CreateWideMask(1, 0);
    private static readonly Vector512<ushort> W11 = CreateWideMask(1, 1);
    private static readonly Vector128<byte> M00 = CreateMask(0, 0);
    private static readonly Vector128<byte> M01 = CreateMask(0, 1);
    private static readonly Vector128<byte> M02 = CreateMask(0, 2);
    private static readonly Vector128<byte> M10 = CreateMask(1, 0);
    private static readonly Vector128<byte> M11 = CreateMask(1, 1);
    private static readonly Vector128<byte> M12 = CreateMask(1, 2);
    private static readonly Vector128<byte> M13 = CreateMask(1, 3);
    private static readonly Vector128<byte> M14 = CreateMask(1, 4);
    private static readonly Vector128<byte> M21 = CreateMask(2, 1);
    private static readonly Vector128<byte> M22 = CreateMask(2, 2);
    private static readonly Vector128<byte> M23 = CreateMask(2, 3);
    private static readonly Vector128<byte> M24 = CreateMask(2, 4);
    private static readonly Vector128<byte> M25 = CreateMask(2, 5);
    private static readonly Vector128<byte> M26 = CreateMask(2, 6);
    private static readonly Vector128<byte> M30 = CreateMask(3, 0);
    private static readonly Vector128<byte> M31 = CreateMask(3, 1);
    private static readonly Vector128<byte> M32 = CreateMask(3, 2);
    private static readonly Vector128<byte> M33 = CreateMask(3, 3);
    private static readonly Vector128<byte> M44 = CreateMask(4, 4);
    private static readonly Vector128<byte> M45 = CreateMask(4, 5);
    private static readonly Vector128<byte> M46 = CreateMask(4, 6);
    private static readonly Vector128<byte> M47 = CreateMask(4, 7);
    private static readonly Vector128<byte> M51 = CreateMask(5, 1);
    private static readonly Vector128<byte> M52 = CreateMask(5, 2);
    private static readonly Vector128<byte> M53 = CreateMask(5, 3);
    private static readonly Vector128<byte> M54 = CreateMask(5, 4);
    private static readonly Vector128<byte> M55 = CreateMask(5, 5);
    private static readonly Vector128<byte> M56 = CreateMask(5, 6);
    private static readonly Vector128<byte> M63 = CreateMask(6, 3);
    private static readonly Vector128<byte> M64 = CreateMask(6, 4);
    private static readonly Vector128<byte> M65 = CreateMask(6, 5);
    private static readonly Vector128<byte> M66 = CreateMask(6, 6);
    private static readonly Vector128<byte> M67 = CreateMask(6, 7);
    private static readonly Vector128<byte> M75 = CreateMask(7, 5);
    private static readonly Vector128<byte> M76 = CreateMask(7, 6);
    private static readonly Vector128<byte> M77 = CreateMask(7, 7);

#endif
    internal static void Reorder(ReadOnlySpan<short> source, Span<short> destination)
    {
        if (source.Length < 64)
            throw new ArgumentException("A DCT block requires 64 coefficients.", nameof(source));
        if (destination.Length < 64)
            throw new ArgumentException("An ordered block requires 64 coefficients.", nameof(destination));
#if !NETSTANDARD2_1
        if (Vector512.IsHardwareAccelerated)
        {
            ReorderVector512(source, destination);
            return;
        }
        if (Vector128.IsHardwareAccelerated)
        {
            ReorderVector(source, destination);
            return;
        }
#endif
        if (source.Overlaps(destination))
        {
            Span<short> original = stackalloc short[64];
            source[..64].CopyTo(original);
            ReorderScalar(original, destination);
        }
        else
            ReorderScalar(source, destination);
    }

#if !NETSTANDARD2_1
    // Two complete source vectors reduce the permutation to four shuffles and
    // two ORs. Consume both before storing to retain arbitrary span overlap.
    // An explicit entry also lets tests check portable software execution.
    internal static void ReorderVector512(ReadOnlySpan<short> source, Span<short> destination)
    {
        source = source[..64];
        destination = destination[..64];
        Vector512<ushort> lower = Vector512.Create(source[..32]).AsUInt16();
        Vector512<ushort> upper = Vector512.Create(source.Slice(32, 32)).AsUInt16();
        (Vector512.Shuffle(lower, W00) | Vector512.Shuffle(upper, W01))
            .AsInt16().CopyTo(destination[..32]);
        (Vector512.Shuffle(lower, W10) | Vector512.Shuffle(upper, W11))
            .AsInt16().CopyTo(destination.Slice(32, 32));
    }

    private static Vector512<ushort> CreateWideMask(int outputGroup, int sourceGroup)
    {
        Span<ushort> mask = stackalloc ushort[32];
        for (int lane = 0; lane < mask.Length; lane++)
        {
            int position = JpegConstants.ZigZag[outputGroup * 32 + lane] - sourceGroup * 32;
            mask[lane] = position is >= 0 and < 32 ? (ushort)position : ushort.MaxValue;
        }
        return Vector512.Create(mask);
    }

    private static void ReorderVector(ReadOnlySpan<short> source, Span<short> destination)
    {
        source = source[..64];
        destination = destination[..64];
        // Keep all source bytes in registers before storing. The same network
        // also supports overlapping spans without a separate block copy.
        Vector128<byte> s0 = Vector128.Create(source.Slice(0, 8)).AsByte();
        Vector128<byte> s1 = Vector128.Create(source.Slice(8, 8)).AsByte();
        Vector128<byte> s2 = Vector128.Create(source.Slice(16, 8)).AsByte();
        Vector128<byte> s3 = Vector128.Create(source.Slice(24, 8)).AsByte();
        Vector128<byte> s4 = Vector128.Create(source.Slice(32, 8)).AsByte();
        Vector128<byte> s5 = Vector128.Create(source.Slice(40, 8)).AsByte();
        Vector128<byte> s6 = Vector128.Create(source.Slice(48, 8)).AsByte();
        Vector128<byte> s7 = Vector128.Create(source.Slice(56, 8)).AsByte();
        (Vector128.Shuffle(s0, M00)
            | Vector128.Shuffle(s1, M01)
            | Vector128.Shuffle(s2, M02)).AsInt16().CopyTo(destination.Slice(0, 8));
        (Vector128.Shuffle(s0, M10)
            | Vector128.Shuffle(s1, M11)
            | Vector128.Shuffle(s2, M12)
            | Vector128.Shuffle(s3, M13)
            | Vector128.Shuffle(s4, M14)).AsInt16().CopyTo(destination.Slice(8, 8));
        (Vector128.Shuffle(s1, M21)
            | Vector128.Shuffle(s2, M22)
            | Vector128.Shuffle(s3, M23)
            | Vector128.Shuffle(s4, M24)
            | Vector128.Shuffle(s5, M25)
            | Vector128.Shuffle(s6, M26)).AsInt16().CopyTo(destination.Slice(16, 8));
        (Vector128.Shuffle(s0, M30)
            | Vector128.Shuffle(s1, M31)
            | Vector128.Shuffle(s2, M32)
            | Vector128.Shuffle(s3, M33)).AsInt16().CopyTo(destination.Slice(24, 8));
        (Vector128.Shuffle(s4, M44)
            | Vector128.Shuffle(s5, M45)
            | Vector128.Shuffle(s6, M46)
            | Vector128.Shuffle(s7, M47)).AsInt16().CopyTo(destination.Slice(32, 8));
        (Vector128.Shuffle(s1, M51)
            | Vector128.Shuffle(s2, M52)
            | Vector128.Shuffle(s3, M53)
            | Vector128.Shuffle(s4, M54)
            | Vector128.Shuffle(s5, M55)
            | Vector128.Shuffle(s6, M56)).AsInt16().CopyTo(destination.Slice(40, 8));
        (Vector128.Shuffle(s3, M63)
            | Vector128.Shuffle(s4, M64)
            | Vector128.Shuffle(s5, M65)
            | Vector128.Shuffle(s6, M66)
            | Vector128.Shuffle(s7, M67)).AsInt16().CopyTo(destination.Slice(48, 8));
        (Vector128.Shuffle(s5, M75)
            | Vector128.Shuffle(s6, M76)
            | Vector128.Shuffle(s7, M77)).AsInt16().CopyTo(destination.Slice(56, 8));
    }

    private static Vector128<byte> CreateMask(int outputGroup, int sourceGroup)
    {
        Span<byte> mask = stackalloc byte[16];
        for (int lane = 0; lane < 8; lane++)
        {
            int position = JpegConstants.ZigZag[outputGroup * 8 + lane] - sourceGroup * 8;
            // Portable Shuffle zeros out-of-range indices. Otherwise select both
            // bytes of the requested short, preserving every signed bit pattern.
            mask[lane * 2] = position is >= 0 and < 8 ? (byte)(position * 2) : byte.MaxValue;
            mask[lane * 2 + 1] = position is >= 0 and < 8 ? (byte)(position * 2 + 1) : byte.MaxValue;
        }
        return Vector128.Create(mask);
    }

#else
    internal static void ReorderVector512(ReadOnlySpan<short> source, Span<short> destination)
        => Reorder(source, destination);

#endif
    private static void ReorderScalar(ReadOnlySpan<short> source, Span<short> destination)
    {
        // Fixed-length slices let the JIT eliminate every constant-index bound
        // check. This path retains exact ordering on other architectures.
        source = source[..64];
        destination = destination[..64];
        destination[0] = source[0];
        destination[1] = source[1];
        destination[2] = source[8];
        destination[3] = source[16];
        destination[4] = source[9];
        destination[5] = source[2];
        destination[6] = source[3];
        destination[7] = source[10];
        destination[8] = source[17];
        destination[9] = source[24];
        destination[10] = source[32];
        destination[11] = source[25];
        destination[12] = source[18];
        destination[13] = source[11];
        destination[14] = source[4];
        destination[15] = source[5];
        destination[16] = source[12];
        destination[17] = source[19];
        destination[18] = source[26];
        destination[19] = source[33];
        destination[20] = source[40];
        destination[21] = source[48];
        destination[22] = source[41];
        destination[23] = source[34];
        destination[24] = source[27];
        destination[25] = source[20];
        destination[26] = source[13];
        destination[27] = source[6];
        destination[28] = source[7];
        destination[29] = source[14];
        destination[30] = source[21];
        destination[31] = source[28];
        destination[32] = source[35];
        destination[33] = source[42];
        destination[34] = source[49];
        destination[35] = source[56];
        destination[36] = source[57];
        destination[37] = source[50];
        destination[38] = source[43];
        destination[39] = source[36];
        destination[40] = source[29];
        destination[41] = source[22];
        destination[42] = source[15];
        destination[43] = source[23];
        destination[44] = source[30];
        destination[45] = source[37];
        destination[46] = source[44];
        destination[47] = source[51];
        destination[48] = source[58];
        destination[49] = source[59];
        destination[50] = source[52];
        destination[51] = source[45];
        destination[52] = source[38];
        destination[53] = source[31];
        destination[54] = source[39];
        destination[55] = source[46];
        destination[56] = source[53];
        destination[57] = source[60];
        destination[58] = source[61];
        destination[59] = source[54];
        destination[60] = source[47];
        destination[61] = source[55];
        destination[62] = source[62];
        destination[63] = source[63];
    }
}

