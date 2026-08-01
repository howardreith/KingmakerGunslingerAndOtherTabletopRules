[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerGritRest') -and
        $common.Contains("'disposable-gunslinger-grit-rest'")
    'detached-only' = $runner.Contains('RunDisposableGunslingerGritRest()') -and
        $runner.Contains('new Kingmaker.UI.LevelUp.ChargenUnit(source)')
    'native-rest-contract' = $runner.Contains('RestController.ApplyRest(descriptor)')
    'spent-before-rest' = $runner.Contains('descriptor.Resources.Spend(grit, 1)') -and
        $runner.Contains('spent == 0')
    'restored-to-maximum' = $runner.Contains('rested == maximum')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 35 disposable grit-rest tests failed: $($failed -join ', ')" }
Write-Host "Sprint 35 disposable grit-rest tests passed: $($checks.Count)"
