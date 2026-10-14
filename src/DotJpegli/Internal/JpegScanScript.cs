// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in
// THIRD-PARTY-NOTICES.md.

namespace DotJpegli.Internal;

// Managed adaptation of ValidateScanScript and ProcessCompressionParams from
// google/jpegli lib/jpegli/encode.cc, revision
// 031a0077f5799a6041004267fc12b956c1f52a20.
internal static class JpegScanScript
{
    internal static JpegScan[] Prepare(IReadOnlyList<JpegScan> script, int componentCount,
        CancellationToken cancellationToken, out bool progressive)
    {
        progressive = false;
        cancellationToken.ThrowIfCancellationRequested();
        if (script is null)
            throw Invalid("The scan script must not be null.");
        int count = script.Count;
        // Upstream has at most 252 AC contexts. Three components can also have
        // at most 11 initial/refinement DC scans each, hence 285 total scans.
        if (count is < 1 or > 285)
            throw Invalid("The scan script must contain between 1 and 285 scans.");

        // Snapshot every caller-owned list before examining scan progression.
        // The returned descriptors and selector arrays are encoder-owned.
        var snapshot = new JpegScan[count];
        for (int index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            JpegScan scan = script[index];
            if (scan is null || scan.Components is null)
                throw Invalid("Each scan must have a descriptor and a component list.");
            int selectedCount = scan.Components.Count;
            if (selectedCount < 1 || selectedCount > componentCount)
                throw Invalid("Each scan must select between one and the number of output components.");
            var selected = new int[selectedCount];
            for (int component = 0; component < selectedCount; component++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                selected[component] = scan.Components[component];
            }
            snapshot[index] = scan with { Components = selected };
        }

        progressive = snapshot[0].SpectralStart != 0 || snapshot[0].SpectralEnd != 63;
        var approximation = new int[componentCount, 64];
        for (int component = 0; component < componentCount; component++)
            for (int coefficient = 0; coefficient < 64; coefficient++)
                approximation[component, coefficient] = -1;

        int acContexts = 0;
        foreach (JpegScan scan in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int previous = -1;
            foreach (int component in scan.Components)
            {
                if (component < 0 || component >= componentCount || component <= previous)
                    throw Invalid("Scan component indices must be valid, unique and strictly increasing.");
                previous = component;
            }
            if (scan.SpectralStart < 0 || scan.SpectralEnd < scan.SpectralStart || scan.SpectralEnd > 63)
                throw Invalid("Each scan must have an ordered spectral range from 0 to 63.");
            if (scan.SuccessiveHigh is < 0 or > 10 || scan.SuccessiveLow is < 0 or > 10)
                throw Invalid("Successive approximation values must be between 0 and 10.");
            if (!progressive)
            {
                if (scan.SpectralStart != 0 || scan.SpectralEnd != 63
                    || scan.SuccessiveHigh != 0 || scan.SuccessiveLow != 0)
                    throw Invalid("Sequential scans must send coefficients 0 through 63 at full precision.");
            }
            else if (scan.SpectralStart == 0 && scan.SpectralEnd != 0)
                throw Invalid("Progressive scans must send DC and AC separately.");
            if (scan.SpectralStart != 0 && scan.Components.Count != 1)
                throw Invalid("Progressive AC scans must select exactly one component.");

            if (scan.SpectralEnd > 0)
            {
                acContexts += scan.Components.Count;
                if (acContexts > 252)
                    throw Invalid("The scan script must use at most 252 AC component contexts.");
            }
            foreach (int component in scan.Components)
            {
                if (scan.SpectralStart != 0 && approximation[component, 0] < 0)
                    throw Invalid("A component's initial DC scan must precede its AC scans.");
                for (int coefficient = scan.SpectralStart; coefficient <= scan.SpectralEnd; coefficient++)
                {
                    int current = approximation[component, coefficient];
                    if (current < 0)
                    {
                        if (scan.SuccessiveHigh != 0)
                            throw Invalid("A coefficient's initial scan must use SuccessiveHigh zero.");
                    }
                    else if (current != scan.SuccessiveHigh || scan.SuccessiveLow != scan.SuccessiveHigh - 1)
                        throw Invalid("Refinement scans must continue each selected coefficient by exactly one bit.");
                    approximation[component, coefficient] = scan.SuccessiveLow;
                }
            }
        }
        for (int component = 0; component < componentCount; component++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int coefficient = 0; coefficient < 64; coefficient++)
                if (approximation[component, coefficient] != 0)
                    throw Invalid("The scan script must deliver every output coefficient through bit zero.");
        }
        return snapshot;
    }

    private static ArgumentException Invalid(string message) => new(message, "ScanScript");
}
