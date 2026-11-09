[CmdletBinding()]
param()

# Behavior tests for the guarded launch's byte-exact UMM Params.xml
# protection, using temporary files, real temporary coordination leases and
# the shipped shared functions. No game is launched.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UmmParams.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$passes = 0
function Assert-True([bool]$Condition, [string]$Name) {
    if ($Condition) { $script:passes++ } else { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name, [string]$Pattern) {
    try { & $Action; $failures.Add($Name) }
    catch {
        if ($Pattern -and $_.Exception.Message -notmatch $Pattern) { $failures.Add("$Name (wrong refusal: $($_.Exception.Message))") }
        else { $script:passes++ }
    }
}

$root = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts\umm-params-tests'
if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
$intake = @'
<?xml version="1.0" encoding="utf-8"?>
<Param>
  <LastUpdateCheck>2026-11-07T18:00:00.0000000-05:00</LastUpdateCheck>
  <ModParams>
    <Mod Id="KingmakerGunslinger" Enabled="true" />
  </ModParams>
</Param>
'@
$intakeBytes = [Text.UTF8Encoding]::new($true).GetBytes($intake.Replace("`n", "`r`n"))
$intakeSha = Get-KmgUmmBytesSha256 $intakeBytes

# The repository's process-observation seam (as in Test-RuntimeLeaseLifecycle):
# one controlled answer for the literal name Kingmaker, seen alike by admission,
# the recheck under the lease and Exit-KmgRuntimeLease. No process is started
# and no real game is observed or touched.
$global:kmgUmmTest = @{ running = $false; appearOnceLockExists = $null }
function global:Get-Process {
    [CmdletBinding()]param([string]$Name, [int]$Id)
    if ($Name -ceq 'Kingmaker') {
        $lock = $global:kmgUmmTest.appearOnceLockExists
        if ($lock -and (Test-Path -LiteralPath $lock)) { $global:kmgUmmTest.running = $true }
        if ($global:kmgUmmTest.running) { [pscustomobject]@{ Id = 900001; ProcessName = 'Kingmaker'; StartTime = [DateTime]::UtcNow } }
        return
    }
    Microsoft.PowerShell.Management\Get-Process @PSBoundParameters
}
function Reset-Game { $global:kmgUmmTest.running = $false; $global:kmgUmmTest.appearOnceLockExists = $null }

function New-Case([string]$Name) {
    $case = Join-Path $root $Name
    $params = Join-Path $case 'game\Kingmaker_Data\Managed\UnityModManager\Params.xml'
    New-Item -ItemType Directory -Path (Split-Path $params -Parent) -Force | Out-Null
    [IO.File]::WriteAllBytes($params, $intakeBytes)
    return [pscustomobject]@{ Root = $case; Params = $params; Backups = (Join-Path $case 'backups'); State = (Join-Path $case 'state') }
}
function Set-Text([string]$Path, [string]$Old, [string]$New) {
    $text = [IO.File]::ReadAllText($Path)
    [IO.File]::WriteAllBytes($Path, [Text.UTF8Encoding]::new($true).GetBytes($text.Replace($Old, $New)))
}
function Test-LockFree($Case) { return -not (Test-Path -LiteralPath (Join-Path $Case.State 'compatibility.lock')) }
function Get-RunDirectoryCount($Case) { return @(Get-ChildItem -LiteralPath $Case.State -Directory).Count }
function Get-LeaseStatusCount($Case, [string]$Status) {
    return @(Get-ChildItem -LiteralPath $Case.State -Directory | ForEach-Object {
        Get-Content -LiteralPath (Join-Path $_.FullName 'runtime-lease.json') -Raw | ConvertFrom-Json } |
        Where-Object { $_.status -ceq $Status }).Count
}
function Test-LeaseIntact($Scope) {
    Assert-KmgRuntimeLease $Scope.Lease
    return ((Get-Content -LiteralPath $Scope.Lease.StatePath -Raw | ConvertFrom-Json).status -ceq 'Active')
}

# The wrapper's own sequence: a real runtime lease from the case's
# coordination root, the backup under it, then the lease released.
function Backup-Owned($Case) {
    $scope = Enter-KmgRuntimeLease -Purpose 'umm-params protection' -StateRoot $Case.State
    try { return Backup-KmgUmmParams -ParamsPath $Case.Params -BackupRoot $Case.Backups -OwnerLease $scope.Lease -Owner 'test' }
    finally { Exit-KmgRuntimeLease $scope }
}

# A separate owner process that backs up under its own lease and then exits
# (an interrupted wrapper). Optionally it changes Params.xml and runs the
# wrapper's cleanup with the destination locked, reporting the outcome.
$childScript = Join-Path $root 'child-owner.ps1'
function Invoke-ChildOwner($Case, [switch]$CleanupWithLockedParams) {
    $out = Join-Path $Case.Root ('child-' + [Guid]::NewGuid().ToString('N') + '.json')
    $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $childScript, '-ScriptsRoot', $PSScriptRoot,
        '-Params', $Case.Params, '-Backups', $Case.Backups, '-StateRoot', $Case.State, '-Output', $out)
    if ($CleanupWithLockedParams) { $arguments += '-CleanupWithLockedParams' }
    & powershell.exe @arguments | Out-Null
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $out)) { throw "Child owner failed ($LASTEXITCODE)." }
    return Get-Content -LiteralPath $out -Raw | ConvertFrom-Json
}

try {
    New-Item -ItemType Directory -Path $root | Out-Null
    [IO.File]::WriteAllText($childScript, @'
param([string]$ScriptsRoot, [string]$Params, [string]$Backups, [string]$StateRoot, [string]$Output, [switch]$CleanupWithLockedParams)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# The parent's process seam applies here too: a real Kingmaker from any other
# lab on this machine must not change this temporary fixture.
function global:Get-Process {
    [CmdletBinding()]param([string]$Name, [int]$Id)
    if ($Name -ceq 'Kingmaker') { return }
    Microsoft.PowerShell.Management\Get-Process @PSBoundParameters
}
. (Join-Path $ScriptsRoot 'UmmParams.Common.ps1')
$scope = Enter-KmgRuntimeLease -Purpose 'umm-params protection' -StateRoot $StateRoot
try { $receipt = Backup-KmgUmmParams -ParamsPath $Params -BackupRoot $Backups -OwnerLease $scope.Lease -Owner 'child' }
finally { Exit-KmgRuntimeLease $scope }
$result = [ordered]@{ receiptPath = $receipt.receiptPath; ownerPid = $PID; installationRan = $false; ummVerified = $null; ummError = $null }
if ($CleanupWithLockedParams) {
    $text = [IO.File]::ReadAllText($Params).Replace('Enabled="true"', 'Enabled="false"')
    [IO.File]::WriteAllBytes($Params, [Text.UTF8Encoding]::new($true).GetBytes($text))
    $lock = [IO.File]::Open($Params, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
    try {
        $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $receipt -ExitWaitSeconds 0 `
            -RestoreInstallation { $script:installationRan = $true; 'verified' }
    } finally { $lock.Dispose() }
    $result.installationRan = ($cleanup.installation -ceq 'verified')
    $result.ummVerified = $cleanup.umm.verified; $result.ummError = $cleanup.umm.error
}
[IO.File]::WriteAllText($Output, ($result | ConvertTo-Json))
'@)

    # Path resolution from the live mod directory.
    Assert-True ((Get-KmgUmmParamsPath 'C:\Games\PK\Mods\KingmakerGunslinger') -ceq
        'C:\Games\PK\Kingmaker_Data\Managed\UnityModManager\Params.xml') 'path-resolves-from-live-mod-directory'

    # 1. Changed LastUpdateCheck timestamp: exact bytes restored by the owner.
    $c = New-Case 'timestamp'
    $r = Backup-Owned $c
    Assert-True ($r.existed -and $r.intakeSha256 -ceq $intakeSha -and $r.state -ceq 'pending' -and $r.schemaVersion -eq 2) 'backup-records-intake'
    Assert-True ($r.ownerProcess.processId -eq $PID -and $r.ownerLease.runId -match '^runtime-' -and (Test-LockFree $c)) 'backup-records-lease-owner-and-releases-lock'
    Assert-True ((Get-KmgUmmFileSha256 $r.backupPath) -ceq $intakeSha) 'backup-bytes-exact'
    Assert-True ((Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'receipt-durable-pending'
    Set-Text $c.Params '2026-11-07T18:00:00.0000000-05:00' '2026-11-08T00:14:01.6825169-05:00'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and $result.disposition -ceq 'restored') 'timestamp-change-restored'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'timestamp-change-hash-matches-intake'
    $done = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($done.state -ceq 'restored' -and $done.restoredBy -ceq 'owner') 'receipt-marked-restored-by-owner'
    Assert-True ((Restore-KmgUmmParams -ReceiptPath $r.receiptPath).disposition -ceq 'already-restored') 'restore-idempotent'

    # 2. Changed enabled-mod profile content: exact bytes restored.
    $c = New-Case 'profile'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'profile-change-restored'

    # 3. Unchanged file: verified without rewriting.
    $c = New-Case 'unchanged'
    $r = Backup-Owned $c
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and $result.disposition -ceq 'unchanged') 'unchanged-verified'

    # 4. Scenario failure: the finally-step still restores.
    $c = New-Case 'scenario-failure'
    $r = Backup-Owned $c
    $scenarioFailed = $false; $result = $null
    try { Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'; throw 'scenario failed' }
    catch { $scenarioFailed = $true }
    finally { $result = Complete-KmgUmmParamsProtection -Receipt $r }
    Assert-True ($scenarioFailed -and $result.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'scenario-failure-restored'

    # 5. Missing backup: refused, nothing written, receipt and evidence retained,
    #    and a new launch is refused while the receipt is pending.
    $c = New-Case 'missing-backup'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    Remove-Item -LiteralPath $r.backupPath -Force
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True (-not $result.verified -and $result.disposition -ceq 'FAILED' -and $result.error -match 'backup is missing') 'missing-backup-fails'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'missing-backup-writes-nothing'
    $kept = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($kept.state -ceq 'pending' -and $kept.attempts -eq 1 -and $kept.lastError) 'missing-backup-receipt-retained'
    Assert-Throws { Backup-Owned $c } 'pending-receipt-refuses-launch' 'still pending'

    # 6. Corrupt backup: refused, nothing written.
    $c = New-Case 'corrupt-backup'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    Set-Text $r.backupPath 'KingmakerGunslinger' 'KingmakerGunslingeR'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True (-not $result.verified -and $result.error -match 'does not match its recorded intake hash') 'corrupt-backup-fails'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'corrupt-backup-writes-nothing'

    # 7. Failed restore (destination locked), then a successful retry.
    $c = New-Case 'retry'
    $r = Backup-Owned $c
    Set-Text $c.Params '2026-11-07T18:00:00.0000000-05:00' '2026-11-08T00:14:01.6825169-05:00'
    $lock = [IO.File]::Open($c.Params, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
    try { $first = Complete-KmgUmmParamsProtection -Receipt $r } finally { $lock.Dispose() }
    Assert-True (-not $first.verified -and (Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'locked-restore-fails-pending'
    Assert-True ((Get-KmgUmmFileSha256 $r.backupPath) -ceq $intakeSha) 'locked-restore-backup-retained'
    $retry = Complete-KmgUmmParamsProtection -Receipt $r
    $after = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($retry.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha -and $after.state -ceq 'restored' -and
        $after.attempts -eq 2 -and $null -eq $after.lastError) 'retry-restores'

    # 8. A Params.xml failure never throws out of the owner's step.
    $c = New-Case 'independent'
    $r = Backup-Owned $c
    Remove-Item -LiteralPath $r.backupPath -Force
    $ummStep = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True (-not $ummStep.verified) 'owner-step-failure-returns'
    Assert-True ((Complete-KmgUmmParamsProtection -Receipt $null).disposition -ceq 'not-protected') 'missing-receipt-not-pass'

    # 9. A file absent at intake is removed again after the run.
    $c = New-Case 'absent'
    Remove-Item -LiteralPath $c.Params -Force
    $r = Backup-Owned $c
    [IO.File]::WriteAllBytes($c.Params, $intakeBytes)
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and -not $r.existed -and -not (Test-Path -LiteralPath $c.Params)) 'absent-intake-restored-absent'

    # 10. Backup protection that cannot be recorded refuses the launch, and a
    #     backup without a live owned lease is refused.
    $c = New-Case 'unwritable'
    New-Item -ItemType Directory -Path $c.Root -Force | Out-Null
    [IO.File]::WriteAllText($c.Backups, 'not a directory')
    Assert-Throws { Backup-Owned $c } 'unrecordable-backup-refuses-launch'
    $c = New-Case 'no-lease'
    $scope = Enter-KmgRuntimeLease -Purpose 'umm-params protection' -StateRoot $c.State
    Exit-KmgRuntimeLease $scope
    Assert-Throws { Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups -OwnerLease $scope.Lease } 'released-lease-refuses-backup'

    # ---- Recovery coordination (repair R5 review) ----

    # 11. No game process, but the original owner is still active: recovery
    #     refuses and leaves bytes and receipt unchanged (also via the script).
    $c = New-Case 'live-owner'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    Assert-Throws { Invoke-KmgUmmParamsRecovery -ReceiptPath $r.receiptPath -StateRoot $c.State } 'live-owner-recovery-refused' 'original owner'
    Assert-Throws { & (Join-Path $PSScriptRoot 'Restore-UmmParams.ps1') -BackupRoot $c.Backups -StateRoot $c.State -Confirm:$false } 'live-owner-script-refused' 'original owner'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes) 'live-owner-bytes-and-receipt-unchanged'
    Assert-True (Test-LockFree $c) 'live-owner-refusal-releases-recovery-lease'
    Assert-True ((Complete-KmgUmmParamsProtection -Receipt $r).verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'live-owner-completes-its-own-receipt'

    # 12. A foreign active lease owns the installation: refused, never stolen.
    $c = New-Case 'foreign-lease'
    $child = Invoke-ChildOwner $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $child.receiptPath
    $foreign = Enter-KmgRuntimeLease -Purpose 'foreign test transaction' -StateRoot $c.State
    try {
        Assert-Throws { Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State } 'foreign-lease-recovery-refused' 'another transaction'
        Assert-True ((Get-Content -LiteralPath $foreign.Lease.LockPath -Raw).Trim() -ceq $foreign.Lease.RunId) 'foreign-lease-not-stolen'
        Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $child.receiptPath) -ceq $receiptBytes) 'foreign-lease-bytes-and-receipt-unchanged'
    } finally { Exit-KmgRuntimeLease $foreign }

    # 13. Kingmaker already running at entry: refused before any lease, by
    #     function and by script. No new lock, run directory or recovery debt;
    #     case 15 is the later permitted recovery on this same receipt.
    $runsBefore = Get-RunDirectoryCount $c
    $global:kmgUmmTest.running = $true
    try {
        Assert-Throws { Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State } 'running-game-recovery-refused' 'recovery refused: Kingmaker is running'
        Assert-Throws { & (Join-Path $PSScriptRoot 'Restore-UmmParams.ps1') -BackupRoot $c.Backups -StateRoot $c.State -Confirm:$false } 'running-game-script-refused' 'recovery refused: Kingmaker is running'
    } finally { Reset-Game }
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $child.receiptPath) -ceq $receiptBytes) 'running-game-refusal-writes-nothing'
    Assert-True ((Test-LockFree $c) -and (Get-RunDirectoryCount $c) -eq $runsBefore -and (Get-LeaseStatusCount $c 'RecoveryRequired') -eq 0) 'running-game-refusal-creates-no-lock-or-debt'

    # 14. A non-owner cannot complete the receipt in place.
    $inPlace = Complete-KmgUmmParamsProtection -Receipt (Read-KmgUmmReceipt $child.receiptPath)
    Assert-True (-not $inPlace.verified -and $inPlace.error -match 'Only the live owner' -and (Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'non-owner-in-place-completion-refused'
    Assert-True ((Read-KmgUmmReceipt $child.receiptPath).state -ceq 'pending') 'premature-restored-marking-prevented'

    # 15. The legitimately interrupted owner, once coordination permits:
    #     original bytes restored, recovery recorded, lock released, and a
    #     repeated recovery is idempotent.
    $recovered = Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State
    $done = Read-KmgUmmReceipt $child.receiptPath
    Assert-True ($recovered.disposition -ceq 'restored' -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'interrupted-owner-recovery-restores'
    Assert-True ($done.state -ceq 'restored' -and $done.restoredBy -ceq 'recovery' -and $done.recoveryLease -match '^runtime-' -and (Test-LockFree $c)) 'recovery-recorded-and-lock-released'
    Assert-True ((Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State).disposition -ceq 'already-restored') 'completed-recovery-idempotent'

    # 16. Interruption recovery through the shipped recovery script.
    $c = New-Case 'recovery-script'
    $child = Invoke-ChildOwner $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    & (Join-Path $PSScriptRoot 'Restore-UmmParams.ps1') -BackupRoot $c.Backups -StateRoot $c.State -Confirm:$false | Out-Null
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha -and @(Get-KmgPendingUmmParamsReceipts $c.Backups).Count -eq 0) 'recovery-script-restores-pending'

    # ---- The wrapper's cleanup path (repair R5 review) ----

    # 17. Installation inspection/hash failure while Params.xml differs from
    #     intake: the UMM step still restores, and the installation failure
    #     stays reported.
    $c = New-Case 'cleanup-installation-failure'
    $r = Backup-Owned $c
    Set-Text $c.Params '2026-11-07T18:00:00.0000000-05:00' '2026-11-08T00:14:01.6825169-05:00'
    $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $r -ExitWaitSeconds 0 `
        -RestoreInstallation { throw 'Get-KmgTreeFingerprint failed: access denied' }
    Assert-True ($cleanup.installationError -match 'Get-KmgTreeFingerprint failed' -and $null -eq $cleanup.installation) 'installation-failure-retained'
    Assert-True ($cleanup.umm.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'installation-failure-umm-still-restores'

    # 18. A UMM failure still lets the installation step run and keeps its
    #     result; the retained receipt and backup then recover the bytes once
    #     the owner has exited.
    $c = New-Case 'cleanup-umm-failure'
    $child = Invoke-ChildOwner $c -CleanupWithLockedParams
    $kept = Read-KmgUmmReceipt $child.receiptPath
    Assert-True ($child.installationRan -and -not $child.ummVerified -and $child.ummError) 'umm-failure-installation-still-runs'
    Assert-True ($kept.state -ceq 'pending' -and $kept.attempts -eq 1 -and $kept.lastError -and
        (Get-KmgUmmFileSha256 $kept.backupPath) -ceq $intakeSha) 'umm-failure-evidence-retained'
    $recovered = Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State
    Assert-True ($recovered.disposition -ceq 'restored' -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'umm-failure-recovered-later'

    # 19. Both steps fail: both failures retained, neither masks the other;
    #     a running game defers the UMM step without blocking installation.
    $c = New-Case 'cleanup-both-failures'
    $r = Backup-Owned $c
    Remove-Item -LiteralPath $r.backupPath -Force
    $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $r -ExitWaitSeconds 0 -RestoreInstallation { throw 'restore failed' }
    Assert-True ($cleanup.installationError -match 'restore failed' -and -not $cleanup.umm.verified -and $cleanup.umm.error -match 'backup is missing') 'both-failures-retained'
    $c = New-Case 'cleanup-game-running'
    $r = Backup-Owned $c
    $runsBefore = Get-RunDirectoryCount $c
    $global:kmgUmmTest.running = $true
    try { $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $r -ExitWaitSeconds 0 -RestoreInstallation { 'verified' } }
    finally { Reset-Game }
    Assert-True ($cleanup.installation -ceq 'verified' -and -not $cleanup.umm.verified -and $cleanup.umm.error -match 'still running' -and
        (Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'running-game-defers-umm-only'
    Assert-True ((Test-LockFree $c) -and (Get-RunDirectoryCount $c) -eq $runsBefore) 'running-game-deferral-creates-no-lock'

    # 20. The wrapper actually uses this path: backup under its lease; the
    #     finally reaches the independent step before any fingerprint; an
    #     unresolved installation or UMM restoration refuses PASS.
    $wrapper = (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Invoke-ExpandedSummoningRuntimeScenario.ps1') -Raw).Replace("`r`n", "`n")
    $finally = $wrapper.IndexOf("finally {`n    # Runs on success, on failure, and on interruption.", [StringComparison]::Ordinal)
    $call = $wrapper.IndexOf('Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $ummReceipt -ParentLease $ummParentLease -RestoreInstallation {', [StringComparison]::Ordinal)
    $fingerprint = $wrapper.IndexOf('Get-KmgTreeFingerprint -Directory $LiveModDirectory', $finally, [StringComparison]::Ordinal)
    Assert-True ($finally -gt 0 -and $call -gt $finally -and $fingerprint -gt $call) 'wrapper-finally-guards-the-fingerprint'
    Assert-True ($wrapper.Contains("`$ummScope = Enter-KmgRuntimeLease -Purpose 'umm-params protection'") -and
        $wrapper.Contains('-OwnerLease $ummScope.Lease') -and $wrapper.Contains('finally { Exit-KmgRuntimeLease $ummScope }')) 'wrapper-backs-up-under-its-lease'
    Assert-True ($wrapper.Contains('if($installationRestorationFailed){throw') -and $wrapper.Contains('if($ummRestorationFailed){throw') -and
        $wrapper.Contains("`$record.restoration -notin @('verified', 'not-needed')")) 'wrapper-refuses-pass-on-unresolved-restoration'
    $inherit = $wrapper.IndexOf('$ummParentLease = if ($startupPeerScope) { $startupPeerScope.Lease } else { $null }', [StringComparison]::Ordinal)
    $release = $wrapper.IndexOf('Exit-KmgRuntimeLease $startupPeerScope', [StringComparison]::Ordinal)
    Assert-True ($inherit -gt $finally -and $inherit -lt $call -and $release -gt $call) 'wrapper-inherits-its-held-startup-lease-through-the-umm-step'

    # ---- Automatic owner completion holds coordination ownership ----

    # 21. The receipt's legitimate original process, while a foreign lease owns
    #     the coordination root: refused, nothing touched, then a legitimate
    #     retry once ownership is available.
    $c = New-Case 'owner-foreign-lease'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    Assert-True (Test-KmgUmmReceiptOwnedByCurrentProcess (Read-KmgUmmReceipt $r.receiptPath)) 'owner-identity-alone-matches'
    $foreign = Enter-KmgRuntimeLease -Purpose 'foreign test transaction' -StateRoot $c.State
    try {
        $first = Complete-KmgUmmParamsProtection -Receipt $r
        Assert-True (-not $first.verified -and $first.disposition -ceq 'FAILED' -and $first.error -match 'another transaction') 'owner-under-foreign-lease-reports-failure'
        Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes) 'owner-under-foreign-lease-bytes-and-receipt-untouched'
        Assert-True ((Test-LeaseIntact $foreign) -and (Get-Content -LiteralPath $foreign.Lease.LockPath -Raw).Trim() -ceq $foreign.Lease.RunId) 'owner-under-foreign-lease-leaves-it-intact'
    } finally { Exit-KmgRuntimeLease $foreign }
    $retry = Complete-KmgUmmParamsProtection -Receipt $r
    $done = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($retry.verified -and $retry.disposition -ceq 'restored' -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'owner-retry-restores-exact-intake'
    Assert-True ($done.state -ceq 'restored' -and $done.restoredBy -ceq 'owner' -and $done.restorationLease -match '^runtime-' -and $done.attempts -eq 1 -and (Test-LockFree $c)) 'owner-retry-completes-under-its-own-released-lease'

    # 22. Restoration refuses without a held lease; only verifying a completed
    #     receipt needs none.
    $c = New-Case 'restore-needs-lease'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    Assert-Throws { Restore-KmgUmmParams -ReceiptPath $r.receiptPath } 'restore-without-lease-refused' 'lease is missing'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'restore-without-lease-writes-nothing'

    # 23. Combined cleanup under a foreign lease: the installation step's own
    #     real ownership failure is kept, and the UMM step does not bypass it.
    $c = New-Case 'combined-foreign-lease'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    $foreign = Enter-KmgRuntimeLease -Purpose 'foreign test transaction' -StateRoot $c.State
    try {
        $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $r -ExitWaitSeconds 0 -RestoreInstallation {
            $scope = Enter-KmgRuntimeLease -Purpose 'restore explicit live-mod backup' -StateRoot $c.State
            Exit-KmgRuntimeLease $scope
            'verified'
        }
        Assert-True ($null -eq $cleanup.installation -and $cleanup.installationError -match 'lock already exists') 'combined-installation-ownership-failure-kept'
        Assert-True (-not $cleanup.umm.verified -and $cleanup.umm.error -match 'another transaction') 'combined-umm-refused-not-bypassing'
        Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes -and (Test-LeaseIntact $foreign)) 'combined-foreign-transaction-untouched'
    } finally { Exit-KmgRuntimeLease $foreign }

    # 24. Automatic completion with Kingmaker already running at entry: no new
    #     lock or recovery debt, and the owner's retry later succeeds.
    $c = New-Case 'owner-game-running'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    $runsBefore = Get-RunDirectoryCount $c
    $global:kmgUmmTest.running = $true
    try { $first = Complete-KmgUmmParamsProtection -Receipt $r } finally { Reset-Game }
    Assert-True (-not $first.verified -and $first.error -match 'completion refused: Kingmaker is running') 'owner-game-running-reported'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes) 'owner-game-running-writes-nothing'
    Assert-True ((Test-LockFree $c) -and (Get-RunDirectoryCount $c) -eq $runsBefore -and (Get-LeaseStatusCount $c 'RecoveryRequired') -eq 0) 'owner-game-running-creates-no-lock-or-debt'
    Assert-True ((Complete-KmgUmmParamsProtection -Receipt $r).verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'owner-game-running-retry-restores'

    # 25. Kingmaker appears after admission, once the lease is held: the recheck
    #     stops the write, and the shared teardown keeps its protection (the
    #     lease is retained for recovery rather than released).
    $c = New-Case 'game-appears-recovery'
    $child = Invoke-ChildOwner $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $child.receiptPath
    $global:kmgUmmTest.appearOnceLockExists = Join-Path $c.State 'compatibility.lock'
    try { Assert-Throws { Invoke-KmgUmmParamsRecovery -ReceiptPath $child.receiptPath -StateRoot $c.State } 'game-appears-recovery-refused' 'retained for recovery' }
    finally { Reset-Game }
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $child.receiptPath) -ceq $receiptBytes) 'game-appears-recovery-writes-nothing'
    Assert-True (-not (Test-LockFree $c) -and (Get-LeaseStatusCount $c 'RecoveryRequired') -eq 1) 'game-appears-recovery-lease-retained'
    $c = New-Case 'game-appears-owner'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    $global:kmgUmmTest.appearOnceLockExists = Join-Path $c.State 'compatibility.lock'
    try { $first = Complete-KmgUmmParamsProtection -Receipt $r } finally { Reset-Game }
    Assert-True (-not $first.verified -and $first.error -match 'retained for recovery') 'game-appears-owner-reported'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes) 'game-appears-owner-writes-nothing'

    # 26. A valid inherited lease: no deadlock against its own lock, restoration
    #     recorded under that lease, and the parent is not released.
    $c = New-Case 'inherited-lease'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $parent = Enter-KmgRuntimeLease -Purpose 'startup protection test' -StateRoot $c.State
    try {
        $notInherited = Complete-KmgUmmParamsProtection -Receipt $r
        Assert-True (-not $notInherited.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'held-lease-is-not-assumed-without-inheritance'
        $cleanup = Invoke-KmgIndependentInstallationAndUmmRestoration -UmmReceipt $r -ParentLease $parent.Lease -ExitWaitSeconds 0 -RestoreInstallation { 'verified' }
        $done = Read-KmgUmmReceipt $r.receiptPath
        Assert-True ($cleanup.umm.verified -and $cleanup.installation -ceq 'verified' -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'inherited-lease-restores'
        Assert-True ($done.state -ceq 'restored' -and $done.restoredBy -ceq 'owner' -and $done.restorationLease -ceq $parent.Lease.RunId) 'inherited-lease-recorded'
        Assert-True ((Test-LeaseIntact $parent) -and $parent.Lease.Stream.CanRead) 'inherited-lease-not-released'
    } finally { Exit-KmgRuntimeLease $parent }
    Assert-True (Test-LockFree $c) 'inherited-lease-released-by-its-owner'

    # 27. An inherited lease from another root, or one already released, is
    #     refused without any write; the owner can still retry.
    $c = New-Case 'inherited-invalid'
    $r = Backup-Owned $c
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    $receiptBytes = Get-KmgUmmFileSha256 $r.receiptPath
    $other = Enter-KmgRuntimeLease -Purpose 'other root' -StateRoot (Join-Path $c.Root 'other-state')
    try {
        $wrongRoot = Complete-KmgUmmParamsProtection -Receipt $r -ParentLease $other.Lease
        Assert-True (-not $wrongRoot.verified -and $wrongRoot.error -match 'different coordination root' -and (Test-LeaseIntact $other)) 'inherited-wrong-root-refused'
    } finally { Exit-KmgRuntimeLease $other }
    $released = Enter-KmgRuntimeLease -Purpose 'already released' -StateRoot $c.State
    Exit-KmgRuntimeLease $released
    $stale = Complete-KmgUmmParamsProtection -Receipt $r -ParentLease $released.Lease
    Assert-True (-not $stale.verified -and $stale.error -match 'no longer owned') 'inherited-released-lease-refused'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha -and (Get-KmgUmmFileSha256 $r.receiptPath) -ceq $receiptBytes -and (Test-LockFree $c)) 'inherited-invalid-writes-nothing'
    Assert-True ((Complete-KmgUmmParamsProtection -Receipt $r).verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'inherited-invalid-owner-retry-restores'
}
finally {
    # A function: drive path with a global: qualifier removes nothing, and the
    # stand-in would break every later check in a shared validation session.
    Remove-Item -Path 'Function:\Get-Process' -Force
    if ((Get-Command Get-Process).CommandType -ne 'Cmdlet') { $failures.Add('process-seam-removed') }
    Remove-Variable -Name kmgUmmTest -Scope Global -Force
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}

if ($failures.Count -gt 0) {
    Write-Host ("UMM Params.xml protection FAIL ({0}): {1}" -f $failures.Count, ($failures -join ', '))
    exit 1
}
Write-Host "UMM Params.xml protection tests PASS=$passes"
