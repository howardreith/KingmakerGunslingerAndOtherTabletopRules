[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\FeatureModuleCatalog.ps1')
$runner = Join-Path $PSScriptRoot 'Invoke-KingmakerCompatibilityProfile.ps1'
$tokens = $null
$parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($runner, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) { throw 'Compatibility runner parse errors.' }
$guards = @($ast.FindAll({ param($node)
    $node -is [Management.Automation.Language.IfStatementAst] -and
    $node.Clauses[0].Item1.Extent.Text -ceq '$moduleScenario'
}, $true) | Sort-Object { $_.Extent.StartOffset })
if ($guards.Count -ne 2) { throw 'Exact compatibility module validation/settings seams changed.' }
$validation = [scriptblock]::Create(($guards[0].Clauses[0].Item2.Statements |
    ForEach-Object { $_.Extent.Text }) -join "`n")
$assignment = @($guards[1].FindAll({ param($node)
    $node -is [Management.Automation.Language.AssignmentStatementAst] -and
    $node.Left.Extent.Text -ceq '$settings'
}, $true))
if ($assignment.Count -ne 1) { throw 'Exact compatibility module settings assignment changed.' }
$makeSettings = [scriptblock]::Create($assignment[0].Extent.Text + "`nreturn `$settings")
$catalog = @(Get-KmgFeatureModuleCatalog)
if ($catalog.Count -ne 12) { throw 'Expected twelve canonical modules.' }
$checks = 0
foreach ($mask in 0..4095) {
    $Parameters = @{}
    for ($index = 0; $index -lt $catalog.Count; $index++) {
        $Parameters[$catalog[$index].RuntimeParameter] = [bool]($mask -band (1 -shl $index))
    }
    & $validation
    $settings = & $makeSettings
    if ($settings.Count -ne 13 -or $settings.schemaVersion -ne 11) { throw 'Wrong compatibility settings schema/count.' }
    foreach ($module in $catalog) {
        if ($settings[$module.JsonKey] -isnot [bool] -or
            $settings[$module.JsonKey] -ne $Parameters[$module.RuntimeParameter]) {
            throw "Compatibility settings lost explicit intent: $($module.JsonKey), mask $mask."
        }
    }
    $checks++
}
foreach ($module in $catalog) {
    foreach ($invalid in @('absent', 'string')) {
        $Parameters = @{}
        foreach ($entry in $catalog) { $Parameters[$entry.RuntimeParameter] = $true }
        if ($invalid -ceq 'absent') { $Parameters.Remove($module.RuntimeParameter) }
        else { $Parameters[$module.RuntimeParameter] = 'true' }
        $rejected = $false
        try { & $validation } catch { $rejected = $true }
        if (-not $rejected) { throw "Invalid module parameter accepted: $($module.RuntimeParameter), $invalid." }
        $checks++
    }
}
Write-Host "Compatibility module parameter/settings checks passed: $checks; no profile staged or game launched."
