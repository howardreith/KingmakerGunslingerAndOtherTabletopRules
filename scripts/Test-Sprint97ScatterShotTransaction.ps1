[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterShotRuntime.cs')
$explosion = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Explosions\FirearmExplosionRuntime.cs')
$marker = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterVolleyRuntime.cs')
$patches = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\CombatTracePatches.cs')
$dischargeRuntime = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeRuntime.cs')
$required = @('Targets.Resolve(caster, aimedTarget)',
  'if (plan.TargetCount == 0)', 'Transition(firearm, expected, discharge.After)',
  'ScatterAttackVolleyDecision.AttackPenalty', 'ScatterVolleyRuntime.Register(',
  'Volleys.Evaluate(', 'if (volley.AllRollsMisfire)',
  'new ScatterExplosionDamageService().Evaluate(',
  'scatterExplosion.BaseDamageMultiplier',
  'if (transitioned) Transition(firearm, expected, before)')
foreach ($token in $required) { if (-not $runtime.Contains($token)) { throw "Missing Scatter Shot transaction token: $token" } }
if (-not $explosion.Contains('baseDamageMultiplier = 1') -or
    -not $explosion.Contains('damageDice.Rolls * 3')) { throw 'Triple base-damage burst adapter is incomplete.' }
if (-not $marker.Contains('damage.DisablePrecisionDamage = true') -or
    -not $patches.Contains('Scatter.ScatterVolleyRuntime.SuppressPrecisionDamage(damage);')) { throw 'Marked scatter damage does not suppress precision damage.' }
if (-not $dischargeRuntime.Contains('if (marker.Definition.IsScatter)') -or
    -not $dischargeRuntime.Contains('PublishScatterOnlyWarning') -or
    $dischargeRuntime.IndexOf('if (marker.Definition.IsScatter)') -gt
      $dischargeRuntime.IndexOf('FirearmRuntimeState.Service.TryGetOrCreate')) {
  throw 'Ordinary scatter attacks are not rejected before chamber inspection/consumption.'
}
Write-Output 'Sprint 97 Scatter Shot transaction contract passed.'
