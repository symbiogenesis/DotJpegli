using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class KernelDispatchTests
{
    [Theory]
    [InlineData(false, ChromaSubsampling.None)]
    [InlineData(false, ChromaSubsampling.Quarter)]
    [InlineData(true, ChromaSubsampling.None)]
    [InlineData(true, ChromaSubsampling.Quarter)]
    public void EveryKernelCombinationPreservesJpegBytes(bool progressive, ChromaSubsampling sampling)
    {
        foreach ((int width, int height) in new[] { (1, 1), (17, 35), (67, 35), (128, 255), (128, 256), (129, 255) })
        {
            byte[] pixels = Pattern(width, height);
            foreach (int workers in new[] { 1, 2 })
            {
                var settings = new JpegliEncoderOptions
                {
                    Progressive = progressive, Subsampling = sampling,
                    MaxDegreeOfParallelism = workers, SmoothingFactor = workers == 1 ? 0 : 27,
                    FuzzyErosionKernel = KernelPreference.Original,
                    NarrowZigZagKernel = KernelPreference.Original
                };
                byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: settings,
                    cancellationToken: TestContext.Current.CancellationToken);
                foreach (KernelPreference erosion in Enum.GetValues<KernelPreference>())
                    foreach (KernelPreference zigzag in Enum.GetValues<KernelPreference>())
                        Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height,
                            options: settings with { FuzzyErosionKernel = erosion, NarrowZigZagKernel = zigzag },
                            cancellationToken: TestContext.Current.CancellationToken));
            }
        }
    }

    [Fact]
    public async Task ConcurrentEncodesKeepTheirOwnKernelPreferences()
    {
        const int width = 67, height = 35;
        byte[] pixels = Pattern(width, height);
        var settings = new JpegliEncoderOptions { Progressive = true, MaxDegreeOfParallelism = 2 };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: settings,
            cancellationToken: TestContext.Current.CancellationToken);
        Task<byte[]>[] encodes = Enum.GetValues<KernelPreference>().SelectMany(erosion =>
            Enum.GetValues<KernelPreference>().Select(zigzag => Task.Run(() =>
                JpegliEncoder.Encode(pixels, width, height,
                    options: settings with { FuzzyErosionKernel = erosion, NarrowZigZagKernel = zigzag },
                    cancellationToken: TestContext.Current.CancellationToken),
                TestContext.Current.CancellationToken))).ToArray();
        foreach (byte[] actual in await Task.WhenAll(encodes))
            Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExplicitPreferencesAreIndependentOfAutomaticPolicy()
    {
        foreach (int width in new[] { 1, 17, 67, 65535 })
        {
            Assert.False(KernelPolicy.UseSelectionNetwork(KernelPreference.Original, width));
            Assert.True(KernelPolicy.UseSelectionNetwork(KernelPreference.Optimized, width));
        }
        Assert.False(KernelPolicy.UseTiledZigZag(KernelPreference.Original));
        Assert.True(KernelPolicy.UseTiledZigZag(KernelPreference.Optimized));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            KernelPolicy.UseSelectionNetwork((KernelPreference)3, 67));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            KernelPolicy.UseTiledZigZag((KernelPreference)3));
    }

    private static byte[] Pattern(int width, int height)
    {
        byte[] pixels = new byte[width * height * 3];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = (byte)((index * 73 + index / 11 * 19) & 255);
        return pixels;
    }

    [Fact]
    public void AutomaticNarrowPolicyKeepsUnsupportedAndSmallInputsOnOriginal()
    {
        Assert.True(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 8, true, false, true));
        Assert.True(KernelPolicy.UseTiledZigZagForConfiguration(65535, 65535, true, 8, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 255, true, 8, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(7, 65535, true, 8, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(65535, 7, true, 8, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, false, 8, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 4, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 16, true, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 8, false, false, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 8, true, true, true));
        Assert.False(KernelPolicy.UseTiledZigZagForConfiguration(128, 256, true, 8, true, false, false));
    }
}
