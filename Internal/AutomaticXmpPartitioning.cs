using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotJpegli.Internal;

/// <summary>Moves an oversized XMP tree into the extension without splitting its properties.</summary>
internal static class AutomaticXmpPartitioning
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Meta = "adobe:ns:meta/";
    private static readonly XName Link = XName.Get("HasExtendedXMP", "http://ns.adobe.com/xmp/note/");

    // Adobe XMP Specification Part 3, section 1.1.3.2 leaves the partition
    // policy to applications. Keep the full tree in the extension so arbitrary
    // arrays, structures, qualifiers and inherited XML attributes stay together.
    // Readers that only support standard XMP will see the generated link alone.
    internal static string? Prepare(string packet, CancellationToken cancellationToken)
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
            string extended = root.ToString(SaveOptions.DisableFormatting);
            cancellationToken.ThrowIfCancellationRequested();
            return extended;
        }
        catch (XmlException error)
        {
            throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket), error);
        }
    }

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
