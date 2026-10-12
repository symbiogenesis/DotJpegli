using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotJpegli.Internal;

/// <summary>Prepares JPEG extended XMP and its standard-packet link before any output.</summary>
internal sealed record ExtendedXmpMetadata(string Packet, string StandardPacket, string Digest, uint Length)
{
    // Adobe XMP Specification Part 3, section 1.1.3.1: a 75-byte prefix
    // (signature, GUID, full length and offset) leaves 65,458 bytes per APP1.
    internal const int MaxChunkBytes = JpegMarkerWriter.MaxMarkerPayloadBytes - 75;
    private static readonly XNamespace Note = "http://ns.adobe.com/xmp/note/";
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Meta = "adobe:ns:meta/";

    internal static ExtendedXmpMetadata? Prepare(JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        string? automaticPacket = options.AutoPartitionXmp && !string.IsNullOrEmpty(options.XmpPacket)
            ? AutomaticXmpPartitioning.Prepare(options.XmpPacket, cancellationToken) : null;
        string? packet = automaticPacket ?? options.ExtendedXmpPacket;
        if (string.IsNullOrEmpty(packet))
            return null;

        string parameter = automaticPacket is null
            ? nameof(options.ExtendedXmpPacket) : nameof(options.XmpPacket);
        var chunks = new Utf8Chunks(packet);
        // MD5 is mandated by the XMP storage format as a packet identifier;
        // it is not used for authentication or a security decision.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        uint length = 0;
        try
        {
            ReadOnlySpan<byte> chunk;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                chunk = chunks.Read();
                if (chunk.IsEmpty)
                    break;
                if ((ulong)length + (uint)chunk.Length > uint.MaxValue)
                    throw new ArgumentOutOfRangeException(parameter,
                        "The UTF-8 extended XMP packet must fit in an unsigned 32-bit byte length.");
                length += (uint)chunk.Length;
                hash.AppendData(chunk);
            }
        }
        catch (EncoderFallbackException error)
        {
            throw new ArgumentException("The extended XMP packet must contain well-formed UTF-16.",
                parameter, error);
        }

        string digest = Convert.ToHexString(hash.GetHashAndReset());
        string standard = automaticPacket is not null || string.IsNullOrEmpty(options.XmpPacket)
            ? CreateStandardPacket(digest) : options.XmpPacket;
        if (automaticPacket is null && !string.IsNullOrEmpty(options.XmpPacket))
            ValidateLink(standard, digest);
        return new(packet, standard, digest, length);
    }

    private static string CreateStandardPacket(string digest) =>
        "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n" +
        "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
        "<rdf:Description rdf:about=\"\" xmlns:xmpNote=\"http://ns.adobe.com/xmp/note/\" xmpNote:HasExtendedXMP=\"" +
        digest + "\"/></rdf:RDF></x:xmpmeta>\n<?xpacket end=\"w\"?>";

    private static void ValidateLink(string packet, string digest)
    {
        const string message = "With extended XMP, the standard XMP packet must contain exactly one top-level " +
            "xmpNote:HasExtendedXMP simple property equal to the uppercase MD5 digest of the extended packet's UTF-8 bytes.";
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = JpegMarkerWriter.MaxXmpPacketBytes
            };
            using var input = new StringReader(packet);
            using var reader = XmlReader.Create(input, settings);
            XDocument document = XDocument.Load(reader);
            XElement? rdf = document.Root?.Name == Rdf + "RDF" ? document.Root
                : document.Root?.Name == Meta + "xmpmeta" ? document.Root.Elements(Rdf + "RDF").SingleOrDefault()
                : null;
            if (rdf is null)
                throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));

            int links = 0;
            foreach (XElement description in rdf.Elements(Rdf + "Description"))
            {
                XAttribute? attribute = description.Attribute(Note + "HasExtendedXMP");
                if (attribute is not null)
                {
                    links++;
                    if (attribute.Value != digest)
                        throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));
                }
                foreach (XElement property in description.Elements(Note + "HasExtendedXMP"))
                {
                    links++;
                    if (property.HasElements || property.Attributes().Any(value => !value.IsNamespaceDeclaration) ||
                        property.Value != digest)
                        throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));
                }
            }
            if (links != 1)
                throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket));
        }
        catch (XmlException error)
        {
            throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket), error);
        }
        catch (InvalidOperationException error)
        {
            throw new ArgumentException(message, nameof(JpegliEncoderOptions.XmpPacket), error);
        }
    }

    /// <summary>Encodes a string into fixed byte chunks, including splits inside UTF-8 sequences.</summary>
    internal sealed class Utf8Chunks(string packet)
    {
        private readonly Encoder encoder = JpegMarkerWriter.XmpEncoding.GetEncoder();
        // Encode complete Unicode scalars into three extra bytes, then carry any
        // excess to the next chunk. This preserves fixed byte offsets even when
        // a multibyte character spans APP1 segments.
        private readonly byte[] buffer = new byte[MaxChunkBytes + 3];
        private int position;
        private int pendingBytes;

        internal ReadOnlySpan<byte> Read()
        {
            if (pendingBytes != 0)
                buffer.AsSpan(MaxChunkBytes, pendingBytes).CopyTo(buffer);
            encoder.Convert(packet.AsSpan(position), buffer.AsSpan(pendingBytes), flush: true,
                out int charsUsed, out int bytesUsed, out _);
            position += charsUsed;
            int available = pendingBytes + bytesUsed;
            int length = Math.Min(available, MaxChunkBytes);
            pendingBytes = available - length;
            return buffer.AsSpan(0, length);
        }
    }
}
