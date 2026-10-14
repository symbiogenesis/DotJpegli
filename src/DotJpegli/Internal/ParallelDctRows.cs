using System.Runtime.ExceptionServices;

namespace DotJpegli.Internal;

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
        blockOffsets = new int[componentCount + 1];
        blocksWide = new int[componentCount];
        blocksHigh = new int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            JpegComponentSampling factor = sampling[component];
            blocksWide[component] = (width * factor.Horizontal + 8 * sampling.MaxHorizontal - 1)
                / (8 * sampling.MaxHorizontal);
            blocksHigh[component] = (height * factor.Vertical + 8 * sampling.MaxVertical - 1)
                / (8 * sampling.MaxVertical);
            blockOffsets[component + 1] = checked(blockOffsets[component] + blocksWide[component] * factor.Vertical);
        }
        coefficients = new float[checked(blockOffsets[^1] * 64)];
        errors = new ExceptionDispatchInfo?[blockOffsets[^1]];
        int length = checked((height - 1) * stride + width * pixels.ComponentsPerPixel);
        source = new PixelSource.Snapshot(pixels, length, cancellationToken);
    }

    internal void PrepareRow(int top, CancellationToken cancellationToken, int singleComponent = -1)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int componentMask = singleComponent < 0 ? (1 << blocksWide.Length) - 1 : 1 << singleComponent;
        if (preparedTop == top && (preparedComponents & componentMask) == componentMask)
            return;
        if (preparedTop != top)
            preparedComponents = 0;
        // Separate-component scans only need that component's row. Computing
        // the other components would multiply progressive/refinement DCT work.
        int firstBlock = singleComponent < 0 ? 0 : blockOffsets[singleComponent];
        int endBlock = singleComponent < 0 ? blockOffsets[^1] : blockOffsets[singleComponent + 1];
        Array.Clear(errors, firstBlock, endBlock - firstBlock);
        int mcuRow = top / (8 * sampling.MaxVertical);
        // Each invocation joins all workers before exposing the row to the
        // ordered consumer. No workers survive cancellation or an output error.
        Parallel.For(firstBlock, endBlock, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(maxDegreeOfParallelism, endBlock - firstBlock)
        }, block =>
        {
            try
            {
                int component = 0;
                while (block >= blockOffsets[component + 1])
                    component++;
                JpegComponentSampling factor = sampling[component];
                int localBlock = block - blockOffsets[component];
                int blockX = localBlock % blocksWide[component];
                int blockY = mcuRow * factor.Vertical + localBlock / blocksWide[component];
                if (blockY >= blocksHigh[component])
                    return; // Dummy blocks retain the ordered consumer's DC predictor.
                Span<float> samples = stackalloc float[64];
                Downsampling.FillBlock(source.GetPixels(), width, height, stride, component,
                    blockX, blockY, sampling.MaxHorizontal, sampling.MaxVertical,
                    factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                ForwardDct.Transform(samples, coefficients.AsSpan(block * 64, 64));
            }
            catch (Exception error)
            {
                // Preserve the public exception type rather than exposing a
                // Parallel.For AggregateException. Slots retain deterministic order.
                errors[block] = ExceptionDispatchInfo.Capture(error);
            }
        });
        cancellationToken.ThrowIfCancellationRequested();
        for (int block = firstBlock; block < endBlock; block++)
            errors[block]?.Throw();
        preparedTop = top;
        preparedComponents |= componentMask;
    }

    internal void CopyBlock(int component, int blockX, int blockY, Span<float> destination)
    {
        int localRow = blockY % sampling[component].Vertical;
        int block = blockOffsets[component] + localRow * blocksWide[component] + blockX;
        coefficients.AsSpan(block * 64, 64).CopyTo(destination);
    }
}
