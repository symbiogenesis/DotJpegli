using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class AdaptiveCoefficientTests
{
    [Theory]
    [InlineData(85f, 1, 1, 1.45039105f)]
    [InlineData(85f, 2, 2, 1.83017135f)]
    [InlineData(0f, 1, 1, 25.0020828f)]
    [InlineData(100f, 1, 1, 0.0100520607f)]
    public void DistanceInferenceMatchesPinnedCppFixtures(float quality,
        int horizontalSampling, int verticalSampling, float expected)
    {
        // Independently compiled with MSVC /O2 /fp:precise using the pinned
        // quant.cc QuantValsToDistance, ScaleToDistance and SetQuantMatrices
        // formulas: google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
        ushort[][] tables = CreateTables(quality, horizontalSampling, verticalSampling);
        float actual = Quantization.QuantValuesToDistance(tables);
        Assert.InRange(MathF.Abs(actual - expected), 0f, 0.00001f);
        if (quality == 0)
            Assert.Contains(tables.SelectMany(table => table), value => value == 255);
        if (quality == 100)
            Assert.Contains(tables.SelectMany(table => table), value => value == 1);
    }

    [Theory]
    [InlineData((ushort)1, 0.00750476215f)]
    [InlineData((ushort)255, 169.840317f)]
    public void SaturatedTableEndpointsRemainUnbounded(ushort quant, float expected)
    {
        // Pinned QuantValsToDistance fixtures with all entries at each
        // baseline endpoint, exercising both open interval branches.
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(_ => Enumerable.Repeat(quant, 64).ToArray()).ToArray();
        Assert.InRange(MathF.Abs(Quantization.QuantValuesToDistance(tables) - expected), 0f, 0.00002f);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void InterpolatedMultipliersMatchPinnedCppFixtures(int horizontalSampling, int verticalSampling)
    {
        // C++ InitQuantizer fixtures at Q85, frequencies 1, 2, 7 and 63.
        float[][] expected = verticalSampling == 2
            ? [[0.0261504892f, 0.308510125f, 0.733811140f, 0.601794302f],
                [1.11580300f, 1.00649536f, 0.712426782f, 1.00932932f],
                [0.862883806f, 0.568313360f, 0.405729949f, 0.515633285f]]
            : [[0.0162002463f, 0.282704055f, 0.771181524f, 0.684187651f],
                [1.10015607f, 1.02895939f, 0.637572050f, 1.03086281f],
                [0.714807510f, 0.612956524f, 0.441353321f, 0.583689928f]];
        float[][] actual = Quantization.CreateAdaptiveZeroBiasMultipliers(
            CreateTables(85, horizontalSampling, verticalSampling),
            horizontalSampling, verticalSampling, false);
        int[] frequencies = [1, 2, 7, 63];
        for (int component = 0; component < 3; component++)
        {
            Assert.Equal(0f, actual[component][0]);
            for (int i = 0; i < frequencies.Length; i++)
                Assert.InRange(MathF.Abs(expected[component][i] - actual[component][frequencies[i]]), 0f, 0.000001f);
        }
    }

    [Theory]
    [InlineData(95f, 0.0044f, 1.0556f, 0.6644f)]
    [InlineData(0f, 0.0568f, 0.9373f, 0.3060f)]
    public void MultiplierInterpolationClampsToPinnedHighAndLowQualityTables(
        float quality, float luma, float cb, float cr)
    {
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(CreateTables(quality), 1, 1, false);
        Assert.Equal(luma, multipliers[0][1]);
        Assert.Equal(cb, multipliers[1][2]);
        Assert.Equal(cr, multipliers[2][63]);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(85f)]
    [InlineData(100f)]
    public void GrayscaleUsesUniformAcMultipliers(float quality)
    {
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(
            [Quantization.CreateGrayscaleTable(quality)], 1, 1, true);
        Assert.Single(multipliers);
        Assert.Equal(0f, multipliers[0][0]);
        Assert.All(multipliers[0].Skip(1), value => Assert.Equal(0.5f, value));
    }

    [Theory]
    [InlineData(0, 0.59082f, 1f)]
    [InlineData(0, 0.59082f, -1f)]
    [InlineData(1, 0.58146f, 1f)]
    [InlineData(1, 0.58146f, -1f)]
    [InlineData(2, 0.57988f, 1f)]
    [InlineData(2, 0.57988f, -1f)]
    public void AcThresholdRetainsBoundaryAndSuppressesBothSigns(int component, float offset, float sign)
    {
        ushort[] table = UnitTable();
        float[] multipliers = new float[64];
        multipliers[1] = multipliers[2] = multipliers[3] = 1f;
        float threshold = offset + 1f;
        float[] coefficients = new float[64];
        coefficients[1] = sign * MathF.BitDecrement(threshold);
        coefficients[2] = sign * threshold;
        coefficients[3] = sign * MathF.BitIncrement(threshold);
        short[] output = new short[64];
        Quantization.Quantize(coefficients, table, output, component, 1f,
            adaptiveQuantization: true, adaptiveStrength: 1f, adaptiveMultipliers: multipliers);
        Assert.Equal((short)0, output[1]);
        Assert.Equal((short)(sign * 2), output[2]);
        Assert.Equal((short)(sign * 2), output[3]);
    }

    [Fact]
    public void AcThresholdUsesEachFrequencyMultiplier()
    {
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(CreateTables(95), 1, 1, false);
        float[] coefficients = new float[64];
        coefficients[1] = coefficients[63] = 1f;
        short[] output = new short[64];
        Quantization.Quantize(coefficients, UnitTable(), output, 0, 0.55f,
            adaptiveQuantization: true, adaptiveStrength: 1f, adaptiveMultipliers: multipliers[0]);
        Assert.Equal((short)1, output[1]);
        Assert.Equal((short)0, output[63]);
    }

    [Fact]
    public void GrayscaleAdaptiveThresholdUsesHalfOffsetAndMultiplier()
    {
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers([UnitTable()], 1, 1, true);
        float[] coefficients = new float[64];
        coefficients[1] = 1.49f;
        coefficients[2] = 1.5f;
        coefficients[3] = -1.49f;
        coefficients[4] = -1.5f;
        short[] output = new short[64];
        Quantization.Quantize(coefficients, UnitTable(), output, 0, 1f, grayscale: true,
            adaptiveQuantization: true, adaptiveStrength: 2f, adaptiveMultipliers: multipliers[0]);
        Assert.Equal(new short[] { 0, 0, 2, 0, -2 }, output.Take(5));
    }

    [Theory]
    [InlineData(11.6f, 11)]
    [InlineData(10.4f, 11)]
    [InlineData(11.75f, 12)]
    [InlineData(10.25f, 10)]
    public void DcThresholdUsesDifferenceFromPreviousDc(float dc, short expected)
    {
        float[] coefficients = new float[64];
        coefficients[0] = dc;
        float[] multipliers = new float[64];
        multipliers[0] = 1f;
        short[] output = new short[64];
        Quantization.Quantize(coefficients, UnitTable(), output, 0, 1f,
            adaptiveQuantization: true, adaptiveStrength: 0.75f,
            adaptiveMultipliers: multipliers, previousDc: 11);
        Assert.Equal(expected, output[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void PinnedDcMultiplierIsZeroAndRoundingIsIndependentOfPreviousDc(int component)
    {
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(CreateTables(85), 1, 1, false);
        float[] coefficients = new float[64];
        coefficients[0] = -0.5f;
        coefficients[1] = 2.5f;
        coefficients[2] = -3.5f;
        short[] output = new short[64];
        Quantization.Quantize(coefficients, UnitTable(), output, component, 1.45f,
            adaptiveQuantization: true, adaptiveStrength: 100f,
            adaptiveMultipliers: multipliers[component], previousDc: 77);
        Assert.Equal((short)-1, output[0]);
        Quantization.Quantize(coefficients, UnitTable(), output, component, 1.45f,
            adaptiveQuantization: true, adaptiveStrength: 0f,
            adaptiveMultipliers: multipliers[component], previousDc: 77);
        Assert.Equal((short)2, output[1]);
        Assert.Equal((short)-4, output[2]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AdaptiveOffKeepsExistingFixedThresholdAndIgnoresAdaptiveArguments(bool grayscale)
    {
        float[] coefficients = new float[64];
        coefficients[0] = -0.5f;
        coefficients[1] = 0.58f;
        coefficients[2] = -0.59f;
        coefficients[3] = 0.6f;
        coefficients[4] = 2.5f;
        coefficients[5] = -3.5f;
        short[] original = new short[64];
        short[] explicitOff = new short[64];
        Quantization.Quantize(coefficients, UnitTable(), original, 0, 1f, grayscale);
        Quantization.Quantize(coefficients, UnitTable(), explicitOff, 0, 1f, grayscale,
            adaptiveQuantization: false, adaptiveStrength: float.NaN,
            adaptiveMultipliers: [float.NaN], previousDc: 100);
        Assert.Equal(original, explicitOff);
        short[] expected = grayscale ? [-1, 1, -1, 1, 2, -4] : [-1, 0, 0, 1, 2, -4];
        Assert.Equal(expected, original.Take(6));
    }

    [Fact]
    public void AdaptiveInputsAreValidated()
    {
        float[] coefficients = new float[64];
        ushort[] table = UnitTable();
        short[] output = new short[64];
        float[] multipliers = new float[64];
        Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.Quantize(coefficients,
            table, output, 0, 1f, adaptiveQuantization: true, adaptiveStrength: float.NaN,
            adaptiveMultipliers: multipliers));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.Quantize(coefficients,
            table, output, 0, 1f, adaptiveQuantization: true, adaptiveStrength: -1f,
            adaptiveMultipliers: multipliers));
        Assert.Throws<ArgumentException>(() => Quantization.Quantize(coefficients,
            table, output, 0, 1f, adaptiveQuantization: true));
        multipliers[63] = float.NaN;
        Assert.Throws<ArgumentException>(() => Quantization.Quantize(coefficients,
            table, output, 0, 1f, adaptiveQuantization: true, adaptiveMultipliers: multipliers));
        Assert.Throws<ArgumentException>(() => Quantization.CreateAdaptiveZeroBiasMultipliers([table], 1, 1, false));
        Assert.Throws<ArgumentException>(() => Quantization.QuantValuesToDistance([new ushort[64]]));
    }

    private static ushort[][] CreateTables(float quality, int horizontalSampling = 1, int verticalSampling = 1)
        => Enumerable.Range(0, 3).Select(component =>
            Quantization.CreateTable(quality, component, horizontalSampling, verticalSampling)).ToArray();

    private static ushort[] UnitTable() => Enumerable.Repeat((ushort)1, 64).ToArray();
}
