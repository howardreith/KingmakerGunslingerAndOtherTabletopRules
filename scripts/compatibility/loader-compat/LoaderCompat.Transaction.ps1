Set-StrictMode -Version Latest
# ---------------------------------------------------------------------------
# Loader-compat session transaction (journal schema 4), dot-sourced by
# LoaderCompat.Common.ps1.
#
# One journal ($script:LcJournal) is the durable record of a session. Every
# claim - the journal itself, the shared compatibility.lock and the private
# loader-compat.lock - is written completely to a private file and then
# published with an atomic no-overwrite rename, so a claim is either absent or
# complete. Every journal update replaces the file atomically, only while it
# still names this transaction. Identities are journaled before the claims they
# name exist. The registry backup is bound to a digest at intake, a snapshot is
# journaled before each import, and the host is re-checked immediately before
# every import. Completion and -RecoverOnly share Complete-LcTransaction; an
# inconclusive registry state keeps the journal, the locks and a
# RecoveryRequired lease until it is restored or the owner resolves it
# explicitly (-RegistryResolution Import|KeepCurrent).
# ---------------------------------------------------------------------------

# Fixture-only hooks. In every live run these hold the real operations or $null.
$script:LcInterruptAfter = $null          # boundary at which to model process death
$script:LcRecoveryPauseHook = $null       # runs once after recovery's first journal read
$script:LcSavesManifest = { Get-LcOwnerSavesManifest }
$script:LcInstallManifest = { Get-LcOwnerInstallManifest }
# Exports the live game-settings key to a file: returns 'exported', or 'absent' when the key does not exist;
# throws when the key cannot be read.
$script:LcRegistryExport = {
    param($Path)
    $r = Invoke-LcReg @('export', $script:LcRegistryKey, $Path, '/y')
    if ($r.ExitCode -eq 0) { return 'exported' }
    if ($r.Output -match 'unable to find') { return 'absent' }
    throw "Registry export failed: $($r.Output)"
}
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

function Get-LcValue($Object, [string]$Name) {
    if ($null -eq $Object) { return $null }
    if ($Object -is [Collections.IDictionary]) { if ($Object.Contains($Name)) { return $Object[$Name] } else { return $null } }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }
    return $property.Value
}

function Set-LcStateValue($State, [string]$Name, $Value) {
    if ($State -is [Collections.IDictionary]) { $State[$Name] = $Value }
    else { $State | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force }
}

function Get-LcSharedLockPath { return Join-Path $script:LcStateRoot 'compatibility.lock' }
function New-LcLeaseRunId { return 'runtime-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N') }
function Test-LcPrivateLockOwned([string]$SessionId) { $info = Get-LcPrivateLockInfo; return $null -ne $info -and $info['session'] -ceq $SessionId }
function Get-LcTransactionDir($Journal) {
    if ($Journal.leaseStatePath) { $directory = Split-Path -Parent $Journal.leaseStatePath }
    else { $directory = Split-Path -Parent $script:LcJournal }
    [void][IO.Directory]::CreateDirectory($directory)
    return $directory
}

# Writes a claim completely to a private file, then publishes it with an atomic
# rename that fails if the claim already exists: the claim is absent or complete.
function Publish-LcClaim([string]$Path, [string]$Content, [string]$ExistsMessage) {
    $directory = Split-Path -Parent $Path
    [void][IO.Directory]::CreateDirectory($directory)
    $temp = Join-Path $directory ('.' + (Split-Path -Leaf $Path) + '.' + [Guid]::NewGuid().ToString('N') + '.claim')
    $bytes = (New-Object Text.UTF8Encoding($false)).GetBytes($Content)
    $stream = [IO.File]::Open($temp, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
    Step-LcBoundary ('claim-written:' + (Split-Path -Leaf $Path))
    try { [IO.File]::Move($temp, $Path) }
    catch [IO.IOException] { Remove-Item -LiteralPath $temp -Force -ErrorAction SilentlyContinue; throw $ExistsMessage }
}

# The shared compatibility.lock in the format the lab's coordination reads (the run id), published atomically.
function Acquire-LcSharedLock([string]$RunId) {
    $path = Get-LcSharedLockPath
    Publish-LcClaim $path ($RunId + "`r`n") "A compatibility transaction lock already exists: $path"
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

function ConvertTo-LcJournalText($Journal) { return ($Journal | ConvertTo-Json -Depth 12) }

function Read-LcJournalFile { return Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json }

function Get-LcJournalTransactionId {
    try { $raw = Read-LcJournalFile } catch { return '<unreadable>' }
    if ($null -ne $raw -and $raw.PSObject.Properties['transactionId']) { return [string]$raw.transactionId }
    return '<legacy>'
}

# A stable identity for any journal: its transaction id, or the digest of an older journal's bytes.
function Get-LcJournalIdentity {
    $id = Get-LcJournalTransactionId
    if ($id -ceq '<legacy>' -or $id -ceq '<unreadable>') { return 'file-sha256:' + (Get-LcSha256 $script:LcJournal) }
    return $id
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
    $bytes = (New-Object Text.UTF8Encoding($false)).GetBytes((ConvertTo-LcJournalText $Journal))
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

# Leftover temporary files of an interrupted update or claim (the journal itself is the last complete record).
function Remove-LcJournalTemps {
    $directory = Split-Path -Parent $script:LcJournal
    if (-not (Test-Path -LiteralPath $directory)) { return }
    $leaf = Split-Path -Leaf $script:LcJournal
    foreach ($file in @(Get-ChildItem -LiteralPath $directory -File -Force | Where-Object { $_.Name -like ($leaf + '.*.tmp') -or $_.Name -like ('.' + $leaf + '.*.claim') })) {
        Remove-Item -LiteralPath $file.FullName -Force
    }
}

# --- acquisition ---------------------------------------------------------------

# Starts a session transaction. The journal is published complete and already
# names the lease identity the shared lock will carry; the private lock is
# recorded as intended before it is created. A failure rolls back exactly what
# this call acquired; if that cannot finish, the journal is kept and -RecoverOnly
# identifies every lock by its recorded identity.
function Enter-LcSessionTransaction([string]$SessionId) {
    $runId = New-LcLeaseRunId
    $leaseDir = Join-Path $script:LcStateRoot $runId
    $journal = [ordered]@{ schemaVersion = 4; transactionId = [Guid]::NewGuid().ToString('N'); sessionId = $SessionId; phase = 'acquiring'
        ownerProcessId = $PID; ownerProcessStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
        leaseRunId = $runId; leaseStatePath = (Join-Path $leaseDir 'runtime-lease.json'); sharedLock = $false
        privateLockIntended = $false; privateLock = $false
        registryBackup = $null; registryBackupSha256 = $null; registryRestore = 'pending'; preRestore = $null
        intake = $null; integrity = $null; reason = $null }
    Publish-LcClaim $script:LcJournal (ConvertTo-LcJournalText $journal) "Another session holds the loader-compat journal ($script:LcJournal); nothing was acquired."
    $stream = $null; $lease = $null
    try {
        Acquire-LcSharedLock $runId
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

# Binds the session's registry backup to its digest at intake.
function Set-LcRegistryBackup($Journal, [string]$Path) {
    $Journal.registryBackup = $Path
    $Journal.registryBackupSha256 = Get-LcSha256 $Path
    Save-LcJournal $Journal
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

# A registry export in canonical form: each key with its complete value entries
# (continuation lines joined in order), values ordered by name within their key,
# keys in ordinal order. Two exports compare equal only if every key path, value
# name, type and ordered data agree.
function Get-LcRegistryCanonical([string]$Path) {
    $sections = [Collections.Generic.SortedDictionary[string, Collections.Generic.List[string]]]::new([StringComparer]::Ordinal)
    $header = [Collections.Generic.List[string]]::new()
    $key = $null; $pending = $null
    foreach ($raw in [IO.File]::ReadAllLines($Path)) {
        $line = $raw.TrimEnd()
        if ($null -ne $pending) {
            $pending += "`n" + $line.Trim()
            if (-not $line.EndsWith('\')) { $sections[$key].Add($pending); $pending = $null }
            continue
        }
        $text = $line.Trim()
        if (-not $text) { continue }
        if ($text.StartsWith('[') -and $text.EndsWith(']')) {
            $key = $text.Substring(1, $text.Length - 2)
            if (-not $sections.ContainsKey($key)) { $sections[$key] = [Collections.Generic.List[string]]::new() }
            continue
        }
        if ($null -eq $key) { $header.Add($text); continue }
        if ($text.EndsWith('\')) { $pending = $text; continue }
        $sections[$key].Add($text)
    }
    if ($null -ne $pending -and $null -ne $key) { $sections[$key].Add($pending) }
    $out = [Collections.Generic.List[string]]::new()
    $out.AddRange($header)
    foreach ($entry in $sections.GetEnumerator()) {
        $out.Add('[' + $entry.Key + ']')
        $out.AddRange([string[]]@($entry.Value | Sort-Object -CaseSensitive))
    }
    return ($out -join "`n")
}

function Test-LcRegistryBackupIntact($Journal) {
    $digest = Get-LcValue $Journal 'registryBackupSha256'
    return $digest -and $Journal.registryBackup -and (Test-Path -LiteralPath $Journal.registryBackup -PathType Leaf) -and
        (Get-LcSha256 $Journal.registryBackup) -ceq $digest
}

# After an interruption during restoration, the live settings are compared with
# the verified backup (already applied) and with the snapshot journaled just
# before the import (not applied). A missing key means the delete ran and the
# import did not (not applied; nothing newer can be lost). An unreadable key is
# 'unreadable'; anything else 'unresolved'.
function Resolve-LcInterruptedRestore($Journal) {
    $probe = Join-Path (Get-LcTransactionDir $Journal) ('registry-reconcile-' + [Guid]::NewGuid().ToString('N') + '.reg')
    try { $status = & $script:LcRegistryExport $probe } catch { return "unreadable: $($_.Exception.Message)" }
    if ($status -ceq 'absent') { return 'not-applied' }
    $current = Get-LcRegistryCanonical $probe
    if ($current -ceq (Get-LcRegistryCanonical $Journal.registryBackup)) { return 'applied' }
    $pre = $Journal.preRestore
    if ($null -ne $pre -and (Get-LcValue $pre 'state') -ceq 'exported' -and (Test-Path -LiteralPath $pre.path) -and
        (Get-LcSha256 $pre.path) -ceq $pre.sha256 -and $current -ceq (Get-LcRegistryCanonical $pre.path)) { return 'not-applied' }
    return 'unresolved: the live game settings match neither the verified session backup nor the snapshot taken just before restoration'
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
# never needed restoring, or the owner explicitly kept the current settings) and
# the owner's files are verified unchanged.
function Test-LcTransactionSucceeded($Result) {
    return $null -ne $Result -and $Result.Outcome -eq 'completed' -and $Result.Registry -in @('applied', 'not-needed', 'owner-kept-current') -and
        $null -ne $Result.Integrity -and (Get-LcValue $Result.Integrity 'status') -eq 'verified'
}

function Get-LcTransactionExitCode($Result) { if (Test-LcTransactionSucceeded $Result) { return 0 } else { return 1 } }

$script:LcResolutionHint = 'Re-run Invoke-LoaderCompatSession.ps1 -RecoverOnly with -RegistryResolution Import (import the session backup) or KeepCurrent (keep the current settings) once the owner has decided.'

# The registry step. Returns $null when the game settings are settled, or a
# deferred result that keeps the journal, the locks and a RecoveryRequired lease.
function Invoke-LcRegistryStep($Journal, $Lease, [scriptblock]$Restore, [string]$Resolution) {
    if ($Journal.registryRestore -in @('applied', 'not-needed', 'owner-kept-current')) { return $null }
    if (-not $Journal.registryBackup) {
        $Journal.registryRestore = 'not-needed'; $Journal.phase = 'restored'; Save-LcJournal $Journal; Step-LcBoundary 'restored'
        return $null
    }
    $settle = { param($Value) $Journal.registryRestore = $Value; $Journal.phase = 'restored'; Save-LcJournal $Journal; Step-LcBoundary 'restored' }
    if ($Resolution -ceq 'KeepCurrent') { & $settle 'owner-kept-current'; return $null }
    $digest = Get-LcValue $Journal 'registryBackupSha256'
    $importNow = $false
    if (-not (Test-LcRegistryBackupIntact $Journal)) {
        if ($digest) { return Set-LcDeferred $Journal $Lease "The registry backup no longer matches the digest recorded at intake; it was not compared or imported. $script:LcResolutionHint (only KeepCurrent applies to a changed backup)" }
        if ($Resolution -cne 'Import' -or -not (Test-Path -LiteralPath $Journal.registryBackup -PathType Leaf)) {
            return Set-LcDeferred $Journal $Lease "This older journal records no digest for its registry backup and no restore phase; the backup is neither compared nor imported without the owner's decision. $script:LcResolutionHint"
        }
        $importNow = $true
    } elseif ($Journal.registryRestore -ceq 'restoring') {
        $decision = Resolve-LcInterruptedRestore $Journal
        if ($decision -ceq 'applied') { & $settle 'applied'; return $null }
        if ($decision -like 'unreadable*') { return Set-LcDeferred $Journal $Lease "The live game settings could not be read ($decision); nothing was imported and ownership is retained for another -RecoverOnly." }
        if ($decision -like 'unresolved*') {
            if ($Resolution -cne 'Import') { return Set-LcDeferred $Journal $Lease "$decision. Nothing was imported. $script:LcResolutionHint" }
        }
        $importNow = $true
    } else {
        $pre = Join-Path (Get-LcTransactionDir $Journal) ('registry-before-restore-' + [Guid]::NewGuid().ToString('N') + '.reg')
        try { $status = & $script:LcRegistryExport $pre } catch { return Set-LcDeferred $Journal $Lease "The live game settings could not be read before restoration ($($_.Exception.Message)); nothing was imported." }
        $Journal.preRestore = [ordered]@{ path = $pre; state = $status; sha256 = $(if ($status -ceq 'exported') { Get-LcSha256 $pre } else { $null }) }
        $importNow = $true
    }
    if ($importNow) {
        $from = $Journal.registryRestore
        $Journal.registryRestore = 'restoring'; $Journal.phase = 'restoring'; Save-LcJournal $Journal
        Step-LcBoundary 'restoring'
        # Immediately before the destructive delete-and-import: the host must still be free.
        $conflicts = Get-LcHostConflicts $Journal.leaseRunId
        if ($conflicts.Count -gt 0) {
            $Journal.registryRestore = $(if ($from -ceq 'pending') { 'pending' } else { 'restoring' })
            return Set-LcDeferred $Journal $Lease "The import was withheld because the host became busy ($($conflicts -join '; ')); game settings were not changed and ownership is retained."
        }
        if ($digest -and -not (Test-LcRegistryBackupIntact $Journal)) {
            return Set-LcDeferred $Journal $Lease 'The registry backup changed before the import; nothing was imported.'
        }
        try { & $Restore $Journal.registryBackup }
        catch { return Set-LcDeferred $Journal $Lease "Game-settings restoration failed: $($_.Exception.Message). Recovery reconciles it against the backup and the pre-restore snapshot." }
        Step-LcBoundary 'import-applied'
        & $settle 'applied'
    }
    return $null
}

function Complete-LcTransaction($Journal, $Lease, [scriptblock]$Restore, [int]$WaitSeconds, [string]$Resolution = '') {
    $released = @('lease-completed', 'shared-released', 'private-released')
    if ($Journal.phase -notin $released) {
        $conflicts = Wait-LcSafeBoundary $Journal.leaseRunId $WaitSeconds
        if ($conflicts.Count -gt 0) {
            return Set-LcDeferred $Journal $Lease "Not at a safe boundary ($($conflicts -join '; ')); game settings were not restored and ownership is retained. Run Invoke-LoaderCompatSession.ps1 -RecoverOnly once the host is free."
        }
        $deferred = Invoke-LcRegistryStep $Journal $Lease $Restore $Resolution
        if ($null -ne $deferred) { return $deferred }
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
            $Lease.State.reason = 'Loader-compat session completed at a safe boundary: game settings settled, owner files compared with the intake, host re-checked, then ownership released.'
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
    $result = Complete-LcTransaction $Journal $lease $Restore $WaitSeconds ''
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

# A schema-2 journal interrupted between creating its lease and recording it: the
# lease is adopted only if it names this session, belongs to the journal's owner
# process and is the run currently holding the shared lock.
function Find-LcMatchingSchema2Lease($Journal) {
    $lockPath = Get-LcSharedLockPath
    if (-not (Test-Path -LiteralPath $lockPath) -or -not (Test-Path -LiteralPath $script:LcStateRoot)) { return $null }
    $holder = (Get-Content -LiteralPath $lockPath -Raw).Trim()
    $path = Join-Path (Join-Path $script:LcStateRoot $holder) 'runtime-lease.json'
    if (-not (Test-Path -LiteralPath $path)) { return $null }
    $state = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if ([string]$state.runId -ceq $holder -and [string]$state.purpose -ceq ('loader-compat ' + $Journal.sessionId) -and
        [string]$state.processId -ceq [string]$Journal.ownerProcessId -and [string]$state.processStartedUtc -ceq [string]$Journal.ownerProcessStartedUtc) {
        return [pscustomobject]@{ RunId = $holder; Path = $path }
    }
    return $null
}

# Journals written by the round-9, round-8, round-7 and earlier runners,
# converted with the same ownership facts. Their registry backups carry no
# digest, so any outstanding import needs the owner's explicit resolution. A
# round-7 lease already Completed is honoured as cleanup only, with its
# owner-file evidence recorded as unavailable.
function Convert-LcLegacyJournal($Raw) {
    $schema = if ($Raw.PSObject.Properties['schemaVersion']) { [int]$Raw.schemaVersion } else { 0 }
    if ($schema -eq 3) {
        $journal = ConvertTo-LcOrdered $Raw
        $journal['convertedFrom'] = 'schema-3'; $journal['registryBackupSha256'] = $null
        return $journal
    }
    $journal = [ordered]@{ schemaVersion = 4; transactionId = [Guid]::NewGuid().ToString('N'); convertedFrom = 'pre-schema-3'
        sessionId = [string]$Raw.sessionId; phase = 'recovery-required'
        ownerProcessId = $null; ownerProcessStartedUtc = $null; ownerLockStartedUtc = $null
        leaseRunId = $null; leaseStatePath = $null; sharedLock = $false; privateLockIntended = $false; privateLock = $false
        registryBackup = (Get-LcValue $Raw 'registryBackup'); registryBackupSha256 = $null; registryRestore = 'restoring'; preRestore = $null
        intake = $null; integrity = $null; reason = 'converted from an older session journal' }
    if ($schema -eq 2) {
        $journal.convertedFrom = 'schema-2'
        foreach ($name in @('ownerProcessId', 'ownerProcessStartedUtc', 'leaseRunId', 'leaseStatePath', 'intake', 'integrity')) { $journal[$name] = Get-LcValue $Raw $name }
        $journal.sharedLock = [bool]$journal.leaseRunId; $journal.privateLock = [bool](Get-LcValue $Raw 'privateLock')
        if (Get-LcValue $Raw 'registryRestored') { $journal.registryRestore = 'applied' }
        if ((Get-LcValue $Raw 'phase') -in @('lease-completed', 'shared-released', 'private-released')) { $journal.phase = $Raw.phase }
        if (-not $journal.leaseRunId) {
            $match = Find-LcMatchingSchema2Lease $journal
            if ($null -ne $match) { $journal.leaseRunId = $match.RunId; $journal.leaseStatePath = $match.Path; $journal.sharedLock = $true }
        }
    } elseif ((Get-LcValue $Raw 'leaseRunId')) {
        $journal.convertedFrom = 'round-7'
        $journal.leaseRunId = [string]$Raw.leaseRunId; $journal.leaseStatePath = [string]$Raw.leaseStatePath; $journal.sharedLock = $true
        $state = Read-KmgCompatibilityJson $journal.leaseStatePath
        $journal.ownerProcessId = $state.processId; $journal.ownerProcessStartedUtc = [string]$state.processStartedUtc
        if ($state.status -ceq 'Completed') {
            $journal.phase = 'lease-completed'; $journal.registryRestore = 'applied'
            $journal.integrity = [ordered]@{ status = 'unavailable'; reason = 'completed by the round-7 runner, whose owner-file comparison is not recorded in its lease or journal' }
        }
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

# -RecoverOnly. The journal identity read first must still be the journal found
# under the recovery guard (a newer session is left for its own recovery). Then
# the original owner must have exited, the host must be free, and the shared lock
# - re-taken under the recorded identity if missing, or taken afresh for an
# unleased older journal after that identity is journaled - must be held through
# this caller's exclusive handle, with the journal still the same transaction,
# before anything is written. The owner-file comparison is refreshed for any
# transaction not yet committed. The command's verdict is the recorded
# transaction verdict, cleanup-only completions included.
function Invoke-LcRecovery([scriptblock]$Restore, [string]$RegistryResolution = '') {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { return New-LcRecoveryResult 'No interrupted session journal.' $true $null }
    $observed = Get-LcJournalIdentity
    if ($null -ne $script:LcRecoveryPauseHook) { $hook = $script:LcRecoveryPauseHook; $script:LcRecoveryPauseHook = $null; & $hook }
    $guard = Enter-LcRecoveryGuard
    $stream = $null
    try {
        if (-not (Test-Path -LiteralPath $script:LcJournal)) {
            return New-LcRecoveryResult "The interrupted session ($observed) was already recovered by another caller; nothing was changed." $true $null
        }
        $current = Get-LcJournalIdentity
        if ($current -cne $observed) {
            throw "The session journal now belongs to a different transaction ($current) than the one this recovery read ($observed); nothing was changed. Run -RecoverOnly again for that session."
        }
        $raw = Read-LcJournalFile
        $journal = if ($raw.PSObject.Properties['schemaVersion'] -and [int]$raw.schemaVersion -eq 4) { ConvertTo-LcOrdered $raw } else { Convert-LcLegacyJournal $raw }
        if (Test-LcOwnerAlive $journal) { throw "The session owner (pid $($journal.ownerProcessId)) is still alive; nothing was changed." }
        $conflicts = Get-LcHostConflicts $journal.leaseRunId
        if ($conflicts.Count -gt 0) { throw "Recovery withheld: $($conflicts -join '; '). Nothing was changed." }
        $lockPath = Get-LcSharedLockPath
        $lease = $null
        if (-not $journal.leaseRunId) {
            # Unleased older journal: journal the recovery's lease identity first, then take the shared lock under it.
            $journal.leaseRunId = New-LcLeaseRunId; $journal.sharedLock = $false
            Save-LcJournal $journal
            Step-LcBoundary 'recovery-lease-intended'
            Acquire-LcSharedLock $journal.leaseRunId
            Step-LcBoundary 'recovery-lock-acquired'
            $journal.sharedLock = $true; Save-LcJournal $journal
            $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
            $lease = [pscustomobject]@{ RunId = $journal.leaseRunId; LockPath = $lockPath; Stream = $stream; State = $null }
        } elseif ($journal.phase -notin @('shared-released', 'private-released')) {
            if (-not (Test-Path -LiteralPath $lockPath)) {
                if ($journal.phase -eq 'lease-completed') { $journal.phase = 'shared-released' }
                elseif ($journal.sharedLock) { Acquire-LcSharedLock $journal.leaseRunId }
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
                        # The lease was completed only after restoration and verification: cleanup only, with the lease's verdict.
                        $journal.phase = 'lease-completed'
                        if ($journal.registryRestore -in @('pending', 'restoring')) { $journal.registryRestore = $(if ($state.PSObject.Properties['registryRestore']) { [string]$state.registryRestore } else { 'applied' }) }
                        $journal.integrity = $(if ($state.PSObject.Properties['integrity']) { $state.integrity } else { [ordered]@{ status = 'unavailable'; reason = 'the completed lease records no owner-file comparison' } })
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
        $cleanupOnly = $journal.phase -in @('lease-completed', 'shared-released', 'private-released')
        # A transaction not yet committed is verified afresh: files may have changed while it was down.
        if (-not $cleanupOnly) { $journal.integrity = $null }
        $result = Complete-LcTransaction $journal $lease $Restore 0 $RegistryResolution
        $ok = Test-LcTransactionSucceeded $result
        $text = "Recovery of $($journal.sessionId): outcome $($result.Outcome); game settings $($result.Registry); owner files $(if ($null -ne $result.Integrity) { Get-LcValue $result.Integrity 'status' } else { 'not verified' })$(if ($cleanupOnly) { ' (cleanup only)' })$(if ($result.Reason) { '; ' + $result.Reason })."
        return New-LcRecoveryResult $text $ok $result
    } finally {
        if ($null -ne $stream) { $stream.Dispose() }
        $guard.Dispose()
    }
}

# The -RecoverOnly command: its exit code is the recovery verdict (refusals included).
function Invoke-LcRecoverOnlyCommand([scriptblock]$Restore, [string]$RegistryResolution = '') {
    try { $recovery = Invoke-LcRecovery $Restore $RegistryResolution }
    catch { Write-Host "Recovery refused: $($_.Exception.Message)"; return 1 }
    Write-Host $recovery.Message
    if ($recovery.Succeeded) { return 0 } else { return 1 }
}
