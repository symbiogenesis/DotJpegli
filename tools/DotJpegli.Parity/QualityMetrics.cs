namespace DotJpegli.Parity;

internal sealed record QualityMetrics(double RgbMse, double RgbPsnrDb, double LumaSsim)
{
    internal static PreparedQualityMetrics Prepare(PpmImage source) => new(source);

    public static QualityMetrics Compare(PpmImage source, PpmImage decoded)
    {
        if (source.Width != decoded.Width || source.Height != decoded.Height)
        {
            throw new InvalidDataException("Decoder changed image dimensions.");
        }

        double squareError = 0;
        for (int index = 0; index < source.Pixels.Length; index++)
        {
            double error = source.Pixels[index] - decoded.Pixels[index];
            squareError += error * error;
        }
        double mse = squareError / source.Pixels.Length;
        double psnr = mse == 0 ? 120 : Math.Min(120, 10 * Math.Log10(255 * 255 / mse));
        const double c1 = .01 * .01 * 255 * 255, c2 = .03 * .03 * 255 * 255;
        double ssimSum = 0;
        long pixelCount = 0;
        for (int top = 0; top < source.Height; top += 8)
        {
            for (int left = 0; left < source.Width; left += 8)
            {
                double sumX = 0, sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0;
                int count = 0;
                for (int y = top; y < Math.Min(top + 8, source.Height); y++)
                {
                    for (int x = left; x < Math.Min(left + 8, source.Width); x++)
                    {
                        int offset = ((y * source.Width) + x) * 3;
                        double sx = Luma(source.Pixels, offset), sy = Luma(decoded.Pixels, offset);
                        sumX += sx;
                        sumY += sy;
                        sumXX += sx * sx;
                        sumYY += sy * sy;
                        sumXY += sx * sy;
                        count++;
                    }
                }
                double meanX = sumX / count, meanY = sumY / count;
                double varianceX = Math.Max(0, (sumXX / count) - (meanX * meanX));
                double varianceY = Math.Max(0, (sumYY / count) - (meanY * meanY));
                double covariance = (sumXY / count) - (meanX * meanY);
                double ssim = ((2 * meanX * meanY) + c1) * ((2 * covariance) + c2) /
                    (((meanX * meanX) + (meanY * meanY) + c1) * (varianceX + varianceY + c2));
                ssimSum += ssim * count;
                pixelCount += count;
            }
        }
        return new(mse, psnr, ssimSum / pixelCount);
    }

    private static double Luma(byte[] pixels, int offset) =>
        (.299 * pixels[offset]) + (.587 * pixels[offset + 1]) + (.114 * pixels[offset + 2]);
}

// Offline search state only. The snapshot and moments are private and immutable,
// so repeated comparisons cannot observe later changes to the caller's raster.
internal sealed class PreparedQualityMetrics
{
    private readonly int width;
    private readonly int height;
    private readonly byte[] pixels;
    private readonly double[] luma;
    private readonly Window[] windows;

    internal PreparedQualityMetrics(PpmImage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateRaster(source);
        this.width = source.Width;
        this.height = source.Height;
        this.pixels = source.Pixels.ToArray();
        this.luma = new double[this.pixels.Length / 3];
        this.windows = new Window[(((this.width - 1) / 8) + 1) * (((this.height - 1) / 8) + 1)];
        int windowIndex = 0;
        for (int top = 0; top < this.height; top += 8)
        {
            for (int left = 0; left < this.width; left += 8)
            {
                double sumX = 0, sumXX = 0;
                int count = 0;
                for (int y = top; y < Math.Min(top + 8, this.height); y++)
                {
                    for (int x = left; x < Math.Min(left + 8, this.width); x++)
                    {
                        int index = (y * this.width) + x;
                        double sx = Luma(this.pixels, index * 3);
                        this.luma[index] = sx;
                        sumX += sx;
                        sumXX += sx * sx;
                        count++;
                    }
                }
                double meanX = sumX / count;
                double varianceX = Math.Max(0, (sumXX / count) - (meanX * meanX));
                this.windows[windowIndex++] = new(meanX, varianceX, count);
            }
        }
    }

    internal QualityMetrics Compare(PpmImage decoded)
    {
        ArgumentNullException.ThrowIfNull(decoded);
        if (this.width != decoded.Width || this.height != decoded.Height)
        {
            throw new InvalidDataException("Decoder changed image dimensions.");
        }
        ValidateRaster(decoded);
        double squareError = 0;
        for (int index = 0; index < this.pixels.Length; index++)
        {
            double error = this.pixels[index] - decoded.Pixels[index];
            squareError += error * error;
        }
        double mse = squareError / this.pixels.Length;
        double psnr = mse == 0 ? 120 : Math.Min(120, 10 * Math.Log10(255 * 255 / mse));
        const double c1 = .01 * .01 * 255 * 255, c2 = .03 * .03 * 255 * 255;
        double ssimSum = 0;
        long pixelCount = 0;
        int windowIndex = 0;
        for (int top = 0; top < this.height; top += 8)
        {
            for (int left = 0; left < this.width; left += 8)
            {
                double sumY = 0, sumYY = 0, sumXY = 0;
                for (int y = top; y < Math.Min(top + 8, this.height); y++)
                {
                    for (int x = left; x < Math.Min(left + 8, this.width); x++)
                    {
                        int index = (y * this.width) + x;
                        double sx = this.luma[index], sy = Luma(decoded.Pixels, index * 3);
                        sumY += sy;
                        sumYY += sy * sy;
                        sumXY += sx * sy;
                    }
                }
                Window window = this.windows[windowIndex++];
                double meanX = window.Mean, meanY = sumY / window.Count;
                double varianceX = window.Variance;
                double varianceY = Math.Max(0, (sumYY / window.Count) - (meanY * meanY));
                double covariance = (sumXY / window.Count) - (meanX * meanY);
                double ssim = ((2 * meanX * meanY) + c1) * ((2 * covariance) + c2) /
                    (((meanX * meanX) + (meanY * meanY) + c1) * (varianceX + varianceY + c2));
                ssimSum += ssim * window.Count;
                pixelCount += window.Count;
            }
        }
        return new(mse, psnr, ssimSum / pixelCount);
    }

    private static void ValidateRaster(PpmImage image)
    {
        if (image.Pixels is null || image.Width <= 0 || image.Height <= 0 ||
            (long)image.Width * image.Height * 3 != image.Pixels.Length)
        {
            throw new InvalidDataException("Quality metrics require a complete RGB8 raster with positive dimensions.");
        }
    }

    private static double Luma(byte[] pixels, int offset) =>
        (.299 * pixels[offset]) + (.587 * pixels[offset + 1]) + (.114 * pixels[offset + 2]);

    private readonly record struct Window(double Mean, double Variance, int Count);
}
