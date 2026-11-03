#requires -Version 7.6
<#
.SYNOPSIS
Selects a bounded set of performance cases for CPU profiling.
.PARAMETER Evidence
Evidence directory containing cases.json and, for full selection, sources.json.
.PARAMETER Full
Selects the worst case for up to three distinct images and a failing smallest image.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $Evidence,
    [switch] $Full
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$cases = Get-Content -LiteralPath (Join-Path $Evidence 'cases.json') -Raw | ConvertFrom-Json -NoEnumerate
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
    $sources = Get-Content -LiteralPath (Join-Path $Evidence 'sources.json') -Raw | ConvertFrom-Json -NoEnumerate
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
