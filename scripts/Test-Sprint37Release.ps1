[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint37QualifiedRelease.Common.ps1')
$script:checks=0
function Pass([scriptblock]$case){& $case;$script:checks++}
function Refuse([scriptblock]$case){$refused=$false;try{& $case}catch{$refused=$true};if(-not $refused){throw 'Unsafe Sprint37 release input accepted'};$script:checks++}

Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint37Vrock -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint37Vrock -Sprint36InfernalDemons -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint37Vrock -Publish -ExpectedBaseCommit ('a'*40)}
Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint37Vrock}
Pass {Assert-KmgSprint37ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s37' '88'}
Refuse {Assert-KmgSprint37ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s36' '88'}
Refuse {Assert-KmgSprint37ProductRoot 'C:/Dev/KingmakerGunslingerLab/wt/s37' '0'}
$surface=Get-KmgSprint37Surface
Pass {if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.routes.Count -ne 3 -or
    $surface.projectCreatures -ne 131 -or $surface.monsterCreatures -ne 109 -or $surface.monsterPlacements -ne 597 -or
    $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
    $surface.generatedRoots -ne 1185 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1214 -or
    @($surface.routes.identity|Sort-Object -Unique).Count -ne 3){throw 'Source-derived Sprint37 surface differs.'}}

$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root ('artifacts/sprint37-publisher-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
function Proof([string]$name,$data){
    $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 35))
    [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE){throw 'Test source unavailable'}
$candidate=[pscustomobject]@{commit=$head;version='0.0.175';packageSha256=('b'*64);dllSha256=('c'*64);dllMvid='7f1c3a92-5d44-4a0e-9c21-6b8e5f0d7a31';sourceStateSha256=('f'*64);packagePath=(Join-Path $folder 'synthetic-candidate.zip')}
[IO.File]::WriteAllText($candidate.packagePath,'synthetic')
$sporeStates=@('sprint32-sprint37-spore-susceptible','sprint32-sprint37-spore-disease-immune',
    'sprint32-sprint37-spore-poison-immune-only','sprint32-sprint37-spore-native-cure',
    'sprint32-sprint37-spore-late-immunity','sprint32-sprint37-spore-footprint-boundary',
    'sprint32-sprint37-spore-descriptor-placement','sprint32-sprint37-spore-protection-facts')
$screechNames=@('sprint32-sprint37-screech-control-roster','sprint32-sprint37-screech-identity-exemption',
    'sprint32-sprint37-screech-live-constitution-dc','sprint32-sprint37-screech-radius-boundary',
    'sprint32-sprint37-screech-stun-expiry','sprint32-sprint37-screech-recharge-refusal',
    'sprint32-sprint37-screech-two-owner-isolation')
$allAssertions=@('synthetic-required')+@($surface.routes.assertion)+$sporeStates+$screechNames+
    @('sprint32-sprint37-screech-manual-interface','sprint32-sprint37-screech-turn-admission-turnbased')+
    @($screechNames|ForEach-Object {$_+'-turnbased'})
# The fresh-execution contract is exercised for real rather than stubbed:
# each synthetic source carries the engine's loaded-build receipt and the
# execution receipt of the controller that actually ran it, in the exact
# directory relationship the contract requires.
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
foreach($id in @('core-profile-and-self-buffs','spore-protection-and-geometry',
    'screech-identity-and-recharge','both-modes-screech','observed-ai-and-babau-dispel',
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
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint37Vrock';status='QUALIFIED';releaseEligible=$true;sprint38Started=$false;gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();
    candidate=@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
    restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';
    roster=$surface;
    scopeDecisions=@{vrockSlaOmissions=@('Telekinesis','Dance of Ruin','Greater Teleport');sporeCounterOmissions=@('Holy water sprinkling');movementOmissions=@('30-foot land speed');demonExemptionScope=@('Project-registered demons only');otherGameplayOmissions=@()};
    coverage=$coverage;publication=@{tag='v0.0.175';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('a'*40);productPr='88'}}
$ledger.roster|Add-Member protectedIdentitySetsEqual $true
function CopyLedger {$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json}
Pass {Assert-KmgSprint37Qualification $ledger $candidate}
Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint37Vrock -Candidate $candidate}
$pr=[pscustomobject]@{baseRefOid=('a'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
Pass {Assert-KmgSprint37PremergeAuthority $ledger $candidate $pr $head ('a'*40) ('a'*40) '88'}
Refuse {Assert-KmgSprint37PremergeAuthority $ledger $candidate $pr $head ('b'*40) ('a'*40) '88'}
# Sprint 37 reuses nothing: a reused mechanics row must refuse outright.
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').result='REUSED_PASS';Assert-KmgSprint37Qualification $l $candidate}
# Each disclosed omission is exact: neither narrowing nor broadening passes.
Refuse {$l=CopyLedger;$l.scopeDecisions.vrockSlaOmissions=@('Telekinesis','Dance of Ruin');Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.sporeCounterOmissions=@();Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.movementOmissions=@('30-foot land speed','50-foot fly speed');Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.scopeDecisions.demonExemptionScope=@('All chaotic evil outsiders');Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.gameplayWaiver=$true;Assert-KmgSprint37Qualification $l $candidate}
# A spore phase that lost one protection state cannot pass on the others.
foreach($state in $sporeStates) {
    Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'spore-protection-and-geometry');
        $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne $state});
        Assert-KmgSprint37Qualification $l $candidate}
}
# The screech must be proven by identity in both modes.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'both-modes-screech');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint37-screech-identity-exemption-turnbased'});
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'screech-identity-and-recharge');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint37-screech-two-owner-isolation'});
    Assert-KmgSprint37Qualification $l $candidate}
# Each screech row also carries the one pin specific to it: the real-time row
# the manual player interface the owner decision requires, and the turn-based
# row the engine's own admission to turn-based combat. Without the second pin a
# -turnbased suffix could stand for a mode the observation never entered.
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'screech-identity-and-recharge');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint37-screech-manual-interface'});
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'both-modes-screech');
    $row.evidenceSources[0].requiredAssertions=@($row.evidenceSources[0].requiredAssertions|Where-Object {$_ -cne 'sprint32-sprint37-screech-turn-admission-turnbased'});
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'public-routes-presentation').routes[0].identity=('d'*32);Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'mixed-on-persistence').evidenceSources=@($main);Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'observed-ai-and-babau-dispel');Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.publication.prerelease=$true;Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$l.sprint38Started=$true;Assert-KmgSprint37Qualification $l $candidate}
# The fresh-execution contract itself. A row may not cite bytes other than the
# ones the game loaded, an unverified or unrestored controller run, a scenario
# that did not pass, another package, an unattributed receipt, or a kind this
# sprint does not support.
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-mvid' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId='00000000-0000-0000-0000-000000000000';loadedModuleSha256=$candidate.dllSha256;processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').proofs=@(SourceProofs @($s));
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.loadedIdentityProof=(Proof 'loaded-other-dll' @{semanticVersion=$candidate.version;gitCommit=$head;moduleVersionId=$candidate.dllMvid;loadedModuleSha256=('d'*64);processId=1});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').proofs=@(SourceProofs @($s));
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs');
    $row.proofs=@($row.evidenceSources[0].nativeProof);Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.executionKind='profile';Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.executionProof=(Proof 'restored-unverified' @{restoration='skipped';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').proofs=@(SourceProofs @($s));
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.executionProof=(Proof 'restored-not-restored' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('b'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').proofs=@(SourceProofs @($s));
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$s=($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').evidenceSources[0];
    $s.executionProof=(Proof 'restored-other-package' @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('a'*64)};liveAfterRestore=@{sha256=('a'*64)};
        runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';evidenceDirectory=(Join-Path $folder $s.runId);outcome='PASS';scenarioStatus='PASS';exitCode=0;package=(Join-Path $folder 'other.zip')})});
    ($l.coverage|Where-Object id -ceq 'core-profile-and-self-buffs').proofs=@(SourceProofs @($s));
    Assert-KmgSprint37Qualification $l $candidate}
Refuse {$l=CopyLedger;$row=($l.coverage|Where-Object id -ceq 'mixed-on-persistence');$s=$row.evidenceSources[0];
    $s.executionProof=(Proof 'stack-unrestored' @{candidateKind='release-candidate';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='RESTORATION_FAILED'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}});
    $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);
    Assert-KmgSprint37Qualification $l $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint37 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
