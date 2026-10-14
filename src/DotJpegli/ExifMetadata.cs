using System.Buffers.Binary;
using System.Text;

namespace DotJpegli;

/// <summary>A directory in a classic TIFF Exif body.</summary>
public enum ExifDirectory
{
    Image,
    Photo,
    Gps,
    Interoperability,
    Thumbnail,
}

/// <summary>The standard, relocatable TIFF value representations supported by <see cref="ExifMetadata"/>.</summary>
public enum ExifFieldType : ushort
{
    Byte = 1,
    Ascii = 2,
    UnsignedShort = 3,
    UnsignedLong = 4,
    UnsignedRational = 5,
    SignedByte = 6,
    Undefined = 7,
    SignedShort = 8,
    SignedLong = 9,
    SignedRational = 10,
    Float = 11,
    Double = 12,
}

/// <summary>A copied TIFF entry value, with numerical components encoded in little-endian order.</summary>
public sealed class ExifEntry
{
    internal ExifEntry(ExifFieldType fieldType, uint count, byte[] value)
    {
        FieldType = fieldType;
        Count = count;
        Value = value;
    }

    public ExifFieldType FieldType { get; }
    public uint Count { get; }
    public ReadOnlyMemory<byte> Value { get; }
}

/// <summary>Parses and edits the TIFF body supplied to <see cref="JpegliEncoderOptions.ExifData"/>.</summary>
/// <remarks>
/// Input excludes the JPEG <c>Exif\0\0</c> signature. Both classic TIFF byte orders are accepted;
/// output and raw entry values use little-endian order. Unknown tags with standard TIFF field
/// types are preserved. Exif, GPS and interoperability directory links are rebuilt automatically,
/// and a bounded JPEG thumbnail is copied with a rebased offset. BigTIFF, additional next-IFD
/// chains, SubIFDs and nonstandard field types are unsupported. Serialization rejects MakerNote,
/// strip/tile/free-offset and other known offset-sensitive entries until callers remove them,
/// because opaque embedded offsets cannot be safely relocated. Arbitrary private tags may also
/// contain offsets: callers must remove such tags when their format requires fixed positions.
/// Parsing rejects inputs whose aggregate value lengths exceed the input length, preventing
/// repeated aliased offsets from causing unbounded memory amplification.
/// The editor is mutable and is not thread safe; it does not interpret metadata semantics.
/// </remarks>
public sealed class ExifMetadata
{
    private const ushort PhotoPointer = 0x8769;
    private const ushort GpsPointer = 0x8825;
    private const ushort InteroperabilityPointer = 0xa005;
    private const ushort ThumbnailOffset = 0x0201;
    private const ushort ThumbnailLength = 0x0202;
    private readonly Dictionary<ExifDirectory, SortedDictionary<ushort, ExifEntry>> _directories = [];
    private byte[]? _jpegThumbnail;

    /// <summary>Creates empty Exif metadata.</summary>
    public ExifMetadata()
    {
        foreach (ExifDirectory directory in Enum.GetValues<ExifDirectory>())
            _directories.Add(directory, []);
    }

    /// <summary>Parses a bounded classic TIFF body, copying all retained values.</summary>
    /// <exception cref="InvalidDataException">The body contains malformed or cyclic TIFF structures.</exception>
    /// <exception cref="NotSupportedException">The body requires unsupported TIFF structures or field types.</exception>
    public static ExifMetadata Parse(ReadOnlySpan<byte> tiff)
    {
        if (tiff.Length < 8 || !((tiff[0] == 'I' && tiff[1] == 'I') || (tiff[0] == 'M' && tiff[1] == 'M')))
            throw Invalid("The TIFF header is missing or invalid.");
        bool littleEndian = tiff[0] == 'I';
        if (Read16(tiff[2..], littleEndian) != 42)
            throw Invalid("Only a classic TIFF header with magic 42 is valid.");

        ExifMetadata result = new();
        List<(ExifDirectory Directory, uint Offset)> pending = [(ExifDirectory.Image, Read32(tiff[4..], littleEndian))];
        HashSet<uint> visited = [];
        List<(int Start, int Length)> directoryRanges = [];
        List<(int Start, int Length)> valueRanges = [];
        long totalValueBytes = 0;
        for (int index = 0; index < pending.Count; index++)
        {
            (ExifDirectory directory, uint offset) = pending[index];
            if (offset == 0)
                continue;
            if (!visited.Add(offset))
                throw Invalid("TIFF directory links contain a cycle or share a directory.");
            int start = CheckedRange(tiff, offset, 2);
            ushort count = Read16(tiff[start..], littleEndian);
            int directoryBytes = 6 + 12 * count;
            CheckedRange(tiff, offset, directoryBytes);
            if (directoryRanges.Any(range => Overlaps(start, directoryBytes, range.Start, range.Length)))
                throw Invalid("TIFF directories overlap.");
            directoryRanges.Add((start, directoryBytes));
            HashSet<ushort> tags = [];
            for (int entryIndex = 0; entryIndex < count; entryIndex++)
            {
                ReadOnlySpan<byte> entryBytes = tiff.Slice(start + 2 + 12 * entryIndex, 12);
                ushort tag = Read16(entryBytes, littleEndian);
                if (!tags.Add(tag))
                    throw Invalid("A TIFF directory contains duplicate tags.");
                ExifFieldType fieldType = (ExifFieldType)Read16(entryBytes[2..], littleEndian);
                int elementSize = SizeOf(fieldType);
                uint valueCount = Read32(entryBytes[4..], littleEndian);
                ulong byteCount = (ulong)valueCount * (uint)elementSize;
                if (byteCount > int.MaxValue)
                    throw Invalid("A TIFF value count exceeds the supported buffer size.");
                int length = (int)byteCount;
                totalValueBytes += length;
                if (totalValueBytes > tiff.Length)
                    throw Invalid("The aggregate TIFF value size exceeds the input body size.");
                ReadOnlySpan<byte> value;
                if (length <= 4)
                    value = entryBytes.Slice(8, length);
                else
                {
                    int valueStart = CheckedRange(tiff, Read32(entryBytes[8..], littleEndian), length);
                    valueRanges.Add((valueStart, length));
                    value = tiff.Slice(valueStart, length);
                }

                if (IsDirectoryPointer(tag))
                {
                    if (fieldType != ExifFieldType.UnsignedLong || valueCount != 1)
                        throw Invalid("A directory pointer must be a single unsigned LONG.");
                    ExifDirectory child = (directory, tag) switch
                    {
                        (ExifDirectory.Image, PhotoPointer) => ExifDirectory.Photo,
                        (ExifDirectory.Image, GpsPointer) => ExifDirectory.Gps,
                        (ExifDirectory.Photo, InteroperabilityPointer) => ExifDirectory.Interoperability,
                        _ => throw Invalid("A directory pointer occurs in an unsupported parent directory."),
                    };
                    uint childOffset = Read32(value, littleEndian);
                    if (childOffset == 0)
                        throw Invalid("A directory pointer cannot be zero.");
                    pending.Add((child, childOffset));
                    continue;
                }

                byte[] copied = value.ToArray();
                if (!littleEndian)
                    ReverseComponents(copied, fieldType);
                result._directories[directory].Add(tag, new(fieldType, valueCount, copied));
            }

            uint next = Read32(tiff.Slice(start + 2 + 12 * count, 4), littleEndian);
            if (next != 0)
            {
                if (directory != ExifDirectory.Image)
                    throw Invalid("Only the image directory can link to a thumbnail next-IFD.");
                pending.Add((ExifDirectory.Thumbnail, next));
            }
        }

        foreach ((int start, int length) in valueRanges)
            if (directoryRanges.Any(range => Overlaps(start, length, range.Start, range.Length)))
                throw Invalid("A TIFF value overlaps a directory.");

        SortedDictionary<ushort, ExifEntry> thumbnail = result._directories[ExifDirectory.Thumbnail];
        bool hasOffset = thumbnail.TryGetValue(ThumbnailOffset, out ExifEntry? offsetEntry);
        bool hasLength = thumbnail.TryGetValue(ThumbnailLength, out ExifEntry? lengthEntry);
        if (hasOffset != hasLength)
            throw Invalid("A JPEG thumbnail requires both offset and length.");
        if (hasOffset)
        {
            uint thumbnailOffset = RequireLong(offsetEntry!);
            uint thumbnailLength = RequireLong(lengthEntry!);
            if (thumbnailLength == 0 || thumbnailLength > int.MaxValue)
                throw Invalid("A JPEG thumbnail length is invalid.");
            int thumbnailStart = CheckedRange(tiff, thumbnailOffset, (int)thumbnailLength);
            if (directoryRanges.Concat(valueRanges).Any(range => Overlaps(thumbnailStart, (int)thumbnailLength, range.Start, range.Length)))
                throw Invalid("A JPEG thumbnail overlaps TIFF metadata.");
            result._jpegThumbnail = tiff.Slice(thumbnailStart, (int)thumbnailLength).ToArray();
        }
        return result;
    }

    /// <summary>Returns an independent copy of an entry, or null when absent.</summary>
    public ExifEntry? GetEntry(ExifDirectory directory, ushort tag)
    {
        return GetDirectory(directory).TryGetValue(tag, out ExifEntry? entry)
            ? new(entry.FieldType, entry.Count, entry.Value.ToArray()) : null;
    }

    /// <summary>Sets a standard TIFF entry; numerical components in <paramref name="value"/> must be little endian.</summary>
    /// <remarks>Directory links and JPEG thumbnail offsets are managed by the editor and cannot be set directly.</remarks>
    public void SetEntry(ExifDirectory directory, ushort tag, ExifFieldType fieldType, uint count, ReadOnlySpan<byte> value)
    {
        SortedDictionary<ushort, ExifEntry> entries = GetDirectory(directory);
        if (IsDirectoryPointer(tag) || tag is ThumbnailOffset or ThumbnailLength)
            throw new ArgumentException("Directory and thumbnail offsets are managed by the editor.", nameof(tag));
        if ((ulong)count * (uint)SizeOf(fieldType) != (ulong)value.Length)
            throw new ArgumentException("The value length must match its TIFF field type and count.", nameof(value));
        entries[tag] = new(fieldType, count, value.ToArray());
    }

    /// <summary>Reads a single unsigned SHORT or LONG, or null if the tag is absent or has another representation.</summary>
    public uint? GetUnsignedInteger(ExifDirectory directory, ushort tag)
    {
        ExifEntry? entry = GetEntry(directory, tag);
        if (entry?.Count != 1)
            return null;
        return entry.FieldType switch
        {
            ExifFieldType.UnsignedShort => BinaryPrimitives.ReadUInt16LittleEndian(entry.Value.Span),
            ExifFieldType.UnsignedLong => BinaryPrimitives.ReadUInt32LittleEndian(entry.Value.Span),
            _ => null,
        };
    }

    public void SetUnsignedShort(ExifDirectory directory, ushort tag, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        SetEntry(directory, tag, ExifFieldType.UnsignedShort, 1, bytes);
    }

    public void SetUnsignedLong(ExifDirectory directory, ushort tag, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        SetEntry(directory, tag, ExifFieldType.UnsignedLong, 1, bytes);
    }

    /// <summary>Reads an ASCII tag without its final NUL, or null if absent or not ASCII.</summary>
    /// <exception cref="InvalidDataException">An ASCII value is not terminated or contains non-ASCII bytes.</exception>
    public string? GetAscii(ExifDirectory directory, ushort tag)
    {
        ExifEntry? entry = GetEntry(directory, tag);
        if (entry?.FieldType != ExifFieldType.Ascii)
            return null;
        ReadOnlySpan<byte> value = entry.Value.Span;
        if (value.IsEmpty || value[^1] != 0 || value.ContainsAnyInRange((byte)128, byte.MaxValue))
            throw Invalid("An ASCII entry must contain ASCII bytes and end in NUL.");
        return Encoding.ASCII.GetString(value[..^1]);
    }

    /// <summary>Sets a NUL-terminated ASCII string.</summary>
    public void SetAscii(ExifDirectory directory, ushort tag, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.AsSpan().Contains('\0') || value.Any(character => character > 127))
            throw new ArgumentException("The string must contain ASCII characters without embedded NULs.", nameof(value));
        byte[] bytes = new byte[checked(value.Length + 1)];
        Encoding.ASCII.GetBytes(value, bytes);
        SetEntry(directory, tag, ExifFieldType.Ascii, (uint)bytes.Length, bytes);
    }

    /// <summary>Sets a single unsigned rational. Zero denominators are rejected.</summary>
    public void SetUnsignedRational(ExifDirectory directory, ushort tag, uint numerator, uint denominator)
    {
        if (denominator == 0)
            throw new ArgumentOutOfRangeException(nameof(denominator));
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, numerator);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], denominator);
        SetEntry(directory, tag, ExifFieldType.UnsignedRational, 1, bytes);
    }

    /// <summary>Removes an entry; removing a JPEG thumbnail offset or length removes the thumbnail and both tags.</summary>
    public bool Remove(ExifDirectory directory, ushort tag)
    {
        SortedDictionary<ushort, ExifEntry> entries = GetDirectory(directory);
        if (directory == ExifDirectory.Thumbnail && tag is ThumbnailOffset or ThumbnailLength)
        {
            _jpegThumbnail = null;
            bool removed = entries.Remove(ThumbnailOffset);
            return entries.Remove(ThumbnailLength) || removed;
        }
        return entries.Remove(tag);
    }

    /// <summary>Returns an independent copy of the JPEG thumbnail, or null when absent.</summary>
    public byte[]? GetJpegThumbnail() => _jpegThumbnail?.ToArray();

    /// <summary>Removes the thumbnail image and its complete directory, including any stale dimensions or orientation.</summary>
    public void RemoveThumbnail()
    {
        _jpegThumbnail = null;
        _directories[ExifDirectory.Thumbnail].Clear();
    }

    /// <summary>Sets JPEG thumbnail bytes, or removes the thumbnail when empty.</summary>
    public void SetJpegThumbnail(ReadOnlySpan<byte> value)
    {
        Remove(ExifDirectory.Thumbnail, ThumbnailOffset);
        if (value.IsEmpty)
            return;
        _jpegThumbnail = value.ToArray();
        SetUnsignedShort(ExifDirectory.Thumbnail, 0x0103, 6);
        _directories[ExifDirectory.Thumbnail][ThumbnailOffset] = LongEntry(0);
        _directories[ExifDirectory.Thumbnail][ThumbnailLength] = LongEntry((uint)value.Length);
    }

    /// <summary>Writes classic little-endian TIFF, rebuilding directory links and rebasing a JPEG thumbnail.</summary>
    /// <exception cref="NotSupportedException">A retained entry has known opaque or unsupported embedded offsets.</exception>
    public byte[] ToArray()
    {
        Dictionary<ExifDirectory, SortedDictionary<ushort, ExifEntry>> directories = [];
        foreach ((ExifDirectory directory, SortedDictionary<ushort, ExifEntry> entries) in _directories)
        {
            foreach (ushort tag in entries.Keys)
                if (IsOffsetSensitive(tag) || (tag is ThumbnailOffset or ThumbnailLength && directory != ExifDirectory.Thumbnail))
                    throw new NotSupportedException($"Exif tag 0x{tag:X4} contains offsets that cannot be safely relocated; remove it before serialization.");
            if (entries.Count != 0 || directory == ExifDirectory.Image)
                directories.Add(directory, new(entries));
        }
        if (directories.ContainsKey(ExifDirectory.Interoperability))
        {
            if (!directories.TryGetValue(ExifDirectory.Photo, out SortedDictionary<ushort, ExifEntry>? photo))
                directories.Add(ExifDirectory.Photo, photo = []);
            photo[InteroperabilityPointer] = LongEntry(0);
        }
        SortedDictionary<ushort, ExifEntry> image = directories[ExifDirectory.Image];
        if (directories.ContainsKey(ExifDirectory.Photo))
            image[PhotoPointer] = LongEntry(0);
        if (directories.ContainsKey(ExifDirectory.Gps))
            image[GpsPointer] = LongEntry(0);

        Dictionary<ExifDirectory, int> offsets = [];
        long total = 8;
        foreach ((ExifDirectory directory, SortedDictionary<ushort, ExifEntry> entries) in directories.OrderBy(pair => pair.Key))
        {
            if (entries.Count > ushort.MaxValue)
                throw new InvalidOperationException("A TIFF directory cannot contain more than 65535 entries.");
            offsets.Add(directory, checked((int)total));
            total += 6L + 12L * entries.Count;
        }
        if (offsets.TryGetValue(ExifDirectory.Photo, out int photoOffset))
            image[PhotoPointer] = LongEntry((uint)photoOffset);
        if (offsets.TryGetValue(ExifDirectory.Gps, out int gpsOffset))
            image[GpsPointer] = LongEntry((uint)gpsOffset);
        if (offsets.TryGetValue(ExifDirectory.Interoperability, out int interoperabilityOffset))
            directories[ExifDirectory.Photo][InteroperabilityPointer] = LongEntry((uint)interoperabilityOffset);
        int dataStart = checked((int)total);
        foreach (ExifEntry entry in directories.Values.SelectMany(entries => entries.Values))
            if (entry.Value.Length > 4)
                total += entry.Value.Length + (entry.Value.Length & 1);
        int jpegOffset = checked((int)total);
        if (_jpegThumbnail is not null)
        {
            total += _jpegThumbnail.Length;
            directories[ExifDirectory.Thumbnail][ThumbnailOffset] = LongEntry((uint)jpegOffset);
            directories[ExifDirectory.Thumbnail][ThumbnailLength] = LongEntry((uint)_jpegThumbnail.Length);
        }
        byte[] bytes = new byte[checked((int)total)];
        bytes[0] = bytes[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), 42);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)offsets[ExifDirectory.Image]);
        int dataOffset = dataStart;
        foreach ((ExifDirectory directory, SortedDictionary<ushort, ExifEntry> entries) in directories.OrderBy(pair => pair.Key))
        {
            int start = offsets[directory];
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(start), (ushort)entries.Count);
            int entryOffset = start + 2;
            foreach ((ushort tag, ExifEntry entry) in entries)
            {
                Span<byte> destination = bytes.AsSpan(entryOffset, 12);
                BinaryPrimitives.WriteUInt16LittleEndian(destination, tag);
                BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], (ushort)entry.FieldType);
                BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], entry.Count);
                if (entry.Value.Length <= 4)
                    entry.Value.Span.CopyTo(destination[8..]);
                else
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], (uint)dataOffset);
                    entry.Value.Span.CopyTo(bytes.AsSpan(dataOffset));
                    dataOffset += entry.Value.Length + (entry.Value.Length & 1);
                }
                entryOffset += 12;
            }
            if (directory == ExifDirectory.Image && offsets.TryGetValue(ExifDirectory.Thumbnail, out int thumbnailOffset))
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(entryOffset), (uint)thumbnailOffset);
        }
        _jpegThumbnail?.CopyTo(bytes, jpegOffset);
        return bytes;
    }

    private SortedDictionary<ushort, ExifEntry> GetDirectory(ExifDirectory directory)
    {
        if (!_directories.TryGetValue(directory, out SortedDictionary<ushort, ExifEntry>? entries))
            throw new ArgumentOutOfRangeException(nameof(directory));
        return entries;
    }

    private static ExifEntry LongEntry(uint value)
    {
        byte[] bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return new(ExifFieldType.UnsignedLong, 1, bytes);
    }

    private static bool IsDirectoryPointer(ushort tag) => tag is PhotoPointer or GpsPointer or InteroperabilityPointer;
    private static bool IsOffsetSensitive(ushort tag) => tag is 0x0111 or 0x0117 or 0x0120 or 0x0121 or 0x0144 or 0x0145 or 0x014a or 0x0207 or 0x0208 or 0x0209 or 0x927c;

    private static int SizeOf(ExifFieldType fieldType) => fieldType switch
    {
        ExifFieldType.Byte or ExifFieldType.Ascii or ExifFieldType.SignedByte or ExifFieldType.Undefined => 1,
        ExifFieldType.UnsignedShort or ExifFieldType.SignedShort => 2,
        ExifFieldType.UnsignedLong or ExifFieldType.SignedLong or ExifFieldType.Float => 4,
        ExifFieldType.UnsignedRational or ExifFieldType.SignedRational or ExifFieldType.Double => 8,
        _ => throw new NotSupportedException($"TIFF field type {(ushort)fieldType} is unsupported."),
    };

    private static void ReverseComponents(Span<byte> value, ExifFieldType fieldType)
    {
        int size = fieldType is ExifFieldType.UnsignedRational or ExifFieldType.SignedRational ? 4 : SizeOf(fieldType);
        if (size > 1)
            for (int start = 0; start < value.Length; start += size)
                value.Slice(start, size).Reverse();
    }

    private static uint RequireLong(ExifEntry entry)
    {
        if (entry.FieldType != ExifFieldType.UnsignedLong || entry.Count != 1)
            throw Invalid("A JPEG thumbnail pointer or length must be a single unsigned LONG.");
        return BinaryPrimitives.ReadUInt32LittleEndian(entry.Value.Span);
    }

    private static int CheckedRange(ReadOnlySpan<byte> tiff, uint offset, int length)
    {
        if (offset < 8 || offset > (uint)tiff.Length || (ulong)length > (ulong)tiff.Length - offset)
            throw Invalid("A TIFF offset or count points outside the body.");
        return (int)offset;
    }

    private static bool Overlaps(int firstStart, int firstLength, int secondStart, int secondLength)
        => (long)firstStart < (long)secondStart + secondLength && (long)secondStart < (long)firstStart + firstLength;
    private static ushort Read16(ReadOnlySpan<byte> value, bool littleEndian) => littleEndian
        ? BinaryPrimitives.ReadUInt16LittleEndian(value) : BinaryPrimitives.ReadUInt16BigEndian(value);
    private static uint Read32(ReadOnlySpan<byte> value, bool littleEndian) => littleEndian
        ? BinaryPrimitives.ReadUInt32LittleEndian(value) : BinaryPrimitives.ReadUInt32BigEndian(value);
    private static InvalidDataException Invalid(string message) => new(message);
}
