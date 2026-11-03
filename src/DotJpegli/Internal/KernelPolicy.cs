namespace DotJpegli.Internal;

#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_1
using System.Runtime.InteropServices;
#endif
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

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
    // A conservative minimum-work deployment guard, not a measured crossover.
    // Interleaved CI favors tiles at 49,601/393,216 pixels; 323-pixel results
    // are inconclusive. Keep small inputs and unsupported vectors on Original.
    internal const int MinimumTiledPixels = 32 * 1024;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool UseSelectionNetwork(KernelPreference preference, int _)
        => preference switch
        {
            // ARM64 and x64 comparisons do not establish a portable AQ guard.
            // Retain both variants without fitting a rule to individual images.
            KernelPreference.Automatic => false,
            KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(preference))
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool UseTiledZigZag(KernelPreference preference, int width = 0, int height = 0)
        => preference switch
        {
            KernelPreference.Automatic => AutomaticTiledZigZag(width, height),
            KernelPreference.Original => false,
            KernelPreference.Optimized => true,
            _ => throw new ArgumentOutOfRangeException(nameof(preference))
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AutomaticTiledZigZag(int width, int height)
    {
#if NETSTANDARD2_1
        return false;
#else
        if (width < 8 || height < 8 || (long)width * height < MinimumTiledPixels)
        {
            return false;
        }

        return UseTiledZigZagForConfiguration(width, height, Vector.IsHardwareAccelerated,
            Vector<float>.Count, Vector256.IsHardwareAccelerated, Vector512.IsHardwareAccelerated,
            RuntimeInformation.ProcessArchitecture == Architecture.X64);
#endif
    }

    // Explicit capabilities make the policy testable on every development host.
    internal static bool UseTiledZigZagForConfiguration(int width, int height, bool accelerated,
        int floatLanes, bool vector256, bool vector512, bool x64)
        => width >= 8 && height >= 8 && (long)width * height >= MinimumTiledPixels
            && x64 && accelerated && floatLanes == 8 && vector256 && !vector512;
}
