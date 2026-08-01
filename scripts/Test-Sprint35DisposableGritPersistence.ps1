[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerGritPersistence') -and
        $common.Contains("'disposable-gunslinger-grit-persistence'")
    'two-detached-units' = ([regex]::Matches($runner,
        'new Kingmaker.UI.LevelUp.ChargenUnit\(source\)')).Count -ge 2
    'nontrivial-current' = $runner.Contains('Stats.Wisdom.BaseValue = 14') -and
        $runner.Contains('maximum == 2 && originalCurrent == 1')
    'native-json-settings' = $runner.Contains('DefaultJsonSettings.DefaultSettings') -and
        $runner.Contains('DeserializeObject<Kingmaker.UnitLogic.UnitAbilityResource>')
    'collection-reconstruction' = $runner.Contains('replacementDescriptor.Resources.PersistantResources =') -and
        $runner.Contains('serializedRecordCount == 1 && replacementCurrent == 1')
    'later-reapply-no-refill' = $runner.Contains('Progression.ReapplyFeaturesOnLevelUp()') -and
        $runner.Contains('currentAfterLaterReapply == 1')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 35 disposable grit-persistence tests failed: $($failed -join ', ')" }
Write-Host "Sprint 35 disposable grit-persistence tests passed: $($checks.Count)"
