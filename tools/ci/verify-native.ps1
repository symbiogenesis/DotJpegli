#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Directory,
    [string] $Revision = '031a0077f5799a6041004267fc12b956c1f52a20'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$metadata = Get-Content -LiteralPath (Join-Path $Directory 'provenance.json') -Raw | ConvertFrom-Json
if ($metadata.sourceUnmodified -isnot [bool] -or -not $metadata.sourceUnmodified -or $metadata.revision -cne $Revision) {
    throw 'Native reference provenance does not match the unmodified pinned source.'
}
$hash = (Get-FileHash -LiteralPath (Join-Path $Directory 'jpegli-parity-native') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($hash -cne $metadata.executableSha256) { throw 'Native reference executable hash mismatch.' }
