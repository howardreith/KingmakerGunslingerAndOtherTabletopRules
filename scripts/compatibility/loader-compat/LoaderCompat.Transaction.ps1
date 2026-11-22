Set-StrictMode -Version Latest
# ---------------------------------------------------------------------------
# Loader-compat session transaction (journal schema 3), dot-sourced by
# LoaderCompat.Common.ps1.
#
# One journal ($script:LcJournal) is the durable record of a session. It is
# claimed atomically (CreateNew) and every update replaces it atomically, only
# while it still names this transaction. Each step is journaled before it can
# leave state behind: the lease identity is chosen and recorded before the shared
# lock exists, the private lock is recorded as intended before it is created, a
# registry snapshot is taken before the backup is imported. Completion and
# -RecoverOnly share Complete-LcTransaction, which acts only at a safe boundary,
# re-checks the host before it commits and releases, never imports twice, and
# removes the journal only after every owned lock is accounted for.
# ---------------------------------------------------------------------------

# Fixture-only hooks. In every live run these hold the real operations or $null.
$script:LcInterruptAfter = $null          # boundary at which to model process death
$script:LcRecoveryPauseHook = $null       # runs once after recovery's first journal read
$script:LcSavesManifest = { Get-LcOwnerSavesManifest }
$script:LcInstallManifest = { Get-LcOwnerInstallManifest }
$script:LcRegistryExport = { param($Path) Backup-LcRegistry $Path }
$script:LcLeaseStateWriter = { param($Lease) Write-KmgRuntimeLeaseState $Lease }

$script:LcInterruptionPrefix = 'Fixture interruption after '
function Step-LcBoundary([string]$Name) { if ($script:LcInterruptAfter -ceq $Name) { throw ($script:LcInterruptionPrefix + $Name + '.') } }
# An injected interruption models process death, so no in-process rollback runs for it.
function Test-LcInterruption($ErrorRecord) { return ([string]$ErrorRecord.Exception.Message).StartsWith($script:LcInterruptionPrefix, [StringComparison]::Ordinal) }

function ConvertTo-LcOrdered($Object) {
    $result = [ordered]@{}
    foreach ($property in $Object.PSObject.Properties) { $result[$property.Name] = $property.Value }
    return $result
}

function Set-LcStateValue($State, [string]$Name, $Value) {
    if ($State -is [Collections.IDictionary]) { $State[$Name] = $Value }
    else { $State | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

function Get-LcSharedLockPath { return Join-Path $script:LcStateRoot 'compatibility.lock' }
function New-LcLeaseRunId { return 'runtime-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N') }
function Test-LcPrivateLockOwned([string]$SessionId) { $info = Get-LcPrivateLockInfo; return $null -ne $info -and $info['session'] -ceq $SessionId }
function Get-LcTransactionDir($Journal) {
    if ($Journal.leaseStatePath) { return Split-Path -Parent $Journal.leaseStatePath }
    return Split-Path -Parent $script:LcJournal
}

# Inputs checked before anything is acquired: the candidate package must exist and
# be readable (its digest is returned) and the harness must be complete.
function Assert-LcSessionInputs([string]$CandidateZip, [string]$HarnessDir) {
    if (-not $CandidateZip -or -not (Test-Path -LiteralPath $CandidateZip -PathType Leaf)) { throw "Candidate ZIP not found: $CandidateZip. Nothing was acquired." }
    $sha = Get-LcSha256 $CandidateZip
    foreach ($name in @('KmgLoaderCompatHarness.dll', 'Info.json')) {
        if (-not (Test-Path -LiteralPath (Join-Path $HarnessDir $name) -PathType Leaf)) { throw "Harness file missing: $(Join-Path $HarnessDir $name). Nothing was acquired." }
    }
    return $sha
}

# Everything that stops this session from launching, restoring or releasing now:
# any Kingmaker process, any sibling-lab lock, or a shared lock held by another run.
function Get-LcHostConflicts([string]$LeaseRunId) {
    $conflicts = [Collections.Generic.List[string]]::new()
    $games = Get-LcGameProcesses
    if ($games.Count -gt 0) { $conflicts.Add("Kingmaker is running (pid $((@($games | ForEach-Object { $_.Id })) -join ','))") }
    $shared = [IO.Path]::GetFullPath((Get-LcSharedLockPath))
    foreach ($lock in $script:LcForeignLocks) {
        if (-not (Test-Path -LiteralPath $lock)) { continue }
        $own = $LeaseRunId -and [IO.Path]::GetFullPath($lock).Equals($shared, [StringComparison]::OrdinalIgnoreCase) -and
            (Get-Content -LiteralPath $lock -Raw).Trim() -ceq $LeaseRunId
        if (-not $own) { $conflicts.Add("another lab holds a runtime lock: $lock") }
    }
    return , $conflicts.ToArray()
}

function Wait-LcSafeBoundary([string]$LeaseRunId, [int]$WaitSeconds) {
    $deadline = (Get-Date).AddSeconds($WaitSeconds)
    while ((Get-LcHostConflicts $LeaseRunId).Count -gt 0 -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 1 }
    return , (Get-LcHostConflicts $LeaseRunId)
}

# Immediately before every fresh launch.
function Assert-LcLaunchAvailable($Lease) {
    $conflicts = Get-LcHostConflicts $(if ($null -ne $Lease) { $Lease.RunId } else { $null })
    if ($conflicts.Count -gt 0) { throw "Launch withheld: $($conflicts -join '; ')." }
    if ($null -ne $Lease) { Assert-KmgRuntimeLease $Lease }
}

# --- journal -----------------------------------------------------------------

function ConvertTo-LcJournalBytes($Journal) { return (New-Object Text.UTF8Encoding($false)).GetBytes(($Journal | ConvertTo-Json -Depth 12)) }

# Claims the journal atomically: CreateNew succeeds for exactly one caller.
function New-LcJournal($Journal) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $script:LcJournal) | Out-Null
    $bytes = ConvertTo-LcJournalBytes $Journal
    try { $stream = [IO.File]::Open($script:LcJournal, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None) }
    catch [IO.IOException] { throw "Another session holds the loader-compat journal ($script:LcJournal); nothing was acquired." }
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
}

function Read-LcJournalFile { return Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json }

function Get-LcJournalTransactionId {
    try { $raw = Read-LcJournalFile } catch { return '<unreadable>' }
    if ($null -ne $raw -and $raw.PSObject.Properties['transactionId']) { return [string]$raw.transactionId }
    return '<legacy>'
}

function Test-LcJournalOwned($Journal) {
    $current = Get-LcJournalTransactionId
    return $current -ceq [string]$Journal.transactionId -or ($current -ceq '<legacy>' -and $Journal.Contains('convertedFrom'))
}

# Every update writes a complete temporary journal and then replaces the file
# atomically, and only while the file on disk is still this transaction's.
function Save-LcJournal($Journal) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { throw "The session journal disappeared: $script:LcJournal" }
    if (-not (Test-LcJournalOwned $Journal)) { throw "The session journal belongs to another transaction; it was not replaced." }
    $temp = $script:LcJournal + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
    $bytes = ConvertTo-LcJournalBytes $Journal
    $stream = [IO.File]::Open($temp, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
    Step-LcBoundary 'journal-temp-written'
    [IO.File]::Replace($temp, $script:LcJournal, [Management.Automation.Language.NullString]::Value)
}

function Remove-LcJournal($Journal) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { return }
    if (-not (Test-LcJournalOwned $Journal)) { throw "The session journal belongs to another transaction; it was not removed." }
    Remove-Item -LiteralPath $script:LcJournal -Force
}

# Leftover temporary journals of an interrupted update (the journal itself is the last complete record).
function Remove-LcJournalTemps {
    $directory = Split-Path -Parent $script:LcJournal
    if (-not (Test-Path -LiteralPath $directory)) { return }
    foreach ($file in @(Get-ChildItem -LiteralPath $directory -Filter ((Split-Path -Leaf $script:LcJournal) + '.*.tmp') -File)) {
        Remove-Item -LiteralPath $file.FullName -Force
    }
}

# --- acquisition ---------------------------------------------------------------

# Starts a session transaction. The journal is claimed first and already names the
# lease identity the shared lock will carry; the private lock is recorded as
# intended before it is created. A failure rolls back exactly what this call
# acquired; if that cannot finish, the journal is kept and -RecoverOnly can
# identify every lock by its recorded identity.
function Enter-LcSessionTransaction([string]$SessionId) {
    $runId = New-LcLeaseRunId
    $leaseDir = Join-Path $script:LcStateRoot $runId
    $journal = [ordered]@{ schemaVersion = 3; transactionId = [Guid]::NewGuid().ToString('N'); sessionId = $SessionId; phase = 'acquiring'
        ownerProcessId = $PID; ownerProcessStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
        leaseRunId = $runId; leaseStatePath = (Join-Path $leaseDir 'runtime-lease.json'); sharedLock = $false
        privateLockIntended = $false; privateLock = $false
        registryBackup = $null; registryRestore = 'pending'; preRestore = $null; intake = $null; integrity = $null; reason = $null }
    New-LcJournal $journal
    $stream = $null; $lease = $null
    try {
        [void](Acquire-KmgCompatibilityLock $script:LcStateRoot $runId)
        $journal.sharedLock = $true; Save-LcJournal $journal
        Step-LcBoundary 'shared-lock-created'
        $lockPath = Get-LcSharedLockPath
        $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
        [void][IO.Directory]::CreateDirectory($leaseDir)
        $lease = [pscustomobject]@{ RunId = $runId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
            StatePath = $journal.leaseStatePath; Compatibility = $false
            State = [ordered]@{ schemaVersion = 1; runId = $runId; purpose = ('loader-compat ' + $SessionId); processId = $PID
                processStartedUtc = $journal.ownerProcessStartedUtc; status = 'Active'; recoveryRequired = $false; reason = $null
                settings = $null; transactionId = $journal.transactionId } }
        & $script:LcLeaseStateWriter $lease
        $journal.privateLockIntended = $true; Save-LcJournal $journal
        Enter-LcLock $SessionId
        Step-LcBoundary 'private-lock-created'
        $journal.privateLock = $true; $journal.phase = 'acquired'; Save-LcJournal $journal
    } catch {
        $failure = $_
        # A modelled process death: handles close as the OS would close them; nothing else is undone.
        if (Test-LcInterruption $failure) { if ($null -ne $stream) { $stream.Dispose() }; throw }
        try {
            if ($null -ne $stream) { $stream.Dispose() }
            if ($journal.privateLockIntended -and (Test-LcPrivateLockOwned $SessionId)) { Exit-LcLock $SessionId }
            $lockPath = Get-LcSharedLockPath
            if ((Test-Path -LiteralPath $lockPath) -and (Get-Content -LiteralPath $lockPath -Raw).Trim() -ceq $runId) {
                if (Test-Path -LiteralPath $journal.leaseStatePath) {
                    $state = Get-Content -LiteralPath $journal.leaseStatePath -Raw | ConvertFrom-Json
                    $state.status = 'Completed'; $state.reason = 'Loader-compat session did not start: ' + $failure.Exception.Message
                    [IO.File]::WriteAllText($journal.leaseStatePath, ($state | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
                }
                Remove-KmgCompatibilityOwnedLock $lockPath $runId
            }
            Remove-LcJournal $journal
        } catch { throw "Session start failed ($($failure.Exception.Message)) and its rollback could not finish ($($_.Exception.Message)); the journal is kept for -RecoverOnly." }
        throw $failure
    }
    return [pscustomobject]@{ Scope = [pscustomobject]@{ Lease = $lease; Acquired = $true }; Journal = $journal }
}

# --- owner-file integrity -------------------------------------------------------

# Records the owner's saves and installation manifests with digests in the
# session's private transaction state beside its lease.
function Save-LcIntake($Journal) {
    $directory = Get-LcTransactionDir $Journal
    $saves = [string[]]@(& $script:LcSavesManifest); $install = [string[]]@(& $script:LcInstallManifest)
    $savesPath = Join-Path $directory 'owner-saves.intake.txt'; $installPath = Join-Path $directory 'owner-install.intake.txt'
    $utf8 = New-Object Text.UTF8Encoding($false)
    [IO.File]::WriteAllLines($savesPath, $saves, $utf8); [IO.File]::WriteAllLines($installPath, $install, $utf8)
    $Journal.intake = [ordered]@{ savesPath = $savesPath; savesSha256 = (Get-LcSha256 $savesPath); savesCount = $saves.Count
        installPath = $installPath; installSha256 = (Get-LcSha256 $installPath); installCount = $install.Count }
    Save-LcJournal $Journal
    return $Journal.intake
}

function Read-LcIntake([string]$Path, [string]$Sha256) {
    if ((Get-LcSha256 $Path) -cne $Sha256) { throw "the recorded intake manifest changed: $Path" }
    return , [string[]]@([IO.File]::ReadAllLines($Path) | Where-Object { $_ })
}

# The owner-file comparison against the recorded intake; any read failure, or no
# intake at all, is an explicit 'unresolved' result rather than a pass.
function Test-LcOwnerIntegrity($Journal) {
    try {
        $intake = $Journal.intake
        if ($null -eq $intake) { return [ordered]@{ status = 'unresolved'; reason = 'no intake manifests were recorded before the session stopped' } }
        $savesBefore = Read-LcIntake $intake.savesPath $intake.savesSha256
        $installBefore = Read-LcIntake $intake.installPath $intake.installSha256
        $savesDiff = Compare-LcManifest $savesBefore ([string[]]@(& $script:LcSavesManifest))
        $installDiff = Compare-LcManifest $installBefore ([string[]]@(& $script:LcInstallManifest))
        return [ordered]@{ status = $(if ($savesDiff.Count -eq 0 -and $installDiff.Count -eq 0) { 'verified' } else { 'changed' })
            ownerSavesUnchanged = ($savesDiff.Count -eq 0); ownerInstallUnchanged = ($installDiff.Count -eq 0)
            ownerSaveChanges = @($savesDiff); ownerInstallChanges = @($installDiff); ownerSaveFiles = $savesBefore.Count }
    } catch { return [ordered]@{ status = 'unresolved'; reason = $_.Exception.Message } }
}

# --- registry restoration --------------------------------------------------------

function Get-LcRegistryFingerprint([string]$Path) {
    return (@([IO.File]::ReadAllLines($Path) | Where-Object { $_.Trim() } | Sort-Object) -join "`n")
}

# After an interruption during restoration: the current game settings are compared
# with the session backup (already applied) and with the snapshot taken just
# before the import (not applied). Anything else cannot be decided safely.
function Resolve-LcInterruptedRestore($Journal) {
    try {
        $probe = Join-Path (Get-LcTransactionDir $Journal) ('registry-reconcile-' + [Guid]::NewGuid().ToString('N') + '.reg')
        & $script:LcRegistryExport $probe
        $current = Get-LcRegistryFingerprint $probe
        if ($current -ceq (Get-LcRegistryFingerprint $Journal.registryBackup)) { return 'applied' }
        $pre = $Journal.preRestore
        if ($null -ne $pre -and (Test-Path -LiteralPath $pre.path) -and (Get-LcSha256 $pre.path) -ceq $pre.sha256 -and
            $current -ceq (Get-LcRegistryFingerprint $pre.path)) { return 'not-applied' }
        return 'unresolved: the game settings match neither the session backup nor the snapshot taken just before restoration'
    } catch { return "unresolved: $($_.Exception.Message)" }
}

# --- completion -------------------------------------------------------------------

function New-LcResult([string]$Outcome, [string]$Reason, $Journal) {
    return [pscustomobject]@{ Outcome = $Outcome; Reason = $Reason; Registry = [string]$Journal.registryRestore; Integrity = $Journal.integrity }
}

function Set-LcDeferred($Journal, $Lease, [string]$Reason) {
    $Journal.phase = 'recovery-required'; $Journal.reason = $Reason
    Save-LcJournal $Journal
    if ($null -ne $Lease -and $null -ne $Lease.State) {
        $Lease.State.status = 'RecoveryRequired'; $Lease.State.recoveryRequired = $true; $Lease.State.reason = $Reason
        & $script:LcLeaseStateWriter $Lease
    }
    return (New-LcResult 'recovery-required' $Reason $Journal)
}

# Clean only when the transaction completed, the game settings were restored (or
# never needed restoring) and the owner's files are verified unchanged.
function Test-LcTransactionSucceeded($Result) {
    return $null -ne $Result -and $Result.Outcome -eq 'completed' -and $Result.Registry -in @('applied', 'not-needed') -and
        $null -ne $Result.Integrity -and $Result.Integrity.status -eq 'verified'
}

function Get-LcTransactionExitCode($Result) { if (Test-LcTransactionSucceeded $Result) { return 0 } else { return 1 } }

function Complete-LcTransaction($Journal, $Lease, [scriptblock]$Restore, [int]$WaitSeconds) {
    $released = @('lease-completed', 'shared-released', 'private-released')
    if ($Journal.phase -notin $released) {
        $conflicts = Wait-LcSafeBoundary $Journal.leaseRunId $WaitSeconds
        if ($conflicts.Count -gt 0) {
            return Set-LcDeferred $Journal $Lease "Not at a safe boundary ($($conflicts -join '; ')); game settings were not restored and ownership is retained. Run Invoke-LoaderCompatSession.ps1 -RecoverOnly once the host is free."
        }
        if ($Journal.registryRestore -eq 'pending') {
            if ($Journal.registryBackup) {
                $pre = Join-Path (Get-LcTransactionDir $Journal) 'registry-before-restore.reg'
                & $script:LcRegistryExport $pre
                $Journal.preRestore = [ordered]@{ path = $pre; sha256 = (Get-LcSha256 $pre) }
                $Journal.registryRestore = 'restoring'; $Journal.phase = 'restoring'; Save-LcJournal $Journal
                Step-LcBoundary 'restoring'
                try { & $Restore $Journal.registryBackup }
                catch { return Set-LcDeferred $Journal $Lease "Game-settings restoration failed: $($_.Exception.Message). Recovery reconciles it against the backup and the pre-restore snapshot." }
                Step-LcBoundary 'import-applied'
                $Journal.registryRestore = 'applied'
            } else { $Journal.registryRestore = 'not-needed' }
            $Journal.phase = 'restored'; Save-LcJournal $Journal
            Step-LcBoundary 'restored'
        } elseif ($Journal.registryRestore -eq 'restoring') {
            $decision = Resolve-LcInterruptedRestore $Journal
            if ($decision -eq 'not-applied') {
                try { & $Restore $Journal.registryBackup }
                catch { return Set-LcDeferred $Journal $Lease "Game-settings restoration failed: $($_.Exception.Message)." }
                $decision = 'applied'
            }
            $Journal.registryRestore = $decision; $Journal.phase = 'restored'; Save-LcJournal $Journal
            Step-LcBoundary 'restored'
        }
        if ($null -eq $Journal.integrity) {
            $Journal.integrity = Test-LcOwnerIntegrity $Journal
            $Journal.phase = 'verified'; Save-LcJournal $Journal
            Step-LcBoundary 'verified'
        }
        # The host must still be free when the result is committed and ownership released.
        $late = Get-LcHostConflicts $Journal.leaseRunId
        if ($late.Count -gt 0) {
            $Journal.integrity = $null
            return Set-LcDeferred $Journal $Lease "The host became busy during verification ($($late -join '; ')); the restoration is recorded, ownership is retained and the owner files are verified again at the next safe boundary."
        }
        if ($null -ne $Lease -and $null -ne $Lease.State) {
            Set-LcStateValue $Lease.State 'integrity' $Journal.integrity
            Set-LcStateValue $Lease.State 'registryRestore' $Journal.registryRestore
            $Lease.State.status = 'Completed'; $Lease.State.recoveryRequired = $false
            $Lease.State.reason = 'Loader-compat session completed at a safe boundary: game settings restored once, owner files compared with the intake, host re-checked, then ownership released.'
            & $script:LcLeaseStateWriter $Lease
        }
        $Journal.phase = 'lease-completed'; Save-LcJournal $Journal
        Step-LcBoundary 'lease-completed'
    }
    if ($Journal.phase -eq 'lease-completed') {
        if ($null -ne $Lease -and $null -ne $Lease.Stream) { $Lease.Stream.Dispose() }
        if ($Journal.leaseRunId) { Remove-KmgCompatibilityOwnedLock (Get-LcSharedLockPath) $Journal.leaseRunId }
        $Journal.phase = 'shared-released'; Save-LcJournal $Journal
        Step-LcBoundary 'shared-released'
    }
    if ($Journal.privateLock -or ($Journal.privateLockIntended -and (Test-LcPrivateLockOwned $Journal.sessionId))) {
        Exit-LcLock $Journal.sessionId
        $Journal.privateLock = $false; $Journal.privateLockIntended = $false; $Journal.phase = 'private-released'; Save-LcJournal $Journal
        Step-LcBoundary 'private-released'
    }
    Remove-LcJournal $Journal
    return (New-LcResult 'completed' $null $Journal)
}

# The live runner's finalizer.
function Complete-LcSessionOwnership($Journal, $Scope, [scriptblock]$Restore, [int]$WaitSeconds = 60) {
    $lease = if ($null -ne $Scope) { $Scope.Lease } else { $null }
    $result = Complete-LcTransaction $Journal $lease $Restore $WaitSeconds
    if ($result.Outcome -ne 'completed' -and $null -ne $lease -and $null -ne $lease.Stream) { $lease.Stream.Dispose() }
    return $result
}

# --- recovery ----------------------------------------------------------------------

function Test-LcOwnerAlive($Journal) {
    if ($Journal.ownerProcessStartedUtc) {
        $id = [int]$Journal.ownerProcessId
        return Test-KmgRuntimeProcessIdentity (Get-Process -Id $id -ErrorAction SilentlyContinue) $id ([string]$Journal.ownerProcessStartedUtc)
    }
    if ($Journal.Contains('ownerLockStartedUtc') -and $Journal.ownerLockStartedUtc) {
        # Older journals: the private lock's pid is alive if a process with it started before the lock was written.
        $process = Get-Process -Id ([int]$Journal.ownerProcessId) -ErrorAction SilentlyContinue
        return $null -ne $process -and $process.StartTime.ToUniversalTime() -le ([DateTime]::Parse($Journal.ownerLockStartedUtc).ToUniversalTime())
    }
    return $false
}

# Journals written by the round-7 and round-8 runners and by earlier runners,
# converted with the same ownership facts. A round-7 lease already Completed is
# honoured (that runner completed its lease only after restoring): cleanup only.
function Convert-LcLegacyJournal($Raw) {
    $journal = [ordered]@{ schemaVersion = 3; transactionId = [Guid]::NewGuid().ToString('N'); convertedFrom = 'pre-schema-3'
        sessionId = [string]$Raw.sessionId; phase = 'recovery-required'
        ownerProcessId = $null; ownerProcessStartedUtc = $null; ownerLockStartedUtc = $null
        leaseRunId = $null; leaseStatePath = $null; sharedLock = $false; privateLockIntended = $false; privateLock = $false
        registryBackup = $(if ($Raw.PSObject.Properties['registryBackup']) { $Raw.registryBackup } else { $null })
        registryRestore = 'pending'; preRestore = $null; intake = $null; integrity = $null; reason = 'converted from an older session journal' }
    if ($Raw.PSObject.Properties['schemaVersion'] -and $Raw.schemaVersion -eq 2) {
        $journal.convertedFrom = 'schema-2'
        foreach ($name in @('ownerProcessId', 'ownerProcessStartedUtc', 'leaseRunId', 'leaseStatePath', 'intake', 'integrity')) { $journal[$name] = $Raw.$name }
        $journal.sharedLock = [bool]$Raw.leaseRunId; $journal.privateLock = [bool]$Raw.privateLock
        $journal.registryRestore = $(if ($Raw.registryRestored) { 'applied' } elseif ($Raw.phase -eq 'restoring') { 'restoring' } else { 'pending' })
        if ($Raw.phase -in @('lease-completed', 'shared-released', 'private-released')) { $journal.phase = $Raw.phase }
    } elseif ($Raw.PSObject.Properties['leaseRunId'] -and $Raw.leaseRunId) {
        $journal.convertedFrom = 'round-7'
        $journal.leaseRunId = [string]$Raw.leaseRunId; $journal.leaseStatePath = [string]$Raw.leaseStatePath; $journal.sharedLock = $true
        $state = Read-KmgCompatibilityJson $journal.leaseStatePath
        $journal.ownerProcessId = $state.processId; $journal.ownerProcessStartedUtc = [string]$state.processStartedUtc
        if ($state.status -ceq 'Completed') {
            $journal.phase = 'lease-completed'; $journal.registryRestore = 'applied'
            $journal.integrity = [ordered]@{ status = 'not-recorded'; reason = 'completed by the round-7 runner, whose owner-file comparison is in its session summary' }
        } elseif ($state.status -ceq 'Active') { $journal.registryRestore = 'restoring' }
    }
    $private = Get-LcPrivateLockInfo
    if ($null -ne $private -and $private['session'] -ceq $journal.sessionId) {
        $journal.privateLock = $true
        if (-not $journal.ownerProcessStartedUtc) { $journal.ownerProcessId = $private['pid']; $journal.ownerLockStartedUtc = $private['startedUtc'] }
    }
    return $journal
}

# Serializes recovery callers; the OS deletes the guard if its holder dies.
function Enter-LcRecoveryGuard {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $script:LcRecoveryLock) | Out-Null
    try { return New-Object IO.FileStream($script:LcRecoveryLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None, 4096, [IO.FileOptions]::DeleteOnClose) }
    catch [IO.IOException] { throw 'Another recovery of this session is in progress; nothing was changed.' }
}

function New-LcRecoveryResult([string]$Message, [bool]$Succeeded, $Result) {
    return [pscustomobject]@{ Message = $Message; Succeeded = $Succeeded; Result = $Result }
}

# -RecoverOnly. Callers are serialized by the recovery guard; under it the journal
# is re-read (another caller may have finished, or a newer session started), the
# original owner must have exited, the host must be free, the shared lock -
# re-taken under the recorded identity if missing, or taken afresh for an
# unleased older journal - must be held through this caller's exclusive handle,
# and the journal must still be the same transaction before anything is written.
# A Completed lease means cleanup only. Refused callers change nothing.
function Invoke-LcRecovery([scriptblock]$Restore) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { return New-LcRecoveryResult 'No interrupted session journal.' $true $null }
    $firstRead = Get-LcJournalTransactionId
    if ($null -ne $script:LcRecoveryPauseHook) { $hook = $script:LcRecoveryPauseHook; $script:LcRecoveryPauseHook = $null; & $hook }
    $guard = Enter-LcRecoveryGuard
    $stream = $null
    try {
        if (-not (Test-Path -LiteralPath $script:LcJournal)) {
            return New-LcRecoveryResult "The interrupted session (journal $firstRead) was already recovered by another caller; nothing was changed." $true $null
        }
        $raw = Read-LcJournalFile
        $journal = if ($raw.PSObject.Properties['schemaVersion'] -and $raw.schemaVersion -eq 3) { ConvertTo-LcOrdered $raw } else { Convert-LcLegacyJournal $raw }
        $cleanupOnly = $journal.phase -in @('lease-completed', 'shared-released', 'private-released')
        if (Test-LcOwnerAlive $journal) { throw "The session owner (pid $($journal.ownerProcessId)) is still alive; nothing was changed." }
        $conflicts = Get-LcHostConflicts $journal.leaseRunId
        if ($conflicts.Count -gt 0) { throw "Recovery withheld: $($conflicts -join '; '). Nothing was changed." }
        $lockPath = Get-LcSharedLockPath
        $lease = $null
        if (-not $journal.leaseRunId) {
            # Unleased older journal: this recovery takes the shared lock itself (a holder makes CreateNew fail).
            $journal.leaseRunId = New-LcLeaseRunId
            [void](Acquire-KmgCompatibilityLock $script:LcStateRoot $journal.leaseRunId)
            $journal.sharedLock = $true
            $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
            $lease = [pscustomobject]@{ RunId = $journal.leaseRunId; LockPath = $lockPath; Stream = $stream; State = $null }
            Save-LcJournal $journal
        } elseif ($journal.phase -notin @('shared-released', 'private-released')) {
            if (-not (Test-Path -LiteralPath $lockPath)) {
                if ($journal.phase -eq 'lease-completed') { $journal.phase = 'shared-released' }
                elseif ($journal.sharedLock) { [void](Acquire-KmgCompatibilityLock $script:LcStateRoot $journal.leaseRunId) }
            }
            if ($journal.phase -ne 'shared-released' -and (Test-Path -LiteralPath $lockPath)) {
                try { $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read) }
                catch [IO.IOException] { throw "Another caller holds this session's recovery; nothing was changed." }
                if ((Get-Content -LiteralPath $lockPath -Raw).Trim() -cne $journal.leaseRunId) { throw 'The shared lock is not held by this session; nothing was changed.' }
                $state = if ($journal.leaseStatePath -and (Test-Path -LiteralPath $journal.leaseStatePath)) { Read-KmgCompatibilityJson $journal.leaseStatePath } else { $null }
                if ($null -ne $state) {
                    if ([string]$state.runId -cne $journal.leaseRunId -or [string]$state.purpose -cne ('loader-compat ' + $journal.sessionId)) {
                        throw 'The recorded lease does not belong to this session; nothing was changed.'
                    }
                    if ($state.status -ceq 'Completed' -and $journal.phase -ne 'lease-completed') {
                        # The lease was completed only after restoration and verification: cleanup only.
                        $journal.phase = 'lease-completed'; $cleanupOnly = $true
                        if ($journal.registryRestore -in @('pending', 'restoring')) { $journal.registryRestore = 'applied' }
                        if ($state.PSObject.Properties['integrity'] -and $null -eq $journal.integrity) { $journal.integrity = $state.integrity }
                        $state = $null
                    }
                }
                $lease = [pscustomobject]@{ RunId = $journal.leaseRunId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
                    StatePath = $journal.leaseStatePath; StateSha256 = $(if ($null -ne $state) { Get-KmgCompatibilitySha256 $journal.leaseStatePath } else { $null })
                    Compatibility = $false; State = $state }
            }
        }
        # Under full ownership the journal must still be this transaction before anything is written.
        if (-not (Test-LcJournalOwned $journal)) { throw 'The session journal changed while recovery took ownership; nothing was changed.' }
        Remove-LcJournalTemps
        $result = Complete-LcTransaction $journal $lease $Restore 0
        $ok = if ($cleanupOnly) { $result.Outcome -eq 'completed' } else { Test-LcTransactionSucceeded $result }
        $text = "Recovery of $($journal.sessionId): outcome $($result.Outcome); game settings $($result.Registry); owner files $(if ($null -ne $result.Integrity) { $result.Integrity.status } else { 'not verified' })$(if ($cleanupOnly) { ' (cleanup only)' })$(if ($result.Reason) { '; ' + $result.Reason })."
        return New-LcRecoveryResult $text $ok $result
    } finally {
        if ($null -ne $stream) { $stream.Dispose() }
        $guard.Dispose()
    }
}
