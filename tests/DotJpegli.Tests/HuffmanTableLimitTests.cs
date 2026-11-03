namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class HuffmanTableLimitTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ExplicitLimitsMergeHistogramsWithoutRedefiningSlots(int limit)
    {
        JpegScan[] scans =
        [
            new([0, 1, 2], 0, 0), new([0], 1, 1), new([0], 2, 2),
            new([0], 3, 3), new([0], 4, 4), new([0], 5, 5),
            new([0], 6, 63), new([1], 1, 63), new([2], 1, 63)
        ];
        long[][] dc = [Histogram(0), Histogram(1), Histogram(2)];
        long[][][] ac = scans.Select(_ => new[] { new long[256], new long[256], new long[256] }).ToArray();
        for (int index = 1; index < scans.Length; index++)
        {
            ac[index][scans[index].Components[0]] = Histogram(index - 1);
        }

        var plan = HuffmanCodingPlan.Create(scans, dc, ac, true, true, limit);
        Assert.Equal(Math.Min(3, limit), plan.Scans[0].Definitions.Count(definition => definition.Selector < 16));
        Assert.Equal(limit, plan.Scans[0].Definitions.Count(definition => definition.Selector >= 16));
        Assert.All(plan.Scans.Skip(1), scan => Assert.Empty(scan.Definitions));
        Assert.All(plan.Scans[0].Selectors, selector => Assert.InRange(selector >> 4, 0, limit - 1));
        var slots = new Dictionary<int, HuffmanTable>();
        for (int index = 1; index < scans.Length; index++)
        {
            int component = scans[index].Components[0];
            HuffmanCodingPlan.ScanCoding coding = plan.Scans[index];
            int slot = coding.Selectors[component] & 15;
            Assert.InRange(slot, 0, limit - 1);
            Assert.Contains((byte)(index - 1), coding.AcTables[component].Symbols);
            if (slots.TryGetValue(slot, out HuffmanTable? table))
            {
                Assert.Same(table, coding.AcTables[component]);
            }
            slots[slot] = coding.AcTables[component];
        }
        Assert.Equal(limit, slots.Count);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(4, 4)]
    public void SequentialBaselineStillUsesOnlyTwoSlots(int limit, int expectedDefinitions)
    {
        long[][] dc = [Histogram(0), Histogram(1), Histogram(2)];
        long[][][] ac = [[Histogram(0), Histogram(1), Histogram(2)]];
        var plan = HuffmanCodingPlan.Create([new([0, 1, 2])], dc, ac, false, true, limit);
        Assert.False(plan.RequiresExtendedSequential);
        Assert.Equal(expectedDefinitions, plan.Scans[0].Definitions.Length);
        Assert.All(plan.Scans[0].Selectors, selector =>
        {
            Assert.InRange(selector >> 4, 0, Math.Min(limit, 2) - 1);
            Assert.InRange(selector & 15, 0, Math.Min(limit, 2) - 1);
        });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(5)]
    public void InvalidLimitsAreRejectedBeforeDestinationOutput(int limit)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => HuffmanCodingPlan.Create([new([0])],
            [Histogram(0)], [[Histogram(0)]], true, true, limit));
        using var output = new MemoryStream();
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(output, new byte[3], 1, 1,
                options: new() { HuffmanTableLimit = limit }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.HuffmanTableLimit), error.ParamName);
        Assert.Equal(0, output.Length);
    }

    private static long[] Histogram(int symbol)
    {
        long[] histogram = new long[256];
        histogram[symbol] = 10000;
        return histogram;
    }
}
