# Command-line encoder

[Documentation](README.md)

The `dotjpegli` CLI converts image files into JPEG using DotJpegli. Windows x64
and ARM64 downloads are compiled with NativeAOT, fully trimmed and self-contained;
users do not need to install .NET. The CLI targets .NET 10 and defines its command
API with System.CommandLine.

## Install

Download the MSI matching your processor from
[GitHub Releases](https://github.com/symbiogenesis/DotJpegli/releases).
The installer installs for the current user and adds its directory to the user
`PATH`. Open a new terminal after installing, then run `dotjpegli --help`.
Windows Settings can repair or uninstall it. A newer MSI upgrades the same
architecture; uninstall removes the installed files and its PATH entry.

For portable use, extract the matching ZIP and run `dotjpegli.exe` directly.
Keep the included license and third-party notices with redistributed binaries.
The separate `-symbols.zip` download is for debugging and is not needed to run
the program. `SHA256SUMS` lists the SHA-256 of each release download.

MSI provides direct installation, standard repair/uninstall and PATH integration.
MSIX requires a trusted signing identity for normal installation, so this project
uses MSI for direct GitHub distribution. The artifacts currently have no
Authenticode signature; checksums identify the files published with the release.

## Encode

```powershell
dotjpegli encode input.png output.jpg
dotjpegli encode input.png output.jpg --quality 90 --progressive
dotjpegli encode input.png output.jpg --distance 1 --subsampling 444 --threads 4
dotjpegli encode --help
```

The CLI reads PNG, JPEG, BMP, TGA and other formats supported by StbImageSharp,
as well as binary P6 PPM. Input pixels are decoded to 8-bit RGB. Alpha is discarded;
composite transparent images before encoding. Input metadata is not copied to
the JPEG. Existing output files are preserved unless `--overwrite` is supplied.
Quality and distance are alternative controls; `--help` lists valid values and
defaults. Errors return a nonzero exit code and diagnostics on standard error.

## Build and package

Use the SDK pinned in the repository's `global.json`. NativeAOT on Windows also
requires Visual Studio C++ build tools and a Windows SDK. In PowerShell 7.6 or later:

```powershell
./tools/publish-cli.ps1 -RuntimeIdentifier win-x64 -OutputRoot C:/temp/dotjpegli-x64
./tools/publish-cli.ps1 -RuntimeIdentifier win-arm64 -OutputRoot C:/temp/dotjpegli-arm64
```

Use a fresh output directory. Each invocation produces a `release` directory
containing `DotJpegli.Cli-<version>-<rid>.zip`, `.msi` and `-symbols.zip` files.
The executable and installer use the library's committed version. Combined MSI
releases require a stable three-part version within Windows Installer's numeric
limits. The packaging script pins WiX 5.0.2, which predates the maintenance-fee
and EULA requirements introduced by later WiX versions.

CI publishes both architectures on their native Windows runners, exercises the
published executable, and checks MSI installation, repair and removal before
retaining the artifacts. See [publishing](publishing.md) for the release process.
