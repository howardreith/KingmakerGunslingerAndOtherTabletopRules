Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')

# Unity Mod Manager rewrites its installed Params.xml (for example its
# LastUpdateCheck timestamp) when the game starts. A guarded launch preserves
# the exact intake bytes before staging, records the backup durably for
# interruption recovery, and restores and verifies them after Kingmaker exits.
# Bytes are copied, never parsed and reserialized.

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
    $json = ([pscustomobject]$Receipt | ConvertTo-Json -Depth 4)
    Write-KmgRuntimeAtomicBytes -Path $Receipt.receiptPath -Bytes ([Text.UTF8Encoding]::new($false).GetBytes($json))
}

function Read-KmgUmmReceipt([string]$ReceiptPath) {
    if (-not (Test-Path -LiteralPath $ReceiptPath -PathType Leaf)) { throw "UMM Params.xml receipt is missing: $ReceiptPath" }
    try { $receipt = Get-Content -LiteralPath $ReceiptPath -Raw | ConvertFrom-Json -ErrorAction Stop }
    catch { throw "UMM Params.xml receipt is not valid JSON: $ReceiptPath" }
    if ($receipt.kind -cne 'KmgUmmParamsReceipt' -or $receipt.schemaVersion -ne 1) { throw "Not a UMM Params.xml receipt: $ReceiptPath" }
    return $receipt
}

function Get-KmgPendingUmmParamsReceipts([string]$BackupRoot) {
    if (-not (Test-Path -LiteralPath $BackupRoot -PathType Container)) { return @() }
    return @(Get-ChildItem -LiteralPath $BackupRoot -Recurse -File -Filter $script:KmgUmmReceiptName |
        ForEach-Object { Read-KmgUmmReceipt $_.FullName } | Where-Object { $_.state -ceq 'pending' })
}

# Before staging or launch. Throws (refusing the launch) when an earlier
# receipt is still pending or the byte copy cannot be verified.
function Backup-KmgUmmParams {
    param([Parameter(Mandatory = $true)][string]$ParamsPath,
        [Parameter(Mandatory = $true)][string]$BackupRoot,
        [string]$Owner)
    $pending = @(Get-KmgPendingUmmParamsReceipts $BackupRoot)
    if ($pending.Count -gt 0) {
        throw ('A previous UMM Params.xml backup is still pending restoration: ' + $pending[0].receiptPath +
            '. Recover it with scripts\Restore-UmmParams.ps1 before launching.')
    }
    $directory = Join-Path $BackupRoot ([DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $existed = Test-Path -LiteralPath $ParamsPath -PathType Leaf
    $receipt = [ordered]@{
        kind = 'KmgUmmParamsReceipt'; schemaVersion = 1; owner = $Owner
        paramsPath = [IO.Path]::GetFullPath($ParamsPath); existed = $existed
        intakeSha256 = $null; intakeLength = $null; backupPath = $null; backupSha256 = $null
        createdUtc = [DateTime]::UtcNow.ToString('o'); state = 'pending'
        receiptPath = (Join-Path $directory $script:KmgUmmReceiptName)
        restoredUtc = $null; restoredSha256 = $null; rewritten = $null; attempts = 0; lastError = $null
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
    if ($check.state -cne 'pending' -or $check.intakeSha256 -cne $receipt.intakeSha256 -or $check.existed -ne $existed) {
        throw 'UMM Params.xml receipt could not be recorded durably; launch refused.'
    }
    return [pscustomobject]$receipt
}

# After Kingmaker exits (success, scenario failure or recovery). Throws on any
# failure; the receipt stays pending and the backup is retained for a retry.
function Restore-KmgUmmParams {
    param([Parameter(Mandatory = $true)][string]$ReceiptPath)
    $receipt = Read-KmgUmmReceipt $ReceiptPath
    if ($receipt.state -ceq 'restored') {
        $current = Get-KmgUmmFileSha256 $receipt.paramsPath
        if ($current -cne $receipt.intakeSha256) { throw 'UMM Params.xml changed after its recorded restoration.' }
        return [pscustomobject]@{ disposition = 'already-restored'; receiptPath = $ReceiptPath; intakeSha256 = $receipt.intakeSha256; restoredSha256 = $current }
    }
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
        Write-KmgUmmReceipt $state
        return [pscustomobject]@{ disposition = $(if ($rewritten) { 'restored' } else { 'unchanged' })
            receiptPath = $ReceiptPath; intakeSha256 = $receipt.intakeSha256; restoredSha256 = $after; attempts = $state.attempts }
    } catch {
        $message = $_.Exception.Message
        try { $state.lastError = $message; Write-KmgUmmReceipt $state } catch { }
        throw "UMM Params.xml restoration failed (backup and receipt retained at $ReceiptPath): $message"
    }
}

# Independent cleanup step for a guarded wrapper's finally: never throws, so a
# Params.xml failure cannot prevent any other restoration step.
function Complete-KmgUmmParamsProtection {
    param([AllowNull()]$Receipt)
    if ($null -eq $Receipt) { return [ordered]@{ verified = $false; disposition = 'not-protected'; error = 'No UMM Params.xml receipt was recorded.' } }
    try {
        $result = Restore-KmgUmmParams -ReceiptPath $Receipt.receiptPath
        return [ordered]@{ verified = $true; disposition = $result.disposition; receiptPath = $result.receiptPath
            intakeSha256 = $result.intakeSha256; restoredSha256 = $result.restoredSha256 }
    } catch {
        return [ordered]@{ verified = $false; disposition = 'FAILED'; error = $_.Exception.Message
            receiptPath = $Receipt.receiptPath; backupPath = $Receipt.backupPath; intakeSha256 = $Receipt.intakeSha256 }
    }
}
