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
$Invariant = [Globalization.CultureInfo]::InvariantCulture
$HostFields = @(
    'cpu', 'affinity', 'os', 'osArchitecture', 'processArchitecture',
    'runtimeProductVersion', 'encoderTargetFramework', 'explicitNet11Simd',
    'logicalProcessors', 'frequency', 'isHighResolution', 'isHardwareAccelerated',
    'vectorFloatWidth', 'vector128', 'vector256', 'vector512', 'serverGc',
    'gcLatencyMode', 'dotnetTieredCompilation', 'complusTieredCompilation',
    'instrumentation', 'githubRunnerOs', 'githubRunnerArchitecture'
)

function Assert-Valid([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-FieldPresence($Value, [string[]] $Names) {
    Assert-Valid ($Value -is [Collections.IDictionary]) 'Expected a JSON object'
    foreach ($name in $Names) {
        Assert-Valid ($Value.Contains($name)) "Missing evidence field: $name"
    }
}

function Test-Integer($Value) {
    return $Value -is [int] -or $Value -is [long] -or $Value -is [bigint]
}

function Assert-PositiveNumber($Value, [string] $Label) {
    $numeric = (Test-Integer $Value) -or $Value -is [double] -or $Value -is [decimal]
    Assert-Valid ($numeric -and [double]::IsFinite([double]$Value) -and $Value -gt 0) "$Label must be finite and positive"
}

function Assert-Sha256($Value, [string] $Label) {
    Assert-Valid ($Value -is [string] -and $Value -cmatch '\A[0-9a-f]{64}\z') "Invalid $Label SHA-256"
}

# Compare JSON values structurally, independent of object property ordering and
# with ordinal strings. PowerShell's usual equality coerces strings and booleans.
function Test-JsonEqual($Left, $Right) {
    if ($null -eq $Left -or $null -eq $Right) { return $null -eq $Left -and $null -eq $Right }
    if ($Left -is [Collections.IDictionary]) {
        if ($Right -isnot [Collections.IDictionary] -or $Left.Count -ne $Right.Count) { return $false }
        foreach ($key in $Left.Keys) {
            if (-not $Right.Contains($key) -or -not (Test-JsonEqual $Left[$key] $Right[$key])) { return $false }
        }
        return $true
    }
    if ($Left -is [array]) {
        if ($Right -isnot [array] -or $Left.Count -ne $Right.Count) { return $false }
        for ($i = 0; $i -lt $Left.Count; $i++) {
            if (-not (Test-JsonEqual $Left[$i] $Right[$i])) { return $false }
        }
        return $true
    }
    if ($Left -is [string]) { return $Right -is [string] -and $Left -ceq $Right }
    if ($Right -is [string] -or $Right -is [Collections.IDictionary] -or $Right -is [array]) { return $false }
    if ($Left -is [bool]) { return $Right -is [bool] -and $Left -eq $Right }
    if ($Right -is [bool]) { return $false }
    return $Left -eq $Right
}

function Get-CaseKey($Case) {
    Assert-FieldPresence $Case @('id', 'sampling', 'progressiveLevel', 'distance')
    Assert-Valid ($Case.id -is [string] -and $Case.id.Length -gt 0) 'Missing case image ID'
    Assert-Valid ((Test-Integer $Case.sampling) -and (Test-Integer $Case.progressiveLevel)) 'Invalid case sampling/progression'
    Assert-PositiveNumber $Case.distance 'Case distance'
    # Length-prefix the ID so punctuation cannot alias another case. Normalize
    # numeric distances, as JSON 1 and 1.0 identify the same case.
    return '{0}:{1}/{2}/{3}/{4}' -f $Case.id.Length, $Case.id, $Case.sampling, $Case.progressiveLevel, ([double]$Case.distance).ToString('R', $Invariant)
}

function Get-Index($Items, [scriptblock] $Key, [string] $Label) {
    Assert-Valid ($Items -is [array] -and $Items.Count -gt 0) "Missing $Label"
    $result = [Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    foreach ($item in $Items) {
        $identity = & $Key $item
        Assert-Valid (-not $result.ContainsKey($identity)) "Duplicate $Label"
        $result.Add($identity, $item)
    }
    return ,$result
}

function Test-SameKeySet($Left, $Right) {
    if ($Left.Count -ne $Right.Count) { return $false }
    foreach ($key in $Left.Keys) {
        if (-not $Right.ContainsKey($key)) { return $false }
    }
    return $true
}

function Get-AllocatedPerEncode($Case, [long] $BlockCount) {
    Assert-FieldPresence $Case @('iterations', 'blocks')
    Assert-Valid ((Test-Integer $Case.iterations) -and $Case.iterations -gt 0) 'Invalid encode iteration count'
    Assert-Valid ($Case.blocks -is [array] -and $Case.blocks.Count -eq $BlockCount) 'Allocation blocks do not match the evidence policy'
    $total, $correction = 0.0, 0.0
    foreach ($block in $Case.blocks) {
        Assert-FieldPresence $block @('allocatedManagedBytes')
        Assert-Valid ((Test-Integer $block.allocatedManagedBytes) -and $block.allocatedManagedBytes -ge 0) 'Allocated bytes must be nonnegative integers'
        # Compensate summation to limit rounding when allocation totals differ.
        $adjusted = ($block.allocatedManagedBytes / $Case.iterations) - $correction
        $next = $total + $adjusted
        $correction = ($next - $total) - $adjusted
        $total = $next
    }
    return $total / $BlockCount
}

function Compare-Evidence($Before, $After, [string] $BeforeRevision, [string] $AfterRevision) {
    foreach ($pair in @(@($Before, $BeforeRevision), @($After, $AfterRevision))) {
        $evidence, $revision = $pair
        Assert-FieldPresence $evidence @('run', 'summary', 'cases', 'sources')
        $run, $status = $evidence.run, $evidence.summary
        Assert-FieldPresence $status @('valid', 'mode', 'testedRevision', 'selectedCoverageComplete', 'cases')
        Assert-FieldPresence $run @('testedRevision', 'expectedCases', 'environment', 'policy', 'githubRunId', 'upstreamPin', 'managedEncoderSha256', 'managedRunnerSha256', 'nativeExecutableSha256', 'corpusManifestSha256')
        Assert-FieldPresence $run.environment $HostFields
        Assert-Valid ($status.valid -is [bool] -and $status.valid) 'Both comparisons require stable matched evidence'
        Assert-Valid ($status.mode -ceq 'diagnostic') 'Expected selected-case diagnostic evidence'
        Assert-Valid ($run.testedRevision -ceq $revision -and $status.testedRevision -ceq $revision) 'Tested revision mismatch'
        Assert-Valid ((Test-Integer $run.expectedCases) -and $run.expectedCases -gt 0) 'Invalid expected case count'
        Assert-Valid ($status.selectedCoverageComplete -is [bool] -and $status.selectedCoverageComplete -and (Test-JsonEqual $status.cases $run.expectedCases)) 'Incomplete selected-case evidence'
        Assert-Valid ($null -eq $run.environment.instrumentation) 'Instrumented timings cannot be used for this comparison'
        foreach ($field in 'managedEncoderSha256', 'managedRunnerSha256', 'nativeExecutableSha256', 'corpusManifestSha256') {
            Assert-Sha256 $run[$field] $field
        }
    }
    $left, $right = $Before.run, $After.run
    Assert-Valid (Test-JsonEqual $left.policy $right.policy) 'Evidence policy mismatch'
    Assert-FieldPresence $left.policy @('blocks')
    $blockCount = $left.policy.blocks
    Assert-Valid ((Test-Integer $blockCount) -and $blockCount -gt 0) 'Invalid policy block count'
    Assert-Valid ($left.githubRunId -and (Test-JsonEqual $left.githubRunId $right.githubRunId)) 'Comparisons must belong to the same CI run'
    foreach ($field in 'upstreamPin', 'nativeExecutableSha256', 'corpusManifestSha256') {
        Assert-Valid (Test-JsonEqual $left[$field] $right[$field]) "Reference/input mismatch: $field"
    }
    foreach ($field in $HostFields) {
        Assert-Valid (Test-JsonEqual $left.environment[$field] $right.environment[$field]) "Host/runtime mismatch: $field"
    }
    Assert-Valid ([string]$left.environment.affinity -cmatch '\A[0-9]+\z') 'Comparisons must be pinned to one CPU'

    $old = Get-Index -Items $Before.cases -Key { param($item) Get-CaseKey $item } -Label 'baseline cases'
    $new = Get-Index -Items $After.cases -Key { param($item) Get-CaseKey $item } -Label 'current cases'
    Assert-Valid ((Test-SameKeySet $old $new) -and $old.Count -eq $left.expectedCases -and $new.Count -eq $right.expectedCases) 'Selected-case mismatch'
    $sourceKey = { param($item) Assert-FieldPresence $item @('id'); return $item.id }
    $oldSources = Get-Index -Items $Before.sources -Key $sourceKey -Label 'baseline sources'
    $newSources = Get-Index -Items $After.sources -Key $sourceKey -Label 'current sources'
    $selectedSources = [Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    foreach ($case in $old.Values) { $selectedSources[$case.id] = $null }
    Assert-Valid ((Test-SameKeySet $oldSources $newSources) -and (Test-SameKeySet $oldSources $selectedSources)) 'Selected source-image mismatch'
    foreach ($imageId in $oldSources.Keys) {
        foreach ($source in $oldSources[$imageId], $newSources[$imageId]) {
            Assert-FieldPresence $source @('category', 'width', 'height', 'ppmSha256', 'pixelsSha256')
        }
        foreach ($field in 'category', 'width', 'height', 'ppmSha256', 'pixelsSha256') {
            Assert-Valid (Test-JsonEqual $oldSources[$imageId][$field] $newSources[$imageId][$field]) "Source-image mismatch: $imageId/$field"
        }
        foreach ($field in 'ppmSha256', 'pixelsSha256') { Assert-Sha256 $oldSources[$imageId][$field] "$imageId/$field" }
    }

    $comparisons = [Collections.Generic.List[object]]::new()
    # Sort by ordinal image ID, then numeric sampling/progression/distance.
    $ordered = [Collections.Generic.List[object]]::new()
    foreach ($case in $new.Values) { $ordered.Add($case) }
    $ordered.Sort([Comparison[object]]{
        param($a, $b)
        $order = [StringComparer]::Ordinal.Compare($a.id, $b.id)
        foreach ($field in 'sampling', 'progressiveLevel', 'distance') {
            if ($order -ne 0) { break }
            $order = ([double]$a[$field]).CompareTo([double]$b[$field])
        }
        return $order
    })
    foreach ($a in $ordered) {
        $key = Get-CaseKey $a
        $b = $old[$key]
        foreach ($case in $b, $a) {
            Assert-FieldPresence $case @('managedJpegSha256', 'nativeJpegSha256', 'managed', 'native')
            Assert-FieldPresence $case.managed @('median')
            Assert-FieldPresence $case.native @('median')
        }
        foreach ($field in 'managedJpegSha256', 'nativeJpegSha256') {
            Assert-Sha256 $b[$field] $field
            Assert-Valid (Test-JsonEqual $b[$field] $a[$field]) "JPEG bytes changed: $key/$field"
        }
        $bm, $am, $bn, $an = $b.managed.median, $a.managed.median, $b.native.median, $a.native.median
        Assert-PositiveNumber $bm 'Baseline managed median'
        Assert-PositiveNumber $am 'Current managed median'
        Assert-PositiveNumber $bn 'Baseline native median'
        Assert-PositiveNumber $an 'Current native median'
        $comparisons.Add([ordered]@{
            case = @($a.id, $a.sampling, $a.progressiveLevel, $a.distance)
            baselineManagedNs = $bm; currentManagedNs = $am
            managedRatio = $am / $bm; nativeRatio = $an / $bn
            baselineAllocatedBytesPerEncode = Get-AllocatedPerEncode $b $blockCount
            currentAllocatedBytesPerEncode = Get-AllocatedPerEncode $a $blockCount
        })
    }
    return [ordered]@{
        baselineRevision = $BeforeRevision; currentRevision = $AfterRevision
        githubRunId = $left.githubRunId; environment = $right.environment
        policy = $right.policy; comparisons = $comparisons.ToArray()
    }
}

function Format-Markdown($Result) {
    $environment = $Result.environment
    $report = [Text.StringBuilder]::new()
    [void]$report.Append("# Same-host encoder development comparison`n`n")
    [void]$report.Append(('Baseline `{0}`, current `{1}`; CPU: {2}; affinity: {3}; Vector128/256/512: {4}/{5}/{6}.' -f $Result.baselineRevision, $Result.currentRevision, $environment.cpu, $environment.affinity, $environment.vector128, $environment.vector256, $environment.vector512) + "`n`n")
    [void]$report.Append("Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.`n`n")
    [void]$report.Append("Allocated bytes are the mean per-block managed allocation total divided by encodes per block; pool capacity and peak working set are separate quantities.`n`n")
    [void]$report.Append("| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |`n| --- | ---: | ---: | ---: | ---: | ---: | ---: |`n")
    foreach ($item in $Result.comparisons) {
        $image, $sampling, $progression, $distance = $item.case
        [void]$report.Append([string]::Format($Invariant, "| {0}/{1}/p{2}/d{3:G} | {4:F3} | {5:F3} | {6:F6} | {7:F6} | {8:F1} | {9:F1} |`n", $image, $sampling, $progression, $distance, ($item.baselineManagedNs / 1e6), ($item.currentManagedNs / 1e6), $item.managedRatio, $item.nativeRatio, $item.baselineAllocatedBytesPerEncode, $item.currentAllocatedBytesPerEncode))
    }
    return $report.ToString()
}

function Read-Evidence([string] $Directory) {
    $fullDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Directory)
    $evidence = @{}
    foreach ($name in 'run', 'summary', 'cases', 'sources') {
        $evidence[$name] = ConvertFrom-Json -InputObject ([IO.File]::ReadAllText([IO.Path]::Combine($fullDirectory, "$name.json"))) -AsHashtable -NoEnumerate -DateKind String
    }
    return $evidence
}

function Save-Report($Result, [string] $Prefix, [string] $SummaryPath) {
    $report = Format-Markdown $Result
    $fullPrefix = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Prefix)
    [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($fullPrefix))
    [IO.File]::WriteAllText("$fullPrefix.md", $report, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText("$fullPrefix.json", (ConvertTo-Json -InputObject $Result -Depth 100) + "`n", [Text.UTF8Encoding]::new($false))
    if ($SummaryPath) {
        $fullSummaryPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($SummaryPath)
        [IO.File]::AppendAllText($fullSummaryPath, $report, [Text.UTF8Encoding]::new($false))
    }
    return $report
}

function Copy-Json($Value) {
    return ConvertFrom-Json -InputObject (ConvertTo-Json -InputObject $Value -Depth 100) -AsHashtable -NoEnumerate -DateKind String
}

function Invoke-SelfTest {
    foreach ($boolean in $true, $false) {
        Assert-Valid ((Test-JsonEqual $boolean $boolean) -and
            -not (Test-JsonEqual $boolean ([int]$boolean)) -and
            -not (Test-JsonEqual ([int]$boolean) $boolean)) 'Boolean/numeric JSON equality self-test failed'
    }
    $beforeRevision, $afterRevision = ('1' * 40), ('2' * 40)
    $environment = @{}
    foreach ($field in $HostFields) { $environment[$field] = $null }
    $environment.cpu = 'Synthetic CPU'; $environment.affinity = '0'
    $environment.vector128 = $true; $environment.vector256 = $true; $environment.vector512 = $false
    $before = @{
        run = @{ testedRevision = $beforeRevision; expectedCases = 1; policy = @{ blocks = 3 }
            githubRunId = 'synthetic'; upstreamPin = '3' * 40; environment = $environment
            recordedAt = '2026-11-02T00:00:00Z' }
        summary = @{ valid = $true; mode = 'diagnostic'; testedRevision = $beforeRevision; selectedCoverageComplete = $true; cases = 1 }
        sources = @(@{ id = 'synthetic'; category = 'controlled'; width = 8; height = 8; ppmSha256 = 'b' * 64; pixelsSha256 = 'c' * 64 })
        cases = @(@{ id = 'synthetic'; sampling = 420; progressiveLevel = 0; distance = 1
            iterations = 5; blocks = @(@{ allocatedManagedBytes = 2000 }, @{ allocatedManagedBytes = 2200 }, @{ allocatedManagedBytes = 2400 })
            managed = @{ median = 100 }; native = @{ median = 125 }; managedJpegSha256 = 'd' * 64; nativeJpegSha256 = 'e' * 64 })
    }
    foreach ($field in 'managedEncoderSha256', 'managedRunnerSha256', 'nativeExecutableSha256', 'corpusManifestSha256') { $before.run[$field] = 'a' * 64 }
    $after = Copy-Json $before
    $after.run.testedRevision = $after.summary.testedRevision = $afterRevision
    $after.cases[0].iterations = 10; $after.cases[0].managed.median = 80
    $after.cases[0].blocks = @(@{ allocatedManagedBytes = 3000 }, @{ allocatedManagedBytes = 3300 }, @{ allocatedManagedBytes = 3600 })
    $result = Compare-Evidence -Before $before -After $after -BeforeRevision $beforeRevision -AfterRevision $afterRevision
    $item = $result.comparisons[0]
    Assert-Valid ($item.managedRatio -eq 0.8 -and $item.nativeRatio -eq 1 -and $item.baselineAllocatedBytesPerEncode -eq 440 -and $item.currentAllocatedBytesPerEncode -eq 330) 'Allocation/ratio normalization self-test failed'
    $mutations = @(
        { param($e) $e.summary.valid = $false },
        { param($e) $e.summary.selectedCoverageComplete = $false },
        { param($e) $e.run.policy.blocks = 4 },
        { param($e) $e.run.testedRevision = '0' * 40 },
        { param($e) $e.run.nativeExecutableSha256 = 'f' * 64 },
        { param($e) $e.run.corpusManifestSha256 = 'f' * 64 },
        { param($e) $e.run.githubRunId = 'different' },
        { param($e) $e.run.environment.instrumentation = 'profiler' },
        { param($e) $e.sources[0].pixelsSha256 = 'f' * 64 },
        { param($e) $e.cases[0].managedJpegSha256 = 'f' * 64 },
        { param($e) $e.cases[0].nativeJpegSha256 = 'f' * 64 },
        { param($e) $e.cases += @(Copy-Json $e.cases[0]) },
        { param($e) $e.cases[0].iterations = 0 },
        { param($e) $e.cases[0].blocks[0].allocatedManagedBytes = -1 },
        { param($e) $e.run.environment.vector128 = -not $e.run.environment.vector128 },
        { param($e) $e.run.environment.vector128 = 1 },
        { param($e) $e.run.environment.vector256 = -not $e.run.environment.vector256 },
        { param($e) $e.run.environment.vector512 = -not $e.run.environment.vector512 },
        { param($e) $e.run.environment.affinity = '0-1' },
        { param($e) $e.run.managedEncoderSha256 = 'A' * 64 },
        { param($e) $e.cases[0].distance = $true },
        { param($e) $e.cases[0].sampling = 420.0 },
        { param($e) $e.cases[0].managed.median = '80' },
        { param($e) $e.cases[0].managed.median = [double]::NaN },
        { param($e) $e.cases[0].managed.median = [double]::PositiveInfinity },
        { param($e) $e.cases[0].blocks[0].allocatedManagedBytes = $false },
        { param($e) $e.cases[0].blocks = @($e.cases[0].blocks[0]) },
        { param($e) $e.summary.valid = 1 },
        { param($e) $e.sources[0].id = 'Synthetic' },
        { param($e) $e.sources += @(Copy-Json $e.sources[0]) },
        { param($e) $e.run.environment.Remove('cpu') }
    )
    foreach ($mutate in $mutations) {
        $invalid = Copy-Json $after
        & $mutate $invalid
        $rejected = $false
        try { $null = Compare-Evidence -Before $before -After $invalid -BeforeRevision $beforeRevision -AfterRevision $afterRevision }
        catch { $rejected = $true }
        Assert-Valid $rejected 'Mismatch rejection self-test failed'
    }
    # Numeric case-key equality and policy property order must survive JSON ports.
    $equivalent = Copy-Json $after
    $equivalent.cases[0].distance = 1.0
    $before.run.policy.warmups = $equivalent.run.policy.warmups = 5
    $equivalent.run.policy = [ordered]@{ warmups = 5; blocks = 3 }
    $null = Compare-Evidence -Before $before -After $equivalent -BeforeRevision $beforeRevision -AfterRevision $afterRevision

    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    $testRoot = [IO.Path]::Combine($tempRoot, 'jpegli-comparator-' + [guid]::NewGuid().ToString('N') + '[synthetic]')
    $previousCulture = [Globalization.CultureInfo]::CurrentCulture
    try {
        [void][IO.Directory]::CreateDirectory($testRoot)
        foreach ($name in 'run', 'summary', 'cases', 'sources') {
            [IO.File]::WriteAllText([IO.Path]::Combine($testRoot, "$name.json"), (ConvertTo-Json -InputObject $after[$name] -Depth 100), [Text.UTF8Encoding]::new($true))
        }
        [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo('de-DE')
        $summaryPath = [IO.Path]::Combine($testRoot, 'summary.md')
        [IO.File]::WriteAllText($summaryPath, "Existing summary`n")
        $prefix = [IO.Path]::Combine($testRoot, 'reports', 'comparison')
        Push-Location -LiteralPath $testRoot
        try {
            $loaded = Read-Evidence '.'
            Assert-Valid (Test-JsonEqual $loaded $after) 'BOM/array/timestamp evidence reading self-test failed'
            $report = Save-Report -Result $result -Prefix './reports/comparison' -SummaryPath './summary.md'
        }
        finally {
            Pop-Location
        }
        Assert-Valid ($report.Contains('0.800000') -and $report.Contains('440.0')) 'Invariant report number formatting self-test failed'
        Assert-Valid ([IO.File]::ReadAllText($summaryPath) -ceq "Existing summary`n$report") 'Summary append self-test failed'
        $roundTrip = ConvertFrom-Json -InputObject ([IO.File]::ReadAllText("$prefix.json")) -AsHashtable -NoEnumerate -DateKind String
        Assert-Valid (Test-JsonEqual $result $roundTrip) 'Report JSON schema self-test failed'
    }
    finally {
        [Globalization.CultureInfo]::CurrentCulture = $previousCulture
        if ([IO.Directory]::Exists($testRoot)) {
            $resolved = [IO.Path]::GetFullPath($testRoot)
            Assert-Valid ([IO.Path]::GetDirectoryName($resolved).Equals($tempRoot.TrimEnd([IO.Path]::DirectorySeparatorChar), [StringComparison]::OrdinalIgnoreCase) -and [IO.Path]::GetFileName($resolved).StartsWith('jpegli-comparator-', [StringComparison]::Ordinal)) 'Refusing to remove an unexpected self-test directory'
            [IO.Directory]::Delete($resolved, $true)
        }
    }
    Write-Output "Performance revision comparator self-test passed ($($mutations.Count) rejected mismatches; synthetic data only)."
}

try {
    if ($SelfTest) { Invoke-SelfTest; return }
    $result = Compare-Evidence -Before (Read-Evidence $Baseline) -After (Read-Evidence $Current) -BeforeRevision $BaselineRevision -AfterRevision $CurrentRevision
    Write-Output (Save-Report -Result $result -Prefix $Output -SummaryPath $Summary)
}
catch {
    throw [InvalidOperationException]::new("Performance revision comparison invalid: $($_.Exception.Message)", $_.Exception)
}
