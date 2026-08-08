[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "Sprint 87 binding contract failed: $Label" }
}
$source = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\GunslingerStartingFirearmOwnershipPatch.cs') -Raw
Require ($source.Contains('[HarmonyPatch(typeof(LevelUpHelper), "AddStartingItems")]')) 'exact-native-target'
Require ($source.Contains('ReferenceIdentityComparer.Instance')) 'reference-before-snapshot'
Require ($source.Contains('ReferenceEquals(unit.Progression.GetMaxClass()')) 'exact-gunslinger-receiver'
Require ($source.Contains('!__state.Before.Contains(item)')) 'new-items-only'
Require ($source.Contains('IsProductionFirearm(item.Blueprint)')) 'production-catalog-only'
Require ($source.Contains('addedFirearms.Length != 1') -and
    $source.Contains('__state.Expected.Item')) 'exactly-one-expected-required'
Require ($source.Contains('BatteredFirearmOriginRuntime.Bind')) 'item-origin-binding'
Require ($source.Contains('HasExistingBoundStarter')) 'repeated-grant-suppression'
'Sprint 87 exact starting-firearm binding source contract passed.'
