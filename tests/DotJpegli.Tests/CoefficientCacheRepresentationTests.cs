namespace DotJpegli.Tests;

using System.Buffers;
using DotJpegli.Performance;
using Xunit;

public sealed class CoefficientCacheRepresentationTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void FullBlocksDcAndProgressiveQueriesPreserveEverySignedCoefficient(
        int mode, bool denseFallback)
    {
        var layout = (CoefficientCacheLayout)mode;
        short[] source = MixedBlocks((CoefficientCacheRepresentation.BlocksPerTile * 2) + 3);
        short[] expected = (short[])source.Clone();
        var bytes = new DirtyPool<byte>(0xA5, extra: 29);
        var indices = new DirtyPool<int>(-741, extra: 7);
        using (CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create(source, layout,
            denseFallback: denseFallback, pool: bytes, indexPool: indices,
            cancellationToken: TestContext.Current.CancellationToken)!)
        {
            Assert.NotNull(cache);
            Assert.Equal(source.Length / 64, cache.BlockCount);
            Assert.Equal(bytes.Outstanding.Sum(buffer => (long)buffer.Length)
                + indices.Outstanding.Sum(buffer => (long)buffer.Length * sizeof(int)), cache.RentedBytes);
            Assert.InRange(cache.UsefulBytes, 1, cache.RentedBytes);

            // Mutating input after preparation must not change immutable cache data.
            Array.Fill(source, (short)73);
            short[] block = new short[71];
            for (int index = cache.BlockCount - 1; index >= 0; index--)
            {
                Array.Fill(block, short.MinValue);
                cache.CopyBlock(index, block);
                Assert.Equal(expected.AsSpan(index * 64, 64).ToArray(), block.AsSpan(0, 64).ToArray());
                Assert.All(block[64..], value => Assert.Equal(short.MinValue, value));
                Assert.Equal(expected[index * 64], cache.GetDc(index));

                // A non-raster order exposes dependencies on an earlier decode.
                for (int query = 0; query < 64; query++)
                {
                    int coefficient = (query * 37) & 63;
                    Assert.Equal(expected[(index * 64) + coefficient],
                        cache.GetCoefficient(index, coefficient));
                }
            }
            bytes.AssertTailsUntouched();
            indices.AssertTailsUntouched();
        }

        Assert.Empty(bytes.Outstanding);
        Assert.Empty(indices.Outstanding);
        Assert.Equal(bytes.Rents, bytes.Returns);
        Assert.Equal(indices.Rents, indices.Returns);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void CompactLayoutsRetainSparseTilesAndFallBackForDenseNoise(int mode)
    {
        var layout = (CoefficientCacheLayout)mode;
        int count = CoefficientCacheRepresentation.BlocksPerTile;
        short[] sparse = new short[count * 64];
        for (int block = 0; block < count; block++)
        {
            sparse[block * 64] = (short)(block - 17);
            sparse[(block * 64) + 63] = (short)((block & 1) == 0 ? -1 : 1);
        }

        using CoefficientCacheRepresentation compact = CoefficientCacheRepresentation.Create(sparse, layout,
            cancellationToken: TestContext.Current.CancellationToken)!;
        using CoefficientCacheRepresentation dense = CoefficientCacheRepresentation.Create(sparse,
            CoefficientCacheLayout.Dense, cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.True(compact.CompactTiles > 0);
        Assert.True(compact.RentedBytes < dense.RentedBytes);

        short[] noise = new short[count * 64];
        for (int index = 0; index < noise.Length; index++)
        {
            noise[index] = (index & 1) == 0 ? short.MinValue : short.MaxValue;
        }
        using CoefficientCacheRepresentation fallback = CoefficientCacheRepresentation.Create(noise, layout,
            cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.Equal(0, fallback.CompactTiles);
        Assert.Equal(1, fallback.DenseTiles);
        short[] actual = new short[64];
        fallback.CopyBlock(count - 1, actual);
        Assert.Equal(noise[^64..], actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AdmissionChargesActualOversizedRentalsAndReturnsRejectedStorage(int mode)
    {
        var layout = (CoefficientCacheLayout)mode;
        short[] source = MixedBlocks(35);
        var observedBytes = new DirtyPool<byte>(0xAC, extra: 313);
        var observedIndices = new DirtyPool<int>(-17, extra: 13);
        long needed;
        using (CoefficientCacheRepresentation observed = CoefficientCacheRepresentation.Create(source, layout,
            pool: observedBytes, indexPool: observedIndices,
            cancellationToken: TestContext.Current.CancellationToken)!)
        {
            needed = observed.RentedBytes;
        }

        var rejectedBytes = new DirtyPool<byte>(0xBC, extra: 313);
        var rejectedIndices = new DirtyPool<int>(-19, extra: 13);
        Assert.Null(CoefficientCacheRepresentation.Create(source, layout, maximumBytes: needed - 1,
            pool: rejectedBytes, indexPool: rejectedIndices,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(rejectedBytes.Outstanding);
        Assert.Empty(rejectedIndices.Outstanding);
        Assert.Equal(rejectedBytes.Rents, rejectedBytes.Returns);
        Assert.Equal(rejectedIndices.Rents, rejectedIndices.Returns);

        using CoefficientCacheRepresentation accepted = CoefficientCacheRepresentation.Create(source, layout,
            maximumBytes: needed, pool: rejectedBytes, indexPool: rejectedIndices,
            cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.NotNull(accepted);
        Assert.Equal(needed, accepted.RentedBytes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void RentalFailureAndCancellationReturnEveryAcceptedArray(int mode)
    {
        var layout = (CoefficientCacheLayout)mode;
        short[] source = MixedBlocks(35);
        int rents = 0;
        var injected = new InvalidOperationException("Injected compact-cache rental failure.");
        void FailSecond()
        {
            if (++rents == 2)
            {
                throw injected;
            }
        }
        var bytes = new DirtyPool<byte>(0xAD, onRent: FailSecond);
        var indices = new DirtyPool<int>(-23, onRent: FailSecond);
        Assert.Same(injected, Assert.Throws<InvalidOperationException>(() =>
            CoefficientCacheRepresentation.Create(source, layout, pool: bytes, indexPool: indices,
                cancellationToken: TestContext.Current.CancellationToken)));
        Assert.Empty(bytes.Outstanding);
        Assert.Empty(indices.Outstanding);
        Assert.Equal(1, bytes.Returns + indices.Returns);

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        int cancellationRents = 0;
        void CancelSecond()
        {
            if (++cancellationRents == 2)
            {
                cancellation.Cancel();
            }
        }
        var canceledBytes = new DirtyPool<byte>(0xAB, onRent: CancelSecond);
        var canceledIndices = new DirtyPool<int>(-29, onRent: CancelSecond);
        OperationCanceledException error = Assert.ThrowsAny<OperationCanceledException>(() =>
            CoefficientCacheRepresentation.Create(source, layout, pool: canceledBytes, indexPool: canceledIndices,
                cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Empty(canceledBytes.Outstanding);
        Assert.Empty(canceledIndices.Outstanding);
        Assert.Equal(canceledBytes.Rents, canceledBytes.Returns);
        Assert.Equal(canceledIndices.Rents, canceledIndices.Returns);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DisposalIsIdempotentAndInvalidQueriesAreRejected(int mode)
    {
        var layout = (CoefficientCacheLayout)mode;
        var bytes = new DirtyPool<byte>(0xBD);
        var indices = new DirtyPool<int>(-31);
        CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create(MixedBlocks(2), layout,
            pool: bytes, indexPool: indices, cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetDc(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetDc(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetCoefficient(0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetCoefficient(0, 64));
        Assert.Throws<ArgumentException>(() => cache.CopyBlock(0, new short[63]));
        cache.Dispose();
        cache.Dispose();
        Assert.Empty(bytes.Outstanding);
        Assert.Empty(indices.Outstanding);
        Assert.Equal(bytes.Rents, bytes.Returns);
        Assert.Equal(indices.Rents, indices.Returns);
        Assert.Throws<ObjectDisposedException>(() => cache.GetDc(0));
        Assert.Throws<ObjectDisposedException>(() => cache.GetCoefficient(0, 1));
        Assert.Throws<ObjectDisposedException>(() => cache.CopyBlock(0, new short[64]));
    }

    [Fact]
    public void InvalidStorageRequestsFailBeforeRenting()
    {
        var bytes = new DirtyPool<byte>(0xBE);
        var indices = new DirtyPool<int>(-37);
        Assert.Throws<ArgumentException>(() => CoefficientCacheRepresentation.Create(new short[65],
            CoefficientCacheLayout.Bitmap, pool: bytes, indexPool: indices,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoefficientCacheRepresentation.Create(new short[64],
            (CoefficientCacheLayout)17, pool: bytes, indexPool: indices,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoefficientCacheRepresentation.Create(new short[64],
            CoefficientCacheLayout.Bitmap, maximumBytes: -1, pool: bytes, indexPool: indices,
            cancellationToken: TestContext.Current.CancellationToken));
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => CoefficientCacheRepresentation.Create(new short[64],
            CoefficientCacheLayout.Bitmap, pool: bytes, indexPool: indices, cancellationToken: canceled.Token));
        Assert.Equal(0, bytes.Rents + indices.Rents);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EmptyCacheUsesNoRentalAndHasNoValidBlock(int mode)
    {
        var bytes = new DirtyPool<byte>(0xBF);
        var indices = new DirtyPool<int>(-41);
        using CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create([],
            (CoefficientCacheLayout)mode, maximumBytes: 0, pool: bytes, indexPool: indices,
            cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.NotNull(cache);
        Assert.Equal(0, cache.BlockCount);
        Assert.Equal(0, cache.UsefulBytes);
        Assert.Equal(0, cache.RentedBytes);
        Assert.Equal(0, cache.CompactTiles + cache.DenseTiles);
        Assert.Equal(0, bytes.Rents + indices.Rents);
        Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetDc(0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ForcedCompactTilesRoundTripSignedAcExtremes(int mode)
    {
        short[] source = new short[64];
        source[0] = short.MaxValue;
        source[1] = short.MinValue;
        source[2] = short.MaxValue;
        source[62] = 1;
        source[63] = short.MinValue;
        using CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create(source,
            (CoefficientCacheLayout)mode, denseFallback: false,
            cancellationToken: TestContext.Current.CancellationToken)!;
        Assert.Equal(1, cache.CompactTiles);
        short[] actual = new short[64];
        cache.CopyBlock(0, actual);
        Assert.Equal(source, actual);
        for (int coefficient = 0; coefficient < 64; coefficient++)
        {
            Assert.Equal(source[coefficient], cache.GetCoefficient(0, coefficient));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void KnownOneNonzeroBlockHasIndependentlyCountedStorageWork(int mode)
    {
        short[] source = new short[64];
        source[0] = 7;
        source[63] = -1;
        using CoefficientCacheRepresentation cache = CoefficientCacheRepresentation.Create(source,
            (CoefficientCacheLayout)mode, denseFallback: false,
            cancellationToken: TestContext.Current.CancellationToken)!;

        // Compact planning reads all 63 ACs twice. Bitmap then reads each input
        // once; bit packing additionally reads/writes the one payload byte.
        // Counts include the compact tile-index write and the stored metadata.
        (long, long, long, long, long) expectedPack = mode switch
        {
            0 => (64L, 128L, 64L, 0L, 0L),
            1 => (190L, 15L, 192L, 0L, 2L),
            _ => (191L, 194L, 254L, 0L, 574L)
        };
        Assert.Equal(expectedPack, Work(cache.PackWork));

        var dc = new CoefficientCacheWork();
        Assert.Equal(7, cache.GetDc(0, dc));
        Assert.Equal((mode == 0 ? 2L : 3L, 0L, 1L, 0L, 0L), Work(dc));

        var absent = new CoefficientCacheWork();
        Assert.Equal(0, cache.GetCoefficient(0, 1, absent));
        (long, long, long, long, long) expectedAbsent = mode switch
        {
            0 => (2L, 0L, 1L, 0L, 0L),
            1 => (9L, 0L, 2L, 0L, 2L),
            _ => (4L, 0L, 2L, 0L, 3L)
        };
        Assert.Equal(expectedAbsent, Work(absent));

        var present = new CoefficientCacheWork();
        Assert.Equal(-1, cache.GetCoefficient(0, 63, present));
        (long, long, long, long, long) expectedPresent = mode switch
        {
            0 => (2L, 0L, 1L, 0L, 0L),
            1 => (13L, 0L, 2L, 1L, 3L),
            _ => (5L, 0L, 2L, 0L, 8L)
        };
        Assert.Equal(expectedPresent, Work(present));

        var expansion = new CoefficientCacheWork();
        short[] actual = new short[64];
        cache.CopyBlock(0, actual, expansion);
        Assert.Equal(source, actual);
        (long, long, long, long, long) expectedExpansion = mode switch
        {
            0 => (128L, 64L, 1L, 0L, 0L),
            1 => (15L, 64L, 3L, 0L, 126L),
            _ => (193L, 64L, 64L, 0L, 194L)
        };
        Assert.Equal(expectedExpansion, Work(expansion));
    }

    private static (long Reads, long Writes, long Index, long Rank, long Bits) Work(CoefficientCacheWork work)
        => (work.Reads, work.Writes, work.IndexOperations, work.RankOperations, work.BitOperations);

    private static short[] MixedBlocks(int count)
    {
        short[] result = new short[count * 64];
        var random = new Random(91371);
        for (int block = 0; block < count; block++)
        {
            result[block * 64] = (short)(block % 3 == 0 ? short.MinValue : block - 37);
            for (int coefficient = 1; coefficient < 64; coefficient++)
            {
                result[(block * 64) + coefficient] = (block % 4) switch
                {
                    0 => 0,
                    1 => coefficient == 63 ? short.MaxValue : (short)0,
                    2 => (short)((coefficient & 1) == 0 ? 1 << (coefficient % 15) : -(1 << (coefficient % 15))),
                    _ => (short)random.Next(short.MinValue, short.MaxValue + 1)
                };
            }
        }
        return result;
    }

    private sealed class DirtyPool<T>(T poison, int extra = 0, Action? onRent = null) : ArrayPool<T>
    {
        private readonly Dictionary<T[], int> minimumLengths = [];
        internal int Rents { get; private set; }
        internal int Returns { get; private set; }
        internal List<T[]> Outstanding { get; } = [];

        public override T[] Rent(int minimumLength)
        {
            onRent?.Invoke();
            this.Rents++;
            var buffer = new T[minimumLength + extra];
            Array.Fill(buffer, poison);
            this.Outstanding.Add(buffer);
            this.minimumLengths.Add(buffer, minimumLength);
            return buffer;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            Assert.True(this.Outstanding.Remove(array), "A compact-cache rental was returned more than once.");
            Assert.True(this.minimumLengths.Remove(array));
            this.Returns++;
            Array.Fill(array, poison);
        }

        internal void AssertTailsUntouched()
        {
            foreach ((T[] buffer, int minimumLength) in this.minimumLengths)
            {
                Assert.All(buffer[minimumLength..], value => Assert.Equal(poison, value));
            }
        }
    }
}
