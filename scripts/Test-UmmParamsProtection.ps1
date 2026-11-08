[CmdletBinding()]
param()

# Behavior tests for the guarded launch's byte-exact UMM Params.xml
# protection, using temporary files and the shipped shared functions. No game.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UmmParams.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$passes = 0
function Assert-True([bool]$Condition, [string]$Name) {
    if ($Condition) { $script:passes++ } else { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { $script:passes++ }
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

function New-Case([string]$Name) {
    $case = Join-Path $root $Name
    $params = Join-Path $case 'game\Kingmaker_Data\Managed\UnityModManager\Params.xml'
    New-Item -ItemType Directory -Path (Split-Path $params -Parent) -Force | Out-Null
    [IO.File]::WriteAllBytes($params, $intakeBytes)
    return [pscustomobject]@{ Params = $params; Backups = (Join-Path $case 'backups') }
}
function Set-Text([string]$Path, [string]$Old, [string]$New) {
    $text = [IO.File]::ReadAllText($Path)
    [IO.File]::WriteAllBytes($Path, [Text.UTF8Encoding]::new($true).GetBytes($text.Replace($Old, $New)))
}

try {
    New-Item -ItemType Directory -Path $root | Out-Null

    # Path resolution from the live mod directory.
    Assert-True ((Get-KmgUmmParamsPath 'C:\Games\PK\Mods\KingmakerGunslinger') -ceq
        'C:\Games\PK\Kingmaker_Data\Managed\UnityModManager\Params.xml') 'path-resolves-from-live-mod-directory'

    # 1. Changed LastUpdateCheck timestamp: exact bytes restored.
    $c = New-Case 'timestamp'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups -Owner 'test'
    Assert-True ($r.existed -and $r.intakeSha256 -ceq $intakeSha -and $r.state -ceq 'pending') 'backup-records-intake'
    Assert-True ((Get-KmgUmmFileSha256 $r.backupPath) -ceq $intakeSha) 'backup-bytes-exact'
    Assert-True ((Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'receipt-durable-pending'
    Set-Text $c.Params '2026-11-07T18:00:00.0000000-05:00' '2026-11-08T00:14:01.6825169-05:00'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and $result.disposition -ceq 'restored') 'timestamp-change-restored'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'timestamp-change-hash-matches-intake'
    Assert-True ((Read-KmgUmmReceipt $r.receiptPath).state -ceq 'restored') 'receipt-marked-restored'
    Assert-True ((Restore-KmgUmmParams -ReceiptPath $r.receiptPath).disposition -ceq 'already-restored') 'restore-idempotent'

    # 2. Changed enabled-mod profile content: exact bytes restored.
    $c = New-Case 'profile'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'profile-change-restored'

    # 3. Unchanged file: verified without rewriting.
    $c = New-Case 'unchanged'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and $result.disposition -ceq 'unchanged') 'unchanged-verified'

    # 4. Scenario failure: the finally-step still restores.
    $c = New-Case 'scenario-failure'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    $scenarioFailed = $false; $result = $null
    try { Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'; throw 'scenario failed' }
    catch { $scenarioFailed = $true }
    finally { $result = Complete-KmgUmmParamsProtection -Receipt $r }
    Assert-True ($scenarioFailed -and $result.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha) 'scenario-failure-restored'

    # 5. Missing backup: refused, nothing written, receipt and evidence retained,
    #    and a new launch is refused while the receipt is pending.
    $c = New-Case 'missing-backup'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    Remove-Item -LiteralPath $r.backupPath -Force
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True (-not $result.verified -and $result.disposition -ceq 'FAILED' -and $result.error -match 'backup is missing') 'missing-backup-fails'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'missing-backup-writes-nothing'
    $kept = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($kept.state -ceq 'pending' -and $kept.attempts -eq 1 -and $kept.lastError) 'missing-backup-receipt-retained'
    Assert-Throws { Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups } 'pending-receipt-refuses-launch'

    # 6. Corrupt backup: refused, nothing written.
    $c = New-Case 'corrupt-backup'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    $changedSha = Get-KmgUmmFileSha256 $c.Params
    Set-Text $r.backupPath 'KingmakerGunslinger' 'KingmakerGunslingeR'
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True (-not $result.verified -and $result.error -match 'does not match its recorded intake hash') 'corrupt-backup-fails'
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $changedSha) 'corrupt-backup-writes-nothing'

    # 7. Failed restore (destination locked), then a successful retry.
    $c = New-Case 'retry'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Set-Text $c.Params '2026-11-07T18:00:00.0000000-05:00' '2026-11-08T00:14:01.6825169-05:00'
    $lock = [IO.File]::Open($c.Params, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
    try { $first = Complete-KmgUmmParamsProtection -Receipt $r } finally { $lock.Dispose() }
    Assert-True (-not $first.verified -and (Read-KmgUmmReceipt $r.receiptPath).state -ceq 'pending') 'locked-restore-fails-pending'
    Assert-True ((Get-KmgUmmFileSha256 $r.backupPath) -ceq $intakeSha) 'locked-restore-backup-retained'
    $retry = Complete-KmgUmmParamsProtection -Receipt $r
    $after = Read-KmgUmmReceipt $r.receiptPath
    Assert-True ($retry.verified -and (Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha -and $after.state -ceq 'restored' -and
        $after.attempts -eq 2 -and $null -eq $after.lastError) 'retry-restores'

    # 8. Independent cleanup: a Params.xml failure never throws, so the next
    #    restoration step still runs.
    $c = New-Case 'independent'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Remove-Item -LiteralPath $r.backupPath -Force
    $otherRan = $false
    $ummStep = Complete-KmgUmmParamsProtection -Receipt $r
    $otherRan = $true
    Assert-True (-not $ummStep.verified -and $otherRan) 'independent-cleanup-continues'
    Assert-True ((Complete-KmgUmmParamsProtection -Receipt $null).disposition -ceq 'not-protected') 'missing-receipt-not-pass'

    # 9. A file absent at intake is removed again after the run.
    $c = New-Case 'absent'
    Remove-Item -LiteralPath $c.Params -Force
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    [IO.File]::WriteAllBytes($c.Params, $intakeBytes)
    $result = Complete-KmgUmmParamsProtection -Receipt $r
    Assert-True ($result.verified -and -not $r.existed -and -not (Test-Path -LiteralPath $c.Params)) 'absent-intake-restored-absent'

    # 10. Backup protection that cannot be recorded refuses the launch.
    $c = New-Case 'unwritable'
    [IO.File]::WriteAllText($c.Backups, 'not a directory')
    Assert-Throws { Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups } 'unrecordable-backup-refuses-launch'

    # 11. Interruption recovery through the shipped recovery script.
    $c = New-Case 'recovery'
    $r = Backup-KmgUmmParams -ParamsPath $c.Params -BackupRoot $c.Backups
    Set-Text $c.Params 'Enabled="true"' 'Enabled="false"'
    & (Join-Path $PSScriptRoot 'Restore-UmmParams.ps1') -BackupRoot $c.Backups -Confirm:$false | Out-Null
    Assert-True ((Get-KmgUmmFileSha256 $c.Params) -ceq $intakeSha -and @(Get-KmgPendingUmmParamsReceipts $c.Backups).Count -eq 0) 'recovery-script-restores-pending'
}
finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}

if ($failures.Count -gt 0) {
    Write-Host ("UMM Params.xml protection FAIL ({0}): {1}" -f $failures.Count, ($failures -join ', '))
    exit 1
}
Write-Host "UMM Params.xml protection tests PASS=$passes"
