using System.Security.Cryptography;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderRawMetadataTests
{
    private const int MaximumPayloadBytes = 65_533;

    [Fact]
    public void NullAndEmptyMarkerListsPreserveExistingJpegBytes()
    {
        byte[] reference = Encode(new());
        Assert.Equal("42F2E14F627342F0603E9BA08653777959192031987F82F32BFB9664F0EC7DD6",
            Convert.ToHexString(SHA256.HashData(reference)));
        Assert.Equal(reference, JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(reference, Encode(new() { MetadataMarkers = [] }));
        Assert.Equal(reference, Encode(new() { MetadataMarkers = Array.Empty<JpegMetadataMarker>() }));

        JpegliEncoderOptions metadata = AllMetadata();
        byte[] existingMetadata = Encode(metadata);
        Assert.Equal(existingMetadata, Encode(metadata with { MetadataMarkers = [] }));
    }

    [Fact]
    public void EveryAppCodeAndCommentAreAllowedAndPreserveOrderAndDuplicateCodes()
    {
        var markers = Enumerable.Range(0xe0, 16)
            .Select(code => new JpegMetadataMarker(code, OpaqueBytes(code - 0xdf))).ToList();
        markers.Add(new(0xfe, new byte[] { 0, 0xff, 0, 0xfe }));
        markers.Add(new(0xe0, ReadOnlyMemory<byte>.Empty));
        markers.Add(new(0xe1, OpaqueBytes(7)));
        markers.Add(new(0xfe, ReadOnlyMemory<byte>.Empty));

        byte[] jpeg = Encode(new() { MetadataMarkers = markers });
        List<Segment> headers = ReadHeaders(jpeg);
        Segment[] raw = headers.Skip(1).Take(markers.Count).ToArray();
        Assert.Equal(markers.Select(marker => marker.MarkerCode), raw.Select(segment => segment.Code));
        for (int index = 0; index < markers.Count; index++)
            AssertRawSegment(raw[index], markers[index]);
        Assert.Equal(0xe0, headers[0].Code); // Existing JFIF remains first.
        Assert.Equal(0xdb, headers[markers.Count + 1].Code);
        Assert.Equal(Encode(new()), RemoveRange(jpeg, raw[0].Start, raw[^1].End));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(513)]
    [InlineData(MaximumPayloadBytes - 1)]
    [InlineData(MaximumPayloadBytes)]
    public void PayloadIsOneUnmodifiedSliceWithoutSignaturesStuffingOrSplitting(int length)
    {
        byte[] backing = Enumerable.Repeat((byte)0xa5, length + 11).ToArray();
        OpaqueBytes(length).CopyTo(backing, 5);
        var marker = new JpegMetadataMarker(0xe3, backing.AsMemory(5, length));
        byte[] jpeg = Encode(new() { MetadataMarkers = [marker] });
        List<Segment> headers = ReadHeaders(jpeg);
        Segment actual = Assert.Single(headers, segment => segment.Code == 0xe3);
        AssertRawSegment(actual, marker);
        Assert.Equal(new[] { 0xe0, 0xe3, 0xdb }, headers.Take(3).Select(segment => segment.Code));
        Assert.Equal(length + 2, actual.Length);
        if (length == MaximumPayloadBytes)
            Assert.Equal(65_535, actual.Length);
        Assert.Equal(Encode(new()), RemoveRange(jpeg, actual.Start, actual.End));
    }

    [Fact]
    public void RawRgbMarkersStartImmediatelyAfterSoiWithoutAutomaticJfif()
    {
        JpegMetadataMarker[] markers = [new(0xe3, OpaqueBytes(17)), new(0xfe, default)];
        var options = new JpegliEncoderOptions { ColorSpace = JpegColorSpace.Rgb, MetadataMarkers = markers };
        byte[] jpeg = Encode(options);
        List<Segment> headers = ReadHeaders(jpeg);
        Assert.Equal(new[] { 0xe3, 0xfe, 0xdb }, headers.Take(3).Select(segment => segment.Code));
        Assert.Equal(2, headers[0].Start);
        for (int index = 0; index < markers.Length; index++)
            AssertRawSegment(headers[index], markers[index]);
        Assert.Equal(Encode(options with { MetadataMarkers = null }), RemoveRange(jpeg, headers[0].Start, headers[1].End));
    }

    [Theory]
    [InlineData("byte", false)]
    [InlineData("byte", true)]
    [InlineData("ushort", false)]
    [InlineData("ushort", true)]
    [InlineData("float", false)]
    [InlineData("float", true)]
    public void InvalidLaterMarkerIsRejectedBeforeAnyOutputForEveryInputTypeAndGrayscale(string format, bool grayscale)
    {
        Array pixels = Pixels(format, grayscale, 1, 1);
        // Standalone/coding markers, out-of-byte codes, and APP15's following gap
        // must not enter the file even when valid markers precede the bad entry.
        foreach (int code in new[] { int.MinValue, -1, 0, 0xd8, 0xd9, 0xda, 0xdb, 0xdf, 0xf0, 0xfd, 0xff, 0x1e0, int.MaxValue })
            AssertRejected(new(code, OpaqueBytes(3)), outOfRange: true);
        AssertRejected(new(0xe3, new byte[MaximumPayloadBytes + 1]), outOfRange: true);
        AssertRejected(null!, outOfRange: false);

        void AssertRejected(JpegMetadataMarker bad, bool outOfRange)
        {
            JpegliEncoderOptions options = AllMetadata() with
            {
                MetadataMarkers = [new(0xe3, OpaqueBytes(3)), new(0xfe, default), bad],
                Progressive = true, ProgressiveRefinementBits = 2,
                OptimizeHuffmanTables = false, RestartInterval = 2,
                SmoothingFactor = 37, AllowOutOfRangeFloatSamples = true
            };
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            ArgumentException streamError = outOfRange
                ? Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Encode(output, pixels, 1, 1, grayscale, options, TestContext.Current.CancellationToken))
                : Assert.Throws<ArgumentException>(() =>
                    Encode(output, pixels, 1, 1, grayscale, options, TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.MetadataMarkers), streamError.ParamName);
            Assert.Equal(0, destination.Length);
            Assert.False(output.Closed);

            ArgumentException memoryError = outOfRange
                ? Assert.Throws<ArgumentOutOfRangeException>(() => Encode(pixels, 1, 1, grayscale, options))
                : Assert.Throws<ArgumentException>(() => Encode(pixels, 1, 1, grayscale, options));
            Assert.Equal(nameof(JpegliEncoderOptions.MetadataMarkers), memoryError.ParamName);
        }
    }

    [Fact]
    public void RawMarkersFollowAllBuiltInMetadataAndCommentBeforeCodingTables()
    {
        JpegMetadataMarker[] markers = [new(0xe3, OpaqueBytes(11)), new(0xe3, default),
            new(0xfe, new byte[] { 0, 0xff, 0 }), new(0xe4, OpaqueBytes(17))];
        JpegliEncoderOptions options = AllMetadata() with { MetadataMarkers = markers };
        byte[] jpeg = Encode(options);
        List<Segment> headers = ReadHeaders(jpeg);
        Assert.Equal(new[] { 0xe0, 0xe1, 0xe1, 0xe1, 0xe1, 0xe2, 0xe2, 0xed, 0xfe, 0xe3, 0xe3, 0xfe, 0xe4, 0xdb },
            headers.Take(14).Select(segment => segment.Code));
        Assert.True(headers[1].Payload.Span.StartsWith("Exif\0\0"u8));
        Assert.True(headers[2].Payload.Span.StartsWith("http://ns.adobe.com/xap/1.0/\0"u8));
        Assert.True(headers[3].Payload.Span.StartsWith("http://ns.adobe.com/xmp/extension/\0"u8));
        Assert.True(headers[4].Payload.Span.StartsWith("http://ns.adobe.com/xmp/extension/\0"u8));
        Assert.True(headers[5].Payload.Span.StartsWith("ICC_PROFILE\0"u8));
        Assert.True(headers[6].Payload.Span.StartsWith("ICC_PROFILE\0"u8));
        Assert.True(headers[7].Payload.Span.StartsWith("Photoshop 3.0\0"u8));
        Assert.True(headers[8].Payload.Span.SequenceEqual("before raw markers"u8));
        for (int index = 0; index < markers.Length; index++)
            AssertRawSegment(headers[9 + index], markers[index]);
        Assert.Equal(Encode(options with { MetadataMarkers = null }), RemoveRange(jpeg, headers[9].Start, headers[12].End));
    }

    [Theory]
    [InlineData("byte", false, "sequential", false)]
    [InlineData("byte", true, "sequential", true)]
    [InlineData("ushort", false, "progressive", false)]
    [InlineData("float", true, "progressive", true)]
    [InlineData("ushort", true, "refined", false)]
    [InlineData("float", false, "refined", true)]
    [InlineData("byte", false, "separate sequential", true)]
    [InlineData("ushort", false, "script", true)]
    public void InertMarkersLeaveAllCodingBytesAndIndependentlyDecodedPixelsUnchanged(
        string format, bool grayscale, string scans, bool optimize)
    {
        const int width = 33, height = 19;
        int stride = width * (grayscale ? 1 : 3) + 5;
        Array pixels = Pixels(format, grayscale, width, height, stride);
        var options = new JpegliEncoderOptions
        {
            Progressive = scans is "progressive" or "refined",
            ProgressiveRefinementBits = scans == "refined" ? 2 : 0,
            SequentialInterleaveComponents = scans != "separate sequential",
            ScanScript = scans == "script" ? [new([2]), new([0, 1])] : null,
            OptimizeHuffmanTables = optimize, RestartInterval = 2, SmoothingFactor = 37
        };
        JpegMetadataMarker[] markers = [new(0xe3, OpaqueBytes(513)), new(0xe4, OpaqueBytes(11)), new(0xfe, OpaqueBytes(19))];
        byte[] reference = Encode(pixels, width, height, grayscale, options, stride);
        byte[] actual = Encode(pixels, width, height, grayscale, options with { MetadataMarkers = markers }, stride);
        List<Segment> actualHeaders = ReadHeaders(actual);
        for (int index = 0; index < markers.Length; index++)
            AssertRawSegment(actualHeaders[index + 1], markers[index]);
        int expectedStart = ReadHeaders(reference).First(segment => segment.Code == 0xdb).Start;
        int actualStart = actualHeaders.First(segment => segment.Code == 0xdb).Start;
        // Equality through EOI checks every coding table, scan, restart marker,
        // entropy byte and stuffing sequence without depending on the decoder.
        Assert.True(reference.AsSpan(expectedStart).SequenceEqual(actual.AsSpan(actualStart)));
        ColorComponents channels = grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult expectedImage = ImageResult.FromMemory(reference, channels);
        ImageResult actualImage = ImageResult.FromMemory(actual, channels);
        Assert.Equal(width, actualImage.Width);
        Assert.Equal(height, actualImage.Height);
        Assert.Equal(expectedImage.Data, actualImage.Data);
    }

    [Theory]
    [InlineData("byte")]
    [InlineData("ushort")]
    [InlineData("float")]
    public void MarkerListIsSnapshottedBeforeFirstOutput(string format)
    {
        var markers = new List<JpegMetadataMarker>
        {
            new(0xe3, OpaqueBytes(17)), new(0xfe, default), new(0xe4, OpaqueBytes(21)), new(0xe3, OpaqueBytes(3))
        };
        Array pixels = Pixels(format, false, 17, 19);
        var options = new JpegliEncoderOptions { MetadataMarkers = markers, Progressive = true };
        byte[] expected = Encode(pixels, 17, 19, false, options);
        bool mutated = false;
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (mutated)
                return;
            mutated = true;
            markers.Reverse();
            markers.Clear();
            markers.Add(new(0xd9, new byte[MaximumPayloadBytes + 1]));
        });
        Encode(output, pixels, 17, 19, false, options, TestContext.Current.CancellationToken);
        Assert.True(mutated);
        Assert.Equal(expected, destination.ToArray());
        Assert.False(output.Closed);
    }

    [Fact]
    public void NonseekableOutputMatchesMemoryEncodingAndRemainsOpen()
    {
        JpegliEncoderOptions options = BoundaryMetadata() with { Progressive = true, ProgressiveRefinementBits = 2 };
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
    public void PreCancellationWritesNothingAndLeavesOutputOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, BoundaryMetadata(), token));
        Assert.Equal(token, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CancellationBeforeEachRawMarkerAndAfterTheListStopsAtTheBoundary(int completedMarkers)
    {
        JpegliEncoderOptions options = BoundaryMetadata();
        byte[] complete = Encode(options);
        Segment[] raw = ReadHeaders(complete).Skip(2).Take(3).ToArray();
        int boundary = completedMarkers == raw.Length ? raw[^1].End : raw[completedMarkers].Start;
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
    [InlineData("marker prefix")]
    [InlineData("marker length")]
    [InlineData("raw data")]
    [InlineData("end of marker")]
    public void RawMarkerIoFailurePreservesOriginalExceptionPartialBytesAndOpenOutput(string part)
    {
        JpegliEncoderOptions options = BoundaryMetadata();
        byte[] complete = Encode(options);
        Segment raw = Assert.Single(ReadHeaders(complete), segment => segment.Code == 0xe4);
        int boundary = part switch
        {
            "marker prefix" => raw.Start + 1,
            "marker length" => raw.Start + 3,
            "raw data" => raw.Start + 7,
            _ => raw.End
        };
        var failure = new IOException("Injected raw APP4 write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, failureBoundary: boundary, failure: failure);
        var error = Assert.Throws<IOException>(() => Encode(output, options, TestContext.Current.CancellationToken));
        Assert.Same(failure, error);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static JpegliEncoderOptions AllMetadata() => new()
    {
        ExifData = OpaqueBytes(19), ExtendedXmpPacket = new string('a', 65_459),
        IccProfile = OpaqueBytes(65_520), IptcData = OpaqueBytes(13), Comment = "before raw markers"
    };

    private static JpegliEncoderOptions BoundaryMetadata() => new()
    {
        Comment = "before raw markers",
        MetadataMarkers = [new(0xe3, OpaqueBytes(19)), new(0xe4, OpaqueBytes(25)), new(0xfe, default)]
    };

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static void Encode(Stream output, JpegliEncoderOptions options, CancellationToken token) =>
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options, cancellationToken: token);

    private static byte[] Encode(Array pixels, int width, int height, bool grayscale, JpegliEncoderOptions options, int stride = 0)
    {
        PixelComponentLayout components = grayscale ? PixelComponentLayout.Gray : PixelComponentLayout.Rgb;
        return pixels switch
        {
            byte[] bytes => JpegliEncoder.Encode(bytes, width, height, grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24,
                options, stride, TestContext.Current.CancellationToken),
            ushort[] words => JpegliEncoder.Encode(words, width, height, components,
                options, stride, TestContext.Current.CancellationToken),
            float[] floats => JpegliEncoder.Encode(floats, width, height, components,
                options, stride, TestContext.Current.CancellationToken),
            _ => throw new ArgumentException("Unknown pixel type.", nameof(pixels))
        };
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
        ReadOnlySpan<byte> prefix = new byte[] { 0xff, 0xd8, 0xff, 0xe3, 0, 0, 0xff, 0xda };
        prefix[..Math.Min(length, prefix.Length)].CopyTo(bytes);
        return bytes;
    }

    private static void AssertRawSegment(Segment actual, JpegMetadataMarker marker)
    {
        Assert.Equal(marker.MarkerCode, actual.Code);
        Assert.Equal(marker.Data.Length + 2, actual.Length);
        Assert.True(actual.Payload.Span.SequenceEqual(marker.Data.Span));
    }

    private static byte[] RemoveRange(byte[] jpeg, int start, int end) =>
        jpeg[..start].Concat(jpeg[end..]).ToArray();

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    // Parse segment lengths independently. Payload bytes resembling JPEG markers
    // must remain inside their declared raw segment, including unstuffed 0xff.
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
