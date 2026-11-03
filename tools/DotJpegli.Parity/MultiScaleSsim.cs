namespace DotJpegli.Parity;

// Formulation and five-scale exponents: Wang, Simoncelli and Bovik (2003),
// https://ece.uwaterloo.ca/~z70wang/publications/msssim.pdf.
// This explicitly defined RGB8 variant uses BT.601 luma, normalized 11x11
// Gaussian weights (sigma 1.5), replicated edges and ceil-sized 2x2 averages.
// It is not a bit-exact implementation of another library's MS-SSIM metric.
internal static class MultiScaleSsim
{
    internal const string MetricId = "dotjpegli-ms-ssim-v1";
    internal const string Definition =
        "RGB8 BT.601 luma .299R+.587G+.114B; five scales with exponents .0448,.2856,.3001,.2363,.1333; " +
        "row-major normalized 11x11 Gaussian sigma1.5 at every pixel with replicated edges; weighted population moments " +
        "and nonnegative variances; C1=(.01*255)^2 C2=(.03*255)^2; CS=(2cov+C2)/(varX+varY+C2); " +
        "L=(2meanX*meanY+C1)/(meanX^2+meanY^2+C1); product of spatialMean(CS)^exponent at first four scales " +
        "and spatialMean(L*CS)^exponent at fifth; each spatial mean clamped[0,1] before power; " +
        "between scales row-major 2x2 averages with replicated odd edges and ceil dimensions, repeating 1x1";

    private const int ScaleCount = 5;
    private const double C1 = .01 * .01 * 255 * 255;
    private const double C2 = .03 * .03 * 255 * 255;
    private static readonly double[] Gaussian = CreateGaussian();
    private static ReadOnlySpan<double> Exponents => [.0448, .2856, .3001, .2363, .1333];

    internal static PreparedMultiScaleSsim Prepare(PpmImage source, CancellationToken cancellationToken = default) => new(source, cancellationToken);

    // Retained independent reference: all five moments are recomputed directly
    // from the source and decoded luma windows for every comparison.
    internal static double Compare(PpmImage source, PpmImage decoded, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRaster(source);
        ValidateRaster(decoded);
        ValidateDimensions(source.Width, source.Height, decoded);
        Scale[] sourceScales = CreatePyramid(source, cancellationToken), decodedScales = CreatePyramid(decoded, cancellationToken);
        double score = 1;
        for (int scaleIndex = 0; scaleIndex < ScaleCount; scaleIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Scale sourceScale = sourceScales[scaleIndex], decodedScale = decodedScales[scaleIndex];
            double spatialSum = 0;
            for (int y = 0; y < sourceScale.Height; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int x = 0; x < sourceScale.Width; x++)
                {
                    double meanX = 0, meanY = 0, secondX = 0, secondY = 0, cross = 0;
                    int kernelIndex = 0;
                    for (int ky = -5; ky <= 5; ky++)
                    {
                        int row = Math.Clamp(y + ky, 0, sourceScale.Height - 1) * sourceScale.Width;
                        for (int kx = -5; kx <= 5; kx++)
                        {
                            int index = row + Math.Clamp(x + kx, 0, sourceScale.Width - 1);
                            double sx = sourceScale.Luma[index], sy = decodedScale.Luma[index];
                            double weight = Gaussian[kernelIndex++];
                            meanX += sx * weight;
                            meanY += sy * weight;
                            secondX += sx * sx * weight;
                            secondY += sy * sy * weight;
                            cross += sx * sy * weight;
                        }
                    }
                    double varianceX = Math.Max(0, secondX - (meanX * meanX));
                    double varianceY = Math.Max(0, secondY - (meanY * meanY));
                    double covariance = cross - (meanX * meanY);
                    double contrastStructure = ((2 * covariance) + C2) / (varianceX + varianceY + C2);
                    double value = scaleIndex == ScaleCount - 1
                        ? contrastStructure * (((2 * meanX * meanY) + C1) / ((meanX * meanX) + (meanY * meanY) + C1))
                        : contrastStructure;
                    spatialSum += value;
                }
            }
            double mean = Math.Clamp(spatialSum / sourceScale.Luma.Length, 0, 1);
            score *= Math.Pow(mean, Exponents[scaleIndex]);
        }
        return score;
    }

    internal static Scale[] CreatePyramid(PpmImage image, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRaster(image);
        var scales = new Scale[ScaleCount];
        double[] luma = new double[image.Pixels.Length / 3];
        for (int y = 0; y < image.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < image.Width; x++)
            {
                int index = (y * image.Width) + x, offset = index * 3;
                luma[index] = (.299 * image.Pixels[offset]) + (.587 * image.Pixels[offset + 1]) + (.114 * image.Pixels[offset + 2]);
            }
        }
        scales[0] = new(image.Width, image.Height, luma);
        for (int scaleIndex = 1; scaleIndex < ScaleCount; scaleIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Scale preceding = scales[scaleIndex - 1];
            int width = ((preceding.Width - 1) / 2) + 1, height = ((preceding.Height - 1) / 2) + 1;
            double[] downsampled = new double[width * height];
            for (int y = 0; y < height; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int upper = 2 * y * preceding.Width;
                int lower = Math.Min((2 * y) + 1, preceding.Height - 1) * preceding.Width;
                for (int x = 0; x < width; x++)
                {
                    int left = 2 * x, right = Math.Min(left + 1, preceding.Width - 1);
                    downsampled[(y * width) + x] =
                        (preceding.Luma[upper + left] + preceding.Luma[upper + right] +
                         preceding.Luma[lower + left] + preceding.Luma[lower + right]) / 4;
                }
            }
            scales[scaleIndex] = new(width, height, downsampled);
        }
        return scales;
    }

    internal static (double[] Means, double[] Variances) PrepareMoments(Scale scale, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        double[] means = new double[scale.Luma.Length], variances = new double[scale.Luma.Length];
        for (int y = 0; y < scale.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < scale.Width; x++)
            {
                double mean = 0, second = 0;
                int kernelIndex = 0;
                for (int ky = -5; ky <= 5; ky++)
                {
                    int row = Math.Clamp(y + ky, 0, scale.Height - 1) * scale.Width;
                    for (int kx = -5; kx <= 5; kx++)
                    {
                        double value = scale.Luma[row + Math.Clamp(x + kx, 0, scale.Width - 1)];
                        double weight = Gaussian[kernelIndex++];
                        mean += value * weight;
                        second += value * value * weight;
                    }
                }
                int index = (y * scale.Width) + x;
                means[index] = mean;
                variances[index] = Math.Max(0, second - (mean * mean));
            }
        }
        return (means, variances);
    }

    internal static double ComparePrepared(Scale[] sourceScales, double[][] means, double[][] variances, PpmImage decoded,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRaster(decoded);
        ValidateDimensions(sourceScales[0].Width, sourceScales[0].Height, decoded);
        Scale[] decodedScales = CreatePyramid(decoded, cancellationToken);
        double score = 1;
        for (int scaleIndex = 0; scaleIndex < ScaleCount; scaleIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Scale sourceScale = sourceScales[scaleIndex], decodedScale = decodedScales[scaleIndex];
            double spatialSum = 0;
            for (int y = 0; y < sourceScale.Height; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int x = 0; x < sourceScale.Width; x++)
                {
                    double meanY = 0, secondY = 0, cross = 0;
                    int kernelIndex = 0;
                    for (int ky = -5; ky <= 5; ky++)
                    {
                        int row = Math.Clamp(y + ky, 0, sourceScale.Height - 1) * sourceScale.Width;
                        for (int kx = -5; kx <= 5; kx++)
                        {
                            int index = row + Math.Clamp(x + kx, 0, sourceScale.Width - 1);
                            double sx = sourceScale.Luma[index], sy = decodedScale.Luma[index];
                            double weight = Gaussian[kernelIndex++];
                            meanY += sy * weight;
                            secondY += sy * sy * weight;
                            cross += sx * sy * weight;
                        }
                    }
                    int pixelIndex = (y * sourceScale.Width) + x;
                    double meanX = means[scaleIndex][pixelIndex], varianceX = variances[scaleIndex][pixelIndex];
                    double varianceY = Math.Max(0, secondY - (meanY * meanY));
                    double covariance = cross - (meanX * meanY);
                    double contrastStructure = ((2 * covariance) + C2) / (varianceX + varianceY + C2);
                    double value = scaleIndex == ScaleCount - 1
                        ? contrastStructure * (((2 * meanX * meanY) + C1) / ((meanX * meanX) + (meanY * meanY) + C1))
                        : contrastStructure;
                    spatialSum += value;
                }
            }
            double mean = Math.Clamp(spatialSum / sourceScale.Luma.Length, 0, 1);
            score *= Math.Pow(mean, Exponents[scaleIndex]);
        }
        return score;
    }

    private static void ValidateRaster(PpmImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Pixels is null || image.Width <= 0 || image.Height <= 0 ||
            (long)image.Width * image.Height * 3 != image.Pixels.Length)
        {
            throw new InvalidDataException("MS-SSIM requires a complete RGB8 raster with positive dimensions.");
        }
    }

    private static void ValidateDimensions(int width, int height, PpmImage decoded)
    {
        if (width != decoded.Width || height != decoded.Height)
        {
            throw new InvalidDataException("Decoder changed image dimensions.");
        }
    }

    private static double[] CreateGaussian()
    {
        double[] weights = new double[121];
        double sum = 0;
        int index = 0;
        for (int y = -5; y <= 5; y++)
        {
            for (int x = -5; x <= 5; x++)
            {
                double value = Math.Exp(-((x * x) + (y * y)) / (2 * 1.5 * 1.5));
                weights[index++] = value;
                sum += value;
            }
        }
        for (int kernelIndex = 0; kernelIndex < weights.Length; kernelIndex++)
        {
            weights[kernelIndex] /= sum;
        }
        return weights;
    }

    internal sealed record Scale(int Width, int Height, double[] Luma);
}

internal sealed class PreparedMultiScaleSsim
{
    private readonly MultiScaleSsim.Scale[] scales;
    private readonly double[][] means;
    private readonly double[][] variances;

    internal PreparedMultiScaleSsim(PpmImage source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.scales = MultiScaleSsim.CreatePyramid(source, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        this.means = new double[this.scales.Length][];
        this.variances = new double[this.scales.Length][];
        for (int index = 0; index < this.scales.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (this.means[index], this.variances[index]) = MultiScaleSsim.PrepareMoments(this.scales[index], cancellationToken);
        }
    }

    internal double Compare(PpmImage decoded, CancellationToken cancellationToken = default) =>
        MultiScaleSsim.ComparePrepared(this.scales, this.means, this.variances, decoded, cancellationToken);
}
