[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$blueprints = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\GritBlueprints.cs')
$bonus = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Grit\GritResourceAmountBonus.cs')
$initial = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Grit\GritInitialLevelRestore.cs')
$class = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\GunslingerClassBlueprints.cs')
$bootstrap = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs')
$manifest = Get-Content -Raw -LiteralPath (Join-Path $root 'blueprints\blueprints.json') | ConvertFrom-Json

$checks = [ordered]@{
    'persistent-unit-resource' = $blueprints.Contains('AddAbilityResources') -and
        $blueprints.Contains('RestoreAmount = true') -and
        $blueprints.Contains('RestoreOnLevelUp = false')
    'wisdom-floor-formula' = $blueprints.Contains('ConfigureBaseAmount(resource, 1)') -and
        $bonus.Contains('StatType.Wisdom') -and $bonus.Contains('wisdomModifier - 1')
    'native-amount-arrays' = $blueprints.Contains('ConfigureEmptyArray(amountField.FieldType, amount, "Class")') -and
        $blueprints.Contains('ConfigureEmptyArray(amountField.FieldType, amount, "ArchetypesDiv")')
    'exact-resource-filter' = $bonus.Contains('resource != Resource') -and
        $bonus.Contains('fact == null || !fact.Active')
    'first-class-level-reconcile' = $initial.Contains('IUnitReapplyFeaturesOnLevelUpHandler') -and
        $initial.Contains('GetClassLevel(CharacterClass) != 1') -and
        $initial.Contains('Owner.Resources.Restore(Resource)')
    'unit-scoped-reapply' = $initial.Contains('IUnitSubscriber') -and
        $initial.Contains('HandleUnitReapplyFeaturesOnLevelUp()')
    'level-one-grant' = $class.Contains('new List<BlueprintFeatureBase> { proficiencies, grit }')
    'manifest-identities' = @($manifest.entries | Where-Object {
        $_.symbol -in @('KMG.Classes.GunslingerGritResource',
            'KMG.Classes.GunslingerGritFeature') -and $_.status -eq 'active'
    }).Count -eq 2
    'registration-count' = $bootstrap.Contains('ExpectedRegisteredBlueprintCount = 29')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 35 grit blueprint tests failed: $($failed -join ', ')" }
Write-Host "Sprint 35 grit blueprint tests passed: $($checks.Count)"
