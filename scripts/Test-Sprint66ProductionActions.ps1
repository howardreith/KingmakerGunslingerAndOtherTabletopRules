[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$files = @(
    'Blueprints\ReloadTestMusketAbilityBlueprints.cs',
    'Blueprints\RepairTestMusketAbilityBlueprints.cs',
    'Blueprints\OverhaulTestMusketAbilityBlueprints.cs',
    'Reloading\ReloadTestMusketAbilityLogic.cs',
    'Reloading\ReloadTestMusketRuntime.cs',
    'Recovery\RepairTestMusketAbilityLogic.cs',
    'Recovery\RepairTestMusketRuntime.cs',
    'Recovery\OverhaulTestMusketAbilityLogic.cs',
    'Recovery\OverhaulTestMusketRuntime.cs'
)
$source = ($files | ForEach-Object {
    Get-Content -Raw -LiteralPath (Join-Path $root "src\KingmakerGunslinger\$_")
}) -join "`n"
$proficiency = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Blueprints\FirearmProficiencyBlueprints.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')

$checks = [ordered]@{
    'production-names' =
        $source.Contains('DisplayName = "Reload Firearm"') -and
        $source.Contains('DisplayName = "Repair Firearm"') -and
        $source.Contains('DisplayName = "Overhaul Firearm"')
    'no-player-facing-test-musket-literals' =
        -not $source.Contains('"Reload Test Musket') -and
        -not $source.Contains('"Repair Test Musket') -and
        -not $source.Contains('"Overhaul Test Musket') -and
        -not $source.Contains('equipped Test Musket')
    'stable-symbols-retained' =
        $source.Contains('Symbol = "KMG.Test.ReloadAbility"') -and
        $source.Contains('Symbol = "KMG.Test.RepairAbility"') -and
        $source.Contains('Symbol = "KMG.Test.OverhaulAbility"')
    'exact-three-ability-grant' =
        $proficiency.Contains('grant.Facts.Length != 3') -and
        $proficiency.Contains('ReferenceEquals(grant.Facts[0], reloadAbility)') -and
        $proficiency.Contains('ReferenceEquals(grant.Facts[1], overhaulAbility)') -and
        $proficiency.Contains('ReferenceEquals(grant.Facts[2], repairAbility)')
    'save-free-runtime-presentation-assertion' =
        $runner.Contains('"production-firearm-actions-presentation"') -and
        $runner.Contains('reload.Name == "Reload Firearm"') -and
        $runner.Contains('repair.Name == "Repair Firearm"')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 66 production-action tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 66 production-action tests passed: $($checks.Count)"
