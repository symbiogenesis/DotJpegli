namespace DotJpegli;

/// <summary>Color components stored in the output JPEG, independently of the source layout.</summary>
public enum JpegColorSpace
{
    /// <summary>Grayscale for grayscale input, XYB for scaled XYB input, and YCbCr for other color input.</summary>
    Automatic = 0,

    /// <summary>One grayscale component: converted luminance for RGB input, or the supplied Y component.</summary>
    Grayscale = 1,

    /// <summary>Y, Cb and Cr components. Grayscale input supplies neutral chroma.</summary>
    YCbCr = 2,

    /// <summary>R, G and B components without an RGB-to-YCbCr transform. YCbCr input is not supported.</summary>
    Rgb = 3,

    /// <summary>
    /// Jpegli scaled XYB components, stored with RGB JPEG identifiers.
    /// Converts sRGB or grayscale input through sRGB linearization and the opsin transform,
    /// or accepts already scaled XYB input. Direct YCbCr input is unsupported.
    /// Requires an explicitly assigned ICC profile describing the scaled output channels;
    /// JpegliIccProfiles.CreateXybProfile provides the built-in profile.
    /// No source ICC transform is performed.
    /// </summary>
    Xyb = 4
}
