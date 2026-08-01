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
    'exact-class-data' = $runner.Contains('classData.BaseAttackBonus.AssetGuid') -and
        $runner.Contains('b3057560ffff3514299e8b93e7648a9d') -and
        $runner.Contains('ff4662bde9e75f145853417313842751') -and
        $runner.Contains('dc0c7c1aba755c54f96c089cdf7d14a3')
    'exact-feature-store-contract' = $runner.Contains('GetMethod("UpdatePreview"') -and
        $runner.Contains('featureStoreContract = DescribeCreationType(featureStore.GetType())') -and
        $runner.Contains('no feature-store method invocation')
    'exact-aggregate-enumeration' = $runner.Contains('BindingFlags.Instance | BindingFlags.DeclaredOnly') -and
        $runner.Contains('enumerableProperty.GetValue(featureStore, null)') -and
        $runner.Contains('b9b6769f8a654a58a6bd55e10801ea22') -and
        $runner.Contains('e70ecf1ed95ca2f40b754f1adb22bbdd') -and
        $runner.Contains('203992ef5b35c864390b4e4a1e200629') -and
        $runner.Contains('6d3728d4e9c9898458fe5e9532951132') -and
        $runner.Contains('5148f69223044799800b65732b6cabea')
    'external-isolation' = $runner.Contains('SameReferences(partyBefore') -and $runner.Contains('SameReferences(unitsBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 disposable Gunslinger preview application tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 disposable Gunslinger preview application tests passed: $($checks.Count)"
