[CmdletBinding()]
param(
    [int]$ProcessId,
    [ValidateRange(1, 86400)][int]$TimeoutSeconds = 14400,
    [ValidateRange(1, 60)][int]$PollSeconds = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)

if ($ProcessId) {
    $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if (-not $process -or $process.ProcessName -ne 'Kingmaker') {
        Write-Output 'could-not-be-identified'
        exit 2
    }
} else {
    $matches = @(Get-Process -Name 'Kingmaker' -ErrorAction SilentlyContinue)
    if ($matches.Count -ne 1) {
        Write-Output 'could-not-be-identified'
        exit 2
    }
    $process = $matches[0]
}

while (-not $process.HasExited) {
    if ([DateTime]::UtcNow -ge $deadline) {
        Write-Output "timed-out pid=$($process.Id)"
        exit 3
    }
    Start-Sleep -Seconds $PollSeconds
    $process.Refresh()
}
Write-Output "exited-normally pid=$($process.Id) exitTime=$([DateTime]::UtcNow.ToString('o'))"
