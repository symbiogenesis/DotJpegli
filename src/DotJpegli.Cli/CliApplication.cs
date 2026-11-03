namespace DotJpegli.Cli;

using System.CommandLine;
using System.Globalization;
using DotJpegli.Parity;
using StbImageSharp;

internal static class CliApplication
{
    internal static int Run(string[] args, TextWriter output, TextWriter error)
    {
        var input = new Argument<string>("input") { Description = "Input image (PNG, JPEG, BMP, TGA, GIF, PSD, HDR or binary P6 PPM)." };
        var destination = new Argument<string>("output") { Description = "Destination JPEG file." };
        var quality = new Option<float>("--quality", "-q") { Description = "Jpegli quality from 0 to 100 (not the libjpeg quality scale).", DefaultValueFactory = _ => 85 };
        var distance = new Option<float?>("--distance") { Description = "Positive perceptual distance, overriding quality; smaller values request finer quantization." };
        var sampling = new Option<string>("--subsampling") { Description = "Chroma subsampling: 444, 422 or 420.", DefaultValueFactory = _ => "420" };
        var progressive = new Option<bool>("--progressive") { Description = "Write a progressive JPEG." };
        var threads = new Option<int>("--threads") { Description = "Maximum encoder workers (positive integer).", DefaultValueFactory = _ => 1 };
        var overwrite = new Option<bool>("--overwrite") { Description = "Replace an existing output file after encoding succeeds." };

        quality.Validators.Add(result =>
        {
            if (result.Tokens.Count == 1 && (!float.TryParse(result.Tokens[0].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                || !float.IsFinite(value) || value is < 0 or > 100))
            {
                result.AddError("Quality must be finite and between 0 and 100.");
            }
        });
        distance.Validators.Add(result =>
        {
            if (result.Tokens.Count == 1 && (!float.TryParse(result.Tokens[0].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                || !float.IsFinite(value) || value <= 0))
            {
                result.AddError("Distance must be finite and greater than zero.");
            }
        });
        _ = sampling.AcceptOnlyFromAmong("444", "422", "420");
        threads.Validators.Add(result =>
        {
            if (result.Tokens.Count == 1 && (!int.TryParse(result.Tokens[0].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value <= 0))
            {
                result.AddError("Threads must be a positive integer.");
            }
        });

        var encode = new Command("encode", "Encode an image as an 8-bit JPEG. Input metadata and alpha are discarded; animated inputs use the first frame.")
        {
            input, destination, quality, distance, sampling, progressive, threads, overwrite
        };
        encode.SetAction(result => Encode(result.GetValue(input)!, result.GetValue(destination)!, new()
        {
            Quality = result.GetValue(quality),
            Distance = result.GetValue(distance),
            Subsampling = result.GetValue(sampling) switch
            {
                "444" => ChromaSubsampling.None,
                "422" => ChromaSubsampling.HalfHorizontal,
                _ => ChromaSubsampling.Quarter
            },
            Progressive = result.GetValue(progressive),
            MaxDegreeOfParallelism = result.GetValue(threads)
        }, result.GetValue(overwrite), output, error));

        var root = new RootCommand("DotJpegli JPEG encoder. No .NET runtime installation is required for the native release executable.") { encode };
        return root.Parse(args).Invoke(new() { Output = output, Error = error, EnableDefaultExceptionHandler = false });
    }

    private static int Encode(string inputPath, string outputPath, JpegliEncoderOptions options, bool overwrite, TextWriter output, TextWriter error)
    {
        string? temporaryPath = null;
        try
        {
            string sourcePath = ResolveFileDirectory(Path.GetFullPath(inputPath));
            string destinationPath = ResolveFileDirectory(Path.GetFullPath(outputPath));
            StringComparison pathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var source = new FileInfo(sourcePath);
            FileSystemInfo? sourceTarget = (source.Attributes & FileAttributes.ReparsePoint) != 0 ? source.ResolveLinkTarget(returnFinalTarget: true) : null;
            if (string.Equals(sourcePath, destinationPath, pathComparison)
                || (sourceTarget is not null && string.Equals(ResolveFileDirectory(sourceTarget.FullName), destinationPath, pathComparison)))
            {
                throw new IOException("Input and output must be different files.");
            }
            if (!overwrite && File.Exists(destinationPath))
            {
                throw new IOException("Output already exists. Use --overwrite to replace it.");
            }

            ImageResult image = ReadImage(sourcePath);
            string directory = Path.GetDirectoryName(destinationPath)!;
            temporaryPath = Path.Combine(directory, $".dotjpegli-{Guid.NewGuid():N}.tmp");
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JpegliEncoder.Encode(stream, image.Data, image.Width, image.Height, options: options);
            }
            File.Move(temporaryPath, destinationPath, overwrite);
            temporaryPath = null;
            output.WriteLine($"Encoded {image.Width} x {image.Height}: {outputPath}");
            return 0;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            error.WriteLine($"dotjpegli: {exception.Message}");
            return 1;
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    private static string ResolveFileDirectory(string path) =>
        Path.Combine(ResolveDirectory(new DirectoryInfo(Path.GetDirectoryName(path)!)), Path.GetFileName(path));

    private static string ResolveDirectory(DirectoryInfo directory)
    {
        if (directory.Parent is null)
        {
            return directory.FullName;
        }
        // Resolve every ancestor: ResolveLinkTarget on a descendant does not resolve its parent links.
        var resolved = new DirectoryInfo(Path.Combine(ResolveDirectory(directory.Parent), directory.Name));
        FileSystemInfo? target = (resolved.Attributes & FileAttributes.ReparsePoint) != 0 ? resolved.ResolveLinkTarget(returnFinalTarget: true) : null;
        return target is null ? resolved.FullName : ResolveDirectory(new DirectoryInfo(target.FullName));
    }

    private static ImageResult ReadImage(string path)
    {
        using FileStream stream = File.OpenRead(path);
        int first = stream.ReadByte();
        int second = stream.ReadByte();
        stream.Position = 0;
        if (first == 'P' && second == '6')
        {
            if (stream.Length > int.MaxValue)
            {
                throw new InvalidDataException("Input exceeds the PPM buffer limit.");
            }
            byte[] data = new byte[(int)stream.Length];
            stream.ReadExactly(data);
            var image = PpmImage.Read(data);
            if (image.Width > 65535 || image.Height > 65535)
            {
                throw new InvalidDataException("Input exceeds JPEG dimensions.");
            }
            return new() { Width = image.Width, Height = image.Height, Data = image.Pixels, Comp = ColorComponents.RedGreenBlue, SourceComp = ColorComponents.RedGreenBlue };
        }
        ImageInfo? info = ImageInfo.FromStream(stream);
        if (info is not { Width: > 0 and <= 65535, Height: > 0 and <= 65535 } dimensions || (long)dimensions.Width * dimensions.Height * 3 > int.MaxValue)
        {
            throw new InvalidDataException("Input is unsupported, corrupt, or exceeds JPEG dimensions or the RGB buffer limit.");
        }
        stream.Position = 0;
        try
        {
            return ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidDataException($"Cannot decode input image: {exception.Message}", exception);
        }
    }
}
