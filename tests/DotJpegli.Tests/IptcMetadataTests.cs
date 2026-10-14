using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace DotJpegli.Tests;

public sealed class IptcMetadataTests
{
    [Fact]
    public void EmptyAndUnknownRepeatedBinaryDatasetsRoundTripExactly()
    {
        Assert.Empty(new IptcMetadata().ToArray());
        Assert.Empty(IptcMetadata.Parse([]).ToArray());
        byte[] input = Join(Field(2, 250, [0, 0x1c, 0xff]), Field(3, 251, []), Field(2, 250, [4, 5]));
        IptcMetadata metadata = IptcMetadata.Parse(input);
        Assert.Equal(input, metadata.ToArray());
        Assert.Equal(new byte[] { 0, 0x1c, 0xff }, metadata.GetValues(2, 250)[0].ToArray());
        Assert.Equal(new byte[] { 4, 5 }, metadata.GetValues(2, 250)[1].ToArray());
        Assert.Single(metadata.GetValues(3, 251));
        Assert.Empty(metadata.GetValues(2, 249));
    }

    [Fact]
    public void ParseSetAndGetOwnDataAndSnapshots()
    {
        byte[] input = Field(2, 250, [1, 2, 3]);
        IptcMetadata metadata = IptcMetadata.Parse(input);
        input[^1] = 99;
        byte[] caller = [7, 8, 9];
        metadata.SetValues(2, 250, [caller.AsMemory(1, 2)]);
        caller[1] = 99;
        ReadOnlyMemory<byte> snapshot = metadata.GetValues(2, 250)[0];
        Assert.Equal(new byte[] { 8, 9 }, snapshot.ToArray());
        Assert.True(MemoryMarshal.TryGetArray(snapshot, out ArraySegment<byte> memory));
        memory.Array![memory.Offset] = 55;
        Assert.Equal(new byte[] { 8, 9 }, metadata.GetValues(2, 250)[0].ToArray());
        byte[] serialized = metadata.ToArray();
        serialized[^1] = 44;
        Assert.Equal(new byte[] { 8, 9 }, metadata.GetValues(2, 250)[0].ToArray());
    }

    [Fact]
    public void ReplacementAndDeletionPreserveUnknownFieldsAndRepeatOrder()
    {
        byte[] keepFirst = Field(1, 7, [42]);
        byte[] keepMiddle = Field(2, 251, [43]);
        byte[] keepLast = Field(3, 9, [44]);
        IptcMetadata metadata = IptcMetadata.Parse(Join(keepFirst, Field(2, 250, [1]), keepMiddle, Field(2, 250, [2]), keepLast));
        metadata.SetValues(2, 250, [new byte[] { 10 }, new byte[] { 11 }, ReadOnlyMemory<byte>.Empty]);
        Assert.Equal(Join(keepFirst, Field(2, 250, [10]), Field(2, 250, [11]), Field(2, 250, []), keepMiddle, keepLast), metadata.ToArray());
        metadata.SetValues(2, 250, []);
        Assert.Equal(Join(keepFirst, keepMiddle, keepLast), metadata.ToArray());
    }

    [Theory]
    [InlineData(32767)]
    [InlineData(32768)]
    [InlineData(65536)]
    public void NewDatasetLengthIsStandardOrExtendedAndParsedPayloadIsExact(int length)
    {
        byte[] payload = Enumerable.Range(0, length).Select(index => (byte)(index % 251)).ToArray();
        var metadata = new IptcMetadata();
        metadata.SetValues(2, 250, [payload]);
        byte[] bytes = metadata.ToArray();
        Assert.Equal(0x1c, bytes[0]);
        Assert.Equal(2, bytes[1]);
        Assert.Equal(250, bytes[2]);
        if (length <= 32767)
        {
            Assert.Equal(length >> 8, bytes[3]);
            Assert.Equal(length & 255, bytes[4]);
            Assert.Equal(payload, bytes[5..]);
        }
        else
        {
            Assert.Equal(new byte[] { 0x80, 4, (byte)(length >> 24), (byte)(length >> 16), (byte)(length >> 8), (byte)length }, bytes[3..9]);
            Assert.Equal(payload, bytes[9..]);
        }
        Assert.Equal(payload, Assert.Single(IptcMetadata.Parse(bytes).GetValues(2, 250)).ToArray());
    }

    [Fact]
    public void ExtendedLengthDescriptorsAndUneditedNonminimalEncodingsArePreserved()
    {
        byte[] original = [0x1c, 2, 250, 0x80, 6, 0, 0, 0, 0, 0, 3, 1, 2, 3];
        IptcMetadata metadata = IptcMetadata.Parse(original);
        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.Single(metadata.GetValues(2, 250)).ToArray());
        metadata.SetValues(2, 251, [new byte[] { 5 }]);
        Assert.Equal(Join(original, Field(2, 251, [5])), metadata.ToArray());
    }

    [Theory]
    [InlineData("00")]
    [InlineData("1C0250")]
    [InlineData("1C02500002AA")]
    [InlineData("1C02508000")]
    [InlineData("1C02508002AA")]
    [InlineData("1C02508004FFFFFFFF")]
    [InlineData("1C0250800102AA")]
    [InlineData("1C025000010000")]
    public void MalformedLengthsMarkersAndTrailingDataAreRejected(string hex) =>
        Assert.Throws<ArgumentException>(() => IptcMetadata.Parse(Convert.FromHexString(hex)));

    [Fact]
    public void TextEditsEstablishUtf8BeforeApplicationRecordAndPreserveAsciiFieldsAndBinaryPreview()
    {
        byte[] binary = Field(2, 202, [0xff, 0x1b, 0x0e]);
        IptcMetadata metadata = IptcMetadata.Parse(Join(Field(2, 5, "ASCII title"u8), binary, Field(2, 80, "Old author"u8)));
        Assert.Equal(new[] { "Old author" }, metadata.GetTextValues(80));
        metadata.SetTextValues(80, ["Café", "日本語"]);
        Assert.Equal(Join(Field(1, 90, [0x1b, 0x25, 0x47]), Field(2, 5, "ASCII title"u8), binary,
            Field(2, 80, Encoding.UTF8.GetBytes("Café")), Field(2, 80, Encoding.UTF8.GetBytes("日本語")),
            Field(2, 0, [0, 4])), metadata.ToArray());
        Assert.Equal(new[] { "Café", "日本語" }, IptcMetadata.Parse(metadata.ToArray()).GetTextValues(80));
        Assert.Equal(new[] { "ASCII title" }, metadata.GetTextValues(5));
        Assert.Equal(new byte[] { 0xff, 0x1b, 0x0e }, Assert.Single(metadata.GetValues(2, 202)).ToArray());
    }

    [Fact]
    public void TextRemovalUsesEmptyValuesWithoutAddingDesignation()
    {
        IptcMetadata metadata = IptcMetadata.Parse(Join(Field(2, 80, "First"u8), Field(2, 80, "Second"u8), Field(2, 5, "Title"u8)));
        metadata.SetTextValues(80, []);
        Assert.Equal(Field(2, 5, "Title"u8), metadata.ToArray());
        Assert.Empty(metadata.GetTextValues(80));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TextDeletionDoesNotDecodeOrRelabelLegacyData(bool duplicateDesignation)
    {
        byte[] designation = duplicateDesignation
            ? Join(Field(1, 90, [0x1b, 0x25, 0x47]), Field(1, 90, [0x1b, 0x28, 0x42])) : [];
        byte[] unrelated = Field(2, 5, [0xe9]);
        IptcMetadata metadata = IptcMetadata.Parse(Join(designation, Field(2, 80, [0xff]), unrelated));
        metadata.SetTextValues(80, []);
        Assert.Equal(Join(designation, unrelated), metadata.ToArray());
    }

    [Fact]
    public void ReplacingLegacyTargetChecksOnlyRetainedTextAndPreservesExistingVersion()
    {
        byte[] version = Field(2, 0, [0, 2]);
        IptcMetadata metadata = IptcMetadata.Parse(Join(version, Field(2, 80, [0xe9]), Field(2, 5, "Title"u8)));
        metadata.SetTextValues(80, ["Replacement"]);
        Assert.Equal(new byte[] { 0, 2 }, Assert.Single(metadata.GetValues(2, 0)).ToArray());
        Assert.Equal(new[] { "Replacement" }, metadata.GetTextValues(80));
        Assert.Equal(new[] { "Title" }, metadata.GetTextValues(5));
    }

    [Theory]
    [InlineData(80, "A\nB")]
    [InlineData(116, "A\rB")]
    [InlineData(80, "A\tB")]
    [InlineData(120, "A\0B")]
    [InlineData(120, "A\tB")]
    [InlineData(116, "A\u0085B")]
    public void CoreTextControlsFailBeforeMutation(byte dataset, string value)
    {
        var metadata = new IptcMetadata();
        Assert.Throws<ArgumentException>(() => metadata.SetTextValues(dataset, [value]));
        Assert.Empty(metadata.ToArray());
    }

    [Fact]
    public void CaptionLineBreaksAreValidAndNewTextIncludesMandatoryRecordVersion()
    {
        var metadata = new IptcMetadata();
        metadata.SetTextValues(120, ["First\r\nSecond"]);
        Assert.Equal(new byte[] { 0, 4 }, Assert.Single(metadata.GetValues(2, 0)).ToArray());
        Assert.Equal(new[] { "First\r\nSecond" }, metadata.GetTextValues(120));
    }

    [Theory]
    [InlineData("1B2842")]
    [InlineData("1B25471B2842")]
    [InlineData("")]
    public void UnsupportedCharacterDesignationRejectsTextReadAndEditAtomically(string hex)
    {
        byte[] original = Join(Field(1, 90, Convert.FromHexString(hex)), Field(2, 120, "ASCII"u8));
        IptcMetadata metadata = IptcMetadata.Parse(original);
        Assert.Throws<InvalidOperationException>(() => metadata.GetTextValues(120));
        Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(120, ["new"]));
        Assert.Equal(original, metadata.ToArray());
    }

    [Fact]
    public void DuplicateDesignationIsRejectedEvenWhenBothClaimUtf8()
    {
        byte[] original = Join(Field(1, 90, [0x1b, 0x25, 0x47]), Field(1, 90, [0x1b, 0x25, 0x47]), Field(2, 120, "ASCII"u8));
        IptcMetadata metadata = IptcMetadata.Parse(original);
        Assert.Throws<InvalidOperationException>(() => metadata.GetTextValues(120));
        Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(120, ["new"]));
        Assert.Equal(original, metadata.ToArray());
    }

    [Theory]
    [InlineData("E9")]
    [InlineData("C3A9")]
    [InlineData("1B284241")]
    [InlineData("0E41")]
    [InlineData("0F41")]
    public void UndesignatedLegacyOrEscapedTextCannotBeRelabeledByUnrelatedEdit(string hex)
    {
        byte[] original = Field(2, 5, Convert.FromHexString(hex));
        IptcMetadata metadata = IptcMetadata.Parse(original);
        Assert.Throws<InvalidOperationException>(() => metadata.GetTextValues(5));
        Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(80, ["new"]));
        Assert.Equal(original, metadata.ToArray());
    }

    [Theory]
    [InlineData("FF")]
    [InlineData("C0AF")]
    [InlineData("EDA080")]
    [InlineData("F4908080")]
    [InlineData("C3")]
    public void InvalidUtf8TextRejectsReadAndUnrelatedEditWithoutChangingBytes(string hex)
    {
        byte[] original = Join(Field(1, 90, [0x1b, 0x25, 0x47]), Field(2, 5, Convert.FromHexString(hex)));
        IptcMetadata metadata = IptcMetadata.Parse(original);
        Assert.Throws<InvalidOperationException>(() => metadata.GetTextValues(5));
        Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(80, ["new"]));
        Assert.Equal(original, metadata.ToArray());
    }

    [Fact]
    public void ApplicationRecordAndObjectDataCannotBeSilentlyRelabeled()
    {
        foreach (byte record in new byte[] { 3, 4, 5, 6, 8 })
        {
            byte[] original = Field(record, 250, [0xff]);
            IptcMetadata metadata = IptcMetadata.Parse(original);
            Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(80, ["new"]));
            Assert.Equal(original, metadata.ToArray());
        }
    }

    [Theory]
    [InlineData(80, 32)]
    [InlineData(116, 128)]
    [InlineData(120, 2000)]
    public void CoreTextLimitsCountUtf8BytesAndRejectOverflowAtomically(byte dataset, int maximum)
    {
        var metadata = new IptcMetadata();
        metadata.SetTextValues(dataset, [new string('é', maximum / 2)]);
        Assert.Equal(maximum, Assert.Single(metadata.GetValues(2, dataset)).Length);
        string[] oversized = [new string('é', maximum / 2) + "a"];
        byte[] before = metadata.ToArray();
        Assert.Throws<ArgumentException>(() => metadata.SetTextValues(dataset, oversized));
        Assert.Equal(before, metadata.ToArray());
        var empty = new IptcMetadata();
        Assert.Throws<ArgumentException>(() => empty.SetTextValues(dataset, oversized));
        Assert.Empty(empty.ToArray());
    }

    [Theory]
    [InlineData(116)]
    [InlineData(120)]
    public void SingletonTextDatasetsRejectRepeatsBeforeAddingDesignation(byte dataset)
    {
        var metadata = new IptcMetadata();
        Assert.Throws<ArgumentException>(() => metadata.SetTextValues(dataset, ["one", "two"]));
        Assert.Empty(metadata.ToArray());
    }

    [Fact]
    public void InvalidUnicodeAndCharacterSetControlFunctionsRejectWithoutMutation()
    {
        var metadata = new IptcMetadata();
        metadata.SetTextValues(80, ["Original"]);
        byte[] before = metadata.ToArray();
        Assert.Throws<ArgumentException>(() => metadata.SetTextValues(80, ["\ud800"]));
        Assert.Throws<InvalidOperationException>(() => metadata.SetTextValues(80, ["\u001b%G"]));
        Assert.Equal(before, metadata.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(125)]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(202)]
    public void KnownBinaryDatasetsRequireRawApi(byte dataset)
    {
        var metadata = new IptcMetadata();
        Assert.Throws<ArgumentException>(() => metadata.GetTextValues(dataset));
        Assert.Throws<ArgumentException>(() => metadata.SetTextValues(dataset, ["text"]));
        metadata.SetValues(2, dataset, [new byte[] { 0xff }]);
        Assert.Equal(new byte[] { 0xff }, Assert.Single(metadata.GetValues(2, dataset)).ToArray());
    }

    private static byte[] Field(byte record, byte dataset, ReadOnlySpan<byte> value)
    {
        Assert.True(value.Length <= 32767);
        byte[] result = new byte[value.Length + 5];
        result[0] = 0x1c;
        result[1] = record;
        result[2] = dataset;
        result[3] = (byte)(value.Length >> 8);
        result[4] = (byte)value.Length;
        value.CopyTo(result.AsSpan(5));
        return result;
    }

    private static byte[] Join(params byte[][] fields) => fields.SelectMany(field => field).ToArray();
}
