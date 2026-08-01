[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerRespecPreview') -and
        $common.Contains("'disposable-gunslinger-respec-preview'")
    'exact-respec-mode' = $runner.Contains('"Respec", false') -and
        $runner.Contains('entity.PrepareRespec()')
    'disposable-fighter-seed' = $runner.Contains('fighterSeeded == 1 && bodyReplaced')
    'gunslinger-respec-preview' = $runner.Contains('previewFighterBefore == 0') -and
        $runner.Contains('previewGunslingerAfter == 1') -and
        $runner.Contains('sourceFighterAfter == 1 && sourceGunslingerAfter == 0')
    'no-commit' = -not $runner.Substring($runner.IndexOf(
        'private RuntimeTestResult RunDisposableGunslingerRespecPreview()')).Contains(
        'GetMethod("Commit"')
    'external-isolation' = $runner.Contains('Exact isolated Respec preview is unavailable.') -and
        $runner.Contains('both controllers canceled and disposable entity disposed')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 34 disposable Gunslinger respec preview tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 34 disposable Gunslinger respec preview tests passed: $($checks.Count)"
