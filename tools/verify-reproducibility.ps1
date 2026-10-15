param(
    [string] $OutputRoot = (Join-Path ([System.IO.Path]::GetTempPath()) ('DotJpegli-repro-' + [guid]::NewGuid().ToString('N')))
)

$ErrorActionPreference = 'Stop'
if (-not [System.IO.Path]::IsPathFullyQualified($OutputRoot)) {
    throw 'OutputRoot must be an absolute path.'
}
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
if (Test-Path -LiteralPath $OutputRoot) {
    throw 'OutputRoot must not already exist; each comparison needs fresh outputs.'
}

$repository = git -C $PSScriptRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0) { throw 'Cannot locate the repository.' }
$revision = git -C $repository rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve HEAD.' }

New-Item -ItemType Directory -Path $OutputRoot | Out-Null
$worktrees = @()
$outputs = @()
try {
    foreach ($lane in @('first', 'different-depth/second')) {
        $checkout = Join-Path $OutputRoot "$lane/checkout"
        $artifacts = Join-Path $OutputRoot "$lane/artifacts"
        git -C $repository worktree add --detach $checkout $revision
        if ($LASTEXITCODE -ne 0) { throw "Cannot create checkout $checkout." }
        $worktrees += $checkout
        New-Item -ItemType Directory -Path $artifacts | Out-Null

        Push-Location $checkout
        try {
            dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
            if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed.' }
            dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output $artifacts -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
            if ($LASTEXITCODE -ne 0) { throw 'Release pack failed.' }
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
        $outputs += ,@(Get-ChildItem -LiteralPath $artifacts -File | Sort-Object Name)
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
    $hashes | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputRoot 'hashes.json')
    $hashes | Format-Table -AutoSize
    Write-Output "Reproducible revision: $revision"
    Write-Output "Evidence: $OutputRoot"
}
finally {
    foreach ($checkout in $worktrees) {
        # These paths belong to this run; retain copied artifacts outside the checkouts.
        $resolved = [System.IO.Path]::GetFullPath($checkout)
        if (-not $resolved.StartsWith($OutputRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Refusing to remove a checkout outside OutputRoot.'
        }
        git -C $repository worktree remove --force $resolved
        if ($LASTEXITCODE -ne 0) { throw "Cannot remove checkout $resolved." }
    }
}
