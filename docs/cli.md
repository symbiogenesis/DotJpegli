# Command-line encoder

[Documentation](README.md)

The `dotjpegli` CLI converts image files to JPEG using DotJpegli. Prebuilt downloads for Windows (`win-x64`, `win-arm64`), Linux (`linux-x64`, `linux-arm64`), and macOS (`osx-arm64`) are compiled with NativeAOT, fully trimmed, and self-contained—no .NET runtime installation is required.

## Install

Download an installer or portable archive for your platform from [GitHub Releases](https://github.com/symbiogenesis/DotJpegli/releases). Verify downloads against `SHA256SUMS`.

### Windows (`win-x64`, `win-arm64`)

- **MSI installer (`DotJpegli.Cli-<version>-<rid>.msi`)**: Installs for the current user and adds `dotjpegli` to the user `PATH`. Open a new terminal after installing and run `dotjpegli --help`. Repair or uninstall from Windows Settings.
- **Portable archive (`DotJpegli.Cli-<version>-<rid>.zip`)** and **standalone executable (`DotJpegli.Cli-<version>-<rid>.exe`)**: Extract the ZIP and run `dotjpegli.exe`, or download the standalone `.exe` and rename it to `dotjpegli.exe`.

Windows downloads are unsigned; verify them with `SHA256SUMS`.

### Linux (`linux-x64`, `linux-arm64`)

- **Debian / Ubuntu package (`.deb`)**: Installs `/usr/bin/dotjpegli` and license files under `/usr/share/doc/dotjpegli`:

  ```sh
  sudo dpkg -i DotJpegli.Cli-<version>-<rid>.deb
  ```

- **RPM package (`.rpm`)**: Installs `/usr/bin/dotjpegli` and license files on Fedora, RHEL, openSUSE, and compatible distributions:

  ```sh
  sudo rpm -Uvh DotJpegli.Cli-<version>-<rid>.rpm
  ```

- **AppImage (`.AppImage`)**: Single-file portable bundle (pass `--appimage-extract-and-run` on systems without FUSE):

  ```sh
  chmod +x DotJpegli.Cli-<version>-<rid>.AppImage
  ./DotJpegli.Cli-<version>-<rid>.AppImage --help
  ```

- **Flatpak bundle (`.flatpak`)**: Single-file bundle (`io.github.symbiogenesis.DotJpegli`) targeting `org.freedesktop.Platform//24.08`:

  ```sh
  flatpak install --user DotJpegli.Cli-<version>-<rid>.flatpak
  flatpak run io.github.symbiogenesis.DotJpegli encode input.png output.jpg
  ```

- **Portable tarball (`.tar.gz`)**: Extract and run `./dotjpegli` from any directory.

### macOS (`osx-arm64`)

- **Installer package (`DotJpegli.Cli-<version>-osx-arm64.pkg`)**: Installs `/usr/local/bin/dotjpegli` and license files under `/usr/local/share/doc/dotjpegli` on Apple Silicon Macs:

  ```sh
  sudo installer -pkg DotJpegli.Cli-<version>-osx-arm64.pkg -target /
  ```

- **Portable tarball (`DotJpegli.Cli-<version>-osx-arm64.tar.gz`)**: Extract and run `./dotjpegli`. The binary is ad-hoc signed (`codesign -s -`) for Apple Silicon; if macOS Gatekeeper prompts after a browser download, clear the quarantine attribute with `xattr -d com.apple.quarantine dotjpegli`.

Keep `LICENSE` and `THIRD-PARTY-NOTICES.md` with redistributed binaries. The `-symbols.zip` and `-symbols.tar.gz` archives contain native debugging symbols (`.pdb`, `.dbg`, `.dSYM`) and are not needed for normal use.

## Encode

```powershell
dotjpegli encode input.png output.jpg
dotjpegli encode input.png output.jpg --quality 90 --progressive
dotjpegli encode input.png output.jpg --distance 1 --subsampling 444 --threads 4
dotjpegli encode --help
```

- **Supported inputs**: PNG, JPEG, BMP, TGA, and other formats supported by StbImageSharp, plus binary P6 PPM.
- **Pixel handling**: Inputs are decoded to 8-bit RGB. Alpha channels are discarded (composite transparent images before encoding), and input metadata is not copied.
- **Output safety**: Existing output files are not overwritten unless `--overwrite` is passed.
- **Options**: Use `--quality` or `--distance` (mutually exclusive) to control compression. Run `dotjpegli encode --help` for all options and defaults.

## Build and package

Use the .NET SDK pinned in `global.json` and the native platform toolchain for the target RID. In PowerShell 7.6 or later:

```powershell
./tools/publish-cli.ps1 -RuntimeIdentifier win-x64 -OutputRoot C:/temp/dotjpegli-win-x64
./tools/publish-cli.ps1 -RuntimeIdentifier win-arm64 -OutputRoot C:/temp/dotjpegli-win-arm64
./tools/publish-cli.ps1 -RuntimeIdentifier linux-x64 -OutputRoot /tmp/dotjpegli-linux-x64
./tools/publish-cli.ps1 -RuntimeIdentifier linux-arm64 -OutputRoot /tmp/dotjpegli-linux-arm64
./tools/publish-cli.ps1 -RuntimeIdentifier osx-arm64 -OutputRoot /tmp/dotjpegli-osx-arm64
```

Each run writes the platform's release assets to `<OutputRoot>/release`:

- **Windows (`win-x64`, `win-arm64`)**: `.exe`, `.zip`, `.msi`, and `-symbols.zip` (requires Visual Studio C++ build tools, Windows SDK, and WiX 5.0.2).
- **Linux (`linux-x64`, `linux-arm64`)**: `.tar.gz`, `-symbols.tar.gz`, `.deb`, `.rpm`, `.AppImage`, and `.flatpak` (requires `clang`, `zlib1g-dev`, `dpkg-deb`, `rpmbuild`, `mksquashfs`, `rsvg-convert`, `desktop-file-validate`, `flatpak`, and `ostree`).
- **macOS (`osx-arm64`)**: `.tar.gz`, `-symbols.tar.gz`, and `.pkg` (requires Xcode command-line tools, `pkgbuild`, and `productbuild`).

CI builds, smoke-tests, and install-tests all five RIDs on native runners. See [publishing](publishing.md) for the release workflow.
