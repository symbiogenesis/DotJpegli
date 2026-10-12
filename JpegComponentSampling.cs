namespace DotJpegli;

/// <summary>Horizontal and vertical eight-by-eight blocks for one component in a color MCU.</summary>
/// <param name="Horizontal">Horizontal sampling factor, from 1 to 4.</param>
/// <param name="Vertical">Vertical sampling factor, from 1 to 4.</param>
public readonly record struct JpegComponentSampling(int Horizontal, int Vertical);
