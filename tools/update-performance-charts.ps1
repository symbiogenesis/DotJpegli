#requires -Version 7.6
<#
.SYNOPSIS
Regenerates README performance charts from retained CI measurements.
.DESCRIPTION
Reads complete performance and quality evidence; never runs benchmarks.
.PARAMETER Performance
Directory containing the full performance summary, cases and run identity.
.PARAMETER Quality
Directory containing the full quality/compression summary and matched-quality data.
.PARAMETER Output
Directory in which to write the SVG charts and their numeric source data.
#>
[CmdletBinding(DefaultParameterSetName = 'Generate')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Generate')][ValidateNotNullOrEmpty()][string] $Performance,
    [Parameter(Mandatory, ParameterSetName = 'Generate')][ValidateNotNullOrEmpty()][string] $Quality,
    [Parameter(Mandatory, ParameterSetName = 'Generate')][ValidateNotNullOrEmpty()][string] $Output,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$commandArguments = @('performance-charts')
if ($SelfTest) {
    $commandArguments += '--self-test'
} else {
    $commandArguments += @('--performance', $Performance, '--quality', $Quality, '--output', $Output)
}
& (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments $commandArguments
