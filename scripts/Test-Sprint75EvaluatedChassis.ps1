[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerEvaluatedChassis()')
$end = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerMulticlassPreview()', $start)
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerEvaluatedChassis') -and
        $common.Contains("'disposable-gunslinger-evaluated-chassis'")
    'exact-modes' = $method.Contains('level == 1 ? "CharGen" : "LevelUp"')
    'intelligence-fixed' = $method.Contains('descriptor.Stats.Intelligence.BaseValue = 10')
    'native-hit-points' = $method.Contains('hpLevelOne == 11 && hpLevelTwo == 18')
    'native-skill-points' = $method.Contains('skillsLevelOne == 4 &&') -and
        $method.Contains('skillsLevelTwo == 4')
    'no-commit' = -not $method.Contains('commit.Invoke')
    'external-isolation' = $method.Contains('detached entity disposal and exact reference snapshots')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 75 evaluated chassis tests failed: $($failed -join ', ')" }
Write-Host "Sprint 75 evaluated chassis tests passed: $($checks.Count)"
