[CmdletBinding()]
param(
    [string]$SteamPath = 'C:\Program Files (x86)\Steam\steam.exe',
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$RecordPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

if (-not (Test-Path -LiteralPath $SteamPath -PathType Leaf)) { throw "Steam executable is missing: $SteamPath" }
if (-not (Test-Path -LiteralPath $GameDirectory -PathType Container)) { throw "Game directory is missing: $GameDirectory" }
Assert-KmgNotRunning
$started = [DateTime]::UtcNow
$process = Start-Process -FilePath $SteamPath -ArgumentList '-applaunch', '640820' -PassThru
$record = [ordered]@{
    launchedAtUtc = $started.ToString('o')
    steamProcessId = $process.Id
    mechanism = 'Steam -applaunch 640820'
    gameDirectory = (Resolve-Path -LiteralPath $GameDirectory).Path
}
if ($RecordPath) {
    $parent = Split-Path -Parent $RecordPath
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw "Record directory is missing: $parent" }
    $record | ConvertTo-Json | Set-Content -LiteralPath $RecordPath -Encoding UTF8
}
$record | ConvertTo-Json
