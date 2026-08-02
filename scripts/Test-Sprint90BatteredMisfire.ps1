[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 90 contract failed: $Label" } }
$runtime = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Misfires\FirearmMisfireRuntime.cs') -Raw
$service = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Misfires\FirearmMisfireConditionService.cs') -Raw
$decision = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Misfires\FirearmMisfireConditionDecision.cs') -Raw
$discharge = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeRuntime.cs') -Raw
Require ($discharge.Contains('result.EffectiveCondition')) 'discharge-context-propagation'
Require ($runtime.Contains('context.EffectiveCondition') -and $runtime.Contains('effectiveCondition')) 'runtime-overlay'
Require ($service.Contains('FirearmCondition.Wrecked') -and $service.Contains('effectiveCondition')) 'effective-consequence'
Require ($decision.Contains('EffectiveCondition') -and $decision.Contains('BrokenToWrecked')) 'decision-proof'
'Sprint 90 battered misfire source contract passed.'
