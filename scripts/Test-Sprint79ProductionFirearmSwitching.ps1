[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableProductionFirearmSwitching') -and
        $common.Contains("'disposable-production-firearm-switching'")
    'two-production-pistols' = $runner.Contains('first = new ItemEntityWeapon(pistol);') -and
        $runner.Contains('second = new ItemEntityWeapon(pistol);')
    'native-switch' = $runner.Contains('unit.Body.PrimaryHand.RemoveItem(false);') -and
        $runner.Contains('unit.Body.PrimaryHand.InsertItem(second);')
    'exact-first' = $runner.Contains('"first-identical-firearm-selected"')
    'exact-second' = $runner.Contains('"second-identical-firearm-selected"')
    'state-isolation' = $runner.Contains('"identical-firearm-state-isolation"')
    'ambiguity' = $runner.Contains('"dual-firearm-ambiguity-fails-closed"') -and
        $runner.Contains('unit.Body.SecondaryHand.InsertItem(second);')
    'external-isolation' = $runner.Contains('detached unit/items disposed and token state forgotten')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 79 production switching tests failed: $($failed -join ', ')" }
Write-Host "Sprint 79 production switching tests passed: $($checks.Count)"
