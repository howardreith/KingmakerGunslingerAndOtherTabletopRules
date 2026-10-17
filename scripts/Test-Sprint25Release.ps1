[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$actual=Get-KmgSprint25SourceSurface
if($actual.registeredRoots -ne ($actual.monsterPlacements+$actual.natureAllyPlacements) -or
    $actual.choices -ne ($actual.generatedRoots+$actual.nativeWrappers)) { throw 'Source surface does not reconcile.' }
# The decision unit fixture models publication without changing source or
# suppression. The production source reader above is exercised independently.
$script:Sprint25TestSurface=$actual | ConvertTo-Json | ConvertFrom-Json
$script:Sprint25TestSurface.generatedRoots=$actual.registeredRoots
$script:Sprint25TestSurface.choices=$actual.registeredRoots+$actual.nativeWrappers
$script:Sprint25TestSurface.suppressedLogicalPlacements=0
function Get-KmgSprint25SourceSurface { return $script:Sprint25TestSurface }
$candidate=[pscustomobject]@{version='0.0.158';commit=('a'*40);packageSha256=('b'*64);
    dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('d'*64)}
function New-Sprint25Fixture {
    $ids=@('composed-profiles-signed-strength-and-commands','great-cleave-native-sequences',
        'ordinary-overrun-native-outcomes','functional-original-bodies-icon-resources',
        'eleven-routes-templates-duration-slot-cleanup','targeted-persistence-on-off-cleanup-absence',
        'standalone-and-high-risk-combined-shared-controls','inherited-unaffected-qualification',
        'complete-public-source-build-package-gate','public-inventory-icon-eleven-roots','exact-package-release-qualification')
    $roster=$script:Sprint25TestSurface | ConvertTo-Json | ConvertFrom-Json
    $roster | Add-Member -NotePropertyName identityChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName routeOrderingChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName protectedIdentitySetsEqual -NotePropertyValue $true
    [pscustomobject]@{schemaVersion=1;mission='Sprint25BrachiosaurusElephant';status='QUALIFIED';
        releaseEligible=$true;sprint26Started=$false;baselineExceptions=@();unresolvedFindings=@();
        ownerVisualReview='NOT_PERFORMED_NONBLOCKING';ownerVisualReviewEvidence=$null;
        swallowedStatePersistenceExemption=$false;
        acceptedLimitations=@('ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD','SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED',
            'PASSIVE_CREATURE_SENSES_UNMODELED','RUN_NATIVE_SUMMON_AI_DISPOSITION');
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;
            pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=7;registered=7;unfiltered=$true}};
        roster=$roster;restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{productMergeCommit=('9'*40);mergeContainsTestedSource=$true;
            tag='v0.0.158';prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object {[pscustomobject]@{id=$_;result='PASS';reuseJustification=$null;priorSource=$null;
            proofs=@([pscustomobject]@{path='unit-fixture-only-not-native-evidence';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;packageSha256=$candidate.packageSha256;
                dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid})}})}
}
$checks=2
Assert-KmgSprint25GiantBodyReleaseDecision -Ledger (New-Sprint25Fixture) -Candidate $candidate
foreach($change in @(
    {param($l) $l.releaseEligible=$false}, {param($l) $l.status='IN_PROGRESS'},
    {param($l) $l.sprint26Started=$true}, {param($l) $l.unresolvedFindings=@('unqualified Overrun')},
    {param($l) $l.baselineExceptions=@('new waiver')},
    {param($l) $l.candidate.source=('f'*40)}, {param($l) $l.candidate.zipSha256=('f'*64)},
    {param($l) $l.candidate.dllSha256=('f'*64)}, {param($l) $l.candidate.dllMvid='different'},
    {param($l) $l.candidate.sourceFingerprint=('f'*64)},
    {param($l) $l.candidate.fullSuite.failures=1}, {param($l) $l.candidate.pipelineExitCode=1},
    {param($l) $l.candidate.fullSuite.executed=0}, {param($l) $l.candidate.fullSuite.registered=8},
    {param($l) $l.candidate.fullSuite.unfiltered=$false},
    {param($l) $l.coverage=$l.coverage[1..10]}, {param($l) $l.coverage[1].id=$l.coverage[0].id},
    {param($l) $l.coverage[0].result='FAIL'}, {param($l) $l.coverage[0].proofs=@()},
    {param($l) $l.coverage[0].evidenceSources=@()},
    {param($l) $l.coverage[0].evidenceSources[0].dllMvid='different'},
    {param($l) $l.coverage[0].result='REUSED_PASS'},
    {param($l) $l.coverage[9].result='REUSED_PASS';$l.coverage[9].priorSource=$candidate.commit;$l.coverage[9].reuseJustification='even justified public inventory must be fresh'},
    {param($l) $l.roster.generatedRoots--}, {param($l) $l.roster.choices--},
    {param($l) $l.roster.suppressedLogicalPlacements=3}, {param($l) $l.roster.identityChanges=1},
    {param($l) $l.roster.protectedIdentitySetsEqual=$false}, {param($l) $l.roster.routeOrderingChanges=1},
    {param($l) $l.acceptedLimitations+=@('new gameplay waiver')},
    {param($l) $l.swallowedStatePersistenceExemption=$true},
    {param($l) $l.ownerVisualReview='APPROVED'},
    {param($l) $l.restoration.current='PENDING'}, {param($l) $l.restoration.workingFixtureClean=$false},
    {param($l) $l.publication.mergeContainsTestedSource=$false},
    {param($l) $l.publication.productMergeCommit=''}, {param($l) $l.publication.tag='v0.0.157'},
    {param($l) $l.publication.draft=$true}, {param($l) $l.publication.prerelease=$true},
    {param($l) $l.publication.latest=$false}
)) {
    $ledger=New-Sprint25Fixture
    & $change $ledger
    $rejected=$false
    try { Assert-KmgSprint25GiantBodyReleaseDecision -Ledger $ledger -Candidate $candidate } catch { $rejected=$true }
    if(-not $rejected) { throw 'Incomplete Sprint 25 qualification was accepted.' }
    $checks++
}
$ledger=New-Sprint25Fixture
$ledger.coverage[7].result='REUSED_PASS'
$ledger.coverage[7].priorSource=$candidate.commit
$ledger.coverage[7].reuseJustification='Unit fixture: unchanged dependencies, exact original execution identities retained.'
Assert-KmgSprint25GiantBodyReleaseDecision -Ledger $ledger -Candidate $candidate
$checks++
Write-Host "Sprint 25 release decision: PASS=$checks; source-derived counts and refusal fixtures only; no publication."
