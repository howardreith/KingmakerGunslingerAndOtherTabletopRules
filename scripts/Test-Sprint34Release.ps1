[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint34QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe release input accepted'};$script:checks++}
Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint34InfernalOutsiders -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint34InfernalOutsiders -Sprints3031 -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint34InfernalOutsiders -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint34InfernalOutsiders}
Pass {Assert-KmgSprint34ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s34' '84'}
Refuse {Assert-KmgSprint34ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s33' '84'}
Refuse {Assert-KmgSprint34ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s34' '0'}
Refuse {Assert-KmgSprint34ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s34' ''}
Pass {$surface=Get-KmgSprint34Surface;if(-not $surface.scopeExact -or $surface.routes.Count -ne 10 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 10){throw 'Source-derived two-key route scope differs.'}}
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint34-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
function Proof([string]$name,$data){
 $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 25))
 [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$blob=(& git -C $root rev-parse ($head+':Info.json')).Trim();if($LASTEXITCODE){throw 'Test dependency unavailable'}
$candidate=[pscustomobject]@{commit=$head;packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';references=@{files=@(@{relativePath='example.dll';sha256=('d'*64)})}}
$native=Proof native @{gitCommit=$head;runId='fixture';status='PASS';assertions=@(@{name='required';status='PASS'})}
$receipt=Proof receipt @{candidateKind='development';releaseQualification=$false;purpose='sprint34-infernal-early';repositoryRoot='C:\Dev\KingmakerGunslingerLab\wt\s34';branch='codex/expanded-summoning-sprint34-bearded-devil-kyton';commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;references=$candidate.references}
$transaction=Proof transaction @{status='Restored';profileId='gunslinger-only';restorationVerified=$true;createdAtUtc='2026-10-29T01:00:00Z';restoredAtUtc='2026-10-29T01:10:00Z'}
$loaded=Proof loaded @{gitCommit=$head;loadedModuleSha256=$candidate.dllSha256;moduleVersionId=$candidate.dllMvid;processId=123}
$deployment=Proof deployment @{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;developmentReceiptSha256=$receipt.sha256}
$orchestration=Proof orchestration @{status='PASS';runId='fixture';resultPath=$native.path;deploymentManifestPath=$deployment.path;kingmakerProcessId=123;startedAtUtc='2026-10-29T01:02:00Z';completedAtUtc='2026-10-29T01:05:00Z'}
$protected=Proof protected @{status='RESTORED';restoration=@{allOriginalSavesExact=$true;gameExitConfirmed=$true}}
$row=[pscustomobject]@{id='barbazu-combat-effects';result='REUSED_PASS';reuseJustification='Synthetic refusal fixture, not native proof';proofs=@($native,$receipt,$transaction,$loaded,$deployment,$orchestration,$protected);evidenceSources=@([pscustomobject]@{source=$head;runId='fixture';packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;nativeProof=$native;receiptProof=$receipt;profile='gunslinger-only';transactionProof=$transaction;loadedProof=$loaded;deploymentProof=$deployment;orchestrationProof=$orchestration;protectedProof=$protected;requiredAssertions=@('required');environmentComparison='synthetic equal references';observerMeaning='synthetic exact observation';dependencies=@(@{path='Info.json';originalBlob=$blob;candidateBlob=$blob;disposition='UNCHANGED';unchangedConsumerReason=''})})}
function CopyRow {$row|ConvertTo-Json -Depth 25|ConvertFrom-Json}
Pass {Assert-KmgSprint34Reuse $row $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger @{mission='Sprint34InfernalOutsiders';coverage=@($row)} -Sprint34InfernalOutsiders -Candidate $candidate}
Refuse {Assert-KmgClosureProofFiles -Ledger @{coverage=@($row)} -Candidate $candidate}
Refuse {$r=CopyRow;$r.id='unselected';Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.result='PASS';Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].requiredAssertions=@('absent');Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].packageSha256=('e'*64);Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].dependencies[0].candidateBlob=('e'*40);Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].dependencies=@();Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].environmentComparison='';Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.proofs=@($native,$receipt);Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].nativeProof.sha256=('e'*64);Assert-KmgSprint34Reuse $r $candidate}
Refuse {$c=$candidate|ConvertTo-Json -Depth 10|ConvertFrom-Json;$c.references.files[0].sha256=('e'*64);Assert-KmgSprint34Reuse $row $c}
Refuse {Assert-KmgSprint34Qualification @{schemaVersion=1;mission='wrong'} $candidate}
Refuse {$r=CopyRow;$r.id='models-icons-ten-routes';Assert-KmgSprint34Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources=@();Assert-KmgSprint34Reuse $r $candidate}
foreach($change in @(@('repositoryRoot','C:\Dev\KingmakerGunslingerLab\wt\s33'),
    @('branch','CODEX/expanded-summoning-sprint34-bearded-devil-kyton'),@('purpose','sprint35-infernal'))) {
    $bad=Get-Content $receipt.path -Raw|ConvertFrom-Json;$bad.($change[0])=$change[1]
    $proof=Proof ('bad-'+$change[0]) $bad
    Refuse {$r=CopyRow;$r.evidenceSources[0].receiptProof=$proof;Assert-KmgSprint34Reuse $r $candidate}
}
# Exercise the complete policy with synthetic files; only the catalog-public
# seam is substituted because the product remains deliberately hidden now.
$script:publicSurface=Get-KmgSprint34Surface
$script:publicSurface.scopePublic=$true
$script:publicSurface.generatedRoots=$script:publicSurface.registeredRoots
$script:publicSurface.choices=$script:publicSurface.generatedRoots+$script:publicSurface.nativeWrappers
function Get-KmgSprint34Surface { return $script:publicSurface }
$candidate|Add-Member version '0.0.172'
$candidate|Add-Member sourceStateSha256 ('f'*64)
$assertions=@('required')+@($script:publicSurface.routes.assertion)
$coverage=@();$sequences=@{}
function Fresh([string]$run) {
    $raw=Proof ('fresh-'+$run) @{gitCommit=$head;runId=$run;status='PASS';assertions=@($assertions|ForEach-Object {@{name=$_;status='PASS'}})}
    $exec=Proof ('execution-'+$run) @{candidateKind='release';restorationVerified=$true;profileId='gunslinger-only';commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;runtimeResults=@(@{runId=$run;status='PASS';runtimeResultSha256=$raw.sha256})}
    return [pscustomobject]@{source=$head;runId=$run;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;nativeProof=$raw;executionProof=$exec;executionKind='profile';profile='gunslinger-only';requiredAssertions=$assertions}
}
$main=Fresh 'synthetic-main'
foreach($id in @('profiles-defenses-isolation','barbazu-combat-effects','kyton-actions-defenses','ai-actions-cleanup','models-icons-ten-routes','highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')) {
    $entry=[pscustomobject]@{id=$id;result='PASS';proofs=@($main.nativeProof,$main.executionProof);evidenceSources=@($main)}
    if($id -ceq 'models-icons-ten-routes') {
        $entry|Add-Member routes @($script:publicSurface.routes|ForEach-Object {@{creature=$_.creature;tier=$_.tier;identity=$_.identity;runId=$main.runId;observed=@('cleanup','duration','eligibility','quantity','slot')}})
    }
    $coverage+=$entry
}
foreach($which in @('on','off')) {
    $sources=@();$stages=@();$proofs=@()
    foreach($stage in @('prepare','cleanup','absent')) {
        $source=Fresh ('synthetic-'+$which+'-'+$stage);$sources+=$source;$proofs+=@($source.nativeProof,$source.executionProof)
        $stages+=@{stage=$stage;runId=$source.runId;rawSha256=$source.nativeProof.sha256;processIdentity=('synthetic-process-'+$which+'-'+$stage)}
    }
    $sequence=@{source=$head;sequence=$which;status='PASS';restoration=@{exactIntake=$true;gameExited=$true};stages=$stages}
    $sequences[$which]=$sequence
    $proof=Proof ('sequence-'+$which) $sequence
    $coverage+= [pscustomobject]@{id=('mixed-'+$which+'-persistence');result='PASS';proofs=($proofs+@($proof));evidenceSources=$sources;sequenceProof=$proof}
}
$repro=Proof reproducibility @{source=$head;primaryZipSha256=$candidate.packageSha256;rebuiltZipSha256=$candidate.packageSha256;primaryDllSha256=$candidate.dllSha256;rebuiltDllSha256=$candidate.dllSha256;exitCode=0}
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint34InfernalOutsiders';status='QUALIFIED';releaseEligible=$true;sprint35Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$script:publicSurface;scopeDecisions=@{dancingChains='STANDARD_ACTION_OWN_FOUR_NO_ACTIVATION_ATTACK';gaze='TARGETED_STANDARD_30FT_ELIGIBLE_ALLIES';healSkill='OWNER_OMITTED_NO_SUBSTITUTE';otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.172';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='84'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 35|ConvertFrom-Json}
Pass {Assert-KmgSprint34Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint34InfernalOutsiders -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint34PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '84'}
Refuse {Assert-KmgSprint34PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '84'}
Refuse {$l=CopyLedger;$l.candidate.dllSha256=('d'*64);Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.otherGameplayOmissions=@('unapproved');Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'kyton-actions-defenses');Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'models-icons-ten-routes').routes[0].identity=('d'*32);Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'models-icons-ten-routes').routes[0].observed=@('quantity');Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint34Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint34Qualification $l $candidate}
$badSequence=$sequences['off']|ConvertTo-Json -Depth 15|ConvertFrom-Json
$badSequence.stages[0].processIdentity='synthetic-process-on-prepare'
$badProof=Proof duplicate-process $badSequence
Refuse {$l=CopyLedger;$r=$l.coverage|Where-Object id -ceq 'mixed-off-persistence';$r.proofs=@($r.proofs|Where-Object path -cne $r.sequenceProof.path)+@($badProof);$r.sequenceProof=$badProof;Assert-KmgSprint34Qualification $l $candidate}
Write-Host "Sprint34 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
