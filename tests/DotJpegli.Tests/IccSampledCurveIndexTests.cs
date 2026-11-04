namespace DotJpegli.Tests;

using System.Buffers.Binary;
using DotJpegli.Internal;
using Xunit;

public sealed class IccSampledCurveIndexTests
{
    [Theory]
    [InlineData(512, false)]
    [InlineData(1025, true)]
    [InlineData(4097, false)]
    public void GridBracketsExactLowerAndUpperBoundsAcrossAllStoredPrecisionValues(int length, bool plateaus)
    {
        double[] values = Samples(length, plateaus).Select(value => value / 65535.0).ToArray();
        IccColorTransform.SampledCurveIndex index = Assert.IsType<IccColorTransform.SampledCurveIndex>(
            IccColorTransform.SampledCurveIndex.TryCreate(values, TestContext.Current.CancellationToken));
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            AssertBracket(index, values, value / 65535.0);
        }
        foreach (double value in values)
        {
            AssertBracket(index, values, value);
            AssertBracket(index, values, Math.BitDecrement(value));
            AssertBracket(index, values, Math.BitIncrement(value));
        }
        index.GetRange(values, double.NaN, out int low, out int high);
        Assert.Equal(0, low);
        Assert.Equal(length, high);
    }

    [Fact]
    public void SmallOversizedAndClusteredCurvesKeepTheOriginalSearch()
    {
        foreach (int length in new[] { 511, 8193 })
        {
            double[] values = Enumerable.Range(0, length).Select(value => (double)value / length).ToArray();
            Assert.Null(IccColorTransform.SampledCurveIndex.TryCreate(values, TestContext.Current.CancellationToken));
        }
        Assert.Null(IccColorTransform.SampledCurveIndex.TryCreate(new double[512], TestContext.Current.CancellationToken));
        double[] clustered = new double[512];
        clustered[^1] = 1;
        Assert.Null(IccColorTransform.SampledCurveIndex.TryCreate(clustered, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void IndexPreparationObservesCancellation()
    {
        double[] values = Samples(1025, false).Select(value => value / 65535.0).ToArray();
        _ = Assert.Throws<OperationCanceledException>(() =>
            IccColorTransform.SampledCurveIndex.TryCreate(values, new CancellationToken(true)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestinationProfilePreservesOriginalInverseInterpolationBits(bool plateaus)
    {
        ushort[] samples = Samples(1025, plateaus);
        double[] values = samples.Select(value => value / 65535.0).ToArray();
        var transform = IccColorTransform.Create(JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile(),
            Profile(samples), true, true, IccRenderingIntent.RelativeColorimetric, TestContext.Current.CancellationToken);
        for (int sample = 0; sample <= ushort.MaxValue; sample += 17)
        {
            AssertConversion(transform, values, (float)(sample / 65535.0));
        }
        foreach (double value in values)
        {
            float input = (float)value;
            AssertConversion(transform, values, input);
            AssertConversion(transform, values, MathF.BitDecrement(input));
            AssertConversion(transform, values, MathF.BitIncrement(input));
        }
    }

    private static void AssertBracket(IccColorTransform.SampledCurveIndex index, double[] values, double value)
    {
        index.GetRange(values, value, out int low, out int high);
        int lower = LowerBound(values, value, false), upper = LowerBound(values, value, true);
        Assert.InRange(lower, low, high);
        Assert.InRange(upper, low, high);
        Assert.InRange(high - low, 0, 33);
    }

    private static int LowerBound(double[] values, double value, bool strict)
    {
        int low = 0, high = values.Length;
        while (low < high)
        {
            int middle = (low + high) / 2;
            if (values[middle] < value || (strict && values[middle] == value))
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }
        return low;
    }

    private static void AssertConversion(IccColorTransform transform, double[] values, float input)
    {
        double value = Math.Clamp(input, 0, 1);
        // An independent linear scan supplies the interval and complete plateau.
        int first = Array.FindIndex(values, knot => knot >= value);
        double expected;
        if (values[first] == value)
        {
            int last = first;
            while (last + 1 < values.Length && values[last + 1] == value)
            {
                last++;
            }
            expected = (double)(value == values[^1] ? first : last) / (values.Length - 1);
        }
        else
        {
            int lower = first - 1;
            expected = (lower + ((value - values[lower]) / (values[first] - values[lower]))) / (values.Length - 1);
        }
        transform.Convert(input, input, input, out float red, out float green, out float blue);
        int expectedBits = BitConverter.SingleToInt32Bits((float)expected);
        Assert.Equal(expectedBits, BitConverter.SingleToInt32Bits(red));
        Assert.Equal(expectedBits, BitConverter.SingleToInt32Bits(green));
        Assert.Equal(expectedBits, BitConverter.SingleToInt32Bits(blue));
    }

    private static ushort[] Samples(int length, bool plateaus)
    {
        ushort[] values = Enumerable.Range(0, length).Select(index => (ushort)((long)index * ushort.MaxValue / (length - 1))).ToArray();
        if (plateaus)
        {
            Array.Fill(values, (ushort)0, 0, 5);
            Array.Fill(values, values[length / 2], length / 2, 8);
            Array.Fill(values, ushort.MaxValue, length - 7, 7);
        }
        return values;
    }

    private static byte[] Profile(ushort[] samples)
    {
        byte[] original = JpegliIccProfiles.CreateLinearSrgbGrayscaleProfile();
        int offset = (original.Length + 3) & ~3;
        int length = 12 + (samples.Length * 2);
        byte[] profile = new byte[offset + length];
        original.CopyTo(profile, 0);
        BinaryPrimitives.WriteUInt32BigEndian(profile, (uint)profile.Length);
        "curv"u8.CopyTo(profile.AsSpan(offset));
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(offset + 8), (uint)samples.Length);
        for (int index = 0; index < samples.Length; index++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(profile.AsSpan(offset + 12 + (index * 2)), samples[index]);
        }
        int tags = (int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(128));
        for (int tag = 0; tag < tags; tag++)
        {
            int entry = 132 + (tag * 12);
            if (profile.AsSpan(entry, 4).SequenceEqual("kTRC"u8))
            {
                BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(entry + 4), (uint)offset);
                BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(entry + 8), (uint)length);
                return profile;
            }
        }
        throw new InvalidOperationException("Missing grayscale curve.");
    }
}
