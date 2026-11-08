Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')

# Unity Mod Manager rewrites its installed Params.xml (for example its
# LastUpdateCheck timestamp) when the game starts. A guarded launch preserves
# the exact intake bytes before staging, records the backup durably for
# interruption recovery, and restores and verifies them after Kingmaker exits.
# Bytes are copied, never parsed and reserialized.
#
# Ownership (repair R5 review): a receipt is created while its owner holds a
# runtime lease from the shared coordination lock, and records that lease and
# the owner's exact process identity (PID and start time, as runtime leases
# do). The receipt identifies who may complete it; it does not grant current
# ownership of the installation. Every restoration therefore also holds a live
# lease in the receipt's coordination root: the owner's own new lease or an
# explicitly inherited one (Complete-KmgUmmParamsProtection), or recovery's
# (Invoke-KmgUmmParamsRecovery, for an interrupted owner). Kingmaker is checked
# before a new lease is taken, as other entry points do, because
# Exit-KmgRuntimeLease retains any lease released while the game runs; it is
# checked again while the lease is held, before anything is written.

$script:KmgUmmReceiptName = 'umm-params-receipt.json'

function Get-KmgUmmParamsPath([string]$LiveModDirectory) {
    if ([string]::IsNullOrWhiteSpace($LiveModDirectory)) { throw 'A live mod directory is required to resolve UMM Params.xml.' }
    $mods = Split-Path -Parent ([IO.Path]::GetFullPath($LiveModDirectory))
    $game = Split-Path -Parent $mods
    return [IO.Path]::GetFullPath((Join-Path $game 'Kingmaker_Data\Managed\UnityModManager\Params.xml'))
}

function Get-KmgUmmBytesSha256([byte[]]$Bytes) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash($Bytes)) -replace '-', '') }
    finally { $sha.Dispose() }
}

function Get-KmgUmmFileSha256([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    return Get-KmgUmmBytesSha256 ([IO.File]::ReadAllBytes($Path))
}

function Write-KmgUmmReceipt($Receipt) {
    $json = ([pscustomobject]$Receipt | ConvertTo-Json -Depth 6)
    Write-KmgRuntimeAtomicBytes -Path $Receipt.receiptPath -Bytes ([Text.UTF8Encoding]::new($false).GetBytes($json))
}

function Read-KmgUmmReceipt([string]$ReceiptPath) {
    if (-not (Test-Path -LiteralPath $ReceiptPath -PathType Leaf)) { throw "UMM Params.xml receipt is missing: $ReceiptPath" }
    try { $receipt = Get-Content -LiteralPath $ReceiptPath -Raw | ConvertFrom-Json -ErrorAction Stop }
    catch { throw "UMM Params.xml receipt is not valid JSON: $ReceiptPath" }
    if ($receipt.kind -cne 'KmgUmmParamsReceipt' -or $receipt.schemaVersion -notin @(1, 2)) { throw "Not a UMM Params.xml receipt: $ReceiptPath" }
    return $receipt
}

function Get-KmgPendingUmmParamsReceipts([string]$BackupRoot) {
    if (-not (Test-Path -LiteralPath $BackupRoot -PathType Container)) { return @() }
    return @(Get-ChildItem -LiteralPath $BackupRoot -Recurse -File -Filter $script:KmgUmmReceiptName |
        ForEach-Object { Read-KmgUmmReceipt $_.FullName } | Where-Object { $_.state -ceq 'pending' })
}

function Test-KmgUmmReceiptOwnedByCurrentProcess($Receipt) {
    if ($Receipt.schemaVersion -lt 2 -or $null -eq $Receipt.PSObject.Properties['ownerProcess'] -or $null -eq $Receipt.ownerProcess) { return $false }
    $self = Get-Process -Id $PID
    return Test-KmgRuntimeProcessIdentity $self ([int]$Receipt.ownerProcess.processId) ([string]$Receipt.ownerProcess.startedUtc)
}

function Assert-KmgUmmGameNotRunning([string]$Operation) {
    if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0) {
        throw "UMM Params.xml $Operation refused: Kingmaker is running."
    }
}

# Before staging or launch, while the caller holds its runtime lease. Throws
# (refusing the launch) when an earlier receipt is still pending, the lease is
# not live and owned, or the byte copy cannot be verified.
function Backup-KmgUmmParams {
    param([Parameter(Mandatory = $true)][string]$ParamsPath,
        [Parameter(Mandatory = $true)][string]$BackupRoot,
        [Parameter(Mandatory = $true)]$OwnerLease,
        [string]$Owner)
    Assert-KmgRuntimeLease $OwnerLease
    if ($OwnerLease.Compatibility -or $null -eq $OwnerLease.State -or $OwnerLease.State.processId -ne $PID) {
        throw 'UMM Params.xml protection requires a live runtime lease owned by this process.'
    }
    $pending = @(Get-KmgPendingUmmParamsReceipts $BackupRoot)
    if ($pending.Count -gt 0) {
        throw ('A previous UMM Params.xml backup is still pending restoration: ' + $pending[0].receiptPath +
            '. Recover it with scripts\Restore-UmmParams.ps1 before launching.')
    }
    $directory = Join-Path $BackupRoot ([DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $existed = Test-Path -LiteralPath $ParamsPath -PathType Leaf
    $receipt = [ordered]@{
        kind = 'KmgUmmParamsReceipt'; schemaVersion = 2; owner = $Owner
        ownerLease = [ordered]@{ runId = $OwnerLease.RunId; statePath = [IO.Path]::GetFullPath($OwnerLease.StatePath)
            stateRoot = [IO.Path]::GetFullPath((Split-Path -Parent $OwnerLease.LockPath)) }
        ownerProcess = [ordered]@{ processId = [int]$OwnerLease.State.processId; startedUtc = [string]$OwnerLease.State.processStartedUtc }
        paramsPath = [IO.Path]::GetFullPath($ParamsPath); existed = $existed
        intakeSha256 = $null; intakeLength = $null; backupPath = $null; backupSha256 = $null
        createdUtc = [DateTime]::UtcNow.ToString('o'); state = 'pending'
        receiptPath = (Join-Path $directory $script:KmgUmmReceiptName)
        restoredUtc = $null; restoredSha256 = $null; rewritten = $null; restoredBy = $null; restorationLease = $null; recoveryLease = $null
        attempts = 0; lastError = $null
    }
    if ($existed) {
        $bytes = [IO.File]::ReadAllBytes($ParamsPath)
        $sha = Get-KmgUmmBytesSha256 $bytes
        $backup = Join-Path $directory 'Params.xml'
        Write-KmgRuntimeAtomicBytes -Path $backup -Bytes $bytes
        if ((Get-KmgUmmFileSha256 $backup) -cne $sha -or (Get-KmgUmmFileSha256 $ParamsPath) -cne $sha) {
            throw 'UMM Params.xml backup could not be verified byte-exactly; launch refused.'
        }
        $receipt.intakeSha256 = $sha; $receipt.intakeLength = $bytes.Length
        $receipt.backupPath = $backup; $receipt.backupSha256 = $sha
    }
    Write-KmgUmmReceipt $receipt
    $check = Read-KmgUmmReceipt $receipt.receiptPath
    if ($check.state -cne 'pending' -or $check.intakeSha256 -cne $receipt.intakeSha256 -or $check.existed -ne $existed -or
        $check.ownerLease.runId -cne $OwnerLease.RunId -or $check.ownerProcess.processId -ne $PID) {
        throw 'UMM Params.xml receipt could not be recorded durably; launch refused.'
    }
    return [pscustomobject]$receipt
}

# The restoration itself, called with the lease its caller holds after that
# caller's ownership checks. Verifying an already-restored receipt writes
# nothing and needs no lease; anything else refuses without a live one. Throws
# on any failure; the receipt stays pending with its error and the backup is
# retained for a retry.
function Restore-KmgUmmParams {
    param([Parameter(Mandatory = $true)][string]$ReceiptPath, $Lease,
        [ValidateSet('owner', 'recovery')][string]$RestoredBy = 'owner')
    $receipt = Read-KmgUmmReceipt $ReceiptPath
    if ($receipt.state -ceq 'restored') {
        $current = Get-KmgUmmFileSha256 $receipt.paramsPath
        if ($current -cne $receipt.intakeSha256) { throw 'UMM Params.xml changed after its recorded restoration.' }
        return [pscustomobject]@{ disposition = 'already-restored'; receiptPath = $ReceiptPath; intakeSha256 = $receipt.intakeSha256; restoredSha256 = $current }
    }
    Assert-KmgRuntimeLease $Lease
    $state = [ordered]@{}
    foreach ($property in $receipt.PSObject.Properties) { $state[$property.Name] = $property.Value }
    $state.attempts = [int]$state.attempts + 1
    try {
        $rewritten = $false
        if ($receipt.existed) {
            if ([string]::IsNullOrEmpty($receipt.backupPath) -or -not (Test-Path -LiteralPath $receipt.backupPath -PathType Leaf)) {
                throw 'UMM Params.xml backup is missing; nothing was written.'
            }
            $bytes = [IO.File]::ReadAllBytes($receipt.backupPath)
            $backupSha = Get-KmgUmmBytesSha256 $bytes
            if ($backupSha -cne $receipt.backupSha256 -or $backupSha -cne $receipt.intakeSha256) {
                throw 'UMM Params.xml backup does not match its recorded intake hash; nothing was written.'
            }
            if ((Get-KmgUmmFileSha256 $receipt.paramsPath) -cne $receipt.intakeSha256) {
                Write-KmgRuntimeAtomicBytes -Path $receipt.paramsPath -Bytes $bytes
                $rewritten = $true
            }
            $after = Get-KmgUmmFileSha256 $receipt.paramsPath
            if ($after -cne $receipt.intakeSha256) { throw 'Restored UMM Params.xml does not match its intake hash.' }
        } else {
            if (Test-Path -LiteralPath $receipt.paramsPath) { Remove-Item -LiteralPath $receipt.paramsPath -Force; $rewritten = $true }
            if (Test-Path -LiteralPath $receipt.paramsPath) { throw 'UMM Params.xml created during the run could not be removed.' }
            $after = $null
        }
        $state.state = 'restored'; $state.restoredUtc = [DateTime]::UtcNow.ToString('o')
        $state.restoredSha256 = $after; $state.rewritten = $rewritten; $state.lastError = $null
        $state.restoredBy = $RestoredBy
        $state.restorationLease = $Lease.RunId
        $state.recoveryLease = $(if ($RestoredBy -ceq 'recovery') { $Lease.RunId } else { $null })
        Write-KmgUmmReceipt $state
        return [pscustomobject]@{ disposition = $(if ($rewritten) { 'restored' } else { 'unchanged' })
            receiptPath = $ReceiptPath; intakeSha256 = $receipt.intakeSha256; restoredSha256 = $after; attempts = $state.attempts
            restoredBy = $state.restoredBy }
    } catch {
        $message = $_.Exception.Message
        try { $state.lastError = $message; Write-KmgUmmReceipt $state } catch { }
        throw "UMM Params.xml restoration failed (backup and receipt retained at $ReceiptPath): $message"
    }
}

# The owner's completion. Requires the receipt's own process identity and valid
# current coordination ownership, held through the game recheck, restoration,
# hash verification and receipt completion. A held parent lease is validated
# and inherited, never released here. A refusal leaves Params.xml and the
# pending receipt untouched and never disturbs another transaction's lock.
function Invoke-KmgUmmOwnerCompletion([string]$ReceiptPath, $ParentLease) {
    $receipt = Read-KmgUmmReceipt $ReceiptPath
    if ($receipt.state -ceq 'restored') { return Restore-KmgUmmParams -ReceiptPath $ReceiptPath }
    if (-not (Test-KmgUmmReceiptOwnedByCurrentProcess $receipt)) {
        throw 'Only the live owner of this UMM Params.xml receipt completes it in place; use scripts\Restore-UmmParams.ps1 to recover an interrupted owner.'
    }
    Assert-KmgUmmGameNotRunning 'completion'
    try { $scope = Enter-KmgRuntimeLease -ParentLease $ParentLease -Purpose 'umm-params completion' -StateRoot $receipt.ownerLease.stateRoot }
    catch { throw "UMM Params.xml completion refused: another transaction owns the installation, or the inherited lease is not valid here. $($_.Exception.Message)" }
    try {
        Assert-KmgUmmGameNotRunning 'completion'
        $receipt = Read-KmgUmmReceipt $ReceiptPath
        if ($receipt.state -cne 'restored' -and -not (Test-KmgUmmReceiptOwnedByCurrentProcess $receipt)) {
            throw 'UMM Params.xml receipt ownership changed before completion.'
        }
        return Restore-KmgUmmParams -ReceiptPath $ReceiptPath -Lease $scope.Lease -RestoredBy owner
    } finally { Exit-KmgRuntimeLease $scope }
}

# Owner's independent cleanup step: never throws, so a Params.xml failure
# cannot prevent any other restoration step. A failure stays in the returned
# outcome and the owner may retry once ownership is available.
function Complete-KmgUmmParamsProtection {
    param([AllowNull()]$Receipt, $ParentLease)
    if ($null -eq $Receipt) { return [ordered]@{ verified = $false; disposition = 'not-protected'; error = 'No UMM Params.xml receipt was recorded.' } }
    try {
        $result = Invoke-KmgUmmOwnerCompletion -ReceiptPath $Receipt.receiptPath -ParentLease $ParentLease
        return [ordered]@{ verified = $true; disposition = $result.disposition; receiptPath = $result.receiptPath
            intakeSha256 = $result.intakeSha256; restoredSha256 = $result.restoredSha256 }
    } catch {
        return [ordered]@{ verified = $false; disposition = 'FAILED'; error = $_.Exception.Message
            receiptPath = $Receipt.receiptPath; backupPath = $Receipt.backupPath; intakeSha256 = $Receipt.intakeSha256 }
    }
}

# Recovery of a receipt whose owner was interrupted. Refuses a game already
# running before creating any lease, so that refusal leaves no new lock or
# recovery obligation. Serialized through the shared coordination lock:
# entering a lease fails while any transaction (the original owner's or a
# foreign one) holds it, and no lock is stolen or cleared. While holding it,
# refuses if the game runs or the receipt's exact owner process is still
# alive. Throws on refusal or failure.
function Invoke-KmgUmmParamsRecovery {
    param([Parameter(Mandatory = $true)][string]$ReceiptPath,
        [string]$StateRoot = 'C:/Dev/KingmakerGunslingerLab/compatibility-state')
    $receipt = Read-KmgUmmReceipt $ReceiptPath
    if ($receipt.schemaVersion -ge 2 -and
        -not ([IO.Path]::GetFullPath($receipt.ownerLease.stateRoot)).Equals([IO.Path]::GetFullPath($StateRoot), [StringComparison]::OrdinalIgnoreCase)) {
        throw 'UMM Params.xml receipt belongs to a different coordination root; recovery refused.'
    }
    Assert-KmgUmmGameNotRunning 'recovery'
    try { $scope = Enter-KmgRuntimeLease -Purpose 'umm-params recovery' -StateRoot $StateRoot }
    catch { throw "UMM Params.xml recovery refused: another transaction owns the installation. $($_.Exception.Message)" }
    try {
        Assert-KmgUmmGameNotRunning 'recovery'
        $receipt = Read-KmgUmmReceipt $ReceiptPath
        if ($receipt.state -cne 'restored') {
            if ($receipt.schemaVersion -lt 2 -or $null -eq $receipt.ownerProcess) {
                throw 'UMM Params.xml recovery refused: the receipt records no owner identity, so an interrupted owner cannot be distinguished from a live one.'
            }
            $owner = Get-Process -Id ([int]$receipt.ownerProcess.processId) -ErrorAction SilentlyContinue
            if (Test-KmgRuntimeProcessIdentity $owner ([int]$receipt.ownerProcess.processId) ([string]$receipt.ownerProcess.startedUtc)) {
                throw "UMM Params.xml recovery refused: the original owner (PID $($receipt.ownerProcess.processId)) is still active."
            }
        }
        return Restore-KmgUmmParams -ReceiptPath $ReceiptPath -Lease $scope.Lease -RestoredBy recovery
    } finally { Exit-KmgRuntimeLease $scope }
}

# The guarded wrapper's restoration: the complete installation step and the
# UMM step run independently. Each failure is retained; neither prevents the
# other; the caller refuses an overall PASS unless both are resolved.
function Invoke-KmgIndependentInstallationAndUmmRestoration {
    param([Parameter(Mandatory = $true)][scriptblock]$RestoreInstallation,
        [AllowNull()]$UmmReceipt,
        $ParentLease,
        [int]$ExitWaitSeconds = 120)
    $installation = $null; $installationError = $null
    try { $installation = & $RestoreInstallation } catch { $installationError = $_.Exception.Message }
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds($ExitWaitSeconds)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $deadline) {
            Start-Sleep -Milliseconds 500
        }
        if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0) {
            $umm = [ordered]@{ verified = $false; disposition = 'FAILED'
                error = 'Kingmaker is still running; Params.xml restoration deferred to scripts\Restore-UmmParams.ps1.'
                receiptPath = $(if ($UmmReceipt) { $UmmReceipt.receiptPath } else { $null }) }
        } else { $umm = Complete-KmgUmmParamsProtection -Receipt $UmmReceipt -ParentLease $ParentLease }
    } catch {
        $umm = [ordered]@{ verified = $false; disposition = 'FAILED'; error = $_.Exception.Message
            receiptPath = $(if ($UmmReceipt) { $UmmReceipt.receiptPath } else { $null }) }
    }
    return [pscustomobject]@{ installation = $installation; installationError = $installationError; umm = $umm }
}
