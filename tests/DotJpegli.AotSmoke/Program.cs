using System.Buffers.Binary;
using System.Text;
using DotJpegli;

const int width = 33, height = 19;
byte[] pixels = new byte[width * height * 3];
for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        int offset = (y * width + x) * 3;
        pixels[offset] = (byte)(x * 7 + y * 3);
        pixels[offset + 1] = (byte)(x * 3 + y * 11);
        pixels[offset + 2] = (byte)(x * 13 + y * 5);
    }

byte[] srgb = JpegliIccProfiles.CreateSrgbProfile();
var exif = new ExifMetadata();
exif.SetAscii(ExifDirectory.Image, 0x010f, "AOT Camera");
var xmp = new XmpMetadata();
var customProperty = new XmpPropertyName("urn:dotjpegli:smoke", "status");
xmp.SetSimpleProperty(customProperty, "preserved");
var iptc = new IptcMetadata();
iptc.SetTextValues(25, ["trimming", "NativeAOT"]);
var options = JpegliMetadata.Reconcile(new()
{
    Quality = 83,
    RestartInterval = 2,
    IccProfile = srgb,
    ExifData = exif.ToArray(),
    XmpPacket = xmp.ToPacket(),
    IptcData = iptc.ToArray(),
    DensityUnit = JpegDensityUnit.DotsPerInch,
    HorizontalDensity = 96,
    VerticalDensity = 96
}, width, height, new()
{
    Artist = "DotJpegli smoke",
    Description = "Trimmed and native metadata",
    ExifColorSpace = 1
});

byte[] baseline = JpegliEncoder.Encode(pixels, width, height, options: options);
List<Marker> baselineMarkers = Inspect(baseline, width, height, 3, progressive: false);
Check(baselineMarkers.Count(marker => marker.Code == 0xda) == 1, "Baseline scan count");
Check(baselineMarkers.Any(marker => marker.Code == 0xdd), "Restart interval marker");
CheckMetadata(baselineMarkers, srgb, customProperty);
using (var output = new MemoryStream())
{
    JpegliEncoder.Encode(output, pixels, width, height, options: options);
    Check(output.CanWrite, "The encoder closed the destination stream");
    Check(output.ToArray().AsSpan().SequenceEqual(baseline), "Array and stream output differ");
}

byte[] progressive = JpegliEncoder.Encode(pixels, width, height, options: new()
{
    Progressive = true,
    ProgressiveRefinementBits = 1,
    MaxDegreeOfParallelism = 2,
    RestartInterval = 2
});
Check(Inspect(progressive, width, height, 3, progressive: true).Count(marker => marker.Code == 0xda) > 1,
    "Progressive scan count");

ushort[] gray = new ushort[width * height];
for (int index = 0; index < gray.Length; index++)
    gray[index] = (ushort)(index * 101);
Inspect(JpegliEncoder.Encode(gray, width, height, PixelComponentLayout.Gray),
    width, height, 1, progressive: false);

float[] linearRgb = new float[pixels.Length];
for (int index = 0; index < linearRgb.Length; index++)
    linearRgb[index] = pixels[index] / 255f;
byte[] converted = JpegliEncoder.Encode(linearRgb, width, height, options: new()
{
    SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(),
    IccProfile = srgb,
    ColorRenderingIntent = IccRenderingIntent.RelativeColorimetric,
    InferCicpTransferFunctionFromIccProfile = true
});
CheckIcc(Inspect(converted, width, height, 3, progressive: false), srgb);
Check(float.IsFinite(JpegliEncoder.QualityToDistance(83)), "Quality conversion");
Console.WriteLine("DotJpegli trimming/NativeAOT smoke passed: baseline, progressive, stream, grayscale, source ICC and metadata.");

static void CheckMetadata(List<Marker> markers, byte[] profile, XmpPropertyName customProperty)
{
    ReadOnlySpan<byte> exifBody = FindPayload(markers, 0xe1, "Exif\0\0"u8).AsSpan(6);
    var exif = ExifMetadata.Parse(exifBody);
    Check(exif.GetUnsignedInteger(ExifDirectory.Image, 0x0100) == width, "Exif image width");
    Check(exif.GetUnsignedInteger(ExifDirectory.Photo, 0xa003) == height, "Exif pixel height");
    Check(exif.GetAscii(ExifDirectory.Image, 0x010f) == "AOT Camera", "Preserved Exif field");
    Check(exif.GetAscii(ExifDirectory.Image, 0x013b) == "DotJpegli smoke", "Reconciled Exif artist");
    exif.SetAscii(ExifDirectory.Image, 0x013b, "Edited");
    Check(ExifMetadata.Parse(exif.ToArray()).GetAscii(ExifDirectory.Image, 0x013b) == "Edited", "Exif edit roundtrip");

    ReadOnlySpan<byte> xmpPrefix = "http://ns.adobe.com/xap/1.0/\0"u8;
    var xmp = XmpMetadata.Parse(Encoding.UTF8.GetString(FindPayload(markers, 0xe1, xmpPrefix).AsSpan(xmpPrefix.Length)));
    Check(xmp.GetSimpleProperty(customProperty) == "preserved", "Preserved XMP property");
    Check(xmp.GetStringArray(new("http://purl.org/dc/elements/1.1/", "creator")).SequenceEqual(["DotJpegli smoke"]),
        "Reconciled XMP creator");
    xmp.SetLocalizedText(new("http://purl.org/dc/elements/1.1/", "description"), "fr", "Image native");
    Check(XmpMetadata.Parse(xmp.ToPacket()).GetLocalizedText(new("http://purl.org/dc/elements/1.1/", "description"), "fr")
        == "Image native", "XMP edit roundtrip");

    ReadOnlySpan<byte> photoshop = FindPayload(markers, 0xed, "Photoshop 3.0\0"u8);
    Check(photoshop.Length >= 26 && photoshop.Slice(14, 8).SequenceEqual("8BIM\u0004\u0004\0\0"u8), "IPTC resource header");
    int iptcLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(photoshop.Slice(22, 4)));
    Check(iptcLength <= photoshop.Length - 26, "IPTC resource length");
    var iptc = IptcMetadata.Parse(photoshop.Slice(26, iptcLength));
    Check(iptc.GetTextValues(25).SequenceEqual(["trimming", "NativeAOT"]), "Preserved IPTC keywords");
    Check(iptc.GetTextValues(80).SequenceEqual(["DotJpegli smoke"]), "Reconciled IPTC by-line");
    iptc.SetTextValues(120, ["Edited caption"]);
    Check(IptcMetadata.Parse(iptc.ToArray()).GetTextValues(120).SequenceEqual(["Edited caption"]), "IPTC edit roundtrip");
    CheckIcc(markers, profile);
}

static void CheckIcc(List<Marker> markers, byte[] profile)
{
    byte[] payload = FindPayload(markers, 0xe2, "ICC_PROFILE\0"u8);
    Check(payload.Length >= 14 && payload[12] == 1 && payload[13] == 1, "ICC chunk sequence");
    Check(payload.AsSpan(14).SequenceEqual(profile), "Embedded ICC profile");
}

static byte[] FindPayload(List<Marker> markers, byte code, ReadOnlySpan<byte> prefix)
{
    byte[]? found = null;
    foreach (Marker marker in markers)
        if (marker.Code == code && marker.Payload.AsSpan().StartsWith(prefix))
        {
            Check(found is null, "Duplicate metadata marker");
            found = marker.Payload;
        }
    return found ?? throw new InvalidOperationException($"Missing JPEG metadata marker 0x{code:x2}.");
}

// Inspect JPEG framing and metadata without depending on a decoder that might itself need trim/AOT support.
static List<Marker> Inspect(byte[] jpeg, int expectedWidth, int expectedHeight, int components, bool progressive)
{
    Check(jpeg.Length > 4 && jpeg[0] == 0xff && jpeg[1] == 0xd8, "JPEG SOI");
    var markers = new List<Marker>();
    int offset = 2;
    while (offset < jpeg.Length)
    {
        Check(jpeg[offset++] == 0xff, "JPEG marker prefix");
        while (offset < jpeg.Length && jpeg[offset] == 0xff)
            offset++;
        Check(offset < jpeg.Length, "Truncated JPEG marker");
        byte code = jpeg[offset++];
        if (code == 0xd9)
        {
            Check(offset == jpeg.Length, "Bytes after JPEG EOI");
            Marker frame = markers.Single(marker => marker.Code is 0xc0 or 0xc2);
            Check(frame.Code == (progressive ? 0xc2 : 0xc0), "JPEG frame type");
            ReadOnlySpan<byte> data = frame.Payload;
            Check(data.Length == 6 + components * 3 && data[0] == 8 && data[5] == components, "JPEG frame components");
            Check(BinaryPrimitives.ReadUInt16BigEndian(data.Slice(1, 2)) == expectedHeight, "JPEG frame height");
            Check(BinaryPrimitives.ReadUInt16BigEndian(data.Slice(3, 2)) == expectedWidth, "JPEG frame width");
            Check(markers.Any(marker => marker.Code == 0xdb) && markers.Any(marker => marker.Code == 0xc4), "JPEG coding tables");
            Check(markers.Any(marker => marker.Code == 0xda), "Missing JPEG scan");
            return markers;
        }
        Check(code != 0 && code != 0xd8 && code is not (>= 0xd0 and <= 0xd7), "Unexpected standalone JPEG marker");
        Check(offset + 2 <= jpeg.Length, "Truncated JPEG segment length");
        int length = BinaryPrimitives.ReadUInt16BigEndian(jpeg.AsSpan(offset, 2));
        Check(length >= 2 && length <= jpeg.Length - offset, "Invalid JPEG segment length");
        markers.Add(new(code, jpeg.AsSpan(offset + 2, length - 2).ToArray()));
        offset += length;
        if (code != 0xda)
            continue;
        int entropyStart = offset;
        while (offset < jpeg.Length)
        {
            if (jpeg[offset] != 0xff)
            {
                offset++;
                continue;
            }
            Check(offset + 1 < jpeg.Length, "Truncated JPEG entropy marker");
            byte next = jpeg[offset + 1];
            if (next == 0 || next is >= 0xd0 and <= 0xd7)
                offset += 2;
            else
                break;
        }
        Check(offset > entropyStart, "Empty JPEG entropy scan");
    }
    throw new InvalidOperationException("Missing JPEG EOI.");
}

static void Check(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

internal sealed record Marker(byte Code, byte[] Payload);
