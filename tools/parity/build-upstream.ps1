[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $SourceDirectory,
    [Parameter(Mandatory)][string] $BuildDirectory,
    [string] $Generator,
    [ValidateRange(1, 64)][int] $Parallelism = 2
)

$ErrorActionPreference = 'Stop'
$pin = '031a0077f5799a6041004267fc12b956c1f52a20'
$repository = 'https://github.com/google/jpegli.git'
$dependencies = @('third_party/highway', 'third_party/skcms', 'third_party/libjpeg-turbo')

foreach ($value in @($SourceDirectory, $BuildDirectory)) {
    if (-not [System.IO.Path]::IsPathFullyQualified($value) -or $value -match '%[^%]+%') {
        throw 'SourceDirectory and BuildDirectory must be resolved absolute paths.'
    }
}
$SourceDirectory = [System.IO.Path]::GetFullPath($SourceDirectory)
$BuildDirectory = [System.IO.Path]::GetFullPath($BuildDirectory)
if ($BuildDirectory.StartsWith($SourceDirectory + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase) -or $BuildDirectory -eq $SourceDirectory) {
    throw 'BuildDirectory must be outside the reference source checkout.'
}

$git = (Get-Command git -ErrorAction Stop).Source
$cmake = (Get-Command cmake -ErrorAction Stop).Source
function Invoke-Git([string[]] $Arguments) {
    & $git @Arguments
    if ($LASTEXITCODE -ne 0) { throw "git failed with exit code $LASTEXITCODE" }
}

New-Item -ItemType Directory -Path $BuildDirectory -Force | Out-Null
$oldProcessDirectory = [Environment]::CurrentDirectory
Push-Location -LiteralPath $BuildDirectory
try {
    [Environment]::CurrentDirectory = $BuildDirectory
    if (-not (Test-Path -LiteralPath $SourceDirectory)) {
        Invoke-Git @('clone', '--no-checkout', '--filter=blob:none', $repository, $SourceDirectory)
        Invoke-Git @('-C', $SourceDirectory, 'fetch', '--depth', '1', 'origin', $pin)
        Invoke-Git @('-C', $SourceDirectory, 'checkout', '--detach', $pin)
    }
    $actualPin = (& $git -C $SourceDirectory rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $actualPin -ne $pin) {
        throw "Reference must already be checked out at $pin; got $actualPin."
    }
    Invoke-Git @('-C', $SourceDirectory, 'diff', '--quiet', '--ignore-submodules=none', 'HEAD', '--')
    Invoke-Git (@('-C', $SourceDirectory, 'submodule', 'update', '--init', '--depth', '1', '--') + $dependencies)
    $dependencyPins = [ordered]@{}
    foreach ($dependency in $dependencies) {
        $expected = ((& $git -C $SourceDirectory ls-tree HEAD -- $dependency) -split '\s+')[2]
        if ($LASTEXITCODE -ne 0 -or $expected -notmatch '^[0-9a-f]{40}$') { throw "Missing pin for $dependency" }
        $dependencyPath = Join-Path $SourceDirectory $dependency
        $actual = (& $git -C $dependencyPath rev-parse HEAD).Trim()
        if ($LASTEXITCODE -ne 0 -or $actual -ne $expected) { throw "Unexpected dependency revision: $dependency" }
        Invoke-Git @('-C', $dependencyPath, 'diff', '--quiet', '--ignore-submodules=none', 'HEAD', '--')
        $dependencyPins[$dependency] = $actual
    }
    $configure = @('-S', (Join-Path $PSScriptRoot 'native'), '-B', $BuildDirectory,
        '-DCMAKE_BUILD_TYPE=Release', "-DJPEGLI_SOURCE_DIR=$SourceDirectory")
    if ($Generator) { $configure += @('-G', $Generator) }
    & $cmake @configure
    if ($LASTEXITCODE -ne 0) { throw "CMake configuration failed with exit code $LASTEXITCODE" }
    & $cmake --build $BuildDirectory --config Release --target jpegli-parity-native --parallel $Parallelism
    if ($LASTEXITCODE -ne 0) { throw "Native reference build failed with exit code $LASTEXITCODE" }
    Invoke-Git @('-C', $SourceDirectory, 'diff', '--quiet', '--ignore-submodules=none', 'HEAD', '--')

    $executableName = if ($IsWindows) { 'jpegli-parity-native.exe' } else { 'jpegli-parity-native' }
    $executable = Join-Path $BuildDirectory $executableName
    if (-not (Test-Path -LiteralPath $executable)) { $executable = Join-Path $BuildDirectory "Release/$executableName" }
    if (-not (Test-Path -LiteralPath $executable)) { throw 'Native reference executable is missing.' }
    $reportedPin = (& $executable version).Trim()
    if ($LASTEXITCODE -ne 0 -or $reportedPin -ne $pin) { throw 'Built reference reported an unexpected revision.' }
    $compilerCache = Get-Content -LiteralPath (Join-Path $BuildDirectory 'CMakeCache.txt') |
        Where-Object { $_ -match '^CMAKE_(CXX_COMPILER|BUILD_TYPE|GENERATOR):' }
    $compilerDescription = Get-ChildItem -Path (Join-Path $BuildDirectory 'CMakeFiles/*/CMakeCXXCompiler.cmake') |
        ForEach-Object { Get-Content -LiteralPath $_.FullName } |
        Where-Object { $_ -match '^set\(CMAKE_CXX_(COMPILER|COMPILER_ID|COMPILER_VERSION|COMPILER_FRONTEND_VARIANT|SIZEOF_DATA_PTR|COMPILER_ARCHITECTURE_ID) ' }
    [ordered]@{
        repository = $repository
        revision = $pin
        dependencies = $dependencyPins
        sourceUnmodified = $true
        cmake = (& $cmake --version | Select-Object -First 1)
        configuration = $compilerCache
        compiler = $compilerDescription
        executable = $executable
        executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash.ToLowerInvariant()
        encode = 'RGB uint8 -> YCbCr; force_baseline=true; explicit 444/420, distance, progressive level, optimized Huffman and AQ; no smoothing/restarts; JFIF 1.01 1x1 aspect'
        decode = 'same pinned jpegli decoder for both streams; full-size RGB uint8; JDCT_ISLOW; fancy upsampling; no ICC transform'
        benchmark = 'same encode settings with AQ and optimized Huffman; fresh jpegli_mem_dest lifecycle through destroy; sum per-encode steady_clock intervals; process/file IO, warmups, verification and returned-buffer free excluded'
    } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $BuildDirectory 'provenance.json') -Encoding utf8
    Write-Output "Native parity reference: $executable"
} finally {
    [Environment]::CurrentDirectory = $oldProcessDirectory
    Pop-Location
}
