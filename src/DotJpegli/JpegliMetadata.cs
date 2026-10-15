using System.Globalization;
using System.Text;
using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>Opt-in reconciliation of structured metadata before encoding.</summary>
public static class JpegliMetadata
{
    private const string TiffNamespace = "http://ns.adobe.com/tiff/1.0/";
    private const string ExifNamespace = "http://ns.adobe.com/exif/1.0/";
    private const string DcNamespace = "http://purl.org/dc/elements/1.1/";

    /// <summary>
    /// Returns an options copy with owned Exif/IPTC payloads and serialized XMP.
    /// Updates image and pixel dimensions, orientation, and physical resolution from
    /// the options' JFIF densities; unspecified units remove stale physical resolution.
    /// Drops the Exif thumbnail, since it may describe different pixels. Optional
    /// descriptive edits synchronize only the supplied fields; null fields and
    /// unrelated metadata are retained. XMP is automatically partitioned if needed.
    /// </summary>
    /// <remarks>
    /// No source pixels or ICC profile are transformed. The returned options must be
    /// used with the same dimensions supplied here. Existing manual extended XMP and
    /// recognized raw Exif/XMP/JFIF/Photoshop markers must be resolved by the caller
    /// first. Unsupported offset-sensitive Exif structures fail explicitly. Raw
    /// embedding APIs retain their behavior when this helper is not called.
    /// </remarks>
    public static JpegliEncoderOptions Reconcile(JpegliEncoderOptions options, int width, int height,
        JpegliMetadataEdits? edits = null, CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(options);
        if (width is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(height));
        edits ??= new();
        if (edits.Orientation is < 1 or > 8)
            throw new ArgumentOutOfRangeException(nameof(edits), "Orientation must be between 1 and 8.");
        if (edits.ExifColorSpace is int colorSpace && colorSpace is not (1 or 65535))
            throw new ArgumentOutOfRangeException(nameof(edits), "Exif ColorSpace must be 1 or 65535.");
        ValidateText(edits.Artist, 32, nameof(edits.Artist));
        ValidateText(edits.Copyright, 128, nameof(edits.Copyright));
        ValidateText(edits.Description, 2000, nameof(edits.Description));
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrEmpty(options.ExtendedXmpPacket))
            throw new ArgumentException("Reconciliation requires a combined XMP tree; merge manual extended XMP first.",
                nameof(options));
        (options with { AutoPartitionXmp = true }).Validate();
        RejectConflictingMarkers(options, cancellationToken);

        ExifMetadata exif = options.ExifData.IsEmpty ? new() : ExifMetadata.Parse(options.ExifData.Span);
        XmpMetadata xmp = string.IsNullOrEmpty(options.XmpPacket) ? new() : XmpMetadata.Parse(options.XmpPacket);
        IptcMetadata iptc = options.IptcData.IsEmpty ? new() : IptcMetadata.Parse(options.IptcData.Span);
        exif.RemoveThumbnail();
        exif.SetUnsignedLong(ExifDirectory.Image, 0x0100, (uint)width);
        exif.SetUnsignedLong(ExifDirectory.Image, 0x0101, (uint)height);
        exif.SetUnsignedLong(ExifDirectory.Photo, 0xa002, (uint)width);
        exif.SetUnsignedLong(ExifDirectory.Photo, 0xa003, (uint)height);
        exif.SetUnsignedShort(ExifDirectory.Image, 0x0112, (ushort)edits.Orientation);
        SetNumber(xmp, TiffNamespace, "ImageWidth", width);
        SetNumber(xmp, TiffNamespace, "ImageLength", height);
        SetNumber(xmp, ExifNamespace, "PixelXDimension", width);
        SetNumber(xmp, ExifNamespace, "PixelYDimension", height);
        SetNumber(xmp, TiffNamespace, "Orientation", edits.Orientation);

        ReconcileResolution(exif, xmp, options);
        if (edits.ExifColorSpace is int chosenColorSpace)
        {
            exif.SetUnsignedShort(ExifDirectory.Photo, 0xa001, (ushort)chosenColorSpace);
            SetNumber(xmp, ExifNamespace, "ColorSpace", chosenColorSpace);
        }
        if (edits.Artist is string artist)
        {
            SetExifText(exif, 0x013b, artist);
            xmp.Remove(new(TiffNamespace, "Artist"));
            var name = new XmpPropertyName(DcNamespace, "creator");
            if (artist.Length == 0)
                xmp.Remove(name);
            else
                xmp.SetStringArray(name, [artist], ordered: true);
            iptc.SetTextValues(80, artist.Length == 0 ? [] : [artist]);
        }
        ReconcileLocalizedText(exif, xmp, iptc, edits.Copyright, 0x8298, "Copyright", "rights", 116);
        ReconcileLocalizedText(exif, xmp, iptc, edits.Description, 0x010e, "ImageDescription", "description", 120);
        cancellationToken.ThrowIfCancellationRequested();

        var result = options with
        {
            ExifData = exif.ToArray(),
            XmpPacket = xmp.ToPacket(),
            AutoPartitionXmp = true,
            IptcData = iptc.ToArray()
        };
        result.Validate();
        // Validate selected standard properties and their final marker size now,
        // so the helper never returns an unusable partition request.
        _ = ExtendedXmpMetadata.Prepare(result, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static void ReconcileResolution(ExifMetadata exif, XmpMetadata xmp, JpegliEncoderOptions options)
    {
        if (options.DensityUnit == JpegDensityUnit.Unspecified)
        {
            exif.Remove(ExifDirectory.Image, 0x011a);
            exif.Remove(ExifDirectory.Image, 0x011b);
            exif.Remove(ExifDirectory.Image, 0x0128);
            xmp.Remove(new(TiffNamespace, "XResolution"));
            xmp.Remove(new(TiffNamespace, "YResolution"));
            xmp.Remove(new(TiffNamespace, "ResolutionUnit"));
            return;
        }
        ushort resolutionUnit = options.DensityUnit == JpegDensityUnit.DotsPerInch ? (ushort)2 : (ushort)3;
        exif.SetUnsignedRational(ExifDirectory.Image, 0x011a, (uint)options.HorizontalDensity, 1);
        exif.SetUnsignedRational(ExifDirectory.Image, 0x011b, (uint)options.VerticalDensity, 1);
        exif.SetUnsignedShort(ExifDirectory.Image, 0x0128, resolutionUnit);
        xmp.SetSimpleProperty(new(TiffNamespace, "XResolution"),
            options.HorizontalDensity.ToString(CultureInfo.InvariantCulture) + "/1");
        xmp.SetSimpleProperty(new(TiffNamespace, "YResolution"),
            options.VerticalDensity.ToString(CultureInfo.InvariantCulture) + "/1");
        SetNumber(xmp, TiffNamespace, "ResolutionUnit", resolutionUnit);
    }

    private static void ReconcileLocalizedText(ExifMetadata exif, XmpMetadata xmp, IptcMetadata iptc,
        string? text, ushort tag, string alias, string property, byte dataset)
    {
        if (text is null)
            return;
        SetExifText(exif, tag, text);
        xmp.Remove(new(TiffNamespace, alias));
        var name = new XmpPropertyName(DcNamespace, property);
        if (text.Length == 0)
            xmp.Remove(name);
        else
            xmp.SetLocalizedText(name, "x-default", text);
        iptc.SetTextValues(dataset, text.Length == 0 ? [] : [text]);
    }

    private static void SetExifText(ExifMetadata exif, ushort tag, string text)
    {
        if (text.Length == 0)
            exif.Remove(ExifDirectory.Image, tag);
        else
            exif.SetAscii(ExifDirectory.Image, tag, text);
    }

    private static void SetNumber(XmpMetadata xmp, string ns, string name, int value) =>
        xmp.SetSimpleProperty(new(ns, name), value.ToString(CultureInfo.InvariantCulture));

    private static void ValidateText(string? text, int maximum, string name)
    {
        if (text is null)
            return;
        if (text.Length > maximum || text.AsSpan().Contains('\0') || !Ascii.IsValid(text))
            throw new ArgumentException($"{name} must be ASCII without NUL and at most {maximum} bytes.", name);
    }

    private static void RejectConflictingMarkers(JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        foreach (JpegMetadataMarker marker in options.PrepareMetadataMarkers(cancellationToken) ?? [])
        {
            ReadOnlySpan<byte> data = marker.Data.Span;
            bool conflict = marker.MarkerCode switch
            {
                0xe0 => data.StartsWith("JFIF\0"u8) || data.StartsWith("JFXX\0"u8),
                0xe1 => data.StartsWith("Exif\0\0"u8) ||
                    data.StartsWith("http://ns.adobe.com/xap/1.0/\0"u8) ||
                    data.StartsWith("http://ns.adobe.com/xmp/extension/\0"u8),
                0xed => data.StartsWith("Photoshop 3.0\0"u8),
                _ => false
            };
            if (conflict)
                throw new ArgumentException("Reconciliation requires recognized raw metadata to be merged into " +
                    "the built-in fields first.", nameof(options.MetadataMarkers));
        }
    }
}
