#requires -Version 7.6
#requires -Modules @{ ModuleName = 'PSScriptAnalyzer'; RequiredVersion = '1.25.0' }, @{ ModuleName = 'powershell-yaml'; RequiredVersion = '0.4.12' }
<#
.SYNOPSIS
Checks repository PowerShell files and GitHub Actions commands with PSScriptAnalyzer.
.DESCRIPTION
Uses every default analyzer rule at every severity. Includes tracked files and
unignored new files, while excluding build artifacts through Git's ignore rules.
GitHub expressions are replaced with inert text for static analysis of run blocks.
.EXAMPLE
./tools/ci/analyze-powershell.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$repository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$settings = Join-Path $repository 'PSScriptAnalyzerSettings.psd1'

function Get-DefaultShell([System.Collections.IDictionary] $Definition) {
    $defaults = $Definition['defaults']
    if ($null -eq $defaults -or $null -eq $defaults['run']) { return $null }
    return $defaults['run']['shell']
}

function Get-RepositoryPath([string[]] $Pattern) {
    $paths = git -C $repository ls-files -z --cached --others --exclude-standard -- @Pattern
    # Rejoin native output lines before splitting records, preserving newlines in filenames.
    return ($paths -join "`n").Split([char]0, [System.StringSplitOptions]::RemoveEmptyEntries)
}

$scriptPaths = @(Get-RepositoryPath -Pattern ':(icase)*.ps1', ':(icase)*.psm1', ':(icase)*.psd1')
$workflowPaths = @(Get-RepositoryPath -Pattern '.github/workflows/*.yml', '.github/workflows/*.yaml')
$commandCount = 0
$diagnostics = @(
    foreach ($path in $scriptPaths) {
        Invoke-ScriptAnalyzer -Path (Join-Path $repository $path) -Settings $settings |
            Select-Object @{ Name = 'Source'; Expression = { $path } }, Line, RuleName, Message
    }
    foreach ($path in $workflowPaths) {
        $workflow = Get-Content -LiteralPath (Join-Path $repository $path) -Raw | ConvertFrom-Yaml
        foreach ($jobName in $workflow['jobs'].Keys) {
            $job = $workflow['jobs'][$jobName]
            # A matrix containing Windows can execute commands with the default pwsh shell.
            $canRunOnWindows = $job['runs-on'] -match 'windows' -or
                ($job['strategy'] | ConvertTo-Json -Depth 20 -Compress) -match 'windows'
            $defaultShell = (Get-DefaultShell -Definition $job) ?? (Get-DefaultShell -Definition $workflow)
            foreach ($step in $job['steps']) {
                if (-not $step.ContainsKey('run')) { continue }
                $shell = $step['shell'] ?? $defaultShell ?? ($canRunOnWindows ? 'pwsh' : 'bash')
                if ($shell.Contains('${{', [System.StringComparison]::Ordinal)) {
                    throw "$path ($jobName): declare an explicit shell so workflow commands can be analyzed."
                }
                if ($shell -match '^powershell(?:\s|$)') {
                    throw "$path ($jobName): use pwsh instead of Windows PowerShell."
                }
                if ($shell -notmatch '^pwsh(?:\s|$)') { continue }
                $commandCount++
                $source = "$path ($jobName / $($step['name'] ?? $step['id'] ?? "command $commandCount"))"
                $definition = [regex]::Replace($step['run'], '\$\{\{[\s\S]*?\}\}', 'GitHubExpression')
                Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $settings |
                    Select-Object @{ Name = 'Source'; Expression = { $source } }, Line, RuleName, Message
            }
        }
    }
)
if ($diagnostics.Count -gt 0) {
    $diagnostics | Format-Table -Wrap | Out-Host
    throw "PowerShell analysis failed with $($diagnostics.Count) diagnostic(s)."
}
Write-Output "PowerShell analysis passed: $($scriptPaths.Count) files and $commandCount workflow commands; no diagnostics."
