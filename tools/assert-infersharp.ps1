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
if (-not $SelfTest) {
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
    return
}

$gate = $PSCommandPath
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $temporaryRoot ('DotJpegli-infersharp-' + [guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($fixtureRoot)
$reportPath = Join-Path $fixtureRoot 'report[fixture].sarif'
$logPath = Join-Path $fixtureRoot 'translation[fixture].txt'
$baselinePath = Join-Path $fixtureRoot 'baseline[fixture].json'
$sourcePath = Join-Path $fixtureRoot 'src/Example.cs'
[void][IO.Directory]::CreateDirectory((Split-Path $sourcePath -Parent))
$sourceText = "class Example { }`n"
[IO.File]::WriteAllText($sourcePath, $sourceText)
$sourceHash = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sourceText)))
$coverage = @'
Method successfully translated: 20 (80%)
Method partially translated: 5 (20%)
Instructions translated: 100 (90%)
Instructions skipped: 10 (10%)
'@
$script:passed = 0

function Write-Json([string] $Path, [object] $Value) { Set-Content -LiteralPath $Path -Value (ConvertTo-Json -InputObject $Value -Depth 20) -Encoding utf8NoBOM -NoNewline }
function Get-RunFixture([object[]] $Results = @()) { @{ tool = @{ driver = @{ name = 'Infer' } }; results = @($Results) } }
function Write-Report([object[]] $Results = @(), [object[]] $Runs = @((Get-RunFixture $Results))) {
    Write-Json $reportPath @{ version = '2.1.0'; runs = @($Runs) }
}
function Get-FindingFixture {
    @{
        ruleId = 'NULL_DEREFERENCE'
        message = @{ text = 'Fixture finding' }
        fingerprints = @{ 'hash/v1' = 'abcdef0123456789' }
        locations = @(@{ physicalLocation = @{
            artifactLocation = @{ uri = 'file:/_/src/Example.cs' }
            region = @{ startLine = 1 }
        } })
    }
}
function Get-BaselineFixture {
    @{
        version = 1
        analyzer = 'InferSharp v1.5'
        reviewedRevision = 'a' * 40
        sources = @{ 'src/Example.cs' = $sourceHash }
        targets = @{ 'net11.0' = @(@{
            ruleId = 'NULL_DEREFERENCE'
            path = 'src/Example.cs'
            line = 1
            fingerprint = 'abcdef0123456789'
            reason = 'Test fixture for an explicitly reviewed finding.'
        }) }
    }
}
function Write-Fixture {
    Write-Report
    [IO.File]::WriteAllText($logPath, $coverage)
    [IO.File]::WriteAllText($sourcePath, $sourceText)
    Write-Json $baselinePath (Get-BaselineFixture)
}
function Assert-Gate([string] $Name, [string] $Failure = '', [switch] $Baseline, [switch] $Relative) {
    $arguments = @{ ReportPath = $reportPath; TranslationLogPath = $logPath }
    if ($Baseline) {
        $arguments.BaselinePath = $baselinePath
        $arguments.TargetFramework = 'net11.0'
        $arguments.SourceRoot = $fixtureRoot
    }
    if ($Relative) {
        $arguments.ReportPath = Split-Path $reportPath -Leaf
        $arguments.TranslationLogPath = Split-Path $logPath -Leaf
        if ($Baseline) {
            $arguments.BaselinePath = Split-Path $baselinePath -Leaf
            $arguments.SourceRoot = '.'
        }
    }
    $caught = $null
    try { $output = @(& $gate @arguments) }
    catch { $caught = $_.Exception.Message }
    if ($Failure) {
        if (-not $caught -or -not $caught.Contains($Failure)) {
            throw "$Name expected failure containing '$Failure'; actual: '$caught'."
        }
    } else {
        if ($caught) { throw "$Name unexpectedly failed: $caught" }
        if (-not ($output -match '^InferSharp: .*unreviewed findings: 0\.$')) { throw "$Name did not report successful findings validation." }
    }
    $script:passed++
    Write-Output "PASS: $Name"
}

try {
    Write-Fixture
    Assert-Gate 'Clean report with nonzero translation'
    Push-Location -LiteralPath $fixtureRoot
    try { Assert-Gate 'Relative report and log paths resolve after Push-Location' -Relative }
    finally { Pop-Location }

    Write-Report -Runs @((Get-RunFixture), (Get-RunFixture))
    Assert-Gate 'Clean report with multiple runs'

    Write-Report @((Get-FindingFixture))
    Assert-Gate 'Findings fail regardless of analyzer exit status' 'reported 1 unreviewed finding'

    Write-Report -Runs @((Get-RunFixture), (Get-RunFixture @((Get-FindingFixture))))
    Assert-Gate 'Findings in later SARIF runs fail' 'reported 1 unreviewed finding'

    Write-Fixture
    [IO.File]::Delete($reportPath)
    Assert-Gate 'Missing report fails' 'Missing InferSharp SARIF report'

    [IO.File]::WriteAllText($reportPath, '{ invalid JSON')
    Assert-Gate 'Malformed JSON fails' 'Invalid InferSharp SARIF'

    foreach ($invalid in @(
        @{ version = '1.0.0'; runs = @((Get-RunFixture)) },
        @{ version = '2.1.0'; runs = @() },
        @{ version = '2.1.0'; runs = (Get-RunFixture) },
        @{ version = '2.1.0' }
    )) {
        Write-Json $reportPath $invalid
        Assert-Gate 'Invalid report shape fails' 'Invalid InferSharp SARIF'
    }
    foreach ($invalidRun in @(
        @{ tool = @{ driver = @{ name = 'Other' } }; results = @() },
        @{ tool = @{ driver = @{ name = 'Infer' } } },
        @{ tool = @{ driver = @{ name = 'Infer' } }; results = $null },
        @{ tool = @{ driver = @{ name = 'Infer' } }; results = @{} },
        @{ results = @() },
        @{ tool = @(@{ driver = @{ name = 'Infer' } }); results = @() },
        (Get-RunFixture @('invalid result'))
    )) {
        Write-Report -Runs @($invalidRun)
        Assert-Gate 'Invalid run, tool or results fails' 'Invalid InferSharp SARIF'
    }

    foreach ($field in 'ruleId', 'message', 'locations') {
        $finding = Get-FindingFixture
        $finding.Remove($field)
        Write-Report @($finding)
        Assert-Gate "Finding without $field fails" 'Invalid InferSharp SARIF'
    }
    foreach ($invalidLocation in @(
        @{},
        @{ physicalLocation = @{} },
        @{ physicalLocation = @{ artifactLocation = @{ uri = 'file:/_/src/Example.cs' }; region = @{ startLine = 0 } } },
        @{ physicalLocation = @{ artifactLocation = @{ uri = 'file:/_/src/Example.cs' }; region = @{ startLine = '1' } } }
    )) {
        $finding = Get-FindingFixture
        $finding.locations = @($invalidLocation)
        Write-Report @($finding)
        Assert-Gate 'Malformed finding location fails' 'Invalid InferSharp SARIF'
    }

    Write-Fixture
    [IO.File]::Delete($logPath)
    Assert-Gate 'Missing translation log fails' 'Missing InferSharp translation log'
    [IO.File]::WriteAllText($logPath, '')
    Assert-Gate 'Empty translation log fails' 'Invalid InferSharp coverage'
    foreach ($label in 'Method successfully translated', 'Method partially translated', 'Instructions translated', 'Instructions skipped') {
        $line = [regex]::Match($coverage, '(?m)^' + [regex]::Escape($label) + ':.*$').Value
        [IO.File]::WriteAllText($logPath, $coverage.Replace($line, ''))
        Assert-Gate "Missing $label statistic fails" 'Invalid InferSharp coverage'
        [IO.File]::WriteAllText($logPath, $coverage + "`n" + $line)
        Assert-Gate "Duplicate $label statistic fails" 'Invalid InferSharp coverage'
    }
    foreach ($label in 'Method successfully translated', 'Instructions translated') {
        [IO.File]::WriteAllText($logPath, [regex]::Replace($coverage, '(?m)^' + [regex]::Escape($label) + ':.*$', "$($label): 0 (0%)"))
        Assert-Gate "Zero $label fails" 'Empty InferSharp translation'
    }
    [IO.File]::WriteAllText($logPath, $coverage.Replace("`n", "`r`n"))
    Assert-Gate 'CRLF translation log succeeds'

    Write-Fixture
    Write-Report @((Get-FindingFixture))
    Assert-Gate 'Exact reviewed baseline identity succeeds' -Baseline
    Push-Location -LiteralPath $fixtureRoot
    try { Assert-Gate 'Relative baseline and source root resolve after Push-Location' -Baseline -Relative }
    finally { Pop-Location }
    [IO.File]::WriteAllText($sourcePath, $sourceText.Replace("`n", "`r`n"))
    Assert-Gate 'Source hashes normalize line endings' -Baseline

    $newFinding = Get-FindingFixture
    $newFinding.ruleId = 'RESOURCE_LEAK'
    Write-Report @((Get-FindingFixture), $newFinding)
    Assert-Gate 'New finding alongside a reviewed finding fails' 'reported 1 unreviewed finding' -Baseline
    Write-Report @((Get-FindingFixture))

    foreach ($field in 'ruleId', 'path', 'line', 'fingerprint') {
        $baseline = Get-BaselineFixture
        switch ($field) {
            'ruleId' { $baseline.targets['net11.0'][0].ruleId = 'RESOURCE_LEAK' }
            'path' {
                $baseline.targets['net11.0'][0].path = 'src/Other.cs'
                $baseline.sources['src/Other.cs'] = $sourceHash
                [IO.File]::WriteAllText((Join-Path $fixtureRoot 'src/Other.cs'), $sourceText)
            }
            'line' { $baseline.targets['net11.0'][0].line = 2 }
            'fingerprint' { $baseline.targets['net11.0'][0].fingerprint = '1234' }
        }
        Write-Json $baselinePath $baseline
        Assert-Gate "Changed finding $field is unreviewed" 'reported 1 unreviewed finding' -Baseline
    }

    Write-Fixture
    Assert-Gate 'Stale baseline fails' 'Stale InferSharp baseline' -Baseline
    Write-Report @((Get-FindingFixture))
    [IO.File]::WriteAllText($sourcePath, "class Changed { }`n")
    Assert-Gate 'Changed reviewed source fails' 'reviewed source changed' -Baseline
    [IO.File]::Delete($sourcePath)
    Assert-Gate 'Missing reviewed source fails' 'reviewed source changed or is missing' -Baseline

    Write-Fixture
    $baseline = Get-BaselineFixture
    $baseline.targets['net11.0'] += $baseline.targets['net11.0'][0]
    Write-Json $baselinePath $baseline
    Assert-Gate 'Duplicate baseline exception fails' 'duplicate exception' -Baseline

    Write-Fixture
    Write-Report @((Get-FindingFixture), (Get-FindingFixture))
    Assert-Gate 'One baseline exception cannot hide duplicate findings' 'reported 1 unreviewed finding' -Baseline

    Write-Fixture
    $baseline = Get-BaselineFixture
    $baseline.targets['net11.0'][0].reason = ''
    Write-Json $baselinePath $baseline
    Assert-Gate 'Unexplained baseline exception fails' 'every exception needs an exact identity' -Baseline

    [IO.File]::Delete($baselinePath)
    Assert-Gate 'Missing baseline fails' 'Invalid InferSharp baseline' -Baseline
    [IO.File]::WriteAllText($baselinePath, '{ invalid JSON')
    Assert-Gate 'Malformed baseline fails' 'Invalid InferSharp baseline' -Baseline
    $baseline = Get-BaselineFixture
    $baseline.targets['net11.0'] = $null
    Write-Json $baselinePath $baseline
    Assert-Gate 'Baseline without selected framework fails' 'Invalid InferSharp baseline' -Baseline
    Write-Output "Passed $script:passed InferSharp gate checks."
}
finally {
    # Remove only this invocation's unique, absolute fixture directory.
    $resolved = (Resolve-Path -LiteralPath $fixtureRoot).ProviderPath
    if ([IO.Path]::GetFullPath($resolved) -cne $fixtureRoot -or
        -not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($resolved) -notmatch '^DotJpegli-infersharp-[0-9a-f]{32}$') {
        throw 'Refusing to remove an unexpected fixture directory.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
