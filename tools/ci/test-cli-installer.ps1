#requires -Version 7.6
<#
.SYNOPSIS
Checks CLI release archives, bundles, and OS installers on disposable CI hosts.
.PARAMETER ArtifactRoot
OutputRoot produced by tools/publish-cli.ps1.
.PARAMETER RuntimeIdentifier
Native platform and architecture of the artifacts being checked.
.PARAMETER SelfTest
Checks PATH restoration and expected release asset sets using synthetic values.
.EXAMPLE
./tools/ci/test-cli-installer.ps1 -ArtifactRoot C:/build/cli -RuntimeIdentifier win-x64
.EXAMPLE
./tools/ci/test-cli-installer.ps1 -ArtifactRoot /tmp/cli -RuntimeIdentifier linux-x64
#>
[CmdletBinding(DefaultParameterSetName = 'Installer')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Installer')][ValidateNotNullOrWhiteSpace()][string] $ArtifactRoot,
    [Parameter(Mandatory, ParameterSetName = 'Installer')][ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-arm64')][string] $RuntimeIdentifier,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

function Get-ExpectedReleaseAssetSet([string] $Version, [string] $TargetRid) {
    $stem = "DotJpegli.Cli-$Version-$TargetRid"
    if ($TargetRid.StartsWith('win-', [StringComparison]::Ordinal)) {
        return @("$stem.exe", "$stem.msi", "$stem.zip", "$stem-symbols.zip")
    }
    if ($TargetRid.StartsWith('linux-', [StringComparison]::Ordinal)) {
        return @("$stem.AppImage", "$stem.deb", "$stem.flatpak", "$stem.rpm", "$stem.tar.gz", "$stem-symbols.tar.gz")
    }
    if ($TargetRid -eq 'osx-arm64') {
        return @("$stem.pkg", "$stem.tar.gz", "$stem-symbols.tar.gz")
    }
    throw "Unsupported CLI RuntimeIdentifier: $TargetRid"
}

function Assert-UserPathRestored([string] $Original, [string] $Current, [string] $InstalledDirectory) {
    # Windows Installer can normalize empty entries when editing a PATH list.
    # Compare every nonempty entry in order, retaining duplicate entries and spelling.
    $originalEntries = $Original.Split(';', [StringSplitOptions]::RemoveEmptyEntries)
    $currentEntries = $Current.Split(';', [StringSplitOptions]::RemoveEmptyEntries)
    if (@($currentEntries | Where-Object { $_.TrimEnd('\') -ieq $InstalledDirectory.TrimEnd('\') }).Count -ne 0) {
        throw 'Uninstall left the CLI directory in the user PATH.'
    }
    if (-not [Linq.Enumerable]::SequenceEqual[string]($originalEntries, $currentEntries, [StringComparer]::Ordinal)) {
        $state = @{ original = $Original; current = $Current } | ConvertTo-Json -Compress
        throw "Uninstall changed the original user PATH entries: $state"
    }
}

if ($SelfTest) {
    $installed = 'C:\Programs\DotJpegli\x64'
    foreach ($restored in @('C:\Windows;C:\Tools', 'C:\Windows;C:\Tools;', ';C:\Windows;;C:\Tools;')) {
        Assert-UserPathRestored -Original 'C:\Windows;C:\Tools;' -Current $restored -InstalledDirectory $installed
    }
    Assert-UserPathRestored -Original '' -Current ';' -InstalledDirectory $installed
    foreach ($invalid in @('C:\Windows', 'C:\Tools;C:\Windows', 'C:\Windows;C:\Tools;C:\Unexpected',
            "C:\Windows;C:\Tools;$installed\")) {
        $rejected = $false
        try { Assert-UserPathRestored -Original 'C:\Windows;C:\Tools;' -Current $invalid -InstalledDirectory $installed } catch {
            $rejected = $true
        }
        if (-not $rejected) { throw 'PATH self-test accepted changed entries or a retained CLI directory.' }
    }
    $rejected = $false
    try { Assert-UserPathRestored -Original 'C:\Tools;C:\Tools' -Current 'C:\Tools' -InstalledDirectory $installed } catch {
        $rejected = $true
    }
    if (-not $rejected) { throw 'PATH self-test accepted a removed duplicate entry.' }

    if ((Get-ExpectedReleaseAssetSet -Version '1.2.3' -TargetRid 'win-x64').Count -ne 4 -or
        (Get-ExpectedReleaseAssetSet -Version '1.2.3' -TargetRid 'win-arm64').Count -ne 4 -or
        (Get-ExpectedReleaseAssetSet -Version '1.2.3' -TargetRid 'linux-x64').Count -ne 6 -or
        (Get-ExpectedReleaseAssetSet -Version '1.2.3' -TargetRid 'linux-arm64').Count -ne 6 -or
        (Get-ExpectedReleaseAssetSet -Version '1.2.3' -TargetRid 'osx-arm64').Count -ne 3) {
        throw 'Release asset matrix self-test failed.'
    }
    Write-Output 'Installer PATH and release asset matrix self-test passed.'
    return
}

$ArtifactRoot = (Resolve-Path -LiteralPath $ArtifactRoot).ProviderPath
$release = Join-Path $ArtifactRoot 'release'
$assets = @(Get-ChildItem -LiteralPath $release -File)
if ($assets.Count -eq 0) { throw 'No CLI release assets were found.' }
$versionMatch = [regex]::Match($assets[0].Name, '\ADotJpegli\.Cli-([0-9]+\.[0-9]+\.[0-9]+)-' + [regex]::Escape($RuntimeIdentifier) + '(?:-symbols)?\.')
if (-not $versionMatch.Success) {
    throw "Unexpected release asset filename: $($assets[0].Name)"
}
$version = $versionMatch.Groups[1].Value
$assetName = "DotJpegli.Cli-$version-$RuntimeIdentifier"
$expectedNames = @(Get-ExpectedReleaseAssetSet -Version $version -TargetRid $RuntimeIdentifier)
if (@(Compare-Object -ReferenceObject $expectedNames -DifferenceObject @($assets.Name) -CaseSensitive).Count -ne 0) {
    throw 'CLI release files do not match the expected version, architecture and complete artifact set.'
}
$portableNotices = @('LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md', 'LICENSE.dotnet.txt', 'THIRD-PARTY-NOTICES.dotnet.txt', 'README.md')
$extracted = Join-Path $ArtifactRoot 'extracted'
$symbols = Join-Path $ArtifactRoot 'extracted-symbols'
$null = [IO.Directory]::CreateDirectory($extracted)
$null = [IO.Directory]::CreateDirectory($symbols)

if ($RuntimeIdentifier.StartsWith('win-', [StringComparison]::Ordinal)) {
    if (-not $IsWindows -or $env:GITHUB_ACTIONS -ne 'true') {
        throw 'Windows installer smoke checks require a disposable Windows GitHub Actions runner.'
    }
    $installer = Join-Path $release "$assetName.msi"
    Expand-Archive -LiteralPath (Join-Path $release "$assetName.zip") -DestinationPath $extracted
    Expand-Archive -LiteralPath (Join-Path $release "$assetName-symbols.zip") -DestinationPath $symbols
    foreach ($name in @('dotjpegli.exe') + $portableNotices) {
        if (-not (Test-Path -LiteralPath (Join-Path $extracted $name) -PathType Leaf)) {
            throw "Portable archive is missing $name."
        }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $symbols 'dotjpegli.pdb') -PathType Leaf)) {
        throw 'Symbols archive is missing the NativeAOT PDB.'
    }
    $expectedHash = (Get-FileHash -LiteralPath (Join-Path $extracted 'dotjpegli.exe') -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath (Join-Path $release "$assetName.exe") -Algorithm SHA256).Hash -ne $expectedHash) {
        throw 'The standalone executable differs from the portable executable.'
    }
    $binary = [IO.File]::ReadAllBytes((Join-Path $extracted 'dotjpegli.exe'))
    $peOffset = [BitConverter]::ToInt32($binary, 0x3c)
    $machine = [BitConverter]::ToUInt16($binary, $peOffset + 4)
    $expectedMachine = $RuntimeIdentifier -eq 'win-arm64' ? 0xaa64 : 0x8664
    if ($machine -ne $expectedMachine) { throw 'The executable does not match the release architecture.' }
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath (Join-Path $extracted 'dotjpegli.exe') `
        -OutputRoot (Join-Path $ArtifactRoot 'portable-smoke') -ExpectedVersion $version

    $architecture = $RuntimeIdentifier.Substring(4)
    $installedDirectory = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) "Programs/DotJpegli/$architecture"
    $installedExecutable = Join-Path $installedDirectory 'dotjpegli.exe'
    if (Test-Path -LiteralPath $installedDirectory) { throw 'A pre-existing CLI installation must not be modified.' }
    $originalUserPath = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
    $pathState = [ordered] @{ original = $originalUserPath }
    $pathStateLog = Join-Path $ArtifactRoot 'msi-path-state.log'

    function Invoke-MsiCommand([string] $Operation, [string] $LogName) {
        $log = Join-Path $ArtifactRoot $LogName
        $process = Start-Process -FilePath (Join-Path $env:SystemRoot 'System32/msiexec.exe') -Wait -PassThru `
            -WindowStyle Hidden -ArgumentList @($Operation, ('"{0}"' -f $installer), '/qn', '/norestart', '/l*v', ('"{0}"' -f $log))
        try {
            if ($process.ExitCode -notin 0, 3010) {
                throw "Windows Installer failed with $($process.ExitCode); see $log."
            }
        } finally {
            $process.Dispose()
        }
    }

    function Assert-InstalledPath {
        $entries = @(([string] [Environment]::GetEnvironmentVariable('PATH', 'User')).Split(';') |
            Where-Object { $_.TrimEnd('\') -ieq $installedDirectory.TrimEnd('\') })
        if ($entries.Count -ne 1) { throw 'The installer must add exactly one user PATH entry.' }
    }

    try {
        Invoke-MsiCommand -Operation '/i' -LogName 'msi-install.log'
        $pathState['installed'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        Assert-InstalledPath
        & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $installedExecutable `
            -OutputRoot (Join-Path $ArtifactRoot 'installed-smoke') -ExpectedVersion $version
        if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
            throw 'The installed executable differs from the portable executable.'
        }
        # The exact file is under the just-created installation on this disposable host.
        Remove-Item -LiteralPath $installedExecutable
        Invoke-MsiCommand -Operation '/fa' -LogName 'msi-repair.log'
        $pathState['repaired'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        Assert-InstalledPath
        if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
            throw 'MSI repair did not restore the executable.'
        }
    } finally {
        Invoke-MsiCommand -Operation '/x' -LogName 'msi-uninstall.log'
        $pathState['uninstalled'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        $pathState | ConvertTo-Json | Set-Content -LiteralPath $pathStateLog -Encoding utf8NoBOM
    }
    if (Test-Path -LiteralPath $installedExecutable) { throw 'Uninstall left the CLI executable behind.' }
    Assert-UserPathRestored -Original $originalUserPath -Current $pathState['uninstalled'] -InstalledDirectory $installedDirectory
    Write-Output 'CLI distribution checks passed: standalone EXE, portable/symbols archives and MSI install, repair, and uninstall.'
    return
}

if ($RuntimeIdentifier -eq 'osx-arm64') {
    if (-not $IsMacOS -or $env:GITHUB_ACTIONS -ne 'true') {
        throw 'macOS installer smoke checks require a disposable macOS GitHub Actions runner.'
    }
    & tar -xzf (Join-Path $release "$assetName.tar.gz") -C $extracted
    & tar -xzf (Join-Path $release "$assetName-symbols.tar.gz") -C $symbols
    foreach ($name in @('dotjpegli') + $portableNotices) {
        if (-not (Test-Path -LiteralPath (Join-Path $extracted $name) -PathType Leaf)) {
            throw "Portable archive is missing $name."
        }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $symbols 'dotjpegli.dSYM/Contents/Resources/DWARF/dotjpegli') -PathType Leaf)) {
        throw 'Symbols archive is missing the Mach-O DWARF bundle.'
    }
    $portableExecutable = Join-Path $extracted 'dotjpegli'
    $expectedHash = (Get-FileHash -LiteralPath $portableExecutable -Algorithm SHA256).Hash
    $binary = [IO.File]::ReadAllBytes($portableExecutable)
    if ([BitConverter]::ToUInt32($binary, 0) -ne 0xfeedfacfu -or [BitConverter]::ToUInt32($binary, 4) -ne 0x0100000cu) {
        throw 'The macOS executable does not match the ARM64 Mach-O architecture.'
    }
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $portableExecutable `
        -OutputRoot (Join-Path $ArtifactRoot 'portable-smoke') -ExpectedVersion $version

    $installedExecutable = '/usr/local/bin/dotjpegli'
    $installedDocDir = '/usr/local/share/doc/dotjpegli'
    if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
        throw 'A pre-existing macOS CLI installation must not be modified.'
    }
    try {
        & sudo installer -pkg (Join-Path $release "$assetName.pkg") -target /
        $null = & pkgutil --pkg-info io.github.symbiogenesis.dotjpegli.cli
        foreach ($name in $portableNotices) {
            if (-not (Test-Path -LiteralPath (Join-Path $installedDocDir $name) -PathType Leaf)) {
                throw "macOS installer omitted documentation file $name."
            }
        }
        if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
            throw 'The PKG-installed executable differs from the portable executable.'
        }
        & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $installedExecutable `
            -OutputRoot (Join-Path $ArtifactRoot 'pkg-smoke') -ExpectedVersion $version
    } finally {
        & sudo rm -f $installedExecutable
        & sudo rm -rf $installedDocDir
        & sudo pkgutil --forget io.github.symbiogenesis.dotjpegli.cli
    }
    if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
        throw 'Uninstall left macOS PKG files behind.'
    }
    Write-Output 'CLI distribution checks passed: macOS portable/symbols tarballs and PKG install and removal.'
    return
}

if (-not $IsLinux -or $env:GITHUB_ACTIONS -ne 'true') {
    throw 'Linux package smoke checks require a disposable Linux GitHub Actions runner.'
}
& tar -xzf (Join-Path $release "$assetName.tar.gz") -C $extracted
& tar -xzf (Join-Path $release "$assetName-symbols.tar.gz") -C $symbols
foreach ($name in @('dotjpegli') + $portableNotices) {
    if (-not (Test-Path -LiteralPath (Join-Path $extracted $name) -PathType Leaf)) {
        throw "Portable archive is missing $name."
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $symbols 'dotjpegli.dbg') -PathType Leaf)) {
    throw 'Symbols archive is missing the NativeAOT ELF debug file.'
}
$portableExecutable = Join-Path $extracted 'dotjpegli'
$expectedHash = (Get-FileHash -LiteralPath $portableExecutable -Algorithm SHA256).Hash
$expectedMachine = $RuntimeIdentifier -eq 'linux-arm64' ? 0xb7 : 0x3e
foreach ($elfPath in @($portableExecutable, (Join-Path $symbols 'dotjpegli.dbg'))) {
    $elfBytes = [IO.File]::ReadAllBytes($elfPath)
    if ($elfBytes.Length -lt 64 -or $elfBytes[0] -ne 0x7f -or $elfBytes[1] -ne 0x45 -or
        $elfBytes[2] -ne 0x4c -or $elfBytes[3] -ne 0x46 -or
        [BitConverter]::ToUInt16($elfBytes, 18) -ne $expectedMachine) {
        throw "ELF binary or symbol file does not match $RuntimeIdentifier`: $elfPath"
    }
}
& (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $portableExecutable `
    -OutputRoot (Join-Path $ArtifactRoot 'portable-smoke') -ExpectedVersion $version

$installedExecutable = '/usr/bin/dotjpegli'
$installedDocDir = '/usr/share/doc/dotjpegli'
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'A pre-existing Linux CLI installation must not be modified.'
}

# Verify Debian (.deb) package install, execution and purge.
try {
    & sudo dpkg -i (Join-Path $release "$assetName.deb")
    foreach ($name in $portableNotices + @('copyright')) {
        if (-not (Test-Path -LiteralPath (Join-Path $installedDocDir $name) -PathType Leaf)) {
            throw "DEB package omitted documentation file $name."
        }
    }
    if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
        throw 'The DEB-installed executable differs from the portable executable.'
    }
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $installedExecutable `
        -OutputRoot (Join-Path $ArtifactRoot 'deb-smoke') -ExpectedVersion $version
} finally {
    & sudo dpkg --purge dotjpegli
}
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'DEB purge left CLI files behind.'
}

# Verify RPM (.rpm) package install, execution and erase.
try {
    & sudo rpm -i --nodeps (Join-Path $release "$assetName.rpm")
    foreach ($name in $portableNotices) {
        if (-not (Test-Path -LiteralPath (Join-Path $installedDocDir $name) -PathType Leaf)) {
            throw "RPM package omitted documentation file $name."
        }
    }
    if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
        throw 'The RPM-installed executable differs from the portable executable.'
    }
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $installedExecutable `
        -OutputRoot (Join-Path $ArtifactRoot 'rpm-smoke') -ExpectedVersion $version
} finally {
    & sudo rpm -e dotjpegli
}
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'RPM erase left CLI files behind.'
}

# Verify AppImage (.AppImage) Type 2 header, embedded payload and direct execution.
$appImagePath = Join-Path $release "$assetName.AppImage"
$appImageBytes = [IO.File]::ReadAllBytes($appImagePath)
if ($appImageBytes.Length -lt 64 -or $appImageBytes[8] -ne 0x41 -or $appImageBytes[9] -ne 0x49 -or
    $appImageBytes[10] -ne 0x02 -or [BitConverter]::ToUInt16($appImageBytes, 18) -ne $expectedMachine) {
    throw 'The AppImage does not have a valid Type 2 header for the target architecture.'
}
$appImageExtractRoot = Join-Path $ArtifactRoot 'appimage-extracted'
$null = [IO.Directory]::CreateDirectory($appImageExtractRoot)
Push-Location -LiteralPath $appImageExtractRoot
try {
    $null = & $appImagePath --appimage-extract
} finally {
    Pop-Location
}
$squashfsRoot = Join-Path $appImageExtractRoot 'squashfs-root'
if ((Get-FileHash -LiteralPath (Join-Path $squashfsRoot 'usr/bin/dotjpegli') -Algorithm SHA256).Hash -ne $expectedHash) {
    throw 'The AppImage executable differs from the portable executable.'
}
foreach ($name in $portableNotices) {
    if (-not (Test-Path -LiteralPath (Join-Path $squashfsRoot "usr/share/doc/dotjpegli/$name") -PathType Leaf)) {
        throw "AppImage omitted documentation file $name."
    }
}
$previousExtractAndRun = $env:APPIMAGE_EXTRACT_AND_RUN
$env:APPIMAGE_EXTRACT_AND_RUN = '1'
try {
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $appImagePath `
        -OutputRoot (Join-Path $ArtifactRoot 'appimage-smoke') -ExpectedVersion $version
} finally {
    $env:APPIMAGE_EXTRACT_AND_RUN = $previousExtractAndRun
}

# Verify Flatpak (.flatpak) single-file bundle import, metadata, exported assets and executable.
$flatpakArch = $RuntimeIdentifier -eq 'linux-arm64' ? 'aarch64' : 'x86_64'
$appId = 'io.github.symbiogenesis.DotJpegli'
$flatpakCheckRepo = Join-Path $ArtifactRoot 'flatpak-check-repo'
$flatpakCheckout = Join-Path $ArtifactRoot 'flatpak-checkout'
& ostree "--repo=$flatpakCheckRepo" init --mode=archive-z2
& flatpak build-import-bundle $flatpakCheckRepo (Join-Path $release "$assetName.flatpak")
& ostree "--repo=$flatpakCheckRepo" checkout --user-mode "app/$appId/$flatpakArch/stable" $flatpakCheckout
$flatpakMetadata = Get-Content -LiteralPath (Join-Path $flatpakCheckout 'metadata') -Raw
if (-not $flatpakMetadata.Contains("name=$appId", [StringComparison]::Ordinal) -or
    -not $flatpakMetadata.Contains('command=dotjpegli', [StringComparison]::Ordinal)) {
    throw 'Flatpak metadata is missing the application ID or command.'
}
foreach ($exportedFile in @(
        "export/share/applications/$appId.desktop",
        "export/share/icons/hicolor/scalable/apps/$appId.svg",
        "files/share/metainfo/$appId.appdata.xml")) {
    if (-not (Test-Path -LiteralPath (Join-Path $flatpakCheckout $exportedFile) -PathType Leaf)) {
        throw "Flatpak bundle is missing $exportedFile."
    }
}
foreach ($name in $portableNotices) {
    if (-not (Test-Path -LiteralPath (Join-Path $flatpakCheckout "files/share/doc/dotjpegli/$name") -PathType Leaf)) {
        throw "Flatpak bundle omitted documentation file $name."
    }
}
$flatpakExecutable = Join-Path $flatpakCheckout 'files/bin/dotjpegli'
if ((Get-FileHash -LiteralPath $flatpakExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
    throw 'The Flatpak executable differs from the portable executable.'
}
& (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $flatpakExecutable `
    -OutputRoot (Join-Path $ArtifactRoot 'flatpak-smoke') -ExpectedVersion $version

Write-Output 'CLI distribution checks passed: Linux portable/symbols tarballs, DEB, RPM, AppImage and Flatpak.'
