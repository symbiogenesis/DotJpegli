// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

/// <summary>Reads source samples in fractional JPEG units, optionally reusing converted planes.</summary>
internal readonly ref struct PixelSource
{
    private readonly ReadOnlySpan<byte> bytes;
    private readonly ReadOnlySpan<ushort> words;
    private readonly ReadOnlySpan<float> floats;
    private readonly SourceType sourceType;
    private readonly bool bgr;
    private readonly bool ycbcr;
    private readonly bool xyb;
    private readonly JpegColorSpace colorSpace;
    private readonly IccColorTransform? colorTransform;
    private readonly ConvertedPlanes? convertedPlanes;

    internal PixelSource(ReadOnlySpan<byte> pixels, PixelLayout layout)
    {
        this.bytes = pixels;
        this.sourceType = SourceType.Byte;
        this.ComponentsPerPixel = layout switch
        {
            PixelLayout.Rgb24 or PixelLayout.Bgr24 or PixelLayout.YCbCr24 or PixelLayout.Xyb24 => 3,
            PixelLayout.Rgba32 or PixelLayout.Bgra32 => 4,
            PixelLayout.Gray8 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };
        this.bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        this.ycbcr = layout == PixelLayout.YCbCr24;
        this.xyb = layout == PixelLayout.Xyb24;
    }

    internal PixelSource(ReadOnlySpan<ushort> pixels, PixelComponentLayout layout)
    {
        this.words = pixels;
        this.sourceType = SourceType.UInt16;
        this.ComponentsPerPixel = GetComponentCount(layout);
        this.bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        this.ycbcr = layout == PixelComponentLayout.YCbCr;
        this.xyb = layout == PixelComponentLayout.Xyb;
    }

    internal PixelSource(ReadOnlySpan<float> pixels, PixelComponentLayout layout)
    {
        this.floats = pixels;
        this.sourceType = SourceType.Float;
        this.ComponentsPerPixel = GetComponentCount(layout);
        this.bgr = layout is PixelComponentLayout.Bgr or PixelComponentLayout.Bgra;
        this.ycbcr = layout == PixelComponentLayout.YCbCr;
        this.xyb = layout == PixelComponentLayout.Xyb;
    }

    internal int ComponentsPerPixel { get; }
    internal ParallelDctRows? ParallelRows { get; }
    // The encode call owns this lease; worker snapshots only borrow its storage.
    internal IDisposable? ConvertedStorage => this.convertedPlanes;
    internal int ConvertedRows => this.convertedPlanes?.CachedHeight ?? 0;
    internal bool HasCompleteConvertedPlanes => this.convertedPlanes?.IsComplete == true;
    internal ReadOnlySpan<float> GetConvertedPlane(int component)
        => this.convertedPlanes is null || !this.convertedPlanes.IsContiguous || (this.convertedPlanes.Is420 && component != 0)
            ? default : this.convertedPlanes.GetPlane(component);
    internal ReadOnlySpan<float> GetConverted420ChromaPlane(int component, out int width, out int height)
    {
        width = this.convertedPlanes?.ChromaWidth ?? 0;
        height = this.convertedPlanes?.ChromaHeight ?? 0;
        return this.convertedPlanes is not { Is420: true, IsContiguous: true } || component is < 1 or > 2 ? default
            : this.convertedPlanes.GetPlane(component);
    }

    // Segments start at complete 16-row source boundaries (eight compact chroma
    // rows). A DCT block therefore never straddles a cached segment boundary.
    internal ReadOnlySpan<float> GetConvertedPlaneSegment(int component, int row, bool compact420,
        out int width, out int height, out int localRow)
    {
        width = height = localRow = 0;
        if (this.convertedPlanes is null || (component != 0 && this.convertedPlanes.Is420 != compact420))
        {
            return default;
        }
        return this.convertedPlanes.GetSegment(component, row, out width, out height, out localRow);
    }
    internal bool IsGrayscaleInput => this.ComponentsPerPixel == 1;
    internal bool CanManageSourceColor => !this.ycbcr && !this.xyb;
    internal JpegColorSpace ColorSpace => this.colorSpace == JpegColorSpace.Automatic
        ? (this.xyb ? JpegColorSpace.Xyb : this.IsGrayscaleInput ? JpegColorSpace.Grayscale : JpegColorSpace.YCbCr) : this.colorSpace;
    internal bool IsGrayscale => this.ColorSpace == JpegColorSpace.Grayscale;
    // XYB is carried in an RGB JPEG: identifiers, entropy selectors and AQ rules
    // are shared, while sampling defaults and quantization matrices differ.
    internal bool IsRgb => this.ColorSpace is JpegColorSpace.Rgb or JpegColorSpace.Xyb;
    internal bool IsXyb => this.ColorSpace == JpegColorSpace.Xyb;
    internal int AnalysisComponent => this.IsRgb ? 1 : 0;

    private PixelSource(PixelSource source, JpegColorSpace colorSpace, ParallelDctRows? parallelRows = null,
        IccColorTransform? colorTransform = null, ConvertedPlanes? convertedPlanes = null,
        bool discardConvertedPlanes = false)
    {
        this.bytes = source.bytes;
        this.words = source.words;
        this.floats = source.floats;
        this.sourceType = source.sourceType;
        this.bgr = source.bgr;
        this.ycbcr = source.ycbcr;
        this.xyb = source.xyb;
        this.ComponentsPerPixel = source.ComponentsPerPixel;
        this.colorSpace = colorSpace;
        this.ParallelRows = parallelRows;
        this.colorTransform = colorTransform ?? source.colorTransform;
        this.convertedPlanes = discardConvertedPlanes ? null : convertedPlanes ?? source.convertedPlanes;
    }

    internal PixelSource WithParallelRows(ParallelDctRows rows) => new(this, this.colorSpace, rows);

    internal PixelSource WithColorTransform(IccColorTransform transform) => new(this, this.colorSpace,
        colorTransform: transform, discardConvertedPlanes: true);

    // Conversion caches preflight their admitted prefix. Validate the remaining
    // source before the caller writes headers, and own cleanup on every failure.
    internal PixelSource WithPreflightedColorPlanes(int width, int height, int stride,
        EncoderCacheBudget? cacheBudget, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        PixelSource source = this;
        // Amortize a maximum of 256 evaluations per distinct source curve.
        // Tiny images retain direct conversion without preparing unused entries.
        if (this.sourceType == SourceType.Byte && this.colorTransform is { } transform && (long)width * height >= 256)
        {
            source = new PixelSource(this, this.colorSpace, colorTransform: transform.WithByteInput());
        }
        PixelSource converted = source.WithConvertedPlanes(width, height, stride, cancellationToken, cacheBudget, allowPartial: true);
        try
        {
            for (int row = converted.ConvertedRows; row < height; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int column = 0; column < width; column++)
                {
                    _ = converted.ReadSample((row * stride) + (column * converted.ComponentsPerPixel), 0);
                }
            }
            return converted;
        }
        catch
        {
            converted.ConvertedStorage?.Dispose();
            throw;
        }
    }

    /// <summary>Converts each source pixel once for reuse by analysis and every encoding scan.</summary>
    internal PixelSource WithConvertedPlanes(int width, int height, int stride,
        CancellationToken cancellationToken, EncoderCacheBudget? cacheBudget = null, bool allowPartial = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.convertedPlanes is not null)
        {
            return this;
        }
        int components = this.IsGrayscale ? 1 : 3;
        var planes = ConvertedPlanes.TryCreate(width, height, stride, this.Length,
            components, is420: false, transientBytes: 0, cacheBudget, allowPartial, cancellationToken);
        if (planes is null)
        {
            return this;
        }

        try
        {
            bool vectorRgb = Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian
                && Vector<float>.Count is 4 or 8 && this.sourceType == SourceType.Byte
                && !this.IsGrayscaleInput && !this.ycbcr && !this.xyb && this.colorTransform is null
                && this.ColorSpace is JpegColorSpace.YCbCr or JpegColorSpace.Grayscale;
            if (planes.IsContiguous)
            {
                Span<float> first = planes.GetPlane(0);
                Span<float> second = components == 1 ? default : planes.GetPlane(1);
                Span<float> third = components == 1 ? default : planes.GetPlane(2);
                for (int y = 0; y < height; y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int offset = y * width;
                    this.ConvertRow(y * stride, first.Slice(offset, width),
                        components == 1 ? default : second.Slice(offset, width),
                        components == 1 ? default : third.Slice(offset, width), vectorRgb);
                }
            }
            else
            {
                for (int y = 0; y < planes.CachedHeight; y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    this.ConvertRow(y * stride, planes.GetRow(0, y),
                        components == 1 ? default : planes.GetRow(1, y),
                        components == 1 ? default : planes.GetRow(2, y), vectorRgb);
                }
            }
            return new PixelSource(this, this.colorSpace, convertedPlanes: planes);
        }
        catch
        {
            planes.Dispose();
            throw;
        }
    }

    // Retain full-resolution Y for adaptive analysis, but feed DCT with the
    // exact ordered 2x2 chroma averages. Only two source rows of chroma scratch
    // are needed; no full-resolution Cb/Cr image is written or reread.
    internal PixelSource WithConverted420Planes(int width, int height, int stride,
        CancellationToken cancellationToken, EncoderCacheBudget? cacheBudget = null, bool allowPartial = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.convertedPlanes is not null)
        {
            return this;
        }

        if (this.sourceType != SourceType.Byte || this.IsGrayscaleInput || this.ycbcr || this.xyb
            || this.colorTransform is not null || this.ColorSpace != JpegColorSpace.YCbCr)
        {
            return this.WithConvertedPlanes(width, height, stride, cancellationToken, cacheBudget, allowPartial);
        }

        long scratchBytes = ConvertedPlanes.PooledBytes((long)width * 4);
        var planes = ConvertedPlanes.TryCreate(width, height, stride, this.Length,
            3, is420: true, transientBytes: scratchBytes, cacheBudget, allowPartial, cancellationToken);
        if (planes is null)
        {
            return this.WithConvertedPlanes(width, height, stride, cancellationToken, cacheBudget, allowPartial);
        }

        float[]? scratch = null;
        long reservedScratchBytes = 0;
        try
        {
            scratch = ArrayPool<float>.Shared.Rent(checked(width * 4));
            scratchBytes = (long)scratch.Length * sizeof(float);
            if (planes.RentedBytes + scratchBytes > ConvertedPlanes.MaximumBytes
                || (cacheBudget is not null && !cacheBudget.TryReserve(scratchBytes, EncoderCacheKind.ConvertedSamples)))
            {
                planes.Dispose();
                ArrayPool<float>.Shared.Return(scratch);
                scratch = null;
                return this.WithConvertedPlanes(width, height, stride, cancellationToken, cacheBudget, allowPartial);
            }
            reservedScratchBytes = scratchBytes;
            bool vectorRgb = Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian && Vector<float>.Count is 4 or 8;
            if (planes.IsContiguous)
            {
                this.ConvertContiguous420Rows(planes, width, height, stride, scratch, vectorRgb, cancellationToken);
                return new PixelSource(this, this.colorSpace, convertedPlanes: planes);
            }
            Span<float> firstCb = scratch.AsSpan(0, width), firstCr = scratch.AsSpan(width, width);
            Span<float> secondCb = scratch.AsSpan(2 * width, width), secondCr = scratch.AsSpan(3 * width, width);
            int chromaRow = 0;
            for (int y = 0; y < planes.CachedHeight; y += 2, chromaRow++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.ConvertRow(y * stride, planes.GetRow(0, y), firstCb, firstCr, vectorRgb);
                if (y + 1 < height)
                {
                    this.ConvertRow((y + 1) * stride, planes.GetRow(0, y + 1),
                        secondCb, secondCr, vectorRgb);
                }
                else
                {
                    firstCb.CopyTo(secondCb);
                    firstCr.CopyTo(secondCr);
                }
                DownsampleConvertedRow(firstCb, secondCb,
                    planes.GetRow(1, chromaRow));
                DownsampleConvertedRow(firstCr, secondCr,
                    planes.GetRow(2, chromaRow));
            }
            if (chromaRow < planes.CachedChromaHeight)
            {
                // Pad source samples before averaging. For an even source
                // height the last real average includes two different rows,
                // whereas every padded row uses the final source row twice.
                cancellationToken.ThrowIfCancellationRequested();
                DownsampleConvertedRow(secondCb, secondCb, planes.GetRow(1, chromaRow));
                DownsampleConvertedRow(secondCr, secondCr, planes.GetRow(2, chromaRow));
                for (int row = chromaRow + 1; row < planes.CachedChromaHeight; row++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    planes.GetRow(1, chromaRow).CopyTo(planes.GetRow(1, row));
                    planes.GetRow(2, chromaRow).CopyTo(planes.GetRow(2, row));
                }
            }
            return new PixelSource(this, this.colorSpace, convertedPlanes: planes);
        }
        catch
        {
            planes.Dispose();
            throw;
        }
        finally
        {
            if (scratch is not null)
            {
                ArrayPool<float>.Shared.Return(scratch);
            }
            if (reservedScratchBytes != 0)
            {
                cacheBudget?.Release(reservedScratchBytes, EncoderCacheKind.ConvertedSamples);
            }
        }
    }

    private void ConvertContiguous420Rows(ConvertedPlanes planes, int width, int height, int stride,
        Span<float> scratch, bool vectorRgb, CancellationToken cancellationToken)
    {
        Span<float> luma = planes.GetPlane(0), cb = planes.GetPlane(1), cr = planes.GetPlane(2);
        Span<float> firstCb = scratch[..width], firstCr = scratch.Slice(width, width);
        Span<float> secondCb = scratch.Slice(2 * width, width), secondCr = scratch.Slice(3 * width, width);
        int chromaWidth = planes.ChromaWidth;
        int chromaRow = 0;
        for (int y = 0; y < height; y += 2, chromaRow++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ConvertRow(y * stride, luma.Slice(y * width, width), firstCb, firstCr, vectorRgb);
            if (y + 1 < height)
            {
                this.ConvertRow((y + 1) * stride, luma.Slice((y + 1) * width, width), secondCb, secondCr, vectorRgb);
            }
            else
            {
                firstCb.CopyTo(secondCb);
                firstCr.CopyTo(secondCr);
            }
            DownsampleConvertedRow(firstCb, secondCb, cb.Slice(chromaRow * chromaWidth, chromaWidth));
            DownsampleConvertedRow(firstCr, secondCr, cr.Slice(chromaRow * chromaWidth, chromaWidth));
        }
        if (chromaRow < planes.ChromaHeight)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Span<float> finalCb = cb.Slice(chromaRow * chromaWidth, chromaWidth);
            Span<float> finalCr = cr.Slice(chromaRow * chromaWidth, chromaWidth);
            DownsampleConvertedRow(secondCb, secondCb, finalCb);
            DownsampleConvertedRow(secondCr, secondCr, finalCr);
            for (int row = chromaRow + 1; row < planes.ChromaHeight; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                finalCb.CopyTo(cb.Slice(row * chromaWidth, chromaWidth));
                finalCr.CopyTo(cr.Slice(row * chromaWidth, chromaWidth));
            }
        }
    }

    private static void DownsampleConvertedRow(ReadOnlySpan<float> first, ReadOnlySpan<float> second, Span<float> output)
    {
        int width = first.Length;
        int x = 0;
#if !NETSTANDARD2_1
        if (Vector256.IsHardwareAccelerated)
        {
            for (; x * 2 <= width - 16; x += 8)
            {
                ((Downsampling.PairwiseSums256(first.Slice(x * 2, 16))
                    + Downsampling.PairwiseSums256(second.Slice(x * 2, 16))) * Vector256.Create(0.25f)).CopyTo(output[x..]);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; x * 2 <= width - 8; x += 4)
            {
                ((Downsampling.PairwiseSums128(first.Slice(x * 2, 8))
                    + Downsampling.PairwiseSums128(second.Slice(x * 2, 8))) * Vector128.Create(0.25f)).CopyTo(output[x..]);
            }
        }
#endif
        for (; x < (width + 1) / 2; x++)
        {
            int left = x * 2, right = Math.Min(left + 1, width - 1);
            output[x] = (first[left] + first[right] + (second[left] + second[right])) * 0.25f;
        }
        float padding = (first[^1] + first[^1] + (second[^1] + second[^1])) * 0.25f;
        output[x..].Fill(padding);
    }

    private void ConvertRow(int rowStart, Span<float> luma, Span<float> cb, Span<float> cr, bool vectorRgb)
    {
#if NETSTANDARD2_1
        int converted = vectorRgb ? ConvertByteRow(rowStart, luma, cb, cr) : 0;
#else
        int converted = vectorRgb ? this.ConvertByteRow(rowStart, luma, cb, cr) : 0;
#endif
        for (int x = converted; x < luma.Length; x++)
        {
            this.ReadConvertedComponents(rowStart + (x * this.ComponentsPerPixel), out float first, out float second, out float third);
            luma[x] = first;
            if (!cb.IsEmpty)
            {
                cb[x] = second;
                cr[x] = third;
            }
        }
    }

#if !NETSTANDARD2_1
    private int ConvertByteRow(int rowStart, Span<float> lumaSamples, Span<float> cbSamples, Span<float> crSamples)
    {
        int lanes = Vector<float>.Count;
        int step = this.ComponentsPerPixel;
        int width = lumaSamples.Length;
        int x = 0;
        if (step == 4)
        {
            var byteMask = new Vector<uint>(0xffU);
            int vectorBytes = lanes * 4;
            for (; x <= width - lanes; x += lanes)
            {
                var packed = Vector.AsVectorUInt32(new Vector<byte>(this.bytes.Slice(rowStart + (x * 4), vectorBytes)));
                var c0 = Vector.ConvertToSingle(Vector.AsVectorInt32(packed & byteMask));
                var green = Vector.ConvertToSingle(Vector.AsVectorInt32((packed >> 8) & byteMask));
                var c2 = Vector.ConvertToSingle(Vector.AsVectorInt32((packed >> 16) & byteMask));
                ColorConversion.RgbToYCbCr(this.bgr ? c2 : c0, green, this.bgr ? c0 : c2,
                    out Vector<float> luma, out Vector<float> cb, out Vector<float> cr);
                luma.CopyTo(lumaSamples.Slice(x, lanes));
                if (!cbSamples.IsEmpty)
                {
                    cb.CopyTo(cbSamples.Slice(x, lanes));
                    cr.CopyTo(crSamples.Slice(x, lanes));
                }
            }
            return x;
        }

        Vector128<byte> redIndices = ComponentIndices(this.bgr ? 2 : 0, step);
        Vector128<byte> greenIndices = ComponentIndices(1, step);
        Vector128<byte> blueIndices = ComponentIndices(this.bgr ? 0 : 2, step);
        int upperBackstep = step == 3 ? 4 : 0;
        Vector128<byte> upperRedIndices = ComponentIndices((this.bgr ? 2 : 0) + upperBackstep, step);
        Vector128<byte> upperGreenIndices = ComponentIndices(1 + upperBackstep, step);
        Vector128<byte> upperBlueIndices = ComponentIndices((this.bgr ? 0 : 2) + upperBackstep, step);
        // Overlap the two RGB loads so an eight-pixel group consumes exactly
        // 24 bytes. Bound every load by the active row, including the final
        // unpadded row; leave remaining pixels to the scalar path.
        int bytesNeeded = lanes == 8 ? 8 * step : 16;
        for (; x <= width - lanes && x * step <= (width * step) - bytesNeeded; x += lanes)
        {
            var lower = Vector128.Create(this.bytes.Slice(rowStart + (x * step), 16));
            // Each mask selects one byte and zeros the other three bytes in
            // each lane. Values are 0..255, so signed conversion is exact too.
            Vector128<int> r0 = Vector128.Shuffle(lower, redIndices).AsInt32();
            Vector128<int> g0 = Vector128.Shuffle(lower, greenIndices).AsInt32();
            Vector128<int> b0 = Vector128.Shuffle(lower, blueIndices).AsInt32();
            Vector<float> red, green, blue;
            if (lanes == 8)
            {
                var upper = Vector128.Create(this.bytes.Slice(rowStart + ((x + 4) * step) - upperBackstep, 16));
                // Assemble integer lanes before conversion, so each channel
                // needs one full-width conversion rather than two half-width ones.
                red = Vector256.ConvertToSingle(Vector256.Create(r0, Vector128.Shuffle(upper, upperRedIndices).AsInt32())).AsVector();
                green = Vector256.ConvertToSingle(Vector256.Create(g0, Vector128.Shuffle(upper, upperGreenIndices).AsInt32())).AsVector();
                blue = Vector256.ConvertToSingle(Vector256.Create(b0, Vector128.Shuffle(upper, upperBlueIndices).AsInt32())).AsVector();
            }
            else
            {
                red = Vector128.ConvertToSingle(r0).AsVector();
                green = Vector128.ConvertToSingle(g0).AsVector();
                blue = Vector128.ConvertToSingle(b0).AsVector();
            }
            ColorConversion.RgbToYCbCr(red, green, blue,
                out Vector<float> luma, out Vector<float> cb, out Vector<float> cr);
            luma.CopyTo(lumaSamples.Slice(x, lanes));
            if (!cbSamples.IsEmpty)
            {
                cb.CopyTo(cbSamples.Slice(x, lanes));
                cr.CopyTo(crSamples.Slice(x, lanes));
            }
        }
        return x;
    }

    private static Vector128<byte> ComponentIndices(int component, int step)
        => Vector128.Create(0xffffff00U | (uint)component, 0xffffff00U | (uint)(component + step),
            0xffffff00U | (uint)(component + (2 * step)), 0xffffff00U | (uint)(component + (3 * step))).AsByte();

#else
    // The portable fallback shares the vector implementation's call signature.
#pragma warning disable IDE0060
    private static int ConvertByteRow(int rowStart, Span<float> lumaSamples, Span<float> cbSamples, Span<float> crSamples) => 0;
#pragma warning restore IDE0060

#endif
    private sealed class ConvertedPlanes(int width, int height, int sourceStride, int sourceLength,
        bool is420, bool separate, int cachedHeight, int segmentRows, EncoderCacheBudget? cacheBudget) : IDisposable
    {
        internal const long MaximumBytes = 64L * 1024 * 1024;
        private float[] samples = [];
        private float[]? cb;
        private float[]? cr;
        private float[][][]? segments;
        internal int Width { get; } = width;
        internal int SourceStride { get; } = sourceStride;
        internal int SourceLength { get; } = sourceLength;
        internal int PlaneLength { get; } = checked(width * cachedHeight);
        internal int CachedHeight { get; } = cachedHeight;
        internal bool IsComplete => this.CachedHeight == height;
        internal bool IsContiguous => segmentRows == 0 && this.IsComplete;
        internal bool Is420 { get; } = is420;
        internal int ChromaWidth { get; } = is420 ? checked((width + 15) / 16 * 8) : width;
        internal int ChromaHeight { get; } = is420 ? checked((height + 15) / 16 * 8) : height;
        internal int CachedChromaHeight { get; } = is420 ? cachedHeight == height ? checked((height + 15) / 16 * 8) : cachedHeight / 2 : cachedHeight;
        internal int ChromaLength => checked(this.ChromaWidth * this.CachedChromaHeight);
        internal long RentedBytes { get; private set; }

        internal Span<float> GetPlane(int component)
            => component == 0 ? this.samples.AsSpan(0, this.PlaneLength)
                : separate ? (component == 1 ? this.cb : this.cr).AsSpan(0, this.ChromaLength)
                : this.samples.AsSpan(this.PlaneLength + ((component - 1) * this.ChromaLength), this.ChromaLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal float GetSample(int component, int index)
        {
            if (this.segments is not null)
            {
                int segmentLength = component == 0 || !this.Is420 ? segmentRows * this.Width : segmentRows / 2 * this.ChromaWidth;
                return this.segments[component][index / segmentLength][index % segmentLength];
            }
            return separate && component != 0 ? (component == 1 ? this.cb! : this.cr!)[index]
                : this.samples[(component * this.PlaneLength) + index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span<float> GetRow(int component, int row)
        {
            if (this.segments is null)
            {
                int width = component == 0 ? this.Width : this.ChromaWidth;
                return this.GetPlane(component).Slice(row * width, width);
            }
            Span<float> segment = this.GetSegment(component, row, out int rowWidth, out _, out int localRow);
            return segment.Slice(localRow * rowWidth, rowWidth);
        }

        internal Span<float> GetSegment(int component, int row, out int rowWidth, out int rowCount, out int localRow)
        {
            rowWidth = component == 0 ? this.Width : this.ChromaWidth;
            int cachedRows = component == 0 ? this.CachedHeight : this.CachedChromaHeight;
            if ((uint)row >= (uint)cachedRows)
            {
                rowCount = localRow = 0;
                return default;
            }
            if (this.segments is null)
            {
                rowCount = cachedRows;
                localRow = row;
                return this.GetPlane(component);
            }
            int rowsPerSegment = component != 0 && this.Is420 ? segmentRows / 2 : segmentRows;
            int segment = row / rowsPerSegment;
            localRow = row % rowsPerSegment;
            rowCount = Math.Min(rowsPerSegment, cachedRows - (segment * rowsPerSegment));
            return this.segments[component][segment].AsSpan(0, rowCount * rowWidth);
        }

        internal static long PooledBytes(long count)
            => (long)Math.Max(16UL, BitOperations.RoundUpToPowerOf2((ulong)count)) * sizeof(float);

        internal static ConvertedPlanes? TryCreate(int width, int height, int sourceStride, int sourceLength,
            int components, bool is420, long transientBytes, EncoderCacheBudget? cacheBudget,
            bool allowPartial, CancellationToken cancellationToken)
        {
            long planeLength = (long)width * height;
            long chromaLength = is420 ? (long)((width + 15) / 16 * 8) * ((height + 15) / 16 * 8) : planeLength;
            long totalLength = planeLength + ((components - 1) * chromaLength);
            long availableBytes = Math.Min(MaximumBytes, cacheBudget?.AvailableBytes ?? MaximumBytes) - transientBytes;
            bool separate = PooledBytes(totalLength) > availableBytes;
            int cachedHeight = height;
            int segmentRows = 0;
            if (separate && PooledBytes(planeLength) + ((components - 1) * PooledBytes(chromaLength)) > availableBytes)
            {
                if (!allowPartial)
                {
                    return null;
                }
                segmentRows = Math.Max(16, 65536 / width / 16 * 16);
                int totalGroups = (height + 15) / 16;
                int lower = 0, upper = totalGroups;
                while (lower < upper)
                {
                    int middle = lower + ((upper - lower + 1) / 2);
                    int rows = Math.Min(height, middle * 16);
                    if (SegmentedBytes(width, height, rows, components, is420, segmentRows) <= availableBytes)
                    {
                        lower = middle;
                    }
                    else
                    {
                        upper = middle - 1;
                    }
                }
                if (lower == 0)
                {
                    return null;
                }
                cachedHeight = Math.Min(height, lower * 16);
            }

            // Keep the ordinary single rental, but avoid rejecting useful planes
            // solely because their combined power-of-two bucket is too large.
            var planes = new ConvertedPlanes(width, height, sourceStride, sourceLength,
                is420, separate, cachedHeight, segmentRows, cacheBudget);
            try
            {
                if (segmentRows != 0)
                {
                    if (!planes.AllocateSegments(components, transientBytes, cancellationToken))
                    {
                        planes.Dispose();
                        return null;
                    }
                }
                else if (!planes.TryRent(checked((int)(separate ? planeLength : totalLength)), transientBytes, out planes.samples)
                    || (separate && components > 1
                        && (!planes.TryRent(checked((int)chromaLength), transientBytes, out planes.cb)
                            || !planes.TryRent(checked((int)chromaLength), transientBytes, out planes.cr))))
                {
                    planes.Dispose();
                    return null;
                }
                cacheBudget?.RecordConvertedRows(cachedHeight, height);
                return planes;
            }
            catch
            {
                planes.Dispose();
                throw;
            }
        }

        private static long SegmentedBytes(int width, int height, int cachedHeight, int components, bool is420, int segmentRows)
        {
            long bytes = 0;
            for (int component = 0; component < components; component++)
            {
                bool chroma = component != 0 && is420;
                int rowWidth = chroma ? (width + 15) / 16 * 8 : width;
                int rows = chroma ? cachedHeight == height ? (height + 15) / 16 * 8 : cachedHeight / 2 : cachedHeight;
                int step = chroma ? segmentRows / 2 : segmentRows;
                bytes += rows / step * PooledBytes((long)rowWidth * step);
                if (rows % step != 0)
                {
                    bytes += PooledBytes((long)rowWidth * (rows % step));
                }
            }
            return bytes;
        }

        private bool AllocateSegments(int components, long transientBytes, CancellationToken cancellationToken)
        {
            this.segments = new float[components][][];
            for (int component = 0; component < components; component++)
            {
                bool chroma = component != 0 && this.Is420;
                int rowWidth = chroma ? this.ChromaWidth : this.Width;
                int rows = chroma ? this.CachedChromaHeight : this.CachedHeight;
                int step = chroma ? segmentRows / 2 : segmentRows;
                int count = (rows + step - 1) / step;
                float[][] chunks = new float[count][];
                this.segments[component] = chunks;
                for (int index = 0; index < count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int length = checked(rowWidth * Math.Min(step, rows - (index * step)));
                    if (!this.TryRent(length, transientBytes, out chunks[index]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool TryRent(int count, long transientBytes, out float[] buffer)
        {
            float[] rental = ArrayPool<float>.Shared.Rent(count);
            long bytes = (long)rental.Length * sizeof(float);
            if (bytes > MaximumBytes - transientBytes - this.RentedBytes
                || (cacheBudget is not null && !cacheBudget.TryReserve(bytes, EncoderCacheKind.ConvertedSamples)))
            {
                ArrayPool<float>.Shared.Return(rental);
                buffer = [];
                return false;
            }
            this.RentedBytes += bytes;
            buffer = rental;
            return true;
        }

        public void Dispose()
        {
            if (this.segments is not null)
            {
                foreach (float[][]? component in this.segments)
                {
                    if (component is null)
                    {
                        continue;
                    }
                    foreach (float[]? chunk in component)
                    {
                        if (chunk is { Length: > 0 })
                        {
                            ArrayPool<float>.Shared.Return(chunk);
                        }
                    }
                }
                this.segments = null;
            }
            if (this.samples.Length != 0)
            {
                ArrayPool<float>.Shared.Return(this.samples);
                this.samples = [];
            }
            if (this.cb is { Length: > 0 })
            {
                ArrayPool<float>.Shared.Return(this.cb);
                this.cb = null;
            }
            if (this.cr is { Length: > 0 })
            {
                ArrayPool<float>.Shared.Return(this.cr);
                this.cr = null;
            }
            if (this.RentedBytes != 0)
            {
                cacheBudget?.Release(this.RentedBytes, EncoderCacheKind.ConvertedSamples);
                this.RentedBytes = 0;
            }
        }
    }

    /// <summary>Workers own source copies or borrow immutable planes for the encode lease's lifetime.</summary>
    internal sealed class Snapshot
    {
        private readonly union SampleStorage(byte[], ushort[], float[]);

        private readonly SampleStorage samples;
        private readonly bool bgr;
        private readonly bool ycbcr;
        private readonly bool xyb;
        private readonly JpegColorSpace colorSpace;
        private readonly int componentsPerPixel;
        private readonly IccColorTransform? colorTransform;
        private readonly ConvertedPlanes? convertedPlanes;

        internal Snapshot(PixelSource source, int length, CancellationToken cancellationToken,
            bool borrowCompactPlanes = false)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.convertedPlanes = source.convertedPlanes;
            // Compact chroma planes do not retain arbitrary full-resolution
            // chroma samples. Only DCT-only consumers can omit their fallback
            // source copy; ordinary snapshots preserve those raw reads.
            // Even when the planes replace the raw samples, the empty array's
            // case preserves the source precision without a separate tag.
            this.samples = source.sourceType switch
            {
                SourceType.Byte => (this.convertedPlanes is not { IsComplete: true } || (this.convertedPlanes.Is420 && !borrowCompactPlanes))
                    ? Copy(source.bytes, length, cancellationToken) : [],
                SourceType.UInt16 => this.convertedPlanes is not { IsComplete: true }
                    ? Copy(source.words, length, cancellationToken) : [],
                SourceType.Float => this.convertedPlanes is not { IsComplete: true }
                    ? Copy(source.floats, length, cancellationToken) : [],
                _ => throw new InvalidOperationException("The source sample precision is invalid.")
            };
            this.bgr = source.bgr;
            this.ycbcr = source.ycbcr;
            this.xyb = source.xyb;
            this.colorSpace = source.colorSpace;
            this.componentsPerPixel = source.ComponentsPerPixel;
            this.colorTransform = source.colorTransform;
        }

        internal PixelSource GetPixels()
        {
            // Reconstruct with the original precision and layout. In particular,
            // normalizing ushort input into floats would change rounding and XYB.
            PixelSource pixels = this.samples switch
            {
                byte[] bytes => new PixelSource(bytes, this.xyb ? PixelLayout.Xyb24
                    : this.ycbcr ? PixelLayout.YCbCr24 : this.componentsPerPixel == 1 ? PixelLayout.Gray8
                    : this.componentsPerPixel == 4 ? (this.bgr ? PixelLayout.Bgra32 : PixelLayout.Rgba32)
                    : this.bgr ? PixelLayout.Bgr24 : PixelLayout.Rgb24),
                ushort[] words => new PixelSource(words, this.GetLayout()),
                float[] floats => new PixelSource(floats, this.GetLayout()),
                null => throw new InvalidOperationException("The snapshot has no sample storage.")
            };
            pixels = pixels.WithColorSpace(this.colorSpace);
            if (this.colorTransform is not null)
            {
                pixels = pixels.WithColorTransform(this.colorTransform);
            }

            return this.convertedPlanes is null ? pixels : new PixelSource(pixels, this.colorSpace, convertedPlanes: this.convertedPlanes);
        }

        private PixelComponentLayout GetLayout() => this.xyb ? PixelComponentLayout.Xyb
            : this.ycbcr ? PixelComponentLayout.YCbCr : this.componentsPerPixel == 1 ? PixelComponentLayout.Gray
            : this.componentsPerPixel == 4 ? (this.bgr ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgba)
            : this.bgr ? PixelComponentLayout.Bgr : PixelComponentLayout.Rgb;

        private static T[] Copy<T>(ReadOnlySpan<T> source, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // A complete copy is returned only after every element is initialized.
#if NETSTANDARD2_1
            var copy = new T[length];
#else
            T[] copy = GC.AllocateUninitializedArray<T>(length);
#endif
            for (int offset = 0; offset < length; offset += Math.Min(65536, length - offset))
            {
                cancellationToken.ThrowIfCancellationRequested();
                int count = Math.Min(65536, length - offset);
                source.Slice(offset, count).CopyTo(copy.AsSpan(offset, count));
            }
            return copy;
        }
    }

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly",
        Justification = "Validation reports the caller's ColorSpace option rather than this internal adapter parameter.")]
    internal PixelSource WithColorSpace(JpegColorSpace outputColorSpace)
    {
        if (this.xyb && outputColorSpace is not (JpegColorSpace.Automatic or JpegColorSpace.Xyb))
        {
            throw new ArgumentException("Already scaled XYB input cannot be converted to another output color space.",
                nameof(JpegliEncoderOptions.ColorSpace));
        }

        if (outputColorSpace == JpegColorSpace.Xyb && this.ycbcr)
        {
            throw new ArgumentException("Direct YCbCr input cannot be converted to XYB output.",
                nameof(JpegliEncoderOptions.ColorSpace));
        }

        if (outputColorSpace == JpegColorSpace.Rgb && this.ycbcr)
        {
            throw new ArgumentException("Direct YCbCr input cannot be converted to RGB output.",
                nameof(JpegliEncoderOptions.ColorSpace));
        }

        return new PixelSource(this, outputColorSpace, discardConvertedPlanes: true);
    }
    internal int Length => this.convertedPlanes?.SourceLength ?? (this.sourceType switch
    {
        SourceType.Byte => this.bytes.Length,
        SourceType.UInt16 => this.words.Length,
        SourceType.Float => this.floats.Length,
        _ => this.floats.Length
    });

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly",
        Justification = "The stored samples originate from the public Encode pixels argument, which remains the reported parameter.")]
    internal bool ValidateSamples(int width, int height, int stride, CancellationToken cancellationToken,
        bool allowOutOfRangeFloatSamples = false)
    {
        if (this.sourceType != SourceType.Float)
        {
            return false;
        }

        // Check only consumed components. Padding, trailing storage and alpha
        // are intentionally ignored, just as in the byte input path.
        int activeComponents = this.IsGrayscaleInput || (this.ycbcr && this.IsGrayscale) ? 1 : 3;
        bool hasOutOfRangeSamples = false;
        for (int row = 0; row < height; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rowStart = row * stride;
            for (int x = 0; x < width; x++)
            {
                int offset = rowStart + (x * this.ComponentsPerPixel);
                for (int component = 0; component < activeComponents; component++)
                {
                    float value = this.floats[offset + component];
                    if (!float.IsFinite(value) || !float.IsFinite(value * 255f))
                    {
                        throw new ArgumentException("Floating-point color and grayscale samples and their scaling to JPEG units must be finite.", "pixels");
                    }

                    if (value is < 0f or > 1f)
                    {
                        if (!allowOutOfRangeFloatSamples)
                        {
                            throw new ArgumentException("Floating-point color and grayscale samples must be between 0 and 1. AllowOutOfRangeFloatSamples permits wider values only without SourceIccProfile conversion.", "pixels");
                        }

                        hasOutOfRangeSamples = true;
                    }
                }
                // Validate the nonlinear transform before any output, including
                // when a finite source sample would overflow sRGB linearization.
                if (this.IsXyb && !this.xyb)
                {
                    _ = this.ReadSample(offset, 0);
                }
            }
        }
        return hasOutOfRangeSamples;
    }

    internal float ReadSample(int offset, int component)
    {
        if (this.convertedPlanes is not null && (!this.convertedPlanes.Is420 || component == 0))
        {
            int row = offset / this.convertedPlanes.SourceStride;
            if ((uint)row < (uint)this.convertedPlanes.CachedHeight)
            {
                int column = (offset - (row * this.convertedPlanes.SourceStride)) / this.ComponentsPerPixel;
                return this.convertedPlanes.GetSample(component, (row * this.convertedPlanes.Width) + column);
            }
        }
        if (this.colorTransform is not null)
        {
            this.ReadManagedRgb(offset, out float red, out float green, out float blue);
            if (this.IsXyb)
            {
                XybColorConversion.SrgbToScaledXyb(red, green, blue, out float scaledX, out float scaledY, out float scaledB);
                return component switch { 0 => scaledX, 1 => scaledY, _ => scaledB };
            }
            if (this.IsGrayscale)
            {
                return red * 255f;
            }

            if (this.IsRgb)
            {
                return (component switch { 0 => red, 1 => green, _ => blue }) * 255f;
            }

            ColorConversion.RgbToYCbCr(red * 255f, green * 255f, blue * 255f,
                out float luma, out float managedCb, out float managedCr);
            return component switch { 0 => luma, 1 => managedCb, _ => managedCr };
        }
        if (this.IsXyb && !this.xyb)
        {
            this.ReadRawScaledXyb(offset, out float scaledX, out float scaledY, out float scaledB);
            return component switch { 0 => scaledX, 1 => scaledY, _ => scaledB };
        }
        if (this.IsGrayscaleInput)
        {
            return this.ColorSpace == JpegColorSpace.YCbCr && component > 0 ? 128f : this.ReadComponent(offset);
        }
        // ChooseColorTransform in the pinned upstream uses NullTransform when
        // both input and output are JCS_YCbCr. Read the supplied component after
        // the same input scaling; smoothing and subsampling still follow below.
        if (this.ycbcr)
        {
            return this.ReadComponent(offset + component);
        }

        // RGB JPEGs preserve the source values, normalizing BGR order and ignoring alpha.
        if (this.IsRgb)
        {
            return this.ReadComponent(offset + (this.bgr ? 2 - component : component));
        }

        float r = this.ReadComponent(offset + (this.bgr ? 2 : 0));
        float g = this.ReadComponent(offset + 1);
        float b = this.ReadComponent(offset + (this.bgr ? 0 : 2));
        ColorConversion.RgbToYCbCr(r, g, b, out float y, out float cb, out float cr);
        return component switch { 0 => y, 1 => cb, _ => cr };
    }

    // The coordinate path avoids dividing an interleaved source offset for cached samples.
    internal float ReadSampleAt(int x, int y, int stride, int component)
        => this.convertedPlanes is null || (uint)y >= (uint)this.convertedPlanes.CachedHeight
            || (this.convertedPlanes.Is420 && component != 0)
            ? this.ReadSample((y * stride) + (x * this.ComponentsPerPixel), component)
            : this.convertedPlanes.GetSample(component, (y * this.convertedPlanes.Width) + x);

    private void ReadConvertedComponents(int offset, out float first, out float second, out float third)
    {
        if (this.colorTransform is not null || (this.IsXyb && !this.xyb))
        {
            float red, green, blue;
            if (this.colorTransform is not null)
            {
                this.ReadManagedRgb(offset, out red, out green, out blue);
            }
            else
            {
                this.ReadRawScaledXyb(offset, out first, out second, out third);
                return;
            }

            if (this.IsXyb)
            {
                XybColorConversion.SrgbToScaledXyb(red, green, blue, out first, out second, out third);
                return;
            }
            first = red * 255f;
            second = green * 255f;
            third = blue * 255f;
            if (!this.IsGrayscale && !this.IsRgb)
            {
                ColorConversion.RgbToYCbCr(first, second, third, out first, out second, out third);
            }

            return;
        }
        if (this.IsGrayscaleInput)
        {
            first = this.ReadComponent(offset);
            second = third = this.ColorSpace == JpegColorSpace.YCbCr ? 128f : first;
            return;
        }
        if (this.ycbcr)
        {
            first = this.ReadComponent(offset);
            second = this.ReadComponent(offset + 1);
            third = this.ReadComponent(offset + 2);
            return;
        }
        float r = this.ReadComponent(offset + (this.bgr ? 2 : 0));
        float g = this.ReadComponent(offset + 1);
        float b = this.ReadComponent(offset + (this.bgr ? 0 : 2));
        if (this.IsRgb)
        {
            first = r;
            second = g;
            third = b;
        }
        else
        {
            ColorConversion.RgbToYCbCr(r, g, b, out first, out second, out third);
        }
    }

    private void ReadRawScaledXyb(int offset, out float first, out float second, out float third)
    {
        int redOffset = offset + (this.IsGrayscaleInput ? 0 : this.bgr ? 2 : 0);
        int blueOffset = this.IsGrayscaleInput ? offset : offset + (this.bgr ? 0 : 2);
        if (this.sourceType == SourceType.Byte)
        {
            byte red = this.bytes[redOffset];
            byte green = this.IsGrayscaleInput ? red : this.bytes[offset + 1];
            XybColorConversion.SrgbBytesToScaledXyb(red, green, this.bytes[blueOffset], out first, out second, out third);
            return;
        }
        float r = this.ReadNormalizedComponent(redOffset);
        float g = this.IsGrayscaleInput ? r : this.ReadNormalizedComponent(offset + 1);
        float b = this.IsGrayscaleInput ? r : this.ReadNormalizedComponent(blueOffset);
        XybColorConversion.SrgbToScaledXyb(r, g, b, out first, out second, out third);
    }

    // Read only a bounded source row, including replicated edge pixels.
    // Gathering preserves ushort's double normalization, float scaling,
    // BGR component order and the omission of alpha/padding components.
    internal void ReadRowScalar(int width, int height, int stride, int component,
        int x, int y, Span<float> samples)
    {
        if (this.convertedPlanes is not null && (!this.convertedPlanes.Is420 || component == 0)
            && Math.Clamp(y, 0, height - 1) < this.convertedPlanes.CachedHeight)
        {
            this.ReadConvertedRow(width, height, component, x, y, samples);
            return;
        }
        int rowStart = Math.Clamp(y, 0, height - 1) * stride;
        for (int column = 0; column < samples.Length; column++)
        {
            int offset = rowStart + (Math.Clamp(x + column, 0, width - 1) * this.ComponentsPerPixel);
            samples[column] = this.ReadSample(offset, component);
        }
    }

    internal void ReadRowVector(int width, int height, int stride, int component,
        int x, int y, Span<float> samples)
    {
        if (this.convertedPlanes is not null && (!this.convertedPlanes.Is420 || component == 0)
            && Math.Clamp(y, 0, height - 1) < this.convertedPlanes.CachedHeight)
        {
            this.ReadConvertedRow(width, height, component, x, y, samples);
            return;
        }
        int lanes = Vector<float>.Count;
        // Nonlinear XYB and null transforms retain their existing scalar
        // semantics. Bound scratch space even if a future runtime has wider vectors.
        if (this.colorTransform is not null || this.IsXyb || this.IsGrayscaleInput || this.ycbcr || this.IsRgb || lanes > 8 || samples.Length < lanes)
        {
            this.ReadRowScalar(width, height, stride, component, x, y, samples);
            return;
        }

        Span<float> gathered = stackalloc float[24];
        Span<float> red = gathered[..lanes];
        Span<float> green = gathered.Slice(lanes, lanes);
        Span<float> blue = gathered.Slice(2 * lanes, lanes);
        int rowStart = Math.Clamp(y, 0, height - 1) * stride;
        int column = 0;
        for (; column <= samples.Length - lanes; column += lanes)
        {
            for (int lane = 0; lane < lanes; lane++)
            {
                int offset = rowStart + (Math.Clamp(x + column + lane, 0, width - 1) * this.ComponentsPerPixel);
                red[lane] = this.ReadComponent(offset + (this.bgr ? 2 : 0));
                green[lane] = this.ReadComponent(offset + 1);
                blue[lane] = this.ReadComponent(offset + (this.bgr ? 0 : 2));
            }
            ColorConversion.RgbToYCbCr(new Vector<float>(red), new Vector<float>(green), new Vector<float>(blue),
                out Vector<float> luma, out Vector<float> cb, out Vector<float> cr);
            Vector<float> converted = component switch { 0 => luma, 1 => cb, _ => cr };
            converted.CopyTo(samples.Slice(column, lanes));
        }

        // A row can end between vector boundaries, without reading source padding.
        if (column != samples.Length)
        {
            this.ReadRowScalar(width, height, stride, component, x + column, y, samples[column..]);
        }
    }

    private void ReadConvertedRow(int width, int height, int component, int x, int y, Span<float> samples)
    {
        ReadOnlySpan<float> row = this.convertedPlanes!.GetRow(component, Math.Clamp(y, 0, height - 1));
        if (x < 0)
        {
            int prefix = (int)Math.Min(samples.Length, -(long)x);
            samples[..prefix].Fill(row[0]);
            samples = samples[prefix..];
            x = 0;
        }
        if (x < width)
        {
            int count = Math.Min(samples.Length, width - x);
            row.Slice(x, count).CopyTo(samples);
            samples = samples[count..];
        }
        samples.Fill(row[^1]);
    }

    private float ReadComponent(int offset) => this.sourceType switch
    {
        SourceType.Byte => this.bytes[offset],
        // Scalar ReadUint16Row and ReadFloatRow in lib/jpegli/input.cc,
        // revision 031a0077f5799a6041004267fc12b956c1f52a20.
        // Upstream's double reciprocal matters before conversion to float.
        SourceType.UInt16 => (float)(this.words[offset] * (1.0 / 257.0)),
        SourceType.Float => this.floats[offset] * 255f,
        _ => this.floats[offset] * 255f
    };

    // ToFloatRow in upstream's extras encoder normalizes directly to 0..1
    // before its color transform. Avoid a rounded JPEG-unit intermediate.
    private float ReadNormalizedComponent(int offset) => this.sourceType switch
    {
        SourceType.Byte => (float)(this.bytes[offset] * (1.0 / 255.0)),
        SourceType.UInt16 => (float)(this.words[offset] * (1.0 / 65535.0)),
        SourceType.Float => this.floats[offset],
        _ => this.floats[offset]
    };

    private void ReadManagedRgb(int offset, out float red, out float green, out float blue)
    {
        int redOffset = offset + (this.IsGrayscaleInput ? 0 : this.bgr ? 2 : 0);
        int blueOffset = offset + (this.bgr ? 0 : 2);
        if (this.sourceType == SourceType.Byte)
        {
            byte first = this.bytes[redOffset];
            this.colorTransform!.ConvertByte(first, this.IsGrayscaleInput ? first : this.bytes[offset + 1],
                this.IsGrayscaleInput ? first : this.bytes[blueOffset], out red, out green, out blue);
        }
        else
        {
            red = this.ReadNormalizedComponent(redOffset);
            green = this.IsGrayscaleInput ? red : this.ReadNormalizedComponent(offset + 1);
            blue = this.IsGrayscaleInput ? red : this.ReadNormalizedComponent(blueOffset);
            this.colorTransform!.Convert(red, green, blue, out red, out green, out blue);
        }
    }

    private static int GetComponentCount(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Rgb or PixelComponentLayout.Bgr or PixelComponentLayout.YCbCr or PixelComponentLayout.Xyb => 3,
        PixelComponentLayout.Rgba or PixelComponentLayout.Bgra => 4,
        PixelComponentLayout.Gray => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(layout))
    };

    private enum SourceType { Byte, UInt16, Float }
}
