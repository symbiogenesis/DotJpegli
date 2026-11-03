namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Internal;
using Xunit;

public sealed class EntropyTokenBudgetTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(128 * 1024)]
    public void EmptyScansNeedNoRentalsEvenWhenTheBudgetCannotSupplyOne(int maximumBytes)
    {
        var pool = new TrackingPool(throwOnRent: 1);
        var scans = new EntropyTokenScans(5, maximumBytes, pool);
        try
        {
            for (int index = 0; index < 5; index++)
            {
                Assert.True(scans[index].IsComplete);
                Assert.Empty(Replay(scans[index]));
            }
        }
        finally
        {
            scans.Dispose();
        }

        scans.Dispose();
        Assert.Empty(pool.Outstanding);
        Assert.Equal(0, pool.SuccessfulRents);
        Assert.Equal(0, pool.Returns);
    }

    [Fact]
    public void UnequalScansShareUnusedCapacityAndReplayTheirCompleteSequences()
    {
        const int maximumBytes = 128 * 1024;
        var pool = new TrackingPool();
        var scans = new EntropyTokenScans(4, maximumBytes, pool);
        try
        {
            HuffmanTable dc = HuffmanTable.CreateDefault(false, false), ac = HuffmanTable.CreateDefault(true, false);
            var coding = new HuffmanCodingPlan.ScanCoding([dc], [ac], [], []);
            scans[0].AddHuffman(0, false, 0, 0, 0);
            using var expectedDc = new MemoryStream();
            var dcWriter = new BitWriter(expectedDc, buffered: true);
            dc.WriteSymbolWithBits(dcWriter, 0, 0, 0);
            dcWriter.Flush();

            using var expectedAc = new MemoryStream();
            var acWriter = new BitWriter(expectedAc, buffered: true);
            // This scan needs more than the former 32KB equal share. The
            // mostly empty DC scan and the unused scans leave capacity for it.
            for (int index = 0; index < 8193; index++)
            {
                scans[1].AddHuffman(0, true, 0, 0, 0);
                ac.WriteSymbolWithBits(acWriter, 0, 0, 0);
            }

            acWriter.Flush();
            Assert.True(scans[0].IsComplete);
            Assert.True(scans[1].IsComplete);
            Assert.Equal(expectedDc.ToArray(), Replay(scans[0], coding));
            Assert.Equal(expectedAc.ToArray(), Replay(scans[1], coding));
            Assert.Equal(maximumBytes, pool.OutstandingBytes);
            Assert.True(pool.MaximumOutstandingBytes <= maximumBytes);
        }
        finally
        {
            scans.Dispose();
        }

        scans.Dispose();
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.SuccessfulRents, pool.Returns);
    }

    [Fact]
    public void AbandoningOneScanRefundsCapacityForLaterScansAndPreservesItsSibling()
    {
        const int maximumBytes = 192;
        var pool = new TrackingPool();
        var scans = new EntropyTokenScans(3, maximumBytes, pool);
        try
        {
            for (int index = 0; index < 32; index++)
            {
                scans[0].AddBits((uint)index, 8);
            }

            scans[1].AddBits(0xff, 8);
            Assert.Equal(maximumBytes, pool.OutstandingBytes);
            scans[0].AddBits(0, 1);
            Assert.False(scans[0].IsComplete);
            Assert.True(scans[1].IsComplete);
            Assert.Equal(64, pool.OutstandingBytes);

            for (int index = 0; index < 32; index++)
            {
                scans[2].AddBits((uint)index, 8);
            }

            Assert.True(scans[2].IsComplete);
            Assert.Equal(maximumBytes, pool.OutstandingBytes);
            Assert.Equal(new byte[] { 0xff, 0 }, Replay(scans[1]));
            Assert.Equal(Enumerable.Range(0, 32).Select(value => (byte)value).ToArray(), Replay(scans[2]));
            scans[0].AddRestart(7);
            using var rejected = new MemoryStream();
            _ = Assert.Throws<InvalidOperationException>(() => scans[0].Replay(new BitWriter(rejected),
                new([], [], [], []), TestContext.Current.CancellationToken));
            Assert.Empty(rejected.ToArray());
            Assert.True(pool.MaximumOutstandingBytes <= maximumBytes);
        }
        finally
        {
            scans.Dispose();
        }

        scans.Dispose();
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.SuccessfulRents, pool.Returns);
    }

    [Fact]
    public void ActualOversizedRentalsConsumeCapacityAndRefundItExactlyOnce()
    {
        const int maximumBytes = 100;
        var pool = new TrackingPool(extraLength: 7);
        var budget = new EntropyTokenBudget(maximumBytes);
        using var first = new EntropyTokens(budget, pool: pool);
        using var sibling = new EntropyTokens(budget, pool: pool);
        first.AddBits(0x42, 8);
        Assert.Equal(8, budget.AvailableBytes);
        Assert.Equal(92, pool.OutstandingBytes);
        sibling.AddBits(0x24, 8);
        Assert.False(sibling.IsComplete);
        Assert.Equal(8, budget.AvailableBytes);
        Assert.Equal("B"u8.ToArray(), Replay(first));

        // All 23 actual array elements belong to the accepted rental, including
        // the seven elements beyond the requested bucket.
        for (int index = 1; index < 23; index++)
        {
            first.AddBits(0, 0);
        }

        Assert.True(first.IsComplete);
        first.AddBits(0, 0);
        Assert.False(first.IsComplete);
        Assert.Equal(maximumBytes, budget.AvailableBytes);
        Assert.Empty(pool.Outstanding);
        first.Dispose();
        sibling.Dispose();
        Assert.Equal(maximumBytes, budget.AvailableBytes);
        using var later = new EntropyTokens(budget, pool: pool);
        later.AddBits(0x24, 8);
        Assert.True(later.IsComplete);
        Assert.Equal("$"u8.ToArray(), Replay(later));
        Assert.True(pool.MaximumOutstandingBytes <= maximumBytes);
        later.Dispose();
        Assert.Equal(maximumBytes, budget.AvailableBytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.SuccessfulRents, pool.Returns);
    }

    [Fact]
    public void RentalLargerThanTheRemainingBudgetIsReturnedWithoutConsumingCapacity()
    {
        var budget = new EntropyTokenBudget(128);
        var pool = new TrackingPool(extraLength: 1);
        using var first = new EntropyTokens(budget, pool: pool);
        first.AddBits(0x42, 8);
        // The requested 32-element bucket would fit, but its 33-element rental
        // does not. No part of the rejected sequence may remain replayable.
        Assert.False(first.IsComplete);
        Assert.Equal(128, budget.AvailableBytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(1, pool.Returns);

        using var sibling = new EntropyTokens(budget);
        sibling.AddBits(0x24, 8);
        Assert.True(sibling.IsComplete);
        Assert.Equal("$"u8.ToArray(), Replay(sibling));
        first.Dispose();
        Assert.Equal(0, budget.AvailableBytes);
    }

    [Fact]
    public void FailedGrowthRentalRetainsThePrefixAndLeavesCapacityForSiblingAndRetry()
    {
        const int maximumBytes = 96 * 1024;
        var budget = new EntropyTokenBudget(maximumBytes);
        var pool = new TrackingPool(throwOnRent: 2);
        var first = new EntropyTokens(budget, pool: pool);
        var sibling = new EntropyTokens(budget, pool: pool);
        try
        {
            for (int index = 0; index < 8192; index++)
            {
                first.AddBits((uint)(index & 255), 8);
            }

            _ = Assert.Throws<InvalidOperationException>(() => first.AddBits(0xff, 8));
            Assert.True(first.IsComplete);
            Assert.Equal(64 * 1024, budget.AvailableBytes);
            byte[] prefix = Enumerable.Range(0, 8192)
                .SelectMany(index => (index & 255) == 255 ? new byte[] { 0xff, 0 } : [(byte)index]).ToArray();
            Assert.Equal(prefix, Replay(first));

            sibling.AddBits(0x24, 8);
            Assert.Equal(32 * 1024, budget.AvailableBytes);
            first.AddBits(0xff, 8);
            Assert.True(first.IsComplete);
            Assert.Equal(prefix.Concat(new byte[] { 0xff, 0 }).ToArray(), Replay(first));
            Assert.Equal("$"u8.ToArray(), Replay(sibling));
            Assert.Equal(0, budget.AvailableBytes);
            Assert.True(pool.MaximumOutstandingBytes <= maximumBytes);
        }
        finally
        {
            first.Dispose();
            sibling.Dispose();
        }

        first.Dispose();
        sibling.Dispose();
        Assert.Equal(maximumBytes, budget.AvailableBytes);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.SuccessfulRents, pool.Returns);
    }

    [Fact]
    public void IndependentOwnersDoNotConsumeOrRefundEachOthersBudget()
    {
        var pool = new TrackingPool();
        var first = new EntropyTokenScans(1, 64, pool);
        var second = new EntropyTokenScans(1, 64, pool);
        try
        {
            first[0].AddBits(0x42, 8);
            second[0].AddBits(0x24, 8);
            Assert.True(first[0].IsComplete);
            Assert.True(second[0].IsComplete);
            Assert.Equal(128, pool.OutstandingBytes);
            first.Dispose();
            Assert.Equal(64, pool.OutstandingBytes);
            Assert.Equal("$"u8.ToArray(), Replay(second[0]));
            first[0].AddBits(0xff, 8);
            Assert.False(first[0].IsComplete);
            Assert.Equal(64, pool.OutstandingBytes);
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }

        Assert.Empty(pool.Outstanding);
        Assert.Equal(pool.SuccessfulRents, pool.Returns);
    }

    [Fact]
    public void ScriptScansShareCapacityAndPrepareOnlyTheirSelectedContexts()
    {
        const int maximumBytes = 128 * 1024;
        var pool = new TrackingPool();
        JpegScan[] script = [new([1], 0, 0, 0, 2), new([2], 3, 63, 2, 1), new([0, 1, 2], 0, 0, 1, 0)];
        using var scans = new EntropyTokenScans(script, progressive: true, maximumBytes, pool);
        var table = HuffmanTable.CreateOptimized(Enumerable.Repeat(1L, 256).ToArray());
        HuffmanCodingPlan.ScanCoding[] coding =
        [
            new([null!, table, null!], [null!, null!, null!], [], []),
            new([null!, null!, null!], [null!, null!, table], [], []),
            new([null!, null!, null!], [null!, null!, null!], [], [])
        ];

        using var expectedDc = new MemoryStream();
        using var expectedAc = new MemoryStream();
        using var expectedRaw = new MemoryStream();
        BitWriter[] writers = [new(expectedDc, buffered: true), new(expectedAc, buffered: true),
            new(expectedRaw, buffered: true)];
        for (int index = 0; index < 40; index++)
        {
            scans[0].AddHuffman(1, false, 3, 5, 3);
            table.WriteSymbolWithBits(writers[0], 3, 5, 3);
            scans[2].AddBits(0xabcdef, 24);
            writers[2].WriteBits(0xabcdef, 24);
        }

        for (int index = 0; index < 8193; index++)
        {
            scans[1].AddHuffman(2, true, 3, 5, 3);
            table.WriteSymbolWithBits(writers[1], 3, 5, 3);
        }

        foreach (BitWriter writer in writers)
        {
            writer.Flush();
        }

        Assert.True(scans[0].IsComplete);
        Assert.True(scans[1].IsComplete);
        Assert.True(scans[2].IsComplete);
        Assert.Equal(expectedDc.ToArray(), Replay(scans[0], coding[0]));
        Assert.Equal(expectedAc.ToArray(), Replay(scans[1], coding[1]));
        Assert.Equal(expectedRaw.ToArray(), Replay(scans[2], coding[2]));
        Assert.Equal(maximumBytes, pool.OutstandingBytes);
        Assert.True(pool.MaximumOutstandingBytes <= maximumBytes);
    }

    private static byte[] Replay(EntropyTokens tokens, HuffmanCodingPlan.ScanCoding? coding = null)
    {
        using var output = new MemoryStream();
        var writer = new BitWriter(output, buffered: true);
        tokens.Replay(writer, coding ?? new([], [], [], []), TestContext.Current.CancellationToken);
        writer.Flush();
        return output.ToArray();
    }

    private sealed class TrackingPool(int extraLength = 0, int throwOnRent = 0) : ArrayPool<uint>
    {
        private int rentAttempts;

        internal int SuccessfulRents { get; private set; }
        internal int Returns { get; private set; }
        internal long OutstandingBytes { get; private set; }
        internal long MaximumOutstandingBytes { get; private set; }
        internal List<uint[]> Outstanding { get; } = [];

        public override uint[] Rent(int minimumLength)
        {
            this.rentAttempts++;
            if (this.rentAttempts == throwOnRent)
            {
                throw new InvalidOperationException("Injected rental failure.");
            }

            uint[] buffer = new uint[minimumLength + extraLength];
            Array.Fill(buffer, uint.MaxValue);
            this.SuccessfulRents++;
            this.Outstanding.Add(buffer);
            this.OutstandingBytes += (long)buffer.Length * sizeof(uint);
            this.MaximumOutstandingBytes = Math.Max(this.MaximumOutstandingBytes, this.OutstandingBytes);
            return buffer;
        }

        public override void Return(uint[] array, bool clearArray = false)
        {
            Assert.True(this.Outstanding.Remove(array), "A rental was returned more than once.");
            this.Returns++;
            this.OutstandingBytes -= (long)array.Length * sizeof(uint);
            Array.Fill(array, uint.MaxValue);
        }
    }
}
