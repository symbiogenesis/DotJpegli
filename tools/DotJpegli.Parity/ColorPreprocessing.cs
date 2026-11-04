namespace DotJpegli.Parity;

using DotJpegli.Internal;

// Explicit offline hypotheses, not a SharpYUV port or a decoder implementation.
// The common real decoder and all measured control quality floors remain decisive.
internal static class ColorPreprocessing
{
    internal sealed record Prepared(float[] Pixels, PixelComponentLayout Layout, string Model,
        double? BeforeError, double? AfterError, int Iterations, int ChangedSamples);
    internal sealed record SamplingAdvice(string Recommendation, bool Complete, long VisitedEdges,
        long TotalEdges, long ChromaEdgeSum, double NormalizedLowerBound, int Threshold,
        string Definition);

    private static readonly double[] LinearBytes = Enumerable.Range(0, 256).Select(value => Linear(value / 255.0)).ToArray();

    internal static void Validate(PpmImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width is <= 0 or > 1024 || image.Height is <= 0 or > 1024
            || image.Pixels.Length != (long)image.Width * image.Height * 3)
        {
            throw new ArgumentException("Offline color preprocessing requires an RGB8 raster with each dimension at most 1024.", nameof(image));
        }
    }

    internal static Prepared? Reconstruct420(PpmImage image, int iterations, CancellationToken cancellationToken)
    {
        Validate(image);
        ArgumentOutOfRangeException.ThrowIfNegative(iterations);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, 4);
        int width = image.Width, height = image.Height, count = width * height;
        int cw = (width + 1) / 2, ch = (height + 1) / 2, nodes = cw * ch;
        float[] luma = new float[count];
        double[] target = new double[count * 3], cb = new double[nodes], cr = new double[nodes];
        double[] baselineCb = new double[nodes], baselineCr = new double[nodes];
        for (int y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < width; x++)
            {
                int pixel = (y * width) + x, p = pixel * 3;
                ColorConversion.RgbToYCbCr(image.Pixels[p], image.Pixels[p + 1], image.Pixels[p + 2],
                    out luma[pixel], out _, out _);
                for (int channel = 0; channel < 3; channel++)
                {
                    target[p + channel] = LinearBytes[image.Pixels[p + channel]];
                }
            }
        }
        for (int y = 0; y < ch; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < cw; x++)
            {
                double lr = 0, lg = 0, lb = 0, bcb = 0, bcr = 0;
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int p = ((Math.Min((y * 2) + dy, height - 1) * width) + Math.Min((x * 2) + dx, width - 1)) * 3;
                        lr += target[p];
                        lg += target[p + 1];
                        lb += target[p + 2];
                        ColorConversion.RgbToYCbCr(image.Pixels[p], image.Pixels[p + 1], image.Pixels[p + 2],
                            out _, out float sourceCb, out float sourceCr);
                        bcb += sourceCb;
                        bcr += sourceCr;
                    }
                }
                int node = (y * cw) + x;
                ColorConversion.RgbToYCbCr((float)(Encoded(lr * .25) * 255), (float)(Encoded(lg * .25) * 255),
                    (float)(Encoded(lb * .25) * 255), out _, out float initialCb, out float initialCr);
                cb[node] = initialCb;
                cr[node] = initialCr;
                baselineCb[node] = bcb * .25;
                baselineCr[node] = bcr * .25;
            }
        }
        double before = ModelError(image, luma, target, baselineCb, baselineCr, null, cancellationToken);
        double bestError = ModelError(image, luma, target, cb, cr, null, cancellationToken);
        if (bestError >= before)
        {
            // A linear-light block seed can hurt the fixed-luma reconstruction.
            // Start correction from the ordinary average when its model is better.
            baselineCb.CopyTo(cb, 0);
            baselineCr.CopyTo(cr, 0);
            bestError = before;
        }
        double[] bestCb = (double[])cb.Clone(), bestCr = (double[])cr.Clone();
        double[][] gradient = Enumerable.Range(0, 4).Select(_ => new double[nodes]).ToArray();
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            foreach (double[] array in gradient)
            {
                Array.Clear(array);
            }
            _ = ModelError(image, luma, target, cb, cr, gradient, cancellationToken);
            for (int node = 0; node < nodes; node++)
            {
                cb[node] = Math.Clamp(cb[node] + Math.Clamp(.25 * gradient[0][node] / Math.Max(gradient[2][node], 1e-15), -4, 4), 0, 255);
                cr[node] = Math.Clamp(cr[node] + Math.Clamp(.25 * gradient[1][node] / Math.Max(gradient[3][node], 1e-15), -4, 4), 0, 255);
            }
            double error = ModelError(image, luma, target, cb, cr, null, cancellationToken);
            if (error < bestError)
            {
                bestError = error;
                cb.CopyTo(bestCb, 0);
                cr.CopyTo(bestCr, 0);
            }
        }
        if (!double.IsFinite(bestError) || !(bestError < before - 1e-12))
        {
            return null;
        }
        float[] output = new float[count * 3];
        int changed = 0;
        for (int y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < width; x++)
            {
                int p = ((y * width) + x) * 3, node = (y / 2 * cw) + (x / 2);
                output[p] = luma[p / 3] / 255f;
                output[p + 1] = (float)(bestCb[node] / 255);
                output[p + 2] = (float)(bestCr[node] / 255);
                changed += (bestCb[node] == baselineCb[node] ? 0 : 1) + (bestCr[node] == baselineCr[node] ? 0 : 1);
            }
        }
        return new(output, PixelComponentLayout.YCbCr,
            "Better of linear-light 2x2 and ordinary chroma-average seeds; centered 3/4-1/4 bilinear unquantized reconstruction; damped diagonal chroma correction; approximate, not SharpYUV",
            before, bestError, iterations, changed);
    }

    private static double ModelError(PpmImage image, float[] luma, double[] target, double[] cb, double[] cr,
        double[][]? gradient, CancellationToken cancellationToken)
    {
        int cw = (image.Width + 1) / 2, ch = (image.Height + 1) / 2;
        Span<int> indices = stackalloc int[4];
        Span<double> weights = stackalloc double[4];
        double total = 0;
        for (int y = 0; y < image.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Axis(y, ch, out int y0, out int y1, out double wy);
            for (int x = 0; x < image.Width; x++)
            {
                Axis(x, cw, out int x0, out int x1, out double wx);
                indices[0] = (y0 * cw) + x0;
                indices[1] = (y0 * cw) + x1;
                indices[2] = (y1 * cw) + x0;
                indices[3] = (y1 * cw) + x1;
                weights[0] = (1 - wx) * (1 - wy);
                weights[1] = wx * (1 - wy);
                weights[2] = (1 - wx) * wy;
                weights[3] = wx * wy;
                double blue = 0, red = 0;
                for (int corner = 0; corner < 4; corner++)
                {
                    blue += weights[corner] * cb[indices[corner]];
                    red += weights[corner] * cr[indices[corner]];
                }
                blue -= 128;
                red -= 128;
                int pixel = (y * image.Width) + x, p = pixel * 3;
                double r = luma[pixel] + (1.402 * red), g = luma[pixel] - (.344136 * blue) - (.714136 * red),
                    b = luma[pixel] + (1.772 * blue);
                double er = target[p] - Linear(Math.Clamp(r / 255, 0, 1)),
                    eg = target[p + 1] - Linear(Math.Clamp(g / 255, 0, 1)),
                    eb = target[p + 2] - Linear(Math.Clamp(b / 255, 0, 1));
                total += (er * er) + (eg * eg) + (eb * eb);
                if (gradient is null)
                {
                    continue;
                }
                double dr = Slope(r), dg = Slope(g), db = Slope(b);
                double gcb = (eg * dg * -.344136) + (eb * db * 1.772),
                    gcr = (er * dr * 1.402) + (eg * dg * -.714136);
                double dcb = (dg * dg * .344136 * .344136) + (db * db * 1.772 * 1.772),
                    dcr = (dr * dr * 1.402 * 1.402) + (dg * dg * .714136 * .714136);
                for (int corner = 0; corner < 4; corner++)
                {
                    int node = indices[corner];
                    double weight = weights[corner];
                    gradient[0][node] += weight * gcb;
                    gradient[1][node] += weight * gcr;
                    gradient[2][node] += weight * weight * dcb;
                    gradient[3][node] += weight * weight * dcr;
                }
            }
        }
        return total / (image.Width * image.Height * 3.0);
    }

    private static void Axis(int pixel, int count, out int first, out int second, out double weight)
    {
        first = (pixel / 2) - ((pixel & 1) == 0 ? 1 : 0);
        second = first + 1;
        weight = (pixel & 1) == 0 ? .75 : .25;
        first = Math.Clamp(first, 0, count - 1);
        second = Math.Clamp(second, 0, count - 1);
        if (first == second)
        {
            weight = 0;
        }
    }

    internal static Prepared? Dering(PpmImage image, double amplitude, CancellationToken cancellationToken)
    {
        Validate(image);
        if (!double.IsFinite(amplitude) || amplitude is <= 0 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(amplitude));
        }
        float[] output = image.Pixels.Select(value => (float)(value * (1.0 / 255.0))).ToArray();
        int changed = 0;
        for (int y = 0; y < image.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < image.Width; x++)
            {
                for (int channel = 0; channel < 3; channel++)
                {
                    int p = (((y * image.Width) + x) * 3) + channel, value = image.Pixels[p];
                    if (value is not (0 or 255))
                    {
                        continue;
                    }
                    bool edge = IsRunEdge(image, x, y, channel, 1, 0) || IsRunEdge(image, x, y, channel, 0, 1);
                    if (edge)
                    {
                        output[p] = (float)((value + (value == 0 ? -amplitude : amplitude)) / 255);
                        changed++;
                    }
                }
            }
        }
        return changed == 0 ? null : new(output, PixelComponentLayout.Rgb,
            "Saturated component run-edge overshoot; decoder clipping hypothesis only; no blur; real decoder quality gates required",
            null, null, 0, changed);
    }

    private static bool IsRunEdge(PpmImage image, int x, int y, int channel, int dx, int dy)
    {
        if (x - dx < 0 || x + dx >= image.Width || y - dy < 0 || y + dy >= image.Height)
        {
            return false;
        }
        int center = image.Pixels[(((y * image.Width) + x) * 3) + channel];
        int previous = image.Pixels[((((y - dy) * image.Width) + x - dx) * 3) + channel];
        int next = image.Pixels[((((y + dy) * image.Width) + x + dx) * 3) + channel];
        return (previous == center && Math.Abs(next - center) >= 32)
            || (next == center && Math.Abs(previous - center) >= 32);
    }

    internal static SamplingAdvice AdviseSampling(PpmImage image, bool earlyExit, CancellationToken cancellationToken)
    {
        Validate(image);
        long edges = ((long)(image.Width - 1) * image.Height) + ((long)(image.Height - 1) * image.Width), sum = 0, visited = 0;
        const int threshold = 32;
        const string definition = "Mean |delta(2B-R-G)|+|delta(2R-G-B)| over every horizontal/vertical RGB8 edge; threshold 32; advisory only, not Google's classifier";
        int bands = (image.Height + 7) / 8, power = 1, bits = 0;
        while (power < bands)
        {
            power *= 2;
            bits++;
        }
        for (int ordinal = 0; ordinal < power; ordinal++)
        {
            int value = ordinal, band = 0;
            for (int bit = 0; bit < bits; bit++)
            {
                band = (band * 2) + (value & 1);
                value >>= 1;
            }
            if (band >= bands)
            {
                continue;
            }
            for (int y = band * 8; y < Math.Min((band + 1) * 8, image.Height); y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int x = 0; x < image.Width; x++)
                {
                    int p = ((y * image.Width) + x) * 3;
                    if (x + 1 < image.Width)
                    {
                        sum += Edge(image.Pixels, p, p + 3);
                        visited++;
                    }
                    if (y + 1 < image.Height)
                    {
                        sum += Edge(image.Pixels, p, p + (image.Width * 3));
                        visited++;
                    }
                }
                if (earlyExit && sum > edges * threshold)
                {
                    return new("444", visited == edges, visited, edges, sum, edges == 0 ? 0 : (double)sum / edges,
                        threshold, definition);
                }
            }
        }
        return new(sum > edges * threshold ? "444" : "420", true, visited, edges, sum,
            edges == 0 ? 0 : (double)sum / edges, threshold, definition);
    }

    private static int Edge(byte[] pixels, int first, int second)
    {
        int dr = pixels[first] - pixels[second], dg = pixels[first + 1] - pixels[second + 1], db = pixels[first + 2] - pixels[second + 2];
        return Math.Abs((2 * db) - dr - dg) + Math.Abs((2 * dr) - dg - db);
    }

    private static double Linear(double value) => value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
    private static double Encoded(double value) => value <= .0031308 ? value * 12.92 : (1.055 * Math.Pow(value, 1 / 2.4)) - .055;
    private static double Slope(double code) => code is <= 0 or >= 255 ? 0
        : code / 255 <= .04045 ? 1 / (255 * 12.92) : 2.4 / (255 * 1.055) * Math.Pow(((code / 255) + .055) / 1.055, 1.4);
}
