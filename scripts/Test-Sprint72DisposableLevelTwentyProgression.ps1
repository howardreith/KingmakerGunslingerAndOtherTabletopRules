[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$methodStart = $runner.IndexOf(
    'private RuntimeTestResult RunDisposableGunslingerLevelTwentyProgression()')
$methodEnd = $runner.IndexOf(
    'private RuntimeTestResult RunDisposableGunslingerMulticlassPreview()', $methodStart)
$method = $runner.Substring($methodStart, $methodEnd - $methodStart)
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerLevelTwentyProgression') -and
        $common.Contains("'disposable-gunslinger-level-twenty-progression'")
    'twenty-native-applications' = $runner.Contains('for (int level = 1; level <= 20; level++)') -and
        $runner.Contains('apply.Invoke(controller, new object[] { descriptor })')
    'exact-build-modes' = $runner.Contains('level == 1 ? "CharGen" : "LevelUp"')
    'evaluated-chassis' = $runner.Contains('bab == 20 && fortitude == 12 && reflex == 12 && will == 6')
    'all-direct-facts' = $runner.Contains('!descriptor.HasFact(feature)') -and
        $runner.Contains('observedFacts == expectedFacts')
    'no-commit' = -not $method.Contains('commit.Invoke')
    'external-isolation' = $runner.Contains(
        'detached entity disposal and exact reference snapshots')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 72 level-twenty progression tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 72 level-twenty progression tests passed: $($checks.Count)"
