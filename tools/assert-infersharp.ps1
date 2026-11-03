#requires -Version 7.6
<#
.SYNOPSIS
Rejects invalid InferSharp analysis and findings without a reviewed exception.
.DESCRIPTION
Infer exits successfully when it finds bugs. This gate also checks translation
coverage and binds each reviewed exception to an exact finding and source hash.
.PARAMETER ReportPath
Path to the InferSharp SARIF report.
.PARAMETER TranslationLogPath
Path to the Cilsil translation coverage log.
.PARAMETER BaselinePath
Optional reviewed exceptions file. Requires TargetFramework.
.PARAMETER TargetFramework
Framework whose exceptions to validate. Requires BaselinePath.
.PARAMETER SourceRoot
Root directory for reviewed source paths; defaults to the repository root.
.PARAMETER SelfTest
Runs synthetic gate fixtures without InferSharp, managed tests or benchmarks.
#>
[CmdletBinding(DefaultParameterSetName = 'Validate')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Validate')][ValidateNotNullOrEmpty()][string] $ReportPath,
    [Parameter(Mandatory, ParameterSetName = 'Validate')][ValidateNotNullOrEmpty()][string] $TranslationLogPath,
    [Parameter(ParameterSetName = 'Validate')][string] $BaselinePath,
    [Parameter(ParameterSetName = 'Validate')][string] $TargetFramework,
    [Parameter(ParameterSetName = 'Validate')][ValidateNotNullOrEmpty()][string] $SourceRoot = (Split-Path $PSScriptRoot -Parent),
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$arguments = @('infer-sharp')
if ($SelfTest) {
    $arguments += '--self-test'
} else {
    $arguments += @('--report', $ReportPath, '--translation-log', $TranslationLogPath, '--source-root', $SourceRoot)
    if ($BaselinePath) { $arguments += @('--baseline', $BaselinePath) }
    if ($TargetFramework) { $arguments += @('--framework', $TargetFramework) }
}
& (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments $arguments
