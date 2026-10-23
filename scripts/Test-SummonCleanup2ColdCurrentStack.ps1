<# Disposable filesystem/package/registry fixtures only. Native process,
   deployment and result are explicit boundaries; no real save/game is used. #>
[CmdletBinding()]
param([switch]$FixtureOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.TestFixtures.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2ColdCurrentStack.Common.ps1')
$script:realColdRestore=${function:Restore-KmgCleanup2ColdWorking}
$script:realColdOwner=${function:Test-KmgCleanup2ColdOwnerAlive}
$script:realAtomicWriter=${function:Write-KmgCompatibilityJsonAtomic}
$script:realColdBackup=${function:Backup-KmgCleanup2ColdWorking}
$script:realOpenCompatibilityLease=${function:Open-KmgCompatibilityRuntimeLease}
$script:realColdBoundary=${function:Restore-KmgCleanup2ColdBoundary}
$script:realColdWorkingRecord=${function:Get-KmgCleanup2ColdWorkingRecord}
$script:realAtomicBytes=${function:Write-KmgRuntimeAtomicBytes}
$script:realColdEnter=${function:Enter-KmgCompatibilityTransaction}
$script:realColdSettingsOpen=${function:Open-KmgSummonCleanup2Settings}
$script:guardSamples=0
$script:failWorkingSettlement=$false
function Open-KmgSummonCleanup2Settings {
    param([string]$KeyPath,[bool]$Writable)
    if($script:currentCase -and $script:currentCase.mode -ceq 'changed-stack-before-entry' -and -not $Writable) {
        [IO.File]::WriteAllText((Join-Path $script:currentCase.environment.install 'Mods/FixtureMod1/settings.json'),
            'foreign change after captured current inventory')
    }
    & $script:realColdSettingsOpen $KeyPath $Writable
}
function Enter-KmgCompatibilityTransaction {
    param($Resolution,[string]$KingmakerInstallDir,[string]$StateRoot,[string]$RunId,[switch]$FixtureMode)
    if($script:currentCase.mode -ceq 'entry-live-before-refusal') {
        $script:currentCase.runId=$RunId;$script:currentCase.running=$true;$script:guardSamples=0
        throw 'Cold fixture unexpected process before compatibility entry.'
    }
    $state=& $script:realColdEnter @PSBoundParameters
    if($script:currentCase.mode -ceq 'entry-rollback-metadata-debt') {
        $script:currentCase.runId=$RunId
        [void](Restore-KmgCompatibilityTransaction -RunId $RunId -StateRoot $StateRoot -FixtureMode:$FixtureMode)
        $script:failWorkingSettlement=$true
        throw 'Cold fixture entry returned no lease after its own verified rollback.'
    }
    return $state
}
function Get-KmgCleanup2ColdWorkingRecord([string]$Path,[switch]$AllowEmpty) {
    if($script:failWorkingSettlement){$script:failWorkingSettlement=$false;throw 'Cold fixture pre-entry timestamp settlement unavailable.'}
    & $script:realColdWorkingRecord $Path -AllowEmpty:$AllowEmpty
}
function Write-KmgRuntimeAtomicBytes([string]$Path,[byte[]]$Bytes,[string]$ExpectedSha256) {
    if($script:currentCase -and $script:currentCase.mode -cin @('intake-evidence-fail','intake-late-wait','preentry-settlement-debt') -and
        [IO.Path]::GetFileName($Path) -ceq 'preexisting-saves.json') {
        if($script:currentCase.mode -ceq 'intake-late-wait'){$script:currentCase.running=$true;$script:guardSamples=0}
        if($script:currentCase.mode -ceq 'preentry-settlement-debt'){$script:failWorkingSettlement=$true}
        throw 'Cold fixture catalog evidence IO failure.'
    }
    & $script:realAtomicBytes $Path $Bytes $ExpectedSha256
}
function Open-KmgCompatibilityRuntimeLease([string]$RunId,[string]$StateRoot) {
    $lease=& $script:realOpenCompatibilityLease $RunId $StateRoot
    $script:currentCase.runId=$RunId;$script:currentCase.lease=$lease
    return $lease
}
function Backup-KmgCleanup2ColdWorking($Context,[string]$RunRoot) {
    if($script:currentCase.mode -ceq 'backup-setup-fail') {
        [IO.File]::WriteAllText((Join-Path $RunRoot 'original-working.zks'),'partial synthetic backup')
        throw 'Cold fixture backup setup failure; no native writer admitted.'
    }
    & $script:realColdBackup $Context $RunRoot
}
function Restore-KmgCleanup2ColdBoundary($Ownership,$Context,$Record,[string]$JournalPath,$Environment,$Lease) {
    & $script:realColdBoundary $Ownership $Context $Record $JournalPath $Environment $Lease
    if($script:currentCase.mode -cin @('late-recovery-wait','late-final-wait','late-final-settings')) {
        $script:currentCase.running=$true;$script:guardSamples=0
        if($script:currentCase.mode -ceq 'late-final-settings'){$script:currentCase.settings.Values['original']='late foreign settings'}
    }
}
function Test-ColdDenied([scriptblock]$Action) {try{& $Action;return $false}catch [IO.IOException]{return $true}catch [UnauthorizedAccessException]{return $true}}
function Test-KmgCleanup2ColdOwnerAlive($Ownership) {
    if($script:currentCase.mode -ceq 'recovery-original-live'){return $true}
    # Recovery tests run in this fixture host after the transaction function
    # has returned. Real recovery retains its actual PID/start-time predicate.
    return $false
}
function Write-KmgCompatibilityJsonAtomic([string]$Path,$Value) {
    if($script:currentCase -and $script:currentCase.mode -ceq 'abnormal-wait' -and
        $Value -is [Collections.IDictionary] -and $Value.Contains('protectedExitGuard') -and $Value.protectedExitGuard.status -ceq 'WAITING_FOR_NATIVE_EXIT') {
        throw 'Cold fixture waiting evidence IO failure.'
    }
    & $script:realAtomicWriter $Path $Value
}
function Assert-KmgNotRunning {
    if($script:currentCase -and $script:currentCase.running) {
        if($script:currentCase.mode -ceq 'abnormal-wait') {
            $script:guardSamples++
            Check (Test-ColdDenied {[IO.File]::WriteAllText($script:currentCase.baselinePath,'forbidden')}) 'cold-wait-baseline-os-write-denied'
            Check (Test-ColdDenied {$s=[IO.File]::Open($script:currentCase.lease.LockPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None);$s.Dispose()}) 'cold-wait-original-lease-retained'
            Check (Test-Path -LiteralPath (Join-Path $script:currentCase.environment.install 'Mods/.kmg-compat-sentinel.json')) 'cold-wait-no-installation-restore'
            if($script:guardSamples -ge 3){$script:currentCase.running=$false;return}
        }
        throw 'Cold fixture native process is still running.'
    }
}
function Get-KmgSummonCleanup2NativeProcessIds {
    if($script:currentCase -and $script:currentCase.mode -ceq 'entry-live-before-refusal' -and $script:currentCase.running) {
        $script:guardSamples++
        Check (Test-ColdDenied {[IO.File]::WriteAllText($script:currentCase.baselinePath,'forbidden')}) 'cold-entry-refusal-retains-baseline-handle'
        Check (Test-ColdDenied {[IO.File]::WriteAllText((Join-Path $script:currentCase.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'),'forbidden')}) 'cold-entry-refusal-retains-working-handle'
        if($script:guardSamples -ge 3){$script:currentCase.running=$false;return @()};return @(4242)
    }
    if($script:currentCase -and $script:currentCase.mode -ceq 'intake-late-wait' -and $script:currentCase.running) {
        $script:guardSamples++
        Check (Test-ColdDenied {[IO.File]::WriteAllText($script:currentCase.baselinePath,'forbidden')}) 'cold-intake-wait-baseline-lease-held'
        Check (Test-ColdDenied {[IO.File]::WriteAllText((Join-Path $script:currentCase.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'),'forbidden')}) 'cold-intake-wait-working-still-read-only'
        if($script:guardSamples -ge 3){$script:currentCase.running=$false;return @()};return @(4242)
    }
    if($script:currentCase -and $script:currentCase.mode -cin @('late-recovery-wait','late-final-wait','late-final-settings') -and $script:currentCase.running) {
        $script:guardSamples++
        Check (Test-ColdDenied {[IO.File]::WriteAllText($script:currentCase.baselinePath,'forbidden')}) 'cold-recovery-late-wait-retains-save-handles'
        $working=Join-Path $script:currentCase.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
        Check (Test-ColdDenied {[IO.File]::WriteAllText($working,'forbidden late write')}) 'cold-final-restored-working-write-denied'
        Check (Test-ColdDenied {[IO.File]::Delete($working)}) 'cold-final-restored-working-delete-denied'
        Check (Test-ColdDenied {[IO.File]::Move($working,$working+'.moved')}) 'cold-final-restored-working-rename-denied'
        Check (Test-ColdDenied {$s=[IO.File]::Open($script:currentCase.lease.LockPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None);$s.Dispose()}) 'cold-final-owner-lock-retained'
        if($script:guardSamples -ge 3){$script:currentCase.running=$false;return @()}
        return @(4242)
    }
    if($script:currentCase -and $script:currentCase.running) {
        try {Assert-KmgNotRunning} catch {return @(4242)}
    }
    return @()
}
function Invoke-KmgCleanup2ColdLaunch($Plan,[string]$Scenario,$Lease,[string]$Deployment,[string]$ScriptsDirectory) {
    Assert-KmgRuntimeLease $Lease
    $case=$script:currentCase;$case.events.Add($Scenario)
    $phase=@($case.events|Where-Object {$_ -like 'working-save-*'}).Count
    $working=Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    $backup=Join-Path (Join-Path $case.environment.stateRoot $Lease.RunId) 'original-working.zks'
    Check (Test-ColdDenied {[IO.File]::WriteAllText($case.baselinePath,'forbidden')}) 'cold-native-baseline-os-write-denied'
    Check (Test-ColdDenied {[IO.File]::Delete($case.baselinePath)}) 'cold-native-baseline-os-delete-denied'
    Check (Test-ColdDenied {[IO.File]::Move($case.baselinePath,$case.baselinePath+'.moved')}) 'cold-native-baseline-os-rename-denied'
    Check (Test-ColdDenied {[IO.File]::WriteAllText((Join-Path $case.environment.saveDirectory 'unrelated.zks'),'forbidden')}) 'cold-native-unrelated-save-os-write-denied'
    Check (Test-ColdDenied {[IO.File]::WriteAllText($backup,'forbidden backup mutation')}) 'cold-original-backup-protected'
    $case.settings.Values['original']='native-cold-'+$phase
    if($phase -lt 3){[IO.File]::WriteAllText($working,'native prepared/cleaned fixture '+$phase)}
    else {Check (Test-ColdDenied {[IO.File]::WriteAllText($working,'absence phase must be read-only')}) 'cold-absence-working-os-write-denied'}
    if($case.mode -ceq 'not-content-indexed' -and $phase -lt 3) {
        [IO.File]::SetAttributes($working,[IO.FileAttributes]::Archive)
        Check ([int](Get-Item -LiteralPath $working -Force).Attributes -eq 32) ('cold-indexing-flag-native-fixture-cleared-phase-'+$phase)
    }
    if($case.mode -ceq ('child-fail-'+$phase)){throw ('Cold fixture phase '+$phase+' failed.')}
    if($case.mode -ceq 'empty-working' -and $phase -eq 1){[IO.File]::WriteAllBytes($working,[byte[]]@());throw 'Cold fixture partial native save.'}
    if($case.mode -ceq 'interrupted-working' -and $phase -eq 1){[IO.File]::Delete($working);throw 'Cold fixture native save interrupted after removal.'}
    if($case.mode -ceq 'working-debt' -and $phase -eq 1){[IO.File]::SetAttributes($working,[IO.FileAttributes]::ReadOnly);throw 'Cold fixture native failure before restoration refusal.'}
    if($case.mode -ceq 'acl-replaced' -and $phase -eq 1){$acl=Get-Acl -LiteralPath $working;$acl.SetAccessRuleProtection($true,$true);Set-Acl -LiteralPath $working -AclObject $acl;throw 'Cold fixture native replacement changed access ACL.'}
    if($case.mode -ceq 'settings-debt'){$case.failSettings=$true}
    if($case.mode -ceq 'unowned-save' -and $phase -eq 1){[IO.File]::WriteAllText((Join-Path $case.environment.saveDirectory 'foreign.zks'),'unowned')}
    if($case.mode -ceq 'abnormal-wait'){$case.running=$true;throw 'Cold fixture original native result IO failure.'}
    $id='cold-native-'+[Guid]::NewGuid().ToString('N');$dir=Join-Path $case.root $id
    $effective=if($case.mode -ceq 'wrong-receipt'){'working-save-expanded-summoning-verify-absent'}else{$Scenario}
    $writes=if($phase -eq 3){0}else{1}
    Json @{runId=$id;evidenceDirectory=$dir;scenario=$effective;expectedModVersion='0.0.163';exitAfterCompletion=$true;parameters=@{saveName='KMG_AUTOMATION_WORKING';persistenceScope='giant-humanoids-ii'}} (Join-Path $dir 'runtime-request.json')
    $assertions=@(@{name='exact-working-save-write';status='PASS'})
    if($phase -eq 2) {
        $proof=@{moduleEnabled=$true;phase='cold-verify';passed=$true;observerCalledAttachOrRebuild=$false;attachmentInitializationInvoked=$false;
            exactTwoHeadFullBodyAttachment=$true;ownedMaterialAndBorrowedNativeTextureSlots=$true;exactDrivenBaselineAndSameNativeFade=$true;
            rootCount=1;nativePoseSourceHeadCount=1;privateHeadBranchCount=2;authoredRendererCount=1;assignedCount=1;
            ownedTextureCount=0;nativeSourceBodyDisabled=$true;nativeRendererScopePreserved=$true}
        if($case.mode -ceq 'head-proof-mutated'){$proof.attachmentInitializationInvoked=$true}
        if($case.mode -ceq 'head-proof-module-off'){$proof.moduleEnabled=$false}
        if($case.mode -cne 'head-proof-missing'){$assertions+=@{name='whole-roster-persistence-ettin-authored-head-cold-verify';status='PASS';observed=($proof|ConvertTo-Json -Compress)}}
    }
    Json @{runId=$id;evidenceDirectory=$dir;scenario=$effective;status='PASS';loadedModVersion='0.0.163';gitCommit=$Plan.candidate.commit;
        assertions=$assertions;workingSaveSmoke=@{workingMatchCount=1;baselineMatchCount=1;
            descriptorReferenceCorrelated=$true;saveWritingApiObserved=$false;expectedWorkingSaveRoutineCount=$writes}} (Join-Path $dir 'runtime-result.json')
    Json @{gitCommit=$Plan.candidate.commit;loadedModuleSha256=$Plan.candidate.dllSha256;moduleVersionId=$Plan.candidate.dllMvid;processId=(10000+$phase)} (Join-Path $dir 'runtime-loaded-build-identity.json')
    return [pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId=$id;evidenceDirectory=$dir}
}
function New-ColdCase([string]$Mode) {
    $case=New-Case $Mode;$script:currentCase=$case
    [IO.File]::WriteAllText((Join-Path $case.environment.saveDirectory 'unrelated.zks'),'protected unrelated fixture')
    if($Mode -cmatch '^positive-intake-(17|18)$') {
        $count=[int]$Matches[1]
        for($i=16;$i -lt $count;$i++) {
            $dir=Join-Path $case.environment.install ('Mods/AdditionalCurrentMod'+$i)
            [void][IO.Directory]::CreateDirectory($dir)
            Json @{Id=('AdditionalCurrentMod'+$i);Version=('current-intake-'+$i)} (Join-Path $dir 'Info.json')
            [IO.File]::WriteAllText((Join-Path $dir 'settings.json'),'additional-current-settings-'+$i)
        }
        $case.originalManifest=@(Get-KmgCompatibilityDirectoryManifest (Join-Path $case.environment.install 'Mods'))
    }
    if($Mode -ceq 'kmg-identity-mismatch') {
        Json @{Id='ForeignIdentity';Version='actual-intake-0'} (Join-Path $case.environment.install 'Mods/KingmakerGunslinger/Info.json')
    }
    return $case
}
function Cold-Plan($Case) {
    New-KmgCleanup2ColdCurrentStackPlan -RepositoryRoot $Case.repository -PackagePath $Case.layout.PackagePath -ExpectedVersion '0.0.163' `
        -DevelopmentPurpose 'summon-cleanup-iteration2' -SaveName 'KMG_AUTOMATION_WORKING' -PersistenceScope 'giant-humanoids-ii'
}
if($FixtureOnly){return}
foreach($count in @(17,18)) {
    $case=New-ColdCase ('positive-intake-'+$count)
    $before=Get-KmgCleanup2ColdWorkingRecord (Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks')
    $plan=Cold-Plan $case;$script:coldReceipt=$null
    $message=Refusal {$script:coldReceipt=Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment}
    Check ($null -eq $message -and $script:coldReceipt.phaseCount -eq 3) ('cold-current-'+$count+'-mod-intake-exact-trio-admitted')
    $ownership=Read-KmgCompatibilityJson (Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-ownership.json')
    Check ($ownership.intakeModCount -eq $count -and @($ownership.intakeMods).Count -eq $count) ('cold-current-'+$count+'-complete-intake-captured')
    Assert-Restored $case ('cold-current-'+$count)
    Check (Test-KmgCleanup2ColdWorkingRecord $before (Get-KmgCleanup2ColdWorkingRecord $before.path)) ('cold-current-'+$count+'-working-exact-restored')
}
$case=New-ColdCase 'changed-stack-before-entry'
$before=Get-KmgCleanup2ColdWorkingRecord (Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks')
$plan=Cold-Plan $case;$message=Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment}
Check ($message -clike '*Current Mods changed during cold intake*') 'cold-changed-current-intake-refused'
Check ($case.events.Count -eq 0 -and -not(Test-Path -LiteralPath $case.environment.stateRoot)) 'cold-changed-intake-no-transaction-deploy-or-native'
Check ([IO.File]::ReadAllText((Join-Path $case.environment.install 'Mods/FixtureMod1/settings.json')) -ceq
    'foreign change after captured current inventory') 'cold-changed-intake-preserves-unowned-change'
Check (Test-KmgCleanup2ColdWorkingRecord $before (Get-KmgCleanup2ColdWorkingRecord $before.path)) 'cold-changed-intake-original-working-exact'
foreach($mode in @('positive','not-content-indexed','child-fail-1','child-fail-2','child-fail-3','empty-working','interrupted-working','wrong-receipt','abnormal-wait',
    'backup-setup-fail','acl-replaced','head-proof-missing','head-proof-mutated','head-proof-module-off','intake-evidence-fail','intake-late-wait','entry-live-before-refusal','late-final-wait')) {
    $case=New-ColdCase $mode
    $working=Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    if($mode -ceq 'not-content-indexed') {[IO.File]::SetAttributes($working,[IO.FileAttributes]::Archive -bor [IO.FileAttributes]::NotContentIndexed)}
    $before=Get-KmgCleanup2ColdWorkingRecord $working
    $plan=Cold-Plan $case;$receipt=$null;$message=Refusal {$script:coldReceipt=Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment}
    if($mode -cin @('positive','not-content-indexed')) {Check ($null -eq $message -and $script:coldReceipt.phaseCount -eq 3) ($mode+'-exact-trio-positive')}
    else {Check ($null -ne $message) ($mode+'-original-failure-retained')}
    Assert-Restored $case ('cold-'+$mode)
    $after=Get-KmgCleanup2ColdWorkingRecord $before.path
    Check (Test-KmgCleanup2ColdWorkingRecord $before $after) ($mode+'-exact-original-working-bytes-length-path-times-attributes-security')
    Check ($case.settings.Values.Count -eq 1 -and $case.settings.Values.original -ceq 'human-original') ($mode+'-exact-settings-restored')
    if($mode -ceq 'not-content-indexed') {
        $ownership=Read-KmgCompatibilityJson (Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-ownership.json')
        $backup=Join-Path (Join-Path $case.environment.stateRoot $case.runId) 'original-working.zks'
        Check ($before.attributes -eq 8224 -and $ownership.working.attributes -eq 8224 -and $after.attributes -eq 8224) 'cold-indexing-flag-exact-original-owned-and-restored'
        Check ((Get-KmgCompatibilitySha256 $backup) -ceq $before.sha256 -and (Get-Item -LiteralPath $backup -Force).Length -eq $before.length) 'cold-indexing-flag-durable-original-backup-exact'
        Check ($script:coldReceipt.phaseCount -eq 3 -and @($case.events|Where-Object {$_ -like 'working-save-*'}).Count -eq 3) 'cold-indexing-flag-all-three-fresh-fixture-phases'
    }
    if($mode -ceq 'abnormal-wait') {
        $journal=Read-KmgCompatibilityJson (Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-journal.json')
        Check ($journal.protectedExitGuard.status -ceq 'EXIT_CONFIRMED' -and $journal.protectedExitGuard.lastEvidenceError -like '*waiting evidence IO*' -and
            $journal.status -ceq 'FAILED_RESTORED' -and $journal.error -like '*original native result IO*') 'cold-shared-protected-wait-io-failure-and-original-verdict'
    }
}
foreach($flag in @([IO.FileAttributes]::ReadOnly,[IO.FileAttributes]::Hidden,[IO.FileAttributes]::System,[IO.FileAttributes]::Temporary)) {
    $case=New-ColdCase ('unsupported-attributes-'+$flag)
    $working=Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    $plan=Cold-Plan $case
    try {
        [IO.File]::SetAttributes($working,[IO.FileAttributes]::Archive -bor $flag)
        Check (([int](Get-Item -LiteralPath $working -Force).Attributes -band [int]$flag) -ne 0) ('cold-unsupported-attribute-real-fixture-'+$flag)
        $message=Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment}
        Check ($message -clike '*ordinary nonlinked file*') ('cold-unsupported-attribute-refused-'+$flag)
        Check ($case.events.Count -eq 0 -and -not(Test-Path -LiteralPath $case.environment.stateRoot)) ('cold-unsupported-attribute-before-transaction-or-native-'+$flag)
    } finally {[IO.File]::SetAttributes($working,[IO.FileAttributes]::Archive)}
}
$case=New-ColdCase 'late-final-settings';$plan=Cold-Plan $case
Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) 'cold-final-foreign-settings-refused'
$journalPath=Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-journal.json'
$journal=Read-KmgCompatibilityJson $journalPath
Check ($journal.recoveryRequired -and $journal.finalBoundaryDebt -and -not $journal.settingsRestored) 'cold-final-foreign-state-debt-retained'
Check ($case.settings.Values.original -ceq 'late foreign settings') 'cold-final-foreign-settings-preserved'
Check (Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock')) 'cold-final-debt-owned-lock-retained'
$case.mode='late-final-recover'
Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment})) 'cold-final-recovery-cannot-replace-foreign-settings'
Check ($case.settings.Values.original -ceq 'late foreign settings') 'cold-final-recovery-preserves-foreign-settings'
$case.settings.Values.original='human-original'
$fixed=Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment
Check ($fixed.status -ceq 'FAILED_RESTORED') 'cold-final-original-state-revalidation-recovers'
Assert-Restored $case 'cold-final-verified-recovery'
$case=New-ColdCase 'entry-rollback-metadata-debt'
$before=Get-KmgCleanup2ColdWorkingRecord (Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks')
$plan=Cold-Plan $case
Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) 'cold-entry-rollback-original-failure-retained'
$journalPath=Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-journal.json'
$journal=Read-KmgCompatibilityJson $journalPath
Check ($journal.recoveryRequired -and -not $journal.nativeAttempted -and -not $journal.workingRestored) 'cold-entry-rollback-durable-no-native-metadata-debt'
$case.mode='entry-rollback-recovery'
$fixed=Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment
Check ($fixed.status -ceq 'PREENTRY_METADATA_RESTORED' -and -not $fixed.saveBytesWritten) 'cold-entry-rollback-recovers-metadata-without-replacing-save'
Check (Test-KmgCleanup2ColdWorkingRecord $before (Get-KmgCleanup2ColdWorkingRecord $before.path)) 'cold-entry-rollback-original-working-exact'
Assert-Restored $case 'cold-entry-rollback-metadata'
Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment})) 'cold-entry-rollback-repeated-recovery-refused'
foreach($foreign in @($false,$true)) {
    $case=New-ColdCase 'preentry-settlement-debt';$before=Get-KmgCleanup2ColdWorkingRecord (Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks')
    $plan=Cold-Plan $case
    Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) 'cold-preentry-io-failure-keeps-original-error'
    $intake=Get-ChildItem -LiteralPath (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') -Directory|Select-Object -First 1
    $journal=Read-KmgCompatibilityJson (Join-Path $intake.FullName 'cleanup2-cold-journal.json')
    $ownership=Read-KmgCompatibilityJson (Join-Path $intake.FullName 'cleanup2-cold-ownership.json')
    Check ($ownership.kind -ceq 'Cleanup2ColdPreentryMetadataOnly' -and -not $ownership.nativeAdmitted -and -not $journal.nativeAttempted) 'cold-preentry-debt-has-durable-no-native-authority'
    Check (-not(Test-Path -LiteralPath (Join-Path (Join-Path $case.environment.stateRoot $journal.runId) 'transaction.json'))) 'cold-preentry-no-installation-transaction'
    $case.mode='preentry-recover'
    if($foreign) {
        [IO.File]::WriteAllText($before.path,'later foreign fixture bytes')
        Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $journal.runId $journal.ownershipSha256 $case.environment})) 'cold-preentry-cannot-replace-changed-working-bytes'
        Check ([IO.File]::ReadAllText($before.path) -ceq 'later foreign fixture bytes') 'cold-preentry-preserves-foreign-bytes'
        Check (Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock')) 'cold-preentry-foreign-debt-lock-retained'
    } else {
        $fixed=Restore-KmgCleanup2ColdCurrentStackTransaction $journal.runId $journal.ownershipSha256 $case.environment
        Check ($fixed.status -ceq 'PREENTRY_METADATA_RESTORED' -and -not $fixed.saveBytesWritten) 'cold-preentry-recovers-only-own-read-metadata'
        Check (Test-KmgCleanup2ColdWorkingRecord $before (Get-KmgCleanup2ColdWorkingRecord $before.path)) 'cold-preentry-original-working-metadata-exact'
        Assert-Restored $case 'cold-preentry-metadata-recovery'
    }
}
foreach($mode in @('working-debt','settings-debt','unowned-save')) {
    $case=New-ColdCase $mode;$plan=Cold-Plan $case
    Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) ($mode+'-fails')
    $runRoot=Join-Path $case.environment.stateRoot $case.runId;$journal=Read-KmgCompatibilityJson (Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-journal.json')
    $hash=$journal.ownershipSha256
    Check ($journal.recoveryRequired -and $journal.status -ceq 'RECOVERY_REQUIRED') ($mode+'-durable-debt-recorded')
    Check (Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock')) ($mode+'-lock-not-cleared')
    Check (Test-Path -LiteralPath (Join-Path $case.environment.install ('Mods.kmg-compat-'+$case.runId+'.original'))) ($mode+'-original-install-backup-retained')
    $case.running=$true
    Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $hash $case.environment})) ($mode+'-recovery-refuses-live-game')
    $case.running=$false
    Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId ('0'*64) $case.environment})) ($mode+'-recovery-refuses-foreign-ownership-hash')
    if($mode -ceq 'working-debt'){[IO.File]::SetAttributes((Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'),[IO.FileAttributes]::Archive)}
    if($mode -ceq 'settings-debt'){$case.failSettings=$false}
    if($mode -ceq 'unowned-save'){Remove-Item -LiteralPath (Join-Path $case.environment.saveDirectory 'foreign.zks')}
    $case.mode='recovery-original-live'
    Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $hash $case.environment})) ($mode+'-recovery-refuses-original-owner')
    $case.mode=$mode
    $recovered=Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $hash $case.environment
    Check ($recovered.status -ceq 'FAILED_RESTORED') ($mode+'-recovered-with-original-failure-retained')
    Assert-Restored $case ($mode+'-recovery')
    $currentWorking=Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    [IO.File]::WriteAllText($currentWorking,'later synthetic human state')
    Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $hash $case.environment})) ($mode+'-repeat-recovery-refuses-discharged-transaction')
    Check ([IO.File]::ReadAllText($currentWorking) -ceq 'later synthetic human state') ($mode+'-repeat-recovery-preserves-later-bytes')
}
foreach($damage in @('missing','wrong-bytes','wrong-length')) {
    $case=New-ColdCase 'working-debt';$plan=Cold-Plan $case
    Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) ('cold-backup-'+$damage+'-initial-debt')
    $intake=Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId
    $journal=Read-KmgCompatibilityJson (Join-Path $intake 'cleanup2-cold-journal.json')
    $backup=Join-Path (Join-Path $case.environment.stateRoot $case.runId) 'original-working.zks'
    $originalBytes=[IO.File]::ReadAllBytes($backup)
    [IO.File]::SetAttributes($backup,[IO.FileAttributes]::Normal)
    if($damage -ceq 'missing'){[IO.File]::Delete($backup)}
    elseif($damage -ceq 'wrong-length'){[IO.File]::WriteAllBytes($backup,[byte[]]@(1))}
    else {$wrong=[byte[]]$originalBytes.Clone();$wrong[0]=$wrong[0] -bxor 1;[IO.File]::WriteAllBytes($backup,$wrong)}
    $working=Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    [IO.File]::SetAttributes($working,[IO.FileAttributes]::Archive)
    $beforeHash=Get-KmgCompatibilitySha256 $working
    $case.mode='backup-damage-recovery'
    Check ($null -ne (Refusal {Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment})) ('cold-backup-'+$damage+'-recovery-refused')
    Check ((Get-KmgCompatibilitySha256 $working) -ceq $beforeHash) ('cold-backup-'+$damage+'-no-save-replacement')
    Check (Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock')) ('cold-backup-'+$damage+'-debt-lock-retained')
    [IO.File]::WriteAllBytes($backup,$originalBytes);[IO.File]::SetAttributes($backup,[IO.FileAttributes]::ReadOnly)
    $fixed=Restore-KmgCleanup2ColdCurrentStackTransaction $case.runId $journal.ownershipSha256 $case.environment
    Check ($fixed.status -ceq 'FAILED_RESTORED') ('cold-backup-'+$damage+'-valid-backup-recovery')
    Assert-Restored $case ('cold-backup-'+$damage)
}
foreach($mode in @('missing-working','missing-baseline','ambiguous-save','duplicate-id','missing-id','kmg-identity-mismatch')) {
    $case=New-ColdCase $mode;$plan=Cold-Plan $case
    Check ($null -ne (Refusal {Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment})) ($mode+'-intake-refused')
    Check ($case.events.Count -eq 0 -and -not(Test-Path -LiteralPath $case.environment.stateRoot)) ($mode+'-before-transaction-or-launch')
}
$case=New-ColdCase 'plan-refusal'
$base=@{RepositoryRoot=$case.repository;PackagePath=$case.layout.PackagePath;ExpectedVersion='0.0.163';DevelopmentPurpose='summon-cleanup-iteration2';SaveName='KMG_AUTOMATION_WORKING';PersistenceScope='giant-humanoids-ii'}
foreach($change in @(@{DevelopmentPurpose='sprint29-hidden-bootstrap'},@{DevelopmentPurpose='summon-cleanup-iteration2-baseline'},@{PersistenceScope='giant-humanoids'},@{SaveName='KMG_AUTOMATION_BASELINE'},@{ExpectedVersion='0.0.164'})) {
    $args=$base.Clone();foreach($key in $change.Keys){$args[$key]=$change[$key]}
    $reads=$script:fixturePackageReads
    Check ($null -ne (Refusal {New-KmgCleanup2ColdCurrentStackPlan @args})) 'cold-other-scope-purpose-version-save-refused'
    Check ($reads -eq $script:fixturePackageReads) 'cold-invalid-plan-no-package-read'
}
$result=[ordered]@{kind='NoGameCleanup2ColdTests';checks=$checks;failures=@($failures.ToArray());elapsedSeconds=$testClock.Elapsed.TotalSeconds;fixtureDirectory=$testRoot;nativeRuntimeExecuted=$false}
Json $result (Join-Path $testRoot 'cold-test-summary.json')
if($failures.Count){throw ('Cold fixture failures: '+($failures -join ', '))}
$result|ConvertTo-Json -Depth 5
