#requires -Version 7.6
<#
.SYNOPSIS
Runs focused synthetic fixtures for the InferSharp findings gate.
.DESCRIPTION
Does not run InferSharp, the managed test suite, or benchmarks.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
$gate = Join-Path (Split-Path $PSScriptRoot -Parent) 'assert-infersharp.ps1'
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
function Get-ReportFixture([object[]] $Runs = @((Get-RunFixture))) { @{ version = '2.1.0'; runs = @($Runs) } }
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
    Write-Json $reportPath (Get-ReportFixture)
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

    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture), (Get-RunFixture)))
    Assert-Gate 'Clean report with multiple runs'

    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture)))))
    Assert-Gate 'Findings fail regardless of analyzer exit status' 'reported 1 unreviewed finding'

    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture), (Get-RunFixture @((Get-FindingFixture)))))
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
        Write-Json $reportPath (Get-ReportFixture @($invalidRun))
        Assert-Gate 'Invalid run, tool or results fails' 'Invalid InferSharp SARIF'
    }

    foreach ($field in 'ruleId', 'message', 'locations') {
        $finding = Get-FindingFixture
        $finding.Remove($field)
        Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @($finding))))
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
        Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @($finding))))
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
    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture)))))
    Assert-Gate 'Exact reviewed baseline identity succeeds' -Baseline
    Push-Location -LiteralPath $fixtureRoot
    try { Assert-Gate 'Relative baseline and source root resolve after Push-Location' -Baseline -Relative }
    finally { Pop-Location }
    [IO.File]::WriteAllText($sourcePath, $sourceText.Replace("`n", "`r`n"))
    Assert-Gate 'Source hashes normalize line endings' -Baseline

    $newFinding = Get-FindingFixture
    $newFinding.ruleId = 'RESOURCE_LEAK'
    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture), $newFinding))))
    Assert-Gate 'New finding alongside a reviewed finding fails' 'reported 1 unreviewed finding' -Baseline
    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture)))))

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
    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture)))))
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
    Write-Json $reportPath (Get-ReportFixture @((Get-RunFixture @((Get-FindingFixture), (Get-FindingFixture)))))
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
