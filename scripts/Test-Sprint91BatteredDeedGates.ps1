[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 91 contract failed: $Label" } }
$deadShot = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Deeds\DeadShotRuntime.cs') -Raw
$lightning = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Deeds\LightningReloadRuntime.cs') -Raw
$whip = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Deeds\PistolWhipRuntime.cs') -Raw
$quickClear = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Deeds\QuickClearRuntime.cs') -Raw
Require (([regex]::Matches($deadShot, 'firearm\.EffectiveCondition').Count) -eq 4) 'dead-shot-all-boundaries'
Require ($lightning.Contains('firearm.EffectiveCondition')) 'lightning-reload-use-gate'
Require ($whip.Contains('firearm.EffectiveCondition')) 'pistol-whip-use-gate'
Require ($quickClear.Contains('firearm.Firearm.Repository.State.Condition') -and $quickClear.Contains('actual condition')) 'quick-clear-actual-maintenance'
'Sprint 91 battered deed-gate source contract passed.'
