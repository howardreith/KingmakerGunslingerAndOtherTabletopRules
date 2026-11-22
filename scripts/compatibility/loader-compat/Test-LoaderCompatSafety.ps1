[CmdletBinding()]
param()
# Offline checks of the loader-compatibility session guards. Temporary fixture
# directories only; no game, loader, registry or owner file is touched.
. (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$checks = 0
function Assert-That([bool]$Condition, [string]$Name) { $script:checks++; if (-not $Condition) { $failures.Add($Name) } }
function Get-Refusal([scriptblock]$Action) { try { & $Action; return $null } catch { return $_.Exception.Message } }

$fixture = Join-Path ([IO.Path]::GetTempPath()) ('KmgLcSafety-' + [Guid]::NewGuid().ToString('N'))
$savedRoot = $script:LcCopiesRoot
try {
    $owner = Join-Path $fixture 'owner'
    $copies = Join-Path $fixture 'game-copies'
    foreach ($dir in @($owner, (Join-Path $copies 'good'), (Join-Path $copies 'noexe'), (Join-Path $fixture 'elsewhere'))) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    foreach ($exe in @((Join-Path $owner 'Kingmaker.exe'), (Join-Path $copies 'good\Kingmaker.exe'), (Join-Path $fixture 'elsewhere\Kingmaker.exe'))) { 'x' | Set-Content $exe }
    $script:LcCopiesRoot = $copies

    Assert-That ((Assert-LcGameCopy (Join-Path $copies 'good') $owner) -eq (Join-Path $copies 'good')) 'copy-under-root-accepted'
    Assert-That ((Get-Refusal { Assert-LcGameCopy $owner $owner }) -like "*owner's installation*") 'owner-install-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $owner 'Mods') $owner }) -like "*owner's installation*") 'path-inside-owner-install-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $fixture 'elsewhere') $owner }) -like '*must live under*') 'copy-outside-root-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $copies 'good\..\..\elsewhere') $owner }) -like '*must live under*') 'traversal-outside-root-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $copies 'noexe') $owner }) -like '*lacks Kingmaker.exe*') 'copy-without-game-refused'

    $paths = Get-LcCopyPaths (Join-Path $copies 'good')
    New-Item -ItemType Directory -Force -Path (Join-Path $paths.Pristine 'Mods\KingmakerGunslinger') | Out-Null
    Assert-That ((Get-Refusal { Set-LcGameCopyMods $paths @('KingmakerGunslinger') 'none.zip' $fixture }) -like '*candidate ZIP*') 'owner-copy-of-product-refused'
    Assert-That ((Get-Refusal { Set-LcGameCopyMods $paths @('NotInstalled') 'none.zip' $fixture }) -like '*not found in the pristine set*') 'unknown-owner-mod-refused'

    $before = @('a|1|x', 'b|2|y')
    Assert-That ((Compare-LcManifest $before $before).Count -eq 0) 'unchanged-manifest-equal'
    Assert-That ((Compare-LcManifest $before @('a|1|x', 'b|2|z')).Count -eq 2) 'changed-file-detected'
    Assert-That ((Compare-LcManifest $before @('a|1|x')).Count -eq 1) 'removed-file-detected'
    Assert-That ((Compare-LcManifest $before @('a|1|x', 'b|2|y', 'c|3|w')).Count -eq 1) 'new-file-detected'

    $notCopy = Get-Process -Id $PID
    Assert-That ((Get-Refusal { Stop-LcOwnedProcess $notCopy $paths }) -like '*not the game copy*') 'foreign-process-never-stopped'

    # The session transaction, with a disposable state root, lock files, journal, owner-file manifests and game
    # settings (a .reg text standing for the key), and a substituted process query standing in for a peer game.
    $saved = @{ State = $script:LcStateRoot; Foreign = $script:LcForeignLocks; Journal = $script:LcJournal; Lock = $script:LcLock
        Guard = $script:LcRecoveryLock; Query = $script:LcGameProcessQuery; Saves = $script:LcSavesManifest; Install = $script:LcInstallManifest
        Export = $script:LcRegistryExport; Writer = $script:LcLeaseStateWriter }
    try {
        $state = Join-Path $fixture 'compatibility-state'
        $compatLock = Join-Path $state 'compatibility.lock'
        $peerLock = Join-Path $fixture 'sibling-lab\runtime.lock'
        New-Item -ItemType Directory -Force -Path (Split-Path $peerLock), $state | Out-Null
        $script:LcStateRoot = $state
        $script:LcForeignLocks = @($compatLock, $peerLock)
        $script:LcJournal = Join-Path $copies 'loader-compat.journal.json'
        $script:LcLock = Join-Path $copies 'loader-compat.lock'
        $script:LcRecoveryLock = Join-Path $copies 'loader-compat.recovery.lock'
        $global:LcTestGames = @()
        $script:LcGameProcessQuery = { @($global:LcTestGames) }
        $defaultSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }
        $global:LcTestSaves = $defaultSaves
        $global:LcTestInstall = { @('install\winhttp.dll|5|cc') }
        $script:LcSavesManifest = { & $global:LcTestSaves }
        $script:LcInstallManifest = { & $global:LcTestInstall }
        $regHead = "Windows Registry Editor Version 5.00`r`n`r`n"
        function New-RegText([string]$Screen, [string]$A, [string]$B, [string]$Hex) {
            return $regHead + "[HKEY_CURRENT_USER\Software\Owlcat Games\Pathfinder Kingmaker]`r`n`"screen`"=`"$Screen`"`r`n`r`n" +
                "[HKEY_CURRENT_USER\Software\Owlcat Games\Pathfinder Kingmaker\Audio]`r`n`"volume`"=`"$A`"`r`n`r`n" +
                "[HKEY_CURRENT_USER\Software\Owlcat Games\Pathfinder Kingmaker\Video]`r`n`"volume`"=`"$B`"`r`n`"layout`"=hex:$Hex`r`n"
        }
        $backupText = New-RegText 'owner' '1' '2' "01,02,03,\`r`n  04,05,06"
        $sessionText = New-RegText 'session' '1' '2' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $sessionText
        $global:LcTestExportFailures = 0
        $global:LcTestExportHook = $null
        $script:LcRegistryExport = {
            param($Path)
            if ($null -ne $global:LcTestExportHook) { & $global:LcTestExportHook }
            if ($global:LcTestExportFailures -gt 0) { $global:LcTestExportFailures--; throw 'registry key is unreadable (fixture access denied)' }
            if ($null -eq $global:LcTestRegistry) { return 'absent' }
            [IO.File]::WriteAllText($Path, $global:LcTestRegistry); return 'exported'
        }
        $global:LcTestRestores = 0
        $restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = [IO.File]::ReadAllText($Path) }
        $backup = Join-Path $fixture 'settings.reg'
        [IO.File]::WriteAllText($backup, $backupText)
        $clean = { -not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcLock) -and -not (Test-Path -LiteralPath $script:LcJournal) }
        function New-FixtureTransaction([string]$Id, [switch]$Bare) {
            $global:LcTestRegistry = $sessionText
            $tx = Enter-LcSessionTransaction $Id
            if (-not $Bare) { [void](Save-LcIntake $tx.Journal); Set-LcRegistryBackup $tx.Journal $backup }
            return $tx
        }
        function Set-FixtureOwnerGone {
            $j = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json
            $j.ownerProcessStartedUtc = '2000-01-01T00:00:00.0000000Z'
            Write-LcJson $script:LcJournal $j
        }
        function Get-FixtureJson([string]$Path) { return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json }
        function Invoke-FixtureInterrupted([string]$Boundary, [scriptblock]$Action) {
            $script:LcInterruptAfter = $Boundary
            try { return Get-Refusal $Action } finally { $script:LcInterruptAfter = $null }
        }
        function Stop-FixtureRunner($Tx) { if ($null -ne $Tx.Scope.Lease.Stream) { $Tx.Scope.Lease.Stream.Dispose() }; Set-FixtureOwnerGone }
        function New-LegacyJournal([string]$Session) {
            Write-LcJson $script:LcJournal ([ordered]@{ sessionId = $Session; registryBackup = $backup })
            [IO.File]::WriteAllText($script:LcLock, "session=$Session`npid=$PID`nstartedUtc=2000-01-01T00:00:00.0000000Z`n")
        }

        # --- inputs and admission
        Assert-That ((Get-Refusal { Assert-LcSessionInputs (Join-Path $fixture 'missing.zip') $fixture }) -like 'Candidate ZIP not found*Nothing was acquired.') 'missing-candidate-refused'
        $zip = Join-Path $fixture 'candidate.zip'; [IO.File]::WriteAllText($zip, 'zip')
        Assert-That ((Get-Refusal { Assert-LcSessionInputs $zip (Join-Path $fixture 'no-harness') }) -like 'Harness file missing*') 'missing-harness-refused'
        Assert-That (& $clean) 'refused-inputs-acquired-nothing'
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-refused' }) -like '*lock already exists*') 'foreign-shared-lock-refused'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run' -and -not (Test-Path -LiteralPath $script:LcJournal)) 'foreign-shared-lock-kept-no-journal'
        Remove-Item -LiteralPath $compatLock -Force
        [IO.File]::WriteAllText($script:LcLock, "session=older-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n")
        Assert-That ($null -ne (Get-Refusal { Enter-LcSessionTransaction 'fixture-private' }) -and -not (Test-Path -LiteralPath $compatLock) -and
            -not (Test-Path -LiteralPath $script:LcJournal) -and ((Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=older-session*')) 'stale-private-lock-rollback-releases-only-own'
        Remove-Item -LiteralPath $script:LcLock -Force

        # --- R9-F1: the initial claim is exclusive and complete.
        $txWin = New-FixtureTransaction 'fixture-winner' -Bare
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-loser' }) -like 'Another session holds the loader-compat journal*nothing was acquired.') 'losing-start-refused'
        Assert-That ((Get-FixtureJson $script:LcJournal).transactionId -ceq $txWin.Journal.transactionId -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txWin.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-winner')) 'winner-journal-complete-and-locks-preserved'
        Assert-That ((Complete-LcSessionOwnership $txWin.Journal $txWin.Scope $restore 0).Outcome -eq 'completed' -and (& $clean)) 'winner-completes'
        $refusal = Invoke-FixtureInterrupted 'claim-written:loader-compat.journal.json' { Enter-LcSessionTransaction 'fixture-claim-crash' }
        Assert-That ($refusal -like '*claim-written:loader-compat.journal.json*' -and -not (Test-Path -LiteralPath $script:LcJournal) -and -not (Test-Path -LiteralPath $compatLock)) 'interrupted-claim-leaves-no-journal'
        $txAfterClaim = New-FixtureTransaction 'fixture-after-claim-crash' -Bare
        Assert-That ((Get-FixtureJson $script:LcJournal).transactionId -ceq $txAfterClaim.Journal.transactionId) 'next-start-after-interrupted-claim'
        [void](Complete-LcSessionOwnership $txAfterClaim.Journal $txAfterClaim.Scope $restore 0)
        $txJ = New-FixtureTransaction 'fixture-journal-write'
        $restoresBefore = $global:LcTestRestores
        $refusal = Invoke-FixtureInterrupted 'journal-temp-written' { Complete-LcSessionOwnership $txJ.Journal $txJ.Scope $restore 0 }
        Assert-That ($refusal -like '*journal-temp-written*' -and (Get-FixtureJson $script:LcJournal).transactionId -ceq $txJ.Journal.transactionId) 'interrupted-update-keeps-last-complete-journal'
        Stop-FixtureRunner $txJ
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean) -and
            @(Get-ChildItem -LiteralPath $copies -Force -Filter '*loader-compat.journal.json*').Count -eq 0) 'recovered-from-last-complete-journal'

        # --- R8-F2: partial acquisition always leaves an identifiable claim.
        $script:LcLeaseStateWriter = { param($Lease) throw 'lease state disk full' }
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-lease-write' }) -like '*lease state disk full*') 'lease-write-failure-refused'
        $script:LcLeaseStateWriter = $saved.Writer
        Assert-That (& $clean) 'lease-write-failure-rolled-back'
        $refusal = Invoke-FixtureInterrupted 'claim-written:compatibility.lock' { Enter-LcSessionTransaction 'fixture-lock-claim-crash' }
        Assert-That ($refusal -like '*claim-written:compatibility.lock*' -and -not (Test-Path -LiteralPath $compatLock) -and -not (Get-FixtureJson $script:LcJournal).sharedLock) 'interrupted-lock-claim-leaves-no-lock'
        Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Result.Outcome -eq 'completed' -and (& $clean)) 'interrupted-lock-claim-recovered'
        $refusal = Invoke-FixtureInterrupted 'shared-lock-created' { Enter-LcSessionTransaction 'fixture-shared-crash' }
        $crash = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like '*shared-lock-created*' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $crash.leaseRunId -and -not (Test-Path -LiteralPath $crash.leaseStatePath)) 'shared-lock-crash-identified-by-journal'
        Set-FixtureOwnerGone
        $restoresBefore = $global:LcTestRestores
        Assert-That ((Invoke-LcRecovery $restore).Result.Outcome -eq 'completed' -and $global:LcTestRestores -eq $restoresBefore -and (& $clean)) 'shared-lock-crash-recovered-without-import'
        $refusal = Invoke-FixtureInterrupted 'private-lock-created' { Enter-LcSessionTransaction 'fixture-private-crash' }
        $crash = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like '*private-lock-created*' -and $crash.privateLockIntended -and -not $crash.privateLock -and (Test-LcPrivateLockOwned 'fixture-private-crash')) 'private-lock-crash-recorded-as-intended'
        Set-FixtureOwnerGone
        Assert-That ($null -eq (Get-Refusal { [void](Invoke-LcRecovery $restore) }) -and (& $clean)) 'private-lock-crash-reconciled-and-released'
        # Legacy (pre-round-7) recovery interrupted between taking the shared lock and recording it as taken.
        New-LegacyJournal 'fixture-legacy-crash'
        $refusal = Invoke-FixtureInterrupted 'recovery-lock-acquired' { Invoke-LcRecovery $restore }
        $converted = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like '*recovery-lock-acquired*' -and $converted.schemaVersion -eq 4 -and $converted.leaseRunId -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $converted.leaseRunId) 'legacy-recovery-lock-identified-by-journal'
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and $recovery.Message -like '*records no digest*' -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $converted.leaseRunId) 'legacy-lock-recognized-as-own-and-retained'
        $recovery = Invoke-LcRecovery $restore 'KeepCurrent' $converted.transactionId
        Assert-That ($recovery.Succeeded -eq $false -and $recovery.Result.Outcome -eq 'completed' -and $recovery.Result.Registry -eq 'owner-kept-current' -and (& $clean)) 'legacy-recovered-after-owner-resolution'
        # A schema-2 journal beside its own partially acquired lease, and beside a foreign one.
        $orphanRun = New-LcLeaseRunId
        $orphanStart = '2000-01-01T00:00:00.0000000Z'
        Write-LcJson $script:LcJournal ([ordered]@{ schemaVersion = 2; sessionId = 'fixture-schema2'; phase = 'acquiring'; ownerProcessId = $PID
            ownerProcessStartedUtc = $orphanStart; leaseRunId = $null; leaseStatePath = $null; privateLock = $false; registryBackup = $null; registryRestored = $false; intake = $null; integrity = $null })
        [IO.File]::WriteAllText($compatLock, $orphanRun + "`r`n")
        New-Item -ItemType Directory -Force -Path (Join-Path $state $orphanRun) | Out-Null
        Write-LcJson (Join-Path (Join-Path $state $orphanRun) 'runtime-lease.json') ([ordered]@{ schemaVersion = 1; runId = $orphanRun; purpose = 'loader-compat fixture-schema2'
            processId = $PID; processStartedUtc = $orphanStart; status = 'Active'; recoveryRequired = $false; reason = $null; settings = $null })
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Result.Outcome -eq 'completed' -and (& $clean)) 'schema2-matching-partial-lease-reconciled'
        $foreignRun = New-LcLeaseRunId
        Write-LcJson $script:LcJournal ([ordered]@{ schemaVersion = 2; sessionId = 'fixture-schema2b'; phase = 'acquiring'; ownerProcessId = $PID
            ownerProcessStartedUtc = $orphanStart; leaseRunId = $null; leaseStatePath = $null; privateLock = $false; registryBackup = $null; registryRestored = $false; intake = $null; integrity = $null })
        [IO.File]::WriteAllText($compatLock, $foreignRun + "`r`n")
        New-Item -ItemType Directory -Force -Path (Join-Path $state $foreignRun) | Out-Null
        Write-LcJson (Join-Path (Join-Path $state $foreignRun) 'runtime-lease.json') ([ordered]@{ schemaVersion = 1; runId = $foreignRun; purpose = 'loader-compat another-session'
            processId = 4; processStartedUtc = $orphanStart; status = 'Active'; recoveryRequired = $false; reason = $null; settings = $null })
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*compatibility.lock*' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $foreignRun) 'schema2-foreign-lease-lock-preserved'
        Remove-Item -LiteralPath $compatLock, $script:LcJournal -Force
        $txNext = New-FixtureTransaction 'fixture-after-crashes'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txNext.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-after-crashes')) 'next-session-acquires-after-crashes'
        Assert-That ((Test-LcTransactionSucceeded (Complete-LcSessionOwnership $txNext.Journal $txNext.Scope $restore 0)) -and (& $clean)) 'next-session-completes-cleanly'
        $txSetup = New-FixtureTransaction 'fixture-setup-failure' -Bare
        $outcome = Complete-LcSessionOwnership $txSetup.Journal $txSetup.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'completed' -and $outcome.Registry -eq 'not-needed' -and $outcome.Integrity.status -eq 'unresolved' -and (& $clean)) 'setup-failure-completed-integrity-unresolved'

        # --- R7-F1: a sibling-lab lock after launch one.
        $txA = New-FixtureTransaction 'fixture-sibling'
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $txA.Scope.Lease })) 'launch-one-admitted-with-own-lock'
        [IO.File]::WriteAllText($peerLock, 'sibling')
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $txA.Scope.Lease }) -like 'Launch withheld: another lab holds a runtime lock*runtime.lock*') 'launch-two-withheld-for-sibling-lock'
        $restoresBefore = $global:LcTestRestores
        $outcome = Complete-LcSessionOwnership $txA.Journal $txA.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $global:LcTestRestores -eq $restoresBefore -and (Test-Path -LiteralPath $compatLock) -and (Test-Path -LiteralPath $script:LcLock)) 'sibling-lock-defers-with-ownership'
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The session owner*still alive*nothing was changed.') 'recovery-refused-live-owner'
        Set-FixtureOwnerGone
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: another lab holds a runtime lock*' -and $global:LcTestRestores -eq $restoresBefore) 'recovery-refused-sibling-lock'
        Remove-Item -LiteralPath $peerLock -Force
        $competing = [IO.File]::Open($compatLock, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like "Another caller holds this session's recovery*" -and $global:LcTestRestores -eq $restoresBefore) 'competing-handle-recovery-refused' }
        finally { $competing.Dispose() }
        $guardHolder = New-Object IO.FileStream($script:LcRecoveryLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None, 4096, [IO.FileOptions]::DeleteOnClose)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Another recovery of this session is in progress*') 'concurrent-recovery-refused' }
        finally { $guardHolder.Dispose() }
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'recovery-after-sibling-release'

        # --- R8-F1: a recovery paused after reading session A resumes after A finished and B was left by an exited owner.
        $txP = New-FixtureTransaction 'fixture-paused-a'
        Stop-FixtureRunner $txP
        $global:LcTestB = $null
        $script:LcRecoveryPauseHook = {
            [void](Invoke-LcRecovery $restore)
            $global:LcTestB = New-FixtureTransaction 'fixture-paused-b'
            $global:LcTestB.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
            $global:LcTestBRegistry = $global:LcTestRegistry; $global:LcTestBRestores = $global:LcTestRestores
        }
        $refusal = Get-Refusal { Invoke-LcRecovery $restore }
        Assert-That ($refusal -like 'The session journal now belongs to a different transaction*nothing was changed*') 'delayed-recovery-refuses-replacement-session'
        Assert-That ((Get-FixtureJson $script:LcJournal).transactionId -ceq $global:LcTestB.Journal.transactionId -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $global:LcTestB.Scope.Lease.RunId -and
            $global:LcTestRegistry -ceq $global:LcTestBRegistry -and $global:LcTestRestores -eq $global:LcTestBRestores) 'replacement-session-untouched'
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and (& $clean)) 'replacement-session-recovers-on-its-own-invocation'
        $txP2 = New-FixtureTransaction 'fixture-paused-same'
        Stop-FixtureRunner $txP2
        $restoresBefore = $global:LcTestRestores
        $global:LcTestFirst = $null
        $script:LcRecoveryPauseHook = { $global:LcTestFirst = Invoke-LcRecovery $restore }
        $second = Invoke-LcRecovery $restore
        Assert-That ($global:LcTestFirst.Succeeded -and $second.Succeeded -and $second.Message -like '*already recovered by another caller*Recorded verdict*applied*verified*' -and
            $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'paused-recovery-one-restore-total'
        # R9-F3: the same pause with owner files changed while down - both callers exit nonzero, one cleanup, one import.
        $txP3 = New-FixtureTransaction 'fixture-paused-changed'
        Stop-FixtureRunner $txP3
        $restoresBefore = $global:LcTestRestores
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|changed-while-waiting') }
        $global:LcTestFirstCode = $null
        $script:LcRecoveryPauseHook = { $global:LcTestFirstCode = Invoke-LcRecoverOnlyCommand $restore }
        $secondCode = Invoke-LcRecoverOnlyCommand $restore
        $global:LcTestSaves = $defaultSaves
        Assert-That ($global:LcTestFirstCode -eq 1 -and $secondCode -eq 1 -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean) -and
            (Get-FixtureJson $txP3.Scope.Lease.StatePath).integrity.status -ceq 'changed') 'paused-recovery-failed-verdict-both-exit-1'
        # A waiting caller whose observed journal names no lease record cannot verify the verdict: nonzero.
        New-LegacyJournal 'fixture-paused-unleased'
        $script:LcRecoveryPauseHook = { Remove-Item -LiteralPath $script:LcJournal, $script:LcLock -Force }
        $unverified = Invoke-LcRecovery $restore
        Assert-That (-not $unverified.Succeeded -and $unverified.Message -like '*verdict cannot be verified*') 'paused-recovery-unverifiable-verdict-fails'

        # --- R8-F1: the journal acted on is the single snapshot read under the guard. A finishes normally and B
        # starts and is interrupted right after that read: the caller leaves B's journal, locks and settings alone.
        $txSa = New-FixtureTransaction 'fixture-snapshot-a'
        Set-FixtureOwnerGone
        $global:LcTestSb = $null
        $script:LcRecoverySnapshotHook = {
            [void](Complete-LcSessionOwnership $txSa.Journal $txSa.Scope $restore 0)
            $global:LcTestSb = New-FixtureTransaction 'fixture-snapshot-b'
            [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $global:LcTestSb.Journal $global:LcTestSb.Scope $restore 0 })
            Stop-FixtureRunner $global:LcTestSb
            $global:LcTestBRegistry = $global:LcTestRegistry; $global:LcTestBRestores = $global:LcTestRestores
            $global:LcTestBJournal = Get-Content -LiteralPath $script:LcJournal -Raw
        }
        $refusal = Get-Refusal { Invoke-LcRecovery $restore }
        $script:LcRecoverySnapshotHook = $null
        Assert-That ($null -ne $refusal -and $refusal -like '*nothing was changed*' -and (Get-Content -LiteralPath $script:LcJournal -Raw) -ceq $global:LcTestBJournal -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $global:LcTestSb.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-snapshot-b') -and
            $global:LcTestRegistry -ceq $global:LcTestBRegistry -and $global:LcTestRestores -eq $global:LcTestBRestores) 'snapshot-a-caller-leaves-b-untouched'
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $global:LcTestBRestores + 1 -and (& $clean)) 'snapshot-b-recovers-on-its-own-invocation'

        # --- R8-F1: a stale journal beside a foreign shared-lock holder.
        $txF = New-FixtureTransaction 'fixture-foreign-shared'
        Stop-FixtureRunner $txF
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        $restoresBefore = $global:LcTestRestores
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*compatibility.lock*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run') 'foreign-shared-lock-preserved-no-restore'
        [IO.File]::WriteAllText($compatLock, $txF.Scope.Lease.RunId)
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'recovery-after-foreign-holder-left'

        # --- R8-F4: a pre-round-7 journal left after (or before) its import is never replayed without the owner.
        $restoresBefore = $global:LcTestRestores
        New-LegacyJournal 'fixture-legacy-applied'
        $global:LcTestRegistry = $backupText
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $backupText) 'legacy-applied-not-reimported'
        $later = New-RegText 'changed-later' '1' '2' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $later
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $later) 'legacy-later-settings-preserved'
        $recovery = Invoke-LcRecovery $restore 'KeepCurrent' (Get-FixtureJson $script:LcJournal).transactionId
        Assert-That ($recovery.Result.Outcome -eq 'completed' -and $recovery.Result.Registry -eq 'owner-kept-current' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $later -and (& $clean)) 'legacy-owner-keeps-current-settings'

        # --- R10-F1: registry comparison keeps keys, value names, types and ordered data together.
        $regA = Join-Path $fixture 'reg-a.reg'; $regB = Join-Path $fixture 'reg-b.reg'
        [IO.File]::WriteAllText($regA, (New-RegText 'owner' '1' '2' "01,02,03,\`r`n  04,05,06"))
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '2' '1' "01,02,03,\`r`n  04,05,06"))
        Assert-That ((Get-LcRegistryCanonical $regA) -cne (Get-LcRegistryCanonical $regB)) 'swapped-subkey-values-unequal'
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '1' '2' "04,05,06,\`r`n  01,02,03"))
        Assert-That ((Get-LcRegistryCanonical $regA) -cne (Get-LcRegistryCanonical $regB)) 'reordered-multiline-data-unequal'
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '1' '2' "01,02,03,\`r`n  04,05,06").Replace("`r`n`r`n", "`r`n"))
        Assert-That ((Get-LcRegistryCanonical $regA) -ceq (Get-LcRegistryCanonical $regB)) 'same-settings-equal'
        # Ordinal: two UTF-16 spellings of the same visible text (U+00E9 vs U+0065 U+0301) are different data.
        $composed = 'caf' + [char]0x00E9; $decomposed = 'cafe' + [char]0x0301
        [IO.File]::WriteAllText($regA, (New-RegText $composed '1' '2' "01,02,03,\`r`n  04,05,06"))
        [IO.File]::WriteAllText($regB, (New-RegText $decomposed '1' '2' "01,02,03,\`r`n  04,05,06"))
        Assert-That (-not (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB))) 'unicode-spellings-unequal-ordinally'
        [IO.File]::WriteAllText($regB, (New-RegText $composed '1' '2' "01,02,03,\`r`n  04,05,06"), [Text.Encoding]::Unicode)
        Assert-That (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB)) 'utf16-export-equals-same-data'
        # In recovery: live data in the other spelling matches neither recorded state, so it is preserved as unresolved.
        $backupE = Join-Path $fixture 'settings-composed.reg'
        $backupEText = New-RegText $composed '1' '2' "01,02,03,\`r`n  04,05,06"
        [IO.File]::WriteAllText($backupE, $backupEText)
        $txUc = New-FixtureTransaction 'fixture-unicode-spelling' -Bare
        [void](Save-LcIntake $txUc.Journal); Set-LcRegistryBackup $txUc.Journal $backupE
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txUc.Journal $txUc.Scope $restore 0 })
        $decomposedText = New-RegText $decomposed '1' '2' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $decomposedText
        Stop-FixtureRunner $txUc
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*unresolved*' -and $global:LcTestRestores -eq $restoresBefore -and
            [string]::Equals($global:LcTestRegistry, $decomposedText, [StringComparison]::Ordinal)) 'unicode-other-spelling-preserved-unresolved'
        [void](Invoke-LcRecovery $restore 'KeepCurrent' $txUc.Journal.transactionId)
        Assert-That ([string]::Equals($global:LcTestRegistry, $decomposedText, [StringComparison]::Ordinal) -and (& $clean)) 'unicode-other-spelling-kept-by-owner'
        $txSw = New-FixtureTransaction 'fixture-swapped'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txSw.Journal $txSw.Scope $restore 0 })
        $swapped = New-RegText 'owner' '2' '1' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $swapped
        Stop-FixtureRunner $txSw
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and $recovery.Message -like '*unresolved*' -and
            $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $swapped) 'swapped-current-state-preserved-and-unresolved'
        Assert-That ((Test-Path -LiteralPath $script:LcJournal) -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txSw.Scope.Lease.RunId -and
            (Get-FixtureJson $txSw.Scope.Lease.StatePath).status -ceq 'RecoveryRequired') 'unresolved-retains-journal-lock-and-lease'
        $recovery = Invoke-LcRecovery $restore 'Import' $txSw.Journal.transactionId
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 2 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'owner-import-resolution-restores'

        # --- R10-F2: an unreadable registry keeps ownership; a later recovery settles the same transaction.
        $txUr = New-FixtureTransaction 'fixture-unreadable-registry'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txUr.Journal $txUr.Scope $restore 0 })
        Stop-FixtureRunner $txUr
        $global:LcTestExportFailures = 1
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*could not be read*unreadable*' -and (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'restoring' -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txUr.Scope.Lease.RunId -and (Test-Path -LiteralPath $script:LcLock)) 'unreadable-registry-retains-ownership'
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $recovery.Result.Registry -eq 'applied' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'unreadable-registry-later-settled-without-reimport'
        $txAb = New-FixtureTransaction 'fixture-key-absent'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txAb.Journal $txAb.Scope $restore 0 })
        $global:LcTestRegistry = $null
        Stop-FixtureRunner $txAb
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'missing-key-imports-once'
        # Interrupted before the import with the snapshot state still live: imported exactly once.
        $txBi = New-FixtureTransaction 'fixture-before-import'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txBi.Journal $txBi.Scope $restore 0 })
        Stop-FixtureRunner $txBi
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'snapshot-state-imports-once'
        # Interrupted after the import applied: reconciled, no second import.
        $txIa = New-FixtureTransaction 'fixture-import-applied'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txIa.Journal $txIa.Scope $restore 0 })
        Stop-FixtureRunner $txIa
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'applied-import-reconciled-no-second-import'

        # --- R10-F3: a peer appearing during the registry export withholds the import, in completion and recovery.
        $txPe = New-FixtureTransaction 'fixture-peer-during-snapshot'
        $restoresBefore = $global:LcTestRestores
        $global:LcTestExportHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6262 }) }
        $outcome = Complete-LcSessionOwnership $txPe.Journal $txPe.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*import was withheld*pid 6262*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'restoring' -and (Get-FixtureJson $script:LcJournal).preRestore.state -ceq 'exported' -and
            (Test-Path -LiteralPath $compatLock) -and (Test-Path -LiteralPath $script:LcLock)) 'peer-during-snapshot-withholds-import-keeps-snapshot'
        $global:LcTestGames = @(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*import was withheld*' -and $global:LcTestRestores -eq $restoresBefore -and (Test-Path -LiteralPath $script:LcJournal)) 'peer-during-recovery-snapshot-withholds-import'
        $global:LcTestGames = @(); $global:LcTestExportHook = $null
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'import-after-peer-left'
        $txPr = New-FixtureTransaction 'fixture-peer-during-reconcile'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txPr.Journal $txPr.Scope $restore 0 })
        Stop-FixtureRunner $txPr
        $global:LcTestExportHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6363 }) }
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*import was withheld*pid 6363*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'restoring' -and (Test-Path -LiteralPath $compatLock)) 'peer-during-reconcile-withholds-import'
        $global:LcTestGames = @(); $global:LcTestExportHook = $null
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'reconcile-import-after-peer-left'
        # R11-F2: the peer changes the settings before it leaves - recovery reconciles against the retained
        # snapshot, preserves the peer's settings and keeps ownership until the owner decides.
        $txPc = New-FixtureTransaction 'fixture-peer-changes-settings'
        $restoresBefore = $global:LcTestRestores
        $global:LcTestExportHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6464 }) }
        $outcome = Complete-LcSessionOwnership $txPc.Journal $txPc.Scope $restore 0
        $global:LcTestExportHook = $null
        $peerText = New-RegText 'peer-changed' '7' '8' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $peerText; $global:LcTestGames = @(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and -not $recovery.Succeeded -and $recovery.Message -like '*unresolved*' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $peerText -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txPc.Scope.Lease.RunId -and
            (Get-FixtureJson $txPc.Scope.Lease.StatePath).status -ceq 'RecoveryRequired') 'peer-change-during-deferral-preserved-with-ownership'
        [void](Invoke-LcRecovery $restore 'KeepCurrent' $txPc.Journal.transactionId)
        Assert-That ($global:LcTestRegistry -ceq $peerText -and $global:LcTestRestores -eq $restoresBefore -and (& $clean)) 'peer-change-kept-by-owner'
        # R10-F3: a peer appearing during the restore-source verification (the last slow read) withholds the import.
        $txVh = New-FixtureTransaction 'fixture-peer-during-backup-read'
        $restoresBefore = $global:LcTestRestores
        $script:LcBackupVerifyHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6565 }) }
        $outcome = Complete-LcSessionOwnership $txVh.Journal $txVh.Scope $restore 0
        $script:LcBackupVerifyHook = $null
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*import was withheld*pid 6565*' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $sessionText -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txVh.Scope.Lease.RunId -and (Test-Path -LiteralPath $script:LcLock)) 'peer-during-backup-read-withholds-import'
        $global:LcTestGames = @(); Set-FixtureOwnerGone
        $script:LcBackupVerifyHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6566 }) }
        $recovery = Invoke-LcRecovery $restore
        $script:LcBackupVerifyHook = $null
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*import was withheld*pid 6566*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Test-Path -LiteralPath $compatLock)) 'peer-during-recovery-backup-read-withholds-import'
        $global:LcTestGames = @()
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'import-after-backup-read-peer-left'

        # --- R10-F4: a verified but uncommitted transaction is verified afresh on recovery.
        $txVf = New-FixtureTransaction 'fixture-stale-verified'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'verified' { Complete-LcSessionOwnership $txVf.Journal $txVf.Scope $restore 0 })
        Stop-FixtureRunner $txVf
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|changed-while-down') }
        $recovery = Invoke-LcRecovery $restore
        $global:LcTestSaves = $defaultSaves
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Integrity.status -eq 'changed' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'stale-verified-refreshed-to-changed'

        # --- R10-F5: a backup changed after intake is refused before any registry change, in completion and recovery.
        $txBk = New-FixtureTransaction 'fixture-backup-changed'
        $restoresBefore = $global:LcTestRestores
        [IO.File]::WriteAllText($backup, (New-RegText 'tampered' '9' '9' "09,09,09,\`r`n  09,09,09"))
        $outcome = Complete-LcSessionOwnership $txBk.Journal $txBk.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*no longer matches the digest*' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $sessionText) 'changed-backup-refused-in-completion'
        Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore 'Import' $txBk.Journal.transactionId
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*no longer matches the digest*' -and $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $sessionText) 'changed-backup-refused-in-recovery'
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'verified-backup-imported-once'
        # The backup replaced with the live (unrestored) settings while recovery exports them for reconciliation:
        # the comparison uses the bytes verified at the start, so nothing is certified applied, and only those
        # original bytes are imported.
        $txRp = New-FixtureTransaction 'fixture-backup-replaced-during-export'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txRp.Journal $txRp.Scope $restore 0 })
        Stop-FixtureRunner $txRp
        $global:LcTestExportHook = { [IO.File]::WriteAllText($backup, $global:LcTestRegistry) }
        $recovery = Invoke-LcRecovery $restore
        $global:LcTestExportHook = $null
        $replacedWith = [IO.File]::ReadAllText($backup)
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ($replacedWith -ceq $sessionText -and $recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and
            (& $clean)) 'backup-replaced-during-export-not-certified-original-imported'
        # The backup replaced during the restore-source verification itself: the held bytes are the verified ones.
        $txRv = New-FixtureTransaction 'fixture-backup-replaced-during-verification'
        $restoresBefore = $global:LcTestRestores
        $script:LcBackupVerifyHook = { [IO.File]::WriteAllText($backup, (New-RegText 'swapped-in' '5' '5' "05,05,05,\`r`n  05,05,05")) }
        $outcome = Complete-LcSessionOwnership $txRv.Journal $txRv.Scope $restore 0
        $script:LcBackupVerifyHook = $null
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ((Test-LcTransactionSucceeded $outcome) -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'backup-replaced-during-verification-original-imported'

        # --- R8-F4 / R9-F3: a round-7 journal whose lease is Completed: cleanup only, with unavailable integrity reported.
        $txL = New-FixtureTransaction 'fixture-round7-completed'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'lease-completed' { Complete-LcSessionOwnership $txL.Journal $txL.Scope $restore 0 })
        $txL.Scope.Lease.Stream.Dispose()
        Write-LcJson $script:LcJournal ([ordered]@{ sessionId = 'fixture-round7-completed'; registryBackup = $backup; leaseRunId = $txL.Scope.Lease.RunId; leaseStatePath = $txL.Scope.Lease.StatePath })
        $leaseState = Get-FixtureJson $txL.Scope.Lease.StatePath
        $leaseState.processStartedUtc = '2000-01-01T00:00:00.0000000Z'
        [IO.File]::WriteAllText($txL.Scope.Lease.StatePath, ($leaseState | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*cleanup only*' -and $recovery.Result.Integrity.status -eq 'unavailable' -and
            $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'round7-completed-cleanup-only-integrity-unavailable'

        # --- R11-F1: every accepted spelling of KeepCurrent keeps the live settings on a pending transaction.
        Assert-That ((ConvertTo-LcResolution 'keepcurrent') -ceq 'KeepCurrent' -and (ConvertTo-LcResolution 'IMPORT') -ceq 'Import' -and
            (ConvertTo-LcResolution '') -ceq '' -and (Get-Refusal { ConvertTo-LcResolution 'keep' }) -like 'Unknown registry resolution*') 'resolution-normalized-case-insensitively'
        foreach ($spelling in @('keepcurrent', 'kEEPcURRENT')) {
            $txKc = New-FixtureTransaction ('fixture-keepcurrent-' + $spelling)
            Stop-FixtureRunner $txKc
            $restoresBefore = $global:LcTestRestores
            $recovery = Invoke-LcRecovery $restore $spelling $txKc.Journal.transactionId
            Assert-That ($recovery.Succeeded -and $recovery.Result.Registry -eq 'owner-kept-current' -and $global:LcTestRestores -eq $restoresBefore -and
                $global:LcTestRegistry -ceq $sessionText -and (& $clean)) "keepcurrent-spelling-$spelling-no-import"
        }

        # --- R11-F3: an owner decision is bound to the transaction that asked for it.
        $txRa = New-FixtureTransaction 'fixture-resolve-a'
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txRa.Journal $txRa.Scope $restore 0 })
        $global:LcTestRegistry = New-RegText 'neither-a' '3' '3' "01,02,03,\`r`n  04,05,06"
        Stop-FixtureRunner $txRa
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Message -like "*-ResolveTransaction $($txRa.Journal.transactionId) -RegistryResolution Import*") 'deferral-names-its-transaction'
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore 'Import' }) -like '*must name the interrupted transaction*nothing was changed*' -and
            (Get-Refusal { Invoke-LcRecovery $restore '' $txRa.Journal.transactionId }) -like '*needs -RegistryResolution*') 'unbound-resolution-refused'
        [void](Invoke-LcRecovery $restore 'KeepCurrent' $txRa.Journal.transactionId)
        $txRb = New-FixtureTransaction 'fixture-resolve-b'
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txRb.Journal $txRb.Scope $restore 0 })
        $bText = New-RegText 'neither-b' '4' '4' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $bText
        Stop-FixtureRunner $txRb
        [void](Invoke-LcRecovery $restore)
        $restoresBefore = $global:LcTestRestores
        $bJournal = Get-Content -LiteralPath $script:LcJournal -Raw
        $refusal = Get-Refusal { Invoke-LcRecovery $restore 'Import' $txRa.Journal.transactionId }
        Assert-That ($refusal -like "*names transaction $($txRa.Journal.transactionId)*nothing was changed*" -and $global:LcTestRegistry -ceq $bText -and
            $global:LcTestRestores -eq $restoresBefore -and (Get-Content -LiteralPath $script:LcJournal -Raw) -ceq $bJournal -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txRb.Scope.Lease.RunId) 'stale-resolution-refused-for-other-transaction'
        [void](Invoke-LcRecovery $restore 'KeepCurrent' $txRb.Journal.transactionId)
        Assert-That ($global:LcTestRegistry -ceq $bText -and (& $clean)) 'second-transaction-settled-by-its-own-decision'
        $noJournal = Invoke-LcRecovery $restore 'Import' $txRb.Journal.transactionId
        Assert-That (-not $noJournal.Succeeded -and $noJournal.Message -like '*not awaiting a decision*') 'resolution-without-pending-journal-reports-not-applied'

        # --- R8-F2: a legacy recovery interrupted after journaling its lease identity, before the lock exists.
        New-LegacyJournal 'fixture-legacy-intent'
        $refusal = Invoke-FixtureInterrupted 'recovery-lease-intended' { Invoke-LcRecovery $restore }
        $intent = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like '*recovery-lease-intended*' -and $intent.leaseRunId -and -not $intent.sharedLock -and -not (Test-Path -LiteralPath $compatLock)) 'legacy-lease-intent-journaled-before-lock'
        $recovery = Invoke-LcRecovery $restore
        $leaseNow = Get-FixtureJson $intent.leaseStatePath
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $intent.leaseRunId -and
            $leaseNow.runId -ceq $intent.leaseRunId -and $leaseNow.status -ceq 'RecoveryRequired' -and $leaseNow.recoveryRequired -and
            (Get-FixtureJson $script:LcJournal).sharedLock) 'legacy-intent-retry-acquires-lock-and-keeps-recovery-lease'
        $global:LcTestCompetitor = $null
        # (The fixture restore is reached through a global: inside the registry step, $restore names the caller's parameter.)
        $global:LcTestBaseRestore = $restore
        $competingRestore = { param($Path) try { [void](Enter-LcRuntimeOwnership 'fixture-competitor'); $global:LcTestCompetitor = 'acquired' } catch { $global:LcTestCompetitor = $_.Exception.Message }; & $global:LcTestBaseRestore $Path }
        $restoresBefore = $global:LcTestRestores
        $recovery = Invoke-LcRecovery $competingRestore 'Import' $intent.transactionId
        Assert-That ($global:LcTestCompetitor -like '*lock already exists*' -and $recovery.Result.Registry -eq 'applied' -and $global:LcTestRestores -eq $restoresBefore + 1 -and
            $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'legacy-intent-import-under-held-shared-lock'
        # A legacy recovery interrupted after taking the lock, before its lease record existed: it was published first.
        $refusal = Invoke-FixtureInterrupted 'recovery-lease-written' { New-LegacyJournal 'fixture-legacy-lease-write'; Invoke-LcRecovery $restore }
        $written = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like '*recovery-lease-written*' -and (Get-FixtureJson $written.leaseStatePath).status -ceq 'Active' -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $written.leaseRunId) 'recovery-lease-record-published-before-work'
        Assert-That ($null -eq (Get-Refusal { [void](Invoke-LcRecovery $restore 'KeepCurrent' (Get-LcValue $written 'transactionId')) }) -and (& $clean)) 'recovery-lease-record-retry-settled'

        # --- R10-F2: a schema-3 journal marked lease-completed with an unresolved restore, its locks still held.
        $tx3 = New-FixtureTransaction 'fixture-schema3-unresolved'
        $tx3.Scope.Lease.Stream.Dispose()
        $j3 = [ordered]@{}
        foreach ($key in $tx3.Journal.Keys) { if ($key -cne 'registryBackupSha256') { $j3[$key] = $tx3.Journal[$key] } }
        $j3.schemaVersion = 3; $j3.phase = 'lease-completed'; $j3.registryRestore = 'unresolved: the game settings match neither the session backup nor the snapshot taken just before restoration'
        $j3.ownerProcessStartedUtc = '2000-01-01T00:00:00.0000000Z'; $j3.integrity = [ordered]@{ status = 'verified' }
        Write-LcJson $script:LcJournal $j3
        $state3 = Get-FixtureJson $tx3.Scope.Lease.StatePath
        $state3.status = 'Completed'; $state3 | Add-Member -NotePropertyName registryRestore -NotePropertyValue $j3.registryRestore -Force
        [IO.File]::WriteAllText($tx3.Scope.Lease.StatePath, ($state3 | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
        $later3 = New-RegText 'schema3-later' '6' '6' "01,02,03,\`r`n  04,05,06"
        $global:LcTestRegistry = $later3
        $restoresBefore = $global:LcTestRestores
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and (Get-FixtureJson $script:LcJournal).transactionId -ceq $tx3.Journal.transactionId -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $tx3.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-schema3-unresolved') -and
            (Get-FixtureJson $tx3.Scope.Lease.StatePath).status -ceq 'RecoveryRequired' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $later3) 'schema3-unresolved-completed-journal-retained'
        $recovery = Invoke-LcRecovery $restore 'Import' $tx3.Journal.transactionId
        Assert-That ($recovery.Succeeded -and $recovery.Result.Registry -eq 'applied' -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and
            (& $clean)) 'schema3-unresolved-settled-by-owner-for-same-transaction'

        # --- completion interrupted at each later boundary resumes without a second restore.
        foreach ($boundary in @('restored', 'verified', 'lease-completed', 'shared-released', 'private-released')) {
            $tx = New-FixtureTransaction ('fixture-interrupt-' + $boundary)
            $restoresBefore = $global:LcTestRestores
            $refusal = Invoke-FixtureInterrupted $boundary { Complete-LcSessionOwnership $tx.Journal $tx.Scope $restore 0 }
            Assert-That ($refusal -like "Fixture interruption after $boundary*" -and $global:LcTestRestores -eq $restoresBefore + 1) "interrupted-after-$boundary"
            Stop-FixtureRunner $tx
            if ($boundary -eq 'shared-released') {
                [IO.File]::WriteAllText($peerLock, 'sibling')
                Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*' -and (Test-Path -LiteralPath $script:LcLock) -and
                    (Get-Content -LiteralPath $peerLock -Raw) -ceq 'sibling') 'shared-released-recovery-preserves-foreign-state'
                Remove-Item -LiteralPath $peerLock -Force
            }
            $recovery = Invoke-LcRecovery $restore
            Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) "recovered-without-second-restore-after-$boundary"
        }

        # --- R9-F2: a peer appearing during verification keeps ownership with the restore recorded.
        $txV = New-FixtureTransaction 'fixture-busy-verification'
        $restoresBefore = $global:LcTestRestores
        $global:LcTestSaves = { $global:LcTestGames = @([pscustomobject]@{ Id = 5151 }); @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }
        $outcome = Complete-LcSessionOwnership $txV.Journal $txV.Scope $restore 0
        $global:LcTestSaves = $defaultSaves
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*host became busy during verification*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and
            (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'applied' -and (Test-Path -LiteralPath $compatLock)) 'busy-verification-ownership-retained'
        Set-FixtureOwnerGone
        $global:LcTestGames = @()
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'busy-verification-completes-without-reimport'

        # --- R8-F3: integrity while owning the host.
        $txI = New-FixtureTransaction 'fixture-integrity'
        $global:LcTestCompetitor = $null
        $global:LcTestSaves = { try { [void](Enter-LcRuntimeOwnership 'fixture-competitor'); $global:LcTestCompetitor = 'acquired' } catch { $global:LcTestCompetitor = $_.Exception.Message }; @('saves\a.zks|10|aa', 'saves\b.zks|21|changed') }
        $outcome = Complete-LcSessionOwnership $txI.Journal $txI.Scope $restore 0
        $global:LcTestSaves = $defaultSaves
        Assert-That ($global:LcTestCompetitor -like '*lock already exists*' -and $outcome.Integrity.status -eq 'changed' -and (& $clean)) 'competing-acquisition-refused-changed-recorded'

        # --- R9-F3: the -RecoverOnly dispatch, run in child processes, exits with the recorded verdict after cleanup.
        $probe = Join-Path $fixture 'recover-only-probe.ps1'
        $commonPath = (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1').Replace("'", "''")
        [IO.File]::WriteAllText($probe, @"
param([string]`$Fixture)
. '$commonPath'
`$script:LcStateRoot = Join-Path `$Fixture 'compatibility-state'
`$script:LcJournal = Join-Path `$Fixture 'game-copies\loader-compat.journal.json'
`$script:LcLock = Join-Path `$Fixture 'game-copies\loader-compat.lock'
`$script:LcRecoveryLock = Join-Path `$Fixture 'game-copies\loader-compat.recovery.lock'
`$script:LcForeignLocks = @((Join-Path `$script:LcStateRoot 'compatibility.lock'), (Join-Path `$Fixture 'sibling-lab\runtime.lock'))
`$script:LcGameProcessQuery = { @() }
`$script:LcSavesManifest = { throw 'not used by these cases' }
`$script:LcInstallManifest = { throw 'not used by these cases' }
`$script:LcRegistryExport = { param(`$Path) throw 'not used by these cases' }
exit (Invoke-LcRecoverOnlyCommand { param(`$Path) throw 'not used by these cases' } '')
"@)
        function Invoke-RecoverOnlyChild { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $probe -Fixture $fixture | Out-Null; return $LASTEXITCODE }
        $cases = @(@('verified', $defaultSaves, 0), @('changed', { @('saves\a.zks|10|aa', 'saves\b.zks|22|changed') }, 1), @('unreadable', { throw 'unreadable saves' }, 1))
        foreach ($case in $cases) {
            $tx = New-FixtureTransaction ('fixture-cleanup-' + $case[0])
            $global:LcTestSaves = $case[1]
            [void](Invoke-FixtureInterrupted 'lease-completed' { Complete-LcSessionOwnership $tx.Journal $tx.Scope $restore 0 })
            $global:LcTestSaves = $defaultSaves
            Stop-FixtureRunner $tx
            $code = Invoke-RecoverOnlyChild
            Assert-That ($code -eq $case[2] -and (& $clean)) ("recover-only-cleanup-" + $case[0] + "-exit-" + $case[2])
        }
        $txDf = New-FixtureTransaction 'fixture-cleanup-deferred'
        [IO.File]::WriteAllText($peerLock, 'sibling')
        [void](Complete-LcSessionOwnership $txDf.Journal $txDf.Scope $restore 0)
        Set-FixtureOwnerGone
        $code = Invoke-RecoverOnlyChild
        Assert-That ($code -eq 1 -and (Test-Path -LiteralPath $script:LcJournal) -and (Test-Path -LiteralPath $compatLock)) 'recover-only-deferred-exit-1-retains-state'
        Remove-Item -LiteralPath $peerLock -Force
        [void](Invoke-LcRecovery $restore)
        Assert-That (& $clean) 'all-fixture-transactions-released'
    }
    finally {
        $script:LcInterruptAfter = $null; $script:LcRecoveryPauseHook = $null; $script:LcRecoverySnapshotHook = $null; $script:LcBackupVerifyHook = $null
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcRecoveryLock = $saved.Guard; $script:LcGameProcessQuery = $saved.Query
        $script:LcSavesManifest = $saved.Saves; $script:LcInstallManifest = $saved.Install
        $script:LcRegistryExport = $saved.Export; $script:LcLeaseStateWriter = $saved.Writer
        Remove-Variable -Name LcTestGames, LcTestRestores, LcTestSaves, LcTestInstall, LcTestCompetitor, LcTestRegistry, LcTestFirst, LcTestB, LcTestBRegistry, LcTestBRestores, LcTestExportFailures, LcTestExportHook, LcTestFirstCode, LcTestSb, LcTestBJournal, LcTestBaseRestore -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 129) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
