[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerComprehensiveAcceptance()')
$end = $runner.IndexOf('private void AppendAcceptanceSlice', $start)
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerComprehensiveAcceptance') -and
        $common.Contains("'disposable-gunslinger-comprehensive-acceptance'")
    'exact-slice-count' = $method.Contains('"30 qualified slices"')
    'progression' = $method.Contains('RunDisposableGunslingerLevelTwentyProgression')
    'class-integration' = $method.Contains('RunDisposableGunslingerLevelUpCommit') -and
        $method.Contains('RunDisposableGunslingerMulticlassCommit')
    'grit' = $method.Contains('RunDisposableGunslingerGritPersistence') -and
        $method.Contains('RunDisposableGunslingerGritRecovery')
    'firearms' = $method.Contains('RunDisposableProductionFirearmSwitching') -and
        $method.Contains('RunDisposableGunslingerGunTraining')
    'deeds' = $method.Contains('RunDisposableGunslingerDeadeye') -and
        $method.Contains('RunDisposableGunslingerStunningShot(true)')
    'bounded-exclusions' = -not $method.Contains('RunDisposableGunslingerStartlingShot') -and
        -not $method.Contains('RunDisposableGunslingerTargetingHead') -and
        -not $method.Contains('RunDisposableGunslingerRespecCommit')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 80 comprehensive tests failed: $($failed -join ', ')" }
Write-Host "Sprint 80 comprehensive tests passed: $($checks.Count)"
