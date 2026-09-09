[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'FeatureModuleCatalog.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
$tokens = $null; $errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Invoke-TeleportationHardeningQualification.ps1'), [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Hardening orchestrator syntax failed.' }
$assignment = @($ast.FindAll({ param($node)
    $node -is [Management.Automation.Language.AssignmentStatementAst] -and
    $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and $node.Left.VariablePath.UserPath -ceq 'steps'
}, $true))
if ($assignment.Count -ne 1) { throw 'Expected one exact native qualification plan.' }
$checks = 0
foreach ($Scope in @('Coexistence', 'Native', 'Boundary')) {
    . ([scriptblock]::Create($assignment[0].Extent.Text))
    $expected = switch ($Scope) { Coexistence { 4 } Native { 11 } Boundary { 26 } }
    if (@($steps).Count -ne $expected) { throw 'Qualification plan count differs.' }
    $checks++
    if ($Scope -ceq 'Coexistence') {
        foreach ($scenario in @('disposable-teleportation-coexistence', 'disposable-teleportation-coexistence-gamepad')) {
            $states = @($steps | Where-Object scenario -CEQ $scenario)
            if ($states.Count -ne 2 -or @($states | Where-Object off).Count -ne 1) { throw 'Each real controller must run ON and OFF.' }
            $checks++
        }
    } elseif ($Scope -ceq 'Native') {
        $off = @($steps | Where-Object off)
        if ($off.Count -ne 1 -or $off[0].scenario -cne 'disposable-teleportation-disabled' -or
            @($steps | Where-Object scenario -CEQ 'working-save-smoke').Count -ne 1 -or
            @($steps.scenario | Select-Object -Unique).Count -ne 11) { throw 'Native scope lost a unique required scenario or changed unrelated module state.' }
        $checks++
    } else {
        if (@($steps.name | Select-Object -Unique).Count -ne 26) { throw 'Boundary matrix contains duplicate settings states.' }
        foreach ($step in $steps) {
            if ($step.scenario -cne 'observe-feature-module-settings' -or $step.configuration.schemaVersion -ne 11 -or
                @($step.configuration.PSObject.Properties).Count -ne 13 -or $step.parameters.Count -ne 12) { throw 'Boundary settings/request schemas differ.' }
            foreach ($module in @(Get-KmgFeatureModuleCatalog)) {
                if ($step.parameters[$module.RuntimeParameter] -isnot [bool] -or
                    $step.configuration.PSObject.Properties[$module.JsonKey].Value -cne $step.parameters[$module.RuntimeParameter]) {
                    throw 'A written module setting differs from its native assertion request.'
                }
            }
        }
        $checks++
    }
}
$original = '{"schemaVersion":11,"gunslinger":false,"teleportation-spells":true}' | ConvertFrom-Json
$before = $original | ConvertTo-Json -Compress
$parameters = Get-KmgOriginalModuleRuntimeParameters -Settings $original
if ($parameters.Count -ne 12 -or $parameters.gunslinger -ne $false -or $parameters.teleportationSpells -ne $true -or
    $parameters.shieldOther -ne $true -or ($original | ConvertTo-Json -Compress) -cne $before) { throw 'Original settings mapping changed OFF/defaults or mutated the captured document.' }
$checks++
$rejected = $false
try { [void](Get-KmgOriginalModuleRuntimeParameters -Settings ('{"gunslinger":"false"}' | ConvertFrom-Json)) } catch { $rejected = $true }
if (-not $rejected) { throw 'Malformed original settings reached the restoration request.' }
$checks++
Write-Host "Hardening controller/native/boundary plan checks passed: $checks; no settings, saves or game process touched."
