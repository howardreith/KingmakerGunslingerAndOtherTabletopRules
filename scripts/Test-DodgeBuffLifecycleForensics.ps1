[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$core = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensics.cs')
$patches = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensicsPatches.cs')
$sampler = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\DodgeBuffLifecycleForensicsSampler.cs')
$installerPath = Join-Path $root 'artifacts\manual-test\dodge-forensics\Install-DodgeForensics.ps1'
$installer = Get-Content -Raw -LiteralPath $installerPath
function Require([bool]$condition, [string]$name) { if (-not $condition) { throw "FAIL: $name" }; Write-Host "PASS: $name" }
Require ($core.Contains('if (!File.Exists(marker)) return;')) 'marker absence disables initialization'
Require ($core.Contains('_enabled = true;') -and $core.Contains('Manual Dodge forensics is enabled')) 'marker presence enables tracing'
Require ($core.Contains('DodgeGuid = "bbd7d42117cc4c23b3e22af3a71621d9"')) 'exact Dodge filter'
Require (-not ($core -match 'TotalDefense|Total Defense')) 'Total Defense is not required'
Require ($core.Contains('RuntimeHelpers.GetHashCode(buff)')) 'runtime references distinguish same-blueprint instances'
$fields = @('sequence','utcTimestamp','eventName','threadId','onGameThread','gameTimeTicks','gameTimeSeconds','turnBasedCombatActive','currentTurnUnitIdentity','ownerIdentity','ownerCharacterName','buffRuntimeReferenceId','buffRuntimeUniqueId','blueprintGuid','blueprintInternalName','casterIdentity','sourceAbilityGuid','sourceAbilityInternalName','contextIdentity','dodgeCollectionCount','dodgeInstanceIdentities','endTimeTicks','endTimeSeconds','timeLeftTicks','timeLeftSeconds','nextTickTimeTicks','nextTickTimeSeconds','nextEventTimeTicks','nextEventTimeSeconds','isPermanent','isActive','isDisposed','rank','collectionNextEventRuntimeIdentity','collectionNextEventBlueprintGuid','collectionNextEventBlueprintInternalName','armorClassModifiedValue','gritAmount','requestedDurationTicks','requestedDurationSeconds','exceptionType','exceptionMessage')
Require (@($fields | Where-Object { $core -notmatch ("\b" + [regex]::Escape($_) + "\b") }).Count -eq 0) 'JSONL schema contains required fields'
$diagnostic = $core + $patches + $sampler
Require (-not ($patches -match 'static void (Prefix|Postfix)\([^\)]*Fact fact')) 'Fact arguments use positional Harmony binding'
Require ($patches.Contains('Postfix(BuffCollection __instance, Fact __0)') -and $patches.Contains('Fact newFact = __0;')) 'OnFactCreated binds newFact positionally'
Require (-not ($diagnostic -match '\.AddBuff\s*\(') -and -not ($diagnostic -match '\.RemoveFact\s*\(') -and -not ($diagnostic -match '\.UpdateNextEvent\s*\(') -and -not ($diagnostic -match 'SaveGame|SaveManager|SaveInfo')) 'diagnostics invoke no prohibited mutation APIs'
Require (-not ($sampler -match 'AddBuff|RemoveFact|UpdateNextEvent|EndTime\s*=|AddModifier|RemoveModifier|Resources\..*(Spend|Restore)|Commands')) 'sampler is read-only'
Require ($core.Contains('_writer.AutoFlush = true;') -and $core.Contains('_writer.Flush();') -and $core.Contains('_writer.BaseStream.Flush();')) 'deterministic immediate flushing'
Require ($core.Contains('PreserveReferencesHandling.None') -and $core.Contains('ReferenceLoopHandling.Error') -and $core.Contains('NullValueHandling.Include')) 'independent fail-closed JSON serializer settings'
Require ($core.Contains('dodge-forensics.write-failed') -and $core.Contains('_enabled = false;')) 'write failure disables diagnostics without throwing'
foreach ($name in @('Install-DodgeForensics.ps1','Collect-DodgeForensics.ps1','Disable-DodgeForensics.ps1','MANUAL-DODGE-FORENSICS-TEST.md')) { Require (Test-Path -LiteralPath (Join-Path $root "artifacts\manual-test\dodge-forensics\$name")) "generated $name present" }
foreach ($name in @('Install-DodgeForensics.ps1','Collect-DodgeForensics.ps1','Disable-DodgeForensics.ps1')) { $errors=$null; [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $root "artifacts\manual-test\dodge-forensics\$name"),[ref]$null,[ref]$errors); Require ($errors.Count -eq 0) "$name syntax" }
Require (-not $installer.Contains('-Single')) 'generated installer contains no invalid -Single token'
Require (-not ($installer -match 'New-Item\s+-LiteralPath')) 'generated installer uses Windows PowerShell-compatible New-Item parameters'
$tokens = $null; $errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($installerPath, [ref]$tokens, [ref]$errors)
$functionAst = $ast.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Resolve-ExactlyOneCandidate' }, $true)
Require ($null -ne $functionAst) 'exact-one resolver is present'
Invoke-Expression $functionAst.Extent.Text
function Expect-CardinalityFailure([object[]]$values, [int]$count) {
    try { [void](Resolve-ExactlyOneCandidate $values 'test candidate'); throw 'Expected cardinality failure.' }
    catch { Require ($_.Exception.Message -eq "Expected exactly one test candidate, found $count.") "cardinality $count fails clearly" }
}
Expect-CardinalityFailure @() 0
$only = [pscustomobject]@{ Name = 'only' }
Require ((Resolve-ExactlyOneCandidate @($only) 'test candidate') -eq $only) 'cardinality one succeeds'
Expect-CardinalityFailure @('first','second') 2
$generated = Join-Path ([IO.Path]::GetTempPath()) ('Install-DodgeForensics-' + [guid]::NewGuid() + '.ps1')
try {
    & (Join-Path $root 'scripts\New-DodgeForensicsInstaller.ps1') -OutputPath $generated
    Require ((Get-Content -LiteralPath $generated -Raw) -eq $installer) 'generator reproduces installer exactly'
} finally { if (Test-Path -LiteralPath $generated) { Remove-Item -LiteralPath $generated -Force } }
Write-Host "Dodge lifecycle forensics focused tests passed ($($fields.Count + 19) assertions)."
