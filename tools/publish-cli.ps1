#requires -Version 7.6
<#
.SYNOPSIS
Publishes the NativeAOT EXE, portable ZIP, symbols ZIP, and Windows MSI.
.DESCRIPTION
Uses a fresh output directory and a pinned, isolated WiX tool installation.
The release directory contains only the four files intended for distribution.
.PARAMETER RuntimeIdentifier
Native Windows architecture to publish. Use a matching Windows build host.
.PARAMETER OutputRoot
Fresh absolute directory for the publish, staging, tooling, and release outputs.
.PARAMETER Version
Release version. Defaults to the library's committed Version property.
.EXAMPLE
./tools/publish-cli.ps1 -RuntimeIdentifier win-x64 -OutputRoot C:/build/cli
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('win-x64', 'win-arm64')][string] $RuntimeIdentifier,
    [Parameter(Mandatory)][ValidateNotNullOrWhiteSpace()][string] $OutputRoot,
    [ValidatePattern('\A[0-9]+\.[0-9]+\.[0-9]+\z', Options = 'None')][string] $Version
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'Windows CLI release packaging requires Windows.' }
if (-not [IO.Path]::IsPathFullyQualified($OutputRoot)) { throw 'OutputRoot must be absolute.' }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
if (Test-Path -LiteralPath $OutputRoot) { throw 'OutputRoot must not already exist.' }
$repository = Split-Path $PSScriptRoot -Parent
if (-not $Version) {
    [xml] $project = Get-Content -LiteralPath (Join-Path $repository 'src/DotJpegli/DotJpegli.csproj') -Raw
    $Version = $project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
}
if ($Version -notmatch '\A[0-9]+\.[0-9]+\.[0-9]+\z') {
    throw 'Windows MSI releases require a stable three-part numeric version.'
}
$msiVersion = [version] $Version
if ($msiVersion.Major -gt 255 -or $msiVersion.Minor -gt 255 -or $msiVersion.Build -gt 65535) {
    throw 'The release version exceeds Windows Installer version limits (255.255.65535).'
}
$architecture = $RuntimeIdentifier.Substring(4)
$upgradeCode = if ($architecture -eq 'x64') {
    '0E6AB2A2-7FF4-41E2-B662-5B2BF41E1ED6'
} else {
    '2F85042E-D658-40E5-A2A2-7FC0F71616BB'
}
$componentCode = if ($architecture -eq 'x64') {
    '662F551A-F61C-4E4A-9BBF-E2CBD10FC33A'
} else {
    '108D5423-28DB-4CA4-A0F0-611759D214ED'
}
$publish = Join-Path $OutputRoot 'publish'
$portable = Join-Path $OutputRoot 'portable'
$symbols = Join-Path $OutputRoot 'symbols'
$release = Join-Path $OutputRoot 'release'
$toolDirectory = Join-Path $OutputRoot 'tools'
foreach ($directory in @($publish, $portable, $symbols, $release, $toolDirectory)) {
    $null = [IO.Directory]::CreateDirectory($directory)
}
$assetName = "DotJpegli.Cli-$Version-$RuntimeIdentifier"

Push-Location -LiteralPath $repository
try {
    # RID-specific SDK/compiler packs intentionally restore at publish time.
    dotnet publish src/DotJpegli.Cli/DotJpegli.Cli.csproj -c Release -r $RuntimeIdentifier `
        --self-contained true -p:PublishAot=true -p:PublishTrimmed=true `
        -p:Version=$Version -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main `
        -p:DebugSymbols=true -o $publish
} finally {
    Pop-Location
}
$executable = Join-Path $publish 'dotjpegli.exe'
$pdb = Join-Path $publish 'dotjpegli.pdb'
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Native executable is missing.' }
if (-not (Test-Path -LiteralPath $pdb -PathType Leaf)) { throw 'Native debug symbols are missing.' }
Copy-Item -LiteralPath $executable -Destination $portable
Copy-Item -LiteralPath $executable -Destination (Join-Path $release "$assetName.exe")
Copy-Item -LiteralPath $pdb -Destination $symbols
foreach ($runtimeNotice in @('LICENSE.dotnet.txt', 'THIRD-PARTY-NOTICES.dotnet.txt')) {
    Copy-Item -LiteralPath (Join-Path $publish $runtimeNotice) -Destination $portable
}
foreach ($notice in @('LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $repository $notice) -Destination $portable
}
Copy-Item -LiteralPath (Join-Path $repository 'docs/cli.md') -Destination (Join-Path $portable 'README.md')

# WiX 5 predates the maintenance-fee/EULA changes in WiX 6 and later.
# Allow the pinned net6 tool to run on the installed, supported .NET runtime.
dotnet tool install wix --version 5.0.2 --tool-path $toolDirectory --allow-roll-forward
$wix = Join-Path $toolDirectory 'wix.exe'
$licenseRtf = Join-Path $OutputRoot 'LICENSE.rtf'
$license = (Get-Content -LiteralPath (Join-Path $repository 'LICENSE') -Raw).
    Replace('\', '\\').Replace('{', '\{').Replace('}', '\}').Replace("`r", '').Replace("`n", '\par ')
[IO.File]::WriteAllText($licenseRtf, '{\rtf1\ansi\deff0 {\fonttbl {\f0 Consolas;}}\f0\fs18 ' + $license + '}')
Push-Location -LiteralPath $OutputRoot
try {
    # The extension cache stays in this fresh output root, outside the checkout/profile.
    & $wix extension add WixToolset.UI.wixext/5.0.2
    & $wix build (Join-Path $repository 'packaging/windows/Package.wxs') `
        -arch $architecture -ext WixToolset.UI.wixext -wx `
        -d "Version=$Version" -d "Architecture=$architecture" -d "UpgradeCode=$upgradeCode" -d "ComponentCode=$componentCode" `
        -d "PayloadDirectory=$portable" -d "LicenseRtf=$licenseRtf" `
        -intermediateFolder (Join-Path $OutputRoot 'wix-obj') -o (Join-Path $release "$assetName.msi")
} finally {
    Pop-Location
}
# Keep the WiX debugging database with the CLI's native symbols.
$wixSymbols = Join-Path $release "$assetName.wixpdb"
if (Test-Path -LiteralPath $wixSymbols) {
    Move-Item -LiteralPath $wixSymbols -Destination $symbols
}
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $portable -File | Select-Object -ExpandProperty FullName) `
    -DestinationPath (Join-Path $release "$assetName.zip") -CompressionLevel Optimal
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $symbols -File | Select-Object -ExpandProperty FullName) `
    -DestinationPath (Join-Path $release "$assetName-symbols.zip") -CompressionLevel Optimal
Write-Output "CLI release assets: $release"
