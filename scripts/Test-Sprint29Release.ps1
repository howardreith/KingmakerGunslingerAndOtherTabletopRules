[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
$actual=Get-KmgSprint29SourceSurface
if(-not $actual.sprint29RosterExact -or $actual.sprint29Placements -ne 8 -or
    $actual.registeredRoots -ne ($actual.monsterPlacements+$actual.natureAllyPlacements) -or
    $actual.choices -ne ($actual.generatedRoots+$actual.nativeWrappers) -or
    $actual.suppressedLogicalPlacements -ne 0) {
    throw 'Sprint 29 source reader does not reconcile the actual public registration.'
}
$script:publicSurface=$actual | ConvertTo-Json | ConvertFrom-Json
function Get-KmgSprint29SourceSurface { return $script:publicSurface }
$notesPath=Join-Path (Split-Path -Parent $PSScriptRoot) 'docs/RELEASE-NOTES-0.0.163.md'
$notes=[IO.File]::ReadAllText($notesPath)
$candidate=[pscustomobject]@{version='0.0.163';commit=('a'*40);packageSha256=('b'*64);
    dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';
    sourceStateSha256=('d'*64)}
$fresh=@('public-eight-native-spellbook-routes',
    'public-inventory-icons-and-body-visibility',
    'affected-giant-melee-ranged-catch-cleave-vital-controls',
    'cloud-fog-storm-lightning-and-ai-lifetimes',
    'ettin-two-weapon-ammunition-owned-equipment',
    'targeted-on-off-cold-load-cleanup-and-absence',
    'standalone-and-high-risk-combined',
    'complete-public-source-build-package-gate',
    'exact-package-release-qualification')
$ids=@('inherited-unaffected-qualification')+$fresh
function New-Sprint29Fixture {
    $roster=$script:publicSurface | ConvertTo-Json | ConvertFrom-Json
    $roster | Add-Member identityChanges 0
    $roster | Add-Member routeOrderingChanges 0
    $roster | Add-Member protectedIdentitySetsEqual $true
    [pscustomobject]@{schemaVersion=1;mission='Sprint29Giants';status='QUALIFIED';
        releaseEligible=$true;sprint30Started=$false;expandedScope=$false;gameplayWaiver=$false;
        baselineExceptions=@();unresolvedFindings=@();
        scopeDecisions=[pscustomobject]@{
            ownerAuthority='Sprint 29 mission and Awesome Blow owner decision 2026-10-21';
            skills='DEFER_NAMED_UNSUPPORTED_CONSUMERS';
            movement='DEFER_UNSUPPORTED_VERTICAL_AQUATIC';
            weather='FILTER_CONTROL_WEATHER';
            awesomeBlowWall='TARGET_COLLISION_1D6_ONLY_STATIC_WALL_NO_HP';
            nativePitUnchanged=$true;rankReallocation=$false;
            substituteBonuses=$false;signatureCombatMagicRequired=$true;
            otherGameplayOmissions=@();disclosurePath='docs/RELEASE-NOTES-0.0.163.md'};
        ownerVisualReview='NOT_PERFORMED_NONBLOCKING';ownerVisualReviewEvidence=$null;
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;
            sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;
            fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=7;registered=7;unfiltered=$true}};
        roster=$roster;restoration=[pscustomobject]@{
            current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{status='PENDING_PRODUCT_MERGE';productPr=64;
            productMergeCommit=$null;mergeContainsTestedSource=$false;
            expectedBaseCommit=('7'*40);tag='v0.0.163';
            prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object {[pscustomobject]@{
            id=$_;result=$(if($_ -ceq 'inherited-unaffected-qualification'){'REUSED_PASS'}else{'PASS'});
            reuseJustification=$(if($_ -ceq 'inherited-unaffected-qualification'){'No changed dependency'}else{$null});
            priorSource=$(if($_ -ceq 'inherited-unaffected-qualification'){('f'*40)}else{$null});
            proofs=@([pscustomobject]@{path='unit-fixture-only';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;
                packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;
                dllMvid=$candidate.dllMvid})}})}
}
function MustRefuse([string]$name,[scriptblock]$mutation,[switch]$Premerge,[switch]$ReleaseDecision) {
    $ledger=New-Sprint29Fixture
    & $mutation $ledger
    $refused=$false
    try {
        if($Premerge) {
            Assert-KmgSprint29PremergeAuthority -Ledger $ledger -Candidate $candidate -Notes $notes -PullRequest $pr `
                -RemoteProductHead $candidate.commit -RemoteMaster ('7'*40) -ExpectedBaseCommit ('7'*40)
        } elseif($ReleaseDecision) {
            Assert-KmgSprint29GiantsReleaseDecision -Ledger $ledger -Candidate $candidate -Notes $notes
        } else {
            Assert-KmgSprint29GiantsQualification -Ledger $ledger -Candidate $candidate -Notes $notes
        }
    } catch { $refused=$true }
    if(-not $refused) { throw "Sprint 29 publisher accepted invalid $name." }
    $script:checks++
}
$checks=0
Assert-KmgSprint29GiantsQualification -Ledger (New-Sprint29Fixture) -Candidate $candidate -Notes $notes
$checks++
$pr=[pscustomobject]@{baseRefOid=('7'*40);baseRefName='master';state='OPEN';
    isDraft=$true;mergeable='MERGEABLE';headRefOid=$candidate.commit}
Assert-KmgSprint29PremergeAuthority -Ledger (New-Sprint29Fixture) -Candidate $candidate -Notes $notes `
    -PullRequest $pr -RemoteProductHead $candidate.commit -RemoteMaster ('7'*40) -ExpectedBaseCommit ('7'*40)
$checks++
foreach($row in @(
    @('status',{param($l)$l.status='IN_PROGRESS'}),
    @('unresolved',{param($l)$l.unresolvedFindings=@('unknown fog lifecycle')}),
    @('blanket waiver',{param($l)$l.gameplayWaiver=$true}),
    @('scope authority',{param($l)$l.scopeDecisions.ownerAuthority='unrecorded'}),
    @('extra omission',{param($l)$l.scopeDecisions.otherGameplayOmissions=@('Chain Lightning')}),
    @('missing target collision',{param($l)$l.scopeDecisions.awesomeBlowWall='NO_DAMAGE'}),
    @('changed pit',{param($l)$l.scopeDecisions.nativePitUnchanged=$false}),
    @('substitute bonus',{param($l)$l.scopeDecisions.substituteBonuses=$true}),
    @('required magic waived',{param($l)$l.scopeDecisions.signatureCombatMagicRequired=$false}),
    @('source',{param($l)$l.candidate.source=('f'*40)}),
    @('ZIP',{param($l)$l.candidate.zipSha256=('f'*64)}),
    @('suite',{param($l)$l.candidate.fullSuite.failures=1}),
    @('public suppression',{param($l)$l.roster.sprint29SuppressedPlacements=8}),
    @('protected identities',{param($l)$l.roster.protectedIdentitySetsEqual=$false}),
    @('working fixture',{param($l)$l.restoration.workingFixtureClean=$false}),
    @('false visual approval',{param($l)$l.ownerVisualReview='APPROVED'}),
    @('missing coverage',{param($l)$l.coverage=@($l.coverage | Where-Object {$_.id -cne 'exact-package-release-qualification'})}),
    @('reused mandatory fresh',{param($l)$l.coverage[-1].result='REUSED_PASS'}),
    @('wrong fresh bytes',{param($l)$l.coverage[1].evidenceSources[0].packageSha256=('f'*64)}),
    @('missing proof',{param($l)$l.coverage[1].proofs=@()}))) {
    MustRefuse -name $row[0] -mutation $row[1]
}
MustRefuse -name 'wrong product PR' -Premerge -mutation {param($l)$l.publication.productPr=63}
MustRefuse -name 'claimed premerge' -Premerge -mutation {param($l)$l.publication.mergeContainsTestedSource=$true}
MustRefuse -name 'wrong pinned base' -Premerge -mutation {param($l)$l.publication.expectedBaseCommit=('8'*40)}
$merged=New-Sprint29Fixture
$merged.publication.productMergeCommit=('9'*40)
$merged.publication.mergeContainsTestedSource=$true
Assert-KmgSprint29GiantsReleaseDecision -Ledger $merged -Candidate $candidate -Notes $notes
$checks++
MustRefuse -name 'missing product merge' -ReleaseDecision -mutation {param($l)$l.publication.productMergeCommit=$null}
Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint29Giants -ExpectedBaseCommit ('7'*40)
$checks++
$refused=$false
try { Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint29Giants -Publish -ExpectedBaseCommit ('7'*40) } catch { $refused=$true }
if(-not $refused) { throw 'Sprint 29 preflight accepted publication.' }
$checks++
$refused=$false
try {
    & (Join-Path $PSScriptRoot 'Publish-Release.ps1') -ReuseQualifiedCandidate `
        -Sprint29Giants -PreflightOnly -ExpectedBaseCommit ('7'*40) `
        -ProductPrNumber '63' -PackagePath 'missing-sprint29.zip' `
        -CandidateRepositoryRoot (Split-Path -Parent $PSScriptRoot) `
        -QualificationLedgerPath 'missing-sprint29-ledger.json'
} catch { $refused=$_.Exception.Message -like '*product PR 64*' }
if(-not $refused) { throw 'Sprint 29 entrypoint did not refuse the wrong PR before promotion.' }
$checks++
"Sprint 29 qualified-release refusal fixtures: $checks passed."
