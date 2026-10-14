using System.Buffers.Binary;
using System.Text;
using Xunit;

namespace DotJpegli.Tests;

public sealed class MetadataReviewTests
{
    [Fact]
    public void BigEndianUnicodeUserCommentKeepsItsMeaningAfterUnrelatedEdit()
    {
        byte[] comment = [.. "UNICODE\0"u8, 0, 0x41, 0, 0x42];
        ExifMetadata metadata = ExifMetadata.Parse(BigEndianPhotoField(0x9286, 7, comment));
        metadata.SetAscii(ExifDirectory.Image, 0x013b, "Ada");
        byte[] encoded = metadata.ToArray();
        byte[] retained = ReadPhotoField(encoded, 0x9286);
        Encoding unicode = encoded[0] == 'M' ? Encoding.BigEndianUnicode : Encoding.Unicode;
        Assert.Equal("AB", unicode.GetString(retained.AsSpan(8)));
        Assert.Equal(comment, retained);
        Assert.Equal("MM"u8.ToArray(), encoded[..2]);
    }

    [Theory]
    [InlineData(0x8828, "000100014E000000000100000002")] // OECF: one signed rational column.
    [InlineData(0xa20c, "000100014E000000000100000002")] // SpatialFrequencyResponse: one unsigned rational column.
    [InlineData(0xa302, "0002000200010102")] // CFA pattern: two rows and columns.
    [InlineData(0xa40b, "0001000100410000")] // DeviceSettingDescription: dimensions and UTF-16 string.
    public void BigEndianUndefinedTablesRetainTheirEmbeddedNumericalByteOrder(int tag, string hex)
    {
        byte[] payload = Convert.FromHexString(hex);
        ExifMetadata metadata = ExifMetadata.Parse(BigEndianPhotoField((ushort)tag, 7, payload));
        metadata.SetUnsignedShort(ExifDirectory.Image, 0x0112, 3);
        byte[] encoded = metadata.ToArray();
        Assert.Equal("MM"u8.ToArray(), encoded[..2]);
        Assert.Equal(payload, ReadPhotoField(encoded, (ushort)tag));
    }

    [Fact]
    public void BigEndianTypedValuesRemainCanonicalInTheApiAndWriteBackInTheSourceByteOrder()
    {
        byte[] rational = Convert.FromHexString("0000000300000008");
        ExifMetadata metadata = ExifMetadata.Parse(BigEndianPhotoField(0x829a, 5, rational));
        Assert.Equal(Convert.FromHexString("0300000008000000"),
            metadata.GetEntry(ExifDirectory.Photo, 0x829a)!.Value.ToArray());
        metadata.SetUnsignedRational(ExifDirectory.Photo, 0x829a, 4, 5);
        metadata.SetUnsignedShort(ExifDirectory.Image, 0x0112, 6);
        byte[] encoded = metadata.ToArray();
        Assert.Equal("MM"u8.ToArray(), encoded[..2]);
        Assert.Equal(Convert.FromHexString("0000000400000005"), ReadPhotoField(encoded, 0x829a));
        ExifMetadata again = ExifMetadata.Parse(encoded);
        Assert.Equal(6u, again.GetUnsignedInteger(ExifDirectory.Image, 0x0112));
        Assert.Equal(Convert.FromHexString("0400000005000000"),
            again.GetEntry(ExifDirectory.Photo, 0x829a)!.Value.ToArray());
    }

    private static byte[] BigEndianPhotoField(ushort tag, ushort type, byte[] payload)
    {
        byte[] tiff = new byte[44 + payload.Length];
        tiff[0] = tiff[1] = (byte)'M';
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(2), 42);
        BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(10), 0x8769);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(12), 4);
        BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(14), 1);
        BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(18), 26);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(26), 1);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(28), tag);
        BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(30), type);
        BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(32), type == 5 ? 1u : (uint)payload.Length);
        BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(36), 44);
        payload.CopyTo(tiff, 44);
        return tiff;
    }

    private static byte[] ReadPhotoField(byte[] tiff, ushort tag)
    {
        bool big = tiff[0] == 'M';
        int image = checked((int)Read32(tiff, 4, big));
        byte[] pointer = ReadField(tiff, image, 0x8769, big);
        int photo = checked((int)(big ? BinaryPrimitives.ReadUInt32BigEndian(pointer) : BinaryPrimitives.ReadUInt32LittleEndian(pointer)));
        return ReadField(tiff, photo, tag, big);
    }

    private static byte[] ReadField(byte[] tiff, int directory, ushort tag, bool big)
    {
        int count = Read16(tiff, directory, big);
        for (int index = 0; index < count; index++)
        {
            int entry = directory + 2 + 12 * index;
            if (Read16(tiff, entry, big) != tag)
                continue;
            int elementSize = Read16(tiff, entry + 2, big) switch { 3 => 2, 4 => 4, 5 => 8, _ => 1 };
            int length = checked((int)Read32(tiff, entry + 4, big) * elementSize);
            int offset = length <= 4 ? entry + 8 : checked((int)Read32(tiff, entry + 8, big));
            return tiff.AsSpan(offset, length).ToArray();
        }
        throw new InvalidOperationException($"Tag {tag:X4} is absent.");
    }

    private static ushort Read16(byte[] bytes, int offset, bool big) => big
        ? BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
    private static uint Read32(byte[] bytes, int offset, bool big) => big
        ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
}
