[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$catalog = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$orchestrator = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('ObserveClassBlueprintContracts') -and
        $catalog.Contains('"observe-class-blueprint-contracts"')
    'no-save-required' = $common.Contains("'observe-class-blueprint-contracts' = [pscustomobject]") -and
        $common.Contains('RequiresSaveName = $false')
    'reads-exact-root-array' = $runner.Contains('root.Progression.CharacterClasses')
    'records-exact-blueprint-identities' = $runner.Contains('characterClass.AssetGuid') -and
        $runner.Contains('characterClass.Progression.AssetGuid')
    'records-mechanical-progressions' = $runner.Contains('characterClass.BaseAttackBonus.AssetGuid') -and
        $runner.Contains('characterClass.WillSave.AssetGuid')
    'does-not-register' = -not $runner.Substring(
        $runner.IndexOf('private RuntimeTestResult RunClassBlueprintContractObservation'),
        $runner.IndexOf('private void Complete(', $runner.IndexOf(
            'private RuntimeTestResult RunClassBlueprintContractObservation')) -
        $runner.IndexOf('private RuntimeTestResult RunClassBlueprintContractObservation')).Contains(
            'BlueprintRegistry.Register')
    'steam-launch-only' = $orchestrator.Contains('[int]$SteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 class-contract observation tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 class-contract observation tests passed: $($checks.Count)"
