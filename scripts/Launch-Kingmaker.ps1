[CmdletBinding()]
param(
    [string]$SteamPath = 'C:\Program Files (x86)\Steam\steam.exe',
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$RecordPath,
    [int]$SteamAppId = 640820,
    [string]$RuntimeRequestPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

if (-not (Test-Path -LiteralPath $SteamPath -PathType Leaf)) { throw "Steam executable is missing: $SteamPath" }
if (-not (Test-Path -LiteralPath $GameDirectory -PathType Container)) { throw "Game directory is missing: $GameDirectory" }
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
