Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031QualifiedRelease.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprints3031Budget.Common.ps1')
$actual=Get-KmgSprints3031Surface
if(-not $actual.scopeExact -or $actual.routes.Count -ne 30 -or @($actual.routes|Where-Object {$_.creature -ceq 'lantern-archon'}).Count -ne 7) { throw 'Actual batch source surface differs.' }
$script:surface=$actual|ConvertTo-Json -Depth 10|ConvertFrom-Json
$script:surface.suppressed=0
$script:surface.generatedRoots=$script:surface.registeredRoots
$script:surface.choices=$script:surface.registeredRoots+$script:surface.nativeWrappers
function Get-KmgSprints3031Surface { return $script:surface }
$temp=Join-Path ([IO.Path]::GetTempPath()) ('kmg-s3031-policy-test-'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($temp)
function Proof([string]$Name,$Value) {
    $path=Join-Path $temp $Name
    [IO.File]::WriteAllText($path,($Value|ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
    return [pscustomobject]@{path=$path;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$head=(& git -C (Split-Path -Parent $PSScriptRoot) rev-parse HEAD).Trim()
$refs=[pscustomobject]@{files=@([pscustomobject]@{relativePath='unit-fixture.dll';sha256=('1'*64)})}
$candidate=[pscustomobject]@{commit=$head;version='0.0.167';packageSha256=('2'*64);dllSha256=('3'*64);dllMvid='01234567-0123-4567-8901-012345678901';sourceStateSha256=('4'*64);references=$refs}
$native=Proof 'unit-native.json' ([pscustomobject]@{gitCommit=$head;runId='unit-prior';status='PASS';gameVersion='2.1.7b';assertions=@([pscustomobject]@{name='unit-observation';status='PASS'})})
$receipt=Proof 'unit-development.json' ([pscustomobject]@{candidateKind='development';releaseQualification=$false;repositoryRoot='C:\Dev\KingmakerGunslingerLab\wt\s30-31';branch='codex/expanded-summoning-sprints30-31-outsider-celestial-pilot';commit=$head;sourceStateSha256=('4'*64);packageSha256=('5'*64);dllSha256=('6'*64);dllMvid='11111111-0123-4567-8901-012345678901';references=$refs})
$profile=Proof 'unit-profile.json' ([pscustomobject]@{candidateKind='development';releaseQualification=$false;profileId='gunslinger-only';packageSha256=('5'*64);restorationVerified=$true;runtimeResults=@([pscustomobject]@{runId='unit-prior';status='PASS';runtimeResultSha256=$native.sha256})})
$dependencyPath='src/KingmakerGunslinger/Summoning/LemureRacialProfile.cs'
$blob=Get-KmgSprints3031Blob $head $dependencyPath
$source=[pscustomobject]@{source=$head;sourceFingerprint=('4'*64);packageSha256=('5'*64);dllSha256=('6'*64);dllMvid='11111111-0123-4567-8901-012345678901';runId='unit-prior';originalVerdict='PASS';profile='gunslinger-only';gameVersion='2.1.7b';receiptProof=$receipt;profileProof=$profile;nativeProof=$native;requiredAssertions=@('unit-observation');observedOutcome='UNIT FIXTURE ONLY';observerMeaning='Unit schema fixture';environmentComparison='Same fixture references';dependencies=@(@('production','configuration','asset','shared-helper','observer')|ForEach-Object {[pscustomobject]@{category=$_;path=$dependencyPath;originalBlob=$blob;candidateBlob=$blob;comparison='IDENTICAL_RELEVANT_BYTES'}})}
$requirements=Get-KmgSprints3031MechanicsRequirements
$claims=@(foreach($c in $surface.creatures) {foreach($g in $requirements[$c]) {[pscustomobject]@{creature=$c;requirement=$g;runId='unit-prior';assertions=@('unit-observation')}}})
$reused=[pscustomobject]@{id='unit-reused-mechanics';kind='mechanics';result='REUSED_PASS';acceptance='OWNER_S3031_SCOPED_DEVELOPMENT_MECHANICS_2026_10_24';reuseJustification='UNIT FIXTURE ONLY';claims=$claims;proofs=@($native,$receipt,$profile);evidenceSources=@($source)}
$freshSources=@();$freshProofs=@()
$finalAssertions=@('whole-roster-persistence-s3031-fresh-absence','whole-roster-persistence-s3031-prepare-aura-consumers','whole-roster-persistence-s3031-cold-aura-consumers','whole-roster-persistence-s3031-prepared-slot-setup','whole-roster-persistence-s3031-cleanup-owned-state-cleanup','whole-roster-persistence-s3031-cleanup-absence','sprints30-31-public-inventory-integration')+@($surface.creatures|ForEach-Object {'whole-roster-persistence-s3031-items-resources-effects-'+$_;'whole-roster-persistence-s3031-aura-source-'+$_;'sprints30-31-public-body-icon-'+$_;'sprints30-31-public-body-reset-'+$_})+@('unit-observation')+@($surface.routes.assertion)+@('shared-circles','source-lifecycle','celestial-cleave'|ForEach-Object {'sprints30-31-final-integration-reset-'+$_})
foreach($i in 1..9) {
    $fixtureProfile=if($i -eq 8){'gunslinger-high-risk-combined'}else{'gunslinger-only'}
    $proof=Proof ('unit-final-'+$i+'.json') ([pscustomobject]@{gitCommit=$head;runId=('unit-final-'+$i);status='PASS';assertions=@($finalAssertions|ForEach-Object {[pscustomobject]@{name=$_;status='PASS'}})})
    $execution=Proof ('unit-final-profile-'+$i+'.json') ([pscustomobject]@{candidateKind='release';profileId=$fixtureProfile;restorationVerified=$true;commit=$head;packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;runtimeResults=@([pscustomobject]@{runId=('unit-final-'+$i);status='PASS';runtimeResultSha256=$proof.sha256})})
    $freshProofs+=@($proof,$execution)
    $freshSources+=[pscustomobject]@{source=$head;candidateKind='release';packageSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;runId=('unit-final-'+$i);nativeProof=$proof;executionProof=$execution;executionKind='profile';profile=$fixtureProfile;requiredAssertions=$finalAssertions}
}
$freshIds=@('public-thirty-routes','public-inventory-icons-bodies','mixed-persistence-six-fresh-processes','standalone-and-high-risk-integration','complete-final-source-build-package-gate','exact-package-release-qualification')
$coverage=@($reused)+@($freshIds|ForEach-Object {[pscustomobject]@{id=$_;kind='final';result='PASS';proofs=$freshProofs;evidenceSources=$freshSources}})
$routeRow=$coverage|Where-Object {$_.id -ceq 'public-thirty-routes'}
$routeRow|Add-Member routes @($surface.routes|ForEach-Object {[pscustomobject]@{creature=$_.creature;tier=$_.tier;identity=$_.identity;runId='unit-final-9';observed=@('slot','quantity','duration','eligibility','cleanup')}})
$p=$coverage|Where-Object {$_.id -ceq 'mixed-persistence-six-fresh-processes'}
$stages=@('on-prepare','on-cleanup-save','on-absence','off-prepare','off-cleanup-save','off-on-absence');$i=0
$p|Add-Member stages @($stages|ForEach-Object {$i++;[pscustomobject]@{id=$_;runId=('unit-final-'+$i);processIdentity=([string](100+$i)+'@2026-10-24T00:00:00Z');result='PASS';freshProcess=$true;originalWorkingRestored=$true;creatures=$surface.creatures}})
for($j=0;$j -lt 6;$j++) {
    $source=$freshSources[$j];$stage=$p.stages[$j]
    $scenario=if($stage.id.EndsWith('prepare')){'working-save-expanded-summoning-prepare'}elseif($stage.id.EndsWith('cleanup-save')){'working-save-expanded-summoning-verify-cleanup'}else{'working-save-expanded-summoning-verify-absent'}
    $raw=Read-KmgSprints3031Proof $source.nativeProof;$raw|Add-Member scenario $scenario
    $source.nativeProof=Proof ('unit-persist-native-'+$j+'.json') $raw
    $source.executionKind='installed-stack'
    $source.executionProof=Proof ('unit-installed-'+$j+'.json') ([pscustomobject]@{candidateKind='release-candidate';releaseQualification=$true;scenario=$scenario;persistenceScope='outsider-celestial';moduleDisabled=($stage.id -ceq 'off-cleanup-save');restoration=[pscustomobject]@{verified=$true;gameSettingsExact=$true;transactionStatus='Restored'};error=$null;candidate=$candidate;launchReceipt=[pscustomobject]@{runId=$source.runId;evidenceDirectory=$temp}})
    # Exact raw filename expected by the installed-stack receipt. Each stage gets its own directory.
    $dir=Join-Path $temp ('persist-'+$j);[void][IO.Directory]::CreateDirectory($dir)
    $rawPath=Join-Path $dir 'runtime-result.json';Copy-Item -LiteralPath $source.nativeProof.path -Destination $rawPath
    $source.nativeProof=[pscustomobject]@{path=$rawPath;sha256=(Get-FileHash $rawPath -Algorithm SHA256).Hash.ToLowerInvariant()}
    $execution=Read-KmgSprints3031Proof $source.executionProof;$execution.launchReceipt.evidenceDirectory=$dir
    $source.executionProof=Proof ('unit-installed-final-'+$j+'.json') $execution
    $process=Proof ('unit-process-'+$j+'.json') ([pscustomobject]@{runId=$source.runId;launchBegan=$true;guardedRequestAccepted=$true;preLaunchKingmakerProcesses=@();steamAppId=640820;kingmakerProcessId=(101+$j);kingmakerStartedAtUtc='2026-10-24T00:00:00Z'})
    $stage|Add-Member processProof $process
    $p.proofs+=@($source.nativeProof,$source.executionProof,$process)
}
$p.evidenceSources=@($freshSources|Select-Object -First 6)
$routeRow.evidenceSources=@($freshSources[8]);$coverage[2].evidenceSources=@($freshSources[8])
$integration=$coverage|Where-Object {$_.id -ceq 'standalone-and-high-risk-integration'}
$integration.evidenceSources=@($freshSources[6],$freshSources[7])
$integration|Add-Member profiles @('gunslinger-only','gunslinger-high-risk-combined')
$integration|Add-Member affectedOldControlsPassed $true
$integration|Add-Member sharedSourcesAndLifecyclePassed $true
$roster=$surface|ConvertTo-Json -Depth 10|ConvertFrom-Json
$roster|Add-Member protectedIdentitySetsEqual $true
$roster|Add-Member lanternOrderingAndReferencesPreserved $true
$ledger=[pscustomobject]@{schemaVersion=1;mission='Sprints3031OutsiderCelestialPilot';status='QUALIFIED';releaseEligible=$true;sprint32Started=$false;baselineExceptions=@();unresolvedFindings=@();expandedScope=$false;gameplayWaiver=$false;candidate=[pscustomobject]@{source=$head;zipSha256=$candidate.packageSha256;dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;sourceFingerprint=$candidate.sourceStateSha256;pipelineExitCode=0;fullSuite=[pscustomobject]@{exitCode=0;failures=0;executed=1;registered=1;unfiltered=$true}};roster=$roster;restoration=[pscustomobject]@{current='EXACT_INTAKE_VERIFIED';workingFixtureClean=$true};scopeDecisions=[pscustomobject]@{houndAttackRoles='GENERAL_PF1';trumpetBlastDc='GENERAL_PF1';offensiveBanishment='RETAIN_CASTER_TRAVEL_EXCLUDED';magicCircle='CANONICAL_PUBLISHED_ADAPTATION';visualStandard='FUNCTIONAL_FIRST'};ownerVisualReview='NOT_PERFORMED_NONBLOCKING';publication=[pscustomobject]@{tag='v0.0.167';prerelease=$false;draft=$false;latest=$true};coverage=$coverage}
foreach($row in $ledger.coverage|Where-Object {$_.kind -ceq 'final'}) {
    foreach($source in $row.evidenceSources) {
        foreach($proof in @($source.nativeProof,$source.executionProof)) {
            if(@($row.proofs|Where-Object {$_.path -ceq $proof.path}).Count -eq 0){$row.proofs+=@($proof)}
        }
    }
}
$notes='Magic Circle printed change shape gestalt visual'
function Validate($l) { Assert-KmgSprints3031Qualification $l $candidate $notes; Assert-KmgClosureProofFiles -Ledger $l -Sprints3031 -Candidate $candidate }
Validate $ledger
$script:checks=1
function Refuse([string]$Name,[scriptblock]$Change) {
    $l=$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json
    & $Change $l
    $refused=$false
    try { Validate $l } catch { $refused=$true }
    if(-not $refused) { throw ('Accepted invalid '+$Name) }
    $script:checks++
}
Refuse 'missing hash' {$args[0].coverage[0].proofs[0].sha256=''}
Refuse 'wrong hash' {$args[0].coverage[0].proofs[0].sha256=('0'*64)}
Refuse 'unsupported reuse' {$args[0].coverage[0].acceptance='generic-trust'}
Refuse 'changed dependency' {$args[0].coverage[0].evidenceSources[0].dependencies[0].candidateBlob=('f'*40)}
Refuse 'unreviewed changed meaning' {$args[0].coverage[0].evidenceSources[0].dependencies[0].comparison='TRUST_ME'}
Refuse 'missing creature' {$l=$args[0];$l.coverage[0].claims=@($l.coverage[0].claims|Where-Object {$_.creature -cne 'trumpet-archon'})}
Refuse 'missing signature outcome' {$l=$args[0];$l.coverage[0].claims=@($l.coverage[0].claims|Where-Object {$_.requirement -cne 'prepared-book'})}
Refuse 'missing routes' {$args[0].coverage[1].routes=@($args[0].coverage[1].routes|Select-Object -Skip 1)}
Refuse 'route without native binding' {$args[0].coverage[1].routes[0].runId='unobserved'}
Refuse 'missing real integration profile' {$l=$args[0];$l.coverage[4].evidenceSources=@($l.coverage[4].evidenceSources|Where-Object {$_.profile -ceq 'gunslinger-only'})}
Refuse 'wrong route GUID' {$args[0].coverage[1].routes[0].identity=('f'*32)}
Refuse 'absent final persistence' {$args[0].coverage[3].stages=@()}
Refuse 'invented process identity' {$args[0].coverage[3].stages[0].processIdentity='invented'}
Refuse 'wrong persistence stage' {$args[0].coverage[3].stages[0].runId='unit-final-7'}
Refuse 'missing body observation' {$args[0].coverage[2].evidenceSources[0].requiredAssertions=@('unit-observation')}
Refuse 'reused process' {$args[0].coverage[3].stages[1].processIdentity=$args[0].coverage[3].stages[0].processIdentity}
Refuse 'failed restoration' {$args[0].restoration.workingFixtureClean=$false}
Refuse 'wrong final artifact' {$args[0].coverage[1].evidenceSources[0].packageSha256=('f'*64)}
Refuse 'false fresh labeling' {$args[0].coverage[0].result='PASS'}
Refuse 'missing native observation' {$args[0].coverage[0].evidenceSources[0].requiredAssertions=@('unobserved')}
Refuse 'hidden mandatory failure' {$args[0].coverage[0].result='FAIL'}
Refuse 'claim with wrong run' {$args[0].coverage[0].claims[0].runId='unobserved'}
Refuse 'claim without its observation' {$args[0].coverage[0].claims[0].assertions=@('unobserved')}
$prior=(& git -C (Split-Path -Parent $PSScriptRoot) rev-parse '3a30c700c2fab164081522d001f48c9afe3290a2').Trim()
$changedPath='src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs'
$oldBlob=Get-KmgSprints3031Blob $prior $changedPath
$newBlob=Get-KmgSprints3031Blob $head $changedPath
if($oldBlob -ceq $newBlob){throw 'Changed-dependency test requires actual integration delta'}
$changed=$ledger|ConvertTo-Json -Depth 40|ConvertFrom-Json
$cs=$changed.coverage[0].evidenceSources[0]
$cs.source=$prior
$originalReceipt=Read-KmgSprints3031Proof $cs.receiptProof;$originalReceipt.commit=$prior
$cs.receiptProof=Proof 'unit-changed-development.json' $originalReceipt
$originalNative=Read-KmgSprints3031Proof $cs.nativeProof;$originalNative.gitCommit=$prior
$cs.nativeProof=Proof 'unit-changed-native.json' $originalNative
$originalProfile=Read-KmgSprints3031Proof $cs.profileProof;$originalProfile.runtimeResults[0].runtimeResultSha256=$cs.nativeProof.sha256
$cs.profileProof=Proof 'unit-changed-profile.json' $originalProfile
foreach($dep in $cs.dependencies){$dep.originalBlob=Get-KmgSprints3031Blob $prior $dep.path}
$cs.dependencies[0].path=$changedPath;$cs.dependencies[0].originalBlob=$oldBlob;$cs.dependencies[0].candidateBlob=$newBlob;$cs.dependencies[0].comparison='REVIEWED_UNCHANGED_OBSERVED_PATH'
$review=Proof 'unit-scoped-review.json' ([pscustomobject]@{mission='Sprints3031OutsiderCelestialPilot';integratedMaster='60175837909f75e7507200bc1e22a6add04ddd51';decisions=@([pscustomobject]@{path=$changedPath;originalBlob=$oldBlob;candidateBlob=$newBlob;outcome=$cs.observedOutcome;disposition='UNCHANGED_OBSERVED_PATH';changedCode='UNIT TEST: integration parent-link change';unchangedConsumerReason='UNIT TEST ONLY; verifies scoped review binding, not actual mechanics acceptance'})})
$cs|Add-Member dependencyReviewProof $review
$changed.coverage[0].proofs=@($cs.receiptProof,$cs.nativeProof,$cs.profileProof,$review)
Validate $changed;$checks++
$cs.dependencies[0].comparison='IDENTICAL_RELEVANT_BYTES'
$blocked=$false;try{Validate $changed}catch{$blocked=$true}
if(-not $blocked){throw 'Changed dependency accepted as identical'};$checks++
$cs.dependencies[0].comparison='REVIEWED_UNCHANGED_OBSERVED_PATH';$cs.dependencyReviewProof.sha256=('f'*64)
$blocked=$false;try{Validate $changed}catch{$blocked=$true}
if(-not $blocked){throw 'Invalid scoped review hash accepted'};$checks++
$refused=$false
try { Assert-KmgClosureProofFiles -Ledger $ledger } catch { $refused=$true }
if(-not $refused) { throw 'Non-batch development refusal weakened.' };$checks++
# Only the two exact installed references added by integrated cleanup may be
# absent from an older receipt; every original reference must remain exact.
$extendedCandidate=$candidate|ConvertTo-Json -Depth 30|ConvertFrom-Json
$extra=@([pscustomobject]@{relativePath='UniRx.dll';sha256='bfee7eca22156d8b95c149ba1383f065124b7b44a62ed4eafc32a5e67f6bc68a'},[pscustomobject]@{relativePath='UnityEngine.ClothModule.dll';sha256='390e7edea7c242f764c4645c8582cf02bfdb3d2d86f51d4ea8d8b91ac5e57e91'})
$extendedCandidate.references.files=@($extendedCandidate.references.files)+$extra
$reuse=$ledger.coverage[0]|ConvertTo-Json -Depth 30|ConvertFrom-Json
$refReview=Proof 'unit-reference-review.json' ([pscustomobject]@{mission='Sprints3031OutsiderCelestialPilot';integratedMaster='60175837909f75e7507200bc1e22a6add04ddd51';referenceAdditions=[pscustomobject]@{identities=@($extra|ForEach-Object {$_.relativePath+':'+$_.sha256});unchangedConsumerReason='UNIT FIXTURE ONLY: exact addition contract'}})
$reuse.evidenceSources[0]|Add-Member dependencyReviewProof $refReview
$reuse.proofs+=@($refReview)
Assert-KmgSprints3031ReusedMechanics $reuse $extendedCandidate;$checks++
$extendedCandidate.references.files[1].sha256=('f'*64)
$blocked=$false;try {Assert-KmgSprints3031ReusedMechanics $reuse $extendedCandidate}catch{$blocked=$true}
if(-not $blocked){throw 'Wrong additional reference hash accepted'};$checks++
$extendedCandidate.references.files[1].sha256='bfee7eca22156d8b95c149ba1383f065124b7b44a62ed4eafc32a5e67f6bc68a'
$extendedCandidate.references.files[0].sha256=('e'*64)
$blocked=$false;try {Assert-KmgSprints3031ReusedMechanics $reuse $extendedCandidate}catch{$blocked=$true}
if(-not $blocked){throw 'Changed original reference accepted'};$checks++
Write-Host "S30-31 publisher behavior PASS=$checks; synthetic unit fixtures only, no release qualification."

$budgetPath=Join-Path $temp 'budget.json';$now=[DateTime]::UtcNow
$budget=[ordered]@{schemaVersion=1;authority='Owner bounded Sprints30-31 closeout decision';startUtc=$now.ToString('o');deadlineUtc=$now.AddHours(7).ToString('o');readinessDeadlineUtc=$now.AddMinutes(120).ToString('o');maximumBuildAttempts=3;maximumGameLaunches=24;buildAttemptCount=0;actualGameLaunchCount=0;currentStage='no-game-readiness';status='ACTIVE';events=@();ownerStopMarker=(Join-Path $temp 'OWNER-STOP');restorationOverrunSeconds=0}
[IO.File]::WriteAllText($budgetPath,($budget|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
$refused=$false;try { Invoke-KmgSprints3031Budget $budgetPath Game test|Out-Null } catch {$refused=$true}
if(-not $refused) {throw 'Game admitted before readiness'}
Invoke-KmgSprints3031Budget $budgetPath Ready hidden|Out-Null
Invoke-KmgSprints3031Budget $budgetPath Build hidden|Out-Null
Invoke-KmgSprints3031Budget $budgetPath Game hidden|Out-Null
$b=Get-Content $budgetPath -Raw|ConvertFrom-Json
if($b.buildAttemptCount -ne 1 -or $b.actualGameLaunchCount -ne 1 -or $b.events.Count -ne 3) {throw 'Predispatch counts not persisted'}
[IO.File]::WriteAllText($budget.ownerStopMarker,'CONTROL ONLY')
$refused=$false;try {Invoke-KmgSprints3031Budget $budgetPath Game test|Out-Null} catch {$refused=$true}
if(-not $refused) {throw 'Owner stop marker ignored'}
Invoke-KmgSprints3031Budget $budgetPath Stop stopped|Out-Null
Write-Host 'S30-31 budget guard accepted persisted counters and refused pre-readiness/owner-stop dispatch; no actual launch.'

# Each negative has its own immutable source fixture; no actual dispatch occurs.
function Check-BudgetRefusal([string]$Name,[scriptblock]$Change) {
    $test=$budget|ConvertTo-Json -Depth 12|ConvertFrom-Json
    $test.ownerStopMarker=Join-Path $temp 'not-present-control-marker'
    $test.currentStage='hidden';$test.status='ACTIVE'
    & $Change $test
    $p=Join-Path $temp ('budget-'+$Name+'.json')
    [IO.File]::WriteAllText($p,($test|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
    $blocked=$false;try {Invoke-KmgSprints3031Budget $p Game test|Out-Null} catch {$blocked=$true}
    if(-not $blocked){throw ('Budget admitted '+$Name)}
}
Check-BudgetRefusal 'build-cap' {$args[0].buildAttemptCount=3}
Check-BudgetRefusal 'game-cap' {$args[0].actualGameLaunchCount=24}
Check-BudgetRefusal 'extended-deadline' {$args[0].deadlineUtc=$now.AddHours(8).ToString('o')}
Check-BudgetRefusal 'expired' {$b=$args[0];$s=$now.AddHours(-8);$b.startUtc=$s.ToString('o');$b.deadlineUtc=$s.AddHours(7).ToString('o');$b.readinessDeadlineUtc=$s.AddMinutes(120).ToString('o')}
Check-BudgetRefusal 'readiness-expired' {$b=$args[0];$s=$now.AddHours(-3);$b.startUtc=$s.ToString('o');$b.deadlineUtc=$s.AddHours(7).ToString('o');$b.readinessDeadlineUtc=$s.AddMinutes(120).ToString('o');$b.currentStage='no-game-readiness'}
$realPath=Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/closeout-triage/CLOSEOUT-BUDGET.json'
if(Test-Path -LiteralPath $realPath) {
    $real=Get-Content -LiteralPath $realPath -Raw|ConvertFrom-Json
    if($real.PSObject.Properties.Name -contains 'authorizedSuspension') {
        $p=Join-Path $temp 'authorized-pause.json'
        $real.ownerStopMarker=Join-Path $temp 'not-present-control-marker'
        [IO.File]::WriteAllText($p,($real|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
        Invoke-KmgSprints3031Budget $p Check test|Out-Null
        if($real.PSObject.Properties.Name -contains 'compileCeilingAmendment') {
            $real.compileCeilingAmendment.excludedTicks++
            [IO.File]::WriteAllText($p,($real|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
            $blocked=$false;try {Invoke-KmgSprints3031Budget $p Check test|Out-Null} catch {$blocked=$true}
            if(-not $blocked){throw 'Unapproved second suspension admitted'}
            $real.compileCeilingAmendment.excludedTicks--
        }
        $real.authorizedSuspension.excludedTicks++
        [IO.File]::WriteAllText($p,($real|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
        $blocked=$false;try {Invoke-KmgSprints3031Budget $p Check test|Out-Null} catch {$blocked=$true}
        if(-not $blocked){throw 'Unapproved pause duration admitted'}
    }
}
Write-Host 'S30-31 budget ceilings, deadlines and exact authorized suspension checks PASS.'
