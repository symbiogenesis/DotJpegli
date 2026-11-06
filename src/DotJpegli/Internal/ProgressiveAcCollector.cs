// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

/// <summary>Feeds independent progressive AC scans from one traversal per component.</summary>
internal static class ProgressiveAcCollector
{
    internal static bool ShouldFuse(IReadOnlyList<JpegScan> scans, QuantizedImage? quantizedImage = null)
    {
        // A complete cache removes repeated block preparation already. Keep the
        // established scan-major path there, without allocating per-scan states.
        if (quantizedImage?.IsComplete == true)
        {
            return false;
        }
        // A single AC scan already prepares each component block only once.
        Span<int> acScanCounts = stackalloc int[3];
        acScanCounts.Clear();
        foreach (JpegScan scan in scans)
        {
            if (scan.SpectralStart > 0 && ++acScanCounts[scan.Components[0]] > 1)
            {
                return true;
            }
        }
        return false;
    }

    internal static void Collect(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, IReadOnlyList<JpegScan> scans,
        long[][][] acFrequencies, EntropyTokenScans tokenScans, int[] restartIntervals,
        QuantizedImage? quantizedImage, CancellationToken cancellationToken)
    {
        // Each scan sees exactly its original raster sequence. Keeping its EOB
        // and correction state separate permits visiting the block only once,
        // including DCT, AQ and quantization outside a partial coefficient cache.
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        Span<short> ordered = stackalloc short[64];
        bool complete = quantizedImage?.IsComplete == true;
        // Without a cache, reorder once for all scans' ordered-coefficient paths.
        bool zigZagOrdered = quantizedImage?.IsZigZagOrdered ?? true;
        Span<float> strengths = adaptive is null || complete ? default : adaptive.Strengths;
        int horizontal = sampling.MaxHorizontal, vertical = sampling.MaxVertical;
        for (int component = 0; component < tables.Length; component++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ScanState[] states = CreateStates(scans, acFrequencies, tokenScans, component);
            if (states.Length == 0)
            {
                continue;
            }

            try
            {
                JpegComponentSampling factor = sampling[component];
                int blocksWide = ((width * factor.Horizontal) + (8 * horizontal) - 1) / (8 * horizontal);
                int blocksHigh = ((height * factor.Vertical) + (8 * vertical) - 1) / (8 * vertical);
                Quantization.PreparedTable? quantizer = complete ? null
                    : Quantization.PreparedTable.Create(tables[component], component, distance,
                        grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                        adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                        rgb: pixels.IsRgb, tailZeroRunBias: pixels.TailZeroRunBias);
                int restartInterval = restartIntervals[component];
                int remaining = restartInterval, restartIndex = 0, lastAnalysisTop = -1;
                for (int blockY = 0; blockY < blocksHigh; blockY++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    bool cachedRow = complete || quantizedImage?.ContainsRow(component, blockY) == true;
                    int analysisTop = blockY / factor.Vertical * 8 * vertical;
                    if (!cachedRow)
                    {
                        pixels.ParallelRows?.PrepareRow(analysisTop, cancellationToken, component);
                        if (adaptive is not null && analysisTop != lastAnalysisTop)
                        {
                            adaptive.ComputeRow(pixels, stride, analysisTop,
                                tables[pixels.AnalysisComponent][1], strengths, cancellationToken);
                            lastAnalysisTop = analysisTop;
                        }
                    }

                    for (int blockX = 0; blockX < blocksWide; blockX++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (restartInterval != 0 && remaining == 0)
                        {
                            foreach (ScanState state in states)
                            {
                                state.Flush();
                                state.Tokens.AddRestart(restartIndex);
                            }
                            restartIndex = (restartIndex + 1) & 7;
                            remaining = restartInterval;
                        }

                        scoped ReadOnlySpan<short> encoded;
                        if (cachedRow)
                        {
                            encoded = quantizedImage!.GetBlock(component, blockX, blockY);
                        }
                        else
                        {
                            if (pixels.ParallelRows is { } parallelRows)
                            {
                                parallelRows.CopyBlock(component, blockX, blockY, coefficients);
                            }
                            else
                            {
                                Downsampling.FillBlock(pixels, width, height, stride, component,
                                    blockX, blockY, horizontal, vertical, factor.Horizontal, factor.Vertical,
                                    samples, smoothingFactor);
                                ForwardDct.Transform(samples, coefficients);
                            }
                            int strengthRow = blockY % factor.Vertical;
                            float strength = adaptive is null ? 0f : strengths[(strengthRow * adaptive.BlocksWide)
                                + (blockX * horizontal / factor.Horizontal)];
                            Quantization.QuantizePrepared(coefficients, quantizer!, quantized, adaptiveStrength: strength);
                            // Validate once before sharing the block between scan
                            // states, rather than repeating validation in each scan.
                            EntropyEncoder.ValidateAc(quantized);
                            if (zigZagOrdered)
                            {
                                ZigZagOrdering.Reorder(quantized, ordered);
                                encoded = ordered;
                            }
                            else
                            {
                                encoded = quantized;
                            }
                        }

                        foreach (ScanState state in states)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            state.Accumulate(encoded, zigZagOrdered);
                        }
                        if (restartInterval != 0)
                        {
                            remaining--;
                        }
                    }
                }
                foreach (ScanState state in states)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    state.Flush();
                }
            }
            finally
            {
                foreach (ScanState state in states)
                {
                    state.Dispose();
                }
            }
        }
    }

    private static ScanState[] CreateStates(IReadOnlyList<JpegScan> scans, long[][][] frequencies,
        EntropyTokenScans tokens, int component)
    {
        int count = 0;
        foreach (JpegScan scan in scans)
        {
            if (scan.SpectralStart > 0 && scan.Components[0] == component)
            {
                count++;
            }
        }
        var states = new ScanState[count];
        int position = 0;
        for (int index = 0; index < scans.Count; index++)
        {
            JpegScan scan = scans[index];
            if (scan.SpectralStart > 0 && scan.Components[0] == component)
            {
                states[position++] = new(scan, frequencies[index][component], tokens[index], component);
            }
        }
        return states;
    }

    private sealed class ScanState(JpegScan scan, long[] frequencies, EntropyTokens tokens, int component) : IDisposable
    {
        private readonly ProgressiveAcRefinementState? refinement = scan.SuccessiveHigh != 0 ? new(pooled: true) : null;
        private int pendingEobRun;

        internal EntropyTokens Tokens { get; } = tokens;

        internal void Accumulate(ReadOnlySpan<short> block, bool zigZagOrdered)
        {
            if (this.refinement is not null)
            {
                ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(block, frequencies, this.refinement,
                    scan.SuccessiveLow, scan.SpectralStart, scan.SpectralEnd, coefficientsValidated: true,
                    tokens: this.Tokens, component: component, zigZagOrdered: zigZagOrdered);
            }
            else
            {
                ProgressiveEntropyEncoder.AccumulateAcFrequencies(block, frequencies, ref this.pendingEobRun,
                    scan.SuccessiveLow, scan.SpectralStart, scan.SpectralEnd, coefficientsValidated: true,
                    tokens: this.Tokens, component: component, zigZagOrdered: zigZagOrdered);
            }
        }

        internal void Flush()
        {
            if (this.refinement is not null)
            {
                ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(frequencies, this.refinement, this.Tokens, component);
            }
            else
            {
                ProgressiveEntropyEncoder.FlushAcFrequencies(frequencies, ref this.pendingEobRun, this.Tokens, component);
            }
        }

        public void Dispose() => this.refinement?.Dispose();
    }
}
