#requires -Version 7.6
<#
.SYNOPSIS
Compare retained CI performance diagnostics without running benchmarks.
.DESCRIPTION
Output is a file prefix: reports are written to PREFIX.md and PREFIX.json.
SelfTest validates the comparator with synthetic evidence only.
.PARAMETER Baseline
Literal path to the baseline CI evidence directory.
.PARAMETER Current
Literal path to the current CI evidence directory.
.PARAMETER BaselineRevision
Full lowercase Git commit ID of the baseline revision.
.PARAMETER CurrentRevision
Full lowercase Git commit ID of the current revision.
.PARAMETER Output
Literal file prefix for the Markdown and JSON reports.
.PARAMETER Summary
Optional literal path to a Markdown summary to append to.
.PARAMETER SelfTest
Validate comparison and reporting with synthetic data without running benchmarks.
.EXAMPLE
./tools/compare-performance-revisions.ps1 -SelfTest
#>
[CmdletBinding(DefaultParameterSetName = 'Compare')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Compare')][ValidateNotNullOrWhiteSpace()][string] $Baseline,
    [Parameter(Mandatory, ParameterSetName = 'Compare')][ValidateNotNullOrWhiteSpace()][string] $Current,
    [Parameter(Mandatory, ParameterSetName = 'Compare')]
    [ValidatePattern('\A[0-9a-f]{40}\z', Options = 'None')][string] $BaselineRevision,
    [Parameter(Mandatory, ParameterSetName = 'Compare')]
    [ValidatePattern('\A[0-9a-f]{40}\z', Options = 'None')][string] $CurrentRevision,
    [Parameter(Mandatory, ParameterSetName = 'Compare')][ValidateNotNullOrWhiteSpace()][string] $Output,
    [Parameter(ParameterSetName = 'Compare')][ValidateNotNullOrWhiteSpace()][string] $Summary,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$commandArguments = @('compare-performance')
if ($SelfTest) {
    $commandArguments += '--self-test'
} else {
    $commandArguments += @('--baseline', $Baseline, '--current', $Current,
        '--baseline-revision', $BaselineRevision, '--current-revision', $CurrentRevision, '--output', $Output)
    if ($Summary) { $commandArguments += @('--summary', $Summary) }
}
& (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments $commandArguments
