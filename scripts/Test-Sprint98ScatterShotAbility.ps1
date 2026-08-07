[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$ability = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Blueprints\ScatterShotBlueprints.cs')
$logic = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterShotAbilityLogic.cs')
$proficiency = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Blueprints\FirearmProficiencyBlueprints.cs')
$bootstrap = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs')
$manifest = Get-Content -Raw (Join-Path $root 'blueprints\blueprints.json')
foreach ($token in @('BurningHandsGuid', 'AbilityRange.Custom',
  'CustomRange = new Feet(15f)', 'CommandType.Standard',
  'ScatterShotAbilityLogic.Create(nativeCone,', 'CanTargetPoint = true',
  'AbilityProjectileType.Cone',
  'burningHands.ResourceAssetIds')) {
  if (-not $ability.Contains($token)) { throw "Missing Scatter Shot ability token: $token" }
}
foreach ($token in @('class ScatterShotAbilityLogic : AbilityDeliverProjectile',
  'base.Deliver(context, target)', 'ScatterShotRuntime.ExecuteFromAbility',
  'ScatterShotRuntime.IsAvailable')) {
  if (-not $logic.Contains($token)) { throw "Missing Scatter delivery token: $token" }
}
if ($logic.Contains('ExecuteForRuntimeTest')) { throw 'Production Scatter ability still calls a test-only API.' }
if (-not $proficiency.Contains('scatterShotAbility') -or
    -not $bootstrap.Contains('ScatterShotBlueprints.Register(library, registry)') -or
    -not $manifest.Contains('KMG.Firearms.ScatterShotAbility')) { throw 'Scatter ability registration/grant is incomplete.' }
Write-Output 'Sprint 98 native-cone Scatter Shot ability contract passed.'
