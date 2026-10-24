<# Disposable actual-process qualification of the cold transaction. All paths
   are confined to repository artifacts. Native execution, registry and package
   content validation are fixture boundaries; no game or real save is used. #>
[CmdletBinding()]
param(
    [ValidateSet('Coordinator','Host','NativeChild','LateChild')][string]$Role='Coordinator',
    [string]$CaseRoot,
    [ValidateSet('positive','child-fail-1','child-fail-2','child-fail-3','early-error-1','early-error-2','early-error-3','query-error','late-final')][string]$Mode='positive',
    [ValidateRange(1,3)][int]$Phase=1
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repository=Split-Path -Parent $PSScriptRoot
$processRoot=[IO.Path]::GetFullPath((Join-Path $repository 'artifacts/cleanup2-cold-process-tests')).TrimEnd('\')+'\'
$scriptPath=$PSCommandPath
$powershell=Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe'
function Assert-ProcessFixturePath([string]$Path,[string]$Boundary) {
    $full=[IO.Path]::GetFullPath($Path)
    if(-not $full.StartsWith($Boundary,[StringComparison]::OrdinalIgnoreCase)){throw 'Cold process fixture path escaped its artifact boundary.'}
    return $full
}
function Write-ProcessFixtureJson($Value,[string]$Path) {
    [IO.File]::WriteAllText($Path,($Value|ConvertTo-Json -Depth 16),[Text.UTF8Encoding]::new($false))
}
function Wait-ProcessFixtureFile([string]$Path,[int]$Seconds=35) {
    $watch=[Diagnostics.Stopwatch]::StartNew()
    while(-not(Test-Path -LiteralPath $Path)) {
        if($watch.Elapsed.TotalSeconds -gt $Seconds){throw ('Cold process fixture timed out: '+[IO.Path]::GetFileName($Path))}
        Start-Sleep -Milliseconds 40
    }
}
if($Role -cne 'Coordinator'){$CaseRoot=Assert-ProcessFixturePath $CaseRoot $processRoot}
if($Role -ceq 'LateChild') {
    $spec=Get-Content -LiteralPath (Join-Path $CaseRoot 'late-spec.json') -Raw|ConvertFrom-Json
    $self=[Diagnostics.Process]::GetCurrentProcess()
    try {$started=$self.StartTime.ToUniversalTime().Ticks}finally{$self.Dispose()}
    $spec|Add-Member NoteProperty processId $PID;$spec|Add-Member NoteProperty startedUtcTicks $started
    Write-ProcessFixtureJson $spec (Join-Path $CaseRoot 'late-ready.json')
    Wait-ProcessFixtureFile (Join-Path $CaseRoot 'late-release') 50
    exit 0
}
if($Role -ceq 'NativeChild') {
    $spec=Get-Content -LiteralPath (Join-Path $CaseRoot "phase-$Phase-spec.json") -Raw|ConvertFrom-Json
    $fixtureBoundary=[IO.Path]::GetFullPath((Join-Path $repository 'artifacts/cleanup2-currentstack-tests')).TrimEnd('\')+'\'
    $working=Assert-ProcessFixturePath $spec.working $fixtureBoundary
    $dir=Assert-ProcessFixturePath $spec.evidenceDirectory ($CaseRoot.TrimEnd('\')+'\')
    [void][IO.Directory]::CreateDirectory($dir)
    if($Phase -lt 3){[IO.File]::WriteAllText($working,'native stand-in phase '+$Phase)}
    else {
        $denied=$false
        try {[IO.File]::WriteAllText($working,'forbidden absence write')}catch [IO.IOException] {$denied=$true}
        if(-not $denied){throw 'Absence stand-in could write Working.'}
    }
    $self=[Diagnostics.Process]::GetCurrentProcess()
    try {$started=$self.StartTime.ToUniversalTime().Ticks}finally{$self.Dispose()}
    Write-ProcessFixtureJson @{processId=$PID;startedUtcTicks=$started;phase=$Phase;ownerProcessId=$spec.ownerProcessId;
        runId=$spec.runId;working=$working;baseline=$spec.baseline;otherSave=$spec.otherSave;backup=$spec.backup;
        lock=$spec.lock;ownership=$spec.ownership;mods=$spec.mods;originalMods=$spec.originalMods;stateRoot=$spec.stateRoot;
        coldJournal=$spec.coldJournal} (Join-Path $CaseRoot "phase-$Phase-ready.json")
    Wait-ProcessFixtureFile (Join-Path $CaseRoot "phase-$Phase-release") 50
    if($Mode -ceq "child-fail-$Phase"){exit 7}
    Write-ProcessFixtureJson @{runId=$spec.runId;evidenceDirectory=$dir;scenario=$spec.scenario;expectedModVersion='0.0.163';
        exitAfterCompletion=$true;parameters=@{saveName='KMG_AUTOMATION_WORKING';persistenceScope='giant-humanoids-ii'}} (Join-Path $dir 'runtime-request.json')
    $assertions=@(@{name='exact-working-save-write';status='PASS'})
    if($Phase -eq 2) {
        $proof=@{moduleEnabled=$true;phase='cold-verify';passed=$true;observerCalledAttachOrRebuild=$false;attachmentInitializationInvoked=$false;
            exactTwoHeadFullBodyAttachment=$true;ownedMaterialAndBorrowedNativeTextureSlots=$true;exactDrivenBaselineAndSameNativeFade=$true;
            rootCount=1;nativePoseSourceHeadCount=1;privateHeadBranchCount=2;authoredRendererCount=1;assignedCount=1;
            ownedTextureCount=0;nativeSourceBodyDisabled=$true;nativeRendererScopePreserved=$true}
        $assertions+=@{name='whole-roster-persistence-ettin-authored-head-cold-verify';status='PASS';observed=($proof|ConvertTo-Json -Compress)}
    }
    Write-ProcessFixtureJson @{runId=$spec.runId;evidenceDirectory=$dir;scenario=$spec.scenario;status='PASS';loadedModVersion='0.0.163';gitCommit=$spec.commit;
        assertions=$assertions;workingSaveSmoke=@{workingMatchCount=1;baselineMatchCount=1;descriptorReferenceCorrelated=$true;
            saveWritingApiObserved=$false;expectedWorkingSaveRoutineCount=$(if($Phase -eq 3){0}else{1})}} (Join-Path $dir 'runtime-result.json')
    Write-ProcessFixtureJson @{gitCommit=$spec.commit;loadedModuleSha256=$spec.dllSha256;moduleVersionId=$spec.dllMvid;
        processId=$PID;processStartedUtcTicks=$started} (Join-Path $dir 'runtime-loaded-build-identity.json')
    exit 0
}
if($Role -ceq 'Host') {
    . (Join-Path $PSScriptRoot 'Test-SummonCleanup2ColdCurrentStack.ps1') -FixtureOnly
    $script:processes=New-Object 'Collections.Generic.List[Diagnostics.Process]'
    $script:queryFailuresRemaining=0
    function Get-KmgSummonCleanup2NativeProcessIds {
        $ids=New-Object 'Collections.Generic.List[int]'
        foreach($process in $script:processes){$process.Refresh();if(-not $process.HasExited){$ids.Add($process.Id)}}
        if($ids.Count -gt 0 -and $script:queryFailuresRemaining -gt 0){$script:queryFailuresRemaining--;throw 'Synthetic native-process query failure.'}
        return $ids.ToArray()
    }
    function Assert-KmgNotRunning {Assert-KmgSummonCleanup2NativeExit}
    function Wait-KmgSummonCleanup2Exit {Assert-KmgSummonCleanup2NativeExit}
    function Restore-KmgCleanup2ColdBoundary($Ownership,$Context,$Record,[string]$JournalPath,$Environment,$Lease) {
        & $script:realColdBoundary $Ownership $Context $Record $JournalPath $Environment $Lease
        if($Mode -ceq 'late-final') {
            Write-ProcessFixtureJson @{ownerProcessId=$PID;working=$Context.working.path;baseline=$script:currentCase.baselinePath;
                otherSave=(Join-Path $Environment.saveDirectory 'unrelated.zks');backup=$Context.backupPath;
                ownership=(Join-Path (Split-Path -Parent $JournalPath) 'cleanup2-cold-ownership.json');lock=$Lease.LockPath;
                coldJournal=$JournalPath} (Join-Path $CaseRoot 'late-spec.json')
            $arguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File',('"'+$scriptPath+'"'),'-Role','LateChild','-CaseRoot',('"'+$CaseRoot+'"'),'-Mode',$Mode)
            $late=Start-Process -FilePath $powershell -ArgumentList $arguments -PassThru -WindowStyle Hidden `
                -RedirectStandardOutput (Join-Path $CaseRoot 'late.stdout') -RedirectStandardError (Join-Path $CaseRoot 'late.stderr')
            $null=$late.Handle;$script:processes.Add($late)
            Wait-ProcessFixtureFile (Join-Path $CaseRoot 'late-ready.json')
        }
    }
    function Invoke-KmgCleanup2ColdLaunch($Plan,[string]$Scenario,$Lease,[string]$Deployment,[string]$ScriptsDirectory) {
        Assert-KmgRuntimeLease $Lease
        if(Test-Path -LiteralPath (Join-Path $CaseRoot 'abort')){throw 'Synthetic coordinator requested cooperative completion.'}
        $case=$script:currentCase;$case.events.Add($Scenario)
        $index=@($case.events|Where-Object {$_ -like 'working-save-*'}).Count
        $runId='process-native-'+[Guid]::NewGuid().ToString('N')
        $evidence=Join-Path $CaseRoot $runId
        $case.settings.Values['original']='native process phase '+$index
        $runRoot=Join-Path $case.environment.stateRoot $Lease.RunId
        $intake=Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $Lease.RunId
        $spec=@{ownerProcessId=$PID;runId=$runId;scenario=$Scenario;working=(Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks');
            baseline=$case.baselinePath;otherSave=(Join-Path $case.environment.saveDirectory 'unrelated.zks');
            backup=(Join-Path $runRoot 'original-working.zks');lock=$Lease.LockPath;ownership=(Join-Path $intake 'cleanup2-cold-ownership.json');
            coldJournal=(Join-Path $intake 'cleanup2-cold-journal.json');stateRoot=$case.environment.stateRoot;
            mods=(Join-Path $case.environment.install 'Mods');originalMods=(Join-Path $case.environment.install ('Mods.kmg-compat-'+$Lease.RunId+'.original'));
            evidenceDirectory=$evidence;commit=$Plan.candidate.commit;dllSha256=$Plan.candidate.dllSha256;dllMvid=$Plan.candidate.dllMvid}
        Write-ProcessFixtureJson $spec (Join-Path $CaseRoot "phase-$index-spec.json")
        $childArguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File',('"'+$scriptPath+'"'),'-Role','NativeChild','-CaseRoot',('"'+$CaseRoot+'"'),'-Mode',$Mode,'-Phase',$index)
        $process=Start-Process -FilePath $powershell -ArgumentList $childArguments -PassThru -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $CaseRoot "phase-$index.stdout") -RedirectStandardError (Join-Path $CaseRoot "phase-$index.stderr")
        # Cache the launched-process handle before a HasExited poll. Windows
        # PowerShell's Start-Process object can otherwise lose ExitCode access.
        $null=$process.Handle
        $script:processes.Add($process)
        Wait-ProcessFixtureFile (Join-Path $CaseRoot "phase-$index-ready.json")
        if($Mode -ceq "early-error-$index" -or $Mode -ceq 'query-error'){
            if($Mode -ceq 'query-error'){$script:queryFailuresRemaining=3}
            throw ('Synthetic launcher failure with native child still alive: phase '+$index)
        }
        if(-not $process.WaitForExit(55000)){throw 'Synthetic child did not complete cooperatively.'}
        if($process.ExitCode -ne 0){throw ('Synthetic native child failed: phase '+$index+' code '+$process.ExitCode)}
        return [pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId=$runId;evidenceDirectory=$evidence}
    }
    $case=New-ColdCase ('process-'+$Mode);$script:currentCase=$case
    $before=Get-KmgCleanup2ColdWorkingRecord (Join-Path $case.environment.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks')
    $plan=Cold-Plan $case;$errorMessage=$null;$receipt=$null
    try {$receipt=Invoke-KmgCleanup2ColdCurrentStackTransaction $plan $case.environment}catch {$errorMessage=$_.Exception.Message}
    $after=Get-KmgCleanup2ColdWorkingRecord $before.path
    $journalPath=Join-Path (Join-Path (Join-Path $case.environment.stateRoot 'cleanup2-cold-intake') $case.runId) 'cleanup2-cold-journal.json'
    $journal=Read-KmgCompatibilityJson $journalPath
    Write-ProcessFixtureJson @{mode=$Mode;error=$errorMessage;phaseCount=@($journal.phases).Count;status=$journal.status;
        workingExact=(Test-KmgCleanup2ColdWorkingRecord $before $after);
        installationExact=(Test-KmgCompatibilityManifestEqual $case.originalManifest @(Get-KmgCompatibilityDirectoryManifest (Join-Path $case.environment.install 'Mods')));
        settingsExact=($case.settings.Values.Count -eq 1 -and $case.settings.Values.original -ceq 'human-original');
        lockReleased=(-not(Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock')));
        recoveryRequired=$journal.recoveryRequired;protectedExitGuard=$journal.protectedExitGuard;fixtureChecks=$script:checks;fixtureFailures=@($script:failures.ToArray());
        nativeRuntimeExecuted=$false} (Join-Path $CaseRoot 'host-result.json')
    foreach($process in $script:processes){if($process.HasExited){$process.Dispose()}}
    exit 0
}
$clock=[Diagnostics.Stopwatch]::StartNew();$checks=0;$failures=New-Object 'Collections.Generic.List[string]'
$suite=Join-Path $processRoot ([Guid]::NewGuid().ToString('N'));[void][IO.Directory]::CreateDirectory($suite)
function Check-Process([bool]$Passed,[string]$Name){$script:checks++;if(-not $Passed){$script:failures.Add($Name)}}
function Test-ProcessWriteDenied([scriptblock]$Action){try{& $Action;return $false}catch [IO.IOException]{return $true}catch [UnauthorizedAccessException]{return $true}}
foreach($mode in @('positive','child-fail-1','child-fail-2','child-fail-3','early-error-1','early-error-2','early-error-3','query-error','late-final')) {
    $case=Join-Path $suite $mode;[void][IO.Directory]::CreateDirectory($case)
    $hostArguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File',('"'+$scriptPath+'"'),'-Role','Host','-CaseRoot',('"'+$case+'"'),'-Mode',$mode)
    $hostProcess=Start-Process -FilePath $powershell -ArgumentList $hostArguments -PassThru -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $case 'host.stdout') -RedirectStandardError (Join-Path $case 'host.stderr')
    $null=$hostProcess.Handle
    $maxPhase=if($mode -match '-([123])$'){[int]$Matches[1]}elseif($mode -ceq 'query-error'){1}else{3}
    $processIds=New-Object 'Collections.Generic.List[int]'
    try {
        for($index=1;$index -le $maxPhase;$index++) {
            Wait-ProcessFixtureFile (Join-Path $case "phase-$index-ready.json")
            $ready=Get-Content -LiteralPath (Join-Path $case "phase-$index-ready.json") -Raw|ConvertFrom-Json
            $child=[Diagnostics.Process]::GetProcessById($ready.processId)
            try {
                Check-Process (-not $hostProcess.HasExited -and -not $child.HasExited -and $ready.ownerProcessId -eq $hostProcess.Id) "$mode/$index-original-guard-host-alive"
                Check-Process ($child.StartTime.ToUniversalTime().Ticks -eq $ready.startedUtcTicks -and -not $processIds.Contains($child.Id)) "$mode/$index-exact-fresh-process"
                $processIds.Add($child.Id)
                foreach($path in @($ready.baseline,$ready.otherSave,$ready.backup,$ready.ownership,$ready.lock)) {
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::WriteAllText($path,'forbidden cross-process mutation')}) "$mode/$index-write-denied-$([IO.Path]::GetFileName($path))"
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::Delete($path)}) "$mode/$index-delete-denied-$([IO.Path]::GetFileName($path))"
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::Move($path,$path+'.forbidden')}) "$mode/$index-rename-denied-$([IO.Path]::GetFileName($path))"
                }
                Check-Process ((Test-Path -LiteralPath (Join-Path $ready.mods '.kmg-compat-sentinel.json')) -and (Test-Path -LiteralPath $ready.originalMods)) "$mode/$index-no-premature-installation-restoration"
                if($mode -ceq "early-error-$index" -or $mode -ceq 'query-error') {
                    $deadline=[DateTime]::UtcNow.AddSeconds(10)
                    do {$journal=Get-Content -LiteralPath $ready.coldJournal -Raw|ConvertFrom-Json;if($journal.protectedExitGuard){break};Start-Sleep -Milliseconds 40}while([DateTime]::UtcNow -lt $deadline)
                    Check-Process (-not $hostProcess.HasExited -and $journal.protectedExitGuard.status -ceq 'WAITING_FOR_NATIVE_EXIT') "$mode/$index-original-host-retains-guards-on-failure"
                }
            } finally {$child.Dispose();[IO.File]::WriteAllText((Join-Path $case "phase-$index-release"),'cooperative exit')}
        }
        if($mode -ceq 'late-final') {
            Wait-ProcessFixtureFile (Join-Path $case 'late-ready.json')
            $ready=Get-Content -LiteralPath (Join-Path $case 'late-ready.json') -Raw|ConvertFrom-Json
            $late=[Diagnostics.Process]::GetProcessById($ready.processId)
            try {
                Check-Process (-not $hostProcess.HasExited -and -not $late.HasExited -and $ready.ownerProcessId -eq $hostProcess.Id) 'late-final-original-host-and-late-child-alive'
                foreach($path in @($ready.working,$ready.baseline,$ready.otherSave,$ready.backup,$ready.ownership,$ready.lock)) {
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::WriteAllText($path,'forbidden after restoration')}) ('late-final-write-denied-'+[IO.Path]::GetFileName($path))
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::Delete($path)}) ('late-final-delete-denied-'+[IO.Path]::GetFileName($path))
                    Check-Process (Test-ProcessWriteDenied {[IO.File]::Move($path,$path+'.forbidden')}) ('late-final-rename-denied-'+[IO.Path]::GetFileName($path))
                }
                $deadline=[DateTime]::UtcNow.AddSeconds(10)
                do {$journal=Get-Content -LiteralPath $ready.coldJournal -Raw|ConvertFrom-Json;if($journal.protectedExitGuard){break};Start-Sleep -Milliseconds 40}while([DateTime]::UtcNow -lt $deadline)
                Check-Process ($journal.protectedExitGuard.status -ceq 'WAITING_FOR_NATIVE_EXIT' -and -not $hostProcess.HasExited) 'late-final-retained-host-waits-after-generic-restore'
            } finally {$late.Dispose();[IO.File]::WriteAllText((Join-Path $case 'late-release'),'cooperative exit')}
        }
        if(-not $hostProcess.WaitForExit(55000)){throw 'Cold process host did not finish after cooperative child exit.'}
        Wait-ProcessFixtureFile (Join-Path $case 'host-result.json')
        $result=Get-Content -LiteralPath (Join-Path $case 'host-result.json') -Raw|ConvertFrom-Json
        Check-Process ($hostProcess.ExitCode -eq 0 -and @($result.fixtureFailures).Count -eq 0) "$mode-host-fixture-result"
        Check-Process ($result.workingExact -and $result.installationExact -and $result.settingsExact -and $result.lockReleased -and -not $result.recoveryRequired) "$mode-exact-original-restoration"
        if($mode -ceq 'positive'){Check-Process (-not $result.error -and $result.phaseCount -eq 3) 'positive-three-fresh-process-receipts'}
        elseif($mode -ceq 'late-final'){Check-Process ($result.error -like '*remains running*' -and $result.status -ceq 'FAILED_RESTORED') 'late-final-original-late-exit-failure-retained'}
        else {Check-Process ($result.error -like '*Synthetic*' -and $result.status -ceq 'FAILED_RESTORED') "$mode-original-failure-retained"}
    } catch {$failures.Add($mode+':'+$_.Exception.Message)}
    finally {
        [IO.File]::WriteAllText((Join-Path $case 'abort'),'cooperative stop before another child')
        for($index=1;$index -le 3;$index++){[IO.File]::WriteAllText((Join-Path $case "phase-$index-release"),'cooperative cleanup')}
        [IO.File]::WriteAllText((Join-Path $case 'late-release'),'cooperative cleanup')
        if(-not $hostProcess.HasExited){[void]$hostProcess.WaitForExit(55000)}
        if($hostProcess.HasExited){$hostProcess.Dispose()}else{$failures.Add($mode+':guard host remains alive; no force termination used')}
    }
}
$summary=[ordered]@{kind='Cleanup2ColdActualProcessFixtures';checks=$checks;failures=@($failures.ToArray());elapsedSeconds=$clock.Elapsed.TotalSeconds;
    fixtureDirectory=$suite;nativeRuntimeExecuted=$false;forcedTerminationUsed=$false}
Write-ProcessFixtureJson $summary (Join-Path $suite 'process-test-summary.json')
$summary|ConvertTo-Json -Depth 5
if($failures.Count){throw ('Cold process fixture failures: '+($failures -join '; '))}
