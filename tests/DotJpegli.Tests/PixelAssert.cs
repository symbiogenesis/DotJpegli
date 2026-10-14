using Xunit;

namespace DotJpegli.Tests;

internal static class PixelAssert
{
    public static void Equal(Array expected, Array actual)
    {
        // Keep the element type so xUnit can compare contiguous unmanaged
        // arrays without boxing every sample, retaining its mismatch diagnostics.
        switch (expected)
        {
            case byte[] bytes:
                Assert.Equal(bytes, Assert.IsType<byte[]>(actual));
                break;
            case ushort[] words:
                Assert.Equal(words, Assert.IsType<ushort[]>(actual));
                break;
            case float[] floats:
                Assert.Equal(floats, Assert.IsType<float[]>(actual));
                break;
            default:
                Assert.Fail("Pixel buffers must be byte[], ushort[], or float[].");
                break;
        }
    }
}
