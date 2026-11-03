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
$commandArguments = @('performance-evidence')
if ($SelfTest) {
    $commandArguments += '--self-test'
} elseif ($AssertSimd) {
    $commandArguments += '--assert-simd'
    if ($Narrow) { $commandArguments += '--narrow' }
    foreach ($path in $Evidence) { $commandArguments += @('--evidence', $path) }
    if ($Architecture) { $commandArguments += @('--architecture', $Architecture) }
} elseif ($SelectProfiles) {
    $commandArguments += '--select-profiles'
    foreach ($path in $Evidence) { $commandArguments += @('--evidence', $path) }
    if ($Full) { $commandArguments += '--full' }
} elseif ($CompareRuntimes) {
    $commandArguments += @('--compare-runtimes', '--directory', $Directory)
    if ($Architecture) { $commandArguments += @('--architecture', $Architecture) }
    if ($Summary) { $commandArguments += @('--summary', $Summary) }
}
& (Join-Path $PSScriptRoot '../Invoke-Automation.ps1') -CommandArguments $commandArguments
