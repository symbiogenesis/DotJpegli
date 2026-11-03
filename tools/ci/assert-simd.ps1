#requires -Version 7.0
[CmdletBinding(DefaultParameterSetName = 'Kernel')]
param(
    [Parameter(Mandatory)][string[]] $Evidence,
    [Parameter(Mandatory, ParameterSetName = 'Kernel')][ValidateSet('X64', 'ARM64')][string] $Architecture,
    [Parameter(Mandatory, ParameterSetName = 'Narrow')][switch] $Narrow
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
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
