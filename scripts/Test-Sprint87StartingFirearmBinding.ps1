[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "Sprint 87 binding contract failed: $Label" }
}
$patch = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\GunslingerStartingFirearmOwnershipPatch.cs') -Raw
$transaction = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\GunslingerStartingFirearmGrantTransaction.cs') -Raw
Require ($patch.Contains('[HarmonyPatch(typeof(LevelUpHelper), "AddStartingItems")]')) 'exact-native-target'
Require ($patch.Contains('[HarmonyPatch(typeof(LevelUpController), "Commit")]')) 'exact-commit-target'
Require ($patch.Contains('[HarmonyPatch(typeof(Player), "RespecCompanion"')) 'exact-respec-success-target'
Require ($transaction.Contains('ReferenceIdentityComparer.Instance')) 'reference-before-snapshot'
Require ($transaction.Contains('ExactGunslingerLevel(descriptor)') -and
    -not $transaction.Contains('GetMaxClass()')) 'exact-gunslinger-level-boundary'
Require ($transaction.Contains('!snapshot.References.Contains(item)')) 'new-items-only'
Require ($transaction.Contains('IsProductionFirearm(item.Blueprint)')) 'production-catalog-only'
Require ($transaction.Contains('current.Length != 1') -and
    $transaction.Contains('snapshot.Expected.Item')) 'exactly-one-expected-required'
Require ($transaction.Contains('BatteredFirearmOriginRuntime.Bind')) 'item-origin-binding'
Require ($transaction.Contains('HasReceipt(ownerId)')) 'durable-repeated-grant-suppression'
Require ($transaction.Contains('RollbackInventory(snapshot')) 'atomic-rollback'
'Sprint 87 exact starting-firearm binding source contract passed.'
