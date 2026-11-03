#requires -Version 7.6
<#
.SYNOPSIS
Verify reproducible Release assemblies and packages from two fresh checkouts.
.DESCRIPTION
Builds the current committed revision at two different path depths, compares
SHA-256 hashes, and retains copied artifacts and hashes outside the checkouts.
.PARAMETER OutputRoot
An absolute, nonexistent directory for this run's checkouts and evidence.
.EXAMPLE
./tools/verify-reproducibility.ps1
#>
[CmdletBinding()]
param(
    [ValidateNotNullOrWhiteSpace()]
    [string] $OutputRoot = (Join-Path ([System.IO.Path]::GetTempPath()) ('DotJpegli-repro-' + [guid]::NewGuid().ToString('N')))
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not [System.IO.Path]::IsPathFullyQualified($OutputRoot)) {
    throw 'OutputRoot must be an absolute path.'
}
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
if (Test-Path -LiteralPath $OutputRoot) {
    throw 'OutputRoot must not already exist; each comparison needs fresh outputs.'
}

$repository = git -C $PSScriptRoot rev-parse --show-toplevel
$revision = git -C $repository rev-parse HEAD

$null = [System.IO.Directory]::CreateDirectory($OutputRoot)
$worktrees = [System.Collections.Generic.List[string]]::new()
$outputs = [System.Collections.Generic.List[object]]::new()
try {
    foreach ($lane in @('first', 'different-depth/second')) {
        $checkout = Join-Path $OutputRoot "$lane/checkout"
        $artifacts = Join-Path $OutputRoot "$lane/artifacts"
        git -C $repository worktree add --detach $checkout $revision
        $worktrees.Add($checkout)
        $null = [System.IO.Directory]::CreateDirectory($artifacts)

        Push-Location -LiteralPath $checkout
        try {
            dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
            dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output $artifacts -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
            foreach ($framework in @('netstandard2.1', 'net10.0', 'net11.0')) {
                $assembly = Join-Path $checkout "src/DotJpegli/bin/Release/$framework/DotJpegli.dll"
                Copy-Item -LiteralPath $assembly -Destination (Join-Path $artifacts "DotJpegli.$framework.dll")
            }
        }
        finally {
            Pop-Location
        }

        $packages = @(Get-ChildItem -LiteralPath $artifacts -Filter '*.nupkg' -File)
        if ($packages.Count -ne 1) { throw 'Expected exactly one NuGet package.' }
        $outputs.Add(@(Get-ChildItem -LiteralPath $artifacts -File | Sort-Object Name))
    }

    if ($outputs[0].Count -ne 4 -or $outputs[1].Count -ne 4) {
        throw 'Expected all three target-framework assemblies and one package from each checkout.'
    }
    $hashes = for ($index = 0; $index -lt $outputs[0].Count; $index++) {
        $first = $outputs[0][$index]
        $second = $outputs[1][$index]
        if ($first.Name -cne $second.Name) { throw 'Artifact filenames differ.' }
        $firstHash = (Get-FileHash -LiteralPath $first.FullName -Algorithm SHA256).Hash
        $secondHash = (Get-FileHash -LiteralPath $second.FullName -Algorithm SHA256).Hash
        if ($firstHash -ne $secondHash) { throw "Artifact bytes differ: $($first.Name)." }
        [pscustomobject]@{ File = $first.Name; SHA256 = $firstHash }
    }
    $hashes | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputRoot 'hashes.json') -Encoding utf8NoBOM
    $hashes | Format-Table -AutoSize
    Write-Output "Reproducible revision: $revision"
    Write-Output "Evidence: $OutputRoot"
}
finally {
    foreach ($checkout in $worktrees) {
        # These paths belong to this run; retain copied artifacts outside the checkouts.
        $resolved = [System.IO.Path]::GetFullPath($checkout)
        $relative = [System.IO.Path]::GetRelativePath($OutputRoot, $resolved)
        if ([System.IO.Path]::IsPathRooted($relative) -or $relative -in '.', '..' -or
            $relative.StartsWith('..' + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::Ordinal)) {
            throw 'Refusing to remove a checkout outside OutputRoot.'
        }
        git -C $repository worktree remove --force $resolved
    }
}
