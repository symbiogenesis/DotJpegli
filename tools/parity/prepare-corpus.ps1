#requires -Version 7.2
<#
.SYNOPSIS
Fetch hash-pinned Kodak RGB8 PNGs and losslessly write canonical P6 inputs.
.DESCRIPTION
No color management, gamma conversion, resizing or lossy re-encoding is applied.
Use -Source with an absolute directory to read existing PNGs instead of downloading.
Corpus pixels are never committed. -SelfTest uses synthetic images without network access.
#>
[CmdletBinding(DefaultParameterSetName = 'Prepare')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Prepare')][string] $Output,
    [Parameter(ParameterSetName = 'Prepare')][string] $Source,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not ('DotJpegli.Tools.CorpusPng' -as [type])) {
    Add-Type -Path (Join-Path $PSScriptRoot 'CorpusPng.cs')
}
if ($SelfTest) {
    if (-not ('DotJpegli.Tools.CorpusPngTests' -as [type])) {
        Add-Type -Path (Join-Path $PSScriptRoot '../tests/CorpusPngTests.cs')
    }
    $decoder = [DotJpegli.Tools.CorpusPng].GetMethod('ToPpm').CreateDelegate([System.Func[byte[], byte[]]])
    [DotJpegli.Tools.CorpusPngTests]::Run($decoder)
    Write-Output 'Corpus preparation self-test passed (all PNG filters and invalid-input checks).'
    return
}

foreach ($value in @($Output, $Source)) {
    if ($value -and -not [System.IO.Path]::IsPathFullyQualified($value)) {
        throw 'Output and source paths must be absolute.'
    }
}
if (Test-Path -LiteralPath $Output) { throw "Output already exists: $Output" }
[System.IO.Directory]::CreateDirectory($Output) | Out-Null
$manifestPath = Join-Path $PSScriptRoot 'corpus.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$client = if (-not $Source) { [System.Net.Http.HttpClient]::new() }
try {
    if ($client) { $client.Timeout = [TimeSpan]::FromSeconds(60) }
    foreach ($entry in $manifest.images) {
        $name = $entry.name
        [byte[]] $data = if ($Source) {
            [System.IO.File]::ReadAllBytes((Join-Path $Source $name))
        } else {
            $client.GetByteArrayAsync($manifest.baseUrl + $name).GetAwaiter().GetResult()
        }
        if ([Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($data)).ToLowerInvariant() -cne $entry.pngSha256) {
            throw "Source hash mismatch: $name"
        }
        [byte[]] $ppm = [DotJpegli.Tools.CorpusPng]::ToPpm($data)
        if ([Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($ppm)).ToLowerInvariant() -cne $entry.ppmSha256) {
            throw "Decoded pixel hash mismatch: $name"
        }
        [System.IO.File]::WriteAllBytes((Join-Path $Output ([System.IO.Path]::ChangeExtension($name, '.ppm'))), $ppm)
        Write-Output "Verified ${name}: $($entry.width)x$($entry.height)"
    }
    # Use the canonical LF manifest on Windows as well as Linux.
    [System.IO.File]::WriteAllText((Join-Path $Output 'corpus.json'),
        [System.IO.File]::ReadAllText($manifestPath).Replace("`r`n", "`n"), [System.Text.UTF8Encoding]::new($false))
} finally {
    if ($client) { $client.Dispose() }
}
