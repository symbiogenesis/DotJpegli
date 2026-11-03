namespace DotJpegli.Internal;

using System.Runtime.ExceptionServices;

/// <summary>Parallel sample conversion and DCT with a single bounded MCU row of results.</summary>
internal sealed class ParallelDctRows
{
    private readonly PixelSource.Snapshot source;
    private readonly int width;
    private readonly int height;
    private readonly int stride;
    private readonly JpegSamplingFactors sampling;
    private readonly int smoothingFactor;
    private readonly int maxDegreeOfParallelism;
    private readonly int[] blockOffsets;
    private readonly int[] blocksWide;
    private readonly int[] blocksHigh;
    private readonly float[] coefficients;
    private readonly ExceptionDispatchInfo?[] errors;
    private int preparedTop = -1;
    private int preparedComponents;

    internal ParallelDctRows(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, int smoothingFactor, int maxDegreeOfParallelism,
        CancellationToken cancellationToken)
    {
        this.width = width;
        this.height = height;
        this.stride = stride;
        this.sampling = sampling;
        this.smoothingFactor = smoothingFactor;
        this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        int componentCount = pixels.IsGrayscale ? 1 : 3;
        this.blockOffsets = new int[componentCount + 1];
        this.blocksWide = new int[componentCount];
        this.blocksHigh = new int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            JpegComponentSampling factor = sampling[component];
            this.blocksWide[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal);
            this.blocksHigh[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical);
            this.blockOffsets[component + 1] = checked(this.blockOffsets[component] + (this.blocksWide[component] * factor.Vertical));
        }
        this.coefficients = new float[checked(this.blockOffsets[^1] * 64)];
        this.errors = new ExceptionDispatchInfo?[this.blockOffsets[^1]];
        int length = checked(((height - 1) * stride) + (width * pixels.ComponentsPerPixel));
        this.source = new PixelSource.Snapshot(pixels, length, cancellationToken);
    }

    internal void PrepareRow(int top, CancellationToken cancellationToken, int singleComponent = -1)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int componentMask = singleComponent < 0 ? (1 << this.blocksWide.Length) - 1 : 1 << singleComponent;
        if (this.preparedTop == top && (this.preparedComponents & componentMask) == componentMask)
        {
            return;
        }

        if (this.preparedTop != top)
        {
            this.preparedComponents = 0;
        }
        // Separate-component scans only need that component's row. Computing
        // the other components would multiply progressive/refinement DCT work.
        int firstBlock = singleComponent < 0 ? 0 : this.blockOffsets[singleComponent];
        int endBlock = singleComponent < 0 ? this.blockOffsets[^1] : this.blockOffsets[singleComponent + 1];
        Array.Clear(this.errors, firstBlock, endBlock - firstBlock);
        int mcuRow = top / (8 * this.sampling.MaxVertical);
        // Each invocation joins all workers before exposing the row to the
        // ordered consumer. No workers survive cancellation or an output error.
        _ = Parallel.For(firstBlock, endBlock, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(this.maxDegreeOfParallelism, endBlock - firstBlock)
        }, block =>
        {
            try
            {
                int component = 0;
                while (block >= this.blockOffsets[component + 1])
                {
                    component++;
                }

                JpegComponentSampling factor = this.sampling[component];
                int localBlock = block - this.blockOffsets[component];
                int blockX = localBlock % this.blocksWide[component];
                int blockY = (mcuRow * factor.Vertical) + (localBlock / this.blocksWide[component]);
                if (blockY >= this.blocksHigh[component])
                {
                    return; // Dummy blocks retain the ordered consumer's DC predictor.
                }

                Span<float> samples = stackalloc float[64];
                Downsampling.FillBlock(this.source.GetPixels(), this.width, this.height, this.stride, component,
                    blockX, blockY, this.sampling.MaxHorizontal, this.sampling.MaxVertical,
                    factor.Horizontal, factor.Vertical, samples, this.smoothingFactor);
                ForwardDct.Transform(samples, this.coefficients.AsSpan(block * 64, 64));
            }
            catch (Exception error)
            {
                // Preserve the public exception type rather than exposing a
                // Parallel.For AggregateException. Slots retain deterministic order.
                this.errors[block] = ExceptionDispatchInfo.Capture(error);
            }
        });
        cancellationToken.ThrowIfCancellationRequested();
        for (int block = firstBlock; block < endBlock; block++)
        {
            this.errors[block]?.Throw();
        }

        this.preparedTop = top;
        this.preparedComponents |= componentMask;
    }

    internal void CopyBlock(int component, int blockX, int blockY, Span<float> destination)
    {
        int localRow = blockY % this.sampling[component].Vertical;
        int block = this.blockOffsets[component] + (localRow * this.blocksWide[component]) + blockX;
        this.coefficients.AsSpan(block * 64, 64).CopyTo(destination);
    }
}
