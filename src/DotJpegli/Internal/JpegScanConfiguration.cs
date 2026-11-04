namespace DotJpegli.Internal;

using System.Diagnostics.CodeAnalysis;

// Normalize public options into one mode, then prepare only that mode's data.
// Reference cases avoid boxing; caller-owned lists are copied before encoding.
internal readonly union JpegScanConfiguration(JpegScanConfiguration.Sequential,
    JpegScanConfiguration.Progressive, JpegScanConfiguration.Preset, JpegScanConfiguration.Scripted)
{
    internal sealed record Sequential(bool InterleaveComponents)
    {
        internal static Sequential Interleaved { get; } = new(true);
        internal static Sequential Separate { get; } = new(false);
    }
    internal sealed record Progressive(bool InterleaveDc, int RefinementBits,
        IReadOnlyList<int>? AcScanEnds, IReadOnlyList<int>? AcRefinementBits);
    internal sealed record Preset(int Level, bool InterleaveSequentialComponents);
    internal sealed record Scripted(IReadOnlyList<JpegScan> Scans);

    internal static JpegScanConfiguration FromOptions(JpegliEncoderOptions options)
    {
        if (options.ScanScript is { } script)
        {
            return new Scripted(script);
        }

        if (options.ProgressiveLevel is int level)
        {
            return new Preset(level, options.SequentialInterleaveComponents);
        }

        return options.Progressive
            ? new Progressive(options.ProgressiveInterleaveDc, options.ProgressiveRefinementBits,
                options.ProgressiveAcScanEnds, options.ProgressiveAcRefinementBits)
            : options.SequentialInterleaveComponents ? Sequential.Interleaved : Sequential.Separate;
    }

    internal Prepared? PrepareScript(int componentCount, CancellationToken cancellationToken) => this switch
    {
        Scripted script => PrepareScript(script, componentCount, cancellationToken),
        Sequential => null,
        Progressive => null,
        Preset => null,
        null => throw new InvalidOperationException("A scan configuration must be initialized.")
    };

    internal Prepared Prepare(JpegSamplingFactors sampling, bool grayscale, Prepared? preparedScript) => this switch
    {
        Sequential sequential => new(false, grayscale || sequential.InterleaveComponents, 0, [63], [0], null),
        Progressive progressive => PrepareProgressive(progressive, grayscale),
        Preset preset => PreparePreset(preset, sampling, grayscale),
        Scripted => preparedScript ?? throw new InvalidOperationException("The scan script must be prepared first."),
        null => throw new InvalidOperationException("A scan configuration must be initialized.")
    };

    private static Prepared PrepareScript(Scripted script, int componentCount, CancellationToken cancellationToken)
    {
        JpegScan[] scans = JpegScanScript.Prepare(script.Scans, componentCount, cancellationToken, out bool progressive);
        return new(progressive, true, 0, [63], [0], scans);
    }

    private static Prepared PreparePreset(Preset preset, JpegSamplingFactors sampling, bool grayscale)
    {
        if (preset.Level == 0)
        {
            return new(false, grayscale || preset.InterleaveSequentialComponents, 0, [63], [0], null);
        }

        // SetDefaultScanScript, lib/jpegli/encode.cc, pinned revision
        // 031a0077f5799a6041004267fc12b956c1f52a20. All levels >= 2 share a script.
        return new(true, grayscale || (sampling.MaxHorizontal == 1 && sampling.MaxVertical == 1), 0,
            preset.Level == 1 ? [63] : [2, 63], preset.Level == 1 ? [1] : [0, 2], null);
    }

    private static Prepared PrepareProgressive(Progressive progressive, bool grayscale)
    {
        int[] ends = PrepareAcScanEnds(progressive.AcScanEnds);
        int[] depths = PrepareAcRefinementBits(progressive.AcRefinementBits, ends.Length, progressive.RefinementBits);
        return new(true, grayscale || progressive.InterleaveDc, progressive.RefinementBits, ends, depths, null);
    }

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly",
        Justification = "Scan validation identifies the public ProgressiveAcScanEnds option forwarded to this helper.")]
    private static int[] PrepareAcScanEnds(IReadOnlyList<int>? ends)
    {
        if (ends is null)
        {
            return [63];
        }

        int count = ends.Count;
        if (count is < 1 or > 63)
        {
            throw new ArgumentException("AC scan ends must contain between 1 and 63 positions.", nameof(JpegliEncoderOptions.ProgressiveAcScanEnds));
        }

        int[] snapshot = new int[count];
        int previous = 0;
        for (int index = 0; index < count; index++)
        {
            int end = ends[index];
            if (end <= previous || end > 63)
            {
                throw new ArgumentException("AC scan ends must be strictly increasing positions from 1 to 63.", nameof(JpegliEncoderOptions.ProgressiveAcScanEnds));
            }

            snapshot[index] = end;
            previous = end;
        }
        if (previous != 63)
        {
            throw new ArgumentException("The final AC scan end must be 63.", nameof(JpegliEncoderOptions.ProgressiveAcScanEnds));
        }

        return snapshot;
    }

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly",
        Justification = "Scan validation identifies the public ProgressiveAcRefinementBits option forwarded to this helper.")]
    private static int[] PrepareAcRefinementBits(IReadOnlyList<int>? depths, int bandCount, int refinementBits)
    {
        int[] snapshot = new int[bandCount];
        if (depths is null)
        {
            Array.Fill(snapshot, refinementBits);
            return snapshot;
        }

        if (depths.Count != bandCount)
        {
            throw new ArgumentException("AC refinement depths must contain one value per AC scan band.", nameof(JpegliEncoderOptions.ProgressiveAcRefinementBits));
        }

        for (int index = 0; index < bandCount; index++)
        {
            int depth = depths[index];
            if (depth is < 0 or > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(JpegliEncoderOptions.ProgressiveAcRefinementBits), "AC refinement depths must be between 0 and 10.");
            }

            snapshot[index] = depth;
        }
        return snapshot;
    }

    internal readonly record struct Prepared(bool Progressive, bool InterleaveInitialScan, int DcRefinementBits,
        int[] AcScanEnds, int[] AcRefinementBits, JpegScan[]? Script)
    {
        internal int MaxRefinementBits => Math.Max(this.DcRefinementBits, this.AcRefinementBits.Max());

        internal JpegScan[] CreateDefaultScript(int componentCount)
        {
            int[] components = Enumerable.Range(0, componentCount).ToArray();
            bool interleaveInitialScan = this.InterleaveInitialScan;
            bool progressive = this.Progressive;
            if (!progressive && interleaveInitialScan)
            {
                return [new(components, 0, 63, 0, this.DcRefinementBits)];
            }

            int dcScanCount = interleaveInitialScan ? 1 : componentCount;
            int scanCount = dcScanCount;
            if (progressive)
            {
                scanCount *= 1 + this.DcRefinementBits;
                foreach (int depth in this.AcRefinementBits)
                {
                    scanCount += componentCount * (1 + depth);
                }
            }

            var scans = new JpegScan[scanCount];
            // These selectors belong to this encode and are only read by scans.
            // Every AC band and refinement round can share the same singleton.
            int[][] componentSelectors = new int[componentCount][];
            foreach (int component in components)
            {
                componentSelectors[component] = componentCount == 1 ? components : [component];
            }

            int nextScan = 0;
            void AddDc(int high, int low)
            {
                if (interleaveInitialScan)
                {
                    scans[nextScan++] = new(components, 0, progressive ? 0 : 63, high, low);
                }
                else
                {
                    foreach (int component in components)
                    {
                        scans[nextScan++] = new(componentSelectors[component], 0, progressive ? 0 : 63, high, low);
                    }
                }
            }
            AddDc(0, this.DcRefinementBits);
            if (!this.Progressive)
            {
                return scans;
            }

            for (int band = 0; band < this.AcScanEnds.Length; band++)
            {
                foreach (int component in components)
                {
                    scans[nextScan++] = new(componentSelectors[component], band == 0 ? 1 : this.AcScanEnds[band - 1] + 1,
                        this.AcScanEnds[band], 0, this.AcRefinementBits[band]);
                }
            }

            for (int low = this.MaxRefinementBits - 1; low >= 0; low--)
            {
                if (this.DcRefinementBits > low)
                {
                    AddDc(low + 1, low);
                }

                for (int band = 0; band < this.AcScanEnds.Length; band++)
                {
                    if (this.AcRefinementBits[band] > low)
                    {
                        foreach (int component in components)
                        {
                            scans[nextScan++] = new(componentSelectors[component], band == 0 ? 1 : this.AcScanEnds[band - 1] + 1,
                                this.AcScanEnds[band], low + 1, low);
                        }
                    }
                }
            }
            return scans;
        }
    }
}
