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
# journaled before the first import attempt and retained until the settings are
# settled, the backup's verified bytes are held for comparison and import, and
# the host is re-checked immediately before every import. Completion and
# -RecoverOnly share Complete-LcTransaction; an inconclusive registry state keeps
# the journal, the locks and a RecoveryRequired lease until it is restored or the
# owner resolves that exact transaction explicitly
# (-ResolveTransaction <id> -RegistryResolution Import|KeepCurrent).
# ---------------------------------------------------------------------------

# Fixture-only hooks. In every live run these hold the real operations or $null.
$script:LcInterruptAfter = $null          # boundary at which to model process death
$script:LcRecoveryPauseHook = $null       # runs once after recovery's first journal read
$script:LcRecoverySnapshotHook = $null    # runs once after the journal snapshot read under the recovery guard
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
    if ($null -ne $raw -and $raw.PSObject.Properties['transactionId'] -and $raw.transactionId) { return [string]$raw.transactionId }
    return '<legacy>'
}

# One read of the journal: its identity (the transaction id, or the digest of an
# older journal's bytes), the parsed record and the bytes' digest all come from
# the same bytes, so the identity checked is the identity of the record acted on.
# Returns $null when no journal exists.
function Read-LcJournalSnapshot {
    try { $bytes = [IO.File]::ReadAllBytes($script:LcJournal) }
    catch [IO.FileNotFoundException] { return $null }
    catch [IO.DirectoryNotFoundException] { return $null }
    $digest = Get-LcBytesSha256 $bytes
    $raw = $null
    try { $raw = (Get-LcTextFromBytes $bytes) | ConvertFrom-Json } catch { $raw = $null }
    $identity = if ($null -ne $raw -and $raw.PSObject.Properties['transactionId'] -and $raw.transactionId) { [string]$raw.transactionId } else { 'file-sha256:' + $digest }
    return [pscustomobject]@{ Identity = $identity; Raw = $raw; Sha256 = $digest }
}

# The journal on disk is this transaction's: the same transaction id, or - for an
# older journal not yet rewritten in schema 4 - exactly the bytes it was converted from.
function Test-LcJournalOwned($Journal) {
    $current = Get-LcJournalTransactionId
    if ($current -ceq [string]$Journal.transactionId) { return $true }
    if ($current -ceq '<legacy>' -and $Journal.Contains('sourceSha256') -and $Journal.sourceSha256) {
        return (Get-LcSha256 $script:LcJournal) -ceq [string]$Journal.sourceSha256
    }
    return $false
}

# The owner's registry decision, normalized the way the runner's ValidateSet
# accepts it (case-insensitively) so every accepted spelling has one meaning.
function ConvertTo-LcResolution([string]$Value) {
    if (-not $Value) { return '' }
    foreach ($name in @('Import', 'KeepCurrent')) { if ([string]::Equals($Value, $name, [StringComparison]::OrdinalIgnoreCase)) { return $name } }
    throw "Unknown registry resolution '$Value' (use Import or KeepCurrent); nothing was changed."
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
function Get-LcTextFromBytes([byte[]]$Bytes) {
    $reader = New-Object IO.StreamReader((New-Object IO.MemoryStream(, $Bytes)), (New-Object Text.UTF8Encoding($false)), $true)
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

function Get-LcRegistryCanonical([string]$Path) { return Get-LcRegistryCanonicalText (Get-LcTextFromBytes ([IO.File]::ReadAllBytes($Path))) }

# Ordinal equality of two canonical exports (never linguistic: distinct UTF-16 data must differ).
function Test-LcRegistryEqual([string]$Left, [string]$Right) { return [string]::Equals($Left, $Right, [StringComparison]::Ordinal) }

function Get-LcRegistryCanonicalText([string]$Text) {
    $sections = [Collections.Generic.SortedDictionary[string, Collections.Generic.List[string]]]::new([StringComparer]::Ordinal)
    $header = [Collections.Generic.List[string]]::new()
    $key = $null; $pending = $null
    foreach ($raw in ($Text -split "`r?`n")) {
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
        $values = $entry.Value.ToArray()
        [Array]::Sort($values, [StringComparer]::Ordinal)
        $out.AddRange([string[]]$values)
    }
    return ($out -join "`n")
}

function Get-LcBytesSha256([byte[]]$Bytes) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return (-join ($sha.ComputeHash($Bytes) | ForEach-Object { $_.ToString('x2') })) } finally { $sha.Dispose() }
}

$script:LcBackupVerifyHook = $null   # fixture-only: runs during the restore-source verification

# The restore source, held stable: the backup's bytes are read once and checked
# against the intake digest (when one exists), written to a private verified copy,
# and that copy is opened with a read handle that denies writes and deletion. The
# comparison uses those bytes and the import uses that copy, so neither can see
# bytes other than the ones verified. Returns $null when the bytes fail their digest.
function Open-LcVerifiedBackup($Journal) {
    $bytes = [IO.File]::ReadAllBytes($Journal.registryBackup)
    if ($null -ne $script:LcBackupVerifyHook) { & $script:LcBackupVerifyHook }
    $digest = Get-LcValue $Journal 'registryBackupSha256'
    $actual = Get-LcBytesSha256 $bytes
    if ($digest -and $actual -cne $digest) { return $null }
    $copy = Join-Path (Get-LcTransactionDir $Journal) ('registry-backup-verified-' + [Guid]::NewGuid().ToString('N') + '.reg')
    [IO.File]::WriteAllBytes($copy, $bytes)
    $handle = [IO.File]::Open($copy, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    $held = New-Object byte[] $handle.Length
    $offset = 0
    while ($offset -lt $held.Length) { $read = $handle.Read($held, $offset, $held.Length - $offset); if ($read -le 0) { break }; $offset += $read }
    if ((Get-LcBytesSha256 $held) -cne $actual) { $handle.Dispose(); return $null }
    return [pscustomobject]@{ Path = $copy; Handle = $handle; Canonical = (Get-LcRegistryCanonicalText (Get-LcTextFromBytes $bytes)); Sha256 = $actual }
}

# After an interruption during restoration, the live settings are compared with
# the verified backup (already applied) and with the snapshot journaled just
# before the import (not applied). A missing key means the delete ran and the
# import did not (not applied; nothing newer can be lost). An unreadable key is
# 'unreadable'; anything else 'unresolved'.
function Resolve-LcInterruptedRestore($Journal, $Verified) {
    $probe = Join-Path (Get-LcTransactionDir $Journal) ('registry-reconcile-' + [Guid]::NewGuid().ToString('N') + '.reg')
    try { $status = & $script:LcRegistryExport $probe } catch { return "unreadable: $($_.Exception.Message)" }
    if ($status -ceq 'absent') { return 'not-applied' }
    $current = Get-LcRegistryCanonical $probe
    if (Test-LcRegistryEqual $current $Verified.Canonical) { return 'applied' }
    $pre = $Journal.preRestore
    if ($null -ne $pre -and (Get-LcValue $pre 'state') -ceq 'exported' -and (Test-Path -LiteralPath $pre.path)) {
        $preBytes = [IO.File]::ReadAllBytes($pre.path)
        if ((Get-LcBytesSha256 $preBytes) -ceq $pre.sha256 -and (Test-LcRegistryEqual $current (Get-LcRegistryCanonicalText (Get-LcTextFromBytes $preBytes)))) { return 'not-applied' }
    }
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

# The owner's decision is bound to this exact transaction.
function Get-LcResolutionHint($Journal) {
    return "To decide, run Invoke-LoaderCompatSession.ps1 -RecoverOnly -ResolveTransaction $($Journal.transactionId) -RegistryResolution Import (import the verified session backup) or KeepCurrent (keep the current settings)."
}

# The registry step. Returns $null when the game settings are settled, or a
# deferred result that keeps the journal, the locks and a RecoveryRequired lease.
function Invoke-LcRegistryStep($Journal, $Lease, [scriptblock]$Restore, [string]$Resolution) {
    $Resolution = ConvertTo-LcResolution $Resolution
    if ($Journal.registryRestore -in @('applied', 'not-needed', 'owner-kept-current')) { return $null }
    if (-not $Journal.registryBackup) {
        $Journal.registryRestore = 'not-needed'; $Journal.phase = 'restored'; Save-LcJournal $Journal; Step-LcBoundary 'restored'
        return $null
    }
    $settle = { param($Value) $Journal.registryRestore = $Value; $Journal.phase = 'restored'; Save-LcJournal $Journal; Step-LcBoundary 'restored' }
    if ($Resolution -ceq 'KeepCurrent') { & $settle 'owner-kept-current'; return $null }
    $hint = Get-LcResolutionHint $Journal
    $digest = Get-LcValue $Journal 'registryBackupSha256'
    if (-not $digest -and $Resolution -cne 'Import') {
        return Set-LcDeferred $Journal $Lease "This journal records no digest for its registry backup (written by an earlier runner); the backup is neither compared nor imported without the owner's decision. $hint"
    }
    if (-not (Test-Path -LiteralPath $Journal.registryBackup -PathType Leaf)) {
        return Set-LcDeferred $Journal $Lease "The registry backup is missing ($($Journal.registryBackup)); nothing was imported. $hint"
    }
    $verified = Open-LcVerifiedBackup $Journal
    if ($null -eq $verified) {
        return Set-LcDeferred $Journal $Lease "The registry backup no longer matches the digest recorded at intake; it was neither compared nor imported. Only KeepCurrent can settle a changed backup. $hint"
    }
    try {
        if ($digest -and $Journal.registryRestore -ceq 'restoring') {
            $decision = Resolve-LcInterruptedRestore $Journal $verified
            if ($decision -ceq 'applied') { & $settle 'applied'; return $null }
            if ($decision -like 'unreadable*') { return Set-LcDeferred $Journal $Lease "The live game settings could not be read ($decision); nothing was imported and ownership is retained for another -RecoverOnly." }
            if ($decision -like 'unresolved*' -and $Resolution -cne 'Import') { return Set-LcDeferred $Journal $Lease "$decision. Nothing was imported. $hint" }
        } elseif ($digest) {
            # First import attempt: snapshot the live settings. From here the journal stays 'restoring' with this
            # snapshot retained, so a withheld or interrupted import is later reconciled against it, never re-snapshotted.
            $pre = Join-Path (Get-LcTransactionDir $Journal) ('registry-before-restore-' + [Guid]::NewGuid().ToString('N') + '.reg')
            try { $status = & $script:LcRegistryExport $pre } catch { return Set-LcDeferred $Journal $Lease "The live game settings could not be read before restoration ($($_.Exception.Message)); nothing was imported." }
            $Journal.preRestore = [ordered]@{ path = $pre; state = $status; sha256 = $(if ($status -ceq 'exported') { Get-LcSha256 $pre } else { $null }) }
            $Journal.registryRestore = 'restoring'; $Journal.phase = 'restoring'; Save-LcJournal $Journal
            Step-LcBoundary 'restoring'
        }
        if ($Journal.registryRestore -cne 'restoring') { $Journal.registryRestore = 'restoring'; $Journal.phase = 'restoring'; Save-LcJournal $Journal }
        # The last host check follows every slow read and immediately precedes the destructive delete-and-import.
        $conflicts = Get-LcHostConflicts $Journal.leaseRunId
        if ($conflicts.Count -gt 0) {
            return Set-LcDeferred $Journal $Lease "The import was withheld because the host became busy ($($conflicts -join '; ')); game settings were not changed, the pre-restore snapshot is retained and ownership is kept."
        }
        try { & $Restore $verified.Path }
        catch { return Set-LcDeferred $Journal $Lease "Game-settings restoration failed: $($_.Exception.Message). Recovery reconciles it against the verified backup and the pre-restore snapshot." }
        Step-LcBoundary 'import-applied'
        & $settle 'applied'
        return $null
    } finally {
        $verified.Handle.Dispose()
        Remove-Item -LiteralPath $verified.Path -Force -ErrorAction SilentlyContinue
    }
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
        # The schema-3 runner could mark a transaction completed (and even release
        # its locks) with an inconclusive restore ('unresolved: ...'). Only a settled
        # outcome is honoured; anything else stays outstanding for the owner.
        $registry = [string]$journal['registryRestore']
        if ($registry -cnotin @('applied', 'not-needed')) {
            $journal['convertedRegistryRestore'] = $registry
            if ($registry -cne 'pending') { $journal['registryRestore'] = 'restoring' }
            if ($journal['phase'] -in @('restored', 'verified', 'lease-completed', 'shared-released', 'private-released')) {
                $journal['phase'] = 'recovery-required'; $journal['integrity'] = $null
            }
        }
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

# -RecoverOnly. The journal identity read first must still be the identity of the
# single journal read under the recovery guard, and that same read is the record
# acted on (a newer session is left for its own recovery). An owner decision is
# honoured only for the transaction it names. Then the original owner must have
# exited, the host must be free, and the shared lock - re-taken under the
# journaled identity if missing, or under a fresh identity journaled first for an
# unleased older journal - must be held through this caller's exclusive handle
# with a durable lease record, the journal still the same transaction, before any
# registry work. The owner-file comparison is refreshed for any transaction not
# yet committed. The command's verdict is the recorded transaction verdict,
# cleanup-only completions and completions by another caller included.
function Invoke-LcRecovery([scriptblock]$Restore, [string]$RegistryResolution = '', [string]$ResolveTransaction = '') {
    $RegistryResolution = ConvertTo-LcResolution $RegistryResolution
    if ($ResolveTransaction -and -not $RegistryResolution) { throw '-ResolveTransaction names the transaction an owner decision applies to and needs -RegistryResolution Import or KeepCurrent; nothing was changed.' }
    if ($RegistryResolution -and -not $ResolveTransaction) {
        throw "-RegistryResolution $RegistryResolution must name the interrupted transaction it decides (-ResolveTransaction, as printed with the deferral); nothing was changed."
    }
    $first = Read-LcJournalSnapshot
    if ($null -eq $first) {
        if ($RegistryResolution) { return New-LcRecoveryResult "No interrupted session journal: transaction $ResolveTransaction is not awaiting a decision and nothing was changed." $false $null }
        return New-LcRecoveryResult 'No interrupted session journal.' $true $null
    }
    $observed = $first.Identity
    if ($null -ne $script:LcRecoveryPauseHook) { $hook = $script:LcRecoveryPauseHook; $script:LcRecoveryPauseHook = $null; & $hook }
    $guard = Enter-LcRecoveryGuard
    $stream = $null
    try {
        # One read under the guard: the identity compared and the record acted on come from the same bytes.
        $snapshot = Read-LcJournalSnapshot
        if ($null -eq $snapshot) { return Get-LcObservedVerdict $observed $first.Raw }
        if ($snapshot.Identity -cne $observed) {
            throw "The session journal now belongs to a different transaction ($($snapshot.Identity)) than the one this recovery read ($observed); nothing was changed. Run -RecoverOnly again for that session."
        }
        if ($null -eq $snapshot.Raw) { throw "The session journal cannot be parsed ($observed); nothing was changed." }
        if ($RegistryResolution -and $snapshot.Identity -cne $ResolveTransaction) {
            throw "The owner's decision names transaction $ResolveTransaction, but the interrupted session journal is transaction $($snapshot.Identity); nothing was changed."
        }
        if ($null -ne $script:LcRecoverySnapshotHook) { $hook = $script:LcRecoverySnapshotHook; $script:LcRecoverySnapshotHook = $null; & $hook }
        $raw = $snapshot.Raw
        $isCurrent = $raw.PSObject.Properties['schemaVersion'] -and [int]$raw.schemaVersion -eq 4
        $journal = if ($isCurrent) { ConvertTo-LcOrdered $raw } else { Convert-LcLegacyJournal $raw }
        if (-not $isCurrent) { $journal['sourceSha256'] = $snapshot.Sha256 }
        if (Test-LcOwnerAlive $journal) { throw "The session owner (pid $($journal.ownerProcessId)) is still alive; nothing was changed." }
        $conflicts = Get-LcHostConflicts $journal.leaseRunId
        if ($conflicts.Count -gt 0) { throw "Recovery withheld: $($conflicts -join '; '). Nothing was changed." }
        $lockPath = Get-LcSharedLockPath
        $lease = $null
        if ($journal.phase -eq 'lease-completed' -and -not (Test-Path -LiteralPath $lockPath)) { $journal.phase = 'shared-released' }
        if ($journal.phase -notin @('shared-released', 'private-released')) {
            # Every recovery that still has work holds this session's shared lock through its own handle and a
            # durable lease record before it acts. The lease identity is journaled first (its intent); an
            # acquisition interrupted after that is completed under the same identity on the next attempt.
            if (-not $journal.leaseRunId) {
                $journal.leaseRunId = New-LcLeaseRunId; $journal.sharedLock = $false
                $journal.leaseStatePath = Join-Path (Join-Path $script:LcStateRoot $journal.leaseRunId) 'runtime-lease.json'
                Save-LcJournal $journal
                Step-LcBoundary 'recovery-lease-intended'
            }
            if (-not (Test-Path -LiteralPath $lockPath)) {
                if (-not (Test-LcJournalOwned $journal)) { throw 'The session journal changed while recovery took ownership; nothing was changed.' }
                Acquire-LcSharedLock $journal.leaseRunId
                Step-LcBoundary 'recovery-lock-acquired'
            }
            try { $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read) }
            catch [IO.IOException] { throw "Another caller holds this session's recovery; nothing was changed." }
            if ((Get-Content -LiteralPath $lockPath -Raw).Trim() -cne $journal.leaseRunId) { throw 'The shared lock is not held by this session; nothing was changed.' }
            if (-not $journal.sharedLock -or -not $journal.leaseStatePath) {
                $journal.sharedLock = $true
                if (-not $journal.leaseStatePath) { $journal.leaseStatePath = Join-Path (Join-Path $script:LcStateRoot $journal.leaseRunId) 'runtime-lease.json' }
                Save-LcJournal $journal
            }
            $state = $null; $publish = $false
            if (Test-Path -LiteralPath $journal.leaseStatePath) {
                $state = Read-KmgCompatibilityJson $journal.leaseStatePath
                if ([string]$state.runId -cne $journal.leaseRunId -or [string]$state.purpose -cne ('loader-compat ' + $journal.sessionId)) {
                    throw 'The recorded lease does not belong to this session; nothing was changed.'
                }
                if ($state.status -ceq 'Completed' -and $journal.phase -ne 'lease-completed' -and $journal.registryRestore -in @('applied', 'not-needed', 'owner-kept-current')) {
                    # Completed only after the settings were settled and the files compared: cleanup only, with the lease's verdict.
                    $journal.phase = 'lease-completed'
                    $journal.integrity = $(if ($state.PSObject.Properties['integrity']) { $state.integrity } else { [ordered]@{ status = 'unavailable'; reason = 'the completed lease records no owner-file comparison' } })
                    $state = $null
                }
            } elseif ($journal.phase -ne 'lease-completed') {
                # No durable lease record (interrupted before it was written, or an older journal): this recovery publishes one.
                [void][IO.Directory]::CreateDirectory((Split-Path -Parent $journal.leaseStatePath))
                $state = [ordered]@{ schemaVersion = 1; runId = $journal.leaseRunId; purpose = ('loader-compat ' + $journal.sessionId); processId = $PID
                    processStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o'); status = 'Active'; recoveryRequired = $false
                    reason = 'Recovery of an interrupted loader-compat session'; settings = $null; transactionId = $journal.transactionId }
                $publish = $true
            }
            $lease = [pscustomobject]@{ RunId = $journal.leaseRunId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
                StatePath = $journal.leaseStatePath; StateSha256 = $(if ($null -ne $state -and -not $publish) { Get-KmgCompatibilitySha256 $journal.leaseStatePath } else { $null })
                Compatibility = $false; State = $state }
            if ($publish) { & $script:LcLeaseStateWriter $lease; Step-LcBoundary 'recovery-lease-written' }
        }
        # Under full ownership the journal must still be this transaction before anything is written.
        if (-not (Test-LcJournalOwned $journal)) { throw 'The session journal changed while recovery took ownership; nothing was changed.' }
        $cleanupOnly = $journal.phase -in @('lease-completed', 'shared-released', 'private-released')
        if (-not $cleanupOnly -and ($null -eq $lease -or $null -eq $lease.Stream -or $null -eq $lease.State)) {
            throw 'Recovery does not hold this session''s shared lock and lease record; nothing was changed.'
        }
        Remove-LcJournalTemps
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

# A caller whose observed journal was completed by another caller while it waited
# reports that transaction's durable verdict from its lease record, or failure
# when no such verdict can be read for the transaction it observed.
function Get-LcObservedVerdict([string]$Observed, $Raw) {
    $prefix = "The interrupted session ($Observed) was already recovered by another caller; nothing was changed by this caller"
    $runId = [string](Get-LcValue $Raw 'leaseRunId'); $path = [string](Get-LcValue $Raw 'leaseStatePath')
    if (-not $runId -or -not $path -or -not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return New-LcRecoveryResult "$prefix. Its verdict cannot be verified: the observed journal names no readable lease record." $false $null
    }
    try { $state = Read-KmgCompatibilityJson $path } catch { return New-LcRecoveryResult "$prefix. Its verdict cannot be verified: the lease record is unreadable ($($_.Exception.Message))." $false $null }
    $stateTx = [string](Get-LcValue $state 'transactionId'); $rawTx = [string](Get-LcValue $Raw 'transactionId')
    if ([string]$state.runId -cne $runId -or ($stateTx -and $rawTx -and $stateTx -cne $rawTx) -or [string]$state.status -cne 'Completed') {
        return New-LcRecoveryResult "$prefix. Its verdict cannot be verified: the lease record does not show this transaction completed (status $($state.status))." $false $null
    }
    $result = [pscustomobject]@{ Outcome = 'completed'; Reason = $null; Registry = [string](Get-LcValue $state 'registryRestore'); Integrity = (Get-LcValue $state 'integrity') }
    $ok = Test-LcTransactionSucceeded $result
    return New-LcRecoveryResult "$prefix. Recorded verdict: game settings $($result.Registry); owner files $(if ($null -ne $result.Integrity) { Get-LcValue $result.Integrity 'status' } else { 'not recorded' })." $ok $result
}

# The -RecoverOnly command: its exit code is the recovery verdict (refusals included).
function Invoke-LcRecoverOnlyCommand([scriptblock]$Restore, [string]$RegistryResolution = '', [string]$ResolveTransaction = '') {
    try { $recovery = Invoke-LcRecovery $Restore $RegistryResolution $ResolveTransaction }
    catch { Write-Host "Recovery refused: $($_.Exception.Message)"; return 1 }
    Write-Host $recovery.Message
    if ($recovery.Succeeded) { return 0 } else { return 1 }
}
