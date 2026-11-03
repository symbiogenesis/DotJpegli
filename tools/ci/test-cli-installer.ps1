#requires -Version 7.6
<#
.SYNOPSIS
Checks portable release assets and MSI install, repair, and removal on disposable CI hosts.
.PARAMETER ArtifactRoot
OutputRoot produced by tools/publish-cli.ps1.
.PARAMETER RuntimeIdentifier
Native architecture of the artifacts being checked.
.EXAMPLE
./tools/ci/test-cli-installer.ps1 -ArtifactRoot C:/build/cli -RuntimeIdentifier win-x64
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrWhiteSpace()][string] $ArtifactRoot,
    [Parameter(Mandatory)][ValidateSet('win-x64', 'win-arm64')][string] $RuntimeIdentifier
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
if (-not $IsWindows -or $env:GITHUB_ACTIONS -ne 'true') {
    throw 'Installer smoke checks require a disposable Windows GitHub Actions runner.'
}
$ArtifactRoot = (Resolve-Path -LiteralPath $ArtifactRoot).ProviderPath
$release = Join-Path $ArtifactRoot 'release'
$assets = @(Get-ChildItem -LiteralPath $release -File)
$installers = @($assets | Where-Object Extension -EQ '.msi')
if ($assets.Count -ne 3 -or $installers.Count -ne 1) { throw 'Expected one MSI and two ZIP release assets.' }
$installer = $installers[0].FullName
$assetName = $installers[0].BaseName
if ($assetName -notmatch "\ADotJpegli\.Cli-([0-9]+\.[0-9]+\.[0-9]+)-$RuntimeIdentifier\z") {
    throw 'Unexpected MSI filename or architecture.'
}
$version = $Matches[1]
$extracted = Join-Path $ArtifactRoot 'extracted'
$symbols = Join-Path $ArtifactRoot 'extracted-symbols'
Expand-Archive -LiteralPath (Join-Path $release "$assetName.zip") -DestinationPath $extracted
Expand-Archive -LiteralPath (Join-Path $release "$assetName-symbols.zip") -DestinationPath $symbols
foreach ($name in @('dotjpegli.exe', 'LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md',
        'LICENSE.dotnet.txt', 'THIRD-PARTY-NOTICES.dotnet.txt', 'README.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $extracted $name) -PathType Leaf)) {
        throw "Portable archive is missing $name."
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $symbols 'dotjpegli.pdb') -PathType Leaf)) {
    throw 'Symbols archive is missing the NativeAOT PDB.'
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
    Assert-InstalledPath
    & (Join-Path $PSScriptRoot 'test-cli.ps1') -ExecutablePath $installedExecutable `
        -OutputRoot (Join-Path $ArtifactRoot 'installed-smoke') -ExpectedVersion $version
    $expectedHash = (Get-FileHash -LiteralPath (Join-Path $extracted 'dotjpegli.exe') -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
        throw 'The installed executable differs from the portable executable.'
    }
    # The exact file is under the just-created installation on this disposable host.
    Remove-Item -LiteralPath $installedExecutable
    Invoke-MsiCommand -Operation '/fa' -LogName 'msi-repair.log'
    Assert-InstalledPath
    if ((Get-FileHash -LiteralPath $installedExecutable -Algorithm SHA256).Hash -ne $expectedHash) {
        throw 'MSI repair did not restore the executable.'
    }
} finally {
    Invoke-MsiCommand -Operation '/x' -LogName 'msi-uninstall.log'
}
if (Test-Path -LiteralPath $installedExecutable) { throw 'Uninstall left the CLI executable behind.' }
if ([string] [Environment]::GetEnvironmentVariable('PATH', 'User') -cne $originalUserPath) {
    throw 'Uninstall did not restore the original user PATH.'
}
Write-Output 'CLI distribution checks passed: portable/symbols archives and MSI install, repair, and uninstall.'
