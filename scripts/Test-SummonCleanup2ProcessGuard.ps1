<# Disposable process-boundary regression. No game, real save, registry or
   human install is used. The original wrapper process must not unwind from
   a failed native child while its separate stand-in process remains alive. #>
[CmdletBinding()]
param([ValidateSet('Parent','GuardChild','StandIn')][string]$Mode='Parent',
    [string]$ControlDirectory,[int]$StandInProcessId=0,[switch]$FailWaitingEvidence)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'

if($Mode -ceq 'StandIn') {
    [IO.File]::WriteAllText((Join-Path $ControlDirectory 'standin-ready'),[string]$PID)
    while(-not [IO.File]::Exists((Join-Path $ControlDirectory 'exit-standin'))) {[Threading.Thread]::Sleep(50)}
    exit 0
}

. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.TestFixtures.ps1')

if($Mode -ceq 'GuardChild') {
    $script:guardStandInId=$StandInProcessId
    $script:guardControl=$ControlDirectory
    $script:failWaitingEvidence=[bool]$FailWaitingEvidence
    $script:currentCase=New-Case 'guard-process-boundary'
    [IO.File]::WriteAllText((Join-Path $script:currentCase.environment.saveDirectory 'unrelated.zks'),'protected unrelated fixture')
    function Assert-KmgNotRunning {
        if($script:currentCase.running -and (Get-Process -Id $script:guardStandInId -ErrorAction SilentlyContinue)) {
            throw 'Separate stand-in game is still running.'
        }
    }
    function Invoke-KmgSummonCleanup2Launch {
        param($Plan,$Lease,[string]$Deployment,[string]$ScriptsDirectory)
        Assert-KmgRuntimeLease $Lease
        $script:currentCase.running=$true
        $descriptor=[ordered]@{wrapperProcessId=$PID;standInProcessId=$script:guardStandInId;
            fixtureRoot=$script:currentCase.root;stateRoot=$script:currentCase.environment.stateRoot;
            evidenceRoot=$script:currentCase.environment.evidenceRoot;lockPath=$Lease.LockPath;
            install=$script:currentCase.environment.install;runId=$Lease.RunId;
            saves=@(Get-ChildItem -LiteralPath $script:currentCase.environment.saveDirectory -File | ForEach-Object FullName)}
        Write-KmgCompatibilityJsonAtomic (Join-Path $script:guardControl 'handles-owned.json') $descriptor
        # Models the reviewed native completion failure: no final result and
        # automatic game exit suppressed. Deliberately unhandled at top level.
        throw 'Fixture native result write failed; automatic stand-in exit suppressed.'
    }
    $script:guardOriginalWriter=(Get-Command Write-KmgCompatibilityJsonAtomic).ScriptBlock
    function Write-KmgCompatibilityJsonAtomic([string]$Path,$Value) {
        if($script:failWaitingEvidence -and $Value -is [Collections.IDictionary] -and
            $Value.Contains('protectedExitGuard') -and $Value.protectedExitGuard.status -ceq 'WAITING_FOR_NATIVE_EXIT') {
            [IO.File]::WriteAllText((Join-Path $script:guardControl 'waiting-evidence-failed'),'expected synthetic evidence IO failure')
            throw 'Fixture waiting-status evidence write denied.'
        }
        & $script:guardOriginalWriter $Path $Value
    }
    Invoke-KmgSummonCleanup2Transaction (Plan $script:currentCase) $script:currentCase.environment | Out-Null
    throw 'Fixture expected its original native failure to survive restoration.'
}

function Start-GuardFixtureProcess([string]$RunMode,[string]$Control,[int]$StandInId,[bool]$EvidenceFault,[string]$Name) {
    $exe=(Get-Process -Id $PID).Path
    $qScript=$PSCommandPath.Replace("'","''");$qControl=$Control.Replace("'","''")
    $code="& '$qScript' -Mode '$RunMode' -ControlDirectory '$qControl' -StandInProcessId $StandInId"
    if($EvidenceFault){$code+=' -FailWaitingEvidence'}
    $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($code))
    Start-Process -FilePath $exe -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-EncodedCommand',$encoded) `
        -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Control ($Name+'.stdout.log')) `
        -RedirectStandardError (Join-Path $Control ($Name+'.stderr.log'))
}
function Wait-GuardFixtureFile([string]$Path,$Process,[int]$Seconds=20) {
    $deadline=[DateTime]::UtcNow.AddSeconds($Seconds)
    while(-not [IO.File]::Exists($Path) -and [DateTime]::UtcNow -lt $deadline) {
        $Process.Refresh()
        if($Process.HasExited){throw "Fixture process exited before its handshake: $Path"}
        [Threading.Thread]::Sleep(50)
    }
    if(-not [IO.File]::Exists($Path)){throw "Fixture handshake timeout: $Path"}
}
function Test-GuardDenied([scriptblock]$Operation) {
    try {& $Operation;return $false} catch [IO.IOException] {return $true} catch [UnauthorizedAccessException] {return $true}
}

foreach($fault in @($false,$true)) {
    $control=Join-Path $testRoot ('process-guard-'+[Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($control)
    $standin=$null;$wrapper=$null
    try {
        $standin=Start-GuardFixtureProcess 'StandIn' $control 0 $false 'standin'
        Wait-GuardFixtureFile (Join-Path $control 'standin-ready') $standin
        $wrapper=Start-GuardFixtureProcess 'GuardChild' $control $standin.Id $fault 'wrapper'
        $handshake=Join-Path $control 'handles-owned.json'
        Wait-GuardFixtureFile $handshake $wrapper
        $owned=Read-KmgCompatibilityJson $handshake
        Check ($owned.wrapperProcessId -eq $wrapper.Id -and $owned.standInProcessId -eq $standin.Id) 'distinct-owning-wrapper-and-standin-processes'
        if($fault){Wait-GuardFixtureFile (Join-Path $control 'waiting-evidence-failed') $wrapper}
        else {
            $waiting=Join-Path $owned.evidenceRoot ($owned.runId+'/installed-stack.json')
            $deadline=[DateTime]::UtcNow.AddSeconds(10);$record=$null
            do {
                try {$record=Read-KmgCompatibilityJson $waiting} catch {}
                if($record -and $record.PSObject.Properties.Name -contains 'protectedExitGuard'){break}
                [Threading.Thread]::Sleep(50)
            } while([DateTime]::UtcNow -lt $deadline)
            Check ($null -ne $record -and $record.PSObject.Properties.Name -contains 'protectedExitGuard' -and
                $record.protectedExitGuard.ownerProcessId -eq $wrapper.Id -and
                $record.restoration.status -ceq 'WAITING_FOR_NATIVE_EXIT') 'waiting-receipt-identifies-live-guard-owner'
        }
        [Threading.Thread]::Sleep(750)
        $wrapper.Refresh();$standin.Refresh()
        Check (-not $wrapper.HasExited -and -not $standin.HasExited) 'unhandled-native-error-does-not-exit-guard-process'
        foreach($save in $owned.saves) {
            Check (Test-GuardDenied { $stream=[IO.File]::Open($save,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::Read);$stream.Dispose() }) 'separate-process-save-write-denied'
            Check (Test-GuardDenied { [IO.File]::Delete($save) }) 'separate-process-save-delete-denied'
            Check (Test-GuardDenied { [IO.File]::Move($save,$save+'.forbidden-rename') }) 'separate-process-save-rename-denied'
        }
        Check (Test-GuardDenied {$stream=[IO.File]::Open($owned.lockPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None);$stream.Dispose()}) 'live-guard-retains-exclusive-transaction-boundary'
        $staged=Join-Path $owned.install 'Mods/.kmg-compat-sentinel.json'
        Check (Test-Path -LiteralPath $staged) 'installation-not-restored-before-native-exit'
        # Only the stand-in's own normal exit is requested. Never kill the
        # guard while a simulated game is alive.
        [IO.File]::WriteAllText((Join-Path $control 'exit-standin'),'normal fixture exit')
        Check ($standin.WaitForExit(10000)) 'standin-exits-normally'
        Check ($wrapper.WaitForExit(20000)) 'original-guard-process-actually-exits-after-native-exit'
        $wrapper.Refresh()
        Check ($wrapper.HasExited -and $wrapper.ExitCode -ne 0) 'original-native-failure-remains-unhandled-nonzero-exit'
        $record=Read-KmgCompatibilityJson (Join-Path $owned.evidenceRoot ($owned.runId+'/installed-stack.json'))
        Check ($record.status -ceq 'FAILED' -and $record.restoration.verified -and $record.restoration.settingsAndSavesExact -and
            $record.protectedExitGuard.status -ceq 'EXIT_CONFIRMED') 'failed-result-retained-with-verified-post-exit-restoration'
        Check (-not(Test-Path -LiteralPath $owned.lockPath)) 'transaction-released-only-after-confirmed-exit'
        Check (Test-KmgCompatibilityManifestEqual @($record.originalManifest) @(Get-KmgCompatibilityDirectoryManifest (Join-Path $owned.install 'Mods'))) 'exact-fixture-install-restored-after-wrapper-exit'
        foreach($save in $owned.saves) {
            $stream=[IO.File]::Open($save,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None);$stream.Dispose()
            Check ([IO.File]::Exists($save)) 'save-lock-released-after-game-and-wrapper-exit'
        }
        if($fault){Check (-not [string]::IsNullOrWhiteSpace($record.protectedExitGuard.lastEvidenceError)) 'waiting-evidence-failure-retained-without-dropping-handles'}
    } finally {
        [IO.File]::WriteAllText((Join-Path $control 'exit-standin'),'fixture safe cleanup')
        if($standin -and -not $standin.HasExited){[void]$standin.WaitForExit(10000)}
        if($wrapper -and -not $wrapper.HasExited -and ($null -eq $standin -or $standin.HasExited)) {
            if(-not $wrapper.WaitForExit(20000)){throw 'Fixture guard did not finish after confirmed stand-in exit; retained for diagnosis.'}
        }
    }
}
$result=[ordered]@{schemaVersion=1;kind='NoGameProcessBoundaryTests';checks=$checks;failures=@($failures.ToArray());
    elapsedSeconds=$testClock.Elapsed.TotalSeconds;evidenceDirectory=$testRoot;nativeRuntimeExecuted=$false}
Json $result (Join-Path $testRoot 'process-test-summary.json')
if($failures.Count){throw ('Cleanup2 process guard tests failed: '+($failures -join ', '))}
$result | ConvertTo-Json -Depth 5
