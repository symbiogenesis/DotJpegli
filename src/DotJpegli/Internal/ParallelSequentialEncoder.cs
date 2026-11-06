namespace DotJpegli.Internal;

using System.Runtime.CompilerServices;

/// <summary>Consumes bounded quantized worker rows in the exact sequential MCU order.</summary>
internal static class ParallelSequentialEncoder
{
    // Keep worker orchestration and row state out of the established serial
    // coordinator's generated code and out of progressive preparation.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Process(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int restartInterval,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        EntropyTokens? tokens, long[][]? dcFrequencies, long[][]? acFrequencies,
        KernelPreference narrowZigZagKernel, CancellationToken cancellationToken,
        BitWriter? writer = null, HuffmanCodingPlan.ScanCoding? coding = null)
    {
        ParallelDctRows rows = pixels.ParallelRows!;
        PixelSource analysisSource = rows.SourceForAnalysis;
        int horizontal = sampling.MaxHorizontal, vertical = sampling.MaxVertical;
        int mcuWidth = horizontal * 8, mcuHeight = vertical * 8;
        int mcusWide = (width + mcuWidth - 1) / mcuWidth;
        var quantizers = new Quantization.PreparedTable[3];
        Span<int> widths = stackalloc int[3], heights = stackalloc int[3];
        for (int component = 0; component < 3; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = ((width * factor.Horizontal) + mcuWidth - 1) / mcuWidth;
            heights[component] = ((height * factor.Vertical) + mcuHeight - 1) / mcuHeight;
            quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                adaptiveQuantization: adaptive is not null,
                adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                narrowZigZagKernel: narrowZigZagKernel, imageWidth: width, imageHeight: height,
                tailZeroRunBias: pixels.TailZeroRunBias);
        }
        Span<float> strengths = adaptive is null ? default : adaptive.Strengths;
        Span<int> previousDc = stackalloc int[3];
        previousDc.Clear();
        int remaining = restartInterval, restart = 0;
        for (int top = 0; top < height; top += mcuHeight)
        {
            cancellationToken.ThrowIfCancellationRequested();
            adaptive?.ComputeRow(analysisSource, stride, top, tables[pixels.AnalysisComponent][1],
                strengths, cancellationToken);
            rows.PrepareQuantizedRow(top, quantizers, strengths, cancellationToken);
            int mcuY = top / mcuHeight;
            for (int mcuX = 0; mcuX < mcusWide; mcuX++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && remaining == 0)
                {
                    if (writer is null)
                    {
                        tokens?.AddRestart(restart);
                    }
                    else
                    {
                        writer.WriteRestartMarker(restart);
                    }
                    previousDc.Clear();
                    restart = (restart + 1) & 7;
                    remaining = restartInterval;
                }
                for (int component = 0; component < 3; component++)
                {
                    JpegComponentSampling factor = sampling[component];
                    for (int row = 0; row < factor.Vertical; row++)
                    {
                        int y = (mcuY * factor.Vertical) + row;
                        for (int column = 0; column < factor.Horizontal; column++)
                        {
                            int x = (mcuX * factor.Horizontal) + column;
                            scoped ReadOnlySpan<short> block;
                            short dcValue;
                            ulong nonzeroAcMask;
                            if (x >= widths[component] || y >= heights[component])
                            {
                                block = default;
                                dcValue = checked((short)previousDc[component]);
                                nonzeroAcMask = 0;
                            }
                            else
                            {
                                block = rows.GetQuantizedBlock(component, x, y, out nonzeroAcMask);
                                dcValue = block[0];
                            }
                            if (writer is null)
                            {
                                if (nonzeroAcMask == 0)
                                {
                                    EntropyEncoder.AccumulateDcOnlyBlock(dcValue, ref previousDc[component],
                                        dcFrequencies![component], acFrequencies![component],
                                        tokens: tokens, component: component);
                                }
                                else
                                {
                                    EntropyEncoder.AccumulateOrderedBlockWithMask(block, nonzeroAcMask,
                                        ref previousDc[component], dcFrequencies![component], acFrequencies![component],
                                        tokens: tokens, component: component);
                                }
                            }
                            else
                            {
                                if (nonzeroAcMask == 0)
                                {
                                    EntropyEncoder.WriteDcOnlyBlock(writer, dcValue, ref previousDc[component],
                                        coding!.DcTables[component], coding.AcTables[component]);
                                }
                                else
                                {
                                    EntropyEncoder.WriteOrderedBlockWithMask(writer, block, nonzeroAcMask,
                                        ref previousDc[component], coding!.DcTables[component], coding.AcTables[component]);
                                }
                            }
                        }
                    }
                }
                if (restartInterval != 0)
                {
                    remaining--;
                }
            }
        }
    }
}
