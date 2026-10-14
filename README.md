# DotJpegli

A fully managed C# JPEG encoder for .NET 10, based on [Google Jpegli](https://github.com/google/jpegli).
No native binaries, unsafe code or runtime NuGet dependencies.

Encode RGB or grayscale pixels as baseline or progressive JPEG, with configurable
quality, sampling and metadata.

**Experimental:** This is an encoder-only port. Output is lossy eight-bit JPEG;
full Jpegli output, compression and performance parity has not been established.

## Getting started

Requires the .NET 10 SDK. From this repository's root, add a project reference to
your .NET 10 application, replacing `path/to/YourApp.csproj` with its project path:

```sh
dotnet add path/to/YourApp.csproj reference src/DotJpegli/DotJpegli.csproj
```

Pass an RGB pixel buffer and its dimensions to the encoder:

```csharp
using DotJpegli;

byte[] rgbPixels = [255, 0, 0]; // One red pixel, in R, G, B order.
byte[] jpeg = JpegliEncoder.Encode(rgbPixels, width: 1, height: 1, options: new()
{
    Quality = 85
});
File.WriteAllBytes("output.jpg", jpeg);
```

## Documentation

- [Encoding](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/encoding.md): pixel layouts, sampling, streams and color spaces.
- [Profiles and metadata](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/metadata.md): ICC profiles, Exif, XMP and other metadata.
- [Compression](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/compression.md) and [quantization](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/quantization.md): quality and advanced options.
- [Port status](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/port-status.md): implemented features, limitations and upstream provenance.
- [Development](https://github.com/symbiogenesis/DotJpegli/blob/main/docs/development.md): builds, focused tests and validation coverage.

## License

BSD-3-Clause. See [LICENSE](https://github.com/symbiogenesis/DotJpegli/blob/main/LICENSE),
[upstream license](https://github.com/symbiogenesis/DotJpegli/blob/main/LICENSE.jpegli) and
[third-party notices](https://github.com/symbiogenesis/DotJpegli/blob/main/THIRD-PARTY-NOTICES.md).
