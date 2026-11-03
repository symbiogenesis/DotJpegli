#requires -Version 7.6
<#
.SYNOPSIS
Validates SIMD evidence, selects CPU profiles, and reports matched runtime comparisons.
.DESCRIPTION
-AssertSimd checks kernel capabilities (-Architecture X64/ARM64) or narrow runs
(-Narrow). -SelectProfiles emits one worst case, or up to three distinct images
and a failing smallest input with -Full. -CompareRuntimes validates and reports
the four pinned runtime experiments. -SelfTest uses synthetic evidence only.
.PARAMETER Evidence
Evidence directories containing run.json for SIMD checks, or one directory with
cases.json and (for -Full) sources.json for profile selection.
.PARAMETER Directory
Parent directory of the four runtime evidence directories.
.PARAMETER Architecture
Expected kernel architecture, or runtime report label (defaults to RUNNER_ARCH).
.PARAMETER Summary
Optional runtime job summary path; defaults to GITHUB_STEP_SUMMARY.
#>
[CmdletBinding(DefaultParameterSetName = 'Kernel')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Kernel')]
    [Parameter(Mandatory, ParameterSetName = 'Narrow')][switch] $AssertSimd,
    [Parameter(Mandatory, ParameterSetName = 'Profiles')][switch] $SelectProfiles,
    [Parameter(Mandatory, ParameterSetName = 'Runtime')][switch] $CompareRuntimes,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest,
    [Parameter(Mandatory, ParameterSetName = 'Kernel')]
    [Parameter(Mandatory, ParameterSetName = 'Narrow')]
    [Parameter(Mandatory, ParameterSetName = 'Profiles')][ValidateNotNullOrEmpty()][string[]] $Evidence,
    [Parameter(Mandatory, ParameterSetName = 'Kernel')]
    [Parameter(ParameterSetName = 'Runtime')][string] $Architecture = $env:RUNNER_ARCH,
    [Parameter(Mandatory, ParameterSetName = 'Narrow')][switch] $Narrow,
    [Parameter(ParameterSetName = 'Profiles')][switch] $Full,
    [Parameter(Mandatory, ParameterSetName = 'Runtime')][ValidateNotNullOrEmpty()][string] $Directory,
    [Parameter(ParameterSetName = 'Runtime')][string] $Summary = $env:GITHUB_STEP_SUMMARY
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
if ($AssertSimd) {
    if (-not $Narrow -and $Architecture -notin 'X64', 'ARM64') { throw 'Architecture must be X64 or ARM64.' }
    foreach ($directory in $Evidence) {
        $run = Get-Content -LiteralPath (Join-Path $directory 'run.json') -Raw | ConvertFrom-Json
        $environment = $run.environment
        if ($Narrow -or $Architecture -eq 'X64') {
            if ($environment.vector256 -isnot [bool] -or -not $environment.vector256 -or
                $environment.vector512 -isnot [bool] -or $environment.vector512 -or $environment.vectorFloatWidth -ne 8) {
                throw 'Expected accelerated narrow SIMD.'
            }
        }
        if (-not $Narrow) {
            if ($environment.vector128 -isnot [bool] -or -not $environment.vector128) { throw 'Expected accelerated SIMD.' }
            if ($Architecture -eq 'ARM64' -and ($environment.vectorFloatWidth -ne 4 -or
                $run.narrowApplicable -isnot [bool] -or $run.narrowApplicable)) {
                throw 'Expected ARM64 four-lane analysis.'
            }
        }
    }
    return
}

if ($SelectProfiles) {
    if ($Evidence.Count -ne 1) { throw 'Profile selection needs exactly one evidence directory.' }
    $cases = Get-Content -LiteralPath (Join-Path $Evidence[0] 'cases.json') -Raw | ConvertFrom-Json -NoEnumerate
    if ($cases -isnot [array] -or $cases.Count -eq 0) { throw 'No performance cases to profile.' }
    $ordered = @($cases | Sort-Object -Property medianRatio -Descending -Stable)
    $profiles = [System.Collections.Generic.List[object]]::new()
    if ($Full) {
        $selected = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($case in $ordered) {
            if (-not $selected.Add($case.id)) { continue }
            $profiles.Add($case)
            if ($selected.Count -eq 3) { break }
        }
        $sources = Get-Content -LiteralPath (Join-Path $Evidence[0] 'sources.json') -Raw | ConvertFrom-Json -NoEnumerate
        if ($sources -isnot [array] -or $sources.Count -eq 0) { throw 'No source images to profile.' }
        $smallest = ($sources | Sort-Object { [long]$_.width * $_.height } -Stable | Select-Object -First 1).id
        if (-not $selected.Contains($smallest)) {
            $case = $ordered | Where-Object { $_.id -ceq $smallest } | Select-Object -First 1
            if ($null -eq $case) { throw 'Smallest source image has no performance cases.' }
            if ($case.medianRatio -gt 1.10) { $profiles.Add($case) }
        }
    } else {
        $profiles.Add($ordered[0])
    }
    foreach ($case in $profiles) {
        [string]::Format([Globalization.CultureInfo]::InvariantCulture, '{0} {1} {2} {3}',
            $case.id, $case.sampling, $case.progressiveLevel, $case.distance)
    }
    return
}

if ($CompareRuntimes) {
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
    return
}

if (-not $SelfTest) { throw 'Select a performance evidence operation.' }
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$directory = Join-Path $temporaryRoot ('dotjpegli-ci-helpers-' + [guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($directory)
function Write-Json([string] $Path, [object] $Value) {
    Set-Content -LiteralPath $Path -Value (ConvertTo-Json -InputObject $Value -Depth 10) -Encoding utf8NoBOM -NoNewline
}
function Assert-Condition([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}
function Assert-Failure([scriptblock] $Action, [string] $Message) {
    $failed = $false
    try { & $Action | Out-Null } catch { $failed = $true }
    Assert-Condition $failed $Message
}
try {
    $native = Join-Path $PSScriptRoot '../parity/build-upstream.ps1'
    $executable = Join-Path $directory ($IsWindows ? 'jpegli-parity-native.exe' : 'jpegli-parity-native')
    [IO.File]::WriteAllText($executable, 'synthetic native binary')
    $metadata = @{
        sourceUnmodified = $true
        revision = '031a0077f5799a6041004267fc12b956c1f52a20'
        executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    & $native -BuildDirectory $directory -VerifyOnly
    [IO.File]::AppendAllText($executable, 'changed')
    Assert-Failure { & $native -BuildDirectory $directory -VerifyOnly } 'Changed native binary was accepted.'
    [IO.File]::WriteAllText($executable, 'synthetic native binary')
    $metadata.revision = 'wrong-revision'
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    Assert-Failure { & $native -BuildDirectory $directory -VerifyOnly } 'Wrong native revision was accepted.'
    $metadata.revision = '031a0077f5799a6041004267fc12b956c1f52a20'
    $metadata.sourceUnmodified = $false
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    Assert-Failure { & $native -BuildDirectory $directory -VerifyOnly } 'Modified native source was accepted.'
    $metadata.sourceUnmodified = $true
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    $release = Join-Path $directory 'Release'
    [void][IO.Directory]::CreateDirectory($release)
    Move-Item -LiteralPath $executable -Destination $release
    & $native -BuildDirectory $directory -VerifyOnly

    $run = @{ environment = @{ vector128 = $true; vector256 = $true; vector512 = $false; vectorFloatWidth = 8 }; narrowApplicable = $true }
    Write-Json (Join-Path $directory 'run.json') $run
    & $PSCommandPath -AssertSimd -Evidence $directory -Architecture X64
    & $PSCommandPath -AssertSimd -Evidence $directory -Narrow
    $run.environment.vector512 = $true
    Write-Json (Join-Path $directory 'run.json') $run
    Assert-Failure { & $PSCommandPath -AssertSimd -Evidence $directory -Narrow } 'Wide SIMD was accepted for narrow evidence.'
    $run.environment.vector256 = 'true'
    $run.environment.vector512 = $false
    Write-Json (Join-Path $directory 'run.json') $run
    Assert-Failure { & $PSCommandPath -AssertSimd -Evidence $directory -Narrow } 'String SIMD capability was accepted as a Boolean.'
    $run.environment = @{ vector128 = $true; vector256 = $false; vector512 = $false; vectorFloatWidth = 4 }
    $run.narrowApplicable = $false
    Write-Json (Join-Path $directory 'run.json') $run
    & $PSCommandPath -AssertSimd -Evidence $directory -Architecture ARM64
    Assert-Failure { & $PSCommandPath -AssertSimd -Evidence $directory -Architecture X64 } 'ARM64 capabilities were accepted as X64.'
    $run.narrowApplicable = $true
    Write-Json (Join-Path $directory 'run.json') $run
    Assert-Failure { & $PSCommandPath -AssertSimd -Evidence $directory -Architecture ARM64 } 'Applicable narrow SIMD was accepted for ARM64.'

    $cases = @(
        @{ id = 'a'; medianRatio = 5; sampling = 444; progressiveLevel = 0; distance = 1 },
        @{ id = 'a'; medianRatio = 4; sampling = 420; progressiveLevel = 2; distance = 4 },
        @{ id = 'b'; medianRatio = 3; sampling = 420; progressiveLevel = 2; distance = 4 },
        @{ id = 'c'; medianRatio = 2; sampling = 444; progressiveLevel = 0; distance = 1 },
        @{ id = 'tiny'; medianRatio = 1.11; sampling = 420; progressiveLevel = 0; distance = 1 }
    )
    Write-Json (Join-Path $directory 'cases.json') $cases
    Write-Json (Join-Path $directory 'sources.json') @(
        @{ id = 'a'; width = 512; height = 768 },
        @{ id = 'tiny'; width = 1; height = 1 }
    )
    $selected = @(& $PSCommandPath -SelectProfiles -Evidence $directory)
    Assert-Condition ($selected.Count -eq 1 -and $selected[0] -ceq 'a 444 0 1') 'Wrong selected-case profile.'
    $selected = @(& $PSCommandPath -SelectProfiles -Evidence $directory -Full)
    Assert-Condition (($selected -join '|') -ceq 'a 444 0 1|b 420 2 4|c 444 0 1|tiny 420 0 1') 'Full profiles must select distinct worst images and the failing smallest input.'
    $cases[4].medianRatio = 1.10
    Write-Json (Join-Path $directory 'cases.json') $cases
    $selected = @(& $PSCommandPath -SelectProfiles -Evidence $directory -Full)
    Assert-Condition ($selected.Count -eq 3) 'Smallest passing input must not add a profile.'
    Write-Json (Join-Path $directory 'cases.json') @()
    Assert-Failure { & $PSCommandPath -SelectProfiles -Evidence $directory } 'Empty case arrays were accepted.'
    Write-Json (Join-Path $directory 'cases.json') $cases[0]
    Assert-Failure { & $PSCommandPath -SelectProfiles -Evidence $directory } 'A case object was accepted instead of an array.'

    $versions = @('10.0.14', '11.0.0-rc.3.26522.148', 'net11-baseline', 'net11-simd')
    for ($i = 0; $i -lt $versions.Count; $i++) {
        $path = Join-Path $directory "runtime-$($versions[$i])"
        [void][IO.Directory]::CreateDirectory($path)
        $run = @{
            testedRevision = 'revision'; managedEncoderSha256 = $(if ($i -eq 3) { 'simd' } elseif ($i -eq 2) { 'baseline' } else { 'shared' })
            managedRunnerSha256 = 'runner'; nativeExecutableSha256 = 'native'
            environment = @{
                cpu = 'synthetic CPU'; affinity = '2'; vectorFloatWidth = 8
                runtimeVersion = $(if ($i -eq 0) { '10.0.14' } else { '11.0.0' })
                runtimeProductVersion = $(if ($i -eq 0) { '10.0.14-servicing.test' } else { '11.0.0-rc.3.26522.148' })
                explicitNet11Simd = ($i -eq 3)
            }
        }
        Write-Json (Join-Path $path 'run.json') $run
        Write-Json (Join-Path $path 'summary.json') @{ valid = $true }
        Write-Json (Join-Path $path 'cases.json') @(@{
            managedJpegSha256 = 'jpeg'; managed = @{ median = (1000000 + 100000 * $i) }; native = @{ median = 500000 }
        })
    }
    $summary = Join-Path $directory 'job-summary.md'
    & $PSCommandPath -CompareRuntimes -Directory $directory -Architecture X64 -Summary $summary | Out-Null
    $result = Get-Content -LiteralPath (Join-Path $directory 'runtime-comparison.json') -Raw | ConvertFrom-Json
    Assert-Condition ($result.net11OverNet10 -eq 1.1 -and [Math]::Abs($result.explicitSimdOverNet11Baseline - 13.0 / 12) -lt 1e-12) 'Wrong runtime comparison ratios.'
    Assert-Condition ((Get-Content -LiteralPath $summary -Raw).Contains('| 10.0.14 | 1.000 | 0.500 |')) 'Missing invariant Markdown timing output.'
    $runtimeCasePath = Join-Path $directory 'runtime-net11-simd/cases.json'
    $runtimeCase = (Get-Content -LiteralPath $runtimeCasePath -Raw | ConvertFrom-Json -NoEnumerate)[0]
    Write-Json $runtimeCasePath $runtimeCase
    Assert-Failure { & $PSCommandPath -CompareRuntimes -Directory $directory -Summary '' } 'Runtime comparison accepted a case object instead of an array.'
    Write-Json $runtimeCasePath @($runtimeCase)
    Write-Json (Join-Path $directory 'runtime-net11-simd/summary.json') @{ valid = 'false' }
    Assert-Failure { & $PSCommandPath -CompareRuntimes -Directory $directory -Summary '' } 'Runtime comparison accepted a string evidence verdict.'
    Write-Json (Join-Path $directory 'runtime-net11-simd/summary.json') @{ valid = $true }
    $simdPath = Join-Path $directory 'runtime-net11-simd/run.json'
    $simdRun = Get-Content -LiteralPath $simdPath -Raw | ConvertFrom-Json
    $simdRun.environment.affinity = '3'
    Write-Json $simdPath $simdRun
    Assert-Failure { & $PSCommandPath -CompareRuntimes -Directory $directory -Summary '' } 'Runtime comparison accepted a different host.'
    $simdRun.environment.affinity = '2'
    Write-Json $simdPath $simdRun
    Write-Json (Join-Path $directory 'runtime-net11-simd/cases.json') @(@{
        managedJpegSha256 = 'changed-jpeg'; managed = @{ median = 1300000 }; native = @{ median = 500000 }
    })
    Assert-Failure { & $PSCommandPath -CompareRuntimes -Directory $directory -Summary '' } 'Runtime comparison accepted changed JPEG bytes.'
    Write-Output 'Performance evidence synthetic checks passed.'
} finally {
    $resolved = [IO.Path]::GetFullPath($directory)
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($resolved) -notmatch '^dotjpegli-ci-helpers-[0-9a-f]{32}$') {
        throw 'Unexpected synthetic fixture cleanup path.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
