[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterVolleyRuntime.cs')
$discharge = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeRuntime.cs')
$misfire = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\Misfires\FirearmMisfirePatches.cs')
$required = @('ConditionalWeakTable<RuleAttackWithWeapon, Marker>',
  'ShouldBypassOrdinaryDischarge', 'BeforeSetRoll', 'AfterIsSuccessRoll',
  'if (marker.IsMisfire) nativeResult = false;', 'Markers.Remove(attack);',
  'new ScatterAttackRollObservation(marker.Target,')
foreach ($token in $required) { if (-not $runtime.Contains($token)) { throw "Missing volley marker token: $token" } }
if (-not $discharge.Contains('Scatter.ScatterVolleyRuntime.ShouldBypassOrdinaryDischarge(attackRoll)')) { throw 'Scatter attacks do not bypass ordinary discharge.' }
if (-not $misfire.Contains('Scatter.ScatterVolleyRuntime.BeforeSetRoll(__instance, ref value);') -or
    -not $misfire.Contains('Scatter.ScatterVolleyRuntime.AfterIsSuccessRoll(__instance, d20, ref __result);')) { throw 'Scatter natural-roll hooks are incomplete.' }
Write-Output 'Sprint 96 scatter volley marker contract passed.'
