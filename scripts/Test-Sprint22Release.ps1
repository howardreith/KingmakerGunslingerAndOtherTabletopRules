[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$candidate=[pscustomobject]@{commit=('a'*40);packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='21d0319e-eb2f-4d5e-a3d6-ecf4c9fd46a5';sourceStateSha256=('d'*64)}
function New-Sprint22Fixture {
    $ids=@('ankylosaurus-profile-carriers-and-ten-routes','tail-rider-rtwp-and-turn-based','original-body-contact-huge-crowd',
        'private-resource-teardown-and-fallback','targeted-enabled-cold-load-cleanup-absence','module-disabled-cold-load-reenable',
        'gunslinger-only','high-risk-combined','exact-package-release-qualification')
    [pscustomobject]@{schemaVersion=1;mission='Sprint22Ankylosaurus';status='QUALIFIED';releaseEligible=$true;sprint23Started=$false;
        baselineExceptions=@();unresolvedFindings=@();candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;
            fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=2580;registered=2580;unfiltered=$true}};
        roster=[pscustomobject]@{generatedRoots=1063;nativeWrappers=29;choices=1092;preservedAnkylosaurusRoots=10;protectedIdentitySetsEqual=$true};
        restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        coverage=@($ids | ForEach-Object { [pscustomobject]@{id=$_;result='PASS';proofs=@([pscustomobject]@{path='local-immutable-proof';sha256=('e'*64)});
            evidenceSources=@([pscustomobject]@{source=$candidate.commit;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid})} })}
}
$checks=1
Assert-KmgSprint22ReleaseDecision -Ledger (New-Sprint22Fixture) -Candidate $candidate
foreach($change in @(
    {param($l) $l.releaseEligible=$false}, {param($l) $l.unresolvedFindings=@('failed rider')},
    {param($l) $l.sprint23Started=$true}, {param($l) $l.candidate.dllSha256=('f'*64)},
    {param($l) $l.candidate.fullSuite.executed=0}, {param($l) $l.candidate.fullSuite.failures=1},
    {param($l) $l.restoration.workingFixtureClean=$false}, {param($l) $l.coverage[1].result='REUSED_PASS'},
    {param($l) $l.coverage[0].evidenceSources[0].dllMvid='different'}, {param($l) $l.roster.protectedIdentitySetsEqual=$false},
    {param($l) $l.coverage[0].proofs=@()}, {param($l) $l.coverage=$l.coverage[1..8]}
)) {
    $l=New-Sprint22Fixture;& $change $l;$rejected=$false
    try { Assert-KmgSprint22ReleaseDecision -Ledger $l -Candidate $candidate } catch {$rejected=$true}
    if(-not $rejected) { throw 'Incomplete Sprint22 qualification was accepted.' };$checks++
}
Write-Host "Sprint22 release decision safety passed: $checks checks; no publication."
