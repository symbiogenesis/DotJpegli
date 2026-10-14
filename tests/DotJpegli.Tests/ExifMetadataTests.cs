using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ExifMetadataTests
{
    [Fact]
    public void EmptyEditorWritesACompleteClassicTiffDirectory()
    {
        Assert.Equal(Convert.FromHexString("49492A0008000000000000000000"), new ExifMetadata().ToArray());
        Assert.Null(ExifMetadata.Parse(Convert.FromHexString("49492A0000000000"))
            .GetUnsignedInteger(ExifDirectory.Image, 0x0100));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParsesIndependentBinaryDirectoriesAndBothByteOrders(bool littleEndian)
    {
        byte[] input = DirectoryFixture(littleEndian);
        ExifMetadata metadata = ExifMetadata.Parse(input);
        Assert.Equal(513u, metadata.GetUnsignedInteger(ExifDirectory.Image, 0x0100));
        Assert.Equal("camera", metadata.GetAscii(ExifDirectory.Image, 0x010e));
        Assert.Equal("N", metadata.GetAscii(ExifDirectory.Gps, 1));
        Assert.Equal("R98", metadata.GetAscii(ExifDirectory.Interoperability, 1));
        ExifEntry exposure = Assert.IsType<ExifEntry>(metadata.GetEntry(ExifDirectory.Photo, 0x829a));
        Assert.Equal(ExifFieldType.UnsignedRational, exposure.FieldType);
        Assert.Equal(1u, exposure.Count);
        Assert.Equal(Convert.FromHexString("010000007D000000"), exposure.Value.ToArray());
        Assert.Equal(Convert.FromHexString("FEFF2C010080"), metadata.GetEntry(ExifDirectory.Photo, 0xc001)!.Value.ToArray());
        Assert.Equal(Convert.FromHexString("33000000010000001E000000010000000000000001000000"),
            metadata.GetEntry(ExifDirectory.Gps, 2)!.Value.ToArray());
        Array.Fill(input, (byte)0);
        Assert.Equal("camera", metadata.GetAscii(ExifDirectory.Image, 0x010e));
    }

    [Fact]
    public void TypedEditsProduceIndependentlyReadableTiffValuesAndLinks()
    {
        ExifMetadata metadata = ExifMetadata.Parse(DirectoryFixture(false));
        metadata.SetUnsignedLong(ExifDirectory.Image, 0x0100, 70_000);
        metadata.SetUnsignedShort(ExifDirectory.Image, 0x0112, 1);
        metadata.SetAscii(ExifDirectory.Image, 0x010e, "edited camera");
        metadata.SetUnsignedRational(ExifDirectory.Photo, 0x829a, 3, 200);
        Assert.True(metadata.Remove(ExifDirectory.Photo, 0xc001));
        Assert.False(metadata.Remove(ExifDirectory.Photo, 0xc001));
        byte[] output = metadata.ToArray();
        Assert.Equal("MM"u8.ToArray(), output[..2]);
        Assert.Equal(42, U16(output, 2));
        int image = (int)U32(output, 4);
        AssertEntry(ExifFieldType.UnsignedLong, 1, Convert.FromHexString("00011170"), ReadEntry(output, image, 0x0100));
        AssertEntry(ExifFieldType.UnsignedShort, 1, Convert.FromHexString("0001"), ReadEntry(output, image, 0x0112));
        Assert.Equal("edited camera\0"u8.ToArray(), ReadEntry(output, image, 0x010e).Value);
        int photo = (int)BinaryPrimitives.ReadUInt32BigEndian(ReadEntry(output, image, 0x8769).Value);
        Assert.Equal(Convert.FromHexString("00000003000000C8"), ReadEntry(output, photo, 0x829a).Value);
        Assert.DoesNotContain((ushort)0xc001, Tags(output, photo));
        int interoperability = (int)BinaryPrimitives.ReadUInt32BigEndian(ReadEntry(output, photo, 0xa005).Value);
        Assert.Equal("R98\0"u8.ToArray(), ReadEntry(output, interoperability, 1).Value);
        int gps = (int)BinaryPrimitives.ReadUInt32BigEndian(ReadEntry(output, image, 0x8825).Value);
        Assert.Equal("N\0"u8.ToArray(), ReadEntry(output, gps, 1).Value);
    }

    [Theory]
    [InlineData(ExifFieldType.Byte, 1, "AB")]
    [InlineData(ExifFieldType.SignedByte, 1, "FE")]
    [InlineData(ExifFieldType.Undefined, 6, "001122334455")]
    [InlineData(ExifFieldType.SignedShort, 2, "FEFF2C01")]
    [InlineData(ExifFieldType.SignedLong, 1, "FEFFFFFF")]
    [InlineData(ExifFieldType.SignedRational, 1, "FEFFFFFF03000000")]
    [InlineData(ExifFieldType.Float, 1, "0000803F")]
    [InlineData(ExifFieldType.Double, 1, "000000000000F03F")]
    public void RawEntriesPreserveStandardTypesAndAreCopied(ExifFieldType type, uint count, string hex)
    {
        ExifMetadata metadata = new();
        byte[] raw = Convert.FromHexString(hex);
        metadata.SetEntry(ExifDirectory.Image, 0xc100, type, count, raw);
        Array.Fill(raw, (byte)0);
        ExifEntry copy = metadata.GetEntry(ExifDirectory.Image, 0xc100)!;
        Assert.True(MemoryMarshal.TryGetArray(copy.Value, out ArraySegment<byte> segment));
        segment.Array![segment.Offset] ^= 255;
        byte[] output = metadata.ToArray();
        AssertEntry(type, count, Convert.FromHexString(hex), ReadEntry(output, 8, 0xc100));
    }

    [Fact]
    public void TypedGettersDoNotCoerceDifferentRepresentations()
    {
        ExifMetadata metadata = new();
        metadata.SetAscii(ExifDirectory.Image, 1, "abc");
        metadata.SetEntry(ExifDirectory.Image, 2, ExifFieldType.UnsignedShort, 2, new byte[4]);
        Assert.Null(metadata.GetUnsignedInteger(ExifDirectory.Image, 1));
        Assert.Null(metadata.GetUnsignedInteger(ExifDirectory.Image, 2));
        Assert.Null(metadata.GetUnsignedInteger(ExifDirectory.Image, 3));
        Assert.Null(metadata.GetAscii(ExifDirectory.Image, 2));
        Assert.Null(metadata.GetAscii(ExifDirectory.Image, 3));
    }

    [Fact]
    public void SettersValidateTypesCountsTextAndManagedPointers()
    {
        ExifMetadata metadata = new();
        Assert.Throws<ArgumentException>(() => metadata.SetAscii(ExifDirectory.Image, 1, "café"));
        Assert.Throws<ArgumentException>(() => metadata.SetAscii(ExifDirectory.Image, 1, "a\0b"));
        Assert.Throws<ArgumentNullException>(() => metadata.SetAscii(ExifDirectory.Image, 1, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => metadata.SetUnsignedRational(ExifDirectory.Photo, 1, 2, 0));
        Assert.Throws<ArgumentException>(() => metadata.SetEntry(ExifDirectory.Image, 1, ExifFieldType.UnsignedLong, uint.MaxValue, new byte[4]));
        Assert.Throws<NotSupportedException>(() => metadata.SetEntry(ExifDirectory.Image, 1, (ExifFieldType)13, 1, new byte[4]));
        Assert.Throws<ArgumentException>(() => metadata.SetUnsignedLong(ExifDirectory.Image, 0x8769, 100));
        Assert.Throws<ArgumentException>(() => metadata.SetUnsignedLong(ExifDirectory.Thumbnail, 0x0201, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => metadata.Remove((ExifDirectory)200, 1));
        metadata.SetEntry(ExifDirectory.Image, 1, ExifFieldType.Ascii, 1, new byte[] { 128 });
        Assert.Throws<InvalidDataException>(() => metadata.GetAscii(ExifDirectory.Image, 1));
    }

    [Fact]
    public void ThumbnailIsCopiedAndOffsetRebasedAfterEdits()
    {
        ExifMetadata metadata = ExifMetadata.Parse(ThumbnailFixture());
        Assert.Equal(Convert.FromHexString("FFD8FFD9"), metadata.GetJpegThumbnail());
        metadata.SetAscii(ExifDirectory.Image, 0x010e, "moves the thumbnail after newly allocated text");
        byte[] output = metadata.ToArray();
        int imageCount = U16(output, 8);
        int thumbnail = (int)U32(output, 8 + 2 + imageCount * 12);
        Assert.NotEqual(26, thumbnail);
        uint jpegOffset = BinaryPrimitives.ReadUInt32LittleEndian(ReadEntry(output, thumbnail, 0x0201).Value);
        Assert.Equal(4u, BinaryPrimitives.ReadUInt32LittleEndian(ReadEntry(output, thumbnail, 0x0202).Value));
        Assert.NotEqual(68u, jpegOffset);
        Assert.Equal(Convert.FromHexString("FFD8FFD9"), output.AsSpan((int)jpegOffset, 4).ToArray());
        ExifMetadata parsed = ExifMetadata.Parse(output);
        parsed.RemoveThumbnail();
        Assert.Null(parsed.GetJpegThumbnail());
        Assert.Equal(0u, U32(parsed.ToArray(), 10 + 12 * U16(parsed.ToArray(), 8)));
        Assert.Null(parsed.GetUnsignedInteger(ExifDirectory.Thumbnail, 0x0103));
    }

    [Fact]
    public void ThumbnailCanBeSetOrRemovedThroughItsManagedTags()
    {
        ExifMetadata metadata = new();
        byte[] thumbnail = Convert.FromHexString("FFD8FFD9");
        metadata.SetJpegThumbnail(thumbnail);
        thumbnail[0] = 0;
        Assert.Equal(Convert.FromHexString("FFD8FFD9"), ExifMetadata.Parse(metadata.ToArray()).GetJpegThumbnail());
        Assert.True(metadata.Remove(ExifDirectory.Thumbnail, 0x0202));
        Assert.Null(metadata.GetJpegThumbnail());
        Assert.Null(metadata.GetEntry(ExifDirectory.Thumbnail, 0x0201));
        metadata.SetJpegThumbnail(new byte[] { 1, 2 });
        metadata.SetJpegThumbnail([]);
        Assert.Null(metadata.GetJpegThumbnail());
    }

    [Theory]
    [InlineData(0x927c)]
    [InlineData(0x0111)]
    [InlineData(0x0117)]
    [InlineData(0x0144)]
    [InlineData(0x0145)]
    [InlineData(0x014a)]
    [InlineData(0x0207)]
    public void KnownOffsetSensitiveValuesRequireExplicitRemoval(ushort tag)
    {
        byte[] input = new byte[26];
        Header(input, true);
        W16(input, 8, 1, true);
        Entry(input, 10, tag, 7, 4, 0x01020304, true);
        ExifMetadata metadata = ExifMetadata.Parse(input);
        Assert.NotNull(metadata.GetEntry(ExifDirectory.Image, tag));
        Assert.Throws<NotSupportedException>(metadata.ToArray);
        Assert.True(metadata.Remove(ExifDirectory.Image, tag));
        Assert.Equal(new ExifMetadata().ToArray(), metadata.ToArray());
    }

    [Theory]
    [InlineData("short header")]
    [InlineData("byte order")]
    [InlineData("magic")]
    [InlineData("root offset")]
    [InlineData("directory count")]
    [InlineData("value count")]
    [InlineData("value offset")]
    [InlineData("value overlaps directory")]
    [InlineData("pointer representation")]
    [InlineData("zero pointer")]
    [InlineData("cycle")]
    [InlineData("shared directory")]
    [InlineData("extra next ifd")]
    [InlineData("duplicate tag")]
    public void RejectsMalformedBinaryTiff(string kind)
    {
        byte[] input = DirectoryFixture(true);
        switch (kind)
        {
            case "short header": input = new byte[7]; break;
            case "byte order": input[0] = (byte)'Z'; break;
            case "magic": W16(input, 2, 43, true); break;
            case "root offset": W32(input, 4, uint.MaxValue, true); break;
            case "directory count": W16(input, 8, ushort.MaxValue, true); break;
            case "value count": W32(input, 26, uint.MaxValue, true); break;
            case "value offset": W32(input, 30, uint.MaxValue, true); break;
            case "value overlaps directory": W32(input, 30, 10, true); break;
            case "pointer representation": W16(input, 36, 3, true); break;
            case "zero pointer": W32(input, 42, 0, true); break;
            case "cycle": W32(input, 42, 8, true); break;
            case "shared directory": W32(input, 54, 64, true); break;
            case "extra next ifd": W32(input, 102, 8, true); break;
            case "duplicate tag": W16(input, 22, 0x0100, true); break;
        }
        Assert.Throws<InvalidDataException>(() => ExifMetadata.Parse(input));
    }

    [Theory]
    [InlineData("missing length")]
    [InlineData("zero length")]
    [InlineData("out of range")]
    [InlineData("overlap")]
    [InlineData("wrong type")]
    public void RejectsMalformedThumbnailReferences(string kind)
    {
        byte[] input = ThumbnailFixture();
        switch (kind)
        {
            case "missing length": W16(input, 52, 0x0203, true); break;
            case "zero length": W32(input, 60, 0, true); break;
            case "out of range": W32(input, 48, uint.MaxValue, true); break;
            case "overlap": W32(input, 48, 28, true); break;
            case "wrong type": W16(input, 54, 3, true); break;
        }
        Assert.Throws<InvalidDataException>(() => ExifMetadata.Parse(input));
    }

    [Fact]
    public void BigEndianFloatingPointValuesAreNormalizedByComponent()
    {
        byte[] input = new byte[46];
        Header(input, false);
        W16(input, 8, 2, false);
        Entry(input, 10, 0xc100, 11, 1, 0x3f800000, false);
        Entry(input, 22, 0xc101, 12, 1, 38, false);
        Convert.FromHexString("3FF0000000000000").CopyTo(input, 38);
        ExifMetadata metadata = ExifMetadata.Parse(input);
        Assert.Equal(Convert.FromHexString("0000803F"), metadata.GetEntry(ExifDirectory.Image, 0xc100)!.Value.ToArray());
        Assert.Equal(Convert.FromHexString("000000000000F03F"), metadata.GetEntry(ExifDirectory.Image, 0xc101)!.Value.ToArray());
    }

    [Fact]
    public void EveryTruncatedPrefixOfTheIndependentFixtureIsRejectedSafely()
    {
        byte[] fixture = DirectoryFixture(true);
        for (int length = 0; length < fixture.Length; length++)
            Assert.Throws<InvalidDataException>(() => ExifMetadata.Parse(fixture.AsSpan(0, length)));
    }

    [Fact]
    public void AliasedOffsetsCannotAmplifyRetainedMemoryWithoutBound()
    {
        byte[] input = new byte[202];
        Header(input, true);
        W16(input, 8, 7, true);
        for (ushort i = 0; i < 7; i++)
            Entry(input, 10 + i * 12, (ushort)(0xc100 + i), 7, 102, 100, true);
        Assert.Throws<InvalidDataException>(() => ExifMetadata.Parse(input));
    }

    [Fact]
    public void UnsupportedFieldTypesFailExplicitly()
    {
        byte[] input = DirectoryFixture(true);
        W16(input, 12, 13, true);
        Assert.Throws<NotSupportedException>(() => ExifMetadata.Parse(input));
    }

    private static byte[] DirectoryFixture(bool littleEndian)
    {
        byte[] bytes = new byte[206];
        Header(bytes, littleEndian);
        W16(bytes, 8, 4, littleEndian);
        Entry(bytes, 10, 0x0100, 3, 1, 0, littleEndian);
        W16(bytes, 18, 513, littleEndian);
        Entry(bytes, 22, 0x010e, 2, 7, 160, littleEndian);
        Entry(bytes, 34, 0x8769, 4, 1, 64, littleEndian);
        Entry(bytes, 46, 0x8825, 4, 1, 106, littleEndian);
        W16(bytes, 64, 3, littleEndian);
        Entry(bytes, 66, 0x829a, 5, 1, 168, littleEndian);
        Entry(bytes, 78, 0xa005, 4, 1, 136, littleEndian);
        Entry(bytes, 90, 0xc001, 8, 3, 176, littleEndian);
        W16(bytes, 106, 2, littleEndian);
        Entry(bytes, 108, 1, 2, 2, 0, littleEndian);
        bytes[116] = (byte)'N';
        Entry(bytes, 120, 2, 5, 3, 182, littleEndian);
        W16(bytes, 136, 1, littleEndian);
        Entry(bytes, 138, 1, 2, 4, 0, littleEndian);
        "R98\0"u8.CopyTo(bytes.AsSpan(146));
        "camera\0"u8.CopyTo(bytes.AsSpan(160));
        W32(bytes, 168, 1, littleEndian);
        W32(bytes, 172, 125, littleEndian);
        W16(bytes, 176, 0xfffe, littleEndian);
        W16(bytes, 178, 300, littleEndian);
        W16(bytes, 180, 0x8000, littleEndian);
        uint[] gps = [51, 1, 30, 1, 0, 1];
        for (int i = 0; i < gps.Length; i++)
            W32(bytes, 182 + i * 4, gps[i], littleEndian);
        return bytes;
    }

    private static byte[] ThumbnailFixture()
    {
        byte[] bytes = new byte[72];
        Header(bytes, true);
        W16(bytes, 8, 1, true);
        Entry(bytes, 10, 0x0112, 3, 1, 1, true);
        W32(bytes, 22, 26, true);
        W16(bytes, 26, 3, true);
        Entry(bytes, 28, 0x0103, 3, 1, 6, true);
        Entry(bytes, 40, 0x0201, 4, 1, 68, true);
        Entry(bytes, 52, 0x0202, 4, 1, 4, true);
        Convert.FromHexString("FFD8FFD9").CopyTo(bytes, 68);
        return bytes;
    }

    private static void AssertEntry(ExifFieldType type, uint count, byte[] value,
        (ExifFieldType Type, uint Count, byte[] Value) actual)
    {
        Assert.Equal(type, actual.Type);
        Assert.Equal(count, actual.Count);
        Assert.Equal(value, actual.Value);
    }

    private static (ExifFieldType Type, uint Count, byte[] Value) ReadEntry(byte[] bytes, int directory, ushort tag)
    {
        int count = U16(bytes, directory);
        for (int i = 0; i < count; i++)
        {
            int offset = directory + 2 + i * 12;
            if (U16(bytes, offset) != tag)
                continue;
            ushort type = U16(bytes, offset + 2);
            int size = type switch { 1 or 2 or 6 or 7 => 1, 3 or 8 => 2, 4 or 9 or 11 => 4, 5 or 10 or 12 => 8, _ => throw new Exception("Invalid field type.") };
            uint valueCount = U32(bytes, offset + 4);
            int length = checked((int)valueCount * size);
            int valueOffset = length <= 4 ? offset + 8 : (int)U32(bytes, offset + 8);
            return ((ExifFieldType)type, valueCount, bytes.AsSpan(valueOffset, length).ToArray());
        }
        throw new Exception($"Missing tag {tag:X4}.");
    }

    private static IEnumerable<ushort> Tags(byte[] bytes, int directory)
    {
        for (int i = 0; i < U16(bytes, directory); i++)
            yield return U16(bytes, directory + 2 + i * 12);
    }

    private static void Header(byte[] bytes, bool littleEndian)
    {
        bytes[0] = bytes[1] = (byte)(littleEndian ? 'I' : 'M');
        W16(bytes, 2, 42, littleEndian);
        W32(bytes, 4, 8, littleEndian);
    }

    private static void Entry(byte[] bytes, int offset, ushort tag, ushort type, uint count, uint value, bool littleEndian)
    {
        W16(bytes, offset, tag, littleEndian);
        W16(bytes, offset + 2, type, littleEndian);
        W32(bytes, offset + 4, count, littleEndian);
        W32(bytes, offset + 8, value, littleEndian);
    }

    private static void W16(byte[] bytes, int offset, ushort value, bool littleEndian)
    {
        if (littleEndian) BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), value);
        else BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), value);
    }

    private static void W32(byte[] bytes, int offset, uint value, bool littleEndian)
    {
        if (littleEndian) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        else BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    }

    private static ushort U16(byte[] bytes, int offset) => bytes[0] == 'I'
        ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset))
        : BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset));
    private static uint U32(byte[] bytes, int offset) => bytes[0] == 'I'
        ? BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset))
        : BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset));
}
