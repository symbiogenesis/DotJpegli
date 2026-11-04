#requires -Version 7.6
<#
.SYNOPSIS
Builds or verifies the pinned, unmodified native jpegli reference.
.PARAMETER SourceDirectory
Absolute path to a new checkout or an existing checkout at the pinned revision.
.PARAMETER BuildDirectory
Absolute build directory outside the reference source checkout.
.PARAMETER Generator
Optional CMake generator, such as Ninja.
.PARAMETER Parallelism
Maximum number of parallel native build jobs.
.PARAMETER VerifyOnly
Checks a cached build's source provenance and executable hash without rebuilding.
.EXAMPLE
./tools/parity/build-upstream.ps1 -SourceDirectory /tmp/jpegli -BuildDirectory /tmp/jpegli-build -Generator Ninja
#>
[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Build')][ValidateNotNullOrWhiteSpace()][string] $SourceDirectory,
    [Parameter(Mandatory, ParameterSetName = 'Build')]
    [Parameter(Mandatory, ParameterSetName = 'Verify')][ValidateNotNullOrWhiteSpace()][string] $BuildDirectory,
    [Parameter(ParameterSetName = 'Build')][ValidateNotNullOrWhiteSpace()][string] $Generator,
    [Parameter(ParameterSetName = 'Build')][ValidateRange(1, 64)][int] $Parallelism = 2,
    [Parameter(Mandatory, ParameterSetName = 'Verify')][switch] $VerifyOnly
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$pin = '031a0077f5799a6041004267fc12b956c1f52a20'
$repository = 'https://github.com/google/jpegli.git'
$dependencies = @('third_party/highway', 'third_party/skcms', 'third_party/libjpeg-turbo')

function Get-NativeExecutable([string] $Directory) {
    $name = $IsWindows ? 'jpegli-parity-native.exe' : 'jpegli-parity-native'
    $path = Join-Path $Directory $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $path = Join-Path $Directory "Release/$name" }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw 'Native reference executable is missing.' }
    return $path
}

if ($VerifyOnly) {
    & (Join-Path $PSScriptRoot '../Invoke-Automation.ps1') -CommandArguments @('native-reference', '--build-directory', $BuildDirectory)
    return
}

foreach ($value in @($SourceDirectory, $BuildDirectory)) {
    if (-not [System.IO.Path]::IsPathFullyQualified($value) -or $value -match '%[^%]+%') {
        throw 'SourceDirectory and BuildDirectory must be resolved absolute paths.'
    }
}
$SourceDirectory = [System.IO.Path]::GetFullPath($SourceDirectory)
$BuildDirectory = [System.IO.Path]::GetFullPath($BuildDirectory)
$sourceRoot = [System.IO.Path]::TrimEndingDirectorySeparator($SourceDirectory)
$buildRoot = [System.IO.Path]::TrimEndingDirectorySeparator($BuildDirectory)
$pathComparison = $IsWindows ? [System.StringComparison]::OrdinalIgnoreCase : [System.StringComparison]::Ordinal
$sourcePrefix = [System.IO.Path]::EndsInDirectorySeparator($sourceRoot) ? $sourceRoot :
    $sourceRoot + [System.IO.Path]::DirectorySeparatorChar
if ($buildRoot.StartsWith($sourcePrefix, $pathComparison) -or $buildRoot.Equals($sourceRoot, $pathComparison)) {
    throw 'BuildDirectory must be outside the reference source checkout.'
}

$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$cmake = (Get-Command cmake -CommandType Application | Select-Object -First 1).Source

[System.IO.Directory]::CreateDirectory($BuildDirectory) | Out-Null
Push-Location -LiteralPath $BuildDirectory
try {
    if (-not (Test-Path -LiteralPath $SourceDirectory)) {
        & $git clone --no-checkout --filter=blob:none $repository $SourceDirectory
        & $git -C $SourceDirectory fetch --depth 1 origin $pin
        & $git -C $SourceDirectory checkout --detach $pin
    }
    $actualPin = (& $git -C $SourceDirectory rev-parse HEAD).Trim()
    if ($actualPin -cne $pin) {
        throw "Reference must already be checked out at $pin; got $actualPin."
    }
    & $git -C $SourceDirectory diff --quiet --ignore-submodules=none HEAD --
    & $git -C $SourceDirectory submodule update --init --depth 1 -- @dependencies
    $dependencyPins = [ordered]@{}
    foreach ($dependency in $dependencies) {
        $expected = ((& $git -C $SourceDirectory ls-tree HEAD -- $dependency) -split '\s+')[2]
        if ($expected -cnotmatch '^[0-9a-f]{40}$') { throw "Missing pin for $dependency" }
        $dependencyPath = Join-Path $SourceDirectory $dependency
        $actual = (& $git -C $dependencyPath rev-parse HEAD).Trim()
        if ($actual -cne $expected) { throw "Unexpected dependency revision: $dependency" }
        & $git -C $dependencyPath diff --quiet --ignore-submodules=none HEAD --
        $dependencyPins[$dependency] = $actual
    }
    $configure = @('-S', (Join-Path $PSScriptRoot 'native'), '-B', $BuildDirectory,
        '-DCMAKE_BUILD_TYPE=Release', "-DJPEGLI_SOURCE_DIR=$SourceDirectory")
    if ($Generator) { $configure += @('-G', $Generator) }
    & $cmake @configure
    & $cmake --build $BuildDirectory --config Release --target jpegli-parity-native --parallel $Parallelism
    $allocationProbe = Join-Path $BuildDirectory 'jpegli-allocation-native'
    if ($IsLinux) {
        & $cmake --build $BuildDirectory --config Release --target jpegli-allocation-native --parallel $Parallelism
        & $allocationProbe allocation-self-test
    }
    & $git -C $SourceDirectory diff --quiet --ignore-submodules=none HEAD --

    $executable = Get-NativeExecutable $BuildDirectory
    $reportedPin = (& $executable version).Trim()
    if ($reportedPin -cne $pin) { throw 'Built reference reported an unexpected revision.' }
    $compilerCache = Get-Content -LiteralPath (Join-Path $BuildDirectory 'CMakeCache.txt') |
        Where-Object { $_ -match '^CMAKE_(CXX_COMPILER|BUILD_TYPE|GENERATOR):' }
    $compilerDescription = Get-ChildItem -LiteralPath (Join-Path $BuildDirectory 'CMakeFiles') -Directory |
        ForEach-Object { Get-ChildItem -LiteralPath $_.FullName -Filter 'CMakeCXXCompiler.cmake' -File } |
        ForEach-Object { Get-Content -LiteralPath $_.FullName } |
        Where-Object { $_ -match '^set\(CMAKE_CXX_(COMPILER|COMPILER_ID|COMPILER_VERSION|COMPILER_FRONTEND_VARIANT|SIZEOF_DATA_PTR|COMPILER_ARCHITECTURE_ID) ' }
    $cmakeVersion = @(& $cmake --version)
    [ordered]@{
        repository = $repository
        revision = $pin
        dependencies = $dependencyPins
        sourceUnmodified = $true
        cmake = $cmakeVersion[0]
        configuration = $compilerCache
        compiler = $compilerDescription
        executable = $executable
        executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash.ToLowerInvariant()
        allocationProbeSha256 = $IsLinux ? (Get-FileHash -LiteralPath $allocationProbe -Algorithm SHA256).Hash.ToLowerInvariant() : $null
        allocationProbe = $IsLinux ? 'Separate untimed GNU linker-wrap malloc/calloc/realloc/aligned_alloc/posix_memalign/memalign plus C++ new; successful requested bytes including full realloc requests; single encode thread' : $null
        encode = 'RGB uint8 -> YCbCr; force_baseline=true; explicit 444/420, distance, progressive level, optimized Huffman and AQ; no smoothing/restarts; JFIF 1.01 1x1 aspect'
        decode = 'same pinned jpegli decoder for both streams; full-size RGB uint8; JDCT_ISLOW; fancy upsampling; no ICC transform'
        benchmark = 'same encode settings with AQ and optimized Huffman; fresh jpegli_mem_dest lifecycle through destroy; sum per-encode steady_clock intervals; process/file IO, warmups, verification and returned-buffer free excluded'
    } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $BuildDirectory 'provenance.json') -Encoding utf8NoBOM
    Write-Output "Native parity reference: $executable"
} finally {
    Pop-Location
}
