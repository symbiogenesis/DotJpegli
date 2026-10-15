using System.Runtime.Intrinsics;
using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class Net11DownsamplingTests
{
    [Fact]
    public void PortableCompositionPreservesAdjacentPairBitsAtBothWidths()
    {
        foreach (float[] samples in Samples())
        {
            Vector256<float> wide = Downsampling.PairwiseSumsPortable(Vector256.Create(samples.AsSpan(0, 8)),
                Vector256.Create(samples.AsSpan(8, 8)));
            Vector128<float> narrow = Downsampling.PairwiseSumsPortable(Vector128.Create(samples.AsSpan(0, 4)),
                Vector128.Create(samples.AsSpan(4, 4)));
            for (int pair = 0; pair < 8; pair++)
                Assert.Equal(BitConverter.SingleToInt32Bits(samples[pair * 2] + samples[pair * 2 + 1]),
                    BitConverter.SingleToInt32Bits(wide[pair]));
            for (int pair = 0; pair < 4; pair++)
                Assert.Equal(BitConverter.SingleToInt32Bits(samples[pair * 2] + samples[pair * 2 + 1]),
                    BitConverter.SingleToInt32Bits(narrow[pair]));
        }
    }

#if NET11_0_OR_GREATER
    [Fact]
    public void UnzipCompositionPreservesPortablePairwiseBitsAtBothWidths()
    {
        foreach (float[] samples in Samples())
        {
            Vector256<float> firstWide = Vector256.Create(samples.AsSpan(0, 8));
            Vector256<float> secondWide = Vector256.Create(samples.AsSpan(8, 8));
            Vector256<float> expectedWide = Downsampling.PairwiseSumsPortable(firstWide, secondWide);
            Vector256<float> actualWide = Downsampling.PairwiseSumsNet11(firstWide, secondWide);
            Vector128<float> firstNarrow = Vector128.Create(samples.AsSpan(0, 4));
            Vector128<float> secondNarrow = Vector128.Create(samples.AsSpan(4, 4));
            Vector128<float> expectedNarrow = Downsampling.PairwiseSumsPortable(firstNarrow, secondNarrow);
            Vector128<float> actualNarrow = Downsampling.PairwiseSumsNet11(firstNarrow, secondNarrow);
            for (int lane = 0; lane < 8; lane++)
                Assert.Equal(BitConverter.SingleToInt32Bits(expectedWide[lane]), BitConverter.SingleToInt32Bits(actualWide[lane]));
            for (int lane = 0; lane < 4; lane++)
                Assert.Equal(BitConverter.SingleToInt32Bits(expectedNarrow[lane]), BitConverter.SingleToInt32Bits(actualNarrow[lane]));
        }
    }
#endif

    private static IEnumerable<float[]> Samples()
    {
        yield return [float.MaxValue, -float.MaxValue, -0f, -0f, 1f, MathF.BitIncrement(1f), float.Epsilon, -float.Epsilon,
            0f, -0f, -float.MaxValue, float.MaxValue, 4096.0005f, -4096f, -1f, 1f];
        var random = new Random(491713);
        for (int row = 0; row < 128; row++)
        {
            float[] samples = new float[16];
            for (int index = 0; index < samples.Length; index++)
            {
                int bits;
                do { bits = (int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1); }
                while ((bits & 0x7f800000) == 0x7f800000);
                samples[index] = BitConverter.Int32BitsToSingle(bits);
            }
            yield return samples;
        }
    }
}
