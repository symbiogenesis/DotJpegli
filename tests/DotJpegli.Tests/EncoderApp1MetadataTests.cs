using System.Security.Cryptography;
using System.Text;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderApp1MetadataTests
{
    private const int MaximumExifBytes = 65_527;
    private const int MaximumXmpBytes = 65_504;
    private const int IccChunkBytes = 65_519;
    private static readonly byte[] ExifSignature = "Exif\0\0"u8.ToArray();
    private static readonly byte[] XmpSignature = "http://ns.adobe.com/xap/1.0/\0"u8.ToArray();

    [Fact]
    public void DefaultAndEmptyApp1OptionsPreserveExistingJpegBytes()
    {
        byte[] reference = Encode(new());
        // The existing one-pixel output captured before metadata support.
        Assert.Equal("A3C09B17325C4F1EDC2FC43141855B604E5AB4ADAD24D3B9E78AE400D3CD3A28",
            Convert.ToHexString(SHA256.HashData(reference)));
        Assert.Equal(reference, JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(reference, Encode(new() { ExifData = ReadOnlyMemory<byte>.Empty, XmpPacket = null }));
        Assert.Equal(reference, Encode(new() { ExifData = ReadOnlyMemory<byte>.Empty, XmpPacket = "" }));
        Assert.DoesNotContain(ReadHeaders(reference), segment => segment.Code == 0xe1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(513)]
    [InlineData(MaximumExifBytes)]
    public void ExifPreservesSlicedOpaqueBytesAndWritesExactlyOneSignature(int length)
    {
        // Deliberately not a TIFF parser fixture: only the raw byte count is validated.
        byte[] backing = OpaqueBytes(length + 11);
        ReadOnlyMemory<byte> exif = backing.AsMemory(5, length);
        byte[] jpeg = Encode(new() { ExifData = exif });
        List<Segment> segments = ReadHeaders(jpeg);
        Segment app1 = Assert.Single(segments, segment => segment.Code == 0xe1);
        Assert.Equal(0xe0, segments[0].Code);
        Assert.Equal(app1, segments[1]);
        Assert.Equal(0xdb, segments[2].Code);
        Assert.Equal(length + 8, app1.Length);
        Assert.True(app1.Payload.Span[..ExifSignature.Length].SequenceEqual(ExifSignature));
        Assert.True(app1.Payload.Span[ExifSignature.Length..].SequenceEqual(exif.Span));
        Assert.Equal(length + ExifSignature.Length, app1.Payload.Length);
        if (length == MaximumExifBytes)
            Assert.Equal(65_535, app1.Length);
    }

    [Fact]
    public void OversizedExifIsRejectedBeforeWritingAnyOutput()
    {
        AssertLengthRejected(new() { ExifData = new byte[MaximumExifBytes + 1] },
            nameof(JpegliEncoderOptions.ExifData));
    }

    [Theory]
    [InlineData("mixed")]
    [InlineData("ASCII limit")]
    [InlineData("two-byte limit")]
    [InlineData("four-byte limit")]
    public void XmpWritesExactStandardSignatureAndUtf8WithoutAddedBomOrTerminator(string kind)
    {
        string packet = Xmp(kind);
        byte[] expected = Encoding.UTF8.GetBytes(packet);
        byte[] jpeg = Encode(new() { XmpPacket = packet });
        List<Segment> segments = ReadHeaders(jpeg);
        Segment app1 = Assert.Single(segments, segment => segment.Code == 0xe1);
        Assert.Equal(0xe0, segments[0].Code);
        Assert.Equal(app1, segments[1]);
        Assert.Equal(0xdb, segments[2].Code);
        Assert.Equal(expected.Length + XmpSignature.Length + 2, app1.Length);
        Assert.True(app1.Payload.Span[..XmpSignature.Length].SequenceEqual(XmpSignature));
        Assert.Equal(expected, app1.Payload.Slice(XmpSignature.Length).ToArray());
        if (kind != "mixed")
        {
            Assert.Equal(MaximumXmpBytes, expected.Length);
            Assert.Equal(65_535, app1.Length);
        }
    }

    [Theory]
    [InlineData("ASCII limit")]
    [InlineData("two-byte limit")]
    [InlineData("four-byte limit")]
    public void XmpLimitCountsUtf8BytesAndRejectsOverflowBeforeOutput(string kind)
    {
        string packet = Xmp(kind) + "a";
        Assert.Equal(MaximumXmpBytes + 1, Encoding.UTF8.GetByteCount(packet));
        AssertLengthRejected(new() { XmpPacket = packet }, nameof(JpegliEncoderOptions.XmpPacket));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void InvalidUtf16XmpIsRejectedBeforeOutput(int kind)
    {
        string packet = kind switch
        {
            0 => "\ud800",
            1 => "\udc00",
            2 => "before\ud800after",
            3 => "before\udc00after",
            4 => "\ud800\ud800",
            _ => "valid 😀 then \ud800"
        };
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { ExifData = OpaqueBytes(7), XmpPacket = packet },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.XmpPacket), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Fact]
    public void App1SegmentsPrecedeIccChunksCommentAndCodingTablesInSpecifiedOrder()
    {
        byte[] exif = OpaqueBytes(19);
        const string packet = "<x:xmpmeta>café 😀</x:xmpmeta>";
        byte[] profile = OpaqueBytes(IccChunkBytes + 1);
        byte[] jpeg = Encode(new()
        {
            ExifData = exif, XmpPacket = packet, IccProfile = profile, Comment = "after APP1 and ICC"
        });
        List<Segment> segments = ReadHeaders(jpeg);
        Assert.Equal(new[] { 0xe0, 0xe1, 0xe1, 0xe2, 0xe2, 0xfe, 0xdb },
            segments.Take(7).Select(segment => segment.Code).ToArray());
        Assert.Equal(ExifSignature.Concat(exif).ToArray(), segments[1].Payload.ToArray());
        Assert.Equal(XmpSignature.Concat(Encoding.UTF8.GetBytes(packet)).ToArray(), segments[2].Payload.ToArray());
        Assert.True(segments[3].Payload.Span[..12].SequenceEqual("ICC_PROFILE\0"u8));
        Assert.Equal(1, segments[3].Payload.Span[12]);
        Assert.Equal(2, segments[3].Payload.Span[13]);
        Assert.True(segments[3].Payload.Span[14..].SequenceEqual(profile.AsSpan(0, IccChunkBytes)));
        Assert.Equal(2, segments[4].Payload.Span[12]);
        Assert.Equal(2, segments[4].Payload.Span[13]);
        Assert.True(segments[4].Payload.Span[14..].SequenceEqual(profile.AsSpan(IccChunkBytes)));
        Assert.True(segments[5].Payload.Span.SequenceEqual("after APP1 and ICC"u8));
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24, false, 0, false)]
    [InlineData(PixelLayout.Rgb24, false, 0, true)]
    [InlineData(PixelLayout.Rgb24, true, 0, false)]
    [InlineData(PixelLayout.Rgb24, true, 0, true)]
    [InlineData(PixelLayout.Rgb24, true, 2, false)]
    [InlineData(PixelLayout.Rgb24, true, 2, true)]
    [InlineData(PixelLayout.Gray8, false, 0, false)]
    [InlineData(PixelLayout.Gray8, false, 0, true)]
    [InlineData(PixelLayout.Gray8, true, 0, false)]
    [InlineData(PixelLayout.Gray8, true, 0, true)]
    [InlineData(PixelLayout.Gray8, true, 2, false)]
    [InlineData(PixelLayout.Gray8, true, 2, true)]
    public void App1LeavesCodingBytesAndDecodedPixelsUnchanged(
        PixelLayout layout, bool progressive, int refinementBits, bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = layout == PixelLayout.Gray8
            ? Enumerable.Range(0, width * height).Select(index => (byte)(index * 73 + index / width * 19)).ToArray()
            : TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions
        {
            Progressive = progressive, ProgressiveRefinementBits = refinementBits,
            OptimizeHuffmanTables = optimize, RestartInterval = 2
        };
        byte[] reference = JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);
        byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, layout, options with
        {
            ExifData = OpaqueBytes(513), XmpPacket = Xmp("mixed"),
            IccProfile = OpaqueBytes(13), Comment = "all metadata 😀",
            DensityUnit = JpegDensityUnit.DotsPerInch, HorizontalDensity = 300, VerticalDensity = 150
        }, cancellationToken: TestContext.Current.CancellationToken);
        // Exact equality from DQT to EOI covers coefficients, all progressive scans,
        // entropy bytes, byte stuffing, restart markers, and the end marker.
        int referenceStart = ReadHeaders(reference).Single(segment => segment.Code == 0xdb).Start;
        int actualStart = ReadHeaders(jpeg).Single(segment => segment.Code == 0xdb).Start;
        Assert.True(reference.AsSpan(referenceStart).SequenceEqual(jpeg.AsSpan(actualStart)));
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult expected = ImageResult.FromMemory(reference, channels);
        ImageResult actual = ImageResult.FromMemory(jpeg, channels);
        Assert.Equal(width, actual.Width);
        Assert.Equal(height, actual.Height);
        Assert.Equal(expected.Data, actual.Data);
    }

    [Fact]
    public void App1StreamOutputNeedsNoSeekingAndLeavesDestinationOpen()
    {
        var options = AllMetadata() with { Progressive = true, ProgressiveRefinementBits = 2 };
        byte[] expected = Encode(options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
        output.WriteByte(42);
        Assert.Equal(expected.Length + 1, destination.Length);
    }

    [Fact]
    public void PreCancellationWithApp1WritesNothingAndLeavesStreamOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: AllMetadata(), cancellationToken: token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)] // After JFIF, before Exif.
    [InlineData(1)] // After Exif, before XMP.
    [InlineData(2)] // After XMP, before ICC.
    [InlineData(3)] // After ICC, before COM.
    [InlineData(4)] // After COM, before DQT.
    public void CancellationStopsBeforeAndAfterApp1Segments(int segmentIndex)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        int boundary = ReadHeaders(complete)[segmentIndex].End;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= boundary)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void App1IoFailurePropagatesOriginalExceptionAndLeavesStreamOpen(int segmentIndex, bool afterPayload)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        Segment segment = ReadHeaders(complete)[segmentIndex];
        // Fail after the four-byte APP1 header, or after the entire segment.
        int boundary = afterPayload ? segment.End : segment.Start + 4;
        var expectedError = new IOException("Injected APP1 write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= boundary)
                throw expectedError;
        });
        var actualError = Assert.Throws<IOException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Same(expectedError, actualError);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static JpegliEncoderOptions AllMetadata() => new()
    {
        ExifData = OpaqueBytes(37), XmpPacket = "<x:xmpmeta>stream 😀</x:xmpmeta>",
        IccProfile = OpaqueBytes(13), Comment = "after APP1"
    };

    private static byte[] OpaqueBytes(int length)
    {
        byte[] bytes = new byte[length];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = (byte)(index * 73 + 19);
        // Include marker-looking bytes and embedded zero bytes, all opaque data.
        if (length >= 8)
            new byte[] { 0xff, 0xd8, 0xff, 0xe1, 0, 0, 0xff, 0xda }.CopyTo(bytes, 0);
        return bytes;
    }

    private static string Xmp(string kind) => kind switch
    {
        "ASCII limit" => new string('a', MaximumXmpBytes),
        "two-byte limit" => new string('é', MaximumXmpBytes / 2),
        "four-byte limit" => string.Concat(Enumerable.Repeat("😀", MaximumXmpBytes / 4)),
        // Arbitrary serialized content remains opaque, including invalid XML and a null.
        _ => "<x:xmpmeta>UTF-8\0 café — 日本語 😀 & unescaped</x:xmpmeta>"
    };

    private static void AssertLengthRejected(JpegliEncoderOptions options, string property)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(property, error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    // Read JPEG lengths independently; marker-looking APP1 bytes are not delimiters.
    private static List<Segment> ReadHeaders(byte[] jpeg)
    {
        Assert.True(jpeg.AsSpan(0, 2).SequenceEqual(new byte[] { 0xff, 0xd8 }));
        var segments = new List<Segment>();
        int position = 2;
        while (true)
        {
            Assert.InRange(position, 2, jpeg.Length - 4);
            int start = position;
            Assert.Equal(0xff, jpeg[position++]);
            int code = jpeg[position++];
            int length = jpeg[position] * 256 + jpeg[position + 1];
            Assert.InRange(length, 2, jpeg.Length - position);
            segments.Add(new(code, start, length, jpeg.AsMemory(position + 2, length - 2)));
            position += length;
            if (code == 0xda)
                return segments;
        }
    }

    private sealed class WriteOnlyStream(Stream inner, Action? afterWrite = null) : Stream
    {
        public bool Closed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !Closed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);
            afterWrite?.Invoke();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            inner.Write(buffer);
            afterWrite?.Invoke();
        }
        public override void WriteByte(byte value)
        {
            inner.WriteByte(value);
            afterWrite?.Invoke();
        }
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
