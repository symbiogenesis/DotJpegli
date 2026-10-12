using System.Buffers.Binary;
using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderIccTransferFunctionTests
{
    // Structural expectations independently checked against extracted FindCICPTag
    // and LookupCICPTransferFunctionFromICCProfile from google/jpegli revision
    // 031a0077f5799a6041004267fc12b956c1f52a20, compiled with MSVC as C++17:
    // 29 structural profiles and all 256 transfer bytes. Synthetic equivalents
    // below retain header opacity, size/offset bounds and last-duplicate behavior.
    private const int IccChunkBytes = 65_519;

    [Fact]
    public void InferenceIsOptInAndEveryExplicitNonUnspecifiedCodeOverridesTheProfile()
    {
        Assert.False(new JpegliEncoderOptions().InferCicpTransferFunctionFromIccProfile);
        byte[] pixels = Pattern(9, 9, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { IccProfile = Profile(16), Distance = 2.375f };
        byte[] original = Encode(pixels, 9, 9, PixelComponentLayout.Rgb, options);
        Assert.Equal(original, Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
            options with { InferCicpTransferFunctionFromIccProfile = false }));
        Assert.False(original.SequenceEqual(Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
            options with { InferCicpTransferFunctionFromIccProfile = true })));

        for (int code = 0; code <= 255; code++)
            if (code != 2)
            {
                JpegliEncoderOptions explicitHint = options with { CicpTransferFunction = code };
                Assert.Equal(Encode(pixels, 9, 9, PixelComponentLayout.Rgb, explicitHint),
                    Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
                        explicitHint with { InferCicpTransferFunctionFromIccProfile = true }));
            }
    }

    [Fact]
    public void LookupReadsAllTransferBytesWithoutValidatingOpaqueHeaderOrOtherCicpFields()
    {
        for (int code = 0; code <= 255; code++)
        {
            byte[] profile = Profile(code);
            // Deliberately invalid ICC size, magic/version and CICP reserved/color fields.
            // Unrelated entries also point outside the profile and must be ignored.
            Assert.Equal(code, IccProfile.LookupTransferFunction(profile,
                TestContext.Current.CancellationToken));
            Assert.Equal(code, IccProfile.LookupTransferFunction(
                SlicedProfile(profile).Span, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(16, 0)]
    [InlineData(18, 1)]
    [InlineData(18, 132)]
    [InlineData(16, 65_511)]
    [InlineData(18, 65_517)]
    [InlineData(16, IccChunkBytes)]
    [InlineData(18, 2 * IccChunkBytes - 6)]
    public void TagOffsetsAndPayloadsAcrossApp2BoundariesUseTheWholeSlicedProfile(int transfer, int offset)
    {
        byte[] profile = Profile(transfer, offset);
        ReadOnlyMemory<byte> slice = SlicedProfile(profile);
        byte[] before = slice.ToArray();
        var options = new JpegliEncoderOptions
        {
            IccProfile = slice, InferCicpTransferFunctionFromIccProfile = true,
            Distance = 2.375f, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
        };
        byte[] pixels = Pattern(9, 9, PixelLayout.YCbCr24);
        byte[] actual = Encode(pixels, 9, 9, PixelComponentLayout.YCbCr, options);
        Assert.Equal(Encode(pixels, 9, 9, PixelComponentLayout.YCbCr,
            options with { InferCicpTransferFunctionFromIccProfile = false, CicpTransferFunction = transfer }), actual);
        Assert.Equal(before, ReassembleIcc(actual));
        Assert.Equal(before, slice.ToArray());
        Assert.Equal(transfer, IccProfile.LookupTransferFunction(slice.Span,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TagDirectoryMayExtendPastTheFirstApp2Chunk()
    {
        const int count = 5500;
        byte[] profile = Profile(18, 132 + count * 12 + 7, count);
        Assert.True(132 + (count - 1) * 12 > IccChunkBytes);
        var options = new JpegliEncoderOptions
        {
            IccProfile = SlicedProfile(profile), InferCicpTransferFunctionFromIccProfile = true,
            Distance = 2.375f
        };
        byte[] pixels = Pattern(9, 9, PixelLayout.Rgb24);
        byte[] actual = Encode(pixels, 9, 9, PixelComponentLayout.Rgb, options);
        Assert.Equal(Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
            options with { InferCicpTransferFunctionFromIccProfile = false, CicpTransferFunction = 18 }), actual);
        Assert.Equal(profile, ReassembleIcc(actual));
    }

    [Theory]
    [InlineData(12u)]
    [InlineData(13u)]
    [InlineData(uint.MaxValue)]
    public void DeclaredTagSizeOnlyRequiresTheFirstTwelvePayloadBytes(uint size)
    {
        byte[] profile = Profile(16);
        WriteUInt32(profile, 132 + 2 * 12 + 8, size);
        Assert.Equal(220, profile.Length);
        Assert.Equal(16, IccProfile.LookupTransferFunction(profile,
            TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("short header")]
    [InlineData("no tags")]
    [InlineData("no cicp entry")]
    [InlineData("incomplete entry")]
    [InlineData("incomplete table")]
    [InlineData("maximum count")]
    [InlineData("overflowing count")]
    [InlineData("short declared size")]
    [InlineData("zero declared size")]
    [InlineData("maximum offset")]
    [InlineData("offset at end")]
    [InlineData("offset after end")]
    [InlineData("eleven payload bytes")]
    [InlineData("truncated payload")]
    [InlineData("wrong payload signature")]
    [InlineData("wrong entry signature")]
    public void MalformedOrMissingLookupFallsBackAndStillEmbedsTheOriginalProfile(string kind)
    {
        byte[] profile = MalformedProfile(kind);
        Assert.Equal(2, IccProfile.LookupTransferFunction(profile,
            TestContext.Current.CancellationToken));
        var options = new JpegliEncoderOptions { IccProfile = SlicedProfile(profile), Distance = 2.375f };
        byte[] pixels = Pattern(9, 9, PixelLayout.Rgb24);
        byte[] expected = Encode(pixels, 9, 9, PixelComponentLayout.Rgb, options);
        byte[] actual = Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
            options with { InferCicpTransferFunctionFromIccProfile = true });
        Assert.Equal(expected, actual);
        Assert.Equal(profile, ReassembleIcc(actual));
    }

    [Theory]
    [InlineData("valid", 18)]
    [InlineData("unspecified", 2)]
    [InlineData("short size", 2)]
    [InlineData("outside profile", 2)]
    [InlineData("wrong payload signature", 2)]
    public void LastDuplicateEntryWinsEvenWhenItCannotBeRead(string kind, int expectedTransfer)
    {
        byte[] profile = Profile(16, 208, 4);
        Array.Resize(ref profile, 268);
        WriteEntry(profile, 2, "cicp"u8, 208, 12);
        WriteEntry(profile, 3, "cicp"u8, 256, kind == "short size" ? 11u : 12u);
        WriteTag(profile, 256, kind == "unspecified" ? 2 : 18);
        if (kind == "outside profile")
            WriteUInt32(profile, 132 + 3 * 12 + 4, uint.MaxValue);
        else if (kind == "wrong payload signature")
            profile[256] = (byte)'C';
        Assert.Equal(expectedTransfer, IccProfile.LookupTransferFunction(profile,
            TestContext.Current.CancellationToken));
        var options = new JpegliEncoderOptions { IccProfile = profile, Distance = 2.375f };
        byte[] pixels = Pattern(9, 9, PixelLayout.Rgb24);
        Assert.Equal(Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
                options with { CicpTransferFunction = expectedTransfer }),
            Encode(pixels, 9, 9, PixelComponentLayout.Rgb,
                options with { InferCicpTransferFunctionFromIccProfile = true }));
    }

    [Fact]
    public void LaterUnrelatedEntriesKeepThePreviouslySelectedCicpTag()
    {
        byte[] profile = Profile(16);
        WriteEntry(profile, 1, "cicp"u8, 208, 12);
        WriteEntry(profile, 2, "desc"u8, uint.MaxValue, 0);
        Assert.Equal(16, IccProfile.LookupTransferFunction(profile,
            TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(0, PixelComponentLayout.Rgb, 16)]
    [InlineData(0, PixelComponentLayout.Gray, 18)]
    [InlineData(0, PixelComponentLayout.YCbCr, 16)]
    [InlineData(1, PixelComponentLayout.Bgra, 18)]
    [InlineData(1, PixelComponentLayout.Gray, 16)]
    [InlineData(1, PixelComponentLayout.YCbCr, 18)]
    [InlineData(2, PixelComponentLayout.Rgba, 16)]
    [InlineData(2, PixelComponentLayout.Gray, 18)]
    [InlineData(2, PixelComponentLayout.YCbCr, 16)]
    public void TypedAndPaddedInputsUseTheResolvedHintAcrossScansAndRestarts(
        int kind, PixelComponentLayout layout, int transfer)
    {
        const int width = 19, height = 17;
        int stride = width * Components(layout) + 7;
        Array pixels = Source(kind, width, height, layout, stride);
        Array before = (Array)pixels.Clone();
        foreach ((bool progressive, int bits, bool optimize) in new[]
        {
            (false, 0, false), (true, 0, true), (true, 3, false)
        })
        {
            var options = new JpegliEncoderOptions
            {
                IccProfile = SlicedProfile(Profile(transfer)), InferCicpTransferFunctionFromIccProfile = true,
                Distance = 2.375f, Progressive = progressive, ProgressiveRefinementBits = bits,
                OptimizeHuffmanTables = optimize, RestartInterval = 2, SmoothingFactor = 37,
                SamplingFactors = new(new(2, 2), new(2, 1), new(1, 1))
            };
            byte[] actual = Encode(pixels, width, height, layout, options, stride);
            Assert.Equal(Encode(pixels, width, height, layout,
                options with { InferCicpTransferFunctionFromIccProfile = false, CicpTransferFunction = transfer }, stride), actual);
            Assert.False(actual.SequenceEqual(Encode(pixels, width, height, layout,
                options with { InferCicpTransferFunctionFromIccProfile = false }, stride)));
            Assert.Equal(options.IccProfile.ToArray(), ReassembleIcc(actual));
            ImageResult decoded = ImageResult.FromMemory(actual, layout == PixelComponentLayout.Gray
                ? ColorComponents.Grey : ColorComponents.RedGreenBlue);
            Assert.Equal((width, height), (decoded.Width, decoded.Height));
        }
        Assert.Equal(before, pixels);
    }

    [Theory]
    [InlineData(16, 1.29577017f, 1.29881847f)]
    [InlineData(18, 1.06647623f, 1.06891489f)]
    public void InferredHintReachesStandardSharedChromaAdaptiveThresholds(
        int transfer, float crMultiplier, float wrongSelectorMultiplier)
    {
        // Independently compiled pinned upstream scalar fixtures retained in
        // EncoderTransferFunctionTests establish these AQ multipliers and strength.
        // Analytical cosines distinguish no hint and an incorrect shared-table basis.
        const float strength = 0.0467278957f, defaultCrMultiplier = 0.607888103f;
        foreach (bool selectorProbe in new[] { false, true })
        {
            double correct = 0.57988f + strength * crMultiplier;
            double wrong = 0.57988f + strength * (selectorProbe ? wrongSelectorMultiplier : defaultCrMultiplier);
            double amplitude = (correct + wrong) / 2d * 3d / (4d * Math.Sqrt(2d));
            float[] pixels = new float[8 * 8 * 3];
            for (int row = 0; row < 8; row++)
                for (int column = 0; column < 8; column++)
                {
                    int offset = (row * 8 + column) * 3;
                    pixels[offset] = pixels[offset + 1] = 128f / 255f;
                    pixels[offset + 2] = (float)((128d + amplitude *
                        Math.Cos(Math.PI * (2 * column + 1) / 16d)) / 255d);
                }
            foreach (bool progressive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions
                {
                    IccProfile = Profile(transfer), InferCicpTransferFunctionFromIccProfile = true,
                    UseStandardQuantizationTables = true, Distance = 0.9f, Subsampling = ChromaSubsampling.None,
                    Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
                    OptimizeHuffmanTables = progressive, RestartInterval = 1
                };
                byte[] actual = Encode(pixels, 8, 8, PixelComponentLayout.YCbCr, options);
                Assert.Equal(Encode(pixels, 8, 8, PixelComponentLayout.YCbCr,
                    options with { InferCicpTransferFunctionFromIccProfile = false, CicpTransferFunction = transfer }), actual);
                JpegDocument document = Read(actual, options);
                Assert.Equal(new[] { 0, 1, 1 }, document.Components.Values.Select(component => component.Quantizer));
                Assert.Equal(selectorProbe ? 1 : 0, document.Blocks[new(3, 0, 0)].Coefficients[1]);
                if (!selectorProbe)
                {
                    byte[] disabled = Encode(pixels, 8, 8, PixelComponentLayout.YCbCr,
                        options with { InferCicpTransferFunctionFromIccProfile = false });
                    Assert.Equal(1, Read(disabled, options).Blocks[new(3, 0, 0)].Coefficients[1]);
                }
            }
        }
    }

    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void InferredFinerQuantizersAreAppliedBeforeOutOfRangeFloatPreflight(int transfer)
    {
        float[] pixels = Enumerable.Repeat(-0.1f, 64).ToArray();
        var options = new JpegliEncoderOptions
        {
            IccProfile = Profile(transfer), Distance = 1f, AdaptiveQuantization = false,
            AllowOutOfRangeFloatSamples = true, Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1
        };
        Assert.Equal(-614, Assert.Single(Read(Encode(pixels, 8, 8, PixelComponentLayout.Gray, options), options)
            .Blocks).Value.Coefficients[0]);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<ArgumentException>(() => Encode(output, pixels, 8, 8, PixelComponentLayout.Gray,
            options with { InferCicpTransferFunctionFromIccProfile = true }, TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
        Assert.All(pixels, sample => Assert.Equal(-0.1f, sample));
    }

    [Fact]
    public void LookupAndEncodingHonorPreCancellationBeforeOutput()
    {
        byte[] profile = Profile(18, 132 + 5500 * 12 + 3, 5500);
        var canceled = new CancellationToken(true);
        var lookupError = Assert.Throws<OperationCanceledException>(() =>
            IccProfile.LookupTransferFunction(profile, canceled));
        Assert.Equal(canceled, lookupError.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        var error = Assert.Throws<OperationCanceledException>(() => Encode(output, new byte[] { 255, 0, 0 },
            1, 1, PixelComponentLayout.Rgb, new()
            {
                IccProfile = profile, InferCicpTransferFunctionFromIccProfile = true
            }, canceled));
        Assert.Equal(canceled, error.CancellationToken);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Fact]
    public void NonSeekableEncodingPreservesIccBytesAndCancellationBetweenChunks()
    {
        byte[] profile = Profile(16, 2 * IccChunkBytes + 7);
        byte[] pixels = Pattern(19, 17, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            IccProfile = SlicedProfile(profile), InferCicpTransferFunctionFromIccProfile = true,
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2, Comment = "after ICC"
        };
        byte[] complete = Encode(pixels, 19, 17, PixelComponentLayout.Rgb, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, pixels, 19, 17, PixelComponentLayout.Rgb, options, TestContext.Current.CancellationToken);
        Assert.Equal(complete, destination.ToArray());
        Assert.Equal(profile, ReassembleIcc(complete));
        Assert.False(output.Closed);

        // SOI + the 18-byte APP0 marker + the first full APP2 marker.
        const int firstChunkEnd = 2 + 18 + IccChunkBytes + 18;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var partial = new MemoryStream();
        using var partialOutput = new WriteOnlyStream(partial, () =>
        {
            if (partial.Length >= firstChunkEnd)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => Encode(partialOutput, pixels, 19, 17,
            PixelComponentLayout.Rgb, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(firstChunkEnd, partial.Length);
        Assert.Equal(complete[..firstChunkEnd], partial.ToArray());
        Assert.False(partialOutput.Closed);
    }

    private static byte[] Profile(int transfer, int offset = 208, int count = 3)
    {
        byte[] profile = Enumerable.Repeat((byte)0xa7, Math.Max(132 + count * 12, offset + 12)).ToArray();
        WriteUInt32(profile, 128, (uint)count);
        WriteEntry(profile, 0, "abcd"u8, uint.MaxValue, uint.MaxValue);
        WriteEntry(profile, count - 1, "cicp"u8, (uint)offset, 12);
        WriteTag(profile, offset, transfer);
        return profile;
    }

    private static void WriteEntry(byte[] profile, int index, ReadOnlySpan<byte> signature, uint offset, uint size)
    {
        int start = 132 + index * 12;
        signature.CopyTo(profile.AsSpan(start, 4));
        WriteUInt32(profile, start + 4, offset);
        WriteUInt32(profile, start + 8, size);
    }

    private static void WriteTag(byte[] profile, int offset, int transfer)
    {
        "cicp"u8.CopyTo(profile.AsSpan(offset));
        profile[offset + 8] = 241;
        profile[offset + 9] = (byte)transfer;
        profile[offset + 10] = 253;
        profile[offset + 11] = 250;
    }

    private static void WriteUInt32(byte[] profile, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(offset, 4), value);

    private static ReadOnlyMemory<byte> SlicedProfile(byte[] profile)
    {
        byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        return backing.AsMemory(3, profile.Length);
    }

    private static byte[] MalformedProfile(string kind)
    {
        byte[] profile = Profile(16);
        const int entry = 132 + 2 * 12;
        switch (kind)
        {
            case "empty": return [];
            case "short header": return profile[..131];
            case "no tags": WriteUInt32(profile, 128, 0); break;
            case "no cicp entry": profile[entry] = (byte)'x'; break;
            case "incomplete entry": WriteUInt32(profile, 128, 1); return profile[..143];
            case "incomplete table": WriteUInt32(profile, 128, 8); break;
            case "maximum count": WriteUInt32(profile, 128, uint.MaxValue); break;
            case "overflowing count": WriteUInt32(profile, 128, 0x40000003); break;
            case "short declared size": WriteUInt32(profile, entry + 8, 11); break;
            case "zero declared size": WriteUInt32(profile, entry + 8, 0); break;
            case "maximum offset": WriteUInt32(profile, entry + 4, uint.MaxValue); break;
            case "offset at end": WriteUInt32(profile, entry + 4, (uint)profile.Length); break;
            case "offset after end": WriteUInt32(profile, entry + 4, (uint)profile.Length + 1); break;
            case "eleven payload bytes": WriteUInt32(profile, entry + 4, (uint)profile.Length - 11); break;
            case "truncated payload": return profile[..^1];
            case "wrong payload signature": profile[208] = (byte)'C'; break;
            case "wrong entry signature": profile[entry] = (byte)'C'; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
        return profile;
    }

    private static byte[] ReassembleIcc(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        using var profile = new MemoryStream();
        int expectedSequence = 1, declaredCount = 0;
        for (int position = 2; position < jpeg.Length;)
        {
            Assert.Equal(0xff, jpeg[position]);
            int marker = jpeg[position + 1];
            if (marker == 0xda)
                break;
            int length = (jpeg[position + 2] << 8) | jpeg[position + 3];
            Assert.InRange(length, 2, jpeg.Length - position - 2);
            if (marker == 0xe2)
            {
                ReadOnlySpan<byte> payload = jpeg.AsSpan(position + 4, length - 2);
                Assert.True(payload[..12].SequenceEqual("ICC_PROFILE\0"u8));
                Assert.Equal(expectedSequence++, payload[12]);
                if (declaredCount == 0)
                    declaredCount = payload[13];
                Assert.Equal(declaredCount, payload[13]);
                profile.Write(payload[14..]);
            }
            position += length + 2;
        }
        Assert.Equal(declaredCount, expectedSequence - 1);
        return profile.ToArray();
    }

    private static int Components(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => 1,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        _ => 3
    };

    private static Array Source(int kind, int width, int height, PixelComponentLayout layout, int stride)
    {
        int components = Components(layout);
        int length = (height - 1) * stride + width * components;
        Array source = kind switch
        {
            0 => new byte[length],
            1 => new ushort[length],
            _ => Enumerable.Repeat(float.NaN, length).ToArray()
        };
        for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
                for (int channel = 0; channel < (layout == PixelComponentLayout.Gray ? 1 : 3); channel++)
                {
                    int targetChannel = layout == PixelComponentLayout.Bgra ? 2 - channel : channel;
                    int offset = row * stride + column * components + targetChannel;
                    int value = 24 + (11 * column + 19 * row + 37 * channel) % 208;
                    if (source is byte[] bytes)
                        bytes[offset] = (byte)value;
                    else if (source is ushort[] words)
                        words[offset] = (ushort)(value * 257 + (column + row + channel) % 113);
                    else
                        ((float[])source)[offset] = (value + (column + row + channel) % 113 / 257f) / 255f;
                }
        return source;
    }

    private static byte[] Encode(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, int stride = 0)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, layout, options, TestContext.Current.CancellationToken, stride);
        return output.ToArray();
    }

    private static void Encode(Stream output, Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options, CancellationToken cancellationToken, int stride = 0)
    {
        if (pixels is byte[] bytes)
        {
            PixelLayout byteLayout = layout switch
            {
                PixelComponentLayout.Gray => PixelLayout.Gray8,
                PixelComponentLayout.YCbCr => PixelLayout.YCbCr24,
                PixelComponentLayout.Rgba => PixelLayout.Rgba32,
                PixelComponentLayout.Bgra => PixelLayout.Bgra32,
                _ => PixelLayout.Rgb24
            };
            JpegliEncoder.Encode(output, bytes, width, height, byteLayout, options, stride, cancellationToken);
        }
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, stride, cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, stride, cancellationToken);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);
}
