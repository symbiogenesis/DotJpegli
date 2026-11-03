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
    [Parameter(Mandatory, ParameterSetName = 'Reproducibility')][switch] $Reproducibility,
    [Parameter(ParameterSetName = 'Reproducibility')][ValidateNotNullOrWhiteSpace()][string] $OutputRoot =
        (Join-Path ([IO.Path]::GetTempPath()) ('DotJpegli-repro-' + [guid]::NewGuid().ToString('N')))
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$frameworks = @('netstandard2.1', 'net10.0', 'net11.0')
if (-not $Reproducibility) {
    if ([IO.Path]::GetFileName($PackagePath) -cne "DotJpegli.$ExpectedVersion.nupkg") {
        throw 'Package filename must match the requested DotJpegli version exactly.'
    }
    $package = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $PackagePath).ProviderPath)
    try {
        $required = @('DotJpegli.nuspec', 'README.md', 'LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md') +
            @($frameworks | ForEach-Object { "lib/$_/DotJpegli.dll" })
        foreach ($name in $required) {
            $entry = $package.GetEntry($name)
            if ($null -eq $entry -or $entry.Length -eq 0) { throw "Missing or empty package entry: $name" }
        }
        $assemblies = @($package.Entries | Where-Object { $_.FullName -like '*.dll' })
        if ($assemblies.Count -ne $frameworks.Count) { throw 'Expected exactly the three managed library assemblies.' }

        $reader = [IO.StreamReader]::new($package.GetEntry('DotJpegli.nuspec').Open())
        try { [xml]$manifest = $reader.ReadToEnd() }
        finally { $reader.Dispose() }
        $metadata = $manifest.package.metadata
        if ($metadata.id -cne 'DotJpegli' -or $metadata.version -cne $ExpectedVersion) {
            throw 'Package ID/version does not match the requested release.'
        }
        if ($metadata.repository.type -cne 'git' -or
            $metadata.repository.url -cne 'https://github.com/symbiogenesis/DotJpegli' -or
            $metadata.repository.branch -cne 'refs/heads/main' -or
            $metadata.repository.commit -cne $ExpectedRevision) {
            throw 'Package repository metadata does not match the tested main revision.'
        }
        if ($metadata.license.type -cne 'expression' -or $metadata.license.InnerText -cne 'BSD-3-Clause' -or
            $metadata.readme -cne 'README.md') {
            throw 'Package license or readme metadata is incorrect.'
        }
        if ($metadata.SelectNodes('.//*[local-name()="dependency"]').Count -ne 0) {
            throw 'The encoder package must have no runtime NuGet dependencies.'
        }
        Write-Output "Verified DotJpegli $ExpectedVersion from $ExpectedRevision (all three targets, licenses, readme, no runtime dependencies)."
    } finally {
        $package.Dispose()
    }
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
$outputs = [Collections.Generic.List[object]]::new()
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
        $packages = @(Get-ChildItem -LiteralPath $artifacts -Filter '*.nupkg' -File)
        if ($packages.Count -ne 1) { throw 'Expected exactly one NuGet package.' }
        $outputs.Add(@(Get-ChildItem -LiteralPath $artifacts -File | Sort-Object Name))
    }

    if ($outputs[0].Count -ne ($frameworks.Count + 1) -or $outputs[1].Count -ne ($frameworks.Count + 1)) {
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
