#requires -Version 7.6
<#
.SYNOPSIS
Publishes the NativeAOT CLI executable, portable/symbols archives, and OS packages.
.DESCRIPTION
Builds the trimmed NativeAOT CLI for the requested RuntimeIdentifier using a fresh
output directory. Produces:
- Windows (win-x64, win-arm64): standalone EXE, portable ZIP, symbols ZIP, and per-user MSI
- Linux (linux-x64, linux-arm64): portable tar.gz, symbols tar.gz, DEB, RPM, AppImage, and Flatpak
- macOS (osx-arm64): portable tar.gz, symbols tar.gz, and macOS installer PKG
.PARAMETER RuntimeIdentifier
Native platform and architecture to publish. Use a matching build host.
.PARAMETER OutputRoot
Fresh absolute directory for the publish, staging, tooling, and release outputs.
.PARAMETER Version
Release version. Defaults to the library's committed Version property.
.EXAMPLE
./tools/publish-cli.ps1 -RuntimeIdentifier win-x64 -OutputRoot C:/build/cli
.EXAMPLE
./tools/publish-cli.ps1 -RuntimeIdentifier linux-x64 -OutputRoot /tmp/cli
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-arm64')][string] $RuntimeIdentifier,
    [Parameter(Mandatory)][ValidateNotNullOrWhiteSpace()][string] $OutputRoot,
    [ValidatePattern('\A[0-9]+\.[0-9]+\.[0-9]+\z', Options = 'None')][string] $Version
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if ($RuntimeIdentifier.StartsWith('win-', [StringComparison]::Ordinal) -and -not $IsWindows) {
    throw 'Windows CLI release packaging requires Windows.'
}
if ($RuntimeIdentifier.StartsWith('linux-', [StringComparison]::Ordinal) -and -not $IsLinux) {
    throw 'Linux CLI release packaging requires Linux.'
}
if ($RuntimeIdentifier.StartsWith('osx-', [StringComparison]::Ordinal) -and -not $IsMacOS) {
    throw 'macOS CLI release packaging requires macOS.'
}
if (-not [IO.Path]::IsPathFullyQualified($OutputRoot)) { throw 'OutputRoot must be absolute.' }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
if (Test-Path -LiteralPath $OutputRoot) { throw 'OutputRoot must not already exist.' }
$repository = Split-Path $PSScriptRoot -Parent
if (-not $Version) {
    [xml] $project = Get-Content -LiteralPath (Join-Path $repository 'src/DotJpegli/DotJpegli.csproj') -Raw
    $Version = $project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
}
if ($Version -notmatch '\A[0-9]+\.[0-9]+\.[0-9]+\z') {
    throw 'CLI releases require a stable three-part numeric version.'
}
$parsedVersion = [version] $Version
if ($parsedVersion.Major -gt 255 -or $parsedVersion.Minor -gt 255 -or $parsedVersion.Build -gt 65535) {
    throw 'The release version exceeds installer version limits (255.255.65535).'
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
$portableNotices = @('LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md', 'LICENSE.dotnet.txt', 'THIRD-PARTY-NOTICES.dotnet.txt', 'README.md')

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

foreach ($runtimeNotice in @('LICENSE.dotnet.txt', 'THIRD-PARTY-NOTICES.dotnet.txt')) {
    Copy-Item -LiteralPath (Join-Path $publish $runtimeNotice) -Destination $portable
}
foreach ($notice in @('LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $repository $notice) -Destination $portable
}
Copy-Item -LiteralPath (Join-Path $repository 'docs/cli.md') -Destination (Join-Path $portable 'README.md')

if ($IsWindows) {
    $architecture = $RuntimeIdentifier.Substring(4)
    $upgradeCode = $architecture -eq 'x64' ? '0E6AB2A2-7FF4-41E2-B662-5B2BF41E1ED6' : '2F85042E-D658-40E5-A2A2-7FC0F71616BB'
    $componentCode = $architecture -eq 'x64' ? '662F551A-F61C-4E4A-9BBF-E2CBD10FC33A' : '108D5423-28DB-4CA4-A0F0-611759D214ED'
    $executable = Join-Path $publish 'dotjpegli.exe'
    $pdb = Join-Path $publish 'dotjpegli.pdb'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Native executable is missing.' }
    if (-not (Test-Path -LiteralPath $pdb -PathType Leaf)) { throw 'Native debug symbols are missing.' }
    Copy-Item -LiteralPath $executable -Destination $portable
    Copy-Item -LiteralPath $executable -Destination (Join-Path $release "$assetName.exe")
    Copy-Item -LiteralPath $pdb -Destination $symbols

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
    return
}

$executable = Join-Path $publish 'dotjpegli'
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Native executable is missing.' }
& chmod 0755 $executable
foreach ($notice in $portableNotices) {
    & chmod 0644 (Join-Path $portable $notice)
}

if ($IsMacOS) {
    $dsym = Join-Path $publish 'dotjpegli.dSYM'
    if (-not (Test-Path -LiteralPath (Join-Path $dsym 'Contents/Resources/DWARF/dotjpegli') -PathType Leaf)) {
        throw 'Native macOS dSYM bundle is missing.'
    }
    & codesign --force --sign - $executable
    Copy-Item -LiteralPath $executable -Destination $portable
    Copy-Item -LiteralPath $dsym -Destination (Join-Path $symbols 'dotjpegli.dSYM') -Recurse

    $previousCopyFileDisable = $env:COPYFILE_DISABLE
    $env:COPYFILE_DISABLE = '1'
    try {
        & tar -czf (Join-Path $release "$assetName.tar.gz") -C $portable dotjpegli @portableNotices
        & tar -czf (Join-Path $release "$assetName-symbols.tar.gz") -C $symbols dotjpegli.dSYM
    } finally {
        $env:COPYFILE_DISABLE = $previousCopyFileDisable
    }

    $pkgRoot = Join-Path $OutputRoot 'pkg-root'
    $pkgBin = Join-Path $pkgRoot 'usr/local/bin'
    $pkgDoc = Join-Path $pkgRoot 'usr/local/share/doc/dotjpegli'
    $pkgResources = Join-Path $OutputRoot 'pkg-resources'
    foreach ($directory in @($pkgBin, $pkgDoc, $pkgResources)) {
        $null = [IO.Directory]::CreateDirectory($directory)
    }
    Copy-Item -LiteralPath (Join-Path $portable 'dotjpegli') -Destination (Join-Path $pkgBin 'dotjpegli')
    & chmod 0755 (Join-Path $pkgBin 'dotjpegli')
    foreach ($notice in $portableNotices) {
        Copy-Item -LiteralPath (Join-Path $portable $notice) -Destination (Join-Path $pkgDoc $notice)
        & chmod 0644 (Join-Path $pkgDoc $notice)
    }
    Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination (Join-Path $pkgResources 'LICENSE')
    $distributionXml = Join-Path $OutputRoot 'Distribution.xml'
    $distributionTemplate = Get-Content -LiteralPath (Join-Path $repository 'packaging/macos/Distribution.xml') -Raw
    [IO.File]::WriteAllText($distributionXml, $distributionTemplate.Replace('__VERSION__', $Version))
    $componentPkg = Join-Path $OutputRoot 'dotjpegli-component.pkg'
    & pkgbuild --root $pkgRoot --identifier io.github.symbiogenesis.dotjpegli.cli --version $Version --install-location / $componentPkg
    & productbuild --distribution $distributionXml --resources $pkgResources --package-path $OutputRoot (Join-Path $release "$assetName.pkg")
    Write-Output "CLI release assets: $release"
    return
}

$dbg = Join-Path $publish 'dotjpegli.dbg'
if (-not (Test-Path -LiteralPath $dbg -PathType Leaf)) { throw 'Native ELF debug symbols are missing.' }
& chmod 0644 $dbg
Copy-Item -LiteralPath $executable -Destination $portable
Copy-Item -LiteralPath $dbg -Destination $symbols

& tar -czf (Join-Path $release "$assetName.tar.gz") -C $portable dotjpegli @portableNotices
& tar -czf (Join-Path $release "$assetName-symbols.tar.gz") -C $symbols dotjpegli.dbg

$isArm64 = $RuntimeIdentifier -eq 'linux-arm64'
$debArch = $isArm64 ? 'arm64' : 'amd64'
$rpmArch = $isArm64 ? 'aarch64' : 'x86_64'
$bundleArch = $isArm64 ? 'aarch64' : 'x86_64'

# Build Debian/Ubuntu (.deb) package.
$debRoot = Join-Path $OutputRoot 'deb-root'
$debControlDir = Join-Path $debRoot 'DEBIAN'
$debBinDir = Join-Path $debRoot 'usr/bin'
$debDocDir = Join-Path $debRoot 'usr/share/doc/dotjpegli'
foreach ($directory in @($debControlDir, $debBinDir, $debDocDir)) {
    $null = [IO.Directory]::CreateDirectory($directory)
}
Copy-Item -LiteralPath (Join-Path $portable 'dotjpegli') -Destination (Join-Path $debBinDir 'dotjpegli')
& chmod 0755 (Join-Path $debBinDir 'dotjpegli')
foreach ($notice in $portableNotices) {
    Copy-Item -LiteralPath (Join-Path $portable $notice) -Destination (Join-Path $debDocDir $notice)
    & chmod 0644 (Join-Path $debDocDir $notice)
}
Copy-Item -LiteralPath (Join-Path $portable 'LICENSE') -Destination (Join-Path $debDocDir 'copyright')
& chmod 0644 (Join-Path $debDocDir 'copyright')
$installedBytes = (Get-ChildItem -LiteralPath (Join-Path $debRoot 'usr') -Recurse -File | Measure-Object -Property Length -Sum).Sum
$installedKiB = [int] [Math]::Ceiling($installedBytes / 1024.0)
$debControl = @(
    'Package: dotjpegli'
    "Version: $Version"
    'Section: graphics'
    'Priority: optional'
    "Architecture: $debArch"
    'Maintainer: Edward Miller <symbiogenesis@outlook.com>'
    'Homepage: https://github.com/symbiogenesis/DotJpegli'
    'Depends: libc6, zlib1g'
    "Installed-Size: $installedKiB"
    'Description: Native JPEG encoder based on Google Jpegli'
    ' Self-contained NativeAOT command-line JPEG encoder built from DotJpegli.'
    ' No .NET runtime installation is required.'
    ''
) -join "`n"
[IO.File]::WriteAllText((Join-Path $debControlDir 'control'), $debControl)
& chmod 0644 (Join-Path $debControlDir 'control')
& find $debRoot -type d -exec chmod 0755 '{}' +
& dpkg-deb --root-owner-group --build $debRoot (Join-Path $release "$assetName.deb")

# Build Fedora/RHEL/openSUSE (.rpm) package.
$rpmTopDir = Join-Path $OutputRoot 'rpmbuild'
foreach ($sub in @('BUILD', 'RPMS', 'SOURCES', 'SPECS', 'SRPMS')) {
    $null = [IO.Directory]::CreateDirectory((Join-Path $rpmTopDir $sub))
}
& rpmbuild -bb --target $rpmArch `
    --define "_topdir $rpmTopDir" `
    --define "pkg_version $Version" `
    --define "payload_dir $portable" `
    (Join-Path $repository 'packaging/linux/dotjpegli.spec')
$builtRpms = @(Get-ChildItem -LiteralPath (Join-Path $rpmTopDir 'RPMS') -Filter '*.rpm' -Recurse -File)
if ($builtRpms.Count -ne 1) { throw 'Expected rpmbuild to produce exactly one RPM package.' }
Move-Item -LiteralPath $builtRpms[0].FullName -Destination (Join-Path $release "$assetName.rpm")

# Build portable AppImage (.AppImage) using pinned, hash-verified AppImage Type 2 runtime.
$appId = 'io.github.symbiogenesis.DotJpegli'
$desktopSource = Join-Path $repository "packaging/linux/$appId.desktop"
$iconSource = Join-Path $repository "packaging/linux/$appId.svg"
$appdataSource = Join-Path $repository "packaging/linux/$appId.appdata.xml"
$appDir = Join-Path $OutputRoot 'AppDir'
$appBinDir = Join-Path $appDir 'usr/bin'
$appDocDir = Join-Path $appDir 'usr/share/doc/dotjpegli'
$appDesktopDir = Join-Path $appDir 'usr/share/applications'
$appIconDir = Join-Path $appDir 'usr/share/icons/hicolor/scalable/apps'
$appMetainfoDir = Join-Path $appDir 'usr/share/metainfo'
foreach ($directory in @($appBinDir, $appDocDir, $appDesktopDir, $appIconDir, $appMetainfoDir)) {
    $null = [IO.Directory]::CreateDirectory($directory)
}
Copy-Item -LiteralPath (Join-Path $portable 'dotjpegli') -Destination (Join-Path $appBinDir 'dotjpegli')
& chmod 0755 (Join-Path $appBinDir 'dotjpegli')
foreach ($notice in $portableNotices) {
    Copy-Item -LiteralPath (Join-Path $portable $notice) -Destination (Join-Path $appDocDir $notice)
    & chmod 0644 (Join-Path $appDocDir $notice)
}
Copy-Item -LiteralPath $desktopSource -Destination (Join-Path $appDesktopDir "$appId.desktop")
Copy-Item -LiteralPath $desktopSource -Destination (Join-Path $appDir "$appId.desktop")
Copy-Item -LiteralPath $iconSource -Destination (Join-Path $appIconDir "$appId.svg")
Copy-Item -LiteralPath $iconSource -Destination (Join-Path $appDir "$appId.svg")
Copy-Item -LiteralPath $appdataSource -Destination (Join-Path $appMetainfoDir "$appId.appdata.xml")
& ln -s usr/bin/dotjpegli (Join-Path $appDir 'AppRun')
& ln -s "$appId.svg" (Join-Path $appDir '.DirIcon')
& find $appDir -type d -exec chmod 0755 '{}' +

$runtimeUrl = "https://github.com/AppImage/type2-runtime/releases/download/20251108/runtime-$bundleArch"
$expectedRuntimeHash = $isArm64 ?
    '00CBDFCF917CC6C0FF6D3347D59E0CA1F7F45A6DF1A428A0D6D8A78664D87444' :
    '2FCA8B443C92510F1483A883F60061AD09B46B978B2631C807CD873A47EC260D'
$runtimePath = Join-Path $toolDirectory "runtime-$bundleArch"
Invoke-WebRequest -Uri $runtimeUrl -OutFile $runtimePath
if ((Get-FileHash -LiteralPath $runtimePath -Algorithm SHA256).Hash -cne $expectedRuntimeHash) {
    throw 'Downloaded AppImage Type 2 runtime did not match the pinned SHA-256 hash.'
}
$squashfsPath = Join-Path $OutputRoot 'appimage.squashfs'
& mksquashfs $appDir $squashfsPath -root-owned -noappend -quiet
$appImagePath = Join-Path $release "$assetName.AppImage"
$appImageStream = [IO.File]::Create($appImagePath)
try {
    $runtimeBytes = [IO.File]::ReadAllBytes($runtimePath)
    $squashfsBytes = [IO.File]::ReadAllBytes($squashfsPath)
    $appImageStream.Write($runtimeBytes, 0, $runtimeBytes.Length)
    $appImageStream.Write($squashfsBytes, 0, $squashfsBytes.Length)
} finally {
    $appImageStream.Dispose()
}
& chmod 0755 $appImagePath

# Build single-file Flatpak bundle (.flatpak).
$flatpakBuildDir = Join-Path $OutputRoot 'flatpak-build'
$flatpakBinDir = Join-Path $flatpakBuildDir 'files/bin'
$flatpakDocDir = Join-Path $flatpakBuildDir 'files/share/doc/dotjpegli'
$flatpakDesktopDir = Join-Path $flatpakBuildDir 'files/share/applications'
$flatpakIconDir = Join-Path $flatpakBuildDir 'files/share/icons/hicolor/scalable/apps'
$flatpakMetainfoDir = Join-Path $flatpakBuildDir 'files/share/metainfo'
$flatpakVarTmp = Join-Path $flatpakBuildDir 'var/tmp'
foreach ($directory in @($flatpakBinDir, $flatpakDocDir, $flatpakDesktopDir, $flatpakIconDir, $flatpakMetainfoDir, $flatpakVarTmp)) {
    $null = [IO.Directory]::CreateDirectory($directory)
}
& ln -s /run (Join-Path $flatpakBuildDir 'var/run')
$flatpakMetadata = @(
    '[Application]'
    "name=$appId"
    "runtime=org.freedesktop.Platform/$bundleArch/24.08"
    "sdk=org.freedesktop.Sdk/$bundleArch/24.08"
    ''
) -join "`n"
[IO.File]::WriteAllText((Join-Path $flatpakBuildDir 'metadata'), $flatpakMetadata)
Copy-Item -LiteralPath (Join-Path $portable 'dotjpegli') -Destination (Join-Path $flatpakBinDir 'dotjpegli')
& chmod 0755 (Join-Path $flatpakBinDir 'dotjpegli')
foreach ($notice in $portableNotices) {
    Copy-Item -LiteralPath (Join-Path $portable $notice) -Destination (Join-Path $flatpakDocDir $notice)
    & chmod 0644 (Join-Path $flatpakDocDir $notice)
}
Copy-Item -LiteralPath $desktopSource -Destination (Join-Path $flatpakDesktopDir "$appId.desktop")
Copy-Item -LiteralPath $iconSource -Destination (Join-Path $flatpakIconDir "$appId.svg")
Copy-Item -LiteralPath $appdataSource -Destination (Join-Path $flatpakMetainfoDir "$appId.appdata.xml")

$flatpakRepo = Join-Path $OutputRoot 'flatpak-repo'
& flatpak build-finish $flatpakBuildDir --command=dotjpegli --filesystem=host --no-inherit-permissions
& ostree "--repo=$flatpakRepo" init --mode=archive-z2
& flatpak build-export "--arch=$bundleArch" --disable-sandbox $flatpakRepo $flatpakBuildDir stable
& flatpak build-bundle "--arch=$bundleArch" --runtime-repo=https://flathub.org/repo/flathub.flatpakrepo `
    $flatpakRepo (Join-Path $release "$assetName.flatpak") $appId stable

Write-Output "CLI release assets: $release"
