using System.Runtime.CompilerServices;

namespace DotJpegli.Internal;

// Diagnostic preferences are carried by immutable encode options and captured
// by each analysis/quantizer. Independent encodes never share a mutable choice.
internal enum KernelPreference
{
    Automatic,
    Original,
    Optimized
}

internal static class KernelPolicy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool UseSelectionNetwork(KernelPreference preference, int width)
        => preference switch
        {
            // Retain the established kernel until isolated interleaved CI
            // measurements justify a hardware/input rule for Automatic.
            KernelPreference.Automatic => false,
            KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(preference))
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool UseTiledZigZag(KernelPreference preference)
        => preference switch
        {
            KernelPreference.Automatic => false,
            KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(preference))
        };
}
