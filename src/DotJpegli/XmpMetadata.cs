using System.Xml;
using System.Xml.Linq;

namespace DotJpegli;

/// <summary>
/// Edits properties in an ordinary, empty-subject XMP RDF packet. Unknown properties,
/// qualifiers and XML namespaces are retained. Instances are mutable and are not thread-safe.
/// </summary>
/// <remarks>
/// XML formatting can change on serialization. Packets containing an extended-XMP digest
/// link can be inspected, but cannot be edited: combine their metadata and repartition it first.
/// This editor does not fetch or combine extended packets or implement arbitrary RDF graphs.
/// </remarks>
public sealed class XmpMetadata
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Meta = "adobe:ns:meta/";
    private static readonly XName ExtendedLink = XName.Get("HasExtendedXMP", "http://ns.adobe.com/xmp/note/");
    private readonly XDocument document;
    private readonly XElement rdf;
    // The XML tree is owned, and property edits cannot introduce digest links.
    // This remains invariant instead of requiring a whole-tree scan on each edit.
    private readonly bool hasExtendedLink;

    /// <summary>Creates an empty XMP packet with one empty-subject RDF description.</summary>
    public XmpMetadata()
    {
        rdf = new XElement(Rdf + "RDF", new XAttribute(XNamespace.Xmlns + "rdf", Rdf.NamespaceName),
            new XElement(Rdf + "Description", new XAttribute(Rdf + "about", "")));
        document = new XDocument(new XElement(Meta + "xmpmeta",
            new XAttribute(XNamespace.Xmlns + "x", Meta.NamespaceName), rdf));
    }

    private XmpMetadata(XDocument document, XElement rdf, bool hasExtendedLink)
    {
        this.document = document;
        this.rdf = rdf;
        this.hasExtendedLink = hasExtendedLink;
    }

    /// <summary>Parses an XMP XML envelope without resolving external entities or allowing DTDs.</summary>
    /// <exception cref="ArgumentException">The packet is malformed or uses unsupported RDF subjects or envelopes.</exception>
    public static XmpMetadata Parse(string packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        const string message = "XMP requires an rdf:RDF or x:xmpmeta envelope containing only " +
            "top-level rdf:Description elements with empty or absent rdf:about, without rdf:ID/nodeID or DTDs.";
        try
        {
            using var input = new StringReader(packet);
            using var reader = XmlReader.Create(input, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = Math.Max(1, packet.Length)
            });
            XDocument parsed = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            XElement? root = parsed.Root;
            XElement? parsedRdf = root?.Name == Rdf + "RDF" ? root
                : root?.Name == Meta + "xmpmeta" && root.Elements().Count() == 1
                    ? root.Element(Rdf + "RDF") : null;
            if (parsedRdf is null || HasText(root!) || HasText(parsedRdf) || !parsedRdf.Elements().Any() ||
                parsedRdf.Elements().Any(element => element.Name != Rdf + "Description"))
                throw new ArgumentException(message, nameof(packet));
            foreach (XElement description in parsedRdf.Elements())
            {
                if (!string.IsNullOrEmpty((string?)description.Attribute(Rdf + "about")) ||
                    description.Attribute(Rdf + "ID") is not null || description.Attribute(Rdf + "nodeID") is not null ||
                    HasText(description))
                    throw new ArgumentException(message, nameof(packet));
            }
            bool hasExtendedLink = false;
            foreach (XElement element in root!.DescendantsAndSelf())
            {
                hasExtendedLink |= element.Name == ExtendedLink || element.Attribute(ExtendedLink) is not null;
                if (element.Nodes().OfType<XProcessingInstruction>().Any(instruction => instruction.Target == "xpacket"))
                    throw new ArgumentException("XMP packet instructions must be outside the XML root.", nameof(packet));
            }
            return new(parsed, parsedRdf, hasExtendedLink);
        }
        catch (XmlException error)
        {
            throw new ArgumentException(message, nameof(packet), error);
        }
    }

    /// <summary>Serializes the current XML packet, including outer packet instructions.</summary>
    public string ToPacket() => document.ToString(SaveOptions.DisableFormatting);

    /// <summary>Gets the first property as an owned XML copy, or null if absent.</summary>
    /// <remarks>Attribute-form properties are returned as equivalent literal elements. Inherited XML context is copied.</remarks>
    public XElement? GetProperty(XmpPropertyName name) => FindProperty(name, copy: true);

    private XElement? FindProperty(XmpPropertyName name, bool copy)
    {
        XName xmlName = PropertyName(name);
        foreach (XElement description in rdf.Elements())
        {
            if (description.Attribute(xmlName) is { } attribute)
            {
                var literal = new XElement(xmlName, attribute.Value);
                return copy ? CopyContext(description, literal) : literal;
            }
            if (description.Element(xmlName) is { } element)
                return copy ? CopyContext(element, new XElement(element)) : element;
        }
        return null;
    }

    /// <summary>Replaces every attribute or element form of a property with an owned copy of the supplied element.</summary>
    /// <remarks>The element name must match the property name. Structured values and qualifiers are supported.</remarks>
    public void SetProperty(XmpPropertyName name, XElement value)
    {
        ArgumentNullException.ThrowIfNull(value);
        XName xmlName = PropertyName(name);
        if (value.Name != xmlName)
            throw new ArgumentException("The XML element name must match the property name.", nameof(value));
        XElement copy = CopyContext(value, new XElement(value));
        if (HasExtendedLink(copy) ||
            copy.DescendantNodes().OfType<XProcessingInstruction>().Any(instruction => instruction.Target == "xpacket"))
            throw new ArgumentException("Properties cannot introduce extended-XMP links or packet instructions.", nameof(value));
        try
        {
            // XElement permits invalid character data until serialization. Check
            // before replacing existing forms so an invalid edit is atomic.
            _ = copy.ToString(SaveOptions.DisableFormatting);
        }
        catch (ArgumentException error)
        {
            throw new ArgumentException("The property must serialize as well-formed XML.", nameof(value), error);
        }
        EnsureEditable();
        RemoveForms(xmlName);
        rdf.Elements().First().Add(copy);
    }

    /// <summary>Gets a literal simple property, or null if absent.</summary>
    /// <exception cref="InvalidOperationException">The property is a structured or URI value.</exception>
    public string? GetSimpleProperty(XmpPropertyName name)
    {
        XElement? property = FindProperty(name, copy: false);
        if (property is null)
            return null;
        return LiteralValue(property);
    }

    /// <summary>Replaces a property, including all duplicate forms, with a literal string.</summary>
    public void SetSimpleProperty(XmpPropertyName name, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        SetProperty(name, new XElement(PropertyName(name), value));
    }

    /// <summary>Gets the exact language entry of an RDF alternative, or null if absent. Language matching ignores case.</summary>
    /// <remarks>No fallback language is chosen; use the language "x-default" to access the default entry.</remarks>
    public string? GetLocalizedText(XmpPropertyName name, string language)
    {
        ValidateLanguage(language);
        XElement? property = FindProperty(name, copy: false);
        if (property is null)
            return null;
        XElement alternative = Alternative(property);
        XElement? item = alternative.Elements(Rdf + "li").FirstOrDefault(element =>
            string.Equals((string?)element.Attribute(XNamespace.Xml + "lang"), language, StringComparison.OrdinalIgnoreCase));
        return item is null ? null : LiteralValue(item);
    }

    /// <summary>Sets one language entry while preserving other languages and qualifiers; removes duplicate property forms.</summary>
    /// <remarks>Editing a non-alternative property is rejected. The "x-default" entry, when present, is written first.</remarks>
    public void SetLocalizedText(XmpPropertyName name, string language, string value)
    {
        ValidateLanguage(language);
        ArgumentNullException.ThrowIfNull(value);
        XElement property = GetProperty(name) ?? new XElement(PropertyName(name), new XElement(Rdf + "Alt"));
        XElement alternative = Alternative(property);
        XElement[] matching = alternative.Elements(Rdf + "li").Where(element =>
            string.Equals((string?)element.Attribute(XNamespace.Xml + "lang"), language, StringComparison.OrdinalIgnoreCase)).ToArray();
        XElement item = matching.Length == 0 ? new XElement(Rdf + "li") : new XElement(matching[0]);
        if (matching.Length != 0)
            _ = LiteralValue(item);
        item.SetAttributeValue(XNamespace.Xml + "lang", language);
        item.Value = value;
        foreach (XElement duplicate in matching)
            duplicate.Remove();
        if (string.Equals(language, "x-default", StringComparison.OrdinalIgnoreCase))
            alternative.AddFirst(item);
        else
            alternative.Add(item);
        SetProperty(name, property);
    }

    /// <summary>Gets an RDF bag or sequence of literal strings, or an empty list if the property is absent.</summary>
    /// <exception cref="InvalidOperationException">The property is not an array of literal strings.</exception>
    public IReadOnlyList<string> GetStringArray(XmpPropertyName name)
    {
        XElement? property = FindProperty(name, copy: false);
        if (property is null)
            return Array.Empty<string>();
        XElement? container = property.Elements().SingleOrDefault();
        if (container is null || (container.Name != Rdf + "Bag" && container.Name != Rdf + "Seq") ||
            HasText(property) || HasText(container) || container.Elements().Any(element => element.Name != Rdf + "li"))
            throw new InvalidOperationException("The XMP property is not an RDF bag or sequence of strings.");
        return Array.AsReadOnly(container.Elements().Select(LiteralValue).ToArray());
    }

    /// <summary>Replaces all forms of a property with an RDF bag, or an RDF sequence when ordered is true.</summary>
    public void SetStringArray(XmpPropertyName name, IReadOnlyList<string> values, bool ordered = false)
    {
        ArgumentNullException.ThrowIfNull(values);
        var container = new XElement(Rdf + (ordered ? "Seq" : "Bag"));
        foreach (string value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            container.Add(new XElement(Rdf + "li", value));
        }
        SetProperty(name, new XElement(PropertyName(name), container));
    }

    /// <summary>Removes all attribute and element forms of a property from every description.</summary>
    /// <returns>True if a property was removed.</returns>
    public bool Remove(XmpPropertyName name)
    {
        XName xmlName = PropertyName(name);
        EnsureEditable();
        return RemoveForms(xmlName);
    }

    private bool RemoveForms(XName name)
    {
        bool removed = false;
        foreach (XElement description in rdf.Elements())
        {
            if (description.Attribute(name) is { } attribute)
            {
                attribute.Remove();
                removed = true;
            }
            foreach (XElement element in description.Elements(name).ToArray())
            {
                element.Remove();
                removed = true;
            }
        }
        return removed;
    }

    private void EnsureEditable()
    {
        if (hasExtendedLink)
            throw new InvalidOperationException("Combine standard and extended XMP, remove the digest link, and repartition before editing.");
    }

    private static bool HasExtendedLink(XElement root) => root.DescendantsAndSelf()
        .Any(element => element.Name == ExtendedLink || element.Attribute(ExtendedLink) is not null);

    private static XName PropertyName(XmpPropertyName name)
    {
        try
        {
            if (string.IsNullOrEmpty(name.NamespaceUri) || string.IsNullOrEmpty(name.LocalName))
                throw new XmlException("A property requires a namespace URI and local name.");
            XmlConvert.VerifyNCName(name.LocalName);
            XmlConvert.VerifyXmlChars(name.NamespaceUri);
            XName xmlName = XName.Get(name.LocalName, name.NamespaceUri);
            if (xmlName.Namespace == Rdf || xmlName.Namespace == Meta || xmlName.Namespace == XNamespace.Xml ||
                xmlName.Namespace == XNamespace.Xmlns || xmlName == ExtendedLink)
                throw new XmlException("Envelope namespaces and extended-XMP links cannot be edited as properties.");
            return xmlName;
        }
        catch (XmlException error)
        {
            throw new ArgumentException("An XMP property requires a nonempty namespace URI and XML NCName, excluding envelope and digest-link names.", nameof(name), error);
        }
    }

    private static XElement CopyContext(XElement source, XElement copy)
    {
        // QName-valued attributes/text can depend on ancestor declarations. Carry
        // those bindings and inherited XML context into a detached owned copy.
        foreach (XElement ancestor in source.AncestorsAndSelf())
            foreach (XAttribute attribute in ancestor.Attributes())
                if ((attribute.IsNamespaceDeclaration || attribute.Name.Namespace == XNamespace.Xml) && copy.Attribute(attribute.Name) is null)
                    copy.Add(new XAttribute(attribute));
        return copy;
    }

    private static string LiteralValue(XElement element)
    {
        if (element.HasElements || element.Attribute(Rdf + "resource") is not null ||
            element.Attribute(Rdf + "parseType") is not null)
            throw new InvalidOperationException("The XMP value is not a literal string.");
        return element.Value;
    }

    private static XElement Alternative(XElement property)
    {
        XElement[] children = property.Elements().ToArray();
        if (children.Length != 1 || children[0].Name != Rdf + "Alt" || HasText(property) || HasText(children[0]) ||
            children[0].Elements().Any(element => element.Name != Rdf + "li" || element.Attribute(XNamespace.Xml + "lang") is null))
            throw new InvalidOperationException("The XMP property is not an RDF language alternative.");
        return children[0];
    }

    private static void ValidateLanguage(string language)
    {
        ArgumentException.ThrowIfNullOrEmpty(language);
        // Language tags consist of ASCII alphanumeric subtags separated by '-'.
        // This also allows XMP's special x-default language without a culture lookup.
        ReadOnlySpan<char> tag = language;
        foreach (Range range in tag.Split('-'))
        {
            ReadOnlySpan<char> part = tag[range];
            if (part.IsEmpty)
                throw new ArgumentException("The language must be a hyphen-separated ASCII language tag.", nameof(language));
            foreach (char character in part)
                if (!char.IsAsciiLetterOrDigit(character))
                    throw new ArgumentException("The language must be a hyphen-separated ASCII language tag.", nameof(language));
        }
    }

    private static bool HasText(XElement element) =>
        element.Nodes().OfType<XText>().Any(text => !string.IsNullOrWhiteSpace(text.Value));
}
