[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$ability = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Blueprints\ScatterShotBlueprints.cs')
$logic = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterShotAbilityLogic.cs')
$proficiency = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Blueprints\FirearmProficiencyBlueprints.cs')
$bootstrap = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs')
$manifest = Get-Content -Raw (Join-Path $root 'blueprints\blueprints.json')
foreach ($token in @('AbilityRange.Close', 'CommandType.Standard',
  'ScatterShotAbilityLogic.Create()', 'CanTargetPoint = false')) {
  if (-not $ability.Contains($token)) { throw "Missing Scatter Shot ability token: $token" }
}
if (-not $logic.Contains('ScatterShotRuntime.ExecuteFromAbility') -or
    -not $logic.Contains('ScatterShotRuntime.IsAvailable')) { throw 'Scatter ability delivery/availability is incomplete.' }
if ($logic.Contains('ExecuteForRuntimeTest')) { throw 'Production Scatter ability still calls a test-only API.' }
if (-not $proficiency.Contains('scatterShotAbility') -or
    -not $bootstrap.Contains('ScatterShotBlueprints.Register(registry)') -or
    -not $manifest.Contains('KMG.Firearms.ScatterShotAbility')) { throw 'Scatter ability registration/grant is incomplete.' }
Write-Output 'Sprint 98 guarded Scatter Shot ability contract passed.'
