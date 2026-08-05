[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterShotRuntime.cs')
$explosion = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Explosions\FirearmExplosionRuntime.cs')
$marker = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterVolleyRuntime.cs')
$patches = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\CombatTracePatches.cs')
$dischargeRuntime = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeRuntime.cs')
$commandPatch = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Firing\EmptyFirearmAttackCommandPatch.cs')
$definitions = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Firearms\FirearmDefinitions.cs')
$required = @('Targets.Resolve(caster, aimedTarget)',
  'Firing into an empty direction is still a completed discharge.',
  'Transition(firearm, expected, discharge.After)',
  'ScatterAttackVolleyDecision.AttackPenalty', 'ScatterVolleyRuntime.Register(',
  'Volleys.Evaluate(', 'if (volley.AllRollsMisfire)',
  'new ScatterExplosionDamageService().Evaluate(',
  'scatterExplosion.BaseDamageMultiplier',
  'Committing the chamber transition is the point of discharge.')
foreach ($token in $required) { if (-not $runtime.Contains($token)) { throw "Missing Scatter Shot transaction token: $token" } }
if ($runtime.Contains('Transition(firearm, expected, before)')) {
  throw 'Scatter Shot still restores a consumed chamber after downstream damage may have resolved.'
}
if (-not $explosion.Contains('baseDamageMultiplier = 1') -or
    -not $explosion.Contains('damageDice.Rolls * 3')) { throw 'Triple base-damage burst adapter is incomplete.' }
if (-not $marker.Contains('damage.DisablePrecisionDamage = true') -or
    -not $patches.Contains('Scatter.ScatterVolleyRuntime.SuppressPrecisionDamage(damage);')) { throw 'Marked scatter damage does not suppress precision damage.' }
if ($dischargeRuntime.Contains('PublishScatterOnlyWarning') -or
    $commandPatch.Contains('Blunderbuss attack requires the granted Scatter Shot ability.') -or
    -not $definitions.Contains('FirearmKind.Blunderbuss') -or
    -not $definitions.Contains('                10,')) {
  throw 'Ordinary Blunderbuss bullet attacks are not preserved as a distinct 10-foot firing mode.'
}
if (-not $runtime.Contains('ExecuteResolved') -or
    -not $runtime.Contains('Targets.Resolve(caster, aimedPoint)')) {
  throw 'Scatter point targeting does not retain the clicked direction through one resolved target plan.'
}
Write-Output 'Sprint 97 Scatter Shot transaction contract passed.'
