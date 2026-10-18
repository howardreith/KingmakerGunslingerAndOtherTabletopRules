[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$actual=Get-KmgSprint26SourceSurface
if($actual.registeredRoots -ne ($actual.monsterPlacements+$actual.natureAllyPlacements) -or
    $actual.choices -ne ($actual.generatedRoots+$actual.nativeWrappers)) { throw 'Source surface does not reconcile.' }
# The decision unit fixture models publication without changing source or
# suppression. The production source reader above is exercised independently.
$script:Sprint26TestSurface=$actual | ConvertTo-Json | ConvertFrom-Json
$script:Sprint26TestSurface.generatedRoots=$actual.registeredRoots
$script:Sprint26TestSurface.choices=$actual.registeredRoots+$actual.nativeWrappers
$script:Sprint26TestSurface.suppressedLogicalPlacements=0
$script:Sprint26TestSurface.sprint26SuppressedPlacements=0
if(-not $actual.sprint26RosterExact -or $actual.sprint26Placements -ne 6) {throw 'The exact two-creature charter placements do not reconcile.'}
function Get-KmgSprint26SourceSurface { return $script:Sprint26TestSurface }
$candidate=[pscustomobject]@{version='0.0.160';commit=('a'*40);packageSha256=('b'*64);
    dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('d'*64)}
function New-Sprint26Fixture {
    $ids=@('composed-profiles-attacks-skills-reach-live-modifiers',
        'gorgon-staged-breath-trample-targeting-immunity-cure',
        'bulette-leap-charge-outcomes-interruption-economy','both-mode-commands-and-exact-owner-ai',
        'functional-bodies-icons-lifecycle-cache-donor-isolation',
        'six-public-roots-slots-quantity-duration-alignment',
        'targeted-persistence-condition-cooldown-on-off-cleanup-absence',
        'standalone-high-risk-shared-controls','inherited-unaffected-qualification',
        'complete-public-source-build-package-gate','public-inventory-icon-six-roots','exact-package-release-qualification')
    $roster=$script:Sprint26TestSurface | ConvertTo-Json | ConvertFrom-Json
    $roster | Add-Member -NotePropertyName identityChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName routeOrderingChanges -NotePropertyValue 0
    $roster | Add-Member -NotePropertyName protectedIdentitySetsEqual -NotePropertyValue $true
    [pscustomobject]@{schemaVersion=1;mission='Sprint26GorgonBulette';status='QUALIFIED';
        releaseEligible=$true;sprint27Started=$false;baselineExceptions=@();unresolvedFindings=@();
        ownerVisualReview='NOT_PERFORMED_NONBLOCKING';ownerVisualReviewEvidence=$null;
        swallowedStatePersistenceExemption=$false;petrificationPersistenceExemption=$false;leapMechanicsWaived=$false;
        charterChoices=[pscustomobject]@{gorgonMonsterVIIIExtension=$true;buletteNaturesAllyVI=$true;untemplated=$true;buletteSurfaceOnly=$true;printedBurrowFeet=20;burrowImplemented=$false;tremorsenseImplemented=$false};
        acceptedLimitations=@('ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD','SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED',
            'PASSIVE_CREATURE_SENSES_UNMODELED','RUN_NATIVE_SUMMON_AI_DISPOSITION');
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;
            pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=7;registered=7;unfiltered=$true}};
        roster=$roster;restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{productMergeCommit=('9'*40);mergeContainsTestedSource=$true;
            tag='v0.0.160';prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object {[pscustomobject]@{id=$_;result='PASS';reuseJustification=$null;priorSource=$null;
            proofs=@([pscustomobject]@{path='unit-fixture-only-not-native-evidence';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;packageSha256=$candidate.packageSha256;
                dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid})}})}
}
$checks=2
Assert-KmgSprint26MagicalBeastReleaseDecision -Ledger (New-Sprint26Fixture) -Candidate $candidate
foreach($change in @(
    {param($l) $l.releaseEligible=$false}, {param($l) $l.status='IN_PROGRESS'},
    {param($l) $l.sprint27Started=$true}, {param($l) $l.unresolvedFindings=@('unqualified Leap')},
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
    {param($l) $l.roster.sprint26SuppressedPlacements=2}, {param($l) $l.roster.identityChanges=1},
    {param($l) $l.roster.protectedIdentitySetsEqual=$false}, {param($l) $l.roster.routeOrderingChanges=1},
    {param($l) $l.acceptedLimitations+=@('new gameplay waiver')},
    {param($l) $l.swallowedStatePersistenceExemption=$true},
    {param($l) $l.petrificationPersistenceExemption=$true}, {param($l) $l.leapMechanicsWaived=$true},
    {param($l) $l.charterChoices.gorgonMonsterVIIIExtension=$false},
    {param($l) $l.charterChoices.buletteSurfaceOnly=$false},
    {param($l) $l.charterChoices.burrowImplemented=$true},
    {param($l) $l.charterChoices.tremorsenseImplemented=$true},
    {param($l) $l.roster.sprint26RosterExact=$false}, {param($l) $l.roster.sprint26Placements=7},
    {param($l) $l.ownerVisualReview='APPROVED'},
    {param($l) $l.restoration.current='PENDING'}, {param($l) $l.restoration.workingFixtureClean=$false},
    {param($l) $l.publication.mergeContainsTestedSource=$false},
    {param($l) $l.publication.productMergeCommit=''}, {param($l) $l.publication.tag='v0.0.157'},
    {param($l) $l.publication.draft=$true}, {param($l) $l.publication.prerelease=$true},
    {param($l) $l.publication.latest=$false}
)) {
    $ledger=New-Sprint26Fixture
    & $change $ledger
    $rejected=$false
    try { Assert-KmgSprint26MagicalBeastReleaseDecision -Ledger $ledger -Candidate $candidate } catch { $rejected=$true }
    if(-not $rejected) { throw 'Incomplete Sprint 26 qualification was accepted.' }
    $checks++
}
$ledger=New-Sprint26Fixture
$ledger.coverage[8].result='REUSED_PASS'
$ledger.coverage[8].priorSource=$candidate.commit
$ledger.coverage[8].reuseJustification='Unit fixture: unchanged dependencies, exact original execution identities retained.'
Assert-KmgSprint26MagicalBeastReleaseDecision -Ledger $ledger -Candidate $candidate
$checks++
$script:Sprint26TestSurface.suppressedLogicalPlacements=1
$script:Sprint26TestSurface.generatedRoots--;$script:Sprint26TestSurface.choices--
Assert-KmgSprint26MagicalBeastReleaseDecision -Ledger (New-Sprint26Fixture) -Candidate $candidate
$script:Sprint26TestSurface.suppressedLogicalPlacements=0
$script:Sprint26TestSurface.generatedRoots++;$script:Sprint26TestSurface.choices++
$checks++
function New-Sprint26PremergeFixture {
    $ledger=New-Sprint26Fixture
    $ledger.publication.productMergeCommit=$null
    $ledger.publication.mergeContainsTestedSource=$false
    $ledger.publication | Add-Member -NotePropertyName status -NotePropertyValue 'PENDING_PRODUCT_MERGE'
    $ledger.publication | Add-Member -NotePropertyName expectedBaseCommit -NotePropertyValue ('7'*40)
    $ledger.publication | Add-Member -NotePropertyName productPr -NotePropertyValue 58
    [pscustomobject]@{ledger=$ledger;head=$candidate.commit;base=('7'*40);expected=('7'*40);
        pr=[pscustomobject]@{state='OPEN';isDraft=$true;mergeable='MERGEABLE';baseRefName='master';
            baseRefOid=('7'*40);headRefOid=$candidate.commit}}
}
function Test-Sprint26PremergeInput($inputState) {
    Assert-KmgSprint26PremergeAuthority -Ledger $inputState.ledger -Candidate $candidate -PullRequest $inputState.pr `
        -RemoteProductHead $inputState.head -RemoteMaster $inputState.base -ExpectedBaseCommit $inputState.expected
}
Test-Sprint26PremergeInput (New-Sprint26PremergeFixture)
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
    $state=New-Sprint26PremergeFixture
    & $change $state
    $refused=$false
    try { Test-Sprint26PremergeInput $state } catch {$refused=$true}
    if(-not $refused){throw 'Invalid premerge authority was accepted.'}
    $checks++
}
$refused=$false
try { Assert-KmgSprint26MagicalBeastReleaseDecision -Ledger (New-Sprint26PremergeFixture).ledger -Candidate $candidate } catch {$refused=$true}
if(-not $refused){throw 'A truthful pending merge was accepted for publication.'}
$checks++
$mode=@{PreflightOnly=$true;Sprint26MagicalBeasts=$true;ExpectedBaseCommit=('7'*40);
    Publish=$false;ConfirmReleaseReady=$false;MergedReleaseCommit=''}
Assert-KmgQualifiedCandidateInvocationMode @mode
$checks++
foreach($change in @(
    {param($m) $m.Publish=$true}, {param($m) $m.ConfirmReleaseReady=$true},
    {param($m) $m.MergedReleaseCommit=('9'*40)}, {param($m) $m.Sprint26MagicalBeasts=$false},
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
Write-Host "Sprint 26 release decision: PASS=$checks; source-derived counts and refusal fixtures only; no publication."

# Exercise actual entrypoint forwarding/refusal before any network or release write.
foreach($entry in @('Publish-QualifiedCandidate.ps1','Publish-Release.ps1')) {
    $parameters=@{Sprint26MagicalBeasts=$true;PreflightOnly=$true;ExpectedBaseCommit=('7'*40);
        Publish=$true;PackagePath='absent-unit-fixture';CandidateRepositoryRoot='absent-unit-fixture';
        QualificationLedgerPath='absent-unit-fixture';ReleaseNotesPath='absent-unit-fixture';ProductPrNumber='58'}
    if($entry -ceq 'Publish-Release.ps1') {$parameters.ReuseQualifiedCandidate=$true}
    $rejected=$false
    try {& (Join-Path $PSScriptRoot $entry) @parameters} catch {
        if($_.Exception.Message -notlike 'PreflightOnly requires one supported sprint*') {throw}
        $rejected=$true
    }
    if(-not $rejected) {throw 'Preflight write reached the publisher.'};$checks++
}
Write-Host "Sprint 26 entrypoint refusals: total PASS=$checks; no native qualification or publication."
