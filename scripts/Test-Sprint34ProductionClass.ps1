[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$classPath = Join-Path $root 'src\KingmakerGunslinger\Blueprints\GunslingerClassBlueprints.cs'
$bootstrapPath = Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs'
$manifestPath = Join-Path $root 'blueprints\blueprints.json'
$class = Get-Content -Raw -LiteralPath $classPath
$bootstrap = Get-Content -Raw -LiteralPath $bootstrapPath
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json

$checks = [ordered]@{
    'exact-native-progression-identities' =
        $class.Contains('b3057560ffff3514299e8b93e7648a9d') -and
        $class.Contains('ff4662bde9e75f145853417313842751') -and
        $class.Contains('dc0c7c1aba755c54f96c089cdf7d14a3')
    'exact-native-proficiency-identities' =
        $class.Contains('e70ecf1ed95ca2f40b754f1adb22bbdd') -and
        $class.Contains('203992ef5b35c864390b4e4a1e200629') -and
        $class.Contains('6d3728d4e9c9898458fe5e9532951132')
    'level-one-grants-aggregate' =
        $class.Contains('level == 1 ? new List<BlueprintFeatureBase> { proficiencies }')
    'twenty-exact-level-rows' =
        $class.Contains('var entries = new LevelEntry[20]') -and
        $class.Contains('progression.LevelEntries.Length != 20')
    'class-chassis-exact' =
        $class.Contains('result.HitDie = DiceType.D10') -and
        $class.Contains('result.SkillPoints = 4') -and
        $class.Contains('result.FortitudeSave = goodSave') -and
        $class.Contains('result.ReflexSave = goodSave') -and
        $class.Contains('result.WillSave = poorSave')
    'bootstrap-registers-three-blueprints' =
        $bootstrap.Contains('ExpectedRegisteredBlueprintCount = 27') -and
        $bootstrap.Contains('GunslingerClassBlueprints.Register(')
    'catalog-publication-is-verified-and-reversible' =
        $class.Contains('GunslingerClassCatalogPublication Publish(') -and
        $class.Contains('root.Progression.CharacterClasses = published') -and
        $class.Contains('ReferenceEquals(root.Progression.CharacterClasses, _published)') -and
        $bootstrap.Contains('classPublication.Rollback()')
    'manifest-has-exact-production-symbols' =
        @($manifest.entries | Where-Object {
            $_.symbol -in @('KMG.Classes.GunslingerClass',
                'KMG.Classes.GunslingerProgression',
                'KMG.Classes.GunslingerProficiencies') -and $_.status -eq 'active'
        }).Count -eq 3
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 production class tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 production class tests passed: $($checks.Count)"
