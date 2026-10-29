[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint33QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe release input accepted'};$script:checks++}
Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint33ShadowOutsiders -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint33ShadowOutsiders -Sprints3031 -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint33ShadowOutsiders -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint33ShadowOutsiders}
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint33-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
function Proof([string]$name,$data){
 $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 25))
 [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$blob=(& git -C $root rev-parse ($head+':Info.json')).Trim();if($LASTEXITCODE){throw 'Test dependency unavailable'}
$candidate=[pscustomobject]@{commit=$head;packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';references=@{files=@(@{relativePath='example.dll';sha256=('d'*64)})}}
$native=Proof native @{gitCommit=$head;runId='fixture';status='PASS';assertions=@(@{name='required';status='PASS'})}
$receipt=Proof receipt @{candidateKind='development';releaseQualification=$false;purpose='sprint33-shadow-outsiders';repositoryRoot='C:\Dev\KingmakerGunslingerLab\wt\s33';branch='codex/expanded-summoning-sprint33-shadow-outsiders';commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;references=$candidate.references}
$transaction=Proof transaction @{status='Restored';profileId='gunslinger-only';restorationVerified=$true;createdAtUtc='2026-10-29T01:00:00Z';restoredAtUtc='2026-10-29T01:10:00Z'}
$loaded=Proof loaded @{gitCommit=$head;loadedModuleSha256=$candidate.dllSha256;moduleVersionId=$candidate.dllMvid;processId=123}
$deployment=Proof deployment @{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;developmentReceiptSha256=$receipt.sha256}
$orchestration=Proof orchestration @{status='PASS';runId='fixture';resultPath=$native.path;deploymentManifestPath=$deployment.path;kingmakerProcessId=123;startedAtUtc='2026-10-29T01:02:00Z';completedAtUtc='2026-10-29T01:05:00Z'}
$protected=Proof protected @{status='RESTORED';restoration=@{allOriginalSavesExact=$true;gameExitConfirmed=$true}}
$row=[pscustomobject]@{id='natural-invisibility-purge';result='REUSED_PASS';reuseJustification='Synthetic refusal fixture, not native proof';proofs=@($native,$receipt,$transaction,$loaded,$deployment,$orchestration,$protected);evidenceSources=@([pscustomobject]@{source=$head;runId='fixture';packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;nativeProof=$native;receiptProof=$receipt;profile='gunslinger-only';transactionProof=$transaction;loadedProof=$loaded;deploymentProof=$deployment;orchestrationProof=$orchestration;protectedProof=$protected;requiredAssertions=@('required');environmentComparison='synthetic equal references';observerMeaning='synthetic exact observation';dependencies=@(@{path='Info.json';originalBlob=$blob;candidateBlob=$blob;disposition='UNCHANGED';unchangedConsumerReason=''})})}
function CopyRow {$row|ConvertTo-Json -Depth 25|ConvertFrom-Json}
Pass {Assert-KmgSprint33Reuse $row $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger @{mission='Sprint33DedicatedShadowOutsiders';coverage=@($row)} -Sprint33ShadowOutsiders -Candidate $candidate}
Refuse {Assert-KmgClosureProofFiles -Ledger @{coverage=@($row)} -Candidate $candidate}
Refuse {$r=CopyRow;$r.id='unselected';Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.result='PASS';Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].requiredAssertions=@('absent');Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].packageSha256=('e'*64);Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].dependencies[0].candidateBlob=('e'*40);Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].dependencies=@();Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].environmentComparison='';Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.proofs=@($native,$receipt);Assert-KmgSprint33Reuse $r $candidate}
Refuse {$r=CopyRow;$r.evidenceSources[0].nativeProof.sha256=('e'*64);Assert-KmgSprint33Reuse $r $candidate}
Refuse {$c=$candidate|ConvertTo-Json -Depth 10|ConvertFrom-Json;$c.references.files[0].sha256=('e'*64);Assert-KmgSprint33Reuse $row $c}
Refuse {Assert-KmgSprint33Qualification @{schemaVersion=1;mission='wrong'} $candidate}
Write-Host "Sprint33 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
