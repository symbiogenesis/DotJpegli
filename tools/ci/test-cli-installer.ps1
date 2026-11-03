#requires -Version 7.6
<#
.SYNOPSIS
Checks EXE/portable release assets and MSI install, repair, and removal on disposable CI hosts.
.PARAMETER ArtifactRoot
OutputRoot produced by tools/publish-cli.ps1.
.PARAMETER RuntimeIdentifier
Native architecture of the artifacts being checked.
.PARAMETER SelfTest
Checks PATH restoration comparisons using synthetic values without installing anything.
.EXAMPLE
./tools/ci/test-cli-installer.ps1 -ArtifactRoot C:/build/cli -RuntimeIdentifier win-x64
#>
[CmdletBinding(DefaultParameterSetName = 'Installer')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Installer')][ValidateNotNullOrWhiteSpace()][string] $ArtifactRoot,
    [Parameter(Mandatory, ParameterSetName = 'Installer')][ValidateSet('win-x64', 'win-arm64')][string] $RuntimeIdentifier,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

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
    Write-Output 'Installer PATH comparison self-test passed.'
    return
}

if (-not $IsWindows -or $env:GITHUB_ACTIONS -ne 'true') {
    throw 'Installer smoke checks require a disposable Windows GitHub Actions runner.'
}
$ArtifactRoot = (Resolve-Path -LiteralPath $ArtifactRoot).ProviderPath
$release = Join-Path $ArtifactRoot 'release'
$assets = @(Get-ChildItem -LiteralPath $release -File)
$installers = @($assets | Where-Object Extension -EQ '.msi')
if ($assets.Count -ne 4 -or $installers.Count -ne 1) { throw 'Expected one EXE, one MSI and two ZIP release assets.' }
$installer = $installers[0].FullName
$assetName = $installers[0].BaseName
if ($assetName -notmatch "\ADotJpegli\.Cli-([0-9]+\.[0-9]+\.[0-9]+)-$RuntimeIdentifier\z") {
    throw 'Unexpected MSI filename or architecture.'
}
$version = $Matches[1]
$expectedNames = @("$assetName.exe", "$assetName.msi", "$assetName.zip", "$assetName-symbols.zip")
if (@(Compare-Object -ReferenceObject $expectedNames -DifferenceObject @($assets.Name) -CaseSensitive).Count -ne 0) {
    throw 'CLI release files do not match the expected version, architecture and complete artifact set.'
}
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
