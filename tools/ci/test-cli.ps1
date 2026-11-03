#requires -Version 7.6
<#
.SYNOPSIS
Checks the published native CLI using small synthetic images and invalid inputs.
.PARAMETER ExecutablePath
Path to the published or installed dotjpegli.exe.
.PARAMETER OutputRoot
Fresh directory for fixtures and encoded outputs.
.PARAMETER ExpectedVersion
Optional version expected in --version output.
.EXAMPLE
./tools/ci/test-cli.ps1 -ExecutablePath C:/build/portable/dotjpegli.exe
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrWhiteSpace()][string] $ExecutablePath,
    [ValidateNotNullOrWhiteSpace()][string] $OutputRoot =
        (Join-Path ([IO.Path]::GetTempPath()) ('DotJpegli-cli-smoke-' + [guid]::NewGuid().ToString('N'))),
    [string] $ExpectedVersion
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$ExecutablePath = (Resolve-Path -LiteralPath $ExecutablePath).ProviderPath
if (Test-Path -LiteralPath $OutputRoot) { throw 'OutputRoot must not already exist.' }
$null = [IO.Directory]::CreateDirectory($OutputRoot)

function Invoke-CliCommand([string[]] $CommandArguments, [switch] $ExpectFailure) {
    $start = [Diagnostics.ProcessStartInfo]::new($ExecutablePath)
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $CommandArguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    try {
        $standardOutput = $process.StandardOutput.ReadToEndAsync()
        $standardError = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $message = $standardOutput.GetAwaiter().GetResult() + $standardError.GetAwaiter().GetResult()
        if (($process.ExitCode -eq 0) -eq $ExpectFailure.IsPresent) {
            throw "Unexpected CLI exit $($process.ExitCode): $($CommandArguments -join ' ')`n$message"
        }
        return $message
    } finally {
        $process.Dispose()
    }
}

function Assert-JpegFrame([string] $Path, [byte] $FrameMarker) {
    $jpeg = [IO.File]::ReadAllBytes($Path)
    if ($jpeg.Length -lt 4 -or $jpeg[0] -ne 255 -or $jpeg[1] -ne 216 -or
        $jpeg[-2] -ne 255 -or $jpeg[-1] -ne 217) { throw 'Output is not a complete JPEG.' }
    $offset = 2
    while ($offset + 3 -lt $jpeg.Length) {
        if ($jpeg[$offset] -ne 255) { throw 'Invalid JPEG marker prefix.' }
        $marker = $jpeg[$offset + 1]
        if ($marker -eq 218) { break }
        $length = ([int] $jpeg[$offset + 2] * 256) + $jpeg[$offset + 3]
        if ($length -lt 2 -or $offset + 2 + $length -gt $jpeg.Length) { throw 'Invalid JPEG segment length.' }
        if ($marker -in 192, 194) {
            if ($marker -ne $FrameMarker -or $jpeg[$offset + 5] -ne 0 -or $jpeg[$offset + 6] -ne 3 -or
                $jpeg[$offset + 7] -ne 0 -or $jpeg[$offset + 8] -ne 5) {
                throw 'Unexpected JPEG frame mode or dimensions.'
            }
            return
        }
        $offset += 2 + $length
    }
    throw 'Output has no JPEG frame.'
}

# A native PE must have no CLR runtime header; this rejects a managed apphost assembly.
$binary = [IO.File]::ReadAllBytes($ExecutablePath)
$peOffset = [BitConverter]::ToInt32($binary, 0x3c)
if ($binary[0] -ne 77 -or $binary[1] -ne 90 -or
    [BitConverter]::ToUInt32($binary, $peOffset) -ne 0x4550 -or
    [BitConverter]::ToUInt16($binary, $peOffset + 24) -ne 0x20b -or
    [BitConverter]::ToUInt32($binary, $peOffset + 24 + 112 + (14 * 8)) -ne 0) {
    throw 'The release executable must be a native 64-bit PE with no CLR header.'
}
foreach ($managedFile in @('dotjpegli.dll', 'dotjpegli.deps.json', 'dotjpegli.runtimeconfig.json')) {
    if (Test-Path -LiteralPath (Join-Path (Split-Path $ExecutablePath -Parent) $managedFile)) {
        throw "The native distribution unexpectedly contains $managedFile."
    }
}
$null = Invoke-CliCommand -CommandArguments @('--help')
$versionText = Invoke-CliCommand -CommandArguments @('--version')
if ($ExpectedVersion -and -not $versionText.Contains($ExpectedVersion, [StringComparison]::Ordinal)) {
    throw "CLI version does not contain $ExpectedVersion."
}
$inputPath = Join-Path $OutputRoot 'tiny image.ppm'
$header = [Text.Encoding]::ASCII.GetBytes("P6`n5 3`n255`n")
[byte[]] $pixels = 0..44 | ForEach-Object { [byte] (($_ * 37) % 256) }
[IO.File]::WriteAllBytes($inputPath, [byte[]] ($header + $pixels))
$baseline = Join-Path $OutputRoot 'baseline.jpg'
$progressive = Join-Path $OutputRoot 'progressive.jpg'
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $baseline, '--quality', '85', '--subsampling', '420')
Assert-JpegFrame -Path $baseline -FrameMarker 192
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $progressive, '--distance', '1', '--progressive', '--threads', '2')
Assert-JpegFrame -Path $progressive -FrameMarker 194
$baselineHash = (Get-FileHash -LiteralPath $baseline -Algorithm SHA256).Hash
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $baseline) -ExpectFailure
if ((Get-FileHash -LiteralPath $baseline -Algorithm SHA256).Hash -ne $baselineHash) {
    throw 'The CLI changed an existing destination without --overwrite.'
}
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $baseline, '--overwrite')
Assert-JpegFrame -Path $baseline -FrameMarker 192
$inputHash = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $inputPath, '--overwrite') -ExpectFailure
if ((Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash -ne $inputHash) {
    throw 'The CLI changed its source image.'
}
$invalidOutput = Join-Path $OutputRoot 'invalid.jpg'
$null = Invoke-CliCommand -CommandArguments @('encode', $inputPath, $invalidOutput, '--quality', '101') -ExpectFailure
$null = Invoke-CliCommand -CommandArguments @('encode', (Join-Path $OutputRoot 'missing.ppm'), $invalidOutput) -ExpectFailure
$invalidInput = Join-Path $OutputRoot 'invalid.txt'
[IO.File]::WriteAllText($invalidInput, 'This is not an image.')
$null = Invoke-CliCommand -CommandArguments @('encode', $invalidInput, $invalidOutput) -ExpectFailure
if (Test-Path -LiteralPath $invalidOutput) { throw 'An invalid invocation left an output file.' }
Write-Output 'Native CLI smoke passed: help/version, baseline/progressive, overwrite safety, and invalid inputs.'
