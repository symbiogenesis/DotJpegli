#requires -Version 7.6
<#
.SYNOPSIS
Validates matching runtime evidence and writes the runtime comparison report.
.PARAMETER Directory
Directory containing the four runtime evidence directories.
.PARAMETER Architecture
Architecture label to include in the report; defaults to RUNNER_ARCH.
.PARAMETER Summary
Optional GitHub job summary path; defaults to GITHUB_STEP_SUMMARY.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $Directory,
    [string] $Architecture = $env:RUNNER_ARCH,
    [string] $Summary = $env:GITHUB_STEP_SUMMARY
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$versions = @('10.0.14', '11.0.0-rc.3.26522.148', 'net11-baseline', 'net11-simd')
$data = foreach ($version in $versions) {
    $path = Join-Path $Directory "runtime-$version"
    $run = Get-Content -LiteralPath (Join-Path $path 'run.json') -Raw | ConvertFrom-Json
    $summaryData = Get-Content -LiteralPath (Join-Path $path 'summary.json') -Raw | ConvertFrom-Json
    $cases = Get-Content -LiteralPath (Join-Path $path 'cases.json') -Raw | ConvertFrom-Json -NoEnumerate
    if ($summaryData.valid -isnot [bool] -or -not $summaryData.valid -or $cases -isnot [array] -or $cases.Count -ne 1) { throw 'Runtime comparison needs stable matched evidence.' }
    $expected = if ($version -eq '10.0.14') { '10.0.14' } else { '11.0.0-rc.3.26522.148' }
    if ($run.environment.runtimeVersion -cne $expected.Split('-')[0]) { throw 'Unexpected runtime version.' }
    $product = $run.environment.runtimeProductVersion
    if ($product -cne $expected -and -not ($expected -eq '10.0.14' -and $product.StartsWith("$expected-servicing.", [StringComparison]::Ordinal))) {
        throw 'Unexpected runtime build.'
    }
    if ($run.environment.explicitNet11Simd -isnot [bool] -or $run.environment.explicitNet11Simd -ne ($version -eq 'net11-simd')) { throw 'Unexpected SIMD experiment build.' }
    [pscustomobject]@{ Run = $run; Case = $cases[0] }
}
$left, $right, $baseline, $simd = $data
if ($baseline.Run.managedEncoderSha256 -ceq $simd.Run.managedEncoderSha256) { throw 'SIMD variant must change the encoder binary.' }
foreach ($field in @('testedRevision', 'managedEncoderSha256', 'managedRunnerSha256', 'nativeExecutableSha256')) {
    if ($left.Run.$field -cne $right.Run.$field) { throw 'Comparison binary/revision mismatch.' }
}
foreach ($item in $data) {
    foreach ($field in @('cpu', 'affinity', 'vectorFloatWidth')) {
        if ($left.Run.environment.$field -cne $item.Run.environment.$field) { throw 'Comparison host mismatch.' }
    }
    if ($left.Run.testedRevision -cne $item.Run.testedRevision) { throw 'Comparison source revision mismatch.' }
    if ($left.Run.nativeExecutableSha256 -cne $item.Run.nativeExecutableSha256) { throw 'Comparison native mismatch.' }
    if ($left.Case.managedJpegSha256 -cne $item.Case.managedJpegSha256) { throw 'Experiment changed JPEG bytes.' }
}
$ratio = $right.Case.managed.median / $left.Case.managed.median
$simdRatio = $simd.Case.managed.median / $baseline.Case.managed.median
$report = [Text.StringBuilder]::new()
[void]$report.Append("# One-case .NET runtime and portable SIMD API comparison`n`nKodak01 4:2:0 sequential distance 1, same CPU, 7 blocks/20 iterations/5 warmups. The first pair uses identical net10.0 binaries to isolate runtime changes. The second pair uses the pinned .NET 11 SDK and runtime, comparing portable Shuffle/Create lane assembly with Zip/Unzip/Concat in the DCT transpose and downsampling. Both implementations use portable APIs, with no CPU-specific intrinsic paths. All four JPEG hashes must match. No whole-corpus or migration verdict.`n`n")
[void]$report.Append("Host: $($left.Run.environment.cpu); architecture: $Architecture.`n`n")
[void]$report.Append("| Build/runtime | Managed median (ms) | Native median (ms) |`n| --- | ---: | ---: |`n")
for ($i = 0; $i -lt $versions.Count; $i++) {
    [void]$report.Append([string]::Format([Globalization.CultureInfo]::InvariantCulture,
        "| {0} | {1:F3} | {2:F3} |`n", $versions[$i], ($data[$i].Case.managed.median / 1e6), ($data[$i].Case.native.median / 1e6)))
}
[void]$report.Append([string]::Format([Globalization.CultureInfo]::InvariantCulture,
    "`n.NET 11/.NET 10 managed latency ratio: **{0:F6}** (lower is better). JPEG hashes match.`n", $ratio))
[void]$report.Append([string]::Format([Globalization.CultureInfo]::InvariantCulture,
    "`nExplicit .NET 11 SIMD/baseline latency ratio on .NET 11: **{0:F6}** (lower is better). This measures this rewrite only, not all possible new API uses.`n", $simdRatio))
$reportText = $report.ToString()
Set-Content -LiteralPath (Join-Path $Directory 'runtime-comparison.md') -Value $reportText -Encoding utf8NoBOM -NoNewline
$json = [ordered]@{
    versions = $versions
    net11OverNet10 = $ratio
    explicitSimdOverNet11Baseline = $simdRatio
    scope = 'one case, same CPU; separate runtime-only and explicit SIMD diagnostics'
} | ConvertTo-Json -Depth 5
Set-Content -LiteralPath (Join-Path $Directory 'runtime-comparison.json') -Value ($json + "`n") -Encoding utf8NoBOM -NoNewline
if ($Summary) { Add-Content -LiteralPath $Summary -Value $reportText -Encoding utf8NoBOM -NoNewline }
Write-Output $reportText
