[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 89 boundary contract failed: $Label" } }
$discharge = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeRuntime.cs') -Raw
$result = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Firing\FirearmDischargeResult.cs') -Raw
$equipped = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Actions\ExactEquippedFirearmResolver.cs') -Raw
$reload = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Reloading\ReloadTestMusketRuntime.cs') -Raw
Require ($discharge.Contains('BatteredFirearmRuntimeUseResolver') -and $discharge.Contains('use.EffectiveCondition')) 'native-discharge-overlay'
Require ($result.Contains('EffectiveCondition') -and $result.Contains('After != Before')) 'actual-state-preserved'
Require ($equipped.Contains('use.EffectiveCondition')) 'shared-equipped-context'
Require ($reload.Contains('context.EffectiveCondition') -and $reload.Contains('actualState.SchemaVersion')) 'reload-effective-copy'
'Sprint 89 battered use-boundary source contract passed.'
