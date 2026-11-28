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
if ($catalog.Count -ne 13) { throw 'Expected thirteen canonical modules.' }
$checks = 0
foreach ($mask in 0..8191) {
    $Parameters = @{}
    for ($index = 0; $index -lt $catalog.Count; $index++) {
        $Parameters[$catalog[$index].RuntimeParameter] = [bool]($mask -band (1 -shl $index))
    }
    & $validation
    $settings = & $makeSettings
    if ($settings.Count -ne 15 -or $settings.schemaVersion -ne 12) { throw 'Wrong compatibility settings schema/count.' }
    if ($settings['turn-undead'] -isnot [bool] -or $settings['turn-undead'] -ne ($mask -ne 0)) {
        throw "Compatibility settings Turn Undead must be OFF only in the all-off profile (mask $mask)."
    }
    # The module matrix's shared generator must write the identical document.
    $generated = New-KmgFeatureModuleSettingsDocument -RuntimeValues $Parameters
    if (($generated | ConvertTo-Json -Depth 4) -cne ($settings | ConvertTo-Json -Depth 4)) {
        throw "Matrix and compatibility settings generators disagree (mask $mask)."
    }
    foreach ($module in $catalog) {
        if ($settings[$module.JsonKey] -isnot [bool] -or
            $settings[$module.JsonKey] -ne $Parameters[$module.RuntimeParameter]) {
            throw "Compatibility settings lost explicit intent: $($module.JsonKey), mask $mask."
        }
    }
    $checks++
}
$rejectedEmpty = $false
try { [void](New-KmgFeatureModuleSettingsDocument -RuntimeValues @{}) } catch { $rejectedEmpty = $true }
if (-not $rejectedEmpty) { throw 'The shared generator accepted missing module values.' }
$checks++
# Independent turnUndead: an explicit value overrides the all-off rule in both writers.
foreach ($mask in @(0, 1, 4096, 8191)) {
    foreach ($explicit in @($true, $false)) {
        $Parameters = @{}
        for ($index = 0; $index -lt $catalog.Count; $index++) {
            $Parameters[$catalog[$index].RuntimeParameter] = [bool]($mask -band (1 -shl $index))
        }
        $Parameters['turnUndead'] = $explicit
        & $validation
        $settings = & $makeSettings
        if ($settings.Count -ne 15 -or $settings['turn-undead'] -isnot [bool] -or $settings['turn-undead'] -ne $explicit) {
            throw "Compatibility settings ignored the explicit turnUndead=$explicit (mask $mask)."
        }
        $generated = New-KmgFeatureModuleSettingsDocument -RuntimeValues $Parameters
        if (($generated | ConvertTo-Json -Depth 4) -cne ($settings | ConvertTo-Json -Depth 4)) {
            throw "Matrix and compatibility settings generators disagree on explicit turnUndead (mask $mask)."
        }
        $checks++
    }
}
$Parameters = @{}
foreach ($entry in $catalog) { $Parameters[$entry.RuntimeParameter] = $true }
$Parameters['turnUndead'] = 'true'
$rejectedTurn = $false
try { & $validation } catch { $rejectedTurn = $true }
if (-not $rejectedTurn) { throw 'A non-Boolean turnUndead was accepted.' }
$rejectedTurn = $false
try { [void](New-KmgFeatureModuleSettingsDocument -RuntimeValues $Parameters) } catch { $rejectedTurn = $true }
if (-not $rejectedTurn) { throw 'The shared generator accepted a non-Boolean turnUndead.' }
$checks++
# Generator-to-loader fixtures: the C# loader test (turn-undead.settings-key) loads these same files.
$fixtureRoot = Join-Path $PSScriptRoot '..\..\tests\fixtures\turn-undead\generated-profiles'
foreach ($case in @(
    @{ File = 'all-off.json'; On = @(); Turn = $null },
    @{ File = 'turn-undead-alone.json'; On = @(); Turn = $true },
    @{ File = 'gunslinger-turn-undead-off.json'; On = @('gunslinger'); Turn = $false })) {
    $values = @{}
    foreach ($module in $catalog) { $values[$module.RuntimeParameter] = [bool]($case.On -contains $module.RuntimeParameter) }
    if ($null -ne $case.Turn) { $values['turnUndead'] = [bool]$case.Turn }
    $generated = New-KmgFeatureModuleSettingsDocument -RuntimeValues $values
    $fixture = Get-Content -LiteralPath (Join-Path $fixtureRoot $case.File) -Raw | ConvertFrom-Json
    $fixtureKeys = @($fixture.PSObject.Properties | ForEach-Object { $_.Name })
    if (($fixtureKeys -join ',') -cne (@($generated.Keys) -join ',')) { throw "Generated keys differ from $($case.File)." }
    foreach ($key in $fixtureKeys) {
        if ($fixture.$key -ne $generated[$key]) { throw "Generated $key differs from $($case.File)." }
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
