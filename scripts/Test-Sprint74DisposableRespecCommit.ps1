[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerRespecCommit()')
$end = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerGritResource()', $start)
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerRespecCommit') -and
        $common.Contains("'disposable-gunslinger-respec-commit'")
    'two-detached-entities' = $method.Contains('sourceEntity = new Kingmaker.UI.LevelUp.ChargenUnit') -and
        $method.Contains('replacementEntity = new Kingmaker.UI.LevelUp.ChargenUnit')
    'exact-respec-commit' = $method.Contains('"Respec", false') -and
        $method.Contains('commit.Invoke(respecController, null)')
    'source-and-replacement' = $method.Contains('sourceFighter == 1 && sourceGunslinger == 0') -and
        $method.Contains('replacementFighter == 0 && replacementGunslinger == 1')
    'facts-and-isolation' = $method.Contains('proficiencies && grit') -and
        $method.Contains('SameReferences(inventoryBefore')
    'starting-inventory-rollback' = $method.Contains('addedInventory.AddRange') -and
        $method.Contains('runtimePlayer.Inventory.Remove(startingItems[index], excess)') -and
        $method.Contains('gunslinger.StartingGold = originalStartingGold')
    'broad-callback-excluded' = -not $method.Contains('RespecCompanion(') -and
        -not $method.Contains('PrepareRespec(')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 74 respec commit tests failed: $($failed -join ', ')" }
Write-Host "Sprint 74 respec commit tests passed: $($checks.Count)"
