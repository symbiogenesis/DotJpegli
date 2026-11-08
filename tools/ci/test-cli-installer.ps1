#requires -Version 7.6
<#
.SYNOPSIS
Checks CLI release archives, bundles, and OS installers on disposable CI hosts.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrWhiteSpace()][string] $ArtifactRoot,
    [Parameter(Mandatory)][ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-arm64')][string] $RuntimeIdentifier
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$ArtifactRoot = (Resolve-Path -LiteralPath $ArtifactRoot).ProviderPath
$release = Join-Path $ArtifactRoot 'release'
$extracted = Join-Path $ArtifactRoot 'extracted'
$automationProject = (Resolve-Path -LiteralPath "$PSScriptRoot/../DotJpegli.Automation/DotJpegli.Automation.csproj").ProviderPath
dotnet build $automationProject -c Release --nologo -v q
$automation = Join-Path (Split-Path $automationProject -Parent) 'bin/Release/net10.0/DotJpegli.Automation.dll'
$version = @(dotnet $automation cli-release --artifact-root $ArtifactRoot --rid $RuntimeIdentifier)[-1]
$assetName = "DotJpegli.Cli-$version-$RuntimeIdentifier"
$portableExecutable = Join-Path $extracted ($RuntimeIdentifier.StartsWith('win-', [StringComparison]::Ordinal) ? 'dotjpegli.exe' : 'dotjpegli')

function Test-InstalledCli([string] $Executable, [string] $SmokeName, [string] $DocDir = '', [string] $InstalledBinary = '', [string[]] $ExtraFiles = @()) {
    $arguments = @('cli-release', '--smoke', $Executable, '--output-root', (Join-Path $ArtifactRoot $SmokeName),
        '--version', $version, '--rid', $RuntimeIdentifier, '--portable-binary', $portableExecutable)
    if ($DocDir) { $arguments += @('--doc-dir', $DocDir) }
    if ($InstalledBinary) { $arguments += @('--installed-binary', $InstalledBinary) }
    foreach ($file in $ExtraFiles) { $arguments += @('--extra-file', $file) }
    dotnet $automation @arguments
}

if ($RuntimeIdentifier.StartsWith('win-', [StringComparison]::Ordinal)) {
    if (-not $IsWindows -or $env:GITHUB_ACTIONS -ne 'true') {
        throw 'Windows installer smoke checks require a disposable Windows GitHub Actions runner.'
    }
    $installer = Join-Path $release "$assetName.msi"
    $installedDirectory = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) "Programs/DotJpegli/$($RuntimeIdentifier.Substring(4))"
    $installedExecutable = Join-Path $installedDirectory 'dotjpegli.exe'
    if (Test-Path -LiteralPath $installedDirectory) { throw 'A pre-existing CLI installation must not be modified.' }
    $originalUserPath = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
    $pathState = [ordered]@{ original = $originalUserPath }

    function Invoke-MsiCommand([string] $Operation, [string] $LogName) {
        $log = Join-Path $ArtifactRoot $LogName
        $process = Start-Process -FilePath (Join-Path $env:SystemRoot 'System32/msiexec.exe') -Wait -PassThru `
            -WindowStyle Hidden -ArgumentList @($Operation, ('"{0}"' -f $installer), '/qn', '/norestart', '/l*v', ('"{0}"' -f $log))
        try {
            if ($process.ExitCode -notin 0, 3010) { throw "Windows Installer failed with $($process.ExitCode); see $log." }
        } finally {
            $process.Dispose()
        }
    }

    function Assert-InstalledPath {
        if (@(([string] [Environment]::GetEnvironmentVariable('PATH', 'User')).Split(';') |
                Where-Object { $_.TrimEnd('\') -ieq $installedDirectory.TrimEnd('\') }).Count -ne 1) {
            throw 'The installer must add exactly one user PATH entry.'
        }
    }

    try {
        Invoke-MsiCommand '/i' 'msi-install.log'
        $pathState['installed'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        Assert-InstalledPath
        Test-InstalledCli $installedExecutable 'installed-smoke'
        Remove-Item -LiteralPath $installedExecutable
        Invoke-MsiCommand '/fa' 'msi-repair.log'
        $pathState['repaired'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        Assert-InstalledPath
        Test-InstalledCli $installedExecutable 'repaired-smoke'
    } finally {
        Invoke-MsiCommand '/x' 'msi-uninstall.log'
        $pathState['uninstalled'] = [string] [Environment]::GetEnvironmentVariable('PATH', 'User')
        $pathState | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $ArtifactRoot 'msi-path-state.log') -Encoding utf8NoBOM
    }
    if (Test-Path -LiteralPath $installedExecutable) { throw 'Uninstall left the CLI executable behind.' }
    dotnet $automation cli-release --assert-path-restored --original $originalUserPath --current $pathState['uninstalled'] --installed-dir $installedDirectory
    Write-Output 'CLI distribution checks passed: standalone EXE, portable/symbols archives and MSI install, repair, and uninstall.'
    return
}

if ($RuntimeIdentifier -eq 'osx-arm64') {
    if (-not $IsMacOS -or $env:GITHUB_ACTIONS -ne 'true') {
        throw 'macOS installer smoke checks require a disposable macOS GitHub Actions runner.'
    }
    $installedExecutable = '/usr/local/bin/dotjpegli'
    $installedDocDir = '/usr/local/share/doc/dotjpegli'
    if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
        throw 'A pre-existing macOS CLI installation must not be modified.'
    }
    try {
        sudo installer -pkg (Join-Path $release "$assetName.pkg") -target /
        $null = pkgutil --pkg-info io.github.symbiogenesis.dotjpegli.cli
        Test-InstalledCli $installedExecutable 'pkg-smoke' -DocDir $installedDocDir
    } finally {
        sudo rm -f $installedExecutable
        sudo rm -rf $installedDocDir
        sudo pkgutil --forget io.github.symbiogenesis.dotjpegli.cli
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
$installedExecutable = '/usr/bin/dotjpegli'
$installedDocDir = '/usr/share/doc/dotjpegli'
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'A pre-existing Linux CLI installation must not be modified.'
}

try {
    sudo dpkg -i (Join-Path $release "$assetName.deb")
    Test-InstalledCli $installedExecutable 'deb-smoke' -DocDir $installedDocDir -ExtraFiles @((Join-Path $installedDocDir 'copyright'))
} finally {
    sudo dpkg --purge dotjpegli
}
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'DEB purge left CLI files behind.'
}

try {
    sudo rpm -i --nodeps (Join-Path $release "$assetName.rpm")
    Test-InstalledCli $installedExecutable 'rpm-smoke' -DocDir $installedDocDir
} finally {
    sudo rpm -e dotjpegli
}
if ((Test-Path -LiteralPath $installedExecutable) -or (Test-Path -LiteralPath $installedDocDir)) {
    throw 'RPM erase left CLI files behind.'
}

$appImagePath = Join-Path $release "$assetName.AppImage"
$appImageExtractRoot = Join-Path $ArtifactRoot 'appimage-extracted'
$null = [IO.Directory]::CreateDirectory($appImageExtractRoot)
Push-Location -LiteralPath $appImageExtractRoot
try {
    $null = & $appImagePath --appimage-extract
} finally {
    Pop-Location
}
$squashfsRoot = Join-Path $appImageExtractRoot 'squashfs-root'
$previousExtractAndRun = $env:APPIMAGE_EXTRACT_AND_RUN
$env:APPIMAGE_EXTRACT_AND_RUN = '1'
try {
    Test-InstalledCli $appImagePath 'appimage-smoke' -DocDir (Join-Path $squashfsRoot 'usr/share/doc/dotjpegli') -InstalledBinary (Join-Path $squashfsRoot 'usr/bin/dotjpegli')
} finally {
    $env:APPIMAGE_EXTRACT_AND_RUN = $previousExtractAndRun
}

$flatpakArch = $RuntimeIdentifier -eq 'linux-arm64' ? 'aarch64' : 'x86_64'
$appId = 'io.github.symbiogenesis.DotJpegli'
$flatpakCheckRepo = Join-Path $ArtifactRoot 'flatpak-check-repo'
$flatpakCheckout = Join-Path $ArtifactRoot 'flatpak-checkout'
ostree "--repo=$flatpakCheckRepo" init --mode=archive-z2
flatpak build-import-bundle $flatpakCheckRepo (Join-Path $release "$assetName.flatpak")
ostree "--repo=$flatpakCheckRepo" checkout --user-mode "app/$appId/$flatpakArch/stable" $flatpakCheckout
$flatpakMetadata = Get-Content -LiteralPath (Join-Path $flatpakCheckout 'metadata') -Raw
if (-not $flatpakMetadata.Contains("name=$appId", [StringComparison]::Ordinal) -or
    -not $flatpakMetadata.Contains('command=dotjpegli', [StringComparison]::Ordinal)) {
    throw 'Flatpak metadata is missing the application ID or command.'
}
Test-InstalledCli (Join-Path $flatpakCheckout 'files/bin/dotjpegli') 'flatpak-smoke' `
    -DocDir (Join-Path $flatpakCheckout 'files/share/doc/dotjpegli') -ExtraFiles @(
    (Join-Path $flatpakCheckout "export/share/applications/$appId.desktop"),
    (Join-Path $flatpakCheckout "export/share/icons/hicolor/scalable/apps/$appId.svg"),
    (Join-Path $flatpakCheckout "files/share/metainfo/$appId.appdata.xml"))
Write-Output 'CLI distribution checks passed: Linux portable/symbols tarballs, DEB, RPM, AppImage and Flatpak.'
