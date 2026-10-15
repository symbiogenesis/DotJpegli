using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ZigZagOrderingTests
{
    [Fact]
    public void EveryShortBitPatternPreservesTheIndependentNaturalOrderMapping()
    {
        short[] source = new short[64], actual = new short[67];
        for (int first = 0; first <= ushort.MaxValue; first += 64)
        {
            for (int index = 0; index < 64; index++)
                source[index] = unchecked((short)(first + index));
            Array.Fill(actual, (short)-12345);
            ZigZagOrdering.Reorder(source, actual);
            for (int position = 0; position < 64; position++)
                Assert.Equal(source[JpegConstants.ZigZag[position]], actual[position]);
            Assert.All(actual[64..], value => Assert.Equal(-12345, value));
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    public void OverlappingSpansUseTheOriginalBlockAndLeaveOtherStorageUntouched(int sourceOffset, int outputOffset)
    {
        short[] storage = Enumerable.Range(0, 96).Select(index => (short)(index * 197 - 9503)).ToArray();
        short[] original = (short[])storage.Clone();
        ZigZagOrdering.Reorder(storage.AsSpan(sourceOffset, 64), storage.AsSpan(outputOffset, 64));
        for (int position = 0; position < storage.Length; position++)
        {
            short expected = position >= outputOffset && position < outputOffset + 64
                ? original[sourceOffset + JpegConstants.ZigZag[position - outputOffset]] : original[position];
            Assert.Equal(expected, storage[position]);
        }
    }

    [Fact]
    public void ShortBuffersAreRejectedBeforeWritingAnyOutput()
    {
        short[] output = Enumerable.Repeat((short)12345, 64).ToArray();
        Assert.Throws<ArgumentException>(() => ZigZagOrdering.Reorder(new short[63], output));
        Assert.Throws<ArgumentException>(() => ZigZagOrdering.Reorder(new short[64], output.AsSpan(0, 63)));
        Assert.All(output, value => Assert.Equal(12345, value));
    }

    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(0, true, 0)]
    [InlineData(0, false, 37)]
    [InlineData(0, true, 37)]
    [InlineData(1, false, 0)]
    [InlineData(1, true, 0)]
    [InlineData(1, false, 37)]
    [InlineData(1, true, 37)]
    [InlineData(2, false, 0)]
    [InlineData(2, true, 0)]
    [InlineData(2, false, 37)]
    [InlineData(2, true, 37)]
    public void OrderedCacheMatchesEveryNaturalCacheCoefficient(int mode, bool adaptive, int smoothing)
    {
        const int width = 37, height = 35;
        JpegSamplingFactors sampling = mode switch
        {
            0 => new(new(2, 2), new(1, 1), new(1, 1)),
            1 => new(new(3, 2), new(2, 1), new(1, 1)),
            _ => new(new(2, 1), new(1, 2), new(1, 1))
        };
        PixelSource pixels = new PixelSource(TestImages.Gradient(width, height), PixelLayout.Rgb24)
            .WithConvertedPlanes(width, height, width * 3, TestContext.Current.CancellationToken);
        using IDisposable? storage = pixels.ConvertedStorage;
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTableForDistance(1.45f, component)).ToArray();
        float[][]? multipliers = adaptive ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, false) : null;
        using QuantizedImage natural = QuantizedImage.Create(pixels, width, height, width * 3, sampling, tables,
            1.45f, smoothing, adaptive ? new AdaptiveQuantization(width, height, sampling.MaxVertical) : null,
            multipliers, TestContext.Current.CancellationToken)!;
        using QuantizedImage ordered = QuantizedImage.Create(pixels, width, height, width * 3, sampling, tables,
            1.45f, smoothing, adaptive ? new AdaptiveQuantization(width, height, sampling.MaxVertical) : null,
            multipliers, TestContext.Current.CancellationToken, zigZagOrdered: true)!;
        Assert.False(natural.IsZigZagOrdered);
        Assert.True(ordered.IsZigZagOrdered);
        for (int component = 0; component < 3; component++)
        {
            JpegComponentSampling factor = sampling[component];
            int columns = (width * factor.Horizontal + sampling.MaxHorizontal * 8 - 1) / (sampling.MaxHorizontal * 8);
            int rows = (height * factor.Vertical + sampling.MaxVertical * 8 - 1) / (sampling.MaxVertical * 8);
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    ReadOnlySpan<short> expected = natural.GetBlock(component, x, y);
                    ReadOnlySpan<short> actual = ordered.GetBlock(component, x, y);
                    for (int position = 0; position < 64; position++)
                        Assert.Equal(expected[JpegConstants.ZigZag[position]], actual[position]);
                }
        }
    }
}
