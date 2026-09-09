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
# Execute only the exact read-only guard, never the settings transaction or launcher.
$matrixPath = Join-Path $PSScriptRoot '..\Invoke-FeatureModuleRuntimeMatrix.ps1'
$matrixAst = [Management.Automation.Language.Parser]::ParseFile($matrixPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) { throw 'Module matrix runner parse errors.' }
$disabledGuards = @($matrixAst.FindAll({ param($node)
    $node -is [Management.Automation.Language.IfStatementAst] -and
    $node.Clauses[0].Item1.Extent.Text.Contains("`$Scenario -ceq 'disposable-teleportation-disabled'") -and
    $node.Extent.Text.Contains('Disabled world-map qualification requires')
}, $true))
if ($disabledGuards.Count -ne 1) { throw 'Exact disabled world-map launcher guard changed.' }
$disabledGuard = [scriptblock]::Create($disabledGuards[0].Extent.Text)
$off = [pscustomobject]@{ Values = @{ teleportationSpells = $false } }
$on = [pscustomobject]@{ Values = @{ teleportationSpells = $true } }
$cases = @(
    @{ Name = 'existing-observation'; Scenario = 'observe-feature-module-settings'; Boundary = $true; Exit = $false; Entries = @($on, $off); Reject = $false },
    @{ Name = 'disabled-single-off'; Scenario = 'disposable-teleportation-disabled'; Boundary = $false; Exit = $true; Entries = @($off); Reject = $false },
    @{ Name = 'disabled-on'; Scenario = 'disposable-teleportation-disabled'; Boundary = $false; Exit = $true; Entries = @($on); Reject = $true },
    @{ Name = 'disabled-boundary'; Scenario = 'disposable-teleportation-disabled'; Boundary = $true; Exit = $true; Entries = @($off); Reject = $true },
    @{ Name = 'disabled-no-exit'; Scenario = 'disposable-teleportation-disabled'; Boundary = $false; Exit = $false; Entries = @($off); Reject = $true },
    @{ Name = 'disabled-empty'; Scenario = 'disposable-teleportation-disabled'; Boundary = $false; Exit = $true; Entries = @(); Reject = $true },
    @{ Name = 'disabled-multiple'; Scenario = 'disposable-teleportation-disabled'; Boundary = $false; Exit = $true; Entries = @($off, $off); Reject = $true }
)
foreach ($case in $cases) {
    $Scenario = $case.Scenario; $boundaryRequested = $case.Boundary
    $ExitAfterCompletion = $case.Exit; $combinations = $case.Entries
    $rejected = $false
    try { & $disabledGuard } catch {
        if ($_.Exception.Message -cne 'Disabled world-map qualification requires one Teleportation-OFF configuration and automatic exit.') { throw }
        $rejected = $true
    }
    if ($rejected -ne $case.Reject) { throw "Disabled world-map guard failed: $($case.Name)." }
    $checks++
}
$argumentGuards = @($matrixAst.FindAll({ param($node)
    $node -is [Management.Automation.Language.IfStatementAst] -and
    $node.Clauses[0].Item1.Extent.Text -ceq "`$Scenario -ceq 'disposable-teleportation-disabled'"
}, $true))
if ($argumentGuards.Count -ne 1) { throw 'Exact disabled runtime arguments seam changed.' }
$setDisabledArguments = [scriptblock]::Create($argumentGuards[0].Extent.Text)
foreach ($Scenario in @('observe-feature-module-settings', 'disposable-teleportation-disabled')) {
    $invokeArguments = @{ Parameters = @{ teleportationSpells = $false } }
    & $setDisabledArguments
    if ($Scenario -ceq 'disposable-teleportation-disabled') {
        if ($invokeArguments.Parameters.Count -ne 0 -or $invokeArguments.SaveName -cne 'KMG_AUTOMATION_WORKING') {
            throw 'Disabled map launcher must use only the strictly typed working-save argument.'
        }
    } elseif ($invokeArguments.ContainsKey('SaveName') -or $invokeArguments.Parameters.Count -ne 1) {
        throw 'Existing settings observation arguments changed.'
    }
    $checks++
}
Write-Host "Compatibility module parameter/settings checks passed: $checks; no profile staged or game launched."
