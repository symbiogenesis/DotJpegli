using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotJpegli.Internal;

/// <summary>Partitions an oversized XMP tree without splitting individual properties.</summary>
internal static class AutomaticXmpPartitioning
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Meta = "adobe:ns:meta/";
    private static readonly XName Link = XName.Get("HasExtendedXMP", "http://ns.adobe.com/xmp/note/");

    // Adobe XMP Specification Part 3, section 1.1.3.2 leaves the partition
    // policy to applications. The default keeps the full tree in the extension.
    // Explicitly selected top-level properties stay in standard XMP as complete
    // subtrees; both envelopes retain inherited XML attributes and namespaces.
    internal sealed record Partition(string ExtendedPacket, XElement? StandardRoot);

    internal static HashSet<XName> CreateSelection(JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        var names = new HashSet<XName>();
        if (options.StandardXmpProperties is not { Count: > 0 } properties)
            return names;
        if (!options.AutoPartitionXmp)
            throw new ArgumentException("Selecting standard XMP properties requires automatic partitioning.",
                nameof(options.StandardXmpProperties));
        foreach (XmpPropertyName property in properties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (string.IsNullOrEmpty(property.NamespaceUri) || string.IsNullOrEmpty(property.LocalName))
                    throw new XmlException("Property names require a namespace URI and local name.");
                XmlConvert.VerifyNCName(property.LocalName);
                XName name = XName.Get(property.LocalName, property.NamespaceUri);
                if (name.Namespace == Rdf || name.Namespace == Meta || name.Namespace == XNamespace.Xml ||
                    name.Namespace == XNamespace.Xmlns || name == Link)
                    throw new XmlException("Envelope and digest-link names cannot be selected as properties.");
                names.Add(name);
            }
            catch (XmlException error)
            {
                throw new ArgumentException("Standard XMP selections must name properties with a nonempty namespace URI " +
                    "and an XML NCName local name, excluding envelope namespaces and HasExtendedXMP.",
                    nameof(options.StandardXmpProperties), error);
            }
        }
        return names;
    }

    internal static Partition? Prepare(string packet, HashSet<XName> selection, CancellationToken cancellationToken)
    {
        var chunks = new ExtendedXmpMetadata.Utf8Chunks(packet);
        int length = 0;
        try
        {
            while (length <= JpegMarkerWriter.MaxXmpPacketBytes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReadOnlySpan<byte> chunk = chunks.Read();
                if (chunk.IsEmpty)
                    return null;
                length += chunk.Length;
            }
        }
        catch (EncoderFallbackException error)
        {
            throw new ArgumentException("The XMP packet must contain well-formed UTF-16.",
                nameof(JpegliEncoderOptions.XmpPacket), error);
        }

        const string message = "Automatic partitioning requires an XMP XML root containing only top-level " +
            "rdf:Description elements with empty or absent rdf:about, without rdf:ID/nodeID, existing " +
            "HasExtendedXMP links, DTDs or nested xpacket instructions.";
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = packet.Length
            };
            using var input = new CancellableReader(packet, cancellationToken);
            using var reader = XmlReader.Create(input, settings);
            XDocument document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            XElement? root = document.Root;
            XElement? rdf = root?.Name == Rdf + "RDF" ? root
                : root?.Name == Meta + "xmpmeta" && root.Elements().Count() == 1
                    ? root.Element(Rdf + "RDF") : null;
            if (rdf is null || HasNonWhitespaceText(root!) || HasNonWhitespaceText(rdf) || !rdf.Elements().Any() ||
                rdf.Elements().Any(element => element.Name != Rdf + "Description"))
                throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));

            foreach (XElement description in rdf.Elements())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty((string?)description.Attribute(Rdf + "about")) ||
                    description.Attribute(Rdf + "ID") is not null || description.Attribute(Rdf + "nodeID") is not null)
                    throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));
            }
            foreach (XElement element in root!.DescendantsAndSelf())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (element.Name == Link || element.Attribute(Link) is not null ||
                    element.Nodes().OfType<XProcessingInstruction>().Any(instruction => instruction.Target == "xpacket"))
                    throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));
            }

            // Serialize only the original root: declarations, document padding
            // and outer xpacket instructions are not part of extended XMP.
            XElement? standardRoot = null;
            if (selection.Count != 0)
            {
                // Copy envelope attributes, including namespace declarations and
                // inherited XML context, without large formatting/padding nodes.
                // Move complete selected properties into those shells so their
                // own whitespace, qualifiers and structured values stay intact.
                standardRoot = new XElement(root.Name, root.Attributes());
                XElement standardRdf = root == rdf ? standardRoot : new XElement(rdf.Name, rdf.Attributes());
                if (root != rdf)
                    standardRoot.Add(standardRdf);
                foreach (XElement description in rdf.Elements())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var standardDescription = new XElement(description.Name);
                    standardRdf.Add(standardDescription);
                    foreach (XAttribute attribute in description.Attributes().ToArray())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (attribute.IsNamespaceDeclaration || attribute.Name.Namespace == Rdf ||
                            attribute.Name.Namespace == XNamespace.Xml || selection.Contains(attribute.Name))
                            standardDescription.Add(new XAttribute(attribute));
                        if (selection.Contains(attribute.Name))
                            attribute.Remove();
                    }
                    foreach (XElement property in description.Elements().ToArray())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (selection.Contains(property.Name))
                        {
                            property.Remove();
                            standardDescription.Add(property);
                        }
                    }
                }
            }
            string extended = root.ToString(SaveOptions.DisableFormatting);
            cancellationToken.ThrowIfCancellationRequested();
            return new(extended, standardRoot);
        }
        catch (XmlException error)
        {
            throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket), error);
        }
    }

    internal static string CreateStandardPacket(XElement root, string digest, CancellationToken cancellationToken)
    {
        // Existing links were rejected during envelope validation. Use the first
        // description so the combined standard packet has exactly one digest link.
        GetRdf(root)!.Elements(Rdf + "Description").First().Add(new XAttribute(Link, digest));
        string packet = root.ToString(SaveOptions.DisableFormatting);
        cancellationToken.ThrowIfCancellationRequested();
        if (packet.Length > JpegMarkerWriter.MaxXmpPacketBytes ||
            JpegMarkerWriter.XmpEncoding.GetByteCount(packet) > JpegMarkerWriter.MaxXmpPacketBytes)
            throw new ArgumentOutOfRangeException(nameof(JpegliEncoderOptions.StandardXmpProperties),
                "The selected properties, XML envelope and digest link must fit in 65,504 UTF-8 bytes.");
        return packet;
    }

    private static XElement? GetRdf(XElement root) => root.Name == Rdf + "RDF" ? root : root.Element(Rdf + "RDF");

    private static bool HasNonWhitespaceText(XElement element) =>
        element.Nodes().OfType<XText>().Any(text => !string.IsNullOrWhiteSpace(text.Value));

    private sealed class CancellableReader(string packet, CancellationToken cancellationToken) : StringReader(packet)
    {
        public override int Read(char[] buffer, int index, int count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return base.Read(buffer, index, count);
        }

        public override int Read(Span<char> buffer)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return base.Read(buffer);
        }
    }
}
