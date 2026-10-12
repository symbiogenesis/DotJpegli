using StbImageSharp;
using Xunit;

namespace DotJpegli.Tests;

public sealed class EncoderContractTests
{
    [Theory]
    [InlineData(PixelLayout.Rgb24, 3)]
    [InlineData(PixelLayout.Bgr24, 3)]
    [InlineData(PixelLayout.Rgba32, 4)]
    [InlineData(PixelLayout.Bgra32, 4)]
    public void PixelLayoutsAndPaddedStrideProduceSameJpeg(PixelLayout layout, int bytesPerPixel)
    {
        const int width = 11, height = 7;
        byte[] rgb = TestImages.Gradient(width, height);
        int stride = width * bytesPerPixel + 13;
        // No padding is required after the last row.
        byte[] source = new byte[(height - 1) * stride + width * bytesPerPixel];
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int input = (y * width + x) * 3;
                int output = y * stride + x * bytesPerPixel;
                source[output + (bgr ? 2 : 0)] = rgb[input];
                source[output + 1] = rgb[input + 1];
                source[output + (bgr ? 0 : 2)] = rgb[input + 2];
                if (bytesPerPixel == 4)
                    source[output + 3] = (byte)(x * 17); // Alpha has no effect.
            }
        Assert.Equal(JpegliEncoder.Encode(rgb, width, height, cancellationToken: TestContext.Current.CancellationToken),
            JpegliEncoder.Encode(source, width, height, layout, stride: stride, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ValidationAndPreCancellationWriteNoOutput()
    {
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(stream, new byte[3], 0, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(stream, new byte[2], 1, 1,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(stream, new byte[3], 1, 1,
            options: new() { Quality = float.NaN }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(stream, new byte[6], 2, 1,
            options: new() { MaxPixels = 1 }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(stream, new byte[3], 1, 1,
            stride: -1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(stream, new byte[3], 1, 1,
            layout: (PixelLayout)99, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Throws<OperationCanceledException>(() => JpegliEncoder.Encode(stream, new byte[3], 1, 1,
            cancellationToken: new CancellationToken(true)));
        Assert.Equal(0, stream.Length);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidProgressiveRefinementBitsWriteNoOutput(int bits)
    {
        using var stream = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliEncoder.Encode(stream, new byte[3], 1, 1,
                options: new() { Progressive = true, ProgressiveRefinementBits = bits },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveRefinementBits), error.ParamName);
        Assert.Equal(0, stream.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void RefinementRequiresProgressiveEncodingBeforeWriting(int bits)
    {
        using var stream = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(stream, new byte[3], 1, 1,
                options: new() { ProgressiveRefinementBits = bits },
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveRefinementBits), error.ParamName);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void EncoderLeavesNonSeekableStreamOpen()
    {
        using var destination = new MemoryStream();
        using var stream = new WriteOnlyStream(destination);
        JpegliEncoder.Encode(stream, new byte[] { 255, 0, 0 }, 1, 1,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(stream.CanWrite);
        ImageResult decoded = ImageResult.FromMemory(destination.ToArray(), ColorComponents.RedGreenBlue);
        Assert.Equal(1, decoded.Width);
        Assert.InRange(decoded.Data[0], (byte)240, (byte)255);
        Assert.InRange(decoded.Data[1], (byte)0, (byte)12);
        Assert.InRange(decoded.Data[2], (byte)0, (byte)12);
    }

    private sealed class WriteOnlyStream(Stream inner) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
        public override void WriteByte(byte value) => inner.WriteByte(value);
    }
}
