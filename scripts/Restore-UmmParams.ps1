<#
.SYNOPSIS
Recovers UMM Params.xml from a pending guarded-launch receipt after an
interrupted run. Serialized through the shared coordination lock: refuses
while the receipt's original owner is still alive, while any other
transaction holds the lock (never stealing or clearing it), or while
Kingmaker is running. The receipt stays pending and the backup is retained on
any refusal or failure.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$ReceiptPath,
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\umm-params',
    [string]$StateRoot = 'C:/Dev/KingmakerGunslingerLab/compatibility-state'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UmmParams.Common.ps1')

$receipts = @(if ($ReceiptPath) { $ReceiptPath } else { Get-KmgPendingUmmParamsReceipts $BackupRoot | ForEach-Object { $_.receiptPath } })
if ($receipts.Count -eq 0) { Write-Host 'No pending UMM Params.xml receipt.'; return }
foreach ($receipt in $receipts) {
    if ($PSCmdlet.ShouldProcess($receipt, 'recover UMM Params.xml intake bytes under the coordination lock')) {
        $result = Invoke-KmgUmmParamsRecovery -ReceiptPath $receipt -StateRoot $StateRoot
        Write-Host "UMM Params.xml $($result.disposition): sha256=$($result.restoredSha256) receipt=$receipt"
    }
}
