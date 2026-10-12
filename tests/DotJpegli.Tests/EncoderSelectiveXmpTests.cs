using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderSelectiveXmpTests
{
    private const int MaximumStandardBytes = 65_504;
    private const int ChunkBytes = 65_458;
    private const int ExtensionHeaderBytes = 75;
    private const string SelectedNamespace = "urn:selected";
    private static readonly byte[] StandardSignature = "http://ns.adobe.com/xap/1.0/\0"u8.ToArray();
    private static readonly byte[] ExtensionSignature = "http://ns.adobe.com/xmp/extension/\0"u8.ToArray();
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Note = "http://ns.adobe.com/xmp/note/";
    private static readonly XNamespace Meta = "adobe:ns:meta/";
    private static readonly XName LinkName = Note + "HasExtendedXMP";

    [Fact]
    public void DefaultAndEmptySelectionsPreserveExistingAutomaticBehavior()
    {
        Assert.Null(new JpegliEncoderOptions().StandardXmpProperties);
        Assert.Equal(Encode(new()), Encode(new() { StandardXmpProperties = [] }));
        var options = new JpegliEncoderOptions { XmpPacket = LargePacket(), AutoPartitionXmp = true };
        Assert.Equal(Encode(options), Encode(options with { StandardXmpProperties = [] }));
        Assert.Empty(Properties(ReadPartition(Encode(options)).Standard));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("opaque XMP\0 café 日本語 😀 & unescaped")]
    public void ValidSelectionDoesNotParseOrChangeSmallPackets(string? packet)
    {
        var options = new JpegliEncoderOptions { XmpPacket = packet, AutoPartitionXmp = true };
        byte[] expected = Encode(options);
        byte[] actual = Encode(options with { StandardXmpProperties = [new(SelectedNamespace, "title")] });
        Assert.Equal(expected, actual);
        Assert.DoesNotContain(ReadHeaders(actual), segment => HasSignature(segment, ExtensionSignature));
        if (!string.IsNullOrEmpty(packet))
        {
            Segment standard = Assert.Single(ReadHeaders(actual), segment => HasSignature(segment, StandardSignature));
            Assert.Equal(Encoding.UTF8.GetBytes(packet), standard.Payload[StandardSignature.Length..].ToArray());
        }
    }

    [Theory]
    [InlineData(MaximumStandardBytes - 1)]
    [InlineData(MaximumStandardBytes)]
    public void ValidSelectionLeavesExactlyFittingOpaqueUtf8PacketsUnchanged(int byteLength)
    {
        const string prefix = "opaque café 日本語 😀 ";
        string packet = prefix + new string('a', byteLength - Encoding.UTF8.GetByteCount(prefix));
        var options = new JpegliEncoderOptions { XmpPacket = packet, AutoPartitionXmp = true };
        byte[] actual = Encode(options with { StandardXmpProperties = [new(SelectedNamespace, "title")] });
        Assert.Equal(Encode(options), actual);
        Segment standard = Assert.Single(ReadHeaders(actual), segment => HasSignature(segment, StandardSignature));
        Assert.Equal(byteLength, standard.Payload.Length - StandardSignature.Length);
        Assert.DoesNotContain(ReadHeaders(actual), segment => HasSignature(segment, ExtensionSignature));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("opaque")]
    public void NonemptySelectionRequiresAutomaticModeBeforeOutput(string? packet)
    {
        AssertRejected(new() { XmpPacket = packet, StandardXmpProperties = [new(SelectedNamespace, "title")] });
    }

    [Theory]
    [InlineData(null, "title")]
    [InlineData("", "title")]
    [InlineData(SelectedNamespace, null)]
    [InlineData(SelectedNamespace, "")]
    [InlineData(SelectedNamespace, "a:b")]
    [InlineData(SelectedNamespace, "two words")]
    [InlineData(SelectedNamespace, "1title")]
    [InlineData(SelectedNamespace, "title\0")]
    [InlineData("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "value")]
    [InlineData("http://www.w3.org/XML/1998/namespace", "lang")]
    [InlineData("http://www.w3.org/2000/xmlns/", "prefix")]
    [InlineData("adobe:ns:meta/", "xmpmeta")]
    [InlineData("http://ns.adobe.com/xmp/note/", "HasExtendedXMP")]
    public void InvalidExpandedNamesAreRejectedBeforeOutput(string? namespaceUri, string? localName)
    {
        AssertRejected(new()
        {
            XmpPacket = "opaque", AutoPartitionXmp = true,
            StandardXmpProperties = [new(SelectedNamespace, "title"), new(namespaceUri!, localName!)]
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void InvalidSelectionIsValidatedEvenWithoutXmp(string? packet)
    {
        AssertRejected(new() { XmpPacket = packet, AutoPartitionXmp = true, StandardXmpProperties = [default] });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SelectionMovesEveryMatchingTopLevelPropertyAndPreservesItsContext(bool directRdf)
    {
        string packet = RichPacket(directRdf);
        XElement original = XDocument.Parse(packet, LoadOptions.PreserveWhitespace).Root!;
        XmpPropertyName[] selection =
        [
            new(SelectedNamespace, "title"), new(SelectedNamespace, "rating"), new(SelectedNamespace, "empty"),
            new(SelectedNamespace, "title"), new(SelectedNamespace, "absent")
        ];
        Partition partition = ReadPartition(Encode(new()
        {
            XmpPacket = packet, AutoPartitionXmp = true, StandardXmpProperties = selection
        }));
        var names = selection.Select(name => XName.Get(name.LocalName, name.NamespaceUri)).ToHashSet();
        Property[] all = Properties(original);
        AssertPropertiesEqual(all.Where(property => names.Contains(property.Name)), Properties(partition.Standard));
        AssertPropertiesEqual(all.Where(property => !names.Contains(property.Name)), Properties(partition.Extension));
        AssertPropertiesEqual(all, Properties(partition.Standard).Concat(Properties(partition.Extension)));
        Assert.Equal(5, Properties(partition.Standard).Length);
        Assert.Contains(Properties(partition.Standard), property => property.Name == XName.Get("empty", SelectedNamespace) && property.Value == "");
        Assert.DoesNotContain(Properties(partition.Extension), property => names.Contains(property.Name));
        AssertEnvelopesPreserved(original, partition.Standard);
        AssertEnvelopesPreserved(original, partition.Extension);
        Assert.DoesNotContain("<?xpacket", Encoding.UTF8.GetString(partition.ExtensionBytes), StringComparison.Ordinal);
    }

    [Fact]
    public void MissingAndCaseDifferentNamesDoNotSelectAnyProperty()
    {
        string packet = LargePacket();
        var options = new JpegliEncoderOptions { XmpPacket = packet, AutoPartitionXmp = true };
        Partition original = ReadPartition(Encode(options));
        Partition selected = ReadPartition(Encode(options with
        {
            StandardXmpProperties = [new(SelectedNamespace, "Title"), new("URN:selected", "title"), new("urn:missing", "title")]
        }));
        Assert.Empty(Properties(selected.Standard));
        AssertPropertiesEqual(Properties(original.Extension), Properties(selected.Extension));
    }

    [Fact]
    public void DuplicateSelectorsDoNotDuplicatePropertiesOrChangeBytes()
    {
        var options = new JpegliEncoderOptions
        {
            XmpPacket = LargePacket(), AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")]
        };
        Assert.Equal(Encode(options), Encode(options with
        {
            StandardXmpProperties = [new(SelectedNamespace, "title"), new(SelectedNamespace, "title"), new(SelectedNamespace, "title")]
        }));
    }

    [Fact]
    public void UnicodeNcNamesAndDifferentNotePropertiesCanBeSelected()
    {
        const string prefix = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected' xmlns:n='http://ns.adobe.com/xmp/note/'><r:Description r:about='' t:état='café'><t:日本語>😀</t:日本語><n:Other>note</n:Other><t:blob>";
        const string suffix = "</t:blob></r:Description></r:RDF>";
        string packet = prefix + new string('a', MaximumStandardBytes) + suffix;
        Partition partition = ReadPartition(Encode(new()
        {
            XmpPacket = packet, AutoPartitionXmp = true,
            StandardXmpProperties = [new(SelectedNamespace, "état"), new(SelectedNamespace, "日本語"), new(Note.NamespaceName, "Other")]
        }));
        Assert.Equal(3, Properties(partition.Standard).Length);
        Assert.Equal("café", Assert.Single(Properties(partition.Standard), property => property.Name.LocalName == "état").Value);
        Assert.Equal(XName.Get("blob", SelectedNamespace), Assert.Single(Properties(partition.Extension)).Name);
    }

    [Fact]
    public void SelectingEveryPropertyAllowsAnExtensionWithOnlyEnvelopes()
    {
        const string root = "<x:xmpmeta xmlns:x='adobe:ns:meta/' xml:lang='en'><r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected'><r:Description r:about='' t:empty=''><t:title>café 😀</t:title></r:Description></r:RDF></x:xmpmeta>";
        string packet = "<?xpacket begin=''?>" + root + new string(' ', MaximumStandardBytes + 1) + "<?xpacket end='w'?>";
        Partition partition = ReadPartition(Encode(new()
        {
            XmpPacket = packet, AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title"), new(SelectedNamespace, "empty")]
        }));
        XElement original = XElement.Parse(root);
        AssertPropertiesEqual(Properties(original), Properties(partition.Standard));
        Assert.Empty(Properties(partition.Extension));
        Assert.True(partition.ExtensionBytes.Length < MaximumStandardBytes);
        AssertEnvelopesPreserved(original, partition.Extension);
    }

    [Fact]
    public void InterPropertyWhitespaceStaysInTheExtensionAndDoesNotFillStandard()
    {
        const string prefix = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected'><r:Description r:about=''><t:title>  café 😀  </t:title>";
        const string suffix = "<!--envelope comment--><?envelope value?><t:blob>remainder</t:blob></r:Description></r:RDF>";
        string whitespace = "\n" + new string(' ', 2 * ChunkBytes);
        string packet = prefix + whitespace + suffix;
        Partition partition = ReadPartition(Encode(new()
        {
            XmpPacket = packet, AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")]
        }));
        Assert.True(partition.StandardBytes.Length < 1_000);
        Assert.Equal("  café 😀  ", Assert.Single(Properties(partition.Standard)).Value);
        Assert.Equal("remainder", Assert.Single(Properties(partition.Extension)).Value);
        Assert.Contains(whitespace, Encoding.UTF8.GetString(partition.ExtensionBytes), StringComparison.Ordinal);
        Assert.Contains("<!--envelope comment-->", Encoding.UTF8.GetString(partition.ExtensionBytes), StringComparison.Ordinal);
        Assert.Contains("<?envelope value?>", Encoding.UTF8.GetString(partition.ExtensionBytes), StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedStandardLimitCountsUtf8BytesAndIncludesGeneratedLink()
    {
        var options = new JpegliEncoderOptions
        {
            XmpPacket = LargePacket(), AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")]
        };
        int initialLength = ReadPartition(Encode(options)).StandardBytes.Length;
        string title = "café 😀";
        int remaining = MaximumStandardBytes - initialLength;
        string filling = new string('é', remaining / 2) + new string('a', remaining % 2);
        string packet = LargePacket(title + filling);
        Partition partition = ReadPartition(Encode(options with { XmpPacket = packet }));
        Assert.Equal(MaximumStandardBytes, partition.StandardBytes.Length);
        Assert.True(Encoding.UTF8.GetString(partition.StandardBytes).Length < MaximumStandardBytes);
        Assert.Equal(title + filling, Assert.Single(Properties(partition.Standard)).Value);
        AssertRejected(options with { XmpPacket = LargePacket(title + filling + "a") }, sizeOverflow: true);
    }

    [Fact]
    public void SelectedMultibyteAttributeCanExceedTheLimitWhileItsCharacterCountFits()
    {
        const string prefix = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected'><r:Description r:about='' t:title='";
        const string suffix = "'><t:blob>unselected</t:blob></r:Description></r:RDF>";
        string packet = prefix + new string('é', 34_000) + suffix;
        Assert.True(packet.Length < MaximumStandardBytes);
        Assert.True(Encoding.UTF8.GetByteCount(packet) > MaximumStandardBytes);
        AssertRejected(new() { XmpPacket = packet, AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")] }, sizeOverflow: true);
    }

    [Theory]
    [InlineData(false, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 2, true)]
    public void SelectionLeavesCodingBytesAndDecodedPixelsUnchanged(bool progressive, int refinement, bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions
        {
            Progressive = progressive, ProgressiveRefinementBits = refinement, OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken);
        byte[] actual = JpegliEncoder.Encode(pixels, width, height, options: options with
        {
            XmpPacket = RichPacket(false), AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")],
            ExifData = new byte[] { 1, 2, 3 }, IccProfile = new byte[] { 4, 5, 6 }, Comment = "metadata 😀"
        }, cancellationToken: TestContext.Current.CancellationToken);
        ReadPartition(actual);
        int expectedStart = ReadHeaders(expected).First(segment => segment.Code == 0xdb).Start;
        int actualStart = ReadHeaders(actual).First(segment => segment.Code == 0xdb).Start;
        Assert.True(expected.AsSpan(expectedStart).SequenceEqual(actual.AsSpan(actualStart)));
        Assert.Equal(ImageResult.FromMemory(expected, ColorComponents.RedGreenBlue).Data,
            ImageResult.FromMemory(actual, ColorComponents.RedGreenBlue).Data);
    }

    [Fact]
    public void NonseekableOutputMatchesMemoryEncodingAndRemainsOpen()
    {
        JpegliEncoderOptions options = SelectedOptions();
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
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, SelectedOptions(), token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CancellationAtSelectedXmpBoundariesStopsWithoutClosingOutput(int xmpSegment)
    {
        JpegliEncoderOptions options = SelectedOptions();
        byte[] complete = Encode(options);
        Segment segment = ReadHeaders(complete).Where(value => value.Code == 0xe1).ElementAt(xmpSegment);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= segment.End)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..segment.End], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegliEncoderOptions SelectedOptions() => new()
    {
        XmpPacket = RichPacket(false), AutoPartitionXmp = true, StandardXmpProperties = [new(SelectedNamespace, "title")]
    };

    private static string LargePacket(string title = "café 😀") =>
        "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected'><r:Description r:about=''><t:title>" +
        title + "</t:title><t:blob>" + new string('a', 2 * ChunkBytes + 1) + "</t:blob></r:Description></r:RDF>";

    private static string RichPacket(bool directRdf)
    {
        string rdf = "<r:RDF xmlns:r='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:t='urn:selected' xmlns:u='urn:unselected' xmlns:unused='urn:preserved' xml:lang='fr' xml:base='https://example.invalid/rdf/' xml:space='preserve'>" +
            "<r:Description r:about='' t:rating='5' t:empty='' u:rating='other' xml:lang='de' xml:base='description/' xml:space='default'>" +
            "<t:title u:qualifier='café' xml:lang='x-default'><r:Alt><r:li xml:lang='en'>café 😀</r:li><r:li xml:lang='ja'>日本語</r:li></r:Alt><!--keep--><?value preserved?></t:title>" +
            "<u:title>same local name</u:title><u:object r:parseType='Resource'><t:title>nested title stays with its parent</t:title></u:object>" +
            "<u:blob>" + new string('a', ChunkBytes - 100) + "😀" + new string('é', ChunkBytes) + "</u:blob></r:Description>" +
            "<r:Description xmlns:alias='urn:selected'><alias:title r:parseType='Resource'><u:child r:resource='urn:child'/></alias:title><alias:rating>second</alias:rating><u:other>retained</u:other></r:Description>" +
            "</r:RDF>";
        string root = directRdf ? rdf : "<x:xmpmeta xmlns:x='adobe:ns:meta/' xmlns:outer='urn:outer' xml:lang='en' xml:base='https://example.invalid/outer/' xml:space='preserve'>" + rdf + "</x:xmpmeta>";
        return "<?xml version='1.0'?><?xpacket begin=''?>" + root + "  <?xpacket end='w'?>";
    }

    private static void AssertRejected(JpegliEncoderOptions options, bool sizeOverflow = false)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Action encode = () => Encode(output, options with { ExifData = new byte[] { 1, 2, 3 } }, TestContext.Current.CancellationToken);
        ArgumentException error = sizeOverflow ? Assert.Throws<ArgumentOutOfRangeException>(encode) : Assert.Throws<ArgumentException>(encode);
        Assert.Equal(nameof(JpegliEncoderOptions.StandardXmpProperties), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    private static byte[] Encode(JpegliEncoderOptions options) => JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1,
        options: options, cancellationToken: TestContext.Current.CancellationToken);

    private static void Encode(Stream output, JpegliEncoderOptions options, CancellationToken token) =>
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options, cancellationToken: token);

    // Read JPEG marker lengths, chunk offsets and the digest independently of the implementation.
    private static Partition ReadPartition(byte[] jpeg)
    {
        List<Segment> segments = ReadHeaders(jpeg);
        Segment standard = Assert.Single(segments, segment => HasSignature(segment, StandardSignature));
        byte[] standardBytes = standard.Payload[StandardSignature.Length..].ToArray();
        Assert.InRange(standardBytes.Length, 1, MaximumStandardBytes);
        Assert.Equal(0xe0, segments[0].Code);
        List<Segment> chunks = segments.Where(segment => HasSignature(segment, ExtensionSignature)).ToList();
        Assert.NotEmpty(chunks);
        Assert.Equal(chunks, segments.Skip(segments.IndexOf(standard) + 1).Take(chunks.Count).ToList());
        Assert.True(segments.IndexOf(standard) < segments.FindIndex(segment => segment.Code == 0xdb));
        long length = ReadUInt32(chunks[0].Payload.Span.Slice(67, 4));
        string digest = Encoding.ASCII.GetString(chunks[0].Payload.Span.Slice(35, 32));
        using var reconstructed = new MemoryStream();
        foreach (Segment chunk in chunks)
        {
            Assert.Equal(digest, Encoding.ASCII.GetString(chunk.Payload.Span.Slice(35, 32)));
            Assert.Equal(length, ReadUInt32(chunk.Payload.Span.Slice(67, 4)));
            Assert.Equal(reconstructed.Length, ReadUInt32(chunk.Payload.Span.Slice(71, 4)));
            int count = chunk.Payload.Length - ExtensionHeaderBytes;
            Assert.Equal(Math.Min(ChunkBytes, length - reconstructed.Length), count);
            Assert.InRange(chunk.Length, ExtensionHeaderBytes + 3, 65_535);
            reconstructed.Write(chunk.Payload.Span[ExtensionHeaderBytes..]);
        }
        byte[] extensionBytes = reconstructed.ToArray();
        Assert.Equal(length, extensionBytes.LongLength);
        Assert.Equal((length + ChunkBytes - 1) / ChunkBytes, chunks.Count);
        Assert.Equal(Convert.ToHexString(MD5.HashData(extensionBytes)), digest);
        var utf8 = new UTF8Encoding(false, true);
        XElement standardRoot = XElement.Parse(utf8.GetString(standardBytes), LoadOptions.PreserveWhitespace);
        XElement extensionRoot = XElement.Parse(utf8.GetString(extensionBytes), LoadOptions.PreserveWhitespace);
        Assert.False(standardBytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
        Assert.False(extensionBytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
        string[] links = standardRoot.DescendantsAndSelf().Attributes(LinkName).Select(attribute => attribute.Value)
            .Concat(standardRoot.DescendantsAndSelf(LinkName).Select(element => element.Value)).ToArray();
        Assert.Equal(digest, Assert.Single(links));
        Assert.Empty(extensionRoot.DescendantsAndSelf().Attributes(LinkName));
        Assert.Empty(extensionRoot.DescendantsAndSelf(LinkName));
        return new(standardRoot, extensionRoot, standardBytes, extensionBytes);
    }

    private static Property[] Properties(XElement root)
    {
        var properties = new List<Property>();
        XElement[] descriptions = RdfRoot(root).Elements(Rdf + "Description").ToArray();
        for (int index = 0; index < descriptions.Length; index++)
        {
            XElement description = descriptions[index];
            foreach (XAttribute attribute in description.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration &&
                attribute.Name.Namespace != Rdf && attribute.Name.Namespace != XNamespace.Xml && attribute.Name != LinkName))
                properties.Add(new(attribute.Name, index, "attribute", attribute.Value, Context(description)));
            foreach (XElement element in description.Elements().Where(element => element.Name != LinkName))
                properties.Add(new(element.Name, index, "element", element.Value, Context(element) + Canonical(element)));
        }
        return properties.ToArray();
    }

    private static string Context(XElement element) => string.Join("|", new[] { "lang", "base", "space" }.Select(local =>
        local + "=" + element.AncestorsAndSelf().Select(ancestor => (string?)ancestor.Attribute(XNamespace.Xml + local)).FirstOrDefault(value => value is not null)));

    private static string Canonical(XNode node) => node switch
    {
        XElement element => "<" + element.Name + string.Concat(element.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration)
            .OrderBy(attribute => attribute.Name.ToString(), StringComparer.Ordinal).Select(attribute => "[" + attribute.Name + "]=" + attribute.Value)) + ">" +
            string.Concat(element.Nodes().Select(Canonical)) + "</" + element.Name + ">",
        XText text => "text:" + text.Value,
        XComment comment => "comment:" + comment.Value,
        XProcessingInstruction instruction => "pi:" + instruction.Target + ":" + instruction.Data,
        _ => node.ToString(SaveOptions.DisableFormatting)
    };

    private static void AssertPropertiesEqual(IEnumerable<Property> expected, IEnumerable<Property> actual) =>
        Assert.Equal(expected.OrderBy(property => property.ToString(), StringComparer.Ordinal), actual.OrderBy(property => property.ToString(), StringComparer.Ordinal));

    private static XElement RdfRoot(XElement root) => root.Name == Rdf + "RDF" ? root : Assert.Single(root.Elements(Rdf + "RDF"));

    private static void AssertEnvelopesPreserved(XElement original, XElement actual)
    {
        Assert.Equal(original.Name, actual.Name);
        AssertEnvelopeAttributes(original, actual);
        XElement expectedRdf = RdfRoot(original);
        XElement actualRdf = RdfRoot(actual);
        AssertEnvelopeAttributes(expectedRdf, actualRdf);
        XElement[] expectedDescriptions = expectedRdf.Elements(Rdf + "Description").ToArray();
        XElement[] actualDescriptions = actualRdf.Elements(Rdf + "Description").ToArray();
        Assert.Equal(expectedDescriptions.Length, actualDescriptions.Length);
        for (int index = 0; index < expectedDescriptions.Length; index++)
            AssertEnvelopeAttributes(expectedDescriptions[index], actualDescriptions[index]);
    }

    private static void AssertEnvelopeAttributes(XElement expected, XElement actual)
    {
        foreach (XAttribute attribute in expected.Attributes().Where(attribute => attribute.IsNamespaceDeclaration ||
            attribute.Name.Namespace == XNamespace.Xml || attribute.Name.Namespace == Rdf))
            Assert.Equal(attribute.Value, (string?)actual.Attribute(attribute.Name));
        foreach (XAttribute declaration in expected.AncestorsAndSelf().SelectMany(element => element.Attributes().Where(attribute => attribute.IsNamespaceDeclaration))
            .GroupBy(attribute => attribute.Name).Select(group => group.First()))
        {
            string prefix = declaration.Name.Namespace == XNamespace.Xmlns ? declaration.Name.LocalName : "";
            Assert.Equal(declaration.Value, prefix.Length == 0 ? actual.GetDefaultNamespace().NamespaceName : actual.GetNamespaceOfPrefix(prefix)?.NamespaceName);
        }
    }

    private static long ReadUInt32(ReadOnlySpan<byte> field) => (long)field[0] * 16_777_216 + field[1] * 65_536 + field[2] * 256 + field[3];
    private static bool HasSignature(Segment segment, byte[] signature) => segment.Code == 0xe1 && segment.Payload.Span.StartsWith(signature);
    private sealed record Property(XName Name, int Description, string Kind, string Value, string Semantics);
    private sealed record Partition(XElement Standard, XElement Extension, byte[] StandardBytes, byte[] ExtensionBytes);
    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

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

    private sealed class WriteOnlyStream(Stream inner, Action? afterWrite = null) : Stream
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
            inner.Write(buffer);
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
