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

    # Shared ownership, per-launch host checks and safe-boundary restoration, with a disposable state root, lock
    # files, journal and a substituted process query standing in for a peer Kingmaker process.
    $saved = @{ State = $script:LcStateRoot; Foreign = $script:LcForeignLocks; Journal = $script:LcJournal; Lock = $script:LcLock; Query = $script:LcGameProcessQuery }
    try {
        $state = Join-Path $fixture 'compatibility-state'
        $compatLock = Join-Path $state 'compatibility.lock'
        $peerLock = Join-Path $fixture 'sibling-lab\runtime.lock'
        New-Item -ItemType Directory -Force -Path (Split-Path $peerLock) | Out-Null
        $script:LcStateRoot = $state
        $script:LcForeignLocks = @($compatLock, $peerLock)
        $script:LcJournal = Join-Path $copies 'loader-compat.journal.json'
        $script:LcLock = Join-Path $copies 'loader-compat.lock'
        $global:LcTestGames = @()
        $script:LcGameProcessQuery = { @($global:LcTestGames) }
        $global:LcTestRestores = 0
        $restore = { param($Path) $global:LcTestRestores++ }

        # A foreign holder of the shared lock is refused and never overwritten.
        New-Item -ItemType Directory -Force -Path $state | Out-Null
        [IO.File]::WriteAllText($compatLock, 'foreign-run')
        Assert-That ((Get-Refusal { Enter-LcRuntimeOwnership 'fixture-refused' }) -like '*lock already exists*') 'foreign-shared-lock-refused'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq 'foreign-run') 'foreign-shared-lock-not-overwritten'
        Remove-Item -LiteralPath $compatLock -Force

        # Session A: launch one admitted, then a peer appears and launch two is withheld.
        $scope = Enter-LcRuntimeOwnership 'fixture-session-a'
        $lease = $scope.Lease
        Enter-LcLock 'fixture-session-a'
        Assert-That ((Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $lease.RunId) 'shared-lock-owned-by-session'
        Assert-That ($null -eq (Get-Refusal { Assert-LcLaunchAvailable $lease })) 'launch-one-admitted-with-own-lock'
        $global:LcTestGames = @([pscustomobject]@{ Id = 4242; Name = 'Kingmaker' })
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $lease }) -like 'Launch withheld: Kingmaker is running (pid 4242)*') 'launch-two-withheld-for-peer-game'
        $global:LcTestGames = @()
        [IO.File]::WriteAllText($peerLock, 'sibling')
        Assert-That ((Get-Refusal { Assert-LcLaunchAvailable $lease }) -like "Launch withheld: another lab holds a runtime lock*runtime.lock") 'launch-withheld-for-sibling-lock'
        Remove-Item -LiteralPath $peerLock -Force

        # The peer is still running at the finalizer: no restoration, ownership and journal retained.
        $journal = [ordered]@{ sessionId = 'fixture-session-a'; registryBackup = (Join-Path $fixture 'settings.reg'); leaseRunId = $lease.RunId; leaseStatePath = $lease.StatePath }
        Write-LcJson $script:LcJournal $journal
        $global:LcTestGames = @([pscustomobject]@{ Id = 4242; Name = 'Kingmaker' })
        $outcome = Complete-LcSessionOwnership $journal $scope $restore 0
        Assert-That ($outcome.Outcome -eq 'recovery-required') 'contention-finalizer-defers'
        Assert-That ($global:LcTestRestores -eq 0) 'contention-registry-not-restored'
        Assert-That ((Test-Path -LiteralPath $compatLock) -and (Get-Content -LiteralPath $compatLock -Raw).Trim() -ceq $lease.RunId) 'contention-shared-lock-retained'
        Assert-That ((Read-KmgCompatibilityJson $lease.StatePath).status -ceq 'RecoveryRequired') 'contention-lease-recovery-required'
        Assert-That ((Test-Path -LiteralPath $script:LcJournal) -and (Read-KmgCompatibilityJson $script:LcJournal).recoveryRequired) 'contention-journal-retained'
        Assert-That (Test-Path -LiteralPath $script:LcLock) 'contention-loader-compat-lock-retained'

        # Recovery refuses while the peer runs and changes nothing.
        Assert-That ((Get-Refusal { Invoke-LcRecovery $restore }) -like 'Kingmaker is running*') 'recovery-refused-while-peer-runs'
        Assert-That ($global:LcTestRestores -eq 0 -and (Test-Path -LiteralPath $script:LcJournal) -and (Test-Path -LiteralPath $compatLock)) 'refused-recovery-changed-nothing'

        # The peer exits and the session process is gone (its recorded start time no longer matches): recovery
        # restores once, completes the lease and releases every owned lock and the journal.
        $global:LcTestGames = @()
        $leaseState = Read-KmgCompatibilityJson $lease.StatePath
        $leaseState.processStartedUtc = '2000-01-01T00:00:00.0000000Z'
        [IO.File]::WriteAllText($lease.StatePath, ($leaseState | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
        Assert-That ((Invoke-LcRecovery $restore) -like 'Recovered interrupted session fixture-session-a*') 'recovery-completes'
        Assert-That ($global:LcTestRestores -eq 1) 'recovery-restored-once'
        Assert-That (-not (Test-Path -LiteralPath $compatLock)) 'recovery-released-shared-lock'
        Assert-That ((Read-KmgCompatibilityJson $lease.StatePath).status -ceq 'Completed') 'recovery-lease-completed'
        Assert-That (-not (Test-Path -LiteralPath $script:LcJournal) -and -not (Test-Path -LiteralPath $script:LcLock)) 'recovery-released-journal-and-loader-lock'

        # Session B: normal completion restores once and releases the shared lock.
        $scopeB = Enter-LcRuntimeOwnership 'fixture-session-b'
        $journalB = [ordered]@{ sessionId = 'fixture-session-b'; registryBackup = (Join-Path $fixture 'settings.reg'); leaseRunId = $scopeB.Lease.RunId; leaseStatePath = $scopeB.Lease.StatePath }
        Write-LcJson $script:LcJournal $journalB
        $outcomeB = Complete-LcSessionOwnership $journalB $scopeB $restore 0
        Assert-That ($outcomeB.Outcome -eq 'restored' -and $global:LcTestRestores -eq 2) 'normal-completion-restores'
        Assert-That (-not (Test-Path -LiteralPath $compatLock) -and (Read-KmgCompatibilityJson $scopeB.Lease.StatePath).status -ceq 'Completed') 'normal-completion-releases-shared-lock'
        Assert-That (-not (Test-Path -LiteralPath $script:LcJournal)) 'normal-completion-removes-journal'
    }
    finally {
        $script:LcStateRoot = $saved.State; $script:LcForeignLocks = $saved.Foreign; $script:LcJournal = $saved.Journal
        $script:LcLock = $saved.Lock; $script:LcGameProcessQuery = $saved.Query
        Remove-Variable -Name LcTestGames, LcTestRestores -Scope Global -ErrorAction SilentlyContinue
    }
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 35) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
