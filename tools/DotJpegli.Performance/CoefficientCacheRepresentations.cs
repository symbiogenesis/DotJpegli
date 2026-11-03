namespace DotJpegli.Performance;

using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;

internal enum CoefficientCacheLayout
{
    Dense,
    Bitmap,
    BitPacked
}

/// <summary>
/// Logical storage work, not machine instructions or elapsed-time predictions.
/// Reads/writes count source and caller-destination short elements, payload bytes
/// and index int elements; temporary planning spans, counters, locals and loop
/// control are excluded. Index operations count source coefficient locations,
/// block/tile resolution, tile-index writes and compact AC location resolution;
/// byte addressing inside binary/bit helpers is excluded.
/// Rank operations count PopCount calls. Bit operations count the
/// explicitly charged shifts, masks, boolean operations and bit-width queries.
/// </summary>
internal sealed class CoefficientCacheWork
{
    public long Reads { get; internal set; }
    public long Writes { get; internal set; }
    public long IndexOperations { get; internal set; }
    public long RankOperations { get; internal set; }
    public long BitOperations { get; internal set; }
}

/// <summary>
/// Offline immutable coefficient-layout study. All layouts retain direct DC;
/// compact tiles additionally retain exact random AC access and block expansion.
/// The source is neither retained nor changed. The encoder does not use this type.
/// </summary>
internal sealed class CoefficientCacheRepresentation : IDisposable
{
    internal const int BlocksPerTile = 32;
    private const int CoefficientsPerBlock = 64;
    private const int DenseBlockBytes = CoefficientsPerBlock * sizeof(short);
    private const long DefaultMaximumBytes = 64L * 1024 * 1024;
    private readonly CoefficientCacheLayout layout;
    private readonly ArrayPool<byte> pool;
    private readonly ArrayPool<int> indexPool;
    private byte[]? payload;
    private int[]? tileOffsets;
    private bool disposed;

    private CoefficientCacheRepresentation(int blockCount, CoefficientCacheLayout layout,
        long payloadBytes, ArrayPool<byte> pool, ArrayPool<int> indexPool,
        CoefficientCacheWork packWork)
    {
        this.BlockCount = blockCount;
        this.layout = layout;
        this.PayloadBytes = payloadBytes;
        this.IndexBytes = layout == CoefficientCacheLayout.Dense ? 0
            : (long)((blockCount + BlocksPerTile - 1) / BlocksPerTile) * sizeof(int);
        this.pool = pool;
        this.indexPool = indexPool;
        this.PackWork = packWork;
    }

    internal int BlockCount { get; }
    internal long PayloadBytes { get; }
    internal long RentedPayloadBytes => this.payload?.LongLength ?? 0;
    internal long IndexBytes { get; }
    internal long RentedIndexBytes => (this.tileOffsets?.LongLength ?? 0) * sizeof(int);
    internal long UsefulBytes => this.PayloadBytes + this.IndexBytes;
    internal long RentedBytes => this.RentedPayloadBytes + this.RentedIndexBytes;
    internal int CompactTiles { get; private set; }
    internal int DenseTiles { get; private set; }
    internal CoefficientCacheWork PackWork { get; }

    internal static CoefficientCacheRepresentation? Create(ReadOnlySpan<short> coefficients,
        CoefficientCacheLayout layout, bool denseFallback = true,
        long maximumBytes = DefaultMaximumBytes, ArrayPool<byte>? pool = null,
        ArrayPool<int>? indexPool = null, CancellationToken cancellationToken = default)
    {
        if (coefficients.Length % CoefficientsPerBlock != 0)
        {
            throw new ArgumentException("Coefficients must contain complete 64-value blocks.", nameof(coefficients));
        }
        if (!Enum.IsDefined(layout))
        {
            throw new ArgumentOutOfRangeException(nameof(layout));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(maximumBytes);
        maximumBytes = Math.Min(maximumBytes, DefaultMaximumBytes);
        cancellationToken.ThrowIfCancellationRequested();
        int blockCount = coefficients.Length / CoefficientsPerBlock;
        var work = new CoefficientCacheWork();
        long payloadBytes = layout == CoefficientCacheLayout.Dense ? (long)coefficients.Length * sizeof(short) : 0;
        Span<byte> widths = stackalloc byte[CoefficientsPerBlock - 1];
        if (layout != CoefficientCacheLayout.Dense)
        {
            for (int first = 0; first < blockCount; first += BlocksPerTile)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int count = Math.Min(BlocksPerTile, blockCount - first);
                int compactBytes = MeasureTile(coefficients.Slice(first * CoefficientsPerBlock,
                    count * CoefficientsPerBlock), layout, widths, work);
                payloadBytes += denseFallback ? Math.Min(compactBytes, count * DenseBlockBytes) : compactBytes;
            }
        }
        long indexBytes = layout == CoefficientCacheLayout.Dense ? 0
            : (long)((blockCount + BlocksPerTile - 1) / BlocksPerTile) * sizeof(int);
        if (payloadBytes > Array.MaxLength || payloadBytes + indexBytes > maximumBytes)
        {
            return null;
        }

        var result = new CoefficientCacheRepresentation(blockCount, layout, payloadBytes,
            pool ?? ArrayPool<byte>.Shared, indexPool ?? ArrayPool<int>.Shared, work);
        try
        {
            if (indexBytes != 0)
            {
                int tileCount = (blockCount + BlocksPerTile - 1) / BlocksPerTile;
                result.tileOffsets = result.indexPool.Rent(tileCount);
                if (result.tileOffsets.Length < tileCount)
                {
                    throw new InvalidOperationException("The index pool returned an undersized array.");
                }
                if (result.RentedIndexBytes > maximumBytes)
                {
                    result.Dispose();
                    return null;
                }
            }
            if (payloadBytes != 0)
            {
                result.payload = result.pool.Rent((int)payloadBytes);
                if (result.payload.Length < payloadBytes)
                {
                    throw new InvalidOperationException("The payload pool returned an undersized array.");
                }
                if (result.RentedBytes > maximumBytes)
                {
                    result.Dispose();
                    return null;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            int position = 0;
            for (int first = 0; first < blockCount; first += BlocksPerTile)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int count = Math.Min(BlocksPerTile, blockCount - first);
                ReadOnlySpan<short> source = coefficients.Slice(first * CoefficientsPerBlock,
                    count * CoefficientsPerBlock);
                int compactBytes = layout == CoefficientCacheLayout.Dense ? count * DenseBlockBytes
                    : MeasureTile(source, layout, widths, work);
                bool dense = layout == CoefficientCacheLayout.Dense
                    || (denseFallback && compactBytes >= count * DenseBlockBytes);
                int bytes = dense ? count * DenseBlockBytes : compactBytes;
                if (result.tileOffsets is not null)
                {
                    result.tileOffsets[first / BlocksPerTile] = dense ? ~position : position;
                    work.Writes++;
                    work.IndexOperations++;
                }
                Span<byte> destination = result.payload.AsSpan(position, bytes);
                if (dense)
                {
                    PackDense(source, destination, work);
                    result.DenseTiles++;
                }
                else
                {
                    if (layout == CoefficientCacheLayout.Bitmap)
                    {
                        PackBitmap(source, destination, work);
                    }
                    else
                    {
                        PackBits(source, destination, widths, work);
                    }
                    result.CompactTiles++;
                }
                position += bytes;
            }
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    internal short GetDc(int block, CoefficientCacheWork? work = null)
        => this.GetCoefficient(block, 0, work);

    internal short GetCoefficient(int block, int coefficient, CoefficientCacheWork? work = null)
    {
        this.ValidateBlock(block);
        ArgumentOutOfRangeException.ThrowIfNegative(coefficient);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(coefficient, CoefficientsPerBlock);
        (int position, int lane, int count, bool dense) = this.LocateBlock(block, work);
        if (dense)
        {
            return ReadShort(this.payload!, position + (lane * DenseBlockBytes) + (coefficient * sizeof(short)), work);
        }
        if (coefficient == 0)
        {
            return ReadShort(this.payload!, position + (lane * sizeof(short)), work);
        }
        if (work is not null)
        {
            work.IndexOperations++;
        }
        if (this.layout == CoefficientCacheLayout.Bitmap)
        {
            ulong mask = ReadMask(this.payload!, position + (count * sizeof(short)) + (lane * sizeof(ulong)), work);
            ulong bit = 1UL << (coefficient - 1);
            work?.BitOperations += 2; // Shift, membership mask.
            if ((mask & bit) == 0)
            {
                return 0;
            }
            int start = ReadUnsignedShort(this.payload!, position + (count * 10) + (lane * sizeof(ushort)), work);
            int rank = BitOperations.PopCount(mask & (bit - 1));
            if (work is not null)
            {
                work.RankOperations++;
                work.BitOperations++;
            }
            return ReadShort(this.payload!, position + (count * 12) + ((start + rank) * sizeof(short)), work);
        }
        int band = coefficient - 1;
        int width = ReadByte(this.payload!, position + (count * sizeof(short)) + band, work);
        int bitOffset = ReadUnsignedShort(this.payload!, position + (count * sizeof(short)) + 63
            + (band * sizeof(ushort)), work);
        return Decode(ReadBits(this.payload!, position + (count * sizeof(short)) + 189,
            bitOffset + (lane * width), width, work), work);
    }

    internal void CopyBlock(int block, Span<short> destination, CoefficientCacheWork? work = null)
    {
        this.ValidateBlock(block);
        if (destination.Length < CoefficientsPerBlock)
        {
            throw new ArgumentException("Destination must hold a complete coefficient block.", nameof(destination));
        }
        (int position, int lane, int count, bool dense) = this.LocateBlock(block, work);
        if (dense)
        {
            for (int coefficient = 0; coefficient < CoefficientsPerBlock; coefficient++)
            {
                destination[coefficient] = ReadShort(this.payload!, position + (lane * DenseBlockBytes)
                    + (coefficient * sizeof(short)), work);
                if (work is not null)
                {
                    work.Writes++;
                }
            }
            return;
        }
        destination[0] = ReadShort(this.payload!, position + (lane * sizeof(short)), work);
        if (work is not null)
        {
            work.Writes++;
        }
        if (this.layout == CoefficientCacheLayout.Bitmap)
        {
            ulong mask = ReadMask(this.payload!, position + (count * sizeof(short)) + (lane * sizeof(ulong)), work);
            int start = ReadUnsignedShort(this.payload!, position + (count * 10) + (lane * sizeof(ushort)), work);
            int packedPosition = position + (count * 12) + (start * sizeof(short));
            if (work is not null)
            {
                work.IndexOperations++;
            }
            for (int coefficient = 1; coefficient < CoefficientsPerBlock; coefficient++)
            {
                bool nonzero = (mask & 1) != 0;
                destination[coefficient] = nonzero ? ReadShort(this.payload!, packedPosition, work) : (short)0;
                if (nonzero)
                {
                    packedPosition += sizeof(short);
                    if (work is not null)
                    {
                        work.IndexOperations++;
                    }
                }
                mask >>= 1;
                if (work is not null)
                {
                    work.Writes++;
                    work.BitOperations += 2;
                }
            }
            return;
        }
        for (int coefficient = 1; coefficient < CoefficientsPerBlock; coefficient++)
        {
            int band = coefficient - 1;
            int width = ReadByte(this.payload!, position + (count * sizeof(short)) + band, work);
            int bitOffset = ReadUnsignedShort(this.payload!, position + (count * sizeof(short)) + 63
                + (band * sizeof(ushort)), work);
            destination[coefficient] = Decode(ReadBits(this.payload!, position + (count * sizeof(short)) + 189,
                bitOffset + (lane * width), width, work), work);
            if (work is not null)
            {
                work.Writes++;
                work.IndexOperations++;
            }
        }
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }
        this.disposed = true;
        byte[]? oldPayload = this.payload;
        int[]? oldOffsets = this.tileOffsets;
        this.payload = null;
        this.tileOffsets = null;
        try
        {
            if (oldPayload is not null)
            {
                this.pool.Return(oldPayload);
            }
        }
        finally
        {
            if (oldOffsets is not null)
            {
                this.indexPool.Return(oldOffsets);
            }
        }
    }

    private void ValidateBlock(int block)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(block);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(block, this.BlockCount);
    }

    private (int Position, int Lane, int Count, bool Dense) LocateBlock(int block, CoefficientCacheWork? work)
    {
        if (work is not null)
        {
            work.IndexOperations++;
        }
        if (this.layout == CoefficientCacheLayout.Dense)
        {
            return (block * DenseBlockBytes, 0, 1, true);
        }
        int tile = block / BlocksPerTile;
        int offset = this.tileOffsets![tile];
        if (work is not null)
        {
            work.Reads++;
        }
        return (offset < 0 ? ~offset : offset, block % BlocksPerTile,
            Math.Min(BlocksPerTile, this.BlockCount - (tile * BlocksPerTile)), offset < 0);
    }

    private static int MeasureTile(ReadOnlySpan<short> source, CoefficientCacheLayout layout,
        Span<byte> widths, CoefficientCacheWork work)
    {
        int count = source.Length / CoefficientsPerBlock;
        if (layout == CoefficientCacheLayout.Bitmap)
        {
            int nonzero = 0;
            for (int block = 0; block < count; block++)
            {
                for (int coefficient = 1; coefficient < CoefficientsPerBlock; coefficient++)
                {
                    if (source[(block * CoefficientsPerBlock) + coefficient] != 0)
                    {
                        nonzero++;
                    }
                    work.Reads++;
                    work.IndexOperations++;
                }
            }
            return (count * 12) + (nonzero * sizeof(short));
        }
        int bits = 0;
        for (int band = 0; band < CoefficientsPerBlock - 1; band++)
        {
            uint maximum = 0;
            for (int block = 0; block < count; block++)
            {
                maximum = Math.Max(maximum, Encode(source[(block * CoefficientsPerBlock) + band + 1], work));
                work.Reads++;
                work.IndexOperations++;
            }
            int width = maximum == 0 ? 0 : 32 - BitOperations.LeadingZeroCount(maximum);
            if (maximum != 0)
            {
                work.BitOperations++;
            }
            widths[band] = (byte)width;
            bits += count * width;
        }
        return (count * sizeof(short)) + 189 + ((bits + 7) / 8);
    }

    private static void PackDense(ReadOnlySpan<short> source, Span<byte> destination, CoefficientCacheWork work)
    {
        for (int coefficient = 0; coefficient < source.Length; coefficient++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(destination.Slice(coefficient * sizeof(short), sizeof(short)),
                source[coefficient]);
            work.Reads++;
            work.Writes += sizeof(short);
            work.IndexOperations++;
        }
    }

    private static void PackBitmap(ReadOnlySpan<short> source, Span<byte> destination, CoefficientCacheWork work)
    {
        int count = source.Length / CoefficientsPerBlock;
        int packed = 0;
        for (int block = 0; block < count; block++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(destination.Slice(block * sizeof(short), sizeof(short)),
                source[block * CoefficientsPerBlock]);
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice((count * 10) + (block * sizeof(ushort)),
                sizeof(ushort)), checked((ushort)packed));
            work.Reads++;
            work.Writes += sizeof(short) + sizeof(ushort);
            work.IndexOperations++;
            ulong mask = 0;
            for (int coefficient = 1; coefficient < CoefficientsPerBlock; coefficient++)
            {
                short value = source[(block * CoefficientsPerBlock) + coefficient];
                work.Reads++;
                work.IndexOperations++;
                if (value != 0)
                {
                    mask |= 1UL << (coefficient - 1);
                    BinaryPrimitives.WriteInt16LittleEndian(destination.Slice((count * 12)
                        + (packed * sizeof(short)), sizeof(short)), value);
                    packed++;
                    work.Writes += sizeof(short);
                    work.BitOperations += 2;
                    work.IndexOperations++;
                }
            }
            BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice((count * sizeof(short))
                + (block * sizeof(ulong)), sizeof(ulong)), mask);
            work.Writes += sizeof(ulong);
        }
    }

    private static void PackBits(ReadOnlySpan<short> source, Span<byte> destination,
        ReadOnlySpan<byte> widths, CoefficientCacheWork work)
    {
        int count = source.Length / CoefficientsPerBlock;
        int dataPosition = (count * sizeof(short)) + 189;
        destination[dataPosition..].Clear();
        work.Writes += destination.Length - dataPosition;
        for (int block = 0; block < count; block++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(destination.Slice(block * sizeof(short), sizeof(short)),
                source[block * CoefficientsPerBlock]);
            work.Reads++;
            work.Writes += sizeof(short);
            work.IndexOperations++;
        }
        int bitOffset = 0;
        for (int band = 0; band < CoefficientsPerBlock - 1; band++)
        {
            int width = widths[band];
            destination[(count * sizeof(short)) + band] = (byte)width;
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice((count * sizeof(short)) + 63
                + (band * sizeof(ushort)), sizeof(ushort)), checked((ushort)bitOffset));
            work.Writes += 1 + sizeof(ushort);
            for (int block = 0; block < count; block++)
            {
                uint value = Encode(source[(block * CoefficientsPerBlock) + band + 1], work);
                work.Reads++;
                work.IndexOperations += 2; // Source coefficient and packed AC position.
                WriteBits(destination[dataPosition..], bitOffset + (block * width), width, value, work);
            }
            bitOffset += count * width;
        }
    }

    private static uint Encode(short value, CoefficientCacheWork work)
    {
        work.BitOperations += 3;
        return (uint)((value << 1) ^ (value >> 15));
    }

    private static short Decode(uint value, CoefficientCacheWork? work)
    {
        work?.BitOperations += 3;
        return (short)((value >> 1) ^ (uint)-(int)(value & 1));
    }

    private static void WriteBits(Span<byte> destination, int bitOffset, int width, uint value,
        CoefficientCacheWork work)
    {
        while (width != 0)
        {
            int bytePosition = bitOffset / 8;
            int shift = bitOffset % 8;
            int take = Math.Min(width, 8 - shift);
            uint mask = (1U << take) - 1;
            destination[bytePosition] |= (byte)((value & mask) << shift);
            value >>= take;
            bitOffset += take;
            width -= take;
            work.Reads++;
            work.Writes++;
            work.BitOperations += 5; // Mask shift, value mask/shift, OR, remaining-value shift.
        }
    }

    private static uint ReadBits(byte[] source, int start, int bitOffset, int width,
        CoefficientCacheWork? work)
    {
        uint result = 0;
        int produced = 0;
        while (width != 0)
        {
            int bytePosition = bitOffset / 8;
            int shift = bitOffset % 8;
            int take = Math.Min(width, 8 - shift);
            uint mask = (1U << take) - 1;
            result |= (((uint)source[start + bytePosition] >> shift) & mask) << produced;
            bitOffset += take;
            width -= take;
            produced += take;
            if (work is not null)
            {
                work.Reads++;
                work.BitOperations += 5;
            }
        }
        return result;
    }

    private static byte ReadByte(byte[] source, int position, CoefficientCacheWork? work)
    {
        if (work is not null)
        {
            work.Reads++;
        }
        return source[position];
    }

    private static short ReadShort(byte[] source, int position, CoefficientCacheWork? work)
    {
        work?.Reads += sizeof(short);
        return BinaryPrimitives.ReadInt16LittleEndian(source.AsSpan(position, sizeof(short)));
    }

    private static ushort ReadUnsignedShort(byte[] source, int position, CoefficientCacheWork? work)
    {
        work?.Reads += sizeof(ushort);
        return BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(position, sizeof(ushort)));
    }

    private static ulong ReadMask(byte[] source, int position, CoefficientCacheWork? work)
    {
        work?.Reads += sizeof(ulong);
        return BinaryPrimitives.ReadUInt64LittleEndian(source.AsSpan(position, sizeof(ulong)));
    }
}
