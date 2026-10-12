using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderAutomaticXmpTests
{
    private const int MaximumStandardBytes = 65_504;
    private const int ChunkBytes = 65_458;
    private const int ExtensionHeaderBytes = 75;
    private static readonly byte[] StandardSignature = "http://ns.adobe.com/xap/1.0/\0"u8.ToArray();
    private static readonly byte[] ExtensionSignature = "http://ns.adobe.com/xmp/extension/\0"u8.ToArray();
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Note = "http://ns.adobe.com/xmp/note/";
    private static readonly XNamespace Meta = "adobe:ns:meta/";

    [Fact]
    public void DefaultAndShortOpaquePacketsKeepTheirExistingBytes()
    {
        Assert.False(new JpegliEncoderOptions().AutoPartitionXmp);
        Assert.Equal(Encode(new()), Encode(new() { AutoPartitionXmp = true }));
        Assert.Equal(Encode(new()), Encode(new() { AutoPartitionXmp = true, XmpPacket = "", ExtendedXmpPacket = "" }));
        const string opaque = "opaque XMP\0 café 😀 & unescaped";
        byte[] reference = Encode(new() { XmpPacket = opaque });
        byte[] actual = Encode(new() { XmpPacket = opaque, AutoPartitionXmp = true });
        Assert.Equal(reference, actual);
        Segment standard = Assert.Single(ReadHeaders(actual), segment => HasSignature(segment, StandardSignature));
        Assert.Equal(Encoding.UTF8.GetBytes(opaque), standard.Payload[StandardSignature.Length..].ToArray());
        Assert.DoesNotContain(ReadHeaders(actual), segment => HasSignature(segment, ExtensionSignature));
    }

    [Theory]
    [InlineData(MaximumStandardBytes - 1)]
    [InlineData(MaximumStandardBytes)]
    [InlineData(MaximumStandardBytes + 1)]
    public void PartitionThresholdUsesUtf8BytesAndPreservesPacketsThatFit(int byteLength)
    {
        string packet = SizedPacket(byteLength);
        Assert.Equal(byteLength, Encoding.UTF8.GetByteCount(packet));
        Assert.True(packet.Length < byteLength);
        byte[] actual = Encode(new() { XmpPacket = packet, AutoPartitionXmp = true });
        if (byteLength <= MaximumStandardBytes)
        {
            Assert.Equal(Encode(new() { XmpPacket = packet }), actual);
            Segment standard = Assert.Single(ReadHeaders(actual), segment => HasSignature(segment, StandardSignature));
            Assert.Equal(Encoding.UTF8.GetBytes(packet), standard.Payload[StandardSignature.Length..].ToArray());
            Assert.DoesNotContain(ReadHeaders(actual), segment => HasSignature(segment, ExtensionSignature));
        }
        else
        {
            AssertRootPreserved(packet, AssertAutomaticPacket(actual));
        }
    }

    [Fact]
    public void DisabledPartitioningStillRejectsOversizedPacketsBeforeOutput()
    {
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(destination, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { XmpPacket = SizedPacket(MaximumStandardBytes + 1) },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(ChunkBytes)]
    [InlineData(ChunkBytes + 1)]
    [InlineData(2 * ChunkBytes)]
    [InlineData(2 * ChunkBytes + 1)]
    public void RootSerializationUsesExactChunkSizesAndDropsOuterPadding(int extensionBytes)
    {
        const string prefix = "<r:RDF xmlns:r=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><r:Description><blob xmlns=\"urn:test\">";
        const string suffix = "</blob></r:Description></r:RDF>";
        string root = prefix + new string('a', extensionBytes - Encoding.UTF8.GetByteCount(prefix + suffix)) + suffix;
        string packet = "<?xpacket begin=''?>" + root + new string(' ', 1_000) + "<?xpacket end='w'?>";
        Assert.True(Encoding.UTF8.GetByteCount(packet) > MaximumStandardBytes);
        Assert.Equal(Encoding.UTF8.GetBytes(root), AssertAutomaticPacket(Encode(new() { XmpPacket = packet, AutoPartitionXmp = true })));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ShortMalformedUtf16StillFailsBeforeOutput(int kind)
    {
        string packet = kind switch { 0 => "\ud800", 1 => "\udc00", _ => "opaque before\ud800after" };
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() =>
            Encode(destination, new() { XmpPacket = packet, AutoPartitionXmp = true }, TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Fact]
    public void MalformedUtf16BeyondSizeDetectionFailsBeforeOutput()
    {
        string packet = SizedPacket(2 * ChunkBytes + 1).Replace("</blob>", "\ud800</blob>", StringComparison.Ordinal);
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() =>
            Encode(destination, new() { XmpPacket = packet, AutoPartitionXmp = true }, TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void XmlRootAndStructuredMetadataSurviveWhileOuterWrappersAreRemoved(bool directRdf)
    {
        const string rdf = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:dc='http://purl.org/dc/elements/1.1/' xmlns:t='urn:test' xml:lang='en'>" +
            "<r:Description r:about='' t:attribute='café &amp; tea'><dc:title><r:Alt><r:li xml:lang='x-default'>日本語 😀</r:li><r:li xml:lang='fr'>été</r:li></r:Alt></dc:title>" +
            "<dc:creator><r:Seq><r:li>Alice</r:li><r:li>Bob</r:li></r:Seq></dc:creator><t:object r:parseType='Resource'><t:child r:resource='urn:child'/></t:object></r:Description>" +
            "<r:Description r:about=''><t:bag><r:Bag><r:li>first</r:li><r:li>second</r:li></r:Bag></t:bag><t:blob>PAYLOAD</t:blob></r:Description></r:RDF>";
        string root = directRdf ? rdf : "<x:xmpmeta xmlns:x='adobe:ns:meta/' xmlns:unused='urn:preserved' xml:lang='en' xml:base='https://example.invalid/metadata/' xml:space='preserve'>\n  " +
            rdf.Replace(" xml:lang='en'", "", StringComparison.Ordinal) + "\n</x:xmpmeta>";
        root = root.Replace("PAYLOAD", new string('é', MaximumStandardBytes), StringComparison.Ordinal);
        string packet = "<?xml version='1.0' encoding='UTF-8'?>\n<?xpacket begin='\uFEFF' id='custom'?>\n" + root + "\n<?xpacket end='w'?>";
        byte[] extension = AssertAutomaticPacket(Encode(new() { XmpPacket = packet, AutoPartitionXmp = true }));
        string serialization = Encoding.UTF8.GetString(extension);
        Assert.StartsWith("<", serialization, StringComparison.Ordinal);
        Assert.DoesNotContain("<?xml", serialization, StringComparison.Ordinal);
        Assert.DoesNotContain("<?xpacket", serialization, StringComparison.Ordinal);
        AssertRootPreserved(packet, extension);
        XElement actualRoot = XElement.Parse(serialization, LoadOptions.PreserveWhitespace);
        XElement actualRdf = actualRoot.Name == Rdf + "RDF" ? actualRoot : Assert.Single(actualRoot.Elements(Rdf + "RDF"));
        Assert.Equal(2, actualRdf.Elements(Rdf + "Description").Count());
        Assert.Equal("en", actualRdf.AncestorsAndSelf().Select(element => (string?)element.Attribute(XNamespace.Xml + "lang")).First(value => value is not null));
        Assert.Equal("urn:test", actualRdf.GetNamespaceOfPrefix("t")?.NamespaceName);
        Assert.Equal("café & tea", (string?)actualRdf.Elements(Rdf + "Description").First().Attribute(XName.Get("attribute", "urn:test")));
    }

    [Fact]
    public void Utf8ScalarsAcrossExtensionBoundariesReconstructAndHashExactly()
    {
        const string prefix = "<r:RDF xmlns:r=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><r:Description r:about=\"\"><blob xmlns=\"urn:test\">";
        const string suffix = "</blob></r:Description></r:RDF>";
        int prefixBytes = Encoding.UTF8.GetByteCount(prefix);
        string root = prefix + new string('a', ChunkBytes - prefixBytes - 1) + "😀" + new string('b', ChunkBytes - 4) + "é" + suffix;
        string packet = "<?xpacket begin=''?>" + root + "<?xpacket end='w'?>";
        byte[] jpeg = Encode(new() { XmpPacket = packet, AutoPartitionXmp = true });
        byte[] extension = AssertAutomaticPacket(jpeg);
        AssertRootPreserved(packet, extension);
        List<Segment> chunks = ReadHeaders(jpeg).Where(segment => HasSignature(segment, ExtensionSignature)).ToList();
        Assert.Equal(3, chunks.Count);
        Assert.Equal(0xf0, chunks[0].Payload.Span[^1]);
        Assert.Equal(0x9f, chunks[1].Payload.Span[ExtensionHeaderBytes]);
        Assert.Equal(0xc3, chunks[1].Payload.Span[^1]);
        Assert.Equal(0xa9, chunks[2].Payload.Span[ExtensionHeaderBytes]);
    }

    [Theory]
    [InlineData("malformed XML")]
    [InlineData("DTD")]
    [InlineData("external DTD")]
    [InlineData("wrong root")]
    [InlineData("multiple RDF roots")]
    [InlineData("missing RDF")]
    [InlineData("missing description")]
    [InlineData("RDF text")]
    [InlineData("wrapper text")]
    [InlineData("extra wrapper child")]
    [InlineData("extra RDF child")]
    [InlineData("named subject")]
    [InlineData("RDF ID")]
    [InlineData("RDF nodeID")]
    [InlineData("nested xpacket")]
    [InlineData("stale attribute")]
    [InlineData("stale element")]
    [InlineData("nested stale element")]
    [InlineData("malformed UTF16")]
    public void InvalidAutomaticPacketsAreRejectedBeforeAnyOutput(string kind)
    {
        const string valid = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:n='http://ns.adobe.com/xmp/note/'><r:Description r:about=''><blob xmlns='urn:test'>PAYLOAD</blob></r:Description></r:RDF>";
        string packet = kind switch
        {
            "malformed XML" => valid[..^1],
            "DTD" => "<!DOCTYPE r:RDF [<!ENTITY value 'unsafe'>]>" + valid,
            "external DTD" => "<!DOCTYPE r:RDF SYSTEM 'file:///this-resource-must-never-be-read.dtd'>" + valid,
            "wrong root" => "<wrong>PAYLOAD</wrong>",
            "multiple RDF roots" => "<x:xmpmeta xmlns:x='adobe:ns:meta/'>" + valid + valid + "</x:xmpmeta>",
            "missing RDF" => "<x:xmpmeta xmlns:x='adobe:ns:meta/'>PAYLOAD</x:xmpmeta>",
            "missing description" => "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>PAYLOAD</r:RDF>",
            "RDF text" => valid.Replace("</r:RDF>", "unexpected text</r:RDF>", StringComparison.Ordinal),
            "wrapper text" => "<x:xmpmeta xmlns:x='adobe:ns:meta/'>unexpected text" + valid + "</x:xmpmeta>",
            "extra wrapper child" => "<x:xmpmeta xmlns:x='adobe:ns:meta/'>" + valid + "<other/></x:xmpmeta>",
            "extra RDF child" => valid.Replace("</r:RDF>", "<other/></r:RDF>", StringComparison.Ordinal),
            "named subject" => valid.Replace("r:about=''", "r:about='urn:subject'", StringComparison.Ordinal),
            "RDF ID" => valid.Replace("r:about=''", "r:ID='subject'", StringComparison.Ordinal),
            "RDF nodeID" => valid.Replace("r:about=''", "r:nodeID='subject'", StringComparison.Ordinal),
            "nested xpacket" => valid.Replace("PAYLOAD", "<?xpacket end='w'?>PAYLOAD", StringComparison.Ordinal),
            "stale attribute" => valid.Replace("r:about=''", "r:about='' n:HasExtendedXMP='OLD'", StringComparison.Ordinal),
            "stale element" => valid.Replace("<blob", "<n:HasExtendedXMP>OLD</n:HasExtendedXMP><blob", StringComparison.Ordinal),
            "nested stale element" => valid.Replace("PAYLOAD", "<n:HasExtendedXMP>OLD</n:HasExtendedXMP>PAYLOAD", StringComparison.Ordinal),
            _ => valid.Replace("PAYLOAD", "\ud800PAYLOAD", StringComparison.Ordinal)
        };
        packet = packet.Replace("PAYLOAD", new string('a', MaximumStandardBytes), StringComparison.Ordinal);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { XmpPacket = packet, AutoPartitionXmp = true, ExifData = new byte[] { 1, 2, 3 } },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("short opaque packet")]
    public void ExplicitExtensionConflictsWithAutomaticModeBeforeOutput(string? standard)
    {
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(destination, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { XmpPacket = standard, AutoPartitionXmp = true, ExtendedXmpPacket = "extension" },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.AutoPartitionXmp), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(false, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 2, false)]
    [InlineData(true, 2, true)]
    public void PartitioningLeavesAllCodingBytesAndDecodedPixelsUnchanged(bool progressive, int refinement, bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions { Progressive = progressive, ProgressiveRefinementBits = refinement, OptimizeHuffmanTables = optimize, RestartInterval = 2 };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken);
        byte[] actual = JpegliEncoder.Encode(pixels, width, height, options: options with
        {
            XmpPacket = SizedPacket(2 * ChunkBytes + 1), AutoPartitionXmp = true,
            ExifData = new byte[] { 1, 2, 3 }, IccProfile = new byte[] { 4, 5, 6 }, Comment = "metadata 😀"
        }, cancellationToken: TestContext.Current.CancellationToken);
        AssertAutomaticPacket(actual);
        int expectedStart = ReadHeaders(expected).First(segment => segment.Code == 0xdb).Start;
        int actualStart = ReadHeaders(actual).First(segment => segment.Code == 0xdb).Start;
        Assert.True(expected.AsSpan(expectedStart).SequenceEqual(actual.AsSpan(actualStart)));
        Assert.Equal(ImageResult.FromMemory(expected, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(actual, ColorComponents.RedGreenBlue).Data);
    }

    [Fact]
    public void NonseekableOutputMatchesMemoryEncodingAndRemainsOpen()
    {
        var options = AutomaticOptions();
        byte[] expected = Encode(options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, options, TestContext.Current.CancellationToken);
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
        output.WriteByte(42);
        Assert.Equal(expected.Length + 1, destination.Length);
    }

    [Fact]
    public void PreCancellationWritesNothingAndLeavesOutputOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, AutomaticOptions(), token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CancellationAtAutomaticXmpBoundariesStopsWithoutClosingOutput(int xmpSegment)
    {
        var options = AutomaticOptions();
        byte[] complete = Encode(options);
        Segment segment = ReadHeaders(complete).Where(value => value.Code == 0xe1).ElementAt(xmpSegment);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= segment.End)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..segment.End], destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void IoFailureWithinAutomaticXmpPropagatesWithoutClosingOutput(int xmpSegment)
    {
        var options = AutomaticOptions();
        byte[] complete = Encode(options);
        Segment segment = ReadHeaders(complete).Where(value => value.Code == 0xe1).ElementAt(xmpSegment);
        int boundary = segment.Start + 11;
        var expected = new IOException("Injected automatic-XMP write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, failureBoundary: boundary, failure: expected);
        var error = Assert.Throws<IOException>(() => Encode(output, options, TestContext.Current.CancellationToken));
        Assert.Same(expected, error);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegliEncoderOptions AutomaticOptions() => new()
    {
        XmpPacket = SizedPacket(2 * ChunkBytes + 1), AutoPartitionXmp = true, ExtendedXmpPacket = ""
    };

    private static string SizedPacket(int bytes)
    {
        const string prefix = "<?xpacket begin='' id='original'?>\n<x:xmpmeta xmlns:x='adobe:ns:meta/'><r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><r:Description r:about=''><blob xmlns='urn:test'>café 日本語 😀";
        const string suffix = "</blob></r:Description></r:RDF></x:xmpmeta>\n<?xpacket end='w'?>";
        return prefix + new string('a', bytes - Encoding.UTF8.GetByteCount(prefix + suffix)) + suffix;
    }

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static void Encode(Stream output, JpegliEncoderOptions options, CancellationToken token) =>
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options, cancellationToken: token);

    // Decode marker lengths, digest fields and offsets independently of the encoder.
    private static byte[] AssertAutomaticPacket(byte[] jpeg)
    {
        List<Segment> segments = ReadHeaders(jpeg);
        Segment standard = Assert.Single(segments, segment => HasSignature(segment, StandardSignature));
        Assert.InRange(standard.Length, 2, 65_535);
        List<Segment> chunks = segments.Where(segment => HasSignature(segment, ExtensionSignature)).ToList();
        Assert.NotEmpty(chunks);
        int standardIndex = segments.IndexOf(standard);
        Assert.Equal(chunks, segments.Skip(standardIndex + 1).Take(chunks.Count).ToList());
        Assert.Equal(0xe0, segments[0].Code);
        Assert.True(standardIndex < segments.FindIndex(segment => segment.Code == 0xdb));
        using var reconstructed = new MemoryStream();
        long fullLength = ReadUInt32(chunks[0].Payload.Span.Slice(67, 4));
        long offset = 0;
        string digest = Encoding.ASCII.GetString(chunks[0].Payload.Span.Slice(35, 32));
        foreach (Segment chunk in chunks)
        {
            Assert.Equal(digest, Encoding.ASCII.GetString(chunk.Payload.Span.Slice(35, 32)));
            Assert.Equal(fullLength, ReadUInt32(chunk.Payload.Span.Slice(67, 4)));
            Assert.Equal(offset, ReadUInt32(chunk.Payload.Span.Slice(71, 4)));
            int count = chunk.Payload.Length - ExtensionHeaderBytes;
            Assert.Equal(Math.Min(ChunkBytes, fullLength - offset), count);
            Assert.Equal(count + ExtensionHeaderBytes + 2, chunk.Length);
            Assert.InRange(chunk.Length, ExtensionHeaderBytes + 3, 65_535);
            reconstructed.Write(chunk.Payload.Span[ExtensionHeaderBytes..]);
            offset += count;
        }
        byte[] bytes = reconstructed.ToArray();
        Assert.Equal(fullLength, bytes.LongLength);
        Assert.Equal((fullLength + ChunkBytes - 1) / ChunkBytes, chunks.Count);
        Assert.Equal(Convert.ToHexString(MD5.HashData(bytes)), digest);
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
        string packet = new UTF8Encoding(false, true).GetString(bytes);
        XElement extensionRoot = XElement.Parse(packet, LoadOptions.PreserveWhitespace);
        Assert.True(extensionRoot.Name == Rdf + "RDF" || extensionRoot.Name == Meta + "xmpmeta");
        XDocument document = XDocument.Parse(Encoding.UTF8.GetString(standard.Payload.Span[StandardSignature.Length..]));
        string[] links = document.Descendants().Attributes(Note + "HasExtendedXMP").Select(attribute => attribute.Value)
            .Concat(document.Descendants(Note + "HasExtendedXMP").Select(element => element.Value)).ToArray();
        Assert.Equal(digest, Assert.Single(links));
        XElement standardRdf = document.Root!.Name == Rdf + "RDF" ? document.Root : Assert.Single(document.Root.Elements(Rdf + "RDF"));
        XElement description = Assert.Single(standardRdf.Elements(Rdf + "Description"));
        Assert.Equal("", (string?)description.Attribute(Rdf + "about"));
        Assert.All(description.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration), attribute =>
            Assert.True(attribute.Name == Rdf + "about" || attribute.Name == Note + "HasExtendedXMP"));
        Assert.All(description.Elements(), element => Assert.Equal(Note + "HasExtendedXMP", element.Name));
        return bytes;
    }

    private static void AssertRootPreserved(string original, byte[] extension)
    {
        XElement expected = XDocument.Parse(original, LoadOptions.PreserveWhitespace).Root!;
        XElement actual = XElement.Parse(new UTF8Encoding(false, true).GetString(extension), LoadOptions.PreserveWhitespace);
        Assert.True(XNode.DeepEquals(expected, actual), "The automatic extension must preserve the complete XML root and its metadata.");
    }

    private static long ReadUInt32(ReadOnlySpan<byte> field) =>
        (long)field[0] * 16_777_216 + field[1] * 65_536 + field[2] * 256 + field[3];

    private static bool HasSignature(Segment segment, byte[] signature) =>
        segment.Code == 0xe1 && segment.Payload.Span.StartsWith(signature);

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    private static List<Segment> ReadHeaders(byte[] jpeg)
    {
        Assert.True(jpeg.AsSpan(0, 2).SequenceEqual(new byte[] { 0xff, 0xd8 }));
        var result = new List<Segment>();
        int position = 2;
        while (true)
        {
            Assert.InRange(position, 2, jpeg.Length - 4);
            int start = position;
            Assert.Equal(0xff, jpeg[position++]);
            int code = jpeg[position++];
            int length = jpeg[position] * 256 + jpeg[position + 1];
            Assert.InRange(length, 2, jpeg.Length - position);
            result.Add(new(code, start, length, jpeg.AsMemory(position + 2, length - 2)));
            position += length;
            if (code == 0xda)
                return result;
        }
    }

    private sealed class WriteOnlyStream(Stream inner, Action? afterWrite = null, long? failureBoundary = null, IOException? failure = null) : Stream
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
