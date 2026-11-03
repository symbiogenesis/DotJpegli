namespace DotJpegli.Internal;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>Preserves single-round multiply-add semantics on every target.</summary>
internal static class FloatingPointMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float FusedMultiplyAdd(float left, float right, float addend)
#if NETSTANDARD2_1
        => StandardMath.FusedMultiplyAdd(left, right, addend);
#else
        => MathF.FusedMultiplyAdd(left, right, addend);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector<float> FusedMultiplyAdd(Vector<float> left, Vector<float> right, Vector<float> addend)
#if NETSTANDARD2_1
        => StandardMath.FusedMultiplyAdd(left, right, addend);
#else
        => Vector.FusedMultiplyAdd(left, right, addend);
#endif
}
