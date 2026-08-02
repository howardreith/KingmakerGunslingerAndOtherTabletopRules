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
Require ($source.Contains('ReferenceEquals(item.Blueprint, pistol)')) 'exact-pistol-blueprint'
Require ($source.Contains('addedPistols.Length != 1')) 'exactly-one-required'
Require ($source.Contains('KingmakerFirearmItemIdentityProvider')) 'engine-item-identity'
Require ($source.Contains('Descriptor.Unit.UniqueId.Trim()')) 'origin-unit-identity'
Require ($source.Contains('RequireForWrite().Bind(itemId, ownerId)')) 'persistent-immutable-binding'
'Sprint 87 exact starting-firearm binding source contract passed.'
