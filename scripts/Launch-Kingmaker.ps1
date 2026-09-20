[CmdletBinding()]
param(
    [string]$SteamPath = 'C:\Program Files (x86)\Steam\steam.exe',
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$RecordPath,
    [int]$SteamAppId = 640820,
    [string]$RuntimeRequestPath,
    $RuntimeLease
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

if (-not (Test-Path -LiteralPath $SteamPath -PathType Leaf)) { throw "Steam executable is missing: $SteamPath" }
if (-not (Test-Path -LiteralPath $GameDirectory -PathType Container)) { throw "Game directory is missing: $GameDirectory" }
$launchScope = Enter-KmgRuntimeLease -ParentLease $RuntimeLease -Purpose 'standalone Steam launch'
$launch = $null
try {
Assert-KmgNotRunning
$launch = Start-KmgSteamKingmaker -SteamPath $SteamPath -AppId $SteamAppId `
    -RequestPath $RuntimeRequestPath
$record = [ordered]@{
    launchedAtUtc = $launch.kingmakerStartedAtUtc.ToString('o')
    steamProcessId = $launch.steamProcessId
    kingmakerProcessId = $launch.kingmakerProcessId
    mechanism = 'Steam -applaunch 640820'
    gameDirectory = (Resolve-Path -LiteralPath $GameDirectory).Path
}
if ($RecordPath) {
    $parent = Split-Path -Parent $RecordPath
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw "Record directory is missing: $parent" }
    $record | ConvertTo-Json | Set-Content -LiteralPath $RecordPath -Encoding UTF8
}
$record | ConvertTo-Json

} finally {
    if ($launchScope.Acquired -and $null -ne $launch) {
        # This entry point only starts the game: it owns no settings override or
        # deployment to restore. Once the exact process exists, ordinary game-
        # running guards protect it. Guarded qualification callers retain their
        # inherited parent lease through their own exit/restoration boundary.
        Assert-KmgRuntimeLease $launchScope.Lease
        $launchScope.Lease.State.status = 'Completed'
        $launchScope.Lease.State.reason = 'Steam launch handed to the identified game process; no live files changed.'
        Write-KmgRuntimeLeaseState $launchScope.Lease
        $launchScope.Lease.Stream.Dispose()
        Remove-KmgCompatibilityOwnedLock $launchScope.Lease.LockPath $launchScope.Lease.RunId
    } else { Exit-KmgRuntimeLease $launchScope }
}
