namespace DotJpegli.Parity;

internal sealed record QualityMetrics(double RgbMse, double RgbPsnrDb, double LumaSsim)
{
    public static QualityMetrics Compare(PpmImage source, PpmImage decoded)
    {
        if (source.Width != decoded.Width || source.Height != decoded.Height)
            throw new InvalidDataException("Decoder changed image dimensions.");
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
                        int offset = (y * source.Width + x) * 3;
                        double sx = Luma(source.Pixels, offset), sy = Luma(decoded.Pixels, offset);
                        sumX += sx; sumY += sy; sumXX += sx * sx; sumYY += sy * sy; sumXY += sx * sy;
                        count++;
                    }
                }
                double meanX = sumX / count, meanY = sumY / count;
                double varianceX = Math.Max(0, sumXX / count - meanX * meanX);
                double varianceY = Math.Max(0, sumYY / count - meanY * meanY);
                double covariance = sumXY / count - meanX * meanY;
                double ssim = ((2 * meanX * meanY + c1) * (2 * covariance + c2)) /
                    ((meanX * meanX + meanY * meanY + c1) * (varianceX + varianceY + c2));
                ssimSum += ssim * count;
                pixelCount += count;
            }
        }
        return new(mse, psnr, ssimSum / pixelCount);
    }

    private static double Luma(byte[] pixels, int offset) =>
        .299 * pixels[offset] + .587 * pixels[offset + 1] + .114 * pixels[offset + 2];
}
