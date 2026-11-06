# Command-line encoder

[Documentation](README.md)

The `dotjpegli` CLI converts image files into JPEG using DotJpegli. Windows
(`win-x64`, `win-arm64`), Linux (`linux-x64`, `linux-arm64`) and macOS
(`osx-arm64`) downloads are compiled with NativeAOT, fully trimmed and
self-contained; users do not need to install .NET. The CLI targets .NET 10 and
defines its command API with System.CommandLine.

## Install

Download a package or portable archive matching your operating system and
processor from [GitHub Releases](https://github.com/symbiogenesis/DotJpegli/releases).
`SHA256SUMS` lists the SHA-256 of each release download.

### Windows (`win-x64`, `win-arm64`)

- **MSI installer (`DotJpegli.Cli-<version>-<rid>.msi`)**: Installs for the
  current user and adds its directory to the user `PATH`. Open a new terminal
  after installing, then run `dotjpegli --help`. Windows Settings can repair or
  uninstall it. A newer MSI upgrades the same architecture; uninstall removes
  the installed files and its `PATH` entry.
- **Portable archive (`DotJpegli.Cli-<version>-<rid>.zip`)** and **standalone
  executable (`DotJpegli.Cli-<version>-<rid>.exe`)**: Extract the ZIP and run
  `dotjpegli.exe` directly, or download the standalone `.exe` and rename it to
  `dotjpegli.exe`.

MSI provides direct installation, standard repair/uninstall and `PATH`
integration. MSIX requires a trusted signing identity for normal installation,
so this project uses MSI for direct GitHub distribution. The Windows artifacts
have no Authenticode signature; checksums identify the files published with the
release.

### Linux (`linux-x64`, `linux-arm64`)

- **Debian / Ubuntu package (`DotJpegli.Cli-<version>-<rid>.deb`)**: Installs
  `/usr/bin/dotjpegli` and license documentation under `/usr/share/doc/dotjpegli`:

  ```sh
  sudo dpkg -i DotJpegli.Cli-<version>-<rid>.deb
  ```

- **RPM package (`DotJpegli.Cli-<version>-<rid>.rpm`)**: Installs
  `/usr/bin/dotjpegli` and license documentation under
  `/usr/share/doc/dotjpegli` on Fedora, RHEL, openSUSE and compatible
  distributions:

  ```sh
  sudo rpm -Uvh DotJpegli.Cli-<version>-<rid>.rpm
  ```

- **AppImage (`DotJpegli.Cli-<version>-<rid>.AppImage`)**: Single-file portable
  Type 2 AppImage bundle. Make it executable and run it directly (or pass
  `--appimage-extract-and-run` on systems without FUSE):

  ```sh
  chmod +x DotJpegli.Cli-<version>-<rid>.AppImage
  ./DotJpegli.Cli-<version>-<rid>.AppImage --help
  ```

- **Flatpak bundle (`DotJpegli.Cli-<version>-<rid>.flatpak`)**: Single-file
  Flatpak bundle (`io.github.symbiogenesis.DotJpegli`) targeting
  `org.freedesktop.Platform//24.08`:

  ```sh
  flatpak install --user DotJpegli.Cli-<version>-<rid>.flatpak
  flatpak run io.github.symbiogenesis.DotJpegli encode input.png output.jpg
  ```

- **Portable tarball (`DotJpegli.Cli-<version>-<rid>.tar.gz`)**: Extract and run
  `dotjpegli` from any directory.

### macOS (`osx-arm64`)

- **Installer package (`DotJpegli.Cli-<version>-osx-arm64.pkg`)**: Installs
  `/usr/local/bin/dotjpegli` and license files under
  `/usr/local/share/doc/dotjpegli` on Apple Silicon Macs:

  ```sh
  sudo installer -pkg DotJpegli.Cli-<version>-osx-arm64.pkg -target /
  ```

- **Portable tarball (`DotJpegli.Cli-<version>-osx-arm64.tar.gz`)**: Extract and
  run `./dotjpegli` directly. The Mach-O binary is ad-hoc signed (`codesign -s -`)
  for Apple Silicon execution; it is not notarized with an Apple Developer ID, so
  remove the quarantine extended attribute (`xattr -d com.apple.quarantine`) if
  macOS Gatekeeper prompts after browser download.

Keep the included `LICENSE`, `LICENSE.jpegli` and `THIRD-PARTY-NOTICES.md` files
with redistributed binaries. The separate `-symbols.zip` (Windows) and
`-symbols.tar.gz` (Linux and macOS) downloads contain native debugging symbols
(`.pdb`, `.dbg` and `.dSYM`) and are not needed to run the program.

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

Use the SDK pinned in the repository's `global.json` and the native platform
toolchain for the target RID. In PowerShell 7.6 or later:

```powershell
./tools/publish-cli.ps1 -RuntimeIdentifier win-x64 -OutputRoot C:/temp/dotjpegli-win-x64
./tools/publish-cli.ps1 -RuntimeIdentifier win-arm64 -OutputRoot C:/temp/dotjpegli-win-arm64
./tools/publish-cli.ps1 -RuntimeIdentifier linux-x64 -OutputRoot /tmp/dotjpegli-linux-x64
./tools/publish-cli.ps1 -RuntimeIdentifier linux-arm64 -OutputRoot /tmp/dotjpegli-linux-arm64
./tools/publish-cli.ps1 -RuntimeIdentifier osx-arm64 -OutputRoot /tmp/dotjpegli-osx-arm64
```

Use a fresh output directory. Each invocation produces a `release` directory
containing the platform's release assets:

- **Windows (`win-x64`, `win-arm64`)**: `.exe`, `.zip`, `.msi` and `-symbols.zip`
  (requires Visual Studio C++ build tools, Windows SDK and WiX 5.0.2).
- **Linux (`linux-x64`, `linux-arm64`)**: `.tar.gz`, `-symbols.tar.gz`, `.deb`,
  `.rpm`, `.AppImage` and `.flatpak` (requires `clang`, `zlib1g-dev`, `dpkg-deb`,
  `rpmbuild`, `mksquashfs`, `rsvg-convert`, `desktop-file-validate`, `flatpak`
  and `ostree`).
- **macOS (`osx-arm64`)**: `.tar.gz`, `-symbols.tar.gz` and `.pkg` (requires
  Xcode command-line tools, `pkgbuild` and `productbuild`).

The executable and installers use the library's committed version. Combined
releases require a stable three-part version within Windows Installer's numeric
limits.

CI publishes all five RIDs on their native GitHub-hosted runners, exercises the
published executable, verifies that packaged binaries match the portable
archive, and tests package installation, execution and removal before retaining
the artifacts. See [publishing](publishing.md) for the release process.
