[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 92 contract failed: $Label" } }
$source = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmSaleValueRuntime.cs') -Raw
$project = Get-Content (Join-Path $root 'src\KingmakerGunslinger\KingmakerGunslinger.csproj') -Raw
Require ($source.Contains('typeof(VendorLogic), "GetItemBuyPrice"')) 'exact-sale-target'
Require ($source.Contains('new Type[] { typeof(ItemEntity) }')) 'exact-signature'
Require ($source.Contains('TryGetOwner(itemId, out owner)')) 'persisted-item-identity-gate'
Require ($source.Contains('FixedExpectedScrapValueGold') -and $source.Contains('__result = fixedValue')) 'fixed-result'
Require ($project.Contains('Gunsmithing\BatteredFirearmSaleValueRuntime.cs')) 'project-inclusion'
'Sprint 92 battered sale-value source contract passed.'
