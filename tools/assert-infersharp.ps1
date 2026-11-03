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
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $ReportPath,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $TranslationLogPath,
    [string] $BaselinePath,
    [string] $TargetFramework,
    [ValidateNotNullOrEmpty()][string] $SourceRoot = (Split-Path $PSScriptRoot -Parent)
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
if (-not (Test-Path -LiteralPath $ReportPath -PathType Leaf)) { throw "Missing InferSharp SARIF report: $ReportPath" }
try { $report = Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json -AsHashtable }
catch { throw "Invalid InferSharp SARIF: $($_.Exception.Message)" }
if ($report -isnot [Collections.IDictionary] -or $report['version'] -ne '2.1.0' -or
    $report['runs'] -isnot [array] -or $report['runs'].Count -eq 0) {
    throw 'Invalid InferSharp SARIF: expected SARIF 2.1.0 with at least one run.'
}

# Cilsil can skip unsupported methods and still exit successfully. Retain its coverage
# statistics, and never accept an empty translation as a clean analysis.
if (-not (Test-Path -LiteralPath $TranslationLogPath -PathType Leaf)) { throw "Missing InferSharp translation log: $TranslationLogPath" }
$log = (Get-Content -LiteralPath $TranslationLogPath -Raw) ?? ''
foreach ($label in 'Method successfully translated', 'Method partially translated', 'Instructions translated', 'Instructions skipped') {
    $statistics = [regex]::Matches($log, '(?m)^' + [regex]::Escape($label) + ': (?<count>[0-9]+) \((?<percent>[0-9]+)%\)\r?$')
    if ($statistics.Count -ne 1) { throw "Invalid InferSharp coverage: missing or ambiguous '$label' statistic." }
    $statistic = $statistics[0]
    if ($label -in 'Method successfully translated', 'Instructions translated' -and [long]$statistic.Groups['count'].Value -eq 0) {
        throw "Empty InferSharp translation: $label must be positive."
    }
    Write-Output $statistic.Value.TrimEnd()
}

$findings = [Collections.Generic.List[object]]::new()
foreach ($run in $report['runs']) {
    if ($run -isnot [Collections.IDictionary] -or $run['tool'] -isnot [Collections.IDictionary] -or
        $run['tool']['driver'] -isnot [Collections.IDictionary] -or $run['tool']['driver']['name'] -ne 'Infer' -or
        $run['results'] -isnot [array]) {
        throw 'Invalid InferSharp SARIF: each Infer run must contain a results array.'
    }
    foreach ($result in $run['results']) {
        if ($result -isnot [Collections.IDictionary]) { throw 'Invalid InferSharp SARIF: each result must be an object.' }
        $findings.Add($result)
    }
}

function Get-Identity {
    [OutputType([string])]
    param([string] $Rule, [string] $Path, [long] $Line, [string] $Fingerprint)
    "$Rule|$Path|$Line|$Fingerprint"
}

$reviewed = [Collections.Generic.Dictionary[string, bool]]::new([StringComparer]::Ordinal)
if ($BaselinePath) {
    try { $baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json -AsHashtable }
    catch { throw "Invalid InferSharp baseline: $($_.Exception.Message)" }
    if ($baseline -isnot [Collections.IDictionary] -or $baseline['version'] -ne 1 -or $baseline['analyzer'] -ne 'InferSharp v1.5' -or
        $baseline['reviewedRevision'] -notmatch '^[0-9a-f]{40}$' -or
        $baseline['sources'] -isnot [Collections.IDictionary] -or $baseline['sources'].Count -eq 0 -or
        $baseline['targets'] -isnot [Collections.IDictionary] -or
        -not $TargetFramework -or $baseline['targets'][$TargetFramework] -isnot [array]) {
        throw 'Invalid InferSharp baseline: expected a reviewed v1.5 baseline for this framework.'
    }
    foreach ($entry in $baseline['sources'].GetEnumerator()) {
        if ($entry.Key -notmatch '^[A-Za-z0-9_.-]+(?:/[A-Za-z0-9_.-]+)*$' -or '..' -in $entry.Key.Split('/') -or
            $entry.Value -notmatch '^[0-9a-f]{64}$') { throw 'Invalid InferSharp baseline: invalid source identity.' }
        $source = Join-Path $SourceRoot $entry.Key
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "InferSharp reviewed source changed or is missing: $($entry.Key)" }
        $normalized = ((Get-Content -LiteralPath $source -Raw) ?? '').Replace("`r`n", "`n")
        $hash = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($normalized)))
        if ($hash -ne $entry.Value) { throw "InferSharp reviewed source changed: $($entry.Key). Re-review the affected exceptions." }
    }
    foreach ($entry in $baseline['targets'][$TargetFramework]) {
        if ($entry -isnot [Collections.IDictionary] -or -not $entry['ruleId'] -or
            -not $entry['path'] -or -not $baseline['sources'].Contains($entry['path']) -or $entry['line'] -le 0 -or
            $entry['fingerprint'] -notmatch '^[0-9a-f]{1,32}$' -or [string]::IsNullOrWhiteSpace($entry['reason'])) {
            throw 'Invalid InferSharp baseline: every exception needs an exact identity, reviewed source and reason.'
        }
        $identity = Get-Identity -Rule $entry['ruleId'] -Path $entry['path'] -Line $entry['line'] -Fingerprint $entry['fingerprint']
        if ($reviewed.ContainsKey($identity)) { throw 'Invalid InferSharp baseline: duplicate exception.' }
        $reviewed[$identity] = $false
    }
} elseif ($TargetFramework) { throw 'Invalid InferSharp baseline: TargetFramework requires BaselinePath.' }

$unreviewed = 0
foreach ($finding in $findings) {
    if ($finding['locations'] -isnot [array] -or $finding['locations'].Count -eq 0 -or
        $finding['locations'][0] -isnot [Collections.IDictionary] -or
        $finding['locations'][0]['physicalLocation'] -isnot [Collections.IDictionary]) {
        throw 'Invalid InferSharp SARIF: each finding requires a physical location.'
    }
    $location = $finding['locations'][0]['physicalLocation']
    if ($finding['ruleId'] -isnot [string] -or [string]::IsNullOrWhiteSpace($finding['ruleId']) -or
        $finding['message'] -isnot [Collections.IDictionary] -or $finding['message']['text'] -isnot [string] -or
        $location['artifactLocation'] -isnot [Collections.IDictionary] -or $location['artifactLocation']['uri'] -isnot [string] -or
        $location['region'] -isnot [Collections.IDictionary] -or $location['region']['startLine'] -isnot [long] -or $location['region']['startLine'] -le 0) {
        throw 'Invalid InferSharp SARIF: each finding requires a rule, message, source URI and positive line number.'
    }
    $path = $location['artifactLocation']['uri'] -replace '^file:/_/', ''
    $fingerprint = if ($finding['fingerprints'] -is [Collections.IDictionary]) { $finding['fingerprints']['hash/v1'] } else { '' }
    $identity = Get-Identity -Rule $finding['ruleId'] -Path $path -Line $location['region']['startLine'] -Fingerprint $fingerprint
    if ($reviewed.ContainsKey($identity) -and -not $reviewed[$identity]) { $reviewed[$identity] = $true }
    else {
        $unreviewed++
        Write-Output "UNREVIEWED: $($finding['ruleId']) $path $($location['region']['startLine']) $($finding['message']['text'])"
    }
}
Write-Output "InferSharp: $($findings.Count) findings; unreviewed findings: $unreviewed."
if ($unreviewed -gt 0) { throw "InferSharp reported $unreviewed unreviewed finding(s). Review the uploaded text and SARIF reports." }
if ($reviewed.Values -contains $false) { throw 'Stale InferSharp baseline: remove exceptions for findings no longer reported.' }
