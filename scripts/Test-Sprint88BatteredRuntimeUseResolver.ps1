[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Gunsmithing\BatteredFirearmRuntimeUseResolver.cs') -Raw
function Require([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Sprint 88 resolver contract failed: $Label" } }
Require ($source.Contains('BatteredFirearmOriginRuntime.TryGetOwner')) 'item-owned-origin-carrier'
Require ($source.Contains('BatteredFirearmUsePolicy.Evaluate(false, false')) 'ordinary-unbound-path'
Require ($source.Contains('ReferenceEquals(owner, user)')) 'exact-origin-comparison'
Require ($source.Contains('user.UniqueId')) 'stable-user-identity'
'Sprint 88 battered runtime-use resolver source contract passed.'
