[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerSelection') -and $common.Contains("'disposable-gunslinger-selection'")
    'exact-char-gen-mode' = $runner.Contains('Enum.Parse(modeType, "CharGen", false)')
    'native-preview-owner' = $runner.Contains('new Kingmaker.UI.LevelUp.ChargenUnit(source)') -and $runner.Contains('descriptor = entity.Descriptor')
    'select-and-apply' = $runner.Contains('SelectClass') -and $runner.Contains('ApplyClassMechanics') -and
        $runner.Contains('ReadExactMember(levelUpState, "SelectedClass")') -and
        $runner.Contains('ReadExactMember(controller, "LevelUpActions")')
    'cancel-rollback' = $runner.Contains('canceledLevel == 0') -and $runner.Contains('GetMethod("Cancel"')
    'external-isolation' = $runner.Contains('SameReferences(partyBefore') -and $runner.Contains('SameReferences(unitsBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 disposable Gunslinger selection tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 disposable Gunslinger selection tests passed: $($checks.Count)"
