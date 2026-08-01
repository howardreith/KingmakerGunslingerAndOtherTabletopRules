[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerLevelUpPreview') -and
        $common.Contains("'disposable-gunslinger-levelup-preview'")
    'exact-levelup-mode' = $runner.Contains('Enum.Parse(start.GetParameters()[4].ParameterType,') -and
        $runner.Contains('"LevelUp", false)')
    'isolated-level-one-seed' = $runner.Contains('applyLevelup.Invoke(seedController') -and
        $runner.Contains('initialLevel == 0 && seededLevel == 1')
    'same-class-preview' = $runner.Contains('previewBefore == 1 && previewAfter == 2') -and
        $runner.Contains('sourceAfter == 1 && queuedCount == 2')
    'cancel-both-controllers' = $runner.Contains('cancel.Invoke(levelController, null)') -and
        $runner.Contains('cancel.Invoke(seedController, null)')
    'external-isolation' = $runner.Contains('both controllers canceled and disposable entity disposed')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 34 disposable Gunslinger level-up preview tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 34 disposable Gunslinger level-up preview tests passed: $($checks.Count)"
