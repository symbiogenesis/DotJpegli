#requires -Version 7.6
<#
.SYNOPSIS
Builds and invokes the compiled repository automation tool.
.DESCRIPTION
Keeps build output off the success stream so callers can consume command output.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $CommandArguments
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$repository = Split-Path $PSScriptRoot -Parent
$project = Join-Path $PSScriptRoot 'DotJpegli.Automation/DotJpegli.Automation.csproj'
$assembly = Join-Path $PSScriptRoot 'DotJpegli.Automation/bin/Release/net10.0/DotJpegli.Automation.dll'
# MSBuild's incremental checks keep repeated invocations current after source edits.
# Capture build messages, including first-run SDK output, so shell consumers receive
# only the requested output (Out-Host would still reach a parent process's stdout).
Push-Location -LiteralPath $repository
try {
    $buildOutput = dotnet build $project -c Release --nologo --verbosity quiet -p:RestoreLockedMode=true 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Could not build the repository automation tool.`n$($buildOutput -join "`n")" }
} finally {
    Pop-Location
}
# Resolve command paths against the caller's working directory.
dotnet $assembly @CommandArguments
if ($LASTEXITCODE -ne 0) { throw "Repository automation failed ($($CommandArguments[0]))." }
