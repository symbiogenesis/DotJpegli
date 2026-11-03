#requires -Version 7.6
<#
.SYNOPSIS
Fetch hash-pinned Kodak RGB8 PNGs and losslessly write canonical P6 inputs.
.DESCRIPTION
No color management, gamma conversion, resizing or lossy re-encoding is applied.
Use -Source with an absolute directory to read existing PNGs instead of downloading.
Corpus pixels are never committed. -SelfTest uses synthetic images without network access.
.PARAMETER Output
Absolute path to a new directory for verified PPM files and the canonical manifest.
.PARAMETER Source
Absolute path to existing PNG inputs. Omit this parameter to download pinned inputs.
.PARAMETER SelfTest
Runs synthetic decoder checks without downloading or preparing the corpus.
.EXAMPLE
./tools/parity/prepare-corpus.ps1 -Output /tmp/parity-corpus
.EXAMPLE
./tools/parity/prepare-corpus.ps1 -SelfTest
#>
[CmdletBinding(DefaultParameterSetName = 'Prepare')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Prepare')][ValidateNotNullOrWhiteSpace()][string] $Output,
    [Parameter(ParameterSetName = 'Prepare')][ValidateNotNullOrWhiteSpace()][string] $Source,
    [Parameter(Mandatory, ParameterSetName = 'SelfTest')][switch] $SelfTest
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$commandArguments = [Collections.Generic.List[string]]::new()
$commandArguments.Add('corpus')
if ($SelfTest) {
    $commandArguments.Add('--self-test')
} else {
    $commandArguments.AddRange([string[]]@('--output', $Output, '--manifest', (Join-Path $PSScriptRoot 'corpus.json')))
    if ($Source) { $commandArguments.AddRange([string[]]@('--source', $Source)) }
}
& (Join-Path $PSScriptRoot '../Invoke-Automation.ps1') -CommandArguments $commandArguments.ToArray()
