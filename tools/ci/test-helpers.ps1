#requires -Version 7.0
# Synthetic fixtures only: no encodes, profiling, or benchmarks.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$directory = Join-Path $temporaryRoot ('dotjpegli-ci-helpers-' + [guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($directory)
function Write-Json([string] $Path, [object] $Value) {
    [IO.File]::WriteAllText($Path, (ConvertTo-Json -InputObject $Value -Depth 10))
}
function Assert([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}
function Expect-Failure([scriptblock] $Action, [string] $Message) {
    $failed = $false
    try { & $Action | Out-Null } catch { $failed = $true }
    Assert $failed $Message
}
try {
    [IO.File]::WriteAllText((Join-Path $directory 'jpegli-parity-native'), 'synthetic native binary')
    $metadata = @{
        sourceUnmodified = $true
        revision = 'test-revision'
        executableSha256 = (Get-FileHash -LiteralPath (Join-Path $directory 'jpegli-parity-native')).Hash.ToLowerInvariant()
    }
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    & "$PSScriptRoot/verify-native.ps1" -Directory $directory -Revision 'test-revision'
    [IO.File]::AppendAllText((Join-Path $directory 'jpegli-parity-native'), 'changed')
    Expect-Failure { & "$PSScriptRoot/verify-native.ps1" -Directory $directory -Revision 'test-revision' } 'Changed native binary was accepted.'
    Expect-Failure { & "$PSScriptRoot/verify-native.ps1" -Directory $directory -Revision 'wrong-revision' } 'Wrong native revision was accepted.'
    $metadata.sourceUnmodified = $false
    Write-Json (Join-Path $directory 'provenance.json') $metadata
    Expect-Failure { & "$PSScriptRoot/verify-native.ps1" -Directory $directory -Revision 'test-revision' } 'Modified native source was accepted.'

    $run = @{ environment = @{ vector128 = $true; vector256 = $true; vector512 = $false; vectorFloatWidth = 8 }; narrowApplicable = $true }
    Write-Json (Join-Path $directory 'run.json') $run
    & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Architecture X64
    & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Narrow
    $run.environment.vector512 = $true
    Write-Json (Join-Path $directory 'run.json') $run
    Expect-Failure { & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Narrow } 'Wide SIMD was accepted for narrow evidence.'
    $run.environment.vector256 = 'true'
    $run.environment.vector512 = $false
    Write-Json (Join-Path $directory 'run.json') $run
    Expect-Failure { & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Narrow } 'String SIMD capability was accepted as a Boolean.'
    $run.environment = @{ vector128 = $true; vector256 = $false; vector512 = $false; vectorFloatWidth = 4 }
    $run.narrowApplicable = $false
    Write-Json (Join-Path $directory 'run.json') $run
    & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Architecture ARM64
    Expect-Failure { & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Architecture X64 } 'ARM64 capabilities were accepted as X64.'
    $run.narrowApplicable = $true
    Write-Json (Join-Path $directory 'run.json') $run
    Expect-Failure { & "$PSScriptRoot/assert-simd.ps1" -Evidence $directory -Architecture ARM64 } 'Applicable narrow SIMD was accepted for ARM64.'

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
    $selected = @(& "$PSScriptRoot/select-profile-cases.ps1" -Evidence $directory)
    Assert ($selected.Count -eq 1 -and $selected[0] -ceq 'a 444 0 1') 'Wrong selected-case profile.'
    $selected = @(& "$PSScriptRoot/select-profile-cases.ps1" -Evidence $directory -Full)
    Assert (($selected -join '|') -ceq 'a 444 0 1|b 420 2 4|c 444 0 1|tiny 420 0 1') 'Full profiles must select distinct worst images and the failing smallest input.'
    $cases[4].medianRatio = 1.10
    Write-Json (Join-Path $directory 'cases.json') $cases
    $selected = @(& "$PSScriptRoot/select-profile-cases.ps1" -Evidence $directory -Full)
    Assert ($selected.Count -eq 3) 'Smallest passing input must not add a profile.'

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
    & "$PSScriptRoot/report-runtime-comparison.ps1" -Directory $directory -Architecture X64 -Summary $summary | Out-Null
    $result = Get-Content -LiteralPath (Join-Path $directory 'runtime-comparison.json') -Raw | ConvertFrom-Json
    Assert ($result.net11OverNet10 -eq 1.1 -and [Math]::Abs($result.explicitSimdOverNet11Baseline - 13.0 / 12) -lt 1e-12) 'Wrong runtime comparison ratios.'
    Assert ((Get-Content -LiteralPath $summary -Raw).Contains('| 10.0.14 | 1.000 | 0.500 |')) 'Missing invariant Markdown timing output.'
    Write-Json (Join-Path $directory 'runtime-net11-simd/summary.json') @{ valid = 'false' }
    Expect-Failure { & "$PSScriptRoot/report-runtime-comparison.ps1" -Directory $directory -Summary '' } 'Runtime comparison accepted a string evidence verdict.'
    Write-Json (Join-Path $directory 'runtime-net11-simd/summary.json') @{ valid = $true }
    $simdPath = Join-Path $directory 'runtime-net11-simd/run.json'
    $simdRun = Get-Content -LiteralPath $simdPath -Raw | ConvertFrom-Json
    $simdRun.environment.affinity = '3'
    Write-Json $simdPath $simdRun
    Expect-Failure { & "$PSScriptRoot/report-runtime-comparison.ps1" -Directory $directory -Summary '' } 'Runtime comparison accepted a different host.'
    $simdRun.environment.affinity = '2'
    Write-Json $simdPath $simdRun
    Write-Json (Join-Path $directory 'runtime-net11-simd/cases.json') @(@{
        managedJpegSha256 = 'changed-jpeg'; managed = @{ median = 1300000 }; native = @{ median = 500000 }
    })
    Expect-Failure { & "$PSScriptRoot/report-runtime-comparison.ps1" -Directory $directory -Summary '' } 'Runtime comparison accepted changed JPEG bytes.'
    Write-Output 'CI helper synthetic checks passed.'
} finally {
    $resolved = [IO.Path]::GetFullPath($directory)
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($resolved) -notmatch '^dotjpegli-ci-helpers-[0-9a-f]{32}$') {
        throw 'Unexpected synthetic fixture cleanup path.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
