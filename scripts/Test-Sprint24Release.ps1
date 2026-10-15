[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$candidate=[pscustomobject]@{commit=('a'*40);packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('d'*64)}
function New-Sprint24Fixture {
    $ids=@('shared-controls-released-consumers-and-exact-ai',
        'targeted-enabled-cold-load-cleanup-absence','module-disabled-cold-load-reenable',
        'complete-public-source-build-package-gate','public-inventory-high-risk-combined',
        'public-eighteen-roots-real-parent-paths','exact-package-release-qualification')
    [pscustomobject]@{schemaVersion=1;mission='Sprint24DeinonychusTyrannosaurus';status='QUALIFIED';
        releaseEligible=$true;sprint25Started=$false;baselineExceptions=@();unresolvedFindings=@();
        swallowedStatePersistenceExemption=$false;
        acceptedLimitations=@('ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD',
            'SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED','PASSIVE_CREATURE_SENSES_UNMODELED',
            'RUN_NATIVE_SUMMON_AI_DISPOSITION');
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;
            pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=2610;registered=2610;unfiltered=$true}};
        roster=[pscustomobject]@{projectCreatures=107;monsterCreatures=95;monsterPlacements=543;
            natureAllyCreatures=95;natureAllyPlacements=550;generatedRoots=1093;nativeWrappers=29;choices=1122;
            suppressedLogicalPlacements=0;identityChanges=0;routeOrderingChanges=0;protectedIdentitySetsEqual=$true};
        restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        publication=[pscustomobject]@{productMergeCommit=('9'*40);mergeContainsTestedSource=$true;
            tag='v0.0.155';prerelease=$false;draft=$false;latest=$true};
        coverage=@($ids | ForEach-Object { [pscustomobject]@{id=$_;result='PASS';proofs=@([pscustomobject]@{path='local-immutable-proof';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid})} })}
}
$checks=1
Assert-KmgSprint24TheropodReleaseDecision -Ledger (New-Sprint24Fixture) -Candidate $candidate
foreach($change in @(
    # Unfinished or unqualified sprint.
    {param($l) $l.releaseEligible=$false}, {param($l) $l.status='IN_PROGRESS'},
    {param($l) $l.unresolvedFindings=@('shared-controls row failed')},
    {param($l) $l.sprint25Started=$true}, {param($l) $l.baselineExceptions=@('waived cold load')},
    # Wrong artifact.
    {param($l) $l.candidate.dllSha256=('f'*64)}, {param($l) $l.candidate.zipSha256=('f'*64)},
    {param($l) $l.candidate.dllMvid='different'}, {param($l) $l.candidate.sourceFingerprint=('f'*64)},
    {param($l) $l.candidate.source=('9'*40)},
    # Stale or failed proof.
    {param($l) $l.candidate.pipelineExitCode=1}, {param($l) $l.candidate.fullSuite.failures=1},
    {param($l) $l.candidate.fullSuite.executed=0},
    {param($l) $l.candidate.fullSuite.registered=2611},
    {param($l) $l.candidate.fullSuite.unfiltered=$false},
    {param($l) $l.coverage[0].evidenceSources[0].dllMvid='different'},
    {param($l) $l.coverage[0].evidenceSources[0].packageSha256=('f'*64)},
    {param($l) $l.coverage[0].proofs=@()}, {param($l) $l.coverage[0].evidenceSources=@()},
    {param($l) $l.coverage[1].result='REUSED_PASS'},
    {param($l) $l.coverage=$l.coverage[1..6]},
    {param($l) $l.coverage[2].id=$l.coverage[1].id},
    # Publication surface or suppression state.
    {param($l) $l.roster.generatedRoots=1075}, {param($l) $l.roster.choices=1104},
    {param($l) $l.roster.suppressedLogicalPlacements=18},
    {param($l) $l.roster.projectCreatures=106}, {param($l) $l.roster.monsterPlacements=534},
    {param($l) $l.roster.natureAllyPlacements=541}, {param($l) $l.roster.identityChanges=1},
    {param($l) $l.roster.routeOrderingChanges=1}, {param($l) $l.roster.protectedIdentitySetsEqual=$false},
    # New waiver or a revived swallowed-state exemption.
    {param($l) $l.acceptedLimitations=@($l.acceptedLimitations)+'SWALLOWED_STATE_SURVIVES_RELOAD'},
    {param($l) $l.acceptedLimitations=@($l.acceptedLimitations[0..2])},
    {param($l) $l.swallowedStatePersistenceExemption=$true},
    # Restoration.
    {param($l) $l.restoration.workingFixtureClean=$false},
    {param($l) $l.restoration.current='PARTIAL'},
    # Wrong merge lineage or invalid publication state.
    {param($l) $l.publication.mergeContainsTestedSource=$false},
    {param($l) $l.publication.productMergeCommit=''},
    {param($l) $l.publication.tag='v0.0.154'},
    {param($l) $l.publication.draft=$true},
    {param($l) $l.publication.prerelease=$true},
    {param($l) $l.publication.latest=$false}
)) {
    $l=New-Sprint24Fixture;& $change $l;$rejected=$false
    try { Assert-KmgSprint24TheropodReleaseDecision -Ledger $l -Candidate $candidate } catch {$rejected=$true}
    if(-not $rejected) { throw 'Incomplete Sprint24 qualification was accepted.' };$checks++
}
Write-Host "Sprint24 release decision safety passed: $checks checks; no publication."
