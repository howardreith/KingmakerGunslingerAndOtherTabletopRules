[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint36QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe Sprint36 release input accepted'};$script:checks++}

Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint36InfernalDemons -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint36InfernalDemons -Sprint35InfernalReach -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint36InfernalDemons -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint36InfernalDemons}
Pass {Assert-KmgSprint36ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s36' '88'}
Refuse {Assert-KmgSprint36ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s35' '88'}
Refuse {Assert-KmgSprint36ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s36' '0'}
$surface=Get-KmgSprint36Surface
Pass {if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.routes.Count -ne 7 -or
    $surface.projectCreatures -ne 130 -or $surface.monsterCreatures -ne 108 -or $surface.monsterPlacements -ne 594 -or
    $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
    $surface.generatedRoots -ne 1182 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1211 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 7){throw 'Source-derived Sprint36 surface differs.'}}

$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint36-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
function Proof([string]$name,$data){
    $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 35))
    [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$candidate=[pscustomobject]@{commit=$head;version='0.0.174';packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('f'*64)}
$allAssertions=@('synthetic-required')+@($surface.routes.assertion)
function Native([string]$run,[string]$source=$head) {
    $raw=Proof ('native-'+$run) @{gitCommit=$source;runId=$run;status='PASS';assertions=@($allAssertions|ForEach-Object {@{name=$_;status='PASS'}})}
    [pscustomobject]@{source=$source;runId=$run;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;nativeProof=$raw;requiredAssertions=$allAssertions}
}
function Assert-KmgSprints3031FreshExecution { param($Source,$Candidate,$Row) }
$main=Native 'synthetic-main'
$coverage=@()
foreach($id in @('profiles-registration','babau-combat-acid','hezrou-combat-control','both-modes-lifetime')) {
    $source=Native ('synthetic-reused-'+$id)
    $coverage+=[pscustomobject]@{id=$id;result='REUSED_PASS';priorSource=$head;
        reuseJustification='UNCHANGED_PRODUCTION_DEPENDENCIES_WITH_EXACT_ARTIFACT_ATTRIBUTION';
        dependencyPaths=@('src/KingmakerGunslinger/Summoning/InfernalRules36.cs');
        proofs=@($source.nativeProof);evidenceSources=@($source)}
}
foreach($id in @('public-routes-presentation','highest-risk-integration')) {
    $entry=[pscustomobject]@{id=$id;result='PASS';proofs=@($main.nativeProof);evidenceSources=@($main)}
    if($id -ceq 'public-routes-presentation') {
        $entry|Add-Member routes @($surface.routes|ForEach-Object {@{creature=$_.creature;tier=$_.tier;identity=$_.identity;runId=$main.runId;observed=@('cleanup','duration','eligibility','quantity','slot')}})
    }
    $coverage+=$entry
}
$sequences=@{}
foreach($which in @('on','off')) {
    $sources=@();$stages=@();$proofs=@()
    foreach($stage in @('prepare','cleanup','absent')) {
        $source=Native ('synthetic-'+$which+'-'+$stage);$sources+=$source;$proofs+=$source.nativeProof
        $stages+=@{stage=$stage;runId=$source.runId;rawSha256=$source.nativeProof.sha256;processIdentity=('synthetic-process-'+$which+'-'+$stage)}
    }
    $sequence=@{source=$head;sequence=$which;status='PASS';restoration=@{exactIntake=$true;gameExited=$true};stages=$stages}
    $sequences[$which]=$sequence;$sequenceProof=Proof ('sequence-'+$which) $sequence
    $coverage+=[pscustomobject]@{id=('mixed-'+$which+'-persistence');result='PASS';proofs=($proofs+@($sequenceProof));evidenceSources=$sources;sequenceProof=$sequenceProof}
}
$gate=Proof gate @{source=$head;status='PASS'}
$repro=Proof reproducibility @{source=$head;primaryZipSha256=$candidate.packageSha256;rebuiltZipSha256=$candidate.packageSha256;primaryDllSha256=$candidate.dllSha256;rebuiltDllSha256=$candidate.dllSha256;exitCode=0}
$coverage+=[pscustomobject]@{id='complete-source-build-package-gate';result='PASS';proofs=@($gate);evidenceSources=@()}
$coverage+=[pscustomobject]@{id='exact-reproducibility';result='PASS';proofs=@($repro);evidenceSources=@()}
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint36BabauHezrou';status='QUALIFIED';releaseEligible=$true;sprint37Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$surface;scopeDecisions=@{protectiveSlimeOmissions=@('Manufactured-weapon branch','Ammunition branch');babauSlaOmissions=@('Darkness');hezrouSlaOmissions=@('Gaseous Form');otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.174';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='88'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json}
Pass {Assert-KmgSprint36Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint36InfernalDemons -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint36PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '88'}
Refuse {Assert-KmgSprint36PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '88'}
Refuse {$l=CopyLedger;$l.scopeDecisions.protectiveSlimeOmissions=@('Manufactured-weapon branch');Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.babauSlaOmissions=@();Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'profiles-registration').dependencyPaths=@('src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs');Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').result='REUSED_PASS';Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].identity=('d'*32);Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'hezrou-combat-control');Assert-KmgSprint36Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint36Qualification $l $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint36 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
