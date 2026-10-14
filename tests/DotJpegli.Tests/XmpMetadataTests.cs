using System.Xml.Linq;
using Xunit;

namespace DotJpegli.Tests;

public sealed class XmpMetadataTests
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Test = "urn:test";
    private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
    private static readonly XmpPropertyName Title = new(Test.NamespaceName, "title");
    private static readonly XmpPropertyName Creator = new(Dc.NamespaceName, "creator");
    private static readonly XmpPropertyName Rights = new(Dc.NamespaceName, "rights");

    [Fact]
    public void EmptyEnvelopeAndLiteralEditsHaveIndependentXmlRepresentation()
    {
        var metadata = new XmpMetadata();
        Assert.Null(metadata.GetSimpleProperty(Title));
        metadata.SetSimpleProperty(Title, "A café 日本語 😀 & <title>");
        XElement root = XElement.Parse(metadata.ToPacket());
        XElement description = Assert.Single(root.Descendants(Rdf + "Description"));
        Assert.Equal("", (string?)description.Attribute(Rdf + "about"));
        Assert.Equal("A café 日本語 😀 & <title>", Assert.Single(description.Elements(Test + "title")).Value);
        Assert.Equal("A café 日本語 😀 & <title>", XmpMetadata.Parse(metadata.ToPacket()).GetSimpleProperty(Title));
        Assert.True(metadata.Remove(Title));
        Assert.False(metadata.Remove(Title));
        Assert.Empty(XElement.Parse(metadata.ToPacket()).Descendants(Test + "title"));
    }

    [Fact]
    public void ExplicitEditRemovesDuplicateAttributeAndElementFormsAcrossDescriptions()
    {
        XmpMetadata metadata = XmpMetadata.Parse(Packet(
            "<rdf:Description t:title='attribute'><t:title>first</t:title><t:title>second</t:title><t:other>keep</t:other></rdf:Description>" +
            "<rdf:Description rdf:about=''><t:title>third</t:title></rdf:Description>"));
        Assert.Equal("attribute", metadata.GetSimpleProperty(Title));
        metadata.SetSimpleProperty(Title, "replacement");
        XElement root = XElement.Parse(metadata.ToPacket());
        Assert.Equal("replacement", Assert.Single(root.Descendants(Test + "title")).Value);
        Assert.DoesNotContain(root.Descendants(Rdf + "Description").Attributes(), attribute => attribute.Name == Test + "title");
        Assert.Equal("keep", Assert.Single(root.Descendants(Test + "other")).Value);
        Assert.True(metadata.Remove(Title));
        Assert.Empty(XElement.Parse(metadata.ToPacket()).Descendants(Test + "title"));
    }

    [Fact]
    public void UnknownStructuredValuesNamespacesQualifiersAndOuterInstructionsSurviveUnrelatedEdit()
    {
        const string structure = "<t:unknown rdf:parseType='Resource' xmlns:q='urn:qualifier' q:meaning='t:Thing'>" +
            "<rdf:value xml:lang='fr'>qualifié</rdf:value><q:role>owner</q:role></t:unknown>";
        XmpMetadata metadata = XmpMetadata.Parse("<?xpacket begin='\uFEFF' id='packet'?>" + Packet(
            "<rdf:Description rdf:about='' xmlns:p='urn:prefix'>" + structure + "</rdf:Description>") + "<?xpacket end='w'?>");
        XElement original = new(XDocument.Parse(metadata.ToPacket()).Descendants(Test + "unknown").Single());
        metadata.SetSimpleProperty(Title, "added");
        XDocument document = XDocument.Parse(metadata.ToPacket());
        XElement unknown = Assert.Single(document.Descendants(Test + "unknown"));
        Assert.True(XNode.DeepEquals(original, unknown));
        Assert.Equal("urn:prefix", unknown.Parent!.GetNamespaceOfPrefix("p")?.NamespaceName);
        Assert.Equal(2, document.Nodes().OfType<XProcessingInstruction>().Count());
    }

    [Fact]
    public void StructuredPropertyUsesOwnedCopiesAndCarriesInheritedNamespaceContext()
    {
        XElement parent = XElement.Parse("<wrapper xmlns:t='urn:test' xmlns:q='urn:qualifier' xml:lang='fr'>" +
            "<t:structure q:type='q:Owner'><q:name>original</q:name></t:structure></wrapper>");
        XElement caller = parent.Elements().Single();
        XmpPropertyName name = new(Test.NamespaceName, "structure");
        var metadata = new XmpMetadata();
        metadata.SetProperty(name, caller);
        caller.Element(XName.Get("name", "urn:qualifier"))!.Value = "caller mutation";
        XElement first = metadata.GetProperty(name)!;
        Assert.Equal("original", first.Elements().Single().Value);
        Assert.Equal("urn:qualifier", first.GetNamespaceOfPrefix("q")?.NamespaceName);
        Assert.Equal("fr", (string?)first.Attribute(XNamespace.Xml + "lang"));
        first.Elements().Single().Value = "getter mutation";
        Assert.Equal("original", metadata.GetProperty(name)!.Elements().Single().Value);
        Assert.Same(parent, caller.Parent);
        Assert.Throws<InvalidOperationException>(() => metadata.GetSimpleProperty(name));
    }

    [Fact]
    public void AttributePropertyCopyIncludesNamespacesAndInheritedLanguage()
    {
        XmpMetadata metadata = XmpMetadata.Parse(Packet("<rdf:Description t:title='value' xml:lang='en' xmlns:q='urn:q'/>"));
        XElement property = metadata.GetProperty(Title)!;
        Assert.Equal(Test + "title", property.Name);
        Assert.Equal("value", property.Value);
        Assert.Equal("en", (string?)property.Attribute(XNamespace.Xml + "lang"));
        Assert.Equal("urn:q", property.GetNamespaceOfPrefix("q")?.NamespaceName);
    }

    [Fact]
    public void LocalizedEditsPreserveOtherLanguagesAndQualifiersAndWriteDefaultFirst()
    {
        XmpMetadata metadata = XmpMetadata.Parse(Packet("<rdf:Description xmlns:dc='" + Dc.NamespaceName + "'>" +
            "<dc:rights xmlns:q='urn:q' q:source='keep'><rdf:Alt>" +
            "<rdf:li xml:lang='fr' q:role='keep'>Droits</rdf:li><rdf:li xml:lang='x-default'>Old</rdf:li>" +
            "<rdf:li xml:lang='en'>Rights</rdf:li></rdf:Alt></dc:rights></rdf:Description>"));
        metadata.SetLocalizedText(Rights, "x-default", "Copyright 2026");
        metadata.SetLocalizedText(Rights, "FR", "Nouveaux droits");
        metadata.SetLocalizedText(Rights, "ja-JP", "著作権");
        XElement property = Assert.Single(XElement.Parse(metadata.ToPacket()).Descendants(Dc + "rights"));
        Assert.Equal("keep", (string?)property.Attribute(XName.Get("source", "urn:q")));
        XElement[] items = property.Element(Rdf + "Alt")!.Elements().ToArray();
        Assert.Equal("x-default", (string?)items[0].Attribute(XNamespace.Xml + "lang"));
        Assert.Equal("Copyright 2026", items[0].Value);
        Assert.Equal("Rights", Assert.Single(items, item => (string?)item.Attribute(XNamespace.Xml + "lang") == "en").Value);
        Assert.Equal("keep", (string?)Assert.Single(items, item => (string?)item.Attribute(XNamespace.Xml + "lang") == "FR")
            .Attribute(XName.Get("role", "urn:q")));
        Assert.Equal("Nouveaux droits", metadata.GetLocalizedText(Rights, "fr"));
        Assert.Null(metadata.GetLocalizedText(Rights, "de"));
        Assert.Equal("著作権", metadata.GetLocalizedText(Rights, "ja-jp"));
    }

    [Fact]
    public void LocalizedCreationAndStringArraysUseExplicitRdfContainers()
    {
        var metadata = new XmpMetadata();
        Assert.Null(metadata.GetLocalizedText(Rights, "x-default"));
        metadata.SetLocalizedText(Rights, "x-default", "Default");
        Assert.Equal("Default", Assert.Single(XElement.Parse(metadata.ToPacket()).Descendants(Rdf + "Alt")).Value);
        Assert.Empty(metadata.GetStringArray(Creator));
        metadata.SetStringArray(Creator, ["first", "second", "first"], ordered: true);
        XElement sequence = Assert.Single(XElement.Parse(metadata.ToPacket()).Descendants(Rdf + "Seq"));
        Assert.Equal(new[] { "first", "second", "first" }, sequence.Elements(Rdf + "li").Select(item => item.Value));
        Assert.Equal(new[] { "first", "second", "first" }, metadata.GetStringArray(Creator));
        metadata.SetStringArray(Creator, [], ordered: false);
        Assert.Empty(Assert.Single(XElement.Parse(metadata.ToPacket()).Descendants(Rdf + "Bag")).Elements());
        Assert.Empty(metadata.GetStringArray(Creator));
    }

    [Fact]
    public void UnexpectedPropertyShapesRejectConvenienceAccessWithoutMutation()
    {
        var metadata = new XmpMetadata();
        metadata.SetSimpleProperty(Rights, "literal");
        string before = metadata.ToPacket();
        Assert.Throws<InvalidOperationException>(() => metadata.GetLocalizedText(Rights, "en"));
        Assert.Throws<InvalidOperationException>(() => metadata.SetLocalizedText(Rights, "en", "new"));
        Assert.Throws<InvalidOperationException>(() => metadata.GetStringArray(Rights));
        Assert.Equal(before, metadata.ToPacket());
        metadata.SetProperty(Title, new XElement(Test + "title", new XAttribute(Rdf + "resource", "urn:value")));
        Assert.Throws<InvalidOperationException>(() => metadata.GetSimpleProperty(Title));
    }

    [Theory]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/>")]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description rdf:about='urn:other'/></rdf:RDF>")]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description rdf:ID='other'/></rdf:RDF>")]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description rdf:nodeID='other'/></rdf:RDF>")]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Bag/></rdf:RDF>")]
    [InlineData("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description>unexpected text</rdf:Description></rdf:RDF>")]
    [InlineData("<x:xmpmeta xmlns:x='adobe:ns:meta/'><extra/></x:xmpmeta>")]
    [InlineData("<!DOCTYPE root [<!ENTITY secret SYSTEM 'file:///C:/secret'>]><root>&secret;</root>")]
    [InlineData("not XML")]
    public void MalformedOrUnsupportedEnvelopeIsRejected(string packet) =>
        Assert.Throws<ArgumentException>(() => XmpMetadata.Parse(packet));

    [Fact]
    public void PacketInstructionsInsidePropertyAreRejected()
    {
        Assert.Throws<ArgumentException>(() => XmpMetadata.Parse(Packet("<rdf:Description><t:title><?xpacket end='w'?></t:title></rdf:Description>")));
        var metadata = new XmpMetadata();
        Assert.Throws<ArgumentException>(() => metadata.SetProperty(Title, new XElement(Test + "title", new XProcessingInstruction("xpacket", "end='w'"))));
    }

    [Theory]
    [InlineData("<rdf:Description xmlns:n='http://ns.adobe.com/xmp/note/' n:HasExtendedXMP='ABC'><t:title>readable</t:title></rdf:Description>")]
    [InlineData("<rdf:Description xmlns:n='http://ns.adobe.com/xmp/note/'><n:HasExtendedXMP>ABC</n:HasExtendedXMP><t:title>readable</t:title></rdf:Description>")]
    public void DigestLinkedPacketIsReadableButEveryMutationIsRejected(string description)
    {
        XmpMetadata metadata = XmpMetadata.Parse(Packet(description));
        Assert.Equal("readable", metadata.GetSimpleProperty(Title));
        string before = metadata.ToPacket();
        Assert.Throws<InvalidOperationException>(() => metadata.SetSimpleProperty(Title, "new"));
        Assert.Throws<InvalidOperationException>(() => metadata.SetLocalizedText(Rights, "x-default", "new"));
        Assert.Throws<InvalidOperationException>(() => metadata.SetStringArray(Creator, ["new"], true));
        Assert.Throws<InvalidOperationException>(() => metadata.Remove(Title));
        Assert.Equal(before, metadata.ToPacket());
    }

    [Fact]
    public void InvalidEditsRejectBeforeRemovingExistingProperties()
    {
        var metadata = new XmpMetadata();
        metadata.SetSimpleProperty(Title, "original");
        string before = metadata.ToPacket();
        Assert.Throws<ArgumentException>(() => metadata.SetSimpleProperty(default, "new"));
        Assert.Throws<ArgumentException>(() => metadata.SetSimpleProperty(new("urn:test", "bad:name"), "new"));
        Assert.Throws<ArgumentException>(() => metadata.SetSimpleProperty(new(Rdf.NamespaceName, "value"), "new"));
        Assert.Throws<ArgumentException>(() => metadata.SetSimpleProperty(new("http://ns.adobe.com/xmp/note/", "HasExtendedXMP"), "new"));
        Assert.Throws<ArgumentException>(() => metadata.SetProperty(Title, new XElement(Test + "other", "new")));
        Assert.Throws<ArgumentException>(() => metadata.SetSimpleProperty(Title, "invalid\0text"));
        Assert.Throws<ArgumentException>(() => metadata.SetLocalizedText(Rights, "bad_language", "new"));
        Assert.Equal(before, metadata.ToPacket());
    }

    private static string Packet(string descriptions) =>
        "<rdf:RDF xmlns:rdf='" + Rdf.NamespaceName + "' xmlns:t='" + Test.NamespaceName + "'>" + descriptions + "</rdf:RDF>";
}
