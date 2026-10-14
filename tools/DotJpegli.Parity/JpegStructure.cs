using System.Text;
using System.Text.Json;

namespace DotJpegli.Parity;

internal sealed record FrameComponent(int Id, int HorizontalSampling, int VerticalSampling, int QuantizationTable);
internal sealed record ScanStructure(int[] Components, int SpectralStart, int SpectralEnd, int SuccessiveHigh, int SuccessiveLow);
internal sealed record JpegStructure(int FrameMarker, int Precision, int Width, int Height, FrameComponent[] Components,
    SortedDictionary<int, int[]> QuantizationTables, ScanStructure[] Scans, string? Jfif, int[] OtherApplicationMarkers)
{
    public static JpegStructure Read(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0xff || bytes[1] != 0xd8) throw new InvalidDataException("Missing JPEG SOI.");
        int position = 2, frameMarker = 0, precision = 0, width = 0, height = 0;
        FrameComponent[]? components = null;
        var tables = new SortedDictionary<int, int[]>();
        var scans = new List<ScanStructure>();
        var otherApplicationMarkers = new List<int>();
        string? jfif = null;
        bool ended = false;
        while (position < bytes.Length)
        {
            if (bytes[position++] != 0xff) throw new InvalidDataException("Expected JPEG marker prefix.");
            while (position < bytes.Length && bytes[position] == 0xff) position++;
            if (position >= bytes.Length) throw new InvalidDataException("Truncated JPEG marker.");
            int marker = bytes[position++];
            if (marker == 0xd9) { ended = true; break; }
            if (marker is 0xd8 or 0x00 || marker is >= 0xd0 and <= 0xd7) throw new InvalidDataException("Unexpected standalone JPEG marker.");
            if (position + 2 > bytes.Length) throw new InvalidDataException("Truncated JPEG segment length.");
            int length = (bytes[position] << 8) | bytes[position + 1];
            if (length < 2 || length > bytes.Length - position) throw new InvalidDataException("Invalid JPEG segment length.");
            ReadOnlySpan<byte> data = bytes.AsSpan(position + 2, length - 2);
            position += length;
            if (marker is 0xc0 or 0xc1 or 0xc2)
            {
                if (components is not null || data.Length < 6 || data.Length != 6 + data[5] * 3)
                    throw new InvalidDataException("Invalid JPEG frame.");
                frameMarker = marker; precision = data[0]; height = (data[1] << 8) | data[2]; width = (data[3] << 8) | data[4];
                components = new FrameComponent[data[5]];
                for (int index = 0; index < components.Length; index++)
                {
                    int offset = 6 + index * 3;
                    components[index] = new(data[offset], data[offset + 1] >> 4, data[offset + 1] & 15, data[offset + 2]);
                }
            }
            else if (marker == 0xdb)
            {
                int offset = 0;
                while (offset < data.Length)
                {
                    int descriptor = data[offset++], tablePrecision = descriptor >> 4, id = descriptor & 15;
                    if (tablePrecision > 1 || data.Length - offset < 64 * (tablePrecision + 1)) throw new InvalidDataException("Invalid JPEG DQT.");
                    if (tables.ContainsKey(id)) throw new InvalidDataException("Repeated JPEG quantization table is outside the parity configuration.");
                    int[] values = new int[64];
                    for (int index = 0; index < values.Length; index++)
                    {
                        values[index] = tablePrecision == 0 ? data[offset++] : (data[offset++] << 8) | data[offset++];
                        if (values[index] is < 1 or > 255) throw new InvalidDataException("Quantization table violates force-baseline parity settings.");
                    }
                    tables[id] = values;
                }
            }
            else if (marker == 0xda)
            {
                if (data.Length < 1 || data.Length != 1 + data[0] * 2 + 3) throw new InvalidDataException("Invalid JPEG SOS.");
                int[] ids = new int[data[0]];
                for (int index = 0; index < ids.Length; index++) ids[index] = data[1 + index * 2];
                int tail = 1 + ids.Length * 2;
                scans.Add(new(ids, data[tail], data[tail + 1], data[tail + 2] >> 4, data[tail + 2] & 15));
                // Stuffed FF00 and RST markers are entropy data; the next other marker ends the scan.
                while (position < bytes.Length)
                {
                    if (bytes[position] != 0xff) { position++; continue; }
                    int prefix = position++;
                    while (position < bytes.Length && bytes[position] == 0xff) position++;
                    if (position >= bytes.Length) throw new InvalidDataException("Truncated JPEG entropy marker.");
                    int entropyMarker = bytes[position];
                    if (entropyMarker == 0 || entropyMarker is >= 0xd0 and <= 0xd7) { position++; continue; }
                    position = prefix;
                    break;
                }
            }
            else if (marker == 0xe0 && data.Length >= 14 && data[..5].SequenceEqual("JFIF\0"u8))
            {
                if (jfif is not null) throw new InvalidDataException("Duplicate JFIF marker.");
                jfif = Convert.ToHexString(data);
            }
            else if (marker is >= 0xe0 and <= 0xef) otherApplicationMarkers.Add(marker);
        }
        if (!ended || position != bytes.Length || components is null || tables.Count == 0 || scans.Count == 0)
            throw new InvalidDataException("Incomplete JPEG structure or trailing bytes.");
        return new(frameMarker, precision, width, height, components, tables, scans.ToArray(), jfif, otherApplicationMarkers.ToArray());
    }

    public static string? Compare(byte[] managed, byte[] native, PpmImage source, int sampling, int progression)
    {
        JpegStructure a = Read(managed), b = Read(native);
        var differences = new List<string>();
        int expectedFrame = progression == 0 ? 0xc0 : 0xc2;
        foreach (var (encoder, jpeg) in new[] { ("managed", a), ("native", b) })
        {
            if (jpeg.Width != source.Width || jpeg.Height != source.Height || jpeg.Precision != 8 || jpeg.FrameMarker != expectedFrame)
                differences.Add($"{encoder} frame contradicts the requested dimensions/precision/progression");
            FrameComponent[] expected = [new(1, sampling == 420 ? 2 : 1, sampling == 420 ? 2 : 1, 0), new(2, 1, 1, 1), new(3, 1, 1, 2)];
            if (!jpeg.Components.SequenceEqual(expected)) differences.Add($"{encoder} component ids, factors or quantization selectors contradict YCbCr {sampling}");
            if (jpeg.OtherApplicationMarkers.Length != 0) differences.Add($"{encoder} unexpected application metadata");
            if (jpeg.Jfif != "4A46494600010100000100010000") differences.Add($"{encoder} JFIF contradicts version1.01/unit0/densities1/no-thumbnail settings");
        }
        if (JsonSerializer.Serialize(a.QuantizationTables) != JsonSerializer.Serialize(b.QuantizationTables)) differences.Add("quantization table values differ");
        if (JsonSerializer.Serialize(a.Scans) != JsonSerializer.Serialize(b.Scans)) differences.Add("component/spectral/refinement scan scripts differ");
        return differences.Count == 0 ? null : string.Join("; ", differences);
    }
}
