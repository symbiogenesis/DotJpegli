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
    internal sealed record Preset(int Level, bool InterleaveSequentialComponents)
    {
        internal static Preset Level0Interleaved { get; } = new(0, true);
        internal static Preset Level1Interleaved { get; } = new(1, true);
        internal static Preset Level2Interleaved { get; } = new(2, true);
        internal static Preset Level3Interleaved { get; } = new(3, true);
    }
    internal sealed record Scripted(IReadOnlyList<JpegScan> Scans);

    internal static JpegScanConfiguration FromOptions(JpegliEncoderOptions options)
    {
        if (options.ScanScript is { } script)
        {
            return new Scripted(script);
        }

        if (options.ProgressiveLevel is int level)
        {
            if (options.SequentialInterleaveComponents)
            {
                return level switch
                {
                    0 => Preset.Level0Interleaved,
                    1 => Preset.Level1Interleaved,
                    2 => Preset.Level2Interleaved,
                    3 => Preset.Level3Interleaved,
                    _ => new Preset(level, true)
                };
            }
            return new Preset(level, false);
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

    private static readonly int[] SingleScanEnds = [63];
    private static readonly int[] TwoBandScanEnds = [2, 63];
    private static readonly int[] ZeroRefinement = [0];
    private static readonly int[] OneRefinement = [1];
    private static readonly int[] TwoBandRefinement = [0, 2];
    private static readonly int[] SingleComponents = [0];
    private static readonly int[] ColorComponents = [0, 1, 2];
    private static readonly int[][] SingleSelectors = [SingleComponents];
    private static readonly int[][] ColorSelectors = [SingleComponents, [1], [2]];
    private static readonly JpegScan[] DefaultGrayscaleSequential = [new(SingleComponents, 0, 63, 0, 0)];
    private static readonly JpegScan[] DefaultColorSequential = [new(ColorComponents, 0, 63, 0, 0)];
    private static readonly JpegScan[] AsymmetricColorInterleaved = BuildAsymmetricColorScript(true);
    private static readonly JpegScan[] AsymmetricColorSeparate = BuildAsymmetricColorScript(false);
    private static readonly JpegScan[] DefaultLevel2Grayscale = new Prepared(true, true, 0, TwoBandScanEnds, TwoBandRefinement, null).BuildDefaultScript(1);
    private static readonly JpegScan[] DefaultLevel2ColorInterleaved = new Prepared(true, true, 0, TwoBandScanEnds, TwoBandRefinement, null).BuildDefaultScript(3);
    private static readonly JpegScan[] DefaultLevel2ColorSeparate = new Prepared(true, false, 0, TwoBandScanEnds, TwoBandRefinement, null).BuildDefaultScript(3);

    internal Prepared Prepare(JpegSamplingFactors sampling, bool grayscale, Prepared? preparedScript) => this switch
    {
        Sequential sequential => new(false, grayscale || sequential.InterleaveComponents, 0, SingleScanEnds, ZeroRefinement, null),
        Progressive progressive => PrepareProgressive(progressive, grayscale),
        Preset preset => PreparePreset(preset, sampling, grayscale),
        Scripted => preparedScript ?? throw new InvalidOperationException("The scan script must be prepared first."),
        null => throw new InvalidOperationException("A scan configuration must be initialized.")
    };

    private static Prepared PrepareScript(Scripted script, int componentCount, CancellationToken cancellationToken)
    {
        JpegScan[] scans = JpegScanScript.Prepare(script.Scans, componentCount, cancellationToken, out bool progressive);
        return new(progressive, true, 0, SingleScanEnds, ZeroRefinement, scans);
    }

    private static Prepared PreparePreset(Preset preset, JpegSamplingFactors sampling, bool grayscale)
    {
        if (preset.Level == 0)
        {
            return new(false, grayscale || preset.InterleaveSequentialComponents, 0, SingleScanEnds, ZeroRefinement, null);
        }

        bool interleaveInitialScan = grayscale || (sampling.MaxHorizontal == 1 && sampling.MaxVertical == 1);
        if (preset.Level >= 3 && !grayscale)
        {
            return new(true, interleaveInitialScan, 0, TwoBandScanEnds, TwoBandRefinement,
                CreateAsymmetricColorScript(interleaveInitialScan));
        }

        // SetDefaultScanScript, lib/jpegli/encode.cc, pinned revision
        // 031a0077f5799a6041004267fc12b956c1f52a20, for levels 1 and 2.
        return new(true, interleaveInitialScan, 0,
            preset.Level == 1 ? SingleScanEnds : TwoBandScanEnds, preset.Level == 1 ? OneRefinement : TwoBandRefinement, null);
    }

    internal static JpegScan[] CreateAsymmetricColorScript(bool interleaveInitialScan)
        => interleaveInitialScan ? AsymmetricColorInterleaved : AsymmetricColorSeparate;

    private static JpegScan[] BuildAsymmetricColorScript(bool interleaveInitialScan)
    {
        int[] all = ColorComponents, y = SingleComponents, cb = ColorSelectors[1], cr = ColorSelectors[2];
        JpegScan[] dc = interleaveInitialScan
            ? [new(all, 0, 0, 0, 0)]
            : [new(y, 0, 0, 0, 0), new(cb, 0, 0, 0, 0), new(cr, 0, 0, 0, 0)];
        return
        [
            .. dc,
            new(y, 1, 2, 0, 0),
            new(cb, 1, 2, 0, 0),
            new(cr, 1, 2, 0, 0),
            new(y, 3, 63, 0, 2),
            new(y, 3, 63, 2, 1),
            new(cb, 3, 63, 0, 1),
            new(cr, 3, 63, 0, 1),
            new(y, 3, 63, 1, 0),
            new(cb, 3, 63, 1, 0),
            new(cr, 3, 63, 1, 0)
        ];
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
            return SingleScanEnds;
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
        if (depths is null && bandCount == 1 && refinementBits == 0)
        {
            return ZeroRefinement;
        }

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
        internal int MaxRefinementBits
        {
            get
            {
                int max = this.DcRefinementBits;
                foreach (int depth in this.AcRefinementBits)
                {
                    if (depth > max)
                    {
                        max = depth;
                    }
                }
                return max;
            }
        }

        internal JpegScan[] CreateDefaultScript(int componentCount)
        {
            bool interleaveInitialScan = this.InterleaveInitialScan;
            bool progressive = this.Progressive;
            if (!progressive && interleaveInitialScan && this.DcRefinementBits == 0)
            {
                if (componentCount == 3)
                {
                    return DefaultColorSequential;
                }
                if (componentCount == 1)
                {
                    return DefaultGrayscaleSequential;
                }
            }
            if (progressive && this.DcRefinementBits == 0
                && ReferenceEquals(this.AcScanEnds, TwoBandScanEnds)
                && ReferenceEquals(this.AcRefinementBits, TwoBandRefinement))
            {
                if (componentCount == 3)
                {
                    return interleaveInitialScan ? DefaultLevel2ColorInterleaved : DefaultLevel2ColorSeparate;
                }
                if (componentCount == 1 && interleaveInitialScan)
                {
                    return DefaultLevel2Grayscale;
                }
            }

            return this.BuildDefaultScript(componentCount);
        }

        internal JpegScan[] BuildDefaultScript(int componentCount)
        {
            bool interleaveInitialScan = this.InterleaveInitialScan;
            bool progressive = this.Progressive;

            int[] components = componentCount == 3 ? ColorComponents
                : componentCount == 1 ? SingleComponents
                : Enumerable.Range(0, componentCount).ToArray();
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
            int[][] componentSelectors;
            if (componentCount == 3)
            {
                componentSelectors = ColorSelectors;
            }
            else if (componentCount == 1)
            {
                componentSelectors = SingleSelectors;
            }
            else
            {
                componentSelectors = new int[componentCount][];
                foreach (int component in components)
                {
                    componentSelectors[component] = [component];
                }
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
