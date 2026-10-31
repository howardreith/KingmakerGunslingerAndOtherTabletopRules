[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint35QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe release input accepted'};$script:checks++}

Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint35InfernalReach -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint35InfernalReach -Sprint34InfernalOutsiders -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint35InfernalReach -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint35InfernalReach}
Pass {Assert-KmgSprint35ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s35' '86'}
Refuse {Assert-KmgSprint35ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s34' '86'}
Refuse {Assert-KmgSprint35ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s35' '0'}
$surface=Get-KmgSprint35Surface
Pass {if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.routes.Count -ne 5 -or
    $surface.projectCreatures -ne 128 -or $surface.monsterCreatures -ne 106 -or $surface.monsterPlacements -ne 587 -or
    $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or $surface.generatedRoots -ne 1175 -or
    $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1204 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 5){throw 'Source-derived Sprint35 surface differs.'}}

$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint35-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
function Proof([string]$name,$data){
    $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 30))
    [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$candidate=[pscustomobject]@{commit=$head;version='0.0.173';packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('f'*64)}
$allAssertions=@('synthetic-required')+@($surface.routes.assertion)
function Native([string]$run) {
    $raw=Proof ('native-'+$run) @{gitCommit=$head;runId=$run;status='PASS';assertions=@($allAssertions|ForEach-Object {@{name=$_;status='PASS'}})}
    [pscustomobject]@{source=$head;runId=$run;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;nativeProof=$raw;requiredAssertions=$allAssertions}
}
# This suite tests Sprint35 policy composition, while the inherited helper's
# own focused suite tests native receipt correlation and refusal in depth.
function Assert-KmgSprints3031FreshExecution { param($Source,$Candidate,$Row) }
$main=Native 'synthetic-main'
$coverage=@()
foreach($id in @('early-wall-bootstrap','barbed-behavior','barbed-combat','bone-behavior','public-routes-presentation','highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')) {
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
$repro=Proof reproducibility @{source=$head;primaryZipSha256=$candidate.packageSha256;rebuiltZipSha256=$candidate.packageSha256;primaryDllSha256=$candidate.dllSha256;rebuiltDllSha256=$candidate.dllSha256;exitCode=0}
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint35InfernalReach';status='QUALIFIED';releaseEligible=$true;sprint36Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$surface;scopeDecisions=@{wallOfIce='ONE_GROUNDED_10FT_GRIDGRAPH_MOVEMENT_ONLY_NO_LOE';barbedSlaOmissions=@('Major Image','Produce Flame','Pyrotechnics',"Order's Wrath");boneSlaOmissions=@('Major Image','Dimensional Anchor');otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.173';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='86'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 35|ConvertFrom-Json}
Pass {Assert-KmgSprint35Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint35InfernalReach -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint35PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '86'}
Refuse {Assert-KmgSprint35PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '86'}
Refuse {$l=CopyLedger;$l.candidate.dllSha256=('d'*64);Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.wallOfIce='DAMAGE_STRIP';Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.barbedSlaOmissions=@('Major Image');Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'bone-behavior');Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'barbed-behavior').result='REUSED_PASS';Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].identity=('d'*32);Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].observed=@('quantity');Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint35Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint35Qualification $l $candidate}
$bad=$sequences.off|ConvertTo-Json -Depth 15|ConvertFrom-Json;$bad.stages[0].processIdentity='synthetic-process-on-prepare'
$badProof=Proof duplicate-process $bad
Refuse {$l=CopyLedger;$r=$l.coverage|Where-Object id -ceq 'mixed-off-persistence';$r.proofs=@($r.proofs|Where-Object path -cne $r.sequenceProof.path)+@($badProof);$r.sequenceProof=$badProof;Assert-KmgSprint35Qualification $l $candidate}
Refuse {Assert-KmgClosureProofFiles -Ledger @{mission='Sprint35InfernalReach';coverage=@([pscustomobject]@{id='x';result='REUSED_PASS';proofs=@($main.nativeProof)})} -Sprint35InfernalReach -Candidate $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint35 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
