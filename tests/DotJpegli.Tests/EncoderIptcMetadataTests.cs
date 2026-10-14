using System.Security.Cryptography;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderIptcMetadataTests
{
    private const int MaximumIptcBytes = 65_506;
    private const int ResourceHeaderBytes = 26;
    private const int IccChunkBytes = 65_519;
    private const int ExtendedXmpChunkBytes = 65_458;

    [Fact]
    public void DefaultAndEmptyIptcPreserveExistingJpegBytes()
    {
        byte[] reference = Encode(new());
        Assert.Equal("42F2E14F627342F0603E9BA08653777959192031987F82F32BFB9664F0EC7DD6",
            Convert.ToHexString(SHA256.HashData(reference)));
        Assert.Equal(reference, JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(reference, Encode(new() { IptcData = ReadOnlyMemory<byte>.Empty }));
        Assert.Equal(reference, Encode(new() { IptcData = OpaqueBytes(7).AsMemory(3, 0) }));
        Assert.DoesNotContain(ReadHeaders(reference), segment => segment.Code == 0xed);

        var metadata = AllMetadata() with { IptcData = default };
        byte[] existingMetadata = Encode(metadata);
        Assert.Equal(existingMetadata, Encode(metadata with { IptcData = ReadOnlyMemory<byte>.Empty }));
        Assert.DoesNotContain(ReadHeaders(existingMetadata), segment => segment.Code == 0xed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(8)]
    [InlineData(513)]
    [InlineData(MaximumIptcBytes - 1)]
    [InlineData(MaximumIptcBytes)]
    public void App13ContainsOneUnmodifiedSlicedIptcResourceWithEvenPadding(int length)
    {
        // Invalid IIM and marker-looking bytes are deliberately opaque. The backing
        // memory outside this slice must never appear in the Photoshop resource.
        byte[] backing = Enumerable.Repeat((byte)0xa5, length + 11).ToArray();
        OpaqueBytes(length).CopyTo(backing, 5);
        ReadOnlyMemory<byte> iptc = backing.AsMemory(5, length);
        byte[] jpeg = Encode(new() { IptcData = iptc });
        List<Segment> headers = ReadHeaders(jpeg);
        Segment app13 = Assert.Single(headers, segment => segment.Code == 0xed);
        Assert.Equal(new[] { 0xe0, 0xed, 0xdb }, headers.Take(3).Select(segment => segment.Code).ToArray());
        AssertIptcResource(app13, iptc.Span);
        if (length >= MaximumIptcBytes - 1)
        {
            // Resource data is padded to an even byte count, so the largest valid
            // APP13 length is 65534, not the odd JPEG segment limit of 65535.
            Assert.Equal(65_534, app13.Length);
        }
    }

    [Theory]
    [InlineData("byte", false)]
    [InlineData("byte", true)]
    [InlineData("ushort", false)]
    [InlineData("ushort", true)]
    [InlineData("float", false)]
    [InlineData("float", true)]
    public void OversizedIptcIsRejectedBeforeAnyOutputForEveryInputTypeAndGrayscale(string format, bool grayscale)
    {
        Array pixels = Pixels(format, grayscale, 1, 1);
        var options = AllMetadata() with
        {
            IptcData = new byte[MaximumIptcBytes + 1],
            Progressive = true, ProgressiveRefinementBits = 2,
            OptimizeHuffmanTables = false, RestartInterval = 2,
            SmoothingFactor = 37, AllowOutOfRangeFloatSamples = true
        };
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Encode(output, pixels, 1, 1, grayscale, options, TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.IptcData), error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);

        var memoryError = Assert.Throws<ArgumentOutOfRangeException>(() => Encode(pixels, 1, 1, grayscale, options));
        Assert.Equal(nameof(JpegliEncoderOptions.IptcData), memoryError.ParamName);
    }

    [Fact]
    public void IptcFollowsExifStandardAndExtendedXmpAndIccBeforeCommentAndCodingTables()
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] jpeg = Encode(options);
        List<Segment> headers = ReadHeaders(jpeg);
        Assert.Equal(new[] { 0xe0, 0xe1, 0xe1, 0xe1, 0xe1, 0xe2, 0xe2, 0xed, 0xfe, 0xdb },
            headers.Take(10).Select(segment => segment.Code).ToArray());
        Assert.True(headers[1].Payload.Span.StartsWith("Exif\0\0"u8));
        Assert.True(headers[2].Payload.Span.StartsWith("http://ns.adobe.com/xap/1.0/\0"u8));
        Assert.True(headers[3].Payload.Span.StartsWith("http://ns.adobe.com/xmp/extension/\0"u8));
        Assert.True(headers[4].Payload.Span.StartsWith("http://ns.adobe.com/xmp/extension/\0"u8));
        Assert.True(headers[5].Payload.Span.StartsWith("ICC_PROFILE\0"u8));
        Assert.True(headers[6].Payload.Span.StartsWith("ICC_PROFILE\0"u8));
        AssertIptcResource(headers[7], options.IptcData.Span);
        Assert.True(headers[8].Payload.Span.SequenceEqual("after IPTC"u8));

        // Removing only APP13 reproduces every existing metadata and coding byte.
        Assert.Equal(Encode(options with { IptcData = default }), RemoveSegment(jpeg, headers[7]));
    }

    [Theory]
    [InlineData("byte", false, false, 0, false)]
    [InlineData("byte", true, false, 0, true)]
    [InlineData("ushort", false, true, 0, false)]
    [InlineData("float", true, true, 0, true)]
    [InlineData("ushort", true, true, 2, false)]
    [InlineData("float", false, true, 2, true)]
    public void IptcLeavesAllCodingBytesAndIndependentlyDecodedPixelsUnchanged(
        string format, bool grayscale, bool progressive, int refinement, bool optimize)
    {
        // Odd dimensions exercise partial MCUs; ignored row elements exercise stride.
        const int width = 33, height = 19;
        int stride = width * (grayscale ? 1 : 3) + 5;
        Array pixels = Pixels(format, grayscale, width, height, stride);
        var options = new JpegliEncoderOptions
        {
            Progressive = progressive, ProgressiveRefinementBits = refinement,
            OptimizeHuffmanTables = optimize, RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] reference = Encode(pixels, width, height, grayscale, options, stride);
        byte[] actual = Encode(pixels, width, height, grayscale,
            options with { IptcData = OpaqueBytes(513) }, stride);
        List<Segment> actualHeaders = ReadHeaders(actual);
        AssertIptcResource(Assert.Single(actualHeaders, segment => segment.Code == 0xed), OpaqueBytes(513));
        int expectedStart = ReadHeaders(reference).First(segment => segment.Code == 0xdb).Start;
        int actualStart = actualHeaders.First(segment => segment.Code == 0xdb).Start;
        // Exact equality through EOI covers coefficients, every scan, Huffman tables,
        // restart markers, entropy padding and byte stuffing.
        Assert.True(reference.AsSpan(expectedStart).SequenceEqual(actual.AsSpan(actualStart)));
        ColorComponents channels = grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult expectedImage = ImageResult.FromMemory(reference, channels);
        ImageResult actualImage = ImageResult.FromMemory(actual, channels);
        Assert.Equal(width, actualImage.Width);
        Assert.Equal(height, actualImage.Height);
        Assert.Equal(expectedImage.Data, actualImage.Data);
    }

    [Fact]
    public void NonseekableOutputMatchesMemoryEncodingAndRemainsOpen()
    {
        var options = AllMetadata() with { Progressive = true, ProgressiveRefinementBits = 2 };
        byte[] expected = Encode(options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, options, TestContext.Current.CancellationToken);
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
        output.WriteByte(42);
        Assert.Equal(expected.Length + 1, destination.Length);
    }

    [Fact]
    public void PreCancellationWithIptcWritesNothingAndLeavesOutputOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, AllMetadata(), token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationBeforeAndAfterIptcStopsAtMetadataBoundary(bool afterIptc)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        Segment iptc = Assert.Single(ReadHeaders(complete), segment => segment.Code == 0xed);
        int boundary = afterIptc ? iptc.End : iptc.Start;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= boundary)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData("marker length")]
    [InlineData("resource header")]
    [InlineData("raw data")]
    [InlineData("odd padding")]
    [InlineData("end of resource")]
    public void IptcIoFailurePreservesOriginalExceptionPartialBytesAndOpenOutput(string part)
    {
        JpegliEncoderOptions options = AllMetadata();
        byte[] complete = Encode(options);
        Segment iptc = Assert.Single(ReadHeaders(complete), segment => segment.Code == 0xed);
        int boundary = part switch
        {
            "marker length" => iptc.Start + 3,
            "resource header" => iptc.Start + 4 + ResourceHeaderBytes - 1,
            "raw data" => iptc.Start + 4 + ResourceHeaderBytes + 3,
            "odd padding" => iptc.End - 1,
            _ => iptc.End
        };
        var failure = new IOException("Injected IPTC APP13 write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, failureBoundary: boundary, failure: failure);
        var error = Assert.Throws<IOException>(() => Encode(output, options, TestContext.Current.CancellationToken));
        Assert.Same(failure, error);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegliEncoderOptions AllMetadata() => new()
    {
        ExifData = OpaqueBytes(19), ExtendedXmpPacket = new string('a', ExtendedXmpChunkBytes + 1),
        IccProfile = OpaqueBytes(IccChunkBytes + 1), IptcData = OpaqueBytes(13), Comment = "after IPTC"
    };

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static void Encode(Stream output, JpegliEncoderOptions options, CancellationToken token) =>
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options, cancellationToken: token);

    private static byte[] Encode(Array pixels, int width, int height, bool grayscale, JpegliEncoderOptions options, int stride = 0)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, grayscale, options, TestContext.Current.CancellationToken, stride);
        return output.ToArray();
    }

    private static void Encode(Stream output, Array pixels, int width, int height, bool grayscale,
        JpegliEncoderOptions options, CancellationToken token, int stride = 0)
    {
        PixelComponentLayout components = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
        switch (pixels)
        {
            case byte[] bytes:
                JpegliEncoder.Encode(output, bytes, width, height, grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24,
                    options, stride, token);
                break;
            case ushort[] words:
                JpegliEncoder.Encode(output, words, width, height, components, options, stride, token);
                break;
            case float[] floats:
                JpegliEncoder.Encode(output, floats, width, height, components, options, stride, token);
                break;
            default:
                throw new ArgumentException("Unknown pixel type.", nameof(pixels));
        }
    }

    private static Array Pixels(string format, bool grayscale, int width, int height, int stride = 0)
    {
        int rowElements = width * (grayscale ? 1 : 3);
        if (stride == 0)
            stride = rowElements;
        byte[] bytes = Enumerable.Repeat((byte)0xf1, (height - 1) * stride + rowElements).ToArray();
        for (int y = 0; y < height; y++)
            for (int index = 0; index < rowElements; index++)
                bytes[y * stride + index] = (byte)(index * 73 + y * 19);
        return format switch
        {
            "byte" => bytes,
            "ushort" => bytes.Select((value, index) => (ushort)(value * 251 + index % 6)).ToArray(),
            "float" => bytes.Select((value, index) => (value * 251 + index % 6) / 65535f).ToArray(),
            _ => throw new ArgumentException("Unknown pixel format.", nameof(format))
        };
    }

    private static byte[] OpaqueBytes(int length)
    {
        byte[] bytes = new byte[length];
        for (int index = 0; index < length; index++)
            bytes[index] = (byte)(index * 73 + 19);
        ReadOnlySpan<byte> prefix = new byte[] { 0xff, 0xd8, 0xff, 0xed, 0, 0, 0xff, 0xda };
        prefix[..Math.Min(length, prefix.Length)].CopyTo(bytes);
        return bytes;
    }

    private static void AssertIptcResource(Segment app13, ReadOnlySpan<byte> data)
    {
        Assert.Equal(0xed, app13.Code);
        ReadOnlySpan<byte> payload = app13.Payload.Span;
        int padding = data.Length & 1;
        Assert.Equal(ResourceHeaderBytes + data.Length + padding, payload.Length);
        Assert.Equal(payload.Length + 2, app13.Length);
        Assert.True(payload[..14].SequenceEqual("Photoshop 3.0\0"u8));
        Assert.True(payload.Slice(14, 4).SequenceEqual("8BIM"u8));
        Assert.Equal(0x04, payload[18]);
        Assert.Equal(0x04, payload[19]);
        Assert.Equal(0, payload[20]); // Empty Pascal resource name.
        Assert.Equal(0, payload[21]); // Name padding to even length.
        long storedLength = (long)payload[22] * 16_777_216 + payload[23] * 65_536 + payload[24] * 256 + payload[25];
        Assert.Equal(data.Length, storedLength);
        Assert.True(payload.Slice(ResourceHeaderBytes, data.Length).SequenceEqual(data));
        if (padding != 0)
            Assert.Equal(0, payload[^1]);
        // The exact payload length excludes any digest resource, terminator or
        // additional image resource after the sole 0x0404 IPTC resource.
    }

    private static byte[] RemoveSegment(byte[] jpeg, Segment segment) =>
        jpeg[..segment.Start].Concat(jpeg[segment.End..]).ToArray();

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    // Read JPEG's big-endian lengths independently of the writer. Marker-looking
    // data and 0xff bytes in APP13 must never be interpreted as header delimiters.
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

    private sealed class WriteOnlyStream(Stream inner, Action? afterWrite = null,
        long? failureBoundary = null, IOException? failure = null) : Stream
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
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            int count = failureBoundary is long boundary ? (int)Math.Min(buffer.Length, boundary - inner.Length) : buffer.Length;
            inner.Write(buffer[..count]);
            if (failureBoundary is long end && inner.Length >= end)
                throw failure!;
            afterWrite?.Invoke();
        }
        public override void WriteByte(byte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            Write(buffer);
        }
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            base.Dispose(disposing);
        }
    }
}
