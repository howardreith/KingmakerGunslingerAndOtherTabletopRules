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
    # settings, and a substituted process query standing in for a peer Kingmaker process.
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
        # Game settings: a string standing for the registry key; export writes it, the restore imports the backup.
        $backupText = "Windows Registry Editor Version 5.00`n[HKEY_CURRENT_USER\Software\Owlcat Games\Pathfinder Kingmaker]`n`"screen`"=`"owner`""
        $sessionText = "Windows Registry Editor Version 5.00`n[HKEY_CURRENT_USER\Software\Owlcat Games\Pathfinder Kingmaker]`n`"screen`"=`"session`""
        $global:LcTestRegistry = $sessionText
        $script:LcRegistryExport = { param($Path) [IO.File]::WriteAllText($Path, $global:LcTestRegistry) }
        $global:LcTestRestores = 0
        $restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = [IO.File]::ReadAllText($Path) }
        $backup = Join-Path $fixture 'settings.reg'
        [IO.File]::WriteAllText($backup, $backupText)
        $clean = { -not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcLock) -and -not (Test-Path -LiteralPath $script:LcJournal) }
        function New-FixtureTransaction([string]$Id, [switch]$Bare) {
            $global:LcTestRegistry = $sessionText
            $tx = Enter-LcSessionTransaction $Id
            if (-not $Bare) { [void](Save-LcIntake $tx.Journal); $tx.Journal.registryBackup = $backup; Save-LcJournal $tx.Journal }
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

        # --- R8-F2: inputs are checked before anything is acquired.
        Assert-That ((Get-Refusal { Assert-LcSessionInputs (Join-Path $fixture 'missing.zip') $fixture }) -like 'Candidate ZIP not found*Nothing was acquired.') 'missing-candidate-refused'
        $zip = Join-Path $fixture 'candidate.zip'; [IO.File]::WriteAllText($zip, 'zip')
        Assert-That ((Get-Refusal { Assert-LcSessionInputs $zip (Join-Path $fixture 'no-harness') }) -like 'Harness file missing*') 'missing-harness-refused'
        Assert-That (& $clean) 'refused-inputs-acquired-nothing'

        # A foreign holder of the shared lock is refused and never overwritten; nothing is left behind.
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-refused' }) -like '*lock already exists*') 'foreign-shared-lock-refused'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run' -and -not (Test-Path -LiteralPath $script:LcJournal)) 'foreign-shared-lock-kept-no-journal'
        Remove-Item -LiteralPath $compatLock -Force

        # A stale private lock of another session: acquisition rolls back and that lock is preserved.
        [IO.File]::WriteAllText($script:LcLock, "session=older-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n")
        Assert-That ($null -ne (Get-Refusal { Enter-LcSessionTransaction 'fixture-private' })) 'stale-private-lock-refused'
        Assert-That (-not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcJournal) -and
            ((Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=older-session*')) 'stale-private-lock-rollback-releases-only-own'
        Remove-Item -LiteralPath $script:LcLock -Force

        # --- R9-F1: two starts interleaved - the loser preserves the winner's journal and locks.
        $txWin = New-FixtureTransaction 'fixture-winner' -Bare
        $winnerId = $txWin.Journal.transactionId
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-loser' }) -like 'Another session holds the loader-compat journal*nothing was acquired.') 'losing-start-refused'
        Assert-That ((Get-LcJournalTransactionId) -ceq $winnerId -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txWin.Scope.Lease.RunId -and
            (Test-LcPrivateLockOwned 'fixture-winner')) 'winner-journal-and-locks-preserved'
        $outcome = Complete-LcSessionOwnership $txWin.Journal $txWin.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'completed' -and (& $clean)) 'winner-completes'

        # --- R9-F1: an interrupted journal update leaves the last complete record, from which recovery finishes.
        $txJ = New-FixtureTransaction 'fixture-journal-write'
        $restoresBefore = $global:LcTestRestores
        $refusal = Invoke-FixtureInterrupted 'journal-temp-written' { Complete-LcSessionOwnership $txJ.Journal $txJ.Scope $restore 0 }
        $lastRecord = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like 'Fixture interruption after journal-temp-written*' -and $lastRecord.transactionId -ceq $txJ.Journal.transactionId -and
            $lastRecord.registryRestore -ceq 'pending') 'interrupted-update-keeps-last-complete-journal'
        $txJ.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'recovered-from-last-complete-journal'
        Assert-That (@(Get-ChildItem -LiteralPath $copies -Filter 'loader-compat.journal.json.*.tmp').Count -eq 0) 'interrupted-update-temp-removed'

        # --- R8-F2: a lease-state write failure after the shared lock exists rolls back what was acquired.
        $script:LcLeaseStateWriter = { param($Lease) throw 'lease state disk full' }
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-lease-write' }) -like '*lease state disk full*') 'lease-write-failure-refused'
        $script:LcLeaseStateWriter = $saved.Writer
        Assert-That (& $clean) 'lease-write-failure-rolled-back'
        # Interruption right after the shared lock is created (no lease state yet): the bound identity lets recovery release it.
        $refusal = Invoke-FixtureInterrupted 'shared-lock-created' { Enter-LcSessionTransaction 'fixture-shared-crash' }
        $crashJournal = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like 'Fixture interruption after shared-lock-created*' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $crashJournal.leaseRunId -and
            -not (Test-Path -LiteralPath $crashJournal.leaseStatePath)) 'shared-lock-crash-identified-by-journal'
        Set-FixtureOwnerGone
        $restoresBefore = $global:LcTestRestores
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Result.Outcome -eq 'completed' -and $global:LcTestRestores -eq $restoresBefore -and (& $clean)) 'shared-lock-crash-recovered-without-import'
        # Interruption right after the private lock is created but before it is journaled.
        $refusal = Invoke-FixtureInterrupted 'private-lock-created' { Enter-LcSessionTransaction 'fixture-private-crash' }
        $crashJournal = Get-FixtureJson $script:LcJournal
        Assert-That ($refusal -like 'Fixture interruption after private-lock-created*' -and $crashJournal.privateLockIntended -and -not $crashJournal.privateLock -and
            (Test-LcPrivateLockOwned 'fixture-private-crash')) 'private-lock-crash-recorded-as-intended'
        Set-FixtureOwnerGone
        $refusal = Get-Refusal { [void](Invoke-LcRecovery $restore) }
        Assert-That ($null -eq $refusal -and (& $clean)) 'private-lock-crash-reconciled-and-released'
        $txNext = New-FixtureTransaction 'fixture-after-crashes'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txNext.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-after-crashes')) 'next-session-acquires-after-crashes'
        $outcome = Complete-LcSessionOwnership $txNext.Journal $txNext.Scope $restore 0
        Assert-That ((Test-LcTransactionSucceeded $outcome) -and (& $clean)) 'next-session-completes-cleanly'

        # A setup failure after acquisition (before intake or backup) completes and releases.
        $txSetup = New-FixtureTransaction 'fixture-setup-failure' -Bare
        $outcome = Complete-LcSessionOwnership $txSetup.Journal $txSetup.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'completed' -and $outcome.Registry -eq 'not-needed' -and $outcome.Integrity.status -eq 'unresolved' -and (& $clean)) 'setup-failure-completed-integrity-unresolved'

        # --- R7-F1 / R8-F1: a sibling-lab lock (no game process) after launch one.
        $txA = New-FixtureTransaction 'fixture-sibling'
        $leaseA = $txA.Scope.Lease
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $leaseA })) 'launch-one-admitted-with-own-lock'
        [IO.File]::WriteAllText($peerLock, 'sibling')
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $leaseA }) -like 'Launch withheld: another lab holds a runtime lock*runtime.lock*') 'launch-two-withheld-for-sibling-lock'
        $restoresBefore = $global:LcTestRestores
        $outcome = Complete-LcSessionOwnership $txA.Journal $txA.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $leaseA.RunId -and (Test-Path -LiteralPath $script:LcLock)) 'sibling-lock-defers-with-ownership'
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The session owner*still alive*nothing was changed.') 'recovery-refused-live-owner'
        Set-FixtureOwnerGone
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: another lab holds a runtime lock*Nothing was changed.' -and
            $global:LcTestRestores -eq $restoresBefore) 'recovery-refused-sibling-lock'
        Remove-Item -LiteralPath $peerLock -Force
        $competing = [IO.File]::Open($compatLock, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like "Another caller holds this session's recovery*" -and $global:LcTestRestores -eq $restoresBefore) 'competing-handle-recovery-refused' }
        finally { $competing.Dispose() }
        $guardHolder = New-Object IO.FileStream($script:LcRecoveryLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None, 4096, [IO.FileOptions]::DeleteOnClose)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Another recovery of this session is in progress*' -and $global:LcTestRestores -eq $restoresBefore) 'concurrent-recovery-refused' }
        finally { $guardHolder.Dispose() }
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean) -and (Get-FixtureJson $leaseA.StatePath).status -ceq 'Completed') 'recovery-after-sibling-release'

        # --- R8-F1: a second recovery paused after its first read while the first finishes: one restore in total.
        $txP = New-FixtureTransaction 'fixture-paused'
        $txP.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $restoresBefore = $global:LcTestRestores
        $global:LcTestFirst = $null
        $script:LcRecoveryPauseHook = { $global:LcTestFirst = Invoke-LcRecovery $restore }
        $second = Invoke-LcRecovery $restore
        Assert-That ($global:LcTestFirst.Succeeded -and $second.Message -like '*already recovered by another caller*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'paused-recovery-one-restore-total'
        # ... and when a newer session starts meanwhile, the resumed caller refuses and preserves it.
        $txP2 = New-FixtureTransaction 'fixture-paused-2'
        $txP2.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $restoresBefore = $global:LcTestRestores
        $global:LcTestNewer = $null
        $script:LcRecoveryPauseHook = { [void](Invoke-LcRecovery $restore); $global:LcTestNewer = New-FixtureTransaction 'fixture-newer' }
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The session owner*still alive*') 'paused-recovery-refuses-newer-session'
        Assert-That ($global:LcTestRestores -eq $restoresBefore + 1 -and (Get-LcJournalTransactionId) -ceq $global:LcTestNewer.Journal.transactionId -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $global:LcTestNewer.Scope.Lease.RunId) 'newer-session-preserved'
        $outcome = Complete-LcSessionOwnership $global:LcTestNewer.Journal $global:LcTestNewer.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'completed' -and (& $clean)) 'newer-session-completes'

        # --- R8-F1: a stale journal beside a foreign shared-lock holder.
        $txF = New-FixtureTransaction 'fixture-foreign-shared'
        $txF.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        $restoresBefore = $global:LcTestRestores
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: another lab holds a runtime lock*compatibility.lock*' -and
            $global:LcTestRestores -eq $restoresBefore -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run') 'foreign-shared-lock-preserved-no-restore'
        [IO.File]::WriteAllText($compatLock, $txF.Scope.Lease.RunId)
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'recovery-after-foreign-holder-left'

        # --- R8-F1: a pre-round-7 (unleased) journal: same owner check, competing callers refused, own shared lock for recovery.
        $global:LcTestRegistry = $sessionText
        Write-LcJson $script:LcJournal ([ordered]@{ sessionId = 'fixture-unleased'; registryBackup = $backup })
        [IO.File]::WriteAllText($script:LcLock, "session=fixture-unleased`npid=$PID`nstartedUtc=$([DateTime]::UtcNow.ToString('o'))`n")
        $restoresBefore = $global:LcTestRestores
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The session owner*still alive*' -and $global:LcTestRestores -eq $restoresBefore) 'unleased-refused-live-owner'
        [IO.File]::WriteAllText($script:LcLock, "session=fixture-unleased`npid=$PID`nstartedUtc=2000-01-01T00:00:00.0000000Z`n")
        [IO.File]::WriteAllText($compatLock, 'competing-runtime')
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*compatibility.lock*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'competing-runtime') 'unleased-refused-competing-shared-holder'
        Remove-Item -LiteralPath $compatLock -Force
        $guardHolder = New-Object IO.FileStream($script:LcRecoveryLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None, 4096, [IO.FileOptions]::DeleteOnClose)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Another recovery*in progress*' -and $global:LcTestRestores -eq $restoresBefore) 'unleased-refused-competing-recovery' }
        finally { $guardHolder.Dispose() }
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Result.Outcome -eq 'completed' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'unleased-recovered-under-own-shared-lock'

        # --- R8-F4: restoration interrupted after the import applied but before it was recorded: no second import.
        $txR = New-FixtureTransaction 'fixture-import-applied'
        $restoresBefore = $global:LcTestRestores
        $refusal = Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txR.Journal $txR.Scope $restore 0 }
        Assert-That ($refusal -like '*import-applied*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'restoring') 'import-applied-interrupted-unrecorded'
        $txR.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $recovery.Result.Registry -eq 'applied' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'import-applied-reconciled-no-second-import'
        # ... interrupted after the snapshot and before the import: recovery imports exactly once.
        $txS = New-FixtureTransaction 'fixture-before-import'
        $restoresBefore = $global:LcTestRestores
        $refusal = Invoke-FixtureInterrupted 'restoring' { Complete-LcSessionOwnership $txS.Journal $txS.Scope $restore 0 }
        $txS.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($refusal -like '*restoring*' -and $recovery.Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'before-import-reconciled-imports-once'
        # ... settings changed afterwards by someone else: an explicit unresolved result, no import, failing outcome.
        $txU = New-FixtureTransaction 'fixture-restore-unresolved'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'import-applied' { Complete-LcSessionOwnership $txU.Journal $txU.Scope $restore 0 })
        $global:LcTestRegistry = $backupText + "`n`"later`"=`"newer-settings`""
        $txU.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
        $recovery = Invoke-LcRecovery $restore
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Registry -like 'unresolved*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and
            $global:LcTestRegistry -like '*newer-settings*') 'unrecognized-settings-unresolved-not-overwritten'
        Assert-That ((& $clean) -and (Get-FixtureJson $txU.Scope.Lease.StatePath).registryRestore -like 'unresolved*') 'unresolved-recorded-in-lease'
        # ... a round-7 journal whose lease is already Completed: cleanup only.
        $txL = New-FixtureTransaction 'fixture-round7-completed'
        $restoresBefore = $global:LcTestRestores
        [void](Invoke-FixtureInterrupted 'lease-completed' { Complete-LcSessionOwnership $txL.Journal $txL.Scope $restore 0 })
        $txL.Scope.Lease.Stream.Dispose()
        Write-LcJson $script:LcJournal ([ordered]@{ sessionId = 'fixture-round7-completed'; registryBackup = $backup; leaseRunId = $txL.Scope.Lease.RunId; leaseStatePath = $txL.Scope.Lease.StatePath })
        $leaseState = Get-FixtureJson $txL.Scope.Lease.StatePath
        $leaseState.processStartedUtc = '2000-01-01T00:00:00.0000000Z'
        [IO.File]::WriteAllText($txL.Scope.Lease.StatePath, ($leaseState | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
        $global:LcTestRegistry = $sessionText
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $recovery.Message -like '*cleanup only*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'round7-completed-lease-cleanup-only'

        # --- R8-F4: completion interrupted at each later boundary resumes without a second restore.
        foreach ($boundary in @('restored', 'verified', 'lease-completed', 'shared-released', 'private-released')) {
            $tx = New-FixtureTransaction ('fixture-interrupt-' + $boundary)
            $restoresBefore = $global:LcTestRestores
            $refusal = Invoke-FixtureInterrupted $boundary { Complete-LcSessionOwnership $tx.Journal $tx.Scope $restore 0 }
            Assert-That ($refusal -like "Fixture interruption after $boundary*" -and $global:LcTestRestores -eq $restoresBefore + 1) "interrupted-after-$boundary"
            $tx.Scope.Lease.Stream.Dispose(); Set-FixtureOwnerGone
            if ($boundary -eq 'shared-released') {
                [IO.File]::WriteAllText($peerLock, 'sibling')
                Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*' -and (Test-Path -LiteralPath $script:LcLock) -and
                    (Test-Path -LiteralPath $script:LcJournal) -and (Get-Content -LiteralPath $peerLock -Raw) -ceq 'sibling') 'shared-released-recovery-preserves-foreign-state'
                Remove-Item -LiteralPath $peerLock -Force
            }
            $recovery = Invoke-LcRecovery $restore
            Assert-That ($recovery.Result.Outcome -eq 'completed' -and $global:LcTestRestores -eq $restoresBefore + 1) "no-duplicate-restore-after-$boundary"
            Assert-That ((& $clean) -and (Get-FixtureJson $tx.Scope.Lease.StatePath).status -ceq 'Completed') "markers-cleared-after-$boundary"
        }

        # --- R9-F2: a peer appears during verification: ownership retained, then completion without another import.
        $txV = New-FixtureTransaction 'fixture-busy-verification'
        $restoresBefore = $global:LcTestRestores
        $global:LcTestSaves = { $global:LcTestGames = @([pscustomobject]@{ Id = 5151 }); @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }
        $outcome = Complete-LcSessionOwnership $txV.Journal $txV.Scope $restore 0
        $global:LcTestSaves = $defaultSaves
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*host became busy during verification*Kingmaker is running (pid 5151)*') 'busy-verification-defers'
        Assert-That ($global:LcTestRestores -eq $restoresBefore + 1 -and (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'applied' -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txV.Scope.Lease.RunId -and (Test-Path -LiteralPath $script:LcLock) -and
            (Get-FixtureJson $txV.Scope.Lease.StatePath).status -ceq 'RecoveryRequired') 'busy-verification-ownership-retained-restore-recorded'
        Set-FixtureOwnerGone
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: Kingmaker is running (pid 5151)*') 'busy-verification-recovery-waits-for-peer'
        $global:LcTestGames = @()
        $recovery = Invoke-LcRecovery $restore
        Assert-That ($recovery.Succeeded -and $recovery.Result.Integrity.status -eq 'verified' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'busy-verification-completes-without-reimport'

        # --- R8-F3: integrity while owning the host; a competing acquisition is refused; changed/unreadable are explicit.
        $txI = New-FixtureTransaction 'fixture-integrity'
        $global:LcTestCompetitor = $null
        $global:LcTestSaves = {
            try { [void](Enter-LcRuntimeOwnership 'fixture-competitor'); $global:LcTestCompetitor = 'acquired' } catch { $global:LcTestCompetitor = $_.Exception.Message }
            @('saves\a.zks|10|aa', 'saves\b.zks|21|changed')
        }
        $outcomeChanged = Complete-LcSessionOwnership $txI.Journal $txI.Scope $restore 0
        $global:LcTestSaves = $defaultSaves
        Assert-That ($global:LcTestCompetitor -like '*lock already exists*') 'competing-acquisition-refused-during-verification'
        Assert-That ($outcomeChanged.Integrity.status -eq 'changed' -and (Get-FixtureJson $txI.Scope.Lease.StatePath).integrity.status -ceq 'changed' -and (& $clean)) 'changed-integrity-recorded'
        $txUn = New-FixtureTransaction 'fixture-unreadable'
        $global:LcTestSaves = { throw 'owner saves folder unreadable' }
        $outcomeUnreadable = Complete-LcSessionOwnership $txUn.Journal $txUn.Scope $restore 0
        $global:LcTestSaves = $defaultSaves
        Assert-That ($outcomeUnreadable.Integrity.status -eq 'unresolved' -and (Get-FixtureJson $txUn.Scope.Lease.StatePath).integrity.status -ceq 'unresolved' -and (& $clean)) 'unreadable-integrity-unresolved'

        # --- R9-F3: only a clean completion succeeds, and the process exit code follows that result.
        $txC = New-FixtureTransaction 'fixture-clean'
        $outcomeClean = Complete-LcSessionOwnership $txC.Journal $txC.Scope $restore 0
        $txD = New-FixtureTransaction 'fixture-deferred'
        [IO.File]::WriteAllText($peerLock, 'sibling')
        $outcomeDeferred = Complete-LcSessionOwnership $txD.Journal $txD.Scope $restore 0
        Remove-Item -LiteralPath $peerLock -Force
        Set-FixtureOwnerGone; [void](Invoke-LcRecovery $restore)
        $probe = Join-Path $fixture 'exit-probe.ps1'
        [IO.File]::WriteAllText($probe, "param([string]`$ResultPath)`n. '$((Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1').Replace("'", "''"))'`nexit (Get-LcTransactionExitCode (Get-Content -LiteralPath `$ResultPath -Raw | ConvertFrom-Json))`n")
        foreach ($case in @(@('clean', $outcomeClean, 0), @('deferred', $outcomeDeferred, 1), @('changed', $outcomeChanged, 1), @('unreadable', $outcomeUnreadable, 1))) {
            $resultPath = Join-Path $fixture ('result-' + $case[0] + '.json')
            Write-LcJson $resultPath $case[1]
            & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $probe -ResultPath $resultPath | Out-Null
            Assert-That ($LASTEXITCODE -eq $case[2] -and (Test-LcTransactionSucceeded $case[1]) -eq ($case[2] -eq 0)) ("exit-code-" + $case[0] + "-" + $case[2])
        }
        Assert-That (& $clean) 'all-fixture-transactions-released'
    }
    finally {
        $script:LcInterruptAfter = $null; $script:LcRecoveryPauseHook = $null
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcRecoveryLock = $saved.Guard; $script:LcGameProcessQuery = $saved.Query
        $script:LcSavesManifest = $saved.Saves; $script:LcInstallManifest = $saved.Install
        $script:LcRegistryExport = $saved.Export; $script:LcLeaseStateWriter = $saved.Writer
        Remove-Variable -Name LcTestGames, LcTestRestores, LcTestSaves, LcTestInstall, LcTestCompetitor, LcTestRegistry, LcTestFirst, LcTestNewer -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 87) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
