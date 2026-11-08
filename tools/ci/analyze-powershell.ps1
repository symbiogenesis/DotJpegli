#requires -Version 7.6
#requires -Modules @{ ModuleName = 'PSScriptAnalyzer'; RequiredVersion = '1.25.0' }, @{ ModuleName = 'powershell-yaml'; RequiredVersion = '0.4.12' }
<#
.SYNOPSIS
Checks repository PowerShell files and GitHub Actions commands with PSScriptAnalyzer.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$repository = (Resolve-Path -LiteralPath "$PSScriptRoot/../..").ProviderPath
$settings = @{ IncludeDefaultRules = $true; Severity = @('Error', 'Warning', 'Information') }

function Get-DefaultShell([Collections.IDictionary] $Map) {
    return $Map['defaults'] -and $Map['defaults']['run'] ? $Map['defaults']['run']['shell'] : $null
}

function Get-RepoPath([string[]] $Pattern) {
    return (@(git -C $repository ls-files -z --cached --others --exclude-standard -- @Pattern) -join "`n").
        Split([char]0, [StringSplitOptions]::RemoveEmptyEntries)
}

$scriptPaths = @(Get-RepoPath ':(icase)*.ps1', ':(icase)*.psm1', ':(icase)*.psd1')
$workflowPaths = @(Get-RepoPath '.github/workflows/*.yml', '.github/workflows/*.yaml')
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
            $canRunOnWindows = $job['runs-on'] -match 'windows' -or ($job['strategy'] | ConvertTo-Json -Depth 20 -Compress) -match 'windows'
            $defaultShell = (Get-DefaultShell $job) ?? (Get-DefaultShell $workflow)
            foreach ($step in $job['steps']) {
                if (-not $step.ContainsKey('run')) { continue }
                $shell = $step['shell'] ?? $defaultShell ?? ($canRunOnWindows ? 'pwsh' : 'bash')
                if ($shell.Contains('${{', [StringComparison]::Ordinal)) {
                    throw "$path ($jobName): declare an explicit shell so workflow commands can be analyzed."
                }
                if ($shell -match '^powershell(?:\s|$)') { throw "$path ($jobName): use pwsh instead of Windows PowerShell." }
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
