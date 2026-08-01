[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerGritResource') -and
        $common.Contains("'disposable-gunslinger-grit-resource'")
    'detached-only' = $runner.Contains('RunDisposableGunslingerGritResource()') -and
        $runner.Contains('new Kingmaker.UI.LevelUp.ChargenUnit(source)')
    'native-resource-path' = $runner.Contains('descriptor.Resources.Spend(grit, 1)') -and
        $runner.Contains('descriptor.Resources.Restore(grit, 1)')
    'no-level-refill' = $runner.Contains('currentAfterLevelUp == 0') -and
        $runner.Contains('gunslingerLevel == 2')
    'external-cleanup' = $runner.Contains('controllers canceled and disposable entity disposed')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 35 disposable grit tests failed: $($failed -join ', ')" }
Write-Host "Sprint 35 disposable grit tests passed: $($checks.Count)"
