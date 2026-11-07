namespace DotJpegli.Internal;

using System.Numerics;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_1
using System.Runtime.InteropServices;
#endif

internal static class PortableVector
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector<float> Load(ReadOnlySpan<float> source) =>
#if NETSTANDARD2_1
        source.Length < Vector<float>.Count
            ? throw new ArgumentOutOfRangeException(nameof(source))
            : MemoryMarshal.Cast<float, Vector<float>>(source)[0];
#else
        new(source);
#endif

#if NETSTANDARD2_1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CopyTo<T>(this Vector<T> vector, Span<T> destination) where T : struct
    {
        if (destination.Length < Vector<T>.Count)
        {
            throw new ArgumentException("The destination must hold a complete vector.", nameof(destination));
        }

        MemoryMarshal.Cast<T, Vector<T>>(destination)[0] = vector;
    }
#endif
}
