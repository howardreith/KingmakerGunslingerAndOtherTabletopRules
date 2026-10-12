[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
$candidate = [pscustomobject]@{commit=('a'*40);packageSha256=('b'*64);dllSha256=('c'*64);
    dllMvid='21d0319e-eb2f-4d5e-a3d6-ecf4c9fd46a5';sourceStateSha256=('d'*64)}
function New-QualifiedFixture {
    $ids = @('boundary-on','boundary-off','matched149-control','gunslinger-only','call-of-the-wild',
        'arms-armor','toggle-custom-soundpacks','high-risk-combined','required-module-states',
        'whole-roster-enabled-persistence','native-cleanup-save-and-fresh-absence',
        'module-disabled-deserialization-cleanup','sprint18-21-regressions','1063-roots-and-29-wrappers',
        'resource-lifecycle-template-initialization','exact-package-release-qualification')
    [pscustomobject]@{schemaVersion=1;releaseEligible=$true;status='QUALIFIED';
        candidate=[pscustomobject]@{source=$candidate.commit;zipSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;
            pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=2562;registered=2562;unfiltered=$true}};
        roster=[pscustomobject]@{generatedRoots=1063;nativeWrappers=29;choices=1092};
        restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};
        coverage=@($ids|ForEach-Object{[pscustomobject]@{id=$_;result='PASS';proofs=@([pscustomobject]@{path='fixture';sha256=('e'*64)})}});
        findings=@([pscustomobject]@{id='feature-module-brown-fur-publication-gate';classification='FIXTURE_DEFECT';native='VERIFIED'});
        baselineExceptions=@()}
}
$checks=0
foreach ($origin in @('git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git',
        'https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git',
        'https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules')) {
    Assert-KmgReleaseOrigin $origin
    $checks++
}
$refused=$false
try {Assert-KmgReleaseOrigin 'git@github.com:howardreith/KingmakerBuffPlanner.git'} catch {$refused=$true}
if(-not $refused){throw 'An unrelated repository origin was accepted.'}
$checks++
function Assert-Refused([scriptblock]$Change) {
    $ledger=New-QualifiedFixture
    & $Change $ledger
    $refused=$false
    try { Assert-KmgClosureReleaseDecision -Ledger $ledger -Candidate $candidate } catch {$refused=$true}
    if (-not $refused) { throw 'Unsafe qualification was accepted.' }
    $script:checks++
}
Assert-KmgClosureReleaseDecision -Ledger (New-QualifiedFixture) -Candidate $candidate
$checks++
Assert-Refused {param($l) $l.coverage[6].result='NOT_RUN'}
Assert-Refused {param($l) $l.coverage=$l.coverage[1..15]}
Assert-Refused {param($l) $l.candidate.zipSha256='f'*64}
Assert-Refused {param($l) $l.candidate.fullSuite.unfiltered=$false}
Assert-Refused {param($l) $l.candidate.fullSuite.executed=2554; $l.candidate.fullSuite.failures=5}
Assert-Refused {param($l) $l.restoration.workingFixtureClean=$false}
Assert-Refused {param($l) $l.findings[0].native='PENDING'}
Assert-Refused {param($l) $l.coverage[0].proofs=@()}
function New-BaselineException {
    [pscustomobject]@{id='teleportation-scroll-template-2c283c993b233df53b487fc7eeac2ba3';rawStatus='FAIL';
        baselineVersion='0.0.149';matchedClauseOrRows=$true;dependenciesUnchanged=$true;
        outsideSprint21AndSafe=$true;downstreamCoverageComplete=$true;knownIssue='tracked-issue';followUp='separate-follow-up'}
}
$qualified=New-QualifiedFixture
$qualified.status='QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS'
$qualified.baselineExceptions=@(New-BaselineException)
$qualified.coverage[0].result='BASELINE_EXCEPTION'
Assert-KmgClosureReleaseDecision -Ledger $qualified -Candidate $candidate
if ($qualified.baselineExceptions[0].rawStatus -cne 'FAIL') {throw 'Raw failed row was rewritten.'}
$checks++
Assert-Refused {param($l) $l.status='QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS';$l.baselineExceptions=@(New-BaselineException);$l.baselineExceptions[0].matchedClauseOrRows=$false}
Assert-Refused {param($l) $l.status='QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS';$l.baselineExceptions=@(New-BaselineException);$l.baselineExceptions[0].id='feature-module-brown-fur-publication-gate'}
Assert-Refused {param($l) $l.status='QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS';$l.baselineExceptions=@(New-BaselineException);$l.baselineExceptions[0].rawStatus='PASS'}
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('kmg-release-proof-'+[Guid]::NewGuid().ToString('N')+'.json')
try {
    [IO.File]::WriteAllText($temporary,'immutable raw evidence')
    $ledger=New-QualifiedFixture
    $proof=[pscustomobject]@{path=$temporary;sha256=(Get-FileHash $temporary).Hash.ToLowerInvariant()}
    foreach ($row in $ledger.coverage) {$row.proofs=@($proof)}
    Assert-KmgClosureProofFiles $ledger
    [IO.File]::WriteAllText($temporary,'changed evidence')
    $refused=$false
    try {Assert-KmgClosureProofFiles $ledger} catch {$refused=$true}
    if (-not $refused) {throw 'Changed immutable proof was accepted.'}
    $checks+=2
} finally {if(Test-Path -LiteralPath $temporary){Remove-Item -LiteralPath $temporary}}
foreach ($file in @('Publish-Release.ps1','Publish-QualifiedCandidate.ps1','QualifiedRelease.Common.ps1','Test-QualifiedRelease.ps1')) {
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot $file),[ref]$tokens,[ref]$errors)
    if ($errors.Count) {throw ($errors|Out-String)}
}
[pscustomobject]@{checks=$checks;failed=0;parsedScripts=4;nativeRuns=0;builds=0;tagWrites=0;publicationWrites=0}
