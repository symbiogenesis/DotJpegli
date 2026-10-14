using System.Buffers.Binary;
using System.Text;
using System.Xml.Linq;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class JpegliMetadataTests
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Tiff = "http://ns.adobe.com/tiff/1.0/";
    private static readonly XNamespace Exif = "http://ns.adobe.com/exif/1.0/";
    private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";

    [Theory]
    [InlineData(JpegDensityUnit.DotsPerInch, 2)]
    [InlineData(JpegDensityUnit.DotsPerCentimeter, 3)]
    public void ReconcilesIndependentTiffXmlAndIimFields(JpegDensityUnit unit, int expectedUnit)
    {
        var original = new JpegliEncoderOptions
        {
            DensityUnit = unit, HorizontalDensity = 300, VerticalDensity = 150, Quality = 73
        };
        JpegliEncoderOptions result = Reconcile(original, 13, 7, new()
        {
            Orientation = 6, Artist = "Ada", Copyright = "Copyright 2026", Description = "A portrait",
            ExifColorSpace = 1
        });
        Assert.Equal(73, result.Quality);
        Assert.True(original.ExifData.IsEmpty);
        Assert.Null(original.XmpPacket);
        Assert.True(original.IptcData.IsEmpty);

        byte[] tiff = result.ExifData.ToArray();
        Assert.Equal("II", Encoding.ASCII.GetString(tiff, 0, 2));
        int image = (int)U32(tiff, 4);
        int photo = (int)Unsigned(tiff, image, 0x8769);
        Assert.Equal(13u, Unsigned(tiff, image, 0x0100));
        Assert.Equal(7u, Unsigned(tiff, image, 0x0101));
        Assert.Equal(13u, Unsigned(tiff, photo, 0xa002));
        Assert.Equal(7u, Unsigned(tiff, photo, 0xa003));
        Assert.Equal(6u, Unsigned(tiff, image, 0x0112));
        Assert.Equal(1u, Unsigned(tiff, photo, 0xa001));
        Assert.Equal((uint)expectedUnit, Unsigned(tiff, image, 0x0128));
        Assert.Equal((300u, 1u), Rational(tiff, image, 0x011a));
        Assert.Equal((150u, 1u), Rational(tiff, image, 0x011b));
        Assert.Equal("Ada", Ascii(tiff, image, 0x013b));
        Assert.Equal("Copyright 2026", Ascii(tiff, image, 0x8298));
        Assert.Equal("A portrait", Ascii(tiff, image, 0x010e));

        XElement xml = XElement.Parse(result.XmpPacket!);
        Assert.Equal("13", Property(xml, Tiff + "ImageWidth").Value);
        Assert.Equal("7", Property(xml, Tiff + "ImageLength").Value);
        Assert.Equal("13", Property(xml, Exif + "PixelXDimension").Value);
        Assert.Equal("7", Property(xml, Exif + "PixelYDimension").Value);
        Assert.Equal("6", Property(xml, Tiff + "Orientation").Value);
        Assert.Equal("1", Property(xml, Exif + "ColorSpace").Value);
        Assert.Equal("300/1", Property(xml, Tiff + "XResolution").Value);
        Assert.Equal("150/1", Property(xml, Tiff + "YResolution").Value);
        Assert.Equal(expectedUnit.ToString(), Property(xml, Tiff + "ResolutionUnit").Value);
        XElement creator = Property(xml, Dc + "creator");
        Assert.Equal("Ada", Assert.Single(creator.Element(Rdf + "Seq")!.Elements(Rdf + "li")).Value);
        AssertLocalized(xml, "rights", "Copyright 2026");
        AssertLocalized(xml, "description", "A portrait");

        var iim = ReadIim(result.IptcData.ToArray());
        Assert.Equal(new byte[] { 0x1b, 0x25, 0x47 }, Assert.Single(iim[(1, 90)]));
        Assert.Equal("Ada", Encoding.UTF8.GetString(Assert.Single(iim[(2, 80)])));
        Assert.Equal("Copyright 2026", Encoding.UTF8.GetString(Assert.Single(iim[(2, 116)])));
        Assert.Equal("A portrait", Encoding.UTF8.GetString(Assert.Single(iim[(2, 120)])));
    }

    [Fact]
    public void UpdatesAliasesWithoutLosingOtherLanguagesOrUnrelatedStructures()
    {
        string packet = """
            <x:xmpmeta xmlns:x="adobe:ns:meta/" xmlns:r="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
              xmlns:t="http://ns.adobe.com/tiff/1.0/" xmlns:d="http://purl.org/dc/elements/1.1/" xmlns:c="urn:custom">
             <r:RDF><r:Description r:about="" t:Artist="old" t:Copyright="old" t:ImageDescription="old">
              <d:rights><r:Alt><r:li xml:lang="x-default">old</r:li><r:li xml:lang="fr">Droits</r:li></r:Alt></d:rights>
              <c:opaque r:parseType="Resource"><c:child qualifier="yes">keep</c:child></c:opaque>
             </r:Description></r:RDF>
            </x:xmpmeta>
            """;
        var sourceExif = new ExifMetadata();
        sourceExif.SetAscii(ExifDirectory.Image, 0x010f, "Camera");
        byte[] tiff = sourceExif.ToArray();
        var source = new JpegliEncoderOptions { XmpPacket = packet, ExifData = tiff };
        var result = Reconcile(source, 1, 1,
            new() { Artist = "New", Copyright = "Notice", Description = "Scene" });
        XElement xml = XElement.Parse(result.XmpPacket!);
        Assert.Empty(xml.DescendantsAndSelf().Attributes(Tiff + "Artist"));
        Assert.Empty(xml.Descendants(Tiff + "Copyright"));
        Assert.Empty(xml.DescendantsAndSelf().Attributes(Tiff + "Copyright"));
        Assert.Empty(xml.DescendantsAndSelf().Attributes(Tiff + "ImageDescription"));
        AssertLocalized(xml, "rights", "Notice");
        Assert.Equal("Droits", Property(xml, Dc + "rights").Descendants(Rdf + "li")
            .Single(item => (string?)item.Attribute(XNamespace.Xml + "lang") == "fr").Value);
        Assert.Equal("keep", Property(xml, XName.Get("opaque", "urn:custom")).Element(XName.Get("child", "urn:custom"))!.Value);
        Assert.Equal("Camera", Ascii(result.ExifData.ToArray(), (int)U32(result.ExifData.Span, 4), 0x010f));
        Assert.Equal(packet, source.XmpPacket);
        Assert.Equal(tiff, source.ExifData.ToArray());
    }

    [Fact]
    public void UnspecifiedDensityRemovesStalePhysicalResolutionAndKeepsJfifAspectRatio()
    {
        var previous = Reconcile(new() { DensityUnit = JpegDensityUnit.DotsPerInch,
            HorizontalDensity = 300, VerticalDensity = 300 }, 2, 2);
        var result = Reconcile(previous with { DensityUnit = JpegDensityUnit.Unspecified,
            HorizontalDensity = 2, VerticalDensity = 1 }, 2, 2);
        byte[] tiff = result.ExifData.ToArray();
        int image = (int)U32(tiff, 4);
        Assert.Null(Entry(tiff, image, 0x011a));
        Assert.Null(Entry(tiff, image, 0x011b));
        Assert.Null(Entry(tiff, image, 0x0128));
        XElement xml = XElement.Parse(result.XmpPacket!);
        Assert.Empty(xml.Descendants(Tiff + "XResolution"));
        Assert.Empty(xml.Descendants(Tiff + "YResolution"));
        Assert.Empty(xml.Descendants(Tiff + "ResolutionUnit"));
        Assert.Equal(2, result.HorizontalDensity);
        Assert.Equal(1, result.VerticalDensity);
    }

    [Fact]
    public void NullDescriptiveEditsPreserveValuesAndEmptyEditsRemoveAllRepresentations()
    {
        var previous = Reconcile(new(), 1, 1,
            new() { Artist = "Ada", Copyright = "Notice", Description = "Scene", ExifColorSpace = 65535 });
        var kept = Reconcile(previous, 2, 2);
        Assert.Equal(previous.IptcData.ToArray(), kept.IptcData.ToArray());
        Assert.Equal(65535u, Unsigned(kept.ExifData.ToArray(),
            (int)Unsigned(kept.ExifData.ToArray(), (int)U32(kept.ExifData.Span, 4), 0x8769), 0xa001));
        var cleared = Reconcile(kept, 2, 2,
            new() { Artist = "", Copyright = "", Description = "" });
        byte[] tiff = cleared.ExifData.ToArray();
        int image = (int)U32(tiff, 4);
        Assert.Null(Entry(tiff, image, 0x013b));
        Assert.Null(Entry(tiff, image, 0x8298));
        Assert.Null(Entry(tiff, image, 0x010e));
        XElement xml = XElement.Parse(cleared.XmpPacket!);
        Assert.Empty(xml.Descendants(Dc + "creator"));
        Assert.Empty(xml.Descendants(Dc + "rights"));
        Assert.Empty(xml.Descendants(Dc + "description"));
        var iim = ReadIim(cleared.IptcData.ToArray());
        Assert.False(iim.ContainsKey((2, 80)));
        Assert.False(iim.ContainsKey((2, 116)));
        Assert.False(iim.ContainsKey((2, 120)));
    }

    [Fact]
    public void DropsTheCompleteStaleThumbnailDirectory()
    {
        var exif = new ExifMetadata();
        exif.SetJpegThumbnail(new byte[] { 0xff, 0xd8, 0xff, 0xd9 });
        exif.SetUnsignedLong(ExifDirectory.Thumbnail, 0x0100, 99);
        byte[] original = exif.ToArray();
        var result = Reconcile(new() { ExifData = original }, 3, 2);
        byte[] bytes = result.ExifData.ToArray();
        int image = (int)U32(bytes, 4);
        Assert.Equal(0u, U32(bytes, image + 2 + 12 * U16(bytes, image)));
        Assert.NotNull(ExifMetadata.Parse(original).GetJpegThumbnail());
        Assert.Null(ExifMetadata.Parse(bytes).GetJpegThumbnail());
    }

    [Fact]
    public void RejectsLegacyTextRelabelingAndUnsafeExifWithoutChangingInputs()
    {
        byte[] legacyIim = [0x1c, 2, 5, 0, 1, 0xe9];
        byte[] iimCopy = legacyIim.ToArray();
        Assert.Throws<InvalidOperationException>(() => Reconcile(new() { IptcData = legacyIim },
            1, 1, new() { Artist = "Ada" }));
        Assert.Equal(iimCopy, legacyIim);

        // IFD0 with one inline UNDEFINED MakerNote: valid framing, unsafe to relocate.
        byte[] unsafeExif = new byte[26];
        unsafeExif[0] = unsafeExif[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(unsafeExif.AsSpan(2), 42);
        BinaryPrimitives.WriteUInt32LittleEndian(unsafeExif.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt16LittleEndian(unsafeExif.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(unsafeExif.AsSpan(10), 0x927c);
        BinaryPrimitives.WriteUInt16LittleEndian(unsafeExif.AsSpan(12), 7);
        BinaryPrimitives.WriteUInt32LittleEndian(unsafeExif.AsSpan(14), 1);
        unsafeExif[18] = 0x55;
        byte[] exifCopy = unsafeExif.ToArray();
        Assert.Throws<NotSupportedException>(() => Reconcile(new() { ExifData = unsafeExif }, 1, 1));
        Assert.Equal(exifCopy, unsafeExif);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EncodingReconciledMetadataPreservesCodingAndDecodedPixelsAcrossPrecisions(bool progressive)
    {
        byte[] pixels = [255, 0, 0, 0, 255, 0, 0, 0, 255, 111, 97, 35];
        var options = new JpegliEncoderOptions { Progressive = progressive };
        var reconciled = Reconcile(options, 2, 2, new() { Artist = "Ada" });
        byte[] reference = JpegliEncoder.Encode(pixels, 2, 2, options: options, cancellationToken: TestContext.Current.CancellationToken);
        byte[] actual = JpegliEncoder.Encode(pixels, 2, 2, options: reconciled, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(CodingBytes(reference), CodingBytes(actual));
        Assert.Equal(ImageResult.FromMemory(reference, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(actual, ColorComponents.RedGreenBlue).Data);
        ushort[] words = pixels.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = pixels.Select(value => value / 255f).ToArray();
        Assert.Equal(CodingBytes(JpegliEncoder.Encode(words, 2, 2, options: options, cancellationToken: TestContext.Current.CancellationToken)),
            CodingBytes(JpegliEncoder.Encode(words, 2, 2, options: reconciled, cancellationToken: TestContext.Current.CancellationToken)));
        Assert.Equal(CodingBytes(JpegliEncoder.Encode(floats, 2, 2, options: options, cancellationToken: TestContext.Current.CancellationToken)),
            CodingBytes(JpegliEncoder.Encode(floats, 2, 2, options: reconciled, cancellationToken: TestContext.Current.CancellationToken)));
    }

    [Theory]
    [InlineData(0xe0, "JFIF\0")]
    [InlineData(0xe0, "JFXX\0")]
    [InlineData(0xe1, "Exif\0\0")]
    [InlineData(0xe1, "http://ns.adobe.com/xap/1.0/\0")]
    [InlineData(0xe1, "http://ns.adobe.com/xmp/extension/\0")]
    [InlineData(0xed, "Photoshop 3.0\0")]
    public void RejectsConflictingRecognizedRawMarkers(int code, string signature)
    {
        Assert.Throws<ArgumentException>(() => Reconcile(new()
        {
            MetadataMarkers = [new(code, Encoding.ASCII.GetBytes(signature))]
        }, 1, 1));
    }

    [Fact]
    public void RetainsUnknownRawMarkersAndOpaqueIcc()
    {
        JpegMetadataMarker marker = new(0xe1, "private"u8.ToArray());
        byte[] icc = [1, 2, 3];
        var result = Reconcile(new() { MetadataMarkers = [marker], IccProfile = icc }, 1, 1);
        Assert.Same(marker, Assert.Single(result.MetadataMarkers!));
        Assert.Equal(icc, result.IccProfile.ToArray());
    }

    [Fact]
    public void RejectsManualExtensionAndStaleDigestLinks()
    {
        Assert.Throws<ArgumentException>(() => Reconcile(new()
        {
            ExtendedXmpPacket = "<extension/>"
        }, 1, 1));
        const string packet = """
            <r:RDF xmlns:r="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:n="http://ns.adobe.com/xmp/note/">
            <r:Description r:about="" n:HasExtendedXMP="ABC"/></r:RDF>
            """;
        Assert.Throws<InvalidOperationException>(() => Reconcile(new() { XmpPacket = packet }, 1, 1));
    }

    [Fact]
    public void PartitionsLargePreservedXmpAndValidatesSelectedStandardProperties()
    {
        var xml = new XmpMetadata();
        XmpPropertyName large = new("urn:custom", "large");
        xml.SetSimpleProperty(large, new string('x', 70_000));
        var result = Reconcile(new() { XmpPacket = xml.ToPacket() }, 1, 1);
        Assert.True(result.AutoPartitionXmp);
        Assert.Contains(new string('x', 70_000), result.XmpPacket);
        byte[] jpeg = JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: result, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(jpeg.Length > 70_000);
        Assert.Throws<ArgumentOutOfRangeException>(() => Reconcile(result with
        {
            StandardXmpProperties = [large]
        }, 1, 1));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(65536, 1)]
    [InlineData(1, 65536)]
    public void RejectsInvalidDimensions(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Reconcile(new(), width, height));

    [Fact]
    public void ValidatesEditsWithoutChangingSuppliedStorage()
    {
        var exif = new ExifMetadata();
        exif.SetAscii(ExifDirectory.Image, 0x010f, "Camera");
        byte[] bytes = exif.ToArray();
        byte[] copy = bytes.ToArray();
        var options = new JpegliEncoderOptions { ExifData = bytes };
        Assert.Throws<ArgumentOutOfRangeException>(() => Reconcile(options, 1, 1, new() { Orientation = 9 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Reconcile(options, 1, 1, new() { ExifColorSpace = 2 }));
        Assert.Throws<ArgumentException>(() => Reconcile(options, 1, 1, new() { Artist = "René" }));
        Assert.Throws<ArgumentException>(() => Reconcile(options, 1, 1, new() { Artist = "A\0B" }));
        Assert.Throws<ArgumentException>(() => Reconcile(options, 1, 1, new() { Artist = new string('A', 33) }));
        Assert.Throws<ArgumentException>(() => Reconcile(options, 1, 1, new() { Copyright = new string('A', 129) }));
        Assert.Throws<ArgumentException>(() => Reconcile(options, 1, 1, new() { Description = new string('A', 2001) }));
        Assert.Equal(copy, bytes);
    }

    [Fact]
    public void HonorsCancellationBeforeParsing()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var exception = Assert.Throws<OperationCanceledException>(() => JpegliMetadata.Reconcile(new()
        {
            ExifData = new byte[] { 1 }
        }, 1, 1, cancellationToken: source.Token));
        Assert.Equal(source.Token, exception.CancellationToken);
    }

    private static JpegliEncoderOptions Reconcile(JpegliEncoderOptions options, int width, int height,
        JpegliMetadataEdits? edits = null) => JpegliMetadata.Reconcile(options, width, height, edits,
            TestContext.Current.CancellationToken);

    private static XElement Property(XElement root, XName name) => Assert.Single(root.Descendants(name));

    private static void AssertLocalized(XElement root, string property, string expected)
    {
        XElement alt = Property(root, Dc + property).Element(Rdf + "Alt")!;
        Assert.Equal(expected, Assert.Single(alt.Elements(Rdf + "li"), item =>
            (string?)item.Attribute(XNamespace.Xml + "lang") == "x-default").Value);
    }

    private static uint U32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);

    private static int? Entry(byte[] bytes, int directory, ushort tag)
    {
        int count = U16(bytes, directory);
        for (int index = 0; index < count; index++)
        {
            int entry = directory + 2 + index * 12;
            if (U16(bytes, entry) == tag)
                return entry;
        }
        return null;
    }

    private static uint Unsigned(byte[] bytes, int directory, ushort tag)
    {
        int entry = Assert.IsType<int>(Entry(bytes, directory, tag));
        Assert.Equal(1u, U32(bytes, entry + 4));
        ushort type = U16(bytes, entry + 2);
        Assert.True(type is 3 or 4);
        return type == 3 ? U16(bytes, entry + 8) : U32(bytes, entry + 8);
    }

    private static (uint, uint) Rational(byte[] bytes, int directory, ushort tag)
    {
        int entry = Assert.IsType<int>(Entry(bytes, directory, tag));
        Assert.Equal((ushort)5, U16(bytes, entry + 2));
        Assert.Equal(1u, U32(bytes, entry + 4));
        int data = (int)U32(bytes, entry + 8);
        return (U32(bytes, data), U32(bytes, data + 4));
    }

    private static string Ascii(byte[] bytes, int directory, ushort tag)
    {
        int entry = Assert.IsType<int>(Entry(bytes, directory, tag));
        Assert.Equal((ushort)2, U16(bytes, entry + 2));
        int count = (int)U32(bytes, entry + 4);
        int data = count <= 4 ? entry + 8 : (int)U32(bytes, entry + 8);
        Assert.Equal(0, bytes[data + count - 1]);
        return Encoding.ASCII.GetString(bytes, data, count - 1);
    }

    private static Dictionary<(byte, byte), List<byte[]>> ReadIim(byte[] bytes)
    {
        var result = new Dictionary<(byte, byte), List<byte[]>>();
        int offset = 0;
        while (offset < bytes.Length)
        {
            Assert.Equal(0x1c, bytes[offset]);
            var key = (bytes[offset + 1], bytes[offset + 2]);
            int length = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 3));
            Assert.True(length < 0x8000);
            if (!result.TryGetValue(key, out List<byte[]>? values))
                result.Add(key, values = []);
            values.Add(bytes.AsSpan(offset + 5, length).ToArray());
            offset += 5 + length;
        }
        Assert.Equal(bytes.Length, offset);
        return result;
    }

    private static byte[] CodingBytes(byte[] jpeg)
    {
        int offset = 2;
        while (jpeg[offset + 1] != 0xdb)
        {
            Assert.Equal(0xff, jpeg[offset]);
            offset += 2 + BinaryPrimitives.ReadUInt16BigEndian(jpeg.AsSpan(offset + 2));
        }
        return jpeg[offset..];
    }
}
