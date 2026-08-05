[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$core = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensics.cs')
$patches = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensicsPatches.cs')
$sampler = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensicsSampler.cs')
function Require([bool]$condition, [string]$name) { if (-not $condition) { throw "FAIL: $name" }; Write-Host "PASS: $name" }
Require ($core.Contains('if (!File.Exists(marker)) return;')) 'marker absence disables initialization'
Require ($core.Contains('_enabled = true;') -and $core.Contains('Manual Dodge forensics is enabled')) 'marker presence enables tracing'
Require ($core.Contains('DodgeGuid = "bbd7d42117cc4c23b3e22af3a71621d9"') -and $core.Contains('ControlBuffName = "TotalDefenseBuff"')) 'exact Dodge and native-control filters'
Require ($core.Contains('RuntimeHelpers.GetHashCode(buff)')) 'runtime references distinguish same-blueprint instances'
$fields = @('sequence','utcTimestamp','eventName','threadId','onGameThread','gameTimeTicks','gameTimeSeconds','turnBasedCombatActive','currentTurnUnitIdentity','ownerIdentity','ownerCharacterName','buffRuntimeReferenceId','buffRuntimeUniqueId','blueprintGuid','blueprintInternalName','casterIdentity','sourceAbilityGuid','sourceAbilityInternalName','contextIdentity','dodgeCollectionCount','dodgeInstanceIdentities','endTimeTicks','endTimeSeconds','timeLeftTicks','timeLeftSeconds','nextTickTimeTicks','nextTickTimeSeconds','nextEventTimeTicks','nextEventTimeSeconds','isPermanent','isActive','isDisposed','rank','collectionNextEventRuntimeIdentity','collectionNextEventBlueprintGuid','collectionNextEventBlueprintInternalName','armorClassModifiedValue','gritAmount','requestedDurationTicks','requestedDurationSeconds','exceptionType','exceptionMessage')
Require (@($fields | Where-Object { $core -notmatch ("\b" + [regex]::Escape($_) + "\b") }).Count -eq 0) 'JSONL schema contains required fields'
$diagnostic = $core + $patches + $sampler
Require (-not ($diagnostic -match '\.AddBuff\s*\(') -and -not ($diagnostic -match '\.RemoveFact\s*\(') -and -not ($diagnostic -match '\.UpdateNextEvent\s*\(') -and -not ($diagnostic -match 'SaveGame|SaveManager|SaveInfo')) 'diagnostics invoke no prohibited mutation APIs'
Require (-not ($sampler -match 'AddBuff|RemoveFact|UpdateNextEvent|EndTime\s*=|AddModifier|RemoveModifier|Resources\..*(Spend|Restore)|Commands')) 'sampler is read-only'
Require ($core.Contains('_writer.AutoFlush = true;') -and $core.Contains('_writer.Flush();') -and $core.Contains('_writer.BaseStream.Flush();')) 'deterministic immediate flushing'
foreach ($name in @('Install-DodgeForensics.ps1','Collect-DodgeForensics.ps1','Disable-DodgeForensics.ps1','MANUAL-DODGE-FORENSICS-TEST.md')) { Require (Test-Path -LiteralPath (Join-Path $root "artifacts\manual-test\dodge-forensics\$name")) "generated $name present" }
foreach ($name in @('Install-DodgeForensics.ps1','Collect-DodgeForensics.ps1','Disable-DodgeForensics.ps1')) { $errors=$null; [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $root "artifacts\manual-test\dodge-forensics\$name"),[ref]$null,[ref]$errors); Require ($errors.Count -eq 0) "$name syntax" }
Write-Host "Dodge lifecycle forensics focused tests passed ($($fields.Count + 11) assertions)."
