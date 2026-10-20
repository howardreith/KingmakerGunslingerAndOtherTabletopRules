[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint28QualifiedRelease.Common.ps1')
$actual=Get-KmgSprint28SourceSurface
if(-not $actual.sprint28RosterExact -or $actual.sprint28Placements -ne 11 -or
    $actual.registeredRoots -ne ($actual.monsterPlacements+$actual.natureAllyPlacements) -or
    $actual.choices -ne ($actual.generatedRoots+$actual.nativeWrappers)) {
    throw 'Sprint 28 source reader does not reconcile the actual eleven-route registration.'
}
# Model publication without changing the source or activation state.
$script:publicSurface=$actual | ConvertTo-Json | ConvertFrom-Json
$script:publicSurface.generatedRoots=$actual.registeredRoots
$script:publicSurface.choices=$actual.registeredRoots+$actual.nativeWrappers
$script:publicSurface.suppressedLogicalPlacements=0
$script:publicSurface.sprint28SuppressedPlacements=0
function Get-KmgSprint28SourceSurface { return $script:publicSurface }
$candidate=[pscustomobject]@{version='0.0.162';commit=('a'*40);packageSha256=('b'*64);
    dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('d'*64)}
$ids=@('composed-adult-profiles-melee-rock-and-feats',
    'native-rock-range-catching-fire-damage-and-controls',
    'rtwp-and-turn-based-native-commands-and-ai',
    'owned-equipment-corpse-loot-and-functional-bodies-icons',
    'targeted-persistence-on-off-cleanup-fresh-absence',
    'eleven-public-roots-quantities-and-protected-identity',
    'standalone-and-high-risk-shared-controls',
    'inherited-unaffected-qualification',
    'complete-public-source-build-package-gate',
    'public-inventory-icon-eleven-roots','exact-package-release-qualification')
function New-Sprint28Fixture {
    $roster=$script:publicSurface | ConvertTo-Json | ConvertFrom-Json
    $roster | Add-Member identityChanges 0
    $roster | Add-Member routeOrderingChanges 0
    $roster | Add-Member protectedIdentitySetsEqual $true
    [pscustomobject]@{schemaVersion=1;mission='Sprint28Giants';status='QUALIFIED';
        releaseEligible=$true;sprint29Started=$false;expandedScope=$false;gameplayWaiver=$false;
        baselineExceptions=@();unresolvedFindings=@();
        skillDecision=[pscustomobject]@{ownerAuthorized=$true;
            authority='Owner Sprint 28 closeout directive 2026-10-20';
            disposition='DEFER_NAMED_CLIMB_CRAFT_SPRINT28';
            deferredSkillNames=@('Climb','Craft');
            affectedCreatures=@('hill-giant','stone-giant','fire-giant');
            printedValues=@('Hill Climb +10','Stone Climb +12',
                'Fire Climb +14','Fire Craft (any one) +8');
            nativePitBehavior='UNCHANGED';newStats=$false;
            rankReallocation=$false;substituteBonuses=$false;
            otherGameplayOmissions=@();
            disclosurePath='docs/RELEASE-NOTES-0.0.162.md'};
        ownerVisualReview='NOT_PERFORMED_NONBLOCKING';ownerVisualReviewEvidence=$null;
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;
            sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;
            fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=7;registered=7;unfiltered=$true}};
        roster=$roster;restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{status='PENDING_PRODUCT_MERGE';productPr=62;
            productMergeCommit=$null;mergeContainsTestedSource=$false;
            expectedBaseCommit=('7'*40);tag='v0.0.162';prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object {[pscustomobject]@{id=$_;result='PASS';
            reuseJustification=$null;priorSource=$null;
            proofs=@([pscustomobject]@{path='unit-fixture-only';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;
                packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;
                dllMvid=$candidate.dllMvid})}})}
}
function MustRefuse([string]$name,[scriptblock]$mutation,[switch]$Premerge,[switch]$ReleaseDecision) {
    $ledger=New-Sprint28Fixture
    & $mutation $ledger
    $refused=$false
    try {
        if($Premerge) { Assert-KmgSprint28PremergeAuthority -Ledger $ledger -Candidate $candidate -PullRequest $pr `
            -RemoteProductHead $candidate.commit -RemoteMaster ('7'*40) -ExpectedBaseCommit ('7'*40) }
        elseif($ReleaseDecision) { Assert-KmgSprint28GiantsReleaseDecision -Ledger $ledger -Candidate $candidate }
        else { Assert-KmgSprint28GiantsQualification -Ledger $ledger -Candidate $candidate }
    } catch { $refused=$true }
    if(-not $refused) { throw "Sprint 28 publisher accepted invalid $name." }
    $script:checks++
}
$checks=1
Assert-KmgSprint28GiantsQualification -Ledger (New-Sprint28Fixture) -Candidate $candidate
$checks++
$pr=[pscustomobject]@{baseRefOid=('7'*40);baseRefName='master';state='OPEN';
    isDraft=$true;mergeable='MERGEABLE';headRefOid=$candidate.commit}
Assert-KmgSprint28PremergeAuthority -Ledger (New-Sprint28Fixture) -Candidate $candidate `
    -PullRequest $pr -RemoteProductHead $candidate.commit -RemoteMaster ('7'*40) `
    -ExpectedBaseCommit ('7'*40)
$checks++
foreach($row in @(
    @('status',{param($l)$l.status='IN_PROGRESS'}),
    @('unresolved',{param($l)$l.unresolvedFindings=@('uncertain catch')}),
    @('gameplay waiver',{param($l)$l.gameplayWaiver=$true}),
    @('missing skill decision',{param($l)$l.skillDecision.ownerAuthorized=$false}),
    @('wrong decision authority',{param($l)$l.skillDecision.authority='unrecorded'}),
    @('broadened skill omission',{param($l)$l.skillDecision.deferredSkillNames=@('Climb','Craft','Swim')}),
    @('unrelated creature',{param($l)$l.skillDecision.affectedCreatures=@('hill-giant','stone-giant','fire-giant','frost-giant')}),
    @('wrong printed skill',{param($l)$l.skillDecision.printedValues[0]='Hill Climb +11'}),
    @('pit behavior changed',{param($l)$l.skillDecision.nativePitBehavior='CHANGED'}),
    @('substitute skill bonus',{param($l)$l.skillDecision.substituteBonuses=$true}),
    @('other omitted behavior',{param($l)$l.skillDecision.otherGameplayOmissions=@('Cleave')}),
    @('missing disclosure',{param($l)$l.skillDecision.disclosurePath=''}),
    @('scope',{param($l)$l.expandedScope=$true}),
    @('source',{param($l)$l.candidate.source=('f'*40)}),
    @('ZIP',{param($l)$l.candidate.zipSha256=('f'*64)}),
    @('DLL',{param($l)$l.candidate.dllSha256=('f'*64)}),
    @('source fingerprint',{param($l)$l.candidate.sourceFingerprint=('f'*64)}),
    @('suite',{param($l)$l.candidate.fullSuite.failures=1}),
    @('public suppression',{param($l)$l.roster.sprint28SuppressedPlacements=11}),
    @('protected identities',{param($l)$l.roster.protectedIdentitySetsEqual=$false}),
    @('working fixture',{param($l)$l.restoration.workingFixtureClean=$false}),
    @('owner visual claim',{param($l)$l.ownerVisualReview='APPROVED'}),
    @('missing coverage',{param($l)$l.coverage=@($l.coverage | Where-Object {$_.id -cne 'exact-package-release-qualification'})}),
    @('reused mandatory fresh',{param($l)$l.coverage[-1].result='REUSED_PASS'}),
    @('reused changed giant mechanics',{param($l)$l.coverage[0].result='REUSED_PASS'}),
    @('wrong fresh bytes',{param($l)$l.coverage[0].evidenceSources[0].packageSha256=('f'*64)}),
    @('missing proof',{param($l)$l.coverage[0].proofs=@()}))) {
    MustRefuse -name $row[0] -mutation $row[1]
}
MustRefuse -name 'wrong product PR' -Premerge -mutation {param($l)$l.publication.productPr=63}
MustRefuse -name 'claimed premerge' -Premerge -mutation {param($l)$l.publication.mergeContainsTestedSource=$true}
MustRefuse -name 'wrong pinned base' -Premerge -mutation {param($l)$l.publication.expectedBaseCommit=('8'*40)}
$merged=New-Sprint28Fixture
$merged.publication.productMergeCommit=('9'*40)
$merged.publication.mergeContainsTestedSource=$true
Assert-KmgSprint28GiantsReleaseDecision -Ledger $merged -Candidate $candidate
$checks++
MustRefuse -name 'missing product merge' -ReleaseDecision -mutation {param($l) $l.publication.productMergeCommit=$null}
$mode=@{PreflightOnly=$true;Sprint28Giants=$true;ExpectedBaseCommit=('7'*40)}
Assert-KmgQualifiedCandidateInvocationMode @mode
$checks++
$mode.Publish=$true
$refused=$false
try { Assert-KmgQualifiedCandidateInvocationMode @mode } catch { $refused=$true }
if(-not $refused) { throw 'Sprint 28 preflight accepted publication.' }
$checks++
$refused=$false
try {
    & (Join-Path $PSScriptRoot 'Publish-Release.ps1') -ReuseQualifiedCandidate `
        -Sprint28Giants -PreflightOnly -ExpectedBaseCommit ('7'*40) `
        -ProductPrNumber '63' -PackagePath 'missing-sprint28.zip' `
        -CandidateRepositoryRoot (Split-Path -Parent $PSScriptRoot) `
        -QualificationLedgerPath 'missing-sprint28-ledger.json'
} catch {
    $refused=$_.Exception.Message -like '*product PR 62*'
}
if(-not $refused) { throw 'Sprint 28 entrypoint did not refuse the wrong PR before promotion.' }
$checks++
"Sprint 28 qualified-release refusal fixtures: $checks passed."
