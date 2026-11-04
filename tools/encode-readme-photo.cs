#:project ../src/DotJpegli/DotJpegli.csproj
#:property PublishAot=false
#:property RestorePackagesWithLockFile=false

// Untimed single-photo encoding for generate-readme-comparison.py.
// Run with the repository's pinned .NET 11 SDK; input is packed RGB8.
using System.Globalization;
using DotJpegli;

if (args.Length != 4)
{
    throw new ArgumentException("Expected INPUT.rgb OUTPUT.jpg WIDTH HEIGHT.");
}

byte[] pixels = File.ReadAllBytes(args[0]);
int width = int.Parse(args[2], CultureInfo.InvariantCulture);
int height = int.Parse(args[3], CultureInfo.InvariantCulture);
File.WriteAllBytes(args[1], JpegliEncoder.Encode(pixels, width, height, options: new()
{
    Distance = 1,
    Subsampling = ChromaSubsampling.Quarter,
    ProgressiveLevel = 2,
    ForceBaseline = true,
    OptimizeHuffmanTables = true,
    AdaptiveQuantization = true,
    ColorSpace = JpegColorSpace.YCbCr,
    MaxDegreeOfParallelism = 1
}));
