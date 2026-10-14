using System.Buffers.Binary;
using System.Text;

namespace DotJpegli;

/// <summary>Edits IPTC Information Interchange Model datasets, preserving repeats and unknown binary fields.</summary>
/// <remarks>
/// Instances own their data, are mutable, and are not thread-safe. Unedited datasets retain their
/// original byte encoding. Text helpers address application record 2 and use strict UTF-8 when
/// designated by dataset 1:90 (ESC % G), or ASCII when no designation is present. Other encodings
/// and ambiguous undesignated non-ASCII text require explicit handling through the raw APIs.
/// Raw APIs leave dataset-specific size, cardinality and semantic restrictions to the caller.
/// Text setters enforce the byte limits, cardinality and character restrictions of By-line (80),
/// Copyright Notice (116), and Caption/Abstract (120); other text datasets retain the raw API's structural scope.
/// </remarks>
public sealed class IptcMetadata
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static ReadOnlySpan<byte> Utf8Designation => [0x1b, 0x25, 0x47];
    private readonly List<Dataset> datasets = [];

    /// <summary>Creates an empty collection of IPTC-IIM datasets.</summary>
    public IptcMetadata() { }

    /// <summary>Parses standard and extended-length IPTC-IIM datasets into owned storage.</summary>
    /// <exception cref="ArgumentException">A tag or length is malformed or truncated.</exception>
    public static IptcMetadata Parse(ReadOnlySpan<byte> data)
    {
        var metadata = new IptcMetadata();
        int offset = 0;
        while (offset < data.Length)
        {
            int start = offset;
            if (data.Length - offset < 5 || data[offset] != 0x1c)
                throw new ArgumentException("Each IPTC dataset requires a complete 0x1c tag and length.", nameof(data));
            byte record = data[offset + 1];
            byte number = data[offset + 2];
            int length = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset + 3, 2));
            offset += 5;
            if ((length & 0x8000) != 0)
            {
                int count = length & 0x7fff;
                if (count == 0 || count > data.Length - offset)
                    throw new ArgumentException("The extended IPTC length descriptor is missing or truncated.", nameof(data));
                length = 0;
                foreach (byte octet in data.Slice(offset, count))
                {
                    if (length > (int.MaxValue - octet) / 256)
                        throw new ArgumentException("The extended IPTC dataset length exceeds supported memory size.", nameof(data));
                    length = length * 256 + octet;
                }
                offset += count;
            }
            if (length > data.Length - offset)
                throw new ArgumentException("The IPTC dataset payload is truncated.", nameof(data));
            byte[] encoded = data.Slice(start, offset - start + length).ToArray();
            metadata.datasets.Add(new(record, number, encoded, encoded.AsMemory(offset - start, length)));
            offset += length;
        }
        return metadata;
    }

    /// <summary>Serializes all datasets, retaining the original encoding of unedited entries.</summary>
    public byte[] ToArray()
    {
        int length = 0;
        foreach (Dataset dataset in datasets)
            length = checked(length + dataset.Encoded.Length);
        byte[] result = new byte[length];
        int offset = 0;
        foreach (Dataset dataset in datasets)
        {
            dataset.Encoded.CopyTo(result, offset);
            offset += dataset.Encoded.Length;
        }
        return result;
    }

    /// <summary>Gets owned snapshots of all matching dataset payloads in their existing order.</summary>
    public IReadOnlyList<ReadOnlyMemory<byte>> GetValues(byte record, byte dataset) =>
        Array.AsReadOnly(datasets.Where(entry => entry.Record == record && entry.Number == dataset)
            .Select(entry => (ReadOnlyMemory<byte>)entry.Value.ToArray()).ToArray());

    /// <summary>Replaces every matching dataset with owned copies of the supplied values. An empty list removes them.</summary>
    /// <remarks>New fields are inserted before higher-numbered records; replacements keep the first old occurrence's position.</remarks>
    public void SetValues(byte record, byte dataset, IReadOnlyList<ReadOnlyMemory<byte>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        Dataset[] replacements = values.Select(value => CreateDataset(record, dataset, value.Span)).ToArray();
        int index = datasets.FindIndex(entry => entry.Record == record && entry.Number == dataset);
        if (index < 0)
        {
            index = datasets.FindIndex(entry => entry.Record > record);
            if (index < 0)
                index = datasets.Count;
        }
        datasets.RemoveAll(entry => entry.Record == record && entry.Number == dataset);
        datasets.InsertRange(index, replacements);
    }

    /// <summary>Gets all text values of an application-record-2 dataset, using strict UTF-8 or the undesignated ASCII default.</summary>
    /// <exception cref="InvalidOperationException">The character-set designation is unsupported or ambiguous, or text is invalid.</exception>
    public IReadOnlyList<string> GetTextValues(byte dataset)
    {
        EnsureTextDataset(dataset);
        bool utf8 = GetTextEncoding();
        return Array.AsReadOnly(datasets.Where(entry => entry.Record == 2 && entry.Number == dataset)
            .Select(entry => DecodeText(entry.Value.Span, utf8)).ToArray());
    }

    /// <summary>Replaces all text values of an application-record-2 dataset and establishes the strict UTF-8 designation ESC % G.</summary>
    /// <remarks>
    /// Before adding the designation, retained application records 2–6 must contain unambiguous ASCII
    /// character data. Known binary record-2 datasets are excluded. Unknown application datasets are
    /// checked conservatively; object record 8 prevents adding a new designation. Existing UTF-8 text
    /// is validated before editing. Missing application RecordVersion is set to version 4.
    /// An empty list removes the selected dataset without decoding text or adding a designation.
    /// Raw APIs remain available for legacy encodings and unknown binary data.
    /// </remarks>
    public void SetTextValues(byte dataset, IReadOnlyList<string> values)
    {
        EnsureTextDataset(dataset);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            SetValues(2, dataset, []);
            return;
        }
        if (dataset is 116 or 120 && values.Count > 1)
            throw new ArgumentException("This IPTC dataset is not repeatable.", nameof(values));
        int maximum = dataset switch { 80 => 32, 116 => 128, 120 => 2000, _ => int.MaxValue };
        var encoded = new ReadOnlyMemory<byte>[values.Count];
        for (int index = 0; index < values.Count; index++)
        {
            ArgumentNullException.ThrowIfNull(values[index]);
            try
            {
                encoded[index] = Utf8.GetBytes(values[index]);
            }
            catch (EncoderFallbackException error)
            {
                throw new ArgumentException("IPTC text must contain well-formed Unicode.", nameof(values), error);
            }
            if (encoded[index].Length > maximum)
                throw new ArgumentException($"IPTC dataset 2:{dataset} text must not exceed {maximum} UTF-8 bytes.", nameof(values));
            _ = DecodeText(encoded[index].Span, true);
            if (dataset is 80 or 116 or 120 && values[index].Any(character => char.IsControl(character) &&
                !(dataset == 120 && character is '\r' or '\n')))
                throw new ArgumentException("This IPTC text field permits graphic characters and spaces; " +
                    "only Caption-Abstract also permits CR/LF.", nameof(values));
        }
        bool utf8 = GetTextEncoding();
        foreach (Dataset entry in datasets)
            if (entry.Record is >= 2 and <= 6 && !(entry.Record == 2 &&
                (IsBinaryDataset(entry.Number) || entry.Number == dataset)))
                _ = DecodeText(entry.Value.Span, utf8);
        if (!utf8 && values.Count != 0 && datasets.Any(entry => entry.Record == 8))
            throw new InvalidOperationException("Adding a character-set designation could relabel existing object-record data.");
        // All copying and validation precede mutation so failed text edits are atomic.
        if (!utf8 && values.Count != 0)
            SetValues(1, 90, [(ReadOnlyMemory<byte>)Utf8Designation.ToArray()]);
        if (!datasets.Any(entry => entry.Record == 2 && entry.Number == 0))
            SetValues(2, 0, [(ReadOnlyMemory<byte>)new byte[] { 0, 4 }]);
        SetValues(2, dataset, encoded);
    }

    private bool GetTextEncoding()
    {
        Dataset[] designations = datasets.Where(entry => entry.Record == 1 && entry.Number == 90).ToArray();
        if (designations.Length == 0)
            return false;
        if (designations.Length != 1 || !designations[0].Value.Span.SequenceEqual(Utf8Designation))
            throw new InvalidOperationException("IPTC text requires one ESC % G UTF-8 designation, or no designation for ASCII.");
        return true;
    }

    private static string DecodeText(ReadOnlySpan<byte> value, bool utf8)
    {
        // Escape and shift control functions can change legacy character sets,
        // so even ASCII bytes containing them cannot safely be relabeled UTF-8.
        if (value.Contains((byte)0x1b) || value.Contains((byte)0x0e) || value.Contains((byte)0x0f))
            throw new InvalidOperationException("IPTC text containing character-set escape or shift functions is unsupported.");
        if (!utf8)
        {
            foreach (byte octet in value)
                if (octet >= 0x80)
                    throw new InvalidOperationException("Undesignated non-ASCII IPTC text has an ambiguous legacy encoding.");
            return Encoding.ASCII.GetString(value);
        }
        try
        {
            return Utf8.GetString(value);
        }
        catch (DecoderFallbackException error)
        {
            throw new InvalidOperationException("IPTC text does not match its UTF-8 designation.", error);
        }
    }

    private static void EnsureTextDataset(byte dataset)
    {
        if (IsBinaryDataset(dataset))
            throw new ArgumentException("This application-record-2 dataset is binary; use the raw value API.", nameof(dataset));
    }

    private static bool IsBinaryDataset(byte dataset) => dataset is 0 or 125 or 200 or 201 or 202;

    private static Dataset CreateDataset(byte record, byte number, ReadOnlySpan<byte> value)
    {
        int header = value.Length <= 0x7fff ? 5 : 9;
        byte[] encoded = new byte[checked(header + value.Length)];
        encoded[0] = 0x1c;
        encoded[1] = record;
        encoded[2] = number;
        if (header == 5)
            BinaryPrimitives.WriteUInt16BigEndian(encoded.AsSpan(3, 2), (ushort)value.Length);
        else
        {
            BinaryPrimitives.WriteUInt16BigEndian(encoded.AsSpan(3, 2), 0x8004);
            BinaryPrimitives.WriteInt32BigEndian(encoded.AsSpan(5, 4), value.Length);
        }
        value.CopyTo(encoded.AsSpan(header));
        return new(record, number, encoded, encoded.AsMemory(header));
    }

    private sealed record Dataset(byte Record, byte Number, byte[] Encoded, ReadOnlyMemory<byte> Value);
}
