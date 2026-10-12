using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderExtendedXmpTests
{
    // The APP1 length includes two length bytes, followed by the 35-byte
    // signature, 32 ASCII digest bytes, full length and offset (four bytes each).
    private const int ExtendedChunkBytes = 65_458;
    private const int ExtendedHeaderBytes = 75;
    private const int MaximumStandardBytes = 65_504;
    private const int IccChunkBytes = 65_519;
    private static readonly byte[] StandardSignature = "http://ns.adobe.com/xap/1.0/\0"u8.ToArray();
    private static readonly byte[] ExtendedSignature = "http://ns.adobe.com/xmp/extension/\0"u8.ToArray();
    private static readonly XName LinkName = XName.Get("HasExtendedXMP", "http://ns.adobe.com/xmp/note/");

    [Fact]
    public void DefaultAndEmptyExtensionPreserveExistingBytesAndStandardXmp()
    {
        byte[] reference = Encode(new());
        Assert.Equal("A3C09B17325C4F1EDC2FC43141855B604E5AB4ADAD24D3B9E78AE400D3CD3A28",
            Convert.ToHexString(SHA256.HashData(reference)));
        Assert.Equal(reference, Encode(new() { ExtendedXmpPacket = null }));
        Assert.Equal(reference, Encode(new() { ExtendedXmpPacket = "", XmpPacket = "" }));
        Assert.DoesNotContain(ReadHeaders(reference), segment => segment.Code == 0xe1);

        // Existing standard packets remain opaque when no extension is supplied.
        var options = new JpegliEncoderOptions { XmpPacket = "opaque XMP\0 café 😀 & unescaped" };
        byte[] standard = Encode(options);
        Assert.Equal(standard, Encode(options with { ExtendedXmpPacket = "" }));
        Assert.Equal(standard, Encode(options with { ExtendedXmpPacket = null }));
        Segment app1 = Assert.Single(ReadHeaders(standard), segment => segment.Code == 0xe1);
        Assert.Equal(StandardSignature.Concat(Encoding.UTF8.GetBytes(options.XmpPacket)).ToArray(),
            app1.Payload.ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(ExtendedChunkBytes)]
    [InlineData(ExtendedChunkBytes + 1)]
    [InlineData(65_504)]
    [InlineData(2 * ExtendedChunkBytes)]
    [InlineData(2 * ExtendedChunkBytes + 1)]
    public void ChunksHaveExactLengthsOffsetsDigestAndUnmodifiedSerialization(int length)
    {
        string packet = new('a', length);
        byte[] jpeg = Encode(new() { ExtendedXmpPacket = packet });
        List<Segment> segments = ReadHeaders(jpeg);
        List<Segment> chunks = AssertExtendedPacket(segments, packet);
        Assert.Equal(0xe0, segments[0].Code);
        Assert.True(HasSignature(segments[1], StandardSignature));
        Assert.Equal(chunks, segments.Skip(2).Take(chunks.Count).ToList());
        Assert.Equal(0xdb, segments[chunks.Count + 2].Code);
    }

    [Fact]
    public void ExtensionSupportsMoreThan255ChunksAndFourByteLengthsAndOffsets()
    {
        // Exceed both ICC's 255-chunk cap and 24-bit byte offsets. The extension
        // format uses uint32 lengths/offsets and does not have a chunk-count byte.
        string packet = new('a', 257 * ExtendedChunkBytes + 1);
        List<Segment> segments = ReadHeaders(Encode(new() { ExtendedXmpPacket = packet }));
        List<Segment> chunks = AssertExtendedPacket(segments, packet);
        Assert.Equal(258, chunks.Count);
        Assert.Equal(1, chunks[0].Payload.Span[67]);
        Assert.Equal(1, chunks[^1].Payload.Span[71]);
        Assert.Equal(ExtendedHeaderBytes + 1, chunks[^1].Payload.Length);
    }

    [Fact]
    public void ExtensionPreservesOpaqueUtf8WithoutAddedBomOrTerminator()
    {
        // The encoder must not parse or reserialize this deliberately invalid XML.
        const string packet = "<x:xmpmeta>UTF-8\0 café — 日本語 😀 & unescaped</x:xmpmeta>";
        List<Segment> segments = ReadHeaders(Encode(new() { ExtendedXmpPacket = packet, XmpPacket = "" }));
        List<Segment> chunks = AssertExtendedPacket(segments, packet);
        Segment chunk = Assert.Single(chunks);
        Assert.Equal((byte)'<', chunk.Payload.Span[ExtendedHeaderBytes]);
        Assert.Equal((byte)'>', chunk.Payload.Span[^1]);
    }

    [Fact]
    public void Utf8CharactersCanSpanTwoChunkBoundariesAndReconstructExactly()
    {
        string packet = SplitUnicodePacket();
        List<Segment> chunks = AssertExtendedPacket(ReadHeaders(Encode(new() { ExtendedXmpPacket = packet })), packet);
        Assert.Equal(3, chunks.Count);
        // The first cut splits a four-byte scalar after its leading byte;
        // the second cut splits a two-byte scalar after its leading byte.
        Assert.Equal(0xf0, chunks[0].Payload.Span[^1]);
        Assert.Equal(0x9f, chunks[1].Payload.Span[ExtendedHeaderBytes]);
        Assert.Equal(0xc3, chunks[1].Payload.Span[^1]);
        Assert.Equal(0xa9, chunks[2].Payload.Span[ExtendedHeaderBytes]);
        Assert.True(Encoding.UTF8.GetByteCount(packet) > packet.Length);
    }

    [Theory]
    [InlineData("attribute")]
    [InlineData("aliased element and RDF root")]
    [InlineData("packet wrapper")]
    [InlineData("multiple descriptions")]
    [InlineData("standard byte limit")]
    public void SuppliedLinkedStandardPacketIsPreservedByteForByte(string kind)
    {
        const string extended = "opaque extension café 😀 & unescaped";
        string digest = Digest(extended);
        string packet = kind switch
        {
            "aliased element and RDF root" =>
                $"<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><r:Description r:about='' xmlns:alias='http://ns.adobe.com/xmp/note/'><alias:HasExtendedXMP>{digest}</alias:HasExtendedXMP></r:Description></r:RDF>",
            "packet wrapper" => "<?xpacket begin='' id='custom'?>\n" + LinkedStandard(digest) + "\n<?xpacket end='w'?>",
            "multiple descriptions" =>
                $"<x:xmpmeta xmlns:x='adobe:ns:meta/'><r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><r:Description r:about='first'/><r:Description r:about='second' xmlns:n='http://ns.adobe.com/xmp/note/' n:HasExtendedXMP='{digest}'><other xmlns='urn:custom'>café 😀</other></r:Description></r:RDF></x:xmpmeta>",
            _ => LinkedStandard(digest)
        };
        if (kind == "standard byte limit")
            packet += new string(' ', MaximumStandardBytes - Encoding.UTF8.GetByteCount(packet));

        List<Segment> segments = ReadHeaders(Encode(new() { XmpPacket = packet, ExtendedXmpPacket = extended }));
        AssertExtendedPacket(segments, extended);
        Segment standard = Assert.Single(segments, segment => HasSignature(segment, StandardSignature));
        Assert.Equal(Encoding.UTF8.GetBytes(packet), standard.Payload[StandardSignature.Length..].ToArray());
        if (kind == "standard byte limit")
            Assert.Equal(65_535, standard.Length);
    }

    [Theory]
    [InlineData("absent")]
    [InlineData("mismatched")]
    [InlineData("lowercase")]
    [InlineData("namespace decoy")]
    [InlineData("duplicate descriptions")]
    [InlineData("attribute and element")]
    [InlineData("structured property")]
    [InlineData("resource property")]
    [InlineData("padded property")]
    [InlineData("padded attribute")]
    [InlineData("nested description")]
    [InlineData("nested property")]
    [InlineData("DTD")]
    [InlineData("malformed XML")]
    [InlineData("wrong root")]
    public void MissingInvalidOrAmbiguousStandardLinkageIsRejectedBeforeOutput(string kind)
    {
        const string extended = "opaque extension café 😀 & unescaped";
        string digest = Digest(extended);
        const string rdfStart = "<x:xmpmeta xmlns:x='adobe:ns:meta/'><r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:n='http://ns.adobe.com/xmp/note/'>";
        const string rdfEnd = "</r:RDF></x:xmpmeta>";
        string packet = kind switch
        {
            "absent" => rdfStart + "<r:Description/>" + rdfEnd,
            "mismatched" => LinkedStandard(new string('0', 32)),
            "lowercase" => LinkedStandard(digest.ToLowerInvariant()),
            "namespace decoy" => LinkedStandard(digest).Replace("http://ns.adobe.com/xmp/note/", "urn:decoy", StringComparison.Ordinal),
            "duplicate descriptions" => rdfStart + $"<r:Description n:HasExtendedXMP='{digest}'/><r:Description n:HasExtendedXMP='{digest}'/>" + rdfEnd,
            "attribute and element" => rdfStart + $"<r:Description n:HasExtendedXMP='{digest}'><n:HasExtendedXMP>{digest}</n:HasExtendedXMP></r:Description>" + rdfEnd,
            "structured property" => rdfStart + $"<r:Description><n:HasExtendedXMP><r:value>{digest}</r:value></n:HasExtendedXMP></r:Description>" + rdfEnd,
            "resource property" => rdfStart + $"<r:Description><n:HasExtendedXMP r:resource='{digest}'/></r:Description>" + rdfEnd,
            "padded property" => rdfStart + $"<r:Description><n:HasExtendedXMP> {digest} </n:HasExtendedXMP></r:Description>" + rdfEnd,
            "padded attribute" => LinkedStandard(" " + digest + " "),
            "nested description" => rdfStart + $"<r:Description><r:Description n:HasExtendedXMP='{digest}'/></r:Description>" + rdfEnd,
            "nested property" => rdfStart + $"<r:Description><container><n:HasExtendedXMP>{digest}</n:HasExtendedXMP></container></r:Description>" + rdfEnd,
            "DTD" => "<!DOCTYPE x:xmpmeta [<!ENTITY identifier '" + digest + "'>]>" + LinkedStandard("&identifier;"),
            "malformed XML" => LinkedStandard(digest)[..^1],
            _ => $"<wrong xmlns:n='http://ns.adobe.com/xmp/note/' n:HasExtendedXMP='{digest}'/>"
        };
        AssertRejectedStandard(packet, extended);
    }

    [Fact]
    public void LinkedStandardPacketAboveByteLimitIsRejectedBeforeOutput()
    {
        const string extended = "extension 😀";
        string packet = LinkedStandard(Digest(extended));
        packet += new string(' ', MaximumStandardBytes + 1 - Encoding.UTF8.GetByteCount(packet));
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(destination, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { XmpPacket = packet, ExtendedXmpPacket = extended },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void MalformedUtf16ExtensionIsRejectedBeforeAnyOutput(int kind)
    {
        string packet = kind switch
        {
            0 => "\ud800",
            1 => "\udc00",
            2 => "before\ud800after",
            3 => "before\udc00after",
            4 => "\ud800\ud800",
            // Place malformed text beyond a complete first chunk.
            _ => new string('a', ExtendedChunkBytes + 1) + "😀 then \ud800"
        };
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { ExifData = OpaqueBytes(19), ExtendedXmpPacket = packet },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.ExtendedXmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Fact]
    public void AllMetadataPrecedesCodingTablesInSpecifiedOrder()
    {
        JpegliEncoderOptions options = AllMetadata();
        List<Segment> segments = ReadHeaders(Encode(options));
        Assert.Equal(new[] { 0xe0, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe2, 0xe2, 0xfe, 0xdb },
            segments.Take(10).Select(segment => segment.Code).ToArray());
        Assert.Equal("Exif\0\0"u8.ToArray().Concat(options.ExifData.ToArray()).ToArray(), segments[1].Payload.ToArray());
        Assert.True(HasSignature(segments[2], StandardSignature));
        List<Segment> chunks = AssertExtendedPacket(segments, options.ExtendedXmpPacket!);
        Assert.Equal(chunks, segments.Skip(3).Take(3).ToList());
        Assert.True(segments[6].Payload.Span[..12].SequenceEqual("ICC_PROFILE\0"u8));
        Assert.Equal(1, segments[6].Payload.Span[12]);
        Assert.Equal(2, segments[6].Payload.Span[13]);
        Assert.True(segments[6].Payload.Span[14..].SequenceEqual(options.IccProfile.Span[..IccChunkBytes]));
        Assert.Equal(2, segments[7].Payload.Span[12]);
        Assert.Equal(2, segments[7].Payload.Span[13]);
        Assert.True(segments[7].Payload.Span[14..].SequenceEqual(options.IccProfile.Span[IccChunkBytes..]));
        Assert.Equal(Encoding.UTF8.GetBytes(options.Comment!), segments[8].Payload.ToArray());
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24, false, 0, false)]
    [InlineData(PixelLayout.Rgb24, false, 0, true)]
    [InlineData(PixelLayout.Rgb24, true, 0, false)]
    [InlineData(PixelLayout.Rgb24, true, 0, true)]
    [InlineData(PixelLayout.Rgb24, true, 2, false)]
    [InlineData(PixelLayout.Rgb24, true, 2, true)]
    [InlineData(PixelLayout.Gray8, false, 0, false)]
    [InlineData(PixelLayout.Gray8, false, 0, true)]
    [InlineData(PixelLayout.Gray8, true, 0, false)]
    [InlineData(PixelLayout.Gray8, true, 0, true)]
    [InlineData(PixelLayout.Gray8, true, 2, false)]
    [InlineData(PixelLayout.Gray8, true, 2, true)]
    public void ExtensionLeavesCodingBytesAndDecodedSamplesUnchanged(
        PixelLayout layout, bool progressive, int refinementBits, bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = layout == PixelLayout.Gray8
            ? Enumerable.Range(0, width * height).Select(index => (byte)(index * 73 + index / width * 19)).ToArray()
            : TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions
        {
            Progressive = progressive, ProgressiveRefinementBits = refinementBits,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] reference = JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);
        JpegliEncoderOptions metadata = AllMetadata();
        byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, layout, options with
        {
            ExifData = metadata.ExifData, ExtendedXmpPacket = metadata.ExtendedXmpPacket,
            IccProfile = metadata.IccProfile, Comment = metadata.Comment,
            DensityUnit = JpegDensityUnit.DotsPerInch, HorizontalDensity = 300, VerticalDensity = 150
        }, cancellationToken: TestContext.Current.CancellationToken);
        // Equality from DQT through EOI covers every scan and refinement,
        // entropy byte, stuffing byte, restart marker and final marker.
        int referenceStart = ReadHeaders(reference).Single(segment => segment.Code == 0xdb).Start;
        int actualStart = ReadHeaders(jpeg).Single(segment => segment.Code == 0xdb).Start;
        Assert.True(reference.AsSpan(referenceStart).SequenceEqual(jpeg.AsSpan(actualStart)));
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult expected = ImageResult.FromMemory(reference, channels);
        ImageResult actual = ImageResult.FromMemory(jpeg, channels);
        Assert.Equal(width, actual.Width);
        Assert.Equal(height, actual.Height);
        Assert.Equal(expected.Data, actual.Data);
    }

    [Fact]
    public void NonseekableStreamProducesIdenticalBytesAndRemainsOpen()
    {
        var options = AllMetadata() with { Progressive = true, ProgressiveRefinementBits = 2 };
        byte[] expected = Encode(options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
        output.WriteByte(42);
        Assert.Equal(expected.Length + 1, destination.Length);
    }

    [Fact]
    public void PreCancellationWritesNothingAndLeavesStreamOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: AllMetadata(), cancellationToken: token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)] // After JFIF.
    [InlineData(1)] // After Exif.
    [InlineData(2)] // After standard XMP, before the first extension chunk.
    [InlineData(3)] // Between the first and second extension chunks.
    [InlineData(4)] // Between the second and third extension chunks.
    [InlineData(5)] // After the final extension chunk.
    [InlineData(6)] // Between ICC chunks.
    [InlineData(7)] // Before COM.
    [InlineData(8)] // Before DQT.
    public void CancellationStopsAtEveryMetadataBoundary(int segmentIndex)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        int boundary = ReadHeaders(complete)[segmentIndex].End;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= boundary)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(5, true)]
    public void IoFailureWithinStandardAndExtendedSegmentsPropagatesAndLeavesStreamOpen(int segmentIndex, bool afterPayload)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        Segment segment = ReadHeaders(complete)[segmentIndex];
        int boundary = afterPayload ? segment.End : segment.Start + 4;
        var expectedError = new IOException("Injected extended-XMP write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, failureBoundary: boundary, failure: expectedError);
        var actualError = Assert.Throws<IOException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Same(expectedError, actualError);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static JpegliEncoderOptions AllMetadata() => new()
    {
        ExifData = OpaqueBytes(37), ExtendedXmpPacket = SplitUnicodePacket(),
        IccProfile = OpaqueBytes(IccChunkBytes + 1), Comment = "after all XMP 😀"
    };

    private static string SplitUnicodePacket() =>
        new string('a', ExtendedChunkBytes - 1) + "😀" + new string('b', ExtendedChunkBytes - 4) + "é";

    private static string Digest(string packet) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(packet)));

    private static string LinkedStandard(string digest) =>
        $"<x:xmpmeta xmlns:x='adobe:ns:meta/'><r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><r:Description r:about='' xmlns:n='http://ns.adobe.com/xmp/note/' n:HasExtendedXMP='{digest}'/></r:RDF></x:xmpmeta>";

    private static void AssertRejectedStandard(string packet, string extended)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { ExifData = OpaqueBytes(19), XmpPacket = packet, ExtendedXmpPacket = extended },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    private static byte[] OpaqueBytes(int length)
    {
        byte[] bytes = new byte[length];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = (byte)(index * 73 + 19);
        if (length >= 8)
            new byte[] { 0xff, 0xd8, 0xff, 0xe1, 0, 0, 0xff, 0xda }.CopyTo(bytes, 0);
        return bytes;
    }

    private static List<Segment> AssertExtendedPacket(List<Segment> segments, string packet)
    {
        byte[] expected = Encoding.UTF8.GetBytes(packet);
        // MD5 is the Adobe format's packet identifier, independently computed here.
        string digest = Convert.ToHexString(MD5.HashData(expected));
        Segment standard = Assert.Single(segments, segment => HasSignature(segment, StandardSignature));
        byte[] standardBytes = standard.Payload[StandardSignature.Length..].ToArray();
        Assert.False(standardBytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
        XDocument document = XDocument.Parse(Encoding.UTF8.GetString(standardBytes));
        string[] links = document.Descendants().Attributes(LinkName).Select(attribute => attribute.Value)
            .Concat(document.Descendants(LinkName).Select(element => element.Value)).ToArray();
        Assert.Equal(digest, Assert.Single(links));

        List<Segment> chunks = segments.Where(segment => HasSignature(segment, ExtendedSignature)).ToList();
        Assert.Equal((expected.Length + ExtendedChunkBytes - 1) / ExtendedChunkBytes, chunks.Count);
        using var reconstructed = new MemoryStream();
        int offset = 0;
        foreach (Segment chunk in chunks)
        {
            int bytes = Math.Min(ExtendedChunkBytes, expected.Length - offset);
            Assert.Equal(bytes + ExtendedHeaderBytes + 2, chunk.Length);
            Assert.InRange(chunk.Length, ExtendedHeaderBytes + 3, 65_535);
            Assert.Equal(digest, Encoding.ASCII.GetString(chunk.Payload.Span.Slice(35, 32)));
            AssertBigEndianField(chunk.Payload.Span.Slice(67, 4), expected.Length);
            AssertBigEndianField(chunk.Payload.Span.Slice(71, 4), offset);
            ReadOnlySpan<byte> data = chunk.Payload.Span[ExtendedHeaderBytes..];
            Assert.True(data.SequenceEqual(expected.AsSpan(offset, bytes)));
            reconstructed.Write(data);
            offset += bytes;
        }
        Assert.Equal(expected.Length, offset);
        Assert.Equal(expected, reconstructed.ToArray());
        return chunks;
    }

    private static void AssertBigEndianField(ReadOnlySpan<byte> field, int value)
    {
        Assert.Equal(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value }, field.ToArray());
        long parsed = (long)field[0] * 16_777_216 + field[1] * 65_536 + field[2] * 256 + field[3];
        Assert.Equal(value, parsed);
    }

    private static bool HasSignature(Segment segment, byte[] signature) =>
        segment.Code == 0xe1 && segment.Payload.Span.StartsWith(signature);

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    // Traverse JPEG segment lengths independently of the production writer.
    private static List<Segment> ReadHeaders(byte[] jpeg)
    {
        Assert.True(jpeg.AsSpan(0, 2).SequenceEqual(new byte[] { 0xff, 0xd8 }));
        var segments = new List<Segment>();
        int position = 2;
        while (true)
        {
            Assert.InRange(position, 2, jpeg.Length - 4);
            int start = position;
            Assert.Equal(0xff, jpeg[position++]);
            int code = jpeg[position++];
            int length = jpeg[position] * 256 + jpeg[position + 1];
            Assert.InRange(length, 2, jpeg.Length - position);
            segments.Add(new(code, start, length, jpeg.AsMemory(position + 2, length - 2)));
            position += length;
            if (code == 0xda)
                return segments;
        }
    }

    private sealed class WriteOnlyStream(
        Stream inner, Action? afterWrite = null, long? failureBoundary = null, IOException? failure = null) : Stream
    {
        public bool Closed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !Closed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            int count = failureBoundary is long boundary ? (int)Math.Min(buffer.Length, boundary - inner.Length) : buffer.Length;
            inner.Write(buffer[..count]);
            if (failureBoundary is long end && inner.Length >= end)
                throw failure!;
            afterWrite?.Invoke();
        }
        public override void WriteByte(byte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            Write(buffer);
        }
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
