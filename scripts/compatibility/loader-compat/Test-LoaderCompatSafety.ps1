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
        Assert-That (-not $recovery.Succeeded -and $recovery.Result.Outcome -eq 'recovery-required' -and $recovery.Message -like '*older journal records no digest*' -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $converted.leaseRunId) 'legacy-lock-recognized-as-own-and-retained'
        $recovery = Invoke-LcRecovery $restore 'KeepCurrent'
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
        Assert-That ($global:LcTestFirst.Succeeded -and $second.Message -like '*already recovered by another caller*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'paused-recovery-one-restore-total'

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
        $recovery = Invoke-LcRecovery $restore 'KeepCurrent'
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
        $recovery = Invoke-LcRecovery $restore 'Import'
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
            (Get-FixtureJson $script:LcJournal).registryRestore -ceq 'pending' -and (Test-Path -LiteralPath $compatLock) -and (Test-Path -LiteralPath $script:LcLock)) 'peer-during-snapshot-withholds-import'
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
        $recovery = Invoke-LcRecovery $restore 'Import'
        Assert-That (-not $recovery.Succeeded -and $recovery.Message -like '*no longer matches the digest*' -and $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $sessionText) 'changed-backup-refused-in-recovery'
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ((Invoke-LcRecovery $restore).Succeeded -and $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'verified-backup-imported-once'

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
        $script:LcInterruptAfter = $null; $script:LcRecoveryPauseHook = $null
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcRecoveryLock = $saved.Guard; $script:LcGameProcessQuery = $saved.Query
        $script:LcSavesManifest = $saved.Saves; $script:LcInstallManifest = $saved.Install
        $script:LcRegistryExport = $saved.Export; $script:LcLeaseStateWriter = $saved.Writer
        Remove-Variable -Name LcTestGames, LcTestRestores, LcTestSaves, LcTestInstall, LcTestCompetitor, LcTestRegistry, LcTestFirst, LcTestB, LcTestBRegistry, LcTestBRestores, LcTestExportFailures, LcTestExportHook -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 99) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
