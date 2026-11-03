#requires -Version 7.6
<#
.SYNOPSIS
Verifies release package contents/provenance or reproducibility across fresh checkouts.
.PARAMETER PackagePath
Literal path to the DotJpegli NuGet package to inspect.
.PARAMETER ExpectedVersion
Exact package version to require in the filename and manifest.
.PARAMETER ExpectedRevision
Full lowercase Git commit ID of the tested main revision.
.PARAMETER Reproducibility
Builds the committed revision at two path depths and compares assembly/package hashes.
.PARAMETER OutputRoot
An absolute, nonexistent directory for fresh checkouts and retained artifacts/hashes.
.PARAMETER SelfTest
Runs synthetic package and reproducibility checks without building the encoder.
.EXAMPLE
./tools/verify-release.ps1 -PackagePath ./DotJpegli.1.0.0.nupkg -ExpectedVersion 1.0.0 -ExpectedRevision ('a' * 40)
.EXAMPLE
./tools/verify-release.ps1 -Reproducibility
#>
[CmdletBinding(DefaultParameterSetName = 'Package')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Package')][ValidateNotNullOrWhiteSpace()][string] $PackagePath,
    [Parameter(Mandatory, ParameterSetName = 'Package')][ValidateNotNullOrWhiteSpace()][string] $ExpectedVersion,
    [Parameter(Mandatory, ParameterSetName = 'Package')][ValidatePattern('\A[0-9a-f]{40}\z', Options = 'None')][string] $ExpectedRevision,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest,
    [Parameter(Mandatory, ParameterSetName = 'Reproducibility')][switch] $Reproducibility,
    [Parameter(ParameterSetName = 'Reproducibility')][ValidateNotNullOrWhiteSpace()][string] $OutputRoot =
        (Join-Path ([IO.Path]::GetTempPath()) ('DotJpegli-repro-' + [guid]::NewGuid().ToString('N')))
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$frameworks = @('netstandard2.1', 'net10.0', 'net11.0')
if ($SelfTest) {
    & (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments @('release', '--self-test')
    return
}
if (-not $Reproducibility) {
    & (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments @(
        'release', '--package', $PackagePath, '--version', $ExpectedVersion, '--revision', $ExpectedRevision)
    return
}
if (-not [IO.Path]::IsPathFullyQualified($OutputRoot)) {
    throw 'OutputRoot must be an absolute path.'
}
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
if (Test-Path -LiteralPath $OutputRoot) {
    throw 'OutputRoot must not already exist; each comparison needs fresh outputs.'
}
$repository = git -C $PSScriptRoot rev-parse --show-toplevel
$revision = git -C $repository rev-parse HEAD
$null = [IO.Directory]::CreateDirectory($OutputRoot)
$worktrees = [Collections.Generic.List[string]]::new()
$outputs = [Collections.Generic.List[string]]::new()
try {
    foreach ($lane in @('first', 'different-depth/second')) {
        $checkout = Join-Path $OutputRoot "$lane/checkout"
        $artifacts = Join-Path $OutputRoot "$lane/artifacts"
        git -C $repository worktree add --detach $checkout $revision
        $worktrees.Add($checkout)
        $null = [IO.Directory]::CreateDirectory($artifacts)
        Push-Location -LiteralPath $checkout
        try {
            dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
            dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output $artifacts -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
            foreach ($framework in $frameworks) {
                $assembly = Join-Path $checkout "src/DotJpegli/bin/Release/$framework/DotJpegli.dll"
                Copy-Item -LiteralPath $assembly -Destination (Join-Path $artifacts "DotJpegli.$framework.dll")
            }
        } finally {
            Pop-Location
        }
        $outputs.Add($artifacts)
    }

    & (Join-Path $PSScriptRoot 'Invoke-Automation.ps1') -CommandArguments @(
        'release', '--first-artifacts', $outputs[0], '--second-artifacts', $outputs[1],
        '--hashes', (Join-Path $OutputRoot 'hashes.json'))
    Write-Output "Reproducible revision: $revision"
    Write-Output "Evidence: $OutputRoot"
} finally {
    foreach ($checkout in $worktrees) {
        # These paths belong to this run; retain copied artifacts outside the checkouts.
        $resolved = [IO.Path]::GetFullPath($checkout)
        $relative = [IO.Path]::GetRelativePath($OutputRoot, $resolved)
        if ([IO.Path]::IsPathRooted($relative) -or $relative -in '.', '..' -or
            $relative.StartsWith('..' + [IO.Path]::DirectorySeparatorChar, [StringComparison]::Ordinal)) {
            throw 'Refusing to remove a checkout outside OutputRoot.'
        }
        git -C $repository worktree remove --force $resolved
    }
}
