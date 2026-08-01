[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerMulticlassPreview') -and
        $common.Contains("'disposable-gunslinger-multiclass-preview'")
    'exact-fighter-source' = $runner.Contains('48ac8db94d5de7645906c7d0ad3bcfbd')
    'fighter-level-one-seed' = $runner.Contains('new object[] { fighter, false }') -and
        $runner.Contains('fighterBefore == 0 && fighterSeeded == 1')
    'gunslinger-multiclass-preview' = $runner.Contains('previewFighter == 1') -and
        $runner.Contains('previewGunslingerAfter == 1') -and
        $runner.Contains('sourceFighterAfter == 1 && sourceGunslingerAfter == 0')
    'exact-actions' = $runner.Contains('queuedCount == 2')
    'external-isolation' = $runner.Contains('Exact isolated multiclass preview is unavailable.') -and
        $runner.Contains('both controllers canceled and disposable entity disposed')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 34 disposable Gunslinger multiclass preview tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 34 disposable Gunslinger multiclass preview tests passed: $($checks.Count)"
