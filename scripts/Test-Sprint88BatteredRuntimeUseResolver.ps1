[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmRuntimeUseResolver.cs') -Raw
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 88 resolver contract failed: $Label" } }
Require ($source.Contains('KingmakerFirearmItemIdentityProvider')) 'exact-item-identity'
Require ($source.Contains('TryGetExisting(out part)')) 'read-only-existing-carrier'
Require ($source.Contains('part.TryGetOwner(itemId, out ownerId)')) 'exact-item-binding'
Require ($source.Contains('BatteredFirearmUsePolicy.Evaluate(false, false')) 'ordinary-unbound-path'
Require ($source.Contains('ownerId.Equals(userId)')) 'exact-origin-comparison'
Require ($source.Contains('user.UniqueId.Trim()')) 'stable-user-identity'
'Sprint 88 battered runtime-use resolver source contract passed.'
