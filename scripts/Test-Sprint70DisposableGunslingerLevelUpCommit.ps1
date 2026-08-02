[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$entry = Get-Content -Raw -LiteralPath (Join-Path $root 'planning\SPRINT-70-ENTRY-CRITERIA.md')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerLevelUpCommit') -and
        $common.Contains("'disposable-gunslinger-levelup-commit'")
    'exact-levelup-mode' = $runner.Contains('RunDisposableGunslingerLevelUpCommit()') -and
        $runner.Contains('"LevelUp", false)')
    'native-commit' = $runner.Contains('commit.Invoke(commitController, null)') -and
        $runner.Contains('committedLevel == 2')
    'success-callback' = $runner.Contains('Action onSuccess = () => successCallback = true') -and
        $runner.Contains('new object[] { descriptor, false, null, onSuccess, levelUp }') -and
        $runner.Contains('"commit-success-callback"')
    'expanded-isolation' = $runner.Contains('SameReferences(remoteBefore') -and
        $runner.Contains('SameReferences(crossSceneBefore') -and
        $runner.Contains('SameReferences(inventoryBefore')
    'first-level-commit-prohibited' = $entry.Contains(
        'Do not invoke first-level `LevelUpController.Commit`')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 70 disposable level-up commit tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 70 disposable level-up commit tests passed: $($checks.Count)"
