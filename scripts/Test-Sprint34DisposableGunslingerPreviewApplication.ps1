[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerPreviewApplication') -and $common.Contains("'disposable-gunslinger-preview-application'")
    'preview-not-source' = $runner.Contains('ReferenceEquals(preview, sourceDescriptor)')
    'native-preview-refresh' = $runner.Contains('previewAfterSelection == 1') -and
        $runner.Contains('ReadExactMember(controller, "LevelUpActions")')
    'source-unchanged' = $runner.Contains('previewAfter == 1') -and $runner.Contains('sourceAfter == 0')
    'external-isolation' = $runner.Contains('SameReferences(partyBefore') -and $runner.Contains('SameReferences(unitsBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 disposable Gunslinger preview application tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 disposable Gunslinger preview application tests passed: $($checks.Count)"
