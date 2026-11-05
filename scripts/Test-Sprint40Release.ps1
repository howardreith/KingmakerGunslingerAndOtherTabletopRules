Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint29QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint40QualifiedRelease.Common.ps1')

$script:checks=0
function Pass([scriptblock]$Body){& $Body;$script:checks++}
function Refuse([scriptblock]$Body){
    $threw=$false;try{& $Body}catch{$threw=$true}
    if(-not $threw){throw 'Expected Sprint40 publisher refusal.'};$script:checks++
}

$folder=Join-Path ([IO.Path]::GetTempPath()) ('kmg-s40-publisher-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $folder|Out-Null
try {
    $head='a'*40
    $candidate=[pscustomobject]@{commit=$head;packageSha256=('b'*64);dllSha256=('c'*64);
        dllMvid='11111111-2222-3333-4444-555555555555';version='0.0.178';sourceStateSha256=('d'*64);
        packagePath=(Join-Path $folder 'candidate.zip')}
    [IO.File]::WriteAllBytes($candidate.packagePath,[byte[]](1,2,3))
    $surface=Get-KmgSprint40Surface
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or $surface.projectCreatures -ne 134 -or
       $surface.monsterCreatures -ne 112 -or $surface.monsterPlacements -ne 600 -or
       $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
       $surface.generatedRoots -ne 1188 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1217 -or
       @($surface.routes).Count -ne 1){throw 'Current Sprint40 source surface is not the exact public candidate shape.'}
    function Proof([string]$name,$value){
        $path=Join-Path $folder ($name+'.json');[IO.File]::WriteAllText($path,($value|ConvertTo-Json -Depth 40))
        [pscustomobject]@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()}
    }
    $requiredByRow=@{
        'profile-equipment-combat'=@('sprint32-sprint40-profile-live-totals','sprint32-sprint40-native-weapon-contributions','sprint32-sprint40-native-full-routine','sprint32-sprint40-spear-power-attack','sprint32-sprint40-native-disarm-owned-gear','sprint32-sprint40-bite-has-no-slow','sprint32-sprint40-native-haste-disarmed-fallback');
        'slow-real-time-and-turn-based'=@('sprint32-sprint40-slow-zero-final-damage-cold-immune-live-dc','sprint32-sprint40-slow-real-miss-no-attempt','sprint32-sprint40-slow-native-haste-counter','sprint32-sprint40-slow-native-descriptor-immunity','sprint32-sprint40-slow-independent-native-owners','sprint32-sprint40-slow-native-finite-expiry','sprint32-sprint40-slow-turn-native-single-action');
        'cold-wall-and-ai'=@('sprint32-sprint40-cold-native-cast-ConeOfCold','sprint32-sprint40-cold-native-cast-IceStorm','sprint32-sprint40-wall-cast-and-obstruction','sprint32-sprint40-wall-cross-species-live-claims','sprint32-sprint40-wall-cross-species-cleanup-isolated','sprint32-sprint40-wall-all-original-bits-restored','sprint32-sprint40-ai-ConeOfCold-rtwp-native-brain-cast','sprint32-sprint40-ai-IceStorm-turn-native-brain-cast','sprint32-sprint40-ai-WallOfIce-rtwp-source-removal');
        'fear-regeneration-and-lifetime'=@('sprint32-sprint40-regeneration-exact-consumer','sprint32-sprint40-regeneration-qualifier-good','sprint32-sprint40-fear-native-exposure','sprint32-sprint40-fear-live-dc-and-independent-source','sprint32-sprint40-native-expiry-gear-and-resources','sprint32-sprint40-native-death-gear-and-resources');
        'presentation-public-route'=@('sprint32-sprint40-body-ice-devil-0','sprint32-sprint40-private-pair-ice-devil','sprint32-sprint40-body-cleanup-ice-devil','sprint32-sprint40-public-route-scope','sprint32-sprint40-public-icon-ice-devil',$surface.routes[0].assertion);
        'affected-nalfeshnee-range'=@('sprint32-sprint40-nalfeshnee-slow-native-range','sprint32-sprint40-nalfeshnee-slow-inside-cast','sprint32-sprint40-nalfeshnee-slow-outside-needs-approach','sprint32-sprint40-nalfeshnee-slow-outside-native-approach')
    }
    $persistenceAssertions=@('whole-roster-persistence-s40-items-owned','whole-roster-persistence-s40-state-prepared',
        'whole-roster-persistence-s40-state-cold','whole-roster-persistence-s40-source-cleanup',
        'whole-roster-persistence-s40-state-cleanup-absence','whole-roster-persistence-s40-state-fresh-absence')
    $allAssertions=@($requiredByRow.Values|ForEach-Object {$_})+$persistenceAssertions
    function LoadedIdentity([string]$run){Proof ('loaded-'+$run) @{semanticVersion=$candidate.version;gitCommit=$head;
        moduleVersionId=$candidate.dllMvid;loadedModuleSha256=$candidate.dllSha256;processId=1}}
    function RestoredScenario([string]$run,[string]$directory){Proof ('restored-'+$run) @{restoration='verified';failures=0;expectedVersion=$candidate.version;
        liveBefore=@{sha256=('e'*64)};liveAfterRestore=@{sha256=('e'*64)};runs=@(@{scenario='disposable-expanded-summoning-sprint32-native-review';
        evidenceDirectory=$directory;outcome='PASS';scenarioStatus='PASS';exitCode=0;package=$candidate.packagePath})}}
    function InstalledStack([string]$run,[string]$directory){Proof ('stack-'+$run) @{candidateKind='release-candidate';releaseQualification=$true;
        restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};launchReceipt=@{runId=$run;evidenceDirectory=$directory};
        candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}}}
    function Native([string]$run,[string[]]$required,[string]$kind='restored-scenario'){
        $directory=Join-Path $folder $run;New-Item -ItemType Directory -Path $directory|Out-Null
        $rawPath=Join-Path $directory 'runtime-result.json'
        [IO.File]::WriteAllText($rawPath,(@{gitCommit=$head;runId=$run;status='PASS';assertions=@($allAssertions|ForEach-Object {@{name=$_;status='PASS'}})}|ConvertTo-Json -Depth 40))
        $raw=[pscustomobject]@{path=$rawPath;sha256=(Get-FileHash $rawPath -Algorithm SHA256).Hash.ToLowerInvariant()}
        $execution=if($kind -ceq 'installed-stack'){InstalledStack $run $directory}else{RestoredScenario $run $directory}
        [pscustomobject]@{source=$head;runId=$run;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;
            nativeProof=$raw;loadedIdentityProof=(LoadedIdentity $run);executionProof=$execution;executionKind=$kind;requiredAssertions=@($required)}
    }
    function SourceProofs($sources){@($sources|ForEach-Object {$_.nativeProof;$_.loadedIdentityProof;$_.executionProof})}
    $coverage=@()
    foreach($id in $requiredByRow.Keys){
        $source=Native ('synthetic-'+$id) $requiredByRow[$id]
        $entry=[pscustomobject]@{id=$id;result='PASS';proofs=@(SourceProofs @($source));evidenceSources=@($source)}
        if($id -ceq 'presentation-public-route'){$entry|Add-Member routes @(@{creature=$surface.routes[0].creature;tier=$surface.routes[0].tier;
            identity=$surface.routes[0].identity;runId=$source.runId;observed=@('cleanup','duration','eligibility','quantity','slot')})}
        $coverage+=$entry
    }
    foreach($which in @('on','off')){
        $sources=@();$stages=@();$proofs=@()
        foreach($stage in @('prepare','cleanup','absent')){
            $source=Native ('synthetic-'+$which+'-'+$stage) $persistenceAssertions 'installed-stack';$sources+=$source;$proofs+=@(SourceProofs @($source))
            $stages+=@{stage=$stage;runId=$source.runId;rawSha256=$source.nativeProof.sha256;processIdentity=('synthetic-process-'+$which+'-'+$stage)}
        }
        $sequenceProof=Proof ('sequence-'+$which) @{source=$head;sequence=$which;status='PASS';restoration=@{exactIntake=$true;gameExited=$true};stages=$stages}
        $coverage+=[pscustomobject]@{id=('mixed-'+$which+'-persistence');result='PASS';proofs=($proofs+@($sequenceProof));evidenceSources=$sources;sequenceProof=$sequenceProof}
    }
    $gate=Proof gate @{source=$head;status='PASS'}
    $repro=Proof reproducibility @{source=$head;primaryZipSha256=$candidate.packageSha256;rebuiltZipSha256=$candidate.packageSha256;
        primaryDllSha256=$candidate.dllSha256;rebuiltDllSha256=$candidate.dllSha256;exitCode=0}
    $coverage+=[pscustomobject]@{id='complete-source-build-package-gate';result='PASS';proofs=@($gate);evidenceSources=@()}
    $coverage+=[pscustomobject]@{id='exact-reproducibility';result='PASS';proofs=@($repro);evidenceSources=@()}
    $ledger=[pscustomobject]@{schemaVersion=1;mission='Sprint40IceDevil';status='QUALIFIED';releaseEligible=$true;sprint41Started=$false;
        gameplayWaiver=$false;baselineExceptions=@();unresolvedFindings=@();candidate=@{source=$head;zipSha256=$candidate.packageSha256;
        dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;
        fullSuite=@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true};reproducibilityProof=$repro};
        restoration=@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';roster=$surface;
        scopeDecisions=@{persistentImageDisposition='OWNER_APPROVED_OMISSION_NO_SUBSTITUTE_NO_ENGINE_IMPOSSIBILITY_CLAIM';
        wallAdaptation='S35_BOUNDED_SINGLE_GROUNDED_10FT_MOVEMENT_WALL_NO_LOE';everySummonExclusions=@('Greater Teleport','Nested Summoning');
        measuredDeviations=@('Effective movement is the printed 60-foot fly with the separate 40-foot land mode unmodeled');
        sprint39CarryForwards=@('NIMBUS_PUBLIC_WORDING_DAZE_NOT_BLINDNESS','NALFESHNEE_OWNED_SLOW_CL12_CLOSE_55_FEET');otherGameplayOmissions=@()};
        coverage=$coverage;publication=@{tag='v0.0.178';draft=$false;prerelease=$false;latest=$true;status='PENDING_PRODUCT_MERGE';
        productMergeCommit='';mergeContainsTestedSource=$false;expectedBaseCommit=('f'*40);productPr='97'}}
    $ledger.roster|Add-Member protectedIdentitySetsEqual $true
    function CopyLedger {$ledger|ConvertTo-Json -Depth 50|ConvertFrom-Json}

    Pass {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint40IceDevil -ExpectedBaseCommit ('f'*40)}
    Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -ExpectedBaseCommit ('f'*40)}
    Refuse {Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly -Sprint39Nalfeshnee -Sprint40IceDevil -ExpectedBaseCommit ('f'*40)}
    Pass {Assert-KmgSprint40ProductRoot 'C:\Dev\KingmakerGunslingerLab\wt\s40' '97'}
    Refuse {Assert-KmgSprint40ProductRoot 'C:\Dev\KingmakerGunslingerLab\wt\s39' '97'}
    Pass {Assert-KmgSprint40Qualification $ledger $candidate}
    Pass {Assert-KmgClosureProofFiles -Ledger $ledger -Sprint40IceDevil -Candidate $candidate}
    $pr=[pscustomobject]@{baseRefOid=('f'*40);baseRefName='master';state='OPEN';isDraft=$true;mergeable='MERGEABLE';headRefOid=$head}
    Pass {Assert-KmgSprint40PremergeAuthority $ledger $candidate $pr $head ('f'*40) ('f'*40) '97'}
    Refuse {Assert-KmgSprint40PremergeAuthority $ledger $candidate $pr $head ('e'*40) ('f'*40) '97'}
    Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'profile-equipment-combat').result='REUSED_PASS';Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.scopeDecisions.persistentImageDisposition='OMITTED';Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.scopeDecisions.wallAdaptation='GLOBAL_NAVMESH';Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.scopeDecisions.otherGameplayOmissions=@('poison');Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.gameplayWaiver=$true;Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.sprint41Started=$true;Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.restoration.workingFixtureClean=$false;Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.candidate.fullSuite.unfiltered=$false;Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$l.coverage=@($l.coverage|Where-Object id -cne 'exact-reproducibility');Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=$l.coverage|Where-Object id -ceq 'affected-nalfeshnee-range';$row.evidenceSources[0].requiredAssertions=@();Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;($l.coverage|Where-Object id -ceq 'presentation-public-route').routes[0].identity=('0'*32);Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=$l.coverage|Where-Object id -ceq 'profile-equipment-combat';$row.proofs=@($row.evidenceSources[0].nativeProof);Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=$l.coverage|Where-Object id -ceq 'mixed-on-persistence';$row.evidenceSources=@($row.evidenceSources[0]);Assert-KmgSprint40Qualification $l $candidate}
    Refuse {$l=CopyLedger;$row=$l.coverage|Where-Object id -ceq 'mixed-off-persistence';$s=$row.evidenceSources[0];
        $s.executionProof=Proof 'stack-development' @{candidateKind='development';releaseQualification=$true;restoration=@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};
        launchReceipt=@{runId=$s.runId;evidenceDirectory=(Join-Path $folder $s.runId)};candidate=@{commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid}};
        $row.proofs=@(SourceProofs $row.evidenceSources)+@($row.sequenceProof);Assert-KmgClosureProofFiles -Ledger $l -Sprint40IceDevil -Candidate $candidate}
}
finally {if(Test-Path -LiteralPath $folder){Remove-Item -LiteralPath $folder -Recurse -Force}}
Write-Host "Sprint40 publisher refusal checks PASS=$script:checks; synthetic proofs only, no network/publication."
