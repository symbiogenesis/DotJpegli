using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotJpegli.Internal;

internal static class PortableVector
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector<float> Load(ReadOnlySpan<float> source)
    {
#if NETSTANDARD2_1
        Span<float> values = stackalloc float[Vector<float>.Count];
        source[..values.Length].CopyTo(values);
        return new Vector<float>(values);
#else
        return new Vector<float>(source);
#endif
    }

#if NETSTANDARD2_1
    internal static void CopyTo<T>(this Vector<T> vector, Span<T> destination) where T : struct
    {
        if (destination.Length < Vector<T>.Count)
            throw new ArgumentException("The destination must hold a complete vector.", nameof(destination));
        for (int lane = 0; lane < Vector<T>.Count; lane++)
            destination[lane] = vector[lane];
    }
#endif
}
