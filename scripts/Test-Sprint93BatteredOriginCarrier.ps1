[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 93 contract failed: $Label" } }
$runtime = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmOriginRuntime.cs') -Raw
$binder = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\GunslingerStartingFirearmOwnershipPatch.cs') -Raw
$use = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmRuntimeUseResolver.cs') -Raw
$sale = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmSaleValueRuntime.cs') -Raw
Require ($runtime.Contains('item.AddEnchantment') -and $runtime.Contains('ParentContext.MaybeCaster')) 'item-owned-context'
Require ($runtime.Contains('Matches(item).Length != 1')) 'exactly-one-marker'
Require ($binder.Contains('BatteredFirearmOriginRuntime.Bind')) 'native-grant-binding'
Require ($use.Contains('BatteredFirearmOriginRuntime.TryGetOwner') -and $use.Contains('ReferenceEquals(owner, user)')) 'effective-use-origin'
Require ($sale.Contains('BatteredFirearmOriginRuntime.IsBattered')) 'sale-origin'
Require (-not $binder.Contains('KingmakerFirearmItemIdentityProvider')) 'no-invented-item-id'
'Sprint 93 item-owned battered-origin source contract passed.'
