[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint38QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe Sprint38 release input accepted'};$script:checks++}

Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint38Glabrezu -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint38Glabrezu -Sprint37Vrock -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint38Glabrezu -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint38Glabrezu}
Pass {Assert-KmgSprint38ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s38' '92'}
Refuse {Assert-KmgSprint38ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s37' '92'}
Refuse {Assert-KmgSprint38ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s38' '0'}
$surface=Get-KmgSprint38Surface
Pass {if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.routes.Count -ne 1 -or
    $surface.projectCreatures -ne 132 -or $surface.monsterCreatures -ne 110 -or $surface.monsterPlacements -ne 598 -or
    $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
    $surface.generatedRoots -ne 1186 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1215 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 1){throw 'Source-derived Sprint38 surface differs.'}}

$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint38-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
function Proof([string]$name,$data){
    $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 35))
    [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$candidate=[pscustomobject]@{commit=$head;version='0.0.176';packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('f'*64);packagePath=(Join-Path $folder 'synthetic-candidate.zip')}
[IO.File]::WriteAllText($candidate.packagePath,'synthetic')
# Every assertion the policy requires by name, so a missing one is what makes
# a refusal case fail rather than an unrelated gap.
$coreNames=@('sprint32-sprint38-core-bootstrap','sprint32-sprint38-core-five-primary-attacks',
    'sprint32-sprint38-core-defenses','sprint32-sprint38-core-feats-and-constant-sense',
    'sprint32-sprint38-core-attack-fallback')
$rendNames=@('sprint32-sprint38-rend-limbs','sprint32-sprint38-rend-round-clock-is-live',
    'sprint32-sprint38-rend-live-attack-command','sprint32-sprint38-rend-pair-rends-once',
    'sprint32-sprint38-rend-other-limbs-never','sprint32-sprint38-rend-history-is-per-victim',
    'sprint32-sprint38-rend-round-not-command')
$stunNames=@('sprint32-sprint38-stun-range-is-printed-close',
    'sprint32-sprint38-stun-buckets-follow-current-hit-points',
    'sprint32-sprint38-stun-refuses-an-immune-recipient',
    'sprint32-sprint38-stun-resistance-uses-printed-caster-level',
    'sprint32-sprint38-vrock-screech-numerical-resistance-unchanged')
$spellNames=@('sprint32-sprint38-spells-at-will-carry-no-resource',
    'sprint32-sprint38-spells-at-will-repeat',
    'sprint32-sprint38-spells-bursts-at-printed-difficulty')
$aiNames=@('sprint32-sprint38-ai-second-enemy-is-real','sprint32-sprint38-ai-candidates-differ',
    'sprint32-sprint38-ai-stun-picks-the-affectable-candidate')
$persistenceNames=@('whole-roster-persistence-s38-items-owned',
    'whole-roster-persistence-s38-stun-stood-then-withdrawn')
$allAssertions=@('synthetic-required')+$persistenceNames+@($surface.routes.assertion)+
    $coreNames+$rendNames+$stunNames+$spellNames+$aiNames
# The fresh-execution contract is exercised for real rather than stubbed: each
# synthetic source carries the engine's loaded-build receipt and the execution
# receipt of the controller that actually ran it, in the exact directory
# relationship the contract requires.
function LoadedIdentity([string]$run) {
    Proof ('loaded-'+$run) @{semanticVersion=$candidate.version;gitCommit=$head;
        moduleVersionId=$candidate.dllMvid;loadedModuleSha256=$candidate.dllSha256;processId=1}
}
function RestoredScenario([string]$run,[string]$directory) {
    Proof ('restored-'+$run) @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';
            evidenceDirectory=$directory;outcome='PASS';scenarioStatus='PASS';exitCode=0;
            package=$candidate.packagePath})}
}
function InstalledStack([string]$run,[string]$directory) {
    Proof ('stack-'+$run) @{candidateKind='release-candidate';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$run;evidenceDirectory=$directory};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}}
}
function Native([string]$run,[string]$source=$head,[string]$kind='restored-scenario') {
    $directory=Join-Path $folder $run
    New-Item -ItemType Directory -Path $directory|Out-Null
    $rawPath=Join-Path $directory 'runtime-result.json'
    [IO.File]::WriteAllText($rawPath,(@{gitCommit=$source;runId=$run;status='PASS';assertions=@($allAssertions|ForEach-Object {@{name=$_;status='PASS'}})}|ConvertTo-Json -Depth 35))
    $raw=[pscustomobject]@{path=$rawPath;sha256=(Get-FileHash $rawPath -Algorithm SHA256).Hash.ToLowerInvariant()}
    $execution=if($kind -ceq 'installed-stack'){InstalledStack $run $directory}else{RestoredScenario $run $directory}
    [pscustomobject]@{source=$source;runId=$run;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;
        nativeProof=$raw;loadedIdentityProof=(LoadedIdentity $run);executionProof=$execution;executionKind=$kind;requiredAssertions=$allAssertions}
}
function SourceProofs($sources){@($sources|ForEach-Object {$_.nativeProof;$_.loadedIdentityProof;$_.executionProof})}
$main=Native 'synthetic-main'
$coverage=@()
foreach($id in @('core-profile-and-body','rend-round-contract','power-word-stun',
    'at-will-spells-and-bursts','observed-ai-target-binding',
    'public-routes-presentation','highest-risk-integration')) {
    $entry=[pscustomobject]@{id=$id;result='PASS';proofs=@(SourceProofs @($main));evidenceSources=@($main)}
    if($id -ceq 'public-routes-presentation') {
        $entry|Add-Member routes @($surface.routes|ForEach-Object {@{creature=$_.creature;tier=$_.tier;identity=$_.identity;runId=$main.runId;observed=@('cleanup','duration','eligibility','quantity','slot')}})
    }
    $coverage+=$entry
}
$sequences=@{}
foreach($which in @('on','off')) {
    $sources=@();$stages=@();$proofs=@()
    foreach($stage in @('prepare','cleanup','absent')) {
        $source=Native ('synthetic-'+$which+'-'+$stage) $head 'installed-stack';$sources+=$source;$proofs+=@(SourceProofs @($source))
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
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint38Glabrezu';status='QUALIFIED';releaseEligible=$true;sprint39Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$surface;
    scopeDecisions=@{glabrezuSlaOmissions=@('Reverse Gravity','Veil');
        everySummonExclusions=@('Greater Teleport','Nested Summoning','Wish');
        measuredDeviations=@('Live Perception 28 from the installed Persuasive feat');
        unrepresentedSkills=@('Bluff','Diplomacy','Intimidate','Knowledge','Sense Motive','Use Magic Device','darkvision','telepathy');
        otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.176';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='92'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json}
Pass {Assert-KmgSprint38Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint38Glabrezu -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint38PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '92'}
Refuse {Assert-KmgSprint38PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '92'}
# Sprint 38 reuses nothing: a reused mechanics row must refuse outright.
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'core-profile-and-body').result='REUSED_PASS';Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'core-profile-and-body').result='REUSED_PASS';Assert-KmgClosureProofFiles -Ledger $l -Sprint38Glabrezu -Candidate $candidate}
# Each disclosed omission and measured deviation is exact: neither narrowing
# nor broadening passes.
Refuse {$l=CopyLedger;$l.scopeDecisions.glabrezuSlaOmissions=@('Reverse Gravity');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.glabrezuSlaOmissions=@('Reverse Gravity','Veil','Confusion');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.everySummonExclusions=@('Greater Teleport','Wish');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.measuredDeviations=@();Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.unrepresentedSkills=@('Bluff');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.otherGameplayOmissions=@('Rend');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.gameplayWaiver=$true;Assert-KmgSprint38Qualification $l $candidate}
# Every named printed fact must be present in the row that owns it. A phase
# that silently lost one case cannot pass on its siblings.
foreach($pair in @(@('core-profile-and-body',$coreNames),@('rend-round-contract',$rendNames),
    @('power-word-stun',$stunNames),@('at-will-spells-and-bursts',$spellNames),
    @('observed-ai-target-binding',$aiNames))) {
    foreach($name in $pair[1]) {
        Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq $pair[0]);
            $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne $name});
            Assert-KmgSprint38Qualification $l $candidate}
    }
}
# The narrow authorized Vrock verification is part of the stun row, so a
# release cannot be qualified without it having been measured.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'power-word-stun');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint38-vrock-screech-numerical-resistance-unchanged'});
    Assert-KmgSprint38Qualification $l $candidate}
# The persistence envelopes must name the saved state this creature carries.
foreach($which in @('on','off')) {
    Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq ('mixed-'+$which+'-persistence'));
        foreach($s in $row.evidenceSources){$s.requiredAssertions=@($s.requiredAssertions|Where-Object {$_ -cne 'whole-roster-persistence-s38-items-owned'})}
        Assert-KmgSprint38Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq ('mixed-'+$which+'-persistence'));
        foreach($s in $row.evidenceSources){$s.requiredAssertions=@($s.requiredAssertions|Where-Object {$_ -cne 'whole-roster-persistence-s38-stun-stood-then-withdrawn'})}
        Assert-KmgSprint38Qualification $l $candidate}
}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].identity=('d'*32);Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.executed=0;Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'observed-ai-target-binding');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'exact-reproducibility');Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.tag='v0.0.175';Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.sprint39Started=$true;Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.ownerVisualReview='APPROVED';Assert-KmgSprint38Qualification $l $candidate}
# The fresh-execution contract itself. A row may not cite bytes other than the
# ones the game loaded, an unverified or unrestored controller run, a scenario
# that did not pass, another package, an unattributed receipt, or a kind this
# sprint does not support.
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-mvid' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId='00000000-0000-0000-0000-000000000000';loadedModuleSha256=$candidate.dllSha256;processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-dll' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId=$candidate.dllMvid;loadedModuleSha256=('d'*64);processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-version' @{semanticVersion='0.0.175';gitCommit=$head;moduleVersionId=$candidate.dllMvid;loadedModuleSha256=$candidate.dllSha256;processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'core-profile-and-body');
    $row.proofs=@($row.evidenceSources[0].nativeProof);Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionKind='profile';Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-unverified' @{restoration='skipped';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-not-restored' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('b'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-other-package' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=(Join-Path $folder 'other.zip')})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-on-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-unrestored' @{candidateKind='release-candidate';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='RESTORATION_FAILED'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgSprint38Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-development' @{candidateKind='development';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgSprint38Qualification $l $candidate}
# And a development receipt may not reach a Sprint 38 row through the closure
# file check either: this sprint admits no reuse, so the allowance that lets
# some sprints cite development evidence cannot apply.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-development-closure' @{candidateKind='development';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgClosureProofFiles -Ledger $l -Sprint38Glabrezu -Candidate $candidate}
# Six fresh processes, not three reused ones.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');
    $sequence=Get-Content -LiteralPath $row.sequenceProof.path -Raw|ConvertFrom-Json
    foreach($stage in $sequence.stages){$stage.processIdentity='synthetic-process-on-prepare'}
    [IO.File]::WriteAllText($row.sequenceProof.path,($sequence|ConvertTo-Json -Depth 35))
    $row.sequenceProof.sha256=(Get-FileHash $row.sequenceProof.path -Algorithm SHA256).Hash.ToLowerInvariant()
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof)
    Assert-KmgSprint38Qualification $l $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint38 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
