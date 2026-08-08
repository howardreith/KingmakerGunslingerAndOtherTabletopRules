[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$binder = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\GunslingerStartingFirearmOwnershipPatch.cs')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerCreationCommit()')
$end = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerRespecCommit()', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Creation commit method is unavailable.' }
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerCreationCommit') -and
        $common.Contains("'disposable-gunslinger-creation-commit'")
    'detached-player-unit' = $method.Contains('new Kingmaker.UI.LevelUp.ChargenUnit(') -and
        $method.Contains('BlueprintRoot.Instance.DefaultPlayerCharacter')
    'exact-chargen-commit' = $method.Contains('"CharGen", false') -and
        $method.Contains('commit.Invoke(controller, null)')
    'level-and-facts' = $method.Contains('committedLevel == 1') -and
        $method.Contains('proficiencies && grit')
    'starting-grant-rollback' = $method.Contains('addedInventory.AddRange') -and
        $method.Contains('runtimePlayer.Inventory.Remove(startingItems[index], excess)') -and
        $method.Contains('gunslinger.StartingGold = originalStartingGold')
    'external-isolation' = $method.Contains('SameReferences(partyBefore') -and
        $method.Contains('SameReferences(crossBefore') -and
        $method.Contains('SameReferences(inventoryBefore')
    'no-save-or-ui' = -not $method.Contains('SaveGame') -and
        -not $method.Contains('Game.Instance.SaveGame') -and
        -not $method.Contains('StartNewGame')
    'absent-detached-grant-safe' = $binder.Contains('if (addedFirearms.Length == 0) return;') -and
        $binder.Contains('if (addedFirearms.Length != 1 || !ReferenceEquals(')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 100 creation commit tests failed: $($failed -join ', ')" }
Write-Host "Sprint 100 creation commit tests passed: $($checks.Count)"
