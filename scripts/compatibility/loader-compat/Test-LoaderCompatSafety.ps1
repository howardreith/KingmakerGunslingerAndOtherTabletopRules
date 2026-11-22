[CmdletBinding()]
param()
# Offline checks of the loader-compatibility session guards and of the supported
# session contract: one new, exclusively owned session followed by its normal
# verified completion, and retention of the session on every failure. Temporary
# fixture directories, lock and journal files, a .reg text standing for the
# game-settings key, substituted owner-file manifests and a substituted process
# query only; no game, loader, registry or owner file is touched.
. (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$ran = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$checks = 0
function Assert-That([bool]$Condition, [string]$Name) { $script:checks++; [void]$script:ran.Add($Name); if (-not $Condition) { $failures.Add($Name) } }
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
        Query = $script:LcGameProcessQuery; Saves = $script:LcSavesManifest; Install = $script:LcInstallManifest
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
        $global:LcTestGames = @()
        # A transient peer: present for the next N process queries, then gone.
        $global:LcTestTransientPeer = 0
        $script:LcGameProcessQuery = {
            if ($global:LcTestTransientPeer -gt 0) { $global:LcTestTransientPeer--; return @([pscustomobject]@{ Id = 9090 }) }
            @($global:LcTestGames)
        }
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
        $hex = "01,02,03,\`r`n  04,05,06"
        $backupText = New-RegText 'owner' '1' '2' $hex
        $sessionText = New-RegText 'session' '1' '2' $hex
        $global:LcTestRegistry = $sessionText
        $global:LcTestExports = 0
        $global:LcTestExportFailures = 0
        $script:LcRegistryExport = {
            param($Path)
            $global:LcTestExports++
            if ($global:LcTestExportFailures -gt 0) { $global:LcTestExportFailures--; throw 'registry key is unreadable (fixture access denied)' }
            if ($null -eq $global:LcTestRegistry) { return 'absent' }
            [IO.File]::WriteAllText($Path, $global:LcTestRegistry); return 'exported'
        }
        $global:LcTestRestores = 0
        $restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = [IO.File]::ReadAllText($Path) }
        $global:LcTestBaseRestore = $restore
        $backup = Join-Path $fixture 'settings.reg'
        [IO.File]::WriteAllText($backup, $backupText)
        $clean = { -not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcLock) -and -not (Test-Path -LiteralPath $script:LcJournal) }
        function Get-FixtureJson([string]$Path) { return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json }
        function New-FixtureTransaction([string]$Id, [switch]$NoBackup) {
            # State left by an earlier case that should have released is itself a failure; clear it so later cases still run.
            if (-not (& $clean)) {
                Assert-That $false ('state-leaked-before-' + $Id)
                foreach ($path in @($script:LcJournal, $compatLock, $script:LcLock)) { if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force } }
            }
            $global:LcTestRegistry = $sessionText
            $tx = Enter-LcSessionTransaction $Id
            [void](Save-LcIntake $tx.Journal)
            if (-not $NoBackup) { Set-LcRegistryBackup $tx.Journal $backup }
            return $tx
        }
        # Every file under the fixture's state root, game-copies root and sibling lab, with its bytes' digest,
        # plus the game settings and the import count: anything a refused start could have changed.
        function Get-FixtureSnapshot {
            $lines = @(foreach ($root in @($state, $copies, (Split-Path $peerLock))) {
                Get-ChildItem -LiteralPath $root -Recurse -Force | Sort-Object FullName | ForEach-Object {
                    if ($_.PSIsContainer) { 'dir|' + $_.FullName } else { 'file|' + $_.FullName + '|' + (Get-LcSha256 $_.FullName) }
                }
            })
            $lines += 'registry|' + $(if ($null -eq $global:LcTestRegistry) { '<absent>' } else { Get-LcBytesSha256 ([Text.Encoding]::UTF8.GetBytes($global:LcTestRegistry)) })
            $lines += 'restores|' + $global:LcTestRestores
            return ($lines -join "`n")
        }
        # A retained session: its journal (phase retained, with the backup, its intake digest and the reason), both
        # locks naming it, its backup unchanged on disk and its lease RecoveryRequired naming the transaction.
        function Test-Retained($Tx) {
            if (-not (Test-Path -LiteralPath $script:LcJournal)) { return $false }
            $j = Get-FixtureJson $script:LcJournal
            $lease = Get-FixtureJson $Tx.Scope.Lease.StatePath
            return $j.transactionId -ceq $Tx.Journal.transactionId -and $j.phase -ceq 'retained' -and $j.reason -and
                $j.registryBackup -ceq $Tx.Journal.registryBackup -and $j.registryBackupSha256 -ceq $Tx.Journal.registryBackupSha256 -and
                (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $Tx.Scope.Lease.RunId -and (Test-LcPrivateLockOwned $Tx.Journal.sessionId) -and
                $lease.status -ceq 'RecoveryRequired' -and $lease.recoveryRequired -and ([string]$lease.reason).Contains($Tx.Journal.transactionId) -and
                (-not $Tx.Journal.registryBackup -or (Get-LcSha256 $Tx.Journal.registryBackup) -ceq $Tx.Journal.registryBackupSha256)
        }
        # A start that must be refused: its refusal text, or - if it unexpectedly started - $null after its claims
        # were dropped, so one regression is reported as a failed check rather than aborting every later case.
        function Get-StartRefusal([string]$Id) {
            try { $unexpected = Enter-LcSessionTransaction $Id }
            catch { return $_.Exception.Message }
            if ($null -ne $unexpected.Scope.Lease.Stream) { $unexpected.Scope.Lease.Stream.Dispose() }
            foreach ($path in @($script:LcJournal, $compatLock, $script:LcLock)) { if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force } }
            return $null
        }
        # A later session refuses the retained state and changes nothing.
        function Test-LaterSessionRefused {
            $beforeRefusal = Get-FixtureSnapshot
            $refusal = Get-StartRefusal 'fixture-later-session'
            return $refusal -like 'Refusing to start a session; nothing was changed*pending session journal exists*' -and (Get-FixtureSnapshot) -ceq $beforeRefusal
        }
        # Fixture stand-in for the documented manual procedure, so the next case starts from a free host.
        function Clear-FixtureRetained {
            foreach ($path in @($script:LcJournal, $compatLock, $script:LcLock)) { if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force } }
            [IO.File]::WriteAllText($backup, $backupText)
            $global:LcTestGames = @(); $global:LcTestSaves = $defaultSaves; $global:LcTestExportFailures = 0
        }

        # --- inputs: refused before anything is acquired
        Assert-That ((Get-Refusal { Assert-LcSessionInputs (Join-Path $fixture 'missing.zip') $fixture }) -like 'Candidate ZIP not found*Nothing was acquired.') 'missing-candidate-refused'
        $zip = Join-Path $fixture 'candidate.zip'; [IO.File]::WriteAllText($zip, 'zip')
        Assert-That ((Get-Refusal { Assert-LcSessionInputs $zip (Join-Path $fixture 'no-harness') }) -like 'Harness file missing*') 'missing-harness-refused'
        Assert-That (& $clean) 'refused-inputs-acquired-nothing'

        # --- pre-existing state: refused by the preflight and by the transaction itself, never taken over or cleaned
        $preexisting = @(
            @{ Name = 'game-process'; Expect = '*Kingmaker is running (pid 4242)*'; Setup = { $global:LcTestGames = @([pscustomobject]@{ Id = 4242 }) } }
            @{ Name = 'shared-lock'; Expect = '*a runtime lock exists:*compatibility.lock*'; Setup = { [IO.File]::WriteAllText($compatLock, "runtime-foreign`r`n") } }
            @{ Name = 'sibling-lock'; Expect = '*a runtime lock exists:*runtime.lock*'; Setup = { [IO.File]::WriteAllText($peerLock, 'sibling') } }
            @{ Name = 'game-copy-lock'; Expect = '*the game-copy lock exists:*(session older-session)*'; Setup = { [IO.File]::WriteAllText($script:LcLock, "session=older-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n") } }
            @{ Name = 'pending-journal'; Expect = '*a pending session journal exists:*(transaction pending-tx)*'; Setup = { Write-LcJson $script:LcJournal ([ordered]@{ schemaVersion = 5; transactionId = 'pending-tx'; phase = 'retained' }) } }
            @{ Name = 'older-journal'; Expect = '*a pending session journal exists:*(transaction old-schema4)*'; Setup = { Write-LcJson $script:LcJournal ([ordered]@{ schemaVersion = 4; transactionId = 'old-schema4'; phase = 'recovery-required'; registryRestore = 'restoring' }) } }
            @{ Name = 'legacy-journal'; Expect = '*a pending session journal exists:*(transaction <none>)*'; Setup = { Write-LcJson $script:LcJournal ([ordered]@{ sessionId = 'pre-round-7'; registryBackup = $backup }) } }
            @{ Name = 'unreadable-journal'; Expect = '*a pending session journal exists:*(transaction <unreadable>)*'; Setup = { [IO.File]::WriteAllText($script:LcJournal, '{ not json') } }
        )
        foreach ($case in $preexisting) {
            & $case.Setup
            $beforeStart = Get-FixtureSnapshot
            $leasesBefore = @(Get-ChildItem -LiteralPath $state -Directory).Count
            $refusal = Get-StartRefusal ('fixture-refused-' + $case.Name)
            $preflight = Get-Refusal { Assert-LcPreflight }
            Assert-That ($refusal -like 'Refusing to start a session; nothing was changed*' -and $refusal -like $case.Expect -and $refusal -like '*Manual recovery*' -and
                (Get-FixtureSnapshot) -ceq $beforeStart -and @(Get-ChildItem -LiteralPath $state -Directory).Count -eq $leasesBefore) ('preexisting-' + $case.Name + '-refused-without-mutation')
            Assert-That ($preflight -like $case.Expect -and (Get-FixtureSnapshot) -ceq $beforeStart) ('preflight-' + $case.Name + '-refused')
            Clear-FixtureRetained
            if (Test-Path -LiteralPath $peerLock) { Remove-Item -LiteralPath $peerLock -Force }
        }
        Assert-That (& $clean) 'preexisting-cases-left-free-host'

        # --- atomic claims and rollback of only this call's own claims
        $claimPath = Join-Path $fixture 'claim.txt'; [IO.File]::WriteAllText($claimPath, 'first')
        $claimRefusal = Get-Refusal { Publish-LcClaim $claimPath 'second' 'claim exists' }
        Assert-That ($claimRefusal -ceq 'claim exists' -and [IO.File]::ReadAllText($claimPath) -ceq 'first' -and
            @(Get-ChildItem -LiteralPath $fixture -Force -Filter '.claim.txt.*.claim').Count -eq 0) 'claim-refuses-existing-and-leaves-it-intact'
        $script:LcLeaseStateWriter = { param($Lease) throw 'lease state disk full' }
        Assert-That ((Get-Refusal { Enter-LcSessionTransaction 'fixture-lease-write' }) -like '*lease state disk full*' -and (& $clean)) 'lease-write-failure-releases-own-claims'
        $global:LcTestRaceLease = $null
        $script:LcLeaseStateWriter = {
            param($Lease)
            Write-KmgRuntimeLeaseState $Lease; $global:LcTestRaceLease = $Lease.StatePath
            [IO.File]::WriteAllText($script:LcLock, "session=racing-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n")
        }
        $raceRefusal = Get-Refusal { Enter-LcSessionTransaction 'fixture-private-race' }
        $script:LcLeaseStateWriter = $saved.Writer
        Assert-That ($raceRefusal -like '*loader-compat lock already exists*' -and -not (Test-Path -LiteralPath $compatLock) -and -not (Test-Path -LiteralPath $script:LcJournal) -and
            (Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=racing-session*' -and (Get-FixtureJson $global:LcTestRaceLease).status -ceq 'Completed') 'game-copy-lock-race-releases-only-own-claims'
        Remove-Item -LiteralPath $script:LcLock -Force
        $txWin = New-FixtureTransaction 'fixture-winner'
        $winnerJournal = Get-Content -LiteralPath $script:LcJournal -Raw
        Assert-That ((Get-StartRefusal 'fixture-loser') -like '*pending session journal exists*' -and (Get-Content -LiteralPath $script:LcJournal -Raw) -ceq $winnerJournal -and
            (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txWin.Scope.Lease.RunId -and (Test-LcPrivateLockOwned 'fixture-winner')) 'second-session-refused-while-first-owns'

        # --- normal completion: verified restoration, verified owner files, then release
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $txWin.Scope.Lease $txWin.Journal })) 'launch-admitted-for-owned-session'
        $restoresBefore = $global:LcTestRestores; $exportsBefore = $global:LcTestExports
        $result = Complete-LcSessionOwnership $txWin.Journal $txWin.Scope $restore
        $leaseState = Get-FixtureJson $txWin.Scope.Lease.StatePath
        Assert-That ((Test-LcTransactionSucceeded $result) -and $result.Registry -ceq 'applied' -and $result.RegistryVerification.status -ceq 'verified' -and
            $global:LcTestRestores -eq $restoresBefore + 1 -and $global:LcTestExports -eq $exportsBefore + 1 -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'normal-completion-verified-and-released'
        Assert-That ($leaseState.status -ceq 'Completed' -and -not $leaseState.recoveryRequired -and $leaseState.registryRestore -ceq 'applied' -and
            $leaseState.registryVerification.status -ceq 'verified' -and $leaseState.integrity.status -ceq 'verified') 'normal-completion-lease-records-verdict'
        $leaseDir = Split-Path -Parent $txWin.Scope.Lease.StatePath
        Assert-That ((Test-Path -LiteralPath $backup) -and @(Get-ChildItem -LiteralPath $leaseDir -Filter 'registry-backup-verified-*').Count -eq 0 -and
            -not $txWin.Scope.Lease.Stream.CanRead) 'normal-completion-keeps-backup-and-drops-verified-copy'
        $txNext = New-FixtureTransaction 'fixture-next'
        Assert-That ((Test-LcTransactionSucceeded (Complete-LcSessionOwnership $txNext.Journal $txNext.Scope $restore)) -and (& $clean)) 'next-session-after-normal-completion'

        # --- conflicts before a later launch and immediately before restoration
        $txSb = New-FixtureTransaction 'fixture-sibling'
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $txSb.Scope.Lease $txSb.Journal })) 'launch-one-admitted'
        $global:LcTestGames = @([pscustomobject]@{ Id = 7070 })
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $txSb.Scope.Lease $txSb.Journal }) -like 'Launch withheld: Kingmaker is running (pid 7070)*') 'later-launch-withheld-for-game-process'
        $global:LcTestGames = @()
        [IO.File]::WriteAllText($peerLock, 'sibling')
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $txSb.Scope.Lease $txSb.Journal }) -like 'Launch withheld: another lab holds a runtime lock*runtime.lock*') 'later-launch-withheld-for-sibling-lock'
        # The completion below must show retention for a conflict still present at completion, so this case clears the
        # launch-time records it just made (the persisted launch-conflict path has its own cases).
        $txSb.Journal.hostConflict = $null; Save-LcJournal $txSb.Journal
        $restoresBefore = $global:LcTestRestores
        $result = Complete-LcSessionOwnership $txSb.Journal $txSb.Scope $restore
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Reason -like '*Not at a safe boundary*runtime.lock*' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $sessionText -and (Test-Retained $txSb) -and (Get-Content -LiteralPath $peerLock -Raw) -ceq 'sibling' -and
            -not (Test-LcTransactionSucceeded $result)) 'sibling-lock-at-completion-retains-without-restore'
        Assert-That (Test-LaterSessionRefused) 'retained-boundary-refused-by-later-session'
        Remove-Item -LiteralPath $peerLock -Force; Clear-FixtureRetained
        # A later launch observes a peer; the peer changes the settings and has exited before completion. The observed
        # conflict persists: no import, the peer's value is preserved, the session is retained and the verdict fails.
        $txGone = New-FixtureTransaction 'fixture-peer-gone-before-completion'
        $launchesGone = @([pscustomobject]@{ verdict = 'PASS' })
        $global:LcTestGames = @([pscustomobject]@{ Id = 8080 })
        $withheldGone = Get-Refusal { Assert-LcLaunchAvailable $txGone.Scope.Lease $txGone.Journal }
        $launchesGone += [pscustomobject]@{ verdict = 'withheld' }
        $peerChanged = New-RegText 'peer-changed-then-exited' '7' '8' $hex
        $global:LcTestRegistry = $peerChanged; $global:LcTestGames = @()
        $restoresBefore = $global:LcTestRestores
        $result = Complete-LcSessionOwnership $txGone.Journal $txGone.Scope $restore
        Assert-That ($withheldGone -like 'Launch withheld: Kingmaker is running (pid 8080)*' -and $result.Outcome -ceq 'retained' -and
            $result.Reason -like '*host conflict was observed during this session at launch*pid 8080*' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $peerChanged -and (Test-Retained $txGone) -and (Get-FixtureJson $script:LcJournal).hostConflict.at -ceq 'launch' -and
            (Get-LcSessionVerdict $launchesGone $false $result).ExitCode -eq 1 -and
            (Get-LcSessionVerdict @([pscustomobject]@{ verdict = 'PASS' }) $false $result).ExitCode -eq 1) 'peer-seen-at-launch-gone-by-completion-retained-no-import'
        Assert-That (Test-LaterSessionRefused) 'peer-seen-at-launch-refused-by-later-session'
        Clear-FixtureRetained
        # Completion is the first to observe a peer, which disappears straight afterwards: retained at once, never
        # waited out and imported; all launches passed, yet the verdict fails.
        $txFirst = New-FixtureTransaction 'fixture-peer-first-seen-at-completion'
        $peerChanged = New-RegText 'peer-at-completion' '6' '6' $hex
        $global:LcTestRegistry = $peerChanged
        $global:LcTestTransientPeer = 1
        $restoresBefore = $global:LcTestRestores
        $result = Complete-LcSessionOwnership $txFirst.Journal $txFirst.Scope $restore
        $peerGoneAfter = $global:LcTestTransientPeer -eq 0 -and (Get-LcHostConflicts $txFirst.Journal.leaseRunId).Count -eq 0
        Assert-That ($peerGoneAfter -and $result.Outcome -ceq 'retained' -and $result.Reason -like '*Not at a safe boundary*pid 9090*' -and
            $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $peerChanged -and (Test-Retained $txFirst) -and
            (Get-FixtureJson $script:LcJournal).hostConflict.at -ceq 'completion' -and
            (Get-LcSessionVerdict @([pscustomobject]@{ verdict = 'PASS' }, [pscustomobject]@{ verdict = 'PASS' }) $false $result).ExitCode -eq 1) 'peer-first-seen-at-completion-then-gone-retained-no-import'
        # The retained session stays retained after the peer has gone: a second completion attempt still imports nothing.
        $again = Complete-LcSessionOwnership $txFirst.Journal $txFirst.Scope $restore
        Assert-That ($again.Outcome -ceq 'retained' -and $global:LcTestRestores -eq $restoresBefore -and $global:LcTestRegistry -ceq $peerChanged) 'conflict-decision-persists-after-peer-left'
        Clear-FixtureRetained
        $txPv = New-FixtureTransaction 'fixture-peer-before-restore'
        $restoresBefore = $global:LcTestRestores
        $script:LcBackupVerifyHook = { $global:LcTestGames = @([pscustomobject]@{ Id = 6565 }) }
        $result = Complete-LcSessionOwnership $txPv.Journal $txPv.Scope $restore
        $script:LcBackupVerifyHook = $null
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Reason -like '*withheld*pid 6565*' -and $result.Registry -ceq 'withheld' -and $global:LcTestRestores -eq $restoresBefore -and
            $global:LcTestRegistry -ceq $sessionText -and (Test-Retained $txPv)) 'peer-immediately-before-restore-withholds-and-retains'
        Clear-FixtureRetained
        $txFc = New-FixtureTransaction 'fixture-foreign-claim-before-restore'
        $restoresBefore = $global:LcTestRestores
        $script:LcBackupVerifyHook = { [IO.File]::WriteAllText($script:LcLock, "session=another-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n") }
        $result = Complete-LcSessionOwnership $txFc.Journal $txFc.Scope $restore
        $script:LcBackupVerifyHook = $null
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Reason -like '*withheld*game-copy lock no longer names this session*' -and $global:LcTestRestores -eq $restoresBefore -and
            (Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=another-session*' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txFc.Scope.Lease.RunId) 'foreign-claim-before-restore-withholds-and-keeps-it'
        Clear-FixtureRetained
        $txLl = New-FixtureTransaction 'fixture-lease-lost'
        $txLl.Scope.Lease.Stream.Dispose()
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $txLl.Scope.Lease $txLl.Journal }) -like '*lease is missing, foreign, closed*') 'launch-withheld-after-lease-handle-lost'
        Assert-That ((Test-LcTransactionSucceeded (Complete-LcSessionOwnership $txLl.Journal $txLl.Scope $restore)) -and (& $clean)) 'lease-handle-loss-still-completes-with-verified-restore'
        $txNb = New-FixtureTransaction 'fixture-no-backup' -NoBackup
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $txNb.Scope.Lease $txNb.Journal }) -like '*no registry backup bound to an intake digest*') 'launch-withheld-without-digest-bound-backup'
        $restoresBefore = $global:LcTestRestores
        $result = Complete-LcSessionOwnership $txNb.Journal $txNb.Scope $restore
        Assert-That ((Test-LcTransactionSucceeded $result) -and $result.Registry -ceq 'not-needed' -and $global:LcTestRestores -eq $restoresBefore -and (& $clean)) 'unlaunched-session-without-backup-completes-not-needed'

        # --- missing, changed, unreadable or digestless backup: retained, nothing imported
        $backupCases = @(
            @{ Name = 'missing'; Expect = '*cannot be read*'; Setup = { param($Tx) Remove-Item -LiteralPath $backup -Force } }
            @{ Name = 'changed'; Expect = '*no longer matches the digest recorded at intake*'; Setup = { param($Tx) [IO.File]::WriteAllText($backup, (New-RegText 'tampered' '9' '9' $hex)) } }
            @{ Name = 'unreadable'; Expect = '*cannot be read*'; Setup = { param($Tx) $global:LcTestHold = [IO.File]::Open($backup, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None) } }
            @{ Name = 'digestless'; Expect = '*no digest recorded at intake*'; Setup = { param($Tx) $Tx.Journal.registryBackupSha256 = $null; Save-LcJournal $Tx.Journal } }
        )
        foreach ($case in $backupCases) {
            $tx = New-FixtureTransaction ('fixture-backup-' + $case.Name)
            & $case.Setup $tx
            $restoresBefore = $global:LcTestRestores
            try { $result = Complete-LcSessionOwnership $tx.Journal $tx.Scope $restore }
            finally { if ($null -ne (Get-Variable -Name LcTestHold -Scope Global -ErrorAction SilentlyContinue) -and $null -ne $global:LcTestHold) { $global:LcTestHold.Dispose(); $global:LcTestHold = $null } }
            $journalNow = Get-FixtureJson $script:LcJournal
            Assert-That ($result.Outcome -ceq 'retained' -and $result.Registry -ceq 'refused' -and $result.Reason -like $case.Expect -and $global:LcTestRestores -eq $restoresBefore -and
                $global:LcTestRegistry -ceq $sessionText -and $journalNow.phase -ceq 'retained' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $tx.Scope.Lease.RunId -and
                (Test-LcPrivateLockOwned $tx.Journal.sessionId) -and (Get-FixtureJson $tx.Scope.Lease.StatePath).status -ceq 'RecoveryRequired') ('backup-' + $case.Name + '-retained-no-import')
            Assert-That (Test-LaterSessionRefused) ('backup-' + $case.Name + '-refused-by-later-session')
            Clear-FixtureRetained
        }
        # The backup replaced after its single verified read: the verified bytes are imported and compared.
        $txRv = New-FixtureTransaction 'fixture-backup-replaced-during-verification'
        $script:LcBackupVerifyHook = { [IO.File]::WriteAllText($backup, (New-RegText 'swapped-in' '5' '5' $hex)) }
        $result = Complete-LcSessionOwnership $txRv.Journal $txRv.Scope $restore
        $script:LcBackupVerifyHook = $null
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ((Test-LcTransactionSucceeded $result) -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'backup-replaced-during-verification-verified-bytes-used'
        # The original backup replaced during the import: the import reads the held verified copy, which denies writes.
        $txRi = New-FixtureTransaction 'fixture-backup-replaced-during-import'
        $global:LcTestCopyWrite = $null
        $replacingRestore = {
            param($Path)
            [IO.File]::WriteAllText($backup, (New-RegText 'replaced-during-import' '5' '5' $hex))
            try { [IO.File]::WriteAllText($Path, 'overwritten'); $global:LcTestCopyWrite = 'written' } catch { $global:LcTestCopyWrite = 'denied' }
            & $global:LcTestBaseRestore $Path
        }
        $result = Complete-LcSessionOwnership $txRi.Journal $txRi.Scope $replacingRestore
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ($global:LcTestCopyWrite -ceq 'denied' -and (Test-LcTransactionSucceeded $result) -and $global:LcTestRegistry -ceq $backupText -and (& $clean)) 'verified-copy-held-against-writes-during-import'

        # --- failed import, and a successful import whose readback does not match exactly
        $swapped = New-RegText 'owner' '2' '1' $hex
        $composed = 'caf' + [char]0x00E9; $decomposed = 'cafe' + [char]0x0301
        $importCases = @(
            @{ Name = 'import-throws'; Registry = 'failed'; Expect = '*restoration failed: reg import failed (fixture)*'
                Restore = { param($Path) $global:LcTestRestores++; throw 'reg import failed (fixture)' } }
            @{ Name = 'import-fails-after-delete'; Registry = 'failed'; Expect = '*may be partly restored*'
                Restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = $null; throw 'import failed after the delete (fixture)' } }
            @{ Name = 'readback-mismatch'; Registry = 'unverified'; Expect = '*not verified (mismatch)*'; Verification = 'mismatch'
                Restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = $swapped } }
            @{ Name = 'readback-unicode-spelling'; Registry = 'unverified'; Expect = '*not verified (mismatch)*'; Verification = 'mismatch'; Backup = (New-RegText $composed '1' '2' $hex)
                Restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = New-RegText $decomposed '1' '2' $hex } }
            @{ Name = 'readback-unreadable'; Registry = 'unverified'; Expect = '*not verified (unreadable)*'; Verification = 'unreadable'
                Restore = { param($Path) & $global:LcTestBaseRestore $Path; $global:LcTestExportFailures = 1 } }
            @{ Name = 'readback-absent'; Registry = 'unverified'; Expect = '*not verified (absent)*'; Verification = 'absent'
                Restore = { param($Path) $global:LcTestRestores++; $global:LcTestRegistry = $null } }
        )
        foreach ($case in $importCases) {
            if ($case.ContainsKey('Backup')) { [IO.File]::WriteAllText($backup, $case.Backup) }
            $tx = New-FixtureTransaction ('fixture-' + $case.Name)
            $restoresBefore = $global:LcTestRestores
            $result = Complete-LcSessionOwnership $tx.Journal $tx.Scope $case.Restore
            $verificationOk = -not $case.ContainsKey('Verification') -or ($result.RegistryVerification.status -ceq $case.Verification -and
                ($case.Verification -cne 'mismatch' -or (Test-Path -LiteralPath $result.RegistryVerification.path)))
            Assert-That ($result.Outcome -ceq 'retained' -and $result.Registry -ceq $case.Registry -and $result.Reason -like $case.Expect -and $verificationOk -and
                $global:LcTestRestores -eq $restoresBefore + 1 -and (Test-Retained $tx) -and -not (Test-LcTransactionSucceeded $result)) ($case.Name + '-retained')
            Assert-That (Test-LaterSessionRefused) ($case.Name + '-refused-by-later-session')
            Clear-FixtureRetained
        }

        # --- owner-file verification before release
        $txOc = New-FixtureTransaction 'fixture-owner-changed'
        $global:LcTestSaves = { @('saves\a.zks|10|aa', 'saves\b.zks|21|changed-during-session') }
        $result = Complete-LcSessionOwnership $txOc.Journal $txOc.Scope $restore
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Registry -ceq 'applied' -and $result.Integrity.status -ceq 'changed' -and $result.Reason -like '*not verified unchanged (changed*' -and
            (Test-Retained $txOc)) 'changed-owner-files-retained-after-verified-restore'
        Clear-FixtureRetained
        $txIt = New-FixtureTransaction 'fixture-intake-tampered'
        [IO.File]::AppendAllText($txIt.Journal.intake.savesPath, "saves\injected.zks|1|zz`r`n")
        $result = Complete-LcSessionOwnership $txIt.Journal $txIt.Scope $restore
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Integrity.status -ceq 'unresolved' -and $result.Reason -like '*intake manifest changed*' -and (Test-Retained $txIt)) 'changed-intake-unresolved-retained'
        Clear-FixtureRetained
        $txCa = New-FixtureTransaction 'fixture-competitor-during-verification'
        $global:LcTestCompetitor = $null
        $global:LcTestSaves = { try { [void](Enter-LcRuntimeOwnership 'fixture-competitor'); $global:LcTestCompetitor = 'acquired' } catch { $global:LcTestCompetitor = $_.Exception.Message }; & $defaultSaves }
        $result = Complete-LcSessionOwnership $txCa.Journal $txCa.Scope $restore
        $global:LcTestSaves = $defaultSaves
        Assert-That ($global:LcTestCompetitor -like '*lock already exists*' -and (Test-LcTransactionSucceeded $result) -and (& $clean)) 'competing-acquisition-refused-during-verification'
        $txHb = New-FixtureTransaction 'fixture-host-busy-after-verification'
        $global:LcTestSaves = { $global:LcTestGames = @([pscustomobject]@{ Id = 5151 }); & $defaultSaves }
        $result = Complete-LcSessionOwnership $txHb.Journal $txHb.Scope $restore
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Registry -ceq 'applied' -and $result.Reason -like '*changed after verification*pid 5151*' -and (Test-Retained $txHb)) 'host-busy-after-verification-retained'
        Clear-FixtureRetained
        $txFr = New-FixtureTransaction 'fixture-foreign-claim-before-release'
        $global:LcTestSaves = { [IO.File]::WriteAllText($script:LcLock, "session=another-session`npid=1`nstartedUtc=2000-01-01T00:00:00Z`n"); & $defaultSaves }
        $result = Complete-LcSessionOwnership $txFr.Journal $txFr.Scope $restore
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Reason -like '*game-copy lock no longer names this session*' -and
            (Get-Content -LiteralPath $script:LcLock -Raw) -like 'session=another-session*' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txFr.Scope.Lease.RunId -and
            (Test-Path -LiteralPath $script:LcJournal)) 'foreign-claim-before-release-retained-and-kept'
        Clear-FixtureRetained
        $txUe = New-FixtureTransaction 'fixture-unexpected-completion-error'
        $script:LcLeaseStateWriter = { param($Lease) throw 'lease record write failed (fixture)' }
        $result = Complete-LcSessionOwnership $txUe.Journal $txUe.Scope $restore
        $script:LcLeaseStateWriter = $saved.Writer
        Assert-That ($result.Outcome -ceq 'retained' -and $result.Reason -like '*Completion stopped unexpectedly: lease record write failed*' -and
            (Get-FixtureJson $script:LcJournal).phase -ceq 'retained' -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $txUe.Scope.Lease.RunId -and
            (Test-LcPrivateLockOwned $txUe.Journal.sessionId)) 'unexpected-completion-error-retained'
        Assert-That (Test-LaterSessionRefused) 'unexpected-completion-error-refused-by-later-session'
        Clear-FixtureRetained

        # --- exact registry comparison: keys, value names, types and ordered data together, compared ordinally
        $regA = Join-Path $fixture 'reg-a.reg'; $regB = Join-Path $fixture 'reg-b.reg'
        [IO.File]::WriteAllText($regA, (New-RegText 'owner' '1' '2' $hex))
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '2' '1' $hex))
        Assert-That (-not (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB))) 'swapped-subkey-values-unequal'
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '1' '2' "04,05,06,\`r`n  01,02,03"))
        Assert-That (-not (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB))) 'reordered-multiline-data-unequal'
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '1' '2' $hex).Replace("`r`n`r`n", "`r`n"))
        Assert-That (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB)) 'same-settings-equal'
        [IO.File]::WriteAllText($regB, (New-RegText 'owner' '1' '2' "01,02,03,\`r`n  04,05,06").Replace('"layout"=hex:', '"layout"=hex(3):'))
        Assert-That (-not (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB))) 'value-type-difference-unequal'
        [IO.File]::WriteAllText($regA, (New-RegText $composed '1' '2' $hex))
        [IO.File]::WriteAllText($regB, (New-RegText $decomposed '1' '2' $hex))
        Assert-That (-not (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB))) 'unicode-spellings-unequal-ordinally'
        [IO.File]::WriteAllText($regB, (New-RegText $composed '1' '2' $hex), [Text.Encoding]::Unicode)
        Assert-That (Test-LcRegistryEqual (Get-LcRegistryCanonical $regA) (Get-LcRegistryCanonical $regB)) 'utf16-export-equals-same-data'
        # A UTF-16 backup (as reg.exe writes it) read back as the same data verifies end to end.
        [IO.File]::WriteAllText($backup, $backupText, [Text.Encoding]::Unicode)
        $txU16 = New-FixtureTransaction 'fixture-utf16-backup'
        $result = Complete-LcSessionOwnership $txU16.Journal $txU16.Scope $restore
        [IO.File]::WriteAllText($backup, $backupText)
        Assert-That ((Test-LcTransactionSucceeded $result) -and $result.RegistryVerification.status -ceq 'verified' -and (& $clean)) 'utf16-backup-restores-and-verifies'

        # --- session verdict: scenario and restoration reported separately, either failure exits 1
        $okTx = [pscustomobject]@{ Outcome = 'completed'; Registry = 'applied'; Integrity = [ordered]@{ status = 'verified' } }
        $retainedTx = [pscustomobject]@{ Outcome = 'retained'; Registry = 'unverified'; Integrity = $null }
        $passing = @([pscustomobject]@{ verdict = 'PASS' }, [pscustomobject]@{ verdict = 'PASS' })
        $failing = @([pscustomobject]@{ verdict = 'PASS' }, [pscustomobject]@{ verdict = 'FAIL' })
        $withheld = @([pscustomobject]@{ verdict = 'PASS' }, [pscustomobject]@{ verdict = 'withheld' })
        $v = Get-LcSessionVerdict $passing $false $okTx
        Assert-That ($v.Status -ceq 'PASS' -and $v.ExitCode -eq 0 -and $v.ScenarioPassed -and $v.TransactionSucceeded) 'verdict-pass-exits-0'
        $v = Get-LcSessionVerdict $failing $false $okTx
        Assert-That ($v.Status -ceq 'FAIL' -and $v.ExitCode -eq 1 -and -not $v.ScenarioPassed -and $v.TransactionSucceeded) 'verdict-scenario-failure-with-clean-restoration-exits-1'
        $v = Get-LcSessionVerdict $passing $false $retainedTx
        Assert-That ($v.Status -ceq 'integrity-failed' -and $v.ExitCode -eq 1 -and $v.ScenarioPassed -and -not $v.TransactionSucceeded) 'verdict-passing-scenario-cannot-hide-restoration-failure'
        $v = Get-LcSessionVerdict $failing $false $retainedTx
        Assert-That ($v.ExitCode -eq 1 -and -not $v.ScenarioPassed -and -not $v.TransactionSucceeded) 'verdict-both-failures-exit-1'
        Assert-That ((Get-LcSessionVerdict $withheld $false $okTx).ExitCode -eq 1) 'verdict-withheld-launch-exits-1'
        Assert-That ((Get-LcSessionVerdict $passing $true $okTx).Status -ceq 'host-error' -and (Get-LcSessionVerdict $passing $true $okTx).ExitCode -eq 1) 'verdict-host-error-exits-1'
        Assert-That ((Get-LcSessionVerdict @() $false $okTx).ExitCode -eq 1) 'verdict-zero-launches-exits-1'
        Assert-That ((Get-LcSessionVerdict $passing $false $null).ExitCode -eq 1) 'verdict-missing-transaction-result-exits-1'
        Assert-That (& $clean) 'all-fixture-transactions-released'
    }
    finally {
        $script:LcBackupVerifyHook = $null
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcGameProcessQuery = $saved.Query
        $script:LcSavesManifest = $saved.Saves; $script:LcInstallManifest = $saved.Install
        $script:LcRegistryExport = $saved.Export; $script:LcLeaseStateWriter = $saved.Writer
        Remove-Variable -Name LcTestGames, LcTestTransientPeer, LcTestRestores, LcTestExports, LcTestExportFailures, LcTestSaves, LcTestInstall, LcTestCompetitor, LcTestRegistry,
            LcTestBaseRestore, LcTestRaceLease, LcTestCopyWrite, LcTestHold -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
# The retained coverage of the supported contract: each named scenario must have executed (a skipped loop or an
# aborted section fails here), and a zero-check run is never a pass.
$requiredCoverage = @(
    'normal-completion-verified-and-released', 'normal-completion-lease-records-verdict', 'next-session-after-normal-completion',
    'preexisting-game-process-refused-without-mutation', 'preexisting-shared-lock-refused-without-mutation', 'preexisting-sibling-lock-refused-without-mutation',
    'preexisting-game-copy-lock-refused-without-mutation', 'preexisting-pending-journal-refused-without-mutation', 'preexisting-older-journal-refused-without-mutation',
    'preexisting-legacy-journal-refused-without-mutation', 'preexisting-unreadable-journal-refused-without-mutation', 'preflight-pending-journal-refused',
    'later-launch-withheld-for-game-process', 'later-launch-withheld-for-sibling-lock', 'sibling-lock-at-completion-retains-without-restore',
    'peer-immediately-before-restore-withholds-and-retains', 'peer-seen-at-launch-gone-by-completion-retained-no-import',
    'peer-first-seen-at-completion-then-gone-retained-no-import', 'conflict-decision-persists-after-peer-left',
    'backup-missing-retained-no-import', 'backup-changed-retained-no-import', 'backup-unreadable-retained-no-import', 'backup-digestless-retained-no-import',
    'backup-replaced-during-verification-verified-bytes-used', 'verified-copy-held-against-writes-during-import',
    'import-throws-retained', 'import-fails-after-delete-retained', 'readback-mismatch-retained', 'readback-unicode-spelling-retained',
    'readback-unreadable-retained', 'readback-absent-retained', 'readback-mismatch-refused-by-later-session',
    'swapped-subkey-values-unequal', 'reordered-multiline-data-unequal', 'value-type-difference-unequal', 'unicode-spellings-unequal-ordinally', 'utf16-export-equals-same-data',
    'changed-owner-files-retained-after-verified-restore', 'host-busy-after-verification-retained', 'unexpected-completion-error-retained',
    'verdict-scenario-failure-with-clean-restoration-exits-1', 'verdict-passing-scenario-cannot-hide-restoration-failure', 'verdict-pass-exits-0',
    'all-fixture-transactions-released')
$missing = @($requiredCoverage | Where-Object { -not $ran.Contains($_) })
if ($checks -eq 0 -or $missing.Count) { throw "Loader-compatibility safety checks did not run the required coverage ($checks checks; missing: $($missing -join ', '))." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed ($($failures.Count) of $checks): $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0 (required scenarios $($requiredCoverage.Count) of $($requiredCoverage.Count) executed). Offline fixtures only."
