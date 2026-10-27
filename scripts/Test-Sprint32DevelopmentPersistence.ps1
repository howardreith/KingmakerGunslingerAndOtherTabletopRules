$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

if (-not (Test-KmgSprint32DevelopmentPersistence `
    -Scope fey-celestial -Purpose sprint32-lillend-support)) {
    throw 'Exact Sprint32 hidden development scope refused.'
}
if (Test-KmgSprint32DevelopmentPersistence `
    -Scope fey-celestial -Purpose '') {
    throw 'Release candidate was treated as hidden development.'
}
$count = 2
foreach ($case in @(
    @('Fey-celestial', 'sprint32-lillend-support'),
    @('fey-celestial', 'Sprint32-lillend-support'),
    @('fey-celestial', 'sprint28-native-prototype'),
    @('giant-humanoids', 'sprint32-lillend-support'),
    @('whole-roster', 'sprint32-lillend-support'),
    @('fey-celestial', 'sprint32-lillend-support ')
)) {
    $refused = $false
    try {
        [void](Test-KmgSprint32DevelopmentPersistence `
            -Scope $case[0] -Purpose $case[1])
    } catch { $refused = $true }
    if (-not $refused) {
        throw 'Invalid Sprint32 hidden development persistence authority accepted.'
    }
    $count++
}
Write-Host "Sprint32 hidden development persistence authority PASS=$count; pure scope/refusal checks only."

# Execute the exact outer launcher's source-only WhatIf route. This must not
# enter any lease/staging/evidence/game boundary. No DLL or save is deployed.
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$root=Split-Path -Parent $PSScriptRoot
$version=(Get-KmgModInfo -RepositoryRoot $root).Version
$stage=Join-Path $root 'artifacts/deploy-staging'
$lock='C:/Dev/KingmakerGunslingerLab/compatibility-state/compatibility.lock'
if(Get-Process Kingmaker -ErrorAction SilentlyContinue){throw 'No running game permitted for source-only tests.'}
$beforeEvidence=@(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | ForEach-Object FullName)
$beforeStage=Test-Path -LiteralPath $stage;$beforeLock=Test-Path -LiteralPath $lock
foreach($scenario in @('working-save-expanded-summoning-prepare','working-save-expanded-summoning-verify-cleanup','working-save-expanded-summoning-verify-absent')) {
 foreach($transition in @($null,'retained','removed','module-off')) {
  $parameters=@{persistenceScope='fey-celestial'}
  if($null -ne $transition){$parameters.suggestionTransition=$transition}
  & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario -ExpectedVersion $version -SaveName KMG_AUTOMATION_WORKING -Parameters $parameters -ExitAfterCompletion:$true -AllowDirtyGit -WhatIf -Confirm:$false
  $count++
 }
}
foreach($case in @(
 @{persistenceScope='fey-celestial';suggestionTransition='Retained'},
 @{persistenceScope='fey-celestial';suggestionTransition=''},
 @{persistenceScope='fey-celestial';suggestionTransition=$null},
 @{persistenceScope='fey-celestial';suggestionTransition=1},
 @{persistenceScope='whole-roster';suggestionTransition='retained'},
 @{persistenceScope='fey-celestial';SuggestionTransition='retained'},
 @{suggestionTransition='retained'}
)) {
 $refused=$false
 try{[void](Test-KmgSprint32SuggestionTransition -TargetedPersistence $true -Parameters $case)}catch{$refused=$true}
 if(-not $refused){throw 'Malformed fixed provider transition accepted.'}
 $count++
}
$refused=$false
try{[void](Test-KmgSprint32SuggestionTransition -TargetedPersistence $false -Parameters @{persistenceScope='fey-celestial';suggestionTransition='retained'})}catch{$refused=$true}
if(-not $refused){throw 'Provider transition admitted outside Working trio.'};$count++
$afterEvidence=@(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | ForEach-Object FullName)
if(@(Compare-Object $beforeEvidence $afterEvidence).Count -or (Test-Path -LiteralPath $stage) -ne $beforeStage -or (Test-Path -LiteralPath $lock) -ne $beforeLock -or (Get-Process Kingmaker -ErrorAction SilentlyContinue)) {throw 'Source-only Sprint32 test crossed a runtime boundary.'}
Write-Host "Sprint32 finite persistence purpose/outer route checks PASS=$count; no game/save/deployment."

# Small consistency check for this added scope across the affected admission layers.
$policy=Get-Content -LiteralPath (Join-Path $root 'src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs') -Raw
if($policy -cnotmatch 'FeyCelestialScope = "fey-celestial";'){throw 'Native persistence scope declaration differs.'}
foreach($file in @('Invoke-KingmakerRuntimeTest.ps1','RuntimeAutomation.Common.ps1','Invoke-Sprint22InstalledStack.ps1')) {
    $source=Get-Content -LiteralPath (Join-Path $PSScriptRoot $file) -Raw
    if(-not $source.Contains("'fey-celestial'")){throw "Sprint32 persistence scope missing from $file."}
}
$request=Get-Content -LiteralPath (Join-Path $root 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs') -Raw
if(-not $request.Contains('ExpandedSummoningRosterPersistencePolicy.FeyCelestialScope')){throw 'Native request does not consume the selected scope declaration.'}
# Test-Sprint32NativePersistenceRequest separately executes the compiled native
# validator after compilation; this small declaration check is not its substitute.
