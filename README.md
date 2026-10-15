# DotJpegli

A fully managed C# JPEG encoder for .NET 10 and .NET 11, based on [Google Jpegli](https://github.com/google/jpegli).
No native binaries, unsafe code or runtime NuGet dependencies.

Encode RGB or grayscale pixels as baseline or progressive JPEG, with configurable
quality, sampling and metadata.
Both target frameworks support trimming and NativeAOT publishing.

**Experimental:** This is an encoder-only port. Output is lossy eight-bit JPEG;
full Jpegli output, compression and performance parity has not been established.
Quality/compression parity has been measured within a
[documented RGB8 YCbCr scope](docs/quality-compression-parity.md).

## Getting started

Building from source requires .NET SDK `11.0.100-rc.2.26504.105`, pinned in
`global.json` with prerelease SDKs enabled. The library supports .NET 10 and
.NET 11 applications; its .NET 11 build uses newer portable SIMD APIs.
From this repository's root, add a project reference to your application,
replacing `path/to/YourApp.csproj` with its project path:

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

To publish an application that references DotJpegli, select its target framework
and runtime identifier, for example:

```sh
dotnet publish path/to/YourApp.csproj -c Release -f net10.0 -r linux-x64 --self-contained true -p:PublishTrimmed=true
dotnet publish path/to/YourApp.csproj -c Release -f net10.0 -r linux-x64 -p:PublishAot=true
```

Use `net11.0` for a .NET 11 application and the RID for your deployment platform.
NativeAOT requires the platform's
[native build tools](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites).
DotJpegli requires no additional linker descriptors or preservation attributes.
See [deployment validation](docs/development.md#trimming-and-nativeaot) for the
library's smoke checks.

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
