[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint39QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
# The index and the offending expression are named, because this suite runs
# more than seventy refusals and 'something was accepted' does not say
# which contract stopped holding.
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true}
    if(-not $refused){throw ('Unsafe Sprint39 release input accepted at refusal #'+($script:checks+1)+': '+$case.ToString().Trim())}
    $script:checks++}

Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint39Nalfeshnee -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint39Nalfeshnee -Sprint37Vrock -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint39Nalfeshnee -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint39Nalfeshnee}
Pass {Assert-KmgSprint39ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s39' '94'}
Refuse {Assert-KmgSprint39ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s38' '94'}
Refuse {Assert-KmgSprint39ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s39' '0'}
$surface=Get-KmgSprint39Surface
Pass {if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.routes.Count -ne 1 -or
    $surface.projectCreatures -ne 133 -or $surface.monsterCreatures -ne 111 -or $surface.monsterPlacements -ne 599 -or
    $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
    $surface.generatedRoots -ne 1187 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1216 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 1){throw 'Source-derived Sprint39 surface differs.'}}

$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint39-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
function Proof([string]$name,$data){
    $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 35))
    [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$candidate=[pscustomobject]@{commit=$head;version='0.0.177';packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('f'*64);packagePath=(Join-Path $folder 'synthetic-candidate.zip')}
[IO.File]::WriteAllText($candidate.packagePath,'synthetic')
# Every assertion the policy requires by name, so a missing one is what makes
# a refusal case fail rather than an unrelated gap.
$coreNames=@('sprint32-sprint39-nalfeshnee-identities','sprint32-sprint39-nalfeshnee-profile',
    'sprint32-sprint39-nalfeshnee-aura-not-double-counted','sprint32-sprint39-nalfeshnee-feats',
    'sprint32-sprint39-nalfeshnee-authored-body')
$nimbusNames=@('sprint32-sprint39-nalfeshnee-nimbus-interface',
    'sprint32-sprint39-nalfeshnee-nimbus-activation',
    'sprint32-sprint39-nalfeshnee-nimbus-one-pending',
    'sprint32-sprint39-nalfeshnee-nimbus-delayed-burst',
    'sprint32-sprint39-nalfeshnee-nimbus-rolled-duration',
    'sprint32-sprint39-nalfeshnee-nimbus-recipients-at-burst',
    'sprint32-sprint39-nalfeshnee-nimbus-uses-exhausted',
    'sprint32-sprint39-nalfeshnee-nimbus-owners-independent',
    'sprint32-sprint39-nalfeshnee-nimbus-second-owner-resolves',
    'sprint32-sprint39-nalfeshnee-nimbus-destroyed-source')
$turnNames=@('sprint32-sprint39-nalfeshnee-nimbus-turn-admission-turnbased',
    'sprint32-sprint39-nalfeshnee-nimbus-activation-turnbased',
    'sprint32-sprint39-nalfeshnee-nimbus-one-pending-turnbased',
    'sprint32-sprint39-nalfeshnee-nimbus-delayed-burst-turnbased',
    'sprint32-sprint39-nalfeshnee-nimbus-rolled-duration-turnbased',
    'sprint32-sprint39-nalfeshnee-nimbus-recipients-at-burst-turnbased')
$spellNames=@('sprint32-sprint39-nalfeshnee-spells-at-will-declarations',
    'sprint32-sprint39-nalfeshnee-spells-general-resistance',
    'sprint32-sprint39-nalfeshnee-aura-self-only',
    'sprint32-sprint39-nalfeshnee-feeblemind-disability',
    'sprint32-sprint39-nalfeshnee-feeblemind-items-permitted',
    'sprint32-sprint39-nalfeshnee-feeblemind-lasting',
    'sprint32-sprint39-nalfeshnee-feeblemind-cure',
    'sprint32-sprint39-nalfeshnee-dispel-one-check-cap-and-no-retry',
    'sprint32-sprint39-nalfeshnee-spells-at-will-repeat')
$persistenceNames=@('whole-roster-persistence-s39-items-owned',
    'whole-roster-persistence-s39-nimbus-pending-armed')
$allAssertions=@('synthetic-required')+$persistenceNames+@($surface.routes.assertion)+
    $coreNames+$nimbusNames+$turnNames+$spellNames
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
foreach($id in @('core-profile-and-body','unholy-nimbus-real-time','unholy-nimbus-turn-based',
    'spells-aura-feeblemind-and-dispel',
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
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint39Nalfeshnee';status='QUALIFIED';releaseEligible=$true;sprint40Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$surface;
    scopeDecisions=@{nalfeshneeSlaOmissions=@('Summon demon reinforcements');
        everySummonExclusions=@('Greater Teleport','Nested Summoning','Wish');
        settledNarrowings=@('At most one pending Unholy Nimbus per creature',
            'Constant Unholy Aura is an exact-source self effect with no ally grant',
            'Feeblemind forbids spellcasting and not magic items',
            'Greater Dispel Magic carries one decisive check over its ordered effects');
        measuredDeviations=@('Effective movement is the printed 40-foot fly with the 30-foot land mode omitted',
            'Slow carries the project-recorded medium reach rather than the printed close derivation');
        unrepresentedSkills=@('Bluff','Diplomacy','Intimidate','Knowledge','Sense Motive','Use Magic Device','darkvision','telepathy');
        otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.177';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='94'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json}
Pass {Assert-KmgSprint39Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint39Nalfeshnee -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint39PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '94'}
Refuse {Assert-KmgSprint39PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '94'}
# Sprint 39 reuses nothing: a reused mechanics row must refuse outright.
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'core-profile-and-body').result='REUSED_PASS';Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'core-profile-and-body').result='REUSED_PASS';Assert-KmgClosureProofFiles -Ledger $l -Sprint39Nalfeshnee -Candidate $candidate}
# Each disclosed omission and measured deviation is exact: neither narrowing
# nor broadening passes.
Refuse {$l=CopyLedger;$l.scopeDecisions.nalfeshneeSlaOmissions=@();Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.nalfeshneeSlaOmissions=@('Summon demon reinforcements','Feeblemind');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.settledNarrowings=@('At most one pending Unholy Nimbus per creature');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.measuredDeviations=@('Effective movement is the printed 40-foot fly with the 30-foot land mode omitted');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.everySummonExclusions=@('Greater Teleport','Wish');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.measuredDeviations=@();Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.unrepresentedSkills=@('Bluff');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.otherGameplayOmissions=@('Unholy Aura ally grant');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.gameplayWaiver=$true;Assert-KmgSprint39Qualification $l $candidate}
# Every named printed fact must be present in the row that owns it. A phase
# that silently lost one case cannot pass on its siblings.
foreach($pair in @(@('core-profile-and-body',$coreNames),
    @('unholy-nimbus-real-time',$nimbusNames),@('unholy-nimbus-turn-based',$turnNames),
    @('spells-aura-feeblemind-and-dispel',$spellNames))) {
    foreach($name in $pair[1]) {
        Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq $pair[0]);
            $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne $name});
            Assert-KmgSprint39Qualification $l $candidate}
    }
}
# The second combat mode is its own row, so a release cannot be qualified on
# the real-time pass alone - which is exactly what a single nimbus row would
# have allowed, and the two modes do not share a clock.
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'unholy-nimbus-turn-based');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'unholy-nimbus-turn-based');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint39-nalfeshnee-nimbus-delayed-burst-turnbased'});
    Assert-KmgSprint39Qualification $l $candidate}
# The persistence envelopes must name the saved state this creature carries.
foreach($which in @('on','off')) {
    Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq ('mixed-'+$which+'-persistence'));
        foreach($s in $row.evidenceSources){$s.requiredAssertions=@($s.requiredAssertions|Where-Object {$_ -cne 'whole-roster-persistence-s39-items-owned'})}
        Assert-KmgSprint39Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq ('mixed-'+$which+'-persistence'));
        foreach($s in $row.evidenceSources){$s.requiredAssertions=@($s.requiredAssertions|Where-Object {$_ -cne 'whole-roster-persistence-s39-nimbus-pending-armed'})}
        Assert-KmgSprint39Qualification $l $candidate}
}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].identity=('d'*32);Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.executed=0;Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'spells-aura-feeblemind-and-dispel');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'exact-reproducibility');Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.tag='v0.0.176';Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.sprint40Started=$true;Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.ownerVisualReview='APPROVED';Assert-KmgSprint39Qualification $l $candidate}
# The fresh-execution contract itself. A row may not cite bytes other than the
# ones the game loaded, an unverified or unrestored controller run, a scenario
# that did not pass, another package, an unattributed receipt, or a kind this
# sprint does not support.
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-mvid' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId='00000000-0000-0000-0000-000000000000';loadedModuleSha256=$candidate.dllSha256;processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-dll' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId=$candidate.dllMvid;loadedModuleSha256=('d'*64);processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-version' @{semanticVersion='0.0.176';gitCommit=$head;moduleVersionId=$candidate.dllMvid;loadedModuleSha256=$candidate.dllSha256;processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'core-profile-and-body');
    $row.proofs=@($row.evidenceSources[0].nativeProof);Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionKind='profile';Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-unverified' @{restoration='skipped';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-not-restored' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('b'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-body').evidenceSources[0];
    $s.executionProof=(Proof 'restored-other-package' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=(Join-Path $folder 'other.zip')})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-body').proofs=@(SourceProofs @($s));
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-on-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-unrestored' @{candidateKind='release-candidate';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='RESTORATION_FAILED'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgSprint39Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-development' @{candidateKind='development';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgSprint39Qualification $l $candidate}
# And a development receipt may not reach a Sprint 39 row through the closure
# file check either: this sprint admits no reuse, so the allowance that lets
# some sprints cite development evidence cannot apply.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-development-closure' @{candidateKind='development';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgClosureProofFiles -Ledger $l -Sprint39Nalfeshnee -Candidate $candidate}
# Six fresh processes, not three reused ones.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-off-persistence');
    $sequence=Get-Content -LiteralPath $row.sequenceProof.path -Raw|ConvertFrom-Json
    foreach($stage in $sequence.stages){$stage.processIdentity='synthetic-process-on-prepare'}
    [IO.File]::WriteAllText($row.sequenceProof.path,($sequence|ConvertTo-Json -Depth 35))
    $row.sequenceProof.sha256=(Get-FileHash $row.sequenceProof.path -Algorithm SHA256).Hash.ToLowerInvariant()
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof)
    Assert-KmgSprint39Qualification $l $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint39 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
