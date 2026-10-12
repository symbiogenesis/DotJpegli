using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class RgbToXybNumericalTests
{
    // Fixtures were emitted by an independent MSVC /fp:strict scalar C++ probe
    // using google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20:
    // TF_SRGB's rational polynomial, CubeRootAndAdd's Newton iterations,
    // opsin_params.h's float constants, and ScaleXYBRow's operation ordering.
    // Inputs are normalized encoded sRGB; outputs are fractional JPEG samples.
    // Managed pow/cbrt intentionally replace the native approximations. On a
    // 33^3 native color grid their largest difference was 0.001397 JPEG samples
    // in X. 0.0025 allows float rounding with a tolerance of 1/400 sample.
    [Theory]
    [InlineData(0f, 0f, 0f, 90.2231598f, -4.49515892e-06f, 106.37365f)]
    [InlineData(1f, 1f, 1f, 90.2231598f, 254.999985f, 106.37365f)]
    [InlineData(1f, 0f, 0f, 255.000183f, 147.269287f, 100.042213f)]
    [InlineData(0f, 1f, 0f, -0.000174758621f, 215.624542f, -3.4246983e-05f)]
    [InlineData(0f, 0f, 1f, 90.2231598f, 83.9015427f, 255f)]
    [InlineData(1f, 1f, 0f, 90.2231598f, 246.964645f, 27.0093651f)]
    [InlineData(1f, 0f, 1f, 229.083557f, 164.654327f, 193.00647f)]
    [InlineData(0f, 1f, 1f, 6.47602987f, 225.594879f, 109.798897f)]
    [InlineData(0.00392156886f, 0.00392156886f, 0.00392156886f, 90.223114f, 1.22282934f, 106.37365f)]
    [InlineData(0.0392156877f, 0.0392156877f, 0.0392156877f, 90.223114f, 10.1849003f, 106.37365f)]
    [InlineData(0.0431372561f, 0.0431372561f, 0.0431372561f, 90.2231598f, 11.0416346f, 106.37365f)]
    [InlineData(0.501960814f, 0.501960814f, 0.501960814f, 90.2231598f, 134.96788f, 106.37365f)]
    [InlineData(0.996078432f, 0.996078432f, 0.996078432f, 90.2231598f, 254.104904f, 106.37365f)]
    [InlineData(0.0666666701f, 0.380392164f, 0.905882359f, 62.126339f, 111.839455f, 202.566986f)]
    [InlineData(0.949019611f, 0.239215687f, 0.419607848f, 224.57785f, 150.351761f, 115.137978f)]
    [InlineData(0.00196078443f, 0.50686276f, 0.995098054f, 49.5708046f, 136.62645f, 195.261383f)]
    [InlineData(0.0404499955f, 0.119607843f, 0.65539217f, 84.5830841f, 59.1103134f, 203.123428f)]
    [InlineData(0.040450003f, 0.119607843f, 0.65539217f, 84.5834274f, 59.1103249f, 203.123413f)]
    [InlineData(0.0404499993f, 0.0404499993f, 0.0404499993f, 90.2231598f, 10.4505758f, 106.373634f)]
    [InlineData(-0.0392156877f, 0.0470588244f, 0.0980392173f, 76.5588684f, 8.25160217f, 116.33754f)]
    [InlineData(1.05882359f, 0.5f, -0.00980392192f, 198.704559f, 183.337677f, 77.0556946f)]
    [InlineData(-1f, -1f, -1f, 90.2231598f, -47.045929f, 106.37365f)]
    public void MatchesPinnedNativeScalarFixtures(float r, float g, float b,
        float expectedX, float expectedY, float expectedBMinusY)
    {
        XybColorConversion.SrgbToScaledXyb(r, g, b, out float x, out float y, out float bMinusY);
        AssertClose(expectedX, x, 0.0025, "X");
        AssertClose(expectedY, y, 0.0025, "Y");
        AssertClose(expectedBMinusY, bMinusY, 0.0025, "B-Y");
    }

    [Fact]
    public void MatchesIndependentDoubleDefinitionAcrossColorCubeAndTransferBoundary()
    {
        float[] values = [0, 0.001f, 0.01f, MathF.BitDecrement(0.04045f), 0.04045f,
            MathF.BitIncrement(0.04045f), 0.0625f, 0.25f, 0.5f, 0.75f, 1];
        foreach (float r in values)
            foreach (float g in values)
                foreach (float b in values)
                    AssertMatchesDoubleDefinition(r, g, b);
    }

    [Theory]
    [InlineData(-1f, -1f, -1f)]
    [InlineData(-0.25f, 0.05f, 0.5f)]
    [InlineData(1.1f, 0.5f, -0.01f)]
    [InlineData(1.25f, 0f, 0f)]
    [InlineData(0.5f, -0.1f, 1.5f)]
    public void ExtendedSrgbMirrorsTransferSignAndClampsOnlyOpsinAbsorbance(float r, float g, float b)
    {
        AssertMatchesDoubleDefinition(r, g, b);
    }

    [Fact]
    public void NeutralRampPreservesChromaOffsetsAndIncreasingLuminance()
    {
        const double expectedX = 0.015386134 * 22.995788804 * 255;
        const double expectedBMinusY = 0.27770459 * 1.502141333 * 255;
        float previousY = float.NegativeInfinity;
        for (int sample = 0; sample <= 255; sample++)
        {
            float encoded = sample / 255f;
            XybColorConversion.SrgbToScaledXyb(encoded, encoded, encoded,
                out float x, out float y, out float bMinusY);
            AssertClose(expectedX, x, 0.001, "neutral X");
            AssertClose(expectedBMinusY, bMinusY, 0.001, "neutral B-Y");
            Assert.True(y > previousY, $"Neutral sample {sample} did not increase Y.");
            previousY = y;
        }
        AssertClose(255, previousY, 0.001, "white Y");
    }

    private static void AssertMatchesDoubleDefinition(float r, float g, float b)
    {
        (double expectedX, double expectedY, double expectedBMinusY) = DoubleDefinition(r, g, b);
        XybColorConversion.SrgbToScaledXyb(r, g, b, out float x, out float y, out float bMinusY);
        string input = $"({r:R}, {g:R}, {b:R})";
        AssertClose(expectedX, x, 0.001, $"X {input}");
        AssertClose(expectedY, y, 0.001, $"Y {input}");
        AssertClose(expectedBMinusY, bMinusY, 0.001, $"B-Y {input}");
    }

    // Independent analytic definition: double transfer, matrix-vector product,
    // biased cone response, XYB projection, then affine JPEG sample scaling.
    // This uses neither the implementation's constants nor its scalar helper.
    private static (double X, double Y, double BMinusY) DoubleDefinition(double r, double g, double b)
    {
        double[] rgb = [Linearize(r), Linearize(g), Linearize(b)];
        double[,] matrix =
        {
            { 0.30, 0.622, 0.078 },
            { 0.23, 0.692, 0.078 },
            { 0.24342268924547819, 0.20476744424496821, 0.5518098665095536 }
        };
        const double bias = 0.0037930732552754493;
        double[] cones = new double[3];
        for (int row = 0; row < 3; row++)
        {
            double absorbance = bias;
            for (int column = 0; column < 3; column++)
                absorbance += matrix[row, column] * rgb[column];
            cones[row] = Math.Cbrt(Math.Max(0, absorbance)) - Math.Cbrt(bias);
        }
        double x = (cones[0] - cones[1]) / 2;
        double y = (cones[0] + cones[1]) / 2;
        return ((x + 0.015386134) * 22.995788804 * 255,
            y * 1.183000077 * 255,
            (cones[2] - y + 0.27770459) * 1.502141333 * 255);

        static double Linearize(double encoded)
        {
            double magnitude = Math.Abs(encoded);
            double linear = magnitude <= 0.04045 ? magnitude / 12.92
                : Math.Pow((magnitude + 0.055) / 1.055, 2.4);
            return Math.CopySign(linear, encoded);
        }
    }

    private static void AssertClose(double expected, float actual, double tolerance, string label)
    {
        Assert.True(float.IsFinite(actual) && Math.Abs(expected - actual) <= tolerance,
            $"{label}: expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
    }
}
