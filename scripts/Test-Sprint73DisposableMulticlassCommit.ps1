[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerMulticlassCommit') -and
        $common.Contains("'disposable-gunslinger-multiclass-commit'")
    'bard-seed' = $runner.Contains('new object[] { bard, false }') -and
        $runner.Contains('bardSeeded == 1')
    'native-commit' = $runner.Contains('commit.Invoke(multiclass, null)') -and
        $runner.Contains('committedGunslinger == 1 && callback')
    'level-one-facts' = $runner.Contains('proficiencies && grit')
    'starter-transaction' = $runner.Contains('multiclass-starter-inventory-transaction') -and
        $runner.Contains('pistolAfterCommit ==') -and $runner.Contains('powderDelta == 20') -and
        $runner.Contains('ballDelta == 20') -and $runner.Contains('kitDelta == 1')
    'archetype-starters' = $runner.Contains('Bard 1/Pistolero 1') -and
        $runner.Contains('Bard 1/Musket Master 1') -and
        $runner.Contains('multiclass-archetype-starters')
    'expanded-isolation' = $runner.Contains('SameReferences(remoteBefore') -and
        $runner.Contains('SameReferences(crossBefore') -and
        $runner.Contains('SameReferences(inventoryBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 73 multiclass commit tests failed: $($failed -join ', ')" }
Write-Host "Sprint 73 multiclass commit tests passed: $($checks.Count)"
