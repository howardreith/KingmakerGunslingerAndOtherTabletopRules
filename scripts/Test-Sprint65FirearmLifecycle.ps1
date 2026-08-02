[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot `
    'RuntimeAutomation.Common.ps1')

$checks = [ordered]@{
    'scenario-allowlisted' =
        $catalog.Contains('ObserveFirearmItemLifecycleContracts') -and
        $catalog.Contains('"observe-firearm-item-lifecycle-contracts"')
    'scenario-save-free' =
        $common.Contains("'observe-firearm-item-lifecycle-contracts' = [pscustomobject]") -and
        $common.Contains("RequiresSaveName = `$false; PermittedSaveName = `$null")
    'exact-native-contracts' =
        $runner.Contains('value.Name == "Remove"') -and
        $runner.Contains('value.Name == "Extract"') -and
        $runner.Contains('value.Name == "CreateEntity"')
    'same-item-reconstruction' =
        $runner.Contains('"same-item-token-reconstruction"') -and
        $runner.Contains('applyEnchantments.Invoke(source, null);')
    'new-item-isolation' =
        $runner.Contains('"new-item-state-isolation"') -and
        $runner.Contains('createdTokens == 0')
    'removal-does-not-transfer' =
        $runner.Contains('"removed-item-state-does-not-transfer"') -and
        $runner.Contains('sourceTokensAfterRemove == 0')
    'duplicate-corruption-preserved' =
        $runner.Contains('"duplicate-token-corruption-fails-closed"') -and
        $runner.Contains('corruptTokensBefore == 2') -and
        $runner.Contains('corruptTokensAfter == 2')
    'detached-cleanup' =
        $runner.Contains('"lifecycle-observation-isolation"') -and
        $runner.Contains('created.Dispose();') -and
        $runner.Contains('corrupt.Dispose();')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 65 lifecycle tests failed: $($failed -join ', ')"
}

Write-Host "Sprint 65 lifecycle tests passed: $($checks.Count)"
