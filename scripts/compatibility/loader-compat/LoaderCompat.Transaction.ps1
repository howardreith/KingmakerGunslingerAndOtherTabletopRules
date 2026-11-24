Set-StrictMode -Version Latest
# ---------------------------------------------------------------------------
# Loader-compat session transaction, dot-sourced by LoaderCompat.Common.ps1.
#
# Supported contract: one NEW, exclusively owned session followed by its normal,
# verified completion. Nothing here recovers, replays, converts or takes over an
# earlier session. A Kingmaker process, any configured runtime or game-copy lock
# and any pending journal are refused before anything changes.
#
# The journal ($script:LcJournal) is the session's durable bookkeeping: its
# transaction id, the lease run id named by the shared compatibility.lock, the
# private loader-compat.lock claim, the registry backup with the digest taken at
# intake, the owner-file intake and the current phase. Every claim is written to
# a private file and published with an atomic no-overwrite rename.
#
# Any host conflict the session observes - a foreign Kingmaker process, a foreign
# lab lock or a lost claim, at a launch or at completion - is recorded in the
# journal (hostConflict) and is itself a retain decision: a peer that has exited
# has stopped writing, but that does not make its changes safe to overwrite, so
# the session never restores automatically after seeing one.
#
# Completion, with no conflict ever observed: hold a verified copy of the backup (one
# read, checked against the intake digest, written to a private copy kept open
# with a handle that denies writes and deletion), re-check the host immediately
# before the registry delete-and-import, import that copy, read the key back and
# compare it ordinally with the same verified bytes, verify the owner files
# against the intake, and only then complete the lease and release this
# session's claims. Any failure returns failure and RETAINS the journal, both
# locks, the backup and a RecoveryRequired lease for deliberate manual recovery
# (docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md, "Manual recovery"); every later
# session refuses that retained state.
# ---------------------------------------------------------------------------

# External boundaries, substituted only by the offline fixtures.
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
$script:LcBackupVerifyHook = $null   # fixture-only: runs inside the backup verification, after its single read

$script:LcManualRecovery = 'See "Manual recovery" in docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md; this runner never recovers, replays or takes over earlier state.'

function Get-LcSharedLockPath { return Join-Path $script:LcStateRoot 'compatibility.lock' }
function New-LcLeaseRunId { return 'runtime-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N') }
function Test-LcPrivateLockOwned([string]$SessionId) { $info = Get-LcPrivateLockInfo; return $null -ne $info -and $info['session'] -ceq $SessionId }
function Get-LcTransactionDir($Journal) {
    $directory = Split-Path -Parent $Journal.leaseStatePath
    [void][IO.Directory]::CreateDirectory($directory)
    return $directory
}
function Read-LcLockText([string]$Path) { if (Test-Path -LiteralPath $Path -PathType Leaf) { return (Get-Content -LiteralPath $Path -Raw).Trim() } else { return $null } }

# Writes a claim completely to a private file, then publishes it with an atomic
# rename that fails if the claim already exists: the claim is absent or complete.
function Publish-LcClaim([string]$Path, [string]$Content, [string]$ExistsMessage) {
    $directory = Split-Path -Parent $Path
    [void][IO.Directory]::CreateDirectory($directory)
    $temp = Join-Path $directory ('.' + (Split-Path -Leaf $Path) + '.' + [Guid]::NewGuid().ToString('N') + '.claim')
    $bytes = (New-Object Text.UTF8Encoding($false)).GetBytes($Content)
    $stream = [IO.File]::Open($temp, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
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
function Assert-LcSessionInputs([string]$CandidateZip, [string]$HarnessDir, [string]$DevelopmentPurpose, [switch]$HistoricalReleaseControl, [string]$HistoricalReleaseVersion) {
    if (-not $CandidateZip -or -not (Test-Path -LiteralPath $CandidateZip -PathType Leaf)) { throw "Candidate ZIP not found: $CandidateZip. Nothing was acquired." }
    # Cheap prerequisites first: the harness files.
    foreach ($name in @('KmgLoaderCompatHarness.dll', 'Info.json')) {
        if (-not (Test-Path -LiteralPath (Join-Path $HarnessDir $name) -PathType Leaf)) { throw "Harness file missing: $(Join-Path $HarnessDir $name). Nothing was acquired." }
    }
    # Admission modes are mutually exclusive; a development purpose is validated before any child process starts.
    if ($HistoricalReleaseControl -and -not [string]::IsNullOrEmpty($DevelopmentPurpose)) { throw '-HistoricalReleaseControl and -CandidateDevelopmentPurpose are mutually exclusive. Nothing was acquired.' }
    if ([bool]$HistoricalReleaseControl -ne (-not [string]::IsNullOrEmpty($HistoricalReleaseVersion))) { throw '-HistoricalReleaseControl requires -HistoricalReleaseVersion, and the version is valid only with it. Nothing was acquired.' }
    $repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
    if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) {
        . (Join-Path $repository 'scripts\DevelopmentCandidate.Common.ps1')
        [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
    }
    # The candidate is admitted exactly as Deploy-Local admits it (or as the recorded historical release), in its own
    # process with every value passed as a bound parameter, and a private copy with the admitted digest is what stages.
    $copyDirectory = Join-Path ([IO.Path]::GetTempPath()) ('kmg-lc-admitted-' + [Guid]::NewGuid().ToString('N'))
    $arguments = @('-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $PSScriptRoot 'Invoke-LcCandidateAdmission.ps1'),
        '-PackagePath', $CandidateZip, '-AdmittedCopyDirectory', $copyDirectory)
    if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) { $arguments += @('-DevelopmentPurpose', $DevelopmentPurpose) }
    if ($HistoricalReleaseControl) { $arguments += @('-HistoricalReleaseVersion', $HistoricalReleaseVersion) }
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $admissionOutput = @(& powershell.exe @arguments 2>&1 | ForEach-Object { "$_" }) }
    finally { $ErrorActionPreference = $previous }
    if ($LASTEXITCODE -ne 0) {
        if (Test-Path -LiteralPath $copyDirectory) { Remove-Item -LiteralPath $copyDirectory -Recurse -Force }
        throw "Candidate admission refused $CandidateZip; nothing was acquired: $(($admissionOutput | Select-Object -Last 3) -join ' ')"
    }
    return (($admissionOutput | Select-Object -Last 1) | ConvertFrom-Json)
}

# --- admission -------------------------------------------------------------------

# Every piece of pre-existing state a new session refuses: any Kingmaker process,
# any configured runtime lock (the shared compatibility.lock included), the
# game-copy lock and a pending journal (current, older or unreadable alike).
function Get-LcPreexistingState {
    $found = [Collections.Generic.List[string]]::new()
    $games = Get-LcGameProcesses
    if ($games.Count -gt 0) { $found.Add("Kingmaker is running (pid $((@($games | ForEach-Object { $_.Id })) -join ','))") }
    foreach ($lock in $script:LcForeignLocks) { if (Test-Path -LiteralPath $lock) { $found.Add("a runtime lock exists: $lock") } }
    if (Test-Path -LiteralPath $script:LcLock) {
        $info = Get-LcPrivateLockInfo
        $found.Add("the game-copy lock exists: $script:LcLock (session $(if ($null -ne $info -and $info['session']) { $info['session'] } else { '<unreadable>' }))")
    }
    if (Test-Path -LiteralPath $script:LcJournal) { $found.Add("a pending session journal exists: $script:LcJournal (transaction $(Get-LcJournalTransactionId))") }
    return , $found.ToArray()
}

function Assert-LcHostFree {
    $found = Get-LcPreexistingState
    if ($found.Count -gt 0) { throw "Refusing to start a session; nothing was changed. Pre-existing state: $($found -join '; '). $script:LcManualRecovery" }
}

# What stops this session from launching or restoring now: any Kingmaker process,
# or any configured lock other than this session's own shared lock.
function Get-LcHostConflicts([string]$LeaseRunId) {
    $conflicts = [Collections.Generic.List[string]]::new()
    $games = Get-LcGameProcesses
    if ($games.Count -gt 0) { $conflicts.Add("Kingmaker is running (pid $((@($games | ForEach-Object { $_.Id })) -join ','))") }
    $shared = [IO.Path]::GetFullPath((Get-LcSharedLockPath))
    foreach ($lock in $script:LcForeignLocks) {
        if (-not (Test-Path -LiteralPath $lock)) { continue }
        $own = $LeaseRunId -and [IO.Path]::GetFullPath($lock).Equals($shared, [StringComparison]::OrdinalIgnoreCase) -and (Read-LcLockText $lock) -ceq $LeaseRunId
        if (-not $own) { $conflicts.Add("another lab holds a runtime lock: $lock") }
    }
    return , $conflicts.ToArray()
}

# Records the first host conflict this session observes, in memory and (when the
# journal is still this session's) durably. Once recorded, the session is retained
# at completion and never restores automatically, even after the peer has gone.
function Register-LcHostConflict($Journal, [string[]]$Conflicts, [string]$Where) {
    if ($null -ne $Journal.hostConflict) { return }
    $Journal.hostConflict = [ordered]@{ observedUtc = [DateTime]::UtcNow.ToString('o'); at = $Where; conflicts = @($Conflicts) }
    try { Save-LcJournal $Journal } catch { }
}

# This session still owns its journal, its shared lock and its game-copy lock.
function Get-LcClaimProblems($Journal) {
    $problems = [Collections.Generic.List[string]]::new()
    if (-not (Test-LcJournalOwned $Journal)) { $problems.Add("the session journal is missing or belongs to another transaction") }
    if ((Read-LcLockText (Get-LcSharedLockPath)) -cne [string]$Journal.leaseRunId) { $problems.Add("the shared compatibility.lock no longer names this session's lease") }
    if (-not (Test-LcPrivateLockOwned $Journal.sessionId)) { $problems.Add("the game-copy lock no longer names this session") }
    return , $problems.ToArray()
}

# Immediately before every fresh launch: the host is free, the lease is held, the
# claims are this session's and the registry backup is bound to its intake digest
# (so nothing can launch that completion could not restore).
function Assert-LcLaunchAvailable($Lease, $Journal) {
    $conflicts = Get-LcHostConflicts $(if ($null -ne $Lease) { $Lease.RunId } else { $null })
    if ($conflicts.Count -gt 0) { Register-LcHostConflict $Journal $conflicts 'launch'; throw "Launch withheld: $($conflicts -join '; ')." }
    Assert-KmgRuntimeLease $Lease
    $problems = Get-LcClaimProblems $Journal
    if ($problems.Count -gt 0) { Register-LcHostConflict $Journal $problems 'launch'; throw "Launch withheld: $($problems -join '; ')." }
    if (-not $Journal.registryBackup -or -not $Journal.registryBackupSha256) { throw 'Launch withheld: no registry backup bound to an intake digest has been recorded.' }
}

# --- journal ---------------------------------------------------------------------

function ConvertTo-LcJournalText($Journal) { return ($Journal | ConvertTo-Json -Depth 12) }

function Get-LcJournalTransactionId {
    try { $raw = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json } catch { return '<unreadable>' }
    if ($null -ne $raw -and $raw.PSObject.Properties['transactionId'] -and $raw.transactionId) { return [string]$raw.transactionId }
    return '<none>'
}

function Test-LcJournalOwned($Journal) {
    return (Test-Path -LiteralPath $script:LcJournal -PathType Leaf) -and (Get-LcJournalTransactionId) -ceq [string]$Journal.transactionId
}

# Every update writes a complete temporary journal and then replaces the file
# atomically, and only while the file on disk is still this transaction's.
function Save-LcJournal($Journal) {
    if (-not (Test-LcJournalOwned $Journal)) { throw "The session journal is missing or belongs to another transaction; it was not replaced." }
    $temp = $script:LcJournal + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
    $bytes = (New-Object Text.UTF8Encoding($false)).GetBytes((ConvertTo-LcJournalText $Journal))
    $stream = [IO.File]::Open($temp, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
    [IO.File]::Replace($temp, $script:LcJournal, [Management.Automation.Language.NullString]::Value)
}

function Remove-LcJournal($Journal) {
    if (-not (Test-LcJournalOwned $Journal)) { throw "The session journal is missing or belongs to another transaction; it was not removed." }
    Remove-Item -LiteralPath $script:LcJournal -Force
}

# --- acquisition -----------------------------------------------------------------

# Starts a new session transaction after refusing any pre-existing state. The
# journal is published complete and already names the lease identity the shared
# lock will carry. A failure here releases exactly what this call acquired; if
# that cannot finish, the remaining claims are reported for manual recovery.
function Enter-LcSessionTransaction([string]$SessionId) {
    Assert-LcHostFree
    $runId = New-LcLeaseRunId
    $leaseDir = Join-Path $script:LcStateRoot $runId
    $journal = [ordered]@{ schemaVersion = 5; contract = 'new-session-only'; transactionId = [Guid]::NewGuid().ToString('N'); sessionId = $SessionId
        phase = 'acquiring'; ownerProcessId = $PID; ownerProcessStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
        leaseRunId = $runId; leaseStatePath = (Join-Path $leaseDir 'runtime-lease.json'); sharedLock = $false; privateLock = $false
        registryBackup = $null; registryBackupSha256 = $null; registryRestore = 'pending'; registryVerification = $null
        hostConflict = $null; intake = $null; integrity = $null; reason = $null }
    Publish-LcClaim $script:LcJournal (ConvertTo-LcJournalText $journal) "A session journal already exists ($script:LcJournal); nothing was acquired. $script:LcManualRecovery"
    $stream = $null; $lease = $null
    try {
        Acquire-LcSharedLock $runId
        $journal.sharedLock = $true; Save-LcJournal $journal
        $lockPath = Get-LcSharedLockPath
        $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
        [void][IO.Directory]::CreateDirectory($leaseDir)
        $lease = [pscustomobject]@{ RunId = $runId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
            StatePath = $journal.leaseStatePath; Compatibility = $false
            State = [ordered]@{ schemaVersion = 1; runId = $runId; purpose = ('loader-compat ' + $SessionId); processId = $PID
                processStartedUtc = $journal.ownerProcessStartedUtc; status = 'Active'; recoveryRequired = $false; reason = $null
                settings = $null; transactionId = $journal.transactionId } }
        & $script:LcLeaseStateWriter $lease
        Enter-LcLock $SessionId
        $journal.privateLock = $true; $journal.phase = 'acquired'; Save-LcJournal $journal
    } catch {
        $failure = $_
        try {
            if ($null -ne $stream) { $stream.Dispose() }
            if (Test-LcPrivateLockOwned $SessionId) { Exit-LcLock $SessionId }
            $lockPath = Get-LcSharedLockPath
            if ((Read-LcLockText $lockPath) -ceq $runId) {
                if (Test-Path -LiteralPath $journal.leaseStatePath) {
                    $state = Get-Content -LiteralPath $journal.leaseStatePath -Raw | ConvertFrom-Json
                    $state.status = 'Completed'; $state.reason = 'Loader-compat session did not start: ' + $failure.Exception.Message
                    [IO.File]::WriteAllText($journal.leaseStatePath, ($state | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
                }
                Remove-KmgCompatibilityOwnedLock $lockPath $runId
            }
            Remove-LcJournal $journal
        } catch {
            throw "Session start failed ($($failure.Exception.Message)) and releasing its own claims could not finish ($($_.Exception.Message)); journal $script:LcJournal (transaction $($journal.transactionId), lease $runId) is retained. $script:LcManualRecovery"
        }
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

# --- owner-file integrity ----------------------------------------------------------

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
        if ($null -eq $intake) { return [ordered]@{ status = 'unresolved'; reason = 'no intake manifests were recorded' } }
        $savesBefore = Read-LcIntake $intake.savesPath $intake.savesSha256
        $installBefore = Read-LcIntake $intake.installPath $intake.installSha256
        $savesDiff = Compare-LcManifest $savesBefore ([string[]]@(& $script:LcSavesManifest))
        $installDiff = Compare-LcManifest $installBefore ([string[]]@(& $script:LcInstallManifest))
        return [ordered]@{ status = $(if ($savesDiff.Count -eq 0 -and $installDiff.Count -eq 0) { 'verified' } else { 'changed' })
            ownerSavesUnchanged = ($savesDiff.Count -eq 0); ownerInstallUnchanged = ($installDiff.Count -eq 0)
            ownerSaveChanges = @($savesDiff); ownerInstallChanges = @($installDiff); ownerSaveFiles = $savesBefore.Count }
    } catch { return [ordered]@{ status = 'unresolved'; reason = $_.Exception.Message } }
}

# --- registry comparison ------------------------------------------------------------

function Get-LcTextFromBytes([byte[]]$Bytes) {
    $reader = New-Object IO.StreamReader((New-Object IO.MemoryStream(, $Bytes)), (New-Object Text.UTF8Encoding($false)), $true)
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

function Get-LcBytesSha256([byte[]]$Bytes) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return (-join ($sha.ComputeHash($Bytes) | ForEach-Object { $_.ToString('x2') })) } finally { $sha.Dispose() }
}

function Get-LcRegistryCanonical([string]$Path) { return Get-LcRegistryCanonicalText (Get-LcTextFromBytes ([IO.File]::ReadAllBytes($Path))) }

# Ordinal equality of two canonical exports (never linguistic: distinct UTF-16 data must differ).
function Test-LcRegistryEqual([string]$Left, [string]$Right) { return [string]::Equals($Left, $Right, [StringComparison]::Ordinal) }

# A registry export in canonical form: each key with its complete value entries
# (continuation lines joined in order), values ordered ordinally within their key,
# keys in ordinal order. Two exports compare equal only if every key path, value
# name, type and ordered data agree.
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

# --- registry restoration -------------------------------------------------------------

# The restore source, held stable: the backup's bytes are read once and checked
# against the intake digest, written to a private verified copy, and that copy is
# opened with a read handle that denies writes and deletion and re-checked through
# that handle. The import uses the copy and the readback comparison uses the same
# verified bytes, so neither can see other contents. Throws, naming the problem,
# when the backup is missing, unreadable or changed.
function Open-LcVerifiedBackup($Journal) {
    $digest = [string]$Journal.registryBackupSha256
    if (-not $digest) { throw 'the registry backup has no digest recorded at intake' }
    try { $bytes = [IO.File]::ReadAllBytes($Journal.registryBackup) }
    catch { throw "the registry backup cannot be read ($($_.Exception.Message))" }
    if ($null -ne $script:LcBackupVerifyHook) { & $script:LcBackupVerifyHook }
    $actual = Get-LcBytesSha256 $bytes
    if ($actual -cne $digest) { throw "the registry backup no longer matches the digest recorded at intake (intake $digest, now $actual)" }
    $copy = Join-Path (Get-LcTransactionDir $Journal) ('registry-backup-verified-' + [Guid]::NewGuid().ToString('N') + '.reg')
    [IO.File]::WriteAllBytes($copy, $bytes)
    $handle = [IO.File]::Open($copy, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    $held = New-Object byte[] $handle.Length
    $offset = 0
    while ($offset -lt $held.Length) { $read = $handle.Read($held, $offset, $held.Length - $offset); if ($read -le 0) { break }; $offset += $read }
    if ((Get-LcBytesSha256 $held) -cne $actual) {
        $handle.Dispose(); Remove-Item -LiteralPath $copy -Force -ErrorAction SilentlyContinue
        throw 'the private verified copy of the registry backup does not hold the verified bytes'
    }
    return [pscustomobject]@{ Path = $copy; Handle = $handle; Canonical = (Get-LcRegistryCanonicalText (Get-LcTextFromBytes $bytes)); Sha256 = $actual }
}

# After the import: the live key, exported and compared ordinally (canonical form)
# with the verified backup bytes. Anything but an exact match is a failed restore.
function Test-LcRegistryReadback($Journal, $Verified) {
    $path = Join-Path (Get-LcTransactionDir $Journal) ('registry-readback-' + [Guid]::NewGuid().ToString('N') + '.reg')
    try { $state = & $script:LcRegistryExport $path }
    catch { return [ordered]@{ status = 'unreadable'; path = $path; reason = "the restored key could not be read back: $($_.Exception.Message)" } }
    if ($state -cne 'exported') { return [ordered]@{ status = 'absent'; path = $path; reason = 'the game-settings key does not exist after the import' } }
    $bytes = [IO.File]::ReadAllBytes($path)
    $sha = Get-LcBytesSha256 $bytes
    if (-not (Test-LcRegistryEqual (Get-LcRegistryCanonicalText (Get-LcTextFromBytes $bytes)) $Verified.Canonical)) {
        return [ordered]@{ status = 'mismatch'; path = $path; sha256 = $sha; reason = 'the game settings read back after the import differ from the verified session backup' }
    }
    return [ordered]@{ status = 'verified'; path = $path; sha256 = $sha; backupSha256 = $Verified.Sha256 }
}

# The registry step of normal completion. Returns $null once the restored key is
# read back equal to the verified backup (or when no backup exists because the
# session never reached intake), otherwise the failure reason.
function Invoke-LcRegistryRestore($Journal, [scriptblock]$Restore) {
    if (-not $Journal.registryBackup) {
        $Journal.registryRestore = 'not-needed'; Save-LcJournal $Journal
        return $null
    }
    try { $verified = Open-LcVerifiedBackup $Journal }
    catch {
        $Journal.registryRestore = 'refused'; Save-LcJournal $Journal
        return "Game settings were not restored: $($_.Exception.Message)."
    }
    try {
        $Journal.registryRestore = 'restoring'; $Journal.phase = 'restoring'; Save-LcJournal $Journal
        # The last checks follow the slow backup verification and immediately precede the destructive delete-and-import.
        $hostConflicts = Get-LcHostConflicts $Journal.leaseRunId; $claimProblems = Get-LcClaimProblems $Journal
        $problems = @($hostConflicts) + @($claimProblems)
        if ($problems.Count -gt 0) {
            Register-LcHostConflict $Journal $problems 'restore'
            $Journal.registryRestore = 'withheld'; Save-LcJournal $Journal
            return "Game-settings restoration was withheld ($($problems -join '; ')); the key was not changed."
        }
        try { & $Restore $verified.Path }
        catch {
            $Journal.registryRestore = 'failed'; Save-LcJournal $Journal
            return "Game-settings restoration failed: $($_.Exception.Message). The key may be partly restored."
        }
        $check = Test-LcRegistryReadback $Journal $verified
        $Journal.registryVerification = $check
        if ($check.status -cne 'verified') {
            $Journal.registryRestore = 'unverified'; Save-LcJournal $Journal
            return "Game-settings restoration is not verified ($($check.status)): $($check.reason)."
        }
        $Journal.registryRestore = 'applied'; $Journal.phase = 'restored'; Save-LcJournal $Journal
        return $null
    } finally {
        $verified.Handle.Dispose()
        Remove-Item -LiteralPath $verified.Path -Force -ErrorAction SilentlyContinue
    }
}

# --- completion --------------------------------------------------------------------

function New-LcResult([string]$Outcome, [string]$Reason, $Journal) {
    return [pscustomobject]@{ Outcome = $Outcome; Reason = $Reason; Registry = [string]$Journal.registryRestore
        RegistryVerification = $Journal.registryVerification; Integrity = $Journal.integrity }
}

# A failed completion keeps everything this session owns - the journal, both
# locks, the backup and the intake - and marks the lease RecoveryRequired, so
# every later session refuses it until it is resolved manually.
function Set-LcRetained($Journal, $Lease, [string]$Reason) {
    $text = "$Reason The session's journal, locks and backup are retained for manual recovery (transaction $($Journal.transactionId), lease $($Journal.leaseRunId)). $script:LcManualRecovery"
    $notes = @()
    try { $Journal.phase = 'retained'; $Journal.reason = $Reason; Save-LcJournal $Journal } catch { $notes += "the journal could not record the retention: $($_.Exception.Message)" }
    if ($null -ne $Lease -and $null -ne $Lease.State) {
        try {
            $Lease.State.status = 'RecoveryRequired'; $Lease.State.recoveryRequired = $true; $Lease.State.reason = $text
            $Lease.State['registryRestore'] = [string]$Journal.registryRestore; $Lease.State['integrity'] = $Journal.integrity
            & $script:LcLeaseStateWriter $Lease
        } catch { $notes += "the lease record could not record the retention: $($_.Exception.Message)" }
    }
    if ($null -ne $Lease -and $null -ne $Lease.Stream) { $Lease.Stream.Dispose() }
    if ($notes.Count) { $text += ' ' + ($notes -join ' ') }
    return (New-LcResult 'retained' $text $Journal)
}

# Normal completion, only for a session that never observed a host conflict:
# verified restoration, verified owner files, then the lease is completed and this
# session's claims released. A conflict observed earlier, or present now, retains
# the session at once - completion never waits for a peer to leave and then
# imports. Every failure, including an unexpected error, retains the session.
function Complete-LcTransaction($Journal, $Lease, [scriptblock]$Restore) {
    try {
        if ($null -ne $Journal.hostConflict) {
            return Set-LcRetained $Journal $Lease "A host conflict was observed during this session at $($Journal.hostConflict.at) ($(@($Journal.hostConflict.conflicts) -join '; ')); game settings were not restored, because a peer may have changed them."
        }
        $conflicts = Get-LcHostConflicts $Journal.leaseRunId
        if ($conflicts.Count -gt 0) {
            Register-LcHostConflict $Journal $conflicts 'completion'
            return Set-LcRetained $Journal $Lease "Not at a safe boundary ($($conflicts -join '; ')); game settings were not restored, because a peer may have changed them."
        }
        $failure = Invoke-LcRegistryRestore $Journal $Restore
        if ($null -ne $failure) { return Set-LcRetained $Journal $Lease $failure }
        $Journal.integrity = Test-LcOwnerIntegrity $Journal
        $Journal.phase = 'verified'; Save-LcJournal $Journal
        if ($Journal.integrity.status -cne 'verified') {
            $detail = if ($Journal.integrity.Contains('reason')) { $Journal.integrity.reason } else { (@($Journal.integrity.ownerSaveChanges) + @($Journal.integrity.ownerInstallChanges)) -join '; ' }
            return Set-LcRetained $Journal $Lease "The owner's files are not verified unchanged ($($Journal.integrity.status): $detail)."
        }
        # Ownership is released only while the host is still free and every claim is still this session's.
        $hostConflicts = Get-LcHostConflicts $Journal.leaseRunId; $claimProblems = Get-LcClaimProblems $Journal
        $late = @($hostConflicts) + @($claimProblems)
        if ($late.Count -gt 0) {
            Register-LcHostConflict $Journal $late 'release'
            return Set-LcRetained $Journal $Lease "The host or the session's claims changed after verification ($($late -join '; '))."
        }
        if ($null -ne $Lease -and $null -ne $Lease.State) {
            $Lease.State['integrity'] = $Journal.integrity
            $Lease.State['registryRestore'] = $Journal.registryRestore
            $Lease.State['registryVerification'] = $Journal.registryVerification
            $Lease.State.status = 'Completed'; $Lease.State.recoveryRequired = $false
            $Lease.State.reason = 'Loader-compat session completed: game settings restored and read back equal to the verified backup, owner files verified against the intake, host re-checked, then ownership released.'
            & $script:LcLeaseStateWriter $Lease
        }
        $Journal.phase = 'completed'; Save-LcJournal $Journal
        if ($null -ne $Lease -and $null -ne $Lease.Stream) { $Lease.Stream.Dispose() }
        Remove-KmgCompatibilityOwnedLock (Get-LcSharedLockPath) $Journal.leaseRunId
        Exit-LcLock $Journal.sessionId
        Remove-LcJournal $Journal
        return (New-LcResult 'completed' $null $Journal)
    } catch {
        return Set-LcRetained $Journal $Lease "Completion stopped unexpectedly: $($_.Exception.Message)."
    }
}

# The live runner's finalizer.
function Complete-LcSessionOwnership($Journal, $Scope, [scriptblock]$Restore) {
    $lease = if ($null -ne $Scope) { $Scope.Lease } else { $null }
    return Complete-LcTransaction $Journal $lease $Restore
}

# Clean only when the transaction completed, the game settings were restored and
# read back exactly (or never needed restoring) and the owner's files are verified.
function Test-LcTransactionSucceeded($Result) {
    return $null -ne $Result -and $Result.Outcome -ceq 'completed' -and $Result.Registry -cin @('applied', 'not-needed') -and
        $null -ne $Result.Integrity -and $Result.Integrity.status -ceq 'verified'
}

# The session verdict: a scenario failure and a restoration failure are reported
# separately, and either one makes the invocation fail. Passing launches never
# make an uncertain restoration successful.
function Get-LcSessionVerdict($Launches, [bool]$HostError, $Transaction) {
    $launchList = @($Launches | Where-Object { $null -ne $_ })
    $scenarioPassed = $launchList.Count -gt 0 -and @($launchList | Where-Object { $_.verdict -cne 'PASS' }).Count -eq 0
    $transactionOk = Test-LcTransactionSucceeded $Transaction
    $status = if ($HostError) { 'host-error' } elseif (-not $transactionOk) { 'integrity-failed' } elseif (-not $scenarioPassed) { 'FAIL' } else { 'PASS' }
    return [pscustomobject]@{ Status = $status; ScenarioPassed = $scenarioPassed; TransactionSucceeded = $transactionOk
        ExitCode = $(if ($status -ceq 'PASS') { 0 } else { 1 }) }
}
