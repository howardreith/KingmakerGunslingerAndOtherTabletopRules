[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "Sprint 86 persistence contract failed: $Label" }
}
$part = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\UnitPartBatteredFirearmOwnership.cs') -Raw
$provider = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\KingmakerBatteredFirearmOwnershipPartProvider.cs') -Raw
Require ($part.Contains('public sealed class UnitPartBatteredFirearmOwnership : UnitPart')) 'save-owned-unit-part'
Require ($part.Contains('[JsonProperty] public string ItemId') -and $part.Contains('[JsonProperty] public string OwnerId')) 'primitive-identities'
Require (-not $part.Contains('ItemEntity') -and -not $part.Contains('UnitEntityData')) 'no-runtime-references'
Require ($part.Contains('persisted battered firearm cannot be rebound')) 'immutable-origin'
Require ($part.Contains('contains duplicate item identities')) 'duplicate-rejection'
Require ($part.Contains('Persisted ownership removal requires the exact originating unit.')) 'owner-checked-removal'
Require ($provider.Contains('TryResolveMainCharacter') -and $provider.Contains('Ensure<UnitPartBatteredFirearmOwnership>')) 'shared-player-host'
'Sprint 86 battered ownership persistence source contract passed.'
