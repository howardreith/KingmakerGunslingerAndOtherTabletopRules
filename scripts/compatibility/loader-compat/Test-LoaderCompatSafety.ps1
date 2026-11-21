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

    # The session transaction, with a disposable state root, lock files, journal and owner-file manifests, and a
    # substituted process query standing in for a peer Kingmaker process.
    $saved = @{ State = $script:LcStateRoot; Foreign = $script:LcForeignLocks; Journal = $script:LcJournal; Lock = $script:LcLock
        Query = $script:LcGameProcessQuery; Saves = $script:LcSavesManifest; Install = $script:LcInstallManifest }
    try {
        $state = Join-Path $fixture 'compatibility-state'
        $compatLock = Join-Path $state 'compatibility.lock'
        $peerLock = Join-Path $fixture 'sibling-lab\runtime.lock'
        New-Item -ItemType Directory -Force -Path (Split-Path $peerLock), $state | Out-Null
        $script:LcStateRoot = $state
        $script:LcForeignLocks = @($compatLock, $peerLock)
        $script:LcJournal = Join-Path $copies 'loader-compat.journal.json'
        $script:LcLock = Join-Path $copies 'loader-compat.lock'
        $global:LcTestGames = @()
        $script:LcGameProcessQuery = { @($global:LcTestGames) }
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }
        $global:LcTestInstall = { @('install\winhttp.dll|5|cc') }
        $script:LcSavesManifest = { & $global:LcTestSaves }
        $script:LcInstallManifest = { & $global:LcTestInstall }
        $global:LcTestRestores = 0
        $restore = { param($Path) $global:LcTestRestores++ }
        $backup = Join-Path $fixture 'settings.reg'
        $clean = { -not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcLock) -and -not (Test-Path -LiteralPath $script:LcJournal) }
        function New-FixtureTransaction([string]$Id, [switch]$Bare) {
            $tx = Enter-LcSessionTransaction $Id
            if (-not $Bare) { [void](Save-LcIntake $tx.Journal); $tx.Journal.registryBackup = $backup; Save-LcJournal $tx.Journal }
            return $tx
        }
        function Set-FixtureOwnerGone {
            $j = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json
            $j.ownerProcessStartedUtc = '2000-01-01T00:00:00.0000000Z'
            Write-LcJson $script:LcJournal $j
        }
        function Get-FixtureLease([string]$Path) { return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json }

        # R8-F2: inputs are checked before anything is acquired.
        Assert-That ((Get-Refusal { Assert-LcSessionInputs (Join-Path $fixture 'missing.zip') $fixture }) -like 'Candidate ZIP not found*Nothing was acquired.') 'missing-candidate-refused'
        $zip = Join-Path $fixture 'candidate.zip'; [IO.File]::WriteAllText($zip, 'zip')
        Assert-That ((Get-Refusal { Assert-LcSessionInputs $zip (Join-Path $fixture 'no-harness') }) -like 'Harness file missing*') 'missing-harness-refused'
        Assert-That (& $clean) 'refused-inputs-acquired-nothing'

        # A foreign holder of the shared lock is refused and never overwritten; nothing is left behind.
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-refused' }) -like '*lock already exists*') 'foreign-shared-lock-refused'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run' -and -not (Test-Path -LiteralPath $script:LcJournal)) 'foreign-shared-lock-kept-no-journal'
        Remove-Item -LiteralPath $compatLock -Force

        # A stale private lock of another session: acquisition is rolled back and that lock preserved.
        [IO.File]::WriteAllText($script:LcLock, "session=older-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n")
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-private' }) -ne $null) 'stale-private-lock-refused'
        Assert-That (-not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcJournal) -and
            ((Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=older-session*')) 'stale-private-lock-rollback-releases-only-own'
        Remove-Item -LiteralPath $script:LcLock -Force

        # R8-F2: a setup failure after acquisition (before intake or backup) is completed by the finalizer, and a
        # following session acquires both locks without manual deletion.
        $txSetup = New-FixtureTransaction 'fixture-setup-failure' -Bare
        $outcome = Complete-LcSessionOwnership $txSetup.Journal $txSetup.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'restored' -and $global:LcTestRestores -eq 0 -and $outcome.Integrity.status -eq 'unresolved') 'setup-failure-completed-integrity-unresolved'
        Assert-That (& $clean) 'setup-failure-released-both-locks'
        $txNext = New-FixtureTransaction 'fixture-after-setup-failure'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txNext.Scope.Lease.RunId -and (Test-Path -LiteralPath $script:LcLock)) 'next-session-acquires-both-locks'
        $outcome = Complete-LcSessionOwnership $txNext.Journal $txNext.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'restored' -and $global:LcTestRestores -eq 1 -and $outcome.Integrity.status -eq 'verified' -and (& $clean)) 'next-session-completes'

        # R7-F1 / R8-F1: a sibling-lab lock (no game process) after launch one.
        $txA = New-FixtureTransaction 'fixture-sibling'
        $leaseA = $txA.Scope.Lease
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $leaseA })) 'launch-one-admitted-with-own-lock'
        [IO.File]::WriteAllText($peerLock, 'sibling')
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $leaseA }) -like 'Launch withheld: another lab holds a runtime lock*runtime.lock*') 'launch-two-withheld-for-sibling-lock'
        $global:LcTestGames = @([pscustomobject]@{ Id = 4242 })
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $leaseA }) -like 'Launch withheld: Kingmaker is running (pid 4242)*') 'launch-withheld-for-peer-game'
        $global:LcTestGames = @()
        $outcome = Complete-LcSessionOwnership $txA.Journal $txA.Scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required' -and $outcome.Reason -like '*another lab holds a runtime lock*') 'sibling-lock-finalizer-defers'
        Assert-That ($global:LcTestRestores -eq 1) 'sibling-lock-no-restore'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $leaseA.RunId -and (Test-Path -LiteralPath $script:LcLock)) 'sibling-lock-both-locks-retained'
        Assert-That ((Get-FixtureLease $leaseA.StatePath).status -ceq 'RecoveryRequired' -and (Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json).phase -ceq 'recovery-required') 'sibling-lock-lease-and-journal-recovery-required'
        # Recovery while the original owner lives (this process): refused, nothing changed.
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The original session owner*still alive*nothing was changed.') 'recovery-refused-live-owner'
        Set-FixtureOwnerGone
        # Owner gone but the sibling still holds its lock: refused, nothing changed.
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: another lab holds a runtime lock*Nothing was changed.') 'recovery-refused-sibling-lock'
        Assert-That ($global:LcTestRestores -eq 1 -and (Test-Path -LiteralPath $script:LcJournal) -and (Test-Path -LiteralPath $script:LcLock) -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $leaseA.RunId) 'refused-recoveries-changed-nothing'
        Remove-Item -LiteralPath $peerLock -Force
        # A competing recovery caller holding the session's recovery handle: refused before any restore.
        $competing = [IO.File]::Open($compatLock, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
        try { Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like "Another caller holds this session's recovery*") 'competing-recovery-refused' }
        finally { $competing.Dispose() }
        Assert-That ($global:LcTestRestores -eq 1) 'competing-recovery-no-restore'
        Assert-That ((Invoke-LcRecovery $restore) -like 'Recovered interrupted session fixture-sibling (integrity: verified)*') 'recovery-after-sibling-release'
        Assert-That ($global:LcTestRestores -eq 2 -and (& $clean) -and (Get-FixtureLease $leaseA.StatePath).status -ceq 'Completed') 'recovery-restored-once-and-released'

        # R8-F1: a stale journal beside a foreign shared-lock holder.
        $txF = New-FixtureTransaction 'fixture-foreign-shared'
        $txF.Scope.Lease.Stream.Dispose()
        Set-FixtureOwnerGone
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld: another lab holds a runtime lock*compatibility.lock*') 'recovery-refused-foreign-shared-lock'
        Assert-That ($global:LcTestRestores -eq 2 -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run' -and (Test-Path -LiteralPath $script:LcJournal)) 'foreign-shared-lock-preserved-no-restore'
        [IO.File]::WriteAllText($compatLock, $txF.Scope.Lease.RunId)
        Assert-That ((Invoke-LcRecovery $restore) -like 'Recovered*' -and $global:LcTestRestores -eq 3 -and (& $clean)) 'recovery-after-foreign-holder-left'

        # R8-F3: integrity is verified while the session still owns the host; a competing acquisition during
        # verification is refused, and a changed or unreadable manifest is an explicit result.
        $txI = New-FixtureTransaction 'fixture-integrity'
        $global:LcTestCompetitor = $null
        $global:LcTestSaves = {
            try { [void](Enter-LcRuntimeOwnership 'fixture-competitor'); $global:LcTestCompetitor = 'acquired' } catch { $global:LcTestCompetitor = $_.Exception.Message }
            @('saves\a.zks|10|aa', 'saves\b.zks|21|changed')
        }
        $outcome = Complete-LcSessionOwnership $txI.Journal $txI.Scope $restore 0
        Assert-That ($global:LcTestCompetitor -like '*lock already exists*') 'competing-acquisition-refused-during-verification'
        Assert-That ($outcome.Integrity.status -eq 'changed' -and -not $outcome.Integrity.ownerSavesUnchanged -and $outcome.Integrity.ownerInstallUnchanged) 'changed-manifest-recorded'
        Assert-That ((Get-FixtureLease $txI.Scope.Lease.StatePath).integrity.status -ceq 'changed' -and (& $clean)) 'changed-integrity-recorded-in-lease'
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }
        $txU = New-FixtureTransaction 'fixture-unreadable'
        $global:LcTestSaves = { throw 'owner saves folder unreadable' }
        $outcome = Complete-LcSessionOwnership $txU.Journal $txU.Scope $restore 0
        Assert-That ($outcome.Integrity.status -eq 'unresolved' -and $outcome.Integrity.reason -like '*unreadable*') 'unreadable-manifest-unresolved'
        Assert-That ((Get-FixtureLease $txU.Scope.Lease.StatePath).integrity.status -ceq 'unresolved' -and (& $clean)) 'unresolved-integrity-recorded-in-lease'
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|20|bb') }

        # R8-F4: completion interrupted at each teardown boundary resumes without a second restore and clears exactly
        # the remaining owned markers (a sibling lock present at the shared-released boundary is preserved first).
        foreach ($boundary in @('restored', 'verified', 'lease-completed', 'shared-released', 'private-released')) {
            $tx = New-FixtureTransaction ('fixture-interrupt-' + $boundary)
            $restoresBefore = $global:LcTestRestores
            $script:LcInterruptAfter = $boundary
            try { $refusal = Get-Refusal { Complete-LcSessionOwnership $tx.Journal $tx.Scope $restore 0 } } finally { $script:LcInterruptAfter = $null }
            Assert-That ($refusal -like "Fixture interruption after $boundary*") "interrupted-after-$boundary"
            Assert-That ($global:LcTestRestores -eq $restoresBefore + 1) "restored-once-before-$boundary"
            $tx.Scope.Lease.Stream.Dispose()
            Set-FixtureOwnerGone
            if ($boundary -eq 'shared-released') {
                [IO.File]::WriteAllText($peerLock, 'sibling')
                Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Recovery withheld*' -and (Test-Path -LiteralPath $script:LcLock) -and
                    (Test-Path -LiteralPath $script:LcJournal) -and (Get-Content -LiteralPath $peerLock -Raw) -ceq 'sibling') 'shared-released-recovery-preserves-foreign-state'
                Remove-Item -LiteralPath $peerLock -Force
            }
            Assert-That ((Invoke-LcRecovery $restore) -like 'Recovered*') "recovered-after-$boundary"
            Assert-That ($global:LcTestRestores -eq $restoresBefore + 1) "no-duplicate-restore-after-$boundary"
            Assert-That ((& $clean) -and (Get-FixtureLease $tx.Scope.Lease.StatePath).status -ceq 'Completed') "markers-cleared-after-$boundary"
        }

        # Older (round-7) journals get the same ownership checks: the lease owner must have exited.
        $txL = New-FixtureTransaction 'fixture-legacy'
        $txL.Scope.Lease.Stream.Dispose()
        Write-LcJson $script:LcJournal ([ordered]@{ sessionId = 'fixture-legacy'; registryBackup = $backup; leaseRunId = $txL.Scope.Lease.RunId; leaseStatePath = $txL.Scope.Lease.StatePath })
        $restoresBefore = $global:LcTestRestores
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'The original session owner*still alive*' -and $global:LcTestRestores -eq $restoresBefore) 'legacy-recovery-refused-live-owner'
        $legacyState = Get-FixtureLease $txL.Scope.Lease.StatePath
        $legacyState.processStartedUtc = '2000-01-01T00:00:00.0000000Z'
        [IO.File]::WriteAllText($txL.Scope.Lease.StatePath, ($legacyState | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
        Assert-That ((Invoke-LcRecovery $restore) -like 'Recovered*' -and $global:LcTestRestores -eq $restoresBefore + 1 -and (& $clean)) 'legacy-recovery-after-owner-exit'
    }
    finally {
        $script:LcInterruptAfter = $null
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcGameProcessQuery = $saved.Query
        $script:LcSavesManifest = $saved.Saves; $script:LcInstallManifest = $saved.Install
        Remove-Variable -Name LcTestGames, LcTestRestores, LcTestSaves, LcTestInstall, LcTestCompetitor -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 74) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
