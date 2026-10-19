[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$actual=Get-KmgSprint27SourceSurface
if($actual.registeredRoots -ne ($actual.monsterPlacements+$actual.natureAllyPlacements) -or
    $actual.choices -ne ($actual.generatedRoots+$actual.nativeWrappers)) { throw 'Source surface does not reconcile.' }
# The decision unit fixture models publication without changing source or
# suppression. The production source reader above is exercised independently.
$script:Sprint27TestSurface=$actual | ConvertTo-Json | ConvertFrom-Json
$script:Sprint27TestSurface.generatedRoots=$actual.registeredRoots
$script:Sprint27TestSurface.choices=$actual.registeredRoots+$actual.nativeWrappers
$script:Sprint27TestSurface.suppressedLogicalPlacements=0
$script:Sprint27TestSurface.sprint27SuppressedPlacements=0
if(-not $actual.sprint27RosterExact -or $actual.sprint27Placements -ne 6) {throw 'The exact one-creature charter placements do not reconcile.'}
function Get-KmgSprint27SourceSurface { return $script:Sprint27TestSurface }
$candidate=[pscustomobject]@{version='0.0.161';commit=('a'*40);packageSha256=('b'*64);
    dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('d'*64)}
function New-Sprint27Fixture {
    $ids=@('composed-profile-ordinary-signed-damage-reach-movement',
        'rtwp-and-turn-based-native-pounce-rake','charge-negative-owner-target-state-and-reactions',
        'exact-owner-ai-and-functional-hybrid-body','lifecycle-cache-donor-rollback-module-boundary',
        'targeted-persistence-on-off-cleanup-fresh-absence',
        'six-public-roots-slots-quantities-duration-alignment-icons',
        'standalone-and-high-risk-shared-controls','inherited-unaffected-qualification',
        'complete-public-source-build-package-gate','public-inventory-icon-six-roots','exact-package-release-qualification')
    $roster=$script:Sprint27TestSurface | ConvertTo-Json | ConvertFrom-Json
    $roster | Add-Member -NotePropertyName identityChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName routeOrderingChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName protectedIdentitySetsEqual -NotePropertyValue $true
    [pscustomobject]@{schemaVersion=1;mission='Sprint27Griffon';status='QUALIFIED';
        releaseEligible=$true;sprint28Started=$false;baselineExceptions=@();unresolvedFindings=@();
        ownerVisualReview='NOT_PERFORMED_NONBLOCKING';ownerVisualReviewEvidence=$null;
        griffonMechanicsWaived=$false;persistenceExemption=$false;
        charterChoices=[pscustomobject]@{griffonNaturesAllyIVOnly=$true;untemplated=$true;noAutomaticGrab=$true;
            effectiveAerialSpeedFeet=80;printedLandSpeedFeet=30;separateLandModeOmitted=$true;omittedFlyRanks=3;
            newGrappleFramework=$false;zAxisFlight=$false;wingOrTailAttacks=$false;expandedScope=$false};
        acceptedLimitations=@('ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD',
            'PASSIVE_CREATURE_SENSES_UNMODELED','RUN_NATIVE_SUMMON_AI_DISPOSITION');
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;
            pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=7;registered=7;unfiltered=$true}};
        roster=$roster;restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{productMergeCommit=('9'*40);mergeContainsTestedSource=$true;
            tag='v0.0.161';prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object {[pscustomobject]@{id=$_;result='PASS';reuseJustification=$null;priorSource=$null;
            proofs=@([pscustomobject]@{path='unit-fixture-only-not-native-evidence';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;packageSha256=$candidate.packageSha256;
                dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid})}})}
}
$checks=2
Assert-KmgSprint27GriffonReleaseDecision -Ledger (New-Sprint27Fixture) -Candidate $candidate
foreach($change in @(
    {param($l) $l.releaseEligible=$false}, {param($l) $l.status='IN_PROGRESS'},
    {param($l) $l.sprint28Started=$true}, {param($l) $l.unresolvedFindings=@('unqualified pounce')},
    {param($l) $l.baselineExceptions=@('new waiver')},
    {param($l) $l.candidate.source=('f'*40)}, {param($l) $l.candidate.zipSha256=('f'*64)},
    {param($l) $l.candidate.dllSha256=('f'*64)}, {param($l) $l.candidate.dllMvid='different'},
    {param($l) $l.candidate.sourceFingerprint=('f'*64)},
    {param($l) $l.candidate.fullSuite.failures=1}, {param($l) $l.candidate.pipelineExitCode=1},
    {param($l) $l.candidate.fullSuite.executed=0}, {param($l) $l.candidate.fullSuite.registered=8},
    {param($l) $l.candidate.fullSuite.unfiltered=$false},
    {param($l) $l.coverage=$l.coverage[1..11]}, {param($l) $l.coverage[1].id=$l.coverage[0].id},
    {param($l) $l.coverage[0].result='FAIL'}, {param($l) $l.coverage[0].proofs=@()},
    {param($l) $l.coverage[0].evidenceSources=@()},
    {param($l) $l.coverage[0].evidenceSources[0].dllMvid='different'},
    {param($l) $l.coverage[0].result='REUSED_PASS'},
    {param($l) $l.coverage[10].result='REUSED_PASS';$l.coverage[10].priorSource=$candidate.commit;$l.coverage[10].reuseJustification='even justified public inventory must be fresh'},
    {param($l) $l.roster.generatedRoots--}, {param($l) $l.roster.choices--},
    {param($l) $l.roster.sprint27SuppressedPlacements=2}, {param($l) $l.roster.identityChanges=1},
    {param($l) $l.roster.protectedIdentitySetsEqual=$false}, {param($l) $l.roster.routeOrderingChanges=1},
    {param($l) $l.acceptedLimitations+=@('new gameplay waiver')},
    {param($l) $l.griffonMechanicsWaived=$true}, {param($l) $l.persistenceExemption=$true},
    {param($l) $l.charterChoices.griffonNaturesAllyIVOnly=$false},
    {param($l) $l.charterChoices.noAutomaticGrab=$false}, {param($l) $l.charterChoices.effectiveAerialSpeedFeet=110},
    {param($l) $l.charterChoices.printedLandSpeedFeet=80}, {param($l) $l.charterChoices.separateLandModeOmitted=$false},
    {param($l) $l.charterChoices.omittedFlyRanks=0}, {param($l) $l.charterChoices.newGrappleFramework=$true},
    {param($l) $l.charterChoices.zAxisFlight=$true}, {param($l) $l.charterChoices.wingOrTailAttacks=$true},
    {param($l) $l.charterChoices.expandedScope=$true},
    {param($l) $l.roster.sprint27RosterExact=$false}, {param($l) $l.roster.sprint27Placements=7},
    {param($l) $l.ownerVisualReview='APPROVED'},
    {param($l) $l.restoration.current='PENDING'}, {param($l) $l.restoration.workingFixtureClean=$false},
    {param($l) $l.publication.mergeContainsTestedSource=$false},
    {param($l) $l.publication.productMergeCommit=''}, {param($l) $l.publication.tag='v0.0.157'},
    {param($l) $l.publication.draft=$true}, {param($l) $l.publication.prerelease=$true},
    {param($l) $l.publication.latest=$false}
)) {
    $ledger=New-Sprint27Fixture
    & $change $ledger
    $rejected=$false
    try { Assert-KmgSprint27GriffonReleaseDecision -Ledger $ledger -Candidate $candidate } catch { $rejected=$true }
    if(-not $rejected) { throw 'Incomplete Sprint 27 qualification was accepted.' }
    $checks++
}
$ledger=New-Sprint27Fixture
$ledger.coverage[8].result='REUSED_PASS'
$ledger.coverage[8].priorSource=$candidate.commit
$ledger.coverage[8].reuseJustification='Unit fixture: unchanged dependencies, exact original execution identities retained.'
Assert-KmgSprint27GriffonReleaseDecision -Ledger $ledger -Candidate $candidate
$checks++
$script:Sprint27TestSurface.suppressedLogicalPlacements=1
$script:Sprint27TestSurface.generatedRoots--;$script:Sprint27TestSurface.choices--
Assert-KmgSprint27GriffonReleaseDecision -Ledger (New-Sprint27Fixture) -Candidate $candidate
$script:Sprint27TestSurface.suppressedLogicalPlacements=0
$script:Sprint27TestSurface.generatedRoots++;$script:Sprint27TestSurface.choices++
$checks++
function New-Sprint27PremergeFixture {
    $ledger=New-Sprint27Fixture
    $ledger.publication.productMergeCommit=$null
    $ledger.publication.mergeContainsTestedSource=$false
    $ledger.publication | Add-Member -NotePropertyName status -NotePropertyValue 'PENDING_PRODUCT_MERGE'
    $ledger.publication | Add-Member -NotePropertyName expectedBaseCommit -NotePropertyValue ('7'*40)
    $ledger.publication | Add-Member -NotePropertyName productPr -NotePropertyValue 60
    [pscustomobject]@{ledger=$ledger;head=$candidate.commit;base=('7'*40);expected=('7'*40);
        pr=[pscustomobject]@{state='OPEN';isDraft=$true;mergeable='MERGEABLE';baseRefName='master';
            baseRefOid=('7'*40);headRefOid=$candidate.commit}}
}
function Test-Sprint27PremergeInput($inputState) {
    Assert-KmgSprint27PremergeAuthority -Ledger $inputState.ledger -Candidate $candidate -PullRequest $inputState.pr `
        -RemoteProductHead $inputState.head -RemoteMaster $inputState.base -ExpectedBaseCommit $inputState.expected
}
Test-Sprint27PremergeInput (New-Sprint27PremergeFixture)
$checks++
foreach($change in @(
    {param($s) $s.pr.state='MERGED'}, {param($s) $s.pr.isDraft=$false},
    {param($s) $s.pr.mergeable='CONFLICTING'}, {param($s) $s.pr.baseRefName='other'},
    {param($s) $s.pr.baseRefOid=('8'*40)}, {param($s) $s.pr.headRefOid=('8'*40)},
    {param($s) $s.head=('8'*40)}, {param($s) $s.base=('8'*40)},
    {param($s) $s.expected='not-a-commit'},
    {param($s) $s.ledger.publication.productMergeCommit=('9'*40)},
    {param($s) $s.ledger.publication.mergeContainsTestedSource=$true},
    {param($s) $s.ledger.publication.status='RELEASED'},
    {param($s) $s.ledger.publication.expectedBaseCommit=('8'*40)},
    {param($s) $s.ledger.publication.productPr=57},
    {param($s) $s.ledger.coverage[0].result='FAIL'}
)) {
    $state=New-Sprint27PremergeFixture
    & $change $state
    $refused=$false
    try { Test-Sprint27PremergeInput $state } catch {$refused=$true}
    if(-not $refused){throw 'Invalid premerge authority was accepted.'}
    $checks++
}
$refused=$false
try { Assert-KmgSprint27GriffonReleaseDecision -Ledger (New-Sprint27PremergeFixture).ledger -Candidate $candidate } catch {$refused=$true}
if(-not $refused){throw 'A truthful pending merge was accepted for publication.'}
$checks++
$mode=@{PreflightOnly=$true;Sprint27Griffon=$true;ExpectedBaseCommit=('7'*40);
    Publish=$false;ConfirmReleaseReady=$false;MergedReleaseCommit=''}
Assert-KmgQualifiedCandidateInvocationMode @mode
$checks++
foreach($change in @(
    {param($m) $m.Publish=$true}, {param($m) $m.ConfirmReleaseReady=$true},
    {param($m) $m.MergedReleaseCommit=('9'*40)}, {param($m) $m.Sprint27Griffon=$false},
    {param($m) $m.ExpectedBaseCommit=''}, {param($m) $m.ExpectedBaseCommit='bad'},
    {param($m) $m.PreflightOnly=$false;$m.MergedReleaseCommit=('9'*40)},
    {param($m) $m.PreflightOnly=$false;$m.ExpectedBaseCommit=''}
)) {
    $inputMode=$mode.Clone(); & $change $inputMode
    $refused=$false
    try {Assert-KmgQualifiedCandidateInvocationMode @inputMode} catch {$refused=$true}
    if(-not $refused){throw 'Invalid preflight/publication mode was accepted.'}
    $checks++
}
Assert-KmgQualifiedCandidateInvocationMode -MergedReleaseCommit ('9'*40)
$checks++
Write-Host "Sprint 27 release decision: PASS=$checks; source-derived counts and refusal fixtures only; no publication."

# Exercise actual entrypoint forwarding/refusal before any network or release write.
foreach($entry in @('Publish-QualifiedCandidate.ps1','Publish-Release.ps1')) {
    $parameters=@{Sprint27Griffon=$true;PreflightOnly=$true;ExpectedBaseCommit=('7'*40);
        Publish=$true;PackagePath='absent-unit-fixture';CandidateRepositoryRoot='absent-unit-fixture';
        QualificationLedgerPath='absent-unit-fixture';ReleaseNotesPath='absent-unit-fixture';ProductPrNumber='60'}
    if($entry -ceq 'Publish-Release.ps1') {$parameters.ReuseQualifiedCandidate=$true}
    $rejected=$false
    try {& (Join-Path $PSScriptRoot $entry) @parameters} catch {
        if($_.Exception.Message -notlike 'PreflightOnly requires one supported sprint*') {throw}
        $rejected=$true
    }
    if(-not $rejected) {throw 'Preflight write reached the publisher.'};$checks++
}
Write-Host "Sprint 27 entrypoint refusals: total PASS=$checks; no native qualification or publication."
