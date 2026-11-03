# Focused fixture tests for the CI findings gate; no analyzer, tests or benchmarks run.
$ErrorActionPreference = 'Stop'
$gate = Join-Path (Split-Path $PSScriptRoot -Parent) 'assert-infersharp.ps1'
$fixtureRoot = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('DotJpegli-infersharp-' + [guid]::NewGuid().ToString('N'))))
$null = New-Item -ItemType Directory -Path $fixtureRoot
$reportPath = Join-Path $fixtureRoot 'report.sarif'
$logPath = Join-Path $fixtureRoot 'translation.txt'
$baselinePath = Join-Path $fixtureRoot 'baseline.json'
$sourcePath = Join-Path $fixtureRoot 'src/Example.cs'
$null = New-Item -ItemType Directory -Path (Split-Path $sourcePath -Parent)
$sourceText = "class Example { }`n"
[IO.File]::WriteAllText($sourcePath, $sourceText)
$sourceHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sourceText))).ToLowerInvariant()
$coverage = @'
Method successfully translated: 20 (80%)
Method partially translated: 5 (20%)
Instructions translated: 100 (90%)
Instructions skipped: 10 (10%)
'@
$script:passed = 0

function Write-Json($Path, $Value) { [IO.File]::WriteAllText($Path, (ConvertTo-Json -InputObject $Value -Depth 20)) }
function New-Run($Results = @()) { @{ tool = @{ driver = @{ name = 'Infer' } }; results = @($Results) } }
function New-Report($Runs = @((New-Run))) { @{ version = '2.1.0'; runs = @($Runs) } }
function New-Finding {
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
function New-Baseline {
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
function Reset-Fixtures {
    Write-Json $reportPath (New-Report)
    [IO.File]::WriteAllText($logPath, $coverage)
    [IO.File]::WriteAllText($sourcePath, $sourceText)
    Write-Json $baselinePath (New-Baseline)
}
function Assert-Gate($Name, [string] $Failure = '', [switch] $Baseline) {
    $arguments = @{ ReportPath = $reportPath; TranslationLogPath = $logPath }
    if ($Baseline) {
        $arguments.BaselinePath = $baselinePath
        $arguments.TargetFramework = 'net11.0'
        $arguments.SourceRoot = $fixtureRoot
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
    Reset-Fixtures
    Assert-Gate 'Clean report with nonzero translation'

    Write-Json $reportPath (New-Report @((New-Run), (New-Run)))
    Assert-Gate 'Clean report with multiple runs'

    Write-Json $reportPath (New-Report @((New-Run @((New-Finding)))))
    Assert-Gate 'Findings fail regardless of analyzer exit status' 'reported 1 unreviewed finding'

    Write-Json $reportPath (New-Report @((New-Run), (New-Run @((New-Finding)))))
    Assert-Gate 'Findings in later SARIF runs fail' 'reported 1 unreviewed finding'

    Reset-Fixtures
    [IO.File]::Delete($reportPath)
    Assert-Gate 'Missing report fails' 'Missing InferSharp SARIF report'

    [IO.File]::WriteAllText($reportPath, '{ invalid JSON')
    Assert-Gate 'Malformed JSON fails' 'Invalid InferSharp SARIF'

    foreach ($invalid in @(
        @{ version = '1.0.0'; runs = @((New-Run)) },
        @{ version = '2.1.0'; runs = @() },
        @{ version = '2.1.0'; runs = (New-Run) },
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
        (New-Run @('invalid result'))
    )) {
        Write-Json $reportPath (New-Report @($invalidRun))
        Assert-Gate 'Invalid run, tool or results fails' 'Invalid InferSharp SARIF'
    }

    Reset-Fixtures
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

    Reset-Fixtures
    Write-Json $reportPath (New-Report @((New-Run @((New-Finding)))))
    Assert-Gate 'Exact reviewed baseline identity succeeds' -Baseline
    [IO.File]::WriteAllText($sourcePath, $sourceText.Replace("`n", "`r`n"))
    Assert-Gate 'Source hashes normalize line endings' -Baseline

    $newFinding = New-Finding
    $newFinding.ruleId = 'RESOURCE_LEAK'
    Write-Json $reportPath (New-Report @((New-Run @((New-Finding), $newFinding))))
    Assert-Gate 'New finding alongside a reviewed finding fails' 'reported 1 unreviewed finding' -Baseline
    Write-Json $reportPath (New-Report @((New-Run @((New-Finding)))))

    foreach ($field in 'ruleId', 'path', 'line', 'fingerprint') {
        $baseline = New-Baseline
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

    Reset-Fixtures
    Assert-Gate 'Stale baseline fails' 'Stale InferSharp baseline' -Baseline
    Write-Json $reportPath (New-Report @((New-Run @((New-Finding)))))
    [IO.File]::WriteAllText($sourcePath, "class Changed { }`n")
    Assert-Gate 'Changed reviewed source fails' 'reviewed source changed' -Baseline
    [IO.File]::Delete($sourcePath)
    Assert-Gate 'Missing reviewed source fails' 'reviewed source changed or is missing' -Baseline

    Reset-Fixtures
    $baseline = New-Baseline
    $baseline.targets['net11.0'] += $baseline.targets['net11.0'][0]
    Write-Json $baselinePath $baseline
    Assert-Gate 'Duplicate baseline exception fails' 'duplicate exception' -Baseline

    Reset-Fixtures
    Write-Json $reportPath (New-Report @((New-Run @((New-Finding), (New-Finding)))))
    Assert-Gate 'One baseline exception cannot hide duplicate findings' 'reported 1 unreviewed finding' -Baseline

    Reset-Fixtures
    $baseline = New-Baseline
    $baseline.targets['net11.0'][0].reason = ''
    Write-Json $baselinePath $baseline
    Assert-Gate 'Unexplained baseline exception fails' 'every exception needs an exact identity' -Baseline

    [IO.File]::Delete($baselinePath)
    Assert-Gate 'Missing baseline fails' 'Invalid InferSharp baseline' -Baseline
    [IO.File]::WriteAllText($baselinePath, '{ invalid JSON')
    Assert-Gate 'Malformed baseline fails' 'Invalid InferSharp baseline' -Baseline
    $baseline = New-Baseline
    $baseline.targets['net11.0'] = $null
    Write-Json $baselinePath $baseline
    Assert-Gate 'Baseline without selected framework fails' 'Invalid InferSharp baseline' -Baseline
    Write-Output "Passed $script:passed InferSharp gate checks."
}
finally {
    # Remove only this invocation's unique, absolute fixture directory.
    $resolved = (Resolve-Path -LiteralPath $fixtureRoot).Path
    if ([IO.Path]::GetFullPath($resolved) -cne $fixtureRoot) { throw 'Refusing to remove an unexpected fixture directory.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
