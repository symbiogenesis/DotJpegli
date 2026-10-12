using System.Security.Cryptography;
using System.Text;
using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderMetadataTests
{
    // JPEG's 16-bit segment length includes its own two bytes. ICC adds a
    // twelve-byte signature and two sequence bytes before each profile chunk.
    private const int IccChunkBytes = 65_519;
    private const int MaximumIccBytes = 16_707_345;
    private const int MaximumCommentBytes = 65_533;

    [Fact]
    public void DefaultAndEmptyMetadataPreserveExistingBytes()
    {
        byte[] jpeg = Encode(new JpegliEncoderOptions());
        // Captured before metadata support, at revision 8018f1d.
        Assert.Equal("A3C09B17325C4F1EDC2FC43141855B604E5AB4ADAD24D3B9E78AE400D3CD3A28",
            Convert.ToHexString(SHA256.HashData(jpeg)));
        Assert.Equal(jpeg, JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(jpeg, Encode(new() { IccProfile = ReadOnlyMemory<byte>.Empty, Comment = "" }));

        List<Segment> segments = ReadHeaders(jpeg);
        Assert.Equal(0xe0, segments[0].Code);
        Assert.Equal(new byte[] { 0x4a, 0x46, 0x49, 0x46, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 },
            segments[0].Payload.ToArray());
        Assert.DoesNotContain(segments, segment => segment.Code is 0xe2 or 0xfe);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(IccChunkBytes)]
    [InlineData(IccChunkBytes + 1)]
    [InlineData(2 * IccChunkBytes)]
    [InlineData(2 * IccChunkBytes + 1)]
    [InlineData(MaximumIccBytes)]
    public void IccChunksHaveExactLengthsSequenceNumbersAndUnmodifiedPayload(int length)
    {
        // Slice a larger backing array so writing the entire array is detectable.
        byte[] backing = Profile(length + 8);
        ReadOnlyMemory<byte> profile = backing.AsMemory(3, length);
        byte[] jpeg = Encode(new() { IccProfile = profile, Comment = "after ICC" });
        List<Segment> segments = ReadHeaders(jpeg);
        int count = (length + IccChunkBytes - 1) / IccChunkBytes;
        Assert.Equal(0xe0, segments[0].Code);
        Assert.Equal(count, segments.Count(segment => segment.Code == 0xe2));

        int position = 0;
        for (int chunk = 0; chunk < count; chunk++)
        {
            Segment segment = segments[chunk + 1];
            int bytes = Math.Min(IccChunkBytes, length - position);
            Assert.Equal(0xe2, segment.Code);
            Assert.Equal(bytes + 16, segment.Length);
            Assert.True(segment.Payload.Span[..12].SequenceEqual("ICC_PROFILE\0"u8));
            Assert.Equal(chunk + 1, segment.Payload.Span[12]);
            Assert.Equal(count, segment.Payload.Span[13]);
            Assert.True(segment.Payload.Span[14..].SequenceEqual(profile.Span.Slice(position, bytes)));
            position += bytes;
        }
        Assert.Equal(length, position);
        Assert.Equal(0xfe, segments[count + 1].Code);
        Assert.True(segments[count + 1].Payload.Span.SequenceEqual("after ICC"u8));
        Assert.Equal(0xdb, segments[count + 2].Code);
    }

    [Fact]
    public void OversizedIccProfileIsRejectedBeforeAnyOutput()
    {
        AssertRejected(new() { IccProfile = new byte[MaximumIccBytes + 1] },
            nameof(JpegliEncoderOptions.IccProfile));
    }

    [Theory]
    [InlineData("mixed")]
    [InlineData("ASCII limit")]
    [InlineData("two-byte limit")]
    [InlineData("four-byte limit")]
    public void CommentContainsExactUtf8BytesWithoutATerminator(string kind)
    {
        string comment = Comment(kind);
        byte[] expected = Encoding.UTF8.GetBytes(comment);
        byte[] jpeg = Encode(new() { Comment = comment });
        List<Segment> segments = ReadHeaders(jpeg);
        Segment segment = Assert.Single(segments, segment => segment.Code == 0xfe);
        Assert.Equal(0xe0, segments[0].Code);
        Assert.Equal(segment, segments[1]);
        Assert.Equal(0xdb, segments[2].Code);
        Assert.Equal(expected.Length + 2, segment.Length);
        Assert.Equal(expected, segment.Payload.ToArray());
        if (kind != "mixed")
            Assert.Equal(MaximumCommentBytes, expected.Length);
    }

    [Theory]
    [InlineData("ASCII limit")]
    [InlineData("two-byte limit")]
    [InlineData("four-byte limit")]
    public void CommentLimitUsesUtf8ByteLengthAndRejectsOverflowBeforeOutput(string kind)
    {
        // Multibyte overflows contain fewer than 65,533 UTF-16 code units.
        string comment = Comment(kind) + "a";
        Assert.Equal(MaximumCommentBytes + 1, Encoding.UTF8.GetByteCount(comment));
        AssertRejected(new() { Comment = comment }, nameof(JpegliEncoderOptions.Comment));
    }

    [Theory]
    [InlineData(JpegDensityUnit.Unspecified, 1, 1)]
    [InlineData(JpegDensityUnit.Unspecified, 65_535, 1)]
    [InlineData(JpegDensityUnit.DotsPerInch, 0x1234, 0xabcd)]
    [InlineData(JpegDensityUnit.DotsPerCentimeter, 65_535, 65_535)]
    public void DensityUpdatesOnlyJfifUnitAndBigEndianDensityFields(JpegDensityUnit unit, int horizontal, int vertical)
    {
        byte[] reference = Encode(new());
        byte[] jpeg = Encode(new() { DensityUnit = unit, HorizontalDensity = horizontal, VerticalDensity = vertical });
        Segment jfif = ReadHeaders(jpeg)[0];
        Assert.Equal(0xe0, jfif.Code);
        Assert.Equal(16, jfif.Length);
        Assert.Equal(new byte[]
        {
            0x4a, 0x46, 0x49, 0x46, 0, 1, 1, (byte)unit,
            (byte)(horizontal >> 8), (byte)horizontal,
            (byte)(vertical >> 8), (byte)vertical, 0, 0
        }, jfif.Payload.ToArray());
        Assert.True(reference.AsSpan(jfif.End).SequenceEqual(jpeg.AsSpan(jfif.End)));
    }

    [Theory]
    [InlineData(nameof(JpegliEncoderOptions.DensityUnit), -1)]
    [InlineData(nameof(JpegliEncoderOptions.DensityUnit), 3)]
    [InlineData(nameof(JpegliEncoderOptions.DensityUnit), int.MaxValue)]
    [InlineData(nameof(JpegliEncoderOptions.HorizontalDensity), 0)]
    [InlineData(nameof(JpegliEncoderOptions.HorizontalDensity), -1)]
    [InlineData(nameof(JpegliEncoderOptions.HorizontalDensity), 65_536)]
    [InlineData(nameof(JpegliEncoderOptions.HorizontalDensity), int.MaxValue)]
    [InlineData(nameof(JpegliEncoderOptions.VerticalDensity), 0)]
    [InlineData(nameof(JpegliEncoderOptions.VerticalDensity), int.MinValue)]
    [InlineData(nameof(JpegliEncoderOptions.VerticalDensity), 65_536)]
    [InlineData(nameof(JpegliEncoderOptions.VerticalDensity), int.MaxValue)]
    public void InvalidDensityIsRejectedBeforeAnyOutput(string property, int value)
    {
        JpegliEncoderOptions options = property switch
        {
            nameof(JpegliEncoderOptions.DensityUnit) => new() { DensityUnit = (JpegDensityUnit)value },
            nameof(JpegliEncoderOptions.HorizontalDensity) => new() { HorizontalDensity = value },
            _ => new() { VerticalDensity = value }
        };
        AssertRejected(options, property);
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24, false, 0)]
    [InlineData(PixelLayout.Rgb24, true, 0)]
    [InlineData(PixelLayout.Rgb24, true, 2)]
    [InlineData(PixelLayout.Gray8, false, 0)]
    [InlineData(PixelLayout.Gray8, true, 2)]
    public void MetadataLeavesScansAndDecodedPixelsUnchanged(PixelLayout layout, bool progressive, int refinementBits)
    {
        const int width = 33, height = 19;
        byte[] pixels = layout == PixelLayout.Gray8
            ? Enumerable.Range(0, width * height).Select(index => (byte)(index * 73 + index / width * 19)).ToArray()
            : TestImages.Gradient(width, height);
        var options = new JpegliEncoderOptions
        {
            Progressive = progressive, ProgressiveRefinementBits = refinementBits, RestartInterval = 2
        };
        byte[] reference = JpegliEncoder.Encode(pixels, width, height, layout, options,
            cancellationToken: TestContext.Current.CancellationToken);
        byte[] jpeg = JpegliEncoder.Encode(pixels, width, height, layout, options with
        {
            IccProfile = Profile(IccChunkBytes + 5), Comment = "JPEG\0metadata — 😀",
            DensityUnit = JpegDensityUnit.DotsPerInch, HorizontalDensity = 300, VerticalDensity = 150
        }, cancellationToken: TestContext.Current.CancellationToken);
        // Comparing from DQT through EOI also covers every progressive/refinement
        // scan, including entropy bytes, stuffing and restart markers.
        int referenceStart = ReadHeaders(reference).Single(segment => segment.Code == 0xdb).Start;
        int actualStart = ReadHeaders(jpeg).Single(segment => segment.Code == 0xdb).Start;
        Assert.True(reference.AsSpan(referenceStart).SequenceEqual(jpeg.AsSpan(actualStart)));
        ColorComponents channels = layout == PixelLayout.Gray8 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        ImageResult expected = ImageResult.FromMemory(reference, channels);
        ImageResult decoded = ImageResult.FromMemory(jpeg, channels);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        Assert.Equal(expected.Data, decoded.Data);
    }

    [Fact]
    public void MetadataStreamEncodingRequiresNoSeekingAndLeavesDestinationOpen()
    {
        var options = new JpegliEncoderOptions
        {
            IccProfile = Profile(IccChunkBytes + 1), Comment = "stream 😀", Progressive = true,
            DensityUnit = JpegDensityUnit.DotsPerCentimeter, HorizontalDensity = 118, VerticalDensity = 118
        };
        byte[] expected = Encode(options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
    }

    [Fact]
    public void PreCancellationWithMetadataWritesNothingAndLeavesStreamOpen()
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1,
                options: new() { IccProfile = Profile(IccChunkBytes + 1), Comment = "cancel" },
                cancellationToken: new CancellationToken(true)));
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(0)] // After JFIF, before the first ICC segment.
    [InlineData(1)] // Between ICC chunks.
    [InlineData(2)] // After the last ICC chunk, before COM.
    [InlineData(3)] // After COM, before DQT.
    public void CancellationStopsAtMetadataSegmentBoundaries(int segmentIndex)
    {
        var options = new JpegliEncoderOptions { IccProfile = Profile(IccChunkBytes + 1), Comment = "cancel 😀" };
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
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
                cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void MetadataIoFailurePropagatesAndLeavesStreamOpen(int segmentIndex)
    {
        var options = new JpegliEncoderOptions { IccProfile = Profile(IccChunkBytes + 1), Comment = "I/O 😀" };
        byte[] complete = Encode(options);
        int boundary = ReadHeaders(complete)[segmentIndex].End;
        var expectedError = new IOException("Injected metadata write failure.");
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, afterWrite: () =>
        {
            if (destination.Length >= boundary)
                throw expectedError;
        });
        IOException actualError = Assert.Throws<IOException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Same(expectedError, actualError);
        Assert.Equal(complete[..boundary], destination.ToArray());
        Assert.False(output.Closed);
    }

    private static byte[] Encode(JpegliEncoderOptions options) =>
        JpegliEncoder.Encode(new byte[] { 255, 0, 0 }, 1, 1, options: options,
            cancellationToken: TestContext.Current.CancellationToken);

    private static byte[] Profile(int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)(i * 73 + 19);
        return bytes;
    }

    private static string Comment(string kind) => kind switch
    {
        "ASCII limit" => new string('a', MaximumCommentBytes),
        "two-byte limit" => new string('é', MaximumCommentBytes / 2) + "a",
        "four-byte limit" => string.Concat(Enumerable.Repeat("😀", MaximumCommentBytes / 4)) + "a",
        _ => "UTF-8\0 café — 日本語 😀"
    };

    private static void AssertRejected(JpegliEncoderOptions options, string property)
    {
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(output, new byte[] { 255, 0, 0 }, 1, 1, options: options,
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(property, error.ParamName);
        Assert.Equal(0, output.Length);
    }

    private sealed record Segment(int Code, int Start, int Length, ReadOnlyMemory<byte> Payload)
    {
        public int End => Start + Length + 2;
    }

    // Independent length-based reader. Marker-looking bytes within APP2 and COM
    // payloads must be preserved and never mistaken for image markers.
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
