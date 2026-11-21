Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Loader-compatibility sessions in a disposable full game copy. Adapted from the
# Kingmaker Cutscene Skip lab runtime harness (scripts/runtime, PR #2). The
# owner's installation, saves and game settings are only hashed and restored;
# every loader and mod change happens inside the copy.

$script:LcLabRoot = 'C:\Dev\KingmakerGunslingerLab'
$script:LcCopiesRoot = Join-Path $script:LcLabRoot 'game-copies'
$script:LcEvidenceRoot = Join-Path $script:LcLabRoot 'runtime-evidence\loader-compat'
$script:LcLock = Join-Path $script:LcCopiesRoot 'loader-compat.lock'
$script:LcJournal = Join-Path $script:LcCopiesRoot 'loader-compat.journal.json'
$script:LcComponentCache = Join-Path $script:LcLabRoot 'private\umm-distributions\cache'
$script:LcOwnerInstall = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
$script:LcOwnerData = Join-Path $env:USERPROFILE 'AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker'
$script:LcRegistryKey = 'HKCU\Software\Owlcat Games\Pathfinder Kingmaker'
# Locks of sibling labs on this machine; a session never starts while one exists.
$script:LcForeignLocks = @(
    'C:\Dev\KingmakerGunslingerLab\compatibility-state\compatibility.lock',
    'C:\Dev\KingmakerSkipCinematicLab\runtime-state\runtime.lock',
    'C:\Dev\KingmakerBuffPlannerLab\runtime-state\deployment.lock',
    'C:\Dev\KingmakerBuffPlannerLab\runtime-fixture-state\fixture.lock',
    'C:\Dev\KingmakerMemoryLeakResearchLab\runtime-state\kmld-runtime.lock',
    'C:\Dev\KingmakerMountedCombatLab\runtime-state\active-transaction.lock')

# The lab's shared runtime ownership: the same compatibility.lock and runtime-lease
# journal that profile staging, deployment, the generic runtime launcher and the
# settings transactions use (scripts/RuntimeCoordination.Common.ps1).
. (Join-Path $PSScriptRoot '..\..\RuntimeCoordination.Common.ps1')
$script:LcStateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
# Every Kingmaker process on the host. A script variable so the offline safety
# checks can substitute a disposable peer without starting a game.
$script:LcGameProcessQuery = { @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) }
function Get-LcGameProcesses { return , @(& $script:LcGameProcessQuery) }

function Get-LcSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

# Takes the shared compatibility lock for the whole session (refuses if any other
# entry point holds it) and records a runtime-lease journal for it.
function Enter-LcRuntimeOwnership([string]$SessionId) {
    return Enter-KmgRuntimeLease -ParentLease $null -Purpose ('loader-compat ' + $SessionId) -StateRoot $script:LcStateRoot
}

# ---------------------------------------------------------------------------
# Session transaction. One durable journal ($script:LcJournal, schema 2) records
# every step before it happens: acquisition of the shared lease and the private
# loader-compat lock, the owner-file intake, the registry backup, the single
# restoration, the integrity result and each release. Completion and -RecoverOnly
# share one routine that resumes from whatever phase was reached, so nothing is
# restored twice and every owned marker is accounted for before the journal goes.
# ---------------------------------------------------------------------------

# Fixture-only interruption point at a teardown boundary; $null in every real run.
$script:LcInterruptAfter = $null
function Step-LcBoundary([string]$Name) { if ($script:LcInterruptAfter -ceq $Name) { throw "Fixture interruption after $Name." } }

# Owner-file manifest providers; the offline checks substitute disposable lists.
$script:LcSavesManifest = { Get-LcOwnerSavesManifest }
$script:LcInstallManifest = { Get-LcOwnerInstallManifest }

function Save-LcJournal($Journal) { Write-LcJson $script:LcJournal $Journal }

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

# Immediately before every fresh launch.
function Assert-LcLaunchAvailable($Lease) {
    $conflicts = Get-LcHostConflicts $(if ($null -ne $Lease) { $Lease.RunId } else { $null })
    if ($conflicts.Count -gt 0) { throw "Launch withheld: $($conflicts -join '; ')." }
    if ($null -ne $Lease) { Assert-KmgRuntimeLease $Lease }
}

# Starts a session transaction. The journal is written before the shared lock is
# taken and updated after each acquisition; a failure undoes exactly what this call
# acquired (never a lock another session holds).
function Enter-LcSessionTransaction([string]$SessionId) {
    if (Test-Path -LiteralPath $script:LcJournal) { throw "An interrupted session journal exists ($script:LcJournal); run with -RecoverOnly first." }
    $journal = [ordered]@{ schemaVersion = 2; sessionId = $SessionId; phase = 'acquiring'
        ownerProcessId = $PID; ownerProcessStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
        leaseRunId = $null; leaseStatePath = $null; privateLock = $false
        registryBackup = $null; registryRestored = $false; intake = $null; integrity = $null; reason = $null }
    Save-LcJournal $journal
    $scope = $null
    try {
        $scope = Enter-LcRuntimeOwnership $SessionId
        $journal.leaseRunId = $scope.Lease.RunId; $journal.leaseStatePath = $scope.Lease.StatePath
        Save-LcJournal $journal
        Enter-LcLock $SessionId
        $journal.privateLock = $true; $journal.phase = 'acquired'
        Save-LcJournal $journal
    } catch {
        $failure = $_
        try {
            if ($journal.privateLock) { Exit-LcLock $SessionId }
            if ($null -ne $scope) {
                $lease = $scope.Lease
                $lease.State.status = 'Completed'; $lease.State.recoveryRequired = $false
                $lease.State.reason = 'Loader-compat session did not start: ' + $failure.Exception.Message
                Write-KmgRuntimeLeaseState $lease
                $lease.Stream.Dispose()
                Remove-KmgCompatibilityOwnedLock $lease.LockPath $lease.RunId
            }
            Remove-Item -LiteralPath $script:LcJournal -Force
        } catch { throw "Session start failed ($($failure.Exception.Message)) and its rollback failed ($($_.Exception.Message)); the journal is kept for -RecoverOnly." }
        throw $failure
    }
    return [pscustomobject]@{ Scope = $scope; Journal = $journal }
}

# Records the owner's saves and installation manifests in the session's private
# transaction state beside its lease journal, so completion or a later recovery
# compares against the same intake boundary.
function Save-LcIntake($Journal) {
    $directory = Split-Path -Parent $Journal.leaseStatePath
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

# The owner-file comparison against the recorded intake. Any read failure, or no
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

function Set-LcDeferred($Journal, $Lease, [string]$Reason) {
    $Journal.phase = 'recovery-required'; $Journal.reason = $Reason
    Save-LcJournal $Journal
    if ($null -ne $Lease -and $null -ne $Lease.State) {
        $Lease.State.status = 'RecoveryRequired'; $Lease.State.recoveryRequired = $true; $Lease.State.reason = $Reason
        Write-KmgRuntimeLeaseState $Lease
    }
}

# Finishes a transaction from its recorded phase: waits for a safe boundary (no
# Kingmaker process, no sibling or foreign lock), restores the game settings
# exactly once, compares the owner's files with the intake while still owning the
# host, then completes the lease and releases the shared lock, the loader-compat
# lock and finally the journal - each step journaled before the next.
function Complete-LcTransaction($Journal, $Lease, [scriptblock]$Restore, [int]$WaitSeconds) {
    $released = @('lease-completed', 'shared-released', 'private-released')
    if ($Journal.phase -notin $released) {
        $deadline = (Get-Date).AddSeconds($WaitSeconds)
        while ((Get-LcHostConflicts $Journal.leaseRunId).Count -gt 0 -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 1 }
        $conflicts = Get-LcHostConflicts $Journal.leaseRunId
        if ($conflicts.Count -gt 0) {
            $reason = "Not at a safe boundary ($($conflicts -join '; ')); game settings were not restored and ownership is retained. Run Invoke-LoaderCompatSession.ps1 -RecoverOnly once the host is free."
            Set-LcDeferred $Journal $Lease $reason
            return [pscustomobject]@{ Outcome = 'recovery-required'; Reason = $reason; Integrity = $Journal.integrity }
        }
        if (-not $Journal.registryRestored) {
            if ($Journal.registryBackup) {
                $Journal.phase = 'restoring'; Save-LcJournal $Journal
                try { & $Restore $Journal.registryBackup }
                catch {
                    $reason = "Game-settings restoration failed: $($_.Exception.Message). Ownership is retained for -RecoverOnly."
                    Set-LcDeferred $Journal $Lease $reason
                    return [pscustomobject]@{ Outcome = 'recovery-required'; Reason = $reason; Integrity = $Journal.integrity }
                }
            }
            $Journal.registryRestored = $true; $Journal.phase = 'restored'; Save-LcJournal $Journal
            Step-LcBoundary 'restored'
        }
        if ($null -eq $Journal.integrity) {
            $Journal.integrity = Test-LcOwnerIntegrity $Journal
            $Journal.phase = 'verified'; Save-LcJournal $Journal
            Step-LcBoundary 'verified'
        }
        if ($null -ne $Lease -and $null -ne $Lease.State) {
            Set-LcStateValue $Lease.State 'integrity' $Journal.integrity
            $Lease.State.status = 'Completed'; $Lease.State.recoveryRequired = $false
            $Lease.State.reason = 'Loader-compat session completed at a safe boundary: game settings restored once, owner files compared with the intake, then ownership released.'
            Write-KmgRuntimeLeaseState $Lease
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
    if ($Journal.privateLock) {
        Exit-LcLock $Journal.sessionId
        $Journal.privateLock = $false; $Journal.phase = 'private-released'; Save-LcJournal $Journal
        Step-LcBoundary 'private-released'
    }
    Remove-Item -LiteralPath $script:LcJournal -Force
    return [pscustomobject]@{ Outcome = 'restored'; Reason = $null; Integrity = $Journal.integrity }
}

# The live runner's finalizer.
function Complete-LcSessionOwnership($Journal, $Scope, [scriptblock]$Restore, [int]$WaitSeconds = 60) {
    $lease = if ($null -ne $Scope) { $Scope.Lease } else { $null }
    $result = Complete-LcTransaction $Journal $lease $Restore $WaitSeconds
    if ($result.Outcome -ne 'restored' -and $null -ne $lease) { $lease.Stream.Dispose() }
    return $result
}

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

function Get-LcPrivateLockInfo {
    if (-not (Test-Path -LiteralPath $script:LcLock)) { return $null }
    $info = @{}
    foreach ($line in [IO.File]::ReadAllLines($script:LcLock)) { if ($line -match '^([a-zA-Z]+)=(.*)$') { $info[$Matches[1]] = $Matches[2] } }
    return $info
}

function Find-LcOrphanLease([string]$SessionId) {
    if (-not (Test-Path -LiteralPath $script:LcStateRoot)) { return $null }
    $found = @(Get-ChildItem -LiteralPath $script:LcStateRoot -Directory | ForEach-Object {
        $path = Join-Path $_.FullName 'runtime-lease.json'
        if (Test-Path -LiteralPath $path) {
            $state = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
            if ([string]$state.purpose -ceq ('loader-compat ' + $SessionId)) { [pscustomobject]@{ RunId = [string]$state.runId; Path = $path } }
        } })
    if ($found.Count -eq 1) { return $found[0] }
    return $null
}

# Older journals (round 7 or earlier) become a schema-2 recovery record with the
# same ownership facts: the lease's owner process, else the private lock's pid.
function Convert-LcLegacyJournal($Raw) {
    $journal = [ordered]@{ schemaVersion = 2; sessionId = [string]$Raw.sessionId; phase = 'recovery-required'
        ownerProcessId = $null; ownerProcessStartedUtc = $null; ownerLockStartedUtc = $null
        leaseRunId = $null; leaseStatePath = $null; privateLock = $false
        registryBackup = $(if ($Raw.PSObject.Properties['registryBackup']) { $Raw.registryBackup } else { $null })
        registryRestored = $false; intake = $null; integrity = $null; reason = 'converted from an older session journal' }
    if ($Raw.PSObject.Properties['leaseRunId'] -and $Raw.leaseRunId) {
        $journal.leaseRunId = [string]$Raw.leaseRunId; $journal.leaseStatePath = [string]$Raw.leaseStatePath
        $state = Read-KmgCompatibilityJson $journal.leaseStatePath
        $journal.ownerProcessId = $state.processId; $journal.ownerProcessStartedUtc = [string]$state.processStartedUtc
    }
    $private = Get-LcPrivateLockInfo
    if ($null -ne $private -and $private['session'] -ceq $journal.sessionId) {
        $journal.privateLock = $true
        if (-not $journal.ownerProcessStartedUtc) { $journal.ownerProcessId = $private['pid']; $journal.ownerLockStartedUtc = $private['startedUtc'] }
    }
    return $journal
}

# -RecoverOnly. Before the first registry write it establishes exclusive ownership
# of the exact interrupted session: the original owner has exited, the host is free
# of games and of every sibling or foreign lock, the shared lock (re-taken if it is
# missing) belongs to this session's lease, and this caller holds the lock's
# exclusive recovery handle. Refused callers change nothing.
function Invoke-LcRecovery([scriptblock]$Restore) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { return 'No interrupted session journal.' }
    $raw = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json
    $journal = if ($raw.PSObject.Properties['schemaVersion'] -and $raw.schemaVersion -eq 2) { ConvertTo-LcOrdered $raw } else { Convert-LcLegacyJournal $raw }
    if (Test-LcOwnerAlive $journal) { throw "The original session owner (pid $($journal.ownerProcessId)) is still alive; nothing was changed." }
    if (-not $journal.leaseRunId) {
        $orphan = Find-LcOrphanLease $journal.sessionId
        if ($null -ne $orphan) { $journal.leaseRunId = $orphan.RunId; $journal.leaseStatePath = $orphan.Path }
    }
    $conflicts = Get-LcHostConflicts $journal.leaseRunId
    if ($conflicts.Count -gt 0) { throw "Recovery withheld: $($conflicts -join '; '). Nothing was changed." }
    $lockPath = Get-LcSharedLockPath
    $stream = $null; $lease = $null
    try {
        if ($journal.leaseRunId -and $journal.phase -notin @('shared-released', 'private-released')) {
            if (-not (Test-Path -LiteralPath $lockPath)) {
                if ($journal.phase -eq 'lease-completed') { $journal.phase = 'shared-released' }
                else { [void](Acquire-KmgCompatibilityLock $script:LcStateRoot $journal.leaseRunId) }
            }
            if (Test-Path -LiteralPath $lockPath) {
                try { $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read) }
                catch [IO.IOException] { throw "Another caller holds this session's recovery; nothing was changed." }
                if ((Get-Content -LiteralPath $lockPath -Raw).Trim() -cne $journal.leaseRunId) { throw 'The shared lock is not held by this session; nothing was changed.' }
                $state = $null
                if ($journal.phase -ne 'lease-completed') {
                    $state = Read-KmgCompatibilityJson $journal.leaseStatePath
                    if ([string]$state.runId -cne $journal.leaseRunId -or [string]$state.purpose -cne ('loader-compat ' + $journal.sessionId)) {
                        throw 'The recorded lease does not belong to this session; nothing was changed.'
                    }
                }
                $lease = [pscustomobject]@{ RunId = $journal.leaseRunId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
                    StatePath = $journal.leaseStatePath; StateSha256 = $(if ($null -ne $state) { Get-KmgCompatibilitySha256 $journal.leaseStatePath } else { $null })
                    Compatibility = $false; State = $state }
            }
        }
        $result = Complete-LcTransaction $journal $lease $Restore 0
        if ($result.Outcome -ne 'restored') { throw "Recovery deferred: $($result.Reason)" }
        return "Recovered interrupted session $($journal.sessionId) (integrity: $($result.Integrity.status))."
    } finally { if ($null -ne $stream) { $stream.Dispose() } }
}

function Write-LcJson([string]$Path, $Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 12), (New-Object Text.UTF8Encoding($false)))
}

# Refuses anything but a copy under the lab's game-copies root, so a mistyped
# path can never touch the loader of a real installation.
function Assert-LcGameCopy([string]$CopyRoot, [string]$OwnerInstall = $script:LcOwnerInstall) {
    $full = [IO.Path]::GetFullPath($CopyRoot).TrimEnd('\')
    $owner = [IO.Path]::GetFullPath($OwnerInstall).TrimEnd('\')
    $allowed = $script:LcCopiesRoot.TrimEnd('\') + '\'
    if ($full -ieq $owner -or $full.StartsWith($owner + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to use the owner's installation as a game copy: $full"
    }
    if (-not $full.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) { throw "A game copy must live under $allowed (got $full)." }
    if (-not (Test-Path -LiteralPath (Join-Path $full 'Kingmaker.exe') -PathType Leaf)) { throw "Game copy lacks Kingmaker.exe: $full" }
    return $full
}

function Get-LcCopyPaths([string]$CopyRoot) {
    $managed = Join-Path $CopyRoot 'Kingmaker_Data\Managed'
    return [pscustomobject]@{
        InstallDir = $CopyRoot; ManagedDir = $managed; UmmDir = Join-Path $managed 'UnityModManager'
        ModsDir = Join-Path $CopyRoot 'Mods'; Pristine = Join-Path $CopyRoot '.lab-pristine'
    }
}

function Assert-LcPreflight {
    if ((Get-LcGameProcesses).Count -gt 0) { throw 'Kingmaker is already running; refusing to take over another session.' }
    foreach ($lock in $script:LcForeignLocks) { if (Test-Path -LiteralPath $lock) { throw "Another lab holds a runtime lock: $lock" } }
    if (Test-Path -LiteralPath $script:LcJournal) { throw "An interrupted session journal exists ($script:LcJournal); run with -RecoverOnly first." }
    $steam = (Get-ItemProperty -LiteralPath 'HKCU:\Software\Valve\Steam').SteamPath
    $cloudLog = Join-Path $steam 'logs\cloud_log.txt'
    $last = @(Select-String -LiteralPath $cloudLog -Pattern '\[AppID 640820\]' | Select-Object -Last 6 | ForEach-Object Line)
    if (-not ($last | Where-Object { $_ -match 'Sync Disabled' })) { throw 'Steam Cloud sync for Kingmaker is not proven disabled (cloud_log.txt); refusing to launch.' }
    if (-not (Get-Process -Name steam -ErrorAction SilentlyContinue)) { throw 'Steam is not running; a direct game-copy launch needs it.' }
}

function Enter-LcLock([string]$SessionId) {
    New-Item -ItemType Directory -Force -Path $script:LcCopiesRoot | Out-Null
    $stream = [IO.File]::Open($script:LcLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes("session=$SessionId`npid=$PID`nstartedUtc=$([DateTime]::UtcNow.ToString('o'))`n")
        $stream.Write($bytes, 0, $bytes.Length)
    } finally { $stream.Dispose() }
}

# Removes the loader-compat lock only when it names this session.
function Exit-LcLock([string]$SessionId) {
    if (-not (Test-Path -LiteralPath $script:LcLock)) { return }
    $info = Get-LcPrivateLockInfo
    if ($null -eq $info -or $info['session'] -cne $SessionId) { throw "The loader-compat lock belongs to another session and was not removed: $script:LcLock" }
    Remove-Item -LiteralPath $script:LcLock -Force
}

function Get-LcFileManifest([string[]]$Paths) {
    return @(& { foreach ($path in $Paths) {
        if (Test-Path -LiteralPath $path -PathType Container) {
            Get-ChildItem -LiteralPath $path -File -Recurse -Force
        } elseif (Test-Path -LiteralPath $path -PathType Leaf) { Get-Item -LiteralPath $path -Force }
    } } | Sort-Object FullName | ForEach-Object { '{0}|{1}|{2}' -f $_.FullName, $_.Length, (Get-LcSha256 $_.FullName) })
}

function Get-LcOwnerSavesManifest { Get-LcFileManifest @((Join-Path $script:LcOwnerData 'Saved Games')) }

# Every file a loader or mod change could affect in the owner's installation.
function Get-LcOwnerInstallManifest {
    $o = $script:LcOwnerInstall
    Get-LcFileManifest @((Join-Path $o 'winhttp.dll'), (Join-Path $o 'doorstop_config.ini'), (Join-Path $o 'Kingmaker.exe'),
        (Join-Path $o 'Kingmaker_Data\Managed\Assembly-CSharp.dll'), (Join-Path $o 'Kingmaker_Data\Managed\UnityModManager'),
        (Join-Path $o 'Mods'))
}

function Compare-LcManifest([string[]]$Before, [string[]]$After) {
    $changes = @()
    $changes += @($Before | Where-Object { $After -notcontains $_ } | ForEach-Object { 'changed/removed: ' + $_.Split('|')[0] })
    $changes += @($After | Where-Object { $Before -notcontains $_ } | ForEach-Object { 'new/changed: ' + $_.Split('|')[0] })
    return ,$changes
}

function Invoke-LcReg([string[]]$Arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { $output = & reg.exe @Arguments 2>&1 | ForEach-Object { "$_" } } finally { $ErrorActionPreference = $previous }
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
}

function Backup-LcRegistry([string]$Path) {
    $r = Invoke-LcReg @('export', $script:LcRegistryKey, $Path, '/y')
    if ($r.ExitCode -ne 0) { throw "Registry export failed: $($r.Output)" }
}

# Exact restoration: remove the key the runs may have changed, then import the
# backup taken before the session.
function Restore-LcRegistry([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Registry backup is missing: $Path" }
    $d = Invoke-LcReg @('delete', $script:LcRegistryKey, '/f')
    if ($d.ExitCode -ne 0 -and $d.Output -notmatch 'unable to find') { throw "Registry delete failed: $($d.Output)" }
    $i = Invoke-LcReg @('import', $Path)
    if ($i.ExitCode -ne 0) { throw "Registry import failed: $($i.Output)" }
}

# One-time capture of the copy's as-copied loader and mod state (a byte copy of
# the owner's installation), so the owner's 0.33.0 set and mods are re-applied exactly.
function Initialize-LcGameCopy($Paths) {
    if (-not (Test-Path -LiteralPath $Paths.Pristine)) {
        $building = "$($Paths.Pristine).building"
        if (Test-Path -LiteralPath $building) { Remove-Item -LiteralPath $building -Recurse -Force }
        New-Item -ItemType Directory -Path $building | Out-Null
        Copy-Item -LiteralPath $Paths.UmmDir -Destination (Join-Path $building 'UnityModManager') -Recurse
        Copy-Item -LiteralPath $Paths.ModsDir -Destination (Join-Path $building 'Mods') -Recurse
        foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Copy-Item -LiteralPath (Join-Path $Paths.InstallDir $name) -Destination $building }
        Move-Item -LiteralPath $building -Destination $Paths.Pristine
    }
    Set-Content -LiteralPath (Join-Path $Paths.InstallDir 'steam_appid.txt') -Value '640820' -Encoding ASCII -NoNewline
}

function Resolve-LcComponent($File) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    New-Item -ItemType Directory -Force -Path $script:LcComponentCache | Out-Null
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $urlKey = -join ($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($File.url)) | ForEach-Object { $_.ToString('x2') }) } finally { $sha.Dispose() }
    $download = Join-Path $script:LcComponentCache $urlKey
    if (-not (Test-Path -LiteralPath $download)) { Invoke-WebRequest -Uri $File.url -OutFile $download -UseBasicParsing }
    $entry = $File.PSObject.Properties['archiveEntry']
    if ($null -eq $entry -or -not $entry.Value) { return $download }
    if ((Get-LcSha256 $download) -ne $File.archiveSha256) { throw "Archive hash mismatch: $($File.url)" }
    $extracted = Join-Path $script:LcComponentCache ($File.sha256 + '.bin')
    if (-not (Test-Path -LiteralPath $extracted)) {
        $archive = [IO.Compression.ZipFile]::OpenRead($download)
        try { [IO.Compression.ZipFileExtensions]::ExtractToFile($archive.GetEntry($entry.Value), $extracted, $true) } finally { $archive.Dispose() }
    }
    return $extracted
}

# Clean install: the loader directory, proxy and proxy configuration are removed
# and rebuilt from one declared file set, never a mixture of old and new files.
function Install-LcLoaderProfile($Paths, [string]$ProfileName) {
    $document = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'loader-profiles.json') -Raw | ConvertFrom-Json
    $installed = [ordered]@{ profile = $ProfileName; files = [ordered]@{} }
    Remove-Item -LiteralPath $Paths.UmmDir -Recurse -Force
    foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Remove-Item -LiteralPath (Join-Path $Paths.InstallDir $name) -Force }
    if ($ProfileName -eq 'owner-0.33.0') {
        $installed.method = 'byte copy of the owner installation (UMM 0.33.0 Nexus distribution, Harmony 2.3.6)'
        Copy-Item -LiteralPath (Join-Path $Paths.Pristine 'UnityModManager') -Destination $Paths.UmmDir -Recurse
        foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Copy-Item -LiteralPath (Join-Path $Paths.Pristine $name) -Destination $Paths.InstallDir }
        foreach ($name in @('Log.txt', 'Params.xml')) { Remove-Item -LiteralPath (Join-Path $Paths.UmmDir $name) -Force -ErrorAction SilentlyContinue }
        foreach ($p in $document.owner033.expected.PSObject.Properties) {
            if ((Get-LcSha256 (Join-Path $Paths.UmmDir $p.Name)) -ne $p.Value) { throw "owner-0.33.0 file differs from its recorded identity: $($p.Name)" }
        }
    } else {
        $property = $document.profiles.PSObject.Properties[$ProfileName]
        if (-not $property) { throw "Unknown loader profile: $ProfileName" }
        $definition = $property.Value
        $installed.method = 'component-assembled from upstream NuGet packages and the UMM repository; not the Nexus installer ZIP'
        $installed.ummVersion = $definition.ummVersion
        $installed.harmonyVersion = $definition.harmonyVersion
        New-Item -ItemType Directory -Path $Paths.UmmDir | Out-Null
        Copy-Item -LiteralPath (Join-Path $Paths.Pristine 'UnityModManager\Config.xml') -Destination $Paths.UmmDir
        foreach ($entry in $definition.files.PSObject.Properties) {
            $source = Resolve-LcComponent $entry.Value
            if ((Get-LcSha256 $source) -ne $entry.Value.sha256) { throw "Component hash mismatch for $($entry.Name)" }
            Copy-Item -LiteralPath $source -Destination (Join-Path $Paths.InstallDir ($entry.Name -replace '/', '\')) -Force
        }
        [IO.File]::WriteAllText((Join-Path $Paths.InstallDir 'doorstop_config.ini'), $document.doorstopConfig, [Text.Encoding]::ASCII)
    }
    foreach ($file in @(Get-ChildItem -LiteralPath $Paths.UmmDir -File)) { $installed.files[$file.Name] = Get-LcSha256 $file.FullName }
    foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { $installed.files[$name] = Get-LcSha256 (Join-Path $Paths.InstallDir $name) }
    return $installed
}

# Rebuilds the copy's Mods folder: the named owner mods from the pristine set,
# the candidate exactly as a user installs it (the ZIP's single mod folder),
# and the test-only harness.
function Set-LcGameCopyMods($Paths, [string[]]$OwnerMods, [string]$CandidateZip, [string]$HarnessDir) {
    if (Test-Path -LiteralPath $Paths.ModsDir) { Remove-Item -LiteralPath $Paths.ModsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $Paths.ModsDir | Out-Null
    $staged = [ordered]@{}
    foreach ($name in @($OwnerMods | Where-Object { $_ })) {
        if ($name -eq 'KingmakerGunslinger') { throw 'The product comes from the candidate ZIP, never the owner set.' }
        $source = Join-Path $Paths.Pristine "Mods\$name"
        if (-not (Test-Path -LiteralPath $source)) { throw "Mod not found in the pristine set: $name" }
        Copy-Item -LiteralPath $source -Destination $Paths.ModsDir -Recurse
        $info = Get-Content -LiteralPath (Join-Path $source 'Info.json') -Raw | ConvertFrom-Json
        $dll = Get-ChildItem -LiteralPath $source -Filter '*.dll' -File | Where-Object { $_.Name -eq $(if ($info.PSObject.Properties['AssemblyName'] -and $info.AssemblyName) { $info.AssemblyName } else { "$name.dll" }) }
        # ManagerVersion is optional in a mod's Info.json.
        $staged[$name] = [ordered]@{ version = $info.Version
            managerVersion = $(if ($info.PSObject.Properties['ManagerVersion']) { $info.ManagerVersion })
            dllSha256 = $(if ($dll) { Get-LcSha256 $dll.FullName }) }
    }
    Expand-Archive -LiteralPath $CandidateZip -DestinationPath $Paths.ModsDir
    $staged['KingmakerGunslinger'] = [ordered]@{ zipSha256 = Get-LcSha256 $CandidateZip
        dllSha256 = Get-LcSha256 (Join-Path $Paths.ModsDir 'KingmakerGunslinger\KingmakerGunslinger.dll')
        managerVersion = (Get-Content -LiteralPath (Join-Path $Paths.ModsDir 'KingmakerGunslinger\Info.json') -Raw | ConvertFrom-Json).ManagerVersion }
    $harnessTarget = Join-Path $Paths.ModsDir 'KmgLoaderCompatHarness'
    New-Item -ItemType Directory -Path $harnessTarget | Out-Null
    Copy-Item -LiteralPath (Join-Path $HarnessDir 'KmgLoaderCompatHarness.dll') -Destination $harnessTarget
    Copy-Item -LiteralPath (Join-Path $HarnessDir 'Info.json') -Destination $harnessTarget
    $staged['KmgLoaderCompatHarness'] = [ordered]@{ dllSha256 = Get-LcSha256 (Join-Path $harnessTarget 'KmgLoaderCompatHarness.dll') }
    return $staged
}

function Start-LcKingmakerCopy($Paths, [string[]]$Arguments) {
    $exe = Join-Path $Paths.InstallDir 'Kingmaker.exe'
    $quoted = @('-popupwindow', '-screen-width', '1280', '-screen-height', '720') + ($Arguments | ForEach-Object { if ($_ -match '\s') { '"' + $_ + '"' } else { $_ } })
    return Start-Process -FilePath $exe -WorkingDirectory $Paths.InstallDir -ArgumentList ($quoted -join ' ') -PassThru
}

# Stops only the process this session started, and only if it runs from the copy.
function Stop-LcOwnedProcess($Process, $Paths) {
    if ($Process.HasExited) { return 'already-exited' }
    $path = $null
    try { $path = $Process.MainModule.FileName } catch { }
    if (-not $path -or -not $path.StartsWith($Paths.InstallDir, [StringComparison]::OrdinalIgnoreCase)) { throw "Refusing to stop process $($Process.Id): not the game copy ($path)." }
    $Process.CloseMainWindow() | Out-Null
    if (-not $Process.WaitForExit(20000)) { $Process.Refresh(); if (-not $Process.HasExited) { $Process.Kill(); $Process.WaitForExit(20000) | Out-Null } }
    return 'stopped-after-timeout'
}
