<#
.SYNOPSIS
Recovers UMM Params.xml from a pending guarded-launch receipt after an
interrupted run. Refuses while Kingmaker is running; the receipt stays pending
and the backup is retained on any failure.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$ReceiptPath,
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\umm-params'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'UmmParams.Common.ps1')

Assert-KmgNotRunning
$receipts = @(if ($ReceiptPath) { $ReceiptPath } else { Get-KmgPendingUmmParamsReceipts $BackupRoot | ForEach-Object { $_.receiptPath } })
if ($receipts.Count -eq 0) { Write-Host 'No pending UMM Params.xml receipt.'; return }
foreach ($receipt in $receipts) {
    if ($PSCmdlet.ShouldProcess($receipt, 'restore UMM Params.xml intake bytes')) {
        $result = Restore-KmgUmmParams -ReceiptPath $receipt
        Write-Host "UMM Params.xml $($result.disposition): sha256=$($result.restoredSha256) receipt=$receipt"
    }
}
