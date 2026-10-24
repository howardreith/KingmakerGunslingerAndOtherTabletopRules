$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

if (-not (Test-KmgSprints3031DevelopmentPersistence `
    -Scope outsider-celestial -Purpose sprints30-31-persistence)) {
    throw 'Exact Sprints 30-31 hidden development scope refused.'
}
if (Test-KmgSprints3031DevelopmentPersistence `
    -Scope outsider-celestial -Purpose '') {
    throw 'Release candidate was treated as hidden development.'
}
$count = 2
foreach ($case in @(
    @('Outsider-celestial', 'sprints30-31-persistence'),
    @('outsider-celestial', 'Sprints30-31-persistence'),
    @('outsider-celestial', 'sprint28-native-prototype'),
    @('giant-humanoids', 'sprints30-31-persistence'),
    @('whole-roster', 'sprints30-31-persistence'),
    @('outsider-celestial', 'sprints30-31-persistence ')
)) {
    $refused = $false
    try {
        [void](Test-KmgSprints3031DevelopmentPersistence `
            -Scope $case[0] -Purpose $case[1])
    } catch { $refused = $true }
    if (-not $refused) {
        throw 'Invalid Sprints 30-31 hidden development persistence authority accepted.'
    }
    $count++
}
Write-Host "Sprints 30-31 hidden development persistence authority PASS=$count; pure scope/refusal checks only."

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
 & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario -ExpectedVersion $version -SaveName KMG_AUTOMATION_WORKING -Parameters @{persistenceScope='outsider-celestial'} -ExitAfterCompletion -AllowDirtyGit -WhatIf -Confirm:$false
 $count++
}
$afterEvidence=@(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | ForEach-Object FullName)
if(@(Compare-Object $beforeEvidence $afterEvidence).Count -or (Test-Path -LiteralPath $stage) -ne $beforeStage -or (Test-Path -LiteralPath $lock) -ne $beforeLock -or (Get-Process Kingmaker -ErrorAction SilentlyContinue)) {throw 'Source-only pilot test crossed a runtime boundary.'}
Write-Host "Pilot finite persistence purpose/outer route checks PASS=$count; no game/save/deployment."
