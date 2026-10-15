param(
    [Parameter(Mandatory)][string] $PackagePath,
    [Parameter(Mandatory)][string] $ExpectedVersion,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $ExpectedRevision
)

$ErrorActionPreference = 'Stop'
if ([System.IO.Path]::GetFileName($PackagePath) -cne "DotJpegli.$ExpectedVersion.nupkg") {
    throw 'Package filename must match the requested DotJpegli version exactly.'
}

$package = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $PackagePath).Path)
try {
    $required = @(
        'DotJpegli.nuspec', 'README.md', 'LICENSE', 'LICENSE.jpegli', 'THIRD-PARTY-NOTICES.md',
        'lib/netstandard2.1/DotJpegli.dll', 'lib/net10.0/DotJpegli.dll', 'lib/net11.0/DotJpegli.dll'
    )
    foreach ($name in $required) {
        $entry = $package.GetEntry($name)
        if ($null -eq $entry -or $entry.Length -eq 0) { throw "Missing or empty package entry: $name" }
    }
    $assemblies = @($package.Entries | Where-Object { $_.FullName -like '*.dll' })
    if ($assemblies.Count -ne 3) { throw 'Expected exactly the three managed library assemblies.' }

    $reader = [System.IO.StreamReader]::new($package.GetEntry('DotJpegli.nuspec').Open())
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
}
finally {
    $package.Dispose()
}
